namespace FSharp.Collections.Immutable.Tests

open System.Collections.Immutable
open FSharp.Collections.Immutable
open FSharp.Collections.Immutable.Tuple
open FSharp.Collections.Immutable.Option
open Microsoft.VisualStudio.TestTools.UnitTesting

// The namespaces are opened in the order that gives the shapes of FSharp.Core: Tuple first, Option last.
// The type annotations of the results are the proof that the functions of the Option namespace shadow the default ones.

/// The `ISet` module of the `Option` namespace.
[<TestClass; ISetTestCategory; OptionNamespaceTestCategory>]
type ISetOptionTests () =

    static let numbers = HashSet.ofArray Inputs.Several :> IImmutableSet<int>

    static let negatedIfNegative (item : int) = if item < 0 then Some (-item) else None

    [<TestMethod; DataRow 5; DataRow 1000>]
    member _.``tryGetValue returns an option`` (value : int) =
        let found : int option = ISet.tryGetValue value numbers

        OverrideAssert.OptionMatches (
            FSharp.Collections.Immutable.ISet.tryGetValue value numbers,
            found,
            "Option.ISet.tryGetValue must match the default"
        )

    [<TestMethod>]
    member _.``choose takes a chooser of options`` () =
        let chosen : HashSet<int> = ISet.choose negatedIfNegative numbers

        CollectionAssert.AreEqual (
            Inputs.Several
            |> Array.choose negatedIfNegative
            |> Array.distinct
            |> Array.sort,
            HashSet.toArray chosen |> Array.sort,
            "Option.ISet.choose must keep the distinct values that Array.choose chooses"
        )

    [<TestMethod>]
    member _.``the functions without an option keep resolving to the default module`` () =
        let filtered : IImmutableSet<int> = ISet.filter (fun item -> item > 0) numbers
        let (accepted : IImmutableSet<int>), (rejected : IImmutableSet<int>) =
            ISet.partition (fun item -> item > 0) numbers

        CollectionAssert.AreEqual (
            Inputs.Several
            |> Array.filter (fun item -> item > 0)
            |> Array.distinct
            |> Array.sort,
            ISet.toArray filtered |> Array.sort,
            "ISet.filter must resolve to the default module"
        )

        Assert.AreEqual<int>(
            ISet.count numbers,
            ISet.count accepted + ISet.count rejected,
            "ISet.partition must resolve to the Tuple module"
        )
