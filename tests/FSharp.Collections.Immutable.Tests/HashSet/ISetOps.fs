/// The functions of the `ISet` module for the contract tests, over hash sets behind the interface.
[<RequireQualifiedAccess>]
module FSharp.Collections.Immutable.Tests.ISetOps

open System.Collections.Immutable
open FSharp.Collections.Immutable

/// The sets of the contract tests are hash sets behind the interface.
let private ofSeq (elements : seq<int>) = HashSet.ofSeq elements :> IImmutableSet<int>

/// The sets that compare by remainders are hash sets of the struct of `forComparer` with `SetInputs.SameRemainder`.
let private ofSeqByRemainder (elements : seq<int>) =
    (HashSet.forComparer SetInputs.SameRemainder).ofSeq elements :> IImmutableSet<int>

let private toArrayPair (struct (first : HashSet<'T1>, second : HashSet<'T2>)) =
    struct (HashSet.toArray first, HashSet.toArray second)

/// The functions of the reading facet.
let reading : SetOps<IImmutableSet<int>> = {
    Name = nameof ISet
    ParamName = SetParamNames.ofFSharpCore
    OfSeq = ofSeq
    OfSeqByRemainder = ofSeqByRemainder
    ToArray = ISet.toArray

    Count = ISet.count
    IsEmpty = ISet.isEmpty

    TryGetValue = ISet.tryGetValue

    Contains = ISet.contains
    Exists = ISet.exists
    Forall = ISet.forall
    IsProperSubset = ISet.isProperSubset
    IsProperSuperset = ISet.isProperSuperset
    IsSubset = ISet.isSubset
    IsSuperset = ISet.isSuperset
    Overlaps = ISet.overlaps
    SetEquals = ISet.setEquals

    Fold = ISet.fold
    FoldBack = ISet.foldBack
    Iter = ISet.iter

    ToList = ISet.toList
    ToSeq = ISet.toSeq
}

/// The functions of the transformation facet; the functions to another element type return hash sets.
let transform : SetTransformOps<IImmutableSet<int>> = {
    Name = nameof ISet
    ParamName = SetParamNames.ofFSharpCore
    OfSeq = ofSeq
    ToArray = ISet.toArray

    Choose = fun chooser set -> ISet.choose chooser set |> HashSet.toArray
    Difference = ISet.difference
    Filter = ISet.filter
    Intersect = ISet.intersect
    IntersectMany = ISet.intersectMany
    Map = fun mapping set -> ISet.map mapping set |> HashSet.toArray
    SymmetricDifference = ISet.symmetricDifference
    Union = ISet.union
    UnionMany = ISet.unionMany

    Partition = ISet.partition
    PartitionWith = fun partitioner set -> ISet.partitionWith partitioner set |> toArrayPair
}

/// The functions of the mutation facet.
let mutation : SetMutationOps<IImmutableSet<int>> = {
    Name = nameof ISet
    ParamName = SetParamNames.ofFSharpCore
    OfSeq = ofSeq
    OfSeqByRemainder = ofSeqByRemainder
    ToArray = ISet.toArray

    Add = ISet.add
    AddRange = ISet.addRange
    Clear = ISet.clear
    Remove = ISet.remove
    RemoveRange = ISet.removeRange
}
