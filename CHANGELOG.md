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
* `HashSet` has every function of the FSharp.Core `Set` module that applies to a hash set, with its semantics, argument
  order and exceptions, plus `addRange`, `build`, `builder`, `choose`, `clear`, `comparer`, `forComparer`, `ofBuilder`,
  `ofReadOnlySpan`, `ofSpan`, `overlaps`, `removeRange`, `setEquals`, `symmetricDifference`, `toBuilder`, `tryGetValue`
  and `update`.
* `HashSet.addRange elements set` and `removeRange elements set` wrap the `Union` and `Except` members that take a
  sequence. The result keeps the comparer of the hash set, an element of the sequence that the comparer finds equal to
  an element of the hash set does not replace it, and the input itself comes back when nothing changes.
* `HashSet.ofReadOnlySpan` and `ofSpan` create a hash set from a span, `build` fills a builder with an action, and
  `update` applies an action to a builder of a hash set and returns its contents, so that the in-place operations of the
  builder, such as `IntersectWith`, are one expression.
* `HashSet.forComparer comparer` creates the `CustomComparerHashSet` struct, whose members `build`, `builder`, `empty`,
  `ofArray`, `ofList`, `ofReadOnlySpan`, `ofSeq`, `ofSpan` and `singleton` create hash sets and builders that compare
  with the comparer, as in `(HashSet.forComparer StringComparer.OrdinalIgnoreCase).ofSeq names`. They replace the BCL
  members `Create(comparer, ...)`, `CreateRange(comparer, items)`, `ToImmutableHashSet(items, comparer)` and
  `CreateBuilder(comparer)`. `ofSeq` also gives an existing hash set the comparer, as `WithComparer` does: the hash set
  itself comes back for the same comparer instance, and an equal but distinct instance rebuilds it.
  `WithComparer(null)` is `HashSet.ofSeq set`. A `null` comparer throws `ArgumentNullException`.
* `HashSet.add` and `remove` return the input itself when nothing changes, so a reference comparison of the input and
  the result tells whether `Add` changed anything, as the `bool` result of `Builder.Add` does. Hash sets have no builder
  module: `builder`, `toBuilder` and `ofBuilder` are `CreateBuilder`, `ToBuilder` and `ToImmutable`, and the builder is
  used directly. The mutating members of the BCL interfaces, such as `ISet<T>.UnionWith` and `ICollection<T>.Add`,
  throw `NotSupportedException` on a hash set and have no function.
* `add`, `addRange`, `remove`, `removeRange`, `clear`, `update`, `filter` and `partition` of `HashSet` keep the comparer
  of their input, and the set algebra keeps the comparer of the first hash set; `map`, `choose` and `partitionWith`
  compare with the default comparer of the result type. Every function throws `ArgumentNullException` for a `null` hash
  set, sequence or array, and for a default flat list that is given as a sequence; `yield!` and `for` of
  `hashSet { }` do the same. `unionMany` of an empty sequence returns an empty hash set, as `Set.unionMany` does.
  `tryGetValue` returns a `voption`, `choose` takes a function that returns a `voption`, and `partition` and
  `partitionWith` return struct tuples.
* The set algebra and the relations of `HashSet` take hash sets, as the `Set` functions do. The BCL members that take a
  sequence map as follows. `Union` and `Except` are `addRange` and `removeRange`, and `Overlaps` and `IsSupersetOf` are
  `Seq.exists` and `Seq.forall` with `HashSet.contains`, which compare in the comparer of the hash set for every
  comparer. To pass a sequence to `Intersect`, `SymmetricExcept`, `IsSubsetOf`, `IsProperSubsetOf`,
  `IsProperSupersetOf` or `SetEquals`, convert it with `(HashSet.forComparer (HashSet.comparer set)).ofSeq elements`:
  the elements get the comparer of the first hash set, as they do in the members. `HashSet.ofSeq elements` converts
  them with the default comparer instead, which merges the elements that it finds equal before the function sees them.
  The answer is that of the member when the comparer of `set` finds equal every pair of elements that the default
  comparer finds equal, as the default comparer and `StringComparer.OrdinalIgnoreCase` do. It can differ when the
  comparer of `set` tells such elements apart, as `HashIdentity.Reference` does for two strings with the same text.
* The element that stays when the comparer finds two elements equal follows the BCL: `union` and `addRange` keep the
  element of the first hash set, `intersect` keeps the elements of the second one, and the creation functions keep the
  first element of their input. `isProperSuperset` is the exception: it counts the elements of the second hash set that
  the comparer of the first one finds equal once, as the documentation of `IsProperSupersetOf` says, where the member
  counts each of them.
* The `ISet` module works on any `IImmutableSet<'T>` with the functions of `HashSet` that need neither a creation nor a
  comparer, `addRange` and `removeRange` included; the transformations that keep the element type return the
  implementation of their input. The interface cannot tell its comparer, so a sequence for a relation, `intersect` or
  `symmetricDifference` is converted with `ISet.addRange elements (ISet.clear set)`, which has the implementation and
  the comparer of `set`, where `HashSet` uses `(HashSet.forComparer (HashSet.comparer set)).ofSeq elements`.
* The `FSharp.Collections.Immutable.Option.HashSet`, `FSharp.Collections.Immutable.Tuple.HashSet`,
  `FSharp.Collections.Immutable.Option.ISet` and `FSharp.Collections.Immutable.Tuple.ISet` modules repeat `choose` and
  `tryGetValue` with options, and `partition` and `partitionWith` with reference tuples.
