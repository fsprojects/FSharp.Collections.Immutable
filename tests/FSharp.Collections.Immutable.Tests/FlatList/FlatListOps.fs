/// The functions of the `FlatList` module for the contract tests.
[<RequireQualifiedAccess>]
module FSharp.Collections.Immutable.Tests.FlatListOps

open System
open FSharp.Collections.Immutable

let private toArrays (arrays : FlatList<FlatList<int>>) = arrays |> FlatList.toArray |> Array.map FlatList.toArray

let private toArrayPair (struct (first : FlatList<'T1>, second : FlatList<'T2>)) =
    struct (FlatList.toArray first, FlatList.toArray second)

/// The functions of the sequence-like facet. `FlatList` names its parameters as the `Array` module does.
let seqLike : SeqOps<FlatList<int>> = {
    Name = nameof FlatList
    Mirror = ArrayMirror
    ParamName = id
    OfSeq = FlatList.ofSeq
    ToArray = FlatList.toArray

    IsEmpty = FlatList.isEmpty
    Length = FlatList.length

    ExactlyOne = FlatList.exactlyOne
    Head = FlatList.head
    Last = FlatList.last
    Tail = FlatList.tail
    TryExactlyOne = FlatList.tryExactlyOne
    TryHead = FlatList.tryHead
    TryLast = FlatList.tryLast
    TryTail = FlatList.tryTail

    Contains = FlatList.contains
    Exists = FlatList.exists
    Exists2 = FlatList.exists2
    Find = FlatList.find
    FindBack = FlatList.findBack
    FindIndex = FlatList.findIndex
    FindIndexBack = FlatList.findIndexBack
    Forall = FlatList.forall
    Forall2 = FlatList.forall2
    Pick = FlatList.pick
    TryFind = FlatList.tryFind
    TryFindBack = FlatList.tryFindBack
    TryFindIndex = FlatList.tryFindIndex
    TryFindIndexBack = FlatList.tryFindIndexBack
    TryPick = FlatList.tryPick

    AllPairs = fun array1 array2 -> FlatList.allPairs array1 array2 |> FlatList.toArray
    Append = FlatList.append
    Choose = FlatList.choose
    ChunkBySize = fun chunkSize array -> FlatList.chunkBySize chunkSize array |> toArrays
    Collect = FlatList.collect
    Concat = FlatList.concat
    Filter = FlatList.filter
    Indexed = FlatList.indexed >> FlatList.toArray
    Map = FlatList.map
    Map2 = FlatList.map2
    Map3 = FlatList.map3
    Mapi = FlatList.mapi
    Mapi2 = FlatList.mapi2
    Pairwise = FlatList.pairwise >> FlatList.toArray
    Permute = FlatList.permute
    Rev = FlatList.rev
    Transpose = fun arrays -> FlatList.transpose arrays |> toArrays
    Unzip = fun pairs -> FlatList.unzip (FlatList.ofArray pairs) |> toArrayPair
    Unzip3 =
        fun triples ->
            let struct (first, second, third) = FlatList.unzip3 (FlatList.ofArray triples)
            struct (FlatList.toArray first, FlatList.toArray second, FlatList.toArray third)
    Where = FlatList.where
    Windowed = fun windowSize array -> FlatList.windowed windowSize array |> toArrays
    Zip = fun array1 array2 -> FlatList.zip array1 array2 |> FlatList.toArray
    Zip3 = fun array1 array2 array3 -> FlatList.zip3 array1 array2 array3 |> FlatList.toArray

    Average = fun items -> FlatList.average (FlatList.ofArray items)
    AverageBy = FlatList.averageBy
    CompareWith = FlatList.compareWith
    Fold = FlatList.fold
    Fold2 = FlatList.fold2
    FoldBack = FlatList.foldBack
    FoldBack2 = FlatList.foldBack2
    Iter = FlatList.iter
    Iter2 = FlatList.iter2
    Iteri = FlatList.iteri
    Iteri2 = FlatList.iteri2
    MapFold =
        fun mapping state array ->
            let struct (results, final) = FlatList.mapFold mapping state array
            struct (FlatList.toArray results, final)
    MapFoldBack =
        fun mapping array state ->
            let struct (results, final) = FlatList.mapFoldBack mapping array state
            struct (FlatList.toArray results, final)
    Max = FlatList.max
    MaxBy = FlatList.maxBy
    Min = FlatList.min
    MinBy = FlatList.minBy
    Reduce = FlatList.reduce
    ReduceBack = FlatList.reduceBack
    Scan = fun folder state array -> FlatList.scan folder state array |> FlatList.toArray
    ScanBack = fun folder array state -> FlatList.scanBack folder array state |> FlatList.toArray
    Sum = FlatList.sum
    SumBy = FlatList.sumBy

    CountBy = fun projection array -> FlatList.countBy projection array |> FlatList.toArray
    Distinct = FlatList.distinct
    DistinctBy = FlatList.distinctBy
    Except = FlatList.except
    GroupBy =
        fun projection array ->
            FlatList.groupBy projection array
            |> FlatList.toArray
            |> Array.map (fun struct (key, items) -> struct (key, FlatList.toArray items))
    Partition = FlatList.partition
    PartitionWith = fun partitioner array -> FlatList.partitionWith partitioner array |> toArrayPair
    Skip = FlatList.skip
    SkipLast = FlatList.skipLast
    SkipWhile = FlatList.skipWhile
    SplitAt = FlatList.splitAt
    SplitInto = fun count array -> FlatList.splitInto count array |> toArrays
    Take = FlatList.take
    TakeLast = FlatList.takeLast
    TakeWhile = FlatList.takeWhile
    Truncate = FlatList.truncate

    Sort = FlatList.sort
    SortBy = FlatList.sortBy
    SortByDescending = FlatList.sortByDescending
    SortDescending = FlatList.sortDescending
    SortWith = FlatList.sortWith
    SortWithComparer = FlatList.sortWithComparer

    RandomChoice = FlatList.randomChoice
    RandomChoiceBy = FlatList.randomChoiceBy
    RandomChoiceWith = FlatList.randomChoiceWith
    RandomChoices = FlatList.randomChoices
    RandomChoicesBy = FlatList.randomChoicesBy
    RandomChoicesWith = FlatList.randomChoicesWith
    RandomSample = FlatList.randomSample
    RandomSampleBy = FlatList.randomSampleBy
    RandomSampleWith = FlatList.randomSampleWith
    RandomShuffle = FlatList.randomShuffle
    RandomShuffleBy = FlatList.randomShuffleBy
    RandomShuffleWith = FlatList.randomShuffleWith

    ToList = FlatList.toList
    ToSeq = FlatList.toSeq
}

