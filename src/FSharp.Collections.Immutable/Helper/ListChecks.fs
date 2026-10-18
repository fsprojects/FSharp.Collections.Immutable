/// <summary>
/// The argument checks of the functions of the FSharp.Core <see cref="T:Microsoft.FSharp.Collections.ListModule"/> module,
/// for the modules that mirror it: <see cref="T:FSharp.Collections.Immutable.IndexedList"/>,
/// <see cref="T:FSharp.Collections.Immutable.IIndexedList"/>, the modules of the queues and the stacks, and their interface
/// modules (ADR 0001, D6).
/// </summary>
/// <remarks>
/// <para>
/// A module that mirrors <see cref="T:Microsoft.FSharp.Collections.ListModule"/> throws what its functions throw for the
/// same input: the same type and the same message, with the name of the parameter of the mirroring function (ADR 0001, D6).
/// Each check takes the lengths or the indices to test and the names to report, and throws what the functions that it names
/// throw. The checks are here, once, so that no module spells the type, the message and the parameter name of every
/// exception again.
/// </para>
/// <para>
/// The documentation of each check gives the name that the FSharp.Core function reports for the parameter.
/// <see cref="T:FSharp.Collections.Immutable.IndexedList"/> and
/// <see cref="T:FSharp.Collections.Immutable.IIndexedList"/> pass those names; a module with other parameter names passes
/// its own.
/// </para>
/// <para>
/// The FSharp.Core functions of two or three lists compare the lengths when the shorter list ends, so a function with a
/// callback applies it to the common prefix before it calls the check of the lengths.
/// <see cref="T:FSharp.Collections.Immutable.ListArrays"/> does that over arrays.
/// </para>
/// </remarks>
module internal FSharp.Collections.Immutable.ListChecks

open System

/// <summary>
/// The message that
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.InsertAt``1(System.Int32,``0,Microsoft.FSharp.Collections.FSharpList{``0})"/>,
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.InsertManyAt``1(System.Int32,System.Collections.Generic.IEnumerable{``0},Microsoft.FSharp.Collections.FSharpList{``0})"/>,
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.RemoveAt``1(System.Int32,Microsoft.FSharp.Collections.FSharpList{``0})"/>,
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.UpdateAt``1(System.Int32,``0,Microsoft.FSharp.Collections.FSharpList{``0})"/> and
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.RemoveManyAt``1(System.Int32,System.Int32,Microsoft.FSharp.Collections.FSharpList{``0})"/> write as a literal.
/// </summary>
/// <remarks>
/// It is not a constant of <see cref="T:FSharp.Collections.Immutable.ErrorStrings"/>, whose every constant is the text of
/// one resource: no resource of FSharp.Core holds this text.
/// </remarks>
[<Literal>]
let private IndexOutsideListBounds = "index must be within bounds of the list"

/// <summary>
/// Rejects an empty list, as
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.Head``1(Microsoft.FSharp.Collections.FSharpList{``0})"/>,
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.Last``1(Microsoft.FSharp.Collections.FSharpList{``0})"/>,
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.Tail``1(Microsoft.FSharp.Collections.FSharpList{``0})"/>,
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.Reduce``1(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpFunc{``0,``0}},Microsoft.FSharp.Collections.FSharpList{``0})"/> and
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.ReduceBack``1(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpFunc{``0,``0}},Microsoft.FSharp.Collections.FSharpList{``0})"/> do.
/// </summary>
/// <param name="name">The name of the parameter to report: <c>"list"</c> for these functions.</param>
/// <param name="length">The length of the list.</param>
/// <exception cref="T:System.ArgumentException">
/// The length is 0. The message is <see cref="P:FSharp.Collections.Immutable.ErrorStrings.InputListWasEmpty"/>, and the
/// parameter name is <paramref name="name"/>.
/// </exception>
/// <remarks>
/// It is a check of its own, besides
/// <see cref="M:FSharp.Collections.Immutable.ListChecks.notEmptySequence(System.String,System.Int32)"/>, because the
/// functions of the two groups give an empty input different messages.
/// </remarks>
let notEmpty (name : string) (length : int) =
    if length = 0 then
        invalidArg name ErrorStrings.InputListWasEmpty

