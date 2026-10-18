namespace FSharp.Collections.Immutable.Tests

open System
open System.Collections.Generic
open System.Collections.Immutable
open FSharp.Collections.Immutable
open Microsoft.VisualStudio.TestTools.UnitTesting

/// <summary>
/// The sets of keys of the tests of the order of a sorted map, as the cases of their rows.
/// </summary>
/// <remarks>
/// MSTest names a row after its values, and a set of bindings is too long to read in a name, so a row holds a case and the
/// test takes the bindings of the case. The checks are generic in the type of the keys, which differs from case to case.
/// </remarks>
[<RequireQualifiedAccess>]
type OrderedKeys =
    /// String keys whose ordinal order, the order of F# generic comparison, is not the order of a culture: the upper case
    /// letters and the underscore come before the lower case letters, and the digits compare one character at a time.
    | Strings
    /// Negative keys, zero and positive keys, in no order.
    | Negatives
    /// Tuple keys, which F# compares item by item: the numbers first, then the strings ordinally.
    | Tuples
    /// One key.
    | OneKey

    /// The sets of several keys: the rows of the tests of the order. The property builds its rows on each read.
    static member Several : OrderedKeys seq = seq {
        OrderedKeys.Strings
        OrderedKeys.Negatives
        OrderedKeys.Tuples
    }

    /// Every set: the rows of the tests of the smallest and the largest binding.
    static member All : OrderedKeys seq = seq {
        yield! OrderedKeys.Several
        OrderedKeys.OneKey
    }

/// <summary>
/// The functions and expressions that create a sorted map from bindings with F# generic comparison of the keys, as the
/// cases of the rows of the tests of the order.
/// </summary>
[<RequireQualifiedAccess>]
type OrderedCreation =
    /// A builder that the bindings fill one by one.
    | Builder
    /// <c>add</c> of each binding to the empty sorted map.
    | Empty
    /// <c>ofArray</c> of struct tuples.
    | OfArrayOfStructTuples
    /// <c>ofList</c> of struct tuples.
    | OfListOfStructTuples
    /// <c>ofSeq</c> of key/value pairs.
    | OfSeqOfKeyValuePairs
    /// <c>ofSeq</c> of struct tuples.
    | OfSeqOfStructTuples
    /// <c>add</c> of each binding to the sorted map of the last one.
    | Singleton
    /// The <c>sortedMap { }</c> expression.
    | Expression

    /// Every creation: the rows of the tests of the order. The property builds its rows on each read.
    static member All : OrderedCreation seq = seq {
        OrderedCreation.Builder
        OrderedCreation.Empty
        OrderedCreation.OfArrayOfStructTuples
        OrderedCreation.OfListOfStructTuples
        OrderedCreation.OfSeqOfKeyValuePairs
        OrderedCreation.OfSeqOfStructTuples
        OrderedCreation.Singleton
        OrderedCreation.Expression
    }

    /// Creates a sorted map of the bindings, whose keys are distinct: add rejects another value of a key, as the BCL does.
    member this.Create<'Key, 'Value when 'Key : not null and 'Key : comparison> (bindings : struct ('Key * 'Value) array) =
        let addAll (table : SortedMap<'Key, 'Value>) =
            (table, bindings)
            ||> Array.fold (fun table struct (key, value) -> SortedMap.add key value table)

        match this with
        | OrderedCreation.Builder ->
            let builder = SortedMap.builder ()

            for struct (key, value) in bindings do
                builder[key] <- value

            SortedMap.ofBuilder builder
        | OrderedCreation.Empty -> addAll SortedMap.empty
        | OrderedCreation.OfArrayOfStructTuples -> SortedMap.ofArray bindings
        // The member under test takes a list
        | OrderedCreation.OfListOfStructTuples -> SortedMap.ofList (List.ofArray bindings)
        | OrderedCreation.OfSeqOfKeyValuePairs ->
            SortedMap.ofSeq (
                bindings
                |> Array.map (fun struct (key, value) -> KeyValuePair (key, value))
            )
        | OrderedCreation.OfSeqOfStructTuples -> SortedMap.ofSeq bindings
        | OrderedCreation.Singleton ->
            let struct (lastKey, lastValue) = Array.last bindings
            addAll (SortedMap.singleton lastKey lastValue)
        | OrderedCreation.Expression -> sortedMap { yield! bindings }

