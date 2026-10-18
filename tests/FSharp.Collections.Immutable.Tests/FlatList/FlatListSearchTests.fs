namespace FSharp.Collections.Immutable.Tests

open System
open System.Collections.Generic
open System.Collections.Immutable
open FSharp.Collections.Immutable
open Microsoft.VisualStudio.TestTools.UnitTesting

/// The searches of a range and the binary searches of `FlatList`, and the searches of a range of its builder.
[<TestClass; FlatListTestCategory; BuilderTestCategory>]
type FlatListSearchTests () =

    /// The inputs of the searches of a range: none, one element and several, with a value twice and in two cases.
    static let searchInputs () = [| [||]; [| "b" |]; FlatListOracles.rangeNames () |]

    /// The values that the searches of a range look for: in the case of the elements, in another case, and a value that
    /// no input has.
    static let valuesToFind = [| "B"; "b"; "Z" |]

    /// Every valid range for the searches from the front. The start index is less than the length, as the BCL of .NET 8
    /// and .NET 10 requires, except for the empty range at the start index 0.
    static let frontRanges (length : int) = seq {
        for startIndex in 0 .. max 0 (length - 1) do
            for count in 0 .. length - startIndex do
                struct (startIndex, count)
    }

    /// Every valid range for the searches from the back: from the start index down over the count of elements.
    static let backRanges (length : int) = seq {
        for startIndex in 0 .. max 0 (length - 1) do
            for count in 0 .. min (startIndex + 1) length do
                struct (startIndex, count)
    }

    /// The searches of a range from the front, by name, with the function, the BCL member that it wraps and the equality
    /// that it searches in. The functions with a comparer get the comparer that ignores the case.
    static let frontSearches
        : Map<
              string,
              struct ((string array -> int -> int -> string -> int) *
              (string array -> int -> int -> string -> int) *
              (string -> string -> bool))
           > =
        let comparer = StringComparer.OrdinalIgnoreCase

        Map.ofArray [|
            "FlatList.indexOfRange",
            struct ((fun input startIndex count value -> FlatList.indexOfRange startIndex count value (FlatList.ofArray input)),
                    (fun input startIndex count value -> (FlatList.ofArray input).IndexOf(value, startIndex, count)),
                    (=))
            "FlatList.indexOfRangeWith",
            struct ((fun input startIndex count value ->
                        FlatList.indexOfRangeWith comparer startIndex count value (FlatList.ofArray input)
                    ),
                    (fun input startIndex count value -> (FlatList.ofArray input).IndexOf(value, startIndex, count, comparer)),
                    FlatListOracles.equalIgnoringCase)
            "FlatListBuilder.indexOfRange",
            struct ((fun input startIndex count value ->
                        FlatListBuilder.indexOfRange startIndex count value (FlatListOracles.builderOf input)
                    ),
                    (fun input startIndex count value -> (FlatListOracles.builderOf input).IndexOf(value, startIndex, count)),
                    (=))
            "FlatListBuilder.indexOfRangeWith",
            struct ((fun input startIndex count value ->
                        FlatListBuilder.indexOfRangeWith comparer startIndex count value (FlatListOracles.builderOf input)
                    ),
                    (fun input startIndex count value ->
                        (FlatListOracles.builderOf input).IndexOf(value, startIndex, count, comparer)
                    ),
                    FlatListOracles.equalIgnoringCase)
        |]

    /// The searches of a range from the back, as the table of the searches from the front.
    static let backSearches
        : Map<
              string,
              struct ((string array -> int -> int -> string -> int) *
              (string array -> int -> int -> string -> int) *
              (string -> string -> bool))
           > =
        let comparer = StringComparer.OrdinalIgnoreCase

        Map.ofArray [|
            "FlatList.lastIndexOfRange",
            struct ((fun input startIndex count value ->
                        FlatList.lastIndexOfRange startIndex count value (FlatList.ofArray input)
                    ),
                    (fun input startIndex count value -> (FlatList.ofArray input).LastIndexOf(value, startIndex, count)),
                    (=))
            "FlatList.lastIndexOfRangeWith",
            struct ((fun input startIndex count value ->
                        FlatList.lastIndexOfRangeWith comparer startIndex count value (FlatList.ofArray input)
                    ),
                    (fun input startIndex count value -> (FlatList.ofArray input).LastIndexOf(value, startIndex, count, comparer)),
                    FlatListOracles.equalIgnoringCase)
            "FlatListBuilder.lastIndexOfRange",
            struct ((fun input startIndex count value ->
                        FlatListBuilder.lastIndexOfRange startIndex count value (FlatListOracles.builderOf input)
                    ),
                    (fun input startIndex count value -> (FlatListOracles.builderOf input).LastIndexOf(value, startIndex, count)),
                    (=))
            "FlatListBuilder.lastIndexOfRangeWith",
            struct ((fun input startIndex count value ->
                        FlatListBuilder.lastIndexOfRangeWith comparer startIndex count value (FlatListOracles.builderOf input)
                    ),
                    (fun input startIndex count value ->
                        (FlatListOracles.builderOf input).LastIndexOf(value, startIndex, count, comparer)
                    ),
                    FlatListOracles.equalIgnoringCase)
        |]

    /// The elements of the binary searches: sorted, ascending, and all different.
    static let sortedValues () = [| 1; 3; 5; 7; 9; 11; 13; 15 |]

    /// The values that the binary searches look for: below every element, in the elements when the range has them, between
    /// two elements, and above every element.
    static let searchValues = [| 0; 7; 8; 16 |]

    /// Strings that are sorted and different when the case is ignored. The ordinal order puts "B" and "D" before "a".
    static let sortedNames () = [| "a"; "B"; "c"; "D"; "e" |]

    /// The binary searches of a range, by name, with an input that the comparer of the search sorts, the comparer, and the
    /// function on the input.
    static let binarySearchRanges : Map<string, struct (int array * IComparer<int> * (int array -> int -> int -> int -> int))> =
        let descending = Comparer<int>.Create(Comparison<int> FlatListOracles.descending)

        Map.ofArray [|
            nameof FlatList.binarySearchRange,
            struct (sortedValues (),
                    Comparer<int>.Default :> IComparer<int>,
                    (fun sorted index count value -> FlatList.binarySearchRange index count value (FlatList.ofArray sorted)))
            nameof FlatList.binarySearchRangeWithComparer,
            struct (Array.rev (sortedValues ()),
                    descending :> IComparer<int>,
                    (fun sorted index count value ->
                        FlatList.binarySearchRangeWithComparer descending index count value (FlatList.ofArray sorted)
                    ))
        |]

    /// Every valid range of every search from the front on every input, with each value to find.
    static member FrontRanges = seq {
        for name in frontSearches.Keys do
            for input in searchInputs () do
                for struct (startIndex, count) in frontRanges input.Length do
                    for value in valuesToFind do
                        struct (name, input, startIndex, count, value)
    }

    /// Every valid range of every search from the back on every input, with each value to find.
    static member BackRanges = seq {
        for name in backSearches.Keys do
            for input in searchInputs () do
                for struct (startIndex, count) in backRanges input.Length do
                    for value in valuesToFind do
                        struct (name, input, startIndex, count, value)
    }

    /// The ranges that the searches from the front reject: a start index that is negative or not less than the length,
    /// except for the range 0, 0, and a count that is negative or beyond the end. The BCL of .NET 11 accepts an empty
    /// range at a start index that equals the length, so the rows with such a range do not throw there. The project
    /// targets .NET 8 and .NET 10.
    static member FrontRangesOutOfRange = seq {
        let ranges = [|
            struct ([||], 0, 1)
            struct ([||], 1, 0)
            struct ([||], -1, 0)
            struct ([| "b" |], 1, 0)
            struct ([| "b" |], 0, 2)
            struct (FlatListOracles.rangeNames (), 5, 0)
            struct (FlatListOracles.rangeNames (), 6, 0)
            struct (FlatListOracles.rangeNames (), -1, 0)
            struct (FlatListOracles.rangeNames (), -1, 2)
            struct (FlatListOracles.rangeNames (), 0, -1)
            struct (FlatListOracles.rangeNames (), 0, 6)
            struct (FlatListOracles.rangeNames (), 2, 4)
            struct (FlatListOracles.rangeNames (), 5, 1)
            struct (FlatListOracles.rangeNames (), 3, 3)
        |]

        for name in frontSearches.Keys do
            for struct (input, startIndex, count) in ranges do
                struct (name, input, startIndex, count)
    }

    /// The ranges that the searches from the back reject: a start index that is negative or not less than the length,
    /// except for the range 0, 0, and a count that is negative or beyond the start index plus one.
    static member BackRangesOutOfRange = seq {
        let ranges = [|
            struct ([||], 0, 1)
            struct ([||], 1, 0)
            struct ([||], -1, 0)
            struct ([| "b" |], 1, 0)
            struct ([| "b" |], 0, 2)
            struct (FlatListOracles.rangeNames (), 5, 1)
            struct (FlatListOracles.rangeNames (), 5, 0)
            struct (FlatListOracles.rangeNames (), -1, 0)
            struct (FlatListOracles.rangeNames (), 3, 5)
            struct (FlatListOracles.rangeNames (), 3, -1)
            struct (FlatListOracles.rangeNames (), 0, 2)
            struct (FlatListOracles.rangeNames (), 4, 6)
            struct (FlatListOracles.rangeNames (), 2, 4)
        |]

        for name in backSearches.Keys do
            for struct (input, startIndex, count) in ranges do
                struct (name, input, startIndex, count)
    }

    /// Every valid range of the sorted integers, with each value to search for, for each binary search of a range.
    static member BinarySearchRanges = seq {
        let length = (sortedValues ()).Length

        for name in binarySearchRanges.Keys do
            for index in 0..length do
                for count in 0 .. length - index do
                    for value in searchValues do
                        struct (name, index, count, value)
    }

    /// Every range that the binary searches of a range reject: a negative index or count, and a range beyond the end.
    static member BinarySearchRangesOutOfRange = seq {
        let ranges = [|
            struct (-1, 2)
            struct (0, -1)
            struct (-1, -1)
            struct (9, 0)
            struct (8, 1)
            struct (0, 9)
            struct (4, 5)
            struct (7, 2)
            struct (3, 6)
        |]

        for name in binarySearchRanges.Keys do
            for struct (index, count) in ranges do
                struct (name, index, count)
    }

    /// Every valid range of the sorted strings, with each value to search for.
    static member BinarySearchRangesOfNames = seq {
        let length = (sortedNames ()).Length

        for index in 0..length do
            for count in 0 .. length - index do
                for value in [| "b"; "D"; "z" |] do
                    struct (index, count, value)
    }

    /// The values to search for in the sorted integers.
    static member SearchValues = searchValues

    /// The values to search for in the sorted strings: in the other case, in the same case, below, and above.
    static member SearchNames = [| "A"; "b"; "C"; "d"; "E"; "f"; "0" |]

    [<TestMethod; DynamicData(nameof FlatListSearchTests.FrontRanges)>]
    member _.``range searches from the front return the first equal element of the range``
        (name : string, input : string array, startIndex : int, count : int, value : string)
        =
        let struct (search, _, equal) = frontSearches[name]

        Assert.AreEqual<int>(
            FlatListOracles.firstInRange equal startIndex count value input,
            search input startIndex count value,
            $"%s{name} must return the index of the first equal element of the range"
        )

    [<TestMethod; DynamicData(nameof FlatListSearchTests.BackRanges)>]
    member _.``range searches from the back return the last equal element of the range``
        (name : string, input : string array, startIndex : int, count : int, value : string)
        =
        let struct (search, _, equal) = backSearches[name]

        Assert.AreEqual<int>(
            FlatListOracles.lastInRange equal startIndex count value input,
            search input startIndex count value,
            $"%s{name} must return the index of the last equal element of the range"
        )

    [<TestMethod; DynamicData(nameof FlatListSearchTests.FrontRangesOutOfRange)>]
    member _.``range searches from the front throw for a range out of range as the BCL does``
        (name : string, input : string array, startIndex : int, count : int)
        =
        let struct (search, bcl, _) = frontSearches[name]

        ExceptionAssert.Same (
            id,
            (fun () -> bcl input startIndex count "b" |> ignore),
            (fun () -> search input startIndex count "b" |> ignore),
            $"%s{name} must throw as the BCL member does for a range out of range"
        )

    [<TestMethod; DynamicData(nameof FlatListSearchTests.BackRangesOutOfRange)>]
    member _.``range searches from the back throw for a range out of range as the BCL does``
        (name : string, input : string array, startIndex : int, count : int)
        =
        let struct (search, bcl, _) = backSearches[name]

        ExceptionAssert.Same (
            id,
            (fun () -> bcl input startIndex count "b" |> ignore),
            (fun () -> search input startIndex count "b" |> ignore),
            $"%s{name} must throw as the BCL member does for a range out of range"
        )

    [<TestMethod; DataRow "indexOfRangeWith"; DataRow "lastIndexOfRangeWith">]
    member _.``range searches with a comparer take null for the default comparer`` (name : string) =
        let names = FlatListOracles.rangeNames ()
        let array = FlatList.ofArray names
        let comparer = Unchecked.defaultof<IEqualityComparer<string>>

        // "B" is there once, at the index 2, where the ignoring comparer would find "b" at the index 1 or 4
        let found, expected =
            match name with
            | "indexOfRangeWith" -> FlatList.indexOfRangeWith comparer 0 5 "B" array, 2
            | _ -> FlatList.lastIndexOfRangeWith comparer 4 5 "B" array, 2

        Assert.AreEqual<int>(expected, found, $"FlatList.%s{name} must compare in the default equality for a null comparer")

        let builder = FlatListOracles.builderOf names

        let foundInBuilder =
            match name with
            | "indexOfRangeWith" -> FlatListBuilder.indexOfRangeWith comparer 0 5 "B" builder
            | _ -> FlatListBuilder.lastIndexOfRangeWith comparer 4 5 "B" builder

        Assert.AreEqual<int>(
            expected,
            foundInBuilder,
            $"FlatListBuilder.%s{name} must compare in the default equality for a null comparer"
        )

    [<TestMethod; DynamicData(nameof FlatListSearchTests.BinarySearchRanges)>]
    member _.``binary searches of a range find the value or the complement of its insertion point in the range``
        (name : string, index : int, count : int, value : int)
        =
        let struct (sorted, comparer, search) = binarySearchRanges[name]

        Assert.AreEqual<int>(
            FlatListOracles.searchRange comparer index count value sorted,
            search sorted index count value,
            $"FlatList.%s{name} must search the range in the order of its comparison"
        )

    [<TestMethod; DynamicData(nameof FlatListSearchTests.BinarySearchRangesOutOfRange)>]
    member _.``binary searches of a range throw for a range out of range as the BCL does``
        (name : string, index : int, count : int)
        =
        let struct (sorted, comparer, search) = binarySearchRanges[name]

        // Array.BinarySearch calls the count of the range length, and the functions call it count
        let countOfLength (parameter : string) =
            match parameter with
            | "length" -> "count"
            | other -> other

        ExceptionAssert.Same (
            countOfLength,
            (fun () ->
                ImmutableArray.BinarySearch (FlatList.ofArray sorted, index, count, 7, comparer)
                |> ignore
            ),
            (fun () -> search sorted index count 7 |> ignore),
            $"FlatList.%s{name} must throw as the BCL does, with the parameter name count"
        )

    [<TestMethod; DataRow(0, 4, "a", 2); DataRow(0, 2, "a", -3); DataRow(2, 2, "B", -3); DataRow(2, 2, "b", 3)>]
    member _.``binarySearchRange compares strings as F# generic comparison does, which is ordinal``
        (index : int, count : int, value : string, expected : int)
        =
        // The ordinal order of these strings is not the order of the current culture, which Comparer.Default would use
        let sorted = FlatList.ofArray [| "A"; "B"; "a"; "b" |]

        Assert.AreEqual<int>(
            expected,
            FlatList.binarySearchRange index count value sorted,
            "FlatList.binarySearchRange must search in the ordinal order of the strings"
        )

    [<TestMethod; DynamicData(nameof FlatListSearchTests.BinarySearchRangesOfNames)>]
    member _.``binarySearchRangeWithComparer searches the range in the order of the comparer``
        (index : int, count : int, value : string)
        =
        let sorted = sortedNames ()

        Assert.AreEqual<int>(
            FlatListOracles.searchRange StringComparer.OrdinalIgnoreCase index count value sorted,
            FlatList.binarySearchRangeWithComparer StringComparer.OrdinalIgnoreCase index count value (FlatList.ofArray sorted),
            "FlatList.binarySearchRangeWithComparer must search in the order of the comparer"
        )

    [<TestMethod; DynamicData(nameof FlatListSearchTests.SearchValues)>]
    member _.``binarySearchWithComparer searches the flat list in the order of the comparer`` (value : int) =
        let descending = Comparer<int>.Create(Comparison<int> FlatListOracles.descending)
        let sorted = Array.rev (sortedValues ())

        Assert.AreEqual<int>(
            FlatListOracles.searchRange descending 0 sorted.Length value sorted,
            FlatList.binarySearchWithComparer descending value (FlatList.ofArray sorted),
            "FlatList.binarySearchWithComparer must search in the order of the comparer"
        )

    [<TestMethod; DynamicData(nameof FlatListSearchTests.SearchNames)>]
    member _.``binarySearchWithComparer finds a string that differs in case when the comparer ignores the case``
        (value : string)
        =
        let sorted = sortedNames ()

        Assert.AreEqual<int>(
            FlatListOracles.searchRange StringComparer.OrdinalIgnoreCase 0 sorted.Length value sorted,
            FlatList.binarySearchWithComparer StringComparer.OrdinalIgnoreCase value (FlatList.ofArray sorted),
            "FlatList.binarySearchWithComparer must compare as the comparer does"
        )

    [<TestMethod; DataRow 7; DataRow 8>]
    member _.``binary searches with a comparer take null for the default comparer`` (value : int) =
        let sorted = FlatList.ofArray (sortedValues ())
        let comparer = Unchecked.defaultof<IComparer<int>>
        let expected = FlatListOracles.searchRange Comparer<int>.Default 0 sorted.Length value (sortedValues ())

        Assert.AreEqual<int>(
            expected,
            FlatList.binarySearchWithComparer comparer value sorted,
            "FlatList.binarySearchWithComparer must compare in the default order for a null comparer"
        )

        Assert.AreEqual<int>(
            expected,
            FlatList.binarySearchRangeWithComparer comparer 0 sorted.Length value sorted,
            "FlatList.binarySearchRangeWithComparer must compare in the default order for a null comparer"
        )

    [<TestMethod; DataRow "binarySearchWithComparer"; DataRow "binarySearchRangeWithComparer">]
    member _.``binary searches with a comparer that throws throw InvalidOperationException with the exception inside``
        (name : string)
        =
        let failure = InvalidOperationException "the comparer fails"

        let comparer = {
            new IComparer<int> with
                member _.Compare (_, _) = raise failure
        }

        let sorted = FlatList.ofArray (sortedValues ())

        let search () =
            match name with
            | "binarySearchWithComparer" -> FlatList.binarySearchWithComparer comparer 7 sorted
            | _ -> FlatList.binarySearchRangeWithComparer comparer 0 8 7 sorted

        let ex =
            Assert.ThrowsExactly<InvalidOperationException>(
                Action (search >> ignore),
                $"FlatList.%s{name} must throw InvalidOperationException for a comparer that throws"
            )

        Assert.AreSame<exn | null>(
            failure,
            ex.InnerException,
            $"FlatList.%s{name} must keep the exception of the comparer inside"
        )
