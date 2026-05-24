namespace FSharp.Collections.Immutable

type IIndexedList<'T> = System.Collections.Immutable.IImmutableList<'T>

type IndexedList<'T> = System.Collections.Immutable.ImmutableList<'T>

[<RequireQualifiedAccess; CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module IndexedList =

    let empty<'T> : IndexedList<'T> = System.Collections.Immutable.ImmutableList.Create<'T> ()
    let ofSeq source : IndexedList<'T> = System.Collections.Immutable.ImmutableList.CreateRange source
    let toSeq (list : IndexedList<'T>) = list :> seq<'T>
    let length (list : IIndexedList<'T>) = list.Count
    let head (list : IIndexedList<'T>) = list.[0]
