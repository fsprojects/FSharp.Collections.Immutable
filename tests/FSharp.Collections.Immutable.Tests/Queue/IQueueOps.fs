/// The functions of the `IQueue` module for the contract tests, over queues seen through the interface.
[<RequireQualifiedAccess>]
module FSharp.Collections.Immutable.Tests.IQueueOps

open FSharp.Collections.Immutable

/// A queue of the elements, seen through the interface.
let private ofSeq (source : seq<'T>) : IQueue<'T> = Queue.ofSeq source

let private toArrays (queues : Queue<Queue<int>>) = queues |> Queue.toArray |> Array.map Queue.toArray

let private toArrayPair (struct (first : Queue<'T1>, second : Queue<'T2>)) = struct (Queue.toArray first, Queue.toArray second)

/// The functions of the sequence-like facet. The functions that change the element type return a queue, which the
/// instance sees through the interface again where the record expects the collection of the module.
let seqLike : SeqOps<IQueue<int>> = {
    Name = nameof IQueue
    Mirror = ListMirror
    ParamName = QueueOps.paramName
    OfSeq = ofSeq
    ToArray = IQueue.toArray

    IsEmpty = IQueue.isEmpty
    Length = IQueue.length

    ExactlyOne = IQueue.exactlyOne
    Head = IQueue.head
    Last = IQueue.last
    Tail = IQueue.tail
    TryExactlyOne = IQueue.tryExactlyOne
    TryHead = IQueue.tryHead
    TryLast = IQueue.tryLast
    TryTail = IQueue.tryTail

    Contains = IQueue.contains
    Exists = IQueue.exists
    Exists2 = IQueue.exists2
    Find = IQueue.find
    FindBack = IQueue.findBack
    FindIndex = IQueue.findIndex
    FindIndexBack = IQueue.findIndexBack
    Forall = IQueue.forall
    Forall2 = IQueue.forall2
    Pick = IQueue.pick
    TryFind = IQueue.tryFind
    TryFindBack = IQueue.tryFindBack
    TryFindIndex = IQueue.tryFindIndex
    TryFindIndexBack = IQueue.tryFindIndexBack
    TryPick = IQueue.tryPick

    AllPairs = fun queue1 queue2 -> IQueue.allPairs queue1 queue2 |> Queue.toArray
    Append = IQueue.append
    Choose = fun chooser queue -> IQueue.choose chooser queue
    ChunkBySize = fun chunkSize queue -> IQueue.chunkBySize chunkSize queue |> toArrays
    Collect = fun mapping queue -> IQueue.collect mapping queue
    Concat = fun queues -> IQueue.concat queues
    Filter = IQueue.filter
    Indexed = IQueue.indexed >> Queue.toArray
    Map = fun mapping queue -> IQueue.map mapping queue
    Map2 = fun mapping queue1 queue2 -> IQueue.map2 mapping queue1 queue2
    Map3 = fun mapping queue1 queue2 queue3 -> IQueue.map3 mapping queue1 queue2 queue3
    Mapi = fun mapping queue -> IQueue.mapi mapping queue
    Mapi2 = fun mapping queue1 queue2 -> IQueue.mapi2 mapping queue1 queue2
    Pairwise = IQueue.pairwise >> Queue.toArray
    Permute = IQueue.permute
    Rev = IQueue.rev
    Transpose = fun queues -> IQueue.transpose queues |> toArrays
    Unzip = fun pairs -> IQueue.unzip (ofSeq pairs) |> toArrayPair
    Unzip3 =
        fun triples ->
            let struct (first, second, third) = IQueue.unzip3 (ofSeq triples)
            struct (Queue.toArray first, Queue.toArray second, Queue.toArray third)
    Where = IQueue.where
    Windowed = fun windowSize queue -> IQueue.windowed windowSize queue |> toArrays
    Zip = fun queue1 queue2 -> IQueue.zip queue1 queue2 |> Queue.toArray
    Zip3 = fun queue1 queue2 queue3 -> IQueue.zip3 queue1 queue2 queue3 |> Queue.toArray

    Average = fun items -> IQueue.average (ofSeq items)
    AverageBy = IQueue.averageBy
    CompareWith = IQueue.compareWith
    Fold = IQueue.fold
    Fold2 = IQueue.fold2
    FoldBack = IQueue.foldBack
    FoldBack2 = IQueue.foldBack2
    Iter = IQueue.iter
    Iter2 = IQueue.iter2
    Iteri = IQueue.iteri
    Iteri2 = IQueue.iteri2
    MapFold =
        fun mapping state queue ->
            let struct (results, final) = IQueue.mapFold mapping state queue
            struct (Queue.toArray results, final)
    MapFoldBack =
        fun mapping queue state ->
            let struct (results, final) = IQueue.mapFoldBack mapping queue state
            struct (Queue.toArray results, final)
    Max = IQueue.max
    MaxBy = IQueue.maxBy
    Min = IQueue.min
    MinBy = IQueue.minBy
    Reduce = IQueue.reduce
    ReduceBack = IQueue.reduceBack
    Scan = fun folder state queue -> IQueue.scan folder state queue |> Queue.toArray
    ScanBack = fun folder queue state -> IQueue.scanBack folder queue state |> Queue.toArray
    Sum = IQueue.sum
    SumBy = IQueue.sumBy

    CountBy = fun projection queue -> IQueue.countBy projection queue |> Queue.toArray
    Distinct = IQueue.distinct
    DistinctBy = IQueue.distinctBy
    Except = IQueue.except
    GroupBy =
        fun projection queue ->
            IQueue.groupBy projection queue
            |> Queue.toArray
            |> Array.map (fun struct (key, items) -> struct (key, Queue.toArray items))
    Partition = IQueue.partition
    PartitionWith = fun partitioner queue -> IQueue.partitionWith partitioner queue |> toArrayPair
    Skip = IQueue.skip
    SkipLast = IQueue.skipLast
    SkipWhile = IQueue.skipWhile
    SplitAt = IQueue.splitAt
    SplitInto = fun count queue -> IQueue.splitInto count queue |> toArrays
    Take = IQueue.take
    TakeLast = IQueue.takeLast
    TakeWhile = IQueue.takeWhile
    Truncate = IQueue.truncate

    Sort = IQueue.sort
    SortBy = IQueue.sortBy
    SortByDescending = IQueue.sortByDescending
    SortDescending = IQueue.sortDescending
    SortWith = IQueue.sortWith
    SortWithComparer = IQueue.sortWithComparer

    RandomChoice = IQueue.randomChoice
    RandomChoiceBy = IQueue.randomChoiceBy
    RandomChoiceWith = IQueue.randomChoiceWith
    RandomChoices = IQueue.randomChoices
    RandomChoicesBy = IQueue.randomChoicesBy
    RandomChoicesWith = IQueue.randomChoicesWith
    RandomSample = IQueue.randomSample
    RandomSampleBy = IQueue.randomSampleBy
    RandomSampleWith = IQueue.randomSampleWith
    RandomShuffle = IQueue.randomShuffle
    RandomShuffleBy = IQueue.randomShuffleBy
    RandomShuffleWith = IQueue.randomShuffleWith

    ToList = IQueue.toList
    ToSeq = IQueue.toSeq
}

/// The functions of the stable sorting facet.
let stableSort : StableSortOps<IQueue<struct (int * char)>> = {
    Name = nameof IQueue
    OfSeq = ofSeq
    ToArray = IQueue.toArray

    Sort = fun items -> ofSeq items |> IQueue.sort |> IQueue.toArray
    SortBy = IQueue.sortBy
    SortByDescending = IQueue.sortByDescending
    SortDescending = fun items -> ofSeq items |> IQueue.sortDescending |> IQueue.toArray
    SortWith = IQueue.sortWith
    SortWithComparer = IQueue.sortWithComparer

    SortIntegersDescending = fun items -> ofSeq items |> IQueue.sortDescending |> IQueue.toArray
    SortIntegersByDescending = fun items -> ofSeq items |> IQueue.sortByDescending id |> IQueue.toArray
}
