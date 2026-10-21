/// <summary>
/// The functions of <see cref="T:FSharp.Collections.Immutable.HashSet"/> that take or return an optional
/// value, with <see cref="T:Microsoft.FSharp.Core.FSharpOption`1"/> instead of
/// <see cref="T:Microsoft.FSharp.Core.FSharpValueOption`1"/>.
/// </summary>
/// <remarks>
/// After <c>open FSharp.Collections.Immutable.Option</c>, these functions shadow the default ones, and the other functions
/// of the default module stay available (ADR 0001, D3). <see cref="T:Microsoft.FSharp.Collections.SetModule"/> has neither
/// function; they take and return options as FSharp.Core would.
/// </remarks>
[<RequireQualifiedAccess>]
module FSharp.Collections.Immutable.Option.HashSet

open FSharp.Collections.Immutable

// ---- Access ----

/// <summary>
/// Returns the element of the hash set that equals a value, or <c>None</c> when there is none.
/// </summary>
/// <param name="value">The value to search for.</param>
/// <param name="set">The input hash set.</param>
/// <returns>The element that the comparer of the hash set considers equal to the value, or <c>None</c>.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(log n). The function wraps <see cref="M:System.Collections.Immutable.ImmutableHashSet`1.TryGetValue(`0,`0@)"/>. The
/// element can differ from an equal value, for example in case under a case-insensitive comparer; the function returns
/// the instance that the hash set holds.
/// </remarks>
[<CompiledName "TryGetValue">]
val tryGetValue : value : 'T -> set : HashSet<'T> -> 'T option

// ---- Transformation ----

/// <summary>
/// Returns the values that the chooser returns for the elements.
/// </summary>
/// <param name="chooser">The function that returns a value, or <c>None</c> to skip the element.</param>
/// <param name="set">The input hash set.</param>
/// <returns>
/// The hash set of the distinct chosen values, which compares with
/// <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>.
/// </returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n log n). The comparer of the input is not kept, even when the chosen values have the type of the elements.
/// </remarks>
[<CompiledName "Choose">]
val choose : chooser : ('T -> 'U option) -> set : HashSet<'T> -> HashSet<'U>
