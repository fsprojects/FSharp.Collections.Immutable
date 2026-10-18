namespace FSharp.Collections.Immutable.Tests

open FSharp.Collections.Immutable
open Microsoft.VisualStudio.TestTools.UnitTesting

/// The contract of the transformation functions of a sequence-like module.
[<AbstractClass>]
type SeqLikeTransformationContract<'C> (ops : SeqOps<'C>) =
    inherit SeqLikeSearchContract<'C> (ops)

    static let toStructPairs (pairs : ('T1 * 'T2) list) =
        pairs
        |> List.map (fun (first, second) -> struct (first, second))

    static let toStructTriples (triples : ('T1 * 'T2 * 'T3) list) =
        triples
        |> List.map (fun (first, second, third) -> struct (first, second, third))

    [<TestMethod; TestCategory(nameof FlatList.allPairs)>]
    member this.``allPairs pairs every element with every element of the other collection`` () =
        let other = ops.OfSeq [ 1; 2; 3 ]
        let otherElements = ops.ToList other

        this.ForEach (
            Inputs.all,
            fun collection elements ->
                Assert.AreEqual<struct (int * int) list>(
                    List.allPairs elements otherElements |> toStructPairs,
                    ops.AllPairs collection other,
                    $"%s{ops.Name}.allPairs of %A{elements} and %A{otherElements}"
                )
        )

    [<TestMethod; TestCategory(nameof FlatList.append)>]
    member this.``append concatenates two collections`` () =
        for first in Inputs.all do
            for second in Inputs.all do
                let collection = ops.OfSeq first
                let other = ops.OfSeq second

                Assert.AreEqual<int list>(
                    ops.ToList collection @ ops.ToList other,
                    ops.ToList (ops.Append collection other),
                    $"%s{ops.Name}.append of %A{first} and %A{second}"
                )

    [<TestMethod; TestCategory(nameof FlatList.choose)>]
    member this.``choose keeps the chosen values in order`` () =
        this.ForEach (
            Inputs.all,
            fun collection elements ->
                let chooser (item : int) = if item > 0 then ValueSome (item * 10) else ValueNone
                let chooserOption (item : int) = if item > 0 then Some (item * 10) else None

                Assert.AreEqual<int list>(
                    List.choose chooserOption elements,
                    ops.ToList (ops.Choose chooser collection),
                    $"%s{ops.Name}.choose of %A{elements}"
                )
        )

    [<TestMethod; TestCategory(nameof FlatList.chunkBySize)>]
    member this.``chunkBySize divides the elements into chunks`` () =
        this.ForEach (
            Inputs.all,
            fun collection elements ->
                for size in [ 1; 3; 8; 9 ] do
                    Assert.AreEqual<int list list>(
                        List.chunkBySize size elements,
                        ops.ChunkBySize size collection,
                        $"%s{ops.Name}.chunkBySize %d{size} of %A{elements}"
                    )
        )

    [<TestMethod; TestCategory(nameof FlatList.chunkBySize)>]
    member this.``chunkBySize throws for a size that is not positive as FSharp.Core does`` () =
        for size in [ 0; -1 ] do
            this.ThrowsLikeFSharpCore (
                (fun () -> Array.chunkBySize size (List.toArray Inputs.several)),
                (fun () -> List.chunkBySize size Inputs.several),
                (fun () -> ops.ChunkBySize size (ops.OfSeq Inputs.several)),
                $"chunkBySize %d{size} must throw"
            )

    [<TestMethod; TestCategory(nameof FlatList.collect)>]
    member this.``collect concatenates the collections of the elements`` () =
        this.ForEach (
            Inputs.all,
            fun collection elements ->
                Assert.AreEqual<int list>(
                    List.collect (fun item -> [ item; item * 10 ]) elements,
                    ops.ToList (ops.Collect (fun item -> ops.OfSeq [ item; item * 10 ]) collection),
                    $"%s{ops.Name}.collect of %A{elements}"
                )
        )

    [<TestMethod; TestCategory(nameof FlatList.concat)>]
    member this.``concat concatenates a sequence of collections`` () =
        let collections = Inputs.all |> List.map (fun input -> ops.OfSeq input)
        let elements = collections |> List.map ops.ToList

        Assert.AreEqual<int list>(
            List.concat elements,
            ops.ToList (ops.Concat collections),
            $"%s{ops.Name}.concat of %A{elements}"
        )
        Assert.AreEqual<int list>([], ops.ToList (ops.Concat []), $"%s{ops.Name}.concat of no collections")

    [<TestMethod; TestCategory(nameof FlatList.filter)>]
    member this.``filter keeps the elements that satisfy the predicate`` () =
        this.ForEach (
            Inputs.all,
            fun collection elements ->
                Assert.AreEqual<int list>(
                    List.filter (fun item -> item > 0) elements,
                    ops.ToList (ops.Filter (fun item -> item > 0) collection),
                    $"%s{ops.Name}.filter of %A{elements}"
                )
        )

    [<TestMethod; TestCategory(nameof FlatList.indexed)>]
    member this.``indexed pairs every element with its index`` () =
        this.ForEach (
            Inputs.all,
            fun collection elements ->
                Assert.AreEqual<struct (int * int) list>(
                    List.indexed elements |> toStructPairs,
                    ops.Indexed collection,
                    $"%s{ops.Name}.indexed of %A{elements}"
                )
        )

    [<TestMethod; TestCategory(nameof FlatList.map)>]
    member this.``map transforms every element`` () =
        this.ForEach (
            Inputs.all,
            fun collection elements ->
                Assert.AreEqual<int list>(
                    List.map (fun item -> item * 3) elements,
                    ops.ToList (ops.Map (fun item -> item * 3) collection),
                    $"%s{ops.Name}.map of %A{elements}"
                )
        )

    [<TestMethod; TestCategory(nameof FlatList.map2)>]
    member this.``map2 transforms the pairs at the same index`` () =
        let collection = ops.OfSeq Inputs.several
        let other = ops.OfSeq Inputs.other

        Assert.AreEqual<int list>(
            List.map2 (fun a b -> a * 100 + b) (ops.ToList collection) (ops.ToList other),
            ops.ToList (ops.Map2 (fun a b -> a * 100 + b) collection other),
            $"%s{ops.Name}.map2 of two collections"
        )

    [<TestMethod; TestCategory(nameof FlatList.map2)>]
    member this.``map2 throws for different lengths as FSharp.Core does`` () =
        this.ThrowsLikeFSharpCore (
            (fun () -> Array.map2 (+) (List.toArray Inputs.singleton) (List.toArray Inputs.several)),
            (fun () -> List.map2 (+) Inputs.singleton Inputs.several),
            (fun () -> ops.Map2 (+) (ops.OfSeq Inputs.singleton) (ops.OfSeq Inputs.several)),
            "map2 of collections of different lengths must throw"
        )

    [<TestMethod; TestCategory(nameof FlatList.map3)>]
    member this.``map3 transforms the triples at the same index`` () =
        let collection = ops.OfSeq Inputs.several
        let other = ops.OfSeq Inputs.other
        let third = ops.OfSeq Inputs.third

        Assert.AreEqual<int list>(
            List.map3 (fun a b c -> a * 10000 + b * 100 + c) (ops.ToList collection) (ops.ToList other) (ops.ToList third),
            ops.ToList (ops.Map3 (fun a b c -> a * 10000 + b * 100 + c) collection other third),
            $"%s{ops.Name}.map3 of three collections"
        )

    [<TestMethod; TestCategory(nameof FlatList.map3)>]
    member this.``map3 throws for different lengths as FSharp.Core does`` () =
        let shortArray = List.toArray Inputs.singleton
        let longArray = List.toArray Inputs.several

        this.ThrowsLikeFSharpCore (
            (fun () -> Array.map3 (fun a b c -> a + b + c) longArray shortArray longArray),
            (fun () -> List.map3 (fun a b c -> a + b + c) Inputs.several Inputs.singleton Inputs.several),
            (fun () ->
                ops.Map3
                    (fun a b c -> a + b + c)
                    (ops.OfSeq Inputs.several)
                    (ops.OfSeq Inputs.singleton)
                    (ops.OfSeq Inputs.several)
            ),
            "map3 of collections of different lengths must throw"
        )

    [<TestMethod; TestCategory(nameof FlatList.mapi)>]
    member this.``mapi transforms every element with its index`` () =
        this.ForEach (
            Inputs.all,
            fun collection elements ->
                Assert.AreEqual<int list>(
                    List.mapi (fun index item -> index * 1000 + item) elements,
                    ops.ToList (ops.Mapi (fun index item -> index * 1000 + item) collection),
                    $"%s{ops.Name}.mapi of %A{elements}"
                )
        )

    [<TestMethod; TestCategory(nameof FlatList.mapi2)>]
    member this.``mapi2 transforms the pairs at the same index with the index`` () =
        let collection = ops.OfSeq Inputs.several
        let other = ops.OfSeq Inputs.other

        Assert.AreEqual<int list>(
            List.mapi2 (fun index a b -> index * 10000 + a * 100 + b) (ops.ToList collection) (ops.ToList other),
            ops.ToList (ops.Mapi2 (fun index a b -> index * 10000 + a * 100 + b) collection other),
            $"%s{ops.Name}.mapi2 of two collections"
        )

    [<TestMethod; TestCategory(nameof FlatList.mapi2)>]
    member this.``mapi2 throws for different lengths as FSharp.Core does`` () =
        this.ThrowsLikeFSharpCore (
            (fun () -> Array.mapi2 (fun _ a b -> a + b) (List.toArray Inputs.singleton) (List.toArray Inputs.several)),
            (fun () -> List.mapi2 (fun _ a b -> a + b) Inputs.singleton Inputs.several),
            (fun () -> ops.Mapi2 (fun _ a b -> a + b) (ops.OfSeq Inputs.singleton) (ops.OfSeq Inputs.several)),
            "mapi2 of collections of different lengths must throw"
        )

    [<TestMethod; TestCategory(nameof FlatList.pairwise)>]
    member this.``pairwise pairs every element with its successor`` () =
        this.ForEach (
            Inputs.all,
            fun collection elements ->
                Assert.AreEqual<struct (int * int) list>(
                    List.pairwise elements |> toStructPairs,
                    ops.Pairwise collection,
                    $"%s{ops.Name}.pairwise of %A{elements}"
                )
        )

    [<TestMethod; TestCategory(nameof FlatList.permute)>]
    member this.``permute moves every element to its new index`` () =
        this.ForEach (
            Inputs.nonEmpty,
            fun collection elements ->
                let length = List.length elements
                let indexMap index = (index + 3) % length

                Assert.AreEqual<int list>(
                    List.permute indexMap elements,
                    ops.ToList (ops.Permute indexMap collection),
                    $"%s{ops.Name}.permute of %A{elements}"
                )
        )

    [<TestMethod; TestCategory(nameof FlatList.permute)>]
    member this.``permute throws for a function that is not a permutation as FSharp.Core does`` () =
        this.ThrowsLikeFSharpCore (
            (fun () -> Array.permute (fun _ -> 0) (List.toArray Inputs.several)),
            (fun () -> List.permute (fun _ -> 0) Inputs.several),
            (fun () -> ops.Permute (fun _ -> 0) (ops.OfSeq Inputs.several)),
            "permute with a function that is not a permutation must throw"
        )

    [<TestMethod; TestCategory(nameof FlatList.rev)>]
    member this.``rev reverses the elements`` () =
        this.ForEach (
            Inputs.all,
            fun collection elements ->
                Assert.AreEqual<int list>(List.rev elements, ops.ToList (ops.Rev collection), $"%s{ops.Name}.rev of %A{elements}")
        )

    [<TestMethod; TestCategory(nameof FlatList.transpose)>]
    member this.``transpose turns the collections into their columns`` () =
        let collections =
            [ Inputs.several; Inputs.other; Inputs.third ]
            |> List.map (fun input -> ops.OfSeq input)
        let elements = collections |> List.map ops.ToList

        Assert.AreEqual<int list list>(
            List.transpose elements,
            ops.Transpose collections,
            $"%s{ops.Name}.transpose of %A{elements}"
        )

    [<TestMethod; TestCategory(nameof FlatList.transpose)>]
    member this.``transpose throws for collections of different lengths as FSharp.Core does`` () =
        this.ThrowsLikeFSharpCore (
            (fun () -> Array.transpose [ List.toArray Inputs.several; List.toArray Inputs.singleton ]),
            (fun () -> List.transpose [ Inputs.several; Inputs.singleton ]),
            (fun () -> ops.Transpose [ ops.OfSeq Inputs.several; ops.OfSeq Inputs.singleton ]),
            "transpose of collections of different lengths must throw"
        )

    [<TestMethod; TestCategory(nameof FlatList.unzip)>]
    member _.``unzip splits the pairs`` () =
        let pairs = List.zip Inputs.several Inputs.other
        let struct (first, second) = ops.Unzip (toStructPairs pairs)
        let expectedFirst, expectedSecond = List.unzip pairs
        Assert.AreEqual<int list>(expectedFirst, first, $"%s{ops.Name}.unzip must return the first components")
        Assert.AreEqual<int list>(expectedSecond, second, $"%s{ops.Name}.unzip must return the second components")

    [<TestMethod; TestCategory(nameof FlatList.unzip3)>]
    member _.``unzip3 splits the triples`` () =
        let triples = List.zip3 Inputs.several Inputs.other Inputs.third
        let struct (first, second, third) = ops.Unzip3 (toStructTriples triples)
        let expectedFirst, expectedSecond, expectedThird = List.unzip3 triples
        Assert.AreEqual<int list>(expectedFirst, first, $"%s{ops.Name}.unzip3 must return the first components")
        Assert.AreEqual<int list>(expectedSecond, second, $"%s{ops.Name}.unzip3 must return the second components")
        Assert.AreEqual<int list>(expectedThird, third, $"%s{ops.Name}.unzip3 must return the third components")

    [<TestMethod; TestCategory(nameof FlatList.where)>]
    member this.``where keeps the elements that satisfy the predicate`` () =
        this.ForEach (
            Inputs.all,
            fun collection elements ->
                Assert.AreEqual<int list>(
                    List.where (fun item -> item % 2 = 0) elements,
                    ops.ToList (ops.Where (fun item -> item % 2 = 0) collection),
                    $"%s{ops.Name}.where of %A{elements}"
                )
        )

    [<TestMethod; TestCategory(nameof FlatList.windowed)>]
    member this.``windowed returns the windows that slide over the elements`` () =
        this.ForEach (
            Inputs.all,
            fun collection elements ->
                for size in [ 1; 3; 8; 9 ] do
                    Assert.AreEqual<int list list>(
                        List.windowed size elements,
                        ops.Windowed size collection,
                        $"%s{ops.Name}.windowed %d{size} of %A{elements}"
                    )
        )

    [<TestMethod; TestCategory(nameof FlatList.windowed)>]
    member this.``windowed throws for a size that is not positive as FSharp.Core does`` () =
        for size in [ 0; -1 ] do
            this.ThrowsLikeFSharpCore (
                (fun () -> Array.windowed size (List.toArray Inputs.several)),
                (fun () -> List.windowed size Inputs.several),
                (fun () -> ops.Windowed size (ops.OfSeq Inputs.several)),
                $"windowed %d{size} must throw"
            )

    [<TestMethod; TestCategory(nameof FlatList.zip)>]
    member _.``zip pairs the elements at the same index`` () =
        let collection = ops.OfSeq Inputs.several
        let other = ops.OfSeq Inputs.other

        Assert.AreEqual<struct (int * int) list>(
            List.zip (ops.ToList collection) (ops.ToList other)
            |> toStructPairs,
            ops.Zip collection other,
            $"%s{ops.Name}.zip of two collections"
        )

    [<TestMethod; TestCategory(nameof FlatList.zip)>]
    member this.``zip throws for different lengths as FSharp.Core does`` () =
        this.ThrowsLikeFSharpCore (
            (fun () -> Array.zip (List.toArray Inputs.singleton) (List.toArray Inputs.several)),
            (fun () -> List.zip Inputs.singleton Inputs.several),
            (fun () -> ops.Zip (ops.OfSeq Inputs.singleton) (ops.OfSeq Inputs.several)),
            "zip of collections of different lengths must throw"
        )

    [<TestMethod; TestCategory(nameof FlatList.zip3)>]
    member _.``zip3 combines the elements at the same index`` () =
        let collection = ops.OfSeq Inputs.several
        let other = ops.OfSeq Inputs.other
        let third = ops.OfSeq Inputs.third

        Assert.AreEqual<struct (int * int * int) list>(
            List.zip3 (ops.ToList collection) (ops.ToList other) (ops.ToList third)
            |> toStructTriples,
            ops.Zip3 collection other third,
            $"%s{ops.Name}.zip3 of three collections"
        )

    [<TestMethod; TestCategory(nameof FlatList.zip3)>]
    member this.``zip3 throws for different lengths as FSharp.Core does`` () =
        this.ThrowsLikeFSharpCore (
            (fun () -> Array.zip3 (List.toArray Inputs.several) (List.toArray Inputs.singleton) (List.toArray Inputs.several)),
            (fun () -> List.zip3 Inputs.several Inputs.singleton Inputs.several),
            (fun () -> ops.Zip3 (ops.OfSeq Inputs.several) (ops.OfSeq Inputs.singleton) (ops.OfSeq Inputs.several)),
            "zip3 of collections of different lengths must throw"
        )
