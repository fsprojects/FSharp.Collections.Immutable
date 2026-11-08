/// <summary>
/// The functions of <see cref="T:FSharp.Collections.Immutable.FrozenSet"/> that return tuples, with reference tuples
/// instead of struct tuples.
/// </summary>
/// <remarks>
/// After <c>open FSharp.Collections.Immutable.Tuple</c>, these functions shadow the default ones, and the other functions
/// of the default module stay available (ADR 0001, D3). Each function has the signature of the function with the same
/// name in <see cref="T:Microsoft.FSharp.Collections.SetModule"/>.
/// </remarks>
[<RequireQualifiedAccess>]
module FSharp.Collections.Immutable.Tuple.FrozenSet

open System.Collections.Frozen

// ---- Partitioning ----

/// <summary>Splits the elements by a predicate.</summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="set">The input frozen set.</param>
/// <returns>
/// The frozen set of the elements that satisfy the predicate and the frozen set of the other ones, both with the
/// comparer of the input.
/// </returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>O(n): the function builds two new frozen sets.</remarks>
[<CompiledName "Partition">]
val partition : predicate : ('T -> bool) -> set : FrozenSet<'T> -> FrozenSet<'T> * FrozenSet<'T>

/// <summary>
/// Splits the elements by the choice that a function returns for them.
/// </summary>
/// <param name="partitioner">The function that returns <c>Choice1Of2</c> or <c>Choice2Of2</c> for an element.</param>
/// <param name="set">The input frozen set.</param>
/// <returns>
/// The frozen set of the values of the first choices and the frozen set of the values of the second choices, which
/// compare with <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>.
/// </returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n): the function builds two new frozen sets.
/// </remarks>
[<CompiledName "PartitionWith">]
val partitionWith : partitioner : ('T -> Choice<'T1, 'T2>) -> set : FrozenSet<'T> -> FrozenSet<'T1> * FrozenSet<'T2>
