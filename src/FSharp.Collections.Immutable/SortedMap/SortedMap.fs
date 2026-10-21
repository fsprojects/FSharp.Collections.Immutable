[<RequireQualifiedAccess>]
module FSharp.Collections.Immutable.SortedMap

open System.Collections.Generic
open System.Collections.Immutable
open System.Linq

// The functions call the members of ImmutableSortedDictionary where it has them (ADR 0001, D5) and visit the bindings with
// the algorithms that the map modules share (`Dictionaries` in Helper.fs). The transformations fill a builder that has the
// comparers of the input, so that they keep them (D5, comparers). The functions that create a map from bindings are the
// members of CustomComparersSortedMap (D12) and the static members of SortedMapExtensions (D13); the module has none of
// their names, so that `SortedMap.ofSeq` reaches the overloads of the extensions.

/// The struct with the default comparers: F# generic comparison of the keys, one cached comparer per type (D5), and the
/// default equality comparer of the values. The creation functions are its members, so that each has one implementation
/// (D12).
let private withDefaultComparers<'Key, 'Value when 'Key : not null and 'Key : comparison> () =
    CustomComparersSortedMap<'Key, 'Value> DefaultComparer<'Key>.Instance

/// The empty sorted map with the default comparers, created once per type of the keys and the values, as the empty F# map
/// is. `ImmutableSortedDictionary<'Key, 'Value>.Empty` orders its keys with `Comparer<'Key>.Default`, which is
/// culture-sensitive for strings, so `empty` is this map instead.
type private DefaultSortedMap<'Key, 'Value when 'Key : not null and 'Key : comparison> private () =

    static let empty = (withDefaultComparers<'Key, 'Value>()).empty

    static member Empty : SortedMap<'Key, 'Value> = empty

// ---- Creation ----

[<GeneralizableValue>]
[<CompiledName "Empty">]
let empty<'Key, 'Value when 'Key : not null and 'Key : comparison> : SortedMap<'Key, 'Value> =
    DefaultSortedMap<'Key, 'Value>.Empty

[<CompiledName "ForComparers">]
let forComparers (keyComparer : IComparer<'Key>) (valueComparer : IEqualityComparer<'Value>) =
    CustomComparersSortedMap<'Key, 'Value>(keyComparer, valueComparer)

