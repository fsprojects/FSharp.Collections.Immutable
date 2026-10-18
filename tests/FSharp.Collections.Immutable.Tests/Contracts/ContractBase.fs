namespace FSharp.Collections.Immutable.Tests

/// <summary>The helpers that every contract base shares.</summary>
/// <param name="name">The name of the module under test, for the messages of the assertions.</param>
/// <param name="mirror">The FSharp.Core module whose exceptions the module mirrors.</param>
/// <param name="paramName">Maps the name of a parameter of the FSharp.Core oracle to the name of the parameter of the module.</param>
[<AbstractClass>]
type ContractBase (name : string, mirror : Mirror, paramName : string -> string) =

    /// The result of the oracle of the FSharp.Core module that the module mirrors.
    member _.Mirrored (arrayOracle : unit -> 'R, listOracle : unit -> 'R) =
        match mirror with
        | ArrayMirror -> arrayOracle ()
        | ListMirror -> listOracle ()

    /// Asserts that a function of the module throws what the same function of the mirrored FSharp.Core module throws.
    member _.ThrowsLikeFSharpCore (arrayOracle : unit -> 'A, listOracle : unit -> 'L, actual : unit -> 'R, message : string) =
        ExceptionAssert.Mirrored (
            mirror,
            paramName,
            arrayOracle >> ignore,
            listOracle >> ignore,
            actual >> ignore,
            $"%s{name}: %s{message}"
        )
