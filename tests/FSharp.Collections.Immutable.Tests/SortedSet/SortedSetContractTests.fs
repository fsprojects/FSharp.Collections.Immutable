namespace FSharp.Collections.Immutable.Tests

open FSharp.Collections.Immutable
open Microsoft.VisualStudio.TestTools.UnitTesting

// The ops records read every sorted set through SortedSetOps.toList, which checks that it enumerates in the order of Set.

/// The reading contract for `SortedSet`.
[<TestClass; SortedSetTestCategory>]
type SortedSetSetTests () =
    inherit SetContract<SortedSet<int>> (SortedSetOps.reading)

/// The transformation contract for `SortedSet`.
[<TestClass; SortedSetTestCategory>]
type SortedSetTransformTests () =
    inherit SetTransformContract<SortedSet<int>> (SortedSetOps.transform)

/// The mutation contract for `SortedSet`.
[<TestClass; SortedSetTestCategory>]
type SortedSetMutationTests () =
    inherit SetMutationContract<SortedSet<int>> (SortedSetOps.mutation)

/// The creation contract for `SortedSet`.
[<TestClass; SortedSetTestCategory>]
type SortedSetCreationTests () =
    inherit SetCreationContract<SortedSet<int>> (SortedSetOps.creation)
