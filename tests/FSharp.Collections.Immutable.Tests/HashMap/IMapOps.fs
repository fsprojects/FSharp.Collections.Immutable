/// The functions of the `IMap` module for the contract tests.
/// The interface module has no creation functions, so the maps under test are hash maps seen through the interface.
[<RequireQualifiedAccess>]
module FSharp.Collections.Immutable.Tests.IMapOps

open FSharp.Collections.Immutable

let private ofSeq (elements : seq<struct (int * string)>) = HashMap.ofSeq elements :> IMap<int, string>

/// The functions of the reading facet. `IMap` names its map parameter `table`, as the `Map` module does.
let reading : MapOps<IMap<int, string>> = {
    Name = nameof IMap
    ParamName = id
    OfSeq = ofSeq
    ToArray = IMap.toArray

    Count = IMap.count
    IsEmpty = IMap.isEmpty
    Keys = IMap.keys
    Values = IMap.values

    Find = IMap.find
    TryFind = IMap.tryFind

    ContainsBinding = IMap.containsBinding
    ContainsKey = IMap.containsKey
    Exists = IMap.exists
    FindKey = IMap.findKey
    Forall = IMap.forall
    Pick = IMap.pick
    TryFindKey = IMap.tryFindKey
    TryPick = IMap.tryPick

    Fold = IMap.fold
    FoldBack = IMap.foldBack
    Iter = IMap.iter

    ToList = IMap.toList
    ToSeq = IMap.toSeq
}

/// The functions of the transformation facet; `choose` and `map` return hash maps.
let transform : MapTransformOps<IMap<int, string>> = {
    Name = nameof IMap
    ParamName = id
    OfSeq = ofSeq
    ToArray = IMap.toArray

    Choose = fun chooser table -> IMap.choose chooser table |> HashMap.toArray
    Filter = IMap.filter
    Map = fun mapping table -> IMap.map mapping table |> HashMap.toArray
    Partition = IMap.partition
}

/// The functions of the mutation facet: `addRange` and `setItems` of the module over key/value pairs, and of its nested
/// modules over tuples (ADR 0001, D13).
let mutation : MapMutationOps<IMap<int, string>> = {
    Name = nameof IMap
    ParamName = id
    OfSeq = ofSeq
    ToArray = IMap.toArray

    Add = IMap.add
    Change = IMap.change
    Clear = IMap.clear
    Remove = IMap.remove
    RemoveRange = IMap.removeRange
    SetItem = IMap.setItem

    KeyValuePairs = { AddRange = IMap.addRange; SetItems = IMap.setItems }
    Tuples = { AddRange = IMap.Tuple.addRange; SetItems = IMap.Tuple.setItems }
    StructTuples = { AddRange = IMap.ValueTuple.addRange; SetItems = IMap.ValueTuple.setItems }
}
