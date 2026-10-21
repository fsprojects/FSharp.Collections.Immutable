namespace FSharp.Collections.Immutable.Tests

open FSharp.Collections.Immutable
open Microsoft.VisualStudio.TestTools.UnitTesting

/// The sequence-like contract for `IndexedList`.
[<TestClass; IndexedListTestCategory>]
type IndexedListSeqLikeTests () =
    inherit SeqLikeContract<IndexedList<int>> (IndexedListOps.seqLike)

/// The creation contract for `IndexedList`.
[<TestClass; IndexedListTestCategory>]
type IndexedListCreationTests () =
    inherit SeqCreationContract<IndexedList<int>> (IndexedListOps.creation)

/// The index-based contract for `IndexedList`.
[<TestClass; IndexedListTestCategory>]
type IndexedListIndexedTests () =
    inherit IndexedContract<IndexedList<int>> (IndexedListOps.indexed)

/// The contract of the functions that wrap members of `IImmutableList<T>`, for `IndexedList`.
[<TestClass; IndexedListTestCategory>]
type IndexedListMembersTests () =
    inherit ListMembersContract<IndexedList<string>> (IndexedListOps.listMembers)

/// The stable sorting contract for `IndexedList`.
[<TestClass; IndexedListTestCategory>]
type IndexedListStableSortTests () =
    inherit StableSortContract<IndexedList<struct (int * char)>> (IndexedListOps.stableSort)

/// The sequence-like contract for `IIndexedList`.
[<TestClass; IIndexedListTestCategory>]
type IIndexedListSeqLikeTests () =
    inherit SeqLikeContract<IIndexedList<int>> (IIndexedListOps.seqLike)

/// The index-based contract for `IIndexedList`.
[<TestClass; IIndexedListTestCategory>]
type IIndexedListIndexedTests () =
    inherit IndexedContract<IIndexedList<int>> (IIndexedListOps.indexed)

/// The contract of the functions that wrap members of `IImmutableList<T>`, for `IIndexedList`.
[<TestClass; IIndexedListTestCategory>]
type IIndexedListMembersTests () =
    inherit ListMembersContract<IIndexedList<string>> (IIndexedListOps.listMembers)

/// The stable sorting contract for `IIndexedList`.
[<TestClass; IIndexedListTestCategory>]
type IIndexedListStableSortTests () =
    inherit StableSortContract<IIndexedList<struct (int * char)>> (IIndexedListOps.stableSort)

/// The bounds on what `IndexedList` allocates for large collections.
[<TestClass; IndexedListTestCategory>]
type IndexedListCostTests () =
    inherit CostContract<IndexedList<int>> (IndexedListOps.cost)

/// The bounds on what `IIndexedList` allocates for large indexed lists.
[<TestClass; IIndexedListTestCategory>]
type IIndexedListCostTests () =
    inherit CostContract<IIndexedList<int>> (IIndexedListOps.cost)
