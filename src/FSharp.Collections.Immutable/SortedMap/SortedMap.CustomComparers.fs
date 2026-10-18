namespace FSharp.Collections.Immutable

open System
open System.Collections.Generic
open System.Collections.Immutable

// The struct holds the comparers and creates sorted maps with them (ADR 0001, D12). A struct cannot run code in a primary
// constructor, so the constructors that check the comparers are explicit. Every struct also has a default value that no
// constructor checks; its fields are null, and the properties give it the comparers that the BCL takes for null ones,
// which the members use. The struct cannot require F# comparison of the keys, because a custom comparer also serves keys
// without it, so its default value cannot fall back to F# generic comparison as the functions of the module do (D5).
[<Struct; NoComparison; NoEquality>]
type CustomComparersSortedMap<'Key, 'Value when 'Key : not null> =

    val private keyComparer : IComparer<'Key> | null
    val private valueComparer : IEqualityComparer<'Value> | null

    new (keyComparer : IComparer<'Key>, valueComparer : IEqualityComparer<'Value>)
        =
        checkNotNull (nameof keyComparer) keyComparer
        checkNotNull (nameof valueComparer) valueComparer
        { keyComparer = keyComparer; valueComparer = valueComparer }

    new (keyComparer : IComparer<'Key>)
        =
        checkNotNull (nameof keyComparer) keyComparer
        { keyComparer = keyComparer; valueComparer = EqualityComparer<'Value>.Default }

    /// Sets the bindings, split into keys and values, one by one in a builder with the comparers, so that a later binding
    /// of a key replaces an earlier one, as in `Map.ofSeq`, unless the value comparer finds the two values equal: the
    /// indexer of the builder then keeps the earlier binding, its key included, as .NET does (D14). Every shape of the
    /// bindings creates its maps here (D13).
    member inline private this.Build ([<InlineIfLambda>] split : 'Binding -> struct ('Key * 'Value), elements : seq<'Binding>) =
        let builder = this.builder ()
        Dictionaries.setAll split builder elements
        builder.ToImmutable ()

    // ---- Creation ----

    [<CompiledName "Empty">]
    member this.empty : SortedMap<'Key, 'Value> =
        ImmutableSortedDictionary.Create<'Key, 'Value>(this.KeyComparer, this.ValueComparer)

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

    [<CompiledName "OfSeq">]
    member this.ofSeq (elements : seq<KeyValuePair<'Key, 'Value>>) =
        // A default flat list given as a sequence is as good as a null one: it fails when it is enumerated, and the
        // exception would name a parameter of the BCL (ADR 0001, D6)
        checkNotNullOrDefaultSeq (nameof elements) elements

        match elements with
        // The keys of a sorted map stay distinct and in order in its own key comparer, so WithComparers cannot meet a
        // collision and returns the map itself or the map with the value comparer, without a copy; another key comparer
        // can make keys equal, and WithComparers would reject them where a later binding has to win
        | :? ImmutableSortedDictionary<'Key, 'Value> as table when obj.ReferenceEquals (table.KeyComparer, this.KeyComparer) ->
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
    member this.ofSeqBy (projection : 'Value -> 'Key) (source : seq<'Value>) : SortedMap<'Key, 'Value> =
        checkNotNullOrDefaultSeq (nameof source) source

        ImmutableSortedDictionary.ToImmutableSortedDictionary (
            source,
            Func<'Value, 'Key> projection,
            Func<'Value, 'Value> id,
            this.KeyComparer,
            this.ValueComparer
        )

    [<CompiledName "Singleton">]
    member this.singleton (key : 'Key) (value : 'Value) : SortedMap<'Key, 'Value> = this.empty.Add (key, value)

    // ---- Builder ----

    [<CompiledName "Build">]
    member this.build (action : SortedMap<'Key, 'Value>.Builder -> unit) : SortedMap<'Key, 'Value> =
        let builder = this.builder ()
        action builder
        builder.ToImmutable ()

    [<CompiledName "CreateBuilder">]
    member this.builder () : SortedMap<'Key, 'Value>.Builder =
        ImmutableSortedDictionary.CreateBuilder<'Key, 'Value>(this.KeyComparer, this.ValueComparer)

    // ---- Properties ----

    member this.KeyComparer : IComparer<'Key> =
        match this.keyComparer with
        | null -> Comparer<'Key>.Default
        | comparer -> comparer

    member this.ValueComparer : IEqualityComparer<'Value> =
        match this.valueComparer with
        | null -> EqualityComparer<'Value>.Default
        | comparer -> comparer
