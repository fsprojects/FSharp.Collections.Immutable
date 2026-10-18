namespace FSharp.Collections.Immutable.Tests

open System
open System.Collections.Generic

/// <summary>
/// The strings that the tests of the functions which wrap members of
/// <see cref="T:System.Collections.Immutable.IImmutableList`1"/> run on.
/// </summary>
/// <remarks>
/// The strings tell the default equality from an equality that ignores the case: "b" and "B" are equal in the second
/// one only. Every property builds new arrays on each read, as <see cref="T:FSharp.Collections.Immutable.Tests.Inputs"/>
/// does.
/// </remarks>
[<AbstractClass; Sealed>]
type ListNames =

    /// No elements.
    static member Empty : string array = [||]

    /// One element.
    static member Singleton : string array = [| "b" |]

    /// Strings that repeat, in two cases: "a" at 0 and 4, "b" at 1 and 5, "B" at 2, "c" at 3, "A" at 6 and "d" at 7. When
    /// the case is ignored, "a" is also at 6, and "b" at 2.
    static member Several : string array = [| "a"; "b"; "B"; "c"; "a"; "b"; "A"; "d" |]

    /// Every input, as the rows of the tests that run on each one.
    static member All = seq {
        ListNames.Empty
        ListNames.Singleton
        ListNames.Several
    }

