[<RequireQualifiedAccess>]
module FSharp.Collections.Immutable.Tuple.FrozenSet

open System.Collections.Frozen

// The functions delegate to the default module and repack its struct tuples.

// ---- Partitioning ----

[<CompiledName "Partition">]
let partition (predicate : 'T -> bool) (set : FrozenSet<'T>) =
    let struct (accepted, rejected) = FSharp.Collections.Immutable.FrozenSet.partition predicate set
    accepted, rejected

[<CompiledName "PartitionWith">]
let partitionWith (partitioner : 'T -> Choice<'T1, 'T2>) (set : FrozenSet<'T>) =
    let struct (first, second) = FSharp.Collections.Immutable.FrozenSet.partitionWith partitioner set
    first, second
