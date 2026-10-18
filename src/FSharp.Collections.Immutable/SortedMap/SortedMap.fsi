/// <summary>
/// Functions for <see cref="T:FSharp.Collections.Immutable.SortedMap`2"/>, the immutable sorted map.
/// </summary>
/// <remarks>
/// <para>
/// The module mirrors <see cref="T:Microsoft.FSharp.Collections.MapModule"/>: each function with a name of that module has
/// its semantics, its argument checks and its exceptions (ADR 0001), except
/// <see cref="M:FSharp.Collections.Immutable.SortedMap.Add``2(``0,``1,System.Collections.Immutable.ImmutableSortedDictionary{``0,``1})"/>.
/// It is the <see cref="M:System.Collections.Immutable.ImmutableSortedDictionary`2.Add(`0,`1)"/> of the BCL: it throws for
/// a key that is bound to another value, where
/// <see cref="M:Microsoft.FSharp.Collections.MapModule.Add``2(``0,``1,Microsoft.FSharp.Collections.FSharpMap{``0,``1})"/>
/// replaces the value, and
/// <see cref="M:FSharp.Collections.Immutable.SortedMap.SetItem``2(``0,``1,System.Collections.Immutable.ImmutableSortedDictionary{``0,``1})"/>
/// replaces it (ADR 0001, D14). The other functions wrap a member of
/// <see cref="T:System.Collections.Immutable.ImmutableSortedDictionary`2"/> and keep its semantics and its exceptions.
/// </para>
/// <para>
/// A sorted map enumerates its bindings in the ascending order of the keys in its key comparer. The functions that visit
/// the bindings one by one follow that order, and the functions that visit them from the end follow the descending order,
/// as the functions of <see cref="T:Microsoft.FSharp.Collections.MapModule"/> do.
/// </para>
/// <para>
/// A sorted map orders its keys with the comparer that it was created with, and compares its values with an equality
/// comparer. The functions of the module create sorted maps that order their keys with F# generic comparison,
/// <see cref="M:Microsoft.FSharp.Core.LanguagePrimitives.FastGenericComparer``1"/>, as
/// <see cref="T:Microsoft.FSharp.Collections.FSharpMap`2"/> does, and not with
/// <see cref="P:System.Collections.Generic.Comparer`1.Default"/>: a sorted map of strings orders them ordinally, not by the
/// rules of a culture. The values compare with <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>.
/// <see cref="M:FSharp.Collections.Immutable.SortedMap.ForComparers``2(System.Collections.Generic.IComparer{``0},System.Collections.Generic.IEqualityComparer{``1})"/>
/// and <see cref="M:FSharp.Collections.Immutable.SortedMap.ForKeyComparer``2(System.Collections.Generic.IComparer{``0})"/>
/// return a <see cref="T:FSharp.Collections.Immutable.CustomComparersSortedMap`2"/>, which creates sorted maps with other
/// comparers (ADR 0001, D12). Every transformation keeps the key comparer. The transformations that keep the type of the
/// values keep the value comparer, and the transformations that change it give the new values the default comparer of
/// their type.
/// </para>
/// <para>
/// A new value of a key that the value comparer finds equal to the current one leaves the binding as it is, its key
/// included, in the functions that set bindings and in the creation functions, as in
/// <see cref="T:System.Collections.Immutable.ImmutableSortedDictionary`2"/> (ADR 0001, D14).
/// </para>
/// <para>
/// The single-key idioms of the BCL are functions of the module or compositions of them.
/// <see cref="M:System.Collections.Immutable.ImmutableSortedDictionary`2.TryGetValue(`0,`1@)"/> is
/// <see cref="M:FSharp.Collections.Immutable.SortedMap.TryFind``2(``0,System.Collections.Immutable.ImmutableSortedDictionary{``0,``1})"/>,
/// and <see cref="M:System.Collections.Immutable.ImmutableDictionary.GetValueOrDefault``2(System.Collections.Immutable.IImmutableDictionary{``0,``1},``0,``1)"/>
/// is <c>SortedMap.tryFind key table |> ValueOption.defaultValue defaultValue</c>. Adding a binding only when the key has
/// none is
/// <see cref="M:FSharp.Collections.Immutable.SortedMap.Change``2(``0,Microsoft.FSharp.Core.FSharpFunc{Microsoft.FSharp.Core.FSharpValueOption{``1},Microsoft.FSharp.Core.FSharpValueOption{``1}},System.Collections.Immutable.ImmutableSortedDictionary{``0,``1})"/>
/// with a function that keeps an existing value. <see cref="T:System.Collections.Immutable.ImmutableInterlocked"/> has no
/// member for a sorted dictionary, but its
/// <see cref="M:System.Collections.Immutable.ImmutableInterlocked.Update``1(``0@,System.Func{``0,``0})"/> replaces a field
/// atomically with the result of a function of the sorted map that it holds, such as a function of this module, and takes a
/// reference to the field.
/// </para>
/// <para>
/// A builder is the BCL builder: the module creates it (<c>builder</c>, <c>toBuilder</c>, <c>build</c> and <c>update</c>)
/// and turns it into a sorted map (<c>ofBuilder</c>), and the members of the builder, such as <c>Add</c>, <c>AddRange</c>,
/// <c>Remove</c>, <c>RemoveRange</c> and the indexer, are called on it. There is no builder module for maps (ADR 0001, D2).
/// </para>
/// <para>
/// A binding comes as a key/value pair, a reference tuple or a struct tuple (ADR 0001, D13). <c>SortedMap.ofArray</c>,
/// <c>SortedMap.ofList</c> and <c>SortedMap.ofSeq</c> take every shape: they are the static members of
/// <see cref="T:FSharp.Collections.Immutable.SortedMapExtensions"/>, which the module leaves visible by having no functions
/// with their names. <see cref="M:FSharp.Collections.Immutable.SortedMap.AddRange``2(System.Collections.Generic.IEnumerable{System.Collections.Generic.KeyValuePair{``0,``1}},System.Collections.Immutable.ImmutableSortedDictionary{``0,``1})"/>
/// and <see cref="M:FSharp.Collections.Immutable.SortedMap.SetItems``2(System.Collections.Generic.IEnumerable{System.Collections.Generic.KeyValuePair{``0,``1}},System.Collections.Immutable.ImmutableSortedDictionary{``0,``1})"/>
/// take key/value pairs, and the nested modules <see cref="T:FSharp.Collections.Immutable.SortedMap.Tuple"/> and
/// <see cref="T:FSharp.Collections.Immutable.SortedMap.ValueTuple"/> take reference and struct tuples.
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
module FSharp.Collections.Immutable.SortedMap

