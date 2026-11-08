/// <summary>
/// The functions of <see cref="T:FSharp.Collections.Immutable.IndexedList"/> that take or return an optional value, with
/// <see cref="T:Microsoft.FSharp.Core.FSharpOption`1"/> instead of <see cref="T:Microsoft.FSharp.Core.FSharpValueOption`1"/>.
/// </summary>
/// <remarks>
/// After <c>open FSharp.Collections.Immutable.Option</c>, these functions shadow the default ones, and the other functions
/// of the default module stay available (ADR 0001, D3). Each function has the signature of the function with the same
/// name in <see cref="T:Microsoft.FSharp.Collections.ListModule"/> where that module has one, and the checks and the
/// exceptions of the default function.
/// </remarks>
[<RequireQualifiedAccess>]
module FSharp.Collections.Immutable.Option.IndexedList

open FSharp.Collections.Immutable

// ---- Creation ----

/// <summary>
/// Creates an indexed list from a generator, which returns the next element and the next state.
/// </summary>
/// <param name="generator">The function that returns the next element and state, or <c>None</c> to stop.</param>
/// <param name="state">The initial state.</param>
/// <returns>The generated indexed list.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Unfold">]
val unfold : generator : ('State -> ('T * 'State) option) -> state : 'State -> IndexedList<'T>

// ---- Access ----

/// <summary>
/// Returns the only element of the indexed list, or <c>None</c> when it does not have exactly one element.
/// </summary>
/// <param name="list">The input indexed list.</param>
/// <returns>The only element, or <c>None</c>.</returns>
/// <remarks>
/// O(1).
/// </remarks>
[<CompiledName "TryExactlyOne">]
val tryExactlyOne : list : IndexedList<'T> -> 'T option

/// <summary>
/// Returns the first element of the indexed list, or <c>None</c> when it is empty.
/// </summary>
/// <param name="list">The input indexed list.</param>
/// <returns>The first element, or <c>None</c>.</returns>
/// <remarks>
/// O(log n).
/// </remarks>
[<CompiledName "TryHead">]
val tryHead : list : IndexedList<'T> -> 'T option

/// <summary>
/// Returns the element at an index, or <c>None</c> when the index is out of range.
/// </summary>
/// <param name="index">The index of the element.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>The element at the index, or <c>None</c>.</returns>
/// <remarks>
/// O(log n).
/// </remarks>
[<CompiledName "TryItem">]
val tryItem : index : int -> list : IndexedList<'T> -> 'T option

/// <summary>
/// Returns the last element of the indexed list, or <c>None</c> when it is empty.
/// </summary>
/// <param name="list">The input indexed list.</param>
/// <returns>The last element, or <c>None</c>.</returns>
/// <remarks>
/// O(log n).
/// </remarks>
[<CompiledName "TryLast">]
val tryLast : list : IndexedList<'T> -> 'T option

/// <summary>
/// Returns the indexed list without its first element, or <c>None</c> when it is empty.
/// </summary>
/// <param name="list">The input indexed list.</param>
/// <returns>The elements after the first one, or <c>None</c>.</returns>
/// <remarks>
/// O(log n).
/// </remarks>
[<CompiledName "TryTail">]
val tryTail : list : IndexedList<'T> -> IndexedList<'T> option

/// <summary>
/// Returns the first element and the other elements, or <c>None</c> when the indexed list is empty.
/// </summary>
/// <param name="list">The input indexed list.</param>
/// <returns>The first element and the elements after it, or <c>None</c>.</returns>
/// <remarks>
/// O(log n).
/// </remarks>
[<CompiledName "TryUncons">]
val tryUncons : list : IndexedList<'T> -> ('T * IndexedList<'T>) option

/// <summary>
/// Matches an indexed list with a first element, and returns that element and the other elements.
/// </summary>
/// <param name="list">The input indexed list.</param>
/// <returns>The first element and the elements after it, or <c>None</c> for an empty indexed list.</returns>
/// <remarks>
/// O(log n).
/// </remarks>
[<CompiledName "ConsPattern">]
val (|Cons|_|) : list : IndexedList<'T> -> ('T * IndexedList<'T>) option

// ---- Search ----

/// <summary>
/// Returns the first value that the chooser returns for the elements.
/// </summary>
/// <param name="chooser">The function that returns a value, or <c>None</c> to go on.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>The first chosen value.</returns>
/// <exception cref="T:System.Collections.Generic.KeyNotFoundException">Thrown when the chooser returns <c>None</c> for every element.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Pick">]
val pick : chooser : ('T -> 'U option) -> list : IndexedList<'T> -> 'U

/// <summary>
/// Returns the first element that satisfies the predicate, or <c>None</c>.
/// </summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>The first element that satisfies the predicate, or <c>None</c>.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "TryFind">]
val tryFind : predicate : ('T -> bool) -> list : IndexedList<'T> -> 'T option

/// <summary>
/// Returns the last element that satisfies the predicate, or <c>None</c>.
/// </summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>The last element that satisfies the predicate, or <c>None</c>.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "TryFindBack">]
val tryFindBack : predicate : ('T -> bool) -> list : IndexedList<'T> -> 'T option

/// <summary>
/// Returns the index of the first element that satisfies the predicate, or <c>None</c>.
/// </summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>The index of the first element that satisfies the predicate, or <c>None</c>.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "TryFindIndex">]
val tryFindIndex : predicate : ('T -> bool) -> list : IndexedList<'T> -> int option

/// <summary>
/// Returns the index of the last element that satisfies the predicate, or <c>None</c>.
/// </summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>The index of the last element that satisfies the predicate, or <c>None</c>.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "TryFindIndexBack">]
val tryFindIndexBack : predicate : ('T -> bool) -> list : IndexedList<'T> -> int option

/// <summary>
/// Returns the index of the last element of a range that satisfies the predicate, or <c>None</c>. The search walks
/// backward from the index of the last element of the range.
/// </summary>
/// <param name="startIndex">The index of the last element of the range, where the search starts.</param>
/// <param name="count">The number of elements of the range, which ends at <paramref name="startIndex"/>.</param>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>
/// The index in the indexed list, not in the range, of the last element of the range that satisfies the predicate, or
/// <c>None</c>.
/// </returns>
/// <exception cref="T:System.ArgumentOutOfRangeException">
/// Thrown when the range does not lie in the indexed list, as in
/// <see cref="M:FSharp.Collections.Immutable.IndexedList.TryFindIndexBackRange``1(System.Int32,System.Int32,Microsoft.FSharp.Core.FSharpFunc{``0,System.Boolean},System.Collections.Immutable.ImmutableList{``0})"/>.
/// </exception>
/// <remarks>
/// O(log n + count).
/// </remarks>
[<CompiledName "TryFindIndexBackRange">]
val tryFindIndexBackRange : startIndex : int -> count : int -> predicate : ('T -> bool) -> list : IndexedList<'T> -> int option

/// <summary>
/// Returns the index of the first element of a range that satisfies the predicate, or <c>None</c>.
/// </summary>
/// <param name="startIndex">The index of the first element of the range.</param>
/// <param name="count">The number of elements of the range.</param>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>
/// The index in the indexed list, not in the range, of the first element of the range that satisfies the predicate, or
/// <c>None</c>.
/// </returns>
/// <exception cref="T:System.ArgumentOutOfRangeException">
/// Thrown when the range does not lie in the indexed list, as in
/// <see cref="M:FSharp.Collections.Immutable.IndexedList.TryFindIndexRange``1(System.Int32,System.Int32,Microsoft.FSharp.Core.FSharpFunc{``0,System.Boolean},System.Collections.Immutable.ImmutableList{``0})"/>.
/// </exception>
/// <remarks>
/// O(log n + count).
/// </remarks>
[<CompiledName "TryFindIndexRange">]
val tryFindIndexRange : startIndex : int -> count : int -> predicate : ('T -> bool) -> list : IndexedList<'T> -> int option

/// <summary>
/// Returns the first value that the chooser returns for the elements, or <c>None</c>.
/// </summary>
/// <param name="chooser">The function that returns a value, or <c>None</c> to go on.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>The first chosen value, or <c>None</c>.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "TryPick">]
val tryPick : chooser : ('T -> 'U option) -> list : IndexedList<'T> -> 'U option

// ---- Transformation ----

/// <summary>
/// Returns the values that the chooser returns for the elements.
/// </summary>
/// <param name="chooser">The function that returns a value, or <c>None</c> to skip the element.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>The chosen values.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Choose">]
val choose : chooser : ('T -> 'U option) -> list : IndexedList<'T> -> IndexedList<'U>
