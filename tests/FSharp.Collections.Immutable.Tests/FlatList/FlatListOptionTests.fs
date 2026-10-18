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

    static let array = FlatList.ofList Inputs.several

    static let negatedIfNegative (item : int) = if item < 0 then Some (-item) else None

    [<TestMethod>]
    member _.``unfold takes a generator of options of reference tuples`` () =
        let generator (state : int) = if state > 0 then Some (state, state - 1) else None
        let generated : FlatList<int> = FlatList.unfold generator 3
        Assert.AreEqual<int list>(
            List.unfold generator 3,
            FlatList.toList generated,
            "Option.FlatList.unfold must generate as List.unfold does"
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

    [<TestMethod>]
    member _.``tryItem returns an option`` () =
        for index in [ -1; 2; 8 ] do
            let found : int option = FlatList.tryItem index array

            OverrideAssert.optionMatches
                (FSharp.Collections.Immutable.FlatList.tryItem index array)
                found
                $"Option.FlatList.tryItem %d{index} must match the default"

    [<TestMethod>]
    member _.``tryLast returns an option`` () =
        let found : int option = FlatList.tryLast array
        OverrideAssert.optionMatches
            (FSharp.Collections.Immutable.FlatList.tryLast array)
            found
            "Option.FlatList.tryLast must match the default"

    [<TestMethod>]
    member _.``tryTail returns an option`` () =
        let found : FlatList<int> option = FlatList.tryTail array

        Assert.AreEqual<int list option>(
            FSharp.Collections.Immutable.FlatList.tryTail array
            |> ValueOption.map FlatList.toList
            |> ValueOption.toOption,
            found |> Option.map FlatList.toList,
            "Option.FlatList.tryTail must match the default"
        )

    [<TestMethod>]
    member _.``pick takes a chooser of options`` () =
        let picked : int = FlatList.pick negatedIfNegative array
        Assert.AreEqual<int>(
            List.pick negatedIfNegative Inputs.several,
            picked,
            "Option.FlatList.pick must pick as List.pick does"
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
            List.tryPick negatedIfNegative Inputs.several,
            found,
            "Option.FlatList.tryPick must pick as List.tryPick does"
        )

    [<TestMethod>]
    member _.``choose takes a chooser of options`` () =
        let chosen : FlatList<int> = FlatList.choose negatedIfNegative array

        Assert.AreEqual<int list>(
            List.choose negatedIfNegative Inputs.several,
            FlatList.toList chosen,
            "Option.FlatList.choose must choose as List.choose does"
        )

    [<TestMethod>]
    member _.``the functions without an option keep resolving to the default module`` () =
        let mapped : FlatList<int> = FlatList.map (fun item -> item * 2) array
        let zipped : FlatList<int * int> = FlatList.zip array array
        Assert.AreEqual<int list>(
            List.map (fun item -> item * 2) Inputs.several,
            FlatList.toList mapped,
            "FlatList.map must resolve to the default module"
        )
        Assert.AreEqual<(int * int) list>(
            List.zip Inputs.several Inputs.several,
            FlatList.toList zipped,
            "FlatList.zip must resolve to the Tuple module"
        )