/// The behaviour of `SortedMap` that the shared contracts do not cover: the order of the keys, which is the order of the
/// FSharp.Core `Map`, the comparers, `minKeyValue`, `maxKeyValue` and `singleton`, null inputs, shared results and the
/// semantics of the functions that wrap the BCL.
[<TestClass; SortedMapTestCategory>]
type SortedMapTests () =

    static let nullTable = Unchecked.defaultof<SortedMap<string, string>>

    static let stringBindings = [|
        struct ("b", 1)
        struct ("a", 2)
        struct ("B", 3)
        struct ("A", 4)
        struct ("10", 5)
        struct ("9", 6)
        struct ("_", 7)
    |]

    static let negativeBindings = [|
        struct (5, "five")
        struct (-3, "minus three")
        struct (0, "zero")
        struct (-12, "minus twelve")
        struct (8, "eight")
    |]

    static let tupleBindings = [|
        struct ((1, "b"), 1)
        struct ((1, "B"), 2)
        struct ((0, "z"), 3)
        struct ((-1, "a"), 4)
        struct ((1, "a"), 5)
    |]

    static let oneBinding = [| struct (42, "answer") |]

    /// A comparer that orders integers from the largest to the smallest.
    static let descending = ComparisonIdentity.FromFunction (fun (left : int) (right : int) -> compare right left)

    /// A value comparer that differs from the default one: it compares strings by their length.
    static let byLength =
        HashIdentity.FromFunctions (fun (value : string) -> value.Length) (fun left right -> left.Length = right.Length)

    /// A map whose keys ignore the case and whose values compare by their length.
    static let customTable () =
        (SortedMap.forComparers StringComparer.OrdinalIgnoreCase byLength).ofArray [|
            "one", "uno"
            "Two", "dos"
            "three", "tres"
        |]

    /// A map with the default comparers.
    static let defaultTable () = SortedMap.ofArray [| struct ("one", "uno"); struct ("two", "dos") |]

    /// The FSharp.Core map of bindings, the oracle of the order.
    static let toMap (bindings : struct ('Key * 'Value) array) =
        bindings
        |> Array.map (fun struct (key, value) -> key, value)
        |> Map.ofArray

    /// The bindings of an FSharp.Core map, in its order.
    static let ofMap (map : Map<'Key, 'Value>) =
        map
        |> Map.toArray
        |> Array.map (fun (key, value) -> struct (key, value))

    /// Asserts that a sorted map has the bindings of an FSharp.Core map, in the order of that map.
    static let inMapOrder (expected : Map<'Key, 'Value>) (table : SortedMap<'Key, 'Value>) (message : string) =
        CollectionAssert.AreEqual (ofMap expected, SortedMap.toArray table, message)

    /// Asserts that the functions that visit the bindings one by one follow the order of the FSharp.Core map of them.
    static let assertVisitingOrder (bindings : struct ('Key * 'Value) array) =
        let expected = toMap bindings
        let table = SortedMap.ofArray bindings
        let message (name : string) = $"SortedMap.%s{name} must follow the order of Map"
        let record (state : struct ('Key * 'Value) array) key value = Array.append state [| struct (key, value) |]
        let recordBack key value (state : struct ('Key * 'Value) array) = Array.append [| struct (key, value) |] state
        let visited = ResizeArray<struct ('Key * 'Value)>()
        SortedMap.iter (fun key value -> visited.Add (struct (key, value))) table
        let tested = ResizeArray<'Key>()

        SortedMap.exists
            (fun key _ ->
                tested.Add key
                false
            )
            table
        |> ignore

        CollectionAssert.AreEqual (
            Seq.toArray (Map.keys expected),
            Seq.toArray (SortedMap.keys table),
            message (nameof SortedMap.keys)
        )

        CollectionAssert.AreEqual (
            Seq.toArray (Map.values expected),
            Seq.toArray (SortedMap.values table),
            message (nameof SortedMap.values)
        )

        CollectionAssert.AreEqual (ofMap expected, SortedMap.toArray table, message (nameof SortedMap.toArray))
        CollectionAssert.AreEqual (ofMap expected, Seq.toArray (SortedMap.toSeq table), message (nameof SortedMap.toSeq))

        CollectionAssert.AreEqual (
            Map.fold record [||] expected,
            SortedMap.fold record [||] table,
            message (nameof SortedMap.fold)
        )

        CollectionAssert.AreEqual (
            Map.foldBack recordBack expected [||],
            SortedMap.foldBack recordBack table [||],
            message (nameof SortedMap.foldBack)
        )

        CollectionAssert.AreEqual (ofMap expected, visited.ToArray (), message (nameof SortedMap.iter))
        CollectionAssert.AreEqual (Seq.toArray (Map.keys expected), tested.ToArray (), message (nameof SortedMap.exists))

        Assert.AreEqual<'Key>(
            Map.findKey (fun _ _ -> true) expected,
            SortedMap.findKey (fun _ _ -> true) table,
            message (nameof SortedMap.findKey)
        )

        Assert.AreEqual<'Key>(
            Map.pick (fun key _ -> Some key) expected,
            SortedMap.pick (fun key _ -> ValueSome key) table,
            message (nameof SortedMap.pick)
        )

    /// Asserts that the transformations that rebuild a sorted map keep the order of the FSharp.Core map of its bindings.
    static let assertTransformationOrder (bindings : struct ('Key * 'Value) array) =
        let expected = toMap bindings
        let table = SortedMap.ofArray bindings

        // Every second key in the order of Map, so that the predicate keeps keys from the whole range
        let kept =
            Map.keys expected
            |> Seq.indexed
            |> Seq.filter (fun (index, _) -> index % 2 = 0)
            |> Seq.map snd
            |> Set.ofSeq

        let predicate key (_ : 'Value) = kept.Contains key
        let mapping (key : 'Key) (value : 'Value) = struct (key, value)

        let chooser key value =
            if predicate key value then
                ValueSome (mapping key value)
            else
                ValueNone

        let message (name : string) = $"SortedMap.%s{name} must keep the order of Map"
        let struct (accepted, rejected) = SortedMap.partition predicate table
        let expectedAccepted, expectedRejected = Map.partition predicate expected

        inMapOrder (Map.filter predicate expected) (SortedMap.filter predicate table) (message (nameof SortedMap.filter))
        inMapOrder expectedAccepted accepted (message (nameof SortedMap.partition))
        inMapOrder expectedRejected rejected (message (nameof SortedMap.partition))
        inMapOrder (Map.map mapping expected) (SortedMap.map mapping table) (message (nameof SortedMap.map))

        inMapOrder
            (Map.filter predicate expected |> Map.map mapping)
            (SortedMap.choose chooser table)
            (message (nameof SortedMap.choose))

    /// Asserts that minKeyValue returns the binding of the smallest key as Map does.
    static let assertMinimum (bindings : struct ('Key * 'Value) array) =
        let struct (minKey, minValue) = SortedMap.minKeyValue (SortedMap.ofArray bindings)

        Assert.AreEqual<'Key * 'Value>(
            Map.minKeyValue (toMap bindings),
            (minKey, minValue),
            "SortedMap.minKeyValue must return the binding of the smallest key"
        )

    /// Asserts that maxKeyValue returns the binding of the largest key as Map does.
    static let assertMaximum (bindings : struct ('Key * 'Value) array) =
        let struct (maxKey, maxValue) = SortedMap.maxKeyValue (SortedMap.ofArray bindings)

        Assert.AreEqual<'Key * 'Value>(
            Map.maxKeyValue (toMap bindings),
            (maxKey, maxValue),
            "SortedMap.maxKeyValue must return the binding of the largest key"
        )

    /// The functions that take a map, by name, on a null map. The rows of the tests are the names: a function has no
    /// readable name in a row.
    static let nullTableCalls : Map<string, unit -> unit> =
        Map.ofArray [|
            nameof SortedMap.add, (fun () -> SortedMap.add "a" "b" nullTable |> ignore)
            nameof SortedMap.addRange,
            (fun () ->
                SortedMap.addRange [| KeyValuePair ("a", "b") |] nullTable
                |> ignore
            )
            nameof SortedMap.change, (fun () -> SortedMap.change "a" id nullTable |> ignore)
            nameof SortedMap.choose,
            (fun () ->
                SortedMap.choose (fun _ value -> ValueSome value) nullTable
                |> ignore
            )
            nameof SortedMap.clear, (fun () -> SortedMap.clear nullTable |> ignore)
            nameof SortedMap.containsBinding, (fun () -> SortedMap.containsBinding "a" "b" nullTable |> ignore)
            nameof SortedMap.containsKey, (fun () -> SortedMap.containsKey "a" nullTable |> ignore)
            nameof SortedMap.containsValue, (fun () -> SortedMap.containsValue "a" nullTable |> ignore)
            nameof SortedMap.count, (fun () -> SortedMap.count nullTable |> ignore)
            nameof SortedMap.exists, (fun () -> SortedMap.exists (fun _ _ -> true) nullTable |> ignore)
            nameof SortedMap.filter, (fun () -> SortedMap.filter (fun _ _ -> true) nullTable |> ignore)
            nameof SortedMap.find, (fun () -> SortedMap.find "a" nullTable |> ignore)
            nameof SortedMap.findKey, (fun () -> SortedMap.findKey (fun _ _ -> true) nullTable |> ignore)
            nameof SortedMap.fold,
            (fun () ->
                SortedMap.fold (fun state _ _ -> state) 0 nullTable
                |> ignore
            )
            nameof SortedMap.foldBack,
            (fun () ->
                SortedMap.foldBack (fun _ _ state -> state) nullTable 0
                |> ignore
            )
            nameof SortedMap.forall, (fun () -> SortedMap.forall (fun _ _ -> true) nullTable |> ignore)
            nameof SortedMap.isEmpty, (fun () -> SortedMap.isEmpty nullTable |> ignore)
            nameof SortedMap.iter, (fun () -> SortedMap.iter (fun _ _ -> ()) nullTable)
            nameof SortedMap.keyComparer, (fun () -> SortedMap.keyComparer nullTable |> ignore)
            nameof SortedMap.keys, (fun () -> SortedMap.keys nullTable |> ignore)
            nameof SortedMap.map, (fun () -> SortedMap.map (fun _ value -> value) nullTable |> ignore)
            nameof SortedMap.maxKeyValue, (fun () -> SortedMap.maxKeyValue nullTable |> ignore)
            nameof SortedMap.minKeyValue, (fun () -> SortedMap.minKeyValue nullTable |> ignore)
            nameof SortedMap.partition, (fun () -> SortedMap.partition (fun _ _ -> true) nullTable |> ignore)
            nameof SortedMap.pick,
            (fun () ->
                SortedMap.pick (fun _ value -> ValueSome value) nullTable
                |> ignore
            )
            nameof SortedMap.remove, (fun () -> SortedMap.remove "a" nullTable |> ignore)
            nameof SortedMap.removeRange, (fun () -> SortedMap.removeRange [| "a" |] nullTable |> ignore)
            nameof SortedMap.setItem, (fun () -> SortedMap.setItem "a" "b" nullTable |> ignore)
            nameof SortedMap.setItems,
            (fun () ->
                SortedMap.setItems [| KeyValuePair ("a", "b") |] nullTable
                |> ignore
            )
            nameof SortedMap.toArray, (fun () -> SortedMap.toArray nullTable |> ignore)
            nameof SortedMap.toBuilder, (fun () -> SortedMap.toBuilder nullTable |> ignore)
            nameof SortedMap.toList, (fun () -> SortedMap.toList nullTable |> ignore)
            nameof SortedMap.toSeq, (fun () -> SortedMap.toSeq nullTable |> ignore)
            nameof SortedMap.tryFind, (fun () -> SortedMap.tryFind "a" nullTable |> ignore)
            nameof SortedMap.tryFindKey, (fun () -> SortedMap.tryFindKey (fun _ _ -> true) nullTable |> ignore)
            nameof SortedMap.tryGetKey, (fun () -> SortedMap.tryGetKey "a" nullTable |> ignore)
            nameof SortedMap.tryPick,
            (fun () ->
                SortedMap.tryPick (fun _ value -> ValueSome value) nullTable
                |> ignore
            )
            nameof SortedMap.update, (fun () -> SortedMap.update ignore nullTable |> ignore)
            nameof SortedMap.valueComparer, (fun () -> SortedMap.valueComparer nullTable |> ignore)
            nameof SortedMap.values, (fun () -> SortedMap.values nullTable |> ignore)
            "Tuple.addRange", (fun () -> SortedMap.Tuple.addRange [| "a", "b" |] nullTable |> ignore)
            "Tuple.setItems", (fun () -> SortedMap.Tuple.setItems [| "a", "b" |] nullTable |> ignore)
            "ValueTuple.addRange",
            (fun () ->
                SortedMap.ValueTuple.addRange [| struct ("a", "b") |] nullTable
                |> ignore
            )
            "ValueTuple.setItems",
            (fun () ->
                SortedMap.ValueTuple.setItems [| struct ("a", "b") |] nullTable
                |> ignore
            )
        |]

    /// The functions that take a sequence, by name, with the name of the sequence parameter and the function on a null
    /// sequence.
    static let nullSequenceCalls : Map<string, struct (string * (unit -> unit))> =
        let table = defaultTable ()

        Map.ofArray [|
            nameof SortedMap.addRange,
            struct ("pairs",
                    fun () ->
                        SortedMap.addRange (Unchecked.defaultof<seq<KeyValuePair<string, string>>>) table
                        |> ignore)
            "ofArray of key/value pairs",
            struct ("elements",
                    fun () ->
                        SortedMap.ofArray (Unchecked.defaultof<KeyValuePair<string, string> array>)
                        |> ignore)
            "ofArray of reference tuples",
            struct ("elements",
                    fun () ->
                        SortedMap.ofArray (Unchecked.defaultof<(string * string) array>)
                        |> ignore)
            "ofArray of struct tuples",
            struct ("elements",
                    fun () ->
                        SortedMap.ofArray (Unchecked.defaultof<struct (string * string) array>)
                        |> ignore)
            "ofSeq of key/value pairs",
            struct ("elements",
                    fun () ->
                        SortedMap.ofSeq (Unchecked.defaultof<seq<KeyValuePair<string, string>>>)
                        |> ignore)
            "ofSeq of reference tuples",
            struct ("elements",
                    fun () ->
                        SortedMap.ofSeq (Unchecked.defaultof<seq<string * string>>)
                        |> ignore)
            "ofSeq of struct tuples",
            struct ("elements",
                    fun () ->
                        SortedMap.ofSeq (Unchecked.defaultof<seq<struct (string * string)>>)
                        |> ignore)
            nameof SortedMap.ofSeqBy,
            struct ("source",
                    fun () ->
                        SortedMap.ofSeqBy id (Unchecked.defaultof<seq<string>>)
                        |> ignore)
            nameof SortedMap.removeRange,
            struct ("keys",
                    fun () ->
                        SortedMap.removeRange (Unchecked.defaultof<seq<string>>) table
                        |> ignore)
            nameof SortedMap.setItems,
            struct ("pairs",
                    fun () ->
                        SortedMap.setItems (Unchecked.defaultof<seq<KeyValuePair<string, string>>>) table
                        |> ignore)
            "Tuple.addRange",
            struct ("pairs",
                    fun () ->
                        SortedMap.Tuple.addRange (Unchecked.defaultof<seq<string * string>>) table
                        |> ignore)
            "Tuple.setItems",
            struct ("pairs",
                    fun () ->
                        SortedMap.Tuple.setItems (Unchecked.defaultof<seq<string * string>>) table
                        |> ignore)
            "ValueTuple.addRange",
            struct ("pairs",
                    fun () ->
                        SortedMap.ValueTuple.addRange (Unchecked.defaultof<seq<struct (string * string)>>) table
                        |> ignore)
            "ValueTuple.setItems",
            struct ("pairs",
                    fun () ->
                        SortedMap.ValueTuple.setItems (Unchecked.defaultof<seq<struct (string * string)>>) table
                        |> ignore)
        |]

    /// The functions that take a sequence, by name, with the name of the sequence parameter and the function on a default
    /// flat list that is given as the sequence. A default flat list converts to a sequence without a check and fails with
    /// InvalidOperationException when the sequence is enumerated, so the other inputs are valid and the function would
    /// enumerate it. The last row gives a sequence of objects, which a default flat list of strings is by the covariance of
    /// seq: the element type of the sequence is not the one of the flat list. The keys of a sorted map need comparison, so
    /// only removeRange takes the objects here, with the keys of a sorted map that a comparer of objects orders; ofSeqBy
    /// takes them through the struct.
    static let defaultSequenceCalls : Map<string, struct (string * (unit -> unit))> =
        let table = defaultTable ()

        let objectTable : SortedMap<obj, string> =
            (SortedMap.forKeyComparer Comparer<obj>.Default).ofArray [| KeyValuePair<obj, string>("one", "uno") |]

        let defaultPairs = Unchecked.defaultof<FlatList<KeyValuePair<string, string>>> :> seq<_>
        let defaultTuples = Unchecked.defaultof<FlatList<string * string>> :> seq<_>
        let defaultStructTuples = Unchecked.defaultof<FlatList<struct (string * string)>> :> seq<_>
        let defaultKeys = Unchecked.defaultof<FlatList<string>> :> seq<_>

        Map.ofArray [|
            nameof SortedMap.addRange, struct ("pairs", fun () -> SortedMap.addRange defaultPairs table |> ignore)
            "ofSeq of key/value pairs", struct ("elements", fun () -> SortedMap.ofSeq defaultPairs |> ignore)
            "ofSeq of reference tuples", struct ("elements", fun () -> SortedMap.ofSeq defaultTuples |> ignore)
            "ofSeq of struct tuples", struct ("elements", fun () -> SortedMap.ofSeq defaultStructTuples |> ignore)
            nameof SortedMap.ofSeqBy, struct ("source", fun () -> SortedMap.ofSeqBy id defaultKeys |> ignore)
            nameof SortedMap.removeRange, struct ("keys", fun () -> SortedMap.removeRange defaultKeys table |> ignore)
            nameof SortedMap.setItems, struct ("pairs", fun () -> SortedMap.setItems defaultPairs table |> ignore)
            "Tuple.addRange", struct ("pairs", fun () -> SortedMap.Tuple.addRange defaultTuples table |> ignore)
            "Tuple.setItems", struct ("pairs", fun () -> SortedMap.Tuple.setItems defaultTuples table |> ignore)
            "ValueTuple.addRange",
            struct ("pairs",
                    fun () ->
                        SortedMap.ValueTuple.addRange defaultStructTuples table
                        |> ignore)
            "ValueTuple.setItems",
            struct ("pairs",
                    fun () ->
                        SortedMap.ValueTuple.setItems defaultStructTuples table
                        |> ignore)
            $"%s{nameof SortedMap.removeRange} (default flat list of strings as objects)",
            struct ("keys",
                    fun () ->
                        SortedMap.removeRange FlatListOracles.defaultStringsAsObjects objectTable
                        |> ignore)
        |]

    /// The creation functions without comparers, by name and shape of the bindings.
    static let defaultCreations : Map<string, unit -> SortedMap<string, string>> =
        Map.ofArray [|
            nameof SortedMap.build, (fun () -> SortedMap.build (fun builder -> builder["a"] <- "b"))
            nameof SortedMap.builder, (fun () -> SortedMap.ofBuilder (SortedMap.builder ()))
            nameof SortedMap.empty, (fun () -> SortedMap.empty)
            "ofArray of key/value pairs", (fun () -> SortedMap.ofArray [| KeyValuePair ("a", "b") |])
            "ofArray of reference tuples", (fun () -> SortedMap.ofArray [| ("a", "b") |])
            "ofArray of struct tuples", (fun () -> SortedMap.ofArray [| struct ("a", "b") |])
            "ofList of key/value pairs", (fun () -> SortedMap.ofList [ KeyValuePair ("a", "b") ])
            "ofList of reference tuples", (fun () -> SortedMap.ofList [ ("a", "b") ])
            "ofList of struct tuples", (fun () -> SortedMap.ofList [ struct ("a", "b") ])
            "ofSeq of key/value pairs", (fun () -> SortedMap.ofSeq (dict [| "a", "b" |]))
            "ofSeq of reference tuples", (fun () -> SortedMap.ofSeq (Seq.singleton ("a", "b")))
            "ofSeq of struct tuples", (fun () -> SortedMap.ofSeq (Seq.singleton (struct ("a", "b"))))
            nameof SortedMap.ofSeqBy, (fun () -> SortedMap.ofSeqBy id [| "a" |])
            nameof SortedMap.singleton, (fun () -> SortedMap.singleton "a" "b")
        |]

    /// The creations of a sorted map of tuple keys by the functions, members and expressions that order the keys with F#
    /// generic comparison, by name. F# creates a new comparer of tuples on each call of FastGenericComparer at the type.
    static let tupleKeyCreations : Map<string, unit -> SortedMap<int * string, int>> =
        Map.ofArray [|
            "build", (fun () -> SortedMap.build (fun builder -> builder[(1, "a")] <- 1))
            "builder", (fun () -> SortedMap.ofBuilder (SortedMap.builder ()))
            "ofArray of struct tuples", (fun () -> SortedMap.ofArray [| struct ((1, "a"), 1) |])
            "ofList of reference tuples", (fun () -> SortedMap.ofList [ (1, "a"), 1 ])
            "ofSeq of key/value pairs", (fun () -> SortedMap.ofSeq [| KeyValuePair ((1, "a"), 1) |])
            "ofSeqBy", (fun () -> SortedMap.ofSeqBy (fun (value : int) -> value, "a") [| 1 |])
            "singleton", (fun () -> SortedMap.singleton (1, "a") 1)
            "sortedMap", (fun () -> sortedMap { yield struct ((1, "a"), 1) })
        |]

    /// The transformations that keep the type of the values, by name, on the map whose keys ignore the case.
    static let sameTypeTransformations : Map<string, SortedMap<string, string> -> SortedMap<string, string>> =
        Map.ofArray [|
            nameof SortedMap.add, SortedMap.add "four" "cuatro"
            nameof SortedMap.addRange, SortedMap.addRange [| KeyValuePair ("four", "cuatro") |]
            nameof SortedMap.change, SortedMap.change "one" (fun _ -> ValueNone)
            nameof SortedMap.clear, SortedMap.clear
            nameof SortedMap.filter, SortedMap.filter (fun (key : string) _ -> key.Length > 3)
            ($"%s{nameof SortedMap.partition} (accepted)",
             fun table ->
                 let struct (accepted, _) = SortedMap.partition (fun (key : string) _ -> key.Length > 3) table
                 accepted)
            ($"%s{nameof SortedMap.partition} (rejected)",
             fun table ->
                 let struct (_, rejected) = SortedMap.partition (fun (key : string) _ -> key.Length > 3) table
                 rejected)
            nameof SortedMap.remove, SortedMap.remove "one"
            nameof SortedMap.removeRange, SortedMap.removeRange [| "one"; "two" |]
            nameof SortedMap.setItem, SortedMap.setItem "four" "cuatro"
            nameof SortedMap.setItems, SortedMap.setItems [| KeyValuePair ("four", "cuatro") |]
            nameof SortedMap.update,
            SortedMap.update (fun (builder : SortedMap<string, string>.Builder) -> builder["four"] <- "cuatro")
            "Tuple.addRange", SortedMap.Tuple.addRange [| "four", "cuatro" |]
            "Tuple.setItems", SortedMap.Tuple.setItems [| "four", "cuatro" |]
            "ValueTuple.addRange", SortedMap.ValueTuple.addRange [| struct ("four", "cuatro") |]
            "ValueTuple.setItems", SortedMap.ValueTuple.setItems [| struct ("four", "cuatro") |]
        |]

    /// The transformations that change the type of the values, by name, on the map whose keys ignore the case.
    static let typeChangingTransformations : Map<string, SortedMap<string, string> -> SortedMap<string, int>> =
        Map.ofArray [|
            nameof SortedMap.choose, SortedMap.choose (fun _ (value : string) -> ValueSome value.Length)
            nameof SortedMap.map, SortedMap.map (fun _ (value : string) -> value.Length)
        |]

    /// The transformations that change the type of the values, by name, on a map whose keys are ordered from the largest to
    /// the smallest.
    static let descendingTransformations : Map<string, SortedMap<int, string> -> SortedMap<int, int>> =
        Map.ofArray [|
            nameof SortedMap.choose, SortedMap.choose (fun _ (value : string) -> ValueSome value.Length)
            nameof SortedMap.map, SortedMap.map (fun _ (value : string) -> value.Length)
        |]

    /// The key comparer of the result of every transformation, by name.
    static let resultKeyComparers : Map<string, SortedMap<string, string> -> IComparer<string>> =
        Map.ofSeq (
            seq {
                for KeyValue (name, transform) in sameTypeTransformations do
                    name, transform >> SortedMap.keyComparer

                for KeyValue (name, transform) in typeChangingTransformations do
                    name, transform >> SortedMap.keyComparer
            }
        )

    /// The functions that change nothing in the map with the default comparers, by name.
    static let unchangingCalls : Map<string, SortedMap<string, string> -> SortedMap<string, string>> =
        Map.ofArray [|
            nameof SortedMap.add, SortedMap.add "one" "uno"
            nameof SortedMap.addRange, SortedMap.addRange [| KeyValuePair ("one", "uno") |]
            $"%s{nameof SortedMap.change} of a key with a binding", SortedMap.change "one" id
            $"%s{nameof SortedMap.change} of a key without a binding", SortedMap.change "three" id
            nameof SortedMap.filter, SortedMap.filter (fun _ _ -> true)
            ($"%s{nameof SortedMap.partition} (accepted)",
             fun table ->
                 let struct (accepted, _) = SortedMap.partition (fun _ _ -> true) table
                 accepted)
            nameof SortedMap.remove, SortedMap.remove "three"
            nameof SortedMap.removeRange, SortedMap.removeRange [| "three" |]
            nameof SortedMap.setItem, SortedMap.setItem "two" "dos"
            nameof SortedMap.setItems, SortedMap.setItems [| KeyValuePair ("two", "dos") |]
            nameof SortedMap.update, SortedMap.update ignore
            "Tuple.addRange", SortedMap.Tuple.addRange [| "one", "uno" |]
            "Tuple.setItems", SortedMap.Tuple.setItems [| "two", "dos" |]
            "ValueTuple.addRange", SortedMap.ValueTuple.addRange [| struct ("one", "uno") |]
            "ValueTuple.setItems", SortedMap.ValueTuple.setItems [| struct ("two", "dos") |]
        |]

    /// The functions that bind "ONE" to "abc" in the map whose keys ignore the case and whose values compare by their
    /// length, by name: the map binds "one" to "uno", a value that the value comparer finds equal to "abc".
    static let equalValueCalls : Map<string, SortedMap<string, string> -> SortedMap<string, string>> =
        Map.ofArray [|
            nameof SortedMap.add, SortedMap.add "ONE" "abc"
            nameof SortedMap.addRange, SortedMap.addRange [| KeyValuePair ("ONE", "abc") |]
            nameof SortedMap.change, SortedMap.change "ONE" (fun _ -> ValueSome "abc")
            nameof SortedMap.setItem, SortedMap.setItem "ONE" "abc"
            nameof SortedMap.setItems, SortedMap.setItems [| KeyValuePair ("ONE", "abc") |]
            "Tuple.addRange", SortedMap.Tuple.addRange [| "ONE", "abc" |]
            "Tuple.setItems", SortedMap.Tuple.setItems [| "ONE", "abc" |]
            "ValueTuple.addRange", SortedMap.ValueTuple.addRange [| struct ("ONE", "abc") |]
            "ValueTuple.setItems", SortedMap.ValueTuple.setItems [| struct ("ONE", "abc") |]
        |]

    /// Every set of several keys with each creation: the rows of the test of the order of the creation functions.
    static member KeysAndCreations = seq {
        for keys in OrderedKeys.Several do
            for creation in OrderedCreation.All do
                struct (keys, creation)
    }

    /// The names of the functions that take a map, as the rows of their test.
    static member NullTableCalls = nullTableCalls.Keys

    /// The names of the functions that take a sequence, as the rows of their test.
    static member NullSequenceCalls = nullSequenceCalls.Keys

    /// The names of the functions that take a sequence and are given a default flat list, as the rows of their test.
    static member DefaultSequenceCalls = defaultSequenceCalls.Keys

    /// The names of the creation functions without comparers, as the rows of their test.
    static member DefaultCreations = defaultCreations.Keys

    /// The names of the creations of sorted maps of tuple keys, as the rows of the test of their shared comparer.
    static member TupleKeyCreations = tupleKeyCreations.Keys

    /// The names of the transformations that keep the type of the values, as the rows of their test.
    static member SameTypeTransformations = sameTypeTransformations.Keys

    /// The names of the transformations that change the type of the values, as the rows of their tests.
    static member TypeChangingTransformations = typeChangingTransformations.Keys

    /// The names of every transformation, as the rows of the test of the key comparer.
    static member Transformations = resultKeyComparers.Keys

    /// The names of the functions that change nothing, as the rows of their test.
    static member UnchangingCalls = unchangingCalls.Keys

    /// The names of the functions that set a value that the value comparer finds equal, as the rows of their test.
    static member EqualValueCalls = equalValueCalls.Keys

    [<TestMethod; DynamicData(nameof SortedMapTests.NullTableCalls)>]
    member _.``functions reject a null map`` (name : string) =
        // Map throws NullReferenceException for a null map; this library checks its arguments (ADR 0001, D6)
        let ex =
            Assert.ThrowsExactly<ArgumentNullException>(Action nullTableCalls[name], $"SortedMap.%s{name} must reject a null map")

        Assert.AreEqual<string | null>("table", ex.ParamName, $"SortedMap.%s{name} must name the map parameter")

    [<TestMethod; DynamicData(nameof SortedMapTests.NullSequenceCalls)>]
    member _.``functions reject a null sequence`` (name : string) =
        let struct (paramName, call) = nullSequenceCalls[name]

        let ex =
            Assert.ThrowsExactly<ArgumentNullException>(Action call, $"SortedMap.%s{name} must reject a null sequence")

        Assert.AreEqual<string | null>(paramName, ex.ParamName, $"SortedMap.%s{name} must name the sequence parameter")

    [<TestMethod; DynamicData(nameof SortedMapTests.DefaultSequenceCalls)>]
    member _.``functions reject a default flat list given as a sequence`` (name : string) =
        // The exception names the parameter of the function, not the one of the BCL member that it would have called
        let struct (paramName, call) = defaultSequenceCalls[name]
        FlatListAssert.RejectsNull ($"SortedMap.%s{name}", paramName, call)

    [<TestMethod>]
    member _.``a flat list that is not default passes as a sequence, also one of strings as a sequence of objects`` () =
        let flatList (elements : 'T array) = FlatList.ofArray elements :> seq<'T>
        let table = defaultTable ()

        CollectionAssert.AreEqual (
            [| struct ("four", "cuatro"); struct ("one", "uno"); struct ("two", "dos") |],
            SortedMap.toArray (SortedMap.addRange (flatList [| KeyValuePair ("four", "cuatro") |]) table),
            "SortedMap.addRange must take the elements of a flat list"
        )

        CollectionAssert.AreEqual (
            [| struct ("one", "uno"); struct ("two", "dos"); struct ("zero", "cero") |],
            SortedMap.toArray (SortedMap.setItems (flatList [| KeyValuePair ("zero", "cero") |]) table),
            "SortedMap.setItems must take the elements of a flat list"
        )

        CollectionAssert.AreEqual (
            [| struct ("two", "dos") |],
            SortedMap.toArray (SortedMap.removeRange (flatList [| "one" |]) table),
            "SortedMap.removeRange must take the elements of a flat list"
        )

        CollectionAssert.AreEqual (
            [| struct ("a", "b") |],
            SortedMap.toArray (SortedMap.ofSeq (flatList [| KeyValuePair ("a", "b") |])),
            "SortedMap.ofSeq must take the key/value pairs of a flat list"
        )

        CollectionAssert.AreEqual (
            [| struct ("a", "b") |],
            SortedMap.toArray (SortedMap.ofSeq (flatList [| ("a", "b") |])),
            "SortedMap.ofSeq must take the reference tuples of a flat list"
        )

        CollectionAssert.AreEqual (
            [| struct ("a", "b") |],
            SortedMap.toArray (SortedMap.ofSeq (flatList [| struct ("a", "b") |])),
            "SortedMap.ofSeq must take the struct tuples of a flat list"
        )

        CollectionAssert.AreEqual (
            [| struct ("ab", "ab") |],
            SortedMap.toArray (SortedMap.ofSeqBy id (flatList [| "ab" |])),
            "SortedMap.ofSeqBy must take the elements of a flat list"
        )

        CollectionAssert.AreEqual (
            [| struct ("one", "uno"); struct ("two", "dos"); struct ("zero", "cero") |],
            SortedMap.toArray (SortedMap.Tuple.setItems (flatList [| ("zero", "cero") |]) table),
            "SortedMap.Tuple.setItems must take the tuples of a flat list"
        )

        CollectionAssert.AreEqual (
            [| struct ("four", "cuatro"); struct ("one", "uno"); struct ("two", "dos") |],
            SortedMap.toArray (SortedMap.ValueTuple.addRange (flatList [| struct ("four", "cuatro") |]) table),
            "SortedMap.ValueTuple.addRange must take the struct tuples of a flat list"
        )

        // A flat list of strings is a sequence of objects, which a type test for the flat list of objects does not see
        let widened = FlatListOracles.stringsAsObjects (FlatList.ofArray [| "one" |])

        let objectTable : SortedMap<obj, string> =
            (SortedMap.forKeyComparer Comparer<obj>.Default).ofArray [|
                KeyValuePair<obj, string>("one", "uno")
                KeyValuePair<obj, string>("two", "dos")
            |]

        CollectionAssert.AreEqual (
            [| "two" :> obj |],
            SortedMap.keys (SortedMap.removeRange widened objectTable)
            |> Seq.toArray,
            "SortedMap.removeRange must take the elements of a flat list of strings as objects"
        )

    [<TestMethod; DynamicData(nameof SortedMapTests.KeysAndCreations)>]
    member _.``the creation functions order the keys as Map does`` (keys : OrderedKeys, creation : OrderedCreation) =
        let message = "The creation must order the keys as Map does"

        match keys with
        | OrderedKeys.Strings -> inMapOrder (toMap stringBindings) (creation.Create stringBindings) message
        | OrderedKeys.Negatives -> inMapOrder (toMap negativeBindings) (creation.Create negativeBindings) message
        | OrderedKeys.Tuples -> inMapOrder (toMap tupleBindings) (creation.Create tupleBindings) message
        | OrderedKeys.OneKey -> inMapOrder (toMap oneBinding) (creation.Create oneBinding) message

    [<TestMethod; DynamicData(nameof OrderedKeys.Several, typeof<OrderedKeys>)>]
    member _.``ofSeqBy orders the keys as Map does`` (keys : OrderedKeys) =
        let message = "SortedMap.ofSeqBy must order the keys as Map does"

        match keys with
        | OrderedKeys.Strings ->
            CollectionAssert.AreEqual (
                Seq.toArray (Map.keys (toMap stringBindings)),
                Seq.toArray (SortedMap.keys (SortedMap.ofSeqBy (fun struct (key, _) -> key) stringBindings)),
                message
            )
        | OrderedKeys.Negatives ->
            CollectionAssert.AreEqual (
                Seq.toArray (Map.keys (toMap negativeBindings)),
                Seq.toArray (SortedMap.keys (SortedMap.ofSeqBy (fun struct (key, _) -> key) negativeBindings)),
                message
            )
        | OrderedKeys.Tuples ->
            CollectionAssert.AreEqual (
                Seq.toArray (Map.keys (toMap tupleBindings)),
                Seq.toArray (SortedMap.keys (SortedMap.ofSeqBy (fun struct (key, _) -> key) tupleBindings)),
                message
            )
        | OrderedKeys.OneKey ->
            CollectionAssert.AreEqual (
                Seq.toArray (Map.keys (toMap oneBinding)),
                Seq.toArray (SortedMap.keys (SortedMap.ofSeqBy (fun struct (key, _) -> key) oneBinding)),
                message
            )

    [<TestMethod; DynamicData(nameof OrderedKeys.Several, typeof<OrderedKeys>)>]
    member _.``the functions that visit the bindings follow the order of Map`` (keys : OrderedKeys) =
        match keys with
        | OrderedKeys.Strings -> assertVisitingOrder stringBindings
        | OrderedKeys.Negatives -> assertVisitingOrder negativeBindings
        | OrderedKeys.Tuples -> assertVisitingOrder tupleBindings
        | OrderedKeys.OneKey -> assertVisitingOrder oneBinding

    [<TestMethod>]
    member _.``foldBack visits the keys from the largest to the smallest`` () =
        let table = SortedMap.ofArray stringBindings
        let visited = SortedMap.foldBack (fun key _ keys -> Array.append [| key |] keys) table [||]
        let reversed = SortedMap.foldBack (fun key _ keys -> Array.append keys [| key |]) table [||]

        CollectionAssert.AreEqual (
            [| "10"; "9"; "A"; "B"; "_"; "a"; "b" |],
            visited,
            "SortedMap.foldBack must cons the keys into their ordinal order"
        )

        CollectionAssert.AreEqual (
            [| "b"; "a"; "_"; "B"; "A"; "9"; "10" |],
            reversed,
            "SortedMap.foldBack must visit the largest key first"
        )

    [<TestMethod; DynamicData(nameof OrderedKeys.Several, typeof<OrderedKeys>)>]
    member _.``the transformations that rebuild the map keep the order of Map`` (keys : OrderedKeys) =
        match keys with
        | OrderedKeys.Strings -> assertTransformationOrder stringBindings
        | OrderedKeys.Negatives -> assertTransformationOrder negativeBindings
        | OrderedKeys.Tuples -> assertTransformationOrder tupleBindings
        | OrderedKeys.OneKey -> assertTransformationOrder oneBinding

    [<TestMethod; DynamicData(nameof OrderedKeys.All, typeof<OrderedKeys>)>]
    member _.``minKeyValue returns the binding of the smallest key as Map does`` (keys : OrderedKeys) =
        match keys with
        | OrderedKeys.Strings -> assertMinimum stringBindings
        | OrderedKeys.Negatives -> assertMinimum negativeBindings
        | OrderedKeys.Tuples -> assertMinimum tupleBindings
        | OrderedKeys.OneKey -> assertMinimum oneBinding

    [<TestMethod>]
    member _.``minKeyValue returns the first key in the order of the key comparer`` () =
        Assert.AreEqual<struct (int * string)>(
            struct (8, "eight"),
            SortedMap.minKeyValue ((SortedMap.forKeyComparer descending).ofArray negativeBindings),
            "SortedMap.minKeyValue must return the first key in the order of the key comparer"
        )

    [<TestMethod; DynamicData(nameof OrderedKeys.All, typeof<OrderedKeys>)>]
    member _.``maxKeyValue returns the binding of the largest key as Map does`` (keys : OrderedKeys) =
        match keys with
        | OrderedKeys.Strings -> assertMaximum stringBindings
        | OrderedKeys.Negatives -> assertMaximum negativeBindings
        | OrderedKeys.Tuples -> assertMaximum tupleBindings
        | OrderedKeys.OneKey -> assertMaximum oneBinding

    [<TestMethod>]
    member _.``maxKeyValue returns the last key in the order of the key comparer`` () =
        Assert.AreEqual<struct (int * string)>(
            struct (-12, "minus twelve"),
            SortedMap.maxKeyValue ((SortedMap.forKeyComparer descending).ofArray negativeBindings),
            "SortedMap.maxKeyValue must return the last key in the order of the key comparer"
        )

    [<TestMethod>]
    member _.``minKeyValue throws for an empty map as Map does`` () =
        ExceptionAssert.Same (
            id,
            (fun () -> Map.minKeyValue Map.empty<int, string> |> ignore),
            (fun () -> SortedMap.minKeyValue SortedMap.empty<int, string> |> ignore),
            "SortedMap.minKeyValue must reject an empty map"
        )

    [<TestMethod>]
    member _.``maxKeyValue throws for an empty map as Map does`` () =
        ExceptionAssert.Same (
            id,
            (fun () -> Map.maxKeyValue Map.empty<int, string> |> ignore),
            (fun () -> SortedMap.maxKeyValue SortedMap.empty<int, string> |> ignore),
            "SortedMap.maxKeyValue must reject an empty map"
        )

    [<TestMethod>]
    member _.``singleton binds the key to the value`` () =
        // The module before ADR 0001 passed the argument of singleton as a comparer to ImmutableSortedDictionary.Create,
        // which built an empty map
        let table = SortedMap.singleton "key" "value"
        Assert.AreEqual<int>(1, SortedMap.count table, "SortedMap.singleton must create one binding")
        Assert.AreEqual<string>("value", SortedMap.find "key" table, "SortedMap.singleton must bind the key to the value")

        Assert.AreSame<IComparer<string>>(
            LanguagePrimitives.FastGenericComparer<string>,
            SortedMap.keyComparer table,
            "SortedMap.singleton must order the keys with F# generic comparison"
        )

    [<TestMethod>]
    member _.``find throws the exception of Map.find, not the one of the indexer`` () =
        let table = defaultTable ()

        let ex =
            Assert.ThrowsExactly<KeyNotFoundException>(
                Action (fun () -> SortedMap.find "three" table |> ignore),
                "SortedMap.find must reject a key without a binding"
            )

        Assert.AreEqual<string>(
            KeyNotFoundException().Message,
            ex.Message,
            "SortedMap.find must throw with the message of Map.find, which does not name the key"
        )

    [<TestMethod; DynamicData(nameof SortedMapTests.DefaultCreations)>]
    member _.``the creation functions without a comparer use F# generic comparison and the default value comparer``
        (name : string)
        =
        let table = defaultCreations[name]()

        // The comparer of F# generic comparison, which Map uses, and not the culture-sensitive Comparer<string>.Default
        Assert.AreSame<IComparer<string>>(
            LanguagePrimitives.FastGenericComparer<string>,
            SortedMap.keyComparer table,
            $"SortedMap.%s{name} must order the keys with F# generic comparison"
        )

        Assert.AreSame<IEqualityComparer<string>>(
            EqualityComparer<string>.Default,
            SortedMap.valueComparer table,
            $"SortedMap.%s{name} must compare the values with the default comparer"
        )

    [<TestMethod; DynamicData(nameof SortedMapTests.TupleKeyCreations)>]
    member _.``the creation functions share one comparer of F# generic comparison per type of the keys`` (name : string) =
        // Comparers created per call would give maps of one type of keys different comparers, and the BCL takes its
        // shortcuts for two maps only when they have the same comparer (ADR 0001, D5)
        Assert.AreSame (
            SortedMap.keyComparer SortedMap.empty<int * string, int>,
            SortedMap.keyComparer (tupleKeyCreations[name]()),
            $"SortedMap.%s{name} must order its keys with the comparer that every sorted map of the type of its keys shares"
        )

    [<TestMethod>]
    member _.``empty is one map per type of the keys and the values`` () =
        Assert.AreSame (SortedMap.empty<string, int>, SortedMap.empty<string, int>, "SortedMap.empty must be created once")
        Assert.IsTrue (SortedMap.isEmpty SortedMap.empty<int, string>, "SortedMap.empty must have no bindings")

    [<TestMethod>]
    member _.``ofSeqBy rejects a repeated key with another value and accepts a repeated element, as the BCL does`` () =
        Assert.ThrowsExactly<ArgumentException>(
            Action (fun () ->
                SortedMap.ofSeqBy (fun (value : int) -> string (value % 2)) [| 1; 3 |]
                |> ignore
            ),
            "SortedMap.ofSeqBy must reject a repeated key with another value, as ImmutableSortedDictionary.ToImmutableSortedDictionary does"
        )
        |> ignore

        let table = SortedMap.ofSeqBy (fun (value : int) -> string value) [| 1; 1 |]
        Assert.AreEqual<int>(1, SortedMap.count table, "SortedMap.ofSeqBy must keep one binding of a repeated element")

    [<TestMethod>]
    member _.``ofSeqBy keeps the first of the elements of a key that the default comparer finds equal`` () =
        // ImmutableSortedDictionary.ToImmutableSortedDictionary keeps the first binding of a key for an equal value
        // (ADR 0001, D14)
        let first = MapInputs.Copy "uno"
        let table = SortedMap.ofSeqBy (fun (value : string) -> value.Length) [| first; MapInputs.Copy "uno" |]

        Assert.AreSame (
            first,
            SortedMap.find 3 table,
            "SortedMap.ofSeqBy must keep the first of the elements that the default comparer finds equal"
        )

    [<TestMethod; DynamicData(nameof SortedMapTests.Transformations)>]
    member _.``the transformations keep the key comparer`` (name : string) =
        Assert.AreSame<IComparer<string>>(
            StringComparer.OrdinalIgnoreCase,
            resultKeyComparers[name](customTable ()),
            $"SortedMap.%s{name} must keep the key comparer"
        )

    [<TestMethod; DynamicData(nameof SortedMapTests.SameTypeTransformations)>]
    member _.``the transformations that keep the type of the values keep the value comparer`` (name : string) =
        Assert.AreSame (
            byLength,
            SortedMap.valueComparer (sameTypeTransformations[name](customTable ())),
            $"SortedMap.%s{name} must keep the value comparer"
        )

    [<TestMethod; DynamicData(nameof SortedMapTests.TypeChangingTransformations)>]
    member _.``map and choose keep the key comparer and give the new values the default comparer`` (name : string) =
        let result =
            descendingTransformations[name]((SortedMap.forKeyComparer descending).ofArray negativeBindings)

        Assert.AreSame<IComparer<int>>(descending, SortedMap.keyComparer result, $"SortedMap.%s{name} must keep the key comparer")

        CollectionAssert.AreEqual (
            [| 8; 5; 0; -3; -12 |],
            Seq.toArray (SortedMap.keys result),
            $"SortedMap.%s{name} must keep the order of the key comparer"
        )

        Assert.AreSame<IEqualityComparer<int>>(
            EqualityComparer<int>.Default,
            SortedMap.valueComparer result,
            $"SortedMap.%s{name} must compare the new values with the default comparer"
        )

    [<TestMethod>]
    member _.``containsValue compares with the value comparer`` () =
        let table = customTable ()
        Assert.IsTrue (SortedMap.containsValue "abc" table, "SortedMap.containsValue must find a value of the same length")
        Assert.IsTrue (SortedMap.containsValue "tres" table, "SortedMap.containsValue must find an equal value")
        Assert.IsFalse (SortedMap.containsValue "abcdef" table, "SortedMap.containsValue must not find a value of another length")

    [<TestMethod>]
    member _.``tryGetKey returns the key that the map holds`` () =
        let table = customTable ()

        Assert.AreEqual<string voption>(
            ValueSome "Two",
            SortedMap.tryGetKey "TWO" table,
            "SortedMap.tryGetKey must return the stored key"
        )

        Assert.AreEqual<string voption>(
            ValueNone,
            SortedMap.tryGetKey "four" table,
            "SortedMap.tryGetKey must return nothing for a missing key"
        )

    [<TestMethod>]
    member _.``keys and values are read-only views that ask the map`` () =
        let table = customTable ()
        let keys = SortedMap.keys table
        Assert.HasCount (3, keys, "SortedMap.keys must have a key per binding")
        Assert.IsTrue (keys.Contains "THREE", "SortedMap.keys must look the keys up with the key comparer")
        Assert.IsTrue (keys.IsReadOnly, "SortedMap.keys must be read-only")

        Assert.ThrowsExactly<NotSupportedException>(Action (fun () -> keys.Add "four"), "SortedMap.keys must reject a change")
        |> ignore

        let values = SortedMap.values table
        Assert.HasCount (3, values, "SortedMap.values must have a value per binding")
        Assert.IsTrue (values.Contains "xyz", "SortedMap.values must look the values up with the value comparer")
        Assert.IsTrue (values.IsReadOnly, "SortedMap.values must be read-only")

    [<TestMethod; DynamicData(nameof SortedMapTests.UnchangingCalls)>]
    member _.``the functions that change nothing return the input itself`` (name : string) =
        let table = defaultTable ()

        Assert.AreSame (
            table,
            unchangingCalls[name] table,
            $"SortedMap.%s{name} must return the input itself when it changes nothing"
        )

    [<TestMethod; DynamicData(nameof SortedMapTests.EqualValueCalls)>]
    member _.``a value that the value comparer finds equal leaves the binding as it is`` (name : string) =
        // ImmutableSortedDictionary.SetItem and AddRange keep the binding, its key included, for an equal value (ADR 0001, D14)
        let table = customTable ()
        let result = equalValueCalls[name] table
        Assert.AreEqual<string>("uno", SortedMap.find "one" result, $"SortedMap.%s{name} must keep the current value")

        Assert.AreEqual<string voption>(
            ValueSome "one",
            SortedMap.tryGetKey "ONE" result,
            $"SortedMap.%s{name} must keep the current key"
        )

        Assert.AreSame (table, result, $"SortedMap.%s{name} must return the input itself")

    [<TestMethod>]
    member _.``clear returns an empty map itself`` () =
        let empty = SortedMap.clear (defaultTable ())
        Assert.AreSame (empty, SortedMap.clear empty, "SortedMap.clear must return an empty input itself")

    [<TestMethod>]
    member _.``add throws what ImmutableSortedDictionary.Add throws for a key that is bound to another value`` () =
        let table = defaultTable ()

        ExceptionAssert.Same (
            id,
            (fun () -> table.Add ("one", "other") |> ignore),
            (fun () -> SortedMap.add "one" "other" table |> ignore),
            "SortedMap.add must throw what ImmutableSortedDictionary.Add throws"
        )

    [<TestMethod>]
    member _.``add rejects a value that the value comparer finds different`` () =
        // The comparer finds "uno" and "longer" different, as it finds "uno" and "abc" equal
        Assert.ThrowsExactly<ArgumentException>(
            Action (fun () -> SortedMap.add "ONE" "longer" (customTable ()) |> ignore),
            "SortedMap.add must reject a value that the value comparer finds different from the current one"
        )
        |> ignore

    [<TestMethod>]
    member _.``setItem replaces the value of a key that add rejects`` () =
        let table = defaultTable ()
        let replaced = SortedMap.setItem "one" "other" table
        Assert.AreEqual<string>("other", SortedMap.find "one" replaced, "SortedMap.setItem must replace the value of the key")
        Assert.AreEqual<string>("uno", SortedMap.find "one" table, "SortedMap.setItem must not change the input")

    [<TestMethod>]
    member _.``containsBinding compares the value with the value comparer where tryFind and equality do not`` () =
        let table =
            (SortedMap.forComparers StringComparer.Ordinal StringComparer.OrdinalIgnoreCase).ofArray [| "one", "Uno" |]

        Assert.IsTrue (
            SortedMap.containsBinding "one" "UNO" table,
            "SortedMap.containsBinding must find a value that the value comparer finds equal"
        )

        Assert.IsFalse (
            SortedMap.tryFind "one" table
            |> ValueOption.exists (fun stored -> stored = "UNO"),
            "F# equality finds \"Uno\" and \"UNO\" different, which is why the function asks the value comparer"
        )

        Assert.IsFalse (SortedMap.containsBinding "one" "DOS" table, "SortedMap.containsBinding must not find another value")

        Assert.IsFalse (
            SortedMap.containsBinding "ONE" "Uno" table,
            "SortedMap.containsBinding must look the key up with the key comparer"
        )

        Assert.IsFalse (
            SortedMap.containsBinding "two" "Uno" table,
            "SortedMap.containsBinding must not find a key without a binding"
        )

    [<TestMethod; DataRow("ONE", "xyz", true); DataRow("ONE", "xy", false); DataRow("four", "xyz", false)>]
    member _.``containsBinding finds the key in the key comparer and the value in the value comparer``
        (key : string, value : string, expected : bool)
        =
        // The keys ignore the case and the values compare by their length: "one" is bound to "uno"
        Assert.AreEqual<bool>(
            expected,
            SortedMap.containsBinding key value (customTable ()),
            "SortedMap.containsBinding must compare the key and the value with the comparers of the map"
        )

    [<TestMethod>]
    member _.``containsBinding throws for a null key as ImmutableSortedDictionary.Contains does`` () =
        let nullKey = Unchecked.defaultof<string>
        let table = defaultTable ()

        // The member names its parameter Key, where ContainsKey names it key
        ExceptionAssert.Same (
            id,
            (fun () -> table.Contains (KeyValuePair (nullKey, "uno")) |> ignore),
            (fun () -> SortedMap.containsBinding nullKey "uno" table |> ignore),
            "SortedMap.containsBinding must throw what ImmutableSortedDictionary.Contains throws for a null key"
        )

        Assert.ThrowsExactly<ArgumentNullException>(
            Action (fun () -> SortedMap.containsKey nullKey table |> ignore),
            "SortedMap.containsKey must reject a null key too"
        )
        |> ignore

    [<TestMethod; DataRow "one"; DataRow "three">]
    member _.``tryFind followed by defaultValue gives what GetValueOrDefault gives`` (key : string) =
        let table = defaultTable ()

        Assert.AreEqual<string>(
            ImmutableDictionary.GetValueOrDefault (table, key, "fallback"),
            SortedMap.tryFind key table
            |> ValueOption.defaultValue "fallback",
            "tryFind followed by defaultValue must give the value of the key, or the default value of a key without a binding"
        )

    [<TestMethod; DataRow("one", "uno"); DataRow("three", "tres")>]
    member _.``change with a function that keeps an existing value adds a binding only when the key has none``
        (key : string, expected : string)
        =
        let table = defaultTable ()

        let added =
            SortedMap.change
                key
                (function
                | ValueNone -> ValueSome "tres"
                | existing -> existing)
                table

        Assert.AreEqual<string>(
            expected,
            SortedMap.find key added,
            "SortedMap.change must keep the value of a key with a binding and add the value of a key without one"
        )

    [<TestMethod>]
    member _.``addRange to an empty map rejects a repeated key with another value as CreateRange does`` () =
        let pairs = [| KeyValuePair ("a", "x"); KeyValuePair ("a", "y") |]

        ExceptionAssert.Same (
            id,
            (fun () -> ImmutableSortedDictionary.CreateRange pairs |> ignore),
            (fun () -> SortedMap.empty |> SortedMap.addRange pairs |> ignore),
            "SortedMap.addRange must reject what ImmutableSortedDictionary.CreateRange rejects"
        )

        Assert.AreEqual<string>(
            "y",
            SortedMap.find "a" (SortedMap.ofSeq pairs),
            "SortedMap.ofSeq must keep the last binding of a repeated key, where CreateRange throws"
        )

    [<TestMethod>]
    member _.``addRange of the mapped pairs rejects what ToImmutableSortedDictionary with an element selector rejects`` () =
        // The keys are the lengths of the texts, the elements are the upper case texts: "a" and "b" share a key
        let source = [| "a"; "b"; "cc" |]
        let key (text : string) = text.Length
        let element (text : string) = text.ToUpperInvariant ()
        let pairs =
            source
            |> Seq.map (fun text -> KeyValuePair (key text, element text))

        ExceptionAssert.Same (
            id,
            (fun () ->
                ImmutableSortedDictionary.ToImmutableSortedDictionary (
                    source,
                    Func<string, int> key,
                    Func<string, string> element
                )
                |> ignore
            ),
            (fun () -> SortedMap.empty |> SortedMap.addRange pairs |> ignore),
            "SortedMap.addRange of the mapped pairs must reject a repeated key like ToImmutableSortedDictionary does"
        )

        Assert.AreEqual<string>(
            "B",
            SortedMap.find 1 (SortedMap.ofSeq pairs),
            "SortedMap.ofSeq of the mapped pairs must keep the last binding of a repeated key"
        )

    [<TestMethod>]
    member _.``addRange of the mapped pairs accepts a repeated key with an equal element as ToImmutableSortedDictionary does``
        ()
        =
        let source = [| "a"; "a"; "cc" |]
        let key (text : string) = text.Length
        let element (text : string) = text.ToUpperInvariant ()
        let pairs =
            source
            |> Seq.map (fun text -> KeyValuePair (key text, element text))

        let expected =
            ImmutableSortedDictionary.ToImmutableSortedDictionary (source, Func<string, int> key, Func<string, string> element)

        CollectionAssert.AreEqual (
            Seq.toArray expected,
            Seq.toArray (SortedMap.empty |> SortedMap.addRange pairs),
            "SortedMap.addRange of the mapped pairs must accept a repeated key with an equal element, as ToImmutableSortedDictionary does"
        )
