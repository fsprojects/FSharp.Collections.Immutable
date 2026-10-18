[<RequireQualifiedAccess>]
module FSharp.Collections.Immutable.Queue

open System
open System.Collections.Generic
open System.Collections.Immutable
open System.Linq
// Opened after System.Collections.Generic, so that Queue names the abbreviation of this library, not the mutable queue
open FSharp.Collections.Immutable

// The functions call a member of ImmutableQueue where it has the semantics of the List function, walk the elements with the
// struct enumerator of the queue, here or in the walks of Queues that IQueue shares, or take a snapshot of the elements in
// an array, apply an Array function or a shared algorithm and build a new queue from the fresh result (ADR 0001, D5). The
// checks of ListChecks throw what the List functions throw (D6), with the names of the parameters of this module; the
// lengths that they check come from Lengths.queueUpTo, which counts only as far as a check needs.

/// A queue of the elements of a sequence that a Seq function sorts when it is read: the buffer reads it once, and the queue
/// dequeues the elements without reversing a back stack first.
let private ofSorted (sorted : seq<'T>) = Queues.ofBuffer (ResizeArray sorted)

// ---- Creation ----

[<GeneralizableValue>]
[<CompiledName "Empty">]
let empty<'T> : Queue<'T> = ImmutableQueue<'T>.Empty

[<CompiledName "Initialize">]
let init length (initializer : int -> 'T) =
    // List.init reports the length under the name "count"
    ListChecks.nonNegative "count" length
    Array.init length initializer |> Queues.ofFreshArray

[<CompiledName "OfArray">]
let ofArray (array : 'T array) =
    checkNotNull (nameof array) array
    ImmutableQueue.Create<'T>(ReadOnlySpan<'T> array)

[<CompiledName "OfList">]
let ofList (list : 'T list) = List.toArray list |> Queues.ofFreshArray

[<CompiledName "OfReadOnlySpan">]
let ofReadOnlySpan (span : ReadOnlySpan<'T>) = ImmutableQueue.Create<'T> span

[<CompiledName "OfSeq">]
let ofSeq (source : seq<'T>) =
    // A default flat list given as a sequence is a null input; the BCL would fail while it enumerates it (ADR 0001, D6)
    checkNotNullOrDefaultSeq (nameof source) source

    match source with
    // An immutable queue cannot change, so it needs no copy
    | :? ImmutableQueue<'T> as queue -> queue
    | _ -> ImmutableQueue.CreateRange source

[<CompiledName "OfSpan">]
let ofSpan (span : Span<'T>) = ImmutableQueue.Create<'T>(Span<'T>.op_Implicit span)

[<CompiledName "Replicate">]
let replicate count (initial : 'T) =
    ListChecks.replicate (nameof count) count
    Array.create count initial |> Queues.ofFreshArray

[<CompiledName "Singleton">]
let singleton (value : 'T) = ImmutableQueue.Create<'T> value

[<CompiledName "Unfold">]
let unfold (generator : 'State -> struct ('T * 'State) voption) (state : 'State) =
    let items = ResizeArray<'T>()
    let mutable state = state
    let mutable go = true

    while go do
        match generator state with
        | ValueSome (struct (item, next)) ->
            items.Add item
            state <- next
        | ValueNone -> go <- false

    Queues.ofBuffer items

// ---- Properties ----

[<CompiledName "IsEmpty">]
let isEmpty (queue : Queue<'T>) =
    checkNotNull (nameof queue) queue
    queue.IsEmpty

[<CompiledName "Length">]
let length (queue : Queue<'T>) =
    checkNotNull (nameof queue) queue
    let mutable count = 0

    for _ in queue do
        count <- count + 1

    count

// ---- Access ----

[<CompiledName "ExactlyOne">]
let exactlyOne (queue : Queue<'T>) =
    checkNotNull (nameof queue) queue
    // List.exactlyOne reports its input under the name "source"
    ListChecks.exactlyOne "source" (Lengths.queueUpTo 2 queue)
    queue.Peek ()

[<CompiledName "Peek">]
let head (queue : Queue<'T>) =
    checkNotNull (nameof queue) queue
    ListChecks.notEmpty (nameof queue) (Lengths.queueUpTo 1 queue)
    queue.Peek ()

[<CompiledName "Last">]
let last (queue : Queue<'T>) =
    checkNotNull (nameof queue) queue
    ListChecks.notEmpty (nameof queue) (Lengths.queueUpTo 1 queue)
    Enumerable.Last queue

[<CompiledName "Dequeue">]
let tail (queue : Queue<'T>) =
    checkNotNull (nameof queue) queue
    ListChecks.notEmpty (nameof queue) (Lengths.queueUpTo 1 queue)
    queue.Dequeue ()

[<CompiledName "TryExactlyOne">]
let tryExactlyOne (queue : Queue<'T>) =
    checkNotNull (nameof queue) queue

    if Lengths.queueUpTo 2 queue = 1 then
        ValueSome (queue.Peek ())
    else
        ValueNone

[<CompiledName "TryPeek">]
let tryHead (queue : Queue<'T>) =
    checkNotNull (nameof queue) queue

    if queue.IsEmpty then
        ValueNone
    else
        ValueSome (queue.Peek ())

[<CompiledName "TryLast">]
let tryLast (queue : Queue<'T>) =
    checkNotNull (nameof queue) queue

    if queue.IsEmpty then
        ValueNone
    else
        ValueSome (Enumerable.Last queue)

[<CompiledName "TryDequeue">]
let tryTail (queue : Queue<'T>) =
    checkNotNull (nameof queue) queue

    if queue.IsEmpty then
        ValueNone
    else
        ValueSome (queue.Dequeue ())

[<CompiledName "TryPeekDequeue">]
let tryUncons (queue : Queue<'T>) =
    checkNotNull (nameof queue) queue

    if queue.IsEmpty then
        ValueNone
    else
        ValueSome (struct (queue.Peek (), queue.Dequeue ()))

[<CompiledName "PeekDequeue">]
let uncons (queue : Queue<'T>) =
    checkNotNull (nameof queue) queue
    // An empty queue throws what List.head throws
    ListChecks.notEmpty (nameof queue) (Lengths.queueUpTo 1 queue)
    struct (queue.Peek (), queue.Dequeue ())

[<return : Struct>]
[<CompiledName "ConsPattern">]
let (|Cons|_|) (queue : Queue<'T>) = tryUncons queue

[<CompiledName "NilPattern">]
let (|Nil|_|) (queue : Queue<'T>) =
    checkNotNull (nameof queue) queue
    queue.IsEmpty

// ---- Search ----

[<CompiledName "Contains">]
let contains (value : 'T) (source : Queue<'T>) =
    checkNotNull (nameof source) source
    let mutable enumerator = source.GetEnumerator ()
    let mutable found = false

    // F# equality with the value on the left, as List.contains compares
    while not found && enumerator.MoveNext () do
        found <- value = enumerator.Current

    found

[<CompiledName "Exists">]
let exists (predicate : 'T -> bool) (queue : Queue<'T>) =
    checkNotNull (nameof queue) queue
    let mutable enumerator = queue.GetEnumerator ()
    let mutable found = false

    while not found && enumerator.MoveNext () do
        found <- predicate enumerator.Current

    found

[<CompiledName "Exists2">]
let exists2 predicate (queue1 : Queue<'T1>) (queue2 : Queue<'T2>) =
    ListArrays.exists2 (nameof queue2) predicate (Snapshot.ofSeq (nameof queue1) queue1) (Snapshot.ofSeq (nameof queue2) queue2)

[<CompiledName "Find">]
let find (predicate : 'T -> bool) (queue : Queue<'T>) =
    checkNotNull (nameof queue) queue

    match Queues.firstMatch predicate queue with
    | ValueSome (struct (_, item)) -> item
    | ValueNone -> Raise.indexNotFound ()

[<CompiledName "FindBack">]
let findBack (predicate : 'T -> bool) (queue : Queue<'T>) =
    match Queues.lastMatch predicate queue with
    | ValueSome (struct (_, item)) -> item
    | ValueNone -> Raise.indexNotFound ()

[<CompiledName "FindIndex">]
let findIndex (predicate : 'T -> bool) (queue : Queue<'T>) =
    checkNotNull (nameof queue) queue

    match Queues.firstMatch predicate queue with
    | ValueSome (struct (index, _)) -> index
    | ValueNone -> Raise.indexNotFound ()

[<CompiledName "FindIndexBack">]
let findIndexBack (predicate : 'T -> bool) (queue : Queue<'T>) =
    match Queues.lastMatch predicate queue with
    | ValueSome (struct (index, _)) -> index
    | ValueNone -> Raise.indexNotFound ()

[<CompiledName "ForAll">]
let forall (predicate : 'T -> bool) (queue : Queue<'T>) =
    checkNotNull (nameof queue) queue
    let mutable enumerator = queue.GetEnumerator ()
    let mutable holds = true

    while holds && enumerator.MoveNext () do
        holds <- predicate enumerator.Current

    holds

[<CompiledName "ForAll2">]
let forall2 predicate (queue1 : Queue<'T1>) (queue2 : Queue<'T2>) =
    ListArrays.forall2
        (nameof queue1)
        (nameof queue2)
        predicate
        (Snapshot.ofSeq (nameof queue1) queue1)
        (Snapshot.ofSeq (nameof queue2) queue2)

[<CompiledName "Pick">]
let pick (chooser : 'T -> 'U voption) (queue : Queue<'T>) =
    checkNotNull (nameof queue) queue

    match Queues.firstChoice chooser queue with
    | ValueSome value -> value
    | ValueNone -> Raise.indexNotFound ()

[<CompiledName "TryFind">]
let tryFind (predicate : 'T -> bool) (queue : Queue<'T>) =
    checkNotNull (nameof queue) queue

    match Queues.firstMatch predicate queue with
    | ValueSome (struct (_, item)) -> ValueSome item
    | ValueNone -> ValueNone

[<CompiledName "TryFindBack">]
let tryFindBack (predicate : 'T -> bool) (queue : Queue<'T>) =
    match Queues.lastMatch predicate queue with
    | ValueSome (struct (_, item)) -> ValueSome item
    | ValueNone -> ValueNone

[<CompiledName "TryFindIndex">]
let tryFindIndex (predicate : 'T -> bool) (queue : Queue<'T>) =
    checkNotNull (nameof queue) queue

    match Queues.firstMatch predicate queue with
    | ValueSome (struct (index, _)) -> ValueSome index
    | ValueNone -> ValueNone

[<CompiledName "TryFindIndexBack">]
let tryFindIndexBack (predicate : 'T -> bool) (queue : Queue<'T>) =
    match Queues.lastMatch predicate queue with
    | ValueSome (struct (index, _)) -> ValueSome index
    | ValueNone -> ValueNone

[<CompiledName "TryPick">]
let tryPick (chooser : 'T -> 'U voption) (queue : Queue<'T>) =
    checkNotNull (nameof queue) queue
    Queues.firstChoice chooser queue

// ---- Transformation ----

[<CompiledName "AllPairs">]
let allPairs (queue1 : Queue<'T1>) (queue2 : Queue<'T2>) =
    Arrays.allPairs structPair (Snapshot.ofSeq (nameof queue1) queue1) (Snapshot.ofSeq (nameof queue2) queue2)
    |> Queues.ofFreshArray

[<CompiledName "Append">]
let append (queue1 : Queue<'T>) (queue2 : Queue<'T>) =
    checkNotNull (nameof queue1) queue1
    checkNotNull (nameof queue2) queue2

    if queue1.IsEmpty then
        queue2
    else
        // Enqueueing keeps the elements of the first queue where they are
        let mutable result = queue1

        for item in queue2 do
            result <- result.Enqueue item

        result

[<CompiledName "Choose">]
let choose (chooser : 'T -> 'U voption) (queue : Queue<'T>) =
    checkNotNull (nameof queue) queue
    let chosen = ResizeArray<'U>()

    for item in queue do
        match chooser item with
        | ValueSome value -> chosen.Add value
        | ValueNone -> ()

    Queues.ofBuffer chosen

[<CompiledName "ChunkBySize">]
let chunkBySize chunkSize (queue : Queue<'T>) =
    checkNotNull (nameof queue) queue
    ListChecks.positive (nameof chunkSize) chunkSize

    Array.chunkBySize chunkSize (Snapshot.ofSeq (nameof queue) queue)
    |> Array.map Queues.ofFreshArray
    |> Queues.ofFreshArray

[<CompiledName "Collect">]
let collect (mapping : 'T -> Queue<'U>) (queue : Queue<'T>) =
    checkNotNull (nameof queue) queue
    let collected = ResizeArray<'U>()

    for item in queue do
        collected.AddRange (mapping item)

    Queues.ofBuffer collected

[<CompiledName "Concat">]
let concat (queues : seq<Queue<'T>>) =
    checkNotNullOrDefaultSeq (nameof queues) queues
    let concatenated = ResizeArray<'T>()

    for queue in queues do
        // A null queue is a null input, as a null indexed list is for IndexedList.concat (ADR 0001, D6)
        checkNotNull (nameof queues) queue

        for item in queue do
            concatenated.Add item

    Queues.ofBuffer concatenated

[<CompiledName "Enqueue">]
let enqueue (value : 'T) (queue : Queue<'T>) =
    checkNotNull (nameof queue) queue
    queue.Enqueue value

[<CompiledName "EnqueueMany">]
let enqueueMany (values : seq<'T>) (queue : Queue<'T>) =
    checkNotNullOrDefaultSeq (nameof values) values
    checkNotNull (nameof queue) queue
    let mutable result = queue

    for value in values do
        result <- result.Enqueue value

    result

[<CompiledName "Filter">]
let filter (predicate : 'T -> bool) (queue : Queue<'T>) =
    checkNotNull (nameof queue) queue
    let kept = ResizeArray<'T>()

    for item in queue do
        if predicate item then
            kept.Add item

    Queues.ofBuffer kept

[<CompiledName "Indexed">]
let indexed (queue : Queue<'T>) =
    Arrays.indexed structPair (Snapshot.ofSeq (nameof queue) queue)
    |> Queues.ofFreshArray

[<CompiledName "Map">]
let map (mapping : 'T -> 'U) (queue : Queue<'T>) =
    Array.map mapping (Snapshot.ofSeq (nameof queue) queue)
    |> Queues.ofFreshArray

[<CompiledName "Map2">]
let map2 (mapping : 'T1 -> 'T2 -> 'U) (queue1 : Queue<'T1>) (queue2 : Queue<'T2>) =
    ListArrays.map2
        (nameof queue1)
        (nameof queue2)
        mapping
        (Snapshot.ofSeq (nameof queue1) queue1)
        (Snapshot.ofSeq (nameof queue2) queue2)
    |> Queues.ofFreshArray

[<CompiledName "Map3">]
let map3 (mapping : 'T1 -> 'T2 -> 'T3 -> 'U) (queue1 : Queue<'T1>) (queue2 : Queue<'T2>) (queue3 : Queue<'T3>) =
    ListArrays.map3
        (nameof queue1)
        (nameof queue2)
        (nameof queue3)
        mapping
        (Snapshot.ofSeq (nameof queue1) queue1)
        (Snapshot.ofSeq (nameof queue2) queue2)
        (Snapshot.ofSeq (nameof queue3) queue3)
    |> Queues.ofFreshArray

[<CompiledName "MapIndexed">]
let mapi (mapping : int -> 'T -> 'U) (queue : Queue<'T>) =
    Array.mapi mapping (Snapshot.ofSeq (nameof queue) queue)
    |> Queues.ofFreshArray

[<CompiledName "MapIndexed2">]
let mapi2 (mapping : int -> 'T1 -> 'T2 -> 'U) (queue1 : Queue<'T1>) (queue2 : Queue<'T2>) =
    ListArrays.mapi2
        (nameof queue1)
        (nameof queue2)
        mapping
        (Snapshot.ofSeq (nameof queue1) queue1)
        (Snapshot.ofSeq (nameof queue2) queue2)
    |> Queues.ofFreshArray

[<CompiledName "Pairwise">]
let pairwise (queue : Queue<'T>) =
    Arrays.pairwise structPair (Snapshot.ofSeq (nameof queue) queue)
    |> Queues.ofFreshArray

[<CompiledName "Permute">]
let permute indexMap (queue : Queue<'T>) =
    // List.permute applies the algorithm of Array.permute to a copy of the list, with the same checks
    Array.permute indexMap (Snapshot.ofSeq (nameof queue) queue)
    |> Queues.ofFreshArray

[<CompiledName "Reverse">]
let rev (queue : Queue<'T>) =
    Array.rev (Snapshot.ofSeq (nameof queue) queue)
    |> Queues.ofFreshArray

[<CompiledName "Transpose">]
let transpose (queues : seq<Queue<'T>>) =
    checkNotNullOrDefaultSeq (nameof queues) queues

    let arrays =
        queues
        |> Seq.map (fun queue -> Snapshot.ofSeq (nameof queues) queue)
        |> Seq.toArray

    // List.transpose names the lists "list.[0]", "list.[1]", …; this module names them "queue.[0]", "queue.[1]", …
    // Once their lengths agree, Array.transpose agrees with List.transpose
    ListChecks.transpose "queue" (arrays |> Array.map _.Length)

    Array.transpose arrays
    |> Array.map Queues.ofFreshArray
    |> Queues.ofFreshArray

[<CompiledName "Unzip">]
let unzip (queue : Queue<struct ('T1 * 'T2)>) =
    let struct (first, second) = Arrays.unzip id (Snapshot.ofSeq (nameof queue) queue)
    struct (Queues.ofFreshArray first, Queues.ofFreshArray second)

[<CompiledName "Unzip3">]
let unzip3 (queue : Queue<struct ('T1 * 'T2 * 'T3)>) =
    let struct (first, second, third) = Arrays.unzip3 id (Snapshot.ofSeq (nameof queue) queue)
    struct (Queues.ofFreshArray first, Queues.ofFreshArray second, Queues.ofFreshArray third)

[<CompiledName "Where">]
let where (predicate : 'T -> bool) (queue : Queue<'T>) = filter predicate queue

[<CompiledName "Windowed">]
let windowed windowSize (queue : Queue<'T>) =
    checkNotNull (nameof queue) queue
    ListChecks.positive (nameof windowSize) windowSize

    Array.windowed windowSize (Snapshot.ofSeq (nameof queue) queue)
    |> Array.map Queues.ofFreshArray
    |> Queues.ofFreshArray

[<CompiledName "Zip">]
let zip (queue1 : Queue<'T1>) (queue2 : Queue<'T2>) =
    let array1 = Snapshot.ofSeq (nameof queue1) queue1
    let array2 = Snapshot.ofSeq (nameof queue2) queue2
    ListChecks.sameLengths (nameof queue1) array1.Length (nameof queue2) array2.Length
    Arrays.zip structPair array1 array2 |> Queues.ofFreshArray

[<CompiledName "Zip3">]
let zip3 (queue1 : Queue<'T1>) (queue2 : Queue<'T2>) (queue3 : Queue<'T3>) =
    let array1 = Snapshot.ofSeq (nameof queue1) queue1
    let array2 = Snapshot.ofSeq (nameof queue2) queue2
    let array3 = Snapshot.ofSeq (nameof queue3) queue3
    ListChecks.zip3 (nameof queue1) array1.Length (nameof queue2) array2.Length (nameof queue3) array3.Length
    Arrays.zip3 structTriple array1 array2 array3
    |> Queues.ofFreshArray

// ---- Folding ----

// The inline functions can use public members only, so they check the queue themselves and count its elements while
// they add them; the empty queue throws what List.average and List.averageBy throw, with the name "source"
[<CompiledName "Average">]
let inline average (queue : Queue< ^T >) : ^T =
    if obj.ReferenceEquals (queue, null) then
        nullArg (nameof queue)

    if queue.IsEmpty then
        invalidArg "source" ErrorStrings.InputSequenceEmpty

    let mutable sum = LanguagePrimitives.GenericZero< ^T>
    let mutable count = 0

    for item in queue do
        sum <- Checked.(+) sum item
        count <- count + 1

    LanguagePrimitives.DivideByInt< ^T> sum count

[<CompiledName "AverageBy">]
let inline averageBy ([<InlineIfLambda>] projection : 'T -> ^U) (queue : Queue<'T>) : ^U =
    if obj.ReferenceEquals (queue, null) then
        nullArg (nameof queue)

    if queue.IsEmpty then
        invalidArg "source" ErrorStrings.InputSequenceEmpty

    let mutable sum = LanguagePrimitives.GenericZero< ^U>
    let mutable count = 0

    for item in queue do
        sum <- Checked.(+) sum (projection item)
        count <- count + 1

    LanguagePrimitives.DivideByInt< ^U> sum count

[<CompiledName "CompareWith">]
let compareWith (comparer : 'T -> 'T -> int) (queue1 : Queue<'T>) (queue2 : Queue<'T>) =
    // Array.compareWith compares as List.compareWith does: the first difference, then the lengths
    Array.compareWith comparer (Snapshot.ofSeq (nameof queue1) queue1) (Snapshot.ofSeq (nameof queue2) queue2)

[<CompiledName "Fold">]
let fold (folder : 'State -> 'T -> 'State) (state : 'State) (queue : Queue<'T>) =
    checkNotNull (nameof queue) queue
    let mutable state = state

    for item in queue do
        state <- folder state item

    state

[<CompiledName "Fold2">]
let fold2 (folder : 'State -> 'T1 -> 'T2 -> 'State) (state : 'State) (queue1 : Queue<'T1>) (queue2 : Queue<'T2>) =
    ListArrays.fold2
        (nameof queue1)
        (nameof queue2)
        folder
        state
        (Snapshot.ofSeq (nameof queue1) queue1)
        (Snapshot.ofSeq (nameof queue2) queue2)

[<CompiledName "FoldBack">]
let foldBack (folder : 'T -> 'State -> 'State) (queue : Queue<'T>) (state : 'State) =
    Array.foldBack folder (Snapshot.ofSeq (nameof queue) queue) state

[<CompiledName "FoldBack2">]
let foldBack2 (folder : 'T1 -> 'T2 -> 'State -> 'State) (queue1 : Queue<'T1>) (queue2 : Queue<'T2>) (state : 'State) =
    let array1 = Snapshot.ofSeq (nameof queue1) queue1
    let array2 = Snapshot.ofSeq (nameof queue2) queue2
    // List.foldBack2 checks the lengths before it calls the folder
    ListChecks.foldBack2 (nameof queue1) array1.Length (nameof queue2) array2.Length
    Array.foldBack2 folder array1 array2 state

[<CompiledName "Iterate">]
let iter (action : 'T -> unit) (queue : Queue<'T>) =
    checkNotNull (nameof queue) queue

    for item in queue do
        action item

[<CompiledName "Iterate2">]
let iter2 (action : 'T1 -> 'T2 -> unit) (queue1 : Queue<'T1>) (queue2 : Queue<'T2>) =
    ListArrays.iter2
        (nameof queue1)
        (nameof queue2)
        action
        (Snapshot.ofSeq (nameof queue1) queue1)
        (Snapshot.ofSeq (nameof queue2) queue2)

[<CompiledName "IterateIndexed">]
let iteri (action : int -> 'T -> unit) (queue : Queue<'T>) =
    checkNotNull (nameof queue) queue
    let mutable index = 0

    for item in queue do
        action index item
        index <- index + 1

[<CompiledName "IterateIndexed2">]
let iteri2 (action : int -> 'T1 -> 'T2 -> unit) (queue1 : Queue<'T1>) (queue2 : Queue<'T2>) =
    ListArrays.iteri2
        (nameof queue1)
        (nameof queue2)
        action
        (Snapshot.ofSeq (nameof queue1) queue1)
        (Snapshot.ofSeq (nameof queue2) queue2)

[<CompiledName "MapFold">]
let mapFold (mapping : 'State -> 'T -> struct ('Result * 'State)) (state : 'State) (queue : Queue<'T>) =
    let source = Snapshot.ofSeq (nameof queue) queue
    let results = Array.zeroCreate source.Length
    let mutable state = state

    for index = 0 to source.Length - 1 do
        let struct (result, next) = mapping state source[index]
        results[index] <- result
        state <- next

    struct (Queues.ofFreshArray results, state)

[<CompiledName "MapFoldBack">]
let mapFoldBack (mapping : 'T -> 'State -> struct ('Result * 'State)) (queue : Queue<'T>) (state : 'State) =
    let source = Snapshot.ofSeq (nameof queue) queue
    let results = Array.zeroCreate source.Length
    let mutable state = state

    for index = source.Length - 1 downto 0 do
        let struct (result, next) = mapping source[index] state
        results[index] <- result
        state <- next

    struct (Queues.ofFreshArray results, state)

// Seq.max and the others compare as the List functions do once the queue has elements, so the List checks come first

[<CompiledName "Max">]
let max (queue : Queue<'T>) =
    checkNotNull (nameof queue) queue
    ListChecks.notEmptySequence (nameof queue) (Lengths.queueUpTo 1 queue)
    Seq.max queue

[<CompiledName "MaxBy">]
let maxBy (projection : 'T -> 'U) (queue : Queue<'T>) =
    checkNotNull (nameof queue) queue
    ListChecks.notEmptySequence (nameof queue) (Lengths.queueUpTo 1 queue)
    Seq.maxBy projection queue

[<CompiledName "Min">]
let min (queue : Queue<'T>) =
    checkNotNull (nameof queue) queue
    ListChecks.notEmptySequence (nameof queue) (Lengths.queueUpTo 1 queue)
    Seq.min queue

[<CompiledName "MinBy">]
let minBy (projection : 'T -> 'U) (queue : Queue<'T>) =
    checkNotNull (nameof queue) queue
    ListChecks.notEmptySequence (nameof queue) (Lengths.queueUpTo 1 queue)
    Seq.minBy projection queue

[<CompiledName "Reduce">]
let reduce (reduction : 'T -> 'T -> 'T) (queue : Queue<'T>) =
    checkNotNull (nameof queue) queue
    ListChecks.notEmpty (nameof queue) (Lengths.queueUpTo 1 queue)
    Seq.reduce reduction queue

[<CompiledName "ReduceBack">]
let reduceBack (reduction : 'T -> 'T -> 'T) (queue : Queue<'T>) =
    checkNotNull (nameof queue) queue
    ListChecks.notEmpty (nameof queue) (Lengths.queueUpTo 1 queue)
    Array.reduceBack reduction (Snapshot.ofSeq (nameof queue) queue)

[<CompiledName "Scan">]
let scan (folder : 'State -> 'T -> 'State) (state : 'State) (queue : Queue<'T>) =
    Array.scan folder state (Snapshot.ofSeq (nameof queue) queue)
    |> Queues.ofFreshArray

[<CompiledName "ScanBack">]
let scanBack (folder : 'T -> 'State -> 'State) (queue : Queue<'T>) (state : 'State) =
    Array.scanBack folder (Snapshot.ofSeq (nameof queue) queue) state
    |> Queues.ofFreshArray

[<CompiledName "Sum">]
let inline sum (queue : Queue< ^T >) : ^T =
    if obj.ReferenceEquals (queue, null) then
        nullArg (nameof queue)

    let mutable sum = LanguagePrimitives.GenericZero< ^T>

    for item in queue do
        sum <- Checked.(+) sum item

    sum

[<CompiledName "SumBy">]
let inline sumBy ([<InlineIfLambda>] projection : 'T -> ^U) (queue : Queue<'T>) : ^U =
    if obj.ReferenceEquals (queue, null) then
        nullArg (nameof queue)

    let mutable sum = LanguagePrimitives.GenericZero< ^U>

    for item in queue do
        sum <- Checked.(+) sum (projection item)

    sum

// ---- Partitioning and grouping ----

[<CompiledName "CountBy">]
let countBy (projection : 'T -> 'Key) (queue : Queue<'T>) =
    Arrays.countBy structPair projection (Snapshot.ofSeq (nameof queue) queue)
    |> Queues.ofFreshArray

[<CompiledName "Distinct">]
let distinct (queue : Queue<'T>) =
    Array.distinct (Snapshot.ofSeq (nameof queue) queue)
    |> Queues.ofFreshArray

[<CompiledName "DistinctBy">]
let distinctBy (projection : 'T -> 'Key) (queue : Queue<'T>) =
    Array.distinctBy projection (Snapshot.ofSeq (nameof queue) queue)
    |> Queues.ofFreshArray

[<CompiledName "Except">]
let except (itemsToExclude : seq<'T>) (queue : Queue<'T>) =
    // List.except checks the items to exclude first and does not read them for an empty list
    checkNotNullOrDefaultSeq (nameof itemsToExclude) itemsToExclude
    checkNotNull (nameof queue) queue

    if queue.IsEmpty then
        queue
    else
        Array.except itemsToExclude (Snapshot.ofSeq (nameof queue) queue)
        |> Queues.ofFreshArray

[<CompiledName "GroupBy">]
let groupBy (projection : 'T -> 'Key) (queue : Queue<'T>) =
    Arrays.groupBy (fun key items -> struct (key, Queues.ofFreshArray items)) projection (Snapshot.ofSeq (nameof queue) queue)
    |> Queues.ofFreshArray

[<CompiledName "Partition">]
let partition (predicate : 'T -> bool) (queue : Queue<'T>) =
    let accepted, rejected = Array.partition predicate (Snapshot.ofSeq (nameof queue) queue)
    struct (Queues.ofFreshArray accepted, Queues.ofFreshArray rejected)

[<CompiledName "PartitionWith">]
let partitionWith (partitioner : 'T -> Choice<'T1, 'T2>) (queue : Queue<'T>) =
    let first, second = Array.partitionWith partitioner (Snapshot.ofSeq (nameof queue) queue)
    struct (Queues.ofFreshArray first, Queues.ofFreshArray second)

[<CompiledName "Skip">]
let skip count (queue : Queue<'T>) =
    checkNotNull (nameof queue) queue
    let mutable rest = queue
    let mutable skipped = 0

    while skipped < count && not rest.IsEmpty do
        rest <- rest.Dequeue ()
        skipped <- skipped + 1

    // The loop stops at the count or at the end of the queue, where the number of skipped elements is the length; a
    // count beyond the length throws what List.skip throws
    ListChecks.skip (nameof count) count skipped
    rest

[<CompiledName "SkipLast">]
let skipLast count (queue : Queue<'T>) =
    let array = Snapshot.ofSeq (nameof queue) queue
    // The checks of List.skip
    ListChecks.skip (nameof count) count array.Length

    if count <= 0 then
        queue
    else
        ImmutableQueue.Create<'T>(ReadOnlySpan<'T>(array, 0, array.Length - count))

[<CompiledName "SkipWhile">]
let skipWhile (predicate : 'T -> bool) (queue : Queue<'T>) =
    checkNotNull (nameof queue) queue
    let mutable rest = queue

    while not rest.IsEmpty && predicate (rest.Peek ()) do
        rest <- rest.Dequeue ()

    rest

[<CompiledName "SplitAt">]
let splitAt index (queue : Queue<'T>) =
    checkNotNull (nameof queue) queue
    let before = ResizeArray<'T>()
    let mutable rest = queue

    while before.Count < index && not rest.IsEmpty do
        before.Add (rest.Peek ())
        rest <- rest.Dequeue ()

    // The loop stops at the index or at the end of the queue, where the number of taken elements is the length
    ListChecks.splitAt (nameof index) index before.Count
    struct ((if rest.IsEmpty then queue else Queues.ofBuffer before), rest)

[<CompiledName "SplitInto">]
let splitInto count (queue : Queue<'T>) =
    checkNotNull (nameof queue) queue
    ListChecks.positive (nameof count) count

    Array.splitInto count (Snapshot.ofSeq (nameof queue) queue)
    |> Array.map Queues.ofFreshArray
    |> Queues.ofFreshArray

[<CompiledName "Take">]
let take count (queue : Queue<'T>) =
    checkNotNull (nameof queue) queue
    let struct (items, more) = Queues.front count queue
    // The front stops at the count or at the end of the queue, where the number of taken elements is the length
    ListChecks.take (nameof count) count items.Count
    if more then Queues.ofBuffer items else queue

[<CompiledName "TakeLast">]
let takeLast count (queue : Queue<'T>) =
    let array = Snapshot.ofSeq (nameof queue) queue
    // The checks of List.take
    ListChecks.take (nameof count) count array.Length

    if count = array.Length then
        queue
    else
        ImmutableQueue.Create<'T>(ReadOnlySpan<'T>(array, array.Length - count, count))

[<CompiledName "TakeWhile">]
let takeWhile (predicate : 'T -> bool) (queue : Queue<'T>) =
    checkNotNull (nameof queue) queue
    let items = ResizeArray<'T>()
    let mutable enumerator = queue.GetEnumerator ()
    let mutable more = enumerator.MoveNext ()

    while more && predicate enumerator.Current do
        items.Add enumerator.Current
        more <- enumerator.MoveNext ()

    if more then Queues.ofBuffer items else queue

[<CompiledName "Truncate">]
let truncate count (queue : Queue<'T>) =
    checkNotNull (nameof queue) queue
    let struct (items, more) = Queues.front count queue
    if more then Queues.ofBuffer items else queue

// ---- Sorting ----

// The sorts are stable, as the sorts of List are (ADR 0001, D5). The functions that compare the elements, or that call the
// projection in the comparison as List.sortByDescending does, sort with the Seq functions of FSharp.Core, which share the
// stable sort of List: the same comparisons run in Array.Sort, which wraps the exception of a comparison in an
// InvalidOperationException (D6). Seq.sortDescending and Seq.sortByDescending are inline and box the elements in every
// comparison once they are compiled into a generic function, so the two descending sorts call Seq.sortWith through
// DescendingSorts, with the cached comparer. sortBy and sortWithComparer sort with LINQ, which computes the keys first, as
// List.sortBy does, and wraps the exception of a comparison as Array.Sort does. A queue of one element is returned
// without a call of the projection or the comparer, as List returns a list of one element.

[<CompiledName "Sort">]
let sort (queue : Queue<'T>) =
    checkNotNull (nameof queue) queue

    if Lengths.queueUpTo 2 queue <= 1 then
        queue
    else
        ofSorted (Seq.sort queue)

[<CompiledName "SortBy">]
let sortBy (projection : 'T -> 'Key) (queue : Queue<'T>) =
    checkNotNull (nameof queue) queue

    if Lengths.queueUpTo 2 queue <= 1 then
        queue
    else
        queue.OrderBy(Func<'T, 'Key> projection, DefaultComparer<'Key>.Instance).ToArray()
        |> Queues.ofFreshArray

[<CompiledName "SortByDescending">]
let sortByDescending (projection : 'T -> 'Key) (queue : Queue<'T>) =
    checkNotNull (nameof queue) queue

    if Lengths.queueUpTo 2 queue <= 1 then
        queue
    else
        ofSorted (DescendingSorts.sortByDescending projection queue)

[<CompiledName "SortDescending">]
let sortDescending (queue : Queue<'T>) =
    checkNotNull (nameof queue) queue

    if Lengths.queueUpTo 2 queue <= 1 then
        queue
    else
        ofSorted (DescendingSorts.sortDescending queue)

[<CompiledName "SortWith">]
let sortWith (comparer : 'T -> 'T -> int) (queue : Queue<'T>) =
    checkNotNull (nameof queue) queue

    if Lengths.queueUpTo 2 queue <= 1 then
        queue
    else
        ofSorted (Seq.sortWith comparer queue)

[<CompiledName "SortWithComparer">]
let sortWithComparer (comparer : IComparer<'T>) (queue : Queue<'T>) =
    checkNotNull (nameof queue) queue

    if Lengths.queueUpTo 2 queue <= 1 then
        queue
    else
        queue.Order(comparer).ToArray() |> Queues.ofFreshArray

// ---- Random ----

// The functions draw the random numbers as the List functions do: they take a snapshot, run the List checks with its
// length and choose from it, or apply the Array function to it

[<CompiledName "RandomChoice">]
let randomChoice (source : Queue<'T>) =
    let array = Snapshot.ofSeq (nameof source) source
    ListChecks.notEmptySequence (nameof source) array.Length
    array[Random.Shared.Next (0, array.Length)]

[<CompiledName "RandomChoiceBy">]
let randomChoiceBy (randomizer : unit -> float) (source : Queue<'T>) =
    let array = Snapshot.ofSeq (nameof source) source
    ListChecks.notEmptySequence (nameof source) array.Length
    array[ListChecks.randomIndex (nameof randomizer) randomizer array.Length]

[<CompiledName "RandomChoiceWith">]
let randomChoiceWith (random : Random) (source : Queue<'T>) =
    checkNotNull (nameof random) random
    let array = Snapshot.ofSeq (nameof source) source
    ListChecks.notEmptySequence (nameof source) array.Length
    array[random.Next (0, array.Length)]

[<CompiledName "RandomChoices">]
let randomChoices count (source : Queue<'T>) =
    let array = Snapshot.ofSeq (nameof source) source
    ListChecks.randomChoices (nameof count) count (nameof source) array.Length
    Array.randomChoices count array |> Queues.ofFreshArray

[<CompiledName "RandomChoicesBy">]
let randomChoicesBy (randomizer : unit -> float) count (source : Queue<'T>) =
    let array = Snapshot.ofSeq (nameof source) source
    ListChecks.randomChoices (nameof count) count (nameof source) array.Length
    Array.randomChoicesBy randomizer count array
    |> Queues.ofFreshArray

[<CompiledName "RandomChoicesWith">]
let randomChoicesWith (random : Random) count (source : Queue<'T>) =
    checkNotNull (nameof random) random
    let array = Snapshot.ofSeq (nameof source) source
    ListChecks.randomChoices (nameof count) count (nameof source) array.Length
    Array.randomChoicesWith random count array
    |> Queues.ofFreshArray

[<CompiledName "RandomSample">]
let randomSample count (source : Queue<'T>) =
    let array = Snapshot.ofSeq (nameof source) source
    ListChecks.randomSample (nameof count) count (nameof source) array.Length
    Array.randomSample count array |> Queues.ofFreshArray

[<CompiledName "RandomSampleBy">]
let randomSampleBy (randomizer : unit -> float) count (source : Queue<'T>) =
    let array = Snapshot.ofSeq (nameof source) source
    ListChecks.randomSample (nameof count) count (nameof source) array.Length
    Array.randomSampleBy randomizer count array
    |> Queues.ofFreshArray

[<CompiledName "RandomSampleWith">]
let randomSampleWith (random : Random) count (source : Queue<'T>) =
    checkNotNull (nameof random) random
    let array = Snapshot.ofSeq (nameof source) source
    ListChecks.randomSample (nameof count) count (nameof source) array.Length
    Array.randomSampleWith random count array
    |> Queues.ofFreshArray

[<CompiledName "RandomShuffle">]
let randomShuffle (source : Queue<'T>) =
    let array = Snapshot.ofSeq (nameof source) source
    Array.randomShuffleInPlace array
    Queues.ofFreshArray array

[<CompiledName "RandomShuffleBy">]
let randomShuffleBy (randomizer : unit -> float) (source : Queue<'T>) =
    let array = Snapshot.ofSeq (nameof source) source
    Array.randomShuffleInPlaceBy randomizer array
    Queues.ofFreshArray array

[<CompiledName "RandomShuffleWith">]
let randomShuffleWith (random : Random) (source : Queue<'T>) =
    checkNotNull (nameof random) random
    let array = Snapshot.ofSeq (nameof source) source
    Array.randomShuffleInPlaceWith random array
    Queues.ofFreshArray array

// ---- Conversion ----

[<CompiledName "ToArray">]
let toArray (queue : Queue<'T>) = Snapshot.ofSeq (nameof queue) queue

[<CompiledName "ToList">]
let toList (queue : Queue<'T>) =
    checkNotNull (nameof queue) queue
    List.ofSeq queue

[<CompiledName "ToSeq">]
let toSeq (queue : Queue<'T>) =
    checkNotNull (nameof queue) queue
    queue :> seq<'T>
