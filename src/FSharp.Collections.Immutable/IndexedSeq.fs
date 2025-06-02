namespace FSharp.Collections.Immutable

type IIndexedSeq<'T> = System.Collections.Generic.IReadOnlyList<'T>

module IndexedSeq =

    let check (seq : IIndexedSeq<_>) = checkNotNull (nameof seq) seq

    let item index seq =
        check seq
        seq.[index]

    let length seq =
        check seq
        seq.Count

module ReadOnlyList = IndexedSeq
