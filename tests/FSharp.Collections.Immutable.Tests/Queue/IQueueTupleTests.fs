namespace FSharp.Collections.Immutable.Tests

open FSharp.Collections.Immutable
open FSharp.Collections.Immutable.Tuple
open Microsoft.VisualStudio.TestTools.UnitTesting

// The type annotations of the results are the proof that the functions of the Tuple namespace shadow the default ones.

/// The `IQueue` module of the `Tuple` namespace.
[<TestClass; IQueueTestCategory; TupleNamespaceTestCategory; PatternMatchingTestCategory>]
type IQueueTupleTests () =

    static let several : IQueue<int> = Queue.ofArray Inputs.Several

    static let other : IQueue<int> = Queue.ofArray Inputs.Other

    static let third : IQueue<int> = Queue.ofArray Inputs.Third

    [<TestMethod>]
    member _.``tryUncons returns a value option of a reference tuple`` () =
        let found : (int * IQueue<int>) voption = IQueue.tryUncons several

        // A tuple compares an array by reference, so the first element and the other elements are compared apart
        match found with
        | ValueSome (head, tail) ->
            Assert.AreEqual<int>(Array.head Inputs.Several, head, "Tuple.IQueue.tryUncons must return the first element")

            CollectionAssert.AreEqual (
                Array.tail Inputs.Several,
                IQueue.toArray tail,
                "Tuple.IQueue.tryUncons must return the other elements"
            )
        | ValueNone -> Assert.Fail "Tuple.IQueue.tryUncons must return the elements of a queue with elements"

    [<TestMethod>]
    member _.``uncons returns a reference tuple`` () =
        let (head : int), (tail : IQueue<int>) = IQueue.uncons several
        Assert.AreEqual<int>(Array.head Inputs.Several, head, "Tuple.IQueue.uncons must return the first element")
        CollectionAssert.AreEqual (
            Array.tail Inputs.Several,
            IQueue.toArray tail,
            "Tuple.IQueue.uncons must return the other elements"
        )

    [<TestMethod>]
    member _.``Cons is a struct partial pattern of reference tuples`` () =
        let pattern : IQueue<int> -> (int * IQueue<int>) voption = IQueue.(|Cons|_|)

        match several with
        | IQueue.Cons (head, tail) ->
            Assert.AreEqual<int>(Array.head Inputs.Several, head, "Tuple.IQueue.Cons must match the first element")
            CollectionAssert.AreEqual (
                Array.tail Inputs.Several,
                IQueue.toArray tail,
                "Tuple.IQueue.Cons must match the other elements"
            )
        | _ -> Assert.Fail "Tuple.IQueue.Cons must match a queue with elements"

        Assert.IsTrue ((pattern (IQueue.clear several)).IsNone, "Tuple.IQueue.Cons must not match an empty queue")

    [<TestMethod>]
    member _.``allPairs returns reference tuples`` () =
        let pairs : Queue<int * int> = IQueue.allPairs several other

        CollectionAssert.AreEqual (
            Array.allPairs Inputs.Several Inputs.Other,
            Queue.toArray pairs,
            "Tuple.IQueue.allPairs must pair as Array.allPairs does"
        )

    [<TestMethod>]
    member _.``indexed returns reference tuples`` () =
        let pairs : Queue<int * int> = IQueue.indexed several

        CollectionAssert.AreEqual (
            Array.indexed Inputs.Several,
            Queue.toArray pairs,
            "Tuple.IQueue.indexed must index as Array.indexed does"
        )

    [<TestMethod>]
    member _.``pairwise returns reference tuples`` () =
        let pairs : Queue<int * int> = IQueue.pairwise several

        CollectionAssert.AreEqual (
            Array.pairwise Inputs.Several,
            Queue.toArray pairs,
            "Tuple.IQueue.pairwise must pair as Array.pairwise does"
        )

    [<TestMethod>]
    member _.``unzip takes and returns reference tuples`` () =
        let pairs : IQueue<int * int> = IQueue.zip several other
        let (first : Queue<int>), (second : Queue<int>) = IQueue.unzip pairs
        CollectionAssert.AreEqual (Inputs.Several, Queue.toArray first, "Tuple.IQueue.unzip must return the first components")
        CollectionAssert.AreEqual (Inputs.Other, Queue.toArray second, "Tuple.IQueue.unzip must return the second components")

    [<TestMethod>]
    member _.``unzip3 takes and returns reference tuples`` () =
        let triples : IQueue<int * int * int> = IQueue.zip3 several other third
        let (first : Queue<int>), (second : Queue<int>), (last : Queue<int>) = IQueue.unzip3 triples
        CollectionAssert.AreEqual (Inputs.Several, Queue.toArray first, "Tuple.IQueue.unzip3 must return the first components")
        CollectionAssert.AreEqual (Inputs.Other, Queue.toArray second, "Tuple.IQueue.unzip3 must return the second components")
        CollectionAssert.AreEqual (Inputs.Third, Queue.toArray last, "Tuple.IQueue.unzip3 must return the third components")

    [<TestMethod>]
    member _.``zip returns reference tuples`` () =
        let pairs : Queue<int * int> = IQueue.zip several other

        CollectionAssert.AreEqual (
            Array.zip Inputs.Several Inputs.Other,
            Queue.toArray pairs,
            "Tuple.IQueue.zip must zip as Array.zip does"
        )

    [<TestMethod>]
    member _.``zip and zip3 throw for different lengths as the defaults do`` () =
        let short : IQueue<int> = Queue.singleton 1

        ExceptionAssert.Same (
            id,
            (fun () ->
                FSharp.Collections.Immutable.IQueue.zip several short
                |> ignore
            ),
            (fun () -> IQueue.zip several short |> ignore),
            "Tuple.IQueue.zip must throw as the default zip does"
        )

        ExceptionAssert.Same (
            id,
            (fun () ->
                FSharp.Collections.Immutable.IQueue.zip3 several short other
                |> ignore
            ),
            (fun () -> IQueue.zip3 several short other |> ignore),
            "Tuple.IQueue.zip3 must throw as the default zip3 does"
        )

    [<TestMethod>]
    member _.``zip3 returns reference tuples`` () =
        let triples : Queue<int * int * int> = IQueue.zip3 several other third

        CollectionAssert.AreEqual (
            Array.zip3 Inputs.Several Inputs.Other Inputs.Third,
            Queue.toArray triples,
            "Tuple.IQueue.zip3 must zip as Array.zip3 does"
        )

    [<TestMethod>]
    member _.``mapFold takes and returns reference tuples`` () =
        let (results : Queue<int>), (state : int) =
            IQueue.mapFold (fun state item -> item * state, state + 1) 1 several

        let expectedResults, expectedState =
            Array.mapFold (fun state item -> item * state, state + 1) 1 Inputs.Several

        CollectionAssert.AreEqual (expectedResults, Queue.toArray results, "Tuple.IQueue.mapFold must map as Array.mapFold does")
        Assert.AreEqual<int>(expectedState, state, "Tuple.IQueue.mapFold must thread the state as Array.mapFold does")

    [<TestMethod>]
    member _.``mapFoldBack takes and returns reference tuples`` () =
        let (results : Queue<int>), (state : int) =
            IQueue.mapFoldBack (fun item state -> item * state, state + 1) several 1

        let expectedResults, expectedState =
            Array.mapFoldBack (fun item state -> item * state, state + 1) Inputs.Several 1

        CollectionAssert.AreEqual (
            expectedResults,
            Queue.toArray results,
            "Tuple.IQueue.mapFoldBack must map as Array.mapFoldBack does"
        )
        Assert.AreEqual<int>(expectedState, state, "Tuple.IQueue.mapFoldBack must thread the state as Array.mapFoldBack does")

    [<TestMethod>]
    member _.``countBy returns reference tuples`` () =
        let counts : Queue<int * int> = IQueue.countBy (fun item -> item % 3) several

        CollectionAssert.AreEqual (
            Array.countBy (fun item -> item % 3) Inputs.Several,
            Queue.toArray counts,
            "Tuple.IQueue.countBy must count as Array.countBy does"
        )

    [<TestMethod>]
    member _.``groupBy returns reference tuples`` () =
        let groups : Queue<int * Queue<int>> = IQueue.groupBy (fun item -> item % 3) several

        let expected = Array.groupBy (fun item -> item % 3) Inputs.Several

        // A group pairs its key with its elements, which a pair compares by reference, so the keys and the elements are
        // compared apart
        CollectionAssert.AreEqual (
            Array.map fst expected,
            groups |> Queue.toArray |> Array.map fst,
            "Tuple.IQueue.groupBy must return the keys as List.groupBy does"
        )

        CollectionAssert.AreEqual (
            Array.map snd expected,
            groups
            |> Queue.toArray
            |> Array.map (fun (_, items) -> Queue.toArray items),
            "Tuple.IQueue.groupBy must group the elements as List.groupBy does"
        )

    [<TestMethod>]
    member _.``partition returns a reference tuple`` () =
        let (accepted : IQueue<int>), (rejected : IQueue<int>) = IQueue.partition (fun item -> item > 0) several
        let expectedAccepted, expectedRejected = Array.partition (fun item -> item > 0) Inputs.Several
        CollectionAssert.AreEqual (
            expectedAccepted,
            IQueue.toArray accepted,
            "Tuple.IQueue.partition must return the accepted elements"
        )
        CollectionAssert.AreEqual (
            expectedRejected,
            IQueue.toArray rejected,
            "Tuple.IQueue.partition must return the rejected elements"
        )

    [<TestMethod>]
    member _.``partitionWith returns a reference tuple`` () =
        let partitioner (item : int) =
            if item > 0 then
                Choice1Of2 item
            else
                Choice2Of2 (string item)

        let (first : Queue<int>), (second : Queue<string>) = IQueue.partitionWith partitioner several
        let expectedFirst, expectedSecond = Array.partitionWith partitioner Inputs.Several
        CollectionAssert.AreEqual (expectedFirst, Queue.toArray first, "Tuple.IQueue.partitionWith must return the first choices")
        CollectionAssert.AreEqual (
            expectedSecond,
            Queue.toArray second,
            "Tuple.IQueue.partitionWith must return the second choices"
        )

    [<TestMethod>]
    member _.``splitAt returns a reference tuple`` () =
        let (before : IQueue<int>), (after : IQueue<int>) = IQueue.splitAt 3 several
        let expectedBefore, expectedAfter = Array.splitAt 3 Inputs.Several
        CollectionAssert.AreEqual (expectedBefore, IQueue.toArray before, "Tuple.IQueue.splitAt must return the first part")
        CollectionAssert.AreEqual (expectedAfter, IQueue.toArray after, "Tuple.IQueue.splitAt must return the second part")

    [<TestMethod>]
    member _.``the functions without a tuple keep resolving to the default module`` () =
        let found : int voption = IQueue.tryFind (fun item -> item > 6) several
        Assert.AreEqual<int voption>(ValueSome 8, found, "IQueue.tryFind must resolve to the default module")

    [<TestMethod>]
    [<DataRow(nameof IQueue.tryUncons, "TryPeekDequeue")>]
    [<DataRow(nameof IQueue.uncons, "PeekDequeue")>]
    member _.``the queue functions compile with the names of the queue operations of the BCL``
        (name : string, compiledName : string)
        =
        Assert.AreEqual<string | null>(
            name,
            QueueOps.sourceName "FSharp.Collections.Immutable.Tuple.IQueue" compiledName,
            "Tuple.IQueue must compile the function under the name of its queue operation in the BCL"
        )
