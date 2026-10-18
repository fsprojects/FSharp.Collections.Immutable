/// The functions of the `Queue` module for the contract tests.
[<RequireQualifiedAccess>]
module FSharp.Collections.Immutable.Tests.QueueOps

open System
open System.Reflection
open FSharp.Collections.Immutable

/// The name that the queue modules give to a parameter that the List module names after the list.
let private queueName (name : string) =
    match name with
    | "list" -> "queue"
    | "list1" -> "queue1"
    | "list2" -> "queue2"
    | "list3" -> "queue3"
    | "lists" -> "queues"
    // List.transpose names the list at index i "list.[i]", and the queue modules "queue.[i]"
    | _ when name.StartsWith ("list.[", StringComparison.Ordinal) -> "queue" + name.Substring ("list".Length)
    | _ -> name

/// <summary>
/// Maps the name of a parameter of a List function to the name of the parameter of the queue modules, which name their
/// queues where the List module names its lists, and keep the other names.
/// </summary>
/// <remarks>
/// The name of several parameters, such as <c>"list1, list2"</c>, maps each of them.
/// </remarks>
let paramName (name : string) =
    name.Split (", ", StringSplitOptions.None)
    |> Array.map queueName
    |> String.concat ", "

/// <summary>
/// The complete message of an exception of a List function with the names of the queue modules: the names of the lists
/// that the second line and the parameter suffix of the message mention become the names of the queues.
/// </summary>
let message (text : string) =
    text
        .Replace("list1", "queue1", StringComparison.Ordinal)
        .Replace("list2", "queue2", StringComparison.Ordinal)
        .Replace("list3", "queue3", StringComparison.Ordinal)
        .Replace("list.[", "queue.[", StringComparison.Ordinal)
        .Replace("'list'", "'queue'", StringComparison.Ordinal)

/// <summary>
/// The name in F# of the function that a module of the queues compiles to a method of the given name, or
/// <see langword="null"/> when the module has no such method or the method has its name in F# too.
/// </summary>
/// <param name="moduleName">The full name of the module, such as <c>FSharp.Collections.Immutable.Option.Queue</c>.</param>
/// <param name="compiledName">The name of the method.</param>
/// <remarks>
/// F# records the name of a function whose <see cref="T:Microsoft.FSharp.Core.CompiledNameAttribute"/> differs from it in a
/// <see cref="T:Microsoft.FSharp.Core.CompilationSourceNameAttribute"/> of the method.
/// </remarks>
let sourceName (moduleName : string) (compiledName : string) : string | null =
    let moduleType =
        typeof<QueueCE>.Assembly.GetType(moduleName, true)
        |> nonNull

    match moduleType.GetMethod (compiledName, BindingFlags.Public ||| BindingFlags.Static) with
    | null -> null
    | methodInfo ->
        match methodInfo.GetCustomAttribute<CompilationSourceNameAttribute>() with
        | null -> null
        | attribute -> attribute.SourceName


let private toArrays (queues : Queue<Queue<int>>) = queues |> Queue.toArray |> Array.map Queue.toArray

let private toArrayPair (struct (first : Queue<'T1>, second : Queue<'T2>)) = struct (Queue.toArray first, Queue.toArray second)

