/// <summary>
/// Functions for the builder of an indexed list, <see cref="T:System.Collections.Immutable.ImmutableList`1.Builder"/>.
/// </summary>
/// <remarks>
/// The builder is mutable. The functions that change it return the same builder, so that the calls chain with the pipe
/// operator. <see cref="M:FSharp.Collections.Immutable.IndexedList.CreateBuilder``1"/> creates a builder, and
/// <see cref="M:FSharp.Collections.Immutable.IndexedList.OfBuilder``1(System.Collections.Immutable.ImmutableList.Builder{``0})"/>
/// returns its contents as an indexed list. A function that takes a sequence throws
/// <see cref="T:System.ArgumentNullException"/> for a <see langword="null"/> sequence and for a default
/// <see cref="T:System.Collections.Immutable.ImmutableArray`1"/> that is given as a sequence, which would fail with
/// <see cref="T:System.InvalidOperationException"/> when the function enumerates it.
/// </remarks>
[<RequireQualifiedAccess>]
module FSharp.Collections.Immutable.IndexedListBuilder

open System.Collections.Generic

// ---- Properties ----

/// <summary>Returns the number of elements in the builder.</summary>
/// <param name="builder">The builder.</param>
/// <returns>The number of elements.</returns>
[<CompiledName "Length">]
val length : builder : IndexedList<'T>.Builder -> int

// ---- Access ----

/// <summary>Returns the element at an index.</summary>
/// <param name="index">The index of the element.</param>
/// <param name="builder">The builder.</param>
/// <returns>The element.</returns>
/// <exception cref="T:System.ArgumentOutOfRangeException">Thrown when the index is out of range.</exception>
[<CompiledName "Item">]
val item : index : int -> builder : IndexedList<'T>.Builder -> 'T

// ---- Search ----

/// <summary>
/// Searches the sorted builder for a value with the binary search algorithm.
/// </summary>
/// <param name="value">The value to search for.</param>
/// <param name="builder">The builder, sorted in the order of F# generic comparison.</param>
/// <returns>
/// The index of the value, or the bitwise complement of the index of the first larger element, or the bitwise
/// complement of the length when no element is larger.
/// </returns>
/// <remarks>
/// O(log n). The function compares as
/// <see cref="M:FSharp.Collections.Immutable.IndexedList.BinarySearch``1(``0,System.Collections.Immutable.ImmutableList{``0})"/>
/// does, and wraps
/// <see cref="M:System.Collections.Immutable.ImmutableList`1.Builder.BinarySearch(`0,System.Collections.Generic.IComparer{`0})"/>.
/// </remarks>
[<CompiledName "BinarySearch">]
val binarySearch : value : 'T -> builder : IndexedList<'T>.Builder -> int when 'T : comparison

/// <summary>
/// Searches a range of a builder for a value with the binary search algorithm and a given comparer.
/// </summary>
/// <param name="comparer">The comparer, in whose order the builder is sorted.</param>
/// <param name="index">The index of the first element of the range.</param>
/// <param name="count">The number of elements of the range.</param>
/// <param name="value">The value to search for.</param>
/// <param name="builder">The builder.</param>
/// <returns>
/// The index of the value, or the bitwise complement of the index of the first larger element of the range, or the
/// bitwise complement of the index after the range when no element of the range is larger.
/// </returns>
/// <exception cref="T:System.ArgumentOutOfRangeException">
/// Thrown when <paramref name="index"/> or <paramref name="count"/> is negative.
/// </exception>
/// <exception cref="T:System.ArgumentException">Thrown when the range extends beyond the end of the builder.</exception>
/// <remarks>
/// <para>
/// O(log n) comparisons. An exception that the comparer throws is not wrapped. The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableList`1.Builder.BinarySearch(System.Int32,System.Int32,`0,System.Collections.Generic.IComparer{`0})"/>.
/// </para>
/// <para>
/// That member does not check that the range lies in the builder, although its documentation says that it throws
/// <see cref="T:System.ArgumentException"/>. The function throws, as the documentation says (ADR 0001, D14).
/// </para>
/// </remarks>
[<CompiledName "BinarySearchRangeWithComparer">]
val binarySearchRangeWithComparer :
    comparer : IComparer<'T> -> index : int -> count : int -> value : 'T -> builder : IndexedList<'T>.Builder -> int

