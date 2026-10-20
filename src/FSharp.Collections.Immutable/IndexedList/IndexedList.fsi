/// <summary>
/// Functions for the indexed list, the immutable list <see cref="T:System.Collections.Immutable.ImmutableList`1"/>.
/// </summary>
/// <remarks>
/// <para>
/// The module mirrors <see cref="T:Microsoft.FSharp.Collections.ListModule"/>: each function has the semantics, the
/// parameter names, the argument checks and the exceptions of the function with the same name in that module (ADR 0001),
/// and its sorting functions are stable. Functions that FSharp.Core lacks wrap a member of
/// <see cref="T:System.Collections.Immutable.ImmutableList`1"/> or follow the function of that module that they resemble.
/// </para>
/// <para>
/// Every function throws <see cref="T:System.ArgumentNullException"/> for a <see langword="null"/> indexed list, as
/// <see cref="T:Microsoft.FSharp.Collections.ArrayModule"/> does for a <see langword="null"/> array. A function that
/// takes a sequence throws it for a <see langword="null"/> sequence and for a default
/// <see cref="T:System.Collections.Immutable.ImmutableArray`1"/> that is given as a sequence, which would fail with
/// <see cref="T:System.InvalidOperationException"/> when the function enumerates it.
/// </para>
/// <para>
/// Functions that return an optional value return a <see cref="T:Microsoft.FSharp.Core.FSharpValueOption`1"/>, and functions
/// that return tuples return struct tuples. The namespaces <see cref="N:FSharp.Collections.Immutable.Option"/> and
/// <see cref="N:FSharp.Collections.Immutable.Tuple"/> contain a module with the same name whose functions use options
/// and reference tuples instead.
/// </para>
/// </remarks>
[<RequireQualifiedAccess>]
module FSharp.Collections.Immutable.IndexedList

open System
open System.Collections.Generic
open System.Collections.Immutable

// ---- Creation ----

/// <summary>
/// The empty indexed list.
/// </summary>
/// <remarks>
/// O(1). The value is <see cref="F:System.Collections.Immutable.ImmutableList`1.Empty"/>.
/// </remarks>
[<GeneralizableValue>]
[<CompiledName "Empty">]
val empty<'T> : IndexedList<'T>

