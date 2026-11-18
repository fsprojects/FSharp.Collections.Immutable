namespace FSharp.Collections.Immutable.Tests

open System.Collections.Generic
open FSharp.Collections.Immutable
open FSharp.Collections.Immutable.Tuple
open Microsoft.VisualStudio.TestTools.UnitTesting

// The type annotations of the results are the proof that the functions of the Tuple namespace shadow the default ones.

/// The `SortedMap` module of the `Tuple` namespace.
[<TestClass; SortedMapTestCategory; TupleNamespaceTestCategory>]
type SortedMapTupleTests () =

    /// The bindings of the inputs as reference pairs.
    static let pairs (bindings : struct (int * string) array) = bindings |> Array.map MapInputs.ToTuple

    static let table = FSharp.Collections.Immutable.SortedMap.ofSeq MapInputs.Several

    static let oracle = Map.ofArray (pairs MapInputs.Several)

    /// The creations of a sorted map from reference tuples, by the member of the extensions that they reach; the type
    /// annotation proves that each one creates a sorted map.
    static let creations : Map<string, (int * string) array -> SortedMap<int, string>> =
        Map.ofArray [|
            "ofArray", (fun bindings -> SortedMap.ofArray bindings)
            // The member under test takes a list
            "ofList", (fun bindings -> SortedMap.ofList (List.ofArray bindings))
            "ofSeq", (fun bindings -> bindings |> SortedMap.ofSeq)
        |]

    /// The changes of the sorted map by the functions of the nested modules of the default module, by their names; each one
    /// adds the bindings of 1 and 100.
    static let nestedModuleChanges : Map<string, SortedMap<int, string> -> SortedMap<int, string>> =
        Map.ofArray [|
            "Tuple.addRange", SortedMap.Tuple.addRange [| 1, "one"; 100, "hundred" |]
            "Tuple.setItems", SortedMap.Tuple.setItems [| 1, "one"; 100, "hundred" |]
            "ValueTuple.addRange", SortedMap.ValueTuple.addRange [| struct (1, "one"); struct (100, "hundred") |]
            "ValueTuple.setItems", SortedMap.ValueTuple.setItems [| struct (1, "one"); struct (100, "hundred") |]
        |]

    /// The names of the creations from reference tuples, as the rows of the test of the creation members.
    static member Creations = creations.Keys

    /// The names of the functions of the nested modules, as the rows of the test of their resolution.
    static member NestedModuleChanges = nestedModuleChanges.Keys

    [<TestMethod>]
    member _.``maxKeyValue returns a reference tuple as Map.maxKeyValue does`` () =
        let largest : int * string = SortedMap.maxKeyValue table
        Assert.AreEqual<int * string>(Map.maxKeyValue oracle, largest, "Tuple.SortedMap.maxKeyValue must match Map.maxKeyValue")

        OverrideAssert.PairMatches (
            FSharp.Collections.Immutable.SortedMap.maxKeyValue table,
            largest,
            "Tuple.SortedMap.maxKeyValue must match the default"
        )

        ExceptionAssert.Same (
            id,
            (fun () -> Map.maxKeyValue Map.empty<int, string> |> ignore),
            (fun () ->
                SortedMap.maxKeyValue FSharp.Collections.Immutable.SortedMap.empty<int, string>
                |> ignore
            ),
            "Tuple.SortedMap.maxKeyValue must reject an empty map as Map.maxKeyValue does"
        )

    [<TestMethod>]
    member _.``minKeyValue returns a reference tuple as Map.minKeyValue does`` () =
        let smallest : int * string = SortedMap.minKeyValue table
        Assert.AreEqual<int * string>(Map.minKeyValue oracle, smallest, "Tuple.SortedMap.minKeyValue must match Map.minKeyValue")

        OverrideAssert.PairMatches (
            FSharp.Collections.Immutable.SortedMap.minKeyValue table,
            smallest,
            "Tuple.SortedMap.minKeyValue must match the default"
        )

        ExceptionAssert.Same (
            id,
            (fun () -> Map.minKeyValue Map.empty<int, string> |> ignore),
            (fun () ->
                SortedMap.minKeyValue FSharp.Collections.Immutable.SortedMap.empty<int, string>
                |> ignore
            ),
            "Tuple.SortedMap.minKeyValue must reject an empty map as Map.minKeyValue does"
        )

    [<TestMethod>]
    member _.``partition returns a reference tuple`` () =
        let (accepted : SortedMap<int, string>), (rejected : SortedMap<int, string>) =
            SortedMap.partition (fun key _ -> key > 0) table

        let expectedAccepted, expectedRejected = Map.partition (fun key _ -> key > 0) oracle

        CollectionAssert.AreEqual (
            Map.toArray expectedAccepted,
            SortedMap.toArray accepted,
            "Tuple.SortedMap.partition must return the accepted bindings"
        )

        CollectionAssert.AreEqual (
            Map.toArray expectedRejected,
            SortedMap.toArray rejected,
            "Tuple.SortedMap.partition must return the rejected bindings"
        )

    [<TestMethod>]
    member _.``toArray returns reference tuples in the order of the keys`` () =
        let array : (int * string) array = SortedMap.toArray table

        CollectionAssert.AreEqual (Map.toArray oracle, array, "Tuple.SortedMap.toArray must follow the order of Map.toArray")

    [<TestMethod>]
    member _.``toList returns reference tuples in the order of the keys`` () =
        let list : (int * string) list = SortedMap.toList table

        CollectionAssert.AreEqual (
            Map.toArray oracle,
            List.toArray list,
            "Tuple.SortedMap.toList must follow the order of Map.toList"
        )

    [<TestMethod>]
    member _.``toSeq returns reference tuples in the order of the keys`` () =
        let sequence : seq<int * string> = SortedMap.toSeq table

        CollectionAssert.AreEqual (
            Seq.toArray (Map.toSeq oracle),
            Seq.toArray sequence,
            "Tuple.SortedMap.toSeq must follow the order of Map.toSeq"
        )

    [<TestMethod>]
    member _.``the functions without a tuple keep resolving to the default module`` () =
        let found : string voption = SortedMap.tryFind 5 table
        let replacements : KeyValuePair<int, string> array = [| KeyValuePair (5, "five again") |]
        let set : SortedMap<int, string> = SortedMap.setItems replacements table
        Assert.AreEqual<string voption>(ValueSome "five", found, "SortedMap.tryFind must resolve to the default module")

        Assert.AreEqual<string>(
            "five again",
            FSharp.Collections.Immutable.SortedMap.find 5 set,
            "SortedMap.setItems must resolve to the default module"
        )

    [<TestMethod; DynamicData(nameof SortedMapTupleTests.Creations)>]
    member _.``the creation members keep resolving to the extensions of the type`` (creation : string) =
        // The module of the namespace has no ofArray, ofList or ofSeq, so they reach the overloads of SortedMapExtensions
        let bindings = pairs MapInputs.RepeatedKeys

        CollectionAssert.AreEqual (
            Map.toArray (Map.ofArray bindings),
            SortedMap.toArray (creations[creation] bindings),
            $"SortedMap.%s{creation} must take reference tuples and keep the last binding of a key in the order of the keys"
        )

    [<TestMethod; DynamicData(nameof SortedMapTupleTests.NestedModuleChanges)>]
    member _.``the nested modules of the default module resolve after opening the namespace`` (change : string) =
        // The module of the namespace has no nested modules, so SortedMap.Tuple and SortedMap.ValueTuple reach the default
        // module
        let changed : SortedMap<int, string> = nestedModuleChanges[change] table

        CollectionAssert.AreEqual (
            Map.toArray (oracle |> Map.add 1 "one" |> Map.add 100 "hundred"),
            SortedMap.toArray changed,
            $"SortedMap.%s{change} must add the bindings in the order of the keys"
        )
