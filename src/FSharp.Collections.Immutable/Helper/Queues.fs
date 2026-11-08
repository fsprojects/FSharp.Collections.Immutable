/// <summary>
/// The helpers that the queue modules share (ADR 0001, D5): the queues of a fresh array and of a buffer, and the walks that
/// read the elements of a queue only up to the ones that a function of <see cref="T:FSharp.Collections.Immutable.Queue"/>
/// or <see cref="T:FSharp.Collections.Immutable.IQueue"/> needs.
/// </summary>
/// <remarks>
/// <para>
/// The walks take the interface, which both modules pass, and the caller checks the queue for <see langword="null"/>. They
/// enumerate an <see cref="T:System.Collections.Immutable.ImmutableQueue`1"/> with its struct enumerator, which allocates
/// nothing, also when a function of <see cref="T:FSharp.Collections.Immutable.IQueue"/> receives it, and any other queue
/// with its own enumerator, which they dispose.
/// </para>
/// <para>
/// Each walk is one generic loop over an enumerator type that implements
/// <see cref="T:System.Collections.Generic.IEnumerator`1"/>. The loop runs with the struct
/// <see cref="T:FSharp.Collections.Immutable.Queues.ImmutableQueueEnumerator`1"/> or with the enumerator of the other queue.
/// The JIT compiles generic code for each struct type argument and calls its members without boxing it, so the struct
/// enumerator of the queue advances in place. Member constraints of an inline loop would need the struct to be public,
/// because they resolve only to members that are accessible from everywhere.
/// </para>
/// <para>
/// The functions of the FSharp.Core <see cref="T:Microsoft.FSharp.Collections.ListModule"/> module, which the queue modules
/// mirror, take a list. A queue is not a list, and the functions of <see cref="T:Microsoft.FSharp.Collections.SeqModule"/>
/// that could stand in for them walk the queue through the interface <see cref="T:System.Collections.Generic.IEnumerator`1"/>
/// and allocate its enumerator on the heap. The summary of each function here says what it returns; the remarks say why it
/// exists.
/// </para>
/// </remarks>
module internal FSharp.Collections.Immutable.Queues

open System
open System.Collections.Generic
open System.Collections.Immutable
open System.Runtime.InteropServices

