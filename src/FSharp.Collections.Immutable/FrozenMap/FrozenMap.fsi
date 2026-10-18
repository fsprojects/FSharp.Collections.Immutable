/// <summary>
/// Functions for <see cref="T:FSharp.Collections.Immutable.FrozenMap`2"/>, the frozen map.
/// </summary>
/// <remarks>
/// <para>
/// The module mirrors <see cref="T:Microsoft.FSharp.Collections.MapModule"/>: each function with a name of that module has
/// its semantics, its argument checks and its exceptions (ADR 0001). The other functions wrap a member of
/// <see cref="T:System.Collections.Frozen.FrozenDictionary`2"/> or of <see cref="T:System.Collections.Frozen.FrozenDictionary"/>
/// and keep its semantics and its exceptions.
/// </para>
/// <para>
/// A frozen map is built once and read many times, so the module has no function that adds or removes a binding. Each
/// transformation builds a new frozen map in O(n), which costs as much as the creation of a frozen map.
/// </para>
/// <para>
/// A frozen map enumerates its bindings in an unspecified order, which stays the same for the same map. The functions that
/// visit the bindings one by one follow that order, where <see cref="T:Microsoft.FSharp.Collections.MapModule"/> follows
/// the order of the keys.
/// </para>
/// <para>
/// A frozen map compares its keys with the equality comparer that it was created with. The functions of the module create
/// frozen maps with the default comparer, <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>;
/// <see cref="M:FSharp.Collections.Immutable.FrozenMap.ForKeyComparer``2(System.Collections.Generic.IEqualityComparer{``0})"/>
/// returns a <see cref="T:FSharp.Collections.Immutable.CustomComparersFrozenMap`2"/>, which creates frozen maps with another
/// comparer (ADR 0001, D12). Every transformation keeps the key comparer. A frozen map has no comparer of its values, and
/// <see cref="T:System.Collections.Frozen.FrozenDictionary"/> has no creation overload with a flag that optimizes the map
/// for reading, so no creation function takes either.
/// </para>
/// <para>
/// A binding comes as a key/value pair, a reference tuple or a struct tuple (ADR 0001, D13). <c>FrozenMap.ofArray</c>,
/// <c>FrozenMap.ofList</c> and <c>FrozenMap.ofSeq</c> take every shape: they are the static members of
/// <see cref="T:FSharp.Collections.Immutable.FrozenMapExtensions"/>, which the module leaves visible by having no functions
/// with their names.
/// </para>
/// <para>
/// Where the FSharp.Core <c>Map</c> module and <see cref="T:System.Collections.Frozen.FrozenDictionary"/> differ in an edge
/// case, the module follows <see cref="T:System.Collections.Frozen.FrozenDictionary"/> (ADR 0001, D14): a later binding of a
/// key replaces the value of an earlier one and keeps its key, <c>ofSeqBy</c> rejects every repeated key, and an existing
/// frozen map whose comparer equals the requested one comes back itself.
/// </para>
/// <para>
/// The single-key members of the BCL are functions of the module under the names of FSharp.Core: <c>tryFind</c> is
/// <c>TryGetValue</c>, and <c>tryFind</c> followed by <c>ValueOption.defaultValue</c> is <c>GetValueOrDefault</c>. A frozen
/// map has no function that adds a binding, so it has no counterpart of <c>TryAdd</c>. The members that return a reference,
/// <c>GetValueRefOrNullRef</c> and the indexer, stay calls on the frozen map, because a function cannot return a reference.
/// </para>
/// <para>
/// A <see langword="null"/> map, sequence or comparer throws <see cref="T:System.ArgumentNullException"/>, and so does a
/// default flat list that is given as a sequence: it fails when a function enumerates it, so it is as good as a
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
module FSharp.Collections.Immutable.FrozenMap

open System
open System.Collections.Generic

// ---- Creation ----

