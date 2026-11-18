namespace FSharp.Collections.Immutable.Tests

open System
open System.Collections.Generic
open FSharp.Collections.Immutable
open Microsoft.VisualStudio.TestTools.UnitTesting

/// The static members of `FrozenMapExtensions`, which `FrozenMap.ofArray`, `FrozenMap.ofList` and `FrozenMap.ofSeq` reach
/// through the abbreviation, called as user code calls them (ADR 0001, D13).
[<TestClass; FrozenMapTestCategory>]
type FrozenMapExtensionsTests () =

    /// The creations of a frozen map from bindings that repeat the key "a", by the member and the shape that they call.
    static let repeatedKeyCreations : Map<string, unit -> FrozenMap<string, int>> =
        Map.ofArray [|
            "ofArray of key/value pairs",
            (fun () -> FrozenMap.ofArray [| KeyValuePair ("a", 1); KeyValuePair ("b", 2); KeyValuePair ("a", 3) |])
            "ofArray of reference tuples", (fun () -> FrozenMap.ofArray [| "a", 1; "b", 2; "a", 3 |])
            "ofArray of struct tuples", (fun () -> FrozenMap.ofArray [| struct ("a", 1); struct ("b", 2); struct ("a", 3) |])
            "ofList of key/value pairs",
            (fun () -> FrozenMap.ofList [ KeyValuePair ("a", 1); KeyValuePair ("b", 2); KeyValuePair ("a", 3) ])
            "ofList of reference tuples", (fun () -> FrozenMap.ofList [ "a", 1; "b", 2; "a", 3 ])
            "ofList of struct tuples", (fun () -> FrozenMap.ofList [ struct ("a", 1); struct ("b", 2); struct ("a", 3) ])
            "ofSeq of key/value pairs",
            (fun () -> FrozenMap.ofSeq (Seq.ofArray [| KeyValuePair ("a", 1); KeyValuePair ("b", 2); KeyValuePair ("a", 3) |]))
            "ofSeq of reference tuples", (fun () -> FrozenMap.ofSeq (Seq.ofArray [| "a", 1; "b", 2; "a", 3 |]))
            "ofSeq of struct tuples",
            (fun () -> FrozenMap.ofSeq (Seq.ofArray [| struct ("a", 1); struct ("b", 2); struct ("a", 3) |]))
        |]

    /// The creations of a frozen map of "one" and "two" in a pipeline, by the member and the source of the bindings.
    static let pipelines : Map<string, unit -> FrozenMap<string, int>> =
        Map.ofArray [|
            "ofArray of an array of key/value pairs",
            (fun () ->
                [| KeyValuePair ("one", 1); KeyValuePair ("two", 2) |]
                |> FrozenMap.ofArray
            )
            "ofArray of an array of reference tuples", (fun () -> [| "one", 1; "two", 2 |] |> FrozenMap.ofArray)
            "ofArray of an array of struct tuples",
            (fun () ->
                [| struct ("one", 1); struct ("two", 2) |]
                |> FrozenMap.ofArray
            )
            "ofList of a list of key/value pairs",
            (fun () ->
                [ KeyValuePair ("one", 1); KeyValuePair ("two", 2) ]
                |> FrozenMap.ofList
            )
            "ofList of a list of reference tuples", (fun () -> [ "one", 1; "two", 2 ] |> FrozenMap.ofList)
            "ofList of a list of struct tuples", (fun () -> [ struct ("one", 1); struct ("two", 2) ] |> FrozenMap.ofList)
            "ofSeq of a sequence of key/value pairs",
            (fun () ->
                Seq.ofArray [| KeyValuePair ("one", 1); KeyValuePair ("two", 2) |]
                |> FrozenMap.ofSeq
            )
            "ofSeq of a sequence of reference tuples", (fun () -> Seq.ofArray [| "one", 1; "two", 2 |] |> FrozenMap.ofSeq)
            "ofSeq of a sequence of struct tuples",
            (fun () ->
                Seq.ofArray [| struct ("one", 1); struct ("two", 2) |]
                |> FrozenMap.ofSeq
            )
            "ofSeq of a dictionary", (fun () -> dict [| "one", 1; "two", 2 |] |> FrozenMap.ofSeq)
            "ofSeq of an F# map", (fun () -> Map.ofArray [| "one", 1; "two", 2 |] |> FrozenMap.ofSeq)
            "ofSeq of a frozen map",
            (fun () ->
                FrozenMap.ofArray [| "one", 1; "two", 2 |]
                |> FrozenMap.ofSeq
            )
        |]

    /// The creations of a frozen map from the given bindings, by the member and the shape that they call.
    static let shapedCreations : Map<string, struct (string * string)[] -> FrozenMap<string, string>> =
        Map.ofArray [|
            "ofArray of key/value pairs", (fun bindings -> FrozenMap.ofArray (Array.map MapInputs.ToKeyValuePair bindings))
            "ofArray of reference tuples", (fun bindings -> FrozenMap.ofArray (Array.map MapInputs.ToTuple bindings))
            "ofArray of struct tuples", (fun bindings -> FrozenMap.ofArray bindings)
            "ofList of key/value pairs",
            (fun bindings ->
                FrozenMap.ofList (
                    bindings
                    |> Array.map MapInputs.ToKeyValuePair
                    |> List.ofArray
                )
            )
            "ofList of reference tuples",
            (fun bindings -> FrozenMap.ofList (bindings |> Array.map MapInputs.ToTuple |> List.ofArray))
            "ofList of struct tuples", (fun bindings -> FrozenMap.ofList (List.ofArray bindings))
            "ofSeq of key/value pairs", (fun bindings -> FrozenMap.ofSeq (Seq.map MapInputs.ToKeyValuePair bindings))
            "ofSeq of reference tuples", (fun bindings -> FrozenMap.ofSeq (Seq.map MapInputs.ToTuple bindings))
            "ofSeq of struct tuples", (fun bindings -> FrozenMap.ofSeq (Seq.ofArray bindings))
        |]

    /// The bindings of a frozen map as reference tuples, sorted by key, to compare them with those of an F# map.
    static let sortedBindings (table : FrozenMap<string, int>) =
        FrozenMap.toArray table
        |> Array.map (fun struct (key, value) -> key, value)
        |> Array.sortBy fst

    /// The names of the creations from bindings that repeat a key, as the rows of the test of the last binding.
    static member RepeatedKeyCreations = repeatedKeyCreations.Keys

    /// The names of the pipelines, as the rows of the test of the pipelines.
    static member Pipelines = pipelines.Keys

    /// The names of the creations from the given bindings, as the rows of the test of the keys that the default comparer
    /// finds equal.
    static member ShapedCreations = shapedCreations.Keys

    [<TestMethod; DynamicData(nameof FrozenMapExtensionsTests.RepeatedKeyCreations)>]
    member _.``every shape keeps the last binding of a key as Map does`` (creation : string) =
        CollectionAssert.AreEqual (
            Map.toArray (Map.ofArray [| "a", 1; "b", 2; "a", 3 |]),
            sortedBindings (repeatedKeyCreations[creation]()),
            "The frozen map must keep the last binding of a key, as Map does"
        )

    [<TestMethod; DynamicData(nameof FrozenMapExtensionsTests.ShapedCreations)>]
    member _.``every shape keeps the earlier key and the later value of keys that the default comparer finds equal``
        (creation : string)
        =
        // The indexer of the dictionary keeps the earlier key and replaces the value, and ToFrozenDictionary freezes both
        // (ADR 0001, D14); the copies are equal in the default comparer and tell the two keys apart
        let firstKey = MapInputs.Copy "one"
        let secondValue = MapInputs.Copy "dos"

        let table =
            shapedCreations[creation] [| struct (firstKey, "uno"); struct (MapInputs.Copy "one", secondValue) |]

        let struct (key, value) =
            Assert.ContainsSingle (FrozenMap.toArray table, "The frozen map must merge the bindings of the equal keys")

        Assert.AreSame (firstKey, key, "The frozen map must keep the earlier key")
        Assert.AreSame (secondValue, value, "The frozen map must keep the later value")

    [<TestMethod; DynamicData(nameof FrozenMapExtensionsTests.Pipelines)>]
    member _.``the creation members resolve the shape of the bindings in a pipeline`` (pipeline : string) =
        CollectionAssert.AreEqual (
            [| "one", 1; "two", 2 |],
            sortedBindings (pipelines[pipeline]()),
            "The pipeline must create the bindings of its source"
        )

    [<TestMethod>]
    member _.``ofSeq returns a frozen map with the default comparer itself`` () =
        let table = FrozenMap.ofArray [| "one", 1; "two", 2 |]
        Assert.AreSame (table, FrozenMap.ofSeq table, "FrozenMap.ofSeq must return a frozen map with the default comparer itself")

    [<TestMethod>]
    member _.``ofSeq gives a frozen map with another comparer the default comparer`` () =
        let ignoreCase =
            (FrozenMap.forKeyComparer StringComparer.OrdinalIgnoreCase).ofArray [| "one", 1; "two", 2 |]

        let table = FrozenMap.ofSeq ignoreCase

        Assert.AreSame<IEqualityComparer<string>>(
            EqualityComparer<string>.Default,
            FrozenMap.keyComparer table,
            "FrozenMap.ofSeq must give a frozen map the default key comparer"
        )

        CollectionAssert.AreEqual (sortedBindings ignoreCase, sortedBindings table, "FrozenMap.ofSeq must keep the bindings")
        Assert.IsFalse (FrozenMap.containsKey "ONE" table, "FrozenMap.ofSeq must look the keys up with the default comparer")
