// Checks the order of the functions in the signature files of the library (ADR 0001, D4).
//
//     dotnet fsi build/check-order.fsx [source directory]
//
// Every module of a signature file groups its `val`s under banner comments `// ---- Category ----`. The banners follow
// the canonical category order, and the `val`s of a category are sorted alphabetically (ordinal), active patterns last.
// The sibling implementation file has the same banners and defines its public functions in the same order.
// A violation is printed as `path(line): message`, and the script exits with code 1.
//
// The script needs no packages, so that the FAKE target `CheckOrder` and a contributor can run it before a build.

open System
open System.IO
open System.Text.RegularExpressions

/// The canonical category sequence of ADR 0001, D4.
let categories = [|
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
let categoryIndex (category : string) =
    let canonical =
        if String.Equals (category, "Partitioning", StringComparison.Ordinal) then
            "Partitioning and grouping"
        else
            category

    Array.IndexOf (categories, canonical)

let bannerRegex = Regex (@"^\s*// ---- (?<category>.+?) ----\s*$", RegexOptions.CultureInvariant)

let moduleRegex =
    Regex (
        @"^(?<indent> *)module\s+(?<access>(?:private|internal)\s+)?(?<name>[A-Za-z_][A-Za-z0-9_]*)\s*=",
        RegexOptions.CultureInvariant
    )

// An identifier or an active pattern such as `(|Cons|_|)`, optionally preceded by attributes on the same line
let private nameGroup = @"(?<name>\(\|[^)]*\|\)|[A-Za-z_][A-Za-z0-9_']*)"

let valRegex =
    Regex (
        @"^(?<indent> *)(?:\[<.*>\]\s*)?val\s+(?:inline\s+)?"
        + nameGroup,
        RegexOptions.CultureInvariant
    )

let letRegex =
    Regex (
        @"^(?<indent> *)(?:\[<.*>\]\s*)?let\s+(?:rec\s+)?(?:inline\s+)?(?<access>(?:private|internal)\s+)?(?:inline\s+)?"
        + nameGroup,
        RegexOptions.CultureInvariant
    )

[<Struct>]
type Banner = { Line : int; Category : string }

[<Struct>]
type Member = { Line : int; Category : string; Name : string }

/// A module of a source file: its banners and its public members in source order.
type Scope = {
    Path : string
    Indent : int
    IsPublic : bool
    Banners : ResizeArray<Banner>
    Members : ResizeArray<Member>
}

let indentOf (line : string) = line.Length - line.AsSpan().TrimStart(' ').Length

/// Reads the modules of a file. `memberRegex` recognises a member declaration (`val` or `let`);
/// members at the content level of a public module count, nested bindings and non-public members do not.
let parse (path : string) (memberRegex : Regex) =
    let scopes = ResizeArray<Scope>()
    let stack = Collections.Generic.Stack<Scope>()
    let lines = File.ReadAllLines path

    for index in 0 .. lines.Length - 1 do
        let line = lines[index]
        let lineNumber = index + 1
        let trimmed = line.AsSpan().Trim()

        // Preprocessor directives start at column 0 and must not close the enclosing modules
        if
            not trimmed.IsEmpty
            && not (trimmed.StartsWith ("#", StringComparison.Ordinal))
        then
            let indent = indentOf line

            while stack.Count > 0 && indent <= stack.Peek().Indent do
                stack.Pop () |> ignore

            let moduleMatch = moduleRegex.Match line

            if moduleMatch.Success then
                let name = moduleMatch.Groups["name"].Value
                let parent =
                    if stack.Count > 0 then
                        ValueSome (stack.Peek ())
                    else
                        ValueNone

                let scope = {
                    Path =
                        match parent with
                        | ValueSome parent -> $"%s{parent.Path}.%s{name}"
                        | ValueNone -> name
                    Indent = indent
                    IsPublic =
                        not moduleMatch.Groups["access"].Success
                        && (parent |> ValueOption.forall _.IsPublic)
                    Banners = ResizeArray ()
                    Members = ResizeArray ()
                }

                scopes.Add scope
                stack.Push scope
            elif stack.Count > 0 then
                let scope = stack.Peek ()
                let bannerMatch = bannerRegex.Match line

                if bannerMatch.Success then
                    scope.Banners.Add { Line = lineNumber; Category = bannerMatch.Groups["category"].Value }
                else
                    let memberMatch = memberRegex.Match line

                    let isContentLevel =
                        memberMatch.Success
                        && memberMatch.Groups["indent"].Length = scope.Indent + 4
                    let isPublic = isContentLevel && not memberMatch.Groups["access"].Success

                    if scope.IsPublic && isPublic then
                        let category =
                            if scope.Banners.Count = 0 then
                                ""
                            else
                                scope.Banners[scope.Banners.Count - 1].Category

                        scope.Members.Add {
                            Line = lineNumber
                            Category = category
                            Name = memberMatch.Groups["name"].Value
                        }

    scopes |> Seq.filter _.IsPublic |> Seq.toArray

let isActivePattern (name : string) = name.StartsWith ("(|", StringComparison.Ordinal)

/// Identifiers sort ordinally; active patterns come after them, also ordinally.
let compareNames (left : string) (right : string) =
    match isActivePattern left, isActivePattern right with
    | false, true -> -1
    | true, false -> 1
    | _ -> String.CompareOrdinal (left, right)

let errors = ResizeArray<struct (string * int * string)>()

let report path line message = errors.Add (struct (path, line, message))

let checkSignature (path : string) (scope : Scope) =
    let mutable previousIndex = -1

    for banner in scope.Banners do
        let index = categoryIndex banner.Category

        if index < 0 then
            report path banner.Line $"Unknown category '%s{banner.Category}' in module %s{scope.Path}"
        elif index <= previousIndex then
            report path banner.Line $"Category '%s{banner.Category}' is out of the canonical order in module %s{scope.Path}"
        else
            previousIndex <- index

        let hasMembers =
            scope.Members
            |> Seq.exists (fun item -> String.Equals (item.Category, banner.Category, StringComparison.Ordinal))

        if not hasMembers then
            report path banner.Line $"Category '%s{banner.Category}' is empty in module %s{scope.Path}; omit its banner"

    for index in 0 .. scope.Members.Count - 1 do
        let item = scope.Members[index]

        if item.Category.Length = 0 then
            report path item.Line $"`%s{item.Name}` comes before the first category banner of module %s{scope.Path}"
        elif index > 0 then
            let previous = scope.Members[index - 1]

            if String.Equals (previous.Category, item.Category, StringComparison.Ordinal) then
                let order = compareNames previous.Name item.Name

                if order = 0 then
                    report path item.Line $"`%s{item.Name}` is declared twice in module %s{scope.Path}"
                elif order > 0 then
                    report path item.Line $"`%s{item.Name}` must come before `%s{previous.Name}` in category '%s{item.Category}'"

let checkImplementation (signaturePath : string) (signatureScopes : Scope array) =
    let implementationPath = Path.ChangeExtension (signaturePath, ".fs")

    if not (File.Exists implementationPath) then
        report signaturePath 1 "The signature file has no implementation file next to it"
    else
        let implementationScopes = parse implementationPath letRegex

        let findScope (scopes : Scope array) path =
            scopes
            |> Array.tryFind (fun scope -> String.Equals (scope.Path, path, StringComparison.Ordinal))
            |> Option.toValueOption

        for signature in signatureScopes do
            match findScope implementationScopes signature.Path with
            | ValueNone -> report implementationPath 1 $"Module %s{signature.Path} of the signature file is missing"
            | ValueSome implementation ->
                let signatureBanners = signature.Banners |> Seq.map _.Category |> Seq.toArray
                let implementationBanners = implementation.Banners |> Seq.map _.Category |> Seq.toArray

                if signatureBanners <> implementationBanners then
                    let line =
                        if implementation.Banners.Count > 0 then
                            implementation.Banners[0].Line
                        else
                            1

                    let expected = String.Join (", ", signatureBanners)
                    let actual = String.Join (", ", implementationBanners)
                    report
                        implementationPath
                        line
                        $"Module %s{signature.Path} has the banners [%s{actual}]; the signature has [%s{expected}]"

                let count = min signature.Members.Count implementation.Members.Count

                let firstDifference =
                    seq { 0 .. count - 1 }
                    |> Seq.tryFind (fun index ->
                        not (
                            String.Equals (
                                signature.Members[index].Name,
                                implementation.Members[index].Name,
                                StringComparison.Ordinal
                            )
                        )
                    )
                    |> Option.toValueOption

                match firstDifference with
                | ValueSome index ->
                    let expected = signature.Members[index]
                    let actual = implementation.Members[index]
                    report
                        implementationPath
                        actual.Line
                        $"`%s{actual.Name}` is defined where the signature declares `%s{expected.Name}` (line %d{expected.Line})"
                | ValueNone when signature.Members.Count <> implementation.Members.Count ->
                    report
                        implementationPath
                        1
                        $"Module %s{signature.Path} defines %d{implementation.Members.Count} public functions; the signature declares %d{signature.Members.Count}"
                | ValueNone -> ()

let sourceDirectory =
    match fsi.CommandLineArgs |> Array.skip 1 with
    | [| directory |] -> Path.GetFullPath directory
    | [||] -> Path.GetFullPath (Path.Combine (__SOURCE_DIRECTORY__, "..", "src"))
    | _ -> failwith "Usage: dotnet fsi build/check-order.fsx [source directory]"

let isGenerated (path : string) =
    let separator = Path.DirectorySeparatorChar
    path.Contains ($"%c{separator}obj%c{separator}", StringComparison.Ordinal)
    || path.Contains ($"%c{separator}bin%c{separator}", StringComparison.Ordinal)

let signatureFiles =
    Directory.EnumerateFiles (sourceDirectory, "*.fsi", SearchOption.AllDirectories)
    |> Seq.filter (isGenerated >> not)
    |> Seq.sort
    |> Seq.toArray

for signaturePath in signatureFiles do
    let scopes = parse signaturePath valRegex

    for scope in scopes do
        checkSignature signaturePath scope

    checkImplementation signaturePath scopes

if errors.Count = 0 then
    printfn $"check-order: %d{signatureFiles.Length} signature files are in order"
else
    for struct (path, line, message) in
        errors
        |> Seq.sortBy (fun struct (path, line, _) -> struct (path, line)) do
        eprintfn $"%s{path}(%d{line}): %s{message}"

    eprintfn $"check-order: %d{errors.Count} violations"
    exit 1
