namespace FSharp.Collections.Immutable.Tests

open FSharp.Collections.Immutable
open Microsoft.VisualStudio.TestTools.UnitTesting

/// The contract of the partitioning and grouping functions of a sequence-like module.
[<AbstractClass>]
type SeqLikePartitioningContract<'C> (ops : SeqOps<'C>) =
    inherit SeqLikeFoldingContract<'C> (ops)

    /// The counts to try on a collection of the given length: negative, zero, inside and the whole length.
    static let countsFor (length : int) = Array.distinct [| -1; 0; min 3 length; length |]

    /// Every input with each count to try on it: negative, zero, inside and the whole length.
    static member InputsAndCounts = seq {
        for input in Inputs.All do
            for count in countsFor input.Length do
                struct (input, count)
    }

    /// Every input with each count from zero to its length: zero, inside and the whole length.
    static member InputsAndCountsWithin = seq {
        for input in Inputs.All do
            for count in Array.distinct [| 0; min 3 input.Length; input.Length |] do
                struct (input, count)
    }

    /// Every input with each count to try on it and a count beyond its length.
    static member InputsAndCountsBeyond = seq {
        for input in Inputs.All do
            for count in Array.append (countsFor input.Length) [| input.Length + 5 |] do
                struct (input, count)
    }

    /// Every input with each limit of the predicates of skipWhile and takeWhile: below every element, between them and
    /// above every element.
    static member InputsAndLimits = seq {
        for input in Inputs.All do
            for limit in [| -10; 6; 100 |] do
                struct (input, limit)
    }

    /// Every input with each number of parts: one, inside the length, the length and beyond it.
    static member InputsAndPartCounts = seq {
        for input in Inputs.All do
            for count in [| 1; 3; 8; 9 |] do
                struct (input, count)
    }

    [<TestMethod; DynamicData(nameof Inputs.All, typeof<Inputs>)>]
    member this.``countBy counts the elements of each key in the order of the first occurrence`` (input : int array) =
        let struct (collection, elements) = this.Create input

        CollectionAssert.AreEqual (
            Array.countBy (fun item -> item % 3) elements
            |> Array.map (fun (key, count) -> struct (key, count)),
            ops.CountBy (fun item -> item % 3) collection,
            $"%s{ops.Name}.countBy must count the elements of each key in the order of the first occurrence"
        )

    [<TestMethod; DynamicData(nameof Inputs.All, typeof<Inputs>)>]
    member this.``distinct keeps the first occurrence of each element`` (input : int array) =
        let struct (collection, elements) = this.Create input

        CollectionAssert.AreEqual (
            Array.distinct elements,
            ops.ToArray (ops.Distinct collection),
            $"%s{ops.Name}.distinct must keep the first occurrence of each element"
        )

    [<TestMethod; DynamicData(nameof Inputs.All, typeof<Inputs>)>]
    member this.``distinctBy keeps the first element of each key`` (input : int array) =
        let struct (collection, elements) = this.Create input

        CollectionAssert.AreEqual (
            Array.distinctBy abs elements,
            ops.ToArray (ops.DistinctBy abs collection),
            $"%s{ops.Name}.distinctBy must keep the first element of each key"
        )

    [<TestMethod; DynamicData(nameof Inputs.All, typeof<Inputs>)>]
    member this.``except removes the excluded items and the duplicates`` (input : int array) =
        let struct (collection, elements) = this.Create input

        CollectionAssert.AreEqual (
            Array.except [ 5; 12 ] elements,
            ops.ToArray (ops.Except [ 5; 12 ] collection),
            $"%s{ops.Name}.except must remove the excluded items and the duplicates"
        )

    [<TestMethod>]
    member this.``except throws for null items to exclude as FSharp.Core does`` () =
        let excluded = Unchecked.defaultof<seq<int>>

        this.ThrowsLikeFSharpCore (
            (fun () -> Array.except excluded Inputs.Several),
            (fun () -> List.except excluded (List.ofArray Inputs.Several)),
            (fun () -> ops.Except excluded (ops.OfSeq Inputs.Several)),
            "except of null items must throw"
        )

    [<TestMethod; DynamicData(nameof Inputs.All, typeof<Inputs>)>]
    member this.``except rejects a default flat list given as the items to exclude as null items`` (input : int array) =
        let struct (collection, _) = this.Create input

        // FSharp.Core checks the items to exclude before it looks at the input, which an empty input does not even read
        ExceptionAssert.Same (
            id,
            (fun () ->
                ops.Except Unchecked.defaultof<seq<int>> collection
                |> ignore
            ),
            (fun () ->
                ops.Except (Inputs.DefaultFlatListAsSeq<int>()) collection
                |> ignore
            ),
            $"%s{ops.Name}.except of a default flat list must throw what it throws for null items"
        )

    [<TestMethod; DynamicData(nameof Inputs.All, typeof<Inputs>)>]
    member this.``groupBy groups the elements by key in the order of the first occurrence`` (input : int array) =
        let struct (collection, elements) = this.Create input
        let expected = Array.groupBy (fun item -> item % 3) elements
        let groups = ops.GroupBy (fun item -> item % 3) collection

        // A group pairs its key with an array, which a pair compares by reference, so the keys and the elements are
        // compared apart
        CollectionAssert.AreEqual (
            Array.map fst expected,
            groups |> Array.map (fun struct (key, _) -> key),
            $"%s{ops.Name}.groupBy must return the keys in the order of their first occurrence"
        )

        CollectionAssert.AreEqual (
            Array.map snd expected,
            groups |> Array.map (fun struct (_, items) -> items),
            $"%s{ops.Name}.groupBy must group the elements of each key in their order"
        )

    [<TestMethod; DynamicData(nameof Inputs.All, typeof<Inputs>)>]
    member this.``partition splits the elements by the predicate`` (input : int array) =
        let struct (collection, elements) = this.Create input
        let expectedAccepted, expectedRejected = Array.partition (fun item -> item > 0) elements
        let struct (accepted, rejected) = ops.Partition (fun item -> item > 0) collection

        CollectionAssert.AreEqual (
            expectedAccepted,
            ops.ToArray accepted,
            $"%s{ops.Name}.partition must return the elements that satisfy the predicate"
        )

        CollectionAssert.AreEqual (
            expectedRejected,
            ops.ToArray rejected,
            $"%s{ops.Name}.partition must return the elements that do not satisfy the predicate"
        )

    [<TestMethod; DynamicData(nameof Inputs.All, typeof<Inputs>)>]
    member this.``partitionWith splits the elements by the choice`` (input : int array) =
        let struct (collection, elements) = this.Create input

        let partitioner (item : int) =
            if item > 0 then
                Choice1Of2 item
            else
                Choice2Of2 (string item)

        let expectedFirst, expectedSecond = Array.partitionWith partitioner elements
        let struct (first, second) = ops.PartitionWith partitioner collection
        CollectionAssert.AreEqual (expectedFirst, first, $"%s{ops.Name}.partitionWith must return the first choices in order")
        CollectionAssert.AreEqual (expectedSecond, second, $"%s{ops.Name}.partitionWith must return the second choices in order")

    [<TestMethod; DynamicData(nameof SeqLikePartitioningContract<'C>.InputsAndCounts)>]
    member this.``skip drops the first elements`` (input : int array, count : int) =
        let struct (collection, elements) = this.Create input

        CollectionAssert.AreEqual (
            Array.skip (max count 0) elements,
            ops.ToArray (ops.Skip count collection),
            $"%s{ops.Name}.skip must drop the first elements"
        )

    [<TestMethod>]
    member this.``skip throws for a count beyond the length as FSharp.Core does`` () =
        this.ThrowsLikeFSharpCore (
            (fun () -> Array.skip 9 Inputs.Several),
            (fun () -> List.skip 9 (List.ofArray Inputs.Several)),
            (fun () -> ops.Skip 9 (ops.OfSeq Inputs.Several)),
            "skip beyond the length must throw"
        )

    [<TestMethod; DynamicData(nameof SeqLikePartitioningContract<'C>.InputsAndCounts)>]
    member this.``skipLast drops the last elements`` (input : int array, count : int) =
        let struct (collection, elements) = this.Create input

        CollectionAssert.AreEqual (
            Array.take (elements.Length - max count 0) elements,
            ops.ToArray (ops.SkipLast count collection),
            $"%s{ops.Name}.skipLast must drop the last elements"
        )

    [<TestMethod>]
    member this.``skipLast throws for a count beyond the length as skip does`` () =
        this.ThrowsLikeFSharpCore (
            (fun () -> Array.skip 9 Inputs.Several),
            (fun () -> List.skip 9 (List.ofArray Inputs.Several)),
            (fun () -> ops.SkipLast 9 (ops.OfSeq Inputs.Several)),
            "skipLast beyond the length must throw"
        )

    [<TestMethod; DynamicData(nameof SeqLikePartitioningContract<'C>.InputsAndLimits)>]
    member this.``skipWhile drops the first elements that satisfy the predicate`` (input : int array, limit : int) =
        let struct (collection, elements) = this.Create input

        CollectionAssert.AreEqual (
            Array.skipWhile (fun item -> item < limit) elements,
            ops.ToArray (ops.SkipWhile (fun item -> item < limit) collection),
            $"%s{ops.Name}.skipWhile must drop the first elements that satisfy the predicate"
        )

    [<TestMethod; DynamicData(nameof SeqLikePartitioningContract<'C>.InputsAndCountsWithin)>]
    member this.``splitAt splits the elements at the index`` (input : int array, index : int) =
        let struct (collection, elements) = this.Create input
        let expectedBefore, expectedAfter = Array.splitAt index elements
        let struct (before, after) = ops.SplitAt index collection

        CollectionAssert.AreEqual (
            expectedBefore,
            ops.ToArray before,
            $"%s{ops.Name}.splitAt must return the elements before the index first"
        )

        CollectionAssert.AreEqual (
            expectedAfter,
            ops.ToArray after,
            $"%s{ops.Name}.splitAt must return the elements from the index second"
        )

    [<TestMethod; DataRow -1; DataRow 9>]
    member this.``splitAt throws for a negative index or one beyond the length as FSharp.Core does`` (index : int) =
        this.ThrowsLikeFSharpCore (
            (fun () -> Array.splitAt index Inputs.Several),
            (fun () -> List.splitAt index (List.ofArray Inputs.Several)),
            (fun () -> ops.SplitAt index (ops.OfSeq Inputs.Several)),
            "splitAt at a negative index or beyond the length must throw"
        )

    [<TestMethod; DynamicData(nameof SeqLikePartitioningContract<'C>.InputsAndPartCounts)>]
    member this.``splitInto splits the elements into parts of nearly equal length`` (input : int array, count : int) =
        let struct (collection, elements) = this.Create input

        CollectionAssert.AreEqual (
            Array.splitInto count elements,
            ops.SplitInto count collection,
            $"%s{ops.Name}.splitInto must split the elements into parts of nearly equal length"
        )

    [<TestMethod>]
    member this.``splitInto throws for a count that is not positive as FSharp.Core does`` () =
        this.ThrowsLikeFSharpCore (
            (fun () -> Array.splitInto 0 Inputs.Several),
            (fun () -> List.splitInto 0 (List.ofArray Inputs.Several)),
            (fun () -> ops.SplitInto 0 (ops.OfSeq Inputs.Several)),
            "splitInto 0 must throw"
        )

    [<TestMethod; DynamicData(nameof SeqLikePartitioningContract<'C>.InputsAndCountsWithin)>]
    member this.``take returns the first elements`` (input : int array, count : int) =
        let struct (collection, elements) = this.Create input

        CollectionAssert.AreEqual (
            Array.take count elements,
            ops.ToArray (ops.Take count collection),
            $"%s{ops.Name}.take must return the first elements"
        )

    [<TestMethod; DataRow -1; DataRow 9>]
    member this.``take throws for a negative count or one beyond the length as FSharp.Core does`` (count : int) =
        this.ThrowsLikeFSharpCore (
            (fun () -> Array.take count Inputs.Several),
            (fun () -> List.take count (List.ofArray Inputs.Several)),
            (fun () -> ops.Take count (ops.OfSeq Inputs.Several)),
            "take of a negative count or beyond the length must throw"
        )

    [<TestMethod; DynamicData(nameof SeqLikePartitioningContract<'C>.InputsAndCountsWithin)>]
    member this.``takeLast returns the last elements`` (input : int array, count : int) =
        let struct (collection, elements) = this.Create input

        CollectionAssert.AreEqual (
            Array.skip (elements.Length - count) elements,
            ops.ToArray (ops.TakeLast count collection),
            $"%s{ops.Name}.takeLast must return the last elements"
        )

    [<TestMethod; DataRow -1; DataRow 9>]
    member this.``takeLast throws for a negative count or one beyond the length as take does`` (count : int) =
        this.ThrowsLikeFSharpCore (
            (fun () -> Array.take count Inputs.Several),
            (fun () -> List.take count (List.ofArray Inputs.Several)),
            (fun () -> ops.TakeLast count (ops.OfSeq Inputs.Several)),
            "takeLast of a negative count or beyond the length must throw"
        )

    [<TestMethod; DynamicData(nameof SeqLikePartitioningContract<'C>.InputsAndLimits)>]
    member this.``takeWhile returns the first elements that satisfy the predicate`` (input : int array, limit : int) =
        let struct (collection, elements) = this.Create input

        CollectionAssert.AreEqual (
            Array.takeWhile (fun item -> item < limit) elements,
            ops.ToArray (ops.TakeWhile (fun item -> item < limit) collection),
            $"%s{ops.Name}.takeWhile must return the first elements that satisfy the predicate"
        )

    [<TestMethod; DynamicData(nameof SeqLikePartitioningContract<'C>.InputsAndCountsBeyond)>]
    member this.``truncate returns at most the given number of first elements`` (input : int array, count : int) =
        let struct (collection, elements) = this.Create input

        CollectionAssert.AreEqual (
            Array.truncate count elements,
            ops.ToArray (ops.Truncate count collection),
            $"%s{ops.Name}.truncate must return at most the given number of first elements"
        )
