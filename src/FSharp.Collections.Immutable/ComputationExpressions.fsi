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

/// <summary>
/// Builds a hash map with the default comparers.
/// </summary>
/// <remarks>
/// A later binding of a key replaces an earlier one, unless the default comparer of the values finds the two values
/// equal: then the earlier binding stays, its key included (ADR 0001, D14). The instance is generic in the type of the
/// keys, which each expression infers; <see cref="T:FSharp.Collections.Immutable.HashMapCE`1"/> takes a key comparer.
/// </remarks>
/// <example id="hashMap-1">
/// <code lang="fsharp">
/// let squares = hashMap {
///     for i in 1..3 do
///         yield struct (i, i * i)
///     yield KeyValuePair (10, 100)
///     yield struct (1, -1)
/// }
/// </code>
/// Evaluates to a hash map of 1 to -1, 2 to 4, 3 to 9 and 10 to 100.
/// </example>
[<GeneralizableValue>]
val hashMap<'Key when 'Key : not null> : HashMapCE<'Key>
