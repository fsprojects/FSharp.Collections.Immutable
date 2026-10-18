namespace FSharp.Collections.Immutable.Tests

open FSharp.Collections.Immutable
open FSharp.Collections.Immutable.Tuple
open FSharp.Collections.Immutable.Option
open Microsoft.VisualStudio.TestTools.UnitTesting

// The namespaces are opened in the order that gives the shapes of FSharp.Core: Tuple first, Option last.
// The type annotations of the results are the proof that the functions of the Option namespace shadow the default ones.

/// The `Stack` module of the `Option` namespace.
[<TestClass; StackTestCategory; OptionNamespaceTestCategory; PatternMatchingTestCategory>]
type StackOptionTests () =

    static let several = Stack.ofArray Inputs.Several

    static let negatedIfNegative (item : int) = if item < 0 then Some (-item) else None

    [<TestMethod>]
    member _.``unfold takes a generator of options of reference tuples`` () =
        let generator (state : int) = if state > 0 then Some (state, state - 1) else None
        let generated : Stack<int> = Stack.unfold generator 3

        CollectionAssert.AreEqual (
            Array.unfold generator 3,
            Stack.toArray generated,
            "Option.Stack.unfold must generate as List.unfold does, the first element on top"
        )

    [<TestMethod>]
    member _.``tryExactlyOne returns an option`` () =
        let found : int option = Stack.tryExactlyOne (Stack.singleton 5)

        OverrideAssert.OptionMatches (
            FSharp.Collections.Immutable.Stack.tryExactlyOne (Stack.singleton 5),
            found,
            "Option.Stack.tryExactlyOne must match the default after conversion"
        )

    [<TestMethod>]
    member _.``tryHead returns an option`` () =
        let found : int option = Stack.tryHead several

        OverrideAssert.OptionMatches (
            FSharp.Collections.Immutable.Stack.tryHead several,
            found,
            "Option.Stack.tryHead must match the default"
        )

    [<TestMethod>]
    member _.``tryLast returns an option`` () =
        let found : int option = Stack.tryLast several

        OverrideAssert.OptionMatches (
            FSharp.Collections.Immutable.Stack.tryLast several,
            found,
            "Option.Stack.tryLast must match the default"
        )

    [<TestMethod>]
    member _.``tryTail returns an option`` () =
        let found : Stack<int> option = Stack.tryTail several

        // The equality of an option compares the elements of an array that it holds
        Assert.AreEqual<int[] option>(
            FSharp.Collections.Immutable.Stack.tryTail several
            |> ValueOption.map Stack.toArray
            |> ValueOption.toOption,
            found |> Option.map Stack.toArray,
            "Option.Stack.tryTail must match the default"
        )

    [<TestMethod>]
    member _.``tryUncons returns an option of a reference tuple`` () =
        let found : (int * Stack<int>) option = Stack.tryUncons several

        // A tuple compares an array by reference, so the first element and the other elements are compared apart
        match found with
        | Some (head, tail) ->
            Assert.AreEqual<int>(Array.head Inputs.Several, head, "Option.Stack.tryUncons must return the first element")

            CollectionAssert.AreEqual (
                Array.tail Inputs.Several,
                Stack.toArray tail,
                "Option.Stack.tryUncons must return the other elements"
            )
        | None -> Assert.Fail "Option.Stack.tryUncons must return the elements of a stack with elements"

        Assert.IsTrue ((Stack.tryUncons Stack.empty<int>).IsNone, "Option.Stack.tryUncons must return None for an empty stack")

    [<TestMethod>]
    member _.``Cons is a partial pattern of options`` () =
        let pattern : Stack<int> -> (int * Stack<int>) option = Stack.(|Cons|_|)

        match several with
        | Stack.Cons (head, tail) ->
            Assert.AreEqual<int>(Array.head Inputs.Several, head, "Option.Stack.Cons must match the first element")
            CollectionAssert.AreEqual (
                Array.tail Inputs.Several,
                Stack.toArray tail,
                "Option.Stack.Cons must match the other elements"
            )
        | _ -> Assert.Fail "Option.Stack.Cons must match a stack with elements"

        Assert.IsTrue ((pattern Stack.empty).IsNone, "Option.Stack.Cons must not match an empty stack")

    [<TestMethod>]
    member _.``pick takes a chooser of options`` () =
        let picked : int = Stack.pick negatedIfNegative several
        Assert.AreEqual<int>(Array.pick negatedIfNegative Inputs.Several, picked, "Option.Stack.pick must pick as List.pick does")

    [<TestMethod>]
    member _.``pick throws as List.pick does`` () =
        ExceptionAssert.Same (
            StackOps.paramName,
            (fun () ->
                List.pick (fun _ -> None) (List.ofArray Inputs.Several)
                |> ignore
            ),
            (fun () -> Stack.pick (fun _ -> None) several |> ignore),
            "Option.Stack.pick without a choice must throw as List.pick does"
        )

    [<TestMethod>]
    member _.``tryFind returns an option`` () =
        let found : int option = Stack.tryFind (fun item -> item > 6) several

        OverrideAssert.OptionMatches (
            FSharp.Collections.Immutable.Stack.tryFind (fun item -> item > 6) several,
            found,
            "Option.Stack.tryFind must match the default"
        )

    [<TestMethod>]
    member _.``tryFindBack returns an option`` () =
        let found : int option = Stack.tryFindBack (fun item -> item > 6) several

        OverrideAssert.OptionMatches (
            FSharp.Collections.Immutable.Stack.tryFindBack (fun item -> item > 6) several,
            found,
            "Option.Stack.tryFindBack must match the default"
        )

    [<TestMethod>]
    member _.``tryFindIndex returns an option`` () =
        let found : int option = Stack.tryFindIndex (fun item -> item > 6) several

        OverrideAssert.OptionMatches (
            FSharp.Collections.Immutable.Stack.tryFindIndex (fun item -> item > 6) several,
            found,
            "Option.Stack.tryFindIndex must match the default"
        )

    [<TestMethod>]
    member _.``tryFindIndexBack returns an option`` () =
        let found : int option = Stack.tryFindIndexBack (fun item -> item > 6) several

        OverrideAssert.OptionMatches (
            FSharp.Collections.Immutable.Stack.tryFindIndexBack (fun item -> item > 6) several,
            found,
            "Option.Stack.tryFindIndexBack must match the default"
        )

    [<TestMethod>]
    member _.``tryPick takes a chooser of options and returns an option`` () =
        let found : int option = Stack.tryPick negatedIfNegative several

        Assert.AreEqual<int option>(
            Array.tryPick negatedIfNegative Inputs.Several,
            found,
            "Option.Stack.tryPick must pick as List.tryPick does"
        )

    [<TestMethod>]
    member _.``choose takes a chooser of options`` () =
        let chosen : Stack<int> = Stack.choose negatedIfNegative several

        CollectionAssert.AreEqual (
            Array.choose negatedIfNegative Inputs.Several,
            Stack.toArray chosen,
            "Option.Stack.choose must choose as List.choose does"
        )

    [<TestMethod>]
    member _.``the functions without an option keep resolving to the default module`` () =
        let mapped : Stack<int> = Stack.map (fun item -> item * 2) several
        let zipped : Stack<int * int> = Stack.zip several several

        CollectionAssert.AreEqual (
            Array.map (fun item -> item * 2) Inputs.Several,
            Stack.toArray mapped,
            "Stack.map must resolve to the default module"
        )

        CollectionAssert.AreEqual (
            Array.zip Inputs.Several Inputs.Several,
            Stack.toArray zipped,
            "Stack.zip must resolve to the Tuple module"
        )

    [<TestMethod>]
    [<DataRow(nameof Stack.tryHead, "TryPeek")>]
    [<DataRow(nameof Stack.tryTail, "TryPop")>]
    [<DataRow(nameof Stack.tryUncons, "TryPeekPop")>]
    member _.``the stack functions compile with the names of the stack operations of the BCL``
        (name : string, compiledName : string)
        =
        Assert.AreEqual<string | null>(
            name,
            StackOps.sourceName "FSharp.Collections.Immutable.Option.Stack" compiledName,
            "Option.Stack must compile the function under the name of its stack operation in the BCL"
        )
