namespace FSharp.Collections.Immutable.Tests

open FSharp.Collections.Immutable
open Microsoft.VisualStudio.TestTools.UnitTesting

/// The reading contract for `HashMap`.
[<TestClass; HashMapTestCategory>]
type HashMapReadingTests () =
    inherit MapContract<HashMap<int, string>> (HashMapOps.reading)

/// The transformation contract for `HashMap`.
[<TestClass; HashMapTestCategory>]
type HashMapTransformTests () =
    inherit MapTransformContract<HashMap<int, string>> (HashMapOps.transform)

/// The mutation contract for `HashMap`.
[<TestClass; HashMapTestCategory>]
type HashMapMutationTests () =
    inherit MapMutationContract<HashMap<int, string>> (HashMapOps.mutation)

/// The creation contract for `HashMap` and its extension members, with the default comparers.
[<TestClass; HashMapTestCategory>]
type HashMapCreationTests () =
    inherit MapCreationContract<HashMap<int, string>> (HashMapOps.creation)

/// The creation contract for the members of `CustomComparersHashMap`, with a key comparer.
[<TestClass; HashMapTestCategory>]
type CustomComparersHashMapCreationTests () =
    inherit MapCreationContract<HashMap<int, string>> (HashMapOps.customComparersCreation)