/// <summary>
/// Rejects an empty input with the message of an empty sequence, as
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.Max``1(Microsoft.FSharp.Collections.FSharpList{``0})"/>,
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.MaxBy``2(Microsoft.FSharp.Core.FSharpFunc{``0,``1},Microsoft.FSharp.Collections.FSharpList{``0})"/>,
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.Min``1(Microsoft.FSharp.Collections.FSharpList{``0})"/> and
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.MinBy``2(Microsoft.FSharp.Core.FSharpFunc{``0,``1},Microsoft.FSharp.Collections.FSharpList{``0})"/> (<c>"list"</c>) and
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.RandomChoice``1(Microsoft.FSharp.Collections.FSharpList{``0})"/>,
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.RandomChoiceBy``1(Microsoft.FSharp.Core.FSharpFunc{Microsoft.FSharp.Core.Unit,System.Double},Microsoft.FSharp.Collections.FSharpList{``0})"/> and
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.RandomChoiceWith``1(System.Random,Microsoft.FSharp.Collections.FSharpList{``0})"/> (<c>"source"</c>) do.
/// </summary>
/// <param name="name">The name of the parameter to report: <c>"list"</c> or <c>"source"</c>, as above.</param>
/// <param name="length">The length of the input.</param>
/// <exception cref="T:System.ArgumentException">
/// The length is 0. The message is <see cref="P:FSharp.Collections.Immutable.ErrorStrings.InputSequenceEmpty"/>, and the
/// parameter name is <paramref name="name"/>.
/// </exception>
/// <remarks>
/// It is a check of its own, besides
/// <see cref="M:FSharp.Collections.Immutable.ListChecks.notEmpty(System.String,System.Int32)"/>, because the messages
/// differ: these functions say "sequence" where the functions of the other check say "list".
/// </remarks>
let notEmptySequence (name : string) (length : int) =
    if length = 0 then
        invalidArg name ErrorStrings.InputSequenceEmpty

/// <summary>
/// Rejects an input without exactly one element, as
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.ExactlyOne``1(Microsoft.FSharp.Collections.FSharpList{``0})"/> does.
/// </summary>
/// <param name="name">The name of the parameter to report: <c>"source"</c> for this function.</param>
/// <param name="length">The length of the input.</param>
/// <exception cref="T:System.ArgumentException">
/// The length is not 1. The message is <see cref="P:FSharp.Collections.Immutable.ErrorStrings.InputSequenceEmpty"/> for no
/// element and <see cref="P:FSharp.Collections.Immutable.ErrorStrings.InputSequenceTooLong"/> for more than one, and the
/// parameter name is <paramref name="name"/>.
/// </exception>
let exactlyOne (name : string) (length : int) =
    if length = 0 then
        invalidArg name ErrorStrings.InputSequenceEmpty
    elif length > 1 then
        invalidArg name ErrorStrings.InputSequenceTooLong

/// <summary>
/// Rejects an index outside the list, as
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.Item``1(System.Int32,Microsoft.FSharp.Collections.FSharpList{``0})"/> does.
/// </summary>
/// <param name="name">The name of the parameter to report: <c>"index"</c> for this function.</param>
/// <param name="index">The index to test.</param>
/// <param name="length">The length of the list.</param>
/// <exception cref="T:System.ArgumentException">
/// The index is negative or not less than the length. The message is
/// <see cref="P:FSharp.Collections.Immutable.ErrorStrings.IndexOutOfBounds"/>, and the parameter name is
/// <paramref name="name"/>.
/// </exception>
/// <remarks>
/// It is a check of its own, besides
/// <see cref="M:FSharp.Collections.Immutable.ListChecks.removeAt(System.String,System.Int32,System.Int32)"/>, because the
/// message differs, although the indices that both reject are the same.
/// </remarks>
let item (name : string) (index : int) (length : int) =
    if index < 0 || index >= length then
        invalidArg name ErrorStrings.IndexOutOfBounds

/// <summary>
/// Rejects an index outside the range from 0 to the length, as
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.InsertAt``1(System.Int32,``0,Microsoft.FSharp.Collections.FSharpList{``0})"/> and
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.InsertManyAt``1(System.Int32,System.Collections.Generic.IEnumerable{``0},Microsoft.FSharp.Collections.FSharpList{``0})"/> do.
/// </summary>
/// <param name="name">The name of the parameter to report: <c>"index"</c> for these functions.</param>
/// <param name="index">The index to test.</param>
/// <param name="length">The length of the list.</param>
/// <exception cref="T:System.ArgumentException">
/// The index is negative or greater than the length. The message is
/// <see cref="P:FSharp.Collections.Immutable.ListChecks.IndexOutsideListBounds"/>, and the parameter name is
/// <paramref name="name"/>.
/// </exception>
/// <remarks>
/// The length itself is a valid index, because an element can be inserted after the last one.
/// </remarks>
let insertAt (name : string) (index : int) (length : int) =
    if index < 0 || index > length then
        invalidArg name IndexOutsideListBounds

