[<RequireQualifiedAccess>]
module FSharp.Collections.Immutable.IIndexedList

open System
open System.Collections.Generic
open System.Collections.Immutable
open System.Linq

// The interface has no members of ImmutableList such as ConvertAll or GetRange, so the functions take a snapshot of the
// elements, apply an Array function or a shared algorithm and build the result. A result with the element type of the
// input keeps the implementation of the input: it comes from Clear and AddRange or another member of the input. The
// checks of ListChecks throw what the List functions throw (ADR 0001, D5 and D6).

/// A list of the implementation of the input with the given elements.
let private rebuild (list : IIndexedList<'T>) (items : seq<'T>) = list.Clear().AddRange items

/// A range of a list in a list of its implementation: the list itself for the whole list. An indexed list copies the range
/// as IndexedList does, in O(log n + count); the other implementations are read from their start through the interface.
let private keepRange (list : IIndexedList<'T>) (start : int) (count : int) =
    if count = list.Count then
        list
    elif count = 0 then
        list.Clear ()
    else
        match list with
        | :? IndexedList<'T> as indexed -> IndexedList.sub indexed start count :> IIndexedList<'T>
        | _ -> list.Clear().AddRange(Enumerable.Take (Enumerable.Skip (list, start), count))

/// The index and the first element that satisfies the predicate, or ValueNone.
let private firstMatch (predicate : 'T -> bool) (list : IIndexedList<'T>) =
    use enumerator = list.GetEnumerator ()
    let mutable result = ValueNone
    let mutable index = 0

    while result.IsNone && enumerator.MoveNext () do
        let item = enumerator.Current

        if predicate item then
            result <- ValueSome (struct (index, item))

        index <- index + 1

    result

/// The index and the last element that satisfies the predicate, or ValueNone. An indexed list searches from its end, where
/// the other implementations are copied first, as List searches a copy from the end too: reading the copy takes O(n) when
/// the last element satisfies the predicate (about 3 ms against 0.005 ms for 100 000 integers), and the indexer of an
/// implementation can take O(n) for each element, so the copy is the safe way to go back through the interface.
let private lastMatch (predicate : 'T -> bool) (list : IIndexedList<'T>) =
    match list with
    | :? IndexedList<'T> as indexed ->
        let index = indexed.FindLastIndex (Predicate predicate)

        if index < 0 then
            ValueNone
        else
            ValueSome (struct (index, indexed[index]))
    | _ ->
        let array = Snapshot.ofSeq (nameof list) list
        let mutable index = array.Length - 1

        while index >= 0 && not (predicate array[index]) do
            index <- index - 1

        if index < 0 then
            ValueNone
        else
            ValueSome (struct (index, array[index]))

/// The first value that the chooser returns for the elements, in their order.
let private firstChoice (chooser : 'T -> 'U voption) (list : IIndexedList<'T>) =
    use enumerator = list.GetEnumerator ()
    let mutable result = ValueNone

    while result.IsNone && enumerator.MoveNext () do
        result <- chooser enumerator.Current

    result

// ---- Properties ----

[<CompiledName "IsEmpty">]
let isEmpty (list : IIndexedList<'T>) =
    checkNotNull (nameof list) list
    list.Count = 0

[<CompiledName "Length">]
let length (list : IIndexedList<'T>) =
    checkNotNull (nameof list) list
    list.Count

// ---- Access ----

[<CompiledName "ExactlyOne">]
let exactlyOne (list : IIndexedList<'T>) =
    checkNotNull (nameof list) list
    // List.exactlyOne reports its input under the name "source"
    ListChecks.exactlyOne "source" list.Count
    list[0]

[<CompiledName "Head">]
let head (list : IIndexedList<'T>) =
    checkNotNull (nameof list) list
    ListChecks.notEmpty (nameof list) list.Count
    list[0]

[<CompiledName "Item">]
let item index (list : IIndexedList<'T>) =
    checkNotNull (nameof list) list
    ListChecks.item (nameof index) index list.Count
    list[index]

[<CompiledName "Last">]
let last (list : IIndexedList<'T>) =
    checkNotNull (nameof list) list
    ListChecks.notEmpty (nameof list) list.Count
    list[list.Count - 1]

[<CompiledName "Tail">]
let tail (list : IIndexedList<'T>) =
    checkNotNull (nameof list) list
    ListChecks.notEmpty (nameof list) list.Count
    list.RemoveAt 0

[<CompiledName "TryExactlyOne">]
let tryExactlyOne (list : IIndexedList<'T>) =
    checkNotNull (nameof list) list
    if list.Count = 1 then ValueSome list[0] else ValueNone

[<CompiledName "TryHead">]
let tryHead (list : IIndexedList<'T>) =
    checkNotNull (nameof list) list
    if list.Count = 0 then ValueNone else ValueSome list[0]

[<CompiledName "TryItem">]
let tryItem index (list : IIndexedList<'T>) =
    checkNotNull (nameof list) list

    if index < 0 || index >= list.Count then
        ValueNone
    else
        ValueSome list[index]

[<CompiledName "TryLast">]
let tryLast (list : IIndexedList<'T>) =
    checkNotNull (nameof list) list

    if list.Count = 0 then
        ValueNone
    else
        ValueSome list[list.Count - 1]

[<CompiledName "TryTail">]
let tryTail (list : IIndexedList<'T>) =
    checkNotNull (nameof list) list
    if list.Count = 0 then
        ValueNone
    else
        ValueSome (list.RemoveAt 0)

[<CompiledName "TryUncons">]
let tryUncons (list : IIndexedList<'T>) =
    checkNotNull (nameof list) list

    if list.Count = 0 then
        ValueNone
    else
        ValueSome (struct (list[0], list.RemoveAt 0))

[<CompiledName "Uncons">]
let uncons (list : IIndexedList<'T>) =
    checkNotNull (nameof list) list
    // An empty list throws what List.head throws
    ListChecks.notEmpty (nameof list) list.Count
    struct (list[0], list.RemoveAt 0)

[<return : Struct>]
[<CompiledName "ConsPattern">]
let (|Cons|_|) (list : IIndexedList<'T>) = tryUncons list

[<CompiledName "NilPattern">]
let (|Nil|_|) (list : IIndexedList<'T>) =
    checkNotNull (nameof list) list
    list.Count = 0

// ---- Search ----

[<CompiledName "Contains">]
let contains (value : 'T) (source : IIndexedList<'T>) =
    checkNotNull (nameof source) source
    // F# equality with the value on the left, as List.contains compares
    Seq.contains value source

[<CompiledName "Exists">]
let exists (predicate : 'T -> bool) (list : IIndexedList<'T>) =
    checkNotNull (nameof list) list
    Seq.exists predicate list

// exists2, forall2 and compareWith can decide before the end. Two indexed lists are walked together by the functions of
// IndexedList, which read the elements up to the pair that decides. The other implementations are copied first, because
// an enumerator reached through the interface takes more than twice as long as the copy on a walk to the end (see
// IndexedList).

[<CompiledName "Exists2">]
let exists2 predicate (list1 : IIndexedList<'T1>) (list2 : IIndexedList<'T2>) =
    match list1, list2 with
    | (:? IndexedList<'T1> as first), (:? IndexedList<'T2> as second) -> IndexedList.exists2 predicate first second
    | _ -> ListArrays.exists2 (nameof list2) predicate (Snapshot.ofSeq (nameof list1) list1) (Snapshot.ofSeq (nameof list2) list2)

[<CompiledName "Find">]
let find (predicate : 'T -> bool) (list : IIndexedList<'T>) =
    checkNotNull (nameof list) list

    match firstMatch predicate list with
    | ValueSome (struct (_, item)) -> item
    | ValueNone -> Raise.indexNotFound ()

[<CompiledName "FindBack">]
let findBack (predicate : 'T -> bool) (list : IIndexedList<'T>) =
    match lastMatch predicate list with
    | ValueSome (struct (_, item)) -> item
    | ValueNone -> Raise.indexNotFound ()

[<CompiledName "FindIndex">]
let findIndex (predicate : 'T -> bool) (list : IIndexedList<'T>) =
    checkNotNull (nameof list) list

    match firstMatch predicate list with
    | ValueSome (struct (index, _)) -> index
    | ValueNone -> Raise.indexNotFound ()

[<CompiledName "FindIndexBack">]
let findIndexBack (predicate : 'T -> bool) (list : IIndexedList<'T>) =
    match lastMatch predicate list with
    | ValueSome (struct (index, _)) -> index
    | ValueNone -> Raise.indexNotFound ()

[<CompiledName "ForAll">]
let forall (predicate : 'T -> bool) (list : IIndexedList<'T>) =
    checkNotNull (nameof list) list
    Seq.forall predicate list

[<CompiledName "ForAll2">]
let forall2 predicate (list1 : IIndexedList<'T1>) (list2 : IIndexedList<'T2>) =
    match list1, list2 with
    | (:? IndexedList<'T1> as first), (:? IndexedList<'T2> as second) -> IndexedList.forall2 predicate first second
    | _ ->
        ListArrays.forall2
            (nameof list1)
            (nameof list2)
            predicate
            (Snapshot.ofSeq (nameof list1) list1)
            (Snapshot.ofSeq (nameof list2) list2)

[<CompiledName "IndexOf">]
let indexOf (value : 'T) (list : IIndexedList<'T>) =
    checkNotNull (nameof list) list
    ImmutableList.IndexOf (list, value)

[<CompiledName "IndexOfRange">]
let indexOfRange startIndex count (value : 'T) (list : IIndexedList<'T>) =
    checkNotNull (nameof list) list
    ListRanges.forward (nameof startIndex) startIndex (nameof count) count (nameof count) list.Count
    ImmutableList.IndexOf (list, value, startIndex, count)

[<CompiledName "IndexOfRangeWith">]
let indexOfRangeWith (comparer : IEqualityComparer<'T>) startIndex count (value : 'T) (list : IIndexedList<'T>) =
    checkNotNull (nameof list) list
    ListRanges.forward (nameof startIndex) startIndex (nameof count) count (nameof count) list.Count
    list.IndexOf (value, startIndex, count, comparer)

[<CompiledName "IndexOfWith">]
let indexOfWith (comparer : IEqualityComparer<'T>) (value : 'T) (list : IIndexedList<'T>) =
    checkNotNull (nameof list) list
    ImmutableList.IndexOf (list, value, comparer)

[<CompiledName "LastIndexOf">]
let lastIndexOf (value : 'T) (list : IIndexedList<'T>) =
    checkNotNull (nameof list) list
    ImmutableList.LastIndexOf (list, value)

[<CompiledName "LastIndexOfRange">]
let lastIndexOfRange startIndex count (value : 'T) (list : IIndexedList<'T>) =
    checkNotNull (nameof list) list
    ListRanges.backward (nameof startIndex) startIndex (nameof count) count list.Count

    // An empty range has no equal element; the member of ImmutableList rejects a start index of Int32.MaxValue for it
    if count = 0 then
        -1
    else
        ImmutableList.LastIndexOf (list, value, startIndex, count)

[<CompiledName "LastIndexOfRangeWith">]
let lastIndexOfRangeWith (comparer : IEqualityComparer<'T>) startIndex count (value : 'T) (list : IIndexedList<'T>) =
    checkNotNull (nameof list) list
    ListRanges.backward (nameof startIndex) startIndex (nameof count) count list.Count

    if count = 0 then
        -1
    else
        list.LastIndexOf (value, startIndex, count, comparer)

[<CompiledName "LastIndexOfWith">]
let lastIndexOfWith (comparer : IEqualityComparer<'T>) (value : 'T) (list : IIndexedList<'T>) =
    checkNotNull (nameof list) list
    ImmutableList.LastIndexOf (list, value, comparer)

[<CompiledName "Pick">]
let pick (chooser : 'T -> 'U voption) (list : IIndexedList<'T>) =
    checkNotNull (nameof list) list

    match firstChoice chooser list with
    | ValueSome value -> value
    | ValueNone -> Raise.indexNotFound ()

[<CompiledName "TryFind">]
let tryFind (predicate : 'T -> bool) (list : IIndexedList<'T>) =
    checkNotNull (nameof list) list

    match firstMatch predicate list with
    | ValueSome (struct (_, item)) -> ValueSome item
    | ValueNone -> ValueNone

[<CompiledName "TryFindBack">]
let tryFindBack (predicate : 'T -> bool) (list : IIndexedList<'T>) =
    match lastMatch predicate list with
    | ValueSome (struct (_, item)) -> ValueSome item
    | ValueNone -> ValueNone

[<CompiledName "TryFindIndex">]
let tryFindIndex (predicate : 'T -> bool) (list : IIndexedList<'T>) =
    checkNotNull (nameof list) list

    match firstMatch predicate list with
    | ValueSome (struct (index, _)) -> ValueSome index
    | ValueNone -> ValueNone

[<CompiledName "TryFindIndexBack">]
let tryFindIndexBack (predicate : 'T -> bool) (list : IIndexedList<'T>) =
    match lastMatch predicate list with
    | ValueSome (struct (index, _)) -> ValueSome index
    | ValueNone -> ValueNone

[<CompiledName "TryPick">]
let tryPick (chooser : 'T -> 'U voption) (list : IIndexedList<'T>) =
    checkNotNull (nameof list) list
    firstChoice chooser list

// ---- Transformation ----

[<CompiledName "Add">]
let add (value : 'T) (list : IIndexedList<'T>) =
    checkNotNull (nameof list) list
    list.Add value

[<CompiledName "AllPairs">]
let allPairs (list1 : IIndexedList<'T1>) (list2 : IIndexedList<'T2>) =
    // Every element of list1 meets every element of list2, so list2 is read again for each of them. Two snapshots read
    // each list once and then index arrays; reading list2 through an enumerator for each element of list1 took 1.4 to
    // 2.5 times as long, and a snapshot of list2 alone was not clearly faster (see IndexedList.allPairs).
    Arrays.allPairs structPair (Snapshot.ofSeq (nameof list1) list1) (Snapshot.ofSeq (nameof list2) list2)
    |> IndexedList.ofArray

[<CompiledName "Append">]
let append (list1 : IIndexedList<'T>) (list2 : IIndexedList<'T>) =
    checkNotNull (nameof list1) list1
    checkNotNull (nameof list2) list2
    list1.AddRange list2

[<CompiledName "Choose">]
let choose (chooser : 'T -> 'U voption) (list : IIndexedList<'T>) =
    checkNotNull (nameof list) list
    // As IndexedList.choose: the chosen elements wait in an array as long as the list (see Buffers)
    let chosen = Buffers.ChosenBuffer<'U> list.Count

    for item in list do
        match chooser item with
        | ValueSome value -> chosen.Add value
        | ValueNone -> ()

    chosen.ToImmutable ()

[<CompiledName "ChunkBySize">]
let chunkBySize chunkSize (list : IIndexedList<'T>) =
    checkNotNull (nameof list) list
    ListChecks.positive (nameof chunkSize) chunkSize

    // One copy of the list and one tree for each chunk. Cutting the chunks from the copy with spans, instead of the arrays
    // that Array.chunkBySize allocates, was between 1.2 times faster and as fast for chunks of 2, 100 and 10 000 elements
    // of a list of a million, so the array function stays.
    Array.chunkBySize chunkSize (Snapshot.ofSeq (nameof list) list)
    |> Seq.map IndexedList.ofArray
    |> IndexedList.ofSeq

[<CompiledName "Collect">]
let collect (mapping : 'T -> IIndexedList<'U>) (list : IIndexedList<'T>) =
    checkNotNull (nameof list) list
    // The collector copies the short results and joins the long indexed lists to its tree as they are (see Collectors)
    let collector = Collectors.ListCollector<'U>()

    for item in list do
        collector.Add (mapping item)

    collector.ToImmutable ()

[<CompiledName "Concat">]
let concat (lists : seq<IIndexedList<'T>>) =
    checkNotNullOrDefaultSeq (nameof lists) lists
    // The collector copies the short lists and joins the long indexed lists to its tree as they are (see Collectors)
    let collector = Collectors.ListCollector<'T>()

    for list in lists do
        // A null list is a null input, and so is a default flat list, which implements the interface and fails when it is
        // read, as a default element is for FlatList.concat (ADR 0001, D6)
        checkNotNullOrDefaultSeq (nameof lists) list
        collector.Add list

    collector.ToImmutable ()

[<CompiledName "Cons">]
let cons (value : 'T) (list : IIndexedList<'T>) =
    checkNotNull (nameof list) list
    list.Insert (0, value)

[<CompiledName "Filter">]
let filter (predicate : 'T -> bool) (list : IIndexedList<'T>) =
    let array = Snapshot.ofSeq (nameof list) list
    rebuild list (Array.filter predicate array)

[<CompiledName "Indexed">]
let indexed (list : IIndexedList<'T>) =
    Arrays.indexed structPair (Snapshot.ofSeq (nameof list) list)
    |> IndexedList.ofArray

[<CompiledName "InsertAt">]
let insertAt index (value : 'T) (source : IIndexedList<'T>) =
    checkNotNull (nameof source) source
    ListChecks.insertAt (nameof index) index source.Count
    source.Insert (index, value)

[<CompiledName "InsertManyAt">]
let insertManyAt index (values : seq<'T>) (source : IIndexedList<'T>) =
    checkNotNull (nameof source) source
    // List checks the index before it reads the values
    ListChecks.insertAt (nameof index) index source.Count
    checkNotNullOrDefaultSeq (nameof values) values
    source.InsertRange (index, values)

[<CompiledName "Map">]
let map (mapping : 'T -> 'U) (list : IIndexedList<'T>) =
    Array.map mapping (Snapshot.ofSeq (nameof list) list)
    |> IndexedList.ofArray

[<CompiledName "Map2">]
let map2 (mapping : 'T1 -> 'T2 -> 'U) (list1 : IIndexedList<'T1>) (list2 : IIndexedList<'T2>) =
    ListArrays.map2
        (nameof list1)
        (nameof list2)
        mapping
        (Snapshot.ofSeq (nameof list1) list1)
        (Snapshot.ofSeq (nameof list2) list2)
    |> IndexedList.ofArray

[<CompiledName "Map3">]
let map3 (mapping : 'T1 -> 'T2 -> 'T3 -> 'U) (list1 : IIndexedList<'T1>) (list2 : IIndexedList<'T2>) (list3 : IIndexedList<'T3>) =
    ListArrays.map3
        (nameof list1)
        (nameof list2)
        (nameof list3)
        mapping
        (Snapshot.ofSeq (nameof list1) list1)
        (Snapshot.ofSeq (nameof list2) list2)
        (Snapshot.ofSeq (nameof list3) list3)
    |> IndexedList.ofArray

[<CompiledName "MapIndexed">]
let mapi (mapping : int -> 'T -> 'U) (list : IIndexedList<'T>) =
    Array.mapi mapping (Snapshot.ofSeq (nameof list) list)
    |> IndexedList.ofArray

[<CompiledName "MapIndexed2">]
let mapi2 (mapping : int -> 'T1 -> 'T2 -> 'U) (list1 : IIndexedList<'T1>) (list2 : IIndexedList<'T2>) =
    ListArrays.mapi2
        (nameof list1)
        (nameof list2)
        mapping
        (Snapshot.ofSeq (nameof list1) list1)
        (Snapshot.ofSeq (nameof list2) list2)
    |> IndexedList.ofArray

[<CompiledName "Pairwise">]
let pairwise (list : IIndexedList<'T>) =
    Arrays.pairwise structPair (Snapshot.ofSeq (nameof list) list)
    |> IndexedList.ofArray

[<CompiledName "Permute">]
let permute indexMap (list : IIndexedList<'T>) =
    // List.permute applies the algorithm of Array.permute to a copy of the list, with the same checks
    let array = Snapshot.ofSeq (nameof list) list
    rebuild list (Array.permute indexMap array)

[<CompiledName "Remove">]
let remove (item : 'T) (list : IIndexedList<'T>) =
    checkNotNull (nameof list) list
    ImmutableList.Remove (list, item)

[<CompiledName "RemoveAt">]
let removeAt index (source : IIndexedList<'T>) =
    checkNotNull (nameof source) source
    ListChecks.removeAt (nameof index) index source.Count
    source.RemoveAt index

[<CompiledName "RemoveMany">]
let removeMany (items : seq<'T>) (list : IIndexedList<'T>) =
    checkNotNull (nameof list) list
    checkNotNullOrDefaultSeq (nameof items) items
    ImmutableList.RemoveRange (list, items)

[<CompiledName "RemoveManyAt">]
let removeManyAt index count (source : IIndexedList<'T>) =
    checkNotNull (nameof source) source
    ListChecks.removeManyAt (nameof index) index count source.Count
    // List walks the first index + count elements, so it removes nothing for a count that is not positive or for a sum
    // that overflows to a negative number
    if count <= 0 || index + count < 0 then
        source
    else
        source.RemoveRange (index, count)

[<CompiledName "RemoveManyWith">]
let removeManyWith (comparer : IEqualityComparer<'T>) (items : seq<'T>) (list : IIndexedList<'T>) =
    checkNotNull (nameof list) list
    checkNotNullOrDefaultSeq (nameof items) items
    list.RemoveRange (items, comparer)

[<CompiledName "RemoveWith">]
let removeWith (comparer : IEqualityComparer<'T>) (item : 'T) (list : IIndexedList<'T>) =
    checkNotNull (nameof list) list
    list.Remove (item, comparer)

[<CompiledName "Replace">]
let replace (oldValue : 'T) (newValue : 'T) (list : IIndexedList<'T>) =
    checkNotNull (nameof list) list
    ImmutableList.Replace (list, oldValue, newValue)

[<CompiledName "ReplaceWith">]
let replaceWith (comparer : IEqualityComparer<'T>) (oldValue : 'T) (newValue : 'T) (list : IIndexedList<'T>) =
    checkNotNull (nameof list) list
    list.Replace (oldValue, newValue, comparer)

[<CompiledName "Reverse">]
let rev (list : IIndexedList<'T>) =
    // The copy is fresh, so it is reversed in place, as IndexedList.rev does (ADR 0001, D5)
    let array = Snapshot.ofSeq (nameof list) list
    Array.Reverse array
    rebuild list array

[<CompiledName "Transpose">]
let transpose (lists : seq<IIndexedList<'T>>) =
    checkNotNullOrDefaultSeq (nameof lists) lists

    let arrays =
        lists
        |> Seq.map (fun list ->
            // A null list, or a default flat list that implements the interface, is a null input named after the sequence
            checkNotNullOrDefaultSeq (nameof lists) list
            Snapshot.ofSeq (nameof lists) list
        )
        |> Seq.toArray

    // List.transpose names the lists "list.[0]", "list.[1]", …; once their lengths agree, Array.transpose agrees with it
    ListChecks.transpose "list" (arrays |> Array.map _.Length)

    Array.transpose arrays
    |> Seq.map IndexedList.ofArray
    |> IndexedList.ofSeq

[<CompiledName "Unzip">]
let unzip (list : IIndexedList<struct ('T1 * 'T2)>) =
    let struct (first, second) = Arrays.unzip id (Snapshot.ofSeq (nameof list) list)
    struct (IndexedList.ofArray first, IndexedList.ofArray second)

[<CompiledName "Unzip3">]
let unzip3 (list : IIndexedList<struct ('T1 * 'T2 * 'T3)>) =
    let struct (first, second, third) = Arrays.unzip3 id (Snapshot.ofSeq (nameof list) list)
    struct (IndexedList.ofArray first, IndexedList.ofArray second, IndexedList.ofArray third)

[<CompiledName "UpdateAt">]
let updateAt index (value : 'T) (source : IIndexedList<'T>) =
    checkNotNull (nameof source) source
    ListChecks.removeAt (nameof index) index source.Count
    source.SetItem (index, value)

[<CompiledName "Where">]
let where (predicate : 'T -> bool) (list : IIndexedList<'T>) =
    let array = Snapshot.ofSeq (nameof list) list
    rebuild list (Array.filter predicate array)

[<CompiledName "Windowed">]
let windowed windowSize (list : IIndexedList<'T>) =
    checkNotNull (nameof list) list
    ListChecks.positive (nameof windowSize) windowSize

    Array.windowed windowSize (Snapshot.ofSeq (nameof list) list)
    |> Seq.map IndexedList.ofArray
    |> IndexedList.ofSeq

[<CompiledName "Zip">]
let zip (list1 : IIndexedList<'T1>) (list2 : IIndexedList<'T2>) =
    checkNotNull (nameof list1) list1
    checkNotNull (nameof list2) list2
    ListChecks.sameLengths (nameof list1) list1.Count (nameof list2) list2.Count

    Arrays.zip structPair (Snapshot.ofSeq (nameof list1) list1) (Snapshot.ofSeq (nameof list2) list2)
    |> IndexedList.ofArray

[<CompiledName "Zip3">]
let zip3 (list1 : IIndexedList<'T1>) (list2 : IIndexedList<'T2>) (list3 : IIndexedList<'T3>) =
    checkNotNull (nameof list1) list1
    checkNotNull (nameof list2) list2
    checkNotNull (nameof list3) list3
    ListChecks.zip3 (nameof list1) list1.Count (nameof list2) list2.Count (nameof list3) list3.Count

    Arrays.zip3
        structTriple
        (Snapshot.ofSeq (nameof list1) list1)
        (Snapshot.ofSeq (nameof list2) list2)
        (Snapshot.ofSeq (nameof list3) list3)
    |> IndexedList.ofArray

// ---- Folding ----

// The inline functions can use public members only, so they check the list themselves; the empty list throws what
// List.average and List.averageBy throw, with the name "source"
[<CompiledName "Average">]
let inline average (list : IIndexedList< ^T >) : ^T =
    if obj.ReferenceEquals (list, null) then
        nullArg (nameof list)

    if list.Count = 0 then
        invalidArg "source" ErrorStrings.InputSequenceEmpty

    let mutable sum = LanguagePrimitives.GenericZero< ^T>

    for item in list do
        sum <- Checked.(+) sum item

    LanguagePrimitives.DivideByInt< ^T> sum list.Count

[<CompiledName "AverageBy">]
let inline averageBy ([<InlineIfLambda>] projection : 'T -> ^U) (list : IIndexedList<'T>) : ^U =
    if obj.ReferenceEquals (list, null) then
        nullArg (nameof list)

    if list.Count = 0 then
        invalidArg "source" ErrorStrings.InputSequenceEmpty

    let mutable sum = LanguagePrimitives.GenericZero< ^U>

    for item in list do
        sum <- Checked.(+) sum (projection item)

    LanguagePrimitives.DivideByInt< ^U> sum list.Count

[<CompiledName "CompareWith">]
let compareWith (comparer : 'T -> 'T -> int) (list1 : IIndexedList<'T>) (list2 : IIndexedList<'T>) =
    match list1, list2 with
    | (:? IndexedList<'T> as first), (:? IndexedList<'T> as second) -> IndexedList.compareWith comparer first second
    | _ ->
        // Array.compareWith compares as List.compareWith does: the first difference, then the lengths
        Array.compareWith comparer (Snapshot.ofSeq (nameof list1) list1) (Snapshot.ofSeq (nameof list2) list2)

[<CompiledName "Fold">]
let fold (folder : 'State -> 'T -> 'State) (state : 'State) (list : IIndexedList<'T>) =
    checkNotNull (nameof list) list
    let mutable state = state

    for item in list do
        state <- folder state item

    state

[<CompiledName "Fold2">]
let fold2 (folder : 'State -> 'T1 -> 'T2 -> 'State) (state : 'State) (list1 : IIndexedList<'T1>) (list2 : IIndexedList<'T2>) =
    ListArrays.fold2
        (nameof list1)
        (nameof list2)
        folder
        state
        (Snapshot.ofSeq (nameof list1) list1)
        (Snapshot.ofSeq (nameof list2) list2)

[<CompiledName "FoldBack">]
let foldBack (folder : 'T -> 'State -> 'State) (list : IIndexedList<'T>) (state : 'State) =
    Array.foldBack folder (Snapshot.ofSeq (nameof list) list) state

[<CompiledName "FoldBack2">]
let foldBack2 (folder : 'T1 -> 'T2 -> 'State -> 'State) (list1 : IIndexedList<'T1>) (list2 : IIndexedList<'T2>) (state : 'State) =
    checkNotNull (nameof list1) list1
    checkNotNull (nameof list2) list2
    // List.foldBack2 checks the lengths before it calls the folder
    ListChecks.foldBack2 (nameof list1) list1.Count (nameof list2) list2.Count
    Array.foldBack2 folder (Snapshot.ofSeq (nameof list1) list1) (Snapshot.ofSeq (nameof list2) list2) state

[<CompiledName "Iterate">]
let iter (action : 'T -> unit) (list : IIndexedList<'T>) =
    checkNotNull (nameof list) list

    for item in list do
        action item

[<CompiledName "Iterate2">]
let iter2 (action : 'T1 -> 'T2 -> unit) (list1 : IIndexedList<'T1>) (list2 : IIndexedList<'T2>) =
    ListArrays.iter2
        (nameof list1)
        (nameof list2)
        action
        (Snapshot.ofSeq (nameof list1) list1)
        (Snapshot.ofSeq (nameof list2) list2)

[<CompiledName "IterateIndexed">]
let iteri (action : int -> 'T -> unit) (list : IIndexedList<'T>) =
    checkNotNull (nameof list) list
    let mutable index = 0

    for item in list do
        action index item
        index <- index + 1

[<CompiledName "IterateIndexed2">]
let iteri2 (action : int -> 'T1 -> 'T2 -> unit) (list1 : IIndexedList<'T1>) (list2 : IIndexedList<'T2>) =
    ListArrays.iteri2
        (nameof list1)
        (nameof list2)
        action
        (Snapshot.ofSeq (nameof list1) list1)
        (Snapshot.ofSeq (nameof list2) list2)

[<CompiledName "MapFold">]
let mapFold (mapping : 'State -> 'T -> struct ('Result * 'State)) (state : 'State) (list : IIndexedList<'T>) =
    let source = Snapshot.ofSeq (nameof list) list
    let results = Array.zeroCreate source.Length
    let mutable state = state

    for index = 0 to source.Length - 1 do
        let struct (result, next) = mapping state source[index]
        results[index] <- result
        state <- next

    struct (IndexedList.ofArray results, state)

[<CompiledName "MapFoldBack">]
let mapFoldBack (mapping : 'T -> 'State -> struct ('Result * 'State)) (list : IIndexedList<'T>) (state : 'State) =
    let source = Snapshot.ofSeq (nameof list) list
    let results = Array.zeroCreate source.Length
    let mutable state = state

    for index = source.Length - 1 downto 0 do
        let struct (result, next) = mapping source[index] state
        results[index] <- result
        state <- next

    struct (IndexedList.ofArray results, state)

// max, maxBy, min and minBy are inline, as the ones of List are, and compare as they do; see IndexedList for the reasons.

[<CompiledName "Max">]
let inline max (list : IIndexedList<'T>) : 'T =
    if obj.ReferenceEquals (list, null) then
        nullArg (nameof list)

    if list.Count = 0 then
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
let inline maxBy ([<InlineIfLambda>] projection : 'T -> 'U) (list : IIndexedList<'T>) : 'T =
    if obj.ReferenceEquals (list, null) then
        nullArg (nameof list)

    if list.Count = 0 then
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
let inline min (list : IIndexedList<'T>) : 'T =
    if obj.ReferenceEquals (list, null) then
        nullArg (nameof list)

    if list.Count = 0 then
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
let inline minBy ([<InlineIfLambda>] projection : 'T -> 'U) (list : IIndexedList<'T>) : 'T =
    if obj.ReferenceEquals (list, null) then
        nullArg (nameof list)

    if list.Count = 0 then
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
let reduce (reduction : 'T -> 'T -> 'T) (list : IIndexedList<'T>) =
    checkNotNull (nameof list) list
    ListChecks.notEmpty (nameof list) list.Count
    Seq.reduce reduction list

[<CompiledName "ReduceBack">]
let reduceBack (reduction : 'T -> 'T -> 'T) (list : IIndexedList<'T>) =
    checkNotNull (nameof list) list
    ListChecks.notEmpty (nameof list) list.Count
    Array.reduceBack reduction (Snapshot.ofSeq (nameof list) list)

[<CompiledName "Scan">]
let scan (folder : 'State -> 'T -> 'State) (state : 'State) (list : IIndexedList<'T>) =
    Array.scan folder state (Snapshot.ofSeq (nameof list) list)
    |> IndexedList.ofArray

[<CompiledName "ScanBack">]
let scanBack (folder : 'T -> 'State -> 'State) (list : IIndexedList<'T>) (state : 'State) =
    Array.scanBack folder (Snapshot.ofSeq (nameof list) list) state
    |> IndexedList.ofArray

[<CompiledName "Sum">]
let inline sum (list : IIndexedList< ^T >) : ^T =
    if obj.ReferenceEquals (list, null) then
        nullArg (nameof list)

    let mutable sum = LanguagePrimitives.GenericZero< ^T>

    for item in list do
        sum <- Checked.(+) sum item

    sum

[<CompiledName "SumBy">]
let inline sumBy ([<InlineIfLambda>] projection : 'T -> ^U) (list : IIndexedList<'T>) : ^U =
    if obj.ReferenceEquals (list, null) then
        nullArg (nameof list)

    let mutable sum = LanguagePrimitives.GenericZero< ^U>

    for item in list do
        sum <- Checked.(+) sum (projection item)

    sum

// ---- Partitioning and grouping ----

[<CompiledName "CountBy">]
let countBy (projection : 'T -> 'Key) (list : IIndexedList<'T>) =
    Arrays.countBy structPair projection (Snapshot.ofSeq (nameof list) list)
    |> IndexedList.ofArray

[<CompiledName "Distinct">]
let distinct (list : IIndexedList<'T>) =
    let array = Snapshot.ofSeq (nameof list) list
    rebuild list (Array.distinct array)

[<CompiledName "DistinctBy">]
let distinctBy (projection : 'T -> 'Key) (list : IIndexedList<'T>) =
    let array = Snapshot.ofSeq (nameof list) list
    rebuild list (Array.distinctBy projection array)

[<CompiledName "Except">]
let except (itemsToExclude : seq<'T>) (list : IIndexedList<'T>) =
    // List.except checks the items to exclude first and does not read them for an empty list
    checkNotNullOrDefaultSeq (nameof itemsToExclude) itemsToExclude
    checkNotNull (nameof list) list

    if list.Count = 0 then
        list
    else
        rebuild list (Array.except itemsToExclude (Snapshot.ofSeq (nameof list) list))

[<CompiledName "GroupBy">]
let groupBy (projection : 'T -> 'Key) (list : IIndexedList<'T>) =
    Arrays.groupBy (fun key items -> struct (key, IndexedList.ofArray items)) projection (Snapshot.ofSeq (nameof list) list)
    |> IndexedList.ofArray

[<CompiledName "Partition">]
let partition (predicate : 'T -> bool) (list : IIndexedList<'T>) =
    let accepted, rejected = Array.partition predicate (Snapshot.ofSeq (nameof list) list)
    struct (rebuild list accepted, rebuild list rejected)

[<CompiledName "PartitionWith">]
let partitionWith (partitioner : 'T -> Choice<'T1, 'T2>) (list : IIndexedList<'T>) =
    let first, second = Array.partitionWith partitioner (Snapshot.ofSeq (nameof list) list)
    struct (IndexedList.ofArray first, IndexedList.ofArray second)

[<CompiledName "Skip">]
let skip count (list : IIndexedList<'T>) =
    checkNotNull (nameof list) list
    ListChecks.skip (nameof count) count list.Count
    let count = Operators.max count 0
    keepRange list count (list.Count - count)

[<CompiledName "SkipLast">]
let skipLast count (list : IIndexedList<'T>) =
    checkNotNull (nameof list) list
    // The checks of List.skip
    ListChecks.skip (nameof count) count list.Count
    keepRange list 0 (list.Count - Operators.max count 0)

[<CompiledName "SkipWhile">]
let skipWhile (predicate : 'T -> bool) (list : IIndexedList<'T>) =
    checkNotNull (nameof list) list

    match firstMatch (fun item -> not (predicate item)) list with
    | ValueSome (struct (index, _)) -> keepRange list index (list.Count - index)
    | ValueNone -> list.Clear ()

[<CompiledName "SplitAt">]
let splitAt index (list : IIndexedList<'T>) =
    checkNotNull (nameof list) list
    ListChecks.splitAt (nameof index) index list.Count
    struct (keepRange list 0 index, keepRange list index (list.Count - index))

[<CompiledName "SplitInto">]
let splitInto count (list : IIndexedList<'T>) =
    checkNotNull (nameof list) list
    ListChecks.positive (nameof count) count

    Array.splitInto count (Snapshot.ofSeq (nameof list) list)
    |> Seq.map IndexedList.ofArray
    |> IndexedList.ofSeq

[<CompiledName "Take">]
let take count (list : IIndexedList<'T>) =
    checkNotNull (nameof list) list
    ListChecks.take (nameof count) count list.Count
    keepRange list 0 count

[<CompiledName "TakeLast">]
let takeLast count (list : IIndexedList<'T>) =
    checkNotNull (nameof list) list
    // The checks of List.take
    ListChecks.take (nameof count) count list.Count
    keepRange list (list.Count - count) count

[<CompiledName "TakeWhile">]
let takeWhile (predicate : 'T -> bool) (list : IIndexedList<'T>) =
    checkNotNull (nameof list) list

    match firstMatch (fun item -> not (predicate item)) list with
    | ValueSome (struct (index, _)) -> keepRange list 0 index
    | ValueNone -> list

[<CompiledName "Truncate">]
let truncate count (list : IIndexedList<'T>) =
    checkNotNull (nameof list) list
    keepRange list 0 (Operators.max 0 (Operators.min count list.Count))

// ---- Sorting ----

// The sorts are stable, as the sorts of List are (ADR 0001, D5). The functions that compare the elements, or that call
// the projection in the comparison as List.sortByDescending does, sort with the Seq functions of FSharp.Core, which share
// the stable sort of List: the same comparisons run in Array.Sort, which wraps the exception of a comparison in an
// InvalidOperationException (D6). Seq.sortDescending and Seq.sortByDescending are inline and box the elements in every
// comparison once they are compiled into a generic function, so the two descending sorts call Seq.sortWith through
// DescendingSorts, with the cached comparer. sortBy and sortWithComparer sort with LINQ, which computes the keys first, as
// List.sortBy does, and wraps the exception of a comparison as Array.Sort does. A list of one element is returned without
// a call of the projection or the comparer, as List returns it.

[<CompiledName "Sort">]
let sort (list : IIndexedList<'T>) =
    checkNotNull (nameof list) list
    if list.Count <= 1 then
        list
    else
        rebuild list (Seq.sort list)

[<CompiledName "SortBy">]
let sortBy (projection : 'T -> 'Key) (list : IIndexedList<'T>) =
    checkNotNull (nameof list) list

    if list.Count <= 1 then
        list
    else
        rebuild list (list.OrderBy(Func<'T, 'Key> projection, DefaultComparer<'Key>.Instance).ToArray())

[<CompiledName "SortByDescending">]
let sortByDescending (projection : 'T -> 'Key) (list : IIndexedList<'T>) =
    checkNotNull (nameof list) list
    if list.Count <= 1 then
        list
    else
        rebuild list (DescendingSorts.sortByDescending projection list)

[<CompiledName "SortDescending">]
let sortDescending (list : IIndexedList<'T>) =
    checkNotNull (nameof list) list
    if list.Count <= 1 then
        list
    else
        rebuild list (DescendingSorts.sortDescending list)

[<CompiledName "SortWith">]
let sortWith (comparer : 'T -> 'T -> int) (list : IIndexedList<'T>) =
    checkNotNull (nameof list) list
    if list.Count <= 1 then
        list
    else
        rebuild list (Seq.sortWith comparer list)

[<CompiledName "SortWithComparer">]
let sortWithComparer (comparer : IComparer<'T>) (list : IIndexedList<'T>) =
    checkNotNull (nameof list) list

    if list.Count <= 1 then
        list
    else
        rebuild list (list.Order(comparer).ToArray())

// ---- Random ----

// The functions draw the random numbers as the List functions do; the ones that build a list apply the Array function
// to a snapshot after the List checks

[<CompiledName "RandomChoice">]
let randomChoice (source : IIndexedList<'T>) =
    checkNotNull (nameof source) source
    ListChecks.notEmptySequence (nameof source) source.Count
    source[Random.Shared.Next (0, source.Count)]

[<CompiledName "RandomChoiceBy">]
let randomChoiceBy (randomizer : unit -> float) (source : IIndexedList<'T>) =
    checkNotNull (nameof source) source
    ListChecks.notEmptySequence (nameof source) source.Count
    source[ListChecks.randomIndex (nameof randomizer) randomizer source.Count]

[<CompiledName "RandomChoiceWith">]
let randomChoiceWith (random : Random) (source : IIndexedList<'T>) =
    checkNotNull (nameof random) random
    checkNotNull (nameof source) source
    ListChecks.notEmptySequence (nameof source) source.Count
    source[random.Next (0, source.Count)]

[<CompiledName "RandomChoices">]
let randomChoices count (source : IIndexedList<'T>) =
    checkNotNull (nameof source) source
    ListChecks.randomChoices (nameof count) count (nameof source) source.Count
    rebuild source (Array.randomChoices count (Snapshot.ofSeq (nameof source) source))

[<CompiledName "RandomChoicesBy">]
let randomChoicesBy (randomizer : unit -> float) count (source : IIndexedList<'T>) =
    checkNotNull (nameof source) source
    ListChecks.randomChoices (nameof count) count (nameof source) source.Count
    rebuild source (Array.randomChoicesBy randomizer count (Snapshot.ofSeq (nameof source) source))

[<CompiledName "RandomChoicesWith">]
let randomChoicesWith (random : Random) count (source : IIndexedList<'T>) =
    checkNotNull (nameof random) random
    checkNotNull (nameof source) source
    ListChecks.randomChoices (nameof count) count (nameof source) source.Count
    rebuild source (Array.randomChoicesWith random count (Snapshot.ofSeq (nameof source) source))

[<CompiledName "RandomSample">]
let randomSample count (source : IIndexedList<'T>) =
    checkNotNull (nameof source) source
    ListChecks.randomSample (nameof count) count (nameof source) source.Count
    rebuild source (Array.randomSample count (Snapshot.ofSeq (nameof source) source))

[<CompiledName "RandomSampleBy">]
let randomSampleBy (randomizer : unit -> float) count (source : IIndexedList<'T>) =
    checkNotNull (nameof source) source
    ListChecks.randomSample (nameof count) count (nameof source) source.Count
    rebuild source (Array.randomSampleBy randomizer count (Snapshot.ofSeq (nameof source) source))

[<CompiledName "RandomSampleWith">]
let randomSampleWith (random : Random) count (source : IIndexedList<'T>) =
    checkNotNull (nameof random) random
    checkNotNull (nameof source) source
    ListChecks.randomSample (nameof count) count (nameof source) source.Count
    rebuild source (Array.randomSampleWith random count (Snapshot.ofSeq (nameof source) source))

[<CompiledName "RandomShuffle">]
let randomShuffle (source : IIndexedList<'T>) =
    let array = Snapshot.ofSeq (nameof source) source
    Array.randomShuffleInPlace array
    rebuild source array

[<CompiledName "RandomShuffleBy">]
let randomShuffleBy (randomizer : unit -> float) (source : IIndexedList<'T>) =
    let array = Snapshot.ofSeq (nameof source) source
    Array.randomShuffleInPlaceBy randomizer array
    rebuild source array

[<CompiledName "RandomShuffleWith">]
let randomShuffleWith (random : Random) (source : IIndexedList<'T>) =
    checkNotNull (nameof random) random
    let array = Snapshot.ofSeq (nameof source) source
    Array.randomShuffleInPlaceWith random array
    rebuild source array

// ---- Conversion ----

[<CompiledName "ToArray">]
let toArray (list : IIndexedList<'T>) = Snapshot.ofSeq (nameof list) list

[<CompiledName "ToList">]
let toList (list : IIndexedList<'T>) =
    checkNotNull (nameof list) list
    List.ofSeq list

[<CompiledName "ToSeq">]
let toSeq (list : IIndexedList<'T>) =
    checkNotNull (nameof list) list
    list :> seq<'T>

// ---- Type-specific ----

[<CompiledName "Clear">]
let clear (list : IIndexedList<'T>) =
    checkNotNull (nameof list) list
    list.Clear ()
