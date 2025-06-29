namespace FSharp.Collections.Immutable.Tests.FlatList

open System
open System.Collections.Immutable
open System.Collections.Generic
open Microsoft.VisualStudio.TestTools.UnitTesting
open FSharp.Collections.Immutable

[<TestClass>]
type SortTests () =

    [<TestMethod>]
    member _.``sort orders elements using default comparer`` () =
        let flatList = FlatList.ofArray [| 3; 1; 4; 2; 5 |]
        let result = FlatList.sort flatList

        CollectionAssert.AreEqual ([| 1; 2; 3; 4; 5 |], FlatList.toArray result)

    [<TestMethod>]
    member _.``sortDescending orders elements in reverse`` () =
        let flatList = FlatList.ofArray [| 3; 1; 4; 2; 5 |]
        let result = FlatList.sortDescending flatList

        CollectionAssert.AreEqual ([| 5; 4; 3; 2; 1 |], FlatList.toArray result)

    [<TestMethod>]
    member _.``sortBy orders using key selector`` () =
        let flatList = FlatList.ofArray [| "apple"; "banana"; "cherry"; "date"; "fig" |]
        let result = FlatList.sortBy String.length flatList

        // Should be sorted by length: "fig", "date", "apple", "banana", "cherry"
        CollectionAssert.AreEqual ([| "fig"; "date"; "apple"; "banana"; "cherry" |], FlatList.toArray result)

    [<TestMethod>]
    member _.``sortByDescending orders using key selector in reverse`` () =
        let flatList = FlatList.ofArray [| "apple"; "banana"; "cherry"; "date"; "fig" |]
        let result = FlatList.sortByDescending String.length flatList

        // Should be sorted by length descending: "banana", "cherry", "apple", "date", "fig"
        CollectionAssert.AreEqual ([| "banana"; "cherry"; "apple"; "date"; "fig" |], FlatList.toArray result)

    [<TestMethod>]
    member _.``sortWith uses custom comparison function`` () =
        let flatList = FlatList.ofArray [| 3; 1; 4; 2; 5 |]

        // Sort using a custom comparison that compares the remainder when divided by 3
        let result = FlatList.sortWith (fun x y -> compare (x % 3) (y % 3)) flatList

        // Should be: 3, 6, 9 (rem 0), then 1, 4, 7 (rem 1), then 2, 5, 8 (rem 2)
        // From our input: 3 (rem 0), then 1, 4 (rem 1), then 2, 5 (rem 2)
        CollectionAssert.AreEqual ([| 3; 1; 4; 2; 5 |], FlatList.toArray result)

    [<TestMethod>]
    member _.``sortWithComparer uses IComparer`` () =
        let flatList = FlatList.ofArray [| 3; 1; 4; 2; 5 |]

        let reverseComparer =
            { new IComparer<int> with
                member _.Compare (x, y) = compare y x
            }

        let result = FlatList.sortWithComparer reverseComparer flatList

        // Should sort in reverse
        CollectionAssert.AreEqual ([| 5; 4; 3; 2; 1 |], FlatList.toArray result)

    [<TestMethod>]
    member _.``sortRange sorts portion of list`` () =
        let flatList = FlatList.ofArray [| 3; 1; 4; 2; 5 |]

        // Sort only elements 1, 2, 3 (indices 1, 2, 3)
        let result = FlatList.sortRange 1 3 flatList

        // Should be: 3, 1, 2, 4, 5
        CollectionAssert.AreEqual ([| 3; 1; 2; 4; 5 |], FlatList.toArray result)

    [<TestMethod>]
    member _.``sortRangeWith sorts portion with custom comparison`` () =
        let flatList = FlatList.ofArray [| 3; 1; 4; 2; 5 |]

        // Sort only elements 1, 2, 3 (indices 1, 2, 3) in reverse
        let result = FlatList.sortRangeWith (fun x y -> compare y x) 1 3 flatList

        // Should be: 3, 4, 2, 1, 5
        CollectionAssert.AreEqual ([| 3; 4; 2; 1; 5 |], FlatList.toArray result)

    [<TestMethod>]
    member _.``sortRangeWithComparer uses IComparer for portion`` () =
        let flatList = FlatList.ofArray [| 3; 1; 4; 2; 5 |]

        let reverseComparer =
            { new IComparer<int> with
                member _.Compare (x, y) = compare y x
            }

        // Sort only elements 1, 2, 3 (indices 1, 2, 3) using reverse comparer
        let result = FlatList.sortRangeWithComparer reverseComparer 1 3 flatList

        // Should be: 3, 4, 2, 1, 5
        CollectionAssert.AreEqual ([| 3; 4; 2; 1; 5 |], FlatList.toArray result)

    [<TestMethod>]
    member _.``compareWith compares elements`` () =
        let list1 = FlatList.ofArray [| 1; 2; 3 |]
        let list2 = FlatList.ofArray [| 1; 2; 4 |]
        let list3 = FlatList.ofArray [| 1; 2; 3; 4 |]

        // Custom comparer that considers elements equal if both odd or both even
        let comparer x y =
            if x % 2 = y % 2 then 0
            elif x % 2 < y % 2 then -1
            else 1

        // list1 and list2 differ at position 2, where 3 and 4 have same parity
        Assert.AreEqual<int> (1, FlatList.compareWith comparer list1 list2)

        // list1 is shorter than list3 (where all elements match)
        Assert.AreEqual<int> (-1, FlatList.compareWith comparer list1 list3)

        // list3 is longer than list2 (where all elements match)
        Assert.AreEqual<int> (1, FlatList.compareWith comparer list3 list2)

        // Standard comparison should still work normally
        Assert.AreEqual<int> (-1, FlatList.compareWith compare list1 list2)
        Assert.AreEqual<int> (-1, FlatList.compareWith compare list1 list3)
        Assert.AreEqual<int> (1, FlatList.compareWith compare list2 list1)
