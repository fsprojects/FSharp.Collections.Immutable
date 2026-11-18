/// <summary>
/// Algorithms that build hash sets with the default comparer of their element type (ADR 0001, D5, Comparers).
/// </summary>
/// <remarks>
/// <para>
/// The transformations of the
/// <see cref="T:FSharp.Collections.Immutable.HashSet"/> and
/// <see cref="T:FSharp.Collections.Immutable.ISet"/>
/// modules that change the element type share them: the
/// comparer of the input cannot compare elements of another type.
/// </para>
/// <para>
/// The functions of the FSharp.Core <see cref="T:Microsoft.FSharp.Collections.SetModule"/> module take an F# set and build
/// F# sets, which are ordered trees. The algorithms here build hash sets from the elements of any sequence, so one
/// implementation serves a hash set and a set seen through its interface. Of the results that the default comparer finds
/// equal, the first one stays, because the builder does not replace an element that it holds.
/// </para>
/// <para>
/// The summary of each function is the summary of the function of
/// <see cref="T:Microsoft.FSharp.Collections.SetModule"/> word for word (FSharp.Core 10.1), and so are the descriptions of
/// the parameters and of the result where the signatures agree. The remarks say what differs and why the function exists.
/// </para>
/// </remarks>
module internal FSharp.Collections.Immutable.HashSets

open System.Collections.Immutable

/// <summary>
/// Returns a new collection containing the values that the given function returns for the elements of the input set, for
/// the elements for which it returns a value.
/// </summary>
/// <param name="chooser">The function to transform elements of the input set into value options.</param>
/// <param name="source">The elements of the input set.</param>
/// <returns>A hash set containing the chosen values.</returns>
/// <remarks>
/// <para>
/// This is an O(n) operation, where n is the number of elements in the set.
/// </para>
/// <para>
/// FSharp.Core has no such function for sets, so there is no summary to repeat. The result has the default comparer of the
/// new element type.
/// </para>
/// </remarks>
/// <seealso cref="M:FSharp.Collections.Immutable.HashSet.Choose``2(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpValueOption{``1}},System.Collections.Immutable.ImmutableHashSet{``0})"/>
/// <seealso cref="M:FSharp.Collections.Immutable.ISet.Choose``2(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpValueOption{``1}},System.Collections.Immutable.IImmutableSet{``0})"/>
let inline choose ([<InlineIfLambda>] chooser : 'T -> 'U voption) (source : seq<'T>) : ImmutableHashSet<'U> =
    let builder = ImmutableHashSet.CreateBuilder<'U>()

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
/// <returns>A hash set containing the transformed elements.</returns>
/// <remarks>
/// <para>
/// This is an O(n) operation, where n is the number of elements in the set.
/// </para>
/// <para>
/// It exists because
/// <see cref="M:Microsoft.FSharp.Collections.SetModule.Map``2(Microsoft.FSharp.Core.FSharpFunc{``0,``1},Microsoft.FSharp.Collections.FSharpSet{``0})"/>
/// takes an F# set and returns an F# set, which is ordered by comparison. The result here is a
/// hash set with the default comparer of the new element type.
/// </para>
/// </remarks>
/// <seealso cref="M:FSharp.Collections.Immutable.HashSet.Map``2(Microsoft.FSharp.Core.FSharpFunc{``0,``1},System.Collections.Immutable.ImmutableHashSet{``0})"/>
/// <seealso cref="M:FSharp.Collections.Immutable.ISet.Map``2(Microsoft.FSharp.Core.FSharpFunc{``0,``1},System.Collections.Immutable.IImmutableSet{``0})"/>
let inline map ([<InlineIfLambda>] mapping : 'T -> 'U) (source : seq<'T>) : ImmutableHashSet<'U> =
    let builder = ImmutableHashSet.CreateBuilder<'U>()

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
/// A struct tuple of two hash sets. The first containing values from <c>Choice1Of2</c> results and the second containing
/// values from <c>Choice2Of2</c> results.
/// </returns>
/// <remarks>
/// <para>
/// This is an O(n) operation, where n is the number of elements in the set.
/// </para>
/// <para>
/// It exists because
/// <see cref="M:Microsoft.FSharp.Collections.SetModule.PartitionWith``3(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpChoice{``1,``2}},Microsoft.FSharp.Collections.FSharpSet{``0})"/>
/// takes an F# set and returns a tuple of F# sets. The default functions return struct
/// tuples (ADR 0001, D3), and the results are hash sets with the default comparers of their element types.
/// </para>
/// </remarks>
/// <seealso cref="M:FSharp.Collections.Immutable.HashSet.PartitionWith``3(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpChoice{``1,``2}},System.Collections.Immutable.ImmutableHashSet{``0})"/>
/// <seealso cref="M:FSharp.Collections.Immutable.ISet.PartitionWith``3(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpChoice{``1,``2}},System.Collections.Immutable.IImmutableSet{``0})"/>
let inline partitionWith
    ([<InlineIfLambda>] partitioner : 'T -> Choice<'T1, 'T2>)
    (source : seq<'T>)
    : struct (ImmutableHashSet<'T1> * ImmutableHashSet<'T2>) =
    let first = ImmutableHashSet.CreateBuilder<'T1>()
    let second = ImmutableHashSet.CreateBuilder<'T2>()

    for item in source do
        match partitioner item with
        | Choice1Of2 value -> first.Add value |> ignore
        | Choice2Of2 value -> second.Add value |> ignore

    struct (first.ToImmutable (), second.ToImmutable ())
