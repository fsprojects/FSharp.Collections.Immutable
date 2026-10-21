/// <summary>
/// Functions for the immutable stack <see cref="T:System.Collections.Immutable.ImmutableStack`1"/>.
/// </summary>
/// <remarks>
/// <para>
/// The module mirrors <see cref="T:Microsoft.FSharp.Collections.ListModule"/>: each function has the semantics, the
/// argument checks and the exceptions of the function with the same name in that module (ADR 0001), and its sorting
/// functions are stable. Where that module names a parameter <c>list</c>, <c>list1</c>, <c>list2</c>, <c>list3</c> or
/// <c>lists</c>, this module names it <c>stack</c>, <c>stack1</c>, <c>stack2</c>, <c>stack3</c> or <c>stacks</c>, and the
/// exceptions report these names. Functions that FSharp.Core lacks wrap a member of
/// <see cref="T:System.Collections.Immutable.ImmutableStack`1"/> or follow the function of that module that they resemble.
/// </para>
/// <para>
/// The order of a stack is the order in which it enumerates its elements, from the top to the bottom: the first element
/// is the top, which <see cref="M:FSharp.Collections.Immutable.Stack.Peek``1(System.Collections.Immutable.ImmutableStack{``0})"/>
/// returns, and the last element is the bottom. Every function treats a stack as a list in this order.
/// <see cref="M:FSharp.Collections.Immutable.Stack.OfSeq``1(System.Collections.Generic.IEnumerable{``0})"/> is the
/// inverse of <see cref="M:FSharp.Collections.Immutable.Stack.ToList``1(System.Collections.Immutable.ImmutableStack{``0})"/>:
/// it puts the first element of the sequence on top.
/// <see cref="M:FSharp.Collections.Immutable.Stack.Push``1(``0,System.Collections.Immutable.ImmutableStack{``0})"/>
/// pushes a value, which becomes the first element.
/// </para>
/// <para>
/// A stack reaches its elements from the top only. The functions that remove the first elements, such as
/// <see cref="M:FSharp.Collections.Immutable.Stack.Pop``1(System.Collections.Immutable.ImmutableStack{``0})"/> and
/// <see cref="M:FSharp.Collections.Immutable.Stack.Skip``1(System.Int32,System.Collections.Immutable.ImmutableStack{``0})"/>,
/// return the stack below them without a copy. The other transformations copy the elements to an array, compute the
/// result and push it onto a new stack.
/// </para>
/// <para>
/// Every function throws <see cref="T:System.ArgumentNullException"/> for a <see langword="null"/> stack, as
/// <see cref="T:Microsoft.FSharp.Collections.ArrayModule"/> does for a <see langword="null"/> array. A function that
/// takes a sequence throws it for a <see langword="null"/> sequence and for a default
/// <see cref="T:System.Collections.Immutable.ImmutableArray`1"/> that is given as a sequence, which would fail with
/// <see cref="T:System.InvalidOperationException"/> when the function enumerates it.
/// </para>
/// <para>
/// Functions that return an optional value return a <see cref="T:Microsoft.FSharp.Core.FSharpValueOption`1"/>, and functions
/// that return tuples return struct tuples. The namespaces <see cref="N:FSharp.Collections.Immutable.Option"/> and
/// <see cref="N:FSharp.Collections.Immutable.Tuple"/> contain a module with the same name whose functions use options
/// and reference tuples instead.
/// </para>
/// </remarks>
[<RequireQualifiedAccess>]
module FSharp.Collections.Immutable.Stack

open System
open System.Collections.Generic
open System.Collections.Immutable
// Opened after System.Collections.Generic, so that Stack names the abbreviation of this library, not the mutable stack
open FSharp.Collections.Immutable

// ---- Creation ----

/// <summary>
/// The empty stack.
/// </summary>
/// <remarks>
/// O(1). The value is <see cref="P:System.Collections.Immutable.ImmutableStack`1.Empty"/>.
/// </remarks>
[<GeneralizableValue>]
[<CompiledName "Empty">]
val empty<'T> : Stack<'T>

