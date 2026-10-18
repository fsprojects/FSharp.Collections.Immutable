namespace FSharp.Collections.Immutable.Tests

open FSharp.Collections.Immutable
open FSharp.Collections.Immutable.Tuple
open Microsoft.VisualStudio.TestTools.UnitTesting

// The type annotations of the results are the proof that the functions of the Tuple namespace shadow the default ones.

/// The `FrozenMap` module of the `Tuple` namespace.
[<TestClass; FrozenMapTestCategory; TupleNamespaceTestCategory>]
type FrozenMapTupleTests () =

    /// The bindings of the inputs as reference pairs.
    static let pairs (bindings : struct (int * string) array) = bindings |> Array.map MapInputs.ToTuple

    static let table = FSharp.Collections.Immutable.FrozenMap.ofSeq MapInputs.Several

    /// The reference pairs of a map, sorted by key.
    static let sortedPairs (table : FrozenMap<int, 'T>) = FrozenMap.toArray table |> Array.sortBy fst

    [<TestMethod>]
    member _.``partition returns a reference tuple`` () =
        let (accepted : FrozenMap<int, string>), (rejected : FrozenMap<int, string>) =
            FrozenMap.partition (fun key _ -> key > 0) table

        let expectedAccepted, expectedRejected =
            Map.partition (fun key _ -> key > 0) (Map.ofArray (pairs MapInputs.Several))

        CollectionAssert.AreEqual (
            Map.toArray expectedAccepted,
            sortedPairs accepted,
            "Tuple.FrozenMap.partition must return the accepted bindings"
        )

        CollectionAssert.AreEqual (
            Map.toArray expectedRejected,
            sortedPairs rejected,
            "Tuple.FrozenMap.partition must return the rejected bindings"
        )

    [<TestMethod>]
    member _.``toArray returns reference tuples in the order of enumeration`` () =
        let array : (int * string) array = FrozenMap.toArray table

        CollectionAssert.AreEqual (
            pairs (FSharp.Collections.Immutable.FrozenMap.toArray table),
            array,
            "Tuple.FrozenMap.toArray must follow the enumeration"
        )

    [<TestMethod>]
    member _.``toList returns reference tuples in the order of enumeration`` () =
        let list : (int * string) list = FrozenMap.toList table

        CollectionAssert.AreEqual (
            pairs (FSharp.Collections.Immutable.FrozenMap.toArray table),
            List.toArray list,
            "Tuple.FrozenMap.toList must follow the enumeration"
        )

    [<TestMethod>]
    member _.``toSeq returns reference tuples in the order of enumeration`` () =
        let sequence : seq<int * string> = FrozenMap.toSeq table

        CollectionAssert.AreEqual (
            pairs (FSharp.Collections.Immutable.FrozenMap.toArray table),
            Seq.toArray sequence,
            "Tuple.FrozenMap.toSeq must follow the enumeration"
        )

    [<TestMethod>]
    member _.``the functions without a tuple keep resolving to the default module`` () =
        let found : string voption = FrozenMap.tryFind 5 table
        let keys : FlatList<int> = FrozenMap.keys table
        Assert.AreEqual<string voption>(ValueSome "five", found, "FrozenMap.tryFind must resolve to the default module")
        Assert.HasCount (MapInputs.Several.Length, keys, "FrozenMap.keys must resolve to the default module")

    [<TestMethod>]
    member _.``the creation members keep resolving to the extensions of the type`` () =
        // The module of the namespace has no ofArray, ofList or ofSeq, so they reach the overloads of FrozenMapExtensions
        let bindings = pairs MapInputs.RepeatedKeys
        let expected = Map.toArray (Map.ofArray bindings)
        let ofArray : FrozenMap<int, string> = FrozenMap.ofArray bindings
        let ofList : FrozenMap<int, string> = FrozenMap.ofList (List.ofArray bindings)
        let ofSeq : FrozenMap<int, string> = bindings |> FrozenMap.ofSeq
        CollectionAssert.AreEqual (expected, sortedPairs ofArray, "FrozenMap.ofArray must take reference tuples")
        CollectionAssert.AreEqual (expected, sortedPairs ofList, "FrozenMap.ofList must take reference tuples")
        CollectionAssert.AreEqual (expected, sortedPairs ofSeq, "FrozenMap.ofSeq must take reference tuples")