/// <summary>
/// The empty frozen map, with the default comparer.
/// </summary>
/// <remarks>
/// O(1). The value is <see cref="P:System.Collections.Frozen.FrozenDictionary`2.Empty"/>.
/// </remarks>
[<GeneralizableValue>]
[<CompiledName "Empty">]
val empty<'Key, 'Value when 'Key : not null> : FrozenMap<'Key, 'Value>

/// <summary>
/// Holds an equality comparer of the keys, to create frozen maps with it.
/// </summary>
/// <param name="keyComparer">The equality comparer of the keys.</param>
/// <returns>The struct whose members create frozen maps with the key comparer.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="keyComparer"/> is <see langword="null"/>.</exception>
/// <remarks>
/// <para>
/// O(1); the struct allocates nothing. Its members have the names of the creation functions of the module (ADR 0001, D12).
/// </para>
/// <para>
/// The struct also gives an existing frozen map another key comparer, through <c>ofSeq</c>. A frozen map whose comparer
/// equals the key comparer of the struct comes back itself, in O(1), as
/// <see cref="M:System.Collections.Frozen.FrozenDictionary.ToFrozenDictionary``2(System.Collections.Generic.IEnumerable{System.Collections.Generic.KeyValuePair{``0,``1}},System.Collections.Generic.IEqualityComparer{``0})"/>
/// returns it. Any other frozen map is rebuilt in O(n), and the keys that the new comparer makes equal merge: the first of
/// them stays, with the value of the last one (ADR 0001, D14).
/// </para>
/// </remarks>
/// <example id="frozenMap-forKeyComparer-1">
/// <code lang="fsharp">
/// let table = (FrozenMap.forKeyComparer StringComparer.OrdinalIgnoreCase).ofSeq [ "one", 1; "ONE", 2 ]
/// </code>
/// Evaluates to a frozen map whose keys ignore the case, with the key "one" bound to 2.
/// </example>
/// <example id="frozenMap-forKeyComparer-2">
/// <code lang="fsharp">
/// let exact = FrozenMap.ofSeq [ "one", 1; "two", 2 ]
/// let ignoreCase = (FrozenMap.forKeyComparer StringComparer.OrdinalIgnoreCase).ofSeq exact
/// </code>
/// Evaluates <c>ignoreCase</c> to a frozen map of the same bindings whose keys ignore the case.
/// </example>
[<CompiledName "ForKeyComparer">]
val forKeyComparer : keyComparer : IEqualityComparer<'Key> -> CustomComparersFrozenMap<'Key, 'Value>

/// <summary>
/// Creates a frozen map from a read-only span of key/value pairs.
/// </summary>
/// <param name="span">The input span of key/value pairs.</param>
/// <returns>The frozen map of the bindings, with the default comparer.</returns>
/// <remarks>
/// <para>
/// O(n). The function wraps
/// <see cref="M:System.Collections.Frozen.FrozenDictionary.Create``2(System.ReadOnlySpan{System.Collections.Generic.KeyValuePair{``0,``1}})"/>
/// and reads the span itself, where <c>ofArray</c> and <c>ofSeq</c> need an array or a sequence, which copies the span first.
/// </para>
/// <para>
/// A repeated key never throws. A later binding of a key replaces the value of an earlier one and leaves its key (ADR 0001,
/// D14). The empty span gives the empty frozen map.
/// </para>
/// <para>
/// A <see cref="T:System.Span`1"/> or an array converts to the parameter only when the types of the call are known, and F#
/// warns about the conversion (FS3391). Where the type arguments are still to be inferred, call <c>ofSpan</c> or
/// <c>ofArray</c>.
/// </para>
/// </remarks>
[<CompiledName "OfReadOnlySpan">]
val ofReadOnlySpan : span : ReadOnlySpan<KeyValuePair<'Key, 'Value>> -> FrozenMap<'Key, 'Value>

