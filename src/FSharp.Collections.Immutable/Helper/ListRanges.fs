/// <summary>
/// The range checks of the functions that wrap the members of <see cref="T:System.Collections.Immutable.ImmutableList`1"/>
/// and of its builder, with the parameter names of the callers (ADR 0001, D6 and D14).
/// </summary>
/// <remarks>
/// <para>
/// The members add an index and a count and compare the sum with the length. The sum wraps around at
/// <see cref="F:System.Int32.MaxValue"/>, so a range that starts near the end of the numbers passes the check of some
/// members, which then do nothing, or answer with an index that is not the index of an element. The checks here compare
/// the count with the difference between the length and the index, which cannot overflow once the index is not negative.
/// </para>
/// <para>
/// A range that the members of <see cref="T:System.Collections.Immutable.ImmutableList`1"/> reject is rejected with the
/// same exception and the name that the member gives the parameter, except that the caller passes the names of its own
/// parameters. The checks add the ranges that the members accept and that the documentation calls invalid.
/// </para>
/// <para>
/// The functions of <see cref="T:FSharp.Collections.Immutable.IndexedList"/>,
/// <see cref="T:FSharp.Collections.Immutable.IIndexedList"/> and
/// <see cref="T:FSharp.Collections.Immutable.IndexedListBuilder"/> that search, reverse, sort or copy a range call the
/// checks of this module before they call the member, so that the three modules reject the same ranges in the same way.
/// </para>
/// </remarks>
module internal FSharp.Collections.Immutable.ListRanges

open System

/// <summary>
/// The message of the <see cref="T:System.ArgumentException"/> for a range that does not fit in the list.
/// </summary>
/// <remarks>
/// The runtime throws this exception from
/// <see cref="M:System.Collections.Generic.List`1.BinarySearch(System.Int32,System.Int32,`0,System.Collections.Generic.IComparer{`0})"/>,
/// with a message that says "array" and "source collection". The text here says "list". It is not a constant of
/// <see cref="T:FSharp.Collections.Immutable.ErrorStrings"/>, because no resource holds it.
/// </remarks>
[<Literal>]
let private OffsetAndLengthOutOfBounds =
    "Offset and length were out of bounds for the list or count is greater than the number of elements from index to the end of the list."

/// <summary>
/// Checks a range of <paramref name="count"/> elements that starts at <paramref name="index"/> in a list.
/// </summary>
/// <param name="indexName">The name of the parameter of the index.</param>
/// <param name="index">The index of the first element of the range.</param>
/// <param name="countName">The name of the parameter of the count.</param>
/// <param name="count">The number of elements of the range.</param>
/// <param name="beyondName">
/// The name that the exception for a range beyond the end of the list gives: the members name the count, except
/// <see cref="M:System.Collections.Immutable.ImmutableList`1.Reverse(System.Int32,System.Int32)"/>, which names the
/// index.
/// </param>
/// <param name="length">The length of the list.</param>
/// <exception cref="T:System.ArgumentOutOfRangeException">
/// The index or the count is negative, or the range ends beyond the list: the checks of
/// <see cref="M:System.Collections.Immutable.ImmutableList`1.GetRange(System.Int32,System.Int32)"/>,
/// <see cref="M:System.Collections.Immutable.ImmutableList`1.IndexOf(`0,System.Int32,System.Int32,System.Collections.Generic.IEqualityComparer{`0})"/>,
/// <see cref="M:System.Collections.Immutable.ImmutableList`1.FindIndex(System.Int32,System.Int32,System.Predicate{`0})"/>,
/// <see cref="M:System.Collections.Immutable.ImmutableList`1.Reverse(System.Int32,System.Int32)"/>,
/// <see cref="M:System.Collections.Immutable.ImmutableList`1.Sort(System.Int32,System.Int32,System.Collections.Generic.IComparer{`0})"/>
/// and <see cref="M:System.Collections.Immutable.ImmutableList`1.CopyTo(System.Int32,`0[],System.Int32,System.Int32)"/>.
/// A range of no elements may start at the end of the list.
/// </exception>
/// <remarks>
/// It serves the functions that look at a range from its first element, such as the ones that search forward for an
/// item or for an element that satisfies a predicate, reverse a range or sort it, and, through
/// <see cref="M:FSharp.Collections.Immutable.ListRanges.blit(System.String,System.Int32,System.String,System.Int32,System.Int32,System.String,System.Int32,System.Int32)"/>,
/// copy it. The members compare the sum of the index and the count with the length, which wraps around (see the remarks of
/// the module).
/// </remarks>
let forward (indexName : string) (index : int) (countName : string) (count : int) (beyondName : string) (length : int) =
    if index < 0 then
        raise (ArgumentOutOfRangeException indexName)

    if count < 0 then
        raise (ArgumentOutOfRangeException countName)

    // The index and the length are not negative, so the difference cannot overflow
    if count > length - index then
        raise (ArgumentOutOfRangeException beyondName)

