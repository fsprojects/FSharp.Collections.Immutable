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
`cons`/`uncons`/`tryUncons`, the struct partial patterns `(|Cons|_|)`/`(|Nil|_|)`, `tryTail`, `enqueue`/`enqueueMany`, `consMany`, `takeLast`/`skipLast`, `clear`
on interface modules, `tryGetKey`/`tryGetValue`, `setItem`/`setItems`/`addRange`/`removeRange`, `containsValue`,
`symmetricDifference`, `overlaps`/`setEquals`, comparer accessors and `with*Comparer` functions.

Removed from the started branch: `iter3`/`iteri3`/`exists3`/`forall3`/`mapi3`/`foldBack3`, `foldi`/`foldi2`/`foldBacki`/
`foldBacki2`, `findDup`/`findDupBy`, `pickBack`/`tryPickBack`/`chooseBack`, the `From`/`Range` variants of `index*`/
`lastIndex*`, `removeAll*`, `sortRange*`, `fill`, `zeroCreate`, `copy`, `ofOption`/`toOption`/`ofValueOption`/
`toValueOption`, `tryHeadAndTail`/`tryLastAndInit`, `enqueueArray`/`enqueueList`/`consArray`/`consList`, `unconsMany`,
and the modules `Seq` (cross conversions) and `IndexedSeq` (`IReadOnlyList`).

### D2. Layout

One folder per collection; every implementation file with public API has a signature file that carries the XML
documentation. Per folder, in compile order: `X.fsi`, `X.fs`, `XOption.fsi`, `XOption.fs`, `XTuple.fsi`, `XTuple.fs`,
`IX.fsi`, `IX.fs` (plus their `Option`/`Tuple` files), `XBuilder.fs` (`FlatList` and `IndexedList` only), `XCE.fs`.
`Helper.fs` comes first and `ComputationExpressions.fs` last. Interface modules compile after the concrete type they return.

Names: `IMap` module and `IMap<'Key, 'Value>` abbreviation for `IImmutableDictionary`; `ISet` module **without** an
`ISet<'T>` abbreviation, because it would shadow `System.Collections.Generic.ISet<'T>`, which `FrozenSet<'T>` implements.
Maps and sets measure their size with `count` (FSharp.Core `Map.count`/`Set.count`), sequences with `length`. `Queue` keeps
`enqueue`/`head`/`tail`/`uncons` (compiled `Enqueue`/`Peek`/`Dequeue`/`PeekDequeue`), `Stack` keeps `cons`/`head`/`tail`/
`uncons` (`Push`/`Peek`/`Pop`/`PeekPop`). Computation expression instances: `flatList`, `indexedList`, `queue`, `stack`,
`hashMap`, `sortedMap`, `hashSet`, `sortedSet`, `frozenMap`, `frozenSet`, and `equatableArray` of ADR 0002.

### D3. Value-type defaults and the `Option`/`Tuple` override namespaces

* Every `try*` function returns `voption`; `choose`, `pick`, `tryPick`, `unfold` and `change` take or return `voption`.
* Every tuple in a signature is a struct tuple: `zip`, `zip3`, `unzip`, `unzip3`, `pairwise`, `indexed`, `allPairs`,
  `groupBy`, `countBy`, `partition`, `partitionWith`, `splitAt`, `mapFold`, `mapFoldBack`, `unfold`, `uncons`, map
  `ofSeq`/`toSeq`/`addRange`/`minKeyValue`/`maxKeyValue`, and so on.
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
coexist with `System.Collections.Generic`.

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
`CheckOrder` before `DotnetBuild`.

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
semantics); a faster candidate that breaks the contract is skipped. Sorting is the main case. Stability follows the
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
`IndexedList` ranges use `GetRange` (O(log n)). `Stack` reverses only in its constructors: the BCL `ImmutableStack.Create`
and `CreateRange` push in order and leave the last element on top, so `ofSeq`, `ofArray`, `ofList`, `ofReadOnlySpan` and
`ofSpan` push from the end instead (`ofSeq [1; 2; 3]` has `1` on top), and every transformation snapshots top-to-bottom,
applies `Array.*` and rebuilds the same way. `Queue.length`/`Stack.length` are O(n) and say so; map and set transformations preserve the comparers of the
input (`CreateRange(keyComparer, valueComparer, …)`, `CreateBuilder(keyComparer)`); interface modules preserve the caller's
implementation through `Clear()` plus `AddRange`/`Enqueue`/`Push`; `FrozenMap.keys`/`values` and `FrozenSet.toFlatList`
return the underlying `ImmutableArray` in O(1).

### D6. Exceptions mirror FSharp.Core

