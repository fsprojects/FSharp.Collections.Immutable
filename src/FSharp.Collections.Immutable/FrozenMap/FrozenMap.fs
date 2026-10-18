[<RequireQualifiedAccess>]
module FSharp.Collections.Immutable.FrozenMap

open System
open System.Collections.Frozen
open System.Collections.Generic

// The functions call the members of FrozenDictionary where it has them (ADR 0001, D5) and visit the bindings with the
// algorithms that the map modules share (`Dictionaries` in Helper.fs). A frozen map has no builder, so the transformations
// collect the bindings in a Dictionary with the key comparer of the input and freeze it once. The functions that create a
// map from bindings are the members of CustomComparersFrozenMap (D12) and the static members of FrozenMapExtensions (D13);
// the module has none of their names, so that `FrozenMap.ofSeq` reaches the overloads of the extensions.

/// Freezes the bindings of a dictionary. ToFrozenDictionary reads a dictionary whose comparer equals the given one
/// directly, without building another one, and the frozen map keeps that comparer.
let inline private freeze (comparer : IEqualityComparer<'Key>) (bindings : Dictionary<'Key, 'Value>) =
    FrozenDictionary.ToFrozenDictionary (bindings, comparer)

// ---- Creation ----

[<GeneralizableValue>]
[<CompiledName "Empty">]
let empty<'Key, 'Value when 'Key : not null> : FrozenMap<'Key, 'Value> =
    FrozenDictionary<'Key, 'Value>.Empty

[<CompiledName "ForKeyComparer">]
let forKeyComparer (keyComparer : IEqualityComparer<'Key>) = CustomComparersFrozenMap<'Key, 'Value> keyComparer

// A span has one shape, so there are no extension overloads (D13); the member of the struct with the default comparer wraps
// FrozenDictionary.Create (D12)
[<CompiledName "OfReadOnlySpan">]
let ofReadOnlySpan (span : ReadOnlySpan<KeyValuePair<'Key, 'Value>>) : FrozenMap<'Key, 'Value> =
    (CustomComparersFrozenMap<'Key, 'Value> EqualityComparer<'Key>.Default).ofReadOnlySpan span

[<CompiledName "OfSeqBy">]
let ofSeqBy (projection : 'Value -> 'Key) (source : seq<'Value>) : FrozenMap<'Key, 'Value> =
    checkNotNullOrDefaultSeq (nameof source) source
    FrozenDictionary.ToFrozenDictionary (source, Func<'Value, 'Key> projection)

[<CompiledName "OfSpan">]
let ofSpan (span : Span<KeyValuePair<'Key, 'Value>>) : FrozenMap<'Key, 'Value> =
    (CustomComparersFrozenMap<'Key, 'Value> EqualityComparer<'Key>.Default).ofSpan span

[<CompiledName "Singleton">]
let singleton (key : 'Key) (value : 'Value) : FrozenMap<'Key, 'Value> =
    FrozenDictionary.ToFrozenDictionary [| KeyValuePair (key, value) |]

// ---- Properties ----

[<CompiledName "Count">]
let count (table : FrozenMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    table.Count

[<CompiledName "IsEmpty">]
let isEmpty (table : FrozenMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    table.Count = 0

[<CompiledName "KeyComparer">]
let keyComparer (table : FrozenMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    table.Comparer

[<CompiledName "Keys">]
let keys (table : FrozenMap<'Key, 'Value>) : FlatList<'Key> =
    checkNotNull (nameof table) table
    // An immutable array over the keys that the map holds, in the order of its enumeration
    table.Keys

[<CompiledName "Values">]
let values (table : FrozenMap<'Key, 'Value>) : FlatList<'Value> =
    checkNotNull (nameof table) table
    table.Values

// ---- Access ----

[<CompiledName "Find">]
let find (key : 'Key) (table : FrozenMap<'Key, 'Value>) =
    checkNotNull (nameof table) table

    // The indexer of a frozen dictionary names the key in its message, which Map.find does not
    match table.TryGetValue key with
    | true, value -> value
    | false, _ -> Raise.keyNotFound ()

[<CompiledName "TryFind">]
let tryFind (key : 'Key) (table : FrozenMap<'Key, 'Value>) =
    checkNotNull (nameof table) table

    match table.TryGetValue key with
    | true, value -> ValueSome value
    | false, _ -> ValueNone

// ---- Search ----

[<CompiledName "ContainsBinding">]
let containsBinding (key : 'Key) (value : 'Value) (table : FrozenMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    // The member is an explicit implementation of ICollection<KeyValuePair>. It compares the value with
    // EqualityComparer.Default, as containsValue does, which tryFind followed by (=) does not reproduce for every type
    (table :> ICollection<KeyValuePair<'Key, 'Value>>).Contains(KeyValuePair (key, value))

[<CompiledName "ContainsKey">]
let containsKey (key : 'Key) (table : FrozenMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    table.ContainsKey key

[<CompiledName "ContainsValue">]
let containsValue (value : 'Value) (table : FrozenMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    // ImmutableArray.Contains compares with EqualityComparer.Default, as FrozenDictionary does for its values
    table.Values.Contains value

[<CompiledName "Exists">]
let exists predicate (table : FrozenMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    Dictionaries.exists predicate table

[<CompiledName "FindKey">]
let findKey predicate (table : FrozenMap<'Key, 'Value>) =
    checkNotNull (nameof table) table

    // Map.findKey searches with Seq.pick, whose message differs from the one of Map.find
    match Dictionaries.tryFindKey predicate table with
    | ValueSome key -> key
    | ValueNone -> Raise.indexNotFound ()

[<CompiledName "ForAll">]
let forall predicate (table : FrozenMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    Dictionaries.forall predicate table

[<CompiledName "Pick">]
let pick (chooser : 'Key -> 'Value -> 'U voption) (table : FrozenMap<'Key, 'Value>) =
    checkNotNull (nameof table) table

    match Dictionaries.tryPick chooser table with
    | ValueSome value -> value
    | ValueNone -> Raise.keyNotFound ()

[<CompiledName "TryFindKey">]
let tryFindKey predicate (table : FrozenMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    Dictionaries.tryFindKey predicate table

[<CompiledName "TryPick">]
let tryPick (chooser : 'Key -> 'Value -> 'U voption) (table : FrozenMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    Dictionaries.tryPick chooser table

// ---- Transformation ----

// The keys of the input are distinct in its comparer, so the transformations size the dictionary for every binding

[<CompiledName "Choose">]
let choose (chooser : 'Key -> 'Value -> 'U voption) (table : FrozenMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    let bindings = Dictionary<'Key, 'U>(table.Count, table.Comparer)
    Dictionaries.choose chooser bindings table
    freeze table.Comparer bindings

[<CompiledName "Filter">]
let filter predicate (table : FrozenMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    let bindings = Dictionary<'Key, 'Value>(table.Count, table.Comparer)

    Dictionaries.choose (fun key value -> if predicate key value then ValueSome value else ValueNone) bindings table

    // Freezing is the expensive part, and the input already holds every binding
    if bindings.Count = table.Count then
        table
    else
        freeze table.Comparer bindings

[<CompiledName "Map">]
let map (mapping : 'Key -> 'Value -> 'U) (table : FrozenMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    let bindings = Dictionary<'Key, 'U>(table.Count, table.Comparer)
    Dictionaries.map mapping bindings table
    freeze table.Comparer bindings

// ---- Folding ----

[<CompiledName "Fold">]
let fold (folder : 'State -> 'Key -> 'Value -> 'State) (state : 'State) (table : FrozenMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    Dictionaries.fold folder state table

[<CompiledName "FoldBack">]
let foldBack (folder : 'Key -> 'Value -> 'State -> 'State) (table : FrozenMap<'Key, 'Value>) (state : 'State) =
    checkNotNull (nameof table) table
    // The keys and the values are arrays in the order of enumeration, so they are read backwards without a copy
    let keys = table.Keys
    let values = table.Values
    let mutable state = state

    for index = keys.Length - 1 downto 0 do
        state <- folder keys[index] values[index] state

    state

[<CompiledName "Iterate">]
let iter action (table : FrozenMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    Dictionaries.iter action table

// ---- Partitioning ----

[<CompiledName "Partition">]
let partition predicate (table : FrozenMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    // Without a builder over the input, the bindings are split into two new dictionaries in one pass
    let accepted = Dictionary<'Key, 'Value> table.Comparer
    let rejected = Dictionary<'Key, 'Value> table.Comparer

    for pair in table do
        if predicate pair.Key pair.Value then
            accepted.Add (pair.Key, pair.Value)
        else
            rejected.Add (pair.Key, pair.Value)

    // A part with every binding is the input itself, which saves freezing them again
    let freezePart (part : Dictionary<'Key, 'Value>) =
        if part.Count = table.Count then
            table
        else
            freeze table.Comparer part

    struct (freezePart accepted, freezePart rejected)

// ---- Conversion ----

[<CompiledName "CopyTo">]
let copyTo (destination : KeyValuePair<'Key, 'Value> array) (destinationIndex : int) (table : FrozenMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    // The member checks the destination and the index, and throws for them as it does for its own arguments
    table.CopyTo (destination, destinationIndex)

[<CompiledName "ToArray">]
let toArray (table : FrozenMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    Dictionaries.toArray structPair table

[<CompiledName "ToList">]
let toList (table : FrozenMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    Dictionaries.toList structPair table

[<CompiledName "ToSeq">]
let toSeq (table : FrozenMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    Dictionaries.toSeq structPair table

#if NET10_0_OR_GREATER
// ---- Type-specific ----

// The type of the alternate keys comes first, so that `alternateLookup<int, _, _>` names it alone.
// An F# type parameter cannot declare `allows ref struct`, so these functions cannot take a span alternate (ADR 0001, D11).

[<CompiledName "AlternateLookup">]
let alternateLookup<'Alternate, 'Key, 'Value when 'Key : not null and 'Alternate : not null>
    (table : FrozenMap<'Key, 'Value>)
    : FrozenMap<'Key, 'Value>.AlternateLookup<'Alternate> =
    checkNotNull (nameof table) table
    table.GetAlternateLookup<'Alternate>()

[<CompiledName "TryAlternateLookup">]
let tryAlternateLookup<'Alternate, 'Key, 'Value when 'Key : not null and 'Alternate : not null>
    (table : FrozenMap<'Key, 'Value>)
    : FrozenMap<'Key, 'Value>.AlternateLookup<'Alternate> voption =
    checkNotNull (nameof table) table

    match table.TryGetAlternateLookup<'Alternate>() with
    | true, lookup -> ValueSome lookup
    | false, _ -> ValueNone
#endif
