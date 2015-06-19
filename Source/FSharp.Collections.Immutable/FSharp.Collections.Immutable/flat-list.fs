﻿#if INTERACTIVE
namespace global
#else
namespace FSharp.Collections.Immutable
#endif

// The FlatList name comes from a similar construct seen in the official F# source code
type FlatList<'T> = System.Collections.Immutable.ImmutableArray<'T>


// based on the F# Array module source
[<RequireQualifiedAccess; CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module FlatList =

    type internal FlatListFactory = System.Collections.Immutable.ImmutableArray

    let inline checkNotDefault argName (list: FlatList<'T>) =
        if list.IsDefault then invalidArg argName "Uninstantiated ImmutableArray/FlatList"
    let inline check (list: FlatList<'T>) = checkNotDefault "list" list
    
    let indexNotFound() = raise (System.Collections.Generic.KeyNotFoundException())

    let length list = check list; list.Length

    let item index list = check list; list.[index]

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
    let exceptWith (comparer: System.Collections.Generic.IEqualityComparer<_>) items list: FlatList<_> =
        check list
        list.RemoveRange(items, comparer)

    /// Removes the specified objects from the list.
    let except items list = exceptWith HashIdentity.Structural items list


    /// Removes all the elements that do not match the conditions defined by the specified predicate.
    let filter predicate list: FlatList<_> =
        check list    
        System.Predicate(not << predicate)
        |> list.RemoveAll

    /// Removes a range of elements from the list.
    let removeRange index (count: int) list: FlatList<_> = check list; list.RemoveRange(index, count)

    let blit source sourceIndex (destination: 'T[]) destinationIndex count =
        checkNotDefault "source" source
        try
            source.CopyTo(sourceIndex, destination, destinationIndex, count)
        with
        |exn -> raise exn // throw same exception with the correct stack trace. Update exception code


    ////////// Building //////////

    let builder(): FlatList<'T>.Builder = FlatListFactory.CreateBuilder()
    let builderWith capacity: FlatList<'T>.Builder = FlatListFactory.CreateBuilder(capacity)

    let toBuilder list: FlatList<_>.Builder = check list; list.ToBuilder()
    let ofBuilderMove (builder: FlatList<_>.Builder): FlatList<_> = 
        checkNotNull "builder" builder
        builder.MoveToImmutable()
    let ofBuilder (builder: FlatList<_>.Builder): FlatList<_> = 
        checkNotNull "builder" builder
        builder.ToImmutable()

    let inline build f =
        let builder = builder()
        f builder
        ofBuilderMove builder
    
    let inline update f list =
        let builder = toBuilder list
        f builder
        ofBuilderMove builder


    ////////// Loop-based //////////

    let init count initializer =
        if count < 0 then
            // throw the same exception
            try
                Array.init count initializer |> ignore
            with
            |exn -> raise exn // use correct stack trace
        let builder = builderWith count
        for i = 0 to count - 1 do
            builder.Add <| initializer i
        ofBuilderMove builder

    let rec private concatAddLengths (arrs: FlatList<FlatList<_>>) i acc =
        if i >= length arrs then acc 
        else concatAddLengths arrs (i+1) (acc + arrs.[i].Length)
                
    let concat (arrs : FlatList<FlatList<'T>>) = // consider generalizing
        let result: FlatList<'T>.Builder = builderWith <| concatAddLengths arrs 0 0
        for i = 0 to length arrs - 1 do
            result.AddRange(arrs.[i]: FlatList<'T>)
        ofBuilderMove result

    ////////// Based on other operations //////////

    let take count list =
        removeRange count (length list - count) list

    let skip index list = removeRange 0 index list

    let truncate count list = if count < length list then take count list else list

    let head list = item 0 list

    let tryItem index list =
        if index < length list then Some <| item index list
        else None

    let tryHead list = tryItem 0 list


    let last list = item (length list - 1) list

    let tryLast list = tryItem (length list - 1) list

    let tail list = removeRange 1 (length list - 1) list

    let tryTail list = if isEmpty list then None else Some <| tail list

    let create count item = init count <| fun _ -> item // optimize


    ////////// Creating //////////

    let ofSeq source: FlatList<'T> = FlatListFactory.CreateRange source

    let empty<'T> : FlatList<_> = FlatListFactory.Create<'T>()

    //////////

    

    

    


    




