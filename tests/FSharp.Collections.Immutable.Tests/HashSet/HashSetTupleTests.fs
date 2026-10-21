namespace FSharp.Collections.Immutable.Tests

open FSharp.Collections.Immutable
open FSharp.Collections.Immutable.Tuple
open Microsoft.VisualStudio.TestTools.UnitTesting

// The type annotations of the results are the proof that the functions of the Tuple namespace shadow the default ones.

/// The `HashSet` module of the `Tuple` namespace.
[<TestClass; HashSetTestCategory; TupleNamespaceTestCategory>]
type HashSetTupleTests () =

    static let numbers = HashSet.ofArray Inputs.Several

    static let expected = Set.ofArray Inputs.Several

    [<TestMethod>]
    member _.``partition returns a reference tuple`` () =
        let (accepted : HashSet<int>), (rejected : HashSet<int>) =
            HashSet.partition (fun item -> item > 0) numbers
        let expectedAccepted, expectedRejected = Set.partition (fun item -> item > 0) expected

        CollectionAssert.AreEqual (
            Set.toArray expectedAccepted,
            HashSet.toArray accepted |> Array.sort,
            "Tuple.HashSet.partition must return the accepted elements"
        )

        CollectionAssert.AreEqual (
            Set.toArray expectedRejected,
            HashSet.toArray rejected |> Array.sort,
            "Tuple.HashSet.partition must return the rejected elements"
        )

    [<TestMethod>]
    member _.``partitionWith returns a reference tuple`` () =
        let partitioner (item : int) =
            if item > 0 then
                Choice1Of2 item
            else
                Choice2Of2 (string item)

        let (first : HashSet<int>), (second : HashSet<string>) = HashSet.partitionWith partitioner numbers
        let expectedFirst, expectedSecond = Set.partitionWith partitioner expected

        CollectionAssert.AreEqual (
            Set.toArray expectedFirst,
            HashSet.toArray first |> Array.sort,
            "Tuple.HashSet.partitionWith must return the first choices"
        )

        CollectionAssert.AreEqual (
            Set.toArray expectedSecond,
            HashSet.toArray second |> Array.sort,
            "Tuple.HashSet.partitionWith must return the second choices"
        )

    [<TestMethod>]
    member _.``the functions without a tuple keep resolving to the default module`` () =
        let found : int voption = HashSet.tryGetValue 8 numbers
        Assert.AreEqual<int voption>(ValueSome 8, found, "HashSet.tryGetValue must resolve to the default module")
