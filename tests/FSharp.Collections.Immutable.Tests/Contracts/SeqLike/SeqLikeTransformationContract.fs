namespace FSharp.Collections.Immutable.Tests

open FSharp.Collections.Immutable
open Microsoft.VisualStudio.TestTools.UnitTesting

/// The contract of the transformation functions of a sequence-like module.
[<AbstractClass>]
type SeqLikeTransformationContract<'C> (ops : SeqOps<'C>) =
    inherit SeqLikeSearchContract<'C> (ops)

    static let toStructPairs (pairs : ('T1 * 'T2) array) =
        pairs
        |> Array.map (fun (first, second) -> struct (first, second))

    static let toStructTriples (triples : ('T1 * 'T2 * 'T3) array) =
        triples
        |> Array.map (fun (first, second, third) -> struct (first, second, third))

    /// Every input with every input, for the functions of two collections of any lengths.
    static member InputPairs = seq {
        for first in Inputs.All do
            for second in Inputs.All do
                struct (first, second)
    }

    /// Every input with each size of a chunk or a window: one, inside the length, the length and beyond it.
    static member InputsAndSizes = seq {
        for input in Inputs.All do
            for size in [| 1; 3; 8; 9 |] do
                struct (input, size)
    }

    [<TestMethod; DynamicData(nameof Inputs.All, typeof<Inputs>)>]
    member this.``allPairs pairs every element with every element of the other collection`` (input : int array) =
        let struct (collection, elements) = this.Create input
        let struct (other, otherElements) = this.Create [| 1; 2; 3 |]

        CollectionAssert.AreEqual (
            Array.allPairs elements otherElements |> toStructPairs,
            ops.AllPairs collection other,
            $"%s{ops.Name}.allPairs must pair every element with every element of the other collection"
        )

    [<TestMethod; DynamicData(nameof SeqLikeTransformationContract<'C>.InputPairs)>]
    member this.``append concatenates two collections`` (first : int array, second : int array) =
        let struct (collection, elements) = this.Create first
        let struct (other, otherElements) = this.Create second

        CollectionAssert.AreEqual (
            Array.append elements otherElements,
            ops.ToArray (ops.Append collection other),
            $"%s{ops.Name}.append must concatenate two collections"
        )

    [<TestMethod; DynamicData(nameof Inputs.All, typeof<Inputs>)>]
    member this.``choose keeps the chosen values in order`` (input : int array) =
        let struct (collection, elements) = this.Create input
        let chooser (item : int) = if item > 0 then ValueSome (item * 10) else ValueNone
        let chooserOption (item : int) = if item > 0 then Some (item * 10) else None

        CollectionAssert.AreEqual (
            Array.choose chooserOption elements,
            ops.ToArray (ops.Choose chooser collection),
            $"%s{ops.Name}.choose must keep the chosen values in order"
        )

    [<TestMethod; DynamicData(nameof SeqLikeTransformationContract<'C>.InputsAndSizes)>]
    member this.``chunkBySize divides the elements into chunks`` (input : int array, size : int) =
        let struct (collection, elements) = this.Create input

        CollectionAssert.AreEqual (
            Array.chunkBySize size elements,
            ops.ChunkBySize size collection,
            $"%s{ops.Name}.chunkBySize must divide the elements into chunks"
        )

    [<TestMethod; DataRow 0; DataRow -1>]
    member this.``chunkBySize throws for a size that is not positive as FSharp.Core does`` (size : int) =
        this.ThrowsLikeFSharpCore (
            (fun () -> Array.chunkBySize size Inputs.Several),
            (fun () -> List.chunkBySize size (List.ofArray Inputs.Several)),
            (fun () -> ops.ChunkBySize size (ops.OfSeq Inputs.Several)),
            "chunkBySize of a size that is not positive must throw"
        )

    [<TestMethod; DynamicData(nameof Inputs.All, typeof<Inputs>)>]
    member this.``collect concatenates the collections of the elements`` (input : int array) =
        let struct (collection, elements) = this.Create input

        CollectionAssert.AreEqual (
            Array.collect (fun item -> [| item; item * 10 |]) elements,
            ops.ToArray (ops.Collect (fun item -> ops.OfSeq [| item; item * 10 |]) collection),
            $"%s{ops.Name}.collect must concatenate the collections of the elements"
        )

    [<TestMethod>]
    member _.``concat concatenates a sequence of collections`` () =
        // A call converts an array to the sequence that OfSeq takes; the function value itself does not take an array
        let collections =
            Inputs.All
            |> Seq.map (fun input -> ops.OfSeq input)
            |> Seq.toArray
        let elements = collections |> Array.map ops.ToArray

        CollectionAssert.AreEqual (
            Array.concat elements,
            ops.ToArray (ops.Concat collections),
            $"%s{ops.Name}.concat must concatenate the collections in order"
        )

        Assert.IsEmpty (ops.ToArray (ops.Concat Array.empty), $"%s{ops.Name}.concat of no collections must be empty")

    [<TestMethod; DynamicData(nameof Inputs.All, typeof<Inputs>)>]
    member this.``filter keeps the elements that satisfy the predicate`` (input : int array) =
        let struct (collection, elements) = this.Create input

        CollectionAssert.AreEqual (
            Array.filter (fun item -> item > 0) elements,
            ops.ToArray (ops.Filter (fun item -> item > 0) collection),
            $"%s{ops.Name}.filter must keep the elements that satisfy the predicate"
        )

    [<TestMethod; DynamicData(nameof Inputs.All, typeof<Inputs>)>]
    member this.``indexed pairs every element with its index`` (input : int array) =
        let struct (collection, elements) = this.Create input

        CollectionAssert.AreEqual (
            Array.indexed elements |> toStructPairs,
            ops.Indexed collection,
            $"%s{ops.Name}.indexed must pair every element with its index"
        )

    [<TestMethod; DynamicData(nameof Inputs.All, typeof<Inputs>)>]
    member this.``map transforms every element`` (input : int array) =
        let struct (collection, elements) = this.Create input

        CollectionAssert.AreEqual (
            Array.map (fun item -> item * 3) elements,
            ops.ToArray (ops.Map (fun item -> item * 3) collection),
            $"%s{ops.Name}.map must transform every element"
        )

    [<TestMethod>]
    member this.``map2 transforms the pairs at the same index`` () =
        let struct (collection, elements) = this.Create Inputs.Several
        let struct (other, otherElements) = this.Create Inputs.Other

        CollectionAssert.AreEqual (
            Array.map2 (fun a b -> a * 100 + b) elements otherElements,
            ops.ToArray (ops.Map2 (fun a b -> a * 100 + b) collection other),
            $"%s{ops.Name}.map2 must transform the pairs at the same index"
        )

    [<TestMethod>]
    member this.``map2 throws for different lengths as FSharp.Core does`` () =
        this.ThrowsLikeFSharpCore (
            (fun () -> Array.map2 (+) Inputs.Singleton Inputs.Several),
            (fun () -> List.map2 (+) (List.ofArray Inputs.Singleton) (List.ofArray Inputs.Several)),
            (fun () -> ops.Map2 (+) (ops.OfSeq Inputs.Singleton) (ops.OfSeq Inputs.Several)),
            "map2 of collections of different lengths must throw"
        )

    [<TestMethod>]
    member this.``map3 transforms the triples at the same index`` () =
        let struct (collection, elements) = this.Create Inputs.Several
        let struct (other, otherElements) = this.Create Inputs.Other
        let struct (third, thirdElements) = this.Create Inputs.Third

        CollectionAssert.AreEqual (
            Array.map3 (fun a b c -> a * 10000 + b * 100 + c) elements otherElements thirdElements,
            ops.ToArray (ops.Map3 (fun a b c -> a * 10000 + b * 100 + c) collection other third),
            $"%s{ops.Name}.map3 must transform the triples at the same index"
        )

    [<TestMethod>]
    member this.``map3 throws for different lengths as FSharp.Core does`` () =
        this.ThrowsLikeFSharpCore (
            (fun () -> Array.map3 (fun a b c -> a + b + c) Inputs.Several Inputs.Singleton Inputs.Several),
            (fun () ->
                List.map3
                    (fun a b c -> a + b + c)
                    (List.ofArray Inputs.Several)
                    (List.ofArray Inputs.Singleton)
                    (List.ofArray Inputs.Several)
            ),
            (fun () ->
                ops.Map3
                    (fun a b c -> a + b + c)
                    (ops.OfSeq Inputs.Several)
                    (ops.OfSeq Inputs.Singleton)
                    (ops.OfSeq Inputs.Several)
            ),
            "map3 of collections of different lengths must throw"
        )

    [<TestMethod; DynamicData(nameof Inputs.All, typeof<Inputs>)>]
    member this.``mapi transforms every element with its index`` (input : int array) =
        let struct (collection, elements) = this.Create input

        CollectionAssert.AreEqual (
            Array.mapi (fun index item -> index * 1000 + item) elements,
            ops.ToArray (ops.Mapi (fun index item -> index * 1000 + item) collection),
            $"%s{ops.Name}.mapi must transform every element with its index"
        )

    [<TestMethod>]
    member this.``mapi2 transforms the pairs at the same index with the index`` () =
        let struct (collection, elements) = this.Create Inputs.Several
        let struct (other, otherElements) = this.Create Inputs.Other

        CollectionAssert.AreEqual (
            Array.mapi2 (fun index a b -> index * 10000 + a * 100 + b) elements otherElements,
            ops.ToArray (ops.Mapi2 (fun index a b -> index * 10000 + a * 100 + b) collection other),
            $"%s{ops.Name}.mapi2 must transform the pairs at the same index with the index"
        )

    [<TestMethod>]
    member this.``mapi2 throws for different lengths as FSharp.Core does`` () =
        this.ThrowsLikeFSharpCore (
            (fun () -> Array.mapi2 (fun _ a b -> a + b) Inputs.Singleton Inputs.Several),
            (fun () -> List.mapi2 (fun _ a b -> a + b) (List.ofArray Inputs.Singleton) (List.ofArray Inputs.Several)),
            (fun () -> ops.Mapi2 (fun _ a b -> a + b) (ops.OfSeq Inputs.Singleton) (ops.OfSeq Inputs.Several)),
            "mapi2 of collections of different lengths must throw"
        )

    [<TestMethod; DynamicData(nameof Inputs.All, typeof<Inputs>)>]
    member this.``pairwise pairs every element with its successor`` (input : int array) =
        let struct (collection, elements) = this.Create input

        CollectionAssert.AreEqual (
            Array.pairwise elements |> toStructPairs,
            ops.Pairwise collection,
            $"%s{ops.Name}.pairwise must pair every element with its successor"
        )

    [<TestMethod; DynamicData(nameof Inputs.NonEmpty, typeof<Inputs>)>]
    member this.``permute moves every element to its new index`` (input : int array) =
        let struct (collection, elements) = this.Create input
        let indexMap index = (index + 3) % elements.Length

        CollectionAssert.AreEqual (
            Array.permute indexMap elements,
            ops.ToArray (ops.Permute indexMap collection),
            $"%s{ops.Name}.permute must move every element to its new index"
        )

    [<TestMethod>]
    member this.``permute throws for a function that is not a permutation as FSharp.Core does`` () =
        this.ThrowsLikeFSharpCore (
            (fun () -> Array.permute (fun _ -> 0) Inputs.Several),
            (fun () -> List.permute (fun _ -> 0) (List.ofArray Inputs.Several)),
            (fun () -> ops.Permute (fun _ -> 0) (ops.OfSeq Inputs.Several)),
            "permute with a function that is not a permutation must throw"
        )

    [<TestMethod; DynamicData(nameof Inputs.All, typeof<Inputs>)>]
    member this.``rev reverses the elements`` (input : int array) =
        let struct (collection, elements) = this.Create input
        CollectionAssert.AreEqual (
            Array.rev elements,
            ops.ToArray (ops.Rev collection),
            $"%s{ops.Name}.rev must reverse the elements"
        )

    [<TestMethod>]
    member _.``transpose turns the collections into their columns`` () =
        let collections =
            [| Inputs.Several; Inputs.Other; Inputs.Third |]
            |> Array.map (fun input -> ops.OfSeq input)
        let elements = collections |> Array.map ops.ToArray

        CollectionAssert.AreEqual (
            Array.transpose elements,
            ops.Transpose collections,
            $"%s{ops.Name}.transpose must turn the collections into their columns"
        )

    [<TestMethod>]
    member this.``transpose throws for collections of different lengths as FSharp.Core does`` () =
        this.ThrowsLikeFSharpCore (
            (fun () -> Array.transpose [| Inputs.Several; Inputs.Singleton |]),
            (fun () -> List.transpose [| List.ofArray Inputs.Several; List.ofArray Inputs.Singleton |]),
            (fun () -> ops.Transpose [| ops.OfSeq Inputs.Several; ops.OfSeq Inputs.Singleton |]),
            "transpose of collections of different lengths must throw"
        )

    [<TestMethod>]
    member _.``unzip splits the pairs`` () =
        let pairs = Array.zip Inputs.Several Inputs.Other
        let struct (first, second) = ops.Unzip (toStructPairs pairs)
        let expectedFirst, expectedSecond = Array.unzip pairs
        CollectionAssert.AreEqual (expectedFirst, first, $"%s{ops.Name}.unzip must return the first components")
        CollectionAssert.AreEqual (expectedSecond, second, $"%s{ops.Name}.unzip must return the second components")

    [<TestMethod>]
    member _.``unzip3 splits the triples`` () =
        let triples = Array.zip3 Inputs.Several Inputs.Other Inputs.Third
        let struct (first, second, third) = ops.Unzip3 (toStructTriples triples)
        let expectedFirst, expectedSecond, expectedThird = Array.unzip3 triples
        CollectionAssert.AreEqual (expectedFirst, first, $"%s{ops.Name}.unzip3 must return the first components")
        CollectionAssert.AreEqual (expectedSecond, second, $"%s{ops.Name}.unzip3 must return the second components")
        CollectionAssert.AreEqual (expectedThird, third, $"%s{ops.Name}.unzip3 must return the third components")

    [<TestMethod; DynamicData(nameof Inputs.All, typeof<Inputs>)>]
    member this.``where keeps the elements that satisfy the predicate`` (input : int array) =
        let struct (collection, elements) = this.Create input

        CollectionAssert.AreEqual (
            Array.where (fun item -> item % 2 = 0) elements,
            ops.ToArray (ops.Where (fun item -> item % 2 = 0) collection),
            $"%s{ops.Name}.where must keep the elements that satisfy the predicate"
        )

    [<TestMethod; DynamicData(nameof SeqLikeTransformationContract<'C>.InputsAndSizes)>]
    member this.``windowed returns the windows that slide over the elements`` (input : int array, size : int) =
        let struct (collection, elements) = this.Create input

        CollectionAssert.AreEqual (
            Array.windowed size elements,
            ops.Windowed size collection,
            $"%s{ops.Name}.windowed must return the windows that slide over the elements"
        )

    [<TestMethod; DataRow 0; DataRow -1>]
    member this.``windowed throws for a size that is not positive as FSharp.Core does`` (size : int) =
        this.ThrowsLikeFSharpCore (
            (fun () -> Array.windowed size Inputs.Several),
            (fun () -> List.windowed size (List.ofArray Inputs.Several)),
            (fun () -> ops.Windowed size (ops.OfSeq Inputs.Several)),
            "windowed of a size that is not positive must throw"
        )

    [<TestMethod>]
    member this.``zip pairs the elements at the same index`` () =
        let struct (collection, elements) = this.Create Inputs.Several
        let struct (other, otherElements) = this.Create Inputs.Other

        CollectionAssert.AreEqual (
            Array.zip elements otherElements |> toStructPairs,
            ops.Zip collection other,
            $"%s{ops.Name}.zip must pair the elements at the same index"
        )

    [<TestMethod>]
    member this.``zip throws for different lengths as FSharp.Core does`` () =
        this.ThrowsLikeFSharpCore (
            (fun () -> Array.zip Inputs.Singleton Inputs.Several),
            (fun () -> List.zip (List.ofArray Inputs.Singleton) (List.ofArray Inputs.Several)),
            (fun () -> ops.Zip (ops.OfSeq Inputs.Singleton) (ops.OfSeq Inputs.Several)),
            "zip of collections of different lengths must throw"
        )

    [<TestMethod>]
    member this.``zip3 combines the elements at the same index`` () =
        let struct (collection, elements) = this.Create Inputs.Several
        let struct (other, otherElements) = this.Create Inputs.Other
        let struct (third, thirdElements) = this.Create Inputs.Third

        CollectionAssert.AreEqual (
            Array.zip3 elements otherElements thirdElements
            |> toStructTriples,
            ops.Zip3 collection other third,
            $"%s{ops.Name}.zip3 must combine the elements at the same index"
        )

    [<TestMethod>]
    member this.``zip3 throws for different lengths as FSharp.Core does`` () =
        this.ThrowsLikeFSharpCore (
            (fun () -> Array.zip3 Inputs.Several Inputs.Singleton Inputs.Several),
            (fun () -> List.zip3 (List.ofArray Inputs.Several) (List.ofArray Inputs.Singleton) (List.ofArray Inputs.Several)),
            (fun () -> ops.Zip3 (ops.OfSeq Inputs.Several) (ops.OfSeq Inputs.Singleton) (ops.OfSeq Inputs.Several)),
            "zip3 of collections of different lengths must throw"
        )