open System.Collections.Generic
open System.Collections.Immutable

// ---- Creation ----

/// <summary>
/// The empty sorted map, which orders its keys with F# generic comparison.
/// </summary>
/// <remarks>
/// O(1). The value is <see cref="F:System.Collections.Immutable.ImmutableSortedDictionary`2.Empty"/> with the key comparer
/// <see cref="M:Microsoft.FSharp.Core.LanguagePrimitives.FastGenericComparer``1"/>, created once per type of the keys and
/// the values, as the empty <see cref="T:Microsoft.FSharp.Collections.FSharpMap`2"/> is.
/// </remarks>
[<GeneralizableValue>]
[<CompiledName "Empty">]
val empty<'Key, 'Value when 'Key : not null and 'Key : comparison> : SortedMap<'Key, 'Value>

/// <summary>
/// Holds a comparer that orders the keys and an equality comparer of the values, to create sorted maps with them.
/// </summary>
/// <param name="keyComparer">The comparer that orders the keys.</param>
/// <param name="valueComparer">The equality comparer of the values.</param>
/// <returns>The struct whose members create sorted maps with the comparers.</returns>
/// <exception cref="T:System.ArgumentNullException">
/// Thrown when <paramref name="keyComparer"/> or <paramref name="valueComparer"/> is <see langword="null"/>.
/// </exception>
/// <remarks>
/// <para>
/// O(1); the struct allocates nothing. Its members have the names of the creation functions of the module (ADR 0001, D12).
/// </para>
/// <para>
/// The <c>ofSeq</c> member of the struct also gives an existing sorted map these comparers, which is how to change the
/// comparers of a map. Unlike <see cref="M:System.Collections.Immutable.ImmutableSortedDictionary`2.WithComparers(System.Collections.Generic.IComparer{`0},System.Collections.Generic.IEqualityComparer{`1})"/>
/// it rebuilds the map unless the key comparer is the same instance, and it merges keys that become equal instead of
/// throwing; the remarks of <see cref="T:FSharp.Collections.Immutable.CustomComparersSortedMap`2"/> have the details.
/// </para>
/// </remarks>
/// <example id="sortedMap-forComparers-1">
/// <code lang="fsharp">
/// let comparers = SortedMap.forComparers StringComparer.OrdinalIgnoreCase StringComparer.Ordinal
/// let table = comparers.ofSeq [ "two", "dos"; "One", "uno" ]
/// </code>
/// Evaluates to a sorted map of "One" and "two", in this order, whose keys ignore the case and whose values compare
/// ordinally.
/// </example>
/// <example id="sortedMap-forComparers-2">
/// <code lang="fsharp">
/// let ignoreCase = (SortedMap.forComparers StringComparer.OrdinalIgnoreCase (SortedMap.valueComparer table)).ofSeq table
/// </code>
/// Evaluates to a sorted map with the bindings of <c>table</c> whose keys ignore the case, in the order of that comparer,
/// and whose values keep the value comparer of <c>table</c>.
/// </example>
[<CompiledName "ForComparers">]
val forComparers :
    keyComparer : IComparer<'Key> -> valueComparer : IEqualityComparer<'Value> -> CustomComparersSortedMap<'Key, 'Value>

/// <summary>
/// Holds a comparer that orders the keys, to create sorted maps with it.
/// </summary>
/// <param name="keyComparer">The comparer that orders the keys.</param>
/// <returns>
/// The struct whose members create sorted maps with the key comparer, and with the default comparer of the values.
/// </returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="keyComparer"/> is <see langword="null"/>.</exception>
/// <remarks>
/// <para>
/// O(1); the struct allocates nothing. Its members have the names of the creation functions of the module (ADR 0001, D12).
/// </para>
/// <para>
/// The <c>ofSeq</c> member of the struct also gives an existing sorted map this key comparer, with the default comparer of
/// the values: the value comparer of the map is replaced, where
/// <see cref="M:System.Collections.Immutable.ImmutableSortedDictionary`2.WithComparers(System.Collections.Generic.IComparer{`0})"/>
/// keeps it. Use
/// <see cref="M:FSharp.Collections.Immutable.SortedMap.ForComparers``2(System.Collections.Generic.IComparer{``0},System.Collections.Generic.IEqualityComparer{``1})"/>
/// with the value comparer of the map to keep it.
/// </para>
/// </remarks>
/// <example id="sortedMap-forKeyComparer-1">
/// <code lang="fsharp">
/// let table = (SortedMap.forKeyComparer StringComparer.OrdinalIgnoreCase).ofSeq [ "two", 2; "one", 1; "ONE", 3 ]
/// </code>
/// Evaluates to a sorted map whose keys ignore the case, with the bindings of "ONE" to 3 and "two" to 2, in this order.
/// </example>
[<CompiledName "ForKeyComparer">]
val forKeyComparer : keyComparer : IComparer<'Key> -> CustomComparersSortedMap<'Key, 'Value>

/// <summary>
/// Creates a sorted map of the elements of a sequence under the keys that a projection returns for them.
/// </summary>
/// <param name="projection">The function that returns the key of an element.</param>
/// <param name="source">The input sequence, whose elements become the values.</param>
/// <returns>The sorted map of the elements under their keys, which orders the keys with F# generic comparison.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="source"/> is <see langword="null"/> or a default flat list.</exception>
/// <exception cref="T:System.ArgumentException">
/// Thrown when two elements have the same key and the default comparer of the values finds them different.
/// </exception>
/// <remarks>
/// O(n log n). The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableSortedDictionary.ToImmutableSortedDictionary``3(System.Collections.Generic.IEnumerable{``0},System.Func{``0,``1},System.Func{``0,``2},System.Collections.Generic.IComparer{``1},System.Collections.Generic.IEqualityComparer{``2})"/>
/// with the element itself as the value and <see cref="M:Microsoft.FSharp.Core.LanguagePrimitives.FastGenericComparer``1"/>,
/// which rejects a repeated key with another element and accepts one with an element that the default comparer of the
/// values finds equal: the first element stays, its key included (ADR 0001, D14). The value of a binding is the element
/// itself; with an element selector, as <c>ToImmutableSortedDictionary</c> takes one,
/// <c>SortedMap.empty |> SortedMap.addRange (source |> Seq.map (fun x -> KeyValuePair (key x, element x)))</c> rejects a
/// repeated key like the BCL does, and <c>SortedMap.ofSeq</c> of the mapped pairs lets the last binding win.
/// </remarks>
[<CompiledName "OfSeqBy">]
val ofSeqBy :
    projection : ('Value -> 'Key) -> source : seq<'Value> -> SortedMap<'Key, 'Value> when 'Key : not null and 'Key : comparison

