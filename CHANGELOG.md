# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.5.0] - 2026-11-06

### Added

* `HashSet` (`ImmutableHashSet`) and `SortedSet` (`ImmutableSortedSet`) modules with `add`, `builder`, `builderWith`,
  `builderWithComparer`, `check`, `clear`, `contains`, `difference`, `empty`, `exists`, `filter`, `forall`, `intersect`,
  `intersectMany`, `isEmpty`, `isProperSubset`, `isProperSuperset`, `isSubset`, `isSuperset`, `iter`, `keyComparer`,
  `length`, `map`, `ofArray`, `ofBuilder`, `ofComparer`, `ofSeq`, `ofSeqWithComparer`, `pick`, `remove`, `singleton`,
  `toArray`, `toBuilder`, `toSeq`, `tryPick`, `union`, `unionMany`, `vTryPick` and `where`.
* `SortedMap` (`ImmutableSortedDictionary`) functions: `add`, `append`, `builder`, `builderWithComparers`,
  `builderWithKeyComparer`, `check`, `clear`, `containsKey`, `except`, `exists`, `filter`, `find`, `findKey`, `forall`,
  `isEmpty`, `iter`, `keyComparer`, `length`, `map`, `ofArray`, `ofBuilder`, `ofComparers`, `ofKeyComparer`, `ofSeq`,
  `ofSeqGroupBy`, `ofSeqWith`, `pick`, `remove`, `singleton`, `toArray`, `toBuilder`, `toSeq`, `tryFind`, `tryFindKey`,
  `tryPick`, `valueComparer`, `vTryFind`, `vTryFindKey`, `vTryPick` and `where`.
* `HashMap` (`ImmutableDictionary`) functions: `builderWithComparers`, `builderWithKeyComparer`, `exists`, `forall`,
  `iter`, `map`, `ofArray`, `ofComparers`, `ofKeyComparer`, `ofSeqGroupBy`, `ofSeqWith`, `pick`, `singleton`, `toArray`,
  `toSeq`, `tryPick`, `valueComparer`, `vTryFind`, `vTryPick` and `where`.
* `FlatList.ofArray`, `FlatList.toArray`, `FlatList.toSeq`, `ImmutableList.ofArray`, `ImmutableList.toSeq`,
  `Queue.toSeq` and `Stack.toSeq`.
* The `Seq` module converts between sequences and the collections: `ofFlatList`, `ofHashMap`, `ofHashSet`,
  `ofImmutableList`, `ofQueue`, `ofSortedMap`, `ofSortedSet`, `ofStack`, `toFlatList`, `toHashMap`, `toHashSet`,
  `toImmutableList`, `toQueue`, `toSortedMap`, `toSortedSet` and `toStack`.
* The `net8.0` target, next to `netstandard2.0`.
* Source Link and a symbol package.

### Changed

* `HashMap.filter` returns a sequence of key-value pairs, where it returned a `HashMap`, and takes any such sequence.
* `ImmutableList.ofList` takes an F# list, where it took a sequence.
* `ImmutableList.toArray` takes an `ImmutableList`, where it took an `IImmutableList`.
* `FlatList.removeAll` and `FlatList.removeAllWith` take the items to remove as a sequence, where they took a `FlatList`.
* `HashMap.append`, `ImmutableList.concat` and `ImmutableList.ofSeq` take any type that implements the sequence type
  they took, so their compiled methods have one more type parameter.
* The compiled methods that 1.0.0 had for these eight functions are gone, so code compiled against 1.0.0 that calls them
  has to be compiled again. F# code compiled against 1.0.0 contains the bodies of the inline `HashMap.append`,
  `HashMap.filter` and `ImmutableList.ofSeq`, so for it only the other five functions matter.
* Some type parameters of the `HashMap` and `SortedMap` functions are annotated `notnull`, which matters only to callers
  that check nullness.

### Removed

* The `netstandard1.6` target: FSharp.Core 4.6.2, the last release for it, has no `nameof`.

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

[Unreleased]: https://github.com/fsprojects/FSharp.Collections.Immutable/compare/releases/1.5.0...HEAD
[1.5.0]: https://github.com/fsprojects/FSharp.Collections.Immutable/compare/releases/1.0.0...releases/1.5.0
[1.0.0]: https://github.com/fsprojects/FSharp.Collections.Immutable/releases/tag/releases/1.0.0
