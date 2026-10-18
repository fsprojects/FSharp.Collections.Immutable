/// <summary>
/// The functions of <see cref="T:FSharp.Collections.Immutable.FrozenMap"/> that take or return an optional value, with
/// <see cref="T:Microsoft.FSharp.Core.FSharpOption`1"/> instead of <see cref="T:Microsoft.FSharp.Core.FSharpValueOption`1"/>.
/// </summary>
/// <remarks>
/// After <c>open FSharp.Collections.Immutable.Option</c>, these functions shadow the default ones, and the other functions
/// of the default module stay available (ADR 0001, D3). Each function has the signature of the function with the same
/// name in <see cref="T:Microsoft.FSharp.Collections.MapModule"/> where that module has one.
/// </remarks>
[<RequireQualifiedAccess>]
module FSharp.Collections.Immutable.Option.FrozenMap

open FSharp.Collections.Immutable

// ---- Access ----

/// <summary>
/// Returns the value of a key, or <c>None</c> when the frozen map has no binding of the key.
/// </summary>
/// <param name="key">The key to find.</param>
/// <param name="table">The input frozen map.</param>
/// <returns>The value of the key, or <c>None</c>.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(1) on average.
/// </remarks>
[<CompiledName "TryFind">]
val tryFind : key : 'Key -> table : FrozenMap<'Key, 'Value> -> 'Value option

// ---- Search ----

/// <summary>
/// Returns the first value, in the order of enumeration, that the chooser returns for the bindings.
/// </summary>
/// <param name="chooser">The function that returns a value, or <c>None</c> to go on.</param>
/// <param name="table">The input frozen map.</param>
/// <returns>The first chosen value.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <exception cref="T:System.Collections.Generic.KeyNotFoundException">Thrown when the chooser returns <c>None</c> for every binding.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Pick">]
val pick : chooser : ('Key -> 'Value -> 'U option) -> table : FrozenMap<'Key, 'Value> -> 'U

/// <summary>
/// Returns the key of the first binding, in the order of enumeration, that satisfies the predicate, or <c>None</c>.
/// </summary>
/// <param name="predicate">The function to test the bindings.</param>
/// <param name="table">The input frozen map.</param>
/// <returns>The key of the first binding that satisfies the predicate, or <c>None</c>.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "TryFindKey">]
val tryFindKey : predicate : ('Key -> 'Value -> bool) -> table : FrozenMap<'Key, 'Value> -> 'Key option

/// <summary>
/// Returns the first value, in the order of enumeration, that the chooser returns for the bindings, or <c>None</c>.
/// </summary>
/// <param name="chooser">The function that returns a value, or <c>None</c> to go on.</param>
/// <param name="table">The input frozen map.</param>
/// <returns>The first chosen value, or <c>None</c>.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "TryPick">]
val tryPick : chooser : ('Key -> 'Value -> 'U option) -> table : FrozenMap<'Key, 'Value> -> 'U option

// ---- Transformation ----

/// <summary>
/// Returns a frozen map of the values that the chooser returns for the bindings, under their keys.
/// </summary>
/// <param name="chooser">The function that returns the new value, or <c>None</c> to drop the binding.</param>
/// <param name="table">The input frozen map.</param>
/// <returns>The frozen map of the chosen values, with the key comparer of the input.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n). The function rebuilds the map: it freezes the chosen bindings into a new frozen map with the key comparer of the
/// input.
/// </remarks>
[<CompiledName "Choose">]
val choose : chooser : ('Key -> 'Value -> 'U option) -> table : FrozenMap<'Key, 'Value> -> FrozenMap<'Key, 'U>

#if NET10_0_OR_GREATER
// ---- Type-specific ----

/// <summary>
/// Returns a lookup of the bindings of the frozen map by keys of another type, or <c>None</c> when the frozen map cannot
/// look its keys up by that type.
/// </summary>
/// <param name="table">The input frozen map.</param>
/// <returns>
/// The lookup; <c>None</c> when the key comparer of the frozen map does not implement
/// <see cref="T:System.Collections.Generic.IAlternateEqualityComparer`2"/> of the alternate key type and the key type, or
/// when the keys are strings, which a frozen map looks up by spans of characters only.
/// </returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(1). The function wraps
/// <see cref="M:System.Collections.Frozen.FrozenDictionary`2.TryGetAlternateLookup``1(System.Collections.Frozen.FrozenDictionary{`0,`1}.AlternateLookup{``0}@)"/>.
/// An F# generic function cannot be instantiated with a byref-like type, so the function cannot look the keys up by a
/// span; for a span, call that method on the frozen map itself.
/// </remarks>
[<CompiledName "TryAlternateLookup">]
val tryAlternateLookup<'Alternate, 'Key, 'Value when 'Key : not null and 'Alternate : not null> :
    table : FrozenMap<'Key, 'Value> -> FrozenMap<'Key, 'Value>.AlternateLookup<'Alternate> option
#endif
