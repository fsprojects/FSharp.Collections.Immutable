// This file contains F# bindings to ImmutableArray from System.Collections.Immutable.
// It provides a flat list implementation that is optimized for performance and memory usage.
// The FlatList type is a wrapper around ImmutableArray, providing a more convenient API for working with immutable lists.
// FlatList code is designed to perform operations without allocating new arrays unnecessarily, making it suitable for high-performance applications.
#if INTERACTIVE
namespace global
#else
namespace FSharp.Collections.Immutable
#endif

open System
open System.Buffers
open System.Collections.Generic
open System.Collections.Immutable
open System.Linq

// The FlatList name comes from a similar construct seen in the official F# source code
type FlatList<'T> = System.Collections.Immutable.ImmutableArray<'T>

// based on the F# Array module source
[<RequireQualifiedAccess;
  CompiledName((nameof System.Collections.Immutable.ImmutableArray)
               + "Module")>]
module FlatList =

    type internal FlatListFactory = System.Collections.Immutable.ImmutableArray

    let inline internal checkNotDefault argName (list : FlatList<'T>) =
        if list.IsDefault then
            invalidArg argName "Uninstantiated ImmutableArray/FlatList"

    let inline internal indexNotFound () =
        raise
        <| System.Collections.Generic.KeyNotFoundException ("An item with the specified key was not found.")

    let inline internal sequenceNotFound () =
        raise
        <| System.InvalidOperationException ("Sequence contains no matching element.")

    ////////// Creating //////////

    [<CompiledName "BuilderWith">]
    let inline builderWith capacity : FlatList<'T>.Builder = FlatListFactory.CreateBuilder (capacity)

    [<CompiledName "MoveFromBuilder">]
    let moveFromBuilder (builder : FlatList<_>.Builder) : FlatList<_> =
        checkNotNull (nameof builder) builder // Keep check for null builder, not default FlatList
        builder.MoveToImmutable ()

    [<CompiledName "Empty">]
    let inline empty<'T> : FlatList<'T> = FlatListFactory.Create<'T> ()

    [<CompiledName "OfArray">]
    let inline ofArray (source : _ array) = FlatListFactory.CreateRange source

    [<CompiledName "OfSeq">]
    let inline ofSeq source = FlatListFactory.CreateRange source

    [<CompiledName "OfList">]
    let inline ofList (source : 'T list) = FlatListFactory.CreateRange source

    [<CompiledName "Singleton">]
    let inline singleton<'T> (item : 'T) : FlatList<'T> = FlatListFactory.Create<'T> (item)

    [<CompiledName "OfOption">]
    let ofOption (option : 'T option) : FlatList<'T> =
        match option with
        | Some x -> singleton x
        | None -> empty

    [<CompiledName "OfValueOption">]
    let ofValueOption (voption : 'T voption) : FlatList<'T> =
        match voption with
        | ValueSome x -> singleton x
        | ValueNone -> empty

    [<CompiledName "Init">]
    let init count (initializer : int -> 'T) =
        if count < 0 then
            invalidArg (nameof count) ErrorStrings.InputMustBeNonNegative

        if count = 0 then
            empty
        else
            // Create a builder with exact capacity needed
            let builder = FlatListFactory.CreateBuilder<'T> count
            // Resize the internal array to ensure all indices are valid
            builder.Count <- count

            // Use Parallel.For to initialize elements in parallel
            System.Threading.Tasks.Parallel.For (0, count, fun i -> builder.[i] <- initializer i)
            |> ignore

            builder.MoveToImmutable ()

    [<CompiledName "Create">]
    let create count (value : 'T) = init count (fun _ -> value)

    [<CompiledName "Replicate">]
    let replicate count initial = create count initial

    [<CompiledName "ZeroCreate">]
    let zeroCreate<'T> (count : int) : FlatList<'T> =
        if count < 0 then
            invalidArg (nameof count) ErrorStrings.InputMustBeNonNegative
        if count = 0 then
            empty
        else
            let arr = Array.zeroCreate<'T> count
            FlatListFactory.CreateRange (arr)

    [<CompiledName "ToSeq">]
    let inline toSeq (flatList : FlatList<_>) = flatList :> seq<_>

    [<CompiledName "ToArray">]
    let inline toArray (list : FlatList<_>) = list.ToArray ()

    [<CompiledName "ToList">]
    let toList (list : FlatList<'T>) : 'T list =
        if list.IsDefaultOrEmpty then
            []
        else
            let len = list.Length
            let mutable result = []
            for i = len - 1 downto 0 do
                result <- list.[i] :: result
            result

    [<CompiledName "ToOption">]
    let toOption (list : FlatList<'T>) : 'T option = if list.Length = 1 then Some list.[0] else None

    [<CompiledName "ToValueOption">]
    let toValueOption (list : FlatList<'T>) : 'T voption = if list.Length = 1 then ValueSome list.[0] else ValueNone

    [<CompiledName "Copy">]
    let copy (list : FlatList<'T>) : FlatList<'T> = list.ToImmutableArray ()

    ////////// Building //////////

    [<CompiledName "OfBuilder">]
    let ofBuilder (builder : FlatList<_>.Builder) : FlatList<_> =
        checkNotNull (nameof builder) builder // Keep check for null builder
        builder.ToImmutable ()

    [<CompiledName "Builder">]
    let inline builder () : FlatList<'T>.Builder = FlatListFactory.CreateBuilder ()

    [<CompiledName "ToBuilder">]
    let toBuilder (list : FlatList<'T>) : FlatList<'T>.Builder = list.ToBuilder ()

    module Builder =

        let inline private check (builder : FlatList<'T>.Builder) = checkNotNull (nameof builder) builder

        [<CompiledName "Add">]
        let add (item : 'T) (builder : FlatList<'T>.Builder) : FlatList<'T>.Builder =
            check builder
            builder.Add item
            builder

    [<CompiledName "IsEmpty">]
    let isEmpty (list : FlatList<_>) = list.IsEmpty

    [<CompiledName "IsDefault">]
    let isDefault (list : FlatList<_>) = list.IsDefault

    [<CompiledName "IsDefaultOrEmpty">]
    let isDefaultOrEmpty (list : FlatList<_>) = list.IsDefaultOrEmpty

    ////////// IReadOnly* //////////

    [<CompiledName "Length">]
    let length (list : FlatList<'T>) = list.Length

    [<CompiledName "Item">]
    let item index (list : FlatList<'T>) = list.[index]

    [<CompiledName "Append">]
    let append (list1 : FlatList<'T>) (list2 : FlatList<'T>) : FlatList<'T> =
        list1.AddRange (list2 :> System.Collections.Generic.IEnumerable<'T>)

    [<CompiledName "IndexRangeWith">]
    let indexRangeWith comparer index count item (list : FlatList<'T>) = list.IndexOf (item, index, count, comparer)

    [<CompiledName "IndexRange">]
    let indexRange index count item list = indexRangeWith HashIdentity.Structural index count item list

    [<CompiledName "IndexFromWith">]
    let indexFromWith comparer index item list = indexRangeWith comparer index (length list - index) item list

    [<CompiledName "IndexFrom">]
    let indexFrom index item list = indexFromWith HashIdentity.Structural index item list

    [<CompiledName "IndexWith">]
    let indexWith comparer item list = indexFromWith comparer 0 item list

    [<CompiledName "Index">]
    let index item (list : FlatList<'T>) =
        let idx = list.IndexOf (item)
        if idx = -1 then indexNotFound () else idx

    [<CompiledName "LastIndexRangeWith">]
    let lastIndexRangeWith comparer index count item (list : FlatList<'T>) = list.LastIndexOf (item, index, count, comparer)

    [<CompiledName "LastIndexRange">]
    let lastIndexRange index count item list = lastIndexRangeWith HashIdentity.Structural index count item list

    [<CompiledName "LastIndexFromWith">]
    let lastIndexFromWith comparer index item list = lastIndexRangeWith comparer index (index + 1) item list

    [<CompiledName "LastIndexFrom">]
    let lastIndexFrom index item list = lastIndexFromWith HashIdentity.Structural index item list

    [<CompiledName "LastIndexWith">]
    let lastIndexWith comparer item list = lastIndexFromWith comparer (length list - 1) item list

    [<CompiledName "LastIndex">]
    let lastIndex item (list : FlatList<'T>) =
        let idx = list.LastIndexOf (item)
        if idx = -1 then indexNotFound () else idx

    [<CompiledName "RemoveAllWith">]
    let removeAllWith comparer (items : 'T seq) (list : FlatList<'T>) : FlatList<'T> =
        let itemsToRemove = HashSet (items, comparer)
        list.RemoveAll (System.Predicate (fun x -> itemsToRemove.Contains x))

    [<CompiledName "RemoveAll">]
    let removeAll items (list : FlatList<'T>) = removeAllWith HashIdentity.Structural items list

    [<CompiledName "Filter">]
    let filter (predicate : 'T -> bool) (list : FlatList<'T>) : FlatList<'T> =
        list.RemoveAll (System.Predicate (not << predicate))

    [<CompiledName "Where">]
    let where (predicate : 'T -> bool) (list : FlatList<'T>) : FlatList<'T> = filter predicate list

    [<CompiledName "RemoveRange">]
    let removeRange index (count : int) (list : FlatList<'T>) : FlatList<'T> = list.RemoveRange (index, count)

    [<CompiledName "Blit">]
    let blit (source : FlatList<'T>) sourceIndex (destination : 'T[]) destinationIndex count =
        source.CopyTo (sourceIndex, destination, destinationIndex, count)

    [<CompiledName "SortRangeWithComparer">]
    let sortRangeWithComparer comparer index count (list : FlatList<'T>) = list.Sort (index, count, comparer)

    [<CompiledName "SortRangeWith">]
    let sortRangeWith comparer index count list =
        sortRangeWithComparer (ComparisonIdentity.FromFunction comparer) index count list

    [<CompiledName "SortRange">]
    let sortRange index count list = sortRangeWithComparer ComparisonIdentity.Structural index count list

    [<CompiledName "SortWithComparer">]
    let sortWithComparer (comparer : System.Collections.Generic.IComparer<'T>) (list : FlatList<'T>) = list.Sort (comparer)

    [<CompiledName "SortWith">]
    let sortWith comparer list = sortWithComparer (ComparisonIdentity.FromFunction comparer) list

    [<CompiledName "Sort">]
    let sort (list : FlatList<'T>) = list.Sort ()

    [<CompiledName "Rev">]
    let rev (list : FlatList<'T>) : FlatList<'T> =
        if list.IsDefaultOrEmpty then
            list
        else
            let len = list.Length
            let builder = FlatListFactory.CreateBuilder<'T> len
            for i = len - 1 downto 0 do
                builder.Add list.[i]
            builder.MoveToImmutable ()

    [<CompiledName "SortDescending">]
    let inline sortDescending (list : FlatList<'T>) : FlatList<'T> when 'T : comparison = sortWith (fun x y -> compare y x) list

    [<CompiledName "SortByDescending">]
    let inline sortByDescending (projection : 'T -> 'Key) (list : FlatList<'T>) : FlatList<'T> when 'Key : comparison =
        sortWith (fun x y -> compare (projection y) (projection x)) list

    [<CompiledName "SortBy">]
    let sortBy (projection : 'T -> 'Key) (list : FlatList<'T>) : FlatList<'T> when 'Key : comparison =
        if list.IsDefaultOrEmpty then
            list
        else
            let items = list.ToArray () // Work with a mutable array for sorting
            System.Array.Sort (items, fun x y -> compare (projection x) (projection y))
            FlatListFactory.CreateRange (items)

    ////////// Loop-based (now LINQ-based where applicable) //////////

    [<CompiledName "Concat">]
    let concat (arrs : FlatList<FlatList<'T>>) =
        let totalLength = Seq.sumBy (fun (innerList : FlatList<'T>) -> innerList.Length) arrs
        let builder = FlatListFactory.CreateBuilder<'T> (totalLength)
        for i = 0 to arrs.Length - 1 do
            let arr = arrs.[i]
            for j = 0 to arr.Length - 1 do
                builder.Add (arr.[j])
        builder.MoveToImmutable ()

    [<CompiledName "Map">]
    let inline map (mapping : 'T -> 'U) (list : FlatList<'T>) : FlatList<'U> = list.Select(mapping).ToImmutableArray ()

    [<CompiledName "Mapi">]
    let mapi (mapping : int -> 'T -> 'U) (list : FlatList<'T>) : FlatList<'U> =
        list.Select(fun x i -> mapping i x).ToImmutableArray ()

    [<CompiledName "Mapi2">]
    let mapi2 (mapping : int -> 'T1 -> 'T2 -> 'U) (list1 : FlatList<'T1>) (list2 : FlatList<'T2>) : FlatList<'U> =
        let len1 = list1.Length
        if len1 <> list2.Length then
            invalidArg (nameof list2) ErrorStrings.ListsHaveDifferentLengths

        Enumerable.Range(0, len1).Select(fun i -> mapping i list1.[i] list2.[i]).ToImmutableArray ()

    [<CompiledName "Mapi3">]
    let mapi3
        (mapping : int -> 'T1 -> 'T2 -> 'T3 -> 'U)
        (list1 : FlatList<'T1>)
        (list2 : FlatList<'T2>)
        (list3 : FlatList<'T3>)
        : FlatList<'U> =
        let len1 = list1.Length
        if len1 <> list2.Length then
            invalidArg (nameof list2) ErrorStrings.ListsHaveDifferentLengths
        if len1 <> list3.Length then
            invalidArg (nameof list3) ErrorStrings.ListsHaveDifferentLengths

        Enumerable.Range(0, len1).Select(fun i -> mapping i list1.[i] list2.[i] list3.[i]).ToImmutableArray ()

    [<CompiledName "Map2">]
    let map2 (mapping : 'T1 -> 'T2 -> 'U) (list1 : FlatList<'T1>) (list2 : FlatList<'T2>) : FlatList<'U> =
        let len1 = list1.Length
        if len1 <> list2.Length then
            invalidArg (nameof list2) ErrorStrings.ListsHaveDifferentLengths

        Enumerable.Range(0, len1).Select(fun i -> mapping list1.[i] list2.[i]).ToImmutableArray ()

    [<CompiledName "Map3">]
    let map3
        (mapping : 'T1 -> 'T2 -> 'T3 -> 'U)
        (list1 : FlatList<'T1>)
        (list2 : FlatList<'T2>)
        (list3 : FlatList<'T3>)
        : FlatList<'U> =
        let len1 = list1.Length
        if len1 <> list2.Length then
            invalidArg (nameof list2) ErrorStrings.ListsHaveDifferentLengths
        if len1 <> list3.Length then
            invalidArg (nameof list3) ErrorStrings.ListsHaveDifferentLengths

        Enumerable.Range(0, len1).Select(fun i -> mapping list1.[i] list2.[i] list3.[i]).ToImmutableArray ()

    [<CompiledName "MapFold">]
    let mapFold<'T, 'State, 'Result>
        (mapping : 'State -> 'T -> 'Result * 'State)
        (state : 'State)
        (list : FlatList<'T>)
        : FlatList<'Result> * 'State =
        checkNotDefault (nameof list) list
        let len = list.Length

        if len = 0 then
            empty, state
        else
            let resultBuilder = FlatListFactory.CreateBuilder<'Result> (len)
            resultBuilder.Count <- len

            let mutable currentState = state

            for i = 0 to len - 1 do
                let item = list.[i]
                let result, newState = mapping currentState item
                resultBuilder.[i] <- result
                currentState <- newState

            resultBuilder.MoveToImmutable (), currentState

    [<CompiledName "MapFoldBack">]
    let mapFoldBack<'T, 'State, 'Result>
        (mapping : 'T -> 'State -> 'Result * 'State)
        (list : FlatList<'T>)
        (state : 'State)
        : FlatList<'Result> * 'State =
        checkNotDefault (nameof list) list
        let len = list.Length

        if len = 0 then
            empty, state
        else
            let resultBuilder = FlatListFactory.CreateBuilder<'Result> (len)
            resultBuilder.Count <- len

            let mutable currentState = state

            for i = len - 1 downto 0 do
                let item = list.[i]
                let result, newState = mapping item currentState
                resultBuilder.[i] <- result
                currentState <- newState

            resultBuilder.MoveToImmutable (), currentState

    [<CompiledName "CountBy">]
    let countBy (projection : 'T -> 'Key) (list : FlatList<'T>) =
        list.GroupBy(projection).Select(fun group -> struct (group.Key, Seq.length group)).ToImmutableArray ()

    [<CompiledName "Indexed">]
    let indexed (list : FlatList<'T>) = list.Select(fun item index -> struct (index, item)).ToImmutableArray ()

    [<CompiledName "Iter">]
    let inline iter (action : 'T -> unit) (list : FlatList<'T>) =
        for item in list do
            action item

    [<CompiledName "Iteri">]
    let iteri action (list : FlatList<'T>) =
        for i = 0 to list.Length - 1 do
            do action i list.[i]

    [<CompiledName "Iter2">]
    let iter2 (action : 'T1 -> 'T2 -> unit) (list1 : FlatList<'T1>) (list2 : FlatList<'T2>) =
        let len = list1.Length
        if len <> list2.Length then
            invalidArg (nameof list2) ErrorStrings.ListsHaveDifferentLengths
        for i = 0 to len - 1 do
            do action list1.[i] list2.[i]

    [<CompiledName "Iter3">]
    let iter3 (action : 'T1 -> 'T2 -> 'T3 -> unit) (list1 : FlatList<'T1>) (list2 : FlatList<'T2>) (list3 : FlatList<'T3>) =
        let len1 = list1.Length
        if len1 <> list2.Length then
            invalidArg (nameof list2) ErrorStrings.ListsHaveDifferentLengths
        if len1 <> list3.Length then
            invalidArg (nameof list3) ErrorStrings.ListsHaveDifferentLengths
        for i = 0 to len1 - 1 do
            action list1.[i] list2.[i] list3.[i]

    [<CompiledName "Iteri2">]
    let iteri2 (action : int -> 'T1 -> 'T2 -> unit) (list1 : FlatList<'T1>) (list2 : FlatList<'T2>) =
        let len1 = list1.Length
        if len1 <> list2.Length then
            invalidArg (nameof list2) ErrorStrings.ListsHaveDifferentLengths
        for i = 0 to len1 - 1 do
            action i list1.[i] list2.[i]

    [<CompiledName "Iteri3">]
    let iteri3
        (action : int -> 'T1 -> 'T2 -> 'T3 -> unit)
        (list1 : FlatList<'T1>)
        (list2 : FlatList<'T2>)
        (list3 : FlatList<'T3>)
        =
        let len1 = list1.Length
        if len1 <> list2.Length then
            invalidArg (nameof list2) ErrorStrings.ListsHaveDifferentLengths
        if len1 <> list3.Length then
            invalidArg (nameof list3) ErrorStrings.ListsHaveDifferentLengths
        for i = 0 to len1 - 1 do
            action i list1.[i] list2.[i] list3.[i]

    [<CompiledName "Exists">]
    let exists (predicate : 'T -> bool) (list : FlatList<'T>) = list.Any (predicate)

    [<CompiledName "Exists2">]
    let exists2 (predicate : 'T1 -> 'T2 -> bool) (list1 : FlatList<'T1>) (list2 : FlatList<'T2>) =
        let len = list1.Length
        if len <> list2.Length then
            invalidArg (nameof list2) ErrorStrings.ListsHaveDifferentLengths
        let rec loop i = i < len && (predicate list1.[i] list2.[i] || loop (i + 1))
        loop 0

    [<CompiledName "Exists3">]
    let exists3 (predicate : 'T1 -> 'T2 -> 'T3 -> bool) (list1 : FlatList<'T1>) (list2 : FlatList<'T2>) (list3 : FlatList<'T3>) =
        let len1 = list1.Length
        if len1 <> list2.Length then
            invalidArg (nameof list2) ErrorStrings.ListsHaveDifferentLengths
        if len1 <> list3.Length then
            invalidArg (nameof list3) ErrorStrings.ListsHaveDifferentLengths
        let rec loop i =
            i < len1
            && (predicate list1.[i] list2.[i] list3.[i] || loop (i + 1))
        loop 0

    [<CompiledName "Forall">]
    let forall (predicate : 'T -> bool) (list : FlatList<'T>) = list.All (predicate)

    [<CompiledName "Forall2">]
    let forall2 (predicate : 'T1 -> 'T2 -> bool) (list1 : FlatList<'T1>) (list2 : FlatList<'T2>) =
        let len1 = list1.Length
        if len1 <> list2.Length then
            invalidArg (nameof list2) ErrorStrings.ListsHaveDifferentLengths
        let rec loop i = i >= len1 || (predicate list1.[i] list2.[i] && loop (i + 1))
        loop 0

    [<CompiledName "Forall3">]
    let forall3 (predicate : 'T1 -> 'T2 -> 'T3 -> bool) (list1 : FlatList<'T1>) (list2 : FlatList<'T2>) (list3 : FlatList<'T3>) =
        let len1 = list1.Length
        if len1 <> list2.Length then
            invalidArg (nameof list2) ErrorStrings.ListsHaveDifferentLengths
        if len1 <> list3.Length then
            invalidArg (nameof list3) ErrorStrings.ListsHaveDifferentLengths
        let rec loop i =
            i >= len1
            || (predicate list1.[i] list2.[i] list3.[i] && loop (i + 1))
        loop 0

    [<CompiledName "Contains">]
    let inline contains item (list : FlatList<'T>) = list.Contains (item)

    [<CompiledName "Partition">]
    let partition (predicate : 'T -> bool) (list : FlatList<'T>) =
        let res1 = builderWith list.Length
        let res2 = builderWith list.Length
        for x in list do // Iteration will cause InvalidOperationException if list is default
            if predicate x then res1.Add x else res2.Add x
        (res1.ToImmutable (), res2.ToImmutable ())

    [<CompiledName "Find">]
    let find (predicate : 'T -> bool) (list : FlatList<'T>) = list.First (predicate)

    [<CompiledName "TryFind">]
    let tryFind (predicate : 'T -> bool) (list : FlatList<'T>) : 'T voption = list.Where (predicate) |> Seq.vtryHead

    [<CompiledName "FindBack">]
    let findBack (predicate : 'T -> bool) (list : FlatList<'T>) = list.Last (predicate)

    [<CompiledName "FindLast">]
    let findLast (predicate : 'T -> bool) (list : FlatList<'T>) = list.Last (predicate)

    [<CompiledName "TryFindBack">]
    let tryFindBack (predicate : 'T -> bool) (list : FlatList<'T>) : 'T voption =
        let mutable result = ValueNone
        for i = 0 to list.Length - 1 do
            let item = list.[i]
            if predicate item then
                result <- ValueSome item
        result

    [<CompiledName "FindIndexBack">]
    let findIndexBack (predicate : 'T -> bool) (list : FlatList<'T>) =
        let mutable idx = -1
        for i = 0 to list.Length - 1 do
            if predicate list.[i] then
                idx <- i
        if idx >= 0 then idx else sequenceNotFound ()

    [<CompiledName "TryFindLast">]
    let tryFindLast (predicate : 'T -> bool) (list : FlatList<'T>) : 'T voption =
        let mutable result = ValueNone
        for i = list.Length - 1 downto 0 do
            let item = list.[i]
            if predicate item && ValueOption.isNone result then
                result <- ValueSome item
        result

    [<CompiledName "TryFindIndexBack">]
    let tryFindIndexBack (predicate : 'T -> bool) (list : FlatList<'T>) : int voption =
        if list.IsDefaultOrEmpty then
            ValueNone
        else
            let mutable i = list.Length - 1
            let mutable result = ValueNone

            while i >= 0 && ValueOption.isNone result do
                if predicate list.[i] then
                    result <- ValueSome i
                i <- i - 1

            result

    [<CompiledName "FindLastIndex">]
    let findLastIndex (predicate : 'T -> bool) (list : FlatList<'T>) : int =
        let mutable found = false
        let mutable idx = -1
        for i = list.Length - 1 downto 0 do
            if predicate list.[i] && not found then
                idx <- i
                found <- true
        if found then idx else sequenceNotFound ()

    [<CompiledName "TryFindLastIndex">]
    let tryFindLastIndex (predicate : 'T -> bool) (list : FlatList<'T>) : int voption =
        let mutable result = ValueNone
        for i = list.Length - 1 downto 0 do
            if predicate list.[i] && ValueOption.isNone result then
                result <- ValueSome i
        result

    [<CompiledName "Pick">]
    let pick (chooser : 'T -> 'U voption) (list : FlatList<'T>) =
        checkNotDefault (nameof list) list
        let mutable result = ValueNone
        let mutable i = 0
        while i < list.Length && ValueOption.isNone result do
            result <- chooser list.[i]
            i <- i + 1
        match result with
        | ValueSome x -> x
        | ValueNone -> sequenceNotFound ()

    [<CompiledName "TryPick">]
    let tryPick (chooser : 'T -> 'U voption) (list : FlatList<'T>) : 'U voption =
        list.Select(chooser).Where(ValueOption.isSome).Select (ValueOption.get)
        |> Seq.vtryHead

    [<CompiledName "PickBack">]
    let pickBack (chooser : 'T -> 'U voption) (list : FlatList<'T>) : 'U =
        let mutable result = ValueNone
        for i = list.Length - 1 downto 0 do
            let v = chooser list.[i]
            if ValueOption.isSome v && ValueOption.isNone result then
                result <- v
        match result with
        | ValueSome x -> x
        | ValueNone -> sequenceNotFound ()

    [<CompiledName "TryPickBack">]
    let tryPickBack (chooser : 'T -> 'U voption) (list : FlatList<'T>) : 'U voption =
        let mutable result = ValueNone
        for i = list.Length - 1 downto 0 do
            let v = chooser list.[i]
            if ValueOption.isSome v && ValueOption.isNone result then
                result <- v
        result

    [<CompiledName "Choose">]
    let choose (chooser : 'T -> 'U voption) (list : FlatList<'T>) : FlatList<'U> =
        list.Select(chooser).Where(ValueOption.isSome).Select(ValueOption.get).ToImmutableArray ()

    [<CompiledName "ChooseBack">]
    let chooseBack (chooser : 'T -> 'U voption) (list : FlatList<'T>) : FlatList<'U> =
        let builder = FlatListFactory.CreateBuilder<'U> ()
        for i = list.Length - 1 downto 0 do
            match chooser list.[i] with
            | ValueSome v -> builder.Add v
            | ValueNone -> ()
        builder.ToImmutable ()

    [<CompiledName "GroupBy">]
    let groupBy (projection : 'T -> 'Key) (list : FlatList<'T>) =
        list.GroupBy(projection).Select(fun group -> struct (group.Key, group.ToImmutableArray ())).ToImmutableArray ()

    [<CompiledName "DistinctBy">]
    let distinctBy (projection : 'T -> 'Key) (list : FlatList<'T>) =
        if list.IsDefaultOrEmpty then
            list
        else
            let setBuilder = ImmutableHashSet.CreateBuilder<'Key> ()
            let arrayBuilder = ImmutableArray.CreateBuilder<'T> ()
            for i = 0 to list.Length - 1 do
                let item = list.[i]
                if setBuilder.Add (projection item) then
                    arrayBuilder.Add (item)
            arrayBuilder.ToImmutable ()

    [<CompiledName "FindDup">]
    let findDup (list : FlatList<'T>) =
        checkNotDefault (nameof list) list
        let seen = System.Collections.Generic.HashSet<'T> ()
        let mutable result = ValueNone
        let mutable i = 0
        while i < list.Length && ValueOption.isNone result do
            let item = list.[i]
            if not (seen.Add (item)) then
                result <- ValueSome item
            i <- i + 1
        match result with
        | ValueSome x -> x
        | ValueNone -> indexNotFound ()

    [<CompiledName "FindDupBy">]
    let findDupBy (projection : 'T -> 'Key) (list : FlatList<'T>) =
        checkNotDefault (nameof list) list
        let seen = System.Collections.Generic.HashSet<'Key> ()
        let mutable result = ValueNone
        let mutable i = 0
        while i < list.Length && ValueOption.isNone result do
            let item = list.[i]
            let key = projection item
            if not (seen.Add (key)) then
                result <- ValueSome item
            i <- i + 1
        match result with
        | ValueSome x -> x
        | ValueNone -> indexNotFound ()

    [<CompiledName "Collect">]
    let collect (mapping : 'T -> 'U seq) (list : FlatList<'T>) : FlatList<'U> = list.SelectMany(mapping).ToImmutableArray ()

    [<CompiledName "TryItem">]
    let tryItem index (list : FlatList<'T>) : voption<'T> =
        // list.Length or list.[index] will throw if list is default before comparison happens
        if list.IsDefault then
            ValueNone // Explicitly handle default case for tryItem to return ValueNone
        elif index >= 0 && index < list.Length then
            ValueSome list.[index]
        else
            ValueNone

    [<CompiledName "Head">]
    let head (list : FlatList<'T>) = list.First ()

    [<CompiledName "TryHead">]
    let tryHead (list : FlatList<'T>) : 'T voption =
        if list.IsDefaultOrEmpty then
            ValueNone
        else
            ValueSome list.[0]

    [<CompiledName "Last">]
    let last (list : FlatList<_>) = list.Last ()

    [<CompiledName "TryLast">]
    let tryLast (list : FlatList<'T>) : 'T voption =
        if list.IsDefaultOrEmpty then
            ValueNone
        else
            ValueSome list.[list.Length - 1]

    [<CompiledName "Tail">]
    let tail (list : FlatList<'T>) =
        if list.IsDefaultOrEmpty then
            invalidArg (nameof list) "List must not be empty to get tail."
        list.Slice (1, list.Length - 1)

    [<CompiledName "TryTail">]
    let tryTail (list : FlatList<'T>) : voption<FlatList<'T>> =
        if list.IsDefaultOrEmpty then
            ValueNone
        else
            ValueSome (list.Slice (1, list.Length - 1))

    [<CompiledName "TryHeadAndTail">]
    let tryHeadAndTail (list : FlatList<'T>) : ('T * FlatList<'T>) voption =
        if list.IsDefaultOrEmpty then
            ValueNone
        else
            ValueSome (list.[0], list.Slice (1, list.Length - 1))

    [<CompiledName "TryLastAndInit">]
    let tryLastAndInit (list : FlatList<'T>) : (FlatList<'T> * 'T) voption =
        if list.IsDefaultOrEmpty then
            ValueNone
        else
            ValueSome (list.Slice (0, list.Length - 1), list.[list.Length - 1])

    [<CompiledName "Take">]
    let take (count : int) (list : FlatList<'T>) =
        if count < 0 then
            invalidArg (nameof count) ErrorStrings.InputMustBeNonNegative
        let len = list.Length
        if count = 0 then empty
        elif count >= len then list
        else list.Slice (0, count)

    [<CompiledName "TakeEnd">]
    let takeEnd (count : int) (list : FlatList<'T>) : FlatList<'T> =
        if count < 0 || count > list.Length then
            invalidArg (nameof count) ErrorStrings.InputMustBeNonNegative
        if count = 0 then
            empty
        else
            list.Slice (list.Length - count, count)

    [<CompiledName "TakeWhile">]
    let takeWhile (predicate : 'T -> bool) (list : FlatList<'T>) = list.TakeWhile(predicate).ToImmutableArray ()

    [<CompiledName "Skip">]
    let skip index (list : FlatList<'T>) =
        if index < 0 then
            invalidArg (nameof index) ErrorStrings.InputMustBeNonNegative
        let len = list.Length
        if index = 0 then list
        elif index >= len then empty
        else list.Slice (index, len - index)

    [<CompiledName "SkipEnd">]
    let skipEnd (count : int) (list : FlatList<'T>) : FlatList<'T> =
        if count < 0 || count > list.Length then
            invalidArg (nameof count) ErrorStrings.InputMustBeNonNegative
        if count = 0 then
            list
        else
            list.Slice (0, list.Length - count)

    [<CompiledName "SkipWhile">]
    let skipWhile (predicate : 'T -> bool) (list : FlatList<'T>) = list.SkipWhile(predicate).ToImmutableArray ()

    [<CompiledName "Sub">]
    let sub start count (list : FlatList<'T>) = list.Slice (start, count)

    [<CompiledName "Truncate">]
    let truncate count (list : FlatList<'T>) = if count < list.Length then list.Slice (0, count) else list

    [<CompiledName "SplitAt">]
    let splitAt index (list : FlatList<'T>) = (list.Slice (0, index), list.Slice (index, list.Length - index))

    [<CompiledName "ChunkBySize">]
    let chunkBySize chunkSize (list : FlatList<'T>) =
        if chunkSize <= 0 then
            invalidArg (nameof chunkSize) ErrorStrings.InputMustBeNonNegative
        let len = list.Length
        if len = 0 then
            empty
        else
            let numChunks = (len + chunkSize - 1) / chunkSize
            let builder = FlatListFactory.CreateBuilder<FlatList<'T>> (numChunks)

            for i = 0 to numChunks - 1 do
                let start = i * chunkSize
                if start < len then
                    let remaining = len - start
                    let count = min chunkSize remaining
                    builder.Add (list.Slice (start, count))

            builder.ToImmutable ()

    [<CompiledName "Build">]
    let inline build f =
        let builder = builder ()
        f builder
        builder.ToImmutable ()

    [<CompiledName "Update">]
    let inline update f (list : FlatList<'T>) =
        let builder = toBuilder list
        f builder
        builder.ToImmutable ()

    [<CompiledName "FindIndex">]
    let findIndex (predicate : 'T -> bool) (list : FlatList<'T>) =
        checkNotDefault (nameof list) list

        let mutable index = -1
        let mutable found = false
        let len = list.Length

        if len = 0 then
            sequenceNotFound ()

        let mutable i = 0
        while i < len && not found do
            if predicate list.[i] then
                index <- i
                found <- true
            else
                i <- i + 1

        if found then index else sequenceNotFound ()

    [<CompiledName "TryFindIndex">]
    let tryFindIndex (predicate : 'T -> bool) (list : FlatList<'T>) : int voption =
        list.Select (fun item i -> struct (item, i))
        |> Seq.where (fun struct (item, i) -> predicate item)
        |> Seq.map (fun struct (item, i) -> i)
        |> Seq.vtryHead

    [<CompiledName "Windowed">]
    let windowed windowSize (list : FlatList<'T>) =
        if windowSize < 1 then
            invalidArg (nameof windowSize) ErrorStrings.InputMustBeNonNegative
        let len = list.Length
        if windowSize > len then
            empty
        else
            Enumerable.Range(0, len - windowSize + 1).Select(fun i -> list.Slice (i, windowSize)).ToImmutableArray ()

    [<CompiledName "Pairwise">]
    let pairwise (list : FlatList<'T>) =
        if list.Length < 2 then
            empty
        else
            Enumerable.Zip(list, list.Skip (1), fun first second -> struct (first, second)).ToImmutableArray ()

    [<CompiledName "SplitInto">]
    let splitInto (count : int) (list : FlatList<'T>) : FlatList<FlatList<'T>> =
        if count <= 0 then
            invalidArg (nameof count) ErrorStrings.InputMustBeNonNegative

        let len = list.Length
        if len = 0 then
            empty
        else
            let chunkSize = (len + count - 1) / count
            chunkBySize chunkSize list

    [<CompiledName "SplitIntoN">]
    let splitIntoN (count : int) (list : FlatList<'T>) : FlatList<FlatList<'T>> = splitInto count list

    [<CompiledName "Distinct">]
    let distinct (list : FlatList<'T>) =
        if list.IsDefaultOrEmpty then
            list
        else
            let seen = System.Collections.Generic.HashSet<'T> ()
            let builder = ImmutableArray.CreateBuilder<'T> ()
            for item in list do
                if seen.Add (item) then
                    builder.Add (item)
            builder.ToImmutable ()

    [<CompiledName "AllPairs">]
    let allPairs (xs : FlatList<'T>) (ys : FlatList<'U>) = xs.SelectMany(fun x -> ys.Select (fun y -> (x, y))).ToImmutableArray ()

    [<CompiledName "Permute">]
    let permute indexMap (list : FlatList<'T>) =
        let len = list.Length
        if len = 0 then
            list
        else
            let builder = FlatListFactory.CreateBuilder<'T> len
            builder.Count <- len
            let usedSourceIndices = System.Collections.Generic.HashSet<int> ()

            for i = 0 to len - 1 do
                let sourceIndex = indexMap i
                if sourceIndex < 0 || sourceIndex >= len then
                    invalidArg (nameof indexMap) "Invalid permutation function, source index out of range"
                if not (usedSourceIndices.Add (sourceIndex)) then
                    invalidArg (nameof indexMap) "Invalid permutation function, duplicate source indices mapped"

                builder.[i] <- list.[sourceIndex]
            builder.MoveToImmutable ()

    [<CompiledName "Zip">]
    let zip (list1 : FlatList<'T>) (list2 : FlatList<'U>) =
        let len1 = list1.Length
        if len1 <> list2.Length then
            invalidArg (nameof list2) ErrorStrings.ListsHaveDifferentLengths
        Enumerable.Range(0, len1).Select(fun i -> struct (list1.[i], list2.[i])).ToImmutableArray ()

    [<CompiledName "Zip3">]
    let zip3 (list1 : FlatList<'T>) (list2 : FlatList<'U>) (list3 : FlatList<'V>) =
        let len1 = list1.Length
        if len1 <> list2.Length then
            invalidArg (nameof list2) ErrorStrings.ListsHaveDifferentLengths
        if len1 <> list3.Length then
            invalidArg (nameof list3) ErrorStrings.ListsHaveDifferentLengths
        Enumerable.Range(0, len1).Select(fun i -> struct (list1.[i], list2.[i], list3.[i])).ToImmutableArray ()

    [<CompiledName "Unzip">]
    let unzip (list : FlatList<struct ('T * 'U)>) =
        if list.IsEmpty then
            struct (empty, empty)
        else
            struct (list.Select(fstv).ToImmutableArray (), list.Select(sndv).ToImmutableArray ())

    [<CompiledName "Unzip3">]
    let unzip3 (list : FlatList<struct ('T * 'U * 'V)>) =
        if list.IsEmpty then
            struct (empty, empty, empty)
        else
            let res1 = list.Select(fun struct (x, _, _) -> x).ToImmutableArray ()
            let res2 = list.Select(fun struct (_, y, _) -> y).ToImmutableArray ()
            let res3 = list.Select(fun struct (_, _, z) -> z).ToImmutableArray ()
            struct (res1, res2, res3)

    [<CompiledName "Average">]
    let inline average<'T
        when 'T : (static member (+) : 'T * 'T -> 'T)
        and 'T : (static member DivideByInt : 'T * int -> 'T)
        and 'T : (static member Zero : 'T)>
        (list : FlatList<'T>)
        =
        if list.Length = 0 then
            invalidArg (nameof list) LanguagePrimitives.ErrorStrings.InputArrayEmptyString
        let sum = list.Aggregate ('T.Zero, fun acc x -> Checked.(+) acc x)
        'T.DivideByInt (sum, list.Length)

    [<CompiledName "AverageBy">]
    let inline averageBy<'T, 'U
        when 'U : (static member (+) : 'U * 'U -> 'U)
        and 'U : (static member DivideByInt : 'U * int -> 'U)
        and 'U : (static member Zero : 'U)>
        (projection : 'T -> 'U)
        (list : FlatList<'T>)
        =
        let sum = list.Aggregate ('U.Zero, fun acc x -> Checked.(+) acc (projection x))
        'U.DivideByInt (sum, list.Length)

    [<CompiledName "Fold">]
    let fold<'T, 'State> (folder : 'State -> 'T -> 'State) (state : 'State) (list : FlatList<'T>) = list.Aggregate (state, folder)

    [<CompiledName "Fold2">]
    let fold2<'T1, 'T2, 'State>
        (folder : 'State -> 'T1 -> 'T2 -> 'State)
        (state : 'State)
        (list1 : FlatList<'T1>)
        (list2 : FlatList<'T2>)
        =
        let len1 = list1.Length
        if len1 <> list2.Length then
            invalidArg (nameof list2) ErrorStrings.ListsHaveDifferentLengths
        let mutable acc = state
        for i = 0 to len1 - 1 do
            acc <- folder acc list1.[i] list2.[i]
        acc

    [<CompiledName "Foldi">]
    let foldi<'T, 'State> (folder : int -> 'State -> 'T -> 'State) (state : 'State) (list : FlatList<'T>) : 'State =
        let mutable acc = state
        for i = 0 to list.Length - 1 do
            acc <- folder i acc list.[i]
        acc

    [<CompiledName "Foldi2">]
    let foldi2<'T1, 'T2, 'State>
        (folder : int -> 'State -> 'T1 -> 'T2 -> 'State)
        (state : 'State)
        (list1 : FlatList<'T1>)
        (list2 : FlatList<'T2>)
        : 'State =
        let len = list1.Length
        if len <> list2.Length then
            invalidArg (nameof list2) ErrorStrings.ListsHaveDifferentLengths
        let mutable acc = state
        for i = 0 to len - 1 do
            acc <- folder i acc list1.[i] list2.[i]
        acc

    [<CompiledName "FoldBack">]
    let foldBack<'T, 'State> (folder : 'T -> 'State -> 'State) (list : FlatList<'T>) (state : 'State) =
        checkNotDefault (nameof list) list
        let mutable acc = state
        for i = list.Length - 1 downto 0 do
            acc <- folder list.[i] acc
        acc

    [<CompiledName "FoldBack2">]
    let foldBack2<'T1, 'T2, 'State>
        (folder : 'T1 -> 'T2 -> 'State -> 'State)
        (list1 : FlatList<'T1>)
        (list2 : FlatList<'T2>)
        (state : 'State)
        =
        let len1 = list1.Length
        if len1 <> list2.Length then
            invalidArg (nameof list2) ErrorStrings.ListsHaveDifferentLengths
        let mutable acc = state
        for i = len1 - 1 downto 0 do
            acc <- folder list1.[i] list2.[i] acc
        acc

    [<CompiledName "FoldBacki">]
    let foldBacki (folder : int -> 'T -> 'State -> 'State) (list : FlatList<'T>) (state : 'State) : 'State =
        let mutable acc = state
        for i = list.Length - 1 downto 0 do
            acc <- folder i list.[i] acc
        acc

    [<CompiledName "FoldBacki2">]
    let foldBacki2
        (folder : int -> 'T1 -> 'T2 -> 'State -> 'State)
        (list1 : FlatList<'T1>)
        (list2 : FlatList<'T2>)
        (state : 'State)
        : 'State =
        let len = list1.Length
        if len <> list2.Length then
            invalidArg (nameof list2) ErrorStrings.ListsHaveDifferentLengths
        let mutable acc = state
        for i = len - 1 downto 0 do
            acc <- folder i list1.[i] list2.[i] acc
        acc

    [<CompiledName "FoldBack3">]
    let foldBack3<'T1, 'T2, 'T3, 'State>
        (folder : 'T1 -> 'T2 -> 'T3 -> 'State -> 'State)
        (list1 : FlatList<'T1>)
        (list2 : FlatList<'T2>)
        (list3 : FlatList<'T3>)
        (state : 'State)
        =
        let len1 = list1.Length
        if len1 <> list2.Length then
            invalidArg (nameof list2) ErrorStrings.ListsHaveDifferentLengths
        if len1 <> list3.Length then
            invalidArg (nameof list3) ErrorStrings.ListsHaveDifferentLengths
        let mutable acc = state
        for i = len1 - 1 downto 0 do
            acc <- folder list1.[i] list2.[i] list3.[i] acc
        acc

    [<CompiledName "Reduce">]
    let reduce (reduction : 'T -> 'T -> 'T) (list : FlatList<'T>) =
        if list.IsDefaultOrEmpty then
            invalidArg (nameof list) LanguagePrimitives.ErrorStrings.InputArrayEmptyString
        else
            list.Aggregate (reduction)

    [<CompiledName "ReduceBack">]
    let reduceBack (reduction : 'T -> 'T -> 'T) (list : FlatList<'T>) =
        if list.IsDefaultOrEmpty then
            invalidArg (nameof list) LanguagePrimitives.ErrorStrings.InputArrayEmptyString
        else
            let len = list.Length
            let mutable result = list.[len - 1]
            for i = len - 2 downto 0 do
                result <- reduction list.[i] result
            result

    [<CompiledName "Scan">]
    let scan<'T, 'State> folder (state : 'State) (list : FlatList<'T>) =
        let builder = FlatListFactory.CreateBuilder<'State> (list.Length + 1)
        builder.Add state
        let mutable currentState = state
        for item in list do
            currentState <- folder currentState item
            builder.Add currentState
        builder.ToImmutable ()

    [<CompiledName "ScanBack">]
    let scanBack<'T, 'State> folder (list : FlatList<'T>) (state : 'State) =
        checkNotDefault (nameof list) list
        let len = list.Length
        let builder = FlatListFactory.CreateBuilder<'State> (len + 1)
        builder.Count <- len + 1

        builder.[len] <- state
        let mutable currentState = state
        for i = len - 1 downto 0 do
            currentState <- folder list.[i] currentState
            builder.[i] <- currentState

        builder.MoveToImmutable ()

    [<CompiledName "ExactlyOne">]
    let exactlyOne (list : FlatList<'T>) = list.Single ()

    [<CompiledName "TryExactlyOne">]
    let tryExactlyOne (list : FlatList<'T>) : 'T voption =
        if list.IsDefaultOrEmpty || list.Length <> 1 then
            ValueNone
        else
            ValueSome list.[0]

    [<CompiledName "Except">]
    let except (itemsToExclude : 'T seq) (list : FlatList<'T>) : FlatList<'T> =
        let excludeSet = HashSet (itemsToExclude)
        filter (fun x -> not (excludeSet.Contains x)) list

    [<CompiledName "Sum">]
    let inline sum (list : FlatList< ^T >) : ^T when ^T : (static member (+) : ^T * ^T -> ^T) and ^T : (static member Zero : ^T) =
        list.Aggregate (LanguagePrimitives.GenericZero< ^T>, fun acc x -> acc + x)

    [<CompiledName "SumBy">]
    let inline sumBy
        (projection : 'T -> 'U)
        (list : FlatList<'T>)
        : 'U when 'U : (static member (+) : 'U * 'U -> 'U) and 'U : (static member Zero : 'U) =
        list.Aggregate (LanguagePrimitives.GenericZero<'U>, fun acc x -> acc + (projection x))

    [<CompiledName "Transpose">]
    let transpose (lists : FlatList<FlatList<'T>>) : FlatList<FlatList<'T>> =
        if lists.IsDefaultOrEmpty then
            empty
        else
            let len0 = lists.[0].Length
            for i = 1 to lists.Length - 1 do
                if lists.[i].Length <> len0 then
                    invalidArg (nameof lists) "All inner arrays must have the same length."

            Enumerable
                .Range(0, len0)
                .Select(fun j -> Enumerable.Range(0, lists.Length).Select(fun i -> lists.[i].[j]).ToImmutableArray ())
                .ToImmutableArray ()

    [<CompiledName "UpdateAt">]
    let updateAt (index : int) (value : 'T) (list : FlatList<'T>) : FlatList<'T> = list.SetItem (index, value)

    [<CompiledName "RemoveAt">]
    let removeAt (index : int) (list : FlatList<'T>) : FlatList<'T> = list.RemoveAt (index)

    [<CompiledName "InsertAt">]
    let insertAt (index : int) (value : 'T) (list : FlatList<'T>) : FlatList<'T> = list.Insert (index, value)

    [<CompiledName "InsertManyAt">]
    let insertManyAt (index : int) (values : 'T seq) (list : FlatList<'T>) : FlatList<'T> = list.InsertRange (index, values)

    [<CompiledName "Fill">]
    let fill (index : int) (count : int) (value : 'T) (list : FlatList<'T>) : FlatList<'T> =
        if index < 0 || count < 0 || index + count > list.Length then
            invalidArg (nameof index) ErrorStrings.InputMustBeNonNegative
        let builder = list.ToBuilder ()
        for i = index to index + count - 1 do
            builder.[i] <- value
        builder.MoveToImmutable ()

    [<CompiledName "Unfold">]
    let unfold<'T, 'State> (generator : 'State -> struct ('T * 'State) voption) (state : 'State) : FlatList<'T> =
        let builder = builder ()
        let mutable currentState = state
        let mutable continuing = true

        while continuing do
            match generator currentState with
            | ValueSome (value, newState) ->
                builder.Add (value)
                currentState <- newState
            | ValueNone -> continuing <- false

        builder.ToImmutable ()

    [<CompiledName "CompareWith">]
    let compareWith (comparer : 'T -> 'T -> int) (list1 : FlatList<'T>) (list2 : FlatList<'T>) : int =
        if list1.IsDefault && list2.IsDefault then
            0
        elif list1.IsDefault then
            -1
        elif list2.IsDefault then
            1
        else
            let len1 = list1.Length
            let len2 = list2.Length
            let minLength = min len1 len2

            let mutable i = 0
            let mutable result = 0
            let mutable continueComparing = true

            while i < minLength && continueComparing do
                result <- comparer list1.[i] list2.[i]
                if result <> 0 then
                    continueComparing <- false
                else
                    i <- i + 1

            if result = 0 then compare len1 len2 else result

    [<CompiledName "Max">]
    let inline max<'T when 'T : comparison> (list : FlatList<'T>) : 'T =
        checkNotDefault (nameof list) list
        if list.IsEmpty then
            invalidArg (nameof list) LanguagePrimitives.ErrorStrings.InputArrayEmptyString
        let mutable acc = list.[0]
        for i = 1 to list.Length - 1 do
            let curr = list.[i]
            if curr > acc then
                acc <- curr
        acc

    [<CompiledName "MaxBy">]
    let inline maxBy<'T, 'Key when 'Key : comparison> (projection : 'T -> 'Key) (list : FlatList<'T>) : 'T =
        checkNotDefault (nameof list) list
        if list.IsEmpty then
            invalidArg (nameof list) LanguagePrimitives.ErrorStrings.InputArrayEmptyString
        let mutable maxVal = list.[0]
        let mutable maxKey = projection maxVal
        for i = 1 to list.Length - 1 do
            let currVal = list.[i]
            let currKey = projection currVal
            if currKey > maxKey then
                maxVal <- currVal
                maxKey <- currKey
        maxVal

    [<CompiledName "Min">]
    let inline min<'T when 'T : comparison> (list : FlatList<'T>) : 'T =
        checkNotDefault (nameof list) list
        if list.IsEmpty then
            invalidArg (nameof list) LanguagePrimitives.ErrorStrings.InputArrayEmptyString
        let mutable acc = list.[0]
        for i = 1 to list.Length - 1 do
            let curr = list.[i]
            if curr < acc then
                acc <- curr
        acc

    [<CompiledName "MinBy">]
    let inline minBy<'T, 'Key when 'Key : comparison> (projection : 'T -> 'Key) (list : FlatList<'T>) : 'T =
        checkNotDefault (nameof list) list
        if list.IsEmpty then
            invalidArg (nameof list) LanguagePrimitives.ErrorStrings.InputArrayEmptyString
        let mutable minVal = list.[0]
        let mutable minKey = projection minVal
        for i = 1 to list.Length - 1 do
            let currVal = list.[i]
            let currKey = projection currVal
            if currKey < minKey then
                minVal <- currVal
                minKey <- currKey
        minVal
