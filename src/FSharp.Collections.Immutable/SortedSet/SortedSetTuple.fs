[<RequireQualifiedAccess>]
module FSharp.Collections.Immutable.Tuple.SortedSet

open FSharp.Collections.Immutable

// The functions delegate to the default module and repack its struct tuples.

// ---- Partitioning ----

[<CompiledName "Partition">]
let partition (predicate : 'T -> bool) (set : SortedSet<'T>) =
    let struct (accepted, rejected) = FSharp.Collections.Immutable.SortedSet.partition predicate set
    accepted, rejected

[<CompiledName "PartitionWith">]
let partitionWith (partitioner : 'T -> Choice<'T1, 'T2>) (set : SortedSet<'T>) =
    let struct (first, second) = FSharp.Collections.Immutable.SortedSet.partitionWith partitioner set
    first, second
