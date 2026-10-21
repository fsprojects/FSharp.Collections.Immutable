namespace FSharp.Collections.Immutable

open System
open System.Collections.Generic
open System.Collections.Immutable

type HashMapBuffer<'Key, 'Value when 'Key : not null> = HashMap<'Key, 'Value>.Builder -> unit

[<Sealed>]
type HashMapCE<'Key when 'Key : not null> (keyComparer : IEqualityComparer<'Key>) =

    do checkNotNull (nameof keyComparer) keyComparer

    new () = HashMapCE<'Key>(EqualityComparer<'Key>.Default)

    member inline _.Zero () : HashMapBuffer<'Key, 'Value> = ignore

    // Every binding goes through the indexer of the builder, which keeps the earlier binding of a key for a value that the
    // value comparer finds equal (ADR 0001, D14)
    member inline _.Yield (pair : struct ('Key * 'Value)) : HashMapBuffer<'Key, 'Value> =
        fun builder ->
            let struct (key, value) = pair
            builder[key] <- value

    member inline _.Yield (pair : KeyValuePair<'Key, 'Value>) : HashMapBuffer<'Key, 'Value> =
        fun builder -> builder[pair.Key] <- pair.Value

    member inline _.YieldFrom (pairs : seq<struct ('Key * 'Value)>) : HashMapBuffer<'Key, 'Value> =
        // A default flat list given as a sequence fails when it is enumerated; the expression rejects it, as the functions
        // of the module do (ADR 0001, D6), when yield! runs and not when the buffer does
        checkNotNullOrDefaultSeq (nameof pairs) pairs

        fun builder ->
            for struct (key, value) in pairs do
                builder[key] <- value

    member inline _.YieldFrom (pairs : struct ('Key * 'Value) array) : HashMapBuffer<'Key, 'Value> =
        // A null array fails when the buffer runs; the expression rejects it when yield! runs, as it rejects a sequence
        checkNotNull (nameof pairs) pairs

        fun builder ->
            for struct (key, value) in pairs do
                builder[key] <- value

    member inline _.YieldFrom (pairs : seq<KeyValuePair<'Key, 'Value>>) : HashMapBuffer<'Key, 'Value> =
        checkNotNullOrDefaultSeq (nameof pairs) pairs

        fun builder ->
            for pair in pairs do
                builder[pair.Key] <- pair.Value

    member inline _.YieldFrom (table : HashMap<'Key, 'Value>) : HashMapBuffer<'Key, 'Value> =
        // A null hash map fails when the buffer runs; the expression rejects it when yield! runs, as it rejects a sequence
        checkNotNull (nameof table) table

        fun builder ->
            for pair in table do
                builder[pair.Key] <- pair.Value

    member inline _.YieldFrom (buffer : HashMapBuffer<'Key, 'Value>) : HashMapBuffer<'Key, 'Value> = buffer

    member inline _.Combine
        (first : HashMapBuffer<'Key, 'Value>, second : HashMapBuffer<'Key, 'Value>)
        : HashMapBuffer<'Key, 'Value>
        =
        fun builder ->
            first builder
            second builder

    member inline _.Delay ([<InlineIfLambda>] delayed : unit -> HashMapBuffer<'Key, 'Value>) : HashMapBuffer<'Key, 'Value> =
        fun builder -> delayed () builder

    member inline _.For
        (source : seq<'T>, [<InlineIfLambda>] body : 'T -> HashMapBuffer<'Key, 'Value>)
        : HashMapBuffer<'Key, 'Value>
        =
        // A default flat list is a sequence that fails when it is enumerated; the expression rejects it as yield! does
        checkNotNullOrDefaultSeq (nameof source) source

        fun builder ->
            for item in source do
                body item builder

    member inline _.While
        ([<InlineIfLambda>] guard : unit -> bool, body : HashMapBuffer<'Key, 'Value>)
        : HashMapBuffer<'Key, 'Value>
        =
        fun builder ->
            while guard () do
                body builder

    member inline _.TryWith
        (body : HashMapBuffer<'Key, 'Value>, [<InlineIfLambda>] handler : exn -> HashMapBuffer<'Key, 'Value>)
        : HashMapBuffer<'Key, 'Value>
        =
        fun builder ->
            try
                body builder
            with ex ->
                handler ex builder

    member inline _.TryFinally
        (body : HashMapBuffer<'Key, 'Value>, [<InlineIfLambda>] compensation : unit -> unit)
        : HashMapBuffer<'Key, 'Value>
        =
        fun builder ->
            try
                body builder
            finally
                compensation ()

    member inline _.Using
        (resource : 'Resource, [<InlineIfLambda>] body : 'Resource -> HashMapBuffer<'Key, 'Value>)
        : HashMapBuffer<'Key, 'Value>
        =
        fun builder ->
            use resource = resource
            body resource builder

    member _.Run (buffer : HashMapBuffer<'Key, 'Value>) : HashMap<'Key, 'Value> =
        let builder = ImmutableDictionary.CreateBuilder<'Key, 'Value> keyComparer
        buffer builder
        builder.ToImmutable ()
