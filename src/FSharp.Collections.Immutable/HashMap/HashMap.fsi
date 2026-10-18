/// <summary>
/// Functions for <see cref="T:FSharp.Collections.Immutable.HashMap`2"/>, the immutable hash map.
/// </summary>
/// <remarks>
/// <para>
/// The module mirrors <see cref="T:Microsoft.FSharp.Collections.MapModule"/>: each function with a name of that module has
/// its semantics, its argument checks and its exceptions (ADR 0001), except
/// <see cref="M:FSharp.Collections.Immutable.HashMap.Add``2(``0,``1,System.Collections.Immutable.ImmutableDictionary{``0,``1})"/>.
/// It is the <see cref="M:System.Collections.Immutable.ImmutableDictionary`2.Add(`0,`1)"/> of the BCL, as it was in 1.0.0:
/// it throws for a key that is bound to another value, where <c>Map.add</c> replaces the value, and
/// <see cref="M:FSharp.Collections.Immutable.HashMap.SetItem``2(``0,``1,System.Collections.Immutable.ImmutableDictionary{``0,``1})"/>
/// replaces it (ADR 0001, D14). The other functions wrap a member of
/// <see cref="T:System.Collections.Immutable.ImmutableDictionary`2"/> and keep its semantics and its exceptions.
/// </para>
/// <para>
/// A hash map enumerates its bindings in an unspecified order, which stays the same for the same map. The functions that
/// visit the bindings one by one follow that order, where <see cref="T:Microsoft.FSharp.Collections.MapModule"/> follows
/// the order of the keys.
/// </para>
/// <para>
/// A hash map compares its keys and its values with the equality comparers that it was created with. The functions of the
/// module create hash maps with the default comparers, <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>;
/// <see cref="M:FSharp.Collections.Immutable.HashMap.ForComparers``2(System.Collections.Generic.IEqualityComparer{``0},System.Collections.Generic.IEqualityComparer{``1})"/>
/// and <see cref="M:FSharp.Collections.Immutable.HashMap.ForKeyComparer``2(System.Collections.Generic.IEqualityComparer{``0})"/>
/// return a <see cref="T:FSharp.Collections.Immutable.CustomComparersHashMap`2"/>, which creates hash maps with other
/// comparers (ADR 0001, D12). Every transformation keeps the key comparer. The transformations that keep the type of the
/// values keep the value comparer, and the transformations that change it give the new values the default comparer of
/// their type.
/// </para>
/// <para>
/// A new value of a key that the value comparer finds equal to the current one leaves the binding as it is, its key
/// included, in the functions that set bindings and in the creation functions, as in
/// <see cref="T:System.Collections.Immutable.ImmutableDictionary`2"/> (ADR 0001, D14).
/// </para>
/// <para>
/// The single-key idioms of the BCL are functions of the module or compositions of them. <c>TryGetValue</c> is
/// <see cref="M:FSharp.Collections.Immutable.HashMap.TryFind``2(``0,System.Collections.Immutable.ImmutableDictionary{``0,``1})"/>,
/// and <c>GetValueOrDefault</c> is <c>HashMap.tryFind key table |> ValueOption.defaultValue defaultValue</c>. Adding a
/// binding only when the key has none, as <c>ImmutableInterlocked.TryAdd</c> does, is
/// <see cref="M:FSharp.Collections.Immutable.HashMap.Change``2(``0,Microsoft.FSharp.Core.FSharpFunc{Microsoft.FSharp.Core.FSharpValueOption{``1},Microsoft.FSharp.Core.FSharpValueOption{``1}},System.Collections.Immutable.ImmutableDictionary{``0,``1})"/>
/// with a function that keeps an existing value. The other members of <c>ImmutableInterlocked</c> replace a field
/// atomically and stay direct calls with a reference to the field.
/// </para>
/// <para>
/// A builder is the BCL builder: the module creates it (<c>builder</c>, <c>toBuilder</c>, <c>build</c> and <c>update</c>)
/// and turns it into a hash map (<c>ofBuilder</c>), and the members of the builder, such as <c>Add</c>, <c>AddRange</c>,
/// <c>Remove</c>, <c>RemoveRange</c> and the indexer, are called on it. There is no builder module for maps (ADR 0001, D2).
/// </para>
/// <para>
/// A binding comes as a key/value pair, a reference tuple or a struct tuple (ADR 0001, D13). <c>HashMap.ofArray</c>,
/// <c>HashMap.ofList</c> and <c>HashMap.ofSeq</c> take every shape: they are the static members of
/// <see cref="T:FSharp.Collections.Immutable.HashMapExtensions"/>, which the module leaves visible by having no functions
/// with their names. <see cref="M:FSharp.Collections.Immutable.HashMap.AddRange``2(System.Collections.Generic.IEnumerable{System.Collections.Generic.KeyValuePair{``0,``1}},System.Collections.Immutable.ImmutableDictionary{``0,``1})"/>
/// and <see cref="M:FSharp.Collections.Immutable.HashMap.SetItems``2(System.Collections.Generic.IEnumerable{System.Collections.Generic.KeyValuePair{``0,``1}},System.Collections.Immutable.ImmutableDictionary{``0,``1})"/>
/// take key/value pairs, and the nested modules <see cref="T:FSharp.Collections.Immutable.HashMap.Tuple"/> and
/// <see cref="T:FSharp.Collections.Immutable.HashMap.ValueTuple"/> take reference and struct tuples.
/// </para>
/// <para>
/// A <see langword="null"/> map, sequence, builder or comparer throws <see cref="T:System.ArgumentNullException"/>, and so
/// does a default flat list that is given as a sequence: it fails when a function enumerates it, so it is as good as a
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
module FSharp.Collections.Immutable.HashMap

open System
open System.Collections.Generic
open System.Collections.Immutable

// ---- Creation ----

/// <summary>
/// The empty hash map, with the default comparers.
/// </summary>
/// <remarks>
/// O(1). The value is <see cref="F:System.Collections.Immutable.ImmutableDictionary`2.Empty"/>.
/// </remarks>
[<GeneralizableValue>]
[<CompiledName "Empty">]
val empty<'Key, 'Value when 'Key : not null> : HashMap<'Key, 'Value>

