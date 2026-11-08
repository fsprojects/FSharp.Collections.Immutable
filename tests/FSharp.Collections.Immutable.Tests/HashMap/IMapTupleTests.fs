namespace FSharp.Collections.Immutable.Tests

open System
open FSharp.Collections.Immutable
open FSharp.Collections.Immutable.Tuple
open Microsoft.VisualStudio.TestTools.UnitTesting

// The type annotations of the results are the proof that the functions of the Tuple namespace shadow the default ones.

/// The `IMap` module of the `Tuple` namespace.
[<TestClass; IMapTestCategory; TupleNamespaceTestCategory>]
type IMapTupleTests () =

    /// The bindings of the inputs as reference pairs.
    static let pairs (bindings : struct (int * string) array) = bindings |> Array.map MapInputs.ToTuple

    static let table = FSharp.Collections.Immutable.HashMap.ofSeq MapInputs.Several :> IMap<int, string>

    /// The reference pairs of a map, sorted by key.
    static let sortedPairs (table : IMap<int, string>) = IMap.toArray table |> Array.sortBy fst

    [<TestMethod>]
    member _.``partition returns a reference tuple`` () =
        let (accepted : IMap<int, string>), (rejected : IMap<int, string>) =
            IMap.partition (fun key _ -> key > 0) table

        let expectedAccepted, expectedRejected =
            Map.partition (fun key _ -> key > 0) (Map.ofArray (pairs MapInputs.Several))

        CollectionAssert.AreEqual (
            Map.toArray expectedAccepted,
            sortedPairs accepted,
            "Tuple.IMap.partition must return the accepted bindings"
        )

        CollectionAssert.AreEqual (
            Map.toArray expectedRejected,
            sortedPairs rejected,
            "Tuple.IMap.partition must return the rejected bindings"
        )

    [<TestMethod>]
    member _.``toArray returns reference tuples in the order of enumeration`` () =
        let array : (int * string) array = IMap.toArray table

        CollectionAssert.AreEqual (
            pairs (FSharp.Collections.Immutable.IMap.toArray table),
            array,
            "Tuple.IMap.toArray must follow the enumeration"
        )

    [<TestMethod>]
    member _.``toList returns reference tuples in the order of enumeration`` () =
        let list : (int * string) list = IMap.toList table

        CollectionAssert.AreEqual (
            pairs (FSharp.Collections.Immutable.IMap.toArray table),
            List.toArray list,
            "Tuple.IMap.toList must follow the enumeration"
        )

    [<TestMethod>]
    member _.``toSeq returns reference tuples in the order of enumeration`` () =
        let sequence : seq<int * string> = IMap.toSeq table

        CollectionAssert.AreEqual (
            pairs (FSharp.Collections.Immutable.IMap.toArray table),
            Seq.toArray sequence,
            "Tuple.IMap.toSeq must follow the enumeration"
        )

    [<TestMethod>]
    member _.``the functions without a tuple keep resolving to the default module`` () =
        let found : string voption = IMap.tryFind 5 table
        Assert.AreEqual<string voption>(ValueSome "five", found, "IMap.tryFind must resolve to the default module")

    [<TestMethod>]
    member _.``the nested modules of the default module resolve after opening the namespace`` () =
        // The module of the namespace has no nested modules, so IMap.Tuple and IMap.ValueTuple reach the default module
        let tuplesAdded : IMap<int, string> = IMap.Tuple.addRange [| 1, "one"; 100, "hundred" |] table
        let tuplesSet : IMap<int, string> = IMap.Tuple.setItems [| 5, "five again" |] table
        let structsAdded : IMap<int, string> = IMap.ValueTuple.addRange [| struct (1, "one") |] table
        let structsSet : IMap<int, string> = IMap.ValueTuple.setItems [| struct (5, "five again") |] table

        CollectionAssert.AreEqual (
            Map.toArray (Map.ofArray [| yield! pairs MapInputs.Several; 1, "one"; 100, "hundred" |]),
            sortedPairs tuplesAdded,
            "IMap.Tuple.addRange must add the bindings"
        )

        Assert.AreEqual<string>(
            "five again",
            FSharp.Collections.Immutable.IMap.find 5 tuplesSet,
            "IMap.Tuple.setItems must set the bindings"
        )

        Assert.AreEqual<string>(
            "one",
            FSharp.Collections.Immutable.IMap.find 1 structsAdded,
            "IMap.ValueTuple.addRange must add the bindings"
        )

        Assert.AreEqual<string>(
            "five again",
            FSharp.Collections.Immutable.IMap.find 5 structsSet,
            "IMap.ValueTuple.setItems must set the bindings"
        )

        Assert.ThrowsExactly<ArgumentException>(
            Action (fun () -> IMap.Tuple.addRange [| 5, "other" |] table |> ignore),
            "IMap.Tuple.addRange must reject another value of a key, as the BCL AddRange does"
        )
        |> ignore
