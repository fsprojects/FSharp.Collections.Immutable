#if INTERACTIVE
namespace global
#else
namespace FSharp.Collections.Immutable
open FSharp.Collections.Immutable.ImmutableCollectionUtil
#endif
open System.Collections.Immutable

[<RequireQualifiedAccess; CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module ImmutableList =

    ////////// Factory //////////

    let inline internal check (list: IImmutableList<_>) = checkNotNull (nameof list) list

    let inline empty<'T> = ImmutableList.Create<'T>()
    let inline singleton<'T> (item : 'T) : ImmutableList<'T> = ImmutableList.Create<'T> (item)

    let inline ofSeq source = checkNotNull (nameof source) source; ImmutableList.CreateRange source
    let inline ofArray (source : _ array) = checkNotNull (nameof source) source; ImmutableList.CreateRange source
    let inline ofList (list : _ list) = ofSeq list

    let inline toSeq (list : ImmutableList<_>) = list :> seq<_>
    let inline toList (list : ImmutableList<_>) = check list; Seq.toList list
    let inline toArray (list : ImmutableList<_>) = check list; Seq.toArray list

    ////////// Building //////////

    let inline ofBuilder (builder : ImmutableList<_>.Builder) = builder.ToImmutable()

    let inline builder () = ImmutableList.CreateBuilder()

    let toBuilder (list: ImmutableList<_>) = check list; list.ToBuilder()

    let inline build f =
        let builder = builder()
        f builder
        builder.ToImmutable()

    let inline update f list =
        let builder = toBuilder list
        f builder
        builder.ToImmutable()


    open System.Collections.Generic
    open System

    ////////// IReadOnly* //////////

    let length list = check list; list.Count

    let item index list = check list; list.[index]

    ////////// ImmutableList //////////

    let contains item (list : ImmutableList<_>) = list.Contains(item)

    let reverse (list : ImmutableList<_>) = list.Reverse()

    let reverseRange (index, count) (list : ImmutableList<_>) = list.Reverse(index, count)

    ////////// IImmutableList //////////

    /// Replaces an element in the list at a given position with the specified element.
    let withItem index value list = check list; list.SetItem(index, value)

    /// Returns a new list with the first matching element in the list replaced with the specified element with
    /// the given comparer.
    let replaceWith comparer oldValue value list =
        check list
        list.Replace(oldValue, value, comparer)

    /// Returns a new list with the first matching element in the list replaced with the specified element.
    let replace oldValue value list =
        replaceWith HashIdentity.Structural oldValue value list


    /// Creates a list with all the items removed, but with the same sorting and ordering semantics as
    /// this list.
    let clear list = check list; list.Clear()

    /// Makes a copy of the list, and adds the specified object to the end of the copied list.
    let add item list = check list; list.Add item


    /// Makes a copy of the list and adds the specified objects to the end of the copied list.
    let append list items = check list; list.AddRange items

    /// Inserts the specified element at the specified index in a immutable list.
    let insert index item list = check list; list.Insert(index, item)

    /// Inserts the specified elements at the specified index in the immutable list.
    let insertRange index items list = check list; list.InsertRange(index, items) // TODO: rename


    /// Removes the first occurrence of a specified object from this immutable list using the given comparer.
    let removeWith comparer item list = check list; list.Remove(item, comparer)

    /// Removes the first occurrence of a specified object from this immutable list.
    let remove item list = removeWith HashIdentity.Structural item list


    /// Removes the specified objects from the list with the given comparer.
    let exceptWith (comparer: IEqualityComparer<_>) items list =
        check list
        list.RemoveRange(items, comparer)

    /// Removes the specified objects from the list.
    let except items list = exceptWith HashIdentity.Structural items list


    /// Removes all the elements that do not match the conditions defined by the specified predicate.
    let filter predicate list =
        check list
        Predicate(not << predicate)
        |> list.RemoveAll

    /// Removes a range of elements from the System.Collections.Immutable.IImmutableList`1.
    let removeRange index (count: int) list = check list; list.RemoveRange(index, count)

    /// Removes the element at the specified index of the immutable list.
    let removeAt index list = check list; list.RemoveAt index

    /// Searches for the specified object and returns the zero-based index of the first occurrence within the range
    /// of elements in the list that starts at the specified index and
    /// contains the specified number of elements.
    let indexRangeWith comparer index count item list =
        check list;
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

    ////////// Filter-based //////////

    let filterFold (predicate: 'State -> 'T -> bool * 'State) initial list =
        let state = ref initial
        filter (fun item ->
            let condition, state' = predicate !state item
            state := state'
            condition) list, !state

    let skipWhile predicate list =
        let condition = ref true
        filter (fun item ->
            if !condition then
                condition := !condition && predicate item
                !condition
            else false) list

    let skipUntil predicate list = skipWhile (not << predicate) list

    let takeWhile predicate list =
        let condition = ref true
        filter (fun item ->
            if !condition then
                condition := !condition && predicate item
                not !condition
            else true) list
    let takeUntil predicate list = takeWhile (not << predicate) list

    ////////// Loop-based //////////

    let concat lists =
        checkNotNull (nameof lists) lists
        build <| fun result ->
            for list in lists do
                result.AddRange list

    let map mapping list =
        check list
        build <| fun builder ->
            for item in list do
                builder.Add(mapping item)

    let choose chooser list =
        check list
        build <| fun builder ->
            for item in list do
                match chooser item with
                |Some item -> builder.Add item
                |None -> ()


    ////////// Based on other operations //////////

    let isEmpty list = length list = 0

    let take count list =
        removeRange count (length list - count) list

    let skip index list = removeRange 0 index list

    let truncate count list = if count < length list then take count list else list

    let splitAt index list = take index list, skip index list

    let head list = item 0 list

    let last (list : IImmutableList<_>) = list.[length list - 1]

    let tail list = removeAt 0 list

    let tryItem index list =
        if index >= length list || index < 0 then None
        else Some(list.[index])

    let tryHead list = tryItem 0 list

    let tryLast list = tryItem (length list - 1) list

    let tryTail list = if isEmpty list then None else Some <| tail list

    let collect mapping list = concat <| map mapping list

    let cons head list = insert 0 head list

    //let ofArray (array: 'T array) = ImmutableList.Create<'T>(items = [||])

    let init count initializer =
        if count < 0 then
            // throw the same exception
            try
                Seq.init count initializer |> ignore
            with
            |exn -> raise exn // get the right stack trace
        build <| fun builder ->
            for i = 0 to count - 1 do
                builder.Add <| initializer i

    let unfold generator state =
        let rec unfoldLoop state (builder: ImmutableList<_>.Builder) =
            match generator state with
            |Some(state, item) -> builder.Add(item); unfoldLoop state builder
            |None -> ()
        build <| unfoldLoop state


    ////////// Seq-based //////////

    let find predicate list = check list; Seq.find predicate list

    let tryFind predicate list = check list; Seq.tryFind predicate list

    let findIndex predicate list = check list; Seq.findIndex predicate list

    let tryFindIndex predicate list = check list; Seq.tryFindIndex predicate list

    let pick chooser list = check list; Seq.pick chooser list

    let fold folder state list = check list; Seq.fold folder state list

    let forall predicate list = check list; Seq.forall predicate list

    let forall2 predicate (list1: IImmutableList<_>) (list2: IImmutableList<_>) =
        checkNotNull (nameof list1) list1; checkNotNull (nameof list2) list2
        Seq.forall2 predicate list1 list2

    let iter action list = check list; Seq.iter action list
