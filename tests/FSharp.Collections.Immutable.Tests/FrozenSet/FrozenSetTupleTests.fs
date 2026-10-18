namespace FSharp.Collections.Immutable.Tests

open System.Collections.Frozen
open FSharp.Collections.Immutable
open FSharp.Collections.Immutable.Tuple
open Microsoft.VisualStudio.TestTools.UnitTesting

// The type annotations of the results are the proof that the functions of the Tuple namespace shadow the default ones.

/// The `FrozenSet` module of the `Tuple` namespace.
[<TestClass; FrozenSetTestCategory; TupleNamespaceTestCategory>]
type FrozenSetTupleTests () =

    static let numbers = FrozenSet.ofArray Inputs.Several

    static let expected = Set.ofArray Inputs.Several

    [<TestMethod>]
    member _.``partition returns a reference tuple`` () =
        let (accepted : FrozenSet<int>), (rejected : FrozenSet<int>) =
            FrozenSet.partition (fun item -> item > 0) numbers
        let expectedAccepted, expectedRejected = Set.partition (fun item -> item > 0) expected

        CollectionAssert.AreEqual (
            Set.toArray expectedAccepted,
            FrozenSet.toArray accepted |> Array.sort,
            "Tuple.FrozenSet.partition must return the accepted elements"
        )

        CollectionAssert.AreEqual (
            Set.toArray expectedRejected,
            FrozenSet.toArray rejected |> Array.sort,
            "Tuple.FrozenSet.partition must return the rejected elements"
        )

    [<TestMethod>]
    member _.``partitionWith returns a reference tuple`` () =
        let partitioner (item : int) =
            if item > 0 then
                Choice1Of2 item
            else
                Choice2Of2 (string item)

        let (first : FrozenSet<int>), (second : FrozenSet<string>) = FrozenSet.partitionWith partitioner numbers
        let expectedFirst, expectedSecond = Set.partitionWith partitioner expected

        CollectionAssert.AreEqual (
            Set.toArray expectedFirst,
            FrozenSet.toArray first |> Array.sort,
            "Tuple.FrozenSet.partitionWith must return the first choices"
        )

        CollectionAssert.AreEqual (
            Set.toArray expectedSecond,
            FrozenSet.toArray second |> Array.sort,
            "Tuple.FrozenSet.partitionWith must return the second choices"
        )

    [<TestMethod>]
    member _.``the functions without a tuple keep resolving to the default module`` () =
        let found : int voption = FrozenSet.tryGetValue 8 numbers
        Assert.AreEqual<int voption>(ValueSome 8, found, "FrozenSet.tryGetValue must resolve to the default module")
