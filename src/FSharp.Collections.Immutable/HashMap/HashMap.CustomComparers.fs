namespace FSharp.Collections.Immutable

open System
open System.Collections.Generic
open System.Collections.Immutable

// The struct holds the comparers and creates hash maps with them (ADR 0001, D12). A struct cannot run code in a primary
// constructor, so the constructors that check the comparers are explicit. Every struct also has a default value that no
// constructor checks; its fields are null, and the properties give it the default comparers, which the members use.
[<Struct; NoComparison; NoEquality>]
type CustomComparersHashMap<'Key, 'Value when 'Key : not null> =

    val private keyComparer : IEqualityComparer<'Key> | null
    val private valueComparer : IEqualityComparer<'Value> | null

    new (keyComparer : IEqualityComparer<'Key>, valueComparer : IEqualityComparer<'Value>)
        =
        checkNotNull (nameof keyComparer) keyComparer
        checkNotNull (nameof valueComparer) valueComparer
        { keyComparer = keyComparer; valueComparer = valueComparer }

    new (keyComparer : IEqualityComparer<'Key>)
        =
        checkNotNull (nameof keyComparer) keyComparer
        { keyComparer = keyComparer; valueComparer = EqualityComparer<'Value>.Default }

    /// Sets the bindings, split into keys and values, one by one in a builder with the comparers, so that a later binding
    /// of a key replaces an earlier one, as in `Map.ofSeq`, unless the value comparer finds the two values equal: the
    /// indexer of the builder then keeps the earlier binding, as .NET does (D14). Every shape of the bindings creates its
    /// maps here (D13).
    member inline private this.Build ([<InlineIfLambda>] split : 'Binding -> struct ('Key * 'Value), elements : seq<'Binding>) =
        let builder = this.builder ()
        Dictionaries.setAll split builder elements
        builder.ToImmutable ()

    // ---- Creation ----

    [<CompiledName "Empty">]
    member this.empty : HashMap<'Key, 'Value> =
        ImmutableDictionary.Create<'Key, 'Value>(this.KeyComparer, this.ValueComparer)

    [<CompiledName "OfArray">]
    member this.ofArray (elements : KeyValuePair<'Key, 'Value> array) =
        checkNotNull (nameof elements) elements
        this.Build (Dictionaries.splitKeyValuePair, elements)

    [<CompiledName "OfArray">]
    member this.ofArray (elements : ('Key * 'Value) array) =
        checkNotNull (nameof elements) elements
        this.Build (Dictionaries.splitTuple, elements)

    [<CompiledName "OfArray">]
    member this.ofArray (elements : struct ('Key * 'Value) array) =
        checkNotNull (nameof elements) elements
        this.Build (id, elements)

    [<CompiledName "OfList">]
    member this.ofList (elements : KeyValuePair<'Key, 'Value> list) = this.Build (Dictionaries.splitKeyValuePair, elements)

    [<CompiledName "OfList">]
    member this.ofList (elements : ('Key * 'Value) list) = this.Build (Dictionaries.splitTuple, elements)

    [<CompiledName "OfList">]
    member this.ofList (elements : struct ('Key * 'Value) list) = this.Build (id, elements)

    [<CompiledName "OfReadOnlySpan">]
    member this.ofReadOnlySpan (span : ReadOnlySpan<KeyValuePair<'Key, 'Value>>) : HashMap<'Key, 'Value> =
        // The overload of CreateRangeWithOverwrite that takes a key comparer has no value comparer, so it can only create a
        // map for the default one. Any other value comparer sets the bindings in a builder with the comparers, which gives
        // the same result: the later binding of a key wins, and a value that the comparer finds equal keeps the earlier
        // binding (D14)
        if obj.ReferenceEquals (this.ValueComparer, EqualityComparer<'Value>.Default) then
            ImmutableDictionary.CreateRangeWithOverwrite (this.KeyComparer, span)
        else
            let builder = this.builder ()

            for pair in span do
                builder[pair.Key] <- pair.Value

            builder.ToImmutable ()

    [<CompiledName "OfSeq">]
    member this.ofSeq (elements : seq<KeyValuePair<'Key, 'Value>>) =
        checkNotNullOrDefaultSeq (nameof elements) elements

        match elements with
        // The keys of a hash map stay distinct in its own key comparer, so WithComparers cannot meet a collision and
        // returns the map itself or the map with the value comparer, without a copy; another key comparer can make keys
        // equal, and WithComparers would reject them where a later binding has to win
        | :? ImmutableDictionary<'Key, 'Value> as table when obj.ReferenceEquals (table.KeyComparer, this.KeyComparer) ->
            table.WithComparers (this.KeyComparer, this.ValueComparer)
        | _ -> this.Build (Dictionaries.splitKeyValuePair, elements)

    [<CompiledName "OfSeq">]
    member this.ofSeq (elements : seq<'Key * 'Value>) =
        checkNotNullOrDefaultSeq (nameof elements) elements
        this.Build (Dictionaries.splitTuple, elements)

    [<CompiledName "OfSeq">]
    member this.ofSeq (elements : seq<struct ('Key * 'Value)>) =
        checkNotNullOrDefaultSeq (nameof elements) elements
        this.Build (id, elements)

    [<CompiledName "OfSeqBy">]
    member this.ofSeqBy (projection : 'Value -> 'Key) (source : seq<'Value>) : HashMap<'Key, 'Value> =
        checkNotNullOrDefaultSeq (nameof source) source

        ImmutableDictionary.ToImmutableDictionary (
            source,
            Func<'Value, 'Key> projection,
            Func<'Value, 'Value> id,
            this.KeyComparer,
            this.ValueComparer
        )

    [<CompiledName "OfSpan">]
    member this.ofSpan (span : Span<KeyValuePair<'Key, 'Value>>) : HashMap<'Key, 'Value> =
        // The BCL creates from a read-only span only; F# converts the span at this call, because the types of the member
        // are known
        this.ofReadOnlySpan span

    [<CompiledName "Singleton">]
    member this.singleton (key : 'Key) (value : 'Value) : HashMap<'Key, 'Value> = this.empty.Add (key, value)

    // ---- Builder ----

    [<CompiledName "Build">]
    member this.build (action : HashMap<'Key, 'Value>.Builder -> unit) : HashMap<'Key, 'Value> =
        let builder = this.builder ()
        action builder
        builder.ToImmutable ()

    [<CompiledName "CreateBuilder">]
    member this.builder () : HashMap<'Key, 'Value>.Builder =
        ImmutableDictionary.CreateBuilder<'Key, 'Value>(this.KeyComparer, this.ValueComparer)

    // ---- Properties ----

    member this.KeyComparer : IEqualityComparer<'Key> =
        match this.keyComparer with
        | null -> EqualityComparer<'Key>.Default
        | comparer -> comparer

    member this.ValueComparer : IEqualityComparer<'Value> =
        match this.valueComparer with
        | null -> EqualityComparer<'Value>.Default
        | comparer -> comparer
