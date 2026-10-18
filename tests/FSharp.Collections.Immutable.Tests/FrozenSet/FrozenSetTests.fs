namespace FSharp.Collections.Immutable.Tests

open System
open System.Collections.Frozen
open System.Collections.Generic
open System.Globalization
open System.Runtime.InteropServices
open FSharp.Collections.Immutable
open Microsoft.VisualStudio.TestTools.UnitTesting

/// Builds frozen sets in a scope that opens `System.Collections.Frozen` after this library, as a file does that opens it
/// last: the module `FrozenSet` and the class `FrozenSet` must still resolve side by side.
module private FrozenOpenedLast =

    open System.Collections.Frozen

    /// A frozen set from the module.
    let fromModule () = FrozenSet.ofArray [| 3; 1; 2 |]

    /// A frozen set from the extension method of the class, called as a static method.
    let fromClass () = FrozenSet.ToFrozenSet [| 3; 1; 2 |]

/// <summary>
/// A check of a frozen set of any element type, which a case of <c>FrozenSetSample</c> runs on the frozen set that it
/// builds.
/// </summary>
type IFrozenSetCheck =
    /// Checks a frozen set.
    abstract Check<'T> : set : FrozenSet<'T> -> unit

/// <summary>
/// The frozen sets of the tests of storage and order, as the cases of their rows.
/// </summary>
/// <remarks>
/// The BCL lays out a frozen set by its size, its element type and its comparer, so the cases cover a small set and large
/// sets of several element types and comparers. A large set prints no readable row, so a case names the set, and
/// <c>Run</c> builds it.
/// </remarks>
[<RequireQualifiedAccess>]
type FrozenSetSample =
    /// The elements of `Inputs.Several`, few enough for the BCL to scan them.
    | Several
    /// The integers from 1 to 100.
    | Integers
    /// The 64-bit integers from 1 to 100.
    | Int64s
    /// 100 words, enough for the BCL to hash them instead of scanning them.
    | Words
    /// The same 100 words, compared without case.
    | CaselessWords
    /// 100 struct pairs of a number and its text, compared with `HashIdentity.Structural`.
    | Pairs

    /// Builds the frozen set of the case and runs a check on it.
    member this.Run (check : IFrozenSetCheck) =
        let words () = [| for index in 1..100 -> $"Word%d{index}" |]

        match this with
        | FrozenSetSample.Several -> check.Check (FrozenSet.ofArray Inputs.Several)
        | FrozenSetSample.Integers -> check.Check (FrozenSet.ofArray [| 1..100 |])
        | FrozenSetSample.Int64s -> check.Check (FrozenSet.ofArray [| 1L .. 100L |])
        | FrozenSetSample.Words -> check.Check (FrozenSet.ofArray (words ()))
        | FrozenSetSample.CaselessWords ->
            check.Check (
                (FrozenSet.forComparer (StringComparer.OrdinalIgnoreCase :> IEqualityComparer<string>)).ofArray(words ())
            )
        | FrozenSetSample.Pairs ->
            check.Check (
                (FrozenSet.forComparer HashIdentity.Structural).ofArray [| for index in 1..100 -> struct (index, string index) |]
            )

