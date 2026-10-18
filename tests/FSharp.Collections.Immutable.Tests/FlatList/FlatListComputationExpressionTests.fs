namespace FSharp.Collections.Immutable.Tests

open System
open FSharp.Collections.Immutable
open Microsoft.VisualStudio.TestTools.UnitTesting

/// The `flatList { }` computation expression.
[<TestClass; FlatListTestCategory; ComputationExpressionTestCategory>]
type FlatListComputationExpressionTests () =

    static let defaultArray = Unchecked.defaultof<FlatList<int>>

    [<TestMethod>]
    member _.``an expression without elements is empty`` () =
        let array : FlatList<int> = flatList { () }
        Assert.IsTrue (FlatList.isEmpty array, "flatList { () } must be empty")

    [<TestMethod>]
    member _.``yield adds the elements in order`` () =
        let array = flatList {
            yield 1
            yield 2
            yield 3
        }

        CollectionAssert.AreEqual ([| 1; 2; 3 |], FlatList.toArray array, "flatList must keep the order of yield")

    [<TestMethod>]
    member _.``implicit yields add the elements in order`` () =
        let array = flatList {
            1
            2
            3
        }

        CollectionAssert.AreEqual ([| 1; 2; 3 |], FlatList.toArray array, "flatList must keep the order of implicit yields")

    [<TestMethod>]
    member _.``yield! adds a sequence, an array, a flat list and another expression`` () =
        let inner = flatList { yield 7 }

        let array = flatList {
            yield! [ 1; 2 ]
            yield! [| 3; 4 |]
            yield! FlatList.ofArray [| 5; 6 |]
            yield! inner
        }

        CollectionAssert.AreEqual ([| 1; 2; 3; 4; 5; 6; 7 |], FlatList.toArray array, "flatList must add the elements of yield!")

    [<TestMethod>]
    member _.``yield! rejects a default flat list as the functions of FlatList do`` () =
        let ex =
            Assert.ThrowsExactly<ArgumentNullException>(
                Action (fun () -> flatList { yield! defaultArray } |> ignore),
                "flatList must reject a default flat list in yield!"
            )

        Assert.AreEqual<string | null>("items", ex.ParamName, "flatList must name the parameter of the default flat list")

    [<TestMethod>]
    member _.``yield! rejects a default flat list given as a sequence as FlatList.ofSeq does`` () =
        let ex =
            Assert.ThrowsExactly<ArgumentNullException>(
                Action (fun () -> flatList { yield! (defaultArray :> seq<int>) } |> ignore),
                "flatList must reject a default flat list in yield! of a sequence"
            )

        Assert.AreEqual<string | null>("items", ex.ParamName, "flatList must name the parameter of the default flat list")

    [<TestMethod>]
    member _.``yield! rejects a null sequence`` () =
        let ex =
            Assert.ThrowsExactly<ArgumentNullException>(
                Action (fun () -> flatList { yield! Unchecked.defaultof<seq<int>> } |> ignore),
                "flatList must reject a null sequence in yield!"
            )

        Assert.AreEqual<string | null>("items", ex.ParamName, "flatList must name the parameter of the null sequence")

    [<TestMethod>]
    member _.``for adds the elements of the body for each item`` () =
        let array = flatList {
            for item in 1..3 do
                yield item * 10
        }

        CollectionAssert.AreEqual ([| 10; 20; 30 |], FlatList.toArray array, "flatList must run the body of for for each item")

    [<TestMethod>]
    member _.``for rejects a default flat list, which it takes as a sequence`` () =
        let ex =
            Assert.ThrowsExactly<ArgumentNullException>(
                Action (fun () ->
                    flatList {
                        for item in defaultArray do
                            yield item
                    }
                    |> ignore
                ),
                "flatList must reject a default flat list in for"
            )

        Assert.AreEqual<string | null>("source", ex.ParamName, "flatList must name the parameter of the default flat list")

    [<TestMethod>]
    member _.``for rejects a null sequence`` () =
        let ex =
            Assert.ThrowsExactly<ArgumentNullException>(
                Action (fun () ->
                    flatList {
                        for item in Unchecked.defaultof<seq<int>> do
                            yield item
                    }
                    |> ignore
                ),
                "flatList must reject a null sequence in for"
            )

        Assert.AreEqual<string | null>("source", ex.ParamName, "flatList must name the parameter of the null sequence")

    [<TestMethod>]
    member _.``while adds the elements while the guard holds`` () =
        let mutable count = 0

        let array = flatList {
            while count < 3 do
                count <- count + 1
                yield count
        }

        CollectionAssert.AreEqual (
            [| 1; 2; 3 |],
            FlatList.toArray array,
            "flatList must run the body of while while the guard holds"
        )

    [<TestMethod>]
    member _.``if without else and following elements combine`` () =
        let array = flatList {
            if false then
                yield 0

            yield 1

            if true then
                yield 2
        }

        CollectionAssert.AreEqual ([| 1; 2 |], FlatList.toArray array, "flatList must combine conditional and plain elements")

    [<TestMethod>]
    member _.``try with adds the elements of the handler after an exception`` () =
        let array = flatList {
            try
                yield 1
                failwith "fails"
            with _ ->
                yield 2
        }

        CollectionAssert.AreEqual (
            [| 1; 2 |],
            FlatList.toArray array,
            "flatList must keep the elements before the exception and add the handler's"
        )

    [<TestMethod>]
    member _.``try finally runs the compensation`` () =
        let mutable compensated = false

        let array = flatList {
            try
                yield 1
            finally
                compensated <- true
        }

        Assert.IsTrue (compensated, "flatList must run the compensation of try finally")
        CollectionAssert.AreEqual ([| 1 |], FlatList.toArray array, "flatList must keep the elements of the body of try finally")

    [<TestMethod>]
    member _.``use disposes the resource after the body`` () =
        let mutable disposed = false

        let array = flatList {
            use _resource = {
                new IDisposable with
                    member _.Dispose () = disposed <- true
            }

            yield 1
        }

        Assert.IsTrue (disposed, "flatList must dispose the resource of use")
        CollectionAssert.AreEqual ([| 1 |], FlatList.toArray array, "flatList must keep the elements of the body of use")