/// <summary>
/// Rejects an index outside the list, as
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.RemoveAt``1(System.Int32,Microsoft.FSharp.Collections.FSharpList{``0})"/> and
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.UpdateAt``1(System.Int32,``0,Microsoft.FSharp.Collections.FSharpList{``0})"/> do.
/// </summary>
/// <param name="name">The name of the parameter to report: <c>"index"</c> for these functions.</param>
/// <param name="index">The index to test.</param>
/// <param name="length">The length of the list.</param>
/// <exception cref="T:System.ArgumentException">
/// The index is negative or not less than the length. The message is
/// <see cref="P:FSharp.Collections.Immutable.ListChecks.IndexOutsideListBounds"/>, and the parameter name is
/// <paramref name="name"/>.
/// </exception>
/// <remarks>
/// Unlike for
/// <see cref="M:FSharp.Collections.Immutable.ListChecks.insertAt(System.String,System.Int32,System.Int32)"/>, the length
/// itself is not a valid index: no element is there to remove or to replace.
/// </remarks>
let removeAt (name : string) (index : int) (length : int) =
    if index < 0 || index >= length then
        invalidArg name IndexOutsideListBounds

/// <summary>
/// Rejects a negative index and a range that ends beyond the list, as
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.RemoveManyAt``1(System.Int32,System.Int32,Microsoft.FSharp.Collections.FSharpList{``0})"/> does.
/// </summary>
/// <param name="name">The name of the parameter to report: <c>"index"</c> for this function.</param>
/// <param name="index">The index of the first element of the range.</param>
/// <param name="count">The number of elements of the range.</param>
/// <param name="length">The length of the list.</param>
/// <exception cref="T:System.ArgumentException">
/// The index is negative, or the index plus the count is greater than the length. The message is
/// <see cref="P:FSharp.Collections.Immutable.ListChecks.IndexOutsideListBounds"/>, and the parameter name is
/// <paramref name="name"/>.
/// </exception>
/// <remarks>
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.RemoveManyAt``1(System.Int32,System.Int32,Microsoft.FSharp.Collections.FSharpList{``0})"/>
/// walks the first <c>index + count</c> elements and keeps the ones before the index, so a count that is not positive
/// removes nothing, even with an index beyond the list, as long as <c>index + count</c> does not exceed the length. The
/// check tests the same sum, which wraps around on overflow there as it does here.
/// </remarks>
let removeManyAt (name : string) (index : int) (count : int) (length : int) =
    if index < 0 || index + count > length then
        invalidArg name IndexOutsideListBounds

/// <summary>
/// Rejects a negative count, as
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.Initialize``1(System.Int32,Microsoft.FSharp.Core.FSharpFunc{System.Int32,``0})"/>,
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.RandomChoices``1(System.Int32,Microsoft.FSharp.Collections.FSharpList{``0})"/> and
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.RandomSample``1(System.Int32,Microsoft.FSharp.Collections.FSharpList{``0})"/> do.
/// </summary>
/// <param name="name">The name of the parameter to report: <c>"count"</c> for these functions.</param>
/// <param name="count">The count to test.</param>
/// <exception cref="T:System.ArgumentException">
/// The count is negative. The message is the one of
/// <see cref="M:FSharp.Collections.Immutable.Raise.invalidArgInputMustBeNonNegative``1(System.String,System.Int32)"/>,
/// which gives the name and the value of the count, and the parameter name is <paramref name="name"/>.
/// </exception>
/// <remarks>
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.Replicate``1(System.Int32,``0)"/>
/// gives the message without the value, so it has a check of its own,
/// <see cref="M:FSharp.Collections.Immutable.ListChecks.replicate(System.String,System.Int32)"/>.
/// </remarks>
let nonNegative (name : string) (count : int) =
    if count < 0 then
        Raise.invalidArgInputMustBeNonNegative name count

