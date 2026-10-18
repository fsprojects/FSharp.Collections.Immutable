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
/// Returns the index of the last element equal to a value, or -1.
/// </summary>
/// <param name="value">The value to find.</param>
/// <param name="builder">The builder.</param>
/// <returns>The index, or -1.</returns>
[<CompiledName "LastIndexOf">]
val lastIndexOf : value : 'T -> builder : FlatList<'T>.Builder -> int

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
/// Removes the first element equal to a value, in <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>.
/// </summary>
/// <param name="value">The value to remove.</param>
/// <param name="builder">The builder.</param>
/// <returns><see langword="true"/> when an element was removed.</returns>
[<CompiledName "Remove">]
val remove : value : 'T -> builder : FlatList<'T>.Builder -> bool

/// <summary>Removes the element at an index.</summary>
/// <param name="index">The index of the removed element.</param>
/// <param name="builder">The builder.</param>
/// <returns>The same builder.</returns>
/// <exception cref="T:System.ArgumentOutOfRangeException">Thrown when the index is out of range.</exception>
[<CompiledName "RemoveAt">]
val removeAt : index : int -> builder : FlatList<'T>.Builder -> FlatList<'T>.Builder

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
