/// <summary>
/// Functions for <see cref="T:FSharp.Collections.Immutable.IMap`2"/>, an immutable map of any implementation.
/// </summary>
/// <remarks>
/// <para>
/// The module mirrors <see cref="T:Microsoft.FSharp.Collections.MapModule"/>: each function with a name of that module has
/// its semantics, its argument checks and its exceptions (ADR 0001), except
/// <see cref="M:FSharp.Collections.Immutable.IMap.Add``2(``0,``1,System.Collections.Immutable.IImmutableDictionary{``0,``1})"/>.
/// It is the <see cref="M:System.Collections.Immutable.IImmutableDictionary`2.Add(`0,`1)"/> of the interface: it throws for
/// a key that is bound to another value, where <c>Map.add</c> replaces the value, and
/// <see cref="M:FSharp.Collections.Immutable.IMap.SetItem``2(``0,``1,System.Collections.Immutable.IImmutableDictionary{``0,``1})"/>
/// replaces it (ADR 0001, D14). The other functions wrap a member of
/// <see cref="T:System.Collections.Immutable.IImmutableDictionary`2"/> and keep its semantics and its exceptions.
/// </para>
/// <para>
/// The functions that visit the bindings one by one follow the order in which the map enumerates them, where
/// <see cref="T:Microsoft.FSharp.Collections.MapModule"/> follows the order of the keys.
/// </para>
/// <para>
/// The interface does not expose the comparers of a map. The transformations that keep the type of the values call the
/// members of the map, so that the result keeps its implementation and its comparers. The transformations that change the
/// type of the values return a <see cref="T:FSharp.Collections.Immutable.HashMap`2"/> with the default comparers.
/// </para>
/// <para>
/// The immutable dictionaries leave a binding as it is, its key included, when their value comparer finds a new value of
/// its key equal to the current one (ADR 0001, D14).
/// </para>
/// <para>
/// The module has the functions that the interface supports. It has no creation, builder or comparer functions and no
/// <c>containsValue</c>, because <see cref="T:System.Collections.Immutable.IImmutableDictionary`2"/> declares none of the
/// members behind them; use <see cref="T:FSharp.Collections.Immutable.HashMap"/> or the module of another implementation.
/// The one member of the interface that the pair-wise functions cannot reach,
/// <see cref="M:System.Collections.Immutable.IImmutableDictionary`2.Contains(System.Collections.Generic.KeyValuePair{`0,`1})"/>,
/// is <see cref="M:FSharp.Collections.Immutable.IMap.ContainsBinding``2(``0,``1,System.Collections.Immutable.IImmutableDictionary{``0,``1})"/>.
/// <c>GetValueOrDefault</c> is <c>IMap.tryFind key table |> ValueOption.defaultValue defaultValue</c>, and adding a
/// binding only when the key has none is
/// <see cref="M:FSharp.Collections.Immutable.IMap.Change``2(``0,Microsoft.FSharp.Core.FSharpFunc{Microsoft.FSharp.Core.FSharpValueOption{``1},Microsoft.FSharp.Core.FSharpValueOption{``1}},System.Collections.Immutable.IImmutableDictionary{``0,``1})"/>
/// with a function that keeps an existing value.
/// </para>
/// <para>
/// <see cref="M:FSharp.Collections.Immutable.IMap.AddRange``2(System.Collections.Generic.IEnumerable{System.Collections.Generic.KeyValuePair{``0,``1}},System.Collections.Immutable.IImmutableDictionary{``0,``1})"/>
/// and <see cref="M:FSharp.Collections.Immutable.IMap.SetItems``2(System.Collections.Generic.IEnumerable{System.Collections.Generic.KeyValuePair{``0,``1}},System.Collections.Immutable.IImmutableDictionary{``0,``1})"/>
/// take the bindings as key/value pairs, and the nested modules <see cref="T:FSharp.Collections.Immutable.IMap.Tuple"/> and
/// <see cref="T:FSharp.Collections.Immutable.IMap.ValueTuple"/> take reference and struct tuples (ADR 0001, D13).
/// </para>
/// <para>
/// A <see langword="null"/> map or sequence throws <see cref="T:System.ArgumentNullException"/>, and so does a default flat
/// list that is given as a sequence: it fails when a function enumerates it, so it is as good as a
/// <see langword="null"/> sequence (ADR 0001, D6).
/// </para>
/// <para>
/// Functions that return an optional value return a <see cref="T:Microsoft.FSharp.Core.FSharpValueOption`1"/>, and
/// functions that return tuples return struct tuples. The namespaces <see cref="N:FSharp.Collections.Immutable.Option"/>
/// and <see cref="N:FSharp.Collections.Immutable.Tuple"/> contain a module with the same name whose functions use options
/// and reference tuples instead.
/// </para>
/// </remarks>
[<RequireQualifiedAccess>]
module FSharp.Collections.Immutable.IMap