/// <summary>
/// Rejects a negative count, as
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.Replicate``1(System.Int32,``0)"/> does.
/// </summary>
/// <param name="name">The name of the parameter to report: <c>"count"</c> for this function.</param>
/// <param name="count">The count to test.</param>
/// <exception cref="T:System.ArgumentException">
/// The count is negative. The message is
/// <see cref="P:FSharp.Collections.Immutable.ErrorStrings.InputMustBeNonNegative"/>, and the parameter name is
/// <paramref name="name"/>.
/// </exception>
/// <remarks>
/// It is a check of its own, besides
/// <see cref="M:FSharp.Collections.Immutable.ListChecks.nonNegative(System.String,System.Int32)"/>, because unlike
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.Initialize``1(System.Int32,Microsoft.FSharp.Core.FSharpFunc{System.Int32,``0})"/>
/// it reports the message without the value.
/// </remarks>
let replicate (name : string) (count : int) =
    if count < 0 then
        invalidArg name ErrorStrings.InputMustBeNonNegative

/// <summary>
/// Rejects a size that is not positive, as
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.ChunkBySize``1(System.Int32,Microsoft.FSharp.Collections.FSharpList{``0})"/> (<c>"chunkSize"</c>),
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.Windowed``1(System.Int32,Microsoft.FSharp.Collections.FSharpList{``0})"/>
/// (<c>"windowSize"</c>) and
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.SplitInto``1(System.Int32,Microsoft.FSharp.Collections.FSharpList{``0})"/> (<c>"count"</c>) do.
/// </summary>
/// <param name="name">The name of the parameter to report: the one in the brackets above.</param>
/// <param name="size">The size to test.</param>
/// <exception cref="T:System.ArgumentException">
/// The size is 0 or negative. The message is the one of
/// <see cref="M:FSharp.Collections.Immutable.Raise.invalidArgInputMustBePositive``1(System.String,System.Int32)"/>, which
/// gives the name and the value of the size, and the parameter name is <paramref name="name"/>.
/// </exception>
let positive (name : string) (size : int) =
    if size <= 0 then
        Raise.invalidArgInputMustBePositive name size

/// <summary>
/// Rejects a negative count and a count beyond the length, as
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.Take``1(System.Int32,Microsoft.FSharp.Collections.FSharpList{``0})"/> does.
/// </summary>
/// <param name="name">The name of the parameter to report: <c>"count"</c> for this function.</param>
/// <param name="count">The count to test.</param>
/// <param name="length">The length of the list.</param>
/// <exception cref="T:System.ArgumentException">
/// The count is negative. The message is the one of
/// <see cref="M:FSharp.Collections.Immutable.Raise.invalidArgInputMustBeNonNegative``1(System.String,System.Int32)"/>, and
/// the parameter name is <paramref name="name"/>.
/// </exception>
/// <exception cref="T:System.InvalidOperationException">
/// The count is greater than the length. The message is the one of
/// <see cref="M:FSharp.Collections.Immutable.Raise.invalidOpListNotEnoughElements``1(System.Int32)"/>, which says how many
/// elements are missing.
/// </exception>
let take (name : string) (count : int) (length : int) =
    if count < 0 then
        Raise.invalidArgInputMustBeNonNegative name count
    elif count > length then
        Raise.invalidOpListNotEnoughElements (count - length)

/// <summary>
/// Rejects a count beyond the length, as
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.Skip``1(System.Int32,Microsoft.FSharp.Collections.FSharpList{``0})"/> does.
/// </summary>
/// <param name="name">The name of the parameter to report: <c>"count"</c> for this function.</param>
/// <param name="count">The count to test.</param>
/// <param name="length">The length of the list.</param>
/// <exception cref="T:System.ArgumentOutOfRangeException">
/// The count is greater than the length. The message is the one of
/// <see cref="M:FSharp.Collections.Immutable.Raise.invalidArgOutOfRange``1(System.String,System.Int32,System.String,System.Int32)"/>,
/// which gives the count and the distance past the list, and the parameter name is <paramref name="name"/>.
/// </exception>
/// <remarks>
/// A negative count skips nothing, so the function does not reject it, as
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.Skip``1(System.Int32,Microsoft.FSharp.Collections.FSharpList{``0})"/> does not.
/// </remarks>
let skip (name : string) (count : int) (length : int) =
    if count > length then
        Raise.invalidArgOutOfRange name count "distance past the list" (count - length)

