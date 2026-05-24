namespace FSharp.Collections.Immutable

type IIndexedList<'T> = System.Collections.Immutable.IImmutableList<'T>

type IndexedList<'T> = System.Collections.Immutable.ImmutableList<'T>

[<RequireQualifiedAccess;
  CompiledName((nameof System.Collections.Immutable.ImmutableList)
               + "Module")>]
module IndexedList =

    let inline empty<'T> : IndexedList<'T> = System.Collections.Immutable.ImmutableList.Create ()
    let inline ofSeq source = System.Collections.Immutable.ImmutableList.CreateRange source
    let inline toSeq (list : IndexedList<'T>) = list :> seq<'T>
    let length (list : IIndexedList<'T>) = list.Count
    let head (list : IIndexedList<'T>) = list.[0]
