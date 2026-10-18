/// The functions of the `HashMap` module for the contract tests.
[<RequireQualifiedAccess>]
module FSharp.Collections.Immutable.Tests.HashMapOps

open System.Collections.Generic
open FSharp.Collections.Immutable

/// An equality comparer of keys that compares their absolute values, for the comparer struct.
let absoluteKeyEquality = {
    new IEqualityComparer<int> with
        member _.Equals (left, right) = abs left = abs right
        member _.GetHashCode key = abs key
}

/// The functions of the reading facet. `HashMap` names its map parameter `table`, as the `Map` module does.
let reading : MapOps<HashMap<int, string>> = {
    Name = nameof HashMap
    ParamName = id
    OfSeq = HashMap.ofSeq
    ToArray = HashMap.toArray

    Count = HashMap.count
    IsEmpty = HashMap.isEmpty
    Keys = fun table -> HashMap.keys table
    Values = fun table -> HashMap.values table

    Find = HashMap.find
    TryFind = HashMap.tryFind

    ContainsBinding = HashMap.containsBinding
    ContainsKey = HashMap.containsKey
    Exists = HashMap.exists
    FindKey = HashMap.findKey
    Forall = HashMap.forall
    Pick = HashMap.pick
    TryFindKey = HashMap.tryFindKey
    TryPick = HashMap.tryPick

    Fold = HashMap.fold
    FoldBack = HashMap.foldBack
    Iter = HashMap.iter

    ToList = HashMap.toList
    ToSeq = HashMap.toSeq
}

/// The functions of the transformation facet.
let transform : MapTransformOps<HashMap<int, string>> = {
    Name = nameof HashMap
    ParamName = id
    OfSeq = HashMap.ofSeq
    ToArray = HashMap.toArray

    Choose = fun chooser table -> HashMap.choose chooser table |> HashMap.toArray
    Filter = HashMap.filter
    Map = fun mapping table -> HashMap.map mapping table |> HashMap.toArray
    Partition = HashMap.partition
}

/// The functions of the mutation facet: `addRange` and `setItems` of the module over key/value pairs, and of its nested
/// modules over tuples (ADR 0001, D13).
let mutation : MapMutationOps<HashMap<int, string>> = {
    Name = nameof HashMap
    ParamName = id
    OfSeq = HashMap.ofSeq
    ToArray = HashMap.toArray

    Add = HashMap.add
    Change = HashMap.change
    Clear = HashMap.clear
    Remove = HashMap.remove
    RemoveRange = HashMap.removeRange
    SetItem = HashMap.setItem

    KeyValuePairs = { AddRange = HashMap.addRange; SetItems = HashMap.setItems }
    Tuples = { AddRange = HashMap.Tuple.addRange; SetItems = HashMap.Tuple.setItems }
    StructTuples = {
        AddRange = HashMap.ValueTuple.addRange
        SetItems = HashMap.ValueTuple.setItems
    }
}

/// The functions of the creation facet with the default comparers: the functions of the module and the overloads of the
/// extension members, one shape per field (ADR 0001, D13).
let creation : MapCreationOps<HashMap<int, string>> = {
    Name = nameof HashMap
    ParamName = id
    OfSeq = HashMap.ofSeq
    ToArray = HashMap.toArray
    ContainsKey = HashMap.containsKey
    EquivalentKey = id

    Empty = fun () -> HashMap.empty
    KeyValuePairs = { OfArray = HashMap.ofArray; OfList = HashMap.ofList; OfSeq = HashMap.ofSeq }
    Tuples = { OfArray = HashMap.ofArray; OfList = HashMap.ofList; OfSeq = HashMap.ofSeq }
    StructTuples = { OfArray = HashMap.ofArray; OfList = HashMap.ofList; OfSeq = HashMap.ofSeq }
    OfSeqBy = HashMap.ofSeqBy
    Singleton = HashMap.singleton
}

/// The comparer struct whose key comparer compares the absolute values of the keys.
let private absoluteKeys : CustomComparersHashMap<int, string> =
    HashMap.forKeyComparer absoluteKeyEquality

/// The functions of the creation facet through the comparer struct with a key comparer of absolute values (ADR 0001, D12):
/// every created map finds a key by its negation.
let customComparersCreation : MapCreationOps<HashMap<int, string>> = {
    Name = nameof CustomComparersHashMap
    ParamName = id
    OfSeq = HashMap.ofSeq
    ToArray = HashMap.toArray
    ContainsKey = HashMap.containsKey
    EquivalentKey = fun key -> -key

    Empty = fun () -> absoluteKeys.empty
    KeyValuePairs = {
        OfArray = absoluteKeys.ofArray
        OfList = absoluteKeys.ofList
        OfSeq = absoluteKeys.ofSeq
    }
    Tuples = {
        OfArray = absoluteKeys.ofArray
        OfList = absoluteKeys.ofList
        OfSeq = absoluteKeys.ofSeq
    }
    StructTuples = {
        OfArray = absoluteKeys.ofArray
        OfList = absoluteKeys.ofList
        OfSeq = absoluteKeys.ofSeq
    }
    OfSeqBy = absoluteKeys.ofSeqBy
    Singleton = absoluteKeys.singleton
}
