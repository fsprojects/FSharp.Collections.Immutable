/// <summary>
/// Checks the order of the functions in the signature files of the library and in their implementation files
/// (ADR 0001, D4).
/// </summary>
/// <remarks>
/// <para>
/// Every module of a signature file groups its <c>val</c>s under banner comments <c>// ---- Category ----</c>. The
/// banners follow the canonical category order, and the <c>val</c>s of a category are sorted alphabetically (ordinal),
/// active patterns last. The sibling implementation file has the same banners and defines the functions of the
/// signature in the same order.
/// </para>
/// <para>
/// The files are read through the parser of Fantomas (<c>Fantomas.FCS</c>): the modules and their values come from the
/// syntax tree, the banners from the comment trivia. The parser only sees the code of one set of defines at a time, so
/// every file is parsed once per target framework of the library and the views are checked one by one.
/// </para>
/// </remarks>
module CheckOrder

open System
open System.Collections.Generic
open System.IO
open System.Text.RegularExpressions
open Fantomas.FCS
open Fantomas.FCS.Syntax
open Fantomas.FCS.SyntaxTrivia
open Fantomas.FCS.Text

/// A violation of the order rules: the file, the line and the message.
[<Struct>]
type Violation = { Path : string; Line : int; Message : string }

/// The canonical category sequence of ADR 0001, D4.
let private categories = [|
    "Creation"
    "Builder"
    "Properties"
    "Access"
    "Search"
    "Transformation"
    "Folding"
    "Partitioning and grouping"
    "Sorting"
    "Random"
    "Conversion"
    "Type-specific"
|]

/// Maps and sets only partition, so their banner may say "Partitioning"; it takes the place of "Partitioning and grouping".
let private categoryIndex (category : string) =
    let canonical =
        if String.Equals (category, "Partitioning", StringComparison.Ordinal) then
            "Partitioning and grouping"
        else
            category

    Array.IndexOf (categories, canonical)

// The major versions of the target frameworks of the library (`TargetFrameworks` in Directory.Build.props). A `#if`
// hides one of its arms from the parser, so every file is parsed with the defines of each framework in turn.
let private targetFrameworks = [| 8; 10 |]

/// The preprocessor symbols that the SDK defines for `net<major>.0`.
let private definesOf (major : int) = [
    "NET"
    "NETCOREAPP"
    $"NET%d{major}_0"
    for version in 5..major do
        $"NET%d{version}_0_OR_GREATER"
]

let private bannerRegex =
    Regex (@"^\s*// ---- (?<category>.+?) ----\s*$", RegexOptions.CultureInvariant)

[<Struct>]
type private Banner = { Line : int; Category : string }

[<Struct>]
type private Member = { Line : int; Category : string; Name : string }

/// A public module of a source file while it is collected: its range and its public values, before the banners are known.
type private Scope = {
    Path : string
    Range : range
    Values : ResizeArray<struct (int * string)>
    Banners : ResizeArray<Banner>
}

/// A public module of a source file: its banners and its public members in source order.
type private Module = { Path : string; Banners : Banner array; Members : Member array }

let private isPublic (access : SynAccess option) =
    match access with
    | None
    | Some (SynAccess.Public _) -> true
    | Some _ -> false

/// The name of a value as the source writes it: the identifier of an active pattern is `|Cons|_|`, the source says `(|Cons|_|)`.
let private displayName (ident : Ident) =
    if ident.idText.StartsWith ("|", StringComparison.Ordinal) then
        $"(%s{ident.idText})"
    else
        ident.idText

let private pathOf (longId : LongIdent) = String.Join (".", longId |> List.map _.idText)

let private childPath (parent : string) (longId : LongIdent) =
    if parent.Length = 0 then
        pathOf longId
    else
        $"%s{parent}.%s{pathOf longId}"

let private newScope (scopes : ResizeArray<Scope>) (path : string) (range : range) =
    let scope = {
        Path = path
        Range = range
        Values = ResizeArray ()
        Banners = ResizeArray ()
    }

    scopes.Add scope
    scope

let private addValue (scope : Scope voption) (ident : Ident) =
    match scope with
    | ValueSome scope -> scope.Values.Add (struct (ident.idRange.StartLine, displayName ident))
    | ValueNone -> ()

