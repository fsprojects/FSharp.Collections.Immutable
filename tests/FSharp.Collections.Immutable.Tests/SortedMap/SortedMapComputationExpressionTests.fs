namespace FSharp.Collections.Immutable.Tests

open System
open System.Collections.Generic
open FSharp.Collections.Immutable
open Microsoft.VisualStudio.TestTools.UnitTesting

/// The `sortedMap { }` computation expression.
[<TestClass; SortedMapTestCategory; ComputationExpressionTestCategory>]
type SortedMapComputationExpressionTests () =

    /// The expressions whose keys ignore the case and that bind "one" to the first value and then "ONE" to the second one,
    /// by the member that sets the second binding.
    static let equalValueExpressions : Map<string, string -> string -> SortedMap<string, string>> =
        let ignoreCase = SortedMapCE StringComparer.OrdinalIgnoreCase

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
            "yield! of a sorted map",
            (fun first second -> ignoreCase {
                yield struct ("one", first)
                yield! SortedMap.singleton "ONE" second
            })
        |]

    /// The expressions that take a sequence, by name, with the name of the sequence parameter and the expression on a null
    /// sequence. The expression rejects the sequence when the member runs, and not when the buffer does.
    static let nullSequenceExpressions : Map<string, struct (string * (unit -> unit))> =
        Map.ofArray [|
            "yield! of a sequence of struct pairs",
            struct ("pairs",
                    fun () ->
                        sortedMap { yield! Unchecked.defaultof<seq<struct (int * string)>> }
                        |> ignore)
            "yield! of a sequence of key/value pairs",
            struct ("pairs",
                    fun () ->
                        sortedMap { yield! Unchecked.defaultof<seq<KeyValuePair<int, string>>> }
                        |> ignore)
            "for",
            struct ("source",
                    fun () ->
                        sortedMap {
                            for item in Unchecked.defaultof<seq<int>> do
                                yield struct (item, "x")
                        }
                        |> ignore)
        |]

    /// The expressions that take an array or a sorted map, by name, with the name of the parameter and the expression on a
    /// null one. A null array or sorted map is not a sequence for the member, which would fail with NullReferenceException
    /// when the buffer runs; the expression rejects it when the member runs.
    static let nullCollectionExpressions : Map<string, struct (string * (unit -> unit))> =
        Map.ofArray [|
            "yield! of an array of struct pairs",
            struct ("pairs",
                    fun () ->
                        sortedMap { yield! Unchecked.defaultof<struct (int * string) array> }
                        |> ignore)
            "yield! of a sorted map",
            struct ("table",
                    fun () ->
                        sortedMap { yield! Unchecked.defaultof<SortedMap<int, string>> }
                        |> ignore)
        |]

    /// The expressions that take a sequence, by name, with the name of the sequence parameter and the expression on a
    /// default flat list that is given as the sequence. A default flat list converts to a sequence without a check and fails
    /// with InvalidOperationException when the sequence is enumerated. The last row gives a sequence of objects, which a
    /// default flat list of strings is by the covariance of seq: the element type of the sequence is not the one of the
    /// flat list. The keys of the map are not the objects, because the expression needs keys with comparison.
    static let defaultSequenceExpressions : Map<string, struct (string * (unit -> unit))> =
        let defaultStructPairs = Unchecked.defaultof<FlatList<struct (int * string)>> :> seq<_>
        let defaultPairs = Unchecked.defaultof<FlatList<KeyValuePair<int, string>>> :> seq<_>
        let defaultKeys = Unchecked.defaultof<FlatList<int>> :> seq<_>

        Map.ofArray [|
            "yield! of a sequence of struct pairs", struct ("pairs", fun () -> sortedMap { yield! defaultStructPairs } |> ignore)
            "yield! of a sequence of key/value pairs", struct ("pairs", fun () -> sortedMap { yield! defaultPairs } |> ignore)
            "for",
            struct ("source",
                    fun () ->
                        sortedMap {
                            for item in defaultKeys do
                                yield struct (item, "x")
                        }
                        |> ignore)
            "for (default flat list of strings as objects)",
            struct ("source",
                    fun () ->
                        sortedMap {
                            for _ in FlatListOracles.defaultStringsAsObjects do
                                yield struct (1, "x")
                        }
                        |> ignore)
        |]

    /// The names of the members that set a binding, as the rows of the test of the values that the default comparer finds
    /// equal.
    static member EqualValueExpressions = equalValueExpressions.Keys

    /// The names of the expressions that take an array or a sorted map and are given a null one, as the rows of their test.
    static member NullCollectionExpressions = nullCollectionExpressions.Keys

    /// The names of the expressions that take a sequence and are given a null one, as the rows of their test.
    static member NullSequenceExpressions = nullSequenceExpressions.Keys

    /// The names of the expressions that take a sequence and are given a default flat list, as the rows of their test.
    static member DefaultSequenceExpressions = defaultSequenceExpressions.Keys

    [<TestMethod>]
    member _.``an expression without bindings is empty`` () =
        let table : SortedMap<int, string> = sortedMap { () }
        Assert.IsTrue (SortedMap.isEmpty table, "sortedMap { () } must be empty")

    [<TestMethod>]
    member _.``yield sets struct pairs and key value pairs`` () =
        let table = sortedMap {
            yield struct (2, "two")
            yield KeyValuePair (1, "one")
        }

        CollectionAssert.AreEqual (
            [| struct (1, "one"); struct (2, "two") |],
            SortedMap.toArray table,
            "sortedMap must set the yielded bindings in the order of the keys"
        )

    [<TestMethod>]
    member _.``implicit yields set the bindings`` () =
        let table = sortedMap {
            struct (2, "two")
            struct (1, "one")
        }

        CollectionAssert.AreEqual (
            [| struct (1, "one"); struct (2, "two") |],
            SortedMap.toArray table,
            "sortedMap must set the implicitly yielded bindings in the order of the keys"
        )

    [<TestMethod>]
    member _.``a later binding of a key replaces an earlier one`` () =
        let table = sortedMap {
            yield struct (1, "one")
            yield struct (1, "uno")
            yield KeyValuePair (1, "eins")
        }

        CollectionAssert.AreEqual (
            [| struct (1, "eins") |],
            SortedMap.toArray table,
            "sortedMap must keep the last binding of a key"
        )

    [<TestMethod; DynamicData(nameof SortedMapComputationExpressionTests.EqualValueExpressions)>]
    member _.``a value that the default comparer finds equal keeps the earlier binding`` (expression : string) =
        // The indexer of the builder keeps the earlier binding, its key included (ADR 0001, D14); the copies are equal in
        // the default comparer of the values and tell the two values apart
        let first = MapInputs.Copy "uno"
        let table = equalValueExpressions[expression] first (MapInputs.Copy "uno")

        let struct (key, value) =
            Assert.ContainsSingle (SortedMap.toArray table, "sortedMap must merge the bindings of the equal keys")

        Assert.AreEqual<string>("one", key, "sortedMap must keep the earlier key")
        Assert.AreSame (first, value, "sortedMap must keep the earlier value")

    [<TestMethod>]
    member _.``yield! sets the bindings of sequences, arrays, maps and other expressions`` () =
        let inner = sortedMap { yield struct (7, "seven") }
        let dictionary = Dictionary<int, string>()
        dictionary[6] <- "six"

        let table = sortedMap {
            yield! inner
            yield! [ struct (2, "two"); struct (1, "one") ]
            yield! [| struct (3, "three") |]
            yield! [ KeyValuePair (4, "four") ]
            yield! SortedMap.singleton 5 "five"
            yield! dictionary
        }

        CollectionAssert.AreEqual (
            [| 1..7 |],
            Seq.toArray (SortedMap.keys table),
            "sortedMap must set the bindings of every yield! in the order of the keys"
        )

    [<TestMethod>]
    member _.``for sets the bindings of the body for each item`` () =
        let table = sortedMap {
            for item in 3..-1..1 do
                yield struct (item, string (item * 10))
        }

        CollectionAssert.AreEqual (
            [| struct (1, "10"); struct (2, "20"); struct (3, "30") |],
            SortedMap.toArray table,
            "sortedMap must run the body of for for each item"
        )

    [<TestMethod>]
    member _.``while sets the bindings while the guard holds`` () =
        let mutable count = 0

        let table = sortedMap {
            while count < 3 do
                count <- count + 1
                yield struct (-count, string count)
        }

        CollectionAssert.AreEqual (
            [| -3; -2; -1 |],
            Seq.toArray (SortedMap.keys table),
            "sortedMap must run the body of while while the guard holds"
        )

    [<TestMethod>]
    member _.``if without else and following bindings combine`` () =
        let table = sortedMap {
            if false then
                yield struct (0, "zero")

            yield struct (1, "one")

            if true then
                yield struct (2, "two")
        }

        CollectionAssert.AreEqual (
            [| 1; 2 |],
            Seq.toArray (SortedMap.keys table),
            "sortedMap must combine conditional and plain bindings"
        )

    [<TestMethod>]
    member _.``try with sets the bindings of the handler after an exception`` () =
        let table = sortedMap {
            try
                yield struct (1, "one")
                failwith "fails"
            with _ ->
                yield struct (2, "two")
        }

        CollectionAssert.AreEqual (
            [| 1; 2 |],
            Seq.toArray (SortedMap.keys table),
            "sortedMap must keep the bindings before the exception and add the handler's"
        )

    [<TestMethod>]
    member _.``try finally runs the compensation`` () =
        let mutable compensated = false

        let table = sortedMap {
            try
                yield struct (1, "one")
            finally
                compensated <- true
        }

        Assert.IsTrue (compensated, "sortedMap must run the compensation of try finally")

        CollectionAssert.AreEqual (
            [| 1 |],
            Seq.toArray (SortedMap.keys table),
            "sortedMap must keep the bindings of the body of try finally"
        )

    [<TestMethod>]
    member _.``use disposes the resource after the body`` () =
        let mutable disposed = false

        let table = sortedMap {
            use _resource = {
                new IDisposable with
                    member _.Dispose () = disposed <- true
            }

            yield struct (1, "one")
        }

        Assert.IsTrue (disposed, "sortedMap must dispose the resource of use")

        CollectionAssert.AreEqual (
            [| 1 |],
            Seq.toArray (SortedMap.keys table),
            "sortedMap must keep the bindings of the body of use"
        )

    [<TestMethod>]
    member _.``sortedMap orders the keys with F# generic comparison`` () =
        let table = sortedMap {
            for key in [| "b"; "a"; "B"; "A"; "_" |] do
                yield struct (key, key.Length)
        }

        CollectionAssert.AreEqual (
            [| "A"; "B"; "_"; "a"; "b" |],
            Seq.toArray (SortedMap.keys table),
            "sortedMap must order the keys ordinally, as Map does"
        )

        Assert.AreSame<IComparer<string>>(
            LanguagePrimitives.FastGenericComparer<string>,
            SortedMap.keyComparer table,
            "sortedMap must order the keys with F# generic comparison"
        )

        Assert.AreSame<IEqualityComparer<int>>(
            EqualityComparer<int>.Default,
            SortedMap.valueComparer table,
            "sortedMap must compare the values with the default comparer"
        )

    [<TestMethod>]
    member _.``SortedMapCE without a comparer orders the keys with F# generic comparison`` () =
        let ordinal = SortedMapCE<string>()
        let table = ordinal { yield struct ("b", 1) }

        Assert.AreSame<IComparer<string>>(
            LanguagePrimitives.FastGenericComparer<string>,
            SortedMap.keyComparer table,
            "SortedMapCE () must order the keys with F# generic comparison"
        )

    [<TestMethod>]
    member _.``SortedMapCE orders the keys with the comparer that it takes`` () =
        let ignoreCase = SortedMapCE StringComparer.OrdinalIgnoreCase

        // Ordinally, "B" and "C" come before "a"; ignoring the case, "a" comes first
        let table = ignoreCase {
            yield struct ("B", 1)
            yield struct ("a", 2)
            yield struct ("C", 3)
            yield struct ("A", 4)
        }

        Assert.AreSame<IComparer<string>>(
            StringComparer.OrdinalIgnoreCase,
            SortedMap.keyComparer table,
            "SortedMapCE must keep the key comparer"
        )

        CollectionAssert.AreEqual (
            [| 4; 1; 3 |],
            SortedMap.toArray table
            |> Array.map (fun struct (_, value) -> value),
            "SortedMapCE must order the keys with the comparer"
        )

        Assert.AreEqual<int>(3, SortedMap.count table, "SortedMapCE must merge the keys that the comparer considers equal")
        Assert.AreEqual<int>(4, SortedMap.find "a" table, "SortedMapCE must keep the last binding of equal keys")

    [<TestMethod; DynamicData(nameof SortedMapComputationExpressionTests.NullSequenceExpressions)>]
    member _.``yield! and for reject a null sequence`` (name : string) =
        let struct (paramName, run) = nullSequenceExpressions[name]
        FlatListAssert.RejectsNull ($"sortedMap %s{name}", paramName, run)

    [<TestMethod; DynamicData(nameof SortedMapComputationExpressionTests.NullCollectionExpressions)>]
    member _.``yield! rejects a null array and a null sorted map`` (name : string) =
        let struct (paramName, run) = nullCollectionExpressions[name]
        FlatListAssert.RejectsNull ($"sortedMap %s{name}", paramName, run)

    [<TestMethod; DynamicData(nameof SortedMapComputationExpressionTests.DefaultSequenceExpressions)>]
    member _.``yield! and for reject a default flat list given as a sequence`` (name : string) =
        // The member rejects it as the functions of SortedMap do, when the expression runs it and not when the buffer does
        let struct (paramName, run) = defaultSequenceExpressions[name]
        FlatListAssert.RejectsNull ($"sortedMap %s{name}", paramName, run)

    [<TestMethod>]
    member _.``yield! and for take a flat list that is not default as a sequence, also one of strings as objects`` () =
        let structPairs = FlatList.ofArray [| struct (1, "one"); struct (2, "two") |] :> seq<_>
        let pairs = FlatList.ofArray [| KeyValuePair (3, "three") |] :> seq<_>
        let keys = FlatList.ofArray [| 4 |] :> seq<_>

        let table = sortedMap {
            yield! structPairs
            yield! pairs

            for key in keys do
                yield struct (key, "four")
        }

        CollectionAssert.AreEqual (
            [| struct (1, "one"); struct (2, "two"); struct (3, "three"); struct (4, "four") |],
            SortedMap.toArray table,
            "sortedMap must take the elements of a flat list in yield! and for"
        )

        // A flat list of strings is a sequence of objects, which a type test for the flat list of objects does not see
        let widened = FlatListOracles.stringsAsObjects (FlatList.ofArray [| "a"; "b" |])

        let objectTable = sortedMap {
            for item in widened do
                yield struct (string item, 1)
        }

        CollectionAssert.AreEqual (
            [| "a"; "b" |],
            Seq.toArray (SortedMap.keys objectTable),
            "sortedMap must take the elements of a flat list of strings as objects in for"
        )

    [<TestMethod>]
    member _.``SortedMapCE rejects a null key comparer`` () =
        let ex =
            Assert.ThrowsExactly<ArgumentNullException>(
                Action (fun () ->
                    SortedMapCE<string>(Unchecked.defaultof<IComparer<string>>)
                    |> ignore
                ),
                "SortedMapCE must reject a null key comparer"
            )

        Assert.AreEqual<string | null>("keyComparer", ex.ParamName, "SortedMapCE must name the key comparer")
