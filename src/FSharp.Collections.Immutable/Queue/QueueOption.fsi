/// <summary>
/// The functions of <see cref="T:FSharp.Collections.Immutable.Queue"/> that take or return an optional value, with
/// <see cref="T:Microsoft.FSharp.Core.FSharpOption`1"/> instead of <see cref="T:Microsoft.FSharp.Core.FSharpValueOption`1"/>.
/// </summary>
/// <remarks>
/// After <c>open FSharp.Collections.Immutable.Option</c>, these functions shadow the default ones, and the other functions
/// of the default module stay available (ADR 0001, D3). Each function has the signature of the function with the same
/// name in <see cref="T:Microsoft.FSharp.Collections.ListModule"/> where that module has one, and the checks and the
/// exceptions of the default function.
/// </remarks>
[<RequireQualifiedAccess>]
module FSharp.Collections.Immutable.Option.Queue

open FSharp.Collections.Immutable

// ---- Creation ----

/// <summary>
/// Creates a queue from a generator, which returns the next element and the next state.
/// </summary>
/// <param name="generator">The function that returns the next element and state, or <c>None</c> to stop.</param>
/// <param name="state">The initial state.</param>
/// <returns>The generated queue, whose first element is the first generated one.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Unfold">]
val unfold : generator : ('State -> ('T * 'State) option) -> state : 'State -> Queue<'T>

// ---- Access ----

/// <summary>
/// Returns the only element of the queue, or <c>None</c> when it does not have exactly one element.
/// </summary>
/// <param name="queue">The input queue.</param>
/// <returns>The only element, or <c>None</c>.</returns>
/// <remarks>
/// Amortized O(1).
/// </remarks>
[<CompiledName "TryExactlyOne">]
val tryExactlyOne : queue : Queue<'T> -> 'T option

/// <summary>
/// Returns the first element of the queue, or <c>None</c> when it is empty.
/// </summary>
/// <param name="queue">The input queue.</param>
/// <returns>The first element, or <c>None</c>.</returns>
/// <remarks>
/// O(1).
/// </remarks>
[<CompiledName "TryPeek">]
val tryHead : queue : Queue<'T> -> 'T option

/// <summary>
/// Returns the last element of the queue, or <c>None</c> when it is empty.
/// </summary>
/// <param name="queue">The input queue.</param>
/// <returns>The last element, or <c>None</c>.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "TryLast">]
val tryLast : queue : Queue<'T> -> 'T option

/// <summary>
/// Returns the queue without its first element, or <c>None</c> when it is empty.
/// </summary>
/// <param name="queue">The input queue.</param>
/// <returns>The elements after the first one, or <c>None</c>.</returns>
/// <remarks>
/// Amortized O(1).
/// </remarks>
[<CompiledName "TryDequeue">]
val tryTail : queue : Queue<'T> -> Queue<'T> option

/// <summary>
/// Returns the first element and the other elements, or <c>None</c> when the queue is empty.
/// </summary>
/// <param name="queue">The input queue.</param>
/// <returns>The first element and the elements after it, or <c>None</c>.</returns>
/// <remarks>
/// Amortized O(1).
/// </remarks>
[<CompiledName "TryPeekDequeue">]
val tryUncons : queue : Queue<'T> -> ('T * Queue<'T>) option

/// <summary>
/// Matches a queue with a first element, and returns that element and the other elements.
/// </summary>
/// <param name="queue">The input queue.</param>
/// <returns>The first element and the elements after it, or <c>None</c> for an empty queue.</returns>
/// <remarks>
/// Amortized O(1).
/// </remarks>
[<CompiledName "ConsPattern">]
val (|Cons|_|) : queue : Queue<'T> -> ('T * Queue<'T>) option

// ---- Search ----

/// <summary>
/// Returns the first value that the chooser returns for the elements.
/// </summary>
/// <param name="chooser">The function that returns a value, or <c>None</c> to go on.</param>
/// <param name="queue">The input queue.</param>
/// <returns>The first chosen value.</returns>
/// <exception cref="T:System.Collections.Generic.KeyNotFoundException">Thrown when the chooser returns <c>None</c> for every element.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Pick">]
val pick : chooser : ('T -> 'U option) -> queue : Queue<'T> -> 'U

/// <summary>
/// Returns the first element that satisfies the predicate, or <c>None</c>.
/// </summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="queue">The input queue.</param>
/// <returns>The first element that satisfies the predicate, or <c>None</c>.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "TryFind">]
val tryFind : predicate : ('T -> bool) -> queue : Queue<'T> -> 'T option

/// <summary>
/// Returns the last element that satisfies the predicate, or <c>None</c>.
/// </summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="queue">The input queue.</param>
/// <returns>The last element that satisfies the predicate, or <c>None</c>.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "TryFindBack">]
val tryFindBack : predicate : ('T -> bool) -> queue : Queue<'T> -> 'T option

/// <summary>
/// Returns the index of the first element that satisfies the predicate, or <c>None</c>.
/// </summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="queue">The input queue.</param>
/// <returns>The index of the first element that satisfies the predicate, or <c>None</c>.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "TryFindIndex">]
val tryFindIndex : predicate : ('T -> bool) -> queue : Queue<'T> -> int option

/// <summary>
/// Returns the index of the last element that satisfies the predicate, or <c>None</c>.
/// </summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="queue">The input queue.</param>
/// <returns>The index of the last element that satisfies the predicate, or <c>None</c>.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "TryFindIndexBack">]
val tryFindIndexBack : predicate : ('T -> bool) -> queue : Queue<'T> -> int option

/// <summary>
/// Returns the first value that the chooser returns for the elements, or <c>None</c>.
/// </summary>
/// <param name="chooser">The function that returns a value, or <c>None</c> to go on.</param>
/// <param name="queue">The input queue.</param>
/// <returns>The first chosen value, or <c>None</c>.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "TryPick">]
val tryPick : chooser : ('T -> 'U option) -> queue : Queue<'T> -> 'U option

// ---- Transformation ----

/// <summary>
/// Returns the values that the chooser returns for the elements.
/// </summary>
/// <param name="chooser">The function that returns a value, or <c>None</c> to skip the element.</param>
/// <param name="queue">The input queue.</param>
/// <returns>The chosen values.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Choose">]
val choose : chooser : ('T -> 'U option) -> queue : Queue<'T> -> Queue<'U>
