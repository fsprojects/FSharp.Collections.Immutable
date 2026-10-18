namespace FSharp.Collections.Immutable

open System
open System.Collections.Immutable

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
type FlatListCE () =

    /// <summary>An expression without elements.</summary>
    member inline _.Zero () : FlatListBuffer<'T> = ignore

    /// <summary>Adds an element.</summary>
    member inline _.Yield (item : 'T) : FlatListBuffer<'T> = fun builder -> builder.Add item

    /// <summary>Adds the elements of a sequence.</summary>
    member inline _.YieldFrom (items : seq<'T>) : FlatListBuffer<'T> = fun builder -> builder.AddRange items

    /// <summary>Adds the elements of an array.</summary>
    member inline _.YieldFrom (items : 'T array) : FlatListBuffer<'T> = fun builder -> builder.AddRange items

    /// <summary>Adds the elements of a flat list.</summary>
    member inline _.YieldFrom (items : FlatList<'T>) : FlatListBuffer<'T> = fun builder -> builder.AddRange items

    /// <summary>Adds the elements of another expression body.</summary>
    member inline _.YieldFrom (buffer : FlatListBuffer<'T>) : FlatListBuffer<'T> = buffer

    /// <summary>Adds the elements of two parts of the expression, in order.</summary>
    member inline _.Combine (first : FlatListBuffer<'T>, second : FlatListBuffer<'T>) : FlatListBuffer<'T> =
        fun builder ->
            first builder
            second builder

    /// <summary>
    /// Runs the body of the expression each time the elements are collected.
    /// </summary>
    member inline _.Delay ([<InlineIfLambda>] delayed : unit -> FlatListBuffer<'T>) : FlatListBuffer<'T> =
        fun builder -> delayed () builder

    /// <summary>Adds the elements of the body for each item of a sequence.</summary>
    member inline _.For (source : seq<'U>, [<InlineIfLambda>] body : 'U -> FlatListBuffer<'T>) : FlatListBuffer<'T> =
        fun builder ->
            for item in source do
                body item builder

    /// <summary>Adds the elements of the body while the guard holds.</summary>
    member inline _.While ([<InlineIfLambda>] guard : unit -> bool, body : FlatListBuffer<'T>) : FlatListBuffer<'T> =
        fun builder ->
            while guard () do
                body builder

    /// <summary>Handles the exceptions of the body.</summary>
    member inline _.TryWith
        (body : FlatListBuffer<'T>, [<InlineIfLambda>] handler : exn -> FlatListBuffer<'T>)
        : FlatListBuffer<'T>
        =
        fun builder ->
            try
                body builder
            with ex ->
                handler ex builder

    /// <summary>Runs the compensation after the body.</summary>
    member inline _.TryFinally (body : FlatListBuffer<'T>, [<InlineIfLambda>] compensation : unit -> unit) : FlatListBuffer<'T> =
        fun builder ->
            try
                body builder
            finally
                compensation ()

    /// <summary>Disposes a resource after the body.</summary>
    member inline _.Using (resource : 'Resource, [<InlineIfLambda>] body : 'Resource -> FlatListBuffer<'T>) : FlatListBuffer<'T> =
        fun builder ->
            use resource = resource
            body resource builder

    /// <summary>Collects the elements of the expression.</summary>
    member _.Run (buffer : FlatListBuffer<'T>) : FlatList<'T> =
        let builder = ImmutableArray.CreateBuilder<'T>()
        buffer builder
        builder.DrainToImmutable ()
