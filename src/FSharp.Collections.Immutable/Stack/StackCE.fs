namespace FSharp.Collections.Immutable

type StackBuffer<'T> = ResizeArray<'T> -> unit

[<Sealed>]
type StackCE () =

    member inline _.Zero () : StackBuffer<'T> = ignore

    member inline _.Yield (item : 'T) : StackBuffer<'T> = fun buffer -> buffer.Add item

    // The checks run when the expression calls the member, as in flatList { }, and name the parameter of the expression:
    // List<'T>.AddRange names a null input "collection" (ADR 0001, D6)

    member inline _.YieldFrom (items : seq<'T>) : StackBuffer<'T> =
        // A default flat list given as a sequence is a null input, as in Stack.ofSeq
        checkNotNullOrDefaultSeq (nameof items) items
        fun buffer -> buffer.AddRange items

    member inline _.YieldFrom (items : 'T array) : StackBuffer<'T> =
        checkNotNull (nameof items) items
        fun buffer -> buffer.AddRange items

    member inline _.YieldFrom (items : Stack<'T>) : StackBuffer<'T> =
        checkNotNull (nameof items) items
        fun buffer -> buffer.AddRange items

    member inline _.YieldFrom (body : StackBuffer<'T>) : StackBuffer<'T> = body

    member inline _.Combine (first : StackBuffer<'T>, second : StackBuffer<'T>) : StackBuffer<'T> =
        fun buffer ->
            first buffer
            second buffer

    member inline _.Delay ([<InlineIfLambda>] delayed : unit -> StackBuffer<'T>) : StackBuffer<'T> =
        fun buffer -> delayed () buffer

    member inline _.For (source : seq<'U>, [<InlineIfLambda>] body : 'U -> StackBuffer<'T>) : StackBuffer<'T> =
        // A default flat list is a sequence that fails when it is enumerated; the expression rejects it as yield! does
        checkNotNullOrDefaultSeq (nameof source) source

        fun buffer ->
            for item in source do
                body item buffer

    member inline _.While ([<InlineIfLambda>] guard : unit -> bool, body : StackBuffer<'T>) : StackBuffer<'T> =
        fun buffer ->
            while guard () do
                body buffer

    member inline _.TryWith (body : StackBuffer<'T>, [<InlineIfLambda>] handler : exn -> StackBuffer<'T>) : StackBuffer<'T> =
        fun buffer ->
            try
                body buffer
            with ex ->
                handler ex buffer

    member inline _.TryFinally (body : StackBuffer<'T>, [<InlineIfLambda>] compensation : unit -> unit) : StackBuffer<'T> =
        fun buffer ->
            try
                body buffer
            finally
                compensation ()

    member inline _.Using (resource : 'Resource, [<InlineIfLambda>] body : 'Resource -> StackBuffer<'T>) : StackBuffer<'T> =
        fun buffer ->
            use resource = resource
            body resource buffer

    member _.Run (body : StackBuffer<'T>) : Stack<'T> =
        let buffer = ResizeArray<'T>()
        body buffer
        StackConstruction.ofArrayUnchecked (buffer.ToArray ())
