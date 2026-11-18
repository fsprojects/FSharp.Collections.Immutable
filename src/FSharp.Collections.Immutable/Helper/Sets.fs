/// <summary>
/// The algorithms that the set modules share, whatever the type of the set (ADR 0001, D5).
/// </summary>
/// <remarks>
/// The functions of the FSharp.Core <see cref="T:Microsoft.FSharp.Collections.SetModule"/> module take F# sets, and the sets
/// of this library are sets of the BCL. An algorithm here works on any type of set that its caller combines, so that the
/// modules of the hash sets and of their interface share it.
/// </remarks>
module internal FSharp.Collections.Immutable.Sets

/// <summary>
/// Combines the sets of a sequence from the first one to the last one, as
/// <see cref="M:Microsoft.FSharp.Collections.SeqModule.Reduce``1(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpFunc{``0,``0}},System.Collections.Generic.IEnumerable{``0})"/>
/// does, or returns <c>ValueNone</c> for
/// an empty sequence.
/// </summary>
/// <param name="checkSet">
/// The check that the function runs for each set before it uses it: the caller passes the null check that names the
/// parameter of the caller.
/// </param>
/// <param name="combine">The function that combines the result so far with the next set: the union or the intersection.</param>
/// <param name="sets">The sets, which the caller has checked for <see langword="null"/>.</param>
/// <returns>The combined set, or <c>ValueNone</c> when the sequence has no set.</returns>
/// <remarks>
/// <para>
/// This is an O(k) operation besides the k - 1 calls of <paramref name="combine"/>, where k is the number of sets in the
/// sequence. The function enumerates the sequence once, without a copy.
/// </para>
/// <para>
/// The result keeps the comparer and the implementation of the first set when <paramref name="combine"/> keeps those of its
/// first argument, as the set algebra of the BCL does.
/// <see cref="M:Microsoft.FSharp.Collections.SetModule.UnionMany``1(System.Collections.Generic.IEnumerable{Microsoft.FSharp.Collections.FSharpSet{``0}})"/>
/// folds from the empty set; starting from the first set
/// instead keeps the comparer of that set.
/// </para>
/// <para>
/// It exists because
/// <see cref="M:Microsoft.FSharp.Collections.SetModule.UnionMany``1(System.Collections.Generic.IEnumerable{Microsoft.FSharp.Collections.FSharpSet{``0}})"/> and
/// <see cref="M:Microsoft.FSharp.Collections.SetModule.IntersectMany``1(System.Collections.Generic.IEnumerable{Microsoft.FSharp.Collections.FSharpSet{``0}})"/>
/// take F# sets, and because their results for an empty sequence
/// differ: the union of no sets is an empty set, and
/// <see cref="M:Microsoft.FSharp.Collections.SetModule.IntersectMany``1(System.Collections.Generic.IEnumerable{Microsoft.FSharp.Collections.FSharpSet{``0}})"/>
/// requires a sequence that is not empty. The function
/// returns a value option so that each caller decides what an empty sequence means.
/// </para>
/// </remarks>
/// <seealso cref="M:FSharp.Collections.Immutable.HashSet.UnionMany``1(System.Collections.Generic.IEnumerable{System.Collections.Immutable.ImmutableHashSet{``0}})"/>
/// <seealso cref="M:FSharp.Collections.Immutable.HashSet.IntersectMany``1(System.Collections.Generic.IEnumerable{System.Collections.Immutable.ImmutableHashSet{``0}})"/>
/// <seealso cref="M:FSharp.Collections.Immutable.ISet.UnionMany``1(System.Collections.Generic.IEnumerable{System.Collections.Immutable.IImmutableSet{``0}})"/>
/// <seealso cref="M:FSharp.Collections.Immutable.ISet.IntersectMany``1(System.Collections.Generic.IEnumerable{System.Collections.Immutable.IImmutableSet{``0}})"/>
let inline reduce
    ([<InlineIfLambda>] checkSet : 'Set -> unit)
    ([<InlineIfLambda>] combine : 'Set -> 'Set -> 'Set)
    (sets : seq<'Set>)
    : 'Set voption =
    use enumerator = sets.GetEnumerator ()

    if enumerator.MoveNext () then
        let mutable result = enumerator.Current
        checkSet result

        while enumerator.MoveNext () do
            let next = enumerator.Current
            checkSet next
            result <- combine result next

        ValueSome result
    else
        ValueNone
