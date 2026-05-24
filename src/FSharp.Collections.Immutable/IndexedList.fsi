namespace FSharp.Collections.Immutable

type IIndexedList<'T> = System.Collections.Immutable.IImmutableList<'T>

type IndexedList<'T> = System.Collections.Immutable.ImmutableList<'T>

/// <summary>Functional helpers for indexed immutable lists.</summary>
[<RequireQualifiedAccess; CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module IndexedList =

    /// <summary>Returns an empty indexed list.</summary>
    val empty<'T> : IndexedList<'T>

    /// <summary>Creates an indexed list from a sequence.</summary>
    val ofSeq : source : seq<'T> -> IndexedList<'T>

    /// <summary>Returns a sequence view of an indexed list.</summary>
    val toSeq : list : IndexedList<'T> -> seq<'T>

    /// <summary>Returns the number of items in the indexed list.</summary>
    val length : list : IIndexedList<'T> -> int

    /// <summary>Returns the first item in the indexed list.</summary>
    val head : list : IIndexedList<'T> -> 'T