open System.Collections.Generic
open System.Collections.Immutable

// ---- Properties ----

/// <summary>Returns the number of bindings of the map.</summary>
/// <param name="table">The input map.</param>
/// <returns>The number of bindings.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>O(1) for the immutable dictionaries.</remarks>
[<CompiledName "Count">]
val count : table : IMap<'Key, 'Value> -> int

/// <summary>Returns whether the map has no bindings.</summary>
/// <param name="table">The input map.</param>
/// <returns><see langword="true"/> when the map is empty.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>O(1) for the immutable dictionaries.</remarks>
[<CompiledName "IsEmpty">]
val isEmpty : table : IMap<'Key, 'Value> -> bool

/// <summary>
/// Returns the keys of the map.
/// </summary>
/// <param name="table">The input map.</param>
/// <returns>The keys, in the order of enumeration of the map.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(1); O(n) to enumerate. The function returns
/// <see cref="P:System.Collections.Generic.IReadOnlyDictionary`2.Keys"/>.
/// </remarks>
[<CompiledName "Keys">]
val keys : table : IMap<'Key, 'Value> -> seq<'Key>

/// <summary>
/// Returns the values of the map, including the duplicates.
/// </summary>
/// <param name="table">The input map.</param>
/// <returns>The values, in the order of enumeration of the map.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(1); O(n) to enumerate. The function returns
/// <see cref="P:System.Collections.Generic.IReadOnlyDictionary`2.Values"/>.
/// </remarks>
[<CompiledName "Values">]
val values : table : IMap<'Key, 'Value> -> seq<'Value>

// ---- Access ----

/// <summary>Returns the value of a key.</summary>
/// <param name="key">The key to find.</param>
/// <param name="table">The input map.</param>
/// <returns>The value of the key.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <exception cref="T:System.Collections.Generic.KeyNotFoundException">Thrown when the map has no binding of the key.</exception>
/// <remarks>O(log n) for the immutable dictionaries.</remarks>
[<CompiledName "Find">]
val find : key : 'Key -> table : IMap<'Key, 'Value> -> 'Value

/// <summary>
/// Returns the value of a key, or <c>ValueNone</c> when the map has no binding of the key.
/// </summary>
/// <param name="key">The key to find.</param>
/// <param name="table">The input map.</param>
/// <returns>The value of the key, or <c>ValueNone</c>.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(log n) for the immutable dictionaries. The function is the <c>TryGetValue</c> of the interface with a <c>voption</c>
/// instead of the <c>out</c> parameter.
/// </remarks>
[<CompiledName "TryFind">]
val tryFind : key : 'Key -> table : IMap<'Key, 'Value> -> 'Value voption

/// <summary>
/// Returns the key of the map that equals a key in the key comparer of the map, or <c>ValueNone</c>.
/// </summary>
/// <param name="key">The key to find.</param>
/// <param name="table">The input map.</param>
/// <returns>The key that the map holds, which can differ from the given one, or <c>ValueNone</c>.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(log n) for the immutable dictionaries. The function wraps
/// <see cref="M:System.Collections.Immutable.IImmutableDictionary`2.TryGetKey(`0,`0@)"/>.
/// </remarks>
[<CompiledName "TryGetKey">]
val tryGetKey : key : 'Key -> table : IMap<'Key, 'Value> -> 'Key voption

