/// <summary>
/// The exception messages of FSharp.Core (<c>FSCore.resx</c>) and of the .NET libraries, in English.
/// </summary>
/// <remarks>
/// The modules throw what the FSharp.Core function that they mirror throws (ADR 0001, D6). FSharp.Core keeps its resources
/// internal, so the texts are copied. The texts of the .NET libraries are for the functions that report a BCL exception
/// under the name of their own parameter.
/// </remarks>
module internal FSharp.Collections.Immutable.ErrorStrings

[<Literal>]
let ArraysHadDifferentLengths = "The arrays have different lengths."

[<Literal>]
let IndexOutOfBounds = "The index was outside the range of elements in the list."

[<Literal>]
let InputArrayEmpty = "The input array was empty."

[<Literal>]
let InputListWasEmpty = "The input list was empty."

[<Literal>]
let InputMustBeNonNegative = "The input must be non-negative."

[<Literal>]
let InputMustBePositive = "The input must be positive."

[<Literal>]
let InputSequenceEmpty = "The input sequence was empty."

[<Literal>]
let InputSequenceTooLong = "The input sequence contains more than one element."

[<Literal>]
let KeyNotFound = "The item, key, or index was not found in the collection."

[<Literal>]
let KeyNotFoundAlt = "An index satisfying the predicate was not found in the collection."

[<Literal>]
let ListsHadDifferentLengths = "The lists had different lengths."

/// The message of the modules written before ADR 0001.
[<Literal>]
let ListsHaveDifferentLengths = "The lists have different lengths."

/// The message of the .NET libraries for a negative argument, as `Array.BinarySearch` reports it.
[<Literal>]
let NeedNonNegativeNumber = "Non-negative number required."

[<Literal>]
let NotAPermutation = "The function did not compute a permutation."

[<Literal>]
let NotEnoughElements = "The input sequence has an insufficient number of elements."

[<Literal>]
let OutOfRange = "The index is outside the legal range."
