namespace FSharp.Collections.Immutable

open System
open System.Collections.Generic

/// <summary>
/// The equality comparers of the keys and the values of hash maps, with the creation functions of
/// <see cref="T:FSharp.Collections.Immutable.HashMap"/> that create hash maps with them (ADR 0001, D12).
/// </summary>
/// <remarks>
/// <para>
/// <see cref="M:FSharp.Collections.Immutable.HashMap.ForComparers``2(System.Collections.Generic.IEqualityComparer{``0},System.Collections.Generic.IEqualityComparer{``1})"/>
/// and <see cref="M:FSharp.Collections.Immutable.HashMap.ForKeyComparer``2(System.Collections.Generic.IEqualityComparer{``0})"/>
/// create the struct. Its members have the names of the creation functions of the module and the comparers of the struct.
/// </para>
/// <para>
/// <c>ofArray</c>, <c>ofList</c> and <c>ofSeq</c> take the bindings as key/value pairs, reference tuples or struct
/// tuples (ADR 0001, D13). A later binding of a key replaces an earlier one, as in
/// <see cref="M:Microsoft.FSharp.Collections.MapModule.OfSeq``2(System.Collections.Generic.IEnumerable{System.Tuple{``0,``1}})"/>,
/// unless the value comparer finds the two values equal: then the earlier binding stays, its key included, as in the
/// builder of <see cref="T:System.Collections.Immutable.ImmutableDictionary`2"/> (ADR 0001, D14). A repeated key never
/// throws, where <see cref="M:System.Collections.Immutable.ImmutableDictionary.CreateRange``2(System.Collections.Generic.IEnumerable{System.Collections.Generic.KeyValuePair{``0,``1}})"/>
/// rejects another value of it: to reject it, add the bindings to an empty map,
/// <c>(HashMap.forComparers keyComparer valueComparer).empty |> HashMap.addRange pairs</c>.
/// </para>
/// <para>
/// <c>ofSeq</c> also takes an existing map, because a map is a sequence of key/value pairs, and gives its bindings the
/// comparers of the struct. This is how to change the comparers of a map, with three differences from
/// <see cref="M:System.Collections.Immutable.ImmutableDictionary`2.WithComparers(System.Collections.Generic.IEqualityComparer{`0},System.Collections.Generic.IEqualityComparer{`1})"/>:
/// the cost, the keys that become equal and the value comparer. The remarks of the <c>ofSeq</c> member describe them.
/// </para>
/// <para>
/// The struct only holds the comparers, so creating it allocates nothing. Its default value holds no comparers and stands
/// for the default comparers, <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>.
/// </para>
/// </remarks>
/// <example id="customComparersHashMap-1">
/// <code lang="fsharp">
/// let table = (HashMap.forKeyComparer StringComparer.OrdinalIgnoreCase).ofSeq [ "one", 1; "ONE", 2; "two", 3 ]
/// </code>
/// Evaluates to a hash map whose keys ignore the case, with the bindings of "ONE" to 2 and "two" to 3.
/// </example>
/// <example id="customComparersHashMap-2">
/// <code lang="fsharp">
/// let ignoreCase = (HashMap.forKeyComparer StringComparer.OrdinalIgnoreCase).ofSeq table
/// let keepValues = (HashMap.forComparers StringComparer.OrdinalIgnoreCase (HashMap.valueComparer table)).ofSeq table
/// </code>
/// Both evaluate to a hash map whose keys ignore the case: the first compares the values with the default comparer, the
/// second keeps the value comparer of <c>table</c>.
/// </example>
[<Struct; NoComparison; NoEquality>]
type CustomComparersHashMap<'Key, 'Value when 'Key : not null> =

    val private keyComparer : IEqualityComparer<'Key> | null
    val private valueComparer : IEqualityComparer<'Value> | null

    /// <summary>Holds the equality comparers of the keys and the values.</summary>
    /// <param name="keyComparer">The equality comparer of the keys.</param>
    /// <param name="valueComparer">The equality comparer of the values.</param>
    /// <exception cref="T:System.ArgumentNullException">
    /// Thrown when <paramref name="keyComparer"/> or <paramref name="valueComparer"/> is <see langword="null"/>.
    /// </exception>
    new :
        keyComparer : IEqualityComparer<'Key> * valueComparer : IEqualityComparer<'Value> -> CustomComparersHashMap<'Key, 'Value>

    /// <summary>
    /// Holds the equality comparer of the keys, and the default comparer of the values.
    /// </summary>
    /// <param name="keyComparer">The equality comparer of the keys.</param>
    /// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="keyComparer"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// The values compare with <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>.
    /// </remarks>
    new : keyComparer : IEqualityComparer<'Key> -> CustomComparersHashMap<'Key, 'Value>

    // ---- Creation ----

    /// <summary>
    /// The empty hash map with the comparers.
    /// </summary>
    /// <remarks>
    /// O(1). The property wraps
    /// <see cref="M:System.Collections.Immutable.ImmutableDictionary.Create``2(System.Collections.Generic.IEqualityComparer{``0},System.Collections.Generic.IEqualityComparer{``1})"/>.
    /// </remarks>
    [<CompiledName "Empty">]
    member empty : HashMap<'Key, 'Value>

    /// <summary>
    /// Creates a hash map with the comparers from an array of key/value pairs.
    /// </summary>
    /// <param name="elements">The input array of key/value pairs.</param>
    /// <returns>The hash map of the bindings.</returns>
    /// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="elements"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// O(n log n). A later binding of a key replaces an earlier one, unless the value comparer finds the two values equal:
    /// then the earlier binding stays, its key included (ADR 0001, D14).
    /// </remarks>
    [<CompiledName "OfArray">]
    member ofArray : elements : KeyValuePair<'Key, 'Value> array -> HashMap<'Key, 'Value>

    /// <summary>
    /// Creates a hash map with the comparers from an array of reference tuples.
    /// </summary>
    /// <param name="elements">The input array of key/value tuples.</param>
    /// <returns>The hash map of the bindings.</returns>
    /// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="elements"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// O(n log n). A later binding of a key replaces an earlier one, unless the value comparer finds the two values equal:
    /// then the earlier binding stays, its key included (ADR 0001, D14).
    /// </remarks>
    [<CompiledName "OfArray">]
    member ofArray : elements : ('Key * 'Value) array -> HashMap<'Key, 'Value>

    /// <summary>
    /// Creates a hash map with the comparers from an array of struct tuples.
    /// </summary>
    /// <param name="elements">The input array of key/value tuples.</param>
    /// <returns>The hash map of the bindings.</returns>
    /// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="elements"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// O(n log n). A later binding of a key replaces an earlier one, unless the value comparer finds the two values equal:
    /// then the earlier binding stays, its key included (ADR 0001, D14).
    /// </remarks>
    [<CompiledName "OfArray">]
    member ofArray : elements : struct ('Key * 'Value) array -> HashMap<'Key, 'Value>

    /// <summary>
    /// Creates a hash map with the comparers from a list of key/value pairs.
    /// </summary>
    /// <param name="elements">The input list of key/value pairs.</param>
    /// <returns>The hash map of the bindings.</returns>
    /// <remarks>
    /// O(n log n). A later binding of a key replaces an earlier one, unless the value comparer finds the two values equal:
    /// then the earlier binding stays, its key included (ADR 0001, D14).
    /// </remarks>
    [<CompiledName "OfList">]
    member ofList : elements : KeyValuePair<'Key, 'Value> list -> HashMap<'Key, 'Value>

    /// <summary>
    /// Creates a hash map with the comparers from a list of reference tuples.
    /// </summary>
    /// <param name="elements">The input list of key/value tuples.</param>
    /// <returns>The hash map of the bindings.</returns>
    /// <remarks>
    /// O(n log n). A later binding of a key replaces an earlier one, unless the value comparer finds the two values equal:
    /// then the earlier binding stays, its key included (ADR 0001, D14).
    /// </remarks>
    [<CompiledName "OfList">]
    member ofList : elements : ('Key * 'Value) list -> HashMap<'Key, 'Value>

    /// <summary>
    /// Creates a hash map with the comparers from a list of struct tuples.
    /// </summary>
    /// <param name="elements">The input list of key/value tuples.</param>
    /// <returns>The hash map of the bindings.</returns>
    /// <remarks>
    /// O(n log n). A later binding of a key replaces an earlier one, unless the value comparer finds the two values equal:
    /// then the earlier binding stays, its key included (ADR 0001, D14).
    /// </remarks>
    [<CompiledName "OfList">]
    member ofList : elements : struct ('Key * 'Value) list -> HashMap<'Key, 'Value>

    /// <summary>
    /// Creates a hash map with the comparers from a read-only span of key/value pairs.
    /// </summary>
    /// <param name="span">The input span of key/value pairs, which the member copies into the hash map.</param>
    /// <returns>The hash map of the bindings.</returns>
    /// <remarks>
    /// <para>
    /// O(n log n). A later binding of a key replaces an earlier one, unless the value comparer finds the two values equal:
    /// then the earlier binding stays, its key included (ADR 0001, D14). A repeated key never throws.
    /// </para>
    /// <para>
    /// With the default comparer of the values the member wraps
    /// <see cref="M:System.Collections.Immutable.ImmutableDictionary.CreateRangeWithOverwrite``2(System.Collections.Generic.IEqualityComparer{``0},System.ReadOnlySpan{System.Collections.Generic.KeyValuePair{``0,``1}})"/>.
    /// That overload has no value comparer, so any other value comparer sets the bindings one by one in a builder with the
    /// comparers, which gives the same result.
    /// </para>
    /// <para>
    /// A <see cref="T:System.Span`1"/> goes to
    /// <see cref="M:FSharp.Collections.Immutable.CustomComparersHashMap`2.OfSpan(System.Span{System.Collections.Generic.KeyValuePair{`0,`1}})"/>.
    /// An array converts to the parameter when the types of the struct are known; otherwise F# reports a type mismatch,
    /// and the argument must be a <see cref="T:System.ReadOnlySpan`1"/>.
    /// </para>
    /// </remarks>
    [<CompiledName "OfReadOnlySpan">]
    member ofReadOnlySpan : span : ReadOnlySpan<KeyValuePair<'Key, 'Value>> -> HashMap<'Key, 'Value>

    /// <summary>
    /// Creates a hash map with the comparers from a sequence of key/value pairs, such as a dictionary or another map.
    /// </summary>
    /// <param name="elements">The input sequence of key/value pairs.</param>
    /// <returns>
    /// The hash map of the bindings. A hash map with the key comparer of the struct comes back itself when it also has the
    /// value comparer, and with the value comparer otherwise.
    /// </returns>
    /// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="elements"/> is <see langword="null"/> or a default flat list.</exception>
    /// <remarks>
    /// <para>
    /// O(n log n); O(1) for a hash map whose key comparer is the instance that the struct holds, whose keys stay distinct,
    /// through
    /// <see cref="M:System.Collections.Immutable.ImmutableDictionary`2.WithComparers(System.Collections.Generic.IEqualityComparer{`0},System.Collections.Generic.IEqualityComparer{`1})"/>.
    /// A hash map with another key comparer is rebuilt, even when the comparer is equivalent.
    /// </para>
    /// <para>
    /// A later binding of a key replaces an earlier one, unless the value comparer finds the two values equal: then the
    /// earlier binding stays, its key included (ADR 0001, D14). Keys that only the comparer of the struct considers equal
    /// merge by the same rule, in the order of enumeration of the map, where <c>WithComparers</c> throws
    /// <see cref="T:System.ArgumentException"/> for another value of them.
    /// </para>
    /// <para>
    /// The value comparer of the struct replaces the one of the map. A struct from
    /// <see cref="M:FSharp.Collections.Immutable.HashMap.ForKeyComparer``2(System.Collections.Generic.IEqualityComparer{``0})"/>
    /// compares the values with the default comparer, where <c>WithComparers</c> with a key comparer keeps the value
    /// comparer of the map: <c>HashMap.forComparers keyComparer (HashMap.valueComparer table)</c> keeps it.
    /// </para>
    /// </remarks>
    [<CompiledName "OfSeq">]
    member ofSeq : elements : seq<KeyValuePair<'Key, 'Value>> -> HashMap<'Key, 'Value>

    /// <summary>
    /// Creates a hash map with the comparers from a sequence of reference tuples.
    /// </summary>
    /// <param name="elements">The input sequence of key/value tuples.</param>
    /// <returns>The hash map of the bindings.</returns>
    /// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="elements"/> is <see langword="null"/> or a default flat list.</exception>
    /// <remarks>
    /// O(n log n). A later binding of a key replaces an earlier one, unless the value comparer finds the two values equal:
    /// then the earlier binding stays, its key included (ADR 0001, D14).
    /// </remarks>
    [<CompiledName "OfSeq">]
    member ofSeq : elements : seq<'Key * 'Value> -> HashMap<'Key, 'Value>

    /// <summary>
    /// Creates a hash map with the comparers from a sequence of struct tuples.
    /// </summary>
    /// <param name="elements">The input sequence of key/value tuples.</param>
    /// <returns>The hash map of the bindings.</returns>
    /// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="elements"/> is <see langword="null"/> or a default flat list.</exception>
    /// <remarks>
    /// O(n log n). A later binding of a key replaces an earlier one, unless the value comparer finds the two values equal:
    /// then the earlier binding stays, its key included (ADR 0001, D14).
    /// </remarks>
    [<CompiledName "OfSeq">]
    member ofSeq : elements : seq<struct ('Key * 'Value)> -> HashMap<'Key, 'Value>

    /// <summary>
    /// Creates a hash map with the comparers of the elements of a sequence under the keys that a projection returns for them.
    /// </summary>
    /// <param name="projection">The function that returns the key of an element.</param>
    /// <param name="source">The input sequence, whose elements become the values.</param>
    /// <returns>The hash map of the elements under their keys.</returns>
    /// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="source"/> is <see langword="null"/> or a default flat list.</exception>
    /// <exception cref="T:System.ArgumentException">
    /// Thrown when two elements have equal keys and are not equal in the value comparer.
    /// </exception>
    /// <remarks>
    /// O(n log n). The member wraps
    /// <see cref="M:System.Collections.Immutable.ImmutableDictionary.ToImmutableDictionary``3(System.Collections.Generic.IEnumerable{``0},System.Func{``0,``1},System.Func{``0,``2},System.Collections.Generic.IEqualityComparer{``1},System.Collections.Generic.IEqualityComparer{``2})"/>,
    /// which rejects a repeated key with another element and accepts one with an element that the value comparer finds
    /// equal: the first element stays, its key included (ADR 0001, D14). The value of a binding is the element itself;
    /// with an element selector, <c>(HashMap.forComparers keyComparer valueComparer).empty |> HashMap.addRange (source |> Seq.map (fun x -> KeyValuePair (key x, element x)))</c>
    /// rejects a repeated key like <c>ToImmutableDictionary</c> does, and <c>ofSeq</c> of the mapped pairs lets the last
    /// binding win.
    /// </remarks>
    [<CompiledName "OfSeqBy">]
    member ofSeqBy : projection : ('Value -> 'Key) -> source : seq<'Value> -> HashMap<'Key, 'Value>

    /// <summary>
    /// Creates a hash map with the comparers from a span of key/value pairs.
    /// </summary>
    /// <param name="span">The input span of key/value pairs, which the member copies into the hash map.</param>
    /// <returns>The hash map of the bindings.</returns>
    /// <remarks>
    /// The BCL creates from a read-only span only, so the member passes the span to
    /// <see cref="M:FSharp.Collections.Immutable.CustomComparersHashMap`2.OfReadOnlySpan(System.ReadOnlySpan{System.Collections.Generic.KeyValuePair{`0,`1}})"/>,
    /// which has the cost, the rule of a repeated key and the rule of equal values of that member (ADR 0001, D14). An array
    /// converts to the parameter when the types of the struct are known.
    /// </remarks>
    [<CompiledName "OfSpan">]
    member ofSpan : span : Span<KeyValuePair<'Key, 'Value>> -> HashMap<'Key, 'Value>

    /// <summary>Creates a hash map of one binding with the comparers.</summary>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    /// <returns>The hash map of the binding.</returns>
    /// <remarks>O(1).</remarks>
    [<CompiledName "Singleton">]
    member singleton : key : 'Key -> value : 'Value -> HashMap<'Key, 'Value>

    // ---- Builder ----

    /// <summary>
    /// Creates a hash map with the comparers with a builder that an action fills.
    /// </summary>
    /// <param name="action">The action that sets the bindings in the builder, which has the comparers.</param>
    /// <returns>The hash map of the bindings that the action set.</returns>
    /// <remarks>
    /// O(1) plus the cost of the action, which sets each binding of the builder in O(log n), plus freezing the nodes that
    /// the action changed: the hash map shares the nodes of the builder instead of copying the entries. The builder is the
    /// one that <see cref="M:FSharp.Collections.Immutable.CustomComparersHashMap`2.CreateBuilder"/> creates, and the member
    /// returns it with <see cref="M:System.Collections.Immutable.ImmutableDictionary`2.Builder.ToImmutable"/>. The indexer
    /// of the builder keeps the earlier binding of a key for a value that the value comparer finds equal, its key included
    /// (ADR 0001, D14).
    /// </remarks>
    [<CompiledName "Build">]
    member build : action : (HashMap<'Key, 'Value>.Builder -> unit) -> HashMap<'Key, 'Value>

    /// <summary>
    /// Creates an empty builder with the comparers.
    /// </summary>
    /// <returns>The builder.</returns>
    /// <remarks>
    /// O(1). The member wraps
    /// <see cref="M:System.Collections.Immutable.ImmutableDictionary.CreateBuilder``2(System.Collections.Generic.IEqualityComparer{``0},System.Collections.Generic.IEqualityComparer{``1})"/>.
    /// </remarks>
    [<CompiledName "CreateBuilder">]
    member builder : unit -> HashMap<'Key, 'Value>.Builder

    // ---- Properties ----

    /// <summary>
    /// The equality comparer of the keys.
    /// </summary>
    /// <remarks>
    /// O(1). The default value of the struct holds no comparer and returns
    /// <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>.
    /// </remarks>
    member KeyComparer : IEqualityComparer<'Key>

    /// <summary>
    /// The equality comparer of the values.
    /// </summary>
    /// <remarks>
    /// O(1). The default value of the struct holds no comparer and returns
    /// <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>.
    /// </remarks>
    member ValueComparer : IEqualityComparer<'Value>
