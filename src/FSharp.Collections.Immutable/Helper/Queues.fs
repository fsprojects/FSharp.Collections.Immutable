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
/// <see cref="T:System.Collections.Generic.IEnumerator`1"/>. The loop runs with the struct <c>ImmutableQueueEnumerator</c>
/// or with the enumerator of the other queue. The JIT compiles generic code for each struct type argument and calls its
/// members without boxing it, so the struct enumerator of the queue advances in place. Member constraints of an inline
/// loop would need the struct to be public, because they resolve only to members that are accessible from everywhere.
/// </para>
/// </remarks>
module internal FSharp.Collections.Immutable.Queues

open System
open System.Collections.Generic
open System.Collections.Immutable
open System.Runtime.InteropServices

/// The struct enumerator of an ImmutableQueue as an IEnumerator, for the loops below, which take it as a type argument.
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

/// Runs a loop over the struct enumerator of an ImmutableQueue, or over the enumerator of another queue, which it
/// disposes. The callers pass the same generic loop twice, once for each type of enumerator.
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
/// <remarks>
/// <see cref="M:System.Collections.Immutable.ImmutableQueue.Create``1(System.ReadOnlySpan{``0})"/> copies the elements
/// into the front stack of the queue, from the last one to the first one, so that the queue dequeues them without
/// reversing a back stack first.
/// </remarks>
let inline ofFreshArray (array : 'T array) = ImmutableQueue.Create<'T>(ReadOnlySpan<'T> array)

/// A queue of the elements of a buffer that nobody changes any more, built as ofFreshArray builds it, without a copy of
/// the buffer.
let ofBuffer (buffer : ResizeArray<'T>) =
    ImmutableQueue.Create<'T>(Span<'T>.op_Implicit(CollectionsMarshal.AsSpan buffer))

/// The index and the first element that satisfies the predicate, or ValueNone.
let firstMatch (predicate : 'T -> bool) (queue : IImmutableQueue<'T>) =
    walk (firstMatchLoop predicate) (firstMatchLoop predicate) queue

/// The first value that the chooser returns for the elements, in their order, or ValueNone.
let firstChoice (chooser : 'T -> 'U voption) (queue : IImmutableQueue<'T>) =
    walk (firstChoiceLoop chooser) (firstChoiceLoop chooser) queue

/// The first elements of a queue, at most the count, and whether the queue has more elements.
let front (count : int) (queue : IImmutableQueue<'T>) = walk (frontLoop count) (frontLoop count) queue

/// The index and the last element that satisfies the predicate, or ValueNone; List searches a copy from the end too. A
/// null queue throws ArgumentNullException for "queue", the name of the parameter in both modules.
let lastMatch (predicate : 'T -> bool) (queue : IImmutableQueue<'T>) =
    let array = Snapshot.ofSeq (nameof queue) queue
    let mutable index = array.Length - 1

    while index >= 0 && not (predicate array[index]) do
        index <- index - 1

    if index < 0 then
        ValueNone
    else
        ValueSome (struct (index, array[index]))