/// <summary>
/// Creates a frozen map of the elements of a sequence under the keys that a projection returns for them.
/// </summary>
/// <param name="projection">The function that returns the key of an element.</param>
/// <param name="source">The input sequence, whose elements become the values.</param>
/// <returns>The frozen map of the elements under their keys.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="source"/> is <see langword="null"/> or a default flat list.</exception>
/// <exception cref="T:System.ArgumentException">Thrown when two elements have the same key, even when they are equal.</exception>
/// <remarks>
/// <para>
/// O(n). The function wraps
/// <see cref="M:System.Collections.Frozen.FrozenDictionary.ToFrozenDictionary``2(System.Collections.Generic.IEnumerable{``0},System.Func{``0,``1},System.Collections.Generic.IEqualityComparer{``1})"/>,
/// which rejects every repeated key (ADR 0001, D14).
/// </para>
/// <para>
/// The overload of <c>ToFrozenDictionary</c> with an element selector has no function. Mapping the source to pairs and
/// calling <c>ofSeq</c> builds the same map in O(n), but keeps the last binding of a repeated key, where the BCL member
/// rejects every repeated key. A frozen map has no <c>addRange</c>, so no function rejects them.
/// </para>
/// </remarks>
[<CompiledName "OfSeqBy">]
val ofSeqBy : projection : ('Value -> 'Key) -> source : seq<'Value> -> FrozenMap<'Key, 'Value>

/// <summary>
/// Creates a frozen map from a span of key/value pairs.
/// </summary>
/// <param name="span">The input span of key/value pairs.</param>
/// <returns>The frozen map of the bindings, with the default comparer.</returns>
/// <remarks>
/// <para>
/// O(n). The function is <c>ofReadOnlySpan</c> for a <see cref="T:System.Span`1"/>. F# converts a span to a read-only span
/// only where the type of the parameter is known, so a call that leaves the type arguments to inference fails with FS0001
/// for <c>ofReadOnlySpan</c> and succeeds here.
/// </para>
/// <para>
/// A repeated key never throws. A later binding of a key replaces the value of an earlier one and leaves its key (ADR 0001,
/// D14).
/// </para>
/// </remarks>
[<CompiledName "OfSpan">]
val ofSpan : span : Span<KeyValuePair<'Key, 'Value>> -> FrozenMap<'Key, 'Value>

/// <summary>Creates a frozen map of one binding.</summary>
/// <param name="key">The key.</param>
/// <param name="value">The value.</param>
/// <returns>The frozen map of the binding, with the default comparer.</returns>
/// <remarks>O(1).</remarks>
[<CompiledName "Singleton">]
val singleton : key : 'Key -> value : 'Value -> FrozenMap<'Key, 'Value>

// ---- Properties ----

/// <summary>Returns the number of bindings of the frozen map.</summary>
/// <param name="table">The input frozen map.</param>
/// <returns>The number of bindings.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>O(1).</remarks>
[<CompiledName "Count">]
val count : table : FrozenMap<'Key, 'Value> -> int

/// <summary>Returns whether the frozen map has no bindings.</summary>
/// <param name="table">The input frozen map.</param>
/// <returns><see langword="true"/> when the frozen map is empty.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>O(1).</remarks>
[<CompiledName "IsEmpty">]
val isEmpty : table : FrozenMap<'Key, 'Value> -> bool

/// <summary>
/// Returns the equality comparer of the keys.
/// </summary>
/// <param name="table">The input frozen map.</param>
/// <returns>The key comparer.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(1). The function wraps <see cref="P:System.Collections.Frozen.FrozenDictionary`2.Comparer"/>.
/// </remarks>
[<CompiledName "KeyComparer">]
val keyComparer : table : FrozenMap<'Key, 'Value> -> IEqualityComparer<'Key>

/// <summary>
/// Returns the keys of the frozen map.
/// </summary>
/// <param name="table">The input frozen map.</param>
/// <returns>The flat list of the keys, in the order of enumeration of the frozen map.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(1). The function wraps <see cref="P:System.Collections.Frozen.FrozenDictionary`2.Keys"/>: the flat list shares the
/// array of the keys of the frozen map instead of copying it. <c>FlatList.asSpan</c> views the array as a span, also
/// without a copy.
/// </remarks>
[<CompiledName "Keys">]
val keys : table : FrozenMap<'Key, 'Value> -> FlatList<'Key>

