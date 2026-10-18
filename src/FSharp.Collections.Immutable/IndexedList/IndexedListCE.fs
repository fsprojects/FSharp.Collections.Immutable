namespace FSharp.Collections.Immutable

open System
open System.Collections.Immutable

type IndexedListBuffer<'T> = IndexedList<'T>.Builder -> unit

[<Sealed>]
type IndexedListCE () =

    member inline _.Zero () : IndexedListBuffer<'T> = ignore

    member inline _.Yield (item : 'T) : IndexedListBuffer<'T> = fun builder -> builder.Add item

    member inline _.YieldFrom (items : seq<'T>) : IndexedListBuffer<'T> =
        // A default flat list given as a sequence is a null input, as in IndexedList.ofSeq (ADR 0001, D6)
        checkNotNullOrDefaultSeq (nameof items) items
        fun builder -> builder.AddRange items

    member inline _.YieldFrom (items : 'T array) : IndexedListBuffer<'T> = fun builder -> builder.AddRange items

    member inline _.YieldFrom (items : IndexedList<'T>) : IndexedListBuffer<'T> = fun builder -> builder.AddRange items

    member inline _.YieldFrom (buffer : IndexedListBuffer<'T>) : IndexedListBuffer<'T> = buffer

    member inline _.Combine (first : IndexedListBuffer<'T>, second : IndexedListBuffer<'T>) : IndexedListBuffer<'T> =
        fun builder ->
            first builder
            second builder

    member inline _.Delay ([<InlineIfLambda>] delayed : unit -> IndexedListBuffer<'T>) : IndexedListBuffer<'T> =
        fun builder -> delayed () builder

    member inline _.For (source : seq<'U>, [<InlineIfLambda>] body : 'U -> IndexedListBuffer<'T>) : IndexedListBuffer<'T> =
        // A default flat list is a sequence that fails when it is enumerated; the expression rejects it as yield! does
        checkNotNullOrDefaultSeq (nameof source) source

        fun builder ->
            for item in source do
                body item builder

    member inline _.While ([<InlineIfLambda>] guard : unit -> bool, body : IndexedListBuffer<'T>) : IndexedListBuffer<'T> =
        fun builder ->
            while guard () do
                body builder

    member inline _.TryWith
        (body : IndexedListBuffer<'T>, [<InlineIfLambda>] handler : exn -> IndexedListBuffer<'T>)
        : IndexedListBuffer<'T>
        =
        fun builder ->
            try
                body builder
            with ex ->
                handler ex builder

    member inline _.TryFinally
        (body : IndexedListBuffer<'T>, [<InlineIfLambda>] compensation : unit -> unit)
        : IndexedListBuffer<'T>
        =
        fun builder ->
            try
                body builder
            finally
                compensation ()

    member inline _.Using
        (resource : 'Resource, [<InlineIfLambda>] body : 'Resource -> IndexedListBuffer<'T>)
        : IndexedListBuffer<'T>
        =
        fun builder ->
            use resource = resource
            body resource builder

    member _.Run (buffer : IndexedListBuffer<'T>) : IndexedList<'T> =
        let builder = ImmutableList.CreateBuilder<'T>()
        buffer builder
        builder.ToImmutable ()
