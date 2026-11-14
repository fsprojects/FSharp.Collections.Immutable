/// <summary>
/// The buffer in which a function collects the elements that it chooses one by one, until it builds the tree of the result
/// (ADR 0001, D5).
/// </summary>
/// <remarks>
/// <para>
/// A result of unknown length waits in a <see cref="T:System.Collections.Generic.List`1"/> that doubles its array as it
/// fills, and the tree is built once, from its elements. The growth costs the copies, and for a result of some 20 000
/// integers or 10 000 references or more the arrays of the growth are allocated in the large object heap, which a collection
/// of the second generation reclaims; for a million elements the list allocated 18 % (integers) to 35 % (strings) more than
/// the tree needs.
/// </para>
/// <para>
/// The number of elements to choose from is known, and it bounds the result. The buffer takes an array of that length: a
/// new one for a short result, where the array is cheap and the pool would only add its own work, and one rented from
/// <see cref="P:System.Buffers.ArrayPool`1.Shared"/> for a long one, which allocates nothing in the steady state. An array
/// of the length of the source is more than the result needs when few of the elements are chosen, and it would be the
/// array of a hundred million elements for a list of that length. The rented array is therefore at most
/// <see cref="F:FSharp.Collections.Immutable.Buffers.PooledUpTo"/> elements long; a longer result moves into a list.
/// </para>
/// <para>
/// The buffer is disposable, and the function that uses it disposes it: the rented array goes back to the pool also when
/// the chooser throws before the list is built.
/// </para>
/// </remarks>
module internal FSharp.Collections.Immutable.Buffers

open System
open System.Buffers
open System.Collections.Generic
open System.Collections.Immutable
open System.Runtime.CompilerServices
open System.Runtime.InteropServices

/// <summary>
/// The least length of the source for which the buffer is rented from the pool.
/// </summary>
/// <remarks>
/// For 4 to 2048 elements, an array of its own and a rented one took the same time, 0 to 20 % less than the list when all
/// of the elements were chosen; the first call that rents an array of a length allocates it, and the first call took 6 to
/// 81 µs for 4 to 2048 elements.
/// An array below the limit is a small allocation that the next collection of the youngest generation reclaims.
/// </remarks>
[<Literal>]
let PooledFrom = 1024

/// <summary>
/// The most elements in the array that the buffer rents; a result that is longer moves into a list.
/// </summary>
[<Literal>]
let PooledUpTo = 1048576

/// <summary>
/// Collects the elements that a function chooses from a source of a known length, and builds the indexed list of them.
/// </summary>
/// <typeparam name="T">The type of the elements.</typeparam>
/// <param name="bound">The number of elements to choose from, which is the most that the result can have.</param>
/// <param name="pool">The pool that lends the array of a long result.</param>
/// <remarks>
/// A buffer is used once: <see cref="M:FSharp.Collections.Immutable.Buffers.ChosenBuffer`1.ToImmutable"/> ends it. The
/// pool is a parameter so that a test can give one that records what it lends and what it gets back.
/// </remarks>
[<Sealed>]
type ChosenBuffer<'T> (bound : int, pool : ArrayPool<'T>) =

    let pooled = bound >= PooledFrom

    let mutable buffer : 'T array =
        if bound = 0 then Array.Empty<'T>()
        elif pooled then pool.Rent (min bound PooledUpTo)
        else Array.zeroCreate bound

    let mutable count = 0
    let mutable overflow : ResizeArray<'T> | null = null

    /// <summary>
    /// Creates a buffer that rents the array of a long result from <see cref="P:System.Buffers.ArrayPool`1.Shared"/>.
    /// </summary>
    /// <param name="bound">The number of elements to choose from, which is the most that the result can have.</param>
    new (bound : int) = new ChosenBuffer<'T> (bound, ArrayPool<'T>.Shared)

    /// Returns the rented array to the pool, cleared as far as it was written when it holds references. The buffer is
    /// left with an empty array, which is full at once, so that an element goes to the slow path of Add.
    member private _.Release () =
        if pooled && buffer.Length > 0 then
            if RuntimeHelpers.IsReferenceOrContainsReferences<'T>() then
                Array.Clear (buffer, 0, count)

            pool.Return (buffer, false)

        buffer <- Array.Empty<'T>()
        count <- 0

    /// Moves the elements of the full array into a list that grows, and gives the array back.
    member private this.Spill () : ResizeArray<'T> =
        let spilled = ResizeArray<'T>(buffer.Length * 2)
        CollectionsMarshal.SetCount (spilled, count)
        ReadOnlySpan<'T>(buffer, 0, count).CopyTo(CollectionsMarshal.AsSpan spilled)
        this.Release ()
        overflow <- spilled
        spilled

    [<MethodImpl(MethodImplOptions.NoInlining)>]
    member private this.AddSlow (value : 'T) =
        match overflow with
        | null -> this.Spill().Add value
        | spilled -> spilled.Add value

    /// <summary>
    /// Appends an element.
    /// </summary>
    /// <param name="value">The element.</param>
    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    member this.Add (value : 'T) =
        if count = buffer.Length then
            this.AddSlow value
        else
            buffer[count] <- value
            count <- count + 1

    /// <summary>
    /// The indexed list of the elements that were added, in their order.
    /// </summary>
    /// <returns>The indexed list, which is empty when nothing was added.</returns>
    member this.ToImmutable () : ImmutableList<'T> =
        match overflow with
        | null ->
            let result = ImmutableList.Create<'T>(ReadOnlySpan<'T>(buffer, 0, count))
            this.Release ()
            result
        | spilled -> ImmutableList.Create<'T>(Span<'T>.op_Implicit(CollectionsMarshal.AsSpan spilled))

    interface IDisposable with

        /// <summary>
        /// Returns the rented array to the pool when the buffer still holds it.
        /// </summary>
        /// <remarks>
        /// A buffer that built its list, or that moved its elements into a list, holds no rented array, and nothing
        /// happens. The call matters when the chooser throws before the list is built.
        /// </remarks>
        member this.Dispose () = this.Release ()

