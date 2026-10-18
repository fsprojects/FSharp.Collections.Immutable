---
title: ADR 0001 – Per-collection modules
category: Explanations
categoryindex: 3
index: 2
---

# ADR 0001: Per-collection modules, Frozen collections and value-type defaults

| | |
|---|---|
| Status | Proposed |
| Date | 2026-10-11 |
| Deciders | maintainers |

## Context

`FSharp.Collections.Immutable` wraps `System.Collections.Immutable` with F#-style modules (`FlatList` = `ImmutableArray`,
`IndexedList` = `ImmutableList`, `Queue`, `Stack`, `HashMap`, `SortedMap`, `HashSet`, `SortedSet`). The published code still
has the 2020 single-file layout, placeholder tests and real defects in the map and set modules: `SortedMap.singleton` builds an
empty map (its argument binds to the comparer overload), `builderWith` ignores its argument, map `filter` returns a sequence
while `where` returns a map, transformations drop custom comparers, `check` helpers are public.

An unmerged branch started the target shape – one folder per collection, implementation `.fs` plus signature `.fsi`,
interface modules, builders, computation expressions, `voption` results, struct tuples – but:

* the order of functions differs in every module (for example `fold` is the 136th function in `FlatList.fsi`, the 56th in
  `IndexedList.fsi` and the 40th in `Queue.fsi`; `item` and `tryItem` are 74 positions apart in `FlatList.fsi`);
* about sixty functions have no FSharp.Core counterpart (`iter3`, `foldi2`, `findDup`, twelve `index*` overloads, …) while
  newer FSharp.Core functions are missing (`partitionWith`, the `random*` family, `removeManyAt`);
* `Stack` transformations flip the element order inconsistently (`rev` is a no-op), `init` runs the initializer through
  `Parallel.For`, `splitInto` uses a different chunking than FSharp.Core, `take`/`skip` clamp instead of throwing;
* the test project does not compile on the current MSTest 4 setup (155 `[<ExpectedException>]` usages), none of its
  2 744 assertions carries a failure message, and several tests lock the defects in;
* `System.Collections.Frozen` (`FrozenDictionary`, `FrozenSet`), shipped in the same NuGet package since 8.0, is not covered.

FSharp.Core itself (checked against the compiler sources, FSharp.Core 10.0.300 API) offers `List` 118, `Array` 131,
`Seq` 118, `Map` 31 and `Set` 33 public functions, none of them returning `voption` or struct tuples, and its signature
files follow no strict ordering rule.

## Decision

### D1. Scope: one module per collection with all relevant functions

The vocabulary of every module is the union of the FSharp.Core functions that apply to the collection, wrappers of the
BCL members of the underlying type, and a small curated set of extras. The complete assignment is the matrix in
Appendix A (sequence-like collections), B (maps) and C (sets).

New collections: `FrozenMap` (`FrozenDictionary<'Key, 'Value>`) and `FrozenSet` (`FrozenSet<'T>`). Their modules offer
creation, lookup, iteration, folds, conversions **and** whole-collection transformations (`map`, `filter`, `choose`,
`partition`, set algebra), each documented as an O(n) rebuild. They offer no incremental mutation (`add`, `remove`,
`setItem`, `change`, `clear`, `addRange`, `removeRange`): a frozen collection is built once and read many times.
ADR 0002 – EquatableArray (`ADR_0002_EquatableArray.md`) adds `EquatableArray<'T>`, an immutable array with value
semantics, whose module mirrors `FlatList`.

Curated extras kept beyond FSharp.Core: `indexOf`/`indexOfWith`/`lastIndexOf`/`lastIndexOfWith`, `binarySearch`,
`sortWithComparer`, `isDefault`/`isDefaultOrEmpty`, `asSpan`/`asMemory`/`ofSpan`/`ofReadOnlySpan`, the builder family
(`builder`, `builderWith`, `ofBuilder`, `toBuilder`, `moveFromBuilder`, `build`, `update`), `blit`, `add`,
`cons`/`uncons`/`tryUncons`, the struct partial patterns `(|Cons|_|)`/`(|Nil|_|)`, `tryTail`, `enqueue`/`enqueueMany`,
`consMany`, `takeLast`/`skipLast`, `clear` on interface modules, `tryGetKey`/`tryGetValue`,
`setItem`/`setItems`/`addRange`/`removeRange`, `containsValue`, `symmetricDifference`, `overlaps`/`setEquals`, comparer
accessors, the `CustomComparers*`/`CustomComparer*` structs that create collections with comparers and the
`forComparers`/`forKeyComparer`/`forComparer` functions that create the structs (D12), and the map functions for every
binding shape (D13).

Removed from the started branch: `iter3`/`iteri3`/`exists3`/`forall3`/`mapi3`/`foldBack3`, `foldi`/`foldi2`/`foldBacki`/
`foldBacki2`, `findDup`/`findDupBy`, `pickBack`/`tryPickBack`/`chooseBack`, the `From`/`Range` variants of `index*`/
`lastIndex*`, `removeAll*`, `sortRange*`, `fill`, `zeroCreate`, `copy`, `ofOption`/`toOption`/`ofValueOption`/
`toValueOption`, `tryHeadAndTail`/`tryLastAndInit`, `enqueueArray`/`enqueueList`/`consArray`/`consList`, `unconsMany`,
and the modules `Seq` (cross conversions) and `IndexedSeq` (`IReadOnlyList`).

### D2. Layout

One folder per collection; every implementation file with public API has a signature file that carries the XML
documentation, except the types file of a folder (below).

**Root modules.** A source file that holds exactly one module declares it as the root module of the file, instead of a
namespace with one nested module: `[<RequireQualifiedAccess>] module FSharp.Collections.Immutable.FlatList`,
`module FSharp.Collections.Immutable.Option.FlatList`, `module FSharp.Collections.Immutable.FlatListBuilder`,
`[<AutoOpen>] module FSharp.Collections.Immutable.HashMapExtensions`. The rule applies to the library and the tests (the ops
modules, `ExceptionAssert`, `Inputs`). A nested module such as `FlatList.Parallel` (D11) or the `Tuple`/`ValueTuple`
submodules of the maps (D13) stays inside its root module. A file with several modules (`Helper.fs`) or with types only
(the type abbreviations, the structs of D12, the computation expression classes, the test classes) declares the
namespace. Modules compile under their F# names
(`FSharp.Collections.Immutable.FlatList`, `FSharp.Collections.Immutable.Option.FlatList`): F# ignores `[<CompiledName>]` on
a module, so no module carries it.

A root module cannot declare a type of its namespace: `type HashMap<…>` inside `module FSharp.Collections.Immutable.HashMap`
would be `HashMap.HashMap<…>`. The type abbreviations of a folder therefore have their own file, `XTypes.fs`, without a
signature file. A signature file alone does not compile: F# needs an implementation file for every signature file
(FS0240). A file that holds only type abbreviations is its own signature, so it carries their XML documentation itself.

Per folder, in compile order: `XTypes.fs`, `X.fsi`, `X.fs`, `XOption.fsi`, `XOption.fs`, `XTuple.fsi`, `XTuple.fs`,
`CustomComparersX.fsi`/`.fs` (maps) or `CustomComparerX.fsi`/`.fs` (sets, D12), `XExtensions.fsi`, `XExtensions.fs`
(maps, D13), `IX.fsi`, `IX.fs` (plus their `Option`/`Tuple` files), `XBuilder.fs` (`FlatList` and `IndexedList` only),
`XCE.fs`.
`Helper.fs` comes first and `ComputationExpressions.fs` last. Interface modules compile after the concrete type they return.

