namespace FSharp.Collections.Immutable.Tests.FlatList

open System
open System.Collections.Immutable
open System.Collections.Generic
open Microsoft.VisualStudio.TestTools.UnitTesting
open FSharp.Collections.Immutable

[<TestClass>]
type MapFoldTests () =

    [<TestMethod>]
    member _.``mapFold transforms elements and accumulates state`` () =
        let flatList = FlatList.ofArray [| 1; 2; 3; 4 |]
        let mapped, total = FlatList.mapFold<int, int, int> (fun state x -> (x * 2, state + x)) 0 flatList

        // Mapped elements are doubled: [2; 4; 6; 8]
        // State accumulates the sum of original elements: 0 + 1 + 2 + 3 + 4 = 10
        Assert.AreEqual<int> (4, mapped.Length)
        Assert.AreEqual<int> (2, mapped.[0])
        Assert.AreEqual<int> (4, mapped.[1])
        Assert.AreEqual<int> (6, mapped.[2])
        Assert.AreEqual<int> (8, mapped.[3])
        Assert.AreEqual<int> (10, total)

    [<TestMethod>]
    member _.``mapFold works with empty list`` () =
        let emptyList = FlatList.empty<int>
        let mapped, state = FlatList.mapFold<int, int, int> (fun state x -> (x * 2, state + x)) 42 emptyList

        // Should return an empty list and the initial state unchanged
        Assert.IsTrue (mapped.IsEmpty)
        Assert.AreEqual<int> (42, state)

    [<TestMethod>]
    member _.``mapFold with string concatenation`` () =
        let flatList = FlatList.ofArray [| "a"; "b"; "c" |]
        let mapped, concatenated =
            FlatList.mapFold<string, string, string> (fun state x -> (x.ToUpper (), state + x)) "" flatList

        // Mapped elements are uppercase: ["A"; "B"; "C"]
        // State concatenates the original strings: "abc"
        Assert.AreEqual<int> (3, mapped.Length)
        Assert.AreEqual<string> ("A", mapped.[0])
        Assert.AreEqual<string> ("B", mapped.[1])
        Assert.AreEqual<string> ("C", mapped.[2])
        Assert.AreEqual<string> ("abc", concatenated)

    [<TestMethod>]
    member _.``mapFoldBack transforms elements and accumulates state in reverse`` () =
        let flatList = FlatList.ofArray [| 1; 2; 3; 4 |]
        let mapped, total = FlatList.mapFoldBack<int, int, int> (fun x state -> (x * 2, state + x)) flatList 0

        // Mapped elements are doubled: [2; 4; 6; 8]
        // State accumulates in reverse: 0 + 4 + 3 + 2 + 1 = 10
        Assert.AreEqual<int> (4, mapped.Length)
        Assert.AreEqual<int> (2, mapped.[0])
        Assert.AreEqual<int> (4, mapped.[1])
        Assert.AreEqual<int> (6, mapped.[2])
        Assert.AreEqual<int> (8, mapped.[3])
        Assert.AreEqual<int> (10, total)

    [<TestMethod>]
    member _.``mapFoldBack works with empty list`` () =
        let emptyList = FlatList.empty<int>
        let mapped, state = FlatList.mapFoldBack<int, int, int> (fun x state -> (x * 2, state + x)) emptyList 42

        // Should return an empty list and the initial state unchanged
        Assert.IsTrue (mapped.IsEmpty)
        Assert.AreEqual<int> (42, state)

    [<TestMethod>]
    member _.``mapFoldBack creates indices in reversed order`` () =
        let chars = FlatList.ofArray [| 'a'; 'b'; 'c' |]
        let indices, count = FlatList.mapFoldBack<char, int, int> (fun _ state -> (state, state + 1)) chars 0

        // Creates indices in reversed order: [2; 1; 0]
        // Final state is the count of elements: 3
        Assert.AreEqual<int> (3, indices.Length)
        Assert.AreEqual<int> (2, indices.[0])
        Assert.AreEqual<int> (1, indices.[1])
        Assert.AreEqual<int> (0, indices.[2])
        Assert.AreEqual<int> (3, count)
