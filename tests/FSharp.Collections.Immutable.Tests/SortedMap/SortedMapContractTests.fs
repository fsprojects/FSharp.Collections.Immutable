namespace FSharp.Collections.Immutable.Tests

open FSharp.Collections.Immutable
open Microsoft.VisualStudio.TestTools.UnitTesting

/// The reading contract for `SortedMap`.
[<TestClass; SortedMapTestCategory>]
type SortedMapReadingTests () =
    inherit MapContract<SortedMap<int, string>> (SortedMapOps.reading)

/// The transformation contract for `SortedMap`.
[<TestClass; SortedMapTestCategory>]
type SortedMapTransformTests () =
    inherit MapTransformContract<SortedMap<int, string>> (SortedMapOps.transform)

/// The mutation contract for `SortedMap`.
[<TestClass; SortedMapTestCategory>]
type SortedMapMutationTests () =
    inherit MapMutationContract<SortedMap<int, string>> (SortedMapOps.mutation)

/// The creation contract for `SortedMap` and its extension members, with the default comparers.
[<TestClass; SortedMapTestCategory>]
type SortedMapCreationTests () =
    inherit MapCreationContract<SortedMap<int, string>> (SortedMapOps.creation)

/// The creation contract for the members of `CustomComparersSortedMap`, with a key comparer.
[<TestClass; SortedMapTestCategory>]
type CustomComparersSortedMapCreationTests () =
    inherit MapCreationContract<SortedMap<int, string>> (SortedMapOps.customComparersCreation)
