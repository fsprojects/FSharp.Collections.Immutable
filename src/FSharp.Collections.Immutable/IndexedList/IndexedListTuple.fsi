/// <summary>
/// The functions of <see cref="T:FSharp.Collections.Immutable.IndexedList"/> that take or return tuples, with reference
/// tuples instead of struct tuples.
/// </summary>
/// <remarks>
/// After <c>open FSharp.Collections.Immutable.Tuple</c>, these functions shadow the default ones, and the other functions
/// of the default module stay available (ADR 0001, D3). Optional values stay
/// <see cref="T:Microsoft.FSharp.Core.FSharpValueOption`1"/>; open the namespace
/// <see cref="N:FSharp.Collections.Immutable.Option"/> after this one for the shapes of
/// <see cref="T:Microsoft.FSharp.Collections.ListModule"/>. Each function has the checks and the exceptions of the default
/// function.
/// </remarks>
[<RequireQualifiedAccess>]
module FSharp.Collections.Immutable.Tuple.IndexedList

open FSharp.Collections.Immutable

// ---- Creation ----

/// <summary>
/// Creates an indexed list from a generator, which returns the next element and the next state.
/// </summary>
/// <param name="generator">The function that returns the next element and state, or <c>ValueNone</c> to stop.</param>
/// <param name="state">The initial state.</param>
/// <returns>The generated indexed list.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Unfold">]
val unfold : generator : ('State -> ('T * 'State) voption) -> state : 'State -> IndexedList<'T>

// ---- Access ----

/// <summary>
/// Returns the first element and the other elements, or <c>ValueNone</c> when the indexed list is empty.
/// </summary>
/// <param name="list">The input indexed list.</param>
/// <returns>The first element and the elements after it, or <c>ValueNone</c>.</returns>
/// <remarks>
/// O(log n).
/// </remarks>
[<CompiledName "TryUncons">]
val tryUncons : list : IndexedList<'T> -> ('T * IndexedList<'T>) voption

/// <summary>Returns the first element and the other elements.</summary>
/// <param name="list">The input indexed list.</param>
/// <returns>The first element and the elements after it.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the indexed list is empty.</exception>
/// <remarks>O(log n).</remarks>
[<CompiledName "Uncons">]
val uncons : list : IndexedList<'T> -> 'T * IndexedList<'T>

/// <summary>
/// Matches an indexed list with a first element, and returns that element and the other elements.
/// </summary>
/// <param name="list">The input indexed list.</param>
/// <returns>The first element and the elements after it, or <c>ValueNone</c> for an empty indexed list.</returns>
/// <remarks>
/// O(log n). A struct partial pattern, so that a match allocates no option.
/// </remarks>
[<return : Struct>]
[<CompiledName "ConsPattern">]
val (|Cons|_|) : list : IndexedList<'T> -> ('T * IndexedList<'T>) voption

// ---- Transformation ----

/// <summary>
/// Returns every element of the first indexed list paired with every element of the second one.
/// </summary>
/// <param name="list1">The first indexed list.</param>
/// <param name="list2">The second indexed list.</param>
/// <returns>The pairs, in the order of the first indexed list and then of the second one.</returns>
/// <remarks>
/// O(n × m).
/// </remarks>
[<CompiledName "AllPairs">]
val allPairs : list1 : IndexedList<'T1> -> list2 : IndexedList<'T2> -> IndexedList<'T1 * 'T2>

/// <summary>Returns every element with its index.</summary>
/// <param name="list">The input indexed list.</param>
/// <returns>The pairs of index and element.</returns>
/// <remarks>O(n).</remarks>
[<CompiledName "Indexed">]
val indexed : list : IndexedList<'T> -> IndexedList<int * 'T>

/// <summary>Returns every element with its successor.</summary>
/// <param name="list">The input indexed list.</param>
/// <returns>The pairs of neighbours; empty for fewer than two elements.</returns>
/// <remarks>O(n).</remarks>
[<CompiledName "Pairwise">]
val pairwise : list : IndexedList<'T> -> IndexedList<'T * 'T>

/// <summary>Splits an indexed list of pairs into two indexed lists.</summary>
/// <param name="list">The input indexed list.</param>
/// <returns>The first components and the second components.</returns>
/// <remarks>O(n).</remarks>
[<CompiledName "Unzip">]
val unzip : list : IndexedList<'T1 * 'T2> -> IndexedList<'T1> * IndexedList<'T2>

/// <summary>Splits an indexed list of triples into three indexed lists.</summary>
/// <param name="list">The input indexed list.</param>
/// <returns>The first, the second and the third components.</returns>
/// <remarks>O(n).</remarks>
[<CompiledName "Unzip3">]
val unzip3 : list : IndexedList<'T1 * 'T2 * 'T3> -> IndexedList<'T1> * IndexedList<'T2> * IndexedList<'T3>

/// <summary>Combines two indexed lists into an indexed list of pairs.</summary>
/// <param name="list1">The first indexed list.</param>
/// <param name="list2">The second indexed list.</param>
/// <returns>The pairs of elements at the same index.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the indexed lists have different lengths.</exception>
/// <remarks>O(n).</remarks>
[<CompiledName "Zip">]
val zip : list1 : IndexedList<'T1> -> list2 : IndexedList<'T2> -> IndexedList<'T1 * 'T2>

/// <summary>
/// Combines three indexed lists into an indexed list of triples.
/// </summary>
/// <param name="list1">The first indexed list.</param>
/// <param name="list2">The second indexed list.</param>
/// <param name="list3">The third indexed list.</param>
/// <returns>The triples of elements at the same index.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the indexed lists have different lengths.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Zip3">]
val zip3 : list1 : IndexedList<'T1> -> list2 : IndexedList<'T2> -> list3 : IndexedList<'T3> -> IndexedList<'T1 * 'T2 * 'T3>

// ---- Folding ----

/// <summary>
/// Maps each element and threads a state through the mapping, from the first element to the last one.
/// </summary>
/// <param name="mapping">The function that returns the result for an element and the next state.</param>
/// <param name="state">The initial state.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>The results and the final state.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "MapFold">]
val mapFold :
    mapping : ('State -> 'T -> 'Result * 'State) -> state : 'State -> list : IndexedList<'T> -> IndexedList<'Result> * 'State

/// <summary>
/// Maps each element and threads a state through the mapping, from the last element to the first one.
/// </summary>
/// <param name="mapping">The function that returns the result for an element and the next state.</param>
/// <param name="list">The input indexed list.</param>
/// <param name="state">The initial state.</param>
/// <returns>The results, in the order of the elements, and the final state.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "MapFoldBack">]
val mapFoldBack :
    mapping : ('T -> 'State -> 'Result * 'State) -> list : IndexedList<'T> -> state : 'State -> IndexedList<'Result> * 'State

// ---- Partitioning and grouping ----

/// <summary>Counts the elements of each key of a projection.</summary>
/// <param name="projection">The function that returns the key of an element.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>The keys, in the order of their first occurrence, with the number of their elements.</returns>
/// <remarks>O(n). The keys compare in F# structural equality.</remarks>
[<CompiledName "CountBy">]
val countBy : projection : ('T -> 'Key) -> list : IndexedList<'T> -> IndexedList<'Key * int> when 'Key : equality

/// <summary>Groups the elements by the key of a projection.</summary>
/// <param name="projection">The function that returns the key of an element.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>The keys, in the order of their first occurrence, with their elements in their order.</returns>
/// <remarks>O(n). The keys compare in F# structural equality.</remarks>
[<CompiledName "GroupBy">]
val groupBy : projection : ('T -> 'Key) -> list : IndexedList<'T> -> IndexedList<'Key * IndexedList<'T>> when 'Key : equality

/// <summary>Splits the elements by a predicate.</summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>The elements that satisfy the predicate and the other ones, both in their order.</returns>
/// <remarks>O(n).</remarks>
[<CompiledName "Partition">]
val partition : predicate : ('T -> bool) -> list : IndexedList<'T> -> IndexedList<'T> * IndexedList<'T>

/// <summary>
/// Splits the elements by the choice that a function returns for them.
/// </summary>
/// <param name="partitioner">The function that returns <c>Choice1Of2</c> or <c>Choice2Of2</c> for an element.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>The values of the first choices and of the second choices, both in the order of the elements.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "PartitionWith">]
val partitionWith : partitioner : ('T -> Choice<'T1, 'T2>) -> list : IndexedList<'T> -> IndexedList<'T1> * IndexedList<'T2>

/// <summary>
/// Splits the indexed list at an index.
/// </summary>
/// <param name="index">The index of the first element of the second part.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>The elements before the index and the elements from the index.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the index is negative.</exception>
/// <exception cref="T:System.InvalidOperationException">Thrown when the index exceeds the length.</exception>
/// <remarks>
/// O(n log n). Each part is a new tree, unless it is empty or the whole indexed list.
/// </remarks>
[<CompiledName "SplitAt">]
val splitAt : index : int -> list : IndexedList<'T> -> IndexedList<'T> * IndexedList<'T>
