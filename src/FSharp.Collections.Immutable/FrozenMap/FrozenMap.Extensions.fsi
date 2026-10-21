/// <summary>
/// The creation functions of <see cref="T:FSharp.Collections.Immutable.FrozenMap`2"/> that take bindings in every shape:
/// static members of <see cref="T:System.Collections.Frozen.FrozenDictionary`2"/>, each overloaded for key/value pairs,
/// reference tuples and struct tuples (ADR 0001, D13).
/// </summary>
/// <remarks>
/// <para>
/// Through the abbreviation <see cref="T:FSharp.Collections.Immutable.FrozenMap`2"/>, <c>FrozenMap.ofSeq pairs</c> reaches
/// these members, because the module <see cref="T:FSharp.Collections.Immutable.FrozenMap"/> has no functions with their
/// names, and overload resolution picks the shape of the bindings; <c>pairs |> FrozenMap.ofSeq</c> works too. Inside generic
/// code, the bindings need a type annotation (FS0041).
/// </para>
/// <para>
/// The members create frozen maps with the default comparer,
/// <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>: they are the members of
/// <see cref="T:FSharp.Collections.Immutable.CustomComparersFrozenMap`2"/> with that comparer. A later binding of a key
/// replaces the value of an earlier one, as in
/// <see cref="M:Microsoft.FSharp.Collections.MapModule.OfSeq``2(System.Collections.Generic.IEnumerable{System.Tuple{``0,``1}})"/>,
/// and the key of the earlier binding stays, as in
/// <see cref="M:System.Collections.Frozen.FrozenDictionary.ToFrozenDictionary``2(System.Collections.Generic.IEnumerable{System.Collections.Generic.KeyValuePair{``0,``1}},System.Collections.Generic.IEqualityComparer{``0})"/>
/// (ADR 0001, D14).
/// </para>
/// <para>
/// Static extension members are an F# feature: C# does not see them as members of
/// <see cref="T:System.Collections.Frozen.FrozenDictionary`2"/>, and a reflection over the module
/// <see cref="T:FSharp.Collections.Immutable.FrozenMap"/> lists none of <c>ofArray</c>, <c>ofList</c> and <c>ofSeq</c>.
/// They compile to public static methods of this module, <c>FrozenMapExtensions.OfArray</c>, <c>OfList</c> and <c>OfSeq</c>,
/// with one overload per shape. C# can call those, or the members of
/// <see cref="T:FSharp.Collections.Immutable.CustomComparersFrozenMap`2"/>, or the BCL.
/// </para>
/// </remarks>
[<AutoOpen>]
module FSharp.Collections.Immutable.FrozenMapExtensions

open System.Collections.Frozen
open System.Collections.Generic

