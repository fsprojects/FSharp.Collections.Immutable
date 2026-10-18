/// <summary>
/// The functions of <see cref="T:FSharp.Collections.Immutable.ISet"/> that return tuples, with reference
/// tuples instead of struct tuples.
/// </summary>
/// <remarks>
/// After <c>open FSharp.Collections.Immutable.Tuple</c>, these functions shadow the default ones, and the other functions
/// of the default module stay available (ADR 0001, D3).
/// </remarks>
[<RequireQualifiedAccess>]
module FSharp.Collections.Immutable.Tuple.ISet

open System.Collections.Immutable
open FSharp.Collections.Immutable

// ---- Partitioning ----

/// <summary>
/// Splits the elements by a predicate.
/// </summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="set">The input set.</param>
/// <returns>
/// The set of the elements that satisfy the predicate and the set of the other ones, both with the implementation and
/// the comparer of the input.
/// </returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>
/// The complexity depends on the implementation: O(n log n) for
/// <see cref="T:System.Collections.Immutable.ImmutableHashSet`1"/> and <see cref="T:System.Collections.Immutable.ImmutableSortedSet`1"/>.
/// </remarks>
[<CompiledName "Partition">]
val partition : predicate : ('T -> bool) -> set : IImmutableSet<'T> -> IImmutableSet<'T> * IImmutableSet<'T>

/// <summary>
/// Splits the elements by the choice that a function returns for them.
/// </summary>
/// <param name="partitioner">The function that returns <c>Choice1Of2</c> or <c>Choice2Of2</c> for an element.</param>
/// <param name="set">The input set.</param>
/// <returns>
/// The hash set of the values of the first choices and the hash set of the values of the second choices, which compare
/// with <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>.
/// </returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n log n).
/// </remarks>
[<CompiledName "PartitionWith">]
val partitionWith : partitioner : ('T -> Choice<'T1, 'T2>) -> set : IImmutableSet<'T> -> HashSet<'T1> * HashSet<'T2>
