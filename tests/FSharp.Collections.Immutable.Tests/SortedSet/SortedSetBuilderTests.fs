namespace FSharp.Collections.Immutable.Tests

open System
open System.Collections.Generic
open FSharp.Collections.Immutable
open Microsoft.VisualStudio.TestTools.UnitTesting

/// The builder functions of `SortedSet` and the builder of its struct with a comparer; sorted sets have no builder module
/// (ADR 0001, Appendix C).
[<TestClass; SortedSetTestCategory; BuilderTestCategory>]
type SortedSetBuilderTests () =

    static let ignoreCase : IComparer<string> = StringComparer.OrdinalIgnoreCase

    /// Creates sorted sets and builders that order strings without case.
    static let caseless = SortedSet.forComparer ignoreCase

    /// The comparer of the sorted sets of strings created without one.
    static let defaultComparer : IComparer<string> = LanguagePrimitives.FastGenericComparer<string>

    [<TestMethod>]
    member _.``build returns the sorted set of the elements that the action added`` () =
        let set =
            SortedSet.build (fun builder ->
                builder.Add "b" |> ignore
                builder.Add "a" |> ignore
                builder.Add "b" |> ignore
            )

        CollectionAssert.AreEqual (
            [| "a"; "b" |],
            SortedSet.toArray set,
            "SortedSet.build must hold the elements that the action added"
        )

        Assert.AreSame (defaultComparer, SortedSet.comparer set, "SortedSet.build must compare with F# generic comparison")

    [<TestMethod>]
    member _.``build hands an empty builder with F# generic comparison to the action`` () =
        let set =
            SortedSet.build<string>(fun builder ->
                Assert.IsEmpty (builder, "SortedSet.build must hand over an empty builder")

                Assert.AreSame (
                    defaultComparer,
                    builder.KeyComparer,
                    "SortedSet.build must hand over a builder that compares with F# generic comparison"
                )
            )

        Assert.IsEmpty (set, "SortedSet.build of an action that adds nothing must return an empty sorted set")

    [<TestMethod>]
    member _.``the build of the struct orders with its comparer`` () =
        let set =
            caseless.build (fun builder ->
                Assert.AreSame (
                    ignoreCase,
                    builder.KeyComparer,
                    "CustomComparerSortedSet.build must hand over a builder that orders with its comparer"
                )

                builder.UnionWith [| "banana"; "apple"; "BANANA" |]
            )

        CollectionAssert.AreEqual (
            [| "apple"; "banana" |],
            SortedSet.toArray set,
            "CustomComparerSortedSet.build must keep the first of the elements that the comparer considers equal"
        )

        Assert.AreSame (ignoreCase, SortedSet.comparer set, "CustomComparerSortedSet.build must keep the comparer")

    [<TestMethod>]
    member _.``builder starts empty with F# generic comparison`` () =
        let builder = SortedSet.builder<string>()
        Assert.IsEmpty (builder, "SortedSet.builder must start without elements")

        Assert.AreSame (
            LanguagePrimitives.FastGenericComparer<string>,
            builder.KeyComparer,
            "SortedSet.builder must compare with F# generic comparison"
        )

        builder.UnionWith [| "b"; "B"; "a"; "A" |]

        CollectionAssert.AreEqual (
            [| "A"; "B"; "a"; "b" |],
            SortedSet.toArray (SortedSet.ofBuilder builder),
            "SortedSet.builder must order strings ordinally"
        )

    [<TestMethod>]
    member _.``the builder of the struct orders with its comparer`` () =
        let builder = caseless.builder ()
        Assert.IsEmpty (builder, "CustomComparerSortedSet.builder must start without elements")
        Assert.IsTrue (builder.Add "banana", "The builder must add the first element")
        Assert.IsTrue (builder.Add "apple", "The builder must add the second element")
        Assert.IsFalse (builder.Add "APPLE", "CustomComparerSortedSet.builder must compare without case")
        Assert.AreSame (ignoreCase, builder.KeyComparer, "CustomComparerSortedSet.builder must keep the comparer")

        CollectionAssert.AreEqual (
            [| "apple"; "banana" |],
            Seq.toArray builder,
            "CustomComparerSortedSet.builder must order with the comparer"
        )

    [<TestMethod>]
    member _.``ofBuilder returns the contents and keeps the builder usable`` () =
        let builder = caseless.builder ()
        builder.UnionWith [| "cherry"; "apple" |]
        let set = SortedSet.ofBuilder builder
        builder.Add "banana" |> ignore

        CollectionAssert.AreEqual (
            [| "apple"; "cherry" |],
            SortedSet.toArray set,
            "SortedSet.ofBuilder must hold the elements of the builder"
        )

        Assert.HasCount (3, builder, "SortedSet.ofBuilder must keep the builder usable")
        Assert.AreSame (ignoreCase, SortedSet.comparer set, "SortedSet.ofBuilder must keep the comparer of the builder")

    [<TestMethod>]
    member _.``toBuilder holds the elements and the comparer of the sorted set`` () =
        let set = caseless.ofArray [| "cherry"; "apple" |]
        let builder = SortedSet.toBuilder set
        builder.Add "banana" |> ignore
        builder.Remove "APPLE" |> ignore

        Assert.AreSame (ignoreCase, builder.KeyComparer, "SortedSet.toBuilder must keep the comparer")

        CollectionAssert.AreEqual (
            [| "banana"; "cherry" |],
            Seq.toArray builder,
            "SortedSet.toBuilder must hold the elements in order"
        )

        CollectionAssert.AreEqual (
            [| "apple"; "cherry" |],
            SortedSet.toArray set,
            "SortedSet.toBuilder must not change the sorted set"
        )

    [<TestMethod>]
    member _.``update changes a copy of the sorted set`` () =
        let set = caseless.ofArray [| "apple"; "Banana" |]

        let updated =
            SortedSet.update
                (fun builder ->
                    builder.Add "cherry" |> ignore
                    builder.Remove "APPLE" |> ignore
                )
                set

        CollectionAssert.AreEqual (
            [| "Banana"; "cherry" |],
            SortedSet.toArray updated,
            "SortedSet.update must hold the changed elements"
        )

        CollectionAssert.AreEqual (
            [| "apple"; "Banana" |],
            SortedSet.toArray set,
            "SortedSet.update must not change the sorted set"
        )

        Assert.AreSame (ignoreCase, SortedSet.comparer updated, "SortedSet.update must keep the comparer")

    [<TestMethod>]
    member _.``update hands a builder with the elements and the comparer of the sorted set to the action`` () =
        let set = caseless.ofArray [| "apple"; "Banana" |]

        SortedSet.update
            (fun builder ->
                CollectionAssert.AreEqual (
                    [| "apple"; "Banana" |],
                    Seq.toArray builder,
                    "SortedSet.update must hand over a builder with the elements of the sorted set"
                )

                Assert.AreSame (
                    ignoreCase,
                    builder.KeyComparer,
                    "SortedSet.update must hand over a builder with the comparer of the sorted set"
                )
            )
            set
        |> ignore

    // The in-place operations of the builder that no function of the module has
    [<TestMethod>]
    member _.``update reaches the in-place operations of the builder`` () =
        let set = SortedSet.ofArray [| 1; 2; 3 |]

        CollectionAssert.AreEqual (
            [| 2; 3 |],
            SortedSet.toArray (SortedSet.update (fun builder -> builder.IntersectWith [| 2; 3; 9 |]) set),
            "SortedSet.update must apply IntersectWith"
        )

        CollectionAssert.AreEqual (
            [| 1; 3; 9 |],
            SortedSet.toArray (SortedSet.update (fun builder -> builder.SymmetricExceptWith [| 2; 9 |]) set),
            "SortedSet.update must apply SymmetricExceptWith"
        )
