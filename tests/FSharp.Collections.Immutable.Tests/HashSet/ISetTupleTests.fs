namespace FSharp.Collections.Immutable.Tests

open System.Collections.Immutable
open FSharp.Collections.Immutable
open FSharp.Collections.Immutable.Tuple
open Microsoft.VisualStudio.TestTools.UnitTesting

// The type annotations of the results are the proof that the functions of the Tuple namespace shadow the default ones.

/// The `ISet` module of the `Tuple` namespace.
[<TestClass; ISetTestCategory; TupleNamespaceTestCategory>]
type ISetTupleTests () =

    static let numbers = HashSet.ofArray Inputs.Several :> IImmutableSet<int>

    static let expected = Set.ofArray Inputs.Several

    [<TestMethod>]
    member _.``partition returns a reference tuple`` () =
        let (accepted : IImmutableSet<int>), (rejected : IImmutableSet<int>) =
            ISet.partition (fun item -> item > 0) numbers
        let expectedAccepted, expectedRejected = Set.partition (fun item -> item > 0) expected

        CollectionAssert.AreEqual (
            Set.toArray expectedAccepted,
            ISet.toArray accepted |> Array.sort,
            "Tuple.ISet.partition must return the accepted elements"
        )

        CollectionAssert.AreEqual (
            Set.toArray expectedRejected,
            ISet.toArray rejected |> Array.sort,
            "Tuple.ISet.partition must return the rejected elements"
        )

    [<TestMethod>]
    member _.``partitionWith returns a reference tuple`` () =
        let partitioner (item : int) =
            if item > 0 then
                Choice1Of2 item
            else
                Choice2Of2 (string item)

        let (first : HashSet<int>), (second : HashSet<string>) = ISet.partitionWith partitioner numbers
        let expectedFirst, expectedSecond = Set.partitionWith partitioner expected

        CollectionAssert.AreEqual (
            Set.toArray expectedFirst,
            HashSet.toArray first |> Array.sort,
            "Tuple.ISet.partitionWith must return the first choices"
        )

        CollectionAssert.AreEqual (
            Set.toArray expectedSecond,
            HashSet.toArray second |> Array.sort,
            "Tuple.ISet.partitionWith must return the second choices"
        )

    [<TestMethod>]
    member _.``the functions without a tuple keep resolving to the default module`` () =
        let found : int voption = ISet.tryGetValue 8 numbers
        Assert.AreEqual<int voption>(ValueSome 8, found, "ISet.tryGetValue must resolve to the default module")
