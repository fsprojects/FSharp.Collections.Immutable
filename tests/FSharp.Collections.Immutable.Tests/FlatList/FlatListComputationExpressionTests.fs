namespace FSharp.Collections.Immutable.Tests

open System
open Microsoft.VisualStudio.TestTools.UnitTesting
open FSharp.Quotations
open FSharp.Collections.Immutable

/// The `flatList { }` computation expression.
[<TestClass; FlatListTestCategory; ComputationExpressionTestCategory>]
type FlatListComputationExpressionTests () =

    static let defaultArray = Unchecked.defaultof<FlatList<int>>

    /// The types of the first parameter of the For members that an expression calls, in the order of the loops: the
    /// compiler chooses the overload of For by the type of the source of the loop, so they tell which overload it chose.
    static let rec loopSources (expression : Expr) : Type list =
        match expression with
        | Patterns.Call (_, info, arguments) when info.Name = "For" ->
            info.GetParameters().[0].ParameterType
            :: List.collect loopSources arguments
        | ExprShape.ShapeVar _ -> []
        | ExprShape.ShapeLambda (_, body) -> loopSources body
        | ExprShape.ShapeCombination (_, arguments) -> List.collect loopSources arguments


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
    member _.``yield! rejects a default flat list of strings given as a sequence of objects`` () =
        let ex =
            Assert.ThrowsExactly<ArgumentNullException>(
                Action (fun () ->
                    flatList { yield! FlatListOracles.defaultStringsAsObjects }
                    |> ignore
                ),
                "flatList must reject a default flat list of strings in yield! of a sequence of objects"
            )

        Assert.AreEqual<string | null>("items", ex.ParamName, "flatList must name the parameter of the default flat list")

    [<TestMethod>]
    member _.``for adds the elements of the body for each item`` () =
        let array = flatList {
            for item in 1..3 do
                yield item * 10
        }

        CollectionAssert.AreEqual ([| 10; 20; 30 |], FlatList.toArray array, "flatList must run the body of for for each item")

    [<TestMethod>]
    member _.``for adds the elements of a flat list in order`` () =
        let array = flatList {
            for item in FlatList.ofArray [| 1; 2; 3 |] do
                yield item * 10
        }

        CollectionAssert.AreEqual (
            [| 10; 20; 30 |],
            FlatList.toArray array,
            "flatList must run the body of for for each item of a flat list"
        )

    [<TestMethod>]
    member _.``for adds the elements of an array in order`` () =
        let array = flatList {
            for item in [| 1; 2; 3 |] do
                yield item * 10
        }

        CollectionAssert.AreEqual (
            [| 10; 20; 30 |],
            FlatList.toArray array,
            "flatList must run the body of for for each item of an array"
        )

    [<TestMethod>]
    member _.``for adds the elements of a list and of a lazy sequence in order`` () =
        let fromList = flatList {
            for item in [ 1; 2; 3 ] do
                yield item * 10
        }

        let fromSequence = flatList {
            for item in Seq.map id [| 1; 2; 3 |] do
                yield item * 10
        }

        CollectionAssert.AreEqual (
            [| 10; 20; 30 |],
            FlatList.toArray fromList,
            "flatList must run the body of for for each item of a list"
        )

        CollectionAssert.AreEqual (
            [| 10; 20; 30 |],
            FlatList.toArray fromSequence,
            "flatList must run the body of for for each item of a lazy sequence"
        )

    [<TestMethod>]
    member _.``for adds the elements of the flat lists of a flat list in order`` () =
        let rows = FlatList.ofArray [| FlatList.ofArray [| 1; 2 |]; FlatList.empty; FlatList.ofArray [| 3 |] |]

        let array = flatList {
            for row in rows do
                for item in row do
                    yield item * 10
        }

        CollectionAssert.AreEqual (
            [| 10; 20; 30 |],
            FlatList.toArray array,
            "flatList must run the inner loop for each item of the outer loop, also when a flat list is empty"
        )

    [<TestMethod>]
    member _.``for gives the type of the items to its body for every kind of source`` () =
        // The body uses a member of the loop variable. The compiler can resolve it only when the overload that it chose
        // gives the loop variable its type, so the test does not compile when an overload loses the type of the items
        let names = [| "a"; "bb" |]

        let fromFlatList = flatList {
            for name in FlatList.ofArray names do
                yield name.Length
        }

        let fromArray = flatList {
            for name in names do
                yield name.Length
        }

        let fromList = flatList {
            for name in List.ofArray names do
                yield name.Length
        }

        let fromSequence = flatList {
            for name in Seq.map id names do
                yield name.Length
        }

        CollectionAssert.AreEqual (
            [| 1; 2 |],
            FlatList.toArray fromFlatList,
            "flatList must give the type of the items of a flat list to the body of for"
        )

        CollectionAssert.AreEqual (
            [| 1; 2 |],
            FlatList.toArray fromArray,
            "flatList must give the type of the items of an array to the body of for"
        )

        CollectionAssert.AreEqual (
            [| 1; 2 |],
            FlatList.toArray fromList,
            "flatList must give the type of the items of a list to the body of for"
        )

        CollectionAssert.AreEqual (
            [| 1; 2 |],
            FlatList.toArray fromSequence,
            "flatList must give the type of the items of a lazy sequence to the body of for"
        )

    [<TestMethod>]
    member _.``for chooses the overload of the type of its source`` () =
        let flatListSource = FlatList.ofArray [| 1; 2; 3 |]
        let arraySource = [| 1; 2; 3 |]
        let listSource = [ 1; 2; 3 ]
        let sequenceSource = Seq.map id arraySource
        let rows = FlatList.ofArray [| flatListSource |]

        Assert.AreEqual<Type list>(
            [ typeof<FlatList<int>> ],
            loopSources
                <@
                    flatList {
                        for item in flatListSource do
                            yield item
                    }
                @>,
            "for must take a flat list as a flat list, which does not box it"
        )

        Assert.AreEqual<Type list>(
            [ typeof<int array> ],
            loopSources
                <@
                    flatList {
                        for item in arraySource do
                            yield item
                    }
                @>,
            "for must take an array as an array"
        )

        Assert.AreEqual<Type list>(
            [ typeof<seq<int>> ],
            loopSources
                <@
                    flatList {
                        for item in listSource do
                            yield item
                    }
                @>,
            "for must take a list as a sequence"
        )

        Assert.AreEqual<Type list>(
            [ typeof<seq<int>> ],
            loopSources
                <@
                    flatList {
                        for item in sequenceSource do
                            yield item
                    }
                @>,
            "for must take a lazy sequence as a sequence"
        )

        Assert.AreEqual<Type list>(
            [ typeof<seq<int>> ],
            loopSources
                <@
                    flatList {
                        for item in (flatListSource :> seq<int>) do
                            yield item
                    }
                @>,
            "for must take a flat list that the code converted to a sequence as a sequence"
        )

        Assert.AreEqual<Type list>(
            [ typeof<FlatList<FlatList<int>>>; typeof<FlatList<int>> ],
            loopSources
                <@
                    flatList {
                        for row in rows do
                            for item in row do
                                yield item
                    }
                @>,
            "for must take the flat lists of a flat list as flat lists, the outer loop first"
        )

    [<TestMethod>]
    member _.``for rejects a default flat list`` () =
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
    member _.``for rejects a default flat list given as a sequence`` () =
        let ex =
            Assert.ThrowsExactly<ArgumentNullException>(
                Action (fun () ->
                    flatList {
                        for item in (defaultArray :> seq<int>) do
                            yield item
                    }
                    |> ignore
                ),
                "flatList must reject a default flat list that for takes as a sequence"
            )

        Assert.AreEqual<string | null>("source", ex.ParamName, "flatList must name the parameter of the default flat list")

    [<TestMethod>]
    member _.``for rejects a null array`` () =
        let ex =
            Assert.ThrowsExactly<ArgumentNullException>(
                Action (fun () ->
                    flatList {
                        for item in Unchecked.defaultof<int array> do
                            yield item
                    }
                    |> ignore
                ),
                "flatList must reject a null array in for"
            )

        Assert.AreEqual<string | null>("source", ex.ParamName, "flatList must name the parameter of the null array")

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
    member _.``for rejects a default flat list of strings given as a sequence of objects`` () =
        let ex =
            Assert.ThrowsExactly<ArgumentNullException>(
                Action (fun () ->
                    flatList {
                        for item in FlatListOracles.defaultStringsAsObjects do
                            yield item
                    }
                    |> ignore
                ),
                "flatList must reject a default flat list of strings in for over a sequence of objects"
            )

        Assert.AreEqual<string | null>("source", ex.ParamName, "flatList must name the parameter of the default flat list")

    [<TestMethod>]
    member _.``yield! and for take a flat list of strings that is not default as a sequence of objects`` () =
        let widened = FlatListOracles.stringsAsObjects (FlatList.ofArray [| "a"; "b" |])

        let array = flatList {
            yield! widened

            for item in widened do
                yield item
        }

        CollectionAssert.AreEqual (
            [| "a" :> obj; "b" :> obj; "a" :> obj; "b" :> obj |],
            FlatList.toArray array,
            "flatList must take the elements of a flat list of strings as objects in yield! and for"
        )

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
