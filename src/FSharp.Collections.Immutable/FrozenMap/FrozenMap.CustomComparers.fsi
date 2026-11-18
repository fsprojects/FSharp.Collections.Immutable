namespace FSharp.Collections.Immutable

open System
open System.Collections.Generic

/// <summary>
/// The equality comparer of the keys of frozen maps, with the creation functions of
/// <see cref="T:FSharp.Collections.Immutable.FrozenMap"/> that create frozen maps with it (ADR 0001, D12).
/// </summary>
/// <remarks>
/// <para>
/// <see cref="M:FSharp.Collections.Immutable.FrozenMap.ForKeyComparer``2(System.Collections.Generic.IEqualityComparer{``0})"/>
/// creates the struct. Its members have the names of the creation functions of the module and the key comparer of the
/// struct. A frozen map has no comparer of its values and no builder, so the struct holds the key comparer alone and has no
/// <c>builder</c>.
/// </para>
/// <para>
/// <c>ofArray</c>, <c>ofList</c> and <c>ofSeq</c> take the bindings as key/value pairs, reference tuples or struct
/// tuples (ADR 0001, D13); <c>ofReadOnlySpan</c> and <c>ofSpan</c> take a span of key/value pairs. A later binding of a
/// key replaces the value of an earlier one, as in
/// <see cref="M:Microsoft.FSharp.Collections.MapModule.OfSeq``2(System.Collections.Generic.IEnumerable{System.Tuple{``0,``1}})"/>,
/// and the key of the earlier binding stays, as in
/// <see cref="M:System.Collections.Frozen.FrozenDictionary.ToFrozenDictionary``2(System.Collections.Generic.IEnumerable{System.Collections.Generic.KeyValuePair{``0,``1}},System.Collections.Generic.IEqualityComparer{``0})"/>
/// (ADR 0001, D14). The kept key shows when the comparer of the struct finds two distinct keys equal. <c>ofSeq</c> also
/// takes an existing map, because a map is a sequence of key/value pairs, and gives its bindings the key comparer of the
/// struct; a frozen map whose comparer equals that comparer comes back itself, as in <c>ToFrozenDictionary</c>.
/// </para>
/// <para>
/// The struct only holds the comparer, so creating it allocates nothing. Its default value holds no comparer and stands for
/// the default comparer, <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>, which the BCL takes for a
/// <see langword="null"/> comparer (ADR 0001, D14).
/// </para>
/// </remarks>
/// <example id="customComparersFrozenMap-1">
/// <code lang="fsharp">
/// let table = (FrozenMap.forKeyComparer StringComparer.OrdinalIgnoreCase).ofSeq [ "one", 1; "ONE", 2; "two", 3 ]
/// </code>
/// Evaluates to a frozen map whose keys ignore the case, with the key "one" bound to 2 and the key "two" bound to 3.
/// </example>
[<Struct; NoComparison; NoEquality>]
type CustomComparersFrozenMap<'Key, 'Value when 'Key : not null> =

    val private keyComparer : IEqualityComparer<'Key> | null

    /// <summary>Holds the equality comparer of the keys.</summary>
    /// <param name="keyComparer">The equality comparer of the keys.</param>
    /// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="keyComparer"/> is <see langword="null"/>.</exception>
    new : keyComparer : IEqualityComparer<'Key> -> CustomComparersFrozenMap<'Key, 'Value>

    // ---- Creation ----

    /// <summary>
    /// The empty frozen map with the key comparer.
    /// </summary>
    /// <remarks>
    /// O(1). The property wraps
    /// <see cref="M:System.Collections.Frozen.FrozenDictionary.ToFrozenDictionary``2(System.Collections.Generic.IEnumerable{System.Collections.Generic.KeyValuePair{``0,``1}},System.Collections.Generic.IEqualityComparer{``0})"/>,
    /// which returns <see cref="P:System.Collections.Frozen.FrozenDictionary`2.Empty"/> for the default comparer.
    /// </remarks>
    [<CompiledName "Empty">]
    member empty : FrozenMap<'Key, 'Value>

    /// <summary>
    /// Creates a frozen map with the key comparer from an array of key/value pairs.
    /// </summary>
    /// <param name="elements">The input array of key/value pairs.</param>
    /// <returns>The frozen map of the bindings.</returns>
    /// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="elements"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// O(n). A later binding of a key replaces the value of an earlier one, whose key stays (ADR 0001, D14).
    /// </remarks>
    [<CompiledName "OfArray">]
    member ofArray : elements : KeyValuePair<'Key, 'Value> array -> FrozenMap<'Key, 'Value>

    /// <summary>
    /// Creates a frozen map with the key comparer from an array of reference tuples.
    /// </summary>
    /// <param name="elements">The input array of key/value tuples.</param>
    /// <returns>The frozen map of the bindings.</returns>
    /// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="elements"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// O(n). A later binding of a key replaces the value of an earlier one, whose key stays (ADR 0001, D14).
    /// </remarks>
    [<CompiledName "OfArray">]
    member ofArray : elements : ('Key * 'Value) array -> FrozenMap<'Key, 'Value>

    /// <summary>
    /// Creates a frozen map with the key comparer from an array of struct tuples.
    /// </summary>
    /// <param name="elements">The input array of key/value tuples.</param>
    /// <returns>The frozen map of the bindings.</returns>
    /// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="elements"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// O(n). A later binding of a key replaces the value of an earlier one, whose key stays (ADR 0001, D14).
    /// </remarks>
    [<CompiledName "OfArray">]
    member ofArray : elements : struct ('Key * 'Value) array -> FrozenMap<'Key, 'Value>

    /// <summary>
    /// Creates a frozen map with the key comparer from a list of key/value pairs.
    /// </summary>
    /// <param name="elements">The input list of key/value pairs.</param>
    /// <returns>The frozen map of the bindings.</returns>
    /// <remarks>
    /// O(n). A later binding of a key replaces the value of an earlier one, whose key stays (ADR 0001, D14).
    /// </remarks>
    [<CompiledName "OfList">]
    member ofList : elements : KeyValuePair<'Key, 'Value> list -> FrozenMap<'Key, 'Value>

    /// <summary>
    /// Creates a frozen map with the key comparer from a list of reference tuples.
    /// </summary>
    /// <param name="elements">The input list of key/value tuples.</param>
    /// <returns>The frozen map of the bindings.</returns>
    /// <remarks>
    /// O(n). A later binding of a key replaces the value of an earlier one, whose key stays (ADR 0001, D14).
    /// </remarks>
    [<CompiledName "OfList">]
    member ofList : elements : ('Key * 'Value) list -> FrozenMap<'Key, 'Value>

    /// <summary>
    /// Creates a frozen map with the key comparer from a list of struct tuples.
    /// </summary>
    /// <param name="elements">The input list of key/value tuples.</param>
    /// <returns>The frozen map of the bindings.</returns>
    /// <remarks>
    /// O(n). A later binding of a key replaces the value of an earlier one, whose key stays (ADR 0001, D14).
    /// </remarks>
    [<CompiledName "OfList">]
    member ofList : elements : struct ('Key * 'Value) list -> FrozenMap<'Key, 'Value>

    /// <summary>
    /// Creates a frozen map with the key comparer from a read-only span of key/value pairs.
    /// </summary>
    /// <param name="span">The input span of key/value pairs.</param>
    /// <returns>The frozen map of the bindings.</returns>
    /// <remarks>
    /// <para>
    /// O(n). The member wraps
    /// <see cref="M:System.Collections.Frozen.FrozenDictionary.Create``2(System.Collections.Generic.IEqualityComparer{``0},System.ReadOnlySpan{System.Collections.Generic.KeyValuePair{``0,``1}})"/>
    /// and reads the span itself, where <c>ofArray</c> and <c>ofSeq</c> need an array or a sequence, which copies the span
    /// first.
    /// </para>
    /// <para>
    /// A repeated key never throws. A later binding of a key replaces the value of an earlier one, whose key stays (ADR 0001,
    /// D14). A <see cref="T:System.Span`1"/> or an array converts to the parameter only when the type arguments of the struct
    /// are known; where the call leaves them to inference, F# reports FS0001. <c>ofSpan</c> and <c>ofArray</c> take a span
    /// and an array whatever the call leaves to inference.
    /// </para>
    /// </remarks>
    [<CompiledName "OfReadOnlySpan">]
    member ofReadOnlySpan : span : ReadOnlySpan<KeyValuePair<'Key, 'Value>> -> FrozenMap<'Key, 'Value>

    /// <summary>
    /// Creates a frozen map with the key comparer from a sequence of key/value pairs, such as a dictionary or another map.
    /// </summary>
    /// <param name="elements">The input sequence of key/value pairs.</param>
    /// <returns>
    /// The frozen map of the bindings. A frozen map whose comparer equals the key comparer of the struct comes back itself,
    /// as in
    /// <see cref="M:System.Collections.Frozen.FrozenDictionary.ToFrozenDictionary``2(System.Collections.Generic.IEnumerable{System.Collections.Generic.KeyValuePair{``0,``1}},System.Collections.Generic.IEqualityComparer{``0})"/>
    /// (ADR 0001, D14).
    /// </returns>
    /// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="elements"/> is <see langword="null"/> or a default flat list.</exception>
    /// <remarks>
    /// O(n); O(1) for a frozen map whose comparer equals the key comparer of the struct, because its keys are distinct in
    /// that comparer. A later binding of a key replaces the value of an earlier one, and the key of the earlier binding
    /// stays (ADR 0001, D14): of the keys that only the comparer of the struct considers equal, the map keeps the first one
    /// in the order of enumeration with the value of the last one.
    /// </remarks>
    [<CompiledName "OfSeq">]
    member ofSeq : elements : seq<KeyValuePair<'Key, 'Value>> -> FrozenMap<'Key, 'Value>

    /// <summary>
    /// Creates a frozen map with the key comparer from a sequence of reference tuples.
    /// </summary>
    /// <param name="elements">The input sequence of key/value tuples.</param>
    /// <returns>The frozen map of the bindings.</returns>
    /// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="elements"/> is <see langword="null"/> or a default flat list.</exception>
    /// <remarks>
    /// O(n). A later binding of a key replaces the value of an earlier one, whose key stays (ADR 0001, D14).
    /// </remarks>
    [<CompiledName "OfSeq">]
    member ofSeq : elements : seq<'Key * 'Value> -> FrozenMap<'Key, 'Value>

    /// <summary>
    /// Creates a frozen map with the key comparer from a sequence of struct tuples.
    /// </summary>
    /// <param name="elements">The input sequence of key/value tuples.</param>
    /// <returns>The frozen map of the bindings.</returns>
    /// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="elements"/> is <see langword="null"/> or a default flat list.</exception>
    /// <remarks>
    /// O(n). A later binding of a key replaces the value of an earlier one, whose key stays (ADR 0001, D14).
    /// </remarks>
    [<CompiledName "OfSeq">]
    member ofSeq : elements : seq<struct ('Key * 'Value)> -> FrozenMap<'Key, 'Value>

    /// <summary>
    /// Creates a frozen map with the key comparer of the elements of a sequence under the keys that a projection returns for
    /// them.
    /// </summary>
    /// <param name="projection">The function that returns the key of an element.</param>
    /// <param name="source">The input sequence, whose elements become the values.</param>
    /// <returns>The frozen map of the elements under their keys.</returns>
    /// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="source"/> is <see langword="null"/> or a default flat list.</exception>
    /// <exception cref="T:System.ArgumentException">
    /// Thrown when two elements have keys that are equal in the key comparer, even when the elements are equal.
    /// </exception>
    /// <remarks>
    /// O(n). The member wraps
    /// <see cref="M:System.Collections.Frozen.FrozenDictionary.ToFrozenDictionary``2(System.Collections.Generic.IEnumerable{``0},System.Func{``0,``1},System.Collections.Generic.IEqualityComparer{``1})"/>,
    /// which rejects every repeated key (ADR 0001, D14).
    /// </remarks>
    [<CompiledName "OfSeqBy">]
    member ofSeqBy : projection : ('Value -> 'Key) -> source : seq<'Value> -> FrozenMap<'Key, 'Value>

    /// <summary>
    /// Creates a frozen map with the key comparer from a span of key/value pairs.
    /// </summary>
    /// <param name="span">The input span of key/value pairs.</param>
    /// <returns>The frozen map of the bindings.</returns>
    /// <remarks>
    /// O(n). The member is <c>ofReadOnlySpan</c> for a <see cref="T:System.Span`1"/>. A repeated key never throws. A later
    /// binding of a key replaces the value of an earlier one, whose key stays (ADR 0001, D14).
    /// </remarks>
    [<CompiledName "OfSpan">]
    member ofSpan : span : Span<KeyValuePair<'Key, 'Value>> -> FrozenMap<'Key, 'Value>

    /// <summary>Creates a frozen map of one binding with the key comparer.</summary>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    /// <returns>The frozen map of the binding.</returns>
    /// <remarks>O(1).</remarks>
    [<CompiledName "Singleton">]
    member singleton : key : 'Key -> value : 'Value -> FrozenMap<'Key, 'Value>

    // ---- Properties ----

    /// <summary>
    /// The equality comparer of the keys.
    /// </summary>
    /// <remarks>
    /// O(1). The default value of the struct holds no comparer and returns
    /// <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>, the comparer that the BCL takes for a
    /// <see langword="null"/> comparer (ADR 0001, D14).
    /// </remarks>
    member KeyComparer : IEqualityComparer<'Key>