/// <summary>
/// Holds equality comparers of the keys and the values, to create hash maps with them.
/// </summary>
/// <param name="keyComparer">The equality comparer of the keys.</param>
/// <param name="valueComparer">The equality comparer of the values.</param>
/// <returns>The struct whose members create hash maps with the comparers.</returns>
/// <exception cref="T:System.ArgumentNullException">
/// Thrown when <paramref name="keyComparer"/> or <paramref name="valueComparer"/> is <see langword="null"/>.
/// </exception>
/// <remarks>
/// <para>
/// O(1); the struct allocates nothing. Its members have the names of the creation functions of the module (ADR 0001, D12).
/// </para>
/// <para>
/// The <c>ofSeq</c> member of the struct also gives an existing hash map these comparers, which is how to change the
/// comparers of a map. Unlike <see cref="M:System.Collections.Immutable.ImmutableDictionary`2.WithComparers(System.Collections.Generic.IEqualityComparer{`0},System.Collections.Generic.IEqualityComparer{`1})"/>
/// it rebuilds the map unless the key comparer is the same instance, and it merges keys that become equal instead of
/// throwing; the remarks of <see cref="T:FSharp.Collections.Immutable.CustomComparersHashMap`2"/> have the details.
/// </para>
/// </remarks>
/// <example id="hashMap-forComparers-1">
/// <code lang="fsharp">
/// let comparers = HashMap.forComparers StringComparer.OrdinalIgnoreCase StringComparer.Ordinal
/// let table = comparers.ofSeq [ "one", "uno"; "two", "dos" ]
/// </code>
/// Evaluates to a hash map whose keys ignore the case and whose values compare ordinally.
/// </example>
/// <example id="hashMap-forComparers-2">
/// <code lang="fsharp">
/// let ignoreCase = (HashMap.forComparers StringComparer.OrdinalIgnoreCase (HashMap.valueComparer table)).ofSeq table
/// </code>
/// Evaluates to a hash map with the bindings of <c>table</c> whose keys ignore the case and whose values keep the value
/// comparer of <c>table</c>.
/// </example>
[<CompiledName "ForComparers">]
val forComparers :
    keyComparer : IEqualityComparer<'Key> -> valueComparer : IEqualityComparer<'Value> -> CustomComparersHashMap<'Key, 'Value>

/// <summary>
/// Holds an equality comparer of the keys, to create hash maps with it.
/// </summary>
/// <param name="keyComparer">The equality comparer of the keys.</param>
/// <returns>
/// The struct whose members create hash maps with the key comparer, and with the default comparer of the values.
/// </returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="keyComparer"/> is <see langword="null"/>.</exception>
/// <remarks>
/// <para>
/// O(1); the struct allocates nothing. Its members have the names of the creation functions of the module (ADR 0001, D12).
/// </para>
/// <para>
/// The <c>ofSeq</c> member of the struct also gives an existing hash map this key comparer, with the default comparer of
/// the values: the value comparer of the map is replaced, where
/// <see cref="M:System.Collections.Immutable.ImmutableDictionary`2.WithComparers(System.Collections.Generic.IEqualityComparer{`0})"/>
/// keeps it. Use
/// <see cref="M:FSharp.Collections.Immutable.HashMap.ForComparers``2(System.Collections.Generic.IEqualityComparer{``0},System.Collections.Generic.IEqualityComparer{``1})"/>
/// with the value comparer of the map to keep it.
/// </para>
/// </remarks>
/// <example id="hashMap-forKeyComparer-1">
/// <code lang="fsharp">
/// let table = (HashMap.forKeyComparer StringComparer.OrdinalIgnoreCase).ofSeq [ "one", 1; "ONE", 2 ]
/// </code>
/// Evaluates to a hash map whose keys ignore the case, with the binding of "ONE" to 2.
/// </example>
[<CompiledName "ForKeyComparer">]
val forKeyComparer : keyComparer : IEqualityComparer<'Key> -> CustomComparersHashMap<'Key, 'Value>

/// <summary>
/// Creates a hash map from a copy of a read-only span of key/value pairs.
/// </summary>
/// <param name="span">The input span of key/value pairs.</param>
/// <returns>The hash map of the bindings, with the default comparers.</returns>
/// <remarks>
/// <para>
/// O(n log n). A later binding of a key replaces an earlier one, unless the default comparer of the values finds the two
/// values equal: then the earlier binding stays, its key included (ADR 0001, D14). A repeated key never throws.
/// </para>
/// <para>
/// The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableDictionary.CreateRangeWithOverwrite``2(System.ReadOnlySpan{System.Collections.Generic.KeyValuePair{``0,``1}})"/>
/// through <see cref="M:FSharp.Collections.Immutable.CustomComparersHashMap`2.OfReadOnlySpan(System.ReadOnlySpan{System.Collections.Generic.KeyValuePair{`0,`1}})"/>.
/// A span has one shape, so there are no overloads for tuples (ADR 0001, D13).
/// </para>
/// <para>
/// A <see cref="T:System.Span`1"/> goes to
/// <see cref="M:FSharp.Collections.Immutable.HashMap.OfSpan``2(System.Span{System.Collections.Generic.KeyValuePair{``0,``1}})"/>,
/// which needs no annotation. An array converts to the parameter only when the types of the call are known, from an
/// annotation of the result or from explicit type arguments; F# then reports the conversion with the warning FS3391.
/// Otherwise F# reports a type mismatch, and the argument must be a <see cref="T:System.ReadOnlySpan`1"/>. The function
/// cannot be used as a value or with the pipeline operator, because a span cannot be a type argument.
/// </para>
/// </remarks>
[<CompiledName "OfReadOnlySpan">]
val ofReadOnlySpan : span : ReadOnlySpan<KeyValuePair<'Key, 'Value>> -> HashMap<'Key, 'Value>

