/// The names of the parameters of the set modules.
[<RequireQualifiedAccess>]
module FSharp.Collections.Immutable.Tests.SetParamNames

/// <summary>
/// Maps the name of a parameter of the FSharp.Core <c>Set</c> module to the name in the set modules.
/// </summary>
/// <remarks>
/// The set modules name their parameters as the <c>Set</c> module does. <c>Set.intersectMany</c> and
/// <c>Set.unionMany</c> call <c>Seq.reduce</c> and <c>Seq.fold</c>, which name the sequence of sets <c>source</c>; the
/// set modules name it <c>sets</c>, as the signatures of those two functions do.
/// </remarks>
let ofFSharpCore (name : string) =
    match name with
    | "source" -> "sets"
    | name -> name
