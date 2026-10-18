namespace FSharp.Collections.Immutable.Tests

open System
open System.Collections.Generic
open FSharp.Collections.Immutable
open Microsoft.VisualStudio.TestTools.UnitTesting

/// The `hashSet { }` computation expression and the builders with a comparer.
[<TestClass; HashSetTestCategory; ComputationExpressionTestCategory>]
type HashSetComputationExpressionTests () =

    /// The elements of a hash set, sorted in the order of F# generic comparison.
    static let sorted (set : HashSet<'T>) = HashSet.toArray set |> Array.sort

    /// <summary>
    /// The expressions that reject their input, by name, with the parameter that names the input and the expression.
    /// </summary>
    /// <remarks>
    /// A default flat list converts to a sequence without a check and fails when the sequence is enumerated, and a null
    /// input reaches <c>UnionWith</c>, which names its own parameter (ADR 0001, D6). Each expression rejects its input
    /// when the buffer is created, not when the expression runs.
    /// </remarks>
    static let rejections : Map<string, struct (string * (unit -> unit))> =
        let nullElements = Unchecked.defaultof<seq<int>>
        let defaultElements = Unchecked.defaultof<FlatList<int>> :> seq<int>

        // The flat list of strings is a sequence of objects, which a type test for the flat list of objects does not see
        let widenedElements = FlatListOracles.defaultStringsAsObjects

        Map.ofArray [|
            "yield! (null sequence)", struct ("items", (fun () -> hashSet { yield! nullElements } |> ignore))
            "yield! (default flat list)", struct ("items", (fun () -> hashSet { yield! defaultElements } |> ignore))
            "yield! (default flat list of strings as objects)",
            struct ("items", (fun () -> hashSet { yield! widenedElements } |> ignore))
            "yield! (null array)", struct ("items", (fun () -> hashSet { yield! Unchecked.defaultof<int array> } |> ignore))
            "yield! (null hash set)",
            struct ("items",
                    (fun () ->
                        hashSet { yield! Unchecked.defaultof<HashSet<int>> }
                        |> ignore
                    ))
            "for (null sequence)",
            struct ("source",
                    (fun () ->
                        hashSet {
                            for item in nullElements do
                                yield item
                        }
                        |> ignore
                    ))
            "for (default flat list)",
            struct ("source",
                    (fun () ->
                        hashSet {
                            for item in defaultElements do
                                yield item
                        }
                        |> ignore
                    ))
            "for (default flat list of strings as objects)",
            struct ("source",
                    (fun () ->
                        hashSet {
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
        let set : HashSet<int> = hashSet { () }
        Assert.IsTrue (HashSet.isEmpty set, "hashSet { () } must be empty")

    [<TestMethod>]
    member _.``yield adds each distinct element once`` () =
        let set = hashSet {
            yield 1
            yield 2
            yield 1
        }

        CollectionAssert.AreEqual ([| 1; 2 |], sorted set, "hashSet must add each distinct element once")

    [<TestMethod>]
    member _.``implicit yields add the elements`` () =
        let set = hashSet {
            3
            1
            2
        }

        CollectionAssert.AreEqual ([| 1; 2; 3 |], sorted set, "hashSet must add the elements of implicit yields")

    [<TestMethod>]
    member _.``yield! adds a sequence, an array, a hash set and another expression`` () =
        let inner = hashSet { yield 7 }

        let set = hashSet {
            // A list is the sequence that the overload of sequences takes
            yield! [ 1; 2 ]
            yield! [| 3; 4 |]
            yield! HashSet.ofArray [| 5; 6; 1 |]
            yield! inner
        }

        CollectionAssert.AreEqual ([| 1..7 |], sorted set, "hashSet must add the elements of yield!")

    [<TestMethod; DynamicData(nameof HashSetComputationExpressionTests.Rejections)>]
    member _.``yield! and for reject a null input and a default flat list given as a sequence`` (name : string) =
        let struct (paramName, actual) = rejections[name]
        FlatListAssert.RejectsNull ($"hashSet {{ %s{name} }}", paramName, actual)

    [<TestMethod>]
    member _.``yield! and for take a flat list of strings that is not default as a sequence of objects`` () =
        let widened = FlatListOracles.stringsAsObjects (FlatList.ofArray [| "a"; "b" |])

        let set = hashSet {
            yield! widened

            for item in widened do
                yield item
        }

        CollectionAssert.AreEquivalent (
            [| ("a" :> obj); ("b" :> obj) |],
            HashSet.toArray set,
            "hashSet must take the elements of a flat list of strings as objects in yield! and for"
        )

    [<TestMethod>]
    member _.``for adds the elements of the body for each item`` () =
        let set = hashSet {
            for item in -2 .. 2 do
                yield item * item
        }

        CollectionAssert.AreEqual ([| 0; 1; 4 |], sorted set, "hashSet must run the body of for for each item")

    [<TestMethod>]
    member _.``while adds the elements while the guard holds`` () =
        let mutable count = 0

        let set = hashSet {
            while count < 3 do
                count <- count + 1
                yield count
        }

        CollectionAssert.AreEqual ([| 1; 2; 3 |], sorted set, "hashSet must run the body of while while the guard holds")

    [<TestMethod>]
    member _.``if without else and following elements combine`` () =
        let set = hashSet {
            if false then
                yield 0

            yield 1

            if true then
                yield 2
        }

        CollectionAssert.AreEqual ([| 1; 2 |], sorted set, "hashSet must combine conditional and plain elements")

    [<TestMethod>]
    member _.``try with adds the elements of the handler after an exception`` () =
        let set = hashSet {
            try
                yield 1
                failwith "fails"
            with _ ->
                yield 2
        }

        CollectionAssert.AreEqual (
            [| 1; 2 |],
            sorted set,
            "hashSet must keep the elements before the exception and add the handler's"
        )

    [<TestMethod>]
    member _.``try finally runs the compensation`` () =
        let mutable compensated = false

        let set = hashSet {
            try
                yield 1
            finally
                compensated <- true
        }

        Assert.IsTrue (compensated, "hashSet must run the compensation of try finally")
        CollectionAssert.AreEqual ([| 1 |], sorted set, "hashSet must keep the elements of the body of try finally")

    [<TestMethod>]
    member _.``use disposes the resource after the body`` () =
        let mutable disposed = false

        let set = hashSet {
            use _resource = {
                new IDisposable with
                    member _.Dispose () = disposed <- true
            }

            yield 1
        }

        Assert.IsTrue (disposed, "hashSet must dispose the resource of use")
        CollectionAssert.AreEqual ([| 1 |], sorted set, "hashSet must keep the elements of the body of use")

    [<TestMethod>]
    member _.``hashSet compares with the default comparer`` () =
        let set = hashSet {
            "apple"
            "APPLE"
        }

        Assert.AreSame (
            EqualityComparer<string>.Default :> IEqualityComparer<string>,
            HashSet.comparer set,
            "hashSet must compare with the default comparer"
        )

        Assert.HasCount (2, set, "hashSet must tell the case apart")

    [<TestMethod>]
    member _.``a builder with a comparer builds hash sets that compare with it`` () =
        let ignoreCase : IEqualityComparer<string> = StringComparer.OrdinalIgnoreCase

        let set = HashSetCE ignoreCase {
            "apple"
            "APPLE"
            "Banana"
        }

        Assert.AreSame (ignoreCase, HashSet.comparer set, "HashSetCE must build hash sets with its comparer")
        CollectionAssert.AreEqual (
            [| "Banana"; "apple" |],
            sorted set,
            "HashSetCE must keep the first of the elements equal in its comparer"
        )

    [<TestMethod>]
    member _.``a builder rejects a null comparer when it is created`` () =
        let ex =
            Assert.ThrowsExactly<ArgumentNullException>(
                Action (fun () ->
                    HashSetCE (Unchecked.defaultof<IEqualityComparer<string>>)
                    |> ignore
                ),
                "HashSetCE must reject a null comparer"
            )

        Assert.AreEqual<string | null>("comparer", ex.ParamName, "HashSetCE must name the parameter comparer")