/// The functions of the sequence-like facet.
let seqLike : SeqOps<Queue<int>> = {
    Name = nameof Queue
    Mirror = ListMirror
    ParamName = paramName
    OfSeq = Queue.ofSeq
    ToArray = Queue.toArray

    IsEmpty = Queue.isEmpty
    Length = Queue.length

    ExactlyOne = Queue.exactlyOne
    Head = Queue.head
    Last = Queue.last
    Tail = Queue.tail
    TryExactlyOne = Queue.tryExactlyOne
    TryHead = Queue.tryHead
    TryLast = Queue.tryLast
    TryTail = Queue.tryTail

    Contains = Queue.contains
    Exists = Queue.exists
    Exists2 = Queue.exists2
    Find = Queue.find
    FindBack = Queue.findBack
    FindIndex = Queue.findIndex
    FindIndexBack = Queue.findIndexBack
    Forall = Queue.forall
    Forall2 = Queue.forall2
    Pick = Queue.pick
    TryFind = Queue.tryFind
    TryFindBack = Queue.tryFindBack
    TryFindIndex = Queue.tryFindIndex
    TryFindIndexBack = Queue.tryFindIndexBack
    TryPick = Queue.tryPick

    AllPairs = fun queue1 queue2 -> Queue.allPairs queue1 queue2 |> Queue.toArray
    Append = Queue.append
    Choose = Queue.choose
    ChunkBySize = fun chunkSize queue -> Queue.chunkBySize chunkSize queue |> toArrays
    Collect = Queue.collect
    Concat = Queue.concat
    Filter = Queue.filter
    Indexed = Queue.indexed >> Queue.toArray
    Map = Queue.map
    Map2 = Queue.map2
    Map3 = Queue.map3
    Mapi = Queue.mapi
    Mapi2 = Queue.mapi2
    Pairwise = Queue.pairwise >> Queue.toArray
    Permute = Queue.permute
    Rev = Queue.rev
    Transpose = fun queues -> Queue.transpose queues |> toArrays
    Unzip = fun pairs -> Queue.unzip (Queue.ofArray pairs) |> toArrayPair
    Unzip3 =
        fun triples ->
            let struct (first, second, third) = Queue.unzip3 (Queue.ofArray triples)
            struct (Queue.toArray first, Queue.toArray second, Queue.toArray third)
    Where = Queue.where
    Windowed = fun windowSize queue -> Queue.windowed windowSize queue |> toArrays
    Zip = fun queue1 queue2 -> Queue.zip queue1 queue2 |> Queue.toArray
    Zip3 = fun queue1 queue2 queue3 -> Queue.zip3 queue1 queue2 queue3 |> Queue.toArray

    Average = fun items -> Queue.average (Queue.ofArray items)
    AverageBy = Queue.averageBy
    CompareWith = Queue.compareWith
    Fold = Queue.fold
    Fold2 = Queue.fold2
    FoldBack = Queue.foldBack
    FoldBack2 = Queue.foldBack2
    Iter = Queue.iter
    Iter2 = Queue.iter2
    Iteri = Queue.iteri
    Iteri2 = Queue.iteri2
    MapFold =
        fun mapping state queue ->
            let struct (results, final) = Queue.mapFold mapping state queue
            struct (Queue.toArray results, final)
    MapFoldBack =
        fun mapping queue state ->
            let struct (results, final) = Queue.mapFoldBack mapping queue state
            struct (Queue.toArray results, final)
    Max = Queue.max
    MaxBy = Queue.maxBy
    Min = Queue.min
    MinBy = Queue.minBy
    Reduce = Queue.reduce
    ReduceBack = Queue.reduceBack
    Scan = fun folder state queue -> Queue.scan folder state queue |> Queue.toArray
    ScanBack = fun folder queue state -> Queue.scanBack folder queue state |> Queue.toArray
    Sum = Queue.sum
    SumBy = Queue.sumBy

    CountBy = fun projection queue -> Queue.countBy projection queue |> Queue.toArray
    Distinct = Queue.distinct
    DistinctBy = Queue.distinctBy
    Except = Queue.except
    GroupBy =
        fun projection queue ->
            Queue.groupBy projection queue
            |> Queue.toArray
            |> Array.map (fun struct (key, items) -> struct (key, Queue.toArray items))
    Partition = Queue.partition
    PartitionWith = fun partitioner queue -> Queue.partitionWith partitioner queue |> toArrayPair
    Skip = Queue.skip
    SkipLast = Queue.skipLast
    SkipWhile = Queue.skipWhile
    SplitAt = Queue.splitAt
    SplitInto = fun count queue -> Queue.splitInto count queue |> toArrays
    Take = Queue.take
    TakeLast = Queue.takeLast
    TakeWhile = Queue.takeWhile
    Truncate = Queue.truncate

    Sort = Queue.sort
    SortBy = Queue.sortBy
    SortByDescending = Queue.sortByDescending
    SortDescending = Queue.sortDescending
    SortWith = Queue.sortWith
    SortWithComparer = Queue.sortWithComparer

    RandomChoice = Queue.randomChoice
    RandomChoiceBy = Queue.randomChoiceBy
    RandomChoiceWith = Queue.randomChoiceWith
    RandomChoices = Queue.randomChoices
    RandomChoicesBy = Queue.randomChoicesBy
    RandomChoicesWith = Queue.randomChoicesWith
    RandomSample = Queue.randomSample
    RandomSampleBy = Queue.randomSampleBy
    RandomSampleWith = Queue.randomSampleWith
    RandomShuffle = Queue.randomShuffle
    RandomShuffleBy = Queue.randomShuffleBy
    RandomShuffleWith = Queue.randomShuffleWith

    ToList = Queue.toList
    ToSeq = Queue.toSeq
}

/// The functions of the creation facet.
let creation : SeqCreationOps<Queue<int>> = {
    Name = nameof Queue
    Mirror = ListMirror
    ParamName = paramName
    ToArray = Queue.toArray

    Empty = fun () -> Queue.empty
    Init = Queue.init
    OfArray = Queue.ofArray
    OfList = Queue.ofList
    OfReadOnlySpan = fun array -> Queue.ofReadOnlySpan (ReadOnlySpan array)
    OfSeq = Queue.ofSeq
    OfSpan = fun array -> Queue.ofSpan (Span array)
    Replicate = Queue.replicate
    Singleton = Queue.singleton
    Unfold = Queue.unfold
}

/// The functions of the stable sorting facet.
let stableSort : StableSortOps<Queue<struct (int * char)>> = {
    Name = nameof Queue
    OfSeq = Queue.ofSeq
    ToArray = Queue.toArray

    Sort = fun items -> Queue.ofArray items |> Queue.sort |> Queue.toArray
    SortBy = Queue.sortBy
    SortByDescending = Queue.sortByDescending
    SortDescending = fun items -> Queue.ofArray items |> Queue.sortDescending |> Queue.toArray
    SortWith = Queue.sortWith
    SortWithComparer = Queue.sortWithComparer

    SortIntegersDescending = fun items -> Queue.ofArray items |> Queue.sortDescending |> Queue.toArray
    SortIntegersByDescending =
        fun items ->
            Queue.ofArray items
            |> Queue.sortByDescending id
            |> Queue.toArray
}
