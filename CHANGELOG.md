# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

* `FlatList` has every function of the FSharp.Core `Array` module that applies to an immutable array, with its semantics
  and exceptions, plus `add`, `asMemory`, `asSpan`, `binarySearch`, `binarySearchRange`, `binarySearchRangeWithComparer`,
  `binarySearchWithComparer`, `indexOf`, `indexOfRange`, `indexOfRangeWith`, `indexOfWith`, `lastIndexOf`,
  `lastIndexOfRange`, `lastIndexOfRangeWith`, `lastIndexOfWith`, `ofReadOnlySpan`, `ofSpan`, `remove`, `removeMany`,
  `removeManyWith`, `removeWith`, `replace`, `replaceWith`, `skipLast` and `takeLast`. `remove` and `removeMany` remove by
  value in `EqualityComparer<'T>.Default`, and the `With` variants take an equality comparer. The functions that wrap a
  member of `ImmutableArray<'T>` throw what the member throws.
* The `FSharp.Collections.Immutable.Option.FlatList` and `FSharp.Collections.Immutable.Tuple.FlatList` modules repeat the
  functions that use value options or struct tuples with options and reference tuples.
* The `FlatListBuilder` module works on the builder of a flat list: `add`, `addRange`, `blit`, `capacity`, `clear`,
  `contains`, `drainToFlatList`, `indexOf`, `indexOfRange`, `indexOfRangeWith`, `insertAt`, `insertManyAt`, `item`,
  `lastIndexOf`, `lastIndexOfRange`, `lastIndexOfRangeWith`, `length`, `remove`, `removeAll`, `removeAt`, `removeMany`,
  `removeManyAt`, `removeManyWith`, `removeWith`, `replace`, `replaceWith`, `rev`, `setCapacity`, `sort`, `sortRange`,
  `sortRangeWith`, `sortRangeWithComparer`, `sortWith`, `sortWithComparer`, `toArray` and `updateAt`. The functions that
  change the builder return it. `replace` and `replaceWith` leave the builder as it is when no element equals the old
  value, as `ImmutableArray<'T>.Builder.Replace` does, where `FlatList.replace` throws `ArgumentException`.
* `flatList { }` builds a flat list.

### Changed

* The package targets `net8.0` and `net10.0`.
* The package requires FSharp.Core 10.1.301 or later, the first release with `partitionWith`.
* `FlatList`: the `try*` functions return `voption`; `choose`, `pick` and `tryPick` take functions that return `voption`;
  the functions that return tuples return struct tuples.
* `FlatList`: every function throws what the FSharp.Core `Array` function of the same name throws. `take` and `splitAt`
  throw `InvalidOperationException` for a count beyond the length, where they threw `ArgumentOutOfRangeException`. A
  default `ImmutableArray` throws `ArgumentNullException`, where it threw `ArgumentException`, and also when it is given
  as a sequence, where it threw `InvalidOperationException`.
* `FlatList.sub` has the meaning of `Array.sub`: `sub array startIndex count` returns `count` elements from `startIndex`.
  It was `sub start stop list`.
* The nested `FlatList.Builder` module is now the `FlatListBuilder` module.

### Removed

* `FlatList.index`, `indexWith`, `lastIndex` and `lastIndexWith`: use `indexOf`, `indexOfWith`, `lastIndexOf` and
  `lastIndexOfWith`. They compare in `EqualityComparer<'T>.Default`, where the removed functions compared in F# structural
  equality: elements that are arrays, or types whose `Equals` differs from F# equality, compare differently.
* `FlatList.indexRange`, `indexRangeWith`, `lastIndexRange` and `lastIndexRangeWith`: use `indexOfRange`,
  `indexOfRangeWith`, `lastIndexOfRange` and `lastIndexOfRangeWith`, which take the same arguments and compare as
  `indexOf` does.
* `FlatList.indexFrom`, `indexFromWith`, `lastIndexFrom` and `lastIndexFromWith`: use `indexOfRange` and
  `indexOfRangeWith` with the count `length - startIndex`, and `lastIndexOfRange` and `lastIndexOfRangeWith` with the
  count `startIndex + 1`.
* `FlatList.removeAll` and `removeAllWith`: use `removeMany` and `removeManyWith`. They remove one element for each of the
  given values, as the removed functions did, and `removeMany` compares in `EqualityComparer<'T>.Default` where `removeAll`
  compared in F# structural equality. The removed functions did not remove the elements that satisfy a predicate: use
  `filter` with the negated predicate for that.
* `FlatList.removeRange`: use `removeManyAt`.

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
