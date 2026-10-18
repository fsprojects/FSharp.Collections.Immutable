namespace FSharp.Collections.Immutable.Tests

open System
open FSharp.Collections.Immutable
open Microsoft.VisualStudio.TestTools.UnitTesting

/// <summary>The contract of the random functions of a sequence-like module.</summary>
/// <remarks>
/// With a seeded generator or a fixed randomizer, a function returns what the function of the mirrored FSharp.Core module
/// returns for the same elements; with <see cref="P:System.Random.Shared"/>, the tests check the properties of the result.
/// </remarks>
[<AbstractClass>]
type SeqLikeRandomContract<'C> (ops : SeqOps<'C>) =
    inherit SeqLikeSortingContract<'C> (ops)

    static let seed = 42

    /// A randomizer that returns the same sequence of numbers for every instance.
    static let randomizer () =
        let random = Random seed
        fun () -> random.NextDouble ()

    /// The elements of a result, sorted, for the comparison of multisets.
    static let sorted (items : int array) = Array.sort items

    /// The counts and the inputs that randomChoices rejects: a negative count, and a count of an empty input.
    static member ChoicesFailures = seq {
        struct (-1, Inputs.Several)
        struct (3, Inputs.Empty)
    }

    [<TestMethod; DynamicData(nameof Inputs.NonEmpty, typeof<Inputs>)>]
    member this.``randomChoice returns an element`` (input : int array) =
        let struct (collection, elements) = this.Create input
        let chosen = ops.RandomChoice collection
        Assert.Contains (chosen, elements, $"%s{ops.Name}.randomChoice must return an element of the collection")

    [<TestMethod>]
    member this.``randomChoice throws for an empty collection as FSharp.Core does`` () =
        this.ThrowsLikeFSharpCore (
            (fun () -> Array.randomChoice Array.empty<int>),
            (fun () -> List.randomChoice List.empty<int>),
            (fun () -> ops.RandomChoice (ops.OfSeq Inputs.Empty)),
            "randomChoice of an empty collection must throw"
        )

    [<TestMethod; DynamicData(nameof Inputs.NonEmpty, typeof<Inputs>)>]
    member this.``randomChoiceBy chooses as FSharp.Core does with the same randomizer`` (input : int array) =
        let struct (collection, elements) = this.Create input

        let expected =
            this.Mirrored (
                (fun () -> Array.randomChoiceBy (randomizer ()) elements),
                fun () -> List.randomChoiceBy (randomizer ()) (List.ofArray elements)
            )

        Assert.AreEqual<int>(
            expected,
            ops.RandomChoiceBy (randomizer ()) collection,
            $"%s{ops.Name}.randomChoiceBy must choose as FSharp.Core does with the same randomizer"
        )

    [<TestMethod>]
    member this.``randomChoiceBy throws for a randomizer outside the range as FSharp.Core does`` () =
        this.ThrowsLikeFSharpCore (
            (fun () -> Array.randomChoiceBy (fun () -> 1.0) Inputs.Several),
            (fun () -> List.randomChoiceBy (fun () -> 1.0) (List.ofArray Inputs.Several)),
            (fun () -> ops.RandomChoiceBy (fun () -> 1.0) (ops.OfSeq Inputs.Several)),
            "randomChoiceBy with a randomizer that returns 1.0 must throw"
        )

    [<TestMethod; DynamicData(nameof Inputs.NonEmpty, typeof<Inputs>)>]
    member this.``randomChoiceWith chooses as FSharp.Core does with the same generator`` (input : int array) =
        let struct (collection, elements) = this.Create input

        let expected =
            this.Mirrored (
                (fun () -> Array.randomChoiceWith (Random seed) elements),
                fun () -> List.randomChoiceWith (Random seed) (List.ofArray elements)
            )

        Assert.AreEqual<int>(
            expected,
            ops.RandomChoiceWith (Random seed) collection,
            $"%s{ops.Name}.randomChoiceWith must choose as FSharp.Core does with the same generator"
        )

    [<TestMethod>]
    member this.``randomChoiceWith throws for a null generator as FSharp.Core does`` () =
        let random = Unchecked.defaultof<Random>

        this.ThrowsLikeFSharpCore (
            (fun () -> Array.randomChoiceWith random Inputs.Several),
            (fun () -> List.randomChoiceWith random (List.ofArray Inputs.Several)),
            (fun () -> ops.RandomChoiceWith random (ops.OfSeq Inputs.Several)),
            "randomChoiceWith of a null generator must throw"
        )

    [<TestMethod; DynamicData(nameof Inputs.NonEmpty, typeof<Inputs>)>]
    member this.``randomChoices returns the given number of elements`` (input : int array) =
        let struct (collection, elements) = this.Create input
        let chosen = ops.ToArray (ops.RandomChoices 20 collection)
        Assert.HasCount (20, chosen, $"%s{ops.Name}.randomChoices 20 must return 20 elements")

        for item in chosen do
            Assert.Contains (item, elements, $"%s{ops.Name}.randomChoices must return elements of the collection")

    [<TestMethod; DynamicData(nameof SeqLikeRandomContract<'C>.ChoicesFailures)>]
    member this.``randomChoices throws for a negative count or an empty collection as FSharp.Core does``
        (count : int, input : int array)
        =
        this.ThrowsLikeFSharpCore (
            (fun () -> Array.randomChoices count input),
            (fun () -> List.randomChoices count (List.ofArray input)),
            (fun () -> ops.RandomChoices count (ops.OfSeq input)),
            "randomChoices of a negative count or of an empty collection must throw"
        )

    [<TestMethod; DynamicData(nameof Inputs.NonEmpty, typeof<Inputs>)>]
    member this.``randomChoicesBy chooses as FSharp.Core does with the same randomizer`` (input : int array) =
        let struct (collection, elements) = this.Create input

        let expected =
            this.Mirrored (
                (fun () -> Array.randomChoicesBy (randomizer ()) 5 elements),
                fun () ->
                    List.randomChoicesBy (randomizer ()) 5 (List.ofArray elements)
                    |> List.toArray
            )

        CollectionAssert.AreEqual (
            expected,
            ops.ToArray (ops.RandomChoicesBy (randomizer ()) 5 collection),
            $"%s{ops.Name}.randomChoicesBy must choose as FSharp.Core does with the same randomizer"
        )

    [<TestMethod; DynamicData(nameof Inputs.NonEmpty, typeof<Inputs>)>]
    member this.``randomChoicesWith chooses as FSharp.Core does with the same generator`` (input : int array) =
        let struct (collection, elements) = this.Create input

        let expected =
            this.Mirrored (
                (fun () -> Array.randomChoicesWith (Random seed) 5 elements),
                fun () ->
                    List.randomChoicesWith (Random seed) 5 (List.ofArray elements)
                    |> List.toArray
            )

        CollectionAssert.AreEqual (
            expected,
            ops.ToArray (ops.RandomChoicesWith (Random seed) 5 collection),
            $"%s{ops.Name}.randomChoicesWith must choose as FSharp.Core does with the same generator"
        )

    [<TestMethod; DynamicData(nameof Inputs.NonEmpty, typeof<Inputs>)>]
    member this.``randomSample returns elements at distinct indices`` (input : int array) =
        let struct (collection, elements) = this.Create input
        let count = min 3 elements.Length
        let sample = ops.ToArray (ops.RandomSample count collection)
        Assert.HasCount (count, sample, $"%s{ops.Name}.randomSample must return the given number of elements")

        // Every sampled element must be available in the input as often as it is sampled
        let remaining = ResizeArray elements

        for item in sample do
            Assert.IsTrue (
                remaining.Remove item,
                $"%s{ops.Name}.randomSample must take each element of the collection once at most"
            )

    [<TestMethod; DataRow -1; DataRow 9>]
    member this.``randomSample throws for a negative count or one beyond the length as FSharp.Core does`` (count : int) =
        this.ThrowsLikeFSharpCore (
            (fun () -> Array.randomSample count Inputs.Several),
            (fun () -> List.randomSample count (List.ofArray Inputs.Several)),
            (fun () -> ops.RandomSample count (ops.OfSeq Inputs.Several)),
            "randomSample of a negative count or beyond the length must throw"
        )

    [<TestMethod; DynamicData(nameof Inputs.NonEmpty, typeof<Inputs>)>]
    member this.``randomSampleBy samples as FSharp.Core does with the same randomizer`` (input : int array) =
        let struct (collection, elements) = this.Create input
        let count = min 3 elements.Length

        let expected =
            this.Mirrored (
                (fun () -> Array.randomSampleBy (randomizer ()) count elements),
                fun () ->
                    List.randomSampleBy (randomizer ()) count (List.ofArray elements)
                    |> List.toArray
            )

        CollectionAssert.AreEqual (
            expected,
            ops.ToArray (ops.RandomSampleBy (randomizer ()) count collection),
            $"%s{ops.Name}.randomSampleBy must sample as FSharp.Core does with the same randomizer"
        )

    [<TestMethod; DynamicData(nameof Inputs.NonEmpty, typeof<Inputs>)>]
    member this.``randomSampleWith samples as FSharp.Core does with the same generator`` (input : int array) =
        let struct (collection, elements) = this.Create input
        let count = min 3 elements.Length

        let expected =
            this.Mirrored (
                (fun () -> Array.randomSampleWith (Random seed) count elements),
                fun () ->
                    List.randomSampleWith (Random seed) count (List.ofArray elements)
                    |> List.toArray
            )

        CollectionAssert.AreEqual (
            expected,
            ops.ToArray (ops.RandomSampleWith (Random seed) count collection),
            $"%s{ops.Name}.randomSampleWith must sample as FSharp.Core does with the same generator"
        )

    [<TestMethod; DynamicData(nameof Inputs.All, typeof<Inputs>)>]
    member this.``randomShuffle returns a permutation of the elements`` (input : int array) =
        let struct (collection, elements) = this.Create input

        CollectionAssert.AreEqual (
            sorted elements,
            sorted (ops.ToArray (ops.RandomShuffle collection)),
            $"%s{ops.Name}.randomShuffle must keep the elements"
        )

    [<TestMethod; DynamicData(nameof Inputs.All, typeof<Inputs>)>]
    member this.``randomShuffleBy shuffles as FSharp.Core does with the same randomizer`` (input : int array) =
        let struct (collection, elements) = this.Create input

        let expected =
            this.Mirrored (
                (fun () -> Array.randomShuffleBy (randomizer ()) elements),
                fun () ->
                    List.randomShuffleBy (randomizer ()) (List.ofArray elements)
                    |> List.toArray
            )

        CollectionAssert.AreEqual (
            expected,
            ops.ToArray (ops.RandomShuffleBy (randomizer ()) collection),
            $"%s{ops.Name}.randomShuffleBy must shuffle as FSharp.Core does with the same randomizer"
        )

    [<TestMethod; DynamicData(nameof Inputs.All, typeof<Inputs>)>]
    member this.``randomShuffleWith shuffles as FSharp.Core does with the same generator`` (input : int array) =
        let struct (collection, elements) = this.Create input

        let expected =
            this.Mirrored (
                (fun () -> Array.randomShuffleWith (Random seed) elements),
                fun () ->
                    List.randomShuffleWith (Random seed) (List.ofArray elements)
                    |> List.toArray
            )

        CollectionAssert.AreEqual (
            expected,
            ops.ToArray (ops.RandomShuffleWith (Random seed) collection),
            $"%s{ops.Name}.randomShuffleWith must shuffle as FSharp.Core does with the same generator"
        )