// The extended type repeats the constraint of the BCL type (FS0957)
type FrozenDictionary<'Key, 'Value when 'Key : not null> with

    /// <summary>
    /// Creates a frozen map from an array of key/value pairs.
    /// </summary>
    /// <param name="elements">The input array of key/value pairs.</param>
    /// <returns>The frozen map of the bindings, with the default comparer.</returns>
    /// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="elements"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// O(n). A later binding of a key replaces the value of an earlier one, whose key stays (ADR 0001, D14).
    /// </remarks>
    [<CompiledName "OfArray">]
    static member ofArray : elements : KeyValuePair<'Key, 'Value> array -> FrozenMap<'Key, 'Value>

    /// <summary>
    /// Creates a frozen map from an array of reference tuples.
    /// </summary>
    /// <param name="elements">The input array of key/value tuples.</param>
    /// <returns>The frozen map of the bindings, with the default comparer.</returns>
    /// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="elements"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// O(n). A later binding of a key replaces the value of an earlier one, whose key stays (ADR 0001, D14).
    /// </remarks>
    [<CompiledName "OfArray">]
    static member ofArray : elements : ('Key * 'Value) array -> FrozenMap<'Key, 'Value>

    /// <summary>
    /// Creates a frozen map from an array of struct tuples.
    /// </summary>
    /// <param name="elements">The input array of key/value tuples.</param>
    /// <returns>The frozen map of the bindings, with the default comparer.</returns>
    /// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="elements"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// O(n). A later binding of a key replaces the value of an earlier one, whose key stays (ADR 0001, D14).
    /// </remarks>
    [<CompiledName "OfArray">]
    static member ofArray : elements : struct ('Key * 'Value) array -> FrozenMap<'Key, 'Value>

    /// <summary>
    /// Creates a frozen map from a list of key/value pairs.
    /// </summary>
    /// <param name="elements">The input list of key/value pairs.</param>
    /// <returns>The frozen map of the bindings, with the default comparer.</returns>
    /// <remarks>
    /// O(n). A later binding of a key replaces the value of an earlier one, whose key stays (ADR 0001, D14).
    /// </remarks>
    [<CompiledName "OfList">]
    static member ofList : elements : KeyValuePair<'Key, 'Value> list -> FrozenMap<'Key, 'Value>

    /// <summary>
    /// Creates a frozen map from a list of reference tuples.
    /// </summary>
    /// <param name="elements">The input list of key/value tuples.</param>
    /// <returns>The frozen map of the bindings, with the default comparer.</returns>
    /// <remarks>
    /// O(n). A later binding of a key replaces the value of an earlier one, whose key stays (ADR 0001, D14).
    /// </remarks>
    [<CompiledName "OfList">]
    static member ofList : elements : ('Key * 'Value) list -> FrozenMap<'Key, 'Value>

    /// <summary>
    /// Creates a frozen map from a list of struct tuples.
    /// </summary>
    /// <param name="elements">The input list of key/value tuples.</param>
    /// <returns>The frozen map of the bindings, with the default comparer.</returns>
    /// <remarks>
    /// O(n). A later binding of a key replaces the value of an earlier one, whose key stays (ADR 0001, D14).
    /// </remarks>
    [<CompiledName "OfList">]
    static member ofList : elements : struct ('Key * 'Value) list -> FrozenMap<'Key, 'Value>

    /// <summary>
    /// Creates a frozen map from a sequence of key/value pairs, such as a dictionary or another map.
    /// </summary>
    /// <param name="elements">The input sequence of key/value pairs.</param>
    /// <returns>
    /// The frozen map of the bindings, with the default comparer. A frozen map whose comparer equals the default comparer
    /// comes back itself, as in
    /// <see cref="M:System.Collections.Frozen.FrozenDictionary.ToFrozenDictionary``2(System.Collections.Generic.IEnumerable{System.Collections.Generic.KeyValuePair{``0,``1}},System.Collections.Generic.IEqualityComparer{``0})"/>
    /// (ADR 0001, D14).
    /// </returns>
    /// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="elements"/> is <see langword="null"/> or a default flat list.</exception>
    /// <remarks>
    /// O(n); O(1) for a frozen map whose comparer equals the default comparer. A later binding of a key replaces the value
    /// of an earlier one, whose key stays (ADR 0001, D14): of the keys that only the default comparer considers equal, the
    /// map keeps the first one in the order of enumeration with the value of the last one.
    /// </remarks>
    [<CompiledName "OfSeq">]
    static member ofSeq : elements : seq<KeyValuePair<'Key, 'Value>> -> FrozenMap<'Key, 'Value>

    /// <summary>
    /// Creates a frozen map from a sequence of reference tuples.
    /// </summary>
    /// <param name="elements">The input sequence of key/value tuples.</param>
    /// <returns>The frozen map of the bindings, with the default comparer.</returns>
    /// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="elements"/> is <see langword="null"/> or a default flat list.</exception>
    /// <remarks>
    /// O(n). A later binding of a key replaces the value of an earlier one, whose key stays (ADR 0001, D14).
    /// </remarks>
    [<CompiledName "OfSeq">]
    static member ofSeq : elements : seq<'Key * 'Value> -> FrozenMap<'Key, 'Value>

    /// <summary>
    /// Creates a frozen map from a sequence of struct tuples.
    /// </summary>
    /// <param name="elements">The input sequence of key/value tuples.</param>
    /// <returns>The frozen map of the bindings, with the default comparer.</returns>
    /// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="elements"/> is <see langword="null"/> or a default flat list.</exception>
    /// <remarks>
    /// O(n). A later binding of a key replaces the value of an earlier one, whose key stays (ADR 0001, D14).
    /// </remarks>
    [<CompiledName "OfSeq">]
    static member ofSeq : elements : seq<struct ('Key * 'Value)> -> FrozenMap<'Key, 'Value>
