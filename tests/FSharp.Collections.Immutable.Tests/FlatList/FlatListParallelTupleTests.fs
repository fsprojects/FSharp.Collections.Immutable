namespace FSharp.Collections.Immutable.Tests

open FSharp.Collections.Immutable
open FSharp.Collections.Immutable.Tuple
open Microsoft.VisualStudio.TestTools.UnitTesting

// The type annotations of the results are the proof that the functions of the Tuple namespace shadow the default ones,
// also in the nested Parallel module.

/// The `FlatList.Parallel` module of the `Tuple` namespace.
[<TestClass; FlatListTestCategory; TupleNamespaceTestCategory; ParallelTestCategory>]
type FlatListParallelTupleTests () =

    static let array = FlatList.ofList Inputs.several

    static let other = FlatList.ofList Inputs.other

    /// A flat list long enough for the functions to split their work among several threads.
    static let large = FlatList.init 10_000 (fun index -> index % 1_000 - 500)

    static let isPositive (item : int) = item > 0

    [<TestMethod; TestCategory(nameof FlatList.Parallel.zip)>]
    member _.``zip returns reference tuples`` () =
        for struct (first, second) in [ struct (array, other); struct (large, FlatList.rev large) ] do
            let pairs : FlatList<int * int> = FlatList.Parallel.zip first second

            Assert.AreEqual<(int * int) list>(
                List.zip (FlatList.toList first) (FlatList.toList second),
                FlatList.toList pairs,
                $"Tuple.FlatList.Parallel.zip of %d{first.Length} elements must zip as List.zip does"
            )

    [<TestMethod; TestCategory(nameof FlatList.Parallel.zip)>]
    member _.``zip throws for different lengths as the default does`` () =
        ExceptionAssert.same
            id
            (fun () ->
                FSharp.Collections.Immutable.FlatList.Parallel.zip array (FlatList.singleton 1)
                |> ignore
            )
            (fun () -> FlatList.Parallel.zip array (FlatList.singleton 1) |> ignore)
            "Tuple.FlatList.Parallel.zip must throw as the default zip does"

    [<TestMethod; TestCategory(nameof FlatList.Parallel.groupBy)>]
    member _.``groupBy returns reference tuples`` () =
        for source in [ array; large ] do
            let groups : FlatList<int * FlatList<int>> = FlatList.Parallel.groupBy (fun item -> item % 3) source

            // The order of the keys and of the elements of a key is open, so both are sorted
            Assert.AreEqual<(int * int list) list>(
                List.groupBy (fun item -> item % 3) (FlatList.toList source)
                |> Seq.map (fun (key, items) -> key, List.sort items)
                |> Seq.sortBy fst
                |> Seq.toList,
                groups
                |> Seq.map (fun (key, items) -> key, items |> Seq.sort |> Seq.toList)
                |> Seq.sortBy fst
                |> Seq.toList,
                $"Tuple.FlatList.Parallel.groupBy of %d{source.Length} elements must group as List.groupBy does"
            )

    [<TestMethod; TestCategory(nameof FlatList.Parallel.partition)>]
    member _.``partition returns a reference tuple`` () =
        for source in [ array; large ] do
            let (accepted : FlatList<int>), (rejected : FlatList<int>) =
                FlatList.Parallel.partition isPositive source
            let expectedAccepted, expectedRejected = List.partition isPositive (FlatList.toList source)

            Assert.AreEqual<int list>(
                expectedAccepted,
                FlatList.toList accepted,
                $"Tuple.FlatList.Parallel.partition of %d{source.Length} elements must return the accepted elements"
            )

            Assert.AreEqual<int list>(
                expectedRejected,
                FlatList.toList rejected,
                $"Tuple.FlatList.Parallel.partition of %d{source.Length} elements must return the rejected elements"
            )

    [<TestMethod; TestCategory(nameof FlatList.Parallel.partitionWith)>]
    member _.``partitionWith returns a reference tuple`` () =
        let partitioner (item : int) =
            if item > 0 then
                Choice1Of2 item
            else
                Choice2Of2 (string item)

        for source in [ array; large ] do
            let (first : FlatList<int>), (second : FlatList<string>) =
                FlatList.Parallel.partitionWith partitioner source
            let expectedFirst, expectedSecond = List.partitionWith partitioner (FlatList.toList source)

            Assert.AreEqual<int list>(
                expectedFirst,
                FlatList.toList first,
                $"Tuple.FlatList.Parallel.partitionWith of %d{source.Length} elements must return the first choices"
            )

            Assert.AreEqual<string list>(
                expectedSecond,
                FlatList.toList second,
                $"Tuple.FlatList.Parallel.partitionWith of %d{source.Length} elements must return the second choices"
            )

    [<TestMethod; TestCategory(nameof FlatList.Parallel.tryFind)>]
    member _.``the functions without a tuple keep resolving to the default module`` () =
        let found : int voption = FlatList.Parallel.tryFind (fun item -> item > 6) array
        Assert.AreEqual<int voption>(ValueSome 8, found, "FlatList.Parallel.tryFind must resolve to the default module")
