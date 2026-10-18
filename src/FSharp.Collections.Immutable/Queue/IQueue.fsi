/// <summary>
/// Functions for <see cref="T:System.Collections.Immutable.IImmutableQueue`1"/>, the interface of the immutable queues.
/// </summary>
/// <remarks>
/// <para>
/// The module mirrors <see cref="T:Microsoft.FSharp.Collections.ListModule"/> as
/// <see cref="T:FSharp.Collections.Immutable.Queue"/> does, over the elements in the order in which they leave the queue:
/// each function has the semantics, the argument checks and the exceptions of the function with the same name in that
/// module (ADR 0001), and its sorting functions are stable. The module has no creation functions, because an interface
/// cannot create a queue.
/// </para>
/// <para>
/// A function whose result has the element type of its input keeps the implementation of the input: it builds the result
/// with the members of the input, <see cref="M:System.Collections.Immutable.IImmutableQueue`1.Clear"/> followed by
/// <see cref="M:System.Collections.Immutable.IImmutableQueue`1.Enqueue(`0)"/>, or
/// <see cref="M:System.Collections.Immutable.IImmutableQueue`1.Dequeue"/>. A function that changes the element type, or
/// that combines several queues into one, returns an <see cref="T:System.Collections.Immutable.ImmutableQueue`1"/>.
/// </para>
/// <para>
/// Every function throws <see cref="T:System.ArgumentNullException"/> for a <see langword="null"/> queue. The
/// complexities in the remarks are the ones of an <see cref="T:System.Collections.Immutable.ImmutableQueue`1"/>, which
/// does not store its length.
/// </para>
/// <para>
/// Functions that return an optional value return a <see cref="T:Microsoft.FSharp.Core.FSharpValueOption`1"/>, and functions
/// that return tuples return struct tuples. The namespaces <see cref="N:FSharp.Collections.Immutable.Option"/> and
/// <see cref="N:FSharp.Collections.Immutable.Tuple"/> contain a module with the same name whose functions use options
/// and reference tuples instead.
/// </para>
/// </remarks>
[<RequireQualifiedAccess>]
module FSharp.Collections.Immutable.IQueue

open System
open System.Collections.Immutable

// ---- Properties ----

/// <summary>Returns whether the queue has no elements.</summary>
/// <param name="queue">The input queue.</param>
/// <returns><see langword="true"/> when the queue is empty.</returns>
/// <remarks>O(1).</remarks>
[<CompiledName "IsEmpty">]
val isEmpty : queue : IQueue<'T> -> bool

/// <summary>
/// Returns the number of elements of the queue.
/// </summary>
/// <param name="queue">The input queue.</param>
/// <returns>The length of the queue.</returns>
/// <remarks>
/// O(n): the interface has no length, so the function counts the elements.
/// </remarks>
[<CompiledName "Length">]
val length : queue : IQueue<'T> -> int

// ---- Access ----

/// <summary>Returns the only element of the queue.</summary>
/// <param name="queue">The input queue.</param>
/// <returns>The only element.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the queue does not have exactly one element.</exception>
/// <remarks>Amortized O(1).</remarks>
[<CompiledName "ExactlyOne">]
val exactlyOne : queue : IQueue<'T> -> 'T

/// <summary>
/// Returns the first element of the queue, the one that leaves it next.
/// </summary>
/// <param name="queue">The input queue.</param>
/// <returns>The first element.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the queue is empty.</exception>
/// <remarks>
/// O(1). The function wraps <see cref="M:System.Collections.Immutable.IImmutableQueue`1.Peek"/>.
/// </remarks>
[<CompiledName "Peek">]
val head : queue : IQueue<'T> -> 'T

/// <summary>
/// Returns the last element of the queue, the one that joined it last.
/// </summary>
/// <param name="queue">The input queue.</param>
/// <returns>The last element.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the queue is empty.</exception>
/// <remarks>
/// O(n). The function wraps <see cref="M:System.Linq.Enumerable.Last``1(System.Collections.Generic.IEnumerable{``0})"/>.
/// </remarks>
[<CompiledName "Last">]
val last : queue : IQueue<'T> -> 'T

/// <summary>
/// Returns the queue without its first element.
/// </summary>
/// <param name="queue">The input queue.</param>
/// <returns>The elements after the first one, in a queue of the implementation of the input.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the queue is empty.</exception>
/// <remarks>
/// Amortized O(1). The function wraps <see cref="M:System.Collections.Immutable.IImmutableQueue`1.Dequeue"/>.
/// </remarks>
[<CompiledName "Dequeue">]
val tail : queue : IQueue<'T> -> IQueue<'T>

/// <summary>
/// Returns the only element of the queue, or <c>ValueNone</c> when it does not have exactly one element.
/// </summary>
/// <param name="queue">The input queue.</param>
/// <returns>The only element, or <c>ValueNone</c>.</returns>
/// <remarks>
/// Amortized O(1).
/// </remarks>
[<CompiledName "TryExactlyOne">]
val tryExactlyOne : queue : IQueue<'T> -> 'T voption

/// <summary>
/// Returns the first element of the queue, or <c>ValueNone</c> when it is empty.
/// </summary>
/// <param name="queue">The input queue.</param>
/// <returns>The first element, or <c>ValueNone</c>.</returns>
/// <remarks>
/// O(1).
/// </remarks>
[<CompiledName "TryPeek">]
val tryHead : queue : IQueue<'T> -> 'T voption

/// <summary>
/// Returns the last element of the queue, or <c>ValueNone</c> when it is empty.
/// </summary>
/// <param name="queue">The input queue.</param>
/// <returns>The last element, or <c>ValueNone</c>.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "TryLast">]
val tryLast : queue : IQueue<'T> -> 'T voption

/// <summary>
/// Returns the queue without its first element, or <c>ValueNone</c> when it is empty.
/// </summary>
/// <param name="queue">The input queue.</param>
/// <returns>The elements after the first one, in a queue of the implementation of the input, or <c>ValueNone</c>.</returns>
/// <remarks>
/// Amortized O(1).
/// </remarks>
[<CompiledName "TryDequeue">]
val tryTail : queue : IQueue<'T> -> IQueue<'T> voption

/// <summary>
/// Returns the first element and the other elements, or <c>ValueNone</c> when the queue is empty.
/// </summary>
/// <param name="queue">The input queue.</param>
/// <returns>The first element and the elements after it, or <c>ValueNone</c>.</returns>
/// <remarks>
/// Amortized O(1).
/// </remarks>
[<CompiledName "TryPeekDequeue">]
val tryUncons : queue : IQueue<'T> -> struct ('T * IQueue<'T>) voption

/// <summary>
/// Returns the first element and the other elements.
/// </summary>
/// <param name="queue">The input queue.</param>
/// <returns>The first element and the elements after it, in a queue of the implementation of the input.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the queue is empty.</exception>
/// <remarks>
/// Amortized O(1). The function throws what <see cref="M:FSharp.Collections.Immutable.IQueue.Peek``1(System.Collections.Immutable.IImmutableQueue{``0})"/>
/// throws.
/// </remarks>
[<CompiledName "PeekDequeue">]
val uncons : queue : IQueue<'T> -> struct ('T * IQueue<'T>)

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
val (|Cons|_|) : queue : IQueue<'T> -> struct ('T * IQueue<'T>) voption

