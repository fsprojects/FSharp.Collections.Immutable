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

/// <summary>
/// Raises the exception for two lists that both have elements and differ in length, as the fallback of
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.FoldBack2``3(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpFunc{``1,Microsoft.FSharp.Core.FSharpFunc{``2,``2}}},Microsoft.FSharp.Collections.FSharpList{``0},Microsoft.FSharp.Collections.FSharpList{``1},``2)"/>
/// does.
/// </summary>
/// <param name="arg1">The name of the parameter of the first list.</param>
/// <param name="len1">The length of the first list.</param>
/// <param name="arg2">The name of the parameter of the second list.</param>
/// <param name="len2">The length of the second list.</param>
/// <returns>Never: the function always raises. The result is generic so that the call can end a branch of any type.</returns>
/// <exception cref="T:System.ArgumentException">
/// Always. The message is <see cref="P:FSharp.Collections.Immutable.ErrorStrings.ListsHadDifferentLengths"/>, a line break
/// and the lengths of both lists, and the parameter name is both names, separated by a comma.
/// </exception>
/// <remarks>
/// It is not <see cref="M:FSharp.Collections.Immutable.Raise.invalidArgDifferentListLength``1(System.String,System.String,System.Int32)"/>,
/// which names the shorter list and says how much shorter it is: the fallback of the function copies the lists into
/// arrays, compares their lengths and reports both.
/// </remarks>
let invalidArg2ListsDifferent (arg1 : string) (len1 : int) (arg2 : string) (len2 : int) : 'T =
    let message = $"{ErrorStrings.ListsHadDifferentLengths}\n{arg1}.Length = {len1}, {arg2}.Length = {len2}"

    raise (ArgumentException (message, $"{arg1}, {arg2}"))

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

/// <summary>
/// Raises the exception for a randomizer that returned a number outside [0, 1), which the functions of FSharp.Core with a
/// randomizer, such as
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.RandomChoiceBy``1(Microsoft.FSharp.Core.FSharpFunc{Microsoft.FSharp.Core.Unit,System.Double},Microsoft.FSharp.Collections.FSharpList{``0})"/>,
/// throw.
/// </summary>
/// <param name="arg">The name of the parameter of the randomizer.</param>
/// <param name="value">The number that the randomizer returned.</param>
/// <returns>Never: the function always raises. The result is generic so that the call can end a branch of any type.</returns>
/// <exception cref="T:System.ArgumentOutOfRangeException">
/// Always. The message is <see cref="P:FSharp.Collections.Immutable.ErrorStrings.OutOfRange"/>, a line break and the number
/// with the range that is allowed, and the parameter name is <paramref name="arg"/>.
/// </exception>
let invalidArgRandomizerOutOfRange (arg : string) (value : float) : 'T =
    let message = $"{ErrorStrings.OutOfRange}\n{arg} returned {value}, should be in range [0.0, 1.0)."

    raise (ArgumentOutOfRangeException (arg, message))

/// <summary>
/// Raises the exception for an empty list and an index beyond it, as
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.SplitAt``1(System.Int32,Microsoft.FSharp.Collections.FSharpList{``0})"/>
/// does.
/// </summary>
/// <returns>Never: the function always raises. The result is generic so that the call can end a branch of any type.</returns>
/// <exception cref="T:System.InvalidOperationException">
/// Always. The message is <see cref="P:FSharp.Collections.Immutable.ErrorStrings.InputListWasEmpty"/>.
/// </exception>
/// <remarks>
/// A longer list that is too short gets
/// <see cref="M:FSharp.Collections.Immutable.Raise.invalidOpListNotEnoughElements``1(System.Int32)"/> instead.
/// </remarks>
let invalidOpListEmpty () : 'T = raise (InvalidOperationException ErrorStrings.InputListWasEmpty)

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
