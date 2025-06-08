namespace FSharp.Collections.Immutable.Tests.FlatList

open System
open System.Collections.Immutable
open System.Collections.Generic
open Microsoft.VisualStudio.TestTools.UnitTesting
open FSharp.Collections.Immutable

[<TestClass>]
type FoldTests () =

    [<TestMethod>]
    member _.``fold accumulates values`` () =
        let flatList = FlatList.ofArray [| 1; 2; 3; 4 |]
        let result = FlatList.fold (fun acc x -> acc + x) 0 flatList

        Assert.AreEqual<int> (10, result) // 0 + 1 + 2 + 3 + 4 = 10

    [<TestMethod>]
    member _.``fold with string concatenation works`` () =
        let flatList = FlatList.ofArray [| "a"; "b"; "c" |]
        let result = FlatList.fold (fun acc x -> acc + x) "" flatList

        Assert.AreEqual<string> ("abc", result)

    [<TestMethod>]
    member _.``fold2 accumulates from two lists`` () =
        let list1 = FlatList.ofArray [| 1; 2; 3 |]
        let list2 = FlatList.ofArray [| 10; 20; 30 |]

        let result = FlatList.fold2 (fun acc x y -> acc + x * y) 0 list1 list2

        Assert.AreEqual<int> (140, result) // 0 + (1*10) + (2*20) + (3*30) = 140

    [<TestMethod>]
    [<ExpectedException(typeof<ArgumentException>)>]
    member _.``fold2 throws when lists have different lengths`` () =
        let list1 = FlatList.ofArray [| 1; 2 |]
        let list2 = FlatList.ofArray [| 10; 20; 30 |]

        FlatList.fold2 (fun acc x y -> acc + x * y) 0 list1 list2
        |> ignore

    [<TestMethod>]
    member _.``foldBack accumulates values starting from the end`` () =
        let flatList = FlatList.ofArray [| 1; 2; 3; 4 |]
        let result = FlatList.foldBack (fun x acc -> acc - x) flatList 0

        // With foldBack: (0 - 4) - 3 - 2 - 1 = -10
        Assert.AreEqual<int> (-10, result)

    [<TestMethod>]
    member _.``foldBack with string concatenation works`` () =
        let flatList = FlatList.ofArray [| "a"; "b"; "c" |]
        let result = FlatList.foldBack (fun x acc -> x + acc) flatList ""

        // With foldBack: "a" + "b" + "c" + "" = "abc"
        Assert.AreEqual<string> ("abc", result)

    [<TestMethod>]
    member _.``foldBack2 accumulates from two lists starting from the end`` () =
        let list1 = FlatList.ofArray [| 1; 2; 3 |]
        let list2 = FlatList.ofArray [| 10; 20; 30 |]

        let result = FlatList.foldBack2 (fun x y acc -> acc + x * y) list1 list2 0

        // With foldBack2: 0 + 3*30 + 2*20 + 1*10 = 140
        Assert.AreEqual<int> (140, result)

    [<TestMethod>]
    [<ExpectedException(typeof<ArgumentException>)>]
    member _.``foldBack2 throws when lists have different lengths`` () =
        let list1 = FlatList.ofArray [| 1; 2 |]
        let list2 = FlatList.ofArray [| 10; 20; 30 |]

        FlatList.foldBack2 (fun x y acc -> acc + x * y) list1 list2 0
        |> ignore

    [<TestMethod>]
    member _.``reduce combines elements`` () =
        let flatList = FlatList.ofArray [| 1; 2; 3; 4 |]
        let result = FlatList.reduce (fun acc x -> acc + x) flatList

        // 1 + 2 + 3 + 4 = 10
        Assert.AreEqual<int> (10, result)

    [<TestMethod>]
    [<ExpectedException(typeof<InvalidOperationException>)>]
    member _.``reduce throws on empty list`` () =
        FlatList.reduce (fun acc x -> acc + x) FlatList.empty<int>
        |> ignore

    [<TestMethod>]
    member _.``reduceBack combines elements starting from the end`` () =
        let flatList = FlatList.ofArray [| 1; 2; 3; 4 |]
        let result = FlatList.reduceBack (fun x acc -> x - acc) flatList

        // 1 - (2 - (3 - 4)) = 1 - (2 - (-1)) = 1 - 3 = -2
        Assert.AreEqual<int> (-2, result)

    [<TestMethod>]
    [<ExpectedException(typeof<InvalidOperationException>)>]
    member _.``reduceBack throws on empty list`` () =
        FlatList.reduceBack (fun x acc -> x + acc) FlatList.empty<int>
        |> ignore

    [<TestMethod>]
    member _.``scan produces intermediate results`` () =
        let flatList = FlatList.ofArray [| 1; 2; 3; 4 |]
        let result = FlatList.scan (fun acc x -> acc + x) 0 flatList

        Assert.AreEqual<int> (5, result.Length)
        Assert.AreEqual<int> (0, result.[0]) // Initial state
        Assert.AreEqual<int> (1, result.[1]) // 0+1
        Assert.AreEqual<int> (3, result.[2]) // 1+2
        Assert.AreEqual<int> (6, result.[3]) // 3+3
        Assert.AreEqual<int> (10, result.[4]) // 6+4

    [<TestMethod>]
    member _.``scanBack produces intermediate results starting from the end`` () =
        let flatList = FlatList.ofArray [| 1; 2; 3; 4 |]
        let result = FlatList.scanBack (fun x acc -> x + acc) flatList 0

        Assert.AreEqual<int> (5, result.Length)
        Assert.AreEqual<int> (10, result.[0]) // 1 + (2 + (3 + (4 + 0)))
        Assert.AreEqual<int> (9, result.[1]) // 2 + (3 + (4 + 0))
        Assert.AreEqual<int> (7, result.[2]) // 3 + (4 + 0)
        Assert.AreEqual<int> (4, result.[3]) // 4 + 0
        Assert.AreEqual<int> (0, result.[4]) // Initial state

    [<TestMethod>]
    member _.``sum calculates sum of elements`` () =
        let flatList = FlatList.ofArray [| 1; 2; 3; 4 |]
        let result = FlatList.sum flatList

        Assert.AreEqual<int> (10, result) // 1 + 2 + 3 + 4 = 10

    [<TestMethod>]
    member _.``sumBy calculates sum using projection function`` () =
        let flatList = FlatList.ofArray [| 1; 2; 3; 4 |]
        let result = FlatList.sumBy (fun x -> x * x) flatList

        Assert.AreEqual<int> (30, result) // 1*1 + 2*2 + 3*3 + 4*4 = 30

    [<TestMethod>]
    member _.``average calculates average of elements`` () =
        let flatList = FlatList.ofArray [| 1.0; 2.0; 3.0; 4.0 |]
        let result = FlatList.average flatList

        Assert.AreEqual<float> (2.5, result) // (1 + 2 + 3 + 4) / 4 = 10 / 4 = 2.5

    [<TestMethod>]
    [<ExpectedException(typeof<ArgumentException>)>]
    member _.``average throws on empty list`` () = FlatList.average FlatList.empty<float> |> ignore

    [<TestMethod>]
    member _.``averageBy calculates average using projection function`` () =
        let flatList = FlatList.ofArray [| 1; 2; 3; 4 |]
        let result = FlatList.averageBy float flatList

        Assert.AreEqual<float> (2.5, result) // (1 + 2 + 3 + 4) / 4 = 10 / 4 = 2.5

    [<TestMethod>]
    member _.``min finds minimum element`` () =
        let flatList = FlatList.ofArray [| 5; 3; 9; 1; 8 |]
        let result = FlatList.min flatList

        Assert.AreEqual<int> (1, result)

    [<TestMethod>]
    member _.``minBy finds element with minimum projected value`` () =
        let people = FlatList.ofArray [| ("Alice", 25); ("Bob", 18); ("Charlie", 32) |]
        let result = FlatList.minBy snd people

        Assert.AreEqual<string * int> (("Bob", 18), result)

    [<TestMethod>]
    member _.``max finds maximum element`` () =
        let flatList = FlatList.ofArray [| 5; 3; 9; 1; 8 |]
        let result = FlatList.max flatList

        Assert.AreEqual<int> (9, result)

    [<TestMethod>]
    member _.``maxBy finds element with maximum projected value`` () =
        let people = FlatList.ofArray [| ("Alice", 25); ("Bob", 18); ("Charlie", 32) |]
        let result = FlatList.maxBy snd people

        Assert.AreEqual<string * int> (("Charlie", 32), result)
