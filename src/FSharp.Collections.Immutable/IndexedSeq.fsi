namespace FSharp.Collections.Immutable

type IIndexedSeq<'T> = System.Collections.Generic.IReadOnlyList<'T>

[<RequireQualifiedAccess>]
module IndexedSeq =

    /// <summary>Returns the item at the specified index.</summary>
    val item : index : int -> seq : IIndexedSeq<'T> -> 'T

    /// <summary>Returns the number of items in the indexed sequence.</summary>
    val length : seq : IIndexedSeq<'T> -> int
