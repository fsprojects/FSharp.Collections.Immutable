/// <summary>
/// Algorithms that build sorted sets one element at a time, for the creation members of <c>CustomComparerSortedSet</c>
/// and the transformations of the <c>SortedSet</c> module that change the element type (ADR 0001, D5).
/// </summary>
/// <remarks>
/// <para>
/// A builder keeps the first of the elements that compare equal, as the FSharp.Core <c>Set</c> module does.
/// <c>ImmutableSortedSet.CreateRange</c> sorts the elements with an unstable sort before it drops the duplicates, so it can
/// keep any of them.
/// </para>
/// <para>
/// The transformations that change the element type compare the new elements with F# generic comparison,
/// <c>DefaultComparer</c> of the <c>Comparers</c> module, the comparer of the sorted sets created without one (D5,
/// Comparers): the comparer of the input cannot compare elements of another type.
/// </para>
/// </remarks>
module internal FSharp.Collections.Immutable.SortedSets

open System.Collections.Generic
open System.Collections.Immutable

/// <summary>
/// The distinct elements of a sequence in a sorted set with the comparer, the first of the elements that compare equal.
/// </summary>
/// <remarks>
/// The caller checks the input: <c>checkNotNullOrDefaultSeq</c> for a sequence, which also rejects a default flat list
/// given as a sequence, and <c>checkNotNull</c> for an array or a list. The exception then names the parameter of the
/// caller and not the parameter of <c>UnionWith</c> (ADR 0001, D6).
/// </remarks>
let ofSeq (comparer : IComparer<'T>) (source : seq<'T>) : ImmutableSortedSet<'T> =
    let builder = ImmutableSortedSet.CreateBuilder<'T> comparer
    builder.UnionWith source
    builder.ToImmutable ()

/// The values that the chooser returns for the elements.
let inline choose ([<InlineIfLambda>] chooser : 'T -> 'U voption) (source : ImmutableSortedSet<'T>) : ImmutableSortedSet<'U> =
    let builder = ImmutableSortedSet.CreateBuilder<'U> DefaultComparer<'U>.Instance

    for item in source do
        match chooser item with
        | ValueSome value -> builder.Add value |> ignore
        | ValueNone -> ()

    builder.ToImmutable ()

/// The results of the mapping of the elements.
let inline map ([<InlineIfLambda>] mapping : 'T -> 'U) (source : ImmutableSortedSet<'T>) : ImmutableSortedSet<'U> =
    let builder = ImmutableSortedSet.CreateBuilder<'U> DefaultComparer<'U>.Instance

    for item in source do
        builder.Add (mapping item) |> ignore

    builder.ToImmutable ()

/// <summary>
/// The values of the first choices and the values of the second choices that the partitioner returns for the elements,
/// which it visits in descending order.
/// </summary>
/// <remarks>
/// <c>Set.partitionWith</c> visits the elements from the largest one and keeps the first of the values that compare
/// equal, and so does a builder: the same one of two equal values stays.
/// </remarks>
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
