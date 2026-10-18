namespace FSharp.Collections.Immutable.Tests

open System
open System.Collections.Generic
open FSharp.Collections.Immutable
open Microsoft.VisualStudio.TestTools.UnitTesting

/// The builder functions of `SortedMap`; maps have no builder module (ADR 0001, D2).
[<TestClass; SortedMapTestCategory; BuilderTestCategory>]
type SortedMapBuilderTests () =

    /// A value comparer that differs from the default one: it finds numbers of the same absolute value equal.
    static let absoluteValues =
        HashIdentity.FromFunctions (fun (value : int) -> abs value) (fun left right -> abs left = abs right)

    [<TestMethod>]
    member _.``build returns the bindings that the action sets in a builder with the default comparers`` () =
        let table =
            SortedMap.build (fun builder ->
                for i in 3..-1..1 do
                    builder[i] <- i * i
            )

        CollectionAssert.AreEqual (
            [| struct (1, 1); struct (2, 4); struct (3, 9) |],
            SortedMap.toArray table,
            "SortedMap.build must return the bindings that the action set, in the order of the keys"
        )

        Assert.AreSame<IComparer<int>>(
            LanguagePrimitives.FastGenericComparer<int>,
            SortedMap.keyComparer table,
            "SortedMap.build must order the keys with F# generic comparison"
        )

        Assert.AreSame<IEqualityComparer<int>>(
            EqualityComparer<int>.Default,
            SortedMap.valueComparer table,
            "SortedMap.build must compare the values with the default comparer"
        )

    [<TestMethod>]
    member _.``build returns an empty map for an action that sets nothing`` () =
        let table : SortedMap<string, int> = SortedMap.build ignore
        Assert.IsTrue (SortedMap.isEmpty table, "SortedMap.build must return an empty map when the action sets nothing")

    [<TestMethod>]
    member _.``build keeps the earlier binding of a key for a value that the default comparer finds equal`` () =
        // The indexer of the builder leaves the binding as it is, its key included (ADR 0001, D14)
        let first = MapInputs.Copy "uno"

        let table =
            SortedMap.build (fun builder ->
                builder["one"] <- first
                builder["one"] <- MapInputs.Copy "uno"
            )

        Assert.AreSame (first, SortedMap.find "one" table, "SortedMap.build must keep the earlier value of the key")

    [<TestMethod>]
    member _.``the build of the comparer struct has both comparers and merges the equal bindings`` () =
        let comparers = SortedMap.forComparers StringComparer.OrdinalIgnoreCase absoluteValues

        let table =
            comparers.build (fun builder ->
                builder["one"] <- 1
                builder["ONE"] <- -1
            )

        Assert.AreSame<IComparer<string>>(
            StringComparer.OrdinalIgnoreCase,
            SortedMap.keyComparer table,
            "The build of the struct must create a sorted map with the key comparer"
        )

        Assert.AreSame (
            absoluteValues,
            SortedMap.valueComparer table,
            "The build of the struct must create a sorted map with the value comparer"
        )

        CollectionAssert.AreEqual (
            [| struct ("one", 1) |],
            SortedMap.toArray table,
            "The build of the struct must keep the earlier binding for a value that the value comparer finds equal"
        )

    [<TestMethod>]
    member _.``update applies the action to a builder over the bindings and keeps the comparers`` () =
        let table = (SortedMap.forKeyComparer StringComparer.OrdinalIgnoreCase).ofArray [| "one", 1; "two", 2 |]

        let updated =
            SortedMap.update
                (fun builder ->
                    builder["THREE"] <- 3
                    builder.Remove "ONE" |> ignore
                )
                table

        CollectionAssert.AreEqual (
            [| struct ("THREE", 3); struct ("two", 2) |],
            SortedMap.toArray updated,
            "SortedMap.update must apply the action to the bindings of the map"
        )

        Assert.AreSame<IComparer<string>>(
            StringComparer.OrdinalIgnoreCase,
            SortedMap.keyComparer updated,
            "SortedMap.update must keep the key comparer"
        )

        Assert.AreEqual<int>(2, SortedMap.count table, "SortedMap.update must not change the input")
        Assert.IsTrue (SortedMap.containsKey "one" table, "SortedMap.update must not change the input")

    [<TestMethod>]
    member _.``builder starts empty with F# generic comparison and the default value comparer`` () =
        let builder = SortedMap.builder<string, int>()
        Assert.AreEqual<int>(0, builder.Count, "SortedMap.builder must start without bindings")

        Assert.AreSame<IComparer<string>>(
            LanguagePrimitives.FastGenericComparer<string>,
            builder.KeyComparer,
            "SortedMap.builder must order the keys with F# generic comparison"
        )

        Assert.AreSame<IEqualityComparer<int>>(
            EqualityComparer<int>.Default,
            builder.ValueComparer,
            "SortedMap.builder must compare the values with the default comparer"
        )

    [<TestMethod>]
    member _.``builder orders the keys as Map does`` () =
        let builder = SortedMap.builder ()

        for key in [| "b"; "a"; "B"; "A"; "_" |] do
            builder[key] <- key.Length

        CollectionAssert.AreEqual (
            [| "A"; "B"; "_"; "a"; "b" |],
            Seq.toArray (SortedMap.keys (SortedMap.ofBuilder builder)),
            "SortedMap.builder must order the keys ordinally, as Map does"
        )

    [<TestMethod>]
    member _.``the builder of the comparer struct has both comparers`` () =
        let valueComparer =
            HashIdentity.FromFunctions (fun (value : int) -> abs value) (fun left right -> abs left = abs right)

        let builder = (SortedMap.forComparers StringComparer.OrdinalIgnoreCase valueComparer).builder()

        Assert.AreSame<IComparer<string>>(
            StringComparer.OrdinalIgnoreCase,
            builder.KeyComparer,
            "The builder of SortedMap.forComparers must have the key comparer"
        )

        Assert.AreSame (
            valueComparer,
            builder.ValueComparer,
            "The builder of SortedMap.forComparers must have the value comparer"
        )

    [<TestMethod>]
    member _.``the builder of a comparer struct with a key comparer has the default comparer of the values`` () =
        let builder : SortedMap<string, int>.Builder =
            (SortedMap.forKeyComparer StringComparer.OrdinalIgnoreCase).builder()

        builder["one"] <- 1
        builder["ONE"] <- 2
        Assert.AreEqual<int>(1, builder.Count, "The builder of SortedMap.forKeyComparer must look the keys up with the comparer")

        Assert.AreSame<IEqualityComparer<int>>(
            EqualityComparer<int>.Default,
            builder.ValueComparer,
            "The builder of SortedMap.forKeyComparer must compare the values with the default comparer"
        )

    [<TestMethod>]
    member _.``ofBuilder returns the bindings and keeps the builder usable`` () =
        let builder : SortedMap<string, int>.Builder =
            (SortedMap.forKeyComparer StringComparer.OrdinalIgnoreCase).builder()

        builder["one"] <- 1
        let table = SortedMap.ofBuilder builder
        builder["two"] <- 2

        CollectionAssert.AreEqual (
            [| struct ("one", 1) |],
            SortedMap.toArray table,
            "SortedMap.ofBuilder must return the bindings"
        )

        Assert.AreEqual<int>(2, builder.Count, "SortedMap.ofBuilder must keep the builder usable")

        Assert.AreSame<IComparer<string>>(
            StringComparer.OrdinalIgnoreCase,
            SortedMap.keyComparer table,
            "SortedMap.ofBuilder must keep the comparers"
        )

    [<TestMethod>]
    member _.``ofBuilder throws for a null builder`` () =
        let ex =
            Assert.ThrowsExactly<ArgumentNullException>(
                Action (fun () ->
                    SortedMap.ofBuilder (Unchecked.defaultof<SortedMap<string, int>.Builder>)
                    |> ignore
                ),
                "SortedMap.ofBuilder must reject a null builder"
            )

        Assert.AreEqual<string | null>("builder", ex.ParamName, "SortedMap.ofBuilder must name the builder parameter")

    [<TestMethod>]
    member _.``toBuilder holds the bindings and leaves the map unchanged`` () =
        let table = (SortedMap.forKeyComparer StringComparer.OrdinalIgnoreCase).ofArray [| "one", 1; "two", 2 |]

        let builder = SortedMap.toBuilder table
        builder["THREE"] <- 3
        builder.Remove "ONE" |> ignore

        Assert.AreEqual<int>(2, builder.Count, "SortedMap.toBuilder must hold the bindings and use the key comparer")

        Assert.AreSame<IComparer<string>>(
            StringComparer.OrdinalIgnoreCase,
            builder.KeyComparer,
            "SortedMap.toBuilder must keep the key comparer"
        )

        Assert.AreEqual<int>(2, SortedMap.count table, "SortedMap.toBuilder must not change the map")
        Assert.IsTrue (SortedMap.containsKey "one" table, "SortedMap.toBuilder must not change the map")

    [<TestMethod>]
    member _.``a builder of a map returns the map itself when nothing changed`` () =
        let table = SortedMap.ofArray [| struct ("one", 1) |]

        Assert.AreSame (
            table,
            SortedMap.ofBuilder (SortedMap.toBuilder table),
            "A builder that changed nothing must return its map"
        )
