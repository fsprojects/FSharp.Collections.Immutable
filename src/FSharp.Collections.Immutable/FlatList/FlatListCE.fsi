namespace FSharp.Collections.Immutable

open System

/// <summary>
/// The body of a <c>flatList { }</c> expression: a function that adds the elements to a builder.
/// </summary>
type FlatListBuffer<'T> = FlatList<'T>.Builder -> unit

/// <summary>
/// The builder of the <c>flatList { }</c> computation expression.
/// </summary>
/// <remarks>
/// The expression collects its elements in an <see cref="T:System.Collections.Immutable.ImmutableArray`1.Builder"/> and
/// returns them without a copy when the builder is full (ADR 0001, D7).
/// </remarks>
[<Sealed>]
type FlatListCE =

    /// <summary>Creates the builder of the expression.</summary>
    new : unit -> FlatListCE

    /// <summary>An expression without elements.</summary>
    member inline Zero : unit -> FlatListBuffer<'T>

    /// <summary>Adds an element.</summary>
    member inline Yield : item : 'T -> FlatListBuffer<'T>

    /// <summary>Adds the elements of a sequence.</summary>
    /// <exception cref="T:System.ArgumentNullException">Thrown when the sequence is <see langword="null"/> or a default flat list.</exception>
    member inline YieldFrom : items : seq<'T> -> FlatListBuffer<'T>

    /// <summary>Adds the elements of an array.</summary>
    /// <exception cref="T:System.ArgumentNullException">Thrown when the array is <see langword="null"/>.</exception>
    member inline YieldFrom : items : 'T array -> FlatListBuffer<'T>

    /// <summary>Adds the elements of a flat list.</summary>
    /// <exception cref="T:System.ArgumentNullException">Thrown when the flat list is a default flat list.</exception>
    member inline YieldFrom : items : FlatList<'T> -> FlatListBuffer<'T>

    /// <summary>Adds the elements of another expression body.</summary>
    member inline YieldFrom : buffer : FlatListBuffer<'T> -> FlatListBuffer<'T>

    /// <summary>Adds the elements of two parts of the expression, in order.</summary>
    member inline Combine : first : FlatListBuffer<'T> * second : FlatListBuffer<'T> -> FlatListBuffer<'T>

    /// <summary>
    /// Runs the body of the expression each time the elements are collected.
    /// </summary>
    member inline Delay : [<InlineIfLambda>] delayed : (unit -> FlatListBuffer<'T>) -> FlatListBuffer<'T>

    /// <summary>Adds the elements of the body for each item of a sequence.</summary>
    member inline For : source : seq<'U> * [<InlineIfLambda>] body : ('U -> FlatListBuffer<'T>) -> FlatListBuffer<'T>

    /// <summary>Adds the elements of the body while the guard holds.</summary>
    member inline While : [<InlineIfLambda>] guard : (unit -> bool) * body : FlatListBuffer<'T> -> FlatListBuffer<'T>

    /// <summary>Handles the exceptions of the body.</summary>
    member inline TryWith :
        body : FlatListBuffer<'T> * [<InlineIfLambda>] handler : (exn -> FlatListBuffer<'T>) -> FlatListBuffer<'T>

    /// <summary>Runs the compensation after the body.</summary>
    member inline TryFinally : body : FlatListBuffer<'T> * [<InlineIfLambda>] compensation : (unit -> unit) -> FlatListBuffer<'T>

    /// <summary>Disposes a resource after the body.</summary>
    member inline Using :
        resource : 'Resource * [<InlineIfLambda>] body : ('Resource -> FlatListBuffer<'T>) -> FlatListBuffer<'T>
            when 'Resource :> IDisposable | null

    /// <summary>Collects the elements of the expression.</summary>
    member Run : buffer : FlatListBuffer<'T> -> FlatList<'T>
