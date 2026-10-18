#if INTERACTIVE
namespace global
#else
namespace FSharp.Collections.Immutable
#endif

open System.Collections.Immutable

/// Argument checks shared by the collection modules.
/// The module keeps its old name, so that the modules written before ADR 0001 compile unchanged until they are replaced.
[<AutoOpen>]
module internal ImmutableCollectionUtil =

    /// Throws `ArgumentNullException` for a `null` argument, as FSharp.Core does for a `null` array or list.
    let inline checkNotNull name (arg : _ | null) =
        match arg with
        | null -> nullArg name
        | _ -> ()

    /// <summary>
    /// Throws <see cref="T:System.ArgumentNullException"/> for a default <see cref="T:System.Collections.Immutable.ImmutableArray`1"/>.
    /// </summary>
    /// <remarks>
    /// A default immutable array has no underlying array. The collection modules treat it as FSharp.Core treats a
    /// <see langword="null"/> array (ADR 0001, D6).
    /// </remarks>
    let inline checkNotDefault name (array : ImmutableArray<'T>) =
        if array.IsDefault then
            nullArg name

/// Exception messages of the modules written before ADR 0001.
module internal ErrorStrings =
    [<Literal>]
    let InputMustBeNonNegative = "The input must be non-negative."

    [<Literal>]
    let ListsHaveDifferentLengths = "The lists have different lengths."
