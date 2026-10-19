/// Argument checks shared by the collection modules.
/// The module keeps its old name, so that the modules written before ADR 0001 compile unchanged until they are replaced.
[<AutoOpen>]
module internal FSharp.Collections.Immutable.ImmutableCollectionUtil

open System
open System.Collections.Immutable
open System.Runtime.InteropServices

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

/// Reads the property IsDefault of a boxed immutable array through reflection, from the type of the array.
let inline readIsDefault (arrayType : Type) (array : obj) : bool =
    arrayType.GetProperty "IsDefault"
    |> nonNull
    |> _.GetValue(array)
    |> unbox<bool>

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
    && readIsDefault sourceType source

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
