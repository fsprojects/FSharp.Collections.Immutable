namespace FSharp.Collections.Immutable.Tests

open System
open System.Collections.Generic
open FSharp.Collections.Immutable
open Microsoft.VisualStudio.TestTools.UnitTesting

/// <summary>
/// The functions that create a hash map from a span of key/value pairs, as the cases of the rows of their tests.
/// </summary>
/// <remarks>
/// A span is a byref-like value, so it cannot be a value of a row or be captured by a closure. A case builds the span of an
/// array inside its own member, for the module function and for the member of the comparer struct.
/// </remarks>
[<RequireQualifiedAccess>]
type SpanCreation =
    /// <c>ofReadOnlySpan</c>, which takes a <c>ReadOnlySpan</c>.
    | OfReadOnlySpan
    /// <c>ofSpan</c>, which takes a <c>Span</c>.
    | OfSpan

    /// Every function, as the rows of the tests that run once per function.
    static member All = seq {
        SpanCreation.OfReadOnlySpan
        SpanCreation.OfSpan
    }

    /// The name of the function, for the messages of the assertions.
    member this.Name =
        match this with
        | SpanCreation.OfReadOnlySpan -> nameof HashMap.ofReadOnlySpan
        | SpanCreation.OfSpan -> nameof HashMap.ofSpan

    /// Creates a hash map with the default comparers from the span of an array.
    member this.Create<'Key, 'Value when 'Key : not null> (pairs : KeyValuePair<'Key, 'Value> array) : HashMap<'Key, 'Value> =
        match this with
        | SpanCreation.OfReadOnlySpan -> HashMap.ofReadOnlySpan (ReadOnlySpan pairs)
        | SpanCreation.OfSpan -> HashMap.ofSpan (Span pairs)

    /// Creates a hash map with the default comparers from an array, which F# converts to the span because the types of the
    /// call are known; F# reports the conversion with FS3391, which is what the tests are about.
    member this.CreateFromArray (pairs : KeyValuePair<string, string> array) : HashMap<string, string> =
        #nowarn "3391"
        match this with
        | SpanCreation.OfReadOnlySpan -> HashMap.ofReadOnlySpan pairs
        | SpanCreation.OfSpan -> HashMap.ofSpan pairs
    #warnon "3391"

    /// Creates a hash map with the given comparers from the span of an array, with the member of the comparer struct.
    member this.CreateWith<'Key, 'Value when 'Key : not null>
        (comparers : CustomComparersHashMap<'Key, 'Value>, pairs : KeyValuePair<'Key, 'Value> array)
        : HashMap<'Key, 'Value>
        =
        match this with
        | SpanCreation.OfReadOnlySpan -> comparers.ofReadOnlySpan (ReadOnlySpan pairs)
        | SpanCreation.OfSpan -> comparers.ofSpan (Span pairs)

    /// Creates a hash map with the given comparers from an array, which F# converts to the span because the types of the
    /// struct are known.
    member this.CreateFromArrayWith
        (comparers : CustomComparersHashMap<string, string>, pairs : KeyValuePair<string, string> array)
        : HashMap<string, string>
        =
        match this with
        | SpanCreation.OfReadOnlySpan -> comparers.ofReadOnlySpan pairs
        | SpanCreation.OfSpan -> comparers.ofSpan pairs

