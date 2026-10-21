/// The functions of the `FrozenMap` module for the contract tests.
/// A frozen map has no incremental changes, so the module has no mutation facet (ADR 0001, D1), and it has no value
/// comparer and no builder, so the creation facet holds neither.
[<RequireQualifiedAccess>]
module FSharp.Collections.Immutable.Tests.FrozenMapOps

open System.Collections.Generic
open FSharp.Collections.Immutable

#if NET10_0_OR_GREATER
/// An equality comparer of `int64` keys that looks them up by `int` alternates, for the alternate lookups (ADR 0001, D11).
let int64ByInt32 = {
    new IEqualityComparer<int64> with
        member _.Equals (left, right) = left = right
        member _.GetHashCode key = key.GetHashCode ()
    interface IAlternateEqualityComparer<int, int64> with
        member _.Equals (alternate : int, key : int64) = int64 alternate = key
        member _.GetHashCode (alternate : int) = (int64 alternate).GetHashCode()
        member _.Create (alternate : int) = int64 alternate
}
#endif

/// The functions of the reading facet. `FrozenMap` names its map parameter `table`, as the `Map` module does.
let reading : MapOps<FrozenMap<int, string>> = {
    Name = nameof FrozenMap
    ParamName = id
    OfSeq = FrozenMap.ofSeq
    ToArray = FrozenMap.toArray

    Count = FrozenMap.count
    IsEmpty = FrozenMap.isEmpty
    // The flat lists of the keys and the values are structs, which the record takes as sequences
    Keys = fun table -> FrozenMap.keys table :> seq<_>
    Values = fun table -> FrozenMap.values table :> seq<_>

    Find = FrozenMap.find
    TryFind = FrozenMap.tryFind

    ContainsBinding = FrozenMap.containsBinding
    ContainsKey = FrozenMap.containsKey
    Exists = FrozenMap.exists
    FindKey = FrozenMap.findKey
    Forall = FrozenMap.forall
    Pick = FrozenMap.pick
    TryFindKey = FrozenMap.tryFindKey
    TryPick = FrozenMap.tryPick

    Fold = FrozenMap.fold
    FoldBack = FrozenMap.foldBack
    Iter = FrozenMap.iter

    ToList = FrozenMap.toList
    ToSeq = FrozenMap.toSeq
}

/// The functions of the transformation facet.
let transform : MapTransformOps<FrozenMap<int, string>> = {
    Name = nameof FrozenMap
    ParamName = id
    OfSeq = FrozenMap.ofSeq
    ToArray = FrozenMap.toArray

    Choose = fun chooser table -> FrozenMap.choose chooser table |> FrozenMap.toArray
    Filter = FrozenMap.filter
    Map = fun mapping table -> FrozenMap.map mapping table |> FrozenMap.toArray
    Partition = FrozenMap.partition
}

/// The functions of the creation facet with the default comparer: the functions of the module and the overloads of the
/// extension members, one shape per field (ADR 0001, D13).
let creation : MapCreationOps<FrozenMap<int, string>> = {
    Name = nameof FrozenMap
    ParamName = id
    OfSeq = FrozenMap.ofSeq
    ToArray = FrozenMap.toArray
    ContainsKey = FrozenMap.containsKey
    EquivalentKey = id

    Empty = fun () -> FrozenMap.empty
    KeyValuePairs = {
        OfArray = FrozenMap.ofArray
        OfList = FrozenMap.ofList
        OfSeq = FrozenMap.ofSeq
    }
    Tuples = {
        OfArray = FrozenMap.ofArray
        OfList = FrozenMap.ofList
        OfSeq = FrozenMap.ofSeq
    }
    StructTuples = {
        OfArray = FrozenMap.ofArray
        OfList = FrozenMap.ofList
        OfSeq = FrozenMap.ofSeq
    }
    OfSeqBy = FrozenMap.ofSeqBy
    Singleton = FrozenMap.singleton
}

/// The comparer struct whose key comparer compares the absolute values of the keys. A frozen map takes the same type of
/// key comparer as a hash map.
let private absoluteKeys : CustomComparersFrozenMap<int, string> =
    FrozenMap.forKeyComparer HashMapOps.absoluteKeyEquality

/// The functions of the creation facet through the comparer struct with a key comparer of absolute values (ADR 0001, D12):
/// every created map finds a key by its negation.
let customComparersCreation : MapCreationOps<FrozenMap<int, string>> = {
    Name = nameof CustomComparersFrozenMap
    ParamName = id
    OfSeq = FrozenMap.ofSeq
    ToArray = FrozenMap.toArray
    ContainsKey = FrozenMap.containsKey
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
