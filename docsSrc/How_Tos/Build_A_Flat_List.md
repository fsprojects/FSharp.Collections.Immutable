---
title: How To build a flat list
category: How To Guides
categoryindex: 2
index: 1
---

# How To build a flat list

A `FlatList<'T>` is an `ImmutableArray<'T>`: a function that adds an element returns a new array. To build a flat list
from many pieces, collect the elements first and create the flat list once.

## From elements that you already have

``` F#
open FSharp.Collections.Immutable

let fromArray = FlatList.ofArray [| 1; 2; 3 |]
let fromList = FlatList.ofList [ 1; 2; 3 ]
let fromSeq = FlatList.ofSeq (seq { 1..3 })
let squares = FlatList.init 5 (fun index -> index * index)
```

## With a computation expression

`flatList { }` takes `yield`, `yield!`, loops and conditions, and creates the flat list when the expression ends:

``` F#
let evenSquares = flatList {
    for number in 1..10 do
        if number % 2 = 0 then
            yield number * number

    yield! [| 121; 144 |]
}
```

## With a builder

`FlatList.build` gives a builder to a function and returns the flat list of the elements that the function added. The
functions of the `FlatListBuilder` module that change the builder return it, so they form a pipeline:

``` F#
let built =
    FlatList.build (fun builder ->
        builder
        |> FlatListBuilder.add 1
        |> FlatListBuilder.addRange [ 2; 3 ]
        |> FlatListBuilder.insertAt 0 0
        |> ignore
    )
```

`FlatList.update` does the same with the elements of an existing flat list, which stays unchanged:

``` F#
let withoutFirst =
    built
    |> FlatList.update (fun builder -> builder |> FlatListBuilder.removeAt 0 |> ignore)
```

## Without a copy

`FlatList.builderWith` creates a builder of a given capacity. When the builder is full, `FlatList.moveFromBuilder` gives
its array to the flat list without a copy and empties the builder:

``` F#
let builder = FlatList.builderWith 3

for number in 1..3 do
    builder.Add number

let moved = FlatList.moveFromBuilder builder
```

`FlatList.moveFromBuilder` throws `InvalidOperationException` when the count of the builder differs from its capacity.
`FlatList.ofBuilder` copies the elements and takes a builder of any count.
