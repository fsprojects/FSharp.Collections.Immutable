/// The functions of the `HashSet` module for the contract tests.
[<RequireQualifiedAccess>]
module FSharp.Collections.Immutable.Tests.HashSetOps

open System
open FSharp.Collections.Immutable

let private toArrayPair (struct (first : HashSet<'T1>, second : HashSet<'T2>)) =
    struct (HashSet.toArray first, HashSet.toArray second)

/// Creates hash sets that compare by remainders: the struct of `forComparer` with `SetInputs.SameRemainder`.
let private ofSeqByRemainder (elements : seq<int>) = (HashSet.forComparer SetInputs.SameRemainder).ofSeq elements

/// The functions of the reading facet.
let reading : SetOps<HashSet<int>> = {
    Name = nameof HashSet
    ParamName = SetParamNames.ofFSharpCore
    OfSeq = HashSet.ofSeq
    OfSeqByRemainder = ofSeqByRemainder
    ToArray = HashSet.toArray

    Count = HashSet.count
    IsEmpty = HashSet.isEmpty

    TryGetValue = HashSet.tryGetValue

    Contains = HashSet.contains
    Exists = HashSet.exists
    Forall = HashSet.forall
    IsProperSubset = HashSet.isProperSubset
    IsProperSuperset = HashSet.isProperSuperset
    IsSubset = HashSet.isSubset
    IsSuperset = HashSet.isSuperset
    Overlaps = HashSet.overlaps
    SetEquals = HashSet.setEquals

    Fold = HashSet.fold
    FoldBack = HashSet.foldBack
    Iter = HashSet.iter

    ToList = HashSet.toList
    ToSeq = HashSet.toSeq
}

/// The functions of the transformation facet.
let transform : SetTransformOps<HashSet<int>> = {
    Name = nameof HashSet
    ParamName = SetParamNames.ofFSharpCore
    OfSeq = HashSet.ofSeq
    ToArray = HashSet.toArray

    Choose = fun chooser set -> HashSet.choose chooser set |> HashSet.toArray
    Difference = HashSet.difference
    Filter = HashSet.filter
    Intersect = HashSet.intersect
    IntersectMany = HashSet.intersectMany
    Map = fun mapping set -> HashSet.map mapping set |> HashSet.toArray
    SymmetricDifference = HashSet.symmetricDifference
    Union = HashSet.union
    UnionMany = HashSet.unionMany

    Partition = HashSet.partition
    PartitionWith = fun partitioner set -> HashSet.partitionWith partitioner set |> toArrayPair
}

/// The functions of the mutation facet.
let mutation : SetMutationOps<HashSet<int>> = {
    Name = nameof HashSet
    ParamName = SetParamNames.ofFSharpCore
    OfSeq = HashSet.ofSeq
    OfSeqByRemainder = ofSeqByRemainder
    ToArray = HashSet.toArray

    Add = HashSet.add
    AddRange = HashSet.addRange
    Clear = HashSet.clear
    Remove = HashSet.remove
    RemoveRange = HashSet.removeRange
}

/// The functions of the creation facet; the struct of `forComparer` compares with `SetInputs.SameRemainder`.
let creation : SetCreationOps<HashSet<int>> =
    let custom = HashSet.forComparer SetInputs.SameRemainder

    {
        Name = nameof HashSet
        ParamName = SetParamNames.ofFSharpCore
        ToArray = HashSet.toArray
        Comparer = fun set -> HashSet.comparer set :> obj

        Empty = fun () -> HashSet.empty
        OfArray = HashSet.ofArray
        OfList = HashSet.ofList
        OfReadOnlySpan = fun array -> HashSet.ofReadOnlySpan (ReadOnlySpan<int> array)
        OfSeq = HashSet.ofSeq
        OfSpan = fun array -> HashSet.ofSpan (Span<int> array)
        Singleton = HashSet.singleton

        CustomComparer = {
            Comparer = custom.Comparer :> obj

            Empty = fun () -> custom.empty
            OfArray = fun array -> custom.ofArray array
            OfList = fun elements -> custom.ofList elements
            OfReadOnlySpan = fun array -> custom.ofReadOnlySpan (ReadOnlySpan<int> array)
            OfSeq = fun elements -> custom.ofSeq elements
            OfSpan = fun array -> custom.ofSpan (Span<int> array)
            Singleton = fun value -> custom.singleton value
        }
    }
