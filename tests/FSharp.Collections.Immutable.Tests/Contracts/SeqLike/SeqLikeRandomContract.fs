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

    /// The elements of a result as a sorted list, for the comparison of multisets.
    static let sorted (items : int list) = List.sort items

    [<TestMethod>]
    member this.``randomChoice returns an element`` () =
        this.ForEach (
            Inputs.nonEmpty,
            fun collection elements ->
                let chosen = ops.RandomChoice collection
                Assert.Contains (chosen, elements, $"%s{ops.Name}.randomChoice must return an element of %A{elements}")
        )

    [<TestMethod>]
    member this.``randomChoice throws for an empty collection as FSharp.Core does`` () =
        this.ThrowsLikeFSharpCore (
            (fun () -> Array.randomChoice Array.empty<int>),
            (fun () -> List.randomChoice List.empty<int>),
            (fun () -> ops.RandomChoice (ops.OfSeq Inputs.empty)),
            "randomChoice of an empty collection must throw"
        )

    [<TestMethod>]
    member this.``randomChoiceBy chooses as FSharp.Core does with the same randomizer`` () =
        this.ForEach (
            Inputs.nonEmpty,
            fun collection elements ->
                let expected =
                    this.Mirrored (
                        (fun () -> Array.randomChoiceBy (randomizer ()) (List.toArray elements)),
                        fun () -> List.randomChoiceBy (randomizer ()) elements
                    )

                Assert.AreEqual<int>(
                    expected,
                    ops.RandomChoiceBy (randomizer ()) collection,
                    $"%s{ops.Name}.randomChoiceBy of %A{elements}"
                )
        )

    [<TestMethod>]
    member this.``randomChoiceBy throws for a randomizer outside the range as FSharp.Core does`` () =
        this.ThrowsLikeFSharpCore (
            (fun () -> Array.randomChoiceBy (fun () -> 1.0) (List.toArray Inputs.several)),
            (fun () -> List.randomChoiceBy (fun () -> 1.0) Inputs.several),
            (fun () -> ops.RandomChoiceBy (fun () -> 1.0) (ops.OfSeq Inputs.several)),
            "randomChoiceBy with a randomizer that returns 1.0 must throw"
        )

    [<TestMethod>]
    member this.``randomChoiceWith chooses as FSharp.Core does with the same generator`` () =
        this.ForEach (
            Inputs.nonEmpty,
            fun collection elements ->
                let expected =
                    this.Mirrored (
                        (fun () -> Array.randomChoiceWith (Random seed) (List.toArray elements)),
                        fun () -> List.randomChoiceWith (Random seed) elements
                    )

                Assert.AreEqual<int>(
                    expected,
                    ops.RandomChoiceWith (Random seed) collection,
                    $"%s{ops.Name}.randomChoiceWith of %A{elements}"
                )
        )

    [<TestMethod>]
    member this.``randomChoiceWith throws for a null generator as FSharp.Core does`` () =
        let random = Unchecked.defaultof<Random>

        this.ThrowsLikeFSharpCore (
            (fun () -> Array.randomChoiceWith random (List.toArray Inputs.several)),
            (fun () -> List.randomChoiceWith random Inputs.several),
            (fun () -> ops.RandomChoiceWith random (ops.OfSeq Inputs.several)),
            "randomChoiceWith of a null generator must throw"
        )

    [<TestMethod>]
    member this.``randomChoices returns the given number of elements`` () =
        this.ForEach (
            Inputs.nonEmpty,
            fun collection elements ->
                let chosen = ops.ToList (ops.RandomChoices 20 collection)
                Assert.HasCount (20, chosen, $"%s{ops.Name}.randomChoices 20 must return 20 elements")

                for item in chosen do
                    Assert.Contains (item, elements, $"%s{ops.Name}.randomChoices must return elements of %A{elements}")
        )

    [<TestMethod>]
    member this.``randomChoices throws for a negative count or an empty collection as FSharp.Core does`` () =
        for struct (count, input) in [ struct (-1, Inputs.several); struct (3, Inputs.empty) ] do
            this.ThrowsLikeFSharpCore (
                (fun () -> Array.randomChoices count (List.toArray input)),
                (fun () -> List.randomChoices count input),
                (fun () -> ops.RandomChoices count (ops.OfSeq input)),
                $"randomChoices %d{count} of %A{input} must throw"
            )

    [<TestMethod>]
    member this.``randomChoicesBy chooses as FSharp.Core does with the same randomizer`` () =
        this.ForEach (
            Inputs.nonEmpty,
            fun collection elements ->
                let expected =
                    this.Mirrored (
                        (fun () ->
                            Array.randomChoicesBy (randomizer ()) 5 (List.toArray elements)
                            |> List.ofArray
                        ),
                        fun () -> List.randomChoicesBy (randomizer ()) 5 elements
                    )

                Assert.AreEqual<int list>(
                    expected,
                    ops.ToList (ops.RandomChoicesBy (randomizer ()) 5 collection),
                    $"%s{ops.Name}.randomChoicesBy of %A{elements}"
                )
        )

    [<TestMethod>]
    member this.``randomChoicesWith chooses as FSharp.Core does with the same generator`` () =
        this.ForEach (
            Inputs.nonEmpty,
            fun collection elements ->
                let expected =
                    this.Mirrored (
                        (fun () ->
                            Array.randomChoicesWith (Random seed) 5 (List.toArray elements)
                            |> List.ofArray
                        ),
                        fun () -> List.randomChoicesWith (Random seed) 5 elements
                    )

                Assert.AreEqual<int list>(
                    expected,
                    ops.ToList (ops.RandomChoicesWith (Random seed) 5 collection),
                    $"%s{ops.Name}.randomChoicesWith of %A{elements}"
                )
        )

    [<TestMethod>]
    member this.``randomSample returns elements at distinct indices`` () =
        this.ForEach (
            Inputs.nonEmpty,
            fun collection elements ->
                let count = min 3 elements.Length
                let sample = ops.ToList (ops.RandomSample count collection)
                Assert.HasCount (count, sample, $"%s{ops.Name}.randomSample %d{count} must return %d{count} elements")

                // Every sampled element must be available in the input as often as it is sampled
                let remaining = ResizeArray elements

                for item in sample do
                    Assert.IsTrue (
                        remaining.Remove item,
                        $"%s{ops.Name}.randomSample must take each element of %A{elements} once at most"
                    )
        )

    [<TestMethod>]
    member this.``randomSample throws for a negative count or one beyond the length as FSharp.Core does`` () =
        for count in [ -1; 9 ] do
            this.ThrowsLikeFSharpCore (
                (fun () -> Array.randomSample count (List.toArray Inputs.several)),
                (fun () -> List.randomSample count Inputs.several),
                (fun () -> ops.RandomSample count (ops.OfSeq Inputs.several)),
                $"randomSample %d{count} must throw"
            )

    [<TestMethod>]
    member this.``randomSampleBy samples as FSharp.Core does with the same randomizer`` () =
        this.ForEach (
            Inputs.nonEmpty,
            fun collection elements ->
                let count = min 3 elements.Length

                let expected =
                    this.Mirrored (
                        (fun () ->
                            Array.randomSampleBy (randomizer ()) count (List.toArray elements)
                            |> List.ofArray
                        ),
                        fun () -> List.randomSampleBy (randomizer ()) count elements
                    )

                Assert.AreEqual<int list>(
                    expected,
                    ops.ToList (ops.RandomSampleBy (randomizer ()) count collection),
                    $"%s{ops.Name}.randomSampleBy of %A{elements}"
                )
        )

    [<TestMethod>]
    member this.``randomSampleWith samples as FSharp.Core does with the same generator`` () =
        this.ForEach (
            Inputs.nonEmpty,
            fun collection elements ->
                let count = min 3 elements.Length

                let expected =
                    this.Mirrored (
                        (fun () ->
                            Array.randomSampleWith (Random seed) count (List.toArray elements)
                            |> List.ofArray
                        ),
                        fun () -> List.randomSampleWith (Random seed) count elements
                    )

                Assert.AreEqual<int list>(
                    expected,
                    ops.ToList (ops.RandomSampleWith (Random seed) count collection),
                    $"%s{ops.Name}.randomSampleWith of %A{elements}"
                )
        )

    [<TestMethod>]
    member this.``randomShuffle returns a permutation of the elements`` () =
        this.ForEach (
            Inputs.all,
            fun collection elements ->
                Assert.AreEqual<int list>(
                    sorted elements,
                    sorted (ops.ToList (ops.RandomShuffle collection)),
                    $"%s{ops.Name}.randomShuffle must keep the elements of %A{elements}"
                )
        )

    [<TestMethod>]
    member this.``randomShuffleBy shuffles as FSharp.Core does with the same randomizer`` () =
        this.ForEach (
            Inputs.all,
            fun collection elements ->
                let expected =
                    this.Mirrored (
                        (fun () ->
                            Array.randomShuffleBy (randomizer ()) (List.toArray elements)
                            |> List.ofArray
                        ),
                        fun () -> List.randomShuffleBy (randomizer ()) elements
                    )

                Assert.AreEqual<int list>(
                    expected,
                    ops.ToList (ops.RandomShuffleBy (randomizer ()) collection),
                    $"%s{ops.Name}.randomShuffleBy of %A{elements}"
                )
        )

    [<TestMethod>]
    member this.``randomShuffleWith shuffles as FSharp.Core does with the same generator`` () =
        this.ForEach (
            Inputs.all,
            fun collection elements ->
                let expected =
                    this.Mirrored (
                        (fun () ->
                            Array.randomShuffleWith (Random seed) (List.toArray elements)
                            |> List.ofArray
                        ),
                        fun () -> List.randomShuffleWith (Random seed) elements
                    )

                Assert.AreEqual<int list>(
                    expected,
                    ops.ToList (ops.RandomShuffleWith (Random seed) collection),
                    $"%s{ops.Name}.randomShuffleWith of %A{elements}"
                )
        )
