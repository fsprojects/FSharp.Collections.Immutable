namespace FSharp.Collections.Immutable.Tests

open FSharp.Collections.Immutable
open Microsoft.VisualStudio.TestTools.UnitTesting

/// <summary>
/// The predicates of the filter tests of the set modules, as the cases of their rows.
/// </summary>
/// <remarks>
/// MSTest names a row after its values, and a function has no readable name, so a row holds a case and the test takes
/// the function of the case.
/// </remarks>
[<RequireQualifiedAccess>]
type SetFilterPredicate =
    /// The positive elements.
    | IsPositive
    /// The even elements.
    | IsEven
    /// No element.
    | Never

    /// Tells whether an element satisfies the predicate.
    member this.Matches (item : int) =
        match this with
        | SetFilterPredicate.IsPositive -> item > 0
        | SetFilterPredicate.IsEven -> item % 2 = 0
        | SetFilterPredicate.Never -> false

/// <summary>
/// The mappings of the map tests of the set modules, as the cases of their rows.
/// </summary>
/// <remarks>
/// MSTest names a row after its values, and a function has no readable name, so a row holds a case and the test takes
/// the function of the case.
/// </remarks>
[<RequireQualifiedAccess>]
type SetMapping =
    /// Ten times the element, which keeps distinct elements distinct.
    | TimesTen
    /// The remainder of the element in the division by 3, which merges elements.
    | RemainderOfThree
    /// Zero for every element, which merges all elements into one.
    | Zero

    /// Maps an element.
    member this.Apply (item : int) =
        match this with
        | SetMapping.TimesTen -> item * 10
        | SetMapping.RemainderOfThree -> item % 3
        | SetMapping.Zero -> 0

