/// <summary>
/// The loops over the elements of an <see cref="T:System.Collections.Immutable.ImmutableList`1"/>, which walk it with the
/// enumerator that is the faster one for the type of the elements (ADR 0001, D5).
/// </summary>
/// <remarks>
/// <para>
/// <c>ImmutableList&lt;T&gt;.GetEnumerator</c> returns a struct. For a value type that is the fastest way to walk the list: the
/// compiler knows the struct, and its calls are direct and inlined. For a reference type the code of a generic function is
/// shared by all reference types, and each call of a member of a generic struct in it needs a lookup of the exact type
/// at run time. Walking the list through <see cref="T:System.Collections.Generic.IEnumerable`1"/> instead, with a boxed
/// enumerator and calls through the interface, took 0.56 to 0.70 times as long for strings (6 against 10 ns for an
/// element) and 1.1 to 1.5 times as long for integers. As functions of the library, iter and fold took 0.6 to 0.7 times
/// as long for strings of 100 to 100 000 elements, tryPick 0.6, mapi and scan 0.7 to 0.9, and none of them changed for
/// integers. The loop of mapFold stays as it was, because the same loop took 1.1 to 1.4 times as long there.
/// </para>
/// <para>
/// The test <c>typeof&lt;'T&gt;.IsValueType</c> is a constant for the compiler of the run time, so each instantiation of a
/// function keeps one of the two loops. The functions are inline, and the body of a loop is passed as a lambda that the
/// compiler inlines into both loops, so that a body is written once. A body cannot capture a mutable local, which is why
/// <see cref="M:FSharp.Collections.Immutable.Walk.iteri``1"/> and
/// <see cref="M:FSharp.Collections.Immutable.Walk.fold``2"/> count and carry the state themselves.
/// </para>
/// </remarks>
module internal FSharp.Collections.Immutable.Walk

open System.Collections.Immutable

/// <summary>
/// Calls an action for each element of a list, in order.
/// </summary>
/// <param name="action">The action.</param>
/// <param name="list">The list, which is not <see langword="null"/>.</param>
let inline iter ([<InlineIfLambda>] action : 'T -> unit) (list : ImmutableList<'T>) =
    if typeof<'T>.IsValueType then
        for item in list do
            action item
    else
        for item in (list :> seq<'T>) do
            action item

/// <summary>
/// Calls an action for each element of a list, in order, with the index of the element.
/// </summary>
/// <param name="action">The action.</param>
/// <param name="list">The list, which is not <see langword="null"/>.</param>
let inline iteri ([<InlineIfLambda>] action : int -> 'T -> unit) (list : ImmutableList<'T>) =
    let mutable index = 0

    if typeof<'T>.IsValueType then
        for item in list do
            action index item
            index <- index + 1
    else
        for item in (list :> seq<'T>) do
            action index item
            index <- index + 1

/// <summary>
/// Threads a state through the elements of a list, in order.
/// </summary>
/// <param name="folder">The function that returns the next state for the state and the element.</param>
/// <param name="state">The first state.</param>
/// <param name="list">The list, which is not <see langword="null"/>.</param>
/// <returns>The last state.</returns>
let inline fold ([<InlineIfLambda>] folder : 'State -> 'T -> 'State) (state : 'State) (list : ImmutableList<'T>) =
    let mutable state = state

    if typeof<'T>.IsValueType then
        for item in list do
            state <- folder state item
    else
        for item in (list :> seq<'T>) do
            state <- folder state item

    state

/// <summary>
/// The first value that a chooser returns for the elements of a list, in order, without a call for the elements after it.
/// </summary>
/// <param name="chooser">The chooser.</param>
/// <param name="list">The list, which is not <see langword="null"/>.</param>
/// <returns>The first value, or <see langword="ValueNone"/>.</returns>
let inline tryPick ([<InlineIfLambda>] chooser : 'T -> 'U voption) (list : ImmutableList<'T>) =
    let mutable result = ValueNone

    if typeof<'T>.IsValueType then
        let mutable enumerator = list.GetEnumerator ()

        try
            while result.IsNone && enumerator.MoveNext () do
                result <- chooser enumerator.Current
        finally
            enumerator.Dispose ()
    else
        use enumerator = (list :> seq<'T>).GetEnumerator()

        while result.IsNone && enumerator.MoveNext () do
            result <- chooser enumerator.Current

    result
