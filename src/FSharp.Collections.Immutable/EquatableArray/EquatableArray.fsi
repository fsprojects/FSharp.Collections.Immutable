namespace FSharp.Collections.Immutable

open System
open System.Collections.Generic
open System.Collections.Immutable
open System.Runtime.CompilerServices
open System.Text.Json.Serialization

/// <summary>
/// An immutable array with value semantics: two arrays are equal when they hold equal elements in the same order.
/// <para>
/// The type is a struct with one field, an <see cref="T:System.Collections.Immutable.ImmutableArray`1"/>. Thus it is
/// as large as one reference, and a conversion to or from <see cref="T:System.Collections.Immutable.ImmutableArray`1"/>
/// does not copy the elements.
/// </para>
/// <para>
/// Equality and the hash code use all elements and compare them with
/// <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>. Every caller gets this equality: F# equality,
/// <see cref="M:System.Object.Equals(System.Object)"/>, <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>,
/// dictionary and set keys, and the C# equality operators. Elements that are F# arrays compare by reference; use
/// <see cref="T:FSharp.Collections.Immutable.EquatableArray`1"/> as the element type to nest arrays.
/// </para>
/// <para>
/// Comparison puts a shorter array first. Arrays of the same length compare by their first different element, in the
/// order of F# generic comparison. Strings compare ordinally, never by culture.
/// </para>
/// <para>
/// The default value is the empty array. The type supports equality and comparison only when its element type does.
/// </para>
/// </summary>
[<Struct; CustomEquality; CustomComparison>]
[<CollectionBuilder(typeof<EquatableArray>, "Create")>]
[<JsonConverter(typeof<EquatableArrayJsonConverterFactory>)>]
[<StructuredFormatDisplay "{StructuredDisplayText}">]
type EquatableArray<[<EqualityConditionalOn; ComparisonConditionalOn>] 'T> =
    // A struct must show its fields in a signature (FS0314); the field stays private
    val private array : ImmutableArray<'T>

    /// <summary>Wraps <paramref name="items"/> without a copy.</summary>
    /// <param name="items">The elements. A default array gives the empty array.</param>
    new : items : ImmutableArray<'T> -> EquatableArray<'T>

    /// <summary>
    /// Gets the elements as an <see cref="T:System.Collections.Immutable.ImmutableArray`1"/>, without a copy.
    /// </summary>
    /// <remarks>
    /// The value is never a default array. For the default value of this type, it is
    /// <see cref="F:System.Collections.Immutable.ImmutableArray`1.Empty"/>.
    /// </remarks>
    member Items : ImmutableArray<'T>

    /// Gets the number of elements.
    member Length : int

    /// Gets a value that shows if the array has no elements.
    member IsEmpty : bool

    /// <summary>Gets the element at <paramref name="index"/>.</summary>
    /// <param name="index">The zero-based position of the element.</param>
    /// <exception cref="T:System.IndexOutOfRangeException">
    /// <paramref name="index"/> is less than zero, or it is not less than the length.
    /// </exception>
    member Item : index : int -> 'T with get

    /// <summary>
    /// Returns an enumerator that is a struct, so that a <see langword="for"/> loop does not allocate.
    /// </summary>
    member GetEnumerator : unit -> ImmutableArray<'T>.Enumerator

    /// Returns a read-only span over the elements, without a copy.
    member AsSpan : unit -> ReadOnlySpan<'T>

    /// Returns a read-only memory region over the elements, without a copy.
    member AsMemory : unit -> ReadOnlyMemory<'T>

    /// <summary>
    /// Returns the elements from <paramref name="start"/> to <paramref name="finish"/>. F# slice expressions call this
    /// method and get the rules of F# array slices.
    /// </summary>
    /// <param name="start">
    /// The index of the first element. If there is no value, or the value is less than zero, the slice starts at the
    /// first element.
    /// </param>
    /// <param name="finish">
    /// The index of the last element. If there is no value, or the value is not less than the length, the slice ends at
    /// the last element.
    /// </param>
    /// <returns>
    /// The selected elements. An empty range gives the empty array. A range of all elements gives the same array without
    /// a copy; other ranges copy their elements.
    /// </returns>
    /// <example>
    /// <code lang="fsharp">
    /// let numbers = EquatableArray.ofList [ 1; 2; 3; 4 ]
    /// numbers[1..2]    // [|2; 3|]
    /// numbers[2..]     // [|3; 4|]
    /// numbers[5..9]    // [||]
    /// </code>
    /// </example>
    member GetSlice : start : int option * finish : int option -> EquatableArray<'T>

    /// <summary>
    /// Returns <paramref name="length"/> elements from <paramref name="start"/>. C# range expressions such as
    /// <c>array[1..^1]</c> call this method.
    /// </summary>
    /// <param name="start">The index of the first element.</param>
    /// <param name="length">The number of elements.</param>
    /// <returns>
    /// The selected elements. A range of all elements gives the same array without a copy; other ranges copy their
    /// elements.
    /// </returns>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    /// The range is not inside the array, as for <see cref="M:System.Collections.Immutable.ImmutableArray`1.Slice(System.Int32,System.Int32)"/>.
    /// </exception>
    member Slice : start : int * length : int -> EquatableArray<'T>

    /// <summary>
    /// Returns <see langword="true"/> when <paramref name="other"/> has equal elements in the same order. The elements
    /// are compared with <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>.
    /// </summary>
    /// <param name="other">The array to compare with.</param>
    member Equals : other : EquatableArray<'T> -> bool

    /// <summary>
    /// Compares the array with <paramref name="other"/>: a shorter array comes first, and arrays of the same length
    /// compare by their first different element in the order of F# generic comparison.
    /// </summary>
    /// <param name="other">The array to compare with.</param>
    /// <returns>A negative number, zero or a positive number.</returns>
    member CompareTo : other : EquatableArray<'T> -> int

    /// <summary>Returns <see langword="true"/> when the arrays have equal elements in the same order.</summary>
    /// <remarks>C# uses this operator for <c>==</c>; F# equality calls the same equality.</remarks>
    /// <param name="left">The first array.</param>
    /// <param name="right">The second array.</param>
    static member op_Equality : left : EquatableArray<'T> * right : EquatableArray<'T> -> bool

    /// <summary>Returns <see langword="true"/> when the arrays do not have equal elements in the same order.</summary>
    /// <remarks>C# uses this operator for <c>!=</c>.</remarks>
    /// <param name="left">The first array.</param>
    /// <param name="right">The second array.</param>
    static member op_Inequality : left : EquatableArray<'T> * right : EquatableArray<'T> -> bool

    /// <summary>
    /// Gives the elements of <paramref name="source"/> as an
    /// <see cref="T:System.Collections.Immutable.ImmutableArray`1"/>, without a copy and never as a default array.
    /// </summary>
    /// <param name="source">The array.</param>
    static member op_Implicit : source : EquatableArray<'T> -> ImmutableArray<'T>

    /// <summary>Wraps <paramref name="source"/> without a copy. A default array gives the empty array.</summary>
    /// <param name="source">The elements.</param>
    static member op_Implicit : source : ImmutableArray<'T> -> EquatableArray<'T>

    interface IEquatable<EquatableArray<'T>>
    interface IComparable<EquatableArray<'T>>
    interface IComparable
    interface IReadOnlyList<'T>
    interface IList<'T>

