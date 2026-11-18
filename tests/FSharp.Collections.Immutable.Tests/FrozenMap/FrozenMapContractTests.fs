namespace FSharp.Collections.Immutable.Tests

open FSharp.Collections.Immutable
open Microsoft.VisualStudio.TestTools.UnitTesting

/// The reading contract for `FrozenMap`.
[<TestClass; FrozenMapTestCategory>]
type FrozenMapReadingTests () =
    inherit MapContract<FrozenMap<int, string>> (FrozenMapOps.reading)

/// The transformation contract for `FrozenMap`.
[<TestClass; FrozenMapTestCategory>]
type FrozenMapTransformTests () =
    inherit MapTransformContract<FrozenMap<int, string>> (FrozenMapOps.transform)

/// The creation contract for `FrozenMap` and its extension members, with the default comparer.
[<TestClass; FrozenMapTestCategory>]
type FrozenMapCreationTests () =
    inherit MapCreationContract<FrozenMap<int, string>> (FrozenMapOps.creation)

/// The creation contract for the members of `CustomComparersFrozenMap`, with a key comparer.
[<TestClass; FrozenMapTestCategory>]
type CustomComparersFrozenMapCreationTests () =
    inherit MapCreationContract<FrozenMap<int, string>> (FrozenMapOps.customComparersCreation)
