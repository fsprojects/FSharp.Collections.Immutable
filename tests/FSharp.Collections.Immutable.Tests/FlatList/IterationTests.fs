namespace FSharp.Collections.Immutable.Tests.FlatList

open System
open System.Collections.Immutable
open System.Collections.Generic
open Microsoft.VisualStudio.TestTools.UnitTesting
open FSharp.Collections.Immutable

[<TestClass>]
type IterationTests () =

    [<TestMethod>]
    member _.``iter applies function to each element`` () =
        let flatList = FlatList.ofArray [| 1; 2; 3 |]
        let mutable sum = 0

        FlatList.iter (fun x -> sum <- sum + x) flatList

        Assert.AreEqual<int> (6, sum)

    [<TestMethod>]
    member _.``iteri applies function with index`` () =
        let flatList = FlatList.ofArray [| 10; 20; 30 |]
        let mutable sum = 0

        FlatList.iteri (fun i x -> sum <- sum + i + x) flatList

        Assert.AreEqual<int> (63, sum) // (0+10) + (1+20) + (2+30) = 63

    [<TestMethod>]
    member _.``iter2 applies function to pairs`` () =
        let list1 = FlatList.ofArray [| 1; 2; 3 |]
        let list2 = FlatList.ofArray [| 10; 20; 30 |]
        let mutable sum = 0

        FlatList.iter2 (fun x y -> sum <- sum + x + y) list1 list2

        Assert.AreEqual<int> (66, sum) // (1+10) + (2+20) + (3+30) = 66

    [<TestMethod>]
    [<ExpectedException(typeof<ArgumentException>)>]
    member _.``iter2 throws for different length lists`` () =
        let list1 = FlatList.ofArray [| 1; 2 |]
        let list2 = FlatList.ofArray [| 10; 20; 30 |]

        FlatList.iter2 (fun x y -> ()) list1 list2

    [<TestMethod>]
    member _.``iteri2 applies function with index`` () =
        let list1 = FlatList.ofArray [| 1; 2; 3 |]
        let list2 = FlatList.ofArray [| 10; 20; 30 |]
        let mutable result = 0

        FlatList.iteri2 (fun i x y -> result <- result + i + x + y) list1 list2

        Assert.AreEqual<int> (69, result) // (0+1+10) + (1+2+20) + (2+3+30) = 69

    [<TestMethod>]
    member _.``contains checks if element exists`` () =
        let flatList = FlatList.ofArray [| 1; 2; 3 |]

        Assert.IsTrue (FlatList.contains 2 flatList)
        Assert.IsFalse (FlatList.contains 4 flatList)

    [<TestMethod>]
    member _.``exists checks if any element satisfies predicate`` () =
        let flatList = FlatList.ofArray [| 1; 2; 3 |]

        Assert.IsTrue (FlatList.exists (fun x -> x = 2) flatList)
        Assert.IsFalse (FlatList.exists (fun x -> x > 10) flatList)

    [<TestMethod>]
    member _.``exists2 checks elements from two lists`` () =
        let list1 = FlatList.ofArray [| 1; 2; 3 |]
        let list2 = FlatList.ofArray [| 3; 2; 1 |]

        Assert.IsTrue (FlatList.exists2 (fun x y -> x = y) list1 list2)
        Assert.IsFalse (FlatList.exists2 (fun x y -> x > 10 && y > 10) list1 list2)

    [<TestMethod>]
    [<ExpectedException(typeof<ArgumentException>)>]
    member _.``exists2 throws when lists have different lengths`` () =
        let list1 = FlatList.ofArray [| 1; 2 |]
        let list2 = FlatList.ofArray [| 1; 2; 3 |]

        FlatList.exists2 (fun x y -> x = y) list1 list2 |> ignore

    [<TestMethod>]
    member _.``forall checks if all elements satisfy predicate`` () =
        let flatList = FlatList.ofArray [| 2; 4; 6 |]

        Assert.IsTrue (FlatList.forall (fun x -> x % 2 = 0) flatList)
        Assert.IsFalse (FlatList.forall (fun x -> x > 3) flatList)

    [<TestMethod>]
    member _.``forall2 checks all element pairs`` () =
        let list1 = FlatList.ofArray [| 1; 2; 3 |]
        let list2 = FlatList.ofArray [| 4; 5; 6 |]

        Assert.IsTrue (FlatList.forall2 (fun x y -> x < y) list1 list2)
        Assert.IsFalse (FlatList.forall2 (fun x y -> x > y) list1 list2)

    [<TestMethod>]
    [<ExpectedException(typeof<ArgumentException>)>]
    member _.``forall2 throws when lists have different lengths`` () =
        let list1 = FlatList.ofArray [| 1; 2 |]
        let list2 = FlatList.ofArray [| 1; 2; 3 |]

        FlatList.forall2 (fun x y -> x = y) list1 list2 |> ignore
