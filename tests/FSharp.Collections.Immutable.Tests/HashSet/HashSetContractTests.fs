namespace FSharp.Collections.Immutable.Tests

open FSharp.Collections.Immutable
open Microsoft.VisualStudio.TestTools.UnitTesting

/// The reading contract for `HashSet`.
[<TestClass; HashSetTestCategory>]
type HashSetSetTests () =
    inherit SetContract<HashSet<int>> (HashSetOps.reading)

/// The transformation contract for `HashSet`.
[<TestClass; HashSetTestCategory>]
type HashSetTransformTests () =
    inherit SetTransformContract<HashSet<int>> (HashSetOps.transform)

/// The mutation contract for `HashSet`.
[<TestClass; HashSetTestCategory>]
type HashSetMutationTests () =
    inherit SetMutationContract<HashSet<int>> (HashSetOps.mutation)

/// The creation contract for `HashSet`.
[<TestClass; HashSetTestCategory>]
type HashSetCreationTests () =
    inherit SetCreationContract<HashSet<int>> (HashSetOps.creation)
