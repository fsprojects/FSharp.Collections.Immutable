[<RequireQualifiedAccess>]
module FSharp.Collections.Immutable.Tuple.ISet

open System.Collections.Immutable
open FSharp.Collections.Immutable

// The functions delegate to the default module and repack its struct tuples.

// ---- Partitioning ----

[<CompiledName "Partition">]
let partition (predicate : 'T -> bool) (set : IImmutableSet<'T>) =
    let struct (accepted, rejected) = ISet.partition predicate set
    accepted, rejected

[<CompiledName "PartitionWith">]
let partitionWith (partitioner : 'T -> Choice<'T1, 'T2>) (set : IImmutableSet<'T>) =
    let struct (first, second) = ISet.partitionWith partitioner set
    first, second
