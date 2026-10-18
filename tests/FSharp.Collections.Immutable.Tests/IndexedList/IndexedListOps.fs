/// The functions of the `IndexedList` module for the contract tests.
[<RequireQualifiedAccess>]
module FSharp.Collections.Immutable.Tests.IndexedListOps

open System
open FSharp.Collections.Immutable

/// The elements of the indexed lists of an indexed list, as arrays. The functions of IIndexedList return indexed lists
/// of indexed lists as well, so the Ops of IIndexedList uses it too.
let toArrays (lists : IndexedList<IndexedList<int>>) = lists |> Seq.map IndexedList.toArray |> Seq.toArray

/// The elements of the two indexed lists of a pair, as a pair of arrays; the Ops of IIndexedList uses it too.
let toArrayPair (struct (first : IndexedList<'T1>, second : IndexedList<'T2>)) =
    struct (IndexedList.toArray first, IndexedList.toArray second)

/// The functions of the sequence-like facet. `IndexedList` names its parameters as the `List` module does, so its
/// exceptions name the parameters that the `List` functions name.
let seqLike : SeqOps<IndexedList<int>> = {
    Name = nameof IndexedList
    Mirror = ListMirror
    ParamName = id
    OfSeq = IndexedList.ofSeq
    ToArray = IndexedList.toArray

    IsEmpty = IndexedList.isEmpty
    Length = IndexedList.length

    ExactlyOne = IndexedList.exactlyOne
    Head = IndexedList.head
    Last = IndexedList.last
    Tail = IndexedList.tail
    TryExactlyOne = IndexedList.tryExactlyOne
    TryHead = IndexedList.tryHead
    TryLast = IndexedList.tryLast
    TryTail = IndexedList.tryTail

    Contains = IndexedList.contains
    Exists = IndexedList.exists
    Exists2 = IndexedList.exists2
    Find = IndexedList.find
    FindBack = IndexedList.findBack
    FindIndex = IndexedList.findIndex
    FindIndexBack = IndexedList.findIndexBack
    Forall = IndexedList.forall
    Forall2 = IndexedList.forall2
    Pick = IndexedList.pick
    TryFind = IndexedList.tryFind
    TryFindBack = IndexedList.tryFindBack
    TryFindIndex = IndexedList.tryFindIndex
    TryFindIndexBack = IndexedList.tryFindIndexBack
    TryPick = IndexedList.tryPick

    AllPairs = fun list1 list2 -> IndexedList.allPairs list1 list2 |> IndexedList.toArray
    Append = IndexedList.append
    Choose = IndexedList.choose
    ChunkBySize = fun chunkSize list -> IndexedList.chunkBySize chunkSize list |> toArrays
    Collect = IndexedList.collect
    Concat = IndexedList.concat
    Filter = IndexedList.filter
    Indexed = IndexedList.indexed >> IndexedList.toArray
    Map = IndexedList.map
    Map2 = IndexedList.map2
    Map3 = IndexedList.map3
    Mapi = IndexedList.mapi
    Mapi2 = IndexedList.mapi2
    Pairwise = IndexedList.pairwise >> IndexedList.toArray
    Permute = IndexedList.permute
    Rev = IndexedList.rev
    Transpose = fun lists -> IndexedList.transpose lists |> toArrays
    Unzip = fun pairs -> IndexedList.unzip (IndexedList.ofArray pairs) |> toArrayPair
    Unzip3 =
        fun triples ->
            let struct (first, second, third) = IndexedList.unzip3 (IndexedList.ofArray triples)
            struct (IndexedList.toArray first, IndexedList.toArray second, IndexedList.toArray third)
    Where = IndexedList.where
    Windowed = fun windowSize list -> IndexedList.windowed windowSize list |> toArrays
    Zip = fun list1 list2 -> IndexedList.zip list1 list2 |> IndexedList.toArray
    Zip3 = fun list1 list2 list3 -> IndexedList.zip3 list1 list2 list3 |> IndexedList.toArray

    Average = fun items -> IndexedList.average (IndexedList.ofArray items)
    AverageBy = IndexedList.averageBy
    CompareWith = IndexedList.compareWith
    Fold = IndexedList.fold
    Fold2 = IndexedList.fold2
    FoldBack = IndexedList.foldBack
    FoldBack2 = IndexedList.foldBack2
    Iter = IndexedList.iter
    Iter2 = IndexedList.iter2
    Iteri = IndexedList.iteri
    Iteri2 = IndexedList.iteri2
    MapFold =
        fun mapping state list ->
            let struct (results, final) = IndexedList.mapFold mapping state list
            struct (IndexedList.toArray results, final)
    MapFoldBack =
        fun mapping list state ->
            let struct (results, final) = IndexedList.mapFoldBack mapping list state
            struct (IndexedList.toArray results, final)
    Max = IndexedList.max
    MaxBy = IndexedList.maxBy
    Min = IndexedList.min
    MinBy = IndexedList.minBy
    Reduce = IndexedList.reduce
    ReduceBack = IndexedList.reduceBack
    Scan = fun folder state list -> IndexedList.scan folder state list |> IndexedList.toArray
    ScanBack =
        fun folder list state ->
            IndexedList.scanBack folder list state
            |> IndexedList.toArray
    Sum = IndexedList.sum
    SumBy = IndexedList.sumBy

    CountBy = fun projection list -> IndexedList.countBy projection list |> IndexedList.toArray
    Distinct = IndexedList.distinct
    DistinctBy = IndexedList.distinctBy
    Except = IndexedList.except
    GroupBy =
        fun projection list ->
            IndexedList.groupBy projection list
            |> Seq.map (fun struct (key, items) -> struct (key, IndexedList.toArray items))
            |> Seq.toArray
    Partition = IndexedList.partition
    PartitionWith = fun partitioner list -> IndexedList.partitionWith partitioner list |> toArrayPair
    Skip = IndexedList.skip
    SkipLast = IndexedList.skipLast
    SkipWhile = IndexedList.skipWhile
    SplitAt = IndexedList.splitAt
    SplitInto = fun count list -> IndexedList.splitInto count list |> toArrays
    Take = IndexedList.take
    TakeLast = IndexedList.takeLast
    TakeWhile = IndexedList.takeWhile
    Truncate = IndexedList.truncate

    Sort = IndexedList.sort
    SortBy = IndexedList.sortBy
    SortByDescending = IndexedList.sortByDescending
    SortDescending = IndexedList.sortDescending
    SortWith = IndexedList.sortWith
    SortWithComparer = IndexedList.sortWithComparer

    RandomChoice = IndexedList.randomChoice
    RandomChoiceBy = IndexedList.randomChoiceBy
    RandomChoiceWith = IndexedList.randomChoiceWith
    RandomChoices = IndexedList.randomChoices
    RandomChoicesBy = IndexedList.randomChoicesBy
    RandomChoicesWith = IndexedList.randomChoicesWith
    RandomSample = IndexedList.randomSample
    RandomSampleBy = IndexedList.randomSampleBy
    RandomSampleWith = IndexedList.randomSampleWith
    RandomShuffle = IndexedList.randomShuffle
    RandomShuffleBy = IndexedList.randomShuffleBy
    RandomShuffleWith = IndexedList.randomShuffleWith

    ToList = IndexedList.toList
    ToSeq = IndexedList.toSeq
}