/// <summary>Creates a sorted map of one binding.</summary>
/// <param name="key">The key.</param>
/// <param name="value">The value.</param>
/// <returns>
/// The sorted map of the binding, which orders its keys with F# generic comparison and compares its values with the
/// default comparer.
/// </returns>
/// <remarks>O(1).</remarks>
[<CompiledName "Singleton">]
val singleton : key : 'Key -> value : 'Value -> SortedMap<'Key, 'Value> when 'Key : not null and 'Key : comparison

// ---- Builder ----

/// <summary>
/// Creates a sorted map with a builder that an action fills.
/// </summary>
/// <param name="action">
/// The action that sets the bindings in the builder, which orders the keys with F# generic comparison and compares the
/// values with the default comparer.
/// </param>
/// <returns>
/// The sorted map of the bindings that the action set, with F# generic comparison of the keys and the default comparer of
/// the values.
/// </returns>
/// <remarks>
/// <para>
/// O(1) apart from the cost of the action, which sets each binding of the builder in O(log n), and from freezing the nodes
/// that the action changed: the sorted map shares the nodes of the builder instead of copying the entries. The function
/// creates the builder with
/// <see cref="M:System.Collections.Immutable.ImmutableSortedDictionary.CreateBuilder``2(System.Collections.Generic.IComparer{``0},System.Collections.Generic.IEqualityComparer{``1})"/>
/// and returns it with <see cref="M:System.Collections.Immutable.ImmutableSortedDictionary`2.Builder.ToImmutable"/>, as
/// <see cref="M:FSharp.Collections.Immutable.CustomComparersSortedMap`2.Build(Microsoft.FSharp.Core.FSharpFunc{System.Collections.Immutable.ImmutableSortedDictionary.Builder{`0,`1},Microsoft.FSharp.Core.Unit})"/>
/// does for other comparers.
/// </para>
/// <para>
/// The indexer of the builder keeps the earlier binding of a key for a value that the value comparer finds equal, its key
/// included (ADR 0001, D14).
/// </para>
/// </remarks>
/// <example id="sortedMap-build-1">
/// <code lang="fsharp">
/// let table = SortedMap.build (fun builder -> for i in 3 .. -1 .. 1 do builder[i] &lt;- i * i)
/// </code>
/// Evaluates to a sorted map of 1 to 1, 2 to 4 and 3 to 9, in this order.
/// </example>
[<CompiledName "Build">]
val build :
    action : (SortedMap<'Key, 'Value>.Builder -> unit) -> SortedMap<'Key, 'Value> when 'Key : not null and 'Key : comparison

/// <summary>
/// Creates an empty builder that orders the keys with F# generic comparison.
/// </summary>
/// <returns>The builder, whose values compare with the default comparer.</returns>
/// <remarks>
/// O(1). The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableSortedDictionary.CreateBuilder``2(System.Collections.Generic.IComparer{``0},System.Collections.Generic.IEqualityComparer{``1})"/>
/// with <see cref="M:Microsoft.FSharp.Core.LanguagePrimitives.FastGenericComparer``1"/>.
/// </remarks>
[<CompiledName "CreateBuilder">]
val builder : unit -> SortedMap<'Key, 'Value>.Builder when 'Key : not null and 'Key : comparison

/// <summary>
/// Returns the contents of a builder as a sorted map.
/// </summary>
/// <param name="builder">The builder, which stays usable.</param>
/// <returns>The sorted map of the bindings of the builder, with its comparers.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="builder"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(1) apart from freezing the nodes that changed since the builder last returned a sorted map: the sorted map shares the
/// nodes of the builder instead of copying the entries. A builder that has not changed since returns the same sorted map
/// again, and a builder that was just filled freezes all its nodes, which takes O(n). The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableSortedDictionary`2.Builder.ToImmutable"/>.
/// </remarks>
[<CompiledName "OfBuilder">]
val ofBuilder : builder : SortedMap<'Key, 'Value>.Builder -> SortedMap<'Key, 'Value>

/// <summary>
/// Creates a builder that contains the bindings of a sorted map.
/// </summary>
/// <param name="table">The input sorted map, which stays unchanged.</param>
/// <returns>The builder, with the comparers of the sorted map.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(1). The function wraps <see cref="M:System.Collections.Immutable.ImmutableSortedDictionary`2.ToBuilder"/>.
/// </remarks>
[<CompiledName "ToBuilder">]
val toBuilder : table : SortedMap<'Key, 'Value> -> SortedMap<'Key, 'Value>.Builder

/// <summary>
/// Creates a sorted map from a builder over the bindings of a sorted map, after an action changes the builder.
/// </summary>
/// <param name="action">The action that changes the builder, which has the comparers of the sorted map.</param>
/// <param name="table">The input sorted map, which stays unchanged.</param>
/// <returns>
/// The sorted map of the changed builder, with the comparers of the input; the input itself when the action changed
/// nothing.
/// </returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(1) apart from the cost of the action, which changes each binding of the builder in O(log n), and from freezing the
/// nodes that the action changed: the sorted map shares the nodes of the builder, and so the nodes of the input, instead of
/// copying the entries. The function creates the builder with
/// <see cref="M:System.Collections.Immutable.ImmutableSortedDictionary`2.ToBuilder"/> and returns it with
/// <see cref="M:System.Collections.Immutable.ImmutableSortedDictionary`2.Builder.ToImmutable"/>.
/// </remarks>
/// <example id="sortedMap-update-1">
/// <code lang="fsharp">
/// let squares = SortedMap.build (fun builder -> for i in 1..3 do builder[i] &lt;- i * i)
/// let table = squares |> SortedMap.update (fun builder -> builder.Remove 1 |> ignore; builder[4] &lt;- 16)
/// </code>
/// Evaluates to a sorted map of 2 to 4, 3 to 9 and 4 to 16, in this order, and <c>squares</c> stays unchanged.
/// </example>
[<CompiledName "Update">]
val update : action : (SortedMap<'Key, 'Value>.Builder -> unit) -> table : SortedMap<'Key, 'Value> -> SortedMap<'Key, 'Value>

