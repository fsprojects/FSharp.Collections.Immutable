/// <summary>
/// The lengths of the queues and the stacks, <see cref="T:System.Collections.Immutable.IImmutableQueue`1"/> and
/// <see cref="T:System.Collections.Immutable.IImmutableStack`1"/>, which do not store their length, counted only as far as
/// a caller needs them (ADR 0001, D5 and D6).
/// </summary>
/// <remarks>
/// <para>
/// The checks of <see cref="T:FSharp.Collections.Immutable.ListChecks"/> take a length, but most of them only tell an empty
/// input from one with elements, or an input of one element from a longer one, and the sorts return an input of at most one
/// element unchanged. A count of a whole queue or stack would make each of them O(n).
/// </para>
/// <para>
/// The modules of the queues and the stacks pass a limit instead: 1 to tell an empty input from one with elements, 2 to
/// tell an input of one element from a longer one. The functions take the interfaces, which the concrete queue and stack
/// implement, so that one function serves a module and its interface module.
/// </para>
/// </remarks>
module internal FSharp.Collections.Immutable.Lengths

open System.Collections.Immutable

/// <summary>
/// Counts the elements of a collection up to a limit, by stepping from the collection to the collection without its first
/// element.
/// </summary>
/// <param name="isEmpty">Tells whether a collection has no element.</param>
/// <param name="rest">Returns the collection without its first element; the collection is not empty.</param>
/// <param name="limit">The largest number to count.</param>
/// <param name="collection">The collection to count.</param>
/// <returns>The smaller of the number of elements and the limit, and 0 for a limit that is not positive.</returns>
/// <remarks>
/// <para>
/// The step runs at most limit - 1 times, so a limit of 1 only reads whether the collection is empty.
/// </para>
/// <para>
/// The queue and the stack differ only in how they tell that no element is left and how they step, so the loop exists
/// once. It is inline and inlines its two functions, so that a count contains the calls of the interface members
/// themselves, not calls of closures.
/// </para>
/// </remarks>
let inline private countUpTo
    ([<InlineIfLambda>] isEmpty : 'Collection -> bool)
    ([<InlineIfLambda>] rest : 'Collection -> 'Collection)
    (limit : int)
    (collection : 'Collection)
    =
    let mutable remaining = collection
    let mutable count = 0

    while count < limit && not (isEmpty remaining) do
        count <- count + 1

        if count < limit then
            remaining <- rest remaining

    count

/// <summary>
/// The number of elements of a queue, counted up to a limit: the length, or the limit for a longer queue.
/// </summary>
/// <param name="limit">The largest number to count; a limit that is not positive gives 0.</param>
/// <param name="queue">The queue, which the caller has checked for <see langword="null"/>.</param>
/// <returns>The smaller of the length of the queue and the limit, and 0 for a limit that is not positive.</returns>
/// <remarks>
/// O(1) for a limit of 1, which only reads <see cref="P:System.Collections.Immutable.IImmutableQueue`1.IsEmpty"/>. A
/// larger limit calls <see cref="M:System.Collections.Immutable.IImmutableQueue`1.Dequeue"/> at most limit - 1 times.
/// Each call allocates a queue, and the call that takes the last element of the front of an
/// <see cref="T:System.Collections.Immutable.ImmutableQueue`1"/> reverses its back once, so the function suits small
/// limits.
/// </remarks>
let queueUpTo (limit : int) (queue : IImmutableQueue<'T>) =
    countUpTo (fun (queue : IImmutableQueue<'T>) -> queue.IsEmpty) (fun queue -> queue.Dequeue ()) limit queue

/// <summary>
/// The number of elements of a stack, counted up to a limit: the length, or the limit for a longer stack.
/// </summary>
/// <param name="limit">
/// The largest number to count; a limit that is not positive gives 0, and <see cref="F:System.Int32.MaxValue"/> counts
/// the whole stack.
/// </param>
/// <param name="stack">The stack, which the caller has checked for <see langword="null"/>.</param>
/// <returns>The smaller of the length of the stack and the limit, and 0 for a limit that is not positive.</returns>
/// <remarks>
/// O(min(n, limit)), without an allocation: <see cref="M:System.Collections.Immutable.IImmutableStack`1.Pop"/> returns
/// the stack below the top element, which exists already.
/// </remarks>
let stackUpTo (limit : int) (stack : IImmutableStack<'T>) =
    countUpTo (fun (stack : IImmutableStack<'T>) -> stack.IsEmpty) (fun stack -> stack.Pop ()) limit stack
