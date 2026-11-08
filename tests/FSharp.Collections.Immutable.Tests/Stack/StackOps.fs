/// The functions of the `Stack` module for the contract tests.
[<RequireQualifiedAccess>]
module FSharp.Collections.Immutable.Tests.StackOps

open System
open System.Reflection
open FSharp.Collections.Immutable

/// <summary>
/// Maps the name of a parameter that a <c>List</c> function reports to the name that the stack modules report.
/// </summary>
/// <remarks>
/// The stack modules name their stacks where <c>List</c> names its lists: <c>list</c>, <c>list1</c>, <c>lists</c> and
/// the list names that <c>List</c> composes, such as <c>"list1, list2"</c> and <c>"list.[1]"</c>, become <c>stack</c>,
/// <c>stack1</c>, <c>stacks</c>, <c>"stack1, stack2"</c> and <c>"stack.[1]"</c>. Every other name, such as
/// <c>source</c>, stays.
/// </remarks>
let paramName (name : string) = name.Replace ("list", "stack", StringComparison.Ordinal)

/// <summary>
/// The name in F# of the function that a module of the stacks compiles to a method of the given name, or
/// <see langword="null"/> when the module has no such method or the method has its name in F# too.
/// </summary>
/// <param name="moduleName">The full name of the module, such as <c>FSharp.Collections.Immutable.Option.Stack</c>.</param>
/// <param name="compiledName">The name of the method.</param>
/// <remarks>
/// F# records the name of a function whose <see cref="T:Microsoft.FSharp.Core.CompiledNameAttribute"/> differs from it in a
/// <see cref="T:Microsoft.FSharp.Core.CompilationSourceNameAttribute"/> of the method.
/// </remarks>
let sourceName (moduleName : string) (compiledName : string) : string | null =
    let moduleType =
        typeof<StackCE>.Assembly.GetType(moduleName, true)
        |> nonNull

    match moduleType.GetMethod (compiledName, BindingFlags.Public ||| BindingFlags.Static) with
    | null -> null
    | methodInfo ->
        match methodInfo.GetCustomAttribute<CompilationSourceNameAttribute>() with
        | null -> null
        | attribute -> attribute.SourceName

let private toArrays (stacks : Stack<Stack<int>>) = stacks |> Stack.toArray |> Array.map Stack.toArray

let private toArrayPair (struct (first : Stack<'T1>, second : Stack<'T2>)) = struct (Stack.toArray first, Stack.toArray second)