/// <summary>
/// Searches a sorted builder for a value with the binary search algorithm and a given comparer.
/// </summary>
/// <param name="comparer">The comparer, in whose order the builder is sorted.</param>
/// <param name="value">The value to search for.</param>
/// <param name="builder">The builder.</param>
/// <returns>
/// The index of the value, or the bitwise complement of the index of the first larger element, or the bitwise
/// complement of the length when no element is larger.
/// </returns>
/// <remarks>
/// O(log n) comparisons. An exception that the comparer throws is not wrapped. The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableList`1.Builder.BinarySearch(`0,System.Collections.Generic.IComparer{`0})"/>.
/// </remarks>
[<CompiledName "BinarySearchWithComparer">]
val binarySearchWithComparer : comparer : IComparer<'T> -> value : 'T -> builder : IndexedList<'T>.Builder -> int

/// <summary>
/// Returns whether the builder contains a value, in <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>.
/// </summary>
/// <param name="value">The value to find.</param>
/// <param name="builder">The builder.</param>
/// <returns><see langword="true"/> when an element equals the value.</returns>
[<CompiledName "Contains">]
val contains : value : 'T -> builder : IndexedList<'T>.Builder -> bool

/// <summary>
/// Returns the index of the first element equal to a value, or -1.
/// </summary>
/// <param name="value">The value to find.</param>
/// <param name="builder">The builder.</param>
/// <returns>The index, or -1.</returns>
[<CompiledName "IndexOf">]
val indexOf : value : 'T -> builder : IndexedList<'T>.Builder -> int

/// <summary>
/// Returns the index of the first element of a range that equals a value, in
/// <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>.
/// </summary>
/// <param name="startIndex">The index of the first element of the range.</param>
/// <param name="count">The number of elements of the range.</param>
/// <param name="value">The value to find.</param>
/// <param name="builder">The builder.</param>
/// <returns>The index in the builder, not in the range, of the first equal element of the range, or -1.</returns>
/// <exception cref="T:System.ArgumentOutOfRangeException">
/// Thrown when <paramref name="startIndex"/> or <paramref name="count"/> is negative, or the range extends beyond the end
/// of the builder.
/// </exception>
/// <remarks>
/// O(log n + count). The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableList`1.Builder.IndexOf(`0,System.Int32,System.Int32,System.Collections.Generic.IEqualityComparer{`0})"/>.
/// </remarks>
[<CompiledName "IndexOfRange">]
val indexOfRange : startIndex : int -> count : int -> value : 'T -> builder : IndexedList<'T>.Builder -> int

/// <summary>
/// Returns the index of the first element of a range that a given equality comparer finds equal to a value.
/// </summary>
/// <param name="comparer">The equality comparer.</param>
/// <param name="startIndex">The index of the first element of the range.</param>
/// <param name="count">The number of elements of the range.</param>
/// <param name="value">The value to find.</param>
/// <param name="builder">The builder.</param>
/// <returns>The index in the builder, not in the range, of the first equal element of the range, or -1.</returns>
/// <exception cref="T:System.ArgumentOutOfRangeException">
/// Thrown when <paramref name="startIndex"/> or <paramref name="count"/> is negative, or the range extends beyond the end
/// of the builder.
/// </exception>
/// <remarks>
/// O(log n + count). The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableList`1.Builder.IndexOf(`0,System.Int32,System.Int32,System.Collections.Generic.IEqualityComparer{`0})"/>.
/// </remarks>
[<CompiledName "IndexOfRangeWith">]
val indexOfRangeWith :
    comparer : IEqualityComparer<'T> -> startIndex : int -> count : int -> value : 'T -> builder : IndexedList<'T>.Builder -> int

