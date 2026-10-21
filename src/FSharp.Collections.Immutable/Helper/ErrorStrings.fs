/// <summary>
/// The exception messages of FSharp.Core (<c>FSCore.resx</c>) and of the .NET libraries, in English.
/// </summary>
/// <remarks>
/// The modules throw what the FSharp.Core function that they mirror throws (ADR 0001, D6). FSharp.Core keeps its resources
/// internal, so the texts are copied. The texts of the .NET libraries are for the functions that report a BCL exception
/// under the name of their own parameter. Each constant is the text of one resource and its comment names the resource,
/// so a text that no resource of FSharp.Core or of the .NET libraries holds does not belong here.
/// </remarks>
module internal FSharp.Collections.Immutable.ErrorStrings

/// FSCore.resx, arraysHadDifferentLengths.
[<Literal>]
let ArraysHadDifferentLengths = "The arrays have different lengths."

/// FSCore.resx, indexOutOfBounds.
[<Literal>]
let IndexOutOfBounds = "The index was outside the range of elements in the list."

/// FSCore.resx, arrayWasEmpty.
[<Literal>]
let InputArrayEmpty = "The input array was empty."

/// FSCore.resx, inputListWasEmpty.
[<Literal>]
let InputListWasEmpty = "The input list was empty."

/// FSCore.resx, inputMustBeNonNegative.
[<Literal>]
let InputMustBeNonNegative = "The input must be non-negative."

/// FSCore.resx, inputMustBePositive.
[<Literal>]
let InputMustBePositive = "The input must be positive."

/// FSCore.resx, inputSequenceEmpty.
[<Literal>]
let InputSequenceEmpty = "The input sequence was empty."

/// FSCore.resx, inputSequenceTooLong.
[<Literal>]
let InputSequenceTooLong = "The input sequence contains more than one element."

/// FSCore.resx, keyNotFound.
[<Literal>]
let KeyNotFound = "The item, key, or index was not found in the collection."

/// FSCore.resx, keyNotFoundAlt.
[<Literal>]
let KeyNotFoundAlt = "An index satisfying the predicate was not found in the collection."

/// FSCore.resx, listsHadDifferentLengths.
[<Literal>]
let ListsHadDifferentLengths = "The lists had different lengths."

/// System.Private.CoreLib, ArgumentOutOfRange_NeedNonNegNum: the message of the .NET libraries for a negative argument, as
/// `Array.BinarySearch` reports it.
[<Literal>]
let NeedNonNegativeNumber = "Non-negative number required."

/// FSCore.resx, notAPermutation.
[<Literal>]
let NotAPermutation = "The function did not compute a permutation."

/// FSCore.resx, notEnoughElements.
[<Literal>]
let NotEnoughElements = "The input sequence has an insufficient number of elements."

/// FSCore.resx, outOfRange.
[<Literal>]
let OutOfRange = "The index is outside the legal range."

/// FSCore.resx, setContainsNoElements.
[<Literal>]
let SetContainsNoElements = "Set contains no elements."
