namespace FSharp.Collections.Immutable.Tests

open System
open FSharp.Collections.Immutable
open Microsoft.VisualStudio.TestTools.UnitTesting

/// The `flatList { }` computation expression.
[<TestClass; FlatListTestCategory; ComputationExpressionTestCategory>]
type FlatListComputationExpressionTests () =

    [<TestMethod; TestCategory(nameof flatList.Zero)>]
    member _.``an expression without elements is empty`` () =
        let array : FlatList<int> = flatList { () }
        Assert.IsTrue (FlatList.isEmpty array, "flatList { () } must be empty")

    [<TestMethod; TestCategory(nameof flatList.Yield)>]
    member _.``yield adds the elements in order`` () =
        let array = flatList {
            yield 1
            yield 2
            yield 3
        }

        Assert.AreEqual<int list>([ 1; 2; 3 ], FlatList.toList array, "flatList must keep the order of yield")

    [<TestMethod; TestCategory(nameof flatList.Yield)>]
    member _.``implicit yields add the elements in order`` () =
        let array = flatList {
            1
            2
            3
        }

        Assert.AreEqual<int list>([ 1; 2; 3 ], FlatList.toList array, "flatList must keep the order of implicit yields")

    // nameof cannot name the overloaded YieldFrom, so the category is the expression itself
    [<TestMethod; TestCategory(nameof flatList)>]
    member _.``yield! adds a sequence, an array, a flat list and another expression`` () =
        let inner = flatList { yield 7 }

        let array = flatList {
            yield! [ 1; 2 ]
            yield! [| 3; 4 |]
            yield! FlatList.ofList [ 5; 6 ]
            yield! inner
        }

        Assert.AreEqual<int list>([ 1; 2; 3; 4; 5; 6; 7 ], FlatList.toList array, "flatList must add the elements of yield!")

    [<TestMethod; TestCategory(nameof flatList.For)>]
    member _.``for adds the elements of the body for each item`` () =
        let array = flatList {
            for item in 1..3 do
                yield item * 10
        }

        Assert.AreEqual<int list>([ 10; 20; 30 ], FlatList.toList array, "flatList must run the body of for for each item")

    [<TestMethod; TestCategory(nameof flatList.While)>]
    member _.``while adds the elements while the guard holds`` () =
        let mutable count = 0

        let array = flatList {
            while count < 3 do
                count <- count + 1
                yield count
        }

        Assert.AreEqual<int list>([ 1; 2; 3 ], FlatList.toList array, "flatList must run the body of while while the guard holds")

    [<TestMethod; TestCategory(nameof flatList.Combine)>]
    member _.``if without else and following elements combine`` () =
        let array = flatList {
            if false then
                yield 0

            yield 1

            if true then
                yield 2
        }

        Assert.AreEqual<int list>([ 1; 2 ], FlatList.toList array, "flatList must combine conditional and plain elements")

    [<TestMethod; TestCategory(nameof flatList.TryWith)>]
    member _.``try with adds the elements of the handler after an exception`` () =
        let array = flatList {
            try
                yield 1
                failwith "fails"
            with _ ->
                yield 2
        }

        Assert.AreEqual<int list>(
            [ 1; 2 ],
            FlatList.toList array,
            "flatList must keep the elements before the exception and add the handler's"
        )

    [<TestMethod; TestCategory(nameof flatList.TryFinally)>]
    member _.``try finally runs the compensation`` () =
        let mutable compensated = false

        let array = flatList {
            try
                yield 1
            finally
                compensated <- true
        }

        Assert.IsTrue (compensated, "flatList must run the compensation of try finally")
        Assert.AreEqual<int list>([ 1 ], FlatList.toList array, "flatList must keep the elements of the body of try finally")

    [<TestMethod; TestCategory(nameof flatList.Using)>]
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
        Assert.AreEqual<int list>([ 1 ], FlatList.toList array, "flatList must keep the elements of the body of use")
