namespace FSharp.Collections.Immutable

open System
open System.Collections.Frozen
open System.Collections.Generic

type FrozenMapBuffer<'Key, 'Value when 'Key : not null> = Dictionary<'Key, 'Value> -> unit

[<Sealed>]
type FrozenMapCE<'Key when 'Key : not null> (keyComparer : IEqualityComparer<'Key>) =

    do checkNotNull (nameof keyComparer) keyComparer

    new () = FrozenMapCE<'Key>(EqualityComparer<'Key>.Default)

    member inline _.Zero () : FrozenMapBuffer<'Key, 'Value> = ignore

    // Every binding goes through the indexer of the dictionary, which replaces the value of an earlier binding of a key and
    // keeps its key, as ToFrozenDictionary does (ADR 0001, D14)
    member inline _.Yield (pair : struct ('Key * 'Value)) : FrozenMapBuffer<'Key, 'Value> =
        fun bindings ->
            let struct (key, value) = pair
            bindings[key] <- value

    member inline _.Yield (pair : KeyValuePair<'Key, 'Value>) : FrozenMapBuffer<'Key, 'Value> =
        fun bindings -> bindings[pair.Key] <- pair.Value

    member inline _.YieldFrom (pairs : seq<struct ('Key * 'Value)>) : FrozenMapBuffer<'Key, 'Value> =
        // A default flat list given as a sequence fails when it is enumerated; the expression rejects it, as the functions
        // of the module do (ADR 0001, D6), when yield! runs and not when the buffer does
        checkNotNullOrDefaultSeq (nameof pairs) pairs

        fun bindings ->
            for struct (key, value) in pairs do
                bindings[key] <- value

    member inline _.YieldFrom (pairs : struct ('Key * 'Value) array) : FrozenMapBuffer<'Key, 'Value> =
        // A null array fails with NullReferenceException when the buffer enumerates it; the expression rejects it when
        // yield! runs, as it does a null sequence (ADR 0001, D6)
        checkNotNull (nameof pairs) pairs

        fun bindings ->
            for struct (key, value) in pairs do
                bindings[key] <- value

    member inline _.YieldFrom (pairs : seq<KeyValuePair<'Key, 'Value>>) : FrozenMapBuffer<'Key, 'Value> =
        checkNotNullOrDefaultSeq (nameof pairs) pairs

        fun bindings ->
            for pair in pairs do
                bindings[pair.Key] <- pair.Value

    member inline _.YieldFrom (table : FrozenMap<'Key, 'Value>) : FrozenMapBuffer<'Key, 'Value> =
        checkNotNull (nameof table) table

        fun bindings ->
            for pair in table do
                bindings[pair.Key] <- pair.Value

    member inline _.YieldFrom (buffer : FrozenMapBuffer<'Key, 'Value>) : FrozenMapBuffer<'Key, 'Value> = buffer

    member inline _.Combine
        (first : FrozenMapBuffer<'Key, 'Value>, second : FrozenMapBuffer<'Key, 'Value>)
        : FrozenMapBuffer<'Key, 'Value>
        =
        fun bindings ->
            first bindings
            second bindings

    member inline _.Delay ([<InlineIfLambda>] delayed : unit -> FrozenMapBuffer<'Key, 'Value>) : FrozenMapBuffer<'Key, 'Value> =
        fun bindings -> delayed () bindings

    member inline _.For
        (source : seq<'T>, [<InlineIfLambda>] body : 'T -> FrozenMapBuffer<'Key, 'Value>)
        : FrozenMapBuffer<'Key, 'Value>
        =
        // A default flat list is a sequence that fails when it is enumerated; the expression rejects it as yield! does
        checkNotNullOrDefaultSeq (nameof source) source

        fun bindings ->
            for item in source do
                body item bindings

    member inline _.While
        ([<InlineIfLambda>] guard : unit -> bool, body : FrozenMapBuffer<'Key, 'Value>)
        : FrozenMapBuffer<'Key, 'Value>
        =
        fun bindings ->
            while guard () do
                body bindings

    member inline _.TryWith
        (body : FrozenMapBuffer<'Key, 'Value>, [<InlineIfLambda>] handler : exn -> FrozenMapBuffer<'Key, 'Value>)
        : FrozenMapBuffer<'Key, 'Value>
        =
        fun bindings ->
            try
                body bindings
            with ex ->
                handler ex bindings

    member inline _.TryFinally
        (body : FrozenMapBuffer<'Key, 'Value>, [<InlineIfLambda>] compensation : unit -> unit)
        : FrozenMapBuffer<'Key, 'Value>
        =
        fun bindings ->
            try
                body bindings
            finally
                compensation ()

    member inline _.Using
        (resource : 'Resource, [<InlineIfLambda>] body : 'Resource -> FrozenMapBuffer<'Key, 'Value>)
        : FrozenMapBuffer<'Key, 'Value>
        =
        fun bindings ->
            use resource = resource
            body resource bindings

    member _.Run (buffer : FrozenMapBuffer<'Key, 'Value>) : FrozenMap<'Key, 'Value> =
        let bindings = Dictionary<'Key, 'Value> keyComparer
        buffer bindings
        // ToFrozenDictionary reads a dictionary whose comparer equals the given one without building another one
        FrozenDictionary.ToFrozenDictionary (bindings, keyComparer)
