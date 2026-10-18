[<RequireQualifiedAccess>]
module FSharp.Collections.Immutable.IndexedListBuilder

open System
open System.Collections.Generic
open System.Collections.Immutable
open System.Linq

/// Replaces the elements of the builder with the same elements in the stable order of the comparer, as the sorting
/// functions of IndexedList order them; the Sort members of the builder are not stable.
let private sortStably (comparer : IComparer<'T>) (builder : IndexedList<'T>.Builder) =
    if builder.Count > 1 then
        let sorted = builder.Order(comparer).ToArray()
        builder.Clear ()
        builder.AddRange sorted

    builder

/// Replaces a range of the elements of the builder with the same elements in the stable order of the comparer, as
/// IndexedList.sortRangeWithComparer orders them; Sort(Int32, Int32, IComparer) of the builder is not stable.
let private sortRangeStably (comparer : IComparer<'T>) index count (builder : IndexedList<'T>.Builder) =
    ListRanges.forward (nameof index) index (nameof count) count (nameof count) builder.Count

    // A range of at most one element needs no sorting, and the comparer does not run for it
    if count > 1 then
        // The range is copied into an array, as IndexedList.sortRangeWithComparer copies it (see there for the reason)
        let items = Array.zeroCreate count
        builder.CopyTo (index, items, 0, count)
        let sorted = items.Order(comparer).ToArray()
        builder.RemoveRange (index, count)
        builder.InsertRange (index, sorted)

    builder

// ---- Properties ----