/// <summary>
/// Returns the values of the frozen map, including the duplicates.
/// </summary>
/// <param name="table">The input frozen map.</param>
/// <returns>The flat list of the values, in the order of enumeration of the frozen map.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(1). The function wraps <see cref="P:System.Collections.Frozen.FrozenDictionary`2.Values"/>: the flat list shares the
/// array of the values of the frozen map instead of copying it. <c>FlatList.asSpan</c> views the array as a span, also
/// without a copy.
/// </remarks>
[<CompiledName "Values">]
val values : table : FrozenMap<'Key, 'Value> -> FlatList<'Value>

// ---- Access ----

/// <summary>Returns the value of a key.</summary>
/// <param name="key">The key to find.</param>
/// <param name="table">The input frozen map.</param>
/// <returns>The value of the key.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <exception cref="T:System.Collections.Generic.KeyNotFoundException">Thrown when the frozen map has no binding of the key.</exception>
/// <remarks>O(1) on average.</remarks>
[<CompiledName "Find">]
val find : key : 'Key -> table : FrozenMap<'Key, 'Value> -> 'Value

/// <summary>
/// Returns the value of a key, or <c>ValueNone</c> when the frozen map has no binding of the key.
/// </summary>
/// <param name="key">The key to find.</param>
/// <param name="table">The input frozen map.</param>
/// <returns>The value of the key, or <c>ValueNone</c>.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(1) on average.
/// </remarks>
[<CompiledName "TryFind">]
val tryFind : key : 'Key -> table : FrozenMap<'Key, 'Value> -> 'Value voption

// ---- Search ----

/// <summary>
/// Returns whether the frozen map binds a key to a value.
/// </summary>
/// <param name="key">The key to find.</param>
/// <param name="value">The value to compare with the value of the key.</param>
/// <param name="table">The input frozen map.</param>
/// <returns><see langword="true"/> when the frozen map has a binding of the key whose value equals the given one.</returns>
/// <exception cref="T:System.ArgumentNullException">
/// Thrown when <paramref name="table"/> or <paramref name="key"/> is <see langword="null"/>.
/// </exception>
/// <remarks>
/// <para>
/// O(1) on average. The function wraps the explicit implementation of
/// <see cref="M:System.Collections.Generic.ICollection`1.Contains(`0)"/> for a key/value pair.
/// </para>
/// <para>
/// A frozen map has no comparer of its values, so the function compares them with
/// <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>, as <c>containsValue</c> does. <c>tryFind</c>
/// followed by <c>=</c> compares with F# equality, which differs from it for some types, such as floats: a map that binds a
/// key to <c>nan</c> contains that binding, and <c>nan = nan</c> is <see langword="false"/>.
/// </para>
/// </remarks>
[<CompiledName "ContainsBinding">]
val containsBinding : key : 'Key -> value : 'Value -> table : FrozenMap<'Key, 'Value> -> bool

/// <summary>Returns whether the frozen map has a binding of a key.</summary>
/// <param name="key">The key to find.</param>
/// <param name="table">The input frozen map.</param>
/// <returns><see langword="true"/> when the frozen map has a binding of the key.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>O(1) on average.</remarks>
[<CompiledName "ContainsKey">]
val containsKey : key : 'Key -> table : FrozenMap<'Key, 'Value> -> bool

/// <summary>
/// Returns whether the frozen map has a binding of a value.
/// </summary>
/// <param name="value">The value to find.</param>
/// <param name="table">The input frozen map.</param>
/// <returns><see langword="true"/> when a value of the frozen map equals the given one.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n). A frozen map has no comparer of its values, so the function compares them with
/// <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>, as the frozen map itself does when it looks a
/// key/value pair up.
/// </remarks>
[<CompiledName "ContainsValue">]
val containsValue : value : 'Value -> table : FrozenMap<'Key, 'Value> -> bool