/// <summary>
/// Creates a hash map of the elements of a sequence under the keys that a projection returns for them.
/// </summary>
/// <param name="projection">The function that returns the key of an element.</param>
/// <param name="source">The input sequence, whose elements become the values.</param>
/// <returns>The hash map of the elements under their keys.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="source"/> is <see langword="null"/> or a default flat list.</exception>
/// <exception cref="T:System.ArgumentException">
/// Thrown when two elements have the same key and the default comparer of the values finds them different.
/// </exception>
/// <remarks>
/// O(n log n). The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableDictionary.ToImmutableDictionary``2(System.Collections.Generic.IEnumerable{``0},System.Func{``0,``1})"/>,
/// which rejects a repeated key with another element and accepts one with an element that the default comparer of the
/// values finds equal: the first element stays, its key included (ADR 0001, D14). The value of a binding is the element
/// itself; with an element selector, as <c>ToImmutableDictionary</c> takes one,
/// <c>HashMap.empty |> HashMap.addRange (source |> Seq.map (fun x -> KeyValuePair (key x, element x)))</c> rejects a
/// repeated key like the BCL does, and <c>HashMap.ofSeq</c> of the mapped pairs lets the last binding win.
/// </remarks>
[<CompiledName "OfSeqBy">]
val ofSeqBy : projection : ('Value -> 'Key) -> source : seq<'Value> -> HashMap<'Key, 'Value>

/// <summary>
/// Creates a hash map from a copy of a span of key/value pairs.
/// </summary>
/// <param name="span">The input span of key/value pairs.</param>
/// <returns>The hash map of the bindings, with the default comparers.</returns>
/// <remarks>
/// <para>
/// O(n log n). The BCL creates from a read-only span only, so the function converts the span without a copy and creates the
/// hash map as
/// <see cref="M:FSharp.Collections.Immutable.HashMap.OfReadOnlySpan``2(System.ReadOnlySpan{System.Collections.Generic.KeyValuePair{``0,``1}})"/>
/// does: a later binding of a key replaces an earlier one, unless the default comparer of the values finds the two values
/// equal, and a repeated key never throws (ADR 0001, D14).
/// </para>
/// <para>
/// An array converts to the parameter only when the types of the call are known, from an annotation of the result or from
/// explicit type arguments. The function cannot be used as a value or with the pipeline operator, because a span cannot be
/// a type argument.
/// </para>
/// </remarks>
[<CompiledName "OfSpan">]
val ofSpan : span : Span<KeyValuePair<'Key, 'Value>> -> HashMap<'Key, 'Value>

/// <summary>Creates a hash map of one binding.</summary>
/// <param name="key">The key.</param>
/// <param name="value">The value.</param>
/// <returns>The hash map of the binding, with the default comparers.</returns>
/// <remarks>O(1).</remarks>
[<CompiledName "Singleton">]
val singleton : key : 'Key -> value : 'Value -> HashMap<'Key, 'Value>

// ---- Builder ----

/// <summary>
/// Creates a hash map with a builder that an action fills.
/// </summary>
/// <param name="action">The action that sets the bindings in the builder, which has the default comparers.</param>
/// <returns>The hash map of the bindings that the action set, with the default comparers.</returns>
/// <remarks>
/// <para>
/// O(1) plus the cost of the action, which sets each binding of the builder in O(log n), plus freezing the nodes that the
/// action changed: the hash map shares the nodes of the builder instead of copying the entries. The function creates the
/// builder with <see cref="M:System.Collections.Immutable.ImmutableDictionary.CreateBuilder``2"/> and returns it with
/// <see cref="M:System.Collections.Immutable.ImmutableDictionary`2.Builder.ToImmutable"/>, as
/// <see cref="M:FSharp.Collections.Immutable.CustomComparersHashMap`2.Build(Microsoft.FSharp.Core.FSharpFunc{System.Collections.Immutable.ImmutableDictionary.Builder{`0,`1},Microsoft.FSharp.Core.Unit})"/>
/// does for other comparers.
/// </para>
/// <para>
/// The indexer of the builder keeps the earlier binding of a key for a value that the value comparer finds equal, its key
/// included (ADR 0001, D14).
/// </para>
/// </remarks>
/// <example id="hashMap-build-1">
/// <code lang="fsharp">
/// let table = HashMap.build (fun builder -> for i in 1..3 do builder[i] &lt;- i * i)
/// </code>
/// Evaluates to a hash map of 1 to 1, 2 to 4 and 3 to 9.
/// </example>
[<CompiledName "Build">]
val build : action : (HashMap<'Key, 'Value>.Builder -> unit) -> HashMap<'Key, 'Value>

/// <summary>
/// Creates an empty builder with the default comparers.
/// </summary>
/// <returns>The builder.</returns>
/// <remarks>
/// O(1). The function wraps <see cref="M:System.Collections.Immutable.ImmutableDictionary.CreateBuilder``2"/>.
/// </remarks>
[<CompiledName "CreateBuilder">]
val builder : unit -> HashMap<'Key, 'Value>.Builder

/// <summary>
/// Returns the contents of a builder as a hash map.
/// </summary>
/// <param name="builder">The builder, which stays usable.</param>
/// <returns>The hash map of the bindings of the builder, with its comparers.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="builder"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(1) apart from freezing the nodes that changed since the builder last returned a hash map: the hash map shares the nodes
/// of the builder instead of copying the entries. The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableDictionary`2.Builder.ToImmutable"/>.
/// </remarks>
[<CompiledName "OfBuilder">]
val ofBuilder : builder : HashMap<'Key, 'Value>.Builder -> HashMap<'Key, 'Value>

/// <summary>
/// Creates a builder that contains the bindings of a hash map.
/// </summary>
/// <param name="table">The input hash map, which stays unchanged.</param>
/// <returns>The builder, with the comparers of the hash map.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(1). The function wraps <see cref="M:System.Collections.Immutable.ImmutableDictionary`2.ToBuilder"/>.
/// </remarks>
[<CompiledName "ToBuilder">]
val toBuilder : table : HashMap<'Key, 'Value> -> HashMap<'Key, 'Value>.Builder

