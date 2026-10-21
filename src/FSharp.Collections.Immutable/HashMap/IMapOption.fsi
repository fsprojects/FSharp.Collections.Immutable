/// <summary>
/// The functions of <see cref="T:FSharp.Collections.Immutable.IMap"/> that take or return an optional
/// value, with <see cref="T:Microsoft.FSharp.Core.FSharpOption`1"/> instead of
/// <see cref="T:Microsoft.FSharp.Core.FSharpValueOption`1"/>.
/// </summary>
/// <remarks>
/// After <c>open FSharp.Collections.Immutable.Option</c>, these functions shadow the default ones, and the other functions
/// of the default module stay available (ADR 0001, D3). Each function has the signature of the function with the same
/// name in <see cref="T:Microsoft.FSharp.Collections.MapModule"/> where that module has one.
/// </remarks>
[<RequireQualifiedAccess>]
module FSharp.Collections.Immutable.Option.IMap

open FSharp.Collections.Immutable

// ---- Access ----

/// <summary>
/// Returns the value of a key, or <c>None</c> when the map has no binding of the key.
/// </summary>
/// <param name="key">The key to find.</param>
/// <param name="table">The input map.</param>
/// <returns>The value of the key, or <c>None</c>.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(log n) for the immutable dictionaries.
/// </remarks>
[<CompiledName "TryFind">]
val tryFind : key : 'Key -> table : IMap<'Key, 'Value> -> 'Value option

/// <summary>
/// Returns the key of the map that equals a key in the key comparer of the map, or <c>None</c>.
/// </summary>
/// <param name="key">The key to find.</param>
/// <param name="table">The input map.</param>
/// <returns>The key that the map holds, which can differ from the given one, or <c>None</c>.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(log n) for the immutable dictionaries. The function wraps
/// <see cref="M:System.Collections.Immutable.IImmutableDictionary`2.TryGetKey(`0,`0@)"/>.
/// </remarks>
[<CompiledName "TryGetKey">]
val tryGetKey : key : 'Key -> table : IMap<'Key, 'Value> -> 'Key option

// ---- Search ----

/// <summary>
/// Returns the first value, in the order of enumeration, that the chooser returns for the bindings.
/// </summary>
/// <param name="chooser">The function that returns a value, or <c>None</c> to go on.</param>
/// <param name="table">The input map.</param>
/// <returns>The first chosen value.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <exception cref="T:System.Collections.Generic.KeyNotFoundException">Thrown when the chooser returns <c>None</c> for every binding.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Pick">]
val pick : chooser : ('Key -> 'Value -> 'U option) -> table : IMap<'Key, 'Value> -> 'U

/// <summary>
/// Returns the key of the first binding, in the order of enumeration, that satisfies the predicate, or <c>None</c>.
/// </summary>
/// <param name="predicate">The function to test the bindings.</param>
/// <param name="table">The input map.</param>
/// <returns>The key of the first binding that satisfies the predicate, or <c>None</c>.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "TryFindKey">]
val tryFindKey : predicate : ('Key -> 'Value -> bool) -> table : IMap<'Key, 'Value> -> 'Key option

/// <summary>
/// Returns the first value, in the order of enumeration, that the chooser returns for the bindings, or <c>None</c>.
/// </summary>
/// <param name="chooser">The function that returns a value, or <c>None</c> to go on.</param>
/// <param name="table">The input map.</param>
/// <returns>The first chosen value, or <c>None</c>.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "TryPick">]
val tryPick : chooser : ('Key -> 'Value -> 'U option) -> table : IMap<'Key, 'Value> -> 'U option

// ---- Transformation ----

/// <summary>
/// Returns a map with the binding of a key changed by a function of its current value.
/// </summary>
/// <param name="key">The key.</param>
/// <param name="f">
/// The function that takes the current value or <c>None</c>, and returns the new value or <c>None</c> to remove the
/// binding.
/// </param>
/// <param name="table">The input map.</param>
/// <returns>The map with the changed binding, of the implementation of the input.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(log n) for the immutable dictionaries. When <paramref name="f"/> returns a value that their value comparer finds
/// equal to the current value, they leave the binding as it is, its key included, and return the input itself (ADR 0001,
/// D14).
/// </remarks>
[<CompiledName "Change">]
val change : key : 'Key -> f : ('Value option -> 'Value option) -> table : IMap<'Key, 'Value> -> IMap<'Key, 'Value>

/// <summary>
/// Returns a hash map of the values that the chooser returns for the bindings, under their keys.
/// </summary>
/// <param name="chooser">The function that returns the new value, or <c>None</c> to drop the binding.</param>
/// <param name="table">The input map.</param>
/// <returns>
/// The hash map of the chosen values, with the default comparers. Keys that are distinct in the map and equal in the
/// default comparer keep the last chosen value, unless the default comparer finds the chosen values equal: then the first
/// of them stays, its key included (ADR 0001, D14).
/// </returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n log n).
/// </remarks>
[<CompiledName "Choose">]
val choose : chooser : ('Key -> 'Value -> 'U option) -> table : IMap<'Key, 'Value> -> HashMap<'Key, 'U> when 'Key : not null
