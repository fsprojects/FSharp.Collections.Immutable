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
