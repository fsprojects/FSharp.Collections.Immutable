/// <summary>
/// Algorithms over arrays that the default modules and the <c>Tuple</c> modules share (ADR 0001, D3).
/// </summary>
/// <remarks>
/// Each algorithm takes the constructor of the tuples that it produces, so that neither variant pays for a second pass.
/// The callers check their arguments first, with the exceptions of the FSharp.Core module that they mirror.
/// </remarks>
module internal FSharp.Collections.Immutable.Arrays

open System
open System.Collections.Generic
open System.Linq

/// The pairs of every element of `array1` with every element of `array2`.
let inline allPairs ([<InlineIfLambda>] tuple : 'T1 -> 'T2 -> 'R) (array1 : 'T1 array) (array2 : 'T2 array) : 'R array =
    let length2 = array2.Length
    let result = Array.zeroCreate (array1.Length * length2)

    for i = 0 to array1.Length - 1 do
        for j = 0 to length2 - 1 do
            result[i * length2 + j] <- tuple array1[i] array2[j]

    result

/// A key that may be null, for a dictionary, which rejects null keys.
[<Struct>]
type NullableKey<'Key> = { Key : 'Key }

/// Compares wrapped keys in F# structural equality, which handles null keys, as FSharp.Core does for its keys.
let nullableKeyComparer<'Key when 'Key : equality> () =
    let structural = HashIdentity.Structural<'Key>

    {
        new IEqualityComparer<NullableKey<'Key>> with
            member _.Equals (left, right) = structural.Equals (left.Key, right.Key)
            member _.GetHashCode key = structural.GetHashCode key.Key
    }

/// <summary>
/// The keys of the projection in the order of their first occurrence, each with the number of its elements.
/// </summary>
/// <remarks>
/// The counting keeps one counter per key, not the elements, so it needs memory for the keys only.
/// </remarks>
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

/// The keys of the projection in the order of their first occurrence, each with its elements in their order.
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

/// Every element with its index.
let inline indexed ([<InlineIfLambda>] tuple : int -> 'T -> 'R) (array : 'T array) : 'R array =
    let result = Array.zeroCreate array.Length

    for i = 0 to result.Length - 1 do
        result[i] <- tuple i array[i]

    result

/// Every element with its successor.
let inline pairwise ([<InlineIfLambda>] tuple : 'T -> 'T -> 'R) (array : 'T array) : 'R array =
    if array.Length < 2 then
        Array.Empty ()
    else
        let result = Array.zeroCreate (array.Length - 1)

        for i = 0 to result.Length - 1 do
            result[i] <- tuple array[i] array[i + 1]

        result

/// Splits the pairs of an array with `split` into two arrays.
let inline unzip ([<InlineIfLambda>] split : 'P -> struct ('T1 * 'T2)) (array : 'P array) =
    let result1 = Array.zeroCreate array.Length
    let result2 = Array.zeroCreate array.Length

    for i = 0 to array.Length - 1 do
        let struct (first, second) = split array[i]
        result1[i] <- first
        result2[i] <- second

    struct (result1, result2)

/// Splits the triples of an array with `split` into three arrays.
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

/// Pairs the elements of two arrays of the same length.
let inline zip ([<InlineIfLambda>] tuple : 'T1 -> 'T2 -> 'R) (array1 : 'T1 array) (array2 : 'T2 array) : 'R array =
    let result = Array.zeroCreate array1.Length

    for i = 0 to result.Length - 1 do
        result[i] <- tuple array1[i] array2[i]

    result

/// Combines the elements of three arrays of the same length.
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
