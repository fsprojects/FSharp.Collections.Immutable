/// <summary>
/// The algorithms that the set modules share, whatever the type of the set (ADR 0001, D5).
/// </summary>
module internal FSharp.Collections.Immutable.Sets

/// <summary>
/// Combines the sets of a sequence from the first one to the last one, as <c>Seq.reduce</c> does, or returns
/// <c>ValueNone</c> for an empty sequence.
/// </summary>
/// <remarks>
/// The caller checks the sequence and passes the check of each set, so that a <see langword="null"/> set throws with the
/// name of the parameter of the caller. The result keeps the comparer and the implementation of the first set when
/// <paramref name="combine"/> keeps those of its first argument, as the set algebra of the BCL does.
/// </remarks>
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