/// <summary>
/// Builds the indexed list of the elements that a function writes into an array of a known length, with a given pool, and
/// returns what the function returns as well.
/// </summary>
/// <typeparam name="T">The type of the elements.</typeparam>
/// <typeparam name="R">The type of what the function returns.</typeparam>
/// <param name="pool">The pool that lends the array of a long list.</param>
/// <param name="length">The number of elements.</param>
/// <param name="fill">
/// Writes the elements at the indexes 0 to <paramref name="length"/> - 1 of the array that it gets, which can be longer.
/// </param>
/// <returns>The indexed list, and the value that <paramref name="fill"/> returned.</returns>
/// <remarks>
/// <para>
/// The array is rented from the pool for <see cref="F:FSharp.Collections.Immutable.Buffers.PooledFrom"/> to
/// <see cref="F:FSharp.Collections.Immutable.Buffers.PooledUpTo"/> elements. It goes back to the pool, cleared as far as
/// it was written when it holds references, also when <paramref name="fill"/> throws.
/// </para>
/// <para>
/// A shorter array is a new one: it is cheap, and the pool would only add its own work. A longer array is a new one of
/// the exact length too. The shared pool lends arrays whose lengths are powers of two, so it would answer a request for
/// 1 048 577 elements with an array of 2 097 152, and it would keep that array after the call.
/// </para>
/// <para>
/// The pool is a parameter so that a test can give one that records what it lends and what it gets back.
/// </para>
/// </remarks>
let ofFilledFrom (pool : ArrayPool<'T>) (length : int) (fill : 'T array -> 'R) : struct (ImmutableList<'T> * 'R) =
    if length < PooledFrom || length > PooledUpTo then
        let items : 'T array =
            if length = 0 then
                Array.Empty<'T>()
            else
                Array.zeroCreate length
        let extra = fill items
        struct (ImmutableList.Create<'T>(ReadOnlySpan<'T>(items)), extra)
    else
        let items = pool.Rent length

        try
            let extra = fill items
            struct (ImmutableList.Create<'T>(ReadOnlySpan<'T>(items, 0, length)), extra)
        finally
            if RuntimeHelpers.IsReferenceOrContainsReferences<'T>() then
                Array.Clear (items, 0, length)

            pool.Return (items, false)

/// <summary>
/// Builds the indexed list of the elements that a function writes into an array of a known length, and returns what the
/// function returns as well.
/// </summary>
/// <typeparam name="T">The type of the elements.</typeparam>
/// <typeparam name="R">The type of what the function returns.</typeparam>
/// <param name="length">The number of elements.</param>
/// <param name="fill">
/// Writes the elements at the indexes 0 to <paramref name="length"/> - 1 of the array that it gets, which can be longer.
/// </param>
/// <returns>The indexed list, and the value that <paramref name="fill"/> returned.</returns>
/// <remarks>
/// The array of a long list comes from <see cref="P:System.Buffers.ArrayPool`1.Shared"/>, as
/// <see cref="M:FSharp.Collections.Immutable.Buffers.ofFilledFrom``2(System.Buffers.ArrayPool{``0},System.Int32,Microsoft.FSharp.Core.FSharpFunc{``0[],``1})"/>
/// describes.
/// </remarks>
let ofFilledWith (length : int) (fill : 'T array -> 'R) : struct (ImmutableList<'T> * 'R) =
    ofFilledFrom ArrayPool<'T>.Shared length fill

/// <summary>
/// Builds the indexed list of the elements that a function writes into an array of a known length.
/// </summary>
/// <typeparam name="T">The type of the elements.</typeparam>
/// <param name="length">The number of elements.</param>
/// <param name="fill">
/// Writes the elements at the indexes 0 to <paramref name="length"/> - 1 of the array that it gets, which can be longer.
/// </param>
/// <returns>The indexed list.</returns>
let ofFilled (length : int) (fill : 'T array -> unit) : ImmutableList<'T> =
    let struct (result, ()) = ofFilledWith length fill
    result