// ---- Search ----

/// <summary>
/// Returns whether the map binds a key to a value, comparing the values with the value comparer of the map.
/// </summary>
/// <param name="key">The key to find.</param>
/// <param name="value">The value that the key must be bound to.</param>
/// <param name="table">The input map.</param>
/// <returns>
/// <see langword="true"/> when the map has a binding of the key whose value its value comparer finds equal to
/// <paramref name="value"/>.
/// </returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// <para>
/// O(log n) for the immutable dictionaries. The function wraps
/// <see cref="M:System.Collections.Immutable.IImmutableDictionary`2.Contains(System.Collections.Generic.KeyValuePair{`0,`1})"/>.
/// A key without a binding gives <see langword="false"/>. A <see langword="null"/> key is answered by the implementation:
/// <see cref="T:System.Collections.Immutable.ImmutableDictionary`2"/> leaves it to its key comparer, which gives
/// <see langword="false"/> for the default comparer and throws <see cref="T:System.ArgumentNullException"/> for a comparer
/// that rejects <see langword="null"/>, and <see cref="T:System.Collections.Immutable.ImmutableSortedDictionary`2"/> throws
/// <see cref="T:System.ArgumentNullException"/>.
/// </para>
/// <para>
/// The interface hides the value comparer of the map, so this is the only function that tests a binding with it: finding
/// the value with <c>IMap.tryFind</c> and comparing with <c>=</c> uses F# equality instead.
/// </para>
/// </remarks>
[<CompiledName "ContainsBinding">]
val containsBinding : key : 'Key -> value : 'Value -> table : IMap<'Key, 'Value> -> bool

/// <summary>Returns whether the map has a binding of a key.</summary>
/// <param name="key">The key to find.</param>
/// <param name="table">The input map.</param>
/// <returns><see langword="true"/> when the map has a binding of the key.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>O(log n) for the immutable dictionaries.</remarks>
[<CompiledName "ContainsKey">]
val containsKey : key : 'Key -> table : IMap<'Key, 'Value> -> bool

/// <summary>Returns whether any binding satisfies the predicate.</summary>
/// <param name="predicate">The function to test the bindings.</param>
/// <param name="table">The input map.</param>
/// <returns><see langword="true"/> when a binding satisfies the predicate.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>O(n).</remarks>
[<CompiledName "Exists">]
val exists : predicate : ('Key -> 'Value -> bool) -> table : IMap<'Key, 'Value> -> bool

/// <summary>
/// Returns the key of the first binding, in the order of enumeration, that satisfies the predicate.
/// </summary>
/// <param name="predicate">The function to test the bindings.</param>
/// <param name="table">The input map.</param>
/// <returns>The key of the first binding that satisfies the predicate.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <exception cref="T:System.Collections.Generic.KeyNotFoundException">Thrown when no binding satisfies the predicate.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "FindKey">]
val findKey : predicate : ('Key -> 'Value -> bool) -> table : IMap<'Key, 'Value> -> 'Key

/// <summary>Returns whether all bindings satisfy the predicate.</summary>
/// <param name="predicate">The function to test the bindings.</param>
/// <param name="table">The input map.</param>
/// <returns><see langword="true"/> when every binding satisfies the predicate.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>O(n).</remarks>
[<CompiledName "ForAll">]
val forall : predicate : ('Key -> 'Value -> bool) -> table : IMap<'Key, 'Value> -> bool