/// <summary>
/// Rejects a negative index and an index beyond the length, as
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.SplitAt``1(System.Int32,Microsoft.FSharp.Collections.FSharpList{``0})"/> does.
/// </summary>
/// <param name="name">The name of the parameter to report: <c>"index"</c> for this function.</param>
/// <param name="index">The index to test.</param>
/// <param name="length">The length of the list.</param>
/// <exception cref="T:System.ArgumentException">
/// The index is negative. The message is the one of
/// <see cref="M:FSharp.Collections.Immutable.Raise.invalidArgInputMustBeNonNegative``1(System.String,System.Int32)"/>, and
/// the parameter name is <paramref name="name"/>.
/// </exception>
/// <exception cref="T:System.InvalidOperationException">
/// The index is greater than the length. The message is the one of
/// <see cref="M:FSharp.Collections.Immutable.Raise.invalidOpListEmpty``1"/> for an empty list and the one of
/// <see cref="M:FSharp.Collections.Immutable.Raise.invalidOpListNotEnoughElements``1(System.Int32)"/> for a longer one.
/// </exception>
/// <remarks>
/// An empty list has a message of its own because
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.SplitAt``1(System.Int32,Microsoft.FSharp.Collections.FSharpList{``0})"/> gives it one.
/// </remarks>
let splitAt (name : string) (index : int) (length : int) =
    if index < 0 then
        Raise.invalidArgInputMustBeNonNegative name index
    elif index > length then
        if length = 0 then
            Raise.invalidOpListEmpty ()
        else
            Raise.invalidOpListNotEnoughElements (index - length)

/// <summary>
/// Rejects lists of different lengths, as
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.Iterate2``2(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpFunc{``1,Microsoft.FSharp.Core.Unit}},Microsoft.FSharp.Collections.FSharpList{``0},Microsoft.FSharp.Collections.FSharpList{``1})"/>,
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.IterateIndexed2``2(Microsoft.FSharp.Core.FSharpFunc{System.Int32,Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpFunc{``1,Microsoft.FSharp.Core.Unit}}},Microsoft.FSharp.Collections.FSharpList{``0},Microsoft.FSharp.Collections.FSharpList{``1})"/>,
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.Fold2``3(Microsoft.FSharp.Core.FSharpFunc{``2,Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpFunc{``1,``2}}},``2,Microsoft.FSharp.Collections.FSharpList{``0},Microsoft.FSharp.Collections.FSharpList{``1})"/>,
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.ForAll2``2(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpFunc{``1,System.Boolean}},Microsoft.FSharp.Collections.FSharpList{``0},Microsoft.FSharp.Collections.FSharpList{``1})"/>,
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.Map2``3(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpFunc{``1,``2}},Microsoft.FSharp.Collections.FSharpList{``0},Microsoft.FSharp.Collections.FSharpList{``1})"/>,
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.MapIndexed2``3(Microsoft.FSharp.Core.FSharpFunc{System.Int32,Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpFunc{``1,``2}}},Microsoft.FSharp.Collections.FSharpList{``0},Microsoft.FSharp.Collections.FSharpList{``1})"/> and
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.Zip``2(Microsoft.FSharp.Collections.FSharpList{``0},Microsoft.FSharp.Collections.FSharpList{``1})"/>
/// do (<c>"list1"</c>, <c>"list2"</c>).
/// </summary>
/// <param name="name1">The name of the parameter of the first list.</param>
/// <param name="length1">The length of the first list.</param>
/// <param name="name2">The name of the parameter of the second list.</param>
/// <param name="length2">The length of the second list.</param>
/// <exception cref="T:System.ArgumentException">
/// The lengths differ. The exception is the one of
/// <see cref="M:FSharp.Collections.Immutable.Raise.invalidArgDifferentListLength``1(System.String,System.String,System.Int32)"/>:
/// the parameter name is the name of the shorter list, and the message says how much shorter it is.
/// </exception>
let sameLengths (name1 : string) (length1 : int) (name2 : string) (length2 : int) =
    if length1 < length2 then
        Raise.invalidArgDifferentListLength name1 name2 (length2 - length1)
    elif length2 < length1 then
        Raise.invalidArgDifferentListLength name2 name1 (length1 - length2)

