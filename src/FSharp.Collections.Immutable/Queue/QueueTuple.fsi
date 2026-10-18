/// <summary>
/// The functions of <see cref="T:FSharp.Collections.Immutable.Queue"/> that take or return tuples, with reference tuples
/// instead of struct tuples.
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
module FSharp.Collections.Immutable.Tuple.Queue

open FSharp.Collections.Immutable

// ---- Creation ----

/// <summary>
/// Creates a queue from a generator, which returns the next element and the next state.
/// </summary>
/// <param name="generator">The function that returns the next element and state, or <c>ValueNone</c> to stop.</param>
/// <param name="state">The initial state.</param>
/// <returns>The generated queue, whose first element is the first generated one.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Unfold">]
val unfold : generator : ('State -> ('T * 'State) voption) -> state : 'State -> Queue<'T>

// ---- Access ----

/// <summary>
/// Returns the first element and the other elements, or <c>ValueNone</c> when the queue is empty.
/// </summary>
/// <param name="queue">The input queue.</param>
/// <returns>The first element and the elements after it, or <c>ValueNone</c>.</returns>
/// <remarks>
/// Amortized O(1).
/// </remarks>
[<CompiledName "TryPeekDequeue">]
val tryUncons : queue : Queue<'T> -> ('T * Queue<'T>) voption

/// <summary>Returns the first element and the other elements.</summary>
/// <param name="queue">The input queue.</param>
/// <returns>The first element and the elements after it.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the queue is empty.</exception>
/// <remarks>Amortized O(1).</remarks>
[<CompiledName "PeekDequeue">]
val uncons : queue : Queue<'T> -> 'T * Queue<'T>

/// <summary>
/// Matches a queue with a first element, and returns that element and the other elements.
/// </summary>
/// <param name="queue">The input queue.</param>
/// <returns>The first element and the elements after it, or <c>ValueNone</c> for an empty queue.</returns>
/// <remarks>
/// Amortized O(1). A struct partial pattern, so that a match allocates no option.
/// </remarks>
[<return : Struct>]
[<CompiledName "ConsPattern">]
val (|Cons|_|) : queue : Queue<'T> -> ('T * Queue<'T>) voption

// ---- Transformation ----

/// <summary>
/// Returns every element of the first queue paired with every element of the second one.
/// </summary>
/// <param name="queue1">The first queue.</param>
/// <param name="queue2">The second queue.</param>
/// <returns>The pairs, in the order of the first queue and then of the second one.</returns>
/// <remarks>
/// O(n × m).
/// </remarks>
[<CompiledName "AllPairs">]
val allPairs : queue1 : Queue<'T1> -> queue2 : Queue<'T2> -> Queue<'T1 * 'T2>

/// <summary>Returns every element with its index.</summary>
/// <param name="queue">The input queue.</param>
/// <returns>The pairs of index and element.</returns>
/// <remarks>O(n).</remarks>
[<CompiledName "Indexed">]
val indexed : queue : Queue<'T> -> Queue<int * 'T>

/// <summary>Returns every element with its successor.</summary>
/// <param name="queue">The input queue.</param>
/// <returns>The pairs of neighbours; empty for fewer than two elements.</returns>
/// <remarks>O(n).</remarks>
[<CompiledName "Pairwise">]
val pairwise : queue : Queue<'T> -> Queue<'T * 'T>

/// <summary>Splits a queue of pairs into two queues.</summary>
/// <param name="queue">The input queue.</param>
/// <returns>The first components and the second components.</returns>
/// <remarks>O(n).</remarks>
[<CompiledName "Unzip">]
val unzip : queue : Queue<'T1 * 'T2> -> Queue<'T1> * Queue<'T2>

/// <summary>Splits a queue of triples into three queues.</summary>
/// <param name="queue">The input queue.</param>
/// <returns>The first, the second and the third components.</returns>
/// <remarks>O(n).</remarks>
[<CompiledName "Unzip3">]
val unzip3 : queue : Queue<'T1 * 'T2 * 'T3> -> Queue<'T1> * Queue<'T2> * Queue<'T3>

/// <summary>Combines two queues into a queue of pairs.</summary>
/// <param name="queue1">The first queue.</param>
/// <param name="queue2">The second queue.</param>
/// <returns>The pairs of elements at the same index.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the queues have different lengths.</exception>
/// <remarks>O(n).</remarks>
[<CompiledName "Zip">]
val zip : queue1 : Queue<'T1> -> queue2 : Queue<'T2> -> Queue<'T1 * 'T2>

/// <summary>Combines three queues into a queue of triples.</summary>
/// <param name="queue1">The first queue.</param>
/// <param name="queue2">The second queue.</param>
/// <param name="queue3">The third queue.</param>
/// <returns>The triples of elements at the same index.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the queues have different lengths.</exception>
/// <remarks>O(n).</remarks>
[<CompiledName "Zip3">]
val zip3 : queue1 : Queue<'T1> -> queue2 : Queue<'T2> -> queue3 : Queue<'T3> -> Queue<'T1 * 'T2 * 'T3>

// ---- Folding ----

/// <summary>
/// Maps each element and threads a state through the mapping, from the first element to the last one.
/// </summary>
/// <param name="mapping">The function that returns the result for an element and the next state.</param>
/// <param name="state">The initial state.</param>
/// <param name="queue">The input queue.</param>
/// <returns>The results and the final state.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "MapFold">]
val mapFold : mapping : ('State -> 'T -> 'Result * 'State) -> state : 'State -> queue : Queue<'T> -> Queue<'Result> * 'State

/// <summary>
/// Maps each element and threads a state through the mapping, from the last element to the first one.
/// </summary>
/// <param name="mapping">The function that returns the result for an element and the next state.</param>
/// <param name="queue">The input queue.</param>
/// <param name="state">The initial state.</param>
/// <returns>The results, in the order of the elements, and the final state.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "MapFoldBack">]
val mapFoldBack : mapping : ('T -> 'State -> 'Result * 'State) -> queue : Queue<'T> -> state : 'State -> Queue<'Result> * 'State

// ---- Partitioning and grouping ----

/// <summary>Counts the elements of each key of a projection.</summary>
/// <param name="projection">The function that returns the key of an element.</param>
/// <param name="queue">The input queue.</param>
/// <returns>The keys, in the order of their first occurrence, with the number of their elements.</returns>
/// <remarks>O(n). The keys compare in F# structural equality.</remarks>
[<CompiledName "CountBy">]
val countBy : projection : ('T -> 'Key) -> queue : Queue<'T> -> Queue<'Key * int> when 'Key : equality

/// <summary>Groups the elements by the key of a projection.</summary>
/// <param name="projection">The function that returns the key of an element.</param>
/// <param name="queue">The input queue.</param>
/// <returns>The keys, in the order of their first occurrence, with their elements in their order.</returns>
/// <remarks>O(n). The keys compare in F# structural equality.</remarks>
[<CompiledName "GroupBy">]
val groupBy : projection : ('T -> 'Key) -> queue : Queue<'T> -> Queue<'Key * Queue<'T>> when 'Key : equality

/// <summary>Splits the elements by a predicate.</summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="queue">The input queue.</param>
/// <returns>The elements that satisfy the predicate and the other ones, both in their order.</returns>
/// <remarks>O(n).</remarks>
[<CompiledName "Partition">]
val partition : predicate : ('T -> bool) -> queue : Queue<'T> -> Queue<'T> * Queue<'T>

/// <summary>
/// Splits the elements by the choice that a function returns for them.
/// </summary>
/// <param name="partitioner">The function that returns <c>Choice1Of2</c> or <c>Choice2Of2</c> for an element.</param>
/// <param name="queue">The input queue.</param>
/// <returns>The values of the first choices and of the second choices, both in the order of the elements.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "PartitionWith">]
val partitionWith : partitioner : ('T -> Choice<'T1, 'T2>) -> queue : Queue<'T> -> Queue<'T1> * Queue<'T2>

/// <summary>
/// Splits the queue at an index.
/// </summary>
/// <param name="index">The index of the first element of the second part.</param>
/// <param name="queue">The input queue.</param>
/// <returns>The elements before the index and the elements from the index.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the index is negative.</exception>
/// <exception cref="T:System.InvalidOperationException">Thrown when the index exceeds the length.</exception>
/// <remarks>
/// Amortized O(index). The second part shares its elements with the input.
/// </remarks>
[<CompiledName "SplitAt">]
val splitAt : index : int -> queue : Queue<'T> -> Queue<'T> * Queue<'T>
