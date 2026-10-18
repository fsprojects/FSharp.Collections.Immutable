[<RequireQualifiedAccess>]
module FSharp.Collections.Immutable.Tuple.IQueue

open FSharp.Collections.Immutable

// The functions that build a tuple per element share their algorithm with the default module and only pass another
// tuple constructor; the others delegate to the default module and repack the tuples, which costs no second pass.

// ---- Access ----

[<CompiledName "TryPeekDequeue">]
let tryUncons (queue : IQueue<'T>) =
    match FSharp.Collections.Immutable.IQueue.tryUncons queue with
    | ValueSome (struct (head, tail)) -> ValueSome (head, tail)
    | ValueNone -> ValueNone

[<CompiledName "PeekDequeue">]
let uncons (queue : IQueue<'T>) =
    let struct (head, tail) = FSharp.Collections.Immutable.IQueue.uncons queue
    head, tail

[<return : Struct>]
[<CompiledName "ConsPattern">]
let (|Cons|_|) (queue : IQueue<'T>) = tryUncons queue

// ---- Transformation ----

[<CompiledName "AllPairs">]
let allPairs (queue1 : IQueue<'T1>) (queue2 : IQueue<'T2>) =
    Arrays.allPairs referencePair (Snapshot.ofSeq (nameof queue1) queue1) (Snapshot.ofSeq (nameof queue2) queue2)
    |> FSharp.Collections.Immutable.Queue.ofArray

[<CompiledName "Indexed">]
let indexed (queue : IQueue<'T>) =
    Arrays.indexed referencePair (Snapshot.ofSeq (nameof queue) queue)
    |> FSharp.Collections.Immutable.Queue.ofArray

[<CompiledName "Pairwise">]
let pairwise (queue : IQueue<'T>) =
    Arrays.pairwise referencePair (Snapshot.ofSeq (nameof queue) queue)
    |> FSharp.Collections.Immutable.Queue.ofArray

[<CompiledName "Unzip">]
let unzip (queue : IQueue<'T1 * 'T2>) =
    let struct (first, second) =
        Arrays.unzip (fun (item1, item2) -> struct (item1, item2)) (Snapshot.ofSeq (nameof queue) queue)

    FSharp.Collections.Immutable.Queue.ofArray first, FSharp.Collections.Immutable.Queue.ofArray second

[<CompiledName "Unzip3">]
let unzip3 (queue : IQueue<'T1 * 'T2 * 'T3>) =
    let struct (first, second, third) =
        Arrays.unzip3 (fun (item1, item2, item3) -> struct (item1, item2, item3)) (Snapshot.ofSeq (nameof queue) queue)

    FSharp.Collections.Immutable.Queue.ofArray first,
    FSharp.Collections.Immutable.Queue.ofArray second,
    FSharp.Collections.Immutable.Queue.ofArray third

[<CompiledName "Zip">]
let zip (queue1 : IQueue<'T1>) (queue2 : IQueue<'T2>) =
    let array1 = Snapshot.ofSeq (nameof queue1) queue1
    let array2 = Snapshot.ofSeq (nameof queue2) queue2
    ListChecks.sameLengths (nameof queue1) array1.Length (nameof queue2) array2.Length

    Arrays.zip referencePair array1 array2
    |> FSharp.Collections.Immutable.Queue.ofArray

[<CompiledName "Zip3">]
let zip3 (queue1 : IQueue<'T1>) (queue2 : IQueue<'T2>) (queue3 : IQueue<'T3>) =
    let array1 = Snapshot.ofSeq (nameof queue1) queue1
    let array2 = Snapshot.ofSeq (nameof queue2) queue2
    let array3 = Snapshot.ofSeq (nameof queue3) queue3
    ListChecks.zip3 (nameof queue1) array1.Length (nameof queue2) array2.Length (nameof queue3) array3.Length

    Arrays.zip3 referenceTriple array1 array2 array3
    |> FSharp.Collections.Immutable.Queue.ofArray

// ---- Folding ----

[<CompiledName "MapFold">]
let mapFold (mapping : 'State -> 'T -> 'Result * 'State) (state : 'State) (queue : IQueue<'T>) =
    let mapStruct state item =
        let result, next = mapping state item
        struct (result, next)

    let struct (results, final) = FSharp.Collections.Immutable.IQueue.mapFold mapStruct state queue
    results, final

[<CompiledName "MapFoldBack">]
let mapFoldBack (mapping : 'T -> 'State -> 'Result * 'State) (queue : IQueue<'T>) (state : 'State) =
    let mapStruct item state =
        let result, next = mapping item state
        struct (result, next)

    let struct (results, final) = FSharp.Collections.Immutable.IQueue.mapFoldBack mapStruct queue state
    results, final

// ---- Partitioning and grouping ----

[<CompiledName "CountBy">]
let countBy (projection : 'T -> 'Key) (queue : IQueue<'T>) =
    Arrays.countBy referencePair projection (Snapshot.ofSeq (nameof queue) queue)
    |> FSharp.Collections.Immutable.Queue.ofArray

[<CompiledName "GroupBy">]
let groupBy (projection : 'T -> 'Key) (queue : IQueue<'T>) =
    Arrays.groupBy
        (fun key items -> key, FSharp.Collections.Immutable.Queue.ofArray items)
        projection
        (Snapshot.ofSeq (nameof queue) queue)
    |> FSharp.Collections.Immutable.Queue.ofArray

[<CompiledName "Partition">]
let partition predicate (queue : IQueue<'T>) =
    let struct (accepted, rejected) = FSharp.Collections.Immutable.IQueue.partition predicate queue
    accepted, rejected

[<CompiledName "PartitionWith">]
let partitionWith (partitioner : 'T -> Choice<'T1, 'T2>) (queue : IQueue<'T>) =
    let struct (first, second) = FSharp.Collections.Immutable.IQueue.partitionWith partitioner queue
    first, second

[<CompiledName "SplitAt">]
let splitAt index (queue : IQueue<'T>) =
    let struct (before, after) = FSharp.Collections.Immutable.IQueue.splitAt index queue
    before, after