/// <summary>
/// The results that the functions which wrap members of <see cref="T:System.Collections.Immutable.IImmutableList`1"/> must
/// give, computed on arrays with FSharp.Core.
/// </summary>
/// <remarks>
/// The oracles that the flat list shares with the indexed list, such as the removal of the first equal element, the
/// replacement, the search in a range from the front, the descending order and the equality that ignores the case, are in
/// <see cref="T:FSharp.Collections.Immutable.Tests.FlatListOracles"/>. Strings are equal in the default equality when
/// F# <c>=</c> finds them equal, which compares the characters.
/// </remarks>
[<RequireQualifiedAccess>]
module ListOracles =

    /// The elements before a range, the elements of the range and the elements after it.
    let splitRange (elements : 'T array) index count =
        struct (elements[.. index - 1], elements[index .. index + count - 1], elements[index + count ..])

    /// The elements with the elements of a range replaced by the result of a function of them: the oracle of the functions
    /// that reverse, sort or remove a range.
    let changeRange (change : 'T array -> 'T array) (elements : 'T array) index count =
        let struct (before, part, after) = splitRange elements index count
        Array.concat [| before; change part; after |]

    /// A stable sort of the elements, as the sorts of List do it.
    let stableSort (comparison : 'T -> 'T -> int) (elements : 'T array) =
        List.sortWith comparison (List.ofArray elements)
        |> List.toArray

    /// The comparison of the keys of pairs in a direction, which ignores the tags.
    let compareKeys (direction : SortDirection) =
        direction.Apply (fun (struct (left : int, _ : char)) (struct (right : int, _ : char)) -> compare left right)

    /// The keys and the tags of the keyed items of <see cref="P:FSharp.Collections.Immutable.Tests.Inputs.ManyPairs"/> after
    /// a stable sort of a range by key, as
    /// <see cref="M:Microsoft.FSharp.Collections.ListModule.Sort``1(Microsoft.FSharp.Collections.FSharpList{``0})"/> sorts
    /// them. The items compare by their keys only.
    let sortedKeyedRange index count =
        KeyedItem.KeysAndTags (
            changeRange (fun part -> List.sort (List.ofArray part) |> List.toArray) (KeyedItem.OfManyPairs ()) index count
        )

    /// The pairs of <see cref="P:FSharp.Collections.Immutable.Tests.Inputs.ManyPairs"/> after a stable sort of a range with a
    /// comparison, as
    /// <see cref="M:Microsoft.FSharp.Collections.ListModule.SortWith``1(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpFunc{``0,System.Int32}},Microsoft.FSharp.Collections.FSharpList{``0})"/>
    /// sorts them.
    let sortedPairRange (comparison : struct (int * char) -> struct (int * char) -> int) index count =
        changeRange (stableSort comparison) Inputs.ManyPairs index count

    /// The elements without one element for each of the items.
    let removeEach (equal : 'T -> 'T -> bool) (items : 'T array) (elements : 'T array) =
        Array.fold (fun remaining item -> FlatListOracles.removeFirst equal item remaining) elements items

    /// The index of the last element of a range that the equality finds equal to the value, or -1: the oracle of the
    /// searches from the back, which ends at the start index and has the count of elements. A range of no elements has no
    /// element wherever it lies, also at <see cref="F:System.Int32.MaxValue"/>, where the bounds of the range overflow in
    /// <see cref="M:FSharp.Collections.Immutable.Tests.FlatListOracles.lastInRange"/>.
    let lastIndexOfRange (equal : 'T -> 'T -> bool) startIndex count (value : 'T) (elements : 'T array) =
        match count with
        | 0 -> -1
        | _ -> FlatListOracles.lastInRange equal startIndex count value elements

/// <summary>
/// Why a function rejects a range of a collection, which tells the parameter that its exception names: the functions name
/// their parameters differently, and a row holds the fault, which every test turns into the name of its function.
/// </summary>
[<RequireQualifiedAccess>]
type RangeFault =
    /// The index of the first element, or of the last element of a backward search, is negative.
    | FirstNegative
    /// The count is negative.
    | CountNegative
    /// The range extends beyond the end of the collection, or the count of a backward search exceeds the length.
    | Beyond
    /// A backward search: the range starts before the first element.
    | BeforeStart
    /// A backward search: the last element of the range is at or after the end of the collection.
    | AfterEnd

    /// The name of the parameter that the exception names: the name of the first index for the faults of the index, the
    /// name of the count, and the name that the function gives a range beyond the end.
    member this.ParamName (first : string, beyond : string) =
        match this with
        | FirstNegative
        | BeforeStart
        | AfterEnd -> first
        | CountNegative -> "count"
        | Beyond -> beyond

/// <summary>
/// The sorted inputs of the binary searches. Every property builds new arrays on each read.
/// </summary>
[<AbstractClass; Sealed>]
type SortedInputs =

    /// Eight integers in ascending order, without duplicates: a binary search of a duplicate may find any of its places.
    static member Ascending : int array = [| 1; 3; 5; 7; 9; 11; 13; 15 |]

    /// Four strings in ascending order when the case is ignored. In the order of their characters, "B" and "D" come before
    /// "a" and "c".
    static member Names : string array = [| "a"; "B"; "c"; "D" |]

    /// A comparer for integers in descending order.
    static member DescendingComparer : IComparer<int> = Comparer<int>.Create (fun left right -> compare right left)

/// <summary>
/// The rows of the tests of the functions which wrap members of
/// <see cref="T:System.Collections.Immutable.IImmutableList`1"/>, for the modules and the builder module of the indexed
/// list.
/// </summary>
/// <remarks>
/// The inputs with several elements, <see cref="P:FSharp.Collections.Immutable.Tests.ListNames.Several"/> and
/// <see cref="P:FSharp.Collections.Immutable.Tests.Inputs.Several"/>, and the ascending integers of
/// <see cref="P:FSharp.Collections.Immutable.Tests.SortedInputs.Ascending"/> have eight elements, so that the rows of the
/// ranges serve all of them.
/// </remarks>
[<AbstractClass; Sealed>]
type ListMemberCases =

    /// The names of the functions that search forward in a range, as the rows of the tests that tell them apart.
    static member ForwardSearches = seq {
        "indexOfRange"
        "indexOfRangeWith"
    }

    /// The names of the functions that search backward in a range.
    static member BackwardSearches = seq {
        "lastIndexOfRange"
        "lastIndexOfRangeWith"
    }

    /// The names of the functions that remove several values, as the rows of the tests that tell them apart.
    static member Removals = seq {
        "removeMany"
        "removeManyWith"
    }

    /// Each function that removes several values with each sequence of values that it rejects, a null sequence and a default
    /// flat list given as a sequence, on a collection without elements and on one with elements: a function may return early
    /// for an empty collection.
    static member RejectedValues = seq {
        for name in ListMemberCases.Removals do
            for kind in [| "null sequence"; "default flat list" |] do
                for input in [| "empty collection"; "collection with elements" |] do
                    struct (name, kind, input)
    }

    /// Every input with each value to remove or to replace: the first of two equal elements, the only element, the last
    /// element, a value that equals another in a different case, and a value that no input has.
    static member Values = seq {
        for input in ListNames.All do
            for value in [| "a"; "b"; "B"; "A"; "c"; "d"; "z" |] do
                struct (input, value)
    }

    /// The values of <see cref="P:FSharp.Collections.Immutable.Tests.ListMemberCases.Values"/> that an element equals in the
    /// default equality of strings.
    static member PresentValues = seq {
        for struct (input, value) in ListMemberCases.Values do
            if Array.contains value input then
                struct (input, value)
    }

    /// The values of <see cref="P:FSharp.Collections.Immutable.Tests.ListMemberCases.Values"/> that an element equals when
    /// the case is ignored.
    static member PresentValuesIgnoringCase = seq {
        for struct (input, value) in ListMemberCases.Values do
            if Array.exists (FlatListOracles.equalIgnoringCase value) input then
                struct (input, value)
    }

    /// Every input with values that no element equals in the default equality of strings.
    static member AbsentValues = seq {
        struct (ListNames.Empty, "a")
        struct (ListNames.Singleton, "a")
        struct (ListNames.Singleton, "B")
        struct (ListNames.Several, "z")
        struct (ListNames.Several, "C")
        struct (ListNames.Several, "D")
    }

    /// Every input with values that no element equals when the case is ignored.
    static member AbsentValuesIgnoringCase = seq {
        struct (ListNames.Empty, "a")
        struct (ListNames.Singleton, "a")
        struct (ListNames.Several, "z")
        struct (ListNames.Several, "e")
    }

    /// Every input with the values to remove: none, one, a value as often as the input with several elements has it and
    /// more often, the same value in two cases, and several values of which some are missing.
    static member ValueSequences = seq {
        for input in ListNames.All do
            for items in
                [|
                    [||]
                    [| "a" |]
                    [| "a"; "a" |]
                    [| "a"; "a"; "a" |]
                    [| "B"; "b" |]
                    [| "b"; "B"; "B" |]
                    [| "z" |]
                    [| "d"; "a"; "z"; "a" |]
                |] do
                struct (input, items)
    }

    /// Every input with sequences of values that no element equals in the default equality of strings.
    static member AbsentValueSequences = seq {
        for input in ListNames.All do
            for items in [| [||]; [| "z" |]; [| "z"; "C" |] |] do
                struct (input, items)
    }

    /// Every input with sequences of values that no element equals when the case is ignored.
    static member AbsentValueSequencesIgnoringCase = seq {
        for input in ListNames.All do
            for items in [| [||]; [| "z" |]; [| "z"; "e" |] |] do
                struct (input, items)
    }

    /// Every range of the input with several elements, as the index of its first element and its length, with the values
    /// to look for: one that occurs twice, one in two cases, and the last element.
    static member ForwardRanges = seq {
        let length = ListNames.Several.Length

        for startIndex in 0..length do
            for count in 0 .. length - startIndex do
                for value in [| "a"; "B"; "d" |] do
                    struct (startIndex, count, value)
    }

    /// Every range of the input with several elements for a backward search, as the index of its last element and its
    /// length, with the values to look for. A range of no elements lies anywhere, so the rows add the indices at and after
    /// the end.
    static member BackwardRanges = seq {
        let length = ListNames.Several.Length

        for startIndex in 0 .. length - 1 do
            for count in 0 .. startIndex + 1 do
                for value in [| "a"; "B"; "d" |] do
                    struct (startIndex, count, value)

        for startIndex in [| length; length + 1; Int32.MaxValue |] do
            for value in [| "a"; "B" |] do
                struct (startIndex, 0, value)
    }

    /// The ranges that the input with several elements does not have, as the index of the first element, the length and
    /// the fault: negative numbers, a range beyond the end, and ranges whose end overflows `Int32.MaxValue`, which the
    /// members of `ImmutableList` accept.
    static member RangeFaults = seq {
        let length = ListNames.Several.Length

        struct (-1, 0, RangeFault.FirstNegative)
        struct (-1, 1, RangeFault.FirstNegative)
        struct (-1, -1, RangeFault.FirstNegative)
        struct (Int32.MinValue, 0, RangeFault.FirstNegative)
        struct (0, -1, RangeFault.CountNegative)
        struct (0, Int32.MinValue, RangeFault.CountNegative)
        struct (0, length + 1, RangeFault.Beyond)
        struct (length - 3, 4, RangeFault.Beyond)
        struct (length, 1, RangeFault.Beyond)
        struct (length + 1, 0, RangeFault.Beyond)
        struct (Int32.MaxValue, 0, RangeFault.Beyond)
        struct (Int32.MaxValue, 1, RangeFault.Beyond)
        struct (1, Int32.MaxValue, RangeFault.Beyond)
    }

    /// The ranges that the input with several elements does not have for a backward search, as the index of the last
    /// element, the length and the fault: negative numbers, a count beyond the length, a range that starts before the
    /// first element, and a last element at or after the end, which the members of `ImmutableList` accept.
    static member BackwardRangeFaults = seq {
        let length = ListNames.Several.Length

        struct (-1, 0, RangeFault.FirstNegative)
        struct (-1, 1, RangeFault.FirstNegative)
        struct (-1, -1, RangeFault.FirstNegative)
        struct (Int32.MinValue, 0, RangeFault.FirstNegative)
        struct (0, -1, RangeFault.CountNegative)
        struct (0, Int32.MinValue, RangeFault.CountNegative)
        struct (length - 1, length + 1, RangeFault.Beyond)
        struct (0, 2, RangeFault.BeforeStart)
        struct (3, 5, RangeFault.BeforeStart)
        struct (length, 1, RangeFault.AfterEnd)
        struct (length + 1, 2, RangeFault.AfterEnd)
        struct (Int32.MaxValue, 1, RangeFault.AfterEnd)
        struct (Int32.MaxValue, 2, RangeFault.AfterEnd)
    }

    /// Every range of the ascending integers with the values to search for: present in some ranges and absent from others,
    /// absent between two elements, before the first one and after the last one.
    static member SearchRanges = seq {
        let length = SortedInputs.Ascending.Length

        for index in 0..length do
            for count in 0 .. length - index do
                for value in [| 7; 4; 0; 99 |] do
                    struct (index, count, value)
    }

    /// The strings to search for with a comparer that ignores the case: equal in both cases, between the elements, before
    /// the first one and after the last one.
    static member NameValues = seq {
        "a"
        "A"
        "b"
        "B"
        "c"
        "D"
        "d"
        "e"
        "0"
    }

    /// The copies that reject a range of a source of eight elements into a target of five elements, as the index in the
    /// source, the index in the target, the count and the name of the parameter that the exception names.
    static member BlitRejections = seq {
        struct (-1, 0, 1, "sourceIndex")
        struct (0, 0, -1, "count")
        struct (6, 0, 3, "count")
        struct (0, -1, 1, "targetIndex")
        struct (0, 3, 3, "targetIndex")
    }

    /// The ranges that the functions that search forward reject, for each of them.
    static member ForwardRangesOutOfRange = seq {
        for name in ListMemberCases.ForwardSearches do
            for struct (startIndex, count, fault) in ListMemberCases.RangeFaults do
                struct (name, startIndex, count, fault)
    }

    /// The ranges that the functions that search backward reject, for each of them.
    static member BackwardRangesOutOfRange = seq {
        for name in ListMemberCases.BackwardSearches do
            for struct (startIndex, count, fault) in ListMemberCases.BackwardRangeFaults do
                struct (name, startIndex, count, fault)
    }
