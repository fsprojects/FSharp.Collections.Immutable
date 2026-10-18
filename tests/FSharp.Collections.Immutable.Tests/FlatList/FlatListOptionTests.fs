namespace FSharp.Collections.Immutable.Tests

open FSharp.Collections.Immutable
open FSharp.Collections.Immutable.Tuple
open FSharp.Collections.Immutable.Option
open Microsoft.VisualStudio.TestTools.UnitTesting

// The namespaces are opened in the order that gives the shapes of FSharp.Core: Tuple first, Option last.
// The type annotations of the results are the proof that the functions of the Option namespace shadow the default ones.

/// The `FlatList` module of the `Option` namespace.
[<TestClass; FlatListTestCategory; OptionNamespaceTestCategory>]
type FlatListOptionTests () =

    static let array = FlatList.ofArray Inputs.Several

    static let negatedIfNegative (item : int) = if item < 0 then Some (-item) else None

    [<TestMethod>]
    member _.``unfold takes a generator of options of reference tuples`` () =
        let generator (state : int) = if state > 0 then Some (state, state - 1) else None
        let generated : FlatList<int> = FlatList.unfold generator 3

        CollectionAssert.AreEqual (
            Array.unfold generator 3,
            FlatList.toArray generated,
            "Option.FlatList.unfold must generate as Array.unfold does"
        )

    [<TestMethod>]
    member _.``tryExactlyOne returns an option`` () =
        let found : int option = FlatList.tryExactlyOne (FlatList.singleton 5)

        OverrideAssert.optionMatches
            (FSharp.Collections.Immutable.FlatList.tryExactlyOne (FlatList.singleton 5))
            found
            "Option.FlatList.tryExactlyOne must match the default after conversion"

    [<TestMethod>]
    member _.``tryHead returns an option`` () =
        let found : int option = FlatList.tryHead array
        OverrideAssert.optionMatches
            (FSharp.Collections.Immutable.FlatList.tryHead array)
            found
            "Option.FlatList.tryHead must match the default"

    [<TestMethod; DataRow -1; DataRow 2; DataRow 8>]
    member _.``tryItem returns an option`` (index : int) =
        let found : int option = FlatList.tryItem index array

        OverrideAssert.optionMatches
            (FSharp.Collections.Immutable.FlatList.tryItem index array)
            found
            "Option.FlatList.tryItem must match the default"

    [<TestMethod>]
    member _.``tryLast returns an option`` () =
        let found : int option = FlatList.tryLast array
        OverrideAssert.optionMatches
            (FSharp.Collections.Immutable.FlatList.tryLast array)
            found
            "Option.FlatList.tryLast must match the default"

    [<TestMethod>]
    member _.``tryTail returns an option`` () =
        let expected = FSharp.Collections.Immutable.FlatList.tryTail array
        let found : FlatList<int> option = FlatList.tryTail array
        Assert.AreEqual<bool>(expected.IsSome, found.IsSome, "Option.FlatList.tryTail must return a tail when the default does")

        // The tails are compared element by element
        match expected, found with
        | ValueSome expected, Some found ->
            CollectionAssert.AreEqual (
                FlatList.toArray expected,
                FlatList.toArray found,
                "Option.FlatList.tryTail must return the tail of the default"
            )
        | _ -> ()

    [<TestMethod>]
    member _.``pick takes a chooser of options`` () =
        let picked : int = FlatList.pick negatedIfNegative array
        Assert.AreEqual<int>(
            Array.pick negatedIfNegative Inputs.Several,
            picked,
            "Option.FlatList.pick must pick as Array.pick does"
        )

    [<TestMethod>]
    member _.``tryFind returns an option`` () =
        let found : int option = FlatList.tryFind (fun item -> item > 6) array

        OverrideAssert.optionMatches
            (FSharp.Collections.Immutable.FlatList.tryFind (fun item -> item > 6) array)
            found
            "Option.FlatList.tryFind must match the default"

    [<TestMethod>]
    member _.``tryFindBack returns an option`` () =
        let found : int option = FlatList.tryFindBack (fun item -> item > 6) array

        OverrideAssert.optionMatches
            (FSharp.Collections.Immutable.FlatList.tryFindBack (fun item -> item > 6) array)
            found
            "Option.FlatList.tryFindBack must match the default"

    [<TestMethod>]
    member _.``tryFindIndex returns an option`` () =
        let found : int option = FlatList.tryFindIndex (fun item -> item > 6) array

        OverrideAssert.optionMatches
            (FSharp.Collections.Immutable.FlatList.tryFindIndex (fun item -> item > 6) array)
            found
            "Option.FlatList.tryFindIndex must match the default"

    [<TestMethod>]
    member _.``tryFindIndexBack returns an option`` () =
        let found : int option = FlatList.tryFindIndexBack (fun item -> item > 6) array

        OverrideAssert.optionMatches
            (FSharp.Collections.Immutable.FlatList.tryFindIndexBack (fun item -> item > 6) array)
            found
            "Option.FlatList.tryFindIndexBack must match the default"

    [<TestMethod>]
    member _.``tryPick takes a chooser of options and returns an option`` () =
        let found : int option = FlatList.tryPick negatedIfNegative array
        Assert.AreEqual<int option>(
            Array.tryPick negatedIfNegative Inputs.Several,
            found,
            "Option.FlatList.tryPick must pick as Array.tryPick does"
        )

    [<TestMethod>]
    member _.``choose takes a chooser of options`` () =
        let chosen : FlatList<int> = FlatList.choose negatedIfNegative array

        CollectionAssert.AreEqual (
            Array.choose negatedIfNegative Inputs.Several,
            FlatList.toArray chosen,
            "Option.FlatList.choose must choose as Array.choose does"
        )

    [<TestMethod>]
    member _.``the functions without an option keep resolving to the default module`` () =
        let mapped : FlatList<int> = FlatList.map (fun item -> item * 2) array
        let zipped : FlatList<int * int> = FlatList.zip array array
        CollectionAssert.AreEqual (
            Array.map (fun item -> item * 2) Inputs.Several,
            FlatList.toArray mapped,
            "FlatList.map must resolve to the default module"
        )
        CollectionAssert.AreEqual (
            Array.zip Inputs.Several Inputs.Several,
            FlatList.toArray zipped,
            "FlatList.zip must resolve to the Tuple module"
        )
