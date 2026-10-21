/// The functions of the `SortedSet` module for the contract tests.
[<RequireQualifiedAccess>]
module FSharp.Collections.Immutable.Tests.SortedSetOps

open System
open FSharp.Collections.Immutable
open Microsoft.VisualStudio.TestTools.UnitTesting

/// <summary>
/// Maps the name of a parameter of the FSharp.Core <c>Set</c> module to the name in the <c>SortedSet</c> module.
/// </summary>
/// <remarks>
/// <c>Set.minElement</c> and <c>Set.maxElement</c> throw from the tree of the F# set, which names the set <c>s</c>. The
/// other names map as in <c>SetParamNames.ofFSharpCore</c>.
/// </remarks>
let paramName (name : string) =
    match name with
    | "s" -> "set"
    | name -> SetParamNames.ofFSharpCore name

/// <summary>
/// The elements of a sorted set, after a check that the sorted set enumerates them in ascending order of its comparer.
/// </summary>
/// <remarks>
/// The contracts compare the elements of a set as a sorted array, because a hash set enumerates them in an unspecified
/// order. A sorted set must enumerate them in ascending order of its comparer, so the contracts check it through this
/// function on every sorted set that they read. For the sorted sets that the contracts create without a comparer, that
/// is the order of <c>Set</c>; the struct of <c>forComparer</c> orders by remainders.
/// </remarks>
let toArray (set : SortedSet<'T>) =
    let elements = SortedSet.toArray set
    let comparer = SortedSet.comparer set

    Assert.IsTrue (
        elements
        |> Array.pairwise
        |> Array.forall (fun (previous, next) -> comparer.Compare (previous, next) < 0),
        "A sorted set must enumerate its distinct elements in ascending order of its comparer"
    )

    elements

let private toArrayPair (struct (first : SortedSet<'T1>, second : SortedSet<'T2>)) = struct (toArray first, toArray second)

/// Creates sorted sets that order by remainders: the struct of `forComparer` with `SetInputs.ByRemainder`.
let private ofSeqByRemainder (elements : seq<int>) = (SortedSet.forComparer SetInputs.ByRemainder).ofSeq elements

/// The functions of the reading facet.
let reading : SetOps<SortedSet<int>> = {
    Name = nameof SortedSet
    ParamName = paramName
    OfSeq = SortedSet.ofSeq
    OfSeqByRemainder = ofSeqByRemainder
    ToArray = toArray

    Count = SortedSet.count
    IsEmpty = SortedSet.isEmpty

    TryGetValue = SortedSet.tryGetValue

    Contains = SortedSet.contains
    Exists = SortedSet.exists
    Forall = SortedSet.forall
    IsProperSubset = SortedSet.isProperSubset
    IsProperSuperset = SortedSet.isProperSuperset
    IsSubset = SortedSet.isSubset
    IsSuperset = SortedSet.isSuperset
    Overlaps = SortedSet.overlaps
    SetEquals = SortedSet.setEquals

    Fold = SortedSet.fold
    FoldBack = SortedSet.foldBack
    Iter = SortedSet.iter

    ToList = SortedSet.toList
    ToSeq = SortedSet.toSeq
}

/// The functions of the transformation facet.
let transform : SetTransformOps<SortedSet<int>> = {
    Name = nameof SortedSet
    ParamName = paramName
    OfSeq = SortedSet.ofSeq
    ToArray = toArray

    Choose = fun chooser set -> SortedSet.choose chooser set |> toArray
    Difference = SortedSet.difference
    Filter = SortedSet.filter
    Intersect = SortedSet.intersect
    IntersectMany = SortedSet.intersectMany
    Map = fun mapping set -> SortedSet.map mapping set |> toArray
    SymmetricDifference = SortedSet.symmetricDifference
    Union = SortedSet.union
    UnionMany = SortedSet.unionMany

    Partition = SortedSet.partition
    PartitionWith = fun partitioner set -> SortedSet.partitionWith partitioner set |> toArrayPair
}

/// The functions of the mutation facet.
let mutation : SetMutationOps<SortedSet<int>> = {
    Name = nameof SortedSet
    ParamName = paramName
    OfSeq = SortedSet.ofSeq
    OfSeqByRemainder = ofSeqByRemainder
    ToArray = toArray

    Add = SortedSet.add
    AddRange = SortedSet.addRange
    Clear = SortedSet.clear
    Remove = SortedSet.remove
    RemoveRange = SortedSet.removeRange
}

/// The functions of the creation facet; the struct of `forComparer` orders with `SetInputs.ByRemainder`.
let creation : SetCreationOps<SortedSet<int>> =
    let custom = SortedSet.forComparer SetInputs.ByRemainder

    {
        Name = nameof SortedSet
        ParamName = paramName
        ToArray = toArray
        Comparer = fun set -> SortedSet.comparer set :> obj

        Empty = fun () -> SortedSet.empty
        OfArray = SortedSet.ofArray
        OfList = SortedSet.ofList
        OfReadOnlySpan = fun array -> SortedSet.ofReadOnlySpan (ReadOnlySpan<int> array)
        OfSeq = SortedSet.ofSeq
        OfSpan = fun array -> SortedSet.ofSpan (Span<int> array)
        Singleton = SortedSet.singleton

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
