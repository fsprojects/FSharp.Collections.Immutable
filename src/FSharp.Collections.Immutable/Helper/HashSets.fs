/// <summary>
/// Algorithms that build hash sets with the default comparer of their element type (ADR 0001, D5, Comparers).
/// </summary>
/// <remarks>
/// The transformations of the <c>HashSet</c> and <c>ISet</c> modules that change the element type share them: the
/// comparer of the input cannot compare elements of another type.
/// </remarks>
module internal FSharp.Collections.Immutable.HashSets

open System.Collections.Immutable

/// The values that the chooser returns for the elements.
let inline choose ([<InlineIfLambda>] chooser : 'T -> 'U voption) (source : seq<'T>) : ImmutableHashSet<'U> =
    let builder = ImmutableHashSet.CreateBuilder<'U>()

    for item in source do
        match chooser item with
        | ValueSome value -> builder.Add value |> ignore
        | ValueNone -> ()

    builder.ToImmutable ()

/// The results of the mapping of the elements.
let inline map ([<InlineIfLambda>] mapping : 'T -> 'U) (source : seq<'T>) : ImmutableHashSet<'U> =
    let builder = ImmutableHashSet.CreateBuilder<'U>()

    for item in source do
        builder.Add (mapping item) |> ignore

    builder.ToImmutable ()

/// The values of the first choices and the values of the second choices that the partitioner returns for the elements.
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
