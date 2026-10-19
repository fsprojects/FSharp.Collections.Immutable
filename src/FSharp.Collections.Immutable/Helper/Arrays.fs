/// <summary>
/// Algorithms over arrays that the default modules and the modules of the
/// <see cref="N:FSharp.Collections.Immutable.Tuple"/> namespace share (ADR 0001, D3).
/// </summary>
/// <remarks>
/// <para>
/// The functions of the FSharp.Core <see cref="T:Microsoft.FSharp.Collections.ArrayModule"/> that build or take pairs and
/// triples work with reference tuples. The default functions of <see cref="T:FSharp.Collections.Immutable.FlatList"/> return
/// struct tuples, and the functions of the <see cref="T:FSharp.Collections.Immutable.Tuple.FlatList"/> module return
/// reference tuples. Calling the function of FSharp.Core and converting its result would allocate every reference tuple
/// and a second array. Each algorithm here takes the constructor of the tuples that it produces, so that neither variant
/// pays for a second pass.
/// </para>
/// <para>
/// Each algorithm has exactly two callers: the function of <see cref="T:FSharp.Collections.Immutable.FlatList"/> with
/// <see cref="M:FSharp.Collections.Immutable.ImmutableCollectionUtil.structPair``2(``0,``1)"/> or
/// <see cref="M:FSharp.Collections.Immutable.ImmutableCollectionUtil.structTriple``3(``0,``1,``2)"/>, and the function of
/// <see cref="T:FSharp.Collections.Immutable.Tuple.FlatList"/> with
/// <see cref="M:FSharp.Collections.Immutable.ImmutableCollectionUtil.referencePair``2(``0,``1)"/> or
/// <see cref="M:FSharp.Collections.Immutable.ImmutableCollectionUtil.referenceTriple``3(``0,``1,``2)"/>, so that it exists
/// once. The callers check their arguments first, with the exceptions of the FSharp.Core function that they mirror, so the
/// algorithms check nothing and document no exceptions.
/// </para>
/// <para>
/// For 10 000 elements,
/// <see cref="M:FSharp.Collections.Immutable.Arrays.zip``3(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpFunc{``1,``2}},``0[],``1[])"/>
/// with struct tuples allocates 80 KB, where the function of FSharp.Core followed by a conversion allocates 400 KB.
/// </para>
/// <para>
/// The summary of each function is the summary of the function of the FSharp.Core
/// <see cref="T:Microsoft.FSharp.Collections.ArrayModule"/> word for word (FSharp.Core 10.1), and so are the descriptions
/// of the parameters and of the result where the signatures agree. The remarks say what differs and why the function
/// exists.
/// </para>
/// </remarks>
module internal FSharp.Collections.Immutable.Arrays

open System
open System.Collections.Generic
open System.Linq

/// <summary>
/// Returns a new array that contains all pairings of elements from the first and second arrays.
/// </summary>
/// <param name="tuple">The function that builds an element of the result from an element of each array.</param>
/// <param name="array1">The first input array.</param>
/// <param name="array2">The second input array.</param>
/// <returns>The resulting array of pairs.</returns>
/// <remarks>
/// <para>
/// This is an O(n*m) operation, where n and m are the lengths of the arrays. The pairs come in the order of the first array
/// and, inside it, of the second.
/// </para>
/// <para>
/// It exists because
/// <see cref="M:Microsoft.FSharp.Collections.ArrayModule.AllPairs``2(``0[],``1[])"/>
/// returns reference tuples. The function of <see cref="T:FSharp.Collections.Immutable.FlatList"/> builds struct tuples
/// with it directly (720 KB for 300 by 300 elements, where the function of FSharp.Core followed by a conversion allocates
/// 3.6 MB).
/// </para>
/// </remarks>
/// <seealso cref="M:FSharp.Collections.Immutable.FlatList.AllPairs``2(System.Collections.Immutable.ImmutableArray{``0},System.Collections.Immutable.ImmutableArray{``1})"/>
/// <seealso cref="M:FSharp.Collections.Immutable.Tuple.FlatList.AllPairs``2(System.Collections.Immutable.ImmutableArray{``0},System.Collections.Immutable.ImmutableArray{``1})"/>
let inline allPairs ([<InlineIfLambda>] tuple : 'T1 -> 'T2 -> 'R) (array1 : 'T1 array) (array2 : 'T2 array) : 'R array =
    let length2 = array2.Length
    let result = Array.zeroCreate (array1.Length * length2)

    for i = 0 to array1.Length - 1 do
        for j = 0 to length2 - 1 do
            result[i * length2 + j] <- tuple array1[i] array2[j]

    result

/// <summary>
/// A key that may be <see langword="null"/>, for a dictionary, which rejects <see langword="null"/> keys.
/// </summary>
/// <remarks>
/// <para>
/// FSharp.Core has no such type in its public API.
/// </para>
/// <para>
/// It exists because
/// <see cref="M:Microsoft.FSharp.Collections.ArrayModule.CountBy``2(Microsoft.FSharp.Core.FSharpFunc{``0,``1},``0[])"/> and
/// <see cref="M:Microsoft.FSharp.Collections.ArrayModule.GroupBy``2(Microsoft.FSharp.Core.FSharpFunc{``0,``1},``0[])"/>
/// accept a <see langword="null"/> key (<c>[| null; "a"; null |]</c> has two keys), and
/// <see cref="T:System.Collections.Generic.Dictionary`2"/> throws <see cref="T:System.ArgumentNullException"/> for one.
/// </para>
/// </remarks>
[<Struct>]
type NullableKey<'Key> = {
    /// <summary>The key, which may be <see langword="null"/>.</summary>
    Key : 'Key
}