/// The behaviour of `HashMap` that the shared contracts do not cover: null inputs, comparers, shared results and the
/// semantics of the functions that wrap the BCL.
[<TestClass; HashMapTestCategory>]
type HashMapTests () =

    static let nullTable = Unchecked.defaultof<HashMap<string, string>>

    /// A value comparer that differs from the default one: it compares strings by their length.
    static let byLength =
        HashIdentity.FromFunctions (fun (value : string) -> value.Length) (fun left right -> left.Length = right.Length)

    /// A map whose keys ignore the case and whose values compare by their length.
    static let customTable () =
        (HashMap.forComparers StringComparer.OrdinalIgnoreCase byLength).ofArray [| "one", "uno"; "Two", "dos"; "three", "tres" |]

    /// A map with the default comparers.
    static let defaultTable () = HashMap.ofArray [| struct ("one", "uno"); struct ("two", "dos") |]

    /// The functions that take a map, by name, on a null map. The rows of the tests are the names: a function has no
    /// readable name in a row.
    static let nullTableCalls : Map<string, unit -> unit> =
        Map.ofArray [|
            nameof HashMap.add, (fun () -> HashMap.add "a" "b" nullTable |> ignore)
            nameof HashMap.addRange,
            (fun () ->
                HashMap.addRange [| KeyValuePair ("a", "b") |] nullTable
                |> ignore
            )
            nameof HashMap.change, (fun () -> HashMap.change "a" id nullTable |> ignore)
            nameof HashMap.choose,
            (fun () ->
                HashMap.choose (fun _ value -> ValueSome value) nullTable
                |> ignore
            )
            nameof HashMap.clear, (fun () -> HashMap.clear nullTable |> ignore)
            nameof HashMap.containsBinding, (fun () -> HashMap.containsBinding "a" "b" nullTable |> ignore)
            nameof HashMap.containsKey, (fun () -> HashMap.containsKey "a" nullTable |> ignore)
            nameof HashMap.containsValue, (fun () -> HashMap.containsValue "a" nullTable |> ignore)
            nameof HashMap.count, (fun () -> HashMap.count nullTable |> ignore)
            nameof HashMap.exists, (fun () -> HashMap.exists (fun _ _ -> true) nullTable |> ignore)
            nameof HashMap.filter, (fun () -> HashMap.filter (fun _ _ -> true) nullTable |> ignore)
            nameof HashMap.find, (fun () -> HashMap.find "a" nullTable |> ignore)
            nameof HashMap.findKey, (fun () -> HashMap.findKey (fun _ _ -> true) nullTable |> ignore)
            nameof HashMap.fold, (fun () -> HashMap.fold (fun state _ _ -> state) 0 nullTable |> ignore)
            nameof HashMap.foldBack,
            (fun () ->
                HashMap.foldBack (fun _ _ state -> state) nullTable 0
                |> ignore
            )
            nameof HashMap.forall, (fun () -> HashMap.forall (fun _ _ -> true) nullTable |> ignore)
            nameof HashMap.isEmpty, (fun () -> HashMap.isEmpty nullTable |> ignore)
            nameof HashMap.iter, (fun () -> HashMap.iter (fun _ _ -> ()) nullTable)
            nameof HashMap.keyComparer, (fun () -> HashMap.keyComparer nullTable |> ignore)
            nameof HashMap.keys, (fun () -> HashMap.keys nullTable |> ignore)
            nameof HashMap.map, (fun () -> HashMap.map (fun _ value -> value) nullTable |> ignore)
            nameof HashMap.partition, (fun () -> HashMap.partition (fun _ _ -> true) nullTable |> ignore)
            nameof HashMap.pick,
            (fun () ->
                HashMap.pick (fun _ value -> ValueSome value) nullTable
                |> ignore
            )
            nameof HashMap.remove, (fun () -> HashMap.remove "a" nullTable |> ignore)
            nameof HashMap.removeRange, (fun () -> HashMap.removeRange [| "a" |] nullTable |> ignore)
            nameof HashMap.setItem, (fun () -> HashMap.setItem "a" "b" nullTable |> ignore)
            nameof HashMap.setItems,
            (fun () ->
                HashMap.setItems [| KeyValuePair ("a", "b") |] nullTable
                |> ignore
            )
            nameof HashMap.toArray, (fun () -> HashMap.toArray nullTable |> ignore)
            nameof HashMap.toBuilder, (fun () -> HashMap.toBuilder nullTable |> ignore)
            nameof HashMap.toList, (fun () -> HashMap.toList nullTable |> ignore)
            nameof HashMap.toSeq, (fun () -> HashMap.toSeq nullTable |> ignore)
            nameof HashMap.tryFind, (fun () -> HashMap.tryFind "a" nullTable |> ignore)
            nameof HashMap.tryFindKey, (fun () -> HashMap.tryFindKey (fun _ _ -> true) nullTable |> ignore)
            nameof HashMap.tryGetKey, (fun () -> HashMap.tryGetKey "a" nullTable |> ignore)
            nameof HashMap.tryPick,
            (fun () ->
                HashMap.tryPick (fun _ value -> ValueSome value) nullTable
                |> ignore
            )
            nameof HashMap.update, (fun () -> HashMap.update ignore nullTable |> ignore)
            nameof HashMap.valueComparer, (fun () -> HashMap.valueComparer nullTable |> ignore)
            nameof HashMap.values, (fun () -> HashMap.values nullTable |> ignore)
            "Tuple.addRange", (fun () -> HashMap.Tuple.addRange [| "a", "b" |] nullTable |> ignore)
            "Tuple.setItems", (fun () -> HashMap.Tuple.setItems [| "a", "b" |] nullTable |> ignore)
            "ValueTuple.addRange",
            (fun () ->
                HashMap.ValueTuple.addRange [| struct ("a", "b") |] nullTable
                |> ignore
            )
            "ValueTuple.setItems",
            (fun () ->
                HashMap.ValueTuple.setItems [| struct ("a", "b") |] nullTable
                |> ignore
            )
        |]

    /// The functions that take a sequence, by name, with the name of the sequence parameter and the function on a null
    /// sequence.
    static let nullSequenceCalls : Map<string, struct (string * (unit -> unit))> =
        let table = defaultTable ()

        Map.ofArray [|
            nameof HashMap.addRange,
            struct ("pairs",
                    fun () ->
                        HashMap.addRange (Unchecked.defaultof<seq<KeyValuePair<string, string>>>) table
                        |> ignore)
            "ofArray of key/value pairs",
            struct ("elements",
                    fun () ->
                        HashMap.ofArray (Unchecked.defaultof<KeyValuePair<string, string> array>)
                        |> ignore)
            "ofArray of reference tuples",
            struct ("elements",
                    fun () ->
                        HashMap.ofArray (Unchecked.defaultof<(string * string) array>)
                        |> ignore)
            "ofArray of struct tuples",
            struct ("elements",
                    fun () ->
                        HashMap.ofArray (Unchecked.defaultof<struct (string * string) array>)
                        |> ignore)
            "ofSeq of key/value pairs",
            struct ("elements",
                    fun () ->
                        HashMap.ofSeq (Unchecked.defaultof<seq<KeyValuePair<string, string>>>)
                        |> ignore)
            "ofSeq of reference tuples",
            struct ("elements",
                    fun () ->
                        HashMap.ofSeq (Unchecked.defaultof<seq<string * string>>)
                        |> ignore)
            "ofSeq of struct tuples",
            struct ("elements",
                    fun () ->
                        HashMap.ofSeq (Unchecked.defaultof<seq<struct (string * string)>>)
                        |> ignore)
            nameof HashMap.ofSeqBy,
            struct ("source",
                    fun () ->
                        HashMap.ofSeqBy id (Unchecked.defaultof<seq<string>>)
                        |> ignore)
            nameof HashMap.removeRange,
            struct ("keys",
                    fun () ->
                        HashMap.removeRange (Unchecked.defaultof<seq<string>>) table
                        |> ignore)
            nameof HashMap.setItems,
            struct ("pairs",
                    fun () ->
                        HashMap.setItems (Unchecked.defaultof<seq<KeyValuePair<string, string>>>) table
                        |> ignore)
            "Tuple.addRange",
            struct ("pairs",
                    fun () ->
                        HashMap.Tuple.addRange (Unchecked.defaultof<seq<string * string>>) table
                        |> ignore)
            "Tuple.setItems",
            struct ("pairs",
                    fun () ->
                        HashMap.Tuple.setItems (Unchecked.defaultof<seq<string * string>>) table
                        |> ignore)
            "ValueTuple.addRange",
            struct ("pairs",
                    fun () ->
                        HashMap.ValueTuple.addRange (Unchecked.defaultof<seq<struct (string * string)>>) table
                        |> ignore)
            "ValueTuple.setItems",
            struct ("pairs",
                    fun () ->
                        HashMap.ValueTuple.setItems (Unchecked.defaultof<seq<struct (string * string)>>) table
                        |> ignore)
        |]

    /// The functions that take a sequence, by name, with the name of the sequence parameter and the function on a default
    /// flat list that is given as the sequence. A default flat list converts to a sequence without a check and fails with
    /// InvalidOperationException when the sequence is enumerated, so the other inputs are valid and the function would
    /// enumerate it. The last rows give a sequence of objects, which a default flat list of strings is by the covariance of
    /// seq: the element type of the sequence is not the one of the flat list.
    static let defaultSequenceCalls : Map<string, struct (string * (unit -> unit))> =
        let table = defaultTable ()
        let objectTable = HashMap.ofArray [| KeyValuePair<obj, string>("one", "uno") |]
        let defaultPairs = Unchecked.defaultof<FlatList<KeyValuePair<string, string>>> :> seq<_>
        let defaultTuples = Unchecked.defaultof<FlatList<string * string>> :> seq<_>
        let defaultStructTuples = Unchecked.defaultof<FlatList<struct (string * string)>> :> seq<_>
        let defaultKeys = Unchecked.defaultof<FlatList<string>> :> seq<_>

        Map.ofArray [|
            nameof HashMap.addRange, struct ("pairs", fun () -> HashMap.addRange defaultPairs table |> ignore)
            "ofSeq of key/value pairs", struct ("elements", fun () -> HashMap.ofSeq defaultPairs |> ignore)
            "ofSeq of reference tuples", struct ("elements", fun () -> HashMap.ofSeq defaultTuples |> ignore)
            "ofSeq of struct tuples", struct ("elements", fun () -> HashMap.ofSeq defaultStructTuples |> ignore)
            nameof HashMap.ofSeqBy, struct ("source", fun () -> HashMap.ofSeqBy id defaultKeys |> ignore)
            nameof HashMap.removeRange, struct ("keys", fun () -> HashMap.removeRange defaultKeys table |> ignore)
            nameof HashMap.setItems, struct ("pairs", fun () -> HashMap.setItems defaultPairs table |> ignore)
            "Tuple.addRange", struct ("pairs", fun () -> HashMap.Tuple.addRange defaultTuples table |> ignore)
            "Tuple.setItems", struct ("pairs", fun () -> HashMap.Tuple.setItems defaultTuples table |> ignore)
            "ValueTuple.addRange",
            struct ("pairs",
                    fun () ->
                        HashMap.ValueTuple.addRange defaultStructTuples table
                        |> ignore)
            "ValueTuple.setItems",
            struct ("pairs",
                    fun () ->
                        HashMap.ValueTuple.setItems defaultStructTuples table
                        |> ignore)
            $"%s{nameof HashMap.ofSeqBy} (default flat list of strings as objects)",
            struct ("source",
                    fun () ->
                        HashMap.ofSeqBy id FlatListOracles.defaultStringsAsObjects
                        |> ignore)
            $"%s{nameof HashMap.removeRange} (default flat list of strings as objects)",
            struct ("keys",
                    fun () ->
                        HashMap.removeRange FlatListOracles.defaultStringsAsObjects objectTable
                        |> ignore)
        |]

    /// The creation functions without comparers, by name and shape of the bindings.
    static let defaultCreations : Map<string, unit -> HashMap<string, string>> =
        Map.ofArray [|
            nameof HashMap.empty, (fun () -> HashMap.empty)
            "ofArray of key/value pairs", (fun () -> HashMap.ofArray [| KeyValuePair ("a", "b") |])
            "ofArray of reference tuples", (fun () -> HashMap.ofArray [| ("a", "b") |])
            "ofArray of struct tuples", (fun () -> HashMap.ofArray [| struct ("a", "b") |])
            "ofList of key/value pairs", (fun () -> HashMap.ofList [ KeyValuePair ("a", "b") ])
            "ofList of reference tuples", (fun () -> HashMap.ofList [ ("a", "b") ])
            "ofList of struct tuples", (fun () -> HashMap.ofList [ struct ("a", "b") ])
            "ofSeq of key/value pairs", (fun () -> HashMap.ofSeq (dict [| "a", "b" |]))
            "ofSeq of reference tuples", (fun () -> HashMap.ofSeq (Seq.singleton ("a", "b")))
            "ofSeq of struct tuples", (fun () -> HashMap.ofSeq (Seq.singleton (struct ("a", "b"))))
            nameof HashMap.build, (fun () -> HashMap.build (fun builder -> builder["a"] <- "b"))
            nameof HashMap.ofReadOnlySpan, (fun () -> HashMap.ofReadOnlySpan (ReadOnlySpan [| KeyValuePair ("a", "b") |]))
            nameof HashMap.ofSeqBy, (fun () -> HashMap.ofSeqBy id [| "a" |])
            nameof HashMap.ofSpan, (fun () -> HashMap.ofSpan (Span [| KeyValuePair ("a", "b") |]))
            nameof HashMap.singleton, (fun () -> HashMap.singleton "a" "b")
        |]

    /// The transformations that keep the type of the values, by name, on the map whose keys ignore the case.
    static let sameTypeTransformations : Map<string, HashMap<string, string> -> HashMap<string, string>> =
        Map.ofArray [|
            nameof HashMap.add, HashMap.add "four" "cuatro"
            nameof HashMap.addRange, HashMap.addRange [| KeyValuePair ("four", "cuatro") |]
            nameof HashMap.change, HashMap.change "one" (fun _ -> ValueNone)
            nameof HashMap.clear, HashMap.clear
            nameof HashMap.filter, HashMap.filter (fun (key : string) _ -> key.Length > 3)
            ($"%s{nameof HashMap.partition} (accepted)",
             fun table ->
                 let struct (accepted, _) = HashMap.partition (fun (key : string) _ -> key.Length > 3) table
                 accepted)
            ($"%s{nameof HashMap.partition} (rejected)",
             fun table ->
                 let struct (_, rejected) = HashMap.partition (fun (key : string) _ -> key.Length > 3) table
                 rejected)
            nameof HashMap.remove, HashMap.remove "one"
            nameof HashMap.removeRange, HashMap.removeRange [| "one"; "two" |]
            nameof HashMap.setItem, HashMap.setItem "four" "cuatro"
            nameof HashMap.setItems, HashMap.setItems [| KeyValuePair ("four", "cuatro") |]
            nameof HashMap.update, HashMap.update (fun (builder : HashMap<string, string>.Builder) -> builder["four"] <- "cuatro")
            "Tuple.addRange", HashMap.Tuple.addRange [| "four", "cuatro" |]
            "Tuple.setItems", HashMap.Tuple.setItems [| "four", "cuatro" |]
            "ValueTuple.addRange", HashMap.ValueTuple.addRange [| struct ("four", "cuatro") |]
            "ValueTuple.setItems", HashMap.ValueTuple.setItems [| struct ("four", "cuatro") |]
        |]

    /// The transformations that change the type of the values, by name, on the map whose keys ignore the case.
    static let typeChangingTransformations : Map<string, HashMap<string, string> -> HashMap<string, int>> =
        Map.ofArray [|
            nameof HashMap.choose, HashMap.choose (fun _ (value : string) -> ValueSome value.Length)
            nameof HashMap.map, HashMap.map (fun _ (value : string) -> value.Length)
        |]

    /// The key comparer of the result of every transformation, by name.
    static let resultKeyComparers : Map<string, HashMap<string, string> -> IEqualityComparer<string>> =
        Map.ofSeq (
            seq {
                for KeyValue (name, transform) in sameTypeTransformations do
                    name, transform >> HashMap.keyComparer

                for KeyValue (name, transform) in typeChangingTransformations do
                    name, transform >> HashMap.keyComparer
            }
        )

    /// The functions that change nothing in the map with the default comparers, by name.
    static let unchangingCalls : Map<string, HashMap<string, string> -> HashMap<string, string>> =
        Map.ofArray [|
            nameof HashMap.add, HashMap.add "one" "uno"
            nameof HashMap.addRange, HashMap.addRange [| KeyValuePair ("one", "uno") |]
            $"%s{nameof HashMap.change} of a key with a binding", HashMap.change "one" id
            $"%s{nameof HashMap.change} of a key without a binding", HashMap.change "three" id
            nameof HashMap.filter, HashMap.filter (fun _ _ -> true)
            ($"%s{nameof HashMap.partition} (accepted)",
             fun table ->
                 let struct (accepted, _) = HashMap.partition (fun _ _ -> true) table
                 accepted)
            nameof HashMap.remove, HashMap.remove "three"
            nameof HashMap.removeRange, HashMap.removeRange [| "three" |]
            nameof HashMap.setItem, HashMap.setItem "two" "dos"
            nameof HashMap.setItems, HashMap.setItems [| KeyValuePair ("two", "dos") |]
            nameof HashMap.update, HashMap.update ignore
            "Tuple.addRange", HashMap.Tuple.addRange [| "one", "uno" |]
            "Tuple.setItems", HashMap.Tuple.setItems [| "two", "dos" |]
            "ValueTuple.addRange", HashMap.ValueTuple.addRange [| struct ("one", "uno") |]
            "ValueTuple.setItems", HashMap.ValueTuple.setItems [| struct ("two", "dos") |]
        |]

    /// The functions that bind "ONE" to "abc" in the map whose keys ignore the case and whose values compare by their
    /// length, by name: the map binds "one" to "uno", a value that the value comparer finds equal to "abc".
    static let equalValueCalls : Map<string, HashMap<string, string> -> HashMap<string, string>> =
        Map.ofArray [|
            nameof HashMap.add, HashMap.add "ONE" "abc"
            nameof HashMap.addRange, HashMap.addRange [| KeyValuePair ("ONE", "abc") |]
            nameof HashMap.change, HashMap.change "ONE" (fun _ -> ValueSome "abc")
            nameof HashMap.setItem, HashMap.setItem "ONE" "abc"
            nameof HashMap.setItems, HashMap.setItems [| KeyValuePair ("ONE", "abc") |]
            "Tuple.addRange", HashMap.Tuple.addRange [| "ONE", "abc" |]
            "Tuple.setItems", HashMap.Tuple.setItems [| "ONE", "abc" |]
            "ValueTuple.addRange", HashMap.ValueTuple.addRange [| struct ("ONE", "abc") |]
            "ValueTuple.setItems", HashMap.ValueTuple.setItems [| struct ("ONE", "abc") |]
        |]

    /// The names of the functions that take a map, as the rows of their test.
    static member NullTableCalls = nullTableCalls.Keys

    /// The names of the functions that take a sequence, as the rows of their test.
    static member NullSequenceCalls = nullSequenceCalls.Keys

    /// The names of the functions that take a sequence and are given a default flat list, as the rows of their test.
    static member DefaultSequenceCalls = defaultSequenceCalls.Keys

    /// The names of the creation functions without comparers, as the rows of their test.
    static member DefaultCreations = defaultCreations.Keys

    /// The names of the transformations that keep the type of the values, as the rows of their tests.
    static member SameTypeTransformations = sameTypeTransformations.Keys

    /// The names of the transformations that change the type of the values, as the rows of their test.
    static member TypeChangingTransformations = typeChangingTransformations.Keys

    /// The names of every transformation, as the rows of the test of the key comparer.
    static member Transformations = resultKeyComparers.Keys

    /// The names of the functions that change nothing, as the rows of their test.
    static member UnchangingCalls = unchangingCalls.Keys

    /// The names of the functions that set a value that the value comparer finds equal, as the rows of their test.
    static member EqualValueCalls = equalValueCalls.Keys

    /// Every input of the creation tests, the bindings that repeat keys included: the inputs of the span functions.
    static member SpanInputs = seq {
        yield! MapInputs.All
        MapInputs.RepeatedKeys
    }

    /// Every function that creates a hash map from a span with every input of the creation tests: the rows of the test of
    /// the bindings.
    static member SpanCreationsAndInputs = seq {
        for creation in SpanCreation.All do
            for input in HashMapTests.SpanInputs do
                struct (creation, input)
    }

    [<TestMethod; DynamicData(nameof HashMapTests.NullTableCalls)>]
    member _.``functions reject a null map`` (name : string) =
        // Map throws NullReferenceException for a null map; this library checks its arguments (ADR 0001, D6)
        let ex =
            Assert.ThrowsExactly<ArgumentNullException>(Action nullTableCalls[name], $"HashMap.%s{name} must reject a null map")

        Assert.AreEqual<string | null>("table", ex.ParamName, $"HashMap.%s{name} must name the map parameter")

    [<TestMethod; DynamicData(nameof HashMapTests.NullSequenceCalls)>]
    member _.``functions reject a null sequence`` (name : string) =
        let struct (paramName, call) = nullSequenceCalls[name]

        let ex =
            Assert.ThrowsExactly<ArgumentNullException>(Action call, $"HashMap.%s{name} must reject a null sequence")

        Assert.AreEqual<string | null>(paramName, ex.ParamName, $"HashMap.%s{name} must name the sequence parameter")

    [<TestMethod; DynamicData(nameof HashMapTests.DefaultSequenceCalls)>]
    member _.``functions reject a default flat list given as a sequence`` (name : string) =
        // The exception names the parameter of the function, not the one of the BCL member that it would have called
        let struct (paramName, call) = defaultSequenceCalls[name]
        FlatListAssert.RejectsNull ($"HashMap.%s{name}", paramName, call)

    [<TestMethod>]
    member _.``a flat list that is not default passes as a sequence, also one of strings as a sequence of objects`` () =
        let flatList (elements : 'T array) = FlatList.ofArray elements :> seq<'T>
        let table = defaultTable ()

        let bindingsOf (map : HashMap<string, string>) =
            HashMap.toArray map
            |> Array.sortBy (fun struct (key, _) -> key)

        let expected = [| struct ("four", "cuatro"); struct ("one", "uno"); struct ("two", "dos") |]

        CollectionAssert.AreEqual (
            expected,
            bindingsOf (HashMap.addRange (flatList [| KeyValuePair ("four", "cuatro") |]) table),
            "HashMap.addRange must take the elements of a flat list"
        )

        CollectionAssert.AreEqual (
            [| struct ("one", "uno"); struct ("two", "dos"); struct ("zero", "cero") |],
            bindingsOf (HashMap.setItems (flatList [| KeyValuePair ("zero", "cero") |]) table),
            "HashMap.setItems must take the elements of a flat list"
        )

        CollectionAssert.AreEqual (
            [| struct ("two", "dos") |],
            bindingsOf (HashMap.removeRange (flatList [| "one" |]) table),
            "HashMap.removeRange must take the elements of a flat list"
        )

        CollectionAssert.AreEqual (
            [| struct ("a", "b") |],
            bindingsOf (HashMap.ofSeq (flatList [| KeyValuePair ("a", "b") |])),
            "HashMap.ofSeq must take the key/value pairs of a flat list"
        )

        CollectionAssert.AreEqual (
            [| struct ("a", "b") |],
            bindingsOf (HashMap.ofSeq (flatList [| ("a", "b") |])),
            "HashMap.ofSeq must take the reference tuples of a flat list"
        )

        CollectionAssert.AreEqual (
            [| struct ("a", "b") |],
            bindingsOf (HashMap.ofSeq (flatList [| struct ("a", "b") |])),
            "HashMap.ofSeq must take the struct tuples of a flat list"
        )

        CollectionAssert.AreEqual (
            [| struct ("ab", "ab") |],
            bindingsOf (HashMap.ofSeqBy id (flatList [| "ab" |])),
            "HashMap.ofSeqBy must take the elements of a flat list"
        )

        // A flat list of strings is a sequence of objects, which a type test for the flat list of objects does not see
        let widened = FlatListOracles.stringsAsObjects (FlatList.ofArray [| "one" |])
        let objectTable =
            HashMap.ofArray [| KeyValuePair<obj, string>("one", "uno"); KeyValuePair<obj, string>("two", "dos") |]

        CollectionAssert.AreEqual (
            [| "two" :> obj |],
            HashMap.keys (HashMap.removeRange widened objectTable)
            |> Seq.toArray,
            "HashMap.removeRange must take the elements of a flat list of strings as objects"
        )

        CollectionAssert.AreEqual (
            [| "one" :> obj |],
            HashMap.keys (HashMap.ofSeqBy id widened) |> Seq.toArray,
            "HashMap.ofSeqBy must take the elements of a flat list of strings as objects"
        )

    [<TestMethod>]
    member _.``find throws the exception of Map.find, not the one of the indexer`` () =
        let table = defaultTable ()

        let ex =
            Assert.ThrowsExactly<KeyNotFoundException>(
                Action (fun () -> HashMap.find "three" table |> ignore),
                "HashMap.find must reject a key without a binding"
            )

        Assert.AreEqual<string>(
            KeyNotFoundException().Message,
            ex.Message,
            "HashMap.find must throw with the message of Map.find, which does not name the key"
        )

    [<TestMethod; DynamicData(nameof HashMapTests.DefaultCreations)>]
    member _.``the creation functions without comparers use the default comparers`` (name : string) =
        let table = defaultCreations[name]()

        Assert.AreSame<IEqualityComparer<string>>(
            EqualityComparer<string>.Default,
            HashMap.keyComparer table,
            $"HashMap.%s{name} must compare the keys with the default comparer"
        )

        Assert.AreSame<IEqualityComparer<string>>(
            EqualityComparer<string>.Default,
            HashMap.valueComparer table,
            $"HashMap.%s{name} must compare the values with the default comparer"
        )

    [<TestMethod>]
    member _.``ofSeqBy rejects a repeated key with another value and accepts a repeated element, as the BCL does`` () =
        Assert.ThrowsExactly<ArgumentException>(
            Action (fun () ->
                HashMap.ofSeqBy (fun (value : int) -> string (value % 2)) [| 1; 3 |]
                |> ignore
            ),
            "HashMap.ofSeqBy must reject a repeated key with another value, as ImmutableDictionary.ToImmutableDictionary does"
        )
        |> ignore

        let table = HashMap.ofSeqBy (fun (value : int) -> string value) [| 1; 1 |]
        Assert.AreEqual<int>(1, HashMap.count table, "HashMap.ofSeqBy must keep one binding of a repeated element")

    [<TestMethod>]
    member _.``ofSeqBy keeps the first of the elements of a key that the default comparer finds equal`` () =
        // ImmutableDictionary.ToImmutableDictionary keeps the first binding of a key for an equal value (ADR 0001, D14)
        let first = MapInputs.Copy "uno"
        let table = HashMap.ofSeqBy (fun (value : string) -> value.Length) [| first; MapInputs.Copy "uno" |]

        Assert.AreSame (
            first,
            HashMap.find 3 table,
            "HashMap.ofSeqBy must keep the first of the elements that the default comparer finds equal"
        )

    [<TestMethod; DynamicData(nameof HashMapTests.Transformations)>]
    member _.``the transformations keep the key comparer`` (name : string) =
        Assert.AreSame<IEqualityComparer<string>>(
            StringComparer.OrdinalIgnoreCase,
            resultKeyComparers[name](customTable ()),
            $"HashMap.%s{name} must keep the key comparer"
        )

    [<TestMethod; DynamicData(nameof HashMapTests.SameTypeTransformations)>]
    member _.``the transformations that keep the type of the values keep the value comparer`` (name : string) =
        Assert.AreSame (
            byLength,
            HashMap.valueComparer (sameTypeTransformations[name](customTable ())),
            $"HashMap.%s{name} must keep the value comparer"
        )

    [<TestMethod; DynamicData(nameof HashMapTests.TypeChangingTransformations)>]
    member _.``the transformations that change the type of the values give the new values the default comparer`` (name : string) =
        Assert.AreSame<IEqualityComparer<int>>(
            EqualityComparer<int>.Default,
            HashMap.valueComparer (typeChangingTransformations[name](customTable ())),
            $"HashMap.%s{name} must compare the new values with the default comparer"
        )

    [<TestMethod>]
    member _.``containsValue compares with the value comparer`` () =
        let table = customTable ()
        Assert.IsTrue (HashMap.containsValue "abc" table, "HashMap.containsValue must find a value of the same length")
        Assert.IsTrue (HashMap.containsValue "tres" table, "HashMap.containsValue must find an equal value")
        Assert.IsFalse (HashMap.containsValue "abcdef" table, "HashMap.containsValue must not find a value of another length")

    [<TestMethod>]
    member _.``tryGetKey returns the key that the map holds`` () =
        let table = customTable ()
        Assert.AreEqual<string voption>(
            ValueSome "Two",
            HashMap.tryGetKey "TWO" table,
            "HashMap.tryGetKey must return the stored key"
        )

        Assert.AreEqual<string voption>(
            ValueNone,
            HashMap.tryGetKey "four" table,
            "HashMap.tryGetKey must return nothing for a missing key"
        )

    [<TestMethod>]
    member _.``keys and values are read-only views that ask the map`` () =
        let table = customTable ()
        let keys = HashMap.keys table
        Assert.HasCount (3, keys, "HashMap.keys must have a key per binding")
        Assert.IsTrue (keys.Contains "THREE", "HashMap.keys must look the keys up with the key comparer")
        Assert.IsTrue (keys.IsReadOnly, "HashMap.keys must be read-only")

        Assert.ThrowsExactly<NotSupportedException>(Action (fun () -> keys.Add "four"), "HashMap.keys must reject a change")
        |> ignore

        let values = HashMap.values table
        Assert.HasCount (3, values, "HashMap.values must have a value per binding")
        Assert.IsTrue (values.Contains "xyz", "HashMap.values must look the values up with the value comparer")
        Assert.IsTrue (values.IsReadOnly, "HashMap.values must be read-only")

    [<TestMethod; DynamicData(nameof HashMapTests.UnchangingCalls)>]
    member _.``the functions that change nothing return the input itself`` (name : string) =
        let table = defaultTable ()
        Assert.AreSame (
            table,
            unchangingCalls[name] table,
            $"HashMap.%s{name} must return the input itself when it changes nothing"
        )

    [<TestMethod; DynamicData(nameof HashMapTests.EqualValueCalls)>]
    member _.``a value that the value comparer finds equal leaves the binding as it is`` (name : string) =
        // ImmutableDictionary.SetItem and AddRange keep the binding, its key included, for an equal value (ADR 0001, D14)
        let table = customTable ()
        let result = equalValueCalls[name] table
        Assert.AreEqual<string>("uno", HashMap.find "one" result, $"HashMap.%s{name} must keep the current value")

        Assert.AreEqual<string voption>(
            ValueSome "one",
            HashMap.tryGetKey "ONE" result,
            $"HashMap.%s{name} must keep the current key"
        )

        Assert.AreSame (table, result, $"HashMap.%s{name} must return the input itself")

    [<TestMethod>]
    member _.``clear returns an empty map itself`` () =
        let empty = HashMap.clear (defaultTable ())
        Assert.AreSame (empty, HashMap.clear empty, "HashMap.clear must return an empty input itself")

    [<TestMethod>]
    member _.``add throws what ImmutableDictionary.Add throws for a key that is bound to another value`` () =
        let table = defaultTable ()

        ExceptionAssert.Same (
            id,
            (fun () -> table.Add ("one", "other") |> ignore),
            (fun () -> HashMap.add "one" "other" table |> ignore),
            "HashMap.add must throw what ImmutableDictionary.Add throws"
        )

    [<TestMethod>]
    member _.``add rejects a value that the value comparer finds different`` () =
        // The comparer finds "uno" and "longer" different, as it finds "uno" and "abc" equal
        Assert.ThrowsExactly<ArgumentException>(
            Action (fun () -> HashMap.add "ONE" "longer" (customTable ()) |> ignore),
            "HashMap.add must reject a value that the value comparer finds different from the current one"
        )
        |> ignore

    [<TestMethod>]
    member _.``setItem replaces the value of a key that add rejects`` () =
        let table = defaultTable ()
        let replaced = HashMap.setItem "one" "other" table
        Assert.AreEqual<string>("other", HashMap.find "one" replaced, "HashMap.setItem must replace the value of the key")
        Assert.AreEqual<string>("uno", HashMap.find "one" table, "HashMap.setItem must not change the input")

    [<TestMethod>]
    member _.``containsBinding compares the value with the value comparer where tryFind and equality do not`` () =
        let table =
            (HashMap.forComparers StringComparer.Ordinal StringComparer.OrdinalIgnoreCase).ofArray [| "one", "Uno" |]

        Assert.IsTrue (
            HashMap.containsBinding "one" "UNO" table,
            "HashMap.containsBinding must find a value that the value comparer finds equal"
        )

        Assert.IsFalse (
            HashMap.tryFind "one" table
            |> ValueOption.exists (fun stored -> stored = "UNO"),
            "F# equality finds \"Uno\" and \"UNO\" different, which is why the function asks the value comparer"
        )

        Assert.IsFalse (HashMap.containsBinding "one" "DOS" table, "HashMap.containsBinding must not find another value")
        Assert.IsFalse (
            HashMap.containsBinding "ONE" "Uno" table,
            "HashMap.containsBinding must look the key up with the key comparer"
        )
        Assert.IsFalse (
            HashMap.containsBinding "two" "Uno" table,
            "HashMap.containsBinding must not find a key without a binding"
        )

    [<TestMethod; DataRow("ONE", "xyz", true); DataRow("ONE", "xy", false); DataRow("four", "xyz", false)>]
    member _.``containsBinding finds the key in the key comparer and the value in the value comparer``
        (key : string, value : string, expected : bool)
        =
        // The keys ignore the case and the values compare by their length: "one" is bound to "uno"
        Assert.AreEqual<bool>(
            expected,
            HashMap.containsBinding key value (customTable ()),
            "HashMap.containsBinding must compare the key and the value with the comparers of the map"
        )

    [<TestMethod>]
    member _.``containsBinding leaves a null key to the key comparer as ImmutableDictionary.Contains does`` () =
        let nullKey = Unchecked.defaultof<string>

        Assert.IsFalse (
            HashMap.containsBinding nullKey "uno" (defaultTable ()),
            "HashMap.containsBinding must give false for a null key with the default key comparer"
        )

        Assert.ThrowsExactly<ArgumentNullException>(
            Action (fun () -> HashMap.containsKey nullKey (defaultTable ()) |> ignore),
            "HashMap.containsKey must reject a null key, which HashMap.containsBinding leaves to the key comparer"
        )
        |> ignore

        Assert.ThrowsExactly<ArgumentNullException>(
            Action (fun () ->
                HashMap.containsBinding nullKey "uno" (customTable ())
                |> ignore
            ),
            "HashMap.containsBinding must throw for a null key when the key comparer rejects it"
        )
        |> ignore

    [<TestMethod; DynamicData(nameof HashMapTests.SpanCreationsAndInputs)>]
    member _.``the span functions create the bindings of a span as Map.ofSeq does``
        (creation : SpanCreation, input : struct (int * string) array)
        =
        let table = creation.Create (Array.map MapInputs.ToKeyValuePair input)

        CollectionAssert.AreEqual (
            MapInputs.OfMap (MapInputs.ToMap input),
            MapInputs.SortByKey (HashMap.toArray table),
            $"HashMap.%s{creation.Name} must keep the last binding of each key, as Map.ofSeq does"
        )

    [<TestMethod; DynamicData(nameof SpanCreation.All, typeof<SpanCreation>)>]
    member _.``the span functions copy the span`` (creation : SpanCreation) =
        let pairs = [| KeyValuePair ("one", "uno") |]
        let table = creation.Create pairs
        pairs[0] <- KeyValuePair ("one", "changed")
        Assert.AreEqual<string>("uno", HashMap.find "one" table, $"HashMap.%s{creation.Name} must copy the span")

    [<TestMethod; DynamicData(nameof SpanCreation.All, typeof<SpanCreation>)>]
    member _.``the span functions take an array when the types of the call are known`` (creation : SpanCreation) =
        let table = creation.CreateFromArray [| KeyValuePair ("one", "uno") |]

        CollectionAssert.AreEqual (
            [| struct ("one", "uno") |],
            HashMap.toArray table,
            $"HashMap.%s{creation.Name} must take an array when the types of the call are known"
        )

    [<TestMethod; DynamicData(nameof SpanCreation.All, typeof<SpanCreation>)>]
    member _.``the span functions keep the first of the bindings of a key with equal values`` (creation : SpanCreation) =
        // The default comparers find the copies equal; the first key and the first value stay (ADR 0001, D14)
        let firstKey = MapInputs.Copy "one"
        let firstValue = MapInputs.Copy "uno"

        let table =
            creation.Create [| KeyValuePair (firstKey, firstValue); KeyValuePair (MapInputs.Copy "one", MapInputs.Copy "uno") |]

        let struct (key, value) =
            Assert.ContainsSingle (HashMap.toArray table, $"HashMap.%s{creation.Name} must merge the bindings of equal keys")

        Assert.AreSame (firstKey, key, $"HashMap.%s{creation.Name} must keep the first key")
        Assert.AreSame (firstValue, value, $"HashMap.%s{creation.Name} must keep the first value")
