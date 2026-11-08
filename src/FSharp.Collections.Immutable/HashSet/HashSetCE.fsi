namespace FSharp.Collections.Immutable

open System

// System.Collections.Generic is not opened: its HashSet would shadow the HashSet abbreviation of this namespace.

/// <summary>
/// The body of a <c>hashSet { }</c> expression: a function that adds the elements to a builder.
/// </summary>
type HashSetBuffer<'T> = HashSet<'T>.Builder -> unit

/// <summary>
/// The builder of the <c>hashSet { }</c> computation expression.
/// </summary>
/// <remarks>
/// The expression collects its elements in an <see cref="T:System.Collections.Immutable.ImmutableHashSet`1.Builder"/> with
/// the equality comparer of the builder (ADR 0001, D7). An element equal to an earlier one is not added again.
/// <c>hashSet { }</c> compares with <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>; an instance created
/// with a comparer builds hash sets that compare with it.
/// </remarks>
[<Sealed>]
type HashSetCE<'T> =

    /// <summary>
    /// Creates the builder of hash sets that compare with an equality comparer.
    /// </summary>
    /// <param name="comparer">The equality comparer of the elements of the hash set.</param>
    /// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="comparer"/> is <see langword="null"/>.</exception>
    new : comparer : System.Collections.Generic.IEqualityComparer<'T> -> HashSetCE<'T>

    /// <summary>
    /// Creates the builder of hash sets that compare with <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>.
    /// </summary>
    new : unit -> HashSetCE<'T>

    /// <summary>An expression without elements.</summary>
    member inline Zero : unit -> HashSetBuffer<'T>

    /// <summary>Adds an element.</summary>
    member inline Yield : item : 'T -> HashSetBuffer<'T>

    /// <summary>Adds the elements of a sequence.</summary>
    /// <exception cref="T:System.ArgumentNullException">Thrown when the sequence is <see langword="null"/> or a default flat list.</exception>
    member inline YieldFrom : items : seq<'T> -> HashSetBuffer<'T>

    /// <summary>Adds the elements of an array.</summary>
    /// <exception cref="T:System.ArgumentNullException">Thrown when the array is <see langword="null"/>.</exception>
    member inline YieldFrom : items : 'T array -> HashSetBuffer<'T>

    /// <summary>Adds the elements of a hash set.</summary>
    /// <exception cref="T:System.ArgumentNullException">Thrown when the hash set is <see langword="null"/>.</exception>
    member inline YieldFrom : items : HashSet<'T> -> HashSetBuffer<'T>

    /// <summary>Adds the elements of another expression body.</summary>
    member inline YieldFrom : buffer : HashSetBuffer<'T> -> HashSetBuffer<'T>

    /// <summary>Adds the elements of two parts of the expression, in order.</summary>
    member inline Combine : first : HashSetBuffer<'T> * second : HashSetBuffer<'T> -> HashSetBuffer<'T>

    /// <summary>
    /// Runs the body of the expression each time the elements are collected.
    /// </summary>
    member inline Delay : [<InlineIfLambda>] delayed : (unit -> HashSetBuffer<'T>) -> HashSetBuffer<'T>

    /// <summary>Adds the elements of the body for each item of a sequence.</summary>
    /// <exception cref="T:System.ArgumentNullException">Thrown when the sequence is <see langword="null"/> or a default flat list.</exception>
    member inline For : source : seq<'U> * [<InlineIfLambda>] body : ('U -> HashSetBuffer<'T>) -> HashSetBuffer<'T>

    /// <summary>Adds the elements of the body while the guard holds.</summary>
    member inline While : [<InlineIfLambda>] guard : (unit -> bool) * body : HashSetBuffer<'T> -> HashSetBuffer<'T>

    /// <summary>Handles the exceptions of the body.</summary>
    member inline TryWith :
        body : HashSetBuffer<'T> * [<InlineIfLambda>] handler : (exn -> HashSetBuffer<'T>) -> HashSetBuffer<'T>

    /// <summary>Runs the compensation after the body.</summary>
    member inline TryFinally : body : HashSetBuffer<'T> * [<InlineIfLambda>] compensation : (unit -> unit) -> HashSetBuffer<'T>

    /// <summary>Disposes a resource after the body.</summary>
    member inline Using :
        resource : 'Resource * [<InlineIfLambda>] body : ('Resource -> HashSetBuffer<'T>) -> HashSetBuffer<'T>
            when 'Resource :> IDisposable | null

    /// <summary>Collects the elements of the expression.</summary>
    member Run : buffer : HashSetBuffer<'T> -> HashSet<'T>