/// The functions of the sequence-like facet.
let seqLike : SeqOps<Stack<int>> = {
    Name = nameof Stack
    Mirror = ListMirror
    ParamName = paramName
    OfSeq = Stack.ofSeq
    ToArray = Stack.toArray

    IsEmpty = Stack.isEmpty
    Length = Stack.length

    ExactlyOne = Stack.exactlyOne
    Head = Stack.head
    Last = Stack.last
    Tail = Stack.tail
    TryExactlyOne = Stack.tryExactlyOne
    TryHead = Stack.tryHead
    TryLast = Stack.tryLast
    TryTail = Stack.tryTail

    Contains = Stack.contains
    Exists = Stack.exists
    Exists2 = Stack.exists2
    Find = Stack.find
    FindBack = Stack.findBack
    FindIndex = Stack.findIndex
    FindIndexBack = Stack.findIndexBack
    Forall = Stack.forall
    Forall2 = Stack.forall2
    Pick = Stack.pick
    TryFind = Stack.tryFind
    TryFindBack = Stack.tryFindBack
    TryFindIndex = Stack.tryFindIndex
    TryFindIndexBack = Stack.tryFindIndexBack
    TryPick = Stack.tryPick

    AllPairs = fun stack1 stack2 -> Stack.allPairs stack1 stack2 |> Stack.toArray
    Append = Stack.append
    Choose = Stack.choose
    ChunkBySize = fun chunkSize stack -> Stack.chunkBySize chunkSize stack |> toArrays
    Collect = Stack.collect
    Concat = Stack.concat
    Filter = Stack.filter
    Indexed = Stack.indexed >> Stack.toArray
    Map = Stack.map
    Map2 = Stack.map2
    Map3 = Stack.map3
    Mapi = Stack.mapi
    Mapi2 = Stack.mapi2
    Pairwise = Stack.pairwise >> Stack.toArray
    Permute = Stack.permute
    Rev = Stack.rev
    Transpose = fun stacks -> Stack.transpose stacks |> toArrays
    Unzip = fun pairs -> Stack.unzip (Stack.ofArray pairs) |> toArrayPair
    Unzip3 =
        fun triples ->
            let struct (first, second, third) = Stack.unzip3 (Stack.ofArray triples)
            struct (Stack.toArray first, Stack.toArray second, Stack.toArray third)
    Where = Stack.where
    Windowed = fun windowSize stack -> Stack.windowed windowSize stack |> toArrays
    Zip = fun stack1 stack2 -> Stack.zip stack1 stack2 |> Stack.toArray
    Zip3 = fun stack1 stack2 stack3 -> Stack.zip3 stack1 stack2 stack3 |> Stack.toArray

    Average = fun items -> Stack.average (Stack.ofArray items)
    AverageBy = Stack.averageBy
    CompareWith = Stack.compareWith
    Fold = Stack.fold
    Fold2 = Stack.fold2
    FoldBack = Stack.foldBack
    FoldBack2 = Stack.foldBack2
    Iter = Stack.iter
    Iter2 = Stack.iter2
    Iteri = Stack.iteri
    Iteri2 = Stack.iteri2
    MapFold =
        fun mapping state stack ->
            let struct (results, final) = Stack.mapFold mapping state stack
            struct (Stack.toArray results, final)
    MapFoldBack =
        fun mapping stack state ->
            let struct (results, final) = Stack.mapFoldBack mapping stack state
            struct (Stack.toArray results, final)
    Max = Stack.max
    MaxBy = Stack.maxBy
    Min = Stack.min
    MinBy = Stack.minBy
    Reduce = Stack.reduce
    ReduceBack = Stack.reduceBack
    Scan = fun folder state stack -> Stack.scan folder state stack |> Stack.toArray
    ScanBack = fun folder stack state -> Stack.scanBack folder stack state |> Stack.toArray
    Sum = Stack.sum
    SumBy = Stack.sumBy

    CountBy = fun projection stack -> Stack.countBy projection stack |> Stack.toArray
    Distinct = Stack.distinct
    DistinctBy = Stack.distinctBy
    Except = Stack.except
    GroupBy =
        fun projection stack ->
            Stack.groupBy projection stack
            |> Stack.toArray
            |> Array.map (fun struct (key, items) -> struct (key, Stack.toArray items))
    Partition = Stack.partition
    PartitionWith = fun partitioner stack -> Stack.partitionWith partitioner stack |> toArrayPair
    Skip = Stack.skip
    SkipLast = Stack.skipLast
    SkipWhile = Stack.skipWhile
    SplitAt = Stack.splitAt
    SplitInto = fun count stack -> Stack.splitInto count stack |> toArrays
    Take = Stack.take
    TakeLast = Stack.takeLast
    TakeWhile = Stack.takeWhile
    Truncate = Stack.truncate

    Sort = Stack.sort
    SortBy = Stack.sortBy
    SortByDescending = Stack.sortByDescending
    SortDescending = Stack.sortDescending
    SortWith = Stack.sortWith
    SortWithComparer = Stack.sortWithComparer

    RandomChoice = Stack.randomChoice
    RandomChoiceBy = Stack.randomChoiceBy
    RandomChoiceWith = Stack.randomChoiceWith
    RandomChoices = Stack.randomChoices
    RandomChoicesBy = Stack.randomChoicesBy
    RandomChoicesWith = Stack.randomChoicesWith
    RandomSample = Stack.randomSample
    RandomSampleBy = Stack.randomSampleBy
    RandomSampleWith = Stack.randomSampleWith
    RandomShuffle = Stack.randomShuffle
    RandomShuffleBy = Stack.randomShuffleBy
    RandomShuffleWith = Stack.randomShuffleWith

    ToList = Stack.toList
    ToSeq = Stack.toSeq
}

/// The functions of the creation facet. Each function creates a stack whose first element is on top, so the stack
/// enumerates the elements in the order of the input, as the facet expects.
let creation : SeqCreationOps<Stack<int>> = {
    Name = nameof Stack
    Mirror = ListMirror
    ParamName = paramName
    ToArray = Stack.toArray

    Empty = fun () -> Stack.empty
    Init = Stack.init
    OfArray = Stack.ofArray
    OfList = Stack.ofList
    OfReadOnlySpan = fun array -> Stack.ofReadOnlySpan (ReadOnlySpan array)
    OfSeq = Stack.ofSeq
    OfSpan = fun array -> Stack.ofSpan (Span array)
    Replicate = Stack.replicate
    Singleton = Stack.singleton
    Unfold = Stack.unfold
}

/// The functions of the stable sorting facet.
let stableSort : StableSortOps<Stack<struct (int * char)>> = {
    Name = nameof Stack
    OfSeq = Stack.ofSeq
    ToArray = Stack.toArray

    Sort = fun items -> Stack.ofArray items |> Stack.sort |> Stack.toArray
    SortBy = Stack.sortBy
    SortByDescending = Stack.sortByDescending
    SortDescending = fun items -> Stack.ofArray items |> Stack.sortDescending |> Stack.toArray
    SortWith = Stack.sortWith
    SortWithComparer = Stack.sortWithComparer

    SortIntegersDescending = fun items -> Stack.ofArray items |> Stack.sortDescending |> Stack.toArray
    SortIntegersByDescending =
        fun items ->
            Stack.ofArray items
            |> Stack.sortByDescending id
            |> Stack.toArray
}
