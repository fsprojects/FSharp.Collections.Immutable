/// <summary>
/// The creation functions of <see cref="T:FSharp.Collections.Immutable.SortedMap`2"/> that take bindings in every shape:
/// static members of <see cref="T:System.Collections.Immutable.ImmutableSortedDictionary`2"/>, each overloaded for
/// key/value pairs, reference tuples and struct tuples (ADR 0001, D13).
/// </summary>
/// <remarks>
/// <para>
/// Through the abbreviation <see cref="T:FSharp.Collections.Immutable.SortedMap`2"/>, <c>SortedMap.ofSeq pairs</c> reaches
/// these members, because the module <see cref="T:FSharp.Collections.Immutable.SortedMap"/> has no functions with their
/// names, and overload resolution picks the shape of the bindings; <c>pairs |> SortedMap.ofSeq</c> works too. Inside
/// generic code, the bindings need a type annotation (FS0041).
/// </para>
/// <para>
/// The members create sorted maps that order their keys with F# generic comparison,
/// <see cref="M:Microsoft.FSharp.Core.LanguagePrimitives.FastGenericComparer``1"/>, and compare their values with
/// <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>: they are the members of
/// <see cref="T:FSharp.Collections.Immutable.CustomComparersSortedMap`2"/> with those comparers. A later binding of a key
/// replaces an earlier one, as in
/// <see cref="M:Microsoft.FSharp.Collections.MapModule.OfSeq``2(System.Collections.Generic.IEnumerable{System.Tuple{``0,``1}})"/>,
/// unless the default comparer of the values finds the two values equal: then the earlier binding stays, its key included,
/// as in the builder of <see cref="T:System.Collections.Immutable.ImmutableSortedDictionary`2"/> (ADR 0001, D14).
/// </para>
/// <para>
/// F# generic comparison needs keys with F# comparison, and a member cannot constrain the type parameters of the type that
/// it extends (FS0340). Each member therefore declares type parameters of its own, <c>'K</c> and <c>'V</c>, for the keys and
/// the values of the sorted map that it creates.
/// </para>
/// <para>
/// A repeated key never throws, where
/// <see cref="M:System.Collections.Immutable.ImmutableSortedDictionary.CreateRange``2(System.Collections.Generic.IEnumerable{System.Collections.Generic.KeyValuePair{``0,``1}})"/>
/// and <c>ToImmutableSortedDictionary</c> reject another value of it. To reject it, add the bindings to an empty map:
/// <c>SortedMap.empty |> SortedMap.addRange pairs</c>, or
/// <c>(SortedMap.forComparers keyComparer valueComparer).empty |> SortedMap.addRange pairs</c> for other comparers; it throws
/// <see cref="T:System.ArgumentException"/> like the BCL.
/// </para>
/// <para>
/// Static extension members are an F# feature: C# does not see them as members of
/// <see cref="T:System.Collections.Immutable.ImmutableSortedDictionary`2"/>, and a reflection over the module
/// <see cref="T:FSharp.Collections.Immutable.SortedMap"/> lists none of <c>ofArray</c>, <c>ofList</c> and <c>ofSeq</c>.
/// They compile to public static methods of this module, <c>SortedMapExtensions.OfArray</c>, <c>OfList</c> and <c>OfSeq</c>,
/// with one overload per shape. C# can call those, or the members of
/// <see cref="T:FSharp.Collections.Immutable.CustomComparersSortedMap`2"/>, or the BCL.
/// </para>
/// </remarks>
[<AutoOpen>]
module FSharp.Collections.Immutable.SortedMapExtensions

open System.Collections.Generic
open System.Collections.Immutable

