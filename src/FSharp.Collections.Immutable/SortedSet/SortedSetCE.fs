namespace FSharp.Collections.Immutable

open System.Collections.Immutable

// System.Collections.Generic is not opened: its SortedSet would shadow the SortedSet abbreviation of this namespace.

type SortedSetBuffer<'T> = SortedSet<'T>.Builder -> unit

[<Sealed>]
type SortedSetCE<'T when 'T : comparison> (comparer : System.Collections.Generic.IComparer<'T>) =

    // The builder checks its comparer when it is created, as CustomComparerSortedSet does, not when an expression runs
    do checkNotNull (nameof comparer) comparer

    new () = SortedSetCE<'T>(DefaultComparer<'T>.Instance)

    member inline _.Zero () : SortedSetBuffer<'T> = ignore

    member inline _.Yield (item : 'T) : SortedSetBuffer<'T> = fun builder -> builder.Add item |> ignore

    // Each input is checked before the buffer is returned, so that the exception names the parameter of the expression and
    // not the parameter of UnionWith (other); a default flat list given as a sequence is rejected as FlatListCE rejects it
    // (ADR 0001, D6)
    member inline _.YieldFrom (items : seq<'T>) : SortedSetBuffer<'T> =
        checkNotNullOrDefaultSeq (nameof items) items
        fun builder -> builder.UnionWith items

    member inline _.YieldFrom (items : 'T array) : SortedSetBuffer<'T> =
        checkNotNull (nameof items) items
        fun builder -> builder.UnionWith items

    member inline _.YieldFrom (items : SortedSet<'T>) : SortedSetBuffer<'T> =
        checkNotNull (nameof items) items
        fun builder -> builder.UnionWith items

    member inline _.YieldFrom (buffer : SortedSetBuffer<'T>) : SortedSetBuffer<'T> = buffer

    member inline _.Combine (first : SortedSetBuffer<'T>, second : SortedSetBuffer<'T>) : SortedSetBuffer<'T> =
        fun builder ->
            first builder
            second builder

    member inline _.Delay ([<InlineIfLambda>] delayed : unit -> SortedSetBuffer<'T>) : SortedSetBuffer<'T> =
        fun builder -> delayed () builder

    member inline _.For (source : seq<'U>, [<InlineIfLambda>] body : 'U -> SortedSetBuffer<'T>) : SortedSetBuffer<'T> =
        // A null source would throw NullReferenceException and a default flat list InvalidOperationException when the loop
        // enumerates; the expression rejects both as yield! does
        checkNotNullOrDefaultSeq (nameof source) source

        fun builder ->
            for item in source do
                body item builder

    member inline _.While ([<InlineIfLambda>] guard : unit -> bool, body : SortedSetBuffer<'T>) : SortedSetBuffer<'T> =
        fun builder ->
            while guard () do
                body builder

    member inline _.TryWith
        (body : SortedSetBuffer<'T>, [<InlineIfLambda>] handler : exn -> SortedSetBuffer<'T>)
        : SortedSetBuffer<'T>
        =
        fun builder ->
            try
                body builder
            with ex ->
                handler ex builder

    member inline _.TryFinally
        (body : SortedSetBuffer<'T>, [<InlineIfLambda>] compensation : unit -> unit)
        : SortedSetBuffer<'T>
        =
        fun builder ->
            try
                body builder
            finally
                compensation ()

    member inline _.Using
        (resource : 'Resource, [<InlineIfLambda>] body : 'Resource -> SortedSetBuffer<'T>)
        : SortedSetBuffer<'T>
        =
        fun builder ->
            use resource = resource
            body resource builder

    member _.Run (buffer : SortedSetBuffer<'T>) : SortedSet<'T> =
        let builder = ImmutableSortedSet.CreateBuilder<'T> comparer
        buffer builder
        builder.ToImmutable ()
