/// <summary>
/// Functions for the builder of a flat list, <see cref="T:System.Collections.Immutable.ImmutableArray`1.Builder"/>.
/// </summary>
/// <remarks>
/// The builder is mutable. The functions that change it return the same builder, so that the calls chain with the pipe
/// operator. <see cref="M:FSharp.Collections.Immutable.FlatList.CreateBuilder``1"/> creates a builder, and
/// <see cref="M:FSharp.Collections.Immutable.FlatList.OfBuilder``1(System.Collections.Immutable.ImmutableArray.Builder{``0})"/>
/// returns its contents as a flat list.
/// </remarks>
[<RequireQualifiedAccess>]
module FSharp.Collections.Immutable.FlatListBuilder

open System.Collections.Generic

// ---- Properties ----

/// <summary>
/// Returns the number of elements that the builder can hold without growing.
/// </summary>
/// <param name="builder">The builder.</param>
/// <returns>The capacity.</returns>
[<CompiledName "Capacity">]
val capacity : builder : FlatList<'T>.Builder -> int

/// <summary>Returns the number of elements in the builder.</summary>
/// <param name="builder">The builder.</param>
/// <returns>The number of elements.</returns>
[<CompiledName "Length">]
val length : builder : FlatList<'T>.Builder -> int

// ---- Access ----

/// <summary>Returns the element at an index.</summary>
/// <param name="index">The index of the element.</param>
/// <param name="builder">The builder.</param>
/// <returns>The element.</returns>
/// <exception cref="T:System.IndexOutOfRangeException">Thrown when the index is out of range.</exception>
[<CompiledName "Item">]
val item : index : int -> builder : FlatList<'T>.Builder -> 'T

// ---- Search ----

/// <summary>
/// Returns whether the builder contains a value, in <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>.
/// </summary>
/// <param name="value">The value to find.</param>
/// <param name="builder">The builder.</param>
/// <returns><see langword="true"/> when an element equals the value.</returns>
[<CompiledName "Contains">]
val contains : value : 'T -> builder : FlatList<'T>.Builder -> bool

/// <summary>
/// Returns the index of the first element equal to a value, or -1.
/// </summary>
/// <param name="value">The value to find.</param>
/// <param name="builder">The builder.</param>
/// <returns>The index, or -1.</returns>
[<CompiledName "IndexOf">]
val indexOf : value : 'T -> builder : FlatList<'T>.Builder -> int

/// <summary>
/// Returns the index of the first element equal to a value in a range, in
/// <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>.
/// </summary>
/// <param name="startIndex">The index of the first element of the range.</param>
/// <param name="count">The number of elements of the range.</param>
/// <param name="value">The value to find.</param>
/// <param name="builder">The builder.</param>
/// <returns>The index in the builder of the first equal element of the range, or -1.</returns>
/// <exception cref="T:System.ArgumentOutOfRangeException">Thrown when <paramref name="startIndex"/> or <paramref name="count"/> is outside the builder.</exception>
/// <remarks>
/// <para>
/// O(count). The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableArray`1.Builder.IndexOf(`0,System.Int32,System.Int32)"/>.
/// </para>
/// <para>
/// The start index must be less than the count of the builder, as in the BCL member, also for an empty range. An empty
/// builder accepts only the start index 0 with the count 0, and the result is -1.
/// </para>
/// </remarks>
[<CompiledName "IndexOfRange">]
val indexOfRange : startIndex : int -> count : int -> value : 'T -> builder : FlatList<'T>.Builder -> int

