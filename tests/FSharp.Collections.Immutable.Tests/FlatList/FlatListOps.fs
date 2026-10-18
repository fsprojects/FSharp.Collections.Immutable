namespace FSharp.Collections.Immutable.Tests

open System
open FSharp.Collections.Immutable

/// The functions of the `FlatList` module for the contract tests.
[<RequireQualifiedAccess>]
module FlatListOps =

    let private toLists (lists : FlatList<FlatList<int>>) = lists |> FlatList.toList |> List.map FlatList.toList

    let private toListPair (struct (first : FlatList<'T1>, second : FlatList<'T2>)) =
        struct (FlatList.toList first, FlatList.toList second)

    /// The functions of the sequence-like facet. `FlatList` names its parameters as the `Array` module does.
    let seqLike : SeqOps<FlatList<int>> = {
        Name = nameof FlatList
        Mirror = ArrayMirror
        ParamName = id
        OfSeq = FlatList.ofSeq
        ToList = FlatList.toList

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

        AllPairs = fun array1 array2 -> FlatList.allPairs array1 array2 |> FlatList.toList
        Append = FlatList.append
        Choose = FlatList.choose
        ChunkBySize = fun chunkSize array -> FlatList.chunkBySize chunkSize array |> toLists
        Collect = FlatList.collect
        Concat = FlatList.concat
        Filter = FlatList.filter
        Indexed = FlatList.indexed >> FlatList.toList
        Map = FlatList.map
        Map2 = FlatList.map2
        Map3 = FlatList.map3
        Mapi = FlatList.mapi
        Mapi2 = FlatList.mapi2
        Pairwise = FlatList.pairwise >> FlatList.toList
        Permute = FlatList.permute
        Rev = FlatList.rev
        Transpose = fun arrays -> FlatList.transpose arrays |> toLists
        Unzip = fun pairs -> FlatList.unzip (FlatList.ofList pairs) |> toListPair
        Unzip3 =
            fun triples ->
                let struct (first, second, third) = FlatList.unzip3 (FlatList.ofList triples)
                struct (FlatList.toList first, FlatList.toList second, FlatList.toList third)
        Where = FlatList.where
        Windowed = fun windowSize array -> FlatList.windowed windowSize array |> toLists
        Zip = fun array1 array2 -> FlatList.zip array1 array2 |> FlatList.toList
        Zip3 = fun array1 array2 array3 -> FlatList.zip3 array1 array2 array3 |> FlatList.toList

        Average = fun items -> FlatList.average (FlatList.ofList items)
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
                struct (FlatList.toList results, final)
        MapFoldBack =
            fun mapping array state ->
                let struct (results, final) = FlatList.mapFoldBack mapping array state
                struct (FlatList.toList results, final)
        Max = FlatList.max
        MaxBy = FlatList.maxBy
        Min = FlatList.min
        MinBy = FlatList.minBy
        Reduce = FlatList.reduce
        ReduceBack = FlatList.reduceBack
        Scan = fun folder state array -> FlatList.scan folder state array |> FlatList.toList
        ScanBack = fun folder array state -> FlatList.scanBack folder array state |> FlatList.toList
        Sum = FlatList.sum
        SumBy = FlatList.sumBy

        CountBy = fun projection array -> FlatList.countBy projection array |> FlatList.toList
        Distinct = FlatList.distinct
        DistinctBy = FlatList.distinctBy
        Except = FlatList.except
        GroupBy =
            fun projection array ->
                FlatList.groupBy projection array
                |> FlatList.toList
                |> List.map (fun struct (key, items) -> struct (key, FlatList.toList items))
        Partition = FlatList.partition
        PartitionWith = fun partitioner array -> FlatList.partitionWith partitioner array |> toListPair
        Skip = FlatList.skip
        SkipLast = FlatList.skipLast
        SkipWhile = FlatList.skipWhile
        SplitAt = FlatList.splitAt
        SplitInto = fun count array -> FlatList.splitInto count array |> toLists
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

        ToArray = FlatList.toArray
        ToSeq = FlatList.toSeq
    }

    /// The functions of the creation facet.
    let creation : SeqCreationOps<FlatList<int>> = {
        Name = nameof FlatList
        Mirror = ArrayMirror
        ParamName = id
        ToList = FlatList.toList

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
        ToList = FlatList.toList

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
