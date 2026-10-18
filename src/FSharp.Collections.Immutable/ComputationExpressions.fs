/// <summary>
/// The computation expressions of the collections (ADR 0001, D7).
/// </summary>
[<AutoOpen>]
module FSharp.Collections.Immutable.ComputationExpressions

/// <summary>Builds a flat list.</summary>
/// <example id="flatList-1">
/// <code lang="fsharp">
/// let squares = flatList {
///     yield 0
///     for i in 1..3 do
///         yield i * i
///     yield! [ 100; 200 ]
/// }
/// </code>
/// Evaluates to a flat list of 0, 1, 4, 9, 100 and 200.
/// </example>
let flatList = FlatListCE ()