Exception types and message texts follow FSharp.Core (`FSCore.resx`): `ArgumentException` for an empty input to `head`,
`tail`, `reduce`, `exactlyOne`, `average`, `min`, `max`, `randomChoice*`, for non-positive counts, length mismatches
(`zip`, `map2`, …), out-of-range indices and invalid permutations; `InvalidOperationException` when `take`, `takeLast` or
`randomSample` find too few elements; `KeyNotFoundException` for `find*`, `findIndex*` and `pick`; `ArgumentNullException`
for a null input and for a default `FlatList` (the array-null analogue; the BCL itself throws on a default array).

### D7. Computation expressions

One `<Name>CE` class per collection with the delayed-function design (`Buffer<'T> -> unit`), members `Zero`, `Yield`,
`YieldFrom` (sequence, array, same collection, buffer), `Combine`, `Delay`, `For`, `While`, `Run`, `TryWith`, `TryFinally`,
`Using`. Buffers are the BCL builders where they exist (`ImmutableArray.Builder` with `DrainToImmutable`,
`ImmutableList.Builder`, dictionary and set builders) and a `ResizeArray` otherwise. Map expressions accept
`yield struct (key, value)` and `yield KeyValuePair`; map and set expression classes take an optional comparer. The
duplicate `FlatListBuilderCE` is dropped.

### D8. Tests: shared contract suites

Tests are rewritten on MSTest 4 with the Microsoft.Testing.Platform runner. Each capability is an abstract generic base
class without `[<TestClass>]` (MSTEST0085/MSTEST0002) that takes an immutable record of the module's functions; every
collection derives one closed `[<TestClass>]` per capability it supports, so inapplicable tests do not exist rather than
being skipped. Facets: `SeqLike`, `SeqCreation`, `Indexed`, `StableSort` (every sequence-like module except `FlatList`,
which sorts unstably, see D5); `Map`, `MapTransform`, `MapMutation`, `MapCreation`; `Set`, `SetTransform`, `SetMutation`,
`SetCreation`. Expectations are computed with FSharp.Core `List` over the collection's
observed enumeration order, so one test body serves FIFO, LIFO and indexed collections. Collection-specific behaviour
(`isDefault`, spans, builders, `Stack.cons` direction, comparers, sorted-only functions, null inputs) lives in a per-collection
file. Exceptions are asserted with `Assert.ThrowsExactly` on type, `ParamName` and message prefix; every assertion carries
a message; categories come from `TestCategories.fs` attributes (collection level) and `TestCategory (nameof …)`
(operation level), never from string literals. In a contract base, the function of any module gives the name, for
example `nameof FlatList.map`. Dedicated files per collection prove that opening the `Option`/`Tuple` namespaces shadows the default
functions and that the override functions are equivalent to the defaults after conversion.

### D9. Delivery: one pull request per collection, stacked

`chore/foundation` (helper module, order check script, test categories and contract bases, changelog and documentation
skeleton, target frameworks), then `feat/flat-list`, `feat/flat-list-parallel` (the nested `Parallel` module of D11),
`feat/equatable-array` (step 2 of ADR 0002: the mirror of the `FlatList` module, its builder functions and
`equatableArray { }`), `feat/indexed-list`, `feat/queue`, `feat/stack`, `feat/hash-map` (with `IMap`), `feat/sorted-map`, `feat/hash-set` (with
`ISet`), `feat/sorted-set`, `feat/frozen-map`, `feat/frozen-set` (both with the alternate lookup of D11), and an optional
`chore/benchmarks` (BenchmarkDotNet, excluded from the solution filter). Each pull request deletes the legacy file
it replaces, keeps the build green and updates the changelog and the documentation table. No release is cut mid-chain.
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
and later assets of the package. They return the BCL `AlternateLookup<'Alternate>` struct, whose `ContainsKey`, `TryGetValue`
and indexer (map) or `Contains` and `TryGetValue` (set) do the lookups; no `findAlternate`-style wrappers over those members
are added. The comparer must implement `IAlternateEqualityComparer<'Alternate, 'Key>`; `tryAlternateLookup` returns
`ValueNone` otherwise. The tests for these functions compile only for `net10.0`.

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

## Consequences

Breaking changes (major version): `try*` return `voption`; tuples become struct tuples; `choose`/`pick`/`tryPick`/`unfold`/
`change` callbacks use `voption`; `ImmutableList` module becomes `IndexedList`; `Seq`, `IndexedSeq`/`ReadOnlyList`, the
`ISet<'T>` abbreviation and the extras listed in D1 are removed; map and set `length` becomes `count`; map `filter`/`map`
follow FSharp.Core semantics; `Stack` element order is defined (first element of `ofSeq` on top); `take`/`skip` throw on
short input; `netstandard2.0` is not a target. The `Option` and `Tuple` namespaces give consumers a one-line migration path.

Benefits: one vocabulary across ten modules, consistent with FSharp.Core 10.0.300; defects of the current map and set modules
fixed by construction (comparer preservation, real `singleton`); fewer allocations (`voption`, struct tuples, span and memory views, zero-copy
bridge); `Frozen*` support; tests that cannot drift between collections; machine-checked function order.

