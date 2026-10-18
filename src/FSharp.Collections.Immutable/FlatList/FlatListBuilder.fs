[<RequireQualifiedAccess>]
module FSharp.Collections.Immutable.FlatListBuilder

open System
open System.Collections.Generic
open System.Collections.Immutable

// ---- Properties ----

[<CompiledName "Capacity">]
let capacity (builder : FlatList<'T>.Builder) = builder.Capacity

[<CompiledName "Length">]
let length (builder : FlatList<'T>.Builder) = builder.Count

// ---- Access ----

[<CompiledName "Item">]
let item index (builder : FlatList<'T>.Builder) = builder[index]

// ---- Search ----

[<CompiledName "Contains">]
let contains (value : 'T) (builder : FlatList<'T>.Builder) = builder.Contains value

[<CompiledName "IndexOf">]
let indexOf (value : 'T) (builder : FlatList<'T>.Builder) = builder.IndexOf value

[<CompiledName "IndexOfRange">]
let indexOfRange (startIndex : int) (count : int) (value : 'T) (builder : FlatList<'T>.Builder) =
    builder.IndexOf (value, startIndex, count)

[<CompiledName "IndexOfRangeWith">]
let indexOfRangeWith
    (comparer : IEqualityComparer<'T>)
    (startIndex : int)
    (count : int)
    (value : 'T)
    (builder : FlatList<'T>.Builder)
    =
    builder.IndexOf (value, startIndex, count, comparer)

[<CompiledName "LastIndexOf">]
let lastIndexOf (value : 'T) (builder : FlatList<'T>.Builder) = builder.LastIndexOf value

[<CompiledName "LastIndexOfRange">]
let lastIndexOfRange (startIndex : int) (count : int) (value : 'T) (builder : FlatList<'T>.Builder) =
    builder.LastIndexOf (value, startIndex, count)

[<CompiledName "LastIndexOfRangeWith">]
let lastIndexOfRangeWith
    (comparer : IEqualityComparer<'T>)
    (startIndex : int)
    (count : int)
    (value : 'T)
    (builder : FlatList<'T>.Builder)
    =
    builder.LastIndexOf (value, startIndex, count, comparer)

// ---- Transformation ----

[<CompiledName "Add">]
let add (value : 'T) (builder : FlatList<'T>.Builder) =
    builder.Add value
    builder

[<CompiledName "AddRange">]
let addRange (values : seq<'T>) (builder : FlatList<'T>.Builder) =
    checkNotNullOrDefaultSeq (nameof values) values
    builder.AddRange values
    builder

[<CompiledName "Clear">]
let clear (builder : FlatList<'T>.Builder) =
    builder.Clear ()
    builder

[<CompiledName "InsertAt">]
let insertAt index (value : 'T) (builder : FlatList<'T>.Builder) =
    builder.Insert (index, value)
    builder

[<CompiledName "InsertManyAt">]
let insertManyAt index (values : seq<'T>) (builder : FlatList<'T>.Builder) =
    // The BCL checks the index before the values, so an index out of range wins over a missing sequence
    if index >= 0 && index <= builder.Count then
        checkNotNullOrDefaultSeq (nameof values) values

    builder.InsertRange (index, values)
    builder

[<CompiledName "Remove">]
let remove (value : 'T) (builder : FlatList<'T>.Builder) = builder.Remove value

[<CompiledName "RemoveAll">]
let removeAll (predicate : 'T -> bool) (builder : FlatList<'T>.Builder) =
    builder.RemoveAll (Predicate<'T> predicate)
    builder

[<CompiledName "RemoveAt">]
let removeAt index (builder : FlatList<'T>.Builder) =
    builder.RemoveAt index
    builder

[<CompiledName "RemoveMany">]
let removeMany (items : seq<'T>) (builder : FlatList<'T>.Builder) =
    checkNotNullOrDefaultSeq (nameof items) items
    builder.RemoveRange items
    builder

[<CompiledName "RemoveManyAt">]
let removeManyAt index count (builder : FlatList<'T>.Builder) =
    // The BCL calls the count length. The checks keep its order, its exception and its message
    if index < 0 || index > builder.Count then
        raise (ArgumentOutOfRangeException (nameof index))

    if count < 0 || index > builder.Count - count then
        raise (ArgumentOutOfRangeException (nameof count))

    builder.RemoveRange (index, count)
    builder

[<CompiledName "RemoveManyWith">]
let removeManyWith (comparer : IEqualityComparer<'T>) (items : seq<'T>) (builder : FlatList<'T>.Builder) =
    checkNotNullOrDefaultSeq (nameof items) items
    builder.RemoveRange (items, comparer)
    builder

[<CompiledName "RemoveWith">]
let removeWith (comparer : IEqualityComparer<'T>) (value : 'T) (builder : FlatList<'T>.Builder) = builder.Remove (value, comparer)

[<CompiledName "Replace">]
let replace (oldValue : 'T) (newValue : 'T) (builder : FlatList<'T>.Builder) =
    builder.Replace (oldValue, newValue)
    builder

[<CompiledName "ReplaceWith">]
let replaceWith (comparer : IEqualityComparer<'T>) (oldValue : 'T) (newValue : 'T) (builder : FlatList<'T>.Builder) =
    builder.Replace (oldValue, newValue, comparer)
    builder

[<CompiledName "Reverse">]
let rev (builder : FlatList<'T>.Builder) =
    builder.Reverse ()
    builder

[<CompiledName "SetCapacity">]
let setCapacity capacity (builder : FlatList<'T>.Builder) =
    builder.Capacity <- capacity
    builder

[<CompiledName "UpdateAt">]
let updateAt index (value : 'T) (builder : FlatList<'T>.Builder) =
    builder[index] <- value
    builder

// ---- Sorting ----

[<CompiledName "Sort">]
let sort (builder : FlatList<'T>.Builder) =
    builder.Sort DefaultComparer<'T>.Instance
    builder

[<CompiledName "SortRange">]
let sortRange index count (builder : FlatList<'T>.Builder) =
    builder.Sort (index, count, DefaultComparer<'T>.Instance)
    builder

[<CompiledName "SortRangeWith">]
let sortRangeWith (comparison : 'T -> 'T -> int) index count (builder : FlatList<'T>.Builder) =
    builder.Sort (index, count, Comparer<'T>.Create(Comparison<'T> comparison))
    builder

[<CompiledName "SortRangeWithComparer">]
let sortRangeWithComparer (comparer : IComparer<'T>) index count (builder : FlatList<'T>.Builder) =
    builder.Sort (index, count, comparer)
    builder

[<CompiledName "SortWith">]
let sortWith (comparer : 'T -> 'T -> int) (builder : FlatList<'T>.Builder) =
    builder.Sort (Comparison<'T> comparer)
    builder

[<CompiledName "SortWithComparer">]
let sortWithComparer (comparer : IComparer<'T>) (builder : FlatList<'T>.Builder) =
    builder.Sort comparer
    builder

// ---- Conversion ----

[<CompiledName "CopyTo">]
let blit (source : FlatList<'T>.Builder) sourceIndex (target : 'T array) targetIndex count =
    // The BCL calls the target destination, its index destinationIndex and the count length. The checks keep its
    // order, its exceptions and its messages
    match withNull target with
    | null -> nullArg (nameof target)
    | _ -> ()

    if count < 0 then
        raise (ArgumentOutOfRangeException (nameof count))

    if sourceIndex < 0 || sourceIndex > source.Count - count then
        raise (ArgumentOutOfRangeException (nameof sourceIndex))

    if targetIndex < 0 || targetIndex > target.Length - count then
        raise (ArgumentOutOfRangeException (nameof targetIndex))

    source.CopyTo (sourceIndex, target, targetIndex, count)

[<CompiledName "DrainToFlatList">]
let drainToFlatList (builder : FlatList<'T>.Builder) : FlatList<'T> = builder.DrainToImmutable ()

[<CompiledName "ToArray">]
let toArray (builder : FlatList<'T>.Builder) = builder.ToArray ()