/// <summary>
/// Checks a range of <paramref name="count"/> elements that ends at <paramref name="startIndex"/> in a list, which a
/// backward search walks from its end to its start.
/// </summary>
/// <param name="startIndexName">The name of the parameter of the index of the last element of the range.</param>
/// <param name="startIndex">The index of the first element that the search looks at.</param>
/// <param name="countName">The name of the parameter of the count.</param>
/// <param name="count">The number of elements of the range.</param>
/// <param name="length">The length of the list.</param>
/// <exception cref="T:System.ArgumentOutOfRangeException">
/// The start index or the count is negative, the count exceeds the length, or a range of elements does not lie inside
/// the list: the checks of
/// <see cref="M:System.Collections.Immutable.ImmutableList`1.LastIndexOf(`0,System.Int32,System.Int32,System.Collections.Generic.IEqualityComparer{`0})"/>
/// and <see cref="M:System.Collections.Immutable.ImmutableList`1.FindLastIndex(System.Int32,System.Int32,System.Predicate{`0})"/>,
/// which also accept a last element at or beyond the end of the list, and answer with the index of another element.
/// </exception>
/// <remarks>
/// <para>
/// A range of no elements lies anywhere, so a start index at or beyond the end of the list passes for it.
/// </para>
/// <para>
/// It is a check of its own, besides
/// <see cref="M:FSharp.Collections.Immutable.ListRanges.forward(System.String,System.Int32,System.String,System.Int32,System.String,System.Int32)"/>,
/// because a backward search gives the index of the last element of the range, not of the first, and the members accept a
/// last element at or beyond the end of the list. It serves the functions that search backward for the last index of an
/// item or of an element that satisfies a predicate.
/// </para>
/// </remarks>
let backward (startIndexName : string) (startIndex : int) (countName : string) (count : int) (length : int) =
    if startIndex < 0 then
        raise (ArgumentOutOfRangeException startIndexName)

    if count < 0 || count > length then
        raise (ArgumentOutOfRangeException countName)

    // The range starts at startIndex - count + 1: the count 1 starts at the start index itself. The subtraction cannot
    // overflow, because the count is positive.
    if
        count > 0
        && (startIndex >= length || count - 1 > startIndex)
    then
        raise (ArgumentOutOfRangeException startIndexName)

/// <summary>
/// Checks the range of a binary search, which the members do not check against the length of the list.
/// </summary>
/// <param name="indexName">The name of the parameter of the index.</param>
/// <param name="index">The index of the first element of the range.</param>
/// <param name="countName">The name of the parameter of the count.</param>
/// <param name="count">The number of elements of the range.</param>
/// <param name="length">The length of the list.</param>
/// <exception cref="T:System.ArgumentOutOfRangeException">The index or the count is negative.</exception>
/// <exception cref="T:System.ArgumentException">
/// The range extends beyond the list. The documentation of
/// <see cref="M:System.Collections.Immutable.ImmutableList`1.BinarySearch(System.Int32,System.Int32,`0,System.Collections.Generic.IComparer{`0})"/>
/// promises this exception, and the member returns a result instead (ADR 0001, D14).
/// </exception>
/// <remarks>
/// It is a check of its own, besides
/// <see cref="M:FSharp.Collections.Immutable.ListRanges.forward(System.String,System.Int32,System.String,System.Int32,System.String,System.Int32)"/>,
/// because the exception for a range beyond the end is an <see cref="T:System.ArgumentException"/>, as
/// <see cref="M:System.Collections.Generic.List`1.BinarySearch(System.Int32,System.Int32,`0,System.Collections.Generic.IComparer{`0})"/>
/// throws it, where the other checks of the module throw an
/// <see cref="T:System.ArgumentOutOfRangeException"/>. It serves the functions that search a range of a sorted list.
/// </remarks>
let search (indexName : string) (index : int) (countName : string) (count : int) (length : int) =
    if index < 0 then
        raise (ArgumentOutOfRangeException indexName)

    if count < 0 then
        raise (ArgumentOutOfRangeException countName)

    if count > length - index then
        raise (ArgumentException (OffsetAndLengthOutOfBounds, countName))

/// <summary>
/// Checks the ranges of a copy of elements from a list into an array: the checks of
/// <see cref="M:System.Collections.Immutable.ImmutableList`1.CopyTo(System.Int32,`0[],System.Int32,System.Int32)"/>.
/// </summary>
/// <param name="sourceIndexName">The name of the parameter of the index in the list.</param>
/// <param name="sourceIndex">The index of the first element to copy.</param>
/// <param name="countName">The name of the parameter of the count.</param>
/// <param name="count">The number of elements to copy.</param>
/// <param name="sourceLength">The length of the list.</param>
/// <param name="targetIndexName">The name of the parameter of the index in the array.</param>
/// <param name="targetIndex">The index in the array at which the copy starts.</param>
/// <param name="targetLength">The length of the array.</param>
/// <exception cref="T:System.ArgumentOutOfRangeException">
/// The source index, the count or the target index is negative, or a range lies beyond the list or the array.
/// </exception>
/// <remarks>
/// It checks the range in the list as
/// <see cref="M:FSharp.Collections.Immutable.ListRanges.forward(System.String,System.Int32,System.String,System.Int32,System.String,System.Int32)"/>
/// does, with the count as the name for a range beyond the end, and then the range in the array. It serves the copying
/// functions of the list and of its builder.
/// </remarks>
let blit
    (sourceIndexName : string)
    (sourceIndex : int)
    (countName : string)
    (count : int)
    (sourceLength : int)
    (targetIndexName : string)
    (targetIndex : int)
    (targetLength : int)
    =
    forward sourceIndexName sourceIndex countName count countName sourceLength

    if targetIndex < 0 || count > targetLength - targetIndex then
        raise (ArgumentOutOfRangeException targetIndexName)
