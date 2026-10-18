namespace FSharp.Collections.Immutable.Tests

open System
open System.Collections.Generic
open FSharp.Collections.Immutable
open Microsoft.VisualStudio.TestTools.UnitTesting

/// The comparer struct of `HashMap`, `CustomComparersHashMap`, and the functions of the module that create it (ADR 0001,
/// D12). The creation contract runs its members with a key comparer; these tests cover the comparers themselves.
[<TestClass; HashMapTestCategory>]
type CustomComparersHashMapTests () =

    /// A value comparer that differs from the default one: it compares strings by their length.
    static let byLength =
        HashIdentity.FromFunctions (fun (value : string) -> value.Length) (fun left right -> left.Length = right.Length)

    static let nullComparer = Unchecked.defaultof<IEqualityComparer<string>>

    /// The key comparer and the value comparer of a row of the tests of null comparers: the comparer whose parameter the
    /// row names is null.
    static let comparersWithNull (nullParameter : string) : struct (IEqualityComparer<string> * IEqualityComparer<string>) =
        match nullParameter with
        | "keyComparer" -> struct (nullComparer, byLength)
        | _ -> struct (StringComparer.Ordinal, nullComparer)

    /// The members of the struct that create a hash map, by name.
    static let creations : Map<string, CustomComparersHashMap<string, string> -> HashMap<string, string>> =
        Map.ofArray [|
            "empty", (fun comparers -> comparers.empty)
            "ofArray of key/value pairs", (fun comparers -> comparers.ofArray [| KeyValuePair ("a", "b") |])
            "ofArray of reference tuples", (fun comparers -> comparers.ofArray [| "a", "b" |])
            "ofArray of struct tuples", (fun comparers -> comparers.ofArray [| struct ("a", "b") |])
            "ofList of key/value pairs", (fun comparers -> comparers.ofList [ KeyValuePair ("a", "b") ])
            "ofList of reference tuples", (fun comparers -> comparers.ofList [ ("a", "b") ])
            "ofList of struct tuples", (fun comparers -> comparers.ofList [ struct ("a", "b") ])
            "ofReadOnlySpan", (fun comparers -> comparers.ofReadOnlySpan (ReadOnlySpan [| KeyValuePair ("a", "b") |]))
            "ofSeq of key/value pairs", (fun comparers -> comparers.ofSeq (dict [| "a", "b" |]))
            "ofSeq of reference tuples", (fun comparers -> comparers.ofSeq (Seq.singleton ("a", "b")))
            "ofSeq of struct tuples", (fun comparers -> comparers.ofSeq (Seq.singleton (struct ("a", "b"))))
            "ofSeqBy", (fun comparers -> comparers.ofSeqBy id [| "a" |])
            "ofSpan", (fun comparers -> comparers.ofSpan (Span [| KeyValuePair ("a", "b") |]))
            "singleton", (fun comparers -> comparers.singleton "a" "b")
            "build", (fun comparers -> comparers.build (fun builder -> builder["a"] <- "b"))
            "builder", (fun comparers -> comparers.builder().ToImmutable())
        |]

    /// The members of the struct that create a hash map from the bindings of "one" to "uno" and of "ONE" to "abc", by name:
    /// keys that ignore the case and values that compare by their length make the two bindings equal.
    static let equalValueCreations : Map<string, CustomComparersHashMap<string, string> -> HashMap<string, string>> =
        Map.ofArray [|
            "ofArray of key/value pairs",
            (fun comparers -> comparers.ofArray [| KeyValuePair ("one", "uno"); KeyValuePair ("ONE", "abc") |])
            "ofArray of reference tuples", (fun comparers -> comparers.ofArray [| "one", "uno"; "ONE", "abc" |])
            "ofArray of struct tuples", (fun comparers -> comparers.ofArray [| struct ("one", "uno"); struct ("ONE", "abc") |])
            "ofList of key/value pairs",
            (fun comparers -> comparers.ofList [ KeyValuePair ("one", "uno"); KeyValuePair ("ONE", "abc") ])
            "ofList of reference tuples", (fun comparers -> comparers.ofList [ "one", "uno"; "ONE", "abc" ])
            "ofList of struct tuples", (fun comparers -> comparers.ofList [ struct ("one", "uno"); struct ("ONE", "abc") ])
            "ofReadOnlySpan",
            (fun comparers ->
                comparers.ofReadOnlySpan (ReadOnlySpan [| KeyValuePair ("one", "uno"); KeyValuePair ("ONE", "abc") |])
            )
            "ofSeq of key/value pairs",
            (fun comparers -> comparers.ofSeq (Seq.ofArray [| KeyValuePair ("one", "uno"); KeyValuePair ("ONE", "abc") |]))
            "ofSeq of reference tuples", (fun comparers -> comparers.ofSeq (Seq.ofArray [| "one", "uno"; "ONE", "abc" |]))
            "ofSeq of struct tuples",
            (fun comparers -> comparers.ofSeq (Seq.ofArray [| struct ("one", "uno"); struct ("ONE", "abc") |]))
            "ofSeqBy",
            (fun comparers ->
                comparers.ofSeqBy
                    (fun value ->
                        if String.Equals (value, "uno", StringComparison.Ordinal) then
                            "one"
                        else
                            "ONE"
                    )
                    [| "uno"; "abc" |]
            )
            "ofSpan", (fun comparers -> comparers.ofSpan (Span [| KeyValuePair ("one", "uno"); KeyValuePair ("ONE", "abc") |]))
        |]

    /// The members of the struct that take a sequence, by name, with the name of the sequence parameter and the member on a
    /// null sequence. The struct has comparers that differ from the default ones.
    static let nullSequenceMembers : Map<string, struct (string * (unit -> unit))> =
        let comparers = HashMap.forComparers StringComparer.OrdinalIgnoreCase byLength

        Map.ofArray [|
            "ofSeq of key/value pairs",
            struct ("elements",
                    fun () ->
                        comparers.ofSeq (Unchecked.defaultof<seq<KeyValuePair<string, string>>>)
                        |> ignore)
            "ofSeq of reference tuples",
            struct ("elements",
                    fun () ->
                        comparers.ofSeq (Unchecked.defaultof<seq<string * string>>)
                        |> ignore)
            "ofSeq of struct tuples",
            struct ("elements",
                    fun () ->
                        comparers.ofSeq (Unchecked.defaultof<seq<struct (string * string)>>)
                        |> ignore)
            "ofSeqBy",
            struct ("source",
                    fun () ->
                        comparers.ofSeqBy id (Unchecked.defaultof<seq<string>>)
                        |> ignore)
        |]

    /// The members of the struct that take a sequence, by name, with the name of the sequence parameter and the member on a
    /// default flat list that is given as the sequence. A default flat list converts to a sequence without a check and fails
    /// with InvalidOperationException when the sequence is enumerated. The last row gives a sequence of objects, which a
    /// default flat list of strings is by the covariance of seq: the element type of the sequence is not the one of the
    /// flat list.
    static let defaultSequenceMembers : Map<string, struct (string * (unit -> unit))> =
        let comparers = HashMap.forComparers StringComparer.OrdinalIgnoreCase byLength
        let objectComparers : CustomComparersHashMap<obj, obj> =
            HashMap.forKeyComparer EqualityComparer<obj>.Default
        let defaultPairs = Unchecked.defaultof<FlatList<KeyValuePair<string, string>>> :> seq<_>
        let defaultTuples = Unchecked.defaultof<FlatList<string * string>> :> seq<_>
        let defaultStructTuples = Unchecked.defaultof<FlatList<struct (string * string)>> :> seq<_>
        let defaultValues = Unchecked.defaultof<FlatList<string>> :> seq<_>

        Map.ofArray [|
            "ofSeq of key/value pairs", struct ("elements", fun () -> comparers.ofSeq defaultPairs |> ignore)
            "ofSeq of reference tuples", struct ("elements", fun () -> comparers.ofSeq defaultTuples |> ignore)
            "ofSeq of struct tuples", struct ("elements", fun () -> comparers.ofSeq defaultStructTuples |> ignore)
            "ofSeqBy", struct ("source", fun () -> comparers.ofSeqBy id defaultValues |> ignore)
            "ofSeqBy (default flat list of strings as objects)",
            struct ("source",
                    fun () ->
                        objectComparers.ofSeqBy id FlatListOracles.defaultStringsAsObjects
                        |> ignore)
        |]

    /// The comparer structs of the two paths of the span members, by name: the default comparer of the values wraps
    /// CreateRangeWithOverwrite, and any other comparer, even one that behaves like the default one, sets the bindings in a
    /// builder.
    static let spanPaths : Map<string, CustomComparersHashMap<string, string>> =
        Map.ofArray [|
            "the default value comparer", HashMap.forKeyComparer StringComparer.OrdinalIgnoreCase
            "another value comparer", HashMap.forComparers StringComparer.OrdinalIgnoreCase StringComparer.Ordinal
        |]

    /// The bindings that a hash map with keys that ignore the case and values that compare ordinally holds after the given
    /// bindings are set in order, sorted by key (ADR 0001, D14): a binding replaces the binding of an equal key only when its
    /// value is different.
    static let expectedBindings (bindings : struct (string * string) array) =
        let expected = ResizeArray<struct (string * string)>()

        for struct (key, value) in bindings do
            let index =
                expected.FindIndex (fun struct (stored, _) -> String.Equals (stored, key, StringComparison.OrdinalIgnoreCase))

            if index < 0 then
                expected.Add (struct (key, value))
            else
                let struct (_, storedValue) = expected[index]

                if not (String.Equals (storedValue, value, StringComparison.Ordinal)) then
                    expected[index] <- struct (key, value)

        expected
        |> Seq.sortBy (fun struct (key, _) -> key)
        |> Seq.toArray

    /// The bindings of a hash map sorted by key.
    static let sortedBindings (table : HashMap<string, string>) =
        HashMap.toArray table
        |> Array.sortBy (fun struct (key, _) -> key)

    /// Every span member and every path with bindings whose keys differ in their case, with values that are equal or
    /// different: the rows of the test of the bindings.
    static member SpanCreationsPathsAndBindings = seq {
        for creation in SpanCreation.All do
            for path in spanPaths.Keys do
                for bindings in
                    [|
                        [||]
                        [| struct ("a", "x"); struct ("A", "x") |]
                        [| struct ("a", "x"); struct ("A", "y") |]
                        [| struct ("a", "x"); struct ("A", "y"); struct ("a", "y") |]
                        [| struct ("a", "x"); struct ("b", "y"); struct ("A", "z"); struct ("B", "y") |]
                    |] do
                    struct (creation, path, bindings)
    }

    /// Every span member with every path, as the rows of the tests that run once per member and path.
    static member SpanCreationsAndPaths = seq {
        for creation in SpanCreation.All do
            for path in spanPaths.Keys do
                struct (creation, path)
    }

    /// The names of the members that create a hash map, as the rows of the test of their comparers.
    static member Creations = creations.Keys

    /// The names of the members that create a hash map from bindings that the comparers find equal, as the rows of their
    /// test.
    static member EqualValueCreations = equalValueCreations.Keys

    /// The names of the members that take a sequence and are given a null one, as the rows of their test.
    static member NullSequenceMembers = nullSequenceMembers.Keys

    /// The names of the members that take a sequence and are given a default flat list, as the rows of their test.
    static member DefaultSequenceMembers = defaultSequenceMembers.Keys

    [<TestMethod; DataRow "keyComparer"; DataRow "valueComparer">]
    member _.``the constructor with both comparers rejects a null comparer`` (nullParameter : string) =
        let struct (keyComparer, valueComparer) = comparersWithNull nullParameter

        let ex =
            Assert.ThrowsExactly<ArgumentNullException>(
                Action (fun () ->
                    CustomComparersHashMap<string, string>(keyComparer, valueComparer)
                    |> ignore
                ),
                "The constructor must reject a null comparer"
            )

        Assert.AreEqual<string | null>(nullParameter, ex.ParamName, "The constructor must name the null comparer")

    [<TestMethod>]
    member _.``the constructor with the key comparer rejects a null comparer`` () =
        let ex =
            Assert.ThrowsExactly<ArgumentNullException>(
                Action (fun () ->
                    CustomComparersHashMap<string, string> nullComparer
                    |> ignore
                ),
                "The constructor must reject a null key comparer"
            )

        Assert.AreEqual<string | null>("keyComparer", ex.ParamName, "The constructor must name the key comparer")

    [<TestMethod; DataRow "keyComparer"; DataRow "valueComparer">]
    member _.``forComparers rejects a null comparer`` (nullParameter : string) =
        let struct (keyComparer, valueComparer) = comparersWithNull nullParameter

        let ex =
            Assert.ThrowsExactly<ArgumentNullException>(
                Action (fun () -> HashMap.forComparers keyComparer valueComparer |> ignore),
                "HashMap.forComparers must reject a null comparer"
            )

        Assert.AreEqual<string | null>(nullParameter, ex.ParamName, "HashMap.forComparers must name the null comparer")

    [<TestMethod>]
    member _.``forKeyComparer rejects a null comparer`` () =
        let ex =
            Assert.ThrowsExactly<ArgumentNullException>(
                Action (fun () ->
                    (HashMap.forKeyComparer nullComparer : CustomComparersHashMap<string, string>)
                    |> ignore
                ),
                "HashMap.forKeyComparer must reject a null key comparer"
            )

        Assert.AreEqual<string | null>("keyComparer", ex.ParamName, "HashMap.forKeyComparer must name the key comparer")

    [<TestMethod; DynamicData(nameof CustomComparersHashMapTests.NullSequenceMembers)>]
    member _.``members reject a null sequence`` (name : string) =
        let struct (paramName, call) = nullSequenceMembers[name]
        FlatListAssert.RejectsNull ($"CustomComparersHashMap.%s{name}", paramName, call)

    [<TestMethod; DynamicData(nameof CustomComparersHashMapTests.DefaultSequenceMembers)>]
    member _.``members reject a default flat list given as a sequence`` (name : string) =
        // The exception names the parameter of the member, not the one of the BCL member that it would have called
        let struct (paramName, call) = defaultSequenceMembers[name]
        FlatListAssert.RejectsNull ($"CustomComparersHashMap.%s{name}", paramName, call)

    [<TestMethod>]
    member _.``members take a flat list that is not default as a sequence, also one of strings as a sequence of objects`` () =
        let flatList (elements : 'T array) = FlatList.ofArray elements :> seq<'T>
        let comparers = HashMap.forKeyComparer StringComparer.OrdinalIgnoreCase

        let sortedBindings (table : HashMap<string, string>) =
            HashMap.toArray table
            |> Array.sortBy (fun struct (key, _) -> key)

        CollectionAssert.AreEqual (
            [| struct ("a", "b") |],
            sortedBindings (comparers.ofSeq (flatList [| KeyValuePair ("a", "b") |])),
            "ofSeq must take the key/value pairs of a flat list"
        )

        CollectionAssert.AreEqual (
            [| struct ("a", "b") |],
            sortedBindings (comparers.ofSeq (flatList [| ("a", "b") |])),
            "ofSeq must take the reference tuples of a flat list"
        )

        CollectionAssert.AreEqual (
            [| struct ("a", "b") |],
            sortedBindings (comparers.ofSeq (flatList [| struct ("a", "b") |])),
            "ofSeq must take the struct tuples of a flat list"
        )

        CollectionAssert.AreEqual (
            [| struct ("ab", "ab") |],
            sortedBindings (comparers.ofSeqBy id (flatList [| "ab" |])),
            "ofSeqBy must take the elements of a flat list"
        )

        // A flat list of strings is a sequence of objects, which a type test for the flat list of objects does not see
        let objectComparers : CustomComparersHashMap<obj, obj> =
            HashMap.forKeyComparer EqualityComparer<obj>.Default
        let widened = FlatListOracles.stringsAsObjects (FlatList.ofArray [| "ab" |])

        CollectionAssert.AreEqual (
            [| "ab" :> obj |],
            HashMap.keys (objectComparers.ofSeqBy id widened)
            |> Seq.toArray,
            "ofSeqBy must take the elements of a flat list of strings as objects"
        )

    [<TestMethod>]
    member _.``forComparers holds both comparers`` () =
        let comparers = HashMap.forComparers StringComparer.OrdinalIgnoreCase byLength

        Assert.AreSame<IEqualityComparer<string>>(
            StringComparer.OrdinalIgnoreCase,
            comparers.KeyComparer,
            "HashMap.forComparers must hold the key comparer"
        )

        Assert.AreSame (byLength, comparers.ValueComparer, "HashMap.forComparers must hold the value comparer")

    [<TestMethod>]
    member _.``forKeyComparer holds the key comparer and the default comparer of the values`` () =
        let comparers : CustomComparersHashMap<string, string> =
            HashMap.forKeyComparer StringComparer.OrdinalIgnoreCase

        Assert.AreSame<IEqualityComparer<string>>(
            StringComparer.OrdinalIgnoreCase,
            comparers.KeyComparer,
            "HashMap.forKeyComparer must hold the key comparer"
        )

        Assert.AreSame<IEqualityComparer<string>>(
            EqualityComparer<string>.Default,
            comparers.ValueComparer,
            "HashMap.forKeyComparer must hold the default comparer of the values"
        )

    [<TestMethod>]
    member _.``forKeyComparer StringComparer.OrdinalIgnoreCase applies the comparer`` () =
        let table =
            (HashMap.forKeyComparer StringComparer.OrdinalIgnoreCase).ofSeq [| "one", 1; "ONE", 2; "two", 3 |]

        Assert.AreSame<IEqualityComparer<string>>(
            StringComparer.OrdinalIgnoreCase,
            HashMap.keyComparer table,
            "The hash map of HashMap.forKeyComparer must have the key comparer"
        )

        Assert.AreEqual<int>(2, HashMap.count table, "The comparer must merge the keys that differ in their case")
        Assert.AreEqual<int>(2, HashMap.find "One" table, "The hash map must keep the last binding of keys that are equal")
        Assert.AreEqual<int>(3, HashMap.find "TWO" table, "The hash map must look the keys up with the comparer")

    [<TestMethod; DynamicData(nameof CustomComparersHashMapTests.Creations)>]
    member _.``every member creates hash maps with the comparers of the struct`` (creation : string) =
        let table = creations[creation](HashMap.forComparers StringComparer.OrdinalIgnoreCase byLength)

        Assert.AreSame<IEqualityComparer<string>>(
            StringComparer.OrdinalIgnoreCase,
            HashMap.keyComparer table,
            "The member must create a hash map with the key comparer of the struct"
        )

        Assert.AreSame (
            byLength,
            HashMap.valueComparer table,
            "The member must create a hash map with the value comparer of the struct"
        )

    [<TestMethod; DynamicData(nameof CustomComparersHashMapTests.EqualValueCreations)>]
    member _.``every member keeps the earlier binding of a key for a value that the value comparer finds equal``
        (creation : string)
        =
        // The indexer of the builder and ToImmutableDictionary keep the earlier binding, its key included (ADR 0001, D14)
        let table =
            equalValueCreations[creation](HashMap.forComparers StringComparer.OrdinalIgnoreCase byLength)

        let struct (key, value) =
            Assert.ContainsSingle (HashMap.toArray table, "The member must merge the bindings of the equal keys")

        Assert.AreEqual<string>("one", key, "The member must keep the earlier key")
        Assert.AreEqual<string>("uno", value, "The member must keep the earlier value")

    [<TestMethod; DynamicData(nameof CustomComparersHashMapTests.SpanCreationsPathsAndBindings)>]
    member _.``the span members replace the binding of a key only for a value that the value comparer finds different``
        (creation : SpanCreation, path : string, bindings : struct (string * string) array)
        =
        let table = creation.CreateWith (spanPaths[path], Array.map MapInputs.ToKeyValuePair bindings)

        CollectionAssert.AreEqual (
            expectedBindings bindings,
            sortedBindings table,
            $"%s{creation.Name} with %s{path} must replace the binding of a key only for a different value"
        )

        Assert.AreSame<IEqualityComparer<string>>(
            StringComparer.OrdinalIgnoreCase,
            HashMap.keyComparer table,
            $"%s{creation.Name} with %s{path} must create a hash map with the key comparer of the struct"
        )

    [<TestMethod; DynamicData(nameof CustomComparersHashMapTests.SpanCreationsAndPaths)>]
    member _.``the span members keep the first key and value of bindings that the comparers find equal``
        (creation : SpanCreation, path : string)
        =
        // The copies are equal in the comparers and tell the two bindings apart (ADR 0001, D14)
        let firstKey = MapInputs.Copy "one"
        let firstValue = MapInputs.Copy "uno"

        let table =
            creation.CreateWith (
                spanPaths[path],
                [| KeyValuePair (firstKey, firstValue); KeyValuePair (MapInputs.Copy "ONE", MapInputs.Copy "uno") |]
            )

        let struct (key, value) =
            Assert.ContainsSingle (
                HashMap.toArray table,
                $"%s{creation.Name} with %s{path} must merge the bindings of equal keys"
            )

        Assert.AreSame (firstKey, key, $"%s{creation.Name} with %s{path} must keep the first key")
        Assert.AreSame (firstValue, value, $"%s{creation.Name} with %s{path} must keep the first value")

    [<TestMethod; DynamicData(nameof CustomComparersHashMapTests.SpanCreationsAndPaths)>]
    member _.``the span members copy the span`` (creation : SpanCreation, path : string) =
        let pairs = [| KeyValuePair ("one", "uno") |]
        let table = creation.CreateWith (spanPaths[path], pairs)
        pairs[0] <- KeyValuePair ("one", "changed")
        Assert.AreEqual<string>("uno", HashMap.find "one" table, $"%s{creation.Name} with %s{path} must copy the span")

    [<TestMethod; DynamicData(nameof CustomComparersHashMapTests.SpanCreationsAndPaths)>]
    member _.``the span members take an array when the types of the struct are known`` (creation : SpanCreation, path : string) =
        let table = creation.CreateFromArrayWith (spanPaths[path], [| KeyValuePair ("one", "uno") |])

        CollectionAssert.AreEqual (
            [| struct ("one", "uno") |],
            HashMap.toArray table,
            $"%s{creation.Name} with %s{path} must take an array"
        )

    [<TestMethod>]
    member _.``the default value of the struct stands for the default comparers`` () =
        let comparers = CustomComparersHashMap<string, string>()

        Assert.AreSame<IEqualityComparer<string>>(
            EqualityComparer<string>.Default,
            comparers.KeyComparer,
            "The default value of the struct must report the default key comparer"
        )

        Assert.AreSame<IEqualityComparer<string>>(
            EqualityComparer<string>.Default,
            comparers.ValueComparer,
            "The default value of the struct must report the default value comparer"
        )

        Assert.AreSame<IEqualityComparer<string>>(
            EqualityComparer<string>.Default,
            HashMap.keyComparer (comparers.ofArray [| "a", "b" |]),
            "The default value of the struct must create hash maps with the default key comparer"
        )

    [<TestMethod>]
    member _.``ofSeq returns a hash map with the comparers of the struct itself and shares one with its key comparer`` () =
        let table =
            (HashMap.forKeyComparer StringComparer.OrdinalIgnoreCase).ofArray [| "one", "uno"; "two", "dos" |]

        Assert.AreSame (
            table,
            (HashMap.forKeyComparer StringComparer.OrdinalIgnoreCase).ofSeq table,
            "ofSeq must return a hash map with the comparers of the struct itself"
        )

        let byLengthTable = (HashMap.forComparers StringComparer.OrdinalIgnoreCase byLength).ofSeq table
        Assert.AreSame (byLength, HashMap.valueComparer byLengthTable, "ofSeq must give the hash map the value comparer")

        CollectionAssert.AreEqual (
            HashMap.toArray table,
            HashMap.toArray byLengthTable,
            "ofSeq must keep the bindings of a hash map with its key comparer, in their order"
        )

    [<TestMethod>]
    member _.``ofSeq gives a hash map the key comparer of the struct and keeps the last of the keys that become equal`` () =
        let table = HashMap.ofArray [| "a", 1; "A", 2; "b", 3 |]

        let struct (_, last) =
            HashMap.toArray table
            |> Array.filter (fun struct (key, _) -> String.Equals (key, "a", StringComparison.OrdinalIgnoreCase))
            |> Array.last

        let ignoreCase = (HashMap.forKeyComparer StringComparer.OrdinalIgnoreCase).ofSeq table

        Assert.AreSame<IEqualityComparer<string>>(
            StringComparer.OrdinalIgnoreCase,
            HashMap.keyComparer ignoreCase,
            "ofSeq must give the hash map the key comparer of the struct"
        )

        Assert.AreEqual<int>(2, HashMap.count ignoreCase, "ofSeq must merge the keys that the comparer of the struct makes equal")

        Assert.AreEqual<int>(
            last,
            HashMap.find "a" ignoreCase,
            "ofSeq must keep the value of the last of the equal keys in the order of enumeration"
        )

    [<TestMethod>]
    member _.``ofSeq keeps the first of the keys that become equal when the value comparer finds their values equal`` () =
        // "uno" and "abc" have the same length, so the earlier binding stays, its key included (ADR 0001, D14)
        let table = HashMap.ofArray [| "a", "uno"; "A", "abc"; "b", "dos" |]

        let struct (firstKey, firstValue) =
            HashMap.toArray table
            |> Array.find (fun struct (key, _) -> String.Equals (key, "a", StringComparison.OrdinalIgnoreCase))

        let ignoreCase = (HashMap.forComparers StringComparer.OrdinalIgnoreCase byLength).ofSeq table
        Assert.AreEqual<int>(2, HashMap.count ignoreCase, "ofSeq must merge the keys that the comparer of the struct makes equal")

        Assert.AreEqual<string voption>(
            ValueSome firstKey,
            HashMap.tryGetKey "a" ignoreCase,
            "ofSeq must keep the first of the equal keys in the order of enumeration"
        )

        Assert.AreEqual<string>(
            firstValue,
            HashMap.find "a" ignoreCase,
            "ofSeq must keep the value of the first of the equal keys"
        )

    [<TestMethod>]
    member _.``ofSeqBy compares the keys and the values with the comparers of the struct`` () =
        let comparers = HashMap.forComparers StringComparer.OrdinalIgnoreCase byLength
        // The two keys differ only in their case, so that the key comparer of the struct makes them equal
        let projection (value : string) =
            if String.Equals (value, "one", StringComparison.Ordinal) then
                "key"
            else
                "KEY"

        Assert.AreEqual<int>(
            1,
            HashMap.count (comparers.ofSeqBy projection [| "one"; "two" |]),
            "ofSeqBy must accept equal keys whose values are equal in the value comparer, as the BCL does"
        )

        Assert.ThrowsExactly<ArgumentException>(
            Action (fun () -> comparers.ofSeqBy projection [| "one"; "three" |] |> ignore),
            "ofSeqBy must reject equal keys whose values differ in the value comparer, as the BCL does"
        )
        |> ignore
