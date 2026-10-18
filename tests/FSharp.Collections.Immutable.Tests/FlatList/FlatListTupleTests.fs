namespace FSharp.Collections.Immutable.Tests

open FSharp.Collections.Immutable
open FSharp.Collections.Immutable.Tuple
open Microsoft.VisualStudio.TestTools.UnitTesting

// The type annotations of the results are the proof that the functions of the Tuple namespace shadow the default ones.

/// The `FlatList` module of the `Tuple` namespace.
[<TestClass; FlatListTestCategory; TupleNamespaceTestCategory>]
type FlatListTupleTests () =

    static let array = FlatList.ofList Inputs.several

    static let other = FlatList.ofList Inputs.other

    static let third = FlatList.ofList Inputs.third

    [<TestMethod>]
    member _.``unfold takes a generator of value options of reference tuples`` () =
        let generator (state : int) =
            if state > 0 then
                ValueSome (state, state - 1)
            else
                ValueNone
        let generated : FlatList<int> = FlatList.unfold generator 3
        Assert.AreEqual<int list>([ 3; 2; 1 ], FlatList.toList generated, "Tuple.FlatList.unfold must generate the elements")

    [<TestMethod>]
    member _.``allPairs returns reference tuples`` () =
        let pairs : FlatList<int * int> = FlatList.allPairs array other

        Assert.AreEqual<(int * int) list>(
            List.allPairs Inputs.several Inputs.other,
            FlatList.toList pairs,
            "Tuple.FlatList.allPairs must pair as List.allPairs does"
        )

    [<TestMethod>]
    member _.``indexed returns reference tuples`` () =
        let pairs : FlatList<int * int> = FlatList.indexed array
        Assert.AreEqual<(int * int) list>(
            List.indexed Inputs.several,
            FlatList.toList pairs,
            "Tuple.FlatList.indexed must index as List.indexed does"
        )

    [<TestMethod>]
    member _.``pairwise returns reference tuples`` () =
        let pairs : FlatList<int * int> = FlatList.pairwise array

        Assert.AreEqual<(int * int) list>(
            List.pairwise Inputs.several,
            FlatList.toList pairs,
            "Tuple.FlatList.pairwise must pair as List.pairwise does"
        )

    [<TestMethod>]
    member _.``unzip takes and returns reference tuples`` () =
        let (first : FlatList<int>), (second : FlatList<int>) = FlatList.unzip (FlatList.zip array other)
        Assert.AreEqual<int list>(Inputs.several, FlatList.toList first, "Tuple.FlatList.unzip must return the first components")
        Assert.AreEqual<int list>(Inputs.other, FlatList.toList second, "Tuple.FlatList.unzip must return the second components")

    [<TestMethod>]
    member _.``unzip3 takes and returns reference tuples`` () =
        let (first : FlatList<int>), (second : FlatList<int>), (last : FlatList<int>) =
            FlatList.unzip3 (FlatList.zip3 array other third)
        Assert.AreEqual<int list>(Inputs.several, FlatList.toList first, "Tuple.FlatList.unzip3 must return the first components")
        Assert.AreEqual<int list>(Inputs.other, FlatList.toList second, "Tuple.FlatList.unzip3 must return the second components")
        Assert.AreEqual<int list>(Inputs.third, FlatList.toList last, "Tuple.FlatList.unzip3 must return the third components")

    [<TestMethod>]
    member _.``zip returns reference tuples`` () =
        let pairs : FlatList<int * int> = FlatList.zip array other
        Assert.AreEqual<(int * int) list>(
            List.zip Inputs.several Inputs.other,
            FlatList.toList pairs,
            "Tuple.FlatList.zip must zip as List.zip does"
        )

    [<TestMethod>]
    member _.``zip throws for different lengths as the default does`` () =
        ExceptionAssert.same
            id
            (fun () ->
                FSharp.Collections.Immutable.FlatList.zip array (FlatList.singleton 1)
                |> ignore
            )
            (fun () -> FlatList.zip array (FlatList.singleton 1) |> ignore)
            "Tuple.FlatList.zip must throw as the default zip does"

    [<TestMethod>]
    member _.``zip3 returns reference tuples`` () =
        let triples : FlatList<int * int * int> = FlatList.zip3 array other third

        Assert.AreEqual<(int * int * int) list>(
            List.zip3 Inputs.several Inputs.other Inputs.third,
            FlatList.toList triples,
            "Tuple.FlatList.zip3 must zip as List.zip3 does"
        )

    [<TestMethod>]
    member _.``mapFold takes and returns reference tuples`` () =
        let (results : FlatList<int>), (state : int) =
            FlatList.mapFold (fun state item -> item * state, state + 1) 1 array
        let expectedResults, expectedState =
            List.mapFold (fun state item -> item * state, state + 1) 1 Inputs.several
        Assert.AreEqual<int list>(
            expectedResults,
            FlatList.toList results,
            "Tuple.FlatList.mapFold must map as List.mapFold does"
        )
        Assert.AreEqual<int>(expectedState, state, "Tuple.FlatList.mapFold must thread the state as List.mapFold does")

    [<TestMethod>]
    member _.``mapFoldBack takes and returns reference tuples`` () =
        let (results : FlatList<int>), (state : int) =
            FlatList.mapFoldBack (fun item state -> item * state, state + 1) array 1
        let expectedResults, expectedState =
            List.mapFoldBack (fun item state -> item * state, state + 1) Inputs.several 1
        Assert.AreEqual<int list>(
            expectedResults,
            FlatList.toList results,
            "Tuple.FlatList.mapFoldBack must map as List.mapFoldBack does"
        )
        Assert.AreEqual<int>(expectedState, state, "Tuple.FlatList.mapFoldBack must thread the state as List.mapFoldBack does")

    [<TestMethod>]
    member _.``countBy returns reference tuples`` () =
        let counts : FlatList<int * int> = FlatList.countBy (fun item -> item % 3) array

        Assert.AreEqual<(int * int) list>(
            List.countBy (fun item -> item % 3) Inputs.several,
            FlatList.toList counts,
            "Tuple.FlatList.countBy must count as List.countBy does"
        )

    [<TestMethod>]
    member _.``groupBy returns reference tuples`` () =
        let groups : FlatList<int * FlatList<int>> = FlatList.groupBy (fun item -> item % 3) array

        Assert.AreEqual<(int * int list) list>(
            List.groupBy (fun item -> item % 3) Inputs.several,
            groups
            |> FlatList.toList
            |> List.map (fun (key, items) -> key, FlatList.toList items),
            "Tuple.FlatList.groupBy must group as List.groupBy does"
        )

    [<TestMethod>]
    member _.``partition returns a reference tuple`` () =
        let (accepted : FlatList<int>), (rejected : FlatList<int>) =
            FlatList.partition (fun item -> item > 0) array
        let expectedAccepted, expectedRejected = List.partition (fun item -> item > 0) Inputs.several
        Assert.AreEqual<int list>(
            expectedAccepted,
            FlatList.toList accepted,
            "Tuple.FlatList.partition must return the accepted elements"
        )
        Assert.AreEqual<int list>(
            expectedRejected,
            FlatList.toList rejected,
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
        let expectedFirst, expectedSecond = List.partitionWith partitioner Inputs.several
        Assert.AreEqual<int list>(
            expectedFirst,
            FlatList.toList first,
            "Tuple.FlatList.partitionWith must return the first choices"
        )
        Assert.AreEqual<string list>(
            expectedSecond,
            FlatList.toList second,
            "Tuple.FlatList.partitionWith must return the second choices"
        )

    [<TestMethod>]
    member _.``splitAt returns a reference tuple`` () =
        let (before : FlatList<int>), (after : FlatList<int>) = FlatList.splitAt 3 array
        let expectedBefore, expectedAfter = List.splitAt 3 Inputs.several
        Assert.AreEqual<int list>(expectedBefore, FlatList.toList before, "Tuple.FlatList.splitAt must return the first part")
        Assert.AreEqual<int list>(expectedAfter, FlatList.toList after, "Tuple.FlatList.splitAt must return the second part")

    [<TestMethod>]
    member _.``the functions without a tuple keep resolving to the default module`` () =
        let found : int voption = FlatList.tryFind (fun item -> item > 6) array
        Assert.AreEqual<int voption>(ValueSome 8, found, "FlatList.tryFind must resolve to the default module")
