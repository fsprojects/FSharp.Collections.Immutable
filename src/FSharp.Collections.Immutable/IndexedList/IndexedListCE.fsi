namespace FSharp.Collections.Immutable

open System

/// <summary>
/// The body of an <c>indexedList { }</c> expression: a function that adds the elements to a builder.
/// </summary>
type IndexedListBuffer<'T> = IndexedList<'T>.Builder -> unit

/// <summary>
/// The builder of the <c>indexedList { }</c> computation expression.
/// </summary>
/// <remarks>
/// The expression collects its elements in an <see cref="T:System.Collections.Immutable.ImmutableList`1.Builder"/> and
/// returns its tree without a copy (ADR 0001, D7).
/// </remarks>
[<Sealed>]
type IndexedListCE =

    /// <summary>Creates the builder of the expression.</summary>
    new : unit -> IndexedListCE

    /// <summary>An expression without elements.</summary>
    member inline Zero : unit -> IndexedListBuffer<'T>

    /// <summary>Adds an element.</summary>
    member inline Yield : item : 'T -> IndexedListBuffer<'T>

    /// <summary>Adds the elements of a sequence.</summary>
    /// <exception cref="T:System.ArgumentNullException">Thrown when the sequence is <see langword="null"/> or a default flat list.</exception>
    member inline YieldFrom : items : seq<'T> -> IndexedListBuffer<'T>

    /// <summary>Adds the elements of an array.</summary>
    /// <exception cref="T:System.ArgumentNullException">Thrown when the array is <see langword="null"/>.</exception>
    member inline YieldFrom : items : 'T array -> IndexedListBuffer<'T>

    /// <summary>Adds the elements of an indexed list.</summary>
    /// <exception cref="T:System.ArgumentNullException">Thrown when the indexed list is <see langword="null"/>.</exception>
    member inline YieldFrom : items : IndexedList<'T> -> IndexedListBuffer<'T>

    /// <summary>Adds the elements of another expression body.</summary>
    member inline YieldFrom : buffer : IndexedListBuffer<'T> -> IndexedListBuffer<'T>

    /// <summary>Adds the elements of two parts of the expression, in order.</summary>
    member inline Combine : first : IndexedListBuffer<'T> * second : IndexedListBuffer<'T> -> IndexedListBuffer<'T>

    /// <summary>
    /// Runs the body of the expression each time the elements are collected.
    /// </summary>
    member inline Delay : [<InlineIfLambda>] delayed : (unit -> IndexedListBuffer<'T>) -> IndexedListBuffer<'T>

    /// <summary>Adds the elements of the body for each item of a sequence.</summary>
    /// <exception cref="T:System.ArgumentNullException">Thrown when the sequence is <see langword="null"/> or a default flat list.</exception>
    member inline For : source : seq<'U> * [<InlineIfLambda>] body : ('U -> IndexedListBuffer<'T>) -> IndexedListBuffer<'T>

    /// <summary>Adds the elements of the body while the guard holds.</summary>
    member inline While : [<InlineIfLambda>] guard : (unit -> bool) * body : IndexedListBuffer<'T> -> IndexedListBuffer<'T>

    /// <summary>Handles the exceptions of the body.</summary>
    member inline TryWith :
        body : IndexedListBuffer<'T> * [<InlineIfLambda>] handler : (exn -> IndexedListBuffer<'T>) -> IndexedListBuffer<'T>

    /// <summary>Runs the compensation after the body.</summary>
    member inline TryFinally :
        body : IndexedListBuffer<'T> * [<InlineIfLambda>] compensation : (unit -> unit) -> IndexedListBuffer<'T>

    /// <summary>Disposes a resource after the body.</summary>
    member inline Using :
        resource : 'Resource * [<InlineIfLambda>] body : ('Resource -> IndexedListBuffer<'T>) -> IndexedListBuffer<'T>
            when 'Resource :> IDisposable | null

    /// <summary>Collects the elements of the expression.</summary>
    member Run : buffer : IndexedListBuffer<'T> -> IndexedList<'T>
