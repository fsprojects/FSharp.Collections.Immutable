namespace FSharp.Collections.Immutable.Tests

open System
open System.Collections.Generic
open FSharp.Collections.Immutable
open Microsoft.VisualStudio.TestTools.UnitTesting

/// <summary>
/// The functions of <see cref="T:FSharp.Collections.Immutable.IndexedList"/> that work on a range of the elements, and its
/// binary searches with a comparer.
/// </summary>
/// <remarks>
/// The rows of the ranges of the input are the ones of <see cref="T:FSharp.Collections.Immutable.Tests.FlatListTests"/>,
/// the functions of which have the same names and the same ranges. The removal, the replacement and the searches in a
/// range of the shared contract are in <see cref="T:FSharp.Collections.Immutable.Tests.ListMembersContract`1"/>.
/// </remarks>
[<TestClass; IndexedListTestCategory>]
type IndexedListRangeTests () =

    static let several () = IndexedList.ofArray Inputs.Several

    /// The functions that sort a range, by name, with the function and the sort of the range in the oracle. The inputs
    /// have no two different elements with the same absolute value, and the comparer of `sortRangeWithComparer` orders by
    /// the absolute value, so the result does not depend on the stability of the sort; the stability has its own tests.
    static let rangeSorts : Map<string, struct ((int -> int -> IndexedList<int> -> IndexedList<int>) * (int array -> int array))> =
        Map.ofArray [|
            nameof IndexedList.sortRange, struct (IndexedList.sortRange, ListOracles.stableSort compare)
            (nameof IndexedList.sortRangeWith,
             struct (IndexedList.sortRangeWith FlatListOracles.descending, ListOracles.stableSort FlatListOracles.descending))
            (nameof IndexedList.sortRangeWithComparer,
             struct (IndexedList.sortRangeWithComparer (Comparer<int>.Create(Comparison<int> FlatListOracles.byAbsolute)),
                     ListOracles.stableSort FlatListOracles.byAbsolute))
        |]


    /// The ranges of the sorts of pairs, which an unstable sort of .NET reorders: more than 16 elements.
    static member StableRanges = seq {
        struct (0, 20)
        struct (1, 18)
        struct (3, 17)
    }

    /// Both directions of the comparison with each range of the sorts of pairs.
    static member StableRangesAndDirections = seq {
        for direction in [| SortDirection.Ascending; SortDirection.Descending |] do
            for struct (index, count) in IndexedListRangeTests.StableRanges do
                struct (direction, index, count)
    }

    /// The ranges that each function that sorts a range rejects.
    static member SortRangesOutOfRange = seq {
        for name in rangeSorts.Keys do
            for struct (index, count, fault) in ListMemberCases.RangeFaults do
                struct (name, index, count, fault)
    }

    /// The names of the functions that sort a range, as the rows of the tests that tell them apart.
    static member RangeSorts = rangeSorts.Keys
    // ---- sub ----

    [<TestMethod; DynamicData(nameof FlatListTests.SubRanges, typeof<FlatListTests>)>]
    member _.``sub returns the range of the elements as Array.sub does`` (input : int array, startIndex : int, count : int) =
        CollectionAssert.AreEqual (
            Array.sub input startIndex count,
            IndexedList.toArray (IndexedList.sub (IndexedList.ofArray input) startIndex count),
            "IndexedList.sub must return the range of the elements as Array.sub does"
        )

    [<TestMethod; DynamicData(nameof FlatListTests.SubRangesOutOfRange, typeof<FlatListTests>)>]
    member _.``sub throws for a range out of range as Array.sub does`` (input : int array, startIndex : int, count : int) =
        ExceptionAssert.Same (
            id,
            (fun () -> Array.sub input startIndex count |> ignore),
            (fun () ->
                IndexedList.sub (IndexedList.ofArray input) startIndex count
                |> ignore
            ),
            "IndexedList.sub must throw as Array.sub does for a range out of range"
        )

    [<TestMethod; DataRow(Int32.MaxValue, 1); DataRow(1, Int32.MaxValue); DataRow(Int32.MaxValue, Int32.MaxValue)>]
    member _.``sub throws for a range whose end overflows`` (startIndex : int, count : int) =
        let ex =
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(
                Action (fun () -> IndexedList.sub (several ()) startIndex count |> ignore),
                "IndexedList.sub must reject a range whose end overflows Int32.MaxValue"
            )

        Assert.AreEqual<string | null>("count", ex.ParamName, "IndexedList.sub must name the count for a range beyond the end")

    [<TestMethod; DataRow 0; DataRow 3; DataRow 8>]
    member _.``sub returns the empty indexed list for an empty range`` (startIndex : int) =
        Assert.AreSame (
            IndexedList.empty<int>,
            IndexedList.sub (several ()) startIndex 0,
            "IndexedList.sub of an empty range must return the empty indexed list"
        )

    [<TestMethod>]
    member _.``sub reports the length of the indexed list in the message for a range beyond the end`` () =
        let ex =
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(
                Action (fun () -> IndexedList.sub (several ()) 5 4 |> ignore),
                "IndexedList.sub must reject a range beyond the end"
            )

        Assert.Contains ("list.Length = 8", ex.Message, "The message of IndexedList.sub must name the length of the indexed list")

    // ---- revRange ----

    [<TestMethod; DynamicData(nameof FlatListTests.SubRanges, typeof<FlatListTests>)>]
    member _.``revRange reverses the elements of the range and keeps the other elements``
        (input : int array, index : int, count : int)
        =
        CollectionAssert.AreEqual (
            ListOracles.changeRange Array.rev input index count,
            IndexedList.toArray (IndexedList.revRange index count (IndexedList.ofArray input)),
            "IndexedList.revRange must reverse the range and keep the other elements"
        )

    [<TestMethod; DynamicData(nameof ListMemberCases.RangeFaults, typeof<ListMemberCases>)>]
    member _.``revRange throws for a range out of range naming the parameter that the BCL names``
        (index : int, count : int, fault : RangeFault)
        =
        let ex =
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(
                Action (fun () -> IndexedList.revRange index count (several ()) |> ignore),
                "IndexedList.revRange must throw ArgumentOutOfRangeException for a range out of range"
            )

        // ImmutableList<T>.Reverse(Int32, Int32) names the index for a range beyond the end
        Assert.AreEqual<string | null>(
            fault.ParamName ("index", "index"),
            ex.ParamName,
            "IndexedList.revRange must name the parameter that the BCL names"
        )

    [<TestMethod; DataRow(0, 0); DataRow(3, 1); DataRow(8, 0); DataRow(7, 1)>]
    member _.``revRange returns the input itself for a range of at most one element`` (index : int, count : int) =
        let list = several ()

        Assert.AreSame (
            list,
            IndexedList.revRange index count list,
            "IndexedList.revRange must not copy a range that has nothing to reverse"
        )

    // ---- sortRange, sortRangeWith and sortRangeWithComparer ----

    [<TestMethod; DynamicData(nameof FlatListTests.SortRanges, typeof<FlatListTests>)>]
    member _.``functions that sort a range sort the range and keep the other elements``
        (name : string, input : int array, index : int, count : int)
        =
        let struct (sortRange, sortPart) = rangeSorts[name]

        CollectionAssert.AreEqual (
            ListOracles.changeRange sortPart input index count,
            IndexedList.toArray (sortRange index count (IndexedList.ofArray input)),
            $"IndexedList.%s{name} must sort the range and keep the other elements"
        )

    [<TestMethod; DynamicData(nameof IndexedListRangeTests.SortRangesOutOfRange)>]
    member _.``functions that sort a range throw for a range out of range naming the parameter that the BCL names``
        (name : string, index : int, count : int, fault : RangeFault)
        =
        let struct (sortRange, _) = rangeSorts[name]

        let ex =
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(
                Action (fun () -> sortRange index count (several ()) |> ignore),
                $"IndexedList.%s{name} must throw ArgumentOutOfRangeException for a range out of range"
            )

        Assert.AreEqual<string | null>(
            fault.ParamName ("index", "count"),
            ex.ParamName,
            $"IndexedList.%s{name} must name the parameter that the BCL names"
        )

    [<TestMethod; DynamicData(nameof FlatListTests.SortRangesWithoutSorting, typeof<FlatListTests>)>]
    member _.``functions that sort a range return the input itself for a range of at most one element``
        (name : string, index : int, count : int)
        =
        let struct (sortRange, _) = rangeSorts[name]
        let list = several ()

        Assert.AreSame (list, sortRange index count list, $"IndexedList.%s{name} must not copy a range that needs no sorting")

    [<TestMethod; DynamicData(nameof IndexedListRangeTests.RangeSorts)>]
    member _.``functions that sort a range do not run the comparison for a range of at most one element`` (name : string) =
        let sorts : Map<string, int -> int -> IndexedList<int> -> IndexedList<int>> =
            let fail _ _ : int = failwith "The comparison must not run"

            Map.ofArray [|
                nameof IndexedList.sortRange, IndexedList.sortRange
                nameof IndexedList.sortRangeWith, IndexedList.sortRangeWith fail
                nameof IndexedList.sortRangeWithComparer,
                IndexedList.sortRangeWithComparer (Comparer<int>.Create(Comparison<int> fail))
            |]

        // A range of one element and an empty one compare nothing
        let list = several ()
        Assert.AreSame (list, sorts[name] 2 1 list, $"IndexedList.%s{name} must not compare the one element of a range")
        Assert.AreSame (list, sorts[name] 5 0 list, $"IndexedList.%s{name} must not compare the elements of an empty range")

    [<TestMethod>]
    member _.``sortRange orders strings ordinally, as F# generic comparison does`` () =
        let list = IndexedList.ofArray [| "b"; "A"; "a"; "B"; "z" |]

        CollectionAssert.AreEqual (
            [| "A"; "B"; "a"; "b"; "z" |],
            IndexedList.toArray (IndexedList.sortRange 0 4 list),
            "IndexedList.sortRange must sort strings ordinally"
        )

    [<TestMethod; DynamicData(nameof IndexedListRangeTests.StableRanges)>]
    member _.``sortRange keeps the order of the elements that compare equal`` (index : int, count : int) =
        // The items compare by their keys only, so the tags show the order of equal items
        let items = KeyedItem.OfManyPairs ()

        CollectionAssert.AreEqual (
            ListOracles.sortedKeyedRange index count,
            KeyedItem.KeysAndTags (IndexedList.toArray (IndexedList.sortRange index count (IndexedList.ofArray items))),
            "IndexedList.sortRange must keep the order of equal elements, as List.sort does"
        )

    [<TestMethod; DynamicData(nameof IndexedListRangeTests.StableRangesAndDirections)>]
    member _.``sortRangeWith keeps the order of the elements that compare equal``
        (direction : SortDirection, index : int, count : int)
        =
        let comparison = ListOracles.compareKeys direction

        CollectionAssert.AreEqual (
            ListOracles.sortedPairRange comparison index count,
            IndexedList.toArray (IndexedList.sortRangeWith comparison index count (IndexedList.ofArray Inputs.ManyPairs)),
            "IndexedList.sortRangeWith must keep the order of equal elements, as List.sortWith does"
        )

    [<TestMethod; DynamicData(nameof IndexedListRangeTests.StableRangesAndDirections)>]
    member _.``sortRangeWithComparer keeps the order of the elements that compare equal``
        (direction : SortDirection, index : int, count : int)
        =
        let comparison = ListOracles.compareKeys direction
        let comparer = Comparer<struct (int * char)>.Create (fun left right -> comparison left right)

        CollectionAssert.AreEqual (
            ListOracles.sortedPairRange comparison index count,
            IndexedList.toArray (IndexedList.sortRangeWithComparer comparer index count (IndexedList.ofArray Inputs.ManyPairs)),
            "IndexedList.sortRangeWithComparer must keep the order of equal elements, as List.sortWith does"
        )

    [<TestMethod>]
    member _.``sortRangeWith wraps the exception of the comparison in an InvalidOperationException`` () =
        let ex =
            Assert.ThrowsExactly<InvalidOperationException>(
                Action (fun () ->
                    IndexedList.sortRangeWith (fun _ _ -> raise (NotSupportedException "The comparison fails")) 1 5 (several ())
                    |> ignore
                ),
                "IndexedList.sortRangeWith must wrap the exception of the comparison, as Array.Sort does"
            )

        Assert.IsInstanceOfType<NotSupportedException>(
            ex.InnerException,
            "The exception of the comparison must be the inner exception"
        )
        |> ignore

    // ---- binarySearchWithComparer, binarySearchRange and binarySearchRangeWithComparer ----

    [<TestMethod; DynamicData(nameof ListMemberCases.NameValues, typeof<ListMemberCases>)>]
    member _.``binarySearchWithComparer searches in the order of the comparer`` (value : string) =
        let comparer = StringComparer.OrdinalIgnoreCase :> IComparer<string>

        Assert.AreEqual<int>(
            Array.BinarySearch (SortedInputs.Names, value, comparer),
            IndexedList.binarySearchWithComparer comparer value (IndexedList.ofArray SortedInputs.Names),
            "IndexedList.binarySearchWithComparer must search in the order of the comparer"
        )

    [<TestMethod>]
    member _.``binarySearchWithComparer finds in the order of the comparer what the order of F# generic comparison loses`` () =
        let list = IndexedList.ofArray SortedInputs.Names

        Assert.AreEqual<int>(
            1,
            IndexedList.binarySearchWithComparer StringComparer.OrdinalIgnoreCase "b" list,
            "IndexedList.binarySearchWithComparer must find b equal to B"
        )

        Assert.IsLessThan (
            0,
            IndexedList.binarySearch "b" list,
            "IndexedList.binarySearch compares ordinally and must not find b in strings sorted when the case is ignored"
        )

    [<TestMethod; DynamicData(nameof ListMemberCases.SearchRanges, typeof<ListMemberCases>)>]
    member _.``binarySearchRange searches the range as Array.BinarySearch does`` (index : int, count : int, value : int) =
        Assert.AreEqual<int>(
            Array.BinarySearch (SortedInputs.Ascending, index, count, value),
            IndexedList.binarySearchRange index count value (IndexedList.ofArray SortedInputs.Ascending),
            "IndexedList.binarySearchRange must search the range as Array.BinarySearch does"
        )

    [<TestMethod; DynamicData(nameof ListMemberCases.SearchRanges, typeof<ListMemberCases>)>]
    member _.``binarySearchRangeWithComparer searches the range in the order of the comparer``
        (index : int, count : int, value : int)
        =
        let descending = Array.rev SortedInputs.Ascending

        Assert.AreEqual<int>(
            Array.BinarySearch (descending, index, count, value, SortedInputs.DescendingComparer),
            IndexedList.binarySearchRangeWithComparer
                SortedInputs.DescendingComparer
                index
                count
                value
                (IndexedList.ofArray descending),
            "IndexedList.binarySearchRangeWithComparer must search the range in the order of the comparer"
        )

    [<TestMethod; DynamicData(nameof ListMemberCases.RangeFaults, typeof<ListMemberCases>)>]
    member _.``the binary searches of a range throw for a range out of range`` (index : int, count : int, fault : RangeFault) =
        let list = IndexedList.ofArray SortedInputs.Ascending

        let actions : (string * (unit -> unit)) array = [|
            nameof IndexedList.binarySearchRange, (fun () -> IndexedList.binarySearchRange index count 7 list |> ignore)
            (nameof IndexedList.binarySearchRangeWithComparer,
             (fun () ->
                 IndexedList.binarySearchRangeWithComparer Comparer<int>.Default index count 7 list
                 |> ignore
             ))
        |]

        for name, action in actions do
            match fault with
            | RangeFault.Beyond ->
                // The BCL does not check this and its documentation promises ArgumentException (ADR 0001, D14)
                let ex =
                    Assert.ThrowsExactly<ArgumentException>(
                        Action action,
                        $"IndexedList.%s{name} must throw ArgumentException for a range beyond the end"
                    )

                Assert.AreEqual<string | null>("count", ex.ParamName, $"IndexedList.%s{name} must name the count")
            | _ ->
                let ex =
                    Assert.ThrowsExactly<ArgumentOutOfRangeException>(
                        Action action,
                        $"IndexedList.%s{name} must throw ArgumentOutOfRangeException for a negative number"
                    )

                Assert.AreEqual<string | null>(
                    fault.ParamName ("index", "count"),
                    ex.ParamName,
                    $"IndexedList.%s{name} must name the parameter that the BCL names"
                )

    [<TestMethod>]
    member _.``the binary searches do not wrap the exception of the comparer`` () =
        let comparer = Comparer<int>.Create (fun _ _ -> raise (NotSupportedException "The comparison fails"))
        let list = IndexedList.ofArray SortedInputs.Ascending

        Assert.ThrowsExactly<NotSupportedException>(
            Action (fun () ->
                IndexedList.binarySearchWithComparer comparer 7 list
                |> ignore
            ),
            "IndexedList.binarySearchWithComparer must not wrap the exception of the comparer"
        )
        |> ignore

        Assert.ThrowsExactly<NotSupportedException>(
            Action (fun () ->
                IndexedList.binarySearchRangeWithComparer comparer 0 4 7 list
                |> ignore
            ),
            "IndexedList.binarySearchRangeWithComparer must not wrap the exception of the comparer"
        )
        |> ignore

    // ---- tryFindIndexRange and tryFindIndexBackRange ----

    [<TestMethod; DynamicData(nameof ListMemberCases.ForwardRanges, typeof<ListMemberCases>)>]
    member _.``tryFindIndexRange returns the index of the first element of the range that satisfies the predicate``
        (startIndex : int, count : int, value : string)
        =
        let expected = FlatListOracles.firstInRange (=) startIndex count value ListNames.Several

        Assert.AreEqual<int voption>(
            (if expected < 0 then ValueNone else ValueSome expected),
            IndexedList.tryFindIndexRange startIndex count (fun item -> item = value) (IndexedList.ofArray ListNames.Several),
            "IndexedList.tryFindIndexRange must return the index of the first element of the range that satisfies the predicate"
        )

    [<TestMethod; DynamicData(nameof ListMemberCases.BackwardRanges, typeof<ListMemberCases>)>]
    member _.``tryFindIndexBackRange returns the index of the last element of the range that satisfies the predicate``
        (startIndex : int, count : int, value : string)
        =
        let expected = ListOracles.lastIndexOfRange (=) startIndex count value ListNames.Several

        Assert.AreEqual<int voption>(
            (if expected < 0 then ValueNone else ValueSome expected),
            IndexedList.tryFindIndexBackRange startIndex count (fun item -> item = value) (IndexedList.ofArray ListNames.Several),
            "IndexedList.tryFindIndexBackRange must return the index of the last element of the range that satisfies the predicate"
        )

    [<TestMethod; DynamicData(nameof ListMemberCases.RangeFaults, typeof<ListMemberCases>)>]
    member _.``tryFindIndexRange throws for a range out of range`` (startIndex : int, count : int, fault : RangeFault) =
        let ex =
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(
                Action (fun () ->
                    IndexedList.tryFindIndexRange startIndex count (fun _ -> true) (several ())
                    |> ignore
                ),
                "IndexedList.tryFindIndexRange must throw ArgumentOutOfRangeException for a range out of range"
            )

        Assert.AreEqual<string | null>(
            fault.ParamName ("startIndex", "count"),
            ex.ParamName,
            "IndexedList.tryFindIndexRange must name the offending parameter"
        )

    [<TestMethod; DynamicData(nameof ListMemberCases.BackwardRangeFaults, typeof<ListMemberCases>)>]
    member _.``tryFindIndexBackRange throws for a range out of range`` (startIndex : int, count : int, fault : RangeFault) =
        let ex =
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(
                Action (fun () ->
                    IndexedList.tryFindIndexBackRange startIndex count (fun _ -> true) (several ())
                    |> ignore
                ),
                "IndexedList.tryFindIndexBackRange must throw ArgumentOutOfRangeException for a range out of range"
            )

        Assert.AreEqual<string | null>(
            fault.ParamName ("startIndex", "count"),
            ex.ParamName,
            "IndexedList.tryFindIndexBackRange must name the offending parameter"
        )

    [<TestMethod>]
    member _.``tryFindIndexRange and tryFindIndexBackRange do not call the predicate outside the range`` () =
        let visited = ResizeArray<int>()

        let record item =
            visited.Add item
            false

        IndexedList.tryFindIndexRange 2 3 record (several ())
        |> ignore
        CollectionAssert.AreEqual (
            [| 8; 5; 0 |],
            visited.ToArray (),
            "tryFindIndexRange must test the elements of the range in order"
        )

        visited.Clear ()
        IndexedList.tryFindIndexBackRange 4 3 record (several ())
        |> ignore
        CollectionAssert.AreEqual (
            [| 0; 5; 8 |],
            visited.ToArray (),
            "tryFindIndexBackRange must test the elements of the range from its last element to its first"
        )
