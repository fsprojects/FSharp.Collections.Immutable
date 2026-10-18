namespace FSharp.Collections.Immutable.Tests

open FSharp.Collections.Immutable
open Microsoft.VisualStudio.TestTools.UnitTesting

/// The contract of the folding functions of a sequence-like module.
[<AbstractClass>]
type SeqLikeFoldingContract<'C> (ops : SeqOps<'C>) =
    inherit SeqLikeTransformationContract<'C> (ops)

    /// A folder whose state records the order in which it saw the elements.
    static let record (visited : int array) (item : int) = Array.append visited [| item |]

    /// A folder from the end whose state records the order in which it saw the elements.
    static let recordBack (item : int) (visited : int array) = Array.append visited [| item |]

    /// Every input with every input, with a prefix of the input with several elements and with an input that differs from
    /// that one at the second element.
    static member ComparedInputs =
        let others = seq {
            yield! Inputs.All
            [| 5; -3 |]
            [| 5; 0 |]
        }

        seq {
            for first in Inputs.All do
                for second in others do
                    struct (first, second)
        }

    [<TestMethod; DataRow([| 1.5 |]); DataRow([| 1.0; 2.0; 4.5; -3.0 |])>]
    member _.``average returns the average of the elements`` (input : float array) =
        Assert.AreEqual<float>(
            Array.average input,
            ops.Average input,
            $"%s{ops.Name}.average must return the average of the elements"
        )

    [<TestMethod>]
    member this.``average throws for an empty collection as FSharp.Core does`` () =
        this.ThrowsLikeFSharpCore (
            (fun () -> Array.average Array.empty<float>),
            (fun () -> List.average List.empty<float>),
            (fun () -> ops.Average Array.empty),
            "average of an empty collection must throw"
        )

    [<TestMethod; DynamicData(nameof Inputs.NonEmpty, typeof<Inputs>)>]
    member this.``averageBy returns the average of the projections`` (input : int array) =
        let struct (collection, elements) = this.Create input

        Assert.AreEqual<float>(
            Array.averageBy float elements,
            ops.AverageBy float collection,
            $"%s{ops.Name}.averageBy must return the average of the projections"
        )

    [<TestMethod>]
    member this.``averageBy throws for an empty collection as FSharp.Core does`` () =
        this.ThrowsLikeFSharpCore (
            (fun () -> Array.averageBy float Array.empty<int>),
            (fun () -> List.averageBy float List.empty<int>),
            (fun () -> ops.AverageBy float (ops.OfSeq Inputs.Empty)),
            "averageBy of an empty collection must throw"
        )

    [<TestMethod; DynamicData(nameof SeqLikeFoldingContract<'C>.ComparedInputs)>]
    member this.``compareWith compares element by element and then by length`` (first : int array, second : int array) =
        let struct (collection, elements) = this.Create first
        let struct (other, otherElements) = this.Create second

        Assert.AreEqual<int>(
            Array.compareWith compare elements otherElements |> sign,
            ops.CompareWith compare collection other |> sign,
            $"%s{ops.Name}.compareWith must compare element by element and then by length"
        )

    [<TestMethod; DynamicData(nameof Inputs.All, typeof<Inputs>)>]
    member this.``fold visits the elements from the first one`` (input : int array) =
        let struct (collection, elements) = this.Create input

        CollectionAssert.AreEqual (
            Array.fold record [||] elements,
            ops.Fold record [||] collection,
            $"%s{ops.Name}.fold must visit the elements from the first one"
        )

    [<TestMethod>]
    member this.``fold2 visits the pairs from the first index`` () =
        let struct (collection, elements) = this.Create Inputs.Several
        let struct (other, otherElements) = this.Create Inputs.Other
        let folder (visited : int array) a b = Array.append visited [| a * 100 + b |]

        CollectionAssert.AreEqual (
            Array.fold2 folder [||] elements otherElements,
            ops.Fold2 folder [||] collection other,
            $"%s{ops.Name}.fold2 must visit the pairs from the first index"
        )

    [<TestMethod>]
    member this.``fold2 throws for different lengths as FSharp.Core does`` () =
        let folder (state : int array) (_ : int) (_ : int) = state

        this.ThrowsLikeFSharpCore (
            (fun () -> Array.fold2 folder [||] Inputs.Singleton Inputs.Several),
            (fun () -> List.fold2 folder [||] (List.ofArray Inputs.Singleton) (List.ofArray Inputs.Several)),
            (fun () -> ops.Fold2 folder [||] (ops.OfSeq Inputs.Singleton) (ops.OfSeq Inputs.Several)),
            "fold2 of collections of different lengths must throw"
        )

    [<TestMethod; DynamicData(nameof Inputs.All, typeof<Inputs>)>]
    member this.``foldBack visits the elements from the last one`` (input : int array) =
        let struct (collection, elements) = this.Create input

        CollectionAssert.AreEqual (
            Array.foldBack recordBack elements [||],
            ops.FoldBack recordBack collection [||],
            $"%s{ops.Name}.foldBack must visit the elements from the last one"
        )

    [<TestMethod>]
    member this.``foldBack2 visits the pairs from the last index`` () =
        let struct (collection, elements) = this.Create Inputs.Several
        let struct (other, otherElements) = this.Create Inputs.Other
        let folder a b (visited : int array) = Array.append visited [| a * 100 + b |]

        CollectionAssert.AreEqual (
            Array.foldBack2 folder elements otherElements [||],
            ops.FoldBack2 folder collection other [||],
            $"%s{ops.Name}.foldBack2 must visit the pairs from the last index"
        )

    [<TestMethod>]
    member this.``foldBack2 throws for different lengths as FSharp.Core does`` () =
        let folder (_ : int) (_ : int) (state : int array) = state

        this.ThrowsLikeFSharpCore (
            (fun () -> Array.foldBack2 folder Inputs.Singleton Inputs.Several [||]),
            (fun () -> List.foldBack2 folder (List.ofArray Inputs.Singleton) (List.ofArray Inputs.Several) [||]),
            (fun () -> ops.FoldBack2 folder (ops.OfSeq Inputs.Singleton) (ops.OfSeq Inputs.Several) [||]),
            "foldBack2 of collections of different lengths must throw"
        )

    [<TestMethod; DynamicData(nameof Inputs.All, typeof<Inputs>)>]
    member this.``iter visits every element in order`` (input : int array) =
        let struct (collection, elements) = this.Create input
        let visited = ResizeArray<int>()
        ops.Iter visited.Add collection
        CollectionAssert.AreEqual (elements, visited.ToArray (), $"%s{ops.Name}.iter must visit every element in order")

    [<TestMethod>]
    member this.``iter2 visits every pair in order`` () =
        let struct (collection, elements) = this.Create Inputs.Several
        let struct (other, otherElements) = this.Create Inputs.Other
        let visited = ResizeArray<struct (int * int)>()
        ops.Iter2 (fun a b -> visited.Add (struct (a, b))) collection other

        CollectionAssert.AreEqual (
            Array.zip elements otherElements
            |> Array.map (fun (a, b) -> struct (a, b)),
            visited.ToArray (),
            $"%s{ops.Name}.iter2 must visit every pair in order"
        )

    [<TestMethod>]
    member this.``iter2 throws for different lengths as FSharp.Core does`` () =
        this.ThrowsLikeFSharpCore (
            (fun () -> Array.iter2 (fun _ _ -> ()) Inputs.Singleton Inputs.Several),
            (fun () -> List.iter2 (fun _ _ -> ()) (List.ofArray Inputs.Singleton) (List.ofArray Inputs.Several)),
            (fun () -> ops.Iter2 (fun _ _ -> ()) (ops.OfSeq Inputs.Singleton) (ops.OfSeq Inputs.Several)),
            "iter2 of collections of different lengths must throw"
        )

    [<TestMethod; DynamicData(nameof Inputs.All, typeof<Inputs>)>]
    member this.``iteri visits every element with its index`` (input : int array) =
        let struct (collection, elements) = this.Create input
        let visited = ResizeArray<struct (int * int)>()
        ops.Iteri (fun index item -> visited.Add (struct (index, item))) collection

        CollectionAssert.AreEqual (
            Array.indexed elements
            |> Array.map (fun (index, item) -> struct (index, item)),
            visited.ToArray (),
            $"%s{ops.Name}.iteri must visit every element with its index"
        )

    [<TestMethod>]
    member this.``iteri2 visits every pair with its index`` () =
        let struct (collection, elements) = this.Create Inputs.Several
        let struct (other, otherElements) = this.Create Inputs.Other
        let visited = ResizeArray<struct (int * int * int)>()
        ops.Iteri2 (fun index a b -> visited.Add (struct (index, a, b))) collection other

        CollectionAssert.AreEqual (
            Array.zip elements otherElements
            |> Array.mapi (fun index (a, b) -> struct (index, a, b)),
            visited.ToArray (),
            $"%s{ops.Name}.iteri2 must visit every pair with its index"
        )

    [<TestMethod>]
    member this.``iteri2 throws for different lengths as FSharp.Core does`` () =
        this.ThrowsLikeFSharpCore (
            (fun () -> Array.iteri2 (fun _ _ _ -> ()) Inputs.Singleton Inputs.Several),
            (fun () -> List.iteri2 (fun _ _ _ -> ()) (List.ofArray Inputs.Singleton) (List.ofArray Inputs.Several)),
            (fun () -> ops.Iteri2 (fun _ _ _ -> ()) (ops.OfSeq Inputs.Singleton) (ops.OfSeq Inputs.Several)),
            "iteri2 of collections of different lengths must throw"
        )

    [<TestMethod; DynamicData(nameof Inputs.All, typeof<Inputs>)>]
    member this.``mapFold maps the elements and threads the state from the first one`` (input : int array) =
        let struct (collection, elements) = this.Create input
        let expectedResults, expectedState =
            Array.mapFold (fun state item -> item * state, state + 1) 1 elements
        let struct (results, state) =
            ops.MapFold (fun state item -> struct (item * state, state + 1)) 1 collection
        CollectionAssert.AreEqual (expectedResults, results, $"%s{ops.Name}.mapFold must map the elements from the first one")
        Assert.AreEqual<int>(expectedState, state, $"%s{ops.Name}.mapFold must thread the state from the first element")

    [<TestMethod; DynamicData(nameof Inputs.All, typeof<Inputs>)>]
    member this.``mapFoldBack maps the elements and threads the state from the last one`` (input : int array) =
        let struct (collection, elements) = this.Create input
        let expectedResults, expectedState =
            Array.mapFoldBack (fun item state -> item * state, state + 1) elements 1
        let struct (results, state) =
            ops.MapFoldBack (fun item state -> struct (item * state, state + 1)) collection 1
        CollectionAssert.AreEqual (expectedResults, results, $"%s{ops.Name}.mapFoldBack must map the elements from the last one")
        Assert.AreEqual<int>(expectedState, state, $"%s{ops.Name}.mapFoldBack must thread the state from the last element")

    [<TestMethod; DynamicData(nameof Inputs.NonEmpty, typeof<Inputs>)>]
    member this.``max returns the greatest element`` (input : int array) =
        let struct (collection, elements) = this.Create input
        Assert.AreEqual<int>(Array.max elements, ops.Max collection, $"%s{ops.Name}.max must return the greatest element")

    [<TestMethod>]
    member this.``max throws for an empty collection as FSharp.Core does`` () =
        this.ThrowsLikeFSharpCore (
            (fun () -> Array.max Array.empty<int>),
            (fun () -> List.max List.empty<int>),
            (fun () -> ops.Max (ops.OfSeq Inputs.Empty)),
            "max of an empty collection must throw"
        )

    [<TestMethod; DynamicData(nameof Inputs.NonEmpty, typeof<Inputs>)>]
    member this.``maxBy returns the first element with the greatest projection`` (input : int array) =
        let struct (collection, elements) = this.Create input

        Assert.AreEqual<int>(
            Array.maxBy abs elements,
            ops.MaxBy abs collection,
            $"%s{ops.Name}.maxBy must return the first element with the greatest projection"
        )

    [<TestMethod>]
    member this.``maxBy throws for an empty collection as FSharp.Core does`` () =
        this.ThrowsLikeFSharpCore (
            (fun () -> Array.maxBy abs Array.empty<int>),
            (fun () -> List.maxBy abs List.empty<int>),
            (fun () -> ops.MaxBy abs (ops.OfSeq Inputs.Empty)),
            "maxBy of an empty collection must throw"
        )

    [<TestMethod; DynamicData(nameof Inputs.NonEmpty, typeof<Inputs>)>]
    member this.``min returns the smallest element`` (input : int array) =
        let struct (collection, elements) = this.Create input
        Assert.AreEqual<int>(Array.min elements, ops.Min collection, $"%s{ops.Name}.min must return the smallest element")

    [<TestMethod>]
    member this.``min throws for an empty collection as FSharp.Core does`` () =
        this.ThrowsLikeFSharpCore (
            (fun () -> Array.min Array.empty<int>),
            (fun () -> List.min List.empty<int>),
            (fun () -> ops.Min (ops.OfSeq Inputs.Empty)),
            "min of an empty collection must throw"
        )

    [<TestMethod; DynamicData(nameof Inputs.NonEmpty, typeof<Inputs>)>]
    member this.``minBy returns the first element with the smallest projection`` (input : int array) =
        let struct (collection, elements) = this.Create input

        Assert.AreEqual<int>(
            Array.minBy abs elements,
            ops.MinBy abs collection,
            $"%s{ops.Name}.minBy must return the first element with the smallest projection"
        )

    [<TestMethod>]
    member this.``minBy throws for an empty collection as FSharp.Core does`` () =
        this.ThrowsLikeFSharpCore (
            (fun () -> Array.minBy abs Array.empty<int>),
            (fun () -> List.minBy abs List.empty<int>),
            (fun () -> ops.MinBy abs (ops.OfSeq Inputs.Empty)),
            "minBy of an empty collection must throw"
        )

    [<TestMethod; DynamicData(nameof Inputs.NonEmpty, typeof<Inputs>)>]
    member this.``reduce combines the elements from the first one`` (input : int array) =
        let struct (collection, elements) = this.Create input

        Assert.AreEqual<int>(
            Array.reduce (fun a b -> a * 2 - b) elements,
            ops.Reduce (fun a b -> a * 2 - b) collection,
            $"%s{ops.Name}.reduce must combine the elements from the first one"
        )

    [<TestMethod>]
    member this.``reduce throws for an empty collection as FSharp.Core does`` () =
        this.ThrowsLikeFSharpCore (
            (fun () -> Array.reduce (+) Array.empty<int>),
            (fun () -> List.reduce (+) List.empty<int>),
            (fun () -> ops.Reduce (+) (ops.OfSeq Inputs.Empty)),
            "reduce of an empty collection must throw"
        )

    [<TestMethod; DynamicData(nameof Inputs.NonEmpty, typeof<Inputs>)>]
    member this.``reduceBack combines the elements from the last one`` (input : int array) =
        let struct (collection, elements) = this.Create input

        Assert.AreEqual<int>(
            Array.reduceBack (fun a b -> a * 2 - b) elements,
            ops.ReduceBack (fun a b -> a * 2 - b) collection,
            $"%s{ops.Name}.reduceBack must combine the elements from the last one"
        )

    [<TestMethod>]
    member this.``reduceBack throws for an empty collection as FSharp.Core does`` () =
        this.ThrowsLikeFSharpCore (
            (fun () -> Array.reduceBack (+) Array.empty<int>),
            (fun () -> List.reduceBack (+) List.empty<int>),
            (fun () -> ops.ReduceBack (+) (ops.OfSeq Inputs.Empty)),
            "reduceBack of an empty collection must throw"
        )

    [<TestMethod; DynamicData(nameof Inputs.All, typeof<Inputs>)>]
    member this.``scan returns every state from the first element`` (input : int array) =
        let struct (collection, elements) = this.Create input

        CollectionAssert.AreEqual (
            Array.scan record [||] elements,
            ops.Scan record [||] collection,
            $"%s{ops.Name}.scan must return every state from the first element"
        )

    [<TestMethod; DynamicData(nameof Inputs.All, typeof<Inputs>)>]
    member this.``scanBack returns every state from the last element`` (input : int array) =
        let struct (collection, elements) = this.Create input

        CollectionAssert.AreEqual (
            Array.scanBack recordBack elements [||],
            ops.ScanBack recordBack collection [||],
            $"%s{ops.Name}.scanBack must return every state from the last element"
        )

    [<TestMethod; DynamicData(nameof Inputs.All, typeof<Inputs>)>]
    member this.``sum adds the elements`` (input : int array) =
        let struct (collection, elements) = this.Create input
        Assert.AreEqual<int>(Array.sum elements, ops.Sum collection, $"%s{ops.Name}.sum must add the elements")

    [<TestMethod; DynamicData(nameof Inputs.All, typeof<Inputs>)>]
    member this.``sumBy adds the projections`` (input : int array) =
        let struct (collection, elements) = this.Create input
        Assert.AreEqual<int>(Array.sumBy abs elements, ops.SumBy abs collection, $"%s{ops.Name}.sumBy must add the projections")
