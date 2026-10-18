namespace FSharp.Collections.Immutable.Tests

open System
open System.Collections.Generic
open FSharp.Collections.Immutable
open Microsoft.VisualStudio.TestTools.UnitTesting

/// The builder functions of `HashMap`; maps have no builder module (ADR 0001, D2).
[<TestClass; HashMapTestCategory; BuilderTestCategory>]
type HashMapBuilderTests () =

    /// A value comparer that differs from the default one: it finds numbers of the same absolute value equal.
    static let absoluteValues =
        HashIdentity.FromFunctions (fun (value : int) -> abs value) (fun left right -> abs left = abs right)

    [<TestMethod>]
    member _.``build returns the bindings that the action sets in a builder with the default comparers`` () =
        let table =
            HashMap.build (fun builder ->
                for i in 1..3 do
                    builder[i] <- i * i
            )

        CollectionAssert.AreEqual (
            [| struct (1, 1); struct (2, 4); struct (3, 9) |],
            MapInputs.SortByKey (HashMap.toArray table),
            "HashMap.build must return the bindings that the action set"
        )

        Assert.AreSame<IEqualityComparer<int>>(
            EqualityComparer<int>.Default,
            HashMap.keyComparer table,
            "HashMap.build must compare the keys with the default comparer"
        )

    [<TestMethod>]
    member _.``build returns an empty map for an action that sets nothing`` () =
        let table : HashMap<string, int> = HashMap.build ignore
        Assert.IsTrue (HashMap.isEmpty table, "HashMap.build must return an empty map when the action sets nothing")

    [<TestMethod>]
    member _.``build keeps the earlier binding of a key for a value that the default comparer finds equal`` () =
        // The indexer of the builder leaves the binding as it is, its key included (ADR 0001, D14)
        let first = MapInputs.Copy "uno"

        let table =
            HashMap.build (fun builder ->
                builder["one"] <- first
                builder["one"] <- MapInputs.Copy "uno"
            )

        Assert.AreSame (first, HashMap.find "one" table, "HashMap.build must keep the earlier value of the key")

    [<TestMethod>]
    member _.``the build of the comparer struct has both comparers and merges the equal bindings`` () =
        let comparers = HashMap.forComparers StringComparer.OrdinalIgnoreCase absoluteValues

        let table =
            comparers.build (fun builder ->
                builder["one"] <- 1
                builder["ONE"] <- -1
            )

        Assert.AreSame<IEqualityComparer<string>>(
            StringComparer.OrdinalIgnoreCase,
            HashMap.keyComparer table,
            "The build of the struct must create a hash map with the key comparer"
        )

        Assert.AreSame (
            absoluteValues,
            HashMap.valueComparer table,
            "The build of the struct must create a hash map with the value comparer"
        )

        CollectionAssert.AreEqual (
            [| struct ("one", 1) |],
            HashMap.toArray table,
            "The build of the struct must keep the earlier binding for a value that the value comparer finds equal"
        )

    [<TestMethod>]
    member _.``update applies the action to a builder over the bindings and keeps the comparers`` () =
        let table = (HashMap.forKeyComparer StringComparer.OrdinalIgnoreCase).ofArray [| "one", 1; "two", 2 |]

        let updated =
            HashMap.update
                (fun builder ->
                    builder["THREE"] <- 3
                    builder.Remove "ONE" |> ignore
                )
                table

        CollectionAssert.AreEqual (
            [| struct ("THREE", 3); struct ("two", 2) |],
            Array.sortBy (fun struct (key, _) -> key) (HashMap.toArray updated),
            "HashMap.update must apply the action to the bindings of the map"
        )

        Assert.AreSame<IEqualityComparer<string>>(
            StringComparer.OrdinalIgnoreCase,
            HashMap.keyComparer updated,
            "HashMap.update must keep the key comparer"
        )

        Assert.AreEqual<int>(2, HashMap.count table, "HashMap.update must not change the input")
        Assert.IsTrue (HashMap.containsKey "one" table, "HashMap.update must not change the input")

    [<TestMethod>]
    member _.``builder starts empty with the default comparers`` () =
        let builder = HashMap.builder<string, int>()
        Assert.AreEqual<int>(0, builder.Count, "HashMap.builder must start without bindings")
        Assert.AreSame<IEqualityComparer<string>>(
            EqualityComparer<string>.Default,
            builder.KeyComparer,
            "HashMap.builder must compare the keys with the default comparer"
        )
        Assert.AreSame<IEqualityComparer<int>>(
            EqualityComparer<int>.Default,
            builder.ValueComparer,
            "HashMap.builder must compare the values with the default comparer"
        )

    [<TestMethod>]
    member _.``the builder of the comparer struct has both comparers`` () =
        let valueComparer =
            HashIdentity.FromFunctions (fun (value : int) -> abs value) (fun left right -> abs left = abs right)
        let builder = (HashMap.forComparers StringComparer.OrdinalIgnoreCase valueComparer).builder()
        Assert.AreSame<IEqualityComparer<string>>(
            StringComparer.OrdinalIgnoreCase,
            builder.KeyComparer,
            "The builder of HashMap.forComparers must have the key comparer"
        )
        Assert.AreSame (valueComparer, builder.ValueComparer, "The builder of HashMap.forComparers must have the value comparer")

    [<TestMethod>]
    member _.``the builder of a comparer struct with a key comparer has the default comparer of the values`` () =
        let builder : HashMap<string, int>.Builder =
            (HashMap.forKeyComparer StringComparer.OrdinalIgnoreCase).builder()
        builder["one"] <- 1
        builder["ONE"] <- 2
        Assert.AreEqual<int>(1, builder.Count, "The builder of HashMap.forKeyComparer must look the keys up with the comparer")

        Assert.AreSame<IEqualityComparer<int>>(
            EqualityComparer<int>.Default,
            builder.ValueComparer,
            "The builder of HashMap.forKeyComparer must compare the values with the default comparer"
        )

    [<TestMethod>]
    member _.``ofBuilder returns the bindings and keeps the builder usable`` () =
        let builder : HashMap<string, int>.Builder =
            (HashMap.forKeyComparer StringComparer.OrdinalIgnoreCase).builder()
        builder["one"] <- 1
        let table = HashMap.ofBuilder builder
        builder["two"] <- 2

        CollectionAssert.AreEqual ([| struct ("one", 1) |], HashMap.toArray table, "HashMap.ofBuilder must return the bindings")
        Assert.AreEqual<int>(2, builder.Count, "HashMap.ofBuilder must keep the builder usable")
        Assert.AreSame<IEqualityComparer<string>>(
            StringComparer.OrdinalIgnoreCase,
            HashMap.keyComparer table,
            "HashMap.ofBuilder must keep the comparers"
        )

    [<TestMethod>]
    member _.``ofBuilder throws for a null builder`` () =
        let ex =
            Assert.ThrowsExactly<ArgumentNullException>(
                Action (fun () ->
                    HashMap.ofBuilder (Unchecked.defaultof<HashMap<string, int>.Builder>)
                    |> ignore
                ),
                "HashMap.ofBuilder must reject a null builder"
            )

        Assert.AreEqual<string | null>("builder", ex.ParamName, "HashMap.ofBuilder must name the builder parameter")

    [<TestMethod>]
    member _.``toBuilder holds the bindings and leaves the map unchanged`` () =
        let table = (HashMap.forKeyComparer StringComparer.OrdinalIgnoreCase).ofArray [| "one", 1; "two", 2 |]

        let builder = HashMap.toBuilder table
        builder["THREE"] <- 3
        builder.Remove "ONE" |> ignore

        Assert.AreEqual<int>(2, builder.Count, "HashMap.toBuilder must hold the bindings and use the key comparer")
        Assert.AreSame<IEqualityComparer<string>>(
            StringComparer.OrdinalIgnoreCase,
            builder.KeyComparer,
            "HashMap.toBuilder must keep the key comparer"
        )
        Assert.AreEqual<int>(2, HashMap.count table, "HashMap.toBuilder must not change the map")
        Assert.IsTrue (HashMap.containsKey "one" table, "HashMap.toBuilder must not change the map")

    [<TestMethod>]
    member _.``a builder of a map returns the map itself when nothing changed`` () =
        let table = HashMap.ofArray [| struct ("one", 1) |]
        Assert.AreSame (table, HashMap.ofBuilder (HashMap.toBuilder table), "A builder that changed nothing must return its map")
