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
* `FlatList.Parallel` has every function of the FSharp.Core `Array.Parallel` module except the in-place sorts, with its
  semantics and exceptions. Its `tryFind`, `tryFindIndex` and `tryPick` return `voption`, `choose` and `tryPick` take
  functions that return `voption`, and `groupBy`, `partition`, `partitionWith` and `zip` return struct tuples; the
  `Parallel` modules of the `Option` and `Tuple` namespaces repeat them with options and reference tuples.
* `IndexedList` has every function of the FSharp.Core `List` module that applies to an immutable list, with its semantics,
  parameter names and exceptions and with stable sorts, plus `binarySearch`, `binarySearchRange`,
  `binarySearchRangeWithComparer`, `binarySearchWithComparer`, `blit`, `indexOf`, `indexOfRange`, `indexOfRangeWith`,
  `indexOfWith`, `lastIndexOf`, `lastIndexOfRange`, `lastIndexOfRangeWith`, `lastIndexOfWith`, `ofReadOnlySpan`, `ofSpan`,
  `removeMany`, `removeManyWith`, `revRange`, `skipLast`, `sortRange`, `sortRangeWith`, `sortRangeWithComparer`,
  `sortWithComparer`, `sub`, `takeLast`, `tryFindIndexBackRange`, `tryFindIndexRange`, `tryUncons`, `uncons` and the struct
  partial active patterns `Cons` and `Nil`.
* The functions of `IndexedList` that wrap a member of `ImmutableList<'T>` throw what the member throws, with the names of
  their own parameters. `removeMany` removes one element for each value, in `EqualityComparer<'T>.Default`. The functions
  that take a range check it before they call the member: the members accept a range whose end overflows `Int32.MaxValue`,
  `BinarySearch` accepts a range beyond the end, and `LastIndexOf` and `FindLastIndex` accept a last element at or after
  the end and answer with the index of another element.
  A function that takes a sequence throws `ArgumentNullException` that names the parameter for a `null` sequence and for a
  default `FlatList` given as a sequence, which the members would reject with `InvalidOperationException` while they
  enumerate it; the `IIndexedList` and `IndexedListBuilder` modules and the `indexedList { }` expression do the same.
* `sortRange`, `sortRangeWith` and `sortRangeWithComparer` are stable, as the other sorts of `IndexedList` are, and do not
  wrap the unstable `ImmutableList<'T>.Sort(index, count, comparer)`.
* The `IIndexedList` module has the functions of `IndexedList` for `IImmutableList<'T>`, without the creation and builder
  functions, `binarySearch*`, `blit`, `revRange`, `sortRange*`, `sub` and `tryFindIndex*Range`, plus `clear`. Its functions
  keep the implementation of the input list where the result has the element type of the input, and return an
  `IndexedList` otherwise. `IIndexedList<'T>` is the abbreviation of `IImmutableList<'T>`, as `IndexedList<'T>` is the
  abbreviation of `ImmutableList<'T>`.
* `max`, `maxBy`, `min` and `minBy` of `IndexedList` and `IIndexedList` are inline, as the ones of `List` are, so that the
  comparison is compiled for the type of the elements and boxes nothing. `exists2`, `forall2` and `compareWith` stop at the
  first pair that decides. `concat` and `collect` share the nodes of an indexed list of at least 16 elements instead of
  copying its elements, and `skip`, `take`, `sub`, `splitAt`, `chunkBySize`, `windowed` and the range sorts read a range in
  O(log n + count).
* The `FSharp.Collections.Immutable.Option` and `FSharp.Collections.Immutable.Tuple` namespaces have `IndexedList` and
  `IIndexedList` modules that repeat the functions that use value options or struct tuples with options and reference
  tuples.
* The `IndexedListBuilder` module works on the builder of an indexed list, and `indexedList { }` builds an indexed list.
  The module removes and replaces values (`remove`, `removeWith`, `removeAll`, `removeMany`, `removeManyAt`,
  `removeManyWith`, `replace`, `replaceWith`), inserts values (`insertManyAt`), reverses, sorts and searches ranges
  (`revRange`, `sortRange`, `sortRangeWith`, `sortRangeWithComparer`, `indexOfRange`, `indexOfRangeWith`,
  `lastIndexOfRange`, `lastIndexOfRangeWith`, `binarySearch`, `binarySearchWithComparer`,
  `binarySearchRangeWithComparer`) and copies elements into an array (`blit`). `replace` throws `ArgumentException` for a
  value that no element equals, as the builder of an indexed list does.

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
* The `ImmutableList` module is `IndexedList`. It works on `ImmutableList<'T>` (`IndexedList<'T>`), where most functions of
  1.0.0 took any `IImmutableList<'T>` and the ones that change a list returned an `IImmutableList<'T>`; the functions for
  `IImmutableList<'T>` are in the new `IIndexedList` module. 1.0.0 compiled the module as `ImmutableListModule`, with the
  F# names of its functions, such as `isEmpty`, and `IndexedList` compiles them to PascalCase names, such as `IsEmpty`.
* `IndexedList.ofList` takes an F# list, where `ofList` took any sequence. The mapping of `collect` returns an indexed
  list and `concat` takes indexed lists, where they took any sequences. `append` takes two indexed lists, where it took a
  list and a sequence: `append list (ofSeq items)` or `insertManyAt (length list) items list` appends a sequence.
* `IndexedList`: `tryFind`, `tryFindIndex`, `tryHead`, `tryItem`, `tryLast` and `tryTail` return `voption`; `choose` and
  `pick` take choosers that return `voption`; `splitAt` returns a struct tuple; and the generator of `unfold` returns a
  `voption` of a struct tuple with the element before the state, as in `List.unfold`, where it returned an `option` of a
  tuple with the state before the element.
