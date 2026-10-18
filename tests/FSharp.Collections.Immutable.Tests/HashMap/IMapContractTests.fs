namespace FSharp.Collections.Immutable.Tests

open FSharp.Collections.Immutable
open Microsoft.VisualStudio.TestTools.UnitTesting

/// The reading contract for `IMap`, over hash maps.
[<TestClass; IMapTestCategory>]
type IMapReadingTests () =
    inherit MapContract<IMap<int, string>> (IMapOps.reading)

/// The transformation contract for `IMap`, over hash maps.
[<TestClass; IMapTestCategory>]
type IMapTransformTests () =
    inherit MapTransformContract<IMap<int, string>> (IMapOps.transform)

/// The mutation contract for `IMap`, over hash maps.
[<TestClass; IMapTestCategory>]
type IMapMutationTests () =
    inherit MapMutationContract<IMap<int, string>> (IMapOps.mutation)
