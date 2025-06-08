namespace FSharp.Collections.Immutable.Tests.FlatList

open System
open System.Collections.Immutable
open System.Collections.Generic
open Microsoft.VisualStudio.TestTools.UnitTesting
open FSharp.Collections.Immutable

[<TestClass>]
type BuilderTests () =

    [<TestMethod>]
    member _.``builder and ofBuilder create FlatList`` () =
        let b = FlatList.builder ()
        b.Add (1)
        b.Add (2)
        b.Add (3)

        let flatList = FlatList.ofBuilder b

        Assert.AreEqual<int> (3, flatList.Length)
        Assert.AreEqual<int> (1, flatList.[0])
        Assert.AreEqual<int> (2, flatList.[1])
        Assert.AreEqual<int> (3, flatList.[2])

    [<TestMethod>]
    member _.``builderWith creates builder with capacity`` () =
        let b = FlatList.builderWith 10
        for i = 1 to 10 do
            b.Add (i)

        let flatList = FlatList.ofBuilder b

        Assert.AreEqual<int> (10, flatList.Length)
        for i = 0 to 9 do
            Assert.AreEqual<int> (i + 1, flatList.[i])

    [<TestMethod>]
    member _.``moveFromBuilder builds FlatList and empties builder`` () =
        let b = FlatList.builder ()
        b.Add (1)
        b.Add (2)
        b.Add (3)

        let flatList = FlatList.moveFromBuilder b

        Assert.AreEqual<int> (3, flatList.Length)
        Assert.AreEqual<int> (0, b.Count)

    [<TestMethod>]
    member _.``toBuilder creates builder from FlatList`` () =
        let original = FlatList.ofArray [| 1; 2; 3 |]
        let builder = FlatList.toBuilder original

        Assert.AreEqual<int> (original.Length, builder.Count)
        for i = 0 to original.Length - 1 do
            Assert.AreEqual<int> (original.[i], builder.[i])

    [<TestMethod>]
    member _.``Builder.add adds to builder`` () =
        let b = FlatList.builder ()
        FlatList.Builder.add 42 b

        Assert.AreEqual<int> (1, b.Count)
        Assert.AreEqual<int> (42, b.[0])

    [<TestMethod>]
    member _.``build applies function to builder and returns FlatList`` () =
        let addItems (builder : FlatList<int>.Builder) =
            builder.Add (1)
            builder.Add (2)
            builder.Add (3)

        let flatList = FlatList.build addItems

        Assert.AreEqual<int> (3, flatList.Length)
        Assert.AreEqual<int> (1, flatList.[0])
        Assert.AreEqual<int> (2, flatList.[1])
        Assert.AreEqual<int> (3, flatList.[2])

    [<TestMethod>]
    member _.``update applies function to builder from FlatList`` () =
        let original = FlatList.ofArray [| 1; 2; 3 |]

        let addItems (builder : FlatList<int>.Builder) =
            builder.Add (4)
            builder.Add (5)

        let result = FlatList.update addItems original

        Assert.AreEqual<int> (5, result.Length)
        Assert.AreEqual<int> (1, result.[0])
        Assert.AreEqual<int> (2, result.[1])
        Assert.AreEqual<int> (3, result.[2])
        Assert.AreEqual<int> (4, result.[3])
        Assert.AreEqual<int> (5, result.[4])
