[<RequireQualifiedAccess>]
module FSharp.Collections.Immutable.IQueue

open System
open System.Collections.Immutable
open System.Linq

// The interface has no members beyond Clear, Enqueue, Dequeue and Peek, so the functions walk the elements, here or in the
// walks of Queues that Queue shares, or take a snapshot of them, apply an Array function or a shared algorithm and build the
// result. A result with the element type of the input keeps the implementation of the input: it comes from Clear and
// Enqueue, or from Dequeue. A result of another element type is an ImmutableQueue. The checks of ListChecks throw what the
// List functions throw (ADR 0001, D5 and D6); the lengths that they check come from Lengths.queueUpTo.
//
// The module does not open System.Collections.Generic: its Queue<T> would shadow the Queue<'T> of this namespace.

/// A queue of the implementation of the input with the given elements: the empty queue of the input, with the elements
/// enqueued in their order.
let private rebuild (queue : IQueue<'T>) (items : seq<'T>) =
    let mutable result = queue.Clear ()

    for item in items do
        result <- result.Enqueue item

    result

// ---- Properties ----

[<CompiledName "IsEmpty">]
let isEmpty (queue : IQueue<'T>) =
    checkNotNull (nameof queue) queue
    queue.IsEmpty

[<CompiledName "Length">]
let length (queue : IQueue<'T>) =
    checkNotNull (nameof queue) queue
    let mutable count = 0

    for _ in queue do
        count <- count + 1

    count

// ---- Access ----

[<CompiledName "ExactlyOne">]
let exactlyOne (queue : IQueue<'T>) =
    checkNotNull (nameof queue) queue
    // List.exactlyOne reports its input under the name "source"
    ListChecks.exactlyOne "source" (Lengths.queueUpTo 2 queue)
    queue.Peek ()

[<CompiledName "Peek">]
let head (queue : IQueue<'T>) =
    checkNotNull (nameof queue) queue
    ListChecks.notEmpty (nameof queue) (Lengths.queueUpTo 1 queue)
    queue.Peek ()

[<CompiledName "Last">]
let last (queue : IQueue<'T>) =
    checkNotNull (nameof queue) queue
    ListChecks.notEmpty (nameof queue) (Lengths.queueUpTo 1 queue)
    Enumerable.Last queue

[<CompiledName "Dequeue">]
let tail (queue : IQueue<'T>) =
    checkNotNull (nameof queue) queue
    ListChecks.notEmpty (nameof queue) (Lengths.queueUpTo 1 queue)
    queue.Dequeue ()

[<CompiledName "TryExactlyOne">]
let tryExactlyOne (queue : IQueue<'T>) =
    checkNotNull (nameof queue) queue

    if Lengths.queueUpTo 2 queue = 1 then
        ValueSome (queue.Peek ())
    else
        ValueNone

[<CompiledName "TryPeek">]
let tryHead (queue : IQueue<'T>) =
    checkNotNull (nameof queue) queue

    if queue.IsEmpty then
        ValueNone
    else
        ValueSome (queue.Peek ())

[<CompiledName "TryLast">]
let tryLast (queue : IQueue<'T>) =
    checkNotNull (nameof queue) queue

    if queue.IsEmpty then
        ValueNone
    else
        ValueSome (Enumerable.Last queue)

[<CompiledName "TryDequeue">]
let tryTail (queue : IQueue<'T>) =
    checkNotNull (nameof queue) queue

    if queue.IsEmpty then
        ValueNone
    else
        ValueSome (queue.Dequeue ())

[<CompiledName "TryPeekDequeue">]
let tryUncons (queue : IQueue<'T>) =
    checkNotNull (nameof queue) queue

    if queue.IsEmpty then
        ValueNone
    else
        ValueSome (struct (queue.Peek (), queue.Dequeue ()))

[<CompiledName "PeekDequeue">]
let uncons (queue : IQueue<'T>) =
    checkNotNull (nameof queue) queue
    // An empty queue throws what List.head throws
    ListChecks.notEmpty (nameof queue) (Lengths.queueUpTo 1 queue)
    struct (queue.Peek (), queue.Dequeue ())

[<return : Struct>]
[<CompiledName "ConsPattern">]
let (|Cons|_|) (queue : IQueue<'T>) = tryUncons queue

[<CompiledName "NilPattern">]
let (|Nil|_|) (queue : IQueue<'T>) =
    checkNotNull (nameof queue) queue
    queue.IsEmpty

// ---- Search ----

[<CompiledName "Contains">]
let contains (value : 'T) (source : IQueue<'T>) =
    checkNotNull (nameof source) source
    // F# equality with the value on the left, as List.contains compares
    Seq.contains value source

[<CompiledName "Exists">]
let exists (predicate : 'T -> bool) (queue : IQueue<'T>) =
    checkNotNull (nameof queue) queue
    Seq.exists predicate queue

[<CompiledName "Exists2">]
let exists2 predicate (queue1 : IQueue<'T1>) (queue2 : IQueue<'T2>) =
    ListArrays.exists2 (nameof queue2) predicate (Snapshot.ofSeq (nameof queue1) queue1) (Snapshot.ofSeq (nameof queue2) queue2)

[<CompiledName "Find">]
let find (predicate : 'T -> bool) (queue : IQueue<'T>) =
    checkNotNull (nameof queue) queue

    match Queues.firstMatch predicate queue with
    | ValueSome (struct (_, item)) -> item
    | ValueNone -> Raise.indexNotFound ()

[<CompiledName "FindBack">]
let findBack (predicate : 'T -> bool) (queue : IQueue<'T>) =
    match Queues.lastMatch predicate queue with
    | ValueSome (struct (_, item)) -> item
    | ValueNone -> Raise.indexNotFound ()

[<CompiledName "FindIndex">]
let findIndex (predicate : 'T -> bool) (queue : IQueue<'T>) =
    checkNotNull (nameof queue) queue

    match Queues.firstMatch predicate queue with
    | ValueSome (struct (index, _)) -> index
    | ValueNone -> Raise.indexNotFound ()

[<CompiledName "FindIndexBack">]
let findIndexBack (predicate : 'T -> bool) (queue : IQueue<'T>) =
    match Queues.lastMatch predicate queue with
    | ValueSome (struct (index, _)) -> index
    | ValueNone -> Raise.indexNotFound ()

[<CompiledName "ForAll">]
let forall (predicate : 'T -> bool) (queue : IQueue<'T>) =
    checkNotNull (nameof queue) queue
    Seq.forall predicate queue

[<CompiledName "ForAll2">]
let forall2 predicate (queue1 : IQueue<'T1>) (queue2 : IQueue<'T2>) =
    ListArrays.forall2
        (nameof queue1)
        (nameof queue2)
        predicate
        (Snapshot.ofSeq (nameof queue1) queue1)
        (Snapshot.ofSeq (nameof queue2) queue2)

[<CompiledName "Pick">]
let pick (chooser : 'T -> 'U voption) (queue : IQueue<'T>) =
    checkNotNull (nameof queue) queue

    match Queues.firstChoice chooser queue with
    | ValueSome value -> value
    | ValueNone -> Raise.indexNotFound ()

[<CompiledName "TryFind">]
let tryFind (predicate : 'T -> bool) (queue : IQueue<'T>) =
    checkNotNull (nameof queue) queue

    match Queues.firstMatch predicate queue with
    | ValueSome (struct (_, item)) -> ValueSome item
    | ValueNone -> ValueNone

[<CompiledName "TryFindBack">]
let tryFindBack (predicate : 'T -> bool) (queue : IQueue<'T>) =
    match Queues.lastMatch predicate queue with
    | ValueSome (struct (_, item)) -> ValueSome item
    | ValueNone -> ValueNone

[<CompiledName "TryFindIndex">]
let tryFindIndex (predicate : 'T -> bool) (queue : IQueue<'T>) =
    checkNotNull (nameof queue) queue

    match Queues.firstMatch predicate queue with
    | ValueSome (struct (index, _)) -> ValueSome index
    | ValueNone -> ValueNone

[<CompiledName "TryFindIndexBack">]
let tryFindIndexBack (predicate : 'T -> bool) (queue : IQueue<'T>) =
    match Queues.lastMatch predicate queue with
    | ValueSome (struct (index, _)) -> ValueSome index
    | ValueNone -> ValueNone

[<CompiledName "TryPick">]
let tryPick (chooser : 'T -> 'U voption) (queue : IQueue<'T>) =
    checkNotNull (nameof queue) queue
    Queues.firstChoice chooser queue

// ---- Transformation ----

[<CompiledName "AllPairs">]
let allPairs (queue1 : IQueue<'T1>) (queue2 : IQueue<'T2>) =
    Arrays.allPairs structPair (Snapshot.ofSeq (nameof queue1) queue1) (Snapshot.ofSeq (nameof queue2) queue2)
    |> Queue.ofArray

[<CompiledName "Append">]
let append (queue1 : IQueue<'T>) (queue2 : IQueue<'T>) =
    checkNotNull (nameof queue1) queue1
    checkNotNull (nameof queue2) queue2
    // Enqueueing into the first queue keeps its implementation, also when it is empty
    let mutable result = queue1

    for item in queue2 do
        result <- result.Enqueue item

    result

[<CompiledName "Choose">]
let choose (chooser : 'T -> 'U voption) (queue : IQueue<'T>) =
    checkNotNull (nameof queue) queue
    let chosen = ResizeArray<'U>()

    for item in queue do
        match chooser item with
        | ValueSome value -> chosen.Add value
        | ValueNone -> ()

    Queues.ofBuffer chosen

[<CompiledName "ChunkBySize">]
let chunkBySize chunkSize (queue : IQueue<'T>) =
    checkNotNull (nameof queue) queue
    ListChecks.positive (nameof chunkSize) chunkSize

    Array.chunkBySize chunkSize (Snapshot.ofSeq (nameof queue) queue)
    |> Array.map Queue.ofArray
    |> Queue.ofArray

[<CompiledName "Collect">]
let collect (mapping : 'T -> IQueue<'U>) (queue : IQueue<'T>) =
    checkNotNull (nameof queue) queue
    let collected = ResizeArray<'U>()

    for item in queue do
        collected.AddRange (mapping item)

    Queues.ofBuffer collected

[<CompiledName "Concat">]
let concat (queues : seq<IQueue<'T>>) =
    checkNotNullOrDefaultSeq (nameof queues) queues
    let concatenated = ResizeArray<'T>()

    for queue in queues do
        // A null queue is a null input, as a null queue is for Queue.concat (ADR 0001, D6)
        checkNotNull (nameof queues) queue
        concatenated.AddRange queue

    Queues.ofBuffer concatenated

[<CompiledName "Enqueue">]
let enqueue (value : 'T) (queue : IQueue<'T>) =
    checkNotNull (nameof queue) queue
    queue.Enqueue value

[<CompiledName "EnqueueMany">]
let enqueueMany (values : seq<'T>) (queue : IQueue<'T>) =
    checkNotNullOrDefaultSeq (nameof values) values
    checkNotNull (nameof queue) queue
    let mutable result = queue

    for value in values do
        result <- result.Enqueue value

    result

[<CompiledName "Filter">]
let filter (predicate : 'T -> bool) (queue : IQueue<'T>) =
    let array = Snapshot.ofSeq (nameof queue) queue
    rebuild queue (Array.filter predicate array)

[<CompiledName "Indexed">]
let indexed (queue : IQueue<'T>) =
    Arrays.indexed structPair (Snapshot.ofSeq (nameof queue) queue)
    |> Queue.ofArray

[<CompiledName "Map">]
let map (mapping : 'T -> 'U) (queue : IQueue<'T>) =
    Array.map mapping (Snapshot.ofSeq (nameof queue) queue)
    |> Queue.ofArray

[<CompiledName "Map2">]
let map2 (mapping : 'T1 -> 'T2 -> 'U) (queue1 : IQueue<'T1>) (queue2 : IQueue<'T2>) =
    ListArrays.map2
        (nameof queue1)
        (nameof queue2)
        mapping
        (Snapshot.ofSeq (nameof queue1) queue1)
        (Snapshot.ofSeq (nameof queue2) queue2)
    |> Queue.ofArray

[<CompiledName "Map3">]
let map3 (mapping : 'T1 -> 'T2 -> 'T3 -> 'U) (queue1 : IQueue<'T1>) (queue2 : IQueue<'T2>) (queue3 : IQueue<'T3>) =
    ListArrays.map3
        (nameof queue1)
        (nameof queue2)
        (nameof queue3)
        mapping
        (Snapshot.ofSeq (nameof queue1) queue1)
        (Snapshot.ofSeq (nameof queue2) queue2)
        (Snapshot.ofSeq (nameof queue3) queue3)
    |> Queue.ofArray

[<CompiledName "MapIndexed">]
let mapi (mapping : int -> 'T -> 'U) (queue : IQueue<'T>) =
    Array.mapi mapping (Snapshot.ofSeq (nameof queue) queue)
    |> Queue.ofArray

[<CompiledName "MapIndexed2">]
let mapi2 (mapping : int -> 'T1 -> 'T2 -> 'U) (queue1 : IQueue<'T1>) (queue2 : IQueue<'T2>) =
    ListArrays.mapi2
        (nameof queue1)
        (nameof queue2)
        mapping
        (Snapshot.ofSeq (nameof queue1) queue1)
        (Snapshot.ofSeq (nameof queue2) queue2)
    |> Queue.ofArray

[<CompiledName "Pairwise">]
let pairwise (queue : IQueue<'T>) =
    Arrays.pairwise structPair (Snapshot.ofSeq (nameof queue) queue)
    |> Queue.ofArray

[<CompiledName "Permute">]
let permute indexMap (queue : IQueue<'T>) =
    // List.permute applies the algorithm of Array.permute to a copy of the list, with the same checks
    let array = Snapshot.ofSeq (nameof queue) queue
    rebuild queue (Array.permute indexMap array)

[<CompiledName "Reverse">]
let rev (queue : IQueue<'T>) =
    let array = Snapshot.ofSeq (nameof queue) queue
    rebuild queue (Array.rev array)

[<CompiledName "Transpose">]
let transpose (queues : seq<IQueue<'T>>) =
    checkNotNullOrDefaultSeq (nameof queues) queues

    let arrays =
        queues
        |> Seq.map (fun queue -> Snapshot.ofSeq (nameof queues) queue)
        |> Seq.toArray

    // List.transpose names the lists "list.[0]", "list.[1]", …; this module names them "queue.[0]", "queue.[1]", …
    // Once their lengths agree, Array.transpose agrees with List.transpose
    ListChecks.transpose "queue" (arrays |> Array.map _.Length)

    Array.transpose arrays
    |> Array.map Queue.ofArray
    |> Queue.ofArray

[<CompiledName "Unzip">]
let unzip (queue : IQueue<struct ('T1 * 'T2)>) =
    let struct (first, second) = Arrays.unzip id (Snapshot.ofSeq (nameof queue) queue)
    struct (Queue.ofArray first, Queue.ofArray second)

[<CompiledName "Unzip3">]
let unzip3 (queue : IQueue<struct ('T1 * 'T2 * 'T3)>) =
    let struct (first, second, third) = Arrays.unzip3 id (Snapshot.ofSeq (nameof queue) queue)
    struct (Queue.ofArray first, Queue.ofArray second, Queue.ofArray third)

[<CompiledName "Where">]
let where (predicate : 'T -> bool) (queue : IQueue<'T>) = filter predicate queue

[<CompiledName "Windowed">]
let windowed windowSize (queue : IQueue<'T>) =
    checkNotNull (nameof queue) queue
    ListChecks.positive (nameof windowSize) windowSize

    Array.windowed windowSize (Snapshot.ofSeq (nameof queue) queue)
    |> Array.map Queue.ofArray
    |> Queue.ofArray

[<CompiledName "Zip">]
let zip (queue1 : IQueue<'T1>) (queue2 : IQueue<'T2>) =
    let array1 = Snapshot.ofSeq (nameof queue1) queue1
    let array2 = Snapshot.ofSeq (nameof queue2) queue2
    ListChecks.sameLengths (nameof queue1) array1.Length (nameof queue2) array2.Length
    Arrays.zip structPair array1 array2 |> Queue.ofArray

[<CompiledName "Zip3">]
let zip3 (queue1 : IQueue<'T1>) (queue2 : IQueue<'T2>) (queue3 : IQueue<'T3>) =
    let array1 = Snapshot.ofSeq (nameof queue1) queue1
    let array2 = Snapshot.ofSeq (nameof queue2) queue2
    let array3 = Snapshot.ofSeq (nameof queue3) queue3
    ListChecks.zip3 (nameof queue1) array1.Length (nameof queue2) array2.Length (nameof queue3) array3.Length
    Arrays.zip3 structTriple array1 array2 array3
    |> Queue.ofArray

// ---- Folding ----

// The inline functions can use public members only, so they check the queue themselves and count its elements while
// they add them; the empty queue throws what List.average and List.averageBy throw, with the name "source"
[<CompiledName "Average">]
let inline average (queue : IQueue< ^T >) : ^T =
    if obj.ReferenceEquals (queue, null) then
        nullArg (nameof queue)

    if queue.IsEmpty then
        invalidArg "source" ErrorStrings.InputSequenceEmpty

    let mutable sum = LanguagePrimitives.GenericZero< ^T>
    let mutable count = 0

    for item in queue do
        sum <- Checked.(+) sum item
        count <- count + 1

    LanguagePrimitives.DivideByInt< ^T> sum count

[<CompiledName "AverageBy">]
let inline averageBy ([<InlineIfLambda>] projection : 'T -> ^U) (queue : IQueue<'T>) : ^U =
    if obj.ReferenceEquals (queue, null) then
        nullArg (nameof queue)

    if queue.IsEmpty then
        invalidArg "source" ErrorStrings.InputSequenceEmpty

    let mutable sum = LanguagePrimitives.GenericZero< ^U>
    let mutable count = 0

    for item in queue do
        sum <- Checked.(+) sum (projection item)
        count <- count + 1

    LanguagePrimitives.DivideByInt< ^U> sum count

[<CompiledName "CompareWith">]
let compareWith (comparer : 'T -> 'T -> int) (queue1 : IQueue<'T>) (queue2 : IQueue<'T>) =
    // Array.compareWith compares as List.compareWith does: the first difference, then the lengths
    Array.compareWith comparer (Snapshot.ofSeq (nameof queue1) queue1) (Snapshot.ofSeq (nameof queue2) queue2)

[<CompiledName "Fold">]
let fold (folder : 'State -> 'T -> 'State) (state : 'State) (queue : IQueue<'T>) =
    checkNotNull (nameof queue) queue
    let mutable state = state

    for item in queue do
        state <- folder state item

    state

[<CompiledName "Fold2">]
let fold2 (folder : 'State -> 'T1 -> 'T2 -> 'State) (state : 'State) (queue1 : IQueue<'T1>) (queue2 : IQueue<'T2>) =
    ListArrays.fold2
        (nameof queue1)
        (nameof queue2)
        folder
        state
        (Snapshot.ofSeq (nameof queue1) queue1)
        (Snapshot.ofSeq (nameof queue2) queue2)

[<CompiledName "FoldBack">]
let foldBack (folder : 'T -> 'State -> 'State) (queue : IQueue<'T>) (state : 'State) =
    Array.foldBack folder (Snapshot.ofSeq (nameof queue) queue) state

[<CompiledName "FoldBack2">]
let foldBack2 (folder : 'T1 -> 'T2 -> 'State -> 'State) (queue1 : IQueue<'T1>) (queue2 : IQueue<'T2>) (state : 'State) =
    let array1 = Snapshot.ofSeq (nameof queue1) queue1
    let array2 = Snapshot.ofSeq (nameof queue2) queue2
    // List.foldBack2 checks the lengths before it calls the folder
    ListChecks.foldBack2 (nameof queue1) array1.Length (nameof queue2) array2.Length
    Array.foldBack2 folder array1 array2 state

[<CompiledName "Iterate">]
let iter (action : 'T -> unit) (queue : IQueue<'T>) =
    checkNotNull (nameof queue) queue

    for item in queue do
        action item

[<CompiledName "Iterate2">]
let iter2 (action : 'T1 -> 'T2 -> unit) (queue1 : IQueue<'T1>) (queue2 : IQueue<'T2>) =
    ListArrays.iter2
        (nameof queue1)
        (nameof queue2)
        action
        (Snapshot.ofSeq (nameof queue1) queue1)
        (Snapshot.ofSeq (nameof queue2) queue2)

[<CompiledName "IterateIndexed">]
let iteri (action : int -> 'T -> unit) (queue : IQueue<'T>) =
    checkNotNull (nameof queue) queue
    let mutable index = 0

    for item in queue do
        action index item
        index <- index + 1

[<CompiledName "IterateIndexed2">]
let iteri2 (action : int -> 'T1 -> 'T2 -> unit) (queue1 : IQueue<'T1>) (queue2 : IQueue<'T2>) =
    ListArrays.iteri2
        (nameof queue1)
        (nameof queue2)
        action
        (Snapshot.ofSeq (nameof queue1) queue1)
        (Snapshot.ofSeq (nameof queue2) queue2)

[<CompiledName "MapFold">]
let mapFold (mapping : 'State -> 'T -> struct ('Result * 'State)) (state : 'State) (queue : IQueue<'T>) =
    let source = Snapshot.ofSeq (nameof queue) queue
    let results = Array.zeroCreate source.Length
    let mutable state = state

    for index = 0 to source.Length - 1 do
        let struct (result, next) = mapping state source[index]
        results[index] <- result
        state <- next

    struct (Queue.ofArray results, state)

[<CompiledName "MapFoldBack">]
let mapFoldBack (mapping : 'T -> 'State -> struct ('Result * 'State)) (queue : IQueue<'T>) (state : 'State) =
    let source = Snapshot.ofSeq (nameof queue) queue
    let results = Array.zeroCreate source.Length
    let mutable state = state

    for index = source.Length - 1 downto 0 do
        let struct (result, next) = mapping source[index] state
        results[index] <- result
        state <- next

    struct (Queue.ofArray results, state)

// Seq.max and the others compare as the List functions do once the queue has elements, so the List checks come first

[<CompiledName "Max">]
let max (queue : IQueue<'T>) =
    checkNotNull (nameof queue) queue
    ListChecks.notEmptySequence (nameof queue) (Lengths.queueUpTo 1 queue)
    Seq.max queue

[<CompiledName "MaxBy">]
let maxBy (projection : 'T -> 'U) (queue : IQueue<'T>) =
    checkNotNull (nameof queue) queue
    ListChecks.notEmptySequence (nameof queue) (Lengths.queueUpTo 1 queue)
    Seq.maxBy projection queue

[<CompiledName "Min">]
let min (queue : IQueue<'T>) =
    checkNotNull (nameof queue) queue
    ListChecks.notEmptySequence (nameof queue) (Lengths.queueUpTo 1 queue)
    Seq.min queue

[<CompiledName "MinBy">]
let minBy (projection : 'T -> 'U) (queue : IQueue<'T>) =
    checkNotNull (nameof queue) queue
    ListChecks.notEmptySequence (nameof queue) (Lengths.queueUpTo 1 queue)
    Seq.minBy projection queue

[<CompiledName "Reduce">]
let reduce (reduction : 'T -> 'T -> 'T) (queue : IQueue<'T>) =
    checkNotNull (nameof queue) queue
    ListChecks.notEmpty (nameof queue) (Lengths.queueUpTo 1 queue)
    Seq.reduce reduction queue

[<CompiledName "ReduceBack">]
let reduceBack (reduction : 'T -> 'T -> 'T) (queue : IQueue<'T>) =
    checkNotNull (nameof queue) queue
    ListChecks.notEmpty (nameof queue) (Lengths.queueUpTo 1 queue)
    Array.reduceBack reduction (Snapshot.ofSeq (nameof queue) queue)

[<CompiledName "Scan">]
let scan (folder : 'State -> 'T -> 'State) (state : 'State) (queue : IQueue<'T>) =
    Array.scan folder state (Snapshot.ofSeq (nameof queue) queue)
    |> Queue.ofArray

[<CompiledName "ScanBack">]
let scanBack (folder : 'T -> 'State -> 'State) (queue : IQueue<'T>) (state : 'State) =
    Array.scanBack folder (Snapshot.ofSeq (nameof queue) queue) state
    |> Queue.ofArray

[<CompiledName "Sum">]
let inline sum (queue : IQueue< ^T >) : ^T =
    if obj.ReferenceEquals (queue, null) then
        nullArg (nameof queue)

    let mutable sum = LanguagePrimitives.GenericZero< ^T>

    for item in queue do
        sum <- Checked.(+) sum item

    sum

[<CompiledName "SumBy">]
let inline sumBy ([<InlineIfLambda>] projection : 'T -> ^U) (queue : IQueue<'T>) : ^U =
    if obj.ReferenceEquals (queue, null) then
        nullArg (nameof queue)

    let mutable sum = LanguagePrimitives.GenericZero< ^U>

    for item in queue do
        sum <- Checked.(+) sum (projection item)

    sum

// ---- Partitioning and grouping ----

[<CompiledName "CountBy">]
let countBy (projection : 'T -> 'Key) (queue : IQueue<'T>) =
    Arrays.countBy structPair projection (Snapshot.ofSeq (nameof queue) queue)
    |> Queue.ofArray

[<CompiledName "Distinct">]
let distinct (queue : IQueue<'T>) =
    let array = Snapshot.ofSeq (nameof queue) queue
    rebuild queue (Array.distinct array)

[<CompiledName "DistinctBy">]
let distinctBy (projection : 'T -> 'Key) (queue : IQueue<'T>) =
    let array = Snapshot.ofSeq (nameof queue) queue
    rebuild queue (Array.distinctBy projection array)

[<CompiledName "Except">]
let except (itemsToExclude : seq<'T>) (queue : IQueue<'T>) =
    // List.except checks the items to exclude first and does not read them for an empty list
    checkNotNullOrDefaultSeq (nameof itemsToExclude) itemsToExclude
    checkNotNull (nameof queue) queue

    if queue.IsEmpty then
        queue
    else
        rebuild queue (Array.except itemsToExclude (Snapshot.ofSeq (nameof queue) queue))

[<CompiledName "GroupBy">]
let groupBy (projection : 'T -> 'Key) (queue : IQueue<'T>) =
    Arrays.groupBy (fun key items -> struct (key, Queue.ofArray items)) projection (Snapshot.ofSeq (nameof queue) queue)
    |> Queue.ofArray

[<CompiledName "Partition">]
let partition (predicate : 'T -> bool) (queue : IQueue<'T>) =
    let accepted, rejected = Array.partition predicate (Snapshot.ofSeq (nameof queue) queue)
    struct (rebuild queue accepted, rebuild queue rejected)

[<CompiledName "PartitionWith">]
let partitionWith (partitioner : 'T -> Choice<'T1, 'T2>) (queue : IQueue<'T>) =
    let first, second = Array.partitionWith partitioner (Snapshot.ofSeq (nameof queue) queue)
    struct (Queue.ofArray first, Queue.ofArray second)

[<CompiledName "Skip">]
let skip count (queue : IQueue<'T>) =
    checkNotNull (nameof queue) queue
    let mutable rest = queue
    let mutable skipped = 0

    while skipped < count && not rest.IsEmpty do
        rest <- rest.Dequeue ()
        skipped <- skipped + 1

    // The loop stops at the count or at the end of the queue, where the number of skipped elements is the length; a
    // count beyond the length throws what List.skip throws
    ListChecks.skip (nameof count) count skipped
    rest

[<CompiledName "SkipLast">]
let skipLast count (queue : IQueue<'T>) =
    let array = Snapshot.ofSeq (nameof queue) queue
    // The checks of List.skip
    ListChecks.skip (nameof count) count array.Length

    if count <= 0 then
        queue
    else
        rebuild queue (ArraySegment<'T>(array, 0, array.Length - count))

[<CompiledName "SkipWhile">]
let skipWhile (predicate : 'T -> bool) (queue : IQueue<'T>) =
    checkNotNull (nameof queue) queue
    let mutable rest = queue

    while not rest.IsEmpty && predicate (rest.Peek ()) do
        rest <- rest.Dequeue ()

    rest

[<CompiledName "SplitAt">]
let splitAt index (queue : IQueue<'T>) =
    checkNotNull (nameof queue) queue
    let before = ResizeArray<'T>()
    let mutable rest = queue

    while before.Count < index && not rest.IsEmpty do
        before.Add (rest.Peek ())
        rest <- rest.Dequeue ()

    // The loop stops at the index or at the end of the queue, where the number of taken elements is the length
    ListChecks.splitAt (nameof index) index before.Count
    struct ((if rest.IsEmpty then queue else rebuild queue before), rest)

[<CompiledName "SplitInto">]
let splitInto count (queue : IQueue<'T>) =
    checkNotNull (nameof queue) queue
    ListChecks.positive (nameof count) count

    Array.splitInto count (Snapshot.ofSeq (nameof queue) queue)
    |> Array.map Queue.ofArray
    |> Queue.ofArray

[<CompiledName "Take">]
let take count (queue : IQueue<'T>) =
    checkNotNull (nameof queue) queue
    let struct (items, more) = Queues.front count queue
    // The front stops at the count or at the end of the queue, where the number of taken elements is the length
    ListChecks.take (nameof count) count items.Count
    if more then rebuild queue items else queue

[<CompiledName "TakeLast">]
let takeLast count (queue : IQueue<'T>) =
    let array = Snapshot.ofSeq (nameof queue) queue
    // The checks of List.take
    ListChecks.take (nameof count) count array.Length

    if count = array.Length then
        queue
    else
        rebuild queue (ArraySegment<'T>(array, array.Length - count, count))

[<CompiledName "TakeWhile">]
let takeWhile (predicate : 'T -> bool) (queue : IQueue<'T>) =
    checkNotNull (nameof queue) queue
    let items = ResizeArray<'T>()
    use enumerator = queue.GetEnumerator ()
    let mutable more = enumerator.MoveNext ()

    while more && predicate enumerator.Current do
        items.Add enumerator.Current
        more <- enumerator.MoveNext ()

    if more then rebuild queue items else queue

[<CompiledName "Truncate">]
let truncate count (queue : IQueue<'T>) =
    checkNotNull (nameof queue) queue
    let struct (items, more) = Queues.front count queue
    if more then rebuild queue items else queue

// ---- Sorting ----

// The sorts are stable, as the sorts of List are (ADR 0001, D5). The functions that compare the elements, or that call the
// projection in the comparison as List.sortByDescending does, sort with the Seq functions of FSharp.Core, which share the
// stable sort of List: the same comparisons run in Array.Sort, which wraps the exception of a comparison in an
// InvalidOperationException (D6). Seq.sortDescending and Seq.sortByDescending are inline and box the elements in every
// comparison once they are compiled into a generic function, so the two descending sorts call Seq.sortWith through
// DescendingSorts, with the cached comparer. sortBy and sortWithComparer sort with LINQ, which computes the keys first, as
// List.sortBy does, and wraps the exception of a comparison as Array.Sort does. A queue of one element is returned
// without a call of the projection or the comparer, as List returns a list of one element.

[<CompiledName "Sort">]
let sort (queue : IQueue<'T>) =
    checkNotNull (nameof queue) queue

    if Lengths.queueUpTo 2 queue <= 1 then
        queue
    else
        rebuild queue (Seq.sort queue)

[<CompiledName "SortBy">]
let sortBy (projection : 'T -> 'Key) (queue : IQueue<'T>) =
    checkNotNull (nameof queue) queue

    if Lengths.queueUpTo 2 queue <= 1 then
        queue
    else
        rebuild queue (queue.OrderBy(Func<'T, 'Key> projection, DefaultComparer<'Key>.Instance).ToArray())

[<CompiledName "SortByDescending">]
let sortByDescending (projection : 'T -> 'Key) (queue : IQueue<'T>) =
    checkNotNull (nameof queue) queue

    if Lengths.queueUpTo 2 queue <= 1 then
        queue
    else
        rebuild queue (DescendingSorts.sortByDescending projection queue)

[<CompiledName "SortDescending">]
let sortDescending (queue : IQueue<'T>) =
    checkNotNull (nameof queue) queue

    if Lengths.queueUpTo 2 queue <= 1 then
        queue
    else
        rebuild queue (DescendingSorts.sortDescending queue)

[<CompiledName "SortWith">]
let sortWith (comparer : 'T -> 'T -> int) (queue : IQueue<'T>) =
    checkNotNull (nameof queue) queue

    if Lengths.queueUpTo 2 queue <= 1 then
        queue
    else
        rebuild queue (Seq.sortWith comparer queue)

[<CompiledName "SortWithComparer">]
let sortWithComparer (comparer : System.Collections.Generic.IComparer<'T>) (queue : IQueue<'T>) =
    checkNotNull (nameof queue) queue

    if Lengths.queueUpTo 2 queue <= 1 then
        queue
    else
        rebuild queue (queue.Order(comparer).ToArray())

// ---- Random ----

// The functions draw the random numbers as the List functions do: they take a snapshot, run the List checks with its
// length and choose from it, or apply the Array function to it

[<CompiledName "RandomChoice">]
let randomChoice (source : IQueue<'T>) =
    let array = Snapshot.ofSeq (nameof source) source
    ListChecks.notEmptySequence (nameof source) array.Length
    array[Random.Shared.Next (0, array.Length)]

[<CompiledName "RandomChoiceBy">]
let randomChoiceBy (randomizer : unit -> float) (source : IQueue<'T>) =
    let array = Snapshot.ofSeq (nameof source) source
    ListChecks.notEmptySequence (nameof source) array.Length
    array[ListChecks.randomIndex (nameof randomizer) randomizer array.Length]

[<CompiledName "RandomChoiceWith">]
let randomChoiceWith (random : Random) (source : IQueue<'T>) =
    checkNotNull (nameof random) random
    let array = Snapshot.ofSeq (nameof source) source
    ListChecks.notEmptySequence (nameof source) array.Length
    array[random.Next (0, array.Length)]

[<CompiledName "RandomChoices">]
let randomChoices count (source : IQueue<'T>) =
    let array = Snapshot.ofSeq (nameof source) source
    ListChecks.randomChoices (nameof count) count (nameof source) array.Length
    rebuild source (Array.randomChoices count array)

[<CompiledName "RandomChoicesBy">]
let randomChoicesBy (randomizer : unit -> float) count (source : IQueue<'T>) =
    let array = Snapshot.ofSeq (nameof source) source
    ListChecks.randomChoices (nameof count) count (nameof source) array.Length
    rebuild source (Array.randomChoicesBy randomizer count array)

[<CompiledName "RandomChoicesWith">]
let randomChoicesWith (random : Random) count (source : IQueue<'T>) =
    checkNotNull (nameof random) random
    let array = Snapshot.ofSeq (nameof source) source
    ListChecks.randomChoices (nameof count) count (nameof source) array.Length
    rebuild source (Array.randomChoicesWith random count array)

[<CompiledName "RandomSample">]
let randomSample count (source : IQueue<'T>) =
    let array = Snapshot.ofSeq (nameof source) source
    ListChecks.randomSample (nameof count) count (nameof source) array.Length
    rebuild source (Array.randomSample count array)

[<CompiledName "RandomSampleBy">]
let randomSampleBy (randomizer : unit -> float) count (source : IQueue<'T>) =
    let array = Snapshot.ofSeq (nameof source) source
    ListChecks.randomSample (nameof count) count (nameof source) array.Length
    rebuild source (Array.randomSampleBy randomizer count array)

[<CompiledName "RandomSampleWith">]
let randomSampleWith (random : Random) count (source : IQueue<'T>) =
    checkNotNull (nameof random) random
    let array = Snapshot.ofSeq (nameof source) source
    ListChecks.randomSample (nameof count) count (nameof source) array.Length
    rebuild source (Array.randomSampleWith random count array)

[<CompiledName "RandomShuffle">]
let randomShuffle (source : IQueue<'T>) =
    let array = Snapshot.ofSeq (nameof source) source
    Array.randomShuffleInPlace array
    rebuild source array

[<CompiledName "RandomShuffleBy">]
let randomShuffleBy (randomizer : unit -> float) (source : IQueue<'T>) =
    let array = Snapshot.ofSeq (nameof source) source
    Array.randomShuffleInPlaceBy randomizer array
    rebuild source array

[<CompiledName "RandomShuffleWith">]
let randomShuffleWith (random : Random) (source : IQueue<'T>) =
    checkNotNull (nameof random) random
    let array = Snapshot.ofSeq (nameof source) source
    Array.randomShuffleInPlaceWith random array
    rebuild source array

// ---- Conversion ----

[<CompiledName "ToArray">]
let toArray (queue : IQueue<'T>) = Snapshot.ofSeq (nameof queue) queue

[<CompiledName "ToList">]
let toList (queue : IQueue<'T>) =
    checkNotNull (nameof queue) queue
    List.ofSeq queue

[<CompiledName "ToSeq">]
let toSeq (queue : IQueue<'T>) =
    checkNotNull (nameof queue) queue
    queue :> seq<'T>

// ---- Type-specific ----

[<CompiledName "Clear">]
let clear (queue : IQueue<'T>) =
    checkNotNull (nameof queue) queue
    queue.Clear ()
