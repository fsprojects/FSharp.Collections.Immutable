/// <summary>
/// The functions of <see cref="T:FSharp.Collections.Immutable.SortedSet"/> that return tuples, with reference
/// tuples instead of struct tuples.
/// </summary>
/// <remarks>
/// After <c>open FSharp.Collections.Immutable.Tuple</c>, these functions shadow the default ones, and the other functions
/// of the default module stay available (ADR 0001, D3). Each function has the signature of the function with the same
/// name in <see cref="T:Microsoft.FSharp.Collections.SetModule"/>.
/// </remarks>
[<RequireQualifiedAccess>]
module FSharp.Collections.Immutable.Tuple.SortedSet

open FSharp.Collections.Immutable

// ---- Partitioning ----

/// <summary>
/// Splits the elements by a predicate.
/// </summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="set">The input sorted set.</param>
/// <returns>
/// The sorted set of the elements that satisfy the predicate and the sorted set of the other ones, both with the comparer
/// of the input.
/// </returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n log n). The function tests the elements in ascending order.
/// </remarks>
[<CompiledName "Partition">]
val partition : predicate : ('T -> bool) -> set : SortedSet<'T> -> SortedSet<'T> * SortedSet<'T>

/// <summary>
/// Splits the elements by the choice that a function returns for them.
/// </summary>
/// <param name="partitioner">The function that returns <c>Choice1Of2</c> or <c>Choice2Of2</c> for an element.</param>
/// <param name="set">The input sorted set.</param>
/// <returns>
/// The sorted set of the values of the first choices and the sorted set of the values of the second choices, which
/// compare with F# generic comparison.
/// </returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n log n). The function calls the partitioner in descending order of the elements and keeps the first of the values
/// that compare equal, as <see cref="M:Microsoft.FSharp.Collections.SetModule.PartitionWith``3(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpChoice{``1,``2}},Microsoft.FSharp.Collections.FSharpSet{``0})"/>
/// does, so that the same one of two equal values stays.
/// </remarks>
[<CompiledName "PartitionWith">]
val partitionWith :
    partitioner : ('T -> Choice<'T1, 'T2>) -> set : SortedSet<'T> -> SortedSet<'T1> * SortedSet<'T2>
        when 'T1 : comparison and 'T2 : comparison
