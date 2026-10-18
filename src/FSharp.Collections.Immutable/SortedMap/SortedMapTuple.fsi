/// <summary>
/// The functions of <see cref="T:FSharp.Collections.Immutable.SortedMap"/> that return tuples, with reference tuples instead
/// of struct tuples.
/// </summary>
/// <remarks>
/// <para>
/// After <c>open FSharp.Collections.Immutable.Tuple</c>, these functions shadow the default ones, and the other functions
/// of the default module stay available (ADR 0001, D3). Each function has the signature of the function with the same
/// name in <see cref="T:Microsoft.FSharp.Collections.MapModule"/>.
/// </para>
/// <para>
/// The functions that take bindings take every shape without this module (D13): <c>SortedMap.ofSeq</c> is overloaded for
/// reference tuples, and the nested module <see cref="T:FSharp.Collections.Immutable.SortedMap.Tuple"/> of the default
/// module holds the functions that change a map by reference tuples.
/// </para>
/// </remarks>
[<RequireQualifiedAccess>]
module FSharp.Collections.Immutable.Tuple.SortedMap

open FSharp.Collections.Immutable

// ---- Access ----

/// <summary>
/// Returns the binding of the largest key of the sorted map.
/// </summary>
/// <param name="table">The input sorted map.</param>
/// <returns>The key and the value of the last binding in the order of the keys.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <exception cref="T:System.Collections.Generic.KeyNotFoundException">Thrown when the sorted map is empty.</exception>
/// <remarks>
/// O(n), where
/// <see cref="M:Microsoft.FSharp.Collections.MapModule.MaxKeyValue``2(Microsoft.FSharp.Collections.FSharpMap{``0,``1})"/>
/// takes O(log n): <see cref="T:System.Collections.Immutable.ImmutableSortedDictionary`2"/> enumerates its bindings only
/// in ascending order and gives no other access to its largest key, so the function visits every binding.
/// </remarks>
[<CompiledName "MaxKeyValue">]
val maxKeyValue : table : SortedMap<'Key, 'Value> -> 'Key * 'Value

/// <summary>Returns the binding of the smallest key of the sorted map.</summary>
/// <param name="table">The input sorted map.</param>
/// <returns>The key and the value of the first binding in the order of the keys.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <exception cref="T:System.Collections.Generic.KeyNotFoundException">Thrown when the sorted map is empty.</exception>
/// <remarks>O(log n).</remarks>
[<CompiledName "MinKeyValue">]
val minKeyValue : table : SortedMap<'Key, 'Value> -> 'Key * 'Value

// ---- Partitioning ----

/// <summary>Splits the bindings by a predicate.</summary>
/// <param name="predicate">The function to test the bindings.</param>
/// <param name="table">The input sorted map.</param>
/// <returns>
/// The sorted map of the bindings that satisfy the predicate and the sorted map of the other ones, both with the
/// comparers of the input.
/// </returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>O(n log n).</remarks>
[<CompiledName "Partition">]
val partition :
    predicate : ('Key -> 'Value -> bool) -> table : SortedMap<'Key, 'Value> -> SortedMap<'Key, 'Value> * SortedMap<'Key, 'Value>

// ---- Conversion ----

/// <summary>
/// Returns a new array of the bindings, in the ascending order of the keys.
/// </summary>
/// <param name="table">The input sorted map.</param>
/// <returns>The array of key/value pairs.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "ToArray">]
val toArray : table : SortedMap<'Key, 'Value> -> ('Key * 'Value) array

/// <summary>
/// Returns a list of the bindings, in the ascending order of the keys.
/// </summary>
/// <param name="table">The input sorted map.</param>
/// <returns>The list of key/value pairs.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "ToList">]
val toList : table : SortedMap<'Key, 'Value> -> ('Key * 'Value) list

/// <summary>
/// Returns the bindings as a sequence, in the ascending order of the keys.
/// </summary>
/// <param name="table">The input sorted map.</param>
/// <returns>The sequence of key/value pairs, computed on demand.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(1); O(n) to enumerate.
/// </remarks>
[<CompiledName "ToSeq">]
val toSeq : table : SortedMap<'Key, 'Value> -> seq<'Key * 'Value>
