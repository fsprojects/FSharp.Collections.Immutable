namespace FSharp.Collections.Immutable.Tests

open FSharp.Collections.Immutable
open FSharp.Collections.Immutable.Tuple
open FSharp.Collections.Immutable.Option
open Microsoft.VisualStudio.TestTools.UnitTesting

// The namespaces are opened in the order that gives the shapes of FSharp.Core: Tuple first, Option last.
// The type annotations of the results are the proof that the functions of the Option namespace shadow the default ones.

/// The `IQueue` module of the `Option` namespace.
[<TestClass; IQueueTestCategory; OptionNamespaceTestCategory; PatternMatchingTestCategory>]
type IQueueOptionTests () =

    static let several : IQueue<int> = Queue.ofArray Inputs.Several

    static let empty : IQueue<int> = Queue.empty<int>

    static let negatedIfNegative (item : int) = if item < 0 then Some (-item) else None

    [<TestMethod>]
    member _.``tryExactlyOne returns an option`` () =
        let single : IQueue<int> = Queue.singleton 5
        let found : int option = IQueue.tryExactlyOne single

        OverrideAssert.OptionMatches (
            FSharp.Collections.Immutable.IQueue.tryExactlyOne single,
            found,
            "Option.IQueue.tryExactlyOne must match the default after conversion"
        )

    [<TestMethod>]
    member _.``tryHead returns an option`` () =
        let found : int option = IQueue.tryHead several

        OverrideAssert.OptionMatches (
            FSharp.Collections.Immutable.IQueue.tryHead several,
            found,
            "Option.IQueue.tryHead must match the default"
        )

    [<TestMethod>]
    member _.``tryLast returns an option`` () =
        let found : int option = IQueue.tryLast several

        OverrideAssert.OptionMatches (
            FSharp.Collections.Immutable.IQueue.tryLast several,
            found,
            "Option.IQueue.tryLast must match the default"
        )

    [<TestMethod>]
    member _.``tryTail returns an option`` () =
        let found : IQueue<int> option = IQueue.tryTail several

        // The equality of an option compares the elements of an array that it holds
        Assert.AreEqual<int[] option>(
            FSharp.Collections.Immutable.IQueue.tryTail several
            |> ValueOption.map IQueue.toArray
            |> ValueOption.toOption,
            found |> Option.map IQueue.toArray,
            "Option.IQueue.tryTail must match the default"
        )

    [<TestMethod>]
    member _.``tryUncons returns an option of a reference tuple`` () =
        let found : (int * IQueue<int>) option = IQueue.tryUncons several

        // A tuple compares an array by reference, so the first element and the other elements are compared apart
        match found with
        | Some (head, tail) ->
            Assert.AreEqual<int>(Array.head Inputs.Several, head, "Option.IQueue.tryUncons must return the first element")

            CollectionAssert.AreEqual (
                Array.tail Inputs.Several,
                IQueue.toArray tail,
                "Option.IQueue.tryUncons must return the other elements"
            )
        | None -> Assert.Fail "Option.IQueue.tryUncons must return the elements of a queue with elements"

        Assert.IsTrue ((IQueue.tryUncons empty).IsNone, "Option.IQueue.tryUncons must return None for an empty queue")

    [<TestMethod>]
    member _.``Cons is a partial pattern of options`` () =
        let pattern : IQueue<int> -> (int * IQueue<int>) option = IQueue.(|Cons|_|)

        match several with
        | IQueue.Cons (head, tail) ->
            Assert.AreEqual<int>(Array.head Inputs.Several, head, "Option.IQueue.Cons must match the first element")
            CollectionAssert.AreEqual (
                Array.tail Inputs.Several,
                IQueue.toArray tail,
                "Option.IQueue.Cons must match the other elements"
            )
        | _ -> Assert.Fail "Option.IQueue.Cons must match a queue with elements"

        Assert.IsTrue ((pattern empty).IsNone, "Option.IQueue.Cons must not match an empty queue")

    [<TestMethod>]
    member _.``pick takes a chooser of options`` () =
        let picked : int = IQueue.pick negatedIfNegative several

        Assert.AreEqual<int>(
            Array.pick negatedIfNegative Inputs.Several,
            picked,
            "Option.IQueue.pick must pick as Array.pick does"
        )

    [<TestMethod>]
    member _.``tryFind returns an option`` () =
        let found : int option = IQueue.tryFind (fun item -> item > 6) several

        OverrideAssert.OptionMatches (
            FSharp.Collections.Immutable.IQueue.tryFind (fun item -> item > 6) several,
            found,
            "Option.IQueue.tryFind must match the default"
        )

    [<TestMethod>]
    member _.``tryFindBack returns an option`` () =
        let found : int option = IQueue.tryFindBack (fun item -> item > 6) several

        OverrideAssert.OptionMatches (
            FSharp.Collections.Immutable.IQueue.tryFindBack (fun item -> item > 6) several,
            found,
            "Option.IQueue.tryFindBack must match the default"
        )

    [<TestMethod>]
    member _.``tryFindIndex returns an option`` () =
        let found : int option = IQueue.tryFindIndex (fun item -> item > 6) several

        OverrideAssert.OptionMatches (
            FSharp.Collections.Immutable.IQueue.tryFindIndex (fun item -> item > 6) several,
            found,
            "Option.IQueue.tryFindIndex must match the default"
        )

    [<TestMethod>]
    member _.``tryFindIndexBack returns an option`` () =
        let found : int option = IQueue.tryFindIndexBack (fun item -> item > 6) several

        OverrideAssert.OptionMatches (
            FSharp.Collections.Immutable.IQueue.tryFindIndexBack (fun item -> item > 6) several,
            found,
            "Option.IQueue.tryFindIndexBack must match the default"
        )

    [<TestMethod>]
    member _.``tryPick takes a chooser of options and returns an option`` () =
        let found : int option = IQueue.tryPick negatedIfNegative several

        Assert.AreEqual<int option>(
            Array.tryPick negatedIfNegative Inputs.Several,
            found,
            "Option.IQueue.tryPick must pick as Array.tryPick does"
        )

    [<TestMethod>]
    member _.``choose takes a chooser of options`` () =
        let chosen : Queue<int> = IQueue.choose negatedIfNegative several

        CollectionAssert.AreEqual (
            Array.choose negatedIfNegative Inputs.Several,
            Queue.toArray chosen,
            "Option.IQueue.choose must choose as Array.choose does"
        )

    [<TestMethod>]
    member _.``the functions without an option keep resolving to the default module`` () =
        let filtered : IQueue<int> = IQueue.filter (fun item -> item > 0) several
        let zipped : Queue<int * int> = IQueue.zip several several

        CollectionAssert.AreEqual (
            Array.filter (fun item -> item > 0) Inputs.Several,
            IQueue.toArray filtered,
            "IQueue.filter must resolve to the default module"
        )

        CollectionAssert.AreEqual (
            Array.zip Inputs.Several Inputs.Several,
            Queue.toArray zipped,
            "IQueue.zip must resolve to the Tuple module"
        )

    [<TestMethod>]
    [<DataRow(nameof IQueue.tryHead, "TryPeek")>]
    [<DataRow(nameof IQueue.tryTail, "TryDequeue")>]
    [<DataRow(nameof IQueue.tryUncons, "TryPeekDequeue")>]
    member _.``the queue functions compile with the names of the queue operations of the BCL``
        (name : string, compiledName : string)
        =
        Assert.AreEqual<string | null>(
            name,
            QueueOps.sourceName "FSharp.Collections.Immutable.Option.IQueue" compiledName,
            "Option.IQueue must compile the function under the name of its queue operation in the BCL"
        )