/// <summary>
/// Returns the index of the first element equal to a value in a range, in a given equality comparer.
/// </summary>
/// <param name="comparer">
/// The equality comparer. <see langword="null"/> stands for
/// <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>.
/// </param>
/// <param name="startIndex">The index of the first element of the range.</param>
/// <param name="count">The number of elements of the range.</param>
/// <param name="value">The value to find.</param>
/// <param name="builder">The builder.</param>
/// <returns>The index in the builder of the first equal element of the range, or -1.</returns>
/// <exception cref="T:System.ArgumentOutOfRangeException">Thrown when <paramref name="startIndex"/> or <paramref name="count"/> is outside the builder.</exception>
/// <remarks>
/// <para>
/// O(count). The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableArray`1.Builder.IndexOf(`0,System.Int32,System.Int32,System.Collections.Generic.IEqualityComparer{`0})"/>.
/// </para>
/// <para>
/// The start index must be less than the count of the builder, as in the BCL member, also for an empty range. An empty
/// builder accepts only the start index 0 with the count 0, and the result is -1.
/// </para>
/// </remarks>
[<CompiledName "IndexOfRangeWith">]
val indexOfRangeWith :
    comparer : IEqualityComparer<'T> -> startIndex : int -> count : int -> value : 'T -> builder : FlatList<'T>.Builder -> int

/// <summary>
/// Returns the index of the last element equal to a value, or -1.
/// </summary>
/// <param name="value">The value to find.</param>
/// <param name="builder">The builder.</param>
/// <returns>The index, or -1.</returns>
[<CompiledName "LastIndexOf">]
val lastIndexOf : value : 'T -> builder : FlatList<'T>.Builder -> int

/// <summary>
/// Returns the index of the last element equal to a value in a range that is searched backward, in
/// <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>.
/// </summary>
/// <param name="startIndex">The index of the last element of the range, where the search starts.</param>
/// <param name="count">The number of elements of the range, counted backward from the start index.</param>
/// <param name="value">The value to find.</param>
/// <param name="builder">The builder.</param>
/// <returns>The index in the builder of the last equal element of the range, or -1.</returns>
/// <exception cref="T:System.ArgumentOutOfRangeException">Thrown when <paramref name="startIndex"/> or <paramref name="count"/> is outside the builder.</exception>
/// <remarks>
/// <para>
/// O(count). The range holds the elements from <paramref name="startIndex"/> down to
/// <c>startIndex - count + 1</c>. The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableArray`1.Builder.LastIndexOf(`0,System.Int32,System.Int32)"/>.
/// </para>
/// <para>
/// The start index must be less than the count of the builder, as in the BCL member, also for an empty range. An empty
/// builder accepts only the start index 0 with the count 0, and the result is -1.
/// </para>
/// </remarks>
[<CompiledName "LastIndexOfRange">]
val lastIndexOfRange : startIndex : int -> count : int -> value : 'T -> builder : FlatList<'T>.Builder -> int

/// <summary>
/// Returns the index of the last element equal to a value in a range that is searched backward, in a given equality
/// comparer.
/// </summary>
/// <param name="comparer">
/// The equality comparer. <see langword="null"/> stands for
/// <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>.
/// </param>
/// <param name="startIndex">The index of the last element of the range, where the search starts.</param>
/// <param name="count">The number of elements of the range, counted backward from the start index.</param>
/// <param name="value">The value to find.</param>
/// <param name="builder">The builder.</param>
/// <returns>The index in the builder of the last equal element of the range, or -1.</returns>
/// <exception cref="T:System.ArgumentOutOfRangeException">Thrown when <paramref name="startIndex"/> or <paramref name="count"/> is outside the builder.</exception>
/// <remarks>
/// <para>
/// O(count). The range holds the elements from <paramref name="startIndex"/> down to
/// <c>startIndex - count + 1</c>. The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableArray`1.Builder.LastIndexOf(`0,System.Int32,System.Int32,System.Collections.Generic.IEqualityComparer{`0})"/>.
/// </para>
/// <para>
/// The start index must be less than the count of the builder, as in the BCL member, also for an empty range. An empty
/// builder accepts only the start index 0 with the count 0, and the result is -1.
/// </para>
/// </remarks>
[<CompiledName "LastIndexOfRangeWith">]
val lastIndexOfRangeWith :
    comparer : IEqualityComparer<'T> -> startIndex : int -> count : int -> value : 'T -> builder : FlatList<'T>.Builder -> int

