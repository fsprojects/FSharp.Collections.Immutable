/// <summary>
/// The exceptions of the <c>DetailedExceptions</c> module of FSharp.Core, with the same types, messages and parameter names.
/// </summary>
/// <remarks>
/// The callers pass the names of their own parameters (ADR 0001, D6).
/// </remarks>
module internal FSharp.Collections.Immutable.Raise

open System
open System.Collections.Generic

/// The arrays of a function of two arrays have different lengths.
let invalidArgDifferentArrayLength (arg1 : string) (len1 : int) (arg2 : string) (len2 : int) : 'T =
    let message =
        $"{ErrorStrings.ArraysHadDifferentLengths}\n{arg1}.Length = {len1}, {arg2}.Length = {len2}"

    raise (ArgumentException (message, arg1))

/// The lists of a function of two lists have different lengths; `diff` is how much shorter `arg1` is.
let invalidArgDifferentListLength (arg1 : string) (arg2 : string) (diff : int) : 'T =
    let elements = if diff = 1 then "element" else "elements"

    let message =
        $"{ErrorStrings.ListsHadDifferentLengths}\n{arg1} is {diff} {elements} shorter than {arg2}"

    raise (ArgumentException (message, arg1))

/// The arrays of a function of three arrays have different lengths.
let invalidArg3ArraysDifferent (arg1 : string) (arg2 : string) (arg3 : string) (len1 : int) (len2 : int) (len3 : int) : 'T =
    let message =
        $"{ErrorStrings.ArraysHadDifferentLengths}\n {arg1}.Length = {len1}, {arg2}.Length = {len2}, {arg3}.Length = {len3}"

    raise (ArgumentException (message, $"{arg1}, {arg2}, {arg3}"))

/// The lists of a function of three lists have different lengths.
let invalidArg3ListsDifferent (arg1 : string) (arg2 : string) (arg3 : string) (len1 : int) (len2 : int) (len3 : int) : 'T =
    let message =
        $"{ErrorStrings.ListsHadDifferentLengths}\n {arg1}.Length = {len1}, {arg2}.Length = {len2}, {arg3}.Length = {len3}"

    raise (ArgumentException (message, $"{arg1}, {arg2}, {arg3}"))

/// A count or an index is negative.
let invalidArgInputMustBeNonNegative (arg : string) (count : int) : 'T =
    raise (ArgumentException ($"{ErrorStrings.InputMustBeNonNegative}\n{arg} = {count}", arg))

/// A size is not positive.
let invalidArgInputMustBePositive (arg : string) (count : int) : 'T =
    raise (ArgumentException ($"{ErrorStrings.InputMustBePositive}\n{arg} = {count}", arg))

/// An index or a count is beyond a bound.
let invalidArgOutOfRange (arg : string) (index : int) (text : string) (bound : int) : 'T =
    let message = $"{ErrorStrings.OutOfRange}\n{arg} = {index}, {text} = {bound}"

    raise (ArgumentOutOfRangeException (arg, message))

/// A list is shorter than an index.
let invalidOpListNotEnoughElements (index : int) : 'T =
    let elements = if index = 1 then "element" else "elements"

    let message =
        $"{ErrorStrings.NotEnoughElements}\nThe list was {index} {elements} shorter than the index"

    raise (InvalidOperationException message)

/// No element satisfies the predicate of a `find` or `pick` function.
let indexNotFound () : 'T = raise (KeyNotFoundException ErrorStrings.KeyNotFoundAlt)

/// A collection has fewer elements than a function needs.
let notEnoughElements () : 'T = raise (InvalidOperationException ErrorStrings.NotEnoughElements)