// ---- Properties ----

/// <summary>Returns the number of bindings of the sorted map.</summary>
/// <param name="table">The input sorted map.</param>
/// <returns>The number of bindings.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>O(1).</remarks>
[<CompiledName "Count">]
val count : table : SortedMap<'Key, 'Value> -> int

/// <summary>Returns whether the sorted map has no bindings.</summary>
/// <param name="table">The input sorted map.</param>
/// <returns><see langword="true"/> when the sorted map is empty.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>O(1).</remarks>
[<CompiledName "IsEmpty">]
val isEmpty : table : SortedMap<'Key, 'Value> -> bool

/// <summary>
/// Returns the comparer that orders the keys.
/// </summary>
/// <param name="table">The input sorted map.</param>
/// <returns>The key comparer.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(1). The function wraps <see cref="P:System.Collections.Immutable.ImmutableSortedDictionary`2.KeyComparer"/>.
/// </remarks>
[<CompiledName "KeyComparer">]
val keyComparer : table : SortedMap<'Key, 'Value> -> IComparer<'Key>

/// <summary>Returns the keys of the sorted map.</summary>
/// <param name="table">The input sorted map.</param>
/// <returns>
/// A read-only view of the keys in ascending order, whose count takes O(1) and whose lookup takes O(log n) in the key
/// comparer.
/// </returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>O(1).</remarks>
[<CompiledName "Keys">]
val keys : table : SortedMap<'Key, 'Value> -> ICollection<'Key>

/// <summary>
/// Returns the equality comparer of the values.
/// </summary>
/// <param name="table">The input sorted map.</param>
/// <returns>The value comparer.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(1). The function wraps <see cref="P:System.Collections.Immutable.ImmutableSortedDictionary`2.ValueComparer"/>.
/// </remarks>
[<CompiledName "ValueComparer">]
val valueComparer : table : SortedMap<'Key, 'Value> -> IEqualityComparer<'Value>

/// <summary>
/// Returns the values of the sorted map, including the duplicates.
/// </summary>
/// <param name="table">The input sorted map.</param>
/// <returns>
/// A read-only view of the values in the ascending order of their keys, whose count takes O(1) and whose lookup takes
/// O(n) in the value comparer.
/// </returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(1).
/// </remarks>
[<CompiledName "Values">]
val values : table : SortedMap<'Key, 'Value> -> ICollection<'Value>

// ---- Access ----

/// <summary>Returns the value of a key.</summary>
/// <param name="key">The key to find.</param>
/// <param name="table">The input sorted map.</param>
/// <returns>The value of the key.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <exception cref="T:System.Collections.Generic.KeyNotFoundException">Thrown when the sorted map has no binding of the key.</exception>
/// <remarks>O(log n).</remarks>
[<CompiledName "Find">]
val find : key : 'Key -> table : SortedMap<'Key, 'Value> -> 'Value

/// <summary>
/// Returns the binding of the largest key of the sorted map.
/// </summary>
/// <param name="table">The input sorted map.</param>
/// <returns>The key and the value of the last binding in the order of the keys.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <exception cref="T:System.Collections.Generic.KeyNotFoundException">Thrown when the sorted map is empty.</exception>
/// <remarks>
/// O(n), where
/// <see cref="M:Microsoft.FSharp.Collections.MapModule.MaxKeyValue``2(Microsoft.FSharp.Collections.FSharpMap{``0,``1})"/>
/// takes O(log n): <see cref="T:System.Collections.Immutable.ImmutableSortedDictionary`2"/> enumerates its bindings only
/// in ascending order and gives no other access to its largest key, so the function visits every binding.
/// </remarks>
[<CompiledName "MaxKeyValue">]
val maxKeyValue : table : SortedMap<'Key, 'Value> -> struct ('Key * 'Value)

/// <summary>Returns the binding of the smallest key of the sorted map.</summary>
/// <param name="table">The input sorted map.</param>
/// <returns>The key and the value of the first binding in the order of the keys.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <exception cref="T:System.Collections.Generic.KeyNotFoundException">Thrown when the sorted map is empty.</exception>
/// <remarks>O(log n).</remarks>
[<CompiledName "MinKeyValue">]
val minKeyValue : table : SortedMap<'Key, 'Value> -> struct ('Key * 'Value)

/// <summary>
/// Returns the value of a key, or <c>ValueNone</c> when the sorted map has no binding of the key.
/// </summary>
/// <param name="key">The key to find.</param>
/// <param name="table">The input sorted map.</param>
/// <returns>The value of the key, or <c>ValueNone</c>.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(log n). The function is the <c>TryGetValue</c> of the BCL with a <c>voption</c> instead of the <c>out</c> parameter.
/// <c>GetValueOrDefault</c> is <c>SortedMap.tryFind key table |> ValueOption.defaultValue defaultValue</c>: one lookup,
/// and a <c>voption</c> is a struct.
/// </remarks>
[<CompiledName "TryFind">]
val tryFind : key : 'Key -> table : SortedMap<'Key, 'Value> -> 'Value voption

/// <summary>
/// Returns the key of the sorted map that equals a key in the key comparer, or <c>ValueNone</c>.
/// </summary>
/// <param name="key">The key to find.</param>
/// <param name="table">The input sorted map.</param>
/// <returns>The key that the sorted map holds, which can differ from the given one, or <c>ValueNone</c>.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(log n). The function wraps <see cref="M:System.Collections.Immutable.ImmutableSortedDictionary`2.TryGetKey(`0,`0@)"/>.
/// </remarks>
[<CompiledName "TryGetKey">]
val tryGetKey : key : 'Key -> table : SortedMap<'Key, 'Value> -> 'Key voption

// ---- Search ----

