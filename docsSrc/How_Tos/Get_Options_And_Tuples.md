---
title: How To get options and reference tuples
category: How To Guides
categoryindex: 2
index: 2
---

# How To get options and reference tuples

The functions of `FlatList` return value options and struct tuples, which need no allocation:

``` F#
open FSharp.Collections.Immutable

let numbers = FlatList.ofArray [| 1; 2; 3; 4 |]

// ValueSome 2
let firstEven = numbers |> FlatList.tryFind (fun number -> number % 2 = 0)

let struct (evens, odds) = numbers |> FlatList.partition (fun number -> number % 2 = 0)
```

Code that works with `option` and reference tuples does not have to convert these results.

## Options

Open the `FSharp.Collections.Immutable.Option` namespace after `FSharp.Collections.Immutable`. The functions that take or
return a value option then take or return an option:

``` F#
open FSharp.Collections.Immutable
open FSharp.Collections.Immutable.Option

// Some 1
let firstOdd = numbers |> FlatList.tryFind (fun number -> number % 2 = 1)

let halves =
    numbers
    |> FlatList.choose (fun number -> if number % 2 = 0 then Some (number / 2) else None)
```

## Reference tuples

Open the `FSharp.Collections.Immutable.Tuple` namespace after `FSharp.Collections.Immutable`. The functions that return
struct tuples then return reference tuples:

``` F#
open FSharp.Collections.Immutable
open FSharp.Collections.Immutable.Tuple

let small, large = numbers |> FlatList.partition (fun number -> number < 3)

// FlatList<int * string>
let named = FlatList.zip numbers (numbers |> FlatList.map string)
```

The two namespaces hold only the functions whose types differ. Every other function of `FlatList` stays the one of
`FSharp.Collections.Immutable`, so the `open` changes these types and nothing else. Both namespaces can be open at the
same time.