Names: `IMap` module and `IMap<'Key, 'Value>` abbreviation for `IImmutableDictionary`; `ISet` module **without** an
`ISet<'T>` abbreviation, because it would shadow `System.Collections.Generic.ISet<'T>`, which `FrozenSet<'T>` implements.
Maps and sets measure their size with `count` (FSharp.Core `Map.count`/`Set.count`), sequences with `length`. `Queue` keeps
`enqueue`/`head`/`tail`/`uncons` (compiled `Enqueue`/`Peek`/`Dequeue`/`PeekDequeue`), `Stack` keeps `cons`/`head`/`tail`/
`uncons` (`Push`/`Peek`/`Pop`/`PeekPop`). Their other functions of that kind use the same .NET vocabulary in compiled
code: `tryHead`/`tryTail`/`tryUncons` compile as `TryPeek`/`TryDequeue`/`TryPeekDequeue` for `Queue` and as
`TryPeek`/`TryPop`/`TryPeekPop` for `Stack`, `enqueueMany` as `EnqueueMany` and `consMany` as `PushMany`; the interface
modules compile the same way. Computation expression instances: `flatList`, `indexedList`, `queue`, `stack`,
`hashMap`, `sortedMap`, `hashSet`, `sortedSet`, `frozenMap`, `frozenSet`, and `equatableArray` of ADR 0002. The structs of
D12 are `CustomComparersHashMap`, `CustomComparersSortedMap`, `CustomComparersFrozenMap`, `CustomComparerHashSet`,
`CustomComparerSortedSet` and `CustomComparerFrozenSet`: the prefix names what the struct holds, the comparers of a map or
the one comparer of a set. The extension modules of D13 are `HashMapExtensions`, `SortedMapExtensions` and
`FrozenMapExtensions`.

### D3. Value-type defaults and the `Option`/`Tuple` override namespaces

* Every `try*` function returns `voption`; `choose`, `pick`, `tryPick`, `unfold` and `change` take or return `voption`.
* Every tuple in a signature is a struct tuple: `zip`, `zip3`, `unzip`, `unzip3`, `pairwise`, `indexed`, `allPairs`,
  `groupBy`, `countBy`, `partition`, `partitionWith`, `splitAt`, `mapFold`, `mapFoldBack`, `unfold`, `uncons`, map
  `toSeq`/`minKeyValue`/`maxKeyValue`, and so on. The map functions that take bindings take them in every shape instead
  (D13): creation through overloads, `addRange` and `setItems` as `KeyValuePair`s with `Tuple`/`ValueTuple` submodules.
  They have no counterpart in the `Tuple` namespace.
* The namespace `FSharp.Collections.Immutable.Option` contains, per collection, a module with the same name holding every
  function whose signature mentions `voption`, converted to the FSharp.Core representation: `voption` becomes `option`, and
  a struct tuple inside an option payload becomes a reference tuple (`unfold : ('State -> ('T * 'State) option) -> …`).
* The namespace `FSharp.Collections.Immutable.Tuple` holds every function whose signature mentions a struct tuple, with
  reference tuples and `voption` kept. A function with a struct tuple inside a `voption`, such as `unfold` or `tryUncons`,
  is in both namespaces.
* The conversion rules are about representation, not about FSharp.Core having the function. Where FSharp.Core has it
  (`tryFind`, `tryHead`, `choose`, `pick`, `unfold`, `zip`, `partition`, `Map.tryFind`, `Map.change`, …), opening both
  namespaces with `.Option` last yields exactly its signature. Extras without a counterpart (`tryTail`, `uncons`,
  `tryUncons`, `tryGetKey`, `tryGetValue`, `tryAlternateLookup`, map and set `choose`, `SortedSet.tryItem`) get the same
  representation, which is the one FSharp.Core would use, but there is no FSharp.Core signature to match.

This relies on F# name resolution, verified in `src/Compiler/Checking/NameResolution.fs` of the compiler: opened modules are
prepended to the environment (most recently opened first), the resolver takes the first module that contains the member and
falls through otherwise (`CollectAtMostOneResult`), and module search precedes type search. Hence after
`open FSharp.Collections.Immutable` and `open FSharp.Collections.Immutable.Option`, `FlatList.tryFind` returns an `option`
while `FlatList.map` still resolves to the default module; `Option.map` keeps resolving to FSharp.Core; a module named
`FrozenSet` coexists with the BCL static class `FrozenSet`, exactly like the existing `Stack`, `Queue` and `HashSet` modules
coexist with `System.Collections.Generic`. The overloads of D13 depend on the same order: `HashMap.ofSeq` reaches the static
members of the type only because the module `HashMap` has no `ofSeq`. The submodules of D13 resolve the same way: after
`open FSharp.Collections.Immutable.Tuple`, `HashMap.Tuple.addRange` falls through the `HashMap` module of the `Tuple`
namespace, which has no `Tuple` module, to the submodule of the default module.

The override modules delegate to the default modules and convert (`ValueOption.toOption`, `ValueOption.ofOption`, struct
pattern repacking). Per-element tuple producers (`zip`, `pairwise`, `indexed`, `allPairs`, `groupBy`, `countBy`) do not
delegate and remap; both modules call one internal inline helper parameterised by the tuple constructor, so neither pays for
a second pass.

The project guideline "prefer anonymous struct records over tuples for return types" is deliberately not applied to the
FSharp.Core-mirrored shapes: the `Tuple` namespace needs a reference-tuple counterpart, and anonymous records have none.

### D4. Order of functions

Functions are grouped by category and sorted alphabetically (ordinal) inside each category; the order is identical in the
signature and the implementation file; each category is a banner comment. The canonical category sequence is
`Creation`, `Builder`, `Properties`, `Access`, `Search`, `Transformation`, `Folding`, `Partitioning and grouping`, `Sorting`,
`Random`, `Conversion`, `Type-specific`; an empty category is omitted, an active pattern is the last row of its category.
A script `build/check-order.fsx` parses every `.fsi`, verifies the banner sequence and the alphabetical order, checks that
the sibling `.fs` has the same banners and the same sequence of `[<CompiledName>]` bindings, and runs as the FAKE target
`CheckOrder` before `DotnetBuild`. The members of the structs of D12 and of the extension modules of D13 follow the same
category order; the script checks the values of modules only.

### D5. Implementation principle: BCL members and System.Linq first

Priority for every function body:

1. a member of the collection type itself (`ImmutableList.ConvertAll`/`FindAll`/`FindIndex`/`FindLastIndex`/`TrueForAll`/
   `Exists`/`ForEach`/`Reverse`/`GetRange`/`IndexOf`/`Insert`/`RemoveRange`/`SetItem`/`BinarySearch`,
   `ImmutableArray.Slice`/`Sort`/`IndexOf`/`AsSpan`/`CopyTo`, set algebra, `ImmutableDictionary.SetItems`/`TryGetKey`/
   `WithComparers`, Frozen struct enumerators and `Keys`/`Values`/`Items`);
2. `System.Linq.ImmutableArrayExtensions` (`Any`, `All`, `ToArray`; its `First`/`Single`/`ElementAt` throw the wrong
   exception type and are not used);
3. the zero-copy bridge `ImmutableCollectionsMarshal.AsArray`/`AsImmutableArray` plus FSharp.Core `Array.*` for `FlatList`
   transformations (a default array is rejected first; the unwrapped array is never mutated; only freshly allocated arrays
   are wrapped);
