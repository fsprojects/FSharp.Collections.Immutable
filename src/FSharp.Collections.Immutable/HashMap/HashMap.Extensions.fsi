/// <summary>
/// The creation functions of <see cref="T:FSharp.Collections.Immutable.HashMap`2"/> that take bindings in every shape: static
/// members of <see cref="T:System.Collections.Immutable.ImmutableDictionary`2"/>, each overloaded for key/value pairs,
/// reference tuples and struct tuples (ADR 0001, D13).
/// </summary>
/// <remarks>
/// <para>
/// Through the abbreviation <see cref="T:FSharp.Collections.Immutable.HashMap`2"/>, <c>HashMap.ofSeq pairs</c> reaches these
/// members, because the module <see cref="T:FSharp.Collections.Immutable.HashMap"/> has no functions with their names, and
/// overload resolution picks the shape of the bindings; <c>pairs |> HashMap.ofSeq</c> works too. Inside generic code, the
/// bindings need a type annotation (FS0041).
/// </para>
/// <para>
/// The members create hash maps with the default comparers,
/// <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>: they are the members of
/// <see cref="T:FSharp.Collections.Immutable.CustomComparersHashMap`2"/> with those comparers. A later binding of a key
/// replaces an earlier one, as in
/// <see cref="M:Microsoft.FSharp.Collections.MapModule.OfSeq``2(System.Collections.Generic.IEnumerable{System.Tuple{``0,``1}})"/>,
/// unless the default comparer of the values finds the two values equal: then the earlier binding stays, its key included,
/// as in the builder of <see cref="T:System.Collections.Immutable.ImmutableDictionary`2"/> (ADR 0001, D14).
/// </para>
/// <para>
/// A repeated key never throws, where
/// <see cref="M:System.Collections.Immutable.ImmutableDictionary.CreateRange``2(System.Collections.Generic.IEnumerable{System.Collections.Generic.KeyValuePair{``0,``1}})"/>
/// and <c>ToImmutableDictionary</c> reject another value of it. To reject it, add the bindings to an empty map:
/// <c>HashMap.empty |> HashMap.addRange pairs</c>, or <c>(HashMap.forComparers keyComparer valueComparer).empty |> HashMap.addRange pairs</c>
/// for other comparers; it throws <see cref="T:System.ArgumentException"/> like the BCL.
/// </para>
/// <para>
/// Static extension members are an F# feature: C# does not see them as members of
/// <see cref="T:System.Collections.Immutable.ImmutableDictionary`2"/>, and a reflection over the module
/// <see cref="T:FSharp.Collections.Immutable.HashMap"/> lists none of <c>ofArray</c>, <c>ofList</c> and <c>ofSeq</c>.
/// They compile to public static methods of this module, <c>HashMapExtensions.OfArray</c>, <c>OfList</c> and <c>OfSeq</c>,
/// with one overload per shape. C# can call those, or the members of
/// <see cref="T:FSharp.Collections.Immutable.CustomComparersHashMap`2"/>, or the BCL.
/// </para>
/// </remarks>
[<AutoOpen>]
module FSharp.Collections.Immutable.HashMapExtensions

open System.Collections.Generic
open System.Collections.Immutable

