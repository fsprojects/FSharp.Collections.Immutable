/// The functions of the `SortedMap` module for the contract tests.
[<RequireQualifiedAccess>]
module FSharp.Collections.Immutable.Tests.SortedMapOps

open FSharp.Collections.Immutable

/// A comparer of keys that orders them by their absolute values, for the comparer struct.
let absoluteKeyOrder =
    ComparisonIdentity.FromFunction (fun (left : int) (right : int) -> compare (abs left) (abs right))

/// The functions of the reading facet. `SortedMap` names its map parameter `table`, as the `Map` module does.
let reading : MapOps<SortedMap<int, string>> = {
    Name = nameof SortedMap
    ParamName = id
    OfSeq = SortedMap.ofSeq
    ToArray = SortedMap.toArray

    Count = SortedMap.count
    IsEmpty = SortedMap.isEmpty
    Keys = fun table -> SortedMap.keys table
    Values = fun table -> SortedMap.values table

    Find = SortedMap.find
    TryFind = SortedMap.tryFind

    ContainsBinding = SortedMap.containsBinding
    ContainsKey = SortedMap.containsKey
    Exists = SortedMap.exists
    FindKey = SortedMap.findKey
    Forall = SortedMap.forall
    Pick = SortedMap.pick
    TryFindKey = SortedMap.tryFindKey
    TryPick = SortedMap.tryPick

    Fold = SortedMap.fold
    FoldBack = SortedMap.foldBack
    Iter = SortedMap.iter

    ToList = SortedMap.toList
    ToSeq = SortedMap.toSeq
}

/// The functions of the transformation facet.
let transform : MapTransformOps<SortedMap<int, string>> = {
    Name = nameof SortedMap
    ParamName = id
    OfSeq = SortedMap.ofSeq
    ToArray = SortedMap.toArray

    Choose = fun chooser table -> SortedMap.choose chooser table |> SortedMap.toArray
    Filter = SortedMap.filter
    Map = fun mapping table -> SortedMap.map mapping table |> SortedMap.toArray
    Partition = SortedMap.partition
}

/// The functions of the mutation facet: `addRange` and `setItems` of the module over key/value pairs, and of its nested
/// modules over tuples (ADR 0001, D13).
let mutation : MapMutationOps<SortedMap<int, string>> = {
    Name = nameof SortedMap
    ParamName = id
    OfSeq = SortedMap.ofSeq
    ToArray = SortedMap.toArray

    Add = SortedMap.add
    Change = SortedMap.change
    Clear = SortedMap.clear
    Remove = SortedMap.remove
    RemoveRange = SortedMap.removeRange
    SetItem = SortedMap.setItem

    KeyValuePairs = { AddRange = SortedMap.addRange; SetItems = SortedMap.setItems }
    Tuples = { AddRange = SortedMap.Tuple.addRange; SetItems = SortedMap.Tuple.setItems }
    StructTuples = {
        AddRange = SortedMap.ValueTuple.addRange
        SetItems = SortedMap.ValueTuple.setItems
    }
}

/// The functions of the creation facet with the default comparers: the functions of the module and the overloads of the
/// extension members, one shape per field (ADR 0001, D13).
let creation : MapCreationOps<SortedMap<int, string>> = {
    Name = nameof SortedMap
    ParamName = id
    OfSeq = SortedMap.ofSeq
    ToArray = SortedMap.toArray
    ContainsKey = SortedMap.containsKey
    EquivalentKey = id

    Empty = fun () -> SortedMap.empty
    KeyValuePairs = {
        OfArray = SortedMap.ofArray
        OfList = SortedMap.ofList
        OfSeq = SortedMap.ofSeq
    }
    Tuples = {
        OfArray = SortedMap.ofArray
        OfList = SortedMap.ofList
        OfSeq = SortedMap.ofSeq
    }
    StructTuples = {
        OfArray = SortedMap.ofArray
        OfList = SortedMap.ofList
        OfSeq = SortedMap.ofSeq
    }
    OfSeqBy = SortedMap.ofSeqBy
    Singleton = SortedMap.singleton
}

/// The comparer struct whose key comparer orders the keys by their absolute values.
let private absoluteKeys : CustomComparersSortedMap<int, string> =
    SortedMap.forKeyComparer absoluteKeyOrder

/// The functions of the creation facet through the comparer struct with a key comparer of absolute values (ADR 0001, D12):
/// every created map finds a key by its negation.
let customComparersCreation : MapCreationOps<SortedMap<int, string>> = {
    Name = nameof CustomComparersSortedMap
    ParamName = id
    OfSeq = SortedMap.ofSeq
    ToArray = SortedMap.toArray
    ContainsKey = SortedMap.containsKey
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