/// <summary>
/// Returns whether the sorted map binds a key to a value, comparing the values with the value comparer.
/// </summary>
/// <param name="key">The key to find.</param>
/// <param name="value">The value that the key must be bound to.</param>
/// <param name="table">The input sorted map.</param>
/// <returns>
/// <see langword="true"/> when the sorted map has a binding of the key whose value the value comparer finds equal to
/// <paramref name="value"/>.
/// </returns>
/// <exception cref="T:System.ArgumentNullException">
/// Thrown when <paramref name="table"/> or <paramref name="key"/> is <see langword="null"/>.
/// </exception>
/// <remarks>
/// <para>
/// O(log n). The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableSortedDictionary`2.Contains(System.Collections.Generic.KeyValuePair{`0,`1})"/>.
/// The key comparer of the sorted map finds the key, and a key without a binding gives <see langword="false"/>. A
/// <see langword="null"/> key throws, as it does in
/// <see cref="M:FSharp.Collections.Immutable.SortedMap.ContainsKey``2(``0,System.Collections.Immutable.ImmutableSortedDictionary{``0,``1})"/>;
/// the parameter of the exception is the one that the member names, <c>Key</c>.
/// </para>
/// <para>
/// The value comparer of the sorted map decides whether the values are equal. No composition of the other functions asks
/// it: <see cref="M:FSharp.Collections.Immutable.SortedMap.TryFind``2(``0,System.Collections.Immutable.ImmutableSortedDictionary{``0,``1})"/>
/// followed by <c>=</c> compares with F# equality, which for a comparer that ignores the case finds <c>"Hello"</c> and
/// <c>"HELLO"</c> different.
/// </para>
/// </remarks>
/// <example id="sortedMap-containsBinding-1">
/// <code lang="fsharp">
/// let table = (SortedMap.forComparers StringComparer.Ordinal StringComparer.OrdinalIgnoreCase).ofSeq [ "one", "Uno"; "two", "Dos" ]
/// let found = table |> SortedMap.containsBinding "one" "UNO"
/// </code>
/// Evaluates to <c>true</c>, where <c>SortedMap.tryFind "one" table</c> is <c>ValueSome "Uno"</c>.
/// </example>
[<CompiledName "ContainsBinding">]
val containsBinding : key : 'Key -> value : 'Value -> table : SortedMap<'Key, 'Value> -> bool

/// <summary>Returns whether the sorted map has a binding of a key.</summary>
/// <param name="key">The key to find.</param>
/// <param name="table">The input sorted map.</param>
/// <returns><see langword="true"/> when the sorted map has a binding of the key.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>O(log n).</remarks>
[<CompiledName "ContainsKey">]
val containsKey : key : 'Key -> table : SortedMap<'Key, 'Value> -> bool

/// <summary>
/// Returns whether the sorted map has a binding of a value, in the value comparer.
/// </summary>
/// <param name="value">The value to find.</param>
/// <param name="table">The input sorted map.</param>
/// <returns><see langword="true"/> when a value of the sorted map equals the given one.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n). The function wraps <see cref="M:System.Collections.Immutable.ImmutableSortedDictionary`2.ContainsValue(`1)"/>.
/// </remarks>
[<CompiledName "ContainsValue">]
val containsValue : value : 'Value -> table : SortedMap<'Key, 'Value> -> bool

/// <summary>Returns whether any binding satisfies the predicate.</summary>
/// <param name="predicate">The function to test the bindings.</param>
/// <param name="table">The input sorted map.</param>
/// <returns><see langword="true"/> when a binding satisfies the predicate.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>O(n).</remarks>
[<CompiledName "Exists">]
val exists : predicate : ('Key -> 'Value -> bool) -> table : SortedMap<'Key, 'Value> -> bool

/// <summary>
/// Returns the key of the first binding, in the order of the keys, that satisfies the predicate.
/// </summary>
/// <param name="predicate">The function to test the bindings.</param>
/// <param name="table">The input sorted map.</param>
/// <returns>The key of the first binding that satisfies the predicate.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <exception cref="T:System.Collections.Generic.KeyNotFoundException">Thrown when no binding satisfies the predicate.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "FindKey">]
val findKey : predicate : ('Key -> 'Value -> bool) -> table : SortedMap<'Key, 'Value> -> 'Key

/// <summary>Returns whether all bindings satisfy the predicate.</summary>
/// <param name="predicate">The function to test the bindings.</param>
/// <param name="table">The input sorted map.</param>
/// <returns><see langword="true"/> when every binding satisfies the predicate.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>O(n).</remarks>
[<CompiledName "ForAll">]
val forall : predicate : ('Key -> 'Value -> bool) -> table : SortedMap<'Key, 'Value> -> bool

