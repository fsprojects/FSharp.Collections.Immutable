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

    [<TestMethod; DynamicData(nameof Inputs.All, typeof<Inputs>)>]
    member _.``toArray returns the elements in order`` (input : int array) =
        // The other tests observe a collection with toArray, so toArray is checked against the sequence that the collection
        // was created from, which ofSeq keeps in its order
        CollectionAssert.AreEqual (
            input,
            ops.ToArray (ops.OfSeq input),
            $"%s{ops.Name}.toArray must return the elements in the order of the sequence that the collection was created from"
        )

    [<TestMethod>]
    member _.``toArray returns a new array`` () =
        let collection = ops.OfSeq Inputs.Several
        let array = ops.ToArray collection
        array[0] <- 1000

        CollectionAssert.AreEqual (
            Inputs.Several,
            ops.ToArray collection,
            $"%s{ops.Name}.toArray must return a copy that does not change the collection"
        )

    [<TestMethod; DynamicData(nameof Inputs.All, typeof<Inputs>)>]
    member this.``toList returns the elements in order`` (input : int array) =
        let struct (collection, elements) = this.Create input

        CollectionAssert.AreEqual (
            elements,
            List.toArray (ops.ToList collection),
            $"%s{ops.Name}.toList must return the elements in the order in which the collection enumerates them"
        )

    [<TestMethod; DynamicData(nameof Inputs.All, typeof<Inputs>)>]
    member this.``toSeq returns the elements in order`` (input : int array) =
        let struct (collection, elements) = this.Create input

        CollectionAssert.AreEqual (
            elements,
            Seq.toArray (ops.ToSeq collection),
            $"%s{ops.Name}.toSeq must return the elements in the order in which the collection enumerates them"
        )