/// <summary>Creates a stack from a function of the index.</summary>
/// <param name="length">The length of the stack.</param>
/// <param name="initializer">The function that computes the element at each index, called in index order.</param>
/// <returns>The created stack, with the element at index 0 on top.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when <paramref name="length"/> is negative.</exception>
/// <remarks>O(n).</remarks>
[<CompiledName "Initialize">]
val init : length : int -> initializer : (int -> 'T) -> Stack<'T>

/// <summary>
/// Creates a stack from a copy of an array.
/// </summary>
/// <param name="array">The input array.</param>
/// <returns>The stack of the elements of the array, with the first element on top.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="array"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n). The function pushes the elements from the last one to the first one. Later changes to the array do not change the stack.
/// </remarks>
[<CompiledName "OfArray">]
val ofArray : array : 'T array -> Stack<'T>

/// <summary>
/// Creates a stack from a list.
/// </summary>
/// <param name="list">The input list.</param>
/// <returns>The stack of the elements of the list, with the first element on top.</returns>
/// <remarks>
/// O(n). The function pushes the elements from the last one to the first one.
/// </remarks>
[<CompiledName "OfList">]
val ofList : list : 'T list -> Stack<'T>

/// <summary>
/// Creates a stack from a copy of a read-only span.
/// </summary>
/// <param name="span">The input span.</param>
/// <returns>The stack of the elements of the span, with the first element on top.</returns>
/// <remarks>
/// O(n). The function pushes the elements from the last one to the first one. It does not wrap
/// <see cref="M:System.Collections.Immutable.ImmutableStack.Create``1(System.ReadOnlySpan{``0})"/>, which leaves the
/// last element on top.
/// </remarks>
[<CompiledName "OfReadOnlySpan">]
val ofReadOnlySpan : span : ReadOnlySpan<'T> -> Stack<'T>

/// <summary>
/// Creates a stack from a sequence.
/// </summary>
/// <param name="source">The input sequence.</param>
/// <returns>The stack of the elements of the sequence, with the first element on top.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="source"/> is <see langword="null"/> or a default flat list.</exception>
/// <remarks>
/// O(n). The function is the inverse of
/// <see cref="M:FSharp.Collections.Immutable.Stack.ToList``1(System.Collections.Immutable.ImmutableStack{``0})"/>: it
/// pushes the elements from the last one to the first one, while
/// <see cref="M:System.Collections.Immutable.ImmutableStack.CreateRange``1(System.Collections.Generic.IEnumerable{``0})"/>
/// leaves the last element on top. A stack is returned itself, without a copy.
/// </remarks>
[<CompiledName "OfSeq">]
val ofSeq : source : seq<'T> -> Stack<'T>

/// <summary>
/// Creates a stack from a copy of a span.
/// </summary>
/// <param name="span">The input span.</param>
/// <returns>The stack of the elements of the span, with the first element on top.</returns>
/// <remarks>
/// O(n). The function pushes the elements from the last one to the first one. It does not wrap
/// <see cref="M:System.Collections.Immutable.ImmutableStack.Create``1(System.ReadOnlySpan{``0})"/>, which leaves the
/// last element on top.
/// </remarks>
[<CompiledName "OfSpan">]
val ofSpan : span : Span<'T> -> Stack<'T>

/// <summary>Creates a stack whose elements are all the given value.</summary>
/// <param name="count">The length of the stack.</param>
/// <param name="initial">The value of every element.</param>
/// <returns>The created stack.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when <paramref name="count"/> is negative.</exception>
/// <remarks>O(n).</remarks>
[<CompiledName "Replicate">]
val replicate : count : int -> initial : 'T -> Stack<'T>

/// <summary>Creates a stack of one element.</summary>
/// <param name="value">The element.</param>
/// <returns>The stack of one element.</returns>
/// <remarks>O(1).</remarks>
[<CompiledName "Singleton">]
val singleton : value : 'T -> Stack<'T>

/// <summary>
/// Creates a stack from a generator, which returns the next element and the next state.
/// </summary>
/// <param name="generator">The function that returns the next element and state, or <c>ValueNone</c> to stop.</param>
/// <param name="state">The initial state.</param>
/// <returns>The generated stack, with the first generated element on top.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Unfold">]
val unfold : generator : ('State -> struct ('T * 'State) voption) -> state : 'State -> Stack<'T>

// ---- Properties ----

/// <summary>
/// Returns whether the stack has no elements.
/// </summary>
/// <param name="stack">The input stack.</param>
/// <returns><see langword="true"/> when the stack is empty.</returns>
/// <remarks>
/// O(1). The function wraps <see cref="P:System.Collections.Immutable.ImmutableStack`1.IsEmpty"/>.
/// </remarks>
[<CompiledName "IsEmpty">]
val isEmpty : stack : Stack<'T> -> bool

/// <summary>
/// Returns the number of elements of the stack.
/// </summary>
/// <param name="stack">The input stack.</param>
/// <returns>The length of the stack.</returns>
/// <remarks>
/// O(n): a stack does not store its length, so the function counts the elements.
/// </remarks>
[<CompiledName "Length">]
val length : stack : Stack<'T> -> int

// ---- Access ----

/// <summary>Returns the only element of the stack.</summary>
/// <param name="stack">The input stack.</param>
/// <returns>The only element.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the stack does not have exactly one element.</exception>
/// <remarks>O(1).</remarks>
[<CompiledName "ExactlyOne">]
val exactlyOne : stack : Stack<'T> -> 'T

/// <summary>
/// Returns the first element of the stack, the element on top.
/// </summary>
/// <param name="stack">The input stack.</param>
/// <returns>The first element.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the stack is empty.</exception>
/// <remarks>
/// O(1). The function wraps <see cref="M:System.Collections.Immutable.ImmutableStack`1.Peek"/>.
/// </remarks>
[<CompiledName "Peek">]
val head : stack : Stack<'T> -> 'T

/// <summary>
/// Returns the last element of the stack, the element at the bottom.
/// </summary>
/// <param name="stack">The input stack.</param>
/// <returns>The last element.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the stack is empty.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Last">]
val last : stack : Stack<'T> -> 'T

/// <summary>
/// Returns the stack without its first element: the stack below the top.
/// </summary>
/// <param name="stack">The input stack.</param>
/// <returns>The elements after the first one.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the stack is empty.</exception>
/// <remarks>
/// O(1). The result is the stack that the top was pushed onto, without a copy. The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableStack`1.Pop"/>.
/// </remarks>
[<CompiledName "Pop">]
val tail : stack : Stack<'T> -> Stack<'T>

/// <summary>
/// Returns the only element of the stack, or <c>ValueNone</c> when it does not have exactly one element.
/// </summary>
/// <param name="stack">The input stack.</param>
/// <returns>The only element, or <c>ValueNone</c>.</returns>
/// <remarks>
/// O(1).
/// </remarks>
[<CompiledName "TryExactlyOne">]
val tryExactlyOne : stack : Stack<'T> -> 'T voption

/// <summary>
/// Returns the first element of the stack, or <c>ValueNone</c> when it is empty.
/// </summary>
/// <param name="stack">The input stack.</param>
/// <returns>The first element, or <c>ValueNone</c>.</returns>
/// <remarks>
/// O(1).
/// </remarks>
[<CompiledName "TryPeek">]
val tryHead : stack : Stack<'T> -> 'T voption

/// <summary>
/// Returns the last element of the stack, or <c>ValueNone</c> when it is empty.
/// </summary>
/// <param name="stack">The input stack.</param>
/// <returns>The last element, or <c>ValueNone</c>.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "TryLast">]
val tryLast : stack : Stack<'T> -> 'T voption

/// <summary>
/// Returns the stack without its first element, or <c>ValueNone</c> when it is empty.
/// </summary>
/// <param name="stack">The input stack.</param>
/// <returns>The elements after the first one, or <c>ValueNone</c>.</returns>
/// <remarks>
/// O(1). The result is the stack that the top was pushed onto, without a copy.
/// </remarks>
[<CompiledName "TryPop">]
val tryTail : stack : Stack<'T> -> Stack<'T> voption

/// <summary>
/// Returns the first element and the other elements, or <c>ValueNone</c> when the stack is empty.
/// </summary>
/// <param name="stack">The input stack.</param>
/// <returns>The first element and the elements after it, or <c>ValueNone</c>.</returns>
/// <remarks>
/// O(1).
/// </remarks>
[<CompiledName "TryPeekPop">]
val tryUncons : stack : Stack<'T> -> struct ('T * Stack<'T>) voption

/// <summary>
/// Returns the first element and the other elements.
/// </summary>
/// <param name="stack">The input stack.</param>
/// <returns>The first element and the elements after it.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the stack is empty.</exception>
/// <remarks>
/// O(1). The function throws what
/// <see cref="M:FSharp.Collections.Immutable.Stack.Peek``1(System.Collections.Immutable.ImmutableStack{``0})"/> throws.
/// </remarks>
[<CompiledName "PeekPop">]
val uncons : stack : Stack<'T> -> struct ('T * Stack<'T>)

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
val (|Cons|_|) : stack : Stack<'T> -> struct ('T * Stack<'T>) voption

/// <summary>Matches an empty stack.</summary>
/// <param name="stack">The input stack.</param>
/// <returns><see langword="true"/> when the stack is empty.</returns>
/// <remarks>O(1).</remarks>
[<CompiledName "NilPattern">]
val (|Nil|_|) : stack : Stack<'T> -> bool

// ---- Search ----

/// <summary>
/// Returns whether the stack contains an element equal to a value, in F# structural equality.
/// </summary>
/// <param name="value">The value to find.</param>
/// <param name="source">The input stack.</param>
/// <returns><see langword="true"/> when an element equals the value.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Contains">]
val contains : value : 'T -> source : Stack<'T> -> bool when 'T : equality

/// <summary>Returns whether any element satisfies the predicate.</summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="stack">The input stack.</param>
/// <returns><see langword="true"/> when an element satisfies the predicate.</returns>
/// <remarks>O(n).</remarks>
[<CompiledName "Exists">]
val exists : predicate : ('T -> bool) -> stack : Stack<'T> -> bool

/// <summary>
/// Returns whether any pair of elements at the same index satisfies the predicate.
/// </summary>
/// <param name="predicate">The function to test the pairs.</param>
/// <param name="stack1">The first stack.</param>
/// <param name="stack2">The second stack.</param>
/// <returns><see langword="true"/> when a pair satisfies the predicate.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when no pair satisfies the predicate and the stacks have different lengths.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Exists2">]
val exists2 : predicate : ('T1 -> 'T2 -> bool) -> stack1 : Stack<'T1> -> stack2 : Stack<'T2> -> bool

/// <summary>Returns the first element that satisfies the predicate.</summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="stack">The input stack.</param>
/// <returns>The first element that satisfies the predicate.</returns>
/// <exception cref="T:System.Collections.Generic.KeyNotFoundException">Thrown when no element satisfies the predicate.</exception>
/// <remarks>O(n).</remarks>
[<CompiledName "Find">]
val find : predicate : ('T -> bool) -> stack : Stack<'T> -> 'T

/// <summary>Returns the last element that satisfies the predicate.</summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="stack">The input stack.</param>
/// <returns>The last element that satisfies the predicate.</returns>
/// <exception cref="T:System.Collections.Generic.KeyNotFoundException">Thrown when no element satisfies the predicate.</exception>
/// <remarks>O(n).</remarks>
[<CompiledName "FindBack">]
val findBack : predicate : ('T -> bool) -> stack : Stack<'T> -> 'T

/// <summary>
/// Returns the index of the first element that satisfies the predicate.
/// </summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="stack">The input stack.</param>
/// <returns>The index of the first element that satisfies the predicate; the top has the index 0.</returns>
/// <exception cref="T:System.Collections.Generic.KeyNotFoundException">Thrown when no element satisfies the predicate.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "FindIndex">]
val findIndex : predicate : ('T -> bool) -> stack : Stack<'T> -> int

/// <summary>
/// Returns the index of the last element that satisfies the predicate.
/// </summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="stack">The input stack.</param>
/// <returns>The index of the last element that satisfies the predicate; the top has the index 0.</returns>
/// <exception cref="T:System.Collections.Generic.KeyNotFoundException">Thrown when no element satisfies the predicate.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "FindIndexBack">]
val findIndexBack : predicate : ('T -> bool) -> stack : Stack<'T> -> int

/// <summary>Returns whether all elements satisfy the predicate.</summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="stack">The input stack.</param>
/// <returns><see langword="true"/> when every element satisfies the predicate.</returns>
/// <remarks>O(n).</remarks>
[<CompiledName "ForAll">]
val forall : predicate : ('T -> bool) -> stack : Stack<'T> -> bool

/// <summary>
/// Returns whether all pairs of elements at the same index satisfy the predicate.
/// </summary>
/// <param name="predicate">The function to test the pairs.</param>
/// <param name="stack1">The first stack.</param>
/// <param name="stack2">The second stack.</param>
/// <returns><see langword="true"/> when every pair satisfies the predicate.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when every pair satisfies the predicate and the stacks have different lengths.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "ForAll2">]
val forall2 : predicate : ('T1 -> 'T2 -> bool) -> stack1 : Stack<'T1> -> stack2 : Stack<'T2> -> bool

/// <summary>
/// Returns the first value that the chooser returns for the elements.
/// </summary>
/// <param name="chooser">The function that returns a value, or <c>ValueNone</c> to go on.</param>
/// <param name="stack">The input stack.</param>
/// <returns>The first chosen value.</returns>
/// <exception cref="T:System.Collections.Generic.KeyNotFoundException">Thrown when the chooser returns <c>ValueNone</c> for every element.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Pick">]
val pick : chooser : ('T -> 'U voption) -> stack : Stack<'T> -> 'U

/// <summary>
/// Returns the first element that satisfies the predicate, or <c>ValueNone</c>.
/// </summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="stack">The input stack.</param>
/// <returns>The first element that satisfies the predicate, or <c>ValueNone</c>.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "TryFind">]
val tryFind : predicate : ('T -> bool) -> stack : Stack<'T> -> 'T voption

/// <summary>
/// Returns the last element that satisfies the predicate, or <c>ValueNone</c>.
/// </summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="stack">The input stack.</param>
/// <returns>The last element that satisfies the predicate, or <c>ValueNone</c>.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "TryFindBack">]
val tryFindBack : predicate : ('T -> bool) -> stack : Stack<'T> -> 'T voption

/// <summary>
/// Returns the index of the first element that satisfies the predicate, or <c>ValueNone</c>.
/// </summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="stack">The input stack.</param>
/// <returns>The index of the first element that satisfies the predicate, or <c>ValueNone</c>.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "TryFindIndex">]
val tryFindIndex : predicate : ('T -> bool) -> stack : Stack<'T> -> int voption

/// <summary>
/// Returns the index of the last element that satisfies the predicate, or <c>ValueNone</c>.
/// </summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="stack">The input stack.</param>
/// <returns>The index of the last element that satisfies the predicate, or <c>ValueNone</c>.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "TryFindIndexBack">]
val tryFindIndexBack : predicate : ('T -> bool) -> stack : Stack<'T> -> int voption

/// <summary>
/// Returns the first value that the chooser returns for the elements, or <c>ValueNone</c>.
/// </summary>
/// <param name="chooser">The function that returns a value, or <c>ValueNone</c> to go on.</param>
/// <param name="stack">The input stack.</param>
/// <returns>The first chosen value, or <c>ValueNone</c>.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "TryPick">]
val tryPick : chooser : ('T -> 'U voption) -> stack : Stack<'T> -> 'U voption

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
val allPairs : stack1 : Stack<'T1> -> stack2 : Stack<'T2> -> Stack<struct ('T1 * 'T2)>

/// <summary>
/// Returns a stack of the elements of the first stack followed by the elements of the second one.
/// </summary>
/// <param name="stack1">The first stack, whose elements come first, on top.</param>
/// <param name="stack2">The second stack, whose elements come last.</param>
/// <returns>The concatenation.</returns>
/// <remarks>
/// O(n), where n is the length of the first stack. The function pushes the elements of the first stack onto the second
/// one, from the last element to the first one, so the result shares the nodes of the second stack. When one of the
/// stacks is empty, the function returns the other one.
/// </remarks>
[<CompiledName "Append">]
val append : stack1 : Stack<'T> -> stack2 : Stack<'T> -> Stack<'T>

/// <summary>
/// Returns the values that the chooser returns for the elements.
/// </summary>
/// <param name="chooser">The function that returns a value, or <c>ValueNone</c> to skip the element.</param>
/// <param name="stack">The input stack.</param>
/// <returns>The chosen values, in the order of their elements.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Choose">]
val choose : chooser : ('T -> 'U voption) -> stack : Stack<'T> -> Stack<'U>

/// <summary>Divides the stack into chunks of a given size.</summary>
/// <param name="chunkSize">The maximum size of a chunk.</param>
/// <param name="stack">The input stack.</param>
/// <returns>The chunks; the last one can be smaller.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when <paramref name="chunkSize"/> is not positive.</exception>
/// <remarks>O(n).</remarks>
[<CompiledName "ChunkBySize">]
val chunkBySize : chunkSize : int -> stack : Stack<'T> -> Stack<Stack<'T>>

/// <summary>
/// Concatenates the stacks that a function returns for the elements.
/// </summary>
/// <param name="mapping">The function that returns a stack for each element.</param>
/// <param name="stack">The input stack.</param>
/// <returns>The concatenation of the returned stacks.</returns>
/// <remarks>
/// O(n + the total length of the returned stacks).
/// </remarks>
[<CompiledName "Collect">]
val collect : mapping : ('T -> Stack<'U>) -> stack : Stack<'T> -> Stack<'U>

/// <summary>Concatenates a sequence of stacks.</summary>
/// <param name="stacks">The stacks.</param>
/// <returns>The concatenation, with the elements of the first stack on top.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="stacks"/> is <see langword="null"/> or a default flat list, or one of its stacks is <see langword="null"/>.</exception>
/// <remarks>O(the total length).</remarks>
[<CompiledName "Concat">]
val concat : stacks : seq<Stack<'T>> -> Stack<'T>

/// <summary>
/// Returns a stack with a value added before the first element: the value pushed on top.
/// </summary>
/// <param name="value">The value to add.</param>
/// <param name="stack">The input stack.</param>
/// <returns>A stack of the value and the elements.</returns>
/// <remarks>
/// O(1). The result shares the nodes of the input. The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableStack`1.Push(`0)"/>.
/// </remarks>
[<CompiledName "Push">]
val cons : value : 'T -> stack : Stack<'T> -> Stack<'T>

/// <summary>
/// Returns a stack with values added before the first element, in their order: the first value ends on top.
/// </summary>
/// <param name="values">The values to add, in the order in which the result enumerates them.</param>
/// <param name="stack">The input stack.</param>
/// <returns>A stack of the values followed by the elements of the input.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="values"/> is <see langword="null"/> or a default flat list.</exception>
/// <remarks>
/// O(m), where m is the number of values. The function pushes the values from the last one to the first one, so the
/// result shares the nodes of the input.
/// </remarks>
[<CompiledName "PushMany">]
val consMany : values : seq<'T> -> stack : Stack<'T> -> Stack<'T>

/// <summary>Returns the elements that satisfy the predicate.</summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="stack">The input stack.</param>
/// <returns>The elements that satisfy the predicate, in their order.</returns>
/// <remarks>O(n).</remarks>
[<CompiledName "Filter">]
val filter : predicate : ('T -> bool) -> stack : Stack<'T> -> Stack<'T>

/// <summary>Returns every element with its index.</summary>
/// <param name="stack">The input stack.</param>
/// <returns>The pairs of index and element; the top has the index 0.</returns>
/// <remarks>O(n).</remarks>
[<CompiledName "Indexed">]
val indexed : stack : Stack<'T> -> Stack<struct (int * 'T)>

/// <summary>Returns the results of a function applied to each element.</summary>
/// <param name="mapping">The function to transform the elements, called in the order of the elements.</param>
/// <param name="stack">The input stack.</param>
/// <returns>The results, in the order of the elements.</returns>
/// <remarks>O(n).</remarks>
[<CompiledName "Map">]
val map : mapping : ('T -> 'U) -> stack : Stack<'T> -> Stack<'U>

/// <summary>
/// Returns the results of a function applied to the pairs of elements at the same index.
/// </summary>
/// <param name="mapping">The function to transform the pairs.</param>
/// <param name="stack1">The first stack.</param>
/// <param name="stack2">The second stack.</param>
/// <returns>The results.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the stacks have different lengths.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Map2">]
val map2 : mapping : ('T1 -> 'T2 -> 'U) -> stack1 : Stack<'T1> -> stack2 : Stack<'T2> -> Stack<'U>

/// <summary>
/// Returns the results of a function applied to the triples of elements at the same index.
/// </summary>
/// <param name="mapping">The function to transform the triples.</param>
/// <param name="stack1">The first stack.</param>
/// <param name="stack2">The second stack.</param>
/// <param name="stack3">The third stack.</param>
/// <returns>The results.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the stacks have different lengths.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Map3">]
val map3 : mapping : ('T1 -> 'T2 -> 'T3 -> 'U) -> stack1 : Stack<'T1> -> stack2 : Stack<'T2> -> stack3 : Stack<'T3> -> Stack<'U>

/// <summary>
/// Returns the results of a function applied to each element and its index.
/// </summary>
/// <param name="mapping">The function to transform the elements.</param>
/// <param name="stack">The input stack.</param>
/// <returns>The results.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "MapIndexed">]
val mapi : mapping : (int -> 'T -> 'U) -> stack : Stack<'T> -> Stack<'U>

/// <summary>
/// Returns the results of a function applied to the index and the pair of elements at that index.
/// </summary>
/// <param name="mapping">The function to transform the pairs.</param>
/// <param name="stack1">The first stack.</param>
/// <param name="stack2">The second stack.</param>
/// <returns>The results.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the stacks have different lengths.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "MapIndexed2">]
val mapi2 : mapping : (int -> 'T1 -> 'T2 -> 'U) -> stack1 : Stack<'T1> -> stack2 : Stack<'T2> -> Stack<'U>

/// <summary>Returns every element with its successor.</summary>
/// <param name="stack">The input stack.</param>
/// <returns>The pairs of neighbours; empty for fewer than two elements.</returns>
/// <remarks>O(n).</remarks>
[<CompiledName "Pairwise">]
val pairwise : stack : Stack<'T> -> Stack<struct ('T * 'T)>

/// <summary>
/// Returns the elements moved to the indices that a function computes.
/// </summary>
/// <param name="indexMap">The function that maps the index of each element to its new index.</param>
/// <param name="stack">The input stack.</param>
/// <returns>The permuted stack.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the function does not compute a permutation.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Permute">]
val permute : indexMap : (int -> int) -> stack : Stack<'T> -> Stack<'T>

/// <summary>
/// Returns the elements in reverse order.
/// </summary>
/// <param name="stack">The input stack.</param>
/// <returns>The reversed stack, whose first element is the last element of the input.</returns>
/// <remarks>
/// O(n). The function pushes the elements onto an empty stack in their order.
/// </remarks>
[<CompiledName "Reverse">]
val rev : stack : Stack<'T> -> Stack<'T>

/// <summary>
/// Returns the transpose of a sequence of stacks of the same length.
/// </summary>
/// <param name="stacks">The stacks.</param>
/// <returns>A stack whose element i holds the element i of every input stack.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="stacks"/> is <see langword="null"/> or a default flat list, or one of its stacks is <see langword="null"/>.</exception>
/// <exception cref="T:System.ArgumentException">Thrown when the stacks have different lengths.</exception>
/// <remarks>
/// O(the total length).
/// </remarks>
[<CompiledName "Transpose">]
val transpose : stacks : seq<Stack<'T>> -> Stack<Stack<'T>>

/// <summary>Splits a stack of pairs into two stacks.</summary>
/// <param name="stack">The input stack.</param>
/// <returns>The first components and the second components.</returns>
/// <remarks>O(n).</remarks>
[<CompiledName "Unzip">]
val unzip : stack : Stack<struct ('T1 * 'T2)> -> struct (Stack<'T1> * Stack<'T2>)

/// <summary>Splits a stack of triples into three stacks.</summary>
/// <param name="stack">The input stack.</param>
/// <returns>The first, the second and the third components.</returns>
/// <remarks>O(n).</remarks>
[<CompiledName "Unzip3">]
val unzip3 : stack : Stack<struct ('T1 * 'T2 * 'T3)> -> struct (Stack<'T1> * Stack<'T2> * Stack<'T3>)

/// <summary>
/// Returns the elements that satisfy the predicate.
/// </summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="stack">The input stack.</param>
/// <returns>The elements that satisfy the predicate, in their order.</returns>
/// <remarks>
/// O(n). A synonym of <see cref="M:FSharp.Collections.Immutable.Stack.Filter``1(Microsoft.FSharp.Core.FSharpFunc{``0,System.Boolean},System.Collections.Immutable.ImmutableStack{``0})"/>.
/// </remarks>
[<CompiledName "Where">]
val where : predicate : ('T -> bool) -> stack : Stack<'T> -> Stack<'T>

/// <summary>
/// Returns the windows of a given size that slide over the stack.
/// </summary>
/// <param name="windowSize">The size of a window.</param>
/// <param name="stack">The input stack.</param>
/// <returns>The windows; empty when the stack is shorter than a window.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when <paramref name="windowSize"/> is not positive.</exception>
/// <remarks>
/// O(n × windowSize).
/// </remarks>
[<CompiledName "Windowed">]
val windowed : windowSize : int -> stack : Stack<'T> -> Stack<Stack<'T>>

/// <summary>Combines two stacks into a stack of pairs.</summary>
/// <param name="stack1">The first stack.</param>
/// <param name="stack2">The second stack.</param>
/// <returns>The pairs of elements at the same index.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the stacks have different lengths.</exception>
/// <remarks>O(n).</remarks>
[<CompiledName "Zip">]
val zip : stack1 : Stack<'T1> -> stack2 : Stack<'T2> -> Stack<struct ('T1 * 'T2)>

/// <summary>Combines three stacks into a stack of triples.</summary>
/// <param name="stack1">The first stack.</param>
/// <param name="stack2">The second stack.</param>
/// <param name="stack3">The third stack.</param>
/// <returns>The triples of elements at the same index.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the stacks have different lengths.</exception>
/// <remarks>O(n).</remarks>
[<CompiledName "Zip3">]
val zip3 : stack1 : Stack<'T1> -> stack2 : Stack<'T2> -> stack3 : Stack<'T3> -> Stack<struct ('T1 * 'T2 * 'T3)>

// ---- Folding ----

/// <summary>Returns the average of the elements.</summary>
/// <param name="stack">The input stack.</param>
/// <returns>The average.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the stack is empty.</exception>
/// <remarks>O(n).</remarks>
[<CompiledName "Average">]
val inline average :
    stack : Stack< ^T > -> ^T
        when ^T : (static member (+) : ^T * ^T -> ^T)
        and ^T : (static member DivideByInt : ^T * int -> ^T)
        and ^T : (static member Zero : ^T)

/// <summary>
/// Returns the average of the results of a function applied to each element.
/// </summary>
/// <param name="projection">The function to transform the elements.</param>
/// <param name="stack">The input stack.</param>
/// <returns>The average of the results.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the stack is empty.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "AverageBy">]
val inline averageBy :
    projection : ('T -> ^U) -> stack : Stack<'T> -> ^U
        when ^U : (static member (+) : ^U * ^U -> ^U)
        and ^U : (static member DivideByInt : ^U * int -> ^U)
        and ^U : (static member Zero : ^U)

/// <summary>Compares two stacks element by element with a comparer.</summary>
/// <param name="comparer">The function that compares two elements.</param>
/// <param name="stack1">The first stack.</param>
/// <param name="stack2">The second stack.</param>
/// <returns>
/// The first non-zero result of the comparer, or the comparison of the lengths when every common element compares equal.
/// </returns>
/// <remarks>O(n).</remarks>
[<CompiledName "CompareWith">]
val compareWith : comparer : ('T -> 'T -> int) -> stack1 : Stack<'T> -> stack2 : Stack<'T> -> int

/// <summary>
/// Applies a function to a state and each element, from the first element to the last one.
/// </summary>
/// <param name="folder">The function that computes the next state.</param>
/// <param name="state">The initial state.</param>
/// <param name="stack">The input stack.</param>
/// <returns>The final state.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Fold">]
val fold : folder : ('State -> 'T -> 'State) -> state : 'State -> stack : Stack<'T> -> 'State

/// <summary>
/// Applies a function to a state and each pair of elements at the same index, from the first index to the last one.
/// </summary>
/// <param name="folder">The function that computes the next state.</param>
/// <param name="state">The initial state.</param>
/// <param name="stack1">The first stack.</param>
/// <param name="stack2">The second stack.</param>
/// <returns>The final state.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the stacks have different lengths.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Fold2">]
val fold2 : folder : ('State -> 'T1 -> 'T2 -> 'State) -> state : 'State -> stack1 : Stack<'T1> -> stack2 : Stack<'T2> -> 'State

/// <summary>
/// Applies a function to each element and a state, from the last element to the first one.
/// </summary>
/// <param name="folder">The function that computes the next state.</param>
/// <param name="stack">The input stack.</param>
/// <param name="state">The initial state.</param>
/// <returns>The final state.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "FoldBack">]
val foldBack : folder : ('T -> 'State -> 'State) -> stack : Stack<'T> -> state : 'State -> 'State

/// <summary>
/// Applies a function to each pair of elements at the same index and a state, from the last index to the first one.
/// </summary>
/// <param name="folder">The function that computes the next state.</param>
/// <param name="stack1">The first stack.</param>
/// <param name="stack2">The second stack.</param>
/// <param name="state">The initial state.</param>
/// <returns>The final state.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the stacks have different lengths.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "FoldBack2">]
val foldBack2 :
    folder : ('T1 -> 'T2 -> 'State -> 'State) -> stack1 : Stack<'T1> -> stack2 : Stack<'T2> -> state : 'State -> 'State

/// <summary>Applies an action to each element.</summary>
/// <param name="action">The action.</param>
/// <param name="stack">The input stack.</param>
/// <remarks>O(n).</remarks>
[<CompiledName "Iterate">]
val iter : action : ('T -> unit) -> stack : Stack<'T> -> unit

/// <summary>
/// Applies an action to each pair of elements at the same index.
/// </summary>
/// <param name="action">The action.</param>
/// <param name="stack1">The first stack.</param>
/// <param name="stack2">The second stack.</param>
/// <exception cref="T:System.ArgumentException">Thrown when the stacks have different lengths.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Iterate2">]
val iter2 : action : ('T1 -> 'T2 -> unit) -> stack1 : Stack<'T1> -> stack2 : Stack<'T2> -> unit

/// <summary>Applies an action to each element and its index.</summary>
/// <param name="action">The action.</param>
/// <param name="stack">The input stack.</param>
/// <remarks>O(n).</remarks>
[<CompiledName "IterateIndexed">]
val iteri : action : (int -> 'T -> unit) -> stack : Stack<'T> -> unit

/// <summary>
/// Applies an action to the index and the pair of elements at that index.
/// </summary>
/// <param name="action">The action.</param>
/// <param name="stack1">The first stack.</param>
/// <param name="stack2">The second stack.</param>
/// <exception cref="T:System.ArgumentException">Thrown when the stacks have different lengths.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "IterateIndexed2">]
val iteri2 : action : (int -> 'T1 -> 'T2 -> unit) -> stack1 : Stack<'T1> -> stack2 : Stack<'T2> -> unit

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
val mapFold :
    mapping : ('State -> 'T -> struct ('Result * 'State)) ->
    state : 'State ->
    stack : Stack<'T> ->
        struct (Stack<'Result> * 'State)

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
val mapFoldBack :
    mapping : ('T -> 'State -> struct ('Result * 'State)) ->
    stack : Stack<'T> ->
    state : 'State ->
        struct (Stack<'Result> * 'State)

/// <summary>Returns the greatest element, in F# generic comparison.</summary>
/// <param name="stack">The input stack.</param>
/// <returns>The greatest element.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the stack is empty.</exception>
/// <remarks>O(n).</remarks>
[<CompiledName "Max">]
val max : stack : Stack<'T> -> 'T when 'T : comparison

/// <summary>
/// Returns the element with the greatest result of a projection.
/// </summary>
/// <param name="projection">The function to transform the elements for the comparison.</param>
/// <param name="stack">The input stack.</param>
/// <returns>The first element with the greatest result.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the stack is empty.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "MaxBy">]
val maxBy : projection : ('T -> 'U) -> stack : Stack<'T> -> 'T when 'U : comparison

/// <summary>Returns the smallest element, in F# generic comparison.</summary>
/// <param name="stack">The input stack.</param>
/// <returns>The smallest element.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the stack is empty.</exception>
/// <remarks>O(n).</remarks>
[<CompiledName "Min">]
val min : stack : Stack<'T> -> 'T when 'T : comparison

/// <summary>
/// Returns the element with the smallest result of a projection.
/// </summary>
/// <param name="projection">The function to transform the elements for the comparison.</param>
/// <param name="stack">The input stack.</param>
/// <returns>The first element with the smallest result.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the stack is empty.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "MinBy">]
val minBy : projection : ('T -> 'U) -> stack : Stack<'T> -> 'T when 'U : comparison

/// <summary>
/// Combines the elements with a function, from the first element to the last one.
/// </summary>
/// <param name="reduction">The function that combines the result so far with the next element.</param>
/// <param name="stack">The input stack.</param>
/// <returns>The combined value.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the stack is empty.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Reduce">]
val reduce : reduction : ('T -> 'T -> 'T) -> stack : Stack<'T> -> 'T

/// <summary>
/// Combines the elements with a function, from the last element to the first one.
/// </summary>
/// <param name="reduction">The function that combines an element with the result so far.</param>
/// <param name="stack">The input stack.</param>
/// <returns>The combined value.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the stack is empty.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "ReduceBack">]
val reduceBack : reduction : ('T -> 'T -> 'T) -> stack : Stack<'T> -> 'T

/// <summary>
/// Returns every state of a fold from the first element to the last one, the initial state first.
/// </summary>
/// <param name="folder">The function that computes the next state.</param>
/// <param name="state">The initial state.</param>
/// <param name="stack">The input stack.</param>
/// <returns>The states; one more than the elements.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Scan">]
val scan : folder : ('State -> 'T -> 'State) -> state : 'State -> stack : Stack<'T> -> Stack<'State>

/// <summary>
/// Returns every state of a fold from the last element to the first one, the initial state last.
/// </summary>
/// <param name="folder">The function that computes the next state.</param>
/// <param name="stack">The input stack.</param>
/// <param name="state">The initial state.</param>
/// <returns>The states; one more than the elements.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "ScanBack">]
val scanBack : folder : ('T -> 'State -> 'State) -> stack : Stack<'T> -> state : 'State -> Stack<'State>

/// <summary>Returns the sum of the elements.</summary>
/// <param name="stack">The input stack.</param>
/// <returns>The sum; zero for an empty stack.</returns>
/// <remarks>O(n).</remarks>
[<CompiledName "Sum">]
val inline sum : stack : Stack< ^T > -> ^T when ^T : (static member (+) : ^T * ^T -> ^T) and ^T : (static member Zero : ^T)

/// <summary>
/// Returns the sum of the results of a function applied to each element.
/// </summary>
/// <param name="projection">The function to transform the elements.</param>
/// <param name="stack">The input stack.</param>
/// <returns>The sum of the results; zero for an empty stack.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "SumBy">]
val inline sumBy :
    projection : ('T -> ^U) -> stack : Stack<'T> -> ^U
        when ^U : (static member (+) : ^U * ^U -> ^U) and ^U : (static member Zero : ^U)

// ---- Partitioning and grouping ----

/// <summary>Counts the elements of each key of a projection.</summary>
/// <param name="projection">The function that returns the key of an element.</param>
/// <param name="stack">The input stack.</param>
/// <returns>The keys, in the order of their first occurrence, with the number of their elements.</returns>
/// <remarks>O(n). The keys compare in F# structural equality.</remarks>
[<CompiledName "CountBy">]
val countBy : projection : ('T -> 'Key) -> stack : Stack<'T> -> Stack<struct ('Key * int)> when 'Key : equality

/// <summary>
/// Returns the elements without duplicates, in F# structural equality.
/// </summary>
/// <param name="stack">The input stack.</param>
/// <returns>The first occurrence of each element, in their order.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Distinct">]
val distinct : stack : Stack<'T> -> Stack<'T> when 'T : equality

/// <summary>
/// Returns the elements without duplicates of the result of a projection.
/// </summary>
/// <param name="projection">The function that returns the key of an element.</param>
/// <param name="stack">The input stack.</param>
/// <returns>The first element of each key, in their order.</returns>
/// <remarks>
/// O(n). The keys compare in F# structural equality.
/// </remarks>
[<CompiledName "DistinctBy">]
val distinctBy : projection : ('T -> 'Key) -> stack : Stack<'T> -> Stack<'T> when 'Key : equality

/// <summary>
/// Returns the distinct elements that do not equal any of the excluded items.
/// </summary>
/// <param name="itemsToExclude">The items to remove.</param>
/// <param name="stack">The input stack.</param>
/// <returns>The distinct remaining elements, in their order.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="itemsToExclude"/> is <see langword="null"/> or a default flat list.</exception>
/// <remarks>
/// O(n + m). An empty stack is returned without a look at the items to exclude.
/// </remarks>
[<CompiledName "Except">]
val except : itemsToExclude : seq<'T> -> stack : Stack<'T> -> Stack<'T> when 'T : equality

/// <summary>Groups the elements by the key of a projection.</summary>
/// <param name="projection">The function that returns the key of an element.</param>
/// <param name="stack">The input stack.</param>
/// <returns>The keys, in the order of their first occurrence, with their elements in their order.</returns>
/// <remarks>O(n). The keys compare in F# structural equality.</remarks>
[<CompiledName "GroupBy">]
val groupBy : projection : ('T -> 'Key) -> stack : Stack<'T> -> Stack<struct ('Key * Stack<'T>)> when 'Key : equality

/// <summary>Splits the elements by a predicate.</summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="stack">The input stack.</param>
/// <returns>The elements that satisfy the predicate and the other ones, both in their order.</returns>
/// <remarks>O(n).</remarks>
[<CompiledName "Partition">]
val partition : predicate : ('T -> bool) -> stack : Stack<'T> -> struct (Stack<'T> * Stack<'T>)

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
val partitionWith : partitioner : ('T -> Choice<'T1, 'T2>) -> stack : Stack<'T> -> struct (Stack<'T1> * Stack<'T2>)

/// <summary>
/// Returns the stack without its first elements.
/// </summary>
/// <param name="count">The number of elements to skip; a negative count skips nothing.</param>
/// <param name="stack">The input stack.</param>
/// <returns>The remaining elements.</returns>
/// <exception cref="T:System.ArgumentOutOfRangeException">Thrown when the count exceeds the length.</exception>
/// <remarks>
/// O(count). The result is the stack below the skipped elements, without a copy.
/// </remarks>
[<CompiledName "Skip">]
val skip : count : int -> stack : Stack<'T> -> Stack<'T>

/// <summary>
/// Returns the stack without its last elements.
/// </summary>
/// <param name="count">The number of elements to skip; a negative count skips nothing.</param>
/// <param name="stack">The input stack.</param>
/// <returns>The remaining elements.</returns>
/// <exception cref="T:System.ArgumentOutOfRangeException">Thrown when the count exceeds the length.</exception>
/// <remarks>
/// O(n). The result is a new stack, unless nothing is skipped. The checks follow
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.Skip``1(System.Int32,Microsoft.FSharp.Collections.FSharpList{``0})"/>.
/// </remarks>
[<CompiledName "SkipLast">]
val skipLast : count : int -> stack : Stack<'T> -> Stack<'T>

/// <summary>
/// Returns the stack without its first elements that satisfy the predicate.
/// </summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="stack">The input stack.</param>
/// <returns>The elements from the first one that does not satisfy the predicate.</returns>
/// <remarks>
/// O(k), where k is the number of skipped elements. The result is the stack below them, without a copy.
/// </remarks>
[<CompiledName "SkipWhile">]
val skipWhile : predicate : ('T -> bool) -> stack : Stack<'T> -> Stack<'T>

/// <summary>
/// Splits the stack at an index.
/// </summary>
/// <param name="index">The index of the first element of the second part.</param>
/// <param name="stack">The input stack.</param>
/// <returns>The elements before the index and the elements from the index.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the index is negative.</exception>
/// <exception cref="T:System.InvalidOperationException">Thrown when the index exceeds the length.</exception>
/// <remarks>
/// O(index). The second part is the stack below the first part, without a copy; the first part is a new stack, unless
/// it is empty or the whole stack.
/// </remarks>
[<CompiledName "SplitAt">]
val splitAt : index : int -> stack : Stack<'T> -> struct (Stack<'T> * Stack<'T>)

/// <summary>
/// Splits the stack into at most a given number of parts of nearly equal length.
/// </summary>
/// <param name="count">The maximum number of parts.</param>
/// <param name="stack">The input stack.</param>
/// <returns>The parts; the first ones are one element longer when the length does not divide evenly.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when <paramref name="count"/> is not positive.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "SplitInto">]
val splitInto : count : int -> stack : Stack<'T> -> Stack<Stack<'T>>

/// <summary>
/// Returns the first elements of the stack.
/// </summary>
/// <param name="count">The number of elements to take.</param>
/// <param name="stack">The input stack.</param>
/// <returns>The first elements.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the count is negative.</exception>
/// <exception cref="T:System.InvalidOperationException">Thrown when the count exceeds the length.</exception>
/// <remarks>
/// O(count). The result is a new stack, unless it is empty or the whole stack.
/// </remarks>
[<CompiledName "Take">]
val take : count : int -> stack : Stack<'T> -> Stack<'T>

/// <summary>
/// Returns the last elements of the stack.
/// </summary>
/// <param name="count">The number of elements to take.</param>
/// <param name="stack">The input stack.</param>
/// <returns>The last elements.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the count is negative.</exception>
/// <exception cref="T:System.InvalidOperationException">Thrown when the count exceeds the length.</exception>
/// <remarks>
/// O(n). The result is the stack below the other elements, without a copy. The checks follow
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.Take``1(System.Int32,Microsoft.FSharp.Collections.FSharpList{``0})"/>.
/// </remarks>
[<CompiledName "TakeLast">]
val takeLast : count : int -> stack : Stack<'T> -> Stack<'T>

/// <summary>
/// Returns the first elements of the stack that satisfy the predicate.
/// </summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="stack">The input stack.</param>
/// <returns>The elements before the first one that does not satisfy the predicate.</returns>
/// <remarks>
/// O(k), where k is the length of the result, which is a new stack, unless it is the whole stack.
/// </remarks>
[<CompiledName "TakeWhile">]
val takeWhile : predicate : ('T -> bool) -> stack : Stack<'T> -> Stack<'T>

/// <summary>
/// Returns at most a given number of first elements.
/// </summary>
/// <param name="count">The maximum number of elements; a count that is not positive returns an empty stack.</param>
/// <param name="stack">The input stack.</param>
/// <returns>The first elements.</returns>
/// <remarks>
/// O(k), where k is the length of the result, which is a new stack, unless it is empty or the whole stack.
/// </remarks>
[<CompiledName "Truncate">]
val truncate : count : int -> stack : Stack<'T> -> Stack<'T>

// ---- Sorting ----

/// <summary>
/// Sorts the elements in the order of F# generic comparison.
/// </summary>
/// <param name="stack">The input stack.</param>
/// <returns>The sorted stack, with the smallest element on top.</returns>
/// <exception cref="T:System.InvalidOperationException">
/// Thrown when the comparison of two elements throws, as in <see cref="M:Microsoft.FSharp.Collections.ListModule.Sort``1(Microsoft.FSharp.Collections.FSharpList{``0})"/>.
/// </exception>
/// <remarks>
/// O(n log n). This is a stable sort, as <see cref="M:Microsoft.FSharp.Collections.ListModule.Sort``1(Microsoft.FSharp.Collections.FSharpList{``0})"/>:
/// equal elements keep their order.
/// </remarks>
[<CompiledName "Sort">]
val sort : stack : Stack<'T> -> Stack<'T> when 'T : comparison

/// <summary>
/// Sorts the elements by the result of a projection, in the order of F# generic comparison.
/// </summary>
/// <param name="projection">The function that returns the key of an element.</param>
/// <param name="stack">The input stack.</param>
/// <returns>The sorted stack.</returns>
/// <remarks>
/// O(n log n). This is a stable sort: elements with equal keys keep their order.
/// </remarks>
[<CompiledName "SortBy">]
val sortBy : projection : ('T -> 'Key) -> stack : Stack<'T> -> Stack<'T> when 'Key : comparison

/// <summary>
/// Sorts the elements by the result of a projection, in descending order.
/// </summary>
/// <param name="projection">The function that returns the key of an element.</param>
/// <param name="stack">The input stack.</param>
/// <returns>The sorted stack.</returns>
/// <exception cref="T:System.InvalidOperationException">
/// Thrown when the projection or the comparison of two keys throws: the projection runs in the comparison, as in
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.SortByDescending``2(Microsoft.FSharp.Core.FSharpFunc{``0,``1},Microsoft.FSharp.Collections.FSharpList{``0})"/>.
/// </exception>
/// <remarks>
/// O(n log n). This is a stable sort: elements with equal keys keep their order.
/// </remarks>
[<CompiledName "SortByDescending">]
val sortByDescending : projection : ('T -> 'Key) -> stack : Stack<'T> -> Stack<'T> when 'Key : comparison

/// <summary>
/// Sorts the elements in descending order of F# generic comparison.
/// </summary>
/// <param name="stack">The input stack.</param>
/// <returns>The sorted stack, with the greatest element on top.</returns>
/// <exception cref="T:System.InvalidOperationException">
/// Thrown when the comparison of two elements throws, as in <see cref="M:Microsoft.FSharp.Collections.ListModule.SortDescending``1(Microsoft.FSharp.Collections.FSharpList{``0})"/>.
/// </exception>
/// <remarks>
/// O(n log n). This is a stable sort: equal elements keep their order.
/// </remarks>
[<CompiledName "SortDescending">]
val sortDescending : stack : Stack<'T> -> Stack<'T> when 'T : comparison

/// <summary>
/// Sorts the elements with a comparison function.
/// </summary>
/// <param name="comparer">The function that compares two elements.</param>
/// <param name="stack">The input stack.</param>
/// <returns>The sorted stack.</returns>
/// <exception cref="T:System.InvalidOperationException">
/// Thrown when the comparer throws, as in
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.SortWith``1(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpFunc{``0,System.Int32}},Microsoft.FSharp.Collections.FSharpList{``0})"/>.
/// </exception>
/// <remarks>
/// O(n log n). This is a stable sort: elements that compare equal keep their order.
/// </remarks>
[<CompiledName "SortWith">]
val sortWith : comparer : ('T -> 'T -> int) -> stack : Stack<'T> -> Stack<'T>

/// <summary>
/// Sorts the elements with a comparer.
/// </summary>
/// <param name="comparer">The comparer.</param>
/// <param name="stack">The input stack.</param>
/// <returns>The sorted stack.</returns>
/// <remarks>
/// O(n log n). This is a stable sort: elements that compare equal keep their order. The function wraps
/// <see cref="M:System.Linq.Enumerable.Order``1(System.Collections.Generic.IEnumerable{``0},System.Collections.Generic.IComparer{``0})"/>.
/// </remarks>
[<CompiledName "SortWithComparer">]
val sortWithComparer : comparer : IComparer<'T> -> stack : Stack<'T> -> Stack<'T>

// ---- Random ----

/// <summary>
/// Returns a random element, chosen with <see cref="P:System.Random.Shared"/>.
/// </summary>
/// <param name="source">The input stack.</param>
/// <returns>A random element.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the stack is empty.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "RandomChoice">]
val randomChoice : source : Stack<'T> -> 'T

/// <summary>
/// Returns a random element, chosen with a function that returns numbers in [0, 1).
/// </summary>
/// <param name="randomizer">The function that returns a random number in [0, 1).</param>
/// <param name="source">The input stack.</param>
/// <returns>A random element.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the stack is empty.</exception>
/// <exception cref="T:System.ArgumentOutOfRangeException">Thrown when the randomizer returns a number outside [0, 1).</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "RandomChoiceBy">]
val randomChoiceBy : randomizer : (unit -> float) -> source : Stack<'T> -> 'T

/// <summary>
/// Returns a random element, chosen with a given random number generator.
/// </summary>
/// <param name="random">The random number generator.</param>
/// <param name="source">The input stack.</param>
/// <returns>A random element.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="random"/> is <see langword="null"/>.</exception>
/// <exception cref="T:System.ArgumentException">Thrown when the stack is empty.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "RandomChoiceWith">]
val randomChoiceWith : random : Random -> source : Stack<'T> -> 'T

/// <summary>
/// Returns random elements, chosen with <see cref="P:System.Random.Shared"/>; an element can repeat.
/// </summary>
/// <param name="count">The number of elements to choose.</param>
/// <param name="source">The input stack.</param>
/// <returns>The chosen elements.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the count is negative, or positive for an empty stack.</exception>
/// <remarks>
/// O(n + count).
/// </remarks>
[<CompiledName "RandomChoices">]
val randomChoices : count : int -> source : Stack<'T> -> Stack<'T>

/// <summary>
/// Returns random elements, chosen with a function that returns numbers in [0, 1); an element can repeat.
/// </summary>
/// <param name="randomizer">The function that returns a random number in [0, 1).</param>
/// <param name="count">The number of elements to choose.</param>
/// <param name="source">The input stack.</param>
/// <returns>The chosen elements.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the count is negative, or positive for an empty stack.</exception>
/// <exception cref="T:System.ArgumentOutOfRangeException">Thrown when the randomizer returns a number outside [0, 1).</exception>
/// <remarks>
/// O(n + count).
/// </remarks>
[<CompiledName "RandomChoicesBy">]
val randomChoicesBy : randomizer : (unit -> float) -> count : int -> source : Stack<'T> -> Stack<'T>

/// <summary>
/// Returns random elements, chosen with a given random number generator; an element can repeat.
/// </summary>
/// <param name="random">The random number generator.</param>
/// <param name="count">The number of elements to choose.</param>
/// <param name="source">The input stack.</param>
/// <returns>The chosen elements.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="random"/> is <see langword="null"/>.</exception>
/// <exception cref="T:System.ArgumentException">Thrown when the count is negative, or positive for an empty stack.</exception>
/// <remarks>
/// O(n + count).
/// </remarks>
[<CompiledName "RandomChoicesWith">]
val randomChoicesWith : random : Random -> count : int -> source : Stack<'T> -> Stack<'T>

/// <summary>
/// Returns random elements at distinct indices, chosen with <see cref="P:System.Random.Shared"/>.
/// </summary>
/// <param name="count">The number of elements to choose.</param>
/// <param name="source">The input stack.</param>
/// <returns>The chosen elements.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the count is negative or exceeds the length.</exception>
/// <remarks>
/// O(n + count).
/// </remarks>
[<CompiledName "RandomSample">]
val randomSample : count : int -> source : Stack<'T> -> Stack<'T>

/// <summary>
/// Returns random elements at distinct indices, chosen with a function that returns numbers in [0, 1).
/// </summary>
/// <param name="randomizer">The function that returns a random number in [0, 1).</param>
/// <param name="count">The number of elements to choose.</param>
/// <param name="source">The input stack.</param>
/// <returns>The chosen elements.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the count is negative or exceeds the length.</exception>
/// <exception cref="T:System.ArgumentOutOfRangeException">Thrown when the randomizer returns a number outside [0, 1).</exception>
/// <remarks>
/// O(n + count).
/// </remarks>
[<CompiledName "RandomSampleBy">]
val randomSampleBy : randomizer : (unit -> float) -> count : int -> source : Stack<'T> -> Stack<'T>

/// <summary>
/// Returns random elements at distinct indices, chosen with a given random number generator.
/// </summary>
/// <param name="random">The random number generator.</param>
/// <param name="count">The number of elements to choose.</param>
/// <param name="source">The input stack.</param>
/// <returns>The chosen elements.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="random"/> is <see langword="null"/>.</exception>
/// <exception cref="T:System.ArgumentException">Thrown when the count is negative or exceeds the length.</exception>
/// <remarks>
/// O(n + count).
/// </remarks>
[<CompiledName "RandomSampleWith">]
val randomSampleWith : random : Random -> count : int -> source : Stack<'T> -> Stack<'T>

/// <summary>
/// Returns the elements in a random order, chosen with <see cref="P:System.Random.Shared"/>.
/// </summary>
/// <param name="source">The input stack.</param>
/// <returns>The shuffled stack.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "RandomShuffle">]
val randomShuffle : source : Stack<'T> -> Stack<'T>

/// <summary>
/// Returns the elements in a random order, chosen with a function that returns numbers in [0, 1).
/// </summary>
/// <param name="randomizer">The function that returns a random number in [0, 1).</param>
/// <param name="source">The input stack.</param>
/// <returns>The shuffled stack.</returns>
/// <exception cref="T:System.ArgumentOutOfRangeException">Thrown when the randomizer returns a number outside [0, 1).</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "RandomShuffleBy">]
val randomShuffleBy : randomizer : (unit -> float) -> source : Stack<'T> -> Stack<'T>

/// <summary>
/// Returns the elements in a random order, chosen with a given random number generator.
/// </summary>
/// <param name="random">The random number generator.</param>
/// <param name="source">The input stack.</param>
/// <returns>The shuffled stack.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="random"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "RandomShuffleWith">]
val randomShuffleWith : random : Random -> source : Stack<'T> -> Stack<'T>

// ---- Conversion ----

/// <summary>
/// Returns a new array with the elements, from the top to the bottom.
/// </summary>
/// <param name="stack">The input stack.</param>
/// <returns>A copy of the elements.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "ToArray">]
val toArray : stack : Stack<'T> -> 'T array

/// <summary>
/// Returns a list of the elements, from the top to the bottom.
/// </summary>
/// <param name="stack">The input stack.</param>
/// <returns>The list of the elements.</returns>
/// <remarks>
/// O(n). The function is the inverse of
/// <see cref="M:FSharp.Collections.Immutable.Stack.OfList``1(Microsoft.FSharp.Collections.FSharpList{``0})"/>.
/// </remarks>
[<CompiledName "ToList">]
val toList : stack : Stack<'T> -> 'T list

/// <summary>
/// Returns the stack as a sequence, which enumerates the elements from the top to the bottom.
/// </summary>
/// <param name="stack">The input stack.</param>
/// <returns>The sequence of the elements.</returns>
/// <remarks>
/// O(1).
/// </remarks>
[<CompiledName "ToSeq">]
val toSeq : stack : Stack<'T> -> seq<'T>
