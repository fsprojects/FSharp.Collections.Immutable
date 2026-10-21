[<RequireQualifiedAccess>]
module FSharp.Collections.Immutable.Tuple.HashSet

open FSharp.Collections.Immutable

// The functions delegate to the default module and repack its struct tuples.

// ---- Partitioning ----

[<CompiledName "Partition">]
let partition (predicate : 'T -> bool) (set : HashSet<'T>) =
    let struct (accepted, rejected) = HashSet.partition predicate set
    accepted, rejected

[<CompiledName "PartitionWith">]
let partitionWith (partitioner : 'T -> Choice<'T1, 'T2>) (set : HashSet<'T>) =
    let struct (first, second) = HashSet.partitionWith partitioner set
    first, second