/// <summary>
/// Compares wrapped keys in F# structural equality, which handles <see langword="null"/> keys, as FSharp.Core does for its
/// keys.
/// </summary>
/// <returns>The comparer of the wrapped keys, which a <see cref="T:System.Collections.Generic.Dictionary`2"/> takes.</returns>
/// <remarks>
/// <para>
/// FSharp.Core has no such function in its public API.
/// </para>
/// <para>
/// It exists because the default comparer of a <see cref="T:System.Collections.Generic.Dictionary`2"/> compares array keys
/// by reference, where
/// <see cref="M:Microsoft.FSharp.Collections.ArrayModule.CountBy``2(Microsoft.FSharp.Core.FSharpFunc{``0,``1},``0[])"/>
/// finds <c>[| 1 |]</c> and <c>[| 1 |]</c> equal.
/// </para>
/// </remarks>
let nullableKeyComparer<'Key when 'Key : equality> () =
    let structural = HashIdentity.Structural<'Key>

    {
        new IEqualityComparer<NullableKey<'Key>> with
            member _.Equals (left, right) = structural.Equals (left.Key, right.Key)
            member _.GetHashCode key = structural.GetHashCode key.Key
    }

/// <summary>
/// Applies a key-generating function to each element of an array and returns an array yielding unique keys and their
/// number of occurrences in the original array.
/// </summary>
/// <param name="tuple">The function that builds an element of the result from a key and its number of occurrences.</param>
/// <param name="projection">
/// A function transforming each item of the input array into a key to be compared against the others.
/// </param>
/// <param name="array">The input array.</param>
/// <returns>The result array.</returns>
/// <remarks>
/// <para>
/// This is an O(n) operation, where n is the length of the array. The unique keys come in the order of their first
/// occurrence. The counting keeps one counter per key, not the elements, so it needs memory for the keys only.
/// </para>
/// <para>
/// It exists because
/// <see cref="M:Microsoft.FSharp.Collections.ArrayModule.CountBy``2(Microsoft.FSharp.Core.FSharpFunc{``0,``1},``0[])"/>
/// returns reference tuples. <see langword="null"/> keys and keys that are arrays work as in FSharp.Core (see
/// <see cref="T:FSharp.Collections.Immutable.Arrays.NullableKey`1"/>).
/// </para>
/// </remarks>
/// <seealso cref="M:FSharp.Collections.Immutable.FlatList.CountBy``2(Microsoft.FSharp.Core.FSharpFunc{``0,``1},System.Collections.Immutable.ImmutableArray{``0})"/>
/// <seealso cref="M:FSharp.Collections.Immutable.Tuple.FlatList.CountBy``2(Microsoft.FSharp.Core.FSharpFunc{``0,``1},System.Collections.Immutable.ImmutableArray{``0})"/>
let inline countBy
    ([<InlineIfLambda>] tuple : 'Key -> int -> 'R)
    (projection : 'T -> 'Key)
    (array : 'T array)
    : 'R array when 'Key : equality =
    let indices = Dictionary<NullableKey<'Key>, int>(nullableKeyComparer<'Key>())
    let keys = ResizeArray<'Key>()
    let counts = ResizeArray<int>()

    for item in array do
        let key = projection item
        let mutable index = 0

        if indices.TryGetValue ({ Key = key }, &index) then
            counts[index] <- counts[index] + 1
        else
            indices.Add ({ Key = key }, keys.Count)
            keys.Add key
            counts.Add 1

    let result = Array.zeroCreate keys.Count

    for index = 0 to result.Length - 1 do
        result[index] <- tuple keys[index] counts[index]

    result

