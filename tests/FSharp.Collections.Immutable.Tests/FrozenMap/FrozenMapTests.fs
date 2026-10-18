namespace FSharp.Collections.Immutable.Tests

open System
open System.Collections.Generic
open System.Runtime.InteropServices
open FSharp.Collections.Immutable
open Microsoft.VisualStudio.TestTools.UnitTesting

/// The behaviour of `FrozenMap` that the shared contracts do not cover: null inputs, the key comparer, the arrays of the
/// keys and the values, and the repeated keys of the functions that wrap the BCL.
[<TestClass; FrozenMapTestCategory>]
type FrozenMapTests () =

    static let nullTable = Unchecked.defaultof<FrozenMap<string, string>>

    /// A map whose keys ignore the case.
    static let ignoreCaseTable () =
        (FrozenMap.forKeyComparer StringComparer.OrdinalIgnoreCase).ofArray [| "one", "uno"; "Two", "dos"; "three", "tres" |]

    /// A map with the default comparer.
    static let defaultTable () = FrozenMap.ofArray [| struct ("one", "uno"); struct ("two", "dos") |]

    /// The array under a flat list, to tell whether two flat lists share their storage.
    static let underlying (array : FlatList<'T>) = ImmutableCollectionsMarshal.AsArray array

    /// The functions that take a map, by name, on a null map. The rows of the tests are the names: a function has no
    /// readable name in a row.
    static let nullTableCalls : Map<string, unit -> unit> =
        Map.ofArray [|
            nameof FrozenMap.choose,
            (fun () ->
                FrozenMap.choose (fun _ value -> ValueSome value) nullTable
                |> ignore
            )
            nameof FrozenMap.containsBinding, (fun () -> FrozenMap.containsBinding "a" "b" nullTable |> ignore)
            nameof FrozenMap.containsKey, (fun () -> FrozenMap.containsKey "a" nullTable |> ignore)
            nameof FrozenMap.containsValue, (fun () -> FrozenMap.containsValue "a" nullTable |> ignore)
            nameof FrozenMap.copyTo, (fun () -> FrozenMap.copyTo [||] 0 nullTable)
            nameof FrozenMap.count, (fun () -> FrozenMap.count nullTable |> ignore)
            nameof FrozenMap.exists, (fun () -> FrozenMap.exists (fun _ _ -> true) nullTable |> ignore)
            nameof FrozenMap.filter, (fun () -> FrozenMap.filter (fun _ _ -> true) nullTable |> ignore)
            nameof FrozenMap.find, (fun () -> FrozenMap.find "a" nullTable |> ignore)
            nameof FrozenMap.findKey, (fun () -> FrozenMap.findKey (fun _ _ -> true) nullTable |> ignore)
            nameof FrozenMap.fold,
            (fun () ->
                FrozenMap.fold (fun state _ _ -> state) 0 nullTable
                |> ignore
            )
            nameof FrozenMap.foldBack,
            (fun () ->
                FrozenMap.foldBack (fun _ _ state -> state) nullTable 0
                |> ignore
            )
            nameof FrozenMap.forall, (fun () -> FrozenMap.forall (fun _ _ -> true) nullTable |> ignore)
            nameof FrozenMap.isEmpty, (fun () -> FrozenMap.isEmpty nullTable |> ignore)
            nameof FrozenMap.iter, (fun () -> FrozenMap.iter (fun _ _ -> ()) nullTable)
            nameof FrozenMap.keyComparer, (fun () -> FrozenMap.keyComparer nullTable |> ignore)
            nameof FrozenMap.keys, (fun () -> FrozenMap.keys nullTable |> ignore)
            nameof FrozenMap.map, (fun () -> FrozenMap.map (fun _ value -> value) nullTable |> ignore)
            nameof FrozenMap.partition, (fun () -> FrozenMap.partition (fun _ _ -> true) nullTable |> ignore)
            nameof FrozenMap.pick,
            (fun () ->
                FrozenMap.pick (fun _ value -> ValueSome value) nullTable
                |> ignore
            )
            nameof FrozenMap.toArray, (fun () -> FrozenMap.toArray nullTable |> ignore)
            nameof FrozenMap.toList, (fun () -> FrozenMap.toList nullTable |> ignore)
            nameof FrozenMap.toSeq, (fun () -> FrozenMap.toSeq nullTable |> ignore)
            nameof FrozenMap.tryFind, (fun () -> FrozenMap.tryFind "a" nullTable |> ignore)
            nameof FrozenMap.tryFindKey, (fun () -> FrozenMap.tryFindKey (fun _ _ -> true) nullTable |> ignore)
            nameof FrozenMap.tryPick,
            (fun () ->
                FrozenMap.tryPick (fun _ value -> ValueSome value) nullTable
                |> ignore
            )
            nameof FrozenMap.values, (fun () -> FrozenMap.values nullTable |> ignore)
        |]

    /// The functions that take a sequence, by name, with the name of the sequence parameter and the function on a null
    /// sequence.
    static let nullSequenceCalls : Map<string, struct (string * (unit -> unit))> =
        Map.ofArray [|
            "ofArray of key/value pairs",
            struct ("elements",
                    fun () ->
                        FrozenMap.ofArray (Unchecked.defaultof<KeyValuePair<string, string> array>)
                        |> ignore)
            "ofArray of reference tuples",
            struct ("elements",
                    fun () ->
                        FrozenMap.ofArray (Unchecked.defaultof<(string * string) array>)
                        |> ignore)
            "ofArray of struct tuples",
            struct ("elements",
                    fun () ->
                        FrozenMap.ofArray (Unchecked.defaultof<struct (string * string) array>)
                        |> ignore)
            "ofSeq of key/value pairs",
            struct ("elements",
                    fun () ->
                        FrozenMap.ofSeq (Unchecked.defaultof<seq<KeyValuePair<string, string>>>)
                        |> ignore)
            "ofSeq of reference tuples",
            struct ("elements",
                    fun () ->
                        FrozenMap.ofSeq (Unchecked.defaultof<seq<string * string>>)
                        |> ignore)
            "ofSeq of struct tuples",
            struct ("elements",
                    fun () ->
                        FrozenMap.ofSeq (Unchecked.defaultof<seq<struct (string * string)>>)
                        |> ignore)
            nameof FrozenMap.ofSeqBy,
            struct ("source",
                    fun () ->
                        FrozenMap.ofSeqBy id (Unchecked.defaultof<seq<string>>)
                        |> ignore)
        |]

    /// The functions that take a sequence, by name, with the name of the sequence parameter and the function on a default
    /// flat list that is given as the sequence. A default flat list converts to a sequence without a check and fails with
    /// InvalidOperationException when the sequence is enumerated, so the function would enumerate it. The last row gives a
    /// sequence of objects, which a default flat list of strings is by the covariance of seq: the element type of the
    /// sequence is not the one of the flat list. The extension overloads of ofSeq run through the struct.
    static let defaultSequenceCalls : Map<string, struct (string * (unit -> unit))> =
        let defaultPairs = Unchecked.defaultof<FlatList<KeyValuePair<string, string>>> :> seq<_>
        let defaultTuples = Unchecked.defaultof<FlatList<string * string>> :> seq<_>
        let defaultStructTuples = Unchecked.defaultof<FlatList<struct (string * string)>> :> seq<_>
        let defaultKeys = Unchecked.defaultof<FlatList<string>> :> seq<_>

        Map.ofArray [|
            "ofSeq of key/value pairs", struct ("elements", fun () -> FrozenMap.ofSeq defaultPairs |> ignore)
            "ofSeq of reference tuples", struct ("elements", fun () -> FrozenMap.ofSeq defaultTuples |> ignore)
            "ofSeq of struct tuples", struct ("elements", fun () -> FrozenMap.ofSeq defaultStructTuples |> ignore)
            nameof FrozenMap.ofSeqBy, struct ("source", fun () -> FrozenMap.ofSeqBy id defaultKeys |> ignore)
            $"%s{nameof FrozenMap.ofSeqBy} (default flat list of strings as objects)",
            struct ("source",
                    fun () ->
                        FrozenMap.ofSeqBy id FlatListOracles.defaultStringsAsObjects
                        |> ignore)
        |]

    /// The creation functions without a comparer, by name and shape of the bindings.
    static let defaultCreations : Map<string, unit -> FrozenMap<string, string>> =
        Map.ofArray [|
            nameof FrozenMap.empty, (fun () -> FrozenMap.empty)
            "ofArray of key/value pairs", (fun () -> FrozenMap.ofArray [| KeyValuePair ("a", "b") |])
            "ofArray of reference tuples", (fun () -> FrozenMap.ofArray [| ("a", "b") |])
            "ofArray of struct tuples", (fun () -> FrozenMap.ofArray [| struct ("a", "b") |])
            "ofList of key/value pairs", (fun () -> FrozenMap.ofList [ KeyValuePair ("a", "b") ])
            "ofList of reference tuples", (fun () -> FrozenMap.ofList [ ("a", "b") ])
            "ofList of struct tuples", (fun () -> FrozenMap.ofList [ struct ("a", "b") ])
            "ofSeq of key/value pairs", (fun () -> FrozenMap.ofSeq (dict [| "a", "b" |]))
            "ofSeq of reference tuples", (fun () -> FrozenMap.ofSeq (Seq.singleton ("a", "b")))
            "ofSeq of struct tuples", (fun () -> FrozenMap.ofSeq (Seq.singleton (struct ("a", "b"))))
            nameof FrozenMap.ofSeqBy, (fun () -> FrozenMap.ofSeqBy id [| "a" |])
            nameof FrozenMap.singleton, (fun () -> FrozenMap.singleton "a" "b")
        |]

    /// The key comparer of the result of every transformation of the map whose keys ignore the case, by name. The
    /// transformations drop some, all or none of the bindings, and change the values or keep them.
    static let resultKeyComparers : Map<string, FrozenMap<string, string> -> IEqualityComparer<string>> =
        Map.ofArray [|
            $"%s{nameof FrozenMap.choose} (some values)",
            fun table ->
                FrozenMap.choose (fun _ (value : string) -> ValueSome value.Length) table
                |> FrozenMap.keyComparer
            $"%s{nameof FrozenMap.choose} (no value)",
            fun table ->
                FrozenMap.choose (fun _ _ -> ValueNone) table
                |> FrozenMap.keyComparer
            $"%s{nameof FrozenMap.filter} (some bindings)",
            fun table ->
                FrozenMap.filter (fun (key : string) _ -> key.Length > 3) table
                |> FrozenMap.keyComparer
            $"%s{nameof FrozenMap.filter} (no binding)",
            fun table ->
                FrozenMap.filter (fun _ _ -> false) table
                |> FrozenMap.keyComparer
            nameof FrozenMap.map,
            fun table ->
                FrozenMap.map (fun _ (value : string) -> value.Length) table
                |> FrozenMap.keyComparer
            $"%s{nameof FrozenMap.partition} (accepted)",
            fun table ->
                let struct (accepted, _) = FrozenMap.partition (fun (key : string) _ -> key.Length > 3) table
                FrozenMap.keyComparer accepted
            $"%s{nameof FrozenMap.partition} (rejected)",
            fun table ->
                let struct (_, rejected) = FrozenMap.partition (fun (key : string) _ -> key.Length > 3) table
                FrozenMap.keyComparer rejected
            $"%s{nameof FrozenMap.partition} (rejected, nothing)",
            fun table ->
                let struct (_, nothing) = FrozenMap.partition (fun _ _ -> true) table
                FrozenMap.keyComparer nothing
        |]

    /// The transformations of a map of integer keys, by name; the key comparer of the map compares the absolute values of
    /// the keys.
    static let absoluteKeyTransformations : Map<string, FrozenMap<int, string> -> FrozenMap<int, string>> =
        Map.ofArray [|
            nameof FrozenMap.choose, FrozenMap.choose (fun _ value -> ValueSome value)
            nameof FrozenMap.filter, FrozenMap.filter (fun key _ -> key > 1)
            nameof FrozenMap.map, FrozenMap.map (fun _ value -> value)
            $"%s{nameof FrozenMap.partition} (accepted)",
            fun table ->
                let struct (accepted, _) = FrozenMap.partition (fun key _ -> key % 2 = 0) table
                accepted
            $"%s{nameof FrozenMap.partition} (rejected)",
            fun table ->
                let struct (_, rejected) = FrozenMap.partition (fun key _ -> key % 2 = 0) table
                rejected
        |]

    /// Maps of every kind that the frozen dictionary tells apart, by name: it picks its implementation by the type of the
    /// keys, the comparer and the size.
    static let tablesOfEveryKind : Map<string, unit -> FrozenMap<string, int>> =
        Map.ofArray [|
            "an empty map", (fun () -> FrozenMap.empty)
            "a small map", (fun () -> FrozenMap.ofArray [| struct ("one", 1); struct ("two", 2) |])
            "a large map", (fun () -> FrozenMap.ofArray [| for index in 1..50 -> struct (string index, index) |])
            "a map with a custom comparer",
            (fun () ->
                (FrozenMap.forKeyComparer HashIdentity.Structural).ofArray [|
                    for index in 1..50 -> struct (string index, index)
                |]
            )
        |]

    /// The creations of a frozen map from the key/value pairs of an array, by the function that takes the span of the array.
    /// A span cannot be captured, so each function makes its span itself.
    static let spanCreations : Map<string, KeyValuePair<int, string> array -> FrozenMap<int, string>> =
        Map.ofArray [|
            nameof FrozenMap.ofReadOnlySpan, (fun pairs -> FrozenMap.ofReadOnlySpan (ReadOnlySpan pairs))
            nameof FrozenMap.ofSpan, (fun pairs -> FrozenMap.ofSpan (Span pairs))
        |]

    /// The creations of a frozen map of string keys from the key/value pairs of an array, by the function that takes the span
    /// of the array.
    static let stringSpanCreations : Map<string, KeyValuePair<string, string> array -> FrozenMap<string, string>> =
        Map.ofArray [|
            nameof FrozenMap.ofReadOnlySpan, (fun pairs -> FrozenMap.ofReadOnlySpan (ReadOnlySpan pairs))
            nameof FrozenMap.ofSpan, (fun pairs -> FrozenMap.ofSpan (Span pairs))
        |]

    /// The names of the functions that take a map, as the rows of their test.
    static member NullTableCalls = nullTableCalls.Keys

    /// The names of the functions that take a sequence, as the rows of their test.
    static member NullSequenceCalls = nullSequenceCalls.Keys

    /// The names of the functions that take a sequence and are given a default flat list, as the rows of their test.
    static member DefaultSequenceCalls = defaultSequenceCalls.Keys

    /// The names of the creation functions without a comparer, as the rows of their test.
    static member DefaultCreations = defaultCreations.Keys

    /// The names of every transformation, as the rows of the test of the key comparer.
    static member Transformations = resultKeyComparers.Keys

    /// Every size of a map of a custom comparer with every transformation: the rows of the test of the key comparer of the
    /// transformations. The frozen dictionaries of a custom comparer search a few keys linearly and hash more keys.
    static member SizesAndAbsoluteKeyTransformations = seq {
        for size in [| 3; 30 |] do
            for name in absoluteKeyTransformations.Keys do
                struct (size, name)
    }

    /// The names of the maps of every kind, as the rows of the tests that run on each kind.
    static member TablesOfEveryKind = tablesOfEveryKind.Keys

    /// The names of the functions that create a frozen map from a span, as the rows of the tests of the span creations.
    static member SpanCreations = spanCreations.Keys

    /// The names of the functions that create a frozen map of string keys from a span, as the rows of the tests of the
    /// copies of strings.
    static member StringSpanCreations = stringSpanCreations.Keys

    /// Every function that creates a frozen map from a span with every input of the creation tests, the bindings that repeat
    /// keys included: the rows of the test of the bindings. The rows of every property are built on each read.
    static member SpanCreationsAndInputs = seq {
        for name in spanCreations.Keys do
            for input in
                seq {
                    yield! MapInputs.All
                    MapInputs.RepeatedKeys
                } do
                struct (name, input)
    }

    /// Every input and every index of the first binding in the destination of copyTo, with a free slot after the last
    /// binding: the rows of the test of the copy.
    static member CopyToInputsAndIndexes = seq {
        for input in MapInputs.All do
            for index in [| 0; 2 |] do
                struct (input, index)
    }

    [<TestMethod; DynamicData(nameof FrozenMapTests.NullTableCalls)>]
    member _.``functions reject a null map`` (name : string) =
        // Map throws NullReferenceException for a null map; this library checks its arguments (ADR 0001, D6)
        let ex =
            Assert.ThrowsExactly<ArgumentNullException>(Action nullTableCalls[name], $"FrozenMap.%s{name} must reject a null map")

        Assert.AreEqual<string | null>("table", ex.ParamName, $"FrozenMap.%s{name} must name the map parameter")

    [<TestMethod; DynamicData(nameof FrozenMapTests.NullSequenceCalls)>]
    member _.``functions reject a null sequence`` (name : string) =
        let struct (paramName, call) = nullSequenceCalls[name]

        let ex =
            Assert.ThrowsExactly<ArgumentNullException>(Action call, $"FrozenMap.%s{name} must reject a null sequence")

        Assert.AreEqual<string | null>(paramName, ex.ParamName, $"FrozenMap.%s{name} must name the sequence parameter")

    [<TestMethod; DynamicData(nameof FrozenMapTests.DefaultSequenceCalls)>]
    member _.``functions reject a default flat list given as a sequence`` (name : string) =
        // The exception names the parameter of the function, not the one of the BCL member that it would have called
        let struct (paramName, call) = defaultSequenceCalls[name]
        FlatListAssert.RejectsNull ($"FrozenMap.%s{name}", paramName, call)

    [<TestMethod>]
    member _.``a flat list that is not default passes as a sequence, also one of strings as a sequence of objects`` () =
        let flatList (elements : 'T array) = FlatList.ofArray elements :> seq<'T>

        let bindingsOf (table : FrozenMap<string, string>) =
            FrozenMap.toArray table
            |> Array.sortBy (fun struct (key, _) -> key)

        CollectionAssert.AreEqual (
            [| struct ("a", "b") |],
            bindingsOf (FrozenMap.ofSeq (flatList [| KeyValuePair ("a", "b") |])),
            "FrozenMap.ofSeq must take the key/value pairs of a flat list"
        )

        CollectionAssert.AreEqual (
            [| struct ("a", "b") |],
            bindingsOf (FrozenMap.ofSeq (flatList [| ("a", "b") |])),
            "FrozenMap.ofSeq must take the reference tuples of a flat list"
        )

        CollectionAssert.AreEqual (
            [| struct ("a", "b") |],
            bindingsOf (FrozenMap.ofSeq (flatList [| struct ("a", "b") |])),
            "FrozenMap.ofSeq must take the struct tuples of a flat list"
        )

        CollectionAssert.AreEqual (
            [| struct ("ab", "ab") |],
            bindingsOf (FrozenMap.ofSeqBy id (flatList [| "ab" |])),
            "FrozenMap.ofSeqBy must take the elements of a flat list"
        )

        // A flat list of strings is a sequence of objects, which a type test for the flat list of objects does not see
        let widened = FlatListOracles.stringsAsObjects (FlatList.ofArray [| "ab" |])

        CollectionAssert.AreEqual (
            [| "ab" :> obj |],
            FrozenMap.keys (FrozenMap.ofSeqBy id widened) |> Seq.toArray,
            "FrozenMap.ofSeqBy must take the elements of a flat list of strings as objects"
        )

    [<TestMethod>]
    member _.``find throws the exception of Map.find, not the one of the indexer`` () =
        let table = defaultTable ()

        let ex =
            Assert.ThrowsExactly<KeyNotFoundException>(
                Action (fun () -> FrozenMap.find "three" table |> ignore),
                "FrozenMap.find must reject a key without a binding"
            )

        Assert.AreEqual<string>(
            KeyNotFoundException().Message,
            ex.Message,
            "FrozenMap.find must throw with the message of Map.find, which does not name the key"
        )

    [<TestMethod; DynamicData(nameof FrozenMapTests.DefaultCreations)>]
    member _.``the creation functions without a comparer use the default comparer`` (name : string) =
        Assert.AreSame<IEqualityComparer<string>>(
            EqualityComparer<string>.Default,
            FrozenMap.keyComparer (defaultCreations[name]()),
            $"FrozenMap.%s{name} must compare the keys with the default comparer"
        )

    // ToFrozenDictionary with a key selector rejects every repeated key, an equal element included (ADR 0001, D14)
    [<TestMethod; DataRow([| 1; 3 |]); DataRow([| 1; 1 |])>]
    member _.``ofSeqBy rejects a repeated key, even of an equal element`` (values : int[]) =
        Assert.ThrowsExactly<ArgumentException>(
            Action (fun () ->
                FrozenMap.ofSeqBy (fun (value : int) -> string (value % 2)) values
                |> ignore
            ),
            "FrozenMap.ofSeqBy must reject a repeated key, as ToFrozenDictionary with a key selector does"
        )
        |> ignore

    [<TestMethod; DynamicData(nameof FrozenMapTests.Transformations)>]
    member _.``the transformations keep the key comparer`` (name : string) =
        Assert.AreSame<IEqualityComparer<string>>(
            StringComparer.OrdinalIgnoreCase,
            resultKeyComparers[name](ignoreCaseTable ()),
            $"FrozenMap.%s{name} must keep the key comparer"
        )

    [<TestMethod>]
    member _.``the transformations look the keys up with the key comparer of the input`` () =
        let table = ignoreCaseTable ()
        let struct (accepted, rejected) = FrozenMap.partition (fun (key : string) _ -> key.Length > 3) table

        Assert.AreEqual<int voption>(
            ValueSome 3,
            FrozenMap.tryFind "ONE" (FrozenMap.map (fun _ (value : string) -> value.Length) table),
            "FrozenMap.map must look the keys up with the comparer of the input"
        )

        Assert.AreEqual<int voption>(
            ValueSome 3,
            FrozenMap.tryFind "TWO" (FrozenMap.choose (fun _ (value : string) -> ValueSome value.Length) table),
            "FrozenMap.choose must look the keys up with the comparer of the input"
        )

        Assert.IsTrue (
            FrozenMap.containsKey "THREE" (FrozenMap.filter (fun (key : string) _ -> key.Length > 3) table),
            "FrozenMap.filter must look the keys up with the comparer of the input"
        )

        Assert.IsTrue (FrozenMap.containsKey "THREE" accepted, "FrozenMap.partition must keep the comparer in the accepted part")
        Assert.IsTrue (FrozenMap.containsKey "TWO" rejected, "FrozenMap.partition must keep the comparer in the rejected part")

    [<TestMethod; DynamicData(nameof FrozenMapTests.SizesAndAbsoluteKeyTransformations)>]
    member _.``the transformations keep a custom key comparer of a small and a large map`` (size : int, name : string) =
        let table =
            (FrozenMap.forKeyComparer HashMapOps.absoluteKeyEquality).ofArray [| for key in 1..size -> struct (key, string key) |]

        let result = absoluteKeyTransformations[name] table

        Assert.AreSame (
            HashMapOps.absoluteKeyEquality,
            FrozenMap.keyComparer result,
            $"FrozenMap.%s{name} must keep the key comparer"
        )

        let key = FrozenMap.findKey (fun _ _ -> true) result

        Assert.IsTrue (FrozenMap.containsKey -key result, $"FrozenMap.%s{name} must look the keys up with the key comparer")

    [<TestMethod>]
    member _.``filter and partition return the input itself for a part with every binding`` () =
        let table = ignoreCaseTable ()
        let struct (accepted, _) = FrozenMap.partition (fun _ _ -> true) table
        let struct (_, rejected) = FrozenMap.partition (fun _ _ -> false) table

        Assert.AreSame (
            table,
            FrozenMap.filter (fun _ _ -> true) table,
            "FrozenMap.filter must return the input itself when it keeps every binding"
        )

        Assert.AreSame (table, accepted, "FrozenMap.partition must return the input itself when every binding is accepted")
        Assert.AreSame (table, rejected, "FrozenMap.partition must return the input itself when every binding is rejected")

        Assert.AreNotSame (
            table,
            FrozenMap.filter (fun (key : string) _ -> key.Length > 3) table,
            "FrozenMap.filter must build a new map when it drops a binding"
        )

    [<TestMethod; DynamicData(nameof FrozenMapTests.TablesOfEveryKind)>]
    member _.``keys and values share the arrays of the frozen map`` (name : string) =
        let table = tablesOfEveryKind[name]()

        Assert.AreSame (
            underlying table.Keys,
            underlying (FrozenMap.keys table),
            "FrozenMap.keys must share the array of the keys"
        )

        Assert.AreSame (
            underlying table.Values,
            underlying (FrozenMap.values table),
            "FrozenMap.values must share the array of the values"
        )

        CollectionAssert.AreEqual (
            FrozenMap.toArray table
            |> Array.map (fun struct (key, _) -> key),
            FlatList.toArray (FrozenMap.keys table),
            "FrozenMap.keys must follow the enumeration"
        )

        CollectionAssert.AreEqual (
            FrozenMap.toArray table
            |> Array.map (fun struct (_, value) -> value),
            FlatList.toArray (FrozenMap.values table),
            "FrozenMap.values must follow the enumeration"
        )

    [<TestMethod>]
    member _.``containsValue compares the values with their default comparer`` () =
        let table = ignoreCaseTable ()
        Assert.IsTrue (FrozenMap.containsValue "dos" table, "FrozenMap.containsValue must find an equal value")

        Assert.IsFalse (
            FrozenMap.containsValue "DOS" table,
            "FrozenMap.containsValue must compare the values with their default comparer, not with the key comparer"
        )

        Assert.IsFalse (FrozenMap.containsValue "cuatro" table, "FrozenMap.containsValue must not find a missing value")

    // The keys ignore the case, and the values compare with the default comparer: "one" is bound to "uno"
    [<TestMethod;
      DataRow("ONE", "uno", true);
      DataRow("ONE", "UNO", false);
      DataRow("one", "dos", false);
      DataRow("four", "uno", false)>]
    member _.``containsBinding finds the key in the key comparer and the value in the default comparer``
        (key : string, value : string, expected : bool)
        =
        Assert.AreEqual<bool>(
            expected,
            FrozenMap.containsBinding key value (ignoreCaseTable ()),
            "FrozenMap.containsBinding must compare the key with the key comparer and the value with the default comparer"
        )

    [<TestMethod>]
    member _.``containsBinding compares the values as EqualityComparer.Default does where tryFind and equality do not`` () =
        let table = FrozenMap.singleton "nan" nan

        Assert.IsTrue (
            FrozenMap.containsBinding "nan" nan table,
            "FrozenMap.containsBinding must find NaN, which EqualityComparer.Default finds equal to itself"
        )

        Assert.IsFalse (
            FrozenMap.tryFind "nan" table
            |> ValueOption.exists (fun stored -> stored = nan),
            "F# equality finds NaN different from itself, which is why the function asks EqualityComparer.Default"
        )

    [<TestMethod; DynamicData(nameof FrozenMapTests.TablesOfEveryKind)>]
    member _.``containsBinding rejects a null key`` (name : string) =
        let table = tablesOfEveryKind[name]()

        let ex =
            Assert.ThrowsExactly<ArgumentNullException>(
                Action (fun () ->
                    FrozenMap.containsBinding (Unchecked.defaultof<string>) 1 table
                    |> ignore
                ),
                "FrozenMap.containsBinding must reject a null key"
            )

        Assert.AreEqual<string | null>("key", ex.ParamName, "FrozenMap.containsBinding must name the key parameter")

    [<TestMethod; DynamicData(nameof FrozenMapTests.SpanCreationsAndInputs)>]
    member _.``the span creations create the bindings of a span as Map.ofSeq does``
        (name : string, input : struct (int * string) array)
        =
        let table = spanCreations[name](Array.map MapInputs.ToKeyValuePair input)

        CollectionAssert.AreEqual (
            MapInputs.OfMap (MapInputs.ToMap input),
            MapInputs.SortByKey (FrozenMap.toArray table),
            $"FrozenMap.%s{name} must keep the last binding of each key, as Map.ofSeq does"
        )

    [<TestMethod; DynamicData(nameof FrozenMapTests.SpanCreations)>]
    member _.``the span creations copy the span`` (name : string) =
        let pairs = [| KeyValuePair (1, "one") |]
        let table = spanCreations[name] pairs
        pairs[0] <- KeyValuePair (1, "changed")
        Assert.AreEqual<string>("one", FrozenMap.find 1 table, $"FrozenMap.%s{name} must copy the span")

    [<TestMethod; DynamicData(nameof FrozenMapTests.StringSpanCreations)>]
    member _.``the span creations keep the first key and the last value of keys that the default comparer finds equal``
        (name : string)
        =
        // The copies are equal in the default comparer and tell the two bindings apart (ADR 0001, D14)
        let firstKey = MapInputs.Copy "one"
        let lastValue = MapInputs.Copy "dos"

        let table =
            stringSpanCreations[name] [| KeyValuePair (firstKey, "uno"); KeyValuePair (MapInputs.Copy "one", lastValue) |]

        let struct (key, value) =
            Assert.ContainsSingle (FrozenMap.toArray table, $"FrozenMap.%s{name} must merge the bindings of equal keys")

        Assert.AreSame (firstKey, key, $"FrozenMap.%s{name} must keep the first key")
        Assert.AreSame (lastValue, value, $"FrozenMap.%s{name} must keep the last value")

    [<TestMethod>]
    member _.``ofReadOnlySpan takes an array or a Span when the types of the call are known`` () =
        let pairs = [| KeyValuePair ("one", "uno") |]

        // F# reports the conversions with FS3391, which is what the test is about
        #nowarn "3391"
        let fromArray : FrozenMap<string, string> = FrozenMap.ofReadOnlySpan pairs
        let fromSpan : FrozenMap<string, string> = FrozenMap.ofReadOnlySpan (Span pairs)
        #warnon "3391"

        CollectionAssert.AreEqual (
            [| struct ("one", "uno") |],
            FrozenMap.toArray fromArray,
            "FrozenMap.ofReadOnlySpan must take an array when the types of the call are known"
        )

        CollectionAssert.AreEqual (
            [| struct ("one", "uno") |],
            FrozenMap.toArray fromSpan,
            "FrozenMap.ofReadOnlySpan must take a Span when the types of the call are known"
        )

    [<TestMethod>]
    member _.``ofSpan takes a Span whose type arguments the call infers`` () =
        // ofReadOnlySpan converts a Span only where the type of the parameter is known, so ofSpan takes the Span itself
        let table = FrozenMap.ofSpan (Span [| KeyValuePair ("one", "uno") |])

        CollectionAssert.AreEqual (
            [| struct ("one", "uno") |],
            FrozenMap.toArray table,
            "FrozenMap.ofSpan must create the bindings of a Span without a type annotation"
        )

    // The destination has a free slot after the last binding, and the first binding goes to the index
    [<TestMethod; DynamicData(nameof FrozenMapTests.CopyToInputsAndIndexes)>]
    member _.``copyTo copies the bindings in the order of enumeration from the index``
        (input : struct (int * string) array, index : int)
        =
        let table = FrozenMap.ofSeq input
        let marker = KeyValuePair (-1, "marker")
        let destination = Array.create (input.Length + index + 1) marker
        FrozenMap.copyTo destination index table

        CollectionAssert.AreEqual (
            Array.concat [|
                Array.create index marker
                FrozenMap.toArray table
                |> Array.map (fun struct (key, value) -> KeyValuePair (key, value))
                [| marker |]
            |],
            destination,
            "FrozenMap.copyTo must fill the destination from the index in the order of enumeration, and leave the other elements"
        )

    [<TestMethod>]
    member _.``copyTo of an empty map at the end of the destination changes nothing`` () =
        let destination = [| KeyValuePair (1, "one") |]
        FrozenMap.copyTo destination 1 FrozenMap.empty

        CollectionAssert.AreEqual (
            [| KeyValuePair (1, "one") |],
            destination,
            "FrozenMap.copyTo must leave the destination as it is for a map without bindings"
        )

    // The destination of three elements has no index -1 and no index 4
    [<TestMethod; DataRow(3, -1); DataRow(3, 4)>]
    member _.``copyTo rejects an index outside the destination`` (length : int, index : int) =
        let table = FrozenMap.ofArray [| struct (1, "one"); struct (2, "two") |]
        let destination = Array.zeroCreate<KeyValuePair<int, string>> length

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            Action (fun () -> FrozenMap.copyTo destination index table),
            "FrozenMap.copyTo must reject an index outside the destination"
        )
        |> ignore

    // Two bindings do not fit after index 2 of three elements, at index 3, in one element or in none
    [<TestMethod; DataRow(3, 2); DataRow(3, 3); DataRow(1, 0); DataRow(0, 0)>]
    member _.``copyTo rejects a destination without room for the bindings`` (length : int, index : int) =
        let table = FrozenMap.ofArray [| struct (1, "one"); struct (2, "two") |]
        let destination = Array.zeroCreate<KeyValuePair<int, string>> length

        let ex =
            Assert.ThrowsExactly<ArgumentException>(
                Action (fun () -> FrozenMap.copyTo destination index table),
                "FrozenMap.copyTo must reject a destination without room for the bindings"
            )

        Assert.AreEqual<string | null>("destination", ex.ParamName, "FrozenMap.copyTo must name the destination parameter")

    [<TestMethod>]
    member _.``copyTo rejects a null destination`` () =
        let table = FrozenMap.ofArray [| struct (1, "one") |]

        let ex =
            Assert.ThrowsExactly<ArgumentNullException>(
                Action (fun () -> FrozenMap.copyTo (Unchecked.defaultof<KeyValuePair<int, string> array>) 0 table),
                "FrozenMap.copyTo must reject a null destination"
            )

        Assert.AreEqual<string | null>("destination", ex.ParamName, "FrozenMap.copyTo must name the destination parameter")

    [<TestMethod; DataRow(3, -1); DataRow(3, 4); DataRow(3, 2); DataRow(3, 3); DataRow(1, 0); DataRow(0, 0)>]
    member _.``copyTo throws what the BCL method throws`` (length : int, index : int) =
        let table = FrozenMap.ofArray [| struct (1, "one"); struct (2, "two") |]
        let destination = Array.zeroCreate<KeyValuePair<int, string>> length

        ExceptionAssert.Same (
            id,
            (fun () -> table.CopyTo (destination, index)),
            (fun () -> FrozenMap.copyTo destination index table),
            "FrozenMap.copyTo must throw what FrozenDictionary.CopyTo throws"
        )

#if NET10_0_OR_GREATER
/// The alternate lookups of `FrozenMap`, which exist on `net10.0` only (ADR 0001, D11).
[<TestClass; FrozenMapTestCategory>]
type FrozenMapAlternateLookupTests () =

    /// Compares string keys and looks them up by arrays of characters, which a frozen map of strings does not support.
    static let stringByChars = {
        new IEqualityComparer<string> with
            member _.Equals (left, right) = String.Equals (left, right, StringComparison.Ordinal)
            member _.GetHashCode key = StringComparer.Ordinal.GetHashCode key
        interface IAlternateEqualityComparer<char array, string> with
            member _.Equals (alternate : char array, key : string) =
                String.Equals (String alternate, key, StringComparison.Ordinal)

            member _.GetHashCode (alternate : char array) = StringComparer.Ordinal.GetHashCode (String alternate)
            member _.Create (alternate : char array) = String alternate
    }

    /// A map of `int64` keys, from 1 to the size, whose comparer looks them up by `int` alternates.
    static let alternateTable (size : int) =
        (FrozenMap.forKeyComparer FrozenMapOps.int64ByInt32).ofArray [| for key in 1L .. int64 size -> struct (key, string key) |]

    /// The alternate lookups on a null map, by name.
    static let nullTableCalls : Map<string, unit -> unit> =
        let nullTable = Unchecked.defaultof<FrozenMap<int64, string>>

        Map.ofArray [|
            nameof FrozenMap.alternateLookup, (fun () -> FrozenMap.alternateLookup<int, _, _> nullTable |> ignore)
            nameof FrozenMap.tryAlternateLookup, (fun () -> FrozenMap.tryAlternateLookup<int, _, _> nullTable |> ignore)
        |]

    /// The names of the alternate lookups, as the rows of the test of a null map.
    static member NullTableCalls = nullTableCalls.Keys

    // The maps are empty, small and large
    [<TestMethod; DataRow 0; DataRow 3; DataRow 30>]
    member _.``alternateLookup finds the bindings by an alternate key`` (size : int) =
        let table = alternateTable size
        let lookup = FrozenMap.alternateLookup<int, _, _> table
        Assert.AreSame (table, lookup.Dictionary, "FrozenMap.alternateLookup must look the bindings of the map up")

        for key in 1..size do
            Assert.IsTrue (lookup.ContainsKey key, $"FrozenMap.alternateLookup must find %d{key}")
            Assert.AreEqual<string>(string key, lookup[key], $"FrozenMap.alternateLookup must return the value of %d{key}")

        Assert.IsFalse (lookup.ContainsKey (size + 1), $"FrozenMap.alternateLookup must not find %d{size + 1}")

    [<TestMethod>]
    member _.``alternateLookup throws when the comparer has no alternate key type`` () =
        let table =
            (FrozenMap.forKeyComparer HashIdentity.Structural).ofArray [| struct (1L, "one"); struct (2L, "two") |]

        Assert.ThrowsExactly<InvalidOperationException>(
            Action (fun () -> FrozenMap.alternateLookup<int, _, _> table |> ignore),
            "FrozenMap.alternateLookup must reject a comparer without the alternate key type, as GetAlternateLookup does"
        )
        |> ignore

    // The maps are empty, small and large
    [<TestMethod; DataRow 0; DataRow 3; DataRow 30>]
    member _.``tryAlternateLookup returns the lookup`` (size : int) =
        let table = alternateTable size

        match FrozenMap.tryAlternateLookup<int, _, _> table with
        | ValueSome lookup ->
            Assert.AreSame (table, lookup.Dictionary, "FrozenMap.tryAlternateLookup must look the bindings of the map up")

            Assert.AreEqual<bool>(
                size > 0,
                lookup.ContainsKey 1,
                "FrozenMap.tryAlternateLookup must find 1 exactly when the map has a binding"
            )
        | ValueNone -> Assert.Fail "FrozenMap.tryAlternateLookup must return a lookup for a comparer with the alternate key type"

    [<TestMethod>]
    member _.``tryAlternateLookup returns nothing for a comparer without the alternate key type`` () =
        let structural =
            (FrozenMap.forKeyComparer HashIdentity.Structural).ofArray [| struct (1L, "one"); struct (2L, "two") |]

        Assert.IsTrue (
            (FrozenMap.tryAlternateLookup<int, _, _> structural).IsNone,
            "FrozenMap.tryAlternateLookup must return nothing for HashIdentity.Structural, which has no alternate key type"
        )

    [<TestMethod>]
    member _.``the alternate lookups of string keys take spans only`` () =
        let table = (FrozenMap.forKeyComparer stringByChars).ofArray [| struct ("one", 1); struct ("two", 2) |]

        Assert.IsTrue (
            (FrozenMap.tryAlternateLookup<char array, _, _> table).IsNone,
            "FrozenMap.tryAlternateLookup must return nothing for string keys and an alternate key that is not a span"
        )

        Assert.ThrowsExactly<InvalidOperationException>(
            Action (fun () -> FrozenMap.alternateLookup<char array, _, _> table |> ignore),
            "FrozenMap.alternateLookup must reject string keys and an alternate key that is not a span"
        )
        |> ignore

    [<TestMethod>]
    member _.``a span alternate key takes the method of the frozen map`` () =
        // The functions of the module cannot take a span, so their documentation points to the method of the map (D11)
        let table =
            (FrozenMap.forKeyComparer StringComparer.Ordinal).ofArray [| struct ("one", 1); struct ("two", 2) |]

        let lookup = table.GetAlternateLookup<ReadOnlySpan<char>>()
        Assert.IsTrue (lookup.ContainsKey ("two".AsSpan()), "GetAlternateLookup must find a key by a span of its characters")
        Assert.AreEqual<int>(2, lookup["two".AsSpan()], "GetAlternateLookup must return the value of a span key")
        Assert.IsFalse (lookup.ContainsKey ("three".AsSpan()), "GetAlternateLookup must not find a missing span key")

    [<TestMethod; DynamicData(nameof FrozenMapAlternateLookupTests.NullTableCalls)>]
    member _.``the alternate lookups reject a null map`` (name : string) =
        let ex =
            Assert.ThrowsExactly<ArgumentNullException>(Action nullTableCalls[name], $"FrozenMap.%s{name} must reject a null map")

        Assert.AreEqual<string | null>("table", ex.ParamName, $"FrozenMap.%s{name} must name the map parameter")
#endif
