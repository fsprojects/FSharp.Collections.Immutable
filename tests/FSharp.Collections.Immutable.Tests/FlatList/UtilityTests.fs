namespace FSharp.Collections.Immutable.Tests.FlatList

open System
open System.Collections.Immutable
open System.Collections.Generic
open Microsoft.VisualStudio.TestTools.UnitTesting
open FSharp.Collections.Immutable

[<TestClass>]
type UtilityTests () =

    [<TestMethod>]
    member _.``blit copies range of elements to array`` () =
        let source = FlatList.ofArray [| 10; 20; 30; 40; 50 |]
        let destination = Array.zeroCreate<int> 5

        FlatList.blit source 1 destination 2 3

        Assert.AreEqual<int> (0, destination.[0])
        Assert.AreEqual<int> (0, destination.[1])
        Assert.AreEqual<int> (20, destination.[2])
        Assert.AreEqual<int> (30, destination.[3])
        Assert.AreEqual<int> (40, destination.[4])

    [<TestMethod>]
    member _.``unfold builds list from generator function`` () =
        // Create a list of powers of 2 up to 2^5
        let result =
            FlatList.unfold
                (fun state ->
                    if state <= 32 then
                        ValueSome (state, state * 2)
                    else
                        ValueNone
                )
                1

        CollectionAssert.AreEqual ([| 1; 2; 4; 8; 16; 32 |], FlatList.toArray result)

    [<TestMethod>]
    member _.``build creats list with builder function`` () =
        let result =
            FlatList.build (fun builder ->
                builder.Add (1)
                builder.Add (2)
                builder.Add (3)
            )

        CollectionAssert.AreEqual ([| 1; 2; 3 |], FlatList.toArray result)

    [<TestMethod>]
    member _.``exactlyOne returns the only element`` () =
        let flatList = FlatList.singleton 42
        let result = FlatList.exactlyOne flatList

        Assert.AreEqual<int> (42, result)

    [<TestMethod>]
    [<ExpectedException(typeof<InvalidOperationException>)>]
    member _.``exactlyOne throws for empty list`` () = FlatList.exactlyOne FlatList.empty<int> |> ignore

    [<TestMethod>]
    [<ExpectedException(typeof<InvalidOperationException>)>]
    member _.``exactlyOne throws for list with multiple elements`` () =
        let flatList = FlatList.ofArray [| 1; 2; 3 |]
        FlatList.exactlyOne flatList |> ignore

    [<TestMethod>]
    member _.``tryExactlyOne returns element for single-element list`` () =
        let flatList = FlatList.singleton 42
        let result = FlatList.tryExactlyOne flatList

        Assert.AreEqual<int voption> (ValueSome 42, result)

    [<TestMethod>]
    member _.``tryExactlyOne returns ValueNone for empty list`` () =
        let result = FlatList.tryExactlyOne FlatList.empty<int>

        Assert.AreEqual<int voption> (ValueNone, result)

    [<TestMethod>]
    member _.``tryExactlyOne returns ValueNone for multi-element list`` () =
        let flatList = FlatList.ofArray [| 1; 2; 3 |]
        let result = FlatList.tryExactlyOne flatList

        Assert.AreEqual<int voption> (ValueNone, result)

    [<TestMethod>]
    member _.``transpose transforms rows into columns`` () =
        let rows = FlatList.ofArray [| FlatList.ofArray [| 1; 2; 3 |]; FlatList.ofArray [| 4; 5; 6 |] |]

        let result = FlatList.transpose rows

        Assert.AreEqual<int> (3, result.Length) // Result has 3 columns

        CollectionAssert.AreEqual ([| 1; 4 |], FlatList.toArray result.[0])
        CollectionAssert.AreEqual ([| 2; 5 |], FlatList.toArray result.[1])
        CollectionAssert.AreEqual ([| 3; 6 |], FlatList.toArray result.[2])

    [<TestMethod>]
    [<ExpectedException(typeof<ArgumentException>)>]
    member _.``transpose throws when inner arrays have different lengths`` () =
        let rows = FlatList.ofArray [| FlatList.ofArray [| 1; 2; 3 |]; FlatList.ofArray [| 4; 5 |] |]

        FlatList.transpose rows |> ignore

    [<TestMethod>]
    member _.``except removes elements from another sequence`` () =
        let flatList = FlatList.ofArray [| 1; 2; 3; 4; 5 |]
        let itemsToExclude = [| 2; 4; 6 |] // Note: 6 is not in the original list

        let result = FlatList.except itemsToExclude flatList

        CollectionAssert.AreEqual ([| 1; 3; 5 |], FlatList.toArray result)

    [<TestMethod>]
    member _.``ofList converts F# list to FlatList`` () =
        let list = [ 1; 2; 3 ]
        let result = FlatList.ofList list

        CollectionAssert.AreEqual ([| 1; 2; 3 |], FlatList.toArray result)

    [<TestMethod>]
    member _.``toList converts FlatList to F# list`` () =
        let flatList = FlatList.ofArray [| 1; 2; 3 |]
        let result = FlatList.toList flatList

        Assert.AreEqual<int list> ([ 1; 2; 3 ], result)

    [<TestMethod>]
    member _.``splitInto divides list into specified number of chunks`` () =
        let flatList = FlatList.ofArray [| 1; 2; 3; 4; 5; 6; 7; 8 |]
        let result = FlatList.splitInto 3 flatList

        Assert.AreEqual<int> (3, result.Length)
        CollectionAssert.AreEqual ([| 1; 2; 3 |], FlatList.toArray result.[0])
        CollectionAssert.AreEqual ([| 4; 5; 6 |], FlatList.toArray result.[1])
        CollectionAssert.AreEqual ([| 7; 8 |], FlatList.toArray result.[2])

    [<TestMethod>]
    [<ExpectedException(typeof<ArgumentException>)>]
    member _.``splitInto throws for non-positive count`` () =
        let flatList = FlatList.ofArray [| 1; 2; 3 |]
        FlatList.splitInto 0 flatList |> ignore

    [<TestMethod>]
    member _.``updateAt changes element at index`` () =
        let flatList = FlatList.ofArray [| 1; 2; 3; 4; 5 |]
        let result = FlatList.updateAt 2 42 flatList

        CollectionAssert.AreEqual ([| 1; 2; 42; 4; 5 |], FlatList.toArray result)

    [<TestMethod>]
    [<ExpectedException(typeof<ArgumentOutOfRangeException>)>]
    member _.``updateAt throws for negative index`` () =
        let flatList = FlatList.ofArray [| 1; 2; 3 |]
        FlatList.updateAt -1 42 flatList |> ignore

    [<TestMethod>]
    [<ExpectedException(typeof<ArgumentOutOfRangeException>)>]
    member _.``updateAt throws for index beyond end`` () =
        let flatList = FlatList.ofArray [| 1; 2; 3 |]
        FlatList.updateAt 3 42 flatList |> ignore

    [<TestMethod>]
    member _.``removeAt removes element at index`` () =
        let flatList = FlatList.ofArray [| 1; 2; 3; 4; 5 |]
        let result = FlatList.removeAt 2 flatList

        CollectionAssert.AreEqual ([| 1; 2; 4; 5 |], FlatList.toArray result)

    [<TestMethod>]
    [<ExpectedException(typeof<ArgumentOutOfRangeException>)>]
    member _.``removeAt throws for negative index`` () =
        let flatList = FlatList.ofArray [| 1; 2; 3 |]
        FlatList.removeAt -1 flatList |> ignore

    [<TestMethod>]
    [<ExpectedException(typeof<ArgumentOutOfRangeException>)>]
    member _.``removeAt throws for index beyond end`` () =
        let flatList = FlatList.ofArray [| 1; 2; 3 |]
        FlatList.removeAt 3 flatList |> ignore

    [<TestMethod>]
    member _.``insertAt adds element at index`` () =
        let flatList = FlatList.ofArray [| 1; 2; 3; 5 |]
        let result = FlatList.insertAt 3 4 flatList

        CollectionAssert.AreEqual ([| 1; 2; 3; 4; 5 |], FlatList.toArray result)

    [<TestMethod>]
    member _.``insertAt can add at beginning`` () =
        let flatList = FlatList.ofArray [| 2; 3; 4 |]
        let result = FlatList.insertAt 0 1 flatList

        CollectionAssert.AreEqual ([| 1; 2; 3; 4 |], FlatList.toArray result)

    [<TestMethod>]
    member _.``insertAt can add at end`` () =
        let flatList = FlatList.ofArray [| 1; 2; 3 |]
        let result = FlatList.insertAt 3 4 flatList

        CollectionAssert.AreEqual ([| 1; 2; 3; 4 |], FlatList.toArray result)

    [<TestMethod>]
    [<ExpectedException(typeof<ArgumentOutOfRangeException>)>]
    member _.``insertAt throws for negative index`` () =
        let flatList = FlatList.ofArray [| 1; 2; 3 |]
        FlatList.insertAt -1 0 flatList |> ignore

    [<TestMethod>]
    [<ExpectedException(typeof<ArgumentOutOfRangeException>)>]
    member _.``insertAt throws for index too high`` () =
        let flatList = FlatList.ofArray [| 1; 2; 3 |]
        FlatList.insertAt 4 0 flatList |> ignore

    [<TestMethod>]
    member _.``insertManyAt adds multiple elements at index`` () =
        let flatList = FlatList.ofArray [| 1; 5 |]
        let result = FlatList.insertManyAt 1 [| 2; 3; 4 |] flatList

        CollectionAssert.AreEqual ([| 1; 2; 3; 4; 5 |], FlatList.toArray result)

    [<TestMethod>]
    [<ExpectedException(typeof<ArgumentOutOfRangeException>)>]
    member _.``insertManyAt throws for negative index`` () =
        let flatList = FlatList.ofArray [| 1; 2; 3 |]
        FlatList.insertManyAt -1 [| 4; 5 |] flatList |> ignore

    [<TestMethod>]
    [<ExpectedException(typeof<ArgumentOutOfRangeException>)>]
    member _.``insertManyAt throws for index too high`` () =
        let flatList = FlatList.ofArray [| 1; 2; 3 |]
        FlatList.insertManyAt 4 [| 4; 5 |] flatList |> ignore

    [<TestMethod>]
    member _.``copy creates a new identical list`` () =
        let original = FlatList.ofArray [| 1; 2; 3 |]
        let copied = FlatList.copy original

        // Should be equal but not the same reference
        CollectionAssert.AreEqual (FlatList.toArray original, FlatList.toArray copied)

        // Since ImmutableArray is a value type with structural equality,
        // these should actually be equal (not reference equality)
        Assert.AreEqual<FlatList<int>> (original, copied)
