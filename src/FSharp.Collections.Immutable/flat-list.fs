#if INTERACTIVE
namespace global
#else
namespace FSharp.Collections.Immutable
#endif

// The FlatList name comes from a similar construct seen in the official F# source code
type FlatList<'T> = System.Collections.Immutable.ImmutableArray<'T>

// based on the F# Array module source
[<RequireQualifiedAccess; CompiledName((nameof System.Collections.Immutable.ImmutableArray) + "Module")>]
module FlatList =

    type internal FlatListFactory = System.Collections.Immutable.ImmutableArray

    let inline empty<'T> : FlatList<_> = FlatListFactory.Create<'T>()

    let inline singleton<'T> (item : 'T) : FlatList<'T> = FlatListFactory.Create<'T> (item)

    let inline ofSeq source: FlatList<'T> = FlatListFactory.CreateRange source

    let inline toSeq (flatList: FlatList<_>) = flatList :> seq<_>

    ////////// Building //////////

    let moveFromBuilder (builder : FlatList<_>.Builder) : FlatList<_> =
        checkNotNull (nameof builder) builder
        builder.MoveToImmutable()
    let ofBuilder (builder : FlatList<_>.Builder) : FlatList<_> =
        checkNotNull (nameof builder) builder
        builder.ToImmutable()

    let builder () : FlatList<'T>.Builder = FlatListFactory.CreateBuilder()

    let builderWith capacity : FlatList<'T>.Builder = FlatListFactory.CreateBuilder(capacity)

    let inline internal checkNotDefault argName (list : FlatList<'T>) =
        if list.IsDefault then invalidArg argName "Uninstantiated ImmutableArray/FlatList"

    let inline internal check (list : FlatList<'T>) = checkNotDefault (nameof list) list

    let inline internal indexNotFound() = raise (System.Collections.Generic.KeyNotFoundException())

    let length list = check list; list.Length

    let item index list = check list; list.[index]

    let append list1 list2 : FlatList<'T> =
        checkNotDefault (nameof list1) list1
        checkNotDefault (nameof list2) list2
        list1.AddRange(list2 : FlatList<_>)

    /// Searches for the specified object and returns the zero-based index of the first occurrence within the range
    /// of elements in the list that starts at the specified index and
    /// contains the specified number of elements.
    let indexRangeWith comparer index count item list =
        check list
        list.IndexOf(item, index, count, comparer)
    let indexRange index count item list =
        indexRangeWith HashIdentity.Structural index count item list
    let indexFromWith comparer index item list =
        indexRangeWith comparer index (length list - index) item
    let indexFrom index item list =
        indexFromWith HashIdentity.Structural index item list
    let indexWith comparer item list =
        indexFromWith comparer 0 item list
    let index item list = indexWith HashIdentity.Structural item list

    /// Searches for the specified object and returns the zero-based index of the last occurrence within the
    /// range of elements in the list that contains the specified number
    /// of elements and ends at the specified index.
    let lastIndexRangeWith comparer index count item list =
        check list
        list.LastIndexOf(item, index, count, comparer)
    let lastIndexRange index count item list =
        lastIndexRangeWith HashIdentity.Structural index count item list
    let lastIndexFromWith comparer index item list =
        lastIndexRangeWith comparer index (index + 1) item list
    let lastIndexFrom index item list =
        lastIndexFromWith HashIdentity.Structural index item list
    let lastIndexWith comparer item list =
        lastIndexFromWith comparer (length list - 1) item list
    let lastIndex item list = lastIndexWith HashIdentity.Structural item list

    let isEmpty (list: FlatList<_>) = list.IsEmpty

    let isDefault (list: FlatList<_>) = list.IsDefault

    let isDefaultOrEmpty (list: FlatList<_>) = list.IsDefaultOrEmpty

    /// Removes the specified objects from the list with the given comparer.
    let removeAllWith (comparer: System.Collections.Generic.IEqualityComparer<_>) items list: FlatList<_> =
        check list
        list.RemoveRange(items, comparer)

    /// Removes the specified objects from the list.
    let removeAll items list = removeAllWith HashIdentity.Structural items list

    /// Removes all the elements that do not match the conditions defined by the specified predicate.
    let filter predicate list: FlatList<_> =
        check list
        System.Predicate(not << predicate)
        |> list.RemoveAll

    /// Removes all the elements that do not match the conditions defined by the specified predicate.
    let where predicate list = filter predicate list

    /// Removes a range of elements from the list.
    let removeRange index (count: int) list: FlatList<_> = check list; list.RemoveRange(index, count)

    let blit source sourceIndex (destination: 'T[]) destinationIndex count =
        checkNotDefault (nameof source) source
        try
            source.CopyTo(sourceIndex, destination, destinationIndex, count)
        with
        |exn -> raise exn // throw same exception with the correct stack trace. Update exception code

    let sortRangeWithComparer comparer index count list =
        check list
        list.Sort(index, count, comparer)
    let sortRangeWith comparer index count list =
        sortRangeWithComparer (ComparisonIdentity.FromFunction comparer) index count list
    let sortRange index count list = sortRangeWithComparer ComparisonIdentity.Structural index count list
    let sortWithComparer (comparer : System.Collections.Generic.IComparer<_>) list = check list; list.Sort(comparer)
    let sortWith comparer list = sortWithComparer (ComparisonIdentity.FromFunction comparer) list
    let sort list = check list; list.Sort()

    ////////// Building //////////

    let toBuilder list: FlatList<_>.Builder = check list; list.ToBuilder()

    let inline private builderWithLengthOf list = builderWith <| length list

    module Builder =
        let inline private check (builder: FlatList<'T>.Builder) = checkNotNull (nameof builder) builder

        let add item builder = check builder; builder.Add(item)

    ////////// Loop-based //////////

    let init count initializer =
        if count < 0 then invalidArg (nameof count) ErrorStrings.InputMustBeNonNegative
        let builder = builderWith count
        for i = 0 to count - 1 do
            builder.Add <| initializer i
        moveFromBuilder builder

    let rec private concatAddLengths (arrs: FlatList<FlatList<_>>) i acc =
        if i >= length arrs then acc
        else concatAddLengths arrs (i+1) (acc + arrs.[i].Length)

    let concat (arrs : FlatList<FlatList<'T>>) = // consider generalizing
        let result: FlatList<'T>.Builder = builderWith <| concatAddLengths arrs 0 0
        for i = 0 to length arrs - 1 do
            result.AddRange(arrs.[i]: FlatList<'T>)
        moveFromBuilder result

    let inline map mapping list =
        check list
        let builder = builderWithLengthOf list
        for i = 0 to length list - 1 do
            builder.Add(mapping list.[i])
        moveFromBuilder builder

    let countBy projection list =
        check list
        // need struct box optimization
        let dict = new System.Collections.Generic.Dictionary<'Key, int>(HashIdentity.Structural)

        // Build the groupings
        for v in list do
            let key = projection v
            let mutable prev = Unchecked.defaultof<_>
            if dict.TryGetValue(key, &prev) then dict.[key] <- prev + 1 else dict.[key] <- 1

        let res = builderWith dict.Count
        let mutable i = 0
        for group in dict do
            res.Add(group.Key, group.Value)
            i <- i + 1
        moveFromBuilder res

    let indexed list =
        check list
        let builder = builderWithLengthOf list
        for i = 0 to length list - 1 do
            builder.Add(i, list.[i])
        moveFromBuilder builder

    let inline iter action list =
        check list
        for i = 0 to length list - 1 do
            action list.[i]

    let iter2 action list1 list2 =
        checkNotDefault (nameof list1) list1
        checkNotDefault (nameof list2) list2
        let f = OptimizedClosures.FSharpFunc<'T,'U, unit>.Adapt(action)
        let len = length list1
        if len <> length list2 then invalidArg (nameof list2) ErrorStrings.ListsHaveDifferentLengths
        for i = 0 to len - 1 do
            f.Invoke(list1.[i], list2.[i])

    let distinctBy projection (list: FlatList<'T>) =
        let builder: FlatList<'T>.Builder = builderWith <| length list
        let set = System.Collections.Generic.HashSet<'Key>(HashIdentity.Structural)
        let mutable outputIndex = 0

        for i = 0 to length list - 1 do
            let item = list.[i]
            if set.Add <| projection item then
                outputIndex <- outputIndex + 1
                Builder.add item builder

        ofBuilder builder

    let map2 mapping list1 list2 =
        checkNotDefault (nameof list1) list1
        checkNotDefault (nameof list2) list2
        let f = OptimizedClosures.FSharpFunc<_,_,_>.Adapt(mapping)
        let len1 = list1.Length
        if len1 <> list2.Length then invalidArg (nameof list2) ErrorStrings.ListsHaveDifferentLengths
        let res = builderWith len1
        for i = 0 to len1 - 1 do
            res.Add <| f.Invoke(list1.[i], list2.[i])
        moveFromBuilder res

    let map3 mapping list1 list2 list3 =
        checkNotDefault (nameof list1) list1
        checkNotDefault (nameof list2) list2
        checkNotDefault (nameof list3) list3
        let f = OptimizedClosures.FSharpFunc<_,_,_,_>.Adapt(mapping)
        let len1 = list1.Length
        if not (len1 = list2.Length)
            then invalidArg (nameof list2) ErrorStrings.ListsHaveDifferentLengths
        if not (len1 = list3.Length)
            then invalidArg (nameof list3) ErrorStrings.ListsHaveDifferentLengths

        let res = builderWith len1
        for i = 0 to len1 - 1 do
            res.Add <| f.Invoke(list1.[i], list2.[i], list3.[i])
        moveFromBuilder res
    let mapi2 mapping list1 list2 =
        checkNotDefault (nameof list1) list1
        checkNotDefault (nameof list2) list2
        let f = OptimizedClosures.FSharpFunc<_,_,_,_>.Adapt(mapping)
        let len1 = list1.Length
        if len1 <> list2.Length then invalidArg (nameof list2) ErrorStrings.ListsHaveDifferentLengths
        let res = builderWith len1
        for i = 0 to len1 - 1 do
            res.Add <| f.Invoke(i,list1.[i], list2.[i])
        moveFromBuilder res

    let iteri action list =
        check list
        let f = OptimizedClosures.FSharpFunc<_,_,_>.Adapt(action)
        let len = list.Length
        for i = 0 to len - 1 do
            f.Invoke(i, list.[i])

    let iteri2 action list1 list2 =
        checkNotDefault (nameof list1) list1
        checkNotDefault (nameof list2) list2
        let f = OptimizedClosures.FSharpFunc<_,_,_,_>.Adapt(action)
        let len1 = list1.Length
        if len1 <> list2.Length then invalidArg (nameof list2) ErrorStrings.ListsHaveDifferentLengths
        for i = 0 to len1 - 1 do
            f.Invoke(i,list1.[i], list2.[i])

    let mapi mapping list =
        check list
        let f = OptimizedClosures.FSharpFunc<_,_,_>.Adapt(mapping)
        let len = list.Length
        let res = builderWithLengthOf list
        for i = 0 to len - 1 do
            res.Add <| f.Invoke(i,list.[i])
        moveFromBuilder res

    let exists predicate list =
        check list
        let len = list.Length
        let rec loop i = i < len && (predicate list.[i] || loop (i+1))
        loop 0

    let inline contains e list =
        check list
        let mutable state = false
        let mutable i = 0
        while (not state && i < list.Length) do
            state <- e = list.[i]
            i <- i + 1
        state

    let exists2 predicate list1 list2 =
        checkNotDefault (nameof list1) list1
        checkNotDefault (nameof list2) list2
        let f = OptimizedClosures.FSharpFunc<_,_,_>.Adapt(predicate)
        let len1 = list1.Length
        if len1 <> list2.Length then invalidArg (nameof list2) ErrorStrings.ListsHaveDifferentLengths
        let rec loop i = i < len1 && (f.Invoke(list1.[i], list2.[i]) || loop (i+1))
        loop 0

    let forall predicate list =
        check list
        let len = list.Length
        let rec loop i = i >= len || (predicate list.[i] && loop (i+1))
        loop 0

    let forall2 predicate list1 list2 =
        checkNotDefault (nameof list1) list1
        checkNotDefault (nameof list2) list2
        let f = OptimizedClosures.FSharpFunc<_,_,_>.Adapt(predicate)
        let len1 = list1.Length
        if len1 <> list2.Length then invalidArg (nameof list2) ErrorStrings.ListsHaveDifferentLengths
        let rec loop i = i >= len1 || (f.Invoke(list1.[i], list2.[i]) && loop (i+1))
        loop 0

    let groupBy projection list =
        check list
        let dict = new System.Collections.Generic.Dictionary<'Key,ResizeArray<'T>>(HashIdentity.Structural)

        // Build the groupings
        for i = 0 to (list.Length - 1) do
            let v = list.[i]
            let key = projection v
            let ok, prev = dict.TryGetValue(key)
            if ok then
                prev.Add(v)
            else
                let prev = new ResizeArray<'T>(1)
                dict.[key] <- prev
                prev.Add(v)

        // Return the list-of-lists.
        let result = builderWith dict.Count
        let mutable i = 0
        for group in dict do
            result.Add(group.Key, ofSeq group.Value)
            i <- i + 1

        moveFromBuilder result

    let pick chooser list =
        check list
        let rec loop i =
            if i >= list.Length then
                indexNotFound()
            else
                match chooser list.[i] with
                | None -> loop(i+1)
                | Some res -> res
        loop 0

    let tryPick chooser list =
        check list
        let rec loop i =
            if i >= list.Length then None else
            match chooser list.[i] with
            | None -> loop(i+1)
            | res -> res
        loop 0

    let choose chooser list =
        check list
        let res = builderWith list.Length
        for i = 0 to list.Length - 1 do
            match chooser list.[i] with
            | None -> ()
            | Some b -> res.Add(b)
        ofBuilder res

    let partition predicate list =
        check list
        let res1 = builderWith list.Length
        let res2 = builderWith list.Length
        for i = 0 to list.Length - 1 do
            let x = list.[i]
            if predicate x then res1.Add(x) else res2.Add(x)
        ofBuilder res1, ofBuilder res2

    let find predicate list =
        check list
        let rec loop i =
            if i >= list.Length then indexNotFound() else
            if predicate list.[i] then list.[i]  else loop (i+1)
        loop 0
    let tryFind predicate list =
        check list
        let rec loop i =
            if i >= list.Length then None else
            if predicate list.[i] then Some list.[i]  else loop (i+1)
        loop 0
    let findBack predicate list =
        check list
        let rec loop i =
            if i < 0 then indexNotFound() else
            if predicate list.[i] then list.[i]  else loop (i - 1)
        loop <| length list - 1
    let tryFindBack predicate list =
        check list
        let rec loop i =
            if i < 0 then None else
            if predicate list.[i] then Some list.[i]  else loop (i+1)
        loop <| length list - 1

    let findIndexBack predicate list =
        check list
        let rec loop i =
            if i < 0 then indexNotFound() else
            if predicate list.[i] then i  else loop (i - 1)
        loop <| length list - 1

    let tryFindIndexBack predicate list =
        check list
        let rec loop i =
            if i < 0 then None else
            if predicate list.[i] then Some i  else loop (i - 1)
        loop <| length list - 1
    // TODO: windowed

    ////////// Based on other operations //////////

    let take count list = removeRange count (length list - count) list

    let inline private lengthWhile predicate list =
        check list
        let mutable count = 0
        while count < list.Length && predicate list.[count] do
            count <- count + 1
        count
    let takeWhile predicate list = take (lengthWhile predicate list) list

    let skip index list = removeRange 0 index list

    let skipWhile predicate list = skip (lengthWhile predicate list) list

    let sub start stop list = skip start list |> take (stop - start - 1)

    let truncate count list = if count < length list then take count list else list

    let splitAt index list = take index list, skip index list

    let head list = item 0 list

    let tryItem index list =
        if index >= length list || index < 0 then None
        else Some(list.[index])

    let tryHead list = tryItem 0 list

    let last (list : FlatList<_>) = list.[length list - 1]

    let tryLast list = tryItem (length list - 1) list

    let tail list = removeRange 1 (length list - 1) list

    let tryTail list = if isEmpty list then None else Some <| tail list

    let create count item = init count <| fun _ -> item // optimize

    let replicate count item = create item count

    let collect mapping list = concat <| map mapping list

    let inline build f =
        let builder = builder()
        f builder
        moveFromBuilder builder

    let inline update f list =
        let builder = toBuilder list
        f builder
        moveFromBuilder builder

    //////////

module ImmutableArray = FlatList
