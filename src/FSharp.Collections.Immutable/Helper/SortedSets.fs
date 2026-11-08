/// <summary>
/// Algorithms that build sorted sets one element at a time, for the creation members of
/// <see cref="T:FSharp.Collections.Immutable.CustomComparerSortedSet`1"/> and the transformations of the
/// <see cref="T:FSharp.Collections.Immutable.SortedSet"/> module that change the element type (ADR 0001, D5).
/// </summary>
/// <remarks>
/// <para>
/// A builder keeps the first of the elements that compare equal, as the FSharp.Core
/// <see cref="T:Microsoft.FSharp.Collections.SetModule"/> module does.
/// <see cref="M:System.Collections.Immutable.ImmutableSortedSet.CreateRange``1(System.Collections.Generic.IComparer{``0},System.Collections.Generic.IEnumerable{``0})"/>
/// sorts the elements with an unstable sort before it drops the duplicates, so it can keep any of them.
/// </para>
/// <para>
/// The transformations that change the element type compare the new elements with F# generic comparison,
/// <see cref="T:FSharp.Collections.Immutable.Comparers.DefaultComparer`1"/>, the comparer of the sorted sets created without
/// one (D5, Comparers): the comparer of the input cannot compare elements of another type.
/// </para>
/// <para>
/// The summary of each function is the summary of the function of
/// <see cref="T:Microsoft.FSharp.Collections.SetModule"/> word for word (FSharp.Core 10.1), and so are the descriptions of
/// the parameters and of the result where the signatures agree. The remarks say what differs and why the function exists.
/// </para>
/// </remarks>
module internal FSharp.Collections.Immutable.SortedSets

open System.Collections.Generic
open System.Collections.Immutable

/// <summary>
/// Builds a new collection from the given enumerable object.
/// </summary>
/// <param name="comparer">The comparer of the elements, which the sorted set keeps.</param>
/// <param name="source">The input sequence.</param>
/// <returns>The sorted set containing the distinct elements, the first of the elements that compare equal.</returns>
/// <remarks>
/// <para>
/// This is an O(n log n) operation, where n is the number of elements in the sequence.
/// </para>
/// <para>
/// The caller checks the input:
/// <see cref="M:FSharp.Collections.Immutable.ImmutableCollectionUtil.checkNotNullOrDefaultSeq``1(System.String,System.Collections.Generic.IEnumerable{``0})"/>
/// for a sequence, which also rejects a
/// default flat list given as a sequence, and
/// <see cref="M:FSharp.Collections.Immutable.ImmutableCollectionUtil.checkNotNull``1(System.String,``0)"/>
/// for an array or a list. The
/// exception then names the parameter of the caller and not the parameter of
/// <see cref="M:System.Collections.Immutable.ImmutableSortedSet`1.Builder.UnionWith(System.Collections.Generic.IEnumerable{`0})"/>
/// (ADR 0001, D6).
/// </para>
/// <para>
/// It exists because
/// <see cref="M:Microsoft.FSharp.Collections.SetModule.OfSeq``1(System.Collections.Generic.IEnumerable{``0})"/>
/// builds an F# set, and because
/// <see cref="M:System.Collections.Immutable.ImmutableSortedSet.CreateRange``1(System.Collections.Generic.IComparer{``0},System.Collections.Generic.IEnumerable{``0})"/>
/// can keep any of the elements that compare equal (see the remarks of the module). The function takes the comparer of the
/// sorted set, so that the creation members of the comparer struct share it.
/// </para>
/// </remarks>
let ofSeq (comparer : IComparer<'T>) (source : seq<'T>) : ImmutableSortedSet<'T> =
    let builder = ImmutableSortedSet.CreateBuilder<'T> comparer
    builder.UnionWith source
    builder.ToImmutable ()

