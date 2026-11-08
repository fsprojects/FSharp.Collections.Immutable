namespace FSharp.Collections.Immutable.Tests

open FSharp.Collections.Immutable
open FSharp.Collections.Immutable.Tuple
open FSharp.Collections.Immutable.Option
open Microsoft.VisualStudio.TestTools.UnitTesting

// The namespaces are opened in the order that gives the shapes of FSharp.Core: Tuple first, Option last.
// The type annotations of the results are the proof that the functions of the Option namespace shadow the default ones.

/// The `IndexedList` module of the `Option` namespace.
[<TestClass; IndexedListTestCategory; OptionNamespaceTestCategory; PatternMatchingTestCategory>]
type IndexedListOptionTests () =

    static let list = IndexedList.ofArray Inputs.Several

    static let negatedIfNegative (item : int) = if item < 0 then Some (-item) else None

    [<TestMethod>]
    member _.``unfold takes a generator of options of reference tuples`` () =
        let generator (state : int) = if state > 0 then Some (state, state - 1) else None
        let generated : IndexedList<int> = IndexedList.unfold generator 3

        CollectionAssert.AreEqual (
            Array.unfold generator 3,
            IndexedList.toArray generated,
            "Option.IndexedList.unfold must generate as List.unfold does"
        )

    [<TestMethod>]
    member _.``tryExactlyOne returns an option`` () =
        let found : int option = IndexedList.tryExactlyOne (IndexedList.singleton 5)

        OverrideAssert.OptionMatches (
            FSharp.Collections.Immutable.IndexedList.tryExactlyOne (IndexedList.singleton 5),
            found,
            "Option.IndexedList.tryExactlyOne must match the default after conversion"
        )

    [<TestMethod>]
    member _.``tryHead returns an option`` () =
        let found : int option = IndexedList.tryHead list

        OverrideAssert.OptionMatches (
            FSharp.Collections.Immutable.IndexedList.tryHead list,
            found,
            "Option.IndexedList.tryHead must match the default"
        )

    [<TestMethod; DataRow -1; DataRow 2; DataRow 8>]
    member _.``tryItem returns an option`` (index : int) =
        let found : int option = IndexedList.tryItem index list

        OverrideAssert.OptionMatches (
            FSharp.Collections.Immutable.IndexedList.tryItem index list,
            found,
            "Option.IndexedList.tryItem must match the default"
        )

    [<TestMethod>]
    member _.``tryLast returns an option`` () =
        let found : int option = IndexedList.tryLast list

        OverrideAssert.OptionMatches (
            FSharp.Collections.Immutable.IndexedList.tryLast list,
            found,
            "Option.IndexedList.tryLast must match the default"
        )

    [<TestMethod>]
    member _.``tryTail returns an option`` () =
        let found : IndexedList<int> option = IndexedList.tryTail list

        // The equality of an option compares the elements of an array that it holds
        Assert.AreEqual<int[] option>(
            FSharp.Collections.Immutable.IndexedList.tryTail list
            |> ValueOption.map IndexedList.toArray
            |> ValueOption.toOption,
            found |> Option.map IndexedList.toArray,
            "Option.IndexedList.tryTail must match the default"
        )

    [<TestMethod>]
    member _.``tryUncons returns an option of a reference tuple`` () =
        let found : (int * IndexedList<int>) option = IndexedList.tryUncons list

        // A tuple compares an array by reference, so the first element and the other elements are compared apart
        let head, tail =
            Assert.WantSome (found, "Option.IndexedList.tryUncons must return the elements of a list with elements")

        Assert.AreEqual<int>(Array.head Inputs.Several, head, "Option.IndexedList.tryUncons must return the first element")

        CollectionAssert.AreEqual (
            Array.tail Inputs.Several,
            IndexedList.toArray tail,
            "Option.IndexedList.tryUncons must return the other elements"
        )

        Assert.AreEqual<(int * IndexedList<int>) option>(
            None,
            IndexedList.tryUncons IndexedList.empty<int>,
            "Option.IndexedList.tryUncons must return None for an empty list"
        )

    [<TestMethod>]
    member _.``Cons is a partial pattern of options`` () =
        let pattern : IndexedList<int> -> (int * IndexedList<int>) option = IndexedList.(|Cons|_|)

        match list with
        | IndexedList.Cons (head, tail) ->
            Assert.AreEqual<int>(Array.head Inputs.Several, head, "Option.IndexedList.Cons must match the first element")
            CollectionAssert.AreEqual (
                Array.tail Inputs.Several,
                IndexedList.toArray tail,
                "Option.IndexedList.Cons must match the other elements"
            )
        | _ -> Assert.Fail "Option.IndexedList.Cons must match a list with elements"

        Assert.IsTrue ((pattern IndexedList.empty).IsNone, "Option.IndexedList.Cons must not match an empty list")

    [<TestMethod>]
    member _.``pick takes a chooser of options`` () =
        let picked : int = IndexedList.pick negatedIfNegative list

        Assert.AreEqual<int>(
            Array.pick negatedIfNegative Inputs.Several,
            picked,
            "Option.IndexedList.pick must pick as List.pick does"
        )

    [<TestMethod>]
    member _.``pick throws as List.pick does`` () =
        ExceptionAssert.Same (
            id,
            (fun () ->
                List.pick (fun _ -> None) (List.ofArray Inputs.Several)
                |> ignore
            ),
            (fun () -> IndexedList.pick (fun _ -> None) list |> ignore),
            "Option.IndexedList.pick without a choice must throw as List.pick does"
        )

    [<TestMethod>]
    member _.``tryFind returns an option`` () =
        let found : int option = IndexedList.tryFind (fun item -> item > 6) list

        OverrideAssert.OptionMatches (
            FSharp.Collections.Immutable.IndexedList.tryFind (fun item -> item > 6) list,
            found,
            "Option.IndexedList.tryFind must match the default"
        )

    [<TestMethod>]
    member _.``tryFindBack returns an option`` () =
        let found : int option = IndexedList.tryFindBack (fun item -> item > 6) list

        OverrideAssert.OptionMatches (
            FSharp.Collections.Immutable.IndexedList.tryFindBack (fun item -> item > 6) list,
            found,
            "Option.IndexedList.tryFindBack must match the default"
        )

    [<TestMethod>]
    member _.``tryFindIndex returns an option`` () =
        let found : int option = IndexedList.tryFindIndex (fun item -> item > 6) list

        OverrideAssert.OptionMatches (
            FSharp.Collections.Immutable.IndexedList.tryFindIndex (fun item -> item > 6) list,
            found,
            "Option.IndexedList.tryFindIndex must match the default"
        )

    [<TestMethod>]
    member _.``tryFindIndexBack returns an option`` () =
        let found : int option = IndexedList.tryFindIndexBack (fun item -> item > 6) list

        OverrideAssert.OptionMatches (
            FSharp.Collections.Immutable.IndexedList.tryFindIndexBack (fun item -> item > 6) list,
            found,
            "Option.IndexedList.tryFindIndexBack must match the default"
        )

    [<TestMethod; DataRow(7, 8); DataRow(1, 2); DataRow(4, 3); DataRow(8, 0)>]
    member _.``tryFindIndexBackRange returns an option`` (startIndex : int, count : int) =
        let found : int option = IndexedList.tryFindIndexBackRange startIndex count (fun item -> item < 0) list

        OverrideAssert.OptionMatches (
            FSharp.Collections.Immutable.IndexedList.tryFindIndexBackRange startIndex count (fun item -> item < 0) list,
            found,
            "Option.IndexedList.tryFindIndexBackRange must match the default"
        )

    [<TestMethod; DataRow(0, 8); DataRow(1, 2); DataRow(3, 5); DataRow(8, 0)>]
    member _.``tryFindIndexRange returns an option`` (startIndex : int, count : int) =
        let found : int option = IndexedList.tryFindIndexRange startIndex count (fun item -> item < 0) list

        OverrideAssert.OptionMatches (
            FSharp.Collections.Immutable.IndexedList.tryFindIndexRange startIndex count (fun item -> item < 0) list,
            found,
            "Option.IndexedList.tryFindIndexRange must match the default"
        )

    [<TestMethod>]
    member _.``tryFindIndexRange and tryFindIndexBackRange throw for a range out of range as the default functions do`` () =
        ExceptionAssert.Same (
            id,
            (fun () ->
                FSharp.Collections.Immutable.IndexedList.tryFindIndexRange 5 4 (fun _ -> true) list
                |> ignore
            ),
            (fun () ->
                IndexedList.tryFindIndexRange 5 4 (fun _ -> true) list
                |> ignore
            ),
            "Option.IndexedList.tryFindIndexRange must throw what the default function throws"
        )

        ExceptionAssert.Same (
            id,
            (fun () ->
                FSharp.Collections.Immutable.IndexedList.tryFindIndexBackRange 2 4 (fun _ -> true) list
                |> ignore
            ),
            (fun () ->
                IndexedList.tryFindIndexBackRange 2 4 (fun _ -> true) list
                |> ignore
            ),
            "Option.IndexedList.tryFindIndexBackRange must throw what the default function throws"
        )

    [<TestMethod>]
    member _.``tryPick takes a chooser of options and returns an option`` () =
        let found : int option = IndexedList.tryPick negatedIfNegative list

        Assert.AreEqual<int option>(
            Array.tryPick negatedIfNegative Inputs.Several,
            found,
            "Option.IndexedList.tryPick must pick as List.tryPick does"
        )

    [<TestMethod>]
    member _.``choose takes a chooser of options`` () =
        let chosen : IndexedList<int> = IndexedList.choose negatedIfNegative list

        CollectionAssert.AreEqual (
            Array.choose negatedIfNegative Inputs.Several,
            IndexedList.toArray chosen,
            "Option.IndexedList.choose must choose as List.choose does"
        )

    [<TestMethod>]
    member _.``the functions without an option keep resolving to the default module`` () =
        let mapped : IndexedList<int> = IndexedList.map (fun item -> item * 2) list
        let zipped : IndexedList<int * int> = IndexedList.zip list list

        CollectionAssert.AreEqual (
            Array.map (fun item -> item * 2) Inputs.Several,
            IndexedList.toArray mapped,
            "IndexedList.map must resolve to the default module"
        )

        CollectionAssert.AreEqual (
            Array.zip Inputs.Several Inputs.Several,
            IndexedList.toArray zipped,
            "IndexedList.zip must resolve to the Tuple module"
        )
