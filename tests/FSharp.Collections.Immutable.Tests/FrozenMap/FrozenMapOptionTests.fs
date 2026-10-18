namespace FSharp.Collections.Immutable.Tests

open FSharp.Collections.Immutable
open FSharp.Collections.Immutable.Tuple
open FSharp.Collections.Immutable.Option
open Microsoft.VisualStudio.TestTools.UnitTesting

// The namespaces are opened in the order that gives the shapes of FSharp.Core: Tuple first, Option last.
// The type annotations of the results are the proof that the functions of the Option namespace shadow the default ones.

/// The `FrozenMap` module of the `Option` namespace.
[<TestClass; FrozenMapTestCategory; OptionNamespaceTestCategory>]
type FrozenMapOptionTests () =

    static let table = FSharp.Collections.Immutable.FrozenMap.ofSeq MapInputs.Several

    static let oracle = MapInputs.ToMap MapInputs.Several

    /// A chooser that returns a value for the bindings of negative keys only.
    static let negatedIfNegative (key : int) (value : string) = if key < 0 then Some (value.Length - key) else None

    /// The predicates of tryFindKey: one that some bindings satisfy and one that no binding satisfies.
    static member Predicates = seq {
        BindingPredicate.HasNegativeKey
        BindingPredicate.Never
    }

    [<TestMethod; DataRow 5; DataRow 1>]
    member _.``tryFind returns an option`` (key : int) =
        let found : string option = FrozenMap.tryFind key table

        OverrideAssert.OptionMatches (
            FSharp.Collections.Immutable.FrozenMap.tryFind key table,
            found,
            "Option.FrozenMap.tryFind must match the default"
        )

    [<TestMethod>]
    member _.``pick takes a chooser of options`` () =
        let picked : int = FrozenMap.pick negatedIfNegative table

        Assert.AreEqual<int>(
            FSharp.Collections.Immutable.FrozenMap.pick
                (fun key value -> negatedIfNegative key value |> ValueOption.ofOption)
                table,
            picked,
            "Option.FrozenMap.pick must pick as the default does"
        )

    [<TestMethod; DynamicData(nameof FrozenMapOptionTests.Predicates)>]
    member _.``tryFindKey returns an option`` (predicate : BindingPredicate) =
        let found : int option = FrozenMap.tryFindKey predicate.Matches table

        OverrideAssert.OptionMatches (
            FSharp.Collections.Immutable.FrozenMap.tryFindKey predicate.Matches table,
            found,
            "Option.FrozenMap.tryFindKey must match the default"
        )

    [<TestMethod>]
    member _.``tryPick takes a chooser of options and returns an option`` () =
        let found : int option = FrozenMap.tryPick negatedIfNegative table

        OverrideAssert.OptionMatches (
            FSharp.Collections.Immutable.FrozenMap.tryPick
                (fun key value -> negatedIfNegative key value |> ValueOption.ofOption)
                table,
            found,
            "Option.FrozenMap.tryPick must match the default"
        )

        let nothing : int option = FrozenMap.tryPick (fun _ _ -> None) table
        Assert.AreEqual<int option>(None, nothing, "Option.FrozenMap.tryPick must return None when the chooser returns nothing")

    [<TestMethod>]
    member _.``choose takes a chooser of options`` () =
        let chosen : FrozenMap<int, int> = FrozenMap.choose negatedIfNegative table

        CollectionAssert.AreEqual (
            oracle
            |> Map.toArray
            |> Array.choose (fun (key, value) ->
                negatedIfNegative key value
                |> Option.map (fun chosen -> struct (key, chosen))
            ),
            MapInputs.SortByKey (FSharp.Collections.Immutable.FrozenMap.toArray chosen),
            "Option.FrozenMap.choose must keep the chosen values"
        )

    [<TestMethod>]
    member _.``the functions without an option keep resolving to the other modules`` () =
        let mapped : FrozenMap<int, int> = FrozenMap.map (fun _ (value : string) -> value.Length) table
        let pairs : (int * string) list = FrozenMap.toList table

        Assert.AreEqual<int>(
            MapInputs.Several.Length,
            FSharp.Collections.Immutable.FrozenMap.count mapped,
            "FrozenMap.map must resolve to the default module"
        )

        CollectionAssert.AreEqual (
            Map.toArray oracle,
            List.toArray pairs |> Array.sortBy fst,
            "FrozenMap.toList must resolve to the Tuple module"
        )

#if NET10_0_OR_GREATER
    [<TestMethod>]
    member _.``tryAlternateLookup returns Some for a comparer with the alternate key type`` () =
        let longs = (FrozenMap.forKeyComparer FrozenMapOps.int64ByInt32).ofArray [| struct (1L, "one") |]

        let found : FrozenMap<int64, string>.AlternateLookup<int> option =
            FrozenMap.tryAlternateLookup<int, _, _> longs

        match found with
        | Some lookup -> Assert.IsTrue (lookup.ContainsKey 1, "Option.FrozenMap.tryAlternateLookup must return a working lookup")
        | None -> Assert.Fail "Option.FrozenMap.tryAlternateLookup must return Some for a comparer with the alternate key type"

    [<TestMethod>]
    member _.``tryAlternateLookup returns None for a comparer without the alternate key type`` () =
        let structural = (FrozenMap.forKeyComparer HashIdentity.Structural).ofArray [| struct (1L, "one") |]

        let none : FrozenMap<int64, string>.AlternateLookup<int> option =
            FrozenMap.tryAlternateLookup<int, _, _> structural

        Assert.IsTrue (none.IsNone, "Option.FrozenMap.tryAlternateLookup must return None for a comparer without the type")
#endif
