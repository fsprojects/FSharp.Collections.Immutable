namespace FSharp.Collections.Immutable.Tests

open FSharp.Collections.Immutable
open FSharp.Collections.Immutable.Tuple
open Microsoft.VisualStudio.TestTools.UnitTesting

// The type annotations of the results are the proof that the functions of the Tuple namespace shadow the default ones.

/// The `SortedSet` module of the `Tuple` namespace.
[<TestClass; SortedSetTestCategory; TupleNamespaceTestCategory>]
type SortedSetTupleTests () =

    static let numbers = SortedSet.ofArray Inputs.Several

    static let expected = Set.ofArray Inputs.Several

    [<TestMethod>]
    member _.``partition returns a reference tuple`` () =
        let (accepted : SortedSet<int>), (rejected : SortedSet<int>) =
            SortedSet.partition (fun item -> item > 0) numbers
        let expectedAccepted, expectedRejected = Set.partition (fun item -> item > 0) expected

        CollectionAssert.AreEqual (
            Set.toArray expectedAccepted,
            SortedSet.toArray accepted,
            "Tuple.SortedSet.partition must return the accepted elements in the order of Set"
        )

        CollectionAssert.AreEqual (
            Set.toArray expectedRejected,
            SortedSet.toArray rejected,
            "Tuple.SortedSet.partition must return the rejected elements in the order of Set"
        )

    [<TestMethod>]
    member _.``partitionWith returns a reference tuple`` () =
        let partitioner (item : int) =
            if item > 0 then
                Choice1Of2 item
            else
                Choice2Of2 (string item)

        let (first : SortedSet<int>), (second : SortedSet<string>) = SortedSet.partitionWith partitioner numbers
        let expectedFirst, expectedSecond = Set.partitionWith partitioner expected

        CollectionAssert.AreEqual (
            Set.toArray expectedFirst,
            SortedSet.toArray first,
            "Tuple.SortedSet.partitionWith must return the first choices in the order of Set"
        )

        CollectionAssert.AreEqual (
            Set.toArray expectedSecond,
            SortedSet.toArray second,
            "Tuple.SortedSet.partitionWith must return the second choices in the order of Set"
        )

    [<TestMethod>]
    member _.``the functions without a tuple keep resolving to the default module`` () =
        let found : int voption = SortedSet.tryGetValue 8 numbers
        Assert.AreEqual<int voption>(ValueSome 8, found, "SortedSet.tryGetValue must resolve to the default module")