* `hashSet { }` builds a hash set, and `HashSetCE` created with an equality comparer builds hash sets that compare with it;
  a `null` comparer throws `ArgumentNullException`.
* `FrozenSet` works on `System.Collections.Frozen.FrozenSet<'T>` with the functions of the FSharp.Core `Set` module that
  read a set or transform a whole set, with their semantics, argument order and exceptions, plus `choose`, `comparer`,
  `forComparer`, `ofReadOnlySpan`, `ofSpan`, `overlaps`, `setEquals`, `symmetricDifference`, `toFlatList`, which returns
  the elements without a copy, and `tryGetValue`. A frozen set is built once: the module has no function that adds or
  removes an element, and every transformation builds a new frozen set. `FrozenSet.union set (FrozenSet.singleton value)`
  and `FrozenSet.difference set (FrozenSet.singleton value)` do what `Set.add` and `Set.remove` do, each as an O(n)
  rebuild that compares in the comparer of `set`. Every function throws `ArgumentNullException` for a `null` frozen set,
  sequence, list or array, and for a default flat list that is given as a sequence; `yield!` and `for` of
  `frozenSet { }` do the same.
* `FrozenSet.ofReadOnlySpan` and `ofSpan` create a frozen set from a span. They wrap `FrozenSet.Create`, and of the
  elements that the comparer finds equal the first stays.
* `FrozenSet.forComparer comparer` creates the `CustomComparerFrozenSet` struct, whose members `empty`, `ofArray`,
  `ofList`, `ofReadOnlySpan`, `ofSeq`, `ofSpan` and `singleton` create frozen sets that compare with the comparer, as in
  `(FrozenSet.forComparer StringComparer.OrdinalIgnoreCase).ofSeq names`. They replace the BCL methods
  `FrozenSet.Create(comparer, ...)` and `ToFrozenSet(source, comparer)`; `ToFrozenSet` has no `optimizeForReading`
  parameter in the package that the library references, so nothing is lost. `ofSeq` also gives an existing frozen set the
  comparer, as `ToFrozenSet` does: the frozen set itself comes back when its comparer equals the comparer of the struct.
  A `null` comparer throws `ArgumentNullException`, where the BCL methods take it as the default comparer:
  `FrozenSet.ofSeq set` gives a frozen set the default comparer, and the default value of the struct has the BCL meaning.
* The members of `FrozenSet<'T>` map to the module as follows. `TryGetValue(x, out v)` is `tryGetValue x set`, which
  returns a `voption` (`Option.FrozenSet.tryGetValue` returns an `option`). `Items` is `toFlatList`, and `CopyTo` is
  `toFlatList` followed by `FlatList.blit` or `FlatList.asSpan`. `IsSubsetOf`, `IsProperSubsetOf`, `IsSupersetOf`,
  `IsProperSupersetOf`, `Overlaps` and `SetEquals` take frozen sets in the module, as the `Set` functions do. To pass a
  sequence, convert it with `(FrozenSet.forComparer (FrozenSet.comparer set)).ofSeq elements`: the elements get the
  comparer of the first frozen set, as they do in the members. `FrozenSet.ofSeq elements` converts them with the default
  comparer instead, which merges the elements that it finds equal before the relation sees them. The answer is that of
  the member when the comparer of `set` finds equal every pair of elements that the default comparer finds equal, as the
  default comparer and `StringComparer.OrdinalIgnoreCase` do. It can differ when the comparer of `set` tells such
  elements apart, as `HashIdentity.Reference` does for two strings with the same text. `Overlaps` and `IsSupersetOf`
  with a sequence are `Seq.exists` and `Seq.forall` with `FrozenSet.contains`, for every comparer. The mutating members
  of the interfaces, such as `ISet<T>.UnionWith` and `ICollection<T>.Add`, throw `NotSupportedException` on the BCL type
  and have no function.
* `FrozenSet`: the element that stays when the comparer finds two elements equal follows the BCL. `union` keeps the
  element of the first frozen set, `intersect` keeps the elements of the second one, as `ImmutableHashSet.Intersect` and
  `ImmutableSortedSet.Intersect` do, because `FrozenSet<'T>` has no such member, and the creation functions keep the
  first element of their input. `isProperSuperset` counts the elements of the second frozen set that the comparer of the
  first one finds equal once, as `FrozenSet<'T>.IsProperSupersetOf` does.
* On `net10.0`, `FrozenSet.alternateLookup` and `FrozenSet.tryAlternateLookup` return the alternate lookup of a frozen set
  whose comparer compares the elements with values of another type. They are `GetAlternateLookup` and
  `TryGetAlternateLookup`. The alternate type comes first, as in `FrozenSet.alternateLookup<int, _> set`, and a lookup by a
  `ReadOnlySpan<char>` calls the BCL method directly, because an F# generic function cannot be instantiated with a
  byref-like type.
* The `FSharp.Collections.Immutable.Option.FrozenSet` and `FSharp.Collections.Immutable.Tuple.FrozenSet` modules repeat
  `choose`, `tryGetValue` and `tryAlternateLookup` with options, and `partition` and `partitionWith` with reference tuples.
* `frozenSet { }` builds a frozen set, and `FrozenSetCE` created with an equality comparer builds frozen sets that compare
  with it; a `null` comparer throws `ArgumentNullException`.

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
* The `ISet<'T>` abbreviation of `IImmutableSet<'T>`, because it shadowed `System.Collections.Generic.ISet<'T>`: use
  `IImmutableSet<'T>` and the `ISet` module.

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
