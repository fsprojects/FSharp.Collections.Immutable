namespace FSharp.Collections.Immutable

open System

// System.Collections.Generic is not opened: its SortedSet would shadow the SortedSet abbreviation of this namespace.

/// <summary>
/// The body of a <c>sortedSet { }</c> expression: a function that adds the elements to a builder.
/// </summary>
type SortedSetBuffer<'T> = SortedSet<'T>.Builder -> unit

/// <summary>
/// The builder of the <c>sortedSet { }</c> computation expression.
/// </summary>
/// <remarks>
/// <para>
/// The expression collects its elements in an <see cref="T:System.Collections.Immutable.ImmutableSortedSet`1.Builder"/>
/// with the comparer of the builder (ADR 0001, D7). An element equal to an earlier one is not added again, so the first of
/// the equal elements stays. <c>sortedSet { }</c> compares with F# generic comparison,
/// <see cref="M:Microsoft.FSharp.Core.LanguagePrimitives.FastGenericComparer``1"/>; an instance created with a comparer
/// builds sorted sets that order their elements with it.
/// </para>
/// <para>
/// The element type must support F# comparison even when the instance is created with a comparer: the constructor without
/// a comparer needs it, and a constraint holds for every constructor of a class.
/// </para>
/// </remarks>
[<Sealed>]
type SortedSetCE<'T when 'T : comparison> =

    /// <summary>
    /// Creates the builder of sorted sets that order their elements with a comparer.
    /// </summary>
    /// <param name="comparer">The comparer of the elements of the sorted set.</param>
    /// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="comparer"/> is <see langword="null"/>.</exception>
    new : comparer : System.Collections.Generic.IComparer<'T> -> SortedSetCE<'T>

    /// <summary>
    /// Creates the builder of sorted sets that compare with F# generic comparison.
    /// </summary>
    new : unit -> SortedSetCE<'T>

    /// <summary>An expression without elements.</summary>
    member inline Zero : unit -> SortedSetBuffer<'T>

    /// <summary>Adds an element.</summary>
    member inline Yield : item : 'T -> SortedSetBuffer<'T>

    /// <summary>Adds the elements of a sequence.</summary>
    /// <exception cref="T:System.ArgumentNullException">Thrown when the sequence is <see langword="null"/> or a default flat list.</exception>
    member inline YieldFrom : items : seq<'T> -> SortedSetBuffer<'T>

    /// <summary>Adds the elements of an array.</summary>
    /// <exception cref="T:System.ArgumentNullException">Thrown when the array is <see langword="null"/>.</exception>
    member inline YieldFrom : items : 'T array -> SortedSetBuffer<'T>

    /// <summary>Adds the elements of a sorted set.</summary>
    /// <exception cref="T:System.ArgumentNullException">Thrown when the sorted set is <see langword="null"/>.</exception>
    member inline YieldFrom : items : SortedSet<'T> -> SortedSetBuffer<'T>

    /// <summary>Adds the elements of another expression body.</summary>
    member inline YieldFrom : buffer : SortedSetBuffer<'T> -> SortedSetBuffer<'T>

    /// <summary>Adds the elements of two parts of the expression, in order.</summary>
    member inline Combine : first : SortedSetBuffer<'T> * second : SortedSetBuffer<'T> -> SortedSetBuffer<'T>

    /// <summary>
    /// Runs the body of the expression each time the elements are collected.
    /// </summary>
    member inline Delay : [<InlineIfLambda>] delayed : (unit -> SortedSetBuffer<'T>) -> SortedSetBuffer<'T>

    /// <summary>Adds the elements of the body for each item of a sequence.</summary>
    /// <exception cref="T:System.ArgumentNullException">Thrown when the sequence is <see langword="null"/> or a default flat list.</exception>
    member inline For : source : seq<'U> * [<InlineIfLambda>] body : ('U -> SortedSetBuffer<'T>) -> SortedSetBuffer<'T>

    /// <summary>Adds the elements of the body while the guard holds.</summary>
    member inline While : [<InlineIfLambda>] guard : (unit -> bool) * body : SortedSetBuffer<'T> -> SortedSetBuffer<'T>

    /// <summary>Handles the exceptions of the body.</summary>
    member inline TryWith :
        body : SortedSetBuffer<'T> * [<InlineIfLambda>] handler : (exn -> SortedSetBuffer<'T>) -> SortedSetBuffer<'T>

    /// <summary>Runs the compensation after the body.</summary>
    member inline TryFinally :
        body : SortedSetBuffer<'T> * [<InlineIfLambda>] compensation : (unit -> unit) -> SortedSetBuffer<'T>

    /// <summary>Disposes a resource after the body.</summary>
    member inline Using :
        resource : 'Resource * [<InlineIfLambda>] body : ('Resource -> SortedSetBuffer<'T>) -> SortedSetBuffer<'T>
            when 'Resource :> IDisposable | null

    /// <summary>Collects the elements of the expression.</summary>
    member Run : buffer : SortedSetBuffer<'T> -> SortedSet<'T>