/// <summary>Returns whether any binding satisfies the predicate.</summary>
/// <param name="predicate">The function to test the bindings.</param>
/// <param name="table">The input frozen map.</param>
/// <returns><see langword="true"/> when a binding satisfies the predicate.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>O(n).</remarks>
[<CompiledName "Exists">]
val exists : predicate : ('Key -> 'Value -> bool) -> table : FrozenMap<'Key, 'Value> -> bool

/// <summary>
/// Returns the key of the first binding, in the order of enumeration, that satisfies the predicate.
/// </summary>
/// <param name="predicate">The function to test the bindings.</param>
/// <param name="table">The input frozen map.</param>
/// <returns>The key of the first binding that satisfies the predicate.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <exception cref="T:System.Collections.Generic.KeyNotFoundException">Thrown when no binding satisfies the predicate.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "FindKey">]
val findKey : predicate : ('Key -> 'Value -> bool) -> table : FrozenMap<'Key, 'Value> -> 'Key

/// <summary>Returns whether all bindings satisfy the predicate.</summary>
/// <param name="predicate">The function to test the bindings.</param>
/// <param name="table">The input frozen map.</param>
/// <returns><see langword="true"/> when every binding satisfies the predicate.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>O(n).</remarks>
[<CompiledName "ForAll">]
val forall : predicate : ('Key -> 'Value -> bool) -> table : FrozenMap<'Key, 'Value> -> bool

