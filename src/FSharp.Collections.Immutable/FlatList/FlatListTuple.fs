[<RequireQualifiedAccess>]
module FSharp.Collections.Immutable.Tuple.FlatList

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

    FSharp.Collections.Immutable.FlatList.unfold generateStruct state

// ---- Transformation ----

[<CompiledName "AllPairs">]
let allPairs (array1 : FlatList<'T1>) (array2 : FlatList<'T2>) =
    Arrays.allPairs referencePair (unwrapArray (nameof array1) array1) (unwrapArray (nameof array2) array2)
    |> wrapArray

[<CompiledName "Indexed">]
let indexed (array : FlatList<'T>) =
    Arrays.indexed referencePair (unwrapArray (nameof array) array)
    |> wrapArray

[<CompiledName "Pairwise">]
let pairwise (array : FlatList<'T>) =
    Arrays.pairwise referencePair (unwrapArray (nameof array) array)
    |> wrapArray

[<CompiledName "Unzip">]
let unzip (array : FlatList<'T1 * 'T2>) =
    let struct (first, second) =
        Arrays.unzip (fun (item1, item2) -> struct (item1, item2)) (unwrapArray (nameof array) array)

    wrapArray first, wrapArray second

[<CompiledName "Unzip3">]
let unzip3 (array : FlatList<'T1 * 'T2 * 'T3>) =
    let struct (first, second, third) =
        Arrays.unzip3 (fun (item1, item2, item3) -> struct (item1, item2, item3)) (unwrapArray (nameof array) array)

    wrapArray first, wrapArray second, wrapArray third

[<CompiledName "Zip">]
let zip (array1 : FlatList<'T1>) (array2 : FlatList<'T2>) =
    let source1 = unwrapArray (nameof array1) array1
    let source2 = unwrapArray (nameof array2) array2
    ArrayChecks.sameLengths (nameof array1) source1 (nameof array2) source2
    Arrays.zip referencePair source1 source2 |> wrapArray

[<CompiledName "Zip3">]
let zip3 (array1 : FlatList<'T1>) (array2 : FlatList<'T2>) (array3 : FlatList<'T3>) =
    let source1 = unwrapArray (nameof array1) array1
    let source2 = unwrapArray (nameof array2) array2
    let source3 = unwrapArray (nameof array3) array3
    ArrayChecks.sameLengths3 (nameof array1) source1 (nameof array2) source2 (nameof array3) source3
    Arrays.zip3 referenceTriple source1 source2 source3
    |> wrapArray

// ---- Folding ----

[<CompiledName "MapFold">]
let mapFold (mapping : 'State -> 'T -> 'Result * 'State) (state : 'State) (array : FlatList<'T>) =
    let mapStruct state item =
        let result, next = mapping state item
        struct (result, next)

    let struct (results, final) = FSharp.Collections.Immutable.FlatList.mapFold mapStruct state array
    results, final

[<CompiledName "MapFoldBack">]
let mapFoldBack (mapping : 'T -> 'State -> 'Result * 'State) (array : FlatList<'T>) (state : 'State) =
    let mapStruct item state =
        let result, next = mapping item state
        struct (result, next)

    let struct (results, final) = FSharp.Collections.Immutable.FlatList.mapFoldBack mapStruct array state
    results, final

// ---- Partitioning and grouping ----

[<CompiledName "CountBy">]
let countBy (projection : 'T -> 'Key) (array : FlatList<'T>) =
    Arrays.countBy referencePair projection (unwrapArray (nameof array) array)
    |> wrapArray

[<CompiledName "GroupBy">]
let groupBy (projection : 'T -> 'Key) (array : FlatList<'T>) =
    Arrays.groupBy (fun key items -> key, wrapArray items) projection (unwrapArray (nameof array) array)
    |> wrapArray

[<CompiledName "Partition">]
let partition predicate (array : FlatList<'T>) =
    let struct (accepted, rejected) = FSharp.Collections.Immutable.FlatList.partition predicate array
    accepted, rejected

[<CompiledName "PartitionWith">]
let partitionWith (partitioner : 'T -> Choice<'T1, 'T2>) (array : FlatList<'T>) =
    let struct (first, second) = FSharp.Collections.Immutable.FlatList.partitionWith partitioner array
    first, second

[<CompiledName "SplitAt">]
let splitAt index (array : FlatList<'T>) =
    let struct (before, after) = FSharp.Collections.Immutable.FlatList.splitAt index array
    before, after

// As in the module above: the functions that build a tuple per element share their algorithm with the default module,
// and the others delegate to it and repack the tuples
[<RequireQualifiedAccess>]
module Parallel =

    // ---- Transformation ----

    [<CompiledName "Zip">]
    let zip (array1 : FlatList<'T1>) (array2 : FlatList<'T2>) =
        let source1 = unwrapArray (nameof array1) array1
        let source2 = unwrapArray (nameof array2) array2
        ArrayChecks.sameLengths (nameof array1) source1 (nameof array2) source2

        ParallelArrays.zip referencePair source1 source2
        |> wrapArray

    // ---- Partitioning and grouping ----

    [<CompiledName "GroupBy">]
    let groupBy (projection : 'T -> 'Key) (array : FlatList<'T>) =
        ParallelArrays.groupBy (fun key items -> key, wrapArray items) projection (unwrapArray (nameof array) array)
        |> wrapArray

    [<CompiledName "Partition">]
    let partition predicate (array : FlatList<'T>) =
        let struct (accepted, rejected) =
            FSharp.Collections.Immutable.FlatList.Parallel.partition predicate array

        accepted, rejected

    [<CompiledName "PartitionWith">]
    let partitionWith (partitioner : 'T -> Choice<'T1, 'T2>) (array : FlatList<'T>) =
        let struct (first, second) =
            FSharp.Collections.Immutable.FlatList.Parallel.partitionWith partitioner array

        first, second
