#if INTERACTIVE
namespace global
#else
namespace FSharp.Collections.Immutable
#endif

[<AutoOpen>]
module internal ImmutableCollectionUtil =
    let inline checkNotNull name (arg : _ | null) =
        match arg with
        | null -> nullArg name
        | _ -> ()

module internal ErrorStrings =
    [<Literal>]
    let InputMustBeNonNegative = "The input must be non-negative."

    [<Literal>]
    let ListsHaveDifferentLengths = "The lists have different lengths."
