namespace FSharp.Collections.Immutable

open System.Collections.Frozen
open System.Collections.Generic

type FrozenSetBuffer<'T> = ResizeArray<'T> -> unit

[<Sealed>]
type FrozenSetCE<'T> (comparer : IEqualityComparer<'T>) =

    // The builder checks its comparer when it is created, as CustomComparerFrozenSet does, not when an expression runs
    do checkNotNull (nameof comparer) comparer

    new () = FrozenSetCE<'T>(EqualityComparer<'T>.Default)

    member inline _.Zero () : FrozenSetBuffer<'T> = ignore

    member inline _.Yield (item : 'T) : FrozenSetBuffer<'T> = fun buffer -> buffer.Add item

    // Each input is checked before the buffer is returned, so that the exception names the parameter of the expression and
    // not the parameter of AddRange (collection), and so that a null array is rejected: AddRange does not throw for it. A
    // default flat list given as a sequence is rejected as FlatListCE rejects it (ADR 0001, D6)
    member inline _.YieldFrom (items : seq<'T>) : FrozenSetBuffer<'T> =
        checkNotNullOrDefaultSeq (nameof items) items
        fun buffer -> buffer.AddRange items

    member inline _.YieldFrom (items : 'T array) : FrozenSetBuffer<'T> =
        checkNotNull (nameof items) items
        fun buffer -> buffer.AddRange items

    member inline _.YieldFrom (items : FrozenSet<'T>) : FrozenSetBuffer<'T> =
        checkNotNull (nameof items) items
        fun buffer -> buffer.AddRange items

    member inline _.YieldFrom (buffer : FrozenSetBuffer<'T>) : FrozenSetBuffer<'T> = buffer

    member inline _.Combine (first : FrozenSetBuffer<'T>, second : FrozenSetBuffer<'T>) : FrozenSetBuffer<'T> =
        fun buffer ->
            first buffer
            second buffer

    member inline _.Delay ([<InlineIfLambda>] delayed : unit -> FrozenSetBuffer<'T>) : FrozenSetBuffer<'T> =
        fun buffer -> delayed () buffer

    member inline _.For (source : seq<'U>, [<InlineIfLambda>] body : 'U -> FrozenSetBuffer<'T>) : FrozenSetBuffer<'T> =
        // A null source would throw NullReferenceException and a default flat list InvalidOperationException when the loop
        // enumerates; the expression rejects both as yield! does
        checkNotNullOrDefaultSeq (nameof source) source

        fun buffer ->
            for item in source do
                body item buffer

    member inline _.While ([<InlineIfLambda>] guard : unit -> bool, body : FrozenSetBuffer<'T>) : FrozenSetBuffer<'T> =
        fun buffer ->
            while guard () do
                body buffer

    member inline _.TryWith
        (body : FrozenSetBuffer<'T>, [<InlineIfLambda>] handler : exn -> FrozenSetBuffer<'T>)
        : FrozenSetBuffer<'T>
        =
        fun buffer ->
            try
                body buffer
            with ex ->
                handler ex buffer

    member inline _.TryFinally
        (body : FrozenSetBuffer<'T>, [<InlineIfLambda>] compensation : unit -> unit)
        : FrozenSetBuffer<'T>
        =
        fun buffer ->
            try
                body buffer
            finally
                compensation ()

    member inline _.Using
        (resource : 'Resource, [<InlineIfLambda>] body : 'Resource -> FrozenSetBuffer<'T>)
        : FrozenSetBuffer<'T>
        =
        fun buffer ->
            use resource = resource
            body resource buffer

    member _.Run (buffer : FrozenSetBuffer<'T>) : FrozenSet<'T> =
        let elements = ResizeArray<'T>()
        buffer elements
        elements.ToFrozenSet comparer
