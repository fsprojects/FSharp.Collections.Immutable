namespace FSharp.Collections.Immutable.Tests

open System
open FSharp.Collections.Immutable
open Microsoft.VisualStudio.TestTools.UnitTesting

/// The `indexedList { }` computation expression.
[<TestClass; IndexedListTestCategory; ComputationExpressionTestCategory>]
type IndexedListComputationExpressionTests () =

    /// The expressions that give yield! a null input, by the shape of the input. The rows of the test are the names: a
    /// function has no readable name in a row.
    static let nullYields : Map<string, unit -> IndexedList<int>> =
        Map.ofArray [|
            "sequence", (fun () -> indexedList { yield! Unchecked.defaultof<seq<int>> })
            "array", (fun () -> indexedList { yield! Unchecked.defaultof<int array> })
            "indexed list", (fun () -> indexedList { yield! Unchecked.defaultof<IndexedList<int>> })
        |]

    /// The sequences that yield! and for reject, by the construct and the input, with the parameter that names the input and
    /// the expression. A default flat list converts to a sequence without a check and fails when the sequence is
    /// enumerated, and the flat list of strings is a sequence of objects, which a type test for the flat list of objects
    /// does not see.
    static let sequenceRejections : Map<string, struct (string * (unit -> unit))> =
        let nullSequence = Unchecked.defaultof<seq<int>>
        let defaultSequence = Inputs.DefaultFlatListAsSeq<int>()
        let widenedSequence = FlatListOracles.defaultStringsAsObjects

        Map.ofArray [|
            "yield! (null sequence)", struct ("items", (fun () -> indexedList { yield! nullSequence } |> ignore))
            "yield! (default flat list)", struct ("items", (fun () -> indexedList { yield! defaultSequence } |> ignore))
            "yield! (default flat list of strings as objects)",
            struct ("items", (fun () -> indexedList { yield! widenedSequence } |> ignore))
            "for (null sequence)",
            struct ("source",
                    (fun () ->
                        indexedList {
                            for item in nullSequence do
                                yield item
                        }
                        |> ignore
                    ))
            "for (default flat list)",
            struct ("source",
                    (fun () ->
                        indexedList {
                            for item in defaultSequence do
                                yield item
                        }
                        |> ignore
                    ))
            "for (default flat list of strings as objects)",
            struct ("source",
                    (fun () ->
                        indexedList {
                            for item in widenedSequence do
                                yield item
                        }
                        |> ignore
                    ))
        |]

    /// The shapes of the null inputs of yield!, as the rows of their test.
    static member NullYields = nullYields.Keys

    /// The constructs and the inputs that take a sequence, as the rows of the test of the sequences that they reject.
    static member SequenceRejections = sequenceRejections.Keys

    [<TestMethod>]
    member _.``an expression without elements is empty`` () =
        let list : IndexedList<int> = indexedList { () }
        Assert.IsTrue (IndexedList.isEmpty list, "indexedList { () } must be empty")

    [<TestMethod>]
    member _.``yield adds the elements in order`` () =
        let list = indexedList {
            yield 1
            yield 2
            yield 3
        }

        CollectionAssert.AreEqual ([| 1; 2; 3 |], IndexedList.toArray list, "indexedList must keep the order of yield")

    [<TestMethod>]
    member _.``implicit yields add the elements in order`` () =
        let list = indexedList {
            1
            2
            3
        }

        CollectionAssert.AreEqual ([| 1; 2; 3 |], IndexedList.toArray list, "indexedList must keep the order of implicit yields")

    [<TestMethod>]
    member _.``yield! adds a sequence, an array, an indexed list and another expression`` () =
        let inner = indexedList { yield 7 }

        let list = indexedList {
            yield! [ 1; 2 ]
            yield! [| 3; 4 |]
            yield! IndexedList.ofArray [| 5; 6 |]
            yield! inner
        }

        CollectionAssert.AreEqual (
            [| 1; 2; 3; 4; 5; 6; 7 |],
            IndexedList.toArray list,
            "indexedList must add the elements of yield!"
        )

    [<TestMethod; DynamicData(nameof IndexedListComputationExpressionTests.NullYields)>]
    member _.``yield! rejects a null input with the name of its parameter`` (shape : string) =
        FlatListAssert.RejectsNull (
            $"indexedList with a null %s{shape} in yield!",
            "items",
            (fun () -> nullYields[shape]() |> ignore)
        )

    [<TestMethod; DynamicData(nameof IndexedListComputationExpressionTests.SequenceRejections)>]
    member _.``yield! and for reject a null sequence, a default flat list and a default flat list of strings`` (name : string) =
        let struct (paramName, expression) = sequenceRejections[name]
        FlatListAssert.RejectsNull ($"indexedList with %s{name}", paramName, expression)

    [<TestMethod>]
    member _.``yield! and for take a flat list of strings that is not default as a sequence of objects`` () =
        let widened = FlatListOracles.stringsAsObjects (FlatList.ofArray [| "a"; "b" |])

        let list = indexedList {
            yield! widened

            for item in widened do
                yield item
        }

        CollectionAssert.AreEqual (
            [| "a" :> obj; "b" :> obj; "a" :> obj; "b" :> obj |],
            IndexedList.toArray list,
            "indexedList must take the elements of a flat list of strings as objects in yield! and for"
        )

    [<TestMethod>]
    member _.``for adds the elements of the body for each item`` () =
        let list = indexedList {
            for item in 1..3 do
                yield item * 10
        }

        CollectionAssert.AreEqual (
            [| 10; 20; 30 |],
            IndexedList.toArray list,
            "indexedList must run the body of for for each item"
        )

    [<TestMethod>]
    member _.``while adds the elements while the guard holds`` () =
        let mutable count = 0

        let list = indexedList {
            while count < 3 do
                count <- count + 1
                yield count
        }

        CollectionAssert.AreEqual (
            [| 1; 2; 3 |],
            IndexedList.toArray list,
            "indexedList must run the body of while while the guard holds"
        )

    [<TestMethod>]
    member _.``if without else and following elements combine`` () =
        let list = indexedList {
            if false then
                yield 0

            yield 1

            if true then
                yield 2
        }

        CollectionAssert.AreEqual (
            [| 1; 2 |],
            IndexedList.toArray list,
            "indexedList must combine conditional and plain elements"
        )

    [<TestMethod>]
    member _.``try with adds the elements of the handler after an exception`` () =
        let list = indexedList {
            try
                yield 1
                failwith "fails"
            with _ ->
                yield 2
        }

        CollectionAssert.AreEqual (
            [| 1; 2 |],
            IndexedList.toArray list,
            "indexedList must keep the elements before the exception and add the handler's"
        )

    [<TestMethod>]
    member _.``try finally runs the compensation`` () =
        let mutable compensated = false

        let list = indexedList {
            try
                yield 1
            finally
                compensated <- true
        }

        Assert.IsTrue (compensated, "indexedList must run the compensation of try finally")
        CollectionAssert.AreEqual (
            [| 1 |],
            IndexedList.toArray list,
            "indexedList must keep the elements of the body of try finally"
        )

    [<TestMethod>]
    member _.``use disposes the resource after the body`` () =
        let mutable disposed = false

        let list = indexedList {
            use _resource = {
                new IDisposable with
                    member _.Dispose () = disposed <- true
            }

            yield 1
        }

        Assert.IsTrue (disposed, "indexedList must dispose the resource of use")
        CollectionAssert.AreEqual ([| 1 |], IndexedList.toArray list, "indexedList must keep the elements of the body of use")
