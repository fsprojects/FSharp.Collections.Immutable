#if INTERACTIVE
namespace global
#else
namespace FSharp.Collections.Immutable

open FSharp.Collections.Immutable

#endif

// The FlatList name comes from a similar construct seen in the official F# source code
type FlatList<'T> = System.Collections.Immutable.ImmutableArray<'T>

// based on the F# Array module source
[<RequireQualifiedAccess; CompiledName((nameof System.Collections.Immutable.ImmutableArray) + "Module")>]
module FlatList =

    type internal FlatListFactory = System.Collections.Immutable.ImmutableArray

    let inline internal checkNotDefault argName (list : FlatList<'T>) =
        if list.IsDefault then invalidArg argName "Uninstantiated ImmutableArray/FlatList"
    let inline internal check (list : FlatList<'T>) = checkNotDefault (nameof list) list
    let inline internal checkEmpty (list : FlatList<_>) = check list; if list.Length = 0 then invalidArg (nameof list) "Source is empty" else ()
    let inline internal raiseOrReturn list = check list; list

    ////////// Creating //////////

    [<CompiledName("Empty")>]
    let inline empty<'T> : FlatList<_> = FlatListFactory.Create<'T>()
    [<CompiledName("Singleton")>]
    let inline singleton<'T> (item : 'T) : FlatList<'T> = FlatListFactory.Create<'T> (item)
    [<CompiledName("Copy")>]
    let copy (list:FlatList<_>) = FlatListFactory.CreateRange list

    [<CompiledName("OfSeq")>]
    let inline ofSeq source = FlatListFactory.CreateRange source
    [<CompiledName("OfArray")>]
    let inline ofArray (source : _ array) = FlatListFactory.CreateRange source
    [<CompiledName("OfList")>]
    let inline ofList (source: _ list) = FlatListFactory.CreateRange source

    [<CompiledName("ToSeq")>]
    let inline toSeq (flatList: FlatList<_>) = check flatList; flatList :> seq<_>
    [<CompiledName("ToArray")>]
    let inline toArray (list : FlatList<_>) = check list; Seq.toArray list
    [<CompiledName("ToList")>]
    let inline toList list = check list; Seq.toList list

    ////////// Building //////////

    [<CompiledName("MoveFromBuilder")>]
    let moveFromBuilder (builder : FlatList<_>.Builder) : FlatList<_> =
        checkNotNull (nameof builder) builder
        builder.MoveToImmutable()
    [<CompiledName("OfBuilder")>]
    let ofBuilder (builder : FlatList<_>.Builder) : FlatList<_> =
        checkNotNull (nameof builder) builder
        builder.ToImmutable()

    [<CompiledName("Builder")>]
    let inline builder () : FlatList<'T>.Builder = FlatListFactory.CreateBuilder()
    [<CompiledName("BuilderWith")>]
    let inline builderWith capacity : FlatList<'T>.Builder = FlatListFactory.CreateBuilder(capacity)

    [<CompiledName("ToBuilder")>]
    let toBuilder list: FlatList<_>.Builder = check list; list.ToBuilder()

    module Builder =
        let inline private check (builder: FlatList<'T>.Builder) = checkNotNull (nameof builder) builder

        [<CompiledName("Add")>]
        let add item builder = check builder; builder.Add(item)

    let inline internal indexNotFound() = raise <| System.Collections.Generic.KeyNotFoundException()

    [<CompiledName("IsEmpty")>]
    let isEmpty (list: FlatList<_>) = list.IsEmpty
    [<CompiledName("IsDefault")>]
    let isDefault (list: FlatList<_>) = list.IsDefault
    [<CompiledName("IsDefaultOrEmpty")>]
    let isDefaultOrEmpty (list: FlatList<_>) = list.IsDefaultOrEmpty

    ////////// IReadOnly* //////////

    [<CompiledName("Length")>]
    let length list = check list; list.Length

    [<CompiledName("Item")>]
    let item index list = check list; list.[index]

    [<CompiledName("Append")>]
    let append list1 list2 : FlatList<'T> =
        checkNotDefault (nameof list1) list1
        checkNotDefault (nameof list2) list2
        list1.AddRange(list2 : FlatList<_>)

    /// Searches for the specified object and returns the zero-based index of the first occurrence within the range
    /// of elements in the list that starts at the specified index and
    /// contains the specified number of elements.
    [<CompiledName("IndexRangeWith")>]
    let indexRangeWith comparer index count item list =
        check list
        list.IndexOf(item, index, count, comparer)
    [<CompiledName("IndexRange")>]
    let indexRange index count item list =
        indexRangeWith HashIdentity.Structural index count item list
    [<CompiledName("IndexFromWith")>]
    let indexFromWith comparer index item list =
        indexRangeWith comparer index (length list - index) item
    [<CompiledName("IndexFrom")>]
    let indexFrom index item list =
        indexFromWith HashIdentity.Structural index item list
    [<CompiledName("IndexWith")>]
    let indexWith comparer item list =
        indexFromWith comparer 0 item list
    [<CompiledName("Index")>]
    let index item list = indexWith HashIdentity.Structural item list

    /// Searches for the specified object and returns the zero-based index of the last occurrence within the
    /// range of elements in the list that contains the specified number
    /// of elements and ends at the specified index.
    [<CompiledName("LastIndexRangeWith")>]
    let lastIndexRangeWith comparer index count item list =
        check list
        list.LastIndexOf(item, index, count, comparer)
    [<CompiledName("LastIndexRange")>]
    let lastIndexRange index count item list =
        lastIndexRangeWith HashIdentity.Structural index count item list
    [<CompiledName("LastIndexFromWith")>]
    let lastIndexFromWith comparer index item list =
        lastIndexRangeWith comparer index (index + 1) item list
    [<CompiledName("LastIndexFrom")>]
    let lastIndexFrom index item list =
        lastIndexFromWith HashIdentity.Structural index item list
    [<CompiledName("LastIndexWith")>]
    let lastIndexWith comparer item list =
        lastIndexFromWith comparer (length list - 1) item list
    [<CompiledName("LastIndex")>]
    let lastIndex item list = lastIndexWith HashIdentity.Structural item list

    /// Removes the specified objects from the list with the given comparer.
    [<CompiledName("RemoveAllWith")>]
    let removeAllWith (comparer: System.Collections.Generic.IEqualityComparer<_>) items list: FlatList<_> =
        check list
        list.RemoveRange(items, comparer)

    /// Removes the specified objects from the list.
    [<CompiledName("RemoveAll")>]
    let removeAll items list = removeAllWith HashIdentity.Structural items list

    /// Removes all the elements that do not match the conditions defined by the specified predicate.
    [<CompiledName("Filter")>]
    let filter predicate list: FlatList<_> =
        check list
        System.Predicate(not << predicate)
        |> list.RemoveAll

    /// Removes all the elements that do not match the conditions defined by the specified predicate.
    [<CompiledName("Where")>]
    let where predicate list = filter predicate list

    /// Removes a range of elements from the list.
    [<CompiledName("RemoveRange")>]
    let removeRange index (count: int) list: FlatList<_> = check list; list.RemoveRange(index, count)

    [<CompiledName("Blit")>]
    let blit source sourceIndex (destination: 'T[]) destinationIndex count =
        checkNotDefault (nameof source) source
        try source.CopyTo(sourceIndex, destination, destinationIndex, count)
        with exn -> raise exn // throw same exception with the correct stack trace. Update exception code

    [<CompiledName("SortRangeWithComparer")>]
    let sortRangeWithComparer comparer index count list =
        check list
        list.Sort(index, count, comparer)
    [<CompiledName("SortRangeWith")>]
    let sortRangeWith comparer index count list =
        sortRangeWithComparer (ComparisonIdentity.FromFunction comparer) index count list
    [<CompiledName("SortRange")>]
    let sortRange index count list = sortRangeWithComparer ComparisonIdentity.Structural index count list
    [<CompiledName("SortWithComparer")>]
    let sortWithComparer (comparer : System.Collections.Generic.IComparer<_>) list = check list; list.Sort(comparer)
    [<CompiledName("SortWith")>]
    let sortWith comparer list = sortWithComparer (ComparisonIdentity.FromFunction comparer) list
    [<CompiledName("Sort")>]
    let sort list = check list; list.Sort()

    ////////// Loop-based //////////

    let inline private builderWithLengthOf list = builderWith <| length list

    [<CompiledName("Init")>]
    let init count initializer =
        if count < 0 then invalidArg (nameof count) ErrorStrings.InputMustBeNonNegative
        let builder = builderWith count
        for i = 0 to count - 1 do
            builder.Add <| initializer i
        moveFromBuilder builder

    [<CompiledName("InitWithValue")>]
    let initWithValue count value =
        if count < 0 then invalidArg (nameof count) ErrorStrings.InputMustBeNonNegative
        let builder = builderWith count
        for i = 0 to count - 1 do
            builder.Add value
        ofBuilder builder

    let rec private concatAddLengths (arrs: FlatList<FlatList<_>>) i acc =
        if i >= length arrs then acc
        else concatAddLengths arrs (i+1) (acc + arrs.[i].Length)

    [<CompiledName("Concat")>]
    let concat (seqs:FlatList<FlatList<_>>) =
        let builder = builderWith <| concatAddLengths seqs 0 0
        for seq in seqs do
            builder.AddRange seq
        ofBuilder builder

    [<CompiledName("Map")>]
    let map mapping = raiseOrReturn >> Seq.map mapping >> ofSeq

    [<CompiledName("CountBy")>]
    let countBy projection = raiseOrReturn >> Seq.countBy projection >> ofSeq

    [<CompiledName("Indexed")>]
    let indexed list = list |> raiseOrReturn |> Seq.indexed |> ofSeq

    [<CompiledName("Iterate")>]
    let iter action = raiseOrReturn >> Seq.iter action

    [<CompiledName("Iterate2")>]
    let iter2 action list1 list2 =
        checkNotDefault (nameof list1) list1
        checkNotDefault (nameof list2) list2
        Seq.iter2 action list1 list2

    [<CompiledName("Distinct")>]
    let distinct (list: FlatList<'T>) = list |> Seq.distinct |> ofSeq

    [<CompiledName("DistinctBy")>]
    let distinctBy projection = raiseOrReturn >> Seq.distinctBy projection >> ofSeq

    [<CompiledName("Map2")>]
    let map2 mapping list1 list2 =
        checkNotDefault (nameof list1) list1
        checkNotDefault (nameof list2) list2
        Seq.map2 mapping list1 list2 |> ofSeq

    [<CompiledName("Map3")>]
    let map3 mapping list1 list2 list3 =
        checkNotDefault (nameof list1) list1
        checkNotDefault (nameof list2) list2
        checkNotDefault (nameof list3) list3
        Seq.map3 mapping list1 list2 list3 |> ofSeq


    [<CompiledName("MapIndexed2")>]
    let mapi2 mapping list1 list2 =
        checkNotDefault (nameof list1) list1
        checkNotDefault (nameof list2) list2
        Seq.mapi2 mapping list1 list2 |> ofSeq

    [<CompiledName("IterateIndexed")>]
    let iteri action = raiseOrReturn >> Seq.iteri action

    [<CompiledName("IterateIndexed2")>]
    let iteri2 action list1 list2 =
        checkNotDefault (nameof list1) list1
        checkNotDefault (nameof list2) list2
        Seq.iteri2 action list1 list2

    [<CompiledName("MapIndexed")>]
    let mapi mapping = raiseOrReturn >> Seq.mapi mapping >> ofSeq

    [<CompiledName("Exists")>]
    let exists predicate = raiseOrReturn >> Seq.exists predicate

    [<CompiledName("Contains")>]
    let contains e = raiseOrReturn >> Seq.contains e

    [<CompiledName("Exists2")>]
    let exists2 predicate list1 list2 =
        checkNotDefault (nameof list1) list1
        checkNotDefault (nameof list2) list2
        Seq.exists2 predicate list1 list2

    [<CompiledName("ForAll")>]
    let forall predicate = raiseOrReturn >> Seq.forall predicate

    [<CompiledName("ForAll2")>]
    let forall2 predicate list1 list2 =
        checkNotDefault (nameof list1) list1
        checkNotDefault (nameof list2) list2
        Seq.forall2 predicate list1 list2

    [<CompiledName("GroupBy")>]
    let groupBy projection = raiseOrReturn >> Seq.groupBy projection >> Seq.map (fun (k, i) -> k, ofSeq i) >> ofSeq

    [<CompiledName("Pick")>]
    let pick chooser = raiseOrReturn >> Seq.pick chooser

    [<CompiledName("TryPick")>]
    let tryPick chooser = raiseOrReturn >> Seq.tryPick chooser

    [<CompiledName("Choose")>]
    let choose chooser = raiseOrReturn >> Seq.choose chooser >> ofSeq

    [<CompiledName("Partition")>]
    let partition predicate list =
        check list
        let res1 = builderWith list.Length
        let res2 = builderWith list.Length
        for i = 0 to list.Length - 1 do
            let x = list.[i]
            if predicate x then res1.Add(x) else res2.Add(x)
        ofBuilder res1, ofBuilder res2

    let rec private tryFindWithCustomStride (list:FlatList<_>) index predicate indexPredicate indexTransform =
        if indexPredicate index then
            if predicate list.[index] then
                Some (index, list.[index])
            else tryFindWithCustomStride list (indexTransform index) predicate indexPredicate indexTransform
        else None

    [<CompiledName("TryFindItem")>]
    let tryFindItem predicate direction list =
        check list
        let startIndex = if direction then 0 else length list - 1
        let indexPredicate = if direction then ((>) (length list)) else ((<=) 0)
        let transform = if direction then ((+) 1) else ((+) -1) // because section is not available
        tryFindWithCustomStride list startIndex predicate indexPredicate transform

    [<CompiledName("Find")>]
    let find predicate = raiseOrReturn >> Seq.find predicate
    [<CompiledName("TryFind")>]
    let tryFind predicate = raiseOrReturn >> Seq.tryFind predicate
    [<CompiledName("FindBack")>]
    let findBack predicate = raiseOrReturn >> Seq.findBack predicate
    [<CompiledName("TryFindBack")>]
    let tryFindBack predicate = raiseOrReturn >> Seq.tryFindBack predicate
    [<CompiledName("FindIndex")>]
    let findIndex predicate = raiseOrReturn >> Seq.findIndex predicate
    [<CompiledName("FindIndexBack")>]
    let findIndexBack predicate = raiseOrReturn >> Seq.findIndexBack predicate
    [<CompiledName("TryFindIndex")>]
    let tryFindIndex predicate = raiseOrReturn >> Seq.tryFindIndex predicate
    [<CompiledName("TryFindIndexBack")>]
    let tryFindIndexBack predicate = raiseOrReturn >> Seq.tryFindIndexBack predicate

    [<CompiledName("Fold")>]
    let fold folder (state: 'state) = raiseOrReturn >> Seq.fold folder state

    [<CompiledName("Scan")>]
    let scan folder (state: 'state) = raiseOrReturn >> Seq.scan folder state >> ofSeq

    [<CompiledName("Fold2")>]
    let fold2 folder (state: 'state) (left:FlatList<'a>) (right:FlatList<'b>) =
        check left; check right
        Seq.fold2 folder state left right

    [<CompiledName("FoldBack2")>]
    let foldBack2 folder (left:FlatList<'a>) (right:FlatList<'b>) (state:'state) =
        check left; check right
        Seq.foldBack2 folder left right state

    [<CompiledName("FoldBack")>]
    let foldBack folder (list:FlatList<'a>) (state: 'state) =
        check list
        Seq.foldBack folder list state

    [<CompiledName("ScanBack")>]
    let scanBack folder (list:FlatList<'a>) (state:'state) =
        check list
        Seq.scanBack folder list state |> ofSeq

    [<CompiledName("Unfold")>]
    let unfold (generator: 'state -> ('a * 'state) option) state =
        Seq.unfold generator state |> ofSeq

    [<CompiledName("Reduce")>]
    let reduce reduction = raiseOrReturn >> Seq.reduce reduction

    [<CompiledName("ReduceBack")>]
    let reduceBack reduction = raiseOrReturn >> Seq.reduceBack reduction

    [<CompiledName("MapFold")>]
    let mapFold mapping (state:'State) (list:FlatList<'T>) =
        check list
        let (items, s) = Seq.mapFold mapping state list
        ofSeq items, s

    [<CompiledName("MapFoldBack")>]
    let mapFoldBack mapping (list:FlatList<'T>) (state:'State) =
        check list
        let (i, s) = Seq.mapFoldBack mapping list state
        ofSeq i, s

    [<CompiledName("Zip")>]
    let zip (left:FlatList<_>) (right:FlatList<_>) =
        check left; check right
        Seq.zip left right |> ofSeq

    [<CompiledName("Zip3")>]
    let zip3 (left:FlatList<_>) (middle:FlatList<_>) (right:FlatList<_>) =
        check left; check middle; check right
        Seq.zip3 left middle right |> ofSeq

    [<CompiledName("Unzip")>]
    let unzip list =
        let left = builderWithLengthOf list
        let right = builderWithLengthOf list
        for item in list do
            left.Add <| fst item
            right.Add <| snd item
        ofBuilder left, ofBuilder right

    [<CompiledName("Unzip3")>]
    let unzip3 list =
        let left = builderWithLengthOf list
        let right = builderWithLengthOf list
        let middle = builderWithLengthOf list
        for item in list do
            left.Add <| fst3 item
            middle.Add <| snd3 item
            right.Add <| thd3 item
        ofBuilder left, ofBuilder middle, ofBuilder right

    [<CompiledName("Windowed")>]
    let windowed windowSize = raiseOrReturn >> Seq.windowed windowSize >> Seq.map ofSeq >> ofSeq

    [<CompiledName("Fill")>]
    let fill target targetIndex count value =
        mapi (fun i a -> if targetIndex <= i && i < targetIndex + count then value else a) target

    ////////// Based on other operations //////////

    [<CompiledName("Take")>]
    let take count list = removeRange count (length list - count) list

    let inline private lengthWhile predicate list =
        check list
        let mutable count = 0
        while count < list.Length && predicate list.[count] do
            count <- count + 1
        count

    [<CompiledName("TakeWhile")>]
    let takeWhile predicate list = take (lengthWhile predicate list) list

    [<CompiledName("Skip")>]
    let skip index list = removeRange 0 index list

    [<CompiledName("SkipWhile")>]
    let skipWhile predicate list = skip (lengthWhile predicate list) list

    [<CompiledName("Sub")>]
    let sub start stop list = skip start list |> take (stop - start - 1)

    [<CompiledName("Truncate")>]
    let truncate count list = if count < length list then take count list else list

    [<CompiledName("SplitAt")>]
    let splitAt index list = take index list, skip index list

    [<CompiledName("Head")>]
    let head list = item 0 list

    [<CompiledName("TryItem")>]
    let tryItem index list =
        if index >= length list || index < 0 then None
        else Some(list.[index])

    [<CompiledName("TryHead")>]
    let tryHead list = tryItem 0 list

    [<CompiledName("Last")>]
    let last (list : FlatList<_>) = list.[length list - 1]

    [<CompiledName("TryLast")>]
    let tryLast list = tryItem (length list - 1) list

    [<CompiledName("Tail")>]
    let tail list = skip 1 list

    [<CompiledName("TryTail")>]
    let tryTail list = if isEmpty list then None else Some <| tail list

    [<CompiledName("Create")>]
    let create = initWithValue

    [<CompiledName("Replicate")>]
    let replicate item = item |> flip initWithValue

    [<CompiledName("Collect")>]
    let collect mapping list = concat <| map mapping list

    [<CompiledName("Build")>]
    let inline build f =
        let builder = builder()
        f builder
        moveFromBuilder builder

    [<CompiledName("Update")>]
    let inline update f list =
        let builder = toBuilder list
        f builder
        moveFromBuilder builder

    [<CompiledName("Sum")>]
    let inline sum ( list:FlatList< ^T > when ^T : (static member (+) : ^T * ^T -> ^T) and ^T : (static member Zero : ^T) ) =
        list |> raiseOrReturn |> reduce (+)

    [<CompiledName("SumBy")>]
    let inline sumBy projection ( list:FlatList< ^T > when ^T : (static member (+) : ^T * ^T -> ^T) and ^T : (static member Zero : ^T) ) =
        list |> raiseOrReturn |> map projection |> reduce (+)

    [<CompiledName("Average")>]
    let inline average ( list:FlatList< ^T > when ^T : (static member (+) : ^T * ^T -> ^T) and ^T : (static member DivideByInt : ^T*int -> ^T) and ^T : (static member Zero : ^T) ) =
        list |> raiseOrReturn |> applyOverFuncs LanguagePrimitives.DivideByInt sum length

    [<CompiledName("AverageBy")>]
    let inline averageBy projection ( list:FlatList< ^T > when ^T : (static member (+) : ^T * ^T -> ^T) and ^T : (static member DivideByInt : ^T*int -> ^T) and ^T : (static member Zero : ^T) ) =
        list |> raiseOrReturn |> applyOverFuncs LanguagePrimitives.DivideByInt ((map projection) >> sum) length

    let private minMaxReduction projection comparison a b =
        let pa = projection a
        let pb = projection b
        if comparison pa pb then a else b

    [<CompiledName("MaxBy")>]
    let maxBy projection (list:FlatList<'a>) = list |> raiseOrReturn |> reduce (minMaxReduction projection (>))

    [<CompiledName("MinBy")>]
    let minBy projection (list:FlatList<'a> when 'a : comparison) = list |> raiseOrReturn |> reduce (minMaxReduction projection (<))

    [<CompiledName("Max")>]
    let max (list:FlatList<'a> when 'a : comparison) = list |> raiseOrReturn |> reduce max
    [<CompiledName("Min")>]
    let min (list:FlatList<'a> when 'a : comparison) = list |> raiseOrReturn |> reduce min

    [<CompiledName("SortBy")>]
    let sortBy projection = sortWith (applyOverArgs LanguagePrimitives.GenericComparison projection)
    [<CompiledName("SortDescending")>]
    let sortDescending (list:FlatList<'a>) = sortWith (flip LanguagePrimitives.GenericComparison) list
    [<CompiledName("SortByDescending")>]
    let sortByDescending projection = sortWith (flip (applyOverArgs LanguagePrimitives.GenericComparison projection))

    [<CompiledName("CompareWith")>]
    let compareWith comparer (left:FlatList<'a>) (right:FlatList<'b>) = zip left right |> Seq.skipWhile ((uncurry comparer) >> ((=) 0)) |> Seq.head |> (uncurry comparer)

    [<CompiledName("TryExactlyOne")>]
    let tryExactlyOne (list:FlatList<_>) = Seq.tryExactlyOne list
    [<CompiledName("ExactlyOne")>]
    let exactlyOne (list:FlatList<_>) = Seq.exactlyOne list

    [<CompiledName("Reverse")>]
    let rev (list:FlatList<_>) = list |> raiseOrReturn |> Seq.rev |> ofSeq
    [<CompiledName("Transpose")>]
    let transpose (list:FlatList<_>) = list |> raiseOrReturn |> Seq.transpose |> Seq.map ofSeq |> ofSeq
    [<CompiledName("Permute")>]
    let permute indexMap (list:FlatList<_>) = list |> raiseOrReturn |> Seq.permute indexMap |> ofSeq
    [<CompiledName("Pairwise")>]
    let pairwise (list:FlatList<_>) = list |> raiseOrReturn |> Seq.pairwise |> ofSeq
    [<CompiledName("Except")>]
    let except itemsToExclude (list:FlatList<_>) = list |> raiseOrReturn |> Seq.except itemsToExclude |> ofSeq
    [<CompiledName("SplitInto")>]
    let splitInto count (list:FlatList<_>) = list |> raiseOrReturn |> Seq.splitInto count |> Seq.map ofSeq |> ofSeq
    [<CompiledName("ChunkBySize")>]
    let chunkBySize chunkSize (list:FlatList<_>) = list |> raiseOrReturn |> Seq.chunkBySize chunkSize |> Seq.map ofSeq |> ofSeq
    [<CompiledName("AllPairs")>]
    let allPairs (left:FlatList<'a>) (right:FlatList<'b>) = Seq.allPairs (raiseOrReturn left) (raiseOrReturn right) |> ofSeq

    //////////

module ImmutableArray = FlatList