/// <summary>
/// The struct enumerator of an <see cref="T:System.Collections.Immutable.ImmutableQueue`1"/> as an
/// <see cref="T:System.Collections.Generic.IEnumerator`1"/>, for the loops below, which take it as a type argument.
/// </summary>
/// <typeparam name="T">The type of the elements of the queue.</typeparam>
/// <remarks>
/// <para>
/// The struct enumerator of the queue, <see cref="T:System.Collections.Immutable.ImmutableQueue`1.Enumerator"/>, implements
/// no interface, so a loop that is generic over <see cref="T:System.Collections.Generic.IEnumerator`1"/> cannot take it. This
/// struct holds it and implements the interface, so the loop is compiled for the struct and calls the members of the queue
/// enumerator directly, without boxing it.
/// </para>
/// <para>
/// The loops never reset an enumerator, and the struct enumerator of the queue cannot be reset, so
/// <see cref="M:System.Collections.IEnumerator.Reset"/> throws <see cref="T:System.NotSupportedException"/>.
/// </para>
/// </remarks>
[<Struct; NoComparison; NoEquality>]
type ImmutableQueueEnumerator<'T> =
    val mutable private enumerator : ImmutableQueue<'T>.Enumerator
    new (queue : ImmutableQueue<'T>) = { enumerator = queue.GetEnumerator () }

    interface IEnumerator<'T> with
        member this.Current = this.enumerator.Current

    interface Collections.IEnumerator with
        member this.Current = box this.enumerator.Current
        member this.MoveNext () = this.enumerator.MoveNext ()
        // The walks never reset an enumerator, and the struct enumerator of the queue cannot be reset
        member _.Reset () = raise (NotSupportedException ())

    interface IDisposable with
        member _.Dispose () = ()

/// <summary>
/// Runs a loop over the struct enumerator of an <see cref="T:System.Collections.Immutable.ImmutableQueue`1"/>, or over the
/// enumerator of another queue, which it disposes.
/// </summary>
/// <param name="immutableQueueLoop">The loop for the struct enumerator of an immutable queue.</param>
/// <param name="otherQueueLoop">The loop for the enumerator of any other queue.</param>
/// <param name="queue">The queue, which the caller has checked for <see langword="null"/>.</param>
/// <returns>The result of the loop that ran.</returns>
/// <remarks>
/// The callers pass the same generic loop twice, once for each type of enumerator, so that the JIT compiles the loop for the
/// struct and for the interface.
/// </remarks>
let inline private walk
    ([<InlineIfLambda>] immutableQueueLoop : ImmutableQueueEnumerator<'T> -> 'Result)
    ([<InlineIfLambda>] otherQueueLoop : IEnumerator<'T> -> 'Result)
    (queue : IImmutableQueue<'T>)
    =
    match queue with
    | :? ImmutableQueue<'T> as queue -> immutableQueueLoop (new ImmutableQueueEnumerator<'T> (queue))
    | _ ->
        use enumerator = queue.GetEnumerator ()
        otherQueueLoop enumerator

/// The loop of firstMatch.
let private firstMatchLoop (predicate : 'T -> bool) (enumerator : 'Enumerator :> IEnumerator<'T>) =
    // A struct enumerator advances only as a mutable value; a call on the parameter would advance a copy
    let mutable enumerator = enumerator
    let mutable result = ValueNone
    let mutable index = 0

    while result.IsNone && enumerator.MoveNext () do
        let item = enumerator.Current

        if predicate item then
            result <- ValueSome (struct (index, item))

        index <- index + 1

    result

/// The loop of firstChoice.
let private firstChoiceLoop (chooser : 'T -> 'U voption) (enumerator : 'Enumerator :> IEnumerator<'T>) =
    let mutable enumerator = enumerator
    let mutable result = ValueNone

    while result.IsNone && enumerator.MoveNext () do
        result <- chooser enumerator.Current

    result

/// The loop of front.
let private frontLoop (count : int) (enumerator : 'Enumerator :> IEnumerator<'T>) =
    let mutable enumerator = enumerator
    let items = ResizeArray<'T>()
    let mutable more = enumerator.MoveNext ()

    while more && items.Count < count do
        items.Add enumerator.Current
        more <- enumerator.MoveNext ()

    struct (items, more)

/// <summary>
/// A queue of the elements of a fresh array, without the null check of
/// <see cref="M:FSharp.Collections.Immutable.Queue.OfArray``1(``0[])"/>.
/// </summary>
/// <param name="array">The elements; neither checked for <see langword="null"/> nor kept.</param>
/// <returns>A queue of the elements of the array, in the order of the array.</returns>
/// <remarks>
/// <para>
/// This is an O(n) operation, where n is the length of the array.
/// </para>
/// <para>
/// <see cref="M:System.Collections.Immutable.ImmutableQueue.Create``1(System.ReadOnlySpan{``0})"/> copies the elements
/// into the front stack of the queue, from the last one to the first one, so that the queue dequeues them without
/// reversing a back stack first.
/// </para>
/// <para>
/// It exists because the functions of the queue modules compute their results in arrays that they have just made, and build
/// the queue from the array.
/// <see cref="M:FSharp.Collections.Immutable.Queue.OfArray``1(``0[])"/>
/// checks its argument for <see langword="null"/> because it is public, and
/// this function leaves out the check that such an array does not need. It is inline, so the call costs nothing.
/// </para>
/// </remarks>
let inline ofFreshArray (array : 'T array) = ImmutableQueue.Create<'T>(ReadOnlySpan<'T> array)

/// <summary>
/// A queue of the elements of a buffer that nobody changes any more, built as
/// <see cref="M:FSharp.Collections.Immutable.Queues.ofFreshArray``1(``0[])"/> builds it, without a
/// copy of the buffer.
/// </summary>
/// <param name="buffer">The buffer; neither checked for <see langword="null"/> nor kept.</param>
/// <returns>A queue of the elements of the buffer, in their order.</returns>
/// <remarks>
/// <para>
/// This is an O(n) operation, where n is the number of elements of the buffer.
/// </para>
/// <para>
/// It exists because the functions that cannot tell the length of their result in advance, such as the ones that choose or
/// that concatenate, collect it in a buffer.
/// <see cref="M:System.Runtime.InteropServices.CollectionsMarshal.AsSpan``1(System.Collections.Generic.List{``0})"/> gives
/// the elements of the buffer as a span, so that the queue reads them in place and the buffer is not copied first.
/// </para>
/// </remarks>
let ofBuffer (buffer : ResizeArray<'T>) =
    ImmutableQueue.Create<'T>(Span<'T>.op_Implicit(CollectionsMarshal.AsSpan buffer))

/// <summary>
/// The index and the first element that satisfies the predicate, or <c>ValueNone</c>.
/// </summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="queue">The queue, which the caller has checked for <see langword="null"/>.</param>
/// <returns>
/// The index and the first element for which the predicate returns true, or <c>ValueNone</c> if the predicate evaluates to
/// false for each element.
/// </returns>
/// <remarks>
/// <para>
/// This is an O(n) operation in the worst case, where n is the length of the queue. The walk stops at the first element
/// that satisfies the predicate.
/// </para>
/// <para>
/// It exists because four searches of the queue modules,
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.Find``1(Microsoft.FSharp.Core.FSharpFunc{``0,System.Boolean},Microsoft.FSharp.Collections.FSharpList{``0})"/>,
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.FindIndex``1(Microsoft.FSharp.Core.FSharpFunc{``0,System.Boolean},Microsoft.FSharp.Collections.FSharpList{``0})"/>,
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.TryFind``1(Microsoft.FSharp.Core.FSharpFunc{``0,System.Boolean},Microsoft.FSharp.Collections.FSharpList{``0})"/> and
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.TryFindIndex``1(Microsoft.FSharp.Core.FSharpFunc{``0,System.Boolean},Microsoft.FSharp.Collections.FSharpList{``0})"/>,
/// differ only in what they return of the first match, its element or its index, and read nothing after it. One walk gives
/// both, and a copy of the queue into a list or an array would read all of it.
/// </para>
/// </remarks>
/// <seealso cref="M:FSharp.Collections.Immutable.Queue.TryFind``1(Microsoft.FSharp.Core.FSharpFunc{``0,System.Boolean},System.Collections.Immutable.ImmutableQueue{``0})"/>
/// <seealso cref="M:FSharp.Collections.Immutable.Queue.TryFindIndex``1(Microsoft.FSharp.Core.FSharpFunc{``0,System.Boolean},System.Collections.Immutable.ImmutableQueue{``0})"/>
/// <seealso cref="M:FSharp.Collections.Immutable.IQueue.TryFind``1(Microsoft.FSharp.Core.FSharpFunc{``0,System.Boolean},System.Collections.Immutable.IImmutableQueue{``0})"/>
/// <seealso cref="M:FSharp.Collections.Immutable.IQueue.TryFindIndex``1(Microsoft.FSharp.Core.FSharpFunc{``0,System.Boolean},System.Collections.Immutable.IImmutableQueue{``0})"/>
let firstMatch (predicate : 'T -> bool) (queue : IImmutableQueue<'T>) =
    walk (firstMatchLoop predicate) (firstMatchLoop predicate) queue

/// <summary>
/// The first value that the chooser returns for the elements, in their order, or <c>ValueNone</c>.
/// </summary>
/// <param name="chooser">The function to transform the elements into value options.</param>
/// <param name="queue">The queue, which the caller has checked for <see langword="null"/>.</param>
/// <returns>The first result that is a value, or <c>ValueNone</c> if the chooser returns <c>ValueNone</c> for each element.</returns>
/// <remarks>
/// <para>
/// This is an O(n) operation in the worst case, where n is the length of the queue. The walk stops at the first value.
/// </para>
/// <para>
/// It exists because
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.Pick``2(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpOption{``1}},Microsoft.FSharp.Collections.FSharpList{``0})"/> and
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.TryPick``2(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpOption{``1}},Microsoft.FSharp.Collections.FSharpList{``0})"/>
/// take a list and a function that returns an option, where the functions of
/// the queue modules take a queue and a function that returns a value option (ADR 0001, D3). The walk serves both
/// functions in both modules.
/// </para>
/// </remarks>
/// <seealso cref="M:FSharp.Collections.Immutable.Queue.TryPick``2(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpValueOption{``1}},System.Collections.Immutable.ImmutableQueue{``0})"/>
/// <seealso cref="M:FSharp.Collections.Immutable.IQueue.TryPick``2(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpValueOption{``1}},System.Collections.Immutable.IImmutableQueue{``0})"/>
let firstChoice (chooser : 'T -> 'U voption) (queue : IImmutableQueue<'T>) =
    walk (firstChoiceLoop chooser) (firstChoiceLoop chooser) queue

/// <summary>
/// The first elements of a queue, at most the count, and whether the queue has more elements.
/// </summary>
/// <param name="count">The largest number of elements to take.</param>
/// <param name="queue">The queue, which the caller has checked for <see langword="null"/>.</param>
/// <returns>
/// A struct tuple of the elements in a new buffer, in their order, and whether the queue has an element after them.
/// </returns>
/// <remarks>
/// <para>
/// This is an O(min(n, count)) operation, where n is the length of the queue: the walk stops after the count.
/// </para>
/// <para>
/// It exists because
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.Take``1(System.Int32,Microsoft.FSharp.Collections.FSharpList{``0})"/> and
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.Truncate``1(System.Int32,Microsoft.FSharp.Collections.FSharpList{``0})"/>
/// keep the start of the queue, and the length of a queue is not stored: the
/// walk reads only as many elements as the result holds, and the flag tells the caller whether the queue itself is the
/// result, as it is when nothing is left out.
/// </para>
/// </remarks>
/// <seealso cref="M:FSharp.Collections.Immutable.Queue.Take``1(System.Int32,System.Collections.Immutable.ImmutableQueue{``0})"/>
/// <seealso cref="M:FSharp.Collections.Immutable.Queue.Truncate``1(System.Int32,System.Collections.Immutable.ImmutableQueue{``0})"/>
/// <seealso cref="M:FSharp.Collections.Immutable.IQueue.Take``1(System.Int32,System.Collections.Immutable.IImmutableQueue{``0})"/>
/// <seealso cref="M:FSharp.Collections.Immutable.IQueue.Truncate``1(System.Int32,System.Collections.Immutable.IImmutableQueue{``0})"/>
let front (count : int) (queue : IImmutableQueue<'T>) = walk (frontLoop count) (frontLoop count) queue

/// <summary>
/// The index and the last element that satisfies the predicate, or <c>ValueNone</c>.
/// </summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="queue">The queue.</param>
/// <returns>
/// The index and the last element for which the predicate returns true, or <c>ValueNone</c> if the predicate evaluates to
/// false for each element.
/// </returns>
/// <exception cref="T:System.ArgumentNullException">The queue is <see langword="null"/>; the parameter name is <c>"queue"</c> in both modules.</exception>
/// <remarks>
/// <para>
/// This is an O(n) operation, where n is the length of the queue: the function copies the queue into an array and searches
/// the copy from its end, as
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.FindBack``1(Microsoft.FSharp.Core.FSharpFunc{``0,System.Boolean},Microsoft.FSharp.Collections.FSharpList{``0})"/>
/// searches a copy of its list.
/// </para>
/// <para>
/// It exists because a queue can be read only from its front, so the four searches from the end,
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.FindBack``1(Microsoft.FSharp.Core.FSharpFunc{``0,System.Boolean},Microsoft.FSharp.Collections.FSharpList{``0})"/>,
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.FindIndexBack``1(Microsoft.FSharp.Core.FSharpFunc{``0,System.Boolean},Microsoft.FSharp.Collections.FSharpList{``0})"/>,
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.TryFindBack``1(Microsoft.FSharp.Core.FSharpFunc{``0,System.Boolean},Microsoft.FSharp.Collections.FSharpList{``0})"/> and
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.TryFindIndexBack``1(Microsoft.FSharp.Core.FSharpFunc{``0,System.Boolean},Microsoft.FSharp.Collections.FSharpList{``0})"/>,
/// need the copy. The function takes the copy itself, so
/// the callers do not check the queue: the snapshot does.
/// </para>
/// </remarks>
/// <seealso cref="M:FSharp.Collections.Immutable.Queue.TryFindBack``1(Microsoft.FSharp.Core.FSharpFunc{``0,System.Boolean},System.Collections.Immutable.ImmutableQueue{``0})"/>
/// <seealso cref="M:FSharp.Collections.Immutable.IQueue.TryFindBack``1(Microsoft.FSharp.Core.FSharpFunc{``0,System.Boolean},System.Collections.Immutable.IImmutableQueue{``0})"/>
let lastMatch (predicate : 'T -> bool) (queue : IImmutableQueue<'T>) =
    let array = Snapshot.ofSeq (nameof queue) queue
    let mutable index = array.Length - 1

    while index >= 0 && not (predicate array[index]) do
        index <- index - 1

    if index < 0 then
        ValueNone
    else
        ValueSome (struct (index, array[index]))
