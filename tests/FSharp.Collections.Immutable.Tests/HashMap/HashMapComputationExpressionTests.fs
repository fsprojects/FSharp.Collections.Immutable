namespace FSharp.Collections.Immutable.Tests

open System
open System.Collections.Generic
open FSharp.Collections.Immutable
open Microsoft.VisualStudio.TestTools.UnitTesting

/// The `hashMap { }` computation expression.
[<TestClass; HashMapTestCategory; ComputationExpressionTestCategory>]
type HashMapComputationExpressionTests () =

    /// The bindings of a map of integer keys, sorted by key.
    static let bindings (table : HashMap<int, string>) = MapInputs.SortByKey (HashMap.toArray table)

    /// The expressions whose keys ignore the case and that bind "one" to the first value and then "ONE" to the second one,
    /// by the member that sets the second binding.
    static let equalValueExpressions : Map<string, string -> string -> HashMap<string, string>> =
        let ignoreCase = HashMapCE StringComparer.OrdinalIgnoreCase

        Map.ofArray [|
            "yield of a struct pair",
            (fun first second -> ignoreCase {
                yield struct ("one", first)
                yield struct ("ONE", second)
            })
            "yield of a key/value pair",
            (fun first second -> ignoreCase {
                yield struct ("one", first)
                yield KeyValuePair ("ONE", second)
            })
            "yield! of a sequence of struct pairs",
            (fun first second -> ignoreCase {
                yield struct ("one", first)
                yield! Seq.singleton (struct ("ONE", second))
            })
            "yield! of an array of struct pairs",
            (fun first second -> ignoreCase {
                yield struct ("one", first)
                yield! [| struct ("ONE", second) |]
            })
            "yield! of key/value pairs",
            (fun first second -> ignoreCase {
                yield struct ("one", first)
                yield! Seq.singleton (KeyValuePair ("ONE", second))
            })
            "yield! of a hash map",
            (fun first second -> ignoreCase {
                yield struct ("one", first)
                yield! HashMap.singleton "ONE" second
            })
        |]

    /// The expressions that take a sequence, by name, with the name of the sequence parameter and the expression on a null
    /// sequence. The expression rejects the sequence when the member runs, and not when the buffer does.
    static let nullSequenceExpressions : Map<string, struct (string * (unit -> unit))> =
        Map.ofArray [|
            "yield! of a sequence of struct pairs",
            struct ("pairs",
                    fun () ->
                        hashMap { yield! Unchecked.defaultof<seq<struct (int * string)>> }
                        |> ignore)
            "yield! of a sequence of key/value pairs",
            struct ("pairs",
                    fun () ->
                        hashMap { yield! Unchecked.defaultof<seq<KeyValuePair<int, string>>> }
                        |> ignore)
            "for",
            struct ("source",
                    fun () ->
                        hashMap {
                            for item in Unchecked.defaultof<seq<int>> do
                                yield struct (item, "x")
                        }
                        |> ignore)
        |]

    /// The expressions that take a sequence, by name, with the name of the sequence parameter and the expression on a
    /// default flat list that is given as the sequence. A default flat list converts to a sequence without a check and fails
    /// with InvalidOperationException when the sequence is enumerated. The last row gives a sequence of objects, which a
    /// default flat list of strings is by the covariance of seq: the element type of the sequence is not the one of the
    /// flat list.
    static let defaultSequenceExpressions : Map<string, struct (string * (unit -> unit))> =
        let defaultStructPairs = Unchecked.defaultof<FlatList<struct (int * string)>> :> seq<_>
        let defaultPairs = Unchecked.defaultof<FlatList<KeyValuePair<int, string>>> :> seq<_>
        let defaultKeys = Unchecked.defaultof<FlatList<int>> :> seq<_>

        Map.ofArray [|
            "yield! of a sequence of struct pairs", struct ("pairs", fun () -> hashMap { yield! defaultStructPairs } |> ignore)
            "yield! of a sequence of key/value pairs", struct ("pairs", fun () -> hashMap { yield! defaultPairs } |> ignore)
            "for",
            struct ("source",
                    fun () ->
                        hashMap {
                            for item in defaultKeys do
                                yield struct (item, "x")
                        }
                        |> ignore)
            "for (default flat list of strings as objects)",
            struct ("source",
                    fun () ->
                        hashMap {
                            for item in FlatListOracles.defaultStringsAsObjects do
                                yield struct (item, "x")
                        }
                        |> ignore)
        |]

    /// The expressions that take an array or a hash map, by name, with the name of the parameter and the expression on a null
    /// one. A null array or hash map is not a sequence for the member, which would fail with NullReferenceException when
    /// the buffer runs; the expression rejects it when the member runs.
    static let nullCollectionExpressions : Map<string, struct (string * (unit -> unit))> =
        Map.ofArray [|
            "yield! of an array of struct pairs",
            struct ("pairs",
                    fun () ->
                        hashMap { yield! Unchecked.defaultof<struct (int * string) array> }
                        |> ignore)
            "yield! of a hash map",
            struct ("table",
                    fun () ->
                        hashMap { yield! Unchecked.defaultof<HashMap<int, string>> }
                        |> ignore)
        |]

    /// The names of the members that set a binding, as the rows of the test of the values that the default comparer finds
    /// equal.
    static member EqualValueExpressions = equalValueExpressions.Keys

    /// The names of the expressions that take an array or a hash map and are given a null one, as the rows of their test.
    static member NullCollectionExpressions = nullCollectionExpressions.Keys

    /// The names of the expressions that take a sequence and are given a null one, as the rows of their test.
    static member NullSequenceExpressions = nullSequenceExpressions.Keys

    /// The names of the expressions that take a sequence and are given a default flat list, as the rows of their test.
    static member DefaultSequenceExpressions = defaultSequenceExpressions.Keys

    [<TestMethod>]
    member _.``an expression without bindings is empty`` () =
        let table : HashMap<int, string> = hashMap { () }
        Assert.IsTrue (HashMap.isEmpty table, "hashMap { () } must be empty")

    [<TestMethod>]
    member _.``yield sets struct pairs and key value pairs`` () =
        let table = hashMap {
            yield struct (1, "one")
            yield KeyValuePair (2, "two")
        }

        CollectionAssert.AreEqual (
            [| struct (1, "one"); struct (2, "two") |],
            bindings table,
            "hashMap must set the yielded bindings"
        )

    [<TestMethod>]
    member _.``implicit yields set the bindings`` () =
        let table = hashMap {
            struct (1, "one")
            struct (2, "two")
        }

        CollectionAssert.AreEqual (
            [| struct (1, "one"); struct (2, "two") |],
            bindings table,
            "hashMap must set the implicitly yielded bindings"
        )

    [<TestMethod>]
    member _.``a later binding of a key replaces an earlier one`` () =
        let table = hashMap {
            yield struct (1, "one")
            yield struct (1, "uno")
            yield KeyValuePair (1, "eins")
        }

        CollectionAssert.AreEqual ([| struct (1, "eins") |], bindings table, "hashMap must keep the last binding of a key")

    [<TestMethod; DynamicData(nameof HashMapComputationExpressionTests.EqualValueExpressions)>]
    member _.``a value that the default comparer finds equal keeps the earlier binding`` (expression : string) =
        // The indexer of the builder keeps the earlier binding, its key included (ADR 0001, D14); the copies are equal in
        // the default comparer of the values and tell the two values apart
        let first = MapInputs.Copy "uno"
        let table = equalValueExpressions[expression] first (MapInputs.Copy "uno")

        let struct (key, value) =
            Assert.ContainsSingle (HashMap.toArray table, "hashMap must merge the bindings of the equal keys")

        Assert.AreEqual<string>("one", key, "hashMap must keep the earlier key")
        Assert.AreSame (first, value, "hashMap must keep the earlier value")

    [<TestMethod>]
    member _.``yield! sets the bindings of sequences, arrays, maps and other expressions`` () =
        let inner = hashMap { yield struct (7, "seven") }
        let dictionary = Dictionary<int, string>()
        dictionary[6] <- "six"

        let table = hashMap {
            yield! [ struct (1, "one"); struct (2, "two") ]
            yield! [| struct (3, "three") |]
            yield! [ KeyValuePair (4, "four") ]
            yield! HashMap.singleton 5 "five"
            yield! dictionary
            yield! inner
        }

        CollectionAssert.AreEqual ([| 1..7 |], MapInputs.KeysOf (bindings table), "hashMap must set the bindings of every yield!")

    [<TestMethod>]
    member _.``for sets the bindings of the body for each item`` () =
        let table = hashMap {
            for item in 1..3 do
                yield struct (item, string (item * 10))
        }

        CollectionAssert.AreEqual (
            [| struct (1, "10"); struct (2, "20"); struct (3, "30") |],
            bindings table,
            "hashMap must run the body of for for each item"
        )

    [<TestMethod>]
    member _.``while sets the bindings while the guard holds`` () =
        let mutable count = 0

        let table = hashMap {
            while count < 3 do
                count <- count + 1
                yield struct (count, string count)
        }

        CollectionAssert.AreEqual (
            [| 1; 2; 3 |],
            MapInputs.KeysOf (bindings table),
            "hashMap must run the body of while while the guard holds"
        )

    [<TestMethod>]
    member _.``if without else and following bindings combine`` () =
        let table = hashMap {
            if false then
                yield struct (0, "zero")

            yield struct (1, "one")

            if true then
                yield struct (2, "two")
        }

        CollectionAssert.AreEqual (
            [| 1; 2 |],
            MapInputs.KeysOf (bindings table),
            "hashMap must combine conditional and plain bindings"
        )

    [<TestMethod>]
    member _.``try with sets the bindings of the handler after an exception`` () =
        let table = hashMap {
            try
                yield struct (1, "one")
                failwith "fails"
            with _ ->
                yield struct (2, "two")
        }

        CollectionAssert.AreEqual (
            [| 1; 2 |],
            MapInputs.KeysOf (bindings table),
            "hashMap must keep the bindings before the exception and add the handler's"
        )

    [<TestMethod>]
    member _.``try finally runs the compensation`` () =
        let mutable compensated = false

        let table = hashMap {
            try
                yield struct (1, "one")
            finally
                compensated <- true
        }

        Assert.IsTrue (compensated, "hashMap must run the compensation of try finally")
        CollectionAssert.AreEqual (
            [| 1 |],
            MapInputs.KeysOf (bindings table),
            "hashMap must keep the bindings of the body of try finally"
        )

    [<TestMethod>]
    member _.``use disposes the resource after the body`` () =
        let mutable disposed = false

        let table = hashMap {
            use _resource = {
                new IDisposable with
                    member _.Dispose () = disposed <- true
            }

            yield struct (1, "one")
        }

        Assert.IsTrue (disposed, "hashMap must dispose the resource of use")
        CollectionAssert.AreEqual (
            [| 1 |],
            MapInputs.KeysOf (bindings table),
            "hashMap must keep the bindings of the body of use"
        )

    [<TestMethod>]
    member _.``hashMap compares with the default comparers`` () =
        let table = hashMap { yield struct ("one", 1) }
        Assert.AreSame<IEqualityComparer<string>>(
            EqualityComparer<string>.Default,
            HashMap.keyComparer table,
            "hashMap must compare the keys with the default comparer"
        )
        Assert.AreSame<IEqualityComparer<int>>(
            EqualityComparer<int>.Default,
            HashMap.valueComparer table,
            "hashMap must compare the values with the default comparer"
        )

    [<TestMethod>]
    member _.``HashMapCE compares the keys with the comparer that it takes`` () =
        let ignoreCase = HashMapCE StringComparer.OrdinalIgnoreCase

        let table = ignoreCase {
            yield struct ("one", 1)
            yield struct ("ONE", 2)
            yield struct ("two", 3)
        }

        Assert.AreSame<IEqualityComparer<string>>(
            StringComparer.OrdinalIgnoreCase,
            HashMap.keyComparer table,
            "HashMapCE must keep the key comparer"
        )
        Assert.AreEqual<int>(2, HashMap.count table, "HashMapCE must merge the keys that the comparer considers equal")
        Assert.AreEqual<int>(2, HashMap.find "One" table, "HashMapCE must keep the last binding of equal keys")

    [<TestMethod; DynamicData(nameof HashMapComputationExpressionTests.NullSequenceExpressions)>]
    member _.``yield! and for reject a null sequence`` (name : string) =
        let struct (paramName, run) = nullSequenceExpressions[name]
        FlatListAssert.RejectsNull ($"hashMap %s{name}", paramName, run)

    [<TestMethod; DynamicData(nameof HashMapComputationExpressionTests.NullCollectionExpressions)>]
    member _.``yield! rejects a null array and a null hash map`` (name : string) =
        let struct (paramName, run) = nullCollectionExpressions[name]
        FlatListAssert.RejectsNull ($"hashMap %s{name}", paramName, run)

    [<TestMethod; DynamicData(nameof HashMapComputationExpressionTests.DefaultSequenceExpressions)>]
    member _.``yield! and for reject a default flat list given as a sequence`` (name : string) =
        // The member rejects it as the functions of HashMap do, when the expression runs it and not when the buffer does
        let struct (paramName, run) = defaultSequenceExpressions[name]
        FlatListAssert.RejectsNull ($"hashMap %s{name}", paramName, run)

    [<TestMethod>]
    member _.``yield! and for take a flat list that is not default as a sequence, also one of strings as objects`` () =
        let structPairs = FlatList.ofArray [| struct (1, "one"); struct (2, "two") |] :> seq<_>
        let pairs = FlatList.ofArray [| KeyValuePair (3, "three") |] :> seq<_>
        let keys = FlatList.ofArray [| 4 |] :> seq<_>

        let table = hashMap {
            yield! structPairs
            yield! pairs

            for key in keys do
                yield struct (key, "four")
        }

        CollectionAssert.AreEqual (
            [| struct (1, "one"); struct (2, "two"); struct (3, "three"); struct (4, "four") |],
            bindings table,
            "hashMap must take the elements of a flat list in yield! and for"
        )

        // A flat list of strings is a sequence of objects, which a type test for the flat list of objects does not see
        let widened = FlatListOracles.stringsAsObjects (FlatList.ofArray [| "a"; "b" |])

        let objectTable = hashMap {
            for item in widened do
                yield struct (item, 1)
        }

        CollectionAssert.AreEquivalent (
            [| "a" :> obj; "b" :> obj |],
            HashMap.keys objectTable |> Seq.toArray,
            "hashMap must take the elements of a flat list of strings as objects in for"
        )

    [<TestMethod>]
    member _.``HashMapCE rejects a null key comparer`` () =
        let ex =
            Assert.ThrowsExactly<ArgumentNullException>(
                Action (fun () ->
                    HashMapCE<string>(Unchecked.defaultof<IEqualityComparer<string>>)
                    |> ignore
                ),
                "HashMapCE must reject a null key comparer"
            )

        Assert.AreEqual<string | null>("keyComparer", ex.ParamName, "HashMapCE must name the key comparer")
