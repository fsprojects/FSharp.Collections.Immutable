namespace FSharp.Collections.Immutable.Tests

open System
open FSharp.Collections.Immutable
open FSharp.Collections.Immutable.Tuple
open FSharp.Collections.Immutable.Option
open Microsoft.VisualStudio.TestTools.UnitTesting

// The namespaces are opened in the order that gives the shapes of FSharp.Core: Tuple first, Option last.
// The type annotations of the results are the proof that the functions of the Option namespace shadow the default ones.

/// The `IMap` module of the `Option` namespace.
[<TestClass; IMapTestCategory; OptionNamespaceTestCategory>]
type IMapOptionTests () =

    static let table = FSharp.Collections.Immutable.HashMap.ofSeq MapInputs.Several :> IMap<int, string>

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
        let found : string option = IMap.tryFind key table

        OverrideAssert.OptionMatches (
            FSharp.Collections.Immutable.IMap.tryFind key table,
            found,
            "Option.IMap.tryFind must match the default"
        )

    [<TestMethod; DataRow 5; DataRow 1>]
    member _.``tryGetKey returns an option`` (key : int) =
        let found : int option = IMap.tryGetKey key table

        OverrideAssert.OptionMatches (
            FSharp.Collections.Immutable.IMap.tryGetKey key table,
            found,
            "Option.IMap.tryGetKey must match the default"
        )

    [<TestMethod>]
    member _.``pick takes a chooser of options`` () =
        let picked : int = IMap.pick negatedIfNegative table

        Assert.AreEqual<int>(
            FSharp.Collections.Immutable.IMap.pick (fun key value -> negatedIfNegative key value |> ValueOption.ofOption) table,
            picked,
            "Option.IMap.pick must pick as the default does"
        )

    [<TestMethod; DynamicData(nameof IMapOptionTests.Predicates)>]
    member _.``tryFindKey returns an option`` (predicate : BindingPredicate) =
        let found : int option = IMap.tryFindKey predicate.Matches table

        OverrideAssert.OptionMatches (
            FSharp.Collections.Immutable.IMap.tryFindKey predicate.Matches table,
            found,
            "Option.IMap.tryFindKey must match the default"
        )

    [<TestMethod>]
    member _.``tryPick takes a chooser of options and returns an option`` () =
        let found : int option = IMap.tryPick negatedIfNegative table

        OverrideAssert.OptionMatches (
            FSharp.Collections.Immutable.IMap.tryPick (fun key value -> negatedIfNegative key value |> ValueOption.ofOption) table,
            found,
            "Option.IMap.tryPick must match the default"
        )

    [<TestMethod; DataRow 5; DataRow 1>]
    member _.``change takes a function of options as Map.change does`` (key : int) =
        let extend (current : string option) = current |> Option.map (fun value -> value + "!")
        let changed : IMap<int, string> = IMap.change key extend table

        CollectionAssert.AreEqual (
            MapInputs.OfMap (Map.change key extend oracle),
            MapInputs.SortByKey (FSharp.Collections.Immutable.IMap.toArray changed),
            "Option.IMap.change must change as Map.change does"
        )

    [<TestMethod>]
    member _.``change leaves the binding as it is for a value that the value comparer finds equal`` () =
        // The function calls IMap.change, whose tests cover both implementations; the immutable dictionaries keep the
        // binding, its key included, for an equal value (ADR 0001, D14)
        let comparers =
            FSharp.Collections.Immutable.HashMap.forComparers StringComparer.OrdinalIgnoreCase StringComparer.OrdinalIgnoreCase

        let ignoreCase : IMap<string, string> = comparers.ofArray [| "one", "uno" |]
        let changed : IMap<string, string> = IMap.change "ONE" (fun _ -> Some "UNO") ignoreCase
        Assert.AreEqual<string option>(Some "uno", IMap.tryFind "one" changed, "Option.IMap.change must keep the current value")
        Assert.AreEqual<string option>(Some "one", IMap.tryGetKey "ONE" changed, "Option.IMap.change must keep the current key")
        Assert.AreSame (ignoreCase, changed, "Option.IMap.change must return the input itself")

    [<TestMethod>]
    member _.``choose takes a chooser of options`` () =
        let chosen : HashMap<int, int> = IMap.choose negatedIfNegative table

        CollectionAssert.AreEqual (
            oracle
            |> Map.toArray
            |> Array.choose (fun (key, value) ->
                negatedIfNegative key value
                |> Option.map (fun chosen -> struct (key, chosen))
            ),
            MapInputs.SortByKey (FSharp.Collections.Immutable.HashMap.toArray chosen),
            "Option.IMap.choose must keep the chosen values"
        )

    [<TestMethod>]
    member _.``the functions without an option keep resolving to the other modules`` () =
        let mapped : HashMap<int, int> = IMap.map (fun _ (value : string) -> value.Length) table
        let pairs : (int * string) list = IMap.toList table

        Assert.AreEqual<int>(
            MapInputs.Several.Length,
            FSharp.Collections.Immutable.HashMap.count mapped,
            "IMap.map must resolve to the default module"
        )

        CollectionAssert.AreEqual (
            Map.toArray oracle,
            List.toArray pairs |> Array.sortBy fst,
            "IMap.toList must resolve to the Tuple module"
        )
