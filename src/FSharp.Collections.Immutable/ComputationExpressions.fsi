/// <summary>
/// The computation expressions of the collections (ADR 0001, D7).
/// </summary>
[<AutoOpen>]
module FSharp.Collections.Immutable.ComputationExpressions

// ---- Creation ----

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
val flatList : FlatListCE

/// <summary>Builds an indexed list.</summary>
/// <example id="indexedList-1">
/// <code lang="fsharp">
/// let squares = indexedList {
///     yield 0
///     for i in 1..3 do
///         yield i * i
///     yield! [ 100; 200 ]
/// }
/// </code>
/// Evaluates to an indexed list of 0, 1, 4, 9, 100 and 200.
/// </example>
val indexedList : IndexedListCE

/// <summary>Builds a queue.</summary>
/// <example id="queue-1">
/// <code lang="fsharp">
/// let squares = queue {
///     yield 0
///     for i in 1..3 do
///         yield i * i
///     yield! [ 100; 200 ]
/// }
/// </code>
/// Evaluates to a queue of 0, 1, 4, 9, 100 and 200, from which 0 leaves first.
/// </example>
val queue : QueueCE
