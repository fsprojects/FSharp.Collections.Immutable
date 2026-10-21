/// <summary>
/// The functions of <see cref="T:FSharp.Collections.Immutable.FrozenMap"/> that return tuples, with reference tuples instead
/// of struct tuples.
/// </summary>
/// <remarks>
/// <para>
/// After <c>open FSharp.Collections.Immutable.Tuple</c>, these functions shadow the default ones, and the other functions
/// of the default module stay available (ADR 0001, D3). Each function has the signature of the function with the same
/// name in <see cref="T:Microsoft.FSharp.Collections.MapModule"/>.
/// </para>
/// <para>
/// The functions that take bindings take every shape without this module (D13): <c>FrozenMap.ofSeq</c> is overloaded for
/// reference tuples.
/// </para>
/// </remarks>
[<RequireQualifiedAccess>]
module FSharp.Collections.Immutable.Tuple.FrozenMap

open FSharp.Collections.Immutable

// ---- Partitioning ----

/// <summary>
/// Splits the bindings by a predicate.
/// </summary>
/// <param name="predicate">The function to test the bindings.</param>
/// <param name="table">The input frozen map.</param>
/// <returns>
/// The frozen map of the bindings that satisfy the predicate and the frozen map of the other ones, both with the key
/// comparer of the input; the input itself in place of a part that has every binding.
/// </returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n). The function rebuilds the map: it freezes each part into a new frozen map with the key comparer of the input.
/// </remarks>
[<CompiledName "Partition">]
val partition :
    predicate : ('Key -> 'Value -> bool) -> table : FrozenMap<'Key, 'Value> -> FrozenMap<'Key, 'Value> * FrozenMap<'Key, 'Value>

// ---- Conversion ----

/// <summary>
/// Returns a new array of the bindings, in the order of enumeration.
/// </summary>
/// <param name="table">The input frozen map.</param>
/// <returns>The array of key/value pairs.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "ToArray">]
val toArray : table : FrozenMap<'Key, 'Value> -> ('Key * 'Value) array

/// <summary>Returns a list of the bindings, in the order of enumeration.</summary>
/// <param name="table">The input frozen map.</param>
/// <returns>The list of key/value pairs.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>O(n).</remarks>
[<CompiledName "ToList">]
val toList : table : FrozenMap<'Key, 'Value> -> ('Key * 'Value) list

/// <summary>
/// Returns the bindings as a sequence, in the order of enumeration.
/// </summary>
/// <param name="table">The input frozen map.</param>
/// <returns>The sequence of key/value pairs, computed on demand.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(1); O(n) to enumerate.
/// </remarks>
[<CompiledName "ToSeq">]
val toSeq : table : FrozenMap<'Key, 'Value> -> seq<'Key * 'Value>
