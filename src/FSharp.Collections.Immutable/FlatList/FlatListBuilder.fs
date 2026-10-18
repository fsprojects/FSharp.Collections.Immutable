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

open System
open System.Collections.Generic
open System.Collections.Immutable

// ---- Properties ----

/// <summary>
/// Returns the number of elements that the builder can hold without growing.
/// </summary>
/// <param name="builder">The builder.</param>
/// <returns>The capacity.</returns>
[<CompiledName "Capacity">]
let capacity (builder : FlatList<'T>.Builder) = builder.Capacity

/// <summary>Returns the number of elements in the builder.</summary>
/// <param name="builder">The builder.</param>
/// <returns>The number of elements.</returns>
[<CompiledName "Length">]
let length (builder : FlatList<'T>.Builder) = builder.Count

// ---- Access ----

/// <summary>Returns the element at an index.</summary>
/// <param name="index">The index of the element.</param>
/// <param name="builder">The builder.</param>
/// <returns>The element.</returns>
/// <exception cref="T:System.IndexOutOfRangeException">Thrown when the index is out of range.</exception>
[<CompiledName "Item">]
let item index (builder : FlatList<'T>.Builder) = builder[index]

// ---- Search ----

/// <summary>
/// Returns whether the builder contains a value, in <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>.
/// </summary>
/// <param name="value">The value to find.</param>
/// <param name="builder">The builder.</param>
/// <returns><see langword="true"/> when an element equals the value.</returns>
[<CompiledName "Contains">]
let contains (value : 'T) (builder : FlatList<'T>.Builder) = builder.Contains value

/// <summary>
/// Returns the index of the first element equal to a value, or -1.
/// </summary>
/// <param name="value">The value to find.</param>
/// <param name="builder">The builder.</param>
/// <returns>The index, or -1.</returns>
[<CompiledName "IndexOf">]
let indexOf (value : 'T) (builder : FlatList<'T>.Builder) = builder.IndexOf value

/// <summary>
/// Returns the index of the last element equal to a value, or -1.
/// </summary>
/// <param name="value">The value to find.</param>
/// <param name="builder">The builder.</param>
/// <returns>The index, or -1.</returns>
[<CompiledName "LastIndexOf">]
let lastIndexOf (value : 'T) (builder : FlatList<'T>.Builder) = builder.LastIndexOf value

// ---- Transformation ----

/// <summary>Adds a value after the last element.</summary>
/// <param name="value">The value to add.</param>
/// <param name="builder">The builder.</param>
/// <returns>The same builder.</returns>
[<CompiledName "Add">]
let add (value : 'T) (builder : FlatList<'T>.Builder) =
    builder.Add value
    builder

/// <summary>Adds values after the last element.</summary>
/// <param name="values">The values to add.</param>
/// <param name="builder">The builder.</param>
/// <returns>The same builder.</returns>
[<CompiledName "AddRange">]
let addRange (values : seq<'T>) (builder : FlatList<'T>.Builder) =
    builder.AddRange values
    builder

/// <summary>Removes every element.</summary>
/// <param name="builder">The builder.</param>
/// <returns>The same builder.</returns>
[<CompiledName "Clear">]
let clear (builder : FlatList<'T>.Builder) =
    builder.Clear ()
    builder

/// <summary>Inserts a value at an index.</summary>
/// <param name="index">The index of the inserted value, from 0 to the count.</param>
/// <param name="value">The value to insert.</param>
/// <param name="builder">The builder.</param>
/// <returns>The same builder.</returns>
/// <exception cref="T:System.ArgumentOutOfRangeException">Thrown when the index is out of range.</exception>
[<CompiledName "InsertAt">]
let insertAt index (value : 'T) (builder : FlatList<'T>.Builder) =
    builder.Insert (index, value)
    builder

/// <summary>
/// Removes the first element equal to a value, in <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>.
/// </summary>
/// <param name="value">The value to remove.</param>
/// <param name="builder">The builder.</param>
/// <returns><see langword="true"/> when an element was removed.</returns>
[<CompiledName "Remove">]
let remove (value : 'T) (builder : FlatList<'T>.Builder) = builder.Remove value

/// <summary>Removes the element at an index.</summary>
/// <param name="index">The index of the removed element.</param>
/// <param name="builder">The builder.</param>
/// <returns>The same builder.</returns>
/// <exception cref="T:System.ArgumentOutOfRangeException">Thrown when the index is out of range.</exception>
[<CompiledName "RemoveAt">]
let removeAt index (builder : FlatList<'T>.Builder) =
    builder.RemoveAt index
    builder

/// <summary>Reverses the order of the elements.</summary>
/// <param name="builder">The builder.</param>
/// <returns>The same builder.</returns>
[<CompiledName "Reverse">]
let rev (builder : FlatList<'T>.Builder) =
    builder.Reverse ()
    builder

/// <summary>
/// Sets the number of elements that the builder can hold without growing.
/// </summary>
/// <param name="capacity">The new capacity, not less than the count.</param>
/// <param name="builder">The builder.</param>
/// <returns>The same builder.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the capacity is less than the count.</exception>
[<CompiledName "SetCapacity">]
let setCapacity capacity (builder : FlatList<'T>.Builder) =
    builder.Capacity <- capacity
    builder

/// <summary>Replaces the element at an index.</summary>
/// <param name="index">The index of the replaced element.</param>
/// <param name="value">The new value.</param>
/// <param name="builder">The builder.</param>
/// <returns>The same builder.</returns>
/// <exception cref="T:System.IndexOutOfRangeException">Thrown when the index is out of range.</exception>
[<CompiledName "UpdateAt">]
let updateAt index (value : 'T) (builder : FlatList<'T>.Builder) =
    builder[index] <- value
    builder

// ---- Sorting ----

/// <summary>Sorts the elements in the order of F# generic comparison.</summary>
/// <param name="builder">The builder.</param>
/// <returns>The same builder.</returns>
/// <remarks>This is not a stable sort.</remarks>
[<CompiledName "Sort">]
let sort (builder : FlatList<'T>.Builder) =
    builder.Sort DefaultComparer<'T>.Instance
    builder

/// <summary>Sorts the elements with a comparison function.</summary>
/// <param name="comparer">The function that compares two elements.</param>
/// <param name="builder">The builder.</param>
/// <returns>The same builder.</returns>
/// <remarks>This is not a stable sort.</remarks>
[<CompiledName "SortWith">]
let sortWith (comparer : 'T -> 'T -> int) (builder : FlatList<'T>.Builder) =
    builder.Sort (Comparison<'T> comparer)
    builder

/// <summary>Sorts the elements with a comparer.</summary>
/// <param name="comparer">The comparer.</param>
/// <param name="builder">The builder.</param>
/// <returns>The same builder.</returns>
/// <remarks>This is not a stable sort.</remarks>
[<CompiledName "SortWithComparer">]
let sortWithComparer (comparer : IComparer<'T>) (builder : FlatList<'T>.Builder) =
    builder.Sort comparer
    builder

// ---- Conversion ----

/// <summary>
/// Returns the contents of the builder as a flat list and empties the builder.
/// </summary>
/// <param name="builder">The builder.</param>
/// <returns>The flat list of the elements; without a copy when the count equals the capacity.</returns>
[<CompiledName "DrainToFlatList">]
let drainToFlatList (builder : FlatList<'T>.Builder) : FlatList<'T> = builder.DrainToImmutable ()

/// <summary>Returns a new array with the elements of the builder.</summary>
/// <param name="builder">The builder.</param>
/// <returns>A copy of the elements.</returns>
[<CompiledName "ToArray">]
let toArray (builder : FlatList<'T>.Builder) = builder.ToArray ()
