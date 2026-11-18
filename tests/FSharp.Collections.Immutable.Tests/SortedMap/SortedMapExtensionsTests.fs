namespace FSharp.Collections.Immutable.Tests

open System
open System.Collections.Generic
open System.Reflection
open FSharp.Collections.Immutable
open Microsoft.VisualStudio.TestTools.UnitTesting

/// The static members of `SortedMapExtensions`, which `SortedMap.ofArray`, `SortedMap.ofList` and `SortedMap.ofSeq` reach
/// through the abbreviation, called as user code calls them (ADR 0001, D13).
[<TestClass; SortedMapTestCategory>]
type SortedMapExtensionsTests () =

    /// The creations of a sorted map from bindings that repeat the key "a", by the member and the shape that they call.
    static let repeatedKeyCreations : Map<string, unit -> SortedMap<string, int>> =
        Map.ofArray [|
            "ofArray of key/value pairs",
            (fun () -> SortedMap.ofArray [| KeyValuePair ("a", 1); KeyValuePair ("b", 2); KeyValuePair ("a", 3) |])
            "ofArray of reference tuples", (fun () -> SortedMap.ofArray [| "a", 1; "b", 2; "a", 3 |])
            "ofArray of struct tuples", (fun () -> SortedMap.ofArray [| struct ("a", 1); struct ("b", 2); struct ("a", 3) |])
            "ofList of key/value pairs",
            (fun () -> SortedMap.ofList [ KeyValuePair ("a", 1); KeyValuePair ("b", 2); KeyValuePair ("a", 3) ])
            "ofList of reference tuples", (fun () -> SortedMap.ofList [ "a", 1; "b", 2; "a", 3 ])
            "ofList of struct tuples", (fun () -> SortedMap.ofList [ struct ("a", 1); struct ("b", 2); struct ("a", 3) ])
            "ofSeq of key/value pairs",
            (fun () -> SortedMap.ofSeq (Seq.ofArray [| KeyValuePair ("a", 1); KeyValuePair ("b", 2); KeyValuePair ("a", 3) |]))
            "ofSeq of reference tuples", (fun () -> SortedMap.ofSeq (Seq.ofArray [| "a", 1; "b", 2; "a", 3 |]))
            "ofSeq of struct tuples",
            (fun () -> SortedMap.ofSeq (Seq.ofArray [| struct ("a", 1); struct ("b", 2); struct ("a", 3) |]))
        |]

    /// The creations of a sorted map that binds each word to its length, by the member and the shape that they call.
    static let wordCreations : Map<string, string array -> SortedMap<string, int>> =
        Map.ofArray [|
            "ofArray of key/value pairs",
            (fun words ->
                SortedMap.ofArray (
                    words
                    |> Array.map (fun word -> KeyValuePair (word, word.Length))
                )
            )
            "ofArray of reference tuples", (fun words -> SortedMap.ofArray (words |> Array.map (fun word -> word, word.Length)))
            "ofArray of struct tuples",
            (fun words -> SortedMap.ofArray (words |> Array.map (fun word -> struct (word, word.Length))))
            // The members under test take lists
            "ofList of key/value pairs", (fun words -> SortedMap.ofList [ for word in words -> KeyValuePair (word, word.Length) ])
            "ofList of reference tuples", (fun words -> SortedMap.ofList [ for word in words -> word, word.Length ])
            "ofList of struct tuples", (fun words -> SortedMap.ofList [ for word in words -> struct (word, word.Length) ])
            "ofSeq of key/value pairs",
            (fun words ->
                SortedMap.ofSeq (
                    words
                    |> Seq.map (fun word -> KeyValuePair (word, word.Length))
                )
            )
            "ofSeq of reference tuples", (fun words -> SortedMap.ofSeq (words |> Seq.map (fun word -> word, word.Length)))
            "ofSeq of struct tuples", (fun words -> SortedMap.ofSeq (words |> Seq.map (fun word -> struct (word, word.Length))))
        |]

    /// The creations of a sorted map of "one" and "two" in a pipeline, by the shape and the source of the bindings; every
    /// source has "two" first.
    static let pipelines : Map<string, unit -> SortedMap<string, int>> =
        Map.ofArray [|
            "an array of key/value pairs",
            (fun () ->
                [| KeyValuePair ("two", 2); KeyValuePair ("one", 1) |]
                |> SortedMap.ofArray
            )
            "an array of reference tuples", (fun () -> [| "two", 2; "one", 1 |] |> SortedMap.ofArray)
            "an array of struct tuples",
            (fun () ->
                [| struct ("two", 2); struct ("one", 1) |]
                |> SortedMap.ofArray
            )
            "a list of key/value pairs",
            (fun () ->
                [ KeyValuePair ("two", 2); KeyValuePair ("one", 1) ]
                |> SortedMap.ofList
            )
            "a list of reference tuples", (fun () -> [ "two", 2; "one", 1 ] |> SortedMap.ofList)
            "a list of struct tuples", (fun () -> [ struct ("two", 2); struct ("one", 1) ] |> SortedMap.ofList)
            "a sequence of key/value pairs",
            (fun () ->
                Seq.ofArray [| KeyValuePair ("two", 2); KeyValuePair ("one", 1) |]
                |> SortedMap.ofSeq
            )
            "a sequence of reference tuples", (fun () -> Seq.ofArray [| "two", 2; "one", 1 |] |> SortedMap.ofSeq)
            "a sequence of struct tuples",
            (fun () ->
                Seq.ofArray [| struct ("two", 2); struct ("one", 1) |]
                |> SortedMap.ofSeq
            )
            "a dictionary", (fun () -> dict [| "two", 2; "one", 1 |] |> SortedMap.ofSeq)
            "an F# map", (fun () -> Map.ofArray [| "two", 2; "one", 1 |] |> SortedMap.ofSeq)
            "a hash map", (fun () -> HashMap.ofArray [| "two", 2; "one", 1 |] |> SortedMap.ofSeq)
            "a sorted map",
            (fun () ->
                SortedMap.ofArray [| "two", 2; "one", 1 |]
                |> SortedMap.ofSeq
            )
        |]

    /// The creations of a sorted map from the given bindings, by the member and the shape that they call.
    static let shapedCreations : Map<string, struct (string * string) array -> SortedMap<string, string>> =
        Map.ofArray [|
            "ofArray of key/value pairs", (fun bindings -> SortedMap.ofArray (Array.map MapInputs.ToKeyValuePair bindings))
            "ofArray of reference tuples", (fun bindings -> SortedMap.ofArray (Array.map MapInputs.ToTuple bindings))
            "ofArray of struct tuples", (fun bindings -> SortedMap.ofArray bindings)
            "ofList of key/value pairs",
            (fun bindings ->
                SortedMap.ofList (
                    bindings
                    |> Array.map MapInputs.ToKeyValuePair
                    |> List.ofArray
                )
            )
            "ofList of reference tuples",
            (fun bindings -> SortedMap.ofList (bindings |> Array.map MapInputs.ToTuple |> List.ofArray))
            "ofList of struct tuples", (fun bindings -> SortedMap.ofList (List.ofArray bindings))
            "ofSeq of key/value pairs", (fun bindings -> SortedMap.ofSeq (Seq.map MapInputs.ToKeyValuePair bindings))
            "ofSeq of reference tuples", (fun bindings -> SortedMap.ofSeq (Seq.map MapInputs.ToTuple bindings))
            "ofSeq of struct tuples", (fun bindings -> SortedMap.ofSeq (Seq.ofArray bindings))
        |]

    /// The names of the creations from bindings that repeat a key, as the rows of the test of the last binding.
    static member RepeatedKeyCreations = repeatedKeyCreations.Keys

    /// The names of the creations from words, as the rows of the test of the order of strings.
    static member WordCreations = wordCreations.Keys

    /// The names of the sources of the pipelines: the rows of the test of the pipelines.
    static member Pipelines = pipelines.Keys

    /// The names of the creations from the given bindings, as the rows of the test of the values that the default comparer
    /// finds equal.
    static member ShapedCreations = shapedCreations.Keys

    /// The keys of the sorted maps that the test of an existing map gives to ofSeq. The property builds new arrays on each
    /// read.
    static member Keys : int[] seq = seq {
        [||]
        [| 42 |]
        [| 5; -3; 8; 0; 12 |]
    }

    [<TestMethod; DynamicData(nameof SortedMapExtensionsTests.RepeatedKeyCreations)>]
    member _.``every shape keeps the last binding of a key as Map does`` (creation : string) =
        CollectionAssert.AreEqual (
            Map.toArray (Map.ofArray [| "a", 1; "b", 2; "a", 3 |])
            |> Array.map (fun (key, value) -> struct (key, value)),
            SortedMap.toArray (repeatedKeyCreations[creation]()),
            "The sorted map must keep the last binding of a key in the order of the keys, as Map does"
        )

    [<TestMethod; DynamicData(nameof SortedMapExtensionsTests.ShapedCreations)>]
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
            Assert.ContainsSingle (SortedMap.toArray table, "The sorted map must merge the bindings of the equal keys")

        Assert.AreSame (firstKey, key, "The sorted map must keep the earlier key")
        Assert.AreSame (firstValue, value, "The sorted map must keep the earlier value")

    [<TestMethod; DynamicData(nameof SortedMapExtensionsTests.WordCreations)>]
    member _.``every shape orders strings ordinally as Map does`` (creation : string) =
        // Ordinally, the digits, the upper case letters and the underscore come before the lower case letters, and "10"
        // before "9"; the order of a culture differs
        let words = [| "b"; "a"; "B"; "A"; "_"; "10"; "9" |]
        let table = wordCreations[creation] words

        Assert.AreSame<IComparer<string>>(
            LanguagePrimitives.FastGenericComparer<string>,
            SortedMap.keyComparer table,
            "The sorted map must order its keys with F# generic comparison"
        )

        CollectionAssert.AreEqual (
            Seq.toArray (Map.keys (Map.ofArray (words |> Array.map (fun word -> word, word.Length)))),
            Seq.toArray (SortedMap.keys table),
            "The sorted map must order strings ordinally, as Map does"
        )

    [<TestMethod; DynamicData(nameof SortedMapExtensionsTests.Pipelines)>]
    member _.``the creation members resolve the shape of the bindings in a pipeline`` (source : string) =
        CollectionAssert.AreEqual (
            [| struct ("one", 1); struct ("two", 2) |],
            SortedMap.toArray (pipelines[source]()),
            "The pipeline must create the bindings of its source in the order of the keys"
        )

    [<TestMethod; DynamicData(nameof SortedMapExtensionsTests.Keys)>]
    member _.``ofSeq returns a sorted map with the default comparers itself`` (keys : int array) =
        let table = SortedMap.ofArray (keys |> Array.map (fun key -> struct (key, string key)))
        Assert.AreSame (
            table,
            SortedMap.ofSeq table,
            "SortedMap.ofSeq must return a sorted map with the default comparers itself"
        )

    [<TestMethod>]
    member _.``ofSeq gives a sorted map with other comparers the default comparers`` () =
        let ignoreCase =
            (SortedMap.forKeyComparer StringComparer.OrdinalIgnoreCase).ofArray [| "one", 1; "Two", 2 |]

        let table = SortedMap.ofSeq ignoreCase

        Assert.AreSame<IComparer<string>>(
            LanguagePrimitives.FastGenericComparer<string>,
            SortedMap.keyComparer table,
            "SortedMap.ofSeq must give a sorted map the key comparer of F# generic comparison"
        )

        // Ordinally, "Two" comes before "one"; ignoring the case, after it
        CollectionAssert.AreEqual (
            [| struct ("Two", 2); struct ("one", 1) |],
            SortedMap.toArray table,
            "SortedMap.ofSeq must keep the bindings and order them ordinally"
        )

        Assert.IsFalse (SortedMap.containsKey "two" table, "SortedMap.ofSeq must look the keys up with F# generic comparison")

    [<TestMethod; DataRow "OfArray"; DataRow "OfList"; DataRow "OfSeq">]
    member _.``the creation members are public static methods of SortedMapExtensions that the module does not have``
        (name : string)
        =
        // C# sees the members only as these methods, and a reflection over the module lists none of them
        let assembly = typeof<CustomComparersSortedMap<int, int>>.Assembly

        let typeNamed (typeName : string) =
            match assembly.GetType typeName with
            | null -> failwith $"The library must have the type %s{typeName}"
            | found -> found

        let methodsNamed (typeName : string) =
            (typeNamed typeName)
                .GetMethods(
                    BindingFlags.Public
                    ||| BindingFlags.Static
                    ||| BindingFlags.DeclaredOnly
                )
            |> Array.filter (fun candidate -> String.Equals (candidate.Name, name, StringComparison.Ordinal))

        Assert.HasCount (
            3,
            methodsNamed "FSharp.Collections.Immutable.SortedMapExtensions",
            $"SortedMapExtensions must have one overload of %s{name} for each shape of the bindings"
        )

        Assert.IsEmpty (
            methodsNamed "FSharp.Collections.Immutable.SortedMap",
            $"The SortedMap module must have no function %s{name}, so that SortedMap.ofSeq reaches the extension members"
        )