/// The functions of the creation facet.
let creation : SeqCreationOps<IndexedList<int>> = {
    Name = nameof IndexedList
    Mirror = ListMirror
    ParamName = id
    ToArray = IndexedList.toArray

    Empty = fun () -> IndexedList.empty
    Init = IndexedList.init
    OfArray = IndexedList.ofArray
    OfList = IndexedList.ofList
    OfReadOnlySpan = fun array -> IndexedList.ofReadOnlySpan (ReadOnlySpan array)
    OfSeq = IndexedList.ofSeq
    OfSpan = fun array -> IndexedList.ofSpan (Span array)
    Replicate = IndexedList.replicate
    Singleton = IndexedList.singleton
    Unfold = IndexedList.unfold
}

/// The functions of the index-based facet.
let indexed : IndexedOps<IndexedList<int>> = {
    Name = nameof IndexedList
    Mirror = ListMirror
    ParamName = id
    OfSeq = IndexedList.ofSeq
    ToArray = IndexedList.toArray

    Item = IndexedList.item
    TryItem = IndexedList.tryItem

    IndexOf = IndexedList.indexOf
    IndexOfWith = IndexedList.indexOfWith
    LastIndexOf = IndexedList.lastIndexOf
    LastIndexOfWith = IndexedList.lastIndexOfWith

    Add = IndexedList.add
    InsertAt = IndexedList.insertAt
    InsertManyAt = IndexedList.insertManyAt
    RemoveAt = IndexedList.removeAt
    RemoveManyAt = IndexedList.removeManyAt
    UpdateAt = IndexedList.updateAt
}

/// The functions of the facet of the members of `IImmutableList<T>`, over strings.
let listMembers : ListMembersOps<IndexedList<string>> = {
    Name = nameof IndexedList
    OfSeq = IndexedList.ofSeq
    ToArray = IndexedList.toArray

    IndexOfRange = IndexedList.indexOfRange
    IndexOfRangeWith = IndexedList.indexOfRangeWith
    LastIndexOfRange = IndexedList.lastIndexOfRange
    LastIndexOfRangeWith = IndexedList.lastIndexOfRangeWith

    Remove = IndexedList.remove
    RemoveMany = IndexedList.removeMany
    RemoveManyWith = IndexedList.removeManyWith
    RemoveWith = IndexedList.removeWith
    Replace = IndexedList.replace
    ReplaceWith = IndexedList.replaceWith
}

/// The functions of the stable sorting facet.
let stableSort : StableSortOps<IndexedList<struct (int * char)>> = {
    Name = nameof IndexedList
    OfSeq = IndexedList.ofSeq
    ToArray = IndexedList.toArray

    Sort =
        fun items ->
            IndexedList.ofArray items
            |> IndexedList.sort
            |> IndexedList.toArray
    SortBy = IndexedList.sortBy
    SortByDescending = IndexedList.sortByDescending
    SortDescending =
        fun items ->
            IndexedList.ofArray items
            |> IndexedList.sortDescending
            |> IndexedList.toArray
    SortWith = IndexedList.sortWith
    SortWithComparer = IndexedList.sortWithComparer

    SortIntegersDescending =
        fun items ->
            IndexedList.ofArray items
            |> IndexedList.sortDescending
            |> IndexedList.toArray
    SortIntegersByDescending =
        fun items ->
            IndexedList.ofArray items
            |> IndexedList.sortByDescending id
            |> IndexedList.toArray
}

/// The functions of the facet whose cost the tests bound.
let cost : CostOps<IndexedList<int>> = {
    Name = nameof IndexedList
    OfSeq = IndexedList.ofSeq
    Max = IndexedList.max
    MaxBy = IndexedList.maxBy
    Min = IndexedList.min
    MinBy = IndexedList.minBy
    Exists2 = IndexedList.exists2
    Forall2 = IndexedList.forall2
    CompareWith = IndexedList.compareWith
    ConcatLength = fun lists -> IndexedList.concat lists |> IndexedList.length
    CollectLength = fun mapping list -> IndexedList.collect mapping list |> IndexedList.length
}
