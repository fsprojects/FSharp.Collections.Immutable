[<RequireQualifiedAccess>]
module FSharp.Collections.Immutable.IndexedList

open System
open System.Collections.Generic
open System.Collections.Immutable
open System.Linq

// The functions call a member of ImmutableList where it has the semantics of the List function, and otherwise take a
// snapshot of the elements in an array, apply an Array function or a shared algorithm and build a new tree from the fresh
// result (ADR 0001, D5). The checks of ListChecks throw what the List functions throw (D6).

/// An indexed list of the elements of a fresh array that nobody changes any more; the tree copies the elements.
let inline private ofFreshArray (array : 'T array) = ImmutableList.CreateRange array

/// A range of an indexed list: the empty list or the list itself without a new tree, otherwise a new tree. The range is
/// copied into an array and the tree is built from the array: GetRange reads the elements of the range one by one from
/// the middle of the tree, which takes O(count log n), where the copy takes O(log n + count) and the build is one pass.
/// For a range of half a million elements of a list of a million that is twice as fast.
let private range (list : IndexedList<'T>) (start : int) (count : int) =
    if count = 0 then
        ImmutableList<'T>.Empty
    elif count = list.Count then
        list
    else
        let items = Array.zeroCreate count
        list.CopyTo (start, items, 0, count)
        ofFreshArray items

/// The first value that the chooser returns for the elements, in their order.
let private firstChoice (chooser : 'T -> 'U voption) (list : IndexedList<'T>) = Walk.tryPick chooser list

/// A range of an indexed list sorted stably: the range is sorted in a copy, with LINQ, which sorts stably, and put back
/// in place of the range. ImmutableList.Sort(Int32, Int32, IComparer) is not stable (ADR 0001, D5).
let private sortRangeStably (comparer : IComparer<'T>) index count (list : IndexedList<'T>) =
    checkNotNull (nameof list) list
    ListRanges.forward (nameof index) index (nameof count) count (nameof count) list.Count

    // A range of at most one element needs no sorting, and the comparer does not run for it
    if count <= 1 then
        list
    else
        // The range is copied into an array, where GetRange reads its elements one by one from the middle of the tree: for
        // 10 000 elements of a list of a million, that took 2.2 ms instead of 2.8 ms
        let items = Array.zeroCreate count
        list.CopyTo (index, items, 0, count)
        let sorted = items.Order(comparer).ToArray()
        list.RemoveRange(index, count).InsertRange(index, sorted)

// ---- Creation ----

[<GeneralizableValue>]
[<CompiledName "Empty">]
let empty<'T> : IndexedList<'T> = ImmutableList<'T>.Empty

[<CompiledName "Initialize">]
let init length (initializer : int -> 'T) =
    // List.init reports the length under the name "count"
    ListChecks.nonNegative "count" length
    Array.init length initializer |> ofFreshArray

[<CompiledName "OfArray">]
let ofArray (array : 'T array) =
    checkNotNull (nameof array) array
    ImmutableList.CreateRange array

[<CompiledName "OfList">]
let ofList (list : 'T list) =
    // An F# list is an IReadOnlyList whose indexer walks the list from its head, and the BCL builds a tree by indexing a
    // list that implements IReadOnlyList: ImmutableList.CreateRange of a list took 0.43 ms for 1000 elements, 41 ms for
    // 10 000 and 5.9 s for 100 000, a time that grows with the square of the length. The copy in an array takes 8 µs, 90 µs
    // and 1.4 ms for the same lists, and the tree is built from the array.
    ofFreshArray (List.toArray list)

[<CompiledName "OfReadOnlySpan">]
let ofReadOnlySpan (span : ReadOnlySpan<'T>) = ImmutableList.Create<'T> span

[<CompiledName "OfSeq">]
let ofSeq (source : seq<'T>) =
    // A default flat list given as a sequence is a null input; the BCL would fail while it enumerates it (ADR 0001, D6)
    checkNotNullOrDefaultSeq (nameof source) source

    match source with
    | :? ('T list) as list -> ofList list
    | _ -> ImmutableList.CreateRange source

[<CompiledName "OfSpan">]
let ofSpan (span : Span<'T>) = ImmutableList.Create<'T>(Span<'T>.op_Implicit span)

[<CompiledName "Replicate">]
let replicate count (initial : 'T) =
    ListChecks.replicate (nameof count) count
    Array.create count initial |> ofFreshArray

[<CompiledName "Singleton">]
let singleton (value : 'T) = ImmutableList.Create<'T> value

[<CompiledName "Unfold">]
let unfold (generator : 'State -> struct ('T * 'State) voption) (state : 'State) =
    // As in choose, the number of elements is not known in advance: a ResizeArray and one CreateRange
    let items = ResizeArray<'T>()
    let mutable state = state
    let mutable go = true

    while go do
        match generator state with
        | ValueSome (struct (item, next)) ->
            items.Add item
            state <- next
        | ValueNone -> go <- false

    ImmutableList.CreateRange items

// ---- Builder ----

[<CompiledName "Build">]
let build (action : IndexedList<'T>.Builder -> unit) =
    let builder = ImmutableList.CreateBuilder<'T>()
    action builder
    builder.ToImmutable ()

[<CompiledName "CreateBuilder">]
let builder () : IndexedList<'T>.Builder = ImmutableList.CreateBuilder<'T>()

[<CompiledName "OfBuilder">]
let ofBuilder (builder : IndexedList<'T>.Builder) =
    checkNotNull (nameof builder) builder
    builder.ToImmutable ()

[<CompiledName "ToBuilder">]
let toBuilder (list : IndexedList<'T>) =
    checkNotNull (nameof list) list
    list.ToBuilder ()

[<CompiledName "Update">]
let update (action : IndexedList<'T>.Builder -> unit) (list : IndexedList<'T>) =
    checkNotNull (nameof list) list
    let builder = list.ToBuilder ()
    action builder
    builder.ToImmutable ()

// ---- Properties ----

[<CompiledName "IsEmpty">]
let isEmpty (list : IndexedList<'T>) =
    checkNotNull (nameof list) list
    list.IsEmpty

[<CompiledName "Length">]
let length (list : IndexedList<'T>) =
    checkNotNull (nameof list) list
    list.Count

// ---- Access ----

[<CompiledName "ExactlyOne">]
let exactlyOne (list : IndexedList<'T>) =
    checkNotNull (nameof list) list
    // List.exactlyOne reports its input under the name "source"
    ListChecks.exactlyOne "source" list.Count
    list[0]

[<CompiledName "Head">]
let head (list : IndexedList<'T>) =
    checkNotNull (nameof list) list
    ListChecks.notEmpty (nameof list) list.Count
    list[0]

[<CompiledName "Item">]
let item index (list : IndexedList<'T>) =
    checkNotNull (nameof list) list
    ListChecks.item (nameof index) index list.Count
    list[index]

[<CompiledName "Last">]
let last (list : IndexedList<'T>) =
    checkNotNull (nameof list) list
    ListChecks.notEmpty (nameof list) list.Count
    list[list.Count - 1]

[<CompiledName "Tail">]
let tail (list : IndexedList<'T>) =
    checkNotNull (nameof list) list
    ListChecks.notEmpty (nameof list) list.Count
    list.RemoveAt 0

[<CompiledName "TryExactlyOne">]
let tryExactlyOne (list : IndexedList<'T>) =
    checkNotNull (nameof list) list
    if list.Count = 1 then ValueSome list[0] else ValueNone

[<CompiledName "TryHead">]
let tryHead (list : IndexedList<'T>) =
    checkNotNull (nameof list) list
    if list.IsEmpty then ValueNone else ValueSome list[0]

[<CompiledName "TryItem">]
let tryItem index (list : IndexedList<'T>) =
    checkNotNull (nameof list) list

    if index < 0 || index >= list.Count then
        ValueNone
    else
        ValueSome list[index]

[<CompiledName "TryLast">]
let tryLast (list : IndexedList<'T>) =
    checkNotNull (nameof list) list

    if list.IsEmpty then
        ValueNone
    else
        ValueSome list[list.Count - 1]

[<CompiledName "TryTail">]
let tryTail (list : IndexedList<'T>) =
    checkNotNull (nameof list) list
    if list.IsEmpty then
        ValueNone
    else
        ValueSome (list.RemoveAt 0)

[<CompiledName "TryUncons">]
let tryUncons (list : IndexedList<'T>) =
    checkNotNull (nameof list) list

    if list.IsEmpty then
        ValueNone
    else
        ValueSome (struct (list[0], list.RemoveAt 0))

[<CompiledName "Uncons">]
let uncons (list : IndexedList<'T>) =
    checkNotNull (nameof list) list
    // An empty list throws what List.head throws
    ListChecks.notEmpty (nameof list) list.Count
    struct (list[0], list.RemoveAt 0)

[<return : Struct>]
[<CompiledName "ConsPattern">]
let (|Cons|_|) (list : IndexedList<'T>) = tryUncons list

[<CompiledName "NilPattern">]
let (|Nil|_|) (list : IndexedList<'T>) =
    checkNotNull (nameof list) list
    list.IsEmpty

// ---- Search ----

[<CompiledName "BinarySearch">]
let binarySearch (value : 'T) (list : IndexedList<'T>) =
    checkNotNull (nameof list) list
    list.BinarySearch (value, DefaultComparer<'T>.Instance)

[<CompiledName "BinarySearchRange">]
let binarySearchRange index count (value : 'T) (list : IndexedList<'T>) =
    checkNotNull (nameof list) list
    ListRanges.search (nameof index) index (nameof count) count list.Count
    list.BinarySearch (index, count, value, DefaultComparer<'T>.Instance)

[<CompiledName "BinarySearchRangeWithComparer">]
let binarySearchRangeWithComparer (comparer : IComparer<'T>) index count (value : 'T) (list : IndexedList<'T>) =
    checkNotNull (nameof list) list
    ListRanges.search (nameof index) index (nameof count) count list.Count
    list.BinarySearch (index, count, value, comparer)

[<CompiledName "BinarySearchWithComparer">]
let binarySearchWithComparer (comparer : IComparer<'T>) (value : 'T) (list : IndexedList<'T>) =
    checkNotNull (nameof list) list
    list.BinarySearch (value, comparer)

[<CompiledName "Contains">]
let contains (value : 'T) (source : IndexedList<'T>) =
    checkNotNull (nameof source) source
    // F# equality with the value on the left, as List.contains compares
    source.Exists (fun item -> value = item)

[<CompiledName "Exists">]
let exists (predicate : 'T -> bool) (list : IndexedList<'T>) =
    checkNotNull (nameof list) list
    list.Exists (Predicate predicate)

// exists2, forall2 and compareWith can decide before the end, so they walk the two lists with their enumerators and read
// the elements up to the pair that decides. ListArrays copies both lists first, which takes O(n) when the first pair
// decides (about 4 ms and 800 KB against 0.006 ms and nothing for two lists of 100 000 integers), and an enumerator
// reached through an interface takes more than twice as long as the copy on a walk to the end, so the struct
// enumerators of the lists are used here. The lengths are checked as ListArrays checks them (ADR 0001, D6).

[<CompiledName "Exists2">]
let exists2 predicate (list1 : IndexedList<'T1>) (list2 : IndexedList<'T2>) =
    checkNotNull (nameof list1) list1
    checkNotNull (nameof list2) list2
    let mutable enumerator1 = list1.GetEnumerator ()
    let mutable enumerator2 = list2.GetEnumerator ()

    try
        let mutable found = false

        while not found
              && enumerator1.MoveNext ()
              && enumerator2.MoveNext () do
            found <- predicate enumerator1.Current enumerator2.Current

        // List.exists2 compares the lengths only when no pair of the common prefix satisfies the predicate
        if not found then
            ListChecks.exists2 (nameof list2) list1.Count list2.Count

        found
    finally
        enumerator1.Dispose ()
        enumerator2.Dispose ()

[<CompiledName "Find">]
let find (predicate : 'T -> bool) (list : IndexedList<'T>) =
    checkNotNull (nameof list) list
    let index = list.FindIndex (Predicate predicate)
    if index < 0 then Raise.indexNotFound () else list[index]

[<CompiledName "FindBack">]
let findBack (predicate : 'T -> bool) (list : IndexedList<'T>) =
    checkNotNull (nameof list) list
    let index = list.FindLastIndex (Predicate predicate)
    if index < 0 then Raise.indexNotFound () else list[index]

[<CompiledName "FindIndex">]
let findIndex (predicate : 'T -> bool) (list : IndexedList<'T>) =
    checkNotNull (nameof list) list
    let index = list.FindIndex (Predicate predicate)
    if index < 0 then Raise.indexNotFound () else index

[<CompiledName "FindIndexBack">]
let findIndexBack (predicate : 'T -> bool) (list : IndexedList<'T>) =
    checkNotNull (nameof list) list
    let index = list.FindLastIndex (Predicate predicate)
    if index < 0 then Raise.indexNotFound () else index

[<CompiledName "ForAll">]
let forall (predicate : 'T -> bool) (list : IndexedList<'T>) =
    checkNotNull (nameof list) list
    list.TrueForAll (Predicate predicate)

[<CompiledName "ForAll2">]
let forall2 predicate (list1 : IndexedList<'T1>) (list2 : IndexedList<'T2>) =
    checkNotNull (nameof list1) list1
    checkNotNull (nameof list2) list2
    let mutable enumerator1 = list1.GetEnumerator ()
    let mutable enumerator2 = list2.GetEnumerator ()

    try
        let mutable holds = true

        while holds && enumerator1.MoveNext () && enumerator2.MoveNext () do
            holds <- predicate enumerator1.Current enumerator2.Current

        // List.forall2 compares the lengths only when every pair of the common prefix satisfies the predicate
        if holds then
            ListChecks.sameLengths (nameof list1) list1.Count (nameof list2) list2.Count

        holds
    finally
        enumerator1.Dispose ()
        enumerator2.Dispose ()

[<CompiledName "IndexOf">]
let indexOf (value : 'T) (list : IndexedList<'T>) =
    checkNotNull (nameof list) list
    list.IndexOf value

[<CompiledName "IndexOfRange">]
let indexOfRange startIndex count (value : 'T) (list : IndexedList<'T>) =
    checkNotNull (nameof list) list
    ListRanges.forward (nameof startIndex) startIndex (nameof count) count (nameof count) list.Count
    list.IndexOf (value, startIndex, count, EqualityComparer<'T>.Default)

[<CompiledName "IndexOfRangeWith">]
let indexOfRangeWith (comparer : IEqualityComparer<'T>) startIndex count (value : 'T) (list : IndexedList<'T>) =
    checkNotNull (nameof list) list
    ListRanges.forward (nameof startIndex) startIndex (nameof count) count (nameof count) list.Count
    list.IndexOf (value, startIndex, count, comparer)

[<CompiledName "IndexOfWith">]
let indexOfWith (comparer : IEqualityComparer<'T>) (value : 'T) (list : IndexedList<'T>) =
    checkNotNull (nameof list) list
    list.IndexOf (value, 0, list.Count, comparer)

[<CompiledName "LastIndexOf">]
let lastIndexOf (value : 'T) (list : IndexedList<'T>) =
    checkNotNull (nameof list) list
    ImmutableList.LastIndexOf (list, value)

[<CompiledName "LastIndexOfRange">]
let lastIndexOfRange startIndex count (value : 'T) (list : IndexedList<'T>) =
    checkNotNull (nameof list) list
    ListRanges.backward (nameof startIndex) startIndex (nameof count) count list.Count

    // An empty range has no equal element; the member rejects a start index of Int32.MaxValue for it
    if count = 0 then
        -1
    else
        list.LastIndexOf (value, startIndex, count, EqualityComparer<'T>.Default)

[<CompiledName "LastIndexOfRangeWith">]
let lastIndexOfRangeWith (comparer : IEqualityComparer<'T>) startIndex count (value : 'T) (list : IndexedList<'T>) =
    checkNotNull (nameof list) list
    ListRanges.backward (nameof startIndex) startIndex (nameof count) count list.Count

    if count = 0 then
        -1
    else
        list.LastIndexOf (value, startIndex, count, comparer)

[<CompiledName "LastIndexOfWith">]
let lastIndexOfWith (comparer : IEqualityComparer<'T>) (value : 'T) (list : IndexedList<'T>) =
    checkNotNull (nameof list) list
    ImmutableList.LastIndexOf (list, value, comparer)

[<CompiledName "Pick">]
let pick (chooser : 'T -> 'U voption) (list : IndexedList<'T>) =
    checkNotNull (nameof list) list

    match firstChoice chooser list with
    | ValueSome value -> value
    | ValueNone -> Raise.indexNotFound ()

[<CompiledName "TryFind">]
let tryFind (predicate : 'T -> bool) (list : IndexedList<'T>) =
    checkNotNull (nameof list) list
    let index = list.FindIndex (Predicate predicate)
    if index < 0 then ValueNone else ValueSome list[index]

[<CompiledName "TryFindBack">]
let tryFindBack (predicate : 'T -> bool) (list : IndexedList<'T>) =
    checkNotNull (nameof list) list
    let index = list.FindLastIndex (Predicate predicate)
    if index < 0 then ValueNone else ValueSome list[index]

[<CompiledName "TryFindIndex">]
let tryFindIndex (predicate : 'T -> bool) (list : IndexedList<'T>) =
    checkNotNull (nameof list) list
    let index = list.FindIndex (Predicate predicate)
    if index < 0 then ValueNone else ValueSome index

[<CompiledName "TryFindIndexBack">]
let tryFindIndexBack (predicate : 'T -> bool) (list : IndexedList<'T>) =
    checkNotNull (nameof list) list
    let index = list.FindLastIndex (Predicate predicate)
    if index < 0 then ValueNone else ValueSome index

[<CompiledName "TryFindIndexBackRange">]
let tryFindIndexBackRange startIndex count (predicate : 'T -> bool) (list : IndexedList<'T>) =
    checkNotNull (nameof list) list
    ListRanges.backward (nameof startIndex) startIndex (nameof count) count list.Count

    // An empty range has no element that satisfies the predicate; the member rejects the start index Int32.MaxValue for it
    let index =
        if count = 0 then
            -1
        else
            list.FindLastIndex (startIndex, count, Predicate predicate)

    if index < 0 then ValueNone else ValueSome index

[<CompiledName "TryFindIndexRange">]
let tryFindIndexRange startIndex count (predicate : 'T -> bool) (list : IndexedList<'T>) =
    checkNotNull (nameof list) list
    ListRanges.forward (nameof startIndex) startIndex (nameof count) count (nameof count) list.Count
    let index = list.FindIndex (startIndex, count, Predicate predicate)
    if index < 0 then ValueNone else ValueSome index

[<CompiledName "TryPick">]
let tryPick (chooser : 'T -> 'U voption) (list : IndexedList<'T>) =
    checkNotNull (nameof list) list
    firstChoice chooser list

// ---- Transformation ----

[<CompiledName "Add">]
let add (value : 'T) (list : IndexedList<'T>) =
    checkNotNull (nameof list) list
    list.Add value

[<CompiledName "AllPairs">]
let allPairs (list1 : IndexedList<'T1>) (list2 : IndexedList<'T2>) =
    // Every element of list1 meets every element of list2, so list2 is read again for each of them. Two snapshots read
    // each list once and then index arrays: against an enumerator of list2 for each element of list1 they took 1.4 to 2.5
    // times less time for 300 x 300, 1000 x 1000, 10 x 100 000 and 100 000 x 10 elements. A snapshot of list2 alone was
    // between 2.3 times slower and 17 % faster, which does not pay for an algorithm of its own next to Arrays.allPairs.
    Arrays.allPairs structPair (Snapshot.ofSeq (nameof list1) list1) (Snapshot.ofSeq (nameof list2) list2)
    |> ofFreshArray

[<CompiledName "Append">]
let append (list1 : IndexedList<'T>) (list2 : IndexedList<'T>) =
    checkNotNull (nameof list1) list1
    checkNotNull (nameof list2) list2
    list1.AddRange list2

[<CompiledName "Choose">]
let choose (chooser : 'T -> 'U voption) (list : IndexedList<'T>) =
    checkNotNull (nameof list) list
    // The chosen elements wait in an array as long as the list, the most that the result can have: a new one below 1024
    // elements, and one rented from the pool above (see Buffers). The ResizeArray that was here doubles its array as it
    // fills, and allocated 18 % more than the tree needs for a million integers and 35 % more for a million strings; the
    // arrays of its growth in the large object heap made the collections of the second generation more frequent. The
    // array took the same time for integers of 10 to 10 000 elements and up to 10 % less above; a buffer of chunks that
    // are joined to the result as subtrees left a tree whose indexer was 6 to 10 % slower (the average of two runs, 100 to
    // 100 000 elements), and it took no less time than the array. A builder rebalances its tree
    // after every Add and took 3.3 to 3.6 times as long for integers and 2.5 times for strings of 100 000 to a million
    // elements; there is no tree of an input to join to its tree here, as collect and concat have (see Collectors).
    let chosen = Buffers.ChosenBuffer<'U> list.Count

    Walk.iter
        (fun item ->
            match chooser item with
            | ValueSome value -> chosen.Add value
            | ValueNone -> ()
        )
        list

    chosen.ToImmutable ()

[<CompiledName "ChunkBySize">]
let chunkBySize chunkSize (list : IndexedList<'T>) =
    checkNotNull (nameof list) list
    ListChecks.positive (nameof chunkSize) chunkSize

    if list.IsEmpty then
        ImmutableList<IndexedList<'T>>.Empty
    elif chunkSize >= list.Count then
        // The only chunk is the list itself
        ImmutableList.Create<IndexedList<'T>> list
    else
        // One copy of the list and one tree for each chunk. Reading each chunk from the tree, with GetRange, took 1.7 to
        // 2.2 times as long for chunks of 2, 100 and 10 000 elements of a list of a million. The chunks are mapped to
        // indexed lists in a sequence that ofSeq builds the result from, as the BCL copies a sequence into a buffer anyway,
        // and an array of the chunks in between is no faster.
        Array.chunkBySize chunkSize (Snapshot.ofSeq (nameof list) list)
        |> Seq.map ofFreshArray
        |> ofSeq

[<CompiledName "Collect">]
let collect (mapping : 'T -> IndexedList<'U>) (list : IndexedList<'T>) =
    checkNotNull (nameof list) list
    // The collector copies the short results and joins the long ones to its tree as they are (see Collectors)
    let collector = Collectors.ListCollector<'U>()

    Walk.iter (fun item -> collector.Add (mapping item)) list
    collector.ToImmutable ()

[<CompiledName "Concat">]
let concat (lists : seq<IndexedList<'T>>) =
    checkNotNullOrDefaultSeq (nameof lists) lists
    // The collector copies the short lists and joins the long ones to its tree as they are (see Collectors)
    let collector = Collectors.ListCollector<'T>()

    for list in lists do
        // A null element is a null input, as a default element is for FlatList.concat (ADR 0001, D6)
        checkNotNull (nameof lists) list
        collector.Add list

    collector.ToImmutable ()

[<CompiledName "Cons">]
let cons (value : 'T) (list : IndexedList<'T>) =
    checkNotNull (nameof list) list
    list.Insert (0, value)

[<CompiledName "Filter">]
let filter (predicate : 'T -> bool) (list : IndexedList<'T>) =
    checkNotNull (nameof list) list
    list.FindAll (Predicate predicate)

[<CompiledName "Indexed">]
let indexed (list : IndexedList<'T>) =
    Arrays.indexed structPair (Snapshot.ofSeq (nameof list) list)
    |> ofFreshArray

[<CompiledName "InsertAt">]
let insertAt index (value : 'T) (source : IndexedList<'T>) =
    checkNotNull (nameof source) source
    ListChecks.insertAt (nameof index) index source.Count
    source.Insert (index, value)

[<CompiledName "InsertManyAt">]
let insertManyAt index (values : seq<'T>) (source : IndexedList<'T>) =
    checkNotNull (nameof source) source
    // List checks the index before it reads the values
    ListChecks.insertAt (nameof index) index source.Count
    checkNotNullOrDefaultSeq (nameof values) values
    source.InsertRange (index, values)

[<CompiledName "Map">]
let map (mapping : 'T -> 'U) (list : IndexedList<'T>) =
    checkNotNull (nameof list) list
    list.ConvertAll (Func<'T, 'U> mapping)

[<CompiledName "Map2">]
let map2 (mapping : 'T1 -> 'T2 -> 'U) (list1 : IndexedList<'T1>) (list2 : IndexedList<'T2>) =
    ListArrays.map2
        (nameof list1)
        (nameof list2)
        mapping
        (Snapshot.ofSeq (nameof list1) list1)
        (Snapshot.ofSeq (nameof list2) list2)
    |> ofFreshArray

[<CompiledName "Map3">]
let map3 (mapping : 'T1 -> 'T2 -> 'T3 -> 'U) (list1 : IndexedList<'T1>) (list2 : IndexedList<'T2>) (list3 : IndexedList<'T3>) =
    ListArrays.map3
        (nameof list1)
        (nameof list2)
        (nameof list3)
        mapping
        (Snapshot.ofSeq (nameof list1) list1)
        (Snapshot.ofSeq (nameof list2) list2)
        (Snapshot.ofSeq (nameof list3) list3)
    |> ofFreshArray

[<CompiledName "MapIndexed">]
let mapi (mapping : int -> 'T -> 'U) (list : IndexedList<'T>) =
    checkNotNull (nameof list) list
    let results = Array.zeroCreate list.Count
    Walk.iteri (fun index item -> results[index] <- mapping index item) list
    ofFreshArray results

[<CompiledName "MapIndexed2">]
let mapi2 (mapping : int -> 'T1 -> 'T2 -> 'U) (list1 : IndexedList<'T1>) (list2 : IndexedList<'T2>) =
    ListArrays.mapi2
        (nameof list1)
        (nameof list2)
        mapping
        (Snapshot.ofSeq (nameof list1) list1)
        (Snapshot.ofSeq (nameof list2) list2)
    |> ofFreshArray

[<CompiledName "Pairwise">]
let pairwise (list : IndexedList<'T>) =
    Arrays.pairwise structPair (Snapshot.ofSeq (nameof list) list)
    |> ofFreshArray

[<CompiledName "Permute">]
let permute indexMap (list : IndexedList<'T>) =
    // List.permute applies the algorithm of Array.permute to a copy of the list, with the same checks
    Array.permute indexMap (Snapshot.ofSeq (nameof list) list)
    |> ofFreshArray

[<CompiledName "Remove">]
let remove (item : 'T) (list : IndexedList<'T>) =
    checkNotNull (nameof list) list
    list.Remove item

[<CompiledName "RemoveAt">]
let removeAt index (source : IndexedList<'T>) =
    checkNotNull (nameof source) source
    ListChecks.removeAt (nameof index) index source.Count
    source.RemoveAt index

[<CompiledName "RemoveMany">]
let removeMany (items : seq<'T>) (list : IndexedList<'T>) =
    checkNotNull (nameof list) list
    // The member names null items as this function does, but it fails while it enumerates a default flat list
    checkNotNullOrDefaultSeq (nameof items) items
    list.RemoveRange items

[<CompiledName "RemoveManyAt">]
let removeManyAt index count (source : IndexedList<'T>) =
    checkNotNull (nameof source) source
    ListChecks.removeManyAt (nameof index) index count source.Count
    // List walks the first index + count elements, so it removes nothing for a count that is not positive or for a sum
    // that overflows to a negative number
    if count <= 0 || index + count < 0 then
        source
    else
        source.RemoveRange (index, count)

[<CompiledName "RemoveManyWith">]
let removeManyWith (comparer : IEqualityComparer<'T>) (items : seq<'T>) (list : IndexedList<'T>) =
    checkNotNull (nameof list) list
    checkNotNullOrDefaultSeq (nameof items) items
    list.RemoveRange (items, comparer)

[<CompiledName "RemoveWith">]
let removeWith (comparer : IEqualityComparer<'T>) (item : 'T) (list : IndexedList<'T>) =
    checkNotNull (nameof list) list
    list.Remove (item, comparer)

[<CompiledName "Replace">]
let replace (oldValue : 'T) (newValue : 'T) (list : IndexedList<'T>) =
    checkNotNull (nameof list) list
    list.Replace (oldValue, newValue)

[<CompiledName "ReplaceWith">]
let replaceWith (comparer : IEqualityComparer<'T>) (oldValue : 'T) (newValue : 'T) (list : IndexedList<'T>) =
    checkNotNull (nameof list) list
    list.Replace (oldValue, newValue, comparer)

[<CompiledName "Reverse">]
let rev (list : IndexedList<'T>) =
    // A copy reversed in place and a new balanced tree are about 8 times faster than ImmutableList.Reverse at 100 000
    // integers (about 3 ms against 23 ms), and the copy is the only extra allocation (ADR 0001, D5); the copy is fresh,
    // so nobody else sees it change
    let array = Snapshot.ofSeq (nameof list) list
    Array.Reverse array
    ofFreshArray array

[<CompiledName "ReverseRange">]
let revRange index count (list : IndexedList<'T>) =
    checkNotNull (nameof list) list
    // ImmutableList<T>.Reverse(Int32, Int32) names the index for a range beyond the end
    ListRanges.forward (nameof index) index (nameof count) count (nameof index) list.Count
    list.Reverse (index, count)

[<CompiledName "Transpose">]
let transpose (lists : seq<IndexedList<'T>>) =
    checkNotNullOrDefaultSeq (nameof lists) lists

    let arrays =
        lists
        |> Seq.map (fun list -> Snapshot.ofSeq (nameof lists) list)
        |> Seq.toArray

    // List.transpose names the lists "list.[0]", "list.[1]", …; once their lengths agree, Array.transpose agrees with it
    ListChecks.transpose "list" (arrays |> Array.map _.Length)

    Array.transpose arrays |> Seq.map ofFreshArray |> ofSeq

[<CompiledName "Unzip">]
let unzip (list : IndexedList<struct ('T1 * 'T2)>) =
    let struct (first, second) = Arrays.unzip id (Snapshot.ofSeq (nameof list) list)
    struct (ofFreshArray first, ofFreshArray second)

[<CompiledName "Unzip3">]
let unzip3 (list : IndexedList<struct ('T1 * 'T2 * 'T3)>) =
    let struct (first, second, third) = Arrays.unzip3 id (Snapshot.ofSeq (nameof list) list)
    struct (ofFreshArray first, ofFreshArray second, ofFreshArray third)

[<CompiledName "UpdateAt">]
let updateAt index (value : 'T) (source : IndexedList<'T>) =
    checkNotNull (nameof source) source
    ListChecks.removeAt (nameof index) index source.Count
    source.SetItem (index, value)

[<CompiledName "Where">]
let where (predicate : 'T -> bool) (list : IndexedList<'T>) =
    checkNotNull (nameof list) list
    list.FindAll (Predicate predicate)

[<CompiledName "Windowed">]
let windowed windowSize (list : IndexedList<'T>) =
    checkNotNull (nameof list) list
    ListChecks.positive (nameof windowSize) windowSize

    if windowSize > list.Count then
        ImmutableList<IndexedList<'T>>.Empty
    elif windowSize = list.Count then
        // The only window is the list itself
        ImmutableList.Create<IndexedList<'T>> list
    else
        // One copy of the list and one tree for each window. Reading each window from the tree, with GetRange, took 2.1
        // and 4.5 times as long for windows of 3 and 100 elements of a list of 100 000.
        Array.windowed windowSize (Snapshot.ofSeq (nameof list) list)
        |> Seq.map ofFreshArray
        |> ofSeq

[<CompiledName "Zip">]
let zip (list1 : IndexedList<'T1>) (list2 : IndexedList<'T2>) =
    checkNotNull (nameof list1) list1
    checkNotNull (nameof list2) list2
    ListChecks.sameLengths (nameof list1) list1.Count (nameof list2) list2.Count

    Arrays.zip structPair (Snapshot.ofSeq (nameof list1) list1) (Snapshot.ofSeq (nameof list2) list2)
    |> ofFreshArray

[<CompiledName "Zip3">]
let zip3 (list1 : IndexedList<'T1>) (list2 : IndexedList<'T2>) (list3 : IndexedList<'T3>) =
    checkNotNull (nameof list1) list1
    checkNotNull (nameof list2) list2
    checkNotNull (nameof list3) list3
    ListChecks.zip3 (nameof list1) list1.Count (nameof list2) list2.Count (nameof list3) list3.Count

    Arrays.zip3
        structTriple
        (Snapshot.ofSeq (nameof list1) list1)
        (Snapshot.ofSeq (nameof list2) list2)
        (Snapshot.ofSeq (nameof list3) list3)
    |> ofFreshArray

// ---- Folding ----

// The inline functions can use public members only, so they check the list themselves; the empty list throws what
// List.average and List.averageBy throw, with the name "source"
[<CompiledName "Average">]
let inline average (list : IndexedList< ^T >) : ^T =
    if obj.ReferenceEquals (list, null) then
        nullArg (nameof list)

    if list.IsEmpty then
        invalidArg "source" ErrorStrings.InputSequenceEmpty

    let mutable sum = LanguagePrimitives.GenericZero< ^T>

    for item in list do
        sum <- Checked.(+) sum item

    LanguagePrimitives.DivideByInt< ^T> sum list.Count

[<CompiledName "AverageBy">]
let inline averageBy ([<InlineIfLambda>] projection : 'T -> ^U) (list : IndexedList<'T>) : ^U =
    if obj.ReferenceEquals (list, null) then
        nullArg (nameof list)

    if list.IsEmpty then
        invalidArg "source" ErrorStrings.InputSequenceEmpty

    let mutable sum = LanguagePrimitives.GenericZero< ^U>

    for item in list do
        sum <- Checked.(+) sum (projection item)

    LanguagePrimitives.DivideByInt< ^U> sum list.Count

[<CompiledName "CompareWith">]
let compareWith (comparer : 'T -> 'T -> int) (list1 : IndexedList<'T>) (list2 : IndexedList<'T>) =
    checkNotNull (nameof list1) list1
    checkNotNull (nameof list2) list2
    let mutable enumerator1 = list1.GetEnumerator ()
    let mutable enumerator2 = list2.GetEnumerator ()

    try
        let mutable result = 0

        while result = 0
              && enumerator1.MoveNext ()
              && enumerator2.MoveNext () do
            result <- comparer enumerator1.Current enumerator2.Current

        // As List.compareWith compares: the first difference, then the lengths
        if result <> 0 then result
        elif list1.Count < list2.Count then -1
        elif list1.Count > list2.Count then 1
        else 0
    finally
        enumerator1.Dispose ()
        enumerator2.Dispose ()

[<CompiledName "Fold">]
let fold (folder : 'State -> 'T -> 'State) (state : 'State) (list : IndexedList<'T>) =
    checkNotNull (nameof list) list
    Walk.fold folder state list

[<CompiledName "Fold2">]
let fold2 (folder : 'State -> 'T1 -> 'T2 -> 'State) (state : 'State) (list1 : IndexedList<'T1>) (list2 : IndexedList<'T2>) =
    ListArrays.fold2
        (nameof list1)
        (nameof list2)
        folder
        state
        (Snapshot.ofSeq (nameof list1) list1)
        (Snapshot.ofSeq (nameof list2) list2)

[<CompiledName "FoldBack">]
let foldBack (folder : 'T -> 'State -> 'State) (list : IndexedList<'T>) (state : 'State) =
    Array.foldBack folder (Snapshot.ofSeq (nameof list) list) state

[<CompiledName "FoldBack2">]
let foldBack2 (folder : 'T1 -> 'T2 -> 'State -> 'State) (list1 : IndexedList<'T1>) (list2 : IndexedList<'T2>) (state : 'State) =
    checkNotNull (nameof list1) list1
    checkNotNull (nameof list2) list2
    // List.foldBack2 checks the lengths before it calls the folder
    ListChecks.foldBack2 (nameof list1) list1.Count (nameof list2) list2.Count
    Array.foldBack2 folder (Snapshot.ofSeq (nameof list1) list1) (Snapshot.ofSeq (nameof list2) list2) state

[<CompiledName "Iterate">]
let iter (action : 'T -> unit) (list : IndexedList<'T>) =
    checkNotNull (nameof list) list

    // ForEach calls a delegate for every element; the loop calls the function directly
    Walk.iter action list

[<CompiledName "Iterate2">]
let iter2 (action : 'T1 -> 'T2 -> unit) (list1 : IndexedList<'T1>) (list2 : IndexedList<'T2>) =
    ListArrays.iter2
        (nameof list1)
        (nameof list2)
        action
        (Snapshot.ofSeq (nameof list1) list1)
        (Snapshot.ofSeq (nameof list2) list2)

[<CompiledName "IterateIndexed">]
let iteri (action : int -> 'T -> unit) (list : IndexedList<'T>) =
    checkNotNull (nameof list) list
    Walk.iteri action list

[<CompiledName "IterateIndexed2">]
let iteri2 (action : int -> 'T1 -> 'T2 -> unit) (list1 : IndexedList<'T1>) (list2 : IndexedList<'T2>) =
    ListArrays.iteri2
        (nameof list1)
        (nameof list2)
        action
        (Snapshot.ofSeq (nameof list1) list1)
        (Snapshot.ofSeq (nameof list2) list2)

[<CompiledName "MapFold">]
let mapFold (mapping : 'State -> 'T -> struct ('Result * 'State)) (state : 'State) (list : IndexedList<'T>) =
    checkNotNull (nameof list) list
    let results = Array.zeroCreate list.Count

    // Not a Walk loop: for strings, the loop of Walk took 1.1 to 1.4 times as long as this one once it was compiled into the
    // library, although its machine code differs only in the order of a copy of the struct that the mapping returns, and
    // the same source in a script ran 0.8 times as long. The loops of iter, fold, mapi and scan took 0.6 to 0.9 times as long.
    let mutable state = state
    let mutable index = 0

    for item in list do
        let struct (result, next) = mapping state item
        results[index] <- result
        state <- next
        index <- index + 1

    struct (ofFreshArray results, state)

[<CompiledName "MapFoldBack">]
let mapFoldBack (mapping : 'T -> 'State -> struct ('Result * 'State)) (list : IndexedList<'T>) (state : 'State) =
    let source = Snapshot.ofSeq (nameof list) list
    let results = Array.zeroCreate source.Length
    let mutable state = state

    for index = source.Length - 1 downto 0 do
        let struct (result, next) = mapping source[index] state
        results[index] <- result
        state <- next

    struct (ofFreshArray results, state)

// max, maxBy, min and minBy are inline, as the ones of List are: the comparison is compiled for the type of the elements
// at the call site. Compiled into a generic function, Seq.max and the others compare through the generic operator, which
// boxes the elements of a value type in every comparison: 4.8 MB and three times the time for 100 000 integers. As sum and
// average, they check the list themselves, because an inline function can use public members only. They compare as the
// List functions do: a later element replaces the earlier one only when it is greater, or smaller, and the projection runs
// once for each element.

[<CompiledName "Max">]
let inline max (list : IndexedList<'T>) : 'T =
    if obj.ReferenceEquals (list, null) then
        nullArg (nameof list)

    if list.IsEmpty then
        invalidArg (nameof list) ErrorStrings.InputSequenceEmpty

    let mutable first = true
    let mutable greatest = Unchecked.defaultof<'T>

    for item in list do
        if first then
            greatest <- item
            first <- false
        elif item > greatest then
            greatest <- item

    greatest

[<CompiledName "MaxBy">]
let inline maxBy ([<InlineIfLambda>] projection : 'T -> 'U) (list : IndexedList<'T>) : 'T =
    if obj.ReferenceEquals (list, null) then
        nullArg (nameof list)

    if list.IsEmpty then
        invalidArg (nameof list) ErrorStrings.InputSequenceEmpty

    let mutable first = true
    let mutable greatest = Unchecked.defaultof<'T>
    let mutable greatestKey = Unchecked.defaultof<'U>

    for item in list do
        let key = projection item

        if first then
            greatest <- item
            greatestKey <- key
            first <- false
        elif key > greatestKey then
            greatest <- item
            greatestKey <- key

    greatest

[<CompiledName "Min">]
let inline min (list : IndexedList<'T>) : 'T =
    if obj.ReferenceEquals (list, null) then
        nullArg (nameof list)

    if list.IsEmpty then
        invalidArg (nameof list) ErrorStrings.InputSequenceEmpty

    let mutable first = true
    let mutable smallest = Unchecked.defaultof<'T>

    for item in list do
        if first then
            smallest <- item
            first <- false
        elif item < smallest then
            smallest <- item

    smallest

[<CompiledName "MinBy">]
let inline minBy ([<InlineIfLambda>] projection : 'T -> 'U) (list : IndexedList<'T>) : 'T =
    if obj.ReferenceEquals (list, null) then
        nullArg (nameof list)

    if list.IsEmpty then
        invalidArg (nameof list) ErrorStrings.InputSequenceEmpty

    let mutable first = true
    let mutable smallest = Unchecked.defaultof<'T>
    let mutable smallestKey = Unchecked.defaultof<'U>

    for item in list do
        let key = projection item

        if first then
            smallest <- item
            smallestKey <- key
            first <- false
        elif key < smallestKey then
            smallest <- item
            smallestKey <- key

    smallest

[<CompiledName "Reduce">]
let reduce (reduction : 'T -> 'T -> 'T) (list : IndexedList<'T>) =
    checkNotNull (nameof list) list
    ListChecks.notEmpty (nameof list) list.Count
    Seq.reduce reduction list

[<CompiledName "ReduceBack">]
let reduceBack (reduction : 'T -> 'T -> 'T) (list : IndexedList<'T>) =
    checkNotNull (nameof list) list
    ListChecks.notEmpty (nameof list) list.Count
    Array.reduceBack reduction (Snapshot.ofSeq (nameof list) list)

[<CompiledName "Scan">]
let scan (folder : 'State -> 'T -> 'State) (state : 'State) (list : IndexedList<'T>) =
    checkNotNull (nameof list) list
    let states = Array.zeroCreate (list.Count + 1)
    states[0] <- state
    Walk.iteri (fun index item -> states[index + 1] <- folder states[index] item) list
    ofFreshArray states

[<CompiledName "ScanBack">]
let scanBack (folder : 'T -> 'State -> 'State) (list : IndexedList<'T>) (state : 'State) =
    Array.scanBack folder (Snapshot.ofSeq (nameof list) list) state
    |> ofFreshArray

[<CompiledName "Sum">]
let inline sum (list : IndexedList< ^T >) : ^T =
    if obj.ReferenceEquals (list, null) then
        nullArg (nameof list)

    let mutable sum = LanguagePrimitives.GenericZero< ^T>

    for item in list do
        sum <- Checked.(+) sum item

    sum

[<CompiledName "SumBy">]
let inline sumBy ([<InlineIfLambda>] projection : 'T -> ^U) (list : IndexedList<'T>) : ^U =
    if obj.ReferenceEquals (list, null) then
        nullArg (nameof list)

    let mutable sum = LanguagePrimitives.GenericZero< ^U>

    for item in list do
        sum <- Checked.(+) sum (projection item)

    sum

// ---- Partitioning and grouping ----

[<CompiledName "CountBy">]
let countBy (projection : 'T -> 'Key) (list : IndexedList<'T>) =
    Arrays.countBy structPair projection (Snapshot.ofSeq (nameof list) list)
    |> ofFreshArray

[<CompiledName "Distinct">]
let distinct (list : IndexedList<'T>) =
    checkNotNull (nameof list) list
    ImmutableList.CreateRange (list.Distinct HashIdentity.Structural<'T>)

[<CompiledName "DistinctBy">]
let distinctBy (projection : 'T -> 'Key) (list : IndexedList<'T>) =
    checkNotNull (nameof list) list
    ImmutableList.CreateRange (list.DistinctBy (Func<'T, 'Key> projection, HashIdentity.Structural<'Key>))

[<CompiledName "Except">]
let except (itemsToExclude : seq<'T>) (list : IndexedList<'T>) =
    // List.except checks the items to exclude first and does not read them for an empty list
    checkNotNullOrDefaultSeq (nameof itemsToExclude) itemsToExclude
    checkNotNull (nameof list) list

    if list.IsEmpty then
        list
    else
        ImmutableList.CreateRange (list.Except (itemsToExclude, HashIdentity.Structural<'T>))

[<CompiledName "GroupBy">]
let groupBy (projection : 'T -> 'Key) (list : IndexedList<'T>) =
    Arrays.groupBy (fun key items -> struct (key, ofFreshArray items)) projection (Snapshot.ofSeq (nameof list) list)
    |> ofFreshArray

[<CompiledName "Partition">]
let partition (predicate : 'T -> bool) (list : IndexedList<'T>) =
    let accepted, rejected = Array.partition predicate (Snapshot.ofSeq (nameof list) list)
    struct (ofFreshArray accepted, ofFreshArray rejected)

[<CompiledName "PartitionWith">]
let partitionWith (partitioner : 'T -> Choice<'T1, 'T2>) (list : IndexedList<'T>) =
    let first, second = Array.partitionWith partitioner (Snapshot.ofSeq (nameof list) list)
    struct (ofFreshArray first, ofFreshArray second)

[<CompiledName "Skip">]
let skip count (list : IndexedList<'T>) =
    checkNotNull (nameof list) list
    ListChecks.skip (nameof count) count list.Count
    let count = Operators.max count 0
    range list count (list.Count - count)

[<CompiledName "SkipLast">]
let skipLast count (list : IndexedList<'T>) =
    checkNotNull (nameof list) list
    // The checks of List.skip
    ListChecks.skip (nameof count) count list.Count
    range list 0 (list.Count - Operators.max count 0)

[<CompiledName "SkipWhile">]
let skipWhile (predicate : 'T -> bool) (list : IndexedList<'T>) =
    checkNotNull (nameof list) list
    let index = list.FindIndex (fun item -> not (predicate item))
    if index < 0 then
        empty
    else
        range list index (list.Count - index)

[<CompiledName "SplitAt">]
let splitAt index (list : IndexedList<'T>) =
    checkNotNull (nameof list) list
    ListChecks.splitAt (nameof index) index list.Count
    struct (range list 0 index, range list index (list.Count - index))

[<CompiledName "SplitInto">]
let splitInto count (list : IndexedList<'T>) =
    checkNotNull (nameof list) list
    ListChecks.positive (nameof count) count

    Array.splitInto count (Snapshot.ofSeq (nameof list) list)
    |> Seq.map ofFreshArray
    |> ofSeq

[<CompiledName "GetRange">]
let sub (list : IndexedList<'T>) startIndex count =
    checkNotNull (nameof list) list

    // The checks of Array.sub, which FlatList.sub follows: GetRange throws ArgumentOutOfRangeException for a negative
    // start index or count too
    if startIndex < 0 then
        Raise.invalidArgInputMustBeNonNegative (nameof startIndex) startIndex

    if count < 0 then
        Raise.invalidArgInputMustBeNonNegative (nameof count) count

    // Array.sub adds the start index and the count, which can overflow; the difference cannot, because the start index is
    // not negative
    if count > list.Count - startIndex then
        Raise.invalidArgOutOfRange (nameof count) count "list.Length" list.Count

    range list startIndex count

[<CompiledName "Take">]
let take count (list : IndexedList<'T>) =
    checkNotNull (nameof list) list
    ListChecks.take (nameof count) count list.Count
    range list 0 count

[<CompiledName "TakeLast">]
let takeLast count (list : IndexedList<'T>) =
    checkNotNull (nameof list) list
    // The checks of List.take
    ListChecks.take (nameof count) count list.Count
    range list (list.Count - count) count

[<CompiledName "TakeWhile">]
let takeWhile (predicate : 'T -> bool) (list : IndexedList<'T>) =
    checkNotNull (nameof list) list
    let index = list.FindIndex (fun item -> not (predicate item))
    if index < 0 then list else range list 0 index

[<CompiledName "Truncate">]
let truncate count (list : IndexedList<'T>) =
    checkNotNull (nameof list) list
    range list 0 (Operators.max 0 (Operators.min count list.Count))

// ---- Sorting ----

// The sorts are stable, as the sorts of List are, and ImmutableList.Sort is not (ADR 0001, D5). The functions that compare
// the elements, or that call the projection in the comparison as List.sortByDescending does, sort with the Seq functions
// of FSharp.Core, which share the stable sort of List: the same comparisons run in Array.Sort, which wraps the exception
// of a comparison in an InvalidOperationException (D6). Seq.sortDescending and Seq.sortByDescending are inline and box the
// elements in every comparison once they are compiled into a generic function, so the two descending sorts call
// Seq.sortWith through DescendingSorts, with the cached comparer. sortBy and sortWithComparer sort with LINQ, which
// computes the keys first, as List.sortBy does, and wraps the exception of a comparison as Array.Sort does. A list of one
// element is returned without a call of the projection or the comparer, as List returns it.

[<CompiledName "Sort">]
let sort (list : IndexedList<'T>) =
    checkNotNull (nameof list) list
    if list.Count <= 1 then
        list
    else
        ImmutableList.CreateRange (Seq.sort list)

[<CompiledName "SortBy">]
let sortBy (projection : 'T -> 'Key) (list : IndexedList<'T>) =
    checkNotNull (nameof list) list

    if list.Count <= 1 then
        list
    else
        list.OrderBy(Func<'T, 'Key> projection, DefaultComparer<'Key>.Instance).ToArray()
        |> ofFreshArray

[<CompiledName "SortByDescending">]
let sortByDescending (projection : 'T -> 'Key) (list : IndexedList<'T>) =
    checkNotNull (nameof list) list

    if list.Count <= 1 then
        list
    else
        ImmutableList.CreateRange (DescendingSorts.sortByDescending projection list)

[<CompiledName "SortDescending">]
let sortDescending (list : IndexedList<'T>) =
    checkNotNull (nameof list) list
    if list.Count <= 1 then
        list
    else
        ImmutableList.CreateRange (DescendingSorts.sortDescending list)

[<CompiledName "SortRange">]
let sortRange index count (list : IndexedList<'T>) = sortRangeStably DefaultComparer<'T>.Instance index count list

[<CompiledName "SortRangeWith">]
let sortRangeWith (comparison : 'T -> 'T -> int) index count (list : IndexedList<'T>) =
    sortRangeStably (ComparisonIdentity.FromFunction comparison) index count list

[<CompiledName "SortRangeWithComparer">]
let sortRangeWithComparer (comparer : IComparer<'T>) index count (list : IndexedList<'T>) =
    sortRangeStably comparer index count list

[<CompiledName "SortWith">]
let sortWith (comparer : 'T -> 'T -> int) (list : IndexedList<'T>) =
    checkNotNull (nameof list) list
    if list.Count <= 1 then
        list
    else
        ImmutableList.CreateRange (Seq.sortWith comparer list)

[<CompiledName "SortWithComparer">]
let sortWithComparer (comparer : IComparer<'T>) (list : IndexedList<'T>) =
    checkNotNull (nameof list) list

    if list.Count <= 1 then
        list
    else
        list.Order(comparer).ToArray() |> ofFreshArray

// ---- Random ----

// The functions draw the random numbers as the List functions do; the ones that build an indexed list apply the Array
// function to a snapshot after the List checks

[<CompiledName "RandomChoice">]
let randomChoice (source : IndexedList<'T>) =
    checkNotNull (nameof source) source
    ListChecks.notEmptySequence (nameof source) source.Count
    source[Random.Shared.Next (0, source.Count)]

[<CompiledName "RandomChoiceBy">]
let randomChoiceBy (randomizer : unit -> float) (source : IndexedList<'T>) =
    checkNotNull (nameof source) source
    ListChecks.notEmptySequence (nameof source) source.Count
    source[ListChecks.randomIndex (nameof randomizer) randomizer source.Count]

[<CompiledName "RandomChoiceWith">]
let randomChoiceWith (random : Random) (source : IndexedList<'T>) =
    checkNotNull (nameof random) random
    checkNotNull (nameof source) source
    ListChecks.notEmptySequence (nameof source) source.Count
    source[random.Next (0, source.Count)]

[<CompiledName "RandomChoices">]
let randomChoices count (source : IndexedList<'T>) =
    checkNotNull (nameof source) source
    ListChecks.randomChoices (nameof count) count (nameof source) source.Count

    Array.randomChoices count (Snapshot.ofSeq (nameof source) source)
    |> ofFreshArray

[<CompiledName "RandomChoicesBy">]
let randomChoicesBy (randomizer : unit -> float) count (source : IndexedList<'T>) =
    checkNotNull (nameof source) source
    ListChecks.randomChoices (nameof count) count (nameof source) source.Count

    Array.randomChoicesBy randomizer count (Snapshot.ofSeq (nameof source) source)
    |> ofFreshArray

[<CompiledName "RandomChoicesWith">]
let randomChoicesWith (random : Random) count (source : IndexedList<'T>) =
    checkNotNull (nameof random) random
    checkNotNull (nameof source) source
    ListChecks.randomChoices (nameof count) count (nameof source) source.Count

    Array.randomChoicesWith random count (Snapshot.ofSeq (nameof source) source)
    |> ofFreshArray

[<CompiledName "RandomSample">]
let randomSample count (source : IndexedList<'T>) =
    checkNotNull (nameof source) source
    ListChecks.randomSample (nameof count) count (nameof source) source.Count

    Array.randomSample count (Snapshot.ofSeq (nameof source) source)
    |> ofFreshArray

[<CompiledName "RandomSampleBy">]
let randomSampleBy (randomizer : unit -> float) count (source : IndexedList<'T>) =
    checkNotNull (nameof source) source
    ListChecks.randomSample (nameof count) count (nameof source) source.Count

    Array.randomSampleBy randomizer count (Snapshot.ofSeq (nameof source) source)
    |> ofFreshArray

[<CompiledName "RandomSampleWith">]
let randomSampleWith (random : Random) count (source : IndexedList<'T>) =
    checkNotNull (nameof random) random
    checkNotNull (nameof source) source
    ListChecks.randomSample (nameof count) count (nameof source) source.Count

    Array.randomSampleWith random count (Snapshot.ofSeq (nameof source) source)
    |> ofFreshArray

[<CompiledName "RandomShuffle">]
let randomShuffle (source : IndexedList<'T>) =
    let array = Snapshot.ofSeq (nameof source) source
    Array.randomShuffleInPlace array
    ofFreshArray array

[<CompiledName "RandomShuffleBy">]
let randomShuffleBy (randomizer : unit -> float) (source : IndexedList<'T>) =
    let array = Snapshot.ofSeq (nameof source) source
    Array.randomShuffleInPlaceBy randomizer array
    ofFreshArray array

[<CompiledName "RandomShuffleWith">]
let randomShuffleWith (random : Random) (source : IndexedList<'T>) =
    checkNotNull (nameof random) random
    let array = Snapshot.ofSeq (nameof source) source
    Array.randomShuffleInPlaceWith random array
    ofFreshArray array

// ---- Conversion ----

[<CompiledName "CopyTo">]
let blit (source : IndexedList<'T>) sourceIndex (target : 'T array) targetIndex count =
    checkNotNull (nameof source) source
    checkNotNull (nameof target) target

    // The checks of ImmutableList.CopyTo, with the names of the parameters of this function (ADR 0001, D6)
    ListRanges.blit
        (nameof sourceIndex)
        sourceIndex
        (nameof count)
        count
        source.Count
        (nameof targetIndex)
        targetIndex
        target.Length
    source.CopyTo (sourceIndex, target, targetIndex, count)

[<CompiledName "ToArray">]
let toArray (list : IndexedList<'T>) = Snapshot.ofSeq (nameof list) list

[<CompiledName "ToList">]
let toList (list : IndexedList<'T>) =
    checkNotNull (nameof list) list
    List.ofSeq list

[<CompiledName "ToSeq">]
let toSeq (list : IndexedList<'T>) =
    checkNotNull (nameof list) list
    list :> seq<'T>
