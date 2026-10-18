---
title: ADR 0002 – EquatableArray
category: Explanations
categoryindex: 3
index: 3
---

# ADR 0002: `EquatableArray<'T>` – an immutable array with value semantics

| | |
|---|---|
| Status | Accepted (2026-10-18) |
| Date | 2026-10-11 |
| Deciders | maintainers |

Facts marked *(verified)* were run for this document with F# 10 on .NET SDK 10.0.401. Semantics were checked in
`dotnet fsi`. Costs were measured on a prototype – a throwaway implementation of this design, kept outside the
repository – compiled in Release for `net8.0` and run on `net10.0`. Allocated bytes are exact; times are stopwatch
readings and show the order of magnitude only.

The maintainers accepted this design and the recommendations of §11 on 2026-10-18. Step 1 of §6 is
[pull request #28](https://github.com/fsprojects/FSharp.Collections.Immutable/pull/28), and its tests (§7) pin the
verified facts. ADR 0001 – Per-collection modules (`ADR_0001_Collection_Modules.md`) proposes to rewrite the
collection modules and is still proposed, in
[pull request #23](https://github.com/fsprojects/FSharp.Collections.Immutable/pull/23); §5 to §7 say where the two
meet.

## Context

`FlatList<'T>` is a type abbreviation of `System.Collections.Immutable.ImmutableArray<'T>`, and the module `FlatList`
(compiled name `ImmutableArrayModule`) is its API. No type of this library has an equality of its own, so a `FlatList`
is compared the way `ImmutableArray` is *(verified)*:

* **Equality depends on who asks.** F# `=`, `hash` and `HashIdentity.Structural` compare the elements. `.Equals`,
  `EqualityComparer<_>.Default`, a default `Dictionary` or `HashSet` and `ValueTuple.Equals` compare the reference of
  the underlying array, so two arrays with equal elements are two different keys. As a field of an F# record or union
  the array is compared by its elements under every comparer, because generated equality goes through
  `IStructuralEquatable`.
* **F# equality on it allocates.** One `a = b` on 32 elements costs 1584 B for `int`, 48 B for `string` or an F# union
  and 2096 B for struct tuples or struct records.
* **Its structural hash reads the last 8 elements only.**
* **Comparison throws.** The type satisfies the `comparison` constraint, but `compare`, `<`, `max`, `List.sort` and
  `Set.ofList` throw on two arrays of different length.
* **The `equality` constraint holds for every element type**: `=` on an `ImmutableArray<int -> int>` compiles.
* **The default value is not the empty array.** `Unchecked.defaultof`, `Array.zeroCreate` and a missing JSON member
  produce it. `default = empty` is `false`, `.Length` and `for` throw `NullReferenceException`, `%A` fails, and
  System.Text.Json cannot write a record that holds one.
* **F# conveniences are missing.** `%A` prints `seq [0; 1; 2; 3; ...]` (the first four elements), `xs[1..2]` does not
  compile, and list patterns do not apply.

.NET leaves this to libraries: [dotnet/runtime#118189](https://github.com/dotnet/runtime/issues/118189) asks for a
collection with value equality and is open with milestone Future. On 2026-09-20 its answers were that "it's trivial to
create a no-cost wrapper around ImmutableArray that has sequence-equality semantics" and that the .NET libraries are
"not going to be adding dozens of extra collection types".

### Use cases

1. **Syntax trees and other models.** Unions and records hold child arrays, also arrays of struct tuples. Generated
   equality drives round-trip tests (`parse (print tree) = tree`), cache keys and checks that a rewriting pass is
   idempotent. This needs element-wise equality that allocates nothing and a hash over all elements. The first consumer
   is the SQL syntax tree of FSharp.Azure.Cosmos, which carries its own wrapper today.
2. **Keys and members in .NET code**: a default `Dictionary` or `HashSet`, struct tuples, C# records.
3. **`Set` and `Map` keys, sorting.**
4. **Persisted documents**: a round trip through System.Text.Json and FSharp.SystemTextJson without registering
   anything, and no broken value from a missing member.
5. **Pattern matching** on arrays of a few elements without converting to an F# list.
6. **Free passage** to and from `ImmutableArray` and `FlatList` APIs.
7. **Size of one reference.** The library targets `net8.0`; consumers run on `net10.0`.

## Decision

1. The library gets `EquatableArray<'T>`, a struct with exactly one field of type `ImmutableArray<'T>` (§1). The name is
   the one the first consumer's wrapper has, so that consumer changes an `open`, not its code (maintainer's decision).
   `FlatList<'T>` stays the plain abbreviation.
2. The default value is the empty array, and the default state of `ImmutableArray` is never observable (§2).
3. Equality and hashing cover all elements and compare them with `EqualityComparer<'T>.Default` (§3, question 1). C#
   `==` and `!=` use the same equality (§1).
4. Comparison is by length first, then by the first differing element, in the order of F# generic comparison (§3,
   question 2).
5. Equality and comparison agree for every element type on which `EqualityComparer<'T>.Default` and F# comparison
   agree. For elements that are F# arrays, `FlatList`s or tuples that hold them, they do not: `Equals` compares such
   elements by reference, `CompareTo` by contents. This is accepted. A default `HashSet` can then hold two values where
   a `Set` holds one (§2, question 8).
6. The type satisfies `equality` and `comparison` only when its element type does.
7. Conversion to and from `ImmutableArray<'T>` is O(1). C# collection expressions and JSON work without registration
   (§4).
8. The module `EquatableArray`, compiled as `EquatableArrayModule`, mirrors the `FlatList` module. Construction reuses
   the builder of `FlatList`, and there are a computation expression and struct active patterns (§5).
9. No language syntax is assumed.

## Consequences

* The library gets its first type that is not an abbreviation, and a use of System.Text.Json, which is part of the
  shared framework of `net8.0`: no package reference is added.
* There are two array types. `EquatableArray` is for fields of records and unions, for keys and for persisted documents.
  `FlatList` is for local computation and for signatures shared with .NET code. Moving between them costs nothing.
* Its equality differs from that of F# lists in two visible points: `NaN` elements, and elements that are arrays (§2).
  For elements that are arrays, hash-based and sorted collections also disagree on which values are one key (§2).
* Consumers need a new release: nuget.org has only version 1.0.0 from 2020.

## 1. The type

The library compiles against the signature below for `net8.0`, with nullness checking on and without warnings (step 1,
pull request #28). The prototype did too, except for `Slice`, which a separate probe verified.

```fsharp
namespace FSharp.Collections.Immutable

[<Struct; CustomEquality; CustomComparison>]
[<CollectionBuilder(typeof<EquatableArray>, "Create")>]
[<JsonConverter(typeof<EquatableArrayJsonConverterFactory>)>]
[<StructuredFormatDisplay "{StructuredDisplayText}">]
type EquatableArray<[<EqualityConditionalOn; ComparisonConditionalOn>] 'T> =
    val private array : ImmutableArray<'T>        // the only field; a struct must show its fields in a signature (FS0314)
    new : items : ImmutableArray<'T> -> EquatableArray<'T>                       // O(1), no copy
    member Items : ImmutableArray<'T>                                            // O(1), never a default instance
    member Length : int
    member IsEmpty : bool
    member Item : index : int -> 'T with get
    member GetEnumerator : unit -> ImmutableArray<'T>.Enumerator                 // a struct: `for` allocates nothing
    member AsSpan : unit -> ReadOnlySpan<'T>
    member AsMemory : unit -> ReadOnlyMemory<'T>
    member GetSlice : start : int option * finish : int option -> EquatableArray<'T>   // F# xs[a..b]
    member Slice : start : int * length : int -> EquatableArray<'T>                 // C# xs[a..b]
    member Equals : other : EquatableArray<'T> -> bool
    member CompareTo : other : EquatableArray<'T> -> int
    static member op_Equality : left : EquatableArray<'T> * right : EquatableArray<'T> -> bool     // C# ==
    static member op_Inequality : left : EquatableArray<'T> * right : EquatableArray<'T> -> bool   // C# !=
    static member op_Implicit : source : EquatableArray<'T> -> ImmutableArray<'T>                  // question 3
    static member op_Implicit : source : ImmutableArray<'T> -> EquatableArray<'T>
    interface IEquatable<EquatableArray<'T>>
    interface IComparable<EquatableArray<'T>>
    interface IComparable
    interface IReadOnlyList<'T>
    interface IList<'T>                                                          // read-only, question 5

/// The factory that collection expressions call. It has exactly one method.
and [<AbstractClass; Sealed>] EquatableArray =
    static member Create : items : ReadOnlySpan<'T> -> EquatableArray<'T>

and [<Sealed>] EquatableArrayJsonConverterFactory =
    inherit JsonConverterFactory
    new : unit -> EquatableArrayJsonConverterFactory
```

* Every member treats a field in the default state as `ImmutableArray<'T>.Empty`.
* `Items` and the constructor keep the member names of the first consumer's wrapper.
* `GetSlice` takes `option` arguments because the compiler requires them for slicing; the repository's preference for
  `voption` cannot apply. A slice that covers the whole array returns the same array, any other slice copies.
* `Slice` gives C# its range syntax *(verified)*: `xs[1..^1]` calls `Slice` with a start and a length, and `xs[^1]` uses
  `Length` and the indexer. `Slice` checks its arguments as `ImmutableArray.Slice` does, so an out-of-range range throws
  `ArgumentOutOfRangeException`, and it shares or copies as `GetSlice` does. Under
  [RFC FS-1351](https://github.com/fsharp/fslang-design/pull/849), F# slicing also uses `Slice(int, int)` before
  `GetSlice`, with tolerant bounds, so both members give the same F# results.
* `op_Equality` and `op_Inequality` give C# `==` and `!=` with the equality of `Equals`. F# 10 accepts them on the type
  without a warning *(verified)* and never calls them: F# `=` uses the generated equality.
* `IStructuralEquatable` and `IStructuralComparable` are deliberately absent: with them F# `=` boxes the value and every
  element again (§3).

## 2. Semantics

All rows are *(verified)* on the prototype.

| | `EquatableArray<'T>` | `FlatList<'T>` |
|---|---|---|
| `=`, `hash`, a field of a record or union | elements | elements |
| `.Equals`, `EqualityComparer<_>.Default`, default `Dictionary` and `HashSet`, `ValueTuple.Equals`, C# `==` | elements | reference |
| Hash | all elements | the last 8 elements |
| `compare`, `<`, `max`, sorting, `Set`, `Map` | length first, then the first differing element | throws when the lengths differ |
| Default value | the empty array: `default = empty`, `Length` is 0, `for` runs, JSON `[]` | `default = empty` is `false`; `Length` and `for` throw |
| `equality`, `comparison` constraints | only when `'T` has them: `=` on an array of functions is error FS0001 | always |
| `%A` and `ToString ()` | the notation of an F# array, with all elements up to the 100 that `%A` shows | `seq [1; 2; 3]`, four elements at most; `ToString ()` gives the type name |
| `xs[1..2]` | the rules of F# array slices: bounds are clamped, an empty range is empty | does not compile |

A value of three elements prints as `[|1; 2; 3|]`. `compare [9] [1; 2]` is -1 as for F# arrays; for F# lists it is 1.

Hash codes come from `System.HashCode`, so they differ between processes, like those of strings. They must not be
persisted.

`%A` is produced through `StructuredFormatDisplay` with a property that returns text. A property that returns a list was
tried: `%A` then prints an `EquatableArray` nested in another one as `...` *(verified)*, which hides the children of a
syntax tree. The price of text is that a value too wide for one line loses its indentation.

**Where it differs from F# lists and arrays.** Elements are compared with `EqualityComparer<'T>.Default`, not with F#
equality:

| | `EquatableArray` | `'T list`, `'T[]` |
|---|---|---|
| `[nan] = [nan]` | `true` | `false` |
| `[nan].Equals [nan]` | `true` | `true` for a list |
| Elements that are F# arrays or `FlatList`s with equal contents, also inside a struct or reference tuple | not equal: such elements compare by reference | equal |
| `[nan] < [1.0]` | `true`: the operators follow `CompareTo` | `false` |

To nest arrays, use `EquatableArray` as the element type: `EquatableArray<EquatableArray<int>>` compares by elements at
both levels.

**Keys.** Equality and comparison agree for every element type on which `EqualityComparer<'T>.Default` and F#
comparison agree. Examples *(verified)* are primitive types (`NaN` included), strings, F# records and unions (also with
array fields), struct tuples of these and `EquatableArray` itself. F# arrays, `FlatList`s and tuples that hold them do
not agree. .NET equality compares their references, and F# comparison compares their contents. Take two
`EquatableArray<int[]>` values whose inner arrays have equal contents but are allocated separately *(verified)*:

| Caller | Result |
|---|---|
| `Equals`, `=` | `false` |
| `CompareTo`, `compare` | 0 |
| A default `HashSet`, `Dictionary` or `ImmutableHashSet`, `HashIdentity.Structural`, `List.distinct` | two keys |
| `Set`, `Map`, `SortedSet`, `ImmutableSortedSet` | one key |

`FlatList` elements and struct or reference tuples of arrays give the same results. `FlatList` elements of different
lengths make `CompareTo` throw `ArgumentException`, as `compare` on two such `FlatList`s does. This is accepted
(decision 5, question 8). Comparison cannot follow a reference, because a reference has no stable order. Equality keeps
the .NET equality of the element. With `EquatableArray` as the element type, the two agree.

## 3. Measured costs and the choice of element comparison

Allocated bytes per operation on 32 elements *(verified)*:

| Elements | `=` `FlatList` | `=` `'T list` | `=` `EquatableArray` | `hash` `FlatList` | `hash` `EquatableArray` | `compare` `FlatList` | `compare` `EquatableArray` | `CompareTo` `EquatableArray` |
|---|---:|---:|---:|---:|---:|---:|---:|---:|
| `int` | 1584 | 0 | 0 | 216 | 0 | 1584 | 48 | 0 |
| `string` | 48 | 0 | 0 | 24 | 0 | 48 | 48 | 0 |
| F# union | 48 | 0 | 0 | 24 | 0 | 48 | 48 | 0 |
| struct tuple | 2096 | 2048 | 0 | 280 | 0 | 2096 | 2096 | 2048 |
| F# struct record | 2096 | 2048 | 0 | 280 | 0 | 2096 | 2096 | 2048 |

* As a field of a record or union: `=` allocates nothing, `hash` allocates 24 B per field (generated hashing boxes the
  field), `compare` costs what it costs on the value itself.
* A union node with 32 children compares in about 0.6 µs without allocation; with a `FlatList` of children it takes
  about 1.1 µs and 48 B.
* `for`, the active patterns and wrapping or unwrapping an `ImmutableArray` allocate nothing.
  `equatableArray { 1; 2; 3 }` allocates 128 B, `EquatableArray.ofArray [| 1; 2; 3 |]` 80 B.
* Use as a `seq<'T>` boxes the value (24 B), like any struct.

**Element equality.** Three ways for a library type to compare its elements, in bytes for one `=` on 32 elements:

| Elements | D: `EqualityComparer<'T>.Default` | ER: `LanguagePrimitives.GenericEqualityER` | S: D plus `IStructuralEquatable` |
|---|---:|---:|---:|
| `int` | 0 | 0 | 1584 |
| `string`, F# union | 0 | 0 | 48 |
| struct tuple, F# struct record | 0 | 2048 | 2096 |

Only S has the rules of F# lists, where `=` is a partial equivalence (`nan = nan` is `false`) while `.Equals` is an
equivalence, and it boxes like `ImmutableArray` itself. ER needs the constraint `'T : equality` on the type parameter,
which a conditionally constrained parameter does not have: it does not compile there. `Unchecked.equals` needs no
constraint, but it boxes struct tuples (2048 B) and makes `.Equals` itself answer `false` for `NaN`, which breaks
dictionary keys. The compiler-only `GenericEqualityWithComparerIntrinsic` brings S down to 48 B for `int` and raises
warning FS1204 outside FSharp.Core. So the type uses D, the only strategy that does not allocate for struct elements –
where even `'T list` allocates 2048 B.

D has one limit: `EqualityComparer<'T>.Default` boxes a struct element that does not implement `IEquatable<'T>`. 32
elements of a struct that only overrides `Equals (obj)` cost 768 B per comparison. Primitive types, strings, enums,
`Guid`, `DateTime`, `Nullable`, struct tuples, `voption` and F# records and unions with generated equality are free.

The prototype compares through `MemoryExtensions.SequenceEqual` over the two spans, which has the semantics of D. Two
values over the same underlying array are equal without a scan, which is sound because D is reflexive.

**Element comparison.** The first draft of this design compared elements with `Comparer<'T>.Default`. That is not
workable *(verified)*:

* `Comparer<string>.Default` compares with the current culture. It puts `"a"` before `"B"`, where F# `compare` does the
  opposite, and it orders `"ı"` and `"i"` one way under the invariant culture and the other way under `tr-TR`. A `Set`
  or `Map` keyed by such arrays would be ordered by the culture of the thread that built it.
* `Comparer<struct (string * int)>.Default` has the same defect, and `Comparer<int[]>.Default` throws.

Elements are therefore compared in the order of F# generic comparison, through a comparer chosen once per element type:
`StringComparer.Ordinal` for `string`; `Comparer<'T>.Default` for the primitive types and `decimal`, where it agrees
with F# `compare` on every edge value tried (`NaN`, signed zero, the extremes); `Unchecked.compare` for everything else,
because it needs no `comparison` constraint, which a conditional constraint cannot supply. For F# arrays, `FlatList`s
and tuples that hold them, this order compares contents, but equality compares references. §2 (Keys) shows the effect
on keys.

| Elements through | `int` | `string` | struct tuple | Order |
|---|---:|---:|---:|---|
| `Comparer<'T>.Default` | 0 | 0 | 0 | strings by culture; throws for arrays |
| `Unchecked.compare` only | 1536 | 0 | 2048 | F# |
| The comparer per element type (decision) | 0 | 0 | 2048 | F# |

(Bytes of one `CompareTo` on 32 elements.) The 48 B of F# `compare` in the first table are two boxes of 24 B: F# generic
comparison boxes both values. .NET sorting uses `IComparable<'T>` and avoids them. Sorting 1000 arrays of 4 `int`
allocates 501 kB with `Array.sort` and nothing with `Array.Sort`. `'T list` is not cheaper: `compare` on an `int list`
of 32 allocates 1536 B.

## 4. Interop

**`ImmutableArray` and `FlatList`.** The constructor and `Items`, or `EquatableArray.ofImmutableArray` and
`toImmutableArray`, convert in O(1) without a copy. `ofFlatList` and `toFlatList` are not added: `FlatList<'T>` is
`ImmutableArray<'T>`, so they would be the same two functions under a second name.

**`op_Implicit` in both directions** (question 3) *(verified)*: at a method argument the conversion is silent in both
directions. In an annotated binding, an argument of a let-bound function, a record field, an `if` branch and a list
element it works with warning FS3391, which is on by default. An argument that is itself an overloaded call, such as
`Api.Take (ImmutableArray.Create (1, 2))` for a parameter of type `EquatableArray<int>`, is error FS0193.

**C#** *(verified with a C# program against the prototype)*: `EquatableArray<int> xs = [1, 2, 3];`, the empty expression
and spreads compile through `CollectionBuilderAttribute`; `foreach` uses the struct enumerator; `xs[1..^1]` and `xs[^1]`
slice and index through `Slice`, `Length` and the indexer; `==`, `!=`, a default `HashSet`, `Dictionary` and `SortedSet`,
and a `record struct` that holds the type all compare by elements; `default` is the empty array.

**F# collection expressions.** The draft of [RFC FS-1342](https://github.com/fsharp/fslang-design/pull/838) gives
target-typed `[ ... ]` to types with `CollectionBuilderAttribute` and requires the builder type to be non-generic with
exactly one such static method. The factory class `EquatableArray` satisfies that, so it must never get a second
`Create`.

**JSON.** System.Text.Json writes any `IEnumerable<'T>` as an array but cannot read this type back
(`NotSupportedException`), with or without the builder attribute. The type therefore names
`EquatableArrayJsonConverterFactory` in a `JsonConverterAttribute`. The converter reads and writes through the
serializer's own handling of `ImmutableArray<'T>`, so element converters and naming policies apply. Measured
*(verified)*:

| | plain System.Text.Json | FSharp.SystemTextJson (defaults; skippable option fields) |
|---|---|---|
| Round trip as a value, a record or class member, a dictionary value and nested in itself | yes | yes, also with unions and struct tuples as elements |
| A default value is written as | `[]` | `[]` |
| A missing member of a record | the empty array | `JsonException` from its record converter, as for every non-optional field |
| A `null` member of a record | the empty array (question 6) | `JsonException` from its record converter |

The JSON is the array that `ImmutableArray` produces, so changing a document member from `FlatList` to `EquatableArray`
does not change stored documents. The factory is public, so that consumer code can also name it in serializer options.
The converter for one element type is private and created through `MakeGenericType`.

## 5. Module, construction and patterns

**Module.** `EquatableArray` is `[<RequireQualifiedAccess>]` and carries
`[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]`, so its compiled name is
`EquatableArrayModule`:

* The factory class `EquatableArray` already has the plain name.
* The attribute is written out because the compiler adds the suffix by itself only next to a non-generic type of the
  same name. Next to a generic type alone the module compiles under the plain name *(verified)*, so without the
  attribute the compiled name would depend on the factory class.
* The name has the form of `ImmutableArrayModule`. A query translator that recognises the functions of this library by
  their compiled declaring type can tell the two modules apart.

Every function of the `FlatList` module exists in `EquatableArray` with the same name, argument order and behaviour and
with `EquatableArray<'T>` in place of `FlatList<'T>`. Each one unwraps, calls the `FlatList` function and wraps, so no
body is written twice. What differs:

* `isDefault` and `isDefaultOrEmpty` are not mirrored, and no function rejects a default argument: it is an empty array.
* `ofImmutableArray` and `toImmutableArray` are added.
* `unsafeOfArray` is added, as in the first consumer: it wraps an array without copying it, through
  `ImmutableCollectionsMarshal.AsImmutableArray`, for a caller that gives the array away and never writes to it again.
  `ofArray` copies. Both throw `ArgumentNullException` for `null`, as FSharp.Core does; the first consumer's functions
  return an empty array.
* Results that nest arrays (`groupBy`, `chunkBySize`, `windowed`, `splitInto`, `transpose`) hold `EquatableArray`s
  inside, otherwise the inner arrays would compare by reference (§2).
* Functions that compare elements (`contains`, `distinct`, `indexOf`, …) keep the F# equality they have in `FlatList`.
  Only the equality of the array itself uses `EqualityComparer<'T>.Default`.

The mirror follows the `FlatList` module of the day. If ADR 0001 is accepted, that means its function set and order,
`voption` and struct tuples, and its `Option` and `Tuple` override modules, each mirrored by delegation.

**Builder.** The builder is `ImmutableArray<'T>.Builder`, the builder of `FlatList`, so every function of the
`FlatListBuilder` module (on the `flat-list` branch) applies as it is. There is no second builder module: it would
repeat `FlatListBuilder` function for function. The module has `builder`, `builderWith`, `ofBuilder`, `moveFromBuilder`,
`toBuilder`, `build` and `update` by the mirror rule.

**Computation expression.** `equatableArray { ... }`, defined next to `flatList` in `ComputationExpressions.fs`. Its
builder type `EquatableArrayCE` differs from `FlatListCE` only in `Run`, so the members that both expressions share move
to a common base type, and `EquatableArrayCE` adds a `YieldFrom` for `EquatableArray<'T>` that does not box.

**Patterns.** Struct partial active patterns in the auto-opened module `EquatableArrayPatterns`, named as in the first
consumer:

```fsharp
val (|Arr0|_|) : array : EquatableArray<'T> -> bool
[<return : Struct>] val (|Arr1|_|) : array : EquatableArray<'T> -> 'T voption
[<return : Struct>] val (|Arr2|_|) : array : EquatableArray<'T> -> struct ('T * 'T) voption
[<return : Struct>] val (|Arr3|_|) : array : EquatableArray<'T> -> struct ('T * 'T * 'T) voption
val (|ArrN|_|) : length : int -> array : EquatableArray<'T> -> bool

match expression with
| Call ("add", Arr2 (Literal a, Literal b)) -> a + b      // nested in a union case
| Call (_, Arr0) -> 0
| Call (_, ArrN 4) -> 4
| _ -> -1
```

A match allocates nothing *(verified)*. Each length is its own partial pattern because a multi-case pattern returns a
heap-allocated `Choice`, 24 B per match *(verified)*. The patterns take `EquatableArray<'T>` because a pattern nested in
a union case cannot convert its input; a `FlatList` is matched after an O(1) wrap, `match EquatableArray items with`
(question 4).

## 6. Implementation plan

```text
src/FSharp.Collections.Immutable/
├── EquatableArray/
│   ├── EquatableArray.fsi / EquatableArray.fs                   – the type, the factory class, the JSON converter factory, the module
│   ├── EquatableArrayPatterns.fsi / EquatableArrayPatterns.fs   – Arr0 … ArrN
│   └── EquatableArrayCE.fs                                      – the builder type of equatableArray { }
└── ComputationExpressions.fs                                    – adds the value equatableArray
tests/FSharp.Collections.Immutable.Tests/
├── TestCategories.fs                                            – adds the category attributes of §7
└── EquatableArray/                                              – one test file per operation group
```

* **One file for the type and its converter.** The attribute on the type names the converter factory, and the factory
  names the type. F# resolves that only inside one recursive group of types, so the factory cannot be a file of its own.
* **Compile order**: after the `FlatList` files, which the module calls; `EquatableArrayCE.fs` after `FlatListCE.fs`.
  With step 2, the folder also gets `EquatableArrayOption.fsi/.fs` and `EquatableArrayTuple.fsi/.fs` right after
  `EquatableArray.fs`, the override modules of ADR 0001 D3, in the compile order of ADR 0001 D2.
* **Nullness**: the converter calls the non-generic `JsonSerializer.Deserialize`, because the generic overload on a
  struct result gives warning FS3265.
* **Public API** has XML documentation under the repository's rules; `CHANGELOG.md` gets an entry under Unreleased, and
  the collection lists of `README.md`, `docsSrc/index.md` and `docsSrc/Explanations/Background.md` get the type.
* **Target framework**: the type needs `net8.0`. `CollectionBuilderAttribute` and `System.HashCode` do not exist on
  `netstandard2.0`, and System.Text.Json would become a package dependency there. ADR 0001 proposes `net8.0;net10.0`,
  which fits. The older local branches (`flat-list` to `indexed-list`) add `netstandard2.0`; should that target ever
  come, the type is compiled for `net8.0` and later only.

**Two steps** (question 7). On `origin/main` the `FlatList` module is one file without a signature. The folder layout,
the full function set, the builder module and the computation expressions exist only on unmerged local branches, and
ADR 0001 proposes to rewrite them in a chain of pull requests with no release before its end.

1. *On `main` now* (pull request #28): the type, the factory class, the converter, the patterns and a module with
   `empty`, `singleton`, `ofSeq`, `ofArray`, `ofList`, `ofImmutableArray`, `unsafeOfArray`, `toImmutableArray`,
   `length`, `isEmpty` and `map`, with the tests of §7. This is what the first consumer uses, and a release can follow.
2. *With the `FlatList` rewrite*: the mirror of the module, the builder functions and the computation expression, as one
   more pull request of that chain and under its rules for layout, function order and tests.

## 7. Test plan

Under the repository's rules: MSTest 4, a failure message on every assert, categories from `TestCategories.fs`,
attributes sorted by type name.

* **Categories.** The `TestCategoryBaseAttribute` descendants of `TestCategories.fs`, on the test classes only, as in
  ADR 0001 D8: `EquatableArrayTestCategory` on every test class, and the area that the class examines –
  `AllocationTestCategory`, `ComparisonTestCategory`, `DefaultValueTestCategory`, `EqualityTestCategory`,
  `FormattingTestCategory`, `InteropTestCategory`, `JsonTestCategory`, `ModuleFunctionsTestCategory` or
  `PatternMatchingTestCategory`. Categories are never strings, and test methods carry none. Step 1 adds
  `TestCategories.fs`; `BuilderTestCategory` and `ComputationExpressionTestCategory` come with step 2.
* **Equality under every caller**: `=`, `.Equals`, `EqualityComparer<_>.Default`, `HashIdentity.Structural`, a default
  `Dictionary` and `HashSet`, `ValueTuple.Equals`, a record field, a recursive union.
* **The defects of `ImmutableArray` as regression tests**: arrays of different length compare, sort and build a `Set`;
  the default value equals the empty one, has length 0, enumerates, formats and serialises; two arrays of 40 elements
  that differ only in the first one hash differently; separately built equal arrays are one dictionary key.
* **The deliberate differences of §2** (`NaN`, array elements, `<` with `NaN`) are pinned, so that changing them is a
  decision.
* **Keys** (§2, decision 5), as regression tests. The elements are `int[]`, `FlatList<int>` and struct and reference
  tuples of `int[]`. Two values whose inner arrays have equal contents but are allocated separately are not equal and
  compare as 0. A default `HashSet` and `Dictionary` hold them as two keys, `Set`, `Map` and `SortedSet` as one.
  `CompareTo` on `FlatList` elements of different lengths throws `ArgumentException`. For
  `EquatableArray<EquatableArray<int>>` and for a record with an array field as the element, every caller sees one key.
* **Comparison**: length first; strings ordinal while the current culture is `tr-TR`; struct tuples of strings; arrays
  as elements.
* **Constraints**: the type parameter carries `EqualityConditionalOn` and `ComparisonConditionalOn`, read by reflection,
  since a test cannot assert a compile error.
* **Allocation**: 0 B, by `GC.GetAllocatedBytesForCurrentThread`, for `=` on arrays of `int`, `string`, a union and
  struct tuples, on the value and as a record field; for `for`; for a pattern match. These tests run in Release only,
  with tiered compilation off so that every method is fully optimized from its first call. A control test shows that
  the counter sees an allocation.
* **Interop**: both conversions keep the underlying array; `CollectionBuilderAttribute` names the factory class and
  `Create`, and the class has exactly one `Create`; `GetSlice` follows F# arrays; `Slice` follows `ImmutableArray.Slice`,
  including `ArgumentOutOfRangeException`; `op_Equality` and `op_Inequality` agree with `Equals`; the mutating members of
  the read-only `IList<'T>` throw `NotSupportedException`.
* **JSON**: the table of §4 under plain System.Text.Json and under FSharp.SystemTextJson, which becomes a test-only
  package reference in `Directory.Packages.props`.
* **Mirrored functions** (step 2 of §6): through the shared contract suites that ADR 0001 proposes, if it is accepted,
  so that no `FlatList` test is copied.
* **Assertions**: whole arrays with `Assert.AreEqual` on `EquatableArray` values, whose equality and `ToString ()` make
  the failure readable; sizes with `Assert.HasCount` and `Assert.IsEmpty`. `CollectionAssert` does not apply: it needs
  the non-generic `ICollection`.

The maintainers chose to cover C# in CI. The C# MSTest project `tests/FSharp.Collections.Immutable.CSharp.Tests` checks
what only the C# compiler binds: collection expressions, indices and ranges, `==` and `!=`, the default collections,
record structs, the implicit conversions and the compiled names of the module.

## 8. Benchmarks before release

A BenchmarkDotNet project (`benchmarks/`; the repository has none today), run on `net8.0` and `net10.0`:

* `=`, `hash` and `compare` for `EquatableArray`, `FlatList`, `'T list` and `'T[]`; elements `int`, `string`, an F#
  union, a struct tuple; lengths 0, 4, 32 and 1024; on the value and as a record field.
* A recursive union three levels deep with 4 and 32 children per node.
* `SequenceEqual` against a plain loop, per element type.
* Sorting and `Set.ofArray` of 1000 arrays; `Array.Sort` through `IComparable<'T>`.
* JSON read and write against `ImmutableArray<'T>`.

Release gates: no allocation in the equality rows that are 0 in §3; `=` not slower than on `FlatList` for any element
type; JSON within 10 % of `ImmutableArray<'T>`.

## 9. Consumers and migration

* **The first consumer** removes its own `EquatableArray` after the release that contains the type and adds
  `open FSharp.Collections.Immutable` to the files that use it. The constructor, `Items`, `Length`, `IsEmpty`, the
  indexer, `for` and the module functions it uses keep their names. It will notice four things:
  * `ofArray null` and `unsafeOfArray null` throw `ArgumentNullException` where its own return an empty array.
  * The type has comparison where its own is `NoComparison`, so its records and unions gain comparison.
  * The patterns take `EquatableArray<'T>` where its own take `ImmutableArray<'T>` (question 4). Its own patterns are
    not used in its production code.
  * `ToString ()` of an array wider than one line, or longer than 100 elements, wraps and ends in `...`, as `%A` of an
    F# array does. Short arrays print the same `[|1; 2|]`.
* **`FlatList` users** change nothing. They switch a field to `EquatableArray<'T>` where the value is a key, is compared
  or is persisted; stored JSON stays valid (§4).
* **Query translators** that map collection functions by compiled declaring type add
  `FSharp.Collections.Immutable.EquatableArrayModule` to the family of `ImmutableArrayModule`.

## 10. Relation to the RFCs

Two F# RFC drafts name this type as their prototype. They are alternatives, and the library type is the same under
either.

* **A new RFC draft**, FS-1355 until its pull request confirms the number, adds `EquatableArray<'T>` and the module
  `EquatableArray` to FSharp.Core as a pure library type with the name, the members and the element rules of this
  document. For that RFC this library is the prototype unchanged.
* **The revision of [RFC FS-1094](https://github.com/fsharp/fslang-design/blob/main/RFCs/FS-1094-immarray.md)**, the RFC
  of suggestion [fsharp/fslang-suggestions#619](https://github.com/fsharp/fslang-suggestions/issues/619), keeps the
  FSharp.Core names `ImmArray<'T>`, `'T immarray` and the module `ImmArray` and makes that type the wrapper. For that
  RFC this library is the same design under another name and with .NET element rules.

Shared with the FS-1094 draft: a struct over one `ImmutableArray<'T>` field, the default value as the empty array,
equality and a hash over all elements, comparison by length first, the conditional constraints, array slicing, the
struct enumerator, `op_Implicit` in both directions and `CollectionBuilderAttribute` on a non-generic factory class.

Deliberately different from the FS-1094 draft:

| | This library | FS-1094 draft |
|---|---|---|
| Element rules | `EqualityComparer<'T>.Default` (§3) | those of `'T list`: `=` is a partial equivalence, `.Equals` an equivalence, elements compared structurally |
| `IStructuralEquatable`, `IStructuralComparable` | absent: they bring the boxing back | present; FSharp.Core can forward the comparer to typed comparers, a library cannot (warning FS1204) |
| Visible effects | `[nan] = [nan]` is `true`; array elements compare by reference in `Equals` and by contents in `CompareTo`; `[nan] < [1.0]` is `true` | as for `'T list` |
| Module | mirrors `FlatList` | mirrors `Array` |
| `%A` | the notation of an F# array | `immarray [1; 2; 3]` |
| Library additions | `Items`, the constructor, `AsMemory`, `Slice`, read-only `IList<'T>`, `==` and `!=`, the JSON converter, the patterns, `equatableArray { }` | no `==`, no converter on the type, patterns left to users |

## 11. Questions and decisions

The maintainers accepted every recommendation on 2026-10-18. The questions stay here with the reasons for each answer.

1. **Element equality.** `EqualityComparer<'T>.Default`, or the rules of F# lists through `IStructuralEquatable`?
   *Decided: `EqualityComparer<'T>.Default`.* It is the only strategy that does not allocate for struct elements,
   and allocation-free equality is the first use case. The price is the two differences of §2 and, for elements that
   are arrays, the keys of question 8.
2. **Comparison.** Length first as for F# arrays, or lexicographic as for F# lists? *Decided: length first.* It
   is the rule of F# arrays and of the FS-1094 draft, and arrays of different length are compared without reading an
   element. Independently of that, elements are compared in the order of F# generic comparison and not with
   `Comparer<'T>.Default` as in the first draft of this design (§3); the maintainers confirmed this too.
3. **`op_Implicit` in both directions**, or only the named conversions? *Decided: both.* The conversions are O(1)
   and lose nothing, a default array becomes empty, and F# warns wherever the conversion is not at a method argument.
4. **The input of the patterns.** `EquatableArray<'T>`, or `ImmutableArray<'T>` as in the first consumer?
   *Decided: `EquatableArray<'T>`*: nested in a union case a pattern cannot convert, at the head of a `match` the
   wrap is one word. Inline patterns constrained on `Length` and the indexer accept both types and F# arrays without
   allocation *(verified)*, but they throw `NullReferenceException` on a default `ImmutableArray` and need the input
   type known at the pattern.
5. **Read-only `IList<'T>` and `ICollection<'T>`.** *Decided: yes.* FSharp.Core and LINQ take their fast paths
   for these two interfaces, not for `IReadOnlyList<'T>`. On 1000 `int` *(verified)*: `Seq.length` 4630 ns → 30 ns,
   `Seq.toArray` 5490 ns and 12.5 kB → 1020 ns and 4 kB, `Enumerable.Count` 940 ns → 40 ns, `Enumerable.ToList` 4600 ns
   → 750 ns, `ResizeArray (xs)` 3080 ns → 580 ns. The members are explicit, report `IsReadOnly`, and the mutating ones
   throw `NotSupportedException`, as on `ImmutableArray`.
6. **A JSON `null`.** The empty array, or an error? *Decided: the empty array*, the JSON counterpart of the
   default value. It takes effect under plain System.Text.Json only (§4).
7. **Two steps** (§6): release the type, the patterns and the converter from `main` before the chain of ADR 0001 starts,
   or make the type one link of that chain? *Decided: release first.* The first consumer is waiting for a
   release, and the chain cuts none before its end.
8. **Keys with array elements** (decision 5). Accept that equality and comparison disagree on F# arrays, `FlatList`s
   and tuples that hold them, or compare such elements by contents, as `.Equals` on an F# list does?
   *Decided: accept.* The element keeps the equality that .NET and C# give it. The type needs no list of element
   types that it treats differently. The other choice is cheap *(verified)*. It compares through
   `LanguagePrimitives.GenericEqualityERComparer`. On 32 elements that are arrays of `int` or `string`, it allocates
   nothing. On 32 `FlatList`s of four `int`, it allocates 7680 B per `Equals` and 3840 B per hash. That is not more than
   `CompareTo` allocates for the same elements. `EquatableArray` as the element type makes the question irrelevant.
