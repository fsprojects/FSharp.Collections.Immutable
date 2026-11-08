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
* `HashMap` has every function of the FSharp.Core `Map` module that applies to a hash map, with its semantics and
  exceptions, except `add`, which stays the `Add` of `ImmutableDictionary`, as in 1.0.0; plus `choose` and `singleton`,
  the builder functions `build` and `update`, and the wrappers of the `ImmutableDictionary` members `addRange`,
  `containsBinding`, `containsValue`, `ofReadOnlySpan`, `ofSeqBy`, `removeRange`, `setItem`, `setItems`, `tryGetKey` and
  `valueComparer`, which keep the semantics of the BCL.
* The transformations of `HashMap` keep the key comparer of the map, and the value comparer when the type of the values
  does not change.
* `HashMap.containsBinding` and `IMap.containsBinding` tell whether a key is bound to a value, comparing the value with the
  value comparer of the map, as `Contains` of a `KeyValuePair` does. `tryFind` followed by `=` compares with F# equality,
  which can differ from the value comparer, and an `IMap` does not expose its value comparer.
* `HashMap.ofReadOnlySpan` and `HashMap.ofSpan` create a hash map from a `ReadOnlySpan` or a `Span` of
  `KeyValuePair<'Key, 'Value>` with `ImmutableDictionary.CreateRangeWithOverwrite`, which takes a read-only span only, and
  the members `ofReadOnlySpan` and `ofSpan` of `CustomComparersHashMap` do so with its comparers. A repeated key never
  throws: the last binding wins, unless the value comparer finds the two values equal. F# converts a `Span` to the
  parameter of `ofReadOnlySpan` only when it has inferred the types of the call, which is why `ofSpan` exists; an array
  converts to either parameter under the same condition.
* `HashMap.build` creates a hash map with a builder that an action fills, `HashMap.update` changes a hash map with a
  builder, and the member `build` of `CustomComparersHashMap` does the first with its comparers.
* `HashMap.ofArray` and `HashMap.ofList` are new, and `HashMap.ofSeq`, which took `KeyValuePair`s, takes tuples too. The
  three take the bindings as `KeyValuePair`s, reference tuples or struct tuples: they are static members of
  `ImmutableDictionary<'Key, 'Value>` in the auto-opened `HashMapExtensions` module, one overload per shape, and every
  shape keeps the last binding of a repeated key, as `Map.ofSeq` does, unless the value comparer finds the two values
  equal. They take a dictionary, an F# `Map` or another hash map too. C# does not see them as members of
  `ImmutableDictionary`; it calls the public static methods `HashMapExtensions.OfArray`, `OfList` and `OfSeq`, or the
  members of the struct below. A reflection over the `HashMap` module does not list them. To reject another value of a
  repeated key, as `ImmutableDictionary.CreateRange` does, add the bindings to an empty map:
  `HashMap.empty |> HashMap.addRange pairs`.
* `HashMap.forComparers` and `HashMap.forKeyComparer` return a `CustomComparersHashMap<'Key, 'Value>` struct whose members
  `build`, `builder`, `empty`, `ofArray`, `ofList`, `ofReadOnlySpan`, `ofSeq`, `ofSeqBy`, `ofSpan` and `singleton` create
  hash maps with its comparers: `(HashMap.forKeyComparer StringComparer.OrdinalIgnoreCase).ofSeq pairs`. A `null`
  comparer throws `ArgumentNullException`.
* `ofSeq` of that struct also gives an existing hash map other comparers, as `WithComparers` does, with three differences.
  It takes O(1) only when the key comparer of the map is the instance of the struct, and rebuilds the map otherwise.
  Keys that the new comparer makes equal merge, the later binding winning, where `WithComparers` throws. And
  `forKeyComparer` resets the value comparer, where `WithComparers` keeps it; to keep it, pass the value comparer of the
  map: `HashMap.forComparers keyComparer (HashMap.valueComparer table)`.
* `HashMap.addRange` and `HashMap.setItems` take `KeyValuePair`s; the nested modules `HashMap.Tuple` and
  `HashMap.ValueTuple` take reference and struct tuples: `HashMap.Tuple.addRange pairs table`.
* `HashMap` and `IMap` keep the convention of `ImmutableDictionary` where it differs from `Map`: a new value of a key that
  the value comparer finds equal to the current one leaves the binding as it is, its key included, in `add`, `change`,
  `setItem`, `setItems`, `addRange`, the creation functions and members, and `hashMap { }`. `add` is the `Add` of
  `ImmutableDictionary`, as it was in 1.0.0: it throws `ArgumentException` for a key that is bound to a value that the
  value comparer finds different, and returns the input itself for the same binding; `setItem` replaces the value.
