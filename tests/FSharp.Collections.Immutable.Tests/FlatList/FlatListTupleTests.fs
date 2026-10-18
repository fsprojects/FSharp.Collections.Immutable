namespace FSharp.Collections.Immutable.Tests

open FSharp.Collections.Immutable
open FSharp.Collections.Immutable.Tuple
open Microsoft.VisualStudio.TestTools.UnitTesting

// The type annotations of the results are the proof that the functions of the Tuple namespace shadow the default ones.

/// The `FlatList` module of the `Tuple` namespace.
[<TestClass; FlatListTestCategory; TupleNamespaceTestCategory>]
type FlatListTupleTests () =

    static let array = FlatList.ofArray Inputs.Several

    static let other = FlatList.ofArray Inputs.Other

    static let third = FlatList.ofArray Inputs.Third

    [<TestMethod>]
    member _.``unfold takes a generator of value options of reference tuples`` () =
        let generator (state : int) =
            if state > 0 then
                ValueSome (state, state - 1)
            else
                ValueNone
        let generated : FlatList<int> = FlatList.unfold generator 3
        CollectionAssert.AreEqual ([| 3; 2; 1 |], FlatList.toArray generated, "Tuple.FlatList.unfold must generate the elements")

    [<TestMethod>]
    member _.``allPairs returns reference tuples`` () =
        let pairs : FlatList<int * int> = FlatList.allPairs array other

        CollectionAssert.AreEqual (
            Array.allPairs Inputs.Several Inputs.Other,
            FlatList.toArray pairs,
            "Tuple.FlatList.allPairs must pair as Array.allPairs does"
        )

    [<TestMethod>]
    member _.``indexed returns reference tuples`` () =
        let pairs : FlatList<int * int> = FlatList.indexed array
        CollectionAssert.AreEqual (
            Array.indexed Inputs.Several,
            FlatList.toArray pairs,
            "Tuple.FlatList.indexed must index as Array.indexed does"
        )

    [<TestMethod>]
    member _.``pairwise returns reference tuples`` () =
        let pairs : FlatList<int * int> = FlatList.pairwise array

        CollectionAssert.AreEqual (
            Array.pairwise Inputs.Several,
            FlatList.toArray pairs,
            "Tuple.FlatList.pairwise must pair as Array.pairwise does"
        )

    [<TestMethod>]
    member _.``unzip takes and returns reference tuples`` () =
        let (first : FlatList<int>), (second : FlatList<int>) = FlatList.unzip (FlatList.zip array other)
        CollectionAssert.AreEqual (
            Inputs.Several,
            FlatList.toArray first,
            "Tuple.FlatList.unzip must return the first components"
        )
        CollectionAssert.AreEqual (
            Inputs.Other,
            FlatList.toArray second,
            "Tuple.FlatList.unzip must return the second components"
        )

    [<TestMethod>]
    member _.``unzip3 takes and returns reference tuples`` () =
        let (first : FlatList<int>), (second : FlatList<int>), (last : FlatList<int>) =
            FlatList.unzip3 (FlatList.zip3 array other third)
        CollectionAssert.AreEqual (
            Inputs.Several,
            FlatList.toArray first,
            "Tuple.FlatList.unzip3 must return the first components"
        )
        CollectionAssert.AreEqual (
            Inputs.Other,
            FlatList.toArray second,
            "Tuple.FlatList.unzip3 must return the second components"
        )
        CollectionAssert.AreEqual (Inputs.Third, FlatList.toArray last, "Tuple.FlatList.unzip3 must return the third components")

    [<TestMethod>]
    member _.``zip returns reference tuples`` () =
        let pairs : FlatList<int * int> = FlatList.zip array other
        CollectionAssert.AreEqual (
            Array.zip Inputs.Several Inputs.Other,
            FlatList.toArray pairs,
            "Tuple.FlatList.zip must zip as Array.zip does"
        )

    [<TestMethod>]
    member _.``zip throws for different lengths as the default does`` () =
        ExceptionAssert.Same (
            id,
            (fun () ->
                FSharp.Collections.Immutable.FlatList.zip array (FlatList.singleton 1)
                |> ignore
            ),
            (fun () -> FlatList.zip array (FlatList.singleton 1) |> ignore),
            "Tuple.FlatList.zip must throw as the default zip does"
        )

    [<TestMethod>]
    member _.``zip3 returns reference tuples`` () =
        let triples : FlatList<int * int * int> = FlatList.zip3 array other third

        CollectionAssert.AreEqual (
            Array.zip3 Inputs.Several Inputs.Other Inputs.Third,
            FlatList.toArray triples,
            "Tuple.FlatList.zip3 must zip as Array.zip3 does"
        )

    [<TestMethod>]
    member _.``mapFold takes and returns reference tuples`` () =
        let (results : FlatList<int>), (state : int) =
            FlatList.mapFold (fun state item -> item * state, state + 1) 1 array
        let expectedResults, expectedState =
            Array.mapFold (fun state item -> item * state, state + 1) 1 Inputs.Several
        CollectionAssert.AreEqual (
            expectedResults,
            FlatList.toArray results,
            "Tuple.FlatList.mapFold must map as Array.mapFold does"
        )
        Assert.AreEqual<int>(expectedState, state, "Tuple.FlatList.mapFold must thread the state as Array.mapFold does")

    [<TestMethod>]
    member _.``mapFoldBack takes and returns reference tuples`` () =
        let (results : FlatList<int>), (state : int) =
            FlatList.mapFoldBack (fun item state -> item * state, state + 1) array 1
        let expectedResults, expectedState =
            Array.mapFoldBack (fun item state -> item * state, state + 1) Inputs.Several 1
        CollectionAssert.AreEqual (
            expectedResults,
            FlatList.toArray results,
            "Tuple.FlatList.mapFoldBack must map as Array.mapFoldBack does"
        )
        Assert.AreEqual<int>(expectedState, state, "Tuple.FlatList.mapFoldBack must thread the state as Array.mapFoldBack does")

    [<TestMethod>]
    member _.``countBy returns reference tuples`` () =
        let counts : FlatList<int * int> = FlatList.countBy (fun item -> item % 3) array

        CollectionAssert.AreEqual (
            Array.countBy (fun item -> item % 3) Inputs.Several,
            FlatList.toArray counts,
            "Tuple.FlatList.countBy must count as Array.countBy does"
        )

    [<TestMethod>]
    member _.``groupBy returns reference tuples`` () =
        let groups : FlatList<int * FlatList<int>> = FlatList.groupBy (fun item -> item % 3) array
        let expected = Array.groupBy (fun item -> item % 3) Inputs.Several

        // A group pairs its key with its elements, which a pair compares by reference, so the keys and the elements are
        // compared apart
        CollectionAssert.AreEqual (
            Array.map fst expected,
            groups |> FlatList.toArray |> Array.map fst,
            "Tuple.FlatList.groupBy must return the keys as Array.groupBy does"
        )

        CollectionAssert.AreEqual (
            Array.map snd expected,
            groups
            |> FlatList.toArray
            |> Array.map (fun (_, items) -> FlatList.toArray items),
            "Tuple.FlatList.groupBy must group the elements as Array.groupBy does"
        )

    [<TestMethod>]
    member _.``partition returns a reference tuple`` () =
        let (accepted : FlatList<int>), (rejected : FlatList<int>) =
            FlatList.partition (fun item -> item > 0) array
        let expectedAccepted, expectedRejected = Array.partition (fun item -> item > 0) Inputs.Several
        CollectionAssert.AreEqual (
            expectedAccepted,
            FlatList.toArray accepted,
            "Tuple.FlatList.partition must return the accepted elements"
        )
        CollectionAssert.AreEqual (
            expectedRejected,
            FlatList.toArray rejected,
            "Tuple.FlatList.partition must return the rejected elements"
        )

    [<TestMethod>]
    member _.``partitionWith returns a reference tuple`` () =
        let partitioner (item : int) =
            if item > 0 then
                Choice1Of2 item
            else
                Choice2Of2 (string item)
        let (first : FlatList<int>), (second : FlatList<string>) = FlatList.partitionWith partitioner array
        let expectedFirst, expectedSecond = Array.partitionWith partitioner Inputs.Several
        CollectionAssert.AreEqual (
            expectedFirst,
            FlatList.toArray first,
            "Tuple.FlatList.partitionWith must return the first choices"
        )
        CollectionAssert.AreEqual (
            expectedSecond,
            FlatList.toArray second,
            "Tuple.FlatList.partitionWith must return the second choices"
        )

    [<TestMethod>]
    member _.``splitAt returns a reference tuple`` () =
        let (before : FlatList<int>), (after : FlatList<int>) = FlatList.splitAt 3 array
        let expectedBefore, expectedAfter = Array.splitAt 3 Inputs.Several
        CollectionAssert.AreEqual (expectedBefore, FlatList.toArray before, "Tuple.FlatList.splitAt must return the first part")
        CollectionAssert.AreEqual (expectedAfter, FlatList.toArray after, "Tuple.FlatList.splitAt must return the second part")

    [<TestMethod>]
    member _.``the functions without a tuple keep resolving to the default module`` () =
        let found : int voption = FlatList.tryFind (fun item -> item > 6) array
        Assert.AreEqual<int voption>(ValueSome 8, found, "FlatList.tryFind must resolve to the default module")
