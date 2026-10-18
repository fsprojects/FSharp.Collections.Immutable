/// <summary>
/// Functions for <see cref="T:System.Collections.Immutable.IImmutableList`1"/>, the interface of the immutable lists.
/// </summary>
/// <remarks>
/// <para>
/// The module mirrors <see cref="T:Microsoft.FSharp.Collections.ListModule"/> as
/// <see cref="T:FSharp.Collections.Immutable.IndexedList"/> does: each function has the semantics, the parameter names,
/// the argument checks and the exceptions of the function with the same name in that module (ADR 0001), and its sorting
/// functions are stable. The module has no creation and no builder functions, because an interface cannot create a list.
/// </para>
/// <para>
/// A function whose result has the element type of its input keeps the implementation of the input: it builds the result
/// with the members of the input, such as <see cref="M:System.Collections.Immutable.IImmutableList`1.Clear"/> followed by
/// <see cref="M:System.Collections.Immutable.IImmutableList`1.AddRange(System.Collections.Generic.IEnumerable{`0})"/>,
/// <see cref="M:System.Collections.Immutable.IImmutableList`1.Insert(System.Int32,`0)"/> or
/// <see cref="M:System.Collections.Immutable.IImmutableList`1.RemoveAt(System.Int32)"/>. A function that changes the
/// element type, or that combines several lists into one, returns an <see cref="T:System.Collections.Immutable.ImmutableList`1"/>.
/// </para>
/// <para>
/// Every function throws <see cref="T:System.ArgumentNullException"/> for a <see langword="null"/> list. A function that
/// takes a sequence throws it for a <see langword="null"/> sequence and for a default
/// <see cref="T:System.Collections.Immutable.ImmutableArray`1"/> that is given as a sequence, which would fail with
/// <see cref="T:System.InvalidOperationException"/> when the function enumerates it. The complexities in the remarks are
/// the ones of an <see cref="T:System.Collections.Immutable.ImmutableList`1"/>.
/// </para>
/// <para>
/// Functions that return an optional value return a <see cref="T:Microsoft.FSharp.Core.FSharpValueOption`1"/>, and functions
/// that return tuples return struct tuples. The namespaces <see cref="N:FSharp.Collections.Immutable.Option"/> and
/// <see cref="N:FSharp.Collections.Immutable.Tuple"/> contain a module with the same name whose functions use options
/// and reference tuples instead.
/// </para>
/// </remarks>
[<RequireQualifiedAccess>]
module FSharp.Collections.Immutable.IIndexedList

open System
open System.Collections.Generic

// ---- Properties ----

/// <summary>Returns whether the list has no elements.</summary>
/// <param name="list">The input list.</param>
/// <returns><see langword="true"/> when the list is empty.</returns>
/// <remarks>O(1).</remarks>
[<CompiledName "IsEmpty">]
val isEmpty : list : IIndexedList<'T> -> bool

/// <summary>Returns the number of elements of the list.</summary>
/// <param name="list">The input list.</param>
/// <returns>The length of the list.</returns>
/// <remarks>O(1).</remarks>
[<CompiledName "Length">]
val length : list : IIndexedList<'T> -> int

// ---- Access ----

/// <summary>Returns the only element of the list.</summary>
/// <param name="list">The input list.</param>
/// <returns>The only element.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the list does not have exactly one element.</exception>
/// <remarks>O(1).</remarks>
[<CompiledName "ExactlyOne">]
val exactlyOne : list : IIndexedList<'T> -> 'T

/// <summary>Returns the first element of the list.</summary>
/// <param name="list">The input list.</param>
/// <returns>The first element.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the list is empty.</exception>
/// <remarks>O(log n).</remarks>
[<CompiledName "Head">]
val head : list : IIndexedList<'T> -> 'T

/// <summary>Returns the element at an index.</summary>
/// <param name="index">The index of the element.</param>
/// <param name="list">The input list.</param>
/// <returns>The element at the index.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the index is negative or not less than the length.</exception>
/// <remarks>O(log n).</remarks>
[<CompiledName "Item">]
val item : index : int -> list : IIndexedList<'T> -> 'T

/// <summary>Returns the last element of the list.</summary>
/// <param name="list">The input list.</param>
/// <returns>The last element.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the list is empty.</exception>
/// <remarks>O(log n).</remarks>
[<CompiledName "Last">]
val last : list : IIndexedList<'T> -> 'T

/// <summary>
/// Returns the list without its first element.
/// </summary>
/// <param name="list">The input list.</param>
/// <returns>The elements after the first one, in a list of the implementation of the input.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the list is empty.</exception>
/// <remarks>
/// O(log n). The function wraps <see cref="M:System.Collections.Immutable.IImmutableList`1.RemoveAt(System.Int32)"/>.
/// </remarks>
[<CompiledName "Tail">]
val tail : list : IIndexedList<'T> -> IIndexedList<'T>

/// <summary>
/// Returns the only element of the list, or <c>ValueNone</c> when it does not have exactly one element.
/// </summary>
/// <param name="list">The input list.</param>
/// <returns>The only element, or <c>ValueNone</c>.</returns>
/// <remarks>
/// O(1).
/// </remarks>
[<CompiledName "TryExactlyOne">]
val tryExactlyOne : list : IIndexedList<'T> -> 'T voption

/// <summary>
/// Returns the first element of the list, or <c>ValueNone</c> when it is empty.
/// </summary>
/// <param name="list">The input list.</param>
/// <returns>The first element, or <c>ValueNone</c>.</returns>
/// <remarks>
/// O(log n).
/// </remarks>
[<CompiledName "TryHead">]
val tryHead : list : IIndexedList<'T> -> 'T voption

/// <summary>
/// Returns the element at an index, or <c>ValueNone</c> when the index is out of range.
/// </summary>
/// <param name="index">The index of the element.</param>
/// <param name="list">The input list.</param>
/// <returns>The element at the index, or <c>ValueNone</c>.</returns>
/// <remarks>
/// O(log n).
/// </remarks>
[<CompiledName "TryItem">]
val tryItem : index : int -> list : IIndexedList<'T> -> 'T voption

/// <summary>
/// Returns the last element of the list, or <c>ValueNone</c> when it is empty.
/// </summary>
/// <param name="list">The input list.</param>
/// <returns>The last element, or <c>ValueNone</c>.</returns>
/// <remarks>
/// O(log n).
/// </remarks>
[<CompiledName "TryLast">]
val tryLast : list : IIndexedList<'T> -> 'T voption

/// <summary>
/// Returns the list without its first element, or <c>ValueNone</c> when it is empty.
/// </summary>
/// <param name="list">The input list.</param>
/// <returns>The elements after the first one, in a list of the implementation of the input, or <c>ValueNone</c>.</returns>
/// <remarks>
/// O(log n).
/// </remarks>
[<CompiledName "TryTail">]
val tryTail : list : IIndexedList<'T> -> IIndexedList<'T> voption

/// <summary>
/// Returns the first element and the other elements, or <c>ValueNone</c> when the list is empty.
/// </summary>
/// <param name="list">The input list.</param>
/// <returns>The first element and the elements after it, or <c>ValueNone</c>.</returns>
/// <remarks>
/// O(log n).
/// </remarks>
[<CompiledName "TryUncons">]
val tryUncons : list : IIndexedList<'T> -> struct ('T * IIndexedList<'T>) voption

/// <summary>
/// Returns the first element and the other elements.
/// </summary>
/// <param name="list">The input list.</param>
/// <returns>The first element and the elements after it, in a list of the implementation of the input.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the list is empty.</exception>
/// <remarks>
/// O(log n). The function throws what <see cref="M:FSharp.Collections.Immutable.IIndexedList.Head``1(System.Collections.Immutable.IImmutableList{``0})"/>
/// throws.
/// </remarks>
[<CompiledName "Uncons">]
val uncons : list : IIndexedList<'T> -> struct ('T * IIndexedList<'T>)

/// <summary>
/// Matches a list with a first element, and returns that element and the other elements.
/// </summary>
/// <param name="list">The input list.</param>
/// <returns>The first element and the elements after it, or <c>ValueNone</c> for an empty list.</returns>
/// <remarks>
/// O(log n). A struct partial pattern, so that a match allocates no option.
/// </remarks>
[<return : Struct>]
[<CompiledName "ConsPattern">]
val (|Cons|_|) : list : IIndexedList<'T> -> struct ('T * IIndexedList<'T>) voption

/// <summary>Matches an empty list.</summary>
/// <param name="list">The input list.</param>
/// <returns><see langword="true"/> when the list is empty.</returns>
/// <remarks>O(1).</remarks>
[<CompiledName "NilPattern">]
val (|Nil|_|) : list : IIndexedList<'T> -> bool

// ---- Search ----

/// <summary>
/// Returns whether the list contains an element equal to a value, in F# structural equality.
/// </summary>
/// <param name="value">The value to find.</param>
/// <param name="source">The input list.</param>
/// <returns><see langword="true"/> when an element equals the value.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Contains">]
val contains : value : 'T -> source : IIndexedList<'T> -> bool when 'T : equality

