[<RequireQualifiedAccess>]
module FSharp.Collections.Immutable.Tuple.IndexedList

open System.Collections.Immutable
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

    FSharp.Collections.Immutable.IndexedList.unfold generateStruct state

// ---- Access ----

[<CompiledName "TryUncons">]
let tryUncons (list : IndexedList<'T>) =
    match FSharp.Collections.Immutable.IndexedList.tryUncons list with
    | ValueSome (struct (head, tail)) -> ValueSome (head, tail)
    | ValueNone -> ValueNone

[<CompiledName "Uncons">]
let uncons (list : IndexedList<'T>) =
    let struct (head, tail) = FSharp.Collections.Immutable.IndexedList.uncons list
    head, tail

[<return : Struct>]
[<CompiledName "ConsPattern">]
let (|Cons|_|) (list : IndexedList<'T>) = tryUncons list

// ---- Transformation ----

[<CompiledName "AllPairs">]
let allPairs (list1 : IndexedList<'T1>) (list2 : IndexedList<'T2>) =
    Arrays.allPairs referencePair (Snapshot.ofSeq (nameof list1) list1) (Snapshot.ofSeq (nameof list2) list2)
    |> ImmutableList.CreateRange

[<CompiledName "Indexed">]
let indexed (list : IndexedList<'T>) =
    Arrays.indexed referencePair (Snapshot.ofSeq (nameof list) list)
    |> ImmutableList.CreateRange

[<CompiledName "Pairwise">]
let pairwise (list : IndexedList<'T>) =
    Arrays.pairwise referencePair (Snapshot.ofSeq (nameof list) list)
    |> ImmutableList.CreateRange

[<CompiledName "Unzip">]
let unzip (list : IndexedList<'T1 * 'T2>) =
    let struct (first, second) =
        Arrays.unzip (fun (item1, item2) -> struct (item1, item2)) (Snapshot.ofSeq (nameof list) list)

    ImmutableList.CreateRange first, ImmutableList.CreateRange second

[<CompiledName "Unzip3">]
let unzip3 (list : IndexedList<'T1 * 'T2 * 'T3>) =
    let struct (first, second, third) =
        Arrays.unzip3 (fun (item1, item2, item3) -> struct (item1, item2, item3)) (Snapshot.ofSeq (nameof list) list)

    ImmutableList.CreateRange first, ImmutableList.CreateRange second, ImmutableList.CreateRange third

[<CompiledName "Zip">]
let zip (list1 : IndexedList<'T1>) (list2 : IndexedList<'T2>) =
    checkNotNull (nameof list1) list1
    checkNotNull (nameof list2) list2
    ListChecks.sameLengths (nameof list1) list1.Count (nameof list2) list2.Count

    Arrays.zip referencePair (Snapshot.ofSeq (nameof list1) list1) (Snapshot.ofSeq (nameof list2) list2)
    |> ImmutableList.CreateRange

[<CompiledName "Zip3">]
let zip3 (list1 : IndexedList<'T1>) (list2 : IndexedList<'T2>) (list3 : IndexedList<'T3>) =
    checkNotNull (nameof list1) list1
    checkNotNull (nameof list2) list2
    checkNotNull (nameof list3) list3
    ListChecks.zip3 (nameof list1) list1.Count (nameof list2) list2.Count (nameof list3) list3.Count

    Arrays.zip3
        referenceTriple
        (Snapshot.ofSeq (nameof list1) list1)
        (Snapshot.ofSeq (nameof list2) list2)
        (Snapshot.ofSeq (nameof list3) list3)
    |> ImmutableList.CreateRange

// ---- Folding ----

[<CompiledName "MapFold">]
let mapFold (mapping : 'State -> 'T -> 'Result * 'State) (state : 'State) (list : IndexedList<'T>) =
    let mapStruct state item =
        let result, next = mapping state item
        struct (result, next)

    let struct (results, final) = FSharp.Collections.Immutable.IndexedList.mapFold mapStruct state list
    results, final

[<CompiledName "MapFoldBack">]
let mapFoldBack (mapping : 'T -> 'State -> 'Result * 'State) (list : IndexedList<'T>) (state : 'State) =
    let mapStruct item state =
        let result, next = mapping item state
        struct (result, next)

    let struct (results, final) = FSharp.Collections.Immutable.IndexedList.mapFoldBack mapStruct list state
    results, final

// ---- Partitioning and grouping ----

[<CompiledName "CountBy">]
let countBy (projection : 'T -> 'Key) (list : IndexedList<'T>) =
    Arrays.countBy referencePair projection (Snapshot.ofSeq (nameof list) list)
    |> ImmutableList.CreateRange

[<CompiledName "GroupBy">]
let groupBy (projection : 'T -> 'Key) (list : IndexedList<'T>) =
    Arrays.groupBy (fun key items -> key, ImmutableList.CreateRange items) projection (Snapshot.ofSeq (nameof list) list)
    |> ImmutableList.CreateRange

[<CompiledName "Partition">]
let partition predicate (list : IndexedList<'T>) =
    let struct (accepted, rejected) = FSharp.Collections.Immutable.IndexedList.partition predicate list
    accepted, rejected

[<CompiledName "PartitionWith">]
let partitionWith (partitioner : 'T -> Choice<'T1, 'T2>) (list : IndexedList<'T>) =
    let struct (first, second) = FSharp.Collections.Immutable.IndexedList.partitionWith partitioner list
    first, second

[<CompiledName "SplitAt">]
let splitAt index (list : IndexedList<'T>) =
    let struct (before, after) = FSharp.Collections.Immutable.IndexedList.splitAt index list
    before, after