/// <summary>
/// Returns the index of the last element equal to a value, or -1.
/// </summary>
/// <param name="value">The value to find.</param>
/// <param name="builder">The builder.</param>
/// <returns>The index, or -1.</returns>
[<CompiledName "LastIndexOf">]
val lastIndexOf : value : 'T -> builder : IndexedList<'T>.Builder -> int

/// <summary>
/// Returns the index of the last element of a range that equals a value, in
/// <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>. The search walks backward from the index of the
/// last element of the range.
/// </summary>
/// <param name="startIndex">The index of the last element of the range, where the search starts.</param>
/// <param name="count">The number of elements of the range, which ends at <paramref name="startIndex"/>.</param>
/// <param name="value">The value to find.</param>
/// <param name="builder">The builder.</param>
/// <returns>The index in the builder, not in the range, of the last equal element of the range, or -1.</returns>
/// <exception cref="T:System.ArgumentOutOfRangeException">
/// Thrown when <paramref name="startIndex"/> or <paramref name="count"/> is negative, <paramref name="count"/> exceeds the
/// length, or a range of elements does not lie in the builder: its first element, at <c>startIndex - count + 1</c>, is
/// before the start, or its last element, at <paramref name="startIndex"/>, is at or after the length.
/// </exception>
/// <remarks>
/// <para>
/// O(log n + count). A count of 0 is an empty range, which has no equal element, for any start index. The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableList`1.Builder.LastIndexOf(`0,System.Int32,System.Int32,System.Collections.Generic.IEqualityComparer{`0})"/>.
/// </para>
/// <para>
/// That member accepts a last element at or after the length and answers with an index that does not belong to the equal
/// element, and it throws a plain <see cref="T:System.ArgumentException"/> for a range that starts before the first
/// element. The function throws <see cref="T:System.ArgumentOutOfRangeException"/> for both (ADR 0001, D14).
/// </para>
/// </remarks>
[<CompiledName "LastIndexOfRange">]
val lastIndexOfRange : startIndex : int -> count : int -> value : 'T -> builder : IndexedList<'T>.Builder -> int

/// <summary>
/// Returns the index of the last element of a range that a given equality comparer finds equal to a value. The search
/// walks backward from the index of the last element of the range.
/// </summary>
/// <param name="comparer">The equality comparer.</param>
/// <param name="startIndex">The index of the last element of the range, where the search starts.</param>
/// <param name="count">The number of elements of the range, which ends at <paramref name="startIndex"/>.</param>
/// <param name="value">The value to find.</param>
/// <param name="builder">The builder.</param>
/// <returns>The index in the builder, not in the range, of the last equal element of the range, or -1.</returns>
/// <exception cref="T:System.ArgumentOutOfRangeException">
/// Thrown when <paramref name="startIndex"/> or <paramref name="count"/> is negative, <paramref name="count"/> exceeds the
/// length, or a range of elements does not lie in the builder: its first element, at <c>startIndex - count + 1</c>, is
/// before the start, or its last element, at <paramref name="startIndex"/>, is at or after the length.
/// </exception>
/// <remarks>
/// <para>
/// O(log n + count). A count of 0 is an empty range, which has no equal element, for any start index. The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableList`1.Builder.LastIndexOf(`0,System.Int32,System.Int32,System.Collections.Generic.IEqualityComparer{`0})"/>.
/// </para>
/// <para>
/// That member accepts a last element at or after the length and answers with an index that does not belong to the equal
/// element, and it throws a plain <see cref="T:System.ArgumentException"/> for a range that starts before the first
/// element. The function throws <see cref="T:System.ArgumentOutOfRangeException"/> for both (ADR 0001, D14).
/// </para>
/// </remarks>
[<CompiledName "LastIndexOfRangeWith">]
val lastIndexOfRangeWith :
    comparer : IEqualityComparer<'T> -> startIndex : int -> count : int -> value : 'T -> builder : IndexedList<'T>.Builder -> int

// ---- Transformation ----

/// <summary>Adds a value after the last element.</summary>
/// <param name="value">The value to add.</param>
/// <param name="builder">The builder.</param>
/// <returns>The same builder.</returns>
[<CompiledName "Add">]
val add : value : 'T -> builder : IndexedList<'T>.Builder -> IndexedList<'T>.Builder

