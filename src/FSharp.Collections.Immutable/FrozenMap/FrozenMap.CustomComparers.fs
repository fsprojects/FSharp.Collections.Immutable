namespace FSharp.Collections.Immutable

open System
open System.Collections.Frozen
open System.Collections.Generic

// The struct holds the key comparer and creates frozen maps with it (ADR 0001, D12). A struct cannot run code in a primary
// constructor, so the constructor that checks the comparer is explicit. Every struct also has a default value that no
// constructor checks; its field is null, and the property gives it the default comparer, which the members use.
[<Struct; NoComparison; NoEquality>]
type CustomComparersFrozenMap<'Key, 'Value when 'Key : not null> =

    val private keyComparer : IEqualityComparer<'Key> | null

    new (keyComparer : IEqualityComparer<'Key>)
        =
        checkNotNull (nameof keyComparer) keyComparer
        { keyComparer = keyComparer }

    /// Sets the bindings, split into keys and values, one by one in a dictionary with the key comparer and freezes it, so
    /// that a later binding of a key replaces the value of an earlier one, as in `Map.ofSeq`, and the earlier key stays, as
    /// in ToFrozenDictionary (D14); every shape of the bindings creates its maps here (D13). A frozen map has no builder;
    /// ToFrozenDictionary reads a dictionary whose comparer equals the given one directly, without building another one.
    member inline private this.Build ([<InlineIfLambda>] split : 'Binding -> struct ('Key * 'Value), elements : seq<'Binding>) =
        let comparer = this.KeyComparer
        let bindings = Dictionary<'Key, 'Value> comparer
        Dictionaries.setAll split bindings elements
        FrozenDictionary.ToFrozenDictionary (bindings, comparer)

    // ---- Creation ----

    [<CompiledName "Empty">]
    member this.empty : FrozenMap<'Key, 'Value> =
        // An empty source gives FrozenDictionary.Empty for the default comparer and an empty map of the comparer otherwise
        FrozenDictionary.ToFrozenDictionary (Array.Empty<KeyValuePair<'Key, 'Value>>(), this.KeyComparer)

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
    member this.ofReadOnlySpan (span : ReadOnlySpan<KeyValuePair<'Key, 'Value>>) : FrozenMap<'Key, 'Value> =
        // Create reads the span into a dictionary with the comparer, so that a later binding of a key replaces the value of
        // an earlier one and the earlier key stays, as for the other shapes (ADR 0001, D14)
        FrozenDictionary.Create (this.KeyComparer, span)

    [<CompiledName "OfSeq">]
    member this.ofSeq (elements : seq<KeyValuePair<'Key, 'Value>>) =
        // A default flat list given as a sequence fails when it is enumerated, so it is rejected as a null sequence is (D6)
        checkNotNullOrDefaultSeq (nameof elements) elements

        match elements with
        // The keys of a frozen map are distinct in its own comparer, so a map whose comparer equals the comparer of the
        // struct comes back itself, as ToFrozenDictionary returns it (ADR 0001, D14); another comparer can make keys equal,
        // and then the keys merge: the earlier key stays and the later value wins
        | :? FrozenDictionary<'Key, 'Value> as table when table.Comparer.Equals this.KeyComparer -> table
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
    member this.ofSeqBy (projection : 'Value -> 'Key) (source : seq<'Value>) : FrozenMap<'Key, 'Value> =
        checkNotNullOrDefaultSeq (nameof source) source
        FrozenDictionary.ToFrozenDictionary (source, Func<'Value, 'Key> projection, this.KeyComparer)

    // Span.op_Implicit is the conversion that F# applies by itself where the type of the parameter is known, with the warning
    // FS3391; calling it explicitly avoids the warning
    [<CompiledName "OfSpan">]
    member this.ofSpan (span : Span<KeyValuePair<'Key, 'Value>>) : FrozenMap<'Key, 'Value> =
        this.ofReadOnlySpan (Span<KeyValuePair<'Key, 'Value>>.op_Implicit span)

    [<CompiledName "Singleton">]
    member this.singleton (key : 'Key) (value : 'Value) : FrozenMap<'Key, 'Value> =
        FrozenDictionary.ToFrozenDictionary ([| KeyValuePair (key, value) |], this.KeyComparer)

    // ---- Properties ----

    member this.KeyComparer : IEqualityComparer<'Key> =
        match this.keyComparer with
        | null -> EqualityComparer<'Key>.Default
        | comparer -> comparer
