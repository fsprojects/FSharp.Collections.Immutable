namespace FSharp.Collections.Immutable.Tests

open System.Collections.Immutable
open FSharp.Collections.Immutable
open FSharp.Collections.Immutable.Tuple
open Microsoft.VisualStudio.TestTools.UnitTesting

// The type annotations of the results are the proof that the functions of the Tuple namespace shadow the default ones.

/// The `IStack` module of the `Tuple` namespace.
[<TestClass; IStackTestCategory; TupleNamespaceTestCategory; PatternMatchingTestCategory>]
type IStackTupleTests () =

    static let several : IImmutableStack<int> = Stack.ofArray Inputs.Several

    static let other : IImmutableStack<int> = Stack.ofArray Inputs.Other

    static let third : IImmutableStack<int> = Stack.ofArray Inputs.Third

    [<TestMethod>]
    member _.``tryUncons returns a value option of a reference tuple`` () =
        let found : (int * IImmutableStack<int>) voption = IStack.tryUncons several

        // A tuple compares an array by reference, so the first element and the other elements are compared apart
        match found with
        | ValueSome (head, tail) ->
            Assert.AreEqual<int>(Array.head Inputs.Several, head, "Tuple.IStack.tryUncons must return the first element")

            CollectionAssert.AreEqual (
                Array.tail Inputs.Several,
                IStack.toArray tail,
                "Tuple.IStack.tryUncons must return the other elements"
            )
        | ValueNone -> Assert.Fail "Tuple.IStack.tryUncons must return the elements of a stack with elements"

    [<TestMethod>]
    member _.``uncons returns a reference tuple`` () =
        let (head : int), (tail : IImmutableStack<int>) = IStack.uncons several
        Assert.AreEqual<int>(Array.head Inputs.Several, head, "Tuple.IStack.uncons must return the first element")
        CollectionAssert.AreEqual (
            Array.tail Inputs.Several,
            IStack.toArray tail,
            "Tuple.IStack.uncons must return the other elements"
        )

    [<TestMethod>]
    member _.``Cons is a struct partial pattern of reference tuples`` () =
        let pattern : IImmutableStack<int> -> (int * IImmutableStack<int>) voption = IStack.(|Cons|_|)

        match several with
        | IStack.Cons (head, tail) ->
            Assert.AreEqual<int>(Array.head Inputs.Several, head, "Tuple.IStack.Cons must match the first element")
            CollectionAssert.AreEqual (
                Array.tail Inputs.Several,
                IStack.toArray tail,
                "Tuple.IStack.Cons must match the other elements"
            )
        | _ -> Assert.Fail "Tuple.IStack.Cons must match a stack with elements"

        Assert.IsTrue ((pattern (IStack.clear several)).IsNone, "Tuple.IStack.Cons must not match an empty stack")

    [<TestMethod>]
    member _.``allPairs returns reference tuples`` () =
        let pairs : Stack<int * int> = IStack.allPairs several other

        CollectionAssert.AreEqual (
            Array.allPairs Inputs.Several Inputs.Other,
            Stack.toArray pairs,
            "Tuple.IStack.allPairs must pair as List.allPairs does"
        )

    [<TestMethod>]
    member _.``indexed returns reference tuples`` () =
        let pairs : Stack<int * int> = IStack.indexed several

        CollectionAssert.AreEqual (
            Array.indexed Inputs.Several,
            Stack.toArray pairs,
            "Tuple.IStack.indexed must index as List.indexed does"
        )

    [<TestMethod>]
    member _.``pairwise returns reference tuples`` () =
        let pairs : Stack<int * int> = IStack.pairwise several

        CollectionAssert.AreEqual (
            Array.pairwise Inputs.Several,
            Stack.toArray pairs,
            "Tuple.IStack.pairwise must pair as List.pairwise does"
        )

    [<TestMethod>]
    member _.``unzip takes and returns reference tuples`` () =
        let pairs : IImmutableStack<int * int> = IStack.zip several other
        let (first : Stack<int>), (second : Stack<int>) = IStack.unzip pairs
        CollectionAssert.AreEqual (Inputs.Several, Stack.toArray first, "Tuple.IStack.unzip must return the first components")
        CollectionAssert.AreEqual (Inputs.Other, Stack.toArray second, "Tuple.IStack.unzip must return the second components")

    [<TestMethod>]
    member _.``unzip3 takes and returns reference tuples`` () =
        let triples : IImmutableStack<int * int * int> = IStack.zip3 several other third
        let (first : Stack<int>), (second : Stack<int>), (last : Stack<int>) = IStack.unzip3 triples
        CollectionAssert.AreEqual (Inputs.Several, Stack.toArray first, "Tuple.IStack.unzip3 must return the first components")
        CollectionAssert.AreEqual (Inputs.Other, Stack.toArray second, "Tuple.IStack.unzip3 must return the second components")
        CollectionAssert.AreEqual (Inputs.Third, Stack.toArray last, "Tuple.IStack.unzip3 must return the third components")

    [<TestMethod>]
    member _.``zip returns reference tuples`` () =
        let pairs : Stack<int * int> = IStack.zip several other

        CollectionAssert.AreEqual (
            Array.zip Inputs.Several Inputs.Other,
            Stack.toArray pairs,
            "Tuple.IStack.zip must zip as List.zip does"
        )

    [<TestMethod>]
    member _.``zip and zip3 throw for different lengths as the defaults do`` () =
        let short : IImmutableStack<int> = Stack.singleton 1

        ExceptionAssert.Same (
            id,
            (fun () ->
                FSharp.Collections.Immutable.IStack.zip several short
                |> ignore
            ),
            (fun () -> IStack.zip several short |> ignore),
            "Tuple.IStack.zip must throw as the default zip does"
        )

        ExceptionAssert.Same (
            id,
            (fun () ->
                FSharp.Collections.Immutable.IStack.zip3 several short other
                |> ignore
            ),
            (fun () -> IStack.zip3 several short other |> ignore),
            "Tuple.IStack.zip3 must throw as the default zip3 does"
        )

    [<TestMethod>]
    member _.``zip3 returns reference tuples`` () =
        let triples : Stack<int * int * int> = IStack.zip3 several other third

        CollectionAssert.AreEqual (
            Array.zip3 Inputs.Several Inputs.Other Inputs.Third,
            Stack.toArray triples,
            "Tuple.IStack.zip3 must zip as List.zip3 does"
        )

    [<TestMethod>]
    member _.``mapFold takes and returns reference tuples`` () =
        let (results : Stack<int>), (state : int) =
            IStack.mapFold (fun state item -> item * state, state + 1) 1 several

        let expectedResults, expectedState =
            Array.mapFold (fun state item -> item * state, state + 1) 1 Inputs.Several

        CollectionAssert.AreEqual (expectedResults, Stack.toArray results, "Tuple.IStack.mapFold must map as List.mapFold does")
        Assert.AreEqual<int>(expectedState, state, "Tuple.IStack.mapFold must thread the state as List.mapFold does")

    [<TestMethod>]
    member _.``mapFoldBack takes and returns reference tuples`` () =
        let (results : Stack<int>), (state : int) =
            IStack.mapFoldBack (fun item state -> item * state, state + 1) several 1

        let expectedResults, expectedState =
            Array.mapFoldBack (fun item state -> item * state, state + 1) Inputs.Several 1

        CollectionAssert.AreEqual (
            expectedResults,
            Stack.toArray results,
            "Tuple.IStack.mapFoldBack must map as List.mapFoldBack does"
        )

        Assert.AreEqual<int>(expectedState, state, "Tuple.IStack.mapFoldBack must thread the state as List.mapFoldBack does")

    [<TestMethod>]
    member _.``countBy returns reference tuples`` () =
        let counts : Stack<int * int> = IStack.countBy (fun item -> item % 3) several

        CollectionAssert.AreEqual (
            Array.countBy (fun item -> item % 3) Inputs.Several,
            Stack.toArray counts,
            "Tuple.IStack.countBy must count as List.countBy does"
        )

    [<TestMethod>]
    member _.``groupBy returns reference tuples`` () =
        let groups : Stack<int * Stack<int>> = IStack.groupBy (fun item -> item % 3) several
        let expected = Array.groupBy (fun item -> item % 3) Inputs.Several

        // A group pairs its key with its elements, which a pair compares by reference, so the keys and the elements are
        // compared apart
        CollectionAssert.AreEqual (
            Array.map fst expected,
            groups |> Stack.toArray |> Array.map fst,
            "Tuple.IStack.groupBy must return the keys as List.groupBy does"
        )

        CollectionAssert.AreEqual (
            Array.map snd expected,
            groups
            |> Stack.toArray
            |> Array.map (fun (_, items) -> Stack.toArray items),
            "Tuple.IStack.groupBy must group the elements as List.groupBy does"
        )

    [<TestMethod>]
    member _.``partition returns a reference tuple`` () =
        let (accepted : IImmutableStack<int>), (rejected : IImmutableStack<int>) =
            IStack.partition (fun item -> item > 0) several

        let expectedAccepted, expectedRejected = Array.partition (fun item -> item > 0) Inputs.Several

        CollectionAssert.AreEqual (
            expectedAccepted,
            IStack.toArray accepted,
            "Tuple.IStack.partition must return the accepted elements"
        )

        CollectionAssert.AreEqual (
            expectedRejected,
            IStack.toArray rejected,
            "Tuple.IStack.partition must return the rejected elements"
        )

    [<TestMethod>]
    member _.``partitionWith returns a reference tuple`` () =
        let partitioner (item : int) =
            if item > 0 then
                Choice1Of2 item
            else
                Choice2Of2 (string item)

        let (first : Stack<int>), (second : Stack<string>) = IStack.partitionWith partitioner several
        let expectedFirst, expectedSecond = Array.partitionWith partitioner Inputs.Several
        CollectionAssert.AreEqual (expectedFirst, Stack.toArray first, "Tuple.IStack.partitionWith must return the first choices")

        CollectionAssert.AreEqual (
            expectedSecond,
            Stack.toArray second,
            "Tuple.IStack.partitionWith must return the second choices"
        )

    [<TestMethod>]
    member _.``splitAt returns a reference tuple`` () =
        let (before : IImmutableStack<int>), (after : IImmutableStack<int>) = IStack.splitAt 3 several
        let expectedBefore, expectedAfter = Array.splitAt 3 Inputs.Several
        CollectionAssert.AreEqual (expectedBefore, IStack.toArray before, "Tuple.IStack.splitAt must return the first part")
        CollectionAssert.AreEqual (expectedAfter, IStack.toArray after, "Tuple.IStack.splitAt must return the second part")

    [<TestMethod>]
    member _.``the functions without a tuple keep resolving to the default module`` () =
        let found : int voption = IStack.tryFind (fun item -> item > 6) several
        Assert.AreEqual<int voption>(ValueSome 8, found, "IStack.tryFind must resolve to the default module")

    [<TestMethod>]
    [<DataRow(nameof IStack.tryUncons, "TryPeekPop")>]
    [<DataRow(nameof IStack.uncons, "PeekPop")>]
    member _.``the stack functions compile with the names of the stack operations of the BCL``
        (name : string, compiledName : string)
        =
        Assert.AreEqual<string | null>(
            name,
            StackOps.sourceName "FSharp.Collections.Immutable.Tuple.IStack" compiledName,
            "Tuple.IStack must compile the function under the name of its stack operation in the BCL"
        )