/// <summary>
/// Creates a hash map from a builder over the bindings of a hash map, after an action changes the builder.
/// </summary>
/// <param name="action">The action that changes the builder, which has the comparers of the hash map.</param>
/// <param name="table">The input hash map, which stays unchanged.</param>
/// <returns>
/// The hash map of the changed builder, with the comparers of the input; the input itself when the action changed nothing.
/// </returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(1) plus the cost of the action, which changes each binding of the builder in O(log n), plus freezing the nodes that
/// the action changed: the hash map shares the nodes that the action left unchanged with the input instead of copying the
/// entries. The function creates the builder with
/// <see cref="M:System.Collections.Immutable.ImmutableDictionary`2.ToBuilder"/> and returns it with
/// <see cref="M:System.Collections.Immutable.ImmutableDictionary`2.Builder.ToImmutable"/>.
/// </remarks>
/// <example id="hashMap-update-1">
/// <code lang="fsharp">
/// let squares = HashMap.build (fun builder -> for i in 1..3 do builder[i] &lt;- i * i)
/// let table = squares |> HashMap.update (fun builder -> builder.Remove 1 |> ignore; builder[4] &lt;- 16)
/// </code>
/// Evaluates to a hash map of 2 to 4, 3 to 9 and 4 to 16, and <c>squares</c> stays unchanged.
/// </example>
[<CompiledName "Update">]
val update : action : (HashMap<'Key, 'Value>.Builder -> unit) -> table : HashMap<'Key, 'Value> -> HashMap<'Key, 'Value>

// ---- Properties ----

/// <summary>Returns the number of bindings of the hash map.</summary>
/// <param name="table">The input hash map.</param>
/// <returns>The number of bindings.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>O(1).</remarks>
[<CompiledName "Count">]
val count : table : HashMap<'Key, 'Value> -> int

/// <summary>Returns whether the hash map has no bindings.</summary>
/// <param name="table">The input hash map.</param>
/// <returns><see langword="true"/> when the hash map is empty.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>O(1).</remarks>
[<CompiledName "IsEmpty">]
val isEmpty : table : HashMap<'Key, 'Value> -> bool

/// <summary>
/// Returns the equality comparer of the keys.
/// </summary>
/// <param name="table">The input hash map.</param>
/// <returns>The key comparer.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(1). The function wraps <see cref="P:System.Collections.Immutable.ImmutableDictionary`2.KeyComparer"/>.
/// </remarks>
[<CompiledName "KeyComparer">]
val keyComparer : table : HashMap<'Key, 'Value> -> IEqualityComparer<'Key>

/// <summary>Returns the keys of the hash map.</summary>
/// <param name="table">The input hash map.</param>
/// <returns>
/// A read-only view of the keys in the order of enumeration of the hash map, whose count takes O(1) and whose lookup
/// takes O(log n) in the key comparer.
/// </returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>O(1).</remarks>
[<CompiledName "Keys">]
val keys : table : HashMap<'Key, 'Value> -> ICollection<'Key>

/// <summary>
/// Returns the equality comparer of the values.
/// </summary>
/// <param name="table">The input hash map.</param>
/// <returns>The value comparer.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(1). The function wraps <see cref="P:System.Collections.Immutable.ImmutableDictionary`2.ValueComparer"/>.
/// </remarks>
[<CompiledName "ValueComparer">]
val valueComparer : table : HashMap<'Key, 'Value> -> IEqualityComparer<'Value>

/// <summary>
/// Returns the values of the hash map, including the duplicates.
/// </summary>
/// <param name="table">The input hash map.</param>
/// <returns>
/// A read-only view of the values in the order of enumeration of the hash map, whose count takes O(1) and whose lookup
/// takes O(n) in the value comparer.
/// </returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(1).
/// </remarks>
[<CompiledName "Values">]
val values : table : HashMap<'Key, 'Value> -> ICollection<'Value>

// ---- Access ----

/// <summary>Returns the value of a key.</summary>
/// <param name="key">The key to find.</param>
/// <param name="table">The input hash map.</param>
/// <returns>The value of the key.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <exception cref="T:System.Collections.Generic.KeyNotFoundException">Thrown when the hash map has no binding of the key.</exception>
/// <remarks>O(log n).</remarks>
[<CompiledName "Find">]
val find : key : 'Key -> table : HashMap<'Key, 'Value> -> 'Value

/// <summary>
/// Returns the value of a key, or <c>ValueNone</c> when the hash map has no binding of the key.
/// </summary>
/// <param name="key">The key to find.</param>
/// <param name="table">The input hash map.</param>
/// <returns>The value of the key, or <c>ValueNone</c>.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(log n). The function is the <c>TryGetValue</c> of the BCL with a <c>voption</c> instead of the <c>out</c> parameter.
/// <c>GetValueOrDefault</c> is <c>HashMap.tryFind key table |> ValueOption.defaultValue defaultValue</c>: one lookup,
/// and a <c>voption</c> is a struct.
/// </remarks>
[<CompiledName "TryFind">]
val tryFind : key : 'Key -> table : HashMap<'Key, 'Value> -> 'Value voption

/// <summary>
/// Returns the key of the hash map that equals a key in the key comparer, or <c>ValueNone</c>.
/// </summary>
/// <param name="key">The key to find.</param>
/// <param name="table">The input hash map.</param>
/// <returns>The key that the hash map holds, which can differ from the given one, or <c>ValueNone</c>.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(log n). The function wraps <see cref="M:System.Collections.Immutable.ImmutableDictionary`2.TryGetKey(`0,`0@)"/>.
/// </remarks>
[<CompiledName "TryGetKey">]
val tryGetKey : key : 'Key -> table : HashMap<'Key, 'Value> -> 'Key voption

// ---- Search ----

