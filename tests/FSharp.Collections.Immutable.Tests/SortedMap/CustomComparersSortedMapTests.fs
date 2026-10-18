namespace FSharp.Collections.Immutable.Tests

open System
open System.Collections.Generic
open FSharp.Collections.Immutable
open Microsoft.VisualStudio.TestTools.UnitTesting

/// The comparer struct of `SortedMap`, `CustomComparersSortedMap`, and the functions of the module that create it (ADR 0001,
/// D12). The creation contract runs its members with a key comparer; these tests cover the comparers themselves.
[<TestClass; SortedMapTestCategory>]
type CustomComparersSortedMapTests () =

    /// A key comparer that differs from the default one: it orders strings ordinally from the last to the first.
    static let reverseOrdinal =
        ComparisonIdentity.FromFunction (fun (left : string) (right : string) -> String.CompareOrdinal (right, left))

    /// A key comparer that orders integers from the largest to the smallest.
    static let descending = ComparisonIdentity.FromFunction (fun (left : int) (right : int) -> compare right left)

    /// A value comparer that differs from the default one: it compares strings by their length.
    static let byLength =
        HashIdentity.FromFunctions (fun (value : string) -> value.Length) (fun left right -> left.Length = right.Length)

    /// A value comparer of numbers that differs from the default one: it finds numbers of the same absolute value equal.
    static let absoluteValues =
        HashIdentity.FromFunctions (fun (value : int) -> abs value) (fun left right -> abs left = abs right)

    /// The struct whose key comparer ignores the case of the keys.
    static let ignoreCase : CustomComparersSortedMap<string, int> =
        SortedMap.forKeyComparer StringComparer.OrdinalIgnoreCase

    static let nullKeyComparer = Unchecked.defaultof<IComparer<string>>

    static let nullValueComparer = Unchecked.defaultof<IEqualityComparer<string>>

    /// The key comparer and the value comparer of a row of the tests of null comparers: the comparer whose parameter the
    /// row names is null.
    static let comparersWithNull (nullParameter : string) : struct (IComparer<string> * IEqualityComparer<string>) =
        match nullParameter with
        | "keyComparer" -> struct (nullKeyComparer, byLength)
        | _ -> struct (StringComparer.Ordinal, nullValueComparer)

    /// The members of the struct that create a sorted map of the keys "b", "a" and "c", each bound to itself, by name.
    static let creations : Map<string, CustomComparersSortedMap<string, string> -> SortedMap<string, string>> =
        Map.ofArray [|
            "empty",
            (fun comparers ->
                comparers.empty
                |> SortedMap.add "b" "b"
                |> SortedMap.add "a" "a"
                |> SortedMap.add "c" "c"
            )
            "ofArray of key/value pairs",
            (fun comparers -> comparers.ofArray [| KeyValuePair ("b", "b"); KeyValuePair ("a", "a"); KeyValuePair ("c", "c") |])
            "ofArray of reference tuples", (fun comparers -> comparers.ofArray [| "b", "b"; "a", "a"; "c", "c" |])
            "ofArray of struct tuples",
            (fun comparers -> comparers.ofArray [| struct ("b", "b"); struct ("a", "a"); struct ("c", "c") |])
            "ofList of key/value pairs",
            (fun comparers -> comparers.ofList [ KeyValuePair ("b", "b"); KeyValuePair ("a", "a"); KeyValuePair ("c", "c") ])
            "ofList of reference tuples", (fun comparers -> comparers.ofList [ "b", "b"; "a", "a"; "c", "c" ])
            "ofList of struct tuples",
            (fun comparers -> comparers.ofList [ struct ("b", "b"); struct ("a", "a"); struct ("c", "c") ])
            "ofSeq of key/value pairs", (fun comparers -> comparers.ofSeq (dict [| "b", "b"; "a", "a"; "c", "c" |]))
            "ofSeq of reference tuples", (fun comparers -> comparers.ofSeq (Seq.ofArray [| "b", "b"; "a", "a"; "c", "c" |]))
            "ofSeq of struct tuples",
            (fun comparers -> comparers.ofSeq (Seq.ofArray [| struct ("b", "b"); struct ("a", "a"); struct ("c", "c") |]))
            "ofSeqBy", (fun comparers -> comparers.ofSeqBy id [| "b"; "a"; "c" |])
            "singleton",
            (fun comparers ->
                comparers.singleton "b" "b"
                |> SortedMap.add "a" "a"
                |> SortedMap.add "c" "c"
            )
            "build",
            (fun comparers ->
                comparers.build (fun builder ->
                    builder["b"] <- "b"
                    builder["a"] <- "a"
                    builder["c"] <- "c"
                )
            )
            "builder",
            (fun comparers ->
                let builder = comparers.builder ()
                builder["b"] <- "b"
                builder["a"] <- "a"
                builder["c"] <- "c"
                builder.ToImmutable ()
            )
        |]

    /// The creations of a sorted map whose keys ignore the case from the bindings of "A" to 3, "B" to 2, "a" to 1 and "b" to
    /// 4, in this order, by the member and the source that they take. The keys "a" and "b" come later than "A" and "B" in
    /// every source, F# generic comparison included, so that their bindings are the last ones of their keys.
    static let ignoreCaseCreations : Map<string, unit -> SortedMap<string, int>> =
        Map.ofArray [|
            "ofArray of key/value pairs",
            (fun () ->
                ignoreCase.ofArray [|
                    KeyValuePair ("A", 3)
                    KeyValuePair ("B", 2)
                    KeyValuePair ("a", 1)
                    KeyValuePair ("b", 4)
                |]
            )
            "ofArray of reference tuples", (fun () -> ignoreCase.ofArray [| "A", 3; "B", 2; "a", 1; "b", 4 |])
            "ofArray of struct tuples",
            (fun () -> ignoreCase.ofArray [| struct ("A", 3); struct ("B", 2); struct ("a", 1); struct ("b", 4) |])
            "ofList of key/value pairs",
            (fun () ->
                ignoreCase.ofList [ KeyValuePair ("A", 3); KeyValuePair ("B", 2); KeyValuePair ("a", 1); KeyValuePair ("b", 4) ]
            )
            "ofList of reference tuples", (fun () -> ignoreCase.ofList [ "A", 3; "B", 2; "a", 1; "b", 4 ])
            "ofList of struct tuples",
            (fun () -> ignoreCase.ofList [ struct ("A", 3); struct ("B", 2); struct ("a", 1); struct ("b", 4) ])
            "ofSeq of key/value pairs",
            (fun () ->
                ignoreCase.ofSeq (
                    Seq.ofArray [| KeyValuePair ("A", 3); KeyValuePair ("B", 2); KeyValuePair ("a", 1); KeyValuePair ("b", 4) |]
                )
            )
            "ofSeq of reference tuples", (fun () -> ignoreCase.ofSeq (Seq.ofArray [| "A", 3; "B", 2; "a", 1; "b", 4 |]))
            "ofSeq of struct tuples",
            (fun () -> ignoreCase.ofSeq (Seq.ofArray [| struct ("A", 3); struct ("B", 2); struct ("a", 1); struct ("b", 4) |]))
            "ofSeq of a dictionary", (fun () -> ignoreCase.ofSeq (dict [| "A", 3; "B", 2; "a", 1; "b", 4 |]))
            "ofSeq of an F# map", (fun () -> ignoreCase.ofSeq (Map.ofArray [| "a", 1; "b", 4; "A", 3; "B", 2 |]))
            // The sorted map has another key comparer, so ofSeq gives its bindings the comparer of the struct one by one
            "ofSeq of a sorted map", (fun () -> ignoreCase.ofSeq (SortedMap.ofArray [| "a", 1; "b", 4; "A", 3; "B", 2 |]))
        |]

    /// The members of the struct that create a sorted map from the bindings of "one" to "uno" and of "ONE" to "abc", by
    /// name: keys that ignore the case and values that compare by their length make the two bindings equal.
    static let equalValueCreations : Map<string, CustomComparersSortedMap<string, string> -> SortedMap<string, string>> =
        Map.ofArray [|
            "ofArray of key/value pairs",
            (fun comparers -> comparers.ofArray [| KeyValuePair ("one", "uno"); KeyValuePair ("ONE", "abc") |])
            "ofArray of reference tuples", (fun comparers -> comparers.ofArray [| "one", "uno"; "ONE", "abc" |])
            "ofArray of struct tuples", (fun comparers -> comparers.ofArray [| struct ("one", "uno"); struct ("ONE", "abc") |])
            "ofList of key/value pairs",
            (fun comparers -> comparers.ofList [ KeyValuePair ("one", "uno"); KeyValuePair ("ONE", "abc") ])
            "ofList of reference tuples", (fun comparers -> comparers.ofList [ "one", "uno"; "ONE", "abc" ])
            "ofList of struct tuples", (fun comparers -> comparers.ofList [ struct ("one", "uno"); struct ("ONE", "abc") ])
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
        |]

    /// The members of the struct that take a sequence, by name, with the name of the sequence parameter and the member on a
    /// null sequence. The struct has comparers that differ from the default ones.
    static let nullSequenceMembers : Map<string, struct (string * (unit -> unit))> =
        let comparers = SortedMap.forComparers StringComparer.OrdinalIgnoreCase byLength

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
        let comparers = SortedMap.forComparers StringComparer.OrdinalIgnoreCase byLength

        let objectComparers : CustomComparersSortedMap<obj, obj> =
            SortedMap.forKeyComparer Comparer<obj>.Default

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

    /// The names of the members that take a sequence and are given a null one, as the rows of their test.
    static member NullSequenceMembers = nullSequenceMembers.Keys

    /// The names of the members that take a sequence and are given a default flat list, as the rows of their test.
    static member DefaultSequenceMembers = defaultSequenceMembers.Keys

    /// The names of the members that create a sorted map, as the rows of the test of their comparers.
    static member Creations = creations.Keys

    /// The names of the creations from bindings whose keys the comparer of the struct considers equal.
    static member IgnoreCaseCreations = ignoreCaseCreations.Keys

    /// The names of the members that create a sorted map from bindings that the comparers find equal, as the rows of their
    /// test.
    static member EqualValueCreations = equalValueCreations.Keys

    /// The keys of the sorted maps that the tests of an existing map give to ofSeq. The property builds new arrays on each
    /// read.
    static member Keys : int[] seq = seq {
        [||]
        [| 42 |]
        [| 5; -3; 8; 0; 12 |]
    }

    [<TestMethod; DataRow "keyComparer"; DataRow "valueComparer">]
    member _.``the constructor with both comparers rejects a null comparer`` (nullParameter : string) =
        let struct (keyComparer, valueComparer) = comparersWithNull nullParameter

        let ex =
            Assert.ThrowsExactly<ArgumentNullException>(
                Action (fun () ->
                    CustomComparersSortedMap<string, string>(keyComparer, valueComparer)
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
                    CustomComparersSortedMap<string, string> nullKeyComparer
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
                Action (fun () -> SortedMap.forComparers keyComparer valueComparer |> ignore),
                "SortedMap.forComparers must reject a null comparer"
            )

        Assert.AreEqual<string | null>(nullParameter, ex.ParamName, "SortedMap.forComparers must name the null comparer")

    [<TestMethod>]
    member _.``forKeyComparer rejects a null comparer`` () =
        let ex =
            Assert.ThrowsExactly<ArgumentNullException>(
                Action (fun () ->
                    (SortedMap.forKeyComparer nullKeyComparer : CustomComparersSortedMap<string, string>)
                    |> ignore
                ),
                "SortedMap.forKeyComparer must reject a null key comparer"
            )

        Assert.AreEqual<string | null>("keyComparer", ex.ParamName, "SortedMap.forKeyComparer must name the key comparer")

    [<TestMethod; DynamicData(nameof CustomComparersSortedMapTests.NullSequenceMembers)>]
    member _.``members reject a null sequence`` (name : string) =
        let struct (paramName, call) = nullSequenceMembers[name]
        FlatListAssert.RejectsNull ($"CustomComparersSortedMap.%s{name}", paramName, call)

    [<TestMethod; DynamicData(nameof CustomComparersSortedMapTests.DefaultSequenceMembers)>]
    member _.``members reject a default flat list given as a sequence`` (name : string) =
        // The exception names the parameter of the member, not the one of the BCL member that it would have called
        let struct (paramName, call) = defaultSequenceMembers[name]
        FlatListAssert.RejectsNull ($"CustomComparersSortedMap.%s{name}", paramName, call)

    [<TestMethod>]
    member _.``members take a flat list that is not default as a sequence, also one of strings as a sequence of objects`` () =
        let flatList (elements : 'T array) = FlatList.ofArray elements :> seq<'T>
        let comparers = SortedMap.forKeyComparer StringComparer.OrdinalIgnoreCase

        CollectionAssert.AreEqual (
            [| struct ("a", "b") |],
            SortedMap.toArray (comparers.ofSeq (flatList [| KeyValuePair ("a", "b") |])),
            "ofSeq must take the key/value pairs of a flat list"
        )

        CollectionAssert.AreEqual (
            [| struct ("a", "b") |],
            SortedMap.toArray (comparers.ofSeq (flatList [| ("a", "b") |])),
            "ofSeq must take the reference tuples of a flat list"
        )

        CollectionAssert.AreEqual (
            [| struct ("a", "b") |],
            SortedMap.toArray (comparers.ofSeq (flatList [| struct ("a", "b") |])),
            "ofSeq must take the struct tuples of a flat list"
        )

        CollectionAssert.AreEqual (
            [| struct ("ab", "ab") |],
            SortedMap.toArray (comparers.ofSeqBy id (flatList [| "ab" |])),
            "ofSeqBy must take the elements of a flat list"
        )

        // A flat list of strings is a sequence of objects, which a type test for the flat list of objects does not see
        let objectComparers : CustomComparersSortedMap<obj, obj> =
            SortedMap.forKeyComparer Comparer<obj>.Default
        let widened = FlatListOracles.stringsAsObjects (FlatList.ofArray [| "ab" |])

        CollectionAssert.AreEqual (
            [| "ab" :> obj |],
            SortedMap.keys (objectComparers.ofSeqBy id widened)
            |> Seq.toArray,
            "ofSeqBy must take the elements of a flat list of strings as objects"
        )

    [<TestMethod>]
    member _.``forComparers holds both comparers`` () =
        let comparers = SortedMap.forComparers reverseOrdinal byLength
        Assert.AreSame (reverseOrdinal, comparers.KeyComparer, "SortedMap.forComparers must hold the key comparer")
        Assert.AreSame (byLength, comparers.ValueComparer, "SortedMap.forComparers must hold the value comparer")

    [<TestMethod>]
    member _.``forKeyComparer holds the key comparer and the default comparer of the values`` () =
        let comparers : CustomComparersSortedMap<string, string> = SortedMap.forKeyComparer reverseOrdinal
        Assert.AreSame (reverseOrdinal, comparers.KeyComparer, "SortedMap.forKeyComparer must hold the key comparer")

        Assert.AreSame<IEqualityComparer<string>>(
            EqualityComparer<string>.Default,
            comparers.ValueComparer,
            "SortedMap.forKeyComparer must hold the default comparer of the values"
        )

    [<TestMethod>]
    member _.``forKeyComparer StringComparer.OrdinalIgnoreCase applies the comparer`` () =
        let table =
            (SortedMap.forKeyComparer StringComparer.OrdinalIgnoreCase).ofSeq [| "two", 2; "one", 1; "ONE", 3 |]

        Assert.AreSame<IComparer<string>>(
            StringComparer.OrdinalIgnoreCase,
            SortedMap.keyComparer table,
            "The sorted map of SortedMap.forKeyComparer must have the key comparer"
        )

        CollectionAssert.AreEqual (
            [| struct ("ONE", 3); struct ("two", 2) |],
            SortedMap.toArray table,
            "The comparer must merge the keys that differ in their case into the last binding of them, in its order"
        )

        Assert.AreEqual<int>(2, SortedMap.find "TWO" table, "The sorted map must look the keys up with the comparer")

    [<TestMethod; DynamicData(nameof CustomComparersSortedMapTests.Creations)>]
    member _.``every member creates sorted maps with the comparers of the struct`` (creation : string) =
        let table = creations[creation](SortedMap.forComparers reverseOrdinal byLength)

        Assert.AreSame (
            reverseOrdinal,
            SortedMap.keyComparer table,
            "The member must create a sorted map with the key comparer of the struct"
        )

        Assert.AreSame (
            byLength,
            SortedMap.valueComparer table,
            "The member must create a sorted map with the value comparer of the struct"
        )

        CollectionAssert.AreEqual (
            [| "c"; "b"; "a" |],
            Seq.toArray (SortedMap.keys table),
            "The member must order the keys with the key comparer of the struct"
        )

    [<TestMethod; DynamicData(nameof CustomComparersSortedMapTests.IgnoreCaseCreations)>]
    member _.``every source keeps the last binding of the keys that the comparer considers equal`` (creation : string) =
        CollectionAssert.AreEqual (
            [| struct ("a", 1); struct ("b", 4) |],
            SortedMap.toArray (ignoreCaseCreations[creation]()),
            "The sorted map must keep the last binding of the keys that differ in their case, as Map keeps the last binding of a key"
        )

    [<TestMethod; DynamicData(nameof CustomComparersSortedMapTests.EqualValueCreations)>]
    member _.``every member keeps the earlier binding of a key for a value that the value comparer finds equal``
        (creation : string)
        =
        // The indexer of the builder and ToImmutableSortedDictionary keep the earlier binding, its key included (ADR 0001,
        // D14)
        let table =
            equalValueCreations[creation](SortedMap.forComparers StringComparer.OrdinalIgnoreCase byLength)

        let struct (key, value) =
            Assert.ContainsSingle (SortedMap.toArray table, "The member must merge the bindings of the equal keys")

        Assert.AreEqual<string>("one", key, "The member must keep the earlier key")
        Assert.AreEqual<string>("uno", value, "The member must keep the earlier value")

    [<TestMethod>]
    member _.``the default value of the struct stands for the comparers that the BCL takes for null ones`` () =
        let comparers = CustomComparersSortedMap<string, string>()
        let words = [| "b"; "a"; "B"; "A"; "_"; "10"; "9" |]
        let table = comparers.ofArray (words |> Array.map (fun word -> word, word))

        Assert.AreSame<IComparer<string>>(
            Comparer<string>.Default,
            comparers.KeyComparer,
            "The default value of the struct must report the key comparer of the BCL"
        )

        Assert.AreSame<IEqualityComparer<string>>(
            EqualityComparer<string>.Default,
            comparers.ValueComparer,
            "The default value of the struct must report the default value comparer"
        )

        Assert.AreSame<IComparer<string>>(
            Comparer<string>.Default,
            SortedMap.keyComparer table,
            "The default value of the struct must create sorted maps with the key comparer of the BCL"
        )

        // The order of the current culture, which differs from the ordinal order of F# generic comparison
        CollectionAssert.AreEqual (
            Array.sortWith (fun left right -> Comparer<string>.Default.Compare(left, right)) words,
            Seq.toArray (SortedMap.keys table),
            "The default value of the struct must order the keys as Comparer<string>.Default does"
        )

    [<TestMethod; DynamicData(nameof CustomComparersSortedMapTests.Keys)>]
    member _.``ofSeq returns a sorted map with the comparers of the struct itself`` (keys : int array) =
        let comparers : CustomComparersSortedMap<int, string> = SortedMap.forKeyComparer descending
        let table = comparers.ofArray (keys |> Array.map (fun key -> struct (key, string key)))
        Assert.AreSame (table, comparers.ofSeq table, "ofSeq must return a sorted map with the comparers of the struct itself")

    [<TestMethod; DynamicData(nameof CustomComparersSortedMapTests.Keys)>]
    member _.``ofSeq keeps a sorted map with the key comparer of the struct and gives it the value comparer`` (keys : int array) =
        let table =
            (SortedMap.forKeyComparer descending).ofArray(keys |> Array.map (fun key -> struct (key, string key)))
        let withValueComparer = (SortedMap.forComparers descending byLength).ofSeq table
        Assert.AreSame (descending, SortedMap.keyComparer withValueComparer, "ofSeq must keep the key comparer of the struct")
        Assert.AreSame (byLength, SortedMap.valueComparer withValueComparer, "ofSeq must give the sorted map the value comparer")

        CollectionAssert.AreEqual (
            SortedMap.toArray table,
            SortedMap.toArray withValueComparer,
            "ofSeq must keep the bindings of a sorted map with the key comparer of the struct, in their order"
        )

    [<TestMethod>]
    member _.``ofSeq keeps the first of the keys that become equal when the value comparer finds their values equal`` () =
        // "uno" and "abc" have the same length, so the earlier binding in the order of enumeration stays, its key included
        // (ADR 0001, D14). The sorted map orders "A" before "a", so "A" is the first of the two keys.
        let table = SortedMap.ofArray [| "a", "uno"; "A", "abc"; "b", "dos" |]

        let struct (firstKey, firstValue) =
            SortedMap.toArray table
            |> Array.find (fun struct (key, _) -> String.Equals (key, "a", StringComparison.OrdinalIgnoreCase))

        let ignoreCase = (SortedMap.forComparers StringComparer.OrdinalIgnoreCase byLength).ofSeq table

        Assert.AreEqual<int>(
            2,
            SortedMap.count ignoreCase,
            "ofSeq must merge the keys that the comparer of the struct makes equal"
        )

        Assert.AreEqual<string voption>(
            ValueSome firstKey,
            SortedMap.tryGetKey "a" ignoreCase,
            "ofSeq must keep the first of the equal keys in the order of enumeration"
        )

        Assert.AreEqual<string>(
            firstValue,
            SortedMap.find "a" ignoreCase,
            "ofSeq must keep the value of the first of the equal keys"
        )

    [<TestMethod>]
    member _.``ofSeq rebuilds a sorted map whose key comparer is another instance, even when it is equivalent`` () =
        let ordinal () =
            ComparisonIdentity.FromFunction (fun (left : string) right -> String.CompareOrdinal (left, right))

        let table = (SortedMap.forKeyComparer (ordinal ())).ofArray [| "a", 1; "b", 2 |]
        let equivalent = ordinal ()
        let result = (SortedMap.forKeyComparer equivalent).ofSeq table

        Assert.AreNotSame (table, result, "ofSeq must rebuild a sorted map whose key comparer is another instance")
        Assert.AreSame (equivalent, SortedMap.keyComparer result, "ofSeq must give the bindings the key comparer of the struct")

        CollectionAssert.AreEqual (
            SortedMap.toArray table,
            SortedMap.toArray result,
            "ofSeq must keep the bindings of a sorted map whose key comparer is equivalent"
        )

    [<TestMethod>]
    member _.``ofSeq gives a sorted map of the module another value comparer through its own key comparer`` () =
        // The sorted maps of the module order their keys with F# generic comparison: "A" comes before "a"
        let table = SortedMap.ofArray [| "b", 2; "a", 1; "A", 0 |]
        let keyComparer = SortedMap.keyComparer table

        Assert.AreSame (
            table,
            (SortedMap.forKeyComparer keyComparer).ofSeq table,
            "ofSeq must return a sorted map with the key comparer and the value comparer of the struct itself"
        )

        let withValueComparer = (SortedMap.forComparers keyComparer absoluteValues).ofSeq table
        Assert.AreSame (keyComparer, SortedMap.keyComparer withValueComparer, "ofSeq must keep the key comparer")
        Assert.AreSame (absoluteValues, SortedMap.valueComparer withValueComparer, "ofSeq must give the value comparer")

        CollectionAssert.AreEqual (
            SortedMap.toArray table,
            SortedMap.toArray withValueComparer,
            "ofSeq must keep the bindings of the sorted map, in their order"
        )

    [<TestMethod>]
    member _.``ofSeq merges the keys that the key comparer of the struct makes equal where WithComparers throws`` () =
        // The sorted map orders "A" before "a", so "a" is the later binding of the two keys
        let table = SortedMap.ofArray [| "a", 1; "A", 2 |]
        let merged = (SortedMap.forKeyComparer StringComparer.OrdinalIgnoreCase).ofSeq table

        CollectionAssert.AreEqual (
            [| struct ("a", 1) |],
            SortedMap.toArray merged,
            "ofSeq must merge the keys into the later binding in the order of enumeration"
        )

        Assert.ThrowsExactly<ArgumentException>(
            Action (fun () ->
                table.WithComparers StringComparer.OrdinalIgnoreCase
                |> ignore
            ),
            "WithComparers must reject keys that become equal with another value, which ofSeq merges"
        )
        |> ignore

    [<TestMethod>]
    member _.``a struct from forKeyComparer replaces the value comparer of the sorted map where WithComparers keeps it`` () =
        let table = (SortedMap.forComparers StringComparer.Ordinal absoluteValues).ofArray [| "a", 1 |]

        Assert.AreSame<IEqualityComparer<int>>(
            EqualityComparer<int>.Default,
            SortedMap.valueComparer ((SortedMap.forKeyComparer StringComparer.OrdinalIgnoreCase).ofSeq table),
            "ofSeq of a struct from forKeyComparer must give the default comparer of the values"
        )

        Assert.AreSame (
            absoluteValues,
            SortedMap.valueComparer (
                (SortedMap.forComparers StringComparer.OrdinalIgnoreCase (SortedMap.valueComparer table)).ofSeq table
            ),
            "ofSeq of a struct from forComparers with the value comparer of the map must keep it"
        )

        Assert.AreSame (
            absoluteValues,
            table.WithComparers(StringComparer.OrdinalIgnoreCase).ValueComparer,
            "WithComparers with a key comparer must keep the value comparer"
        )

    [<TestMethod>]
    member _.``ofSeqBy compares the keys and the values with the comparers of the struct`` () =
        let comparers = SortedMap.forComparers StringComparer.OrdinalIgnoreCase byLength
        // The two keys differ only in their case, so that the key comparer of the struct makes them equal
        let projection (value : string) =
            if String.Equals (value, "one", StringComparison.Ordinal) then
                "key"
            else
                "KEY"

        Assert.AreEqual<int>(
            1,
            SortedMap.count (comparers.ofSeqBy projection [| "one"; "two" |]),
            "ofSeqBy must accept equal keys whose values are equal in the value comparer, as the BCL does"
        )

        Assert.ThrowsExactly<ArgumentException>(
            Action (fun () -> comparers.ofSeqBy projection [| "one"; "three" |] |> ignore),
            "ofSeqBy must reject equal keys whose values differ in the value comparer, as the BCL does"
        )
        |> ignore
