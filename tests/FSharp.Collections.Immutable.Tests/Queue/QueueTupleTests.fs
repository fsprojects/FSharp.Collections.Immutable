namespace FSharp.Collections.Immutable.Tests

open FSharp.Collections.Immutable
open FSharp.Collections.Immutable.Tuple
open Microsoft.VisualStudio.TestTools.UnitTesting

// The type annotations of the results are the proof that the functions of the Tuple namespace shadow the default ones.

/// The `Queue` module of the `Tuple` namespace.
[<TestClass; QueueTestCategory; TupleNamespaceTestCategory; PatternMatchingTestCategory>]
type QueueTupleTests () =

    static let several = Queue.ofArray Inputs.Several

    static let other = Queue.ofArray Inputs.Other

    static let third = Queue.ofArray Inputs.Third

    [<TestMethod>]
    member _.``unfold takes a generator of value options of reference tuples`` () =
        let generator (state : int) =
            if state > 0 then
                ValueSome (state, state - 1)
            else
                ValueNone

        let generated : Queue<int> = Queue.unfold generator 3
        CollectionAssert.AreEqual ([| 3; 2; 1 |], Queue.toArray generated, "Tuple.Queue.unfold must generate the elements")

    [<TestMethod>]
    member _.``tryUncons returns a value option of a reference tuple`` () =
        let found : (int * Queue<int>) voption = Queue.tryUncons several

        // A tuple compares an array by reference, so the first element and the other elements are compared apart
        match found with
        | ValueSome (head, tail) ->
            Assert.AreEqual<int>(Array.head Inputs.Several, head, "Tuple.Queue.tryUncons must return the first element")

            CollectionAssert.AreEqual (
                Array.tail Inputs.Several,
                Queue.toArray tail,
                "Tuple.Queue.tryUncons must return the other elements"
            )
        | ValueNone -> Assert.Fail "Tuple.Queue.tryUncons must return the elements of a queue with elements"

    [<TestMethod>]
    member _.``uncons returns a reference tuple`` () =
        let (head : int), (tail : Queue<int>) = Queue.uncons several
        Assert.AreEqual<int>(Array.head Inputs.Several, head, "Tuple.Queue.uncons must return the first element")
        CollectionAssert.AreEqual (
            Array.tail Inputs.Several,
            Queue.toArray tail,
            "Tuple.Queue.uncons must return the other elements"
        )

    [<TestMethod>]
    member _.``uncons throws as the default does`` () =
        ExceptionAssert.Same (
            id,
            (fun () ->
                FSharp.Collections.Immutable.Queue.uncons Queue.empty<int>
                |> ignore
            ),
            (fun () -> Queue.uncons Queue.empty<int> |> ignore),
            "Tuple.Queue.uncons must throw as the default uncons does"
        )

    [<TestMethod>]
    member _.``Cons is a struct partial pattern of reference tuples`` () =
        let pattern : Queue<int> -> (int * Queue<int>) voption = Queue.(|Cons|_|)

        match several with
        | Queue.Cons (head, tail) ->
            Assert.AreEqual<int>(Array.head Inputs.Several, head, "Tuple.Queue.Cons must match the first element")
            CollectionAssert.AreEqual (
                Array.tail Inputs.Several,
                Queue.toArray tail,
                "Tuple.Queue.Cons must match the other elements"
            )
        | _ -> Assert.Fail "Tuple.Queue.Cons must match a queue with elements"

        Assert.IsTrue ((pattern Queue.empty).IsNone, "Tuple.Queue.Cons must not match an empty queue")

    [<TestMethod>]
    member _.``allPairs returns reference tuples`` () =
        let pairs : Queue<int * int> = Queue.allPairs several other

        CollectionAssert.AreEqual (
            Array.allPairs Inputs.Several Inputs.Other,
            Queue.toArray pairs,
            "Tuple.Queue.allPairs must pair as Array.allPairs does"
        )

    [<TestMethod>]
    member _.``indexed returns reference tuples`` () =
        let pairs : Queue<int * int> = Queue.indexed several

        CollectionAssert.AreEqual (
            Array.indexed Inputs.Several,
            Queue.toArray pairs,
            "Tuple.Queue.indexed must index as Array.indexed does"
        )

    [<TestMethod>]
    member _.``pairwise returns reference tuples`` () =
        let pairs : Queue<int * int> = Queue.pairwise several

        CollectionAssert.AreEqual (
            Array.pairwise Inputs.Several,
            Queue.toArray pairs,
            "Tuple.Queue.pairwise must pair as Array.pairwise does"
        )

    [<TestMethod>]
    member _.``unzip takes and returns reference tuples`` () =
        let (first : Queue<int>), (second : Queue<int>) = Queue.unzip (Queue.zip several other)
        CollectionAssert.AreEqual (Inputs.Several, Queue.toArray first, "Tuple.Queue.unzip must return the first components")
        CollectionAssert.AreEqual (Inputs.Other, Queue.toArray second, "Tuple.Queue.unzip must return the second components")

    [<TestMethod>]
    member _.``unzip3 takes and returns reference tuples`` () =
        let (first : Queue<int>), (second : Queue<int>), (last : Queue<int>) =
            Queue.unzip3 (Queue.zip3 several other third)

        CollectionAssert.AreEqual (Inputs.Several, Queue.toArray first, "Tuple.Queue.unzip3 must return the first components")
        CollectionAssert.AreEqual (Inputs.Other, Queue.toArray second, "Tuple.Queue.unzip3 must return the second components")
        CollectionAssert.AreEqual (Inputs.Third, Queue.toArray last, "Tuple.Queue.unzip3 must return the third components")

    [<TestMethod>]
    member _.``zip returns reference tuples`` () =
        let pairs : Queue<int * int> = Queue.zip several other

        CollectionAssert.AreEqual (
            Array.zip Inputs.Several Inputs.Other,
            Queue.toArray pairs,
            "Tuple.Queue.zip must zip as Array.zip does"
        )

    [<TestMethod>]
    member _.``zip and zip3 throw for different lengths as the defaults do`` () =
        let short = Queue.singleton 1

        ExceptionAssert.Same (
            id,
            (fun () ->
                FSharp.Collections.Immutable.Queue.zip several short
                |> ignore
            ),
            (fun () -> Queue.zip several short |> ignore),
            "Tuple.Queue.zip must throw as the default zip does"
        )

        ExceptionAssert.Same (
            id,
            (fun () ->
                FSharp.Collections.Immutable.Queue.zip3 several short other
                |> ignore
            ),
            (fun () -> Queue.zip3 several short other |> ignore),
            "Tuple.Queue.zip3 must throw as the default zip3 does"
        )

    [<TestMethod>]
    member _.``zip3 returns reference tuples`` () =
        let triples : Queue<int * int * int> = Queue.zip3 several other third

        CollectionAssert.AreEqual (
            Array.zip3 Inputs.Several Inputs.Other Inputs.Third,
            Queue.toArray triples,
            "Tuple.Queue.zip3 must zip as Array.zip3 does"
        )

    [<TestMethod>]
    member _.``mapFold takes and returns reference tuples`` () =
        let (results : Queue<int>), (state : int) =
            Queue.mapFold (fun state item -> item * state, state + 1) 1 several

        let expectedResults, expectedState =
            Array.mapFold (fun state item -> item * state, state + 1) 1 Inputs.Several

        CollectionAssert.AreEqual (expectedResults, Queue.toArray results, "Tuple.Queue.mapFold must map as Array.mapFold does")
        Assert.AreEqual<int>(expectedState, state, "Tuple.Queue.mapFold must thread the state as Array.mapFold does")

    [<TestMethod>]
    member _.``mapFoldBack takes and returns reference tuples`` () =
        let (results : Queue<int>), (state : int) =
            Queue.mapFoldBack (fun item state -> item * state, state + 1) several 1

        let expectedResults, expectedState =
            Array.mapFoldBack (fun item state -> item * state, state + 1) Inputs.Several 1

        CollectionAssert.AreEqual (
            expectedResults,
            Queue.toArray results,
            "Tuple.Queue.mapFoldBack must map as Array.mapFoldBack does"
        )
        Assert.AreEqual<int>(expectedState, state, "Tuple.Queue.mapFoldBack must thread the state as Array.mapFoldBack does")

    [<TestMethod>]
    member _.``countBy returns reference tuples`` () =
        let counts : Queue<int * int> = Queue.countBy (fun item -> item % 3) several

        CollectionAssert.AreEqual (
            Array.countBy (fun item -> item % 3) Inputs.Several,
            Queue.toArray counts,
            "Tuple.Queue.countBy must count as Array.countBy does"
        )

    [<TestMethod>]
    member _.``groupBy returns reference tuples`` () =
        let groups : Queue<int * Queue<int>> = Queue.groupBy (fun item -> item % 3) several

        let expected = Array.groupBy (fun item -> item % 3) Inputs.Several

        // A group pairs its key with its elements, which a pair compares by reference, so the keys and the elements are
        // compared apart
        CollectionAssert.AreEqual (
            Array.map fst expected,
            groups |> Queue.toArray |> Array.map fst,
            "Tuple.Queue.groupBy must return the keys as List.groupBy does"
        )

        CollectionAssert.AreEqual (
            Array.map snd expected,
            groups
            |> Queue.toArray
            |> Array.map (fun (_, items) -> Queue.toArray items),
            "Tuple.Queue.groupBy must group the elements as List.groupBy does"
        )

    [<TestMethod>]
    member _.``partition returns a reference tuple`` () =
        let (accepted : Queue<int>), (rejected : Queue<int>) = Queue.partition (fun item -> item > 0) several
        let expectedAccepted, expectedRejected = Array.partition (fun item -> item > 0) Inputs.Several
        CollectionAssert.AreEqual (
            expectedAccepted,
            Queue.toArray accepted,
            "Tuple.Queue.partition must return the accepted elements"
        )
        CollectionAssert.AreEqual (
            expectedRejected,
            Queue.toArray rejected,
            "Tuple.Queue.partition must return the rejected elements"
        )

    [<TestMethod>]
    member _.``partitionWith returns a reference tuple`` () =
        let partitioner (item : int) =
            if item > 0 then
                Choice1Of2 item
            else
                Choice2Of2 (string item)

        let (first : Queue<int>), (second : Queue<string>) = Queue.partitionWith partitioner several
        let expectedFirst, expectedSecond = Array.partitionWith partitioner Inputs.Several
        CollectionAssert.AreEqual (expectedFirst, Queue.toArray first, "Tuple.Queue.partitionWith must return the first choices")
        CollectionAssert.AreEqual (
            expectedSecond,
            Queue.toArray second,
            "Tuple.Queue.partitionWith must return the second choices"
        )

    [<TestMethod>]
    member _.``splitAt returns a reference tuple`` () =
        let (before : Queue<int>), (after : Queue<int>) = Queue.splitAt 3 several
        let expectedBefore, expectedAfter = Array.splitAt 3 Inputs.Several
        CollectionAssert.AreEqual (expectedBefore, Queue.toArray before, "Tuple.Queue.splitAt must return the first part")
        CollectionAssert.AreEqual (expectedAfter, Queue.toArray after, "Tuple.Queue.splitAt must return the second part")

    [<TestMethod>]
    member _.``the functions without a tuple keep resolving to the default module`` () =
        let found : int voption = Queue.tryFind (fun item -> item > 6) several
        Assert.AreEqual<int voption>(ValueSome 8, found, "Queue.tryFind must resolve to the default module")

    [<TestMethod>]
    [<DataRow(nameof Queue.tryUncons, "TryPeekDequeue")>]
    [<DataRow(nameof Queue.uncons, "PeekDequeue")>]
    member _.``the queue functions compile with the names of the queue operations of the BCL``
        (name : string, compiledName : string)
        =
        Assert.AreEqual<string | null>(
            name,
            QueueOps.sourceName "FSharp.Collections.Immutable.Tuple.Queue" compiledName,
            "Tuple.Queue must compile the function under the name of its queue operation in the BCL"
        )
