/// <summary>
/// The functions of <see cref="T:FSharp.Collections.Immutable.IStack"/> that take or return tuples, with reference tuples
/// instead of struct tuples.
/// </summary>
/// <remarks>
/// After <c>open FSharp.Collections.Immutable.Tuple</c>, these functions shadow the default ones, and the other functions
/// of the default module stay available (ADR 0001, D3). Optional values stay
/// <see cref="T:Microsoft.FSharp.Core.FSharpValueOption`1"/>; open the namespace
/// <see cref="N:FSharp.Collections.Immutable.Option"/> after this one for the shapes of
/// <see cref="T:Microsoft.FSharp.Collections.ListModule"/>. Each function has the checks and the exceptions of the default
/// function, and the results keep the implementation of the input where the default function keeps it.
/// </remarks>
[<RequireQualifiedAccess>]
module FSharp.Collections.Immutable.Tuple.IStack

open FSharp.Collections.Immutable

// ---- Access ----

/// <summary>
/// Returns the first element and the other elements, or <c>ValueNone</c> when the stack is empty.
/// </summary>
/// <param name="stack">The input stack.</param>
/// <returns>The first element and the elements after it, or <c>ValueNone</c>.</returns>
/// <remarks>
/// O(1).
/// </remarks>
[<CompiledName "TryPeekPop">]
val tryUncons : stack : IStack<'T> -> ('T * IStack<'T>) voption

/// <summary>Returns the first element and the other elements.</summary>
/// <param name="stack">The input stack.</param>
/// <returns>The first element and the elements after it, in a stack of the implementation of the input.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the stack is empty.</exception>
/// <remarks>O(1).</remarks>
[<CompiledName "PeekPop">]
val uncons : stack : IStack<'T> -> 'T * IStack<'T>

/// <summary>
/// Matches a stack with a first element, and returns that element and the other elements.
/// </summary>
/// <param name="stack">The input stack.</param>
/// <returns>The first element and the elements after it, or <c>ValueNone</c> for an empty stack.</returns>
/// <remarks>
/// O(1). A struct partial pattern, so that a match allocates no option.
/// </remarks>
[<return : Struct>]
[<CompiledName "ConsPattern">]
val (|Cons|_|) : stack : IStack<'T> -> ('T * IStack<'T>) voption

// ---- Transformation ----

/// <summary>
/// Returns every element of the first stack paired with every element of the second one.
/// </summary>
/// <param name="stack1">The first stack.</param>
/// <param name="stack2">The second stack.</param>
/// <returns>The pairs, in the order of the first stack and then of the second one.</returns>
/// <remarks>
/// O(n × m).
/// </remarks>
[<CompiledName "AllPairs">]
val allPairs : stack1 : IStack<'T1> -> stack2 : IStack<'T2> -> Stack<'T1 * 'T2>

/// <summary>Returns every element with its index.</summary>
/// <param name="stack">The input stack.</param>
/// <returns>The pairs of index and element; the top has the index 0.</returns>
/// <remarks>O(n).</remarks>
[<CompiledName "Indexed">]
val indexed : stack : IStack<'T> -> Stack<int * 'T>

/// <summary>Returns every element with its successor.</summary>
/// <param name="stack">The input stack.</param>
/// <returns>The pairs of neighbours; empty for fewer than two elements.</returns>
/// <remarks>O(n).</remarks>
[<CompiledName "Pairwise">]
val pairwise : stack : IStack<'T> -> Stack<'T * 'T>

/// <summary>Splits a stack of pairs into two stacks.</summary>
/// <param name="stack">The input stack.</param>
/// <returns>The first components and the second components.</returns>
/// <remarks>O(n).</remarks>
[<CompiledName "Unzip">]
val unzip : stack : IStack<'T1 * 'T2> -> Stack<'T1> * Stack<'T2>

/// <summary>Splits a stack of triples into three stacks.</summary>
/// <param name="stack">The input stack.</param>
/// <returns>The first, the second and the third components.</returns>
/// <remarks>O(n).</remarks>
[<CompiledName "Unzip3">]
val unzip3 : stack : IStack<'T1 * 'T2 * 'T3> -> Stack<'T1> * Stack<'T2> * Stack<'T3>

