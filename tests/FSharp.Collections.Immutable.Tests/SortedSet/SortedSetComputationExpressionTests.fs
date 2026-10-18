namespace FSharp.Collections.Immutable.Tests

open System
open System.Collections.Generic
open FSharp.Collections.Immutable
open Microsoft.VisualStudio.TestTools.UnitTesting

/// The `sortedSet { }` computation expression and the builders with a comparer.
[<TestClass; SortedSetTestCategory; ComputationExpressionTestCategory>]
type SortedSetComputationExpressionTests () =

    /// A flat list of strings as a sequence of IComparable. A flat list of strings is also a sequence of IComparable, by
    /// the covariance of seq: the element type of the sequence is not the one of the flat list, and a type test for the flat
    /// list of IComparable does not see it. obj has no F# comparison, which the expression requires, and IComparable has.
    static let stringsAsComparables (strings : FlatList<string>) : seq<IComparable> =
        match box strings with
        | :? seq<IComparable> as widened -> widened
        | _ -> invalidOp "A flat list of strings must be a sequence of IComparable"

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
        let widenedElements = stringsAsComparables Unchecked.defaultof<FlatList<string>>

        Map.ofArray [|
            "yield! (null sequence)", struct ("items", (fun () -> sortedSet { yield! nullElements } |> ignore))
            "yield! (default flat list)", struct ("items", (fun () -> sortedSet { yield! defaultElements } |> ignore))
            "yield! (default flat list of strings as comparables)",
            struct ("items", (fun () -> sortedSet { yield! widenedElements } |> ignore))
            "yield! (null array)",
            struct ("items",
                    (fun () ->
                        sortedSet { yield! Unchecked.defaultof<int array> }
                        |> ignore
                    ))
            "yield! (null sorted set)",
            struct ("items",
                    (fun () ->
                        sortedSet { yield! Unchecked.defaultof<SortedSet<int>> }
                        |> ignore
                    ))
            "for (null sequence)",
            struct ("source",
                    (fun () ->
                        sortedSet {
                            for item in nullElements do
                                yield item
                        }
                        |> ignore
                    ))
            "for (default flat list)",
            struct ("source",
                    (fun () ->
                        sortedSet {
                            for item in defaultElements do
                                yield item
                        }
                        |> ignore
                    ))
            "for (default flat list of strings as comparables)",
            struct ("source",
                    (fun () ->
                        sortedSet {
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
        let set : SortedSet<int> = sortedSet { () }
        Assert.IsTrue (SortedSet.isEmpty set, "sortedSet { () } must be empty")

    [<TestMethod>]
    member _.``yield adds each distinct element once in ascending order`` () =
        let set = sortedSet {
            yield 2
            yield 1
            yield 2
        }

        CollectionAssert.AreEqual ([| 1; 2 |], SortedSet.toArray set, "sortedSet must add each distinct element once")

    [<TestMethod>]
    member _.``implicit yields add the elements in the order of Set`` () =
        let set = sortedSet {
            "b"
            "B"
            "a"
            "A"
        }

        CollectionAssert.AreEqual (
            Set.toArray (Set.ofArray [| "b"; "B"; "a"; "A" |]),
            SortedSet.toArray set,
            "sortedSet must order the elements of implicit yields as Set does"
        )

    [<TestMethod>]
    member _.``yield! adds a sequence, an array, a sorted set and another expression`` () =
        let inner = sortedSet { yield 7 }

        let set = sortedSet {
            // A list is the sequence that the overload of sequences takes
            yield! [ 2; 1 ]
            yield! [| 4; 3 |]
            yield! SortedSet.ofArray [| 6; 5; 1 |]
            yield! inner
        }

        CollectionAssert.AreEqual ([| 1..7 |], SortedSet.toArray set, "sortedSet must add the elements of yield!")

    [<TestMethod; DynamicData(nameof SortedSetComputationExpressionTests.Rejections)>]
    member _.``yield! and for reject a null input and a default flat list given as a sequence`` (name : string) =
        let struct (paramName, actual) = rejections[name]
        FlatListAssert.RejectsNull ($"sortedSet {{ %s{name} }}", paramName, actual)

    [<TestMethod>]
    member _.``yield! and for take a flat list of strings that is not default as a sequence of comparables`` () =
        let widened = stringsAsComparables (FlatList.ofArray [| "b"; "a" |])

        let set = sortedSet {
            yield! widened

            for item in widened do
                yield item
        }

        CollectionAssert.AreEqual (
            [| ("a" :> IComparable); ("b" :> IComparable) |],
            SortedSet.toArray set,
            "sortedSet must take the elements of a flat list of strings as comparables in yield! and for"
        )

    [<TestMethod>]
    member _.``for adds the elements of the body for each item`` () =
        let set = sortedSet {
            for item in -2 .. 2 do
                yield item * item
        }

        CollectionAssert.AreEqual ([| 0; 1; 4 |], SortedSet.toArray set, "sortedSet must run the body of for for each item")

    [<TestMethod>]
    member _.``while adds the elements while the guard holds`` () =
        let mutable count = 3

        let set = sortedSet {
            while count > 0 do
                yield count
                count <- count - 1
        }

        CollectionAssert.AreEqual (
            [| 1; 2; 3 |],
            SortedSet.toArray set,
            "sortedSet must run the body of while while the guard holds"
        )

    [<TestMethod>]
    member _.``if without else and following elements combine`` () =
        let set = sortedSet {
            if false then
                yield 0

            yield 2

            if true then
                yield 1
        }

        CollectionAssert.AreEqual ([| 1; 2 |], SortedSet.toArray set, "sortedSet must combine conditional and plain elements")

    [<TestMethod>]
    member _.``try with adds the elements of the handler after an exception`` () =
        let set = sortedSet {
            try
                yield 2
                failwith "fails"
            with _ ->
                yield 1
        }

        CollectionAssert.AreEqual (
            [| 1; 2 |],
            SortedSet.toArray set,
            "sortedSet must keep the elements before the exception and add the handler's"
        )

    [<TestMethod>]
    member _.``try finally runs the compensation`` () =
        let mutable compensated = false

        let set = sortedSet {
            try
                yield 1
            finally
                compensated <- true
        }

        Assert.IsTrue (compensated, "sortedSet must run the compensation of try finally")
        CollectionAssert.AreEqual ([| 1 |], SortedSet.toArray set, "sortedSet must keep the elements of the body of try finally")

    [<TestMethod>]
    member _.``use disposes the resource after the body`` () =
        let mutable disposed = false

        let set = sortedSet {
            use _resource = {
                new IDisposable with
                    member _.Dispose () = disposed <- true
            }

            yield 1
        }

        Assert.IsTrue (disposed, "sortedSet must dispose the resource of use")
        CollectionAssert.AreEqual ([| 1 |], SortedSet.toArray set, "sortedSet must keep the elements of the body of use")

    [<TestMethod>]
    member _.``sortedSet compares with F# generic comparison`` () =
        let set = sortedSet {
            "apple"
            "APPLE"
        }

        Assert.AreSame (
            LanguagePrimitives.FastGenericComparer<string>,
            SortedSet.comparer set,
            "sortedSet must compare with F# generic comparison"
        )

        CollectionAssert.AreEqual ([| "APPLE"; "apple" |], SortedSet.toArray set, "sortedSet must order ordinally")

    [<TestMethod>]
    member _.``a builder with a comparer builds sorted sets that order with it`` () =
        let ignoreCase : IComparer<string> = StringComparer.OrdinalIgnoreCase

        let set = SortedSetCE ignoreCase {
            "cherry"
            "apple"
            "APPLE"
            "Banana"
        }

        Assert.AreSame (ignoreCase, SortedSet.comparer set, "SortedSetCE must build sorted sets with its comparer")

        CollectionAssert.AreEqual (
            [| "apple"; "Banana"; "cherry" |],
            SortedSet.toArray set,
            "SortedSetCE must order with its comparer and keep the first of the elements equal in it"
        )

    [<TestMethod>]
    member _.``a builder rejects a null comparer when it is created`` () =
        let ex =
            Assert.ThrowsExactly<ArgumentNullException>(
                Action (fun () ->
                    SortedSetCE (Unchecked.defaultof<IComparer<string>>)
                    |> ignore
                ),
                "SortedSetCE must reject a null comparer"
            )

        Assert.AreEqual<string | null>("comparer", ex.ParamName, "SortedSetCE must name the parameter comparer")