/// <summary>
/// Rejects lists of different lengths with a message without lengths, as
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.Exists2``2(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpFunc{``1,System.Boolean}},Microsoft.FSharp.Collections.FSharpList{``0},Microsoft.FSharp.Collections.FSharpList{``1})"/> does.
/// </summary>
/// <param name="name2">The name of the parameter of the second list: <c>"list2"</c> for this function.</param>
/// <param name="length1">The length of the first list.</param>
/// <param name="length2">The length of the second list.</param>
/// <exception cref="T:System.ArgumentException">
/// The lengths differ. The message is <see cref="P:FSharp.Collections.Immutable.ErrorStrings.ListsHadDifferentLengths"/>,
/// and the parameter name is always the name of the second list, <paramref name="name2"/>.
/// </exception>
/// <remarks>
/// It is a check of its own, besides
/// <see cref="M:FSharp.Collections.Immutable.ListChecks.sameLengths(System.String,System.Int32,System.String,System.Int32)"/>,
/// because the exception of
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.Exists2``2(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpFunc{``1,System.Boolean}},Microsoft.FSharp.Collections.FSharpList{``0},Microsoft.FSharp.Collections.FSharpList{``1})"/>
/// neither names the shorter list nor says how much shorter it is.
/// </remarks>
let exists2 (name2 : string) (length1 : int) (length2 : int) =
    if length1 <> length2 then
        invalidArg name2 ErrorStrings.ListsHadDifferentLengths

/// <summary>
/// Rejects lists of different lengths before the folder is called, as
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.FoldBack2``3(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpFunc{``1,Microsoft.FSharp.Core.FSharpFunc{``2,``2}}},Microsoft.FSharp.Collections.FSharpList{``0},Microsoft.FSharp.Collections.FSharpList{``1},``2)"/> does
/// (<c>"list1"</c>, <c>"list2"</c>).
/// </summary>
/// <param name="name1">The name of the parameter of the first list.</param>
/// <param name="length1">The length of the first list.</param>
/// <param name="name2">The name of the parameter of the second list.</param>
/// <param name="length2">The length of the second list.</param>
/// <exception cref="T:System.ArgumentException">
/// The lengths differ. When one of the lists is empty, the exception is the one of
/// <see cref="M:FSharp.Collections.Immutable.ListChecks.sameLengths(System.String,System.Int32,System.String,System.Int32)"/>.
/// Otherwise it is the one of
/// <see cref="M:FSharp.Collections.Immutable.Raise.invalidArg2ListsDifferent``1(System.String,System.Int32,System.String,System.Int32)"/>,
/// which names both lists and gives both lengths.
/// </exception>
/// <remarks>
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.FoldBack2``3(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpFunc{``1,Microsoft.FSharp.Core.FSharpFunc{``2,``2}}},Microsoft.FSharp.Collections.FSharpList{``0},Microsoft.FSharp.Collections.FSharpList{``1},``2)"/>
/// reports the same mistake in two ways. An empty list against a list with elements is reported as the other functions
/// of two lists report it. Two lists with elements reach the fallback of the function, which copies them into arrays to
/// fold them from the end, and it reports their lengths. Hence the two branches of the check.
/// </remarks>
let foldBack2 (name1 : string) (length1 : int) (name2 : string) (length2 : int) =
    if length1 <> length2 then
        if length1 = 0 || length2 = 0 then
            sameLengths name1 length1 name2 length2
        else
            Raise.invalidArg2ListsDifferent name1 length1 name2 length2

/// <summary>
/// Rejects lists of different lengths and reports their lengths, as
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.Map3``4(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpFunc{``1,Microsoft.FSharp.Core.FSharpFunc{``2,``3}}},Microsoft.FSharp.Collections.FSharpList{``0},Microsoft.FSharp.Collections.FSharpList{``1},Microsoft.FSharp.Collections.FSharpList{``2})"/> does
/// (<c>"list1"</c>, <c>"list2"</c>, <c>"list3"</c>).
/// </summary>
/// <param name="name1">The name of the parameter of the first list.</param>
/// <param name="length1">The length of the first list.</param>
/// <param name="name2">The name of the parameter of the second list.</param>
/// <param name="length2">The length of the second list.</param>
/// <param name="name3">The name of the parameter of the third list.</param>
/// <param name="length3">The length of the third list.</param>
/// <exception cref="T:System.ArgumentException">
/// The lengths differ. The exception is the one of
/// <see cref="M:FSharp.Collections.Immutable.Raise.invalidArg3ListsDifferent``1(System.String,System.String,System.String,System.Int32,System.Int32,System.Int32)"/>:
/// it names the three lists and gives their lengths.
/// </exception>
let map3 (name1 : string) (length1 : int) (name2 : string) (length2 : int) (name3 : string) (length3 : int) =
    if length1 <> length2 || length1 <> length3 then
        Raise.invalidArg3ListsDifferent name1 name2 name3 length1 length2 length3

