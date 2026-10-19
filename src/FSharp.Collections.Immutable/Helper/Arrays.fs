/// Algorithms over arrays that the default modules and the `Tuple` modules share (ADR 0001, D3).
/// Each algorithm takes the constructor of the tuples that it produces, so that neither variant pays for a second pass.
/// The callers check their arguments first, with the exceptions of the FSharp.Core module that they mirror.
/// The algorithms exist because the functions of the FSharp.Core `Array` module that build or take pairs and triples work
/// with reference tuples. The default functions of `FlatList` return struct tuples, and the functions of the `Tuple`
/// namespace return reference tuples. Calling the `Array` function and converting its result would allocate every
/// reference tuple and a second array. Each algorithm has exactly two callers, the function of `FlatList` with
/// `structPair` or `structTriple` and the function of `Tuple.FlatList` with `referencePair` or `referenceTriple`, so
/// that it exists once. For 10 000 elements, `zip` of struct tuples allocates 80 KB where `Array.zip` followed by a
/// conversion allocates 400 KB.
/// The first sentences of the comment of each function are the summary of the function of the FSharp.Core `Array` module
/// word for word (FSharp.Core 10.1), and the sentences after them say what differs and why the function exists.
module internal FSharp.Collections.Immutable.Arrays

open System
open System.Collections.Generic
open System.Linq

/// Returns a new array that contains all pairings of elements from the first and second arrays.
/// The pairs are built with `tuple`. It exists because `Array.allPairs` returns reference tuples. `FlatList.allPairs`
/// builds struct tuples with it directly (720 KB for 300 by 300 elements, where `Array.allPairs` followed by a conversion
/// allocates 3.6 MB).
let inline allPairs ([<InlineIfLambda>] tuple : 'T1 -> 'T2 -> 'R) (array1 : 'T1 array) (array2 : 'T2 array) : 'R array =
    let length2 = array2.Length
    let result = Array.zeroCreate (array1.Length * length2)

    for i = 0 to array1.Length - 1 do
        for j = 0 to length2 - 1 do
            result[i * length2 + j] <- tuple array1[i] array2[j]

    result

/// A key that may be null, for a dictionary, which rejects null keys. FSharp.Core has no such type in its public API.
/// It exists because `Array.countBy` and `Array.groupBy` accept a null key (`[| null; "a"; null |]` has two keys), and
/// `Dictionary` throws `ArgumentNullException` for one.
[<Struct>]
type NullableKey<'Key> = { Key : 'Key }

/// Compares wrapped keys in F# structural equality, which handles null keys, as FSharp.Core does for its keys.
/// It exists because the default comparer of a `Dictionary` compares array keys by reference, where `Array.countBy`
/// finds `[| 1 |]` and `[| 1 |]` equal.
let nullableKeyComparer<'Key when 'Key : equality> () =
    let structural = HashIdentity.Structural<'Key>

    {
        new IEqualityComparer<NullableKey<'Key>> with
            member _.Equals (left, right) = structural.Equals (left.Key, right.Key)
            member _.GetHashCode key = structural.GetHashCode key.Key
    }

/// Applies a key-generating function to each element of an array and returns an array yielding unique keys and their
/// number of occurrences in the original array.
/// The unique keys come in the order of their first occurrence, and the counting keeps one counter per key, not the
/// elements, so it needs memory for the keys only. The tuples are built with `tuple`. It exists because `Array.countBy`
/// returns reference tuples; null keys and keys that are arrays work as in FSharp.Core.
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

/// Applies a key-generating function to each element of an array and yields an array of unique keys. Each unique key
/// contains an array of all elements that match to this key.
/// The keys come in the order of their first occurrence, and the elements of a key in their order. The tuples are built
/// with `tuple`. It exists because `Array.groupBy` returns reference tuples. It groups once with a lookup, as the project
/// rules ask.
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

/// Builds a new array whose elements are the corresponding elements of the input array paired with the integer index
/// (from 0) of each element.
/// The pairs are built with `tuple`. It exists because `Array.indexed` returns reference tuples (320 KB for 10 000
/// elements, where struct tuples take 80 KB).
let inline indexed ([<InlineIfLambda>] tuple : int -> 'T -> 'R) (array : 'T array) : 'R array =
    let result = Array.zeroCreate array.Length

    for i = 0 to result.Length - 1 do
        result[i] <- tuple i array[i]

    result

/// Returns an array of each element in the input array paired with its predecessor, with the exception of the first
/// element which is only returned as the predecessor of the second element. The predecessor comes first in the returned
/// pairs.
/// The pairs are built with `tuple`. It exists because `Array.pairwise` returns reference tuples.
let inline pairwise ([<InlineIfLambda>] tuple : 'T -> 'T -> 'R) (array : 'T array) : 'R array =
    if array.Length < 2 then
        Array.Empty ()
    else
        let result = Array.zeroCreate (array.Length - 1)

        for i = 0 to result.Length - 1 do
            result[i] <- tuple array[i] array[i + 1]

        result

/// Splits an array of pairs into two arrays.
/// `split` reads a pair as a struct tuple. It exists because `Array.unzip` takes reference tuples. `FlatList.unzip` has
/// struct tuples and passes `id` as `split`, `Tuple.FlatList.unzip` has reference tuples and converts one pair at a time,
/// so that no array of converted pairs (400 KB for 10 000 pairs) is built.
let inline unzip ([<InlineIfLambda>] split : 'P -> struct ('T1 * 'T2)) (array : 'P array) =
    let result1 = Array.zeroCreate array.Length
    let result2 = Array.zeroCreate array.Length

    for i = 0 to array.Length - 1 do
        let struct (first, second) = split array[i]
        result1[i] <- first
        result2[i] <- second

    struct (result1, result2)

/// Splits an array of triples into three arrays.
/// `split` reads a triple as a struct tuple. It exists for the reason of `unzip`: `Array.unzip3` takes reference tuples.
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

/// Combines the two arrays into an array of pairs. The two arrays must have equal lengths, otherwise an ArgumentException
/// is raised.
/// The pairs are built with `tuple`, and the callers check the lengths first and raise the exception of `Array.zip`. It
/// exists because `Array.zip` returns reference tuples (400 KB for 10 000 elements with the conversion, 80 KB here).
let inline zip ([<InlineIfLambda>] tuple : 'T1 -> 'T2 -> 'R) (array1 : 'T1 array) (array2 : 'T2 array) : 'R array =
    let result = Array.zeroCreate array1.Length

    for i = 0 to result.Length - 1 do
        result[i] <- tuple array1[i] array2[i]

    result

/// Combines three arrays into an array of triples. The three arrays must have equal lengths, otherwise an
/// ArgumentException is raised. (The summary of `Array.zip3` says "an array of pairs", which is a slip.)
/// The triples are built with `tuple`, and the callers check the lengths first. It exists for the reason of `zip`:
/// `Array.zip3` returns reference tuples.
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
