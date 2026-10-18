namespace FSharp.Collections.Immutable.Tests

open System.Collections.Generic
open Microsoft.VisualStudio.TestTools.UnitTesting

// Each attribute gives one category: its type name without the `TestCategoryAttribute` suffix, written as separate words.
// The attributes are sorted by type name (ordinal); a new attribute goes to its sorted position.
// The attributes go on the test classes only, for the collection and the area that a class examines; test methods carry
// no categories (ADR 0001, D8).

/// The builder functions and the builder modules.
type BuilderTestCategoryAttribute () =
    inherit TestCategoryBaseAttribute ()
    static let categories : IList<string> = [| "Builder" |]
    override _.TestCategories = categories

/// The computation expressions.
type ComputationExpressionTestCategoryAttribute () =
    inherit TestCategoryBaseAttribute ()
    static let categories : IList<string> = [| "Computation expression" |]
    override _.TestCategories = categories

/// The conventions of the test project itself.
type ConventionTestCategoryAttribute () =
    inherit TestCategoryBaseAttribute ()
    static let categories : IList<string> = [| "Convention" |]
    override _.TestCategories = categories

/// The `FlatList` module (`ImmutableArray`).
type FlatListTestCategoryAttribute () =
    inherit TestCategoryBaseAttribute ()
    static let categories : IList<string> = [| "FlatList" |]
    override _.TestCategories = categories

/// The `FrozenMap` module (`FrozenDictionary`).
type FrozenMapTestCategoryAttribute () =
    inherit TestCategoryBaseAttribute ()
    static let categories : IList<string> = [| "FrozenMap" |]
    override _.TestCategories = categories

/// The `FrozenSet` module (`FrozenSet`).
type FrozenSetTestCategoryAttribute () =
    inherit TestCategoryBaseAttribute ()
    static let categories : IList<string> = [| "FrozenSet" |]
    override _.TestCategories = categories

/// The `HashMap` module (`ImmutableDictionary`).
type HashMapTestCategoryAttribute () =
    inherit TestCategoryBaseAttribute ()
    static let categories : IList<string> = [| "HashMap" |]
    override _.TestCategories = categories

/// The `HashSet` module (`ImmutableHashSet`).
type HashSetTestCategoryAttribute () =
    inherit TestCategoryBaseAttribute ()
    static let categories : IList<string> = [| "HashSet" |]
    override _.TestCategories = categories

/// The `IIndexedList` module (`IImmutableList`).
type IIndexedListTestCategoryAttribute () =
    inherit TestCategoryBaseAttribute ()
    static let categories : IList<string> = [| "IIndexedList" |]
    override _.TestCategories = categories

/// The `IMap` module (`IImmutableDictionary`).
type IMapTestCategoryAttribute () =
    inherit TestCategoryBaseAttribute ()
    static let categories : IList<string> = [| "IMap" |]
    override _.TestCategories = categories

/// The `IQueue` module (`IImmutableQueue`).
type IQueueTestCategoryAttribute () =
    inherit TestCategoryBaseAttribute ()
    static let categories : IList<string> = [| "IQueue" |]
    override _.TestCategories = categories

/// The `ISet` module (`IImmutableSet`).
type ISetTestCategoryAttribute () =
    inherit TestCategoryBaseAttribute ()
    static let categories : IList<string> = [| "ISet" |]
    override _.TestCategories = categories

/// The `IStack` module (`IImmutableStack`).
type IStackTestCategoryAttribute () =
    inherit TestCategoryBaseAttribute ()
    static let categories : IList<string> = [| "IStack" |]
    override _.TestCategories = categories

/// The `IndexedList` module (`ImmutableList`).
type IndexedListTestCategoryAttribute () =
    inherit TestCategoryBaseAttribute ()
    static let categories : IList<string> = [| "IndexedList" |]
    override _.TestCategories = categories

/// The modules of the `FSharp.Collections.Immutable.Option` namespace.
type OptionNamespaceTestCategoryAttribute () =
    inherit TestCategoryBaseAttribute ()
    static let categories : IList<string> = [| "Option namespace" |]
    override _.TestCategories = categories

/// The nested `Parallel` modules.
type ParallelTestCategoryAttribute () =
    inherit TestCategoryBaseAttribute ()
    static let categories : IList<string> = [| "Parallel" |]
    override _.TestCategories = categories

/// The active patterns.
type PatternMatchingTestCategoryAttribute () =
    inherit TestCategoryBaseAttribute ()
    static let categories : IList<string> = [| "Pattern matching" |]
    override _.TestCategories = categories

/// The `Queue` module (`ImmutableQueue`).
type QueueTestCategoryAttribute () =
    inherit TestCategoryBaseAttribute ()
    static let categories : IList<string> = [| "Queue" |]
    override _.TestCategories = categories

/// The `SortedMap` module (`ImmutableSortedDictionary`).
type SortedMapTestCategoryAttribute () =
    inherit TestCategoryBaseAttribute ()
    static let categories : IList<string> = [| "SortedMap" |]
    override _.TestCategories = categories

/// The `SortedSet` module (`ImmutableSortedSet`).
type SortedSetTestCategoryAttribute () =
    inherit TestCategoryBaseAttribute ()
    static let categories : IList<string> = [| "SortedSet" |]
    override _.TestCategories = categories

/// The `Stack` module (`ImmutableStack`).
type StackTestCategoryAttribute () =
    inherit TestCategoryBaseAttribute ()
    static let categories : IList<string> = [| "Stack" |]
    override _.TestCategories = categories

/// The modules of the `FSharp.Collections.Immutable.Tuple` namespace.
type TupleNamespaceTestCategoryAttribute () =
    inherit TestCategoryBaseAttribute ()
    static let categories : IList<string> = [| "Tuple namespace" |]
    override _.TestCategories = categories
