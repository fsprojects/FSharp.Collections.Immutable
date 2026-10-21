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
/// Builds a frozen set that compares with <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>.
/// </summary>
/// <remarks>
/// The value is a type function, so that one value builds frozen sets of every element type. A
/// <see cref="T:FSharp.Collections.Immutable.FrozenSetCE`1"/> created with an equality comparer builds frozen sets that
/// compare with it. The expression freezes its elements once, after it has collected all of them.
/// </remarks>
/// <example id="frozenSet-1">
/// <code lang="fsharp">
/// let squares = frozenSet {
///     for i in -2..2 do
///         yield i * i
///     yield! [ 4; 9 ]
/// }
/// </code>
/// Evaluates to a frozen set of 0, 1, 4 and 9.
/// </example>
/// <example id="frozenSet-2">
/// <code lang="fsharp">
/// let names = FrozenSetCE StringComparer.OrdinalIgnoreCase { "Ann"; "ANN"; "Bob" }
/// </code>
/// Evaluates to a frozen set of "Ann" and "Bob" that ignores case.
/// </example>
[<GeneralizableValue>]
val frozenSet<'T> : FrozenSetCE<'T>

/// <summary>
/// Builds a hash set that compares with <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>.
/// </summary>
/// <remarks>
/// The value is a type function, so that one value builds hash sets of every element type. A
/// <see cref="T:FSharp.Collections.Immutable.HashSetCE`1"/> created with an equality comparer builds hash sets that
/// compare with it.
/// </remarks>
/// <example id="hashSet-1">
/// <code lang="fsharp">
/// let squares = hashSet {
///     for i in -2..2 do
///         yield i * i
///     yield! [ 4; 9 ]
/// }
/// </code>
/// Evaluates to a hash set of 0, 1, 4 and 9.
/// </example>
/// <example id="hashSet-2">
/// <code lang="fsharp">
/// let names = HashSetCE StringComparer.OrdinalIgnoreCase { "Ann"; "ANN"; "Bob" }
/// </code>
/// Evaluates to a hash set of "Ann" and "Bob" that ignores case.
/// </example>
[<GeneralizableValue>]
val hashSet<'T> : HashSetCE<'T>