* `change` with a function that keeps an existing value adds a binding only when the key has none, as
  `ImmutableInterlocked.TryAdd` does. The other single-key idioms of the BCL need no new function: `tryFind` is
  `TryGetValue`, and `tryFind` followed by `ValueOption.defaultValue` is `GetValueOrDefault`. `ToImmutableDictionary` with
  an element selector has no function; to reject a repeated key like the BCL does, add the mapped pairs to an empty map,
  `HashMap.empty |> HashMap.addRange (source |> Seq.map (fun x -> KeyValuePair (key x, element x)))`, and `HashMap.ofSeq`
  of the mapped pairs lets the last binding win. Maps have no builder module: call `Add`, `AddRange`, `Remove`,
  `RemoveRange`, the indexer and the other members on the builder.
* The `IMap` module works on any `IImmutableDictionary<'Key, 'Value>` with the same functions, except the creation, builder
  and comparer functions and `containsValue`, which the interface does not declare, and has the nested modules `Tuple` and
  `ValueTuple` too. Its `add` is the `Add` of the interface and its `containsBinding` the `Contains` of a `KeyValuePair`.
  Its transformations keep the implementation and the comparers of the map; `map` and `choose` return a `HashMap`.
* The `FSharp.Collections.Immutable.Option` and `FSharp.Collections.Immutable.Tuple` namespaces have `HashMap` and `IMap`
  modules that repeat the functions that use value options or return struct tuples with options and reference tuples.
* `hashMap { }` builds a hash map from `yield struct (key, value)` and `yield KeyValuePair (key, value)`; `HashMapCE` takes
  a key comparer, and a `null` one throws `ArgumentNullException`.
* `SortedMap` has every function of the FSharp.Core `Map` module that applies to a sorted map, with its semantics and
  exceptions, `minKeyValue` and `maxKeyValue` included, except `add`, which is the `Add` of
  `ImmutableSortedDictionary`; plus `choose` and `singleton`, the builder functions `build`, `builder`, `ofBuilder`,
  `toBuilder` and `update`, and the wrappers of the `ImmutableSortedDictionary` members `add`, `addRange`, `clear`,
  `containsBinding`, `containsValue`, `keyComparer`, `ofSeqBy`, `removeRange`, `setItem`, `setItems`, `tryGetKey` and
  `valueComparer`, which keep the semantics of the BCL. `maxKeyValue` visits every binding, because
  `ImmutableSortedDictionary` gives no other access to its largest key.
* `SortedMap.containsBinding` tells whether a key is bound to a value, comparing the value with the value comparer of the
  map, as `Contains` of a `KeyValuePair` does. `tryFind` followed by `=` compares with F# equality, which can differ from
  the value comparer. A `null` key throws `ArgumentNullException`.
* `SortedMap.build` creates a sorted map with a builder that an action fills, `SortedMap.update` changes a sorted map with
  a builder, and the member `build` of `CustomComparersSortedMap` does the first with its comparers.
* `SortedMap.ofArray`, `SortedMap.ofList` and `SortedMap.ofSeq` take the bindings as `KeyValuePair`s, reference tuples or
  struct tuples: they are static members of `ImmutableSortedDictionary<'Key, 'Value>` in the auto-opened
  `SortedMapExtensions` module, one overload per shape, and every shape keeps the last binding of a repeated key, as
  `Map.ofSeq` does, unless the value comparer finds the two values equal. They take a dictionary, an F# `Map` or another
  map too. C# does not see them as members of `ImmutableSortedDictionary`; it calls the public static methods
  `SortedMapExtensions.OfArray`, `OfList` and `OfSeq`, or the members of the struct below. A reflection over the
  `SortedMap` module does not list them. To reject another value of a repeated key, as
  `ImmutableSortedDictionary.CreateRange` does, add the bindings to an empty map:
  `SortedMap.empty |> SortedMap.addRange pairs`.
* `SortedMap.forComparers` and `SortedMap.forKeyComparer` return a `CustomComparersSortedMap<'Key, 'Value>` struct whose
  members `build`, `builder`, `empty`, `ofArray`, `ofList`, `ofSeq`, `ofSeqBy` and `singleton` create sorted maps with its
  comparers: `(SortedMap.forKeyComparer StringComparer.OrdinalIgnoreCase).ofSeq pairs`. A `null` comparer throws
  `ArgumentNullException`.
* `ofSeq` of that struct also gives an existing sorted map other comparers, as `WithComparers` does, with three
  differences. It takes O(1) only when the key comparer of the map is the instance of the struct, and rebuilds the map
  otherwise; a sorted map that the module creates orders its keys with F# generic comparison, so pass
  `SortedMap.keyComparer table` to change only its value comparer in O(1). Keys that the new comparer makes equal merge,
  the later binding winning, where `WithComparers` throws. And `forKeyComparer` resets the value comparer, where
  `WithComparers` keeps it; to keep it, pass the value comparer of the map:
  `SortedMap.forComparers keyComparer (SortedMap.valueComparer table)`.
* `SortedMap.addRange` and `SortedMap.setItems` take `KeyValuePair`s; the nested modules `SortedMap.Tuple` and
  `SortedMap.ValueTuple` take reference and struct tuples: `SortedMap.Tuple.addRange pairs table`.
