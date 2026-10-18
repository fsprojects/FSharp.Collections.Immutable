namespace FSharp.Collections.Immutable.Tests

open System
open System.Collections.Generic
open FSharp.Collections.Immutable
open Microsoft.VisualStudio.TestTools.UnitTesting

/// The static members of `HashMapExtensions`, which `HashMap.ofArray`, `HashMap.ofList` and `HashMap.ofSeq` reach through
/// the abbreviation, called as user code calls them (ADR 0001, D13).
[<TestClass; HashMapTestCategory>]
type HashMapExtensionsTests () =

    /// The creations of a hash map from bindings that repeat the key "a", by the member and the shape that they call.
    static let repeatedKeyCreations : Map<string, unit -> HashMap<string, int>> =
        Map.ofArray [|
            "ofArray of key/value pairs",
            (fun () -> HashMap.ofArray [| KeyValuePair ("a", 1); KeyValuePair ("b", 2); KeyValuePair ("a", 3) |])
            "ofArray of reference tuples", (fun () -> HashMap.ofArray [| "a", 1; "b", 2; "a", 3 |])
            "ofArray of struct tuples", (fun () -> HashMap.ofArray [| struct ("a", 1); struct ("b", 2); struct ("a", 3) |])
            "ofList of key/value pairs",
            (fun () -> HashMap.ofList [ KeyValuePair ("a", 1); KeyValuePair ("b", 2); KeyValuePair ("a", 3) ])
            "ofList of reference tuples", (fun () -> HashMap.ofList [ "a", 1; "b", 2; "a", 3 ])
            "ofList of struct tuples", (fun () -> HashMap.ofList [ struct ("a", 1); struct ("b", 2); struct ("a", 3) ])
            "ofSeq of key/value pairs",
            (fun () -> HashMap.ofSeq (Seq.ofArray [| KeyValuePair ("a", 1); KeyValuePair ("b", 2); KeyValuePair ("a", 3) |]))
            "ofSeq of reference tuples", (fun () -> HashMap.ofSeq (Seq.ofArray [| "a", 1; "b", 2; "a", 3 |]))
            "ofSeq of struct tuples",
            (fun () -> HashMap.ofSeq (Seq.ofArray [| struct ("a", 1); struct ("b", 2); struct ("a", 3) |]))
        |]

    /// The creations of a hash map of "one" and "two" in a pipeline, by the source of the bindings.
    static let pipelines : Map<string, unit -> HashMap<string, int>> =
        Map.ofArray [|
            "an array of key/value pairs",
            (fun () ->
                [| KeyValuePair ("one", 1); KeyValuePair ("two", 2) |]
                |> HashMap.ofArray
            )
            "a list of reference tuples", (fun () -> [ "one", 1; "two", 2 ] |> HashMap.ofList)
            "an array of struct tuples",
            (fun () ->
                [| struct ("one", 1); struct ("two", 2) |]
                |> HashMap.ofArray
            )
            "a sequence of reference tuples", (fun () -> Seq.ofArray [| "one", 1; "two", 2 |] |> HashMap.ofSeq)
            "a dictionary", (fun () -> dict [| "one", 1; "two", 2 |] |> HashMap.ofSeq)
            "an F# map", (fun () -> Map.ofArray [| "one", 1; "two", 2 |] |> HashMap.ofSeq)
            "a hash map", (fun () -> HashMap.ofArray [| "one", 1; "two", 2 |] |> HashMap.ofSeq)
        |]

    /// The bindings of a hash map as reference tuples, sorted by key, to compare them with those of an F# map.
    static let sortedBindings (table : HashMap<string, int>) =
        HashMap.toArray table
        |> Array.map (fun struct (key, value) -> key, value)
        |> Array.sortBy fst

    /// The creations of a hash map from the given bindings, by the member and the shape that they call.
    static let shapedCreations : Map<string, struct (string * string) array -> HashMap<string, string>> =
        Map.ofArray [|
            "ofArray of key/value pairs", (fun bindings -> HashMap.ofArray (Array.map MapInputs.ToKeyValuePair bindings))
            "ofArray of reference tuples", (fun bindings -> HashMap.ofArray (Array.map MapInputs.ToTuple bindings))
            "ofArray of struct tuples", (fun bindings -> HashMap.ofArray bindings)
            "ofList of key/value pairs",
            (fun bindings ->
                HashMap.ofList (
                    bindings
                    |> Array.map MapInputs.ToKeyValuePair
                    |> List.ofArray
                )
            )
            "ofList of reference tuples",
            (fun bindings -> HashMap.ofList (bindings |> Array.map MapInputs.ToTuple |> List.ofArray))
            "ofList of struct tuples", (fun bindings -> HashMap.ofList (List.ofArray bindings))
            "ofSeq of key/value pairs", (fun bindings -> HashMap.ofSeq (Seq.map MapInputs.ToKeyValuePair bindings))
            "ofSeq of reference tuples", (fun bindings -> HashMap.ofSeq (Seq.map MapInputs.ToTuple bindings))
            "ofSeq of struct tuples", (fun bindings -> HashMap.ofSeq (Seq.ofArray bindings))
        |]

    /// The names of the creations from bindings that repeat a key, as the rows of the test of the last binding.
    static member RepeatedKeyCreations = repeatedKeyCreations.Keys

    /// The names of the sources of the pipelines: the rows of the test of the pipelines.
    static member Pipelines = pipelines.Keys

    /// The names of the creations from the given bindings, as the rows of the test of the values that the default comparer
    /// finds equal.
    static member ShapedCreations = shapedCreations.Keys

    [<TestMethod; DynamicData(nameof HashMapExtensionsTests.RepeatedKeyCreations)>]
    member _.``every shape keeps the last binding of a key as Map does`` (creation : string) =
        CollectionAssert.AreEqual (
            Map.toArray (Map.ofArray [| "a", 1; "b", 2; "a", 3 |]),
            sortedBindings (repeatedKeyCreations[creation]()),
            "The hash map must keep the last binding of a key, as Map does"
        )

    [<TestMethod; DynamicData(nameof HashMapExtensionsTests.ShapedCreations)>]
    member _.``every shape keeps the earlier binding of a key for a value that the default comparer finds equal``
        (creation : string)
        =
        // The indexer of the builder keeps the earlier binding, its key included (ADR 0001, D14); the copies are equal in
        // the default comparers and tell the two bindings apart
        let firstKey = MapInputs.Copy "one"
        let firstValue = MapInputs.Copy "uno"

        let table =
            shapedCreations[creation] [| struct (firstKey, firstValue); struct (MapInputs.Copy "one", MapInputs.Copy "uno") |]

        let struct (key, value) =
            Assert.ContainsSingle (HashMap.toArray table, "The hash map must merge the bindings of the equal keys")

        Assert.AreSame (firstKey, key, "The hash map must keep the earlier key")
        Assert.AreSame (firstValue, value, "The hash map must keep the earlier value")

    [<TestMethod; DynamicData(nameof HashMapExtensionsTests.Pipelines)>]
    member _.``the creation members resolve the shape of the bindings in a pipeline`` (source : string) =
        CollectionAssert.AreEqual (
            [| "one", 1; "two", 2 |],
            sortedBindings (pipelines[source]()),
            "The pipeline must create the bindings of its source"
        )

    [<TestMethod>]
    member _.``ofSeq returns a hash map with the default comparers itself`` () =
        let table = HashMap.ofArray [| "one", 1; "two", 2 |]
        Assert.AreSame (table, HashMap.ofSeq table, "HashMap.ofSeq must return a hash map with the default comparers itself")

    [<TestMethod>]
    member _.``ofSeq gives a hash map with other comparers the default comparers`` () =
        let ignoreCase =
            (HashMap.forKeyComparer StringComparer.OrdinalIgnoreCase).ofArray [| "one", 1; "two", 2 |]
        let table = HashMap.ofSeq ignoreCase

        Assert.AreSame<IEqualityComparer<string>>(
            EqualityComparer<string>.Default,
            HashMap.keyComparer table,
            "HashMap.ofSeq must give a hash map the default key comparer"
        )

        CollectionAssert.AreEqual (sortedBindings ignoreCase, sortedBindings table, "HashMap.ofSeq must keep the bindings")
        Assert.IsFalse (HashMap.containsKey "ONE" table, "HashMap.ofSeq must look the keys up with the default comparer")