// The extended type repeats the constraint of the BCL type (FS0957)
type ImmutableDictionary<'Key, 'Value when 'Key : not null> with

    /// <summary>
    /// Creates a hash map from an array of key/value pairs.
    /// </summary>
    /// <param name="elements">The input array of key/value pairs.</param>
    /// <returns>The hash map of the bindings, with the default comparers.</returns>
    /// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="elements"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// O(n log n). A later binding of a key replaces an earlier one, unless the default comparer of the values finds the
    /// two values equal: then the earlier binding stays, its key included (ADR 0001, D14).
    /// </remarks>
    [<CompiledName "OfArray">]
    static member ofArray : elements : KeyValuePair<'Key, 'Value> array -> HashMap<'Key, 'Value>

    /// <summary>
    /// Creates a hash map from an array of reference tuples.
    /// </summary>
    /// <param name="elements">The input array of key/value tuples.</param>
    /// <returns>The hash map of the bindings, with the default comparers.</returns>
    /// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="elements"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// O(n log n). A later binding of a key replaces an earlier one, unless the default comparer of the values finds the
    /// two values equal: then the earlier binding stays, its key included (ADR 0001, D14).
    /// </remarks>
    [<CompiledName "OfArray">]
    static member ofArray : elements : ('Key * 'Value) array -> HashMap<'Key, 'Value>

    /// <summary>
    /// Creates a hash map from an array of struct tuples.
    /// </summary>
    /// <param name="elements">The input array of key/value tuples.</param>
    /// <returns>The hash map of the bindings, with the default comparers.</returns>
    /// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="elements"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// O(n log n). A later binding of a key replaces an earlier one, unless the default comparer of the values finds the
    /// two values equal: then the earlier binding stays, its key included (ADR 0001, D14).
    /// </remarks>
    [<CompiledName "OfArray">]
    static member ofArray : elements : struct ('Key * 'Value) array -> HashMap<'Key, 'Value>

    /// <summary>
    /// Creates a hash map from a list of key/value pairs.
    /// </summary>
    /// <param name="elements">The input list of key/value pairs.</param>
    /// <returns>The hash map of the bindings, with the default comparers.</returns>
    /// <remarks>
    /// O(n log n). A later binding of a key replaces an earlier one, unless the default comparer of the values finds the
    /// two values equal: then the earlier binding stays, its key included (ADR 0001, D14).
    /// </remarks>
    [<CompiledName "OfList">]
    static member ofList : elements : KeyValuePair<'Key, 'Value> list -> HashMap<'Key, 'Value>

    /// <summary>
    /// Creates a hash map from a list of reference tuples.
    /// </summary>
    /// <param name="elements">The input list of key/value tuples.</param>
    /// <returns>The hash map of the bindings, with the default comparers.</returns>
    /// <remarks>
    /// O(n log n). A later binding of a key replaces an earlier one, unless the default comparer of the values finds the
    /// two values equal: then the earlier binding stays, its key included (ADR 0001, D14).
    /// </remarks>
    [<CompiledName "OfList">]
    static member ofList : elements : ('Key * 'Value) list -> HashMap<'Key, 'Value>

    /// <summary>
    /// Creates a hash map from a list of struct tuples.
    /// </summary>
    /// <param name="elements">The input list of key/value tuples.</param>
    /// <returns>The hash map of the bindings, with the default comparers.</returns>
    /// <remarks>
    /// O(n log n). A later binding of a key replaces an earlier one, unless the default comparer of the values finds the
    /// two values equal: then the earlier binding stays, its key included (ADR 0001, D14).
    /// </remarks>
    [<CompiledName "OfList">]
    static member ofList : elements : struct ('Key * 'Value) list -> HashMap<'Key, 'Value>

    /// <summary>
    /// Creates a hash map from a sequence of key/value pairs, such as a dictionary or another map.
    /// </summary>
    /// <param name="elements">The input sequence of key/value pairs.</param>
    /// <returns>
    /// The hash map of the bindings, with the default comparers. A hash map with the default comparers comes back itself.
    /// </returns>
    /// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="elements"/> is <see langword="null"/> or a default flat list.</exception>
    /// <remarks>
    /// O(n log n); O(1) for a hash map with the default key comparer. A later binding of a key replaces an earlier one,
    /// unless the default comparer of the values finds the two values equal: then the earlier binding stays, its key
    /// included (ADR 0001, D14). Keys that only the default comparer considers equal follow the same rule in the order of
    /// enumeration.
    /// </remarks>
    [<CompiledName "OfSeq">]
    static member ofSeq : elements : seq<KeyValuePair<'Key, 'Value>> -> HashMap<'Key, 'Value>

    /// <summary>
    /// Creates a hash map from a sequence of reference tuples.
    /// </summary>
    /// <param name="elements">The input sequence of key/value tuples.</param>
    /// <returns>The hash map of the bindings, with the default comparers.</returns>
    /// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="elements"/> is <see langword="null"/> or a default flat list.</exception>
    /// <remarks>
    /// O(n log n). A later binding of a key replaces an earlier one, unless the default comparer of the values finds the
    /// two values equal: then the earlier binding stays, its key included (ADR 0001, D14).
    /// </remarks>
    [<CompiledName "OfSeq">]
    static member ofSeq : elements : seq<'Key * 'Value> -> HashMap<'Key, 'Value>

    /// <summary>
    /// Creates a hash map from a sequence of struct tuples.
    /// </summary>
    /// <param name="elements">The input sequence of key/value tuples.</param>
    /// <returns>The hash map of the bindings, with the default comparers.</returns>
    /// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="elements"/> is <see langword="null"/> or a default flat list.</exception>
    /// <remarks>
    /// O(n log n). A later binding of a key replaces an earlier one, unless the default comparer of the values finds the
    /// two values equal: then the earlier binding stays, its key included (ADR 0001, D14).
    /// </remarks>
    [<CompiledName "OfSeq">]
    static member ofSeq : elements : seq<struct ('Key * 'Value)> -> HashMap<'Key, 'Value>