[<CompiledName "Length">]
let length (builder : IndexedList<'T>.Builder) = builder.Count

// ---- Access ----

[<CompiledName "Item">]
let item index (builder : IndexedList<'T>.Builder) = builder[index]

// ---- Search ----

[<CompiledName "BinarySearch">]
let binarySearch (value : 'T) (builder : IndexedList<'T>.Builder) = builder.BinarySearch (value, DefaultComparer<'T>.Instance)

[<CompiledName "BinarySearchRangeWithComparer">]
let binarySearchRangeWithComparer (comparer : IComparer<'T>) index count (value : 'T) (builder : IndexedList<'T>.Builder) =
    ListRanges.search (nameof index) index (nameof count) count builder.Count
    builder.BinarySearch (index, count, value, comparer)

[<CompiledName "BinarySearchWithComparer">]
let binarySearchWithComparer (comparer : IComparer<'T>) (value : 'T) (builder : IndexedList<'T>.Builder) =
    builder.BinarySearch (value, comparer)

[<CompiledName "Contains">]
let contains (value : 'T) (builder : IndexedList<'T>.Builder) = builder.Contains value

[<CompiledName "IndexOf">]
let indexOf (value : 'T) (builder : IndexedList<'T>.Builder) = builder.IndexOf value

[<CompiledName "IndexOfRange">]
let indexOfRange startIndex count (value : 'T) (builder : IndexedList<'T>.Builder) =
    ListRanges.forward (nameof startIndex) startIndex (nameof count) count (nameof count) builder.Count
    builder.IndexOf (value, startIndex, count, EqualityComparer<'T>.Default)

[<CompiledName "IndexOfRangeWith">]
let indexOfRangeWith (comparer : IEqualityComparer<'T>) startIndex count (value : 'T) (builder : IndexedList<'T>.Builder) =
    ListRanges.forward (nameof startIndex) startIndex (nameof count) count (nameof count) builder.Count
    builder.IndexOf (value, startIndex, count, comparer)

[<CompiledName "LastIndexOf">]
let lastIndexOf (value : 'T) (builder : IndexedList<'T>.Builder) = builder.LastIndexOf value

[<CompiledName "LastIndexOfRange">]
let lastIndexOfRange startIndex count (value : 'T) (builder : IndexedList<'T>.Builder) =
    ListRanges.backward (nameof startIndex) startIndex (nameof count) count builder.Count

    // An empty range has no equal element; the member rejects a start index of Int32.MaxValue for it
    if count = 0 then
        -1
    else
        builder.LastIndexOf (value, startIndex, count, EqualityComparer<'T>.Default)

[<CompiledName "LastIndexOfRangeWith">]
let lastIndexOfRangeWith (comparer : IEqualityComparer<'T>) startIndex count (value : 'T) (builder : IndexedList<'T>.Builder) =
    ListRanges.backward (nameof startIndex) startIndex (nameof count) count builder.Count

    if count = 0 then
        -1
    else
        builder.LastIndexOf (value, startIndex, count, comparer)

// ---- Transformation ----

[<CompiledName "Add">]
let add (value : 'T) (builder : IndexedList<'T>.Builder) =
    builder.Add value
    builder

[<CompiledName "AddRange">]
let addRange (values : seq<'T>) (builder : IndexedList<'T>.Builder) =
    // The member would name a null sequence items, and it fails while it enumerates a default flat list (ADR 0001, D6)
    checkNotNullOrDefaultSeq (nameof values) values
    builder.AddRange values
    builder

[<CompiledName "Clear">]
let clear (builder : IndexedList<'T>.Builder) =
    builder.Clear ()
    builder

[<CompiledName "InsertAt">]
let insertAt index (value : 'T) (builder : IndexedList<'T>.Builder) =
    builder.Insert (index, value)
    builder

[<CompiledName "InsertManyAt">]
let insertManyAt index (values : seq<'T>) (builder : IndexedList<'T>.Builder) =
    // The member checks the index before the values, so an index out of range wins over a missing sequence
    if index >= 0 && index <= builder.Count then
        checkNotNullOrDefaultSeq (nameof values) values

    builder.InsertRange (index, values)
    builder

[<CompiledName "Remove">]
let remove (value : 'T) (builder : IndexedList<'T>.Builder) = builder.Remove value

[<CompiledName "RemoveAll">]
let removeAll (predicate : 'T -> bool) (builder : IndexedList<'T>.Builder) =
    builder.RemoveAll (Predicate predicate) |> ignore
    builder

[<CompiledName "RemoveAt">]
let removeAt index (builder : IndexedList<'T>.Builder) =
    builder.RemoveAt index
    builder

[<CompiledName "RemoveMany">]
let removeMany (items : seq<'T>) (builder : IndexedList<'T>.Builder) =
    // The member names null items as this function does, but it fails while it enumerates a default flat list
    checkNotNullOrDefaultSeq (nameof items) items
    builder.RemoveRange items
    builder

[<CompiledName "RemoveManyAt">]
let removeManyAt (index : int) (count : int) (builder : IndexedList<'T>.Builder) =
    builder.RemoveRange (index, count)
    builder

[<CompiledName "RemoveManyWith">]
let removeManyWith (comparer : IEqualityComparer<'T>) (items : seq<'T>) (builder : IndexedList<'T>.Builder) =
    checkNotNullOrDefaultSeq (nameof items) items
    builder.RemoveRange (items, comparer)
    builder

[<CompiledName "RemoveWith">]
let removeWith (comparer : IEqualityComparer<'T>) (value : 'T) (builder : IndexedList<'T>.Builder) =
    builder.Remove (value, comparer)

[<CompiledName "Replace">]
let replace (oldValue : 'T) (newValue : 'T) (builder : IndexedList<'T>.Builder) =
    builder.Replace (oldValue, newValue)
    builder

[<CompiledName "ReplaceWith">]
let replaceWith (comparer : IEqualityComparer<'T>) (oldValue : 'T) (newValue : 'T) (builder : IndexedList<'T>.Builder) =
    builder.Replace (oldValue, newValue, comparer)
    builder

[<CompiledName "Reverse">]
let rev (builder : IndexedList<'T>.Builder) =
    builder.Reverse ()
    builder

[<CompiledName "ReverseRange">]
let revRange index count (builder : IndexedList<'T>.Builder) =
    // ImmutableList<T>.Builder.Reverse(Int32, Int32) names the count for a range beyond the end
    ListRanges.forward (nameof index) index (nameof count) count (nameof count) builder.Count
    builder.Reverse (index, count)
    builder

[<CompiledName "UpdateAt">]
let updateAt index (value : 'T) (builder : IndexedList<'T>.Builder) =
    builder[index] <- value
    builder

// ---- Sorting ----

[<CompiledName "Sort">]
let sort (builder : IndexedList<'T>.Builder) = sortStably DefaultComparer<'T>.Instance builder

[<CompiledName "SortRange">]
let sortRange index count (builder : IndexedList<'T>.Builder) = sortRangeStably DefaultComparer<'T>.Instance index count builder

[<CompiledName "SortRangeWith">]
let sortRangeWith (comparison : 'T -> 'T -> int) index count (builder : IndexedList<'T>.Builder) =
    sortRangeStably (ComparisonIdentity.FromFunction comparison) index count builder

[<CompiledName "SortRangeWithComparer">]
let sortRangeWithComparer (comparer : IComparer<'T>) index count (builder : IndexedList<'T>.Builder) =
    sortRangeStably comparer index count builder

[<CompiledName "SortWith">]
let sortWith (comparer : 'T -> 'T -> int) (builder : IndexedList<'T>.Builder) =
    sortStably (ComparisonIdentity.FromFunction comparer) builder

[<CompiledName "SortWithComparer">]
let sortWithComparer (comparer : IComparer<'T>) (builder : IndexedList<'T>.Builder) = sortStably comparer builder

// ---- Conversion ----

[<CompiledName "CopyTo">]
let blit (source : IndexedList<'T>.Builder) sourceIndex (target : 'T array) targetIndex count =
    checkNotNull (nameof target) target
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
let toArray (builder : IndexedList<'T>.Builder) =
    let array = Array.zeroCreate builder.Count
    builder.CopyTo array
    array