/// <summary>
/// Returns a new collection containing the values that the given function returns for the elements of the input set, for
/// the elements for which it returns a value.
/// </summary>
/// <param name="chooser">The function to transform elements of the input set into value options.</param>
/// <param name="source">The elements of the input set.</param>
/// <returns>A sorted set containing the chosen values.</returns>
/// <remarks>
/// <para>
/// This is an O(n log n) operation, where n is the number of elements in the set.
/// </para>
/// <para>
/// FSharp.Core has no such function for sets, so there is no summary to repeat. The result is ordered by F# generic
/// comparison of the new element type (see the remarks of the module).
/// </para>
/// </remarks>
/// <seealso cref="M:FSharp.Collections.Immutable.SortedSet.Choose``2(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpValueOption{``1}},System.Collections.Immutable.ImmutableSortedSet{``0})"/>
let inline choose ([<InlineIfLambda>] chooser : 'T -> 'U voption) (source : ImmutableSortedSet<'T>) : ImmutableSortedSet<'U> =
    let builder = ImmutableSortedSet.CreateBuilder<'U> DefaultComparer<'U>.Instance

    for item in source do
        match chooser item with
        | ValueSome value -> builder.Add value |> ignore
        | ValueNone -> ()

    builder.ToImmutable ()

/// <summary>
/// Returns a new collection containing the results of applying the given function to each element of the input set.
/// </summary>
/// <param name="mapping">The function to transform elements of the input set.</param>
/// <param name="source">The elements of the input set.</param>
/// <returns>A sorted set containing the transformed elements.</returns>
/// <remarks>
/// <para>
/// This is an O(n log n) operation, where n is the number of elements in the set.
/// </para>
/// <para>
/// It exists because
/// <see cref="M:Microsoft.FSharp.Collections.SetModule.Map``2(Microsoft.FSharp.Core.FSharpFunc{``0,``1},Microsoft.FSharp.Collections.FSharpSet{``0})"/>
/// takes an F# set and returns one. The result here is a sorted set of the BCL, ordered by F#
/// generic comparison of the new element type (see the remarks of the module).
/// </para>
/// </remarks>
/// <seealso cref="M:FSharp.Collections.Immutable.SortedSet.Map``2(Microsoft.FSharp.Core.FSharpFunc{``0,``1},System.Collections.Immutable.ImmutableSortedSet{``0})"/>
let inline map ([<InlineIfLambda>] mapping : 'T -> 'U) (source : ImmutableSortedSet<'T>) : ImmutableSortedSet<'U> =
    let builder = ImmutableSortedSet.CreateBuilder<'U> DefaultComparer<'U>.Instance

    for item in source do
        builder.Add (mapping item) |> ignore

    builder.ToImmutable ()

/// <summary>
/// Splits the set into two sets, by applying the given partitioning function to each element. Returns <c>Choice1Of2</c>
/// elements in the first set and <c>Choice2Of2</c> elements in the second set.
/// </summary>
/// <param name="partitioner">The function to transform and classify each input element into one of two output types.</param>
/// <param name="source">The elements of the input set.</param>
/// <returns>
/// A struct tuple of two sorted sets. The first containing values from <c>Choice1Of2</c> results and the second containing
/// values from <c>Choice2Of2</c> results.
/// </returns>
/// <remarks>
/// <para>
/// This is an O(n log n) operation, where n is the number of elements in the set. The function visits the elements in
/// descending order.
/// </para>
/// <para>
/// <see cref="M:Microsoft.FSharp.Collections.SetModule.PartitionWith``3(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpChoice{``1,``2}},Microsoft.FSharp.Collections.FSharpSet{``0})"/>
/// visits the elements from the largest one and keeps the first of the values that compare equal, and so
/// does a builder: the same one of two equal values stays. It exists because
/// <see cref="M:Microsoft.FSharp.Collections.SetModule.PartitionWith``3(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpChoice{``1,``2}},Microsoft.FSharp.Collections.FSharpSet{``0})"/>
/// takes an F# set and returns
/// a tuple of F# sets, where the default functions return struct tuples (ADR 0001, D3).
/// </para>
/// </remarks>
/// <seealso cref="M:FSharp.Collections.Immutable.SortedSet.PartitionWith``3(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpChoice{``1,``2}},System.Collections.Immutable.ImmutableSortedSet{``0})"/>
let inline partitionWith
    ([<InlineIfLambda>] partitioner : 'T -> Choice<'T1, 'T2>)
    (source : ImmutableSortedSet<'T>)
    : struct (ImmutableSortedSet<'T1> * ImmutableSortedSet<'T2>) =
    let first = ImmutableSortedSet.CreateBuilder<'T1> DefaultComparer<'T1>.Instance
    let second = ImmutableSortedSet.CreateBuilder<'T2> DefaultComparer<'T2>.Instance

    // Reverse walks the tree from the largest element without a copy
    for item in source.Reverse () do
        match partitioner item with
        | Choice1Of2 value -> first.Add value |> ignore
        | Choice2Of2 value -> second.Add value |> ignore

    struct (first.ToImmutable (), second.ToImmutable ())
