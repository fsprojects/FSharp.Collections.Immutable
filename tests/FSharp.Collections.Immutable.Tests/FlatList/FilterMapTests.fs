namespace FSharp.Collections.Immutable.Tests.FlatList

open System
open System.Collections.Immutable
open System.Collections.Generic
open Microsoft.VisualStudio.TestTools.UnitTesting
open FSharp.Collections.Immutable

[<TestClass>]
type FilterMapTests () =

    [<TestMethod>]
    member _.``map transforms elements`` () =
        let flatList = FlatList.ofArray [| 1; 2; 3 |]
        let result = FlatList.map (fun x -> x * 2) flatList

        Assert.AreEqual<int> (3, result.Length)
        Assert.AreEqual<int> (2, result.[0])
        Assert.AreEqual<int> (4, result.[1])
        Assert.AreEqual<int> (6, result.[2])

    [<TestMethod>]
    member _.``mapi transforms elements with index`` () =
        let flatList = FlatList.ofArray [| 10; 20; 30 |]
        let result = FlatList.mapi (fun i x -> i + x) flatList

        Assert.AreEqual<int> (3, result.Length)
        Assert.AreEqual<int> (10, result.[0]) // 0 + 10
        Assert.AreEqual<int> (21, result.[1]) // 1 + 20
        Assert.AreEqual<int> (32, result.[2]) // 2 + 30

    [<TestMethod>]
    member _.``map2 transforms pairs of elements`` () =
        let list1 = FlatList.ofArray [| 1; 2; 3 |]
        let list2 = FlatList.ofArray [| 10; 20; 30 |]

        let result = FlatList.map2 (fun x y -> x * y) list1 list2

        Assert.AreEqual<int> (3, result.Length)
        Assert.AreEqual<int> (10, result.[0]) // 1 * 10
        Assert.AreEqual<int> (40, result.[1]) // 2 * 20
        Assert.AreEqual<int> (90, result.[2]) // 3 * 30

    [<TestMethod>]
    [<ExpectedException(typeof<ArgumentException>)>]
    member _.``map2 throws when lists have different lengths`` () =
        let list1 = FlatList.ofArray [| 1; 2 |]
        let list2 = FlatList.ofArray [| 10; 20; 30 |]

        FlatList.map2 (fun x y -> x * y) list1 list2 |> ignore

    [<TestMethod>]
    member _.``mapi2 transforms pairs with index`` () =
        let list1 = FlatList.ofArray [| 1; 2; 3 |]
        let list2 = FlatList.ofArray [| 10; 20; 30 |]

        let result = FlatList.mapi2 (fun i x y -> i + x + y) list1 list2

        Assert.AreEqual<int> (3, result.Length)
        Assert.AreEqual<int> (11, result.[0]) // 0 + 1 + 10
        Assert.AreEqual<int> (23, result.[1]) // 1 + 2 + 20
        Assert.AreEqual<int> (35, result.[2]) // 2 + 3 + 30

    [<TestMethod>]
    [<ExpectedException(typeof<ArgumentException>)>]
    member _.``mapi2 throws when lists have different lengths`` () =
        let list1 = FlatList.ofArray [| 1; 2 |]
        let list2 = FlatList.ofArray [| 10; 20; 30 |]

        FlatList.mapi2 (fun i x y -> i + x + y) list1 list2
        |> ignore

    [<TestMethod>]
    member _.``filter keeps elements matching predicate`` () =
        let flatList = FlatList.ofArray [| 1; 2; 3; 4; 5 |]
        let result = FlatList.filter (fun x -> x % 2 = 0) flatList

        Assert.AreEqual<int> (2, result.Length)
        Assert.AreEqual<int> (2, result.[0])
        Assert.AreEqual<int> (4, result.[1])

    [<TestMethod>]
    member _.``where is alias for filter`` () =
        let flatList = FlatList.ofArray [| 1; 2; 3; 4; 5 |]
        let filtered = FlatList.filter (fun x -> x % 2 = 0) flatList
        let wheered = FlatList.where (fun x -> x % 2 = 0) flatList

        Assert.AreEqual<int> (filtered.Length, wheered.Length)
        for i = 0 to filtered.Length - 1 do
            Assert.AreEqual<int> (filtered.[i], wheered.[i])

    [<TestMethod>]
    member _.``choose selects and maps elements`` () =
        let flatList = FlatList.ofArray [| 1; 2; 3; 4; 5 |]
        let result = FlatList.choose (fun x -> if x % 2 = 0 then ValueSome (x * 10) else ValueNone) flatList

        CollectionAssert.AreEqual ([| 20; 40 |], FlatList.toArray result)

    [<TestMethod>]
    member _.``collect maps and concatenates`` () =
        let flatList = FlatList.ofArray [| 1; 2; 3 |]
        let result = FlatList.collect (fun x -> [ x; x * 10 ]) flatList

        Assert.AreEqual<int> (6, result.Length)
        Assert.AreEqual<int> (1, result.[0])
        Assert.AreEqual<int> (10, result.[1])
        Assert.AreEqual<int> (2, result.[2])
        Assert.AreEqual<int> (20, result.[3])
        Assert.AreEqual<int> (3, result.[4])
        Assert.AreEqual<int> (30, result.[5])

    [<TestMethod>]
    member _.``partition splits elements based on predicate`` () =
        let flatList = FlatList.ofArray [| 1; 2; 3; 4; 5 |]
        let evens, odds = FlatList.partition (fun x -> x % 2 = 0) flatList

        CollectionAssert.AreEqual ([| 2; 4 |], FlatList.toArray evens)
        CollectionAssert.AreEqual ([| 1; 3; 5 |], FlatList.toArray odds)

    [<TestMethod>]
    member _.``distinct removes duplicates`` () =
        let flatList = FlatList.ofArray [| 1; 2; 3; 1; 2; 5 |]
        let result = FlatList.distinct flatList

        Assert.AreEqual<int> (4, result.Length)
        Assert.IsTrue (FlatList.contains 1 result)
        Assert.IsTrue (FlatList.contains 2 result)
        Assert.IsTrue (FlatList.contains 3 result)
        Assert.IsTrue (FlatList.contains 5 result)

    [<TestMethod>]
    member _.``distinctBy removes duplicates using projection`` () =
        let flatList = FlatList.ofArray [| "apple"; "banana"; "apricot"; "berry" |]
        let result = FlatList.distinctBy (fun (s : string) -> s.[0]) flatList

        Assert.AreEqual<int> (2, result.Length)
        // Only one item starting with 'a' and one with 'b'
        Assert.IsTrue (result |> FlatList.exists (fun s -> s.[0] = 'a'))
        Assert.IsTrue (result |> FlatList.exists (fun s -> s.[0] = 'b'))

    [<TestMethod>]
    member _.``indexed pairs elements with their indices`` () =
        let flatList = FlatList.ofArray [| "a"; "b"; "c" |]
        let result = FlatList.indexed flatList

        Assert.AreEqual<int> (3, result.Length)
        Assert.AreEqual<struct (int * string)> (struct (0, "a"), result.[0])
        Assert.AreEqual<struct (int * string)> (struct (1, "b"), result.[1])
        Assert.AreEqual<struct (int * string)> (struct (2, "c"), result.[2])