/// <summary>
/// Applies a key-generating function to each element of an array and yields an array of unique keys. Each unique key
/// contains an array of all elements that match to this key.
/// </summary>
/// <param name="tuple">The function that builds an element of the result from a key and the array of its elements.</param>
/// <param name="projection">A function that transforms an element of the array into a comparable key.</param>
/// <param name="array">The input array.</param>
/// <returns>The result array.</returns>
/// <remarks>
/// <para>
/// This is an O(n) operation, where n is the length of the array. The keys come in the order of their first occurrence,
/// and the elements of a key in their order.
/// </para>
/// <para>
/// It exists because
/// <see cref="M:Microsoft.FSharp.Collections.ArrayModule.GroupBy``2(Microsoft.FSharp.Core.FSharpFunc{``0,``1},``0[])"/>
/// returns reference tuples. It groups once with
/// <see cref="M:System.Linq.Enumerable.ToLookup``2(System.Collections.Generic.IEnumerable{``0},System.Func{``0,``1},System.Collections.Generic.IEqualityComparer{``1})"/>,
/// as the rules of the project ask: the lookup holds the groups, where
/// <see cref="M:Microsoft.FSharp.Collections.SeqModule.GroupBy``2(Microsoft.FSharp.Core.FSharpFunc{``0,``1},System.Collections.Generic.IEnumerable{``0})"/>
/// groups again at every enumeration and allocates a tuple per group. The lookup accepts a <see langword="null"/> key, and it compares the keys in F#
/// structural equality, as FSharp.Core does.
/// </para>
/// </remarks>
/// <seealso cref="M:FSharp.Collections.Immutable.FlatList.GroupBy``2(Microsoft.FSharp.Core.FSharpFunc{``0,``1},System.Collections.Immutable.ImmutableArray{``0})"/>
/// <seealso cref="M:FSharp.Collections.Immutable.Tuple.FlatList.GroupBy``2(Microsoft.FSharp.Core.FSharpFunc{``0,``1},System.Collections.Immutable.ImmutableArray{``0})"/>
let inline groupBy
    ([<InlineIfLambda>] tuple : 'Key -> 'T array -> 'R)
    (projection : 'T -> 'Key)
    (array : 'T array)
    : 'R array when 'Key : equality =
    let lookup = array.ToLookup ((fun item -> projection item), HashIdentity.Structural<'Key>)
    let result = Array.zeroCreate lookup.Count
    let mutable index = 0

    for grouping in lookup do
        result[index] <- tuple grouping.Key (grouping.ToArray ())
        index <- index + 1

    result

/// <summary>
/// Builds a new array whose elements are the corresponding elements of the input array paired with the integer index
/// (from 0) of each element.
/// </summary>
/// <param name="tuple">The function that builds an element of the result from an index and the element at it.</param>
/// <param name="array">The input array.</param>
/// <returns>The array of indexed elements.</returns>
/// <remarks>
/// <para>
/// This is an O(n) operation, where n is the length of the array.
/// </para>
/// <para>
/// It exists because
/// <see cref="M:Microsoft.FSharp.Collections.ArrayModule.Indexed``1(``0[])"/>
/// returns reference tuples (320 KB for 10 000 elements, where struct tuples take 80 KB).
/// </para>
/// </remarks>
/// <seealso cref="M:FSharp.Collections.Immutable.FlatList.Indexed``1(System.Collections.Immutable.ImmutableArray{``0})"/>
/// <seealso cref="M:FSharp.Collections.Immutable.Tuple.FlatList.Indexed``1(System.Collections.Immutable.ImmutableArray{``0})"/>
let inline indexed ([<InlineIfLambda>] tuple : int -> 'T -> 'R) (array : 'T array) : 'R array =
    let result = Array.zeroCreate array.Length

    for i = 0 to result.Length - 1 do
        result[i] <- tuple i array[i]

    result

/// <summary>
/// Returns an array of each element in the input array paired with its predecessor, with the exception of the first
/// element which is only returned as the predecessor of the second element. The predecessor comes first in the returned
/// pairs.
/// </summary>
/// <param name="tuple">The function that builds an element of the result from a predecessor and its successor.</param>
/// <param name="array">The input array.</param>
/// <returns>The result array.</returns>
/// <remarks>
/// <para>
/// This is an O(n) operation, where n is the length of the array. An array of fewer than two elements gives the empty
/// array.
/// </para>
/// <para>
/// It exists because
/// <see cref="M:Microsoft.FSharp.Collections.ArrayModule.Pairwise``1(``0[])"/>
/// returns reference tuples.
/// </para>
/// </remarks>
/// <seealso cref="M:FSharp.Collections.Immutable.FlatList.Pairwise``1(System.Collections.Immutable.ImmutableArray{``0})"/>
/// <seealso cref="M:FSharp.Collections.Immutable.Tuple.FlatList.Pairwise``1(System.Collections.Immutable.ImmutableArray{``0})"/>
let inline pairwise ([<InlineIfLambda>] tuple : 'T -> 'T -> 'R) (array : 'T array) : 'R array =
    if array.Length < 2 then
        Array.Empty ()
    else
        let result = Array.zeroCreate (array.Length - 1)

        for i = 0 to result.Length - 1 do
            result[i] <- tuple array[i] array[i + 1]

        result

/// <summary>
/// Splits an array of pairs into two arrays.
/// </summary>
/// <param name="split">The function that reads a pair as a struct tuple.</param>
/// <param name="array">The input array.</param>
/// <returns>The two arrays, as a struct tuple.</returns>
/// <remarks>
/// <para>
/// This is an O(n) operation, where n is the length of the array.
/// </para>
/// <para>
/// It exists because
/// <see cref="M:Microsoft.FSharp.Collections.ArrayModule.Unzip``2(System.Tuple{``0,``1}[])"/>
/// takes reference tuples. The function of <see cref="T:FSharp.Collections.Immutable.FlatList"/> has struct tuples and
/// passes <see cref="M:Microsoft.FSharp.Core.Operators.Identity``1(``0)"/> as <paramref name="split"/>. The function of
/// <see cref="T:FSharp.Collections.Immutable.Tuple.FlatList"/> has reference tuples and converts one pair at a time, so
/// that no array of converted pairs (400 KB for 10 000 pairs) is built.
/// </para>
/// </remarks>
/// <seealso cref="M:FSharp.Collections.Immutable.FlatList.Unzip``2(System.Collections.Immutable.ImmutableArray{System.ValueTuple{``0,``1}})"/>
/// <seealso cref="M:FSharp.Collections.Immutable.Tuple.FlatList.Unzip``2(System.Collections.Immutable.ImmutableArray{System.Tuple{``0,``1}})"/>
let inline unzip ([<InlineIfLambda>] split : 'P -> struct ('T1 * 'T2)) (array : 'P array) =
    let result1 = Array.zeroCreate array.Length
    let result2 = Array.zeroCreate array.Length

    for i = 0 to array.Length - 1 do
        let struct (first, second) = split array[i]
        result1[i] <- first
        result2[i] <- second

    struct (result1, result2)

/// <summary>
/// Splits an array of triples into three arrays.
/// </summary>
/// <param name="split">The function that reads a triple as a struct tuple.</param>
/// <param name="array">The input array.</param>
/// <returns>The struct tuple of three arrays.</returns>
/// <remarks>
/// <para>
/// This is an O(n) operation, where n is the length of the array.
/// </para>
/// <para>
/// It exists for the reason of
/// <see cref="M:FSharp.Collections.Immutable.Arrays.unzip``3(Microsoft.FSharp.Core.FSharpFunc{``0,System.ValueTuple{``1,``2}},``0[])"/>:
/// <see cref="M:Microsoft.FSharp.Collections.ArrayModule.Unzip3``3(System.Tuple{``0,``1,``2}[])"/>
/// takes reference tuples.
/// </para>
/// </remarks>
/// <seealso cref="M:FSharp.Collections.Immutable.FlatList.Unzip3``3(System.Collections.Immutable.ImmutableArray{System.ValueTuple{``0,``1,``2}})"/>
/// <seealso cref="M:FSharp.Collections.Immutable.Tuple.FlatList.Unzip3``3(System.Collections.Immutable.ImmutableArray{System.Tuple{``0,``1,``2}})"/>
let inline unzip3 ([<InlineIfLambda>] split : 'P -> struct ('T1 * 'T2 * 'T3)) (array : 'P array) =
    let result1 = Array.zeroCreate array.Length
    let result2 = Array.zeroCreate array.Length
    let result3 = Array.zeroCreate array.Length

    for i = 0 to array.Length - 1 do
        let struct (first, second, third) = split array[i]
        result1[i] <- first
        result2[i] <- second
        result3[i] <- third

    struct (result1, result2, result3)

/// <summary>
/// Combines the two arrays into an array of pairs. The two arrays must have equal lengths, otherwise an
/// <see cref="T:System.ArgumentException"/> is raised.
/// </summary>
/// <param name="tuple">The function that builds an element of the result from the elements at the same index.</param>
/// <param name="array1">The first input array.</param>
/// <param name="array2">The second input array.</param>
/// <returns>The array of tupled elements.</returns>
/// <remarks>
/// <para>
/// This is an O(n) operation, where n is the length of the arrays. The callers check the lengths first and raise the
/// exception of
/// <see cref="M:Microsoft.FSharp.Collections.ArrayModule.Zip``2(``0[],``1[])"/>.
/// </para>
/// <para>
/// It exists because that function returns reference tuples (400 KB for 10 000 elements with the conversion, 80 KB here).
/// </para>
/// </remarks>
/// <seealso cref="M:FSharp.Collections.Immutable.FlatList.Zip``2(System.Collections.Immutable.ImmutableArray{``0},System.Collections.Immutable.ImmutableArray{``1})"/>
/// <seealso cref="M:FSharp.Collections.Immutable.Tuple.FlatList.Zip``2(System.Collections.Immutable.ImmutableArray{``0},System.Collections.Immutable.ImmutableArray{``1})"/>
let inline zip ([<InlineIfLambda>] tuple : 'T1 -> 'T2 -> 'R) (array1 : 'T1 array) (array2 : 'T2 array) : 'R array =
    let result = Array.zeroCreate array1.Length

    for i = 0 to result.Length - 1 do
        result[i] <- tuple array1[i] array2[i]

    result

/// <summary>
/// Combines three arrays into an array of triples. The three arrays must have equal lengths, otherwise an
/// <see cref="T:System.ArgumentException"/> is raised.
/// </summary>
/// <param name="tuple">The function that builds an element of the result from the elements at the same index.</param>
/// <param name="array1">The first input array.</param>
/// <param name="array2">The second input array.</param>
/// <param name="array3">The third input array.</param>
/// <returns>The array of tupled elements.</returns>
/// <remarks>
/// <para>
/// This is an O(n) operation, where n is the length of the arrays. The callers check the lengths first.
/// </para>
/// <para>
/// It exists for the reason of
/// <see cref="M:FSharp.Collections.Immutable.Arrays.zip``3(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpFunc{``1,``2}},``0[],``1[])"/>:
/// <see cref="M:Microsoft.FSharp.Collections.ArrayModule.Zip3``3(``0[],``1[],``2[])"/>
/// returns reference tuples.
/// </para>
/// <para>
/// The summary of that function says "an array of pairs", which is a slip: it returns triples.
/// </para>
/// </remarks>
/// <seealso cref="M:FSharp.Collections.Immutable.FlatList.Zip3``3(System.Collections.Immutable.ImmutableArray{``0},System.Collections.Immutable.ImmutableArray{``1},System.Collections.Immutable.ImmutableArray{``2})"/>
/// <seealso cref="M:FSharp.Collections.Immutable.Tuple.FlatList.Zip3``3(System.Collections.Immutable.ImmutableArray{``0},System.Collections.Immutable.ImmutableArray{``1},System.Collections.Immutable.ImmutableArray{``2})"/>
let inline zip3
    ([<InlineIfLambda>] tuple : 'T1 -> 'T2 -> 'T3 -> 'R)
    (array1 : 'T1 array)
    (array2 : 'T2 array)
    (array3 : 'T3 array)
    : 'R array =
    let result = Array.zeroCreate array1.Length

    for i = 0 to result.Length - 1 do
        result[i] <- tuple array1[i] array2[i] array3[i]

    result
