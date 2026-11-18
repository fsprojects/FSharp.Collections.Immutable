namespace FSharp.Collections.Immutable.Tests

open FSharp.Collections.Immutable
open FSharp.Collections.Immutable.Tuple
open Microsoft.VisualStudio.TestTools.UnitTesting

// The type annotations of the results are the proof that the functions of the Tuple namespace shadow the default ones.

/// The `IndexedList` module of the `Tuple` namespace.
[<TestClass; IndexedListTestCategory; TupleNamespaceTestCategory; PatternMatchingTestCategory>]
type IndexedListTupleTests () =

    static let list = IndexedList.ofArray Inputs.Several

    static let other = IndexedList.ofArray Inputs.Other

    static let third = IndexedList.ofArray Inputs.Third

    [<TestMethod>]
    member _.``unfold takes a generator of value options of reference tuples`` () =
        let generator (state : int) =
            if state > 0 then
                ValueSome (state, state - 1)
            else
                ValueNone

        let generated : IndexedList<int> = IndexedList.unfold generator 3
        CollectionAssert.AreEqual (
            [| 3; 2; 1 |],
            IndexedList.toArray generated,
            "Tuple.IndexedList.unfold must generate the elements"
        )

    [<TestMethod>]
    member _.``tryUncons returns a value option of a reference tuple`` () =
        let found : (int * IndexedList<int>) voption = IndexedList.tryUncons list

        // A tuple compares an array by reference, so the first element and the other elements are compared apart
        let head, tail =
            Assert.WantValueSome (found, "Tuple.IndexedList.tryUncons must return the elements of a list with elements")

        Assert.AreEqual<int>(Array.head Inputs.Several, head, "Tuple.IndexedList.tryUncons must return the first element")

        CollectionAssert.AreEqual (
            Array.tail Inputs.Several,
            IndexedList.toArray tail,
            "Tuple.IndexedList.tryUncons must return the other elements"
        )

    [<TestMethod>]
    member _.``uncons returns a reference tuple`` () =
        let (head : int), (tail : IndexedList<int>) = IndexedList.uncons list
        Assert.AreEqual<int>(Array.head Inputs.Several, head, "Tuple.IndexedList.uncons must return the first element")
        CollectionAssert.AreEqual (
            Array.tail Inputs.Several,
            IndexedList.toArray tail,
            "Tuple.IndexedList.uncons must return the other elements"
        )

    [<TestMethod>]
    member _.``uncons throws as the default does`` () =
        ExceptionAssert.Same (
            id,
            (fun () ->
                FSharp.Collections.Immutable.IndexedList.uncons IndexedList.empty<int>
                |> ignore
            ),
            (fun () -> IndexedList.uncons IndexedList.empty<int> |> ignore),
            "Tuple.IndexedList.uncons must throw as the default uncons does"
        )

    [<TestMethod>]
    member _.``Cons is a struct partial pattern of reference tuples`` () =
        let pattern : IndexedList<int> -> (int * IndexedList<int>) voption = IndexedList.(|Cons|_|)

        match list with
        | IndexedList.Cons (head, tail) ->
            Assert.AreEqual<int>(Array.head Inputs.Several, head, "Tuple.IndexedList.Cons must match the first element")
            CollectionAssert.AreEqual (
                Array.tail Inputs.Several,
                IndexedList.toArray tail,
                "Tuple.IndexedList.Cons must match the other elements"
            )
        | _ -> Assert.Fail "Tuple.IndexedList.Cons must match a list with elements"

        Assert.IsTrue ((pattern IndexedList.empty).IsNone, "Tuple.IndexedList.Cons must not match an empty list")

    [<TestMethod>]
    member _.``allPairs returns reference tuples`` () =
        let pairs : IndexedList<int * int> = IndexedList.allPairs list other

        CollectionAssert.AreEqual (
            Array.allPairs Inputs.Several Inputs.Other,
            IndexedList.toArray pairs,
            "Tuple.IndexedList.allPairs must pair as List.allPairs does"
        )

    [<TestMethod>]
    member _.``indexed returns reference tuples`` () =
        let pairs : IndexedList<int * int> = IndexedList.indexed list

        CollectionAssert.AreEqual (
            Array.indexed Inputs.Several,
            IndexedList.toArray pairs,
            "Tuple.IndexedList.indexed must index as List.indexed does"
        )

    [<TestMethod>]
    member _.``pairwise returns reference tuples`` () =
        let pairs : IndexedList<int * int> = IndexedList.pairwise list

        CollectionAssert.AreEqual (
            Array.pairwise Inputs.Several,
            IndexedList.toArray pairs,
            "Tuple.IndexedList.pairwise must pair as List.pairwise does"
        )

    [<TestMethod>]
    member _.``unzip takes and returns reference tuples`` () =
        let (first : IndexedList<int>), (second : IndexedList<int>) =
            IndexedList.unzip (IndexedList.zip list other)
        CollectionAssert.AreEqual (
            Inputs.Several,
            IndexedList.toArray first,
            "Tuple.IndexedList.unzip must return the first components"
        )
        CollectionAssert.AreEqual (
            Inputs.Other,
            IndexedList.toArray second,
            "Tuple.IndexedList.unzip must return the second components"
        )

    [<TestMethod>]
    member _.``unzip3 takes and returns reference tuples`` () =
        let (first : IndexedList<int>), (second : IndexedList<int>), (last : IndexedList<int>) =
            IndexedList.unzip3 (IndexedList.zip3 list other third)

        CollectionAssert.AreEqual (
            Inputs.Several,
            IndexedList.toArray first,
            "Tuple.IndexedList.unzip3 must return the first components"
        )
        CollectionAssert.AreEqual (
            Inputs.Other,
            IndexedList.toArray second,
            "Tuple.IndexedList.unzip3 must return the second components"
        )
        CollectionAssert.AreEqual (
            Inputs.Third,
            IndexedList.toArray last,
            "Tuple.IndexedList.unzip3 must return the third components"
        )

    [<TestMethod>]
    member _.``zip returns reference tuples`` () =
        let pairs : IndexedList<int * int> = IndexedList.zip list other

        CollectionAssert.AreEqual (
            Array.zip Inputs.Several Inputs.Other,
            IndexedList.toArray pairs,
            "Tuple.IndexedList.zip must zip as List.zip does"
        )

    [<TestMethod>]
    member _.``zip and zip3 throw for different lengths as the defaults do`` () =
        let short = IndexedList.singleton 1

        ExceptionAssert.Same (
            id,
            (fun () ->
                FSharp.Collections.Immutable.IndexedList.zip list short
                |> ignore
            ),
            (fun () -> IndexedList.zip list short |> ignore),
            "Tuple.IndexedList.zip must throw as the default zip does"
        )

        ExceptionAssert.Same (
            id,
            (fun () ->
                FSharp.Collections.Immutable.IndexedList.zip3 list short other
                |> ignore
            ),
            (fun () -> IndexedList.zip3 list short other |> ignore),
            "Tuple.IndexedList.zip3 must throw as the default zip3 does"
        )

    [<TestMethod>]
    member _.``zip3 returns reference tuples`` () =
        let triples : IndexedList<int * int * int> = IndexedList.zip3 list other third

        CollectionAssert.AreEqual (
            Array.zip3 Inputs.Several Inputs.Other Inputs.Third,
            IndexedList.toArray triples,
            "Tuple.IndexedList.zip3 must zip as List.zip3 does"
        )

    [<TestMethod>]
    member _.``mapFold takes and returns reference tuples`` () =
        let (results : IndexedList<int>), (state : int) =
            IndexedList.mapFold (fun state item -> item * state, state + 1) 1 list

        let expectedResults, expectedState =
            Array.mapFold (fun state item -> item * state, state + 1) 1 Inputs.Several

        CollectionAssert.AreEqual (
            expectedResults,
            IndexedList.toArray results,
            "Tuple.IndexedList.mapFold must map as List.mapFold does"
        )
        Assert.AreEqual<int>(expectedState, state, "Tuple.IndexedList.mapFold must thread the state as List.mapFold does")

    [<TestMethod>]
    member _.``mapFoldBack takes and returns reference tuples`` () =
        let (results : IndexedList<int>), (state : int) =
            IndexedList.mapFoldBack (fun item state -> item * state, state + 1) list 1

        let expectedResults, expectedState =
            Array.mapFoldBack (fun item state -> item * state, state + 1) Inputs.Several 1

        CollectionAssert.AreEqual (
            expectedResults,
            IndexedList.toArray results,
            "Tuple.IndexedList.mapFoldBack must map as List.mapFoldBack does"
        )

        Assert.AreEqual<int>(expectedState, state, "Tuple.IndexedList.mapFoldBack must thread the state as List.mapFoldBack does")

    [<TestMethod>]
    member _.``countBy returns reference tuples`` () =
        let counts : IndexedList<int * int> = IndexedList.countBy (fun item -> item % 3) list

        CollectionAssert.AreEqual (
            Array.countBy (fun item -> item % 3) Inputs.Several,
            IndexedList.toArray counts,
            "Tuple.IndexedList.countBy must count as List.countBy does"
        )

    [<TestMethod>]
    member _.``groupBy returns reference tuples`` () =
        let groups : IndexedList<int * IndexedList<int>> = IndexedList.groupBy (fun item -> item % 3) list
        let expected = Array.groupBy (fun item -> item % 3) Inputs.Several

        // A group pairs its key with its elements, which a pair compares by reference, so the keys and the elements are
        // compared apart
        CollectionAssert.AreEqual (
            Array.map fst expected,
            groups |> Seq.map fst |> Seq.toArray,
            "Tuple.IndexedList.groupBy must return the keys as List.groupBy does"
        )

        CollectionAssert.AreEqual (
            Array.map snd expected,
            groups
            |> Seq.map (fun (_, items) -> IndexedList.toArray items)
            |> Seq.toArray,
            "Tuple.IndexedList.groupBy must group the elements as List.groupBy does"
        )

    [<TestMethod>]
    member _.``partition returns a reference tuple`` () =
        let (accepted : IndexedList<int>), (rejected : IndexedList<int>) =
            IndexedList.partition (fun item -> item > 0) list
        let expectedAccepted, expectedRejected = Array.partition (fun item -> item > 0) Inputs.Several
        CollectionAssert.AreEqual (
            expectedAccepted,
            IndexedList.toArray accepted,
            "Tuple.IndexedList.partition must return the accepted elements"
        )
        CollectionAssert.AreEqual (
            expectedRejected,
            IndexedList.toArray rejected,
            "Tuple.IndexedList.partition must return the rejected elements"
        )

    [<TestMethod>]
    member _.``partitionWith returns a reference tuple`` () =
        let partitioner (item : int) =
            if item > 0 then
                Choice1Of2 item
            else
                Choice2Of2 (string item)

        let (first : IndexedList<int>), (second : IndexedList<string>) =
            IndexedList.partitionWith partitioner list
        let expectedFirst, expectedSecond = Array.partitionWith partitioner Inputs.Several
        CollectionAssert.AreEqual (
            expectedFirst,
            IndexedList.toArray first,
            "Tuple.IndexedList.partitionWith must return the first choices"
        )
        CollectionAssert.AreEqual (
            expectedSecond,
            IndexedList.toArray second,
            "Tuple.IndexedList.partitionWith must return the second choices"
        )

    [<TestMethod>]
    member _.``splitAt returns a reference tuple`` () =
        let (before : IndexedList<int>), (after : IndexedList<int>) = IndexedList.splitAt 3 list
        let expectedBefore, expectedAfter = Array.splitAt 3 Inputs.Several
        CollectionAssert.AreEqual (
            expectedBefore,
            IndexedList.toArray before,
            "Tuple.IndexedList.splitAt must return the first part"
        )
        CollectionAssert.AreEqual (
            expectedAfter,
            IndexedList.toArray after,
            "Tuple.IndexedList.splitAt must return the second part"
        )

    [<TestMethod>]
    member _.``the functions without a tuple keep resolving to the default module`` () =
        let found : int voption = IndexedList.tryFind (fun item -> item > 6) list
        Assert.AreEqual<int voption>(ValueSome 8, found, "IndexedList.tryFind must resolve to the default module")