/// <summary>
/// Returns the first value, in the order of enumeration, that the chooser returns for the bindings.
/// </summary>
/// <param name="chooser">The function that returns a value, or <c>ValueNone</c> to go on.</param>
/// <param name="table">The input map.</param>
/// <returns>The first chosen value.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <exception cref="T:System.Collections.Generic.KeyNotFoundException">Thrown when the chooser returns <c>ValueNone</c> for every binding.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Pick">]
val pick : chooser : ('Key -> 'Value -> 'U voption) -> table : IMap<'Key, 'Value> -> 'U

/// <summary>
/// Returns the key of the first binding, in the order of enumeration, that satisfies the predicate, or <c>ValueNone</c>.
/// </summary>
/// <param name="predicate">The function to test the bindings.</param>
/// <param name="table">The input map.</param>
/// <returns>The key of the first binding that satisfies the predicate, or <c>ValueNone</c>.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "TryFindKey">]
val tryFindKey : predicate : ('Key -> 'Value -> bool) -> table : IMap<'Key, 'Value> -> 'Key voption

/// <summary>
/// Returns the first value, in the order of enumeration, that the chooser returns for the bindings, or <c>ValueNone</c>.
/// </summary>
/// <param name="chooser">The function that returns a value, or <c>ValueNone</c> to go on.</param>
/// <param name="table">The input map.</param>
/// <returns>The first chosen value, or <c>ValueNone</c>.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "TryPick">]
val tryPick : chooser : ('Key -> 'Value -> 'U voption) -> table : IMap<'Key, 'Value> -> 'U voption

// ---- Transformation ----

/// <summary>
/// Returns a map with a binding of a key to a value, for a key that is not bound to another value.
/// </summary>
/// <param name="key">The key.</param>
/// <param name="value">The value.</param>
/// <param name="table">The input map.</param>
/// <returns>The map with the binding, of the implementation of the input; the input itself when it already has the binding.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <exception cref="T:System.ArgumentException">
/// Thrown when the map binds the key to a value that its value comparer finds different.
/// </exception>
/// <remarks>
/// <para>
/// O(log n) for the immutable dictionaries. The function wraps
/// <see cref="M:System.Collections.Immutable.IImmutableDictionary`2.Add(`0,`1)"/> and behaves as it does (ADR 0001, D14),
/// as <see cref="M:FSharp.Collections.Immutable.HashMap.Add``2(``0,``1,System.Collections.Immutable.ImmutableDictionary{``0,``1})"/>
/// does: it does not replace the value of a key, where <c>Map.add</c> does. When the value comparer of the map finds the
/// value equal to the current value of the key, the binding stays as it is, its key included, and the input itself comes
/// back.
/// </para>
/// <para>
/// Use <see cref="M:FSharp.Collections.Immutable.IMap.SetItem``2(``0,``1,System.Collections.Immutable.IImmutableDictionary{``0,``1})"/>
/// to replace the value of a key, and
/// <see cref="M:FSharp.Collections.Immutable.IMap.Change``2(``0,Microsoft.FSharp.Core.FSharpFunc{Microsoft.FSharp.Core.FSharpValueOption{``1},Microsoft.FSharp.Core.FSharpValueOption{``1}},System.Collections.Immutable.IImmutableDictionary{``0,``1})"/>
/// to add a binding only when the key has none, or to compute the new value from the current one.
/// </para>
/// </remarks>
[<CompiledName "Add">]
val add : key : 'Key -> value : 'Value -> table : IMap<'Key, 'Value> -> IMap<'Key, 'Value>

/// <summary>
/// Returns a map with the bindings of key/value pairs added.
/// </summary>
/// <param name="pairs">The key/value pairs to add.</param>
/// <param name="table">The input map.</param>
/// <returns>The map with the bindings, of the implementation of the input.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="pairs"/> is <see langword="null"/> or a default flat list, or <paramref name="table"/> is <see langword="null"/>.</exception>
/// <exception cref="T:System.ArgumentException">
/// Thrown when a pair has the key of a binding of the map or of another pair, and a value that the value comparer of the
/// map finds different.
/// </exception>
/// <remarks>
/// O(m log(n + m)) for the immutable dictionaries. The function wraps
/// <see cref="M:System.Collections.Immutable.IImmutableDictionary`2.AddRange(System.Collections.Generic.IEnumerable{System.Collections.Generic.KeyValuePair{`0,`1}})"/>.
/// The immutable dictionaries reject another value of a key and accept a pair whose value their value comparer finds
/// equal to the value of the key, leaving that binding as it is, its key included (ADR 0001, D14).
/// <see cref="T:FSharp.Collections.Immutable.IMap.Tuple"/> and <see cref="T:FSharp.Collections.Immutable.IMap.ValueTuple"/>
/// add tuples.
/// </remarks>
[<CompiledName "AddRange">]
val addRange : pairs : seq<KeyValuePair<'Key, 'Value>> -> table : IMap<'Key, 'Value> -> IMap<'Key, 'Value>

/// <summary>
/// Returns a map with the binding of a key changed by a function of its current value.
/// </summary>
/// <param name="key">The key.</param>
/// <param name="f">
/// The function that takes the current value or <c>ValueNone</c>, and returns the new value or <c>ValueNone</c> to remove
/// the binding.
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
val change : key : 'Key -> f : ('Value voption -> 'Value voption) -> table : IMap<'Key, 'Value> -> IMap<'Key, 'Value>

/// <summary>
/// Returns a hash map of the values that the chooser returns for the bindings, under their keys.
/// </summary>
/// <param name="chooser">The function that returns the new value, or <c>ValueNone</c> to drop the binding.</param>
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
val choose : chooser : ('Key -> 'Value -> 'U voption) -> table : IMap<'Key, 'Value> -> HashMap<'Key, 'U> when 'Key : not null

/// <summary>
/// Returns an empty map of the implementation and with the comparers of a map.
/// </summary>
/// <param name="table">The input map.</param>
/// <returns>The empty map.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(1) for the immutable dictionaries. The function wraps <see cref="M:System.Collections.Immutable.IImmutableDictionary`2.Clear"/>.
/// </remarks>
[<CompiledName "Clear">]
val clear : table : IMap<'Key, 'Value> -> IMap<'Key, 'Value>

/// <summary>
/// Returns the bindings that satisfy the predicate.
/// </summary>
/// <param name="predicate">The function to test the bindings.</param>
/// <param name="table">The input map.</param>
/// <returns>
/// The map of the bindings that satisfy the predicate, of the implementation of the input; the input itself when every
/// binding does.
/// </returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n log n) for the immutable dictionaries. The function adds the bindings that stay to the result of
/// <see cref="M:System.Collections.Immutable.IImmutableDictionary`2.Clear"/>.
/// </remarks>
[<CompiledName "Filter">]
val filter : predicate : ('Key -> 'Value -> bool) -> table : IMap<'Key, 'Value> -> IMap<'Key, 'Value>

/// <summary>
/// Returns the results of a function applied to each binding, under the keys of the bindings.
/// </summary>
/// <param name="mapping">The function to transform the bindings.</param>
/// <param name="table">The input map.</param>
/// <returns>
/// The hash map of the results, with the default comparers. Keys that are distinct in the map and equal in the default
/// comparer keep the last result, unless the default comparer finds the results equal: then the first of them stays, its
/// key included (ADR 0001, D14).
/// </returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n log n).
/// </remarks>
[<CompiledName "Map">]
val map : mapping : ('Key -> 'Value -> 'U) -> table : IMap<'Key, 'Value> -> HashMap<'Key, 'U> when 'Key : not null

/// <summary>Returns a map without the binding of a key.</summary>
/// <param name="key">The key to remove.</param>
/// <param name="table">The input map.</param>
/// <returns>The map without the binding, of the implementation of the input.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>O(log n) for the immutable dictionaries.</remarks>
[<CompiledName "Remove">]
val remove : key : 'Key -> table : IMap<'Key, 'Value> -> IMap<'Key, 'Value>

/// <summary>
/// Returns a map without the bindings of keys.
/// </summary>
/// <param name="keys">The keys to remove; a key without a binding is skipped.</param>
/// <param name="table">The input map.</param>
/// <returns>The map without the bindings, of the implementation of the input.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="keys"/> is <see langword="null"/> or a default flat list, or <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(m log n) for the immutable dictionaries. The function wraps
/// <see cref="M:System.Collections.Immutable.IImmutableDictionary`2.RemoveRange(System.Collections.Generic.IEnumerable{`0})"/>.
/// </remarks>
[<CompiledName "RemoveRange">]
val removeRange : keys : seq<'Key> -> table : IMap<'Key, 'Value> -> IMap<'Key, 'Value>

/// <summary>
/// Returns a map with a binding of a key to a value, which replaces an existing binding of the key.
/// </summary>
/// <param name="key">The key.</param>
/// <param name="value">The value.</param>
/// <param name="table">The input map.</param>
/// <returns>The map with the binding, of the implementation of the input.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(log n) for the immutable dictionaries. The function wraps
/// <see cref="M:System.Collections.Immutable.IImmutableDictionary`2.SetItem(`0,`1)"/> and replaces the value of a key, as
/// <c>Map.add</c> does, where
/// <see cref="M:FSharp.Collections.Immutable.IMap.Add``2(``0,``1,System.Collections.Immutable.IImmutableDictionary{``0,``1})"/>
/// throws for a key that is bound to another value. When the value comparer of an immutable dictionary finds the value
/// equal to the current value of the key, the dictionary leaves the binding as it is, its key included, and returns the
/// input itself (ADR 0001, D14).
/// </remarks>
[<CompiledName "SetItem">]
val setItem : key : 'Key -> value : 'Value -> table : IMap<'Key, 'Value> -> IMap<'Key, 'Value>

/// <summary>
/// Returns a map with the bindings of key/value pairs, which replace the existing bindings of their keys.
/// </summary>
/// <param name="pairs">The key/value pairs to set; a later pair of a key replaces an earlier one.</param>
/// <param name="table">The input map.</param>
/// <returns>The map with the bindings, of the implementation of the input.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="pairs"/> is <see langword="null"/> or a default flat list, or <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(m log(n + m)) for the immutable dictionaries. The function wraps
/// <see cref="M:System.Collections.Immutable.IImmutableDictionary`2.SetItems(System.Collections.Generic.IEnumerable{System.Collections.Generic.KeyValuePair{`0,`1}})"/>.
/// The immutable dictionaries leave a binding as it is, its key included, when their value comparer finds the value of a
/// pair equal to the current value of its key or to the value of an earlier pair of the key (ADR 0001, D14).
/// <see cref="T:FSharp.Collections.Immutable.IMap.Tuple"/> and <see cref="T:FSharp.Collections.Immutable.IMap.ValueTuple"/>
/// set tuples.
/// </remarks>
[<CompiledName "SetItems">]
val setItems : pairs : seq<KeyValuePair<'Key, 'Value>> -> table : IMap<'Key, 'Value> -> IMap<'Key, 'Value>

// ---- Folding ----

/// <summary>
/// Applies a function to a state and each binding, in the order of enumeration.
/// </summary>
/// <param name="folder">The function that computes the next state.</param>
/// <param name="state">The initial state.</param>
/// <param name="table">The input map.</param>
/// <returns>The final state.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Fold">]
val fold : folder : ('State -> 'Key -> 'Value -> 'State) -> state : 'State -> table : IMap<'Key, 'Value> -> 'State

/// <summary>
/// Applies a function to each binding and a state, in the reverse order of enumeration.
/// </summary>
/// <param name="folder">The function that computes the next state.</param>
/// <param name="table">The input map.</param>
/// <param name="state">The initial state.</param>
/// <returns>The final state.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n). The function visits the bindings from the last one that the map enumerates to the first one; for a hash map
/// that is not the order of the keys. It copies the bindings first.
/// </remarks>
[<CompiledName "FoldBack">]
val foldBack : folder : ('Key -> 'Value -> 'State -> 'State) -> table : IMap<'Key, 'Value> -> state : 'State -> 'State

/// <summary>
/// Applies an action to each binding, in the order of enumeration.
/// </summary>
/// <param name="action">The action.</param>
/// <param name="table">The input map.</param>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Iterate">]
val iter : action : ('Key -> 'Value -> unit) -> table : IMap<'Key, 'Value> -> unit

// ---- Partitioning ----

/// <summary>Splits the bindings by a predicate.</summary>
/// <param name="predicate">The function to test the bindings.</param>
/// <param name="table">The input map.</param>
/// <returns>
/// The map of the bindings that satisfy the predicate and the map of the other ones, both of the implementation of the
/// input.
/// </returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>O(n log n) for the immutable dictionaries.</remarks>
[<CompiledName "Partition">]
val partition :
    predicate : ('Key -> 'Value -> bool) -> table : IMap<'Key, 'Value> -> struct (IMap<'Key, 'Value> * IMap<'Key, 'Value>)

// ---- Conversion ----

/// <summary>
/// Returns a new array of the bindings, in the order of enumeration.
/// </summary>
/// <param name="table">The input map.</param>
/// <returns>The array of key/value pairs.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "ToArray">]
val toArray : table : IMap<'Key, 'Value> -> struct ('Key * 'Value) array

/// <summary>Returns a list of the bindings, in the order of enumeration.</summary>
/// <param name="table">The input map.</param>
/// <returns>The list of key/value pairs.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>O(n).</remarks>
[<CompiledName "ToList">]
val toList : table : IMap<'Key, 'Value> -> struct ('Key * 'Value) list

/// <summary>
/// Returns the bindings as a sequence, in the order of enumeration.
/// </summary>
/// <param name="table">The input map.</param>
/// <returns>The sequence of key/value pairs, computed on demand.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(1); O(n) to enumerate.
/// </remarks>
[<CompiledName "ToSeq">]
val toSeq : table : IMap<'Key, 'Value> -> seq<struct ('Key * 'Value)>

/// <summary>
/// The functions of <see cref="T:FSharp.Collections.Immutable.IMap"/> that take bindings, with reference tuples instead of
/// key/value pairs.
/// </summary>
/// <remarks>
/// A function of the module takes one shape of the bindings, so the shape of the tuples of most F# code has a module of its
/// own (ADR 0001, D13): <c>IMap.Tuple.addRange pairs table</c>. The functions resolve after
/// <c>open FSharp.Collections.Immutable.Tuple</c> too, whose <c>IMap</c> module has no nested module of this name.
/// </remarks>
[<RequireQualifiedAccess>]
module Tuple =

    // ---- Transformation ----

    /// <summary>
    /// Returns a map with the bindings of key/value tuples added.
    /// </summary>
    /// <param name="pairs">The key/value tuples to add.</param>
    /// <param name="table">The input map.</param>
    /// <returns>The map with the bindings, of the implementation of the input.</returns>
    /// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="pairs"/> is <see langword="null"/> or a default flat list, or <paramref name="table"/> is <see langword="null"/>.</exception>
    /// <exception cref="T:System.ArgumentException">
    /// Thrown when a tuple has the key of a binding of the map or of another tuple, and a value that the value comparer of
    /// the map finds different.
    /// </exception>
    /// <remarks>
    /// O(m log(n + m)) for the immutable dictionaries. The function wraps
    /// <see cref="M:System.Collections.Immutable.IImmutableDictionary`2.AddRange(System.Collections.Generic.IEnumerable{System.Collections.Generic.KeyValuePair{`0,`1}})"/>.
    /// The immutable dictionaries reject another value of a key and accept a tuple whose value their value comparer finds
    /// equal to the value of the key, leaving that binding as it is, its key included (ADR 0001, D14).
    /// </remarks>
    [<CompiledName "AddRange">]
    val addRange : pairs : seq<'Key * 'Value> -> table : IMap<'Key, 'Value> -> IMap<'Key, 'Value>

    /// <summary>
    /// Returns a map with the bindings of key/value tuples, which replace the existing bindings of their keys.
    /// </summary>
    /// <param name="pairs">The key/value tuples to set; a later tuple of a key replaces an earlier one.</param>
    /// <param name="table">The input map.</param>
    /// <returns>The map with the bindings, of the implementation of the input.</returns>
    /// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="pairs"/> is <see langword="null"/> or a default flat list, or <paramref name="table"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// O(m log(n + m)) for the immutable dictionaries. The function wraps
    /// <see cref="M:System.Collections.Immutable.IImmutableDictionary`2.SetItems(System.Collections.Generic.IEnumerable{System.Collections.Generic.KeyValuePair{`0,`1}})"/>.
    /// The immutable dictionaries leave a binding as it is, its key included, when their value comparer finds the value of
    /// a tuple equal to the current value of its key or to the value of an earlier tuple of the key (ADR 0001, D14).
    /// </remarks>
    [<CompiledName "SetItems">]
    val setItems : pairs : seq<'Key * 'Value> -> table : IMap<'Key, 'Value> -> IMap<'Key, 'Value>

/// <summary>
/// The functions of <see cref="T:FSharp.Collections.Immutable.IMap"/> that take bindings, with struct tuples instead of
/// key/value pairs.
/// </summary>
/// <remarks>
/// A function of the module takes one shape of the bindings, so the struct tuples of ADR 0001, D3 have a module of their own
/// (D13): <c>IMap.ValueTuple.setItems pairs table</c>.
/// </remarks>
[<RequireQualifiedAccess>]
module ValueTuple =

    // ---- Transformation ----

    /// <summary>
    /// Returns a map with the bindings of key/value tuples added.
    /// </summary>
    /// <param name="pairs">The key/value tuples to add.</param>
    /// <param name="table">The input map.</param>
    /// <returns>The map with the bindings, of the implementation of the input.</returns>
    /// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="pairs"/> is <see langword="null"/> or a default flat list, or <paramref name="table"/> is <see langword="null"/>.</exception>
    /// <exception cref="T:System.ArgumentException">
    /// Thrown when a tuple has the key of a binding of the map or of another tuple, and a value that the value comparer of
    /// the map finds different.
    /// </exception>
    /// <remarks>
    /// O(m log(n + m)) for the immutable dictionaries. The function wraps
    /// <see cref="M:System.Collections.Immutable.IImmutableDictionary`2.AddRange(System.Collections.Generic.IEnumerable{System.Collections.Generic.KeyValuePair{`0,`1}})"/>.
    /// The immutable dictionaries reject another value of a key and accept a tuple whose value their value comparer finds
    /// equal to the value of the key, leaving that binding as it is, its key included (ADR 0001, D14).
    /// </remarks>
    [<CompiledName "AddRange">]
    val addRange : pairs : seq<struct ('Key * 'Value)> -> table : IMap<'Key, 'Value> -> IMap<'Key, 'Value>

    /// <summary>
    /// Returns a map with the bindings of key/value tuples, which replace the existing bindings of their keys.
    /// </summary>
    /// <param name="pairs">The key/value tuples to set; a later tuple of a key replaces an earlier one.</param>
    /// <param name="table">The input map.</param>
    /// <returns>The map with the bindings, of the implementation of the input.</returns>
    /// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="pairs"/> is <see langword="null"/> or a default flat list, or <paramref name="table"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// O(m log(n + m)) for the immutable dictionaries. The function wraps
    /// <see cref="M:System.Collections.Immutable.IImmutableDictionary`2.SetItems(System.Collections.Generic.IEnumerable{System.Collections.Generic.KeyValuePair{`0,`1}})"/>.
    /// The immutable dictionaries leave a binding as it is, its key included, when their value comparer finds the value of
    /// a tuple equal to the current value of its key or to the value of an earlier tuple of the key (ADR 0001, D14).
    /// </remarks>
    [<CompiledName "SetItems">]
    val setItems : pairs : seq<struct ('Key * 'Value)> -> table : IMap<'Key, 'Value> -> IMap<'Key, 'Value>
