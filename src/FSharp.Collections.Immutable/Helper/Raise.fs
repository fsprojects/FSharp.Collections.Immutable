/// <summary>
/// The exceptions of the <c>DetailedExceptions</c> module of FSharp.Core, with the same types, messages and parameter names.
/// </summary>
/// <remarks>
/// The callers pass the names of their own parameters (ADR 0001, D6).
/// </remarks>
module internal FSharp.Collections.Immutable.Raise

open System
open System.Collections.Generic

let inline private invalidArgFormat (arg : string) (format : string) (paramArray : obj array) =
    raise (ArgumentException (String.Format (format, paramArray), arg))

/// The arrays of a function of two arrays have different lengths.
let invalidArgDifferentArrayLength (arg1 : string) (len1 : int) (arg2 : string) (len2 : int) : 'T =
    invalidArgFormat arg1 "{0}\n{1}.Length = {2}, {3}.Length = {4}" [|
        ErrorStrings.ArraysHadDifferentLengths
        arg1
        len1
        arg2
        len2
    |]

/// The lists of a function of two lists have different lengths; `diff` is how much shorter `arg1` is.
let invalidArgDifferentListLength (arg1 : string) (arg2 : string) (diff : int) : 'T =
    invalidArgFormat arg1 "{0}\n{1} is {2} {3} shorter than {4}" [|
        ErrorStrings.ListsHadDifferentLengths
        arg1
        diff
        (if diff = 1 then "element" else "elements")
        arg2
    |]

/// The arrays of a function of three arrays have different lengths.
let invalidArg3ArraysDifferent (arg1 : string) (arg2 : string) (arg3 : string) (len1 : int) (len2 : int) (len3 : int) : 'T =
    invalidArgFormat
        (String.Concat [| arg1; ", "; arg2; ", "; arg3 |])
        "{0}\n {1}.Length = {2}, {3}.Length = {4}, {5}.Length = {6}"
        [| ErrorStrings.ArraysHadDifferentLengths; arg1; len1; arg2; len2; arg3; len3 |]

/// The lists of a function of three lists have different lengths.
let invalidArg3ListsDifferent (arg1 : string) (arg2 : string) (arg3 : string) (len1 : int) (len2 : int) (len3 : int) : 'T =
    invalidArgFormat
        (String.Concat [| arg1; ", "; arg2; ", "; arg3 |])
        "{0}\n {1}.Length = {2}, {3}.Length = {4}, {5}.Length = {6}"
        [| ErrorStrings.ListsHadDifferentLengths; arg1; len1; arg2; len2; arg3; len3 |]

/// A count or an index is negative.
let invalidArgInputMustBeNonNegative (arg : string) (count : int) : 'T =
    invalidArgFormat arg "{0}\n{1} = {2}" [| ErrorStrings.InputMustBeNonNegative; arg; count |]

/// A size is not positive.
let invalidArgInputMustBePositive (arg : string) (count : int) : 'T =
    invalidArgFormat arg "{0}\n{1} = {2}" [| ErrorStrings.InputMustBePositive; arg; count |]

/// An index or a count is beyond a bound.
let invalidArgOutOfRange (arg : string) (index : int) (text : string) (bound : int) : 'T =
    let message =
        String.Format ("{0}\n{1} = {2}, {3} = {4}", [| box ErrorStrings.OutOfRange; arg; index; text; bound |])

    raise (ArgumentOutOfRangeException (arg, message))

/// A list is shorter than an index.
let invalidOpListNotEnoughElements (index : int) : 'T =
    let message =
        String.Format (
            "{0}\nThe list was {1} {2} shorter than the index",
            [| box ErrorStrings.NotEnoughElements; index; (if index = 1 then "element" else "elements") |]
        )

    raise (InvalidOperationException message)

/// No element satisfies the predicate of a `find` or `pick` function.
let indexNotFound () : 'T = raise (KeyNotFoundException ErrorStrings.KeyNotFoundAlt)

/// A collection has fewer elements than a function needs.
let notEnoughElements () : 'T = raise (InvalidOperationException ErrorStrings.NotEnoughElements)
