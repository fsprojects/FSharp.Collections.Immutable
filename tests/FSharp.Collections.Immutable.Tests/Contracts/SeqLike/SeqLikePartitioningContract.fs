namespace FSharp.Collections.Immutable.Tests

open FSharp.Collections.Immutable
open Microsoft.VisualStudio.TestTools.UnitTesting

/// The contract of the partitioning and grouping functions of a sequence-like module.
[<AbstractClass>]
type SeqLikePartitioningContract<'C> (ops : SeqOps<'C>) =
    inherit SeqLikeFoldingContract<'C> (ops)

    /// The counts to try on a collection of the given length: negative, zero, inside and the whole length.
    static let countsFor (length : int) = List.distinct [ -1; 0; min 3 length; length ]

    [<TestMethod>]
    member this.``countBy counts the elements of each key in the order of the first occurrence`` () =
        this.ForEach (
            Inputs.all,
            fun collection elements ->
                Assert.AreEqual<struct (int * int) list>(
                    List.countBy (fun item -> item % 3) elements
                    |> List.map (fun (key, count) -> struct (key, count)),
                    ops.CountBy (fun item -> item % 3) collection,
                    $"%s{ops.Name}.countBy of %A{elements}"
                )
        )

    [<TestMethod>]
    member this.``distinct keeps the first occurrence of each element`` () =
        this.ForEach (
            Inputs.all,
            fun collection elements ->
                Assert.AreEqual<int list>(
                    List.distinct elements,
                    ops.ToList (ops.Distinct collection),
                    $"%s{ops.Name}.distinct of %A{elements}"
                )
        )

    [<TestMethod>]
    member this.``distinctBy keeps the first element of each key`` () =
        this.ForEach (
            Inputs.all,
            fun collection elements ->
                Assert.AreEqual<int list>(
                    List.distinctBy abs elements,
                    ops.ToList (ops.DistinctBy abs collection),
                    $"%s{ops.Name}.distinctBy of %A{elements}"
                )
        )

    [<TestMethod>]
    member this.``except removes the excluded items and the duplicates`` () =
        this.ForEach (
            Inputs.all,
            fun collection elements ->
                Assert.AreEqual<int list>(
                    List.except [ 5; 12 ] elements,
                    ops.ToList (ops.Except [ 5; 12 ] collection),
                    $"%s{ops.Name}.except of %A{elements}"
                )
        )

    [<TestMethod>]
    member this.``except throws for null items to exclude as FSharp.Core does`` () =
        let excluded = Unchecked.defaultof<seq<int>>

        this.ThrowsLikeFSharpCore (
            (fun () -> Array.except excluded (List.toArray Inputs.several)),
            (fun () -> List.except excluded Inputs.several),
            (fun () -> ops.Except excluded (ops.OfSeq Inputs.several)),
            "except of null items must throw"
        )

    [<TestMethod>]
    member this.``groupBy groups the elements by key in the order of the first occurrence`` () =
        this.ForEach (
            Inputs.all,
            fun collection elements ->
                Assert.AreEqual<struct (int * int list) list>(
                    List.groupBy (fun item -> item % 3) elements
                    |> List.map (fun (key, items) -> struct (key, items)),
                    ops.GroupBy (fun item -> item % 3) collection,
                    $"%s{ops.Name}.groupBy of %A{elements}"
                )
        )

    [<TestMethod>]
    member this.``partition splits the elements by the predicate`` () =
        this.ForEach (
            Inputs.all,
            fun collection elements ->
                let expectedAccepted, expectedRejected = List.partition (fun item -> item > 0) elements
                let struct (accepted, rejected) = ops.Partition (fun item -> item > 0) collection
                Assert.AreEqual<int list>(
                    expectedAccepted,
                    ops.ToList accepted,
                    $"%s{ops.Name}.partition accepted of %A{elements}"
                )
                Assert.AreEqual<int list>(
                    expectedRejected,
                    ops.ToList rejected,
                    $"%s{ops.Name}.partition rejected of %A{elements}"
                )
        )

    [<TestMethod>]
    member this.``partitionWith splits the elements by the choice`` () =
        this.ForEach (
            Inputs.all,
            fun collection elements ->
                let partitioner (item : int) =
                    if item > 0 then
                        Choice1Of2 item
                    else
                        Choice2Of2 (string item)
                let expectedFirst, expectedSecond = List.partitionWith partitioner elements
                let struct (first, second) = ops.PartitionWith partitioner collection
                Assert.AreEqual<int list>(expectedFirst, first, $"%s{ops.Name}.partitionWith first choices of %A{elements}")
                Assert.AreEqual<string list>(expectedSecond, second, $"%s{ops.Name}.partitionWith second choices of %A{elements}")
        )

    [<TestMethod>]
    member this.``skip drops the first elements`` () =
        this.ForEach (
            Inputs.all,
            fun collection elements ->
                for count in countsFor elements.Length do
                    Assert.AreEqual<int list>(
                        List.skip (max count 0) elements,
                        ops.ToList (ops.Skip count collection),
                        $"%s{ops.Name}.skip %d{count} of %A{elements}"
                    )
        )

    [<TestMethod>]
    member this.``skip throws for a count beyond the length as FSharp.Core does`` () =
        this.ThrowsLikeFSharpCore (
            (fun () -> Array.skip 9 (List.toArray Inputs.several)),
            (fun () -> List.skip 9 Inputs.several),
            (fun () -> ops.Skip 9 (ops.OfSeq Inputs.several)),
            "skip beyond the length must throw"
        )

    [<TestMethod>]
    member this.``skipLast drops the last elements`` () =
        this.ForEach (
            Inputs.all,
            fun collection elements ->
                for count in countsFor elements.Length do
                    Assert.AreEqual<int list>(
                        List.take (elements.Length - max count 0) elements,
                        ops.ToList (ops.SkipLast count collection),
                        $"%s{ops.Name}.skipLast %d{count} of %A{elements}"
                    )
        )

    [<TestMethod>]
    member this.``skipLast throws for a count beyond the length as skip does`` () =
        this.ThrowsLikeFSharpCore (
            (fun () -> Array.skip 9 (List.toArray Inputs.several)),
            (fun () -> List.skip 9 Inputs.several),
            (fun () -> ops.SkipLast 9 (ops.OfSeq Inputs.several)),
            "skipLast beyond the length must throw"
        )

    [<TestMethod>]
    member this.``skipWhile drops the first elements that satisfy the predicate`` () =
        this.ForEach (
            Inputs.all,
            fun collection elements ->
                for limit in [ -10; 6; 100 ] do
                    Assert.AreEqual<int list>(
                        List.skipWhile (fun item -> item < limit) elements,
                        ops.ToList (ops.SkipWhile (fun item -> item < limit) collection),
                        $"%s{ops.Name}.skipWhile below %d{limit} of %A{elements}"
                    )
        )

    [<TestMethod>]
    member this.``splitAt splits the elements at the index`` () =
        this.ForEach (
            Inputs.all,
            fun collection elements ->
                for index in List.distinct [ 0; min 3 elements.Length; elements.Length ] do
                    let expectedBefore, expectedAfter = List.splitAt index elements
                    let struct (before, after) = ops.SplitAt index collection

                    Assert.AreEqual<int list>(
                        expectedBefore,
                        ops.ToList before,
                        $"%s{ops.Name}.splitAt %d{index} first part of %A{elements}"
                    )

                    Assert.AreEqual<int list>(
                        expectedAfter,
                        ops.ToList after,
                        $"%s{ops.Name}.splitAt %d{index} second part of %A{elements}"
                    )
        )

    [<TestMethod>]
    member this.``splitAt throws for a negative index or one beyond the length as FSharp.Core does`` () =
        for index in [ -1; 9 ] do
            this.ThrowsLikeFSharpCore (
                (fun () -> Array.splitAt index (List.toArray Inputs.several)),
                (fun () -> List.splitAt index Inputs.several),
                (fun () -> ops.SplitAt index (ops.OfSeq Inputs.several)),
                $"splitAt %d{index} must throw"
            )

    [<TestMethod>]
    member this.``splitInto splits the elements into parts of nearly equal length`` () =
        this.ForEach (
            Inputs.all,
            fun collection elements ->
                for count in [ 1; 3; 8; 9 ] do
                    Assert.AreEqual<int list list>(
                        List.splitInto count elements,
                        ops.SplitInto count collection,
                        $"%s{ops.Name}.splitInto %d{count} of %A{elements}"
                    )
        )

    [<TestMethod>]
    member this.``splitInto throws for a count that is not positive as FSharp.Core does`` () =
        this.ThrowsLikeFSharpCore (
            (fun () -> Array.splitInto 0 (List.toArray Inputs.several)),
            (fun () -> List.splitInto 0 Inputs.several),
            (fun () -> ops.SplitInto 0 (ops.OfSeq Inputs.several)),
            "splitInto 0 must throw"
        )

    [<TestMethod>]
    member this.``take returns the first elements`` () =
        this.ForEach (
            Inputs.all,
            fun collection elements ->
                for count in List.distinct [ 0; min 3 elements.Length; elements.Length ] do
                    Assert.AreEqual<int list>(
                        List.take count elements,
                        ops.ToList (ops.Take count collection),
                        $"%s{ops.Name}.take %d{count} of %A{elements}"
                    )
        )

    [<TestMethod>]
    member this.``take throws for a negative count or one beyond the length as FSharp.Core does`` () =
        for count in [ -1; 9 ] do
            this.ThrowsLikeFSharpCore (
                (fun () -> Array.take count (List.toArray Inputs.several)),
                (fun () -> List.take count Inputs.several),
                (fun () -> ops.Take count (ops.OfSeq Inputs.several)),
                $"take %d{count} must throw"
            )

    [<TestMethod>]
    member this.``takeLast returns the last elements`` () =
        this.ForEach (
            Inputs.all,
            fun collection elements ->
                for count in List.distinct [ 0; min 3 elements.Length; elements.Length ] do
                    Assert.AreEqual<int list>(
                        List.skip (elements.Length - count) elements,
                        ops.ToList (ops.TakeLast count collection),
                        $"%s{ops.Name}.takeLast %d{count} of %A{elements}"
                    )
        )

    [<TestMethod>]
    member this.``takeLast throws for a negative count or one beyond the length as take does`` () =
        for count in [ -1; 9 ] do
            this.ThrowsLikeFSharpCore (
                (fun () -> Array.take count (List.toArray Inputs.several)),
                (fun () -> List.take count Inputs.several),
                (fun () -> ops.TakeLast count (ops.OfSeq Inputs.several)),
                $"takeLast %d{count} must throw"
            )

    [<TestMethod>]
    member this.``takeWhile returns the first elements that satisfy the predicate`` () =
        this.ForEach (
            Inputs.all,
            fun collection elements ->
                for limit in [ -10; 6; 100 ] do
                    Assert.AreEqual<int list>(
                        List.takeWhile (fun item -> item < limit) elements,
                        ops.ToList (ops.TakeWhile (fun item -> item < limit) collection),
                        $"%s{ops.Name}.takeWhile below %d{limit} of %A{elements}"
                    )
        )

    [<TestMethod>]
    member this.``truncate returns at most the given number of first elements`` () =
        this.ForEach (
            Inputs.all,
            fun collection elements ->
                for count in countsFor elements.Length @ [ elements.Length + 5 ] do
                    Assert.AreEqual<int list>(
                        List.truncate count elements,
                        ops.ToList (ops.Truncate count collection),
                        $"%s{ops.Name}.truncate %d{count} of %A{elements}"
                    )
        )
