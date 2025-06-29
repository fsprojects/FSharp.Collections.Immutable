namespace FSharp.Collections.Immutable.Tests.FlatList

open System
open System.Collections.Immutable
open System.Collections.Generic
open Microsoft.VisualStudio.TestTools.UnitTesting
open FSharp.Collections.Immutable

[<TestClass>]
type PairOperationTests () =

    [<TestMethod>]
    member _.``allPairs creates all combinations`` () =
        let list1 = FlatList.ofArray [| 1; 2 |]
        let list2 = FlatList.ofArray [| 'a'; 'b'; 'c' |]
        let result = FlatList.allPairs list1 list2

        Assert.AreEqual<int> (6, result.Length)
        Assert.AreEqual<int * char> ((1, 'a'), result.[0])
        Assert.AreEqual<int * char> ((1, 'b'), result.[1])
        Assert.AreEqual<int * char> ((1, 'c'), result.[2])
        Assert.AreEqual<int * char> ((2, 'a'), result.[3])
        Assert.AreEqual<int * char> ((2, 'b'), result.[4])
        Assert.AreEqual<int * char> ((2, 'c'), result.[5])

    [<TestMethod>]
    member _.``permute reorders elements`` () =
        let flatList = FlatList.ofArray [| 10; 20; 30 |]
        let result = FlatList.permute (fun i -> (i + 2) % 3) flatList

        Assert.AreEqual<int> (3, result.Length)
        Assert.AreEqual<int> (30, result.[0])
        Assert.AreEqual<int> (10, result.[1])
        Assert.AreEqual<int> (20, result.[2])

    [<TestMethod>]
    [<ExpectedException(typeof<ArgumentException>)>]
    member _.``permute throws for invalid permutation function`` () =
        let flatList = FlatList.ofArray [| 10; 20; 30 |]
        FlatList.permute (fun _ -> 10) flatList |> ignore

    [<TestMethod>]
    member _.``zip combines two lists into pairs`` () =
        let list1 = FlatList.ofArray [| 1; 2; 3 |]
        let list2 = FlatList.ofArray [| "a"; "b"; "c" |]

        let result = FlatList.zip list1 list2

        Assert.AreEqual<int> (3, result.Length)
        Assert.AreEqual<struct (int * string)> (struct (1, "a"), result.[0])
        Assert.AreEqual<struct (int * string)> (struct (2, "b"), result.[1])
        Assert.AreEqual<struct (int * string)> (struct (3, "c"), result.[2])

    [<TestMethod>]
    [<ExpectedException(typeof<ArgumentException>)>]
    member _.``zip throws when lists have different lengths`` () =
        let list1 = FlatList.ofArray [| 1; 2 |]
        let list2 = FlatList.ofArray [| "a"; "b"; "c" |]

        FlatList.zip list1 list2 |> ignore

    [<TestMethod>]
    member _.``unzip splits pairs into two lists`` () =
        let flatList = FlatList.ofArray [| struct (1, "a"); (2, "b"); (3, "c") |]

        let struct (list1, list2) = FlatList.unzip flatList

        Assert.AreEqual<int> (3, list1.Length)
        Assert.AreEqual<int> (3, list2.Length)

        Assert.AreEqual<int> (1, list1.[0])
        Assert.AreEqual<int> (2, list1.[1])
        Assert.AreEqual<int> (3, list1.[2])

        Assert.AreEqual<string> ("a", list2.[0])
        Assert.AreEqual<string> ("b", list2.[1])
        Assert.AreEqual<string> ("c", list2.[2])

    [<TestMethod>]
    member _.``zip3 combines three lists into triples`` () =
        let list1 = FlatList.ofArray [| 1; 2; 3 |]
        let list2 = FlatList.ofArray [| "a"; "b"; "c" |]
        let list3 = FlatList.ofArray [| true; false; true |]

        let result = FlatList.zip3 list1 list2 list3

        Assert.AreEqual<int> (3, result.Length)
        Assert.AreEqual<struct (int * string * bool)> (struct (1, "a", true), result.[0])
        Assert.AreEqual<struct (int * string * bool)> (struct (2, "b", false), result.[1])
        Assert.AreEqual<struct (int * string * bool)> (struct (3, "c", true), result.[2])

    [<TestMethod>]
    [<ExpectedException(typeof<ArgumentException>)>]
    member _.``zip3 throws when lists have different lengths (first and second)`` () =
        let list1 = FlatList.ofArray [| 1; 2 |]
        let list2 = FlatList.ofArray [| "a"; "b"; "c" |]
        let list3 = FlatList.ofArray [| true; false; true |]

        FlatList.zip3 list1 list2 list3 |> ignore

    [<TestMethod>]
    [<ExpectedException(typeof<ArgumentException>)>]
    member _.``zip3 throws when lists have different lengths (first and third)`` () =
        let list1 = FlatList.ofArray [| 1; 2 |]
        let list2 = FlatList.ofArray [| "a"; "b" |]
        let list3 = FlatList.ofArray [| true; false; true |]

        FlatList.zip3 list1 list2 list3 |> ignore

    [<TestMethod>]
    member _.``unzip3 splits triples into three lists`` () =
        let flatList = FlatList.ofArray [| struct (1, "a", true); (2, "b", false); (3, "c", true) |]

        let struct (list1, list2, list3) = FlatList.unzip3 flatList

        Assert.AreEqual<int> (3, list1.Length)
        Assert.AreEqual<int> (3, list2.Length)
        Assert.AreEqual<int> (3, list3.Length)

        Assert.AreEqual<int> (1, list1.[0])
        Assert.AreEqual<int> (2, list1.[1])
        Assert.AreEqual<int> (3, list1.[2])

        Assert.AreEqual<string> ("a", list2.[0])
        Assert.AreEqual<string> ("b", list2.[1])
        Assert.AreEqual<string> ("c", list2.[2])

        Assert.AreEqual<bool> (true, list3.[0])
        Assert.AreEqual<bool> (false, list3.[1])
        Assert.AreEqual<bool> (true, list3.[2])

    [<TestMethod>]
    member _.``transpose reorients list of lists`` () =
        let matrix = FlatList.ofArray [| FlatList.ofArray [| 1; 2; 3 |]; FlatList.ofArray [| 4; 5; 6 |] |]

        let result = FlatList.transpose matrix

        Assert.AreEqual<int> (3, result.Length)
        Assert.AreEqual<int> (2, result.[0].Length)
        Assert.AreEqual<int> (2, result.[1].Length)
        Assert.AreEqual<int> (2, result.[2].Length)

        // First column
        Assert.AreEqual<int> (1, result.[0].[0])
        Assert.AreEqual<int> (4, result.[0].[1])

        // Second column
        Assert.AreEqual<int> (2, result.[1].[0])
        Assert.AreEqual<int> (5, result.[1].[1])

        // Third column
        Assert.AreEqual<int> (3, result.[2].[0])
        Assert.AreEqual<int> (6, result.[2].[1])

    [<TestMethod>]
    [<ExpectedException(typeof<ArgumentException>)>]
    member _.``transpose throws when inner arrays have different lengths`` () =
        let matrix = FlatList.ofArray [| FlatList.ofArray [| 1; 2; 3 |]; FlatList.ofArray [| 4; 5 |] |]

        FlatList.transpose matrix |> ignore