/// <summary>
/// Returns the first value, in the order of enumeration, that the chooser returns for the bindings.
/// </summary>
/// <param name="chooser">The function that returns a value, or <c>ValueNone</c> to go on.</param>
/// <param name="table">The input frozen map.</param>
/// <returns>The first chosen value.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <exception cref="T:System.Collections.Generic.KeyNotFoundException">Thrown when the chooser returns <c>ValueNone</c> for every binding.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Pick">]
val pick : chooser : ('Key -> 'Value -> 'U voption) -> table : FrozenMap<'Key, 'Value> -> 'U

/// <summary>
/// Returns the key of the first binding, in the order of enumeration, that satisfies the predicate, or <c>ValueNone</c>.
/// </summary>
/// <param name="predicate">The function to test the bindings.</param>
/// <param name="table">The input frozen map.</param>
/// <returns>The key of the first binding that satisfies the predicate, or <c>ValueNone</c>.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "TryFindKey">]
val tryFindKey : predicate : ('Key -> 'Value -> bool) -> table : FrozenMap<'Key, 'Value> -> 'Key voption

/// <summary>
/// Returns the first value, in the order of enumeration, that the chooser returns for the bindings, or <c>ValueNone</c>.
/// </summary>
/// <param name="chooser">The function that returns a value, or <c>ValueNone</c> to go on.</param>
/// <param name="table">The input frozen map.</param>
/// <returns>The first chosen value, or <c>ValueNone</c>.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "TryPick">]
val tryPick : chooser : ('Key -> 'Value -> 'U voption) -> table : FrozenMap<'Key, 'Value> -> 'U voption

// ---- Transformation ----

/// <summary>
/// Returns a frozen map of the values that the chooser returns for the bindings, under their keys.
/// </summary>
/// <param name="chooser">The function that returns the new value, or <c>ValueNone</c> to drop the binding.</param>
/// <param name="table">The input frozen map.</param>
/// <returns>The frozen map of the chosen values, with the key comparer of the input.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n). The function rebuilds the map: it freezes the chosen bindings into a new frozen map with the key comparer of the
/// input.
/// </remarks>
[<CompiledName "Choose">]
val choose : chooser : ('Key -> 'Value -> 'U voption) -> table : FrozenMap<'Key, 'Value> -> FrozenMap<'Key, 'U>

/// <summary>
/// Returns the bindings that satisfy the predicate.
/// </summary>
/// <param name="predicate">The function to test the bindings.</param>
/// <param name="table">The input frozen map.</param>
/// <returns>
/// The frozen map of the bindings that satisfy the predicate, with the key comparer of the input; the input itself when
/// every binding does.
/// </returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n). The function rebuilds the map: it freezes the bindings that satisfy the predicate into a new frozen map with the
/// key comparer of the input.
/// </remarks>
[<CompiledName "Filter">]
val filter : predicate : ('Key -> 'Value -> bool) -> table : FrozenMap<'Key, 'Value> -> FrozenMap<'Key, 'Value>

/// <summary>
/// Returns the results of a function applied to each binding, under the keys of the bindings.
/// </summary>
/// <param name="mapping">The function to transform the bindings.</param>
/// <param name="table">The input frozen map.</param>
/// <returns>The frozen map of the results, with the key comparer of the input.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n). The function rebuilds the map: it freezes the results into a new frozen map with the key comparer of the input.
/// </remarks>
[<CompiledName "Map">]
val map : mapping : ('Key -> 'Value -> 'U) -> table : FrozenMap<'Key, 'Value> -> FrozenMap<'Key, 'U>

// ---- Folding ----

/// <summary>
/// Applies a function to a state and each binding, in the order of enumeration.
/// </summary>
/// <param name="folder">The function that computes the next state.</param>
/// <param name="state">The initial state.</param>
/// <param name="table">The input frozen map.</param>
/// <returns>The final state.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Fold">]
val fold : folder : ('State -> 'Key -> 'Value -> 'State) -> state : 'State -> table : FrozenMap<'Key, 'Value> -> 'State

/// <summary>
/// Applies a function to each binding and a state, in the reverse order of enumeration.
/// </summary>
/// <param name="folder">The function that computes the next state.</param>
/// <param name="table">The input frozen map.</param>
/// <param name="state">The initial state.</param>
/// <returns>The final state.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n). The function visits the bindings from the last one that the frozen map enumerates to the first one, so it sees
/// them in the reverse order of
/// <see cref="M:FSharp.Collections.Immutable.FrozenMap.Fold``3(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpFunc{``1,Microsoft.FSharp.Core.FSharpFunc{``2,``0}}},``0,System.Collections.Frozen.FrozenDictionary{``1,``2})"/>,
/// not in the order of the keys. It reads the arrays of the keys and the values of the frozen map backwards, without a
/// copy.
/// </remarks>
[<CompiledName "FoldBack">]
val foldBack : folder : ('Key -> 'Value -> 'State -> 'State) -> table : FrozenMap<'Key, 'Value> -> state : 'State -> 'State

/// <summary>
/// Applies an action to each binding, in the order of enumeration.
/// </summary>
/// <param name="action">The action.</param>
/// <param name="table">The input frozen map.</param>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Iterate">]
val iter : action : ('Key -> 'Value -> unit) -> table : FrozenMap<'Key, 'Value> -> unit

// ---- Partitioning ----

/// <summary>
/// Splits the bindings by a predicate.
/// </summary>
/// <param name="predicate">The function to test the bindings.</param>
/// <param name="table">The input frozen map.</param>
/// <returns>
/// The frozen map of the bindings that satisfy the predicate and the frozen map of the other ones, both with the key
/// comparer of the input; the input itself in place of a part that has every binding.
/// </returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n). The function rebuilds the map: it freezes each part into a new frozen map with the key comparer of the input.
/// </remarks>
[<CompiledName "Partition">]
val partition :
    predicate : ('Key -> 'Value -> bool) ->
    table : FrozenMap<'Key, 'Value> ->
        struct (FrozenMap<'Key, 'Value> * FrozenMap<'Key, 'Value>)

// ---- Conversion ----

/// <summary>
/// Copies the bindings of the frozen map to an array, starting at an index.
/// </summary>
/// <param name="destination">The array that receives the bindings.</param>
/// <param name="destinationIndex">The index in <paramref name="destination"/> of the first binding.</param>
/// <param name="table">The input frozen map.</param>
/// <exception cref="T:System.ArgumentNullException">
/// Thrown when <paramref name="table"/> or <paramref name="destination"/> is <see langword="null"/>.
/// </exception>
/// <exception cref="T:System.ArgumentOutOfRangeException">
/// Thrown when <paramref name="destinationIndex"/> is negative or greater than the length of <paramref name="destination"/>.
/// </exception>
/// <exception cref="T:System.ArgumentException">
/// Thrown when <paramref name="destination"/> has no room for every binding from <paramref name="destinationIndex"/> on.
/// </exception>
/// <remarks>
/// <para>
/// O(n). The function wraps
/// <see cref="M:System.Collections.Frozen.FrozenDictionary`2.CopyTo(System.Collections.Generic.KeyValuePair{`0,`1}[],System.Int32)"/>
/// and fills an array that the caller owns, in the order of enumeration of the frozen map, where <c>toArray</c> allocates a
/// new array of struct tuples. The elements before the index and after the last binding stay as they are.
/// </para>
/// <para>
/// The BCL gives <see cref="T:System.ArgumentOutOfRangeException"/> no parameter name. The overload of the method that takes
/// a <see cref="T:System.Span`1"/> stays a call on the frozen map: a module function cannot be overloaded, and a parameter
/// of a byref-like type cannot be partially applied.
/// </para>
/// </remarks>
[<CompiledName "CopyTo">]
val copyTo : destination : KeyValuePair<'Key, 'Value> array -> destinationIndex : int -> table : FrozenMap<'Key, 'Value> -> unit

/// <summary>
/// Returns a new array of the bindings, in the order of enumeration.
/// </summary>
/// <param name="table">The input frozen map.</param>
/// <returns>The array of key/value pairs.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "ToArray">]
val toArray : table : FrozenMap<'Key, 'Value> -> struct ('Key * 'Value) array

/// <summary>Returns a list of the bindings, in the order of enumeration.</summary>
/// <param name="table">The input frozen map.</param>
/// <returns>The list of key/value pairs.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>O(n).</remarks>
[<CompiledName "ToList">]
val toList : table : FrozenMap<'Key, 'Value> -> struct ('Key * 'Value) list

/// <summary>
/// Returns the bindings as a sequence, in the order of enumeration.
/// </summary>
/// <param name="table">The input frozen map.</param>
/// <returns>The sequence of key/value pairs, computed on demand.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(1); O(n) to enumerate.
/// </remarks>
[<CompiledName "ToSeq">]
val toSeq : table : FrozenMap<'Key, 'Value> -> seq<struct ('Key * 'Value)>

#if NET10_0_OR_GREATER
// ---- Type-specific ----

/// <summary>
/// Returns a lookup of the bindings of the frozen map by keys of another type.
/// </summary>
/// <param name="table">The input frozen map.</param>
/// <returns>
/// The lookup, whose
/// <see cref="M:System.Collections.Frozen.FrozenDictionary`2.AlternateLookup`1.ContainsKey(`2)"/>,
/// <see cref="M:System.Collections.Frozen.FrozenDictionary`2.AlternateLookup`1.TryGetValue(`2,`1@)"/> and
/// <see cref="P:System.Collections.Frozen.FrozenDictionary`2.AlternateLookup`1.Item(`2)"/> find a binding by an alternate
/// key.
/// </returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <exception cref="T:System.InvalidOperationException">
/// Thrown when the key comparer of the frozen map does not implement
/// <see cref="T:System.Collections.Generic.IAlternateEqualityComparer`2"/> of the alternate key type and the key type, or
/// when the keys are strings, which a frozen map looks up by spans of characters only.
/// </exception>
/// <remarks>
/// <para>
/// O(1). The function wraps <see cref="M:System.Collections.Frozen.FrozenDictionary`2.GetAlternateLookup``1"/>. The type of
/// the alternate keys is the first type argument, so that a call names it and leaves the others to inference.
/// </para>
/// <para>
/// The function is on <c>net10.0</c> only: the BCL member comes with .NET 9, and the library has no <c>net9.0</c> target
/// (ADR 0001, D11).
/// </para>
/// <para>
/// An F# generic function cannot be instantiated with a byref-like type, so the function cannot look the keys up by a
/// span, such as the <see cref="T:System.ReadOnlySpan`1"/> of characters that string keys take with
/// <see cref="P:System.StringComparer.Ordinal"/>. For a span, call
/// <see cref="M:System.Collections.Frozen.FrozenDictionary`2.GetAlternateLookup``1"/> on the frozen map itself.
/// </para>
/// </remarks>
/// <example id="alternateLookup-1">
/// <code lang="fsharp">
/// // The comparer implements IEqualityComparer&lt;int64&gt; and IAlternateEqualityComparer&lt;int, int64&gt;
/// let table = (FrozenMap.forKeyComparer comparer).ofSeq [ 1L, "one"; 2L, "two" ]
/// let lookup = FrozenMap.alternateLookup&lt;int, _, _&gt; table
/// let two = lookup[2]
///
/// // A span needs the method of the frozen map
/// let names = (FrozenMap.forKeyComparer StringComparer.Ordinal).ofSeq [ "one", 1; "two", 2 ]
/// let one = names.GetAlternateLookup&lt;ReadOnlySpan&lt;char&gt;&gt;().ContainsKey ("one".AsSpan ())
/// </code>
/// Evaluates <c>two</c> to <c>"two"</c> and <c>one</c> to <see langword="true"/>.
/// </example>
[<CompiledName "AlternateLookup">]
val alternateLookup<'Alternate, 'Key, 'Value when 'Key : not null and 'Alternate : not null> :
    table : FrozenMap<'Key, 'Value> -> FrozenMap<'Key, 'Value>.AlternateLookup<'Alternate>

/// <summary>
/// Returns a lookup of the bindings of the frozen map by keys of another type, or <c>ValueNone</c> when the frozen map
/// cannot look its keys up by that type.
/// </summary>
/// <param name="table">The input frozen map.</param>
/// <returns>
/// The lookup, whose
/// <see cref="M:System.Collections.Frozen.FrozenDictionary`2.AlternateLookup`1.ContainsKey(`2)"/>,
/// <see cref="M:System.Collections.Frozen.FrozenDictionary`2.AlternateLookup`1.TryGetValue(`2,`1@)"/> and
/// <see cref="P:System.Collections.Frozen.FrozenDictionary`2.AlternateLookup`1.Item(`2)"/> find a binding by an alternate
/// key; <c>ValueNone</c> when the key comparer of the frozen map does not implement
/// <see cref="T:System.Collections.Generic.IAlternateEqualityComparer`2"/> of the alternate key type and the key type, or
/// when the keys are strings, which a frozen map looks up by spans of characters only.
/// </returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// <para>
/// O(1). The function wraps
/// <see cref="M:System.Collections.Frozen.FrozenDictionary`2.TryGetAlternateLookup``1(System.Collections.Frozen.FrozenDictionary{`0,`1}.AlternateLookup{``0}@)"/>.
/// The type of the alternate keys is the first type argument, so that a call names it and leaves the others to inference:
/// <c>FrozenMap.tryAlternateLookup&lt;int, _, _&gt; table</c>.
/// </para>
/// <para>
/// The function is on <c>net10.0</c> only: the BCL member comes with .NET 9, and the library has no <c>net9.0</c> target
/// (ADR 0001, D11).
/// </para>
/// <para>
/// An F# generic function cannot be instantiated with a byref-like type, so the function cannot look the keys up by a
/// span, such as the <see cref="T:System.ReadOnlySpan`1"/> of characters that string keys take with
/// <see cref="P:System.StringComparer.Ordinal"/>. For a span, call
/// <see cref="M:System.Collections.Frozen.FrozenDictionary`2.TryGetAlternateLookup``1(System.Collections.Frozen.FrozenDictionary{`0,`1}.AlternateLookup{``0}@)"/>
/// on the frozen map itself.
/// </para>
/// </remarks>
[<CompiledName "TryAlternateLookup">]
val tryAlternateLookup<'Alternate, 'Key, 'Value when 'Key : not null and 'Alternate : not null> :
    table : FrozenMap<'Key, 'Value> -> FrozenMap<'Key, 'Value>.AlternateLookup<'Alternate> voption
#endif
