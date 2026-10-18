namespace FSharp.Collections.Immutable.Tests

open Microsoft.VisualStudio.TestTools.UnitTesting

/// <summary>
/// The functions of a module whose cost the tests of <see cref="T:FSharp.Collections.Immutable.Tests.CostContract`1"/>
/// bound, over integers.
/// </summary>
type CostOps<'C> = {
    /// The name of the module, for the messages of the assertions.
    Name : string
    /// Creates a collection that enumerates the given elements in their order.
    OfSeq : seq<int> -> 'C
    /// The greatest element.
    Max : 'C -> int
    /// The first element with the greatest result of the projection.
    MaxBy : (int -> int) -> 'C -> int
    /// The smallest element.
    Min : 'C -> int
    /// The first element with the smallest result of the projection.
    MinBy : (int -> int) -> 'C -> int
    /// Whether any pair of elements at the same index satisfies the predicate.
    Exists2 : (int -> int -> bool) -> 'C -> 'C -> bool
    /// Whether all pairs of elements at the same index satisfy the predicate.
    Forall2 : (int -> int -> bool) -> 'C -> 'C -> bool
    /// The comparison of two collections element by element with a comparer.
    CompareWith : (int -> int -> int) -> 'C -> 'C -> int
    /// The number of elements of the concatenation of the collections.
    ConcatLength : seq<'C> -> int
    /// The number of elements of the concatenation of the collections that a function returns for the elements.
    CollectLength : (int -> 'C) -> 'C -> int
}

/// <summary>
/// The bounds on what the functions of a module allocate for large collections: the functions that compare or search
/// must not box the elements, the ones that can decide before the end must not copy the collections, and the ones that
/// concatenate long indexed lists must share their nodes instead of copying their elements.
/// </summary>
/// <remarks>
/// Each limit is a tenth of what the implementation that it excludes allocates, or less. The tests create the
/// collections before they measure, and measure the second run of a function (see
/// <see cref="T:FSharp.Collections.Immutable.Tests.AllocationAssert"/>).
/// </remarks>
[<AbstractClass>]
type CostContract<'C> (ops : CostOps<'C>) =

    /// The integers of the comparisons. A comparison that boxes them allocates two boxes of 24 bytes for each of the
    /// 10 000, 480 KB; the function itself allocates a few hundred bytes.
    static let comparedIntegers = Array.init 10_000 (fun index -> (index * 7919) % 10_007)

    static let boxingLimit = 100_000L

    /// Whether the optimizer of the F# compiler is on. The inline functions are compiled at the call site, and the
    /// optimizer is what compiles the comparison of two integers into a comparison of integers: with the optimizer off, as
    /// in a Debug build, it is a call of the generic operator, which boxes them, as it is for the inline functions of
    /// FSharp.Core.
#if DEBUG
    static let optimized = false
#else
    static let optimized = true
