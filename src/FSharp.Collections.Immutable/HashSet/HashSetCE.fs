namespace FSharp.Collections.Immutable

open System.Collections.Immutable

// System.Collections.Generic is not opened: its HashSet would shadow the HashSet abbreviation of this namespace.

type HashSetBuffer<'T> = HashSet<'T>.Builder -> unit

[<Sealed>]
type HashSetCE<'T> (comparer : System.Collections.Generic.IEqualityComparer<'T>) =

    // The builder checks its comparer when it is created, as CustomComparerHashSet does, not when an expression runs
    do checkNotNull (nameof comparer) comparer

    new () = HashSetCE<'T>(System.Collections.Generic.EqualityComparer<'T>.Default)

    member inline _.Zero () : HashSetBuffer<'T> = ignore

    member inline _.Yield (item : 'T) : HashSetBuffer<'T> = fun builder -> builder.Add item |> ignore

    // Each input is checked before the buffer is returned, so that the exception names the parameter of the expression and
    // not the parameter of UnionWith (other); a default flat list given as a sequence is rejected as FlatListCE rejects it
    // (ADR 0001, D6)
    member inline _.YieldFrom (items : seq<'T>) : HashSetBuffer<'T> =
        checkNotNullOrDefaultSeq (nameof items) items
        fun builder -> builder.UnionWith items

    member inline _.YieldFrom (items : 'T array) : HashSetBuffer<'T> =
        checkNotNull (nameof items) items
        fun builder -> builder.UnionWith items

    member inline _.YieldFrom (items : HashSet<'T>) : HashSetBuffer<'T> =
        checkNotNull (nameof items) items
        fun builder -> builder.UnionWith items

    member inline _.YieldFrom (buffer : HashSetBuffer<'T>) : HashSetBuffer<'T> = buffer

    member inline _.Combine (first : HashSetBuffer<'T>, second : HashSetBuffer<'T>) : HashSetBuffer<'T> =
        fun builder ->
            first builder
            second builder

    member inline _.Delay ([<InlineIfLambda>] delayed : unit -> HashSetBuffer<'T>) : HashSetBuffer<'T> =
        fun builder -> delayed () builder

    member inline _.For (source : seq<'U>, [<InlineIfLambda>] body : 'U -> HashSetBuffer<'T>) : HashSetBuffer<'T> =
        // A null source would throw NullReferenceException and a default flat list InvalidOperationException when the loop
        // enumerates; the expression rejects both as yield! does
        checkNotNullOrDefaultSeq (nameof source) source

        fun builder ->
            for item in source do
                body item builder

    member inline _.While ([<InlineIfLambda>] guard : unit -> bool, body : HashSetBuffer<'T>) : HashSetBuffer<'T> =
        fun builder ->
            while guard () do
                body builder

    member inline _.TryWith
        (body : HashSetBuffer<'T>, [<InlineIfLambda>] handler : exn -> HashSetBuffer<'T>)
        : HashSetBuffer<'T>
        =
        fun builder ->
            try
                body builder
            with ex ->
                handler ex builder

    member inline _.TryFinally (body : HashSetBuffer<'T>, [<InlineIfLambda>] compensation : unit -> unit) : HashSetBuffer<'T> =
        fun builder ->
            try
                body builder
            finally
                compensation ()

    member inline _.Using (resource : 'Resource, [<InlineIfLambda>] body : 'Resource -> HashSetBuffer<'T>) : HashSetBuffer<'T> =
        fun builder ->
            use resource = resource
            body resource builder

    member _.Run (buffer : HashSetBuffer<'T>) : HashSet<'T> =
        let builder = ImmutableHashSet.CreateBuilder<'T> comparer
        buffer builder
        builder.ToImmutable ()