/// <summary>
/// Rejects lists of different lengths and reports the lengths that remain after their common prefix, as
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.Zip3``3(Microsoft.FSharp.Collections.FSharpList{``0},Microsoft.FSharp.Collections.FSharpList{``1},Microsoft.FSharp.Collections.FSharpList{``2})"/>
/// does (<c>"list1"</c>, <c>"list2"</c>, <c>"list3"</c>).
/// </summary>
/// <param name="name1">The name of the parameter of the first list.</param>
/// <param name="length1">The length of the first list.</param>
/// <param name="name2">The name of the parameter of the second list.</param>
/// <param name="length2">The length of the second list.</param>
/// <param name="name3">The name of the parameter of the third list.</param>
/// <param name="length3">The length of the third list.</param>
/// <exception cref="T:System.ArgumentException">
/// The lengths differ. The exception is the one of
/// <see cref="M:FSharp.Collections.Immutable.Raise.invalidArg3ListsDifferent``1(System.String,System.String,System.String,System.Int32,System.Int32,System.Int32)"/>,
/// with each length reduced by the length of the shortest list.
/// </exception>
/// <remarks>
/// It is a check of its own, besides
/// <see cref="M:FSharp.Collections.Immutable.ListChecks.map3(System.String,System.Int32,System.String,System.Int32,System.String,System.Int32)"/>,
/// because
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.Zip3``3(Microsoft.FSharp.Collections.FSharpList{``0},Microsoft.FSharp.Collections.FSharpList{``1},Microsoft.FSharp.Collections.FSharpList{``2})"/>
/// walks the three lists together and reports what is left of each when the shortest one ends.
/// </remarks>
let zip3 (name1 : string) (length1 : int) (name2 : string) (length2 : int) (name3 : string) (length3 : int) =
    if length1 <> length2 || length1 <> length3 then
        let common = min length1 (min length2 length3)
        Raise.invalidArg3ListsDifferent name1 name2 name3 (length1 - common) (length2 - common) (length3 - common)

/// <summary>
/// Rejects lists of different lengths, as
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.Transpose``1(System.Collections.Generic.IEnumerable{Microsoft.FSharp.Collections.FSharpList{``0}})"/> does.
/// </summary>
/// <param name="name">The name of the parameter of the list of lists: <c>"list"</c> for this function.</param>
/// <param name="lengths">The lengths of the lists, in the order of the list of lists.</param>
/// <exception cref="T:System.ArgumentException">
/// The lengths differ. The exception is the one of
/// <see cref="M:FSharp.Collections.Immutable.Raise.invalidArgDifferentListLength``1(System.String,System.String,System.Int32)"/>:
/// the parameter name is the name of the shorter list of a pair that includes the first list, and the message says how
/// much shorter it is. The lists are named by their positions: for the name <c>"list"</c>, <c>"list.[0]"</c>,
/// <c>"list.[1]"</c> and so on.
/// </exception>
/// <remarks>
/// <para>
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.Transpose``1(System.Collections.Generic.IEnumerable{Microsoft.FSharp.Collections.FSharpList{``0}})"/>
/// takes the first element of every list in each round, and fails in the first round in which a list has none, which is
/// the round of the length of the shortest list. It then compares each list with the first one. When the first list is
/// among the shortest, the pair is the first list and the first list that goes on, which is longer. Otherwise the pair is
/// the first list that has run out, which is shorter, and the first list. The check takes the lengths at once and finds
/// the same pair, so the caller does not take the lists apart.
/// </para>
/// <para>
/// Fewer than two lists cannot differ, so they pass.
/// </para>
/// </remarks>
let transpose (name : string) (lengths : int array) =
    if lengths.Length > 1 then
        let shortest = Array.min lengths
        let first = lengths[0]
        let element (index : int) = String.Format ("{0}.[{1}]", name, index)

        if first = shortest then
            // The first list ends first, so the first list that goes on is longer
            let mutable index = 1

            while index < lengths.Length && lengths[index] = shortest do
                index <- index + 1

            if index < lengths.Length then
                Raise.invalidArgDifferentListLength (element 0) (element index) (lengths[index] - shortest)
        else
            // The first list goes on, so the first list that ends is shorter
            let mutable index = 1

            while lengths[index] <> shortest do
                index <- index + 1

            Raise.invalidArgDifferentListLength (element index) (element 0) (first - shortest)

