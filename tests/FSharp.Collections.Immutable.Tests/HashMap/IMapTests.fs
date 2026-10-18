namespace FSharp.Collections.Immutable.Tests

open System
open System.Collections.Generic
open System.Collections.Immutable
open FSharp.Collections.Immutable
open Microsoft.VisualStudio.TestTools.UnitTesting

/// <summary>
/// The implementations of the maps that the tests of <c>IMap</c> see through the interface, as the cases of their rows.
/// </summary>
[<RequireQualifiedAccess>]
type MapImplementation =
    /// An <see cref="T:System.Collections.Immutable.ImmutableDictionary`2"/>.
    | Hash
    /// An <see cref="T:System.Collections.Immutable.ImmutableSortedDictionary`2"/>.
    | Sorted

    /// Every implementation, as the rows of the tests that run once per implementation.
    static member All = seq {
        MapImplementation.Hash
        MapImplementation.Sorted
    }

/// The behaviour of `IMap` that the shared contracts do not cover: null inputs, the implementation and the comparers of the
/// results, and the functions that wrap the interface.
[<TestClass; IMapTestCategory>]
type IMapTests () =

    static let nullTable = Unchecked.defaultof<IMap<string, string>>

    static let bindings = [| KeyValuePair ("one", "uno"); KeyValuePair ("Two", "dos"); KeyValuePair ("three", "tres") |]

    /// A map of the given implementation whose keys and values ignore the case, seen through the interface.
    static let tableOf (implementation : MapImplementation) : IMap<string, string> =
        match implementation with
        | MapImplementation.Hash ->
            (HashMap.forComparers StringComparer.OrdinalIgnoreCase StringComparer.OrdinalIgnoreCase).ofArray bindings
        | MapImplementation.Sorted ->
            ImmutableSortedDictionary.CreateRange (StringComparer.OrdinalIgnoreCase, StringComparer.OrdinalIgnoreCase, bindings)

    /// The key comparer of the immutable dictionaries; the interface does not expose it.
    static let keyComparerOf (table : IMap<string, 'Value>) : obj | null =
        match table with
        | :? ImmutableDictionary<string, 'Value> as hash -> hash.KeyComparer
        | :? ImmutableSortedDictionary<string, 'Value> as sorted -> sorted.KeyComparer
        | _ -> null

    /// The functions that take a map, by name, on a null map. The rows of the tests are the names: a function has no
    /// readable name in a row.
    static let nullTableCalls : Map<string, unit -> unit> =
        Map.ofArray [|
            nameof IMap.add, (fun () -> IMap.add "a" "b" nullTable |> ignore)
            nameof IMap.addRange,
            (fun () ->
                IMap.addRange [| KeyValuePair ("a", "b") |] nullTable
                |> ignore
            )
            nameof IMap.change, (fun () -> IMap.change "a" id nullTable |> ignore)
            nameof IMap.choose,
            (fun () ->
                IMap.choose (fun _ value -> ValueSome value) nullTable
                |> ignore
            )
            nameof IMap.clear, (fun () -> IMap.clear nullTable |> ignore)
            nameof IMap.containsBinding, (fun () -> IMap.containsBinding "a" "b" nullTable |> ignore)
            nameof IMap.containsKey, (fun () -> IMap.containsKey "a" nullTable |> ignore)
            nameof IMap.count, (fun () -> IMap.count nullTable |> ignore)
            nameof IMap.exists, (fun () -> IMap.exists (fun _ _ -> true) nullTable |> ignore)
            nameof IMap.filter, (fun () -> IMap.filter (fun _ _ -> true) nullTable |> ignore)
            nameof IMap.find, (fun () -> IMap.find "a" nullTable |> ignore)
            nameof IMap.findKey, (fun () -> IMap.findKey (fun _ _ -> true) nullTable |> ignore)
            nameof IMap.fold, (fun () -> IMap.fold (fun state _ _ -> state) 0 nullTable |> ignore)
            nameof IMap.foldBack, (fun () -> IMap.foldBack (fun _ _ state -> state) nullTable 0 |> ignore)
            nameof IMap.forall, (fun () -> IMap.forall (fun _ _ -> true) nullTable |> ignore)
            nameof IMap.isEmpty, (fun () -> IMap.isEmpty nullTable |> ignore)
            nameof IMap.iter, (fun () -> IMap.iter (fun _ _ -> ()) nullTable)
            nameof IMap.keys, (fun () -> IMap.keys nullTable |> ignore)
            nameof IMap.map, (fun () -> IMap.map (fun _ value -> value) nullTable |> ignore)
            nameof IMap.partition, (fun () -> IMap.partition (fun _ _ -> true) nullTable |> ignore)
            nameof IMap.pick,
            (fun () ->
                IMap.pick (fun _ value -> ValueSome value) nullTable
                |> ignore
            )
            nameof IMap.remove, (fun () -> IMap.remove "a" nullTable |> ignore)
            nameof IMap.removeRange, (fun () -> IMap.removeRange [| "a" |] nullTable |> ignore)
            nameof IMap.setItem, (fun () -> IMap.setItem "a" "b" nullTable |> ignore)
            nameof IMap.setItems,
            (fun () ->
                IMap.setItems [| KeyValuePair ("a", "b") |] nullTable
                |> ignore
            )
            nameof IMap.toArray, (fun () -> IMap.toArray nullTable |> ignore)
            nameof IMap.toList, (fun () -> IMap.toList nullTable |> ignore)
            nameof IMap.toSeq, (fun () -> IMap.toSeq nullTable |> ignore)
            nameof IMap.tryFind, (fun () -> IMap.tryFind "a" nullTable |> ignore)
            nameof IMap.tryFindKey, (fun () -> IMap.tryFindKey (fun _ _ -> true) nullTable |> ignore)
            nameof IMap.tryGetKey, (fun () -> IMap.tryGetKey "a" nullTable |> ignore)
            nameof IMap.tryPick,
            (fun () ->
                IMap.tryPick (fun _ value -> ValueSome value) nullTable
                |> ignore
            )
            nameof IMap.values, (fun () -> IMap.values nullTable |> ignore)
            "Tuple.addRange", (fun () -> IMap.Tuple.addRange [| "a", "b" |] nullTable |> ignore)
            "Tuple.setItems", (fun () -> IMap.Tuple.setItems [| "a", "b" |] nullTable |> ignore)
            "ValueTuple.addRange",
            (fun () ->
                IMap.ValueTuple.addRange [| struct ("a", "b") |] nullTable
                |> ignore
            )
            "ValueTuple.setItems",
            (fun () ->
                IMap.ValueTuple.setItems [| struct ("a", "b") |] nullTable
                |> ignore
            )
        |]

    /// The functions that take a sequence, by name, with the name of the sequence parameter and the function on a null
    /// sequence.
    static let nullSequenceCalls : Map<string, struct (string * (unit -> unit))> =
        let table = tableOf MapImplementation.Hash

        Map.ofArray [|
            nameof IMap.addRange,
            struct ("pairs",
                    fun () ->
                        IMap.addRange (Unchecked.defaultof<seq<KeyValuePair<string, string>>>) table
                        |> ignore)
            nameof IMap.removeRange,
            struct ("keys",
                    fun () ->
                        IMap.removeRange (Unchecked.defaultof<seq<string>>) table
                        |> ignore)
            nameof IMap.setItems,
            struct ("pairs",
                    fun () ->
                        IMap.setItems (Unchecked.defaultof<seq<KeyValuePair<string, string>>>) table
                        |> ignore)
            "Tuple.addRange",
            struct ("pairs",
                    fun () ->
                        IMap.Tuple.addRange (Unchecked.defaultof<seq<string * string>>) table
                        |> ignore)
            "Tuple.setItems",
            struct ("pairs",
                    fun () ->
                        IMap.Tuple.setItems (Unchecked.defaultof<seq<string * string>>) table
                        |> ignore)
            "ValueTuple.addRange",
            struct ("pairs",
                    fun () ->
                        IMap.ValueTuple.addRange (Unchecked.defaultof<seq<struct (string * string)>>) table
                        |> ignore)
            "ValueTuple.setItems",
            struct ("pairs",
                    fun () ->
                        IMap.ValueTuple.setItems (Unchecked.defaultof<seq<struct (string * string)>>) table
                        |> ignore)
        |]

    /// The functions that take a sequence, by name, with the name of the sequence parameter and the function on a default
    /// flat list that is given as the sequence. A default flat list converts to a sequence without a check and fails with
    /// InvalidOperationException when the sequence is enumerated, so the other inputs are valid and the function would
    /// enumerate it. The last row gives a sequence of objects, which a default flat list of strings is by the covariance of
    /// seq: the element type of the sequence is not the one of the flat list.
    static let defaultSequenceCalls : Map<string, struct (string * (unit -> unit))> =
        let table = tableOf MapImplementation.Hash
        let objectTable = HashMap.ofArray [| KeyValuePair<obj, string>("one", "uno") |] :> IMap<obj, string>
        let defaultPairs = Unchecked.defaultof<FlatList<KeyValuePair<string, string>>> :> seq<_>
        let defaultTuples = Unchecked.defaultof<FlatList<string * string>> :> seq<_>
        let defaultStructTuples = Unchecked.defaultof<FlatList<struct (string * string)>> :> seq<_>
        let defaultKeys = Unchecked.defaultof<FlatList<string>> :> seq<_>

        Map.ofArray [|
            nameof IMap.addRange, struct ("pairs", fun () -> IMap.addRange defaultPairs table |> ignore)
            nameof IMap.removeRange, struct ("keys", fun () -> IMap.removeRange defaultKeys table |> ignore)
            nameof IMap.setItems, struct ("pairs", fun () -> IMap.setItems defaultPairs table |> ignore)
            "Tuple.addRange", struct ("pairs", fun () -> IMap.Tuple.addRange defaultTuples table |> ignore)
            "Tuple.setItems", struct ("pairs", fun () -> IMap.Tuple.setItems defaultTuples table |> ignore)
            "ValueTuple.addRange", struct ("pairs", fun () -> IMap.ValueTuple.addRange defaultStructTuples table |> ignore)
            "ValueTuple.setItems", struct ("pairs", fun () -> IMap.ValueTuple.setItems defaultStructTuples table |> ignore)
            $"%s{nameof IMap.removeRange} (default flat list of strings as objects)",
            struct ("keys",
                    fun () ->
                        IMap.removeRange FlatListOracles.defaultStringsAsObjects objectTable
                        |> ignore)
        |]

    /// The transformations that keep the type of the values, by name.
    static let sameTypeTransformations : Map<string, IMap<string, string> -> IMap<string, string>> =
        Map.ofArray [|
            nameof IMap.add, IMap.add "four" "cuatro"
            nameof IMap.addRange, IMap.addRange [| KeyValuePair ("four", "cuatro") |]
            nameof IMap.change, IMap.change "one" (fun _ -> ValueNone)
            nameof IMap.clear, IMap.clear
            nameof IMap.filter, IMap.filter (fun (key : string) _ -> key.Length > 3)
            ($"%s{nameof IMap.partition} (accepted)",
             fun table ->
                 let struct (accepted, _) = IMap.partition (fun (key : string) _ -> key.Length > 3) table
                 accepted)
            ($"%s{nameof IMap.partition} (rejected)",
             fun table ->
                 let struct (_, rejected) = IMap.partition (fun (key : string) _ -> key.Length > 3) table
                 rejected)
            nameof IMap.remove, IMap.remove "one"
            nameof IMap.removeRange, IMap.removeRange [| "one"; "two" |]
            nameof IMap.setItem, IMap.setItem "four" "cuatro"
            nameof IMap.setItems, IMap.setItems [| KeyValuePair ("four", "cuatro") |]
            "Tuple.addRange", IMap.Tuple.addRange [| "four", "cuatro" |]
            "Tuple.setItems", IMap.Tuple.setItems [| "four", "cuatro" |]
            "ValueTuple.addRange", IMap.ValueTuple.addRange [| struct ("four", "cuatro") |]
            "ValueTuple.setItems", IMap.ValueTuple.setItems [| struct ("four", "cuatro") |]
        |]

    /// The transformations that change the type of the values, by name.
    static let typeChangingTransformations : Map<string, IMap<string, string> -> HashMap<string, int>> =
        Map.ofArray [|
            nameof IMap.choose, IMap.choose (fun _ (value : string) -> ValueSome value.Length)
            nameof IMap.map, IMap.map (fun _ (value : string) -> value.Length)
        |]

    /// The functions that bind "ONE" to "UNO" in a map whose keys and values ignore the case, by name: the map binds "one"
    /// to "uno", a value that the value comparer finds equal to "UNO".
    static let equalValueCalls : Map<string, IMap<string, string> -> IMap<string, string>> =
        Map.ofArray [|
            nameof IMap.add, IMap.add "ONE" "UNO"
            nameof IMap.addRange, IMap.addRange [| KeyValuePair ("ONE", "UNO") |]
            nameof IMap.change, IMap.change "ONE" (fun _ -> ValueSome "UNO")
            nameof IMap.setItem, IMap.setItem "ONE" "UNO"
            nameof IMap.setItems, IMap.setItems [| KeyValuePair ("ONE", "UNO") |]
            "Tuple.addRange", IMap.Tuple.addRange [| "ONE", "UNO" |]
            "Tuple.setItems", IMap.Tuple.setItems [| "ONE", "UNO" |]
            "ValueTuple.addRange", IMap.ValueTuple.addRange [| struct ("ONE", "UNO") |]
            "ValueTuple.setItems", IMap.ValueTuple.setItems [| struct ("ONE", "UNO") |]
        |]

    /// The transformations that change the type of the values and give every binding the same result, by name.
    static let constantTransformations : Map<string, IMap<string, int> -> HashMap<string, int>> =
        Map.ofArray [|
            nameof IMap.choose, IMap.choose (fun _ _ -> ValueSome 0)
            nameof IMap.map, IMap.map (fun _ _ -> 0)
        |]

    /// The names of the functions that take a map, as the rows of their test.
    static member NullTableCalls = nullTableCalls.Keys

    /// The names of the functions that take a sequence, as the rows of their test.
    static member NullSequenceCalls = nullSequenceCalls.Keys

    /// The names of the functions that take a sequence and are given a default flat list, as the rows of their test.
    static member DefaultSequenceCalls = defaultSequenceCalls.Keys

    /// Every implementation with each transformation that keeps the type of the values.
    static member ImplementationsAndSameTypeTransformations = seq {
        for implementation in MapImplementation.All do
            for name in sameTypeTransformations.Keys do
                struct (implementation, name)
    }

    /// Every implementation with each transformation that changes the type of the values.
    static member ImplementationsAndTypeChangingTransformations = seq {
        for implementation in MapImplementation.All do
            for name in typeChangingTransformations.Keys do
                struct (implementation, name)
    }

    /// Every implementation with each function that sets a value that the value comparer finds equal to the current one.
    static member ImplementationsAndEqualValueCalls = seq {
        for implementation in MapImplementation.All do
            for name in equalValueCalls.Keys do
                struct (implementation, name)
    }

    /// The names of the transformations that give every binding the same result, as the rows of their test.
    static member ConstantTransformations = constantTransformations.Keys

    [<TestMethod; DynamicData(nameof IMapTests.NullTableCalls)>]
    member _.``functions reject a null map`` (name : string) =
        let ex =
            Assert.ThrowsExactly<ArgumentNullException>(Action nullTableCalls[name], $"IMap.%s{name} must reject a null map")
        Assert.AreEqual<string | null>("table", ex.ParamName, $"IMap.%s{name} must name the map parameter")

    [<TestMethod; DynamicData(nameof IMapTests.NullSequenceCalls)>]
    member _.``functions reject a null sequence`` (name : string) =
        let struct (paramName, call) = nullSequenceCalls[name]
        let ex =
            Assert.ThrowsExactly<ArgumentNullException>(Action call, $"IMap.%s{name} must reject a null sequence")
        Assert.AreEqual<string | null>(paramName, ex.ParamName, $"IMap.%s{name} must name the sequence parameter")

    [<TestMethod; DynamicData(nameof IMapTests.DefaultSequenceCalls)>]
    member _.``functions reject a default flat list given as a sequence`` (name : string) =
        // The exception names the parameter of the function, not the one of the BCL member that it would have called
        let struct (paramName, call) = defaultSequenceCalls[name]
        FlatListAssert.RejectsNull ($"IMap.%s{name}", paramName, call)

    [<TestMethod; DynamicData(nameof MapImplementation.All, typeof<MapImplementation>)>]
    member _.``a flat list that is not default passes as a sequence, also one of strings as a sequence of objects``
        (implementation : MapImplementation)
        =
        let flatList (elements : 'T array) = FlatList.ofArray elements :> seq<'T>
        let table = tableOf implementation

        let bindingsOf (map : IMap<string, string>) =
            IMap.toArray map
            |> Array.sortBy (fun struct (key, _) -> key)

        CollectionAssert.AreEqual (
            [|
                struct ("Two", "dos")
                struct ("four", "cuatro")
                struct ("one", "uno")
                struct ("three", "tres")
            |],
            bindingsOf (IMap.addRange (flatList [| KeyValuePair ("four", "cuatro") |]) table),
            "IMap.addRange must take the elements of a flat list"
        )

        CollectionAssert.AreEqual (
            [|
                struct ("Two", "dos")
                struct ("one", "uno")
                struct ("three", "tres")
                struct ("zero", "cero")
            |],
            bindingsOf (IMap.setItems (flatList [| KeyValuePair ("zero", "cero") |]) table),
            "IMap.setItems must take the elements of a flat list"
        )

        CollectionAssert.AreEqual (
            [| struct ("Two", "dos"); struct ("three", "tres") |],
            bindingsOf (IMap.removeRange (flatList [| "one" |]) table),
            "IMap.removeRange must take the elements of a flat list"
        )

        // A flat list of strings is a sequence of objects, which a type test for the flat list of objects does not see
        let widened = FlatListOracles.stringsAsObjects (FlatList.ofArray [| "one" |])
        let objectTable =
            HashMap.ofArray [| KeyValuePair<obj, string>("one", "uno"); KeyValuePair<obj, string>("two", "dos") |]
            :> IMap<obj, string>

        CollectionAssert.AreEqual (
            [| "two" :> obj |],
            IMap.keys (IMap.removeRange widened objectTable)
            |> Seq.toArray,
            "IMap.removeRange must take the elements of a flat list of strings as objects"
        )

    [<TestMethod; DynamicData(nameof IMapTests.ImplementationsAndSameTypeTransformations)>]
    member _.``the transformations that keep the type of the values keep the implementation and the comparers``
        (implementation : MapImplementation, name : string)
        =
        let table = tableOf implementation
        let result = sameTypeTransformations[name] table
        Assert.AreEqual<Type>(table.GetType (), result.GetType (), $"IMap.%s{name} must keep the implementation of the map")
        Assert.AreSame<obj>(StringComparer.OrdinalIgnoreCase, keyComparerOf result, $"IMap.%s{name} must keep the key comparer")

    [<TestMethod; DynamicData(nameof IMapTests.ImplementationsAndTypeChangingTransformations)>]
    member _.``map and choose return hash maps with the default comparers`` (implementation : MapImplementation, name : string) =
        let result = typeChangingTransformations[name](tableOf implementation)

        Assert.AreSame<IEqualityComparer<string>>(
            EqualityComparer<string>.Default,
            HashMap.keyComparer result,
            $"IMap.%s{name} must compare the keys of its hash map with the default comparer"
        )

        Assert.AreSame<IEqualityComparer<int>>(
            EqualityComparer<int>.Default,
            HashMap.valueComparer result,
            $"IMap.%s{name} must compare the values of its hash map with the default comparer"
        )

        Assert.AreEqual<int>(3, HashMap.find "Two" result, $"IMap.%s{name} must keep the keys of the map")

    [<TestMethod>]
    member _.``map keeps the last value of keys that only the default comparer considers equal`` () =
        let first = String ([| 'a'; 'b' |])
        let second = String ([| 'a'; 'b' |])

        let table : IMap<string, int> =
            (HashMap.forKeyComparer HashIdentity.Reference).ofArray [| struct (first, 1); struct (second, 2) |]

        Assert.AreEqual<int>(2, IMap.count table, "The keys must be distinct in the reference comparer of the map")
        let struct (_, lastValue) = IMap.toArray table |> Array.last
        let mapped = IMap.map (fun _ value -> value * 10) table
        Assert.AreEqual<int>(1, HashMap.count mapped, "IMap.map must merge the keys that the default comparer considers equal")
        Assert.AreEqual<int>(
            lastValue * 10,
            HashMap.find "ab" mapped,
            "IMap.map must keep the last value in the order of enumeration"
        )

    [<TestMethod; DynamicData(nameof IMapTests.ConstantTransformations)>]
    member _.``map and choose keep the first of the keys that only the default comparer considers equal for an equal result``
        (name : string)
        =
        // The builder of the result keeps the first binding of a key for an equal value (ADR 0001, D14)
        let table : IMap<string, int> =
            (HashMap.forKeyComparer HashIdentity.Reference).ofArray [|
                struct (MapInputs.Copy "ab", 1)
                struct (MapInputs.Copy "ab", 2)
            |]

        let struct (firstKey, _) = IMap.toArray table |> Array.head

        let struct (key, _) =
            Assert.ContainsSingle (
                HashMap.toArray (constantTransformations[name] table),
                $"IMap.%s{name} must merge the equal keys"
            )

        Assert.AreSame (firstKey, key, $"IMap.%s{name} must keep the first of the equal keys in the order of enumeration")

    [<TestMethod; DynamicData(nameof MapImplementation.All, typeof<MapImplementation>)>]
    member _.``filter and partition return the input itself when every binding stays`` (implementation : MapImplementation) =
        let table = tableOf implementation
        Assert.AreSame (
            table,
            IMap.filter (fun _ _ -> true) table,
            "IMap.filter must return the input itself when every binding stays"
        )
        let struct (accepted, rejected) = IMap.partition (fun _ _ -> true) table

        Assert.AreSame (table, accepted, "IMap.partition must return the input itself when every binding satisfies the predicate")

        Assert.IsTrue (IMap.isEmpty rejected, "IMap.partition must return an empty map of the other bindings")

    [<TestMethod; DynamicData(nameof IMapTests.ImplementationsAndEqualValueCalls)>]
    member _.``a value that the value comparer finds equal leaves the binding as it is``
        (implementation : MapImplementation, name : string)
        =
        // The immutable dictionaries keep the binding, its key included, for an equal value (ADR 0001, D14)
        let table = tableOf implementation
        let result = equalValueCalls[name] table
        Assert.AreEqual<string>("uno", IMap.find "one" result, $"IMap.%s{name} must keep the current value")
        Assert.AreEqual<string voption>(ValueSome "one", IMap.tryGetKey "ONE" result, $"IMap.%s{name} must keep the current key")
        Assert.AreSame (table, result, $"IMap.%s{name} must return the input itself")

    [<TestMethod; DynamicData(nameof MapImplementation.All, typeof<MapImplementation>)>]
    member _.``tryGetKey returns the key that the map holds`` (implementation : MapImplementation) =
        let table = tableOf implementation
        Assert.AreEqual<string voption>(ValueSome "Two", IMap.tryGetKey "TWO" table, "IMap.tryGetKey must return the stored key")
        Assert.AreEqual<string voption>(
            ValueNone,
            IMap.tryGetKey "four" table,
            "IMap.tryGetKey must return nothing for a missing key"
        )

    [<TestMethod>]
    member _.``the functions follow the order of enumeration of the implementation`` () =
        let table = tableOf MapImplementation.Sorted
        let expectedKeys = [| "one"; "three"; "Two" |]
        CollectionAssert.AreEqual (expectedKeys, Seq.toArray (IMap.keys table), "IMap.keys must follow the order of a sorted map")

        CollectionAssert.AreEqual (
            [| "uno"; "tres"; "dos" |],
            Seq.toArray (IMap.values table),
            "IMap.values must follow the order of a sorted map"
        )

        CollectionAssert.AreEqual (
            expectedKeys,
            IMap.foldBack (fun key _ keys -> Array.append [| key |] keys) table [||],
            "IMap.foldBack must visit a sorted map from its last key"
        )

        Assert.AreEqual<string voption>(
            ValueSome "three",
            IMap.tryFindKey (fun (key : string) _ -> key.StartsWith ("t", StringComparison.OrdinalIgnoreCase)) table,
            "IMap.tryFindKey must return the first key of a sorted map that satisfies the predicate"
        )

    [<TestMethod; DynamicData(nameof MapImplementation.All, typeof<MapImplementation>)>]
    member _.``add throws what the implementation throws for a value that the value comparer finds different``
        (implementation : MapImplementation)
        =
        let table = tableOf implementation

        ExceptionAssert.Same (
            id,
            (fun () -> table.Add ("ONE", "dos") |> ignore),
            (fun () -> IMap.add "ONE" "dos" table |> ignore),
            "IMap.add must throw what IImmutableDictionary.Add throws"
        )

    [<TestMethod; DynamicData(nameof MapImplementation.All, typeof<MapImplementation>)>]
    member _.``setItem replaces the value of a key that add rejects`` (implementation : MapImplementation) =
        let table = tableOf implementation
        let replaced = IMap.setItem "ONE" "dos" table
        Assert.AreEqual<string>("dos", IMap.find "one" replaced, "IMap.setItem must replace the value of the key")
        Assert.AreEqual<string>("uno", IMap.find "one" table, "IMap.setItem must not change the input")

    [<TestMethod; DynamicData(nameof MapImplementation.All, typeof<MapImplementation>)>]
    member _.``containsBinding compares the value with the value comparer of the map where tryFind and equality do not``
        (implementation : MapImplementation)
        =
        // The keys and the values ignore the case: "one" is bound to "uno"
        let table = tableOf implementation

        Assert.IsTrue (
            IMap.containsBinding "ONE" "UNO" table,
            "IMap.containsBinding must find a value that the value comparer finds equal"
        )

        Assert.IsFalse (
            IMap.tryFind "ONE" table
            |> ValueOption.exists (fun stored -> stored = "UNO"),
            "F# equality finds \"uno\" and \"UNO\" different, which is why the function asks the map"
        )

        Assert.IsFalse (IMap.containsBinding "ONE" "DOS" table, "IMap.containsBinding must not find another value")
        Assert.IsFalse (IMap.containsBinding "four" "UNO" table, "IMap.containsBinding must not find a key without a binding")

    [<TestMethod>]
    member _.``containsBinding leaves a null key to the implementation`` () =
        let nullKey = Unchecked.defaultof<string>
        let defaultHash : IMap<string, string> = HashMap.ofArray [| "one", "uno" |]

        Assert.IsFalse (
            IMap.containsBinding nullKey "uno" defaultHash,
            "IMap.containsBinding must give false for a null key of a hash map with the default key comparer"
        )

        Assert.ThrowsExactly<ArgumentNullException>(
            Action (fun () ->
                IMap.containsBinding nullKey "uno" (tableOf MapImplementation.Hash)
                |> ignore
            ),
            "IMap.containsBinding must throw for a null key of a hash map whose key comparer rejects it"
        )
        |> ignore

        Assert.ThrowsExactly<ArgumentNullException>(
            Action (fun () ->
                IMap.containsBinding nullKey "uno" (tableOf MapImplementation.Sorted)
                |> ignore
            ),
            "IMap.containsBinding must throw for a null key of a sorted map, as ImmutableSortedDictionary.Contains does"
        )
        |> ignore

    [<TestMethod>]
    member _.``filter keeps the implementation, the comparers and the order of a SortedMap`` () =
        let valueComparer =
            HashIdentity.FromFunctions (fun (value : int) -> abs value) (fun left right -> abs left = abs right)

        // The keys are ordered by F# generic comparison, which puts the upper case letters before the lower case ones
        let table =
            (SortedMap.forComparers LanguagePrimitives.FastGenericComparer<string> valueComparer).ofArray [|
                struct ("b", 1)
                struct ("a", 2)
                struct ("B", 3)
                struct ("A", 4)
            |]
            :> IMap<string, int>

        match IMap.filter (fun _ value -> value > 1) table with
        | :? SortedMap<string, int> as filtered ->
            Assert.AreSame<IComparer<string>>(
                LanguagePrimitives.FastGenericComparer<string>,
                SortedMap.keyComparer filtered,
                "IMap.filter must keep the key comparer of a sorted map"
            )

            Assert.AreSame (
                valueComparer,
                SortedMap.valueComparer filtered,
                "IMap.filter must keep the value comparer of a sorted map"
            )

            CollectionAssert.AreEqual (
                [| "A"; "B"; "a" |],
                Seq.toArray (SortedMap.keys filtered),
                "IMap.filter must keep the order of the key comparer of a sorted map"
            )
        | filtered ->
            Assert.Fail $"IMap.filter must keep the implementation of a sorted map, not return %s{filtered.GetType().Name}"
