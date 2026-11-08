namespace FSharp.Collections.Immutable.Tests

open FSharp.Collections.Immutable
open FSharp.Collections.Immutable.Tuple
open FSharp.Collections.Immutable.Option
open Microsoft.VisualStudio.TestTools.UnitTesting

// The namespaces are opened in the order that gives the shapes of FSharp.Core: Tuple first, Option last.
// The type annotations of the results are the proof that the functions of the Option namespace shadow the default ones.

/// The `IIndexedList` module of the `Option` namespace.
[<TestClass; IIndexedListTestCategory; OptionNamespaceTestCategory; PatternMatchingTestCategory>]
type IIndexedListOptionTests () =

    static let list : IIndexedList<int> = IndexedList.ofArray Inputs.Several

    static let empty : IIndexedList<int> = IndexedList.empty<int>

    static let negatedIfNegative (item : int) = if item < 0 then Some (-item) else None

    [<TestMethod>]
    member _.``tryExactlyOne returns an option`` () =
        let single : IIndexedList<int> = IndexedList.singleton 5
        let found : int option = IIndexedList.tryExactlyOne single

        OverrideAssert.OptionMatches (
            FSharp.Collections.Immutable.IIndexedList.tryExactlyOne single,
            found,
            "Option.IIndexedList.tryExactlyOne must match the default after conversion"
        )

    [<TestMethod>]
    member _.``tryHead returns an option`` () =
        let found : int option = IIndexedList.tryHead list

        OverrideAssert.OptionMatches (
            FSharp.Collections.Immutable.IIndexedList.tryHead list,
            found,
            "Option.IIndexedList.tryHead must match the default"
        )

    [<TestMethod; DataRow -1; DataRow 2; DataRow 8>]
    member _.``tryItem returns an option`` (index : int) =
        let found : int option = IIndexedList.tryItem index list

        OverrideAssert.OptionMatches (
            FSharp.Collections.Immutable.IIndexedList.tryItem index list,
            found,
            "Option.IIndexedList.tryItem must match the default"
        )

    [<TestMethod>]
    member _.``tryLast returns an option`` () =
        let found : int option = IIndexedList.tryLast list

        OverrideAssert.OptionMatches (
            FSharp.Collections.Immutable.IIndexedList.tryLast list,
            found,
            "Option.IIndexedList.tryLast must match the default"
        )

    [<TestMethod>]
    member _.``tryTail returns an option`` () =
        let found : IIndexedList<int> option = IIndexedList.tryTail list

        // The equality of an option compares the elements of an array that it holds
        Assert.AreEqual<int[] option>(
            FSharp.Collections.Immutable.IIndexedList.tryTail list
            |> ValueOption.map IIndexedList.toArray
            |> ValueOption.toOption,
            found |> Option.map IIndexedList.toArray,
            "Option.IIndexedList.tryTail must match the default"
        )

    [<TestMethod>]
    member _.``tryUncons returns an option of a reference tuple`` () =
        let found : (int * IIndexedList<int>) option = IIndexedList.tryUncons list

        // A tuple compares an array by reference, so the first element and the other elements are compared apart
        let head, tail =
            Assert.WantSome (found, "Option.IIndexedList.tryUncons must return the elements of a list with elements")

        Assert.AreEqual<int>(Array.head Inputs.Several, head, "Option.IIndexedList.tryUncons must return the first element")

        CollectionAssert.AreEqual (
            Array.tail Inputs.Several,
            IIndexedList.toArray tail,
            "Option.IIndexedList.tryUncons must return the other elements"
        )

        Assert.IsTrue ((IIndexedList.tryUncons empty).IsNone, "Option.IIndexedList.tryUncons must return None for an empty list")

    [<TestMethod>]
    member _.``Cons is a partial pattern of options`` () =
        let pattern : IIndexedList<int> -> (int * IIndexedList<int>) option = IIndexedList.(|Cons|_|)

        match list with
        | IIndexedList.Cons (head, tail) ->
            Assert.AreEqual<int>(Array.head Inputs.Several, head, "Option.IIndexedList.Cons must match the first element")
            CollectionAssert.AreEqual (
                Array.tail Inputs.Several,
                IIndexedList.toArray tail,
                "Option.IIndexedList.Cons must match the other elements"
            )
        | _ -> Assert.Fail "Option.IIndexedList.Cons must match a list with elements"

        Assert.IsTrue ((pattern empty).IsNone, "Option.IIndexedList.Cons must not match an empty list")

    [<TestMethod>]
    member _.``pick takes a chooser of options`` () =
        let picked : int = IIndexedList.pick negatedIfNegative list

        Assert.AreEqual<int>(
            Array.pick negatedIfNegative Inputs.Several,
            picked,
            "Option.IIndexedList.pick must pick as List.pick does"
        )

    [<TestMethod>]
    member _.``tryFind returns an option`` () =
        let found : int option = IIndexedList.tryFind (fun item -> item > 6) list

        OverrideAssert.OptionMatches (
            FSharp.Collections.Immutable.IIndexedList.tryFind (fun item -> item > 6) list,
            found,
            "Option.IIndexedList.tryFind must match the default"
        )

    [<TestMethod>]
    member _.``tryFindBack returns an option`` () =
        let found : int option = IIndexedList.tryFindBack (fun item -> item > 6) list

        OverrideAssert.OptionMatches (
            FSharp.Collections.Immutable.IIndexedList.tryFindBack (fun item -> item > 6) list,
            found,
            "Option.IIndexedList.tryFindBack must match the default"
        )

    [<TestMethod>]
    member _.``tryFindIndex returns an option`` () =
        let found : int option = IIndexedList.tryFindIndex (fun item -> item > 6) list

        OverrideAssert.OptionMatches (
            FSharp.Collections.Immutable.IIndexedList.tryFindIndex (fun item -> item > 6) list,
            found,
            "Option.IIndexedList.tryFindIndex must match the default"
        )

    [<TestMethod>]
    member _.``tryFindIndexBack returns an option`` () =
        let found : int option = IIndexedList.tryFindIndexBack (fun item -> item > 6) list

        OverrideAssert.OptionMatches (
            FSharp.Collections.Immutable.IIndexedList.tryFindIndexBack (fun item -> item > 6) list,
            found,
            "Option.IIndexedList.tryFindIndexBack must match the default"
        )

    [<TestMethod>]
    member _.``tryPick takes a chooser of options and returns an option`` () =
        let found : int option = IIndexedList.tryPick negatedIfNegative list

        Assert.AreEqual<int option>(
            Array.tryPick negatedIfNegative Inputs.Several,
            found,
            "Option.IIndexedList.tryPick must pick as List.tryPick does"
        )

    [<TestMethod>]
    member _.``choose takes a chooser of options`` () =
        let chosen : IndexedList<int> = IIndexedList.choose negatedIfNegative list

        CollectionAssert.AreEqual (
            Array.choose negatedIfNegative Inputs.Several,
            IndexedList.toArray chosen,
            "Option.IIndexedList.choose must choose as List.choose does"
        )

    [<TestMethod>]
    member _.``the functions without an option keep resolving to the default module`` () =
        let filtered : IIndexedList<int> = IIndexedList.filter (fun item -> item > 0) list
        let zipped : IndexedList<int * int> = IIndexedList.zip list list

        CollectionAssert.AreEqual (
            Array.filter (fun item -> item > 0) Inputs.Several,
            IIndexedList.toArray filtered,
            "IIndexedList.filter must resolve to the default module"
        )

        CollectionAssert.AreEqual (
            Array.zip Inputs.Several Inputs.Several,
            IndexedList.toArray zipped,
            "IIndexedList.zip must resolve to the Tuple module"
        )