/// <summary>
/// Rejects a negative count and, for a positive count, an empty input, as
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.RandomChoices``1(System.Int32,Microsoft.FSharp.Collections.FSharpList{``0})"/>,
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.RandomChoicesBy``1(Microsoft.FSharp.Core.FSharpFunc{Microsoft.FSharp.Core.Unit,System.Double},System.Int32,Microsoft.FSharp.Collections.FSharpList{``0})"/> and
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.RandomChoicesWith``1(System.Random,System.Int32,Microsoft.FSharp.Collections.FSharpList{``0})"/> do.
/// </summary>
/// <param name="countName">The name of the parameter of the count: <c>"count"</c> for these functions.</param>
/// <param name="count">The count to test.</param>
/// <param name="sourceName">The name of the parameter of the input: <c>"source"</c> for these functions.</param>
/// <param name="length">The length of the input.</param>
/// <exception cref="T:System.ArgumentException">
/// The count is negative (the exception of
/// <see cref="M:FSharp.Collections.Immutable.ListChecks.nonNegative(System.String,System.Int32)"/>), or the count is
/// positive and the input is empty (the message is
/// <see cref="P:FSharp.Collections.Immutable.ErrorStrings.InputSequenceEmpty"/>, and the parameter name is
/// <paramref name="sourceName"/>).
/// </exception>
/// <remarks>
/// A count of 0 draws nothing, so it is valid for an empty input.
/// </remarks>
let randomChoices (countName : string) (count : int) (sourceName : string) (length : int) =
    nonNegative countName count

    if length = 0 && count > 0 then
        invalidArg sourceName ErrorStrings.InputSequenceEmpty

/// <summary>
/// Rejects what
/// <see cref="M:FSharp.Collections.Immutable.ListChecks.randomChoices(System.String,System.Int32,System.String,System.Int32)"/>
/// rejects and a count beyond the length, as
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.RandomSample``1(System.Int32,Microsoft.FSharp.Collections.FSharpList{``0})"/>,
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.RandomSampleBy``1(Microsoft.FSharp.Core.FSharpFunc{Microsoft.FSharp.Core.Unit,System.Double},System.Int32,Microsoft.FSharp.Collections.FSharpList{``0})"/> and
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.RandomSampleWith``1(System.Random,System.Int32,Microsoft.FSharp.Collections.FSharpList{``0})"/> do.
/// </summary>
/// <param name="countName">The name of the parameter of the count: <c>"count"</c> for these functions.</param>
/// <param name="count">The count to test.</param>
/// <param name="sourceName">The name of the parameter of the input: <c>"source"</c> for these functions.</param>
/// <param name="length">The length of the input.</param>
/// <exception cref="T:System.ArgumentException">
/// The input is not empty and shorter than the count. The message is
/// <see cref="P:FSharp.Collections.Immutable.ErrorStrings.NotEnoughElements"/>, and the parameter name is
/// <paramref name="countName"/>.
/// </exception>
/// <remarks>
/// A sample draws each element at most once, so unlike the choices it cannot be larger than the input.
/// </remarks>
let randomSample (countName : string) (count : int) (sourceName : string) (length : int) =
    randomChoices countName count sourceName length

    if length > 0 && count > length then
        invalidArg countName ErrorStrings.NotEnoughElements

/// <summary>
/// The index in a list of the given length that the functions with a randomizer draw, as
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.RandomChoiceBy``1(Microsoft.FSharp.Core.FSharpFunc{Microsoft.FSharp.Core.Unit,System.Double},Microsoft.FSharp.Collections.FSharpList{``0})"/> does.
/// </summary>
/// <param name="name">The name of the parameter of the randomizer: <c>"randomizer"</c> for these functions.</param>
/// <param name="randomizer">The function that returns a number from 0 up to, but not including, 1.</param>
/// <param name="length">The length of the list.</param>
/// <returns>The number that the randomizer returned, times the length, without its fraction.</returns>
/// <exception cref="T:System.ArgumentOutOfRangeException">
/// The randomizer returned a number outside [0, 1), or not a number. The message is the one of
/// <see cref="M:FSharp.Collections.Immutable.Raise.invalidArgRandomizerOutOfRange``1(System.String,System.Double)"/>, and
/// the parameter name is <paramref name="name"/>.
/// </exception>
/// <remarks>
/// The check and the index are one function because the FSharp.Core functions with a randomizer test every number that it
/// returns before they use it.
/// </remarks>
let randomIndex (name : string) (randomizer : unit -> float) (length : int) =
    let value = randomizer ()

    if value >= 0.0 && value < 1.0 then
        int (value * float length)
    else
        Raise.invalidArgRandomizerOutOfRange name value
