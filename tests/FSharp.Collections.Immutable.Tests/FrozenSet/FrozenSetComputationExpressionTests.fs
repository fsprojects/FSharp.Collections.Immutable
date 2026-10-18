namespace FSharp.Collections.Immutable.Tests

open System
open System.Collections.Frozen
open System.Collections.Generic
open FSharp.Collections.Immutable
open Microsoft.VisualStudio.TestTools.UnitTesting

/// The `frozenSet { }` computation expression and the builders with a comparer.
[<TestClass; FrozenSetTestCategory; ComputationExpressionTestCategory>]
type FrozenSetComputationExpressionTests () =

    /// The elements of a frozen set, sorted in the order of F# generic comparison.
    static let sorted (set : FrozenSet<'T>) = FrozenSet.toArray set |> Array.sort

    /// <summary>
    /// The expressions that reject their input, by name, with the parameter that names the input and the expression.
    /// </summary>
    /// <remarks>
    /// A default flat list converts to a sequence without a check and fails when the sequence is enumerated. A null
    /// sequence or frozen set reaches <c>AddRange</c>, which names its own parameter, and a null array passes
    /// <c>AddRange</c> without an exception (ADR 0001, D6). Each expression rejects its input when the buffer is created,
    /// not when the expression runs.
    /// </remarks>
    static let rejections : Map<string, struct (string * (unit -> unit))> =
        let nullElements = Unchecked.defaultof<seq<int>>
        let defaultElements = Unchecked.defaultof<FlatList<int>> :> seq<int>

        // The flat list of strings is a sequence of objects, which a type test for the flat list of objects does not see
        let widenedElements = FlatListOracles.defaultStringsAsObjects

        Map.ofArray [|
            "yield! (null sequence)", struct ("items", (fun () -> frozenSet { yield! nullElements } |> ignore))
            "yield! (default flat list)", struct ("items", (fun () -> frozenSet { yield! defaultElements } |> ignore))
            "yield! (default flat list of strings as objects)",
            struct ("items", (fun () -> frozenSet { yield! widenedElements } |> ignore))
            "yield! (null array)",
            struct ("items",
                    (fun () ->
                        frozenSet { yield! Unchecked.defaultof<int array> }
                        |> ignore
                    ))
            "yield! (null frozen set)",
            struct ("items",
                    (fun () ->
                        frozenSet { yield! Unchecked.defaultof<FrozenSet<int>> }
                        |> ignore
                    ))
            "for (null sequence)",
            struct ("source",
                    (fun () ->
                        frozenSet {
                            for item in nullElements do
                                yield item
                        }
                        |> ignore
                    ))
            "for (default flat list)",
            struct ("source",
                    (fun () ->
                        frozenSet {
                            for item in defaultElements do
                                yield item
                        }
                        |> ignore
                    ))
            "for (default flat list of strings as objects)",
            struct ("source",
                    (fun () ->
                        frozenSet {
                            for item in widenedElements do
                                yield item
                        }
                        |> ignore
                    ))
        |]

    /// The names of the expressions that reject their input, as rows.
    static member Rejections = rejections.Keys

    [<TestMethod>]
    member _.``an expression without elements is empty`` () =
        let set : FrozenSet<int> = frozenSet { () }
        Assert.IsTrue (FrozenSet.isEmpty set, "frozenSet { () } must be empty")

    [<TestMethod>]
    member _.``yield adds each distinct element once`` () =
        let set = frozenSet {
            yield 1
            yield 2
            yield 1
        }

        CollectionAssert.AreEqual ([| 1; 2 |], sorted set, "frozenSet must add each distinct element once")

    [<TestMethod>]
    member _.``implicit yields add the elements`` () =
        let set = frozenSet {
            3
            1
            2
        }

        CollectionAssert.AreEqual ([| 1; 2; 3 |], sorted set, "frozenSet must add the elements of implicit yields")

    [<TestMethod>]
    member _.``yield! adds a sequence, an array, a frozen set and another expression`` () =
        let inner = frozenSet { yield 7 }

        let set = frozenSet {
            // A list is the sequence that the overload of sequences takes
            yield! [ 1; 2 ]
            yield! [| 3; 4 |]
            yield! FrozenSet.ofArray [| 5; 6; 1 |]
            yield! inner
        }

        CollectionAssert.AreEqual ([| 1..7 |], sorted set, "frozenSet must add the elements of yield!")

    [<TestMethod; DynamicData(nameof FrozenSetComputationExpressionTests.Rejections)>]
    member _.``yield! and for reject a null input and a default flat list given as a sequence`` (name : string) =
        let struct (paramName, actual) = rejections[name]
        FlatListAssert.RejectsNull ($"frozenSet {{ %s{name} }}", paramName, actual)

    [<TestMethod>]
    member _.``yield! and for take a flat list of strings that is not default as a sequence of objects`` () =
        let widened = FlatListOracles.stringsAsObjects (FlatList.ofArray [| "a"; "b" |])

        let set = frozenSet {
            yield! widened

            for item in widened do
                yield item
        }

        CollectionAssert.AreEquivalent (
            [| ("a" :> obj); ("b" :> obj) |],
            FrozenSet.toArray set,
            "frozenSet must take the elements of a flat list of strings as objects in yield! and for"
        )

    [<TestMethod>]
    member _.``for adds the elements of the body for each item`` () =
        let set = frozenSet {
            for item in -2 .. 2 do
                yield item * item
        }

        CollectionAssert.AreEqual ([| 0; 1; 4 |], sorted set, "frozenSet must run the body of for for each item")

    [<TestMethod>]
    member _.``while adds the elements while the guard holds`` () =
        let mutable count = 0

        let set = frozenSet {
            while count < 3 do
                count <- count + 1
                yield count
        }

        CollectionAssert.AreEqual ([| 1; 2; 3 |], sorted set, "frozenSet must run the body of while while the guard holds")

    [<TestMethod>]
    member _.``if without else and following elements combine`` () =
        let set = frozenSet {
            if false then
                yield 0

            yield 1

            if true then
                yield 2
        }

        CollectionAssert.AreEqual ([| 1; 2 |], sorted set, "frozenSet must combine conditional and plain elements")

    [<TestMethod>]
    member _.``try with adds the elements of the handler after an exception`` () =
        let set = frozenSet {
            try
                yield 1
                failwith "fails"
            with _ ->
                yield 2
        }

        CollectionAssert.AreEqual (
            [| 1; 2 |],
            sorted set,
            "frozenSet must keep the elements before the exception and add the handler's"
        )

    [<TestMethod>]
    member _.``try finally runs the compensation`` () =
        let mutable compensated = false

        let set = frozenSet {
            try
                yield 1
            finally
                compensated <- true
        }

        Assert.IsTrue (compensated, "frozenSet must run the compensation of try finally")
        CollectionAssert.AreEqual ([| 1 |], sorted set, "frozenSet must keep the elements of the body of try finally")

    [<TestMethod>]
    member _.``use disposes the resource after the body`` () =
        let mutable disposed = false

        let set = frozenSet {
            use _resource = {
                new IDisposable with
                    member _.Dispose () = disposed <- true
            }

            yield 1
        }

        Assert.IsTrue (disposed, "frozenSet must dispose the resource of use")
        CollectionAssert.AreEqual ([| 1 |], sorted set, "frozenSet must keep the elements of the body of use")

    [<TestMethod>]
    member _.``frozenSet compares with the default comparer`` () =
        let set = frozenSet {
            "apple"
            "APPLE"
        }

        Assert.AreSame (
            EqualityComparer<string>.Default :> IEqualityComparer<string>,
            FrozenSet.comparer set,
            "frozenSet must compare with the default comparer"
        )

        Assert.HasCount (2, set, "frozenSet must tell the case apart")

    [<TestMethod>]
    member _.``a builder with a comparer builds frozen sets that compare with it`` () =
        let ignoreCase : IEqualityComparer<string> = StringComparer.OrdinalIgnoreCase

        let set = FrozenSetCE ignoreCase {
            "apple"
            "APPLE"
            "Banana"
        }

        Assert.AreSame (ignoreCase, FrozenSet.comparer set, "FrozenSetCE must build frozen sets with its comparer")

        CollectionAssert.AreEqual (
            [| "Banana"; "apple" |],
            sorted set,
            "FrozenSetCE must keep the first of the elements equal in its comparer"
        )

    [<TestMethod>]
    member _.``a builder rejects a null comparer when it is created`` () =
        let ex =
            Assert.ThrowsExactly<ArgumentNullException>(
                Action (fun () ->
                    FrozenSetCE (Unchecked.defaultof<IEqualityComparer<string>>)
                    |> ignore
                ),
                "FrozenSetCE must reject a null comparer"
            )

        Assert.AreEqual<string | null>("comparer", ex.ParamName, "FrozenSetCE must name the parameter comparer")
