/// <summary>
/// Parallel algorithms over arrays for the functions of
/// <see cref="T:Microsoft.FSharp.Collections.ArrayModule.Parallel"/> that take or return options or reference tuples
/// (ADR 0001, D3 and D11).
/// </summary>
/// <remarks>
/// <para>
/// The default functions of <see cref="T:FSharp.Collections.Immutable.FlatList"/> take and return value options and struct
/// tuples, and the functions of the <see cref="T:FSharp.Collections.Immutable.Tuple.FlatList"/> module return reference
/// tuples, where the FSharp.Core functions take and return options and return reference tuples. Calling the FSharp.Core
/// function and converting would allocate for every element. The algorithms here take value options and tuple constructors
/// instead, so that the default modules and the modules of the <see cref="N:FSharp.Collections.Immutable.Tuple"/> namespace
/// share them and nothing is allocated per element.
/// </para>
/// <para>
/// The callers check their arguments first, with the exceptions of
/// <see cref="T:Microsoft.FSharp.Collections.ArrayModule.Parallel"/>, so the algorithms check nothing and document no
/// exceptions.
/// </para>
/// <para>
/// The summary of each function that mirrors a function of the FSharp.Core
/// <see cref="T:Microsoft.FSharp.Collections.ArrayModule.Parallel"/> module is the summary of that function word for word
/// (FSharp.Core 10.1), with <c>ValueSome</c> and <c>ValueNone</c> where it says <c>Some</c> and <c>None</c>, and so are the
/// descriptions of the parameters and of the result where the signatures agree. The remarks say what differs and why the
/// function exists.
/// </para>
/// </remarks>
module internal FSharp.Collections.Immutable.ParallelArrays

open System
open System.Collections.Generic
open System.Linq
open System.Threading.Tasks

/// <summary>
/// The chunks of a chunked parallel loop over <paramref name="length"/> elements on <paramref name="processors"/>
/// processors, each as its first index and the index after its last one.
/// </summary>
/// <param name="processors">The number of processors.</param>
/// <param name="length">The number of elements.</param>
/// <returns>The chunks, each as a struct tuple of its first index and the index after its last one.</returns>
/// <remarks>
/// <para>
/// The chunks are those of the chunked loops of <see cref="T:Microsoft.FSharp.Collections.ArrayModule.Parallel"/>: one chunk
/// per processor, and a single chunk for fewer than 256 elements, which are not worth a second thread.
/// </para>
/// <para>
/// The arithmetic stays in the range of <see cref="T:System.Int32"/> for every array length. The count divides the length
/// minus one, because the length plus the chunk length overflows for arrays of more than half the maximum length. The end
/// of a chunk adds only the elements that remain to its first index, because the first index plus the full chunk length
/// can overflow for arrays near the maximum length on machines with more than 56 processors.
/// </para>
/// <para>
/// FSharp.Core has no such function in its public API. It is a function of its own, so that
/// <see cref="M:FSharp.Collections.Immutable.ParallelArrays.forEachChunk(System.Int32,Microsoft.FSharp.Core.FSharpFunc{System.Int32,Microsoft.FSharp.Core.FSharpFunc{System.Int32,Microsoft.FSharp.Core.Unit}})"/>
/// and the tests share it, and the tests can check the arithmetic for lengths that no array can have.
/// </para>
/// </remarks>
let chunks (processors : int) (length : int) : struct (int * int) array =
    let chunk =
        if length < 256 then length
        elif length % processors = 0 then length / processors
        else length / processors + 1

    let count = if chunk = 0 then 0 else (length - 1) / chunk + 1

    Array.init
        count
        (fun index ->
            let first = index * chunk
            struct (first, first + min chunk (length - first))
        )

/// <summary>
/// Runs an action on the chunks of the indices from 0 to <paramref name="length"/> in parallel.
/// </summary>
/// <param name="length">The number of elements.</param>
/// <param name="action">The action, which takes the first index of its chunk and the index after its last one.</param>
/// <remarks>
/// <para>
/// A loop per chunk calls no delegate per element.
/// </para>
/// <para>
/// FSharp.Core has no such function in its public API. It exists because a
/// <see cref="M:System.Threading.Tasks.Parallel.For(System.Int32,System.Int32,System.Action{System.Int32})"/> over the
/// indices calls a delegate for every element, where the action of a chunk is inlined and loops over its indices.
/// </para>
/// </remarks>
let inline forEachChunk (length : int) ([<InlineIfLambda>] action : int -> int -> unit) =
    let bounds = chunks Environment.ProcessorCount length

    Parallel.For (
        0,
        bounds.Length,
        fun index ->
            let struct (first, last) = bounds[index]
            action first last
    )
    |> ignore

