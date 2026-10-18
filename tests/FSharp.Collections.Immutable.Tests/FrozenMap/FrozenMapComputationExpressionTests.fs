namespace FSharp.Collections.Immutable.Tests

open System
open System.Collections.Generic
open FSharp.Collections.Immutable
open Microsoft.VisualStudio.TestTools.UnitTesting

/// The `frozenMap { }` computation expression.
[<TestClass; FrozenMapTestCategory; ComputationExpressionTestCategory>]
type FrozenMapComputationExpressionTests () =

    /// The bindings of a map of integer keys, sorted by key.
    static let bindings (table : FrozenMap<int, string>) = MapInputs.SortByKey (FrozenMap.toArray table)

    /// The expressions that bind a key to the first value and then a key that the default comparer finds equal to the second
    /// value, by the member that sets the second binding.
    static let equalKeyExpressions : Map<string, string -> string -> string -> FrozenMap<string, string>> =
        Map.ofArray [|
            "yield of a struct pair",
            (fun firstKey secondKey value -> frozenMap {
                yield struct (firstKey, "uno")
                yield struct (secondKey, value)
            })
            "yield of a key/value pair",
            (fun firstKey secondKey value -> frozenMap {
                yield struct (firstKey, "uno")
                yield KeyValuePair (secondKey, value)
            })
            "yield! of a sequence of struct pairs",
            (fun firstKey secondKey value -> frozenMap {
                yield struct (firstKey, "uno")
                yield! Seq.singleton (struct (secondKey, value))
            })
            "yield! of an array of struct pairs",
            (fun firstKey secondKey value -> frozenMap {
                yield struct (firstKey, "uno")
                yield! [| struct (secondKey, value) |]
            })
            "yield! of key/value pairs",
            (fun firstKey secondKey value -> frozenMap {
                yield struct (firstKey, "uno")
                yield! Seq.singleton (KeyValuePair (secondKey, value))
            })
            "yield! of a frozen map",
            (fun firstKey secondKey value -> frozenMap {
                yield struct (firstKey, "uno")
                yield! FrozenMap.singleton secondKey value
            })
        |]

    /// The expressions that take a sequence, by name, with the name of the sequence parameter and the expression on a null
    /// sequence. The expression rejects the sequence when the member runs, and not when the buffer does.
    static let nullSequenceExpressions : Map<string, struct (string * (unit -> unit))> =
        Map.ofArray [|
            "yield! of a sequence of struct pairs",
            struct ("pairs",
                    fun () ->
                        frozenMap { yield! Unchecked.defaultof<seq<struct (int * string)>> }
                        |> ignore)
            "yield! of a sequence of key/value pairs",
            struct ("pairs",
                    fun () ->
                        frozenMap { yield! Unchecked.defaultof<seq<KeyValuePair<int, string>>> }
                        |> ignore)
            "for",
            struct ("source",
                    fun () ->
                        frozenMap {
                            for item in Unchecked.defaultof<seq<int>> do
                                yield struct (item, "x")
                        }
                        |> ignore)
        |]

    /// The expressions that take an array or a frozen map, by name, with the name of the parameter and the expression on a
    /// null one. They are not sequences by type, so the contract of the sequences does not reach them. The expression
    /// rejects the input when the member runs, and not when the buffer enumerates it.
    static let nullCollectionExpressions : Map<string, struct (string * (unit -> unit))> =
        Map.ofArray [|
            "yield! of an array of struct pairs",
            struct ("pairs",
                    fun () ->
                        frozenMap { yield! Unchecked.defaultof<struct (int * string) array> }
                        |> ignore)
            "yield! of a frozen map",
            struct ("table",
                    fun () ->
                        frozenMap { yield! Unchecked.defaultof<FrozenMap<int, string>> }
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
            "yield! of a sequence of struct pairs", struct ("pairs", fun () -> frozenMap { yield! defaultStructPairs } |> ignore)
            "yield! of a sequence of key/value pairs", struct ("pairs", fun () -> frozenMap { yield! defaultPairs } |> ignore)
            "for",
            struct ("source",
                    fun () ->
                        frozenMap {
                            for item in defaultKeys do
                                yield struct (item, "x")
                        }
                        |> ignore)
            "for (default flat list of strings as objects)",
            struct ("source",
                    fun () ->
                        frozenMap {
                            for item in FlatListOracles.defaultStringsAsObjects do
                                yield struct (item, "x")
                        }
                        |> ignore)
        |]

    /// The names of the members that set a binding, as the rows of the test of the keys that the default comparer finds
    /// equal.
    static member EqualKeyExpressions = equalKeyExpressions.Keys

    /// The names of the expressions that take a sequence and are given a null one, as the rows of their test.
    static member NullSequenceExpressions = nullSequenceExpressions.Keys

    /// The names of the expressions that take a sequence and are given a default flat list, as the rows of their test.
    static member DefaultSequenceExpressions = defaultSequenceExpressions.Keys

    /// The names of the expressions that take an array or a frozen map and are given a null one, as the rows of their test.
    static member NullCollectionExpressions = nullCollectionExpressions.Keys

    [<TestMethod>]
    member _.``an expression without bindings is empty`` () =
        let table : FrozenMap<int, string> = frozenMap { () }
        Assert.IsTrue (FrozenMap.isEmpty table, "frozenMap { () } must be empty")

    [<TestMethod>]
    member _.``yield sets struct pairs and key value pairs`` () =
        let table = frozenMap {
            yield struct (1, "one")
            yield KeyValuePair (2, "two")
        }

        CollectionAssert.AreEqual (
            [| struct (1, "one"); struct (2, "two") |],
            bindings table,
            "frozenMap must set the yielded bindings"
        )

    [<TestMethod>]
    member _.``implicit yields set the bindings`` () =
        let table = frozenMap {
            struct (1, "one")
            struct (2, "two")
        }

        CollectionAssert.AreEqual (
            [| struct (1, "one"); struct (2, "two") |],
            bindings table,
            "frozenMap must set the implicitly yielded bindings"
        )

    [<TestMethod>]
    member _.``a later binding of a key replaces an earlier one`` () =
        let table = frozenMap {
            yield struct (1, "one")
            yield struct (1, "uno")
            yield KeyValuePair (1, "eins")
        }

        CollectionAssert.AreEqual ([| struct (1, "eins") |], bindings table, "frozenMap must keep the last binding of a key")

    [<TestMethod; DynamicData(nameof FrozenMapComputationExpressionTests.EqualKeyExpressions)>]
    member _.``a key that the default comparer finds equal keeps the earlier key and takes the later value``
        (expression : string)
        =
        // The indexer of the dictionary keeps the earlier key and replaces the value, and ToFrozenDictionary freezes both
        // (ADR 0001, D14); the copies are equal in the default comparer and tell the two keys apart
        let firstKey = MapInputs.Copy "one"
        let value = MapInputs.Copy "dos"
        let table = equalKeyExpressions[expression] firstKey (MapInputs.Copy "one") value

        let struct (key, kept) =
            Assert.ContainsSingle (FrozenMap.toArray table, "frozenMap must merge the bindings of the equal keys")

        Assert.AreSame (firstKey, key, "frozenMap must keep the earlier key")
        Assert.AreSame (value, kept, "frozenMap must take the later value")

    [<TestMethod; DynamicData(nameof FrozenMapComputationExpressionTests.NullSequenceExpressions)>]
    member _.``yield! and for reject a null sequence`` (name : string) =
        let struct (paramName, run) = nullSequenceExpressions[name]
        FlatListAssert.RejectsNull ($"frozenMap %s{name}", paramName, run)

    [<TestMethod; DynamicData(nameof FrozenMapComputationExpressionTests.DefaultSequenceExpressions)>]
    member _.``yield! and for reject a default flat list given as a sequence`` (name : string) =
        // The member rejects it as the functions of FrozenMap do, when the expression runs it and not when the buffer does
        let struct (paramName, run) = defaultSequenceExpressions[name]
        FlatListAssert.RejectsNull ($"frozenMap %s{name}", paramName, run)

    [<TestMethod; DynamicData(nameof FrozenMapComputationExpressionTests.NullCollectionExpressions)>]
    member _.``yield! rejects a null array and a null frozen map`` (name : string) =
        // A null array or frozen map fails with NullReferenceException when the buffer enumerates it; the member rejects it
        // as it does a null sequence, when the expression runs it
        let struct (paramName, run) = nullCollectionExpressions[name]
        FlatListAssert.RejectsNull ($"frozenMap %s{name}", paramName, run)

    [<TestMethod>]
    member _.``yield! and for take a flat list that is not default as a sequence, also one of strings as objects`` () =
        let structPairs = FlatList.ofArray [| struct (1, "one"); struct (2, "two") |] :> seq<_>
        let pairs = FlatList.ofArray [| KeyValuePair (3, "three") |] :> seq<_>
        let keys = FlatList.ofArray [| 4 |] :> seq<_>

        let table = frozenMap {
            yield! structPairs
            yield! pairs

            for key in keys do
                yield struct (key, "four")
        }

        CollectionAssert.AreEqual (
            [| struct (1, "one"); struct (2, "two"); struct (3, "three"); struct (4, "four") |],
            bindings table,
            "frozenMap must take the elements of a flat list in yield! and for"
        )

        // A flat list of strings is a sequence of objects, which a type test for the flat list of objects does not see
        let widened = FlatListOracles.stringsAsObjects (FlatList.ofArray [| "a"; "b" |])

        let objectTable = frozenMap {
            for item in widened do
                yield struct (item, 1)
        }

        CollectionAssert.AreEquivalent (
            [| "a" :> obj; "b" :> obj |],
            FrozenMap.keys objectTable |> Seq.toArray,
            "frozenMap must take the elements of a flat list of strings as objects in for"
        )

    [<TestMethod>]
    member _.``yield! sets the bindings of sequences, arrays, maps and other expressions`` () =
        let inner = frozenMap { yield struct (7, "seven") }
        let dictionary = Dictionary<int, string>()
        dictionary[6] <- "six"

        let table = frozenMap {
            yield! [ struct (1, "one"); struct (2, "two") ]
            yield! [| struct (3, "three") |]
            yield! [ KeyValuePair (4, "four") ]
            yield! FrozenMap.singleton 5 "five"
            yield! dictionary
            yield! inner
            yield! HashMap.singleton 8 "eight"
        }

        CollectionAssert.AreEqual (
            [| 1..8 |],
            MapInputs.KeysOf (bindings table),
            "frozenMap must set the bindings of every yield!"
        )

    [<TestMethod>]
    member _.``for sets the bindings of the body for each item`` () =
        let table = frozenMap {
            for item in 1..3 do
                yield struct (item, string (item * 10))
        }

        CollectionAssert.AreEqual (
            [| struct (1, "10"); struct (2, "20"); struct (3, "30") |],
            bindings table,
            "frozenMap must run the body of for for each item"
        )

    [<TestMethod>]
    member _.``while sets the bindings while the guard holds`` () =
        let mutable count = 0

        let table = frozenMap {
            while count < 3 do
                count <- count + 1
                yield struct (count, string count)
        }

        CollectionAssert.AreEqual (
            [| 1; 2; 3 |],
            MapInputs.KeysOf (bindings table),
            "frozenMap must run the body of while while the guard holds"
        )

    [<TestMethod>]
    member _.``if without else and following bindings combine`` () =
        let table = frozenMap {
            if false then
                yield struct (0, "zero")

            yield struct (1, "one")

            if true then
                yield struct (2, "two")
        }

        CollectionAssert.AreEqual (
            [| 1; 2 |],
            MapInputs.KeysOf (bindings table),
            "frozenMap must combine conditional and plain bindings"
        )

    [<TestMethod>]
    member _.``try with sets the bindings of the handler after an exception`` () =
        let table = frozenMap {
            try
                yield struct (1, "one")
                failwith "fails"
            with _ ->
                yield struct (2, "two")
        }

        CollectionAssert.AreEqual (
            [| 1; 2 |],
            MapInputs.KeysOf (bindings table),
            "frozenMap must keep the bindings before the exception and add the handler's"
        )

    [<TestMethod>]
    member _.``try finally runs the compensation`` () =
        let mutable compensated = false

        let table = frozenMap {
            try
                yield struct (1, "one")
            finally
                compensated <- true
        }

        Assert.IsTrue (compensated, "frozenMap must run the compensation of try finally")

        CollectionAssert.AreEqual (
            [| 1 |],
            MapInputs.KeysOf (bindings table),
            "frozenMap must keep the bindings of the body of try finally"
        )

    [<TestMethod>]
    member _.``use disposes the resource after the body`` () =
        let mutable disposed = false

        let table = frozenMap {
            use _resource = {
                new IDisposable with
                    member _.Dispose () = disposed <- true
            }

            yield struct (1, "one")
        }

        Assert.IsTrue (disposed, "frozenMap must dispose the resource of use")

        CollectionAssert.AreEqual (
            [| 1 |],
            MapInputs.KeysOf (bindings table),
            "frozenMap must keep the bindings of the body of use"
        )

    [<TestMethod>]
    member _.``frozenMap compares the keys with the default comparer`` () =
        let table = frozenMap { yield struct ("one", 1) }

        Assert.AreSame<IEqualityComparer<string>>(
            EqualityComparer<string>.Default,
            FrozenMap.keyComparer table,
            "frozenMap must compare the keys with the default comparer"
        )

    [<TestMethod>]
    member _.``FrozenMapCE compares the keys with the comparer that it takes`` () =
        let ignoreCase = FrozenMapCE StringComparer.OrdinalIgnoreCase

        let table = ignoreCase {
            yield struct ("one", 1)
            yield struct ("ONE", 2)
            yield struct ("two", 3)
        }

        Assert.AreSame<IEqualityComparer<string>>(
            StringComparer.OrdinalIgnoreCase,
            FrozenMap.keyComparer table,
            "FrozenMapCE must keep the key comparer"
        )

        Assert.AreEqual<int>(2, FrozenMap.count table, "FrozenMapCE must merge the keys that the comparer considers equal")
        Assert.AreEqual<int>(2, FrozenMap.find "One" table, "FrozenMapCE must keep the last value of equal keys")

        CollectionAssert.AreEquivalent (
            [| "one"; "two" |],
            FlatList.toArray (FrozenMap.keys table),
            "FrozenMapCE must keep the first of equal keys"
        )

    [<TestMethod>]
    member _.``FrozenMapCE rejects a null key comparer`` () =
        let ex =
            Assert.ThrowsExactly<ArgumentNullException>(
                Action (fun () ->
                    FrozenMapCE<string>(Unchecked.defaultof<IEqualityComparer<string>>)
                    |> ignore
                ),
                "FrozenMapCE must reject a null key comparer"
            )

        Assert.AreEqual<string | null>("keyComparer", ex.ParamName, "FrozenMapCE must name the key comparer")
