namespace FSharp.Collections.Immutable

open System

/// <summary>
/// The body of a <c>stack { }</c> expression: a function that adds the elements to a buffer, in their order.
/// </summary>
type StackBuffer<'T> = ResizeArray<'T> -> unit

/// <summary>
/// The builder of the <c>stack { }</c> computation expression.
/// </summary>
/// <remarks>
/// The expression collects its elements in a <see cref="T:System.Collections.Generic.List`1"/> and pushes them onto an empty
/// stack from the last one to the first one, so that the first element of the expression ends on top and the stack
/// enumerates the elements in the order of the expression (ADR 0001, D7).
/// </remarks>
[<Sealed>]
type StackCE =

    /// <summary>Creates the builder of the expression.</summary>
    new : unit -> StackCE

    /// <summary>An expression without elements.</summary>
    member inline Zero : unit -> StackBuffer<'T>

    /// <summary>Adds an element.</summary>
    member inline Yield : item : 'T -> StackBuffer<'T>

    /// <summary>Adds the elements of a sequence.</summary>
    /// <exception cref="T:System.ArgumentNullException">Thrown when the sequence is <see langword="null"/> or a default flat list.</exception>
    member inline YieldFrom : items : seq<'T> -> StackBuffer<'T>

    /// <summary>Adds the elements of an array.</summary>
    /// <exception cref="T:System.ArgumentNullException">Thrown when the array is <see langword="null"/>.</exception>
    member inline YieldFrom : items : 'T array -> StackBuffer<'T>

    /// <summary>Adds the elements of a stack, from the top to the bottom.</summary>
    /// <exception cref="T:System.ArgumentNullException">Thrown when the stack is <see langword="null"/>.</exception>
    member inline YieldFrom : items : Stack<'T> -> StackBuffer<'T>

    /// <summary>Adds the elements of another expression body.</summary>
    member inline YieldFrom : body : StackBuffer<'T> -> StackBuffer<'T>

    /// <summary>Adds the elements of two parts of the expression, in order.</summary>
    member inline Combine : first : StackBuffer<'T> * second : StackBuffer<'T> -> StackBuffer<'T>

    /// <summary>
    /// Runs the body of the expression each time the elements are collected.
    /// </summary>
    member inline Delay : [<InlineIfLambda>] delayed : (unit -> StackBuffer<'T>) -> StackBuffer<'T>

    /// <summary>Adds the elements of the body for each item of a sequence.</summary>
    /// <exception cref="T:System.ArgumentNullException">Thrown when the sequence is <see langword="null"/> or a default flat list.</exception>
    member inline For : source : seq<'U> * [<InlineIfLambda>] body : ('U -> StackBuffer<'T>) -> StackBuffer<'T>

    /// <summary>Adds the elements of the body while the guard holds.</summary>
    member inline While : [<InlineIfLambda>] guard : (unit -> bool) * body : StackBuffer<'T> -> StackBuffer<'T>

    /// <summary>Handles the exceptions of the body.</summary>
    member inline TryWith : body : StackBuffer<'T> * [<InlineIfLambda>] handler : (exn -> StackBuffer<'T>) -> StackBuffer<'T>

    /// <summary>Runs the compensation after the body.</summary>
    member inline TryFinally : body : StackBuffer<'T> * [<InlineIfLambda>] compensation : (unit -> unit) -> StackBuffer<'T>

    /// <summary>Disposes a resource after the body.</summary>
    member inline Using :
        resource : 'Resource * [<InlineIfLambda>] body : ('Resource -> StackBuffer<'T>) -> StackBuffer<'T>
            when 'Resource :> IDisposable | null

    /// <summary>
    /// Collects the elements of the expression into a stack whose top is the first element.
    /// </summary>
    member Run : body : StackBuffer<'T> -> Stack<'T>
