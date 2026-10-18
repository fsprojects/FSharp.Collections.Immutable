[<RequireQualifiedAccess>]
module FSharp.Collections.Immutable.FlatList

open System
open System.Collections.Generic
open System.Collections.Immutable
open System.Runtime.InteropServices

// Most functions unwrap the underlying array without a copy, call the FSharp.Core Array function, and wrap its fresh
// result without a copy (ADR 0001, D5), so that they have the semantics and the exceptions of the Array module (D6).

// ---- Creation ----

[<CompiledName "Create">]
let create count (value : 'T) = Array.create count value |> wrapArray

[<GeneralizableValue>]
[<CompiledName "Empty">]
let empty<'T> : FlatList<'T> = ImmutableArray<'T>.Empty

[<CompiledName "Initialize">]
let init count (initializer : int -> 'T) = Array.init count initializer |> wrapArray

[<CompiledName "OfArray">]
let ofArray (array : 'T array) = Array.copy array |> wrapArray

[<CompiledName "OfList">]
let ofList (list : 'T list) = Array.ofList list |> wrapArray

[<CompiledName "OfReadOnlySpan">]
let ofReadOnlySpan (span : ReadOnlySpan<'T>) = ImmutableArray.Create<'T> span

[<CompiledName "OfSeq">]
let ofSeq (source : seq<'T>) =
    match source with
    // An immutable array needs no copy; a default one is a default FlatList, which no function accepts (ADR 0001, D6)
    | :? ImmutableArray<'T> as array ->
        checkNotDefault (nameof source) array
        array
    | _ ->
        // A default flat list of a derived type is also a sequence of its base type, and Array.ofSeq would fail on it
        checkNotNullOrDefaultSeq (nameof source) source
        Array.ofSeq source |> wrapArray

[<CompiledName "OfSpan">]
let ofSpan (span : Span<'T>) = ImmutableArray.Create<'T> span

[<CompiledName "Replicate">]
let replicate count (initial : 'T) = Array.replicate count initial |> wrapArray

[<CompiledName "Singleton">]
let singleton (value : 'T) = ImmutableArray.Create<'T> value

[<CompiledName "Unfold">]
let unfold (generator : 'State -> struct ('T * 'State) voption) (state : 'State) =
    let builder = ImmutableArray.CreateBuilder<'T>()
    let mutable state = state
    let mutable go = true

    while go do
        match generator state with
        | ValueSome (struct (item, next)) ->
            builder.Add item
            state <- next
        | ValueNone -> go <- false

    builder.DrainToImmutable ()

// ---- Builder ----

[<CompiledName "Build">]
let build (action : FlatList<'T>.Builder -> unit) =
    let builder = ImmutableArray.CreateBuilder<'T>()
    action builder
    builder.DrainToImmutable ()

[<CompiledName "CreateBuilder">]
let builder () : FlatList<'T>.Builder = ImmutableArray.CreateBuilder<'T>()

[<CompiledName "CreateBuilderWithCapacity">]
let builderWith (capacity : int) : FlatList<'T>.Builder = ImmutableArray.CreateBuilder<'T> capacity

[<CompiledName "MoveFromBuilder">]
let moveFromBuilder (builder : FlatList<'T>.Builder) =
    checkNotNull (nameof builder) builder
    builder.MoveToImmutable ()

[<CompiledName "OfBuilder">]
let ofBuilder (builder : FlatList<'T>.Builder) =
    checkNotNull (nameof builder) builder
    builder.ToImmutable ()

[<CompiledName "ToBuilder">]
let toBuilder (array : FlatList<'T>) =
    checkNotDefault (nameof array) array
    array.ToBuilder ()

[<CompiledName "Update">]
let update (action : FlatList<'T>.Builder -> unit) (array : FlatList<'T>) =
    checkNotDefault (nameof array) array
    let builder = array.ToBuilder ()
    action builder
    builder.DrainToImmutable ()

// ---- Properties ----

[<CompiledName "IsEmpty">]
let isEmpty (array : FlatList<'T>) =
    checkNotDefault (nameof array) array
    array.IsEmpty

[<CompiledName "Length">]
let length (array : FlatList<'T>) =
    checkNotDefault (nameof array) array
    array.Length

// ---- Access ----

[<CompiledName "ExactlyOne">]
let exactlyOne (array : FlatList<'T>) = Array.exactlyOne (unwrapArray (nameof array) array)

[<CompiledName "Head">]
let head (array : FlatList<'T>) = Array.head (unwrapArray (nameof array) array)

[<CompiledName "Item">]
let item index (array : FlatList<'T>) = Array.item index (unwrapArray (nameof array) array)

[<CompiledName "Last">]
let last (array : FlatList<'T>) = Array.last (unwrapArray (nameof array) array)

[<CompiledName "Tail">]
let tail (array : FlatList<'T>) = Array.tail (unwrapArray (nameof array) array) |> wrapArray

[<CompiledName "TryExactlyOne">]
let tryExactlyOne (array : FlatList<'T>) =
    checkNotDefault (nameof array) array
    if array.Length = 1 then ValueSome array[0] else ValueNone

[<CompiledName "TryHead">]
let tryHead (array : FlatList<'T>) =
    checkNotDefault (nameof array) array
    if array.IsEmpty then ValueNone else ValueSome array[0]

[<CompiledName "TryItem">]
let tryItem index (array : FlatList<'T>) =
    checkNotDefault (nameof array) array

    if index < 0 || index >= array.Length then
        ValueNone
    else
        ValueSome array[index]

[<CompiledName "TryLast">]
let tryLast (array : FlatList<'T>) =
    checkNotDefault (nameof array) array

    if array.IsEmpty then
        ValueNone
    else
        ValueSome array[array.Length - 1]

[<CompiledName "TryTail">]
let tryTail (array : FlatList<'T>) =
    let source = unwrapArray (nameof array) array

    if source.Length = 0 then
        ValueNone
    else
        ValueSome (Array.tail source |> wrapArray)

// ---- Search ----

// Array.BinarySearch calls the count of a range length. The functions of this module report it as count, the name of
// their own parameter, with the type and the message of the BCL; the check of a range beyond the end stays with the BCL
let private checkBinarySearchRange index count =
    if index < 0 then
        raise (ArgumentOutOfRangeException (nameof index, ErrorStrings.NeedNonNegativeNumber))

    if count < 0 then
        raise (ArgumentOutOfRangeException (nameof count, ErrorStrings.NeedNonNegativeNumber))

[<CompiledName "BinarySearch">]
let binarySearch (value : 'T) (array : FlatList<'T>) =
    checkNotDefault (nameof array) array
    ImmutableArray.BinarySearch (array, value, DefaultComparer<'T>.Instance)

[<CompiledName "BinarySearchRange">]
let binarySearchRange index count (value : 'T) (array : FlatList<'T>) =
    checkNotDefault (nameof array) array
    checkBinarySearchRange index count
    ImmutableArray.BinarySearch (array, index, count, value, DefaultComparer<'T>.Instance)

[<CompiledName "BinarySearchRangeWithComparer">]
let binarySearchRangeWithComparer (comparer : IComparer<'T>) index count (value : 'T) (array : FlatList<'T>) =
    checkNotDefault (nameof array) array
    checkBinarySearchRange index count
    ImmutableArray.BinarySearch (array, index, count, value, comparer)

[<CompiledName "BinarySearchWithComparer">]
let binarySearchWithComparer (comparer : IComparer<'T>) (value : 'T) (array : FlatList<'T>) =
    checkNotDefault (nameof array) array
    ImmutableArray.BinarySearch (array, value, comparer)

[<CompiledName "Contains">]
let contains (value : 'T) (array : FlatList<'T>) = Array.contains value (unwrapArray (nameof array) array)

[<CompiledName "Exists">]
let exists predicate (array : FlatList<'T>) = Array.exists predicate (unwrapArray (nameof array) array)

[<CompiledName "Exists2">]
let exists2 predicate (array1 : FlatList<'T1>) (array2 : FlatList<'T2>) =
    Array.exists2 predicate (unwrapArray (nameof array1) array1) (unwrapArray (nameof array2) array2)

[<CompiledName "Find">]
let find predicate (array : FlatList<'T>) = Array.find predicate (unwrapArray (nameof array) array)

[<CompiledName "FindBack">]
let findBack predicate (array : FlatList<'T>) = Array.findBack predicate (unwrapArray (nameof array) array)

[<CompiledName "FindIndex">]
let findIndex predicate (array : FlatList<'T>) = Array.findIndex predicate (unwrapArray (nameof array) array)

[<CompiledName "FindIndexBack">]
let findIndexBack predicate (array : FlatList<'T>) = Array.findIndexBack predicate (unwrapArray (nameof array) array)

[<CompiledName "ForAll">]
let forall predicate (array : FlatList<'T>) = Array.forall predicate (unwrapArray (nameof array) array)

[<CompiledName "ForAll2">]
let forall2 predicate (array1 : FlatList<'T1>) (array2 : FlatList<'T2>) =
    Array.forall2 predicate (unwrapArray (nameof array1) array1) (unwrapArray (nameof array2) array2)

[<CompiledName "IndexOf">]
let indexOf (value : 'T) (array : FlatList<'T>) =
    checkNotDefault (nameof array) array
    array.IndexOf value

[<CompiledName "IndexOfRange">]
let indexOfRange (startIndex : int) (count : int) (value : 'T) (array : FlatList<'T>) =
    checkNotDefault (nameof array) array
    array.IndexOf (value, startIndex, count)

[<CompiledName "IndexOfRangeWith">]
let indexOfRangeWith (comparer : IEqualityComparer<'T>) (startIndex : int) (count : int) (value : 'T) (array : FlatList<'T>) =
    checkNotDefault (nameof array) array
    array.IndexOf (value, startIndex, count, comparer)

[<CompiledName "IndexOfWith">]
let indexOfWith (comparer : IEqualityComparer<'T>) (value : 'T) (array : FlatList<'T>) =
    checkNotDefault (nameof array) array
    array.IndexOf (value, 0, array.Length, comparer)

[<CompiledName "LastIndexOf">]
let lastIndexOf (value : 'T) (array : FlatList<'T>) =
    checkNotDefault (nameof array) array
    array.LastIndexOf value

[<CompiledName "LastIndexOfRange">]
let lastIndexOfRange (startIndex : int) (count : int) (value : 'T) (array : FlatList<'T>) =
    checkNotDefault (nameof array) array
    array.LastIndexOf (value, startIndex, count)

[<CompiledName "LastIndexOfRangeWith">]
let lastIndexOfRangeWith (comparer : IEqualityComparer<'T>) (startIndex : int) (count : int) (value : 'T) (array : FlatList<'T>) =
    checkNotDefault (nameof array) array
    array.LastIndexOf (value, startIndex, count, comparer)

[<CompiledName "LastIndexOfWith">]
let lastIndexOfWith (comparer : IEqualityComparer<'T>) (value : 'T) (array : FlatList<'T>) =
    checkNotDefault (nameof array) array

    // The BCL rejects the start index -1 of an empty array, so an empty array answers without asking it
    if array.IsEmpty then
        -1
    else
        array.LastIndexOf (value, array.Length - 1, array.Length, comparer)

[<CompiledName "Pick">]
let pick (chooser : 'T -> 'U voption) (array : FlatList<'T>) =
    let source = unwrapArray (nameof array) array
    let mutable result = ValueNone
    let mutable index = 0

    while result.IsNone && index < source.Length do
        result <- chooser source[index]
        index <- index + 1

    match result with
    | ValueSome value -> value
    | ValueNone -> Raise.indexNotFound ()

[<CompiledName "TryFind">]
let tryFind predicate (array : FlatList<'T>) =
    let source = unwrapArray (nameof array) array
    let mutable result = ValueNone
    let mutable index = 0

    while result.IsNone && index < source.Length do
        if predicate source[index] then
            result <- ValueSome source[index]

        index <- index + 1

    result

[<CompiledName "TryFindBack">]
let tryFindBack predicate (array : FlatList<'T>) =
    let source = unwrapArray (nameof array) array
    let mutable result = ValueNone
    let mutable index = source.Length - 1

    while result.IsNone && index >= 0 do
        if predicate source[index] then
            result <- ValueSome source[index]

        index <- index - 1

    result

[<CompiledName "TryFindIndex">]
let tryFindIndex predicate (array : FlatList<'T>) =
    let source = unwrapArray (nameof array) array
    let mutable result = ValueNone
    let mutable index = 0

    while result.IsNone && index < source.Length do
        if predicate source[index] then
            result <- ValueSome index

        index <- index + 1

    result

[<CompiledName "TryFindIndexBack">]
let tryFindIndexBack predicate (array : FlatList<'T>) =
    let source = unwrapArray (nameof array) array
    let mutable result = ValueNone
    let mutable index = source.Length - 1

    while result.IsNone && index >= 0 do
        if predicate source[index] then
            result <- ValueSome index

        index <- index - 1

    result

[<CompiledName "TryPick">]
let tryPick (chooser : 'T -> 'U voption) (array : FlatList<'T>) =
    let source = unwrapArray (nameof array) array
    let mutable result = ValueNone
    let mutable index = 0

    while result.IsNone && index < source.Length do
        result <- chooser source[index]
        index <- index + 1

    result

// ---- Transformation ----

[<CompiledName "Add">]
let add (value : 'T) (array : FlatList<'T>) =
    checkNotDefault (nameof array) array
    array.Add value

[<CompiledName "AllPairs">]
let allPairs (array1 : FlatList<'T1>) (array2 : FlatList<'T2>) =
    Arrays.allPairs structPair (unwrapArray (nameof array1) array1) (unwrapArray (nameof array2) array2)
    |> wrapArray

[<CompiledName "Append">]
let append (array1 : FlatList<'T>) (array2 : FlatList<'T>) =
    checkNotDefault (nameof array1) array1
    checkNotDefault (nameof array2) array2
    array1.AddRange array2

[<CompiledName "Choose">]
let choose (chooser : 'T -> 'U voption) (array : FlatList<'T>) =
    let source = unwrapArray (nameof array) array
    let builder = ImmutableArray.CreateBuilder<'U>()

    for item in source do
        match chooser item with
        | ValueSome value -> builder.Add value
        | ValueNone -> ()

    builder.DrainToImmutable ()

[<CompiledName "ChunkBySize">]
let chunkBySize chunkSize (array : FlatList<'T>) =
    Array.chunkBySize chunkSize (unwrapArray (nameof array) array)
    |> Array.map wrapArray
    |> wrapArray

[<CompiledName "Collect">]
let collect (mapping : 'T -> FlatList<'U>) (array : FlatList<'T>) =
    let source = unwrapArray (nameof array) array
    let builder = ImmutableArray.CreateBuilder<'U>()

    for item in source do
        let items = mapping item
        // Array.collect dereferences a null result; a default result is a default FlatList, rejected as in concat
        checkNotDefault (nameof mapping) items
        builder.AddRange items

    builder.DrainToImmutable ()

[<CompiledName "Concat">]
let concat (arrays : seq<FlatList<'T>>) =
    checkNotNullOrDefaultSeq (nameof arrays) arrays

    arrays
    |> Seq.map (fun array -> unwrapArray (nameof arrays) array)
    |> Array.concat
    |> wrapArray

[<CompiledName "Filter">]
let filter predicate (array : FlatList<'T>) =
    Array.filter predicate (unwrapArray (nameof array) array)
    |> wrapArray

[<CompiledName "Indexed">]
let indexed (array : FlatList<'T>) =
    Arrays.indexed structPair (unwrapArray (nameof array) array)
    |> wrapArray

[<CompiledName "InsertAt">]
let insertAt index (value : 'T) (source : FlatList<'T>) =
    Array.insertAt index value (unwrapArray (nameof source) source)
    |> wrapArray

[<CompiledName "InsertManyAt">]
let insertManyAt index (values : seq<'T>) (source : FlatList<'T>) =
    let array = unwrapArray (nameof source) source

    // Array.insertManyAt checks the index before it enumerates the values, and it names the values source then, as
    // Seq.toArray does; the check of the values comes after the one of the index and names the parameter of this function
    if index >= 0 && index <= array.Length then
        checkNotNullOrDefaultSeq (nameof values) values

    Array.insertManyAt index values array |> wrapArray

[<CompiledName "Map">]
let map (mapping : 'T -> 'U) (array : FlatList<'T>) =
    Array.map mapping (unwrapArray (nameof array) array)
    |> wrapArray

[<CompiledName "Map2">]
let map2 (mapping : 'T1 -> 'T2 -> 'U) (array1 : FlatList<'T1>) (array2 : FlatList<'T2>) =
    Array.map2 mapping (unwrapArray (nameof array1) array1) (unwrapArray (nameof array2) array2)
    |> wrapArray

[<CompiledName "Map3">]
let map3 (mapping : 'T1 -> 'T2 -> 'T3 -> 'U) (array1 : FlatList<'T1>) (array2 : FlatList<'T2>) (array3 : FlatList<'T3>) =
    Array.map3
        mapping
        (unwrapArray (nameof array1) array1)
        (unwrapArray (nameof array2) array2)
        (unwrapArray (nameof array3) array3)
    |> wrapArray

[<CompiledName "MapIndexed">]
let mapi (mapping : int -> 'T -> 'U) (array : FlatList<'T>) =
    Array.mapi mapping (unwrapArray (nameof array) array)
    |> wrapArray

[<CompiledName "MapIndexed2">]
let mapi2 (mapping : int -> 'T1 -> 'T2 -> 'U) (array1 : FlatList<'T1>) (array2 : FlatList<'T2>) =
    Array.mapi2 mapping (unwrapArray (nameof array1) array1) (unwrapArray (nameof array2) array2)
    |> wrapArray

[<CompiledName "Pairwise">]
let pairwise (array : FlatList<'T>) =
    Arrays.pairwise structPair (unwrapArray (nameof array) array)
    |> wrapArray

[<CompiledName "Permute">]
let permute indexMap (array : FlatList<'T>) =
    Array.permute indexMap (unwrapArray (nameof array) array)
    |> wrapArray

[<CompiledName "Remove">]
let remove (item : 'T) (source : FlatList<'T>) =
    checkNotDefault (nameof source) source
    source.Remove item

[<CompiledName "RemoveAt">]
let removeAt index (source : FlatList<'T>) =
    Array.removeAt index (unwrapArray (nameof source) source)
    |> wrapArray

[<CompiledName "RemoveMany">]
let removeMany (items : seq<'T>) (source : FlatList<'T>) =
    checkNotDefault (nameof source) source
    checkNotNullOrDefaultSeq (nameof items) items
    source.RemoveRange items

[<CompiledName "RemoveManyAt">]
let removeManyAt index count (source : FlatList<'T>) =
    Array.removeManyAt index count (unwrapArray (nameof source) source)
    |> wrapArray

[<CompiledName "RemoveManyWith">]
let removeManyWith (comparer : IEqualityComparer<'T>) (items : seq<'T>) (source : FlatList<'T>) =
    checkNotDefault (nameof source) source
    checkNotNullOrDefaultSeq (nameof items) items
    source.RemoveRange (items, comparer)

[<CompiledName "RemoveWith">]
let removeWith (comparer : IEqualityComparer<'T>) (item : 'T) (source : FlatList<'T>) =
    checkNotDefault (nameof source) source
    source.Remove (item, comparer)

[<CompiledName "Replace">]
let replace (oldValue : 'T) (newValue : 'T) (array : FlatList<'T>) =
    checkNotDefault (nameof array) array
    array.Replace (oldValue, newValue)

[<CompiledName "ReplaceWith">]
let replaceWith (comparer : IEqualityComparer<'T>) (oldValue : 'T) (newValue : 'T) (array : FlatList<'T>) =
    checkNotDefault (nameof array) array
    array.Replace (oldValue, newValue, comparer)

[<CompiledName "Reverse">]
let rev (array : FlatList<'T>) = Array.rev (unwrapArray (nameof array) array) |> wrapArray

[<CompiledName "Transpose">]
let transpose (arrays : seq<FlatList<'T>>) =
    checkNotNullOrDefaultSeq (nameof arrays) arrays

    arrays
    |> Seq.map (fun array -> unwrapArray (nameof arrays) array)
    |> Array.transpose
    |> Array.map wrapArray
    |> wrapArray

[<CompiledName "Unzip">]
let unzip (array : FlatList<struct ('T1 * 'T2)>) =
    let struct (first, second) = Arrays.unzip id (unwrapArray (nameof array) array)
    struct (wrapArray first, wrapArray second)

[<CompiledName "Unzip3">]
let unzip3 (array : FlatList<struct ('T1 * 'T2 * 'T3)>) =
    let struct (first, second, third) = Arrays.unzip3 id (unwrapArray (nameof array) array)
    struct (wrapArray first, wrapArray second, wrapArray third)

[<CompiledName "UpdateAt">]
let updateAt index (value : 'T) (source : FlatList<'T>) =
    Array.updateAt index value (unwrapArray (nameof source) source)
    |> wrapArray

[<CompiledName "Where">]
let where predicate (array : FlatList<'T>) =
    Array.filter predicate (unwrapArray (nameof array) array)
    |> wrapArray

[<CompiledName "Windowed">]
let windowed windowSize (array : FlatList<'T>) =
    Array.windowed windowSize (unwrapArray (nameof array) array)
    |> Array.map wrapArray
    |> wrapArray

[<CompiledName "Zip">]
let zip (array1 : FlatList<'T1>) (array2 : FlatList<'T2>) =
    let source1 = unwrapArray (nameof array1) array1
    let source2 = unwrapArray (nameof array2) array2
    ArrayChecks.sameLengths (nameof array1) source1 (nameof array2) source2
    Arrays.zip structPair source1 source2 |> wrapArray

[<CompiledName "Zip3">]
let zip3 (array1 : FlatList<'T1>) (array2 : FlatList<'T2>) (array3 : FlatList<'T3>) =
    let source1 = unwrapArray (nameof array1) array1
    let source2 = unwrapArray (nameof array2) array2
    let source3 = unwrapArray (nameof array3) array3
    ArrayChecks.sameLengths3 (nameof array1) source1 (nameof array2) source2 (nameof array3) source3

    Arrays.zip3 structTriple source1 source2 source3
    |> wrapArray

// ---- Folding ----

// The inline functions can use public members only, so they check and unwrap the array themselves
[<CompiledName "Average">]
let inline average (array : FlatList< ^T >) : ^T =
    if array.IsDefault then
        nullArg (nameof array)

    Array.average (
        ImmutableCollectionsMarshal.AsArray array
        |> Unchecked.nonNull
    )

[<CompiledName "AverageBy">]
let inline averageBy ([<InlineIfLambda>] projection : 'T -> ^U) (array : FlatList<'T>) : ^U =
    if array.IsDefault then
        nullArg (nameof array)

    Array.averageBy
        projection
        (ImmutableCollectionsMarshal.AsArray array
         |> Unchecked.nonNull)

[<CompiledName "CompareWith">]
let compareWith comparer (array1 : FlatList<'T>) (array2 : FlatList<'T>) =
    Array.compareWith comparer (unwrapArray (nameof array1) array1) (unwrapArray (nameof array2) array2)

[<CompiledName "Fold">]
let fold (folder : 'State -> 'T -> 'State) (state : 'State) (array : FlatList<'T>) =
    Array.fold folder state (unwrapArray (nameof array) array)

[<CompiledName "Fold2">]
let fold2 (folder : 'State -> 'T1 -> 'T2 -> 'State) (state : 'State) (array1 : FlatList<'T1>) (array2 : FlatList<'T2>) =
    Array.fold2 folder state (unwrapArray (nameof array1) array1) (unwrapArray (nameof array2) array2)

[<CompiledName "FoldBack">]
let foldBack (folder : 'T -> 'State -> 'State) (array : FlatList<'T>) (state : 'State) =
    Array.foldBack folder (unwrapArray (nameof array) array) state

[<CompiledName "FoldBack2">]
let foldBack2 (folder : 'T1 -> 'T2 -> 'State -> 'State) (array1 : FlatList<'T1>) (array2 : FlatList<'T2>) (state : 'State) =
    Array.foldBack2 folder (unwrapArray (nameof array1) array1) (unwrapArray (nameof array2) array2) state

[<CompiledName "Iterate">]
let iter action (array : FlatList<'T>) = Array.iter action (unwrapArray (nameof array) array)

[<CompiledName "Iterate2">]
let iter2 action (array1 : FlatList<'T1>) (array2 : FlatList<'T2>) =
    Array.iter2 action (unwrapArray (nameof array1) array1) (unwrapArray (nameof array2) array2)

[<CompiledName "IterateIndexed">]
let iteri action (array : FlatList<'T>) = Array.iteri action (unwrapArray (nameof array) array)

[<CompiledName "IterateIndexed2">]
let iteri2 action (array1 : FlatList<'T1>) (array2 : FlatList<'T2>) =
    Array.iteri2 action (unwrapArray (nameof array1) array1) (unwrapArray (nameof array2) array2)

[<CompiledName "MapFold">]
let mapFold (mapping : 'State -> 'T -> struct ('Result * 'State)) (state : 'State) (array : FlatList<'T>) =
    let source = unwrapArray (nameof array) array
    let results = Array.zeroCreate source.Length
    let mutable state = state

    for index = 0 to source.Length - 1 do
        let struct (result, next) = mapping state source[index]
        results[index] <- result
        state <- next

    struct (wrapArray results, state)

[<CompiledName "MapFoldBack">]
let mapFoldBack (mapping : 'T -> 'State -> struct ('Result * 'State)) (array : FlatList<'T>) (state : 'State) =
    let source = unwrapArray (nameof array) array
    let results = Array.zeroCreate source.Length
    let mutable state = state

    for index = source.Length - 1 downto 0 do
        let struct (result, next) = mapping source[index] state
        results[index] <- result
        state <- next

    struct (wrapArray results, state)

[<CompiledName "Max">]
let max (array : FlatList<'T>) = Array.max (unwrapArray (nameof array) array)

[<CompiledName "MaxBy">]
let maxBy (projection : 'T -> 'U) (array : FlatList<'T>) = Array.maxBy projection (unwrapArray (nameof array) array)

[<CompiledName "Min">]
let min (array : FlatList<'T>) = Array.min (unwrapArray (nameof array) array)

[<CompiledName "MinBy">]
let minBy (projection : 'T -> 'U) (array : FlatList<'T>) = Array.minBy projection (unwrapArray (nameof array) array)

[<CompiledName "Reduce">]
let reduce reduction (array : FlatList<'T>) = Array.reduce reduction (unwrapArray (nameof array) array)

[<CompiledName "ReduceBack">]
let reduceBack reduction (array : FlatList<'T>) = Array.reduceBack reduction (unwrapArray (nameof array) array)

[<CompiledName "Scan">]
let scan (folder : 'State -> 'T -> 'State) (state : 'State) (array : FlatList<'T>) =
    Array.scan folder state (unwrapArray (nameof array) array)
    |> wrapArray

[<CompiledName "ScanBack">]
let scanBack (folder : 'T -> 'State -> 'State) (array : FlatList<'T>) (state : 'State) =
    Array.scanBack folder (unwrapArray (nameof array) array) state
    |> wrapArray

[<CompiledName "Sum">]
let inline sum (array : FlatList< ^T >) : ^T =
    if array.IsDefault then
        nullArg (nameof array)

    Array.sum (
        ImmutableCollectionsMarshal.AsArray array
        |> Unchecked.nonNull
    )

[<CompiledName "SumBy">]
let inline sumBy ([<InlineIfLambda>] projection : 'T -> ^U) (array : FlatList<'T>) : ^U =
    if array.IsDefault then
        nullArg (nameof array)

    Array.sumBy
        projection
        (ImmutableCollectionsMarshal.AsArray array
         |> Unchecked.nonNull)

// ---- Partitioning and grouping ----

[<CompiledName "CountBy">]
let countBy (projection : 'T -> 'Key) (array : FlatList<'T>) =
    Arrays.countBy structPair projection (unwrapArray (nameof array) array)
    |> wrapArray

[<CompiledName "Distinct">]
let distinct (array : FlatList<'T>) =
    Array.distinct (unwrapArray (nameof array) array)
    |> wrapArray

[<CompiledName "DistinctBy">]
let distinctBy (projection : 'T -> 'Key) (array : FlatList<'T>) =
    Array.distinctBy projection (unwrapArray (nameof array) array)
    |> wrapArray

[<CompiledName "Except">]
let except (itemsToExclude : seq<'T>) (array : FlatList<'T>) =
    // FSharp.Core checks the items to exclude first
    checkNotNullOrDefaultSeq (nameof itemsToExclude) itemsToExclude
    Array.except itemsToExclude (unwrapArray (nameof array) array)
    |> wrapArray

[<CompiledName "GroupBy">]
let groupBy (projection : 'T -> 'Key) (array : FlatList<'T>) =
    Arrays.groupBy (fun key items -> struct (key, wrapArray items)) projection (unwrapArray (nameof array) array)
    |> wrapArray

[<CompiledName "Partition">]
let partition predicate (array : FlatList<'T>) =
    let accepted, rejected = Array.partition predicate (unwrapArray (nameof array) array)
    struct (wrapArray accepted, wrapArray rejected)

[<CompiledName "PartitionWith">]
let partitionWith (partitioner : 'T -> Choice<'T1, 'T2>) (array : FlatList<'T>) =
    let first, second = Array.partitionWith partitioner (unwrapArray (nameof array) array)
    struct (wrapArray first, wrapArray second)

// The range functions repeat the checks of Array.skip, Array.take and Array.splitAt and then slice: a slice copies the
// selected elements, unless the range is empty or covers the whole array (ADR 0001, D5)

[<CompiledName "Skip">]
let skip count (array : FlatList<'T>) =
    checkNotDefault (nameof array) array

    if count > array.Length then
        Raise.invalidArgOutOfRange (nameof count) count "array.Length" array.Length

    if count = array.Length then
        empty
    else
        let count = Operators.max count 0
        array.Slice (count, array.Length - count)

[<CompiledName "SkipLast">]
let skipLast count (array : FlatList<'T>) =
    checkNotDefault (nameof array) array

    if count > array.Length then
        Raise.invalidArgOutOfRange (nameof count) count "array.Length" array.Length

    if count = array.Length then
        empty
    else
        array.Slice (0, array.Length - Operators.max count 0)

[<CompiledName "SkipWhile">]
let skipWhile predicate (array : FlatList<'T>) =
    let source = unwrapArray (nameof array) array
    let mutable index = 0

    while index < source.Length && predicate source[index] do
        index <- index + 1

    array.Slice (index, source.Length - index)

[<CompiledName "SplitAt">]
let splitAt index (array : FlatList<'T>) =
    checkNotDefault (nameof array) array

    if index < 0 then
        Raise.invalidArgInputMustBeNonNegative (nameof index) index

    if array.Length < index then
        Raise.notEnoughElements ()

    struct (array.Slice (0, index), array.Slice (index, array.Length - index))

[<CompiledName "SplitInto">]
let splitInto count (array : FlatList<'T>) =
    Array.splitInto count (unwrapArray (nameof array) array)
    |> Array.map wrapArray
    |> wrapArray

[<CompiledName "GetSubArray">]
let sub (array : FlatList<'T>) startIndex count =
    checkNotDefault (nameof array) array

    if startIndex < 0 then
        Raise.invalidArgInputMustBeNonNegative (nameof startIndex) startIndex

    if count < 0 then
        Raise.invalidArgInputMustBeNonNegative (nameof count) count

    // Array.sub compares the sum of the start index and the count with the length, which can overflow; the difference
    // cannot, because the start index is not negative
    if count > array.Length - startIndex then
        Raise.invalidArgOutOfRange (nameof count) count "array.Length" array.Length

    array.Slice (startIndex, count)

[<CompiledName "Take">]
let take count (array : FlatList<'T>) =
    checkNotDefault (nameof array) array

    if count < 0 then
        Raise.invalidArgInputMustBeNonNegative (nameof count) count

    if count = 0 then
        empty
    else
        if count > array.Length then
            Raise.notEnoughElements ()

        array.Slice (0, count)

[<CompiledName "TakeLast">]
let takeLast count (array : FlatList<'T>) =
    checkNotDefault (nameof array) array

    if count < 0 then
        Raise.invalidArgInputMustBeNonNegative (nameof count) count

    if count = 0 then
        empty
    else
        if count > array.Length then
            Raise.notEnoughElements ()

        array.Slice (array.Length - count, count)

[<CompiledName "TakeWhile">]
let takeWhile predicate (array : FlatList<'T>) =
    let source = unwrapArray (nameof array) array
    let mutable count = 0

    while count < source.Length && predicate source[count] do
        count <- count + 1

    array.Slice (0, count)

[<CompiledName "Truncate">]
let truncate count (array : FlatList<'T>) =
    checkNotDefault (nameof array) array

    if count <= 0 then
        empty
    else
        array.Slice (0, Operators.min count array.Length)

// ---- Sorting ----

[<CompiledName "Sort">]
let sort (array : FlatList<'T>) = Array.sort (unwrapArray (nameof array) array) |> wrapArray

[<CompiledName "SortBy">]
let sortBy (projection : 'T -> 'Key) (array : FlatList<'T>) =
    Array.sortBy projection (unwrapArray (nameof array) array)
    |> wrapArray

[<CompiledName "SortByDescending">]
let sortByDescending (projection : 'T -> 'Key) (array : FlatList<'T>) =
    Array.sortByDescending projection (unwrapArray (nameof array) array)
    |> wrapArray

[<CompiledName "SortDescending">]
let sortDescending (array : FlatList<'T>) =
    Array.sortDescending (unwrapArray (nameof array) array)
    |> wrapArray

[<CompiledName "SortRange">]
let sortRange index count (array : FlatList<'T>) =
    checkNotDefault (nameof array) array
    array.Sort (index, count, DefaultComparer<'T>.Instance)

[<CompiledName "SortRangeWith">]
let sortRangeWith (comparison : 'T -> 'T -> int) index count (array : FlatList<'T>) =
    checkNotDefault (nameof array) array
    array.Sort (index, count, Comparer<'T>.Create(Comparison<'T> comparison))

[<CompiledName "SortRangeWithComparer">]
let sortRangeWithComparer (comparer : IComparer<'T>) index count (array : FlatList<'T>) =
    checkNotDefault (nameof array) array
    array.Sort (index, count, comparer)

[<CompiledName "SortWith">]
let sortWith comparer (array : FlatList<'T>) =
    Array.sortWith comparer (unwrapArray (nameof array) array)
    |> wrapArray

[<CompiledName "SortWithComparer">]
let sortWithComparer (comparer : IComparer<'T>) (array : FlatList<'T>) =
    checkNotDefault (nameof array) array
    array.Sort comparer

// ---- Random ----

[<CompiledName "RandomChoice">]
let randomChoice (source : FlatList<'T>) = Array.randomChoice (unwrapArray (nameof source) source)

[<CompiledName "RandomChoiceBy">]
let randomChoiceBy randomizer (source : FlatList<'T>) = Array.randomChoiceBy randomizer (unwrapArray (nameof source) source)

[<CompiledName "RandomChoiceWith">]
let randomChoiceWith (random : Random) (source : FlatList<'T>) =
    Array.randomChoiceWith random (unwrapArray (nameof source) source)

[<CompiledName "RandomChoices">]
let randomChoices count (source : FlatList<'T>) =
    Array.randomChoices count (unwrapArray (nameof source) source)
    |> wrapArray

[<CompiledName "RandomChoicesBy">]
let randomChoicesBy randomizer count (source : FlatList<'T>) =
    Array.randomChoicesBy randomizer count (unwrapArray (nameof source) source)
    |> wrapArray

[<CompiledName "RandomChoicesWith">]
let randomChoicesWith (random : Random) count (source : FlatList<'T>) =
    Array.randomChoicesWith random count (unwrapArray (nameof source) source)
    |> wrapArray

[<CompiledName "RandomSample">]
let randomSample count (source : FlatList<'T>) =
    Array.randomSample count (unwrapArray (nameof source) source)
    |> wrapArray

[<CompiledName "RandomSampleBy">]
let randomSampleBy randomizer count (source : FlatList<'T>) =
    Array.randomSampleBy randomizer count (unwrapArray (nameof source) source)
    |> wrapArray

[<CompiledName "RandomSampleWith">]
let randomSampleWith (random : Random) count (source : FlatList<'T>) =
    Array.randomSampleWith random count (unwrapArray (nameof source) source)
    |> wrapArray

[<CompiledName "RandomShuffle">]
let randomShuffle (source : FlatList<'T>) =
    Array.randomShuffle (unwrapArray (nameof source) source)
    |> wrapArray

[<CompiledName "RandomShuffleBy">]
let randomShuffleBy randomizer (source : FlatList<'T>) =
    Array.randomShuffleBy randomizer (unwrapArray (nameof source) source)
    |> wrapArray

[<CompiledName "RandomShuffleWith">]
let randomShuffleWith (random : Random) (source : FlatList<'T>) =
    Array.randomShuffleWith random (unwrapArray (nameof source) source)
    |> wrapArray

// ---- Conversion ----

[<CompiledName "AsMemory">]
let asMemory (array : FlatList<'T>) =
    checkNotDefault (nameof array) array
    array.AsMemory ()

[<CompiledName "AsSpan">]
let asSpan (array : FlatList<'T>) =
    checkNotDefault (nameof array) array
    array.AsSpan ()

[<CompiledName "CopyTo">]
let blit (source : FlatList<'T>) sourceIndex (target : 'T array) targetIndex count =
    Array.blit (unwrapArray (nameof source) source) sourceIndex target targetIndex count

[<CompiledName "ToArray">]
let toArray (array : FlatList<'T>) = Array.copy (unwrapArray (nameof array) array)

[<CompiledName "ToList">]
let toList (array : FlatList<'T>) = Array.toList (unwrapArray (nameof array) array)

[<CompiledName "ToSeq">]
let toSeq (array : FlatList<'T>) =
    checkNotDefault (nameof array) array
    array :> seq<'T>

// ---- Type-specific ----

[<CompiledName "IsDefault">]
let isDefault (array : FlatList<'T>) = array.IsDefault

[<CompiledName "IsDefaultOrEmpty">]
let isDefaultOrEmpty (array : FlatList<'T>) = array.IsDefaultOrEmpty

// Most functions unwrap the underlying array without a copy, call the Array.Parallel function, and wrap its fresh result
// without a copy (ADR 0001, D11), so that they have the semantics and the exceptions of Array.Parallel. Where the
// Array.Parallel function takes or returns options or builds a tuple per element or per group (choose, tryFind,
// tryFindIndex, tryPick, zip, groupBy), an algorithm of ParallelArrays with value options and struct tuples runs instead.
// Array.Parallel sorts a copy of its input, so the shared array is never changed.
[<RequireQualifiedAccess>]
module Parallel =

    // ---- Creation ----

    [<CompiledName "Initialize">]
    let init count (initializer : int -> 'T) = Array.Parallel.init count initializer |> wrapArray

    // ---- Search ----

    [<CompiledName "Exists">]
    let exists predicate (array : FlatList<'T>) = Array.Parallel.exists predicate (unwrapArray (nameof array) array)

    [<CompiledName "ForAll">]
    let forall predicate (array : FlatList<'T>) = Array.Parallel.forall predicate (unwrapArray (nameof array) array)

    [<CompiledName "TryFind">]
    let tryFind predicate (array : FlatList<'T>) =
        let source = unwrapArray (nameof array) array

        match ParallelArrays.tryFindIndex predicate source with
        | ValueSome index -> ValueSome source[index]
        | ValueNone -> ValueNone

    [<CompiledName "TryFindIndex">]
    let tryFindIndex predicate (array : FlatList<'T>) =
        ParallelArrays.tryFindIndex predicate (unwrapArray (nameof array) array)

    [<CompiledName "TryPick">]
    let tryPick (chooser : 'T -> 'U voption) (array : FlatList<'T>) =
        ParallelArrays.tryPick chooser (unwrapArray (nameof array) array)

    // ---- Transformation ----

    [<CompiledName "Choose">]
    let choose (chooser : 'T -> 'U voption) (array : FlatList<'T>) =
        ParallelArrays.choose chooser (unwrapArray (nameof array) array)
        |> wrapArray

    [<CompiledName "Collect">]
    let collect (mapping : 'T -> FlatList<'U>) (array : FlatList<'T>) =
        // As in Array.Parallel.collect, the mapping runs in parallel, and the results are copied in their order into one
        // array of their total length. Array.Parallel.collect dereferences a null result; a default flat list from the
        // mapping throws ArgumentNullException instead, as in the sequential collect (ADR 0001, D6).
        let results = Array.Parallel.map mapping (unwrapArray (nameof array) array)
        let mutable length = 0

        for result in results do
            checkNotDefault (nameof mapping) result
            length <- length + result.Length

        let builder = ImmutableArray.CreateBuilder<'U> length

        for result in results do
            builder.AddRange result

        builder.MoveToImmutable ()

    [<CompiledName "Filter">]
    let filter predicate (array : FlatList<'T>) =
        Array.Parallel.filter predicate (unwrapArray (nameof array) array)
        |> wrapArray

    [<CompiledName "Map">]
    let map (mapping : 'T -> 'U) (array : FlatList<'T>) =
        Array.Parallel.map mapping (unwrapArray (nameof array) array)
        |> wrapArray

    [<CompiledName "MapIndexed">]
    let mapi (mapping : int -> 'T -> 'U) (array : FlatList<'T>) =
        Array.Parallel.mapi mapping (unwrapArray (nameof array) array)
        |> wrapArray

    [<CompiledName "Zip">]
    let zip (array1 : FlatList<'T1>) (array2 : FlatList<'T2>) =
        let source1 = unwrapArray (nameof array1) array1
        let source2 = unwrapArray (nameof array2) array2
        ArrayChecks.sameLengths (nameof array1) source1 (nameof array2) source2
        ParallelArrays.zip structPair source1 source2 |> wrapArray

    // ---- Folding ----

    // The inline functions can use public members only, so they check and unwrap the array themselves
    [<CompiledName "Average">]
    let inline average (array : FlatList< ^T >) : ^T =
        if array.IsDefault then
            nullArg (nameof array)

        Array.Parallel.average (
            ImmutableCollectionsMarshal.AsArray array
            |> Unchecked.nonNull
        )

    [<CompiledName "AverageBy">]
    let inline averageBy ([<InlineIfLambda>] projection : 'T -> ^U) (array : FlatList<'T>) : ^U =
        if array.IsDefault then
            nullArg (nameof array)

        Array.Parallel.averageBy
            projection
            (ImmutableCollectionsMarshal.AsArray array
             |> Unchecked.nonNull)

    [<CompiledName "Iterate">]
    let iter action (array : FlatList<'T>) = Array.Parallel.iter action (unwrapArray (nameof array) array)

    [<CompiledName "IterateIndexed">]
    let iteri action (array : FlatList<'T>) = Array.Parallel.iteri action (unwrapArray (nameof array) array)

    [<CompiledName "Max">]
    let max (array : FlatList<'T>) = Array.Parallel.max (unwrapArray (nameof array) array)

    [<CompiledName "MaxBy">]
    let maxBy (projection : 'T -> 'U) (array : FlatList<'T>) =
        Array.Parallel.maxBy projection (unwrapArray (nameof array) array)

    [<CompiledName "Min">]
    let min (array : FlatList<'T>) = Array.Parallel.min (unwrapArray (nameof array) array)

    [<CompiledName "MinBy">]
    let minBy (projection : 'T -> 'U) (array : FlatList<'T>) =
        Array.Parallel.minBy projection (unwrapArray (nameof array) array)

    [<CompiledName "Reduce">]
    let reduce reduction (array : FlatList<'T>) = Array.Parallel.reduce reduction (unwrapArray (nameof array) array)

    [<CompiledName "ReduceBy">]
    let reduceBy (projection : 'T -> 'U) reduction (array : FlatList<'T>) =
        Array.Parallel.reduceBy projection reduction (unwrapArray (nameof array) array)

    // Array.Parallel.sum dereferences a null array without a check; a default flat list throws ArgumentNullException,
    // which its documentation promises
    [<CompiledName "Sum">]
    let inline sum (array : FlatList< ^T >) : ^T =
        if array.IsDefault then
            nullArg (nameof array)

        Array.Parallel.sum (
            ImmutableCollectionsMarshal.AsArray array
            |> Unchecked.nonNull
        )

    [<CompiledName "SumBy">]
    let inline sumBy ([<InlineIfLambda>] projection : 'T -> ^U) (array : FlatList<'T>) : ^U =
        if array.IsDefault then
            nullArg (nameof array)

        Array.Parallel.sumBy
            projection
            (ImmutableCollectionsMarshal.AsArray array
             |> Unchecked.nonNull)

    // ---- Partitioning and grouping ----

    [<CompiledName "GroupBy">]
    let groupBy (projection : 'T -> 'Key) (array : FlatList<'T>) =
        ParallelArrays.groupBy (fun key items -> struct (key, wrapArray items)) projection (unwrapArray (nameof array) array)
        |> wrapArray

    [<CompiledName "Partition">]
    let partition predicate (array : FlatList<'T>) =
        let accepted, rejected = Array.Parallel.partition predicate (unwrapArray (nameof array) array)
        struct (wrapArray accepted, wrapArray rejected)

    [<CompiledName "PartitionWith">]
    let partitionWith (partitioner : 'T -> Choice<'T1, 'T2>) (array : FlatList<'T>) =
        let first, second = Array.Parallel.partitionWith partitioner (unwrapArray (nameof array) array)
        struct (wrapArray first, wrapArray second)

    // ---- Sorting ----

    [<CompiledName "Sort">]
    let sort (array : FlatList<'T>) =
        Array.Parallel.sort (unwrapArray (nameof array) array)
        |> wrapArray

    [<CompiledName "SortBy">]
    let sortBy (projection : 'T -> 'Key) (array : FlatList<'T>) =
        Array.Parallel.sortBy projection (unwrapArray (nameof array) array)
        |> wrapArray

    [<CompiledName "SortByDescending">]
    let sortByDescending (projection : 'T -> 'Key) (array : FlatList<'T>) =
        Array.Parallel.sortByDescending projection (unwrapArray (nameof array) array)
        |> wrapArray

    [<CompiledName "SortDescending">]
    let sortDescending (array : FlatList<'T>) =
        Array.Parallel.sortDescending (unwrapArray (nameof array) array)
        |> wrapArray

    [<CompiledName "SortWith">]
    let sortWith comparer (array : FlatList<'T>) =
        Array.Parallel.sortWith comparer (unwrapArray (nameof array) array)
        |> wrapArray
