namespace FSharp.Collections.Immutable.Tests.FlatList

open System
open System.Collections.Immutable
open System.Collections.Generic
open Microsoft.VisualStudio.TestTools.UnitTesting
open FSharp.Collections.Immutable

[<TestClass>]
type SearchTests () =

    [<TestMethod>]
    member _.``find returns first element matching predicate`` () =
        let flatList = FlatList.ofArray [| 1; 2; 3; 4 |]
        Assert.AreEqual<int> (2, FlatList.find (fun x -> x % 2 = 0) flatList)

    [<TestMethod>]
    [<ExpectedException(typeof<KeyNotFoundException>)>]
    member _.``find throws when no element satisfies predicate`` () =
        let flatList = FlatList.ofArray [| 1; 3; 5 |]
        FlatList.find (fun x -> x % 2 = 0) flatList |> ignore

    [<TestMethod>]
    member _.``tryFind returns element or ValueNone`` () =
        let flatList = FlatList.ofArray [| 1; 2; 3 |]

        Assert.AreEqual<int voption> (ValueSome 2, FlatList.tryFind (fun x -> x % 2 = 0) flatList)
        Assert.AreEqual<int voption> (ValueNone, FlatList.tryFind (fun x -> x > 10) flatList)

    [<TestMethod>]
    member _.``findBack returns last element matching predicate`` () =
        let flatList = FlatList.ofArray [| 1; 2; 3; 4; 2 |]
        Assert.AreEqual<int> (4, FlatList.findBack (fun x -> x % 2 = 0) flatList)

    [<TestMethod>]
    [<ExpectedException(typeof<KeyNotFoundException>)>]
    member _.``findBack throws when no element satisfies predicate`` () =
        let flatList = FlatList.ofArray [| 1; 3; 5 |]
        FlatList.findBack (fun x -> x % 2 = 0) flatList |> ignore

    [<TestMethod>]
    member _.``tryFindBack returns element or ValueNone`` () =
        let flatList = FlatList.ofArray [| 1; 2; 3; 4; 2 |]

        Assert.AreEqual<int voption> (ValueSome 4, FlatList.tryFindBack (fun x -> x % 2 = 0) flatList)
        Assert.AreEqual<int voption> (ValueNone, FlatList.tryFindBack (fun x -> x > 10) flatList)

    [<TestMethod>]
    member _.``findIndex returns index of first element matching predicate`` () =
        let flatList = FlatList.ofArray [| 1; 2; 3; 4 |]
        Assert.AreEqual<int> (1, FlatList.findIndex (fun x -> x % 2 = 0) flatList)

    [<TestMethod>]
    [<ExpectedException(typeof<KeyNotFoundException>)>]
    member _.``findIndex throws when no element satisfies predicate`` () =
        let flatList = FlatList.ofArray [| 1; 3; 5 |]
        FlatList.findIndex (fun x -> x % 2 = 0) flatList |> ignore

    [<TestMethod>]
    member _.``tryFindIndex returns index or ValueNone`` () =
        let flatList = FlatList.ofArray [| 1; 2; 3 |]

        Assert.AreEqual<int voption> (ValueSome 1, FlatList.tryFindIndex (fun x -> x % 2 = 0) flatList)
        Assert.AreEqual<int voption> (ValueNone, FlatList.tryFindIndex (fun x -> x > 10) flatList)

    [<TestMethod>]
    member _.``findIndexBack returns index of last element matching predicate`` () =
        let flatList = FlatList.ofArray [| 1; 2; 3; 4; 2 |]
        Assert.AreEqual<int> (3, FlatList.findIndexBack (fun x -> x % 2 = 0) flatList)

    [<TestMethod>]
    [<ExpectedException(typeof<KeyNotFoundException>)>]
    member _.``findIndexBack throws when no element satisfies predicate`` () =
        let flatList = FlatList.ofArray [| 1; 3; 5 |]
        FlatList.findIndexBack (fun x -> x % 2 = 0) flatList
        |> ignore

    [<TestMethod>]
    member _.``tryFindIndexBack returns index or ValueNone`` () =
        let flatList = FlatList.ofArray [| 1; 2; 3; 4; 2 |]

        Assert.AreEqual<int voption> (ValueSome 3, FlatList.tryFindIndexBack (fun x -> x % 2 = 0) flatList)
        Assert.AreEqual<int voption> (ValueNone, FlatList.tryFindIndexBack (fun x -> x > 10) flatList)

    [<TestMethod>]
    member _.``pick returns first value from chooser that returns ValueSome`` () =
        let flatList = FlatList.ofArray [| 1; 2; 3; 4 |]
        let result = FlatList.pick (fun x -> if x > 2 then ValueSome (x * 10) else ValueNone) flatList

        Assert.AreEqual<int> (30, result)

    [<TestMethod>]
    [<ExpectedException(typeof<KeyNotFoundException>)>]
    member _.``pick throws when chooser returns ValueNone for all elements`` () =
        let flatList = FlatList.ofArray [| 1; 2; 3 |]
        FlatList.pick (fun x -> if x > 10 then ValueSome x else ValueNone) flatList
        |> ignore

    [<TestMethod>]
    member _.``tryPick returns first value from chooser that returns ValueSome, or ValueNone`` () =
        let flatList = FlatList.ofArray [| 1; 2; 3; 4 |]

        let result1 = FlatList.tryPick (fun x -> if x > 2 then ValueSome (x * 10) else ValueNone) flatList
        Assert.AreEqual<int voption> (ValueSome 30, result1)

        let result2 = FlatList.tryPick (fun x -> if x > 10 then ValueSome x else ValueNone) flatList
        Assert.AreEqual<int voption> (ValueNone, result2)

    [<TestMethod>]
    member _.``index returns position of first occurrence of item`` () =
        let flatList = FlatList.ofArray [| 10; 20; 30; 20; 40 |]

        Assert.AreEqual<int> (1, FlatList.index 20 flatList)

    [<TestMethod>]
    [<ExpectedException(typeof<KeyNotFoundException>)>]
    member _.``index throws when item not found`` () =
        let flatList = FlatList.ofArray [| 10; 20; 30 |]
        FlatList.index 50 flatList |> ignore

    [<TestMethod>]
    member _.``lastIndex returns position of last occurrence of item`` () =
        let flatList = FlatList.ofArray [| 10; 20; 30; 20; 40 |]

        Assert.AreEqual<int> (3, FlatList.lastIndex 20 flatList)

    [<TestMethod>]
    [<ExpectedException(typeof<KeyNotFoundException>)>]
    member _.``lastIndex throws when item not found`` () =
        let flatList = FlatList.ofArray [| 10; 20; 30 |]
        FlatList.lastIndex 50 flatList |> ignore

    [<TestMethod>]
    member _.``tryFindBack returns last element matching predicate`` () =
        let flatList = FlatList.ofArray [| 1; 2; 3; 4; 2 |]

        Assert.AreEqual<int voption> (ValueSome 4, FlatList.tryFindBack (fun x -> x % 2 = 0) flatList)
        Assert.AreEqual<int voption> (ValueNone, FlatList.tryFindBack (fun x -> x > 10) flatList)