/// <summary>
/// Apply the given function to each element of the array. Return the array comprised of the results <c>x</c> for each
/// element where the function returns <c>ValueSome x</c>.
/// </summary>
/// <param name="chooser">The function to generate value options from the elements.</param>
/// <param name="array">The input array.</param>
/// <returns>The array of results.</returns>
/// <remarks>
/// <para>
/// Performs the operation in parallel using <see cref="M:System.Threading.Tasks.Parallel.For"/>. The order in which the
/// given function is applied to elements of the input array is not specified. This is an O(n) operation, where n is the
/// length of the array.
/// </para>
/// <para>
/// It exists because
/// <see cref="M:Microsoft.FSharp.Collections.ArrayModule.Parallel.Choose``2(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpOption{``1}},``0[])"/>
/// takes a chooser that returns options, and would convert every value option to an option. The chooser runs in parallel
/// into an array of value options, which a sequential pass compacts.
/// </para>
/// </remarks>
/// <seealso cref="M:FSharp.Collections.Immutable.FlatList.Parallel.Choose``2(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpValueOption{``1}},System.Collections.Immutable.ImmutableArray{``0})"/>
let choose (chooser : 'T -> 'U voption) (array : 'T array) : 'U array =
    let chosen = Array.Parallel.map chooser array
    let mutable count = 0

    for value in chosen do
        if value.IsSome then
            count <- count + 1

    let result = Array.zeroCreate count
    let mutable index = 0

    for value in chosen do
        match value with
        | ValueSome item ->
            result[index] <- item
            index <- index + 1
        | ValueNone -> ()

    result

/// <summary>
/// The comparer of the keys of a parallel grouping.
/// </summary>
/// <returns>
/// The comparer of F# structural equality, except that the keys of the types <see cref="T:System.Double"/> and
/// <see cref="T:System.Single"/> compare with <see cref="M:Microsoft.FSharp.Core.LanguagePrimitives.GenericEqualityER``1(``0,``0)"/>.
/// </returns>
/// <remarks>
/// <para>
/// Keys of type <see cref="T:System.Double"/> and <see cref="T:System.Single"/> compare as in
/// <see cref="M:Microsoft.FSharp.Collections.ArrayModule.Parallel.GroupBy``2(Microsoft.FSharp.Core.FSharpFunc{``0,``1},``0[])"/>:
/// with <see cref="M:Microsoft.FSharp.Core.LanguagePrimitives.GenericEqualityER``1(``0,``0)"/>, so that all NaN keys are in
/// one group. Other keys compare in F# structural equality, in which NaN is not equal to itself.
/// </para>
/// <para>
/// A key that contains NaN, such as a tuple, therefore makes a group of its own, as in
/// <see cref="M:Microsoft.FSharp.Collections.ArrayModule.GroupBy``2(Microsoft.FSharp.Core.FSharpFunc{``0,``1},``0[])"/>.
/// The function of FSharp.Core throws <see cref="T:System.AggregateException"/> for such a key: its second pass cannot find
/// the key that its first pass added. That failure is not mirrored.
/// </para>
/// <para>
/// FSharp.Core has no such function in its public API. It exists because the grouping here runs over the keys with
/// <see cref="M:System.Linq.Enumerable.ToLookup``3(System.Collections.Generic.IEnumerable{``0},System.Func{``0,``1},System.Func{``0,``2},System.Collections.Generic.IEqualityComparer{``1})"/>,
/// which takes the comparer of the keys, and the comparer has to be that of FSharp.Core.
/// </para>
/// </remarks>
let groupingComparer<'Key when 'Key : equality> () : IEqualityComparer<'Key> =
    if
        Type.(=) (typeof<'Key>, typeof<float>)
        || Type.(=) (typeof<'Key>, typeof<float32>)
    then
        HashIdentity.FromFunctions<'Key> LanguagePrimitives.GenericHash LanguagePrimitives.GenericEqualityER
    else
        HashIdentity.Structural<'Key>

/// <summary>
/// Applies a key-generating function to each element of an array in parallel and yields an array of unique keys. Each
/// unique key contains an array of all elements that match to this key.
/// </summary>
/// <param name="tuple">The function that builds an element of the result from a key and the array of its elements.</param>
/// <param name="projection">A function that transforms an element of the array into a comparable key.</param>
/// <param name="array">The input array.</param>
/// <returns>The result array.</returns>
/// <remarks>
/// <para>
/// Performs the operation in parallel using <see cref="M:System.Threading.Tasks.Parallel.For"/>. The order in which the
/// given function is applied to elements of the input array is not specified. The order of the keys and values in the
/// result is also not specified. This is an O(n) operation, where n is the length of the array.
/// </para>
/// <para>
/// It exists because
/// <see cref="M:Microsoft.FSharp.Collections.ArrayModule.Parallel.GroupBy``2(Microsoft.FSharp.Core.FSharpFunc{``0,``1},``0[])"/>
/// returns reference tuples. The projection, the work of the caller, runs in parallel. The grouping then runs over the
/// computed keys with
/// <see cref="M:System.Linq.Enumerable.ToLookup``3(System.Collections.Generic.IEnumerable{``0},System.Func{``0,``1},System.Func{``0,``2},System.Collections.Generic.IEqualityComparer{``1})"/>
/// and the comparer of <see cref="M:FSharp.Collections.Immutable.ParallelArrays.groupingComparer``1"/>, so that a
/// <see langword="null"/> key is a key like any other. The result has the order of
/// <see cref="M:Microsoft.FSharp.Collections.ArrayModule.GroupBy``2(Microsoft.FSharp.Core.FSharpFunc{``0,``1},``0[])"/>,
/// one of the orders that
/// <see cref="M:Microsoft.FSharp.Collections.ArrayModule.Parallel.GroupBy``2(Microsoft.FSharp.Core.FSharpFunc{``0,``1},``0[])"/>
/// allows.
/// </para>
/// </remarks>
/// <seealso cref="M:FSharp.Collections.Immutable.FlatList.Parallel.GroupBy``2(Microsoft.FSharp.Core.FSharpFunc{``0,``1},System.Collections.Immutable.ImmutableArray{``0})"/>
/// <seealso cref="M:FSharp.Collections.Immutable.Tuple.FlatList.Parallel.GroupBy``2(Microsoft.FSharp.Core.FSharpFunc{``0,``1},System.Collections.Immutable.ImmutableArray{``0})"/>
let inline groupBy
    ([<InlineIfLambda>] tuple : 'Key -> 'T array -> 'R)
    (projection : 'T -> 'Key)
    (array : 'T array)
    : 'R array when 'Key : equality =
    let keys = Array.Parallel.map projection array

    let lookup =
        Enumerable
            .Range(0, array.Length)
            .ToLookup((fun index -> keys[index]), (fun index -> array[index]), groupingComparer<'Key>())

    let result = Array.zeroCreate lookup.Count
    let mutable index = 0

    for grouping in lookup do
        result[index] <- tuple grouping.Key (grouping.ToArray ())
        index <- index + 1

    result

/// <summary>
/// Returns the index of the first element in the array that satisfies the given predicate. Returns <c>ValueNone</c> if no
/// such element exists.
/// </summary>
/// <param name="predicate">The function to test the input elements.</param>
/// <param name="array">The input array.</param>
/// <returns>The index of the first element that satisfies the predicate, or <c>ValueNone</c>.</returns>
/// <remarks>
/// <para>
/// This is an O(n) operation in the worst case, where n is the length of the array.
/// </para>
/// <para>
/// It exists because
/// <see cref="M:Microsoft.FSharp.Collections.ArrayModule.Parallel.TryFindIndex``1(Microsoft.FSharp.Core.FSharpFunc{``0,System.Boolean},``0[])"/>
/// returns an option, where the default functions return a value option. The predicate runs in parallel. A match breaks the
/// loop, which then starts only the elements before it, and the lowest index that broke the loop wins, as in the function
/// of FSharp.Core.
/// </para>
/// </remarks>
/// <seealso cref="M:FSharp.Collections.Immutable.FlatList.Parallel.TryFindIndex``1(Microsoft.FSharp.Core.FSharpFunc{``0,System.Boolean},System.Collections.Immutable.ImmutableArray{``0})"/>
/// <seealso cref="M:FSharp.Collections.Immutable.FlatList.Parallel.TryFind``1(Microsoft.FSharp.Core.FSharpFunc{``0,System.Boolean},System.Collections.Immutable.ImmutableArray{``0})"/>
let tryFindIndex (predicate : 'T -> bool) (array : 'T array) : int voption =
    let result =
        Parallel.For (
            0,
            array.Length,
            fun index (state : ParallelLoopState) ->
                if predicate array[index] then
                    state.Break ()
        )

    let lowest = result.LowestBreakIteration

    if lowest.HasValue then
        ValueSome (int lowest.Value)
    else
        ValueNone

/// <summary>
/// Applies the given function to successive elements, returning the first result where the function returns
/// <c>ValueSome x</c> for some <c>x</c>. If the function never returns <c>ValueSome x</c> then <c>ValueNone</c> is
/// returned.
/// </summary>
/// <param name="chooser">The function to transform the array elements into value options.</param>
/// <param name="array">The input array.</param>
/// <returns>The first transformed element that is <c>ValueSome x</c>.</returns>
/// <remarks>
/// <para>
/// This is an O(n) operation in the worst case, where n is the length of the array.
/// </para>
/// <para>
/// It exists because
/// <see cref="M:Microsoft.FSharp.Collections.ArrayModule.Parallel.TryPick``2(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpOption{``1}},``0[])"/>
/// takes a chooser that returns options, where the default functions use value options. The chooser runs in parallel. A
/// value breaks the loop, which then starts only the elements before it, and the value of the lowest index wins. The
/// function of FSharp.Core keeps every value in a concurrent dictionary by index; here a lock guards the value of the
/// lowest index so far, and only the elements that have a value take it.
/// </para>
/// </remarks>
/// <seealso cref="M:FSharp.Collections.Immutable.FlatList.Parallel.TryPick``2(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpValueOption{``1}},System.Collections.Immutable.ImmutableArray{``0})"/>
let tryPick (chooser : 'T -> 'U voption) (array : 'T array) : 'U voption =
    let gate = obj ()
    let lowest = ref Int32.MaxValue
    let chosen = ref ValueNone

    Parallel.For (
        0,
        array.Length,
        fun index (state : ParallelLoopState) ->
            match chooser array[index] with
            | ValueSome _ as value ->
                lock
                    gate
                    (fun () ->
                        if index < lowest.Value then
                            lowest.Value <- index
                            chosen.Value <- value
                    )

                state.Break ()
            | ValueNone -> ()
    )
    |> ignore

    chosen.Value

/// <summary>
/// Combines the two arrays into an array of pairs. The two arrays must have equal lengths, otherwise an
/// <see cref="T:System.ArgumentException"/> is raised.
/// </summary>
/// <param name="tuple">The function that builds an element of the result from the elements at the same index.</param>
/// <param name="array1">The first input array.</param>
/// <param name="array2">The second input array.</param>
/// <returns>The array of tupled elements.</returns>
/// <remarks>
/// <para>
/// This is an O(n) operation, where n is the length of the arrays. The chunks are those of
/// <see cref="M:Microsoft.FSharp.Collections.ArrayModule.Parallel.Zip``2(``0[],``1[])"/>. The callers check the lengths first
/// and raise its exception.
/// </para>
/// <para>
/// It exists because that function returns reference tuples, which allocate for every element, where the default function
/// returns struct tuples.
/// </para>
/// </remarks>
/// <seealso cref="M:FSharp.Collections.Immutable.FlatList.Parallel.Zip``2(System.Collections.Immutable.ImmutableArray{``0},System.Collections.Immutable.ImmutableArray{``1})"/>
/// <seealso cref="M:FSharp.Collections.Immutable.Tuple.FlatList.Parallel.Zip``2(System.Collections.Immutable.ImmutableArray{``0},System.Collections.Immutable.ImmutableArray{``1})"/>
let inline zip ([<InlineIfLambda>] tuple : 'T1 -> 'T2 -> 'R) (array1 : 'T1 array) (array2 : 'T2 array) : 'R array =
    let result = Array.zeroCreate array1.Length

    forEachChunk
        result.Length
        (fun first last ->
            for index = first to last - 1 do
                result[index] <- tuple array1[index] array2[index]
        )

    result
