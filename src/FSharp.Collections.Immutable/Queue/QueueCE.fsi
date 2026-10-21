namespace FSharp.Collections.Immutable

open System

/// <summary>
/// The body of a <c>queue { }</c> expression: a function that adds the elements to a buffer.
/// </summary>
type QueueBuffer<'T> = ResizeArray<'T> -> unit

/// <summary>
/// The builder of the <c>queue { }</c> computation expression.
/// </summary>
/// <remarks>
/// An immutable queue has no builder, so the expression collects its elements in a
/// <see cref="T:System.Collections.Generic.List`1"/> and creates the queue from them when it ends (ADR 0001, D7). The first
/// element of the expression is the first one to leave the queue.
/// </remarks>
[<Sealed>]
type QueueCE =

    /// <summary>Creates the builder of the expression.</summary>
    new : unit -> QueueCE

    /// <summary>An expression without elements.</summary>
    member inline Zero : unit -> QueueBuffer<'T>

    /// <summary>Adds an element.</summary>
    member inline Yield : item : 'T -> QueueBuffer<'T>

    /// <summary>Adds the elements of a sequence.</summary>
    /// <exception cref="T:System.ArgumentNullException">Thrown when the sequence is <see langword="null"/> or a default flat list.</exception>
    member inline YieldFrom : items : seq<'T> -> QueueBuffer<'T>

    /// <summary>Adds the elements of an array.</summary>
    /// <exception cref="T:System.ArgumentNullException">Thrown when the array is <see langword="null"/>.</exception>
    member inline YieldFrom : items : 'T array -> QueueBuffer<'T>

    /// <summary>
    /// Adds the elements of a queue, in the order in which they leave it.
    /// </summary>
    /// <exception cref="T:System.ArgumentNullException">Thrown when the queue is <see langword="null"/>.</exception>
    member inline YieldFrom : items : Queue<'T> -> QueueBuffer<'T>

    /// <summary>Adds the elements of another expression body.</summary>
    member inline YieldFrom : buffer : QueueBuffer<'T> -> QueueBuffer<'T>

    /// <summary>Adds the elements of two parts of the expression, in order.</summary>
    member inline Combine : first : QueueBuffer<'T> * second : QueueBuffer<'T> -> QueueBuffer<'T>

    /// <summary>
    /// Runs the body of the expression each time the elements are collected.
    /// </summary>
    member inline Delay : [<InlineIfLambda>] delayed : (unit -> QueueBuffer<'T>) -> QueueBuffer<'T>

    /// <summary>Adds the elements of the body for each item of a sequence.</summary>
    /// <exception cref="T:System.ArgumentNullException">Thrown when the sequence is <see langword="null"/> or a default flat list.</exception>
    member inline For : source : seq<'U> * [<InlineIfLambda>] body : ('U -> QueueBuffer<'T>) -> QueueBuffer<'T>

    /// <summary>Adds the elements of the body while the guard holds.</summary>
    member inline While : [<InlineIfLambda>] guard : (unit -> bool) * body : QueueBuffer<'T> -> QueueBuffer<'T>

    /// <summary>Handles the exceptions of the body.</summary>
    member inline TryWith : body : QueueBuffer<'T> * [<InlineIfLambda>] handler : (exn -> QueueBuffer<'T>) -> QueueBuffer<'T>

    /// <summary>Runs the compensation after the body.</summary>
    member inline TryFinally : body : QueueBuffer<'T> * [<InlineIfLambda>] compensation : (unit -> unit) -> QueueBuffer<'T>

    /// <summary>Disposes a resource after the body.</summary>
    member inline Using :
        resource : 'Resource * [<InlineIfLambda>] body : ('Resource -> QueueBuffer<'T>) -> QueueBuffer<'T>
            when 'Resource :> IDisposable | null

    /// <summary>Collects the elements of the expression.</summary>
    member Run : buffer : QueueBuffer<'T> -> Queue<'T>