/// <summary>Returns whether any element satisfies the predicate.</summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="list">The input list.</param>
/// <returns><see langword="true"/> when an element satisfies the predicate.</returns>
/// <remarks>O(n).</remarks>
[<CompiledName "Exists">]
val exists : predicate : ('T -> bool) -> list : IIndexedList<'T> -> bool

/// <summary>
/// Returns whether any pair of elements at the same index satisfies the predicate.
/// </summary>
/// <param name="predicate">The function to test the pairs.</param>
/// <param name="list1">The first list.</param>
/// <param name="list2">The second list.</param>
/// <returns><see langword="true"/> when a pair satisfies the predicate.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when no pair satisfies the predicate and the lists have different lengths.</exception>
/// <remarks>
/// O(n). Two indexed lists are walked together, and the walk stops at the first pair that satisfies the predicate; the
/// other implementations are copied first.
/// </remarks>
[<CompiledName "Exists2">]
val exists2 : predicate : ('T1 -> 'T2 -> bool) -> list1 : IIndexedList<'T1> -> list2 : IIndexedList<'T2> -> bool

/// <summary>Returns the first element that satisfies the predicate.</summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="list">The input list.</param>
/// <returns>The first element that satisfies the predicate.</returns>
/// <exception cref="T:System.Collections.Generic.KeyNotFoundException">Thrown when no element satisfies the predicate.</exception>
/// <remarks>O(n).</remarks>
[<CompiledName "Find">]
val find : predicate : ('T -> bool) -> list : IIndexedList<'T> -> 'T

/// <summary>Returns the last element that satisfies the predicate.</summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="list">The input list.</param>
/// <returns>The last element that satisfies the predicate.</returns>
/// <exception cref="T:System.Collections.Generic.KeyNotFoundException">Thrown when no element satisfies the predicate.</exception>
/// <remarks>
/// O(n). An indexed list is searched from its end, and the other implementations in a copy.
/// </remarks>
[<CompiledName "FindBack">]
val findBack : predicate : ('T -> bool) -> list : IIndexedList<'T> -> 'T

/// <summary>
/// Returns the index of the first element that satisfies the predicate.
/// </summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="list">The input list.</param>
/// <returns>The index of the first element that satisfies the predicate.</returns>
/// <exception cref="T:System.Collections.Generic.KeyNotFoundException">Thrown when no element satisfies the predicate.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "FindIndex">]
val findIndex : predicate : ('T -> bool) -> list : IIndexedList<'T> -> int

/// <summary>
/// Returns the index of the last element that satisfies the predicate.
/// </summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="list">The input list.</param>
/// <returns>The index of the last element that satisfies the predicate.</returns>
/// <exception cref="T:System.Collections.Generic.KeyNotFoundException">Thrown when no element satisfies the predicate.</exception>
/// <remarks>
/// O(n). An indexed list is searched from its end, and the other implementations in a copy.
/// </remarks>
[<CompiledName "FindIndexBack">]
val findIndexBack : predicate : ('T -> bool) -> list : IIndexedList<'T> -> int

/// <summary>Returns whether all elements satisfy the predicate.</summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="list">The input list.</param>
/// <returns><see langword="true"/> when every element satisfies the predicate.</returns>
/// <remarks>O(n).</remarks>
[<CompiledName "ForAll">]
val forall : predicate : ('T -> bool) -> list : IIndexedList<'T> -> bool

/// <summary>
/// Returns whether all pairs of elements at the same index satisfy the predicate.
/// </summary>
/// <param name="predicate">The function to test the pairs.</param>
/// <param name="list1">The first list.</param>
/// <param name="list2">The second list.</param>
/// <returns><see langword="true"/> when every pair satisfies the predicate.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when every pair satisfies the predicate and the lists have different lengths.</exception>
/// <remarks>
/// O(n). Two indexed lists are walked together, and the walk stops at the first pair that does not satisfy the predicate;
/// the other implementations are copied first.
/// </remarks>
[<CompiledName "ForAll2">]
val forall2 : predicate : ('T1 -> 'T2 -> bool) -> list1 : IIndexedList<'T1> -> list2 : IIndexedList<'T2> -> bool

/// <summary>
/// Returns the index of the first element equal to a value, in <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>.
/// </summary>
/// <param name="value">The value to find.</param>
/// <param name="list">The input list.</param>
/// <returns>The index of the first equal element, or -1.</returns>
/// <remarks>
/// O(n). The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableList.IndexOf``1(System.Collections.Immutable.IImmutableList{``0},``0)"/>.
/// </remarks>
[<CompiledName "IndexOf">]
val indexOf : value : 'T -> list : IIndexedList<'T> -> int

/// <summary>
/// Returns the index of the first element of a range that equals a value, in
/// <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>.
/// </summary>
/// <param name="startIndex">The index of the first element of the range.</param>
/// <param name="count">The number of elements of the range.</param>
/// <param name="value">The value to find.</param>
/// <param name="list">The input list.</param>
/// <returns>The index in the list, not in the range, of the first equal element of the range, or -1.</returns>
/// <exception cref="T:System.ArgumentOutOfRangeException">
/// Thrown when <paramref name="startIndex"/> or <paramref name="count"/> is negative, or the range extends beyond the end
/// of the list.
/// </exception>
/// <remarks>
/// O(log n + count). The function checks the range for every implementation of the interface, and wraps
/// <see cref="M:System.Collections.Immutable.ImmutableList.IndexOf``1(System.Collections.Immutable.IImmutableList{``0},``0,System.Int32,System.Int32)"/>.
/// </remarks>
[<CompiledName "IndexOfRange">]
val indexOfRange : startIndex : int -> count : int -> value : 'T -> list : IIndexedList<'T> -> int

/// <summary>
/// Returns the index of the first element of a range that a given equality comparer finds equal to a value.
/// </summary>
/// <param name="comparer">The equality comparer.</param>
/// <param name="startIndex">The index of the first element of the range.</param>
/// <param name="count">The number of elements of the range.</param>
/// <param name="value">The value to find.</param>
/// <param name="list">The input list.</param>
/// <returns>The index in the list, not in the range, of the first equal element of the range, or -1.</returns>
/// <exception cref="T:System.ArgumentOutOfRangeException">
/// Thrown when <paramref name="startIndex"/> or <paramref name="count"/> is negative, or the range extends beyond the end
/// of the list.
/// </exception>
/// <remarks>
/// O(log n + count). The function checks the range for every implementation of the interface, and wraps
/// <see cref="M:System.Collections.Immutable.IImmutableList`1.IndexOf(`0,System.Int32,System.Int32,System.Collections.Generic.IEqualityComparer{`0})"/>.
/// </remarks>
[<CompiledName "IndexOfRangeWith">]
val indexOfRangeWith :
    comparer : IEqualityComparer<'T> -> startIndex : int -> count : int -> value : 'T -> list : IIndexedList<'T> -> int

/// <summary>
/// Returns the index of the first element equal to a value in a given equality comparer.
/// </summary>
/// <param name="comparer">The equality comparer.</param>
/// <param name="value">The value to find.</param>
/// <param name="list">The input list.</param>
/// <returns>The index of the first equal element, or -1.</returns>
/// <remarks>
/// O(n). The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableList.IndexOf``1(System.Collections.Immutable.IImmutableList{``0},``0,System.Collections.Generic.IEqualityComparer{``0})"/>.
/// </remarks>
[<CompiledName "IndexOfWith">]
val indexOfWith : comparer : IEqualityComparer<'T> -> value : 'T -> list : IIndexedList<'T> -> int

/// <summary>
/// Returns the index of the last element equal to a value, in <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>.
/// </summary>
/// <param name="value">The value to find.</param>
/// <param name="list">The input list.</param>
/// <returns>The index of the last equal element, or -1.</returns>
/// <remarks>
/// O(n). The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableList.LastIndexOf``1(System.Collections.Immutable.IImmutableList{``0},``0)"/>.
/// </remarks>
[<CompiledName "LastIndexOf">]
val lastIndexOf : value : 'T -> list : IIndexedList<'T> -> int

/// <summary>
/// Returns the index of the last element of a range that equals a value, in
/// <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>. The search walks backward from the index of the
/// last element of the range.
/// </summary>
/// <param name="startIndex">The index of the last element of the range, where the search starts.</param>
/// <param name="count">The number of elements of the range, which ends at <paramref name="startIndex"/>.</param>
/// <param name="value">The value to find.</param>
/// <param name="list">The input list.</param>
/// <returns>The index in the list, not in the range, of the last equal element of the range, or -1.</returns>
/// <exception cref="T:System.ArgumentOutOfRangeException">
/// Thrown when <paramref name="startIndex"/> or <paramref name="count"/> is negative, <paramref name="count"/> exceeds the
/// length, or a range of elements does not lie in the list: its first element, at <c>startIndex - count + 1</c>, is before
/// the start, or its last element, at <paramref name="startIndex"/>, is at or after the length.
/// </exception>
/// <remarks>
/// O(log n + count). A count of 0 is an empty range, which has no equal element, for any start index. The function checks
/// the range for every implementation of the interface, and wraps
/// <see cref="M:System.Collections.Immutable.ImmutableList.LastIndexOf``1(System.Collections.Immutable.IImmutableList{``0},``0,System.Int32,System.Int32)"/>.
/// </remarks>
[<CompiledName "LastIndexOfRange">]
val lastIndexOfRange : startIndex : int -> count : int -> value : 'T -> list : IIndexedList<'T> -> int

/// <summary>
/// Returns the index of the last element of a range that a given equality comparer finds equal to a value. The search
/// walks backward from the index of the last element of the range.
/// </summary>
/// <param name="comparer">The equality comparer.</param>
/// <param name="startIndex">The index of the last element of the range, where the search starts.</param>
/// <param name="count">The number of elements of the range, which ends at <paramref name="startIndex"/>.</param>
/// <param name="value">The value to find.</param>
/// <param name="list">The input list.</param>
/// <returns>The index in the list, not in the range, of the last equal element of the range, or -1.</returns>
/// <exception cref="T:System.ArgumentOutOfRangeException">
/// Thrown when <paramref name="startIndex"/> or <paramref name="count"/> is negative, <paramref name="count"/> exceeds the
/// length, or a range of elements does not lie in the list: its first element, at <c>startIndex - count + 1</c>, is before
/// the start, or its last element, at <paramref name="startIndex"/>, is at or after the length.
/// </exception>
/// <remarks>
/// O(log n + count). A count of 0 is an empty range, which has no equal element, for any start index. The function checks
/// the range for every implementation of the interface, and wraps
/// <see cref="M:System.Collections.Immutable.IImmutableList`1.LastIndexOf(`0,System.Int32,System.Int32,System.Collections.Generic.IEqualityComparer{`0})"/>.
/// </remarks>
[<CompiledName "LastIndexOfRangeWith">]
val lastIndexOfRangeWith :
    comparer : IEqualityComparer<'T> -> startIndex : int -> count : int -> value : 'T -> list : IIndexedList<'T> -> int

/// <summary>
/// Returns the index of the last element equal to a value in a given equality comparer.
/// </summary>
/// <param name="comparer">The equality comparer.</param>
/// <param name="value">The value to find.</param>
/// <param name="list">The input list.</param>
/// <returns>The index of the last equal element, or -1.</returns>
/// <remarks>
/// O(n). The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableList.LastIndexOf``1(System.Collections.Immutable.IImmutableList{``0},``0,System.Collections.Generic.IEqualityComparer{``0})"/>.
/// </remarks>
[<CompiledName "LastIndexOfWith">]
val lastIndexOfWith : comparer : IEqualityComparer<'T> -> value : 'T -> list : IIndexedList<'T> -> int

/// <summary>
/// Returns the first value that the chooser returns for the elements.
/// </summary>
/// <param name="chooser">The function that returns a value, or <c>ValueNone</c> to go on.</param>
/// <param name="list">The input list.</param>
/// <returns>The first chosen value.</returns>
/// <exception cref="T:System.Collections.Generic.KeyNotFoundException">Thrown when the chooser returns <c>ValueNone</c> for every element.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Pick">]
val pick : chooser : ('T -> 'U voption) -> list : IIndexedList<'T> -> 'U

/// <summary>
/// Returns the first element that satisfies the predicate, or <c>ValueNone</c>.
/// </summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="list">The input list.</param>
/// <returns>The first element that satisfies the predicate, or <c>ValueNone</c>.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "TryFind">]
val tryFind : predicate : ('T -> bool) -> list : IIndexedList<'T> -> 'T voption

/// <summary>
/// Returns the last element that satisfies the predicate, or <c>ValueNone</c>.
/// </summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="list">The input list.</param>
/// <returns>The last element that satisfies the predicate, or <c>ValueNone</c>.</returns>
/// <remarks>
/// O(n). An indexed list is searched from its end, and the other implementations in a copy.
/// </remarks>
[<CompiledName "TryFindBack">]
val tryFindBack : predicate : ('T -> bool) -> list : IIndexedList<'T> -> 'T voption

/// <summary>
/// Returns the index of the first element that satisfies the predicate, or <c>ValueNone</c>.
/// </summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="list">The input list.</param>
/// <returns>The index of the first element that satisfies the predicate, or <c>ValueNone</c>.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "TryFindIndex">]
val tryFindIndex : predicate : ('T -> bool) -> list : IIndexedList<'T> -> int voption

/// <summary>
/// Returns the index of the last element that satisfies the predicate, or <c>ValueNone</c>.
/// </summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="list">The input list.</param>
/// <returns>The index of the last element that satisfies the predicate, or <c>ValueNone</c>.</returns>
/// <remarks>
/// O(n). An indexed list is searched from its end, and the other implementations in a copy.
/// </remarks>
[<CompiledName "TryFindIndexBack">]
val tryFindIndexBack : predicate : ('T -> bool) -> list : IIndexedList<'T> -> int voption

/// <summary>
/// Returns the first value that the chooser returns for the elements, or <c>ValueNone</c>.
/// </summary>
/// <param name="chooser">The function that returns a value, or <c>ValueNone</c> to go on.</param>
/// <param name="list">The input list.</param>
/// <returns>The first chosen value, or <c>ValueNone</c>.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "TryPick">]
val tryPick : chooser : ('T -> 'U voption) -> list : IIndexedList<'T> -> 'U voption

// ---- Transformation ----

/// <summary>
/// Returns a list with a value added after the last element.
/// </summary>
/// <param name="value">The value to add.</param>
/// <param name="list">The input list.</param>
/// <returns>A list of the implementation of the input with the elements and the value.</returns>
/// <remarks>
/// O(log n). The function wraps <see cref="M:System.Collections.Immutable.IImmutableList`1.Add(`0)"/>.
/// </remarks>
[<CompiledName "Add">]
val add : value : 'T -> list : IIndexedList<'T> -> IIndexedList<'T>

/// <summary>
/// Returns every element of the first list paired with every element of the second one.
/// </summary>
/// <param name="list1">The first list.</param>
/// <param name="list2">The second list.</param>
/// <returns>The pairs, in the order of the first list and then of the second one.</returns>
/// <remarks>
/// O(n × m).
/// </remarks>
[<CompiledName "AllPairs">]
val allPairs : list1 : IIndexedList<'T1> -> list2 : IIndexedList<'T2> -> IndexedList<struct ('T1 * 'T2)>

/// <summary>
/// Returns a list of the elements of the first list followed by the elements of the second one.
/// </summary>
/// <param name="list1">The first list.</param>
/// <param name="list2">The second list.</param>
/// <returns>The concatenation, in a list of the implementation of the first list.</returns>
/// <remarks>
/// O(m log(n + m)). The function wraps
/// <see cref="M:System.Collections.Immutable.IImmutableList`1.AddRange(System.Collections.Generic.IEnumerable{`0})"/>
/// of the first list.
/// </remarks>
[<CompiledName "Append">]
val append : list1 : IIndexedList<'T> -> list2 : IIndexedList<'T> -> IIndexedList<'T>

/// <summary>
/// Returns the values that the chooser returns for the elements.
/// </summary>
/// <param name="chooser">The function that returns a value, or <c>ValueNone</c> to skip the element.</param>
/// <param name="list">The input list.</param>
/// <returns>The chosen values.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Choose">]
val choose : chooser : ('T -> 'U voption) -> list : IIndexedList<'T> -> IndexedList<'U>

/// <summary>Divides the list into chunks of a given size.</summary>
/// <param name="chunkSize">The maximum size of a chunk.</param>
/// <param name="list">The input list.</param>
/// <returns>The chunks; the last one can be smaller.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when <paramref name="chunkSize"/> is not positive.</exception>
/// <remarks>O(n).</remarks>
[<CompiledName "ChunkBySize">]
val chunkBySize : chunkSize : int -> list : IIndexedList<'T> -> IndexedList<IndexedList<'T>>

/// <summary>
/// Concatenates the lists that a function returns for the elements.
/// </summary>
/// <param name="mapping">The function that returns a list for each element.</param>
/// <param name="list">The input list.</param>
/// <returns>The concatenation of the returned lists.</returns>
/// <remarks>
/// O(n + the total length of the returned lists that have fewer than 16 elements), and O(log m) for each returned
/// indexed list of at least 16 elements, where m is the length of the result: the result shares the nodes of such a list.
/// </remarks>
[<CompiledName "Collect">]
val collect : mapping : ('T -> IIndexedList<'U>) -> list : IIndexedList<'T> -> IndexedList<'U>

/// <summary>Concatenates a sequence of lists.</summary>
/// <param name="lists">The lists.</param>
/// <returns>The concatenation, in an indexed list, because the lists can have different implementations.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="lists"/> is <see langword="null"/> or a default flat list, or one of its lists is <see langword="null"/> or a default flat list.</exception>
/// <remarks>
/// O(the total length of the lists that have fewer than 16 elements, and of the lists that are not indexed lists), and
/// O(log m) for each indexed list of at least 16 elements, where m is the length of the result: the result shares the
/// nodes of such a list.
/// </remarks>
[<CompiledName "Concat">]
val concat : lists : seq<IIndexedList<'T>> -> IndexedList<'T>

/// <summary>
/// Returns a list with a value added before the first element.
/// </summary>
/// <param name="value">The value to add.</param>
/// <param name="list">The input list.</param>
/// <returns>A list of the implementation of the input with the value and the elements.</returns>
/// <remarks>
/// O(log n). The function wraps <see cref="M:System.Collections.Immutable.IImmutableList`1.Insert(System.Int32,`0)"/>.
/// </remarks>
[<CompiledName "Cons">]
val cons : value : 'T -> list : IIndexedList<'T> -> IIndexedList<'T>

/// <summary>Returns the elements that satisfy the predicate.</summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="list">The input list.</param>
/// <returns>The elements that satisfy the predicate, in their order, in a list of the implementation of the input.</returns>
/// <remarks>O(n).</remarks>
[<CompiledName "Filter">]
val filter : predicate : ('T -> bool) -> list : IIndexedList<'T> -> IIndexedList<'T>

/// <summary>Returns every element with its index.</summary>
/// <param name="list">The input list.</param>
/// <returns>The pairs of index and element.</returns>
/// <remarks>O(n).</remarks>
[<CompiledName "Indexed">]
val indexed : list : IIndexedList<'T> -> IndexedList<struct (int * 'T)>

/// <summary>
/// Returns a list with a value inserted at an index.
/// </summary>
/// <param name="index">The index of the inserted value, from 0 to the length.</param>
/// <param name="value">The value to insert.</param>
/// <param name="source">The input list.</param>
/// <returns>A list of the implementation of the input with the value.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the index is negative or greater than the length.</exception>
/// <remarks>
/// O(log n). The function wraps <see cref="M:System.Collections.Immutable.IImmutableList`1.Insert(System.Int32,`0)"/>.
/// </remarks>
[<CompiledName "InsertAt">]
val insertAt : index : int -> value : 'T -> source : IIndexedList<'T> -> IIndexedList<'T>

/// <summary>
/// Returns a list with values inserted at an index.
/// </summary>
/// <param name="index">The index of the first inserted value, from 0 to the length.</param>
/// <param name="values">The values to insert.</param>
/// <param name="source">The input list.</param>
/// <returns>A list of the implementation of the input with the values.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the index is negative or greater than the length.</exception>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="values"/> is <see langword="null"/> or a default flat list.</exception>
/// <remarks>
/// O(m log(n + m)). The function wraps
/// <see cref="M:System.Collections.Immutable.IImmutableList`1.InsertRange(System.Int32,System.Collections.Generic.IEnumerable{`0})"/>.
/// </remarks>
[<CompiledName "InsertManyAt">]
val insertManyAt : index : int -> values : seq<'T> -> source : IIndexedList<'T> -> IIndexedList<'T>

/// <summary>Returns the results of a function applied to each element.</summary>
/// <param name="mapping">The function to transform the elements, called in the order of the elements.</param>
/// <param name="list">The input list.</param>
/// <returns>The results, in the order of the elements.</returns>
/// <remarks>O(n).</remarks>
[<CompiledName "Map">]
val map : mapping : ('T -> 'U) -> list : IIndexedList<'T> -> IndexedList<'U>

/// <summary>
/// Returns the results of a function applied to the pairs of elements at the same index.
/// </summary>
/// <param name="mapping">The function to transform the pairs.</param>
/// <param name="list1">The first list.</param>
/// <param name="list2">The second list.</param>
/// <returns>The results.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the lists have different lengths.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Map2">]
val map2 : mapping : ('T1 -> 'T2 -> 'U) -> list1 : IIndexedList<'T1> -> list2 : IIndexedList<'T2> -> IndexedList<'U>

/// <summary>
/// Returns the results of a function applied to the triples of elements at the same index.
/// </summary>
/// <param name="mapping">The function to transform the triples.</param>
/// <param name="list1">The first list.</param>
/// <param name="list2">The second list.</param>
/// <param name="list3">The third list.</param>
/// <returns>The results.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the lists have different lengths.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Map3">]
val map3 :
    mapping : ('T1 -> 'T2 -> 'T3 -> 'U) ->
    list1 : IIndexedList<'T1> ->
    list2 : IIndexedList<'T2> ->
    list3 : IIndexedList<'T3> ->
        IndexedList<'U>

/// <summary>
/// Returns the results of a function applied to each element and its index.
/// </summary>
/// <param name="mapping">The function to transform the elements.</param>
/// <param name="list">The input list.</param>
/// <returns>The results.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "MapIndexed">]
val mapi : mapping : (int -> 'T -> 'U) -> list : IIndexedList<'T> -> IndexedList<'U>

/// <summary>
/// Returns the results of a function applied to the index and the pair of elements at that index.
/// </summary>
/// <param name="mapping">The function to transform the pairs.</param>
/// <param name="list1">The first list.</param>
/// <param name="list2">The second list.</param>
/// <returns>The results.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the lists have different lengths.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "MapIndexed2">]
val mapi2 : mapping : (int -> 'T1 -> 'T2 -> 'U) -> list1 : IIndexedList<'T1> -> list2 : IIndexedList<'T2> -> IndexedList<'U>

/// <summary>Returns every element with its successor.</summary>
/// <param name="list">The input list.</param>
/// <returns>The pairs of neighbours; empty for fewer than two elements.</returns>
/// <remarks>O(n).</remarks>
[<CompiledName "Pairwise">]
val pairwise : list : IIndexedList<'T> -> IndexedList<struct ('T * 'T)>

/// <summary>
/// Returns the elements moved to the indices that a function computes.
/// </summary>
/// <param name="indexMap">The function that maps the index of each element to its new index.</param>
/// <param name="list">The input list.</param>
/// <returns>The permuted elements, in a list of the implementation of the input.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the function does not compute a permutation.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Permute">]
val permute : indexMap : (int -> int) -> list : IIndexedList<'T> -> IIndexedList<'T>

/// <summary>
/// Returns a list without the first element that equals a value, in
/// <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>.
/// </summary>
/// <param name="item">The value to remove.</param>
/// <param name="list">The input list.</param>
/// <returns>A list of the implementation of the input without the element.</returns>
/// <remarks>
/// <para>
/// O(n). An <see cref="T:System.Collections.Immutable.ImmutableList`1"/> that has no equal element comes back as it is.
/// The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableList.Remove``1(System.Collections.Immutable.IImmutableList{``0},``0)"/>.
/// </para>
/// <para>
/// To remove several values, use
/// <see cref="M:FSharp.Collections.Immutable.IIndexedList.RemoveMany``1(System.Collections.Generic.IEnumerable{``0},System.Collections.Immutable.IImmutableList{``0})"/>;
/// to remove by index, use
/// <see cref="M:FSharp.Collections.Immutable.IIndexedList.RemoveAt``1(System.Int32,System.Collections.Immutable.IImmutableList{``0})"/>;
/// to remove the elements that satisfy a predicate, use
/// <see cref="M:FSharp.Collections.Immutable.IIndexedList.Filter``1(Microsoft.FSharp.Core.FSharpFunc{``0,System.Boolean},System.Collections.Immutable.IImmutableList{``0})"/>
/// with the negated predicate.
/// </para>
/// </remarks>
[<CompiledName "Remove">]
val remove : item : 'T -> list : IIndexedList<'T> -> IIndexedList<'T>

/// <summary>
/// Returns a list without the element at an index.
/// </summary>
/// <param name="index">The index of the removed element.</param>
/// <param name="source">The input list.</param>
/// <returns>A list of the implementation of the input without the element.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the index is out of range.</exception>
/// <remarks>
/// O(log n). The function wraps <see cref="M:System.Collections.Immutable.IImmutableList`1.RemoveAt(System.Int32)"/>.
/// </remarks>
[<CompiledName "RemoveAt">]
val removeAt : index : int -> source : IIndexedList<'T> -> IIndexedList<'T>

/// <summary>
/// Returns a list without one element for each of the given values, in
/// <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>.
/// </summary>
/// <param name="items">The values to remove.</param>
/// <param name="list">The input list.</param>
/// <returns>A list of the implementation of the input without the elements.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="items"/> is <see langword="null"/> or a default flat list.</exception>
/// <remarks>
/// <para>
/// O(n × m) for m values. Each value removes the first element that equals it and that no earlier value has removed; a
/// value that equals no remaining element removes nothing, and an
/// <see cref="T:System.Collections.Immutable.ImmutableList`1"/> that has none of the values comes back as it is. The
/// function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableList.RemoveRange``1(System.Collections.Immutable.IImmutableList{``0},System.Collections.Generic.IEnumerable{``0})"/>.
/// </para>
/// <para>
/// To remove every element that equals one of the values, and the duplicates of the other elements, use
/// <see cref="M:FSharp.Collections.Immutable.IIndexedList.Except``1(System.Collections.Generic.IEnumerable{``0},System.Collections.Immutable.IImmutableList{``0})"/>.
/// </para>
/// </remarks>
[<CompiledName "RemoveMany">]
val removeMany : items : seq<'T> -> list : IIndexedList<'T> -> IIndexedList<'T>

/// <summary>
/// Returns a list without a range of elements.
/// </summary>
/// <param name="index">The index of the first removed element.</param>
/// <param name="count">
/// The number of removed elements. A count that is not positive removes nothing, and so does a count whose sum with the
/// index exceeds <see cref="F:System.Int32.MaxValue"/>, as in
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.RemoveManyAt``1(System.Int32,System.Int32,Microsoft.FSharp.Collections.FSharpList{``0})"/>.
/// </param>
/// <param name="source">The input list.</param>
/// <returns>A list of the implementation of the input without the range.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the index is negative or the range ends beyond the list.</exception>
/// <remarks>
/// O(count × log n). The function wraps
/// <see cref="M:System.Collections.Immutable.IImmutableList`1.RemoveRange(System.Int32,System.Int32)"/>.
/// </remarks>
[<CompiledName "RemoveManyAt">]
val removeManyAt : index : int -> count : int -> source : IIndexedList<'T> -> IIndexedList<'T>

/// <summary>
/// Returns a list without one element for each of the given values, in a given equality comparer.
/// </summary>
/// <param name="comparer">The equality comparer.</param>
/// <param name="items">The values to remove.</param>
/// <param name="list">The input list.</param>
/// <returns>A list of the implementation of the input without the elements.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="items"/> is <see langword="null"/> or a default flat list.</exception>
/// <remarks>
/// O(n × m) for m values. Each value removes the first element that the comparer finds equal to it and that no earlier
/// value has removed; a value that equals no remaining element removes nothing, and an
/// <see cref="T:System.Collections.Immutable.ImmutableList`1"/> that has none of the values comes back as it is. The
/// function wraps
/// <see cref="M:System.Collections.Immutable.IImmutableList`1.RemoveRange(System.Collections.Generic.IEnumerable{`0},System.Collections.Generic.IEqualityComparer{`0})"/>.
/// </remarks>
[<CompiledName "RemoveManyWith">]
val removeManyWith : comparer : IEqualityComparer<'T> -> items : seq<'T> -> list : IIndexedList<'T> -> IIndexedList<'T>

/// <summary>
/// Returns a list without the first element that equals a value in a given equality comparer.
/// </summary>
/// <param name="comparer">The equality comparer.</param>
/// <param name="item">The value to remove.</param>
/// <param name="list">The input list.</param>
/// <returns>A list of the implementation of the input without the element.</returns>
/// <remarks>
/// O(n). An <see cref="T:System.Collections.Immutable.ImmutableList`1"/> that has no equal element comes back as it is.
/// The function wraps
/// <see cref="M:System.Collections.Immutable.IImmutableList`1.Remove(`0,System.Collections.Generic.IEqualityComparer{`0})"/>.
/// </remarks>
[<CompiledName "RemoveWith">]
val removeWith : comparer : IEqualityComparer<'T> -> item : 'T -> list : IIndexedList<'T> -> IIndexedList<'T>

/// <summary>
/// Returns a list with the first element that equals a value replaced by another value, in
/// <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>.
/// </summary>
/// <param name="oldValue">The value to replace.</param>
/// <param name="newValue">The new value.</param>
/// <param name="list">The input list.</param>
/// <returns>A list of the implementation of the input with the new value in place of the element.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when no element equals <paramref name="oldValue"/>.</exception>
/// <remarks>
/// <para>
/// O(n). The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableList.Replace``1(System.Collections.Immutable.IImmutableList{``0},``0,``0)"/>.
/// </para>
/// <para>
/// To replace every element that equals a value, use
/// <see cref="M:FSharp.Collections.Immutable.IIndexedList.Map``2(Microsoft.FSharp.Core.FSharpFunc{``0,``1},System.Collections.Immutable.IImmutableList{``0})"/>;
/// to replace the element at an index, use
/// <see cref="M:FSharp.Collections.Immutable.IIndexedList.UpdateAt``1(System.Int32,``0,System.Collections.Immutable.IImmutableList{``0})"/>.
/// </para>
/// </remarks>
[<CompiledName "Replace">]
val replace : oldValue : 'T -> newValue : 'T -> list : IIndexedList<'T> -> IIndexedList<'T>

/// <summary>
/// Returns a list with the first element that equals a value in a given equality comparer replaced by another value.
/// </summary>
/// <param name="comparer">The equality comparer.</param>
/// <param name="oldValue">The value to replace.</param>
/// <param name="newValue">The new value.</param>
/// <param name="list">The input list.</param>
/// <returns>A list of the implementation of the input with the new value in place of the element.</returns>
/// <exception cref="T:System.ArgumentException">
/// Thrown when the comparer finds no element equal to <paramref name="oldValue"/>.
/// </exception>
/// <remarks>
/// O(n). The function wraps
/// <see cref="M:System.Collections.Immutable.IImmutableList`1.Replace(`0,`0,System.Collections.Generic.IEqualityComparer{`0})"/>.
/// </remarks>
[<CompiledName "ReplaceWith">]
val replaceWith :
    comparer : IEqualityComparer<'T> -> oldValue : 'T -> newValue : 'T -> list : IIndexedList<'T> -> IIndexedList<'T>

/// <summary>
/// Returns the elements in reverse order.
/// </summary>
/// <param name="list">The input list.</param>
/// <returns>The reversed elements, in a list of the implementation of the input.</returns>
/// <remarks>
/// O(n). The function copies the elements to an array and reverses it with
/// <see cref="M:System.Array.Reverse``1(``0[])"/>, as
/// <see cref="M:FSharp.Collections.Immutable.IndexedList.Reverse``1(System.Collections.Immutable.ImmutableList{``0})"/>
/// does. Then it adds the reversed elements with
/// <see cref="M:System.Collections.Immutable.IImmutableList`1.AddRange(System.Collections.Generic.IEnumerable{`0})"/> to
/// the empty list that <see cref="M:System.Collections.Immutable.IImmutableList`1.Clear"/> of the input returns.
/// </remarks>
[<CompiledName "Reverse">]
val rev : list : IIndexedList<'T> -> IIndexedList<'T>

/// <summary>
/// Returns the transpose of a sequence of lists of the same length.
/// </summary>
/// <param name="lists">The lists.</param>
/// <returns>An indexed list whose element i holds the element i of every input list.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="lists"/> is <see langword="null"/> or a default flat list, or one of its lists is <see langword="null"/> or a default flat list.</exception>
/// <exception cref="T:System.ArgumentException">Thrown when the lists have different lengths.</exception>
/// <remarks>
/// O(the total length).
/// </remarks>
[<CompiledName "Transpose">]
val transpose : lists : seq<IIndexedList<'T>> -> IndexedList<IndexedList<'T>>

/// <summary>Splits a list of pairs into two indexed lists.</summary>
/// <param name="list">The input list.</param>
/// <returns>The first components and the second components.</returns>
/// <remarks>O(n).</remarks>
[<CompiledName "Unzip">]
val unzip : list : IIndexedList<struct ('T1 * 'T2)> -> struct (IndexedList<'T1> * IndexedList<'T2>)

/// <summary>Splits a list of triples into three indexed lists.</summary>
/// <param name="list">The input list.</param>
/// <returns>The first, the second and the third components.</returns>
/// <remarks>O(n).</remarks>
[<CompiledName "Unzip3">]
val unzip3 : list : IIndexedList<struct ('T1 * 'T2 * 'T3)> -> struct (IndexedList<'T1> * IndexedList<'T2> * IndexedList<'T3>)

/// <summary>
/// Returns a list with the element at an index replaced by a value.
/// </summary>
/// <param name="index">The index of the replaced element.</param>
/// <param name="value">The new value.</param>
/// <param name="source">The input list.</param>
/// <returns>A list of the implementation of the input with the new value.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the index is out of range.</exception>
/// <remarks>
/// O(log n). The function wraps <see cref="M:System.Collections.Immutable.IImmutableList`1.SetItem(System.Int32,`0)"/>.
/// </remarks>
[<CompiledName "UpdateAt">]
val updateAt : index : int -> value : 'T -> source : IIndexedList<'T> -> IIndexedList<'T>

/// <summary>
/// Returns the elements that satisfy the predicate.
/// </summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="list">The input list.</param>
/// <returns>The elements that satisfy the predicate, in their order, in a list of the implementation of the input.</returns>
/// <remarks>
/// O(n). A synonym of <see cref="M:FSharp.Collections.Immutable.IIndexedList.Filter``1(Microsoft.FSharp.Core.FSharpFunc{``0,System.Boolean},System.Collections.Immutable.IImmutableList{``0})"/>.
/// </remarks>
[<CompiledName "Where">]
val where : predicate : ('T -> bool) -> list : IIndexedList<'T> -> IIndexedList<'T>

/// <summary>
/// Returns the windows of a given size that slide over the list.
/// </summary>
/// <param name="windowSize">The size of a window.</param>
/// <param name="list">The input list.</param>
/// <returns>The windows; empty when the list is shorter than a window.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when <paramref name="windowSize"/> is not positive.</exception>
/// <remarks>
/// O(n × windowSize).
/// </remarks>
[<CompiledName "Windowed">]
val windowed : windowSize : int -> list : IIndexedList<'T> -> IndexedList<IndexedList<'T>>

/// <summary>Combines two lists into an indexed list of pairs.</summary>
/// <param name="list1">The first list.</param>
/// <param name="list2">The second list.</param>
/// <returns>The pairs of elements at the same index.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the lists have different lengths.</exception>
/// <remarks>O(n).</remarks>
[<CompiledName "Zip">]
val zip : list1 : IIndexedList<'T1> -> list2 : IIndexedList<'T2> -> IndexedList<struct ('T1 * 'T2)>

/// <summary>Combines three lists into an indexed list of triples.</summary>
/// <param name="list1">The first list.</param>
/// <param name="list2">The second list.</param>
/// <param name="list3">The third list.</param>
/// <returns>The triples of elements at the same index.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the lists have different lengths.</exception>
/// <remarks>O(n).</remarks>
[<CompiledName "Zip3">]
val zip3 :
    list1 : IIndexedList<'T1> -> list2 : IIndexedList<'T2> -> list3 : IIndexedList<'T3> -> IndexedList<struct ('T1 * 'T2 * 'T3)>

// ---- Folding ----

/// <summary>Returns the average of the elements.</summary>
/// <param name="list">The input list.</param>
/// <returns>The average.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the list is empty.</exception>
/// <remarks>O(n).</remarks>
[<CompiledName "Average">]
val inline average :
    list : IIndexedList< ^T > -> ^T
        when ^T : (static member (+) : ^T * ^T -> ^T)
        and ^T : (static member DivideByInt : ^T * int -> ^T)
        and ^T : (static member Zero : ^T)

/// <summary>
/// Returns the average of the results of a function applied to each element.
/// </summary>
/// <param name="projection">The function to transform the elements.</param>
/// <param name="list">The input list.</param>
/// <returns>The average of the results.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the list is empty.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "AverageBy">]
val inline averageBy :
    projection : ('T -> ^U) -> list : IIndexedList<'T> -> ^U
        when ^U : (static member (+) : ^U * ^U -> ^U)
        and ^U : (static member DivideByInt : ^U * int -> ^U)
        and ^U : (static member Zero : ^U)

/// <summary>Compares two lists element by element with a comparer.</summary>
/// <param name="comparer">The function that compares two elements.</param>
/// <param name="list1">The first list.</param>
/// <param name="list2">The second list.</param>
/// <returns>
/// The first non-zero result of the comparer, or the comparison of the lengths when every common element compares equal.
/// </returns>
/// <remarks>
/// O(n). Two indexed lists are walked together, and the walk stops at the first pair of elements that the comparer does
/// not consider equal; the other implementations are copied first.
/// </remarks>
[<CompiledName "CompareWith">]
val compareWith : comparer : ('T -> 'T -> int) -> list1 : IIndexedList<'T> -> list2 : IIndexedList<'T> -> int

/// <summary>
/// Applies a function to a state and each element, from the first element to the last one.
/// </summary>
/// <param name="folder">The function that computes the next state.</param>
/// <param name="state">The initial state.</param>
/// <param name="list">The input list.</param>
/// <returns>The final state.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Fold">]
val fold : folder : ('State -> 'T -> 'State) -> state : 'State -> list : IIndexedList<'T> -> 'State

/// <summary>
/// Applies a function to a state and each pair of elements at the same index, from the first index to the last one.
/// </summary>
/// <param name="folder">The function that computes the next state.</param>
/// <param name="state">The initial state.</param>
/// <param name="list1">The first list.</param>
/// <param name="list2">The second list.</param>
/// <returns>The final state.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the lists have different lengths.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Fold2">]
val fold2 :
    folder : ('State -> 'T1 -> 'T2 -> 'State) ->
    state : 'State ->
    list1 : IIndexedList<'T1> ->
    list2 : IIndexedList<'T2> ->
        'State

/// <summary>
/// Applies a function to each element and a state, from the last element to the first one.
/// </summary>
/// <param name="folder">The function that computes the next state.</param>
/// <param name="list">The input list.</param>
/// <param name="state">The initial state.</param>
/// <returns>The final state.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "FoldBack">]
val foldBack : folder : ('T -> 'State -> 'State) -> list : IIndexedList<'T> -> state : 'State -> 'State

/// <summary>
/// Applies a function to each pair of elements at the same index and a state, from the last index to the first one.
/// </summary>
/// <param name="folder">The function that computes the next state.</param>
/// <param name="list1">The first list.</param>
/// <param name="list2">The second list.</param>
/// <param name="state">The initial state.</param>
/// <returns>The final state.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the lists have different lengths.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "FoldBack2">]
val foldBack2 :
    folder : ('T1 -> 'T2 -> 'State -> 'State) ->
    list1 : IIndexedList<'T1> ->
    list2 : IIndexedList<'T2> ->
    state : 'State ->
        'State

/// <summary>Applies an action to each element.</summary>
/// <param name="action">The action.</param>
/// <param name="list">The input list.</param>
/// <remarks>O(n).</remarks>
[<CompiledName "Iterate">]
val iter : action : ('T -> unit) -> list : IIndexedList<'T> -> unit

/// <summary>
/// Applies an action to each pair of elements at the same index.
/// </summary>
/// <param name="action">The action.</param>
/// <param name="list1">The first list.</param>
/// <param name="list2">The second list.</param>
/// <exception cref="T:System.ArgumentException">Thrown when the lists have different lengths.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Iterate2">]
val iter2 : action : ('T1 -> 'T2 -> unit) -> list1 : IIndexedList<'T1> -> list2 : IIndexedList<'T2> -> unit

/// <summary>Applies an action to each element and its index.</summary>
/// <param name="action">The action.</param>
/// <param name="list">The input list.</param>
/// <remarks>O(n).</remarks>
[<CompiledName "IterateIndexed">]
val iteri : action : (int -> 'T -> unit) -> list : IIndexedList<'T> -> unit

/// <summary>
/// Applies an action to the index and the pair of elements at that index.
/// </summary>
/// <param name="action">The action.</param>
/// <param name="list1">The first list.</param>
/// <param name="list2">The second list.</param>
/// <exception cref="T:System.ArgumentException">Thrown when the lists have different lengths.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "IterateIndexed2">]
val iteri2 : action : (int -> 'T1 -> 'T2 -> unit) -> list1 : IIndexedList<'T1> -> list2 : IIndexedList<'T2> -> unit

/// <summary>
/// Maps each element and threads a state through the mapping, from the first element to the last one.
/// </summary>
/// <param name="mapping">The function that returns the result for an element and the next state.</param>
/// <param name="state">The initial state.</param>
/// <param name="list">The input list.</param>
/// <returns>The results and the final state.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "MapFold">]
val mapFold :
    mapping : ('State -> 'T -> struct ('Result * 'State)) ->
    state : 'State ->
    list : IIndexedList<'T> ->
        struct (IndexedList<'Result> * 'State)

/// <summary>
/// Maps each element and threads a state through the mapping, from the last element to the first one.
/// </summary>
/// <param name="mapping">The function that returns the result for an element and the next state.</param>
/// <param name="list">The input list.</param>
/// <param name="state">The initial state.</param>
/// <returns>The results, in the order of the elements, and the final state.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "MapFoldBack">]
val mapFoldBack :
    mapping : ('T -> 'State -> struct ('Result * 'State)) ->
    list : IIndexedList<'T> ->
    state : 'State ->
        struct (IndexedList<'Result> * 'State)

/// <summary>Returns the greatest element, in F# generic comparison.</summary>
/// <param name="list">The input list.</param>
/// <returns>The greatest element.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the list is empty.</exception>
/// <remarks>
/// O(n). The function is inline, as <see cref="M:Microsoft.FSharp.Collections.ListModule.Max``1(Microsoft.FSharp.Collections.FSharpList{``0})"/>
/// is, so that the comparison is compiled for the type of the elements and boxes nothing.
/// </remarks>
[<CompiledName "Max">]
val inline max : list : IIndexedList<'T> -> 'T when 'T : comparison

/// <summary>
/// Returns the element with the greatest result of a projection.
/// </summary>
/// <param name="projection">The function to transform the elements for the comparison.</param>
/// <param name="list">The input list.</param>
/// <returns>The first element with the greatest result.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the list is empty.</exception>
/// <remarks>
/// O(n). The function is inline, as <see cref="M:Microsoft.FSharp.Collections.ListModule.MaxBy``2(Microsoft.FSharp.Core.FSharpFunc{``0,``1},Microsoft.FSharp.Collections.FSharpList{``0})"/>
/// is, so that the comparison is compiled for the type of the results and boxes nothing.
/// </remarks>
[<CompiledName "MaxBy">]
val inline maxBy : projection : ('T -> 'U) -> list : IIndexedList<'T> -> 'T when 'U : comparison

/// <summary>Returns the smallest element, in F# generic comparison.</summary>
/// <param name="list">The input list.</param>
/// <returns>The smallest element.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the list is empty.</exception>
/// <remarks>
/// O(n). The function is inline, as <see cref="M:Microsoft.FSharp.Collections.ListModule.Min``1(Microsoft.FSharp.Collections.FSharpList{``0})"/>
/// is, so that the comparison is compiled for the type of the elements and boxes nothing.
/// </remarks>
[<CompiledName "Min">]
val inline min : list : IIndexedList<'T> -> 'T when 'T : comparison

/// <summary>
/// Returns the element with the smallest result of a projection.
/// </summary>
/// <param name="projection">The function to transform the elements for the comparison.</param>
/// <param name="list">The input list.</param>
/// <returns>The first element with the smallest result.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the list is empty.</exception>
/// <remarks>
/// O(n). The function is inline, as <see cref="M:Microsoft.FSharp.Collections.ListModule.MinBy``2(Microsoft.FSharp.Core.FSharpFunc{``0,``1},Microsoft.FSharp.Collections.FSharpList{``0})"/>
/// is, so that the comparison is compiled for the type of the results and boxes nothing.
/// </remarks>
[<CompiledName "MinBy">]
val inline minBy : projection : ('T -> 'U) -> list : IIndexedList<'T> -> 'T when 'U : comparison

/// <summary>
/// Combines the elements with a function, from the first element to the last one.
/// </summary>
/// <param name="reduction">The function that combines the result so far with the next element.</param>
/// <param name="list">The input list.</param>
/// <returns>The combined value.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the list is empty.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Reduce">]
val reduce : reduction : ('T -> 'T -> 'T) -> list : IIndexedList<'T> -> 'T

/// <summary>
/// Combines the elements with a function, from the last element to the first one.
/// </summary>
/// <param name="reduction">The function that combines an element with the result so far.</param>
/// <param name="list">The input list.</param>
/// <returns>The combined value.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the list is empty.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "ReduceBack">]
val reduceBack : reduction : ('T -> 'T -> 'T) -> list : IIndexedList<'T> -> 'T

/// <summary>
/// Returns every state of a fold from the first element to the last one, the initial state first.
/// </summary>
/// <param name="folder">The function that computes the next state.</param>
/// <param name="state">The initial state.</param>
/// <param name="list">The input list.</param>
/// <returns>The states; one more than the elements.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Scan">]
val scan : folder : ('State -> 'T -> 'State) -> state : 'State -> list : IIndexedList<'T> -> IndexedList<'State>

/// <summary>
/// Returns every state of a fold from the last element to the first one, the initial state last.
/// </summary>
/// <param name="folder">The function that computes the next state.</param>
/// <param name="list">The input list.</param>
/// <param name="state">The initial state.</param>
/// <returns>The states; one more than the elements.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "ScanBack">]
val scanBack : folder : ('T -> 'State -> 'State) -> list : IIndexedList<'T> -> state : 'State -> IndexedList<'State>

/// <summary>Returns the sum of the elements.</summary>
/// <param name="list">The input list.</param>
/// <returns>The sum; zero for an empty list.</returns>
/// <remarks>O(n).</remarks>
[<CompiledName "Sum">]
val inline sum : list : IIndexedList< ^T > -> ^T when ^T : (static member (+) : ^T * ^T -> ^T) and ^T : (static member Zero : ^T)

/// <summary>
/// Returns the sum of the results of a function applied to each element.
/// </summary>
/// <param name="projection">The function to transform the elements.</param>
/// <param name="list">The input list.</param>
/// <returns>The sum of the results; zero for an empty list.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "SumBy">]
val inline sumBy :
    projection : ('T -> ^U) -> list : IIndexedList<'T> -> ^U
        when ^U : (static member (+) : ^U * ^U -> ^U) and ^U : (static member Zero : ^U)

// ---- Partitioning and grouping ----

/// <summary>Counts the elements of each key of a projection.</summary>
/// <param name="projection">The function that returns the key of an element.</param>
/// <param name="list">The input list.</param>
/// <returns>The keys, in the order of their first occurrence, with the number of their elements.</returns>
/// <remarks>O(n). The keys compare in F# structural equality.</remarks>
[<CompiledName "CountBy">]
val countBy : projection : ('T -> 'Key) -> list : IIndexedList<'T> -> IndexedList<struct ('Key * int)> when 'Key : equality

/// <summary>
/// Returns the elements without duplicates, in F# structural equality.
/// </summary>
/// <param name="list">The input list.</param>
/// <returns>The first occurrence of each element, in their order, in a list of the implementation of the input.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Distinct">]
val distinct : list : IIndexedList<'T> -> IIndexedList<'T> when 'T : equality

/// <summary>
/// Returns the elements without duplicates of the result of a projection.
/// </summary>
/// <param name="projection">The function that returns the key of an element.</param>
/// <param name="list">The input list.</param>
/// <returns>The first element of each key, in their order, in a list of the implementation of the input.</returns>
/// <remarks>
/// O(n). The keys compare in F# structural equality.
/// </remarks>
[<CompiledName "DistinctBy">]
val distinctBy : projection : ('T -> 'Key) -> list : IIndexedList<'T> -> IIndexedList<'T> when 'Key : equality

/// <summary>
/// Returns the distinct elements that do not equal any of the excluded items.
/// </summary>
/// <param name="itemsToExclude">The items to remove.</param>
/// <param name="list">The input list.</param>
/// <returns>The distinct remaining elements, in their order, in a list of the implementation of the input.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="itemsToExclude"/> is <see langword="null"/> or a default flat list.</exception>
/// <remarks>
/// O(n + m). An empty list is returned without a look at the items to exclude.
/// </remarks>
[<CompiledName "Except">]
val except : itemsToExclude : seq<'T> -> list : IIndexedList<'T> -> IIndexedList<'T> when 'T : equality

/// <summary>Groups the elements by the key of a projection.</summary>
/// <param name="projection">The function that returns the key of an element.</param>
/// <param name="list">The input list.</param>
/// <returns>The keys, in the order of their first occurrence, with their elements in their order.</returns>
/// <remarks>O(n). The keys compare in F# structural equality.</remarks>
[<CompiledName "GroupBy">]
val groupBy :
    projection : ('T -> 'Key) -> list : IIndexedList<'T> -> IndexedList<struct ('Key * IndexedList<'T>)> when 'Key : equality

/// <summary>Splits the elements by a predicate.</summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="list">The input list.</param>
/// <returns>
/// The elements that satisfy the predicate and the other ones, both in their order, in lists of the implementation of
/// the input.
/// </returns>
/// <remarks>O(n).</remarks>
[<CompiledName "Partition">]
val partition : predicate : ('T -> bool) -> list : IIndexedList<'T> -> struct (IIndexedList<'T> * IIndexedList<'T>)

/// <summary>
/// Splits the elements by the choice that a function returns for them.
/// </summary>
/// <param name="partitioner">The function that returns <c>Choice1Of2</c> or <c>Choice2Of2</c> for an element.</param>
/// <param name="list">The input list.</param>
/// <returns>The values of the first choices and of the second choices, both in the order of the elements.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "PartitionWith">]
val partitionWith :
    partitioner : ('T -> Choice<'T1, 'T2>) -> list : IIndexedList<'T> -> struct (IndexedList<'T1> * IndexedList<'T2>)

/// <summary>
/// Returns the list without its first elements.
/// </summary>
/// <param name="count">The number of elements to skip; a negative count skips nothing.</param>
/// <param name="list">The input list.</param>
/// <returns>The remaining elements, in a list of the implementation of the input.</returns>
/// <exception cref="T:System.ArgumentOutOfRangeException">Thrown when the count exceeds the length.</exception>
/// <remarks>
/// O(log n + k) for an indexed list, where k is the length of the result; the other implementations are read from their
/// start. The list itself is returned when nothing is skipped.
/// </remarks>
[<CompiledName "Skip">]
val skip : count : int -> list : IIndexedList<'T> -> IIndexedList<'T>

/// <summary>
/// Returns the list without its last elements.
/// </summary>
/// <param name="count">The number of elements to skip; a negative count skips nothing.</param>
/// <param name="list">The input list.</param>
/// <returns>The remaining elements, in a list of the implementation of the input.</returns>
/// <exception cref="T:System.ArgumentOutOfRangeException">Thrown when the count exceeds the length.</exception>
/// <remarks>
/// O(log n + k) for an indexed list, where k is the length of the result; the other implementations are read from their
/// start. The list itself is returned when nothing is skipped. The checks follow
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.Skip``1(System.Int32,Microsoft.FSharp.Collections.FSharpList{``0})"/>.
/// </remarks>
[<CompiledName "SkipLast">]
val skipLast : count : int -> list : IIndexedList<'T> -> IIndexedList<'T>

/// <summary>
/// Returns the list without its first elements that satisfy the predicate.
/// </summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="list">The input list.</param>
/// <returns>The elements from the first one that does not satisfy the predicate, in a list of the implementation of the input.</returns>
/// <remarks>
/// O(n + k) for an indexed list, where k is the length of the result.
/// </remarks>
[<CompiledName "SkipWhile">]
val skipWhile : predicate : ('T -> bool) -> list : IIndexedList<'T> -> IIndexedList<'T>

/// <summary>Splits the list at an index.</summary>
/// <param name="index">The index of the first element of the second part.</param>
/// <param name="list">The input list.</param>
/// <returns>The elements before the index and the elements from the index, in lists of the implementation of the input.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the index is negative.</exception>
/// <exception cref="T:System.InvalidOperationException">Thrown when the index exceeds the length.</exception>
/// <remarks>O(n) for an indexed list.</remarks>
[<CompiledName "SplitAt">]
val splitAt : index : int -> list : IIndexedList<'T> -> struct (IIndexedList<'T> * IIndexedList<'T>)

/// <summary>
/// Splits the list into at most a given number of parts of nearly equal length.
/// </summary>
/// <param name="count">The maximum number of parts.</param>
/// <param name="list">The input list.</param>
/// <returns>The parts; the first ones are one element longer when the length does not divide evenly.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when <paramref name="count"/> is not positive.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "SplitInto">]
val splitInto : count : int -> list : IIndexedList<'T> -> IndexedList<IndexedList<'T>>

/// <summary>
/// Returns the first elements of the list.
/// </summary>
/// <param name="count">The number of elements to take.</param>
/// <param name="list">The input list.</param>
/// <returns>The first elements, in a list of the implementation of the input.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the count is negative.</exception>
/// <exception cref="T:System.InvalidOperationException">Thrown when the count exceeds the length.</exception>
/// <remarks>
/// O(log n + count) for an indexed list; the other implementations are read from their start. The list itself is returned
/// when the count is the length.
/// </remarks>
[<CompiledName "Take">]
val take : count : int -> list : IIndexedList<'T> -> IIndexedList<'T>

/// <summary>
/// Returns the last elements of the list.
/// </summary>
/// <param name="count">The number of elements to take.</param>
/// <param name="list">The input list.</param>
/// <returns>The last elements, in a list of the implementation of the input.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the count is negative.</exception>
/// <exception cref="T:System.InvalidOperationException">Thrown when the count exceeds the length.</exception>
/// <remarks>
/// O(log n + count) for an indexed list; the other implementations are read from their start. The list itself is returned
/// when the count is the length. The checks follow
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.Take``1(System.Int32,Microsoft.FSharp.Collections.FSharpList{``0})"/>.
/// </remarks>
[<CompiledName "TakeLast">]
val takeLast : count : int -> list : IIndexedList<'T> -> IIndexedList<'T>

/// <summary>
/// Returns the first elements of the list that satisfy the predicate.
/// </summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="list">The input list.</param>
/// <returns>The elements before the first one that does not satisfy the predicate, in a list of the implementation of the input.</returns>
/// <remarks>
/// O(k) for an indexed list, where k is the length of the result.
/// </remarks>
[<CompiledName "TakeWhile">]
val takeWhile : predicate : ('T -> bool) -> list : IIndexedList<'T> -> IIndexedList<'T>

/// <summary>
/// Returns at most a given number of first elements.
/// </summary>
/// <param name="count">The maximum number of elements; a count that is not positive returns an empty list.</param>
/// <param name="list">The input list.</param>
/// <returns>The first elements, in a list of the implementation of the input.</returns>
/// <remarks>
/// O(log n + k) for an indexed list, where k is the length of the result; the other implementations are read from their
/// start. The list itself is returned when it is not longer than the count.
/// </remarks>
[<CompiledName "Truncate">]
val truncate : count : int -> list : IIndexedList<'T> -> IIndexedList<'T>

// ---- Sorting ----

/// <summary>
/// Sorts the elements in the order of F# generic comparison.
/// </summary>
/// <param name="list">The input list.</param>
/// <returns>The sorted elements, in a list of the implementation of the input.</returns>
/// <exception cref="T:System.InvalidOperationException">
/// Thrown when the comparison of two elements throws, as in <see cref="M:Microsoft.FSharp.Collections.ListModule.Sort``1(Microsoft.FSharp.Collections.FSharpList{``0})"/>.
/// </exception>
/// <remarks>
/// O(n log n). This is a stable sort, as <see cref="M:Microsoft.FSharp.Collections.ListModule.Sort``1(Microsoft.FSharp.Collections.FSharpList{``0})"/>:
/// equal elements keep their order.
/// </remarks>
[<CompiledName "Sort">]
val sort : list : IIndexedList<'T> -> IIndexedList<'T> when 'T : comparison

/// <summary>
/// Sorts the elements by the result of a projection, in the order of F# generic comparison.
/// </summary>
/// <param name="projection">The function that returns the key of an element.</param>
/// <param name="list">The input list.</param>
/// <returns>The sorted elements, in a list of the implementation of the input.</returns>
/// <remarks>
/// O(n log n). This is a stable sort: elements with equal keys keep their order.
/// </remarks>
[<CompiledName "SortBy">]
val sortBy : projection : ('T -> 'Key) -> list : IIndexedList<'T> -> IIndexedList<'T> when 'Key : comparison

/// <summary>
/// Sorts the elements by the result of a projection, in descending order.
/// </summary>
/// <param name="projection">The function that returns the key of an element.</param>
/// <param name="list">The input list.</param>
/// <returns>The sorted elements, in a list of the implementation of the input.</returns>
/// <exception cref="T:System.InvalidOperationException">
/// Thrown when the projection or the comparison of two keys throws: the projection runs in the comparison, as in
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.SortByDescending``2(Microsoft.FSharp.Core.FSharpFunc{``0,``1},Microsoft.FSharp.Collections.FSharpList{``0})"/>.
/// </exception>
/// <remarks>
/// O(n log n). This is a stable sort: elements with equal keys keep their order.
/// </remarks>
[<CompiledName "SortByDescending">]
val sortByDescending : projection : ('T -> 'Key) -> list : IIndexedList<'T> -> IIndexedList<'T> when 'Key : comparison

/// <summary>
/// Sorts the elements in descending order of F# generic comparison.
/// </summary>
/// <param name="list">The input list.</param>
/// <returns>The sorted elements, in a list of the implementation of the input.</returns>
/// <exception cref="T:System.InvalidOperationException">
/// Thrown when the comparison of two elements throws, as in <see cref="M:Microsoft.FSharp.Collections.ListModule.SortDescending``1(Microsoft.FSharp.Collections.FSharpList{``0})"/>.
/// </exception>
/// <remarks>
/// O(n log n). This is a stable sort: equal elements keep their order.
/// </remarks>
[<CompiledName "SortDescending">]
val sortDescending : list : IIndexedList<'T> -> IIndexedList<'T> when 'T : comparison

/// <summary>
/// Sorts the elements with a comparison function.
/// </summary>
/// <param name="comparer">The function that compares two elements.</param>
/// <param name="list">The input list.</param>
/// <returns>The sorted elements, in a list of the implementation of the input.</returns>
/// <exception cref="T:System.InvalidOperationException">
/// Thrown when the comparer throws, as in
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.SortWith``1(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpFunc{``0,System.Int32}},Microsoft.FSharp.Collections.FSharpList{``0})"/>.
/// </exception>
/// <remarks>
/// O(n log n). This is a stable sort: elements that compare equal keep their order.
/// </remarks>
[<CompiledName "SortWith">]
val sortWith : comparer : ('T -> 'T -> int) -> list : IIndexedList<'T> -> IIndexedList<'T>

/// <summary>
/// Sorts the elements with a comparer.
/// </summary>
/// <param name="comparer">The comparer.</param>
/// <param name="list">The input list.</param>
/// <returns>The sorted elements, in a list of the implementation of the input.</returns>
/// <remarks>
/// O(n log n). This is a stable sort: elements that compare equal keep their order. The function wraps
/// <see cref="M:System.Linq.Enumerable.Order``1(System.Collections.Generic.IEnumerable{``0},System.Collections.Generic.IComparer{``0})"/>.
/// </remarks>
[<CompiledName "SortWithComparer">]
val sortWithComparer : comparer : IComparer<'T> -> list : IIndexedList<'T> -> IIndexedList<'T>

// ---- Random ----

/// <summary>
/// Returns a random element, chosen with <see cref="P:System.Random.Shared"/>.
/// </summary>
/// <param name="source">The input list.</param>
/// <returns>A random element.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the list is empty.</exception>
/// <remarks>
/// O(log n).
/// </remarks>
[<CompiledName "RandomChoice">]
val randomChoice : source : IIndexedList<'T> -> 'T

/// <summary>
/// Returns a random element, chosen with a function that returns numbers in [0, 1).
/// </summary>
/// <param name="randomizer">The function that returns a random number in [0, 1).</param>
/// <param name="source">The input list.</param>
/// <returns>A random element.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the list is empty.</exception>
/// <exception cref="T:System.ArgumentOutOfRangeException">Thrown when the randomizer returns a number outside [0, 1).</exception>
/// <remarks>
/// O(log n).
/// </remarks>
[<CompiledName "RandomChoiceBy">]
val randomChoiceBy : randomizer : (unit -> float) -> source : IIndexedList<'T> -> 'T

/// <summary>
/// Returns a random element, chosen with a given random number generator.
/// </summary>
/// <param name="random">The random number generator.</param>
/// <param name="source">The input list.</param>
/// <returns>A random element.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="random"/> is <see langword="null"/>.</exception>
/// <exception cref="T:System.ArgumentException">Thrown when the list is empty.</exception>
/// <remarks>
/// O(log n).
/// </remarks>
[<CompiledName "RandomChoiceWith">]
val randomChoiceWith : random : Random -> source : IIndexedList<'T> -> 'T

/// <summary>
/// Returns random elements, chosen with <see cref="P:System.Random.Shared"/>; an element can repeat.
/// </summary>
/// <param name="count">The number of elements to choose.</param>
/// <param name="source">The input list.</param>
/// <returns>The chosen elements, in a list of the implementation of the input.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the count is negative, or positive for an empty list.</exception>
/// <remarks>
/// O(n + count).
/// </remarks>
[<CompiledName "RandomChoices">]
val randomChoices : count : int -> source : IIndexedList<'T> -> IIndexedList<'T>

/// <summary>
/// Returns random elements, chosen with a function that returns numbers in [0, 1); an element can repeat.
/// </summary>
/// <param name="randomizer">The function that returns a random number in [0, 1).</param>
/// <param name="count">The number of elements to choose.</param>
/// <param name="source">The input list.</param>
/// <returns>The chosen elements, in a list of the implementation of the input.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the count is negative, or positive for an empty list.</exception>
/// <exception cref="T:System.ArgumentOutOfRangeException">Thrown when the randomizer returns a number outside [0, 1).</exception>
/// <remarks>
/// O(n + count).
/// </remarks>
[<CompiledName "RandomChoicesBy">]
val randomChoicesBy : randomizer : (unit -> float) -> count : int -> source : IIndexedList<'T> -> IIndexedList<'T>

/// <summary>
/// Returns random elements, chosen with a given random number generator; an element can repeat.
/// </summary>
/// <param name="random">The random number generator.</param>
/// <param name="count">The number of elements to choose.</param>
/// <param name="source">The input list.</param>
/// <returns>The chosen elements, in a list of the implementation of the input.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="random"/> is <see langword="null"/>.</exception>
/// <exception cref="T:System.ArgumentException">Thrown when the count is negative, or positive for an empty list.</exception>
/// <remarks>
/// O(n + count).
/// </remarks>
[<CompiledName "RandomChoicesWith">]
val randomChoicesWith : random : Random -> count : int -> source : IIndexedList<'T> -> IIndexedList<'T>

/// <summary>
/// Returns random elements at distinct indices, chosen with <see cref="P:System.Random.Shared"/>.
/// </summary>
/// <param name="count">The number of elements to choose.</param>
/// <param name="source">The input list.</param>
/// <returns>The chosen elements, in a list of the implementation of the input.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the count is negative or exceeds the length.</exception>
/// <remarks>
/// O(n + count).
/// </remarks>
[<CompiledName "RandomSample">]
val randomSample : count : int -> source : IIndexedList<'T> -> IIndexedList<'T>

/// <summary>
/// Returns random elements at distinct indices, chosen with a function that returns numbers in [0, 1).
/// </summary>
/// <param name="randomizer">The function that returns a random number in [0, 1).</param>
/// <param name="count">The number of elements to choose.</param>
/// <param name="source">The input list.</param>
/// <returns>The chosen elements, in a list of the implementation of the input.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the count is negative or exceeds the length.</exception>
/// <exception cref="T:System.ArgumentOutOfRangeException">Thrown when the randomizer returns a number outside [0, 1).</exception>
/// <remarks>
/// O(n + count).
/// </remarks>
[<CompiledName "RandomSampleBy">]
val randomSampleBy : randomizer : (unit -> float) -> count : int -> source : IIndexedList<'T> -> IIndexedList<'T>

/// <summary>
/// Returns random elements at distinct indices, chosen with a given random number generator.
/// </summary>
/// <param name="random">The random number generator.</param>
/// <param name="count">The number of elements to choose.</param>
/// <param name="source">The input list.</param>
/// <returns>The chosen elements, in a list of the implementation of the input.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="random"/> is <see langword="null"/>.</exception>
/// <exception cref="T:System.ArgumentException">Thrown when the count is negative or exceeds the length.</exception>
/// <remarks>
/// O(n + count).
/// </remarks>
[<CompiledName "RandomSampleWith">]
val randomSampleWith : random : Random -> count : int -> source : IIndexedList<'T> -> IIndexedList<'T>

/// <summary>
/// Returns the elements in a random order, chosen with <see cref="P:System.Random.Shared"/>.
/// </summary>
/// <param name="source">The input list.</param>
/// <returns>The shuffled elements, in a list of the implementation of the input.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "RandomShuffle">]
val randomShuffle : source : IIndexedList<'T> -> IIndexedList<'T>

/// <summary>
/// Returns the elements in a random order, chosen with a function that returns numbers in [0, 1).
/// </summary>
/// <param name="randomizer">The function that returns a random number in [0, 1).</param>
/// <param name="source">The input list.</param>
/// <returns>The shuffled elements, in a list of the implementation of the input.</returns>
/// <exception cref="T:System.ArgumentOutOfRangeException">Thrown when the randomizer returns a number outside [0, 1).</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "RandomShuffleBy">]
val randomShuffleBy : randomizer : (unit -> float) -> source : IIndexedList<'T> -> IIndexedList<'T>

/// <summary>
/// Returns the elements in a random order, chosen with a given random number generator.
/// </summary>
/// <param name="random">The random number generator.</param>
/// <param name="source">The input list.</param>
/// <returns>The shuffled elements, in a list of the implementation of the input.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="random"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "RandomShuffleWith">]
val randomShuffleWith : random : Random -> source : IIndexedList<'T> -> IIndexedList<'T>

// ---- Conversion ----

/// <summary>Returns a new array with the elements.</summary>
/// <param name="list">The input list.</param>
/// <returns>A copy of the elements.</returns>
/// <remarks>O(n).</remarks>
[<CompiledName "ToArray">]
val toArray : list : IIndexedList<'T> -> 'T array

/// <summary>Returns a list of the elements.</summary>
/// <param name="list">The input list.</param>
/// <returns>The list of the elements.</returns>
/// <remarks>O(n).</remarks>
[<CompiledName "ToList">]
val toList : list : IIndexedList<'T> -> 'T list

/// <summary>Returns the list as a sequence.</summary>
/// <param name="list">The input list.</param>
/// <returns>The sequence of the elements.</returns>
/// <remarks>O(1).</remarks>
[<CompiledName "ToSeq">]
val toSeq : list : IIndexedList<'T> -> seq<'T>

// ---- Type-specific ----

/// <summary>
/// Returns an empty list of the implementation of the input.
/// </summary>
/// <param name="list">The input list.</param>
/// <returns>The empty list.</returns>
/// <remarks>
/// O(1). The function wraps <see cref="M:System.Collections.Immutable.IImmutableList`1.Clear"/>, which keeps the
/// implementation of the list and its settings.
/// </remarks>
[<CompiledName "Clear">]
val clear : list : IIndexedList<'T> -> IIndexedList<'T>