4. `System.Linq.Enumerable` and the `ToImmutableX`/`ToFrozenX` extension methods (`ToLookup` for `groupBy`/`countBy`,
   `TakeLast`/`SkipLast`, `Chunk`, `DistinctBy`, `MinBy`/`MaxBy`, `Order*`);
5. FSharp.Core `Seq.*`/`Array.*` for shapes LINQ lacks (`mapFold`, `scanBack`, `windowed`, `pairwise`, `splitInto`,
   `transpose`, `permute`, `random*`);
6. hand-written enumerator loops only where measured (an internal `SeqV` module for `Queue`/`Stack`/interface modules).

The priority ranks only implementations that meet the documented contract (D6 exceptions, ordering, stability, comparison
semantics); a faster candidate that breaks the contract is skipped. In the other direction, a member gives way to a
measured faster path that keeps the contract: `IndexedList.rev` copies the elements to an array, reverses it and builds
the result with `ImmutableList.CreateRange`, about 2.5 times faster than `ImmutableList.Reverse` at 100 000 elements with
the same allocation, and `IIndexedList.rev` reverses the same array and keeps the implementation of its input. Sorting is
the main case of a contract that excludes a member. Stability follows the
FSharp.Core module that a collection mirrors: `FlatList` and `FlatList.Parallel` sort unstably, as `Array.sort*` and
`Array.Parallel.sort*` are documented ("This is not a stable sort"), through `ImmutableArray.Sort` or `Array.sort*` on the
bridge; `IndexedList`, `Queue`, `Stack` and their interface modules sort stably, as `List.sort*` and `Seq.sort*` are
documented, through LINQ `OrderBy`/`OrderByDescending` (documented stable), not through the unstable `ImmutableList.Sort`.
Every sort compares in the order of F# generic comparison (`LanguagePrimitives.FastGenericComparer<'T>`, the comparer
behind `compare`), never with a culture-sensitive comparer such as `Comparer<string>.Default`. ADR 0002 §3 shows the
effect on strings and uses the same order for the elements of `EquatableArray`.

Consequences for the types: `FlatList` range operations (`take`, `skip`, `truncate`, `splitAt`, `takeLast`, `skipLast`,
`chunkBySize`, `windowed`) copy the selected elements. An `ImmutableArray` is a single array reference without an offset or a
length, so `Slice` allocates and copies unless the range is empty (`Empty` is returned) or covers the whole input (the input
itself is returned); zero-copy views of a range are available through `asSpan`/`asMemory` and the span's own `Slice`.
`IndexedList` ranges use `GetRange`, which builds a new tree from the selected elements: its work and allocation grow with
the length of the range (48 B per element in a measurement), not with log n. `Stack` reverses only in its constructors:
the BCL `ImmutableStack.Create` and `CreateRange` push in order and leave the last element on top, so `ofSeq`, `ofArray`,
`ofList`, `ofReadOnlySpan` and `ofSpan` push from the end instead (`ofSeq [1; 2; 3]` has `1` on top), and every
transformation snapshots top-to-bottom, applies `Array.*` and rebuilds the same way. `Queue.length`/`Stack.length` are O(n) and say so. Interface modules keep the
caller's implementation in same-type transformations through `Clear()` plus `AddRange`/`Enqueue`/`Push`.
`FrozenMap.keys`/`values` and `FrozenSet.toFlatList` return the underlying `ImmutableArray` in O(1).

**Comparers.** A transformation keeps each comparer whose type does not change. The key comparer of a map is always kept,
because the keys keep their type (`CreateRange(keyComparer, …)`, `CreateBuilder(keyComparer)`). The value comparer of a map
and the element comparer of a set are kept by `filter`, `partition` and set algebra. `map` and `choose` change the value or
element type, so their result gets the default comparer of the new type. The interface modules cannot read the comparers
of their input, so their type-changing transformations return concrete collections with default comparers. The defaults,
which every module function and extension member that creates a collection uses (comparers come in through the structs
of D12 only), are `EqualityComparer<'T>.Default` for hash-based collections, as ADR 0002 uses for
equality, and the order of F# generic comparison (`LanguagePrimitives.FastGenericComparer<'T>`) for sorted collections.
Thus `SortedSet<string>` is ordinal by default, as F# `Set<string>` is. The sorted modules therefore require
`'T : comparison` and `'Key : comparison` in the functions that create a collection, as `Set` and `Map` do. The default
sorted comparer is one cached instance per type: `FastGenericComparer` creates a new comparer per call for some types, such
as tuples, and the BCL takes its fast paths for equal comparers (`Union`, `WithComparer`) only for the same instance.

### D6. Exceptions mirror FSharp.Core

Each function validates its arguments as the FSharp.Core function of the mirrored module does for the same input, and
throws the same exception type with the same message. The mirrored module is `Array` for `FlatList`, `Array.Parallel` for
`FlatList.Parallel`, `List` for `IndexedList`, `Queue`, `Stack` and their interface modules, `Map` for the map modules and
`Set` for the set modules. The rules differ per operation and per module (FSharp.Core 10, measured):

| Input | `Array` | `List` |
|---|---|---|
| `take 0`, `init 0`, `replicate 0` | valid | valid |
| `take -1`, `init -1`, `replicate -1` | `ArgumentException` | `ArgumentException` |
| `skip -1`, `truncate -1` | valid, as 0 | valid, as 0 |
| `take` or `splitAt` beyond the length | `InvalidOperationException` | `InvalidOperationException` |
| `skip` beyond the length | `ArgumentOutOfRangeException` | `ArgumentOutOfRangeException` |
| `item` out of range | `IndexOutOfRangeException` | `ArgumentException` |
| `insertAt`, `removeAt` out of range | `ArgumentException` | `ArgumentException` |
| `randomSample` beyond the length | `ArgumentException` | `ArgumentException` |
| `chunkBySize 0`, `windowed 0`, `splitInto 0` | `ArgumentException` | `ArgumentException` |
| empty input to `head`, `reduce`, `exactlyOne`, `average`, `min`, `max` | `ArgumentException` | `ArgumentException` |
| no match in `find*`, `findIndex*`, `pick` | `KeyNotFoundException` | `KeyNotFoundException` |

A `null` input, and a default `FlatList`, throw `ArgumentNullException`, as a `null` array does in `Array`. Deliberate
deviations, each documented on the function:

- `ParamName` is the name of the parameter in this library's signature, such as `list`, `queue` or `map`. FSharp.Core uses
  `array`, `list`, `source` or `s`.
- The extras of D1 have no FSharp.Core counterpart. `takeLast` and `skipLast` follow `take` and `skip`; the others throw
  what the BCL member that they wrap throws.

### D7. Computation expressions

One `<Name>CE` class per collection with the delayed-function design (`Buffer<'T> -> unit`), members `Zero`, `Yield`,
`YieldFrom` (sequence, array, same collection, buffer), `Combine`, `Delay`, `For`, `While`, `Run`, `TryWith`, `TryFinally`,
`Using`. Buffers are the BCL builders where they exist (`ImmutableArray.Builder` with `DrainToImmutable`,
`ImmutableList.Builder`, dictionary and set builders) and a `ResizeArray` otherwise. Map expressions accept
`yield struct (key, value)` and `yield KeyValuePair`. Map and set expression classes have a constructor without arguments,
for the default comparers, and one that takes the key or element comparer, as the structs of D12 do. Their
instances are type functions, `[<GeneralizableValue>] let hashSet<'T> = HashSetCE<'T> ()`, because the class holds a
comparer of the element type; the sorted ones require `'T : comparison` for their default comparer (D5). The duplicate
`FlatListBuilderCE` is dropped.

### D8. Tests: shared contract suites