/// <summary>Matches an empty queue.</summary>
/// <param name="queue">The input queue.</param>
/// <returns><see langword="true"/> when the queue is empty.</returns>
/// <remarks>O(1).</remarks>
[<CompiledName "NilPattern">]
val (|Nil|_|) : queue : IQueue<'T> -> bool

// ---- Search ----

/// <summary>
/// Returns whether the queue contains an element equal to a value, in F# structural equality.
/// </summary>
/// <param name="value">The value to find.</param>
/// <param name="source">The input queue.</param>
/// <returns><see langword="true"/> when an element equals the value.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Contains">]
val contains : value : 'T -> source : IQueue<'T> -> bool when 'T : equality

/// <summary>Returns whether any element satisfies the predicate.</summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="queue">The input queue.</param>
/// <returns><see langword="true"/> when an element satisfies the predicate.</returns>
/// <remarks>O(n).</remarks>
[<CompiledName "Exists">]
val exists : predicate : ('T -> bool) -> queue : IQueue<'T> -> bool

/// <summary>
/// Returns whether any pair of elements at the same index satisfies the predicate.
/// </summary>
/// <param name="predicate">The function to test the pairs.</param>
/// <param name="queue1">The first queue.</param>
/// <param name="queue2">The second queue.</param>
/// <returns><see langword="true"/> when a pair satisfies the predicate.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when no pair satisfies the predicate and the queues have different lengths.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Exists2">]
val exists2 : predicate : ('T1 -> 'T2 -> bool) -> queue1 : IQueue<'T1> -> queue2 : IQueue<'T2> -> bool

/// <summary>Returns the first element that satisfies the predicate.</summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="queue">The input queue.</param>
/// <returns>The first element that satisfies the predicate.</returns>
/// <exception cref="T:System.Collections.Generic.KeyNotFoundException">Thrown when no element satisfies the predicate.</exception>
/// <remarks>O(n).</remarks>
[<CompiledName "Find">]
val find : predicate : ('T -> bool) -> queue : IQueue<'T> -> 'T

/// <summary>Returns the last element that satisfies the predicate.</summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="queue">The input queue.</param>
/// <returns>The last element that satisfies the predicate.</returns>
/// <exception cref="T:System.Collections.Generic.KeyNotFoundException">Thrown when no element satisfies the predicate.</exception>
/// <remarks>O(n).</remarks>
[<CompiledName "FindBack">]
val findBack : predicate : ('T -> bool) -> queue : IQueue<'T> -> 'T

/// <summary>
/// Returns the index of the first element that satisfies the predicate.
/// </summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="queue">The input queue.</param>
/// <returns>The index of the first element that satisfies the predicate.</returns>
/// <exception cref="T:System.Collections.Generic.KeyNotFoundException">Thrown when no element satisfies the predicate.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "FindIndex">]
val findIndex : predicate : ('T -> bool) -> queue : IQueue<'T> -> int

/// <summary>
/// Returns the index of the last element that satisfies the predicate.
/// </summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="queue">The input queue.</param>
/// <returns>The index of the last element that satisfies the predicate.</returns>
/// <exception cref="T:System.Collections.Generic.KeyNotFoundException">Thrown when no element satisfies the predicate.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "FindIndexBack">]
val findIndexBack : predicate : ('T -> bool) -> queue : IQueue<'T> -> int

/// <summary>Returns whether all elements satisfy the predicate.</summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="queue">The input queue.</param>
/// <returns><see langword="true"/> when every element satisfies the predicate.</returns>
/// <remarks>O(n).</remarks>
[<CompiledName "ForAll">]
val forall : predicate : ('T -> bool) -> queue : IQueue<'T> -> bool

/// <summary>
/// Returns whether all pairs of elements at the same index satisfy the predicate.
/// </summary>
/// <param name="predicate">The function to test the pairs.</param>
/// <param name="queue1">The first queue.</param>
/// <param name="queue2">The second queue.</param>
/// <returns><see langword="true"/> when every pair satisfies the predicate.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when every pair satisfies the predicate and the queues have different lengths.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "ForAll2">]
val forall2 : predicate : ('T1 -> 'T2 -> bool) -> queue1 : IQueue<'T1> -> queue2 : IQueue<'T2> -> bool

/// <summary>
/// Returns the first value that the chooser returns for the elements.
/// </summary>
/// <param name="chooser">The function that returns a value, or <c>ValueNone</c> to go on.</param>
/// <param name="queue">The input queue.</param>
/// <returns>The first chosen value.</returns>
/// <exception cref="T:System.Collections.Generic.KeyNotFoundException">Thrown when the chooser returns <c>ValueNone</c> for every element.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Pick">]
val pick : chooser : ('T -> 'U voption) -> queue : IQueue<'T> -> 'U

/// <summary>
/// Returns the first element that satisfies the predicate, or <c>ValueNone</c>.
/// </summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="queue">The input queue.</param>
/// <returns>The first element that satisfies the predicate, or <c>ValueNone</c>.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "TryFind">]
val tryFind : predicate : ('T -> bool) -> queue : IQueue<'T> -> 'T voption

/// <summary>
/// Returns the last element that satisfies the predicate, or <c>ValueNone</c>.
/// </summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="queue">The input queue.</param>
/// <returns>The last element that satisfies the predicate, or <c>ValueNone</c>.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "TryFindBack">]
val tryFindBack : predicate : ('T -> bool) -> queue : IQueue<'T> -> 'T voption

/// <summary>
/// Returns the index of the first element that satisfies the predicate, or <c>ValueNone</c>.
/// </summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="queue">The input queue.</param>
/// <returns>The index of the first element that satisfies the predicate, or <c>ValueNone</c>.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "TryFindIndex">]
val tryFindIndex : predicate : ('T -> bool) -> queue : IQueue<'T> -> int voption

/// <summary>
/// Returns the index of the last element that satisfies the predicate, or <c>ValueNone</c>.
/// </summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="queue">The input queue.</param>
/// <returns>The index of the last element that satisfies the predicate, or <c>ValueNone</c>.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "TryFindIndexBack">]
val tryFindIndexBack : predicate : ('T -> bool) -> queue : IQueue<'T> -> int voption

/// <summary>
/// Returns the first value that the chooser returns for the elements, or <c>ValueNone</c>.
/// </summary>
/// <param name="chooser">The function that returns a value, or <c>ValueNone</c> to go on.</param>
/// <param name="queue">The input queue.</param>
/// <returns>The first chosen value, or <c>ValueNone</c>.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "TryPick">]
val tryPick : chooser : ('T -> 'U voption) -> queue : IQueue<'T> -> 'U voption

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
val allPairs : queue1 : IQueue<'T1> -> queue2 : IQueue<'T2> -> Queue<struct ('T1 * 'T2)>

/// <summary>
/// Returns a queue of the elements of the first queue followed by the elements of the second one.
/// </summary>
/// <param name="queue1">The first queue.</param>
/// <param name="queue2">The second queue.</param>
/// <returns>The concatenation, in a queue of the implementation of the first queue.</returns>
/// <remarks>
/// <para>
/// O(m), where m is the length of the second queue: the function enqueues its elements into the first queue with
/// <see cref="M:System.Collections.Immutable.IImmutableQueue`1.Enqueue(`0)"/>.
/// </para>
/// <para>
/// The result keeps the implementation of the first queue, which receives the elements, as
/// <see cref="M:System.Collections.Immutable.IImmutableQueue`1.Enqueue(`0)"/> keeps it (ADR 0001, D14). It does so also
/// when the first queue is empty: the function does not return the second queue then, as
/// <see cref="M:FSharp.Collections.Immutable.Queue.Append``1(System.Collections.Immutable.ImmutableQueue{``0},System.Collections.Immutable.ImmutableQueue{``0})"/>
/// does, because the second queue can have another implementation.
/// </para>
/// </remarks>
[<CompiledName "Append">]
val append : queue1 : IQueue<'T> -> queue2 : IQueue<'T> -> IQueue<'T>

/// <summary>
/// Returns the values that the chooser returns for the elements.
/// </summary>
/// <param name="chooser">The function that returns a value, or <c>ValueNone</c> to skip the element.</param>
/// <param name="queue">The input queue.</param>
/// <returns>The chosen values.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Choose">]
val choose : chooser : ('T -> 'U voption) -> queue : IQueue<'T> -> Queue<'U>

/// <summary>Divides the queue into chunks of a given size.</summary>
/// <param name="chunkSize">The maximum size of a chunk.</param>
/// <param name="queue">The input queue.</param>
/// <returns>The chunks; the last one can be smaller.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when <paramref name="chunkSize"/> is not positive.</exception>
/// <remarks>O(n).</remarks>
[<CompiledName "ChunkBySize">]
val chunkBySize : chunkSize : int -> queue : IQueue<'T> -> Queue<Queue<'T>>

/// <summary>
/// Concatenates the queues that a function returns for the elements.
/// </summary>
/// <param name="mapping">The function that returns a queue for each element.</param>
/// <param name="queue">The input queue.</param>
/// <returns>The concatenation of the returned queues.</returns>
/// <remarks>
/// O(n + the total length of the returned queues).
/// </remarks>
[<CompiledName "Collect">]
val collect : mapping : ('T -> IQueue<'U>) -> queue : IQueue<'T> -> Queue<'U>

/// <summary>Concatenates a sequence of queues.</summary>
/// <param name="queues">The queues.</param>
/// <returns>The concatenation, in an immutable queue, because the queues can have different implementations.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="queues"/> is <see langword="null"/> or a default flat list, or one of its queues is <see langword="null"/>.</exception>
/// <remarks>O(the total length).</remarks>
[<CompiledName "Concat">]
val concat : queues : seq<IQueue<'T>> -> Queue<'T>

/// <summary>
/// Returns a queue with a value added after the last element.
/// </summary>
/// <param name="value">The value to add.</param>
/// <param name="queue">The input queue.</param>
/// <returns>A queue of the implementation of the input with the elements and the value, which leaves it last.</returns>
/// <remarks>
/// O(1). The function wraps <see cref="M:System.Collections.Immutable.IImmutableQueue`1.Enqueue(`0)"/>.
/// </remarks>
[<CompiledName "Enqueue">]
val enqueue : value : 'T -> queue : IQueue<'T> -> IQueue<'T>

/// <summary>
/// Returns a queue with values added after the last element, in their order.
/// </summary>
/// <param name="values">The values to add.</param>
/// <param name="queue">The input queue.</param>
/// <returns>A queue of the implementation of the input with the elements and then the values.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="values"/> is <see langword="null"/> or a default flat list.</exception>
/// <remarks>
/// O(m), where m is the number of values; without values, the function returns the input. The function calls
/// <see cref="M:System.Collections.Immutable.IImmutableQueue`1.Enqueue(`0)"/> for each value.
/// </remarks>
[<CompiledName "EnqueueMany">]
val enqueueMany : values : seq<'T> -> queue : IQueue<'T> -> IQueue<'T>

/// <summary>Returns the elements that satisfy the predicate.</summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="queue">The input queue.</param>
/// <returns>The elements that satisfy the predicate, in their order, in a queue of the implementation of the input.</returns>
/// <remarks>O(n).</remarks>
[<CompiledName "Filter">]
val filter : predicate : ('T -> bool) -> queue : IQueue<'T> -> IQueue<'T>

/// <summary>Returns every element with its index.</summary>
/// <param name="queue">The input queue.</param>
/// <returns>The pairs of index and element.</returns>
/// <remarks>O(n).</remarks>
[<CompiledName "Indexed">]
val indexed : queue : IQueue<'T> -> Queue<struct (int * 'T)>

/// <summary>Returns the results of a function applied to each element.</summary>
/// <param name="mapping">The function to transform the elements, called in the order of the elements.</param>
/// <param name="queue">The input queue.</param>
/// <returns>The results, in the order of the elements.</returns>
/// <remarks>O(n).</remarks>
[<CompiledName "Map">]
val map : mapping : ('T -> 'U) -> queue : IQueue<'T> -> Queue<'U>

/// <summary>
/// Returns the results of a function applied to the pairs of elements at the same index.
/// </summary>
/// <param name="mapping">The function to transform the pairs.</param>
/// <param name="queue1">The first queue.</param>
/// <param name="queue2">The second queue.</param>
/// <returns>The results.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the queues have different lengths.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Map2">]
val map2 : mapping : ('T1 -> 'T2 -> 'U) -> queue1 : IQueue<'T1> -> queue2 : IQueue<'T2> -> Queue<'U>

/// <summary>
/// Returns the results of a function applied to the triples of elements at the same index.
/// </summary>
/// <param name="mapping">The function to transform the triples.</param>
/// <param name="queue1">The first queue.</param>
/// <param name="queue2">The second queue.</param>
/// <param name="queue3">The third queue.</param>
/// <returns>The results.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the queues have different lengths.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Map3">]
val map3 :
    mapping : ('T1 -> 'T2 -> 'T3 -> 'U) -> queue1 : IQueue<'T1> -> queue2 : IQueue<'T2> -> queue3 : IQueue<'T3> -> Queue<'U>

/// <summary>
/// Returns the results of a function applied to each element and its index.
/// </summary>
/// <param name="mapping">The function to transform the elements.</param>
/// <param name="queue">The input queue.</param>
/// <returns>The results.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "MapIndexed">]
val mapi : mapping : (int -> 'T -> 'U) -> queue : IQueue<'T> -> Queue<'U>

/// <summary>
/// Returns the results of a function applied to the index and the pair of elements at that index.
/// </summary>
/// <param name="mapping">The function to transform the pairs.</param>
/// <param name="queue1">The first queue.</param>
/// <param name="queue2">The second queue.</param>
/// <returns>The results.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the queues have different lengths.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "MapIndexed2">]
val mapi2 : mapping : (int -> 'T1 -> 'T2 -> 'U) -> queue1 : IQueue<'T1> -> queue2 : IQueue<'T2> -> Queue<'U>

/// <summary>Returns every element with its successor.</summary>
/// <param name="queue">The input queue.</param>
/// <returns>The pairs of neighbours; empty for fewer than two elements.</returns>
/// <remarks>O(n).</remarks>
[<CompiledName "Pairwise">]
val pairwise : queue : IQueue<'T> -> Queue<struct ('T * 'T)>

/// <summary>
/// Returns the elements moved to the indices that a function computes.
/// </summary>
/// <param name="indexMap">The function that maps the index of each element to its new index.</param>
/// <param name="queue">The input queue.</param>
/// <returns>The permuted elements, in a queue of the implementation of the input.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the function does not compute a permutation.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Permute">]
val permute : indexMap : (int -> int) -> queue : IQueue<'T> -> IQueue<'T>

/// <summary>Returns the elements in reverse order.</summary>
/// <param name="queue">The input queue.</param>
/// <returns>The reversed elements, in a queue of the implementation of the input.</returns>
/// <remarks>O(n).</remarks>
[<CompiledName "Reverse">]
val rev : queue : IQueue<'T> -> IQueue<'T>

/// <summary>
/// Returns the transpose of a sequence of queues of the same length.
/// </summary>
/// <param name="queues">The queues.</param>
/// <returns>An immutable queue whose element i holds the element i of every input queue.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="queues"/> is <see langword="null"/> or a default flat list, or one of its queues is <see langword="null"/>.</exception>
/// <exception cref="T:System.ArgumentException">Thrown when the queues have different lengths.</exception>
/// <remarks>
/// O(the total length).
/// </remarks>
[<CompiledName "Transpose">]
val transpose : queues : seq<IQueue<'T>> -> Queue<Queue<'T>>

/// <summary>Splits a queue of pairs into two immutable queues.</summary>
/// <param name="queue">The input queue.</param>
/// <returns>The first components and the second components.</returns>
/// <remarks>O(n).</remarks>
[<CompiledName "Unzip">]
val unzip : queue : IQueue<struct ('T1 * 'T2)> -> struct (Queue<'T1> * Queue<'T2>)

/// <summary>Splits a queue of triples into three immutable queues.</summary>
/// <param name="queue">The input queue.</param>
/// <returns>The first, the second and the third components.</returns>
/// <remarks>O(n).</remarks>
[<CompiledName "Unzip3">]
val unzip3 : queue : IQueue<struct ('T1 * 'T2 * 'T3)> -> struct (Queue<'T1> * Queue<'T2> * Queue<'T3>)

/// <summary>
/// Returns the elements that satisfy the predicate.
/// </summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="queue">The input queue.</param>
/// <returns>The elements that satisfy the predicate, in their order, in a queue of the implementation of the input.</returns>
/// <remarks>
/// O(n). A synonym of <see cref="M:FSharp.Collections.Immutable.IQueue.Filter``1(Microsoft.FSharp.Core.FSharpFunc{``0,System.Boolean},System.Collections.Immutable.IImmutableQueue{``0})"/>.
/// </remarks>
[<CompiledName "Where">]
val where : predicate : ('T -> bool) -> queue : IQueue<'T> -> IQueue<'T>

/// <summary>
/// Returns the windows of a given size that slide over the queue.
/// </summary>
/// <param name="windowSize">The size of a window.</param>
/// <param name="queue">The input queue.</param>
/// <returns>The windows; empty when the queue is shorter than a window.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when <paramref name="windowSize"/> is not positive.</exception>
/// <remarks>
/// O(n × windowSize).
/// </remarks>
[<CompiledName "Windowed">]
val windowed : windowSize : int -> queue : IQueue<'T> -> Queue<Queue<'T>>

/// <summary>Combines two queues into an immutable queue of pairs.</summary>
/// <param name="queue1">The first queue.</param>
/// <param name="queue2">The second queue.</param>
/// <returns>The pairs of elements at the same index.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the queues have different lengths.</exception>
/// <remarks>O(n).</remarks>
[<CompiledName "Zip">]
val zip : queue1 : IQueue<'T1> -> queue2 : IQueue<'T2> -> Queue<struct ('T1 * 'T2)>

/// <summary>Combines three queues into an immutable queue of triples.</summary>
/// <param name="queue1">The first queue.</param>
/// <param name="queue2">The second queue.</param>
/// <param name="queue3">The third queue.</param>
/// <returns>The triples of elements at the same index.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the queues have different lengths.</exception>
/// <remarks>O(n).</remarks>
[<CompiledName "Zip3">]
val zip3 : queue1 : IQueue<'T1> -> queue2 : IQueue<'T2> -> queue3 : IQueue<'T3> -> Queue<struct ('T1 * 'T2 * 'T3)>

// ---- Folding ----

/// <summary>Returns the average of the elements.</summary>
/// <param name="queue">The input queue.</param>
/// <returns>The average.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the queue is empty.</exception>
/// <remarks>O(n).</remarks>
[<CompiledName "Average">]
val inline average :
    queue : IQueue< ^T > -> ^T
        when ^T : (static member (+) : ^T * ^T -> ^T)
        and ^T : (static member DivideByInt : ^T * int -> ^T)
        and ^T : (static member Zero : ^T)

/// <summary>
/// Returns the average of the results of a function applied to each element.
/// </summary>
/// <param name="projection">The function to transform the elements.</param>
/// <param name="queue">The input queue.</param>
/// <returns>The average of the results.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the queue is empty.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "AverageBy">]
val inline averageBy :
    projection : ('T -> ^U) -> queue : IQueue<'T> -> ^U
        when ^U : (static member (+) : ^U * ^U -> ^U)
        and ^U : (static member DivideByInt : ^U * int -> ^U)
        and ^U : (static member Zero : ^U)

/// <summary>Compares two queues element by element with a comparer.</summary>
/// <param name="comparer">The function that compares two elements.</param>
/// <param name="queue1">The first queue.</param>
/// <param name="queue2">The second queue.</param>
/// <returns>
/// The first non-zero result of the comparer, or the comparison of the lengths when every common element compares equal.
/// </returns>
/// <remarks>O(n).</remarks>
[<CompiledName "CompareWith">]
val compareWith : comparer : ('T -> 'T -> int) -> queue1 : IQueue<'T> -> queue2 : IQueue<'T> -> int

/// <summary>
/// Applies a function to a state and each element, from the first element to the last one.
/// </summary>
/// <param name="folder">The function that computes the next state.</param>
/// <param name="state">The initial state.</param>
/// <param name="queue">The input queue.</param>
/// <returns>The final state.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Fold">]
val fold : folder : ('State -> 'T -> 'State) -> state : 'State -> queue : IQueue<'T> -> 'State

/// <summary>
/// Applies a function to a state and each pair of elements at the same index, from the first index to the last one.
/// </summary>
/// <param name="folder">The function that computes the next state.</param>
/// <param name="state">The initial state.</param>
/// <param name="queue1">The first queue.</param>
/// <param name="queue2">The second queue.</param>
/// <returns>The final state.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the queues have different lengths.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Fold2">]
val fold2 : folder : ('State -> 'T1 -> 'T2 -> 'State) -> state : 'State -> queue1 : IQueue<'T1> -> queue2 : IQueue<'T2> -> 'State

/// <summary>
/// Applies a function to each element and a state, from the last element to the first one.
/// </summary>
/// <param name="folder">The function that computes the next state.</param>
/// <param name="queue">The input queue.</param>
/// <param name="state">The initial state.</param>
/// <returns>The final state.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "FoldBack">]
val foldBack : folder : ('T -> 'State -> 'State) -> queue : IQueue<'T> -> state : 'State -> 'State

/// <summary>
/// Applies a function to each pair of elements at the same index and a state, from the last index to the first one.
/// </summary>
/// <param name="folder">The function that computes the next state.</param>
/// <param name="queue1">The first queue.</param>
/// <param name="queue2">The second queue.</param>
/// <param name="state">The initial state.</param>
/// <returns>The final state.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the queues have different lengths.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "FoldBack2">]
val foldBack2 :
    folder : ('T1 -> 'T2 -> 'State -> 'State) -> queue1 : IQueue<'T1> -> queue2 : IQueue<'T2> -> state : 'State -> 'State

/// <summary>Applies an action to each element.</summary>
/// <param name="action">The action.</param>
/// <param name="queue">The input queue.</param>
/// <remarks>O(n).</remarks>
[<CompiledName "Iterate">]
val iter : action : ('T -> unit) -> queue : IQueue<'T> -> unit

/// <summary>
/// Applies an action to each pair of elements at the same index.
/// </summary>
/// <param name="action">The action.</param>
/// <param name="queue1">The first queue.</param>
/// <param name="queue2">The second queue.</param>
/// <exception cref="T:System.ArgumentException">Thrown when the queues have different lengths.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Iterate2">]
val iter2 : action : ('T1 -> 'T2 -> unit) -> queue1 : IQueue<'T1> -> queue2 : IQueue<'T2> -> unit

/// <summary>Applies an action to each element and its index.</summary>
/// <param name="action">The action.</param>
/// <param name="queue">The input queue.</param>
/// <remarks>O(n).</remarks>
[<CompiledName "IterateIndexed">]
val iteri : action : (int -> 'T -> unit) -> queue : IQueue<'T> -> unit

/// <summary>
/// Applies an action to the index and the pair of elements at that index.
/// </summary>
/// <param name="action">The action.</param>
/// <param name="queue1">The first queue.</param>
/// <param name="queue2">The second queue.</param>
/// <exception cref="T:System.ArgumentException">Thrown when the queues have different lengths.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "IterateIndexed2">]
val iteri2 : action : (int -> 'T1 -> 'T2 -> unit) -> queue1 : IQueue<'T1> -> queue2 : IQueue<'T2> -> unit

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
val mapFold :
    mapping : ('State -> 'T -> struct ('Result * 'State)) ->
    state : 'State ->
    queue : IQueue<'T> ->
        struct (Queue<'Result> * 'State)

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
val mapFoldBack :
    mapping : ('T -> 'State -> struct ('Result * 'State)) ->
    queue : IQueue<'T> ->
    state : 'State ->
        struct (Queue<'Result> * 'State)

/// <summary>Returns the greatest element, in F# generic comparison.</summary>
/// <param name="queue">The input queue.</param>
/// <returns>The greatest element.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the queue is empty.</exception>
/// <remarks>O(n).</remarks>
[<CompiledName "Max">]
val max : queue : IQueue<'T> -> 'T when 'T : comparison

/// <summary>
/// Returns the element with the greatest result of a projection.
/// </summary>
/// <param name="projection">The function to transform the elements for the comparison.</param>
/// <param name="queue">The input queue.</param>
/// <returns>The first element with the greatest result.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the queue is empty.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "MaxBy">]
val maxBy : projection : ('T -> 'U) -> queue : IQueue<'T> -> 'T when 'U : comparison

/// <summary>Returns the smallest element, in F# generic comparison.</summary>
/// <param name="queue">The input queue.</param>
/// <returns>The smallest element.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the queue is empty.</exception>
/// <remarks>O(n).</remarks>
[<CompiledName "Min">]
val min : queue : IQueue<'T> -> 'T when 'T : comparison

/// <summary>
/// Returns the element with the smallest result of a projection.
/// </summary>
/// <param name="projection">The function to transform the elements for the comparison.</param>
/// <param name="queue">The input queue.</param>
/// <returns>The first element with the smallest result.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the queue is empty.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "MinBy">]
val minBy : projection : ('T -> 'U) -> queue : IQueue<'T> -> 'T when 'U : comparison

/// <summary>
/// Combines the elements with a function, from the first element to the last one.
/// </summary>
/// <param name="reduction">The function that combines the result so far with the next element.</param>
/// <param name="queue">The input queue.</param>
/// <returns>The combined value.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the queue is empty.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Reduce">]
val reduce : reduction : ('T -> 'T -> 'T) -> queue : IQueue<'T> -> 'T

/// <summary>
/// Combines the elements with a function, from the last element to the first one.
/// </summary>
/// <param name="reduction">The function that combines an element with the result so far.</param>
/// <param name="queue">The input queue.</param>
/// <returns>The combined value.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the queue is empty.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "ReduceBack">]
val reduceBack : reduction : ('T -> 'T -> 'T) -> queue : IQueue<'T> -> 'T

/// <summary>
/// Returns every state of a fold from the first element to the last one, the initial state first.
/// </summary>
/// <param name="folder">The function that computes the next state.</param>
/// <param name="state">The initial state.</param>
/// <param name="queue">The input queue.</param>
/// <returns>The states; one more than the elements.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Scan">]
val scan : folder : ('State -> 'T -> 'State) -> state : 'State -> queue : IQueue<'T> -> Queue<'State>

/// <summary>
/// Returns every state of a fold from the last element to the first one, the initial state last.
/// </summary>
/// <param name="folder">The function that computes the next state.</param>
/// <param name="queue">The input queue.</param>
/// <param name="state">The initial state.</param>
/// <returns>The states; one more than the elements.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "ScanBack">]
val scanBack : folder : ('T -> 'State -> 'State) -> queue : IQueue<'T> -> state : 'State -> Queue<'State>

/// <summary>Returns the sum of the elements.</summary>
/// <param name="queue">The input queue.</param>
/// <returns>The sum; zero for an empty queue.</returns>
/// <remarks>O(n).</remarks>
[<CompiledName "Sum">]
val inline sum : queue : IQueue< ^T > -> ^T when ^T : (static member (+) : ^T * ^T -> ^T) and ^T : (static member Zero : ^T)

/// <summary>
/// Returns the sum of the results of a function applied to each element.
/// </summary>
/// <param name="projection">The function to transform the elements.</param>
/// <param name="queue">The input queue.</param>
/// <returns>The sum of the results; zero for an empty queue.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "SumBy">]
val inline sumBy :
    projection : ('T -> ^U) -> queue : IQueue<'T> -> ^U
        when ^U : (static member (+) : ^U * ^U -> ^U) and ^U : (static member Zero : ^U)

// ---- Partitioning and grouping ----

/// <summary>Counts the elements of each key of a projection.</summary>
/// <param name="projection">The function that returns the key of an element.</param>
/// <param name="queue">The input queue.</param>
/// <returns>The keys, in the order of their first occurrence, with the number of their elements.</returns>
/// <remarks>O(n). The keys compare in F# structural equality.</remarks>
[<CompiledName "CountBy">]
val countBy : projection : ('T -> 'Key) -> queue : IQueue<'T> -> Queue<struct ('Key * int)> when 'Key : equality

/// <summary>
/// Returns the elements without duplicates, in F# structural equality.
/// </summary>
/// <param name="queue">The input queue.</param>
/// <returns>The first occurrence of each element, in their order, in a queue of the implementation of the input.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Distinct">]
val distinct : queue : IQueue<'T> -> IQueue<'T> when 'T : equality

/// <summary>
/// Returns the elements without duplicates of the result of a projection.
/// </summary>
/// <param name="projection">The function that returns the key of an element.</param>
/// <param name="queue">The input queue.</param>
/// <returns>The first element of each key, in their order, in a queue of the implementation of the input.</returns>
/// <remarks>
/// O(n). The keys compare in F# structural equality.
/// </remarks>
[<CompiledName "DistinctBy">]
val distinctBy : projection : ('T -> 'Key) -> queue : IQueue<'T> -> IQueue<'T> when 'Key : equality

/// <summary>
/// Returns the distinct elements that do not equal any of the excluded items.
/// </summary>
/// <param name="itemsToExclude">The items to remove.</param>
/// <param name="queue">The input queue.</param>
/// <returns>The distinct remaining elements, in their order, in a queue of the implementation of the input.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="itemsToExclude"/> is <see langword="null"/> or a default flat list.</exception>
/// <remarks>
/// O(n + m). An empty queue is returned without a look at the items to exclude.
/// </remarks>
[<CompiledName "Except">]
val except : itemsToExclude : seq<'T> -> queue : IQueue<'T> -> IQueue<'T> when 'T : equality

/// <summary>Groups the elements by the key of a projection.</summary>
/// <param name="projection">The function that returns the key of an element.</param>
/// <param name="queue">The input queue.</param>
/// <returns>The keys, in the order of their first occurrence, with their elements in their order.</returns>
/// <remarks>O(n). The keys compare in F# structural equality.</remarks>
[<CompiledName "GroupBy">]
val groupBy : projection : ('T -> 'Key) -> queue : IQueue<'T> -> Queue<struct ('Key * Queue<'T>)> when 'Key : equality

/// <summary>Splits the elements by a predicate.</summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="queue">The input queue.</param>
/// <returns>
/// The elements that satisfy the predicate and the other ones, both in their order, in queues of the implementation of
/// the input.
/// </returns>
/// <remarks>O(n).</remarks>
[<CompiledName "Partition">]
val partition : predicate : ('T -> bool) -> queue : IQueue<'T> -> struct (IQueue<'T> * IQueue<'T>)

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
val partitionWith : partitioner : ('T -> Choice<'T1, 'T2>) -> queue : IQueue<'T> -> struct (Queue<'T1> * Queue<'T2>)

/// <summary>
/// Returns the queue without its first elements.
/// </summary>
/// <param name="count">The number of elements to skip; a negative count skips nothing.</param>
/// <param name="queue">The input queue.</param>
/// <returns>The remaining elements, in a queue of the implementation of the input.</returns>
/// <exception cref="T:System.ArgumentOutOfRangeException">Thrown when the count exceeds the length.</exception>
/// <remarks>
/// Amortized O(count). The function dequeues the elements with
/// <see cref="M:System.Collections.Immutable.IImmutableQueue`1.Dequeue"/>; a count that is not positive returns the input.
/// </remarks>
[<CompiledName "Skip">]
val skip : count : int -> queue : IQueue<'T> -> IQueue<'T>

/// <summary>
/// Returns the queue without its last elements.
/// </summary>
/// <param name="count">The number of elements to skip; a negative count skips nothing.</param>
/// <param name="queue">The input queue.</param>
/// <returns>The remaining elements, in a queue of the implementation of the input.</returns>
/// <exception cref="T:System.ArgumentOutOfRangeException">Thrown when the count exceeds the length.</exception>
/// <remarks>
/// O(n). A count that is not positive returns the input. The checks follow
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.Skip``1(System.Int32,Microsoft.FSharp.Collections.FSharpList{``0})"/>.
/// </remarks>
[<CompiledName "SkipLast">]
val skipLast : count : int -> queue : IQueue<'T> -> IQueue<'T>

/// <summary>
/// Returns the queue without its first elements that satisfy the predicate.
/// </summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="queue">The input queue.</param>
/// <returns>The elements from the first one that does not satisfy the predicate, in a queue of the implementation of the input.</returns>
/// <remarks>
/// Amortized O(k), where k is the number of skipped elements. The function dequeues the elements with
/// <see cref="M:System.Collections.Immutable.IImmutableQueue`1.Dequeue"/>.
/// </remarks>
[<CompiledName "SkipWhile">]
val skipWhile : predicate : ('T -> bool) -> queue : IQueue<'T> -> IQueue<'T>

/// <summary>
/// Splits the queue at an index.
/// </summary>
/// <param name="index">The index of the first element of the second part.</param>
/// <param name="queue">The input queue.</param>
/// <returns>The elements before the index and the elements from the index, in queues of the implementation of the input.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the index is negative.</exception>
/// <exception cref="T:System.InvalidOperationException">Thrown when the index exceeds the length.</exception>
/// <remarks>
/// Amortized O(index). The function dequeues the elements of the first part from the input, which gives the second part;
/// the first part is the input itself when the index is the length.
/// </remarks>
[<CompiledName "SplitAt">]
val splitAt : index : int -> queue : IQueue<'T> -> struct (IQueue<'T> * IQueue<'T>)

/// <summary>
/// Splits the queue into at most a given number of parts of nearly equal length.
/// </summary>
/// <param name="count">The maximum number of parts.</param>
/// <param name="queue">The input queue.</param>
/// <returns>The parts; the first ones are one element longer when the length does not divide evenly.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when <paramref name="count"/> is not positive.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "SplitInto">]
val splitInto : count : int -> queue : IQueue<'T> -> Queue<Queue<'T>>

/// <summary>
/// Returns the first elements of the queue.
/// </summary>
/// <param name="count">The number of elements to take.</param>
/// <param name="queue">The input queue.</param>
/// <returns>The first elements, in a queue of the implementation of the input.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the count is negative.</exception>
/// <exception cref="T:System.InvalidOperationException">Thrown when the count exceeds the length.</exception>
/// <remarks>
/// O(count). The input itself is returned when the count is the length.
/// </remarks>
[<CompiledName "Take">]
val take : count : int -> queue : IQueue<'T> -> IQueue<'T>

/// <summary>
/// Returns the last elements of the queue.
/// </summary>
/// <param name="count">The number of elements to take.</param>
/// <param name="queue">The input queue.</param>
/// <returns>The last elements, in a queue of the implementation of the input.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the count is negative.</exception>
/// <exception cref="T:System.InvalidOperationException">Thrown when the count exceeds the length.</exception>
/// <remarks>
/// O(n). The input itself is returned when the count is the length. The checks follow
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.Take``1(System.Int32,Microsoft.FSharp.Collections.FSharpList{``0})"/>.
/// </remarks>
[<CompiledName "TakeLast">]
val takeLast : count : int -> queue : IQueue<'T> -> IQueue<'T>

/// <summary>
/// Returns the first elements of the queue that satisfy the predicate.
/// </summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="queue">The input queue.</param>
/// <returns>The elements before the first one that does not satisfy the predicate, in a queue of the implementation of the input.</returns>
/// <remarks>
/// O(k), where k is the length of the result. The input itself is returned when every element satisfies the predicate.
/// </remarks>
[<CompiledName "TakeWhile">]
val takeWhile : predicate : ('T -> bool) -> queue : IQueue<'T> -> IQueue<'T>

/// <summary>
/// Returns at most a given number of first elements.
/// </summary>
/// <param name="count">The maximum number of elements; a count that is not positive returns an empty queue.</param>
/// <param name="queue">The input queue.</param>
/// <returns>The first elements, in a queue of the implementation of the input.</returns>
/// <remarks>
/// O(count). The input itself is returned when it is not longer than the count.
/// </remarks>
[<CompiledName "Truncate">]
val truncate : count : int -> queue : IQueue<'T> -> IQueue<'T>

// ---- Sorting ----

/// <summary>
/// Sorts the elements in the order of F# generic comparison.
/// </summary>
/// <param name="queue">The input queue.</param>
/// <returns>The sorted elements, in a queue of the implementation of the input.</returns>
/// <exception cref="T:System.InvalidOperationException">
/// Thrown when the comparison of two elements throws, as in <see cref="M:Microsoft.FSharp.Collections.ListModule.Sort``1(Microsoft.FSharp.Collections.FSharpList{``0})"/>.
/// </exception>
/// <remarks>
/// O(n log n). This is a stable sort, as <see cref="M:Microsoft.FSharp.Collections.ListModule.Sort``1(Microsoft.FSharp.Collections.FSharpList{``0})"/>:
/// equal elements keep their order.
/// </remarks>
[<CompiledName "Sort">]
val sort : queue : IQueue<'T> -> IQueue<'T> when 'T : comparison

/// <summary>
/// Sorts the elements by the result of a projection, in the order of F# generic comparison.
/// </summary>
/// <param name="projection">The function that returns the key of an element.</param>
/// <param name="queue">The input queue.</param>
/// <returns>The sorted elements, in a queue of the implementation of the input.</returns>
/// <remarks>
/// O(n log n). This is a stable sort: elements with equal keys keep their order.
/// </remarks>
[<CompiledName "SortBy">]
val sortBy : projection : ('T -> 'Key) -> queue : IQueue<'T> -> IQueue<'T> when 'Key : comparison

/// <summary>
/// Sorts the elements by the result of a projection, in descending order.
/// </summary>
/// <param name="projection">The function that returns the key of an element.</param>
/// <param name="queue">The input queue.</param>
/// <returns>The sorted elements, in a queue of the implementation of the input.</returns>
/// <exception cref="T:System.InvalidOperationException">
/// Thrown when the projection or the comparison of two keys throws: the projection runs in the comparison, as in
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.SortByDescending``2(Microsoft.FSharp.Core.FSharpFunc{``0,``1},Microsoft.FSharp.Collections.FSharpList{``0})"/>.
/// </exception>
/// <remarks>
/// O(n log n). This is a stable sort: elements with equal keys keep their order.
/// </remarks>
[<CompiledName "SortByDescending">]
val sortByDescending : projection : ('T -> 'Key) -> queue : IQueue<'T> -> IQueue<'T> when 'Key : comparison

/// <summary>
/// Sorts the elements in descending order of F# generic comparison.
/// </summary>
/// <param name="queue">The input queue.</param>
/// <returns>The sorted elements, in a queue of the implementation of the input.</returns>
/// <exception cref="T:System.InvalidOperationException">
/// Thrown when the comparison of two elements throws, as in <see cref="M:Microsoft.FSharp.Collections.ListModule.SortDescending``1(Microsoft.FSharp.Collections.FSharpList{``0})"/>.
/// </exception>
/// <remarks>
/// O(n log n). This is a stable sort: equal elements keep their order.
/// </remarks>
[<CompiledName "SortDescending">]
val sortDescending : queue : IQueue<'T> -> IQueue<'T> when 'T : comparison

/// <summary>
/// Sorts the elements with a comparison function.
/// </summary>
/// <param name="comparer">The function that compares two elements.</param>
/// <param name="queue">The input queue.</param>
/// <returns>The sorted elements, in a queue of the implementation of the input.</returns>
/// <exception cref="T:System.InvalidOperationException">
/// Thrown when the comparer throws, as in
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.SortWith``1(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpFunc{``0,System.Int32}},Microsoft.FSharp.Collections.FSharpList{``0})"/>.
/// </exception>
/// <remarks>
/// O(n log n). This is a stable sort: elements that compare equal keep their order.
/// </remarks>
[<CompiledName "SortWith">]
val sortWith : comparer : ('T -> 'T -> int) -> queue : IQueue<'T> -> IQueue<'T>

/// <summary>
/// Sorts the elements with a comparer.
/// </summary>
/// <param name="comparer">The comparer.</param>
/// <param name="queue">The input queue.</param>
/// <returns>The sorted elements, in a queue of the implementation of the input.</returns>
/// <remarks>
/// O(n log n). This is a stable sort: elements that compare equal keep their order. The function wraps
/// <see cref="M:System.Linq.Enumerable.Order``1(System.Collections.Generic.IEnumerable{``0},System.Collections.Generic.IComparer{``0})"/>.
/// </remarks>
[<CompiledName "SortWithComparer">]
val sortWithComparer : comparer : System.Collections.Generic.IComparer<'T> -> queue : IQueue<'T> -> IQueue<'T>

// ---- Random ----

/// <summary>
/// Returns a random element, chosen with <see cref="P:System.Random.Shared"/>.
/// </summary>
/// <param name="source">The input queue.</param>
/// <returns>A random element.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the queue is empty.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "RandomChoice">]
val randomChoice : source : IQueue<'T> -> 'T

/// <summary>
/// Returns a random element, chosen with a function that returns numbers in [0, 1).
/// </summary>
/// <param name="randomizer">The function that returns a random number in [0, 1).</param>
/// <param name="source">The input queue.</param>
/// <returns>A random element.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the queue is empty.</exception>
/// <exception cref="T:System.ArgumentOutOfRangeException">Thrown when the randomizer returns a number outside [0, 1).</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "RandomChoiceBy">]
val randomChoiceBy : randomizer : (unit -> float) -> source : IQueue<'T> -> 'T

/// <summary>
/// Returns a random element, chosen with a given random number generator.
/// </summary>
/// <param name="random">The random number generator.</param>
/// <param name="source">The input queue.</param>
/// <returns>A random element.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="random"/> is <see langword="null"/>.</exception>
/// <exception cref="T:System.ArgumentException">Thrown when the queue is empty.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "RandomChoiceWith">]
val randomChoiceWith : random : Random -> source : IQueue<'T> -> 'T

/// <summary>
/// Returns random elements, chosen with <see cref="P:System.Random.Shared"/>; an element can repeat.
/// </summary>
/// <param name="count">The number of elements to choose.</param>
/// <param name="source">The input queue.</param>
/// <returns>The chosen elements, in a queue of the implementation of the input.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the count is negative, or positive for an empty queue.</exception>
/// <remarks>
/// O(n + count).
/// </remarks>
[<CompiledName "RandomChoices">]
val randomChoices : count : int -> source : IQueue<'T> -> IQueue<'T>

/// <summary>
/// Returns random elements, chosen with a function that returns numbers in [0, 1); an element can repeat.
/// </summary>
/// <param name="randomizer">The function that returns a random number in [0, 1).</param>
/// <param name="count">The number of elements to choose.</param>
/// <param name="source">The input queue.</param>
/// <returns>The chosen elements, in a queue of the implementation of the input.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the count is negative, or positive for an empty queue.</exception>
/// <exception cref="T:System.ArgumentOutOfRangeException">Thrown when the randomizer returns a number outside [0, 1).</exception>
/// <remarks>
/// O(n + count).
/// </remarks>
[<CompiledName "RandomChoicesBy">]
val randomChoicesBy : randomizer : (unit -> float) -> count : int -> source : IQueue<'T> -> IQueue<'T>

/// <summary>
/// Returns random elements, chosen with a given random number generator; an element can repeat.
/// </summary>
/// <param name="random">The random number generator.</param>
/// <param name="count">The number of elements to choose.</param>
/// <param name="source">The input queue.</param>
/// <returns>The chosen elements, in a queue of the implementation of the input.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="random"/> is <see langword="null"/>.</exception>
/// <exception cref="T:System.ArgumentException">Thrown when the count is negative, or positive for an empty queue.</exception>
/// <remarks>
/// O(n + count).
/// </remarks>
[<CompiledName "RandomChoicesWith">]
val randomChoicesWith : random : Random -> count : int -> source : IQueue<'T> -> IQueue<'T>

/// <summary>
/// Returns random elements at distinct indices, chosen with <see cref="P:System.Random.Shared"/>.
/// </summary>
/// <param name="count">The number of elements to choose.</param>
/// <param name="source">The input queue.</param>
/// <returns>The chosen elements, in a queue of the implementation of the input.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the count is negative or exceeds the length.</exception>
/// <remarks>
/// O(n + count).
/// </remarks>
[<CompiledName "RandomSample">]
val randomSample : count : int -> source : IQueue<'T> -> IQueue<'T>

/// <summary>
/// Returns random elements at distinct indices, chosen with a function that returns numbers in [0, 1).
/// </summary>
/// <param name="randomizer">The function that returns a random number in [0, 1).</param>
/// <param name="count">The number of elements to choose.</param>
/// <param name="source">The input queue.</param>
/// <returns>The chosen elements, in a queue of the implementation of the input.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the count is negative or exceeds the length.</exception>
/// <exception cref="T:System.ArgumentOutOfRangeException">Thrown when the randomizer returns a number outside [0, 1).</exception>
/// <remarks>
/// O(n + count).
/// </remarks>
[<CompiledName "RandomSampleBy">]
val randomSampleBy : randomizer : (unit -> float) -> count : int -> source : IQueue<'T> -> IQueue<'T>

/// <summary>
/// Returns random elements at distinct indices, chosen with a given random number generator.
/// </summary>
/// <param name="random">The random number generator.</param>
/// <param name="count">The number of elements to choose.</param>
/// <param name="source">The input queue.</param>
/// <returns>The chosen elements, in a queue of the implementation of the input.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="random"/> is <see langword="null"/>.</exception>
/// <exception cref="T:System.ArgumentException">Thrown when the count is negative or exceeds the length.</exception>
/// <remarks>
/// O(n + count).
/// </remarks>
[<CompiledName "RandomSampleWith">]
val randomSampleWith : random : Random -> count : int -> source : IQueue<'T> -> IQueue<'T>

/// <summary>
/// Returns the elements in a random order, chosen with <see cref="P:System.Random.Shared"/>.
/// </summary>
/// <param name="source">The input queue.</param>
/// <returns>The shuffled elements, in a queue of the implementation of the input.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "RandomShuffle">]
val randomShuffle : source : IQueue<'T> -> IQueue<'T>

/// <summary>
/// Returns the elements in a random order, chosen with a function that returns numbers in [0, 1).
/// </summary>
/// <param name="randomizer">The function that returns a random number in [0, 1).</param>
/// <param name="source">The input queue.</param>
/// <returns>The shuffled elements, in a queue of the implementation of the input.</returns>
/// <exception cref="T:System.ArgumentOutOfRangeException">Thrown when the randomizer returns a number outside [0, 1).</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "RandomShuffleBy">]
val randomShuffleBy : randomizer : (unit -> float) -> source : IQueue<'T> -> IQueue<'T>

/// <summary>
/// Returns the elements in a random order, chosen with a given random number generator.
/// </summary>
/// <param name="random">The random number generator.</param>
/// <param name="source">The input queue.</param>
/// <returns>The shuffled elements, in a queue of the implementation of the input.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="random"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "RandomShuffleWith">]
val randomShuffleWith : random : Random -> source : IQueue<'T> -> IQueue<'T>

// ---- Conversion ----

/// <summary>
/// Returns a new array with the elements, the first element of the queue first.
/// </summary>
/// <param name="queue">The input queue.</param>
/// <returns>A copy of the elements.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "ToArray">]
val toArray : queue : IQueue<'T> -> 'T array

/// <summary>
/// Returns a list of the elements, the first element of the queue first.
/// </summary>
/// <param name="queue">The input queue.</param>
/// <returns>The list of the elements.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "ToList">]
val toList : queue : IQueue<'T> -> 'T list

/// <summary>
/// Returns the queue as a sequence, which enumerates the elements in the order in which they leave the queue.
/// </summary>
/// <param name="queue">The input queue.</param>
/// <returns>The sequence of the elements.</returns>
/// <remarks>
/// O(1).
/// </remarks>
[<CompiledName "ToSeq">]
val toSeq : queue : IQueue<'T> -> seq<'T>

// ---- Type-specific ----

/// <summary>
/// Returns an empty queue of the implementation of the input.
/// </summary>
/// <param name="queue">The input queue.</param>
/// <returns>The empty queue.</returns>
/// <remarks>
/// O(1). The function wraps <see cref="M:System.Collections.Immutable.IImmutableQueue`1.Clear"/>, which keeps the
/// implementation of the queue and its settings.
/// </remarks>
[<CompiledName "Clear">]
val clear : queue : IQueue<'T> -> IQueue<'T>