/// The identifier that a top-level binding defines, when the binding is a public function or value.
let rec private boundIdent (pattern : SynPat) =
    match pattern with
    | SynPat.Typed (pat = pattern) -> boundIdent pattern
    | SynPat.Named (ident = SynIdent (ident, _); accessibility = access) when isPublic access -> ValueSome ident
    | SynPat.LongIdent (longDotId = SynLongIdent ([ ident ], _, _); accessibility = access) when isPublic access ->
        ValueSome ident
    | _ -> ValueNone

let rec private walkSignatureDecls (scopes : ResizeArray<Scope>) (parent : string) (scope : Scope voption) decls =
    for decl in decls do
        match decl with
        | SynModuleSigDecl.Val (SynValSig (ident = SynIdent (ident, _); accessibility = SynValSigAccess.Single access), _) when
            isPublic access
            ->
            addValue scope ident
        | SynModuleSigDecl.NestedModule (
            moduleInfo = (SynComponentInfo (accessibility = access) as info); moduleDecls = decls; range = range) when
            isPublic access
            ->
            let path = childPath parent info.LongIdent
            walkSignatureDecls scopes path (ValueSome (newScope scopes path range)) decls
        | SynModuleSigDecl.NamespaceFragment fragment -> walkSignatureModule scopes fragment
        | _ -> ()

and private walkSignatureModule (scopes : ResizeArray<Scope>) fragment =
    match fragment with
    | SynModuleOrNamespaceSig (longId = longId; kind = kind; decls = decls; accessibility = access; range = range) ->
        if kind.IsModule then
            if isPublic access then
                let path = pathOf longId
                walkSignatureDecls scopes path (ValueSome (newScope scopes path range)) decls
        else
            // A namespace is not a scope: its values do not exist, its modules are the scopes
            let path = if kind.IsGlobalNamespace then "" else pathOf longId
            walkSignatureDecls scopes path ValueNone decls

let rec private walkImplementationDecls (scopes : ResizeArray<Scope>) (parent : string) (scope : Scope voption) decls =
    for decl in decls do
        match decl with
        | SynModuleDecl.Let (bindings = bindings) ->
            for SynBinding (headPat = pattern) in bindings do
                match boundIdent pattern with
                | ValueSome ident -> addValue scope ident
                | ValueNone -> ()
        | SynModuleDecl.NestedModule (
            moduleInfo = (SynComponentInfo (accessibility = access) as info); decls = decls; range = range) when isPublic access ->
            let path = childPath parent info.LongIdent
            walkImplementationDecls scopes path (ValueSome (newScope scopes path range)) decls
        | SynModuleDecl.NamespaceFragment fragment -> walkImplementationModule scopes fragment
        | _ -> ()

and private walkImplementationModule (scopes : ResizeArray<Scope>) fragment =
    match fragment with
    | SynModuleOrNamespace (longId = longId; kind = kind; decls = decls; accessibility = access; range = range) ->
        if kind.IsModule then
            if isPublic access then
                let path = pathOf longId
                walkImplementationDecls scopes path (ValueSome (newScope scopes path range)) decls
        else
            let path = if kind.IsGlobalNamespace then "" else pathOf longId
            walkImplementationDecls scopes path ValueNone decls

/// Banners are comments, so they come from the trivia; each one belongs to the innermost module that spans its line.
let private collectBanners (lines : string array) (trivia : ParsedInputTrivia) (scopes : ResizeArray<Scope>) =
    for comment in trivia.CodeComments do
        match comment with
        | CommentTrivia.LineComment range ->
            let line = range.StartLine
            let bannerMatch = bannerRegex.Match lines[line - 1]

            if bannerMatch.Success then
                let owner =
                    scopes
                    |> Seq.filter (fun scope -> scope.Range.StartLine <= line && line <= scope.Range.EndLine)
                    |> Seq.sortByDescending _.Range.StartLine
                    |> Seq.tryHead

                match owner with
                | Some owner -> owner.Banners.Add { Line = line; Category = bannerMatch.Groups["category"].Value }
                | None -> ()
        | CommentTrivia.BlockComment _ -> ()

