namespace FSharp.Collections.Immutable.Tests

open FSharp.Collections.Immutable
open Microsoft.VisualStudio.TestTools.UnitTesting

/// The sequence-like contract for `FlatList`.
[<TestClass; FlatListTestCategory>]
type FlatListSeqLikeTests () =
    inherit SeqLikeContract<FlatList<int>> (FlatListOps.seqLike)

/// The creation contract for `FlatList`.
[<TestClass; FlatListTestCategory>]
type FlatListCreationTests () =
    inherit SeqCreationContract<FlatList<int>> (FlatListOps.creation)

/// The index-based contract for `FlatList`.
[<TestClass; FlatListTestCategory>]
type FlatListIndexedTests () =
    inherit IndexedContract<FlatList<int>> (FlatListOps.indexed)
