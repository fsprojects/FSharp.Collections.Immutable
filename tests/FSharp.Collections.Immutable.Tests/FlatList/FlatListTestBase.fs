namespace FSharp.Collections.Immutable.Tests.FlatList

open System
open System.Collections.Immutable
open System.Collections.Generic
open Microsoft.VisualStudio.TestTools.UnitTesting
open FSharp.Collections.Immutable

[<AutoOpen>]
module TestData =
    let emptyIntList = FlatList.empty<int>
    let singletonIntList = FlatList.singleton 42
    let standardIntList = FlatList.ofArray [| 10; 20; 30; 40; 50 |]
    let evenOddIntList = FlatList.ofArray [| 1; 2; 3; 4; 5; 6 |]
    let repeatedIntList = FlatList.ofArray [| 1; 2; 3; 1; 2; 5 |]
    let stringList = FlatList.ofArray [| "apple"; "banana"; "cherry"; "date" |]
