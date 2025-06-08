namespace FSharp.Collections.Immutable.Tests.FlatList

open System
open System.Collections.Immutable
open System.Collections.Generic
open Microsoft.VisualStudio.TestTools.UnitTesting
open FSharp.Collections.Immutable

[<TestClass>]
type BasicOperationsTests () =

    [<TestMethod>]
    member _.``isEmpty returns true for empty FlatList`` () =
        Assert.IsTrue (FlatList.isEmpty FlatList.empty<int>)
        Assert.IsFalse (FlatList.isEmpty (FlatList.singleton 1))

    [<TestMethod>]
    member _.``isDefault returns true for default FlatList`` () =
        let defaultList = Unchecked.defaultof<FlatList<int>>
        Assert.IsTrue (FlatList.isDefault defaultList)
        Assert.IsFalse (FlatList.isDefault FlatList.empty<int>)

    [<TestMethod>]
    member _.``isDefaultOrEmpty returns true for default or empty FlatList`` () =
        let defaultList = Unchecked.defaultof<FlatList<int>>
        Assert.IsTrue (FlatList.isDefaultOrEmpty defaultList)
        Assert.IsTrue (FlatList.isDefaultOrEmpty FlatList.empty<int>)
        Assert.IsFalse (FlatList.isDefaultOrEmpty (FlatList.singleton 1))

    [<TestMethod>]
    member _.``length returns number of elements`` () =
        Assert.AreEqual<int> (0, FlatList.length FlatList.empty<int>)
        Assert.AreEqual<int> (3, FlatList.length (FlatList.ofArray [| 1; 2; 3 |]))

    [<TestMethod>]
    member _.``item returns element at index`` () =
        let flatList = FlatList.ofArray [| 10; 20; 30 |]

        Assert.AreEqual<int> (10, FlatList.item 0 flatList)
        Assert.AreEqual<int> (20, FlatList.item 1 flatList)
        Assert.AreEqual<int> (30, FlatList.item 2 flatList)

    [<TestMethod>]
    [<ExpectedException(typeof<IndexOutOfRangeException>)>]
    member _.``item throws for out of range index`` () =
        let flatList = FlatList.ofArray [| 1; 2; 3 |]
        FlatList.item 3 flatList |> ignore

    [<TestMethod>]
    member _.``tryItem returns element or ValueNone`` () =
        let flatList = FlatList.ofArray [| 10; 20; 30 |]

        Assert.AreEqual<int voption> (ValueSome 10, FlatList.tryItem 0 flatList)
        Assert.AreEqual<int voption> (ValueSome 20, FlatList.tryItem 1 flatList)
        Assert.AreEqual<int voption> (ValueNone, FlatList.tryItem 3 flatList)
        Assert.AreEqual<int voption> (ValueNone, FlatList.tryItem -1 flatList)
        Assert.AreEqual<int voption> (ValueNone, FlatList.tryItem 0 (Unchecked.defaultof<FlatList<int>>))

    [<TestMethod>]
    member _.``head returns first element`` () =
        let flatList = FlatList.ofArray [| 10; 20; 30 |]
        Assert.AreEqual<int> (10, FlatList.head flatList)

    [<TestMethod>]
    [<ExpectedException(typeof<InvalidOperationException>)>]
    member _.``head throws for empty list`` () = FlatList.head FlatList.empty<int> |> ignore

    [<TestMethod>]
    member _.``tryHead returns first element or ValueNone`` () =
        let flatList = FlatList.ofArray [| 10; 20; 30 |]
        Assert.AreEqual<int voption> (ValueSome 10, FlatList.tryHead flatList)
        Assert.AreEqual<int voption> (ValueNone, FlatList.tryHead FlatList.empty<int>)

    [<TestMethod>]
    member _.``tail returns all but first element`` () =
        let flatList = FlatList.ofArray [| 10; 20; 30 |]
        let result = FlatList.tail flatList

        Assert.AreEqual<int> (2, result.Length)
        Assert.AreEqual<int> (20, result.[0])
        Assert.AreEqual<int> (30, result.[1])

    [<TestMethod>]
    [<ExpectedException(typeof<ArgumentException>)>]
    member _.``tail throws for empty list`` () = FlatList.tail FlatList.empty<int> |> ignore

    [<TestMethod>]
    member _.``tryTail returns tail or ValueNone`` () =
        let flatList = FlatList.ofArray [| 10; 20; 30 |]

        match FlatList.tryTail flatList with
        | ValueSome tail ->
            Assert.AreEqual<int> (2, tail.Length)
            Assert.AreEqual<int> (20, tail.[0])
            Assert.AreEqual<int> (30, tail.[1])
        | ValueNone -> Assert.Fail ("Should be ValueSome")

        Assert.AreEqual<FlatList<int> voption> (ValueNone, FlatList.tryTail FlatList.empty<int>)

    [<TestMethod>]
    member _.``last returns last element`` () =
        let flatList = FlatList.ofArray [| 10; 20; 30 |]
        Assert.AreEqual<int> (30, FlatList.last flatList)

    [<TestMethod>]
    [<ExpectedException(typeof<InvalidOperationException>)>]
    member _.``last throws for empty list`` () = FlatList.last FlatList.empty<int> |> ignore

    [<TestMethod>]
    member _.``tryLast returns last element or ValueNone`` () =
        let flatList = FlatList.ofArray [| 10; 20; 30 |]
        Assert.AreEqual<int voption> (ValueSome 30, FlatList.tryLast flatList)
        Assert.AreEqual<int voption> (ValueNone, FlatList.tryLast FlatList.empty<int>)

    [<TestMethod>]
    member _.``exactlyOne returns the single element`` () =
        let flatList = FlatList.singleton 42
        Assert.AreEqual<int> (42, FlatList.exactlyOne flatList)

    [<TestMethod>]
    [<ExpectedException(typeof<InvalidOperationException>)>]
    member _.``exactlyOne throws for empty list`` () = FlatList.exactlyOne FlatList.empty<int> |> ignore

    [<TestMethod>]
    [<ExpectedException(typeof<InvalidOperationException>)>]
    member _.``exactlyOne throws for list with multiple elements`` () =
        FlatList.exactlyOne (FlatList.ofArray [| 1; 2 |]) |> ignore

    [<TestMethod>]
    member _.``tryExactlyOne returns the single element or ValueNone`` () =
        let singletonList = FlatList.singleton 42
        let emptyList = FlatList.empty<int>
        let multipleList = FlatList.ofArray [| 1; 2 |]

        Assert.AreEqual<int voption> (ValueSome 42, FlatList.tryExactlyOne singletonList)
        Assert.AreEqual<int voption> (ValueNone, FlatList.tryExactlyOne emptyList)
        Assert.AreEqual<int voption> (ValueNone, FlatList.tryExactlyOne multipleList)
