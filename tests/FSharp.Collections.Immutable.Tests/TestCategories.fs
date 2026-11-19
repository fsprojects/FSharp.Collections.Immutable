namespace FSharp.Collections.Immutable.Tests

open System.Collections.Generic
open Microsoft.VisualStudio.TestTools.UnitTesting

// Each attribute gives one category: its type name without the `TestCategoryAttribute` suffix, written as separate words.
// The attributes are sorted by type name (ordinal); a new attribute goes to its sorted position.
// The attributes go on the test classes only, for the collection and the area that a class examines; test methods carry
// no categories (ADR 0001, D8).

/// Categorizes a test that counts the bytes an operation allocates. These tests are compiled in Release builds only,
/// because a Debug build does not optimize the code that they measure.
type AllocationTestCategoryAttribute () =
    inherit TestCategoryBaseAttribute ()
    static let categories : IList<string> = [| "Allocation" |]
    override _.TestCategories = categories

/// The assertions that extend `Assert` (`Assert.fs`).
type AssertExtensionsTestCategoryAttribute () =
    inherit TestCategoryBaseAttribute ()
    static let categories : IList<string> = [| "Assert extensions" |]
    override _.TestCategories = categories

/// The builder functions and the builder modules.
type BuilderTestCategoryAttribute () =
    inherit TestCategoryBaseAttribute ()
    static let categories : IList<string> = [| "Builder" |]
    override _.TestCategories = categories

/// Categorizes a test of the order of values.
type ComparisonTestCategoryAttribute () =
    inherit TestCategoryBaseAttribute ()
    static let categories : IList<string> = [| "Comparison" |]
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

/// Categorizes a test of the default value of a struct collection, which must behave as the empty collection.
type DefaultValueTestCategoryAttribute () =
    inherit TestCategoryBaseAttribute ()
    static let categories : IList<string> = [| "Default value" |]
    override _.TestCategories = categories

/// Categorizes a test of equality and hash codes.
type EqualityTestCategoryAttribute () =
    inherit TestCategoryBaseAttribute ()
    static let categories : IList<string> = [| "Equality" |]
    override _.TestCategories = categories

/// <summary>Categorizes a test of <see cref="T:FSharp.Collections.Immutable.EquatableArray`1"/>.</summary>
type EquatableArrayTestCategoryAttribute () =
    inherit TestCategoryBaseAttribute ()
    static let categories : IList<string> = [| "EquatableArray" |]
    override _.TestCategories = categories

/// The `FlatList` module (`ImmutableArray`).
type FlatListTestCategoryAttribute () =
    inherit TestCategoryBaseAttribute ()
    static let categories : IList<string> = [| "FlatList" |]
    override _.TestCategories = categories

/// Categorizes a test of the text that %A and ToString give for a value.
type FormattingTestCategoryAttribute () =
    inherit TestCategoryBaseAttribute ()
    static let categories : IList<string> = [| "Formatting" |]
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

/// Categorizes a test of the members that .NET interfaces, conversions, spans and C# use.
type InteropTestCategoryAttribute () =
    inherit TestCategoryBaseAttribute ()
    static let categories : IList<string> = [| "Interop" |]
    override _.TestCategories = categories

/// Categorizes a test of the conversion to and from JSON.
type JsonTestCategoryAttribute () =
    inherit TestCategoryBaseAttribute ()
    static let categories : IList<string> = [| "JSON" |]
    override _.TestCategories = categories

/// Categorizes a test of the functions of a collection module.
type ModuleFunctionsTestCategoryAttribute () =
    inherit TestCategoryBaseAttribute ()
    static let categories : IList<string> = [| "Module functions" |]
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
