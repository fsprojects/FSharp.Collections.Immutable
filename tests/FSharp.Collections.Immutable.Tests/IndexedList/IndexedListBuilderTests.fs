namespace FSharp.Collections.Immutable.Tests

open System
open System.Collections.Generic
open FSharp.Collections.Immutable
open Microsoft.VisualStudio.TestTools.UnitTesting

/// The builder functions of `IndexedList` and the `IndexedListBuilder` module.
[<TestClass; IndexedListTestCategory; BuilderTestCategory>]
type IndexedListBuilderTests () =

    /// A builder that holds the given elements.
    static let builderOf (elements : 'T array) =
        let builder = IndexedList.builder<'T>()
        builder.AddRange elements
        builder

    /// A builder that holds the elements of the input with several elements.
    static let filled () = builderOf Inputs.Several

    /// A builder that holds the strings of the removal, the replacement and the searches.
    static let named () = builderOf ListNames.Several

    /// An equality comparer that finds strings equal when the case is ignored.
    static let ignoringCaseEquality : IEqualityComparer<string> = StringComparer.OrdinalIgnoreCase

    /// The functions that search forward in a range of the strings, by name.
    static let forwardSearches : Map<string, int -> int -> string -> IndexedList<string>.Builder -> int> =
        Map.ofArray [|
            nameof IndexedListBuilder.indexOfRange, IndexedListBuilder.indexOfRange
            nameof IndexedListBuilder.indexOfRangeWith, IndexedListBuilder.indexOfRangeWith ignoringCaseEquality
        |]

    /// The functions that search backward in a range of the strings, by name.
    static let backwardSearches : Map<string, int -> int -> string -> IndexedList<string>.Builder -> int> =
        Map.ofArray [|
            nameof IndexedListBuilder.lastIndexOfRange, IndexedListBuilder.lastIndexOfRange
            nameof IndexedListBuilder.lastIndexOfRangeWith, IndexedListBuilder.lastIndexOfRangeWith ignoringCaseEquality
        |]

    /// The functions that sort a range, by name, with the function and the sort of the range in the oracle. The inputs
    /// have no two different elements with the same absolute value, and the comparer of `sortRangeWithComparer` orders by
    /// the absolute value, so the result does not depend on the stability of the sort; the stability has its own tests.
    static let rangeSorts
        : Map<string, struct ((int -> int -> IndexedList<int>.Builder -> IndexedList<int>.Builder) * (int array -> int array))> =
        Map.ofArray [|
            nameof IndexedListBuilder.sortRange, struct (IndexedListBuilder.sortRange, ListOracles.stableSort compare)
            (nameof IndexedListBuilder.sortRangeWith,
             struct (IndexedListBuilder.sortRangeWith FlatListOracles.descending,
                     ListOracles.stableSort FlatListOracles.descending))
            (nameof IndexedListBuilder.sortRangeWithComparer,
             struct (IndexedListBuilder.sortRangeWithComparer (Comparer<int>.Create(Comparison<int> FlatListOracles.byAbsolute)),
                     ListOracles.stableSort FlatListOracles.byAbsolute))
        |]

    /// The functions of the builder that take a sequence, by name and by the input that they reject, with the parameter that
    /// names the input and the function on a builder. A default flat list converts to a sequence without a check and fails
    /// when the sequence is enumerated, so the other inputs are valid. The flat list of strings is a sequence of objects,
    /// which a type test for the flat list of objects does not see.
    static let sequenceRejections : Map<string, struct (string * (unit -> unit))> =
        let nullValues = Unchecked.defaultof<seq<int>>
        let defaultValues = Inputs.DefaultFlatListAsSeq<int>()
        let widenedValues = FlatListOracles.defaultStringsAsObjects
        let objects () = IndexedList.builder<obj>()
        let comparer = EqualityComparer<int>.Default

        Map.ofArray [|
            "addRange (null sequence)",
            struct ("values", (fun () -> IndexedListBuilder.addRange nullValues (filled ()) |> ignore))
            "addRange (default flat list)",
            struct ("values",
                    (fun () ->
                        IndexedListBuilder.addRange defaultValues (filled ())
                        |> ignore
                    ))
            "addRange (default flat list of strings as objects)",
            struct ("values",
                    (fun () ->
                        IndexedListBuilder.addRange widenedValues (objects ())
                        |> ignore
                    ))
            "insertManyAt (null sequence)",
            struct ("values",
                    (fun () ->
                        IndexedListBuilder.insertManyAt 0 nullValues (filled ())
                        |> ignore
                    ))
            "insertManyAt (default flat list)",
            struct ("values",
                    (fun () ->
                        IndexedListBuilder.insertManyAt 0 defaultValues (filled ())
                        |> ignore
                    ))
            "insertManyAt (default flat list of strings as objects)",
            struct ("values",
                    (fun () ->
                        IndexedListBuilder.insertManyAt 0 widenedValues (objects ())
                        |> ignore
                    ))
            "removeMany (null sequence)",
            struct ("items",
                    (fun () ->
                        IndexedListBuilder.removeMany nullValues (filled ())
                        |> ignore
                    ))
            "removeMany (default flat list)",
            struct ("items",
                    (fun () ->
                        IndexedListBuilder.removeMany defaultValues (filled ())
                        |> ignore
                    ))
            "removeMany (default flat list of strings as objects)",
            struct ("items",
                    (fun () ->
                        IndexedListBuilder.removeMany widenedValues (objects ())
                        |> ignore
                    ))
            "removeManyWith (null sequence)",
            struct ("items",
                    (fun () ->
                        IndexedListBuilder.removeManyWith comparer nullValues (filled ())
                        |> ignore
                    ))
            "removeManyWith (default flat list)",
            struct ("items",
                    (fun () ->
                        IndexedListBuilder.removeManyWith comparer defaultValues (filled ())
                        |> ignore
                    ))
            "removeManyWith (default flat list of strings as objects)",
            struct ("items",
                    (fun () ->
                        IndexedListBuilder.removeManyWith EqualityComparer<obj>.Default widenedValues (objects ())
                        |> ignore
                    ))
        |]

    /// The functions that change a builder, by name, on a builder; each must return the builder.
    static let chainedFunctions : Map<string, IndexedList<int>.Builder -> IndexedList<int>.Builder> =
        Map.ofArray [|
            nameof IndexedListBuilder.insertManyAt, IndexedListBuilder.insertManyAt 2 [| 98; 99 |]
            nameof IndexedListBuilder.removeAll, IndexedListBuilder.removeAll (fun item -> item < 0)
            nameof IndexedListBuilder.removeMany, IndexedListBuilder.removeMany [| 5; 8 |]
            nameof IndexedListBuilder.removeManyAt, IndexedListBuilder.removeManyAt 1 2
            nameof IndexedListBuilder.removeManyWith, IndexedListBuilder.removeManyWith EqualityComparer<int>.Default [| 5; 8 |]
            nameof IndexedListBuilder.replace, IndexedListBuilder.replace 5 99
            nameof IndexedListBuilder.replaceWith, IndexedListBuilder.replaceWith EqualityComparer<int>.Default 5 99
            nameof IndexedListBuilder.revRange, IndexedListBuilder.revRange 1 4
            nameof IndexedListBuilder.sortRange, IndexedListBuilder.sortRange 1 4
            nameof IndexedListBuilder.sortRangeWith, IndexedListBuilder.sortRangeWith compare 1 4
            nameof IndexedListBuilder.sortRangeWithComparer, IndexedListBuilder.sortRangeWithComparer Comparer<int>.Default 1 4
        |]

    /// The names of the rows of the test of the sequences that the functions reject.
    static member SequenceRejections = sequenceRejections.Keys

    /// The names of the functions that change a builder, as the rows of the test of the result.
    static member ChainedFunctions = chainedFunctions.Keys

    /// The names of the functions that sort a range, as the rows of the tests that tell them apart.
    static member RangeSorts = rangeSorts.Keys

    /// The ranges that each function that sorts a range rejects.
    static member SortRangesOutOfRange = seq {
        for name in rangeSorts.Keys do
            for struct (index, count, fault) in ListMemberCases.RangeFaults do
                struct (name, index, count, fault)
    }

    /// The ranges that removeManyAt rejects, with the parameter that the BCL names: it names the index for an index
    /// beyond the end of the builder, and the count for a count that does not fit.
    static member RemovalRangesOutOfRange = seq {
        struct (-1, 0, "index")
        struct (0, -1, "count")
        struct (5, 4, "count")
        struct (8, 1, "count")
        struct (9, 0, "index")
        struct (Int32.MaxValue, 1, "index")
        struct (1, Int32.MaxValue, "count")
    }

    [<TestMethod>]
    member _.``build returns the elements that the action adds`` () =
        let list = IndexedList.build (fun builder -> builder.AddRange Inputs.Several)
        CollectionAssert.AreEqual (Inputs.Several, IndexedList.toArray list, "IndexedList.build must return the added elements")

    [<TestMethod>]
    member _.``builder starts empty`` () =
        let builder = IndexedList.builder<int>()
        Assert.AreEqual<int>(0, builder.Count, "IndexedList.builder must start without elements")

    [<TestMethod>]
    member _.``ofBuilder returns the contents and keeps the builder usable`` () =
        let builder = filled ()
        let list = IndexedList.ofBuilder builder
        builder.Add 99
        CollectionAssert.AreEqual (Inputs.Several, IndexedList.toArray list, "IndexedList.ofBuilder must not see later changes")
        Assert.AreEqual<int>(Inputs.Several.Length + 1, builder.Count, "IndexedList.ofBuilder must keep the builder usable")

    [<TestMethod>]
    member _.``ofBuilder throws for a null builder`` () =
        FlatListAssert.RejectsNull (
            "IndexedList.ofBuilder",
            "builder",
            (fun () ->
                IndexedList.ofBuilder (Unchecked.defaultof<IndexedList<int>.Builder>)
                |> ignore
            )
        )

    [<TestMethod>]
    member _.``toBuilder starts with the elements and leaves the indexed list unchanged`` () =
        let list = IndexedList.ofArray Inputs.Several
        let builder = IndexedList.toBuilder list
        builder.Add 99

        CollectionAssert.AreEqual (
            Array.append Inputs.Several [| 99 |],
            Seq.toArray builder,
            "IndexedList.toBuilder must hold the elements"
        )

        CollectionAssert.AreEqual (
            Inputs.Several,
            IndexedList.toArray list,
            "IndexedList.toBuilder must not change the indexed list"
        )

    [<TestMethod>]
    member _.``update applies the action to a builder over the elements`` () =
        let list = IndexedList.ofArray Inputs.Several
        let updated = IndexedList.update (fun builder -> builder.RemoveAt 0) list
        CollectionAssert.AreEqual (
            Array.tail Inputs.Several,
            IndexedList.toArray updated,
            "IndexedList.update must apply the action"
        )
        CollectionAssert.AreEqual (Inputs.Several, IndexedList.toArray list, "IndexedList.update must not change the input")

    [<TestMethod>]
    member _.``add, addRange and insertAt return the same builder`` () =
        let builder = IndexedList.builder<int>()

        let result =
            builder
            |> IndexedListBuilder.add 1
            |> IndexedListBuilder.addRange [| 2; 3 |]
            |> IndexedListBuilder.insertAt 0 0

        Assert.AreSame (builder, result, "The functions that change a builder must return it")
        CollectionAssert.AreEqual ([| 0; 1; 2; 3 |], Seq.toArray builder, "add, addRange and insertAt must change the builder")

    [<TestMethod>]
    member _.``length counts the elements`` () =
        Assert.AreEqual<int>(
            Inputs.Several.Length,
            IndexedListBuilder.length (filled ()),
            "IndexedListBuilder.length must count the elements"
        )

    [<TestMethod>]
    member _.``clear removes every element`` () =
        let builder = filled () |> IndexedListBuilder.clear
        Assert.AreEqual<int>(0, IndexedListBuilder.length builder, "IndexedListBuilder.clear must remove every element")

    [<TestMethod>]
    member _.``contains, indexOf and lastIndexOf search the builder`` () =
        let builder = filled ()
        Assert.IsTrue (IndexedListBuilder.contains 8 builder, "IndexedListBuilder.contains must find 8")
        Assert.AreEqual<int>(1, IndexedListBuilder.indexOf -3 builder, "IndexedListBuilder.indexOf must return the first index")
        Assert.AreEqual<int>(
            5,
            IndexedListBuilder.lastIndexOf -3 builder,
            "IndexedListBuilder.lastIndexOf must return the last index"
        )
        Assert.AreEqual<int>(-1, IndexedListBuilder.indexOf 1000 builder, "IndexedListBuilder.indexOf must return -1 when absent")

    [<TestMethod>]
    member _.``item and updateAt read and replace an element`` () =
        let builder = filled () |> IndexedListBuilder.updateAt 2 99
        Assert.AreEqual<int>(99, IndexedListBuilder.item 2 builder, "IndexedListBuilder.updateAt must replace the element")

    [<TestMethod>]
    member _.``item throws for an index out of range`` () =
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            Action (fun () -> IndexedListBuilder.item 8 (filled ()) |> ignore),
            "IndexedListBuilder.item must reject an index out of range as the builder does"
        )
        |> ignore

    [<TestMethod>]
    member _.``remove and removeAt remove elements`` () =
        let builder = filled ()
        Assert.IsTrue (IndexedListBuilder.remove -3 builder, "IndexedListBuilder.remove must report a removed element")
        Assert.IsFalse (IndexedListBuilder.remove 1000 builder, "IndexedListBuilder.remove must report a missing element")
        IndexedListBuilder.removeAt 0 builder |> ignore
        CollectionAssert.AreEqual ([| 8; 5; 0; -3; 12; 7 |], Seq.toArray builder, "remove and removeAt must remove the elements")

    [<TestMethod>]
    member _.``rev reverses the builder`` () =
        let builder = filled () |> IndexedListBuilder.rev
        CollectionAssert.AreEqual (
            Array.rev Inputs.Several,
            Seq.toArray builder,
            "IndexedListBuilder.rev must reverse the elements"
        )

    [<TestMethod>]
    member _.``sort, sortWith and sortWithComparer order the builder`` () =
        CollectionAssert.AreEqual (
            Array.sort Inputs.Several,
            Seq.toArray (filled () |> IndexedListBuilder.sort),
            "IndexedListBuilder.sort must sort ascending"
        )

        CollectionAssert.AreEqual (
            Array.sortDescending Inputs.Several,
            Seq.toArray (
                filled ()
                |> IndexedListBuilder.sortWith (fun left right -> compare right left)
            ),
            "IndexedListBuilder.sortWith must use the comparison function"
        )

        CollectionAssert.AreEqual (
            Array.sortDescending Inputs.Several,
            Seq.toArray (
                filled ()
                |> IndexedListBuilder.sortWithComparer (Comparer<int>.Create (fun left right -> compare right left))
            ),
            "IndexedListBuilder.sortWithComparer must use the comparer"
        )

    [<TestMethod>]
    member _.``the sorts of the builder are stable`` () =
        // The sorts of Array are not stable, so the expectations come from the stable sorts of List
        let key (struct (key : int, _ : char)) = key
        let compareKeys left right = compare (key left) (key right)

        let builder () =
            let builder = IndexedList.builder<struct (int * char)>()
            builder.AddRange Inputs.ManyPairs
            builder

        CollectionAssert.AreEqual (
            List.sortWith compareKeys (List.ofArray Inputs.ManyPairs)
            |> List.toArray,
            Seq.toArray (builder () |> IndexedListBuilder.sortWith compareKeys),
            "IndexedListBuilder.sortWith must keep the order of equal elements, as List.sortWith does"
        )

        CollectionAssert.AreEqual (
            List.sortWith compareKeys (List.ofArray Inputs.ManyPairs)
            |> List.toArray,
            Seq.toArray (
                builder ()
                |> IndexedListBuilder.sortWithComparer (
                    Comparer<struct (int * char)>.Create (fun left right -> compareKeys left right)
                )
            ),
            "IndexedListBuilder.sortWithComparer must keep the order of equal elements, as List.sortWith does"
        )

        let keyed () = builderOf (KeyedItem.OfManyPairs ())

        CollectionAssert.AreEqual (
            List.sort (List.ofSeq (keyed ())) |> KeyedItem.KeysAndTags,
            keyed () |> IndexedListBuilder.sort |> KeyedItem.KeysAndTags,
            "IndexedListBuilder.sort must keep the order of equal elements, as List.sort does"
        )

    [<TestMethod>]
    member _.``toArray copies the elements`` () =
        CollectionAssert.AreEqual (
            Inputs.Several,
            IndexedListBuilder.toArray (filled ()),
            "IndexedListBuilder.toArray must copy the elements"
        )

    // ---- the functions that change a builder return it ----

    [<TestMethod; DynamicData(nameof IndexedListBuilderTests.ChainedFunctions)>]
    member _.``the functions that change a builder return it`` (name : string) =
        let builder = filled ()

        Assert.AreSame (
            builder,
            chainedFunctions[name] builder,
            $"IndexedListBuilder.%s{name} must return the builder, so that the calls chain"
        )

    [<TestMethod; DynamicData(nameof IndexedListBuilderTests.SequenceRejections)>]
    member _.``functions that take a sequence reject a null sequence, a default flat list and a default flat list of strings``
        (name : string)
        =
        let struct (paramName, action) = sequenceRejections[name]
        FlatListAssert.RejectsNull ($"IndexedListBuilder.%s{name}", paramName, action)

    [<TestMethod;
      DataRow(-1, "null sequence");
      DataRow(9, "null sequence");
      DataRow(-1, "default flat list");
      DataRow(9, "default flat list")>]
    member _.``insertManyAt rejects an index out of range before the values as the BCL does`` (index : int, kind : string) =
        // The builder has 8 elements, so the indices -1 and 9 are out of range. The member checks the index before it reads
        // the values, and the default flat list is as good as a null sequence for it
        let values =
            if kind = "null sequence" then
                Unchecked.defaultof<seq<int>>
            else
                Inputs.DefaultFlatListAsSeq<int>()

        let ex =
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(
                Action (fun () ->
                    IndexedListBuilder.insertManyAt index values (filled ())
                    |> ignore
                ),
                $"IndexedListBuilder.insertManyAt must check the index before a %s{kind}"
            )

        Assert.AreEqual<string | null>("index", ex.ParamName, "IndexedListBuilder.insertManyAt must name the index")

    [<TestMethod>]
    member _.``a flat list of strings that is not default passes as a sequence of objects`` () =
        let widened = FlatListOracles.stringsAsObjects (FlatList.ofArray [| "a"; "b" |])
        let builder = IndexedList.builder<obj>()

        // Add the elements, insert them again after the first one, then remove one element for each of them
        builder
        |> IndexedListBuilder.addRange widened
        |> IndexedListBuilder.insertManyAt 1 widened
        |> IndexedListBuilder.removeMany widened
        |> ignore

        CollectionAssert.AreEqual (
            [| "a" :> obj; "b" :> obj |],
            Seq.toArray builder,
            "addRange, insertManyAt and removeMany must take the elements of a flat list of strings as objects"
        )

        IndexedListBuilder.removeManyWith EqualityComparer<obj>.Default widened builder
        |> ignore

        Assert.IsEmpty (Seq.toArray builder, "removeManyWith must take the elements of a flat list of strings as objects")

    // ---- insertManyAt, removeManyAt and removeAll ----

    [<TestMethod; DataRow 0; DataRow 3; DataRow 8>]
    member _.``insertManyAt inserts the values at the index`` (index : int) =
        CollectionAssert.AreEqual (
            Array.insertManyAt index [| 98; 99 |] Inputs.Several,
            Seq.toArray (
                filled ()
                |> IndexedListBuilder.insertManyAt index [| 98; 99 |]
            ),
            "IndexedListBuilder.insertManyAt must insert the values at the index"
        )

    [<TestMethod; DataRow -1; DataRow 9>]
    member _.``insertManyAt throws for an index out of range`` (index : int) =
        let ex =
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(
                Action (fun () ->
                    IndexedListBuilder.insertManyAt index [| 98 |] (filled ())
                    |> ignore
                ),
                "IndexedListBuilder.insertManyAt must reject an index out of range as the builder does"
            )

        Assert.AreEqual<string | null>("index", ex.ParamName, "IndexedListBuilder.insertManyAt must name the index")

    [<TestMethod; DynamicData(nameof FlatListTests.SubRanges, typeof<FlatListTests>)>]
    member _.``removeManyAt removes the elements of the range`` (input : int array, index : int, count : int) =
        CollectionAssert.AreEqual (
            ListOracles.changeRange (fun _ -> [||]) input index count,
            Seq.toArray (
                builderOf input
                |> IndexedListBuilder.removeManyAt index count
            ),
            "IndexedListBuilder.removeManyAt must remove the elements of the range"
        )

    [<TestMethod; DynamicData(nameof IndexedListBuilderTests.RemovalRangesOutOfRange)>]
    member _.``removeManyAt throws for a range out of range as the builder does`` (index : int, count : int, paramName : string) =
        let ex =
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(
                Action (fun () ->
                    IndexedListBuilder.removeManyAt index count (filled ())
                    |> ignore
                ),
                "IndexedListBuilder.removeManyAt must reject a range out of range as the builder does"
            )

        Assert.AreEqual<string | null>(
            paramName,
            ex.ParamName,
            "IndexedListBuilder.removeManyAt must name the parameter that the BCL names"
        )

    [<TestMethod; DynamicData(nameof Inputs.All, typeof<Inputs>)>]
    member _.``removeAll removes the elements that satisfy the predicate`` (input : int array) =
        CollectionAssert.AreEqual (
            Array.filter (fun item -> item % 2 <> 0) input,
            Seq.toArray (
                builderOf input
                |> IndexedListBuilder.removeAll (fun item -> item % 2 = 0)
            ),
            "IndexedListBuilder.removeAll must remove the elements that satisfy the predicate"
        )

    // ---- remove, removeWith, removeMany and removeManyWith ----

    [<TestMethod; DynamicData(nameof ListMemberCases.Values, typeof<ListMemberCases>)>]
    member _.``remove removes the first equal element and tells whether it removed one`` (input : string array, value : string) =
        let builder = builderOf input
        let removed = IndexedListBuilder.remove value builder

        Assert.AreEqual<bool>(
            Array.contains value input,
            removed,
            "IndexedListBuilder.remove must tell whether it removed an element"
        )

        CollectionAssert.AreEqual (
            FlatListOracles.removeFirst (=) value input,
            Seq.toArray builder,
            "IndexedListBuilder.remove must remove the first equal element"
        )

    [<TestMethod; DynamicData(nameof ListMemberCases.Values, typeof<ListMemberCases>)>]
    member _.``removeWith removes the first element that the comparer finds equal and tells whether it removed one``
        (input : string array, value : string)
        =
        let builder = builderOf input
        let removed = IndexedListBuilder.removeWith ignoringCaseEquality value builder

        Assert.AreEqual<bool>(
            Array.exists (FlatListOracles.equalIgnoringCase value) input,
            removed,
            "IndexedListBuilder.removeWith must tell whether it removed an element"
        )

        CollectionAssert.AreEqual (
            FlatListOracles.removeFirst FlatListOracles.equalIgnoringCase value input,
            Seq.toArray builder,
            "IndexedListBuilder.removeWith must remove the first element that the comparer finds equal"
        )

    [<TestMethod; DynamicData(nameof ListMemberCases.ValueSequences, typeof<ListMemberCases>)>]
    member _.``removeMany removes one element for each value`` (input : string array, items : string array) =
        CollectionAssert.AreEqual (
            ListOracles.removeEach (=) items input,
            Seq.toArray (builderOf input |> IndexedListBuilder.removeMany items),
            "IndexedListBuilder.removeMany must remove one element for each value"
        )

    [<TestMethod; DynamicData(nameof ListMemberCases.ValueSequences, typeof<ListMemberCases>)>]
    member _.``removeManyWith removes one element for each value that the comparer finds equal``
        (input : string array, items : string array)
        =
        CollectionAssert.AreEqual (
            ListOracles.removeEach FlatListOracles.equalIgnoringCase items input,
            Seq.toArray (
                builderOf input
                |> IndexedListBuilder.removeManyWith ignoringCaseEquality items
            ),
            "IndexedListBuilder.removeManyWith must remove one element for each value that the comparer finds equal"
        )

    // ---- replace and replaceWith ----

    [<TestMethod; DynamicData(nameof ListMemberCases.PresentValues, typeof<ListMemberCases>)>]
    member _.``replace replaces the first equal element`` (input : string array, oldValue : string) =
        CollectionAssert.AreEqual (
            Assert.WantValueSome (
                FlatListOracles.replaceFirst (=) oldValue "X" input,
                "The oracle must find the old value, which the rows take from the elements"
            ),
            Seq.toArray (builderOf input |> IndexedListBuilder.replace oldValue "X"),
            "IndexedListBuilder.replace must replace the first equal element"
        )

    [<TestMethod; DynamicData(nameof ListMemberCases.PresentValuesIgnoringCase, typeof<ListMemberCases>)>]
    member _.``replaceWith replaces the first element that the comparer finds equal`` (input : string array, oldValue : string) =
        CollectionAssert.AreEqual (
            Assert.WantValueSome (
                FlatListOracles.replaceFirst FlatListOracles.equalIgnoringCase oldValue "X" input,
                "The oracle must find the old value, which the rows take from the elements"
            ),
            Seq.toArray (
                builderOf input
                |> IndexedListBuilder.replaceWith ignoringCaseEquality oldValue "X"
            ),
            "IndexedListBuilder.replaceWith must replace the first element that the comparer finds equal"
        )

    [<TestMethod; DynamicData(nameof ListMemberCases.AbsentValues, typeof<ListMemberCases>)>]
    member _.``replace throws ArgumentException naming the old value when no element equals it, as the builder does``
        (input : string array, oldValue : string)
        =
        let builder = builderOf input

        let ex =
            Assert.ThrowsExactly<ArgumentException>(
                Action (fun () -> IndexedListBuilder.replace oldValue "X" builder |> ignore),
                "IndexedListBuilder.replace must throw ArgumentException for a value that no element equals"
            )

        Assert.AreEqual<string | null>("oldValue", ex.ParamName, "IndexedListBuilder.replace must name the old value")
        CollectionAssert.AreEqual (
            input,
            Seq.toArray builder,
            "IndexedListBuilder.replace must not change the builder when it throws"
        )

    [<TestMethod; DynamicData(nameof ListMemberCases.AbsentValuesIgnoringCase, typeof<ListMemberCases>)>]
    member _.``replaceWith throws ArgumentException naming the old value when the comparer finds no element equal``
        (input : string array, oldValue : string)
        =
        let ex =
            Assert.ThrowsExactly<ArgumentException>(
                Action (fun () ->
                    IndexedListBuilder.replaceWith ignoringCaseEquality oldValue "X" (builderOf input)
                    |> ignore
                ),
                "IndexedListBuilder.replaceWith must throw ArgumentException for a value that the comparer finds in no element"
            )

        Assert.AreEqual<string | null>("oldValue", ex.ParamName, "IndexedListBuilder.replaceWith must name the old value")

    // ---- revRange ----

    [<TestMethod; DynamicData(nameof FlatListTests.SubRanges, typeof<FlatListTests>)>]
    member _.``revRange reverses the elements of the range and keeps the other elements``
        (input : int array, index : int, count : int)
        =
        CollectionAssert.AreEqual (
            ListOracles.changeRange Array.rev input index count,
            Seq.toArray (builderOf input |> IndexedListBuilder.revRange index count),
            "IndexedListBuilder.revRange must reverse the range and keep the other elements"
        )

    [<TestMethod; DynamicData(nameof ListMemberCases.RangeFaults, typeof<ListMemberCases>)>]
    member _.``revRange throws for a range out of range naming the parameter that the BCL names``
        (index : int, count : int, fault : RangeFault)
        =
        let ex =
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(
                Action (fun () ->
                    IndexedListBuilder.revRange index count (filled ())
                    |> ignore
                ),
                "IndexedListBuilder.revRange must throw ArgumentOutOfRangeException for a range out of range"
            )

        // ImmutableList<T>.Builder.Reverse(Int32, Int32) names the count for a range beyond the end
        Assert.AreEqual<string | null>(
            fault.ParamName ("index", "count"),
            ex.ParamName,
            "IndexedListBuilder.revRange must name the parameter that the BCL names"
        )

    // ---- sortRange, sortRangeWith and sortRangeWithComparer ----

    [<TestMethod; DynamicData(nameof FlatListTests.SortRanges, typeof<FlatListTests>)>]
    member _.``functions that sort a range sort the range and keep the other elements``
        (name : string, input : int array, index : int, count : int)
        =
        let struct (sortRange, sortPart) = rangeSorts[name]

        CollectionAssert.AreEqual (
            ListOracles.changeRange sortPart input index count,
            Seq.toArray (sortRange index count (builderOf input)),
            $"IndexedListBuilder.%s{name} must sort the range and keep the other elements"
        )

    [<TestMethod; DynamicData(nameof IndexedListBuilderTests.SortRangesOutOfRange)>]
    member _.``functions that sort a range throw for a range out of range naming the parameter that the BCL names``
        (name : string, index : int, count : int, fault : RangeFault)
        =
        let struct (sortRange, _) = rangeSorts[name]

        let ex =
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(
                Action (fun () -> sortRange index count (filled ()) |> ignore),
                $"IndexedListBuilder.%s{name} must throw ArgumentOutOfRangeException for a range out of range"
            )

        Assert.AreEqual<string | null>(
            fault.ParamName ("index", "count"),
            ex.ParamName,
            $"IndexedListBuilder.%s{name} must name the parameter that the BCL names"
        )

    [<TestMethod; DynamicData(nameof IndexedListBuilderTests.RangeSorts)>]
    member _.``functions that sort a range do not run the comparison for a range of at most one element`` (name : string) =
        let sorts : Map<string, int -> int -> IndexedList<int>.Builder -> IndexedList<int>.Builder> =
            let fail _ _ : int = failwith "The comparison must not run"

            Map.ofArray [|
                nameof IndexedListBuilder.sortRange, IndexedListBuilder.sortRange
                nameof IndexedListBuilder.sortRangeWith, IndexedListBuilder.sortRangeWith fail
                nameof IndexedListBuilder.sortRangeWithComparer,
                IndexedListBuilder.sortRangeWithComparer (Comparer<int>.Create(Comparison<int> fail))
            |]

        // A range of one element and an empty one compare nothing
        CollectionAssert.AreEqual (
            Inputs.Several,
            Seq.toArray (sorts[name] 2 1 (filled ())),
            $"IndexedListBuilder.%s{name} must not compare the one element of a range"
        )

        CollectionAssert.AreEqual (
            Inputs.Several,
            Seq.toArray (sorts[name] 5 0 (filled ())),
            $"IndexedListBuilder.%s{name} must not compare the elements of an empty range"
        )

    [<TestMethod; DynamicData(nameof IndexedListRangeTests.StableRanges, typeof<IndexedListRangeTests>)>]
    member _.``sortRange keeps the order of the elements that compare equal`` (index : int, count : int) =
        CollectionAssert.AreEqual (
            ListOracles.sortedKeyedRange index count,
            KeyedItem.KeysAndTags (
                builderOf (KeyedItem.OfManyPairs ())
                |> IndexedListBuilder.sortRange index count
            ),
            "IndexedListBuilder.sortRange must keep the order of equal elements, as List.sort does"
        )

    [<TestMethod; DynamicData(nameof IndexedListRangeTests.StableRangesAndDirections, typeof<IndexedListRangeTests>)>]
    member _.``sortRangeWith keeps the order of the elements that compare equal``
        (direction : SortDirection, index : int, count : int)
        =
        let comparison = ListOracles.compareKeys direction

        CollectionAssert.AreEqual (
            ListOracles.sortedPairRange comparison index count,
            Seq.toArray (
                builderOf Inputs.ManyPairs
                |> IndexedListBuilder.sortRangeWith comparison index count
            ),
            "IndexedListBuilder.sortRangeWith must keep the order of equal elements, as List.sortWith does"
        )

    [<TestMethod; DynamicData(nameof IndexedListRangeTests.StableRangesAndDirections, typeof<IndexedListRangeTests>)>]
    member _.``sortRangeWithComparer keeps the order of the elements that compare equal``
        (direction : SortDirection, index : int, count : int)
        =
        let comparison = ListOracles.compareKeys direction
        let comparer = Comparer<struct (int * char)>.Create (fun left right -> comparison left right)

        CollectionAssert.AreEqual (
            ListOracles.sortedPairRange comparison index count,
            Seq.toArray (
                builderOf Inputs.ManyPairs
                |> IndexedListBuilder.sortRangeWithComparer comparer index count
            ),
            "IndexedListBuilder.sortRangeWithComparer must keep the order of equal elements, as List.sortWith does"
        )

    // ---- indexOfRange, indexOfRangeWith, lastIndexOfRange and lastIndexOfRangeWith ----

    [<TestMethod; DynamicData(nameof ListMemberCases.ForwardRanges, typeof<ListMemberCases>)>]
    member _.``indexOfRange and indexOfRangeWith return the index of the first equal element of the range or -1``
        (startIndex : int, count : int, value : string)
        =
        Assert.AreEqual<int>(
            FlatListOracles.firstInRange (=) startIndex count value ListNames.Several,
            IndexedListBuilder.indexOfRange startIndex count value (named ()),
            "IndexedListBuilder.indexOfRange must return the index of the first equal element of the range, or -1"
        )

        Assert.AreEqual<int>(
            FlatListOracles.firstInRange FlatListOracles.equalIgnoringCase startIndex count value ListNames.Several,
            IndexedListBuilder.indexOfRangeWith ignoringCaseEquality startIndex count value (named ()),
            "IndexedListBuilder.indexOfRangeWith must compare with the equality comparer"
        )

    [<TestMethod; DynamicData(nameof ListMemberCases.BackwardRanges, typeof<ListMemberCases>)>]
    member _.``lastIndexOfRange and lastIndexOfRangeWith return the index of the last equal element of the range or -1``
        (startIndex : int, count : int, value : string)
        =
        Assert.AreEqual<int>(
            ListOracles.lastIndexOfRange (=) startIndex count value ListNames.Several,
            IndexedListBuilder.lastIndexOfRange startIndex count value (named ()),
            "IndexedListBuilder.lastIndexOfRange must return the index of the last equal element of the range, or -1"
        )

        Assert.AreEqual<int>(
            ListOracles.lastIndexOfRange FlatListOracles.equalIgnoringCase startIndex count value ListNames.Several,
            IndexedListBuilder.lastIndexOfRangeWith ignoringCaseEquality startIndex count value (named ()),
            "IndexedListBuilder.lastIndexOfRangeWith must compare with the equality comparer"
        )

    [<TestMethod; DynamicData(nameof ListMemberCases.ForwardRangesOutOfRange, typeof<ListMemberCases>)>]
    member _.``the searches forward in a range throw for a range out of range``
        (name : string, startIndex : int, count : int, fault : RangeFault)
        =
        let ex =
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(
                Action (fun () ->
                    forwardSearches[name] startIndex count "a" (named ())
                    |> ignore
                ),
                $"IndexedListBuilder.%s{name} must throw ArgumentOutOfRangeException for a range out of range"
            )

        Assert.AreEqual<string | null>(
            fault.ParamName ("startIndex", "count"),
            ex.ParamName,
            $"IndexedListBuilder.%s{name} must name the offending parameter"
        )

    [<TestMethod; DynamicData(nameof ListMemberCases.BackwardRangesOutOfRange, typeof<ListMemberCases>)>]
    member _.``the searches backward in a range throw for a range out of range``
        (name : string, startIndex : int, count : int, fault : RangeFault)
        =
        let ex =
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(
                Action (fun () ->
                    backwardSearches[name] startIndex count "a" (named ())
                    |> ignore
                ),
                $"IndexedListBuilder.%s{name} must throw ArgumentOutOfRangeException for a range out of range"
            )

        Assert.AreEqual<string | null>(
            fault.ParamName ("startIndex", "count"),
            ex.ParamName,
            $"IndexedListBuilder.%s{name} must name the offending parameter"
        )

    // ---- binarySearch, binarySearchWithComparer and binarySearchRangeWithComparer ----

    [<TestMethod; DataRow 7; DataRow 1; DataRow 15; DataRow 4; DataRow 0; DataRow 99>]
    member _.``binarySearch returns what Array.BinarySearch returns`` (value : int) =
        Assert.AreEqual<int>(
            Array.BinarySearch (SortedInputs.Ascending, value),
            IndexedListBuilder.binarySearch value (builderOf SortedInputs.Ascending),
            "IndexedListBuilder.binarySearch must search as Array.BinarySearch does"
        )

    [<TestMethod; DataRow 0; DataRow 1; DataRow 2; DataRow 3>]
    member _.``binarySearch uses the order of sort, which is ordinal for strings`` (index : int) =
        // In the order of the characters, and not in the order of a culture, "A" and "B" come before "a" and "b"
        let strings = [| "A"; "B"; "a"; "b" |]

        Assert.AreEqual<int>(
            index,
            IndexedListBuilder.binarySearch strings[index] (builderOf strings),
            "IndexedListBuilder.binarySearch must find every element of a builder sorted ordinally at its index"
        )
    [<TestMethod; DynamicData(nameof ListMemberCases.NameValues, typeof<ListMemberCases>)>]
    member _.``binarySearchWithComparer searches in the order of the comparer`` (value : string) =
        let comparer = StringComparer.OrdinalIgnoreCase :> IComparer<string>

        Assert.AreEqual<int>(
            Array.BinarySearch (SortedInputs.Names, value, comparer),
            IndexedListBuilder.binarySearchWithComparer comparer value (builderOf SortedInputs.Names),
            "IndexedListBuilder.binarySearchWithComparer must search in the order of the comparer"
        )

    [<TestMethod; DynamicData(nameof ListMemberCases.SearchRanges, typeof<ListMemberCases>)>]
    member _.``binarySearchRangeWithComparer searches the range in the order of the comparer``
        (index : int, count : int, value : int)
        =
        let descending = Array.rev SortedInputs.Ascending

        Assert.AreEqual<int>(
            Array.BinarySearch (descending, index, count, value, SortedInputs.DescendingComparer),
            IndexedListBuilder.binarySearchRangeWithComparer
                SortedInputs.DescendingComparer
                index
                count
                value
                (builderOf descending),
            "IndexedListBuilder.binarySearchRangeWithComparer must search the range in the order of the comparer"
        )

    [<TestMethod; DynamicData(nameof ListMemberCases.RangeFaults, typeof<ListMemberCases>)>]
    member _.``binarySearchRangeWithComparer throws for a range out of range`` (index : int, count : int, fault : RangeFault) =
        let action () =
            IndexedListBuilder.binarySearchRangeWithComparer
                Comparer<int>.Default
                index
                count
                7
                (builderOf SortedInputs.Ascending)
            |> ignore

        match fault with
        | RangeFault.Beyond ->
            // The BCL does not check this and its documentation promises ArgumentException (ADR 0001, D14)
            let ex =
                Assert.ThrowsExactly<ArgumentException>(
                    Action action,
                    "IndexedListBuilder.binarySearchRangeWithComparer must throw ArgumentException for a range beyond the end"
                )

            Assert.AreEqual<string | null>(
                "count",
                ex.ParamName,
                "IndexedListBuilder.binarySearchRangeWithComparer must name the count"
            )
        | _ ->
            let ex =
                Assert.ThrowsExactly<ArgumentOutOfRangeException>(
                    Action action,
                    "IndexedListBuilder.binarySearchRangeWithComparer must throw ArgumentOutOfRangeException for a negative number"
                )

            Assert.AreEqual<string | null>(
                fault.ParamName ("index", "count"),
                ex.ParamName,
                "IndexedListBuilder.binarySearchRangeWithComparer must name the parameter that the BCL names"
            )

    // ---- blit ----

    [<TestMethod>]
    member _.``blit copies a range into an array`` () =
        let target = Array.zeroCreate<int> 5
        IndexedListBuilder.blit (filled ()) 2 target 1 3

        CollectionAssert.AreEqual (
            [| 0; 8; 5; 0; 0 |],
            target,
            "IndexedListBuilder.blit must copy three elements from index 2 to index 1"
        )

    [<TestMethod; DynamicData(nameof ListMemberCases.BlitRejections, typeof<ListMemberCases>)>]
    member _.``blit throws for a range out of range with the names of its parameters``
        (sourceIndex : int, targetIndex : int, count : int, paramName : string)
        =
        let ex =
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(
                Action (fun () -> IndexedListBuilder.blit (filled ()) sourceIndex (Array.zeroCreate 5) targetIndex count),
                "IndexedListBuilder.blit must reject a range out of range"
            )

        Assert.AreEqual<string | null>(paramName, ex.ParamName, $"IndexedListBuilder.blit must name %s{paramName}")

    [<TestMethod>]
    member _.``blit throws for a null target with the name of the parameter`` () =
        FlatListAssert.RejectsNull (
            "IndexedListBuilder.blit",
            "target",
            (fun () -> IndexedListBuilder.blit (filled ()) 0 (Unchecked.defaultof<int array>) 0 0)
        )
