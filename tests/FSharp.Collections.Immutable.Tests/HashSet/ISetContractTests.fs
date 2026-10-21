namespace FSharp.Collections.Immutable.Tests

open System.Collections.Immutable
open Microsoft.VisualStudio.TestTools.UnitTesting

// The interface module has no creation functions, so it has no creation contract (ADR 0001, Appendix C).

/// The reading contract for `ISet`, over hash sets behind the interface.
[<TestClass; ISetTestCategory>]
type ISetSetTests () =
    inherit SetContract<IImmutableSet<int>> (ISetOps.reading)

/// The transformation contract for `ISet`, over hash sets behind the interface.
[<TestClass; ISetTestCategory>]
type ISetTransformTests () =
    inherit SetTransformContract<IImmutableSet<int>> (ISetOps.transform)

/// The mutation contract for `ISet`, over hash sets behind the interface.
[<TestClass; ISetTestCategory>]
type ISetMutationTests () =
    inherit SetMutationContract<IImmutableSet<int>> (ISetOps.mutation)