#endif


    /// The length of the collections that a function reads a few elements of: a copy of two of them is 800 KB.
    static let walkedLength = 100_000

    static let copyLimit = 50_000L

    /// The collections that a concatenation joins: 100 of 1000 elements, for which a copy allocates 5.8 MB.
    static let joinedCount = 100

    static let joinedLength = 1_000

    static let joinLimit = 1_000_000L

    /// Asserts that a comparison of 10 000 integers boxes none of them; the test is inconclusive when the optimizer is off.
    member private _.AssertNoBoxes (action : unit -> unit, message : string) =
        if optimized then
            AllocationAssert.LessThan (boxingLimit, action, message)
        else
            Assert.Inconclusive "The comparison of integers boxes them while the optimizer of the compiler is off"

    [<TestMethod>]
    member this.``max allocates no box for an integer in a comparison`` () =
        let collection = ops.OfSeq comparedIntegers

        Assert.AreEqual<int>(Array.max comparedIntegers, ops.Max collection, $"%s{ops.Name}.max must return the greatest element")

        this.AssertNoBoxes (
            (fun () -> ops.Max collection |> ignore),
            $"%s{ops.Name}.max of 10 000 integers must not box them in the comparisons"
        )

    [<TestMethod>]
    member this.``maxBy allocates no box for a key in a comparison`` () =
        let collection = ops.OfSeq comparedIntegers

        Assert.AreEqual<int>(
            Array.maxBy (fun item -> -item) comparedIntegers,
            ops.MaxBy (fun item -> -item) collection,
            $"%s{ops.Name}.maxBy must return the first element with the greatest result"
        )

        this.AssertNoBoxes (
            (fun () -> ops.MaxBy (fun item -> -item) collection |> ignore),
            $"%s{ops.Name}.maxBy of 10 000 integers must not box the keys in the comparisons"
        )

    [<TestMethod>]
    member this.``min allocates no box for an integer in a comparison`` () =
        let collection = ops.OfSeq comparedIntegers

        Assert.AreEqual<int>(Array.min comparedIntegers, ops.Min collection, $"%s{ops.Name}.min must return the smallest element")

        this.AssertNoBoxes (
            (fun () -> ops.Min collection |> ignore),
            $"%s{ops.Name}.min of 10 000 integers must not box them in the comparisons"
        )

    [<TestMethod>]
    member this.``minBy allocates no box for a key in a comparison`` () =
        let collection = ops.OfSeq comparedIntegers

        Assert.AreEqual<int>(
            Array.minBy (fun item -> -item) comparedIntegers,
            ops.MinBy (fun item -> -item) collection,
            $"%s{ops.Name}.minBy must return the first element with the smallest result"
        )

        this.AssertNoBoxes (
            (fun () -> ops.MinBy (fun item -> -item) collection |> ignore),
            $"%s{ops.Name}.minBy of 10 000 integers must not box the keys in the comparisons"
        )

    [<TestMethod>]
    member _.``exists2 stops at the first pair that satisfies the predicate`` () =
        let first = ops.OfSeq (Array.init walkedLength id)
        let second = ops.OfSeq (Array.init walkedLength id)

        Assert.IsTrue (
            ops.Exists2 (fun left right -> left = right) first second,
            $"%s{ops.Name}.exists2 must find the first pair"
        )

        AllocationAssert.LessThan (
            copyLimit,
            (fun () ->
                ops.Exists2 (fun left right -> left = right) first second
                |> ignore
            ),
            $"%s{ops.Name}.exists2 must not copy the collections when the first pair satisfies the predicate"
        )

    [<TestMethod>]
    member _.``forall2 stops at the first pair that does not satisfy the predicate`` () =
        let first = ops.OfSeq (Array.init walkedLength id)
        let second = ops.OfSeq (Array.init walkedLength id)

        Assert.IsFalse (
            ops.Forall2 (fun left right -> left <> right) first second,
            $"%s{ops.Name}.forall2 must find that the first pair does not satisfy the predicate"
        )

        AllocationAssert.LessThan (
            copyLimit,
            (fun () ->
                ops.Forall2 (fun left right -> left <> right) first second
                |> ignore
            ),
            $"%s{ops.Name}.forall2 must not copy the collections when the first pair does not satisfy the predicate"
        )

    [<TestMethod>]
    member _.``compareWith stops at the first pair that the comparer finds different`` () =
        let first = ops.OfSeq (Array.init walkedLength (fun index -> index + 1))
        let second = ops.OfSeq (Array.init walkedLength id)

        Assert.AreEqual<int>(
            1,
            ops.CompareWith compare first second,
            $"%s{ops.Name}.compareWith must return the first difference"
        )

        AllocationAssert.LessThan (
            copyLimit,
            (fun () -> ops.CompareWith compare first second |> ignore),
            $"%s{ops.Name}.compareWith must not copy the collections when the first pair differs"
        )

    [<TestMethod>]
    member _.``concat joins long collections instead of copying their elements`` () =
        let collections = Array.init joinedCount (fun _ -> ops.OfSeq (Array.init joinedLength id))

        Assert.AreEqual<int>(
            joinedCount * joinedLength,
            ops.ConcatLength collections,
            $"%s{ops.Name}.concat must keep every element"
        )

        AllocationAssert.LessThan (
            joinLimit,
            (fun () -> ops.ConcatLength collections |> ignore),
            $"%s{ops.Name}.concat of 100 collections of 1000 elements must not copy their elements"
        )

    [<TestMethod>]
    member _.``collect joins the long collections that the function returns instead of copying their elements`` () =
        let collection = ops.OfSeq (Array.init joinedLength id)
        let source = ops.OfSeq (Array.init joinedCount id)

        Assert.AreEqual<int>(
            joinedCount * joinedLength,
            ops.CollectLength (fun _ -> collection) source,
            $"%s{ops.Name}.collect must keep every element"
        )

        AllocationAssert.LessThan (
            joinLimit,
            (fun () -> ops.CollectLength (fun _ -> collection) source |> ignore),
            $"%s{ops.Name}.collect of 100 collections of 1000 elements must not copy their elements"
        )
