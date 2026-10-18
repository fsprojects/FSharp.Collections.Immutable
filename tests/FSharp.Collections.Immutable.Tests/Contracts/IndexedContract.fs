namespace FSharp.Collections.Immutable.Tests

open System.Collections.Generic
open FSharp.Collections.Immutable
open Microsoft.VisualStudio.TestTools.UnitTesting

/// <summary>
/// The contract of the index-based functions of <c>FlatList</c>, <c>IndexedList</c> and <c>IIndexedList</c>
/// (ADR 0001, Appendix A).
/// </summary>
[<AbstractClass>]
type IndexedContract<'C> (ops : IndexedOps<'C>) =
    inherit ContractBase (ops.Name, ops.Mirror, ops.ParamName)

    /// An equality comparer that ignores the sign, for the functions that take a comparer.
    static let absoluteEquality = {
        new IEqualityComparer<int> with
            member _.Equals (left, right) = abs left = abs right
            member _.GetHashCode item = abs item
    }

    /// The collection of the input with several elements and the elements that it enumerates.
    let several () =
        let collection = ops.OfSeq Inputs.several
        struct (collection, ops.ToList collection)

    [<TestMethod; TestCategory(nameof FlatList.item)>]
    member _.``item returns the element at the index`` () =
        let struct (collection, elements) = several ()

        for index in 0 .. elements.Length - 1 do
            Assert.AreEqual<int>(List.item index elements, ops.Item index collection, $"%s{ops.Name}.item %d{index}")

    [<TestMethod; TestCategory(nameof FlatList.item)>]
    member this.``item throws for an index out of range as FSharp.Core does`` () =
        let struct (collection, elements) = several ()

        for index in [ -1; elements.Length ] do
            this.ThrowsLikeFSharpCore (
                (fun () -> Array.item index (List.toArray elements)),
                (fun () -> List.item index elements),
                (fun () -> ops.Item index collection),
                $"item %d{index} must throw"
            )

    [<TestMethod; TestCategory(nameof FlatList.tryItem)>]
    member _.``tryItem returns the element at the index or nothing`` () =
        let struct (collection, elements) = several ()

        for index in -1 .. elements.Length do
            Assert.AreEqual<int voption>(
                List.tryItem index elements |> ValueOption.ofOption,
                ops.TryItem index collection,
                $"%s{ops.Name}.tryItem %d{index}"
            )

    [<TestMethod; TestCategory(nameof FlatList.indexOf)>]
    member _.``indexOf returns the index of the first equal element or -1`` () =
        let struct (collection, elements) = several ()

        for value in [ 5; -3; 7; 1000 ] do
            Assert.AreEqual<int>(
                List.tryFindIndex ((=) value) elements
                |> Option.defaultValue -1,
                ops.IndexOf value collection,
                $"%s{ops.Name}.indexOf %d{value}"
            )

    [<TestMethod; TestCategory(nameof FlatList.indexOfWith)>]
    member _.``indexOfWith compares with the equality comparer`` () =
        let struct (collection, elements) = several ()

        for value in [ -5; 3; -12; 1000 ] do
            Assert.AreEqual<int>(
                List.tryFindIndex (fun item -> abs item = abs value) elements
                |> Option.defaultValue -1,
                ops.IndexOfWith absoluteEquality value collection,
                $"%s{ops.Name}.indexOfWith %d{value}"
            )

    [<TestMethod; TestCategory(nameof FlatList.lastIndexOf)>]
    member _.``lastIndexOf returns the index of the last equal element or -1`` () =
        let struct (collection, elements) = several ()

        for value in [ 5; -3; 7; 1000 ] do
            Assert.AreEqual<int>(
                List.tryFindIndexBack ((=) value) elements
                |> Option.defaultValue -1,
                ops.LastIndexOf value collection,
                $"%s{ops.Name}.lastIndexOf %d{value}"
            )

        Assert.AreEqual<int>(-1, ops.LastIndexOf 5 (ops.OfSeq Inputs.empty), $"%s{ops.Name}.lastIndexOf in an empty collection")

    [<TestMethod; TestCategory(nameof FlatList.lastIndexOfWith)>]
    member _.``lastIndexOfWith compares with the equality comparer`` () =
        let struct (collection, elements) = several ()

        for value in [ -5; 3; -12; 1000 ] do
            Assert.AreEqual<int>(
                List.tryFindIndexBack (fun item -> abs item = abs value) elements
                |> Option.defaultValue -1,
                ops.LastIndexOfWith absoluteEquality value collection,
                $"%s{ops.Name}.lastIndexOfWith %d{value}"
            )

        Assert.AreEqual<int>(
            -1,
            ops.LastIndexOfWith absoluteEquality 5 (ops.OfSeq Inputs.empty),
            $"%s{ops.Name}.lastIndexOfWith in an empty collection"
        )

    [<TestMethod; TestCategory(nameof FlatList.add)>]
    member _.``add appends the value`` () =
        for input in Inputs.all do
            let collection = ops.OfSeq input

            Assert.AreEqual<int list>(
                ops.ToList collection @ [ 99 ],
                ops.ToList (ops.Add 99 collection),
                $"%s{ops.Name}.add to %A{input}"
            )

    [<TestMethod; TestCategory(nameof FlatList.insertAt)>]
    member _.``insertAt inserts the value at the index`` () =
        let struct (collection, elements) = several ()

        for index in [ 0; 3; elements.Length ] do
            Assert.AreEqual<int list>(
                List.insertAt index 99 elements,
                ops.ToList (ops.InsertAt index 99 collection),
                $"%s{ops.Name}.insertAt %d{index}"
            )

    [<TestMethod; TestCategory(nameof FlatList.insertAt)>]
    member this.``insertAt throws for an index out of range as FSharp.Core does`` () =
        let struct (collection, elements) = several ()

        for index in [ -1; elements.Length + 1 ] do
            this.ThrowsLikeFSharpCore (
                (fun () -> Array.insertAt index 99 (List.toArray elements)),
                (fun () -> List.insertAt index 99 elements),
                (fun () -> ops.InsertAt index 99 collection),
                $"insertAt %d{index} must throw"
            )

    [<TestMethod; TestCategory(nameof FlatList.insertManyAt)>]
    member _.``insertManyAt inserts the values at the index`` () =
        let struct (collection, elements) = several ()

        for index in [ 0; 3; elements.Length ] do
            for values in [ []; [ 98; 99 ] ] do
                Assert.AreEqual<int list>(
                    List.insertManyAt index values elements,
                    ops.ToList (ops.InsertManyAt index values collection),
                    $"%s{ops.Name}.insertManyAt %d{index} %A{values}"
                )

    [<TestMethod; TestCategory(nameof FlatList.insertManyAt)>]
    member this.``insertManyAt throws for an index out of range as FSharp.Core does`` () =
        let struct (collection, elements) = several ()

        for index in [ -1; elements.Length + 1 ] do
            this.ThrowsLikeFSharpCore (
                (fun () -> Array.insertManyAt index [ 99 ] (List.toArray elements)),
                (fun () -> List.insertManyAt index [ 99 ] elements),
                (fun () -> ops.InsertManyAt index [ 99 ] collection),
                $"insertManyAt %d{index} must throw"
            )

    [<TestMethod; TestCategory(nameof FlatList.removeAt)>]
    member _.``removeAt removes the element at the index`` () =
        let struct (collection, elements) = several ()

        for index in [ 0; 3; elements.Length - 1 ] do
            Assert.AreEqual<int list>(
                List.removeAt index elements,
                ops.ToList (ops.RemoveAt index collection),
                $"%s{ops.Name}.removeAt %d{index}"
            )

    [<TestMethod; TestCategory(nameof FlatList.removeAt)>]
    member this.``removeAt throws for an index out of range as FSharp.Core does`` () =
        let struct (collection, elements) = several ()

        for index in [ -1; elements.Length ] do
            this.ThrowsLikeFSharpCore (
                (fun () -> Array.removeAt index (List.toArray elements)),
                (fun () -> List.removeAt index elements),
                (fun () -> ops.RemoveAt index collection),
                $"removeAt %d{index} must throw"
            )

    [<TestMethod; TestCategory(nameof FlatList.removeManyAt)>]
    member _.``removeManyAt removes the elements of the range`` () =
        let struct (collection, elements) = several ()

        for struct (index, count) in [ struct (0, 2); struct (3, 3); struct (elements.Length - 1, 1); struct (2, 0) ] do
            Assert.AreEqual<int list>(
                List.removeManyAt index count elements,
                ops.ToList (ops.RemoveManyAt index count collection),
                $"%s{ops.Name}.removeManyAt %d{index} %d{count}"
            )

    [<TestMethod; TestCategory(nameof FlatList.removeManyAt)>]
    member this.``removeManyAt throws for a range out of range as FSharp.Core does`` () =
        let struct (collection, elements) = several ()

        for struct (index, count) in [ struct (-1, 1); struct (elements.Length - 1, 2); struct (0, -1) ] do
            this.ThrowsLikeFSharpCore (
                (fun () -> Array.removeManyAt index count (List.toArray elements)),
                (fun () -> List.removeManyAt index count elements),
                (fun () -> ops.RemoveManyAt index count collection),
                $"removeManyAt %d{index} %d{count} must throw"
            )

    [<TestMethod; TestCategory(nameof FlatList.updateAt)>]
    member _.``updateAt replaces the element at the index`` () =
        let struct (collection, elements) = several ()

        for index in [ 0; elements.Length - 1 ] do
            Assert.AreEqual<int list>(
                List.updateAt index 99 elements,
                ops.ToList (ops.UpdateAt index 99 collection),
                $"%s{ops.Name}.updateAt %d{index}"
            )

    [<TestMethod; TestCategory(nameof FlatList.updateAt)>]
    member this.``updateAt throws for an index out of range as FSharp.Core does`` () =
        let struct (collection, elements) = several ()

        for index in [ -1; elements.Length ] do
            this.ThrowsLikeFSharpCore (
                (fun () -> Array.updateAt index 99 (List.toArray elements)),
                (fun () -> List.updateAt index 99 elements),
                (fun () -> ops.UpdateAt index 99 collection),
                $"updateAt %d{index} must throw"
            )
