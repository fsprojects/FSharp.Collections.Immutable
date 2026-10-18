namespace FSharp.Collections.Immutable.Tests

open FSharp.Collections.Immutable
open Microsoft.VisualStudio.TestTools.UnitTesting

/// <summary>
/// The helpers that the contract bases of the sequence-like modules share.
/// </summary>
/// <remarks>
/// The contract tests compute their expectations with the FSharp.Core <c>Array</c> module over the elements in the order
/// in which the collection enumerates them, so one test body serves FIFO, LIFO and indexed collections (ADR 0001, D8).
/// </remarks>
[<AbstractClass>]
type SeqContractBase<'C> (ops : SeqOps<'C>) =
    inherit ContractBase (ops.Name, ops.Mirror, ops.ParamName)

    /// The collection of the elements of an input and the elements in the order in which the collection enumerates them.
    member _.Create (input : int array) =
        let collection = ops.OfSeq input
        struct (collection, ops.ToArray collection)

/// The contract of the properties and the access functions of a sequence-like module.
[<AbstractClass>]
type SeqLikeAccessContract<'C> (ops : SeqOps<'C>) =
    inherit SeqContractBase<'C> (ops)

    /// The inputs that do not have exactly one element.
    static member NotSingletons = seq {
        Inputs.Empty
        Inputs.Several
    }

    // ---- Properties ----

    [<TestMethod; DynamicData(nameof Inputs.All, typeof<Inputs>)>]
    member this.``isEmpty is true only for a collection without elements`` (input : int array) =
        let struct (collection, elements) = this.Create input

        Assert.AreEqual<bool>(
            Array.isEmpty elements,
            ops.IsEmpty collection,
            $"%s{ops.Name}.isEmpty must be true only for a collection without elements"
        )

    [<TestMethod; DynamicData(nameof Inputs.All, typeof<Inputs>)>]
    member this.``length counts the elements`` (input : int array) =
        let struct (collection, elements) = this.Create input
        Assert.AreEqual<int>(Array.length elements, ops.Length collection, $"%s{ops.Name}.length must count the elements")

    // ---- Access ----

    [<TestMethod>]
    member _.``exactlyOne returns the only element`` () =
        let collection = ops.OfSeq Inputs.Singleton

        Assert.AreEqual<int>(
            Array.exactlyOne Inputs.Singleton,
            ops.ExactlyOne collection,
            $"%s{ops.Name}.exactlyOne must return the only element"
        )

    [<TestMethod; DynamicData(nameof SeqLikeAccessContract<'C>.NotSingletons)>]
    member this.``exactlyOne throws for no element or several elements as FSharp.Core does`` (input : int array) =
        this.ThrowsLikeFSharpCore (
            (fun () -> Array.exactlyOne input),
            (fun () -> List.exactlyOne (List.ofArray input)),
            (fun () -> ops.ExactlyOne (ops.OfSeq input)),
            "exactlyOne of no element or of several elements must throw"
        )

    [<TestMethod; DynamicData(nameof Inputs.NonEmpty, typeof<Inputs>)>]
    member this.``head returns the first element`` (input : int array) =
        let struct (collection, elements) = this.Create input
        Assert.AreEqual<int>(Array.head elements, ops.Head collection, $"%s{ops.Name}.head must return the first element")

    [<TestMethod>]
    member this.``head throws for an empty collection as FSharp.Core does`` () =
        this.ThrowsLikeFSharpCore (
            (fun () -> Array.head Array.empty<int>),
            (fun () -> List.head List.empty<int>),
            (fun () -> ops.Head (ops.OfSeq Inputs.Empty)),
            "head of an empty collection must throw"
        )

    [<TestMethod; DynamicData(nameof Inputs.NonEmpty, typeof<Inputs>)>]
    member this.``last returns the last element`` (input : int array) =
        let struct (collection, elements) = this.Create input
        Assert.AreEqual<int>(Array.last elements, ops.Last collection, $"%s{ops.Name}.last must return the last element")

    [<TestMethod>]
    member this.``last throws for an empty collection as FSharp.Core does`` () =
        this.ThrowsLikeFSharpCore (
            (fun () -> Array.last Array.empty<int>),
            (fun () -> List.last List.empty<int>),
            (fun () -> ops.Last (ops.OfSeq Inputs.Empty)),
            "last of an empty collection must throw"
        )

    [<TestMethod; DynamicData(nameof Inputs.NonEmpty, typeof<Inputs>)>]
    member this.``tail returns the elements after the first one`` (input : int array) =
        let struct (collection, elements) = this.Create input

        CollectionAssert.AreEqual (
            Array.tail elements,
            ops.ToArray (ops.Tail collection),
            $"%s{ops.Name}.tail must return the elements after the first one"
        )

    [<TestMethod>]
    member this.``tail throws for an empty collection as FSharp.Core does`` () =
        this.ThrowsLikeFSharpCore (
            (fun () -> Array.tail Array.empty<int>),
            (fun () -> List.tail List.empty<int>),
            (fun () -> ops.Tail (ops.OfSeq Inputs.Empty)),
            "tail of an empty collection must throw"
        )

    [<TestMethod; DynamicData(nameof Inputs.All, typeof<Inputs>)>]
    member this.``tryExactlyOne returns the only element or nothing`` (input : int array) =
        let struct (collection, elements) = this.Create input

        Assert.AreEqual<int voption>(
            Array.tryExactlyOne elements |> ValueOption.ofOption,
            ops.TryExactlyOne collection,
            $"%s{ops.Name}.tryExactlyOne must return the only element or nothing"
        )

    [<TestMethod; DynamicData(nameof Inputs.All, typeof<Inputs>)>]
    member this.``tryHead returns the first element or nothing`` (input : int array) =
        let struct (collection, elements) = this.Create input

        Assert.AreEqual<int voption>(
            Array.tryHead elements |> ValueOption.ofOption,
            ops.TryHead collection,
            $"%s{ops.Name}.tryHead must return the first element or nothing"
        )

    [<TestMethod; DynamicData(nameof Inputs.All, typeof<Inputs>)>]
    member this.``tryLast returns the last element or nothing`` (input : int array) =
        let struct (collection, elements) = this.Create input

        Assert.AreEqual<int voption>(
            Array.tryLast elements |> ValueOption.ofOption,
            ops.TryLast collection,
            $"%s{ops.Name}.tryLast must return the last element or nothing"
        )

    [<TestMethod; DynamicData(nameof Inputs.All, typeof<Inputs>)>]
    member this.``tryTail returns the elements after the first one or nothing`` (input : int array) =
        let struct (collection, elements) = this.Create input
        let tail = ops.TryTail collection

        Assert.AreEqual<bool>(
            not (Array.isEmpty elements),
            tail.IsSome,
            $"%s{ops.Name}.tryTail must return a tail exactly for a collection with elements"
        )

        // CollectionAssert compares the elements of the tail and reports the first one that differs
        match tail with
        | ValueSome tail ->
            CollectionAssert.AreEqual (
                Array.tail elements,
                ops.ToArray tail,
                $"%s{ops.Name}.tryTail must return the elements after the first one"
            )
        | ValueNone -> ()
