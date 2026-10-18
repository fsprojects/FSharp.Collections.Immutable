namespace FSharp.Collections.Immutable.Tests

open FSharp.Collections.Immutable
open Microsoft.VisualStudio.TestTools.UnitTesting

/// <summary>
/// The contract of the functions that every sequence-like module provides: <c>FlatList</c>, <c>IndexedList</c>,
/// <c>IIndexedList</c>, <c>Queue</c>, <c>IQueue</c>, <c>Stack</c> and <c>IStack</c> (ADR 0001, Appendix A).
/// </summary>
/// <remarks>
/// The facet is a chain of abstract classes, one file per category; a collection derives one test class from this last one.
/// </remarks>
[<AbstractClass>]
type SeqLikeContract<'C> (ops : SeqOps<'C>) =
    inherit SeqLikeRandomContract<'C> (ops)

    [<TestMethod>]
    member this.``toArray returns the elements in order`` () =
        this.ForEach (
            Inputs.all,
            fun collection elements ->
                CollectionAssert.AreEqual (List.toArray elements, ops.ToArray collection, $"%s{ops.Name}.toArray of %A{elements}")
        )

    [<TestMethod>]
    member _.``toArray returns a new array`` () =
        let collection = ops.OfSeq Inputs.several
        let array = ops.ToArray collection
        array[0] <- 1000

        Assert.AreEqual<int list>(
            Inputs.several,
            ops.ToList collection,
            $"%s{ops.Name}.toArray must return a copy that does not change the collection"
        )

    [<TestMethod>]
    member this.``toSeq returns the elements in order`` () =
        this.ForEach (
            Inputs.all,
            fun collection elements ->
                Assert.AreEqual<int list>(elements, List.ofSeq (ops.ToSeq collection), $"%s{ops.Name}.toSeq of %A{elements}")
        )
