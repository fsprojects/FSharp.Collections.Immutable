namespace FSharp.Collections.Immutable.Tests

open System.Collections.Generic
open FSharp.Collections.Immutable
open Microsoft.VisualStudio.TestTools.UnitTesting

/// The form in which the indexed list modules give a sequence to the BCL: an F# list in a new array, because the BCL builds
/// a tree by indexing, and any other sequence as it is.
[<TestClass; IndexedListTestCategory>]
type SnapshotTests () =

    /// The sequences that are not F# lists, by name. The rows of the test are the names: a sequence has no readable name in
    /// a row.
    static let otherSequences : Map<string, seq<int>> =
        Map.ofArray [|
            "array", [| 1; 2; 3 |] :> seq<int>
            "resizable array", ResizeArray [ 1; 2; 3 ] :> seq<int>
            "indexed list", IndexedList.ofArray [| 1; 2; 3 |] :> seq<int>
            "flat list", FlatList.ofArray [| 1; 2; 3 |] :> seq<int>
            "sequence expression", seq { 1..3 }
            "set", Set.ofArray [| 1; 2; 3 |] :> seq<int>
        |]

    /// The names of the sequences that are not F# lists, as the rows of their test.
    static member OtherSequences = otherSequences.Keys

    [<TestMethod; DataRow 0; DataRow 1; DataRow 5>]
    member _.``forTree copies an F# list into an array`` (length : int) =
        let list = List.init length id
        let result = Snapshot.forTree list

        Assert.IsInstanceOfType<int array>(result, "Snapshot.forTree must give the elements of an F# list in an array")
        |> ignore

        CollectionAssert.AreEqual (
            List.toArray list,
            Seq.toArray result,
            "Snapshot.forTree must keep the elements of the list in order"
        )

    [<TestMethod; DataRow 0; DataRow 1; DataRow 5>]
    member _.``forTree copies a list of strings that is given as a sequence of objects`` (length : int) =
        let list = List.init length string
        // The list of strings itself: the runtime gives it the type of the sequence by covariance
        let widened = list |> box |> unbox<seq<obj>>
        let result = Snapshot.forTree widened

        Assert.IsInstanceOfType<obj array>(
            result,
            "Snapshot.forTree must give the elements of a list of a derived type in an array"
        )
        |> ignore

        CollectionAssert.AreEqual (
            List.toArray list,
            Seq.toArray result,
            "Snapshot.forTree must keep the elements of the list in order"
        )

    [<TestMethod; DynamicData(nameof SnapshotTests.OtherSequences)>]
    member _.``forTree returns a sequence that is not an F# list as it is`` (name : string) =
        let source = otherSequences[name]

        Assert.AreSame<seq<int>>(
            source,
            Snapshot.forTree source,
            $"Snapshot.forTree must return the %s{name} itself, because the BCL does not index it slowly"
        )

    [<TestMethod>]
    member _.``forTree returns a sequence of key and value pairs as it is`` () =
        // A dictionary is a generic type that is not a list
        let source = Dictionary<int, string>() :> seq<KeyValuePair<int, string>>
        Assert.AreSame<seq<KeyValuePair<int, string>>>(
            source,
            Snapshot.forTree source,
            "Snapshot.forTree must return a dictionary itself"
        )