* `IndexedList`: every function throws what the FSharp.Core `List` function of the same name throws, and names the
  parameters of `List`, such as `source` in `ofSeq`, `itemsToExclude` in `except` and `list1` in `append`. `head`, `last`
  and `tail` of an empty list, and `item` and `removeAt` for an index outside the list, throw `ArgumentException`, where
  they threw `ArgumentOutOfRangeException`. `take` and `splitAt` throw `InvalidOperationException` for a count beyond the
  length and `ArgumentException` for a negative count, and `skip` and `truncate` return the list and an empty list for a
  negative count, where all four threw `ArgumentOutOfRangeException`. `forall2` throws `ArgumentException` for lists of
  different lengths when every pair of the shorter list satisfies the predicate, where it returned `true`. `contains` and
  `ofBuilder` throw `ArgumentNullException` for a `null` input, where they threw `NullReferenceException`. A default
  `FlatList` given as a sequence to `ofSeq`, `concat` and `except` throws `ArgumentNullException`, where it threw
  `InvalidOperationException` (`except` of an empty list returned).
* `IndexedList.skipWhile` and `takeWhile` do what their names say, as the functions of `List` do. In 1.0.0, `skipWhile`
  returned the elements before the first one that fails the predicate, and `takeWhile` returned the elements from it on.
* `IndexedList.except` removes every element equal to an excluded item and the duplicates of the other elements, as
  `List.except` does, where the 1.0.0 `except` removed one element for each item, as `removeMany` does now.
* `IndexedList.contains` compares in F# structural equality, where 1.0.0 compared in `EqualityComparer<'T>.Default`:
  arrays with equal elements are equal, and `nan` is not equal to itself. `remove` and `replace`, and `indexOf`,
  `lastIndexOf`, `indexOfRange` and `lastIndexOfRange`, which replace `index`, `lastIndex`, `indexRange` and
  `lastIndexRange`, compare in `EqualityComparer<'T>.Default`, where 1.0.0 compared in F# structural equality
  (`HashIdentity.Structural`), so elements that are arrays, or whose type defines `Equals` differently from F# equality,
  compare differently.
* The functions that replace renamed functions of 1.0.0 differ from them in these cases. `insertAt`, `insertManyAt` and
  `updateAt` throw `ArgumentException` for an index outside the list, where `insert`, `insertRange` and `withItem` threw
  `ArgumentOutOfRangeException`. `removeManyAt` throws `ArgumentException` for a range that is not inside the list and
  removes nothing for a negative count or a range whose end overflows `Int32.MaxValue`, where `removeRange` threw
  `ArgumentOutOfRangeException`. `lastIndexOf` returns -1 for an empty list, where `lastIndex` threw
  `ArgumentOutOfRangeException`. `lastIndexOfRange` throws `ArgumentOutOfRangeException` for a start index at or after the
  end of the list, where `lastIndexRange` accepted it and could answer with an index that the list does not have. `rev`
  and `revRange` throw `ArgumentNullException` for a `null` list, where `reverse` and `reverseRange` threw
  `NullReferenceException`.
* `IndexedList.rev` builds the reversed list from a reversed copy of the elements. At 100 000 integers it takes about 3 ms
  where `reverse`, which called `ImmutableList.Reverse`, took about 23 ms, and it allocates 5.2 MB where `reverse`
  allocated 4.8 MB.

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
* The functions of the `ImmutableList` module that `IndexedList` has under another name and with the same arguments:
  `lastIndex` to `lastIndexOf`, `lastIndexWith` to `lastIndexOfWith`, `indexRange` to `indexOfRange`, `indexRangeWith` to
  `indexOfRangeWith`, `lastIndexRange` to `lastIndexOfRange`, `lastIndexRangeWith` to `lastIndexOfRangeWith`, `insert` to
  `insertAt`, `insertRange` to `insertManyAt`, `removeRange` to `removeManyAt`, `exceptWith` to `removeManyWith`, `reverse`
  to `rev`, `reverseRange` to `revRange` and `withItem` to `updateAt`. Only `revRange index count list` differs: it takes
  the index and the count as two arguments, where `reverseRange` took them as a tuple.
* `ImmutableList.index` and `indexWith`: use `IndexedList.indexOf` and `indexOfWith`. `ImmutableList.indexFrom`,
  `indexFromWith`, `lastIndexFrom` and `lastIndexFromWith`: use `IndexedList.indexOfRange`, `indexOfRangeWith`,
  `lastIndexOfRange` and `lastIndexOfRangeWith`. `indexFrom index item list` is
  `indexOfRange index (length list - index) item list`, and `lastIndexFrom index item list` is
  `lastIndexOfRange index (index + 1) item list`. `index`, `indexWith`, `indexFrom` and `indexFromWith` returned a function
  that took the list a second time, as in `index item list list`; the functions that replace them return the index.
* `ImmutableList.clear`: use `IndexedList.empty`, or `IIndexedList.clear` to keep the implementation of an interface value.
* `ImmutableList.skipUntil` and `takeUntil`: use `IndexedList.skipWhile` and `takeWhile` with the negated predicate. They
  returned each other's result in 1.0.0: `skipUntil p list` returned what `takeWhile (not << p) list` returns, and
  `takeUntil p list` returned what `skipWhile (not << p) list` returns.
* `ImmutableList.filterFold`: it has no replacement.
* The `IndexedSeq` module and the `IIndexedSeq<'T>` abbreviation of `IReadOnlyList<'T>`: use `IReadOnlyList<'T>`, whose
  `Count` and indexer replace `IndexedSeq.length` and `IndexedSeq.item`. `IndexedSeq.check` needs no replacement, because
  every function of the list modules throws `ArgumentNullException` for a `null` list.

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