/// <summary>
/// Returns whether the hash map binds a key to a value, comparing the values with the value comparer.
/// </summary>
/// <param name="key">The key to find.</param>
/// <param name="value">The value that the key must be bound to.</param>
/// <param name="table">The input hash map.</param>
/// <returns>
/// <see langword="true"/> when the hash map has a binding of the key whose value the value comparer finds equal to
/// <paramref name="value"/>.
/// </returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// <para>
/// O(log n). The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableDictionary`2.Contains(System.Collections.Generic.KeyValuePair{`0,`1})"/>.
/// A key without a binding gives <see langword="false"/>. The member does not check a <see langword="null"/> key, as
/// <see cref="M:FSharp.Collections.Immutable.HashMap.ContainsKey``2(``0,System.Collections.Immutable.ImmutableDictionary{``0,``1})"/>
/// does, so the key comparer decides: the default comparer finds no binding and the function gives
/// <see langword="false"/>, and a comparer that rejects <see langword="null"/>, such as
/// <see cref="P:System.StringComparer.Ordinal"/>, throws <see cref="T:System.ArgumentNullException"/>.
/// </para>
/// <para>
/// The value comparer of the map decides whether the values are equal. No composition of the other functions asks it:
/// <c>HashMap.tryFind</c> followed by <c>=</c> compares with F# equality, which for a comparer that ignores the case finds
/// <c>"Hello"</c> and <c>"HELLO"</c> different.
/// </para>
/// </remarks>
/// <example id="hashMap-containsBinding-1">
/// <code lang="fsharp">
/// let table = (HashMap.forComparers StringComparer.Ordinal StringComparer.OrdinalIgnoreCase).ofSeq [ "one", "Uno"; "two", "Dos" ]
/// let found = table |> HashMap.containsBinding "one" "UNO"
/// </code>
/// Evaluates to <c>true</c>, where <c>HashMap.tryFind "one" table</c> is <c>ValueSome "Uno"</c>.
/// </example>
[<CompiledName "ContainsBinding">]
val containsBinding : key : 'Key -> value : 'Value -> table : HashMap<'Key, 'Value> -> bool

/// <summary>Returns whether the hash map has a binding of a key.</summary>
/// <param name="key">The key to find.</param>
/// <param name="table">The input hash map.</param>
/// <returns><see langword="true"/> when the hash map has a binding of the key.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>O(log n).</remarks>
[<CompiledName "ContainsKey">]
val containsKey : key : 'Key -> table : HashMap<'Key, 'Value> -> bool

/// <summary>
/// Returns whether the hash map has a binding of a value, in the value comparer.
/// </summary>
/// <param name="value">The value to find.</param>
/// <param name="table">The input hash map.</param>
/// <returns><see langword="true"/> when a value of the hash map equals the given one.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n). The function wraps <see cref="M:System.Collections.Immutable.ImmutableDictionary`2.ContainsValue(`1)"/>.
/// </remarks>
[<CompiledName "ContainsValue">]
val containsValue : value : 'Value -> table : HashMap<'Key, 'Value> -> bool

/// <summary>Returns whether any binding satisfies the predicate.</summary>
/// <param name="predicate">The function to test the bindings.</param>
/// <param name="table">The input hash map.</param>
/// <returns><see langword="true"/> when a binding satisfies the predicate.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>O(n).</remarks>
[<CompiledName "Exists">]
val exists : predicate : ('Key -> 'Value -> bool) -> table : HashMap<'Key, 'Value> -> bool

/// <summary>
/// Returns the key of the first binding, in the order of enumeration, that satisfies the predicate.
/// </summary>
/// <param name="predicate">The function to test the bindings.</param>
/// <param name="table">The input hash map.</param>
/// <returns>The key of the first binding that satisfies the predicate.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <exception cref="T:System.Collections.Generic.KeyNotFoundException">Thrown when no binding satisfies the predicate.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "FindKey">]
val findKey : predicate : ('Key -> 'Value -> bool) -> table : HashMap<'Key, 'Value> -> 'Key

/// <summary>Returns whether all bindings satisfy the predicate.</summary>
/// <param name="predicate">The function to test the bindings.</param>
/// <param name="table">The input hash map.</param>
/// <returns><see langword="true"/> when every binding satisfies the predicate.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>O(n).</remarks>
[<CompiledName "ForAll">]
val forall : predicate : ('Key -> 'Value -> bool) -> table : HashMap<'Key, 'Value> -> bool

