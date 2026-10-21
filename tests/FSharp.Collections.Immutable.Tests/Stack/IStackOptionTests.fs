namespace FSharp.Collections.Immutable.Tests

open System.Collections.Immutable
open FSharp.Collections.Immutable
open FSharp.Collections.Immutable.Tuple
open FSharp.Collections.Immutable.Option
open Microsoft.VisualStudio.TestTools.UnitTesting

// The namespaces are opened in the order that gives the shapes of FSharp.Core: Tuple first, Option last.
// The type annotations of the results are the proof that the functions of the Option namespace shadow the default ones.

/// The `IStack` module of the `Option` namespace.
[<TestClass; IStackTestCategory; OptionNamespaceTestCategory; PatternMatchingTestCategory>]
type IStackOptionTests () =

    static let several : IImmutableStack<int> = Stack.ofArray Inputs.Several

    static let empty : IImmutableStack<int> = Stack.empty<int>

    static let negatedIfNegative (item : int) = if item < 0 then Some (-item) else None

    [<TestMethod>]
    member _.``tryExactlyOne returns an option`` () =
        let single : IImmutableStack<int> = Stack.singleton 5
        let found : int option = IStack.tryExactlyOne single

        OverrideAssert.OptionMatches (
            FSharp.Collections.Immutable.IStack.tryExactlyOne single,
            found,
            "Option.IStack.tryExactlyOne must match the default after conversion"
        )

    [<TestMethod>]
    member _.``tryHead returns an option`` () =
        let found : int option = IStack.tryHead several
        OverrideAssert.OptionMatches (
            FSharp.Collections.Immutable.IStack.tryHead several,
            found,
            "Option.IStack.tryHead must match the default"
        )

    [<TestMethod>]
    member _.``tryLast returns an option`` () =
        let found : int option = IStack.tryLast several
        OverrideAssert.OptionMatches (
            FSharp.Collections.Immutable.IStack.tryLast several,
            found,
            "Option.IStack.tryLast must match the default"
        )

    [<TestMethod>]
    member _.``tryTail returns an option`` () =
        let found : IImmutableStack<int> option = IStack.tryTail several

        // The equality of an option compares the elements of an array that it holds
        Assert.AreEqual<int[] option>(
            FSharp.Collections.Immutable.IStack.tryTail several
            |> ValueOption.map IStack.toArray
            |> ValueOption.toOption,
            found |> Option.map IStack.toArray,
            "Option.IStack.tryTail must match the default"
        )

    [<TestMethod>]
    member _.``tryUncons returns an option of a reference tuple`` () =
        let found : (int * IImmutableStack<int>) option = IStack.tryUncons several

        // A tuple compares an array by reference, so the first element and the other elements are compared apart
        match found with
        | Some (head, tail) ->
            Assert.AreEqual<int>(Array.head Inputs.Several, head, "Option.IStack.tryUncons must return the first element")

            CollectionAssert.AreEqual (
                Array.tail Inputs.Several,
                IStack.toArray tail,
                "Option.IStack.tryUncons must return the other elements"
            )
        | None -> Assert.Fail "Option.IStack.tryUncons must return the elements of a stack with elements"

        Assert.IsTrue ((IStack.tryUncons empty).IsNone, "Option.IStack.tryUncons must return None for an empty stack")

    [<TestMethod>]
    member _.``Cons is a partial pattern of options`` () =
        let pattern : IImmutableStack<int> -> (int * IImmutableStack<int>) option = IStack.(|Cons|_|)

        match several with
        | IStack.Cons (head, tail) ->
            Assert.AreEqual<int>(Array.head Inputs.Several, head, "Option.IStack.Cons must match the first element")
            CollectionAssert.AreEqual (
                Array.tail Inputs.Several,
                IStack.toArray tail,
                "Option.IStack.Cons must match the other elements"
            )
        | _ -> Assert.Fail "Option.IStack.Cons must match a stack with elements"

        Assert.IsTrue ((pattern empty).IsNone, "Option.IStack.Cons must not match an empty stack")

    [<TestMethod>]
    member _.``pick takes a chooser of options`` () =
        let picked : int = IStack.pick negatedIfNegative several
        Assert.AreEqual<int>(
            Array.pick negatedIfNegative Inputs.Several,
            picked,
            "Option.IStack.pick must pick as List.pick does"
        )

    [<TestMethod>]
    member _.``tryFind returns an option`` () =
        let found : int option = IStack.tryFind (fun item -> item > 6) several

        OverrideAssert.OptionMatches (
            FSharp.Collections.Immutable.IStack.tryFind (fun item -> item > 6) several,
            found,
            "Option.IStack.tryFind must match the default"
        )

    [<TestMethod>]
    member _.``tryFindBack returns an option`` () =
        let found : int option = IStack.tryFindBack (fun item -> item > 6) several

        OverrideAssert.OptionMatches (
            FSharp.Collections.Immutable.IStack.tryFindBack (fun item -> item > 6) several,
            found,
            "Option.IStack.tryFindBack must match the default"
        )

    [<TestMethod>]
    member _.``tryFindIndex returns an option`` () =
        let found : int option = IStack.tryFindIndex (fun item -> item > 6) several

        OverrideAssert.OptionMatches (
            FSharp.Collections.Immutable.IStack.tryFindIndex (fun item -> item > 6) several,
            found,
            "Option.IStack.tryFindIndex must match the default"
        )

    [<TestMethod>]
    member _.``tryFindIndexBack returns an option`` () =
        let found : int option = IStack.tryFindIndexBack (fun item -> item > 6) several

        OverrideAssert.OptionMatches (
            FSharp.Collections.Immutable.IStack.tryFindIndexBack (fun item -> item > 6) several,
            found,
            "Option.IStack.tryFindIndexBack must match the default"
        )

    [<TestMethod>]
    member _.``tryPick takes a chooser of options and returns an option`` () =
        let found : int option = IStack.tryPick negatedIfNegative several

        Assert.AreEqual<int option>(
            Array.tryPick negatedIfNegative Inputs.Several,
            found,
            "Option.IStack.tryPick must pick as List.tryPick does"
        )

    [<TestMethod>]
    member _.``choose takes a chooser of options`` () =
        let chosen : Stack<int> = IStack.choose negatedIfNegative several

        CollectionAssert.AreEqual (
            Array.choose negatedIfNegative Inputs.Several,
            Stack.toArray chosen,
            "Option.IStack.choose must choose as List.choose does"
        )

    [<TestMethod>]
    member _.``the functions without an option keep resolving to the default module`` () =
        let filtered : IImmutableStack<int> = IStack.filter (fun item -> item > 0) several
        let zipped : Stack<int * int> = IStack.zip several several

        CollectionAssert.AreEqual (
            Array.filter (fun item -> item > 0) Inputs.Several,
            IStack.toArray filtered,
            "IStack.filter must resolve to the default module"
        )

        CollectionAssert.AreEqual (
            Array.zip Inputs.Several Inputs.Several,
            Stack.toArray zipped,
            "IStack.zip must resolve to the Tuple module"
        )

    [<TestMethod>]
    [<DataRow(nameof IStack.tryHead, "TryPeek")>]
    [<DataRow(nameof IStack.tryTail, "TryPop")>]
    [<DataRow(nameof IStack.tryUncons, "TryPeekPop")>]
    member _.``the stack functions compile with the names of the stack operations of the BCL``
        (name : string, compiledName : string)
        =
        Assert.AreEqual<string | null>(
            name,
            StackOps.sourceName "FSharp.Collections.Immutable.Option.IStack" compiledName,
            "Option.IStack must compile the function under the name of its stack operation in the BCL"
        )