/// <summary>Creates an indexed list from a function of the index.</summary>
/// <param name="length">The length of the indexed list.</param>
/// <param name="initializer">The function that computes the element at each index, called in index order.</param>
/// <returns>The created indexed list.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when <paramref name="length"/> is negative.</exception>
/// <remarks>O(n).</remarks>
[<CompiledName "Initialize">]
val init : length : int -> initializer : (int -> 'T) -> IndexedList<'T>

/// <summary>
/// Creates an indexed list from a copy of an array.
/// </summary>
/// <param name="array">The input array.</param>
/// <returns>The indexed list of the elements of the array.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="array"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n). Later changes to the array do not change the indexed list.
/// </remarks>
[<CompiledName "OfArray">]
val ofArray : array : 'T array -> IndexedList<'T>

/// <summary>Creates an indexed list from a list.</summary>
/// <param name="list">The input list.</param>
/// <returns>The indexed list of the elements of the list.</returns>
/// <remarks>
/// O(n). The elements are copied into an array first, because the BCL builds the tree by indexing and the indexer of an
/// F# list walks the list from its head, so that building the tree from the list itself takes O(n²).
/// </remarks>
[<CompiledName "OfList">]
val ofList : list : 'T list -> IndexedList<'T>

/// <summary>
/// Creates an indexed list from a copy of a read-only span.
/// </summary>
/// <param name="span">The input span.</param>
/// <returns>The indexed list of the elements of the span.</returns>
/// <remarks>
/// O(n). The function wraps <see cref="M:System.Collections.Immutable.ImmutableList.Create``1(System.ReadOnlySpan{``0})"/>.
/// </remarks>
[<CompiledName "OfReadOnlySpan">]
val ofReadOnlySpan : span : ReadOnlySpan<'T> -> IndexedList<'T>

/// <summary>
/// Creates an indexed list from a sequence.
/// </summary>
/// <param name="source">The input sequence.</param>
/// <returns>The indexed list of the elements of the sequence.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="source"/> is <see langword="null"/> or a default flat list.</exception>
/// <remarks>
/// O(n). The function wraps <see cref="M:System.Collections.Immutable.ImmutableList.CreateRange``1(System.Collections.Generic.IEnumerable{``0})"/>,
/// which returns an indexed list itself, without a copy.
/// An F# list is copied into an array first, as <see cref="M:FSharp.Collections.Immutable.IndexedList.OfList``1(Microsoft.FSharp.Collections.FSharpList{``0})"/> does.
/// </remarks>
[<CompiledName "OfSeq">]
val ofSeq : source : seq<'T> -> IndexedList<'T>

/// <summary>
/// Creates an indexed list from a copy of a span.
/// </summary>
/// <param name="span">The input span.</param>
/// <returns>The indexed list of the elements of the span.</returns>
/// <remarks>
/// O(n). The function wraps <see cref="M:System.Collections.Immutable.ImmutableList.Create``1(System.ReadOnlySpan{``0})"/>.
/// </remarks>
[<CompiledName "OfSpan">]
val ofSpan : span : Span<'T> -> IndexedList<'T>

/// <summary>
/// Creates an indexed list whose elements are all the given value.
/// </summary>
/// <param name="count">The length of the indexed list.</param>
/// <param name="initial">The value of every element.</param>
/// <returns>The created indexed list.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when <paramref name="count"/> is negative.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Replicate">]
val replicate : count : int -> initial : 'T -> IndexedList<'T>

/// <summary>Creates an indexed list of one element.</summary>
/// <param name="value">The element.</param>
/// <returns>The indexed list of one element.</returns>
/// <remarks>O(1).</remarks>
[<CompiledName "Singleton">]
val singleton : value : 'T -> IndexedList<'T>

/// <summary>
/// Creates an indexed list from a generator, which returns the next element and the next state.
/// </summary>
/// <param name="generator">The function that returns the next element and state, or <c>ValueNone</c> to stop.</param>
/// <param name="state">The initial state.</param>
/// <returns>The generated indexed list.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Unfold">]
val unfold : generator : ('State -> struct ('T * 'State) voption) -> state : 'State -> IndexedList<'T>

// ---- Builder ----

/// <summary>
/// Creates an indexed list with a builder that an action fills.
/// </summary>
/// <param name="action">The action that adds the elements to the builder.</param>
/// <returns>The indexed list of the elements that the action added.</returns>
/// <remarks>
/// O(n log n) for an action that adds n elements one by one; the result shares the tree of the builder.
/// </remarks>
[<CompiledName "Build">]
val build : action : (IndexedList<'T>.Builder -> unit) -> IndexedList<'T>

/// <summary>
/// Creates an empty builder.
/// </summary>
/// <returns>An empty builder.</returns>
/// <remarks>
/// O(1). The function wraps <see cref="M:System.Collections.Immutable.ImmutableList.CreateBuilder``1"/>.
/// </remarks>
[<CompiledName "CreateBuilder">]
val builder : unit -> IndexedList<'T>.Builder

/// <summary>
/// Returns the contents of a builder as an indexed list.
/// </summary>
/// <param name="builder">The builder, which stays usable.</param>
/// <returns>The indexed list of the elements of the builder.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="builder"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n) at most: the builder freezes the nodes that it created since it returned its contents last time, without a
/// copy. The function wraps <see cref="M:System.Collections.Immutable.ImmutableList`1.Builder.ToImmutable"/>.
/// </remarks>
[<CompiledName "OfBuilder">]
val ofBuilder : builder : IndexedList<'T>.Builder -> IndexedList<'T>

/// <summary>
/// Creates a builder that starts with the elements of an indexed list.
/// </summary>
/// <param name="list">The input indexed list, which stays unchanged.</param>
/// <returns>The builder.</returns>
/// <remarks>
/// O(1): the builder shares the tree of the indexed list until it changes. The function wraps <see cref="M:System.Collections.Immutable.ImmutableList`1.ToBuilder"/>.
/// </remarks>
[<CompiledName "ToBuilder">]
val toBuilder : list : IndexedList<'T> -> IndexedList<'T>.Builder

/// <summary>
/// Creates an indexed list from a builder over the elements of an indexed list, after an action changes the builder.
/// </summary>
/// <param name="action">The action that changes the builder.</param>
/// <param name="list">The input indexed list, which stays unchanged.</param>
/// <returns>The indexed list of the elements of the changed builder.</returns>
/// <remarks>
/// O(k log n) for an action that makes k changes: the builder shares the tree of the indexed list and copies only the
/// nodes that the changes touch.
/// </remarks>
[<CompiledName "Update">]
val update : action : (IndexedList<'T>.Builder -> unit) -> list : IndexedList<'T> -> IndexedList<'T>

// ---- Properties ----

/// <summary>Returns whether the indexed list has no elements.</summary>
/// <param name="list">The input indexed list.</param>
/// <returns><see langword="true"/> when the indexed list is empty.</returns>
/// <remarks>O(1).</remarks>
[<CompiledName "IsEmpty">]
val isEmpty : list : IndexedList<'T> -> bool

/// <summary>Returns the number of elements of the indexed list.</summary>
/// <param name="list">The input indexed list.</param>
/// <returns>The length of the indexed list.</returns>
/// <remarks>O(1).</remarks>
[<CompiledName "Length">]
val length : list : IndexedList<'T> -> int

// ---- Access ----

/// <summary>Returns the only element of the indexed list.</summary>
/// <param name="list">The input indexed list.</param>
/// <returns>The only element.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the indexed list does not have exactly one element.</exception>
/// <remarks>O(1).</remarks>
[<CompiledName "ExactlyOne">]
val exactlyOne : list : IndexedList<'T> -> 'T

/// <summary>Returns the first element of the indexed list.</summary>
/// <param name="list">The input indexed list.</param>
/// <returns>The first element.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the indexed list is empty.</exception>
/// <remarks>O(log n).</remarks>
[<CompiledName "Head">]
val head : list : IndexedList<'T> -> 'T

/// <summary>Returns the element at an index.</summary>
/// <param name="index">The index of the element.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>The element at the index.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the index is negative or not less than the length.</exception>
/// <remarks>O(log n).</remarks>
[<CompiledName "Item">]
val item : index : int -> list : IndexedList<'T> -> 'T

/// <summary>Returns the last element of the indexed list.</summary>
/// <param name="list">The input indexed list.</param>
/// <returns>The last element.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the indexed list is empty.</exception>
/// <remarks>O(log n).</remarks>
[<CompiledName "Last">]
val last : list : IndexedList<'T> -> 'T

/// <summary>Returns the indexed list without its first element.</summary>
/// <param name="list">The input indexed list.</param>
/// <returns>The elements after the first one.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the indexed list is empty.</exception>
/// <remarks>O(log n). The result shares most of its tree with the input.</remarks>
[<CompiledName "Tail">]
val tail : list : IndexedList<'T> -> IndexedList<'T>

/// <summary>
/// Returns the only element of the indexed list, or <c>ValueNone</c> when it does not have exactly one element.
/// </summary>
/// <param name="list">The input indexed list.</param>
/// <returns>The only element, or <c>ValueNone</c>.</returns>
/// <remarks>
/// O(1).
/// </remarks>
[<CompiledName "TryExactlyOne">]
val tryExactlyOne : list : IndexedList<'T> -> 'T voption

/// <summary>
/// Returns the first element of the indexed list, or <c>ValueNone</c> when it is empty.
/// </summary>
/// <param name="list">The input indexed list.</param>
/// <returns>The first element, or <c>ValueNone</c>.</returns>
/// <remarks>
/// O(log n).
/// </remarks>
[<CompiledName "TryHead">]
val tryHead : list : IndexedList<'T> -> 'T voption

/// <summary>
/// Returns the element at an index, or <c>ValueNone</c> when the index is out of range.
/// </summary>
/// <param name="index">The index of the element.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>The element at the index, or <c>ValueNone</c>.</returns>
/// <remarks>
/// O(log n).
/// </remarks>
[<CompiledName "TryItem">]
val tryItem : index : int -> list : IndexedList<'T> -> 'T voption

/// <summary>
/// Returns the last element of the indexed list, or <c>ValueNone</c> when it is empty.
/// </summary>
/// <param name="list">The input indexed list.</param>
/// <returns>The last element, or <c>ValueNone</c>.</returns>
/// <remarks>
/// O(log n).
/// </remarks>
[<CompiledName "TryLast">]
val tryLast : list : IndexedList<'T> -> 'T voption

/// <summary>
/// Returns the indexed list without its first element, or <c>ValueNone</c> when it is empty.
/// </summary>
/// <param name="list">The input indexed list.</param>
/// <returns>The elements after the first one, or <c>ValueNone</c>.</returns>
/// <remarks>
/// O(log n).
/// </remarks>
[<CompiledName "TryTail">]
val tryTail : list : IndexedList<'T> -> IndexedList<'T> voption

/// <summary>
/// Returns the first element and the other elements, or <c>ValueNone</c> when the indexed list is empty.
/// </summary>
/// <param name="list">The input indexed list.</param>
/// <returns>The first element and the elements after it, or <c>ValueNone</c>.</returns>
/// <remarks>
/// O(log n).
/// </remarks>
[<CompiledName "TryUncons">]
val tryUncons : list : IndexedList<'T> -> struct ('T * IndexedList<'T>) voption

/// <summary>
/// Returns the first element and the other elements.
/// </summary>
/// <param name="list">The input indexed list.</param>
/// <returns>The first element and the elements after it.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the indexed list is empty.</exception>
/// <remarks>
/// O(log n). The function throws what <see cref="M:FSharp.Collections.Immutable.IndexedList.Head``1(System.Collections.Immutable.ImmutableList{``0})"/>
/// throws.
/// </remarks>
[<CompiledName "Uncons">]
val uncons : list : IndexedList<'T> -> struct ('T * IndexedList<'T>)

/// <summary>
/// Matches an indexed list with a first element, and returns that element and the other elements.
/// </summary>
/// <param name="list">The input indexed list.</param>
/// <returns>The first element and the elements after it, or <c>ValueNone</c> for an empty indexed list.</returns>
/// <remarks>
/// O(log n). A struct partial pattern, so that a match allocates no option.
/// </remarks>
[<return : Struct>]
[<CompiledName "ConsPattern">]
val (|Cons|_|) : list : IndexedList<'T> -> struct ('T * IndexedList<'T>) voption

/// <summary>Matches an empty indexed list.</summary>
/// <param name="list">The input indexed list.</param>
/// <returns><see langword="true"/> when the indexed list is empty.</returns>
/// <remarks>O(1).</remarks>
[<CompiledName "NilPattern">]
val (|Nil|_|) : list : IndexedList<'T> -> bool

// ---- Search ----

/// <summary>
/// Searches a sorted indexed list for a value with the binary search algorithm.
/// </summary>
/// <param name="value">The value to search for.</param>
/// <param name="list">The input indexed list, sorted in the order of F# generic comparison.</param>
/// <returns>
/// The index of the value, or the bitwise complement of the index of the first larger element, or the bitwise
/// complement of the length when no element is larger.
/// </returns>
/// <remarks>
/// O(log n). The function compares with <see cref="M:Microsoft.FSharp.Core.LanguagePrimitives.FastGenericComparer``1"/>,
/// the order that <see cref="M:FSharp.Collections.Immutable.IndexedList.Sort``1(System.Collections.Immutable.ImmutableList{``0})"/>
/// produces. The function wraps <see cref="M:System.Collections.Immutable.ImmutableList`1.BinarySearch(`0,System.Collections.Generic.IComparer{`0})"/>.
/// </remarks>
[<CompiledName "BinarySearch">]
val binarySearch : value : 'T -> list : IndexedList<'T> -> int when 'T : comparison

/// <summary>
/// Searches a range of a sorted indexed list for a value with the binary search algorithm.
/// </summary>
/// <param name="index">The index of the first element of the range.</param>
/// <param name="count">The number of elements of the range.</param>
/// <param name="value">The value to search for.</param>
/// <param name="list">The input indexed list, sorted in the order of F# generic comparison.</param>
/// <returns>
/// The index of the value, or the bitwise complement of the index of the first larger element of the range, or the
/// bitwise complement of the index after the range when no element of the range is larger.
/// </returns>
/// <exception cref="T:System.ArgumentOutOfRangeException">
/// Thrown when <paramref name="index"/> or <paramref name="count"/> is negative.
/// </exception>
/// <exception cref="T:System.ArgumentException">Thrown when the range extends beyond the end of the indexed list.</exception>
/// <remarks>
/// <para>
/// O(log n). The function compares as
/// <see cref="M:FSharp.Collections.Immutable.IndexedList.BinarySearch``1(``0,System.Collections.Immutable.ImmutableList{``0})"/>
/// does, and wraps
/// <see cref="M:System.Collections.Immutable.ImmutableList`1.BinarySearch(System.Int32,System.Int32,`0,System.Collections.Generic.IComparer{`0})"/>.
/// </para>
/// <para>
/// That member does not check that the range lies in the indexed list, although its documentation says that it throws
/// <see cref="T:System.ArgumentException"/>. The function throws, as the documentation says (ADR 0001, D14).
/// </para>
/// </remarks>
[<CompiledName "BinarySearchRange">]
val binarySearchRange : index : int -> count : int -> value : 'T -> list : IndexedList<'T> -> int when 'T : comparison

/// <summary>
/// Searches a range of an indexed list for a value with the binary search algorithm and a given comparer.
/// </summary>
/// <param name="comparer">The comparer, in whose order the indexed list is sorted.</param>
/// <param name="index">The index of the first element of the range.</param>
/// <param name="count">The number of elements of the range.</param>
/// <param name="value">The value to search for.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>
/// The index of the value, or the bitwise complement of the index of the first larger element of the range, or the
/// bitwise complement of the index after the range when no element of the range is larger.
/// </returns>
/// <exception cref="T:System.ArgumentOutOfRangeException">
/// Thrown when <paramref name="index"/> or <paramref name="count"/> is negative.
/// </exception>
/// <exception cref="T:System.ArgumentException">Thrown when the range extends beyond the end of the indexed list.</exception>
/// <remarks>
/// <para>
/// O(log n) comparisons. An exception that the comparer throws is not wrapped. The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableList`1.BinarySearch(System.Int32,System.Int32,`0,System.Collections.Generic.IComparer{`0})"/>.
/// </para>
/// <para>
/// That member does not check that the range lies in the indexed list, although its documentation says that it throws
/// <see cref="T:System.ArgumentException"/>. The function throws, as the documentation says (ADR 0001, D14).
/// </para>
/// </remarks>
[<CompiledName "BinarySearchRangeWithComparer">]
val binarySearchRangeWithComparer :
    comparer : IComparer<'T> -> index : int -> count : int -> value : 'T -> list : IndexedList<'T> -> int

/// <summary>
/// Searches a sorted indexed list for a value with the binary search algorithm and a given comparer.
/// </summary>
/// <param name="comparer">The comparer, in whose order the indexed list is sorted.</param>
/// <param name="value">The value to search for.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>
/// The index of the value, or the bitwise complement of the index of the first larger element, or the bitwise
/// complement of the length when no element is larger.
/// </returns>
/// <remarks>
/// O(log n) comparisons. An exception that the comparer throws is not wrapped. The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableList`1.BinarySearch(`0,System.Collections.Generic.IComparer{`0})"/>.
/// </remarks>
[<CompiledName "BinarySearchWithComparer">]
val binarySearchWithComparer : comparer : IComparer<'T> -> value : 'T -> list : IndexedList<'T> -> int

/// <summary>
/// Returns whether the indexed list contains an element equal to a value, in F# structural equality.
/// </summary>
/// <param name="value">The value to find.</param>
/// <param name="source">The input indexed list.</param>
/// <returns><see langword="true"/> when an element equals the value.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Contains">]
val contains : value : 'T -> source : IndexedList<'T> -> bool when 'T : equality

/// <summary>
/// Returns whether any element satisfies the predicate.
/// </summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="list">The input indexed list.</param>
/// <returns><see langword="true"/> when an element satisfies the predicate.</returns>
/// <remarks>
/// O(n). The function wraps <see cref="M:System.Collections.Immutable.ImmutableList`1.Exists(System.Predicate{`0})"/>.
/// </remarks>
[<CompiledName "Exists">]
val exists : predicate : ('T -> bool) -> list : IndexedList<'T> -> bool

/// <summary>
/// Returns whether any pair of elements at the same index satisfies the predicate.
/// </summary>
/// <param name="predicate">The function to test the pairs.</param>
/// <param name="list1">The first indexed list.</param>
/// <param name="list2">The second indexed list.</param>
/// <returns><see langword="true"/> when a pair satisfies the predicate.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when no pair satisfies the predicate and the indexed lists have different lengths.</exception>
/// <remarks>
/// O(n). The function walks the two indexed lists together and stops at the first pair that satisfies the predicate.
/// </remarks>
[<CompiledName "Exists2">]
val exists2 : predicate : ('T1 -> 'T2 -> bool) -> list1 : IndexedList<'T1> -> list2 : IndexedList<'T2> -> bool

/// <summary>Returns the first element that satisfies the predicate.</summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>The first element that satisfies the predicate.</returns>
/// <exception cref="T:System.Collections.Generic.KeyNotFoundException">Thrown when no element satisfies the predicate.</exception>
/// <remarks>O(n).</remarks>
[<CompiledName "Find">]
val find : predicate : ('T -> bool) -> list : IndexedList<'T> -> 'T

/// <summary>Returns the last element that satisfies the predicate.</summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>The last element that satisfies the predicate.</returns>
/// <exception cref="T:System.Collections.Generic.KeyNotFoundException">Thrown when no element satisfies the predicate.</exception>
/// <remarks>O(n).</remarks>
[<CompiledName "FindBack">]
val findBack : predicate : ('T -> bool) -> list : IndexedList<'T> -> 'T

/// <summary>
/// Returns the index of the first element that satisfies the predicate.
/// </summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>The index of the first element that satisfies the predicate.</returns>
/// <exception cref="T:System.Collections.Generic.KeyNotFoundException">Thrown when no element satisfies the predicate.</exception>
/// <remarks>
/// O(n). The function wraps <see cref="M:System.Collections.Immutable.ImmutableList`1.FindIndex(System.Predicate{`0})"/>.
/// </remarks>
[<CompiledName "FindIndex">]
val findIndex : predicate : ('T -> bool) -> list : IndexedList<'T> -> int

/// <summary>
/// Returns the index of the last element that satisfies the predicate.
/// </summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>The index of the last element that satisfies the predicate.</returns>
/// <exception cref="T:System.Collections.Generic.KeyNotFoundException">Thrown when no element satisfies the predicate.</exception>
/// <remarks>
/// O(n). The function wraps <see cref="M:System.Collections.Immutable.ImmutableList`1.FindLastIndex(System.Predicate{`0})"/>.
/// </remarks>
[<CompiledName "FindIndexBack">]
val findIndexBack : predicate : ('T -> bool) -> list : IndexedList<'T> -> int

/// <summary>
/// Returns whether all elements satisfy the predicate.
/// </summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="list">The input indexed list.</param>
/// <returns><see langword="true"/> when every element satisfies the predicate.</returns>
/// <remarks>
/// O(n). The function wraps <see cref="M:System.Collections.Immutable.ImmutableList`1.TrueForAll(System.Predicate{`0})"/>.
/// </remarks>
[<CompiledName "ForAll">]
val forall : predicate : ('T -> bool) -> list : IndexedList<'T> -> bool

/// <summary>
/// Returns whether all pairs of elements at the same index satisfy the predicate.
/// </summary>
/// <param name="predicate">The function to test the pairs.</param>
/// <param name="list1">The first indexed list.</param>
/// <param name="list2">The second indexed list.</param>
/// <returns><see langword="true"/> when every pair satisfies the predicate.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when every pair satisfies the predicate and the indexed lists have different lengths.</exception>
/// <remarks>
/// O(n). The function walks the two indexed lists together and stops at the first pair that does not satisfy the
/// predicate.
/// </remarks>
[<CompiledName "ForAll2">]
val forall2 : predicate : ('T1 -> 'T2 -> bool) -> list1 : IndexedList<'T1> -> list2 : IndexedList<'T2> -> bool

/// <summary>
/// Returns the index of the first element equal to a value, in <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>.
/// </summary>
/// <param name="value">The value to find.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>The index of the first equal element, or -1.</returns>
/// <remarks>
/// O(n). The function wraps <see cref="M:System.Collections.Immutable.ImmutableList`1.IndexOf(`0)"/>.
/// </remarks>
[<CompiledName "IndexOf">]
val indexOf : value : 'T -> list : IndexedList<'T> -> int

/// <summary>
/// Returns the index of the first element of a range that equals a value, in
/// <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>.
/// </summary>
/// <param name="startIndex">The index of the first element of the range.</param>
/// <param name="count">The number of elements of the range.</param>
/// <param name="value">The value to find.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>The index in the indexed list, not in the range, of the first equal element of the range, or -1.</returns>
/// <exception cref="T:System.ArgumentOutOfRangeException">
/// Thrown when <paramref name="startIndex"/> or <paramref name="count"/> is negative, or the range extends beyond the end
/// of the indexed list.
/// </exception>
/// <remarks>
/// O(log n + count). The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableList`1.IndexOf(`0,System.Int32,System.Int32,System.Collections.Generic.IEqualityComparer{`0})"/>.
/// </remarks>
[<CompiledName "IndexOfRange">]
val indexOfRange : startIndex : int -> count : int -> value : 'T -> list : IndexedList<'T> -> int

/// <summary>
/// Returns the index of the first element of a range that a given equality comparer finds equal to a value.
/// </summary>
/// <param name="comparer">The equality comparer.</param>
/// <param name="startIndex">The index of the first element of the range.</param>
/// <param name="count">The number of elements of the range.</param>
/// <param name="value">The value to find.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>The index in the indexed list, not in the range, of the first equal element of the range, or -1.</returns>
/// <exception cref="T:System.ArgumentOutOfRangeException">
/// Thrown when <paramref name="startIndex"/> or <paramref name="count"/> is negative, or the range extends beyond the end
/// of the indexed list.
/// </exception>
/// <remarks>
/// O(log n + count). The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableList`1.IndexOf(`0,System.Int32,System.Int32,System.Collections.Generic.IEqualityComparer{`0})"/>.
/// </remarks>
[<CompiledName "IndexOfRangeWith">]
val indexOfRangeWith :
    comparer : IEqualityComparer<'T> -> startIndex : int -> count : int -> value : 'T -> list : IndexedList<'T> -> int

/// <summary>
/// Returns the index of the first element equal to a value in a given equality comparer.
/// </summary>
/// <param name="comparer">The equality comparer.</param>
/// <param name="value">The value to find.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>The index of the first equal element, or -1.</returns>
/// <remarks>
/// O(n). The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableList`1.IndexOf(`0,System.Int32,System.Int32,System.Collections.Generic.IEqualityComparer{`0})"/>.
/// </remarks>
[<CompiledName "IndexOfWith">]
val indexOfWith : comparer : IEqualityComparer<'T> -> value : 'T -> list : IndexedList<'T> -> int

/// <summary>
/// Returns the index of the last element equal to a value, in <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>.
/// </summary>
/// <param name="value">The value to find.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>The index of the last equal element, or -1.</returns>
/// <remarks>
/// O(n). The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableList.LastIndexOf``1(System.Collections.Immutable.IImmutableList{``0},``0)"/>.
/// </remarks>
[<CompiledName "LastIndexOf">]
val lastIndexOf : value : 'T -> list : IndexedList<'T> -> int

/// <summary>
/// Returns the index of the last element of a range that equals a value, in
/// <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>. The search walks backward from the index of the
/// last element of the range.
/// </summary>
/// <param name="startIndex">The index of the last element of the range, where the search starts.</param>
/// <param name="count">The number of elements of the range, which ends at <paramref name="startIndex"/>.</param>
/// <param name="value">The value to find.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>The index in the indexed list, not in the range, of the last equal element of the range, or -1.</returns>
/// <exception cref="T:System.ArgumentOutOfRangeException">
/// Thrown when <paramref name="startIndex"/> or <paramref name="count"/> is negative, <paramref name="count"/> exceeds the
/// length, or a range of elements does not lie in the indexed list: its first element, at
/// <c>startIndex - count + 1</c>, is before the start, or its last element, at <paramref name="startIndex"/>, is at or after
/// the length.
/// </exception>
/// <remarks>
/// <para>
/// O(log n + count). A count of 0 is an empty range, which has no equal element, for any start index. The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableList`1.LastIndexOf(`0,System.Int32,System.Int32,System.Collections.Generic.IEqualityComparer{`0})"/>.
/// </para>
/// <para>
/// That member accepts a last element at or after the length and answers with an index that does not belong to the equal
/// element, and it throws a plain <see cref="T:System.ArgumentException"/> for a range that starts before the first
/// element. The function throws <see cref="T:System.ArgumentOutOfRangeException"/> for both (ADR 0001, D14).
/// </para>
/// </remarks>
[<CompiledName "LastIndexOfRange">]
val lastIndexOfRange : startIndex : int -> count : int -> value : 'T -> list : IndexedList<'T> -> int

/// <summary>
/// Returns the index of the last element of a range that a given equality comparer finds equal to a value. The search
/// walks backward from the index of the last element of the range.
/// </summary>
/// <param name="comparer">The equality comparer.</param>
/// <param name="startIndex">The index of the last element of the range, where the search starts.</param>
/// <param name="count">The number of elements of the range, which ends at <paramref name="startIndex"/>.</param>
/// <param name="value">The value to find.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>The index in the indexed list, not in the range, of the last equal element of the range, or -1.</returns>
/// <exception cref="T:System.ArgumentOutOfRangeException">
/// Thrown when <paramref name="startIndex"/> or <paramref name="count"/> is negative, <paramref name="count"/> exceeds the
/// length, or a range of elements does not lie in the indexed list: its first element, at
/// <c>startIndex - count + 1</c>, is before the start, or its last element, at <paramref name="startIndex"/>, is at or after
/// the length.
/// </exception>
/// <remarks>
/// <para>
/// O(log n + count). A count of 0 is an empty range, which has no equal element, for any start index. The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableList`1.LastIndexOf(`0,System.Int32,System.Int32,System.Collections.Generic.IEqualityComparer{`0})"/>.
/// </para>
/// <para>
/// That member accepts a last element at or after the length and answers with an index that does not belong to the equal
/// element, and it throws a plain <see cref="T:System.ArgumentException"/> for a range that starts before the first
/// element. The function throws <see cref="T:System.ArgumentOutOfRangeException"/> for both (ADR 0001, D14).
/// </para>
/// </remarks>
[<CompiledName "LastIndexOfRangeWith">]
val lastIndexOfRangeWith :
    comparer : IEqualityComparer<'T> -> startIndex : int -> count : int -> value : 'T -> list : IndexedList<'T> -> int

/// <summary>
/// Returns the index of the last element equal to a value in a given equality comparer.
/// </summary>
/// <param name="comparer">The equality comparer.</param>
/// <param name="value">The value to find.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>The index of the last equal element, or -1.</returns>
/// <remarks>
/// O(n). The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableList.LastIndexOf``1(System.Collections.Immutable.IImmutableList{``0},``0,System.Collections.Generic.IEqualityComparer{``0})"/>.
/// </remarks>
[<CompiledName "LastIndexOfWith">]
val lastIndexOfWith : comparer : IEqualityComparer<'T> -> value : 'T -> list : IndexedList<'T> -> int

/// <summary>
/// Returns the first value that the chooser returns for the elements.
/// </summary>
/// <param name="chooser">The function that returns a value, or <c>ValueNone</c> to go on.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>The first chosen value.</returns>
/// <exception cref="T:System.Collections.Generic.KeyNotFoundException">Thrown when the chooser returns <c>ValueNone</c> for every element.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Pick">]
val pick : chooser : ('T -> 'U voption) -> list : IndexedList<'T> -> 'U

/// <summary>
/// Returns the first element that satisfies the predicate, or <c>ValueNone</c>.
/// </summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>The first element that satisfies the predicate, or <c>ValueNone</c>.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "TryFind">]
val tryFind : predicate : ('T -> bool) -> list : IndexedList<'T> -> 'T voption

/// <summary>
/// Returns the last element that satisfies the predicate, or <c>ValueNone</c>.
/// </summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>The last element that satisfies the predicate, or <c>ValueNone</c>.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "TryFindBack">]
val tryFindBack : predicate : ('T -> bool) -> list : IndexedList<'T> -> 'T voption

/// <summary>
/// Returns the index of the first element that satisfies the predicate, or <c>ValueNone</c>.
/// </summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>The index of the first element that satisfies the predicate, or <c>ValueNone</c>.</returns>
/// <remarks>
/// O(n). The function wraps <see cref="M:System.Collections.Immutable.ImmutableList`1.FindIndex(System.Predicate{`0})"/>.
/// </remarks>
[<CompiledName "TryFindIndex">]
val tryFindIndex : predicate : ('T -> bool) -> list : IndexedList<'T> -> int voption

/// <summary>
/// Returns the index of the last element that satisfies the predicate, or <c>ValueNone</c>.
/// </summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>The index of the last element that satisfies the predicate, or <c>ValueNone</c>.</returns>
/// <remarks>
/// O(n). The function wraps <see cref="M:System.Collections.Immutable.ImmutableList`1.FindLastIndex(System.Predicate{`0})"/>.
/// </remarks>
[<CompiledName "TryFindIndexBack">]
val tryFindIndexBack : predicate : ('T -> bool) -> list : IndexedList<'T> -> int voption

/// <summary>
/// Returns the index of the last element of a range that satisfies the predicate, or <c>ValueNone</c>. The search walks
/// backward from the index of the last element of the range.
/// </summary>
/// <param name="startIndex">The index of the last element of the range, where the search starts.</param>
/// <param name="count">The number of elements of the range, which ends at <paramref name="startIndex"/>.</param>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>The index in the indexed list, not in the range, of the last element of the range that satisfies the predicate, or <c>ValueNone</c>.</returns>
/// <exception cref="T:System.ArgumentOutOfRangeException">
/// Thrown when <paramref name="startIndex"/> or <paramref name="count"/> is negative, <paramref name="count"/> exceeds the
/// length, or a range of elements does not lie in the indexed list: its first element, at
/// <c>startIndex - count + 1</c>, is before the start, or its last element, at <paramref name="startIndex"/>, is at or after
/// the length.
/// </exception>
/// <remarks>
/// <para>
/// O(log n + count). A count of 0 is an empty range, which has no element that satisfies the predicate, for any start
/// index. The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableList`1.FindLastIndex(System.Int32,System.Int32,System.Predicate{`0})"/>.
/// </para>
/// <para>
/// That member does not check a negative count and answers with the index of an element for a start index at or after
/// the length. The function throws <see cref="T:System.ArgumentOutOfRangeException"/> for both (ADR 0001, D14).
/// </para>
/// </remarks>
[<CompiledName "TryFindIndexBackRange">]
val tryFindIndexBackRange : startIndex : int -> count : int -> predicate : ('T -> bool) -> list : IndexedList<'T> -> int voption

/// <summary>
/// Returns the index of the first element of a range that satisfies the predicate, or <c>ValueNone</c>.
/// </summary>
/// <param name="startIndex">The index of the first element of the range.</param>
/// <param name="count">The number of elements of the range.</param>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>The index in the indexed list, not in the range, of the first element of the range that satisfies the predicate, or <c>ValueNone</c>.</returns>
/// <exception cref="T:System.ArgumentOutOfRangeException">
/// Thrown when <paramref name="startIndex"/> or <paramref name="count"/> is negative, or the range extends beyond the end
/// of the indexed list.
/// </exception>
/// <remarks>
/// O(log n + count). The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableList`1.FindIndex(System.Int32,System.Int32,System.Predicate{`0})"/>.
/// </remarks>
[<CompiledName "TryFindIndexRange">]
val tryFindIndexRange : startIndex : int -> count : int -> predicate : ('T -> bool) -> list : IndexedList<'T> -> int voption

/// <summary>
/// Returns the first value that the chooser returns for the elements, or <c>ValueNone</c>.
/// </summary>
/// <param name="chooser">The function that returns a value, or <c>ValueNone</c> to go on.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>The first chosen value, or <c>ValueNone</c>.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "TryPick">]
val tryPick : chooser : ('T -> 'U voption) -> list : IndexedList<'T> -> 'U voption

// ---- Transformation ----

/// <summary>
/// Returns an indexed list with a value added after the last element.
/// </summary>
/// <param name="value">The value to add.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>An indexed list of the elements and the value.</returns>
/// <remarks>
/// O(log n). The function wraps <see cref="M:System.Collections.Immutable.ImmutableList`1.Add(`0)"/>.
/// </remarks>
[<CompiledName "Add">]
val add : value : 'T -> list : IndexedList<'T> -> IndexedList<'T>

/// <summary>
/// Returns every element of the first indexed list paired with every element of the second one.
/// </summary>
/// <param name="list1">The first indexed list.</param>
/// <param name="list2">The second indexed list.</param>
/// <returns>The pairs, in the order of the first indexed list and then of the second one.</returns>
/// <remarks>
/// O(n × m).
/// </remarks>
[<CompiledName "AllPairs">]
val allPairs : list1 : IndexedList<'T1> -> list2 : IndexedList<'T2> -> IndexedList<struct ('T1 * 'T2)>

/// <summary>
/// Returns an indexed list of the elements of the first indexed list followed by the elements of the second one.
/// </summary>
/// <param name="list1">The first indexed list.</param>
/// <param name="list2">The second indexed list.</param>
/// <returns>The concatenation.</returns>
/// <remarks>
/// O(m log(n + m)). When the first indexed list is empty, the function returns the second one. The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableList`1.AddRange(System.Collections.Generic.IEnumerable{`0})"/>.
/// </remarks>
[<CompiledName "Append">]
val append : list1 : IndexedList<'T> -> list2 : IndexedList<'T> -> IndexedList<'T>

/// <summary>
/// Returns the values that the chooser returns for the elements.
/// </summary>
/// <param name="chooser">The function that returns a value, or <c>ValueNone</c> to skip the element.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>The chosen values.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Choose">]
val choose : chooser : ('T -> 'U voption) -> list : IndexedList<'T> -> IndexedList<'U>

/// <summary>Divides the indexed list into chunks of a given size.</summary>
/// <param name="chunkSize">The maximum size of a chunk.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>The chunks; the last one can be smaller.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when <paramref name="chunkSize"/> is not positive.</exception>
/// <remarks>
/// O(n). The chunk is the indexed list itself when it is as long as the indexed list.
/// </remarks>
[<CompiledName "ChunkBySize">]
val chunkBySize : chunkSize : int -> list : IndexedList<'T> -> IndexedList<IndexedList<'T>>

/// <summary>
/// Concatenates the indexed lists that a function returns for the elements.
/// </summary>
/// <param name="mapping">The function that returns an indexed list for each element.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>The concatenation of the returned indexed lists.</returns>
/// <remarks>
/// O(n + the total length of the returned indexed lists that have fewer than 16 elements), and O(log m) for each of the
/// longer ones, where m is the length of the result: the elements of a returned indexed list of at least 16 elements are
/// not copied, the result shares its nodes.
/// </remarks>
[<CompiledName "Collect">]
val collect : mapping : ('T -> IndexedList<'U>) -> list : IndexedList<'T> -> IndexedList<'U>

/// <summary>Concatenates a sequence of indexed lists.</summary>
/// <param name="lists">The indexed lists.</param>
/// <returns>The concatenation.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="lists"/> is <see langword="null"/> or a default flat list, or one of its indexed lists is <see langword="null"/>.</exception>
/// <remarks>
/// O(the total length of the indexed lists that have fewer than 16 elements), and O(log m) for each of the longer ones,
/// where m is the length of the result: the elements of an indexed list of at least 16 elements are not copied, the result
/// shares its nodes.
/// </remarks>
[<CompiledName "Concat">]
val concat : lists : seq<IndexedList<'T>> -> IndexedList<'T>

/// <summary>
/// Returns an indexed list with a value added before the first element.
/// </summary>
/// <param name="value">The value to add.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>An indexed list of the value and the elements.</returns>
/// <remarks>
/// O(log n). The function wraps <see cref="M:System.Collections.Immutable.ImmutableList`1.Insert(System.Int32,`0)"/>.
/// </remarks>
[<CompiledName "Cons">]
val cons : value : 'T -> list : IndexedList<'T> -> IndexedList<'T>

/// <summary>
/// Returns the elements that satisfy the predicate.
/// </summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>The elements that satisfy the predicate, in their order.</returns>
/// <remarks>
/// O(n). The function wraps <see cref="M:System.Collections.Immutable.ImmutableList`1.FindAll(System.Predicate{`0})"/>.
/// </remarks>
[<CompiledName "Filter">]
val filter : predicate : ('T -> bool) -> list : IndexedList<'T> -> IndexedList<'T>

/// <summary>Returns every element with its index.</summary>
/// <param name="list">The input indexed list.</param>
/// <returns>The pairs of index and element.</returns>
/// <remarks>O(n).</remarks>
[<CompiledName "Indexed">]
val indexed : list : IndexedList<'T> -> IndexedList<struct (int * 'T)>

/// <summary>
/// Returns an indexed list with a value inserted at an index.
/// </summary>
/// <param name="index">The index of the inserted value, from 0 to the length.</param>
/// <param name="value">The value to insert.</param>
/// <param name="source">The input indexed list.</param>
/// <returns>The indexed list with the value.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the index is negative or greater than the length.</exception>
/// <remarks>
/// O(log n). The function wraps <see cref="M:System.Collections.Immutable.ImmutableList`1.Insert(System.Int32,`0)"/>.
/// </remarks>
[<CompiledName "InsertAt">]
val insertAt : index : int -> value : 'T -> source : IndexedList<'T> -> IndexedList<'T>

/// <summary>
/// Returns an indexed list with values inserted at an index.
/// </summary>
/// <param name="index">The index of the first inserted value, from 0 to the length.</param>
/// <param name="values">The values to insert.</param>
/// <param name="source">The input indexed list.</param>
/// <returns>The indexed list with the values.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the index is negative or greater than the length.</exception>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="values"/> is <see langword="null"/> or a default flat list.</exception>
/// <remarks>
/// O(m log(n + m)). The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableList`1.InsertRange(System.Int32,System.Collections.Generic.IEnumerable{`0})"/>.
/// </remarks>
[<CompiledName "InsertManyAt">]
val insertManyAt : index : int -> values : seq<'T> -> source : IndexedList<'T> -> IndexedList<'T>

/// <summary>
/// Returns the results of a function applied to each element.
/// </summary>
/// <param name="mapping">The function to transform the elements, called in the order of the elements.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>The results, in the order of the elements.</returns>
/// <remarks>
/// O(n). The function wraps <see cref="M:System.Collections.Immutable.ImmutableList`1.ConvertAll``1(System.Func{`0,``0})"/>.
/// </remarks>
[<CompiledName "Map">]
val map : mapping : ('T -> 'U) -> list : IndexedList<'T> -> IndexedList<'U>

/// <summary>
/// Returns the results of a function applied to the pairs of elements at the same index.
/// </summary>
/// <param name="mapping">The function to transform the pairs.</param>
/// <param name="list1">The first indexed list.</param>
/// <param name="list2">The second indexed list.</param>
/// <returns>The results.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the indexed lists have different lengths.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Map2">]
val map2 : mapping : ('T1 -> 'T2 -> 'U) -> list1 : IndexedList<'T1> -> list2 : IndexedList<'T2> -> IndexedList<'U>

/// <summary>
/// Returns the results of a function applied to the triples of elements at the same index.
/// </summary>
/// <param name="mapping">The function to transform the triples.</param>
/// <param name="list1">The first indexed list.</param>
/// <param name="list2">The second indexed list.</param>
/// <param name="list3">The third indexed list.</param>
/// <returns>The results.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the indexed lists have different lengths.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Map3">]
val map3 :
    mapping : ('T1 -> 'T2 -> 'T3 -> 'U) ->
    list1 : IndexedList<'T1> ->
    list2 : IndexedList<'T2> ->
    list3 : IndexedList<'T3> ->
        IndexedList<'U>

/// <summary>
/// Returns the results of a function applied to each element and its index.
/// </summary>
/// <param name="mapping">The function to transform the elements.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>The results.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "MapIndexed">]
val mapi : mapping : (int -> 'T -> 'U) -> list : IndexedList<'T> -> IndexedList<'U>

/// <summary>
/// Returns the results of a function applied to the index and the pair of elements at that index.
/// </summary>
/// <param name="mapping">The function to transform the pairs.</param>
/// <param name="list1">The first indexed list.</param>
/// <param name="list2">The second indexed list.</param>
/// <returns>The results.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the indexed lists have different lengths.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "MapIndexed2">]
val mapi2 : mapping : (int -> 'T1 -> 'T2 -> 'U) -> list1 : IndexedList<'T1> -> list2 : IndexedList<'T2> -> IndexedList<'U>

/// <summary>Returns every element with its successor.</summary>
/// <param name="list">The input indexed list.</param>
/// <returns>The pairs of neighbours; empty for fewer than two elements.</returns>
/// <remarks>O(n).</remarks>
[<CompiledName "Pairwise">]
val pairwise : list : IndexedList<'T> -> IndexedList<struct ('T * 'T)>

/// <summary>
/// Returns the elements moved to the indices that a function computes.
/// </summary>
/// <param name="indexMap">The function that maps the index of each element to its new index.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>The permuted indexed list.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the function does not compute a permutation.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Permute">]
val permute : indexMap : (int -> int) -> list : IndexedList<'T> -> IndexedList<'T>

/// <summary>
/// Returns the indexed list without the first element that equals a value, in
/// <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>.
/// </summary>
/// <param name="item">The value to remove.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>The indexed list without the element; the input itself when no element equals the value.</returns>
/// <remarks>
/// <para>
/// O(n). The function wraps <see cref="M:System.Collections.Immutable.ImmutableList`1.Remove(`0)"/>.
/// </para>
/// <para>
/// To remove several values, use
/// <see cref="M:FSharp.Collections.Immutable.IndexedList.RemoveMany``1(System.Collections.Generic.IEnumerable{``0},System.Collections.Immutable.ImmutableList{``0})"/>;
/// to remove by index, use
/// <see cref="M:FSharp.Collections.Immutable.IndexedList.RemoveAt``1(System.Int32,System.Collections.Immutable.ImmutableList{``0})"/>;
/// to remove the elements that satisfy a predicate, use
/// <see cref="M:FSharp.Collections.Immutable.IndexedList.Filter``1(Microsoft.FSharp.Core.FSharpFunc{``0,System.Boolean},System.Collections.Immutable.ImmutableList{``0})"/>
/// with the negated predicate.
/// </para>
/// </remarks>
[<CompiledName "Remove">]
val remove : item : 'T -> list : IndexedList<'T> -> IndexedList<'T>

/// <summary>
/// Returns an indexed list without the element at an index.
/// </summary>
/// <param name="index">The index of the removed element.</param>
/// <param name="source">The input indexed list.</param>
/// <returns>The indexed list without the element.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the index is out of range.</exception>
/// <remarks>
/// O(log n). The function wraps <see cref="M:System.Collections.Immutable.ImmutableList`1.RemoveAt(System.Int32)"/>.
/// </remarks>
[<CompiledName "RemoveAt">]
val removeAt : index : int -> source : IndexedList<'T> -> IndexedList<'T>

/// <summary>
/// Returns the indexed list without one element for each of the given values, in
/// <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>.
/// </summary>
/// <param name="items">The values to remove.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>The indexed list without the elements; the input itself when no element equals any of the values.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="items"/> is <see langword="null"/> or a default flat list.</exception>
/// <remarks>
/// <para>
/// O(n × m) for m values. Each value removes the first element that equals it and that no earlier value has removed; a
/// value that equals no remaining element removes nothing. The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableList`1.RemoveRange(System.Collections.Generic.IEnumerable{`0})"/>.
/// </para>
/// <para>
/// To remove every element that equals one of the values, and the duplicates of the other elements, use
/// <see cref="M:FSharp.Collections.Immutable.IndexedList.Except``1(System.Collections.Generic.IEnumerable{``0},System.Collections.Immutable.ImmutableList{``0})"/>.
/// </para>
/// </remarks>
[<CompiledName "RemoveMany">]
val removeMany : items : seq<'T> -> list : IndexedList<'T> -> IndexedList<'T>

/// <summary>
/// Returns an indexed list without a range of elements.
/// </summary>
/// <param name="index">The index of the first removed element.</param>
/// <param name="count">
/// The number of removed elements. A count that is not positive removes nothing, and so does a count whose sum with the
/// index exceeds <see cref="F:System.Int32.MaxValue"/>, as in
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.RemoveManyAt``1(System.Int32,System.Int32,Microsoft.FSharp.Collections.FSharpList{``0})"/>.
/// </param>
/// <param name="source">The input indexed list.</param>
/// <returns>The indexed list without the range.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the index is negative or the range ends beyond the indexed list.</exception>
/// <remarks>
/// O(count × log n). The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableList`1.RemoveRange(System.Int32,System.Int32)"/>.
/// </remarks>
[<CompiledName "RemoveManyAt">]
val removeManyAt : index : int -> count : int -> source : IndexedList<'T> -> IndexedList<'T>

/// <summary>
/// Returns the indexed list without one element for each of the given values, in a given equality comparer.
/// </summary>
/// <param name="comparer">The equality comparer.</param>
/// <param name="items">The values to remove.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>The indexed list without the elements; the input itself when no element equals any of the values.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="items"/> is <see langword="null"/> or a default flat list.</exception>
/// <remarks>
/// O(n × m) for m values. Each value removes the first element that the comparer finds equal to it and that no earlier
/// value has removed; a value that equals no remaining element removes nothing. The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableList`1.RemoveRange(System.Collections.Generic.IEnumerable{`0},System.Collections.Generic.IEqualityComparer{`0})"/>.
/// </remarks>
[<CompiledName "RemoveManyWith">]
val removeManyWith : comparer : IEqualityComparer<'T> -> items : seq<'T> -> list : IndexedList<'T> -> IndexedList<'T>

/// <summary>
/// Returns the indexed list without the first element that equals a value in a given equality comparer.
/// </summary>
/// <param name="comparer">The equality comparer.</param>
/// <param name="item">The value to remove.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>The indexed list without the element; the input itself when no element equals the value.</returns>
/// <remarks>
/// O(n). The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableList`1.Remove(`0,System.Collections.Generic.IEqualityComparer{`0})"/>.
/// </remarks>
[<CompiledName "RemoveWith">]
val removeWith : comparer : IEqualityComparer<'T> -> item : 'T -> list : IndexedList<'T> -> IndexedList<'T>

/// <summary>
/// Returns the indexed list with the first element that equals a value replaced by another value, in
/// <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>.
/// </summary>
/// <param name="oldValue">The value to replace.</param>
/// <param name="newValue">The new value.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>The indexed list with the new value in place of the element.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when no element equals <paramref name="oldValue"/>.</exception>
/// <remarks>
/// <para>
/// O(n). The function wraps <see cref="M:System.Collections.Immutable.ImmutableList`1.Replace(`0,`0)"/>.
/// </para>
/// <para>
/// To replace every element that equals a value, use
/// <see cref="M:FSharp.Collections.Immutable.IndexedList.Map``2(Microsoft.FSharp.Core.FSharpFunc{``0,``1},System.Collections.Immutable.ImmutableList{``0})"/>;
/// to replace the element at an index, use
/// <see cref="M:FSharp.Collections.Immutable.IndexedList.UpdateAt``1(System.Int32,``0,System.Collections.Immutable.ImmutableList{``0})"/>.
/// </para>
/// </remarks>
[<CompiledName "Replace">]
val replace : oldValue : 'T -> newValue : 'T -> list : IndexedList<'T> -> IndexedList<'T>

/// <summary>
/// Returns the indexed list with the first element that equals a value in a given equality comparer replaced by another
/// value.
/// </summary>
/// <param name="comparer">The equality comparer.</param>
/// <param name="oldValue">The value to replace.</param>
/// <param name="newValue">The new value.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>The indexed list with the new value in place of the element.</returns>
/// <exception cref="T:System.ArgumentException">
/// Thrown when the comparer finds no element equal to <paramref name="oldValue"/>.
/// </exception>
/// <remarks>
/// O(n). The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableList`1.Replace(`0,`0,System.Collections.Generic.IEqualityComparer{`0})"/>.
/// </remarks>
[<CompiledName "ReplaceWith">]
val replaceWith : comparer : IEqualityComparer<'T> -> oldValue : 'T -> newValue : 'T -> list : IndexedList<'T> -> IndexedList<'T>

/// <summary>
/// Returns the elements in reverse order.
/// </summary>
/// <param name="list">The input indexed list.</param>
/// <returns>The reversed indexed list.</returns>
/// <remarks>
/// O(n). The function copies the elements to an array, reverses it with
/// <see cref="M:System.Array.Reverse``1(``0[])"/> and builds a new tree with
/// <see cref="M:System.Collections.Immutable.ImmutableList.CreateRange``1(System.Collections.Generic.IEnumerable{``0})"/>.
/// At 100 000 integers, this takes about 3 ms where
/// <see cref="M:System.Collections.Immutable.ImmutableList`1.Reverse"/> takes about 23 ms; the copy of the elements is
/// the only extra allocation.
/// </remarks>
[<CompiledName "Reverse">]
val rev : list : IndexedList<'T> -> IndexedList<'T>

/// <summary>
/// Returns the indexed list with the elements of a range in reverse order.
/// </summary>
/// <param name="index">The index of the first element of the range.</param>
/// <param name="count">The number of elements of the range.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>The indexed list with the reversed range; the input itself when the range has at most one element.</returns>
/// <exception cref="T:System.ArgumentOutOfRangeException">
/// Thrown when <paramref name="index"/> or <paramref name="count"/> is negative, or the range extends beyond the end of
/// the indexed list.
/// </exception>
/// <remarks>
/// O(count × log n). The elements outside the range keep their places. The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableList`1.Reverse(System.Int32,System.Int32)"/>.
/// </remarks>
[<CompiledName "ReverseRange">]
val revRange : index : int -> count : int -> list : IndexedList<'T> -> IndexedList<'T>

/// <summary>
/// Returns the transpose of a sequence of indexed lists of the same length.
/// </summary>
/// <param name="lists">The indexed lists.</param>
/// <returns>An indexed list whose element i holds the element i of every input indexed list.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="lists"/> is <see langword="null"/> or a default flat list, or one of its indexed lists is <see langword="null"/>.</exception>
/// <exception cref="T:System.ArgumentException">Thrown when the indexed lists have different lengths.</exception>
/// <remarks>
/// O(the total length).
/// </remarks>
[<CompiledName "Transpose">]
val transpose : lists : seq<IndexedList<'T>> -> IndexedList<IndexedList<'T>>

/// <summary>Splits an indexed list of pairs into two indexed lists.</summary>
/// <param name="list">The input indexed list.</param>
/// <returns>The first components and the second components.</returns>
/// <remarks>O(n).</remarks>
[<CompiledName "Unzip">]
val unzip : list : IndexedList<struct ('T1 * 'T2)> -> struct (IndexedList<'T1> * IndexedList<'T2>)

/// <summary>Splits an indexed list of triples into three indexed lists.</summary>
/// <param name="list">The input indexed list.</param>
/// <returns>The first, the second and the third components.</returns>
/// <remarks>O(n).</remarks>
[<CompiledName "Unzip3">]
val unzip3 : list : IndexedList<struct ('T1 * 'T2 * 'T3)> -> struct (IndexedList<'T1> * IndexedList<'T2> * IndexedList<'T3>)

/// <summary>
/// Returns an indexed list with the element at an index replaced by a value.
/// </summary>
/// <param name="index">The index of the replaced element.</param>
/// <param name="value">The new value.</param>
/// <param name="source">The input indexed list.</param>
/// <returns>The indexed list with the new value.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the index is out of range.</exception>
/// <remarks>
/// O(log n). The function wraps <see cref="M:System.Collections.Immutable.ImmutableList`1.SetItem(System.Int32,`0)"/>.
/// </remarks>
[<CompiledName "UpdateAt">]
val updateAt : index : int -> value : 'T -> source : IndexedList<'T> -> IndexedList<'T>

/// <summary>
/// Returns the elements that satisfy the predicate.
/// </summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>The elements that satisfy the predicate, in their order.</returns>
/// <remarks>
/// O(n). A synonym of <see cref="M:FSharp.Collections.Immutable.IndexedList.Filter``1(Microsoft.FSharp.Core.FSharpFunc{``0,System.Boolean},System.Collections.Immutable.ImmutableList{``0})"/>.
/// </remarks>
[<CompiledName "Where">]
val where : predicate : ('T -> bool) -> list : IndexedList<'T> -> IndexedList<'T>

/// <summary>
/// Returns the windows of a given size that slide over the indexed list.
/// </summary>
/// <param name="windowSize">The size of a window.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>The windows; empty when the indexed list is shorter than a window.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when <paramref name="windowSize"/> is not positive.</exception>
/// <remarks>
/// O(n × windowSize). The window is the indexed list itself when it is as long as the indexed list.
/// </remarks>
[<CompiledName "Windowed">]
val windowed : windowSize : int -> list : IndexedList<'T> -> IndexedList<IndexedList<'T>>

/// <summary>Combines two indexed lists into an indexed list of pairs.</summary>
/// <param name="list1">The first indexed list.</param>
/// <param name="list2">The second indexed list.</param>
/// <returns>The pairs of elements at the same index.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the indexed lists have different lengths.</exception>
/// <remarks>O(n).</remarks>
[<CompiledName "Zip">]
val zip : list1 : IndexedList<'T1> -> list2 : IndexedList<'T2> -> IndexedList<struct ('T1 * 'T2)>

/// <summary>
/// Combines three indexed lists into an indexed list of triples.
/// </summary>
/// <param name="list1">The first indexed list.</param>
/// <param name="list2">The second indexed list.</param>
/// <param name="list3">The third indexed list.</param>
/// <returns>The triples of elements at the same index.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the indexed lists have different lengths.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Zip3">]
val zip3 :
    list1 : IndexedList<'T1> -> list2 : IndexedList<'T2> -> list3 : IndexedList<'T3> -> IndexedList<struct ('T1 * 'T2 * 'T3)>

// ---- Folding ----

/// <summary>Returns the average of the elements.</summary>
/// <param name="list">The input indexed list.</param>
/// <returns>The average.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the indexed list is empty.</exception>
/// <remarks>O(n).</remarks>
[<CompiledName "Average">]
val inline average :
    list : IndexedList< ^T > -> ^T
        when ^T : (static member (+) : ^T * ^T -> ^T)
        and ^T : (static member DivideByInt : ^T * int -> ^T)
        and ^T : (static member Zero : ^T)

/// <summary>
/// Returns the average of the results of a function applied to each element.
/// </summary>
/// <param name="projection">The function to transform the elements.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>The average of the results.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the indexed list is empty.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "AverageBy">]
val inline averageBy :
    projection : ('T -> ^U) -> list : IndexedList<'T> -> ^U
        when ^U : (static member (+) : ^U * ^U -> ^U)
        and ^U : (static member DivideByInt : ^U * int -> ^U)
        and ^U : (static member Zero : ^U)

/// <summary>
/// Compares two indexed lists element by element with a comparer.
/// </summary>
/// <param name="comparer">The function that compares two elements.</param>
/// <param name="list1">The first indexed list.</param>
/// <param name="list2">The second indexed list.</param>
/// <returns>
/// The first non-zero result of the comparer, or the comparison of the lengths when every common element compares equal.
/// </returns>
/// <remarks>
/// O(n). The function walks the two indexed lists together and stops at the first pair of elements that the comparer
/// does not consider equal.
/// </remarks>
[<CompiledName "CompareWith">]
val compareWith : comparer : ('T -> 'T -> int) -> list1 : IndexedList<'T> -> list2 : IndexedList<'T> -> int

/// <summary>
/// Applies a function to a state and each element, from the first element to the last one.
/// </summary>
/// <param name="folder">The function that computes the next state.</param>
/// <param name="state">The initial state.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>The final state.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Fold">]
val fold : folder : ('State -> 'T -> 'State) -> state : 'State -> list : IndexedList<'T> -> 'State

/// <summary>
/// Applies a function to a state and each pair of elements at the same index, from the first index to the last one.
/// </summary>
/// <param name="folder">The function that computes the next state.</param>
/// <param name="state">The initial state.</param>
/// <param name="list1">The first indexed list.</param>
/// <param name="list2">The second indexed list.</param>
/// <returns>The final state.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the indexed lists have different lengths.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Fold2">]
val fold2 :
    folder : ('State -> 'T1 -> 'T2 -> 'State) -> state : 'State -> list1 : IndexedList<'T1> -> list2 : IndexedList<'T2> -> 'State

/// <summary>
/// Applies a function to each element and a state, from the last element to the first one.
/// </summary>
/// <param name="folder">The function that computes the next state.</param>
/// <param name="list">The input indexed list.</param>
/// <param name="state">The initial state.</param>
/// <returns>The final state.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "FoldBack">]
val foldBack : folder : ('T -> 'State -> 'State) -> list : IndexedList<'T> -> state : 'State -> 'State

/// <summary>
/// Applies a function to each pair of elements at the same index and a state, from the last index to the first one.
/// </summary>
/// <param name="folder">The function that computes the next state.</param>
/// <param name="list1">The first indexed list.</param>
/// <param name="list2">The second indexed list.</param>
/// <param name="state">The initial state.</param>
/// <returns>The final state.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the indexed lists have different lengths.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "FoldBack2">]
val foldBack2 :
    folder : ('T1 -> 'T2 -> 'State -> 'State) -> list1 : IndexedList<'T1> -> list2 : IndexedList<'T2> -> state : 'State -> 'State

/// <summary>
/// Applies an action to each element.
/// </summary>
/// <param name="action">The action.</param>
/// <param name="list">The input indexed list.</param>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Iterate">]
val iter : action : ('T -> unit) -> list : IndexedList<'T> -> unit

/// <summary>
/// Applies an action to each pair of elements at the same index.
/// </summary>
/// <param name="action">The action.</param>
/// <param name="list1">The first indexed list.</param>
/// <param name="list2">The second indexed list.</param>
/// <exception cref="T:System.ArgumentException">Thrown when the indexed lists have different lengths.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Iterate2">]
val iter2 : action : ('T1 -> 'T2 -> unit) -> list1 : IndexedList<'T1> -> list2 : IndexedList<'T2> -> unit

/// <summary>Applies an action to each element and its index.</summary>
/// <param name="action">The action.</param>
/// <param name="list">The input indexed list.</param>
/// <remarks>O(n).</remarks>
[<CompiledName "IterateIndexed">]
val iteri : action : (int -> 'T -> unit) -> list : IndexedList<'T> -> unit

/// <summary>
/// Applies an action to the index and the pair of elements at that index.
/// </summary>
/// <param name="action">The action.</param>
/// <param name="list1">The first indexed list.</param>
/// <param name="list2">The second indexed list.</param>
/// <exception cref="T:System.ArgumentException">Thrown when the indexed lists have different lengths.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "IterateIndexed2">]
val iteri2 : action : (int -> 'T1 -> 'T2 -> unit) -> list1 : IndexedList<'T1> -> list2 : IndexedList<'T2> -> unit

/// <summary>
/// Maps each element and threads a state through the mapping, from the first element to the last one.
/// </summary>
/// <param name="mapping">The function that returns the result for an element and the next state.</param>
/// <param name="state">The initial state.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>The results and the final state.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "MapFold">]
val mapFold :
    mapping : ('State -> 'T -> struct ('Result * 'State)) ->
    state : 'State ->
    list : IndexedList<'T> ->
        struct (IndexedList<'Result> * 'State)

/// <summary>
/// Maps each element and threads a state through the mapping, from the last element to the first one.
/// </summary>
/// <param name="mapping">The function that returns the result for an element and the next state.</param>
/// <param name="list">The input indexed list.</param>
/// <param name="state">The initial state.</param>
/// <returns>The results, in the order of the elements, and the final state.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "MapFoldBack">]
val mapFoldBack :
    mapping : ('T -> 'State -> struct ('Result * 'State)) ->
    list : IndexedList<'T> ->
    state : 'State ->
        struct (IndexedList<'Result> * 'State)

/// <summary>Returns the greatest element, in F# generic comparison.</summary>
/// <param name="list">The input indexed list.</param>
/// <returns>The greatest element.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the indexed list is empty.</exception>
/// <remarks>
/// O(n). The function is inline, as <see cref="M:Microsoft.FSharp.Collections.ListModule.Max``1(Microsoft.FSharp.Collections.FSharpList{``0})"/>
/// is, so that the comparison is compiled for the type of the elements and boxes nothing.
/// </remarks>
[<CompiledName "Max">]
val inline max : list : IndexedList<'T> -> 'T when 'T : comparison

/// <summary>
/// Returns the element with the greatest result of a projection.
/// </summary>
/// <param name="projection">The function to transform the elements for the comparison.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>The first element with the greatest result.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the indexed list is empty.</exception>
/// <remarks>
/// O(n). The function is inline, as <see cref="M:Microsoft.FSharp.Collections.ListModule.MaxBy``2(Microsoft.FSharp.Core.FSharpFunc{``0,``1},Microsoft.FSharp.Collections.FSharpList{``0})"/>
/// is, so that the comparison is compiled for the type of the results and boxes nothing.
/// </remarks>
[<CompiledName "MaxBy">]
val inline maxBy : projection : ('T -> 'U) -> list : IndexedList<'T> -> 'T when 'U : comparison

/// <summary>Returns the smallest element, in F# generic comparison.</summary>
/// <param name="list">The input indexed list.</param>
/// <returns>The smallest element.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the indexed list is empty.</exception>
/// <remarks>
/// O(n). The function is inline, as <see cref="M:Microsoft.FSharp.Collections.ListModule.Min``1(Microsoft.FSharp.Collections.FSharpList{``0})"/>
/// is, so that the comparison is compiled for the type of the elements and boxes nothing.
/// </remarks>
[<CompiledName "Min">]
val inline min : list : IndexedList<'T> -> 'T when 'T : comparison

/// <summary>
/// Returns the element with the smallest result of a projection.
/// </summary>
/// <param name="projection">The function to transform the elements for the comparison.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>The first element with the smallest result.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the indexed list is empty.</exception>
/// <remarks>
/// O(n). The function is inline, as <see cref="M:Microsoft.FSharp.Collections.ListModule.MinBy``2(Microsoft.FSharp.Core.FSharpFunc{``0,``1},Microsoft.FSharp.Collections.FSharpList{``0})"/>
/// is, so that the comparison is compiled for the type of the results and boxes nothing.
/// </remarks>
[<CompiledName "MinBy">]
val inline minBy : projection : ('T -> 'U) -> list : IndexedList<'T> -> 'T when 'U : comparison

/// <summary>
/// Combines the elements with a function, from the first element to the last one.
/// </summary>
/// <param name="reduction">The function that combines the result so far with the next element.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>The combined value.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the indexed list is empty.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Reduce">]
val reduce : reduction : ('T -> 'T -> 'T) -> list : IndexedList<'T> -> 'T

/// <summary>
/// Combines the elements with a function, from the last element to the first one.
/// </summary>
/// <param name="reduction">The function that combines an element with the result so far.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>The combined value.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the indexed list is empty.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "ReduceBack">]
val reduceBack : reduction : ('T -> 'T -> 'T) -> list : IndexedList<'T> -> 'T

/// <summary>
/// Returns every state of a fold from the first element to the last one, the initial state first.
/// </summary>
/// <param name="folder">The function that computes the next state.</param>
/// <param name="state">The initial state.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>The states; one more than the elements.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Scan">]
val scan : folder : ('State -> 'T -> 'State) -> state : 'State -> list : IndexedList<'T> -> IndexedList<'State>

/// <summary>
/// Returns every state of a fold from the last element to the first one, the initial state last.
/// </summary>
/// <param name="folder">The function that computes the next state.</param>
/// <param name="list">The input indexed list.</param>
/// <param name="state">The initial state.</param>
/// <returns>The states; one more than the elements.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "ScanBack">]
val scanBack : folder : ('T -> 'State -> 'State) -> list : IndexedList<'T> -> state : 'State -> IndexedList<'State>

/// <summary>Returns the sum of the elements.</summary>
/// <param name="list">The input indexed list.</param>
/// <returns>The sum; zero for an empty indexed list.</returns>
/// <remarks>O(n).</remarks>
[<CompiledName "Sum">]
val inline sum : list : IndexedList< ^T > -> ^T when ^T : (static member (+) : ^T * ^T -> ^T) and ^T : (static member Zero : ^T)

/// <summary>
/// Returns the sum of the results of a function applied to each element.
/// </summary>
/// <param name="projection">The function to transform the elements.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>The sum of the results; zero for an empty indexed list.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "SumBy">]
val inline sumBy :
    projection : ('T -> ^U) -> list : IndexedList<'T> -> ^U
        when ^U : (static member (+) : ^U * ^U -> ^U) and ^U : (static member Zero : ^U)

// ---- Partitioning and grouping ----

/// <summary>Counts the elements of each key of a projection.</summary>
/// <param name="projection">The function that returns the key of an element.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>The keys, in the order of their first occurrence, with the number of their elements.</returns>
/// <remarks>O(n). The keys compare in F# structural equality.</remarks>
[<CompiledName "CountBy">]
val countBy : projection : ('T -> 'Key) -> list : IndexedList<'T> -> IndexedList<struct ('Key * int)> when 'Key : equality

/// <summary>
/// Returns the elements without duplicates, in F# structural equality.
/// </summary>
/// <param name="list">The input indexed list.</param>
/// <returns>The first occurrence of each element, in their order.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Distinct">]
val distinct : list : IndexedList<'T> -> IndexedList<'T> when 'T : equality

/// <summary>
/// Returns the elements without duplicates of the result of a projection.
/// </summary>
/// <param name="projection">The function that returns the key of an element.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>The first element of each key, in their order.</returns>
/// <remarks>
/// O(n). The keys compare in F# structural equality.
/// </remarks>
[<CompiledName "DistinctBy">]
val distinctBy : projection : ('T -> 'Key) -> list : IndexedList<'T> -> IndexedList<'T> when 'Key : equality

/// <summary>
/// Returns the distinct elements that do not equal any of the excluded items.
/// </summary>
/// <param name="itemsToExclude">The items to remove.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>The distinct remaining elements, in their order.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="itemsToExclude"/> is <see langword="null"/> or a default flat list.</exception>
/// <remarks>
/// O(n + m). An empty indexed list is returned without a look at the items to exclude.
/// </remarks>
[<CompiledName "Except">]
val except : itemsToExclude : seq<'T> -> list : IndexedList<'T> -> IndexedList<'T> when 'T : equality

/// <summary>Groups the elements by the key of a projection.</summary>
/// <param name="projection">The function that returns the key of an element.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>The keys, in the order of their first occurrence, with their elements in their order.</returns>
/// <remarks>O(n). The keys compare in F# structural equality.</remarks>
[<CompiledName "GroupBy">]
val groupBy :
    projection : ('T -> 'Key) -> list : IndexedList<'T> -> IndexedList<struct ('Key * IndexedList<'T>)> when 'Key : equality

/// <summary>Splits the elements by a predicate.</summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>The elements that satisfy the predicate and the other ones, both in their order.</returns>
/// <remarks>O(n).</remarks>
[<CompiledName "Partition">]
val partition : predicate : ('T -> bool) -> list : IndexedList<'T> -> struct (IndexedList<'T> * IndexedList<'T>)

/// <summary>
/// Splits the elements by the choice that a function returns for them.
/// </summary>
/// <param name="partitioner">The function that returns <c>Choice1Of2</c> or <c>Choice2Of2</c> for an element.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>The values of the first choices and of the second choices, both in the order of the elements.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "PartitionWith">]
val partitionWith :
    partitioner : ('T -> Choice<'T1, 'T2>) -> list : IndexedList<'T> -> struct (IndexedList<'T1> * IndexedList<'T2>)

/// <summary>
/// Returns the indexed list without its first elements.
/// </summary>
/// <param name="count">The number of elements to skip; a negative count skips nothing.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>The remaining elements.</returns>
/// <exception cref="T:System.ArgumentOutOfRangeException">Thrown when the count exceeds the length.</exception>
/// <remarks>
/// O(log n + k), where k is the length of the result, which is a new tree, unless it is empty or the whole indexed list.
/// </remarks>
[<CompiledName "Skip">]
val skip : count : int -> list : IndexedList<'T> -> IndexedList<'T>

/// <summary>
/// Returns the indexed list without its last elements.
/// </summary>
/// <param name="count">The number of elements to skip; a negative count skips nothing.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>The remaining elements.</returns>
/// <exception cref="T:System.ArgumentOutOfRangeException">Thrown when the count exceeds the length.</exception>
/// <remarks>
/// O(log n + k), where k is the length of the result, which is a new tree, unless it is empty or the whole indexed list.
/// The checks follow <see cref="M:Microsoft.FSharp.Collections.ListModule.Skip``1(System.Int32,Microsoft.FSharp.Collections.FSharpList{``0})"/>.
/// </remarks>
[<CompiledName "SkipLast">]
val skipLast : count : int -> list : IndexedList<'T> -> IndexedList<'T>

/// <summary>
/// Returns the indexed list without its first elements that satisfy the predicate.
/// </summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>The elements from the first one that does not satisfy the predicate.</returns>
/// <remarks>
/// O(n + k), where k is the length of the result.
/// </remarks>
[<CompiledName "SkipWhile">]
val skipWhile : predicate : ('T -> bool) -> list : IndexedList<'T> -> IndexedList<'T>

/// <summary>
/// Splits the indexed list at an index.
/// </summary>
/// <param name="index">The index of the first element of the second part.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>The elements before the index and the elements from the index.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the index is negative.</exception>
/// <exception cref="T:System.InvalidOperationException">Thrown when the index exceeds the length.</exception>
/// <remarks>
/// O(n). Each part is a new tree, unless it is empty or the whole indexed list.
/// </remarks>
[<CompiledName "SplitAt">]
val splitAt : index : int -> list : IndexedList<'T> -> struct (IndexedList<'T> * IndexedList<'T>)

/// <summary>
/// Splits the indexed list into at most a given number of parts of nearly equal length.
/// </summary>
/// <param name="count">The maximum number of parts.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>The parts; the first ones are one element longer when the length does not divide evenly.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when <paramref name="count"/> is not positive.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "SplitInto">]
val splitInto : count : int -> list : IndexedList<'T> -> IndexedList<IndexedList<'T>>

/// <summary>
/// Returns the elements of a range of the indexed list.
/// </summary>
/// <param name="list">The input indexed list.</param>
/// <param name="startIndex">The index of the first element of the range.</param>
/// <param name="count">The number of elements of the range.</param>
/// <returns>The elements of the range, in their order.</returns>
/// <exception cref="T:System.ArgumentException">
/// Thrown when <paramref name="startIndex"/> or <paramref name="count"/> is negative.
/// </exception>
/// <exception cref="T:System.ArgumentOutOfRangeException">
/// Thrown when the range extends beyond the end of the indexed list.
/// </exception>
/// <remarks>
/// <para>
/// O(log n + count). The result is a new tree, unless it is empty or the whole indexed list. A range that
/// <see cref="M:FSharp.Collections.Immutable.IndexedList.Skip``1(System.Int32,System.Collections.Immutable.ImmutableList{``0})"/>
/// and then
/// <see cref="M:FSharp.Collections.Immutable.IndexedList.Take``1(System.Int32,System.Collections.Immutable.ImmutableList{``0})"/>
/// take builds two trees, and this function builds one: in a measurement, 1 000 elements from the middle of an indexed
/// list of 1 000 000 elements took 0.1 ms and allocated 52 KB, where skip followed by take took 46 ms and allocated 26 MB.
/// </para>
/// <para>
/// The function throws what <see cref="M:Microsoft.FSharp.Collections.ArrayModule.GetSubArray``1(``0[],System.Int32,System.Int32)"/>
/// throws, as <see cref="M:FSharp.Collections.Immutable.FlatList.GetSubArray``1(System.Collections.Immutable.ImmutableArray{``0},System.Int32,System.Int32)"/>
/// does.
/// </para>
/// </remarks>
[<CompiledName "GetRange">]
val sub : list : IndexedList<'T> -> startIndex : int -> count : int -> IndexedList<'T>

/// <summary>
/// Returns the first elements of the indexed list.
/// </summary>
/// <param name="count">The number of elements to take.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>The first elements.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the count is negative.</exception>
/// <exception cref="T:System.InvalidOperationException">Thrown when the count exceeds the length.</exception>
/// <remarks>
/// O(log n + count). The result is a new tree, unless it is empty or the whole indexed list.
/// </remarks>
[<CompiledName "Take">]
val take : count : int -> list : IndexedList<'T> -> IndexedList<'T>

/// <summary>
/// Returns the last elements of the indexed list.
/// </summary>
/// <param name="count">The number of elements to take.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>The last elements.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the count is negative.</exception>
/// <exception cref="T:System.InvalidOperationException">Thrown when the count exceeds the length.</exception>
/// <remarks>
/// O(log n + count). The result is a new tree, unless it is empty or the whole indexed list. The checks follow
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.Take``1(System.Int32,Microsoft.FSharp.Collections.FSharpList{``0})"/>.
/// </remarks>
[<CompiledName "TakeLast">]
val takeLast : count : int -> list : IndexedList<'T> -> IndexedList<'T>

/// <summary>
/// Returns the first elements of the indexed list that satisfy the predicate.
/// </summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>The elements before the first one that does not satisfy the predicate.</returns>
/// <remarks>
/// O(k), where k is the length of the result.
/// </remarks>
[<CompiledName "TakeWhile">]
val takeWhile : predicate : ('T -> bool) -> list : IndexedList<'T> -> IndexedList<'T>

/// <summary>
/// Returns at most a given number of first elements.
/// </summary>
/// <param name="count">The maximum number of elements; a count that is not positive returns an empty indexed list.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>The first elements.</returns>
/// <remarks>
/// O(log n + k), where k is the length of the result, which is a new tree, unless it is empty or the whole indexed list.
/// </remarks>
[<CompiledName "Truncate">]
val truncate : count : int -> list : IndexedList<'T> -> IndexedList<'T>

// ---- Sorting ----

/// <summary>
/// Sorts the elements in the order of F# generic comparison.
/// </summary>
/// <param name="list">The input indexed list.</param>
/// <returns>The sorted indexed list.</returns>
/// <exception cref="T:System.InvalidOperationException">
/// Thrown when the comparison of two elements throws, as in <see cref="M:Microsoft.FSharp.Collections.ListModule.Sort``1(Microsoft.FSharp.Collections.FSharpList{``0})"/>.
/// </exception>
/// <remarks>
/// O(n log n). This is a stable sort, as <see cref="M:Microsoft.FSharp.Collections.ListModule.Sort``1(Microsoft.FSharp.Collections.FSharpList{``0})"/>:
/// equal elements keep their order.
/// </remarks>
[<CompiledName "Sort">]
val sort : list : IndexedList<'T> -> IndexedList<'T> when 'T : comparison

/// <summary>
/// Sorts the elements by the result of a projection, in the order of F# generic comparison.
/// </summary>
/// <param name="projection">The function that returns the key of an element.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>The sorted indexed list.</returns>
/// <remarks>
/// O(n log n). This is a stable sort: elements with equal keys keep their order.
/// </remarks>
[<CompiledName "SortBy">]
val sortBy : projection : ('T -> 'Key) -> list : IndexedList<'T> -> IndexedList<'T> when 'Key : comparison

/// <summary>
/// Sorts the elements by the result of a projection, in descending order.
/// </summary>
/// <param name="projection">The function that returns the key of an element.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>The sorted indexed list.</returns>
/// <exception cref="T:System.InvalidOperationException">
/// Thrown when the projection or the comparison of two keys throws: the projection runs in the comparison, as in
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.SortByDescending``2(Microsoft.FSharp.Core.FSharpFunc{``0,``1},Microsoft.FSharp.Collections.FSharpList{``0})"/>.
/// </exception>
/// <remarks>
/// O(n log n). This is a stable sort: elements with equal keys keep their order.
/// </remarks>
[<CompiledName "SortByDescending">]
val sortByDescending : projection : ('T -> 'Key) -> list : IndexedList<'T> -> IndexedList<'T> when 'Key : comparison

/// <summary>
/// Sorts the elements in descending order of F# generic comparison.
/// </summary>
/// <param name="list">The input indexed list.</param>
/// <returns>The sorted indexed list.</returns>
/// <exception cref="T:System.InvalidOperationException">
/// Thrown when the comparison of two elements throws, as in <see cref="M:Microsoft.FSharp.Collections.ListModule.SortDescending``1(Microsoft.FSharp.Collections.FSharpList{``0})"/>.
/// </exception>
/// <remarks>
/// O(n log n). This is a stable sort: equal elements keep their order.
/// </remarks>
[<CompiledName "SortDescending">]
val sortDescending : list : IndexedList<'T> -> IndexedList<'T> when 'T : comparison

/// <summary>
/// Sorts a range of the elements in the order of F# generic comparison.
/// </summary>
/// <param name="index">The index of the first element of the range.</param>
/// <param name="count">The number of elements of the range.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>The indexed list with the sorted range; the input itself when the range has at most one element.</returns>
/// <exception cref="T:System.ArgumentOutOfRangeException">
/// Thrown when <paramref name="index"/> or <paramref name="count"/> is negative, or the range extends beyond the end of
/// the indexed list.
/// </exception>
/// <exception cref="T:System.InvalidOperationException">Thrown when the comparison of two elements throws.</exception>
/// <remarks>
/// <para>
/// O(count × log n). This is a stable sort: elements that compare equal keep their order. The elements outside the range
/// keep their places.
/// </para>
/// <para>
/// The function sorts a copy of the range with
/// <see cref="M:System.Linq.Enumerable.Order``1(System.Collections.Generic.IEnumerable{``0},System.Collections.Generic.IComparer{``0})"/>
/// and puts it back with
/// <see cref="M:System.Collections.Immutable.ImmutableList`1.RemoveRange(System.Int32,System.Int32)"/> and
/// <see cref="M:System.Collections.Immutable.ImmutableList`1.InsertRange(System.Int32,System.Collections.Generic.IEnumerable{`0})"/>,
/// not with the unstable <see cref="M:System.Collections.Immutable.ImmutableList`1.Sort(System.Int32,System.Int32,System.Collections.Generic.IComparer{`0})"/>,
/// which copies the whole indexed list: in a measurement, 10 000 elements of an indexed list of 1 000 000 elements took
/// 2.3 ms and allocated 1.6 MB, where that member took 68 ms and allocated 52 MB.
/// </para>
/// </remarks>
[<CompiledName "SortRange">]
val sortRange : index : int -> count : int -> list : IndexedList<'T> -> IndexedList<'T> when 'T : comparison

/// <summary>
/// Sorts a range of the elements with a comparison function.
/// </summary>
/// <param name="comparison">The function that compares two elements.</param>
/// <param name="index">The index of the first element of the range.</param>
/// <param name="count">The number of elements of the range.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>The indexed list with the sorted range; the input itself when the range has at most one element.</returns>
/// <exception cref="T:System.ArgumentOutOfRangeException">
/// Thrown when <paramref name="index"/> or <paramref name="count"/> is negative, or the range extends beyond the end of
/// the indexed list.
/// </exception>
/// <exception cref="T:System.InvalidOperationException">Thrown when the comparison function throws.</exception>
/// <remarks>
/// O(count × log n). This is a stable sort: elements that compare equal keep their order. The elements outside the range
/// keep their places. The function sorts a copy of the range as
/// <see cref="M:FSharp.Collections.Immutable.IndexedList.SortRange``1(System.Int32,System.Int32,System.Collections.Immutable.ImmutableList{``0})"/>
/// does.
/// </remarks>
[<CompiledName "SortRangeWith">]
val sortRangeWith : comparison : ('T -> 'T -> int) -> index : int -> count : int -> list : IndexedList<'T> -> IndexedList<'T>

/// <summary>
/// Sorts a range of the elements with a comparer.
/// </summary>
/// <param name="comparer">The comparer.</param>
/// <param name="index">The index of the first element of the range.</param>
/// <param name="count">The number of elements of the range.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>The indexed list with the sorted range; the input itself when the range has at most one element.</returns>
/// <exception cref="T:System.ArgumentOutOfRangeException">
/// Thrown when <paramref name="index"/> or <paramref name="count"/> is negative, or the range extends beyond the end of
/// the indexed list.
/// </exception>
/// <exception cref="T:System.InvalidOperationException">Thrown when the comparer throws.</exception>
/// <remarks>
/// O(count × log n). This is a stable sort: elements that compare equal keep their order. The elements outside the range
/// keep their places. The function sorts a copy of the range as
/// <see cref="M:FSharp.Collections.Immutable.IndexedList.SortRange``1(System.Int32,System.Int32,System.Collections.Immutable.ImmutableList{``0})"/>
/// does.
/// </remarks>
[<CompiledName "SortRangeWithComparer">]
val sortRangeWithComparer : comparer : IComparer<'T> -> index : int -> count : int -> list : IndexedList<'T> -> IndexedList<'T>

/// <summary>
/// Sorts the elements with a comparison function.
/// </summary>
/// <param name="comparer">The function that compares two elements.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>The sorted indexed list.</returns>
/// <exception cref="T:System.InvalidOperationException">
/// Thrown when the comparer throws, as in
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.SortWith``1(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpFunc{``0,System.Int32}},Microsoft.FSharp.Collections.FSharpList{``0})"/>.
/// </exception>
/// <remarks>
/// O(n log n). This is a stable sort: elements that compare equal keep their order.
/// </remarks>
[<CompiledName "SortWith">]
val sortWith : comparer : ('T -> 'T -> int) -> list : IndexedList<'T> -> IndexedList<'T>

/// <summary>
/// Sorts the elements with a comparer.
/// </summary>
/// <param name="comparer">The comparer.</param>
/// <param name="list">The input indexed list.</param>
/// <returns>The sorted indexed list.</returns>
/// <remarks>
/// O(n log n). This is a stable sort: elements that compare equal keep their order. The function wraps
/// <see cref="M:System.Linq.Enumerable.Order``1(System.Collections.Generic.IEnumerable{``0},System.Collections.Generic.IComparer{``0})"/>,
/// not the unstable <see cref="M:System.Collections.Immutable.ImmutableList`1.Sort(System.Collections.Generic.IComparer{`0})"/>.
/// </remarks>
[<CompiledName "SortWithComparer">]
val sortWithComparer : comparer : IComparer<'T> -> list : IndexedList<'T> -> IndexedList<'T>

// ---- Random ----

/// <summary>
/// Returns a random element, chosen with <see cref="P:System.Random.Shared"/>.
/// </summary>
/// <param name="source">The input indexed list.</param>
/// <returns>A random element.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the indexed list is empty.</exception>
/// <remarks>
/// O(log n).
/// </remarks>
[<CompiledName "RandomChoice">]
val randomChoice : source : IndexedList<'T> -> 'T

/// <summary>
/// Returns a random element, chosen with a function that returns numbers in [0, 1).
/// </summary>
/// <param name="randomizer">The function that returns a random number in [0, 1).</param>
/// <param name="source">The input indexed list.</param>
/// <returns>A random element.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the indexed list is empty.</exception>
/// <exception cref="T:System.ArgumentOutOfRangeException">Thrown when the randomizer returns a number outside [0, 1).</exception>
/// <remarks>
/// O(log n).
/// </remarks>
[<CompiledName "RandomChoiceBy">]
val randomChoiceBy : randomizer : (unit -> float) -> source : IndexedList<'T> -> 'T

/// <summary>
/// Returns a random element, chosen with a given random number generator.
/// </summary>
/// <param name="random">The random number generator.</param>
/// <param name="source">The input indexed list.</param>
/// <returns>A random element.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="random"/> is <see langword="null"/>.</exception>
/// <exception cref="T:System.ArgumentException">Thrown when the indexed list is empty.</exception>
/// <remarks>
/// O(log n).
/// </remarks>
[<CompiledName "RandomChoiceWith">]
val randomChoiceWith : random : Random -> source : IndexedList<'T> -> 'T

/// <summary>
/// Returns random elements, chosen with <see cref="P:System.Random.Shared"/>; an element can repeat.
/// </summary>
/// <param name="count">The number of elements to choose.</param>
/// <param name="source">The input indexed list.</param>
/// <returns>The chosen elements.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the count is negative, or positive for an empty indexed list.</exception>
/// <remarks>
/// O(n + count).
/// </remarks>
[<CompiledName "RandomChoices">]
val randomChoices : count : int -> source : IndexedList<'T> -> IndexedList<'T>

/// <summary>
/// Returns random elements, chosen with a function that returns numbers in [0, 1); an element can repeat.
/// </summary>
/// <param name="randomizer">The function that returns a random number in [0, 1).</param>
/// <param name="count">The number of elements to choose.</param>
/// <param name="source">The input indexed list.</param>
/// <returns>The chosen elements.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the count is negative, or positive for an empty indexed list.</exception>
/// <exception cref="T:System.ArgumentOutOfRangeException">Thrown when the randomizer returns a number outside [0, 1).</exception>
/// <remarks>
/// O(n + count).
/// </remarks>
[<CompiledName "RandomChoicesBy">]
val randomChoicesBy : randomizer : (unit -> float) -> count : int -> source : IndexedList<'T> -> IndexedList<'T>

/// <summary>
/// Returns random elements, chosen with a given random number generator; an element can repeat.
/// </summary>
/// <param name="random">The random number generator.</param>
/// <param name="count">The number of elements to choose.</param>
/// <param name="source">The input indexed list.</param>
/// <returns>The chosen elements.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="random"/> is <see langword="null"/>.</exception>
/// <exception cref="T:System.ArgumentException">Thrown when the count is negative, or positive for an empty indexed list.</exception>
/// <remarks>
/// O(n + count).
/// </remarks>
[<CompiledName "RandomChoicesWith">]
val randomChoicesWith : random : Random -> count : int -> source : IndexedList<'T> -> IndexedList<'T>

/// <summary>
/// Returns random elements at distinct indices, chosen with <see cref="P:System.Random.Shared"/>.
/// </summary>
/// <param name="count">The number of elements to choose.</param>
/// <param name="source">The input indexed list.</param>
/// <returns>The chosen elements.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the count is negative or exceeds the length.</exception>
/// <remarks>
/// O(n + count).
/// </remarks>
[<CompiledName "RandomSample">]
val randomSample : count : int -> source : IndexedList<'T> -> IndexedList<'T>

/// <summary>
/// Returns random elements at distinct indices, chosen with a function that returns numbers in [0, 1).
/// </summary>
/// <param name="randomizer">The function that returns a random number in [0, 1).</param>
/// <param name="count">The number of elements to choose.</param>
/// <param name="source">The input indexed list.</param>
/// <returns>The chosen elements.</returns>
/// <exception cref="T:System.ArgumentException">Thrown when the count is negative or exceeds the length.</exception>
/// <exception cref="T:System.ArgumentOutOfRangeException">Thrown when the randomizer returns a number outside [0, 1).</exception>
/// <remarks>
/// O(n + count).
/// </remarks>
[<CompiledName "RandomSampleBy">]
val randomSampleBy : randomizer : (unit -> float) -> count : int -> source : IndexedList<'T> -> IndexedList<'T>

/// <summary>
/// Returns random elements at distinct indices, chosen with a given random number generator.
/// </summary>
/// <param name="random">The random number generator.</param>
/// <param name="count">The number of elements to choose.</param>
/// <param name="source">The input indexed list.</param>
/// <returns>The chosen elements.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="random"/> is <see langword="null"/>.</exception>
/// <exception cref="T:System.ArgumentException">Thrown when the count is negative or exceeds the length.</exception>
/// <remarks>
/// O(n + count).
/// </remarks>
[<CompiledName "RandomSampleWith">]
val randomSampleWith : random : Random -> count : int -> source : IndexedList<'T> -> IndexedList<'T>

/// <summary>
/// Returns the elements in a random order, chosen with <see cref="P:System.Random.Shared"/>.
/// </summary>
/// <param name="source">The input indexed list.</param>
/// <returns>The shuffled indexed list.</returns>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "RandomShuffle">]
val randomShuffle : source : IndexedList<'T> -> IndexedList<'T>

/// <summary>
/// Returns the elements in a random order, chosen with a function that returns numbers in [0, 1).
/// </summary>
/// <param name="randomizer">The function that returns a random number in [0, 1).</param>
/// <param name="source">The input indexed list.</param>
/// <returns>The shuffled indexed list.</returns>
/// <exception cref="T:System.ArgumentOutOfRangeException">Thrown when the randomizer returns a number outside [0, 1).</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "RandomShuffleBy">]
val randomShuffleBy : randomizer : (unit -> float) -> source : IndexedList<'T> -> IndexedList<'T>

/// <summary>
/// Returns the elements in a random order, chosen with a given random number generator.
/// </summary>
/// <param name="random">The random number generator.</param>
/// <param name="source">The input indexed list.</param>
/// <returns>The shuffled indexed list.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="random"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "RandomShuffleWith">]
val randomShuffleWith : random : Random -> source : IndexedList<'T> -> IndexedList<'T>

// ---- Conversion ----

/// <summary>
/// Copies a range of the indexed list into an array.
/// </summary>
/// <param name="source">The input indexed list.</param>
/// <param name="sourceIndex">The index of the first element to copy.</param>
/// <param name="target">The array to copy to.</param>
/// <param name="targetIndex">The index in the target array of the first copied element.</param>
/// <param name="count">The number of elements to copy.</param>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="target"/> is <see langword="null"/>.</exception>
/// <exception cref="T:System.ArgumentOutOfRangeException">Thrown when an index or the count is negative, or a range is outside its indexed list or array.</exception>
/// <remarks>
/// O(log n + count). The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableList`1.CopyTo(System.Int32,`0[],System.Int32,System.Int32)"/>
/// and reports the names of its own parameters.
/// </remarks>
[<CompiledName "CopyTo">]
val blit : source : IndexedList<'T> -> sourceIndex : int -> target : 'T array -> targetIndex : int -> count : int -> unit

/// <summary>Returns a new array with the elements.</summary>
/// <param name="list">The input indexed list.</param>
/// <returns>A copy of the elements.</returns>
/// <remarks>O(n).</remarks>
[<CompiledName "ToArray">]
val toArray : list : IndexedList<'T> -> 'T array

/// <summary>Returns a list of the elements.</summary>
/// <param name="list">The input indexed list.</param>
/// <returns>The list of the elements.</returns>
/// <remarks>O(n).</remarks>
[<CompiledName "ToList">]
val toList : list : IndexedList<'T> -> 'T list

/// <summary>Returns the indexed list as a sequence.</summary>
/// <param name="list">The input indexed list.</param>
/// <returns>The sequence of the elements.</returns>
/// <remarks>O(1).</remarks>
[<CompiledName "ToSeq">]
val toSeq : list : IndexedList<'T> -> seq<'T>
