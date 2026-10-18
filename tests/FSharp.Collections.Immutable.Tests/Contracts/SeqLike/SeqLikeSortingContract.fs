namespace FSharp.Collections.Immutable.Tests

open System.Collections.Generic
open FSharp.Collections.Immutable
open Microsoft.VisualStudio.TestTools.UnitTesting

/// <summary>The contract of the sorting functions of a sequence-like module.</summary>
/// <remarks>
/// Equal keys belong to equal elements in these inputs, so the results do not depend on the stability of the sort;
/// <c>StableSortContract</c> checks the stability of the modules that promise it.
/// </remarks>
[<AbstractClass>]
type SeqLikeSortingContract<'C> (ops : SeqOps<'C>) =
    inherit SeqLikePartitioningContract<'C> (ops)

    [<TestMethod>]
    member this.``sort orders the elements ascending`` () =
        this.ForEach (
            Inputs.all,
            fun collection elements ->
                Assert.AreEqual<int list>(
                    List.sort elements,
                    ops.ToList (ops.Sort collection),
                    $"%s{ops.Name}.sort of %A{elements}"
                )
        )

    [<TestMethod>]
    member this.``sortBy orders the elements by the projection`` () =
        this.ForEach (
            Inputs.all,
            fun collection elements ->
                Assert.AreEqual<int list>(
                    List.sortBy abs elements,
                    ops.ToList (ops.SortBy abs collection),
                    $"%s{ops.Name}.sortBy of %A{elements}"
                )
        )

    [<TestMethod>]
    member this.``sortByDescending orders the elements by the projection descending`` () =
        this.ForEach (
            Inputs.all,
            fun collection elements ->
                Assert.AreEqual<int list>(
                    List.sortByDescending abs elements,
                    ops.ToList (ops.SortByDescending abs collection),
                    $"%s{ops.Name}.sortByDescending of %A{elements}"
                )
        )

    [<TestMethod>]
    member this.``sortDescending orders the elements descending`` () =
        this.ForEach (
            Inputs.all,
            fun collection elements ->
                Assert.AreEqual<int list>(
                    List.sortDescending elements,
                    ops.ToList (ops.SortDescending collection),
                    $"%s{ops.Name}.sortDescending of %A{elements}"
                )
        )

    [<TestMethod>]
    member this.``sortWith orders the elements with the comparison function`` () =
        let descending (left : int) (right : int) = compare right left

        this.ForEach (
            Inputs.all,
            fun collection elements ->
                Assert.AreEqual<int list>(
                    List.sortWith descending elements,
                    ops.ToList (ops.SortWith descending collection),
                    $"%s{ops.Name}.sortWith of %A{elements}"
                )
        )

    [<TestMethod>]
    member this.``sortWithComparer orders the elements with the comparer`` () =
        let descending = Comparer<int>.Create (fun left right -> compare right left)

        this.ForEach (
            Inputs.all,
            fun collection elements ->
                Assert.AreEqual<int list>(
                    List.sortDescending elements,
                    ops.ToList (ops.SortWithComparer descending collection),
                    $"%s{ops.Name}.sortWithComparer of %A{elements}"
                )
        )
