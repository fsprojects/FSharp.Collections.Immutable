/// <summary>
/// The functions of <see cref="T:FSharp.Collections.Immutable.FrozenSet"/> that take or return an optional value, with
/// <see cref="T:Microsoft.FSharp.Core.FSharpOption`1"/> instead of <see cref="T:Microsoft.FSharp.Core.FSharpValueOption`1"/>.
/// </summary>
/// <remarks>
/// After <c>open FSharp.Collections.Immutable.Option</c>, these functions shadow the default ones, and the other functions
/// of the default module stay available (ADR 0001, D3). <see cref="T:Microsoft.FSharp.Collections.SetModule"/> has none of
/// these functions; they take and return options as FSharp.Core would.
/// </remarks>
[<RequireQualifiedAccess>]
module FSharp.Collections.Immutable.Option.FrozenSet

open System.Collections.Frozen

// ---- Access ----

/// <summary>
/// Returns the element of the frozen set that equals a value, or <c>None</c> when there is none.
/// </summary>
/// <param name="value">The value to search for.</param>
/// <param name="set">The input frozen set.</param>
/// <returns>The element that the comparer of the frozen set considers equal to the value, or <c>None</c>.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(1) on average. The function wraps <see cref="M:System.Collections.Frozen.FrozenSet`1.TryGetValue(`0,`0@)"/>. The
/// element can differ from an equal value, for example in case under a case-insensitive comparer; the function returns
/// the instance that the frozen set holds.
/// </remarks>
[<CompiledName "TryGetValue">]
val tryGetValue : value : 'T -> set : FrozenSet<'T> -> 'T option

// ---- Transformation ----

/// <summary>
/// Returns the values that the chooser returns for the elements.
/// </summary>
/// <param name="chooser">The function that returns a value, or <c>None</c> to skip the element.</param>
/// <param name="set">The input frozen set.</param>
/// <returns>
/// The frozen set of the distinct chosen values, which compares with
/// <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>.
/// </returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n): the function builds a new frozen set. The comparer of the input is not kept, even when the chosen values have
/// the type of the elements.
/// </remarks>
[<CompiledName "Choose">]
val choose : chooser : ('T -> 'U option) -> set : FrozenSet<'T> -> FrozenSet<'U>

#if NET10_0_OR_GREATER
// ---- Type-specific ----

/// <summary>
/// Returns a lookup that finds the elements of the frozen set by values of an alternate type, or <c>None</c> when the
/// comparer of the frozen set cannot compare that type.
/// </summary>
/// <param name="set">The input frozen set.</param>
/// <returns>
/// The lookup, or <c>None</c> when the comparer of the frozen set does not implement
/// <see cref="T:System.Collections.Generic.IAlternateEqualityComparer`2"/> for the alternate type and the element type.
/// </returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(1). The function wraps
/// <see cref="M:System.Collections.Frozen.FrozenSet`1.TryGetAlternateLookup``1(System.Collections.Frozen.FrozenSet{`0}.AlternateLookup{``0}@)"/>
/// and exists on <c>net10.0</c> only. An F# generic function cannot be instantiated with a byref-like type, so a lookup
/// by a <see cref="T:System.ReadOnlySpan`1"/> needs that method.
/// </remarks>
[<CompiledName "TryAlternateLookup">]
val tryAlternateLookup<'Alternate, 'T> : set : FrozenSet<'T> -> FrozenSet<'T>.AlternateLookup<'Alternate> option
#endif
