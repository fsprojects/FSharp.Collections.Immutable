namespace FSharp.Collections.Immutable.Tests

open System
open System.Collections.Generic
open System.Globalization
open FSharp.Collections.Immutable
open Microsoft.VisualStudio.TestTools.UnitTesting

/// <summary>
/// A creation function of <c>SortedSet</c> without a comparer, as a row of the tests that run on each one.
/// </summary>
/// <remarks>
/// The rows are union cases, because MSTest names a row after its values and a function prints no readable value. A case
/// creates sorted sets of any element type with F# comparison, which a table of functions of one type cannot do.
/// </remarks>
type SortedSetCreation =
    /// <c>SortedSet.empty</c>, which ignores the elements.
    | Empty
    /// <c>SortedSet.ofArray</c>.
    | OfArray
    /// <c>SortedSet.ofList</c>, with the elements in a list.
    | OfList
    /// <c>SortedSet.ofReadOnlySpan</c>, with a read-only span over the elements.
    | OfReadOnlySpan
    /// <c>SortedSet.ofSeq</c>, with the elements in a lazy sequence.
    | OfSeq
    /// <c>SortedSet.ofSpan</c>, with a span over the elements.
    | OfSpan
    /// <c>SortedSet.add</c> of every element to <c>SortedSet.empty</c>.
    | Add
    /// <c>SortedSet.singleton</c> of every element, united by <c>SortedSet.unionMany</c>.
    | Singleton
    /// <c>SortedSet.build</c> with an action that adds the elements to the builder.
    | Build
    /// A builder of <c>SortedSet.builder</c> filled with the elements, through <c>SortedSet.ofBuilder</c>.
    | Builder

    /// Creates a sorted set of the elements with the function of the case.
    member this.Create<'T when 'T : comparison> (elements : 'T array) : SortedSet<'T> =
        match this with
        | Empty -> SortedSet.empty
        | OfArray -> SortedSet.ofArray elements
        // The function under test takes a list
        | OfList -> SortedSet.ofList (List.ofArray elements)
        | OfReadOnlySpan -> SortedSet.ofReadOnlySpan (ReadOnlySpan<'T> elements)
        | OfSeq -> SortedSet.ofSeq (Seq.map id elements)
        | OfSpan -> SortedSet.ofSpan (Span<'T> elements)
        | Add -> Array.fold (fun set item -> SortedSet.add item set) SortedSet.empty elements
        | Singleton ->
            elements
            |> Array.map SortedSet.singleton
            |> SortedSet.unionMany
        | Build -> SortedSet.build (fun builder -> builder.UnionWith elements)
        | Builder ->
            let builder = SortedSet.builder ()
            builder.UnionWith elements
            SortedSet.ofBuilder builder

/// <summary>
/// The behaviour of <c>SortedSet</c> that the shared contracts do not cover: the order of F# generic comparison, the
/// comparers, the struct of <c>forComparer</c>, the sorted-only functions, shared storage and null inputs.
/// </summary>
/// <remarks>
/// The tables of functions are maps by name, and the names are the rows of their tests: MSTest names a row after its
/// values, and neither a function nor a sorted set prints a readable value.
/// </remarks>
[<TestClass; SortedSetTestCategory>]
type SortedSetTests () =

    static let ignoreCase : IComparer<string> = StringComparer.OrdinalIgnoreCase

    /// Creates sorted sets that order strings without case.
    static let caseless = SortedSet.forComparer ignoreCase

    /// Creates sorted sets of pairs that order the pairs by their numbers alone, so that pairs with the same number
    /// compare equal.
    static let byKey =
        SortedSet.forComparer (
            ComparisonIdentity.FromFunction (fun ((left : int), (_ : string)) (right, _) -> compare left right)
        )

    /// The comparer of the sorted sets of strings created without one. FSharp.Core keeps one comparer per primitive type
    /// and per string, so the comparer resolved here is the one of the module.
    static let defaultComparer : IComparer<string> = LanguagePrimitives.FastGenericComparer<string>

    /// Orders the numbers from the largest one to the smallest one.
    static let descending : IComparer<int> = ComparisonIdentity.FromFunction (fun left right -> compare right left)

    /// Strings in an order that differs between ordinal comparison, which F# generic comparison uses, and the culture.
    static let words = [| "b"; "B"; "a"; "A"; "é"; "e"; "Z"; "z" |]

    /// Numbers with duplicates, negative numbers and zero.
    static let numbers = [| 5; -3; 8; 5; 0; -3; 12; 7; -10 |]

    /// Pairs whose strings decide the order: F# generic comparison compares them ordinally, while the default comparer of
    /// a tuple compares them in the culture.
    static let pairs = [| (1, "b"); (0, "z"); (1, "B"); (1, "a"); (-1, "a"); (0, "z") |]

    /// Words that compare without case, so that a value can equal an element that differs from it.
    static let fruits () = caseless.ofArray [| "cherry"; "apple"; "Banana" |]

    static let isLong (word : string) = word.Length > 5

    static let byLength (word : string) = if isLong word then Choice1Of2 word else Choice2Of2 word

    static let nullSet = Unchecked.defaultof<SortedSet<int>>

    static let nullComparer = Unchecked.defaultof<IComparer<string>>

    /// Adds the elements one by one to a sorted set.
    static let addAll (set : SortedSet<string>) (elements : string array) =
        Array.fold (fun set item -> SortedSet.add item set) set elements

    /// The sorted set of a builder of the struct that added the elements.
    static let ofCaselessBuilder (elements : string array) =
        let builder = caseless.builder ()
        builder.UnionWith elements
        SortedSet.ofBuilder builder

    /// <summary>
    /// The members of the struct, by name, each with the sorted set that it creates from "cherry", "apple", "APPLE" and
    /// "Banana".
    /// </summary>
    /// <remarks>
    /// <c>empty</c> and <c>singleton</c> get the other elements through <c>SortedSet.add</c>.
    /// </remarks>
    static let structCreations : Map<string, unit -> SortedSet<string>> =
        Map.ofArray [|
            nameof caseless.empty, (fun () -> addAll caseless.empty [| "cherry"; "apple"; "APPLE"; "Banana" |])
            nameof caseless.ofArray, (fun () -> caseless.ofArray [| "cherry"; "apple"; "APPLE"; "Banana" |])
            // The member under test takes a list
            nameof caseless.ofList, (fun () -> caseless.ofList [ "cherry"; "apple"; "APPLE"; "Banana" ])
            nameof caseless.ofReadOnlySpan,
            (fun () -> caseless.ofReadOnlySpan (ReadOnlySpan<string> [| "cherry"; "apple"; "APPLE"; "Banana" |]))
            nameof caseless.ofSeq, (fun () -> caseless.ofSeq (Seq.ofArray [| "cherry"; "apple"; "APPLE"; "Banana" |]))
            nameof caseless.ofSpan, (fun () -> caseless.ofSpan (Span<string> [| "cherry"; "apple"; "APPLE"; "Banana" |]))
            nameof caseless.singleton, (fun () -> addAll (caseless.singleton "cherry") [| "apple"; "APPLE"; "Banana" |])
            nameof caseless.build,
            (fun () -> caseless.build (fun builder -> builder.UnionWith [| "cherry"; "apple"; "APPLE"; "Banana" |]))
            nameof caseless.builder, (fun () -> ofCaselessBuilder [| "cherry"; "apple"; "APPLE"; "Banana" |])
        |]

    /// The members of the struct that take elements, by name, with the struct that orders pairs by their numbers.
    static let byKeyCreations : Map<string, (int * string) array -> SortedSet<int * string>> =
        Map.ofArray [|
            nameof byKey.ofArray, (fun input -> byKey.ofArray input)
            // The member under test takes a list
            nameof byKey.ofList, (fun input -> byKey.ofList (List.ofArray input))
            nameof byKey.ofReadOnlySpan, (fun input -> byKey.ofReadOnlySpan (ReadOnlySpan<int * string> input))
            nameof byKey.ofSeq, (fun input -> byKey.ofSeq (Seq.ofArray input))
            nameof byKey.ofSpan, (fun input -> byKey.ofSpan (Span<int * string> input))
        |]

    /// The incremental changes, by name, applied to the fruits.
    static let changes : Map<string, SortedSet<string> -> SortedSet<string>> =
        Map.ofArray [|
            nameof SortedSet.add, SortedSet.add "date"
            nameof SortedSet.addRange, SortedSet.addRange [| "APPLE"; "date" |]
            nameof SortedSet.clear, SortedSet.clear
            nameof SortedSet.remove, SortedSet.remove "APPLE"
            nameof SortedSet.removeRange, SortedSet.removeRange [| "APPLE"; "date" |]
            nameof SortedSet.update, SortedSet.update (fun builder -> builder.Add "date" |> ignore)
        |]

    /// The elements of the fruits that are long, through partition.
    static let longFruits (set : SortedSet<string>) =
        let struct (accepted, _) = SortedSet.partition isLong set
        accepted

    /// The elements of the fruits that are short, through partition.
    static let shortFruits (set : SortedSet<string>) =
        let struct (_, rejected) = SortedSet.partition isLong set
        rejected

    /// The functions that select elements, by name, with the elements that they return for the fruits.
    static let selections : Map<string, struct ((SortedSet<string> -> SortedSet<string>) * string array)> =
        Map.ofArray [|
            nameof SortedSet.filter, struct (SortedSet.filter isLong, [| "Banana"; "cherry" |])
            $"%s{nameof SortedSet.partition} (accepted)", struct (longFruits, [| "Banana"; "cherry" |])
            $"%s{nameof SortedSet.partition} (rejected)", struct (shortFruits, [| "apple" |])
        |]

    /// The set algebra, by name, with the elements that it returns for the fruits and the sorted set of "APPLE" and "date".
    static let algebra : Map<string, struct ((SortedSet<string> -> SortedSet<string> -> SortedSet<string>) * string array)> =
        Map.ofArray [|
            nameof SortedSet.difference, struct (SortedSet.difference, [| "Banana"; "cherry" |])
            // Intersect keeps the elements of the second set
            nameof SortedSet.intersect, struct (SortedSet.intersect, [| "APPLE" |])
            nameof SortedSet.symmetricDifference, struct (SortedSet.symmetricDifference, [| "Banana"; "cherry"; "date" |])
            nameof SortedSet.union, struct (SortedSet.union, [| "apple"; "Banana"; "cherry"; "date" |])
        |]

    /// The first choices of partitionWith for the fruits.
    static let firstChoices (set : SortedSet<string>) =
        let struct (first, _) = SortedSet.partitionWith byLength set
        first

    /// The second choices of partitionWith for the fruits.
    static let secondChoices (set : SortedSet<string>) =
        let struct (_, second) = SortedSet.partitionWith byLength set
        second

    /// The transformations to another element type, by name, with the elements that they return for the fruits.
    static let typeChanges : Map<string, struct ((SortedSet<string> -> SortedSet<string>) * string array)> =
        Map.ofArray [|
            nameof SortedSet.map, struct (SortedSet.map id, [| "Banana"; "apple"; "cherry" |])
            nameof SortedSet.choose, struct (SortedSet.choose ValueSome, [| "Banana"; "apple"; "cherry" |])
            $"%s{nameof SortedSet.partitionWith} (first)", struct (firstChoices, [| "Banana"; "cherry" |])
            $"%s{nameof SortedSet.partitionWith} (second)", struct (secondChoices, [| "apple" |])
        |]

    /// The functions that take a sorted set, by name and parameter, with the name of the parameter and a call with null.
    static let nullRejections : Map<string, struct (string * (unit -> unit))> =
        let set = SortedSet.ofArray Inputs.Several

        Map.ofArray [|
            nameof SortedSet.add, struct ("set", (fun () -> SortedSet.add 1 nullSet |> ignore))
            nameof SortedSet.addRange, struct ("set", (fun () -> SortedSet.addRange [| 1 |] nullSet |> ignore))
            nameof SortedSet.choose, struct ("set", (fun () -> SortedSet.choose ValueSome nullSet |> ignore))
            nameof SortedSet.clear, struct ("set", (fun () -> SortedSet.clear nullSet |> ignore))
            nameof SortedSet.comparer, struct ("set", (fun () -> SortedSet.comparer nullSet |> ignore))
            nameof SortedSet.contains, struct ("set", (fun () -> SortedSet.contains 1 nullSet |> ignore))
            nameof SortedSet.count, struct ("set", (fun () -> SortedSet.count nullSet |> ignore))
            $"%s{nameof SortedSet.difference} (set1)", struct ("set1", (fun () -> SortedSet.difference nullSet set |> ignore))
            $"%s{nameof SortedSet.difference} (set2)", struct ("set2", (fun () -> SortedSet.difference set nullSet |> ignore))
            nameof SortedSet.exists, struct ("set", (fun () -> SortedSet.exists ((=) 1) nullSet |> ignore))
            nameof SortedSet.filter, struct ("set", (fun () -> SortedSet.filter ((=) 1) nullSet |> ignore))
            nameof SortedSet.fold, struct ("set", (fun () -> SortedSet.fold (+) 0 nullSet |> ignore))
            nameof SortedSet.foldBack, struct ("set", (fun () -> SortedSet.foldBack (+) nullSet 0 |> ignore))
            nameof SortedSet.forall, struct ("set", (fun () -> SortedSet.forall ((=) 1) nullSet |> ignore))
            nameof SortedSet.indexOf, struct ("set", (fun () -> SortedSet.indexOf 1 nullSet |> ignore))
            $"%s{nameof SortedSet.intersect} (set1)", struct ("set1", (fun () -> SortedSet.intersect nullSet set |> ignore))
            $"%s{nameof SortedSet.intersect} (set2)", struct ("set2", (fun () -> SortedSet.intersect set nullSet |> ignore))
            nameof SortedSet.intersectMany, struct ("sets", (fun () -> SortedSet.intersectMany [| set; nullSet |] |> ignore))
            nameof SortedSet.isEmpty, struct ("set", (fun () -> SortedSet.isEmpty nullSet |> ignore))
            $"%s{nameof SortedSet.isProperSubset} (set1)",
            struct ("set1", (fun () -> SortedSet.isProperSubset nullSet set |> ignore))
            $"%s{nameof SortedSet.isProperSubset} (set2)",
            struct ("set2", (fun () -> SortedSet.isProperSubset set nullSet |> ignore))
            $"%s{nameof SortedSet.isProperSuperset} (set1)",
            struct ("set1", (fun () -> SortedSet.isProperSuperset nullSet set |> ignore))
            $"%s{nameof SortedSet.isProperSuperset} (set2)",
            struct ("set2", (fun () -> SortedSet.isProperSuperset set nullSet |> ignore))
            $"%s{nameof SortedSet.isSubset} (set1)", struct ("set1", (fun () -> SortedSet.isSubset nullSet set |> ignore))
            $"%s{nameof SortedSet.isSubset} (set2)", struct ("set2", (fun () -> SortedSet.isSubset set nullSet |> ignore))
            $"%s{nameof SortedSet.isSuperset} (set1)", struct ("set1", (fun () -> SortedSet.isSuperset nullSet set |> ignore))
            $"%s{nameof SortedSet.isSuperset} (set2)", struct ("set2", (fun () -> SortedSet.isSuperset set nullSet |> ignore))
            nameof SortedSet.item, struct ("set", (fun () -> SortedSet.item 0 nullSet |> ignore))
            nameof SortedSet.iter, struct ("set", (fun () -> SortedSet.iter ignore nullSet))
            nameof SortedSet.map, struct ("set", (fun () -> SortedSet.map id nullSet |> ignore))
            nameof SortedSet.maxElement, struct ("set", (fun () -> SortedSet.maxElement nullSet |> ignore))
            nameof SortedSet.minElement, struct ("set", (fun () -> SortedSet.minElement nullSet |> ignore))
            $"%s{nameof SortedSet.overlaps} (set1)", struct ("set1", (fun () -> SortedSet.overlaps nullSet set |> ignore))
            $"%s{nameof SortedSet.overlaps} (set2)", struct ("set2", (fun () -> SortedSet.overlaps set nullSet |> ignore))
            nameof SortedSet.partition, struct ("set", (fun () -> SortedSet.partition ((=) 1) nullSet |> ignore))
            nameof SortedSet.partitionWith,
            struct ("set",
                    (fun () ->
                        SortedSet.partitionWith (fun (item : int) -> Choice<int, int>.Choice1Of2 item) nullSet
                        |> ignore
                    ))
            nameof SortedSet.remove, struct ("set", (fun () -> SortedSet.remove 1 nullSet |> ignore))
            nameof SortedSet.removeRange, struct ("set", (fun () -> SortedSet.removeRange [| 1 |] nullSet |> ignore))
            nameof SortedSet.rev, struct ("set", (fun () -> SortedSet.rev nullSet |> ignore))
            $"%s{nameof SortedSet.setEquals} (set1)", struct ("set1", (fun () -> SortedSet.setEquals nullSet set |> ignore))
            $"%s{nameof SortedSet.setEquals} (set2)", struct ("set2", (fun () -> SortedSet.setEquals set nullSet |> ignore))
            $"%s{nameof SortedSet.symmetricDifference} (set1)",
            struct ("set1", (fun () -> SortedSet.symmetricDifference nullSet set |> ignore))
            $"%s{nameof SortedSet.symmetricDifference} (set2)",
            struct ("set2", (fun () -> SortedSet.symmetricDifference set nullSet |> ignore))
            nameof SortedSet.toArray, struct ("set", (fun () -> SortedSet.toArray nullSet |> ignore))
            nameof SortedSet.toBuilder, struct ("set", (fun () -> SortedSet.toBuilder nullSet |> ignore))
            nameof SortedSet.toList, struct ("set", (fun () -> SortedSet.toList nullSet |> ignore))
            nameof SortedSet.toSeq, struct ("set", (fun () -> SortedSet.toSeq nullSet |> ignore))
            nameof SortedSet.tryGetValue, struct ("set", (fun () -> SortedSet.tryGetValue 1 nullSet |> ignore))
            nameof SortedSet.tryItem, struct ("set", (fun () -> SortedSet.tryItem 0 nullSet |> ignore))
            $"%s{nameof SortedSet.union} (set1)", struct ("set1", (fun () -> SortedSet.union nullSet set |> ignore))
            $"%s{nameof SortedSet.union} (set2)", struct ("set2", (fun () -> SortedSet.union set nullSet |> ignore))
            nameof SortedSet.unionMany, struct ("sets", (fun () -> SortedSet.unionMany [| nullSet; set |] |> ignore))
            nameof SortedSet.update, struct ("set", (fun () -> SortedSet.update ignore nullSet |> ignore))
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
        let set = SortedSet.ofArray Inputs.Several
        let ints = SortedSet.forComparer (Comparer<int>.Default :> IComparer<int>)
        let objectComparer = SortedSet.forComparer (Comparer<obj>.Default :> IComparer<obj>)
        let nullElements = Unchecked.defaultof<seq<int>>
        let defaultElements = Unchecked.defaultof<FlatList<int>> :> seq<int>
        let nullSets = Unchecked.defaultof<seq<SortedSet<int>>>
        let defaultSets = Unchecked.defaultof<FlatList<SortedSet<int>>> :> seq<SortedSet<int>>

        // The flat list of strings is a sequence of objects, which a type test for the flat list of objects does not see. obj
        // has no F# comparison, so the sorted sets of objects and the creation from objects come from the struct.
        let widenedElements = FlatListOracles.defaultStringsAsObjects
        let objects = objectComparer.ofArray [| ("a" :> obj) |]

        Map.ofArray [|
            $"%s{nameof SortedSet.addRange} (null elements)",
            struct ("elements", (fun () -> SortedSet.addRange nullElements set |> ignore))
            $"%s{nameof SortedSet.addRange} (default flat list)",
            struct ("elements", (fun () -> SortedSet.addRange defaultElements set |> ignore))
            $"%s{nameof SortedSet.addRange} (default flat list of strings as objects)",
            struct ("elements", (fun () -> SortedSet.addRange widenedElements objects |> ignore))
            $"%s{nameof SortedSet.intersectMany} (null sets)",
            struct ("sets", (fun () -> SortedSet.intersectMany nullSets |> ignore))
            $"%s{nameof SortedSet.intersectMany} (default flat list)",
            struct ("sets", (fun () -> SortedSet.intersectMany defaultSets |> ignore))
            $"%s{nameof SortedSet.ofSeq} (null elements)", struct ("elements", (fun () -> SortedSet.ofSeq nullElements |> ignore))
            $"%s{nameof SortedSet.ofSeq} (default flat list)",
            struct ("elements", (fun () -> SortedSet.ofSeq defaultElements |> ignore))
            $"%s{nameof SortedSet.removeRange} (null elements)",
            struct ("elements", (fun () -> SortedSet.removeRange nullElements set |> ignore))
            $"%s{nameof SortedSet.removeRange} (default flat list)",
            struct ("elements", (fun () -> SortedSet.removeRange defaultElements set |> ignore))
            $"%s{nameof SortedSet.removeRange} (default flat list of strings as objects)",
            struct ("elements", (fun () -> SortedSet.removeRange widenedElements objects |> ignore))
            $"%s{nameof SortedSet.unionMany} (null sets)", struct ("sets", (fun () -> SortedSet.unionMany nullSets |> ignore))
            $"%s{nameof SortedSet.unionMany} (default flat list)",
            struct ("sets", (fun () -> SortedSet.unionMany defaultSets |> ignore))
            // The struct of forComparer
            $"forComparer %s{nameof ints.ofSeq} (null elements)",
            struct ("elements", (fun () -> ints.ofSeq nullElements |> ignore))
            $"forComparer %s{nameof ints.ofSeq} (default flat list)",
            struct ("elements", (fun () -> ints.ofSeq defaultElements |> ignore))
            $"forComparer %s{nameof ints.ofSeq} (default flat list of strings as objects)",
            struct ("elements", (fun () -> objectComparer.ofSeq widenedElements |> ignore))
        |]

    /// The creation functions that create a sorted set of the elements, as rows.
    static member Creations = seq {
        OfArray
        OfList
        OfReadOnlySpan
        OfSeq
        OfSpan
        Add
        Singleton
        Build
        Builder
    }

    /// Every creation function without a comparer, the empty sorted set included, as rows.
    static member DefaultCreations = seq {
        Empty
        yield! SortedSetTests.Creations
    }

    /// The creation functions that take the elements as they are, as rows.
    static member ElementCreations = seq {
        OfArray
        OfList
        OfReadOnlySpan
        OfSeq
        OfSpan
    }

    /// The names of the members of the struct that create sorted sets, as rows.
    static member StructCreations = structCreations.Keys

    /// The names of the members of the struct that take elements, as rows.
    static member ByKeyCreations = byKeyCreations.Keys

    /// The names of the incremental changes, as rows.
    static member Changes = changes.Keys

    /// The names of the functions that select elements, as rows.
    static member Selections = selections.Keys

    /// The names of the set algebra, as rows.
    static member Algebra = algebra.Keys

    /// The names of the transformations to another element type, as rows.
    static member TypeChanges = typeChanges.Keys

    /// The names of the functions that take a sorted set, with the parameter where a function takes two, as rows.
    static member NullRejections = nullRejections.Keys

    /// The names of the functions that take a sequence, with the input that they reject, as rows.
    static member SequenceRejections = sequenceRejections.Keys

    /// The indices outside the numbers: before the first one, the number of distinct numbers and the largest index.
    static member OutOfRangeIndices = seq {
        -1
        (Set.ofArray numbers).Count
        Int32.MaxValue
    }

    /// The indices of tryItem with the element at each one or nothing: inside and on both sides of the numbers.
    static member TryItemCases = seq {
        let last = (Set.ofArray numbers).Count - 1
        struct (-1, ValueNone)
        struct (0, ValueSome -10)
        struct (3, ValueSome 5)
        struct (last, ValueSome 12)
        struct (last + 1, ValueNone)
    }

    // ---- Order ----

    [<TestMethod; DynamicData(nameof SortedSetTests.Creations)>]
    member _.``strings compare ordinally as in Set`` (creation : SortedSetCreation) =
        CollectionAssert.AreEqual (
            Set.toArray (Set.ofArray words),
            SortedSet.toArray (creation.Create words),
            "The creation function must order strings as Set does"
        )

    [<TestMethod>]
    member _.``strings sort ordinally, not in the order of the culture`` () =
        CollectionAssert.AreEqual (
            [| "A"; "B"; "Z"; "a"; "b"; "e"; "z"; "é" |],
            SortedSet.toArray (SortedSet.ofArray words),
            "SortedSet.ofArray must order strings ordinally, not in the order of the culture"
        )

    [<TestMethod; DynamicData(nameof SortedSetTests.Creations)>]
    member _.``negative numbers come first as in Set`` (creation : SortedSetCreation) =
        CollectionAssert.AreEqual (
            Set.toArray (Set.ofArray numbers),
            SortedSet.toArray (creation.Create numbers),
            "The creation function must order numbers as Set does"
        )

    [<TestMethod>]
    member _.``numbers sort in ascending order`` () =
        CollectionAssert.AreEqual (
            [| -10; -3; 0; 5; 7; 8; 12 |],
            SortedSet.toArray (SortedSet.ofArray numbers),
            "SortedSet.ofArray must order the numbers in ascending order"
        )

    [<TestMethod; DynamicData(nameof SortedSetTests.Creations)>]
    member _.``tuples compare their strings ordinally as in Set`` (creation : SortedSetCreation) =
        CollectionAssert.AreEqual (
            Set.toArray (Set.ofArray pairs),
            SortedSet.toArray (creation.Create pairs),
            "The creation function must order tuples as Set does"
        )

    [<TestMethod>]
    member _.``tuples sort their strings ordinally`` () =
        CollectionAssert.AreEqual (
            [| (-1, "a"); (0, "z"); (1, "B"); (1, "a"); (1, "b") |],
            SortedSet.toArray (SortedSet.ofArray pairs),
            "SortedSet.ofArray must order tuples in F# generic comparison"
        )

    [<TestMethod; DynamicData(nameof SortedSetTests.ElementCreations)>]
    member _.``ofArray, ofList, ofReadOnlySpan, ofSeq and ofSpan keep the first of the elements that compare equal, as Set does``
        (creation : SortedSetCreation)
        =
        // F# generic comparison considers 0.0 and -0.0 equal, while the sign tells them apart
        let input = [|
            for round in 0..1 do
                for index in 0..99 do
                    let key = index * 37 % 100
                    let negative = (key + round) % 2 = 0
                    (key, (if negative then -0.0 else 0.0))
        |]

        let signs (elements : (int * float) array) =
            elements
            |> Array.map (fun (key, value) -> key, Double.IsNegative value)

        CollectionAssert.AreEqual (
            signs (Set.toArray (Set.ofArray input)),
            signs (SortedSet.toArray (creation.Create input)),
            "The creation function must keep the first of the elements that compare equal, as Set does"
        )

    [<TestMethod>]
    member _.``foldBack visits the elements in descending order as Set.foldBack does`` () =
        let visitedBy (foldBack : (string -> unit -> unit) -> unit -> unit) =
            let visited = ResizeArray<string>()
            foldBack (fun item () -> visited.Add item) ()
            visited.ToArray ()

        let expected = visitedBy (fun folder state -> Set.foldBack folder (Set.ofArray words) state)
        let actual = visitedBy (fun folder state -> SortedSet.foldBack folder (SortedSet.ofArray words) state)

        CollectionAssert.AreEqual (expected, actual, "SortedSet.foldBack must visit the elements as Set.foldBack does")

        CollectionAssert.AreEqual (
            [| "é"; "z"; "e"; "b"; "a"; "Z"; "B"; "A" |],
            actual,
            "SortedSet.foldBack must visit the elements in descending order"
        )

    [<TestMethod>]
    member _.``partitionWith keeps the same one of two equal values as Set.partitionWith`` () =
        // F# generic comparison considers 0.0 and -0.0 equal. The smaller element of each pair of choices gives -0.0, and
        // Set.partitionWith keeps the value of the larger one, because it visits the elements from the largest one
        let partitioner (item : int) =
            let value = if item < 3 then -0.0 else 0.0
            if item % 2 = 0 then Choice1Of2 value else Choice2Of2 value

        let input = [| 1; 2; 3; 4 |]
        let expectedFirst, expectedSecond = Set.partitionWith partitioner (Set.ofArray input)
        let struct (first, second) = SortedSet.partitionWith partitioner (SortedSet.ofArray input)

        Assert.AreEqual<bool>(
            Double.IsNegative (Set.minElement expectedFirst),
            Double.IsNegative (SortedSet.minElement first),
            "SortedSet.partitionWith must keep the first choice that Set.partitionWith keeps"
        )

        Assert.AreEqual<bool>(
            Double.IsNegative (Set.minElement expectedSecond),
            Double.IsNegative (SortedSet.minElement second),
            "SortedSet.partitionWith must keep the second choice that Set.partitionWith keeps"
        )

    [<TestMethod>]
    member _.``rev returns the elements in descending order`` () =
        CollectionAssert.AreEqual (
            Array.rev (Set.toArray (Set.ofArray numbers)),
            Seq.toArray (SortedSet.rev (SortedSet.ofArray numbers)),
            "SortedSet.rev must return the elements from the largest one"
        )

        CollectionAssert.AreEqual (
            [| "cherry"; "Banana"; "apple" |],
            Seq.toArray (SortedSet.rev (fruits ())),
            "SortedSet.rev must follow the comparer of the sorted set"
        )

        Assert.IsEmpty (SortedSet.rev SortedSet.empty<int>, "SortedSet.rev of an empty sorted set must be empty")

    // ---- Comparers ----

    [<TestMethod; DynamicData(nameof SortedSetTests.DefaultCreations)>]
    member _.``creation without a comparer compares with F# generic comparison`` (creation : SortedSetCreation) =
        Assert.AreSame (
            defaultComparer,
            SortedSet.comparer (creation.Create [| "a" |]),
            "The creation function must compare with F# generic comparison"
        )

    [<TestMethod; DynamicData(nameof SortedSetTests.Creations)>]
    member _.``the sorted sets of one element type share one comparer`` (creation : SortedSetCreation) =
        // FastGenericComparer creates a new comparer for each call at a concrete type that is not primitive, which an
        // optimized build makes of an inlined function; the module resolves the comparer once per type instead
        Assert.AreSame (
            SortedSet.comparer SortedSet.empty<int * string>,
            SortedSet.comparer (creation.Create pairs),
            "The creation function must compare with the comparer of the type"
        )

    [<TestMethod>]
    member _.``forComparer creates the struct with the comparer`` () =
        Assert.AreSame (ignoreCase, (SortedSet.forComparer ignoreCase).Comparer, "SortedSet.forComparer must keep the comparer")

        Assert.AreSame (
            ignoreCase,
            (CustomComparerSortedSet ignoreCase).Comparer,
            "The constructor of CustomComparerSortedSet must keep the comparer"
        )

    [<TestMethod>]
    member _.``forComparer orders an element type without F# comparison`` () =
        // Functions do not support F# comparison, so only a comparer can order them: here by their results for 0
        let byResult =
            ComparisonIdentity.FromFunction (fun (left : int -> int) right -> compare (left 0) (right 0))

        let set =
            (SortedSet.forComparer byResult).ofArray [| (fun x -> x + 2); (fun x -> x + 1); (fun x -> x + 3) |]

        CollectionAssert.AreEqual (
            [| 1; 2; 3 |],
            SortedSet.toArray set |> Array.map (fun item -> item 0),
            "CustomComparerSortedSet.ofArray must order the elements with the comparer"
        )

    [<TestMethod; DynamicData(nameof SortedSetTests.StructCreations)>]
    member _.``the members of the struct create sorted sets that order with its comparer`` (name : string) =
        let set = structCreations[name]()

        Assert.AreSame (ignoreCase, SortedSet.comparer set, $"CustomComparerSortedSet.%s{name} must keep the comparer")

        CollectionAssert.AreEqual (
            [| "apple"; "Banana"; "cherry" |],
            SortedSet.toArray set,
            $"CustomComparerSortedSet.%s{name} must order with the comparer and keep the first of the elements equal in it"
        )

    [<TestMethod; DynamicData(nameof SortedSetTests.ByKeyCreations)>]
    member _.``the members of the struct keep the first of many elements that compare equal`` (name : string) =
        // An unstable sort, as in ImmutableSortedSet.CreateRange, would keep a later element for some of the keys
        let input = [|
            for round in [| "first"; "second"; "third" |] do
                for index in 0..99 do
                    (index * 37 % 100, round)
        |]

        CollectionAssert.AreEqual (
            [| for key in 0..99 -> (key, "first") |],
            SortedSet.toArray (byKeyCreations[name] input),
            $"CustomComparerSortedSet.%s{name} must keep the first of the elements that compare equal"
        )

    [<TestMethod>]
    member _.``the struct gives an existing sorted set its comparer`` () =
        let set = SortedSet.ofArray [| "apple"; "APPLE"; "Banana" |]
        let ignoringCase = caseless.ofSeq set

        Assert.AreSame (
            ignoreCase,
            SortedSet.comparer ignoringCase,
            "CustomComparerSortedSet.ofSeq must give the sorted set the comparer"
        )

        // The sorted set enumerates "APPLE" before "apple", so the new one keeps "APPLE"
        CollectionAssert.AreEqual (
            [| "APPLE"; "Banana" |],
            SortedSet.toArray ignoringCase,
            "CustomComparerSortedSet.ofSeq must keep the first of the elements that the comparer considers equal"
        )

        Assert.HasCount (3, set, "CustomComparerSortedSet.ofSeq must not change the sorted set")

    // The BCL tests the instance of the comparer, not its behaviour, so two equal comparers are two comparers (D14)
    [<TestMethod>]
    member _.``the struct returns a sorted set only for the same comparer instance`` () =
        let first : IComparer<string> = StringComparer.Create (CultureInfo.InvariantCulture, true)
        let second : IComparer<string> = StringComparer.Create (CultureInfo.InvariantCulture, true)
        Assert.AreNotSame (first, second, "The test needs two comparer instances")
        Assert.AreEqual<obj>(first, second, "The test needs two comparers that are equal")

        let set = (SortedSet.forComparer first).ofArray [| "apple"; "Banana" |]

        Assert.AreSame (
            set,
            (SortedSet.forComparer first).ofSeq set,
            "CustomComparerSortedSet.ofSeq must return a sorted set that has the same comparer instance"
        )

        let rebuilt = (SortedSet.forComparer second).ofSeq set

        Assert.AreNotSame (
            set,
            rebuilt,
            "CustomComparerSortedSet.ofSeq must rebuild a sorted set whose comparer is another instance"
        )

        Assert.AreSame (
            second,
            SortedSet.comparer rebuilt,
            "CustomComparerSortedSet.ofSeq must give the rebuilt sorted set its comparer"
        )

        CollectionAssert.AreEqual (
            [| "apple"; "Banana" |],
            SortedSet.toArray rebuilt,
            "CustomComparerSortedSet.ofSeq must keep the elements"
        )

    [<TestMethod>]
    member _.``ofSeq gives a sorted set with another comparer F# generic comparison`` () =
        let copy = SortedSet.ofSeq (fruits ())

        Assert.AreSame (defaultComparer, SortedSet.comparer copy, "SortedSet.ofSeq must compare with F# generic comparison")

        CollectionAssert.AreEqual (
            [| "Banana"; "apple"; "cherry" |],
            SortedSet.toArray copy,
            "SortedSet.ofSeq must order the elements of a sorted set with another comparer ordinally"
        )

    [<TestMethod>]
    member _.``the default value of the struct orders with the default comparer of the BCL`` () =
        // The struct cannot require F# comparison, so its default value falls back as the BCL does for a null comparer
        let defaultValue = Unchecked.defaultof<CustomComparerSortedSet<int>>
        let bclDefault = Comparer<int>.Default :> IComparer<int>
        let set = defaultValue.ofArray [| 3; 1; 2; 1 |]

        Assert.AreSame (bclDefault, defaultValue.Comparer, "The default CustomComparerSortedSet must return Comparer.Default")
        Assert.AreSame (bclDefault, SortedSet.comparer set, "The default CustomComparerSortedSet must create sorted sets with it")

        Assert.AreSame (
            bclDefault,
            SortedSet.comparer defaultValue.empty,
            "The default CustomComparerSortedSet must create empty sorted sets with it"
        )

        CollectionAssert.AreEqual (
            [| 1; 2; 3 |],
            SortedSet.toArray set,
            "The default CustomComparerSortedSet must order with Comparer.Default"
        )

    [<TestMethod; DynamicData(nameof SortedSetTests.Changes)>]
    member _.``add, addRange, clear, remove, removeRange and update keep the comparer`` (name : string) =
        Assert.AreSame (ignoreCase, SortedSet.comparer (changes[name](fruits ())), $"SortedSet.%s{name} must keep the comparer")

    [<TestMethod>]
    member _.``addRange adds the elements that are not equal in the comparer to an element of the sorted set`` () =
        // "APPLE" equals "apple" without case, so the sorted set keeps "apple" (ADR 0001, D14)
        CollectionAssert.AreEqual (
            [| "apple"; "Banana"; "cherry"; "date" |],
            SortedSet.toArray (SortedSet.addRange [| "APPLE"; "date" |] (fruits ())),
            "SortedSet.addRange must compare with the comparer of the sorted set"
        )

    // ImmutableSortedSet.Union refills the tree from an unstable sort when the sequence is large against the set, and then
    // keeps an arbitrary one of the elements that compare equal (measured: 4 of 25 elements of the set kept for 250 added
    // elements). The builder adds the elements one by one, so the element of the set stays whatever the size of the
    // sequence.
    [<TestMethod; DataRow 1; DataRow 2; DataRow 10; DataRow 40>]
    member _.``addRange keeps the elements of the sorted set whatever the size of the sequence`` (spellings : int) =
        let expected = [| for key in 0..24 -> (key, "set") |]
        let set = byKey.ofArray expected

        let added = [|
            for round in 0 .. spellings - 1 do
                for key in 0..24 do
                    (key, $"added%d{round}")
        |]

        CollectionAssert.AreEqual (
            expected,
            SortedSet.toArray (SortedSet.addRange added set),
            "SortedSet.addRange must keep the element of the sorted set for every key"
        )

    // Union adds the smaller sorted set to the larger one when both have the same comparer, so the element of the larger one
    // stays (ADR 0001, D14); addRange keeps the element of the sorted set that receives the elements
    [<TestMethod>]
    member _.``addRange keeps the elements of the sorted set when the sequence is a larger sorted set with its comparer`` () =
        let set = caseless.ofArray [| "apple" |]
        let larger = caseless.ofArray [| "APPLE"; "banana" |]

        CollectionAssert.AreEqual (
            [| "apple"; "banana" |],
            SortedSet.toArray (SortedSet.addRange larger set),
            "SortedSet.addRange must keep the element of the sorted set that receives the elements"
        )

        CollectionAssert.AreEqual (
            [| "APPLE"; "banana" |],
            SortedSet.toArray (SortedSet.union set larger),
            "SortedSet.union must keep the element of the larger sorted set when both have the same comparer"
        )

    [<TestMethod>]
    member _.``addRange to an empty sorted set keeps its comparer for a sorted set with another comparer`` () =
        // ImmutableSortedSet.Union answers an empty sorted set with the sorted set of its argument when the comparers are the
        // same; with another comparer the result must still order with the comparer of the empty sorted set
        let result = SortedSet.addRange (SortedSet.ofArray [| "apple"; "APPLE" |]) caseless.empty

        Assert.AreSame (ignoreCase, SortedSet.comparer result, "SortedSet.addRange must keep the comparer of the sorted set")
        Assert.HasCount (1, result, "SortedSet.addRange must compare with the comparer of the sorted set")

    [<TestMethod>]
    member _.``remove removes the element that equals the value in the comparer`` () =
        CollectionAssert.AreEqual (
            [| "Banana"; "cherry" |],
            SortedSet.toArray (SortedSet.remove "APPLE" (fruits ())),
            "SortedSet.remove must remove the element that equals the value in the comparer"
        )

    [<TestMethod>]
    member _.``removeRange removes the elements that are equal in the comparer to an element of the sequence`` () =
        CollectionAssert.AreEqual (
            [| "Banana"; "cherry" |],
            SortedSet.toArray (SortedSet.removeRange [| "APPLE"; "date" |] (fruits ())),
            "SortedSet.removeRange must compare with the comparer of the sorted set"
        )

    [<TestMethod; DynamicData(nameof SortedSetTests.Selections)>]
    member _.``filter and partition keep the comparer`` (name : string) =
        let struct (select, expected) = selections[name]
        let result = select (fruits ())

        Assert.AreSame (ignoreCase, SortedSet.comparer result, $"SortedSet.%s{name} must keep the comparer")
        CollectionAssert.AreEqual (expected, SortedSet.toArray result, $"SortedSet.%s{name} must order with the comparer")

    [<TestMethod>]
    member _.``partition keeps comparing without case`` () =
        Assert.IsTrue (
            SortedSet.contains "BANANA" (longFruits (fruits ())),
            "SortedSet.partition must keep comparing without case"
        )

    [<TestMethod; DynamicData(nameof SortedSetTests.Algebra)>]
    member _.``set algebra keeps the comparer of the first set and compares with it`` (name : string) =
        let struct (combine, expected) = algebra[name]
        let result = combine (fruits ()) (SortedSet.ofArray [| "APPLE"; "date" |])

        Assert.AreSame (ignoreCase, SortedSet.comparer result, $"SortedSet.%s{name} must keep the comparer of the first set")

        CollectionAssert.AreEqual (
            expected,
            SortedSet.toArray result,
            $"SortedSet.%s{name} must compare and order with the comparer of the first set"
        )

    [<TestMethod>]
    member _.``isProperSuperset counts the elements that the comparer of the first set finds equal once`` () =
        // The second set tells "apple" and "APPLE" apart; for the first set they are one element, and it holds "Banana"
        // besides, as the documentation of IsProperSupersetOf says (ADR 0001, D14)
        Assert.IsTrue (
            SortedSet.isProperSuperset (caseless.ofArray [| "apple"; "Banana" |]) (SortedSet.ofArray [| "apple"; "APPLE" |]),
            "SortedSet.isProperSuperset must count the elements that the comparer of the first set finds equal once"
        )

    [<TestMethod>]
    member _.``the predicates of two sets compare with the comparer of the first set`` () =
        let apple = caseless.ofArray [| "APPLE" |]
        let set = fruits ()

        Assert.IsTrue (SortedSet.isSubset apple set, "SortedSet.isSubset must compare without case")
        Assert.IsTrue (SortedSet.isProperSubset apple set, "SortedSet.isProperSubset must compare without case")
        Assert.IsTrue (SortedSet.isSuperset set apple, "SortedSet.isSuperset must compare without case")
        Assert.IsTrue (SortedSet.isProperSuperset set apple, "SortedSet.isProperSuperset must compare without case")
        Assert.IsTrue (SortedSet.overlaps apple set, "SortedSet.overlaps must compare without case")

        Assert.IsTrue (
            SortedSet.setEquals set (SortedSet.ofArray [| "APPLE"; "BANANA"; "CHERRY" |]),
            "SortedSet.setEquals must compare without case"
        )

        Assert.IsFalse (
            SortedSet.isSubset (SortedSet.ofArray [| "APPLE" |]) set,
            "SortedSet.isSubset must compare with the default comparer of the first set"
        )

    [<TestMethod>]
    member _.``intersectMany and unionMany keep the comparer of the first set`` () =
        let sets = [| fruits (); SortedSet.ofArray [| "APPLE"; "date" |] |]
        let intersection = SortedSet.intersectMany sets
        let union = SortedSet.unionMany sets

        Assert.AreSame (
            ignoreCase,
            SortedSet.comparer intersection,
            "SortedSet.intersectMany must keep the comparer of the first set"
        )

        Assert.AreSame (ignoreCase, SortedSet.comparer union, "SortedSet.unionMany must keep the comparer of the first set")

        CollectionAssert.AreEqual (
            [| "APPLE" |],
            SortedSet.toArray intersection,
            "SortedSet.intersectMany must compare without case"
        )

        CollectionAssert.AreEqual (
            [| "apple"; "Banana"; "cherry"; "date" |],
            SortedSet.toArray union,
            "SortedSet.unionMany must compare without case"
        )

    [<TestMethod>]
    member _.``unionMany of no sets is empty with F# generic comparison`` () =
        let union = SortedSet.unionMany Seq.empty<SortedSet<string>>
        Assert.IsEmpty (union, "SortedSet.unionMany of no sets must be empty")

        Assert.AreSame (
            defaultComparer,
            SortedSet.comparer union,
            "SortedSet.unionMany of no sets must compare with F# generic comparison"
        )

    [<TestMethod; DynamicData(nameof SortedSetTests.TypeChanges)>]
    member _.``map, choose and partitionWith compare with F# generic comparison`` (name : string) =
        let struct (transform, expected) = typeChanges[name]
        let result = transform (fruits ())

        Assert.AreSame (defaultComparer, SortedSet.comparer result, $"SortedSet.%s{name} must compare with F# generic comparison")
        CollectionAssert.AreEqual (expected, SortedSet.toArray result, $"SortedSet.%s{name} must order ordinally")

    [<TestMethod>]
    member _.``map does not keep the comparer of its input`` () =
        // The default comparer tells the case apart, although the element type is the same
        Assert.HasCount (
            4,
            SortedSet.add "APPLE" (SortedSet.map id (fruits ())),
            "SortedSet.map must not keep the comparer of its input"
        )

    [<TestMethod>]
    member _.``map to another type orders the results as Set.map does`` () =
        // The strings of the pairs differ in case, so the culture would order them differently
        CollectionAssert.AreEqual (
            Set.toArray (Set.map snd (Set.ofArray pairs)),
            SortedSet.toArray (SortedSet.map snd (SortedSet.ofArray pairs)),
            "SortedSet.map must order the results as Set.map does"
        )

    // ---- Sharing ----

    [<TestMethod; DynamicData(nameof SetInputs.All, typeof<SetInputs>)>]
    member _.``ofSeq of a sorted set with the comparer returns it`` (input : int array) =
        let set = SortedSet.ofArray input
        Assert.AreSame (set, SortedSet.ofSeq set, "SortedSet.ofSeq of a sorted set with the default comparer must return it")

        let downward = SortedSet.forComparer descending
        let ordered = downward.ofArray input

        Assert.AreSame (
            ordered,
            downward.ofSeq ordered,
            "CustomComparerSortedSet.ofSeq of a sorted set with the comparer must return it"
        )

    [<TestMethod>]
    member _.``add returns the input when it contains an equal element`` () =
        let set = fruits ()

        Assert.AreSame (set, SortedSet.add "APPLE" set, "SortedSet.add of an element equal in the comparer must return the input")

    [<TestMethod>]
    member _.``remove returns the input when it has no equal element`` () =
        let set = fruits ()
        Assert.AreSame (set, SortedSet.remove "date" set, "SortedSet.remove of a missing element must return the input")

    [<TestMethod>]
    member _.``clear returns an empty input`` () =
        let set = caseless.empty
        Assert.AreSame (set, SortedSet.clear set, "SortedSet.clear of an empty sorted set must return the input")

    // ---- Access ----

    [<TestMethod>]
    member _.``tryGetValue returns the element that the sorted set holds`` () =
        let stored = String ('x', 3)
        let set = caseless.ofArray [| stored; "apple" |]

        match SortedSet.tryGetValue "XXX" set with
        | ValueSome found -> Assert.AreSame (stored, found, "SortedSet.tryGetValue must return the instance of the sorted set")
        | ValueNone -> Assert.Fail "SortedSet.tryGetValue must find the element that equals the value in the comparer"

        Assert.AreEqual<string voption>(
            ValueSome "apple",
            SortedSet.tryGetValue "APPLE" set,
            "SortedSet.tryGetValue must compare without case"
        )

        Assert.AreEqual<string voption>(
            ValueNone,
            SortedSet.tryGetValue "date" set,
            "SortedSet.tryGetValue must return nothing for a missing value"
        )

    [<TestMethod>]
    member _.``contains compares with the comparer of the sorted set`` () =
        Assert.IsTrue (SortedSet.contains "BANANA" (fruits ()), "SortedSet.contains must compare without case")

        Assert.IsFalse (
            SortedSet.contains "BANANA" (SortedSet.ofArray [| "Banana" |]),
            "SortedSet.contains must compare with F# generic comparison"
        )

    [<TestMethod>]
    member _.``item returns the elements in ascending order`` () =
        let set = SortedSet.ofArray numbers

        CollectionAssert.AreEqual (
            Set.toArray (Set.ofArray numbers),
            Array.init (SortedSet.count set) (fun index -> SortedSet.item index set),
            "SortedSet.item must return the elements in ascending order"
        )

    [<TestMethod; DynamicData(nameof SortedSetTests.OutOfRangeIndices)>]
    member _.``item throws ArgumentOutOfRangeException for an index out of range`` (index : int) =
        let set = SortedSet.ofArray numbers

        let ex =
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(
                (fun () -> SortedSet.item index set |> ignore),
                "SortedSet.item must throw what the indexer throws"
            )

        Assert.AreEqual<string | null>("index", ex.ParamName, "SortedSet.item must name the index")

    [<TestMethod; DynamicData(nameof SortedSetTests.TryItemCases)>]
    member _.``tryItem returns the element at the index or nothing`` (index : int, expected : int voption) =
        Assert.AreEqual<int voption>(
            expected,
            SortedSet.tryItem index (SortedSet.ofArray numbers),
            "SortedSet.tryItem must return the element at the index or nothing"
        )

    [<TestMethod>]
    member _.``tryItem of an empty sorted set returns nothing`` () =
        Assert.AreEqual<int voption>(
            ValueNone,
            SortedSet.tryItem 0 SortedSet.empty<int>,
            "SortedSet.tryItem of an empty sorted set must return nothing"
        )

    // The negative results are the bitwise complements of the index of the next larger element: ~~~0, ~~~2 and ~~~3
    [<TestMethod; DataRow(10, 0); DataRow(20, 1); DataRow(30, 2); DataRow(5, -1); DataRow(25, -3); DataRow(35, -4)>]
    member _.``indexOf returns the index or the complement of the index of the next larger element``
        (value : int, expected : int)
        =
        Assert.AreEqual<int>(
            expected,
            SortedSet.indexOf value (SortedSet.ofArray [| 30; 10; 20 |]),
            "SortedSet.indexOf must return the index or the complement of the index of the next larger element"
        )

    [<TestMethod>]
    member _.``indexOf compares with the comparer of the sorted set`` () =
        Assert.AreEqual<int>(1, SortedSet.indexOf "BANANA" (fruits ()), "SortedSet.indexOf must compare with the comparer")

    [<TestMethod>]
    member _.``minElement and maxElement return the smallest and the largest element as Set does`` () =
        Assert.AreEqual<int>(
            Set.minElement (Set.ofArray numbers),
            SortedSet.minElement (SortedSet.ofArray numbers),
            "SortedSet.minElement of numbers"
        )

        Assert.AreEqual<int>(
            Set.maxElement (Set.ofArray numbers),
            SortedSet.maxElement (SortedSet.ofArray numbers),
            "SortedSet.maxElement of numbers"
        )

        Assert.AreEqual<string>(
            Set.minElement (Set.ofArray words),
            SortedSet.minElement (SortedSet.ofArray words),
            "SortedSet.minElement of strings"
        )

        Assert.AreEqual<string>(
            Set.maxElement (Set.ofArray words),
            SortedSet.maxElement (SortedSet.ofArray words),
            "SortedSet.maxElement of strings"
        )

        Assert.AreEqual<int * string>(
            Set.maxElement (Set.ofArray pairs),
            SortedSet.maxElement (SortedSet.ofArray pairs),
            "SortedSet.maxElement of tuples"
        )

        Assert.AreEqual<int>(42, SortedSet.minElement (SortedSet.singleton 42), "SortedSet.minElement of one element")
        Assert.AreEqual<int>(42, SortedSet.maxElement (SortedSet.singleton 42), "SortedSet.maxElement of one element")

    [<TestMethod>]
    member _.``minElement and maxElement follow the comparer of the sorted set`` () =
        let set = (SortedSet.forComparer descending).ofArray numbers
        Assert.AreEqual<int>(12, SortedSet.minElement set, "SortedSet.minElement must return the first element of the comparer")
        Assert.AreEqual<int>(-10, SortedSet.maxElement set, "SortedSet.maxElement must return the last element of the comparer")

    [<TestMethod>]
    member _.``minElement throws for an empty sorted set as Set does`` () =
        ExceptionAssert.Same (
            SortedSetOps.paramName,
            (fun () -> Set.minElement Set.empty<int> |> ignore),
            (fun () -> SortedSet.minElement SortedSet.empty<int> |> ignore),
            "SortedSet.minElement of an empty sorted set must throw"
        )

    [<TestMethod>]
    member _.``maxElement throws for an empty sorted set as Set does`` () =
        ExceptionAssert.Same (
            SortedSetOps.paramName,
            (fun () -> Set.maxElement Set.empty<int> |> ignore),
            (fun () -> SortedSet.maxElement SortedSet.empty<int> |> ignore),
            "SortedSet.maxElement of an empty sorted set must throw"
        )

    // ---- Null inputs ----

    [<TestMethod; DynamicData(nameof SortedSetTests.NullRejections)>]
    member _.``functions reject a null sorted set`` (name : string) =
        let struct (paramName, actual) = nullRejections[name]

        let ex =
            Assert.ThrowsExactly<ArgumentNullException>(Action actual, $"SortedSet.%s{name} must reject a null sorted set")

        Assert.AreEqual<string | null>(paramName, ex.ParamName, $"SortedSet.%s{name} must name the parameter of the sorted set")

    [<TestMethod; DynamicData(nameof SortedSetTests.SequenceRejections)>]
    member _.``functions that take a sequence reject a null sequence and a default flat list given as the sequence``
        (name : string)
        =
        let struct (paramName, actual) = sequenceRejections[name]
        FlatListAssert.RejectsNull ($"SortedSet.%s{name}", paramName, actual)

    [<TestMethod>]
    member _.``a flat list of strings that is not default passes as a sequence of objects`` () =
        let widened = FlatListOracles.stringsAsObjects (FlatList.ofArray [| "a"; "b" |])
        let objectComparer = SortedSet.forComparer (Comparer<obj>.Default :> IComparer<obj>)
        let objects = objectComparer.ofArray [| ("b" :> obj); ("c" :> obj) |]

        CollectionAssert.AreEqual (
            [| ("a" :> obj); ("b" :> obj) |],
            SortedSet.toArray (objectComparer.ofSeq widened),
            "CustomComparerSortedSet.ofSeq must take the elements of a flat list of strings as objects"
        )

        CollectionAssert.AreEqual (
            [| ("a" :> obj); ("b" :> obj); ("c" :> obj) |],
            SortedSet.toArray (SortedSet.addRange widened objects),
            "SortedSet.addRange must take the elements of a flat list of strings as objects"
        )

        CollectionAssert.AreEqual (
            [| ("c" :> obj) |],
            SortedSet.toArray (SortedSet.removeRange widened objects),
            "SortedSet.removeRange must take the elements of a flat list of strings as objects"
        )

    // The members of the struct reject null inputs in the creation contract
    [<TestMethod>]
    member _.``ofBuilder rejects a null builder`` () =
        let ex =
            Assert.ThrowsExactly<ArgumentNullException>(
                Action (fun () ->
                    SortedSet.ofBuilder (Unchecked.defaultof<SortedSet<int>.Builder>)
                    |> ignore
                ),
                "SortedSet.ofBuilder must reject a null builder"
            )

        Assert.AreEqual<string | null>("builder", ex.ParamName, "SortedSet.ofBuilder must name the parameter builder")

    // forComparer calls the constructor of the struct, which checks the comparer, so both name its parameter (D12)
    [<TestMethod>]
    member _.``forComparer rejects a null comparer`` () =
        let ex =
            Assert.ThrowsExactly<ArgumentNullException>(
                Action (fun () -> SortedSet.forComparer nullComparer |> ignore),
                "SortedSet.forComparer must reject a null comparer"
            )

        Assert.AreEqual<string | null>("comparer", ex.ParamName, "SortedSet.forComparer must name the parameter comparer")

    [<TestMethod>]
    member _.``the constructor of the struct rejects a null comparer`` () =
        let ex =
            Assert.ThrowsExactly<ArgumentNullException>(
                Action (fun () -> CustomComparerSortedSet nullComparer |> ignore),
                "The constructor of CustomComparerSortedSet must reject a null comparer"
            )

        Assert.AreEqual<string | null>(
            "comparer",
            ex.ParamName,
            "The constructor of CustomComparerSortedSet must name the parameter comparer"
        )
