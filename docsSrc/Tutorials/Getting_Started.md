---
title: Getting Started
category: Tutorials
categoryindex: 1
index: 1
---

# Getting Started

## Installation

First, add the NuGet package to your project:
```
dotnet add package FSharp.Collections.Immutable
```
## Basic Setup

Here's a minimal example to get started:
``` F#
open FSharp.Collections.Immutable
```

``` F#
// Create an immutable list
let list1 = ImmutableList.ofSeq [1; 2; 3]

// Create an immutable hash set
let set1 = HashSet.ofSeq ["a"; "b"]

// Create an immutable dictionary from tuples, key/value pairs or struct tuples
let dict1 = HashMap.ofSeq [ "key1", 42; "key2", 100 ]

// Create an immutable dictionary whose keys ignore the case; the later binding of "KEY1" wins
let dict2 = (HashMap.forKeyComparer System.StringComparer.OrdinalIgnoreCase).ofSeq [ "key1", 42; "KEY1", 100 ]


printfn "List: %A" (list1 |> Seq.toList)
printfn "Set: %A" (set1 |> Seq.toList)
printfn "Dict: %A" (dict1 |> Seq.toList)
printfn "Dict ignoring the case: %A" (dict2 |> Seq.toList)
```

## Next Steps

- Check out the [How-To Guides](../How_Tos/Doing_A_Thing.html) for common scenarios
- Read the [Background](../Explanations/Background.html) for deeper understanding
- Browse the [API Reference](../reference/index.html) for detailed documentation
