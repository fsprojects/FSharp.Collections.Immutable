namespace FSharp.Collections.Immutable.Tests

open System
open System.Collections.Generic
open FSharp.Collections.Immutable
open FSharp.Collections.Immutable.Tuple
open Microsoft.VisualStudio.TestTools.UnitTesting

// The type annotations of the results are the proof that the functions of the Tuple namespace shadow the default ones.

/// The `HashMap` module of the `Tuple` namespace.
[<TestClass; HashMapTestCategory; TupleNamespaceTestCategory>]
type HashMapTupleTests () =

    /// The bindings of the inputs as reference pairs.
    static let pairs (bindings : struct (int * string) array) = bindings |> Array.map MapInputs.ToTuple

    static let table = FSharp.Collections.Immutable.HashMap.ofSeq MapInputs.Several

    /// The reference pairs of a map, sorted by key.
    static let sortedPairs (table : HashMap<int, 'T>) = HashMap.toArray table |> Array.sortBy fst

    [<TestMethod>]
    member _.``partition returns a reference tuple`` () =
        let (accepted : HashMap<int, string>), (rejected : HashMap<int, string>) =
            HashMap.partition (fun key _ -> key > 0) table

        let expectedAccepted, expectedRejected =
            Map.partition (fun key _ -> key > 0) (Map.ofArray (pairs MapInputs.Several))

        CollectionAssert.AreEqual (
            Map.toArray expectedAccepted,
            sortedPairs accepted,
            "Tuple.HashMap.partition must return the accepted bindings"
        )

        CollectionAssert.AreEqual (
            Map.toArray expectedRejected,
            sortedPairs rejected,
            "Tuple.HashMap.partition must return the rejected bindings"
        )

    [<TestMethod>]
    member _.``toArray returns reference tuples in the order of enumeration`` () =
        let array : (int * string) array = HashMap.toArray table

        CollectionAssert.AreEqual (
            pairs (FSharp.Collections.Immutable.HashMap.toArray table),
            array,
            "Tuple.HashMap.toArray must follow the enumeration"
        )

    [<TestMethod>]
    member _.``toList returns reference tuples in the order of enumeration`` () =
        let list : (int * string) list = HashMap.toList table

        CollectionAssert.AreEqual (
            pairs (FSharp.Collections.Immutable.HashMap.toArray table),
            List.toArray list,
            "Tuple.HashMap.toList must follow the enumeration"
        )

    [<TestMethod>]
    member _.``toSeq returns reference tuples in the order of enumeration`` () =
        let sequence : seq<int * string> = HashMap.toSeq table

        CollectionAssert.AreEqual (
            pairs (FSharp.Collections.Immutable.HashMap.toArray table),
            Seq.toArray sequence,
            "Tuple.HashMap.toSeq must follow the enumeration"
        )

    [<TestMethod>]
    member _.``the functions without a tuple keep resolving to the default module`` () =
        let found : string voption = HashMap.tryFind 5 table
        let replacements : KeyValuePair<int, string> array = [| KeyValuePair (5, "five again") |]
        let set : HashMap<int, string> = HashMap.setItems replacements table
        Assert.AreEqual<string voption>(ValueSome "five", found, "HashMap.tryFind must resolve to the default module")

        Assert.AreEqual<string>(
            "five again",
            FSharp.Collections.Immutable.HashMap.find 5 set,
            "HashMap.setItems must resolve to the default module"
        )

    [<TestMethod>]
    member _.``the creation members keep resolving to the extensions of the type`` () =
        // The module of the namespace has no ofArray, ofList or ofSeq, so they reach the overloads of HashMapExtensions
        let bindings = pairs MapInputs.RepeatedKeys
        let expected = Map.toArray (Map.ofArray bindings)
        let ofArray : HashMap<int, string> = HashMap.ofArray bindings
        let ofList : HashMap<int, string> = HashMap.ofList (List.ofArray bindings)
        let ofSeq : HashMap<int, string> = bindings |> HashMap.ofSeq
        CollectionAssert.AreEqual (expected, sortedPairs ofArray, "HashMap.ofArray must take reference tuples")
        CollectionAssert.AreEqual (expected, sortedPairs ofList, "HashMap.ofList must take reference tuples")
        CollectionAssert.AreEqual (expected, sortedPairs ofSeq, "HashMap.ofSeq must take reference tuples")

    [<TestMethod>]
    member _.``the nested modules of the default module resolve after opening the namespace`` () =
        // The module of the namespace has no nested modules, so HashMap.Tuple and HashMap.ValueTuple reach the default module
        let tuplesAdded : HashMap<int, string> = HashMap.Tuple.addRange [| 1, "one"; 100, "hundred" |] table
        let tuplesSet : HashMap<int, string> = HashMap.Tuple.setItems [| 5, "five again" |] table
        let structsAdded : HashMap<int, string> = HashMap.ValueTuple.addRange [| struct (1, "one") |] table
        let structsSet : HashMap<int, string> = HashMap.ValueTuple.setItems [| struct (5, "five again") |] table

        CollectionAssert.AreEqual (
            Map.toArray (Map.ofArray [| yield! pairs MapInputs.Several; 1, "one"; 100, "hundred" |]),
            sortedPairs tuplesAdded,
            "HashMap.Tuple.addRange must add the bindings"
        )

        Assert.AreEqual<string>(
            "five again",
            FSharp.Collections.Immutable.HashMap.find 5 tuplesSet,
            "HashMap.Tuple.setItems must set the bindings"
        )

        Assert.AreEqual<string>(
            "one",
            FSharp.Collections.Immutable.HashMap.find 1 structsAdded,
            "HashMap.ValueTuple.addRange must add the bindings"
        )

        Assert.AreEqual<string>(
            "five again",
            FSharp.Collections.Immutable.HashMap.find 5 structsSet,
            "HashMap.ValueTuple.setItems must set the bindings"
        )

        Assert.ThrowsExactly<ArgumentException>(
            Action (fun () -> HashMap.Tuple.addRange [| 5, "other" |] table |> ignore),
            "HashMap.Tuple.addRange must reject another value of a key, as the BCL AddRange does"
        )
        |> ignore
