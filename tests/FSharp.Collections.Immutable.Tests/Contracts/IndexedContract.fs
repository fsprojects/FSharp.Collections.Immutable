namespace FSharp.Collections.Immutable.Tests

open System
open System.Collections.Generic
open FSharp.Collections.Immutable
open Microsoft.VisualStudio.TestTools.UnitTesting

/// <summary>
/// The contract of the index-based functions of <see cref="T:FSharp.Collections.Immutable.FlatList"/>,
/// <see cref="T:FSharp.Collections.Immutable.IndexedList"/> and
/// <see cref="T:FSharp.Collections.Immutable.IIndexedList"/> (ADR 0001, Appendix A).
/// </summary>
/// <remarks>
/// The tests run on the input with several elements; the indices of their rows are computed from its length.
/// </remarks>
[<AbstractClass>]
type IndexedContract<'C> (ops : IndexedOps<'C>) =
    inherit ContractBase (ops.Name, ops.Mirror, ops.ParamName)

    /// An equality comparer that ignores the sign, for the functions that take a comparer.
    static let absoluteEquality = {
        new IEqualityComparer<int> with
            member _.Equals (left, right) = abs left = abs right
            member _.GetHashCode item = abs item
    }

    /// The values of a kind that insertManyAt rejects: a null sequence, or a default flat list given as a sequence.
    static let invalidValues (kind : string) : seq<int> =
        if kind = "null sequence" then
            Unchecked.defaultof<seq<int>>
        else
            Inputs.DefaultFlatListAsSeq<int>()

    /// The collection of the input with several elements and the elements that it enumerates.
    let several () =
        let collection = ops.OfSeq Inputs.Several
        struct (collection, ops.ToArray collection)

    /// Every index of an element.
    static member Indices = seq { 0 .. Inputs.Several.Length - 1 }

    /// The indices next to the elements: before the first one and after the last one.
    static member IndicesOutOfRange = seq {
        -1
        Inputs.Several.Length
    }

    /// Every index of an element and the indices next to them.
    static member IndicesAndOutOfRange = seq { -1 .. Inputs.Several.Length }

    /// The indices to insert at: before the first element, inside and after the last one.
    static member InsertionIndices = seq {
        0
        3
        Inputs.Several.Length
    }

    /// The indices to insert at that are out of range: before the first element and beyond the one after the last.
    static member InsertionIndicesOutOfRange = seq {
        -1
        Inputs.Several.Length + 1
    }

    /// Each index to insert at with no values and with two values.
    static member InsertionIndicesAndValues = seq {
        for index in IndexedContract<'C>.InsertionIndices do
            for values in [| [||]; [| 98; 99 |] |] do
                struct (index, values)
    }

    /// The sequences that insertManyAt rejects, by kind: a null sequence and a default flat list given as a sequence.
    static member InvalidValueKinds = seq {
        "null sequence"
        "default flat list"
    }

    /// Each index to insert at that is out of range with each kind of the values that insertManyAt rejects.
    static member InsertionIndicesOutOfRangeAndInvalidValues = seq {
        for index in IndexedContract<'C>.InsertionIndicesOutOfRange do
            for kind in IndexedContract<'C>.InvalidValueKinds do
                struct (index, kind)
    }

    /// The indices to remove at: the first element, one inside and the last one.
    static member RemovalIndices = seq {
        0
        3
        Inputs.Several.Length - 1
    }

    /// The ranges to remove, as an index and a count: from the first element, inside, the last element, and no element.
    static member RemovalRanges = seq {
        struct (0, 2)
        struct (3, 3)
        struct (Inputs.Several.Length - 1, 1)
        struct (2, 0)
    }

    /// The ranges that are out of range, as an index and a count: before the first element and beyond the last one.
    static member RemovalRangesOutOfRange = seq {
        struct (-1, 1)
        struct (Inputs.Several.Length - 1, 2)
    }

    /// The ranges with a count that is not positive, as an index and a count. `List` removes nothing for them while the
    /// index plus the count stays within the list, and throws otherwise; `Array` copies by the count and throws or returns
    /// what the copies give.
    static member NonPositiveCountRanges = seq {
        struct (0, 0)
        struct (0, -1)
        struct (3, -2)
        struct (10, -5)
        struct (20, -5)
    }

    /// The ranges whose end, the index plus the count, overflows `Int32.MaxValue`, as an index and a count, and the count
    /// that reaches `Int32.MaxValue` from the first element. `List` walks the first index + count elements: a sum that
    /// wraps around to a negative number removes nothing, and a sum that stays positive and passes the end throws;
    /// `Array` throws for both.
    static member OverflowingRanges = seq {
        struct (1, Int32.MaxValue)
        struct (Inputs.Several.Length - 1, Int32.MaxValue)
        struct (Int32.MaxValue, 1)
        struct (0, Int32.MaxValue)
    }

    /// The indices of the first and the last element.
    static member FirstAndLastIndices = seq {
        0
        Inputs.Several.Length - 1
    }

    [<TestMethod; DynamicData(nameof IndexedContract<'C>.Indices)>]
    member _.``item returns the element at the index`` (index : int) =
        let struct (collection, elements) = several ()
        Assert.AreEqual<int>(
            Array.item index elements,
            ops.Item index collection,
            $"%s{ops.Name}.item must return the element at the index"
        )

    [<TestMethod; DynamicData(nameof IndexedContract<'C>.IndicesOutOfRange)>]
    member this.``item throws for an index out of range as FSharp.Core does`` (index : int) =
        let struct (collection, elements) = several ()

        this.ThrowsLikeFSharpCore (
            (fun () -> Array.item index elements),
            (fun () -> List.item index (List.ofArray elements)),
            (fun () -> ops.Item index collection),
            "item at an index out of range must throw"
        )

    [<TestMethod; DynamicData(nameof IndexedContract<'C>.IndicesAndOutOfRange)>]
    member _.``tryItem returns the element at the index or nothing`` (index : int) =
        let struct (collection, elements) = several ()

        Assert.AreEqual<int voption>(
            Array.tryItem index elements |> ValueOption.ofOption,
            ops.TryItem index collection,
            $"%s{ops.Name}.tryItem must return the element at the index or nothing"
        )

    [<TestMethod; DataRow 5; DataRow -3; DataRow 7; DataRow 1000>]
    member _.``indexOf returns the index of the first equal element or -1`` (value : int) =
        let struct (collection, elements) = several ()

        Assert.AreEqual<int>(
            Array.tryFindIndex ((=) value) elements
            |> Option.defaultValue -1,
            ops.IndexOf value collection,
            $"%s{ops.Name}.indexOf must return the index of the first equal element or -1"
        )

    [<TestMethod; DataRow -5; DataRow 3; DataRow -12; DataRow 1000>]
    member _.``indexOfWith compares with the equality comparer`` (value : int) =
        let struct (collection, elements) = several ()

        Assert.AreEqual<int>(
            Array.tryFindIndex (fun item -> abs item = abs value) elements
            |> Option.defaultValue -1,
            ops.IndexOfWith absoluteEquality value collection,
            $"%s{ops.Name}.indexOfWith must compare with the equality comparer"
        )

    [<TestMethod; DataRow 5; DataRow -3; DataRow 7; DataRow 1000>]
    member _.``lastIndexOf returns the index of the last equal element or -1`` (value : int) =
        let struct (collection, elements) = several ()

        Assert.AreEqual<int>(
            Array.tryFindIndexBack ((=) value) elements
            |> Option.defaultValue -1,
            ops.LastIndexOf value collection,
            $"%s{ops.Name}.lastIndexOf must return the index of the last equal element or -1"
        )

    [<TestMethod>]
    member _.``lastIndexOf returns -1 in an empty collection`` () =
        Assert.AreEqual<int>(
            -1,
            ops.LastIndexOf 5 (ops.OfSeq Inputs.Empty),
            $"%s{ops.Name}.lastIndexOf must return -1 in an empty collection"
        )

    [<TestMethod; DataRow -5; DataRow 3; DataRow -12; DataRow 1000>]
    member _.``lastIndexOfWith compares with the equality comparer`` (value : int) =
        let struct (collection, elements) = several ()

        Assert.AreEqual<int>(
            Array.tryFindIndexBack (fun item -> abs item = abs value) elements
            |> Option.defaultValue -1,
            ops.LastIndexOfWith absoluteEquality value collection,
            $"%s{ops.Name}.lastIndexOfWith must compare with the equality comparer"
        )

    [<TestMethod>]
    member _.``lastIndexOfWith returns -1 in an empty collection`` () =
        Assert.AreEqual<int>(
            -1,
            ops.LastIndexOfWith absoluteEquality 5 (ops.OfSeq Inputs.Empty),
            $"%s{ops.Name}.lastIndexOfWith must return -1 in an empty collection"
        )

    [<TestMethod; DynamicData(nameof Inputs.All, typeof<Inputs>)>]
    member _.``add appends the value`` (input : int array) =
        let collection = ops.OfSeq input

        CollectionAssert.AreEqual (
            Array.append (ops.ToArray collection) [| 99 |],
            ops.ToArray (ops.Add 99 collection),
            $"%s{ops.Name}.add must append the value"
        )

    [<TestMethod; DynamicData(nameof IndexedContract<'C>.InsertionIndices)>]
    member _.``insertAt inserts the value at the index`` (index : int) =
        let struct (collection, elements) = several ()

        CollectionAssert.AreEqual (
            Array.insertAt index 99 elements,
            ops.ToArray (ops.InsertAt index 99 collection),
            $"%s{ops.Name}.insertAt must insert the value at the index"
        )

    [<TestMethod; DynamicData(nameof IndexedContract<'C>.InsertionIndicesOutOfRange)>]
    member this.``insertAt throws for an index out of range as FSharp.Core does`` (index : int) =
        let struct (collection, elements) = several ()

        this.ThrowsLikeFSharpCore (
            (fun () -> Array.insertAt index 99 elements),
            (fun () -> List.insertAt index 99 (List.ofArray elements)),
            (fun () -> ops.InsertAt index 99 collection),
            "insertAt at an index out of range must throw"
        )

    [<TestMethod; DynamicData(nameof IndexedContract<'C>.InsertionIndicesAndValues)>]
    member _.``insertManyAt inserts the values at the index`` (index : int, values : int array) =
        let struct (collection, elements) = several ()

        CollectionAssert.AreEqual (
            Array.insertManyAt index values elements,
            ops.ToArray (ops.InsertManyAt index values collection),
            $"%s{ops.Name}.insertManyAt must insert the values at the index"
        )

    [<TestMethod; DynamicData(nameof IndexedContract<'C>.InsertionIndicesOutOfRange)>]
    member this.``insertManyAt throws for an index out of range as FSharp.Core does`` (index : int) =
        let struct (collection, elements) = several ()

        this.ThrowsLikeFSharpCore (
            (fun () -> Array.insertManyAt index [ 99 ] elements),
            (fun () -> List.insertManyAt index [ 99 ] (List.ofArray elements)),
            (fun () -> ops.InsertManyAt index [ 99 ] collection),
            "insertManyAt at an index out of range must throw"
        )

    [<TestMethod; DynamicData(nameof IndexedContract<'C>.InvalidValueKinds)>]
    member _.``insertManyAt rejects null values and a default flat list given as the values`` (kind : string) =
        let struct (collection, _) = several ()

        let ex =
            Assert.ThrowsExactly<ArgumentNullException>(
                Action (fun () -> ops.InsertManyAt 0 (invalidValues kind) collection |> ignore),
                $"%s{ops.Name}.insertManyAt must reject a %s{kind}"
            )

        Assert.AreEqual<string | null>("values", ex.ParamName, $"%s{ops.Name}.insertManyAt must name the values for a %s{kind}")

    [<TestMethod; DynamicData(nameof IndexedContract<'C>.InsertionIndicesOutOfRangeAndInvalidValues)>]
    member this.``insertManyAt checks the index before the values as FSharp.Core does`` (index : int, kind : string) =
        let struct (collection, elements) = several ()
        let values = invalidValues kind

        // The oracles check the index first and never enumerate the default flat list
        this.ThrowsLikeFSharpCore (
            (fun () -> Array.insertManyAt index values elements),
            (fun () -> List.insertManyAt index values (List.ofArray elements)),
            (fun () -> ops.InsertManyAt index values collection),
            $"insertManyAt at an index out of range with a %s{kind} must throw for the index"
        )

    [<TestMethod; DynamicData(nameof IndexedContract<'C>.RemovalIndices)>]
    member _.``removeAt removes the element at the index`` (index : int) =
        let struct (collection, elements) = several ()

        CollectionAssert.AreEqual (
            Array.removeAt index elements,
            ops.ToArray (ops.RemoveAt index collection),
            $"%s{ops.Name}.removeAt must remove the element at the index"
        )

    [<TestMethod; DynamicData(nameof IndexedContract<'C>.IndicesOutOfRange)>]
    member this.``removeAt throws for an index out of range as FSharp.Core does`` (index : int) =
        let struct (collection, elements) = several ()

        this.ThrowsLikeFSharpCore (
            (fun () -> Array.removeAt index elements),
            (fun () -> List.removeAt index (List.ofArray elements)),
            (fun () -> ops.RemoveAt index collection),
            "removeAt at an index out of range must throw"
        )

    [<TestMethod; DynamicData(nameof IndexedContract<'C>.RemovalRanges)>]
    member _.``removeManyAt removes the elements of the range`` (index : int, count : int) =
        let struct (collection, elements) = several ()

        CollectionAssert.AreEqual (
            Array.removeManyAt index count elements,
            ops.ToArray (ops.RemoveManyAt index count collection),
            $"%s{ops.Name}.removeManyAt must remove the elements of the range"
        )

    [<TestMethod; DynamicData(nameof IndexedContract<'C>.RemovalRangesOutOfRange)>]
    member this.``removeManyAt throws for a range out of range as FSharp.Core does`` (index : int, count : int) =
        let struct (collection, elements) = several ()

        this.ThrowsLikeFSharpCore (
            (fun () -> Array.removeManyAt index count elements),
            (fun () -> List.removeManyAt index count (List.ofArray elements)),
            (fun () -> ops.RemoveManyAt index count collection),
            "removeManyAt of a range out of range must throw"
        )

    [<TestMethod; DynamicData(nameof IndexedContract<'C>.NonPositiveCountRanges)>]
    member this.``removeManyAt of a count that is not positive does what FSharp.Core does`` (index : int, count : int) =
        this.RemovesManyAtLikeFSharpCore (index, count)

    [<TestMethod; DynamicData(nameof IndexedContract<'C>.OverflowingRanges)>]
    member this.``removeManyAt of a range whose end overflows does what FSharp.Core does`` (index : int, count : int) =
        this.RemovesManyAtLikeFSharpCore (index, count)

    /// Asserts that removeManyAt returns what the mirrored FSharp.Core function returns for the range, or throws what it
    /// throws.
    member private this.RemovesManyAtLikeFSharpCore (index : int, count : int) =
        let struct (collection, elements) = several ()
        let arrayOracle () = Array.removeManyAt index count elements

        let listOracle () =
            List.removeManyAt index count (List.ofArray elements)
            |> List.toArray

        let outcome =
            try
                Ok (this.Mirrored (arrayOracle, listOracle))
            with ex ->
                Error ex

        match outcome with
        | Ok expected ->
            CollectionAssert.AreEqual (
                expected,
                ops.ToArray (ops.RemoveManyAt index count collection),
                $"%s{ops.Name}.removeManyAt must return what FSharp.Core returns"
            )
        | Error _ ->
            this.ThrowsLikeFSharpCore (
                arrayOracle,
                listOracle,
                (fun () -> ops.RemoveManyAt index count collection),
                "removeManyAt must throw what FSharp.Core throws"
            )

    [<TestMethod; DynamicData(nameof IndexedContract<'C>.FirstAndLastIndices)>]
    member _.``updateAt replaces the element at the index`` (index : int) =
        let struct (collection, elements) = several ()

        CollectionAssert.AreEqual (
            Array.updateAt index 99 elements,
            ops.ToArray (ops.UpdateAt index 99 collection),
            $"%s{ops.Name}.updateAt must replace the element at the index"
        )

    [<TestMethod; DynamicData(nameof IndexedContract<'C>.IndicesOutOfRange)>]
    member this.``updateAt throws for an index out of range as FSharp.Core does`` (index : int) =
        let struct (collection, elements) = several ()

        this.ThrowsLikeFSharpCore (
            (fun () -> Array.updateAt index 99 elements),
            (fun () -> List.updateAt index 99 (List.ofArray elements)),
            (fun () -> ops.UpdateAt index 99 collection),
            "updateAt at an index out of range must throw"
        )
