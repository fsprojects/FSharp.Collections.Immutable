namespace FSharp.Collections.Immutable.Tests

open System
open System.Collections.Generic
open System.Globalization
open FSharp.Collections.Immutable
open Microsoft.VisualStudio.TestTools.UnitTesting

/// The comparer struct of `FrozenMap`, `CustomComparersFrozenMap`, and the function of the module that creates it (ADR 0001,
/// D12). The creation contract runs its members with a key comparer; these tests cover the comparer itself.
[<TestClass; FrozenMapTestCategory>]
type CustomComparersFrozenMapTests () =

    static let nullComparer = Unchecked.defaultof<IEqualityComparer<string>>

    /// The members of the struct that create a frozen map, by name.
    static let creations : Map<string, CustomComparersFrozenMap<string, string> -> FrozenMap<string, string>> =
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
        |]

    /// The members of the struct that create a frozen map from the bindings of "a" to 1, "b" to 2 and "A" to 3, by name:
    /// keys that ignore the case make the first and the last binding equal.
    static let equalKeyCreations : Map<string, CustomComparersFrozenMap<string, int> -> FrozenMap<string, int>> =
        Map.ofArray [|
            "ofArray of key/value pairs",
            (fun comparers -> comparers.ofArray [| KeyValuePair ("a", 1); KeyValuePair ("b", 2); KeyValuePair ("A", 3) |])
            "ofArray of reference tuples", (fun comparers -> comparers.ofArray [| "a", 1; "b", 2; "A", 3 |])
            "ofArray of struct tuples",
            (fun comparers -> comparers.ofArray [| struct ("a", 1); struct ("b", 2); struct ("A", 3) |])
            "ofList of key/value pairs",
            (fun comparers -> comparers.ofList [ KeyValuePair ("a", 1); KeyValuePair ("b", 2); KeyValuePair ("A", 3) ])
            "ofList of reference tuples", (fun comparers -> comparers.ofList [ "a", 1; "b", 2; "A", 3 ])
            "ofList of struct tuples", (fun comparers -> comparers.ofList [ struct ("a", 1); struct ("b", 2); struct ("A", 3) ])
            "ofReadOnlySpan",
            (fun comparers ->
                comparers.ofReadOnlySpan (ReadOnlySpan [| KeyValuePair ("a", 1); KeyValuePair ("b", 2); KeyValuePair ("A", 3) |])
            )
            "ofSeq of key/value pairs",
            (fun comparers ->
                comparers.ofSeq (Seq.ofArray [| KeyValuePair ("a", 1); KeyValuePair ("b", 2); KeyValuePair ("A", 3) |])
            )
            "ofSeq of reference tuples", (fun comparers -> comparers.ofSeq (Seq.ofArray [| "a", 1; "b", 2; "A", 3 |]))
            "ofSeq of struct tuples",
            (fun comparers -> comparers.ofSeq (Seq.ofArray [| struct ("a", 1); struct ("b", 2); struct ("A", 3) |]))
            "ofSpan",
            (fun comparers -> comparers.ofSpan (Span [| KeyValuePair ("a", 1); KeyValuePair ("b", 2); KeyValuePair ("A", 3) |]))
        |]

    /// The members of the struct that take the span of an array, by name. A span cannot be captured, so each member makes
    /// its own.
    static let spanMembers
        : Map<string, CustomComparersFrozenMap<string, string> -> KeyValuePair<string, string> array -> FrozenMap<string, string>> =
        Map.ofArray [|
            "ofReadOnlySpan", (fun comparers pairs -> comparers.ofReadOnlySpan (ReadOnlySpan pairs))
            "ofSpan", (fun comparers pairs -> comparers.ofSpan (Span pairs))
        |]

    /// The members of the struct that take a sequence, by name, with the name of the sequence parameter and the member on a
    /// null sequence.
    static let nullSequenceMembers : Map<string, struct (string * (unit -> unit))> =
        let comparers : CustomComparersFrozenMap<string, string> =
            FrozenMap.forKeyComparer StringComparer.OrdinalIgnoreCase

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
        let comparers : CustomComparersFrozenMap<string, string> =
            FrozenMap.forKeyComparer StringComparer.OrdinalIgnoreCase

        let objectComparers : CustomComparersFrozenMap<obj, obj> =
            FrozenMap.forKeyComparer EqualityComparer<obj>.Default
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

    /// The bindings of a frozen map as reference tuples, sorted by key.
    static let sortedBindings (table : FrozenMap<string, int>) =
        FrozenMap.toArray table
        |> Array.map (fun struct (key, value) -> key, value)
        |> Array.sortBy fst

    /// The names of the members that create a frozen map, as the rows of the tests of their comparer.
    static member Creations = creations.Keys

    /// The names of the members that create a frozen map from keys that the comparer makes equal, as the rows of the test of
    /// the kept binding.
    static member EqualKeyCreations = equalKeyCreations.Keys

    /// The names of the members that take a span, as the rows of the tests of the spans.
    static member SpanMembers = spanMembers.Keys

    /// The names of the members that take a sequence and are given a null one, as the rows of their test.
    static member NullSequenceMembers = nullSequenceMembers.Keys

    /// The names of the members that take a sequence and are given a default flat list, as the rows of their test.
    static member DefaultSequenceMembers = defaultSequenceMembers.Keys

    [<TestMethod>]
    member _.``the constructor rejects a null comparer`` () =
        let ex =
            Assert.ThrowsExactly<ArgumentNullException>(
                Action (fun () ->
                    CustomComparersFrozenMap<string, string> nullComparer
                    |> ignore
                ),
                "The constructor must reject a null key comparer"
            )

        Assert.AreEqual<string | null>("keyComparer", ex.ParamName, "The constructor must name the key comparer")

    [<TestMethod>]
    member _.``forKeyComparer rejects a null comparer`` () =
        let ex =
            Assert.ThrowsExactly<ArgumentNullException>(
                Action (fun () ->
                    (FrozenMap.forKeyComparer nullComparer : CustomComparersFrozenMap<string, string>)
                    |> ignore
                ),
                "FrozenMap.forKeyComparer must reject a null key comparer"
            )

        Assert.AreEqual<string | null>("keyComparer", ex.ParamName, "FrozenMap.forKeyComparer must name the key comparer")

    [<TestMethod; DynamicData(nameof CustomComparersFrozenMapTests.NullSequenceMembers)>]
    member _.``members reject a null sequence`` (name : string) =
        let struct (paramName, call) = nullSequenceMembers[name]
        FlatListAssert.RejectsNull ($"CustomComparersFrozenMap.%s{name}", paramName, call)

    [<TestMethod; DynamicData(nameof CustomComparersFrozenMapTests.DefaultSequenceMembers)>]
    member _.``members reject a default flat list given as a sequence`` (name : string) =
        // The exception names the parameter of the member, not the one of the BCL member that it would have called
        let struct (paramName, call) = defaultSequenceMembers[name]
        FlatListAssert.RejectsNull ($"CustomComparersFrozenMap.%s{name}", paramName, call)

    [<TestMethod>]
    member _.``members take a flat list that is not default as a sequence, also one of strings as a sequence of objects`` () =
        let flatList (elements : 'T array) = FlatList.ofArray elements :> seq<'T>
        let comparers : CustomComparersFrozenMap<string, string> =
            FrozenMap.forKeyComparer StringComparer.OrdinalIgnoreCase

        let bindingsOf (table : FrozenMap<string, string>) =
            FrozenMap.toArray table
            |> Array.sortBy (fun struct (key, _) -> key)

        CollectionAssert.AreEqual (
            [| struct ("a", "b") |],
            bindingsOf (comparers.ofSeq (flatList [| KeyValuePair ("a", "b") |])),
            "ofSeq must take the key/value pairs of a flat list"
        )

        CollectionAssert.AreEqual (
            [| struct ("a", "b") |],
            bindingsOf (comparers.ofSeq (flatList [| ("a", "b") |])),
            "ofSeq must take the reference tuples of a flat list"
        )

        CollectionAssert.AreEqual (
            [| struct ("a", "b") |],
            bindingsOf (comparers.ofSeq (flatList [| struct ("a", "b") |])),
            "ofSeq must take the struct tuples of a flat list"
        )

        CollectionAssert.AreEqual (
            [| struct ("ab", "ab") |],
            bindingsOf (comparers.ofSeqBy id (flatList [| "ab" |])),
            "ofSeqBy must take the elements of a flat list"
        )

        // A flat list of strings is a sequence of objects, which a type test for the flat list of objects does not see
        let objectComparers : CustomComparersFrozenMap<obj, obj> =
            FrozenMap.forKeyComparer EqualityComparer<obj>.Default
        let widened = FlatListOracles.stringsAsObjects (FlatList.ofArray [| "ab" |])

        CollectionAssert.AreEqual (
            [| "ab" :> obj |],
            FrozenMap.keys (objectComparers.ofSeqBy id widened)
            |> Seq.toArray,
            "ofSeqBy must take the elements of a flat list of strings as objects"
        )

    [<TestMethod>]
    member _.``forKeyComparer holds the key comparer`` () =
        let comparers : CustomComparersFrozenMap<string, string> =
            FrozenMap.forKeyComparer StringComparer.OrdinalIgnoreCase

        Assert.AreSame<IEqualityComparer<string>>(
            StringComparer.OrdinalIgnoreCase,
            comparers.KeyComparer,
            "FrozenMap.forKeyComparer must hold the key comparer"
        )

    [<TestMethod>]
    member _.``forKeyComparer StringComparer.OrdinalIgnoreCase applies the comparer`` () =
        let table =
            (FrozenMap.forKeyComparer StringComparer.OrdinalIgnoreCase).ofArray [| "one", 1; "ONE", 2; "two", 3 |]

        Assert.AreSame<IEqualityComparer<string>>(
            StringComparer.OrdinalIgnoreCase,
            FrozenMap.keyComparer table,
            "The frozen map of FrozenMap.forKeyComparer must have the key comparer"
        )

        Assert.AreEqual<int>(2, FrozenMap.count table, "The comparer must merge the keys that differ in their case")
        Assert.AreEqual<int>(2, FrozenMap.find "One" table, "The frozen map must keep the last binding of keys that are equal")
        Assert.AreEqual<int>(3, FrozenMap.find "TWO" table, "The frozen map must look the keys up with the comparer")

    [<TestMethod; DynamicData(nameof CustomComparersFrozenMapTests.Creations)>]
    member _.``every member creates frozen maps with the comparer of the struct`` (creation : string) =
        let table = creations[creation](FrozenMap.forKeyComparer StringComparer.OrdinalIgnoreCase)

        Assert.AreSame<IEqualityComparer<string>>(
            StringComparer.OrdinalIgnoreCase,
            FrozenMap.keyComparer table,
            "The member must create a frozen map with the key comparer of the struct"
        )

    [<TestMethod>]
    member _.``the default value of the struct stands for the default comparer`` () =
        let comparers = CustomComparersFrozenMap<string, string>()

        Assert.AreSame<IEqualityComparer<string>>(
            EqualityComparer<string>.Default,
            comparers.KeyComparer,
            "The default value of the struct must report the default key comparer"
        )

    [<TestMethod; DynamicData(nameof CustomComparersFrozenMapTests.Creations)>]
    member _.``every member of the default value of the struct creates frozen maps with the default comparer``
        (creation : string)
        =
        let table = creations[creation](CustomComparersFrozenMap<string, string>())

        Assert.AreSame<IEqualityComparer<string>>(
            EqualityComparer<string>.Default,
            FrozenMap.keyComparer table,
            "The member of the default value of the struct must create a frozen map with the default key comparer"
        )

    [<TestMethod; DynamicData(nameof CustomComparersFrozenMapTests.EqualKeyCreations)>]
    member _.``every shape keeps the first of the keys that the comparer makes equal with the value of the last one``
        (creation : string)
        =
        // ToFrozenDictionary keeps the key of the earlier binding and the value of the later one (ADR 0001, D14)
        CollectionAssert.AreEqual (
            [| "a", 3; "b", 2 |],
            sortedBindings (equalKeyCreations[creation](FrozenMap.forKeyComparer StringComparer.OrdinalIgnoreCase)),
            "The frozen map must keep the first of the equal keys with the value of the last one"
        )

    [<TestMethod>]
    member _.``ofSeq returns a frozen map with the comparer of the struct itself`` () =
        let table = (FrozenMap.forKeyComparer StringComparer.OrdinalIgnoreCase).ofArray [| "one", 1; "two", 2 |]

        Assert.AreSame (
            table,
            (FrozenMap.forKeyComparer StringComparer.OrdinalIgnoreCase).ofSeq table,
            "ofSeq must return a frozen map with the comparer of the struct itself"
        )

    // The first two rows hold equal comparers that are not the same instance, which ToFrozenDictionary accepts as well
    // (ADR 0001, D14)
    [<TestMethod; DataRow(true, true, true); DataRow(false, false, true); DataRow(true, false, false); DataRow(false, true, false)>]
    member _.``ofSeq returns a frozen map itself only when its comparer equals the comparer of the struct``
        (mapIgnoresCase : bool, structIgnoresCase : bool, itself : bool)
        =
        // Two comparers of the same culture and options are equal without being the same instance
        let mapComparer = StringComparer.Create (CultureInfo.InvariantCulture, mapIgnoresCase)
        let structComparer = StringComparer.Create (CultureInfo.InvariantCulture, structIgnoresCase)
        Assert.AreNotSame (mapComparer, structComparer, "The comparers must be two instances")
        let table = (FrozenMap.forKeyComparer mapComparer).ofArray [| "one", 1; "two", 2 |]
        let result = (FrozenMap.forKeyComparer structComparer).ofSeq table

        Assert.AreEqual<bool>(
            itself,
            obj.ReferenceEquals (table, result),
            "ofSeq must return a frozen map itself exactly when its comparer equals the comparer of the struct"
        )

        CollectionAssert.AreEqual (sortedBindings table, sortedBindings result, "ofSeq must keep the bindings of a frozen map")

    [<TestMethod>]
    member _.``ofSeq gives a frozen map the key comparer of the struct and merges the keys that become equal`` () =
        let table = FrozenMap.ofArray [| "a", 1; "A", 2; "b", 3 |]

        // A frozen map enumerates its bindings in an unspecified order, which decides the first key and the last value
        let equalKeys =
            FrozenMap.toArray table
            |> Array.filter (fun struct (key, _) -> String.Equals (key, "a", StringComparison.OrdinalIgnoreCase))

        let struct (firstKey, _) = Array.head equalKeys
        let struct (_, lastValue) = Array.last equalKeys
        let ignoreCase = (FrozenMap.forKeyComparer StringComparer.OrdinalIgnoreCase).ofSeq table

        Assert.AreSame<IEqualityComparer<string>>(
            StringComparer.OrdinalIgnoreCase,
            FrozenMap.keyComparer ignoreCase,
            "ofSeq must give the frozen map the key comparer of the struct"
        )

        CollectionAssert.AreEqual (
            [| firstKey, lastValue; "b", 3 |],
            sortedBindings ignoreCase,
            "ofSeq must keep the first of the keys that become equal in the order of enumeration, with the last value"
        )

    [<TestMethod; DynamicData(nameof CustomComparersFrozenMapTests.SpanMembers)>]
    member _.``the span members copy the span`` (name : string) =
        let pairs = [| KeyValuePair ("one", "uno") |]
        let table = spanMembers[name] (FrozenMap.forKeyComparer StringComparer.OrdinalIgnoreCase) pairs
        pairs[0] <- KeyValuePair ("one", "changed")
        Assert.AreEqual<string>("uno", FrozenMap.find "ONE" table, $"%s{name} must copy the span")

    [<TestMethod; DynamicData(nameof CustomComparersFrozenMapTests.SpanMembers)>]
    member _.``the span members create an empty map with the comparer for no bindings`` (name : string) =
        let table = spanMembers[name] (FrozenMap.forKeyComparer StringComparer.OrdinalIgnoreCase) [||]

        Assert.IsTrue (FrozenMap.isEmpty table, $"%s{name} must create an empty map for no bindings")

        Assert.AreSame<IEqualityComparer<string>>(
            StringComparer.OrdinalIgnoreCase,
            FrozenMap.keyComparer table,
            $"%s{name} must give the empty map the key comparer of the struct"
        )

    [<TestMethod; DynamicData(nameof CustomComparersFrozenMapTests.SpanMembers)>]
    member _.``the span members keep the first key and the last value of the keys that the comparer makes equal``
        (name : string)
        =
        // The copies tell the two bindings apart; the keys are equal in the comparer of the struct (ADR 0001, D14)
        let firstKey = MapInputs.Copy "one"
        let lastValue = MapInputs.Copy "dos"

        let table =
            spanMembers[name] (FrozenMap.forKeyComparer StringComparer.OrdinalIgnoreCase) [|
                KeyValuePair (firstKey, "uno")
                KeyValuePair (MapInputs.Copy "ONE", lastValue)
            |]

        let struct (key, value) =
            Assert.ContainsSingle (FrozenMap.toArray table, $"%s{name} must merge the bindings of the equal keys")

        Assert.AreSame (firstKey, key, $"%s{name} must keep the first key")
        Assert.AreSame (lastValue, value, $"%s{name} must keep the last value")

    [<TestMethod>]
    member _.``ofSeqBy rejects the keys that the comparer of the struct makes equal`` () =
        let comparers = FrozenMap.forKeyComparer StringComparer.OrdinalIgnoreCase

        Assert.AreEqual<int>(
            2,
            FrozenMap.count (comparers.ofSeqBy id [| "a"; "b" |]),
            "ofSeqBy must accept the keys that differ in the key comparer"
        )

        Assert.ThrowsExactly<ArgumentException>(
            Action (fun () -> comparers.ofSeqBy id [| "a"; "A" |] |> ignore),
            "ofSeqBy must reject the keys that the key comparer makes equal, as the BCL does"
        )
        |> ignore
