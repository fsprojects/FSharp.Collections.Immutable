namespace FSharp.Collections.Immutable.Tests

open System
open System.Collections.Generic
open Microsoft.VisualStudio.TestTools.UnitTesting

/// <summary>
/// The functions of a module that wraps members of <see cref="T:System.Collections.Immutable.IImmutableList`1"/>, for the
/// contract tests, over <see cref="T:System.String"/> elements.
/// </summary>
/// <remarks>
/// The strings tell the default equality from an equality that ignores the case. The module and its interface module have
/// these functions, and the module of the builder works on a builder, which has its own tests.
/// </remarks>
type ListMembersOps<'C> = {
    /// The name of the module, for the messages of the assertions.
    Name : string
    /// Creates a collection that enumerates the given elements in their order.
    OfSeq : seq<string> -> 'C
    /// The elements in the order in which the collection enumerates them.
    ToArray : 'C -> string array

    IndexOfRange : int -> int -> string -> 'C -> int
    IndexOfRangeWith : IEqualityComparer<string> -> int -> int -> string -> 'C -> int
    LastIndexOfRange : int -> int -> string -> 'C -> int
    LastIndexOfRangeWith : IEqualityComparer<string> -> int -> int -> string -> 'C -> int

    Remove : string -> 'C -> 'C
    RemoveMany : seq<string> -> 'C -> 'C
    RemoveManyWith : IEqualityComparer<string> -> seq<string> -> 'C -> 'C
    RemoveWith : IEqualityComparer<string> -> string -> 'C -> 'C
    Replace : string -> string -> 'C -> 'C
    ReplaceWith : IEqualityComparer<string> -> string -> string -> 'C -> 'C
}

