namespace FSharp.Collections.Immutable.Tests

open System
open FSharp.Collections.Immutable
open Microsoft.VisualStudio.TestTools.UnitTesting

/// The `stack { }` computation expression, whose first element ends on top.
[<TestClass; StackTestCategory; ComputationExpressionTestCategory>]
type StackComputationExpressionTests () =

    /// The expressions that give yield! a null input, by the shape of the input. The rows of the test are the names: a
    /// function has no readable name in a row.
    static let nullYields : Map<string, unit -> Stack<int>> =
        Map.ofArray [|
            "sequence", (fun () -> stack { yield! Unchecked.defaultof<seq<int>> })
            "array", (fun () -> stack { yield! Unchecked.defaultof<int array> })
            "stack", (fun () -> stack { yield! Unchecked.defaultof<Stack<int>> })
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
            "yield! (null sequence)", struct ("items", (fun () -> stack { yield! nullSequence } |> ignore))
            "yield! (default flat list)", struct ("items", (fun () -> stack { yield! defaultSequence } |> ignore))
            "yield! (default flat list of strings as objects)",
            struct ("items", (fun () -> stack { yield! widenedSequence } |> ignore))
            "for (null sequence)",
            struct ("source",
                    (fun () ->
                        stack {
                            for item in nullSequence do
                                yield item
                        }
                        |> ignore
                    ))
            "for (default flat list)",
            struct ("source",
                    (fun () ->
                        stack {
                            for item in defaultSequence do
                                yield item
                        }
                        |> ignore
                    ))
            "for (default flat list of strings as objects)",
            struct ("source",
                    (fun () ->
                        stack {
                            for item in widenedSequence do
                                yield item
                        }
                        |> ignore
                    ))
        |]

    /// The members of the builder that take a collection, called on the builder without running an expression: the builder
    /// checks the input when the expression calls the member, not when the collected elements are added to the buffer.
    static let builderCalls : Map<string, struct (string * (unit -> unit))> =
        let nullSequence = Unchecked.defaultof<seq<int>>
        let defaultSequence = Inputs.DefaultFlatListAsSeq<int>()

        Map.ofArray [|
            "YieldFrom (null sequence)", struct ("items", (fun () -> stack.YieldFrom nullSequence |> ignore))
            "YieldFrom (default flat list)", struct ("items", (fun () -> stack.YieldFrom defaultSequence |> ignore))
            "YieldFrom (null array)", struct ("items", (fun () -> stack.YieldFrom (Unchecked.defaultof<int array>) |> ignore))
            "YieldFrom (null stack)", struct ("items", (fun () -> stack.YieldFrom (Unchecked.defaultof<Stack<int>>) |> ignore))
            "For (null sequence)",
            struct ("source",
                    (fun () ->
                        stack.For (nullSequence, (fun item -> stack.Yield item))
                        |> ignore
                    ))
            "For (default flat list)",
            struct ("source",
                    (fun () ->
                        stack.For (defaultSequence, (fun item -> stack.Yield item))
                        |> ignore
                    ))
        |]

    /// The shapes of the null inputs of yield!, as the rows of their test.
    static member NullYields = nullYields.Keys

    /// The constructs and the inputs that take a sequence, as the rows of the test of the sequences that they reject.
    static member SequenceRejections = sequenceRejections.Keys

    /// The members of the builder that take a collection, as the rows of the test of the inputs that they reject.
    static member BuilderCalls = builderCalls.Keys

    [<TestMethod>]
    member _.``an expression without elements is empty`` () =
        let result : Stack<int> = stack { () }
        Assert.IsTrue (Stack.isEmpty result, "stack { () } must be empty")

    [<TestMethod>]
    member _.``yield adds the elements in order, the first one on top`` () =
        let result = stack {
            yield 1
            yield 2
            yield 3
        }

        CollectionAssert.AreEqual ([| 1; 2; 3 |], Stack.toArray result, "stack must keep the order of yield")
        Assert.AreEqual<int>(1, result.Peek (), "stack must put the first element on top")

        CollectionAssert.AreEqual (
            Stack.toArray (Stack.ofArray [| 1; 2; 3 |]),
            Stack.toArray result,
            "stack must build what Stack.ofArray builds"
        )

    [<TestMethod>]
    member _.``implicit yields add the elements in order`` () =
        let result = stack {
            1
            2
            3
        }

        CollectionAssert.AreEqual ([| 1; 2; 3 |], Stack.toArray result, "stack must keep the order of implicit yields")

    [<TestMethod>]
    member _.``yield! adds a sequence, an array, a stack and another expression in their order`` () =
        let inner = stack { yield 7 }

        let result = stack {
            yield! [ 1; 2 ]
            yield! [| 3; 4 |]
            yield! Stack.ofArray [| 5; 6 |]
            yield! inner
        }

        CollectionAssert.AreEqual (
            [| 1; 2; 3; 4; 5; 6; 7 |],
            Stack.toArray result,
            "stack must add the elements of yield! in their order"
        )

    [<TestMethod; DynamicData(nameof StackComputationExpressionTests.NullYields)>]
    member _.``yield! rejects a null input with the name of its parameter`` (shape : string) =
        let ex =
            Assert.ThrowsExactly<ArgumentNullException>(
                Action (fun () -> nullYields[shape]() |> ignore),
                $"stack must reject a null %s{shape} in yield!"
            )

        Assert.AreEqual<string | null>("items", ex.ParamName, $"stack must name the parameter of the null %s{shape}")

    [<TestMethod; DynamicData(nameof StackComputationExpressionTests.SequenceRejections)>]
    member _.``yield! and for reject a null sequence, a default flat list and a default flat list of strings`` (name : string) =
        let struct (paramName, expression) = sequenceRejections[name]
        FlatListAssert.RejectsNull ($"stack with %s{name}", paramName, expression)

    [<TestMethod; DynamicData(nameof StackComputationExpressionTests.BuilderCalls)>]
    member _.``the builder checks its input when the expression calls the member`` (name : string) =
        let struct (paramName, call) = builderCalls[name]
        FlatListAssert.RejectsNull ($"StackCE.%s{name}", paramName, call)

    [<TestMethod>]
    member _.``yield! and for take a flat list that is not default as a sequence`` () =
        let flat = FlatList.ofArray [| 1; 2 |] :> seq<int>
        let widened = FlatListOracles.stringsAsObjects (FlatList.ofArray [| "a"; "b" |])

        let numbers = stack {
            yield! flat

            for item in flat do
                yield item * 10
        }

        CollectionAssert.AreEqual (
            [| 1; 2; 10; 20 |],
            Stack.toArray numbers,
            "stack must take the elements of a flat list in yield! and for"
        )

        let objects = stack {
            yield! widened

            for item in widened do
                yield item
        }

        CollectionAssert.AreEqual (
            [| "a" :> obj; "b" :> obj; "a" :> obj; "b" :> obj |],
            Stack.toArray objects,
            "stack must take the elements of a flat list of strings as objects in yield! and for"
        )

    [<TestMethod>]
    member _.``for adds the elements of the body for each item`` () =
        let result = stack {
            for item in 1..3 do
                yield item * 10
        }

        CollectionAssert.AreEqual ([| 10; 20; 30 |], Stack.toArray result, "stack must run the body of for for each item")

    [<TestMethod>]
    member _.``while adds the elements while the guard holds`` () =
        let mutable count = 0

        let result = stack {
            while count < 3 do
                count <- count + 1
                yield count
        }

        CollectionAssert.AreEqual ([| 1; 2; 3 |], Stack.toArray result, "stack must run the body of while while the guard holds")

    [<TestMethod>]
    member _.``if without else and following elements combine`` () =
        let result = stack {
            if false then
                yield 0

            yield 1

            if true then
                yield 2
        }

        CollectionAssert.AreEqual ([| 1; 2 |], Stack.toArray result, "stack must combine conditional and plain elements")

    [<TestMethod>]
    member _.``try with adds the elements of the handler after an exception`` () =
        let result = stack {
            try
                yield 1
                failwith "fails"
            with _ ->
                yield 2
        }

        CollectionAssert.AreEqual (
            [| 1; 2 |],
            Stack.toArray result,
            "stack must keep the elements before the exception and add the handler's"
        )

    [<TestMethod>]
    member _.``try finally runs the compensation`` () =
        let mutable compensated = false

        let result = stack {
            try
                yield 1
            finally
                compensated <- true
        }

        Assert.IsTrue (compensated, "stack must run the compensation of try finally")
        CollectionAssert.AreEqual ([| 1 |], Stack.toArray result, "stack must keep the elements of the body of try finally")

    [<TestMethod>]
    member _.``use disposes the resource after the body`` () =
        let mutable disposed = false

        let result = stack {
            use _resource = {
                new IDisposable with
                    member _.Dispose () = disposed <- true
            }

            yield 1
        }

        Assert.IsTrue (disposed, "stack must dispose the resource of use")
        CollectionAssert.AreEqual ([| 1 |], Stack.toArray result, "stack must keep the elements of the body of use")

    [<TestMethod>]
    member _.``each run collects the elements again`` () =
        let mutable runs = 0

        let build () = stack {
            runs <- runs + 1
            yield runs
        }

        CollectionAssert.AreEqual ([| 1 |], Stack.toArray (build ()), "stack must run the body when it builds the stack")
        CollectionAssert.AreEqual ([| 2 |], Stack.toArray (build ()), "stack must run the body again for the next stack")
