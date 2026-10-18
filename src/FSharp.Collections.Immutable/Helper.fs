#if INTERACTIVE
namespace global
#else
namespace FSharp.Collections.Immutable
#endif

open System
open System.Collections.Generic
open System.Collections.Immutable
open System.Linq
open System.Runtime.InteropServices

/// Argument checks shared by the collection modules.
/// The module keeps its old name, so that the modules written before ADR 0001 compile unchanged until they are replaced.
[<AutoOpen>]
module internal ImmutableCollectionUtil =

    /// Throws `ArgumentNullException` for a `null` argument, as FSharp.Core does for a `null` array or list.
    let inline checkNotNull name (arg : _ | null) =
        match arg with
        | null -> nullArg name
        | _ -> ()

    /// <summary>
    /// Throws <see cref="T:System.ArgumentNullException"/> for a default <see cref="T:System.Collections.Immutable.ImmutableArray`1"/>.
    /// </summary>
    /// <remarks>
    /// A default immutable array has no underlying array. The collection modules treat it as FSharp.Core treats a
    /// <see langword="null"/> array (ADR 0001, D6).
    /// </remarks>
    let inline checkNotDefault name (array : ImmutableArray<'T>) =
        if array.IsDefault then
            nullArg name

    /// <summary>
    /// Tells whether an object is a default <see cref="T:System.Collections.Immutable.ImmutableArray`1"/>, of any element type.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="T:System.Collections.Generic.IEnumerable`1"/> is covariant, so a sequence of a reference type is also a
    /// sequence of its base types: a default <c>ImmutableArray&lt;string&gt;</c> can be a <c>seq&lt;obj&gt;</c>. The type
    /// test <c>:? ImmutableArray&lt;obj&gt;</c> does not recognize it, because the type is another instantiation.
    /// </para>
    /// <para>
    /// The function finds the instantiation from the type of the object and reads the property <c>IsDefault</c> through
    /// reflection. It allocates nothing for an object of another type. It is inline, because the public inline members of
    /// the computation expression call the check, and an inline member cannot call an internal function.
    /// </para>
    /// </remarks>
    let inline isDefaultImmutableArray (source : obj) : bool =
        let sourceType = source.GetType ()

        sourceType.IsGenericType
        && Type.(=) (sourceType.GetGenericTypeDefinition (), typedefof<ImmutableArray<_>>)
        && unbox<bool>((nonNull (sourceType.GetProperty "IsDefault")).GetValue source)

    /// <summary>
    /// Throws <see cref="T:System.ArgumentNullException"/> for a <see langword="null"/> sequence and for a default
    /// <see cref="T:System.Collections.Immutable.ImmutableArray`1"/> that is given as a sequence.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A default immutable array converts to a sequence without a check, and the sequence fails with
    /// <see cref="T:System.InvalidOperationException"/> when a function enumerates it. A function that takes a sequence
    /// calls this check, which treats a default immutable array as FSharp.Core treats a <see langword="null"/> sequence
    /// (ADR 0001, D6). Any other sequence passes.
    /// </para>
    /// <para>
    /// An immutable array of the element type is recognized by its type, without reflection. A default immutable array of
    /// a derived reference type, which arrives as a sequence of a base type by covariance, is recognized by its generic
    /// type and by reading <c>IsDefault</c> through reflection.
    /// </para>
    /// </remarks>
    let inline checkNotNullOrDefaultSeq name (source : seq<'T>) =
        checkNotNull name source

        match source with
        | :? ImmutableArray<'T> as array -> checkNotDefault name array
        | _ ->
            if isDefaultImmutableArray source then
                nullArg name

    /// <summary>
    /// The array under an immutable array, without a copy (ADR 0001, D5).
    /// </summary>
    /// <remarks>
    /// The caller reads the array and never writes to it: the immutable array and every value that shares it see the same
    /// memory. A default immutable array throws <see cref="T:System.ArgumentNullException"/> for <paramref name="name"/>.
    /// </remarks>
    let inline unwrapArray name (array : ImmutableArray<'T>) : 'T array =
        checkNotDefault name array
        // The check above guarantees an underlying array
        ImmutableCollectionsMarshal.AsArray array
        |> Unchecked.nonNull

    /// <summary>
    /// An immutable array over an array, without a copy (ADR 0001, D5).
    /// </summary>
    /// <remarks>
    /// Only an array that nobody can write to any more may be wrapped: a fresh result of a computation.
    /// </remarks>
    let inline wrapArray (array : 'T array) : ImmutableArray<'T> = ImmutableCollectionsMarshal.AsImmutableArray array

    /// The constructor of a struct pair, for the algorithms shared by the default and the `Tuple` modules.
    let inline structPair (first : 'T1) (second : 'T2) = struct (first, second)

    /// The constructor of a reference pair, for the algorithms shared by the default and the `Tuple` modules.
    let inline referencePair (first : 'T1) (second : 'T2) = (first, second)

    /// The constructor of a struct triple, for the algorithms shared by the default and the `Tuple` modules.
    let inline structTriple (first : 'T1) (second : 'T2) (third : 'T3) = struct (first, second, third)

    /// The constructor of a reference triple, for the algorithms shared by the default and the `Tuple` modules.
    let inline referenceTriple (first : 'T1) (second : 'T2) (third : 'T3) = (first, second, third)

/// The default comparers of the collections (ADR 0001, D5).
[<AutoOpen>]
module internal Comparers =

    /// <summary>
    /// The comparer of F# generic comparison for a type: the comparer of the sorts, of <c>binarySearch</c> and of the
    /// sorted collections that are created without a comparer.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The class resolves <see cref="M:Microsoft.FSharp.Core.LanguagePrimitives.FastGenericComparer``1"/> once per type, in
    /// generic code, where FSharp.Core returns the comparer that it keeps for the type.
    /// </para>
    /// <para>
    /// A call of <c>FastGenericComparer</c> at a concrete type that is not primitive creates a new comparer each time, and
    /// the F# optimizer creates such calls when it inlines a small function of the library into its caller. Collections of
    /// one type would then have different comparers, and the BCL could not take its shortcuts for two collections with the
    /// same comparer, such as the one of <c>ImmutableSortedSet.Union</c>. A static field cannot be inlined.
    /// </para>
    /// </remarks>
    [<Sealed>]
    type DefaultComparer<'T when 'T : comparison> private () =

        static let comparer = LanguagePrimitives.FastGenericComparer<'T>

        /// The comparer of F# generic comparison for the type.
        static member Instance : IComparer<'T> = comparer

/// <summary>
/// The exception messages of FSharp.Core (<c>FSCore.resx</c>) and of the .NET libraries, in English.
/// </summary>
/// <remarks>
/// The modules throw what the FSharp.Core function that they mirror throws (ADR 0001, D6). FSharp.Core keeps its resources
/// internal, so the texts are copied. The texts of the .NET libraries are for the functions that report a BCL exception
/// under the name of their own parameter.
/// </remarks>
module internal ErrorStrings =

    [<Literal>]
    let ArraysHadDifferentLengths = "The arrays have different lengths."

    [<Literal>]
    let IndexOutOfBounds = "The index was outside the range of elements in the list."

    [<Literal>]
    let InputArrayEmpty = "The input array was empty."

    [<Literal>]
    let InputListWasEmpty = "The input list was empty."

    [<Literal>]
    let InputMustBeNonNegative = "The input must be non-negative."

    [<Literal>]
    let InputMustBePositive = "The input must be positive."

    [<Literal>]
    let InputSequenceEmpty = "The input sequence was empty."

    [<Literal>]
    let InputSequenceTooLong = "The input sequence contains more than one element."

    [<Literal>]
    let KeyNotFound = "The item, key, or index was not found in the collection."

    [<Literal>]
    let KeyNotFoundAlt = "An index satisfying the predicate was not found in the collection."

    [<Literal>]
    let ListsHadDifferentLengths = "The lists had different lengths."

    /// The message of the modules written before ADR 0001.
    [<Literal>]
    let ListsHaveDifferentLengths = "The lists have different lengths."

    /// The message of the .NET libraries for a negative argument, as `Array.BinarySearch` reports it.
    [<Literal>]
    let NeedNonNegativeNumber = "Non-negative number required."

    [<Literal>]
    let NotAPermutation = "The function did not compute a permutation."

    [<Literal>]
    let NotEnoughElements = "The input sequence has an insufficient number of elements."

    [<Literal>]
    let OutOfRange = "The index is outside the legal range."

/// <summary>
/// The exceptions of the <c>DetailedExceptions</c> module of FSharp.Core, with the same types, messages and parameter names.
/// </summary>
/// <remarks>
/// The callers pass the names of their own parameters (ADR 0001, D6).
/// </remarks>
module internal Raise =

    let inline private invalidArgFormat (arg : string) (format : string) (paramArray : obj array) =
        raise (ArgumentException (String.Format (format, paramArray), arg))

    /// The arrays of a function of two arrays have different lengths.
    let invalidArgDifferentArrayLength (arg1 : string) (len1 : int) (arg2 : string) (len2 : int) : 'T =
        invalidArgFormat arg1 "{0}\n{1}.Length = {2}, {3}.Length = {4}" [|
            ErrorStrings.ArraysHadDifferentLengths
            arg1
            len1
            arg2
            len2
        |]

    /// The lists of a function of two lists have different lengths; `diff` is how much shorter `arg1` is.
    let invalidArgDifferentListLength (arg1 : string) (arg2 : string) (diff : int) : 'T =
        invalidArgFormat arg1 "{0}\n{1} is {2} {3} shorter than {4}" [|
            ErrorStrings.ListsHadDifferentLengths
            arg1
            diff
            (if diff = 1 then "element" else "elements")
            arg2
        |]

    /// The arrays of a function of three arrays have different lengths.
    let invalidArg3ArraysDifferent (arg1 : string) (arg2 : string) (arg3 : string) (len1 : int) (len2 : int) (len3 : int) : 'T =
        invalidArgFormat
            (String.Concat [| arg1; ", "; arg2; ", "; arg3 |])
            "{0}\n {1}.Length = {2}, {3}.Length = {4}, {5}.Length = {6}"
            [| ErrorStrings.ArraysHadDifferentLengths; arg1; len1; arg2; len2; arg3; len3 |]

    /// The lists of a function of three lists have different lengths.
    let invalidArg3ListsDifferent (arg1 : string) (arg2 : string) (arg3 : string) (len1 : int) (len2 : int) (len3 : int) : 'T =
        invalidArgFormat
            (String.Concat [| arg1; ", "; arg2; ", "; arg3 |])
            "{0}\n {1}.Length = {2}, {3}.Length = {4}, {5}.Length = {6}"
            [| ErrorStrings.ListsHadDifferentLengths; arg1; len1; arg2; len2; arg3; len3 |]

    /// A count or an index is negative.
    let invalidArgInputMustBeNonNegative (arg : string) (count : int) : 'T =
        invalidArgFormat arg "{0}\n{1} = {2}" [| ErrorStrings.InputMustBeNonNegative; arg; count |]

    /// A size is not positive.
    let invalidArgInputMustBePositive (arg : string) (count : int) : 'T =
        invalidArgFormat arg "{0}\n{1} = {2}" [| ErrorStrings.InputMustBePositive; arg; count |]

    /// An index or a count is beyond a bound.
    let invalidArgOutOfRange (arg : string) (index : int) (text : string) (bound : int) : 'T =
        let message =
            String.Format ("{0}\n{1} = {2}, {3} = {4}", [| box ErrorStrings.OutOfRange; arg; index; text; bound |])

        raise (ArgumentOutOfRangeException (arg, message))

    /// A list is shorter than an index.
    let invalidOpListNotEnoughElements (index : int) : 'T =
        let message =
            String.Format (
                "{0}\nThe list was {1} {2} shorter than the index",
                [| box ErrorStrings.NotEnoughElements; index; (if index = 1 then "element" else "elements") |]
            )

        raise (InvalidOperationException message)

    /// No element satisfies the predicate of a `find` or `pick` function.
    let indexNotFound () : 'T = raise (KeyNotFoundException ErrorStrings.KeyNotFoundAlt)

    /// A collection has fewer elements than a function needs.
    let notEnoughElements () : 'T = raise (InvalidOperationException ErrorStrings.NotEnoughElements)

/// The checks of the FSharp.Core Array module that the functions of several arrays share.
module internal ArrayChecks =

    /// The arrays of a function of two arrays must have the same length.
    let inline sameLengths (name1 : string) (array1 : 'T1 array) (name2 : string) (array2 : 'T2 array) =
        if array1.Length <> array2.Length then
            Raise.invalidArgDifferentArrayLength name1 array1.Length name2 array2.Length

    /// The arrays of a function of three arrays must have the same length.
    let inline sameLengths3
        (name1 : string)
        (array1 : 'T1 array)
        (name2 : string)
        (array2 : 'T2 array)
        (name3 : string)
        (array3 : 'T3 array)
        =
        if
            array1.Length <> array2.Length
            || array1.Length <> array3.Length
        then
            Raise.invalidArg3ArraysDifferent name1 name2 name3 array1.Length array2.Length array3.Length

/// <summary>
/// Algorithms over arrays that the default modules and the <c>Tuple</c> modules share (ADR 0001, D3).
/// </summary>
/// <remarks>
/// Each algorithm takes the constructor of the tuples that it produces, so that neither variant pays for a second pass.
/// The callers check their arguments first, with the exceptions of the FSharp.Core module that they mirror.
/// </remarks>
module internal Arrays =

    /// The pairs of every element of `array1` with every element of `array2`.
    let inline allPairs ([<InlineIfLambda>] tuple : 'T1 -> 'T2 -> 'R) (array1 : 'T1 array) (array2 : 'T2 array) : 'R array =
        let length2 = array2.Length
        let result = Array.zeroCreate (array1.Length * length2)

        for i = 0 to array1.Length - 1 do
            for j = 0 to length2 - 1 do
                result[i * length2 + j] <- tuple array1[i] array2[j]

        result

    /// A key that may be null, for a dictionary, which rejects null keys.
    [<Struct>]
    type NullableKey<'Key> = { Key : 'Key }

    /// Compares wrapped keys in F# structural equality, which handles null keys, as FSharp.Core does for its keys.
    let nullableKeyComparer<'Key when 'Key : equality> () =
        let structural = HashIdentity.Structural<'Key>

        {
            new IEqualityComparer<NullableKey<'Key>> with
                member _.Equals (left, right) = structural.Equals (left.Key, right.Key)
                member _.GetHashCode key = structural.GetHashCode key.Key
        }

    /// <summary>
    /// The keys of the projection in the order of their first occurrence, each with the number of its elements.
    /// </summary>
    /// <remarks>
    /// The counting keeps one counter per key, not the elements, so it needs memory for the keys only.
    /// </remarks>
    let inline countBy
        ([<InlineIfLambda>] tuple : 'Key -> int -> 'R)
        (projection : 'T -> 'Key)
        (array : 'T array)
        : 'R array when 'Key : equality =
        let indices = Dictionary<NullableKey<'Key>, int>(nullableKeyComparer<'Key>())
        let keys = ResizeArray<'Key>()
        let counts = ResizeArray<int>()

        for item in array do
            let key = projection item
            let mutable index = 0

            if indices.TryGetValue ({ Key = key }, &index) then
                counts[index] <- counts[index] + 1
            else
                indices.Add ({ Key = key }, keys.Count)
                keys.Add key
                counts.Add 1

        let result = Array.zeroCreate keys.Count

        for index = 0 to result.Length - 1 do
            result[index] <- tuple keys[index] counts[index]

        result

    /// The keys of the projection in the order of their first occurrence, each with its elements in their order.
    let inline groupBy
        ([<InlineIfLambda>] tuple : 'Key -> 'T array -> 'R)
        (projection : 'T -> 'Key)
        (array : 'T array)
        : 'R array when 'Key : equality =
        let lookup = array.ToLookup ((fun item -> projection item), HashIdentity.Structural<'Key>)
        let result = Array.zeroCreate lookup.Count
        let mutable index = 0

        for grouping in lookup do
            result[index] <- tuple grouping.Key (grouping.ToArray ())
            index <- index + 1

        result

    /// Every element with its index.
    let inline indexed ([<InlineIfLambda>] tuple : int -> 'T -> 'R) (array : 'T array) : 'R array =
        let result = Array.zeroCreate array.Length

        for i = 0 to result.Length - 1 do
            result[i] <- tuple i array[i]

        result

    /// Every element with its successor.
    let inline pairwise ([<InlineIfLambda>] tuple : 'T -> 'T -> 'R) (array : 'T array) : 'R array =
        if array.Length < 2 then
            Array.Empty ()
        else
            let result = Array.zeroCreate (array.Length - 1)

            for i = 0 to result.Length - 1 do
                result[i] <- tuple array[i] array[i + 1]

            result

    /// Splits the pairs of an array with `split` into two arrays.
    let inline unzip ([<InlineIfLambda>] split : 'P -> struct ('T1 * 'T2)) (array : 'P array) =
        let result1 = Array.zeroCreate array.Length
        let result2 = Array.zeroCreate array.Length

        for i = 0 to array.Length - 1 do
            let struct (first, second) = split array[i]
            result1[i] <- first
            result2[i] <- second

        struct (result1, result2)

    /// Splits the triples of an array with `split` into three arrays.
    let inline unzip3 ([<InlineIfLambda>] split : 'P -> struct ('T1 * 'T2 * 'T3)) (array : 'P array) =
        let result1 = Array.zeroCreate array.Length
        let result2 = Array.zeroCreate array.Length
        let result3 = Array.zeroCreate array.Length

        for i = 0 to array.Length - 1 do
            let struct (first, second, third) = split array[i]
            result1[i] <- first
            result2[i] <- second
            result3[i] <- third

        struct (result1, result2, result3)

    /// Pairs the elements of two arrays of the same length.
    let inline zip ([<InlineIfLambda>] tuple : 'T1 -> 'T2 -> 'R) (array1 : 'T1 array) (array2 : 'T2 array) : 'R array =
        let result = Array.zeroCreate array1.Length

        for i = 0 to result.Length - 1 do
            result[i] <- tuple array1[i] array2[i]

        result

    /// Combines the elements of three arrays of the same length.
    let inline zip3
        ([<InlineIfLambda>] tuple : 'T1 -> 'T2 -> 'T3 -> 'R)
        (array1 : 'T1 array)
        (array2 : 'T2 array)
        (array3 : 'T3 array)
        : 'R array =
        let result = Array.zeroCreate array1.Length

        for i = 0 to result.Length - 1 do
            result[i] <- tuple array1[i] array2[i] array3[i]

        result
