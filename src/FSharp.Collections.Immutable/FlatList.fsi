// This file contains F# bindings to ImmutableArray from System.Collections.Immutable.
// It provides a flat list implementation that is optimized for performance and memory usage.
// The FlatList type is a wrapper around ImmutableArray, providing a more convenient API for working with immutable lists.
// FlatList code is designed to perform operations without allocating new arrays unnecessarily, making it suitable for high-performance applications.
#if INTERACTIVE
namespace global
#else
namespace FSharp.Collections.Immutable
#endif

open System
open System.Collections.Generic
open System.Collections.Immutable

// The FlatList name comes from a similar construct seen in the official F# source code
type FlatList<'T> = System.Collections.Immutable.ImmutableArray<'T>

// based on the F# Array module source
[<RequireQualifiedAccess;
  CompiledName((nameof System.Collections.Immutable.ImmutableArray)
               + "Module")>]
module FlatList =

    ////////// Creating //////////

    /// <summary>Creates a new builder with the specified capacity</summary>
    /// <param name="capacity">The initial capacity of the builder</param>
    /// <returns>An empty builder with the specified capacity</returns>
    [<CompiledName "BuilderWith">]
    val inline builderWith<'T> : capacity : int -> FlatList<'T>.Builder

    /// <summary>Builds a <see cref="FlatList{T}"/> from a builder, moving the elements and leaving the builder empty</summary>
    /// <param name="builder">The builder to build from</param>
    /// <returns>A <see cref="FlatList{T}"/> containing the elements from the builder</returns>
    /// <exception cref="System.ArgumentNullException">Thrown when builder is null</exception>
    [<CompiledName "MoveFromBuilder">]
    val moveFromBuilder<'T> : builder : FlatList<'T>.Builder -> FlatList<'T>

    /// <summary>Returns an empty <see cref="FlatList{T}"/></summary>
    /// <returns>An empty <see cref="FlatList{T}"/></returns>
    /// <example>
    /// <code>
    /// let emptyList = FlatList.empty&lt;int&gt;
    /// printfn "Is empty? %b" (FlatList.isEmpty emptyList) // true
    /// </code>
    /// </example>
    [<CompiledName "Empty">]
    val inline empty<'T> : FlatList<'T>

    /// <summary>Builds a <see cref="FlatList{T}"/> from the given <see cref="array{T}"/></summary>
    /// <param name="source">The <see cref="array{T}"/> to build the <see cref="FlatList{T}"/> from</param>
    /// <returns>A <see cref="FlatList{T}"/> containing the elements of the array</returns>
    [<CompiledName "OfArray">]
    val inline ofArray<'T> : source : 'T array -> FlatList<'T>

    /// <summary>Builds a <see cref="FlatList{T}"/> from the given <see cref="seq{T}"/></summary>
    /// <param name="source">The <see cref="seq{T}"/> to build the <see cref="FlatList{T}"/> from</param>
    /// <returns>A <see cref="FlatList{T}"/> containing the elements of the sequence</returns>
    [<CompiledName "OfSeq">]
    val inline ofSeq<'T> : source : seq<'T> -> FlatList<'T>

    /// <summary>Builds a <see cref="FlatList{T}"/> from the given <see cref="list{T}"/></summary>
    /// <param name="source">The <see cref="list{T}"/> to build the <see cref="FlatList{T}"/> from</param>
    /// <returns>A <see cref="FlatList{T}"/> containing the elements of the sequence</returns>
    [<CompiledName "OfList">]
    val inline ofList<'T> : source : 'T list -> FlatList<'T>

    /// <summary>Creates a list from a value option.</summary>
    /// <param name="option">The input option.</param>
    /// <returns>A list of one element if the option is Some, and an empty list if the option is None.</returns>
    [<CompiledName "OfOption">]
    val ofOption<'T> : option : 'T option -> FlatList<'T>

    /// <summary>Creates a list from a value option.</summary>
    /// <param name="voption">The input value option.</param>
    /// <returns>A list of one element if the option is ValueSome, and an empty list if the option is ValueNone.</returns>
    [<CompiledName "OfValueOption">]
    val ofValueOption<'T> : voption : 'T voption -> FlatList<'T>

    /// <summary>Returns a <see cref="FlatList{T}"/> with a single element</summary>
    /// <param name="item">The item to put into the <see cref="FlatList{T}"/></param>
    /// <returns>A <see cref="FlatList{T}"/> containing only the given item</returns>
    [<CompiledName "Singleton">]
    val inline singleton<'T> : item : 'T -> FlatList<'T>

    /// <summary>Creates a <see cref="FlatList{T}"/> by initializing each element with the given function</summary>
    /// <param name="count">The number of elements to create</param>
    /// <param name="initializer">The function to initialize each element</param>
    /// <returns>A new <see cref="FlatList{T}"/> with the initialized elements</returns>
    /// <exception cref="System.ArgumentException">Thrown when count is negative</exception>
    [<CompiledName "Init">]
    val init<'T> : count : int -> initializer : (int -> 'T) -> FlatList<'T>

    /// <summary>Creates a <see cref="FlatList{T}"/> of a given length with all elements set to the given value</summary>
    /// <param name="count">The length of the <see cref="FlatList{T}"/> to create</param>
    /// <param name="value">The value to replicate</param>
    /// <returns>A <see cref="FlatList{T}"/> of the specified length with all elements equal to the given value</returns>
    [<CompiledName "Create">]
    val create<'T> : count : int -> value : 'T -> FlatList<'T>

    /// <summary>Replicates a value into a <see cref="FlatList{T}"/> of a given length</summary>
    /// <param name="count">The length of the <see cref="FlatList{T}"/> to create</param>
    /// <param name="initial">The value to replicate</param>
    /// <returns>A <see cref="FlatList{T}"/> of the specified length with all elements equal to the given value</returns>
    [<CompiledName "Replicate">]
    val replicate<'T> : count : int -> initial : 'T -> FlatList<'T>

    /// <summary>Creates an <see cref="FlatList{T}"/> of a specified length, with all the elements initialized to the default zero value for the type.</summary>
    /// <param name="count">The length of the <see cref="FlatList{T}"/> to create.</param>
    /// <returns>The created <see cref="FlatList{T}"/>.</returns>
    [<CompiledName "ZeroCreate">]
    val zeroCreate<'T> : count : int -> FlatList<'T>

    /// <summary>Views the <see cref="FlatList{T}"/> as a <see cref="seq{T}"/></summary>
    /// <param name="flatList">The input <see cref="FlatList{T}"/></param>
    /// <returns>The <see cref="seq{T}"/> containing the elements of the <see cref="FlatList{T}"/></returns>
    [<CompiledName "ToSeq">]
    val inline toSeq<'T> : flatList : FlatList<'T> -> seq<'T>

    /// <summary>Builds an <see cref="array{T}"/> from the given <see cref="FlatList{T}"/></summary>
    /// <param name="list">The <see cref="FlatList{T}"/> to build the <see cref="array{T}"/> from</param>
    /// <returns>An <see cref="array{T}"/> containing the elements of the <see cref="FlatList{T}"/></returns>
    [<CompiledName "ToArray">]
    val inline toArray<'T> : list : FlatList<'T> -> 'T array

    /// <summary>Builds an <see cref="list{T}"/> from the given <see cref="FlatList{T}"/></summary>
    /// <param name="list">The <see cref="FlatList{T}"/> to build the <see cref="list{T}"/> from</param>
    /// <returns>An <see cref="list{T}"/> containing the elements of the <see cref="FlatList{T}"/></returns>
    [<CompiledName "ToList">]
    val toList<'T> : list : FlatList<'T> -> 'T list

    /// <summary>Converts a list to an option. If the list has one element, it returns <c>Some</c> of that element.
    /// Otherwise, it returns <c>None</c>.</summary>
    /// <param name="list">The input list.</param>
    /// <returns>An option representing the list's single element, or <c>None</c>.</returns>
    [<CompiledName "ToOption">]
    val toOption<'T> : list : FlatList<'T> -> 'T option

    /// <summary>Converts a list to a value option. If the list has one element, it returns <c>ValueSome</c> of that element.
    /// Otherwise, it returns <c>ValueNone</c>.</summary>
    /// <param name="list">The input list.</param>
    /// <returns>A value option representing the list's single element, or <c>ValueNone</c>.</returns>
    [<CompiledName "ToValueOption">]
    val toValueOption<'T> : list : FlatList<'T> -> 'T voption

    /// <summary>Builds a new <see cref="FlatList{T}"/> that contains the elements of the given <see cref="FlatList{T}"/>.</summary>
    /// <param name="list">The input <see cref="FlatList{T}"/>.</param>
    [<CompiledName "Copy">]
    val copy<'T> : list : FlatList<'T> -> FlatList<'T>

    ////////// Building //////////

    /// <summary>Builds a <see cref="FlatList{T}"/> from a builder, copying the elements</summary>
    /// <param name="builder">The builder to build from</param>
    /// <returns>A <see cref="FlatList{T}"/> containing the elements from the builder</returns>
    /// <exception cref="System.ArgumentNullException">Thrown when builder is null</exception>
    [<CompiledName "OfBuilder">]
    val ofBuilder<'T> : builder : FlatList<'T>.Builder -> FlatList<'T>

    /// <summary>Creates a new builder</summary>
    /// <returns>An empty builder</returns>
    [<CompiledName "Builder">]
    val inline builder<'T> : unit -> FlatList<'T>.Builder

    /// <summary>Creates a builder containing the elements of the input <see cref="FlatList{T}"/></summary>
    /// <param name="list">The <see cref="FlatList{T}"/> to create the builder from</param>
    /// <returns>A builder containing the elements of the <see cref="FlatList{T}"/></returns>
    [<CompiledName "ToBuilder">]
    val toBuilder<'T> : list : FlatList<'T> -> FlatList<'T>.Builder

    module Builder =

        /// <summary>Adds an item to the builder</summary>
        /// <param name="item">The item to add</param>
        /// <param name="builder">The builder to add to</param>
        [<CompiledName "Add">]
        val add<'T> : item : 'T -> builder : FlatList<'T>.Builder -> FlatList<'T>.Builder

    /// <summary>Checks if the <see cref="FlatList{T}"/> is empty</summary>
    /// <param name="list">The <see cref="FlatList{T}"/> to check</param>
    /// <returns>True if the <see cref="FlatList{T}"/> is empty, false otherwise</returns>
    [<CompiledName "IsEmpty">]
    val isEmpty<'T> : list : FlatList<'T> -> bool

    /// <summary>Checks if the <see cref="FlatList{T}"/> is uninstantiated</summary>
    /// <param name="list">The <see cref="FlatList{T}"/> to check</param>
    /// <returns>True if the <see cref="FlatList{T}"/> is uninstantiated, false otherwise</returns>
    [<CompiledName "IsDefault">]
    val isDefault<'T> : list : FlatList<'T> -> bool

    /// <summary>Checks if the <see cref="FlatList{T}"/> is uninstantiated or empty</summary>
    /// <param name="list">The <see cref="FlatList{T}"/> to check</param>
    /// <returns>True if the <see cref="FlatList{T}"/> is uninstantiated or empty, false otherwise</returns>
    [<CompiledName "IsDefaultOrEmpty">]
    val isDefaultOrEmpty<'T> : list : FlatList<'T> -> bool

    ////////// IReadOnly* //////////

    /// <summary>Returns the number of elements in the <see cref="FlatList{T}"/></summary>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The number of elements in the <see cref="FlatList{T}"/></returns>
    [<CompiledName "Length">]
    val length<'T> : list : FlatList<'T> -> int

    /// <summary>Gets the element at the specified index in the <see cref="FlatList{T}"/></summary>
    /// <param name="index">The index to retrieve</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The element at the specified index</returns>
    /// <exception cref="System.IndexOutOfRangeException">Thrown when the index is out of range</exception>
    [<CompiledName "Item">]
    val item<'T> : index : int -> list : FlatList<'T> -> 'T

    /// <summary>Appends two <see cref="FlatList{T}"/>s to create a new <see cref="FlatList{T}"/> containing all elements from both <see cref="FlatList{T}"/>s</summary>
    /// <param name="list1">The first <see cref="FlatList{T}"/></param>
    /// <param name="list2">The second <see cref="FlatList{T}"/></param>
    /// <returns>A new <see cref="FlatList{T}"/> containing all elements from both input <see cref="FlatList{T}"/>s</returns>
    [<CompiledName "Append">]
    val append<'T> : list1 : FlatList<'T> -> list2 : FlatList<'T> -> FlatList<'T>

    /// <summary>Searches for the specified object and returns the zero-based index of the first occurrence within the range
    /// of elements in the <see cref="FlatList{T}"/> that starts at the specified index and
    /// contains the specified number of elements.</summary>
    /// <param name="comparer">The equality comparer to use</param>
    /// <param name="index">The starting index</param>
    /// <param name="count">The number of elements to search</param>
    /// <param name="item">The item to search for</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The zero-based index of the first occurrence of the item</returns>
    [<CompiledName "IndexRangeWith">]
    val indexRangeWith<'T> :
        comparer : IEqualityComparer<'T> -> index : int -> count : int -> item : 'T -> list : FlatList<'T> -> int

    /// <summary>Searches for the specified object and returns the zero-based index of the first occurrence within the range
    /// of elements in the <see cref="FlatList{T}"/> that starts at the specified index and
    /// contains the specified number of elements.</summary>
    /// <param name="index">The starting index</param>
    /// <param name="count">The number of elements to search</param>
    /// <param name="item">The item to search for</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The zero-based index of the first occurrence of the item</returns>
    [<CompiledName "IndexRange">]
    val indexRange<'T when 'T : equality> : index : int -> count : int -> item : 'T -> list : FlatList<'T> -> int

    /// <summary>Searches for the specified object and returns the zero-based index of the first occurrence within the range
    /// of elements in the <see cref="FlatList{T}"/> that starts at the specified index and
    /// contains the specified number of elements.</summary>
    /// <param name="comparer">The equality comparer to use</param>
    /// <param name="index">The starting index</param>
    /// <param name="item">The item to search for</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The zero-based index of the first occurrence of the item</returns>
    [<CompiledName "IndexFromWith">]
    val indexFromWith<'T> : comparer : IEqualityComparer<'T> -> index : int -> item : 'T -> list : FlatList<'T> -> int

    /// <summary>Searches for the specified object and returns the zero-based index of the first occurrence within the range
    /// of elements in the <see cref="FlatList{T}"/> that starts at the specified index and
    /// contains the specified number of elements.</summary>
    /// <param name="index">The starting index</param>
    /// <param name="item">The item to search for</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The zero-based index of the first occurrence of the item</returns>
    [<CompiledName "IndexFrom">]
    val indexFrom<'T when 'T : equality> : index : int -> item : 'T -> list : FlatList<'T> -> int

    /// <summary>Searches for the specified object and returns the zero-based index of the first occurrence within the range
    /// of elements in the <see cref="FlatList{T}"/> that starts at the specified index and
    /// contains the specified number of elements.</summary>
    /// <param name="comparer">The equality comparer to use</param>
    /// <param name="item">The item to search for</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The zero-based index of the first occurrence of the item</returns>
    [<CompiledName "IndexWith">]
    val indexWith<'T> : comparer : IEqualityComparer<'T> -> item : 'T -> list : FlatList<'T> -> int

    /// <summary>Searches for the specified object and returns the zero-based index of the first occurrence within the range
    /// of elements in the <see cref="FlatList{T}"/> that starts at the specified index and
    /// contains the specified number of elements.</summary>
    /// <param name="item">The item to search for</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The zero-based index of the first occurrence of the item</returns>
    [<CompiledName "Index">]
    val index<'T when 'T : equality> : item : 'T -> list : FlatList<'T> -> int

    /// <summary>Searches for the specified object and returns the zero-based index of the last occurrence within the
    /// range of elements in the <see cref="FlatList{T}"/> that contains the specified number
    /// of elements and ends at the specified index.</summary>
    /// <param name="comparer">The equality comparer to use</param>
    /// <param name="index">The ending index</param>
    /// <param name="count">The number of elements to search</param>
    /// <param name="item">The item to search for</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The zero-based index of the last occurrence of the item</returns>
    [<CompiledName "LastIndexRangeWith">]
    val lastIndexRangeWith<'T> :
        comparer : IEqualityComparer<'T> -> index : int -> count : int -> item : 'T -> list : FlatList<'T> -> int

    /// <summary>Searches for the specified object and returns the zero-based index of the last occurrence within the
    /// range of elements in the <see cref="FlatList{T}"/> that contains the specified number
    /// of elements and ends at the specified index.</summary>
    /// <param name="index">The ending index</param>
    /// <param name="count">The number of elements to search</param>
    /// <param name="item">The item to search for</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The zero-based index of the last occurrence of the item</returns>
    [<CompiledName "LastIndexRange">]
    val lastIndexRange<'T when 'T : equality> : index : int -> count : int -> item : 'T -> list : FlatList<'T> -> int

    /// <summary>Searches for the specified object and returns the zero-based index of the last occurrence within the
    /// range of elements in the <see cref="FlatList{T}"/> that contains the specified number
    /// of elements and ends at the specified index.</summary>
    /// <param name="comparer">The equality comparer to use</param>
    /// <param name="index">The ending index</param>
    /// <param name="item">The item to search for</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The zero-based index of the last occurrence of the item</returns>
    [<CompiledName "LastIndexFromWith">]
    val lastIndexFromWith<'T> : comparer : IEqualityComparer<'T> -> index : int -> item : 'T -> list : FlatList<'T> -> int

    /// <summary>Searches for the specified object and returns the zero-based index of the last occurrence within the
    /// range of elements in the <see cref="FlatList{T}"/> that contains the specified number
    /// of elements and ends at the specified index.</summary>
    /// <param name="index">The ending index</param>
    /// <param name="item">The item to search for</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The zero-based index of the last occurrence of the item</returns>
    [<CompiledName "LastIndexFrom">]
    val lastIndexFrom<'T when 'T : equality> : index : int -> item : 'T -> list : FlatList<'T> -> int

    /// <summary>Searches for the specified object and returns the zero-based index of the last occurrence within the
    /// range of elements in the <see cref="FlatList{T}"/> that contains the specified number
    /// of elements and ends at the specified index.</summary>
    /// <param name="comparer">The equality comparer to use</param>
    /// <param name="item">The item to search for</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The zero-based index of the last occurrence of the item</returns>
    [<CompiledName "LastIndexWith">]
    val lastIndexWith<'T> : comparer : IEqualityComparer<'T> -> item : 'T -> list : FlatList<'T> -> int

    /// <summary>Searches for the specified object and returns the zero-based index of the last occurrence within the
    /// range of elements in the <see cref="FlatList{T}"/> that contains the specified number
    /// of elements and ends at the specified index.</summary>
    /// <param name="item">The item to search for</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The zero-based index of the last occurrence of the item</returns>
    [<CompiledName "LastIndex">]
    val lastIndex<'T when 'T : equality> : item : 'T -> list : FlatList<'T> -> int

    /// <summary>Removes the specified objects from the <see cref="FlatList{T}"/> with the given comparer.</summary>
    /// <param name="comparer">The equality comparer to use</param>
    /// <param name="items">The items to remove</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>A new <see cref="FlatList{T}"/> with the specified items removed</returns>
    [<CompiledName "RemoveAllWith">]
    val removeAllWith<'T> : comparer : IEqualityComparer<'T> -> items : 'T seq -> list : FlatList<'T> -> FlatList<'T>

    /// <summary>Removes the specified objects from the <see cref="FlatList{T}"/>.</summary>
    /// <param name="items">The items to remove</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>A new <see cref="FlatList{T}"/> with the specified items removed</returns>
    [<CompiledName "RemoveAll">]
    val removeAll<'T when 'T : equality> : items : 'T seq -> list : FlatList<'T> -> FlatList<'T>

    /// <summary>Removes all the elements that do not match the conditions defined by the specified predicate.</summary>
    /// <param name="predicate">The predicate to test elements</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>A new <see cref="FlatList{T}"/> with elements that match the predicate</returns>
    /// <example>
    /// <code>
    /// let numbers = FlatList.ofArray [|1; 2; 3; 4; 5; 6|]
    /// let evens = FlatList.filter (fun x -> x % 2 = 0) numbers
    /// // evens is [|2; 4; 6|]
    /// </code>
    /// </example>
    [<CompiledName "Filter">]
    val filter<'T> : predicate : ('T -> bool) -> list : FlatList<'T> -> FlatList<'T>

    /// <summary>Removes all the elements that do not match the conditions defined by the specified predicate.</summary>
    /// <param name="predicate">The predicate to test elements</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>A new <see cref="FlatList{T}"/> with elements that match the predicate</returns>
    [<CompiledName "Where">]
    val where<'T> : predicate : ('T -> bool) -> list : FlatList<'T> -> FlatList<'T>

    /// <summary>Removes a range of elements from the <see cref="FlatList{T}"/>.</summary>
    /// <param name="index">The starting index</param>
    /// <param name="count">The number of elements to remove</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>A new <see cref="FlatList{T}"/> with the specified range of elements removed</returns>
    [<CompiledName "RemoveRange">]
    val removeRange<'T> : index : int -> count : int -> list : FlatList<'T> -> FlatList<'T>

    /// <summary>Fills the elements of a list with a specified value.</summary>
    /// <param name="index">The starting index in the target list.</param>
    /// <param name="count">The number of elements to fill.</param>
    /// <param name="value">The value to fill with.</param>
    /// <param name="list">The input list.</param>
    /// <returns>A new list with the specified range filled with the value.</returns>
    [<CompiledName "Fill">]
    val fill<'T> : index : int -> count : int -> value : 'T -> list : FlatList<'T> -> FlatList<'T>

    /// <summary>Copies a range of elements from the source <see cref="FlatList{T}"/> to the destination array</summary>
    /// <param name="source">The source <see cref="FlatList{T}"/></param>
    /// <param name="sourceIndex">The starting index in the source <see cref="FlatList{T}"/></param>
    /// <param name="destination">The destination array</param>
    /// <param name="destinationIndex">The starting index in the destination array</param>
    /// <param name="count">The number of elements to copy</param>
    /// <exception cref="System.ArgumentException">Thrown when the range is invalid</exception>
    [<CompiledName "Blit">]
    val blit<'T> :
        source : FlatList<'T> -> sourceIndex : int -> destination : 'T[] -> destinationIndex : int -> count : int -> unit

    /// <summary>Sorts a range of elements in the <see cref="FlatList{T}"/> using the specified comparer</summary>
    /// <param name="comparer">The comparer to use</param>
    /// <param name="index">The starting index</param>
    /// <param name="count">The number of elements to sort</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>A new <see cref="FlatList{T}"/> with the specified range of elements sorted</returns>
    [<CompiledName "SortRangeWithComparer">]
    val sortRangeWithComparer<'T> : comparer : IComparer<'T> -> index : int -> count : int -> list : FlatList<'T> -> FlatList<'T>

    /// <summary>Sorts a range of elements in the <see cref="FlatList{T}"/> using the specified comparison function</summary>
    /// <param name="comparer">The comparison function to use</param>
    /// <param name="index">The starting index</param>
    /// <param name="count">The number of elements to sort</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>A new <see cref="FlatList{T}"/> with the specified range of elements sorted</returns>
    [<CompiledName "SortRangeWith">]
    val sortRangeWith<'T> : comparer : ('T -> 'T -> int) -> index : int -> count : int -> list : FlatList<'T> -> FlatList<'T>

    /// <summary>Sorts a range of elements in the <see cref="FlatList{T}"/> using the default comparer</summary>
    /// <param name="index">The starting index</param>
    /// <param name="count">The number of elements to sort</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>A new <see cref="FlatList{T}"/> with the specified range of elements sorted</returns>
    [<CompiledName "SortRange">]
    val sortRange<'T when 'T : comparison> : index : int -> count : int -> list : FlatList<'T> -> FlatList<'T>

    /// <summary>Sorts the elements in the <see cref="FlatList{T}"/> using the specified comparer</summary>
    /// <param name="comparer">The comparer to use</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>A new <see cref="FlatList{T}"/> with the elements sorted</returns>
    [<CompiledName "SortWithComparer">]
    val sortWithComparer<'T> : comparer : IComparer<'T> -> list : FlatList<'T> -> FlatList<'T>

    /// <summary>Sorts the elements in the <see cref="FlatList{T}"/> using the specified comparison function</summary>
    /// <param name="comparer">The comparison function to use</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>A new <see cref="FlatList{T}"/> with the elements sorted</returns>
    [<CompiledName "SortWith">]
    val sortWith<'T> : comparer : ('T -> 'T -> int) -> list : FlatList<'T> -> FlatList<'T>

    /// <summary>Sorts the elements in the <see cref="FlatList{T}"/> using the default comparer</summary>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>A new <see cref="FlatList{T}"/> with the elements sorted</returns>
    [<CompiledName "Sort">]
    val sort<'T> : list : FlatList<'T> -> FlatList<'T>

    /// <summary>Returns a new <see cref="FlatList{T}"/> with the elements in reverse order.</summary>
    /// <param name="list">The input <see cref="FlatList{T}"/>.</param>
    /// <returns>The reversed <see cref="FlatList{T}"/>.</returns>
    [<CompiledName "Rev">]
    val rev<'T> : list : FlatList<'T> -> FlatList<'T>

    /// <summary>Returns a new <see cref="FlatList{T}"/> that contains elements of the original <see cref="FlatList{T}"/> sorted in descending order.</summary>
    /// <param name="list">The input <see cref="FlatList{T}"/>.</param>
    /// <returns>The sorted <see cref="FlatList{T}"/>.</returns>
    [<CompiledName "SortDescending">]
    val inline sortDescending<'T when 'T : comparison> : list : FlatList<'T> -> FlatList<'T>

    /// <summary>Returns a new <see cref="FlatList{T}"/> that contains elements of the original <see cref="FlatList{T}"/> sorted in descending order using the specified projection.</summary>
    /// <param name="projection">The function to transform the elements into a type that supports comparison.</param>
    /// <param name="list">The input <see cref="FlatList{T}"/>.</param>
    /// <returns>The sorted <see cref="FlatList{T}"/>.</returns>
    [<CompiledName "SortByDescending">]
    val inline sortByDescending<'T, 'Key when 'Key : comparison> :
        projection : ('T -> 'Key) -> list : FlatList<'T> -> FlatList<'T>

    /// <summary>Sorts the <see cref="FlatList{T}"/> using keys given by the given projection. Keys are compared using Operators.compare.</summary>
    /// <param name="projection">The function to transform the elements into a type supporting comparison.</param>
    /// <param name="list">The input <see cref="FlatList{T}"/>.</param>
    /// <returns>The sorted <see cref="FlatList{T}"/>.</returns>
    [<CompiledName "SortBy">]
    val sortBy<'T, 'Key when 'Key : comparison> : projection : ('T -> 'Key) -> list : FlatList<'T> -> FlatList<'T>

    ////////// Loop-based (now LINQ-based where applicable) //////////

    /// <summary>Concatenates a <see cref="FlatList{T}"/> of <see cref="FlatList{T}"/>s into a single <see cref="FlatList{T}"/></summary>
    /// <param name="arrs">The <see cref="FlatList{T}"/> of <see cref="FlatList{T}"/>s to concatenate</param>
    /// <returns>A new <see cref="FlatList{T}"/> containing all elements from the input <see cref="FlatList{T}"/>s</returns>
    [<CompiledName "Concat">]
    val concat<'T> : arrs : FlatList<FlatList<'T>> -> FlatList<'T>

    /// <summary>Builds a new <see cref="FlatList{T}"/> from the elements of a <see cref="FlatList{T}"/> by applying a mapping function to each element</summary>
    /// <param name="mapping">A function to transform elements from the input <see cref="FlatList{T}"/></param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>A <see cref="FlatList{T}"/> containing the transformed elements</returns>
    /// <example>
    /// <code>
    /// let numbers = FlatList.ofArray [|1; 2; 3; 4; 5|]
    /// let squares = FlatList.map (fun x -> x * x) numbers
    /// // squares is [|1; 4; 9; 16; 25|]
    /// </code>
    /// </example>
    [<CompiledName "Map">]
    val inline map<'T, 'U> : mapping : ('T -> 'U) -> list : FlatList<'T> -> FlatList<'U>

    /// <summary>Builds a new <see cref="FlatList{T}"/> whose elements are the results of applying the given function
    /// to each of the elements of the <see cref="FlatList{T}"/>. The integer index passed to the
    /// function indicates the index of element being transformed.</summary>
    /// <param name="mapping">A function to transform an element and its index into a result element.</param>
    /// <param name="list">The input <see cref="FlatList{T}"/>.</param>
    /// <returns>The <see cref="FlatList{T}"/> of transformed elements.</returns>
    [<CompiledName "Mapi">]
    val mapi<'T, 'U> : mapping : (int -> 'T -> 'U) -> list : FlatList<'T> -> FlatList<'U>

    /// <summary>Builds a new <see cref="FlatList{T}"/> whose elements are the results of applying the given function
    /// to the corresponding elements of the two collections pairwise, also passing the index of
    /// the elements. The two input <see cref="FlatList{T}"/>s must have the same lengths.</summary>
    /// <param name="mapping">The function to transform pairs of input elements and their indices.</param>
    /// <param name="list1">The first input <see cref="FlatList{T}"/>.</param>
    /// <param name="list2">The second input <see cref="FlatList{T}"/>.</param>
    /// <returns>The <see cref="FlatList{T}"/> of transformed elements.</returns>
    [<CompiledName "Mapi2">]
    val mapi2<'T1, 'T2, 'U> :
        mapping : (int -> 'T1 -> 'T2 -> 'U) -> list1 : FlatList<'T1> -> list2 : FlatList<'T2> -> FlatList<'U>

    /// <summary>Builds a new <see cref="FlatList{T}"/> whose elements are the results of applying the given function
    /// to the corresponding elements of the three collections pairwise, also passing the index of
    /// the elements. The three input <see cref="FlatList{T}"/>s must have the same lengths.</summary>
    /// <param name="mapping">The function to transform triples of input elements and their indices.</param>
    /// <param name="list1">The first input <see cref="FlatList{T}"/>.</param>
    /// <param name="list2">The second input <see cref="FlatList{T}"/>.</param>
    /// <param name="list3">The third input <see cref="FlatList{T}"/>.</param>
    /// <returns>The <see cref="FlatList{T}"/> of transformed elements.</returns>
    [<CompiledName "Mapi3">]
    val mapi3<'T1, 'T2, 'T3, 'U> :
        mapping : (int -> 'T1 -> 'T2 -> 'T3 -> 'U) ->
        list1 : FlatList<'T1> ->
        list2 : FlatList<'T2> ->
        list3 : FlatList<'T3> ->
            FlatList<'U>

    /// <summary>Builds a new collection whose elements are the results of applying the given function
    /// to the corresponding elements of the two collections pairwise. The two input
    /// <see cref="FlatList{T}"/>s must have the same lengths.</summary>
    /// <param name="mapping">The function to transform the pairs of the input elements.</param>
    /// <param name="list1">The first input <see cref="FlatList{T}"/>.</param>
    /// <param name="list2">The second input <see cref="FlatList{T}"/>.</param>
    /// <returns>The <see cref="FlatList{T}"/> of transformed elements.</returns>
    [<CompiledName "Map2">]
    val map2<'T1, 'T2, 'U> : mapping : ('T1 -> 'T2 -> 'U) -> list1 : FlatList<'T1> -> list2 : FlatList<'T2> -> FlatList<'U>

    /// <summary>Builds a new collection whose elements are the results of applying the given function
    /// to the corresponding elements of the three collections pairwise. The three input
    /// <see cref="FlatList{T}"/>s must have the same lengths.</summary>
    /// <param name="mapping">The function to transform the triples of the input elements.</param>
    /// <param name="list1">The first input <see cref="FlatList{T}"/>.</param>
    /// <param name="list2">The second input <see cref="FlatList{T}"/>.</param>
    /// <param name="list3">The third input <see cref="FlatList{T}"/>.</param>
    /// <returns>The <see cref="FlatList{T}"/> of transformed elements.</returns>
    [<CompiledName "Map3">]
    val map3<'T1, 'T2, 'T3, 'U> :
        mapping : ('T1 -> 'T2 -> 'T3 -> 'U) ->
        list1 : FlatList<'T1> ->
        list2 : FlatList<'T2> ->
        list3 : FlatList<'T3> ->
            FlatList<'U>

    /// <summary>Builds a new <see cref="FlatList{U}"/> whose elements are the results of applying the given function
    /// to each of the elements of the <see cref="FlatList{T}"/> while threading an accumulator argument
    /// through the computation.</summary>
    /// <param name="mapping">The function to transform elements from the input <see cref="FlatList{T}"/> and
    /// thread an accumulator state.</param>
    /// <param name="state">The initial state of the accumulator.</param>
    /// <param name="list">The input <see cref="FlatList{T}"/>.</param>
    /// <returns>A <see cref="FlatList{U}"/> of transformed elements, and the final accumulator value.</returns>
    /// <example>
    /// <code>
    /// // Calculate a running sum while squaring each element
    /// let numbers = FlatList.ofArray [|1; 2; 3; 4|]
    /// let squares, sum = FlatList.mapFold (fun state x -> x * x, state + x) 0 numbers
    /// // squares is [|1; 4; 9; 16|]
    /// // sum is 10
    /// </code>
    /// </example>
    [<CompiledName "MapFold">]
    val mapFold<'T, 'State, 'Result> :
        mapping : ('State -> 'T -> 'Result * 'State) -> state : 'State -> list : FlatList<'T> -> FlatList<'Result> * 'State

    /// <summary>Builds a new <see cref="FlatList{U}"/> whose elements are the results of applying the given function
    /// to each of the elements of the <see cref="FlatList{T}"/> while threading an accumulator argument
    /// through the computation, starting from the end of the list.</summary>
    /// <param name="mapping">The function to transform elements from the input <see cref="FlatList{T}"/> and
    /// thread an accumulator state, starting from the end.</param>
    /// <param name="list">The input <see cref="FlatList{T}"/>.</param>
    /// <param name="state">The initial state of the accumulator.</param>
    /// <returns>A <see cref="FlatList{U}"/> of transformed elements, and the final accumulator value.</returns>
    /// <example>
    /// <code>
    /// // Create a reverse-order list of indices while computing sum
    /// let chars = FlatList.ofArray [|'a'; 'b'; 'c'|]
    /// let indices, sum = FlatList.mapFoldBack (fun x state -> state, state + 1) chars 0
    /// // indices is [|2; 1; 0|]
    /// // sum is 3
    /// </code>
    /// </example>
    [<CompiledName "MapFoldBack">]
    val mapFoldBack<'T, 'State, 'Result> :
        mapping : ('T -> 'State -> 'Result * 'State) -> list : FlatList<'T> -> state : 'State -> FlatList<'Result> * 'State

    /// <summary>Counts the number of elements in the <see cref="FlatList{T}"/> that satisfy the given predicate</summary>
    /// <param name="projection">A function to project elements from the input <see cref="FlatList{T}"/></param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>A <see cref="FlatList{T}"/> of key-value pairs where the key is the projected value and the value is the count</returns>
    [<CompiledName "CountBy">]
    val countBy<'T, 'Key> : projection : ('T -> 'Key) -> list : FlatList<'T> -> FlatList<struct ('Key * int)>

    /// <summary>Creates a <see cref="FlatList{T}"/> containing the elements of the original <see cref="FlatList{T}"/> paired with their indices</summary>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>A <see cref="FlatList{T}"/> containing pairs of indices and elements</returns>
    [<CompiledName "Indexed">]
    val indexed<'T> : list : FlatList<'T> -> FlatList<struct (int * 'T)>

    /// <summary>Applies the given function to each element of the <see cref="FlatList{T}"/></summary>
    /// <param name="action">A function to apply to each element</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    [<CompiledName "Iter">]
    val inline iter<'T> : action : ('T -> unit) -> list : FlatList<'T> -> unit

    /// <summary>Applies the given function to each element of the <see cref="FlatList{T}"/> and its index</summary>
    /// <param name="action">A function to apply to each element and its index</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    [<CompiledName "Iteri">]
    val iteri<'T> : action : (int -> 'T -> unit) -> list : FlatList<'T> -> unit

    /// <summary>Applies the given function to pair of elements at the same position in the two <see cref="FlatList{T}"/>s</summary>
    /// <param name="action">A function to apply to pairs of elements</param>
    /// <param name="list1">The first input <see cref="FlatList{T}"/></param>
    /// <param name="list2">The second input <see cref="FlatList{T}"/></param>
    /// <exception cref="System.ArgumentException">Thrown when the <see cref="FlatList{T}"/>s have different lengths</exception>
    [<CompiledName "Iter2">]
    val iter2<'T1, 'T2> : action : ('T1 -> 'T2 -> unit) -> list1 : FlatList<'T1> -> list2 : FlatList<'T2> -> unit

    /// <summary>Applies the given function to the trio of elements at the same position in the three <see cref="FlatList{T}"/>s.</summary>
    /// <param name="action">A function to apply to trios of elements.</param>
    /// <param name="list1">The first input <see cref="FlatList{T}"/></param>
    /// <param name="list2">The second input <see cref="FlatList{T}"/></param>
    /// <param name="list3">The third input <see cref="FlatList{T}"/></param>
    /// <exception cref="System.ArgumentException">Thrown when the <see cref="FlatList{T}"/>s have different lengths</exception>
    [<CompiledName "Iter3">]
    val iter3<'T1, 'T2, 'T3> :
        action : ('T1 -> 'T2 -> 'T3 -> unit) -> list1 : FlatList<'T1> -> list2 : FlatList<'T2> -> list3 : FlatList<'T3> -> unit

    /// <summary>Applies the given function to the pair of elements at the same position in the two <see cref="FlatList{T}"/>s along with their index</summary>
    /// <param name="action">A function to apply to pairs of elements and their index</param>
    /// <param name="list1">The first input <see cref="FlatList{T}"/></param>
    /// <param name="list2">The second input <see cref="FlatList{T}"/></param>
    /// <exception cref="System.ArgumentException">Thrown when the <see cref="FlatList{T}"/>s have different lengths</exception>
    [<CompiledName "Iteri2">]
    val iteri2<'T1, 'T2> : action : (int -> 'T1 -> 'T2 -> unit) -> list1 : FlatList<'T1> -> list2 : FlatList<'T2> -> unit

    /// <summary>Applies the given function to the trio of elements at the same position in the three <see cref="FlatList{T}"/>s along with their index.</summary>
    /// <param name="action">A function to apply to trios of elements and their index.</param>
    /// <param name="list1">The first input <see cref="FlatList{T}"/></param>
    /// <param name="list2">The second input <see cref="FlatList{T}"/></param>
    /// <param name="list3">The third input <see cref="FlatList{T}"/></param>
    /// <exception cref="System.ArgumentException">Thrown when the <see cref="FlatList{T}"/>s have different lengths</exception>
    [<CompiledName "Iteri3">]
    val iteri3<'T1, 'T2, 'T3> :
        action : (int -> 'T1 -> 'T2 -> 'T3 -> unit) ->
        list1 : FlatList<'T1> ->
        list2 : FlatList<'T2> ->
        list3 : FlatList<'T3> ->
            unit

    /// <summary>Tests if any element of the <see cref="FlatList{T}"/> satisfies the given predicate</summary>
    /// <param name="predicate">A function to test each element</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>True if any element satisfies the predicate, false otherwise</returns>
    [<CompiledName "Exists">]
    val exists<'T> : predicate : ('T -> bool) -> list : FlatList<'T> -> bool

    /// <summary>Tests if any corresponding pair of elements from the two <see cref="FlatList{T}"/>s satisfies the given predicate</summary>
    /// <param name="predicate">A function to test pairs of elements</param>
    /// <param name="list1">The first input <see cref="FlatList{T}"/></param>
    /// <param name="list2">The second input <see cref="FlatList{T}"/></param>
    /// <returns>True if any pair of elements satisfies the predicate, false otherwise</returns>
    /// <exception cref="System.ArgumentException">Thrown when the <see cref="FlatList{T}"/>s have different lengths</exception>
    [<CompiledName "Exists2">]
    val exists2<'T1, 'T2> : predicate : ('T1 -> 'T2 -> bool) -> list1 : FlatList<'T1> -> list2 : FlatList<'T2> -> bool

    /// <summary>Tests if any corresponding trio of elements from the three <see cref="FlatList{T}"/>s satisfies the given predicate.</summary>
    /// <param name="predicate">A function to test trios of elements.</param>
    /// <param name="list1">The first input <see cref="FlatList{T}"/></param>
    /// <param name="list2">The second input <see cref="FlatList{T}"/></param>
    /// <param name="list3">The third input <see cref="FlatList{T}"/></param>
    /// <returns>True if any trio of elements satisfies the predicate, false otherwise.</returns>
    /// <exception cref="System.ArgumentException">Thrown when the <see cref="FlatList{T}"/>s have different lengths</exception>
    [<CompiledName "Exists3">]
    val exists3<'T1, 'T2, 'T3> :
        predicate : ('T1 -> 'T2 -> 'T3 -> bool) -> list1 : FlatList<'T1> -> list2 : FlatList<'T2> -> list3 : FlatList<'T3> -> bool

    /// <summary>Tests if all elements of the <see cref="FlatList{T}"/> satisfy the given predicate</summary>
    /// <param name="predicate">A function to test each element</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>True if all elements satisfy the predicate, false otherwise</returns>
    [<CompiledName "Forall">]
    val forall<'T> : predicate : ('T -> bool) -> list : FlatList<'T> -> bool

    /// <summary>Tests if all corresponding pairs of elements from the two <see cref="FlatList{T}"/>s satisfy the given predicate</summary>
    /// <param name="predicate">A function to test pairs of elements</param>
    /// <param name="list1">The first input <see cref="FlatList{T}"/></param>
    /// <param name="list2">The second input <see cref="FlatList{T}"/></param>
    /// <returns>True if all pairs of elements satisfy the predicate, false otherwise</returns>
    /// <exception cref="System.ArgumentException">Thrown when the <see cref="FlatList{T}"/>s have different lengths</exception>
    [<CompiledName "Forall2">]
    val forall2<'T1, 'T2> : predicate : ('T1 -> 'T2 -> bool) -> list1 : FlatList<'T1> -> list2 : FlatList<'T2> -> bool

    /// <summary>Tests if all corresponding trios of elements from the three <see cref="FlatList{T}"/>s satisfy the given predicate.</summary>
    /// <param name="predicate">A function to test trios of elements.</param>
    /// <param name="list1">The first input <see cref="FlatList{T}"/></param>
    /// <param name="list2">The second input <see cref="FlatList{T}"/></param>
    /// <param name="list3">The third input <see cref="FlatList{T}"/></param>
    /// <returns>True if all trios of elements satisfy the predicate, false otherwise.</returns>
    /// <exception cref="System.ArgumentException">Thrown when the <see cref="FlatList{T}"/>s have different lengths</exception>
    [<CompiledName "Forall3">]
    val forall3<'T1, 'T2, 'T3> :
        predicate : ('T1 -> 'T2 -> 'T3 -> bool) -> list1 : FlatList<'T1> -> list2 : FlatList<'T2> -> list3 : FlatList<'T3> -> bool

    /// <summary>Tests if the given element exists in the <see cref="FlatList{T}"/></summary>
    /// <param name="e">The element to find</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>True if the element exists in the <see cref="FlatList{T}"/>, false otherwise</returns>
    [<CompiledName "Contains">]
    val inline contains<'T> : item : 'T -> list : FlatList<'T> -> bool

    /// <summary>Splits the <see cref="FlatList{T}"/> into two <see cref="FlatList{T}"/>s, containing the elements for which the given predicate returns true and false respectively</summary>
    /// <param name="predicate">A function to test each element</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>A tuple of two <see cref="FlatList{T}"/>s, containing the elements for which the predicate returns true and false respectively</returns>
    [<CompiledName "Partition">]
    val partition<'T> : predicate : ('T -> bool) -> list : FlatList<'T> -> FlatList<'T> * FlatList<'T>

    /// <summary>Returns the first element for which the given predicate returns true</summary>
    /// <param name="predicate">A function to test elements</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The first element for which the predicate returns true</returns>
    /// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown if no element satisfies the predicate</exception>
    [<CompiledName "Find">]
    val find<'T> : predicate : ('T -> bool) -> list : FlatList<'T> -> 'T

    /// <summary>Returns the first element for which the given predicate returns true, or ValueNone if no such element exists</summary>
    /// <param name="predicate">A function to test elements</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>ValueSome value if an element satisfies the predicate, ValueNone otherwise</returns>
    [<CompiledName "TryFind">]
    val tryFind<'T> : predicate : ('T -> bool) -> list : FlatList<'T> -> 'T voption

    /// <summary>Returns the last element for which the given predicate returns true</summary>
    /// <param name="predicate">A function to test elements</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The last element for which the predicate returns true</returns>
    /// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown if no element satisfies the predicate</exception>
    [<CompiledName "FindBack">]
    val findBack<'T> : predicate : ('T -> bool) -> list : FlatList<'T> -> 'T

    /// <summary>Returns the last element for which the given predicate returns true, or ValueNone if no such element exists</summary>
    /// <param name="predicate">A function to test elements</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>ValueSome value if an element satisfies the predicate, ValueNone otherwise</returns>
    [<CompiledName "TryFindBack">]
    val tryFindBack<'T> : predicate : ('T -> bool) -> list : FlatList<'T> -> 'T voption

    /// <summary>Returns the last element for which the given function returns true.</summary>
    /// <param name="predicate">The function to test elements.</param>
    /// <param name="list">The input list.</param>
    /// <returns>The last element for which the predicate returns true.</returns>
    /// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown if no element satisfies the predicate.</exception>
    [<CompiledName "FindLast">]
    val findLast<'T> : predicate : ('T -> bool) -> list : FlatList<'T> -> 'T

    /// <summary>Returns the last element for which the given function returns true, or ValueNone if no such element exists.</summary>
    /// <param name="predicate">The function to test elements.</param>
    /// <param name="list">The input list.</param>
    /// <returns>ValueSome value if an element satisfies the predicate, ValueNone otherwise</returns>
    [<CompiledName "TryFindLast">]
    val tryFindLast<'T> : predicate : ('T -> bool) -> list : FlatList<'T> -> 'T voption

    /// <summary>Returns the last index for which the given predicate returns true</summary>
    /// <param name="predicate">A function to test elements</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The last index for which the predicate returns true</returns>
    /// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown if no element satisfies the predicate</exception>
    [<CompiledName "FindIndexBack">]
    val findIndexBack<'T> : predicate : ('T -> bool) -> list : FlatList<'T> -> int

    /// <summary>Returns the last index for which the given predicate returns true, or ValueNone if no such element exists</summary>
    /// <param name="predicate">A function to test elements</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>ValueSome index if an element satisfies the predicate, ValueNone otherwise</returns>
    [<CompiledName "TryFindIndexBack">]
    val tryFindIndexBack<'T> : predicate : ('T -> bool) -> list : FlatList<'T> -> int voption

    /// <summary>Returns the index of the last element in the <see cref="FlatList{T}"/> that satisfies the given predicate.</summary>
    /// <param name="predicate">The function to test elements.</param>
    /// <param name="list">The input list.</param>
    /// <returns>The index of the last element that satisfies the predicate.</returns>
    /// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown if no element satisfies the predicate.</exception>
    [<CompiledName "FindLastIndex">]
    val findLastIndex<'T> : predicate : ('T -> bool) -> list : FlatList<'T> -> int

    /// <summary>Returns the index of the last element in the <see cref="FlatList{T}"/> that satisfies the given predicate, or ValueNone if no such element exists.</summary>
    /// <param name="predicate">The function to test elements.</param>
    /// <param name="list">The input list.</param>
    /// <returns>ValueSome index if an element satisfies the predicate, ValueNone otherwise</returns>
    [<CompiledName "TryFindLastIndex">]
    val tryFindLastIndex<'T> : predicate : ('T -> bool) -> list : FlatList<'T> -> int voption

    /// <summary>Returns the first value for which the given function returns ValueSome value</summary>
    /// <param name="chooser">A function to generate options from the elements</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The first value for which the chooser returns ValueSome value</returns>
    /// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown if the chooser returns ValueNone for all elements</exception>
    [<CompiledName "Pick">]
    val pick<'T, 'U> : chooser : ('T -> 'U voption) -> list : FlatList<'T> -> 'U

    /// <summary>Returns the first value for which the given function returns ValueSome value, or ValueNone</summary>
    /// <param name="chooser">A function to generate options from the elements</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The first value for which the chooser returns ValueSome value, or ValueNone</returns>
    [<CompiledName "TryPick">]
    val tryPick<'T, 'U> : chooser : ('T -> 'U voption) -> list : FlatList<'T> -> 'U voption

    /// <summary>Returns the last value for which the given function returns ValueSome value.</summary>
    /// <param name="chooser">A function to generate options from the elements.</param>
    /// <param name="list">The input list.</param>
    /// <returns>The last value for which the chooser returns ValueSome value.</returns>
    /// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown if the chooser returns ValueNone for all elements.</exception>
    [<CompiledName "PickBack">]
    val pickBack<'T, 'U> : chooser : ('T -> 'U voption) -> list : FlatList<'T> -> 'U

    /// <summary>Returns the last value for which the given function returns ValueSome value.</summary>
    /// <param name="chooser">A function to generate options from the elements.</param>
    /// <param name="list">The input list.</param>
    /// <returns>The last value for which the chooser returns ValueSome value, or ValueNone.</returns>
    [<CompiledName "TryPickBack">]
    val tryPickBack<'T, 'U> : chooser : ('T -> 'U voption) -> list : FlatList<'T> -> 'U voption

    /// <summary>Builds a new <see cref="FlatList{T}"/> containing only the elements for which the given function returns ValueSome value</summary>
    /// <param name="chooser">A function to generate options from the elements</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>A <see cref="FlatList{T}"/> containing the values wrapped in ValueSome by the chooser</returns>
    [<CompiledName "Choose">]
    val choose<'T, 'U> : chooser : ('T -> 'U voption) -> list : FlatList<'T> -> FlatList<'U>

    /// <summary>Builds a new collection from the elements of the input collection for which the given function returns a <c>ValueSome</c> value.
    /// The elements are processed in reverse order.</summary>
    /// <param name="chooser">A function to generate options from the elements.</param>
    /// <param name="list">The input list.</param>
    /// <returns>A new list containing the values from the successful choices, in reverse order of processing.</returns>
    [<CompiledName "ChooseBack">]
    val chooseBack<'T, 'U> : chooser : ('T -> 'U voption) -> list : FlatList<'T> -> FlatList<'U>

    /// <summary>Creates a <see cref="FlatList{T}"/> by applying a key-generating function to each element of the <see cref="FlatList{T}"/> and grouping the elements by the resulting keys</summary>
    /// <param name="projection">A function to transform elements into keys</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>A <see cref="FlatList{T}"/> of tuples where each tuple contains a key and a <see cref="FlatList{T}"/> of all elements that match the key</returns>
    [<CompiledName "GroupBy">]
    val groupBy<'T, 'Key> : projection : ('T -> 'Key) -> list : FlatList<'T> -> FlatList<struct ('Key * FlatList<'T>)>

    /// <summary>Returns a new <see cref="FlatList{T}"/> that contains the elements of the original <see cref="FlatList{T}"/> but with duplicates eliminated by using the supplied projection function</summary>
    /// <param name="projection">A function to transform elements before comparing them</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>A <see cref="FlatList{T}"/> with distinct elements as determined by the projection function</returns>
    [<CompiledName "DistinctBy">]
    val distinctBy<'T, 'Key> : projection : ('T -> 'Key) -> list : FlatList<'T> -> FlatList<'T>

    /// <summary>Finds the first duplicate element in the <see cref="FlatList{T}"/>.</summary>
    /// <param name="list">The input <see cref="FlatList{T}"/>.</param>
    /// <returns>The first duplicate element.</returns>
    /// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown if no duplicate is found.</exception>
    [<CompiledName "FindDup">]
    val findDup<'T> : list : FlatList<'T> -> 'T

    /// <summary>Finds the first element in the <see cref="FlatList{T}"/> that is a duplicate of a preceding element according to the given projection function.</summary>
    /// <param name="projection">The function to transform the elements into a type supporting comparison.</param>
    /// <param name="list">The input <see cref="FlatList{T}"/>.</param>
    /// <returns>The first duplicate element.</returns>
    /// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown if no duplicate is found.</exception>
    [<CompiledName "FindDupBy">]
    val findDupBy<'T, 'Key> : projection : ('T -> 'Key) -> list : FlatList<'T> -> 'T

    /// <summary>Creates a new <see cref="FlatList{T}"/> by applying a mapping function to each element of the input <see cref="FlatList{T}"/> and concatenating the results</summary>
    /// <param name="mapping">A function to transform elements of the input <see cref="FlatList{T}"/> into <see cref="FlatList{T}"/>s</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>A <see cref="FlatList{T}"/> containing the concatenation of all the <see cref="FlatList{T}"/>s generated by the mapping function</returns>
    [<CompiledName "Collect">]
    val collect<'T, 'U> : mapping : ('T -> 'U seq) -> list : FlatList<'T> -> FlatList<'U>

    /// <summary>Gets an element in the <see cref="FlatList{T}"/> at the specified index</summary>
    /// <param name="index">The index of the element to retrieve</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>ValueSome value containing the element, or ValueNone if the index is out of range</returns>
    [<CompiledName "TryItem">]
    val tryItem<'T> : index : int -> list : FlatList<'T> -> 'T voption

    /// <summary>Returns the first element of the <see cref="FlatList{T}"/></summary>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The first element of the <see cref="FlatList{T}"/></returns>
    /// <exception cref="System.ArgumentException">Thrown when the <see cref="FlatList{T}"/> is empty</exception>
    [<CompiledName "Head">]
    val head<'T> : list : FlatList<'T> -> 'T

    /// <summary>Returns the first element of the <see cref="FlatList{T}"/>, or ValueNone if the <see cref="FlatList{T}"/> is empty</summary>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>ValueSome value containing the first element, or ValueNone if the <see cref="FlatList{T}"/> is empty</returns>
    [<CompiledName "TryHead">]
    val tryHead<'T> : list : FlatList<'T> -> 'T voption

    /// <summary>Returns the first element and the rest of the <see cref="FlatList{T}"/>, or <c>ValueNone</c> if the <see cref="FlatList{T}"/> is empty.</summary>
    /// <param name="list">The input list.</param>
    /// <returns>An option containing the first element and the rest of the <see cref="FlatList{T}"/>, or <c>ValueNone</c>.</returns>
    [<CompiledName "TryHeadAndTail">]
    val tryHeadAndTail<'T> : list : FlatList<'T> -> ('T * FlatList<'T>) voption

    /// <summary>Returns the last element of the <see cref="FlatList{T}"/></summary>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The last element of the <see cref="FlatList{T}"/></returns>
    /// <exception cref="System.ArgumentException">Thrown when the <see cref="FlatList{T}"/> is empty</exception>
    [<CompiledName "Last">]
    val last<'T> : list : FlatList<'T> -> 'T

    /// <summary>Returns the last element of the <see cref="FlatList{T}"/>, or ValueNone if the <see cref="FlatList{T}"/> is empty</summary>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>ValueSome value containing the last element, or ValueNone if the <see cref="FlatList{T}"/> is empty</returns>
    [<CompiledName "TryLast">]
    val tryLast<'T> : list : FlatList<'T> -> 'T voption

    /// <summary>Returns the last element and all but the last element of the <see cref="FlatList{T}"/>, or <c>ValueNone</c> if the <see cref="FlatList{T}"/> is empty.</summary>
    /// <param name="list">The input list.</param>
    /// <returns>An option containing the last element and all but the last element of the <see cref="FlatList{T}"/>, or <c>ValueNone</c>.</returns>
    [<CompiledName "TryLastAndInit">]
    val tryLastAndInit<'T> : list : FlatList<'T> -> (FlatList<'T> * 'T) voption

    /// <summary>Returns the <see cref="FlatList{T}"/> without its first element</summary>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>A <see cref="FlatList{T}"/> containing all elements of the input <see cref="FlatList{T}"/> except the first one</returns>
    [<CompiledName "Tail">]
    val tail<'T> : list : FlatList<'T> -> FlatList<'T>

    /// <summary>Returns the <see cref="FlatList{T}"/> without its first element, or ValueNone if the <see cref="FlatList{T}"/> is empty</summary>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>ValueSome value containing the <see cref="FlatList{T}"/> without its first element, or ValueNone if the <see cref="FlatList{T}"/> is empty</returns>
    [<CompiledName "TryTail">]
    val tryTail<'T> : list : FlatList<'T> -> FlatList<'T> voption

    /// <summary>Returns the first N elements of the <see cref="FlatList{T}"/></summary>
    /// <param name="count">The number of elements to take</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>A <see cref="FlatList{T}"/> containing the first N elements</returns>
    /// <exception cref="System.ArgumentException">Thrown when <paramref name="count"/> is negative or greater than the length of the list.</exception>
    [<CompiledName "Take">]
    val take<'T> : count : int -> list : FlatList<'T> -> FlatList<'T>

    /// <summary>Returns the last <paramref name="count"/> elements of the <see cref="FlatList{T}"/>.</summary>
    /// <param name="count">The number of elements to take from the end of the <see cref="FlatList{T}"/>.</param>
    /// <param name="list">The input list.</param>
    /// <returns>A new list containing the last <paramref name="count"/> elements.</returns>
    /// <exception cref="System.ArgumentException">Thrown when <paramref name="count"/> is negative or greater than the length of the list.</exception>
    [<CompiledName "TakeEnd">]
    val takeEnd<'T> : count : int -> list : FlatList<'T> -> FlatList<'T>

    /// <summary>Returns a <see cref="FlatList{T}"/> containing the first elements of the input <see cref="FlatList{T}"/> for which the given predicate returns true</summary>
    /// <param name="predicate">A function to test each element</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>A <see cref="FlatList{T}"/> containing the first elements for which the predicate returns true</returns>
    [<CompiledName "TakeWhile">]
    val takeWhile<'T> : predicate : ('T -> bool) -> list : FlatList<'T> -> FlatList<'T>

    /// <summary>Returns the <see cref="FlatList{T}"/> without its first N elements</summary>
    /// <param name="index">The number of elements to skip</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>A <see cref="FlatList{T}"/> containing all except the first N elements</returns>
    /// <exception cref="System.ArgumentException">Thrown when <paramref name="index"/> is negative or greater than the length of the list.</exception>
    [<CompiledName "Skip">]
    val skip<'T> : index : int -> list : FlatList<'T> -> FlatList<'T>

    /// <summary>Returns a new <see cref="FlatList{T}"/> that does not contain the last <paramref name="count"/> elements of the original <see cref="FlatList{T}"/>.</summary>
    /// <param name="count">The number of elements to skip from the end of the <see cref="FlatList{T}"/>.</param>
    /// <param name="list">The input list.</param>
    /// <returns>A new list without the last <paramref name="count"/> elements.</returns>
    /// <exception cref="System.ArgumentException">Thrown when <paramref name="count"/> is negative or greater than the length of the list.</exception>
    [<CompiledName "SkipEnd">]
    val skipEnd<'T> : count : int -> list : FlatList<'T> -> FlatList<'T>

    /// <summary>Returns a <see cref="FlatList{T}"/> that skips the elements of the input <see cref="FlatList{T}"/> while the given predicate returns true, then returns the rest</summary>
    /// <param name="predicate">A function to test each element</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>A <see cref="FlatList{T}"/> that skips the elements while the predicate returns true, then contains the rest</returns>
    [<CompiledName "SkipWhile">]
    val skipWhile<'T> : predicate : ('T -> bool) -> list : FlatList<'T> -> FlatList<'T>

    /// <summary>Gets a sublist of the input <see cref="FlatList{T}"/></summary>
    /// <param name="start">The index of the first element to include</param>
    /// <param name="count">The number of elements in the sublist</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>A <see cref="FlatList{T}"/> containing the elements from start index for the given count</returns>
    /// <exception cref="System.ArgumentException">Thrown when <paramref name="start"/> is negative, <paramref name="count"/> is negative, or the sum of <paramref name="start"/> and <paramref name="count"/> exceeds the length of the list.</exception>
    [<CompiledName "Sub">]
    val sub<'T> : start : int -> count : int -> list : FlatList<'T> -> FlatList<'T>

    /// <summary>Returns a <see cref="FlatList{T}"/> that contains no more than N elements of the input <see cref="FlatList{T}"/></summary>
    /// <param name="count">The maximum number of elements to include</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>A <see cref="FlatList{T}"/> containing at most N elements</returns>
    /// <exception cref="System.ArgumentException">Thrown when <paramref name="count"/> is negative.</exception>
    [<CompiledName "Truncate">]
    val truncate<'T> : count : int -> list : FlatList<'T> -> FlatList<'T>

    /// <summary>Splits the <see cref="FlatList{T}"/> into two <see cref="FlatList{T}"/>s at the specified index</summary>
    /// <param name="index">The index at which to split the <see cref="FlatList{T}"/></param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>A tuple of two <see cref="FlatList{T}"/>s, the first containing the elements up to the index, the second containing the rest</returns>
    [<CompiledName "SplitAt">]
    val splitAt<'T> : index : int -> list : FlatList<'T> -> FlatList<'T> * FlatList<'T>

    /// <summary>Splits the <see cref="FlatList{T}"/> into chunks of size at most 'chunkSize'</summary>
    /// <param name="chunkSize">The maximum size of each chunk</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The <see cref="FlatList{T}"/> split into chunks</returns>
    /// <exception cref="System.ArgumentException">Thrown when chunkSize is not positive or when <see cref="FlatList{T}"/> is default</exception>
    [<CompiledName "ChunkBySize">]
    val chunkBySize<'T> : chunkSize : int -> list : FlatList<'T> -> FlatList<FlatList<'T>>

    /// <summary>Applies a function to the builder and returns the resulting <see cref="FlatList{T}"/></summary>
    /// <param name="f">The function to apply to the builder</param>
    /// <returns>The <see cref="FlatList{T}"/> created from the builder after applying the function</returns>
    [<CompiledName "Build">]
    val inline build<'T> : f : (FlatList<'T>.Builder -> unit) -> FlatList<'T>

    /// <summary>Updates the <see cref="FlatList{T}"/> by applying a function to a builder initialized with the <see cref="FlatList{T}"/>'s elements</summary>
    /// <param name="f">The function to apply to the builder</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The updated <see cref="FlatList{T}"/></returns>
    [<CompiledName "Update">]
    val inline update<'T> : f : (FlatList<'T>.Builder -> unit) -> list : FlatList<'T> -> FlatList<'T>

    /// <summary>Returns the index of the first element in the <see cref="FlatList{T}"/> that satisfies the given predicate</summary>
    /// <param name="predicate">The function to test the input elements</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The index of the first element that satisfies the predicate</returns>
    /// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown if no element satisfies the predicate</exception>
    [<CompiledName "FindIndex">]
    val findIndex<'T> : predicate : ('T -> bool) -> list : FlatList<'T> -> int

    /// <summary>Returns the index of the first element in the <see cref="FlatList{T}"/> that satisfies the given predicate, or ValueNone if no such element exists</summary>
    /// <param name="predicate">The function to test the input elements</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The index of the first element that satisfies the predicate, or ValueNone</returns>
    [<CompiledName "TryFindIndex">]
    val tryFindIndex<'T> : predicate : ('T -> bool) -> list : FlatList<'T> -> int voption

    /// <summary>Returns a new <see cref="FlatList{T}"/> containing elements corresponding to a sliding window of elements from the input <see cref="FlatList{T}"/></summary>
    /// <param name="windowSize">The size of the window</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The resulting <see cref="FlatList{T}"/> of sliding windows</returns>
    /// <exception cref="System.ArgumentException">Thrown when windowSize is not positive or when <see cref="FlatList{T}"/> is default</exception>
    /// <example>
    /// <code>
    /// let numbers = FlatList.ofArray [|1; 2; 3; 4; 5|]
    /// let windows = FlatList.windowed 3 numbers
    /// // windows is [|[|1; 2; 3|]; [|2; 3; 4|]; [|3; 4; 5|]|]
    ///
    /// // Calculate moving averages
    /// let movingAverages =
    ///     windows
    ///     |> FlatList.map (fun window ->
    ///         FlatList.average window)
    /// // movingAverages is [|2.0; 3.0; 4.0|]
    /// </code>
    /// </example>
    [<CompiledName "Windowed">]
    val windowed<'T> : windowSize : int -> list : FlatList<'T> -> FlatList<FlatList<'T>>

    /// <summary>Returns a new <see cref="FlatList{T}"/> containing pairs of adjacent elements from the input <see cref="FlatList{T}"/></summary>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The resulting <see cref="FlatList{T}"/> of pairs</returns>
    [<CompiledName "Pairwise">]
    val pairwise<'T> : list : FlatList<'T> -> FlatList<struct ('T * 'T)>

    /// <summary>Splits the input <see cref="FlatList{T}"/> into at most count chunks.</summary>
    /// <param name="count">The maximum number of chunks.</param>
    /// <param name="list">The input <see cref="FlatList{T}"/>.</param>
    /// <returns>The <see cref="FlatList{T}"/> split into chunks.</returns>
    /// <exception cref="System.ArgumentException">Thrown when count is not positive.</exception>
    [<CompiledName "SplitInto">]
    val splitInto<'T> : count : int -> list : FlatList<'T> -> FlatList<FlatList<'T>>

    /// <summary>Returns a new <see cref="FlatList{T}"/> that contains the elements of the original <see cref="FlatList{T}"/> but with duplicates removed</summary>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The <see cref="FlatList{T}"/> with distinct elements</returns>
    [<CompiledName "Distinct">]
    val distinct<'T> : list : FlatList<'T> -> FlatList<'T>

    /// <summary>Returns a new <see cref="FlatList{T}"/> that contains all pairwise combinations of elements from the first and second <see cref="FlatList{T}"/>s</summary>
    /// <param name="xs">The first input <see cref="FlatList{T}"/></param>
    /// <param name="ys">The second input <see cref="FlatList{T}"/></param>
    /// <returns>The <see cref="FlatList{T}"/> of all pairwise combinations</returns>
    [<CompiledName "AllPairs">]
    val allPairs<'T, 'U> : xs : FlatList<'T> -> ys : FlatList<'U> -> FlatList<('T * 'U)>

    /// <summary>Returns a new <see cref="FlatList{T}"/> with the elements permuted according to the specified permutation</summary>
    /// <param name="indexMap">The function that maps input indices to output indices</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The permuted <see cref="FlatList{T}"/></returns>
    /// <exception cref="System.ArgumentException">Thrown when the permutation function returns an out-of-range index</exception>
    [<CompiledName "Permute">]
    val permute<'T> : indexMap : (int -> int) -> list : FlatList<'T> -> FlatList<'T>

    /// <summary>Combines the two <see cref="FlatList{T}"/>s into a <see cref="FlatList{T}"/> of pairs. The two <see cref="FlatList{T}"/>s must have equal lengths</summary>
    /// <param name="list1">The first input <see cref="FlatList{T}"/></param>
    /// <param name="list2">The second input <see cref="FlatList{T}"/></param>
    /// <returns>The <see cref="FlatList{T}"/> of pairs</returns>
    /// <exception cref="System.ArgumentException">Thrown when the <see cref="FlatList{T}"/>s have different lengths</exception>
    [<CompiledName "Zip">]
    val zip<'T, 'U> : list1 : FlatList<'T> -> list2 : FlatList<'U> -> FlatList<struct ('T * 'U)>

    /// <summary>Combines the three <see cref="FlatList{T}"/>s into a <see cref="FlatList{T}"/> of triples. The three <see cref="FlatList{T}"/>s must have equal lengths</summary>
    /// <param name="list1">The first input <see cref="FlatList{T}"/></param>
    /// <param name="list2">The second input <see cref="FlatList{T}"/></param>
    /// <param name="list3">The third input <see cref="FlatList{T}"/></param>
    /// <returns>The <see cref="FlatList{T}"/> of triples</returns>
    /// <exception cref="System.ArgumentException">Thrown when the <see cref="FlatList{T}"/>s have different lengths</exception>
    [<CompiledName "Zip3">]
    val zip3<'T, 'U, 'V> : list1 : FlatList<'T> -> list2 : FlatList<'U> -> list3 : FlatList<'V> -> FlatList<struct ('T * 'U * 'V)>

    /// <summary>Splits a <see cref="FlatList{T}"/> of pairs into two <see cref="FlatList{T}"/>s</summary>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The two <see cref="FlatList{T}"/>s unzipped from the input <see cref="FlatList{T}"/></returns>
    [<CompiledName "Unzip">]
    val unzip<'T, 'U> : list : FlatList<struct ('T * 'U)> -> struct (FlatList<'T> * FlatList<'U>)

    /// <summary>Splits a <see cref="FlatList{T}"/> of triples into three <see cref="FlatList{T}"/>s</summary>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The three <see cref="FlatList{T}"/>s unzipped from the input <see cref="FlatList{T}"/></returns>
    [<CompiledName "Unzip3">]
    val unzip3<'T, 'U, 'V> : list : FlatList<struct ('T * 'U * 'V)> -> struct (FlatList<'T> * FlatList<'U> * FlatList<'V>)

    /// <summary>Returns the average of the elements in the <see cref="FlatList{T}"/></summary>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The average of the elements</returns>
    /// <exception cref="System.ArgumentException">Thrown when the <see cref="FlatList{T}"/> is empty</exception>
    [<CompiledName "Average">]
    val inline average<'T
        when 'T : (static member (+) : 'T * 'T -> 'T)
        and 'T : (static member DivideByInt : 'T * int -> 'T)
        and 'T : (static member Zero : 'T)> : list : FlatList<'T> -> 'T

    /// <summary>Returns the average of the results of applying the function to each element of the <see cref="FlatList{T}"/></summary>
    /// <param name="projection">The function to transform the <see cref="FlatList{T}"/> elements before averaging</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The average of the projected elements</returns>
    /// <exception cref="System.ArgumentException">Thrown when the <see cref="FlatList{T}"/> is empty</exception>
    [<CompiledName "AverageBy">]
    val inline averageBy<'T, 'U
        when 'U : (static member (+) : 'U * 'U -> 'U)
        and 'U : (static member DivideByInt : 'U * int -> 'U)
        and 'U : (static member Zero : 'U)> : projection : ('T -> 'U) -> list : FlatList<'T> -> 'U

    /// <summary>Applies a function to each element of the <see cref="FlatList{T}"/>, threading an accumulator argument through the computation</summary>
    /// <param name="folder">The function to update the state given the input elements</param>
    /// <param name="state">The initial state</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The final state</returns>
    /// <example>
    /// <code>
    /// let numbers = FlatList.ofArray [|1; 2; 3; 4; 5|]
    /// let sum = FlatList.fold (fun acc x -> acc + x) 0 numbers
    /// // sum is 15
    ///
    /// // Computing the average
    /// let count = FlatList.length numbers
    /// let total = FlatList.fold (fun sum x -> sum + x) 0 numbers
    /// let avg = float total / float count
    /// </code>
    /// </example>
    [<CompiledName "Fold">]
    val fold<'T, 'State> : folder : ('State -> 'T -> 'State) -> state : 'State -> list : FlatList<'T> -> 'State

    /// <summary>Applies a function to each element of the collection, threading an accumulator argument
    /// through the computation. The integer index passed to the function indicates the index of the
    /// element. The seed is used as the initial accumulator value.</summary>
    /// <param name="folder">The function to update the state given the index, the input elements, and the previous state.</param>
    /// <param name="state">The initial state.</param>
    /// <param name="list">The input list.</param>
    /// <returns>The final state.</returns>
    [<CompiledName "Foldi">]
    val foldi<'T, 'State> : folder : (int -> 'State -> 'T -> 'State) -> state : 'State -> list : FlatList<'T> -> 'State

    /// <summary>Applies a function to corresponding elements of two <see cref="FlatList{T}"/>s, threading an accumulator argument through the computation</summary>
    /// <param name="folder">The function to update the state given the input elements from both <see cref="FlatList{T}"/>s</param>
    /// <param name="state">The initial state</param>
    /// <param name="list1">The first input <see cref="FlatList{T}"/></param>
    /// <param name="list2">The second input <see cref="FlatList{T}"/></param>
    /// <returns>The final state</returns>
    /// <exception cref="System.ArgumentException">Thrown when the <see cref="FlatList{T}"/>s have different lengths</exception>
    [<CompiledName "Fold2">]
    val fold2<'T1, 'T2, 'State> :
        folder : ('State -> 'T1 -> 'T2 -> 'State) -> state : 'State -> list1 : FlatList<'T1> -> list2 : FlatList<'T2> -> 'State

    /// <summary>Applies a function to corresponding elements of two <see cref="FlatList{T}"/>s, threading an accumulator argument
    /// through the computation. The integer index passed to the function indicates the index of the
    /// elements. The seed is used as the initial accumulator value.</summary>
    /// <param name="folder">The function to update the state given the index, the input elements from both <see cref="FlatList{T}"/>s, and the previous state.</param>
    /// <param name="state">The initial state.</param>
    /// <param name="list1">The first input list.</param>
    /// <param name="list2">The second input list.</param>
    /// <returns>The final state.</returns>
    /// <exception cref="System.ArgumentException">Thrown when the lists have different lengths.</exception>
    [<CompiledName "Foldi2">]
    val foldi2<'T1, 'T2, 'State> :
        folder : (int -> 'State -> 'T1 -> 'T2 -> 'State) ->
        state : 'State ->
        list1 : FlatList<'T1> ->
        list2 : FlatList<'T2> ->
            'State

    /// <summary>Applies a function to each element of the <see cref="FlatList{T}"/>, threading an accumulator argument through the computation, starting from the end.</summary>
    /// <param name="folder">The function to update the state given the input elements, starting from the end.</param>
    /// <param name="list">The input <see cref="FlatList{T}"/>.</param>
    /// <param name="state">The initial state.</param>
    /// <returns>The final state.</returns>
    /// <exception cref="System.ArgumentException">Thrown when the <see cref="FlatList{T}"/> is default or empty</exception>
    [<CompiledName "FoldBack">]
    val foldBack<'T, 'State> : folder : ('T -> 'State -> 'State) -> list : FlatList<'T> -> state : 'State -> 'State

    /// <summary>Applies a function to each element of the collection, starting from the end, threading an
    /// accumulator argument through the computation. The integer index passed to the function indicates
    /// the index of the element. The seed is used as the initial accumulator value.</summary>
    /// <param name="folder">The function to update the state given the index, the input elements, and the previous state.</param>
    /// <param name="list">The input list.</param>
    /// <param name="state">The initial state.</param>
    /// <returns>The final state.</returns>
    [<CompiledName "FoldBacki">]
    val foldBacki<'T, 'State> : folder : (int -> 'T -> 'State -> 'State) -> list : FlatList<'T> -> state : 'State -> 'State

    /// <summary>Applies a function to corresponding elements of two <see cref="FlatList{T}"/>s, threading an accumulator argument through the computation, starting from the end.</summary>
    /// <param name="folder">The function to update the state given the input elements from both <see cref="FlatList{T}"/>s, starting from the end.</param>
    /// <param name="list1">The first input <see cref="FlatList{T}"/></param>
    /// <param name="list2">The second input <see cref="FlatList{T}"/></param>
    /// <param name="state">The initial state.</param>
    /// <returns>The final state.</returns>
    /// <exception cref="System.ArgumentException">Thrown when the <see cref="FlatList{T}"/>s have different lengths</exception>
    [<CompiledName "FoldBack2">]
    val foldBack2<'T1, 'T2, 'State> :
        folder : ('T1 -> 'T2 -> 'State -> 'State) -> list1 : FlatList<'T1> -> list2 : FlatList<'T2> -> state : 'State -> 'State

    /// <summary>Applies a function to corresponding elements of two <see cref="FlatList{T}"/>s, starting from the end, threading
    /// an accumulator argument through the computation. The integer index passed to the function indicates
    /// the index of the elements. The seed is used as the initial accumulator value.</summary>
    /// <param name="folder">The function to update the state given the index, the input elements from both <see cref="FlatList{T}"/>s, and the previous state.</param>
    /// <param name="list1">The first input list.</param>
    /// <param name="list2">The second input list.</param>
    /// <param name="state">The initial state.</param>
    /// <returns>The final state.</returns>
    /// <exception cref="System.ArgumentException">Thrown when the lists have different lengths.</exception>
    [<CompiledName "FoldBacki2">]
    val foldBacki2<'T1, 'T2, 'State> :
        folder : (int -> 'T1 -> 'T2 -> 'State -> 'State) ->
        list1 : FlatList<'T1> ->
        list2 : FlatList<'T2> ->
        state : 'State ->
            'State

    /// <summary>Applies a function to corresponding elements of three <see cref="FlatList{T}"/>s, threading an accumulator argument through the computation, starting from the end.</summary>
    /// <param name="folder">The function to update the state given the input elements from all three <see cref="FlatList{T}"/>s, starting from the end.</param>
    /// <param name="list1">The first input <see cref="FlatList{T}"/></param>
    /// <param name="list2">The second input <see cref="FlatList{T}"/></param>
    /// <param name="list3">The third input <see cref="FlatList{T}"/></param>
    /// <param name="state">The initial state.</param>
    /// <returns>The final state.</returns>
    /// <exception cref="System.ArgumentException">Thrown when the <see cref="FlatList{T}"/>s have different lengths</exception>
    [<CompiledName "FoldBack3">]
    val foldBack3<'T1, 'T2, 'T3, 'State> :
        folder : ('T1 -> 'T2 -> 'T3 -> 'State -> 'State) ->
        list1 : FlatList<'T1> ->
        list2 : FlatList<'T2> ->
        list3 : FlatList<'T3> ->
        state : 'State ->
            'State

    /// <summary>Applies a function to each element of the <see cref="FlatList{T}"/>, threading an accumulator argument through the computation.
    /// This function takes the second argument, and applies the function to it and the first element of the <see cref="FlatList{T}"/>.
    /// Then, it passes this result into the function along with the second element, and so on.
    /// Finally, it returns the final result. If the <see cref="FlatList{T}"/> is empty, an exception is raised.</summary>
    /// <param name="reduction">The function to reduce the <see cref="FlatList{T}"/> with</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The final accumulated value</returns>
    /// <exception cref="System.ArgumentException">Thrown when the <see cref="FlatList{T}"/> is empty</exception>
    [<CompiledName "Reduce">]
    val reduce<'T> : reduction : ('T -> 'T -> 'T) -> list : FlatList<'T> -> 'T

    /// <summary>Applies a function to each element of the <see cref="FlatList{T}"/>, threading an accumulator argument through the computation, starting from the end.
    /// This function takes the last element of the <see cref="FlatList{T}"/> and the second-to-last element, and applies the function to them.
    /// Then, it passes this result into the function along with the third-to-last element, and so on.
    /// Finally, it returns the final result. If the <see cref="FlatList{T}"/> is empty, an exception is raised.</summary>
    /// <param name="reduction">The function to reduce the <see cref="FlatList{T}"/> with, starting from the end</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The final accumulated value</returns>
    /// <exception cref="System.ArgumentException">Thrown when the <see cref="FlatList{T}"/> is empty</exception>
    [<CompiledName "ReduceBack">]
    val reduceBack<'T> : reduction : ('T -> 'T -> 'T) -> list : FlatList<'T> -> 'T

    /// <summary>Like fold, but returns both the intermediate and final results</summary>
    /// <param name="folder">The function to update the state given the input elements</param>
    /// <param name="state">The initial state</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The <see cref="FlatList{T}"/> of all intermediate and final states</returns>
    [<CompiledName "Scan">]
    val scan<'T, 'State> : folder : ('State -> 'T -> 'State) -> state : 'State -> list : FlatList<'T> -> FlatList<'State>

    /// <summary>Like foldBack, but returns both the intermediate and final results</summary>
    /// <param name="folder">The function to update the state given the input elements, starting from the end</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <param name="state">The initial state</param>
    /// <returns>The <see cref="FlatList{T}"/> of all intermediate and final states, in reverse order of computation</returns>
    [<CompiledName "ScanBack">]
    val scanBack<'T, 'State> : folder : ('T -> 'State -> 'State) -> list : FlatList<'T> -> state : 'State -> FlatList<'State>

    /// <summary>Returns the only element of the <see cref="FlatList{T}"/>.</summary>
    /// <param name="list">The input <see cref="FlatList{T}"/>.</param>
    /// <returns>The only element of the <see cref="FlatList{T}"/>.</returns>
    /// <exception cref="System.ArgumentException">Thrown when the input does not have precisely one element.</exception>
    [<CompiledName "ExactlyOne">]
    val exactlyOne<'T> : list : FlatList<'T> -> 'T

    /// <summary>Returns the only element of the <see cref="FlatList{T}"/> or None if the <see cref="FlatList{T}"/> is empty or contains more than one element.</summary>
    /// <param name="list">The input <see cref="FlatList{T}"/>.</param>
    /// <returns>The only element of the <see cref="FlatList{T}"/> or None.</returns>
    [<CompiledName "TryExactlyOne">]
    val tryExactlyOne<'T> : list : FlatList<'T> -> 'T voption

    /// <summary>Returns a new list with the distinct elements of the input <see cref="FlatList{T}"/> which do not appear in the itemsToExclude sequence</summary>
    /// <param name="itemsToExclude">A sequence whose elements that also occur in the input <see cref="FlatList{T}"/> will cause those elements to be removed</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>A new <see cref="FlatList{T}"/> that contains the distinct elements of list that do not appear in itemsToExclude</returns>
    [<CompiledName "Except">]
    val except<'T> : itemsToExclude : 'T seq -> list : FlatList<'T> -> FlatList<'T>

    /// <summary>Returns the sum of the elements in the <see cref="FlatList{T}"/>.</summary>
    /// <param name="list">The input <see cref="FlatList{T}"/>.</param>
    /// <returns>The resulting sum.</returns>
    /// <exception cref="System.ArgumentNullException">Thrown when the input <see cref="FlatList{T}"/> is default or empty.</exception>
    [<CompiledName "Sum">]
    val inline sum<'T when 'T : (static member (+) : 'T * 'T -> 'T) and 'T : (static member Zero : 'T)> :
        list : FlatList<'T> -> 'T

    /// <summary>Returns the sum of the results generated by applying the function to each element of the <see cref="FlatList{T}"/>.</summary>
    /// <param name="projection">The function to transform the <see cref="FlatList{T}"/> elements into the type to be summed.</param>
    /// <param name="list">The input <see cref="FlatList{T}"/>.</param>
    /// <returns>The resulting sum.</returns>
    /// <exception cref="System.ArgumentNullException">Thrown when the input <see cref="FlatList{T}"/> is default or empty.</exception>
    [<CompiledName "SumBy">]
    val inline sumBy<'T, 'U when 'U : (static member (+) : 'U * 'U -> 'U) and 'U : (static member Zero : 'U)> :
        projection : ('T -> 'U) -> list : FlatList<'T> -> 'U

    /// <summary>Returns the transpose of the given sequence of <see cref="FlatList{T}"/>s.</summary>
    /// <param name="lists">The input <see cref="FlatList{T}"/> of <see cref="FlatList{T}"/>s.</param>
    [<CompiledName "Transpose">]
    val transpose<'T> : lists : FlatList<FlatList<'T>> -> FlatList<FlatList<'T>>

    /// <summary>Updates the element at the specified index in the array.</summary>
    /// <param name="index">The index of the element to update.</param>
    /// <param name="value">The new value for the element.</param>
    /// <param name="list">The input array.</param>
    /// <returns>The updated array.</returns>
    [<CompiledName "UpdateAt">]
    val updateAt<'T> : index : int -> value : 'T -> list : FlatList<'T> -> FlatList<'T>

    /// <summary>Removes the element at the specified index from the array.</summary>
    /// <param name="index">The index of the element to remove.</param>
    /// <param name="list">The input array.</param>
    /// <returns>The array with the element removed.</returns>
    [<CompiledName "RemoveAt">]
    val removeAt<'T> : index : int -> list : FlatList<'T> -> FlatList<'T>

    /// <summary>Inserts an element at the specified index in the array.</summary>
    /// <param name="index">The index at which to insert the element.</param>
    /// <param name="value">The element to insert.</param>
    /// <param name="list">The input array.</param>
    /// <returns>The array with the element inserted.</returns>
    [<CompiledName "InsertAt">]
    val insertAt<'T> : index : int -> value : 'T -> list : FlatList<'T> -> FlatList<'T>

    /// <summary>Inserts multiple elements at the specified index in the array.</summary>
    /// <param name="index">The index at which to insert the elements.</param>
    /// <param name="values">The elements to insert.</param>
    /// <param name="list">The input array.</param>
    /// <returns>The array with the elements inserted.</returns>
    [<CompiledName "InsertManyAt">]
    val insertManyAt<'T> : index : int -> values : 'T seq -> list : FlatList<'T> -> FlatList<'T>

    /// <summary>Generates a <see cref="FlatList{T}"/> by repeatedly applying a function to a state.</summary>
    /// <param name="generator">The function to generate the next element and state.</param>
    /// <param name="state">The initial state.</param>
    /// <returns>The generated sequence.</returns>
    [<CompiledName "Unfold">]
    val unfold<'T, 'State> : generator : ('State -> struct ('T * 'State) voption) -> state : 'State -> FlatList<'T>

    /// <summary>Compares two arrays using a custom comparison function.</summary>
    /// <param name="comparer">The function to compare elements.</param>
    /// <param name="list1">The first input array.</param>
    /// <param name="list2">The second input array.</param>
    /// <returns>The comparison result.</returns>
    [<CompiledName "CompareWith">]
    val compareWith<'T> : comparer : ('T -> 'T -> int) -> list1 : FlatList<'T> -> list2 : FlatList<'T> -> int

    /// <summary>Returns the maximum element in the array.</summary>
    /// <param name="list">The input array.</param>
    /// <returns>The maximum element.</returns>
    /// <exception cref="System.ArgumentException">Thrown when the input array is empty.</exception>
    [<CompiledName "Max">]
    val inline max<'T when 'T : comparison> : list : FlatList<'T> -> 'T

    /// <summary>Returns the maximum element in the array according to a projection function.</summary>
    /// <param name="projection">The function to transform the elements into a type supporting comparison.</param>
    /// <param name="list">The input array.</param>
    /// <returns>The maximum element.</returns>
    /// <exception cref="System.ArgumentException">Thrown when the input array is empty.</exception>
    [<CompiledName "MaxBy">]
    val inline maxBy<'T, 'Key when 'Key : comparison> : projection : ('T -> 'Key) -> list : FlatList<'T> -> 'T

    /// <summary>Returns the minimum element in the <see cref="FlatList{T}"/>.</summary>
    /// <param name="list">The input <see cref="FlatList{T}"/>.</param>
    /// <returns>The minimum element.</returns>
    /// <exception cref="System.ArgumentException">Thrown when the input <see cref="FlatList{T}"/> is default or empty.</exception>
    [<CompiledName "Min">]
    val inline min<'T when 'T : comparison> : list : FlatList<'T> -> 'T

    /// <summary>Returns the minimum element in the <see cref="FlatList{T}"/> according to a projection function.</summary>
    /// <param name="projection">The function to transform the elements into a type supporting comparison.</param>
    /// <param name="list">The input <see cref="FlatList{T}"/>.</param>
    /// <returns>The minimum element.</returns>
    /// <exception cref="System.ArgumentException">Thrown when the input <see cref="FlatList{T}"/> is default or empty.</exception>
    [<CompiledName "MinBy">]
    val inline minBy<'T, 'Key when 'Key : comparison> : projection : ('T -> 'Key) -> list : FlatList<'T> -> 'T