/// <summary>
/// Creates <see cref="T:FSharp.Collections.Immutable.EquatableArray`1"/> values for C# collection expressions.
/// </summary>
/// <remarks>
/// The class has exactly one method. Collection expressions require one such method on a non-generic builder type, so do
/// not add a second <see cref="M:FSharp.Collections.Immutable.EquatableArray.Create``1(System.ReadOnlySpan{``0})"/>.
/// </remarks>
and [<AbstractClass; Sealed>] EquatableArray =

    /// <summary>Copies <paramref name="items"/> into a new array.</summary>
    /// <param name="items">The elements.</param>
    /// <returns>An array with the elements of <paramref name="items"/>, in the same order.</returns>
    static member Create : items : ReadOnlySpan<'T> -> EquatableArray<'T>

/// <summary>
/// Converts <see cref="T:FSharp.Collections.Immutable.EquatableArray`1"/> values to and from JSON arrays with
/// System.Text.Json.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="T:FSharp.Collections.Immutable.EquatableArray`1"/> names this factory in a
/// <see cref="T:System.Text.Json.Serialization.JsonConverterAttribute"/>, so no registration is necessary.
/// </para>
/// <para>
/// The JSON is the same as for <see cref="T:System.Collections.Immutable.ImmutableArray`1"/>, and the converters and the
/// naming policy of the serializer options apply to the elements. The default value is written as an empty JSON array.
/// A JSON <c>null</c> gives the empty array.
/// </para>
/// </remarks>
and [<Sealed>] EquatableArrayJsonConverterFactory =
    inherit JsonConverterFactory

    /// Creates the factory.
    new : unit -> EquatableArrayJsonConverterFactory

