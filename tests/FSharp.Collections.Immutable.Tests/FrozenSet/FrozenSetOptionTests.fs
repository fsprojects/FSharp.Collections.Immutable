namespace FSharp.Collections.Immutable.Tests

open System.Collections.Frozen
open FSharp.Collections.Immutable
open FSharp.Collections.Immutable.Tuple
open FSharp.Collections.Immutable.Option
open Microsoft.VisualStudio.TestTools.UnitTesting

// The namespaces are opened in the order that gives the shapes of FSharp.Core: Tuple first, Option last.
// The type annotations of the results are the proof that the functions of the Option namespace shadow the default ones.

/// The `FrozenSet` module of the `Option` namespace.
[<TestClass; FrozenSetTestCategory; OptionNamespaceTestCategory>]
type FrozenSetOptionTests () =

    static let numbers = FrozenSet.ofArray Inputs.Several

    static let negatedIfNegative (item : int) = if item < 0 then Some (-item) else None

    [<TestMethod; DataRow 5; DataRow 1000>]
    member _.``tryGetValue returns an option`` (value : int) =
        let found : int option = FrozenSet.tryGetValue value numbers

        OverrideAssert.OptionMatches (
            FSharp.Collections.Immutable.FrozenSet.tryGetValue value numbers,
            found,
            "Option.FrozenSet.tryGetValue must match the default"
        )

    [<TestMethod>]
    member _.``choose takes a chooser of options`` () =
        let chosen : FrozenSet<int> = FrozenSet.choose negatedIfNegative numbers

        CollectionAssert.AreEqual (
            Inputs.Several
            |> Array.choose negatedIfNegative
            |> Array.distinct
            |> Array.sort,
            FrozenSet.toArray chosen |> Array.sort,
            "Option.FrozenSet.choose must keep the distinct values that Array.choose chooses"
        )

    [<TestMethod>]
    member _.``the functions without an option keep resolving to the default module`` () =
        let mapped : FrozenSet<int> = FrozenSet.map (fun item -> item * 2) numbers
        let (accepted : FrozenSet<int>), (rejected : FrozenSet<int>) =
            FrozenSet.partition (fun item -> item > 0) numbers

        CollectionAssert.AreEqual (
            Inputs.Several
            |> Array.map (fun item -> item * 2)
            |> Array.distinct
            |> Array.sort,
            FrozenSet.toArray mapped |> Array.sort,
            "FrozenSet.map must resolve to the default module"
        )

        Assert.AreEqual<int>(
            FrozenSet.count numbers,
            FrozenSet.count accepted + FrozenSet.count rejected,
            "FrozenSet.partition must resolve to the Tuple module"
        )

#if NET10_0_OR_GREATER
    [<TestMethod>]
    member _.``tryAlternateLookup returns an option`` () =
        let found : FrozenSet<int64>.AlternateLookup<int> option =
            FrozenSet.tryAlternateLookup<int, _>(FrozenSetOps.ofInt64WithInt32Comparer [| 1L; 2L; 3L |])

        match found with
        | Some lookup ->
            Assert.IsTrue (lookup.Contains 2, "Option.FrozenSet.tryAlternateLookup must return a lookup that finds 2")
        | None -> Assert.Fail "Option.FrozenSet.tryAlternateLookup must return the lookup of a comparer with the alternate type"

        let missing : FrozenSet<int64>.AlternateLookup<int> option =
            FrozenSet.tryAlternateLookup<int, _>(FrozenSet.ofArray [| 1L |])

        Assert.IsTrue (
            missing.IsNone,
            "Option.FrozenSet.tryAlternateLookup must return None for a comparer that does not compare the alternate type"
        )
#endif