/// <summary>
/// The contract of the transformations that every set module provides: <c>HashSet</c>, <c>SortedSet</c>, <c>ISet</c> and
/// <c>FrozenSet</c> (ADR 0001, Appendix C). The set algebra takes its arguments in the order of FSharp.Core.
/// </summary>
[<AbstractClass>]
type SetTransformContract<'S> (ops : SetTransformOps<'S>) =
    inherit SetContractBase<'S> (ops.Name, ops.ParamName, ops.OfSeq, ops.ToArray)

    /// A chooser that keeps the positive elements and makes some of them collide.
    static let chooser (item : int) = if item > 0 then ValueSome (item % 5) else ValueNone

    /// The same chooser as `chooser`, for the FSharp.Core oracle.
    static let chooserOption (item : int) = if item > 0 then Some (item % 5) else None

    /// Every input with each predicate of filter, as rows.
    static member InputsAndFilterPredicates = seq {
        for input in SetInputs.All do
            for predicate in [| SetFilterPredicate.IsPositive; SetFilterPredicate.IsEven; SetFilterPredicate.Never |] do
                struct (input, predicate)
    }

    /// Every input with each mapping of map, as rows.
    static member InputsAndMappings = seq {
        for input in SetInputs.All do
            for mapping in [| SetMapping.TimesTen; SetMapping.RemainderOfThree; SetMapping.Zero |] do
                struct (input, mapping)
    }

    /// <summary>
    /// The sequences of inputs of the functions of many sets, as rows: one input, two overlapping inputs, three inputs,
    /// and the empty input after and before another one.
    /// </summary>
    /// <remarks>
    /// A row that is an array of arrays is wrapped in a <c>TestDataRow</c>, because MSTest spreads an array of a reference
    /// type into the parameters of the test.
    /// </remarks>
    static member InputSequences = seq {
        TestDataRow<int[][]> [| Inputs.Several |]
        TestDataRow<int[][]> [| Inputs.Several; Inputs.Other |]
        TestDataRow<int[][]> [| Inputs.Several; SetInputs.Subset; Inputs.Other |]
        TestDataRow<int[][]> [| Inputs.Other; Inputs.Empty |]
        TestDataRow<int[][]> [| Inputs.Empty; Inputs.Singleton |]
    }

    // ---- Transformation ----

    [<TestMethod; DynamicData(nameof SetInputs.All, typeof<SetInputs>)>]
    member this.``choose keeps the distinct chosen values`` (input : int array) =
        let struct (set, expected) = this.Create input

        CollectionAssert.AreEqual (
            expected
            |> Set.toArray
            |> Array.choose chooserOption
            |> Array.distinct
            |> Array.sort,
            Array.sort (ops.Choose chooser set),
            $"%s{ops.Name}.choose must keep the distinct chosen values"
        )

    [<TestMethod; DynamicData(nameof SetInputs.Pairs, typeof<SetInputs>)>]
    member this.``difference removes the elements of the second set from the first one`` (first : int array, second : int array) =
        let struct (set1, expected1) = this.Create first
        let struct (set2, expected2) = this.Create second

        CollectionAssert.AreEqual (
            Set.toArray (Set.difference expected1 expected2),
            this.Sorted (ops.Difference set1 set2),
            $"%s{ops.Name}.difference must remove the elements of the second set from the first one"
        )

    [<TestMethod; DynamicData(nameof SetTransformContract<'S>.InputsAndFilterPredicates)>]
    member this.``filter keeps the elements that satisfy the predicate`` (input : int array, predicate : SetFilterPredicate) =
        let struct (set, expected) = this.Create input

        CollectionAssert.AreEqual (
            Set.toArray (Set.filter predicate.Matches expected),
            this.Sorted (ops.Filter predicate.Matches set),
            $"%s{ops.Name}.filter must keep the elements that satisfy the predicate"
        )

    [<TestMethod; DynamicData(nameof SetInputs.Pairs, typeof<SetInputs>)>]
    member this.``intersect keeps the elements that are in both sets`` (first : int array, second : int array) =
        let struct (set1, expected1) = this.Create first
        let struct (set2, expected2) = this.Create second

        CollectionAssert.AreEqual (
            Set.toArray (Set.intersect expected1 expected2),
            this.Sorted (ops.Intersect set1 set2),
            $"%s{ops.Name}.intersect must keep the elements that are in both sets"
        )

    [<TestMethod; DynamicData(nameof SetTransformContract<'S>.InputSequences)>]
    member this.``intersectMany keeps the elements that are in every set`` (inputs : int[][]) =
        let sets = inputs |> Array.map (fun input -> ops.OfSeq input)

        CollectionAssert.AreEqual (
            Set.toArray (Set.intersectMany (Array.map Set.ofArray inputs)),
            this.Sorted (ops.IntersectMany sets),
            $"%s{ops.Name}.intersectMany must keep the elements that are in every set"
        )

    [<TestMethod>]
    member this.``intersectMany throws for an empty sequence as FSharp.Core does`` () =
        this.ThrowsLikeSet (
            (fun () -> Set.intersectMany Seq.empty<Set<int>>),
            (fun () -> ops.IntersectMany Seq.empty),
            "intersectMany of no sets must throw"
        )

    [<TestMethod>]
    member this.``intersectMany throws for a null sequence as FSharp.Core does`` () =
        this.ThrowsLikeSet (
            (fun () -> Set.intersectMany (Unchecked.defaultof<seq<Set<int>>>)),
            (fun () -> ops.IntersectMany (Unchecked.defaultof<seq<'S>>)),
            "intersectMany of a null sequence must throw"
        )

    [<TestMethod; DynamicData(nameof SetTransformContract<'S>.InputsAndMappings)>]
    member this.``map keeps the distinct results`` (input : int array, mapping : SetMapping) =
        let struct (set, expected) = this.Create input

        CollectionAssert.AreEqual (
            Set.toArray (Set.map mapping.Apply expected),
            Array.sort (ops.Map mapping.Apply set),
            $"%s{ops.Name}.map must keep the distinct results"
        )

    [<TestMethod; DynamicData(nameof SetInputs.Pairs, typeof<SetInputs>)>]
    member this.``symmetricDifference keeps the elements that are in exactly one of the sets``
        (first : int array, second : int array)
        =
        let struct (set1, expected1) = this.Create first
        let struct (set2, expected2) = this.Create second

        CollectionAssert.AreEqual (
            Set.toArray (Set.union (Set.difference expected1 expected2) (Set.difference expected2 expected1)),
            this.Sorted (ops.SymmetricDifference set1 set2),
            $"%s{ops.Name}.symmetricDifference must keep the elements that are in exactly one of the sets"
        )

    [<TestMethod; DynamicData(nameof SetInputs.Pairs, typeof<SetInputs>)>]
    member this.``union keeps the elements that are in either set`` (first : int array, second : int array) =
        let struct (set1, expected1) = this.Create first
        let struct (set2, expected2) = this.Create second

        CollectionAssert.AreEqual (
            Set.toArray (Set.union expected1 expected2),
            this.Sorted (ops.Union set1 set2),
            $"%s{ops.Name}.union must keep the elements that are in either set"
        )

    [<TestMethod; DynamicData(nameof SetTransformContract<'S>.InputSequences)>]
    member this.``unionMany keeps the elements that are in any set`` (inputs : int[][]) =
        let sets = inputs |> Array.map (fun input -> ops.OfSeq input)

        CollectionAssert.AreEqual (
            Set.toArray (Set.unionMany (Array.map Set.ofArray inputs)),
            this.Sorted (ops.UnionMany sets),
            $"%s{ops.Name}.unionMany must keep the elements that are in any set"
        )

    [<TestMethod>]
    member this.``unionMany of no sets is empty`` () =
        Assert.IsEmpty (this.Sorted (ops.UnionMany Seq.empty), $"%s{ops.Name}.unionMany of no sets must be empty")

    [<TestMethod>]
    member this.``unionMany throws for a null sequence as FSharp.Core does`` () =
        this.ThrowsLikeSet (
            (fun () -> Set.unionMany (Unchecked.defaultof<seq<Set<int>>>)),
            (fun () -> ops.UnionMany (Unchecked.defaultof<seq<'S>>)),
            "unionMany of a null sequence must throw"
        )

    // ---- Partitioning ----

    [<TestMethod; DynamicData(nameof SetInputs.All, typeof<SetInputs>)>]
    member this.``partition splits the elements by the predicate`` (input : int array) =
        let struct (set, expected) = this.Create input
        let expectedAccepted, expectedRejected = Set.partition (fun item -> item > 0) expected
        let struct (accepted, rejected) = ops.Partition (fun item -> item > 0) set

        CollectionAssert.AreEqual (
            Set.toArray expectedAccepted,
            this.Sorted accepted,
            $"%s{ops.Name}.partition must return the elements that satisfy the predicate first"
        )

        CollectionAssert.AreEqual (
            Set.toArray expectedRejected,
            this.Sorted rejected,
            $"%s{ops.Name}.partition must return the other elements second"
        )

    [<TestMethod; DynamicData(nameof SetInputs.All, typeof<SetInputs>)>]
    member this.``partitionWith splits the elements by the choice`` (input : int array) =
        let struct (set, expected) = this.Create input

        let partitioner (item : int) =
            if item > 0 then
                Choice1Of2 (item % 5)
            else
                Choice2Of2 (string (abs item % 2))

        let expectedFirst, expectedSecond = Set.partitionWith partitioner expected
        let struct (first, second) = ops.PartitionWith partitioner set

        CollectionAssert.AreEqual (
            Set.toArray expectedFirst,
            Array.sort first,
            $"%s{ops.Name}.partitionWith must return the values of the first choices first"
        )

        CollectionAssert.AreEqual (
            Set.toArray expectedSecond,
            Array.sort second,
            $"%s{ops.Name}.partitionWith must return the values of the second choices second"
        )
