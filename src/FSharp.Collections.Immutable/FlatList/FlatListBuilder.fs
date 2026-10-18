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

[<CompiledName "LastIndexOf">]
let lastIndexOf (value : 'T) (builder : FlatList<'T>.Builder) = builder.LastIndexOf value

// ---- Transformation ----

[<CompiledName "Add">]
let add (value : 'T) (builder : FlatList<'T>.Builder) =
    builder.Add value
    builder

[<CompiledName "AddRange">]
let addRange (values : seq<'T>) (builder : FlatList<'T>.Builder) =
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

[<CompiledName "Remove">]
let remove (value : 'T) (builder : FlatList<'T>.Builder) = builder.Remove value

[<CompiledName "RemoveAt">]
let removeAt index (builder : FlatList<'T>.Builder) =
    builder.RemoveAt index
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

[<CompiledName "SortWith">]
let sortWith (comparer : 'T -> 'T -> int) (builder : FlatList<'T>.Builder) =
    builder.Sort (Comparison<'T> comparer)
    builder

[<CompiledName "SortWithComparer">]
let sortWithComparer (comparer : IComparer<'T>) (builder : FlatList<'T>.Builder) =
    builder.Sort comparer
    builder

// ---- Conversion ----

[<CompiledName "DrainToFlatList">]
let drainToFlatList (builder : FlatList<'T>.Builder) : FlatList<'T> = builder.DrainToImmutable ()

[<CompiledName "ToArray">]
let toArray (builder : FlatList<'T>.Builder) = builder.ToArray ()
