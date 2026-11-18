namespace FSharp.Collections.Immutable.Tests

/// <summary>
/// Long F# lists for the tests of the functions that give a sequence to a member of the BCL that builds a tree.
/// </summary>
/// <remarks>
/// <para>
/// An F# list is an <see cref="T:System.Collections.Generic.IReadOnlyList`1"/> whose indexer walks the list from its head,
/// and the members of <see cref="T:System.Collections.Immutable.ImmutableList`1"/> and of its builder that take a sequence
/// build a tree by indexing: for 40 000 elements they take about a second, and four times as long for each doubling. The
/// functions copy the list into an array first, which takes a few milliseconds for the 200 000 elements of the lists here.
/// The tests run with a timeout that is far above the time of the copy and a fraction of the time of the build by indexing.
/// </para>
/// <para>
/// The interface is covariant, so the BCL also indexes a list of strings that is given as a sequence of objects, which is
/// not a list of the element type of the function. Both kinds of lists are rows of the tests.
/// </para>
/// </remarks>
[<AbstractClass; Sealed>]
type LongLists =

    /// A list of the element type of the function that takes it.
    static member OfElementType = "a list of objects"

    /// A list of a type that is derived from the element type, as a sequence of the element type.
    static member OfDerivedType = "a list of strings as objects"

    /// The kinds of lists, as the rows of the tests.
    static member Kinds = seq {
        LongLists.OfElementType
        LongLists.OfDerivedType
    }

    /// The elements of the lists, in their order.
    static member Elements = Array.init 200_000 string

    /// A new list of a kind, as the sequence of objects that a function takes.
    static member Create (kind : string) : seq<obj> =
        if kind = LongLists.OfElementType then
            LongLists.Elements |> Seq.cast<obj> |> Seq.toList :> seq<obj>
        else
            // The list of strings itself, not a copy: the runtime gives it the type of the sequence by covariance
            LongLists.Elements |> List.ofArray |> box |> unbox<seq<obj>>
