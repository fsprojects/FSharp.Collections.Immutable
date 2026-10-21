namespace FSharp.Collections.Immutable

open System
open System.Collections.Frozen
open System.Collections.Generic

/// <summary>
/// The body of a <c>frozenSet { }</c> expression: a function that adds the elements to a buffer.
/// </summary>
type FrozenSetBuffer<'T> = ResizeArray<'T> -> unit

/// <summary>
/// The builder of the <c>frozenSet { }</c> computation expression.
/// </summary>
/// <remarks>
/// A frozen set has no builder, so the expression collects its elements in a
/// <see cref="T:System.Collections.Generic.List`1"/> and freezes them once, with
/// <see cref="M:System.Collections.Frozen.FrozenSet.ToFrozenSet``1(System.Collections.Generic.IEnumerable{``0},System.Collections.Generic.IEqualityComparer{``0})"/>
/// and the equality comparer of the builder (ADR 0001, D7). Of the elements that the comparer considers equal, the frozen
/// set holds the first one. <c>frozenSet { }</c> compares with
/// <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>; an instance created with a comparer builds frozen
/// sets that compare with it.
/// </remarks>
[<Sealed>]
type FrozenSetCE<'T> =

    /// <summary>
    /// Creates the builder of frozen sets that compare with an equality comparer.
    /// </summary>
    /// <param name="comparer">The equality comparer of the elements of the frozen set.</param>
    /// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="comparer"/> is <see langword="null"/>.</exception>
    new : comparer : IEqualityComparer<'T> -> FrozenSetCE<'T>

    /// <summary>
    /// Creates the builder of frozen sets that compare with <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>.
    /// </summary>
    new : unit -> FrozenSetCE<'T>

    /// <summary>An expression without elements.</summary>
    member inline Zero : unit -> FrozenSetBuffer<'T>

    /// <summary>Adds an element.</summary>
    member inline Yield : item : 'T -> FrozenSetBuffer<'T>

    /// <summary>Adds the elements of a sequence.</summary>
    /// <exception cref="T:System.ArgumentNullException">Thrown when the sequence is <see langword="null"/> or a default flat list.</exception>
    member inline YieldFrom : items : seq<'T> -> FrozenSetBuffer<'T>

    /// <summary>Adds the elements of an array.</summary>
    /// <exception cref="T:System.ArgumentNullException">Thrown when the array is <see langword="null"/>.</exception>
    member inline YieldFrom : items : 'T array -> FrozenSetBuffer<'T>

    /// <summary>Adds the elements of a frozen set.</summary>
    /// <exception cref="T:System.ArgumentNullException">Thrown when the frozen set is <see langword="null"/>.</exception>
    member inline YieldFrom : items : FrozenSet<'T> -> FrozenSetBuffer<'T>

    /// <summary>Adds the elements of another expression body.</summary>
    member inline YieldFrom : buffer : FrozenSetBuffer<'T> -> FrozenSetBuffer<'T>

    /// <summary>Adds the elements of two parts of the expression, in order.</summary>
    member inline Combine : first : FrozenSetBuffer<'T> * second : FrozenSetBuffer<'T> -> FrozenSetBuffer<'T>

    /// <summary>
    /// Runs the body of the expression each time the elements are collected.
    /// </summary>
    member inline Delay : [<InlineIfLambda>] delayed : (unit -> FrozenSetBuffer<'T>) -> FrozenSetBuffer<'T>

    /// <summary>Adds the elements of the body for each item of a sequence.</summary>
    /// <exception cref="T:System.ArgumentNullException">Thrown when the sequence is <see langword="null"/> or a default flat list.</exception>
    member inline For : source : seq<'U> * [<InlineIfLambda>] body : ('U -> FrozenSetBuffer<'T>) -> FrozenSetBuffer<'T>

    /// <summary>Adds the elements of the body while the guard holds.</summary>
    member inline While : [<InlineIfLambda>] guard : (unit -> bool) * body : FrozenSetBuffer<'T> -> FrozenSetBuffer<'T>

    /// <summary>Handles the exceptions of the body.</summary>
    member inline TryWith :
        body : FrozenSetBuffer<'T> * [<InlineIfLambda>] handler : (exn -> FrozenSetBuffer<'T>) -> FrozenSetBuffer<'T>

    /// <summary>Runs the compensation after the body.</summary>
    member inline TryFinally :
        body : FrozenSetBuffer<'T> * [<InlineIfLambda>] compensation : (unit -> unit) -> FrozenSetBuffer<'T>

    /// <summary>Disposes a resource after the body.</summary>
    member inline Using :
        resource : 'Resource * [<InlineIfLambda>] body : ('Resource -> FrozenSetBuffer<'T>) -> FrozenSetBuffer<'T>
            when 'Resource :> IDisposable | null

    /// <summary>Collects the elements of the expression and freezes them.</summary>
    member Run : buffer : FrozenSetBuffer<'T> -> FrozenSet<'T>
