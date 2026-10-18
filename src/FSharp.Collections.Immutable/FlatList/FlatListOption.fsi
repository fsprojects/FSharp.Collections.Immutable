/// <summary>
/// The functions of <see cref="T:FSharp.Collections.Immutable.FlatList"/> that take or return an optional
/// value, with <see cref="T:Microsoft.FSharp.Core.FSharpOption`1"/> instead of
/// <see cref="T:Microsoft.FSharp.Core.FSharpValueOption`1"/>.
/// </summary>
/// <remarks>
/// After <c>open FSharp.Collections.Immutable.Option</c>, these functions shadow the default ones, and the other functions
/// of the default module stay available (ADR 0001, D3). Each function has the signature of the function with the same
/// name in <see cref="T:Microsoft.FSharp.Collections.ArrayModule"/> where that module has one.
/// </remarks>
[<RequireQualifiedAccess>]
module FSharp.Collections.Immutable.Option.FlatList

open FSharp.Collections.Immutable

// ---- Creation ----

/// <summary>
/// Creates a flat list from a generator, which returns the next element and the next state.
/// </summary>
/// <param name="generator">The function that returns the next element and state, or <c>None</c> to stop.</param>
/// <param name="state">The initial state.</param>
/// <returns>The generated flat list.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Unfold">]
val unfold : generator : ('State -> ('T * 'State) option) -> state : 'State -> FlatList<'T>

// ---- Access ----

/// <summary>
/// Returns the only element of the flat list, or <c>None</c> when it does not have exactly one element.
/// </summary>
/// <param name="array">The input flat list.</param>
/// <returns>The only element, or <c>None</c>.</returns>
/// <remarks>
/// O(1).
/// </remarks>
[<CompiledName "TryExactlyOne">]
val tryExactlyOne : array : FlatList<'T> -> 'T option

/// <summary>
/// Returns the first element of the flat list, or <c>None</c> when it is empty.
/// </summary>
/// <param name="array">The input flat list.</param>
/// <returns>The first element, or <c>None</c>.</returns>
/// <remarks>
/// O(1).
/// </remarks>
[<CompiledName "TryHead">]
val tryHead : array : FlatList<'T> -> 'T option

/// <summary>
/// Returns the element at an index, or <c>None</c> when the index is out of range.
/// </summary>
/// <param name="index">The index of the element.</param>
/// <param name="array">The input flat list.</param>
/// <returns>The element at the index, or <c>None</c>.</returns>
/// <remarks>
/// O(1).
/// </remarks>
[<CompiledName "TryItem">]
val tryItem : index : int -> array : FlatList<'T> -> 'T option

/// <summary>
/// Returns the last element of the flat list, or <c>None</c> when it is empty.
/// </summary>
/// <param name="array">The input flat list.</param>
/// <returns>The last element, or <c>None</c>.</returns>
/// <remarks>
/// O(1).
/// </remarks>
[<CompiledName "TryLast">]
val tryLast : array : FlatList<'T> -> 'T option

/// <summary>
/// Returns the flat list without its first element, or <c>None</c> when it is empty.
/// </summary>
/// <param name="array">The input flat list.</param>
/// <returns>A copy of the elements after the first one, or <c>None</c>.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "TryTail">]
val tryTail : array : FlatList<'T> -> FlatList<'T> option

// ---- Search ----

/// <summary>
/// Returns the first value that the chooser returns for the elements.
/// </summary>
/// <param name="chooser">The function that returns a value, or <c>None</c> to go on.</param>
/// <param name="array">The input flat list.</param>
/// <returns>The first chosen value.</returns>
/// <exception cref="T:System.Collections.Generic.KeyNotFoundException">Thrown when the chooser returns <c>None</c> for every element.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Pick">]
val pick : chooser : ('T -> 'U option) -> array : FlatList<'T> -> 'U

/// <summary>
/// Returns the first element that satisfies the predicate, or <c>None</c>.
/// </summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="array">The input flat list.</param>
/// <returns>The first element that satisfies the predicate, or <c>None</c>.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "TryFind">]
val tryFind : predicate : ('T -> bool) -> array : FlatList<'T> -> 'T option

/// <summary>
/// Returns the last element that satisfies the predicate, or <c>None</c>.
/// </summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="array">The input flat list.</param>
/// <returns>The last element that satisfies the predicate, or <c>None</c>.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "TryFindBack">]
val tryFindBack : predicate : ('T -> bool) -> array : FlatList<'T> -> 'T option

/// <summary>
/// Returns the index of the first element that satisfies the predicate, or <c>None</c>.
/// </summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="array">The input flat list.</param>
/// <returns>The index of the first element that satisfies the predicate, or <c>None</c>.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "TryFindIndex">]
val tryFindIndex : predicate : ('T -> bool) -> array : FlatList<'T> -> int option

/// <summary>
/// Returns the index of the last element that satisfies the predicate, or <c>None</c>.
/// </summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="array">The input flat list.</param>
/// <returns>The index of the last element that satisfies the predicate, or <c>None</c>.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "TryFindIndexBack">]
val tryFindIndexBack : predicate : ('T -> bool) -> array : FlatList<'T> -> int option

/// <summary>
/// Returns the first value that the chooser returns for the elements, or <c>None</c>.
/// </summary>
/// <param name="chooser">The function that returns a value, or <c>None</c> to go on.</param>
/// <param name="array">The input flat list.</param>
/// <returns>The first chosen value, or <c>None</c>.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "TryPick">]
val tryPick : chooser : ('T -> 'U option) -> array : FlatList<'T> -> 'U option

// ---- Transformation ----

/// <summary>
/// Returns the values that the chooser returns for the elements.
/// </summary>
/// <param name="chooser">The function that returns a value, or <c>None</c> to skip the element.</param>
/// <param name="array">The input flat list.</param>
/// <returns>The chosen values.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Choose">]
val choose : chooser : ('T -> 'U option) -> array : FlatList<'T> -> FlatList<'U>

/// <summary>
/// The functions of <see cref="T:FSharp.Collections.Immutable.FlatList.Parallel"/> that take or return an optional
/// value, with <see cref="T:Microsoft.FSharp.Core.FSharpOption`1"/> instead of
/// <see cref="T:Microsoft.FSharp.Core.FSharpValueOption`1"/>.
/// </summary>
/// <remarks>
/// The other functions of the default module stay available after <c>open FSharp.Collections.Immutable.Option</c>. Each
/// function has the signature of the function with the same name in
/// <see cref="T:Microsoft.FSharp.Collections.ArrayModule.Parallel"/>.
/// </remarks>
[<RequireQualifiedAccess>]
module Parallel =

    // ---- Search ----

    /// <summary>
    /// Returns the first element that satisfies the predicate, or <c>None</c>.
    /// </summary>
    /// <param name="predicate">The function to test the elements, called in parallel.</param>
    /// <param name="array">The input flat list.</param>
    /// <returns>The first element that satisfies the predicate, or <c>None</c>.</returns>
    /// <remarks>
    /// O(n). The predicate can also run for elements after the first one that satisfies it.
    /// </remarks>
    [<CompiledName "TryFind">]
    val tryFind : predicate : ('T -> bool) -> array : FlatList<'T> -> 'T option

    /// <summary>
    /// Returns the index of the first element that satisfies the predicate, or <c>None</c>.
    /// </summary>
    /// <param name="predicate">The function to test the elements, called in parallel.</param>
    /// <param name="array">The input flat list.</param>
    /// <returns>The index of the first element that satisfies the predicate, or <c>None</c>.</returns>
    /// <remarks>
    /// O(n). The predicate can also run for elements after the first one that satisfies it.
    /// </remarks>
    [<CompiledName "TryFindIndex">]
    val tryFindIndex : predicate : ('T -> bool) -> array : FlatList<'T> -> int option

    /// <summary>
    /// Returns the first value that the chooser returns for the elements, or <c>None</c>.
    /// </summary>
    /// <param name="chooser">The function that returns a value, or <c>None</c> to go on, called in parallel.</param>
    /// <param name="array">The input flat list.</param>
    /// <returns>The value for the first element that the chooser returns a value for, or <c>None</c>.</returns>
    /// <remarks>
    /// O(n). The chooser can also run for elements after the first one that it returns a value for.
    /// </remarks>
    [<CompiledName "TryPick">]
    val tryPick : chooser : ('T -> 'U option) -> array : FlatList<'T> -> 'U option

    // ---- Transformation ----

    /// <summary>
    /// Returns the values that the chooser returns for the elements.
    /// </summary>
    /// <param name="chooser">
    /// The function that returns a value, or <c>None</c> to skip the element, called in parallel.
    /// </param>
    /// <param name="array">The input flat list.</param>
    /// <returns>The chosen values, in the order of their elements.</returns>
    /// <remarks>
    /// O(n).
    /// </remarks>
    [<CompiledName "Choose">]
    val choose : chooser : ('T -> 'U option) -> array : FlatList<'T> -> FlatList<'U>