/// <summary>Adds values after the last element.</summary>
/// <param name="values">The values to add.</param>
/// <param name="builder">The builder.</param>
/// <returns>The same builder.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="values"/> is <see langword="null"/> or a default flat list.</exception>
[<CompiledName "AddRange">]
val addRange : values : seq<'T> -> builder : IndexedList<'T>.Builder -> IndexedList<'T>.Builder

/// <summary>Removes every element.</summary>
/// <param name="builder">The builder.</param>
/// <returns>The same builder.</returns>
[<CompiledName "Clear">]
val clear : builder : IndexedList<'T>.Builder -> IndexedList<'T>.Builder

/// <summary>Inserts a value at an index.</summary>
/// <param name="index">The index of the inserted value, from 0 to the count.</param>
/// <param name="value">The value to insert.</param>
/// <param name="builder">The builder.</param>
/// <returns>The same builder.</returns>
/// <exception cref="T:System.ArgumentOutOfRangeException">Thrown when the index is out of range.</exception>
[<CompiledName "InsertAt">]
val insertAt : index : int -> value : 'T -> builder : IndexedList<'T>.Builder -> IndexedList<'T>.Builder

/// <summary>
/// Inserts values at an index.
/// </summary>
/// <param name="index">The index of the first inserted value, from 0 to the count.</param>
/// <param name="values">The values to insert.</param>
/// <param name="builder">The builder.</param>
/// <returns>The same builder.</returns>
/// <exception cref="T:System.ArgumentOutOfRangeException">Thrown when the index is out of range.</exception>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="values"/> is <see langword="null"/> or a default flat list.</exception>
/// <remarks>
/// The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableList`1.Builder.InsertRange(System.Int32,System.Collections.Generic.IEnumerable{`0})"/>,
/// which checks the index before the values: an index out of range wins over a missing sequence.
/// </remarks>
[<CompiledName "InsertManyAt">]
val insertManyAt : index : int -> values : seq<'T> -> builder : IndexedList<'T>.Builder -> IndexedList<'T>.Builder

/// <summary>
/// Removes the first element equal to a value, in <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>.
/// </summary>
/// <param name="value">The value to remove.</param>
/// <param name="builder">The builder.</param>
/// <returns><see langword="true"/> when an element was removed.</returns>
[<CompiledName "Remove">]
val remove : value : 'T -> builder : IndexedList<'T>.Builder -> bool