/// <summary>
/// Returns the first value, in the order of enumeration, that the chooser returns for the bindings.
/// </summary>
/// <param name="chooser">The function that returns a value, or <c>ValueNone</c> to go on.</param>
/// <param name="table">The input hash map.</param>
/// <returns>The first chosen value.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <exception cref="T:System.Collections.Generic.KeyNotFoundException">Thrown when the chooser returns <c>ValueNone</c> for every binding.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Pick">]
val pick : chooser : ('Key -> 'Value -> 'U voption) -> table : HashMap<'Key, 'Value> -> 'U

/// <summary>
/// Returns the key of the first binding, in the order of enumeration, that satisfies the predicate, or <c>ValueNone</c>.
/// </summary>
/// <param name="predicate">The function to test the bindings.</param>
/// <param name="table">The input hash map.</param>
/// <returns>The key of the first binding that satisfies the predicate, or <c>ValueNone</c>.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "TryFindKey">]
val tryFindKey : predicate : ('Key -> 'Value -> bool) -> table : HashMap<'Key, 'Value> -> 'Key voption

/// <summary>
/// Returns the first value, in the order of enumeration, that the chooser returns for the bindings, or <c>ValueNone</c>.
/// </summary>
/// <param name="chooser">The function that returns a value, or <c>ValueNone</c> to go on.</param>
/// <param name="table">The input hash map.</param>
/// <returns>The first chosen value, or <c>ValueNone</c>.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "TryPick">]
val tryPick : chooser : ('Key -> 'Value -> 'U voption) -> table : HashMap<'Key, 'Value> -> 'U voption

// ---- Transformation ----

/// <summary>
/// Returns a hash map with a binding of a key to a value, for a key that is not bound to another value.
/// </summary>
/// <param name="key">The key.</param>
/// <param name="value">The value.</param>
/// <param name="table">The input hash map.</param>
/// <returns>The hash map with the binding; the input itself when it already has the binding.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <exception cref="T:System.ArgumentException">
/// Thrown when the hash map binds the key to a value that the value comparer finds different.
/// </exception>
/// <remarks>
/// <para>
/// O(log n). The function wraps <see cref="M:System.Collections.Immutable.ImmutableDictionary`2.Add(`0,`1)"/> and
/// behaves as it does (ADR 0001, D14), as the 1.0.0 function did: it does not replace the value of a key, where
/// <c>Map.add</c> does. When the value comparer of the hash map finds the value equal to the current value of the key, the
/// binding stays as it is, its key included, and the input itself comes back.
/// </para>
/// <para>
/// Use <see cref="M:FSharp.Collections.Immutable.HashMap.SetItem``2(``0,``1,System.Collections.Immutable.ImmutableDictionary{``0,``1})"/>
/// to replace the value of a key, and
/// <see cref="M:FSharp.Collections.Immutable.HashMap.Change``2(``0,Microsoft.FSharp.Core.FSharpFunc{Microsoft.FSharp.Core.FSharpValueOption{``1},Microsoft.FSharp.Core.FSharpValueOption{``1}},System.Collections.Immutable.ImmutableDictionary{``0,``1})"/>
/// to add a binding only when the key has none, or to compute the new value from the current one.
/// </para>
/// </remarks>
[<CompiledName "Add">]
val add : key : 'Key -> value : 'Value -> table : HashMap<'Key, 'Value> -> HashMap<'Key, 'Value>

/// <summary>
/// Returns a hash map with the bindings of key/value pairs added.
/// </summary>
/// <param name="pairs">The key/value pairs to add.</param>
/// <param name="table">The input hash map.</param>
/// <returns>The hash map with the bindings.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="pairs"/> is <see langword="null"/> or a default flat list, or <paramref name="table"/> is <see langword="null"/>.</exception>
/// <exception cref="T:System.ArgumentException">
/// Thrown when a pair has the key of a binding of the hash map or of another pair, and a value that the value comparer
/// finds different.
/// </exception>
/// <remarks>
/// O(m log(n + m)). The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableDictionary`2.AddRange(System.Collections.Generic.IEnumerable{System.Collections.Generic.KeyValuePair{`0,`1}})"/>,
/// which rejects another value of a key and accepts a pair whose value the value comparer finds equal to the value of the
/// key, leaving that binding as it is, its key included (ADR 0001, D14). Use
/// <see cref="M:FSharp.Collections.Immutable.HashMap.SetItems``2(System.Collections.Generic.IEnumerable{System.Collections.Generic.KeyValuePair{``0,``1}},System.Collections.Immutable.ImmutableDictionary{``0,``1})"/>
/// to replace the values, and <see cref="T:FSharp.Collections.Immutable.HashMap.Tuple"/> and
/// <see cref="T:FSharp.Collections.Immutable.HashMap.ValueTuple"/> to add tuples.
/// </remarks>
[<CompiledName "AddRange">]
val addRange : pairs : seq<KeyValuePair<'Key, 'Value>> -> table : HashMap<'Key, 'Value> -> HashMap<'Key, 'Value>

/// <summary>
/// Returns a hash map with the binding of a key changed by a function of its current value.
/// </summary>
/// <param name="key">The key.</param>
/// <param name="f">
/// The function that takes the current value or <c>ValueNone</c>, and returns the new value or <c>ValueNone</c> to remove
/// the binding.
/// </param>
/// <param name="table">The input hash map.</param>
/// <returns>The hash map with the changed binding; the input itself when nothing changes.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// <para>
/// O(log n). When <paramref name="f"/> returns a value that the value comparer of the hash map finds equal to the current
/// value, the binding stays as it is, its key included, as in
/// <see cref="M:System.Collections.Immutable.ImmutableDictionary`2.SetItem(`0,`1)"/> (ADR 0001, D14).
/// </para>
/// <para>
/// Adding a binding only when the key has none is <c>HashMap.change key (function ValueNone -> ValueSome value | existing -> existing)</c>,
/// which the BCL offers as <c>ImmutableInterlocked.TryAdd</c> for a field.
/// </para>
/// </remarks>
[<CompiledName "Change">]
val change : key : 'Key -> f : ('Value voption -> 'Value voption) -> table : HashMap<'Key, 'Value> -> HashMap<'Key, 'Value>

/// <summary>
/// Returns a hash map of the values that the chooser returns for the bindings, under their keys.
/// </summary>
/// <param name="chooser">The function that returns the new value, or <c>ValueNone</c> to drop the binding.</param>
/// <param name="table">The input hash map.</param>
/// <returns>
/// The hash map of the chosen values, with the key comparer of the input and the default comparer of the new values.
/// </returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n log n).
/// </remarks>
[<CompiledName "Choose">]
val choose : chooser : ('Key -> 'Value -> 'U voption) -> table : HashMap<'Key, 'Value> -> HashMap<'Key, 'U>

/// <summary>
/// Returns an empty hash map with the comparers of a hash map.
/// </summary>
/// <param name="table">The input hash map.</param>
/// <returns>The empty hash map; the input itself when it is empty.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(1). The function wraps <see cref="M:System.Collections.Immutable.ImmutableDictionary`2.Clear"/>.
/// </remarks>
[<CompiledName "Clear">]
val clear : table : HashMap<'Key, 'Value> -> HashMap<'Key, 'Value>

/// <summary>Returns the bindings that satisfy the predicate.</summary>
/// <param name="predicate">The function to test the bindings.</param>
/// <param name="table">The input hash map.</param>
/// <returns>The hash map of the bindings that satisfy the predicate, with the comparers of the input; the input itself when every binding does.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>O(n log n).</remarks>
[<CompiledName "Filter">]
val filter : predicate : ('Key -> 'Value -> bool) -> table : HashMap<'Key, 'Value> -> HashMap<'Key, 'Value>

/// <summary>
/// Returns the results of a function applied to each binding, under the keys of the bindings.
/// </summary>
/// <param name="mapping">The function to transform the bindings.</param>
/// <param name="table">The input hash map.</param>
/// <returns>
/// The hash map of the results, with the key comparer of the input and the default comparer of the new values.
/// </returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n log n).
/// </remarks>
[<CompiledName "Map">]
val map : mapping : ('Key -> 'Value -> 'U) -> table : HashMap<'Key, 'Value> -> HashMap<'Key, 'U>

/// <summary>Returns a hash map without the binding of a key.</summary>
/// <param name="key">The key to remove.</param>
/// <param name="table">The input hash map.</param>
/// <returns>The hash map without the binding; the input itself when it has no binding of the key.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>O(log n).</remarks>
[<CompiledName "Remove">]
val remove : key : 'Key -> table : HashMap<'Key, 'Value> -> HashMap<'Key, 'Value>

/// <summary>
/// Returns a hash map without the bindings of keys.
/// </summary>
/// <param name="keys">The keys to remove; a key without a binding is skipped.</param>
/// <param name="table">The input hash map.</param>
/// <returns>The hash map without the bindings.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="keys"/> is <see langword="null"/> or a default flat list, or <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(m log n). The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableDictionary`2.RemoveRange(System.Collections.Generic.IEnumerable{`0})"/>.
/// </remarks>
[<CompiledName "RemoveRange">]
val removeRange : keys : seq<'Key> -> table : HashMap<'Key, 'Value> -> HashMap<'Key, 'Value>

/// <summary>
/// Returns a hash map with a binding of a key to a value, which replaces an existing binding of the key.
/// </summary>
/// <param name="key">The key.</param>
/// <param name="value">The value.</param>
/// <param name="table">The input hash map.</param>
/// <returns>The hash map with the binding; the input itself when the binding stays as it is.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(log n). The function wraps <see cref="M:System.Collections.Immutable.ImmutableDictionary`2.SetItem(`0,`1)"/> and
/// replaces the value of a key, as <c>Map.add</c> does, where
/// <see cref="M:FSharp.Collections.Immutable.HashMap.Add``2(``0,``1,System.Collections.Immutable.ImmutableDictionary{``0,``1})"/>
/// throws for a key that is bound to another value. When the value comparer of the hash map finds the value equal to the
/// current value of the key, the binding stays as it is, its key included (ADR 0001, D14).
/// </remarks>
[<CompiledName "SetItem">]
val setItem : key : 'Key -> value : 'Value -> table : HashMap<'Key, 'Value> -> HashMap<'Key, 'Value>

/// <summary>
/// Returns a hash map with the bindings of key/value pairs, which replace the existing bindings of their keys.
/// </summary>
/// <param name="pairs">The key/value pairs to set; a later pair of a key replaces an earlier one.</param>
/// <param name="table">The input hash map.</param>
/// <returns>The hash map with the bindings.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="pairs"/> is <see langword="null"/> or a default flat list, or <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(m log(n + m)). The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableDictionary`2.SetItems(System.Collections.Generic.IEnumerable{System.Collections.Generic.KeyValuePair{`0,`1}})"/>.
/// A pair whose value the value comparer finds equal to the current value of its key, or to the value of an earlier pair
/// of the key, leaves that binding as it is, its key included (ADR 0001, D14).
/// <see cref="T:FSharp.Collections.Immutable.HashMap.Tuple"/> and <see cref="T:FSharp.Collections.Immutable.HashMap.ValueTuple"/>
/// set tuples.
/// </remarks>
[<CompiledName "SetItems">]
val setItems : pairs : seq<KeyValuePair<'Key, 'Value>> -> table : HashMap<'Key, 'Value> -> HashMap<'Key, 'Value>

// ---- Folding ----

/// <summary>
/// Applies a function to a state and each binding, in the order of enumeration.
/// </summary>
/// <param name="folder">The function that computes the next state.</param>
/// <param name="state">The initial state.</param>
/// <param name="table">The input hash map.</param>
/// <returns>The final state.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Fold">]
val fold : folder : ('State -> 'Key -> 'Value -> 'State) -> state : 'State -> table : HashMap<'Key, 'Value> -> 'State

/// <summary>
/// Applies a function to each binding and a state, in the reverse order of enumeration.
/// </summary>
/// <param name="folder">The function that computes the next state.</param>
/// <param name="table">The input hash map.</param>
/// <param name="state">The initial state.</param>
/// <returns>The final state.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n). The function visits the bindings from the last one that the hash map enumerates to the first one, so it sees
/// them in the reverse order of
/// <see cref="M:FSharp.Collections.Immutable.HashMap.Fold``3(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpFunc{``1,Microsoft.FSharp.Core.FSharpFunc{``2,``0}}},``0,System.Collections.Immutable.ImmutableDictionary{``1,``2})"/>,
/// not in the order of the keys. It copies the bindings first.
/// </remarks>
[<CompiledName "FoldBack">]
val foldBack : folder : ('Key -> 'Value -> 'State -> 'State) -> table : HashMap<'Key, 'Value> -> state : 'State -> 'State

/// <summary>
/// Applies an action to each binding, in the order of enumeration.
/// </summary>
/// <param name="action">The action.</param>
/// <param name="table">The input hash map.</param>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Iterate">]
val iter : action : ('Key -> 'Value -> unit) -> table : HashMap<'Key, 'Value> -> unit

// ---- Partitioning ----

/// <summary>Splits the bindings by a predicate.</summary>
/// <param name="predicate">The function to test the bindings.</param>
/// <param name="table">The input hash map.</param>
/// <returns>
/// The hash map of the bindings that satisfy the predicate and the hash map of the other ones, both with the comparers of
/// the input.
/// </returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>O(n log n).</remarks>
[<CompiledName "Partition">]
val partition :
    predicate : ('Key -> 'Value -> bool) ->
    table : HashMap<'Key, 'Value> ->
        struct (HashMap<'Key, 'Value> * HashMap<'Key, 'Value>)

// ---- Conversion ----

/// <summary>
/// Returns a new array of the bindings, in the order of enumeration.
/// </summary>
/// <param name="table">The input hash map.</param>
/// <returns>The array of key/value pairs.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "ToArray">]
val toArray : table : HashMap<'Key, 'Value> -> struct ('Key * 'Value) array

/// <summary>Returns a list of the bindings, in the order of enumeration.</summary>
/// <param name="table">The input hash map.</param>
/// <returns>The list of key/value pairs.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>O(n).</remarks>
[<CompiledName "ToList">]
val toList : table : HashMap<'Key, 'Value> -> struct ('Key * 'Value) list

/// <summary>
/// Returns the bindings as a sequence, in the order of enumeration.
/// </summary>
/// <param name="table">The input hash map.</param>
/// <returns>The sequence of key/value pairs, computed on demand.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(1); O(n) to enumerate.
/// </remarks>
[<CompiledName "ToSeq">]
val toSeq : table : HashMap<'Key, 'Value> -> seq<struct ('Key * 'Value)>

/// <summary>
/// The functions of <see cref="T:FSharp.Collections.Immutable.HashMap"/> that take bindings, with reference tuples instead of
/// key/value pairs.
/// </summary>
/// <remarks>
/// A function of the module takes one shape of the bindings, so the shape of the tuples of most F# code has a module of its
/// own (ADR 0001, D13): <c>HashMap.Tuple.addRange pairs table</c>. The functions resolve after
/// <c>open FSharp.Collections.Immutable.Tuple</c> too, whose <c>HashMap</c> module has no nested module of this name.
/// </remarks>
[<RequireQualifiedAccess>]
module Tuple =

    // ---- Transformation ----

    /// <summary>
    /// Returns a hash map with the bindings of key/value tuples added.
    /// </summary>
    /// <param name="pairs">The key/value tuples to add.</param>
    /// <param name="table">The input hash map.</param>
    /// <returns>The hash map with the bindings.</returns>
    /// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="pairs"/> is <see langword="null"/> or a default flat list, or <paramref name="table"/> is <see langword="null"/>.</exception>
    /// <exception cref="T:System.ArgumentException">
    /// Thrown when a tuple has the key of a binding of the hash map or of another tuple, and a value that the value
    /// comparer finds different.
    /// </exception>
    /// <remarks>
    /// O(m log(n + m)). The function wraps
    /// <see cref="M:System.Collections.Immutable.ImmutableDictionary`2.AddRange(System.Collections.Generic.IEnumerable{System.Collections.Generic.KeyValuePair{`0,`1}})"/>,
    /// which rejects another value of a key and accepts a tuple whose value the value comparer finds equal to the value of
    /// the key, leaving that binding as it is, its key included (ADR 0001, D14).
    /// </remarks>
    [<CompiledName "AddRange">]
    val addRange : pairs : seq<'Key * 'Value> -> table : HashMap<'Key, 'Value> -> HashMap<'Key, 'Value>

    /// <summary>
    /// Returns a hash map with the bindings of key/value tuples, which replace the existing bindings of their keys.
    /// </summary>
    /// <param name="pairs">The key/value tuples to set; a later tuple of a key replaces an earlier one.</param>
    /// <param name="table">The input hash map.</param>
    /// <returns>The hash map with the bindings.</returns>
    /// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="pairs"/> is <see langword="null"/> or a default flat list, or <paramref name="table"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// O(m log(n + m)). The function wraps
    /// <see cref="M:System.Collections.Immutable.ImmutableDictionary`2.SetItems(System.Collections.Generic.IEnumerable{System.Collections.Generic.KeyValuePair{`0,`1}})"/>.
    /// A tuple whose value the value comparer finds equal to the current value of its key, or to the value of an earlier
    /// tuple of the key, leaves that binding as it is, its key included (ADR 0001, D14).
    /// </remarks>
    [<CompiledName "SetItems">]
    val setItems : pairs : seq<'Key * 'Value> -> table : HashMap<'Key, 'Value> -> HashMap<'Key, 'Value>

/// <summary>
/// The functions of <see cref="T:FSharp.Collections.Immutable.HashMap"/> that take bindings, with struct tuples instead of
/// key/value pairs.
/// </summary>
/// <remarks>
/// A function of the module takes one shape of the bindings, so the struct tuples of ADR 0001, D3 have a module of their own
/// (D13): <c>HashMap.ValueTuple.setItems pairs table</c>.
/// </remarks>
[<RequireQualifiedAccess>]
module ValueTuple =

    // ---- Transformation ----

    /// <summary>
    /// Returns a hash map with the bindings of key/value tuples added.
    /// </summary>
    /// <param name="pairs">The key/value tuples to add.</param>
    /// <param name="table">The input hash map.</param>
    /// <returns>The hash map with the bindings.</returns>
    /// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="pairs"/> is <see langword="null"/> or a default flat list, or <paramref name="table"/> is <see langword="null"/>.</exception>
    /// <exception cref="T:System.ArgumentException">
    /// Thrown when a tuple has the key of a binding of the hash map or of another tuple, and a value that the value
    /// comparer finds different.
    /// </exception>
    /// <remarks>
    /// O(m log(n + m)). The function wraps
    /// <see cref="M:System.Collections.Immutable.ImmutableDictionary`2.AddRange(System.Collections.Generic.IEnumerable{System.Collections.Generic.KeyValuePair{`0,`1}})"/>,
    /// which rejects another value of a key and accepts a tuple whose value the value comparer finds equal to the value of
    /// the key, leaving that binding as it is, its key included (ADR 0001, D14).
    /// </remarks>
    [<CompiledName "AddRange">]
    val addRange : pairs : seq<struct ('Key * 'Value)> -> table : HashMap<'Key, 'Value> -> HashMap<'Key, 'Value>

    /// <summary>
    /// Returns a hash map with the bindings of key/value tuples, which replace the existing bindings of their keys.
    /// </summary>
    /// <param name="pairs">The key/value tuples to set; a later tuple of a key replaces an earlier one.</param>
    /// <param name="table">The input hash map.</param>
    /// <returns>The hash map with the bindings.</returns>
    /// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="pairs"/> is <see langword="null"/> or a default flat list, or <paramref name="table"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// O(m log(n + m)). The function wraps
    /// <see cref="M:System.Collections.Immutable.ImmutableDictionary`2.SetItems(System.Collections.Generic.IEnumerable{System.Collections.Generic.KeyValuePair{`0,`1}})"/>.
    /// A tuple whose value the value comparer finds equal to the current value of its key, or to the value of an earlier
    /// tuple of the key, leaves that binding as it is, its key included (ADR 0001, D14).
    /// </remarks>
    [<CompiledName "SetItems">]
    val setItems : pairs : seq<struct ('Key * 'Value)> -> table : HashMap<'Key, 'Value> -> HashMap<'Key, 'Value>
