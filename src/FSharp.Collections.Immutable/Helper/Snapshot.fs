/// <summary>
/// The snapshots that the modules of the collections without an underlying array take before they apply an algorithm of
/// the FSharp.Core <see cref="T:Microsoft.FSharp.Collections.ArrayModule"/> module, of
/// <see cref="T:FSharp.Collections.Immutable.Arrays"/> or of <see cref="T:FSharp.Collections.Immutable.ListArrays"/>
/// (ADR 0001, D5).
/// </summary>
/// <remarks>
/// The algorithms work on arrays. A collection such as an <see cref="T:System.Collections.Immutable.ImmutableList`1"/>, or
/// a queue or a stack, has none, so the module copies its elements into a new array first, applies the algorithm and
/// builds the result from the array it gets.
/// </remarks>
module internal FSharp.Collections.Immutable.Snapshot

open System
open System.Linq

/// <summary>
/// The elements of a collection in a new array, in the order in which the collection enumerates them.
/// </summary>
/// <param name="name">The name of the parameter of the collection to report: the caller passes the name of its own.</param>
/// <param name="source">The collection.</param>
/// <returns>A new array that holds the elements, which the caller may change in place.</returns>
/// <exception cref="T:System.ArgumentNullException">
/// The collection is <see langword="null"/>. The parameter name is <paramref name="name"/>.
/// </exception>
/// <remarks>
/// <para>
/// This is an O(n) operation, where n is the number of elements. A collection that implements
/// <see cref="T:System.Collections.Generic.ICollection`1"/> copies its elements with
/// <see cref="M:System.Collections.Generic.ICollection`1.CopyTo(`0[],System.Int32)"/> in one pass, without the growing
/// buffers of a collection that only enumerates; an empty one gives the shared empty array, which has nothing to change.
/// </para>
/// <para>
/// It exists to check the collection and to copy it in one place. The null check reports the name of the parameter of the
/// caller, where
/// <see cref="M:Microsoft.FSharp.Collections.SeqModule.ToArray``1(System.Collections.Generic.IEnumerable{``0})"/> and
/// <see cref="M:Microsoft.FSharp.Collections.ArrayModule.OfSeq``1(System.Collections.Generic.IEnumerable{``0})"/>
/// report <c>"source"</c> (ADR 0001, D6). The copy goes through
/// <see cref="M:System.Linq.Enumerable.ToArray``1(System.Collections.Generic.IEnumerable{``0})"/>, because the priority of
/// ADR 0001, D5 puts System.Linq before FSharp.Core.
/// </para>
/// </remarks>
let ofSeq (name : string) (source : seq<'T>) : 'T array =
    checkNotNull name source
    Enumerable.ToArray source

/// <summary>
/// A sequence in a form from which the BCL builds the tree of an
/// <see cref="T:System.Collections.Immutable.ImmutableList`1"/> in one pass: the elements of an F# list in a new array,
/// and any other sequence as it is.
/// </summary>
/// <param name="source">The sequence, which the caller has checked for <see langword="null"/>.</param>
/// <returns>A new array of the elements of an F# list, or <paramref name="source"/> itself.</returns>
/// <remarks>
/// <para>
/// <see cref="M:System.Collections.Immutable.ImmutableList.CreateRange``1(System.Collections.Generic.IEnumerable{``0})"/>,
/// <see cref="M:System.Collections.Immutable.ImmutableList`1.AddRange(System.Collections.Generic.IEnumerable{`0})"/> and
/// <see cref="M:System.Collections.Immutable.ImmutableList`1.InsertRange(System.Int32,System.Collections.Generic.IEnumerable{`0})"/>,
/// and the same members of the builder, build a tree by indexing a sequence that implements
/// <see cref="T:System.Collections.Generic.IReadOnlyList`1"/>. An F# list implements it, and its indexer walks the list
/// from its head, so the build takes time that grows with the square of the length: 67 ms, 257 ms and 1.16 s for an
/// insertion of 10 000, 20 000 and 40 000 elements. The copy of the list in an array is one pass, and so is the build
/// from the array: 0.16 ms, 0.37 ms and 0.68 ms for the same insertions.
/// </para>
/// <para>
/// The interface is covariant, so the BCL also indexes a list of strings that is given as a sequence of objects. Such a
/// value is not a <see cref="T:Microsoft.FSharp.Collections.FSharpList`1"/> of <see cref="T:System.Object"/>: the type
/// test for a list of the element type, which is the cheap one and comes first, does not find it, and the type of the
/// value tells that it is a list.
/// </para>
/// <para>
/// Reading the type of the value takes some 300 processor cycles, a sixth of the insertion of ten elements into a list
/// of a hundred. An array, a <see cref="T:System.Collections.Generic.List`1"/> and the immutable collections implement
/// <see cref="T:System.Collections.ICollection"/>, which an F# list does not, so one more type test returns them at once.
/// </para>
/// </remarks>
let forTree (source : seq<'T>) : seq<'T> =
    match source with
    | :? ('T list) as list -> List.toArray list
    | :? System.Collections.ICollection -> source
    | _ ->
        let sourceType = source.GetType ()

        if
            sourceType.IsGenericType
            && Type.(=) (sourceType.GetGenericTypeDefinition (), typedefof<_ list>)
        then
            Enumerable.ToArray source
        else
            source