// ---- Transformation ----

/// <summary>Adds a value after the last element.</summary>
/// <param name="value">The value to add.</param>
/// <param name="builder">The builder.</param>
/// <returns>The same builder.</returns>
[<CompiledName "Add">]
val add : value : 'T -> builder : FlatList<'T>.Builder -> FlatList<'T>.Builder

/// <summary>Adds values after the last element.</summary>
/// <param name="values">The values to add.</param>
/// <param name="builder">The builder.</param>
/// <returns>The same builder.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="values"/> is <see langword="null"/> or a default flat list.</exception>
[<CompiledName "AddRange">]
val addRange : values : seq<'T> -> builder : FlatList<'T>.Builder -> FlatList<'T>.Builder

/// <summary>Removes every element.</summary>
/// <param name="builder">The builder.</param>
/// <returns>The same builder.</returns>
[<CompiledName "Clear">]
val clear : builder : FlatList<'T>.Builder -> FlatList<'T>.Builder

/// <summary>Inserts a value at an index.</summary>
/// <param name="index">The index of the inserted value, from 0 to the count.</param>
/// <param name="value">The value to insert.</param>
/// <param name="builder">The builder.</param>
/// <returns>The same builder.</returns>
/// <exception cref="T:System.ArgumentOutOfRangeException">Thrown when the index is out of range.</exception>
[<CompiledName "InsertAt">]
val insertAt : index : int -> value : 'T -> builder : FlatList<'T>.Builder -> FlatList<'T>.Builder

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
/// O(n + m) for m values. The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableArray`1.Builder.InsertRange(System.Int32,System.Collections.Generic.IEnumerable{`0})"/>.
/// The index is checked before the values, as the BCL member does.
/// </remarks>
[<CompiledName "InsertManyAt">]
val insertManyAt : index : int -> values : seq<'T> -> builder : FlatList<'T>.Builder -> FlatList<'T>.Builder

/// <summary>
/// Removes the first element equal to a value, in <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>.
/// </summary>
/// <param name="value">The value to remove.</param>
/// <param name="builder">The builder.</param>
/// <returns><see langword="true"/> when an element was removed.</returns>
[<CompiledName "Remove">]
val remove : value : 'T -> builder : FlatList<'T>.Builder -> bool

