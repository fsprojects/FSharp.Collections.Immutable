/// The functions of the `IStack` module for the contract tests, over stacks seen through the interface.
[<RequireQualifiedAccess>]
module FSharp.Collections.Immutable.Tests.IStackOps

open System.Collections.Immutable
open FSharp.Collections.Immutable

/// A stack of the elements, the first one on top, seen through the interface.
let private ofSeq (source : seq<'T>) : IImmutableStack<'T> = Stack.ofSeq source

let private toArrays (stacks : Stack<Stack<int>>) = stacks |> Stack.toArray |> Array.map Stack.toArray

let private toArrayPair (struct (first : Stack<'T1>, second : Stack<'T2>)) = struct (Stack.toArray first, Stack.toArray second)

/// The functions of the sequence-like facet. The functions that change the element type return a stack, which the
/// instance sees through the interface again where the record expects the collection of the module.
let seqLike : SeqOps<IImmutableStack<int>> = {
    Name = nameof IStack
    Mirror = ListMirror
    ParamName = StackOps.paramName
    OfSeq = ofSeq
    ToArray = IStack.toArray

    IsEmpty = IStack.isEmpty
    Length = IStack.length

    ExactlyOne = IStack.exactlyOne
    Head = IStack.head
    Last = IStack.last
    Tail = IStack.tail
    TryExactlyOne = IStack.tryExactlyOne
    TryHead = IStack.tryHead
    TryLast = IStack.tryLast
    TryTail = IStack.tryTail

    Contains = IStack.contains
    Exists = IStack.exists
    Exists2 = IStack.exists2
    Find = IStack.find
    FindBack = IStack.findBack
    FindIndex = IStack.findIndex
    FindIndexBack = IStack.findIndexBack
    Forall = IStack.forall
    Forall2 = IStack.forall2
    Pick = IStack.pick
    TryFind = IStack.tryFind
    TryFindBack = IStack.tryFindBack
    TryFindIndex = IStack.tryFindIndex
    TryFindIndexBack = IStack.tryFindIndexBack
    TryPick = IStack.tryPick

    AllPairs = fun stack1 stack2 -> IStack.allPairs stack1 stack2 |> Stack.toArray
    Append = IStack.append
    Choose = fun chooser stack -> IStack.choose chooser stack
    ChunkBySize = fun chunkSize stack -> IStack.chunkBySize chunkSize stack |> toArrays
    Collect = fun mapping stack -> IStack.collect mapping stack
    Concat = fun stacks -> IStack.concat stacks
    Filter = IStack.filter
    Indexed = IStack.indexed >> Stack.toArray
    Map = fun mapping stack -> IStack.map mapping stack
    Map2 = fun mapping stack1 stack2 -> IStack.map2 mapping stack1 stack2
    Map3 = fun mapping stack1 stack2 stack3 -> IStack.map3 mapping stack1 stack2 stack3
    Mapi = fun mapping stack -> IStack.mapi mapping stack
    Mapi2 = fun mapping stack1 stack2 -> IStack.mapi2 mapping stack1 stack2
    Pairwise = IStack.pairwise >> Stack.toArray
    Permute = IStack.permute
    Rev = IStack.rev
    Transpose = fun stacks -> IStack.transpose stacks |> toArrays
    Unzip = fun pairs -> IStack.unzip (ofSeq pairs) |> toArrayPair
    Unzip3 =
        fun triples ->
            let struct (first, second, third) = IStack.unzip3 (ofSeq triples)
            struct (Stack.toArray first, Stack.toArray second, Stack.toArray third)
    Where = IStack.where
    Windowed = fun windowSize stack -> IStack.windowed windowSize stack |> toArrays
    Zip = fun stack1 stack2 -> IStack.zip stack1 stack2 |> Stack.toArray
    Zip3 = fun stack1 stack2 stack3 -> IStack.zip3 stack1 stack2 stack3 |> Stack.toArray

    Average = fun items -> IStack.average (ofSeq items)
    AverageBy = IStack.averageBy
    CompareWith = IStack.compareWith
    Fold = IStack.fold
    Fold2 = IStack.fold2
    FoldBack = IStack.foldBack
    FoldBack2 = IStack.foldBack2
    Iter = IStack.iter
    Iter2 = IStack.iter2
    Iteri = IStack.iteri
    Iteri2 = IStack.iteri2
    MapFold =
        fun mapping state stack ->
            let struct (results, final) = IStack.mapFold mapping state stack
            struct (Stack.toArray results, final)
    MapFoldBack =
        fun mapping stack state ->
            let struct (results, final) = IStack.mapFoldBack mapping stack state
            struct (Stack.toArray results, final)
    Max = IStack.max
    MaxBy = IStack.maxBy
    Min = IStack.min
    MinBy = IStack.minBy
    Reduce = IStack.reduce
    ReduceBack = IStack.reduceBack
    Scan = fun folder state stack -> IStack.scan folder state stack |> Stack.toArray
    ScanBack = fun folder stack state -> IStack.scanBack folder stack state |> Stack.toArray
    Sum = IStack.sum
    SumBy = IStack.sumBy

    CountBy = fun projection stack -> IStack.countBy projection stack |> Stack.toArray
    Distinct = IStack.distinct
    DistinctBy = IStack.distinctBy
    Except = IStack.except
    GroupBy =
        fun projection stack ->
            IStack.groupBy projection stack
            |> Stack.toArray
            |> Array.map (fun struct (key, items) -> struct (key, Stack.toArray items))
    Partition = IStack.partition
    PartitionWith = fun partitioner stack -> IStack.partitionWith partitioner stack |> toArrayPair
    Skip = IStack.skip
    SkipLast = IStack.skipLast
    SkipWhile = IStack.skipWhile
    SplitAt = IStack.splitAt
    SplitInto = fun count stack -> IStack.splitInto count stack |> toArrays
    Take = IStack.take
    TakeLast = IStack.takeLast
    TakeWhile = IStack.takeWhile
    Truncate = IStack.truncate

    Sort = IStack.sort
    SortBy = IStack.sortBy
    SortByDescending = IStack.sortByDescending
    SortDescending = IStack.sortDescending
    SortWith = IStack.sortWith
    SortWithComparer = IStack.sortWithComparer

    RandomChoice = IStack.randomChoice
    RandomChoiceBy = IStack.randomChoiceBy
    RandomChoiceWith = IStack.randomChoiceWith
    RandomChoices = IStack.randomChoices
    RandomChoicesBy = IStack.randomChoicesBy
    RandomChoicesWith = IStack.randomChoicesWith
    RandomSample = IStack.randomSample
    RandomSampleBy = IStack.randomSampleBy
    RandomSampleWith = IStack.randomSampleWith
    RandomShuffle = IStack.randomShuffle
    RandomShuffleBy = IStack.randomShuffleBy
    RandomShuffleWith = IStack.randomShuffleWith

    ToList = IStack.toList
    ToSeq = IStack.toSeq
}

/// The functions of the stable sorting facet.
let stableSort : StableSortOps<IImmutableStack<struct (int * char)>> = {
    Name = nameof IStack
    OfSeq = ofSeq
    ToArray = IStack.toArray

    Sort = fun items -> ofSeq items |> IStack.sort |> IStack.toArray
    SortBy = IStack.sortBy
    SortByDescending = IStack.sortByDescending
    SortDescending = fun items -> ofSeq items |> IStack.sortDescending |> IStack.toArray
    SortWith = IStack.sortWith
    SortWithComparer = IStack.sortWithComparer

    SortIntegersDescending = fun items -> ofSeq items |> IStack.sortDescending |> IStack.toArray
    SortIntegersByDescending = fun items -> ofSeq items |> IStack.sortByDescending id |> IStack.toArray
}