/// The functions of the creation facet.
let creation : SeqCreationOps<FlatList<int>> = {
    Name = nameof FlatList
    Mirror = ArrayMirror
    ParamName = id
    ToArray = FlatList.toArray

    Empty = fun () -> FlatList.empty
    Init = FlatList.init
    OfArray = FlatList.ofArray
    OfList = FlatList.ofList
    OfReadOnlySpan = fun array -> FlatList.ofReadOnlySpan (ReadOnlySpan array)
    OfSeq = FlatList.ofSeq
    OfSpan = fun array -> FlatList.ofSpan (Span array)
    Replicate = FlatList.replicate
    Singleton = FlatList.singleton
    Unfold = FlatList.unfold
}

/// The functions of the index-based facet.
let indexed : IndexedOps<FlatList<int>> = {
    Name = nameof FlatList
    Mirror = ArrayMirror
    ParamName = id
    OfSeq = FlatList.ofSeq
    ToArray = FlatList.toArray

    Item = FlatList.item
    TryItem = FlatList.tryItem

    IndexOf = FlatList.indexOf
    IndexOfWith = FlatList.indexOfWith
    LastIndexOf = FlatList.lastIndexOf
    LastIndexOfWith = FlatList.lastIndexOfWith

    Add = FlatList.add
    InsertAt = FlatList.insertAt
    InsertManyAt = FlatList.insertManyAt
    RemoveAt = FlatList.removeAt
    RemoveManyAt = FlatList.removeManyAt
    UpdateAt = FlatList.updateAt
}
