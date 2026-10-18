namespace FSharp.Collections.Immutable

open System
open System.Collections.Generic
open System.Collections.Immutable

/// <summary>An immutable array, <see cref="T:System.Collections.Immutable.ImmutableArray`1"/>.</summary>
/// <remarks>The name comes from a similar construct in the sources of the F# compiler.</remarks>
type FlatList<'T> = ImmutableArray<'T>

/// <summary>Functions for <see cref="T:FSharp.Collections.Immutable.FlatList`1"/>, the immutable array.</summary>
/// <remarks>
/// <para>
/// The module mirrors <see cref="T:Microsoft.FSharp.Collections.ArrayModule"/>: each function has the semantics, the
/// argument checks and the exceptions of the function with the same name in that module (ADR 0001). Functions that
/// FSharp.Core lacks wrap a member of <see cref="T:System.Collections.Immutable.ImmutableArray`1"/> and throw what that
/// member throws.
/// </para>
/// <para>
/// A default immutable array has no underlying array. Every function except
/// <see cref="M:FSharp.Collections.Immutable.FlatList.IsDefault``1(System.Collections.Immutable.ImmutableArray{``0})"/>
/// and <see cref="M:FSharp.Collections.Immutable.FlatList.IsDefaultOrEmpty``1(System.Collections.Immutable.ImmutableArray{``0})"/>
/// throws <see cref="T:System.ArgumentNullException"/> for it, as <see cref="T:Microsoft.FSharp.Collections.ArrayModule"/>
/// does for a <see langword="null"/> array.
/// </para>
/// <para>
/// Functions that return an optional value return a <see cref="T:Microsoft.FSharp.Core.FSharpValueOption`1"/>, and functions
/// that return tuples return struct tuples. The namespaces <see cref="N:FSharp.Collections.Immutable.Option"/> and
/// <see cref="N:FSharp.Collections.Immutable.Tuple"/> contain a module with the same name whose functions use options
/// and reference tuples instead.
/// </para>
/// </remarks>
[<RequireQualifiedAccess>]
module FlatList =

    // ---- Creation ----

    /// <summary>Creates a flat list whose elements are all the given value.</summary>
    /// <param name="count">The length of the flat list.</param>
    /// <param name="value">The value of every element.</param>
    /// <returns>The created flat list.</returns>
    /// <exception cref="T:System.ArgumentException">Thrown when <paramref name="count"/> is negative.</exception>
    /// <remarks>O(n).</remarks>
    [<CompiledName "Create">]
    val create : count : int -> value : 'T -> FlatList<'T>

    /// <summary>The empty flat list.</summary>
    /// <remarks>O(1). The value is <see cref="P:System.Collections.Immutable.ImmutableArray`1.Empty"/>.</remarks>
    [<GeneralizableValue>]
    [<CompiledName "Empty">]
    val empty<'T> : FlatList<'T>

    /// <summary>Creates a flat list from a function of the index.</summary>
    /// <param name="count">The length of the flat list.</param>
    /// <param name="initializer">The function that computes the element at each index, called in index order.</param>
    /// <returns>The created flat list.</returns>
    /// <exception cref="T:System.ArgumentException">Thrown when <paramref name="count"/> is negative.</exception>
    /// <remarks>O(n).</remarks>
    [<CompiledName "Initialize">]
    val init : count : int -> initializer : (int -> 'T) -> FlatList<'T>

    /// <summary>Creates a flat list from a copy of an array.</summary>
    /// <param name="array">The input array.</param>
    /// <returns>The flat list of the elements of the array.</returns>
    /// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="array"/> is <see langword="null"/>.</exception>
    /// <remarks>O(n). Later changes to the array do not change the flat list.</remarks>
    [<CompiledName "OfArray">]
    val ofArray : array : 'T array -> FlatList<'T>

    /// <summary>Creates a flat list from a list.</summary>
    /// <param name="list">The input list.</param>
    /// <returns>The flat list of the elements of the list.</returns>
    /// <remarks>O(n).</remarks>
    [<CompiledName "OfList">]
    val ofList : list : 'T list -> FlatList<'T>

    /// <summary>Creates a flat list from a copy of a read-only span.</summary>
    /// <param name="span">The input span.</param>
    /// <returns>The flat list of the elements of the span.</returns>
    /// <remarks>O(n). The function wraps <see cref="M:System.Collections.Immutable.ImmutableArray.Create``1(System.ReadOnlySpan{``0})"/>.</remarks>
    [<CompiledName "OfReadOnlySpan">]
    val ofReadOnlySpan : span : ReadOnlySpan<'T> -> FlatList<'T>

    /// <summary>Creates a flat list from a sequence.</summary>
    /// <param name="source">The input sequence.</param>
    /// <returns>The flat list of the elements of the sequence.</returns>
    /// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <remarks>O(n).</remarks>
    [<CompiledName "OfSeq">]
    val ofSeq : source : seq<'T> -> FlatList<'T>

    /// <summary>Creates a flat list from a copy of a span.</summary>
    /// <param name="span">The input span.</param>
    /// <returns>The flat list of the elements of the span.</returns>
    /// <remarks>O(n). The function wraps <see cref="M:System.Collections.Immutable.ImmutableArray.Create``1(System.Span{``0})"/>.</remarks>
    [<CompiledName "OfSpan">]
    val ofSpan : span : Span<'T> -> FlatList<'T>

    /// <summary>Creates a flat list whose elements are all the given value.</summary>
    /// <param name="count">The length of the flat list.</param>
    /// <param name="initial">The value of every element.</param>
    /// <returns>The created flat list.</returns>
    /// <exception cref="T:System.ArgumentException">Thrown when <paramref name="count"/> is negative.</exception>
    /// <remarks>O(n).</remarks>
    [<CompiledName "Replicate">]
    val replicate : count : int -> initial : 'T -> FlatList<'T>

    /// <summary>Creates a flat list of one element.</summary>
    /// <param name="value">The element.</param>
    /// <returns>The flat list of one element.</returns>
    /// <remarks>O(1).</remarks>
    [<CompiledName "Singleton">]
    val singleton : value : 'T -> FlatList<'T>

    /// <summary>Creates a flat list from a generator, which returns the next element and the next state.</summary>
    /// <param name="generator">The function that returns the next element and state, or <c>ValueNone</c> to stop.</param>
    /// <param name="state">The initial state.</param>
    /// <returns>The generated flat list.</returns>
    /// <remarks>O(n).</remarks>
    [<CompiledName "Unfold">]
    val unfold : generator : ('State -> struct ('T * 'State) voption) -> state : 'State -> FlatList<'T>

    // ---- Builder ----

    /// <summary>Creates a flat list with a builder that an action fills.</summary>
    /// <param name="action">The action that adds the elements to the builder.</param>
    /// <returns>The flat list of the elements that the action added.</returns>
    /// <remarks>O(n).</remarks>
    [<CompiledName "Build">]
    val build : action : (FlatList<'T>.Builder -> unit) -> FlatList<'T>

    /// <summary>Creates an empty builder.</summary>
    /// <returns>A builder of capacity 8.</returns>
    /// <remarks>O(1). The function wraps <see cref="M:System.Collections.Immutable.ImmutableArray.CreateBuilder``1"/>.</remarks>
    [<CompiledName "CreateBuilder">]
    val builder : unit -> FlatList<'T>.Builder

    /// <summary>Creates an empty builder of a given capacity.</summary>
    /// <param name="capacity">The initial capacity.</param>
    /// <returns>The builder.</returns>
    /// <exception cref="T:System.ArgumentOutOfRangeException">Thrown when <paramref name="capacity"/> is negative.</exception>
    /// <remarks>O(capacity). The function wraps <see cref="M:System.Collections.Immutable.ImmutableArray.CreateBuilder``1(System.Int32)"/>.</remarks>
    [<CompiledName "CreateBuilderWithCapacity">]
    val builderWith : capacity : int -> FlatList<'T>.Builder

    /// <summary>Returns the contents of a full builder as a flat list without a copy and empties the builder.</summary>
    /// <param name="builder">The builder, whose count must equal its capacity.</param>
    /// <returns>The flat list of the elements of the builder.</returns>
    /// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="builder"/> is <see langword="null"/>.</exception>
    /// <exception cref="T:System.InvalidOperationException">Thrown when the count of the builder differs from its capacity.</exception>
    /// <remarks>O(1). The function wraps <see cref="M:System.Collections.Immutable.ImmutableArray`1.Builder.MoveToImmutable"/>.</remarks>
    [<CompiledName "MoveFromBuilder">]
    val moveFromBuilder : builder : FlatList<'T>.Builder -> FlatList<'T>

    /// <summary>Returns a copy of the contents of a builder as a flat list.</summary>
    /// <param name="builder">The builder, which stays unchanged.</param>
    /// <returns>The flat list of the elements of the builder.</returns>
    /// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="builder"/> is <see langword="null"/>.</exception>
    /// <remarks>O(n). The function wraps <see cref="M:System.Collections.Immutable.ImmutableArray`1.Builder.ToImmutable"/>.</remarks>
    [<CompiledName "OfBuilder">]
    val ofBuilder : builder : FlatList<'T>.Builder -> FlatList<'T>

    /// <summary>Creates a builder that contains a copy of the elements of a flat list.</summary>
    /// <param name="array">The input flat list.</param>
    /// <returns>The builder.</returns>
    /// <remarks>O(n). The function wraps <see cref="M:System.Collections.Immutable.ImmutableArray`1.ToBuilder"/>.</remarks>
    [<CompiledName "ToBuilder">]
    val toBuilder : array : FlatList<'T> -> FlatList<'T>.Builder

    /// <summary>Creates a flat list from a builder over the elements of a flat list, after an action changes the builder.</summary>
    /// <param name="action">The action that changes the builder.</param>
    /// <param name="array">The input flat list, which stays unchanged.</param>
    /// <returns>The flat list of the elements of the changed builder.</returns>
    /// <remarks>O(n).</remarks>
    [<CompiledName "Update">]
    val update : action : (FlatList<'T>.Builder -> unit) -> array : FlatList<'T> -> FlatList<'T>

    // ---- Properties ----

    /// <summary>Returns whether the flat list has no elements.</summary>
    /// <param name="array">The input flat list.</param>
    /// <returns><see langword="true"/> when the flat list is empty.</returns>
    /// <remarks>O(1).</remarks>
    [<CompiledName "IsEmpty">]
    val isEmpty : array : FlatList<'T> -> bool

    /// <summary>Returns the number of elements of the flat list.</summary>
    /// <param name="array">The input flat list.</param>
    /// <returns>The length of the flat list.</returns>
    /// <remarks>O(1).</remarks>
    [<CompiledName "Length">]
    val length : array : FlatList<'T> -> int

    // ---- Access ----

    /// <summary>Returns the only element of the flat list.</summary>
    /// <param name="array">The input flat list.</param>
    /// <returns>The only element.</returns>
    /// <exception cref="T:System.ArgumentException">Thrown when the flat list does not have exactly one element.</exception>
    /// <remarks>O(1).</remarks>
    [<CompiledName "ExactlyOne">]
    val exactlyOne : array : FlatList<'T> -> 'T

    /// <summary>Returns the first element of the flat list.</summary>
    /// <param name="array">The input flat list.</param>
    /// <returns>The first element.</returns>
    /// <exception cref="T:System.ArgumentException">Thrown when the flat list is empty.</exception>
    /// <remarks>O(1).</remarks>
    [<CompiledName "Head">]
    val head : array : FlatList<'T> -> 'T

    /// <summary>Returns the element at an index.</summary>
    /// <param name="index">The index of the element.</param>
    /// <param name="array">The input flat list.</param>
    /// <returns>The element at the index.</returns>
    /// <exception cref="T:System.IndexOutOfRangeException">Thrown when the index is negative or not less than the length.</exception>
    /// <remarks>O(1).</remarks>
    [<CompiledName "Item">]
    val item : index : int -> array : FlatList<'T> -> 'T

    /// <summary>Returns the last element of the flat list.</summary>
    /// <param name="array">The input flat list.</param>
    /// <returns>The last element.</returns>
    /// <exception cref="T:System.ArgumentException">Thrown when the flat list is empty.</exception>
    /// <remarks>O(1).</remarks>
    [<CompiledName "Last">]
    val last : array : FlatList<'T> -> 'T

    /// <summary>Returns the flat list without its first element.</summary>
    /// <param name="array">The input flat list.</param>
    /// <returns>A copy of the elements after the first one.</returns>
    /// <exception cref="T:System.ArgumentException">Thrown when the flat list is empty.</exception>
    /// <remarks>O(n).</remarks>
    [<CompiledName "Tail">]
    val tail : array : FlatList<'T> -> FlatList<'T>

    /// <summary>Returns the only element of the flat list, or <c>ValueNone</c> when it does not have exactly one element.</summary>
    /// <param name="array">The input flat list.</param>
    /// <returns>The only element, or <c>ValueNone</c>.</returns>
    /// <remarks>O(1).</remarks>
    [<CompiledName "TryExactlyOne">]
    val tryExactlyOne : array : FlatList<'T> -> 'T voption

    /// <summary>Returns the first element of the flat list, or <c>ValueNone</c> when it is empty.</summary>
    /// <param name="array">The input flat list.</param>
    /// <returns>The first element, or <c>ValueNone</c>.</returns>
    /// <remarks>O(1).</remarks>
    [<CompiledName "TryHead">]
    val tryHead : array : FlatList<'T> -> 'T voption

    /// <summary>Returns the element at an index, or <c>ValueNone</c> when the index is out of range.</summary>
    /// <param name="index">The index of the element.</param>
    /// <param name="array">The input flat list.</param>
    /// <returns>The element at the index, or <c>ValueNone</c>.</returns>
    /// <remarks>O(1).</remarks>
    [<CompiledName "TryItem">]
    val tryItem : index : int -> array : FlatList<'T> -> 'T voption

    /// <summary>Returns the last element of the flat list, or <c>ValueNone</c> when it is empty.</summary>
    /// <param name="array">The input flat list.</param>
    /// <returns>The last element, or <c>ValueNone</c>.</returns>
    /// <remarks>O(1).</remarks>
    [<CompiledName "TryLast">]
    val tryLast : array : FlatList<'T> -> 'T voption

    /// <summary>Returns the flat list without its first element, or <c>ValueNone</c> when it is empty.</summary>
    /// <param name="array">The input flat list.</param>
    /// <returns>A copy of the elements after the first one, or <c>ValueNone</c>.</returns>
    /// <remarks>O(n).</remarks>
    [<CompiledName "TryTail">]
    val tryTail : array : FlatList<'T> -> FlatList<'T> voption

    // ---- Search ----

    /// <summary>Searches a sorted flat list for a value with the binary search algorithm.</summary>
    /// <param name="value">The value to search for.</param>
    /// <param name="array">The input flat list, sorted in the order of F# generic comparison.</param>
    /// <returns>
    /// The index of the value, or the bitwise complement of the index of the first larger element, or the bitwise
    /// complement of the length when no element is larger.
    /// </returns>
    /// <remarks>
    /// O(log n). The function compares with <see cref="P:Microsoft.FSharp.Core.LanguagePrimitives.FastGenericComparer``1"/>,
    /// the order that <see cref="M:FSharp.Collections.Immutable.FlatList.Sort``1(System.Collections.Immutable.ImmutableArray{``0})"/> produces.
    /// </remarks>
    [<CompiledName "BinarySearch">]
    val binarySearch : value : 'T -> array : FlatList<'T> -> int when 'T : comparison

    /// <summary>Returns whether the flat list contains an element equal to a value, in F# structural equality.</summary>
    /// <param name="value">The value to find.</param>
    /// <param name="array">The input flat list.</param>
    /// <returns><see langword="true"/> when an element equals the value.</returns>
    /// <remarks>O(n).</remarks>
    [<CompiledName "Contains">]
    val contains : value : 'T -> array : FlatList<'T> -> bool when 'T : equality

    /// <summary>Returns whether any element satisfies the predicate.</summary>
    /// <param name="predicate">The function to test the elements.</param>
    /// <param name="array">The input flat list.</param>
    /// <returns><see langword="true"/> when an element satisfies the predicate.</returns>
    /// <remarks>O(n).</remarks>
    [<CompiledName "Exists">]
    val exists : predicate : ('T -> bool) -> array : FlatList<'T> -> bool

    /// <summary>Returns whether any pair of elements at the same index satisfies the predicate.</summary>
    /// <param name="predicate">The function to test the pairs.</param>
    /// <param name="array1">The first flat list.</param>
    /// <param name="array2">The second flat list.</param>
    /// <returns><see langword="true"/> when a pair satisfies the predicate.</returns>
    /// <exception cref="T:System.ArgumentException">Thrown when the flat lists have different lengths.</exception>
    /// <remarks>O(n).</remarks>
    [<CompiledName "Exists2">]
    val exists2 : predicate : ('T1 -> 'T2 -> bool) -> array1 : FlatList<'T1> -> array2 : FlatList<'T2> -> bool

    /// <summary>Returns the first element that satisfies the predicate.</summary>
    /// <param name="predicate">The function to test the elements.</param>
    /// <param name="array">The input flat list.</param>
    /// <returns>The first element that satisfies the predicate.</returns>
    /// <exception cref="T:System.Collections.Generic.KeyNotFoundException">Thrown when no element satisfies the predicate.</exception>
    /// <remarks>O(n).</remarks>
    [<CompiledName "Find">]
    val find : predicate : ('T -> bool) -> array : FlatList<'T> -> 'T

    /// <summary>Returns the last element that satisfies the predicate.</summary>
    /// <param name="predicate">The function to test the elements.</param>
    /// <param name="array">The input flat list.</param>
    /// <returns>The last element that satisfies the predicate.</returns>
    /// <exception cref="T:System.Collections.Generic.KeyNotFoundException">Thrown when no element satisfies the predicate.</exception>
    /// <remarks>O(n).</remarks>
    [<CompiledName "FindBack">]
    val findBack : predicate : ('T -> bool) -> array : FlatList<'T> -> 'T

    /// <summary>Returns the index of the first element that satisfies the predicate.</summary>
    /// <param name="predicate">The function to test the elements.</param>
    /// <param name="array">The input flat list.</param>
    /// <returns>The index of the first element that satisfies the predicate.</returns>
    /// <exception cref="T:System.Collections.Generic.KeyNotFoundException">Thrown when no element satisfies the predicate.</exception>
    /// <remarks>O(n).</remarks>
    [<CompiledName "FindIndex">]
    val findIndex : predicate : ('T -> bool) -> array : FlatList<'T> -> int

    /// <summary>Returns the index of the last element that satisfies the predicate.</summary>
    /// <param name="predicate">The function to test the elements.</param>
    /// <param name="array">The input flat list.</param>
    /// <returns>The index of the last element that satisfies the predicate.</returns>
    /// <exception cref="T:System.Collections.Generic.KeyNotFoundException">Thrown when no element satisfies the predicate.</exception>
    /// <remarks>O(n).</remarks>
    [<CompiledName "FindIndexBack">]
    val findIndexBack : predicate : ('T -> bool) -> array : FlatList<'T> -> int

    /// <summary>Returns whether all elements satisfy the predicate.</summary>
    /// <param name="predicate">The function to test the elements.</param>
    /// <param name="array">The input flat list.</param>
    /// <returns><see langword="true"/> when every element satisfies the predicate.</returns>
    /// <remarks>O(n).</remarks>
    [<CompiledName "ForAll">]
    val forall : predicate : ('T -> bool) -> array : FlatList<'T> -> bool

    /// <summary>Returns whether all pairs of elements at the same index satisfy the predicate.</summary>
    /// <param name="predicate">The function to test the pairs.</param>
    /// <param name="array1">The first flat list.</param>
    /// <param name="array2">The second flat list.</param>
    /// <returns><see langword="true"/> when every pair satisfies the predicate.</returns>
    /// <exception cref="T:System.ArgumentException">Thrown when the flat lists have different lengths.</exception>
    /// <remarks>O(n).</remarks>
    [<CompiledName "ForAll2">]
    val forall2 : predicate : ('T1 -> 'T2 -> bool) -> array1 : FlatList<'T1> -> array2 : FlatList<'T2> -> bool

    /// <summary>Returns the index of the first element equal to a value, in <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>.</summary>
    /// <param name="value">The value to find.</param>
    /// <param name="array">The input flat list.</param>
    /// <returns>The index of the first equal element, or -1.</returns>
    /// <remarks>O(n). The function wraps <see cref="M:System.Collections.Immutable.ImmutableArray`1.IndexOf(`0)"/>.</remarks>
    [<CompiledName "IndexOf">]
    val indexOf : value : 'T -> array : FlatList<'T> -> int

    /// <summary>Returns the index of the first element equal to a value in a given equality comparer.</summary>
    /// <param name="comparer">The equality comparer.</param>
    /// <param name="value">The value to find.</param>
    /// <param name="array">The input flat list.</param>
    /// <returns>The index of the first equal element, or -1.</returns>
    /// <remarks>
    /// O(n). The function wraps
    /// <see cref="M:System.Collections.Immutable.ImmutableArray`1.IndexOf(`0,System.Int32,System.Int32,System.Collections.Generic.IEqualityComparer{`0})"/>.
    /// </remarks>
    [<CompiledName "IndexOfWith">]
    val indexOfWith : comparer : IEqualityComparer<'T> -> value : 'T -> array : FlatList<'T> -> int

    /// <summary>Returns the index of the last element equal to a value, in <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>.</summary>
    /// <param name="value">The value to find.</param>
    /// <param name="array">The input flat list.</param>
    /// <returns>The index of the last equal element, or -1.</returns>
    /// <remarks>O(n). The function wraps <see cref="M:System.Collections.Immutable.ImmutableArray`1.LastIndexOf(`0)"/>.</remarks>
    [<CompiledName "LastIndexOf">]
    val lastIndexOf : value : 'T -> array : FlatList<'T> -> int

    /// <summary>Returns the index of the last element equal to a value in a given equality comparer.</summary>
    /// <param name="comparer">The equality comparer.</param>
    /// <param name="value">The value to find.</param>
    /// <param name="array">The input flat list.</param>
    /// <returns>The index of the last equal element, or -1.</returns>
    /// <remarks>
    /// O(n). The function wraps
    /// <see cref="M:System.Collections.Immutable.ImmutableArray`1.LastIndexOf(`0,System.Int32,System.Int32,System.Collections.Generic.IEqualityComparer{`0})"/>.
    /// </remarks>
    [<CompiledName "LastIndexOfWith">]
    val lastIndexOfWith : comparer : IEqualityComparer<'T> -> value : 'T -> array : FlatList<'T> -> int

    /// <summary>Returns the first value that the chooser returns for the elements.</summary>
    /// <param name="chooser">The function that returns a value, or <c>ValueNone</c> to go on.</param>
    /// <param name="array">The input flat list.</param>
    /// <returns>The first chosen value.</returns>
    /// <exception cref="T:System.Collections.Generic.KeyNotFoundException">Thrown when the chooser returns <c>ValueNone</c> for every element.</exception>
    /// <remarks>O(n).</remarks>
    [<CompiledName "Pick">]
    val pick : chooser : ('T -> 'U voption) -> array : FlatList<'T> -> 'U

    /// <summary>Returns the first element that satisfies the predicate, or <c>ValueNone</c>.</summary>
    /// <param name="predicate">The function to test the elements.</param>
    /// <param name="array">The input flat list.</param>
    /// <returns>The first element that satisfies the predicate, or <c>ValueNone</c>.</returns>
    /// <remarks>O(n).</remarks>
    [<CompiledName "TryFind">]
    val tryFind : predicate : ('T -> bool) -> array : FlatList<'T> -> 'T voption

    /// <summary>Returns the last element that satisfies the predicate, or <c>ValueNone</c>.</summary>
    /// <param name="predicate">The function to test the elements.</param>
    /// <param name="array">The input flat list.</param>
    /// <returns>The last element that satisfies the predicate, or <c>ValueNone</c>.</returns>
    /// <remarks>O(n).</remarks>
    [<CompiledName "TryFindBack">]
    val tryFindBack : predicate : ('T -> bool) -> array : FlatList<'T> -> 'T voption

    /// <summary>Returns the index of the first element that satisfies the predicate, or <c>ValueNone</c>.</summary>
    /// <param name="predicate">The function to test the elements.</param>
    /// <param name="array">The input flat list.</param>
    /// <returns>The index of the first element that satisfies the predicate, or <c>ValueNone</c>.</returns>
    /// <remarks>O(n).</remarks>
    [<CompiledName "TryFindIndex">]
    val tryFindIndex : predicate : ('T -> bool) -> array : FlatList<'T> -> int voption

    /// <summary>Returns the index of the last element that satisfies the predicate, or <c>ValueNone</c>.</summary>
    /// <param name="predicate">The function to test the elements.</param>
    /// <param name="array">The input flat list.</param>
    /// <returns>The index of the last element that satisfies the predicate, or <c>ValueNone</c>.</returns>
    /// <remarks>O(n).</remarks>
    [<CompiledName "TryFindIndexBack">]
    val tryFindIndexBack : predicate : ('T -> bool) -> array : FlatList<'T> -> int voption

    /// <summary>Returns the first value that the chooser returns for the elements, or <c>ValueNone</c>.</summary>
    /// <param name="chooser">The function that returns a value, or <c>ValueNone</c> to go on.</param>
    /// <param name="array">The input flat list.</param>
    /// <returns>The first chosen value, or <c>ValueNone</c>.</returns>
    /// <remarks>O(n).</remarks>
    [<CompiledName "TryPick">]
    val tryPick : chooser : ('T -> 'U voption) -> array : FlatList<'T> -> 'U voption

    // ---- Transformation ----

    /// <summary>Returns a flat list with a value added after the last element.</summary>
    /// <param name="value">The value to add.</param>
    /// <param name="array">The input flat list.</param>
    /// <returns>A flat list of the elements and the value.</returns>
    /// <remarks>O(n). The function wraps <see cref="M:System.Collections.Immutable.ImmutableArray`1.Add(`0)"/>.</remarks>
    [<CompiledName "Add">]
    val add : value : 'T -> array : FlatList<'T> -> FlatList<'T>

    /// <summary>Returns every element of the first flat list paired with every element of the second one.</summary>
    /// <param name="array1">The first flat list.</param>
    /// <param name="array2">The second flat list.</param>
    /// <returns>The pairs, in the order of the first flat list and then of the second one.</returns>
    /// <remarks>O(n × m).</remarks>
    [<CompiledName "AllPairs">]
    val allPairs : array1 : FlatList<'T1> -> array2 : FlatList<'T2> -> FlatList<struct ('T1 * 'T2)>

    /// <summary>Returns a flat list of the elements of the first flat list followed by the elements of the second one.</summary>
    /// <param name="array1">The first flat list.</param>
    /// <param name="array2">The second flat list.</param>
    /// <returns>The concatenation.</returns>
    /// <remarks>O(n + m). When one of the flat lists is empty, the function returns the other one.</remarks>
    [<CompiledName "Append">]
    val append : array1 : FlatList<'T> -> array2 : FlatList<'T> -> FlatList<'T>

    /// <summary>Returns the values that the chooser returns for the elements.</summary>
    /// <param name="chooser">The function that returns a value, or <c>ValueNone</c> to skip the element.</param>
    /// <param name="array">The input flat list.</param>
    /// <returns>The chosen values.</returns>
    /// <remarks>O(n).</remarks>
    [<CompiledName "Choose">]
    val choose : chooser : ('T -> 'U voption) -> array : FlatList<'T> -> FlatList<'U>

    /// <summary>Divides the flat list into chunks of a given size.</summary>
    /// <param name="chunkSize">The maximum size of a chunk.</param>
    /// <param name="array">The input flat list.</param>
    /// <returns>The chunks; the last one can be smaller.</returns>
    /// <exception cref="T:System.ArgumentException">Thrown when <paramref name="chunkSize"/> is not positive.</exception>
    /// <remarks>O(n). The chunks are copies.</remarks>
    [<CompiledName "ChunkBySize">]
    val chunkBySize : chunkSize : int -> array : FlatList<'T> -> FlatList<FlatList<'T>>

    /// <summary>Concatenates the flat lists that a function returns for the elements.</summary>
    /// <param name="mapping">The function that returns a flat list for each element.</param>
    /// <param name="array">The input flat list.</param>
    /// <returns>The concatenation of the returned flat lists.</returns>
    /// <remarks>O(n + the total length of the returned flat lists).</remarks>
    [<CompiledName "Collect">]
    val collect : mapping : ('T -> FlatList<'U>) -> array : FlatList<'T> -> FlatList<'U>

    /// <summary>Concatenates a sequence of flat lists.</summary>
    /// <param name="arrays">The flat lists.</param>
    /// <returns>The concatenation.</returns>
    /// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="arrays"/> is <see langword="null"/>.</exception>
    /// <remarks>O(the total length).</remarks>
    [<CompiledName "Concat">]
    val concat : arrays : seq<FlatList<'T>> -> FlatList<'T>

    /// <summary>Returns the elements that satisfy the predicate.</summary>
    /// <param name="predicate">The function to test the elements.</param>
    /// <param name="array">The input flat list.</param>
    /// <returns>The elements that satisfy the predicate, in their order.</returns>
    /// <remarks>O(n).</remarks>
    [<CompiledName "Filter">]
    val filter : predicate : ('T -> bool) -> array : FlatList<'T> -> FlatList<'T>

    /// <summary>Returns every element with its index.</summary>
    /// <param name="array">The input flat list.</param>
    /// <returns>The pairs of index and element.</returns>
    /// <remarks>O(n).</remarks>
    [<CompiledName "Indexed">]
    val indexed : array : FlatList<'T> -> FlatList<struct (int * 'T)>

    /// <summary>Returns a flat list with a value inserted at an index.</summary>
    /// <param name="index">The index of the inserted value, from 0 to the length.</param>
    /// <param name="value">The value to insert.</param>
    /// <param name="source">The input flat list.</param>
    /// <returns>The flat list with the value.</returns>
    /// <exception cref="T:System.ArgumentException">Thrown when the index is negative or greater than the length.</exception>
    /// <remarks>O(n).</remarks>
    [<CompiledName "InsertAt">]
    val insertAt : index : int -> value : 'T -> source : FlatList<'T> -> FlatList<'T>

    /// <summary>Returns a flat list with values inserted at an index.</summary>
    /// <param name="index">The index of the first inserted value, from 0 to the length.</param>
    /// <param name="values">The values to insert.</param>
    /// <param name="source">The input flat list.</param>
    /// <returns>The flat list with the values.</returns>
    /// <exception cref="T:System.ArgumentException">Thrown when the index is negative or greater than the length.</exception>
    /// <remarks>O(n + m).</remarks>
    [<CompiledName "InsertManyAt">]
    val insertManyAt : index : int -> values : seq<'T> -> source : FlatList<'T> -> FlatList<'T>

    /// <summary>Returns the results of a function applied to each element.</summary>
    /// <param name="mapping">The function to transform the elements.</param>
    /// <param name="array">The input flat list.</param>
    /// <returns>The results, in the order of the elements.</returns>
    /// <remarks>O(n).</remarks>
    [<CompiledName "Map">]
    val map : mapping : ('T -> 'U) -> array : FlatList<'T> -> FlatList<'U>

    /// <summary>Returns the results of a function applied to the pairs of elements at the same index.</summary>
    /// <param name="mapping">The function to transform the pairs.</param>
    /// <param name="array1">The first flat list.</param>
    /// <param name="array2">The second flat list.</param>
    /// <returns>The results.</returns>
    /// <exception cref="T:System.ArgumentException">Thrown when the flat lists have different lengths.</exception>
    /// <remarks>O(n).</remarks>
    [<CompiledName "Map2">]
    val map2 : mapping : ('T1 -> 'T2 -> 'U) -> array1 : FlatList<'T1> -> array2 : FlatList<'T2> -> FlatList<'U>

    /// <summary>Returns the results of a function applied to the triples of elements at the same index.</summary>
    /// <param name="mapping">The function to transform the triples.</param>
    /// <param name="array1">The first flat list.</param>
    /// <param name="array2">The second flat list.</param>
    /// <param name="array3">The third flat list.</param>
    /// <returns>The results.</returns>
    /// <exception cref="T:System.ArgumentException">Thrown when the flat lists have different lengths.</exception>
    /// <remarks>O(n).</remarks>
    [<CompiledName "Map3">]
    val map3 :
        mapping : ('T1 -> 'T2 -> 'T3 -> 'U) ->
        array1 : FlatList<'T1> ->
        array2 : FlatList<'T2> ->
        array3 : FlatList<'T3> ->
            FlatList<'U>

    /// <summary>Returns the results of a function applied to each element and its index.</summary>
    /// <param name="mapping">The function to transform the elements.</param>
    /// <param name="array">The input flat list.</param>
    /// <returns>The results.</returns>
    /// <remarks>O(n).</remarks>
    [<CompiledName "MapIndexed">]
    val mapi : mapping : (int -> 'T -> 'U) -> array : FlatList<'T> -> FlatList<'U>

    /// <summary>Returns the results of a function applied to the index and the pair of elements at that index.</summary>
    /// <param name="mapping">The function to transform the pairs.</param>
    /// <param name="array1">The first flat list.</param>
    /// <param name="array2">The second flat list.</param>
    /// <returns>The results.</returns>
    /// <exception cref="T:System.ArgumentException">Thrown when the flat lists have different lengths.</exception>
    /// <remarks>O(n).</remarks>
    [<CompiledName "MapIndexed2">]
    val mapi2 : mapping : (int -> 'T1 -> 'T2 -> 'U) -> array1 : FlatList<'T1> -> array2 : FlatList<'T2> -> FlatList<'U>

    /// <summary>Returns every element with its successor.</summary>
    /// <param name="array">The input flat list.</param>
    /// <returns>The pairs of neighbours; empty for fewer than two elements.</returns>
    /// <remarks>O(n).</remarks>
    [<CompiledName "Pairwise">]
    val pairwise : array : FlatList<'T> -> FlatList<struct ('T * 'T)>

    /// <summary>Returns the elements moved to the indices that a function computes.</summary>
    /// <param name="indexMap">The function that maps the index of each element to its new index.</param>
    /// <param name="array">The input flat list.</param>
    /// <returns>The permuted flat list.</returns>
    /// <exception cref="T:System.ArgumentException">Thrown when the function does not compute a permutation.</exception>
    /// <remarks>O(n).</remarks>
    [<CompiledName "Permute">]
    val permute : indexMap : (int -> int) -> array : FlatList<'T> -> FlatList<'T>

    /// <summary>Returns a flat list without the element at an index.</summary>
    /// <param name="index">The index of the removed element.</param>
    /// <param name="source">The input flat list.</param>
    /// <returns>The flat list without the element.</returns>
    /// <exception cref="T:System.ArgumentException">Thrown when the index is out of range.</exception>
    /// <remarks>O(n).</remarks>
    [<CompiledName "RemoveAt">]
    val removeAt : index : int -> source : FlatList<'T> -> FlatList<'T>

    /// <summary>Returns a flat list without a range of elements.</summary>
    /// <param name="index">The index of the first removed element.</param>
    /// <param name="count">The number of removed elements.</param>
    /// <param name="source">The input flat list.</param>
    /// <returns>The flat list without the range.</returns>
    /// <exception cref="T:System.ArgumentException">Thrown when the range is out of range.</exception>
    /// <remarks>O(n).</remarks>
    [<CompiledName "RemoveManyAt">]
    val removeManyAt : index : int -> count : int -> source : FlatList<'T> -> FlatList<'T>

    /// <summary>Returns the elements in reverse order.</summary>
    /// <param name="array">The input flat list.</param>
    /// <returns>The reversed flat list.</returns>
    /// <remarks>O(n).</remarks>
    [<CompiledName "Reverse">]
    val rev : array : FlatList<'T> -> FlatList<'T>

    /// <summary>Returns the transpose of a sequence of flat lists of the same length.</summary>
    /// <param name="arrays">The flat lists.</param>
    /// <returns>A flat list whose element i holds the element i of every input flat list.</returns>
    /// <exception cref="T:System.ArgumentException">Thrown when the flat lists have different lengths.</exception>
    /// <remarks>O(the total length).</remarks>
    [<CompiledName "Transpose">]
    val transpose : arrays : seq<FlatList<'T>> -> FlatList<FlatList<'T>>

    /// <summary>Splits a flat list of pairs into two flat lists.</summary>
    /// <param name="array">The input flat list.</param>
    /// <returns>The first components and the second components.</returns>
    /// <remarks>O(n).</remarks>
    [<CompiledName "Unzip">]
    val unzip : array : FlatList<struct ('T1 * 'T2)> -> struct (FlatList<'T1> * FlatList<'T2>)

    /// <summary>Splits a flat list of triples into three flat lists.</summary>
    /// <param name="array">The input flat list.</param>
    /// <returns>The first, the second and the third components.</returns>
    /// <remarks>O(n).</remarks>
    [<CompiledName "Unzip3">]
    val unzip3 : array : FlatList<struct ('T1 * 'T2 * 'T3)> -> struct (FlatList<'T1> * FlatList<'T2> * FlatList<'T3>)

    /// <summary>Returns a flat list with the element at an index replaced by a value.</summary>
    /// <param name="index">The index of the replaced element.</param>
    /// <param name="value">The new value.</param>
    /// <param name="source">The input flat list.</param>
    /// <returns>The flat list with the new value.</returns>
    /// <exception cref="T:System.ArgumentException">Thrown when the index is out of range.</exception>
    /// <remarks>O(n).</remarks>
    [<CompiledName "UpdateAt">]
    val updateAt : index : int -> value : 'T -> source : FlatList<'T> -> FlatList<'T>

    /// <summary>Returns the elements that satisfy the predicate.</summary>
    /// <param name="predicate">The function to test the elements.</param>
    /// <param name="array">The input flat list.</param>
    /// <returns>The elements that satisfy the predicate, in their order.</returns>
    /// <remarks>O(n). A synonym of <see cref="M:FSharp.Collections.Immutable.FlatList.Filter``1(Microsoft.FSharp.Core.FSharpFunc{``0,System.Boolean},System.Collections.Immutable.ImmutableArray{``0})"/>.</remarks>
    [<CompiledName "Where">]
    val where : predicate : ('T -> bool) -> array : FlatList<'T> -> FlatList<'T>

    /// <summary>Returns the windows of a given size that slide over the flat list.</summary>
    /// <param name="windowSize">The size of a window.</param>
    /// <param name="array">The input flat list.</param>
    /// <returns>The windows; empty when the flat list is shorter than a window.</returns>
    /// <exception cref="T:System.ArgumentException">Thrown when <paramref name="windowSize"/> is not positive.</exception>
    /// <remarks>O(n × windowSize). The windows are copies.</remarks>
    [<CompiledName "Windowed">]
    val windowed : windowSize : int -> array : FlatList<'T> -> FlatList<FlatList<'T>>

    /// <summary>Combines two flat lists into a flat list of pairs.</summary>
    /// <param name="array1">The first flat list.</param>
    /// <param name="array2">The second flat list.</param>
    /// <returns>The pairs of elements at the same index.</returns>
    /// <exception cref="T:System.ArgumentException">Thrown when the flat lists have different lengths.</exception>
    /// <remarks>O(n).</remarks>
    [<CompiledName "Zip">]
    val zip : array1 : FlatList<'T1> -> array2 : FlatList<'T2> -> FlatList<struct ('T1 * 'T2)>

    /// <summary>Combines three flat lists into a flat list of triples.</summary>
    /// <param name="array1">The first flat list.</param>
    /// <param name="array2">The second flat list.</param>
    /// <param name="array3">The third flat list.</param>
    /// <returns>The triples of elements at the same index.</returns>
    /// <exception cref="T:System.ArgumentException">Thrown when the flat lists have different lengths.</exception>
    /// <remarks>O(n).</remarks>
    [<CompiledName "Zip3">]
    val zip3 : array1 : FlatList<'T1> -> array2 : FlatList<'T2> -> array3 : FlatList<'T3> -> FlatList<struct ('T1 * 'T2 * 'T3)>

    // ---- Folding ----

    /// <summary>Returns the average of the elements.</summary>
    /// <param name="array">The input flat list.</param>
    /// <returns>The average.</returns>
    /// <exception cref="T:System.ArgumentException">Thrown when the flat list is empty.</exception>
    /// <remarks>O(n).</remarks>
    [<CompiledName "Average">]
    val inline average :
        array : FlatList< ^T > -> ^T
            when ^T : (static member (+) : ^T * ^T -> ^T)
            and ^T : (static member DivideByInt : ^T * int -> ^T)
            and ^T : (static member Zero : ^T)

    /// <summary>Returns the average of the results of a function applied to each element.</summary>
    /// <param name="projection">The function to transform the elements.</param>
    /// <param name="array">The input flat list.</param>
    /// <returns>The average of the results.</returns>
    /// <exception cref="T:System.ArgumentException">Thrown when the flat list is empty.</exception>
    /// <remarks>O(n).</remarks>
    [<CompiledName "AverageBy">]
    val inline averageBy :
        projection : ('T -> ^U) -> array : FlatList<'T> -> ^U
            when ^U : (static member (+) : ^U * ^U -> ^U)
            and ^U : (static member DivideByInt : ^U * int -> ^U)
            and ^U : (static member Zero : ^U)

    /// <summary>Compares two flat lists element by element with a comparer.</summary>
    /// <param name="comparer">The function that compares two elements.</param>
    /// <param name="array1">The first flat list.</param>
    /// <param name="array2">The second flat list.</param>
    /// <returns>
    /// The first non-zero result of the comparer, or the comparison of the lengths when every common element compares equal.
    /// </returns>
    /// <remarks>O(n).</remarks>
    [<CompiledName "CompareWith">]
    val compareWith : comparer : ('T -> 'T -> int) -> array1 : FlatList<'T> -> array2 : FlatList<'T> -> int

    /// <summary>Applies a function to a state and each element, from the first element to the last one.</summary>
    /// <param name="folder">The function that computes the next state.</param>
    /// <param name="state">The initial state.</param>
    /// <param name="array">The input flat list.</param>
    /// <returns>The final state.</returns>
    /// <remarks>O(n).</remarks>
    [<CompiledName "Fold">]
    val fold : folder : ('State -> 'T -> 'State) -> state : 'State -> array : FlatList<'T> -> 'State

    /// <summary>Applies a function to a state and each pair of elements at the same index, from the first index to the last one.</summary>
    /// <param name="folder">The function that computes the next state.</param>
    /// <param name="state">The initial state.</param>
    /// <param name="array1">The first flat list.</param>
    /// <param name="array2">The second flat list.</param>
    /// <returns>The final state.</returns>
    /// <exception cref="T:System.ArgumentException">Thrown when the flat lists have different lengths.</exception>
    /// <remarks>O(n).</remarks>
    [<CompiledName "Fold2">]
    val fold2 :
        folder : ('State -> 'T1 -> 'T2 -> 'State) -> state : 'State -> array1 : FlatList<'T1> -> array2 : FlatList<'T2> -> 'State

    /// <summary>Applies a function to each element and a state, from the last element to the first one.</summary>
    /// <param name="folder">The function that computes the next state.</param>
    /// <param name="array">The input flat list.</param>
    /// <param name="state">The initial state.</param>
    /// <returns>The final state.</returns>
    /// <remarks>O(n).</remarks>
    [<CompiledName "FoldBack">]
    val foldBack : folder : ('T -> 'State -> 'State) -> array : FlatList<'T> -> state : 'State -> 'State

    /// <summary>Applies a function to each pair of elements at the same index and a state, from the last index to the first one.</summary>
    /// <param name="folder">The function that computes the next state.</param>
    /// <param name="array1">The first flat list.</param>
    /// <param name="array2">The second flat list.</param>
    /// <param name="state">The initial state.</param>
    /// <returns>The final state.</returns>
    /// <exception cref="T:System.ArgumentException">Thrown when the flat lists have different lengths.</exception>
    /// <remarks>O(n).</remarks>
    [<CompiledName "FoldBack2">]
    val foldBack2 :
        folder : ('T1 -> 'T2 -> 'State -> 'State) -> array1 : FlatList<'T1> -> array2 : FlatList<'T2> -> state : 'State -> 'State

    /// <summary>Applies an action to each element.</summary>
    /// <param name="action">The action.</param>
    /// <param name="array">The input flat list.</param>
    /// <remarks>O(n).</remarks>
    [<CompiledName "Iterate">]
    val iter : action : ('T -> unit) -> array : FlatList<'T> -> unit

    /// <summary>Applies an action to each pair of elements at the same index.</summary>
    /// <param name="action">The action.</param>
    /// <param name="array1">The first flat list.</param>
    /// <param name="array2">The second flat list.</param>
    /// <exception cref="T:System.ArgumentException">Thrown when the flat lists have different lengths.</exception>
    /// <remarks>O(n).</remarks>
    [<CompiledName "Iterate2">]
    val iter2 : action : ('T1 -> 'T2 -> unit) -> array1 : FlatList<'T1> -> array2 : FlatList<'T2> -> unit

    /// <summary>Applies an action to each element and its index.</summary>
    /// <param name="action">The action.</param>
    /// <param name="array">The input flat list.</param>
    /// <remarks>O(n).</remarks>
    [<CompiledName "IterateIndexed">]
    val iteri : action : (int -> 'T -> unit) -> array : FlatList<'T> -> unit

    /// <summary>Applies an action to the index and the pair of elements at that index.</summary>
    /// <param name="action">The action.</param>
    /// <param name="array1">The first flat list.</param>
    /// <param name="array2">The second flat list.</param>
    /// <exception cref="T:System.ArgumentException">Thrown when the flat lists have different lengths.</exception>
    /// <remarks>O(n).</remarks>
    [<CompiledName "IterateIndexed2">]
    val iteri2 : action : (int -> 'T1 -> 'T2 -> unit) -> array1 : FlatList<'T1> -> array2 : FlatList<'T2> -> unit

    /// <summary>Maps each element and threads a state through the mapping, from the first element to the last one.</summary>
    /// <param name="mapping">The function that returns the result for an element and the next state.</param>
    /// <param name="state">The initial state.</param>
    /// <param name="array">The input flat list.</param>
    /// <returns>The results and the final state.</returns>
    /// <remarks>O(n).</remarks>
    [<CompiledName "MapFold">]
    val mapFold :
        mapping : ('State -> 'T -> struct ('Result * 'State)) ->
        state : 'State ->
        array : FlatList<'T> ->
            struct (FlatList<'Result> * 'State)

    /// <summary>Maps each element and threads a state through the mapping, from the last element to the first one.</summary>
    /// <param name="mapping">The function that returns the result for an element and the next state.</param>
    /// <param name="array">The input flat list.</param>
    /// <param name="state">The initial state.</param>
    /// <returns>The results, in the order of the elements, and the final state.</returns>
    /// <remarks>O(n).</remarks>
    [<CompiledName "MapFoldBack">]
    val mapFoldBack :
        mapping : ('T -> 'State -> struct ('Result * 'State)) ->
        array : FlatList<'T> ->
        state : 'State ->
            struct (FlatList<'Result> * 'State)

    /// <summary>Returns the greatest element, in F# generic comparison.</summary>
    /// <param name="array">The input flat list.</param>
    /// <returns>The greatest element.</returns>
    /// <exception cref="T:System.ArgumentException">Thrown when the flat list is empty.</exception>
    /// <remarks>O(n).</remarks>
    [<CompiledName "Max">]
    val max : array : FlatList<'T> -> 'T when 'T : comparison

    /// <summary>Returns the element with the greatest result of a projection.</summary>
    /// <param name="projection">The function to transform the elements for the comparison.</param>
    /// <param name="array">The input flat list.</param>
    /// <returns>The first element with the greatest result.</returns>
    /// <exception cref="T:System.ArgumentException">Thrown when the flat list is empty.</exception>
    /// <remarks>O(n).</remarks>
    [<CompiledName "MaxBy">]
    val maxBy : projection : ('T -> 'U) -> array : FlatList<'T> -> 'T when 'U : comparison

    /// <summary>Returns the smallest element, in F# generic comparison.</summary>
    /// <param name="array">The input flat list.</param>
    /// <returns>The smallest element.</returns>
    /// <exception cref="T:System.ArgumentException">Thrown when the flat list is empty.</exception>
    /// <remarks>O(n).</remarks>
    [<CompiledName "Min">]
    val min : array : FlatList<'T> -> 'T when 'T : comparison

    /// <summary>Returns the element with the smallest result of a projection.</summary>
    /// <param name="projection">The function to transform the elements for the comparison.</param>
    /// <param name="array">The input flat list.</param>
    /// <returns>The first element with the smallest result.</returns>
    /// <exception cref="T:System.ArgumentException">Thrown when the flat list is empty.</exception>
    /// <remarks>O(n).</remarks>
    [<CompiledName "MinBy">]
    val minBy : projection : ('T -> 'U) -> array : FlatList<'T> -> 'T when 'U : comparison

    /// <summary>Combines the elements with a function, from the first element to the last one.</summary>
    /// <param name="reduction">The function that combines the result so far with the next element.</param>
    /// <param name="array">The input flat list.</param>
    /// <returns>The combined value.</returns>
    /// <exception cref="T:System.ArgumentException">Thrown when the flat list is empty.</exception>
    /// <remarks>O(n).</remarks>
    [<CompiledName "Reduce">]
    val reduce : reduction : ('T -> 'T -> 'T) -> array : FlatList<'T> -> 'T

    /// <summary>Combines the elements with a function, from the last element to the first one.</summary>
    /// <param name="reduction">The function that combines an element with the result so far.</param>
    /// <param name="array">The input flat list.</param>
    /// <returns>The combined value.</returns>
    /// <exception cref="T:System.ArgumentException">Thrown when the flat list is empty.</exception>
    /// <remarks>O(n).</remarks>
    [<CompiledName "ReduceBack">]
    val reduceBack : reduction : ('T -> 'T -> 'T) -> array : FlatList<'T> -> 'T

    /// <summary>Returns every state of a fold from the first element to the last one, the initial state first.</summary>
    /// <param name="folder">The function that computes the next state.</param>
    /// <param name="state">The initial state.</param>
    /// <param name="array">The input flat list.</param>
    /// <returns>The states; one more than the elements.</returns>
    /// <remarks>O(n).</remarks>
    [<CompiledName "Scan">]
    val scan : folder : ('State -> 'T -> 'State) -> state : 'State -> array : FlatList<'T> -> FlatList<'State>

    /// <summary>Returns every state of a fold from the last element to the first one, the initial state last.</summary>
    /// <param name="folder">The function that computes the next state.</param>
    /// <param name="array">The input flat list.</param>
    /// <param name="state">The initial state.</param>
    /// <returns>The states; one more than the elements.</returns>
    /// <remarks>O(n).</remarks>
    [<CompiledName "ScanBack">]
    val scanBack : folder : ('T -> 'State -> 'State) -> array : FlatList<'T> -> state : 'State -> FlatList<'State>

    /// <summary>Returns the sum of the elements.</summary>
    /// <param name="array">The input flat list.</param>
    /// <returns>The sum; zero for an empty flat list.</returns>
    /// <remarks>O(n).</remarks>
    [<CompiledName "Sum">]
    val inline sum : array : FlatList< ^T > -> ^T when ^T : (static member (+) : ^T * ^T -> ^T) and ^T : (static member Zero : ^T)

    /// <summary>Returns the sum of the results of a function applied to each element.</summary>
    /// <param name="projection">The function to transform the elements.</param>
    /// <param name="array">The input flat list.</param>
    /// <returns>The sum of the results; zero for an empty flat list.</returns>
    /// <remarks>O(n).</remarks>
    [<CompiledName "SumBy">]
    val inline sumBy :
        projection : ('T -> ^U) -> array : FlatList<'T> -> ^U
            when ^U : (static member (+) : ^U * ^U -> ^U) and ^U : (static member Zero : ^U)

    // ---- Partitioning and grouping ----

    /// <summary>Counts the elements of each key of a projection.</summary>
    /// <param name="projection">The function that returns the key of an element.</param>
    /// <param name="array">The input flat list.</param>
    /// <returns>The keys, in the order of their first occurrence, with the number of their elements.</returns>
    /// <remarks>O(n). The keys compare in F# structural equality.</remarks>
    [<CompiledName "CountBy">]
    val countBy : projection : ('T -> 'Key) -> array : FlatList<'T> -> FlatList<struct ('Key * int)> when 'Key : equality

    /// <summary>Returns the elements without duplicates, in F# structural equality.</summary>
    /// <param name="array">The input flat list.</param>
    /// <returns>The first occurrence of each element, in their order.</returns>
    /// <remarks>O(n).</remarks>
    [<CompiledName "Distinct">]
    val distinct : array : FlatList<'T> -> FlatList<'T> when 'T : equality

    /// <summary>Returns the elements without duplicates of the result of a projection.</summary>
    /// <param name="projection">The function that returns the key of an element.</param>
    /// <param name="array">The input flat list.</param>
    /// <returns>The first element of each key, in their order.</returns>
    /// <remarks>O(n).</remarks>
    [<CompiledName "DistinctBy">]
    val distinctBy : projection : ('T -> 'Key) -> array : FlatList<'T> -> FlatList<'T> when 'Key : equality

    /// <summary>Returns the distinct elements that do not equal any of the excluded items.</summary>
    /// <param name="itemsToExclude">The items to remove.</param>
    /// <param name="array">The input flat list.</param>
    /// <returns>The distinct remaining elements, in their order.</returns>
    /// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="itemsToExclude"/> is <see langword="null"/>.</exception>
    /// <remarks>O(n + m).</remarks>
    [<CompiledName "Except">]
    val except : itemsToExclude : seq<'T> -> array : FlatList<'T> -> FlatList<'T> when 'T : equality

    /// <summary>Groups the elements by the key of a projection.</summary>
    /// <param name="projection">The function that returns the key of an element.</param>
    /// <param name="array">The input flat list.</param>
    /// <returns>The keys, in the order of their first occurrence, with their elements in their order.</returns>
    /// <remarks>O(n). The keys compare in F# structural equality.</remarks>
    [<CompiledName "GroupBy">]
    val groupBy : projection : ('T -> 'Key) -> array : FlatList<'T> -> FlatList<struct ('Key * FlatList<'T>)> when 'Key : equality

    /// <summary>Splits the elements by a predicate.</summary>
    /// <param name="predicate">The function to test the elements.</param>
    /// <param name="array">The input flat list.</param>
    /// <returns>The elements that satisfy the predicate and the other ones, both in their order.</returns>
    /// <remarks>O(n).</remarks>
    [<CompiledName "Partition">]
    val partition : predicate : ('T -> bool) -> array : FlatList<'T> -> struct (FlatList<'T> * FlatList<'T>)

    /// <summary>Splits the elements by the choice that a function returns for them.</summary>
    /// <param name="partitioner">The function that returns <c>Choice1Of2</c> or <c>Choice2Of2</c> for an element.</param>
    /// <param name="array">The input flat list.</param>
    /// <returns>The values of the first choices and of the second choices, both in the order of the elements.</returns>
    /// <remarks>O(n).</remarks>
    [<CompiledName "PartitionWith">]
    val partitionWith : partitioner : ('T -> Choice<'T1, 'T2>) -> array : FlatList<'T> -> struct (FlatList<'T1> * FlatList<'T2>)

    /// <summary>Returns the flat list without its first elements.</summary>
    /// <param name="count">The number of elements to skip; a negative count skips nothing.</param>
    /// <param name="array">The input flat list.</param>
    /// <returns>The remaining elements.</returns>
    /// <exception cref="T:System.ArgumentOutOfRangeException">Thrown when the count exceeds the length.</exception>
    /// <remarks>O(n). The result is a copy, unless it is empty or the whole flat list.</remarks>
    [<CompiledName "Skip">]
    val skip : count : int -> array : FlatList<'T> -> FlatList<'T>

    /// <summary>Returns the flat list without its last elements.</summary>
    /// <param name="count">The number of elements to skip; a negative count skips nothing.</param>
    /// <param name="array">The input flat list.</param>
    /// <returns>The remaining elements.</returns>
    /// <exception cref="T:System.ArgumentOutOfRangeException">Thrown when the count exceeds the length.</exception>
    /// <remarks>O(n). The checks follow <see cref="M:Microsoft.FSharp.Collections.ArrayModule.Skip``1(System.Int32,``0[])"/>. The result is a copy, unless it is empty or the whole flat list.</remarks>
    [<CompiledName "SkipLast">]
    val skipLast : count : int -> array : FlatList<'T> -> FlatList<'T>

    /// <summary>Returns the flat list without its first elements that satisfy the predicate.</summary>
    /// <param name="predicate">The function to test the elements.</param>
    /// <param name="array">The input flat list.</param>
    /// <returns>The elements from the first one that does not satisfy the predicate.</returns>
    /// <remarks>O(n). The result is a copy, unless it is empty or the whole flat list.</remarks>
    [<CompiledName "SkipWhile">]
    val skipWhile : predicate : ('T -> bool) -> array : FlatList<'T> -> FlatList<'T>

    /// <summary>Splits the flat list at an index.</summary>
    /// <param name="index">The index of the first element of the second part.</param>
    /// <param name="array">The input flat list.</param>
    /// <returns>The elements before the index and the elements from the index.</returns>
    /// <exception cref="T:System.ArgumentException">Thrown when the index is negative.</exception>
    /// <exception cref="T:System.InvalidOperationException">Thrown when the index exceeds the length.</exception>
    /// <remarks>O(n). The parts are copies, unless they are empty or the whole flat list.</remarks>
    [<CompiledName "SplitAt">]
    val splitAt : index : int -> array : FlatList<'T> -> struct (FlatList<'T> * FlatList<'T>)

    /// <summary>Splits the flat list into at most a given number of parts of nearly equal length.</summary>
    /// <param name="count">The maximum number of parts.</param>
    /// <param name="array">The input flat list.</param>
    /// <returns>The parts; the first ones are one element longer when the length does not divide evenly.</returns>
    /// <exception cref="T:System.ArgumentException">Thrown when <paramref name="count"/> is not positive.</exception>
    /// <remarks>O(n). The parts are copies.</remarks>
    [<CompiledName "SplitInto">]
    val splitInto : count : int -> array : FlatList<'T> -> FlatList<FlatList<'T>>

    /// <summary>Returns the first elements of the flat list.</summary>
    /// <param name="count">The number of elements to take.</param>
    /// <param name="array">The input flat list.</param>
    /// <returns>The first elements.</returns>
    /// <exception cref="T:System.ArgumentException">Thrown when the count is negative.</exception>
    /// <exception cref="T:System.InvalidOperationException">Thrown when the count exceeds the length.</exception>
    /// <remarks>O(n). The result is a copy, unless it is empty or the whole flat list.</remarks>
    [<CompiledName "Take">]
    val take : count : int -> array : FlatList<'T> -> FlatList<'T>

    /// <summary>Returns the last elements of the flat list.</summary>
    /// <param name="count">The number of elements to take.</param>
    /// <param name="array">The input flat list.</param>
    /// <returns>The last elements.</returns>
    /// <exception cref="T:System.ArgumentException">Thrown when the count is negative.</exception>
    /// <exception cref="T:System.InvalidOperationException">Thrown when the count exceeds the length.</exception>
    /// <remarks>O(n). The checks follow <see cref="M:Microsoft.FSharp.Collections.ArrayModule.Take``1(System.Int32,``0[])"/>. The result is a copy, unless it is empty or the whole flat list.</remarks>
    [<CompiledName "TakeLast">]
    val takeLast : count : int -> array : FlatList<'T> -> FlatList<'T>

    /// <summary>Returns the first elements of the flat list that satisfy the predicate.</summary>
    /// <param name="predicate">The function to test the elements.</param>
    /// <param name="array">The input flat list.</param>
    /// <returns>The elements before the first one that does not satisfy the predicate.</returns>
    /// <remarks>O(n). The result is a copy, unless it is empty or the whole flat list.</remarks>
    [<CompiledName "TakeWhile">]
    val takeWhile : predicate : ('T -> bool) -> array : FlatList<'T> -> FlatList<'T>

    /// <summary>Returns at most a given number of first elements.</summary>
    /// <param name="count">The maximum number of elements; a count that is not positive returns an empty flat list.</param>
    /// <param name="array">The input flat list.</param>
    /// <returns>The first elements.</returns>
    /// <remarks>O(n). The result is a copy, unless it is empty or the whole flat list.</remarks>
    [<CompiledName "Truncate">]
    val truncate : count : int -> array : FlatList<'T> -> FlatList<'T>

    // ---- Sorting ----

    /// <summary>Sorts the elements in the order of F# generic comparison.</summary>
    /// <param name="array">The input flat list.</param>
    /// <returns>The sorted flat list.</returns>
    /// <remarks>O(n log n). This is not a stable sort, as <see cref="M:Microsoft.FSharp.Collections.ArrayModule.Sort``1(``0[])"/>.</remarks>
    [<CompiledName "Sort">]
    val sort : array : FlatList<'T> -> FlatList<'T> when 'T : comparison

    /// <summary>Sorts the elements by the result of a projection, in the order of F# generic comparison.</summary>
    /// <param name="projection">The function that returns the key of an element.</param>
    /// <param name="array">The input flat list.</param>
    /// <returns>The sorted flat list.</returns>
    /// <remarks>O(n log n). This is not a stable sort.</remarks>
    [<CompiledName "SortBy">]
    val sortBy : projection : ('T -> 'Key) -> array : FlatList<'T> -> FlatList<'T> when 'Key : comparison

    /// <summary>Sorts the elements by the result of a projection, in descending order.</summary>
    /// <param name="projection">The function that returns the key of an element.</param>
    /// <param name="array">The input flat list.</param>
    /// <returns>The sorted flat list.</returns>
    /// <remarks>O(n log n). This is not a stable sort.</remarks>
    [<CompiledName "SortByDescending">]
    val sortByDescending : projection : ('T -> 'Key) -> array : FlatList<'T> -> FlatList<'T> when 'Key : comparison

    /// <summary>Sorts the elements in descending order of F# generic comparison.</summary>
    /// <param name="array">The input flat list.</param>
    /// <returns>The sorted flat list.</returns>
    /// <remarks>O(n log n). This is not a stable sort.</remarks>
    [<CompiledName "SortDescending">]
    val sortDescending : array : FlatList<'T> -> FlatList<'T> when 'T : comparison

    /// <summary>Sorts the elements with a comparison function.</summary>
    /// <param name="comparer">The function that compares two elements.</param>
    /// <param name="array">The input flat list.</param>
    /// <returns>The sorted flat list.</returns>
    /// <remarks>O(n log n). This is not a stable sort.</remarks>
    [<CompiledName "SortWith">]
    val sortWith : comparer : ('T -> 'T -> int) -> array : FlatList<'T> -> FlatList<'T>

    /// <summary>Sorts the elements with a comparer.</summary>
    /// <param name="comparer">The comparer.</param>
    /// <param name="array">The input flat list.</param>
    /// <returns>The sorted flat list; the input itself when it is already sorted.</returns>
    /// <remarks>
    /// O(n log n). This is not a stable sort. The function wraps
    /// <see cref="M:System.Collections.Immutable.ImmutableArray`1.Sort(System.Collections.Generic.IComparer{`0})"/>.
    /// </remarks>
    [<CompiledName "SortWithComparer">]
    val sortWithComparer : comparer : IComparer<'T> -> array : FlatList<'T> -> FlatList<'T>

    // ---- Random ----

    /// <summary>Returns a random element, chosen with <see cref="P:System.Random.Shared"/>.</summary>
    /// <param name="source">The input flat list.</param>
    /// <returns>A random element.</returns>
    /// <exception cref="T:System.ArgumentException">Thrown when the flat list is empty.</exception>
    /// <remarks>O(1).</remarks>
    [<CompiledName "RandomChoice">]
    val randomChoice : source : FlatList<'T> -> 'T

    /// <summary>Returns a random element, chosen with a function that returns numbers in [0, 1).</summary>
    /// <param name="randomizer">The function that returns a random number in [0, 1).</param>
    /// <param name="source">The input flat list.</param>
    /// <returns>A random element.</returns>
    /// <exception cref="T:System.ArgumentException">Thrown when the flat list is empty.</exception>
    /// <exception cref="T:System.ArgumentOutOfRangeException">Thrown when the randomizer returns a number outside [0, 1).</exception>
    /// <remarks>O(1).</remarks>
    [<CompiledName "RandomChoiceBy">]
    val randomChoiceBy : randomizer : (unit -> float) -> source : FlatList<'T> -> 'T

    /// <summary>Returns a random element, chosen with a given random number generator.</summary>
    /// <param name="random">The random number generator.</param>
    /// <param name="source">The input flat list.</param>
    /// <returns>A random element.</returns>
    /// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="random"/> is <see langword="null"/>.</exception>
    /// <exception cref="T:System.ArgumentException">Thrown when the flat list is empty.</exception>
    /// <remarks>O(1).</remarks>
    [<CompiledName "RandomChoiceWith">]
    val randomChoiceWith : random : Random -> source : FlatList<'T> -> 'T

    /// <summary>Returns random elements, chosen with <see cref="P:System.Random.Shared"/>; an element can repeat.</summary>
    /// <param name="count">The number of elements to choose.</param>
    /// <param name="source">The input flat list.</param>
    /// <returns>The chosen elements.</returns>
    /// <exception cref="T:System.ArgumentException">Thrown when the count is negative or the flat list is empty.</exception>
    /// <remarks>O(count).</remarks>
    [<CompiledName "RandomChoices">]
    val randomChoices : count : int -> source : FlatList<'T> -> FlatList<'T>

    /// <summary>Returns random elements, chosen with a function that returns numbers in [0, 1); an element can repeat.</summary>
    /// <param name="randomizer">The function that returns a random number in [0, 1).</param>
    /// <param name="count">The number of elements to choose.</param>
    /// <param name="source">The input flat list.</param>
    /// <returns>The chosen elements.</returns>
    /// <exception cref="T:System.ArgumentException">Thrown when the count is negative or the flat list is empty.</exception>
    /// <exception cref="T:System.ArgumentOutOfRangeException">Thrown when the randomizer returns a number outside [0, 1).</exception>
    /// <remarks>O(count).</remarks>
    [<CompiledName "RandomChoicesBy">]
    val randomChoicesBy : randomizer : (unit -> float) -> count : int -> source : FlatList<'T> -> FlatList<'T>

    /// <summary>Returns random elements, chosen with a given random number generator; an element can repeat.</summary>
    /// <param name="random">The random number generator.</param>
    /// <param name="count">The number of elements to choose.</param>
    /// <param name="source">The input flat list.</param>
    /// <returns>The chosen elements.</returns>
    /// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="random"/> is <see langword="null"/>.</exception>
    /// <exception cref="T:System.ArgumentException">Thrown when the count is negative or the flat list is empty.</exception>
    /// <remarks>O(count).</remarks>
    [<CompiledName "RandomChoicesWith">]
    val randomChoicesWith : random : Random -> count : int -> source : FlatList<'T> -> FlatList<'T>

    /// <summary>Returns random elements at distinct indices, chosen with <see cref="P:System.Random.Shared"/>.</summary>
    /// <param name="count">The number of elements to choose.</param>
    /// <param name="source">The input flat list.</param>
    /// <returns>The chosen elements.</returns>
    /// <exception cref="T:System.ArgumentException">Thrown when the count is negative or exceeds the length.</exception>
    /// <remarks>O(count).</remarks>
    [<CompiledName "RandomSample">]
    val randomSample : count : int -> source : FlatList<'T> -> FlatList<'T>

    /// <summary>Returns random elements at distinct indices, chosen with a function that returns numbers in [0, 1).</summary>
    /// <param name="randomizer">The function that returns a random number in [0, 1).</param>
    /// <param name="count">The number of elements to choose.</param>
    /// <param name="source">The input flat list.</param>
    /// <returns>The chosen elements.</returns>
    /// <exception cref="T:System.ArgumentException">Thrown when the count is negative or exceeds the length.</exception>
    /// <exception cref="T:System.ArgumentOutOfRangeException">Thrown when the randomizer returns a number outside [0, 1).</exception>
    /// <remarks>O(count).</remarks>
    [<CompiledName "RandomSampleBy">]
    val randomSampleBy : randomizer : (unit -> float) -> count : int -> source : FlatList<'T> -> FlatList<'T>

    /// <summary>Returns random elements at distinct indices, chosen with a given random number generator.</summary>
    /// <param name="random">The random number generator.</param>
    /// <param name="count">The number of elements to choose.</param>
    /// <param name="source">The input flat list.</param>
    /// <returns>The chosen elements.</returns>
    /// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="random"/> is <see langword="null"/>.</exception>
    /// <exception cref="T:System.ArgumentException">Thrown when the count is negative or exceeds the length.</exception>
    /// <remarks>O(count).</remarks>
    [<CompiledName "RandomSampleWith">]
    val randomSampleWith : random : Random -> count : int -> source : FlatList<'T> -> FlatList<'T>

    /// <summary>Returns the elements in a random order, chosen with <see cref="P:System.Random.Shared"/>.</summary>
    /// <param name="source">The input flat list.</param>
    /// <returns>The shuffled flat list.</returns>
    /// <remarks>O(n).</remarks>
    [<CompiledName "RandomShuffle">]
    val randomShuffle : source : FlatList<'T> -> FlatList<'T>

    /// <summary>Returns the elements in a random order, chosen with a function that returns numbers in [0, 1).</summary>
    /// <param name="randomizer">The function that returns a random number in [0, 1).</param>
    /// <param name="source">The input flat list.</param>
    /// <returns>The shuffled flat list.</returns>
    /// <exception cref="T:System.ArgumentOutOfRangeException">Thrown when the randomizer returns a number outside [0, 1).</exception>
    /// <remarks>O(n).</remarks>
    [<CompiledName "RandomShuffleBy">]
    val randomShuffleBy : randomizer : (unit -> float) -> source : FlatList<'T> -> FlatList<'T>

    /// <summary>Returns the elements in a random order, chosen with a given random number generator.</summary>
    /// <param name="random">The random number generator.</param>
    /// <param name="source">The input flat list.</param>
    /// <returns>The shuffled flat list.</returns>
    /// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="random"/> is <see langword="null"/>.</exception>
    /// <remarks>O(n).</remarks>
    [<CompiledName "RandomShuffleWith">]
    val randomShuffleWith : random : Random -> source : FlatList<'T> -> FlatList<'T>

    // ---- Conversion ----

    /// <summary>Returns a read-only memory region over the elements, without a copy.</summary>
    /// <param name="array">The input flat list.</param>
    /// <returns>The memory region.</returns>
    /// <remarks>O(1). The function wraps <see cref="M:System.Collections.Immutable.ImmutableArray`1.AsMemory"/>.</remarks>
    [<CompiledName "AsMemory">]
    val asMemory : array : FlatList<'T> -> ReadOnlyMemory<'T>

    /// <summary>Returns a read-only span over the elements, without a copy.</summary>
    /// <param name="array">The input flat list.</param>
    /// <returns>The span.</returns>
    /// <remarks>
    /// O(1). The function wraps <see cref="M:System.Collections.Immutable.ImmutableArray`1.AsSpan"/>. A range of the span is a
    /// view of a range of the flat list that copies nothing.
    /// </remarks>
    [<CompiledName "AsSpan">]
    val asSpan : array : FlatList<'T> -> ReadOnlySpan<'T>

    /// <summary>Copies a range of the flat list into an array.</summary>
    /// <param name="source">The input flat list.</param>
    /// <param name="sourceIndex">The index of the first element to copy.</param>
    /// <param name="target">The array to copy to.</param>
    /// <param name="targetIndex">The index in the target array of the first copied element.</param>
    /// <param name="count">The number of elements to copy.</param>
    /// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="target"/> is <see langword="null"/>.</exception>
    /// <exception cref="T:System.ArgumentException">Thrown when a range is outside its flat list or array.</exception>
    /// <remarks>O(count).</remarks>
    [<CompiledName "CopyTo">]
    val blit : source : FlatList<'T> -> sourceIndex : int -> target : 'T array -> targetIndex : int -> count : int -> unit

    /// <summary>Returns a new array with the elements.</summary>
    /// <param name="array">The input flat list.</param>
    /// <returns>A copy of the elements.</returns>
    /// <remarks>O(n).</remarks>
    [<CompiledName "ToArray">]
    val toArray : array : FlatList<'T> -> 'T array

    /// <summary>Returns a list of the elements.</summary>
    /// <param name="array">The input flat list.</param>
    /// <returns>The list of the elements.</returns>
    /// <remarks>O(n).</remarks>
    [<CompiledName "ToList">]
    val toList : array : FlatList<'T> -> 'T list

    /// <summary>Returns the flat list as a sequence.</summary>
    /// <param name="array">The input flat list.</param>
    /// <returns>The sequence of the elements.</returns>
    /// <remarks>O(1).</remarks>
    [<CompiledName "ToSeq">]
    val toSeq : array : FlatList<'T> -> seq<'T>

    // ---- Type-specific ----

    /// <summary>Returns whether the immutable array is a default value without an underlying array.</summary>
    /// <param name="array">The input flat list, which can be a default value.</param>
    /// <returns><see langword="true"/> for a default value.</returns>
    /// <remarks>O(1). The function wraps <see cref="P:System.Collections.Immutable.ImmutableArray`1.IsDefault"/>.</remarks>
    [<CompiledName "IsDefault">]
    val isDefault : array : FlatList<'T> -> bool

    /// <summary>Returns whether the immutable array is a default value or empty.</summary>
    /// <param name="array">The input flat list, which can be a default value.</param>
    /// <returns><see langword="true"/> for a default value or an empty flat list.</returns>
    /// <remarks>O(1). The function wraps <see cref="P:System.Collections.Immutable.ImmutableArray`1.IsDefaultOrEmpty"/>.</remarks>
    [<CompiledName "IsDefaultOrEmpty">]
    val isDefaultOrEmpty : array : FlatList<'T> -> bool