// The extended type repeats the constraint of the BCL type (FS0957)
type ImmutableSortedDictionary<'Key, 'Value when 'Key : not null> with

    /// <summary>
    /// Creates a sorted map from an array of key/value pairs.
    /// </summary>
    /// <param name="elements">The input array of key/value pairs.</param>
    /// <returns>The sorted map of the bindings, which orders its keys with F# generic comparison.</returns>
    /// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="elements"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// O(n log n). A later binding of a key replaces an earlier one, unless the default comparer of the values finds the
    /// two values equal: then the earlier binding stays, its key included (ADR 0001, D14).
    /// </remarks>
    [<CompiledName "OfArray">]
    static member ofArray<'K, 'V when 'K : not null and 'K : comparison> :
        elements : KeyValuePair<'K, 'V> array -> SortedMap<'K, 'V>

    /// <summary>
    /// Creates a sorted map from an array of reference tuples.
    /// </summary>
    /// <param name="elements">The input array of key/value tuples.</param>
    /// <returns>The sorted map of the bindings, which orders its keys with F# generic comparison.</returns>
    /// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="elements"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// O(n log n). A later binding of a key replaces an earlier one, unless the default comparer of the values finds the
    /// two values equal: then the earlier binding stays, its key included (ADR 0001, D14).
    /// </remarks>
    [<CompiledName "OfArray">]
    static member ofArray<'K, 'V when 'K : not null and 'K : comparison> : elements : ('K * 'V) array -> SortedMap<'K, 'V>

    /// <summary>
    /// Creates a sorted map from an array of struct tuples.
    /// </summary>
    /// <param name="elements">The input array of key/value tuples.</param>
    /// <returns>The sorted map of the bindings, which orders its keys with F# generic comparison.</returns>
    /// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="elements"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// O(n log n). A later binding of a key replaces an earlier one, unless the default comparer of the values finds the
    /// two values equal: then the earlier binding stays, its key included (ADR 0001, D14).
    /// </remarks>
    [<CompiledName "OfArray">]
    static member ofArray<'K, 'V when 'K : not null and 'K : comparison> : elements : struct ('K * 'V) array -> SortedMap<'K, 'V>

    /// <summary>
    /// Creates a sorted map from a list of key/value pairs.
    /// </summary>
    /// <param name="elements">The input list of key/value pairs.</param>
    /// <returns>The sorted map of the bindings, which orders its keys with F# generic comparison.</returns>
    /// <remarks>
    /// O(n log n). A later binding of a key replaces an earlier one, unless the default comparer of the values finds the
    /// two values equal: then the earlier binding stays, its key included (ADR 0001, D14).
    /// </remarks>
    [<CompiledName "OfList">]
    static member ofList<'K, 'V when 'K : not null and 'K : comparison> :
        elements : KeyValuePair<'K, 'V> list -> SortedMap<'K, 'V>

    /// <summary>
    /// Creates a sorted map from a list of reference tuples.
    /// </summary>
    /// <param name="elements">The input list of key/value tuples.</param>
    /// <returns>The sorted map of the bindings, which orders its keys with F# generic comparison.</returns>
    /// <remarks>
    /// O(n log n). A later binding of a key replaces an earlier one, unless the default comparer of the values finds the
    /// two values equal: then the earlier binding stays, its key included (ADR 0001, D14).
    /// </remarks>
    [<CompiledName "OfList">]
    static member ofList<'K, 'V when 'K : not null and 'K : comparison> : elements : ('K * 'V) list -> SortedMap<'K, 'V>

    /// <summary>
    /// Creates a sorted map from a list of struct tuples.
    /// </summary>
    /// <param name="elements">The input list of key/value tuples.</param>
    /// <returns>The sorted map of the bindings, which orders its keys with F# generic comparison.</returns>
    /// <remarks>
    /// O(n log n). A later binding of a key replaces an earlier one, unless the default comparer of the values finds the
    /// two values equal: then the earlier binding stays, its key included (ADR 0001, D14).
    /// </remarks>
    [<CompiledName "OfList">]
    static member ofList<'K, 'V when 'K : not null and 'K : comparison> : elements : struct ('K * 'V) list -> SortedMap<'K, 'V>

    /// <summary>
    /// Creates a sorted map from a sequence of key/value pairs, such as a dictionary or another map.
    /// </summary>
    /// <param name="elements">The input sequence of key/value pairs.</param>
    /// <returns>
    /// The sorted map of the bindings, which orders its keys with F# generic comparison. A sorted map with the default
    /// comparers comes back itself.
    /// </returns>
    /// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="elements"/> is <see langword="null"/> or a default flat list.</exception>
    /// <remarks>
    /// O(n log n); O(1) for a sorted map that orders its keys with F# generic comparison. A later binding of a key replaces
    /// an earlier one, unless the default comparer of the values finds the two values equal: then the earlier binding
    /// stays, its key included (ADR 0001, D14). Keys that only F# generic comparison considers equal follow the same rule
    /// in the order of enumeration.
    /// </remarks>
    [<CompiledName "OfSeq">]
    static member ofSeq<'K, 'V when 'K : not null and 'K : comparison> : elements : seq<KeyValuePair<'K, 'V>> -> SortedMap<'K, 'V>

    /// <summary>
    /// Creates a sorted map from a sequence of reference tuples.
    /// </summary>
    /// <param name="elements">The input sequence of key/value tuples.</param>
    /// <returns>The sorted map of the bindings, which orders its keys with F# generic comparison.</returns>
    /// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="elements"/> is <see langword="null"/> or a default flat list.</exception>
    /// <remarks>
    /// O(n log n). A later binding of a key replaces an earlier one, unless the default comparer of the values finds the
    /// two values equal: then the earlier binding stays, its key included (ADR 0001, D14).
    /// </remarks>
    [<CompiledName "OfSeq">]
    static member ofSeq<'K, 'V when 'K : not null and 'K : comparison> : elements : seq<'K * 'V> -> SortedMap<'K, 'V>

    /// <summary>
    /// Creates a sorted map from a sequence of struct tuples.
    /// </summary>
    /// <param name="elements">The input sequence of key/value tuples.</param>
    /// <returns>The sorted map of the bindings, which orders its keys with F# generic comparison.</returns>
    /// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="elements"/> is <see langword="null"/> or a default flat list.</exception>
    /// <remarks>
    /// O(n log n). A later binding of a key replaces an earlier one, unless the default comparer of the values finds the
    /// two values equal: then the earlier binding stays, its key included (ADR 0001, D14).
    /// </remarks>
    [<CompiledName "OfSeq">]
    static member ofSeq<'K, 'V when 'K : not null and 'K : comparison> : elements : seq<struct ('K * 'V)> -> SortedMap<'K, 'V>
