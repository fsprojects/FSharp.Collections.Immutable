namespace FSharp.Collections.Immutable.Tests

open FSharp.Collections.Immutable
open FSharp.Collections.Immutable.Tuple
open Microsoft.VisualStudio.TestTools.UnitTesting

// The type annotations of the results are the proof that the functions of the Tuple namespace shadow the default ones.

/// The `IIndexedList` module of the `Tuple` namespace.
[<TestClass; IIndexedListTestCategory; TupleNamespaceTestCategory; PatternMatchingTestCategory>]
type IIndexedListTupleTests () =

    static let list : IIndexedList<int> = IndexedList.ofArray Inputs.Several

    static let other : IIndexedList<int> = IndexedList.ofArray Inputs.Other

    static let third : IIndexedList<int> = IndexedList.ofArray Inputs.Third

    [<TestMethod>]
    member _.``tryUncons returns a value option of a reference tuple`` () =
        let found : (int * IIndexedList<int>) voption = IIndexedList.tryUncons list

        // A tuple compares an array by reference, so the first element and the other elements are compared apart
        let head, tail =
            Assert.WantValueSome (found, "Tuple.IIndexedList.tryUncons must return the elements of a list with elements")

        Assert.AreEqual<int>(Array.head Inputs.Several, head, "Tuple.IIndexedList.tryUncons must return the first element")

        CollectionAssert.AreEqual (
            Array.tail Inputs.Several,
            IIndexedList.toArray tail,
            "Tuple.IIndexedList.tryUncons must return the other elements"
        )

    [<TestMethod>]
    member _.``uncons returns a reference tuple`` () =
        let (head : int), (tail : IIndexedList<int>) = IIndexedList.uncons list
        Assert.AreEqual<int>(Array.head Inputs.Several, head, "Tuple.IIndexedList.uncons must return the first element")
        CollectionAssert.AreEqual (
            Array.tail Inputs.Several,
            IIndexedList.toArray tail,
            "Tuple.IIndexedList.uncons must return the other elements"
        )

    [<TestMethod>]
    member _.``Cons is a struct partial pattern of reference tuples`` () =
        let pattern : IIndexedList<int> -> (int * IIndexedList<int>) voption = IIndexedList.(|Cons|_|)

        match list with
        | IIndexedList.Cons (head, tail) ->
            Assert.AreEqual<int>(Array.head Inputs.Several, head, "Tuple.IIndexedList.Cons must match the first element")
            CollectionAssert.AreEqual (
                Array.tail Inputs.Several,
                IIndexedList.toArray tail,
                "Tuple.IIndexedList.Cons must match the other elements"
            )
        | _ -> Assert.Fail "Tuple.IIndexedList.Cons must match a list with elements"

        Assert.IsTrue ((pattern (IIndexedList.clear list)).IsNone, "Tuple.IIndexedList.Cons must not match an empty list")

    [<TestMethod>]
    member _.``allPairs returns reference tuples`` () =
        let pairs : IndexedList<int * int> = IIndexedList.allPairs list other

        CollectionAssert.AreEqual (
            Array.allPairs Inputs.Several Inputs.Other,
            IndexedList.toArray pairs,
            "Tuple.IIndexedList.allPairs must pair as List.allPairs does"
        )

    [<TestMethod>]
    member _.``indexed returns reference tuples`` () =
        let pairs : IndexedList<int * int> = IIndexedList.indexed list

        CollectionAssert.AreEqual (
            Array.indexed Inputs.Several,
            IndexedList.toArray pairs,
            "Tuple.IIndexedList.indexed must index as List.indexed does"
        )

    [<TestMethod>]
    member _.``pairwise returns reference tuples`` () =
        let pairs : IndexedList<int * int> = IIndexedList.pairwise list

        CollectionAssert.AreEqual (
            Array.pairwise Inputs.Several,
            IndexedList.toArray pairs,
            "Tuple.IIndexedList.pairwise must pair as List.pairwise does"
        )

    [<TestMethod>]
    member _.``unzip takes and returns reference tuples`` () =
        let pairs : IIndexedList<int * int> = IIndexedList.zip list other
        let (first : IndexedList<int>), (second : IndexedList<int>) = IIndexedList.unzip pairs
        CollectionAssert.AreEqual (
            Inputs.Several,
            IndexedList.toArray first,
            "Tuple.IIndexedList.unzip must return the first components"
        )
        CollectionAssert.AreEqual (
            Inputs.Other,
            IndexedList.toArray second,
            "Tuple.IIndexedList.unzip must return the second components"
        )

    [<TestMethod>]
    member _.``unzip3 takes and returns reference tuples`` () =
        let triples : IIndexedList<int * int * int> = IIndexedList.zip3 list other third
        let (first : IndexedList<int>), (second : IndexedList<int>), (last : IndexedList<int>) =
            IIndexedList.unzip3 triples
        CollectionAssert.AreEqual (
            Inputs.Several,
            IndexedList.toArray first,
            "Tuple.IIndexedList.unzip3 must return the first components"
        )
        CollectionAssert.AreEqual (
            Inputs.Other,
            IndexedList.toArray second,
            "Tuple.IIndexedList.unzip3 must return the second components"
        )
        CollectionAssert.AreEqual (
            Inputs.Third,
            IndexedList.toArray last,
            "Tuple.IIndexedList.unzip3 must return the third components"
        )

    [<TestMethod>]
    member _.``zip returns reference tuples`` () =
        let pairs : IndexedList<int * int> = IIndexedList.zip list other

        CollectionAssert.AreEqual (
            Array.zip Inputs.Several Inputs.Other,
            IndexedList.toArray pairs,
            "Tuple.IIndexedList.zip must zip as List.zip does"
        )

    [<TestMethod>]
    member _.``zip and zip3 throw for different lengths as the defaults do`` () =
        let short : IIndexedList<int> = IndexedList.singleton 1

        ExceptionAssert.Same (
            id,
            (fun () ->
                FSharp.Collections.Immutable.IIndexedList.zip list short
                |> ignore
            ),
            (fun () -> IIndexedList.zip list short |> ignore),
            "Tuple.IIndexedList.zip must throw as the default zip does"
        )

        ExceptionAssert.Same (
            id,
            (fun () ->
                FSharp.Collections.Immutable.IIndexedList.zip3 list short other
                |> ignore
            ),
            (fun () -> IIndexedList.zip3 list short other |> ignore),
            "Tuple.IIndexedList.zip3 must throw as the default zip3 does"
        )

    [<TestMethod>]
    member _.``zip3 returns reference tuples`` () =
        let triples : IndexedList<int * int * int> = IIndexedList.zip3 list other third

        CollectionAssert.AreEqual (
            Array.zip3 Inputs.Several Inputs.Other Inputs.Third,
            IndexedList.toArray triples,
            "Tuple.IIndexedList.zip3 must zip as List.zip3 does"
        )

    [<TestMethod>]
    member _.``mapFold takes and returns reference tuples`` () =
        let (results : IndexedList<int>), (state : int) =
            IIndexedList.mapFold (fun state item -> item * state, state + 1) 1 list

        let expectedResults, expectedState =
            Array.mapFold (fun state item -> item * state, state + 1) 1 Inputs.Several

        CollectionAssert.AreEqual (
            expectedResults,
            IndexedList.toArray results,
            "Tuple.IIndexedList.mapFold must map as List.mapFold does"
        )
        Assert.AreEqual<int>(expectedState, state, "Tuple.IIndexedList.mapFold must thread the state as List.mapFold does")

    [<TestMethod>]
    member _.``mapFoldBack takes and returns reference tuples`` () =
        let (results : IndexedList<int>), (state : int) =
            IIndexedList.mapFoldBack (fun item state -> item * state, state + 1) list 1

        let expectedResults, expectedState =
            Array.mapFoldBack (fun item state -> item * state, state + 1) Inputs.Several 1

        CollectionAssert.AreEqual (
            expectedResults,
            IndexedList.toArray results,
            "Tuple.IIndexedList.mapFoldBack must map as List.mapFoldBack does"
        )

        Assert.AreEqual<int>(
            expectedState,
            state,
            "Tuple.IIndexedList.mapFoldBack must thread the state as List.mapFoldBack does"
        )

    [<TestMethod>]
    member _.``countBy returns reference tuples`` () =
        let counts : IndexedList<int * int> = IIndexedList.countBy (fun item -> item % 3) list

        CollectionAssert.AreEqual (
            Array.countBy (fun item -> item % 3) Inputs.Several,
            IndexedList.toArray counts,
            "Tuple.IIndexedList.countBy must count as List.countBy does"
        )

    [<TestMethod>]
    member _.``groupBy returns reference tuples`` () =
        let groups : IndexedList<int * IndexedList<int>> = IIndexedList.groupBy (fun item -> item % 3) list
        let expected = Array.groupBy (fun item -> item % 3) Inputs.Several

        // A group pairs its key with its elements, which a pair compares by reference, so the keys and the elements are
        // compared apart
        CollectionAssert.AreEqual (
            Array.map fst expected,
            groups |> Seq.map fst |> Seq.toArray,
            "Tuple.IIndexedList.groupBy must return the keys as List.groupBy does"
        )

        CollectionAssert.AreEqual (
            Array.map snd expected,
            groups
            |> Seq.map (fun (_, items) -> IndexedList.toArray items)
            |> Seq.toArray,
            "Tuple.IIndexedList.groupBy must group the elements as List.groupBy does"
        )

    [<TestMethod>]
    member _.``partition returns a reference tuple`` () =
        let (accepted : IIndexedList<int>), (rejected : IIndexedList<int>) =
            IIndexedList.partition (fun item -> item > 0) list

        let expectedAccepted, expectedRejected = Array.partition (fun item -> item > 0) Inputs.Several
        CollectionAssert.AreEqual (
            expectedAccepted,
            IIndexedList.toArray accepted,
            "Tuple.IIndexedList.partition must return the accepted elements"
        )
        CollectionAssert.AreEqual (
            expectedRejected,
            IIndexedList.toArray rejected,
            "Tuple.IIndexedList.partition must return the rejected elements"
        )

    [<TestMethod>]
    member _.``partitionWith returns a reference tuple`` () =
        let partitioner (item : int) =
            if item > 0 then
                Choice1Of2 item
            else
                Choice2Of2 (string item)

        let (first : IndexedList<int>), (second : IndexedList<string>) =
            IIndexedList.partitionWith partitioner list
        let expectedFirst, expectedSecond = Array.partitionWith partitioner Inputs.Several
        CollectionAssert.AreEqual (
            expectedFirst,
            IndexedList.toArray first,
            "Tuple.IIndexedList.partitionWith must return the first choices"
        )
        CollectionAssert.AreEqual (
            expectedSecond,
            IndexedList.toArray second,
            "Tuple.IIndexedList.partitionWith must return the second choices"
        )

    [<TestMethod>]
    member _.``splitAt returns a reference tuple`` () =
        let (before : IIndexedList<int>), (after : IIndexedList<int>) = IIndexedList.splitAt 3 list
        let expectedBefore, expectedAfter = Array.splitAt 3 Inputs.Several
        CollectionAssert.AreEqual (
            expectedBefore,
            IIndexedList.toArray before,
            "Tuple.IIndexedList.splitAt must return the first part"
        )
        CollectionAssert.AreEqual (
            expectedAfter,
            IIndexedList.toArray after,
            "Tuple.IIndexedList.splitAt must return the second part"
        )

    [<TestMethod>]
    member _.``the functions without a tuple keep resolving to the default module`` () =
        let found : int voption = IIndexedList.tryFind (fun item -> item > 6) list
        Assert.AreEqual<int voption>(ValueSome 8, found, "IIndexedList.tryFind must resolve to the default module")
