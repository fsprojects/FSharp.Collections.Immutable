namespace FSharp.Collections.Immutable

open System
open System.Collections.Generic

/// <summary>
/// The body of a <c>sortedMap { }</c> expression: a function that sets the bindings in a builder.
/// </summary>
type SortedMapBuffer<'Key, 'Value when 'Key : not null> = SortedMap<'Key, 'Value>.Builder -> unit

/// <summary>
/// The builder of the <c>sortedMap { }</c> computation expression.
/// </summary>
/// <remarks>
/// <para>
/// The expression collects its bindings in an
/// <see cref="T:System.Collections.Immutable.ImmutableSortedDictionary`2.Builder"/> (ADR 0001, D7). It yields a binding as
/// a struct pair, <c>yield struct (key, value)</c>, or as a <see cref="T:System.Collections.Generic.KeyValuePair`2"/>. A
/// later binding of a key replaces an earlier one, as in
/// <see cref="M:FSharp.Collections.Immutable.SortedMapExtensions.OfSeq``4(System.Collections.Generic.IEnumerable{System.ValueTuple{``2,``3}})"/>,
/// unless the default comparer of the values finds the two values equal: then the earlier binding stays, its key
/// included, as the indexer of the builder keeps it (ADR 0001, D14).
/// </para>
/// <para>
/// The keys are ordered by the comparer that the constructor takes, or by F# generic comparison,
/// <see cref="M:Microsoft.FSharp.Core.LanguagePrimitives.FastGenericComparer``1"/>, when it is omitted; the values compare
/// with the default equality comparer. The builder is generic in the type of the keys, so that it can hold a comparer of
/// them, and each <c>sortedMap { }</c> expression infers the type of its keys. The type of the keys must support F#
/// comparison, because the constructor without a comparer orders them with it.
/// <c>SortedMapCE StringComparer.OrdinalIgnoreCase { yield struct ("a", 1) }</c> builds a map whose keys ignore the case.
/// </para>
/// </remarks>
[<Sealed>]
type SortedMapCE<'Key when 'Key : not null and 'Key : comparison> =

    /// <summary>
    /// Creates the builder of sorted maps whose keys are ordered by a key comparer.
    /// </summary>
    /// <param name="keyComparer">The comparer that orders the keys of the sorted map.</param>
    /// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="keyComparer"/> is <see langword="null"/>.</exception>
    new : keyComparer : IComparer<'Key> -> SortedMapCE<'Key>

    /// <summary>
    /// Creates the builder of sorted maps whose keys are ordered by F# generic comparison,
    /// <see cref="M:Microsoft.FSharp.Core.LanguagePrimitives.FastGenericComparer``1"/>.
    /// </summary>
    new : unit -> SortedMapCE<'Key>

    /// <summary>An expression without bindings.</summary>
    member inline Zero : unit -> SortedMapBuffer<'Key, 'Value>

    /// <summary>
    /// Sets a binding given as a struct pair of a key and a value.
    /// </summary>
    /// <remarks>
    /// A value that the default comparer of the values finds equal to the value of an earlier binding of the key keeps the
    /// earlier binding, its key included (ADR 0001, D14).
    /// </remarks>
    member inline Yield : pair : struct ('Key * 'Value) -> SortedMapBuffer<'Key, 'Value>

    /// <summary>
    /// Sets a binding given as a key/value pair.
    /// </summary>
    /// <remarks>
    /// A value that the default comparer of the values finds equal to the value of an earlier binding of the key keeps the
    /// earlier binding, its key included (ADR 0001, D14).
    /// </remarks>
    member inline Yield : pair : KeyValuePair<'Key, 'Value> -> SortedMapBuffer<'Key, 'Value>

    /// <summary>
    /// Sets the bindings of a sequence of struct pairs.
    /// </summary>
    /// <exception cref="T:System.ArgumentNullException">Thrown when the sequence is <see langword="null"/> or a default flat list.</exception>
    /// <remarks>
    /// A value that the default comparer of the values finds equal to the value of an earlier binding of the key keeps the
    /// earlier binding, its key included (ADR 0001, D14).
    /// </remarks>
    member inline YieldFrom : pairs : seq<struct ('Key * 'Value)> -> SortedMapBuffer<'Key, 'Value>

    /// <summary>
    /// Sets the bindings of an array of struct pairs.
    /// </summary>
    /// <exception cref="T:System.ArgumentNullException">Thrown when the array is <see langword="null"/>.</exception>
    /// <remarks>
    /// A value that the default comparer of the values finds equal to the value of an earlier binding of the key keeps the
    /// earlier binding, its key included (ADR 0001, D14).
    /// </remarks>
    member inline YieldFrom : pairs : struct ('Key * 'Value) array -> SortedMapBuffer<'Key, 'Value>

    /// <summary>
    /// Sets the bindings of a sequence of key/value pairs, such as another dictionary.
    /// </summary>
    /// <exception cref="T:System.ArgumentNullException">Thrown when the sequence is <see langword="null"/> or a default flat list.</exception>
    /// <remarks>
    /// A value that the default comparer of the values finds equal to the value of an earlier binding of the key keeps the
    /// earlier binding, its key included (ADR 0001, D14).
    /// </remarks>
    member inline YieldFrom : pairs : seq<KeyValuePair<'Key, 'Value>> -> SortedMapBuffer<'Key, 'Value>

    /// <summary>
    /// Sets the bindings of a sorted map.
    /// </summary>
    /// <exception cref="T:System.ArgumentNullException">Thrown when the sorted map is <see langword="null"/>.</exception>
    /// <remarks>
    /// A value that the default comparer of the values finds equal to the value of an earlier binding of the key keeps the
    /// earlier binding, its key included (ADR 0001, D14).
    /// </remarks>
    member inline YieldFrom : table : SortedMap<'Key, 'Value> -> SortedMapBuffer<'Key, 'Value>

    /// <summary>Sets the bindings of another expression body.</summary>
    member inline YieldFrom : buffer : SortedMapBuffer<'Key, 'Value> -> SortedMapBuffer<'Key, 'Value>

    /// <summary>Sets the bindings of two parts of the expression, in order.</summary>
    member inline Combine :
        first : SortedMapBuffer<'Key, 'Value> * second : SortedMapBuffer<'Key, 'Value> -> SortedMapBuffer<'Key, 'Value>

    /// <summary>
    /// Runs the body of the expression each time the bindings are collected.
    /// </summary>
    member inline Delay : [<InlineIfLambda>] delayed : (unit -> SortedMapBuffer<'Key, 'Value>) -> SortedMapBuffer<'Key, 'Value>

    /// <summary>Sets the bindings of the body for each item of a sequence.</summary>
    /// <exception cref="T:System.ArgumentNullException">Thrown when the sequence is <see langword="null"/> or a default flat list.</exception>
    member inline For :
        source : seq<'T> * [<InlineIfLambda>] body : ('T -> SortedMapBuffer<'Key, 'Value>) -> SortedMapBuffer<'Key, 'Value>

    /// <summary>Sets the bindings of the body while the guard holds.</summary>
    member inline While :
        [<InlineIfLambda>] guard : (unit -> bool) * body : SortedMapBuffer<'Key, 'Value> -> SortedMapBuffer<'Key, 'Value>

    /// <summary>Handles the exceptions of the body.</summary>
    member inline TryWith :
        body : SortedMapBuffer<'Key, 'Value> * [<InlineIfLambda>] handler : (exn -> SortedMapBuffer<'Key, 'Value>) ->
            SortedMapBuffer<'Key, 'Value>

    /// <summary>Runs the compensation after the body.</summary>
    member inline TryFinally :
        body : SortedMapBuffer<'Key, 'Value> * [<InlineIfLambda>] compensation : (unit -> unit) -> SortedMapBuffer<'Key, 'Value>

    /// <summary>Disposes a resource after the body.</summary>
    member inline Using :
        resource : 'Resource * [<InlineIfLambda>] body : ('Resource -> SortedMapBuffer<'Key, 'Value>) ->
            SortedMapBuffer<'Key, 'Value>
            when 'Resource :> IDisposable | null

    /// <summary>Collects the bindings of the expression.</summary>
    member Run : buffer : SortedMapBuffer<'Key, 'Value> -> SortedMap<'Key, 'Value>
