namespace FSharp.Collections.Immutable.Tests

open FSharp.Collections.Immutable
open Microsoft.VisualStudio.TestTools.UnitTesting

/// The contract of the folding functions of a sequence-like module.
[<AbstractClass>]
type SeqLikeFoldingContract<'C> (ops : SeqOps<'C>) =
    inherit SeqLikeTransformationContract<'C> (ops)

    /// A folder whose state records the order in which it saw the elements.
    static let record (state : int list) (item : int) = item :: state

    /// A folder from the end whose state records the order in which it saw the elements.
    static let recordBack (item : int) (state : int list) = item :: state

    [<TestMethod; TestCategory(nameof FlatList.average)>]
    member _.``average returns the average of the elements`` () =
        for input in [ [ 1.5 ]; [ 1.0; 2.0; 4.5; -3.0 ] ] do
            Assert.AreEqual<float>(List.average input, ops.Average input, $"%s{ops.Name}.average of %A{input}")

    [<TestMethod; TestCategory(nameof FlatList.average)>]
    member this.``average throws for an empty collection as FSharp.Core does`` () =
        this.ThrowsLikeFSharpCore (
            (fun () -> Array.average Array.empty<float>),
            (fun () -> List.average List.empty<float>),
            (fun () -> ops.Average []),
            "average of an empty collection must throw"
        )

    [<TestMethod; TestCategory(nameof FlatList.averageBy)>]
    member this.``averageBy returns the average of the projections`` () =
        this.ForEach (
            Inputs.nonEmpty,
            fun collection elements ->
                Assert.AreEqual<float>(
                    List.averageBy float elements,
                    ops.AverageBy float collection,
                    $"%s{ops.Name}.averageBy of %A{elements}"
                )
        )

    [<TestMethod; TestCategory(nameof FlatList.averageBy)>]
    member this.``averageBy throws for an empty collection as FSharp.Core does`` () =
        this.ThrowsLikeFSharpCore (
            (fun () -> Array.averageBy float Array.empty<int>),
            (fun () -> List.averageBy float List.empty<int>),
            (fun () -> ops.AverageBy float (ops.OfSeq Inputs.empty)),
            "averageBy of an empty collection must throw"
        )

    [<TestMethod; TestCategory(nameof FlatList.compareWith)>]
    member _.``compareWith compares element by element and then by length`` () =
        for first in Inputs.all do
            for second in Inputs.all @ [ [ 5; -3 ]; [ 5; 0 ] ] do
                let collection = ops.OfSeq first
                let other = ops.OfSeq second

                Assert.AreEqual<int>(
                    List.compareWith compare (ops.ToList collection) (ops.ToList other)
                    |> sign,
                    ops.CompareWith compare collection other |> sign,
                    $"%s{ops.Name}.compareWith of %A{first} and %A{second}"
                )

    [<TestMethod; TestCategory(nameof FlatList.fold)>]
    member this.``fold visits the elements from the first one`` () =
        this.ForEach (
            Inputs.all,
            fun collection elements ->
                Assert.AreEqual<int list>(
                    List.fold record [] elements,
                    ops.Fold record [] collection,
                    $"%s{ops.Name}.fold over %A{elements}"
                )
        )

    [<TestMethod; TestCategory(nameof FlatList.fold2)>]
    member _.``fold2 visits the pairs from the first index`` () =
        let collection = ops.OfSeq Inputs.several
        let other = ops.OfSeq Inputs.other
        let folder (state : int list) a b = a * 100 + b :: state

        Assert.AreEqual<int list>(
            List.fold2 folder [] (ops.ToList collection) (ops.ToList other),
            ops.Fold2 folder [] collection other,
            $"%s{ops.Name}.fold2 over two collections"
        )

    [<TestMethod; TestCategory(nameof FlatList.fold2)>]
    member this.``fold2 throws for different lengths as FSharp.Core does`` () =
        let folder (state : int list) (_ : int) (_ : int) = state

        this.ThrowsLikeFSharpCore (
            (fun () -> Array.fold2 folder [] (List.toArray Inputs.singleton) (List.toArray Inputs.several)),
            (fun () -> List.fold2 folder [] Inputs.singleton Inputs.several),
            (fun () -> ops.Fold2 folder [] (ops.OfSeq Inputs.singleton) (ops.OfSeq Inputs.several)),
            "fold2 of collections of different lengths must throw"
        )

    [<TestMethod; TestCategory(nameof FlatList.foldBack)>]
    member this.``foldBack visits the elements from the last one`` () =
        this.ForEach (
            Inputs.all,
            fun collection elements ->
                Assert.AreEqual<int list>(
                    List.foldBack recordBack elements [],
                    ops.FoldBack recordBack collection [],
                    $"%s{ops.Name}.foldBack over %A{elements}"
                )
        )

    [<TestMethod; TestCategory(nameof FlatList.foldBack2)>]
    member _.``foldBack2 visits the pairs from the last index`` () =
        let collection = ops.OfSeq Inputs.several
        let other = ops.OfSeq Inputs.other
        let folder a b (state : int list) = a * 100 + b :: state

        Assert.AreEqual<int list>(
            List.foldBack2 folder (ops.ToList collection) (ops.ToList other) [],
            ops.FoldBack2 folder collection other [],
            $"%s{ops.Name}.foldBack2 over two collections"
        )

    [<TestMethod; TestCategory(nameof FlatList.foldBack2)>]
    member this.``foldBack2 throws for different lengths as FSharp.Core does`` () =
        let folder (_ : int) (_ : int) (state : int list) = state

        this.ThrowsLikeFSharpCore (
            (fun () -> Array.foldBack2 folder (List.toArray Inputs.singleton) (List.toArray Inputs.several) []),
            (fun () -> List.foldBack2 folder Inputs.singleton Inputs.several []),
            (fun () -> ops.FoldBack2 folder (ops.OfSeq Inputs.singleton) (ops.OfSeq Inputs.several) []),
            "foldBack2 of collections of different lengths must throw"
        )

    [<TestMethod; TestCategory(nameof FlatList.iter)>]
    member this.``iter visits every element in order`` () =
        this.ForEach (
            Inputs.all,
            fun collection elements ->
                let visited = ResizeArray<int>()
                ops.Iter visited.Add collection
                Assert.AreEqual<int list>(elements, List.ofSeq visited, $"%s{ops.Name}.iter over %A{elements}")
        )

    [<TestMethod; TestCategory(nameof FlatList.iter2)>]
    member _.``iter2 visits every pair in order`` () =
        let collection = ops.OfSeq Inputs.several
        let other = ops.OfSeq Inputs.other
        let visited = ResizeArray<struct (int * int)>()
        ops.Iter2 (fun a b -> visited.Add (struct (a, b))) collection other

        Assert.AreEqual<struct (int * int) list>(
            List.zip (ops.ToList collection) (ops.ToList other)
            |> List.map (fun (a, b) -> struct (a, b)),
            List.ofSeq visited,
            $"%s{ops.Name}.iter2 over two collections"
        )

    [<TestMethod; TestCategory(nameof FlatList.iter2)>]
    member this.``iter2 throws for different lengths as FSharp.Core does`` () =
        this.ThrowsLikeFSharpCore (
            (fun () -> Array.iter2 (fun _ _ -> ()) (List.toArray Inputs.singleton) (List.toArray Inputs.several)),
            (fun () -> List.iter2 (fun _ _ -> ()) Inputs.singleton Inputs.several),
            (fun () -> ops.Iter2 (fun _ _ -> ()) (ops.OfSeq Inputs.singleton) (ops.OfSeq Inputs.several)),
            "iter2 of collections of different lengths must throw"
        )

    [<TestMethod; TestCategory(nameof FlatList.iteri)>]
    member this.``iteri visits every element with its index`` () =
        this.ForEach (
            Inputs.all,
            fun collection elements ->
                let visited = ResizeArray<struct (int * int)>()
                ops.Iteri (fun index item -> visited.Add (struct (index, item))) collection

                Assert.AreEqual<struct (int * int) list>(
                    List.indexed elements
                    |> List.map (fun (index, item) -> struct (index, item)),
                    List.ofSeq visited,
                    $"%s{ops.Name}.iteri over %A{elements}"
                )
        )

    [<TestMethod; TestCategory(nameof FlatList.iteri2)>]
    member _.``iteri2 visits every pair with its index`` () =
        let collection = ops.OfSeq Inputs.several
        let other = ops.OfSeq Inputs.other
        let visited = ResizeArray<struct (int * int * int)>()
        ops.Iteri2 (fun index a b -> visited.Add (struct (index, a, b))) collection other

        Assert.AreEqual<struct (int * int * int) list>(
            List.zip (ops.ToList collection) (ops.ToList other)
            |> List.mapi (fun index (a, b) -> struct (index, a, b)),
            List.ofSeq visited,
            $"%s{ops.Name}.iteri2 over two collections"
        )

    [<TestMethod; TestCategory(nameof FlatList.iteri2)>]
    member this.``iteri2 throws for different lengths as FSharp.Core does`` () =
        this.ThrowsLikeFSharpCore (
            (fun () -> Array.iteri2 (fun _ _ _ -> ()) (List.toArray Inputs.singleton) (List.toArray Inputs.several)),
            (fun () -> List.iteri2 (fun _ _ _ -> ()) Inputs.singleton Inputs.several),
            (fun () -> ops.Iteri2 (fun _ _ _ -> ()) (ops.OfSeq Inputs.singleton) (ops.OfSeq Inputs.several)),
            "iteri2 of collections of different lengths must throw"
        )

    [<TestMethod; TestCategory(nameof FlatList.mapFold)>]
    member this.``mapFold maps the elements and threads the state from the first one`` () =
        this.ForEach (
            Inputs.all,
            fun collection elements ->
                let expectedResults, expectedState = List.mapFold (fun state item -> item * state, state + 1) 1 elements
                let struct (results, state) =
                    ops.MapFold (fun state item -> struct (item * state, state + 1)) 1 collection
                Assert.AreEqual<int list>(expectedResults, results, $"%s{ops.Name}.mapFold results over %A{elements}")
                Assert.AreEqual<int>(expectedState, state, $"%s{ops.Name}.mapFold state over %A{elements}")
        )

    [<TestMethod; TestCategory(nameof FlatList.mapFoldBack)>]
    member this.``mapFoldBack maps the elements and threads the state from the last one`` () =
        this.ForEach (
            Inputs.all,
            fun collection elements ->
                let expectedResults, expectedState =
                    List.mapFoldBack (fun item state -> item * state, state + 1) elements 1

                let struct (results, state) =
                    ops.MapFoldBack (fun item state -> struct (item * state, state + 1)) collection 1
                Assert.AreEqual<int list>(expectedResults, results, $"%s{ops.Name}.mapFoldBack results over %A{elements}")
                Assert.AreEqual<int>(expectedState, state, $"%s{ops.Name}.mapFoldBack state over %A{elements}")
        )

    [<TestMethod; TestCategory(nameof FlatList.max)>]
    member this.``max returns the greatest element`` () =
        this.ForEach (
            Inputs.nonEmpty,
            fun collection elements ->
                Assert.AreEqual<int>(List.max elements, ops.Max collection, $"%s{ops.Name}.max of %A{elements}")
        )

    [<TestMethod; TestCategory(nameof FlatList.max)>]
    member this.``max throws for an empty collection as FSharp.Core does`` () =
        this.ThrowsLikeFSharpCore (
            (fun () -> Array.max Array.empty<int>),
            (fun () -> List.max List.empty<int>),
            (fun () -> ops.Max (ops.OfSeq Inputs.empty)),
            "max of an empty collection must throw"
        )

    [<TestMethod; TestCategory(nameof FlatList.maxBy)>]
    member this.``maxBy returns the first element with the greatest projection`` () =
        this.ForEach (
            Inputs.nonEmpty,
            fun collection elements ->
                Assert.AreEqual<int>(List.maxBy abs elements, ops.MaxBy abs collection, $"%s{ops.Name}.maxBy of %A{elements}")
        )

    [<TestMethod; TestCategory(nameof FlatList.maxBy)>]
    member this.``maxBy throws for an empty collection as FSharp.Core does`` () =
        this.ThrowsLikeFSharpCore (
            (fun () -> Array.maxBy abs Array.empty<int>),
            (fun () -> List.maxBy abs List.empty<int>),
            (fun () -> ops.MaxBy abs (ops.OfSeq Inputs.empty)),
            "maxBy of an empty collection must throw"
        )

    [<TestMethod; TestCategory(nameof FlatList.min)>]
    member this.``min returns the smallest element`` () =
        this.ForEach (
            Inputs.nonEmpty,
            fun collection elements ->
                Assert.AreEqual<int>(List.min elements, ops.Min collection, $"%s{ops.Name}.min of %A{elements}")
        )

    [<TestMethod; TestCategory(nameof FlatList.min)>]
    member this.``min throws for an empty collection as FSharp.Core does`` () =
        this.ThrowsLikeFSharpCore (
            (fun () -> Array.min Array.empty<int>),
            (fun () -> List.min List.empty<int>),
            (fun () -> ops.Min (ops.OfSeq Inputs.empty)),
            "min of an empty collection must throw"
        )

    [<TestMethod; TestCategory(nameof FlatList.minBy)>]
    member this.``minBy returns the first element with the smallest projection`` () =
        this.ForEach (
            Inputs.nonEmpty,
            fun collection elements ->
                Assert.AreEqual<int>(List.minBy abs elements, ops.MinBy abs collection, $"%s{ops.Name}.minBy of %A{elements}")
        )

    [<TestMethod; TestCategory(nameof FlatList.minBy)>]
    member this.``minBy throws for an empty collection as FSharp.Core does`` () =
        this.ThrowsLikeFSharpCore (
            (fun () -> Array.minBy abs Array.empty<int>),
            (fun () -> List.minBy abs List.empty<int>),
            (fun () -> ops.MinBy abs (ops.OfSeq Inputs.empty)),
            "minBy of an empty collection must throw"
        )

    [<TestMethod; TestCategory(nameof FlatList.reduce)>]
    member this.``reduce combines the elements from the first one`` () =
        this.ForEach (
            Inputs.nonEmpty,
            fun collection elements ->
                Assert.AreEqual<int>(
                    List.reduce (fun a b -> a * 2 - b) elements,
                    ops.Reduce (fun a b -> a * 2 - b) collection,
                    $"%s{ops.Name}.reduce of %A{elements}"
                )
        )

    [<TestMethod; TestCategory(nameof FlatList.reduce)>]
    member this.``reduce throws for an empty collection as FSharp.Core does`` () =
        this.ThrowsLikeFSharpCore (
            (fun () -> Array.reduce (+) Array.empty<int>),
            (fun () -> List.reduce (+) List.empty<int>),
            (fun () -> ops.Reduce (+) (ops.OfSeq Inputs.empty)),
            "reduce of an empty collection must throw"
        )

    [<TestMethod; TestCategory(nameof FlatList.reduceBack)>]
    member this.``reduceBack combines the elements from the last one`` () =
        this.ForEach (
            Inputs.nonEmpty,
            fun collection elements ->
                Assert.AreEqual<int>(
                    List.reduceBack (fun a b -> a * 2 - b) elements,
                    ops.ReduceBack (fun a b -> a * 2 - b) collection,
                    $"%s{ops.Name}.reduceBack of %A{elements}"
                )
        )

    [<TestMethod; TestCategory(nameof FlatList.reduceBack)>]
    member this.``reduceBack throws for an empty collection as FSharp.Core does`` () =
        this.ThrowsLikeFSharpCore (
            (fun () -> Array.reduceBack (+) Array.empty<int>),
            (fun () -> List.reduceBack (+) List.empty<int>),
            (fun () -> ops.ReduceBack (+) (ops.OfSeq Inputs.empty)),
            "reduceBack of an empty collection must throw"
        )

    [<TestMethod; TestCategory(nameof FlatList.scan)>]
    member this.``scan returns every state from the first element`` () =
        this.ForEach (
            Inputs.all,
            fun collection elements ->
                Assert.AreEqual<int list list>(
                    List.scan record [] elements,
                    ops.Scan record [] collection,
                    $"%s{ops.Name}.scan over %A{elements}"
                )
        )

    [<TestMethod; TestCategory(nameof FlatList.scanBack)>]
    member this.``scanBack returns every state from the last element`` () =
        this.ForEach (
            Inputs.all,
            fun collection elements ->
                Assert.AreEqual<int list list>(
                    List.scanBack recordBack elements [],
                    ops.ScanBack recordBack collection [],
                    $"%s{ops.Name}.scanBack over %A{elements}"
                )
        )

    [<TestMethod; TestCategory(nameof FlatList.sum)>]
    member this.``sum adds the elements`` () =
        this.ForEach (
            Inputs.all,
            fun collection elements ->
                Assert.AreEqual<int>(List.sum elements, ops.Sum collection, $"%s{ops.Name}.sum of %A{elements}")
        )

    [<TestMethod; TestCategory(nameof FlatList.sumBy)>]
    member this.``sumBy adds the projections`` () =
        this.ForEach (
            Inputs.all,
            fun collection elements ->
                Assert.AreEqual<int>(List.sumBy abs elements, ops.SumBy abs collection, $"%s{ops.Name}.sumBy of %A{elements}")
        )