/// <summary>
/// The functions that visit the elements of a frozen set, as the cases of rows.
/// </summary>
[<RequireQualifiedAccess>]
type FrozenSetVisit =
    | Fold
    | FoldBack
    | Iter
    | ToArray
    | ToFlatList
    | ToList
    | ToSeq

    /// The name of the function in the module, for the messages of the assertions.
    member this.Name =
        match this with
        | FrozenSetVisit.Fold -> nameof FrozenSet.fold
        | FrozenSetVisit.FoldBack -> nameof FrozenSet.foldBack
        | FrozenSetVisit.Iter -> nameof FrozenSet.iter
        | FrozenSetVisit.ToArray -> nameof FrozenSet.toArray
        | FrozenSetVisit.ToFlatList -> nameof FrozenSet.toFlatList
        | FrozenSetVisit.ToList -> nameof FrozenSet.toList
        | FrozenSetVisit.ToSeq -> nameof FrozenSet.toSeq

    /// The elements of a frozen set in the order in which the function visits them.
    member this.Apply (set : FrozenSet<'T>) : 'T array =
        match this with
        | FrozenSetVisit.Fold -> FrozenSet.fold (fun visited item -> Array.append visited [| item |]) [||] set
        // foldBack visits the elements from the last one, so it puts each element before the ones that it visited
        | FrozenSetVisit.FoldBack -> FrozenSet.foldBack (fun item visited -> Array.append [| item |] visited) set [||]
        | FrozenSetVisit.Iter ->
            let visited = ResizeArray<'T>()
            FrozenSet.iter visited.Add set
            visited.ToArray ()
        | FrozenSetVisit.ToArray -> FrozenSet.toArray set
        | FrozenSetVisit.ToFlatList -> Seq.toArray (FrozenSet.toFlatList set)
        // The function under test returns a list
        | FrozenSetVisit.ToList -> List.toArray (FrozenSet.toList set)
        | FrozenSetVisit.ToSeq -> Seq.toArray (FrozenSet.toSeq set)

/// <summary>
/// A relation of a frozen set with a sequence, as the table of its tests holds it: the strings that the test builds the
/// frozen set and the sequence from, the member of the class that takes the sequence, and the function of the module that
/// takes a frozen set.
/// </summary>
type private SequenceRelation = {
    /// The names of the strings of the frozen set, in the pool of the tests.
    SetItems : string array
    /// The names of the strings of the sequence, in the pool of the tests.
    SequenceItems : string array
    /// The member of the class, which takes the sequence.
    ClassMember : FrozenSet<string> -> seq<string> -> bool
    /// The function of the module, which takes a frozen set.
    ModuleFunction : FrozenSet<string> -> FrozenSet<string> -> bool
}

/// <summary>
/// The behaviour of <c>FrozenSet</c> that the shared contracts do not cover: comparers, the sequence forms of the
/// relations, the struct of <c>forComparer</c>, shared storage, the order of large frozen sets, null inputs and the module
/// next to the class <c>System.Collections.Frozen.FrozenSet</c>.
/// </summary>
/// <remarks>
/// The tables of functions are maps by name, and the names are the rows of their tests: MSTest names a row after its
/// values, and neither a function nor a frozen set prints a readable value.
/// </remarks>
[<TestClass; FrozenSetTestCategory>]
type FrozenSetTests () =

    static let ignoreCase : IEqualityComparer<string> = StringComparer.OrdinalIgnoreCase

    static let defaultComparer : IEqualityComparer<string> = EqualityComparer<string>.Default

    /// Creates frozen sets that compare without case.
    static let caseless = FrozenSet.forComparer ignoreCase

    /// Words that compare without case, so that a value can equal an element that differs from it.
    static let words () = caseless.ofArray [| "apple"; "Banana"; "cherry" |]

    /// <summary>
    /// Adds a value to a frozen set, which has no <c>add</c>: <c>union</c> keeps the comparer of the frozen set, and the
    /// element of the frozen set where the value equals it in that comparer.
    /// </summary>
    static let add (value : string) (set : FrozenSet<string>) = FrozenSet.union set (FrozenSet.singleton value)

    /// The elements of a frozen set, sorted in the order of F# generic comparison.
    static let sorted (set : FrozenSet<'T>) = FrozenSet.toArray set |> Array.sort

    static let isLong (word : string) = word.Length > 5

    static let byLength (word : string) = if isLong word then Choice1Of2 word else Choice2Of2 word

    static let nullSet = Unchecked.defaultof<FrozenSet<int>>

    static let nullComparer = Unchecked.defaultof<IEqualityComparer<string>>

    /// The creation functions without a comparer, by name.
    static let defaultCreations : Map<string, unit -> FrozenSet<string>> =
        Map.ofArray [|
            nameof FrozenSet.empty, (fun () -> FrozenSet.empty<string>)
            nameof FrozenSet.ofArray, (fun () -> FrozenSet.ofArray [| "a" |])
            // The function under test takes a list
            nameof FrozenSet.ofList, (fun () -> FrozenSet.ofList [ "a" ])
            nameof FrozenSet.ofReadOnlySpan, (fun () -> FrozenSet.ofReadOnlySpan (ReadOnlySpan<string> [| "a" |]))
            nameof FrozenSet.ofSeq, (fun () -> FrozenSet.ofSeq (Seq.singleton "a"))
            nameof FrozenSet.ofSpan, (fun () -> FrozenSet.ofSpan (Span<string> [| "a" |]))
            nameof FrozenSet.singleton, (fun () -> FrozenSet.singleton "a")
        |]

    /// The members of the struct, by name, each with the frozen set that it creates from "apple" and "APPLE".
    static let structCreations : Map<string, unit -> FrozenSet<string>> =
        Map.ofArray [|
            nameof caseless.empty, (fun () -> caseless.empty |> add "apple" |> add "APPLE")
            nameof caseless.ofArray, (fun () -> caseless.ofArray [| "apple"; "APPLE" |])
            // The member under test takes a list
            nameof caseless.ofList, (fun () -> caseless.ofList [ "apple"; "APPLE" ])
            nameof caseless.ofReadOnlySpan, (fun () -> caseless.ofReadOnlySpan (ReadOnlySpan<string> [| "apple"; "APPLE" |]))
            nameof caseless.ofSeq, (fun () -> caseless.ofSeq (Seq.ofArray [| "apple"; "APPLE" |]))
            nameof caseless.ofSpan, (fun () -> caseless.ofSpan (Span<string> [| "apple"; "APPLE" |]))
            nameof caseless.singleton, (fun () -> caseless.singleton "apple" |> add "APPLE")
        |]

    /// The functions that select elements, by name, applied to the words.
    static let selections : Map<string, FrozenSet<string> -> FrozenSet<string>> =
        Map.ofArray [|
            nameof FrozenSet.filter, FrozenSet.filter isLong
            ($"%s{nameof FrozenSet.partition} (accepted)",
             fun set ->
                 let struct (accepted, _) = FrozenSet.partition isLong set
                 accepted)
            ($"%s{nameof FrozenSet.partition} (rejected)",
             fun set ->
                 let struct (_, rejected) = FrozenSet.partition isLong set
                 rejected)
        |]

    /// The set algebra, by name, with the elements that it returns for the words and the frozen set of "APPLE" and "date".
    static let algebra : Map<string, struct ((FrozenSet<string> -> FrozenSet<string> -> FrozenSet<string>) * string array)> =
        Map.ofArray [|
            nameof FrozenSet.difference, struct (FrozenSet.difference, [| "Banana"; "cherry" |])
            // Intersect keeps the elements of the second set (ADR 0001, D14)
            nameof FrozenSet.intersect, struct (FrozenSet.intersect, [| "APPLE" |])
            nameof FrozenSet.symmetricDifference, struct (FrozenSet.symmetricDifference, [| "Banana"; "cherry"; "date" |])
            nameof FrozenSet.union, struct (FrozenSet.union, [| "Banana"; "apple"; "cherry"; "date" |])
        |]

    /// The transformations to another element type, by name, applied to the words.
    static let typeChanges : Map<string, FrozenSet<string> -> FrozenSet<string>> =
        Map.ofArray [|
            nameof FrozenSet.map, FrozenSet.map id
            nameof FrozenSet.choose, FrozenSet.choose ValueSome
            ($"%s{nameof FrozenSet.partitionWith} (first)",
             fun set ->
                 let struct (first, _) = FrozenSet.partitionWith byLength set
                 first)
            ($"%s{nameof FrozenSet.partitionWith} (second)",
             fun set ->
                 let struct (_, second) = FrozenSet.partitionWith byLength set
                 second)
        |]

    /// The functions that take a frozen set, by name and parameter, with the name of the parameter and a call with null.
    static let nullRejections : Map<string, struct (string * (unit -> unit))> =
        let set = FrozenSet.ofArray Inputs.Several

        Map.ofArray [|
            nameof FrozenSet.choose, struct ("set", (fun () -> FrozenSet.choose ValueSome nullSet |> ignore))
            nameof FrozenSet.comparer, struct ("set", (fun () -> FrozenSet.comparer nullSet |> ignore))
            nameof FrozenSet.contains, struct ("set", (fun () -> FrozenSet.contains 1 nullSet |> ignore))
            nameof FrozenSet.count, struct ("set", (fun () -> FrozenSet.count nullSet |> ignore))
            $"%s{nameof FrozenSet.difference} (set1)", struct ("set1", (fun () -> FrozenSet.difference nullSet set |> ignore))
            $"%s{nameof FrozenSet.difference} (set2)", struct ("set2", (fun () -> FrozenSet.difference set nullSet |> ignore))
            nameof FrozenSet.exists, struct ("set", (fun () -> FrozenSet.exists ((=) 1) nullSet |> ignore))
            nameof FrozenSet.filter, struct ("set", (fun () -> FrozenSet.filter ((=) 1) nullSet |> ignore))
            nameof FrozenSet.fold, struct ("set", (fun () -> FrozenSet.fold (+) 0 nullSet |> ignore))
            nameof FrozenSet.foldBack, struct ("set", (fun () -> FrozenSet.foldBack (+) nullSet 0 |> ignore))
            nameof FrozenSet.forall, struct ("set", (fun () -> FrozenSet.forall ((=) 1) nullSet |> ignore))
            $"%s{nameof FrozenSet.intersect} (set1)", struct ("set1", (fun () -> FrozenSet.intersect nullSet set |> ignore))
            $"%s{nameof FrozenSet.intersect} (set2)", struct ("set2", (fun () -> FrozenSet.intersect set nullSet |> ignore))
            $"%s{nameof FrozenSet.intersectMany} (null first)",
            struct ("sets", (fun () -> FrozenSet.intersectMany [| nullSet; set |] |> ignore))
            $"%s{nameof FrozenSet.intersectMany} (null second)",
            struct ("sets", (fun () -> FrozenSet.intersectMany [| set; nullSet |] |> ignore))
            nameof FrozenSet.isEmpty, struct ("set", (fun () -> FrozenSet.isEmpty nullSet |> ignore))
            $"%s{nameof FrozenSet.isProperSubset} (set1)",
            struct ("set1", (fun () -> FrozenSet.isProperSubset nullSet set |> ignore))
            $"%s{nameof FrozenSet.isProperSubset} (set2)",
            struct ("set2", (fun () -> FrozenSet.isProperSubset set nullSet |> ignore))
            $"%s{nameof FrozenSet.isProperSuperset} (set1)",
            struct ("set1", (fun () -> FrozenSet.isProperSuperset nullSet set |> ignore))
            $"%s{nameof FrozenSet.isProperSuperset} (set2)",
            struct ("set2", (fun () -> FrozenSet.isProperSuperset set nullSet |> ignore))
            $"%s{nameof FrozenSet.isSubset} (set1)", struct ("set1", (fun () -> FrozenSet.isSubset nullSet set |> ignore))
            $"%s{nameof FrozenSet.isSubset} (set2)", struct ("set2", (fun () -> FrozenSet.isSubset set nullSet |> ignore))
            $"%s{nameof FrozenSet.isSuperset} (set1)", struct ("set1", (fun () -> FrozenSet.isSuperset nullSet set |> ignore))
            $"%s{nameof FrozenSet.isSuperset} (set2)", struct ("set2", (fun () -> FrozenSet.isSuperset set nullSet |> ignore))
            nameof FrozenSet.iter, struct ("set", (fun () -> FrozenSet.iter ignore nullSet))
            nameof FrozenSet.map, struct ("set", (fun () -> FrozenSet.map id nullSet |> ignore))
            $"%s{nameof FrozenSet.overlaps} (set1)", struct ("set1", (fun () -> FrozenSet.overlaps nullSet set |> ignore))
            $"%s{nameof FrozenSet.overlaps} (set2)", struct ("set2", (fun () -> FrozenSet.overlaps set nullSet |> ignore))
            nameof FrozenSet.partition, struct ("set", (fun () -> FrozenSet.partition ((=) 1) nullSet |> ignore))
            nameof FrozenSet.partitionWith,
            struct ("set",
                    (fun () ->
                        FrozenSet.partitionWith (fun (item : int) -> Choice<int, int>.Choice1Of2 item) nullSet
                        |> ignore
                    ))
            $"%s{nameof FrozenSet.setEquals} (set1)", struct ("set1", (fun () -> FrozenSet.setEquals nullSet set |> ignore))
            $"%s{nameof FrozenSet.setEquals} (set2)", struct ("set2", (fun () -> FrozenSet.setEquals set nullSet |> ignore))
            $"%s{nameof FrozenSet.symmetricDifference} (set1)",
            struct ("set1", (fun () -> FrozenSet.symmetricDifference nullSet set |> ignore))
            $"%s{nameof FrozenSet.symmetricDifference} (set2)",
            struct ("set2", (fun () -> FrozenSet.symmetricDifference set nullSet |> ignore))
            nameof FrozenSet.toArray, struct ("set", (fun () -> FrozenSet.toArray nullSet |> ignore))
            nameof FrozenSet.toFlatList, struct ("set", (fun () -> FrozenSet.toFlatList nullSet |> ignore))
            nameof FrozenSet.toList, struct ("set", (fun () -> FrozenSet.toList nullSet |> ignore))
            nameof FrozenSet.toSeq, struct ("set", (fun () -> FrozenSet.toSeq nullSet |> ignore))
            nameof FrozenSet.tryGetValue, struct ("set", (fun () -> FrozenSet.tryGetValue 1 nullSet |> ignore))
            $"%s{nameof FrozenSet.union} (set1)", struct ("set1", (fun () -> FrozenSet.union nullSet set |> ignore))
            $"%s{nameof FrozenSet.union} (set2)", struct ("set2", (fun () -> FrozenSet.union set nullSet |> ignore))
            $"%s{nameof FrozenSet.unionMany} (null first)",
            struct ("sets", (fun () -> FrozenSet.unionMany [| nullSet; set |] |> ignore))
            $"%s{nameof FrozenSet.unionMany} (null second)",
            struct ("sets", (fun () -> FrozenSet.unionMany [| set; nullSet |] |> ignore))
        |]

    /// <summary>
    /// The functions that take a sequence, by name and by the input that they reject, with the parameter that names the
    /// input and the function on it.
    /// </summary>
    /// <remarks>
    /// A default flat list converts to a sequence without a check and fails when the sequence is enumerated, so the other
    /// inputs are valid and the function would enumerate the sequence (ADR 0001, D6).
    /// </remarks>
    static let sequenceRejections : Map<string, struct (string * (unit -> unit))> =
        let ints = FrozenSet.forComparer (EqualityComparer<int>.Default :> IEqualityComparer<int>)
        let objects = FrozenSet.forComparer (EqualityComparer<obj>.Default :> IEqualityComparer<obj>)
        let nullElements = Unchecked.defaultof<seq<int>>
        let defaultElements = Unchecked.defaultof<FlatList<int>> :> seq<int>
        let nullSets = Unchecked.defaultof<seq<FrozenSet<int>>>
        let defaultSets = Unchecked.defaultof<FlatList<FrozenSet<int>>> :> seq<FrozenSet<int>>

        // The flat list of strings is a sequence of objects, which a type test for the flat list of objects does not see
        let widenedElements = FlatListOracles.defaultStringsAsObjects

        Map.ofArray [|
            $"%s{nameof FrozenSet.intersectMany} (null sets)",
            struct ("sets", (fun () -> FrozenSet.intersectMany nullSets |> ignore))
            $"%s{nameof FrozenSet.intersectMany} (default flat list)",
            struct ("sets", (fun () -> FrozenSet.intersectMany defaultSets |> ignore))
            $"%s{nameof FrozenSet.ofSeq} (null elements)", struct ("elements", (fun () -> FrozenSet.ofSeq nullElements |> ignore))
            $"%s{nameof FrozenSet.ofSeq} (default flat list)",
            struct ("elements", (fun () -> FrozenSet.ofSeq defaultElements |> ignore))
            $"%s{nameof FrozenSet.ofSeq} (default flat list of strings as objects)",
            struct ("elements", (fun () -> FrozenSet.ofSeq widenedElements |> ignore))
            $"%s{nameof FrozenSet.unionMany} (null sets)", struct ("sets", (fun () -> FrozenSet.unionMany nullSets |> ignore))
            $"%s{nameof FrozenSet.unionMany} (default flat list)",
            struct ("sets", (fun () -> FrozenSet.unionMany defaultSets |> ignore))
            // The struct of forComparer
            $"forComparer %s{nameof ints.ofSeq} (null elements)",
            struct ("elements", (fun () -> ints.ofSeq nullElements |> ignore))
            $"forComparer %s{nameof ints.ofSeq} (default flat list)",
            struct ("elements", (fun () -> ints.ofSeq defaultElements |> ignore))
            $"forComparer %s{nameof ints.ofSeq} (default flat list of strings as objects)",
            struct ("elements", (fun () -> objects.ofSeq widenedElements |> ignore))
        |]

    /// <summary>
    /// The strings of the tests of the sequence forms of the relations, by name: "a1" and "a2" have the text "a" and are
    /// different instances, "A" differs from them in case, and "b" has other text.
    /// </summary>
    static let pool : Map<string, string> =
        Map.ofArray [| "a1", String [| 'a' |]; "a2", String [| 'a' |]; "A", "A"; "b", "b" |]

    /// The strings of the pool for their names.
    static let strings (names : string array) = names |> Array.map (fun name -> pool[name])

    /// <summary>
    /// The comparers of the first frozen set in the tests of the sequence forms, by name, each with whether it finds equal
    /// every pair of strings that the default comparer finds equal.
    /// </summary>
    static let sequenceComparers : Map<string, struct (IEqualityComparer<string> * bool)> =
        Map.ofArray [|
            "default comparer", struct (defaultComparer, true)
            "comparer that ignores case", struct (ignoreCase, true)
            // It tells "a1" from "a2", which the default comparer finds equal
            "reference comparer", struct (HashIdentity.Reference, false)
        |]

    /// <summary>
    /// The relations of a frozen set with a sequence, by name.
    /// </summary>
    /// <remarks>
    /// The strings tell the comparers apart: when the first frozen set compares by reference, each relation answers
    /// differently for the sequence that the default comparer converts, which keeps one of "a1" and "a2", than for the
    /// sequence that the comparer of the first frozen set converts.
    /// </remarks>
    static let sequenceRelations : Map<string, SequenceRelation> =
        Map.ofArray [|
            nameof FrozenSet.isProperSubset,
            {
                SetItems = [| "a1"; "a2" |]
                SequenceItems = [| "a1"; "a2"; "b" |]
                ClassMember = fun set elements -> set.IsProperSubsetOf elements
                ModuleFunction = FrozenSet.isProperSubset
            }
            nameof FrozenSet.isProperSuperset,
            {
                SetItems = [| "a1"; "a2"; "A" |]
                SequenceItems = [| "a1"; "a2"; "A" |]
                ClassMember = fun set elements -> set.IsProperSupersetOf elements
                ModuleFunction = FrozenSet.isProperSuperset
            }
            nameof FrozenSet.isSubset,
            {
                SetItems = [| "a1"; "a2"; "A" |]
                SequenceItems = [| "a1"; "a2"; "A" |]
                ClassMember = fun set elements -> set.IsSubsetOf elements
                ModuleFunction = FrozenSet.isSubset
            }
            nameof FrozenSet.isSuperset,
            {
                SetItems = [| "a1" |]
                SequenceItems = [| "a1"; "a2" |]
                ClassMember = fun set elements -> set.IsSupersetOf elements
                ModuleFunction = FrozenSet.isSuperset
            }
            nameof FrozenSet.overlaps,
            {
                SetItems = [| "a1" |]
                SequenceItems = [| "a2"; "a1" |]
                ClassMember = fun set elements -> set.Overlaps elements
                ModuleFunction = FrozenSet.overlaps
            }
            nameof FrozenSet.setEquals,
            {
                SetItems = [| "a1"; "a2"; "A" |]
                SequenceItems = [| "a1"; "a2"; "A" |]
                ClassMember = fun set elements -> set.SetEquals elements
                ModuleFunction = FrozenSet.setEquals
            }
        |]

    /// The relations that need no conversion, by name, as the changelog gives them: each tests the elements with contains.
    static let streamingRecipes : Map<string, FrozenSet<string> -> seq<string> -> bool> =
        Map.ofArray [|
            nameof FrozenSet.isSuperset, (fun set elements -> Seq.forall (fun item -> FrozenSet.contains item set) elements)
            nameof FrozenSet.overlaps, (fun set elements -> Seq.exists (fun item -> FrozenSet.contains item set) elements)
        |]

    /// <summary>
    /// The frozen sets of 3, 1 and 2, by the way that builds them: the module and the class, each with
    /// <c>System.Collections.Frozen</c> opened before and after this library.
    /// </summary>
    static let resolutions : Map<string, unit -> FrozenSet<int>> =
        Map.ofArray [|
            "the module", (fun () -> FrozenSet.ofArray [| 3; 1; 2 |])
            "the class", (fun () -> FrozenSet.ToFrozenSet [| 3; 1; 2 |])
            "the module with System.Collections.Frozen opened last", FrozenOpenedLast.fromModule
            "the class with System.Collections.Frozen opened last", FrozenOpenedLast.fromClass
        |]

    /// Asserts that toFlatList returns the items of a frozen set without a copy and in the order of enumeration.
    static let sharesItems (set : FrozenSet<'T>) =
        let flatList = FrozenSet.toFlatList set

        Assert.AreSame (
            ImmutableCollectionsMarshal.AsArray set.Items,
            ImmutableCollectionsMarshal.AsArray flatList,
            "FrozenSet.toFlatList must return the items without a copy"
        )

        CollectionAssert.AreEqual (
            Seq.toArray set,
            Seq.toArray flatList,
            "FrozenSet.toFlatList must keep the order of enumeration"
        )

    /// The names of the creation functions without a comparer, as rows.
    static member DefaultCreations = defaultCreations.Keys

    /// The names of the members of the struct that create frozen sets, as rows.
    static member StructCreations = structCreations.Keys

    /// The names of the functions that select elements, as rows.
    static member Selections = selections.Keys

    /// The names of the set algebra, as rows.
    static member Algebra = algebra.Keys

    /// The names of the transformations to another element type, as rows.
    static member TypeChanges = typeChanges.Keys

    /// The names of the functions that take a frozen set, with the parameter where a function takes two, as rows.
    static member NullRejections = nullRejections.Keys

    /// The names of the functions that take a sequence, with the input that they reject, as rows.
    static member SequenceRejections = sequenceRejections.Keys

    /// The relations of a frozen set with a sequence, each with every comparer of the first frozen set, as rows.
    static member SequenceRelationRows = seq {
        for relation in sequenceRelations.Keys do
            for comparer in sequenceComparers.Keys do
                struct (relation, comparer)
    }

    /// The relations that need no conversion, each with every comparer of the first frozen set, as rows.
    static member StreamingRows = seq {
        for relation in streamingRecipes.Keys do
            for comparer in sequenceComparers.Keys do
                struct (relation, comparer)
    }

    /// The ways that build a frozen set through the module or the class, as rows.
    static member Resolutions = resolutions.Keys

    /// The frozen sets whose items toFlatList returns: a small one and two large ones, as rows.
    static member SharingSamples = [| FrozenSetSample.Several; FrozenSetSample.Integers; FrozenSetSample.CaselessWords |]

    /// <summary>
    /// The large frozen sets with every function that visits the elements, as rows: the contracts run on small sets, and
    /// the BCL stores large ones in hash tables of different layouts.
    /// </summary>
    static member Visits = seq {
        for sample in
            [|
                FrozenSetSample.Integers
                FrozenSetSample.Int64s
                FrozenSetSample.Words
                FrozenSetSample.CaselessWords
                FrozenSetSample.Pairs
            |] do
            for visit in
                [|
                    FrozenSetVisit.Fold
                    FrozenSetVisit.FoldBack
                    FrozenSetVisit.Iter
                    FrozenSetVisit.ToArray
                    FrozenSetVisit.ToFlatList
                    FrozenSetVisit.ToList
                    FrozenSetVisit.ToSeq
                |] do
                struct (sample, visit)
    }

    // ---- Comparers ----

    [<TestMethod; DynamicData(nameof FrozenSetTests.DefaultCreations)>]
    member _.``creation without a comparer compares with the default comparer`` (name : string) =
        Assert.AreSame (
            defaultComparer,
            FrozenSet.comparer (defaultCreations[name]()),
            $"FrozenSet.%s{name} must compare with EqualityComparer.Default"
        )

    [<TestMethod>]
    member _.``forComparer creates the struct with the comparer`` () =
        Assert.AreSame (ignoreCase, (FrozenSet.forComparer ignoreCase).Comparer, "FrozenSet.forComparer must keep the comparer")

        Assert.AreSame (
            ignoreCase,
            (CustomComparerFrozenSet ignoreCase).Comparer,
            "The constructor of CustomComparerFrozenSet must keep the comparer"
        )

    [<TestMethod; DynamicData(nameof FrozenSetTests.StructCreations)>]
    member _.``the members of the struct create frozen sets that compare with its comparer`` (name : string) =
        let set = structCreations[name]()
        Assert.AreSame (ignoreCase, FrozenSet.comparer set, $"CustomComparerFrozenSet.%s{name} must keep the comparer")

        Assert.AreEqual<string>(
            "apple",
            Assert.ContainsSingle (set, $"CustomComparerFrozenSet.%s{name} must compare without case"),
            $"CustomComparerFrozenSet.%s{name} must keep the first of the elements that the comparer considers equal"
        )

    // The shared contract states that one element of each class stays. Which one stays is a property of the BCL member that
    // creates the set, so each module pins it, here in both orders of the input.
    [<TestMethod;
      DataRow("ofReadOnlySpan", "apple", "APPLE");
      DataRow("ofReadOnlySpan", "APPLE", "apple");
      DataRow("ofSpan", "apple", "APPLE");
      DataRow("ofSpan", "APPLE", "apple")>]
    member _.``the span members of the struct keep the first of the elements that its comparer considers equal``
        (name : string, first : string, second : string)
        =
        let input = [| first; second |]

        let set =
            match name with
            | "ofReadOnlySpan" -> caseless.ofReadOnlySpan (ReadOnlySpan<string> input)
            | "ofSpan" -> caseless.ofSpan (Span<string> input)
            | name -> failwith $"CustomComparerFrozenSet has no span member %s{name}"

        Assert.AreEqual<string>(
            first,
            Assert.ContainsSingle (set, $"CustomComparerFrozenSet.%s{name} must compare without case"),
            $"CustomComparerFrozenSet.%s{name} must keep the first of the elements that the comparer considers equal"
        )

    [<TestMethod; DataRow "ofReadOnlySpan"; DataRow "ofSpan">]
    member _.``the span functions keep the first of the elements that are equal in the default comparer`` (name : string) =
        // Two strings that are equal and are two instances, so the instance tells which one stays
        let first = String ('x', 3)
        let second = String ('x', 3)

        let create (input : string array) =
            match name with
            | "ofReadOnlySpan" -> FrozenSet.ofReadOnlySpan (ReadOnlySpan<string> input)
            | "ofSpan" -> FrozenSet.ofSpan (Span<string> input)
            | name -> failwith $"FrozenSet has no span function %s{name}"

        Assert.AreSame (
            first,
            Assert.ContainsSingle (create [| first; second |], $"FrozenSet.%s{name} must hold equal elements once"),
            $"FrozenSet.%s{name} must keep the first of the elements that are equal"
        )

        Assert.AreSame (
            second,
            Assert.ContainsSingle (create [| second; first |], $"FrozenSet.%s{name} must hold equal elements once"),
            $"FrozenSet.%s{name} must keep the first of the elements that are equal in either order"
        )

    [<TestMethod>]
    member _.``the struct gives an existing frozen set its comparer`` () =
        let set = FrozenSet.ofArray [| "apple"; "APPLE"; "Banana" |]
        let ignoringCase = caseless.ofSeq set

        Assert.AreSame (
            ignoreCase,
            FrozenSet.comparer ignoringCase,
            "CustomComparerFrozenSet.ofSeq must give the frozen set the comparer"
        )

        Assert.HasCount (
            2,
            ignoringCase,
            "CustomComparerFrozenSet.ofSeq must merge the elements that the comparer considers equal"
        )

        Assert.HasCount (3, set, "CustomComparerFrozenSet.ofSeq must not change the frozen set")

    [<TestMethod>]
    member _.``the default value of the struct compares with the default comparer`` () =
        let defaultValue = Unchecked.defaultof<CustomComparerFrozenSet<string>>
        let set = defaultValue.ofArray [| "apple"; "APPLE" |]

        Assert.AreSame (
            defaultComparer,
            defaultValue.Comparer,
            "The default CustomComparerFrozenSet must return the default comparer"
        )

        Assert.AreSame (
            defaultComparer,
            FrozenSet.comparer set,
            "The default CustomComparerFrozenSet must create frozen sets with it"
        )

        Assert.HasCount (2, set, "The default CustomComparerFrozenSet must tell the case apart")

        Assert.AreSame (
            FrozenSet<string>.Empty,
            defaultValue.empty,
            "The default CustomComparerFrozenSet must return the empty frozen set of the class"
        )

    [<TestMethod; DynamicData(nameof FrozenSetTests.Selections)>]
    member _.``filter and partition keep the comparer`` (name : string) =
        Assert.AreSame (ignoreCase, FrozenSet.comparer (selections[name](words ())), $"FrozenSet.%s{name} must keep the comparer")

    [<TestMethod>]
    member _.``partition keeps comparing without case`` () =
        let struct (accepted, _) = FrozenSet.partition isLong (words ())
        Assert.IsTrue (FrozenSet.contains "BANANA" accepted, "FrozenSet.partition must keep comparing without case")

    [<TestMethod; DynamicData(nameof FrozenSetTests.Algebra)>]
    member _.``set algebra keeps the comparer of the first set and compares with it`` (name : string) =
        let struct (combine, expected) = algebra[name]
        let result = combine (words ()) (FrozenSet.ofArray [| "APPLE"; "date" |])

        Assert.AreSame (ignoreCase, FrozenSet.comparer result, $"FrozenSet.%s{name} must keep the comparer of the first set")
        CollectionAssert.AreEqual (expected, sorted result, $"FrozenSet.%s{name} must compare with the comparer of the first set")

    [<TestMethod>]
    member _.``intersect compares with the comparer of the first set when it is the default one`` () =
        let intersection = FrozenSet.intersect (FrozenSet.ofArray [| "APPLE"; "date" |]) (words ())

        Assert.AreSame (
            defaultComparer,
            FrozenSet.comparer intersection,
            "FrozenSet.intersect must keep the comparer of the first set"
        )

        Assert.IsEmpty (intersection, "FrozenSet.intersect must tell APPLE from apple in the default comparer of the first set")

    [<TestMethod>]
    member _.``isProperSuperset counts the elements that the comparer of the first set finds equal once`` () =
        // The second set tells "apple" and "APPLE" apart; for the first set they are one element, and it holds "Banana"
        // besides. FrozenSet answers with set semantics already, as the documentation of IsProperSupersetOf says (ADR 0001,
        // D14)
        Assert.IsTrue (
            FrozenSet.isProperSuperset (caseless.ofArray [| "apple"; "Banana" |]) (FrozenSet.ofArray [| "apple"; "APPLE" |]),
            "FrozenSet.isProperSuperset must count the elements that the comparer of the first set finds equal once"
        )

    [<TestMethod>]
    member _.``the predicates of two sets compare with the comparer of the first set`` () =
        let apple = caseless.ofArray [| "APPLE" |]
        let set = words ()

        Assert.IsTrue (FrozenSet.isSubset apple set, "FrozenSet.isSubset must compare without case")
        Assert.IsTrue (FrozenSet.isProperSubset apple set, "FrozenSet.isProperSubset must compare without case")
        Assert.IsTrue (FrozenSet.isSuperset set apple, "FrozenSet.isSuperset must compare without case")
        Assert.IsTrue (FrozenSet.isProperSuperset set apple, "FrozenSet.isProperSuperset must compare without case")
        Assert.IsTrue (FrozenSet.overlaps apple set, "FrozenSet.overlaps must compare without case")

        Assert.IsTrue (
            FrozenSet.setEquals set (FrozenSet.ofArray [| "APPLE"; "BANANA"; "CHERRY" |]),
            "FrozenSet.setEquals must compare without case"
        )

        Assert.IsFalse (
            FrozenSet.isSubset (FrozenSet.ofArray [| "APPLE" |]) set,
            "FrozenSet.isSubset must compare with the default comparer of the first set"
        )

    // The way to pass a sequence to a relation, as the changelog gives it: convert the sequence with the comparer of
    // the first set. The default comparer gives the same answer when the comparer of the first set finds equal what the
    // default comparer finds equal, and another answer when it tells such strings apart, as the reference comparer does
    [<TestMethod; DynamicData(nameof FrozenSetTests.SequenceRelationRows)>]
    member _.``a relation with a sequence takes the sequence that the comparer of the first set converts``
        (relation : string, comparer : string)
        =
        Assert.AreNotSame (pool["a1"], pool["a2"], "The test needs two strings with the same text that are different instances")

        let relationOfSequence = sequenceRelations[relation]
        let struct (firstComparer, defaultFindsEqualWhatItFindsEqual) = sequenceComparers[comparer]
        let set = (FrozenSet.forComparer firstComparer).ofArray(strings relationOfSequence.SetItems)
        let elements = strings relationOfSequence.SequenceItems
        let expected = relationOfSequence.ClassMember set elements

        Assert.AreEqual<bool>(
            expected,
            relationOfSequence.ModuleFunction set ((FrozenSet.forComparer (FrozenSet.comparer set)).ofSeq elements),
            $"FrozenSet.%s{relation} must answer as the member of the class for the sequence that the comparer of the first set converts"
        )

        let withDefaultComparer = relationOfSequence.ModuleFunction set (FrozenSet.ofSeq elements)

        if defaultFindsEqualWhatItFindsEqual then
            Assert.AreEqual<bool>(
                expected,
                withDefaultComparer,
                $"FrozenSet.%s{relation} must answer as the member of the class for the sequence that the default comparer converts, because the comparer of the first set finds equal what the default comparer finds equal"
            )
        else
            Assert.AreNotEqual<bool>(
                expected,
                withDefaultComparer,
                $"FrozenSet.%s{relation} must answer differently for the sequence that the default comparer converts, because the comparer of the first set tells apart strings that the default comparer finds equal"
            )

    [<TestMethod; DynamicData(nameof FrozenSetTests.StreamingRows)>]
    member _.``overlaps and isSuperset with a sequence test each element with contains`` (relation : string, comparer : string) =
        let relationOfSequence = sequenceRelations[relation]
        let struct (firstComparer, _) = sequenceComparers[comparer]
        let set = (FrozenSet.forComparer firstComparer).ofArray(strings relationOfSequence.SetItems)
        let elements = strings relationOfSequence.SequenceItems

        Assert.AreEqual<bool>(
            relationOfSequence.ClassMember set elements,
            streamingRecipes[relation] set elements,
            $"%s{relation} with a sequence must test each element with FrozenSet.contains, in the comparer of the first set"
        )

    [<TestMethod>]
    member _.``intersectMany and unionMany keep the comparer of the first set`` () =
        let sets = [| words (); FrozenSet.ofArray [| "APPLE"; "date" |] |]
        let intersection = FrozenSet.intersectMany sets
        let union = FrozenSet.unionMany sets

        Assert.AreSame (
            ignoreCase,
            FrozenSet.comparer intersection,
            "FrozenSet.intersectMany must keep the comparer of the first set"
        )

        Assert.AreSame (ignoreCase, FrozenSet.comparer union, "FrozenSet.unionMany must keep the comparer of the first set")

        CollectionAssert.AreEqual (
            [| "APPLE" |],
            sorted intersection,
            "FrozenSet.intersectMany must compare without case and hold the element of the last set"
        )

        CollectionAssert.AreEqual (
            [| "Banana"; "apple"; "cherry"; "date" |],
            sorted union,
            "FrozenSet.unionMany must compare without case"
        )

    [<TestMethod>]
    member _.``unionMany of no sets is empty with the default comparer`` () =
        let union = FrozenSet.unionMany Seq.empty<FrozenSet<string>>
        Assert.IsEmpty (union, "FrozenSet.unionMany of no sets must be empty")

        Assert.AreSame (
            defaultComparer,
            FrozenSet.comparer union,
            "FrozenSet.unionMany of no sets must compare with the default comparer"
        )

    [<TestMethod; DynamicData(nameof FrozenSetTests.TypeChanges)>]
    member _.``map, choose and partitionWith compare with the default comparer of the result`` (name : string) =
        Assert.AreSame (
            defaultComparer,
            FrozenSet.comparer (typeChanges[name](words ())),
            $"FrozenSet.%s{name} must compare with the default comparer"
        )

    [<TestMethod>]
    member _.``map does not keep the comparer of its input`` () =
        // The default comparer tells the case apart, although the element type is the same
        Assert.IsFalse (
            FrozenSet.contains "APPLE" (FrozenSet.map id (words ())),
            "FrozenSet.map must not keep the comparer of its input"
        )

    // ---- Sharing and order ----

    [<TestMethod>]
    member _.``empty is the empty frozen set of the class`` () =
        Assert.AreSame (FrozenSet<int>.Empty, FrozenSet.empty<int>, "FrozenSet.empty must be the empty frozen set of the class")

    [<TestMethod>]
    member _.``ofSeq of a frozen set with the default comparer returns it`` () =
        let set = FrozenSet.ofArray Inputs.Several
        Assert.AreSame (set, FrozenSet.ofSeq set, "FrozenSet.ofSeq of a frozen set with the default comparer must not copy it")

        let copy = FrozenSet.ofSeq (words ())
        Assert.AreSame (defaultComparer, FrozenSet.comparer copy, "FrozenSet.ofSeq must compare with the default comparer")
        Assert.HasCount (3, copy, "FrozenSet.ofSeq must keep the elements of a frozen set with another comparer")

    [<TestMethod>]
    member _.``the struct returns a frozen set that has its comparer`` () =
        let set = words ()

        Assert.AreSame (
            set,
            caseless.ofSeq set,
            "CustomComparerFrozenSet.ofSeq of a frozen set with the comparer must not copy it"
        )

    // ToFrozenSet returns a frozen set whose comparer Equals the comparer, not only the same instance (ADR 0001, D14)
    [<TestMethod>]
    member _.``the struct returns a frozen set whose comparer equals its comparer`` () =
        let first : IEqualityComparer<string> = StringComparer.Create (CultureInfo.InvariantCulture, true)
        let second : IEqualityComparer<string> = StringComparer.Create (CultureInfo.InvariantCulture, true)
        Assert.AreNotSame (first, second, "The test needs two comparer instances")
        Assert.AreEqual<obj>(first, second, "The test needs two comparers that are equal")

        let set = (FrozenSet.forComparer first).ofArray [| "apple"; "Banana" |]

        Assert.AreSame (
            set,
            (FrozenSet.forComparer second).ofSeq set,
            "CustomComparerFrozenSet.ofSeq must return a frozen set whose comparer equals its comparer"
        )

    [<TestMethod>]
    member _.``intersectMany of one frozen set returns it`` () =
        let set = words ()
        Assert.AreSame (set, FrozenSet.intersectMany [| set |], "FrozenSet.intersectMany of one frozen set must return it")

    [<TestMethod; DynamicData(nameof FrozenSetTests.SharingSamples)>]
    member _.``toFlatList returns the items of the frozen set without a copy`` (sample : FrozenSetSample) =
        sample.Run
            {
                new IFrozenSetCheck with
                    member _.Check set = sharesItems set
            }

    [<TestMethod; DynamicData(nameof FrozenSetTests.Visits)>]
    member _.``the functions visit the elements of large sets in the order of enumeration``
        (sample : FrozenSetSample, visit : FrozenSetVisit)
        =
        sample.Run
            {
                new IFrozenSetCheck with
                    member _.Check set =
                        CollectionAssert.AreEqual (
                            Seq.toArray set,
                            visit.Apply set,
                            $"FrozenSet.%s{visit.Name} must visit the elements in the order of enumeration"
                        )
            }

    // ---- Access and search ----

    [<TestMethod>]
    member _.``tryGetValue returns the element that the frozen set holds`` () =
        let stored = String ('x', 3)
        let set = caseless.ofArray [| stored; "apple" |]

        match FrozenSet.tryGetValue "XXX" set with
        | ValueSome found -> Assert.AreSame (stored, found, "FrozenSet.tryGetValue must return the instance of the frozen set")
        | ValueNone -> Assert.Fail "FrozenSet.tryGetValue must find the element that equals the value in the comparer"

        Assert.AreEqual<string voption>(
            ValueSome "apple",
            FrozenSet.tryGetValue "APPLE" set,
            "FrozenSet.tryGetValue must compare without case"
        )

        Assert.AreEqual<string voption>(
            ValueNone,
            FrozenSet.tryGetValue "date" set,
            "FrozenSet.tryGetValue must return nothing for a missing value"
        )

    [<TestMethod>]
    member _.``contains compares with the comparer of the frozen set`` () =
        Assert.IsTrue (FrozenSet.contains "BANANA" (words ()), "FrozenSet.contains must compare without case")

        Assert.IsFalse (
            FrozenSet.contains "BANANA" (FrozenSet.ofArray [| "Banana" |]),
            "FrozenSet.contains must compare with the default comparer"
        )

    // ---- Null inputs ----

    [<TestMethod; DynamicData(nameof FrozenSetTests.NullRejections)>]
    member _.``functions reject a null frozen set`` (name : string) =
        let struct (paramName, actual) = nullRejections[name]

        let ex =
            Assert.ThrowsExactly<ArgumentNullException>(Action actual, $"FrozenSet.%s{name} must reject a null frozen set")

        Assert.AreEqual<string | null>(paramName, ex.ParamName, $"FrozenSet.%s{name} must name the parameter of the frozen set")

    [<TestMethod; DynamicData(nameof FrozenSetTests.SequenceRejections)>]
    member _.``functions that take a sequence reject a null sequence and a default flat list given as the sequence``
        (name : string)
        =
        let struct (paramName, actual) = sequenceRejections[name]
        FlatListAssert.RejectsNull ($"FrozenSet.%s{name}", paramName, actual)

    [<TestMethod>]
    member _.``a flat list that is not default passes as a sequence of objects and as a sequence of frozen sets`` () =
        let widened = FlatListOracles.stringsAsObjects (FlatList.ofArray [| "a"; "b" |])
        let sets = FlatList.ofArray [| FrozenSet.ofArray [| 1; 2 |]; FrozenSet.ofArray [| 2; 3 |] |]

        CollectionAssert.AreEquivalent (
            [| ("a" :> obj); ("b" :> obj) |],
            FrozenSet.toArray (FrozenSet.ofSeq widened),
            "FrozenSet.ofSeq must take the elements of a flat list of strings as objects"
        )

        CollectionAssert.AreEquivalent (
            [| ("a" :> obj); ("b" :> obj) |],
            FrozenSet.toArray ((FrozenSet.forComparer (EqualityComparer<obj>.Default :> IEqualityComparer<obj>)).ofSeq widened),
            "CustomComparerFrozenSet.ofSeq must take the elements of a flat list of strings as objects"
        )

        CollectionAssert.AreEquivalent (
            [| 1; 2; 3 |],
            FrozenSet.toArray (FrozenSet.unionMany sets),
            "FrozenSet.unionMany must take the frozen sets of a flat list"
        )

        CollectionAssert.AreEquivalent (
            [| 2 |],
            FrozenSet.toArray (FrozenSet.intersectMany sets),
            "FrozenSet.intersectMany must take the frozen sets of a flat list"
        )

    // forComparer calls the constructor of the struct, which checks the comparer, so both name its parameter (D12)
    [<TestMethod>]
    member _.``forComparer rejects a null comparer`` () =
        let ex =
            Assert.ThrowsExactly<ArgumentNullException>(
                Action (fun () -> FrozenSet.forComparer nullComparer |> ignore),
                "FrozenSet.forComparer must reject a null comparer"
            )

        Assert.AreEqual<string | null>("comparer", ex.ParamName, "FrozenSet.forComparer must name the parameter comparer")

    [<TestMethod>]
    member _.``the constructor of the struct rejects a null comparer`` () =
        let ex =
            Assert.ThrowsExactly<ArgumentNullException>(
                Action (fun () -> CustomComparerFrozenSet nullComparer |> ignore),
                "The constructor of CustomComparerFrozenSet must reject a null comparer"
            )

        Assert.AreEqual<string | null>(
            "comparer",
            ex.ParamName,
            "The constructor of CustomComparerFrozenSet must name the parameter comparer"
        )

    // ---- Name resolution ----

    [<TestMethod; DynamicData(nameof FrozenSetTests.Resolutions)>]
    member _.``the module and the class FrozenSet resolve side by side`` (name : string) =
        // This file opens System.Collections.Frozen before this library, and FrozenOpenedLast opens it after this library.
        // FrozenSet.ofArray compiles only against the module and FrozenSet.ToFrozenSet only against the class.
        CollectionAssert.AreEqual (
            [| 1; 2; 3 |],
            sorted (resolutions[name]()),
            $"FrozenSet must build the frozen set through %s{name}"
        )

#if NET10_0_OR_GREATER
/// <summary>
/// The alternate lookups of <c>FrozenSet</c>, which exist on <c>net10.0</c> only (ADR 0001, D11).
/// </summary>
/// <remarks>
/// The frozen sets of 64-bit integers whose comparer also compares 32-bit integers are rows of their sizes: the BCL scans
/// a small one and hashes a large one. The frozen sets whose comparers compare no 32-bit integers are rows of the names of
/// the comparers.
/// </remarks>
[<TestClass; FrozenSetTestCategory>]
type FrozenSetAlternateLookupTests () =

    static let nullSet = Unchecked.defaultof<FrozenSet<int64>>

    /// The frozen set of the 64-bit integers from 1 to a count, whose comparer also compares 32-bit integers.
    static let numbers (count : int) = FrozenSetOps.ofInt64WithInt32Comparer [| 1L .. int64 count |]

    /// The frozen sets of 64-bit integers whose comparers compare no 32-bit integers, by the name of the comparer.
    static let withoutAlternate : Map<string, unit -> FrozenSet<int64>> =
        Map.ofArray [|
            "HashIdentity.Structural", (fun () -> (FrozenSet.forComparer HashIdentity.Structural).ofArray [| 1L; 2L; 3L |])
            "EqualityComparer.Default", (fun () -> FrozenSet.ofArray [| 1L; 2L; 3L |])
        |]

    /// The alternate lookups, by name, each with a call with a null frozen set.
    static let nullRejections : Map<string, unit -> unit> =
        Map.ofArray [|
            nameof FrozenSet.alternateLookup, (fun () -> FrozenSet.alternateLookup<int, int64> nullSet |> ignore)
            nameof FrozenSet.tryAlternateLookup, (fun () -> FrozenSet.tryAlternateLookup<int, int64> nullSet |> ignore)
        |]

    /// The names of the comparers that compare no 32-bit integers, as rows.
    static member WithoutAlternate = withoutAlternate.Keys

    /// The names of the alternate lookups, as rows.
    static member NullRejections = nullRejections.Keys

    [<TestMethod; DataRow 3; DataRow 100>]
    member _.``alternateLookup finds the elements by a value of the alternate type`` (count : int) =
        let set = numbers count
        let lookup = FrozenSet.alternateLookup<int, _> set
        Assert.AreSame (set, lookup.Set, "FrozenSet.alternateLookup must look up in the frozen set")
        Assert.IsTrue (lookup.Contains 2, "FrozenSet.alternateLookup must find 2")
        Assert.IsFalse (lookup.Contains 1000, "FrozenSet.alternateLookup must not find 1000")

        match lookup.TryGetValue 3 with
        | true, actual -> Assert.AreEqual<int64>(3L, actual, "FrozenSet.alternateLookup must return the element")
        | false, _ -> Assert.Fail "FrozenSet.alternateLookup must find 3"

    [<TestMethod; DynamicData(nameof FrozenSetAlternateLookupTests.WithoutAlternate)>]
    member _.``alternateLookup throws for a comparer without the alternate type`` (comparer : string) =
        let set = withoutAlternate[comparer]()

        Assert.ThrowsExactly<InvalidOperationException>(
            Action (fun () -> FrozenSet.alternateLookup<int, _> set |> ignore),
            "FrozenSet.alternateLookup must throw for a comparer that does not compare the alternate type"
        )
        |> ignore

    [<TestMethod; DataRow 3; DataRow 100>]
    member _.``tryAlternateLookup returns the lookup of a comparer with the alternate type`` (count : int) =
        let set = numbers count

        match FrozenSet.tryAlternateLookup<int, _> set with
        | ValueSome lookup ->
            Assert.AreSame (set, lookup.Set, "FrozenSet.tryAlternateLookup must look up in the frozen set")
            Assert.IsTrue (lookup.Contains 2, "FrozenSet.tryAlternateLookup must return a lookup that finds 2")
        | ValueNone -> Assert.Fail "FrozenSet.tryAlternateLookup must return the lookup of a comparer with the alternate type"

    [<TestMethod; DynamicData(nameof FrozenSetAlternateLookupTests.WithoutAlternate)>]
    member _.``tryAlternateLookup returns nothing for a comparer without the alternate type`` (comparer : string) =
        Assert.IsTrue (
            (FrozenSet.tryAlternateLookup<int, _>(withoutAlternate[comparer]())).IsNone,
            "FrozenSet.tryAlternateLookup must return ValueNone for a comparer that does not compare the alternate type"
        )

    [<TestMethod>]
    member _.``a span alternate takes the method of the class`` () =
        // An F# generic function cannot be instantiated with ReadOnlySpan<char>, so the module cannot wrap this lookup
        let ordinal : IEqualityComparer<string> = StringComparer.Ordinal
        let words = (FrozenSet.forComparer ordinal).ofArray [| "apple"; "Banana" |]
        let lookup = words.GetAlternateLookup<ReadOnlySpan<char>>()
        Assert.IsTrue (lookup.Contains ("apple".AsSpan()), "The alternate lookup of the class must find a span")
        Assert.IsFalse (lookup.Contains ("APPLE".AsSpan()), "The alternate lookup of the class must compare ordinally")

    [<TestMethod; DynamicData(nameof FrozenSetAlternateLookupTests.NullRejections)>]
    member _.``the alternate lookups reject a null frozen set`` (name : string) =
        let ex =
            Assert.ThrowsExactly<ArgumentNullException>(
                Action (nullRejections[name]),
                $"FrozenSet.%s{name} must reject a null frozen set"
            )

        Assert.AreEqual<string | null>("set", ex.ParamName, $"FrozenSet.%s{name} must name the parameter set")
#endif