Tests are rewritten on MSTest 4 with the Microsoft.Testing.Platform runner. Each capability is an abstract generic base
class without `[<TestClass>]` (MSTEST0085/MSTEST0002) that takes an immutable record of the module's functions; every
collection derives one closed `[<TestClass>]` per capability it supports, so inapplicable tests do not exist rather than
being skipped. Facets: `SeqLike`, `SeqCreation`, `Indexed`, `StableSort` (every sequence-like module except `FlatList`,
which sorts unstably, see D5); `Map`, `MapTransform`, `MapMutation`, `MapCreation`; `Set`, `SetTransform`, `SetMutation`,
`SetCreation`. The map facets run every binding shape of D13, and the creation facets also the structs of D12.
Expectations are computed with FSharp.Core `List` over the collection's observed enumeration order, so one test body serves
FIFO, LIFO and indexed collections. Collection-specific behaviour
(`isDefault`, spans, builders, `Stack.cons` direction, comparers, sorted-only functions, null inputs) lives in a per-collection
file. Expected exceptions come from the same oracle: the test calls the function of the mirrored FSharp.Core module (D6)
on the same input and asserts the same type and message with `Assert.ThrowsExactly`; `ParamName` is checked against the
ops record. Every assertion carries a message. Categories are the `TestCategoryBaseAttribute` descendants of
`TestCategories.fs` on the test classes: the collection and the component or area that the class examines, such as
`[<TestClass; FlatListTestCategory; BuilderTestCategory>]`. They are never strings, and test methods carry none. Dedicated
files per collection prove that opening the `Option`/`Tuple` namespaces shadows the default functions and that the override
functions are equivalent to the defaults after conversion.

### D9. Delivery: one pull request per collection, stacked

`chore/foundation` (helper module, order check script, test categories and contract bases, changelog and documentation
skeleton, target frameworks), then `feat/flat-list`, `feat/flat-list-parallel` (the nested `Parallel` module of D11),
`feat/equatable-array` (step 2 of ADR 0002: the mirror of the `FlatList` module, its builder functions and
`equatableArray { }`), `feat/indexed-list`, `feat/queue`, `feat/stack`, `feat/hash-map` (with `IMap`), `feat/sorted-map`,
`feat/hash-set` (with `ISet`), `feat/sorted-set`, `feat/frozen-map`, `feat/frozen-set` (both with the alternate lookup of
D11), and an optional `chore/benchmarks` (BenchmarkDotNet, excluded from the solution filter). Each pull request deletes
the legacy file it replaces, keeps the build green and updates the changelog and the documentation table. No release is
cut mid-chain.
Step 1 of ADR 0002 (the `EquatableArray` type, its patterns and JSON converter) MAY ship from `main` before the chain.

### D10. Target frameworks

`net8.0;net10.0`. The published package already targeted `net8.0` only; `netstandard2.0` is not added (the LINQ surface
used in D5 would need conditional fallbacks); `net9.0` is not added (short-term support, ended 2026-05; the `net8.0` asset
serves it). `net10.0` enables `#if NET10_0_OR_GREATER` paths for newer LINQ members and the Frozen alternate lookup (D11).

### D11. `FlatList.Parallel` and the Frozen alternate lookup

`FlatList` gets a nested module `Parallel` that mirrors `Array.Parallel` through the zero-copy bridge of D5: every function
unwraps the array, calls `Array.Parallel.*` and wraps the fresh result (`map`, `mapi`, `choose`, `collect`, `filter`, `init`,
`iter`, `iteri`, `zip`, `groupBy`, `partition`, `partitionWith`, `sort`, `sortBy`, `sortByDescending`, `sortDescending`,
`sortWith`, `reduce`, `reduceBy`, `sum`, `sumBy`, `average`, `averageBy`, `max`, `maxBy`, `min`, `minBy`, `exists`, `forall`,
`tryFind`, `tryFindIndex`, `tryPick`); the three `sortInPlace*` functions are omitted because they mutate. The nested module
lives in `FlatList.fsi`/`FlatList.fs` (as `Array.Parallel` lives in `array.fs`) with its own category banners, and has its
`Option` and `Tuple` counterparts (`choose`, `tryFind`, `tryFindIndex`, `tryPick`; `groupBy`, `partition`, `partitionWith`,
`zip`). Its tests compare every function with the sequential `FlatList` counterpart on the same inputs.

`FrozenMap` and `FrozenSet` expose `alternateLookup<'Alternate>` (`GetAlternateLookup`) and `tryAlternateLookup<'Alternate>`
(`TryGetAlternateLookup`, returning `voption`) under `#if NET10_0_OR_GREATER`, because the API exists only in the `net9.0`
and later assets of the package. Their type parameters are `<'Alternate, 'Key, 'Value>` (map) and `<'Alternate, 'T>` (set),
`'Alternate` first: F# does not accept explicit type arguments for only some of the type parameters (FS0687), so a call
reads `FrozenMap.alternateLookup<int, _, _> table`. They return the BCL `AlternateLookup<'Alternate>` struct, whose
`ContainsKey`, `TryGetValue` and indexer (map) or `Contains` and `TryGetValue` (set) do the lookups; no
`findAlternate`-style wrappers over those members are added. The comparer must implement
`IAlternateEqualityComparer<'Alternate, 'Key>`; `tryAlternateLookup` returns `ValueNone` otherwise. The tests for these
functions compile only for `net10.0`.