/// <summary>
/// Functions that create, read and transform <see cref="T:FSharp.Collections.Immutable.EquatableArray`1"/> values.
/// </summary>
[<RequireQualifiedAccess>]
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module EquatableArray =

    // ---- Creation ----

    /// <summary>Gets the array without elements.</summary>
    /// <remarks>The default value of <see cref="T:FSharp.Collections.Immutable.EquatableArray`1"/> is equal to it.</remarks>
    [<GeneralizableValue>]
    [<CompiledName "Empty">]
    val empty<'T> : EquatableArray<'T>

    /// <summary>Copies the elements of <paramref name="array"/> into a new array.</summary>
    /// <param name="array">The elements.</param>
    /// <returns>An array with the elements of <paramref name="array"/>, in the same order.</returns>
    /// <exception cref="T:System.ArgumentNullException"><paramref name="array"/> is <see langword="null"/>.</exception>
    /// <remarks>O(n). Later changes to <paramref name="array"/> do not change the result.</remarks>
    [<CompiledName "OfArray">]
    val ofArray : array : 'T array -> EquatableArray<'T>

    /// <summary>Wraps <paramref name="items"/> without a copy.</summary>
    /// <param name="items">The elements. A default array gives the empty array.</param>
    /// <returns>An array with the elements of <paramref name="items"/>.</returns>
    /// <remarks>O(1).</remarks>
    [<CompiledName "OfImmutableArray">]
    val ofImmutableArray : items : ImmutableArray<'T> -> EquatableArray<'T>

    /// <summary>Copies the elements of <paramref name="list"/> into a new array.</summary>
    /// <param name="list">The elements.</param>
    /// <returns>An array with the elements of <paramref name="list"/>, in the same order.</returns>
    /// <remarks>O(n).</remarks>
    [<CompiledName "OfList">]
    val ofList : list : 'T list -> EquatableArray<'T>

    /// <summary>Copies the elements of <paramref name="source"/> into a new array.</summary>
    /// <param name="source">The elements.</param>
    /// <returns>An array with the elements of <paramref name="source"/>, in the same order.</returns>
    /// <exception cref="T:System.ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="T:System.InvalidOperationException">
    /// <paramref name="source"/> is a default <see cref="T:System.Collections.Immutable.ImmutableArray`1"/>.
    /// </exception>
    /// <remarks>
    /// O(n). An <see cref="T:System.Collections.Immutable.ImmutableArray`1"/> given as a sequence is not copied, because it
    /// cannot change.
    /// </remarks>
    [<CompiledName "OfSeq">]
    val ofSeq : source : seq<'T> -> EquatableArray<'T>

    /// <summary>Returns an array with one element.</summary>
    /// <param name="value">The element.</param>
    /// <returns>An array that holds <paramref name="value"/> only.</returns>
    [<CompiledName "Singleton">]
    val singleton : value : 'T -> EquatableArray<'T>

    /// <summary>
    /// Wraps <paramref name="array"/> without a copy, as
    /// <see cref="M:System.Runtime.InteropServices.ImmutableCollectionsMarshal.AsImmutableArray``1(``0[])"/> does.
    /// </summary>
    /// <param name="array">The elements, in an array that nothing writes to again.</param>
    /// <returns>An array with the elements of <paramref name="array"/>.</returns>
    /// <exception cref="T:System.ArgumentNullException"><paramref name="array"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// <para>O(1).</para>
    /// <para>
    /// The caller gives the array away. The result stays immutable only while nothing writes to
    /// <paramref name="array"/>. A later write changes the elements, the equality and the hash code of the result, also
    /// where the result is already a key of a dictionary or an element of a set. Use
    /// <see cref="M:FSharp.Collections.Immutable.EquatableArrayModule.OfArray``1(``0[])"/> to copy.
    /// </para>
    /// </remarks>
    [<CompiledName "UnsafeOfArray">]
    val unsafeOfArray : array : 'T array -> EquatableArray<'T>

    // ---- Properties ----

    /// <summary>Returns <see langword="true"/> when <paramref name="array"/> has no elements.</summary>
    /// <param name="array">The array.</param>
    /// <remarks>O(1).</remarks>
    [<CompiledName "IsEmpty">]
    val isEmpty : array : EquatableArray<'T> -> bool

    /// <summary>Returns the number of elements of <paramref name="array"/>.</summary>
    /// <param name="array">The array.</param>
    /// <remarks>O(1).</remarks>
    [<CompiledName "Length">]
    val length : array : EquatableArray<'T> -> int

    // ---- Transformation ----

    /// <summary>
    /// Returns a new array with the results of <paramref name="mapping"/> for each element of
    /// <paramref name="array"/>, in the same order.
    /// </summary>
    /// <param name="mapping">The function to apply to each element.</param>
    /// <param name="array">The array.</param>
    /// <remarks>O(n).</remarks>
    [<CompiledName "Map">]
    val map : mapping : ('T -> 'U) -> array : EquatableArray<'T> -> EquatableArray<'U>

    // ---- Conversion ----

    /// <summary>
    /// Returns the elements of <paramref name="array"/> as an
    /// <see cref="T:System.Collections.Immutable.ImmutableArray`1"/>, without a copy.
    /// </summary>
    /// <param name="array">The array.</param>
    /// <returns>The elements. The value is never a default array.</returns>
    /// <remarks>O(1).</remarks>
    [<CompiledName "ToImmutableArray">]
    val toImmutableArray : array : EquatableArray<'T> -> ImmutableArray<'T>