/// <summary>Combines two stacks into a stack of pairs.</summary>
/// <param name="stack1">The first stack.</param>
/// <param name="stack2">The second stack.</param>
/// <returns>The pairs of elements at the same index.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the stacks have different lengths.</exception>
/// <remarks>O(n).</remarks>
[<CompiledName "Zip">]
val zip : stack1 : IStack<'T1> -> stack2 : IStack<'T2> -> Stack<'T1 * 'T2>

/// <summary>Combines three stacks into a stack of triples.</summary>
/// <param name="stack1">The first stack.</param>
/// <param name="stack2">The second stack.</param>
/// <param name="stack3">The third stack.</param>
/// <returns>The triples of elements at the same index.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the stacks have different lengths.</exception>
/// <remarks>O(n).</remarks>
[<CompiledName "Zip3">]
val zip3 : stack1 : IStack<'T1> -> stack2 : IStack<'T2> -> stack3 : IStack<'T3> -> Stack<'T1 * 'T2 * 'T3>

// ---- Folding ----

/// <summary>
/// Maps each element and threads a state through the mapping, from the first element to the last one.
/// </summary>
/// <param name="mapping">The function that returns the result for an element and the next state.</param>
/// <param name="state">The initial state.</param>
/// <param name="stack">The input stack.</param>
/// <returns>The results and the final state.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "MapFold">]
val mapFold : mapping : ('State -> 'T -> 'Result * 'State) -> state : 'State -> stack : IStack<'T> -> Stack<'Result> * 'State

/// <summary>
/// Maps each element and threads a state through the mapping, from the last element to the first one.
/// </summary>
/// <param name="mapping">The function that returns the result for an element and the next state.</param>
/// <param name="stack">The input stack.</param>
/// <param name="state">The initial state.</param>
/// <returns>The results, in the order of the elements, and the final state.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "MapFoldBack">]
val mapFoldBack : mapping : ('T -> 'State -> 'Result * 'State) -> stack : IStack<'T> -> state : 'State -> Stack<'Result> * 'State

// ---- Partitioning and grouping ----

/// <summary>Counts the elements of each key of a projection.</summary>
/// <param name="projection">The function that returns the key of an element.</param>
/// <param name="stack">The input stack.</param>
/// <returns>The keys, in the order of their first occurrence, with the number of their elements.</returns>
/// <remarks>O(n). The keys compare in F# structural equality.</remarks>
[<CompiledName "CountBy">]
val countBy : projection : ('T -> 'Key) -> stack : IStack<'T> -> Stack<'Key * int> when 'Key : equality

/// <summary>Groups the elements by the key of a projection.</summary>
/// <param name="projection">The function that returns the key of an element.</param>
/// <param name="stack">The input stack.</param>
/// <returns>The keys, in the order of their first occurrence, with their elements in their order.</returns>
/// <remarks>O(n). The keys compare in F# structural equality.</remarks>
[<CompiledName "GroupBy">]
val groupBy : projection : ('T -> 'Key) -> stack : IStack<'T> -> Stack<'Key * Stack<'T>> when 'Key : equality

/// <summary>Splits the elements by a predicate.</summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="stack">The input stack.</param>
/// <returns>
/// The elements that satisfy the predicate and the other ones, both in their order, in stacks of the implementation of
/// the input.
/// </returns>
/// <remarks>O(n).</remarks>
[<CompiledName "Partition">]
val partition : predicate : ('T -> bool) -> stack : IStack<'T> -> IStack<'T> * IStack<'T>

/// <summary>
/// Splits the elements by the choice that a function returns for them.
/// </summary>
/// <param name="partitioner">The function that returns <c>Choice1Of2</c> or <c>Choice2Of2</c> for an element.</param>
/// <param name="stack">The input stack.</param>
/// <returns>The values of the first choices and of the second choices, both in the order of the elements.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "PartitionWith">]
val partitionWith : partitioner : ('T -> Choice<'T1, 'T2>) -> stack : IStack<'T> -> Stack<'T1> * Stack<'T2>

/// <summary>Splits the stack at an index.</summary>
/// <param name="index">The index of the first element of the second part.</param>
/// <param name="stack">The input stack.</param>
/// <returns>The elements before the index and the elements from the index, in stacks of the implementation of the input.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the index is negative.</exception>
/// <exception cref="T:System.InvalidOperationException">Thrown when the index exceeds the length.</exception>
/// <remarks>O(index).</remarks>
[<CompiledName "SplitAt">]
val splitAt : index : int -> stack : IStack<'T> -> IStack<'T> * IStack<'T>