Costs: about 150 sequence functions × 7 modules plus maps, sets and frozen collections to implement and document; the
override namespaces double the number of signature files; contributors must learn the category order (enforced by the
script, so mistakes fail fast).

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
| `rev` | | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | |
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
| `emptyWithComparers` / `emptyWithKeyComparer` | | ᵇ | ᵇ | – | – | |
| `ofArray` / `ofList` / `ofSeq` | Ⓣ | ✓ | ✓ | – | ✓ | `seq<struct ('Key * 'Value)>` |
| `ofKeyValuePairs` / `ofKeyValuePairsWithKeyComparer` | | ᵇ | ᵇ | – | ᵇ | `seq<KeyValuePair>` |
| `ofSeqBy` | | ᵇ | ᵇ | – | ᵇ | key selector |
| `ofSeqWithKeyComparer` | Ⓣ | ᵇ | ᵇ | – | ᵇ | |
| `singleton` | | ✓ | ✓ | – | ✓ | curried `key value` |
| **Builder** | | | | | | |
| `builder` / `builderWithComparers` / `builderWithKeyComparer` / `ofBuilder` / `toBuilder` | | ᵇ | ᵇ | – | – | |
| **Properties** | | | | | | |
| `count` / `isEmpty` | | ✓ | ✓ | ✓ | ✓ | |
| `keyComparer` / `valueComparer` | | ᵇ | ᵇ | – | ᵇ/– | |
| `keys` / `values` | | ✓ | ✓ | ✓ | ✓ | Frozen → `FlatList` O(1) |
| **Access** | | | | | | |
| `find` | | ✓ | ✓ | ✓ | ✓ | `KeyNotFoundException` |
| `maxKeyValue` / `minKeyValue` | Ⓣ | – | ✓ | – | – | sorted only |
| `tryFind` | Ⓥ | ✓ | ✓ | ✓ | ✓ | |
| `tryGetKey` | Ⓥ | ᵇ | ᵇ | ᵇ | – | |
| **Search** | | | | | | |
| `containsKey` / `containsValue` | | ✓/ᵇ | ✓/ᵇ | ✓/– | ✓/ᵃ | |
| `exists` / `forall` | | ✓ | ✓ | ✓ | ✓ | |
| `findKey` / `tryFindKey` | Ⓥ | ✓ | ✓ | ✓ | ✓ | |
| `pick` / `tryPick` | Ⓥ | ✓ | ✓ | ✓ | ✓ | |
| **Transformation** | | | | | | |
| `add` / `addRange` | Ⓣ (`addRange`) | ✓/ᵇ | ✓/ᵇ | ✓/ᵇ | – | |
| `change` | Ⓥ | ✓ | ✓ | ✓ | – | |
| `choose` | Ⓥ | ᵃ | ᵃ | ᵃ | ᵃ | Frozen rebuild |
| `clear` | | ᵇ | ᵇ | ᵇ | – | keeps comparers |
| `filter` | | ✓ | ✓ | ✓ | ✓ | returns a map |
| `map` | | ✓ | ✓ | ✓ | ✓ | keys kept |
| `remove` / `removeRange` | | ✓/ᵇ | ✓/ᵇ | ✓/ᵇ | – | |
| `setItem` / `setItems` | | ᵇ | ᵇ | ᵇ | – | add or replace |
| `withComparers` / `withKeyComparer` | | ᵇ | ᵇ | – | – | |
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
`FrozenMap`, `net10.0` only). `Tuple.*`: `addRange`, `ofArray`,
`ofList`, `ofSeq`, `ofSeqWithKeyComparer`, `partition`, `toArray`, `toList`, `toSeq` (+ `maxKeyValue`, `minKeyValue` for
`SortedMap`).

## Appendix C – sets

`HashSet` (`ImmutableHashSet`), `SortedSet` (`ImmutableSortedSet`), `ISet` (`IImmutableSet`), `FrozenSet` (`FrozenSet`).

| Function | Shape | HashSet | SortedSet | ISet | FrozenSet | Note |
|---|---|---|---|---|---|---|
| **Creation** | | | | | | |
| `empty` / `emptyWithComparer` | | ✓/ᵇ | ✓/ᵇ | – | ✓/– | |
| `ofArray` / `ofList` / `ofSeq` | | ✓ | ✓ | – | ✓ | |
| `ofSeqWithComparer` | | ᵇ | ᵇ | – | ᵇ | |
| `singleton` | | ✓ | ✓ | – | ✓ | |
| **Builder** | | | | | | |
| `builder` / `builderWithComparer` / `ofBuilder` / `toBuilder` | | ᵇ | ᵇ | – | – | |
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
| `withComparer` | | ᵇ | ᵇ | – | – | |
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
