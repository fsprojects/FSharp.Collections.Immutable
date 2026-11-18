---
title: How To use a flat list with sequences
category: How To Guides
categoryindex: 2
index: 3
---

# How To use a flat list with sequences

A `FlatList<'T>` is an `ImmutableArray<'T>`, which is a struct. A function that takes a `seq<'T>` boxes it: the call
allocates one object of 24 bytes on a 64-bit runtime, whatever the length of the flat list, and copies no element. The
boxed flat list implements `IReadOnlyList<'T>`, so `Seq.length`, `Seq.last` and `Seq.toArray` do not enumerate it.

The box is rarely the problem. What matters is how a pipeline reads the elements.

## When every element is read

The functions of `FlatList` work on the array under the flat list. They box nothing and create no enumerator:

``` F#
open FSharp.Collections.Immutable

let numbers = FlatList.init 10_000 id

let total = numbers |> FlatList.map (fun number -> number + 1) |> FlatList.sum
```

Each step builds a flat list. That is the cheapest way to read all the elements, and the wrong one when a step can stop
early.

## When a pipeline stops early

The functions of `Seq` are lazy: a pipeline that needs a few elements reads only those. The flat list is boxed once:

``` F#
let firstFive =
    numbers
    |> Seq.filter (fun number -> number % 2 = 0)
    |> Seq.map (fun number -> number + 1)
    |> Seq.truncate 5
    |> FlatList.ofSeq
```

With `FlatList.filter` and `FlatList.map`, the same pipeline builds two flat lists of 5 000 elements first.

## With the methods of LINQ

`System.Linq` has extension methods for `ImmutableArray<'T>`. F# binds a call on a flat list to them, and they read the
array directly:

``` F#
open System.Linq

let firstFiveWithLinq =
    numbers.Where(fun number -> number % 2 = 0).Select(fun number -> number + 1)
    |> Seq.truncate 5
    |> FlatList.ofSeq

let hasNegative = numbers.Any (fun number -> number < 0)
```

Only the first call of a chain reads the flat list. The rest work on the iterator that it returns, which is a class.

`Where`, `Select`, `Any`, `All`, `Aggregate` and `ToArray` have a version for `ImmutableArray<'T>`. The other methods,
such as `Sum`, `Count`, `Take` and `OrderBy`, are the methods of `System.Linq.Enumerable`: called on a flat list, they
box it and read it through an enumerator object. `Seq.sum` does the same, so `FlatList.sum` reads a flat list faster
than both.

## Where a sequence is taken

A function of `FlatList` that takes a sequence recognises a flat list: `FlatList.ofSeq` returns it as it is. In
`flatList { }`, `yield!` and `for` take a sequence, an array or a flat list, and read an array or a flat list directly,
without a box:

``` F#
let combined = flatList {
    yield! numbers

    for number in FlatList.ofArray [| 1; 2; 3 |] do
        yield number * 10

    yield! seq { 100..102 }
}
```

The compiler chooses the overload of `for` by the type of the source, which must be known where the loop is written. A
function parameter without a type annotation does not compile as the source of a loop, as it does not for `yield!`.
Annotate it:

``` F#
let doubled (items : FlatList<int>) = flatList { for item in items do yield item * 2 }
```
