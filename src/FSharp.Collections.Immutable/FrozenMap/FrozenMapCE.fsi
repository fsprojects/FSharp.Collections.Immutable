namespace FSharp.Collections.Immutable

open System
open System.Collections.Generic

/// <summary>
/// The body of a <c>frozenMap { }</c> expression: a function that sets the bindings in a dictionary.
/// </summary>
type FrozenMapBuffer<'Key, 'Value when 'Key : not null> = Dictionary<'Key, 'Value> -> unit

/// <summary>
/// The builder of the <c>frozenMap { }</c> computation expression.
/// </summary>
/// <remarks>
/// <para>
/// A frozen map has no builder, so the expression collects its bindings in a
/// <see cref="T:System.Collections.Generic.Dictionary`2"/> with the key comparer (ADR 0001, D7) and freezes them once, when
/// it runs. It yields a binding as a struct pair, <c>yield struct (key, value)</c>, or as a
/// <see cref="T:System.Collections.Generic.KeyValuePair`2"/>. A later binding of a key replaces the value of an earlier one,
/// as in
/// <see cref="M:FSharp.Collections.Immutable.FrozenMapExtensions.OfSeq``2(System.Collections.Generic.IEnumerable{System.ValueTuple{``0,``1}})"/>,
/// and the key of the earlier binding stays, as the indexer of the dictionary keeps it (ADR 0001, D14).
/// </para>
/// <para>
/// The keys compare with the key comparer that the constructor takes, or with
/// <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/> when it is omitted. The builder is generic in the
/// type of the keys, so that it can hold a comparer of them, and each <c>frozenMap { }</c> expression infers the type of its
/// keys. <c>FrozenMapCE StringComparer.OrdinalIgnoreCase { yield struct ("a", 1) }</c> builds a map whose keys ignore the
/// case.
/// </para>
/// </remarks>
[<Sealed>]
type FrozenMapCE<'Key when 'Key : not null> =

    /// <summary>
    /// Creates the builder of frozen maps whose keys compare with a key comparer.
    /// </summary>
    /// <param name="keyComparer">The equality comparer of the keys of the frozen map.</param>
    /// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="keyComparer"/> is <see langword="null"/>.</exception>
    new : keyComparer : IEqualityComparer<'Key> -> FrozenMapCE<'Key>

    /// <summary>
    /// Creates the builder of frozen maps whose keys compare with <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>.
    /// </summary>
    new : unit -> FrozenMapCE<'Key>

    /// <summary>An expression without bindings.</summary>
    member inline Zero : unit -> FrozenMapBuffer<'Key, 'Value>

    /// <summary>
    /// Sets a binding given as a struct pair of a key and a value.
    /// </summary>
    /// <remarks>
    /// The value replaces the value of an earlier binding of the key, whose key stays (ADR 0001, D14).
    /// </remarks>
    member inline Yield : pair : struct ('Key * 'Value) -> FrozenMapBuffer<'Key, 'Value>

    /// <summary>
    /// Sets a binding given as a key/value pair.
    /// </summary>
    /// <remarks>
    /// The value replaces the value of an earlier binding of the key, whose key stays (ADR 0001, D14).
    /// </remarks>
    member inline Yield : pair : KeyValuePair<'Key, 'Value> -> FrozenMapBuffer<'Key, 'Value>

    /// <summary>
    /// Sets the bindings of a sequence of struct pairs.
    /// </summary>
    /// <exception cref="T:System.ArgumentNullException">Thrown when the sequence is <see langword="null"/> or a default flat list.</exception>
    /// <remarks>
    /// A value replaces the value of an earlier binding of its key, whose key stays (ADR 0001, D14).
    /// </remarks>
    member inline YieldFrom : pairs : seq<struct ('Key * 'Value)> -> FrozenMapBuffer<'Key, 'Value>

    /// <summary>
    /// Sets the bindings of an array of struct pairs.
    /// </summary>
    /// <exception cref="T:System.ArgumentNullException">Thrown when the array is <see langword="null"/>.</exception>
    /// <remarks>
    /// A value replaces the value of an earlier binding of its key, whose key stays (ADR 0001, D14).
    /// </remarks>
    member inline YieldFrom : pairs : struct ('Key * 'Value) array -> FrozenMapBuffer<'Key, 'Value>

    /// <summary>
    /// Sets the bindings of a sequence of key/value pairs, such as another dictionary.
    /// </summary>
    /// <exception cref="T:System.ArgumentNullException">Thrown when the sequence is <see langword="null"/> or a default flat list.</exception>
    /// <remarks>
    /// A value replaces the value of an earlier binding of its key, whose key stays (ADR 0001, D14).
    /// </remarks>
    member inline YieldFrom : pairs : seq<KeyValuePair<'Key, 'Value>> -> FrozenMapBuffer<'Key, 'Value>

    /// <summary>
    /// Sets the bindings of a frozen map.
    /// </summary>
    /// <exception cref="T:System.ArgumentNullException">Thrown when the frozen map is <see langword="null"/>.</exception>
    /// <remarks>
    /// A value replaces the value of an earlier binding of its key, whose key stays (ADR 0001, D14).
    /// </remarks>
    member inline YieldFrom : table : FrozenMap<'Key, 'Value> -> FrozenMapBuffer<'Key, 'Value>

    /// <summary>Sets the bindings of another expression body.</summary>
    member inline YieldFrom : buffer : FrozenMapBuffer<'Key, 'Value> -> FrozenMapBuffer<'Key, 'Value>

    /// <summary>Sets the bindings of two parts of the expression, in order.</summary>
    member inline Combine :
        first : FrozenMapBuffer<'Key, 'Value> * second : FrozenMapBuffer<'Key, 'Value> -> FrozenMapBuffer<'Key, 'Value>

    /// <summary>
    /// Runs the body of the expression each time the bindings are collected.
    /// </summary>
    member inline Delay : [<InlineIfLambda>] delayed : (unit -> FrozenMapBuffer<'Key, 'Value>) -> FrozenMapBuffer<'Key, 'Value>

    /// <summary>Sets the bindings of the body for each item of a sequence.</summary>
    /// <exception cref="T:System.ArgumentNullException">Thrown when the sequence is <see langword="null"/> or a default flat list.</exception>
    member inline For :
        source : seq<'T> * [<InlineIfLambda>] body : ('T -> FrozenMapBuffer<'Key, 'Value>) -> FrozenMapBuffer<'Key, 'Value>

    /// <summary>Sets the bindings of the body while the guard holds.</summary>
    member inline While :
        [<InlineIfLambda>] guard : (unit -> bool) * body : FrozenMapBuffer<'Key, 'Value> -> FrozenMapBuffer<'Key, 'Value>

    /// <summary>Handles the exceptions of the body.</summary>
    member inline TryWith :
        body : FrozenMapBuffer<'Key, 'Value> * [<InlineIfLambda>] handler : (exn -> FrozenMapBuffer<'Key, 'Value>) ->
            FrozenMapBuffer<'Key, 'Value>

    /// <summary>Runs the compensation after the body.</summary>
    member inline TryFinally :
        body : FrozenMapBuffer<'Key, 'Value> * [<InlineIfLambda>] compensation : (unit -> unit) -> FrozenMapBuffer<'Key, 'Value>

    /// <summary>Disposes a resource after the body.</summary>
    member inline Using :
        resource : 'Resource * [<InlineIfLambda>] body : ('Resource -> FrozenMapBuffer<'Key, 'Value>) ->
            FrozenMapBuffer<'Key, 'Value>
            when 'Resource :> IDisposable | null

    /// <summary>Collects the bindings of the expression and freezes them.</summary>
    member Run : buffer : FrozenMapBuffer<'Key, 'Value> -> FrozenMap<'Key, 'Value>