/// Orders the banners and the values of a scope by line and gives every value the category of the banner above it.
let private resolve (scope : Scope) =
    let banners = scope.Banners |> Seq.sortBy _.Line |> Seq.toArray

    let members =
        scope.Values
        |> Seq.sortBy (fun struct (line, _) -> line)
        |> Seq.map (fun struct (line, name) ->
            let category =
                banners
                |> Array.tryFindBack (fun banner -> banner.Line < line)
                |> Option.map _.Category
                |> Option.defaultValue ""

            { Line = line; Category = category; Name = name }
        )
        |> Seq.toArray

    { Path = scope.Path; Banners = banners; Members = members }

/// Parses a file with the given defines and returns its public modules; a parse error is a violation.
let private parseModules (errors : ResizeArray<Violation>) (path : string) (defines : string list) =
    let lines = File.ReadAllLines path
    let source = SourceText.ofString (String.Join (Environment.NewLine, lines))
    let isSignature = path.EndsWith (".fsi", StringComparison.OrdinalIgnoreCase)
    let tree, diagnostics = Parse.parseFile isSignature source defines

    for diagnostic in diagnostics do
        if diagnostic.Severity.IsError then
            let line =
                diagnostic.Range
                |> Option.map _.StartLine
                |> Option.defaultValue 1

            errors.Add { Path = path; Line = line; Message = $"Parse error: %s{diagnostic.Message}" }

    let scopes = ResizeArray<Scope>()

    match tree with
    | ParsedInput.SigFile (ParsedSigFileInput (contents = contents; trivia = trivia)) ->
        for fragment in contents do
            walkSignatureModule scopes fragment

        collectBanners lines trivia scopes
    | ParsedInput.ImplFile (ParsedImplFileInput (contents = contents; trivia = trivia)) ->
        for fragment in contents do
            walkImplementationModule scopes fragment

        collectBanners lines trivia scopes

    scopes |> Seq.map resolve |> Seq.toArray

let private isActivePattern (name : string) = name.StartsWith ("(|", StringComparison.Ordinal)

/// Identifiers sort ordinally; active patterns come after them, also ordinally.
let private compareNames (left : string) (right : string) =
    match isActivePattern left, isActivePattern right with
    | false, true -> -1
    | true, false -> 1
    | _ -> String.CompareOrdinal (left, right)

let private report (errors : ResizeArray<Violation>) path line message =
    errors.Add { Path = path; Line = line; Message = message }

let private checkSignature (errors : ResizeArray<Violation>) (path : string) (scope : Module) =
    let report = report errors path
    let mutable previousIndex = -1

    for banner in scope.Banners do
        let index = categoryIndex banner.Category

        if index < 0 then
            report banner.Line $"Unknown category '%s{banner.Category}' in module %s{scope.Path}"
        elif index <= previousIndex then
            report banner.Line $"Category '%s{banner.Category}' is out of the canonical order in module %s{scope.Path}"
        else
            previousIndex <- index

        let hasMembers =
            scope.Members
            |> Array.exists (fun item -> String.Equals (item.Category, banner.Category, StringComparison.Ordinal))

        if not hasMembers then
            report banner.Line $"Category '%s{banner.Category}' is empty in module %s{scope.Path}; omit its banner"

    for index in 0 .. scope.Members.Length - 1 do
        let item = scope.Members[index]

        if item.Category.Length = 0 then
            report item.Line $"`%s{item.Name}` comes before the first category banner of module %s{scope.Path}"
        elif index > 0 then
            let previous = scope.Members[index - 1]

            if String.Equals (previous.Category, item.Category, StringComparison.Ordinal) then
                let order = compareNames previous.Name item.Name

                if order = 0 then
                    report item.Line $"`%s{item.Name}` is declared twice in module %s{scope.Path}"
                elif order > 0 then
                    report item.Line $"`%s{item.Name}` must come before `%s{previous.Name}` in category '%s{item.Category}'"

