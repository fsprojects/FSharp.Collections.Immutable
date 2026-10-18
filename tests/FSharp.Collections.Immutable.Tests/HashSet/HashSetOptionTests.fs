namespace FSharp.Collections.Immutable.Tests

open FSharp.Collections.Immutable
open FSharp.Collections.Immutable.Tuple
open FSharp.Collections.Immutable.Option
open Microsoft.VisualStudio.TestTools.UnitTesting

// The namespaces are opened in the order that gives the shapes of FSharp.Core: Tuple first, Option last.
// The type annotations of the results are the proof that the functions of the Option namespace shadow the default ones.

/// The `HashSet` module of the `Option` namespace.
[<TestClass; HashSetTestCategory; OptionNamespaceTestCategory>]
type HashSetOptionTests () =

    static let numbers = HashSet.ofArray Inputs.Several

    static let negatedIfNegative (item : int) = if item < 0 then Some (-item) else None

    [<TestMethod; DataRow 5; DataRow 1000>]
    member _.``tryGetValue returns an option`` (value : int) =
        let found : int option = HashSet.tryGetValue value numbers

        OverrideAssert.OptionMatches (
            FSharp.Collections.Immutable.HashSet.tryGetValue value numbers,
            found,
            "Option.HashSet.tryGetValue must match the default"
        )

    [<TestMethod>]
    member _.``choose takes a chooser of options`` () =
        let chosen : HashSet<int> = HashSet.choose negatedIfNegative numbers

        CollectionAssert.AreEqual (
            Inputs.Several
            |> Array.choose negatedIfNegative
            |> Array.distinct
            |> Array.sort,
            HashSet.toArray chosen |> Array.sort,
            "Option.HashSet.choose must keep the distinct values that Array.choose chooses"
        )

    [<TestMethod>]
    member _.``the functions without an option keep resolving to the default module`` () =
        let mapped : HashSet<int> = HashSet.map (fun item -> item * 2) numbers
        let (accepted : HashSet<int>), (rejected : HashSet<int>) =
            HashSet.partition (fun item -> item > 0) numbers

        CollectionAssert.AreEqual (
            Inputs.Several
            |> Array.map (fun item -> item * 2)
            |> Array.distinct
            |> Array.sort,
            HashSet.toArray mapped |> Array.sort,
            "HashSet.map must resolve to the default module"
        )

        Assert.AreEqual<int>(
            HashSet.count numbers,
            HashSet.count accepted + HashSet.count rejected,
            "HashSet.partition must resolve to the Tuple module"
        )
