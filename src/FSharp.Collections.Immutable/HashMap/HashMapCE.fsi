namespace FSharp.Collections.Immutable

open System
open System.Collections.Generic

/// <summary>
/// The body of a <c>hashMap { }</c> expression: a function that sets the bindings in a builder.
/// </summary>
type HashMapBuffer<'Key, 'Value when 'Key : not null> = HashMap<'Key, 'Value>.Builder -> unit

/// <summary>
/// The builder of the <c>hashMap { }</c> computation expression.
/// </summary>
/// <remarks>
/// <para>
/// The expression collects its bindings in an <see cref="T:System.Collections.Immutable.ImmutableDictionary`2.Builder"/>
/// (ADR 0001, D7). It yields a binding as a struct pair, <c>yield struct (key, value)</c>, or as a
/// <see cref="T:System.Collections.Generic.KeyValuePair`2"/>. A later binding of a key replaces an earlier one, as in
/// <see cref="M:FSharp.Collections.Immutable.HashMapExtensions.OfSeq``2(System.Collections.Generic.IEnumerable{System.ValueTuple{``0,``1}})"/>,
/// unless the default comparer of the values finds the two values equal: then the earlier binding stays, its key
/// included, as the indexer of the builder keeps it (ADR 0001, D14).
/// </para>
/// <para>
/// The keys compare with the key comparer that the constructor takes, or with
/// <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/> when it is omitted; the values compare with the
/// default comparer. The builder is generic in the type of the keys, so that it can hold a comparer of them, and each
/// <c>hashMap { }</c> expression infers the type of its keys. <c>HashMapCE StringComparer.OrdinalIgnoreCase { yield struct ("a", 1) }</c>
/// builds a map whose keys ignore the case.
/// </para>
/// </remarks>
[<Sealed>]
type HashMapCE<'Key when 'Key : not null> =

    /// <summary>
    /// Creates the builder of hash maps whose keys compare with a key comparer.
    /// </summary>
    /// <param name="keyComparer">The equality comparer of the keys of the hash map.</param>
    /// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="keyComparer"/> is <see langword="null"/>.</exception>
    new : keyComparer : IEqualityComparer<'Key> -> HashMapCE<'Key>

    /// <summary>
    /// Creates the builder of hash maps whose keys compare with <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>.
    /// </summary>
    new : unit -> HashMapCE<'Key>

    /// <summary>An expression without bindings.</summary>
    member inline Zero : unit -> HashMapBuffer<'Key, 'Value>

    /// <summary>
    /// Sets a binding given as a struct pair of a key and a value.
    /// </summary>
    /// <remarks>
    /// A value that the default comparer of the values finds equal to the value of an earlier binding of the key keeps the
    /// earlier binding, its key included (ADR 0001, D14).
    /// </remarks>
    member inline Yield : pair : struct ('Key * 'Value) -> HashMapBuffer<'Key, 'Value>

    /// <summary>
    /// Sets a binding given as a key/value pair.
    /// </summary>
    /// <remarks>
    /// A value that the default comparer of the values finds equal to the value of an earlier binding of the key keeps the
    /// earlier binding, its key included (ADR 0001, D14).
    /// </remarks>
    member inline Yield : pair : KeyValuePair<'Key, 'Value> -> HashMapBuffer<'Key, 'Value>

    /// <summary>
    /// Sets the bindings of a sequence of struct pairs.
    /// </summary>
    /// <exception cref="T:System.ArgumentNullException">Thrown when the sequence is <see langword="null"/> or a default flat list.</exception>
    /// <remarks>
    /// A value that the default comparer of the values finds equal to the value of an earlier binding of the key keeps the
    /// earlier binding, its key included (ADR 0001, D14).
    /// </remarks>
    member inline YieldFrom : pairs : seq<struct ('Key * 'Value)> -> HashMapBuffer<'Key, 'Value>

    /// <summary>
    /// Sets the bindings of an array of struct pairs.
    /// </summary>
    /// <exception cref="T:System.ArgumentNullException">Thrown when the array is <see langword="null"/>.</exception>
    /// <remarks>
    /// A value that the default comparer of the values finds equal to the value of an earlier binding of the key keeps the
    /// earlier binding, its key included (ADR 0001, D14).
    /// </remarks>
    member inline YieldFrom : pairs : struct ('Key * 'Value) array -> HashMapBuffer<'Key, 'Value>

    /// <summary>
    /// Sets the bindings of a sequence of key/value pairs, such as another dictionary.
    /// </summary>
    /// <exception cref="T:System.ArgumentNullException">Thrown when the sequence is <see langword="null"/> or a default flat list.</exception>
    /// <remarks>
    /// A value that the default comparer of the values finds equal to the value of an earlier binding of the key keeps the
    /// earlier binding, its key included (ADR 0001, D14).
    /// </remarks>
    member inline YieldFrom : pairs : seq<KeyValuePair<'Key, 'Value>> -> HashMapBuffer<'Key, 'Value>

    /// <summary>
    /// Sets the bindings of a hash map.
    /// </summary>
    /// <exception cref="T:System.ArgumentNullException">Thrown when the hash map is <see langword="null"/>.</exception>
    /// <remarks>
    /// A value that the default comparer of the values finds equal to the value of an earlier binding of the key keeps the
    /// earlier binding, its key included (ADR 0001, D14).
    /// </remarks>
    member inline YieldFrom : table : HashMap<'Key, 'Value> -> HashMapBuffer<'Key, 'Value>

    /// <summary>Sets the bindings of another expression body.</summary>
    member inline YieldFrom : buffer : HashMapBuffer<'Key, 'Value> -> HashMapBuffer<'Key, 'Value>

    /// <summary>Sets the bindings of two parts of the expression, in order.</summary>
    member inline Combine :
        first : HashMapBuffer<'Key, 'Value> * second : HashMapBuffer<'Key, 'Value> -> HashMapBuffer<'Key, 'Value>

    /// <summary>
    /// Runs the body of the expression each time the bindings are collected.
    /// </summary>
    member inline Delay : [<InlineIfLambda>] delayed : (unit -> HashMapBuffer<'Key, 'Value>) -> HashMapBuffer<'Key, 'Value>

    /// <summary>Sets the bindings of the body for each item of a sequence.</summary>
    /// <exception cref="T:System.ArgumentNullException">Thrown when the sequence is <see langword="null"/> or a default flat list.</exception>
    member inline For :
        source : seq<'T> * [<InlineIfLambda>] body : ('T -> HashMapBuffer<'Key, 'Value>) -> HashMapBuffer<'Key, 'Value>

    /// <summary>Sets the bindings of the body while the guard holds.</summary>
    member inline While :
        [<InlineIfLambda>] guard : (unit -> bool) * body : HashMapBuffer<'Key, 'Value> -> HashMapBuffer<'Key, 'Value>

    /// <summary>Handles the exceptions of the body.</summary>
    member inline TryWith :
        body : HashMapBuffer<'Key, 'Value> * [<InlineIfLambda>] handler : (exn -> HashMapBuffer<'Key, 'Value>) ->
            HashMapBuffer<'Key, 'Value>

    /// <summary>Runs the compensation after the body.</summary>
    member inline TryFinally :
        body : HashMapBuffer<'Key, 'Value> * [<InlineIfLambda>] compensation : (unit -> unit) -> HashMapBuffer<'Key, 'Value>

    /// <summary>Disposes a resource after the body.</summary>
    member inline Using :
        resource : 'Resource * [<InlineIfLambda>] body : ('Resource -> HashMapBuffer<'Key, 'Value>) -> HashMapBuffer<'Key, 'Value>
            when 'Resource :> IDisposable | null

    /// <summary>Collects the bindings of the expression.</summary>
    member Run : buffer : HashMapBuffer<'Key, 'Value> -> HashMap<'Key, 'Value>
