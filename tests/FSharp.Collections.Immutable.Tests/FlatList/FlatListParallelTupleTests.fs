namespace FSharp.Collections.Immutable.Tests

open FSharp.Collections.Immutable
open FSharp.Collections.Immutable.Tuple
open Microsoft.VisualStudio.TestTools.UnitTesting

// The type annotations of the results are the proof that the functions of the Tuple namespace shadow the default ones,
// also in the nested Parallel module.

/// The `FlatList.Parallel` module of the `Tuple` namespace.
[<TestClass; FlatListTestCategory; TupleNamespaceTestCategory; ParallelTestCategory>]
type FlatListParallelTupleTests () =

    static let array = FlatList.ofArray Inputs.Several

    /// The elements of a flat list long enough for the functions to split their work among several threads.
    static let large () = Array.init 10_000 (fun index -> index % 1_000 - 500)

    static let isPositive (item : int) = item > 0

    /// The flat list of the elements of an input.
    static let flatListOf (input : ParallelInput) = FlatList.ofArray (input.Elements large)

    /// The input with several elements of the contracts and the larger input.
    static member Sources = seq {
        ParallelInput.Several
        ParallelInput.Large
    }

    [<TestMethod; DynamicData(nameof FlatListParallelTupleTests.Sources)>]
    member _.``zip returns reference tuples`` (input : ParallelInput) =
        let first = flatListOf input

        // The input with several elements is zipped with the second input of the contracts, the larger input with its
        // reverse
        let second =
            match input with
            | ParallelInput.Several -> FlatList.ofArray Inputs.Other
            | _ -> FlatList.rev first

        let pairs : FlatList<int * int> = FlatList.Parallel.zip first second

        CollectionAssert.AreEqual (
            Array.zip (FlatList.toArray first) (FlatList.toArray second),
            FlatList.toArray pairs,
            "Tuple.FlatList.Parallel.zip must zip as Array.zip does"
        )

    [<TestMethod>]
    member _.``zip throws for different lengths as the default does`` () =
        ExceptionAssert.Same (
            id,
            (fun () ->
                FSharp.Collections.Immutable.FlatList.Parallel.zip array (FlatList.singleton 1)
                |> ignore
            ),
            (fun () -> FlatList.Parallel.zip array (FlatList.singleton 1) |> ignore),
            "Tuple.FlatList.Parallel.zip must throw as the default zip does"
        )

    [<TestMethod; DynamicData(nameof FlatListParallelTupleTests.Sources)>]
    member _.``groupBy returns reference tuples`` (input : ParallelInput) =
        let source = flatListOf input
        let groups : FlatList<int * FlatList<int>> = FlatList.Parallel.groupBy (fun item -> item % 3) source

        // The order of the keys and of the elements of a key is open, so both are sorted; a group pairs its key with its
        // elements, which a pair compares by reference, so the keys and the elements are compared apart
        let expected =
            Array.groupBy (fun item -> item % 3) (FlatList.toArray source)
            |> Array.map (fun (key, items) -> key, Array.sort items)
            |> Array.sortBy fst

        let actual =
            groups
            |> Seq.map (fun (key, items) -> key, items |> Seq.sort |> Seq.toArray)
            |> Seq.sortBy fst
            |> Seq.toArray

        CollectionAssert.AreEqual (
            Array.map fst expected,
            Array.map fst actual,
            "Tuple.FlatList.Parallel.groupBy must find the keys as Array.groupBy does"
        )

        CollectionAssert.AreEqual (
            Array.map snd expected,
            Array.map snd actual,
            "Tuple.FlatList.Parallel.groupBy must group the elements of each key as Array.groupBy does"
        )

    [<TestMethod>]
    member _.``groupBy puts the NaN keys of type float and float32 in one group as Array.Parallel does`` () =
        // The keys are compared as text, because NaN is not equal to itself
        let check (source : 'Key array) =
            let groups : FlatList<'Key * FlatList<'Key>> = FlatList.Parallel.groupBy id (FlatList.ofArray source)

            CollectionAssert.AreEqual (
                Array.Parallel.groupBy id source
                |> Seq.map (fun (key, items) -> string key, items.Length)
                |> Seq.sort
                |> Seq.toArray,
                groups
                |> Seq.map (fun (key, items) -> string key, items.Length)
                |> Seq.sort
                |> Seq.toArray,
                $"Tuple.FlatList.Parallel.groupBy of %A{source} must group as Array.Parallel.groupBy does"
            )

        check [| nan; 1.0; nan; 2.0; nan |]
        check [| nanf; 1.0f; nanf; 2.0f; nanf |]

    [<TestMethod; DynamicData(nameof FlatListParallelTupleTests.Sources)>]
    member _.``partition returns a reference tuple`` (input : ParallelInput) =
        let source = flatListOf input
        let (accepted : FlatList<int>), (rejected : FlatList<int>) =
            FlatList.Parallel.partition isPositive source
        let expectedAccepted, expectedRejected = Array.partition isPositive (FlatList.toArray source)

        CollectionAssert.AreEqual (
            expectedAccepted,
            FlatList.toArray accepted,
            "Tuple.FlatList.Parallel.partition must return the accepted elements"
        )

        CollectionAssert.AreEqual (
            expectedRejected,
            FlatList.toArray rejected,
            "Tuple.FlatList.Parallel.partition must return the rejected elements"
        )

    [<TestMethod; DynamicData(nameof FlatListParallelTupleTests.Sources)>]
    member _.``partitionWith returns a reference tuple`` (input : ParallelInput) =
        let source = flatListOf input

        let partitioner (item : int) =
            if item > 0 then
                Choice1Of2 item
            else
                Choice2Of2 (string item)

        let (first : FlatList<int>), (second : FlatList<string>) =
            FlatList.Parallel.partitionWith partitioner source
        let expectedFirst, expectedSecond = Array.partitionWith partitioner (FlatList.toArray source)

        CollectionAssert.AreEqual (
            expectedFirst,
            FlatList.toArray first,
            "Tuple.FlatList.Parallel.partitionWith must return the first choices"
        )

        CollectionAssert.AreEqual (
            expectedSecond,
            FlatList.toArray second,
            "Tuple.FlatList.Parallel.partitionWith must return the second choices"
        )

    [<TestMethod>]
    member _.``the functions without a tuple keep resolving to the default module`` () =
        let found : int voption = FlatList.Parallel.tryFind (fun item -> item > 6) array
        Assert.AreEqual<int voption>(ValueSome 8, found, "FlatList.Parallel.tryFind must resolve to the default module")
