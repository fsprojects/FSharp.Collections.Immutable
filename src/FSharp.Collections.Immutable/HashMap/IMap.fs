[<RequireQualifiedAccess>]
module FSharp.Collections.Immutable.IMap

open System.Collections.Generic
open System.Collections.Immutable

// The functions call the members of the interface where it has them (ADR 0001, D5) and visit the bindings with the
// algorithms that the map modules share (`Dictionaries` in Helper/Dictionaries.fs). The interface hides the comparers of
// a map, so the transformations that keep the type of the values build their result with `Clear` and `AddRange` of the
// map itself, which keep its implementation and its comparers; the ones that change the type of the values build a hash
// map with the default comparers (D5, comparers).

// ---- Properties ----

[<CompiledName "Count">]
let count (table : IMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    table.Count

[<CompiledName "IsEmpty">]
let isEmpty (table : IMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    table.Count = 0

[<CompiledName "Keys">]
let keys (table : IMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    table.Keys

[<CompiledName "Values">]
let values (table : IMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    table.Values

// ---- Access ----

[<CompiledName "Find">]
let find (key : 'Key) (table : IMap<'Key, 'Value>) =
    checkNotNull (nameof table) table

    match table.TryGetValue key with
    | true, value -> value
    | false, _ -> Raise.keyNotFound ()

[<CompiledName "TryFind">]
let tryFind (key : 'Key) (table : IMap<'Key, 'Value>) =
    checkNotNull (nameof table) table

    match table.TryGetValue key with
    | true, value -> ValueSome value
    | false, _ -> ValueNone

[<CompiledName "TryGetKey">]
let tryGetKey (key : 'Key) (table : IMap<'Key, 'Value>) =
    checkNotNull (nameof table) table

    match table.TryGetKey key with
    | true, actualKey -> ValueSome actualKey
    | false, _ -> ValueNone

// ---- Search ----

[<CompiledName "ContainsBinding">]
let containsBinding (key : 'Key) (value : 'Value) (table : IMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    // The interface hides the value comparer of the map, so only the member of the interface can decide whether the stored
    // value equals the value
    table.Contains (KeyValuePair (key, value))

[<CompiledName "ContainsKey">]
let containsKey (key : 'Key) (table : IMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    table.ContainsKey key

[<CompiledName "Exists">]
let exists predicate (table : IMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    Dictionaries.exists predicate table

[<CompiledName "FindKey">]
let findKey predicate (table : IMap<'Key, 'Value>) =
    checkNotNull (nameof table) table

    // Map.findKey searches with Seq.pick, whose message differs from the one of Map.find
    match Dictionaries.tryFindKey predicate table with
    | ValueSome key -> key
    | ValueNone -> Raise.indexNotFound ()

[<CompiledName "ForAll">]
let forall predicate (table : IMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    Dictionaries.forall predicate table

[<CompiledName "Pick">]
let pick (chooser : 'Key -> 'Value -> 'U voption) (table : IMap<'Key, 'Value>) =
    checkNotNull (nameof table) table

    match Dictionaries.tryPick chooser table with
    | ValueSome value -> value
    | ValueNone -> Raise.keyNotFound ()

[<CompiledName "TryFindKey">]
let tryFindKey predicate (table : IMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    Dictionaries.tryFindKey predicate table

[<CompiledName "TryPick">]
let tryPick (chooser : 'Key -> 'Value -> 'U voption) (table : IMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    Dictionaries.tryPick chooser table

// ---- Transformation ----

// add is IImmutableDictionary.Add, as HashMap.add is ImmutableDictionary.Add: it throws for a key bound to another value
// and returns the map itself for an equal binding. Map.add replaces the value, which setItem does (ADR 0001, D14)
[<CompiledName "Add">]
let add (key : 'Key) (value : 'Value) (table : IMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    table.Add (key, value)

[<CompiledName "AddRange">]
let addRange (pairs : seq<KeyValuePair<'Key, 'Value>>) (table : IMap<'Key, 'Value>) =
    checkNotNullOrDefaultSeq (nameof pairs) pairs
    checkNotNull (nameof table) table
    table.AddRange pairs

[<CompiledName "Change">]
let change (key : 'Key) (f : 'Value voption -> 'Value voption) (table : IMap<'Key, 'Value>) =
    checkNotNull (nameof table) table

    let current =
        match table.TryGetValue key with
        | true, value -> ValueSome value
        | false, _ -> ValueNone

    match f current with
    | ValueSome value -> table.SetItem (key, value)
    | ValueNone -> table.Remove key

[<CompiledName "Choose">]
let choose (chooser : 'Key -> 'Value -> 'U voption) (table : IMap<'Key, 'Value>) : HashMap<'Key, 'U> =
    checkNotNull (nameof table) table
    // The interface hides the key comparer of the map, so the new map compares with the default comparers
    let builder = ImmutableDictionary.CreateBuilder<'Key, 'U>()
    Dictionaries.choose chooser builder table
    builder.ToImmutable ()

[<CompiledName "Clear">]
let clear (table : IMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    table.Clear ()

[<CompiledName "Filter">]
let filter predicate (table : IMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    let accepted = ResizeArray<KeyValuePair<'Key, 'Value>>()

    for pair in table do
        if predicate pair.Key pair.Value then
            accepted.Add pair

    if accepted.Count = table.Count then
        table
    else
        table.Clear().AddRange accepted

[<CompiledName "Map">]
let map (mapping : 'Key -> 'Value -> 'U) (table : IMap<'Key, 'Value>) : HashMap<'Key, 'U> =
    checkNotNull (nameof table) table
    let builder = ImmutableDictionary.CreateBuilder<'Key, 'U>()
    Dictionaries.map mapping builder table
    builder.ToImmutable ()

[<CompiledName "Remove">]
let remove (key : 'Key) (table : IMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    table.Remove key

[<CompiledName "RemoveRange">]
let removeRange (keys : seq<'Key>) (table : IMap<'Key, 'Value>) =
    checkNotNullOrDefaultSeq (nameof keys) keys
    checkNotNull (nameof table) table
    table.RemoveRange keys

[<CompiledName "SetItem">]
let setItem (key : 'Key) (value : 'Value) (table : IMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    table.SetItem (key, value)

[<CompiledName "SetItems">]
let setItems (pairs : seq<KeyValuePair<'Key, 'Value>>) (table : IMap<'Key, 'Value>) =
    checkNotNullOrDefaultSeq (nameof pairs) pairs
    checkNotNull (nameof table) table
    table.SetItems pairs

// ---- Folding ----

[<CompiledName "Fold">]
let fold (folder : 'State -> 'Key -> 'Value -> 'State) (state : 'State) (table : IMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    Dictionaries.fold folder state table

[<CompiledName "FoldBack">]
let foldBack (folder : 'Key -> 'Value -> 'State -> 'State) (table : IMap<'Key, 'Value>) (state : 'State) =
    checkNotNull (nameof table) table
    Dictionaries.foldBack folder table state

[<CompiledName "Iterate">]
let iter action (table : IMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    Dictionaries.iter action table

// ---- Partitioning ----

[<CompiledName "Partition">]
let partition predicate (table : IMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    let accepted = ResizeArray<KeyValuePair<'Key, 'Value>>()
    let rejected = ResizeArray<KeyValuePair<'Key, 'Value>>()

    for pair in table do
        if predicate pair.Key pair.Value then
            accepted.Add pair
        else
            rejected.Add pair

    let empty = table.Clear ()
    let acceptedTable =
        if rejected.Count = 0 then
            table
        else
            empty.AddRange accepted
    struct (acceptedTable, empty.AddRange rejected)

// ---- Conversion ----

[<CompiledName "ToArray">]
let toArray (table : IMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    Dictionaries.toArray structPair table

[<CompiledName "ToList">]
let toList (table : IMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    Dictionaries.toList structPair table

[<CompiledName "ToSeq">]
let toSeq (table : IMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    Dictionaries.toSeq structPair table

// The submodules take the bindings as tuples, convert them to key/value pairs on demand and call the members that the
// functions of the module call, so that every shape has the semantics of the member of the map (D13).
[<RequireQualifiedAccess>]
module Tuple =

    // ---- Transformation ----

    [<CompiledName "AddRange">]
    let addRange (pairs : seq<'Key * 'Value>) (table : IMap<'Key, 'Value>) =
        checkNotNullOrDefaultSeq (nameof pairs) pairs
        checkNotNull (nameof table) table
        table.AddRange (Dictionaries.toKeyValuePairs Dictionaries.splitTuple pairs)

    [<CompiledName "SetItems">]
    let setItems (pairs : seq<'Key * 'Value>) (table : IMap<'Key, 'Value>) =
        checkNotNullOrDefaultSeq (nameof pairs) pairs
        checkNotNull (nameof table) table
        table.SetItems (Dictionaries.toKeyValuePairs Dictionaries.splitTuple pairs)

[<RequireQualifiedAccess>]
module ValueTuple =

    // ---- Transformation ----

    [<CompiledName "AddRange">]
    let addRange (pairs : seq<struct ('Key * 'Value)>) (table : IMap<'Key, 'Value>) =
        checkNotNullOrDefaultSeq (nameof pairs) pairs
        checkNotNull (nameof table) table
        table.AddRange (Dictionaries.toKeyValuePairs id pairs)

    [<CompiledName "SetItems">]
    let setItems (pairs : seq<struct ('Key * 'Value)>) (table : IMap<'Key, 'Value>) =
        checkNotNullOrDefaultSeq (nameof pairs) pairs
        checkNotNull (nameof table) table
        table.SetItems (Dictionaries.toKeyValuePairs id pairs)
