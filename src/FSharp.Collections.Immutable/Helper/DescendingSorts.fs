/// <summary>
/// The descending sorts of the modules that mirror <see cref="T:Microsoft.FSharp.Collections.ListModule"/>: stable sorts
/// in the order of F# generic comparison, with the comparer that the type caches, and without the boxes of the inline
/// sorts of <see cref="T:Microsoft.FSharp.Collections.SeqModule"/> (ADR 0001, D5).
/// </summary>
/// <remarks>
/// <para>
/// <see cref="M:Microsoft.FSharp.Collections.SeqModule.SortDescending``1(System.Collections.Generic.IEnumerable{``0})"/> and
/// <see cref="M:Microsoft.FSharp.Collections.SeqModule.SortByDescending``2(Microsoft.FSharp.Core.FSharpFunc{``0,``1},System.Collections.Generic.IEnumerable{``0})"/>
/// are inline. Compiled into a generic function, they compare with the generic operator, which boxes the elements, or the
/// keys, of a value type in every comparison: at 10 000 integers, a sort allocates 7.5 MB, eleven times as much as a sort
/// with a comparison function.
/// </para>
/// <para>
/// The sorts here call
/// <see cref="M:Microsoft.FSharp.Collections.SeqModule.SortWith``1(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpFunc{``0,System.Int32}},System.Collections.Generic.IEnumerable{``0})"/>,
/// which is not inline and runs the stable sort of <see cref="T:Microsoft.FSharp.Collections.ListModule"/>, with the comparer that
/// <see cref="T:FSharp.Collections.Immutable.Comparers.DefaultComparer`1"/> caches for the type. The comparer compares
/// as the generic operator does, so the comparisons, their order and the exceptions that they throw are those of
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.SortDescending``1(Microsoft.FSharp.Collections.FSharpList{``0})"/>
/// and
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.SortByDescending``2(Microsoft.FSharp.Core.FSharpFunc{``0,``1},Microsoft.FSharp.Collections.FSharpList{``0})"/>
/// (ADR 0001, D6): a comparison or a projection that throws makes the sort throw an
/// <see cref="T:System.InvalidOperationException"/> that holds the exception.
/// </para>
/// </remarks>
module internal FSharp.Collections.Immutable.DescendingSorts

/// <summary>
/// The elements of a sequence in descending order of F# generic comparison. The sequence sorts the elements stably
/// when it is read.
/// </summary>
/// <param name="source">The elements, which the caller has checked for <see langword="null"/>.</param>
/// <returns>The elements, equal elements in their order.</returns>
/// <remarks>
/// <para>
/// The comparer compares the second element with the first, as
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.SortDescending``1(Microsoft.FSharp.Collections.FSharpList{``0})"/>
/// does. The sort allocates the arrays
/// of FSharp.Core and nothing per comparison, for a type that has a primitive comparer.
/// </para>
/// <para>
/// It exists instead of
/// <see cref="M:Microsoft.FSharp.Collections.SeqModule.SortDescending``1(System.Collections.Generic.IEnumerable{``0})"/>,
/// which boxes the elements of a value type in every comparison (see the
/// remarks of the module).
/// </para>
/// </remarks>
let sortDescending (source : seq<'T>) =
    let comparer = DefaultComparer<'T>.Instance
    // The annotations keep the arguments non-nullable: IComparer<'T>.Compare takes nullable ones, and the lambda
    // would get them, and the sequence of the result an element type that is nullable too
    Seq.sortWith (fun (left : 'T) (right : 'T) -> comparer.Compare (right, left)) source

/// <summary>
/// The elements of a sequence in descending order of a projection, in F# generic comparison. The sequence sorts the
/// elements stably when it is read.
/// </summary>
/// <param name="projection">The function that returns the key of an element.</param>
/// <param name="source">The elements, which the caller has checked for <see langword="null"/>.</param>
/// <returns>The elements, those with equal keys in their order.</returns>
/// <remarks>
/// <para>
/// The projection runs in the comparison, twice for each comparison and for the second element first, as it does in
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.SortByDescending``2(Microsoft.FSharp.Core.FSharpFunc{``0,``1},Microsoft.FSharp.Collections.FSharpList{``0})"/>;
/// the keys are not computed beforehand.
/// </para>
/// <para>
/// It exists instead of
/// <see cref="M:Microsoft.FSharp.Collections.SeqModule.SortByDescending``2(Microsoft.FSharp.Core.FSharpFunc{``0,``1},System.Collections.Generic.IEnumerable{``0})"/>,
/// which boxes the keys of a value type in every comparison (see the remarks
/// of the module).
/// </para>
/// </remarks>
let sortByDescending (projection : 'T -> 'Key) (source : seq<'T>) =
    let comparer = DefaultComparer<'Key>.Instance
    Seq.sortWith (fun (left : 'T) (right : 'T) -> comparer.Compare (projection right, projection left)) source
