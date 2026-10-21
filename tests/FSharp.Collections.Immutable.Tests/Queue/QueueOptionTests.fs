namespace FSharp.Collections.Immutable.Tests

open FSharp.Collections.Immutable
open FSharp.Collections.Immutable.Tuple
open FSharp.Collections.Immutable.Option
open Microsoft.VisualStudio.TestTools.UnitTesting

// The namespaces are opened in the order that gives the shapes of FSharp.Core: Tuple first, Option last.
// The type annotations of the results are the proof that the functions of the Option namespace shadow the default ones.

/// The `Queue` module of the `Option` namespace.
[<TestClass; QueueTestCategory; OptionNamespaceTestCategory; PatternMatchingTestCategory>]
type QueueOptionTests () =

    static let several = Queue.ofArray Inputs.Several

    static let negatedIfNegative (item : int) = if item < 0 then Some (-item) else None

    [<TestMethod>]
    member _.``unfold takes a generator of options of reference tuples`` () =
        let generator (state : int) = if state > 0 then Some (state, state - 1) else None
        let generated : Queue<int> = Queue.unfold generator 3

        CollectionAssert.AreEqual (
            Array.unfold generator 3,
            Queue.toArray generated,
            "Option.Queue.unfold must generate as Array.unfold does"
        )

    [<TestMethod>]
    member _.``tryExactlyOne returns an option`` () =
        let found : int option = Queue.tryExactlyOne (Queue.singleton 5)

        OverrideAssert.OptionMatches (
            FSharp.Collections.Immutable.Queue.tryExactlyOne (Queue.singleton 5),
            found,
            "Option.Queue.tryExactlyOne must match the default after conversion"
        )

    [<TestMethod>]
    member _.``tryHead returns an option`` () =
        let found : int option = Queue.tryHead several

        OverrideAssert.OptionMatches (
            FSharp.Collections.Immutable.Queue.tryHead several,
            found,
            "Option.Queue.tryHead must match the default"
        )

    [<TestMethod>]
    member _.``tryLast returns an option`` () =
        let found : int option = Queue.tryLast several

        OverrideAssert.OptionMatches (
            FSharp.Collections.Immutable.Queue.tryLast several,
            found,
            "Option.Queue.tryLast must match the default"
        )

    [<TestMethod>]
    member _.``tryTail returns an option`` () =
        let found : Queue<int> option = Queue.tryTail several

        // The equality of an option compares the elements of an array that it holds
        Assert.AreEqual<int[] option>(
            FSharp.Collections.Immutable.Queue.tryTail several
            |> ValueOption.map Queue.toArray
            |> ValueOption.toOption,
            found |> Option.map Queue.toArray,
            "Option.Queue.tryTail must match the default"
        )

    [<TestMethod>]
    member _.``tryUncons returns an option of a reference tuple`` () =
        let found : (int * Queue<int>) option = Queue.tryUncons several

        // A tuple compares an array by reference, so the first element and the other elements are compared apart
        match found with
        | Some (head, tail) ->
            Assert.AreEqual<int>(Array.head Inputs.Several, head, "Option.Queue.tryUncons must return the first element")

            CollectionAssert.AreEqual (
                Array.tail Inputs.Several,
                Queue.toArray tail,
                "Option.Queue.tryUncons must return the other elements"
            )
        | None -> Assert.Fail "Option.Queue.tryUncons must return the elements of a queue with elements"

        Assert.IsTrue ((Queue.tryUncons Queue.empty<int>).IsNone, "Option.Queue.tryUncons must return None for an empty queue")

    [<TestMethod>]
    member _.``Cons is a partial pattern of options`` () =
        let pattern : Queue<int> -> (int * Queue<int>) option = Queue.(|Cons|_|)

        match several with
        | Queue.Cons (head, tail) ->
            Assert.AreEqual<int>(Array.head Inputs.Several, head, "Option.Queue.Cons must match the first element")
            CollectionAssert.AreEqual (
                Array.tail Inputs.Several,
                Queue.toArray tail,
                "Option.Queue.Cons must match the other elements"
            )
        | _ -> Assert.Fail "Option.Queue.Cons must match a queue with elements"

        Assert.IsTrue ((pattern Queue.empty).IsNone, "Option.Queue.Cons must not match an empty queue")

    [<TestMethod>]
    member _.``pick takes a chooser of options`` () =
        let picked : int = Queue.pick negatedIfNegative several

        Assert.AreEqual<int>(
            Array.pick negatedIfNegative Inputs.Several,
            picked,
            "Option.Queue.pick must pick as Array.pick does"
        )

    [<TestMethod>]
    member _.``pick throws as Array.pick does`` () =
        ExceptionAssert.Same (
            QueueOps.paramName,
            (fun () ->
                List.pick (fun _ -> None) (List.ofArray Inputs.Several)
                |> ignore
            ),
            (fun () -> Queue.pick (fun _ -> None) several |> ignore),
            "Option.Queue.pick without a choice must throw as Array.pick does"
        )

    [<TestMethod>]
    member _.``tryFind returns an option`` () =
        let found : int option = Queue.tryFind (fun item -> item > 6) several

        OverrideAssert.OptionMatches (
            FSharp.Collections.Immutable.Queue.tryFind (fun item -> item > 6) several,
            found,
            "Option.Queue.tryFind must match the default"
        )

    [<TestMethod>]
    member _.``tryFindBack returns an option`` () =
        let found : int option = Queue.tryFindBack (fun item -> item > 6) several

        OverrideAssert.OptionMatches (
            FSharp.Collections.Immutable.Queue.tryFindBack (fun item -> item > 6) several,
            found,
            "Option.Queue.tryFindBack must match the default"
        )

    [<TestMethod>]
    member _.``tryFindIndex returns an option`` () =
        let found : int option = Queue.tryFindIndex (fun item -> item > 6) several

        OverrideAssert.OptionMatches (
            FSharp.Collections.Immutable.Queue.tryFindIndex (fun item -> item > 6) several,
            found,
            "Option.Queue.tryFindIndex must match the default"
        )

    [<TestMethod>]
    member _.``tryFindIndexBack returns an option`` () =
        let found : int option = Queue.tryFindIndexBack (fun item -> item > 6) several

        OverrideAssert.OptionMatches (
            FSharp.Collections.Immutable.Queue.tryFindIndexBack (fun item -> item > 6) several,
            found,
            "Option.Queue.tryFindIndexBack must match the default"
        )

    [<TestMethod>]
    member _.``tryPick takes a chooser of options and returns an option`` () =
        let found : int option = Queue.tryPick negatedIfNegative several

        Assert.AreEqual<int option>(
            Array.tryPick negatedIfNegative Inputs.Several,
            found,
            "Option.Queue.tryPick must pick as Array.tryPick does"
        )

    [<TestMethod>]
    member _.``choose takes a chooser of options`` () =
        let chosen : Queue<int> = Queue.choose negatedIfNegative several

        CollectionAssert.AreEqual (
            Array.choose negatedIfNegative Inputs.Several,
            Queue.toArray chosen,
            "Option.Queue.choose must choose as Array.choose does"
        )

    [<TestMethod>]
    member _.``the functions without an option keep resolving to the default module`` () =
        let mapped : Queue<int> = Queue.map (fun item -> item * 2) several
        let zipped : Queue<int * int> = Queue.zip several several

        CollectionAssert.AreEqual (
            Array.map (fun item -> item * 2) Inputs.Several,
            Queue.toArray mapped,
            "Queue.map must resolve to the default module"
        )

        CollectionAssert.AreEqual (
            Array.zip Inputs.Several Inputs.Several,
            Queue.toArray zipped,
            "Queue.zip must resolve to the Tuple module"
        )

    [<TestMethod>]
    [<DataRow(nameof Queue.tryHead, "TryPeek")>]
    [<DataRow(nameof Queue.tryTail, "TryDequeue")>]
    [<DataRow(nameof Queue.tryUncons, "TryPeekDequeue")>]
    member _.``the queue functions compile with the names of the queue operations of the BCL``
        (name : string, compiledName : string)
        =
        Assert.AreEqual<string | null>(
            name,
            QueueOps.sourceName "FSharp.Collections.Immutable.Option.Queue" compiledName,
            "Option.Queue must compile the function under the name of its queue operation in the BCL"
        )
