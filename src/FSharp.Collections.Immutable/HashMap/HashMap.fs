[<RequireQualifiedAccess>]
module FSharp.Collections.Immutable.HashMap

open System
open System.Collections.Generic
open System.Collections.Immutable

// The functions call the members of ImmutableDictionary where it has them (ADR 0001, D5) and visit the bindings with the
// algorithms that the map modules share (`Dictionaries` in Helper/Dictionaries.fs). The transformations fill a builder
// that has the comparers of the input, so that they keep them (D5, comparers). The functions that create a map from
// bindings are the members of CustomComparersHashMap (D12) and the static members of HashMapExtensions (D13); the module
// has none of their names, so that `HashMap.ofSeq` reaches the overloads of the extensions.

// ---- Creation ----

[<GeneralizableValue>]
[<CompiledName "Empty">]
let empty<'Key, 'Value when 'Key : not null> : HashMap<'Key, 'Value> =
    ImmutableDictionary<'Key, 'Value>.Empty

[<CompiledName "ForComparers">]
let forComparers (keyComparer : IEqualityComparer<'Key>) (valueComparer : IEqualityComparer<'Value>) =
    CustomComparersHashMap<'Key, 'Value>(keyComparer, valueComparer)

[<CompiledName "ForKeyComparer">]
let forKeyComparer (keyComparer : IEqualityComparer<'Key>) = CustomComparersHashMap<'Key, 'Value> keyComparer

// A span has one shape, so there are no extension overloads (D13); the member of the struct with the default comparers
// wraps ImmutableDictionary.CreateRangeWithOverwrite (D12)
[<CompiledName "OfReadOnlySpan">]
let ofReadOnlySpan (span : ReadOnlySpan<KeyValuePair<'Key, 'Value>>) : HashMap<'Key, 'Value> =
    (CustomComparersHashMap<'Key, 'Value> EqualityComparer<'Key>.Default).ofReadOnlySpan span

[<CompiledName "OfSeqBy">]
let ofSeqBy (projection : 'Value -> 'Key) (source : seq<'Value>) : HashMap<'Key, 'Value> =
    checkNotNullOrDefaultSeq (nameof source) source
    ImmutableDictionary.ToImmutableDictionary (source, Func<'Value, 'Key> projection)

// The BCL creates from a read-only span only; the member of the struct converts the span, which F# does not do at a call
// of a function whose type arguments it has not inferred yet
[<CompiledName "OfSpan">]
let ofSpan (span : Span<KeyValuePair<'Key, 'Value>>) : HashMap<'Key, 'Value> =
    (CustomComparersHashMap<'Key, 'Value> EqualityComparer<'Key>.Default).ofSpan span

[<CompiledName "Singleton">]
let singleton (key : 'Key) (value : 'Value) : HashMap<'Key, 'Value> = ImmutableDictionary<'Key, 'Value>.Empty.Add(key, value)

// ---- Builder ----

[<CompiledName "Build">]
let build (action : HashMap<'Key, 'Value>.Builder -> unit) : HashMap<'Key, 'Value> =
    (CustomComparersHashMap<'Key, 'Value> EqualityComparer<'Key>.Default).build action

[<CompiledName "CreateBuilder">]
let builder () : HashMap<'Key, 'Value>.Builder = ImmutableDictionary.CreateBuilder<'Key, 'Value>()

[<CompiledName "OfBuilder">]
let ofBuilder (builder : HashMap<'Key, 'Value>.Builder) =
    checkNotNull (nameof builder) builder
    builder.ToImmutable ()

[<CompiledName "ToBuilder">]
let toBuilder (table : HashMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    table.ToBuilder ()

[<CompiledName "Update">]
let update (action : HashMap<'Key, 'Value>.Builder -> unit) (table : HashMap<'Key, 'Value>) : HashMap<'Key, 'Value> =
    checkNotNull (nameof table) table
    // The builder starts with the bindings and the comparers of the map, and ToImmutable returns the map itself when the
    // action changed nothing
    let builder = table.ToBuilder ()
    action builder
    builder.ToImmutable ()

// ---- Properties ----

[<CompiledName "Count">]
let count (table : HashMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    table.Count

[<CompiledName "IsEmpty">]
let isEmpty (table : HashMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    table.IsEmpty

[<CompiledName "KeyComparer">]
let keyComparer (table : HashMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    table.KeyComparer

[<CompiledName "Keys">]
let keys (table : HashMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    // The explicit implementation of IDictionary is a view whose count and lookup ask the map, as Map.keys is
    (table :> IDictionary<'Key, 'Value>).Keys

[<CompiledName "ValueComparer">]
let valueComparer (table : HashMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    table.ValueComparer

[<CompiledName "Values">]
let values (table : HashMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    (table :> IDictionary<'Key, 'Value>).Values

// ---- Access ----

[<CompiledName "Find">]
let find (key : 'Key) (table : HashMap<'Key, 'Value>) =
    checkNotNull (nameof table) table

    match table.TryGetValue key with
    | true, value -> value
    | false, _ -> Raise.keyNotFound ()

[<CompiledName "TryFind">]
let tryFind (key : 'Key) (table : HashMap<'Key, 'Value>) =
    checkNotNull (nameof table) table

    match table.TryGetValue key with
    | true, value -> ValueSome value
    | false, _ -> ValueNone

[<CompiledName "TryGetKey">]
let tryGetKey (key : 'Key) (table : HashMap<'Key, 'Value>) =
    checkNotNull (nameof table) table

    match table.TryGetKey key with
    | true, actualKey -> ValueSome actualKey
    | false, _ -> ValueNone

// ---- Search ----

[<CompiledName "ContainsBinding">]
let containsBinding (key : 'Key) (value : 'Value) (table : HashMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    // The value comparer of the map decides whether the stored value equals the value, which no other function can ask
    table.Contains (KeyValuePair (key, value))

[<CompiledName "ContainsKey">]
let containsKey (key : 'Key) (table : HashMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    table.ContainsKey key

[<CompiledName "ContainsValue">]
let containsValue (value : 'Value) (table : HashMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    table.ContainsValue value

[<CompiledName "Exists">]
let exists predicate (table : HashMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    Dictionaries.exists predicate table

[<CompiledName "FindKey">]
let findKey predicate (table : HashMap<'Key, 'Value>) =
    checkNotNull (nameof table) table

    // Map.findKey searches with Seq.pick, whose message differs from the one of Map.find
    match Dictionaries.tryFindKey predicate table with
    | ValueSome key -> key
    | ValueNone -> Raise.indexNotFound ()

[<CompiledName "ForAll">]
let forall predicate (table : HashMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    Dictionaries.forall predicate table

[<CompiledName "Pick">]
let pick (chooser : 'Key -> 'Value -> 'U voption) (table : HashMap<'Key, 'Value>) =
    checkNotNull (nameof table) table

    match Dictionaries.tryPick chooser table with
    | ValueSome value -> value
    | ValueNone -> Raise.keyNotFound ()

[<CompiledName "TryFindKey">]
let tryFindKey predicate (table : HashMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    Dictionaries.tryFindKey predicate table

[<CompiledName "TryPick">]
let tryPick (chooser : 'Key -> 'Value -> 'U voption) (table : HashMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    Dictionaries.tryPick chooser table

// ---- Transformation ----

// add is ImmutableDictionary.Add, as it was in 1.0.0: it throws for a key bound to another value and returns the map itself
// for an equal binding. Map.add replaces the value, which setItem does (ADR 0001, D14)
[<CompiledName "Add">]
let add (key : 'Key) (value : 'Value) (table : HashMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    table.Add (key, value)

[<CompiledName "AddRange">]
let addRange (pairs : seq<KeyValuePair<'Key, 'Value>>) (table : HashMap<'Key, 'Value>) =
    checkNotNullOrDefaultSeq (nameof pairs) pairs
    checkNotNull (nameof table) table
    table.AddRange pairs

[<CompiledName "Change">]
let change (key : 'Key) (f : 'Value voption -> 'Value voption) (table : HashMap<'Key, 'Value>) =
    checkNotNull (nameof table) table

    let current =
        match table.TryGetValue key with
        | true, value -> ValueSome value
        | false, _ -> ValueNone

    match f current with
    | ValueSome value -> table.SetItem (key, value)
    | ValueNone -> table.Remove key

[<CompiledName "Choose">]
let choose (chooser : 'Key -> 'Value -> 'U voption) (table : HashMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    // The keys keep their type and comparer; the new values get the default comparer of their type
    let builder = ImmutableDictionary.CreateBuilder<'Key, 'U> table.KeyComparer
    Dictionaries.choose chooser builder table
    builder.ToImmutable ()

[<CompiledName "Clear">]
let clear (table : HashMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    table.Clear ()

[<CompiledName "Filter">]
let filter predicate (table : HashMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    // A builder over the input shares the bindings that stay and returns the input itself when every binding stays
    let builder = table.ToBuilder ()
    Dictionaries.filter predicate builder table
    builder.ToImmutable ()

[<CompiledName "Map">]
let map (mapping : 'Key -> 'Value -> 'U) (table : HashMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    let builder = ImmutableDictionary.CreateBuilder<'Key, 'U> table.KeyComparer
    Dictionaries.map mapping builder table
    builder.ToImmutable ()

[<CompiledName "Remove">]
let remove (key : 'Key) (table : HashMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    table.Remove key

[<CompiledName "RemoveRange">]
let removeRange (keys : seq<'Key>) (table : HashMap<'Key, 'Value>) =
    checkNotNullOrDefaultSeq (nameof keys) keys
    checkNotNull (nameof table) table
    table.RemoveRange keys

[<CompiledName "SetItem">]
let setItem (key : 'Key) (value : 'Value) (table : HashMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    table.SetItem (key, value)

[<CompiledName "SetItems">]
let setItems (pairs : seq<KeyValuePair<'Key, 'Value>>) (table : HashMap<'Key, 'Value>) =
    checkNotNullOrDefaultSeq (nameof pairs) pairs
    checkNotNull (nameof table) table
    table.SetItems pairs

// ---- Folding ----

[<CompiledName "Fold">]
let fold (folder : 'State -> 'Key -> 'Value -> 'State) (state : 'State) (table : HashMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    Dictionaries.fold folder state table

[<CompiledName "FoldBack">]
let foldBack (folder : 'Key -> 'Value -> 'State -> 'State) (table : HashMap<'Key, 'Value>) (state : 'State) =
    checkNotNull (nameof table) table
    Dictionaries.foldBack folder table state

[<CompiledName "Iterate">]
let iter action (table : HashMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    Dictionaries.iter action table

// ---- Partitioning ----

[<CompiledName "Partition">]
let partition predicate (table : HashMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    let accepted = table.ToBuilder ()
    let rejected = table.Clear().ToBuilder()
    Dictionaries.partition predicate accepted rejected table
    struct (accepted.ToImmutable (), rejected.ToImmutable ())

// ---- Conversion ----

[<CompiledName "ToArray">]
let toArray (table : HashMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    Dictionaries.toArray structPair table

[<CompiledName "ToList">]
let toList (table : HashMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    Dictionaries.toList structPair table

[<CompiledName "ToSeq">]
let toSeq (table : HashMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    Dictionaries.toSeq structPair table

// The submodules take the bindings as tuples, convert them to key/value pairs on demand and call the members that the
// functions of the module call, so that every shape has the semantics of the BCL member (D13).
[<RequireQualifiedAccess>]
module Tuple =

    // ---- Transformation ----

    [<CompiledName "AddRange">]
    let addRange (pairs : seq<'Key * 'Value>) (table : HashMap<'Key, 'Value>) =
        checkNotNullOrDefaultSeq (nameof pairs) pairs
        checkNotNull (nameof table) table
        table.AddRange (Dictionaries.toKeyValuePairs Dictionaries.splitTuple pairs)

    [<CompiledName "SetItems">]
    let setItems (pairs : seq<'Key * 'Value>) (table : HashMap<'Key, 'Value>) =
        checkNotNullOrDefaultSeq (nameof pairs) pairs
        checkNotNull (nameof table) table
        table.SetItems (Dictionaries.toKeyValuePairs Dictionaries.splitTuple pairs)

[<RequireQualifiedAccess>]
module ValueTuple =

    // ---- Transformation ----

    [<CompiledName "AddRange">]
    let addRange (pairs : seq<struct ('Key * 'Value)>) (table : HashMap<'Key, 'Value>) =
        checkNotNullOrDefaultSeq (nameof pairs) pairs
        checkNotNull (nameof table) table
        table.AddRange (Dictionaries.toKeyValuePairs id pairs)

    [<CompiledName "SetItems">]
    let setItems (pairs : seq<struct ('Key * 'Value)>) (table : HashMap<'Key, 'Value>) =
        checkNotNullOrDefaultSeq (nameof pairs) pairs
        checkNotNull (nameof table) table
        table.SetItems (Dictionaries.toKeyValuePairs id pairs)
