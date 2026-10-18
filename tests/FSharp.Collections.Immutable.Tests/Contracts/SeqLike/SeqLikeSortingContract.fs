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

    [<TestMethod; DynamicData(nameof Inputs.All, typeof<Inputs>)>]
    member this.``sort orders the elements ascending`` (input : int array) =
        let struct (collection, elements) = this.Create input

        CollectionAssert.AreEqual (
            Array.sort elements,
            ops.ToArray (ops.Sort collection),
            $"%s{ops.Name}.sort must order the elements ascending"
        )

    [<TestMethod; DynamicData(nameof Inputs.All, typeof<Inputs>)>]
    member this.``sortBy orders the elements by the projection`` (input : int array) =
        let struct (collection, elements) = this.Create input

        CollectionAssert.AreEqual (
            Array.sortBy abs elements,
            ops.ToArray (ops.SortBy abs collection),
            $"%s{ops.Name}.sortBy must order the elements by the projection"
        )

    [<TestMethod; DynamicData(nameof Inputs.All, typeof<Inputs>)>]
    member this.``sortByDescending orders the elements by the projection descending`` (input : int array) =
        let struct (collection, elements) = this.Create input

        CollectionAssert.AreEqual (
            Array.sortByDescending abs elements,
            ops.ToArray (ops.SortByDescending abs collection),
            $"%s{ops.Name}.sortByDescending must order the elements by the projection descending"
        )

    [<TestMethod; DynamicData(nameof Inputs.All, typeof<Inputs>)>]
    member this.``sortDescending orders the elements descending`` (input : int array) =
        let struct (collection, elements) = this.Create input

        CollectionAssert.AreEqual (
            Array.sortDescending elements,
            ops.ToArray (ops.SortDescending collection),
            $"%s{ops.Name}.sortDescending must order the elements descending"
        )

    [<TestMethod; DynamicData(nameof Inputs.All, typeof<Inputs>)>]
    member this.``sortWith orders the elements with the comparison function`` (input : int array) =
        let struct (collection, elements) = this.Create input
        let descending (left : int) (right : int) = compare right left

        CollectionAssert.AreEqual (
            Array.sortWith descending elements,
            ops.ToArray (ops.SortWith descending collection),
            $"%s{ops.Name}.sortWith must order the elements with the comparison function"
        )

    [<TestMethod; DynamicData(nameof Inputs.All, typeof<Inputs>)>]
    member this.``sortWithComparer orders the elements with the comparer`` (input : int array) =
        let struct (collection, elements) = this.Create input
        let descending = Comparer<int>.Create (fun left right -> compare right left)

        CollectionAssert.AreEqual (
            Array.sortDescending elements,
            ops.ToArray (ops.SortWithComparer descending collection),
            $"%s{ops.Name}.sortWithComparer must order the elements with the comparer"
        )