/// <summary>
/// Removes every element that satisfies a predicate.
/// </summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="builder">The builder.</param>
/// <returns>The same builder.</returns>
/// <remarks>
/// O(n). The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableArray`1.Builder.RemoveAll(System.Predicate{`0})"/>.
/// </remarks>
[<CompiledName "RemoveAll">]
val removeAll : predicate : ('T -> bool) -> builder : FlatList<'T>.Builder -> FlatList<'T>.Builder

/// <summary>Removes the element at an index.</summary>
/// <param name="index">The index of the removed element.</param>
/// <param name="builder">The builder.</param>
/// <returns>The same builder.</returns>
/// <exception cref="T:System.ArgumentOutOfRangeException">Thrown when the index is out of range.</exception>
[<CompiledName "RemoveAt">]
val removeAt : index : int -> builder : FlatList<'T>.Builder -> FlatList<'T>.Builder

/// <summary>
/// Removes one element for each of the given values, in
/// <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>.
/// </summary>
/// <param name="items">The values to remove.</param>
/// <param name="builder">The builder.</param>
/// <returns>The same builder.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="items"/> is <see langword="null"/> or a default flat list.</exception>
/// <remarks>
/// O(n * m) for m values. Each value removes the first remaining element that equals it; a value that equals no remaining
/// element removes nothing. The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableArray`1.Builder.RemoveRange(System.Collections.Generic.IEnumerable{`0})"/>.
/// </remarks>
[<CompiledName "RemoveMany">]
val removeMany : items : seq<'T> -> builder : FlatList<'T>.Builder -> FlatList<'T>.Builder

/// <summary>
/// Removes a range of elements.
/// </summary>
/// <param name="index">The index of the first removed element.</param>
/// <param name="count">The number of removed elements.</param>
/// <param name="builder">The builder.</param>
/// <returns>The same builder.</returns>
/// <exception cref="T:System.ArgumentOutOfRangeException">Thrown when <paramref name="index"/> is out of range, or <paramref name="count"/> is negative or the range extends beyond the end of the builder.</exception>
/// <remarks>
/// O(n). The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableArray`1.Builder.RemoveRange(System.Int32,System.Int32)"/>. It
/// names the parameter <paramref name="count"/> where the BCL names it <c>length</c>.
/// </remarks>
[<CompiledName "RemoveManyAt">]
val removeManyAt : index : int -> count : int -> builder : FlatList<'T>.Builder -> FlatList<'T>.Builder

/// <summary>
/// Removes one element for each of the given values, in a given equality comparer.
/// </summary>
/// <param name="comparer">
/// The equality comparer. <see langword="null"/> stands for
/// <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>.
/// </param>
/// <param name="items">The values to remove.</param>
/// <param name="builder">The builder.</param>
/// <returns>The same builder.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="items"/> is <see langword="null"/> or a default flat list.</exception>
/// <remarks>
/// O(n * m) for m values. Each value removes the first remaining element that equals it; a value that equals no remaining
/// element removes nothing. The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableArray`1.Builder.RemoveRange(System.Collections.Generic.IEnumerable{`0},System.Collections.Generic.IEqualityComparer{`0})"/>.
/// </remarks>
[<CompiledName "RemoveManyWith">]
val removeManyWith : comparer : IEqualityComparer<'T> -> items : seq<'T> -> builder : FlatList<'T>.Builder -> FlatList<'T>.Builder

/// <summary>
/// Removes the first element equal to a value, in a given equality comparer.
/// </summary>
/// <param name="comparer">
/// The equality comparer. <see langword="null"/> stands for
/// <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>.
/// </param>
/// <param name="value">The value to remove.</param>
/// <param name="builder">The builder.</param>
/// <returns><see langword="true"/> when an element was removed.</returns>
/// <remarks>
/// O(n). The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableArray`1.Builder.Remove(`0,System.Collections.Generic.IEqualityComparer{`0})"/>.
/// </remarks>
[<CompiledName "RemoveWith">]
val removeWith : comparer : IEqualityComparer<'T> -> value : 'T -> builder : FlatList<'T>.Builder -> bool

/// <summary>
/// Replaces the first element that equals a value with another value, in
/// <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>.
/// </summary>
/// <param name="oldValue">The value to replace.</param>
/// <param name="newValue">The new value.</param>
/// <param name="builder">The builder.</param>
/// <returns>The same builder.</returns>
/// <remarks>
/// <para>
/// O(n). The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableArray`1.Builder.Replace(`0,`0)"/>.
/// </para>
/// <para>
/// The builder stays as it is when no element equals the old value: the BCL member does not throw. The function
/// <see cref="M:FSharp.Collections.Immutable.FlatList.Replace``1(``0,``0,System.Collections.Immutable.ImmutableArray{``0})"/>
/// of the flat list throws <see cref="T:System.ArgumentException"/> in that case.
/// </para>
/// </remarks>
[<CompiledName "Replace">]
val replace : oldValue : 'T -> newValue : 'T -> builder : FlatList<'T>.Builder -> FlatList<'T>.Builder

/// <summary>
/// Replaces the first element that equals a value with another value, in a given equality comparer.
/// </summary>
/// <param name="comparer">
/// The equality comparer. <see langword="null"/> stands for
/// <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>.
/// </param>
/// <param name="oldValue">The value to replace.</param>
/// <param name="newValue">The new value.</param>
/// <param name="builder">The builder.</param>
/// <returns>The same builder.</returns>
/// <remarks>
/// <para>
/// O(n). The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableArray`1.Builder.Replace(`0,`0,System.Collections.Generic.IEqualityComparer{`0})"/>.
/// </para>
/// <para>
/// The builder stays as it is when no element equals the old value: the BCL member does not throw. The function
/// <see cref="M:FSharp.Collections.Immutable.FlatList.ReplaceWith``1(System.Collections.Generic.IEqualityComparer{``0},``0,``0,System.Collections.Immutable.ImmutableArray{``0})"/>
/// of the flat list throws <see cref="T:System.ArgumentException"/> in that case.
/// </para>
/// </remarks>
[<CompiledName "ReplaceWith">]
val replaceWith :
    comparer : IEqualityComparer<'T> -> oldValue : 'T -> newValue : 'T -> builder : FlatList<'T>.Builder -> FlatList<'T>.Builder

/// <summary>Reverses the order of the elements.</summary>
/// <param name="builder">The builder.</param>
/// <returns>The same builder.</returns>
[<CompiledName "Reverse">]
val rev : builder : FlatList<'T>.Builder -> FlatList<'T>.Builder

/// <summary>
/// Sets the number of elements that the builder can hold without growing.
/// </summary>
/// <param name="capacity">The new capacity, not less than the count.</param>
/// <param name="builder">The builder.</param>
/// <returns>The same builder.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the capacity is less than the count.</exception>
[<CompiledName "SetCapacity">]
val setCapacity : capacity : int -> builder : FlatList<'T>.Builder -> FlatList<'T>.Builder

/// <summary>Replaces the element at an index.</summary>
/// <param name="index">The index of the replaced element.</param>
/// <param name="value">The new value.</param>
/// <param name="builder">The builder.</param>
/// <returns>The same builder.</returns>
/// <exception cref="T:System.IndexOutOfRangeException">Thrown when the index is out of range.</exception>
[<CompiledName "UpdateAt">]
val updateAt : index : int -> value : 'T -> builder : FlatList<'T>.Builder -> FlatList<'T>.Builder

// ---- Sorting ----

/// <summary>Sorts the elements in the order of F# generic comparison.</summary>
/// <param name="builder">The builder.</param>
/// <returns>The same builder.</returns>
/// <remarks>This is not a stable sort.</remarks>
[<CompiledName "Sort">]
val sort : builder : FlatList<'T>.Builder -> FlatList<'T>.Builder when 'T : comparison

/// <summary>
/// Sorts a range of the elements in the order of F# generic comparison.
/// </summary>
/// <param name="index">The index of the first element of the range.</param>
/// <param name="count">The number of elements of the range.</param>
/// <param name="builder">The builder.</param>
/// <returns>The same builder.</returns>
/// <exception cref="T:System.ArgumentOutOfRangeException">Thrown when <paramref name="index"/> or <paramref name="count"/> is negative, or the range extends beyond the end of the builder.</exception>
/// <remarks>
/// O(count log count). This is not a stable sort. The elements outside the range keep their places. The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableArray`1.Builder.Sort(System.Int32,System.Int32,System.Collections.Generic.IComparer{`0})"/>.
/// </remarks>
[<CompiledName "SortRange">]
val sortRange : index : int -> count : int -> builder : FlatList<'T>.Builder -> FlatList<'T>.Builder when 'T : comparison

/// <summary>
/// Sorts a range of the elements with a comparison function.
/// </summary>
/// <param name="comparison">The function that compares two elements.</param>
/// <param name="index">The index of the first element of the range.</param>
/// <param name="count">The number of elements of the range.</param>
/// <param name="builder">The builder.</param>
/// <returns>The same builder.</returns>
/// <exception cref="T:System.ArgumentOutOfRangeException">Thrown when <paramref name="index"/> or <paramref name="count"/> is negative, or the range extends beyond the end of the builder.</exception>
/// <remarks>
/// O(count log count). This is not a stable sort. The elements outside the range keep their places. The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableArray`1.Builder.Sort(System.Int32,System.Int32,System.Collections.Generic.IComparer{`0})"/>.
/// </remarks>
[<CompiledName "SortRangeWith">]
val sortRangeWith :
    comparison : ('T -> 'T -> int) -> index : int -> count : int -> builder : FlatList<'T>.Builder -> FlatList<'T>.Builder

/// <summary>
/// Sorts a range of the elements with a comparer.
/// </summary>
/// <param name="comparer">
/// The comparer. <see langword="null"/> stands for <see cref="P:System.Collections.Generic.Comparer`1.Default"/>.
/// </param>
/// <param name="index">The index of the first element of the range.</param>
/// <param name="count">The number of elements of the range.</param>
/// <param name="builder">The builder.</param>
/// <returns>The same builder.</returns>
/// <exception cref="T:System.ArgumentOutOfRangeException">Thrown when <paramref name="index"/> or <paramref name="count"/> is negative, or the range extends beyond the end of the builder.</exception>
/// <remarks>
/// O(count log count). This is not a stable sort. The elements outside the range keep their places. The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableArray`1.Builder.Sort(System.Int32,System.Int32,System.Collections.Generic.IComparer{`0})"/>.
/// </remarks>
[<CompiledName "SortRangeWithComparer">]
val sortRangeWithComparer :
    comparer : IComparer<'T> -> index : int -> count : int -> builder : FlatList<'T>.Builder -> FlatList<'T>.Builder

/// <summary>Sorts the elements with a comparison function.</summary>
/// <param name="comparer">The function that compares two elements.</param>
/// <param name="builder">The builder.</param>
/// <returns>The same builder.</returns>
/// <remarks>This is not a stable sort.</remarks>
[<CompiledName "SortWith">]
val sortWith : comparer : ('T -> 'T -> int) -> builder : FlatList<'T>.Builder -> FlatList<'T>.Builder

/// <summary>Sorts the elements with a comparer.</summary>
/// <param name="comparer">The comparer.</param>
/// <param name="builder">The builder.</param>
/// <returns>The same builder.</returns>
/// <remarks>This is not a stable sort.</remarks>
[<CompiledName "SortWithComparer">]
val sortWithComparer : comparer : IComparer<'T> -> builder : FlatList<'T>.Builder -> FlatList<'T>.Builder

// ---- Conversion ----

/// <summary>
/// Copies a range of the builder into an array.
/// </summary>
/// <param name="source">The builder.</param>
/// <param name="sourceIndex">The index of the first element to copy.</param>
/// <param name="target">The array to copy to.</param>
/// <param name="targetIndex">The index in the target array of the first copied element.</param>
/// <param name="count">The number of elements to copy.</param>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="target"/> is <see langword="null"/>.</exception>
/// <exception cref="T:System.ArgumentOutOfRangeException">Thrown when a range is outside its builder or array.</exception>
/// <remarks>
/// O(count). The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableArray`1.Builder.CopyTo(System.Int32,`0[],System.Int32,System.Int32)"/>.
/// It names the parameters <paramref name="target"/>, <paramref name="targetIndex"/> and <paramref name="count"/> where
/// the BCL names them <c>destination</c>, <c>destinationIndex</c> and <c>length</c>.
/// </remarks>
[<CompiledName "CopyTo">]
val blit : source : FlatList<'T>.Builder -> sourceIndex : int -> target : 'T array -> targetIndex : int -> count : int -> unit

/// <summary>
/// Returns the contents of the builder as a flat list and empties the builder.
/// </summary>
/// <param name="builder">The builder.</param>
/// <returns>The flat list of the elements; without a copy when the count equals the capacity.</returns>
[<CompiledName "DrainToFlatList">]
val drainToFlatList : builder : FlatList<'T>.Builder -> FlatList<'T>

/// <summary>Returns a new array with the elements of the builder.</summary>
/// <param name="builder">The builder.</param>
/// <returns>A copy of the elements.</returns>
[<CompiledName "ToArray">]
val toArray : builder : FlatList<'T>.Builder -> 'T array
