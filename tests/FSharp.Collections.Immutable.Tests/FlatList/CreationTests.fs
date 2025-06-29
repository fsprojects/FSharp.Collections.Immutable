namespace FSharp.Collections.Immutable.Tests.FlatList

open System
open System.Collections.Immutable
open System.Collections.Generic
open Microsoft.VisualStudio.TestTools.UnitTesting
open FSharp.Collections.Immutable

[<TestClass>]
type CreationTests () =

    [<TestMethod>]
    member _.``empty returns empty FlatList`` () =
        let empty = FlatList.empty<int>
        Assert.IsTrue (empty.IsEmpty)
        Assert.AreEqual<int> (0, empty.Length)

    [<TestMethod>]
    member _.``singleton creates FlatList with one element`` () =
        let flatList = FlatList.singleton 42

        Assert.AreEqual<int> (1, flatList.Length)
        Assert.AreEqual<int> (42, flatList.[0])

    [<TestMethod>]
    member _.``ofArray converts array to FlatList`` () =
        let arr = [| 1; 2; 3 |]
        let flatList = FlatList.ofArray arr

        Assert.AreEqual<int> (arr.Length, flatList.Length)
        for i = 0 to arr.Length - 1 do
            Assert.AreEqual<int> (arr.[i], flatList.[i])

    [<TestMethod>]
    member _.``ofSeq converts sequence to FlatList`` () =
        let seq = seq {
            1
            2
            3
        }
        let flatList = FlatList.ofSeq seq
        let expected = [| 1; 2; 3 |]

        Assert.AreEqual<int> (expected.Length, flatList.Length)
        for i = 0 to expected.Length - 1 do
            Assert.AreEqual<int> (expected.[i], flatList.[i])

    [<TestMethod>]
    member _.``ofList converts list to FlatList`` () =
        let list = [ 1; 2; 3 ]
        let flatList = FlatList.ofList list

        Assert.AreEqual<int> (list.Length, flatList.Length)
        for i = 0 to list.Length - 1 do
            Assert.AreEqual<int> (list.[i], flatList.[i])

    [<TestMethod>]
    member _.``init creates initialized FlatList`` () =
        let flatList = FlatList.init 5 (fun i -> i * 2)
        let expected = [| 0; 2; 4; 6; 8 |]

        Assert.AreEqual<int> (expected.Length, flatList.Length)
        for i = 0 to expected.Length - 1 do
            Assert.AreEqual<int> (expected.[i], flatList.[i])

    [<TestMethod>]
    [<ExpectedException(typeof<ArgumentException>)>]
    member _.``init throws for negative count`` () = FlatList.init -1 id |> ignore

    [<TestMethod>]
    member _.``create makes FlatList with repeated values`` () =
        let flatList = FlatList.create 3 "test"

        Assert.AreEqual<int> (3, flatList.Length)
        for i = 0 to flatList.Length - 1 do
            Assert.AreEqual<string> ("test", flatList.[i])

    [<TestMethod>]
    member _.``replicate makes FlatList with repeated values`` () =
        let flatList = FlatList.replicate 3 "test"

        Assert.AreEqual<int> (3, flatList.Length)
        for i = 0 to flatList.Length - 1 do
            Assert.AreEqual<string> ("test", flatList.[i])

    [<TestMethod>]
    member _.``toSeq converts FlatList to sequence`` () =
        let original = [| 1; 2; 3 |]
        let flatList = FlatList.ofArray original
        let seq = FlatList.toSeq flatList

        let result = Seq.toArray seq
        CollectionAssert.AreEqual (original, result)

    [<TestMethod>]
    member _.``toArray converts FlatList to array`` () =
        let original = [| 1; 2; 3 |]
        let flatList = FlatList.ofArray original
        let result = FlatList.toArray flatList

        CollectionAssert.AreEqual (original, result)

    [<TestMethod>]
    member _.``toList converts FlatList to list`` () =
        let original = [| 1; 2; 3 |]
        let flatList = FlatList.ofArray original
        let result = FlatList.toList flatList

        Assert.AreEqual<int> (original.Length, result.Length)
        for i = 0 to original.Length - 1 do
            Assert.AreEqual<int> (original.[i], result.[i])

    [<TestMethod>]
    member _.``copy makes a new FlatList with same elements`` () =
        let original = FlatList.ofArray [| 1; 2; 3 |]
        let copy = FlatList.copy original

        Assert.AreEqual<int> (original.Length, copy.Length)
        for i = 0 to original.Length - 1 do
            Assert.AreEqual<int> (original.[i], copy.[i])

        // Verify both reference the same array under the hood
        Assert.AreEqual<int> (original.GetHashCode (), copy.GetHashCode ())

    [<TestMethod>]
    member _.``zeroCreate creates FlatList with default values`` () =
        let flatList = FlatList.zeroCreate<int> 3

        Assert.AreEqual<int> (3, flatList.Length)
        for i = 0 to flatList.Length - 1 do
            Assert.AreEqual<int> (0, flatList.[i])

    [<TestMethod>]
    member _.``zeroCreate creates FlatList with reference type defaults`` () =
        let flatList = FlatList.zeroCreate<string> 2

        Assert.AreEqual<int> (2, flatList.Length)
        for i = 0 to flatList.Length - 1 do
            Assert.AreEqual<string> (null, flatList.[i])

    [<TestMethod>]
    member _.``ofOption creates singleton for Some`` () =
        let result = FlatList.ofOption (Some 42)

        Assert.AreEqual<int> (1, result.Length)
        Assert.AreEqual<int> (42, result.[0])

    [<TestMethod>]
    member _.``ofOption creates empty for None`` () =
        let result = FlatList.ofOption None

        Assert.AreEqual<int> (0, result.Length)
        Assert.IsTrue (FlatList.isEmpty result)

    [<TestMethod>]
    member _.``ofValueOption creates singleton for ValueSome`` () =
        let result = FlatList.ofValueOption (ValueSome 42)

        Assert.AreEqual<int> (1, result.Length)
        Assert.AreEqual<int> (42, result.[0])

    [<TestMethod>]
    member _.``ofValueOption creates empty for ValueNone`` () =
        let result = FlatList.ofValueOption ValueNone

        Assert.AreEqual<int> (0, result.Length)
        Assert.IsTrue (FlatList.isEmpty result)

    [<TestMethod>]
    member _.``toOption returns Some for singleton`` () =
        let flatList = FlatList.singleton 42
        let result = FlatList.toOption flatList

        Assert.AreEqual<int option> (Some 42, result)

    [<TestMethod>]
    member _.``toOption returns None for empty`` () =
        let flatList = FlatList.empty<int>
        let result = FlatList.toOption flatList

        Assert.AreEqual<int option> (None, result)

    [<TestMethod>]
    member _.``toOption returns None for multiple elements`` () =
        let flatList = FlatList.ofArray [| 1; 2 |]
        let result = FlatList.toOption flatList

        Assert.AreEqual<int option> (None, result)

    [<TestMethod>]
    member _.``toValueOption returns ValueSome for singleton`` () =
        let flatList = FlatList.singleton 42
        let result = FlatList.toValueOption flatList

        Assert.AreEqual<int voption> (ValueSome 42, result)

    [<TestMethod>]
    member _.``toValueOption returns ValueNone for empty`` () =
        let flatList = FlatList.empty<int>
        let result = FlatList.toValueOption flatList

        Assert.AreEqual<int voption> (ValueNone, result)

    [<TestMethod>]
    member _.``toValueOption returns ValueNone for multiple elements`` () =
        let flatList = FlatList.ofArray [| 1; 2 |]
        let result = FlatList.toValueOption flatList

        Assert.AreEqual<int voption> (ValueNone, result)