/// <summary>
/// Returns the first value, in the order of the keys, that the chooser returns for the bindings.
/// </summary>
/// <param name="chooser">The function that returns a value, or <c>ValueNone</c> to go on.</param>
/// <param name="table">The input sorted map.</param>
/// <returns>The first chosen value.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <exception cref="T:System.Collections.Generic.KeyNotFoundException">Thrown when the chooser returns <c>ValueNone</c> for every binding.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Pick">]
val pick : chooser : ('Key -> 'Value -> 'U voption) -> table : SortedMap<'Key, 'Value> -> 'U

/// <summary>
/// Returns the key of the first binding, in the order of the keys, that satisfies the predicate, or <c>ValueNone</c>.
/// </summary>
/// <param name="predicate">The function to test the bindings.</param>
/// <param name="table">The input sorted map.</param>
/// <returns>The key of the first binding that satisfies the predicate, or <c>ValueNone</c>.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "TryFindKey">]
val tryFindKey : predicate : ('Key -> 'Value -> bool) -> table : SortedMap<'Key, 'Value> -> 'Key voption

/// <summary>
/// Returns the first value, in the order of the keys, that the chooser returns for the bindings, or <c>ValueNone</c>.
/// </summary>
/// <param name="chooser">The function that returns a value, or <c>ValueNone</c> to go on.</param>
/// <param name="table">The input sorted map.</param>
/// <returns>The first chosen value, or <c>ValueNone</c>.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "TryPick">]
val tryPick : chooser : ('Key -> 'Value -> 'U voption) -> table : SortedMap<'Key, 'Value> -> 'U voption

// ---- Transformation ----

/// <summary>
/// Returns a sorted map with a binding of a key to a value, for a key that is not bound to another value.
/// </summary>
/// <param name="key">The key.</param>
/// <param name="value">The value.</param>
/// <param name="table">The input sorted map.</param>
/// <returns>The sorted map with the binding; the input itself when it already has the binding.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <exception cref="T:System.ArgumentException">
/// Thrown when the sorted map binds the key to a value that the value comparer finds different.
/// </exception>
/// <remarks>
/// <para>
/// O(log n). The function wraps <see cref="M:System.Collections.Immutable.ImmutableSortedDictionary`2.Add(`0,`1)"/> and
/// behaves as it does (ADR 0001, D14): it does not replace the value of a key, where
/// <see cref="M:Microsoft.FSharp.Collections.MapModule.Add``2(``0,``1,Microsoft.FSharp.Collections.FSharpMap{``0,``1})"/>
/// does. When the value comparer of the sorted map finds the value equal to the current value of the key, the binding stays
/// as it is, its key included, and the input itself comes back.
/// </para>
/// <para>
/// Use <see cref="M:FSharp.Collections.Immutable.SortedMap.SetItem``2(``0,``1,System.Collections.Immutable.ImmutableSortedDictionary{``0,``1})"/>
/// to replace the value of a key, and
/// <see cref="M:FSharp.Collections.Immutable.SortedMap.Change``2(``0,Microsoft.FSharp.Core.FSharpFunc{Microsoft.FSharp.Core.FSharpValueOption{``1},Microsoft.FSharp.Core.FSharpValueOption{``1}},System.Collections.Immutable.ImmutableSortedDictionary{``0,``1})"/>
/// to add a binding only when the key has none, or to compute the new value from the current one.
/// </para>
/// </remarks>
[<CompiledName "Add">]
val add : key : 'Key -> value : 'Value -> table : SortedMap<'Key, 'Value> -> SortedMap<'Key, 'Value>

/// <summary>
/// Returns a sorted map with the bindings of key/value pairs added.
/// </summary>
/// <param name="pairs">The key/value pairs to add.</param>
/// <param name="table">The input sorted map.</param>
/// <returns>The sorted map with the bindings.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="pairs"/> is <see langword="null"/> or a default flat list, or <paramref name="table"/> is <see langword="null"/>.</exception>
/// <exception cref="T:System.ArgumentException">
/// Thrown when a pair has the key of a binding of the sorted map or of another pair, and a value that the value comparer
/// finds different.
/// </exception>
/// <remarks>
/// O(m log(n + m)). The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableSortedDictionary`2.AddRange(System.Collections.Generic.IEnumerable{System.Collections.Generic.KeyValuePair{`0,`1}})"/>,
/// which rejects another value of a key and accepts a pair whose value the value comparer finds equal to the value of the
/// key, leaving that binding as it is, its key included (ADR 0001, D14). Use
/// <see cref="M:FSharp.Collections.Immutable.SortedMap.SetItems``2(System.Collections.Generic.IEnumerable{System.Collections.Generic.KeyValuePair{``0,``1}},System.Collections.Immutable.ImmutableSortedDictionary{``0,``1})"/>
/// to replace the values, and <see cref="T:FSharp.Collections.Immutable.SortedMap.Tuple"/> and
/// <see cref="T:FSharp.Collections.Immutable.SortedMap.ValueTuple"/> to add tuples.
/// </remarks>
[<CompiledName "AddRange">]
val addRange : pairs : seq<KeyValuePair<'Key, 'Value>> -> table : SortedMap<'Key, 'Value> -> SortedMap<'Key, 'Value>

/// <summary>
/// Returns a sorted map with the binding of a key changed by a function of its current value.
/// </summary>
/// <param name="key">The key.</param>
/// <param name="f">
/// The function that takes the current value or <c>ValueNone</c>, and returns the new value or <c>ValueNone</c> to remove
/// the binding.
/// </param>
/// <param name="table">The input sorted map.</param>
/// <returns>The sorted map with the changed binding; the input itself when nothing changes.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// <para>
/// O(log n). When <paramref name="f"/> returns a value that the value comparer of the sorted map finds equal to the
/// current value, the binding stays as it is, its key included, as in
/// <see cref="M:System.Collections.Immutable.ImmutableSortedDictionary`2.SetItem(`0,`1)"/> (ADR 0001, D14).
/// </para>
/// <para>
/// Adding a binding only when the key has none is
/// <c>SortedMap.change key (function ValueNone -> ValueSome value | existing -> existing)</c>.
/// </para>
/// </remarks>
[<CompiledName "Change">]
val change : key : 'Key -> f : ('Value voption -> 'Value voption) -> table : SortedMap<'Key, 'Value> -> SortedMap<'Key, 'Value>

/// <summary>
/// Returns a sorted map of the values that the chooser returns for the bindings, under their keys.
/// </summary>
/// <param name="chooser">The function that returns the new value, or <c>ValueNone</c> to drop the binding.</param>
/// <param name="table">The input sorted map.</param>
/// <returns>
/// The sorted map of the chosen values, with the key comparer of the input and the default comparer of the new values.
/// </returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n log n).
/// </remarks>
[<CompiledName "Choose">]
val choose : chooser : ('Key -> 'Value -> 'U voption) -> table : SortedMap<'Key, 'Value> -> SortedMap<'Key, 'U>

/// <summary>
/// Returns an empty sorted map with the comparers of a sorted map.
/// </summary>
/// <param name="table">The input sorted map.</param>
/// <returns>The empty sorted map; the input itself when it is empty.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(1). The function wraps <see cref="M:System.Collections.Immutable.ImmutableSortedDictionary`2.Clear"/>.
/// </remarks>
[<CompiledName "Clear">]
val clear : table : SortedMap<'Key, 'Value> -> SortedMap<'Key, 'Value>

/// <summary>Returns the bindings that satisfy the predicate.</summary>
/// <param name="predicate">The function to test the bindings.</param>
/// <param name="table">The input sorted map.</param>
/// <returns>
/// The sorted map of the bindings that satisfy the predicate, with the comparers of the input; the input itself when
/// every binding does.
/// </returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>O(n log n).</remarks>
[<CompiledName "Filter">]
val filter : predicate : ('Key -> 'Value -> bool) -> table : SortedMap<'Key, 'Value> -> SortedMap<'Key, 'Value>

/// <summary>
/// Returns the results of a function applied to each binding, under the keys of the bindings.
/// </summary>
/// <param name="mapping">The function to transform the bindings.</param>
/// <param name="table">The input sorted map.</param>
/// <returns>
/// The sorted map of the results, with the key comparer of the input and the default comparer of the new values.
/// </returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n log n).
/// </remarks>
[<CompiledName "Map">]
val map : mapping : ('Key -> 'Value -> 'U) -> table : SortedMap<'Key, 'Value> -> SortedMap<'Key, 'U>

/// <summary>Returns a sorted map without the binding of a key.</summary>
/// <param name="key">The key to remove.</param>
/// <param name="table">The input sorted map.</param>
/// <returns>The sorted map without the binding; the input itself when it has no binding of the key.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>O(log n).</remarks>
[<CompiledName "Remove">]
val remove : key : 'Key -> table : SortedMap<'Key, 'Value> -> SortedMap<'Key, 'Value>

/// <summary>
/// Returns a sorted map without the bindings of keys.
/// </summary>
/// <param name="keys">The keys to remove; a key without a binding is skipped.</param>
/// <param name="table">The input sorted map.</param>
/// <returns>The sorted map without the bindings.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="keys"/> is <see langword="null"/> or a default flat list, or <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(m log n). The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableSortedDictionary`2.RemoveRange(System.Collections.Generic.IEnumerable{`0})"/>.
/// </remarks>
[<CompiledName "RemoveRange">]
val removeRange : keys : seq<'Key> -> table : SortedMap<'Key, 'Value> -> SortedMap<'Key, 'Value>

/// <summary>
/// Returns a sorted map with a binding of a key to a value, which replaces an existing binding of the key.
/// </summary>
/// <param name="key">The key.</param>
/// <param name="value">The value.</param>
/// <param name="table">The input sorted map.</param>
/// <returns>The sorted map with the binding; the input itself when the binding stays as it is.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(log n). The function wraps <see cref="M:System.Collections.Immutable.ImmutableSortedDictionary`2.SetItem(`0,`1)"/> and
/// replaces the value of a key, as
/// <see cref="M:Microsoft.FSharp.Collections.MapModule.Add``2(``0,``1,Microsoft.FSharp.Collections.FSharpMap{``0,``1})"/>
/// does, where
/// <see cref="M:FSharp.Collections.Immutable.SortedMap.Add``2(``0,``1,System.Collections.Immutable.ImmutableSortedDictionary{``0,``1})"/>
/// throws for a key that is bound to another value. When the value comparer of the sorted map finds the value equal to the
/// current value of the key, the binding stays as it is, its key included (ADR 0001, D14).
/// </remarks>
[<CompiledName "SetItem">]
val setItem : key : 'Key -> value : 'Value -> table : SortedMap<'Key, 'Value> -> SortedMap<'Key, 'Value>

/// <summary>
/// Returns a sorted map with the bindings of key/value pairs, which replace the existing bindings of their keys.
/// </summary>
/// <param name="pairs">The key/value pairs to set; a later pair of a key replaces an earlier one.</param>
/// <param name="table">The input sorted map.</param>
/// <returns>The sorted map with the bindings.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="pairs"/> is <see langword="null"/> or a default flat list, or <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(m log(n + m)). The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableSortedDictionary`2.SetItems(System.Collections.Generic.IEnumerable{System.Collections.Generic.KeyValuePair{`0,`1}})"/>.
/// A pair whose value the value comparer finds equal to the current value of its key, or to the value of an earlier pair
/// of the key, leaves that binding as it is, its key included (ADR 0001, D14).
/// <see cref="T:FSharp.Collections.Immutable.SortedMap.Tuple"/> and
/// <see cref="T:FSharp.Collections.Immutable.SortedMap.ValueTuple"/> set tuples.
/// </remarks>
[<CompiledName "SetItems">]
val setItems : pairs : seq<KeyValuePair<'Key, 'Value>> -> table : SortedMap<'Key, 'Value> -> SortedMap<'Key, 'Value>

// ---- Folding ----

/// <summary>
/// Applies a function to a state and each binding, in the ascending order of the keys.
/// </summary>
/// <param name="folder">The function that computes the next state.</param>
/// <param name="state">The initial state.</param>
/// <param name="table">The input sorted map.</param>
/// <returns>The final state.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Fold">]
val fold : folder : ('State -> 'Key -> 'Value -> 'State) -> state : 'State -> table : SortedMap<'Key, 'Value> -> 'State

/// <summary>
/// Applies a function to each binding and a state, in the descending order of the keys.
/// </summary>
/// <param name="folder">The function that computes the next state.</param>
/// <param name="table">The input sorted map.</param>
/// <param name="state">The initial state.</param>
/// <returns>The final state.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n). The function visits the bindings from the largest key to the smallest one, as
/// <see cref="M:Microsoft.FSharp.Collections.MapModule.FoldBack``3(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpFunc{``1,Microsoft.FSharp.Core.FSharpFunc{``2,``2}}},Microsoft.FSharp.Collections.FSharpMap{``0,``1},``2)"/>
/// does. It copies the bindings first, because the sorted map enumerates them only in ascending order.
/// </remarks>
[<CompiledName "FoldBack">]
val foldBack : folder : ('Key -> 'Value -> 'State -> 'State) -> table : SortedMap<'Key, 'Value> -> state : 'State -> 'State

/// <summary>
/// Applies an action to each binding, in the ascending order of the keys.
/// </summary>
/// <param name="action">The action.</param>
/// <param name="table">The input sorted map.</param>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Iterate">]
val iter : action : ('Key -> 'Value -> unit) -> table : SortedMap<'Key, 'Value> -> unit

// ---- Partitioning ----

/// <summary>Splits the bindings by a predicate.</summary>
/// <param name="predicate">The function to test the bindings.</param>
/// <param name="table">The input sorted map.</param>
/// <returns>
/// The sorted map of the bindings that satisfy the predicate and the sorted map of the other ones, both with the
/// comparers of the input.
/// </returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>O(n log n).</remarks>
[<CompiledName "Partition">]
val partition :
    predicate : ('Key -> 'Value -> bool) ->
    table : SortedMap<'Key, 'Value> ->
        struct (SortedMap<'Key, 'Value> * SortedMap<'Key, 'Value>)

// ---- Conversion ----

/// <summary>
/// Returns a new array of the bindings, in the ascending order of the keys.
/// </summary>
/// <param name="table">The input sorted map.</param>
/// <returns>The array of key/value pairs.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "ToArray">]
val toArray : table : SortedMap<'Key, 'Value> -> struct ('Key * 'Value) array

/// <summary>
/// Returns a list of the bindings, in the ascending order of the keys.
/// </summary>
/// <param name="table">The input sorted map.</param>
/// <returns>The list of key/value pairs.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "ToList">]
val toList : table : SortedMap<'Key, 'Value> -> struct ('Key * 'Value) list

/// <summary>
/// Returns the bindings as a sequence, in the ascending order of the keys.
/// </summary>
/// <param name="table">The input sorted map.</param>
/// <returns>The sequence of key/value pairs, computed on demand.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="table"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(1); O(n) to enumerate.
/// </remarks>
[<CompiledName "ToSeq">]
val toSeq : table : SortedMap<'Key, 'Value> -> seq<struct ('Key * 'Value)>

/// <summary>
/// The functions of <see cref="T:FSharp.Collections.Immutable.SortedMap"/> that take bindings, with reference tuples
/// instead of key/value pairs.
/// </summary>
/// <remarks>
/// A function of the module takes one shape of the bindings, so the shape of the tuples of most F# code has a module of its
/// own (ADR 0001, D13): <c>SortedMap.Tuple.addRange pairs table</c>. The functions resolve after
/// <c>open FSharp.Collections.Immutable.Tuple</c> too, whose <c>SortedMap</c> module has no nested module of this name.
/// </remarks>
[<RequireQualifiedAccess>]
module Tuple =

    // ---- Transformation ----

    /// <summary>
    /// Returns a sorted map with the bindings of key/value tuples added.
    /// </summary>
    /// <param name="pairs">The key/value tuples to add.</param>
    /// <param name="table">The input sorted map.</param>
    /// <returns>The sorted map with the bindings.</returns>
    /// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="pairs"/> is <see langword="null"/> or a default flat list, or <paramref name="table"/> is <see langword="null"/>.</exception>
    /// <exception cref="T:System.ArgumentException">
    /// Thrown when a tuple has the key of a binding of the sorted map or of another tuple, and a value that the value
    /// comparer finds different.
    /// </exception>
    /// <remarks>
    /// O(m log(n + m)). The function wraps
    /// <see cref="M:System.Collections.Immutable.ImmutableSortedDictionary`2.AddRange(System.Collections.Generic.IEnumerable{System.Collections.Generic.KeyValuePair{`0,`1}})"/>,
    /// which rejects another value of a key and accepts a tuple whose value the value comparer finds equal to the value of
    /// the key, leaving that binding as it is, its key included (ADR 0001, D14).
    /// </remarks>
    [<CompiledName "AddRange">]
    val addRange : pairs : seq<'Key * 'Value> -> table : SortedMap<'Key, 'Value> -> SortedMap<'Key, 'Value>

    /// <summary>
    /// Returns a sorted map with the bindings of key/value tuples, which replace the existing bindings of their keys.
    /// </summary>
    /// <param name="pairs">The key/value tuples to set; a later tuple of a key replaces an earlier one.</param>
    /// <param name="table">The input sorted map.</param>
    /// <returns>The sorted map with the bindings.</returns>
    /// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="pairs"/> is <see langword="null"/> or a default flat list, or <paramref name="table"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// O(m log(n + m)). The function wraps
    /// <see cref="M:System.Collections.Immutable.ImmutableSortedDictionary`2.SetItems(System.Collections.Generic.IEnumerable{System.Collections.Generic.KeyValuePair{`0,`1}})"/>.
    /// A tuple whose value the value comparer finds equal to the current value of its key, or to the value of an earlier
    /// tuple of the key, leaves that binding as it is, its key included (ADR 0001, D14).
    /// </remarks>
    [<CompiledName "SetItems">]
    val setItems : pairs : seq<'Key * 'Value> -> table : SortedMap<'Key, 'Value> -> SortedMap<'Key, 'Value>

/// <summary>
/// The functions of <see cref="T:FSharp.Collections.Immutable.SortedMap"/> that take bindings, with struct tuples instead
/// of key/value pairs.
/// </summary>
/// <remarks>
/// A function of the module takes one shape of the bindings, so the struct tuples of ADR 0001, D3 have a module of their own
/// (D13): <c>SortedMap.ValueTuple.setItems pairs table</c>.
/// </remarks>
[<RequireQualifiedAccess>]
module ValueTuple =

    // ---- Transformation ----

    /// <summary>
    /// Returns a sorted map with the bindings of key/value tuples added.
    /// </summary>
    /// <param name="pairs">The key/value tuples to add.</param>
    /// <param name="table">The input sorted map.</param>
    /// <returns>The sorted map with the bindings.</returns>
    /// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="pairs"/> is <see langword="null"/> or a default flat list, or <paramref name="table"/> is <see langword="null"/>.</exception>
    /// <exception cref="T:System.ArgumentException">
    /// Thrown when a tuple has the key of a binding of the sorted map or of another tuple, and a value that the value
    /// comparer finds different.
    /// </exception>
    /// <remarks>
    /// O(m log(n + m)). The function wraps
    /// <see cref="M:System.Collections.Immutable.ImmutableSortedDictionary`2.AddRange(System.Collections.Generic.IEnumerable{System.Collections.Generic.KeyValuePair{`0,`1}})"/>,
    /// which rejects another value of a key and accepts a tuple whose value the value comparer finds equal to the value of
    /// the key, leaving that binding as it is, its key included (ADR 0001, D14).
    /// </remarks>
    [<CompiledName "AddRange">]
    val addRange : pairs : seq<struct ('Key * 'Value)> -> table : SortedMap<'Key, 'Value> -> SortedMap<'Key, 'Value>

    /// <summary>
    /// Returns a sorted map with the bindings of key/value tuples, which replace the existing bindings of their keys.
    /// </summary>
    /// <param name="pairs">The key/value tuples to set; a later tuple of a key replaces an earlier one.</param>
    /// <param name="table">The input sorted map.</param>
    /// <returns>The sorted map with the bindings.</returns>
    /// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="pairs"/> is <see langword="null"/> or a default flat list, or <paramref name="table"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// O(m log(n + m)). The function wraps
    /// <see cref="M:System.Collections.Immutable.ImmutableSortedDictionary`2.SetItems(System.Collections.Generic.IEnumerable{System.Collections.Generic.KeyValuePair{`0,`1}})"/>.
    /// A tuple whose value the value comparer finds equal to the current value of its key, or to the value of an earlier
    /// tuple of the key, leaves that binding as it is, its key included (ADR 0001, D14).
    /// </remarks>
    [<CompiledName "SetItems">]
    val setItems : pairs : seq<struct ('Key * 'Value)> -> table : SortedMap<'Key, 'Value> -> SortedMap<'Key, 'Value>
