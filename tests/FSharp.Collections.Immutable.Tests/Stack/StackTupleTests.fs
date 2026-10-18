namespace FSharp.Collections.Immutable.Tests

open FSharp.Collections.Immutable
open FSharp.Collections.Immutable.Tuple
open Microsoft.VisualStudio.TestTools.UnitTesting

// The type annotations of the results are the proof that the functions of the Tuple namespace shadow the default ones.

/// The `Stack` module of the `Tuple` namespace.
[<TestClass; StackTestCategory; TupleNamespaceTestCategory; PatternMatchingTestCategory>]
type StackTupleTests () =

    static let several = Stack.ofArray Inputs.Several

    static let other = Stack.ofArray Inputs.Other

    static let third = Stack.ofArray Inputs.Third

    [<TestMethod>]
    member _.``unfold takes a generator of value options of reference tuples`` () =
        let generator (state : int) =
            if state > 0 then
                ValueSome (state, state - 1)
            else
                ValueNone

        let generated : Stack<int> = Stack.unfold generator 3
        CollectionAssert.AreEqual (
            [| 3; 2; 1 |],
            Stack.toArray generated,
            "Tuple.Stack.unfold must generate the elements, the first one on top"
        )

    [<TestMethod>]
    member _.``tryUncons returns a value option of a reference tuple`` () =
        let found : (int * Stack<int>) voption = Stack.tryUncons several

        // A tuple compares an array by reference, so the first element and the other elements are compared apart
        match found with
        | ValueSome (head, tail) ->
            Assert.AreEqual<int>(Array.head Inputs.Several, head, "Tuple.Stack.tryUncons must return the first element")

            CollectionAssert.AreEqual (
                Array.tail Inputs.Several,
                Stack.toArray tail,
                "Tuple.Stack.tryUncons must return the other elements"
            )
        | ValueNone -> Assert.Fail "Tuple.Stack.tryUncons must return the elements of a stack with elements"

    [<TestMethod>]
    member _.``uncons returns a reference tuple`` () =
        let (head : int), (tail : Stack<int>) = Stack.uncons several
        Assert.AreEqual<int>(Array.head Inputs.Several, head, "Tuple.Stack.uncons must return the first element")
        CollectionAssert.AreEqual (
            Array.tail Inputs.Several,
            Stack.toArray tail,
            "Tuple.Stack.uncons must return the other elements"
        )

    [<TestMethod>]
    member _.``uncons throws as the default does`` () =
        ExceptionAssert.Same (
            id,
            (fun () ->
                FSharp.Collections.Immutable.Stack.uncons Stack.empty<int>
                |> ignore
            ),
            (fun () -> Stack.uncons Stack.empty<int> |> ignore),
            "Tuple.Stack.uncons must throw as the default uncons does"
        )

    [<TestMethod>]
    member _.``Cons is a struct partial pattern of reference tuples`` () =
        let pattern : Stack<int> -> (int * Stack<int>) voption = Stack.(|Cons|_|)

        match several with
        | Stack.Cons (head, tail) ->
            Assert.AreEqual<int>(Array.head Inputs.Several, head, "Tuple.Stack.Cons must match the first element")
            CollectionAssert.AreEqual (
                Array.tail Inputs.Several,
                Stack.toArray tail,
                "Tuple.Stack.Cons must match the other elements"
            )
        | _ -> Assert.Fail "Tuple.Stack.Cons must match a stack with elements"

        Assert.IsTrue ((pattern Stack.empty).IsNone, "Tuple.Stack.Cons must not match an empty stack")

    [<TestMethod>]
    member _.``allPairs returns reference tuples`` () =
        let pairs : Stack<int * int> = Stack.allPairs several other

        CollectionAssert.AreEqual (
            Array.allPairs Inputs.Several Inputs.Other,
            Stack.toArray pairs,
            "Tuple.Stack.allPairs must pair as List.allPairs does"
        )

    [<TestMethod>]
    member _.``indexed returns reference tuples`` () =
        let pairs : Stack<int * int> = Stack.indexed several

        CollectionAssert.AreEqual (
            Array.indexed Inputs.Several,
            Stack.toArray pairs,
            "Tuple.Stack.indexed must index as List.indexed does"
        )

    [<TestMethod>]
    member _.``pairwise returns reference tuples`` () =
        let pairs : Stack<int * int> = Stack.pairwise several

        CollectionAssert.AreEqual (
            Array.pairwise Inputs.Several,
            Stack.toArray pairs,
            "Tuple.Stack.pairwise must pair as List.pairwise does"
        )

    [<TestMethod>]
    member _.``unzip takes and returns reference tuples`` () =
        let (first : Stack<int>), (second : Stack<int>) = Stack.unzip (Stack.zip several other)
        CollectionAssert.AreEqual (Inputs.Several, Stack.toArray first, "Tuple.Stack.unzip must return the first components")
        CollectionAssert.AreEqual (Inputs.Other, Stack.toArray second, "Tuple.Stack.unzip must return the second components")

    [<TestMethod>]
    member _.``unzip3 takes and returns reference tuples`` () =
        let (first : Stack<int>), (second : Stack<int>), (last : Stack<int>) =
            Stack.unzip3 (Stack.zip3 several other third)

        CollectionAssert.AreEqual (Inputs.Several, Stack.toArray first, "Tuple.Stack.unzip3 must return the first components")
        CollectionAssert.AreEqual (Inputs.Other, Stack.toArray second, "Tuple.Stack.unzip3 must return the second components")
        CollectionAssert.AreEqual (Inputs.Third, Stack.toArray last, "Tuple.Stack.unzip3 must return the third components")

    [<TestMethod>]
    member _.``zip returns reference tuples`` () =
        let pairs : Stack<int * int> = Stack.zip several other

        CollectionAssert.AreEqual (
            Array.zip Inputs.Several Inputs.Other,
            Stack.toArray pairs,
            "Tuple.Stack.zip must zip as List.zip does"
        )

    [<TestMethod>]
    member _.``zip and zip3 throw for different lengths as the defaults do`` () =
        let short = Stack.singleton 1

        ExceptionAssert.Same (
            id,
            (fun () ->
                FSharp.Collections.Immutable.Stack.zip several short
                |> ignore
            ),
            (fun () -> Stack.zip several short |> ignore),
            "Tuple.Stack.zip must throw as the default zip does"
        )

        ExceptionAssert.Same (
            id,
            (fun () ->
                FSharp.Collections.Immutable.Stack.zip3 several short other
                |> ignore
            ),
            (fun () -> Stack.zip3 several short other |> ignore),
            "Tuple.Stack.zip3 must throw as the default zip3 does"
        )

    [<TestMethod>]
    member _.``zip3 returns reference tuples`` () =
        let triples : Stack<int * int * int> = Stack.zip3 several other third

        CollectionAssert.AreEqual (
            Array.zip3 Inputs.Several Inputs.Other Inputs.Third,
            Stack.toArray triples,
            "Tuple.Stack.zip3 must zip as List.zip3 does"
        )

    [<TestMethod>]
    member _.``mapFold takes and returns reference tuples`` () =
        let (results : Stack<int>), (state : int) =
            Stack.mapFold (fun state item -> item * state, state + 1) 1 several

        let expectedResults, expectedState =
            Array.mapFold (fun state item -> item * state, state + 1) 1 Inputs.Several

        CollectionAssert.AreEqual (expectedResults, Stack.toArray results, "Tuple.Stack.mapFold must map as List.mapFold does")
        Assert.AreEqual<int>(expectedState, state, "Tuple.Stack.mapFold must thread the state as List.mapFold does")

    [<TestMethod>]
    member _.``mapFoldBack takes and returns reference tuples`` () =
        let (results : Stack<int>), (state : int) =
            Stack.mapFoldBack (fun item state -> item * state, state + 1) several 1

        let expectedResults, expectedState =
            Array.mapFoldBack (fun item state -> item * state, state + 1) Inputs.Several 1

        CollectionAssert.AreEqual (
            expectedResults,
            Stack.toArray results,
            "Tuple.Stack.mapFoldBack must map as List.mapFoldBack does"
        )

        Assert.AreEqual<int>(expectedState, state, "Tuple.Stack.mapFoldBack must thread the state as List.mapFoldBack does")

    [<TestMethod>]
    member _.``countBy returns reference tuples`` () =
        let counts : Stack<int * int> = Stack.countBy (fun item -> item % 3) several

        CollectionAssert.AreEqual (
            Array.countBy (fun item -> item % 3) Inputs.Several,
            Stack.toArray counts,
            "Tuple.Stack.countBy must count as List.countBy does"
        )

    [<TestMethod>]
    member _.``groupBy returns reference tuples`` () =
        let groups : Stack<int * Stack<int>> = Stack.groupBy (fun item -> item % 3) several
        let expected = Array.groupBy (fun item -> item % 3) Inputs.Several

        // A group pairs its key with its elements, which a pair compares by reference, so the keys and the elements are
        // compared apart
        CollectionAssert.AreEqual (
            Array.map fst expected,
            groups |> Stack.toArray |> Array.map fst,
            "Tuple.Stack.groupBy must return the keys as List.groupBy does"
        )

        CollectionAssert.AreEqual (
            Array.map snd expected,
            groups
            |> Stack.toArray
            |> Array.map (fun (_, items) -> Stack.toArray items),
            "Tuple.Stack.groupBy must group the elements as List.groupBy does"
        )

    [<TestMethod>]
    member _.``partition returns a reference tuple`` () =
        let (accepted : Stack<int>), (rejected : Stack<int>) = Stack.partition (fun item -> item > 0) several
        let expectedAccepted, expectedRejected = Array.partition (fun item -> item > 0) Inputs.Several

        CollectionAssert.AreEqual (
            expectedAccepted,
            Stack.toArray accepted,
            "Tuple.Stack.partition must return the accepted elements"
        )

        CollectionAssert.AreEqual (
            expectedRejected,
            Stack.toArray rejected,
            "Tuple.Stack.partition must return the rejected elements"
        )

    [<TestMethod>]
    member _.``partitionWith returns a reference tuple`` () =
        let partitioner (item : int) =
            if item > 0 then
                Choice1Of2 item
            else
                Choice2Of2 (string item)

        let (first : Stack<int>), (second : Stack<string>) = Stack.partitionWith partitioner several
        let expectedFirst, expectedSecond = Array.partitionWith partitioner Inputs.Several
        CollectionAssert.AreEqual (expectedFirst, Stack.toArray first, "Tuple.Stack.partitionWith must return the first choices")

        CollectionAssert.AreEqual (
            expectedSecond,
            Stack.toArray second,
            "Tuple.Stack.partitionWith must return the second choices"
        )

    [<TestMethod>]
    member _.``splitAt returns a reference tuple`` () =
        let (before : Stack<int>), (after : Stack<int>) = Stack.splitAt 3 several
        let expectedBefore, expectedAfter = Array.splitAt 3 Inputs.Several
        CollectionAssert.AreEqual (expectedBefore, Stack.toArray before, "Tuple.Stack.splitAt must return the first part")
        CollectionAssert.AreEqual (expectedAfter, Stack.toArray after, "Tuple.Stack.splitAt must return the second part")

    [<TestMethod>]
    member _.``the functions without a tuple keep resolving to the default module`` () =
        let found : int voption = Stack.tryFind (fun item -> item > 6) several
        Assert.AreEqual<int voption>(ValueSome 8, found, "Stack.tryFind must resolve to the default module")

    [<TestMethod>]
    [<DataRow(nameof Stack.tryUncons, "TryPeekPop")>]
    [<DataRow(nameof Stack.uncons, "PeekPop")>]
    member _.``the stack functions compile with the names of the stack operations of the BCL``
        (name : string, compiledName : string)
        =
        Assert.AreEqual<string | null>(
            name,
            StackOps.sourceName "FSharp.Collections.Immutable.Tuple.Stack" compiledName,
            "Tuple.Stack must compile the function under the name of its stack operation in the BCL"
        )
