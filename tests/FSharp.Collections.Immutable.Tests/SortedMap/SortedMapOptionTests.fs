namespace FSharp.Collections.Immutable.Tests

open System
open FSharp.Collections.Immutable
open FSharp.Collections.Immutable.Tuple
open FSharp.Collections.Immutable.Option
open Microsoft.VisualStudio.TestTools.UnitTesting

/// <summary>
/// The predicates over the keys of the bindings of <c>tryFindKey</c> in the tests of the <c>Option</c> namespace, as the
/// cases of their rows.
/// </summary>
/// <remarks>
/// MSTest names a row after its values, and a function has no readable name, so a row holds a case and the test takes the
/// function of the case.
/// </remarks>
[<RequireQualifiedAccess>]
type KeyPredicate =
    /// The bindings of keys above ten.
    | AboveTen
    /// The bindings of negative keys.
    | Negative
    /// No binding.
    | Never

    /// Every predicate: the rows of the test of <c>tryFindKey</c>. The property builds its rows on each read.
    static member All : KeyPredicate seq = seq {
        KeyPredicate.AboveTen
        KeyPredicate.Negative
        KeyPredicate.Never
    }

    /// Tells whether a binding satisfies the predicate.
    member this.Matches (key : int) (_ : string) =
        match this with
        | KeyPredicate.AboveTen -> key > 10
        | KeyPredicate.Negative -> key < 0
        | KeyPredicate.Never -> false

// The namespaces are opened in the order that gives the shapes of FSharp.Core: Tuple first, Option last.
// The type annotations of the results are the proof that the functions of the Option namespace shadow the default ones.

/// The `SortedMap` module of the `Option` namespace.
[<TestClass; SortedMapTestCategory; OptionNamespaceTestCategory>]
type SortedMapOptionTests () =

    static let table = FSharp.Collections.Immutable.SortedMap.ofSeq MapInputs.Several

    static let oracle = MapInputs.ToMap MapInputs.Several

    /// A chooser that returns a value for the bindings of negative keys only.
    static let negatedIfNegative (key : int) (value : string) = if key < 0 then Some (value.Length - key) else None

    [<TestMethod; DataRow 5; DataRow 1>]
    member _.``tryFind returns an option`` (key : int) =
        let found : string option = SortedMap.tryFind key table

        OverrideAssert.OptionMatches (
            FSharp.Collections.Immutable.SortedMap.tryFind key table,
            found,
            "Option.SortedMap.tryFind must match the default"
        )

    [<TestMethod; DataRow 5; DataRow 1>]
    member _.``tryGetKey returns an option`` (key : int) =
        let found : int option = SortedMap.tryGetKey key table

        OverrideAssert.OptionMatches (
            FSharp.Collections.Immutable.SortedMap.tryGetKey key table,
            found,
            "Option.SortedMap.tryGetKey must match the default"
        )

    [<TestMethod>]
    member _.``pick takes a chooser of options as Map.pick does`` () =
        let picked : int = SortedMap.pick negatedIfNegative table

        Assert.AreEqual<int>(
            Map.pick negatedIfNegative oracle,
            picked,
            "Option.SortedMap.pick must pick the first value in the order of the keys, as Map.pick does"
        )

    [<TestMethod; DynamicData(nameof KeyPredicate.All, typeof<KeyPredicate>)>]
    member _.``tryFindKey returns an option as Map.tryFindKey does`` (predicate : KeyPredicate) =
        let found : int option = SortedMap.tryFindKey predicate.Matches table

        Assert.AreEqual<int option>(
            Map.tryFindKey predicate.Matches oracle,
            found,
            "Option.SortedMap.tryFindKey must find the first key in the order of the keys, as Map.tryFindKey does"
        )

        OverrideAssert.OptionMatches (
            FSharp.Collections.Immutable.SortedMap.tryFindKey predicate.Matches table,
            found,
            "Option.SortedMap.tryFindKey must match the default"
        )

    [<TestMethod>]
    member _.``tryPick takes a chooser of options and returns an option as Map.tryPick does`` () =
        let found : int option = SortedMap.tryPick negatedIfNegative table

        Assert.AreEqual<int option>(
            Map.tryPick negatedIfNegative oracle,
            found,
            "Option.SortedMap.tryPick must pick the first value in the order of the keys, as Map.tryPick does"
        )

        let nothing : int option = SortedMap.tryPick (fun _ _ -> None) table
        Assert.AreEqual<int option>(None, nothing, "Option.SortedMap.tryPick must return None when the chooser returns nothing")

    [<TestMethod; DataRow 5; DataRow 1>]
    member _.``change takes a function of options as Map.change does`` (key : int) =
        let extend (current : string option) = current |> Option.map (fun value -> value + "!")
        let changed : SortedMap<int, string> = SortedMap.change key extend table

        CollectionAssert.AreEqual (
            MapInputs.OfMap (Map.change key extend oracle),
            FSharp.Collections.Immutable.SortedMap.toArray changed,
            "Option.SortedMap.change must change as Map.change does"
        )

    [<TestMethod>]
    member _.``change leaves the binding as it is for a value that the value comparer finds equal`` () =
        // The function calls SortedMap.change, which keeps the binding, its key included, for an equal value (ADR 0001, D14)
        let comparers =
            FSharp.Collections.Immutable.SortedMap.forComparers StringComparer.OrdinalIgnoreCase StringComparer.OrdinalIgnoreCase

        let ignoreCase = comparers.ofArray [| "one", "uno" |]
        let changed : SortedMap<string, string> = SortedMap.change "ONE" (fun _ -> Some "UNO") ignoreCase

        Assert.AreEqual<string option>(
            Some "uno",
            SortedMap.tryFind "one" changed,
            "Option.SortedMap.change must keep the current value"
        )

        Assert.AreEqual<string option>(
            Some "one",
            SortedMap.tryGetKey "ONE" changed,
            "Option.SortedMap.change must keep the current key"
        )

        Assert.AreSame (ignoreCase, changed, "Option.SortedMap.change must return the input itself")

    [<TestMethod>]
    member _.``choose takes a chooser of options`` () =
        let chosen : SortedMap<int, int> = SortedMap.choose negatedIfNegative table

        CollectionAssert.AreEqual (
            oracle
            |> Map.toArray
            |> Array.choose (fun (key, value) ->
                negatedIfNegative key value
                |> Option.map (fun chosen -> struct (key, chosen))
            ),
            FSharp.Collections.Immutable.SortedMap.toArray chosen,
            "Option.SortedMap.choose must keep the chosen values in the order of the keys"
        )

    [<TestMethod>]
    member _.``the functions without an option keep resolving to the other modules`` () =
        let mapped : SortedMap<int, int> = SortedMap.map (fun _ (value : string) -> value.Length) table
        let pairs : (int * string) list = SortedMap.toList table

        Assert.AreEqual<int>(
            MapInputs.Several.Length,
            FSharp.Collections.Immutable.SortedMap.count mapped,
            "SortedMap.map must resolve to the default module"
        )

        CollectionAssert.AreEqual (Map.toArray oracle, List.toArray pairs, "SortedMap.toList must resolve to the Tuple module")
