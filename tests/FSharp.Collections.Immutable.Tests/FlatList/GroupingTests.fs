namespace FSharp.Collections.Immutable.Tests.FlatList

open System
open System.Collections.Immutable
open System.Collections.Generic
open Microsoft.VisualStudio.TestTools.UnitTesting
open FSharp.Collections.Immutable
open FSharp.Collections.Immutable.Tests

[<TestClass>]
type GroupingTests () =

    [<TestMethod>]
    member _.``countBy groups and counts elements`` () =
        let flatList = FlatList.ofArray [| 1; 2; 3; 1; 2; 5 |]
        let result = FlatList.countBy id flatList

        Assert.AreEqual<int> (4, result.Length)

        // Find the counts for each key
        let countFor key =
            result
            |> FlatList.find (fun struct (k, _) -> k = key)
            |> sndv

        Assert.AreEqual<int> (2, countFor 1)
        Assert.AreEqual<int> (2, countFor 2)
        Assert.AreEqual<int> (1, countFor 3)
        Assert.AreEqual<int> (1, countFor 5)

    [<TestMethod>]
    member _.``groupBy groups elements`` () =
        let flatList = FlatList.ofArray [| 1; 2; 3; 1; 2; 5 |]
        let result = FlatList.groupBy (fun x -> x % 2) flatList

        Assert.AreEqual<int> (2, result.Length)

        // Find group for a key
        let groupFor key =
            result
            |> FlatList.find (fun struct (k, _) -> k = key)
            |> sndv

        let evenGroup = groupFor 0
        let oddGroup = groupFor 1

        Assert.AreEqual<int> (2, evenGroup.Length)
        Assert.AreEqual<int> (4, oddGroup.Length)

        Assert.IsTrue (FlatList.forall (fun x -> x % 2 = 0) evenGroup)
        Assert.IsTrue (FlatList.forall (fun x -> x % 2 = 1) oddGroup)

    [<TestMethod>]
    member _.``chunkBySize splits into chunks`` () =
        let flatList = FlatList.ofArray [| 1; 2; 3; 4; 5; 6; 7 |]
        let result = FlatList.chunkBySize 3 flatList

        Assert.AreEqual<int> (3, result.Length)

        Assert.AreEqual<int> (3, result.[0].Length)
        Assert.AreEqual<int> (3, result.[1].Length)
        Assert.AreEqual<int> (1, result.[2].Length)

        // Check first chunk
        Assert.AreEqual<int> (1, result.[0].[0])
        Assert.AreEqual<int> (2, result.[0].[1])
        Assert.AreEqual<int> (3, result.[0].[2])

        // Check second chunk
        Assert.AreEqual<int> (4, result.[1].[0])
        Assert.AreEqual<int> (5, result.[1].[1])
        Assert.AreEqual<int> (6, result.[1].[2])

        // Check third chunk
        Assert.AreEqual<int> (7, result.[2].[0])

    [<TestMethod>]
    [<ExpectedException(typeof<ArgumentException>)>]
    member _.``chunkBySize throws for non-positive chunk size`` () =
        let flatList = FlatList.ofArray [| 1; 2; 3 |]
        FlatList.chunkBySize 0 flatList |> ignore

    [<TestMethod>]
    member _.``splitInto divides list into specified number of chunks`` () =
        let flatList = FlatList.ofArray [| 1; 2; 3; 4; 5; 6; 7; 8 |]

        // Split into 3 chunks: [|1; 2; 3|], [|4; 5; 6|], [|7; 8|]
        let result = FlatList.splitInto 3 flatList

        Assert.AreEqual<int> (3, result.Length)
        Assert.AreEqual<int> (3, result.[0].Length)
        Assert.AreEqual<int> (3, result.[1].Length)
        Assert.AreEqual<int> (2, result.[2].Length)

        // Check first chunk
        Assert.AreEqual<int> (1, result.[0].[0])
        Assert.AreEqual<int> (2, result.[0].[1])
        Assert.AreEqual<int> (3, result.[0].[2])

        // Check second chunk
        Assert.AreEqual<int> (4, result.[1].[0])
        Assert.AreEqual<int> (5, result.[1].[1])
        Assert.AreEqual<int> (6, result.[1].[2])

        // Check third chunk
        Assert.AreEqual<int> (7, result.[2].[0])
        Assert.AreEqual<int> (8, result.[2].[1])

    [<TestMethod>]
    [<ExpectedException(typeof<ArgumentException>)>]
    member _.``splitInto throws for non-positive count`` () =
        let flatList = FlatList.ofArray [| 1; 2; 3 |]
        FlatList.splitInto 0 flatList |> ignore

    [<TestMethod>]
    member _.``windowed creates sliding windows`` () =
        let flatList = FlatList.ofArray [| 10; 20; 30; 40; 50 |]
        let result = FlatList.windowed 3 flatList

        Assert.AreEqual<int> (3, result.Length)

        Assert.AreEqual<int> (3, result.[0].Length)
        Assert.AreEqual<int> (10, result.[0].[0])
        Assert.AreEqual<int> (20, result.[0].[1])
        Assert.AreEqual<int> (30, result.[0].[2])

        Assert.AreEqual<int> (3, result.[1].Length)
        Assert.AreEqual<int> (20, result.[1].[0])
        Assert.AreEqual<int> (30, result.[1].[1])
        Assert.AreEqual<int> (40, result.[1].[2])

        Assert.AreEqual<int> (3, result.[2].Length)
        Assert.AreEqual<int> (30, result.[2].[0])
        Assert.AreEqual<int> (40, result.[2].[1])
        Assert.AreEqual<int> (50, result.[2].[2])

    [<TestMethod>]
    [<ExpectedException(typeof<ArgumentException>)>]
    member _.``windowed throws for non-positive window size`` () =
        let flatList = FlatList.ofArray [| 1; 2; 3 |]
        FlatList.windowed 0 flatList |> ignore

    [<TestMethod>]
    member _.``pairwise creates adjacent pairs`` () =
        let flatList = FlatList.ofArray [| 10; 20; 30; 40 |]
        let result = FlatList.pairwise flatList

        Assert.AreEqual<int> (3, result.Length)
        Assert.AreEqual<struct (int * int)> (struct (10, 20), result.[0])
        Assert.AreEqual<struct (int * int)> (struct (20, 30), result.[1])
        Assert.AreEqual<struct (int * int)> (struct (30, 40), result.[2])

    [<TestMethod>]
    member _.``pairwise returns empty for singleton or empty`` () =
        Assert.AreEqual<int> (0, (FlatList.pairwise (FlatList.singleton 1)).Length)
        Assert.AreEqual<int> (0, (FlatList.pairwise FlatList.empty<int>).Length)
