namespace FSharp.Collections.Immutable.Tests

open System.Collections.Immutable
open Microsoft.VisualStudio.TestTools.UnitTesting
open FSharp.Collections.Immutable

[<TestClass; EquatableArrayTestCategory; PatternMatchingTestCategory>]
type EquatableArrayPatternTests () =

    /// Names the pattern that an array of up to five elements matches first.
    static let describe (array : EquatableArray<int>) =
        match array with
        | Arr0 -> "none"
        | Arr1 single -> $"one {single}"
        | Arr2 (first, second) -> $"two {first} {second}"
        | Arr3 (first, second, third) -> $"three {first} {second} {third}"
        | ArrN 4 -> $"four {array[3]}"
        | _ -> "more"

    [<TestMethod>]
    member _.``each length matches its pattern and longer arrays fall through`` () =
        Assert.AreEqual ("none", describe EquatableArray.empty, "the empty array must match Arr0")
        Assert.AreEqual ("one 1", describe (ea [ 1 ]), "one element must match Arr1")
        Assert.AreEqual ("two 1 2", describe (ea [ 1; 2 ]), "two elements must match Arr2")
        Assert.AreEqual ("three 1 2 3", describe (ea [ 1; 2; 3 ]), "three elements must match Arr3")
        Assert.AreEqual ("four 4", describe (ea [ 1; 2; 3; 4 ]), "four elements must match ArrN 4")
        Assert.AreEqual ("more", describe (ea [ 1; 2; 3; 4; 5 ]), "five elements must fall through to the next case")

    [<TestMethod>]
    member _.``the default value matches Arr0`` () =
        Assert.AreEqual ("none", describe Unchecked.defaultof<EquatableArray<int>>, "the default value must match Arr0")

    [<TestMethod>]
    member _.``a struct tuple payload also binds with the struct keyword`` () =
        let sum =
            match ea [ 1; 2 ] with
            | Arr2 (struct (first, second)) -> first + second
            | _ -> 0

        Assert.AreEqual (3, sum, "Arr2 must bind the struct tuple with the struct keyword too")

    [<TestMethod>]
    member _.``a pattern nested in a union case matches the children`` () =
        let sum =
            match Branch (ea [ Leaf 1; Leaf 2 ]) with
            | Branch (Arr2 (Leaf first, Leaf second)) -> first + second
            | _ -> 0

        Assert.AreEqual (3, sum, "Arr2 must match the children of a union case and their own patterns")

    [<TestMethod>]
    member _.``an ImmutableArray matches after a wrap without a copy`` () =
        let items = ImmutableArray.Create (7, 8)

        let matched =
            match EquatableArray items with
            | Arr2 (first, second) -> first * 10 + second
            | _ -> 0

        Assert.AreEqual (78, matched, "a wrapped ImmutableArray must match the patterns")
