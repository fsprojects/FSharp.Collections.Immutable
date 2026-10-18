[<RequireQualifiedAccess>]
module FSharp.Collections.Immutable.Tuple.Queue

open FSharp.Collections.Immutable

// The functions that build a tuple per element share their algorithm with the default module and only pass another
// tuple constructor; the others delegate to the default module and repack the tuples, which costs no second pass.

// ---- Creation ----

[<CompiledName "Unfold">]
let unfold (generator : 'State -> ('T * 'State) voption) (state : 'State) =
    let generateStruct state =
        match generator state with
        | ValueSome (item, next) -> ValueSome (struct (item, next))
        | ValueNone -> ValueNone

    FSharp.Collections.Immutable.Queue.unfold generateStruct state

// ---- Access ----

[<CompiledName "TryPeekDequeue">]
let tryUncons (queue : Queue<'T>) =
    match FSharp.Collections.Immutable.Queue.tryUncons queue with
    | ValueSome (struct (head, tail)) -> ValueSome (head, tail)
    | ValueNone -> ValueNone

[<CompiledName "PeekDequeue">]
let uncons (queue : Queue<'T>) =
    let struct (head, tail) = FSharp.Collections.Immutable.Queue.uncons queue
    head, tail

[<return : Struct>]
[<CompiledName "ConsPattern">]
let (|Cons|_|) (queue : Queue<'T>) = tryUncons queue

// ---- Transformation ----

[<CompiledName "AllPairs">]
let allPairs (queue1 : Queue<'T1>) (queue2 : Queue<'T2>) =
    Arrays.allPairs referencePair (Snapshot.ofSeq (nameof queue1) queue1) (Snapshot.ofSeq (nameof queue2) queue2)
    |> Queues.ofFreshArray

[<CompiledName "Indexed">]
let indexed (queue : Queue<'T>) =
    Arrays.indexed referencePair (Snapshot.ofSeq (nameof queue) queue)
    |> Queues.ofFreshArray

[<CompiledName "Pairwise">]
let pairwise (queue : Queue<'T>) =
    Arrays.pairwise referencePair (Snapshot.ofSeq (nameof queue) queue)
    |> Queues.ofFreshArray

[<CompiledName "Unzip">]
let unzip (queue : Queue<'T1 * 'T2>) =
    let struct (first, second) =
        Arrays.unzip (fun (item1, item2) -> struct (item1, item2)) (Snapshot.ofSeq (nameof queue) queue)

    Queues.ofFreshArray first, Queues.ofFreshArray second

[<CompiledName "Unzip3">]
let unzip3 (queue : Queue<'T1 * 'T2 * 'T3>) =
    let struct (first, second, third) =
        Arrays.unzip3 (fun (item1, item2, item3) -> struct (item1, item2, item3)) (Snapshot.ofSeq (nameof queue) queue)

    Queues.ofFreshArray first, Queues.ofFreshArray second, Queues.ofFreshArray third

[<CompiledName "Zip">]
let zip (queue1 : Queue<'T1>) (queue2 : Queue<'T2>) =
    let array1 = Snapshot.ofSeq (nameof queue1) queue1
    let array2 = Snapshot.ofSeq (nameof queue2) queue2
    ListChecks.sameLengths (nameof queue1) array1.Length (nameof queue2) array2.Length
    Arrays.zip referencePair array1 array2
    |> Queues.ofFreshArray

[<CompiledName "Zip3">]
let zip3 (queue1 : Queue<'T1>) (queue2 : Queue<'T2>) (queue3 : Queue<'T3>) =
    let array1 = Snapshot.ofSeq (nameof queue1) queue1
    let array2 = Snapshot.ofSeq (nameof queue2) queue2
    let array3 = Snapshot.ofSeq (nameof queue3) queue3
    ListChecks.zip3 (nameof queue1) array1.Length (nameof queue2) array2.Length (nameof queue3) array3.Length
    Arrays.zip3 referenceTriple array1 array2 array3
    |> Queues.ofFreshArray

// ---- Folding ----

[<CompiledName "MapFold">]
let mapFold (mapping : 'State -> 'T -> 'Result * 'State) (state : 'State) (queue : Queue<'T>) =
    let mapStruct state item =
        let result, next = mapping state item
        struct (result, next)

    let struct (results, final) = FSharp.Collections.Immutable.Queue.mapFold mapStruct state queue
    results, final

[<CompiledName "MapFoldBack">]
let mapFoldBack (mapping : 'T -> 'State -> 'Result * 'State) (queue : Queue<'T>) (state : 'State) =
    let mapStruct item state =
        let result, next = mapping item state
        struct (result, next)

    let struct (results, final) = FSharp.Collections.Immutable.Queue.mapFoldBack mapStruct queue state
    results, final

// ---- Partitioning and grouping ----

[<CompiledName "CountBy">]
let countBy (projection : 'T -> 'Key) (queue : Queue<'T>) =
    Arrays.countBy referencePair projection (Snapshot.ofSeq (nameof queue) queue)
    |> Queues.ofFreshArray

[<CompiledName "GroupBy">]
let groupBy (projection : 'T -> 'Key) (queue : Queue<'T>) =
    Arrays.groupBy (fun key items -> key, Queues.ofFreshArray items) projection (Snapshot.ofSeq (nameof queue) queue)
    |> Queues.ofFreshArray

[<CompiledName "Partition">]
let partition predicate (queue : Queue<'T>) =
    let struct (accepted, rejected) = FSharp.Collections.Immutable.Queue.partition predicate queue
    accepted, rejected

[<CompiledName "PartitionWith">]
let partitionWith (partitioner : 'T -> Choice<'T1, 'T2>) (queue : Queue<'T>) =
    let struct (first, second) = FSharp.Collections.Immutable.Queue.partitionWith partitioner queue
    first, second

[<CompiledName "SplitAt">]
let splitAt index (queue : Queue<'T>) =
    let struct (before, after) = FSharp.Collections.Immutable.Queue.splitAt index queue
    before, after
