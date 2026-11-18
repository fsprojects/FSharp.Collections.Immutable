namespace FSharp.Collections.Immutable.Tests

open System
open System.Collections.Generic
open FSharp.Collections.Immutable
open Microsoft.VisualStudio.TestTools.UnitTesting

/// The builder functions of `HashSet` and the builder of its struct with a comparer; hash sets have no builder module
/// (ADR 0001, Appendix C).
[<TestClass; HashSetTestCategory; BuilderTestCategory>]
type HashSetBuilderTests () =

    static let ignoreCase : IEqualityComparer<string> = StringComparer.OrdinalIgnoreCase

    static let defaultComparer : IEqualityComparer<string> = EqualityComparer<string>.Default

    /// Creates hash sets and builders that compare without case.
    static let caseless = HashSet.forComparer ignoreCase

    /// The elements of a builder, sorted in the order of F# generic comparison.
    static let sorted (elements : seq<string>) = Seq.toArray elements |> Array.sort

    [<TestMethod>]
    member _.``build returns the hash set of the elements that the action added`` () =
        let set =
            HashSet.build (fun builder ->
                builder.Add "b" |> ignore
                builder.Add "a" |> ignore
                builder.Add "b" |> ignore
            )

        CollectionAssert.AreEqual ([| "a"; "b" |], sorted set, "HashSet.build must hold the elements that the action added")
        Assert.AreSame (defaultComparer, HashSet.comparer set, "HashSet.build must compare with the default comparer")

    [<TestMethod>]
    member _.``build hands an empty builder with the default comparer to the action`` () =
        let set =
            HashSet.build<string>(fun builder ->
                Assert.IsEmpty (builder, "HashSet.build must hand over an empty builder")

                Assert.AreSame (
                    defaultComparer,
                    builder.KeyComparer,
                    "HashSet.build must hand over a builder that compares with the default comparer"
                )
            )

        Assert.IsEmpty (set, "HashSet.build of an action that adds nothing must return an empty hash set")

    [<TestMethod>]
    member _.``the build of the struct compares with its comparer`` () =
        let set =
            caseless.build (fun builder ->
                Assert.AreSame (
                    ignoreCase,
                    builder.KeyComparer,
                    "CustomComparerHashSet.build must hand over a builder that compares with its comparer"
                )

                builder.UnionWith [| "apple"; "Banana"; "APPLE" |]
            )

        CollectionAssert.AreEqual (
            [| "Banana"; "apple" |],
            sorted set,
            "CustomComparerHashSet.build must keep the first of the elements that the comparer considers equal"
        )

        Assert.AreSame (ignoreCase, HashSet.comparer set, "CustomComparerHashSet.build must keep the comparer")

    [<TestMethod>]
    member _.``builder starts empty with the default comparer`` () =
        let builder = HashSet.builder<string>()
        Assert.IsEmpty (builder, "HashSet.builder must start without elements")

        Assert.AreSame (defaultComparer, builder.KeyComparer, "HashSet.builder must compare with the default comparer")

    [<TestMethod>]
    member _.``the builder of the struct compares with its comparer`` () =
        let builder = caseless.builder ()
        Assert.IsEmpty (builder, "CustomComparerHashSet.builder must start without elements")
        Assert.IsTrue (builder.Add "apple", "The builder must add the first element")
        Assert.IsFalse (builder.Add "APPLE", "CustomComparerHashSet.builder must compare without case")
        Assert.AreSame (ignoreCase, builder.KeyComparer, "CustomComparerHashSet.builder must keep the comparer")

    [<TestMethod>]
    member _.``ofBuilder returns the contents and keeps the builder usable`` () =
        let builder = caseless.builder ()
        builder.UnionWith [| "apple"; "Banana" |]
        let set = HashSet.ofBuilder builder
        builder.Add "cherry" |> ignore

        CollectionAssert.AreEqual ([| "Banana"; "apple" |], sorted set, "HashSet.ofBuilder must hold the elements of the builder")
        Assert.HasCount (3, builder, "HashSet.ofBuilder must keep the builder usable")
        Assert.AreSame (ignoreCase, HashSet.comparer set, "HashSet.ofBuilder must keep the comparer of the builder")

    [<TestMethod>]
    member _.``toBuilder holds the elements and the comparer of the hash set`` () =
        let set = caseless.ofArray [| "apple"; "Banana" |]
        let builder = HashSet.toBuilder set
        builder.Add "cherry" |> ignore
        builder.Remove "APPLE" |> ignore

        Assert.AreSame (ignoreCase, builder.KeyComparer, "HashSet.toBuilder must keep the comparer")
        CollectionAssert.AreEqual ([| "Banana"; "cherry" |], sorted builder, "HashSet.toBuilder must hold the elements")
        CollectionAssert.AreEqual ([| "Banana"; "apple" |], sorted set, "HashSet.toBuilder must not change the hash set")

    [<TestMethod>]
    member _.``update changes a copy of the hash set`` () =
        let set = caseless.ofArray [| "apple"; "Banana" |]

        let updated =
            HashSet.update
                (fun builder ->
                    builder.Add "cherry" |> ignore
                    builder.Remove "APPLE" |> ignore
                )
                set

        CollectionAssert.AreEqual ([| "Banana"; "cherry" |], sorted updated, "HashSet.update must hold the changed elements")
        CollectionAssert.AreEqual ([| "Banana"; "apple" |], sorted set, "HashSet.update must not change the hash set")
        Assert.AreSame (ignoreCase, HashSet.comparer updated, "HashSet.update must keep the comparer")

    [<TestMethod>]
    member _.``update hands a builder with the elements and the comparer of the hash set to the action`` () =
        let set = caseless.ofArray [| "apple"; "Banana" |]

        HashSet.update
            (fun builder ->
                CollectionAssert.AreEqual (
                    [| "Banana"; "apple" |],
                    sorted builder,
                    "HashSet.update must hand over a builder with the elements of the hash set"
                )

                Assert.AreSame (
                    ignoreCase,
                    builder.KeyComparer,
                    "HashSet.update must hand over a builder with the comparer of the hash set"
                )
            )
            set
        |> ignore

    // The in-place operations of the builder that no function of the module has
    [<TestMethod>]
    member _.``update reaches the in-place operations of the builder`` () =
        let set = HashSet.ofArray [| 1; 2; 3 |]

        CollectionAssert.AreEqual (
            [| 2; 3 |],
            HashSet.toArray (HashSet.update (fun builder -> builder.IntersectWith [| 2; 3; 9 |]) set)
            |> Array.sort,
            "HashSet.update must apply IntersectWith"
        )

        CollectionAssert.AreEqual (
            [| 1; 3; 9 |],
            HashSet.toArray (HashSet.update (fun builder -> builder.SymmetricExceptWith [| 2; 9 |]) set)
            |> Array.sort,
            "HashSet.update must apply SymmetricExceptWith"
        )