The wrappers cannot serve `ReadOnlySpan<char>`, the alternate key type that string keys use with `StringComparer.Ordinal`.
Since F# 9, F# can instantiate generic parameters that C# declares with the `allows ref struct` anti-constraint, so
`map.GetAlternateLookup<ReadOnlySpan<char>> ()` and the members of the returned struct work when called directly. F# cannot
declare that anti-constraint on its own type parameters, though: the
[F# byrefs documentation](https://learn.microsoft.com/dotnet/fsharp/language-reference/byrefs) states that generic code
supporting byref-like types cannot be authored in F#, and [dotnet/fsharp#16800](https://github.com/dotnet/fsharp/issues/16800)
limited the F# 9 support to consumption. An F# wrapper instantiated with `ReadOnlySpan<char>` therefore fails with FS0412
("A type instantiation involves a byref type").
[fslang-suggestions#1476](https://github.com/fsharp/fslang-suggestions/issues/1476) and RFC FS-1356 propose that F# declares
the anti-constraint. When that ships, the wrappers declare `'Alternate : allows ref struct` and accept spans. Until then, the
documentation of both functions points span users to the BCL methods, and the wrappers serve alternate key types that are
not byref-like, such as an `int` alternate for `int64` keys with a custom comparer.

### D12. Comparers: one `CustomComparers*` or `CustomComparer*` struct per collection

No module function creates a collection with comparers. Every collection that can be created with comparers gets a struct,
in its own file of the collection's folder, that holds the comparers and creates the collection with them. The prefix
names what the struct holds: `CustomComparers` for maps, `CustomComparer` for sets, which have one comparer. Functions that
use a comparer for one operation only, such as `sortWithComparer`, `indexOfWith` and `lastIndexOfWith`, keep their comparer
argument.

| Struct | Constructors | Module functions that create it | Comparer properties |
|---|---|---|---|
| `CustomComparersHashMap<'Key, 'Value>` | `(keyComparer : IEqualityComparer<'Key>, valueComparer : IEqualityComparer<'Value>)`, `(keyComparer)` | `HashMap.forComparers`, `HashMap.forKeyComparer` | `KeyComparer`, `ValueComparer` |
| `CustomComparersSortedMap<'Key, 'Value>` | `(keyComparer : IComparer<'Key>, valueComparer : IEqualityComparer<'Value>)`, `(keyComparer)` | `SortedMap.forComparers`, `SortedMap.forKeyComparer` | `KeyComparer`, `ValueComparer` |
| `CustomComparersFrozenMap<'Key, 'Value>` | `(keyComparer : IEqualityComparer<'Key>)` | `FrozenMap.forKeyComparer` | `KeyComparer` |
| `CustomComparerHashSet<'T>` | `(comparer : IEqualityComparer<'T>)` | `HashSet.forComparer` | `Comparer` |
| `CustomComparerSortedSet<'T>` | `(comparer : IComparer<'T>)` | `SortedSet.forComparer` | `Comparer` |
| `CustomComparerFrozenSet<'T>` | `(comparer : IEqualityComparer<'T>)` | `FrozenSet.forComparer` | `Comparer` |

A map constructor with the key comparer alone gives the values `EqualityComparer<'Value>.Default`; a frozen dictionary has
no value comparer, so `CustomComparersFrozenMap` has one constructor. The module functions are in the `Creation` category
and take the comparers in the order of the constructors: `forComparers keyComparer valueComparer` and
`forKeyComparer keyComparer` for maps, `forComparer comparer` for sets. A call reads as one phrase,
`(HashMap.forKeyComparer StringComparer.OrdinalIgnoreCase).ofSeq pairs`.

The members of a struct are the creation functions of the module with the comparers applied, under the names of those
functions: `empty`, `singleton`, `ofArray`, `ofList` and `ofSeq` (for maps with the overloads of D13), `ofSeqBy` (maps) and
`builder` (all collections except the frozen ones, which have no builder). The member names stay lowercase, so that a call
reads like the module function that it stands for. Their compiled names are PascalCase (`[<CompiledName "OfSeq">]`), as
for module functions. The structs are `[<Struct; NoComparison; NoEquality>]`: they only carry comparers, which have no
meaningful equality, and creating one allocates nothing.

The structs replace the comparer variants of the creation functions: `emptyWithComparers`, `emptyWithKeyComparer`,
`emptyWithComparer`, `ofSeqWithComparer`, `ofSeqWithKeyComparer`, `ofKeyValuePairsWithKeyComparer`,
`builderWithComparers`, `builderWithKeyComparer`, `builderWithComparer`, `withComparers`, `withKeyComparer` and
`withComparer`. `ofSeq` of a struct also takes an existing collection, because a map is a sequence of `KeyValuePair`s and a
set a sequence of elements, so it gives that collection other comparers. The comparer properties of the modules
(`keyComparer`, `valueComparer`, `comparer`) stay: they read the comparers of a collection.

### D13. Map functions take bindings in every shape

A map binding comes in three shapes: `KeyValuePair<'Key, 'Value>`, which every .NET dictionary enumerates (`dict` and
`Map` included); the reference tuple `'Key * 'Value` of most F# code (`Map.toList`); and the struct tuple of D3. A module
function takes one shape. F# types a tuple literal that is written as the argument as struct tuples, but a list of reference
tuples that is bound before, or any sequence of `KeyValuePair`s, needs a conversion first.

Each map folder therefore has an `[<AutoOpen>]` root module `XExtensions` in its own file. It extends the BCL type
(`ImmutableDictionary<'Key, 'Value>`, `ImmutableSortedDictionary<'Key, 'Value>`, `FrozenDictionary<'Key, 'Value>`) with
the static members `ofArray`, `ofList` and `ofSeq`, each overloaded for the three shapes. Through the type abbreviation,
`HashMap.ofSeq pairs` reaches them and overload resolution picks the shape; a pipeline `pairs |> HashMap.ofSeq` works too.
This was checked with lists, arrays, `dict` and an existing map. The rules that follow from F#:

* The map modules have no `ofArray`, `ofList` or `ofSeq`: module search precedes type search (D3), so a module function
  with the same name hides the extension members.
* Only functions with one argument get overloads, because F# does not overload curried members (FS0816). `addRange` and
  `setItems` take the bindings and the map, so they stay module functions: the default takes `KeyValuePair`s, the shape
  that every dictionary already has, and the nested submodules `Tuple` and `ValueTuple` of the module hold the same
  functions for reference and struct tuples, `HashMap.Tuple.addRange pairs table` and
  `HashMap.ValueTuple.setItems pairs table`. The interface module `IMap` has the same submodules.
* `SortedMapExtensions` members declare type parameters of their own (`ofSeq<'K, 'V when 'K : comparison>`): F# generic
  comparison needs `comparison`, and a member cannot add a constraint to the type parameters of the extended type
  (FS0340). The extensions repeat the constraints of the BCL types (`'Key : not null`).
* Every overload has the semantics of `Map.ofSeq`: a later binding of a key replaces an earlier one. The extension members
  are the members of the struct of D12 with the default comparers, so each shape has one implementation.
  `ofKeyValuePairs` disappears: it is the `KeyValuePair` overload of `ofSeq`.
* Overload resolution needs the shape. Inside generic code, such as `let build pairs = HashMap.ofSeq pairs`, the argument
  needs a type annotation (FS0041).

Static extension members are an F# feature: C# does not see them as members of the BCL types. C# callers use the members
of the structs of D12, whose compiled names are PascalCase, or the BCL itself. Sets need no extensions and no submodules,
because their elements have one shape.

## Consequences

Breaking changes (major version): `try*` return `voption`; tuples become struct tuples; `choose`/`pick`/`tryPick`/`unfold`/
`change` callbacks use `voption`; `ImmutableList` module becomes `IndexedList`; `Seq`, `IndexedSeq`/`ReadOnlyList`, the
`ISet<'T>` abbreviation and the extras listed in D1 are removed; the comparer variants of the creation functions and
`ofKeyValuePairs` give way to the structs of D12 and to the overloads of map creation, which C# does not see
(D13); map and set `length` becomes `count`; map `filter`/`map` follow FSharp.Core semantics; sorted maps and sets order
by F# generic comparison unless a comparer is given; `Stack`
element order is defined (first element of `ofSeq` on top); `take`/`skip` throw on short input; `netstandard2.0` is not a
target. The `Option` and `Tuple` namespaces give consumers a one-line migration path.

Benefits: one vocabulary across ten modules, consistent with FSharp.Core 10.0.300; defects of the current map and set modules
fixed by construction (comparer preservation, real `singleton`); fewer allocations (`voption`, struct tuples, span and memory views, zero-copy
bridge); `Frozen*` support; tests that cannot drift between collections; machine-checked function order; the comparers of a
collection in one place; maps built from bindings of any shape without a conversion.

Costs: about 150 sequence functions × 7 modules plus maps, sets and frozen collections to implement and document; the
override namespaces double the number of signature files; contributors must learn the category order (enforced by the
script, so mistakes fail fast); every folder gets a types file, and the map and set folders a struct file, the map folders
also an extensions file; an overloaded map `ofSeq` needs a type annotation inside generic code.

## Alternatives considered

* **Strictly alphabetical order** – trivially checkable but reads worse in generated documentation; rejected in favour of
  categories with alphabetical order inside.
* **Keep every extra from the started branch** – about 190 matrix rows, more surface to document and test for functions
  without precedent in FSharp.Core; rejected.
* **Read-only Frozen modules** (no `map`/`filter`) – honest about the type's purpose but forces every transformation
  through `Seq` and a re-freeze; rejected in favour of documented O(n) transformations without incremental mutation.
* **Per-collection test files** (as on the started branch) – full independence but roughly tenfold duplication and the
  drift that produced today's contradicting exception contracts; rejected.
* **`Assert.Inconclusive` or capability flags** for inapplicable contract tests – noisy reports and silently skipped tests
  when a flag is forgotten; rejected in favour of one base class per capability.
* **`netstandard2.0`** – would keep .NET Framework consumers but requires conditional LINQ fallbacks; not a regression
  because the published package is `net8.0` only; deferred.
* **A comparer variant of every creation function** (`emptyWithComparers`, `emptyWithKeyComparer`, `ofSeqWithKeyComparer`,
  `builderWithComparers`, `withComparers`, …), the first design of D12 – one more function per creation function and per
  combination of comparers; replaced by one struct per collection that holds the comparers and offers every creation
  function with them.
* **One module function per binding shape** (`ofSeq` over struct tuples, `ofKeyValuePairs`, `Tuple.HashMap.ofSeq` over
  reference tuples), the first design of D13 – three names or a namespace switch for one operation; replaced by overloads
  on the type, which module functions cannot have.
* **A namespace with one nested module in every file**, the first layout of D2 – one more level of indentation and the
  type abbreviation next to the module; replaced by root modules and a types file per folder.

## Appendix A – sequence-like collections

`FlatList` (`ImmutableArray`), `IndexedList` (`ImmutableList`), `IIndexedList` (`IImmutableList`), `Queue`, `IQueue`,
`Stack`, `IStack`. Legend: `✓` FSharp.Core semantics · `ᵇ` wraps a BCL-specific member · `ᵃ` extra beyond FSharp.Core ·
`–` not provided. Shape: `Ⓥ` `voption` by default (`option` in `Option.*`) · `Ⓣ` struct tuple (reference tuple in `Tuple.*`).
Interface modules have no creation, builder or comparer functions; their same-type transformations return the interface,
their element-type-changing transformations return the concrete type.

| Function | Shape | FlatList | IndexedList | IIndexedList | Queue | IQueue | Stack | IStack | Note |
|---|---|---|---|---|---|---|---|---|---|
| **Creation** | | | | | | | | | |
| `create` | | ✓ | – | – | – | – | – | – | Array-only name; others use `replicate` |
| `empty` | | ✓ | ✓ | – | ✓ | – | ✓ | – | `ImmutableX<'T>.Empty` |
| `init` | | ✓ | ✓ | – | ✓ | – | ✓ | – | sequential initializer |
| `ofArray` | | ✓ | ✓ | – | ✓ | – | ✓ | – | copies; Stack pushes from the end |
| `ofList` | | ✓ | ✓ | – | ✓ | – | ✓ | – | |
| `ofReadOnlySpan` | | ᵇ | ᵇ | – | ᵇ | – | ᵇ | – | `Create(ReadOnlySpan)`; Stack pushes from the end, because the BCL `Create` leaves the last element on top |
| `ofSeq` | | ✓ | ✓ | – | ✓ | – | ✓ | – | `CreateRange`; Stack pushes a snapshot from the end, so the first element is on top |
| `ofSpan` | | ᵇ | ᵇ | – | ᵇ | – | ᵇ | – | as `ofReadOnlySpan` |
| `replicate` | | ✓ | ✓ | – | ✓ | – | ✓ | – | |
| `singleton` | | ✓ | ✓ | – | ✓ | – | ✓ | – | |
| `unfold` | Ⓥ+Ⓣ | ✓ | ✓ | – | ✓ | – | ✓ | – | `'State -> struct ('T * 'State) voption` |
| **Builder** | | | | | | | | | |
| `build` | | ᵃ | ᵃ | – | – | – | – | – | `(Builder -> unit) -> X<'T>` |
| `builder` | | ᵇ | ᵇ | – | – | – | – | – | |
| `builderWith` | | ᵇ | – | – | – | – | – | – | capacity |
| `moveFromBuilder` | | ᵇ | – | – | – | – | – | – | `MoveToImmutable` |
| `ofBuilder` | | ᵇ | ᵇ | – | – | – | – | – | `ToImmutable` |
| `toBuilder` | | ᵇ | ᵇ | – | – | – | – | – | |
| `update` | | ᵃ | ᵃ | – | – | – | – | – | `toBuilder` → f → `ToImmutable` |
| **Properties** | | | | | | | | | |
| `isEmpty` | | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | |
| `length` | | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | Queue/Stack O(n) |
| **Access** | | | | | | | | | |
| `exactlyOne` | | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | `ArgumentException` |
| `head` | | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | Queue/Stack `Peek` |
| `item` | | ✓ | ✓ | ✓ | – | – | – | – | |
| `last` | | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | LINQ `Last` for Queue/Stack |
| `tail` | | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | `Dequeue`/`Pop` |
| `tryExactlyOne` | Ⓥ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | |
| `tryHead` | Ⓥ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | |
| `tryItem` | Ⓥ | ✓ | ✓ | ✓ | – | – | – | – | |
| `tryLast` | Ⓥ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | |
| `tryTail` | Ⓥ | ᵃ | ᵃ | ᵃ | ᵃ | ᵃ | ᵃ | ᵃ | |
| `tryUncons` | Ⓥ+Ⓣ | – | ᵃ | ᵃ | ᵃ | ᵃ | ᵃ | ᵃ | `struct ('T * X<'T>) voption` |
| `uncons` | Ⓣ | – | ᵃ | ᵃ | ᵃ | ᵃ | ᵃ | ᵃ | compiled `PeekDequeue`/`PeekPop` |
| `(\|Cons\|_\|)` | Ⓥ+Ⓣ | – | ᵃ | ᵃ | ᵃ | ᵃ | ᵃ | ᵃ | `[<return: Struct>]`, `struct ('T * X<'T>) voption`; a multi-case pattern allocates a `Choice` per match (ADR 0002 §5) |
| `(\|Nil\|_\|)` | | – | ᵃ | ᵃ | ᵃ | ᵃ | ᵃ | ᵃ | returns `bool` |
| **Search** | | | | | | | | | |
| `binarySearch` | | ᵇ | ᵇ | – | – | – | – | – | BCL bitwise-complement contract |
| `contains` | | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | |
| `exists` / `exists2` | | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | |
| `find` / `findBack` | | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | `KeyNotFoundException` |
| `findIndex` / `findIndexBack` | | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | |
| `forall` / `forall2` | | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | |
| `indexOf` / `indexOfWith` | | ᵇ | ᵇ | ᵇ | – | – | – | – | `-1` when absent |
| `lastIndexOf` / `lastIndexOfWith` | | ᵇ | ᵇ | ᵇ | – | – | – | – | |
| `pick` | Ⓥ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | |
| `tryFind` / `tryFindBack` | Ⓥ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | |
| `tryFindIndex` / `tryFindIndexBack` | Ⓥ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | |
| `tryPick` | Ⓥ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | |
| **Transformation** | | | | | | | | | |
| `add` | | ᵇ | ᵇ | ᵇ | – | – | – | – | append one |
| `allPairs` | Ⓣ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | |
| `append` | | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | |
| `choose` | Ⓥ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | |
| `chunkBySize` | | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | `X<X<'T>>` |
| `collect` / `concat` | | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | |
| `cons` | | – | ᵃ | ᵃ | – | – | ᵇ | ᵇ | Stack `Push` |
| `consMany` | | – | – | – | – | – | ᵃ | ᵃ | first element ends on top |
| `enqueue` / `enqueueMany` | | – | – | – | ᵇ/ᵃ | ᵇ/ᵃ | – | – | |
| `filter` / `where` | | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | |
| `indexed` | Ⓣ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | |
| `insertAt` / `insertManyAt` | | ✓ | ✓ | ✓ | – | – | – | – | |
| `map` / `map2` / `map3` / `mapi` / `mapi2` | | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | |
| `pairwise` | Ⓣ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | |
| `permute` | | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | |
| `removeAt` / `removeManyAt` | | ✓ | ✓ | ✓ | – | – | – | – | |
| `rev` | | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | IndexedList: array copy, `Array.Reverse`, `CreateRange`, measured faster than `ImmutableList.Reverse` (D5) |
| `transpose` | | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | |
| `unzip` / `unzip3` | Ⓣ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | |
| `updateAt` | | ✓ | ✓ | ✓ | – | – | – | – | |
| `windowed` | | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | |
| `zip` / `zip3` | Ⓣ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | `ArgumentException` on mismatch |
| **Folding** | | | | | | | | | |
| `average` / `averageBy` | | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | |
| `compareWith` | | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | |
| `fold` / `fold2` / `foldBack` / `foldBack2` | | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | |
| `iter` / `iter2` / `iteri` / `iteri2` | | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | |
| `mapFold` / `mapFoldBack` | Ⓣ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | |
| `max` / `maxBy` / `min` / `minBy` | | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | |
| `reduce` / `reduceBack` | | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | |
| `scan` / `scanBack` | | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | |
| `sum` / `sumBy` | | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | |
| **Partitioning and grouping** | | | | | | | | | |
| `countBy` | Ⓣ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | |
| `distinct` / `distinctBy` | | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | |
| `except` | | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | FSharp.Core semantics |
| `groupBy` | Ⓣ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | |
| `partition` / `partitionWith` | Ⓣ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | |
| `skip` / `skipWhile` | | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | |
| `skipLast` / `takeLast` | | ᵃ | ᵃ | ᵃ | ᵃ | ᵃ | ᵃ | ᵃ | |
| `splitAt` | Ⓣ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | |
| `splitInto` | | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | FSharp.Core chunk sizes |
| `take` / `takeWhile` / `truncate` | | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | `take` throws when short |
| **Sorting** | | | | | | | | | |
| `sort` / `sortBy` / `sortByDescending` / `sortDescending` / `sortWith` | | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | FlatList unstable like `Array.sort*`; the others stable like `List.sort*` (LINQ `OrderBy`) |
| `sortWithComparer` | | ᵇ | ᵇ | ᵇ | ᵇ | ᵇ | ᵇ | ᵇ | `IComparer<'T>`; same stability rule |
| **Random** | | | | | | | | | |
| `randomChoice`, `randomChoices`, `randomSample`, `randomShuffle` (+ `By`, `With`) | | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | 12 functions |
| **Conversion** | | | | | | | | | |
| `asMemory` / `asSpan` | | ᵇ | – | – | – | – | – | – | O(1) |
| `blit` | | ✓ | ✓ | – | – | – | – | – | `CopyTo` |
| `toArray` / `toList` / `toSeq` | | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | |
| **Type-specific** | | | | | | | | | |
| `clear` | | – | – | ᵇ | – | ᵇ | – | ᵇ | keeps the implementation |
| `isDefault` / `isDefaultOrEmpty` | | ᵇ | – | – | – | – | – | – | accept a default `FlatList` |
| **Parallel** (nested module `FlatList.Parallel`, mirrors `Array.Parallel`; same category order inside) | | | | | | | | | |
| `average` / `averageBy` / `max` / `maxBy` / `min` / `minBy` / `reduce` / `reduceBy` / `sum` / `sumBy` | | ✓ | – | – | – | – | – | – | Folding |
| `choose` / `collect` / `filter` / `init` / `iter` / `iteri` / `map` / `mapi` / `zip` | Ⓥ (`choose`), Ⓣ (`zip`) | ✓ | – | – | – | – | – | – | Transformation (`init`: Creation) |
| `exists` / `forall` / `tryFind` / `tryFindIndex` / `tryPick` | Ⓥ (`try*`) | ✓ | – | – | – | – | – | – | Search |
| `groupBy` / `partition` / `partitionWith` | Ⓣ | ✓ | – | – | – | – | – | – | Partitioning and grouping |
| `sort` / `sortBy` / `sortByDescending` / `sortDescending` / `sortWith` | | ✓ | – | – | – | – | – | – | Sorting, unstable like `Array.Parallel.sort*`; `sortInPlace*` omitted |

`Option.*`: `choose`, `pick`, `tryExactlyOne`, `tryFind`, `tryFindBack`, `tryFindIndex`, `tryFindIndexBack`, `tryHead`,
`tryItem`, `tryLast`, `tryPick`, `tryTail`, `tryUncons`, `unfold`, `(|Cons|_|)`.
`Tuple.*`: `allPairs`, `countBy`, `groupBy`, `indexed`, `mapFold`, `mapFoldBack`, `pairwise`, `partition`, `partitionWith`,
`splitAt`, `unfold`, `unzip`, `unzip3`, `zip`, `zip3`, `uncons`, `tryUncons`, `(|Cons|_|)` where present.

`EquatableArray<'T>` of ADR 0002 mirrors the `FlatList` column, without `isDefault` and `isDefaultOrEmpty`, and adds
`ofImmutableArray`, `toImmutableArray` and `unsafeOfArray`.

## Appendix B – maps

`HashMap` (`ImmutableDictionary`), `SortedMap` (`ImmutableSortedDictionary`), `IMap` (`IImmutableDictionary`),
`FrozenMap` (`FrozenDictionary`).

| Function | Shape | HashMap | SortedMap | IMap | FrozenMap | Note |
|---|---|---|---|---|---|---|
| **Creation** | | | | | | |
| `empty` | | ✓ | ✓ | – | ✓ | |
| `forComparers` / `forKeyComparer` | | ᵃ | ᵃ | – | –/ᵃ | create the `CustomComparers*` struct of D12 |
| `ofArray` / `ofList` / `ofSeq` | | ✓ | ✓ | – | ✓ | static extension members (D13), each overloaded for `KeyValuePair`, `'Key * 'Value` and `struct ('Key * 'Value)` |
| `ofSeqBy` | | ᵇ | ᵇ | – | ᵇ | key selector |
| `singleton` | | ✓ | ✓ | – | ✓ | curried `key value` |
| **Builder** | | | | | | |
| `builder` / `ofBuilder` / `toBuilder` | | ᵇ | ᵇ | – | – | |
| **Properties** | | | | | | |
| `count` / `isEmpty` | | ✓ | ✓ | ✓ | ✓ | |
| `keyComparer` / `valueComparer` | | ᵇ | ᵇ | – | ᵇ/– | |
| `keys` / `values` | | ✓ | ✓ | ✓ | ✓ | the cheapest view without a copy: `ICollection<_>` as `Map.keys` (hash, sorted), `seq<_>` (`IMap`: the interface has only `IReadOnlyDictionary.Keys`), `FlatList` O(1) (frozen) |
| **Access** | | | | | | |
| `find` | | ✓ | ✓ | ✓ | ✓ | `KeyNotFoundException` |
| `maxKeyValue` / `minKeyValue` | Ⓣ | – | ✓ | – | – | sorted only; `maxKeyValue` is O(n), because `ImmutableSortedDictionary` enumerates forwards only |
| `tryFind` | Ⓥ | ✓ | ✓ | ✓ | ✓ | |
| `tryGetKey` | Ⓥ | ᵇ | ᵇ | ᵇ | – | |
| **Search** | | | | | | |
| `containsKey` / `containsValue` | | ✓/ᵇ | ✓/ᵇ | ✓/– | ✓/ᵃ | |
| `exists` / `forall` | | ✓ | ✓ | ✓ | ✓ | |
| `findKey` / `tryFindKey` | Ⓥ | ✓ | ✓ | ✓ | ✓ | |
| `pick` / `tryPick` | Ⓥ | ✓ | ✓ | ✓ | ✓ | |
| **Transformation** | | | | | | |
| `add` / `addRange` | | ✓/ᵇ | ✓/ᵇ | ✓/ᵇ | – | `addRange` takes `KeyValuePair`s; `Tuple.addRange` and `ValueTuple.addRange` in the submodules (D13) |
| `change` | Ⓥ | ✓ | ✓ | ✓ | – | |
| `choose` | Ⓥ | ᵃ | ᵃ | ᵃ | ᵃ | Frozen rebuild |
| `clear` | | ᵇ | ᵇ | ᵇ | – | keeps comparers |
| `filter` | | ✓ | ✓ | ✓ | ✓ | returns a map |
| `map` | | ✓ | ✓ | ✓ | ✓ | keys kept |
| `remove` / `removeRange` | | ✓/ᵇ | ✓/ᵇ | ✓/ᵇ | – | |
| `setItem` / `setItems` | | ᵇ | ᵇ | ᵇ | – | add or replace; `setItems` takes `KeyValuePair`s, `Tuple.setItems` and `ValueTuple.setItems` in the submodules (D13) |
| **Folding** | | | | | | |
| `fold` / `foldBack` / `iter` | | ✓ | ✓ | ✓ | ✓ | |
| **Partitioning** | | | | | | |
| `partition` | Ⓣ | ✓ | ✓ | ✓ | ✓ | |
| **Conversion** | | | | | | |
| `toArray` / `toList` / `toSeq` | Ⓣ | ✓ | ✓ | ✓ | ✓ | `struct ('Key * 'Value)` |
| **Type-specific** | | | | | | |
| `alternateLookup<'Alternate>` | | – | – | – | ᵇ | `GetAlternateLookup`; `#if NET10_0_OR_GREATER` |
| `tryAlternateLookup<'Alternate>` | Ⓥ | – | – | – | ᵇ | `TryGetAlternateLookup`; `#if NET10_0_OR_GREATER` |

`Option.*`: `change`, `choose`, `pick`, `tryFind`, `tryFindKey`, `tryGetKey`, `tryPick` (+ `tryAlternateLookup` for
`FrozenMap`, `net10.0` only). `Tuple.*`: `partition`, `toArray`, `toList`, `toSeq` (+ `maxKeyValue`, `minKeyValue` for
`SortedMap`). Submodules `Tuple` and `ValueTuple` (D13): `addRange`, `setItems` (`HashMap`, `SortedMap`, `IMap`).

The structs of D12:

| Member | `CustomComparersHashMap` | `CustomComparersSortedMap` | `CustomComparersFrozenMap` |
|---|---|---|---|
| constructor with both comparers | ✓ | ✓ | – |
| constructor with the key comparer | ✓ | ✓ | ✓ |
| `KeyComparer` / `ValueComparer` | ✓ | ✓ | `KeyComparer` |
| `empty` / `singleton` | ✓ | ✓ | ✓ |
| `ofArray` / `ofList` / `ofSeq`, each in the three shapes of D13 | ✓ | ✓ | ✓ |
| `ofSeqBy` | ✓ | ✓ | ✓ |
| `builder` | ✓ | ✓ | – |

## Appendix C – sets

`HashSet` (`ImmutableHashSet`), `SortedSet` (`ImmutableSortedSet`), `ISet` (`IImmutableSet`), `FrozenSet` (`FrozenSet`).

| Function | Shape | HashSet | SortedSet | ISet | FrozenSet | Note |
|---|---|---|---|---|---|---|
| **Creation** | | | | | | |
| `empty` | | ✓ | ✓ | – | ✓ | |
| `forComparer` | | ᵃ | ᵃ | – | ᵃ | creates the `CustomComparer*` struct of D12 |
| `ofArray` / `ofList` / `ofSeq` | | ✓ | ✓ | – | ✓ | |
| `singleton` | | ✓ | ✓ | – | ✓ | |
| **Builder** | | | | | | |
| `builder` / `ofBuilder` / `toBuilder` | | ᵇ | ᵇ | – | – | |
| **Properties** | | | | | | |
| `comparer` | | ᵇ | ᵇ | – | ᵇ | |
| `count` / `isEmpty` | | ✓ | ✓ | ✓ | ✓ | |
| **Access** | | | | | | |
| `item` / `tryItem` / `indexOf` | Ⓥ (`tryItem`) | – | ᵇ | – | – | sorted only |
| `maxElement` / `minElement` | | – | ✓ | – | – | sorted only |
| `tryGetValue` | Ⓥ | ᵇ | ᵇ | ᵇ | ᵇ | |
| **Search** | | | | | | |
| `contains` / `exists` / `forall` | | ✓ | ✓ | ✓ | ✓ | |
| `isProperSubset` / `isProperSuperset` / `isSubset` / `isSuperset` | | ✓ | ✓ | ✓ | ✓ | `isSubset set1 set2` = set1 ⊆ set2 |
| `overlaps` / `setEquals` | | ᵇ | ᵇ | ᵇ | ᵇ | |
| **Transformation** | | | | | | |
| `add` / `remove` / `clear` | | ✓ | ✓ | ✓ | – | |
| `choose` | Ⓥ | ᵃ | ᵃ | ᵃ | ᵃ | |
| `difference` / `intersect` / `union` | | ✓ | ✓ | ✓ | ✓ | FSharp.Core argument order |
| `filter` / `map` | | ✓ | ✓ | ✓ | ✓ | comparer kept where the element type allows |
| `intersectMany` / `unionMany` | | ✓ | ✓ | ✓ | ✓ | |
| `rev` | | – | ᵇ | – | – | |
| `symmetricDifference` | | ᵇ | ᵇ | ᵇ | ᵃ | |
| **Folding** | | | | | | |
| `fold` / `foldBack` / `iter` | | ✓ | ✓ | ✓ | ✓ | |
| **Partitioning** | | | | | | |
| `partition` / `partitionWith` | Ⓣ | ✓ | ✓ | ✓ | ✓ | |
| **Conversion** | | | | | | |
| `toArray` / `toList` / `toSeq` | | ✓ | ✓ | ✓ | ✓ | |
| `toFlatList` | | – | – | – | ᵇ | O(1) via `Items` |
| **Type-specific** | | | | | | |
| `alternateLookup<'Alternate>` | | – | – | – | ᵇ | `GetAlternateLookup`; `#if NET10_0_OR_GREATER` |
| `tryAlternateLookup<'Alternate>` | Ⓥ | – | – | – | ᵇ | `TryGetAlternateLookup`; `#if NET10_0_OR_GREATER` |

`Option.*`: `choose`, `tryGetValue` (+ `tryItem` for `SortedSet`, `tryAlternateLookup` for `FrozenSet`, `net10.0` only).
`Tuple.*`: `partition`, `partitionWith`.

The structs of D12:

| Member | `CustomComparerHashSet` | `CustomComparerSortedSet` | `CustomComparerFrozenSet` |
|---|---|---|---|
| constructor with the comparer | `IEqualityComparer<'T>` | `IComparer<'T>` | `IEqualityComparer<'T>` |
| `Comparer` | ✓ | ✓ | ✓ |
| `empty` / `singleton` | ✓ | ✓ | ✓ |
| `ofArray` / `ofList` / `ofSeq` | ✓ | ✓ | ✓ |
| `builder` | ✓ | ✓ | – |
