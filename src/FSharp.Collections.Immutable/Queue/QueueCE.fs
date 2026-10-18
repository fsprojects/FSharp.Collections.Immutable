namespace FSharp.Collections.Immutable

type QueueBuffer<'T> = ResizeArray<'T> -> unit

[<Sealed>]
type QueueCE () =

    member inline _.Zero () : QueueBuffer<'T> = ignore

    member inline _.Yield (item : 'T) : QueueBuffer<'T> = fun buffer -> buffer.Add item

    // The checks run before the buffer is touched and name the parameter of yield!: the buffer would name its own parameter, or
    // not check the argument at all (AddRange of a null array converts it to an empty span and adds nothing)
    member inline _.YieldFrom (items : seq<'T>) : QueueBuffer<'T> =
        // A default flat list given as a sequence is a null input, as in Queue.ofSeq (ADR 0001, D6)
        checkNotNullOrDefaultSeq (nameof items) items
        fun buffer -> buffer.AddRange items

    member inline _.YieldFrom (items : 'T array) : QueueBuffer<'T> =
        checkNotNull (nameof items) items
        fun buffer -> buffer.AddRange items

    member inline _.YieldFrom (items : Queue<'T>) : QueueBuffer<'T> =
        checkNotNull (nameof items) items

        fun buffer ->
            for item in items do
                buffer.Add item

    member inline _.YieldFrom (buffer : QueueBuffer<'T>) : QueueBuffer<'T> = buffer

    member inline _.Combine (first : QueueBuffer<'T>, second : QueueBuffer<'T>) : QueueBuffer<'T> =
        fun buffer ->
            first buffer
            second buffer

    member inline _.Delay ([<InlineIfLambda>] delayed : unit -> QueueBuffer<'T>) : QueueBuffer<'T> =
        fun buffer -> delayed () buffer

    member inline _.For (source : seq<'U>, [<InlineIfLambda>] body : 'U -> QueueBuffer<'T>) : QueueBuffer<'T> =
        // A default flat list is a sequence that fails when it is enumerated; the expression rejects it as yield! does
        checkNotNullOrDefaultSeq (nameof source) source

        fun buffer ->
            for item in source do
                body item buffer

    member inline _.While ([<InlineIfLambda>] guard : unit -> bool, body : QueueBuffer<'T>) : QueueBuffer<'T> =
        fun buffer ->
            while guard () do
                body buffer

    member inline _.TryWith (body : QueueBuffer<'T>, [<InlineIfLambda>] handler : exn -> QueueBuffer<'T>) : QueueBuffer<'T> =
        fun buffer ->
            try
                body buffer
            with ex ->
                handler ex buffer

    member inline _.TryFinally (body : QueueBuffer<'T>, [<InlineIfLambda>] compensation : unit -> unit) : QueueBuffer<'T> =
        fun buffer ->
            try
                body buffer
            finally
                compensation ()

    member inline _.Using (resource : 'Resource, [<InlineIfLambda>] body : 'Resource -> QueueBuffer<'T>) : QueueBuffer<'T> =
        fun buffer ->
            use resource = resource
            body resource buffer

    member _.Run (buffer : QueueBuffer<'T>) : Queue<'T> =
        let items = ResizeArray<'T>()
        buffer items
        // Nobody else sees the buffer, so the queue takes its elements without a copy of the buffer
        Queues.ofBuffer items
