# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

* `FlatList` has every function of the FSharp.Core `Array` module that applies to an immutable array, with its semantics
  and exceptions, plus `add`, `asMemory`, `asSpan`, `binarySearch`, `indexOf`, `indexOfWith`, `lastIndexOf`,
  `lastIndexOfWith`, `ofReadOnlySpan`, `ofSpan`, `remove`, `removeMany`, `removeManyWith`, `removeWith`, `skipLast`,
  `sortWithComparer`, `takeLast` and `tryTail`. `remove` and `removeMany` remove by value in
  `EqualityComparer<'T>.Default`, and the `With` variants take an equality comparer.
* The `FSharp.Collections.Immutable.Option.FlatList` and `FSharp.Collections.Immutable.Tuple.FlatList` modules repeat the
  functions that use value options or struct tuples with options and reference tuples.
* The `FlatListBuilder` module works on the builder of a flat list, and `flatList { }` builds a flat list.

### Changed

* The package targets `net8.0` and `net10.0`.
* The package requires FSharp.Core 10.1.301 or later, the first release with `partitionWith`.
* `FlatList`: the `try*` functions return `voption`; `choose`, `pick`, `tryPick` and `unfold` take functions that return
  `voption`; the functions that return tuples return struct tuples.
* `FlatList`: every function throws what the FSharp.Core `Array` function of the same name throws. `take` and `splitAt`
  throw for a count beyond the length, and a default `ImmutableArray` throws `ArgumentNullException`, also when it is
  given as a sequence.
* `FlatList.sub` has the meaning of `Array.sub`: `sub list startIndex count` returns `count` elements from `startIndex`.
  It was `sub start stop list`.

### Removed

* `FlatList.index`, `indexWith`, `indexFrom`, `indexFromWith`, `indexRange`, `indexRangeWith` and their `lastIndex`
  counterparts: use `indexOf`, `indexOfWith`, `lastIndexOf` and `lastIndexOfWith`.
* `FlatList.removeAll`, `removeAllWith` and `removeRange`: use `removeMany`, `removeManyWith` and `removeManyAt`, and
  `filter` with the negated predicate to remove the elements that satisfy a predicate.
* The `ImmutableArray` module alias of `FlatList`.
* The `Seq` module of conversions between the collections.

## [1.0.0] - 2020-02-23

First release

### Implemented type abbreviations and modules with functions for:
* `FlatList` (`ImmutableArray`)
* `ImmutableList`
* `Stack` (`ImmutableStack`)
* `Queue` (`ImmutableQueue`)
* `HashMap` (`ImmutableDictionary`)
* `SortedMap` (`ImmutableSortedDictionary`)
* `HashSet` (`ImmutableHashSet`)
* `SortedSet` (`ImmutableSortedSet`)
* `IIndexedSeq` (`IReadOnlyList`)

[Unreleased]: https://github.com/fsprojects/FSharp.Collections.Immutable/compare/releases/1.0.0...HEAD
[1.0.0]: https://github.com/fsprojects/FSharp.Collections.Immutable/releases/tag/releases/1.0.0
