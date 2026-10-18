/// <summary>
/// The accumulators that build an <see cref="T:System.Collections.Immutable.ImmutableList`1"/> from many collections, for
/// the functions that concatenate them (ADR 0001, D5).
/// </summary>
/// <remarks>
/// <para>
/// An indexed list is a balanced tree. Appending another indexed list to a builder joins the tree of the appended list to
/// the tree of the builder: it takes O(log n) and allocates a few nodes, whatever the length of the appended list, and the
/// result shares the nodes of the appended list, as any immutable tree shares its nodes. Appending any other collection
/// builds a subtree from its elements.
/// </para>
/// <para>
/// The join costs about a microsecond, so it pays only for an indexed list of some length. The functions of this module
/// measure how long: with 100 000 elements in all and fresh collections of 1, 3, 10, 30, 100 and 1000 elements, copying
/// the elements into a buffer and building the tree once, as
/// <see cref="M:System.Collections.Immutable.ImmutableList.CreateRange``1(System.Collections.Generic.IEnumerable{``0})"/>
/// does, is 1.2 to 1.8 times as fast as joining for collections of 1, 3 and 10 elements, and joining is 1.4 times as fast
/// for 30 and 2.4 times for 100 and 1000. Concatenating 100 indexed lists of 1000 elements takes 0.1 to 0.4 ms with joins
/// and 3 to 7 ms with copies, and allocates 76 KB instead of 5.8 MB; one indexed list of 100 000 elements takes 4 µs
/// instead of 3 to 7 ms.
/// </para>
/// </remarks>
module internal FSharp.Collections.Immutable.Collectors

open System.Collections.Immutable

/// <summary>
/// The least length of an indexed list that the accumulator joins to its result instead of copying it.
/// </summary>
/// <remarks>
/// The break-even of the measurements in the remarks of the module lies between 10 and 30 elements.
/// </remarks>
[<Literal>]
let private JoinedLength = 16

/// <summary>
/// Appends collections to an indexed list under construction: it copies the short ones, and joins the long indexed lists as
/// they are.
/// </summary>
/// <typeparam name="T">The type of the elements.</typeparam>
/// <remarks>
/// <para>
/// The copied elements wait in a buffer until a long indexed list comes or the result is asked for, and then the builder
/// takes them in one piece, so that a run of short collections costs what
/// <see cref="M:System.Collections.Immutable.ImmutableList.CreateRange``1(System.Collections.Generic.IEnumerable{``0})"/>
/// costs, and a long indexed list costs a join. The elements keep the order in which the collections were added.
/// </para>
/// <para>
/// It exists because a builder alone is 1.6 to 1.8 times slower than the buffer for collections of one element, and the buffer
/// alone copies every element of a long indexed list that the join would have shared (see the remarks of
/// <see cref="T:FSharp.Collections.Immutable.Collectors"/>). Functions that add the elements one at a time, such as
/// <see cref="M:FSharp.Collections.Immutable.IndexedList.Choose``2(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpValueOption{``1}},System.Collections.Immutable.ImmutableList{``0})"/>,
/// need neither: the buffer is 2.3 times as fast as
/// <see cref="M:System.Collections.Immutable.ImmutableList`1.Builder.Add(`0)"/> for 100 000 elements.
/// </para>
/// </remarks>
[<Sealed>]
type ListCollector<'T> () =

    let builder = ImmutableList.CreateBuilder<'T>()
    let pending = ResizeArray<'T>()

    /// Moves the copied elements into the builder.
    member private _.Flush () =
        if pending.Count > 0 then
            builder.AddRange pending
            pending.Clear ()

    /// <summary>
    /// Appends the elements of a collection to the result.
    /// </summary>
    /// <param name="items">The collection, which can be an indexed list.</param>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="items"/> is <see langword="null"/>.
    /// </exception>
    /// <remarks>
    /// An indexed list of at least 16 elements is joined to the result, which then shares its nodes; any other collection
    /// is copied.
    /// </remarks>
    member this.Add (items : seq<'T>) =
        match items with
        | :? ImmutableList<'T> as list when list.Count >= JoinedLength ->
            this.Flush ()
            builder.AddRange list
        | _ -> pending.AddRange items

    /// <summary>
    /// The indexed list of the elements that were added.
    /// </summary>
    /// <returns>The elements of the collections, in the order in which the collections were added.</returns>
    member this.ToImmutable () =
        this.Flush ()
        builder.ToImmutable ()
