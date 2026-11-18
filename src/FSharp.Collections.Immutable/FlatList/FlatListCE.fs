namespace FSharp.Collections.Immutable

open System
open System.Collections.Immutable

type FlatListBuffer<'T> = FlatList<'T>.Builder -> unit

[<Sealed>]
type FlatListCE () =

    member inline _.Zero () : FlatListBuffer<'T> = ignore

    member inline _.Yield (item : 'T) : FlatListBuffer<'T> = fun builder -> builder.Add item

    member inline _.YieldFrom (items : seq<'T>) : FlatListBuffer<'T> =
        // A default flat list given as a sequence is still a default flat list, as in FlatList.ofSeq (ADR 0001, D6)
        checkNotNullOrDefaultSeq (nameof items) items
        fun builder -> builder.AddRange items

    member inline _.YieldFrom (items : 'T array) : FlatListBuffer<'T> = fun builder -> builder.AddRange items

    member inline _.YieldFrom (items : FlatList<'T>) : FlatListBuffer<'T> =
        // The builder would dereference the missing array of a default flat list; the expression rejects it as the
        // functions of FlatList do (ADR 0001, D6)
        checkNotDefault (nameof items) items
        fun builder -> builder.AddRange items

    member inline _.YieldFrom (buffer : FlatListBuffer<'T>) : FlatListBuffer<'T> = buffer

    member inline _.Combine (first : FlatListBuffer<'T>, second : FlatListBuffer<'T>) : FlatListBuffer<'T> =
        fun builder ->
            first builder
            second builder

    member inline _.Delay ([<InlineIfLambda>] delayed : unit -> FlatListBuffer<'T>) : FlatListBuffer<'T> =
        fun builder -> delayed () builder

    member inline _.For (source : seq<'U>, [<InlineIfLambda>] body : 'U -> FlatListBuffer<'T>) : FlatListBuffer<'T> =
        // A default flat list is a sequence that fails when it is enumerated; the expression rejects it as yield! does
        checkNotNullOrDefaultSeq (nameof source) source

        fun builder ->
            for item in source do
                body item builder

    member inline _.For (source : 'U array, [<InlineIfLambda>] body : 'U -> FlatListBuffer<'T>) : FlatListBuffer<'T> =
        // The check is in the function that the member returns: a statement before that function costs one more closure
        // for each loop, so that a loop inside a loop allocates for each inner loop
        fun builder ->
            checkNotNull (nameof source) source

            for item in source do
                body item builder

    member inline _.For (source : FlatList<'U>, [<InlineIfLambda>] body : 'U -> FlatListBuffer<'T>) : FlatListBuffer<'T> =
        // The overload for sequences would box the flat list and read it through an enumerator object. The check is in the
        // returned function for the reason given at the overload for arrays
        fun builder ->
            for item in unwrapArray (nameof source) source do
                body item builder

    member inline _.While ([<InlineIfLambda>] guard : unit -> bool, body : FlatListBuffer<'T>) : FlatListBuffer<'T> =
        fun builder ->
            while guard () do
                body builder

    member inline _.TryWith
        (body : FlatListBuffer<'T>, [<InlineIfLambda>] handler : exn -> FlatListBuffer<'T>)
        : FlatListBuffer<'T>
        =
        fun builder ->
            try
                body builder
            with ex ->
                handler ex builder

    member inline _.TryFinally (body : FlatListBuffer<'T>, [<InlineIfLambda>] compensation : unit -> unit) : FlatListBuffer<'T> =
        fun builder ->
            try
                body builder
            finally
                compensation ()

    member inline _.Using (resource : 'Resource, [<InlineIfLambda>] body : 'Resource -> FlatListBuffer<'T>) : FlatListBuffer<'T> =
        fun builder ->
            use resource = resource
            body resource builder

    member _.Run (buffer : FlatListBuffer<'T>) : FlatList<'T> =
        let builder = ImmutableArray.CreateBuilder<'T>()
        buffer builder
        builder.DrainToImmutable ()
