#if INTERACTIVE
namespace global
#else
namespace FSharp.Collections.Immutable
#endif

open System
open System.Collections.Generic
open System.Collections.Immutable
open System.Linq

// The FlatList name comes from a similar construct seen in the official F# source code
type FlatList<'T> = System.Collections.Immutable.ImmutableArray<'T>

// based on the F# Array module source
[<RequireQualifiedAccess;
  CompiledName((nameof System.Collections.Immutable.ImmutableArray)
               + "Module")>]
module FlatList =

    type internal FlatListFactory = System.Collections.Immutable.ImmutableArray

    let inline internal checkNotDefault argName (list : FlatList<'T>) =
        if list.IsDefault then
            invalidArg argName "Uninstantiated ImmutableArray/FlatList"

    let inline internal indexNotFound () = raise <| System.Collections.Generic.KeyNotFoundException ()

    let inline private lengthWhile (predicate : 'T -> bool) (list : FlatList<'T>) = list.TakeWhile(predicate).Count ()

    ////////// Creating //////////

    /// <summary>Creates a new builder with the specified capacity</summary>
    /// <param name="capacity">The initial capacity of the builder</param>
    /// <returns>An empty builder with the specified capacity</returns>
    let inline builderWith capacity : FlatList<'T>.Builder = FlatListFactory.CreateBuilder (capacity)

    /// <summary>Builds a <see cref="FlatList{T}"/> from a builder, moving the elements and leaving the builder empty</summary>
    /// <param name="builder">The builder to build from</param>
    /// <returns>A <see cref="FlatList{T}"/> containing the elements from the builder</returns>
    /// <exception cref="System.ArgumentNullException">Thrown when builder is null</exception>
    let moveFromBuilder (builder : FlatList<_>.Builder) : FlatList<_> =
        checkNotNull (nameof builder) builder // Keep check for null builder, not default FlatList
        builder.MoveToImmutable ()

    /// <summary>Returns an empty <see cref="FlatList{T}"/></summary>
    /// <returns>An empty <see cref="FlatList{T}"/></returns>
    /// <example>
    /// <code>
    /// let emptyList = FlatList.empty&lt;int&gt;
    /// printfn "Is empty? %b" (FlatList.isEmpty emptyList) // true
    /// </code>
    /// </example>
    let inline empty<'T> : FlatList<'T> = FlatListFactory.Create<'T> ()

    /// <summary>Builds a <see cref="FlatList{T}"/> from the given array</summary>
    /// <param name="source">The array to build the <see cref="FlatList{T}"/> from</param>
    /// <returns>A <see cref="FlatList{T}"/> containing the elements of the array</returns>
    let inline ofArray (source : _ array) = FlatListFactory.CreateRange source

    /// <summary>Builds a <see cref="FlatList{T}"/> from the given sequence</summary>
    /// <param name="source">The sequence to build the <see cref="FlatList{T}"/> from</param>
    /// <returns>A <see cref="FlatList{T}"/> containing the elements of the sequence</returns>
    let inline ofSeq source = FlatListFactory.CreateRange source

    /// <summary>Returns a <see cref="FlatList{T}"/> with a single element</summary>
    /// <param name="item">The item to put into the <see cref="FlatList{T}"/></param>
    /// <returns>A <see cref="FlatList{T}"/> containing only the given item</returns>
    let inline singleton<'T> (item : 'T) : FlatList<'T> = FlatListFactory.Create<'T> (item)

    /// <summary>Creates a <see cref="FlatList{T}"/> by initializing each element with the given function</summary>
    /// <param name="count">The number of elements to create</param>
    /// <param name="initializer">The function to initialize each element</param>
    /// <returns>A new <see cref="FlatList{T}"/> with the initialized elements</returns>
    /// <exception cref="System.ArgumentException">Thrown when count is negative</exception>
    let init count (initializer : int -> 'T) =
        if count < 0 then
            invalidArg (nameof count) ErrorStrings.InputMustBeNonNegative
        ParallelEnumerable.Range(0, count).Select(initializer).ToImmutableArray ()

    /// <summary>Creates a <see cref="FlatList{T}"/> of a given length with all elements set to the given value</summary>
    /// <param name="count">The length of the <see cref="FlatList{T}"/> to create</param>
    /// <param name="value">The value to replicate</param>
    /// <returns>A <see cref="FlatList{T}"/> of the specified length with all elements equal to the given value</returns>
    let create count (value : 'T) = init count (fun _ -> value)

    /// <summary>Replicates a value into a <see cref="FlatList{T}"/> of a given length</summary>
    /// <param name="count">The length of the <see cref="FlatList{T}"/> to create</param>
    /// <param name="initial">The value to replicate</param>
    /// <returns>A <see cref="FlatList{T}"/> of the specified length with all elements equal to the given value</returns>
    let replicate count initial = create count initial

    /// <summary>Views the <see cref="FlatList{T}"/> as a sequence</summary>
    /// <param name="flatList">The input <see cref="FlatList{T}"/></param>
    /// <returns>The sequence containing the elements of the <see cref="FlatList{T}"/></returns>
    let inline toSeq (flatList : FlatList<_>) = flatList :> seq<_>

    /// <summary>Builds an array from the given <see cref="FlatList{T}"/></summary>
    /// <param name="list">The <see cref="FlatList{T}"/> to build the array from</param>
    /// <returns>An array containing the elements of the <see cref="FlatList{T}"/></returns>
    let inline toArray (list : FlatList<_>) = list.ToArray ()

    ////////// Building //////////

    /// <summary>Builds a <see cref="FlatList{T}"/> from a builder, copying the elements</summary>
    /// <param name="builder">The builder to build from</param>
    /// <returns>A <see cref="FlatList{T}"/> containing the elements from the builder</returns>
    /// <exception cref="System.ArgumentNullException">Thrown when builder is null</exception>
    let ofBuilder (builder : FlatList<_>.Builder) : FlatList<_> =
        checkNotNull (nameof builder) builder // Keep check for null builder
        builder.MoveToImmutable ()

    /// <summary>Creates a new builder</summary>
    /// <returns>An empty builder</returns>
    let inline builder () : FlatList<'T>.Builder = FlatListFactory.CreateBuilder ()

    /// <summary>Creates a builder containing the elements of the input <see cref="FlatList{T}"/></summary>
    /// <param name="list">The <see cref="FlatList{T}"/> to create the builder from</param>
    /// <returns>A builder containing the elements of the <see cref="FlatList{T}"/></returns>
    let toBuilder (list : FlatList<'T>) : FlatList<'T>.Builder = list.ToBuilder ()

    module Builder =

        let inline private check (builder : FlatList<'T>.Builder) = checkNotNull (nameof builder) builder

        /// <summary>Adds an item to the builder</summary>
        /// <param name="item">The item to add</param>
        /// <param name="builder">The builder to add to</param>
        let add item builder =
            check builder
            builder.Add (item)

    /// <summary>Checks if the <see cref="FlatList{T}"/> is empty</summary>
    /// <param name="list">The <see cref="FlatList{T}"/> to check</param>
    /// <returns>True if the <see cref="FlatList{T}"/> is empty, false otherwise</returns>
    let isEmpty (list : FlatList<_>) = list.IsEmpty

    /// <summary>Checks if the <see cref="FlatList{T}"/> is uninstantiated</summary>
    /// <param name="list">The <see cref="FlatList{T}"/> to check</param>
    /// <returns>True if the <see cref="FlatList{T}"/> is uninstantiated, false otherwise</returns>
    let isDefault (list : FlatList<_>) = list.IsDefault

    /// <summary>Checks if the <see cref="FlatList{T}"/> is uninstantiated or empty</summary>
    /// <param name="list">The <see cref="FlatList{T}"/> to check</param>
    /// <returns>True if the <see cref="FlatList{T}"/> is uninstantiated or empty, false otherwise</returns>
    let isDefaultOrEmpty (list : FlatList<_>) = list.IsDefaultOrEmpty

    ////////// IReadOnly* //////////

    /// <summary>Returns the number of elements in the <see cref="FlatList{T}"/></summary>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The number of elements in the <see cref="FlatList{T}"/></returns>
    let length (list : FlatList<'T>) = list.Length

    /// <summary>Gets the element at the specified index in the <see cref="FlatList{T}"/></summary>
    /// <param name="index">The index to retrieve</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The element at the specified index</returns>
    /// <exception cref="System.IndexOutOfRangeException">Thrown when the index is out of range</exception>
    let item index (list : FlatList<'T>) = list.[index]

    /// <summary>Appends two <see cref="FlatList{T}"/>s to create a new <see cref="FlatList{T}"/> containing all elements from both <see cref="FlatList{T}"/>s</summary>
    /// <param name="list1">The first <see cref="FlatList{T}"/></param>
    /// <param name="list2">The second <see cref="FlatList{T}"/></param>
    /// <returns>A new <see cref="FlatList{T}"/> containing all elements from both input <see cref="FlatList{T}"/>s</returns>
    let append (list1 : FlatList<'T>) (list2 : FlatList<'T>) : FlatList<'T> =
        list1.AddRange (list2 :> System.Collections.Generic.IEnumerable<'T>)

    /// <summary>Searches for the specified object and returns the zero-based index of the first occurrence within the range
    /// of elements in the <see cref="FlatList{T}"/> that starts at the specified index and
    /// contains the specified number of elements.</summary>
    /// <param name="comparer">The equality comparer to use</param>
    /// <param name="index">The starting index</param>
    /// <param name="count">The number of elements to search</param>
    /// <param name="item">The item to search for</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The zero-based index of the first occurrence of the item</returns>
    let indexRangeWith comparer index count item (list : FlatList<'T>) = list.IndexOf (item, index, count, comparer)

    /// <summary>Searches for the specified object and returns the zero-based index of the first occurrence within the range
    /// of elements in the <see cref="FlatList{T}"/> that starts at the specified index and
    /// contains the specified number of elements.</summary>
    /// <param name="index">The starting index</param>
    /// <param name="count">The number of elements to search</param>
    /// <param name="item">The item to search for</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The zero-based index of the first occurrence of the item</returns>
    let indexRange index count item list = indexRangeWith HashIdentity.Structural index count item list

    /// <summary>Searches for the specified object and returns the zero-based index of the first occurrence within the range
    /// of elements in the <see cref="FlatList{T}"/> that starts at the specified index and
    /// contains the specified number of elements.</summary>
    /// <param name="comparer">The equality comparer to use</param>
    /// <param name="index">The starting index</param>
    /// <param name="item">The item to search for</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The zero-based index of the first occurrence of the item</returns>
    let indexFromWith comparer index item list = indexRangeWith comparer index (length list - index) item list

    /// <summary>Searches for the specified object and returns the zero-based index of the first occurrence within the range
    /// of elements in the <see cref="FlatList{T}"/> that starts at the specified index and
    /// contains the specified number of elements.</summary>
    /// <param name="index">The starting index</param>
    /// <param name="item">The item to search for</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The zero-based index of the first occurrence of the item</returns>
    let indexFrom index item list = indexFromWith HashIdentity.Structural index item list

    /// <summary>Searches for the specified object and returns the zero-based index of the first occurrence within the range
    /// of elements in the <see cref="FlatList{T}"/> that starts at the specified index and
    /// contains the specified number of elements.</summary>
    /// <param name="comparer">The equality comparer to use</param>
    /// <param name="item">The item to search for</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The zero-based index of the first occurrence of the item</returns>
    let indexWith comparer item list = indexFromWith comparer 0 item list

    /// <summary>Searches for the specified object and returns the zero-based index of the first occurrence within the range
    /// of elements in the <see cref="FlatList{T}"/> that starts at the specified index and
    /// contains the specified number of elements.</summary>
    /// <param name="item">The item to search for</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The zero-based index of the first occurrence of the item</returns>
    let index item list = indexWith HashIdentity.Structural item list

    /// <summary>Searches for the specified object and returns the zero-based index of the last occurrence within the
    /// range of elements in the <see cref="FlatList{T}"/> that contains the specified number
    /// of elements and ends at the specified index.</summary>
    /// <param name="comparer">The equality comparer to use</param>
    /// <param name="index">The ending index</param>
    /// <param name="count">The number of elements to search</param>
    /// <param name="item">The item to search for</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The zero-based index of the last occurrence of the item</returns>
    let lastIndexRangeWith comparer index count item (list : FlatList<'T>) = list.LastIndexOf (item, index, count, comparer)

    /// <summary>Searches for the specified object and returns the zero-based index of the last occurrence within the
    /// range of elements in the <see cref="FlatList{T}"/> that contains the specified number
    /// of elements and ends at the specified index.</summary>
    /// <param name="index">The ending index</param>
    /// <param name="count">The number of elements to search</param>
    /// <param name="item">The item to search for</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The zero-based index of the last occurrence of the item</returns>
    let lastIndexRange index count item list = lastIndexRangeWith HashIdentity.Structural index count item list

    /// <summary>Searches for the specified object and returns the zero-based index of the last occurrence within the
    /// range of elements in the <see cref="FlatList{T}"/> that contains the specified number
    /// of elements and ends at the specified index.</summary>
    /// <param name="comparer">The equality comparer to use</param>
    /// <param name="index">The ending index</param>
    /// <param name="item">The item to search for</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The zero-based index of the last occurrence of the item</returns>
    let lastIndexFromWith comparer index item list = lastIndexRangeWith comparer index (index + 1) item list

    /// <summary>Searches for the specified object and returns the zero-based index of the last occurrence within the
    /// range of elements in the <see cref="FlatList{T}"/> that contains the specified number
    /// of elements and ends at the specified index.</summary>
    /// <param name="index">The ending index</param>
    /// <param name="item">The item to search for</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The zero-based index of the last occurrence of the item</returns>
    let lastIndexFrom index item list = lastIndexFromWith HashIdentity.Structural index item list

    /// <summary>Searches for the specified object and returns the zero-based index of the last occurrence within the
    /// range of elements in the <see cref="FlatList{T}"/> that contains the specified number
    /// of elements and ends at the specified index.</summary>
    /// <param name="comparer">The equality comparer to use</param>
    /// <param name="item">The item to search for</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The zero-based index of the last occurrence of the item</returns>
    let lastIndexWith comparer item list = lastIndexFromWith comparer (length list - 1) item list

    /// <summary>Searches for the specified object and returns the zero-based index of the last occurrence within the
    /// range of elements in the <see cref="FlatList{T}"/> that contains the specified number
    /// of elements and ends at the specified index.</summary>
    /// <param name="item">The item to search for</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The zero-based index of the last occurrence of the item</returns>
    let lastIndex item list = lastIndexWith HashIdentity.Structural item list

    /// <summary>Removes the specified objects from the <see cref="FlatList{T}"/> with the given comparer.</summary>
    /// <param name="comparer">The equality comparer to use</param>
    /// <param name="items">The items to remove</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>A new <see cref="FlatList{T}"/> with the specified items removed</returns>
    let removeAllWith comparer (items : 'T seq) (list : FlatList<'T>) : FlatList<'T> = list.RemoveRange (items, comparer)

    /// <summary>Removes the specified objects from the <see cref="FlatList{T}"/>.</summary>
    /// <param name="items">The items to remove</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>A new <see cref="FlatList{T}"/> with the specified items removed</returns>
    let removeAll items (list : FlatList<'T>) = removeAllWith HashIdentity.Structural items list

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
    let filter (predicate : 'T -> bool) (list : FlatList<'T>) : FlatList<'T> =
        list.RemoveAll (System.Predicate (not << predicate))

    /// <summary>Removes all the elements that do not match the conditions defined by the specified predicate.</summary>
    /// <param name="predicate">The predicate to test elements</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>A new <see cref="FlatList{T}"/> with elements that match the predicate</returns>
    let where (predicate : 'T -> bool) (list : FlatList<'T>) : FlatList<'T> = filter predicate list

    /// <summary>Removes a range of elements from the <see cref="FlatList{T}"/>.</summary>
    /// <param name="index">The starting index</param>
    /// <param name="count">The number of elements to remove</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>A new <see cref="FlatList{T}"/> with the specified range of elements removed</returns>
    let removeRange index (count : int) (list : FlatList<'T>) : FlatList<'T> = list.RemoveRange (index, count)

    /// <summary>Copies a range of elements from the source <see cref="FlatList{T}"/> to the destination array</summary>
    /// <param name="source">The source <see cref="FlatList{T}"/></param>
    /// <param name="sourceIndex">The starting index in the source <see cref="FlatList{T}"/></param>
    /// <param name="destination">The destination array</param>
    /// <param name="destinationIndex">The starting index in the destination array</param>
    /// <param name="count">The number of elements to copy</param>
    /// <exception cref="System.ArgumentException">Thrown when the range is invalid</exception>
    let blit (source : FlatList<'T>) sourceIndex (destination : 'T[]) destinationIndex count =
        source.CopyTo (sourceIndex, destination, destinationIndex, count)

    /// <summary>Sorts a range of elements in the <see cref="FlatList{T}"/> using the specified comparer</summary>
    /// <param name="comparer">The comparer to use</param>
    /// <param name="index">The starting index</param>
    /// <param name="count">The number of elements to sort</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>A new <see cref="FlatList{T}"/> with the specified range of elements sorted</returns>
    let sortRangeWithComparer comparer index count (list : FlatList<'T>) = list.Sort (index, count, comparer)

    /// <summary>Sorts a range of elements in the <see cref="FlatList{T}"/> using the specified comparison function</summary>
    /// <param name="comparer">The comparison function to use</param>
    /// <param name="index">The starting index</param>
    /// <param name="count">The number of elements to sort</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>A new <see cref="FlatList{T}"/> with the specified range of elements sorted</returns>
    let sortRangeWith comparer index count list =
        sortRangeWithComparer (ComparisonIdentity.FromFunction comparer) index count list

    /// <summary>Sorts a range of elements in the <see cref="FlatList{T}"/> using the default comparer</summary>
    /// <param name="index">The starting index</param>
    /// <param name="count">The number of elements to sort</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>A new <see cref="FlatList{T}"/> with the specified range of elements sorted</returns>
    let sortRange index count list = sortRangeWithComparer ComparisonIdentity.Structural index count list

    /// <summary>Sorts the elements in the <see cref="FlatList{T}"/> using the specified comparer</summary>
    /// <param name="comparer">The comparer to use</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>A new <see cref="FlatList{T}"/> with the elements sorted</returns>
    let sortWithComparer (comparer : System.Collections.Generic.IComparer<'T>) (list : FlatList<'T>) = list.Sort (comparer)

    /// <summary>Sorts the elements in the <see cref="FlatList{T}"/> using the specified comparison function</summary>
    /// <param name="comparer">The comparison function to use</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>A new <see cref="FlatList{T}"/> with the elements sorted</returns>
    let sortWith comparer list = sortWithComparer (ComparisonIdentity.FromFunction comparer) list

    /// <summary>Sorts the elements in the <see cref="FlatList{T}"/> using the default comparer</summary>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>A new <see cref="FlatList{T}"/> with the elements sorted</returns>
    let sort (list : FlatList<'T>) = list.Sort ()

    /// <summary>Returns a new array that contains elements of the original array sorted in descending order.</summary>
    /// <param name="list">The input array.</param>
    /// <returns>The sorted array.</returns>
    let inline sortDescending (list : FlatList<'T>) : FlatList<'T> when 'T : comparison = sortWith (fun x y -> compare y x) list

    /// <summary>Returns a new array that contains elements of the original array sorted in descending order using the specified projection.</summary>
    /// <param name="projection">The function to transform the elements into a type that supports comparison.</param>
    /// <param name="list">The input array.</param>
    /// <returns>The sorted array.</returns>
    let inline sortByDescending (projection : 'T -> 'Key) (list : FlatList<'T>) : FlatList<'T> when 'Key : comparison =
        sortWith (fun x y -> compare (projection y) (projection x)) list

    /// <summary>Sorts the array using keys given by the given projection. Keys are compared using Operators.compare.</summary>
    /// <param name="projection">The function to transform the elements into a type supporting comparison.</param>
    /// <param name="list">The input array.</param>
    /// <returns>The sorted array.</returns>
    let inline sortBy (projection : 'T -> 'Key) (list : FlatList<'T>) : FlatList<'T> when 'Key : comparison =
        sortWith (fun x y -> compare (projection x) (projection y)) list

    ////////// Loop-based (now LINQ-based where applicable) //////////

    /// <summary>Concatenates a <see cref="FlatList{T}"/> of <see cref="FlatList{T}"/>s into a single <see cref="FlatList{T}"/></summary>
    /// <param name="arrs">The <see cref="FlatList{T}"/> of <see cref="FlatList{T}"/>s to concatenate</param>
    /// <returns>A new <see cref="FlatList{T}"/> containing all elements from the input <see cref="FlatList{T}"/>s</returns>
    let concat (arrs : FlatList<FlatList<'T>>) =
        let builder = FlatListFactory.CreateBuilder<'T> (arrs.Sum _.Length)
        for i = 0 to arrs.Length - 1 do
            let arr = arrs.[i]
            for j = 0 to arrs.[i].Length - 1 do
                builder.Add (arr.[j])
        builder.MoveToImmutable ()

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
    let inline map (mapping : 'T -> 'U) (list : FlatList<'T>) : FlatList<'U> = list.Select(mapping).ToImmutableArray ()

    /// <summary>Build a new array whose elements are the results of applying the given function
    /// to each of the elements of the array. The integer index passed to the
    /// function indicates the index of element being transformed.</summary>
    /// <param name="mapping">A function to transform an element and its index into a result element.</param>
    /// <param name="list">The input array.</param>
    /// <returns>The array of transformed elements.</returns>
    let mapi (mapping : int -> 'T -> 'U) (list : FlatList<'T>) : FlatList<'U> =
        list.Select(fun x i -> mapping i x).ToImmutableArray ()

    /// <summary>Builds a new array whose elements are the results of applying the given function
    /// to the corresponding elements of the two collections pairwise, also passing the index of
    /// the elements. The two input arrays must have the same lengths.</summary>
    /// <param name="mapping">The function to transform pairs of input elements and their indices.</param>
    /// <param name="list1">The first input array.</param>
    /// <param name="list2">The second input array.</param>
    /// <returns>The array of transformed elements.</returns>
    let mapi2 (mapping : int -> 'T1 -> 'T2 -> 'U) (list1 : FlatList<'T1>) (list2 : FlatList<'T2>) : FlatList<'U> =
        let len1 = list1.Length
        if len1 <> list2.Length then
            invalidArg (nameof list2) ErrorStrings.ListsHaveDifferentLengths

        Enumerable.Range(0, len1).Select(fun i -> mapping i list1.[i] list2.[i]).ToImmutableArray ()

    /// <summary>Builds a new collection whose elements are the results of applying the given function
    /// to the corresponding elements of the two collections pairwise. The two input
    /// arrays must have the same lengths.</summary>
    /// <param name="mapping">The function to transform the pairs of the input elements.</param>
    /// <param name="list1">The first input array.</param>
    /// <param name="list2">The second input array.</param>
    /// <returns>The array of transformed elements.</returns>
    let map2 (mapping : 'T1 -> 'T2 -> 'U) (list1 : FlatList<'T1>) (list2 : FlatList<'T2>) : FlatList<'U> =
        let len1 = list1.Length
        if len1 <> list2.Length then
            invalidArg (nameof list2) ErrorStrings.ListsHaveDifferentLengths

        Enumerable.Range(0, len1).Select(fun i -> mapping list1.[i] list2.[i]).ToImmutableArray ()

    /// <summary>Counts the number of elements in the <see cref="FlatList{T}"/> that satisfy the given predicate</summary>
    /// <param name="projection">A function to project elements from the input <see cref="FlatList{T}"/></param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>A <see cref="FlatList{T}"/> of key-value pairs where the key is the projected value and the value is the count</returns>
    let countBy (projection : 'T -> 'Key) (list : FlatList<'T>) =
        list.GroupBy(projection).Select(fun group -> struct (group.Key, group.Count ())).ToImmutableArray ()

    /// <summary>Creates a <see cref="FlatList{T}"/> containing the elements of the original <see cref="FlatList{T}"/> paired with their indices</summary>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>A <see cref="FlatList{T}"/> containing pairs of indices and elements</returns>
    let indexed (list : FlatList<'T>) = list.Select(fun item index -> struct (index, item)).ToImmutableArray ()

    /// <summary>Applies the given function to each element of the <see cref="FlatList{T}"/></summary>
    /// <param name="action">A function to apply to each element</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    let inline iter action list =
        for item in list do
            action item

    /// <summary>Applies the given function to each element of the <see cref="FlatList{T}"/> and its index</summary>
    /// <param name="action">A function to apply to each element and its index</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    let iteri action (list : FlatList<'T>) =
        for i = 0 to list.Length - 1 do
            do action i list.[i]

    /// <summary>Applies the given function to pair of elements at the same position in the two <see cref="FlatList{T}"/>s</summary>
    /// <param name="action">A function to apply to pairs of elements</param>
    /// <param name="list1">The first input <see cref="FlatList{T}"/></param>
    /// <param name="list2">The second input <see cref="FlatList{T}"/></param>
    /// <exception cref="System.ArgumentException">Thrown when the <see cref="FlatList{T}"/>s have different lengths</exception>
    let iter2 action (list1 : FlatList<'T>) (list2 : FlatList<'T>) =
        let len = list1.Length
        if len <> list2.Length then
            invalidArg (nameof list2) ErrorStrings.ListsHaveDifferentLengths
        for i = 0 to len - 1 do
            do action list1.[i] list2.[i]

    /// <summary>Applies the given function to the pair of elements at the same position in the two <see cref="FlatList{T}"/>s along with their index</summary>
    /// <param name="action">A function to apply to pairs of elements and their index</param>
    /// <param name="list1">The first input <see cref="FlatList{T}"/></param>
    /// <param name="list2">The second input <see cref="FlatList{T}"/></param>
    /// <exception cref="System.ArgumentException">Thrown when the <see cref="FlatList{T}"/>s have different lengths</exception>
    let iteri2 action (list1 : FlatList<'T>) (list2 : FlatList<'T>) =
        let len1 = list1.Length
        if len1 <> list2.Length then
            invalidArg (nameof list2) ErrorStrings.ListsHaveDifferentLengths
        for i = 0 to len1 - 1 do
            action i list1.[i] list2.[i]

    /// <summary>Tests if any element of the <see cref="FlatList{T}"/> satisfies the given predicate</summary>
    /// <param name="predicate">A function to test each element</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>True if any element satisfies the predicate, false otherwise</returns>
    let exists (predicate : 'T -> bool) (list : FlatList<'T>) = list.Any (predicate)

    /// <summary>Tests if any corresponding pair of elements from the two <see cref="FlatList{T}"/>s satisfies the given predicate</summary>
    /// <param name="predicate">A function to test pairs of elements</param>
    /// <param name="list1">The first input <see cref="FlatList{T}"/></param>
    /// <param name="list2">The second input <see cref="FlatList{T}"/></param>
    /// <returns>True if any pair of elements satisfies the predicate, false otherwise</returns>
    /// <exception cref="System.ArgumentException">Thrown when the <see cref="FlatList{T}"/>s have different lengths</exception>
    let exists2 (predicate : 'T -> 'T -> bool) (list1 : FlatList<'T>) (list2 : FlatList<'T>) =
        let len1 = list1.Length
        if len1 <> list2.Length then
            invalidArg (nameof list2) ErrorStrings.ListsHaveDifferentLengths
        let rec loop i = i < len1 && (predicate list1.[i] list2.[i] || loop (i + 1))
        loop 0

    /// <summary>Tests if all elements of the <see cref="FlatList{T}"/> satisfy the given predicate</summary>
    /// <param name="predicate">A function to test each element</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>True if all elements satisfy the predicate, false otherwise</returns>
    let forall (predicate : 'T -> bool) (list : FlatList<'T>) = list.All (predicate)

    /// <summary>Tests if all corresponding pairs of elements from the two <see cref="FlatList{T}"/>s satisfy the given predicate</summary>
    /// <param name="predicate">A function to test pairs of elements</param>
    /// <param name="list1">The first input <see cref="FlatList{T}"/></param>
    /// <param name="list2">The second input <see cref="FlatList{T}"/></param>
    /// <returns>True if all pairs of elements satisfy the predicate, false otherwise</returns>
    /// <exception cref="System.ArgumentException">Thrown when the <see cref="FlatList{T}"/>s have different lengths</exception>
    let forall2 predicate (list1 : FlatList<'T>) (list2 : FlatList<'T>) =
        let len1 = list1.Length
        if len1 <> list2.Length then
            invalidArg (nameof list2) ErrorStrings.ListsHaveDifferentLengths
        let rec loop i = i >= len1 || (predicate list1.[i] list2.[i] && loop (i + 1))
        loop 0

    /// <summary>Tests if the given element exists in the <see cref="FlatList{T}"/></summary>
    /// <param name="e">The element to find</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>True if the element exists in the <see cref="FlatList{T}"/>, false otherwise</returns>
    let inline contains item (list : FlatList<'T>) = list.Contains (item)

    /// <summary>Splits the <see cref="FlatList{T}"/> into two <see cref="FlatList{T}"/>s, containing the elements for which the given predicate returns true and false respectively</summary>
    /// <param name="predicate">A function to test each element</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>A tuple of two <see cref="FlatList{T}"/>s, containing the elements for which the predicate returns true and false respectively</returns>
    let partition (predicate : 'T -> bool) (list : FlatList<'T>) =
        let res1 = builderWith list.Length
        let res2 = builderWith list.Length
        for x in list do // Iteration will cause InvalidOperationException if list is default
            if predicate x then res1.Add x else res2.Add x
        (ofBuilder res1, ofBuilder res2)


    /// <summary>Returns the first element for which the given predicate returns true</summary>
    /// <param name="predicate">A function to test elements</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The first element for which the predicate returns true</returns>
    /// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown if no element satisfies the predicate</exception>
    let find (predicate : 'T -> bool) (list : FlatList<'T>) = list.First (predicate)

    /// <summary>Returns the first element for which the given predicate returns true, or ValueNone if no such element exists</summary>
    /// <param name="predicate">A function to test elements</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>ValueSome value if an element satisfies the predicate, ValueNone otherwise</returns>
    let tryFind (predicate : 'T -> bool) (list : FlatList<'T>) : 'T voption = list.Where (predicate) |> Seq.vtryHead

    /// <summary>Returns the last element for which the given predicate returns true</summary>
    /// <param name="predicate">A function to test elements</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The last element for which the predicate returns true</returns>
    /// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown if no element satisfies the predicate</exception>
    let findBack (predicate : 'T -> bool) (list : FlatList<'T>) = list.Last (predicate)

    /// <summary>Returns the last element for which the given predicate returns true, or ValueNone if no such element exists</summary>
    /// <param name="predicate">A function to test elements</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>ValueSome value if an element satisfies the predicate, ValueNone otherwise</returns>
    let tryFindBack (predicate : 'T -> bool) (list : FlatList<'T>) : 'T voption =
        seq {
            for i = list.Length - 1 downto 0 do
                yield list.[i]
        }
        |> Seq.where predicate
        |> Seq.vtryHead

    /// <summary>Returns the last index for which the given predicate returns true</summary>
    /// <param name="predicate">A function to test elements</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The last index for which the predicate returns true</returns>
    /// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown if no element satisfies the predicate</exception>
    let findIndexBack (predicate : 'T -> bool) (list : FlatList<'T>) =
        let len = list.Length - 1
        seq {
            for i = len downto 0 do
                yield struct (len - i, list.[i])
        }
        |> Seq.where (fun struct (i, item) -> predicate item)
        |> Seq.map (fun struct (i, item) -> i)
        |> Seq.head

    /// <summary>Returns the last index for which the given predicate returns true, or ValueNone if no such element exists</summary>
    /// <param name="predicate">A function to test elements</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>ValueSome index if an element satisfies the predicate, ValueNone otherwise</returns>
    let tryFindIndexBack (predicate : 'T -> bool) (list : FlatList<'T>) : int voption =
        let len = list.Length - 1
        seq {
            for i = len downto 0 do
                yield struct (len - i, list.[i])
        }
        |> Seq.where (fun struct (i, item) -> predicate item)
        |> Seq.map (fun struct (i, item) -> i)
        |> Seq.vtryHead

    /// <summary>Returns the first value for which the given function returns ValueSome value</summary>
    /// <param name="chooser">A function to generate options from the elements</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The first value for which the chooser returns ValueSome value</returns>
    /// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown if the chooser returns ValueNone for all elements</exception>
    let pick (chooser : 'T -> 'U voption) (list : FlatList<'T>) =
        list.Select(chooser).Where(ValueOption.isSome).Select (ValueOption.get)
        |> Seq.head

    /// <summary>Returns the first value for which the given function returns ValueSome value, or ValueNone if no such element exists</summary>
    /// <param name="chooser">A function to generate options from the elements</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The first value for which the chooser returns ValueSome value, or ValueNone</returns>
    let tryPick (chooser : 'T -> 'U voption) (list : FlatList<'T>) : 'U voption =
        list.Select(chooser).Where(ValueOption.isSome).Select (ValueOption.get)
        |> Seq.vtryHead

    /// <summary>Builds a new <see cref="FlatList{T}"/> containing only the elements for which the given function returns ValueSome value</summary>
    /// <param name="chooser">A function to generate options from the elements</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>A <see cref="FlatList{T}"/> containing the values wrapped in ValueSome by the chooser</returns>
    let choose (chooser : 'T -> 'T voption) (list : FlatList<'T>) =
        list.Select(chooser).Where(ValueOption.isSome).Select(ValueOption.get).ToImmutableArray ()

    /// <summary>Creates a <see cref="FlatList{T}"/> by applying a key-generating function to each element of the <see cref="FlatList{T}"/> and grouping the elements by the resulting keys</summary>
    /// <param name="projection">A function to transform elements into keys</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>A <see cref="FlatList{T}"/> of tuples where each tuple contains a key and a <see cref="FlatList{T}"/> of all elements that match the key</returns>
    let groupBy (projection : 'T -> 'Key) (list : FlatList<'T>) =
        list.GroupBy(projection).Select(fun group -> struct (group.Key, group.ToImmutableArray ())).ToImmutableArray ()

    /// <summary>Returns a new <see cref="FlatList{T}"/> that contains the elements of the original <see cref="FlatList{T}"/> but with duplicates eliminated by using the supplied projection function</summary>
    /// <param name="projection">A function to transform elements before comparing them</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>A <see cref="FlatList{T}"/> with distinct elements as determined by the projection function</returns>
    let distinctBy (projection : 'T -> 'Key) (list : FlatList<'T>) =
        let setBuilder = ImmutableHashSet.CreateBuilder<'Key> ()
        let arrayBuilder = ImmutableArray.CreateBuilder<'T> ()
        for i = 0 to list.Length - 1 do
            let item = list.[i] // list.[i] will throw if list is default
            if setBuilder.Add (projection item) then
                arrayBuilder.Add (item)
        arrayBuilder.ToImmutable ()

    /// <summary>Creates a new <see cref="FlatList{T}"/> by applying a mapping function to each element of the input <see cref="FlatList{T}"/> and concatenating the results</summary>
    /// <param name="mapping">A function to transform elements of the input <see cref="FlatList{T}"/> into <see cref="FlatList{T}"/>s</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>A <see cref="FlatList{T}"/> containing the concatenation of all the <see cref="FlatList{T}"/>s generated by the mapping function</returns>
    let collect (mapping : 'T -> 'U seq) (list : FlatList<'T>) : FlatList<'U> = list.SelectMany(mapping).ToImmutableArray ()

    /// <summary>Gets an element in the <see cref="FlatList{T}"/> at the specified index</summary>
    /// <param name="index">The index of the element to retrieve</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>ValueSome value containing the element, or ValueNone if the index is out of range</returns>
    let tryItem index (list : FlatList<'T>) : voption<'T> =
        // list.Length or list.[index] will throw if list is default before comparison happens
        if list.IsDefault then
            ValueNone // Explicitly handle default case for tryItem to return ValueNone
        elif index >= 0 && index < list.Length then
            ValueSome list.[index]
        else
            ValueNone

    /// <summary>Returns the first element of the <see cref="FlatList{T}"/></summary>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The first element of the <see cref="FlatList{T}"/></returns>
    /// <exception cref="System.ArgumentException">Thrown when the <see cref="FlatList{T}"/> is empty</exception>
    let head (list : FlatList<'T>) = list.First ()

    /// <summary>Returns the first element of the <see cref="FlatList{T}"/>, or ValueNone if the <see cref="FlatList{T}"/> is empty</summary>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>ValueSome value containing the first element, or ValueNone if the <see cref="FlatList{T}"/> is empty</returns>
    let tryHead (list : FlatList<'T>) : 'T voption = if list.IsEmpty then ValueNone else ValueSome list.[0]

    /// <summary>Returns the last element of the <see cref="FlatList{T}"/></summary>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The last element of the <see cref="FlatList{T}"/></returns>
    /// <exception cref="System.ArgumentException">Thrown when the <see cref="FlatList{T}"/> is empty</exception>
    let last (list : FlatList<_>) = list.Last () // Enumerable.Last throws if empty or default

    /// <summary>Returns the last element of the <see cref="FlatList{T}"/>, or ValueNone if the <see cref="FlatList{T}"/> is empty</summary>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>ValueSome value containing the last element, or ValueNone if the <see cref="FlatList{T}"/> is empty</returns>
    let tryLast (list : FlatList<'T>) : 'T voption =
        if list.IsEmpty then
            ValueNone
        else
            ValueSome list.[list.Length - 1]

    /// <summary>Returns the <see cref="FlatList{T}"/> without its first element</summary>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>A <see cref="FlatList{T}"/> containing all elements of the input <see cref="FlatList{T}"/> except the first one</returns>
    let tail (list : FlatList<'T>) =
        if list.IsEmpty then
            invalidArg (nameof list) "List must not be empty to get tail."
        list.Slice (1, list.Length - 1)

    /// <summary>Returns the <see cref="FlatList{T}"/> without its first element, or ValueNone if the <see cref="FlatList{T}"/> is empty</summary>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>ValueSome value containing the <see cref="FlatList{T}"/> without its first element, or ValueNone if the <see cref="FlatList{T}"/> is empty</returns>
    let tryTail (list : FlatList<'T>) : voption<FlatList<'T>> =
        if list.IsEmpty then
            ValueNone
        else
            ValueSome (list.Slice (1, list.Length - 1))

    /// <summary>Returns the first N elements of the <see cref="FlatList{T}"/></summary>
    /// <param name="count">The number of elements to take</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>A <see cref="FlatList{T}"/> containing the first N elements</returns>
    let take (count : int) (list : FlatList<'T>) =
        if count < 0 then
            invalidArg (nameof count) ErrorStrings.InputMustBeNonNegative
        let len = list.Length // Will throw if list is default
        if count = 0 then empty
        elif count >= len then list
        else list.Slice (0, count)

    /// <summary>Returns a <see cref="FlatList{T}"/> containing the first elements of the input <see cref="FlatList{T}"/> for which the given predicate returns true</summary>
    /// <param name="predicate">A function to test each element</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>A <see cref="FlatList{T}"/> containing the first elements for which the predicate returns true</returns>
    let takeWhile (predicate : 'T -> bool) (list : FlatList<'T>) = list.TakeWhile(predicate).ToImmutableArray ()

    /// <summary>Returns the <see cref="FlatList{T}"/> without its first N elements</summary>
    /// <param name="index">The number of elements to skip</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>A <see cref="FlatList{T}"/> containing all except the first N elements</returns>
    let skip index (list : FlatList<'T>) =
        if index < 0 then
            invalidArg (nameof index) ErrorStrings.InputMustBeNonNegative
        let len = list.Length // Will throw if list is default
        if index = 0 then list
        elif index >= len then empty
        else list.Slice (index, len - index)

    /// <summary>Returns a <see cref="FlatList{T}"/> that skips the elements of the input <see cref="FlatList{T}"/> while the given predicate returns true, then returns the rest</summary>
    /// <param name="predicate">A function to test each element</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>A <see cref="FlatList{T}"/> that skips the elements while the predicate returns true, then contains the rest</returns>
    let skipWhile (predicate : 'T -> bool) (list : FlatList<'T>) = list.SkipWhile(predicate).ToImmutableArray ()

    /// <summary>Gets a sublist of the input <see cref="FlatList{T}"/></summary>
    /// <param name="start">The index of the first element to include</param>
    /// <param name="count">The number of elements in the sublist</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>A <see cref="FlatList{T}"/> containing the elements from start index for the given count</returns>
    let sub start count (list : FlatList<'T>) = list.Slice (start, count)

    /// <summary>Returns a <see cref="FlatList{T}"/> that contains no more than N elements of the input <see cref="FlatList{T}"/></summary>
    /// <param name="count">The maximum number of elements to include</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>A <see cref="FlatList{T}"/> containing at most N elements</returns>
    let truncate count (list : FlatList<'T>) = if count < list.Length then list.Slice (0, count) else list // list.Length throws if default

    /// <summary>Splits the <see cref="FlatList{T}"/> into two <see cref="FlatList{T}"/>s at the specified index</summary>
    /// <param name="index">The index at which to split the <see cref="FlatList{T}"/></param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>A tuple of two <see cref="FlatList{T}"/>s, the first containing the elements up to the index, the second containing the rest</returns>
    let splitAt index (list : FlatList<'T>) = (list.Slice (0, index), list.Slice (index, list.Length - index))

    /// <summary>Applies a function to the builder and returns the resulting <see cref="FlatList{T}"/></summary>
    /// <param name="f">The function to apply to the builder</param>
    /// <returns>The <see cref="FlatList{T}"/> created from the builder after applying the function</returns>
    let inline build f =
        let builder = builder ()
        f builder
        moveFromBuilder builder

    /// <summary>Updates the <see cref="FlatList{T}"/> by applying a function to a builder initialized with the <see cref="FlatList{T}"/>'s elements</summary>
    /// <param name="f">The function to apply to the builder</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The updated <see cref="FlatList{T}"/></returns>
    let inline update f (list : FlatList<'T>) =
        let builder = toBuilder list // toBuilder will throw if list is default
        f builder
        moveFromBuilder builder

    /// <summary>Returns the index of the first element in the <see cref="FlatList{T}"/> that satisfies the given predicate</summary>
    /// <param name="predicate">The function to test the input elements</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The index of the first element that satisfies the predicate</returns>
    /// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown if no element satisfies the predicate</exception>
    let findIndex (predicate : 'T -> bool) (list : FlatList<'T>) =
        list.Select (fun item i -> struct (item, i))
        |> Seq.where (fun struct (item, i) -> predicate item)
        |> Seq.map (fun struct (item, i) -> i)
        |> Seq.head

    /// <summary>Returns the index of the first element in the <see cref="FlatList{T}"/> that satisfies the given predicate, or ValueNone if no such element exists</summary>
    /// <param name="predicate">The function to test the input elements</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The index of the first element that satisfies the predicate, or ValueNone</returns>
    let tryFindIndex (predicate : 'T -> bool) (list : FlatList<'T>) : int voption =
        list.Select (fun item i -> struct (item, i))
        |> Seq.where (fun struct (item, i) -> predicate item)
        |> Seq.map (fun struct (item, i) -> i)
        |> Seq.vtryHead

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
    let windowed windowSize (list : FlatList<'T>) =
        if windowSize < 1 then
            invalidArg (nameof windowSize) ErrorStrings.InputMustBeNonNegative
        let len = list.Length // Will throw if list is default
        if windowSize > len then
            empty
        else
            Enumerable
                .Range(0, len - windowSize + 1)
                .Select(fun i -> list.Slice (i, windowSize)) // list.Slice throws if list is default (already caught by len)
                .ToImmutableArray ()

    /// <summary>Returns a new <see cref="FlatList{T}"/> containing pairs of adjacent elements from the input <see cref="FlatList{T}"/></summary>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The resulting <see cref="FlatList{T}"/> of pairs</returns>
    let pairwise (list : FlatList<'T>) =
        if list.Length < 2 then
            empty // list.Length throws if default
        else
            Enumerable.Zip(list, list.Skip (1), fun first second -> (first, second)).ToImmutableArray ()

    /// <summary>Splits the <see cref="FlatList{T}"/> into chunks of size at most 'chunkSize'</summary>
    /// <param name="chunkSize">The maximum size of each chunk</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The <see cref="FlatList{T}"/> split into chunks</returns>
    /// <exception cref="System.ArgumentException">Thrown when chunkSize is not positive or when <see cref="FlatList{T}"/> is default</exception>
    let chunkBySize chunkSize (list : FlatList<'T>) =
        if chunkSize <= 0 then
            invalidArg (nameof chunkSize) ErrorStrings.InputMustBeNonNegative
        let len = list.Length // Will throw if list is default
        if len = 0 then
            empty
        else
            let numChunks = (len + chunkSize - 1) / chunkSize
            Enumerable
                .Range(0, numChunks)
                .Select(fun i ->
                    let start = i * chunkSize
                    let count = min chunkSize (len - start)
                    list.Slice (start, count)
                ) // list.Slice throws if list is default (already caught by len)
                .ToImmutableArray ()

    /// <summary>Splits the input array into at most count chunks.</summary>
    /// <param name="count">The maximum number of chunks.</param>
    /// <param name="list">The input array.</param>
    /// <returns>The array split into chunks.</returns>
    /// <exception cref="System.ArgumentException">Thrown when count is not positive.</exception>
    let splitInto (count : int) (list : FlatList<'T>) : FlatList<FlatList<'T>> =
        if count <= 0 then
            invalidArg (nameof count) ErrorStrings.InputMustBeNonNegative

        let len = list.Length
        if len = 0 then
            empty
        else
            let chunkSize = (len + count - 1) / count // ceil(len / count)
            chunkBySize chunkSize list

    /// <summary>Returns a new <see cref="FlatList{T}"/> that contains the elements of the original <see cref="FlatList{T}"/> but with duplicates removed</summary>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The <see cref="FlatList{T}"/> with distinct elements</returns>
    let distinct (list : FlatList<'T>) =
        let builder = ImmutableHashSet.CreateBuilder<'T> ()
        for i = 0 to list.Length - 1 do
            let item = list.[i] // list.[i] will throw if list is default
            builder.Add (item) |> ignore
        builder.ToImmutableArray ()

    /// <summary>Returns a new <see cref="FlatList{T}"/> that contains all pairwise combinations of elements from the first and second <see cref="FlatList{T}"/>s</summary>
    /// <param name="xs">The first input <see cref="FlatList{T}"/></param>
    /// <param name="ys">The second input <see cref="FlatList{T}"/></param>
    /// <returns>The <see cref="FlatList{T}"/> of all pairwise combinations</returns>
    let allPairs (xs : FlatList<'T>) (ys : FlatList<'U>) = xs.SelectMany(fun x -> ys.Select (fun y -> (x, y))).ToImmutableArray ()

    /// <summary>Returns a new <see cref="FlatList{T}"/> with the elements permuted according to the specified permutation</summary>
    /// <param name="indexMap">The function that maps input indices to output indices</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The permuted <see cref="FlatList{T}"/></returns>
    /// <exception cref="System.ArgumentException">Thrown when the permutation function returns an out-of-range index</exception>
    let permute indexMap (list : FlatList<'T>) =
        let len = list.Length // Will throw if list is default
        let permutedArray = Array.zeroCreate<'T> len
        for i = 0 to len - 1 do
            let j = indexMap i
            if j < 0 || j >= len then
                invalidArg (nameof indexMap) "Invalid permutation"
            permutedArray.[j] <- list.[i] // list.[i] will throw if list is default (already caught by len)
        FlatListFactory.CreateRange permutedArray

    /// <summary>Combines the two <see cref="FlatList{T}"/>s into a <see cref="FlatList{T}"/> of pairs. The two <see cref="FlatList{T}"/>s must have equal lengths</summary>
    /// <param name="list1">The first input <see cref="FlatList{T}"/></param>
    /// <param name="list2">The second input <see cref="FlatList{T}"/></param>
    /// <returns>The <see cref="FlatList{T}"/> of pairs</returns>
    /// <exception cref="System.ArgumentException">Thrown when the <see cref="FlatList{T}"/>s have different lengths</exception>
    let zip (list1 : FlatList<'T>) (list2 : FlatList<'U>) =
        let len1 = list1.Length // .Length throws if default
        if len1 <> list2.Length then
            invalidArg (nameof list2) ErrorStrings.ListsHaveDifferentLengths
        Enumerable
            .Range(0, len1)
            .Select(fun i -> (list1.[i], list2.[i])) // .[i] throws if default (caught by .Length)
            .ToImmutableArray ()

    /// <summary>Combines the three <see cref="FlatList{T}"/>s into a <see cref="FlatList{T}"/> of triples. The three <see cref="FlatList{T}"/>s must have equal lengths</summary>
    /// <param name="list1">The first input <see cref="FlatList{T}"/></param>
    /// <param name="list2">The second input <see cref="FlatList{T}"/></param>
    /// <param name="list3">The third input <see cref="FlatList{T}"/></param>
    /// <returns>The <see cref="FlatList{T}"/> of triples</returns>
    /// <exception cref="System.ArgumentException">Thrown when the <see cref="FlatList{T}"/>s have different lengths</exception>
    let zip3 (list1 : FlatList<'T>) (list2 : FlatList<'U>) (list3 : FlatList<'V>) =
        let len1 = list1.Length // .Length throws if default
        if len1 <> list2.Length then
            invalidArg (nameof list2) ErrorStrings.ListsHaveDifferentLengths
        if len1 <> list3.Length then
            invalidArg (nameof list3) ErrorStrings.ListsHaveDifferentLengths
        Enumerable
            .Range(0, len1)
            .Select(fun i -> (list1.[i], list2.[i], list3.[i])) // .[i] throws if default (caught by .Length)
            .ToImmutableArray ()

    /// <summary>Splits a <see cref="FlatList{T}"/> of pairs into two <see cref="FlatList{T}"/>s</summary>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The two <see cref="FlatList{T}"/>s unzipped from the input <see cref="FlatList{T}"/></returns>
    let unzip (list : FlatList<'T * 'U>) =
        if list.IsEmpty then
            (empty, empty) // IsEmpty is safe for default (true)
        else
            (list.Select(fst).ToImmutableArray (), list.Select(snd).ToImmutableArray ())

    /// <summary>Splits a <see cref="FlatList{T}"/> of triples into three <see cref="FlatList{T}"/>s</summary>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The three <see cref="FlatList{T}"/>s unzipped from the input <see cref="FlatList{T}"/></returns>
    let unzip3 (list : FlatList<'T * 'U * 'V>) =
        if list.IsEmpty then
            (empty, empty, empty) // IsEmpty is safe for default (true)
        else
            let res1 = list.Select(fun (x, _, _) -> x).ToImmutableArray ()
            let res2 = list.Select(fun (_, y, _) -> y).ToImmutableArray ()
            let res3 = list.Select(fun (_, _, z) -> z).ToImmutableArray ()
            (res1, res2, res3)

    /// <summary>Returns the average of the elements in the <see cref="FlatList{T}"/></summary>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The average of the elements</returns>
    /// <exception cref="System.ArgumentException">Thrown when the <see cref="FlatList{T}"/> is empty</exception>
    let inline average<'T
        when 'T : (static member (+) : 'T * 'T -> 'T)
        and 'T : (static member DivideByInt : 'T * int -> 'T)
        and 'T : (static member Zero : 'T)>
        (list : FlatList<'T>)
        =
        if list.Length = 0 then
            invalidArg (nameof list) LanguagePrimitives.ErrorStrings.InputArrayEmptyString
        let sum = list.Aggregate ('T.Zero, fun acc x -> Checked.(+) acc x) // Average will throw if default
        'T.DivideByInt (sum, list.Length)

    /// <summary>Returns the average of the results of applying the function to each element of the <see cref="FlatList{T}"/></summary>
    /// <param name="projection">The function to transform the <see cref="FlatList{T}"/> elements before averaging</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The average of the projected elements</returns>
    /// <exception cref="System.ArgumentException">Thrown when the <see cref="FlatList{T}"/> is empty</exception>
    let inline averageBy<'T, 'U
        when 'U : (static member (+) : 'U * 'U -> 'U)
        and 'U : (static member DivideByInt : 'U * int -> 'U)
        and 'U : (static member Zero : 'U)>
        (projection : 'T -> 'U)
        (list : FlatList<'T>)
        =
        let sum = list.Aggregate ('U.Zero, fun acc x -> Checked.(+) acc (projection x)) // AverageBy will throw if default
        'U.DivideByInt (sum, list.Length)


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
    let fold<'T, 'State> (folder : 'State -> 'T -> 'State) (state : 'State) (list : FlatList<'T>) = list.Aggregate (state, folder) // Aggregate will throw if default

    /// <summary>Applies a function to corresponding elements of two <see cref="FlatList{T}"/>s, threading an accumulator argument through the computation</summary>
    /// <param name="folder">The function to update the state given the input elements from both <see cref="FlatList{T}"/>s</param>
    /// <param name="state">The initial state</param>
    /// <param name="list1">The first input <see cref="FlatList{T}"/></param>
    /// <param name="list2">The second input <see cref="FlatList{T}"/></param>
    /// <returns>The final state</returns>
    /// <exception cref="System.ArgumentException">Thrown when the <see cref="FlatList{T}"/>s have different lengths</exception>
    let fold2<'T1, 'T2, 'State>
        (folder : 'State -> 'T1 -> 'T2 -> 'State)
        (state : 'State)
        (list1 : FlatList<'T1>)
        (list2 : FlatList<'T2>)
        =
        let len1 = list1.Length // .Length throws if default
        if len1 <> list2.Length then
            invalidArg (nameof list2) ErrorStrings.ListsHaveDifferentLengths
        let mutable acc = state
        for i = 0 to len1 - 1 do
            acc <- folder acc list1.[i] list2.[i] // .[i] throws if default (caught by .Length)
        acc

    /// <summary>Applies a function to each element of the <see cref="FlatList{T}"/>, threading an accumulator argument through the computation, starting from the end</summary>
    /// <param name="folder">The function to update the state given the input elements, starting from the end</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <param name="state">The initial state</param>
    /// <returns>The final state</returns>
    let foldBack<'T, 'State> (folder : 'State -> 'T -> 'State) (list : FlatList<'T>) (state : 'State) =
        seq { for i = list.Length - 1 downto 0 do yield list.[i] }
        |> _.Aggregate(state, folder)

    /// <summary>Applies a function to corresponding elements of two <see cref="FlatList{T}"/>s, threading an accumulator argument through the computation, starting from the end</summary>
    /// <param name="folder">The function to update the state given the input elements from both <see cref="FlatList{T}"/>s, starting from the end</param>
    /// <param name="list1">The first input <see cref="FlatList{T}"/></param>
    /// <param name="list2">The second input <see cref="FlatList{T}"/></param>
    /// <param name="state">The initial state</param>
    /// <returns>The final state</returns>
    /// <exception cref="System.ArgumentException">Thrown when the <see cref="FlatList{T}"/>s have different lengths</exception>
    let foldBack2<'T1, 'T2, 'State>
        (folder : 'T1 -> 'T2 -> 'State -> 'State)
        (list1 : FlatList<'T1>)
        (list2 : FlatList<'T2>)
        (state : 'State)
        =
        let len1 = list1.Length // .Length throws if default
        if len1 <> list2.Length then
            invalidArg (nameof list2) ErrorStrings.ListsHaveDifferentLengths
        let mutable acc = state
        for i = len1 - 1 downto 0 do
            acc <- folder list1.[i] list2.[i] acc // .[i] throws if default (caught by .Length)
        acc

    /// <summary>Applies a function to each element of the <see cref="FlatList{T}"/>, threading an accumulator argument through the computation.
    /// This function takes the second argument, and applies the function to it and the first element of the <see cref="FlatList{T}"/>.
    /// Then, it passes this result into the function along with the second element, and so on.
    /// Finally, it returns the final result. If the <see cref="FlatList{T}"/> is empty, an exception is raised.</summary>
    /// <param name="reduction">The function to reduce the <see cref="FlatList{T}"/> with</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The final accumulated value</returns>
    /// <exception cref="System.ArgumentException">Thrown when the <see cref="FlatList{T}"/> is empty</exception>
    let reduce (reduction : 'T -> 'T -> 'T) (list : FlatList<'T>) = list.Aggregate (reduction) // Aggregate will throw if default (via GetEnumerator)

    /// <summary>Applies a function to each element of the <see cref="FlatList{T}"/>, threading an accumulator argument through the computation, starting from the end.
    /// This function takes the last element of the <see cref="FlatList{T}"/> and the second-to-last element, and applies the function to them.
    /// Then, it passes this result into the function along with the third-to-last element, and so on.
    /// Finally, it returns the final result. If the <see cref="FlatList{T}"/> is empty, an exception is raised.</summary>
    /// <param name="reduction">The function to reduce the <see cref="FlatList{T}"/> with, starting from the end</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The final accumulated value</returns>
    /// <exception cref="System.ArgumentException">Thrown when the <see cref="FlatList{T}"/> is empty</exception>
    let reduceBack (reduction : 'T -> 'T -> 'T) (list : FlatList<'T>) =
        (seq {
            for i = list.Length - 1 downto 0 do
                list[i]
        })
            .Aggregate (reduction)

    /// <summary>Like fold, but returns both the intermediate and final results</summary>
    /// <param name="folder">The function to update the state given the input elements</param>
    /// <param name="state">The initial state</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The <see cref="FlatList{T}"/> of all intermediate and final states</returns>
    let scan<'T, 'State> folder (state : 'State) (list : FlatList<'T>) =
        let builder = FlatListFactory.CreateBuilder<'State> (list.Length + 1) // list.Length throws if default
        builder.Add state
        let mutable currentState = state
        for item in list do // Iteration throws if default (caught by list.Length)
            currentState <- folder currentState item
            builder.Add currentState
        builder.MoveToImmutable ()

    /// <summary>Like foldBack, but returns both the intermediate and final results</summary>
    /// <param name="folder">The function to update the state given the input elements, starting from the end</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <param name="state">The initial state</param>
    /// <returns>The <see cref="FlatList{T}"/> of all intermediate and final states, in reverse order of computation</returns>
    let scanBack<'T, 'State> folder (list : FlatList<'T>) (state : 'State) =
        let len = list.Length // list.Length throws if default
        let results = Array.zeroCreate<'State> (len + 1)
        results.[len] <- state
        let mutable currentState = state
        for i = len - 1 downto 0 do
            currentState <- folder list.[i] currentState // list.[i] throws if default (caught by list.Length)
            results.[i] <- currentState
        FlatListFactory.CreateRange results

    /// <summary>Returns the only element of the array.</summary>
    /// <param name="array">The input array.</param>
    /// <returns>The only element of the array.</returns>
    /// <exception cref="System.ArgumentException">Thrown when the input does not have precisely one element.</exception>
    let exactlyOne (list : FlatList<'T>) = list.Single ()

    /// <summary>Returns the only element of the array or None if the array is empty or contains more than one element.</summary>
    /// <param name="array">The input array.</param>
    /// <returns>The only element of the array or None.</returns>
    let tryExactlyOne (list : FlatList<'T>) : 'T voption =
        if list.IsDefaultOrEmpty || list.Length <> 1 then
            ValueNone
        else
            ValueSome list.[0]

    /// <summary>Returns a new list with the distinct elements of the input array which do not appear in the itemsToExclude sequence</summary>
    /// <param name="itemsToExclude">A sequence whose elements that also occur in the input array will cause those elements to be removed</param>
    /// <param name="list">The input array</param>
    /// <returns>A new array that contains the distinct elements of list that do not appear in itemsToExclude</returns>
    let except (itemsToExclude : 'T seq) (list : FlatList<'T>) : FlatList<'T> =
        let excludeSet = HashSet (itemsToExclude)
        filter (fun x -> not (excludeSet.Contains x)) list

    /// <summary>Returns the sum of the elements in the array.</summary>
    /// <param name="list">The input array.</param>
    /// <returns>The resulting sum.</returns>
    /// <exception cref="System.ArgumentNullException">Thrown when the input array is null.</exception>
    let inline sum (list : FlatList< ^T >) : ^T when ^T : (static member (+) : ^T * ^T -> ^T) and ^T : (static member Zero : ^T) =
        list.Aggregate (LanguagePrimitives.GenericZero< ^T>, fun acc x -> acc + x)

    /// <summary>Returns the sum of the results generated by applying the function to each element of the array.</summary>
    /// <param name="projection">The function to transform the array elements into the type to be summed.</param>
    /// <param name="list">The input array.</param>
    /// <returns>The resulting sum.</returns>
    let inline sumBy
        (projection : 'T -> 'U)
        (list : FlatList<'T>)
        : 'U when 'U : (static member (+) : 'U * 'U -> 'U) and 'U : (static member Zero : 'U) =
        list.Aggregate (LanguagePrimitives.GenericZero<'U>, fun acc x -> acc + (projection x))

    /// <summary>Returns the transpose of the given sequence of arrays.</summary>
    /// <param name="lists">The input sequence of arrays.</param>
    /// <returns>The transposed array.</returns>
    /// <exception cref="System.ArgumentException">Thrown when the input arrays differ in length.</exception>
    let transpose (lists : FlatList<FlatList<'T>>) : FlatList<FlatList<'T>> =
        if lists.IsDefaultOrEmpty then
            empty
        else
            let len0 = lists.[0].Length
            // Verify all inner arrays have the same length
            for i = 1 to lists.Length - 1 do
                if lists.[i].Length <> len0 then
                    invalidArg (nameof lists) "All inner arrays must have the same length."

            Enumerable
                .Range(0, len0)
                .Select(fun j -> Enumerable.Range(0, lists.Length).Select(fun i -> lists.[i].[j]).ToImmutableArray ())
                .ToImmutableArray ()

    /// <summary>Builds a list from the given array.</summary>
    /// <param name="list">The input array.</param>
    /// <returns>The list of array elements.</returns>
    let toList (list : FlatList<'T>) : 'T list =
        if list.IsDefaultOrEmpty then
            []
        else
            seq {
                for i = 0 to list.Length - 1 do
                    yield list.[i]
            }
            |> Seq.toList

    /// <summary>Builds a <see cref="FlatList{T}"/> from a list.</summary>
    /// <param name="list">The input list.</param>
    /// <returns>A <see cref="FlatList{T}"/> containing the elements from the list.</returns>
    let ofList (list : 'T list) : FlatList<'T> =
        let builder = FlatListFactory.CreateBuilder ()
        list |> List.iter (fun item -> builder.Add item)
        moveFromBuilder builder

    /// <summary>Return a new array with the item at a given index set to the new value.</summary>
    /// <param name="index">The index of the item to be replaced.</param>
    /// <param name="value">The new value.</param>
    /// <param name="list">The input array.</param>
    /// <returns>The result array.</returns>
    /// <exception cref="System.ArgumentException">Thrown when index is outside 0..source.Length - 1</exception>
    let updateAt (index : int) (value : 'T) (list : FlatList<'T>) : FlatList<'T> = list.SetItem (index, value)

    /// <summary>Return a new array with the item at a given index removed.</summary>
    /// <param name="index">The index of the item to be removed.</param>
    /// <param name="list">The input array.</param>
    /// <returns>The result array.</returns>
    /// <exception cref="System.ArgumentException">Thrown when index is outside 0..source.Length - 1</exception>
    let removeAt (index : int) (list : FlatList<'T>) : FlatList<'T> = list.RemoveAt (index)

    /// <summary>Return a new array with a new item inserted before the given index.</summary>
    /// <param name="index">The index where the item should be inserted.</param>
    /// <param name="value">The value to insert.</param>
    /// <param name="list">The input array.</param>
    /// <returns>The result array.</returns>
    /// <exception cref="System.ArgumentException">Thrown when index is below 0 or greater than source.Length.</exception>
    let insertAt (index : int) (value : 'T) (list : FlatList<'T>) : FlatList<'T> = list.Insert (index, value)

    /// <summary>Return a new array with new items inserted before the given index.</summary>
    /// <param name="index">The index where the items should be inserted.</param>
    /// <param name="values">The values to insert.</param>
    /// <param name="list">The input array.</param>
    /// <returns>The result array.</returns>
    /// <exception cref="System.ArgumentException">Thrown when index is below 0 or greater than source.Length.</exception>
    let insertManyAt (index : int) (values : 'T seq) (list : FlatList<'T>) : FlatList<'T> = list.InsertRange (index, values)

    /// <summary>Builds a new array that contains the elements of the given array.</summary>
    /// <param name="list">The input array.</param>
    /// <returns>A copy of the input array.</returns>
    let copy (list : FlatList<'T>) : FlatList<'T> = list.ToImmutableArray ()

    /// <summary>Returns an array that contains no more than N elements in a new array.</summary>
    /// <param name="generator">A function that takes in the current state and returns an option tuple of the next
    /// element of the array and the next state value.</param>
    /// <param name="state">The initial state value.</param>
    /// <returns>The result array.</returns>
    let unfold<'T, 'State> (generator : 'State -> ('T * 'State) voption) (state : 'State) : FlatList<'T> =
        let builder = builder ()
        let rec loop state =
            match generator state with
            | ValueSome (item, newState) ->
                builder.Add item
                loop newState
            | ValueNone -> ()

        loop state
        moveFromBuilder builder

    /// <summary>Compares two arrays using the given comparison function, element by element.</summary>
    /// <param name="comparer">A function that takes an element from each array and returns an int.
    /// If it evaluates to a non-zero value iteration is stopped and that value is returned.</param>
    /// <param name="array1">The first input array.</param>
    /// <param name="array2">The second input array.</param>
    /// <returns>Returns the first non-zero result from the comparison function. If the first array has
    /// a larger element, the return value is always positive. If the second array has a larger
    /// element, the return value is always negative. When the elements are equal in the two
    /// arrays, 1 is returned if the first array is longer, 0 is returned if they are equal in
    /// length, and -1 is returned when the second array is longer.</returns>
    let inline compareWith (comparer : 'T -> 'T -> int) (array1 : FlatList<'T>) (array2 : FlatList<'T>) : int =
        let mutable result = 0
        let mutable i = 0
        let len1 = array1.Length
        let len2 = array2.Length
        let minLength = min len1 len2

        while i < minLength && result = 0 do
            result <- comparer array1.[i] array2.[i]
            i <- i + 1

        if result <> 0 then result
        elif len1 > len2 then 1
        elif len1 < len2 then -1
        else 0

    /// <summary>Returns the greatest of all elements of the array, compared via Operators.max.</summary>
    /// <param name="list">The input array.</param>
    /// <exception cref="System.ArgumentException">Thrown when the array is empty.</exception>
    /// <returns>The maximum element.</returns>
    let inline max (list : FlatList<'T>) : 'T when 'T : comparison =
        checkNotDefault (nameof list) list
        let mutable acc = list.[0]

        for i = 1 to list.Length - 1 do
            let curr = list.[i]

            if curr > acc then
                acc <- curr

        acc

    /// <summary>Returns the greatest of all elements of the array, compared via Operators.max on the function result.</summary>
    /// <param name="projection">The function to transform the elements into a type supporting comparison.</param>
    /// <param name="list">The input array.</param>
    /// <exception cref="System.ArgumentException">Thrown when the array is empty.</exception>
    /// <returns>The maximum element.</returns>
    let inline maxBy (projection : 'T -> 'Key) (list : FlatList<'T>) : 'T when 'Key : comparison =
        checkNotDefault (nameof list) list
        let mutable maxVal = list.[0]
        let mutable maxKey = projection maxVal
        for i = 1 to list.Length - 1 do
            let currVal = list.[i]
            let currKey = projection currVal
            if currKey > maxKey then
                maxVal <- currVal
                maxKey <- currKey
        maxVal

    /// <summary>Returns the smallest of all elements of the array, compared via Operators.min.</summary>
    /// <param name="list">The input array.</param>
    /// <exception cref="System.ArgumentException">Thrown when the array is empty.</exception>
    /// <returns>The minimum element.</returns>
    let inline min (list : FlatList<'T>) : 'T when 'T : comparison =
        checkNotDefault (nameof list) list
        let mutable acc = list.[0]

        for i = 1 to list.Length - 1 do
            let curr = list.[i]

            if curr < acc then
                acc <- curr

        acc

    /// <summary>Returns the smallest of all elements of the array, compared via Operators.min on the function result.</summary>
    /// <param name="projection">The function to transform the elements into a type supporting comparison.</param>
    /// <param name="list">The input array.</param>
    /// <exception cref="System.ArgumentException">Thrown when the array is empty.</exception>
    /// <returns>The minimum element.</returns>
    let inline minBy (projection : 'T -> 'Key) (list : FlatList<'T>) : 'T when 'Key : comparison =
        checkNotDefault (nameof list) list
        let mutable minVal = list.[0]
        let mutable minKey = projection minVal
        for i = 1 to list.Length - 1 do
            let currVal = list.[i]
            let currKey = projection currVal
            if currKey < minKey then
                minVal <- currVal
                minKey <- currKey
        minVal
