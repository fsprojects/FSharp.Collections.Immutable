[<RequireQualifiedAccess>]
module FSharp.Collections.Immutable.Tuple.Stack

open FSharp.Collections.Immutable
open FSharp.Collections.Immutable.StackConstruction

// The functions that build a tuple per element share their algorithm with the default module and only pass another
// tuple constructor; the others delegate to the default module and repack the tuples, which costs no second pass.

// ---- Creation ----

[<CompiledName "Unfold">]
let unfold (generator : 'State -> ('T * 'State) voption) (state : 'State) =
    let generateStruct state =
        match generator state with
        | ValueSome (item, next) -> ValueSome (struct (item, next))
        | ValueNone -> ValueNone

    FSharp.Collections.Immutable.Stack.unfold generateStruct state

// ---- Access ----

[<CompiledName "TryPeekPop">]
let tryUncons (stack : Stack<'T>) =
    match FSharp.Collections.Immutable.Stack.tryUncons stack with
    | ValueSome (struct (head, tail)) -> ValueSome (head, tail)
    | ValueNone -> ValueNone

[<CompiledName "PeekPop">]
let uncons (stack : Stack<'T>) =
    let struct (head, tail) = FSharp.Collections.Immutable.Stack.uncons stack
    head, tail

[<return : Struct>]
[<CompiledName "ConsPattern">]
let (|Cons|_|) (stack : Stack<'T>) = tryUncons stack

// ---- Transformation ----

[<CompiledName "AllPairs">]
let allPairs (stack1 : Stack<'T1>) (stack2 : Stack<'T2>) =
    Arrays.allPairs referencePair (Snapshot.ofSeq (nameof stack1) stack1) (Snapshot.ofSeq (nameof stack2) stack2)
    |> ofArrayUnchecked

[<CompiledName "Indexed">]
let indexed (stack : Stack<'T>) =
    Arrays.indexed referencePair (Snapshot.ofSeq (nameof stack) stack)
    |> ofArrayUnchecked

[<CompiledName "Pairwise">]
let pairwise (stack : Stack<'T>) =
    Arrays.pairwise referencePair (Snapshot.ofSeq (nameof stack) stack)
    |> ofArrayUnchecked

[<CompiledName "Unzip">]
let unzip (stack : Stack<'T1 * 'T2>) =
    let struct (first, second) =
        Arrays.unzip (fun (item1, item2) -> struct (item1, item2)) (Snapshot.ofSeq (nameof stack) stack)

    ofArrayUnchecked first, ofArrayUnchecked second

[<CompiledName "Unzip3">]
let unzip3 (stack : Stack<'T1 * 'T2 * 'T3>) =
    let struct (first, second, third) =
        Arrays.unzip3 (fun (item1, item2, item3) -> struct (item1, item2, item3)) (Snapshot.ofSeq (nameof stack) stack)

    ofArrayUnchecked first, ofArrayUnchecked second, ofArrayUnchecked third

[<CompiledName "Zip">]
let zip (stack1 : Stack<'T1>) (stack2 : Stack<'T2>) =
    let array1 = Snapshot.ofSeq (nameof stack1) stack1
    let array2 = Snapshot.ofSeq (nameof stack2) stack2
    ListChecks.sameLengths (nameof stack1) array1.Length (nameof stack2) array2.Length
    Arrays.zip referencePair array1 array2 |> ofArrayUnchecked

[<CompiledName "Zip3">]
let zip3 (stack1 : Stack<'T1>) (stack2 : Stack<'T2>) (stack3 : Stack<'T3>) =
    let array1 = Snapshot.ofSeq (nameof stack1) stack1
    let array2 = Snapshot.ofSeq (nameof stack2) stack2
    let array3 = Snapshot.ofSeq (nameof stack3) stack3
    ListChecks.zip3 (nameof stack1) array1.Length (nameof stack2) array2.Length (nameof stack3) array3.Length

    Arrays.zip3 referenceTriple array1 array2 array3
    |> ofArrayUnchecked

// ---- Folding ----

[<CompiledName "MapFold">]
let mapFold (mapping : 'State -> 'T -> 'Result * 'State) (state : 'State) (stack : Stack<'T>) =
    let mapStruct state item =
        let result, next = mapping state item
        struct (result, next)

    let struct (results, final) = FSharp.Collections.Immutable.Stack.mapFold mapStruct state stack
    results, final

[<CompiledName "MapFoldBack">]
let mapFoldBack (mapping : 'T -> 'State -> 'Result * 'State) (stack : Stack<'T>) (state : 'State) =
    let mapStruct item state =
        let result, next = mapping item state
        struct (result, next)

    let struct (results, final) = FSharp.Collections.Immutable.Stack.mapFoldBack mapStruct stack state
    results, final

// ---- Partitioning and grouping ----

[<CompiledName "CountBy">]
let countBy (projection : 'T -> 'Key) (stack : Stack<'T>) =
    Arrays.countBy referencePair projection (Snapshot.ofSeq (nameof stack) stack)
    |> ofArrayUnchecked

[<CompiledName "GroupBy">]
let groupBy (projection : 'T -> 'Key) (stack : Stack<'T>) =
    Arrays.groupBy (fun key items -> key, ofArrayUnchecked items) projection (Snapshot.ofSeq (nameof stack) stack)
    |> ofArrayUnchecked

[<CompiledName "Partition">]
let partition predicate (stack : Stack<'T>) =
    let struct (accepted, rejected) = FSharp.Collections.Immutable.Stack.partition predicate stack
    accepted, rejected

[<CompiledName "PartitionWith">]
let partitionWith (partitioner : 'T -> Choice<'T1, 'T2>) (stack : Stack<'T>) =
    let struct (first, second) = FSharp.Collections.Immutable.Stack.partitionWith partitioner stack
    first, second

[<CompiledName "SplitAt">]
let splitAt index (stack : Stack<'T>) =
    let struct (before, after) = FSharp.Collections.Immutable.Stack.splitAt index stack
    before, after