/// <summary>
/// The contract of the functions that wrap members of <see cref="T:System.Collections.Immutable.IImmutableList`1"/>: the
/// removal and the replacement of a value, and the search in a range.
/// <see cref="T:FSharp.Collections.Immutable.IndexedList"/> and <see cref="T:FSharp.Collections.Immutable.IIndexedList"/>
/// have them (ADR 0001, D14).
/// </summary>
/// <remarks>
/// <see cref="T:FSharp.Collections.Immutable.FlatList"/> is not a list of this contract: it has no interface, and the tests
/// of its own removal, replacement and search are in its tests.
/// </remarks>
[<AbstractClass>]
type ListMembersContract<'C> (ops : ListMembersOps<'C>) =

    /// An equality comparer that finds strings equal when the case is ignored.
    static let ignoringCaseEquality : IEqualityComparer<string> = StringComparer.OrdinalIgnoreCase

    /// The replacement value of the tests; no input has it.
    static let replacement = "X"

    /// The functions that search forward in a range, by name.
    let forwardSearches : Map<string, int -> int -> string -> 'C -> int> =
        Map.ofArray [|
            "indexOfRange", ops.IndexOfRange
            "indexOfRangeWith", ops.IndexOfRangeWith ignoringCaseEquality
        |]

    /// The functions that search backward in a range, by name.
    let backwardSearches : Map<string, int -> int -> string -> 'C -> int> =
        Map.ofArray [|
            "lastIndexOfRange", ops.LastIndexOfRange
            "lastIndexOfRangeWith", ops.LastIndexOfRangeWith ignoringCaseEquality
        |]

    /// The functions that remove several values, by name.
    let removals : Map<string, seq<string> -> 'C -> 'C> =
        Map.ofArray [| "removeMany", ops.RemoveMany; "removeManyWith", ops.RemoveManyWith ignoringCaseEquality |]

    // ---- remove and removeWith ----

    [<TestMethod; DynamicData(nameof ListMemberCases.Values, typeof<ListMemberCases>)>]
    member _.``remove removes the first equal element`` (input : string array, value : string) =
        CollectionAssert.AreEqual (
            FlatListOracles.removeFirst (=) value input,
            ops.ToArray (ops.Remove value (ops.OfSeq input)),
            $"%s{ops.Name}.remove must remove the first equal element, in the default equality"
        )

    [<TestMethod; DynamicData(nameof ListMemberCases.AbsentValues, typeof<ListMemberCases>)>]
    member _.``remove returns the input itself when no element equals the value`` (input : string array, value : string) =
        let collection = ops.OfSeq input

        Assert.AreSame (
            box collection,
            box (ops.Remove value collection),
            $"%s{ops.Name}.remove must return a collection that has no equal element as it is"
        )

    [<TestMethod; DynamicData(nameof ListMemberCases.Values, typeof<ListMemberCases>)>]
    member _.``removeWith removes the first element that the comparer finds equal`` (input : string array, value : string) =
        CollectionAssert.AreEqual (
            FlatListOracles.removeFirst FlatListOracles.equalIgnoringCase value input,
            ops.ToArray (ops.RemoveWith ignoringCaseEquality value (ops.OfSeq input)),
            $"%s{ops.Name}.removeWith must remove the first element that the comparer finds equal"
        )

    [<TestMethod; DynamicData(nameof ListMemberCases.AbsentValuesIgnoringCase, typeof<ListMemberCases>)>]
    member _.``removeWith returns the input itself when the comparer finds no element equal``
        (input : string array, value : string)
        =
        let collection = ops.OfSeq input

        Assert.AreSame (
            box collection,
            box (ops.RemoveWith ignoringCaseEquality value collection),
            $"%s{ops.Name}.removeWith must return a collection that has no equal element as it is"
        )

    // ---- removeMany and removeManyWith ----

    [<TestMethod; DynamicData(nameof ListMemberCases.ValueSequences, typeof<ListMemberCases>)>]
    member _.``removeMany removes one element for each value`` (input : string array, items : string array) =
        CollectionAssert.AreEqual (
            ListOracles.removeEach (=) items input,
            ops.ToArray (ops.RemoveMany items (ops.OfSeq input)),
            $"%s{ops.Name}.removeMany must remove one element for each value"
        )

    [<TestMethod>]
    member _.``removeMany takes the values as any sequence`` () =
        let collection = ops.OfSeq ListNames.Several
        // The values "a", "B" and "a" remove the first "a", the only "B" and the second "a"
        let expected = [| "b"; "c"; "b"; "A"; "d" |]

        CollectionAssert.AreEqual (
            expected,
            ops.ToArray (ops.RemoveMany [| "a"; "B"; "a" |] collection),
            $"%s{ops.Name}.removeMany must take the values as an array"
        )

        CollectionAssert.AreEqual (
            expected,
            ops.ToArray (ops.RemoveMany [ "a"; "B"; "a" ] collection),
            $"%s{ops.Name}.removeMany must take the values as a list"
        )

        CollectionAssert.AreEqual (
            expected,
            ops.ToArray (ops.RemoveMany (Seq.map id [| "a"; "B"; "a" |]) collection),
            $"%s{ops.Name}.removeMany must take the values as a lazy sequence"
        )

    [<TestMethod; DynamicData(nameof ListMemberCases.AbsentValueSequences, typeof<ListMemberCases>)>]
    member _.``removeMany returns the input itself when no value is in it`` (input : string array, items : string array) =
        let collection = ops.OfSeq input

        Assert.AreSame (
            box collection,
            box (ops.RemoveMany items collection),
            $"%s{ops.Name}.removeMany must return a collection that has none of the values as it is"
        )

    [<TestMethod; DynamicData(nameof ListMemberCases.ValueSequences, typeof<ListMemberCases>)>]
    member _.``removeManyWith removes one element for each value that the comparer finds equal``
        (input : string array, items : string array)
        =
        CollectionAssert.AreEqual (
            ListOracles.removeEach FlatListOracles.equalIgnoringCase items input,
            ops.ToArray (ops.RemoveManyWith ignoringCaseEquality items (ops.OfSeq input)),
            $"%s{ops.Name}.removeManyWith must remove one element for each value that the comparer finds equal"
        )

    [<TestMethod; DynamicData(nameof ListMemberCases.AbsentValueSequencesIgnoringCase, typeof<ListMemberCases>)>]
    member _.``removeManyWith returns the input itself when the comparer finds none of the values``
        (input : string array, items : string array)
        =
        let collection = ops.OfSeq input

        Assert.AreSame (
            box collection,
            box (ops.RemoveManyWith ignoringCaseEquality items collection),
            $"%s{ops.Name}.removeManyWith must return a collection that has none of the values as it is"
        )

    [<TestMethod; DynamicData(nameof ListMemberCases.RejectedValues, typeof<ListMemberCases>)>]
    member _.``the removals of several values reject null items and a default flat list given as the items``
        (name : string, kind : string, input : string)
        =
        let items =
            if kind = "null sequence" then
                Unchecked.defaultof<seq<string>>
            else
                Inputs.DefaultFlatListAsSeq<string>()

        let collection =
            ops.OfSeq (
                if input = "empty collection" then
                    ListNames.Empty
                else
                    ListNames.Several
            )

        // A default flat list is a boxed struct, not null: it fails with InvalidOperationException when it is enumerated
        FlatListAssert.RejectsNull (
            $"%s{ops.Name}.%s{name} of a %s{kind} on a %s{input}",
            "items",
            (fun () -> removals[name] items collection |> ignore)
        )

    [<TestMethod; DynamicData(nameof ListMemberCases.Removals, typeof<ListMemberCases>)>]
    member _.``the removals of several values check the collection before the items`` (name : string) =
        // The member cannot be called on a null collection, so the function checks the collection before the items
        FlatListAssert.RejectsNull (
            $"%s{ops.Name}.%s{name} of a null collection and a default flat list",
            "list",
            (fun () ->
                removals[name] (Inputs.DefaultFlatListAsSeq<string>()) Unchecked.defaultof<'C>
                |> ignore
            )
        )

    // ---- replace and replaceWith ----

    [<TestMethod; DynamicData(nameof ListMemberCases.PresentValues, typeof<ListMemberCases>)>]
    member _.``replace replaces the first equal element`` (input : string array, oldValue : string) =
        CollectionAssert.AreEqual (
            Assert.WantValueSome (
                FlatListOracles.replaceFirst (=) oldValue replacement input,
                "The oracle must find the old value, which the rows take from the elements"
            ),
            ops.ToArray (ops.Replace oldValue replacement (ops.OfSeq input)),
            $"%s{ops.Name}.replace must replace the first equal element, in the default equality"
        )

    [<TestMethod; DynamicData(nameof ListMemberCases.PresentValuesIgnoringCase, typeof<ListMemberCases>)>]
    member _.``replaceWith replaces the first element that the comparer finds equal`` (input : string array, oldValue : string) =
        CollectionAssert.AreEqual (
            Assert.WantValueSome (
                FlatListOracles.replaceFirst FlatListOracles.equalIgnoringCase oldValue replacement input,
                "The oracle must find the old value, which the rows take from the elements"
            ),
            ops.ToArray (ops.ReplaceWith ignoringCaseEquality oldValue replacement (ops.OfSeq input)),
            $"%s{ops.Name}.replaceWith must replace the first element that the comparer finds equal"
        )

    [<TestMethod; DynamicData(nameof ListMemberCases.AbsentValues, typeof<ListMemberCases>)>]
    member _.``replace throws ArgumentException naming the old value when no element equals it``
        (input : string array, oldValue : string)
        =
        let ex =
            Assert.ThrowsExactly<ArgumentException>(
                Action (fun () -> ops.Replace oldValue replacement (ops.OfSeq input) |> ignore),
                $"%s{ops.Name}.replace must throw ArgumentException for a value that no element equals"
            )

        Assert.AreEqual<string | null>("oldValue", ex.ParamName, $"%s{ops.Name}.replace must name the old value")

    [<TestMethod; DynamicData(nameof ListMemberCases.AbsentValuesIgnoringCase, typeof<ListMemberCases>)>]
    member _.``replaceWith throws ArgumentException naming the old value when the comparer finds no element equal``
        (input : string array, oldValue : string)
        =
        let ex =
            Assert.ThrowsExactly<ArgumentException>(
                Action (fun () ->
                    ops.ReplaceWith ignoringCaseEquality oldValue replacement (ops.OfSeq input)
                    |> ignore
                ),
                $"%s{ops.Name}.replaceWith must throw ArgumentException for a value that the comparer finds in no element"
            )

        Assert.AreEqual<string | null>("oldValue", ex.ParamName, $"%s{ops.Name}.replaceWith must name the old value")

    // ---- indexOfRange, indexOfRangeWith, lastIndexOfRange and lastIndexOfRangeWith ----

    [<TestMethod; DynamicData(nameof ListMemberCases.ForwardRanges, typeof<ListMemberCases>)>]
    member _.``indexOfRange returns the index of the first equal element of the range or -1``
        (startIndex : int, count : int, value : string)
        =
        Assert.AreEqual<int>(
            FlatListOracles.firstInRange (=) startIndex count value ListNames.Several,
            ops.IndexOfRange startIndex count value (ops.OfSeq ListNames.Several),
            $"%s{ops.Name}.indexOfRange must return the index of the first equal element of the range, or -1"
        )

    [<TestMethod; DynamicData(nameof ListMemberCases.ForwardRanges, typeof<ListMemberCases>)>]
    member _.``indexOfRangeWith compares with the equality comparer`` (startIndex : int, count : int, value : string) =
        Assert.AreEqual<int>(
            FlatListOracles.firstInRange FlatListOracles.equalIgnoringCase startIndex count value ListNames.Several,
            ops.IndexOfRangeWith ignoringCaseEquality startIndex count value (ops.OfSeq ListNames.Several),
            $"%s{ops.Name}.indexOfRangeWith must compare with the equality comparer"
        )

    [<TestMethod; DynamicData(nameof ListMemberCases.BackwardRanges, typeof<ListMemberCases>)>]
    member _.``lastIndexOfRange returns the index of the last equal element of the range or -1``
        (startIndex : int, count : int, value : string)
        =
        Assert.AreEqual<int>(
            ListOracles.lastIndexOfRange (=) startIndex count value ListNames.Several,
            ops.LastIndexOfRange startIndex count value (ops.OfSeq ListNames.Several),
            $"%s{ops.Name}.lastIndexOfRange must return the index of the last equal element of the range, or -1"
        )

    [<TestMethod; DynamicData(nameof ListMemberCases.BackwardRanges, typeof<ListMemberCases>)>]
    member _.``lastIndexOfRangeWith compares with the equality comparer`` (startIndex : int, count : int, value : string) =
        Assert.AreEqual<int>(
            ListOracles.lastIndexOfRange FlatListOracles.equalIgnoringCase startIndex count value ListNames.Several,
            ops.LastIndexOfRangeWith ignoringCaseEquality startIndex count value (ops.OfSeq ListNames.Several),
            $"%s{ops.Name}.lastIndexOfRangeWith must compare with the equality comparer"
        )

    [<TestMethod>]
    member _.``the searches in a range return -1 in an empty collection`` () =
        let empty = ops.OfSeq ListNames.Empty

        Assert.AreEqual<int>(
            -1,
            ops.IndexOfRange 0 0 "a" empty,
            $"%s{ops.Name}.indexOfRange must return -1 in an empty collection"
        )
        Assert.AreEqual<int>(
            -1,
            ops.IndexOfRangeWith ignoringCaseEquality 0 0 "a" empty,
            $"%s{ops.Name}.indexOfRangeWith must return -1 in an empty collection"
        )
        Assert.AreEqual<int>(
            -1,
            ops.LastIndexOfRange 0 0 "a" empty,
            $"%s{ops.Name}.lastIndexOfRange must return -1 in an empty collection"
        )
        Assert.AreEqual<int>(
            -1,
            ops.LastIndexOfRangeWith ignoringCaseEquality 0 0 "a" empty,
            $"%s{ops.Name}.lastIndexOfRangeWith must return -1 in an empty collection"
        )

    [<TestMethod; DynamicData(nameof ListMemberCases.ForwardRangesOutOfRange, typeof<ListMemberCases>)>]
    member _.``the searches forward in a range throw for a range out of range``
        (name : string, startIndex : int, count : int, fault : RangeFault)
        =
        let ex =
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(
                Action (fun () ->
                    forwardSearches[name] startIndex count "a" (ops.OfSeq ListNames.Several)
                    |> ignore
                ),
                $"%s{ops.Name}.%s{name} must throw ArgumentOutOfRangeException for a range out of range"
            )

        Assert.AreEqual<string | null>(
            fault.ParamName ("startIndex", "count"),
            ex.ParamName,
            $"%s{ops.Name}.%s{name} must name the offending parameter"
        )

    [<TestMethod; DynamicData(nameof ListMemberCases.BackwardRangesOutOfRange, typeof<ListMemberCases>)>]
    member _.``the searches backward in a range throw for a range out of range``
        (name : string, startIndex : int, count : int, fault : RangeFault)
        =
        let ex =
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(
                Action (fun () ->
                    backwardSearches[name] startIndex count "a" (ops.OfSeq ListNames.Several)
                    |> ignore
                ),
                $"%s{ops.Name}.%s{name} must throw ArgumentOutOfRangeException for a range out of range"
            )

        Assert.AreEqual<string | null>(
            fault.ParamName ("startIndex", "count"),
            ex.ParamName,
            $"%s{ops.Name}.%s{name} must name the offending parameter"
        )
