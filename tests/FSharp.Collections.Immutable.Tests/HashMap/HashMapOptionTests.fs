namespace FSharp.Collections.Immutable.Tests

open System
open FSharp.Collections.Immutable
open FSharp.Collections.Immutable.Tuple
open FSharp.Collections.Immutable.Option
open Microsoft.VisualStudio.TestTools.UnitTesting

// The namespaces are opened in the order that gives the shapes of FSharp.Core: Tuple first, Option last.
// The type annotations of the results are the proof that the functions of the Option namespace shadow the default ones.

/// The `HashMap` module of the `Option` namespace.
[<TestClass; HashMapTestCategory; OptionNamespaceTestCategory>]
type HashMapOptionTests () =

    static let table = FSharp.Collections.Immutable.HashMap.ofSeq MapInputs.Several

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
        let found : string option = HashMap.tryFind key table

        OverrideAssert.OptionMatches (
            FSharp.Collections.Immutable.HashMap.tryFind key table,
            found,
            "Option.HashMap.tryFind must match the default"
        )

    [<TestMethod; DataRow 5; DataRow 1>]
    member _.``tryGetKey returns an option`` (key : int) =
        let found : int option = HashMap.tryGetKey key table

        OverrideAssert.OptionMatches (
            FSharp.Collections.Immutable.HashMap.tryGetKey key table,
            found,
            "Option.HashMap.tryGetKey must match the default"
        )

    [<TestMethod>]
    member _.``pick takes a chooser of options`` () =
        let picked : int = HashMap.pick negatedIfNegative table

        Assert.AreEqual<int>(
            FSharp.Collections.Immutable.HashMap.pick (fun key value -> negatedIfNegative key value |> ValueOption.ofOption) table,
            picked,
            "Option.HashMap.pick must pick as the default does"
        )

    [<TestMethod; DynamicData(nameof HashMapOptionTests.Predicates)>]
    member _.``tryFindKey returns an option`` (predicate : BindingPredicate) =
        let found : int option = HashMap.tryFindKey predicate.Matches table

        OverrideAssert.OptionMatches (
            FSharp.Collections.Immutable.HashMap.tryFindKey predicate.Matches table,
            found,
            "Option.HashMap.tryFindKey must match the default"
        )

    [<TestMethod>]
    member _.``tryPick takes a chooser of options and returns an option`` () =
        let found : int option = HashMap.tryPick negatedIfNegative table

        OverrideAssert.OptionMatches (
            FSharp.Collections.Immutable.HashMap.tryPick
                (fun key value -> negatedIfNegative key value |> ValueOption.ofOption)
                table,
            found,
            "Option.HashMap.tryPick must match the default"
        )

        let nothing : int option = HashMap.tryPick (fun _ _ -> None) table
        Assert.AreEqual<int option>(None, nothing, "Option.HashMap.tryPick must return None when the chooser returns nothing")

    [<TestMethod; DataRow 5; DataRow 1>]
    member _.``change takes a function of options as Map.change does`` (key : int) =
        let extend (current : string option) = current |> Option.map (fun value -> value + "!")
        let changed : HashMap<int, string> = HashMap.change key extend table

        CollectionAssert.AreEqual (
            MapInputs.OfMap (Map.change key extend oracle),
            MapInputs.SortByKey (FSharp.Collections.Immutable.HashMap.toArray changed),
            "Option.HashMap.change must change as Map.change does"
        )

    [<TestMethod>]
    member _.``change leaves the binding as it is for a value that the value comparer finds equal`` () =
        // The function calls HashMap.change, which keeps the binding, its key included, for an equal value (ADR 0001, D14)
        let comparers =
            FSharp.Collections.Immutable.HashMap.forComparers StringComparer.OrdinalIgnoreCase StringComparer.OrdinalIgnoreCase

        let ignoreCase = comparers.ofArray [| "one", "uno" |]
        let changed : HashMap<string, string> = HashMap.change "ONE" (fun _ -> Some "UNO") ignoreCase
        Assert.AreEqual<string option>(
            Some "uno",
            HashMap.tryFind "one" changed,
            "Option.HashMap.change must keep the current value"
        )
        Assert.AreEqual<string option>(
            Some "one",
            HashMap.tryGetKey "ONE" changed,
            "Option.HashMap.change must keep the current key"
        )
        Assert.AreSame (ignoreCase, changed, "Option.HashMap.change must return the input itself")

    [<TestMethod>]
    member _.``choose takes a chooser of options`` () =
        let chosen : HashMap<int, int> = HashMap.choose negatedIfNegative table

        CollectionAssert.AreEqual (
            oracle
            |> Map.toArray
            |> Array.choose (fun (key, value) ->
                negatedIfNegative key value
                |> Option.map (fun chosen -> struct (key, chosen))
            ),
            MapInputs.SortByKey (FSharp.Collections.Immutable.HashMap.toArray chosen),
            "Option.HashMap.choose must keep the chosen values"
        )

    [<TestMethod>]
    member _.``the functions without an option keep resolving to the other modules`` () =
        let mapped : HashMap<int, int> = HashMap.map (fun _ (value : string) -> value.Length) table
        let pairs : (int * string) list = HashMap.toList table

        Assert.AreEqual<int>(
            MapInputs.Several.Length,
            FSharp.Collections.Immutable.HashMap.count mapped,
            "HashMap.map must resolve to the default module"
        )

        CollectionAssert.AreEqual (
            Map.toArray oracle,
            List.toArray pairs |> Array.sortBy fst,
            "HashMap.toList must resolve to the Tuple module"
        )
