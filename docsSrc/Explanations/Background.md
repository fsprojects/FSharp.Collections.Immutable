---
title: Background
category: Explanations
categoryindex: 3
index: 1
---

# Background

## System.Collections.Immutable and F#

[System.Collections.Immutable](https://learn.microsoft.com/en-us/dotnet/api/system.collections.immutable) is a high-performance .NET library providing a suite of immutable collection types, such as arrays, lists, stacks, queues, dictionaries, and sets. These collections are designed for scenarios where data structures need to be shared safely across threads or require non-destructive updates, making them ideal for functional programming patterns.

While F# has its own built-in immutable collections, System.Collections.Immutable collections are engineered for performance and scalability, especially in concurrent and multi-threaded environments. They use advanced algorithms to minimize memory allocations and maximize efficiency when creating modified copies of collections.

**FSharp.Collections.Immutable** provides idiomatic F# bindings for these .NET collections, allowing F# developers to leverage their performance and safety benefits with a familiar F#-style API.

## Why Use System.Collections.Immutable in F#?

- **Performance**: Optimized for fast structural sharing and minimal memory overhead compared to standard F# collections in certain scenarios.
- **Thread Safety**: Immutable by design, making them safe for concurrent access without locks.
- **Rich API**: Feature-rich and consistent with .NET ecosystem standards.
- **Interoperability**: Seamless integration with C# and other .NET languages.

## Available Collections

- `FlatList` (`ImmutableArray`)
- `ImmutableList`
- `Stack` (`ImmutableStack`)
- `Queue` (`ImmutableQueue`)
- `HashMap` (`ImmutableDictionary`)
- `SortedMap` (`ImmutableSortedDictionary`)
- `HashSet` (`ImmutableHashSet`)
- `SortedSet` (`ImmutableSortedSet`)
- `IIndexedSeq` (`IReadOnlyList`)

---

FSharp.Collections.Immutable enables F# developers to use these performant, thread-safe, and feature-rich immutable collections in a natural and idiomatic way.