/// <summary>
/// Removes the elements that satisfy a predicate.
/// </summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="builder">The builder.</param>
/// <returns>The same builder.</returns>
/// <remarks>
/// O(n + k × log n) for k removed elements. The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableList`1.Builder.RemoveAll(System.Predicate{`0})"/>, which returns the
/// number of removed elements; compare the lengths to find it.
/// </remarks>
[<CompiledName "RemoveAll">]
val removeAll : predicate : ('T -> bool) -> builder : IndexedList<'T>.Builder -> IndexedList<'T>.Builder

/// <summary>Removes the element at an index.</summary>
/// <param name="index">The index of the removed element.</param>
/// <param name="builder">The builder.</param>
/// <returns>The same builder.</returns>
/// <exception cref="T:System.ArgumentOutOfRangeException">Thrown when the index is out of range.</exception>
[<CompiledName "RemoveAt">]
val removeAt : index : int -> builder : IndexedList<'T>.Builder -> IndexedList<'T>.Builder

/// <summary>
/// Removes one element for each of the given values, in
/// <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>.
/// </summary>
/// <param name="items">The values to remove.</param>
/// <param name="builder">The builder.</param>
/// <returns>The same builder.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="items"/> is <see langword="null"/> or a default flat list.</exception>
/// <remarks>
/// O(n × m) for m values. Each value removes the first element that equals it and that no earlier value has removed; a
/// value that equals no remaining element removes nothing. The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableList`1.Builder.RemoveRange(System.Collections.Generic.IEnumerable{`0})"/>.
/// </remarks>
[<CompiledName "RemoveMany">]
val removeMany : items : seq<'T> -> builder : IndexedList<'T>.Builder -> IndexedList<'T>.Builder

/// <summary>
/// Removes a range of elements.
/// </summary>
/// <param name="index">The index of the first removed element.</param>
/// <param name="count">The number of removed elements.</param>
/// <param name="builder">The builder.</param>
/// <returns>The same builder.</returns>
/// <exception cref="T:System.ArgumentOutOfRangeException">
/// Thrown when <paramref name="index"/> or <paramref name="count"/> is negative, or the range extends beyond the end of
/// the builder.
/// </exception>
/// <remarks>
/// O(count × log n). The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableList`1.Builder.RemoveRange(System.Int32,System.Int32)"/>, which
/// throws for a negative count. The function of the indexed list removes nothing for a count that is not positive, as
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.RemoveManyAt``1(System.Int32,System.Int32,Microsoft.FSharp.Collections.FSharpList{``0})"/>
/// does.
/// </remarks>
[<CompiledName "RemoveManyAt">]
val removeManyAt : index : int -> count : int -> builder : IndexedList<'T>.Builder -> IndexedList<'T>.Builder

/// <summary>
/// Removes one element for each of the given values, in a given equality comparer.
/// </summary>
/// <param name="comparer">The equality comparer.</param>
/// <param name="items">The values to remove.</param>
/// <param name="builder">The builder.</param>
/// <returns>The same builder.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="items"/> is <see langword="null"/> or a default flat list.</exception>
/// <remarks>
/// O(n × m) for m values. Each value removes the first element that the comparer finds equal to it and that no earlier
/// value has removed; a value that equals no remaining element removes nothing. The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableList`1.Builder.RemoveRange(System.Collections.Generic.IEnumerable{`0},System.Collections.Generic.IEqualityComparer{`0})"/>.
/// </remarks>
[<CompiledName "RemoveManyWith">]
val removeManyWith :
    comparer : IEqualityComparer<'T> -> items : seq<'T> -> builder : IndexedList<'T>.Builder -> IndexedList<'T>.Builder

/// <summary>
/// Removes the first element that equals a value in a given equality comparer.
/// </summary>
/// <param name="comparer">The equality comparer.</param>
/// <param name="value">The value to remove.</param>
/// <param name="builder">The builder.</param>
/// <returns><see langword="true"/> when an element was removed.</returns>
/// <remarks>
/// O(n). The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableList`1.Builder.Remove(`0,System.Collections.Generic.IEqualityComparer{`0})"/>.
/// </remarks>
[<CompiledName "RemoveWith">]
val removeWith : comparer : IEqualityComparer<'T> -> value : 'T -> builder : IndexedList<'T>.Builder -> bool

/// <summary>
/// Replaces the first element that equals a value by another value, in
/// <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>.
/// </summary>
/// <param name="oldValue">The value to replace.</param>
/// <param name="newValue">The new value.</param>
/// <param name="builder">The builder.</param>
/// <returns>The same builder.</returns>
/// <exception cref="T:System.ArgumentException">
/// Thrown when no element equals <paramref name="oldValue"/>, as the member does.
/// </exception>
/// <remarks>
/// O(n). The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableList`1.Builder.Replace(`0,`0)"/>.
/// </remarks>
[<CompiledName "Replace">]
val replace : oldValue : 'T -> newValue : 'T -> builder : IndexedList<'T>.Builder -> IndexedList<'T>.Builder

/// <summary>
/// Replaces the first element that equals a value in a given equality comparer by another value.
/// </summary>
/// <param name="comparer">The equality comparer.</param>
/// <param name="oldValue">The value to replace.</param>
/// <param name="newValue">The new value.</param>
/// <param name="builder">The builder.</param>
/// <returns>The same builder.</returns>
/// <exception cref="T:System.ArgumentException">
/// Thrown when the comparer finds no element equal to <paramref name="oldValue"/>, as the member does.
/// </exception>
/// <remarks>
/// O(n). The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableList`1.Builder.Replace(`0,`0,System.Collections.Generic.IEqualityComparer{`0})"/>.
/// </remarks>
[<CompiledName "ReplaceWith">]
val replaceWith :
    comparer : IEqualityComparer<'T> ->
    oldValue : 'T ->
    newValue : 'T ->
    builder : IndexedList<'T>.Builder ->
        IndexedList<'T>.Builder

/// <summary>Reverses the order of the elements.</summary>
/// <param name="builder">The builder.</param>
/// <returns>The same builder.</returns>
[<CompiledName "Reverse">]
val rev : builder : IndexedList<'T>.Builder -> IndexedList<'T>.Builder

/// <summary>
/// Reverses the order of the elements of a range.
/// </summary>
/// <param name="index">The index of the first element of the range.</param>
/// <param name="count">The number of elements of the range.</param>
/// <param name="builder">The builder.</param>
/// <returns>The same builder.</returns>
/// <exception cref="T:System.ArgumentOutOfRangeException">
/// Thrown when <paramref name="index"/> or <paramref name="count"/> is negative, or the range extends beyond the end of
/// the builder.
/// </exception>
/// <remarks>
/// O(count × log n). The elements outside the range keep their places. The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableList`1.Builder.Reverse(System.Int32,System.Int32)"/>.
/// </remarks>
[<CompiledName "ReverseRange">]
val revRange : index : int -> count : int -> builder : IndexedList<'T>.Builder -> IndexedList<'T>.Builder

/// <summary>Replaces the element at an index.</summary>
/// <param name="index">The index of the replaced element.</param>
/// <param name="value">The new value.</param>
/// <param name="builder">The builder.</param>
/// <returns>The same builder.</returns>
/// <exception cref="T:System.ArgumentOutOfRangeException">Thrown when the index is out of range.</exception>
[<CompiledName "UpdateAt">]
val updateAt : index : int -> value : 'T -> builder : IndexedList<'T>.Builder -> IndexedList<'T>.Builder

// ---- Sorting ----

/// <summary>
/// Sorts the elements in the order of F# generic comparison.
/// </summary>
/// <param name="builder">The builder.</param>
/// <returns>The same builder.</returns>
/// <remarks>
/// This is a stable sort, as <see cref="M:FSharp.Collections.Immutable.IndexedList.Sort``1(System.Collections.Immutable.ImmutableList{``0})"/>.
/// </remarks>
[<CompiledName "Sort">]
val sort : builder : IndexedList<'T>.Builder -> IndexedList<'T>.Builder when 'T : comparison

/// <summary>
/// Sorts a range of the elements in the order of F# generic comparison.
/// </summary>
/// <param name="index">The index of the first element of the range.</param>
/// <param name="count">The number of elements of the range.</param>
/// <param name="builder">The builder.</param>
/// <returns>The same builder.</returns>
/// <exception cref="T:System.ArgumentOutOfRangeException">
/// Thrown when <paramref name="index"/> or <paramref name="count"/> is negative, or the range extends beyond the end of
/// the builder.
/// </exception>
/// <exception cref="T:System.InvalidOperationException">Thrown when the comparison of two elements throws.</exception>
/// <remarks>
/// O(count × log n). This is a stable sort, as
/// <see cref="M:FSharp.Collections.Immutable.IndexedList.SortRange``1(System.Int32,System.Int32,System.Collections.Immutable.ImmutableList{``0})"/>:
/// elements that compare equal keep their order, and the elements outside the range keep their places. The function
/// does not wrap the unstable
/// <see cref="M:System.Collections.Immutable.ImmutableList`1.Builder.Sort(System.Int32,System.Int32,System.Collections.Generic.IComparer{`0})"/>.
/// </remarks>
[<CompiledName "SortRange">]
val sortRange : index : int -> count : int -> builder : IndexedList<'T>.Builder -> IndexedList<'T>.Builder when 'T : comparison

/// <summary>
/// Sorts a range of the elements with a comparison function.
/// </summary>
/// <param name="comparison">The function that compares two elements.</param>
/// <param name="index">The index of the first element of the range.</param>
/// <param name="count">The number of elements of the range.</param>
/// <param name="builder">The builder.</param>
/// <returns>The same builder.</returns>
/// <exception cref="T:System.ArgumentOutOfRangeException">
/// Thrown when <paramref name="index"/> or <paramref name="count"/> is negative, or the range extends beyond the end of
/// the builder.
/// </exception>
/// <exception cref="T:System.InvalidOperationException">Thrown when the comparison function throws.</exception>
/// <remarks>
/// O(count × log n). This is a stable sort: elements that compare equal keep their order, and the elements outside the
/// range keep their places.
/// </remarks>
[<CompiledName "SortRangeWith">]
val sortRangeWith :
    comparison : ('T -> 'T -> int) -> index : int -> count : int -> builder : IndexedList<'T>.Builder -> IndexedList<'T>.Builder

/// <summary>
/// Sorts a range of the elements with a comparer.
/// </summary>
/// <param name="comparer">The comparer.</param>
/// <param name="index">The index of the first element of the range.</param>
/// <param name="count">The number of elements of the range.</param>
/// <param name="builder">The builder.</param>
/// <returns>The same builder.</returns>
/// <exception cref="T:System.ArgumentOutOfRangeException">
/// Thrown when <paramref name="index"/> or <paramref name="count"/> is negative, or the range extends beyond the end of
/// the builder.
/// </exception>
/// <exception cref="T:System.InvalidOperationException">Thrown when the comparer throws.</exception>
/// <remarks>
/// O(count × log n). This is a stable sort: elements that compare equal keep their order, and the elements outside the
/// range keep their places.
/// </remarks>
[<CompiledName "SortRangeWithComparer">]
val sortRangeWithComparer :
    comparer : IComparer<'T> -> index : int -> count : int -> builder : IndexedList<'T>.Builder -> IndexedList<'T>.Builder

/// <summary>
/// Sorts the elements with a comparison function.
/// </summary>
/// <param name="comparer">The function that compares two elements.</param>
/// <param name="builder">The builder.</param>
/// <returns>The same builder.</returns>
/// <remarks>
/// This is a stable sort: elements that compare equal keep their order.
/// </remarks>
[<CompiledName "SortWith">]
val sortWith : comparer : ('T -> 'T -> int) -> builder : IndexedList<'T>.Builder -> IndexedList<'T>.Builder

/// <summary>
/// Sorts the elements with a comparer.
/// </summary>
/// <param name="comparer">The comparer.</param>
/// <param name="builder">The builder.</param>
/// <returns>The same builder.</returns>
/// <remarks>
/// This is a stable sort: elements that compare equal keep their order.
/// </remarks>
[<CompiledName "SortWithComparer">]
val sortWithComparer : comparer : IComparer<'T> -> builder : IndexedList<'T>.Builder -> IndexedList<'T>.Builder

// ---- Conversion ----

/// <summary>
/// Copies a range of the elements of the builder into an array.
/// </summary>
/// <param name="source">The builder.</param>
/// <param name="sourceIndex">The index of the first copied element in the builder.</param>
/// <param name="target">The array to copy into.</param>
/// <param name="targetIndex">The index in the array of the first copied element.</param>
/// <param name="count">The number of elements to copy.</param>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="target"/> is <see langword="null"/>.</exception>
/// <exception cref="T:System.ArgumentOutOfRangeException">
/// Thrown when <paramref name="sourceIndex"/>, <paramref name="count"/> or <paramref name="targetIndex"/> is negative, or
/// a range extends beyond the end of the builder or of the array.
/// </exception>
/// <remarks>
/// O(log n + count). The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableList`1.Builder.CopyTo(System.Int32,`0[],System.Int32,System.Int32)"/>
/// and reports the names of its own parameters.
/// </remarks>
[<CompiledName "CopyTo">]
val blit : source : IndexedList<'T>.Builder -> sourceIndex : int -> target : 'T array -> targetIndex : int -> count : int -> unit

/// <summary>Returns a new array with the elements of the builder.</summary>
/// <param name="builder">The builder.</param>
/// <returns>A copy of the elements.</returns>
[<CompiledName "ToArray">]
val toArray : builder : IndexedList<'T>.Builder -> 'T array