[<CompiledName "ForKeyComparer">]
let forKeyComparer (keyComparer : IComparer<'Key>) = CustomComparersSortedMap<'Key, 'Value> keyComparer

[<CompiledName "OfSeqBy">]
let ofSeqBy (projection : 'Value -> 'Key) (source : seq<'Value>) : SortedMap<'Key, 'Value> =
    (withDefaultComparers ()).ofSeqBy projection source

[<CompiledName "Singleton">]
let singleton (key : 'Key) (value : 'Value) : SortedMap<'Key, 'Value> = (withDefaultComparers ()).singleton key value

// ---- Builder ----

[<CompiledName "Build">]
let build (action : SortedMap<'Key, 'Value>.Builder -> unit) : SortedMap<'Key, 'Value> =
    (withDefaultComparers<'Key, 'Value>()).build action

[<CompiledName "CreateBuilder">]
let builder () : SortedMap<'Key, 'Value>.Builder = (withDefaultComparers<'Key, 'Value>()).builder()

[<CompiledName "OfBuilder">]
let ofBuilder (builder : SortedMap<'Key, 'Value>.Builder) =
    checkNotNull (nameof builder) builder
    builder.ToImmutable ()

[<CompiledName "ToBuilder">]
let toBuilder (table : SortedMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    table.ToBuilder ()

[<CompiledName "Update">]
let update (action : SortedMap<'Key, 'Value>.Builder -> unit) (table : SortedMap<'Key, 'Value>) : SortedMap<'Key, 'Value> =
    checkNotNull (nameof table) table
    // The builder starts with the bindings and the comparers of the map, and ToImmutable returns the map itself when the
    // action changed nothing
    let builder = table.ToBuilder ()
    action builder
    builder.ToImmutable ()

// ---- Properties ----

[<CompiledName "Count">]
let count (table : SortedMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    table.Count

[<CompiledName "IsEmpty">]
let isEmpty (table : SortedMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    table.IsEmpty

[<CompiledName "KeyComparer">]
let keyComparer (table : SortedMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    table.KeyComparer

[<CompiledName "Keys">]
let keys (table : SortedMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    // The explicit implementation of IDictionary is a view whose count and lookup ask the map, as Map.keys is
    (table :> IDictionary<'Key, 'Value>).Keys

[<CompiledName "ValueComparer">]
let valueComparer (table : SortedMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    table.ValueComparer

[<CompiledName "Values">]
let values (table : SortedMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    (table :> IDictionary<'Key, 'Value>).Values

// ---- Access ----

[<CompiledName "Find">]
let find (key : 'Key) (table : SortedMap<'Key, 'Value>) =
    checkNotNull (nameof table) table

    match table.TryGetValue key with
    | true, value -> value
    | false, _ -> Raise.keyNotFound ()

[<CompiledName "MaxKeyValue">]
let maxKeyValue (table : SortedMap<'Key, 'Value>) =
    checkNotNull (nameof table) table

    if table.IsEmpty then
        // Map.maxKeyValue throws the exception with its default message
        Raise.keyNotFound ()
    else
        // The dictionary enumerates only forwards and hides its tree, so the largest key takes a pass over the bindings
        let pair = Enumerable.Last table
        struct (pair.Key, pair.Value)

[<CompiledName "MinKeyValue">]
let minKeyValue (table : SortedMap<'Key, 'Value>) =
    checkNotNull (nameof table) table

    if table.IsEmpty then
        // Map.minKeyValue throws the exception with its default message
        Raise.keyNotFound ()
    else
        // The enumerator descends the left spine of the tree to the smallest key, which takes O(log n)
        let pair = Enumerable.First table
        struct (pair.Key, pair.Value)

[<CompiledName "TryFind">]
let tryFind (key : 'Key) (table : SortedMap<'Key, 'Value>) =
    checkNotNull (nameof table) table

    match table.TryGetValue key with
    | true, value -> ValueSome value
    | false, _ -> ValueNone

[<CompiledName "TryGetKey">]
let tryGetKey (key : 'Key) (table : SortedMap<'Key, 'Value>) =
    checkNotNull (nameof table) table

    match table.TryGetKey key with
    | true, actualKey -> ValueSome actualKey
    | false, _ -> ValueNone

// ---- Search ----

[<CompiledName "ContainsBinding">]
let containsBinding (key : 'Key) (value : 'Value) (table : SortedMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    // The value comparer of the map decides whether the stored value equals the value, which no other function can ask
    table.Contains (KeyValuePair (key, value))

[<CompiledName "ContainsKey">]
let containsKey (key : 'Key) (table : SortedMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    table.ContainsKey key

[<CompiledName "ContainsValue">]
let containsValue (value : 'Value) (table : SortedMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    table.ContainsValue value

// exists and forall loop over the struct enumerator of the sorted map for value types, which allocates nothing. The
// algorithm that the map modules share, over the key/value pairs of any dictionary, allocated 56 bytes per call, the boxed
// enumerator; LINQ Any and All allocated 136 bytes, and Seq.exists and Seq.forall 80 bytes (measured in Release; a measured
// faster path replaces a higher priority, D5). A map with a reference type as the key or the value is walked through the
// interface instead, at the price of that boxed enumerator, 56 bytes per call: the code that all reference types share needs a
// lookup of the exact type at run time for each call of a member of the generic struct enumerator, and for 100 to 100 000
// bindings exists and forall took 0.55 to 0.84 times as long for strings, and 0.60 to 0.69 for a string next to an integer,
// where for integers the interface took 1.3 to 1.6 times as long. Dispose returns the pooled stack of the enumerator.

[<CompiledName "Exists">]
let exists (predicate : 'Key -> 'Value -> bool) (table : SortedMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    let mutable found = false

    if typeof<'Key>.IsValueType && typeof<'Value>.IsValueType then
        let mutable enumerator = table.GetEnumerator ()

        try
            while not found && enumerator.MoveNext () do
                let pair = enumerator.Current
                found <- predicate pair.Key pair.Value
        finally
            enumerator.Dispose ()
    else
        use enumerator = (table :> seq<KeyValuePair<'Key, 'Value>>).GetEnumerator()

        while not found && enumerator.MoveNext () do
            let pair = enumerator.Current
            found <- predicate pair.Key pair.Value

    found

[<CompiledName "FindKey">]
let findKey predicate (table : SortedMap<'Key, 'Value>) =
    checkNotNull (nameof table) table

    // Map.findKey searches with Seq.pick, whose message differs from the one of Map.find
    match Dictionaries.tryFindKey predicate table with
    | ValueSome key -> key
    | ValueNone -> Raise.indexNotFound ()

[<CompiledName "ForAll">]
let forall (predicate : 'Key -> 'Value -> bool) (table : SortedMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    let mutable satisfied = true

    if typeof<'Key>.IsValueType && typeof<'Value>.IsValueType then
        let mutable enumerator = table.GetEnumerator ()

        try
            while satisfied && enumerator.MoveNext () do
                let pair = enumerator.Current
                satisfied <- predicate pair.Key pair.Value
        finally
            enumerator.Dispose ()
    else
        use enumerator = (table :> seq<KeyValuePair<'Key, 'Value>>).GetEnumerator()

        while satisfied && enumerator.MoveNext () do
            let pair = enumerator.Current
            satisfied <- predicate pair.Key pair.Value

    satisfied

[<CompiledName "Pick">]
let pick (chooser : 'Key -> 'Value -> 'U voption) (table : SortedMap<'Key, 'Value>) =
    checkNotNull (nameof table) table

    match Dictionaries.tryPick chooser table with
    | ValueSome value -> value
    | ValueNone -> Raise.keyNotFound ()

[<CompiledName "TryFindKey">]
let tryFindKey predicate (table : SortedMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    Dictionaries.tryFindKey predicate table

[<CompiledName "TryPick">]
let tryPick (chooser : 'Key -> 'Value -> 'U voption) (table : SortedMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    Dictionaries.tryPick chooser table

// ---- Transformation ----

// add is ImmutableSortedDictionary.Add: it throws for a key bound to another value and returns the map itself for an equal
// binding. Map.add replaces the value, which setItem does (ADR 0001, D14)
[<CompiledName "Add">]
let add (key : 'Key) (value : 'Value) (table : SortedMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    table.Add (key, value)

[<CompiledName "AddRange">]
let addRange (pairs : seq<KeyValuePair<'Key, 'Value>>) (table : SortedMap<'Key, 'Value>) =
    // A default flat list given as a sequence is as good as a null one: it fails when the BCL member enumerates it, and
    // the exception would name the parameter of the member (ADR 0001, D6)
    checkNotNullOrDefaultSeq (nameof pairs) pairs
    checkNotNull (nameof table) table
    table.AddRange pairs

[<CompiledName "Change">]
let change (key : 'Key) (f : 'Value voption -> 'Value voption) (table : SortedMap<'Key, 'Value>) =
    checkNotNull (nameof table) table

    let current =
        match table.TryGetValue key with
        | true, value -> ValueSome value
        | false, _ -> ValueNone

    match f current with
    | ValueSome value -> table.SetItem (key, value)
    | ValueNone -> table.Remove key

[<CompiledName "Choose">]
let choose (chooser : 'Key -> 'Value -> 'U voption) (table : SortedMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    // The keys keep their type, their comparer and so their order; the new values get the default comparer of their type
    let builder = ImmutableSortedDictionary.CreateBuilder<'Key, 'U> table.KeyComparer
    Dictionaries.choose chooser builder table
    builder.ToImmutable ()

[<CompiledName "Clear">]
let clear (table : SortedMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    table.Clear ()

[<CompiledName "Filter">]
let filter predicate (table : SortedMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    // A builder over the input shares the bindings that stay and returns the input itself when every binding stays
    let builder = table.ToBuilder ()
    Dictionaries.filter predicate builder table
    builder.ToImmutable ()

[<CompiledName "Map">]
let map (mapping : 'Key -> 'Value -> 'U) (table : SortedMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    let builder = ImmutableSortedDictionary.CreateBuilder<'Key, 'U> table.KeyComparer
    Dictionaries.map mapping builder table
    builder.ToImmutable ()

[<CompiledName "Remove">]
let remove (key : 'Key) (table : SortedMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    table.Remove key

[<CompiledName "RemoveRange">]
let removeRange (keys : seq<'Key>) (table : SortedMap<'Key, 'Value>) =
    checkNotNullOrDefaultSeq (nameof keys) keys
    checkNotNull (nameof table) table
    table.RemoveRange keys

[<CompiledName "SetItem">]
let setItem (key : 'Key) (value : 'Value) (table : SortedMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    table.SetItem (key, value)

[<CompiledName "SetItems">]
let setItems (pairs : seq<KeyValuePair<'Key, 'Value>>) (table : SortedMap<'Key, 'Value>) =
    checkNotNullOrDefaultSeq (nameof pairs) pairs
    checkNotNull (nameof table) table
    table.SetItems pairs

// ---- Folding ----

[<CompiledName "Fold">]
let fold (folder : 'State -> 'Key -> 'Value -> 'State) (state : 'State) (table : SortedMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    Dictionaries.fold folder state table

[<CompiledName "FoldBack">]
let foldBack (folder : 'Key -> 'Value -> 'State -> 'State) (table : SortedMap<'Key, 'Value>) (state : 'State) =
    checkNotNull (nameof table) table
    // The bindings come in ascending order, so the reverse order of enumeration is the descending order of Map.foldBack
    Dictionaries.foldBack folder table state

[<CompiledName "Iterate">]
let iter action (table : SortedMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    Dictionaries.iter action table

// ---- Partitioning ----

[<CompiledName "Partition">]
let partition predicate (table : SortedMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    let accepted = table.ToBuilder ()
    let rejected = table.Clear().ToBuilder()
    Dictionaries.partition predicate accepted rejected table
    struct (accepted.ToImmutable (), rejected.ToImmutable ())

// ---- Conversion ----

[<CompiledName "ToArray">]
let toArray (table : SortedMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    Dictionaries.toArray structPair table

[<CompiledName "ToList">]
let toList (table : SortedMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    Dictionaries.toList structPair table

[<CompiledName "ToSeq">]
let toSeq (table : SortedMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    Dictionaries.toSeq structPair table

// The submodules take the bindings as tuples, convert them to key/value pairs on demand and call the members that the
// functions of the module call, so that every shape has the semantics of the BCL member (D13).
[<RequireQualifiedAccess>]
module Tuple =

    // ---- Transformation ----

    [<CompiledName "AddRange">]
    let addRange (pairs : seq<'Key * 'Value>) (table : SortedMap<'Key, 'Value>) =
        checkNotNullOrDefaultSeq (nameof pairs) pairs
        checkNotNull (nameof table) table
        table.AddRange (Dictionaries.toKeyValuePairs Dictionaries.splitTuple pairs)

    [<CompiledName "SetItems">]
    let setItems (pairs : seq<'Key * 'Value>) (table : SortedMap<'Key, 'Value>) =
        checkNotNullOrDefaultSeq (nameof pairs) pairs
        checkNotNull (nameof table) table
        table.SetItems (Dictionaries.toKeyValuePairs Dictionaries.splitTuple pairs)

[<RequireQualifiedAccess>]
module ValueTuple =

    // ---- Transformation ----

    [<CompiledName "AddRange">]
    let addRange (pairs : seq<struct ('Key * 'Value)>) (table : SortedMap<'Key, 'Value>) =
        checkNotNullOrDefaultSeq (nameof pairs) pairs
        checkNotNull (nameof table) table
        table.AddRange (Dictionaries.toKeyValuePairs id pairs)

    [<CompiledName "SetItems">]
    let setItems (pairs : seq<struct ('Key * 'Value)>) (table : SortedMap<'Key, 'Value>) =
        checkNotNullOrDefaultSeq (nameof pairs) pairs
        checkNotNull (nameof table) table
        table.SetItems (Dictionaries.toKeyValuePairs id pairs)
