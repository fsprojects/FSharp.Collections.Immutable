namespace FSharp.Collections.Immutable.Tests

open FSharp.Collections.Immutable
open Microsoft.VisualStudio.TestTools.UnitTesting

/// <summary>The helpers that the contract bases of the sequence-like modules share.</summary>
/// <remarks>
/// The contract tests compute their expectations with the FSharp.Core <c>List</c> module over the elements in the order
/// in which the collection enumerates them, so one test body serves FIFO, LIFO and indexed collections (ADR 0001, D8).
/// </remarks>
[<AbstractClass>]
type SeqContractBase<'C> (ops : SeqOps<'C>) =
    inherit ContractBase (ops.Name, ops.Mirror, ops.ParamName)

    /// Runs a check on every given input: the collection of its elements and the elements that the collection enumerates.
    member _.ForEach (inputs : int list list, check : 'C -> int list -> unit) =
        for input in inputs do
            let collection = ops.OfSeq input
            check collection (ops.ToList collection)

/// The contract of the properties and the access functions of a sequence-like module.
[<AbstractClass>]
type SeqLikeAccessContract<'C> (ops : SeqOps<'C>) =
    inherit SeqContractBase<'C> (ops)

    // ---- Properties ----

    [<TestMethod>]
    member this.``isEmpty is true only for a collection without elements`` () =
        this.ForEach (
            Inputs.all,
            fun collection elements ->
                Assert.AreEqual<bool>(List.isEmpty elements, ops.IsEmpty collection, $"%s{ops.Name}.isEmpty of %A{elements}")
        )

    [<TestMethod>]
    member this.``length counts the elements`` () =
        this.ForEach (
            Inputs.all,
            fun collection elements ->
                Assert.AreEqual<int>(List.length elements, ops.Length collection, $"%s{ops.Name}.length of %A{elements}")
        )

    // ---- Access ----

    [<TestMethod>]
    member this.``exactlyOne returns the only element`` () =
        let collection = ops.OfSeq Inputs.singleton
        Assert.AreEqual<int>(
            List.exactlyOne Inputs.singleton,
            ops.ExactlyOne collection,
            $"%s{ops.Name}.exactlyOne of one element"
        )

    [<TestMethod>]
    member this.``exactlyOne throws for no element or several elements as FSharp.Core does`` () =
        for input in [ Inputs.empty; Inputs.several ] do
            this.ThrowsLikeFSharpCore (
                (fun () -> Array.exactlyOne (List.toArray input)),
                (fun () -> List.exactlyOne input),
                (fun () -> ops.ExactlyOne (ops.OfSeq input)),
                $"exactlyOne of %A{input} must throw"
            )

    [<TestMethod>]
    member this.``head returns the first element`` () =
        this.ForEach (
            Inputs.nonEmpty,
            fun collection elements ->
                Assert.AreEqual<int>(List.head elements, ops.Head collection, $"%s{ops.Name}.head of %A{elements}")
        )

    [<TestMethod>]
    member this.``head throws for an empty collection as FSharp.Core does`` () =
        this.ThrowsLikeFSharpCore (
            (fun () -> Array.head Array.empty<int>),
            (fun () -> List.head List.empty<int>),
            (fun () -> ops.Head (ops.OfSeq Inputs.empty)),
            "head of an empty collection must throw"
        )

    [<TestMethod>]
    member this.``last returns the last element`` () =
        this.ForEach (
            Inputs.nonEmpty,
            fun collection elements ->
                Assert.AreEqual<int>(List.last elements, ops.Last collection, $"%s{ops.Name}.last of %A{elements}")
        )

    [<TestMethod>]
    member this.``last throws for an empty collection as FSharp.Core does`` () =
        this.ThrowsLikeFSharpCore (
            (fun () -> Array.last Array.empty<int>),
            (fun () -> List.last List.empty<int>),
            (fun () -> ops.Last (ops.OfSeq Inputs.empty)),
            "last of an empty collection must throw"
        )

    [<TestMethod>]
    member this.``tail returns the elements after the first one`` () =
        this.ForEach (
            Inputs.nonEmpty,
            fun collection elements ->
                Assert.AreEqual<int list>(
                    List.tail elements,
                    ops.ToList (ops.Tail collection),
                    $"%s{ops.Name}.tail of %A{elements}"
                )
        )

    [<TestMethod>]
    member this.``tail throws for an empty collection as FSharp.Core does`` () =
        this.ThrowsLikeFSharpCore (
            (fun () -> Array.tail Array.empty<int>),
            (fun () -> List.tail List.empty<int>),
            (fun () -> ops.Tail (ops.OfSeq Inputs.empty)),
            "tail of an empty collection must throw"
        )

    [<TestMethod>]
    member this.``tryExactlyOne returns the only element or nothing`` () =
        this.ForEach (
            Inputs.all,
            fun collection elements ->
                Assert.AreEqual<int voption>(
                    List.tryExactlyOne elements |> ValueOption.ofOption,
                    ops.TryExactlyOne collection,
                    $"%s{ops.Name}.tryExactlyOne of %A{elements}"
                )
        )

    [<TestMethod>]
    member this.``tryHead returns the first element or nothing`` () =
        this.ForEach (
            Inputs.all,
            fun collection elements ->
                Assert.AreEqual<int voption>(
                    List.tryHead elements |> ValueOption.ofOption,
                    ops.TryHead collection,
                    $"%s{ops.Name}.tryHead of %A{elements}"
                )
        )

    [<TestMethod>]
    member this.``tryLast returns the last element or nothing`` () =
        this.ForEach (
            Inputs.all,
            fun collection elements ->
                Assert.AreEqual<int voption>(
                    List.tryLast elements |> ValueOption.ofOption,
                    ops.TryLast collection,
                    $"%s{ops.Name}.tryLast of %A{elements}"
                )
        )

    [<TestMethod>]
    member this.``tryTail returns the elements after the first one or nothing`` () =
        this.ForEach (
            Inputs.all,
            fun collection elements ->
                let expected =
                    match elements with
                    | [] -> ValueNone
                    | _ :: rest -> ValueSome rest

                Assert.AreEqual<int list voption>(
                    expected,
                    ops.TryTail collection |> ValueOption.map ops.ToList,
                    $"%s{ops.Name}.tryTail of %A{elements}"
                )
        )
