/// The functions of the `IIndexedList` module for the contract tests, over indexed lists seen through the interface.
[<RequireQualifiedAccess>]
module FSharp.Collections.Immutable.Tests.IIndexedListOps

open FSharp.Collections.Immutable

/// An indexed list of the elements, seen through the interface.
let private ofSeq (source : seq<'T>) : IIndexedList<'T> = IndexedList.ofSeq source

// The functions of IIndexedList return indexed lists of indexed lists and pairs of indexed lists as the ones of IndexedList
let private toArrays = IndexedListOps.toArrays

let private toArrayPair pair = IndexedListOps.toArrayPair pair

/// The functions of the sequence-like facet. The functions that change the element type return an indexed list, which
/// the instance sees through the interface again where the record expects the collection of the module.
let seqLike : SeqOps<IIndexedList<int>> = {
    Name = nameof IIndexedList
    Mirror = ListMirror
    ParamName = id
    OfSeq = ofSeq
    ToArray = IIndexedList.toArray

    IsEmpty = IIndexedList.isEmpty
    Length = IIndexedList.length

    ExactlyOne = IIndexedList.exactlyOne
    Head = IIndexedList.head
    Last = IIndexedList.last
    Tail = IIndexedList.tail
    TryExactlyOne = IIndexedList.tryExactlyOne
    TryHead = IIndexedList.tryHead
    TryLast = IIndexedList.tryLast
    TryTail = IIndexedList.tryTail

    Contains = IIndexedList.contains
    Exists = IIndexedList.exists
    Exists2 = IIndexedList.exists2
    Find = IIndexedList.find
    FindBack = IIndexedList.findBack
    FindIndex = IIndexedList.findIndex
    FindIndexBack = IIndexedList.findIndexBack
    Forall = IIndexedList.forall
    Forall2 = IIndexedList.forall2
    Pick = IIndexedList.pick
    TryFind = IIndexedList.tryFind
    TryFindBack = IIndexedList.tryFindBack
    TryFindIndex = IIndexedList.tryFindIndex
    TryFindIndexBack = IIndexedList.tryFindIndexBack
    TryPick = IIndexedList.tryPick

    AllPairs = fun list1 list2 -> IIndexedList.allPairs list1 list2 |> IndexedList.toArray
    Append = IIndexedList.append
    Choose = fun chooser list -> IIndexedList.choose chooser list
    ChunkBySize = fun chunkSize list -> IIndexedList.chunkBySize chunkSize list |> toArrays
    Collect = fun mapping list -> IIndexedList.collect mapping list
    Concat = fun lists -> IIndexedList.concat lists
    Filter = IIndexedList.filter
    Indexed = IIndexedList.indexed >> IndexedList.toArray
    Map = fun mapping list -> IIndexedList.map mapping list
    Map2 = fun mapping list1 list2 -> IIndexedList.map2 mapping list1 list2
    Map3 = fun mapping list1 list2 list3 -> IIndexedList.map3 mapping list1 list2 list3
    Mapi = fun mapping list -> IIndexedList.mapi mapping list
    Mapi2 = fun mapping list1 list2 -> IIndexedList.mapi2 mapping list1 list2
    Pairwise = IIndexedList.pairwise >> IndexedList.toArray
    Permute = IIndexedList.permute
    Rev = IIndexedList.rev
    Transpose = fun lists -> IIndexedList.transpose lists |> toArrays
    Unzip = fun pairs -> IIndexedList.unzip (ofSeq pairs) |> toArrayPair
    Unzip3 =
        fun triples ->
            let struct (first, second, third) = IIndexedList.unzip3 (ofSeq triples)
            struct (IndexedList.toArray first, IndexedList.toArray second, IndexedList.toArray third)
    Where = IIndexedList.where
    Windowed = fun windowSize list -> IIndexedList.windowed windowSize list |> toArrays
    Zip = fun list1 list2 -> IIndexedList.zip list1 list2 |> IndexedList.toArray
    Zip3 = fun list1 list2 list3 -> IIndexedList.zip3 list1 list2 list3 |> IndexedList.toArray

    Average = fun items -> IIndexedList.average (ofSeq items)
    AverageBy = IIndexedList.averageBy
    CompareWith = IIndexedList.compareWith
    Fold = IIndexedList.fold
    Fold2 = IIndexedList.fold2
    FoldBack = IIndexedList.foldBack
    FoldBack2 = IIndexedList.foldBack2
    Iter = IIndexedList.iter
    Iter2 = IIndexedList.iter2
    Iteri = IIndexedList.iteri
    Iteri2 = IIndexedList.iteri2
    MapFold =
        fun mapping state list ->
            let struct (results, final) = IIndexedList.mapFold mapping state list
            struct (IndexedList.toArray results, final)
    MapFoldBack =
        fun mapping list state ->
            let struct (results, final) = IIndexedList.mapFoldBack mapping list state
            struct (IndexedList.toArray results, final)
    Max = IIndexedList.max
    MaxBy = IIndexedList.maxBy
    Min = IIndexedList.min
    MinBy = IIndexedList.minBy
    Reduce = IIndexedList.reduce
    ReduceBack = IIndexedList.reduceBack
    Scan = fun folder state list -> IIndexedList.scan folder state list |> IndexedList.toArray
    ScanBack =
        fun folder list state ->
            IIndexedList.scanBack folder list state
            |> IndexedList.toArray
    Sum = IIndexedList.sum
    SumBy = IIndexedList.sumBy

    CountBy = fun projection list -> IIndexedList.countBy projection list |> IndexedList.toArray
    Distinct = IIndexedList.distinct
    DistinctBy = IIndexedList.distinctBy
    Except = IIndexedList.except
    GroupBy =
        fun projection list ->
            IIndexedList.groupBy projection list
            |> Seq.map (fun struct (key, items) -> struct (key, IndexedList.toArray items))
            |> Seq.toArray
    Partition = IIndexedList.partition
    PartitionWith = fun partitioner list -> IIndexedList.partitionWith partitioner list |> toArrayPair
    Skip = IIndexedList.skip
    SkipLast = IIndexedList.skipLast
    SkipWhile = IIndexedList.skipWhile
    SplitAt = IIndexedList.splitAt
    SplitInto = fun count list -> IIndexedList.splitInto count list |> toArrays
    Take = IIndexedList.take
    TakeLast = IIndexedList.takeLast
    TakeWhile = IIndexedList.takeWhile
    Truncate = IIndexedList.truncate

    Sort = IIndexedList.sort
    SortBy = IIndexedList.sortBy
    SortByDescending = IIndexedList.sortByDescending
    SortDescending = IIndexedList.sortDescending
    SortWith = IIndexedList.sortWith
    SortWithComparer = IIndexedList.sortWithComparer

    RandomChoice = IIndexedList.randomChoice
    RandomChoiceBy = IIndexedList.randomChoiceBy
    RandomChoiceWith = IIndexedList.randomChoiceWith
    RandomChoices = IIndexedList.randomChoices
    RandomChoicesBy = IIndexedList.randomChoicesBy
    RandomChoicesWith = IIndexedList.randomChoicesWith
    RandomSample = IIndexedList.randomSample
    RandomSampleBy = IIndexedList.randomSampleBy
    RandomSampleWith = IIndexedList.randomSampleWith
    RandomShuffle = IIndexedList.randomShuffle
    RandomShuffleBy = IIndexedList.randomShuffleBy
    RandomShuffleWith = IIndexedList.randomShuffleWith

    ToList = IIndexedList.toList
    ToSeq = IIndexedList.toSeq
}

/// The functions of the index-based facet.
let indexed : IndexedOps<IIndexedList<int>> = {
    Name = nameof IIndexedList
    Mirror = ListMirror
    ParamName = id
    OfSeq = ofSeq
    ToArray = IIndexedList.toArray

    Item = IIndexedList.item
    TryItem = IIndexedList.tryItem

    IndexOf = IIndexedList.indexOf
    IndexOfWith = IIndexedList.indexOfWith
    LastIndexOf = IIndexedList.lastIndexOf
    LastIndexOfWith = IIndexedList.lastIndexOfWith

    Add = IIndexedList.add
    InsertAt = IIndexedList.insertAt
    InsertManyAt = IIndexedList.insertManyAt
    RemoveAt = IIndexedList.removeAt
    RemoveManyAt = IIndexedList.removeManyAt
    UpdateAt = IIndexedList.updateAt
}

/// The functions of the facet of the members of `IImmutableList<T>`, over strings.
let listMembers : ListMembersOps<IIndexedList<string>> = {
    Name = nameof IIndexedList
    OfSeq = ofSeq
    ToArray = IIndexedList.toArray

    IndexOfRange = IIndexedList.indexOfRange
    IndexOfRangeWith = IIndexedList.indexOfRangeWith
    LastIndexOfRange = IIndexedList.lastIndexOfRange
    LastIndexOfRangeWith = IIndexedList.lastIndexOfRangeWith

    Remove = IIndexedList.remove
    RemoveMany = IIndexedList.removeMany
    RemoveManyWith = IIndexedList.removeManyWith
    RemoveWith = IIndexedList.removeWith
    Replace = IIndexedList.replace
    ReplaceWith = IIndexedList.replaceWith
}

/// The functions of the stable sorting facet.
let stableSort : StableSortOps<IIndexedList<struct (int * char)>> = {
    Name = nameof IIndexedList
    OfSeq = ofSeq
    ToArray = IIndexedList.toArray

    Sort = fun items -> ofSeq items |> IIndexedList.sort |> IIndexedList.toArray
    SortBy = IIndexedList.sortBy
    SortByDescending = IIndexedList.sortByDescending
    SortDescending =
        fun items ->
            ofSeq items
            |> IIndexedList.sortDescending
            |> IIndexedList.toArray
    SortWith = IIndexedList.sortWith
    SortWithComparer = IIndexedList.sortWithComparer

    SortIntegersDescending =
        fun items ->
            ofSeq items
            |> IIndexedList.sortDescending
            |> IIndexedList.toArray
    SortIntegersByDescending =
        fun items ->
            ofSeq items
            |> IIndexedList.sortByDescending id
            |> IIndexedList.toArray
}

/// The functions of the facet whose cost the tests bound.
let cost : CostOps<IIndexedList<int>> = {
    Name = nameof IIndexedList
    OfSeq = ofSeq
    Max = IIndexedList.max
    MaxBy = IIndexedList.maxBy
    Min = IIndexedList.min
    MinBy = IIndexedList.minBy
    Exists2 = IIndexedList.exists2
    Forall2 = IIndexedList.forall2
    CompareWith = IIndexedList.compareWith
    ConcatLength = fun lists -> IIndexedList.concat lists |> IndexedList.length
    CollectLength = fun mapping list -> IIndexedList.collect mapping list |> IndexedList.length
}
