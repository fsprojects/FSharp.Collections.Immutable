namespace FSharp.Collections.Immutable.Tests

open FSharp.Collections.Immutable
open FSharp.Collections.Immutable.Tuple
open FSharp.Collections.Immutable.Option
open Microsoft.VisualStudio.TestTools.UnitTesting

// The namespaces are opened in the order that gives the shapes of FSharp.Core: Tuple first, Option last.
// The type annotations of the results are the proof that the functions of the Option namespace shadow the default ones.

/// The `SortedSet` module of the `Option` namespace.
[<TestClass; SortedSetTestCategory; OptionNamespaceTestCategory>]
type SortedSetOptionTests () =

    static let numbers = SortedSet.ofArray Inputs.Several

    static let negatedIfNegative (item : int) = if item < 0 then Some (-item) else None

    /// The indices of tryItem, as rows: before the first element, the first one, one inside and the number of elements.
    static member Indices = seq {
        -1
        0
        3
        (Set.ofArray Inputs.Several).Count
    }

    [<TestMethod; DataRow 5; DataRow 1000>]
    member _.``tryGetValue returns an option`` (value : int) =
        let found : int option = SortedSet.tryGetValue value numbers

        OverrideAssert.OptionMatches (
            FSharp.Collections.Immutable.SortedSet.tryGetValue value numbers,
            found,
            "Option.SortedSet.tryGetValue must match the default"
        )

    [<TestMethod; DynamicData(nameof SortedSetOptionTests.Indices)>]
    member _.``tryItem returns an option`` (index : int) =
        let found : int option = SortedSet.tryItem index numbers

        OverrideAssert.OptionMatches (
            FSharp.Collections.Immutable.SortedSet.tryItem index numbers,
            found,
            "Option.SortedSet.tryItem must match the default"
        )

    [<TestMethod>]
    member _.``choose takes a chooser of options`` () =
        let chosen : SortedSet<int> = SortedSet.choose negatedIfNegative numbers

        CollectionAssert.AreEqual (
            Set.toArray (Set.ofArray (Array.choose negatedIfNegative Inputs.Several)),
            SortedSet.toArray chosen,
            "Option.SortedSet.choose must keep the distinct values that Array.choose chooses, in the order of Set"
        )

    [<TestMethod>]
    member _.``the functions without an option keep resolving to the default module`` () =
        let mapped : SortedSet<int> = SortedSet.map (fun item -> item * 2) numbers
        let (accepted : SortedSet<int>), (rejected : SortedSet<int>) =
            SortedSet.partition (fun item -> item > 0) numbers

        CollectionAssert.AreEqual (
            Set.toArray (Set.map (fun item -> item * 2) (Set.ofArray Inputs.Several)),
            SortedSet.toArray mapped,
            "SortedSet.map must resolve to the default module"
        )

        Assert.AreEqual<int>(
            SortedSet.count numbers,
            SortedSet.count accepted + SortedSet.count rejected,
            "SortedSet.partition must resolve to the Tuple module"
        )