* The single-key idioms of the BCL are functions of the module: `tryFind` is `TryGetValue`, `tryFind` followed by
  `ValueOption.defaultValue` is `GetValueOrDefault`, and `change` with a function that keeps an existing value adds a
  binding only when the key has none. `ImmutableInterlocked` has no member for a sorted dictionary, but its `Update`
  replaces a field atomically with the result of a function of the sorted map, such as `SortedMap.change`.
  `ToImmutableSortedDictionary` with an element selector has no function; to reject a repeated key like the BCL does, add
  the mapped pairs to an empty map,
  `SortedMap.empty |> SortedMap.addRange (source |> Seq.map (fun x -> KeyValuePair (key x, element x)))`, and
  `SortedMap.ofSeq` of the mapped pairs lets the last binding win. Maps have no builder module: call `Add`, `AddRange`,
  `Remove`, `RemoveRange`, the indexer and the other members on the builder.
* The functions of `SortedMap` that create a sorted map without a key comparer (`singleton`, `ofSeqBy`, `build`, `builder`,
  `ofArray`, `ofList`, `ofSeq` and `sortedMap { }`) order its keys with F# generic comparison, as `Map` does, so the keys
  need `comparison`, and a sorted map of strings enumerates them in ordinal order, where `Comparer<'Key>.Default` orders
  them by the rules of the current culture.
* The transformations of `SortedMap` keep the key comparer of the map, and the value comparer when the type of the values
  does not change; `map` and `choose` compare the new values with the default comparer.
* `tryFind`, `tryFindKey`, `tryGetKey` and `tryPick` of `SortedMap` return a `voption`, `tryFindKey` returns the key,
  `pick` and `tryPick` take choosers that return a `voption`, and `partition`, `toArray`, `toList`, `toSeq`,
  `minKeyValue` and `maxKeyValue` return struct tuples. `find`, `findKey` and `pick` throw the `KeyNotFoundException` of
  `Map`, and every function throws `ArgumentNullException` for a `null` map, sequence or builder, and for a default
  `FlatList` given as a sequence, which would fail only when it is enumerated.
* `SortedMap` keeps the convention of `ImmutableSortedDictionary` where it differs from `Map`: a new value of a key that
  the value comparer finds equal to the current one leaves the binding as it is, its key included, in `add`, `change`,
  `setItem`, `setItems`, `addRange`, the creation functions and members, and `sortedMap { }`. `add` is the `Add` of
  `ImmutableSortedDictionary`: it throws `ArgumentException` for a key that is bound to a value that the value comparer
  finds different, and returns the input itself for the same binding; `setItem` replaces the value.
* `SortedMap.toSeq` and `SortedMap.ofSeq` replace `Seq.ofSortedMap` and `Seq.toSortedMap` of the former `Seq` module of
  conversions between the collections. `toSeq` yields struct tuples instead of `KeyValuePair`s, and a sorted map is
  itself a `seq<KeyValuePair<_, _>>`, so `Seq.toArray table` gives the old shape.
* The `FSharp.Collections.Immutable.Option` and `FSharp.Collections.Immutable.Tuple` namespaces have `SortedMap` modules
  that repeat the functions that use value options or return struct tuples with options and reference tuples.
* `sortedMap { }` builds a sorted map from `yield struct (key, value)` and `yield KeyValuePair (key, value)`;
  `SortedMapCE` takes a key comparer, and a `null` one throws `ArgumentNullException`, as do `yield!` of a `null`
  sequence, array or sorted map, `for` over a `null` sequence, and a default `FlatList` given as a sequence to `yield!`
  or `for`.

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
* `HashMap.tryFind` returns a `voption`, where it returned an `option`;
  `FSharp.Collections.Immutable.Option.HashMap.tryFind` returns the `option`.
* `HashMap.ofSeq` keeps the last binding of a repeated key, where it threw `ArgumentException` for a key that is bound to
  two different values.
* `HashMap.ofSeq` throws `ArgumentNullException` for a default flat list given as a sequence, where it threw
  `InvalidOperationException`.
* `HashMap.filter` returns the bindings that satisfy the predicate, where it returned all the bindings of the hash map.
* `SortedMap.empty` orders its keys with F# generic comparison, as `Map.empty` does, where it used
  `Comparer<'Key>.Default`, which orders strings by the rules of the current culture: a sorted map of strings now
  enumerates them in ordinal order. It requires `'Key : comparison`, where it accepted any key type; for keys without
  comparison, or for another order, use `(SortedMap.forKeyComparer comparer).empty`. C# callers see the compiled name
  `Empty`, where it was `empty`.

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
* `HashMap.length`, `append` and `except`: use `count`, `addRange` and `removeRange`. `addRange pairs table` takes the
  bindings before the hash map, where `append map pairs` took the hash map first.
* `HashMap.check`: no replacement is needed, because every function of the module throws `ArgumentNullException` for a
  `null` hash map.
* The `HashMapBuilder<'Key, 'Value>` type abbreviation: use `HashMap<'Key, 'Value>.Builder`.
* The `SortedMapBuilder<'Key, 'Value>` abbreviation: use `SortedMap<'Key, 'Value>.Builder`.

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
