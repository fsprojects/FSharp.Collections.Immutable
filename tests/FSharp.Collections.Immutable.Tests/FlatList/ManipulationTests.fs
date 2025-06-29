namespace FSharp.Collections.Immutable.Tests.FlatList

open System
open System.Collections.Immutable
open System.Collections.Generic
open Microsoft.VisualStudio.TestTools.UnitTesting
open FSharp.Collections.Immutable

[<TestClass>]
type ManipulationTests () =

    [<TestMethod>]
    member _.``append combines two FlatLists`` () =
        let list1 = FlatList.ofArray [| 1; 2 |]
        let list2 = FlatList.ofArray [| 3; 4 |]

        let result = FlatList.append list1 list2

        Assert.AreEqual<int> (4, result.Length)
        Assert.AreEqual<int> (1, result.[0])
        Assert.AreEqual<int> (2, result.[1])
        Assert.AreEqual<int> (3, result.[2])
        Assert.AreEqual<int> (4, result.[3])

    [<TestMethod>]
    member _.``concat combines multiple FlatLists`` () =
        let lists =
            FlatList.ofArray [| FlatList.ofArray [| 1; 2 |]; FlatList.ofArray [| 3; 4 |]; FlatList.ofArray [| 5; 6 |] |]

        let result = FlatList.concat lists

        Assert.AreEqual<int> (6, result.Length)
        for i = 0 to 5 do
            Assert.AreEqual<int> (i + 1, result.[i])

    [<TestMethod>]
    member _.``take returns first N elements`` () =
        let flatList = FlatList.ofArray [| 10; 20; 30; 40; 50 |]

        let result1 = FlatList.take 3 flatList
        Assert.AreEqual<int> (3, result1.Length)
        Assert.AreEqual<int> (10, result1.[0])
        Assert.AreEqual<int> (20, result1.[1])
        Assert.AreEqual<int> (30, result1.[2])

        let result2 = FlatList.take 0 flatList
        Assert.AreEqual<int> (0, result2.Length)

        let result3 = FlatList.take 10 flatList
        Assert.AreEqual<int> (5, result3.Length)

    [<TestMethod>]
    [<ExpectedException(typeof<ArgumentException>)>]
    member _.``take throws for negative count`` () =
        let flatList = FlatList.ofArray [| 1; 2; 3 |]
        FlatList.take -1 flatList |> ignore

    [<TestMethod>]
    member _.``takeWhile returns elements while predicate is true`` () =
        let flatList = FlatList.ofArray [| 2; 4; 6; 7; 8; 10 |]
        let result = FlatList.takeWhile (fun x -> x % 2 = 0) flatList

        Assert.AreEqual<int> (3, result.Length)
        Assert.AreEqual<int> (2, result.[0])
        Assert.AreEqual<int> (4, result.[1])
        Assert.AreEqual<int> (6, result.[2])

    [<TestMethod>]
    member _.``skip returns all but first N elements`` () =
        let flatList = FlatList.ofArray [| 10; 20; 30; 40; 50 |]

        let result1 = FlatList.skip 2 flatList
        Assert.AreEqual<int> (3, result1.Length)
        Assert.AreEqual<int> (30, result1.[0])
        Assert.AreEqual<int> (40, result1.[1])
        Assert.AreEqual<int> (50, result1.[2])

        let result2 = FlatList.skip 0 flatList
        Assert.AreEqual<int> (5, result2.Length)

        let result3 = FlatList.skip 5 flatList
        Assert.AreEqual<int> (0, result3.Length)

        let result4 = FlatList.skip 10 flatList
        Assert.AreEqual<int> (0, result4.Length)

    [<TestMethod>]
    [<ExpectedException(typeof<ArgumentException>)>]
    member _.``skip throws for negative count`` () =
        let flatList = FlatList.ofArray [| 1; 2; 3 |]
        FlatList.skip -1 flatList |> ignore

    [<TestMethod>]
    member _.``skipWhile skips elements while predicate is true`` () =
        let flatList = FlatList.ofArray [| 2; 4; 6; 7; 8; 10 |]
        let result = FlatList.skipWhile (fun x -> x % 2 = 0) flatList

        Assert.AreEqual<int> (3, result.Length)
        Assert.AreEqual<int> (7, result.[0])
        Assert.AreEqual<int> (8, result.[1])
        Assert.AreEqual<int> (10, result.[2])

    [<TestMethod>]
    member _.``sub gets a sublist`` () =
        let flatList = FlatList.ofArray [| 10; 20; 30; 40; 50 |]
        let result = FlatList.sub 1 3 flatList

        Assert.AreEqual<int> (3, result.Length)
        Assert.AreEqual<int> (20, result.[0])
        Assert.AreEqual<int> (30, result.[1])
        Assert.AreEqual<int> (40, result.[2])

    [<TestMethod>]
    member _.``truncate limits to at most N elements`` () =
        let flatList = FlatList.ofArray [| 10; 20; 30; 40; 50 |]

        let result1 = FlatList.truncate 3 flatList
        Assert.AreEqual<int> (3, result1.Length)
        Assert.AreEqual<int> (10, result1.[0])
        Assert.AreEqual<int> (20, result1.[1])
        Assert.AreEqual<int> (30, result1.[2])

        let result2 = FlatList.truncate 10 flatList
        Assert.AreEqual<int> (5, result2.Length)

    [<TestMethod>]
    member _.``splitAt splits list at index`` () =
        let flatList = FlatList.ofArray [| 10; 20; 30; 40; 50 |]
        let first, second = FlatList.splitAt 2 flatList

        Assert.AreEqual<int> (2, first.Length)
        Assert.AreEqual<int> (10, first.[0])
        Assert.AreEqual<int> (20, first.[1])

        Assert.AreEqual<int> (3, second.Length)
        Assert.AreEqual<int> (30, second.[0])
        Assert.AreEqual<int> (40, second.[1])
        Assert.AreEqual<int> (50, second.[2])

    [<TestMethod>]
    member _.``updateAt updates element at given index`` () =
        let flatList = FlatList.ofArray [| 10; 20; 30 |]
        let result = FlatList.updateAt 1 99 flatList

        Assert.AreEqual<int> (3, result.Length)
        Assert.AreEqual<int> (10, result.[0])
        Assert.AreEqual<int> (99, result.[1])
        Assert.AreEqual<int> (30, result.[2])

    [<TestMethod>]
    member _.``removeAt removes element at given index`` () =
        let flatList = FlatList.ofArray [| 10; 20; 30 |]
        let result = FlatList.removeAt 1 flatList

        Assert.AreEqual<int> (2, result.Length)
        Assert.AreEqual<int> (10, result.[0])
        Assert.AreEqual<int> (30, result.[1])

    [<TestMethod>]
    member _.``insertAt inserts element at given index`` () =
        let flatList = FlatList.ofArray [| 10; 20; 30 |]

        // Insert at beginning
        let result1 = FlatList.insertAt 0 5 flatList
        Assert.AreEqual<int> (4, result1.Length)
        Assert.AreEqual<int> (5, result1.[0])
        Assert.AreEqual<int> (10, result1.[1])

        // Insert in middle
        let result2 = FlatList.insertAt 2 25 flatList
        Assert.AreEqual<int> (4, result2.Length)
        Assert.AreEqual<int> (10, result2.[0])
        Assert.AreEqual<int> (20, result2.[1])
        Assert.AreEqual<int> (25, result2.[2])
        Assert.AreEqual<int> (30, result2.[3])

        // Insert at end
        let result3 = FlatList.insertAt 3 40 flatList
        Assert.AreEqual<int> (4, result3.Length)
        Assert.AreEqual<int> (10, result3.[0])
        Assert.AreEqual<int> (20, result3.[1])
        Assert.AreEqual<int> (30, result3.[2])
        Assert.AreEqual<int> (40, result3.[3])

    [<TestMethod>]
    member _.``insertManyAt inserts multiple elements at given index`` () =
        let flatList = FlatList.ofArray [| 10; 20; 30 |]
        let valuesToInsert = [| 21; 22; 23 |]

        let result = FlatList.insertManyAt 2 valuesToInsert flatList

        Assert.AreEqual<int> (6, result.Length)
        Assert.AreEqual<int> (10, result.[0])
        Assert.AreEqual<int> (20, result.[1])
        Assert.AreEqual<int> (21, result.[2])
        Assert.AreEqual<int> (22, result.[3])
        Assert.AreEqual<int> (23, result.[4])
        Assert.AreEqual<int> (30, result.[5])

    [<TestMethod>]
    member _.``removeRange removes elements in range`` () =
        let flatList = FlatList.ofArray [| 10; 20; 30; 40; 50 |]
        let result = FlatList.removeRange 1 3 flatList

        Assert.AreEqual<int> (2, result.Length)
        Assert.AreEqual<int> (10, result.[0])
        Assert.AreEqual<int> (50, result.[1])

    [<TestMethod>]
    member _.``removeAll removes specified elements`` () =
        let flatList = FlatList.ofArray [| 10; 20; 30; 20; 40 |]
        let itemsToRemove = [| 20; 30 |]

        let result = FlatList.removeAll itemsToRemove flatList

        Assert.AreEqual<int> (2, result.Length)
        Assert.AreEqual<int> (10, result.[0])
        Assert.AreEqual<int> (40, result.[1])

    [<TestMethod>]
    member _.``except removes elements from another collection`` () =
        let flatList = FlatList.ofArray [| 1; 2; 3; 4; 5; 3; 1 |]
        let toExclude = [| 1; 3; 9 |]

        let result = FlatList.except toExclude flatList

        Assert.AreEqual<int> (3, result.Length)
        Assert.IsTrue (FlatList.contains 2 result)
        Assert.IsTrue (FlatList.contains 4 result)
        Assert.IsTrue (FlatList.contains 5 result)

    [<TestMethod>]
    member _.``takeEnd returns last N elements`` () =
        let flatList = FlatList.ofArray [| 10; 20; 30; 40; 50 |]

        let result1 = FlatList.takeEnd 3 flatList
        Assert.AreEqual<int> (3, result1.Length)
        Assert.AreEqual<int> (30, result1.[0])
        Assert.AreEqual<int> (40, result1.[1])
        Assert.AreEqual<int> (50, result1.[2])

        let result2 = FlatList.takeEnd 0 flatList
        Assert.AreEqual<int> (0, result2.Length)

    [<TestMethod>]
    [<ExpectedException(typeof<ArgumentException>)>]
    member _.``takeEnd throws for negative count`` () =
        let flatList = FlatList.ofArray [| 1; 2; 3 |]
        FlatList.takeEnd -1 flatList |> ignore

    [<TestMethod>]
    member _.``skipEnd skips last N elements`` () =
        let flatList = FlatList.ofArray [| 10; 20; 30; 40; 50 |]

        let result1 = FlatList.skipEnd 2 flatList
        Assert.AreEqual<int> (3, result1.Length)
        Assert.AreEqual<int> (10, result1.[0])
        Assert.AreEqual<int> (20, result1.[1])
        Assert.AreEqual<int> (30, result1.[2])

        let result2 = FlatList.skipEnd 0 flatList
        Assert.AreEqual<int> (5, result2.Length)

    [<TestMethod>]
    [<ExpectedException(typeof<ArgumentException>)>]
    member _.``skipEnd throws for negative count`` () =
        let flatList = FlatList.ofArray [| 1; 2; 3 |]
        FlatList.skipEnd -1 flatList |> ignore

    [<TestMethod>]
    member _.``findDup finds first duplicate element`` () =
        let flatList = FlatList.ofArray [| 1; 2; 3; 2; 4 |]
        let result = FlatList.findDup flatList

        Assert.AreEqual<int> (2, result)

    [<TestMethod>]
    [<ExpectedException(typeof<KeyNotFoundException>)>]
    member _.``findDup throws when no duplicates`` () =
        let flatList = FlatList.ofArray [| 1; 2; 3 |]
        FlatList.findDup flatList |> ignore

    [<TestMethod>]
    member _.``findDupBy finds first duplicate element by projection`` () =
        let flatList = FlatList.ofArray [| "apple"; "banana"; "avocado"; "cherry" |]
        let result = FlatList.findDupBy (fun (s : string) -> s.[0]) flatList

        Assert.AreEqual<string> ("avocado", result)

    [<TestMethod>]
    [<ExpectedException(typeof<KeyNotFoundException>)>]
    member _.``findDupBy throws when no duplicates`` () =
        let flatList = FlatList.ofArray [| "apple"; "banana"; "cherry" |]
        FlatList.findDupBy (fun (s : string) -> s.[0]) flatList
        |> ignore

    [<TestMethod>]
    member _.``unfold generates FlatList from state`` () =
        let result =
            FlatList.unfold
                (fun state ->
                    if state > 5 then
                        ValueNone
                    else
                        ValueSome (struct (state, state + 1))
                )
                1

        Assert.AreEqual<int> (5, result.Length)
        Assert.AreEqual<int> (1, result.[0])
        Assert.AreEqual<int> (2, result.[1])
        Assert.AreEqual<int> (3, result.[2])
        Assert.AreEqual<int> (4, result.[3])
        Assert.AreEqual<int> (5, result.[4])

    [<TestMethod>]
    member _.``unfold handles empty generation`` () =
        let result = FlatList.unfold (fun _ -> ValueNone) 1

        Assert.AreEqual<int> (0, result.Length)
        Assert.IsTrue (FlatList.isEmpty result)

    [<TestMethod>]
    member _.``compareWith compares two FlatLists`` () =
        let list1 = FlatList.ofArray [| 1; 2; 3 |]
        let list2 = FlatList.ofArray [| 1; 2; 3 |]
        let list3 = FlatList.ofArray [| 1; 2; 4 |]
        let list4 = FlatList.ofArray [| 1; 2 |]

        Assert.AreEqual<int> (0, FlatList.compareWith compare list1 list2)
        Assert.IsTrue (FlatList.compareWith compare list1 list3 < 0)
        Assert.IsTrue (FlatList.compareWith compare list3 list1 > 0)
        Assert.IsTrue (FlatList.compareWith compare list4 list1 < 0)
        Assert.IsTrue (FlatList.compareWith compare list1 list4 > 0)

    [<TestMethod>]
    member _.``transpose transposes matrix of lists`` () =
        let matrix =
            FlatList.ofArray [| FlatList.ofArray [| 1; 2; 3 |]; FlatList.ofArray [| 4; 5; 6 |]; FlatList.ofArray [| 7; 8; 9 |] |]

        let result = FlatList.transpose matrix

        Assert.AreEqual<int> (3, result.Length)

        let col1 = result.[0]
        Assert.AreEqual<int> (3, col1.Length)
        Assert.AreEqual<int> (1, col1.[0])
        Assert.AreEqual<int> (4, col1.[1])
        Assert.AreEqual<int> (7, col1.[2])

        let col2 = result.[1]
        Assert.AreEqual<int> (3, col2.Length)
        Assert.AreEqual<int> (2, col2.[0])
        Assert.AreEqual<int> (5, col2.[1])
        Assert.AreEqual<int> (8, col2.[2])

        let col3 = result.[2]
        Assert.AreEqual<int> (3, col3.Length)
        Assert.AreEqual<int> (3, col3.[0])
        Assert.AreEqual<int> (6, col3.[1])
        Assert.AreEqual<int> (9, col3.[2])
