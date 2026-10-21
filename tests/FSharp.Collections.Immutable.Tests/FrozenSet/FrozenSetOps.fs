/// The functions of the `FrozenSet` module for the contract tests.
[<RequireQualifiedAccess>]
module FSharp.Collections.Immutable.Tests.FrozenSetOps

open System
open System.Collections.Frozen
open System.Collections.Generic
open FSharp.Collections.Immutable

let private toArrayPair (struct (first : FrozenSet<'T1>, second : FrozenSet<'T2>)) =
    struct (FrozenSet.toArray first, FrozenSet.toArray second)

/// The functions of the reading facet.
let reading : SetOps<FrozenSet<int>> = {
    Name = nameof FrozenSet
    ParamName = SetParamNames.ofFSharpCore
    OfSeq = FrozenSet.ofSeq
    OfSeqByRemainder = fun elements -> (FrozenSet.forComparer SetInputs.SameRemainder).ofSeq elements
    ToArray = FrozenSet.toArray

    Count = FrozenSet.count
    IsEmpty = FrozenSet.isEmpty

    TryGetValue = FrozenSet.tryGetValue

    Contains = FrozenSet.contains
    Exists = FrozenSet.exists
    Forall = FrozenSet.forall
    IsProperSubset = FrozenSet.isProperSubset
    IsProperSuperset = FrozenSet.isProperSuperset
    IsSubset = FrozenSet.isSubset
    IsSuperset = FrozenSet.isSuperset
    Overlaps = FrozenSet.overlaps
    SetEquals = FrozenSet.setEquals

    Fold = FrozenSet.fold
    FoldBack = FrozenSet.foldBack
    Iter = FrozenSet.iter

    ToList = FrozenSet.toList
    ToSeq = FrozenSet.toSeq
}

/// The functions of the transformation facet.
let transform : SetTransformOps<FrozenSet<int>> = {
    Name = nameof FrozenSet
    ParamName = SetParamNames.ofFSharpCore
    OfSeq = FrozenSet.ofSeq
    ToArray = FrozenSet.toArray

    Choose = fun chooser set -> FrozenSet.choose chooser set |> FrozenSet.toArray
    Difference = FrozenSet.difference
    Filter = FrozenSet.filter
    Intersect = FrozenSet.intersect
    IntersectMany = FrozenSet.intersectMany
    Map = fun mapping set -> FrozenSet.map mapping set |> FrozenSet.toArray
    SymmetricDifference = FrozenSet.symmetricDifference
    Union = FrozenSet.union
    UnionMany = FrozenSet.unionMany

    Partition = FrozenSet.partition
    PartitionWith = fun partitioner set -> FrozenSet.partitionWith partitioner set |> toArrayPair
}

/// The functions of the creation facet; the struct of `forComparer` compares with `SetInputs.SameRemainder`. A frozen set
/// has no mutation facet: it is built once (ADR 0001, D1).
let creation : SetCreationOps<FrozenSet<int>> =
    let custom = FrozenSet.forComparer SetInputs.SameRemainder

    {
        Name = nameof FrozenSet
        ParamName = SetParamNames.ofFSharpCore
        ToArray = FrozenSet.toArray
        Comparer = fun set -> FrozenSet.comparer set :> obj

        Empty = fun () -> FrozenSet.empty
        OfArray = FrozenSet.ofArray
        OfList = FrozenSet.ofList
        OfReadOnlySpan = fun array -> FrozenSet.ofReadOnlySpan (ReadOnlySpan<int> array)
        OfSeq = FrozenSet.ofSeq
        OfSpan = fun array -> FrozenSet.ofSpan (Span<int> array)
        Singleton = FrozenSet.singleton

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

#if NET10_0_OR_GREATER
/// <summary>
/// Compares <see cref="T:System.Int64"/> values, and <see cref="T:System.Int32"/> values with them, for the alternate
/// lookups, which exist on <c>net10.0</c> only.
/// </summary>
[<Sealed>]
type Int64WithInt32Comparer () =

    interface IEqualityComparer<int64> with
        member _.Equals (left : int64, right : int64) = left = right
        member _.GetHashCode (value : int64) = value.GetHashCode ()

    interface IAlternateEqualityComparer<int, int64> with
        member _.Equals (alternate : int, other : int64) = int64 alternate = other
        member _.GetHashCode (alternate : int) = (int64 alternate).GetHashCode()
        member _.Create (alternate : int) = int64 alternate

/// <summary>
/// A frozen set of the values, whose comparer also compares <see cref="T:System.Int32"/> values with them.
/// </summary>
let ofInt64WithInt32Comparer (values : seq<int64>) = (FrozenSet.forComparer (Int64WithInt32Comparer ())).ofSeq values
#endif
