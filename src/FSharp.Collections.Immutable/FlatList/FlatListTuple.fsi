/// <summary>
/// The functions of <see cref="T:FSharp.Collections.Immutable.FlatList"/> that take or return tuples, with
/// reference tuples instead of struct tuples.
/// </summary>
/// <remarks>
/// After <c>open FSharp.Collections.Immutable.Tuple</c>, these functions shadow the default ones, and the other functions
/// of the default module stay available (ADR 0001, D3). Optional values stay
/// <see cref="T:Microsoft.FSharp.Core.FSharpValueOption`1"/>; open the namespace
/// <see cref="N:FSharp.Collections.Immutable.Option"/> after this one for the shapes of
/// <see cref="T:Microsoft.FSharp.Collections.ArrayModule"/>.
/// </remarks>
[<RequireQualifiedAccess>]
module FSharp.Collections.Immutable.Tuple.FlatList

open FSharp.Collections.Immutable

// ---- Creation ----

/// <summary>Creates a flat list from a generator, which returns the next element and the next state.</summary>
/// <param name="generator">The function that returns the next element and state, or <c>ValueNone</c> to stop.</param>
/// <param name="state">The initial state.</param>
/// <returns>The generated flat list.</returns>
/// <remarks>O(n).</remarks>
[<CompiledName "Unfold">]
val unfold : generator : ('State -> ('T * 'State) voption) -> state : 'State -> FlatList<'T>

// ---- Transformation ----

/// <summary>Returns every element of the first flat list paired with every element of the second one.</summary>
/// <param name="array1">The first flat list.</param>
/// <param name="array2">The second flat list.</param>
/// <returns>The pairs, in the order of the first flat list and then of the second one.</returns>
/// <remarks>O(n × m).</remarks>
[<CompiledName "AllPairs">]
val allPairs : array1 : FlatList<'T1> -> array2 : FlatList<'T2> -> FlatList<'T1 * 'T2>

/// <summary>Returns every element with its index.</summary>
/// <param name="array">The input flat list.</param>
/// <returns>The pairs of index and element.</returns>
/// <remarks>O(n).</remarks>
[<CompiledName "Indexed">]
val indexed : array : FlatList<'T> -> FlatList<int * 'T>

/// <summary>Returns every element with its successor.</summary>
/// <param name="array">The input flat list.</param>
/// <returns>The pairs of neighbours; empty for fewer than two elements.</returns>
/// <remarks>O(n).</remarks>
[<CompiledName "Pairwise">]
val pairwise : array : FlatList<'T> -> FlatList<'T * 'T>

/// <summary>Splits a flat list of pairs into two flat lists.</summary>
/// <param name="array">The input flat list.</param>
/// <returns>The first components and the second components.</returns>
/// <remarks>O(n).</remarks>
[<CompiledName "Unzip">]
val unzip : array : FlatList<'T1 * 'T2> -> FlatList<'T1> * FlatList<'T2>

/// <summary>Splits a flat list of triples into three flat lists.</summary>
/// <param name="array">The input flat list.</param>
/// <returns>The first, the second and the third components.</returns>
/// <remarks>O(n).</remarks>
[<CompiledName "Unzip3">]
val unzip3 : array : FlatList<'T1 * 'T2 * 'T3> -> FlatList<'T1> * FlatList<'T2> * FlatList<'T3>

/// <summary>Combines two flat lists into a flat list of pairs.</summary>
/// <param name="array1">The first flat list.</param>
/// <param name="array2">The second flat list.</param>
/// <returns>The pairs of elements at the same index.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the flat lists have different lengths.</exception>
/// <remarks>O(n).</remarks>
[<CompiledName "Zip">]
val zip : array1 : FlatList<'T1> -> array2 : FlatList<'T2> -> FlatList<'T1 * 'T2>

/// <summary>Combines three flat lists into a flat list of triples.</summary>
/// <param name="array1">The first flat list.</param>
/// <param name="array2">The second flat list.</param>
/// <param name="array3">The third flat list.</param>
/// <returns>The triples of elements at the same index.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the flat lists have different lengths.</exception>
/// <remarks>O(n).</remarks>
[<CompiledName "Zip3">]
val zip3 : array1 : FlatList<'T1> -> array2 : FlatList<'T2> -> array3 : FlatList<'T3> -> FlatList<'T1 * 'T2 * 'T3>

// ---- Folding ----

/// <summary>Maps each element and threads a state through the mapping, from the first element to the last one.</summary>
/// <param name="mapping">The function that returns the result for an element and the next state.</param>
/// <param name="state">The initial state.</param>
/// <param name="array">The input flat list.</param>
/// <returns>The results and the final state.</returns>
/// <remarks>O(n).</remarks>
[<CompiledName "MapFold">]
val mapFold : mapping : ('State -> 'T -> 'Result * 'State) -> state : 'State -> array : FlatList<'T> -> FlatList<'Result> * 'State

/// <summary>Maps each element and threads a state through the mapping, from the last element to the first one.</summary>
/// <param name="mapping">The function that returns the result for an element and the next state.</param>
/// <param name="array">The input flat list.</param>
/// <param name="state">The initial state.</param>
/// <returns>The results, in the order of the elements, and the final state.</returns>
/// <remarks>O(n).</remarks>
[<CompiledName "MapFoldBack">]
val mapFoldBack :
    mapping : ('T -> 'State -> 'Result * 'State) -> array : FlatList<'T> -> state : 'State -> FlatList<'Result> * 'State

// ---- Partitioning and grouping ----

/// <summary>Counts the elements of each key of a projection.</summary>
/// <param name="projection">The function that returns the key of an element.</param>
/// <param name="array">The input flat list.</param>
/// <returns>The keys, in the order of their first occurrence, with the number of their elements.</returns>
/// <remarks>O(n). The keys compare in F# structural equality.</remarks>
[<CompiledName "CountBy">]
val countBy : projection : ('T -> 'Key) -> array : FlatList<'T> -> FlatList<'Key * int> when 'Key : equality

/// <summary>Groups the elements by the key of a projection.</summary>
/// <param name="projection">The function that returns the key of an element.</param>
/// <param name="array">The input flat list.</param>
/// <returns>The keys, in the order of their first occurrence, with their elements in their order.</returns>
/// <remarks>O(n). The keys compare in F# structural equality.</remarks>
[<CompiledName "GroupBy">]
val groupBy : projection : ('T -> 'Key) -> array : FlatList<'T> -> FlatList<'Key * FlatList<'T>> when 'Key : equality

/// <summary>Splits the elements by a predicate.</summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="array">The input flat list.</param>
/// <returns>The elements that satisfy the predicate and the other ones, both in their order.</returns>
/// <remarks>O(n).</remarks>
[<CompiledName "Partition">]
val partition : predicate : ('T -> bool) -> array : FlatList<'T> -> FlatList<'T> * FlatList<'T>

/// <summary>Splits the elements by the choice that a function returns for them.</summary>
/// <param name="partitioner">The function that returns <c>Choice1Of2</c> or <c>Choice2Of2</c> for an element.</param>
/// <param name="array">The input flat list.</param>
/// <returns>The values of the first choices and of the second choices, both in the order of the elements.</returns>
/// <remarks>O(n).</remarks>
[<CompiledName "PartitionWith">]
val partitionWith : partitioner : ('T -> Choice<'T1, 'T2>) -> array : FlatList<'T> -> FlatList<'T1> * FlatList<'T2>

/// <summary>Splits the flat list at an index.</summary>
/// <param name="index">The index of the first element of the second part.</param>
/// <param name="array">The input flat list.</param>
/// <returns>The elements before the index and the elements from the index.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the index is negative.</exception>
/// <exception cref="T:System.InvalidOperationException">Thrown when the index exceeds the length.</exception>
/// <remarks>O(n). The parts are copies, unless they are empty or the whole flat list.</remarks>
[<CompiledName "SplitAt">]
val splitAt : index : int -> array : FlatList<'T> -> FlatList<'T> * FlatList<'T>
