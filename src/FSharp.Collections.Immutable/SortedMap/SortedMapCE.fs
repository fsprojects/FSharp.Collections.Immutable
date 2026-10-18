namespace FSharp.Collections.Immutable

open System
open System.Collections.Generic
open System.Collections.Immutable

type SortedMapBuffer<'Key, 'Value when 'Key : not null> = SortedMap<'Key, 'Value>.Builder -> unit

[<Sealed>]
type SortedMapCE<'Key when 'Key : not null and 'Key : comparison> (keyComparer : IComparer<'Key>) =

    do checkNotNull (nameof keyComparer) keyComparer

    new () = SortedMapCE<'Key>(DefaultComparer<'Key>.Instance)

    member inline _.Zero () : SortedMapBuffer<'Key, 'Value> = ignore

    // Every binding goes through the indexer of the builder, which keeps the earlier binding of a key for a value that the
    // value comparer finds equal (ADR 0001, D14)
    member inline _.Yield (pair : struct ('Key * 'Value)) : SortedMapBuffer<'Key, 'Value> =
        fun builder ->
            let struct (key, value) = pair
            builder[key] <- value

    member inline _.Yield (pair : KeyValuePair<'Key, 'Value>) : SortedMapBuffer<'Key, 'Value> =
        fun builder -> builder[pair.Key] <- pair.Value

    member inline _.YieldFrom (pairs : seq<struct ('Key * 'Value)>) : SortedMapBuffer<'Key, 'Value> =
        // A default flat list given as a sequence fails when it is enumerated; the expression rejects it, as the functions
        // of the module do (ADR 0001, D6), when yield! runs and not when the buffer does
        checkNotNullOrDefaultSeq (nameof pairs) pairs

        fun builder ->
            for struct (key, value) in pairs do
                builder[key] <- value

    member inline _.YieldFrom (pairs : struct ('Key * 'Value) array) : SortedMapBuffer<'Key, 'Value> =
        // A null array would fail with NullReferenceException when the buffer runs; the expression rejects it by the name of
        // the parameter, when yield! runs (ADR 0001, D6)
        checkNotNull (nameof pairs) pairs

        fun builder ->
            for struct (key, value) in pairs do
                builder[key] <- value

    member inline _.YieldFrom (pairs : seq<KeyValuePair<'Key, 'Value>>) : SortedMapBuffer<'Key, 'Value> =
        checkNotNullOrDefaultSeq (nameof pairs) pairs

        fun builder ->
            for pair in pairs do
                builder[pair.Key] <- pair.Value

    member inline _.YieldFrom (table : SortedMap<'Key, 'Value>) : SortedMapBuffer<'Key, 'Value> =
        checkNotNull (nameof table) table

        fun builder ->
            for pair in table do
                builder[pair.Key] <- pair.Value

    member inline _.YieldFrom (buffer : SortedMapBuffer<'Key, 'Value>) : SortedMapBuffer<'Key, 'Value> = buffer

    member inline _.Combine
        (first : SortedMapBuffer<'Key, 'Value>, second : SortedMapBuffer<'Key, 'Value>)
        : SortedMapBuffer<'Key, 'Value>
        =
        fun builder ->
            first builder
            second builder

    member inline _.Delay ([<InlineIfLambda>] delayed : unit -> SortedMapBuffer<'Key, 'Value>) : SortedMapBuffer<'Key, 'Value> =
        fun builder -> delayed () builder

    member inline _.For
        (source : seq<'T>, [<InlineIfLambda>] body : 'T -> SortedMapBuffer<'Key, 'Value>)
        : SortedMapBuffer<'Key, 'Value>
        =
        // A default flat list is a sequence that fails when it is enumerated; the expression rejects it as yield! does
        checkNotNullOrDefaultSeq (nameof source) source

        fun builder ->
            for item in source do
                body item builder

    member inline _.While
        ([<InlineIfLambda>] guard : unit -> bool, body : SortedMapBuffer<'Key, 'Value>)
        : SortedMapBuffer<'Key, 'Value>
        =
        fun builder ->
            while guard () do
                body builder

    member inline _.TryWith
        (body : SortedMapBuffer<'Key, 'Value>, [<InlineIfLambda>] handler : exn -> SortedMapBuffer<'Key, 'Value>)
        : SortedMapBuffer<'Key, 'Value>
        =
        fun builder ->
            try
                body builder
            with ex ->
                handler ex builder

    member inline _.TryFinally
        (body : SortedMapBuffer<'Key, 'Value>, [<InlineIfLambda>] compensation : unit -> unit)
        : SortedMapBuffer<'Key, 'Value>
        =
        fun builder ->
            try
                body builder
            finally
                compensation ()

    member inline _.Using
        (resource : 'Resource, [<InlineIfLambda>] body : 'Resource -> SortedMapBuffer<'Key, 'Value>)
        : SortedMapBuffer<'Key, 'Value>
        =
        fun builder ->
            use resource = resource
            body resource builder

    member _.Run (buffer : SortedMapBuffer<'Key, 'Value>) : SortedMap<'Key, 'Value> =
        let builder = ImmutableSortedDictionary.CreateBuilder<'Key, 'Value> keyComparer
        buffer builder
        builder.ToImmutable ()
