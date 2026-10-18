namespace FSharp.Collections.Immutable.Tests

open System
open System.Collections.Generic
open FSharp.Collections.Immutable
open Microsoft.VisualStudio.TestTools.UnitTesting

/// The builder functions of `FlatList` and the `FlatListBuilder` module.
[<TestClass; FlatListTestCategory; BuilderTestCategory>]
type FlatListBuilderTests () =

    /// A builder that holds the elements of the input with several elements.
    static let filled () =
        let builder = FlatList.builder<int>()
        builder.AddRange Inputs.Several
        builder


    /// The length of the arrays that the blits copy into.
    static let targetLength = 5

    /// The functions of the builder that take a sequence, by name and by the input that they reject, with the parameter
    /// that names the input and the function on a builder.
    static let sequenceRejections : Map<string, struct (string * (unit -> unit))> =
        let nullValues = Unchecked.defaultof<seq<int>>
        let defaultValues = Unchecked.defaultof<FlatList<int>> :> seq<int>
        let integers () = FlatList.builder<int>()
        let objects () = FlatList.builder<obj>()
        let comparer = EqualityComparer<int>.Default

        Map.ofArray [|
            "addRange (default flat list of strings as objects)",
            struct ("values",
                    (fun () ->
                        FlatListBuilder.addRange FlatListOracles.defaultStringsAsObjects (objects ())
                        |> ignore
                    ))
            "insertManyAt (null sequence)",
            struct ("values",
                    (fun () ->
                        FlatListBuilder.insertManyAt 0 nullValues (integers ())
                        |> ignore
                    ))
            "insertManyAt (default flat list)",
            struct ("values",
                    (fun () ->
                        FlatListBuilder.insertManyAt 0 defaultValues (integers ())
                        |> ignore
                    ))
            "insertManyAt (default flat list of strings as objects)",
            struct ("values",
                    (fun () ->
                        FlatListBuilder.insertManyAt 0 FlatListOracles.defaultStringsAsObjects (objects ())
                        |> ignore
                    ))
            "removeMany (null sequence)",
            struct ("items",
                    (fun () ->
                        FlatListBuilder.removeMany nullValues (integers ())
                        |> ignore
                    ))
            "removeMany (default flat list)",
            struct ("items",
                    (fun () ->
                        FlatListBuilder.removeMany defaultValues (integers ())
                        |> ignore
                    ))
            "removeMany (default flat list of strings as objects)",
            struct ("items",
                    (fun () ->
                        FlatListBuilder.removeMany FlatListOracles.defaultStringsAsObjects (objects ())
                        |> ignore
                    ))
            "removeManyWith (null sequence)",
            struct ("items",
                    (fun () ->
                        FlatListBuilder.removeManyWith comparer nullValues (integers ())
                        |> ignore
                    ))
            "removeManyWith (default flat list)",
            struct ("items",
                    (fun () ->
                        FlatListBuilder.removeManyWith comparer defaultValues (integers ())
                        |> ignore
                    ))
            "removeManyWith (default flat list of strings as objects)",
            struct ("items",
                    (fun () ->
                        FlatListBuilder.removeManyWith
                            EqualityComparer<obj>.Default
                            FlatListOracles.defaultStringsAsObjects
                            (objects ())
                        |> ignore
                    ))
        |]

    /// The functions of the builder that change it and return it, by name, applied with valid arguments to a builder of
    /// the input with several elements.
    static let chainedFunctions : Map<string, FlatList<int>.Builder -> FlatList<int>.Builder> =
        let comparer = EqualityComparer<int>.Default

        Map.ofArray [|
            nameof FlatListBuilder.insertManyAt, FlatListBuilder.insertManyAt 1 [| 100 |]
            nameof FlatListBuilder.removeAll, FlatListBuilder.removeAll (fun item -> item < 0)
            nameof FlatListBuilder.removeMany, FlatListBuilder.removeMany [| 5 |]
            nameof FlatListBuilder.removeManyAt, FlatListBuilder.removeManyAt 1 2
            nameof FlatListBuilder.removeManyWith, FlatListBuilder.removeManyWith comparer [| 5 |]
            nameof FlatListBuilder.replace, FlatListBuilder.replace 5 99
            nameof FlatListBuilder.replaceWith, FlatListBuilder.replaceWith comparer 5 99
            nameof FlatListBuilder.sortRange, FlatListBuilder.sortRange 1 4
            nameof FlatListBuilder.sortRangeWith, FlatListBuilder.sortRangeWith compare 1 4
            nameof FlatListBuilder.sortRangeWithComparer, FlatListBuilder.sortRangeWithComparer Comparer<int>.Default 1 4
        |]

    /// The functions that sort a range, by name, with the function and the sort of the range in the oracle. The names are
    /// the ones of the table of FlatList, so that the rows of its tests run here too.
    static let rangeSorts
        : Map<string, struct ((int -> int -> FlatList<int>.Builder -> FlatList<int>.Builder) * (int array -> int array))> =
        Map.ofArray [|
            nameof FlatList.sortRange, struct (FlatListBuilder.sortRange, Array.sortWith compare)
            nameof FlatList.sortRangeWith,
            struct (FlatListBuilder.sortRangeWith FlatListOracles.descending, Array.sortWith FlatListOracles.descending)
            nameof FlatList.sortRangeWithComparer,
            struct (FlatListBuilder.sortRangeWithComparer (Comparer<int>.Create(Comparison<int> FlatListOracles.byAbsolute)),
                    Array.sortWith FlatListOracles.byAbsolute)
        |]

    /// The predicates of removeAll, by name.
    static let removalPredicates : Map<string, int -> bool> =
        Map.ofArray [|
            "negative", (fun item -> item < 0)
            "positive", (fun item -> item > 0)
            "every element", (fun _ -> true)
            "no element", (fun _ -> false)
        |]

    /// The names of the functions that take a sequence, as the rows of the test of the sequences that they reject.
    static member SequenceRejections = sequenceRejections.Keys

    /// The names of the functions that change the builder and return it, as the rows of their test.
    static member ChainedFunctions = chainedFunctions.Keys

    /// Every index of every input, with the values to insert: none, one and two.
    static member InsertionPoints = seq {
        for input in Inputs.All do
            for index in 0 .. input.Length do
                for values in [| [||]; [| 100 |]; [| 100; 200 |] |] do
                    struct (input, index, values)
    }

    /// Every input with an index below its first element and one beyond its end.
    static member InsertionPointsOutOfRange = seq {
        for input in Inputs.All do
            for index in [| -1; input.Length + 1 |] do
                struct (input, index)
    }

    /// Every range of every input, as the index of its first element and its length.
    static member RemovalRanges = seq {
        for input in Inputs.All do
            for index in 0 .. input.Length do
                for count in 0 .. input.Length - index do
                    struct (input, index, count)
    }

    /// The ranges that the removal of a range rejects: an index out of range, and a count that is negative or beyond the
    /// end.
    static member RemovalRangesOutOfRange = seq {
        struct (Inputs.Empty, 1, 0)
        struct (Inputs.Empty, 0, 1)
        struct (Inputs.Empty, -1, 0)
        struct (Inputs.Several, -1, 0)
        struct (Inputs.Several, 9, 0)
        struct (Inputs.Several, 0, -1)
        struct (Inputs.Several, 0, 9)
        struct (Inputs.Several, 5, 4)
        struct (Inputs.Several, 8, 1)
        struct (Inputs.Several, -1, -1)
    }

    /// Every predicate of removeAll on every input.
    static member RemovalPredicates = seq {
        for name in removalPredicates.Keys do
            for input in Inputs.All do
                struct (name, input)
    }

    /// Every range of every input that fits a target of five elements, copied to the first and to the last place that fits.
    static member BlitRanges = seq {
        for input in Inputs.All do
            for sourceIndex in 0 .. input.Length do
                for count in 0 .. min (input.Length - sourceIndex) targetLength do
                    for targetIndex in Array.distinct [| 0; targetLength - count |] do
                        struct (input, sourceIndex, targetIndex, count)
    }

    /// The ranges that the blit rejects, for a target of five elements: in the order of the checks of the BCL, a negative
    /// count, a source range out of range and a target range out of range.
    static member BlitRangesOutOfRange = seq {
        struct (Inputs.Empty, 1, 0, 0)
        struct (Inputs.Empty, 0, 0, 1)
        struct (Inputs.Several, 6, 0, 3)
        struct (Inputs.Several, 0, 3, 3)
        struct (Inputs.Several, 0, 6, 0)
        struct (Inputs.Several, -1, 0, 1)
        struct (Inputs.Several, 0, -1, 1)
        struct (Inputs.Several, 0, 0, -1)
        struct (Inputs.Several, 9, 0, 0)
        struct (Inputs.Several, 0, 0, 9)
        struct (Inputs.Several, -1, -1, -1)
    }

    [<TestMethod>]
    member _.``build returns the elements that the action adds`` () =
        let array = FlatList.build (fun builder -> builder.AddRange Inputs.Several)
        CollectionAssert.AreEqual (Inputs.Several, FlatList.toArray array, "FlatList.build must return the added elements")

    [<TestMethod>]
    member _.``addRange rejects a default flat list, which it takes as a sequence`` () =
        let ex =
            Assert.ThrowsExactly<ArgumentNullException>(
                Action (fun () ->
                    FlatListBuilder.addRange (Unchecked.defaultof<FlatList<int>> :> seq<_>) (FlatList.builder<int>())
                    |> ignore
                ),
                "FlatListBuilder.addRange must reject a default flat list"
            )

        Assert.AreEqual<string | null>(
            "values",
            ex.ParamName,
            "FlatListBuilder.addRange must name the parameter of the default flat list"
        )

    [<TestMethod>]
    member _.``addRange rejects a null sequence and names the parameter of the function`` () =
        let ex =
            Assert.ThrowsExactly<ArgumentNullException>(
                Action (fun () ->
                    FlatListBuilder.addRange Unchecked.defaultof<seq<int>> (FlatList.builder<int>())
                    |> ignore
                ),
                "FlatListBuilder.addRange must reject a null sequence"
            )

        Assert.AreEqual<string | null>(
            "values",
            ex.ParamName,
            "FlatListBuilder.addRange must name its own parameter, not the one of the BCL"
        )

    [<TestMethod>]
    member _.``builder starts empty`` () =
        let builder = FlatList.builder<int>()
        Assert.AreEqual<int>(0, builder.Count, "FlatList.builder must start without elements")

    [<TestMethod>]
    member _.``builderWith reserves the capacity`` () =
        let builder = FlatList.builderWith<int> 10
        Assert.AreEqual<int>(10, builder.Capacity, "FlatList.builderWith must reserve the capacity")
        Assert.AreEqual<int>(0, builder.Count, "FlatList.builderWith must start without elements")

    [<TestMethod>]
    member _.``builderWith throws for a negative capacity`` () =
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            Action (fun () -> FlatList.builderWith<int> -1 |> ignore),
            "FlatList.builderWith must reject a negative capacity as the BCL does"
        )
        |> ignore

    [<TestMethod>]
    member _.``moveFromBuilder takes the contents of a full builder`` () =
        let builder = FlatList.builderWith<int> 3
        builder.AddRange [| 1; 2; 3 |]
        let array = FlatList.moveFromBuilder builder
        CollectionAssert.AreEqual ([| 1; 2; 3 |], FlatList.toArray array, "FlatList.moveFromBuilder must return the elements")
        Assert.AreEqual<int>(0, builder.Count, "FlatList.moveFromBuilder must empty the builder")

    [<TestMethod>]
    member _.``moveFromBuilder throws for a builder that is not full`` () =
        let builder = FlatList.builderWith<int> 3
        builder.Add 1

        Assert.ThrowsExactly<InvalidOperationException>(
            Action (fun () -> FlatList.moveFromBuilder builder |> ignore),
            "FlatList.moveFromBuilder must reject a builder whose count differs from its capacity"
        )
        |> ignore

    [<TestMethod>]
    member _.``ofBuilder copies the contents and keeps the builder`` () =
        let builder = filled ()
        let array = FlatList.ofBuilder builder
        builder.Add 99
        CollectionAssert.AreEqual (Inputs.Several, FlatList.toArray array, "FlatList.ofBuilder must copy the elements")
        Assert.AreEqual<int>(Inputs.Several.Length + 1, builder.Count, "FlatList.ofBuilder must keep the builder usable")

    [<TestMethod>]
    member _.``ofBuilder throws for a null builder`` () =
        let ex =
            Assert.ThrowsExactly<ArgumentNullException>(
                Action (fun () ->
                    FlatList.ofBuilder (Unchecked.defaultof<FlatList<int>.Builder>)
                    |> ignore
                ),
                "FlatList.ofBuilder must reject a null builder"
            )

        Assert.AreEqual<string | null>("builder", ex.ParamName, "FlatList.ofBuilder must name the builder parameter")

    [<TestMethod>]
    member _.``toBuilder copies the elements into a new builder`` () =
        let array = FlatList.ofArray Inputs.Several
        let builder = FlatList.toBuilder array
        builder.Add 99

        CollectionAssert.AreEqual (
            Array.append Inputs.Several [| 99 |],
            Seq.toArray builder,
            "FlatList.toBuilder must hold the elements"
        )

        CollectionAssert.AreEqual (Inputs.Several, FlatList.toArray array, "FlatList.toBuilder must not change the flat list")

    [<TestMethod>]
    member _.``update applies the action to a copy`` () =
        let array = FlatList.ofArray Inputs.Several
        let updated = FlatList.update (fun builder -> builder.RemoveAt 0) array
        CollectionAssert.AreEqual (Array.tail Inputs.Several, FlatList.toArray updated, "FlatList.update must apply the action")
        CollectionAssert.AreEqual (Inputs.Several, FlatList.toArray array, "FlatList.update must not change the input")

    [<TestMethod>]
    member _.``add, addRange and insertAt return the same builder`` () =
        let builder = FlatList.builder<int>()
        let result =
            builder
            |> FlatListBuilder.add 1
            |> FlatListBuilder.addRange [ 2; 3 ]
            |> FlatListBuilder.insertAt 0 0
        Assert.AreSame (builder, result, "The functions that change a builder must return it")
        CollectionAssert.AreEqual ([| 0; 1; 2; 3 |], Seq.toArray builder, "add, addRange and insertAt must change the builder")

    [<TestMethod>]
    member _.``capacity, setCapacity and length report the builder`` () =
        let builder =
            FlatList.builderWith<int> 4
            |> FlatListBuilder.add 1
            |> FlatListBuilder.setCapacity 10
        Assert.AreEqual<int>(10, FlatListBuilder.capacity builder, "FlatListBuilder.capacity must return the capacity")
        Assert.AreEqual<int>(1, FlatListBuilder.length builder, "FlatListBuilder.length must count the elements")

    [<TestMethod>]
    member _.``clear removes every element`` () =
        let builder = filled () |> FlatListBuilder.clear
        Assert.AreEqual<int>(0, FlatListBuilder.length builder, "FlatListBuilder.clear must remove every element")

    [<TestMethod>]
    member _.``contains, indexOf and lastIndexOf search the builder`` () =
        let builder = filled ()
        Assert.IsTrue (FlatListBuilder.contains 8 builder, "FlatListBuilder.contains must find 8")
        Assert.AreEqual<int>(1, FlatListBuilder.indexOf -3 builder, "FlatListBuilder.indexOf must return the first index")
        Assert.AreEqual<int>(5, FlatListBuilder.lastIndexOf -3 builder, "FlatListBuilder.lastIndexOf must return the last index")
        Assert.AreEqual<int>(-1, FlatListBuilder.indexOf 1000 builder, "FlatListBuilder.indexOf must return -1 when absent")

    [<TestMethod>]
    member _.``drainToFlatList returns the elements and empties the builder`` () =
        let builder = filled ()
        let array = FlatListBuilder.drainToFlatList builder

        CollectionAssert.AreEqual (
            Inputs.Several,
            FlatList.toArray array,
            "FlatListBuilder.drainToFlatList must return the elements"
        )

        Assert.AreEqual<int>(0, FlatListBuilder.length builder, "FlatListBuilder.drainToFlatList must empty the builder")

    [<TestMethod>]
    member _.``item and updateAt read and replace an element`` () =
        let builder = filled () |> FlatListBuilder.updateAt 2 99
        Assert.AreEqual<int>(99, FlatListBuilder.item 2 builder, "FlatListBuilder.updateAt must replace the element")

    [<TestMethod>]
    member _.``remove and removeAt remove elements`` () =
        let builder = filled ()
        Assert.IsTrue (FlatListBuilder.remove -3 builder, "FlatListBuilder.remove must report a removed element")
        Assert.IsFalse (FlatListBuilder.remove 1000 builder, "FlatListBuilder.remove must report a missing element")
        FlatListBuilder.removeAt 0 builder |> ignore
        CollectionAssert.AreEqual ([| 8; 5; 0; -3; 12; 7 |], Seq.toArray builder, "remove and removeAt must remove the elements")

    [<TestMethod>]
    member _.``rev reverses the builder`` () =
        let builder = filled () |> FlatListBuilder.rev
        CollectionAssert.AreEqual (Array.rev Inputs.Several, Seq.toArray builder, "FlatListBuilder.rev must reverse the elements")

    [<TestMethod>]
    member _.``sort, sortWith and sortWithComparer order the builder`` () =
        CollectionAssert.AreEqual (
            Array.sort Inputs.Several,
            Seq.toArray (filled () |> FlatListBuilder.sort),
            "FlatListBuilder.sort must sort ascending"
        )

        CollectionAssert.AreEqual (
            Array.sortDescending Inputs.Several,
            Seq.toArray (
                filled ()
                |> FlatListBuilder.sortWith (fun left right -> compare right left)
            ),
            "FlatListBuilder.sortWith must use the comparison function"
        )

        CollectionAssert.AreEqual (
            Array.sortDescending Inputs.Several,
            Seq.toArray (
                filled ()
                |> FlatListBuilder.sortWithComparer (Comparer<int>.Create (fun left right -> compare right left))
            ),
            "FlatListBuilder.sortWithComparer must use the comparer"
        )

    [<TestMethod>]
    member _.``toArray copies the elements`` () =
        let builder = filled ()
        CollectionAssert.AreEqual (
            Inputs.Several,
            FlatListBuilder.toArray builder,
            "FlatListBuilder.toArray must copy the elements"
        )

    [<TestMethod; DynamicData(nameof FlatListBuilderTests.ChainedFunctions)>]
    member _.``functions that change the builder return it`` (name : string) =
        let builder = filled ()
        let result = chainedFunctions[name] builder
        Assert.AreSame (builder, result, $"FlatListBuilder.%s{name} must return the builder that it changed")

    [<TestMethod; DynamicData(nameof FlatListBuilderTests.SequenceRejections)>]
    member _.``functions that take a sequence reject a null sequence, a default flat list and a default flat list of strings``
        (name : string)
        =
        let struct (paramName, actual) = sequenceRejections[name]
        FlatListAssert.RejectsNull ($"FlatListBuilder.%s{name}", paramName, actual)

    [<TestMethod>]
    member _.``a flat list of strings that is not default passes as a sequence of objects`` () =
        let widened = FlatListOracles.stringsAsObjects (FlatList.ofArray [| "a"; "b" |])
        let builder = FlatList.builder<obj>()

        // Add the elements, insert them again after the first one, then remove one element for each of them
        builder
        |> FlatListBuilder.addRange widened
        |> FlatListBuilder.insertManyAt 1 widened
        |> FlatListBuilder.removeMany widened
        |> ignore

        CollectionAssert.AreEqual (
            [| "a" :> obj; "b" :> obj |],
            Seq.toArray builder,
            "addRange, insertManyAt and removeMany must take the elements of a flat list of strings as objects"
        )
    [<TestMethod; DynamicData(nameof FlatListBuilderTests.InsertionPoints)>]
    member _.``insertManyAt inserts the values at the index as Array.insertManyAt does``
        (input : int array, index : int, values : int array)
        =
        let builder = FlatListOracles.builderOf input
        FlatListBuilder.insertManyAt index values builder |> ignore

        CollectionAssert.AreEqual (
            Array.insertManyAt index values input,
            Seq.toArray builder,
            "FlatListBuilder.insertManyAt must insert the values at the index"
        )

    [<TestMethod; DynamicData(nameof FlatListBuilderTests.InsertionPointsOutOfRange)>]
    member _.``insertManyAt throws for an index out of range as the BCL does`` (input : int array, index : int) =
        ExceptionAssert.Same (
            id,
            (fun () -> (FlatListOracles.builderOf input).InsertRange(index, [| 100 |])),
            (fun () ->
                FlatListBuilder.insertManyAt index [| 100 |] (FlatListOracles.builderOf input)
                |> ignore
            ),
            "FlatListBuilder.insertManyAt must throw as InsertRange does for an index out of range"
        )

    [<TestMethod; DataRow(-1); DataRow(99)>]
    member _.``insertManyAt checks the index before the values as the BCL does`` (index : int) =
        let values = Unchecked.defaultof<seq<int>>

        ExceptionAssert.Same (
            id,
            (fun () -> (filled ()).InsertRange(index, values)),
            (fun () ->
                FlatListBuilder.insertManyAt index values (filled ())
                |> ignore
            ),
            "FlatListBuilder.insertManyAt must report the index out of range, not the missing values"
        )

    [<TestMethod; DynamicData(nameof FlatListBuilderTests.RemovalRanges)>]
    member _.``removeManyAt removes the range as Array.removeManyAt does`` (input : int array, index : int, count : int) =
        let builder = FlatListOracles.builderOf input
        FlatListBuilder.removeManyAt index count builder |> ignore

        CollectionAssert.AreEqual (
            Array.removeManyAt index count input,
            Seq.toArray builder,
            "FlatListBuilder.removeManyAt must remove the range"
        )

    [<TestMethod; DynamicData(nameof FlatListBuilderTests.RemovalRangesOutOfRange)>]
    member _.``removeManyAt throws for a range out of range as the BCL does`` (input : int array, index : int, count : int) =
        // RemoveRange calls the count length, and the function calls it count
        let countOfLength (parameter : string) =
            match parameter with
            | "length" -> "count"
            | other -> other

        ExceptionAssert.Same (
            countOfLength,
            (fun () -> (FlatListOracles.builderOf input).RemoveRange(index, count)),
            (fun () ->
                FlatListBuilder.removeManyAt index count (FlatListOracles.builderOf input)
                |> ignore
            ),
            "FlatListBuilder.removeManyAt must throw as RemoveRange does, with the parameter name count"
        )

    [<TestMethod; DynamicData(nameof FlatListTests.RemovalItems, typeof<FlatListTests>)>]
    member _.``removeMany removes one element for each value`` (input : int array, items : int array) =
        let builder = FlatListOracles.builderOf input
        FlatListBuilder.removeMany items builder |> ignore

        CollectionAssert.AreEqual (
            Array.fold (fun elements item -> FlatListOracles.removeFirst (=) item elements) input items,
            Seq.toArray builder,
            "FlatListBuilder.removeMany must remove one element for each value"
        )

    [<TestMethod>]
    member _.``removeMany compares in the default equality, which tells b from B`` () =
        let builder = FlatListOracles.builderOf (FlatListOracles.names ())
        FlatListBuilder.removeMany [| "B" |] builder |> ignore
        CollectionAssert.AreEqual (
            [| "a"; "b"; "c" |],
            Seq.toArray builder,
            "FlatListBuilder.removeMany must not find B equal to b"
        )

    [<TestMethod; DynamicData(nameof FlatListTests.NameItems, typeof<FlatListTests>)>]
    member _.``removeManyWith removes one element for each value that the comparer finds equal`` (items : string array) =
        let builder = FlatListOracles.builderOf (FlatListOracles.names ())
        FlatListBuilder.removeManyWith StringComparer.OrdinalIgnoreCase items builder
        |> ignore

        CollectionAssert.AreEqual (
            Array.fold
                (fun elements item -> FlatListOracles.removeFirst FlatListOracles.equalIgnoringCase item elements)
                (FlatListOracles.names ())
                items,
            Seq.toArray builder,
            "FlatListBuilder.removeManyWith must remove one element for each value that the comparer finds equal"
        )

    [<TestMethod; DynamicData(nameof FlatListTests.NameValues, typeof<FlatListTests>)>]
    member _.``removeWith removes the first element that the comparer finds equal and reports it`` (value : string) =
        let names = FlatListOracles.names ()
        let builder = FlatListOracles.builderOf names
        let removed = FlatListBuilder.removeWith StringComparer.OrdinalIgnoreCase value builder
        let expected = FlatListOracles.removeFirst FlatListOracles.equalIgnoringCase value names

        Assert.AreEqual<bool>(
            expected.Length < names.Length,
            removed,
            "FlatListBuilder.removeWith must report whether it removed an element"
        )

        CollectionAssert.AreEqual (
            expected,
            Seq.toArray builder,
            "FlatListBuilder.removeWith must remove the first element that the comparer finds equal"
        )

    [<TestMethod; DynamicData(nameof FlatListBuilderTests.RemovalPredicates)>]
    member _.``removeAll removes the elements that satisfy the predicate`` (name : string, input : int array) =
        let predicate = removalPredicates[name]
        let builder = FlatListOracles.builderOf input
        FlatListBuilder.removeAll predicate builder |> ignore

        CollectionAssert.AreEqual (
            input |> Array.filter (predicate >> not),
            Seq.toArray builder,
            $"FlatListBuilder.removeAll must remove the elements of the predicate: %s{name}"
        )

    [<TestMethod; DynamicData(nameof FlatListTests.ReplacedValues, typeof<FlatListTests>)>]
    member _.``replace replaces the first equal element`` (input : int array, oldValue : int) =
        let builder = FlatListOracles.builderOf input
        FlatListBuilder.replace oldValue 99 builder |> ignore

        CollectionAssert.AreEqual (
            Assert.WantValueSome (
                FlatListOracles.replaceFirst (=) oldValue 99 input,
                "The test data must contain the value to replace"
            ),
            Seq.toArray builder,
            "FlatListBuilder.replace must replace the first equal element"
        )

    [<TestMethod; DynamicData(nameof FlatListTests.AbsentValues, typeof<FlatListTests>)>]
    member _.``replace leaves the builder as it is when no element equals the value, as the BCL does``
        (input : int array, value : int)
        =
        let builder = FlatListOracles.builderOf input
        FlatListBuilder.replace value 0 builder |> ignore

        CollectionAssert.AreEqual (
            input,
            Seq.toArray builder,
            "FlatListBuilder.replace must not change the builder and must not throw when no element equals the value"
        )

    [<TestMethod; DynamicData(nameof FlatListTests.ReplacedNames, typeof<FlatListTests>)>]
    member _.``replaceWith replaces the first element that the comparer finds equal`` (oldValue : string) =
        let names = FlatListOracles.names ()
        let builder = FlatListOracles.builderOf names
        FlatListBuilder.replaceWith StringComparer.OrdinalIgnoreCase oldValue "X" builder
        |> ignore

        CollectionAssert.AreEqual (
            Assert.WantValueSome (
                FlatListOracles.replaceFirst FlatListOracles.equalIgnoringCase oldValue "X" names,
                "The test data must contain a string that the comparer finds equal"
            ),
            Seq.toArray builder,
            "FlatListBuilder.replaceWith must replace the first element that the comparer finds equal"
        )

    [<TestMethod>]
    member _.``replaceWith leaves the builder as it is when no element equals the value, as the BCL does`` () =
        let names = FlatListOracles.names ()
        let builder = FlatListOracles.builderOf names
        FlatListBuilder.replaceWith StringComparer.OrdinalIgnoreCase "d" "X" builder
        |> ignore

        CollectionAssert.AreEqual (
            names,
            Seq.toArray builder,
            "FlatListBuilder.replaceWith must not change the builder and must not throw when no element equals the value"
        )

    [<TestMethod>]
    member _.``replace and replaceWith compare in the default equality, which tells b from B, for a null comparer`` () =
        let comparer = Unchecked.defaultof<IEqualityComparer<string>>

        let replaced =
            FlatListOracles.builderOf (FlatListOracles.names ())
            |> FlatListBuilder.replaceWith comparer "B" "X"

        let plain =
            FlatListOracles.builderOf (FlatListOracles.names ())
            |> FlatListBuilder.replace "B" "X"

        CollectionAssert.AreEqual (
            [| "a"; "b"; "X"; "c" |],
            Seq.toArray replaced,
            "FlatListBuilder.replaceWith must compare in the default equality for a null comparer"
        )

        CollectionAssert.AreEqual (
            [| "a"; "b"; "X"; "c" |],
            Seq.toArray plain,
            "FlatListBuilder.replace must not find B equal to b"
        )

    [<TestMethod; DynamicData(nameof FlatListTests.SortRanges, typeof<FlatListTests>)>]
    member _.``functions that sort a range sort the range and keep the other elements``
        (name : string, input : int array, index : int, count : int)
        =
        let struct (sortRange, sortPart) = rangeSorts[name]
        let builder = FlatListOracles.builderOf input
        sortRange index count builder |> ignore

        let expected =
            Array.concat [|
                Array.sub input 0 index
                sortPart (Array.sub input index count)
                Array.sub input (index + count) (input.Length - index - count)
            |]

        CollectionAssert.AreEqual (
            expected,
            Seq.toArray builder,
            $"FlatListBuilder.%s{name} must sort the range and keep the other elements"
        )

    [<TestMethod; DynamicData(nameof FlatListTests.SortRangesOutOfRange, typeof<FlatListTests>)>]
    member _.``functions that sort a range throw for a range out of range as the BCL does``
        (name : string, index : int, count : int, paramName : string)
        =
        let struct (sortRange, _) = rangeSorts[name]

        let ex =
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(
                Action (fun () -> sortRange index count (filled ()) |> ignore),
                $"FlatListBuilder.%s{name} must throw ArgumentOutOfRangeException for a range out of range"
            )

        Assert.AreEqual<string | null>(
            paramName,
            ex.ParamName,
            $"FlatListBuilder.%s{name} must name the parameter that the BCL names"
        )

    [<TestMethod>]
    member _.``sortRange orders strings ordinally, as F# generic comparison does`` () =
        let builder = FlatListOracles.builderOf [| "b"; "A"; "a"; "B" |]
        FlatListBuilder.sortRange 0 4 builder |> ignore

        CollectionAssert.AreEqual (
            [| "A"; "B"; "a"; "b" |],
            Seq.toArray builder,
            "FlatListBuilder.sortRange must sort strings ordinally"
        )

    [<TestMethod; DynamicData(nameof FlatListBuilderTests.BlitRanges)>]
    member _.``blit copies the range into the array as Array.blit does``
        (input : int array, sourceIndex : int, targetIndex : int, count : int)
        =
        let target = Array.create targetLength (-1)
        let expected = Array.copy target
        Array.blit input sourceIndex expected targetIndex count
        FlatListBuilder.blit (FlatListOracles.builderOf input) sourceIndex target targetIndex count

        CollectionAssert.AreEqual (expected, target, "FlatListBuilder.blit must copy the range into the array")

    [<TestMethod; DynamicData(nameof FlatListBuilderTests.BlitRangesOutOfRange)>]
    member _.``blit throws for a range out of range as the BCL does``
        (input : int array, sourceIndex : int, targetIndex : int, count : int)
        =
        // CopyTo calls the target destination, its index destinationIndex and the count length
        let ofBcl (parameter : string) =
            match parameter with
            | "destination" -> "target"
            | "destinationIndex" -> "targetIndex"
            | "length" -> "count"
            | other -> other

        ExceptionAssert.Same (
            ofBcl,
            (fun () -> (FlatListOracles.builderOf input).CopyTo(sourceIndex, Array.zeroCreate targetLength, targetIndex, count)),
            (fun () ->
                FlatListBuilder.blit
                    (FlatListOracles.builderOf input)
                    sourceIndex
                    (Array.zeroCreate targetLength)
                    targetIndex
                    count
            ),
            "FlatListBuilder.blit must throw as CopyTo does, with the parameter names of the function"
        )

    [<TestMethod; DataRow(0, 0, 0); DataRow(99, 99, 99)>]
    member _.``blit rejects a null array before the ranges as the BCL does`` (sourceIndex : int, targetIndex : int, count : int) =
        let target = Unchecked.defaultof<int array>

        FlatListAssert.RejectsNull (
            "FlatListBuilder.blit",
            "target",
            (fun () -> FlatListBuilder.blit (filled ()) sourceIndex target targetIndex count)
        )
