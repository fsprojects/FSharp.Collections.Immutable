/// <summary>
/// Parallel algorithms over arrays for the functions of <see cref="T:Microsoft.FSharp.Collections.ArrayModule.Parallel"/>
/// that take or return options or reference tuples (ADR 0001, D3 and D11).
/// </summary>
/// <remarks>
/// The algorithms take value options and tuple constructors instead, so that the default modules and the <c>Tuple</c> modules
/// share them and nothing is allocated per element. The callers check their arguments first, with the exceptions of
/// <see cref="T:Microsoft.FSharp.Collections.ArrayModule.Parallel"/>.
/// </remarks>
module internal FSharp.Collections.Immutable.ParallelArrays

open System
open System.Linq
open System.Threading.Tasks

/// <summary>The length of the chunks of a chunked parallel loop over <paramref name="length"/> elements.</summary>
/// <remarks>
/// The chunks are those of the chunked loops of <see cref="T:Microsoft.FSharp.Collections.ArrayModule.Parallel"/>: one chunk
/// per processor, and a single chunk for fewer than 256 elements, which are not worth a second thread.
/// </remarks>
let chunkLength (length : int) =
    let processors = Environment.ProcessorCount

    if length < 256 then length
    elif length % processors = 0 then length / processors
    else length / processors + 1

/// <summary>Runs an action on the chunks of the indices from 0 to <paramref name="length"/> in parallel.</summary>
/// <remarks>
/// The action takes the first index of its chunk and the index after its last one. A loop per chunk calls no delegate per
/// element.
/// </remarks>
let inline forEachChunk (length : int) ([<InlineIfLambda>] action : int -> int -> unit) =
    let chunk = chunkLength length
    let chunks = if chunk = 0 then 0 else (length + chunk - 1) / chunk

    Parallel.For (
        0,
        chunks,
        fun index ->
            let first = index * chunk
            action first (min length (first + chunk))
    )
    |> ignore

/// <summary>The values that the chooser returns for the elements, in the order of the elements.</summary>
/// <remarks>
/// The chooser runs in parallel into an array of value options, which a sequential pass compacts. Calling
/// <see cref="M:Microsoft.FSharp.Collections.ArrayModule.Parallel.Choose``2(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpOption{``1}},``0[])"/>
/// instead would convert every value option to an option.
/// </remarks>
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

/// <summary>The keys of the projection, each with its elements; the order of both is not specified.</summary>
/// <remarks>
/// The projection, the work of the caller, runs in parallel. The grouping then runs over the computed keys with
/// <see cref="M:System.Linq.Enumerable.ToLookup``3(System.Collections.Generic.IEnumerable{``0},System.Func{``0,``1},System.Func{``0,``2},System.Collections.Generic.IEqualityComparer{``1})"/>,
/// as the sequential grouping of the <c>Arrays</c> module does, so that the keys compare in F# structural equality and a
/// <see langword="null"/> key is a key like any other. The result has the order of
/// <see cref="M:Microsoft.FSharp.Collections.ArrayModule.GroupBy``2(Microsoft.FSharp.Core.FSharpFunc{``0,``1},``0[])"/>,
/// one of the orders that
/// <see cref="M:Microsoft.FSharp.Collections.ArrayModule.Parallel.GroupBy``2(Microsoft.FSharp.Core.FSharpFunc{``0,``1},``0[])"/>
/// allows.
/// </remarks>
let inline groupBy
    ([<InlineIfLambda>] tuple : 'Key -> 'T array -> 'R)
    (projection : 'T -> 'Key)
    (array : 'T array)
    : 'R array when 'Key : equality =
    let keys = Array.Parallel.map projection array

    let lookup =
        Enumerable
            .Range(0, array.Length)
            .ToLookup((fun index -> keys[index]), (fun index -> array[index]), HashIdentity.Structural<'Key>)

    let result = Array.zeroCreate lookup.Count
    let mutable index = 0

    for grouping in lookup do
        result[index] <- tuple grouping.Key (grouping.ToArray ())
        index <- index + 1

    result

/// <summary>The index of the first element that satisfies the predicate, or <c>ValueNone</c>.</summary>
/// <remarks>
/// The predicate runs in parallel. A match breaks the loop, which then starts only the elements before it, and the lowest
/// index that broke the loop wins, as in
/// <see cref="M:Microsoft.FSharp.Collections.ArrayModule.Parallel.TryFindIndex``1(Microsoft.FSharp.Core.FSharpFunc{``0,System.Boolean},``0[])"/>.
/// </remarks>
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

/// <summary>The value that the chooser returns for the first element with a value, or <c>ValueNone</c>.</summary>
/// <remarks>
/// The chooser runs in parallel. A value breaks the loop, which then starts only the elements before it, and the value of
/// the lowest index wins. The function of FSharp.Core,
/// <see cref="M:Microsoft.FSharp.Collections.ArrayModule.Parallel.TryPick``2(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpOption{``1}},``0[])"/>,
/// keeps every value in a concurrent dictionary by index; here a lock guards the value of the lowest index so far, and
/// only the elements that have a value take it.
/// </remarks>
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

/// <summary>Pairs the elements of two arrays of the same length in parallel.</summary>
/// <remarks>
/// The chunks are those of <see cref="M:Microsoft.FSharp.Collections.ArrayModule.Parallel.Zip``2(``0[],``1[])"/>.
/// </remarks>
let inline zip ([<InlineIfLambda>] tuple : 'T1 -> 'T2 -> 'R) (array1 : 'T1 array) (array2 : 'T2 array) : 'R array =
    let result = Array.zeroCreate array1.Length

    forEachChunk
        result.Length
        (fun first last ->
            for index = first to last - 1 do
                result[index] <- tuple array1[index] array2[index]
        )

    result