let private checkImplementation
    (errors : ResizeArray<Violation>)
    (implementationPath : string)
    (signatureModules : Module array)
    (implementationModules : Module array)
    =
    let report = report errors implementationPath

    for signature in signatureModules do
        let implementation =
            implementationModules
            |> Array.tryFind (fun scope -> String.Equals (scope.Path, signature.Path, StringComparison.Ordinal))

        match implementation with
        | None -> report 1 $"Module %s{signature.Path} of the signature file is missing"
        | Some implementation ->
            let signatureBanners = signature.Banners |> Array.map _.Category
            let implementationBanners = implementation.Banners |> Array.map _.Category

            if signatureBanners <> implementationBanners then
                let line =
                    if implementation.Banners.Length > 0 then
                        implementation.Banners[0].Line
                    else
                        1

                let expected = String.Join (", ", signatureBanners)
                let actual = String.Join (", ", implementationBanners)
                report line $"Module %s{signature.Path} has the banners [%s{actual}]; the signature has [%s{expected}]"

            // The signature decides what is public: a binding that the signature omits is a private helper, whatever
            // its access modifier says, so it takes no place in the order.
            let declared = HashSet<string>(signature.Members |> Seq.map _.Name, StringComparer.Ordinal)
            let members =
                implementation.Members
                |> Array.filter (fun item -> declared.Contains item.Name)
            let count = min signature.Members.Length members.Length

            // A member must have the same name and the same category, so that a moved banner cannot go unnoticed
            let differs index =
                let expected = signature.Members[index]
                let actual = members[index]

                not (String.Equals (expected.Name, actual.Name, StringComparison.Ordinal))
                || not (String.Equals (expected.Category, actual.Category, StringComparison.Ordinal))

            match seq { 0 .. count - 1 } |> Seq.tryFind differs with
            | Some index ->
                let expected = signature.Members[index]
                let actual = members[index]

                if String.Equals (expected.Name, actual.Name, StringComparison.Ordinal) then
                    report
                        actual.Line
                        $"`%s{actual.Name}` is in category '%s{actual.Category}'; the signature puts it in '%s{expected.Category}' (line %d{expected.Line})"
                else
                    report
                        actual.Line
                        $"`%s{actual.Name}` is defined where the signature declares `%s{expected.Name}` (line %d{expected.Line})"
            | None when signature.Members.Length <> members.Length ->
                report
                    1
                    $"Module %s{signature.Path} defines %d{members.Length} of the %d{signature.Members.Length} functions that the signature declares"
            | None -> ()

let private isGenerated (path : string) =
    let separator = Path.DirectorySeparatorChar

    path.Contains ($"%c{separator}obj%c{separator}", StringComparison.Ordinal)
    || path.Contains ($"%c{separator}bin%c{separator}", StringComparison.Ordinal)

/// Checks one signature file and its sibling implementation file; the files are independent, so the pairs run in parallel.
let private checkPair (signaturePath : string) =
    let errors = ResizeArray<Violation>()
    let implementationPath = Path.ChangeExtension (signaturePath, ".fs") |> nonNull
    let hasImplementation = File.Exists implementationPath

    if not hasImplementation then
        report errors signaturePath 1 "The signature file has no implementation file next to it"

    for major in targetFrameworks do
        let defines = definesOf major
        let signatureModules = parseModules errors signaturePath defines

        for scope in signatureModules do
            checkSignature errors signaturePath scope

        if hasImplementation then
            let implementationModules = parseModules errors implementationPath defines
            checkImplementation errors implementationPath signatureModules implementationModules

    errors.ToArray ()

/// <summary>Checks every signature file under a directory and its sibling implementation file.</summary>
/// <param name="sourceDirectory">The directory of the sources; generated files under <c>bin</c> and <c>obj</c> are skipped.</param>
/// <returns>The number of signature files checked and the violations, sorted by path and line.</returns>
let check (sourceDirectory : string) =
    let signatureFiles =
        Directory.EnumerateFiles (sourceDirectory, "*.fsi", SearchOption.AllDirectories)
        |> Seq.filter (isGenerated >> not)
        |> Seq.sort
        |> Seq.toArray

    // The views of the target frameworks mostly agree, so the same violation is usually found once per framework
    let violations =
        signatureFiles
        |> Array.Parallel.collect checkPair
        |> Array.distinct
        |> Array.sortBy (fun violation -> struct (violation.Path, violation.Line))

    struct (signatureFiles.Length, violations)
