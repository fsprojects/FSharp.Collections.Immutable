namespace FSharp.Collections.Immutable.Tests

open System.Collections.Frozen
open Microsoft.VisualStudio.TestTools.UnitTesting

// A frozen set is built once, so it derives no test class from the mutation contract (ADR 0001, D1).

/// The reading contract for `FrozenSet`.
[<TestClass; FrozenSetTestCategory>]
type FrozenSetSetTests () =
    inherit SetContract<FrozenSet<int>> (FrozenSetOps.reading)

/// The transformation contract for `FrozenSet`.
[<TestClass; FrozenSetTestCategory>]
type FrozenSetTransformTests () =
    inherit SetTransformContract<FrozenSet<int>> (FrozenSetOps.transform)

/// The creation contract for `FrozenSet`.
[<TestClass; FrozenSetTestCategory>]
type FrozenSetCreationTests () =
    inherit SetCreationContract<FrozenSet<int>> (FrozenSetOps.creation)
