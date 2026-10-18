/// <summary>
/// Functions for <see cref="T:FSharp.Collections.Immutable.SortedSet`1"/>, the immutable sorted set.
/// </summary>
/// <remarks>
/// <para>
/// The module mirrors <see cref="T:Microsoft.FSharp.Collections.SetModule"/>: each function has the semantics, the
/// argument order and the exceptions of the function with the same name in that module (ADR 0001). Functions that
/// FSharp.Core lacks wrap a member of <see cref="T:System.Collections.Immutable.ImmutableSortedSet`1"/> and throw what that
/// member throws. Every function throws <see cref="T:System.ArgumentNullException"/> for a <see langword="null"/> sorted set.
/// </para>
/// <para>
/// The creation functions create sorted sets that compare with F# generic comparison. Such a sorted set enumerates its
/// elements in the order of the F# set of the same elements: strings, for example, compare ordinally and not in the order
/// of the current culture.
/// <see cref="M:FSharp.Collections.Immutable.SortedSet.ForComparer``1(System.Collections.Generic.IComparer{``0})"/>
/// creates a <see cref="T:FSharp.Collections.Immutable.CustomComparerSortedSet`1"/>, whose members create sorted sets
/// with another comparer (ADR 0001, D12).
/// </para>
/// <para>
/// The functions that visit the elements visit them in ascending order of the comparer of the sorted set;
/// <see cref="M:FSharp.Collections.Immutable.SortedSet.FoldBack``2(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpFunc{``1,``1}},System.Collections.Immutable.ImmutableSortedSet{``0},``1)"/>
/// and
/// <see cref="M:FSharp.Collections.Immutable.SortedSet.PartitionWith``3(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpChoice{``1,``2}},System.Collections.Immutable.ImmutableSortedSet{``0})"/>
/// visit them in descending order, as the functions of the same name in
/// <see cref="T:Microsoft.FSharp.Collections.SetModule"/> do.
/// </para>
/// <para>
/// The transformations that keep the element type keep the comparer of their input; the set algebra keeps the comparer of
/// the first set. The transformations that change the element type compare the new elements with F# generic comparison.
/// </para>
/// <para>
/// Functions that return an optional value return a <see cref="T:Microsoft.FSharp.Core.FSharpValueOption`1"/>, and functions
/// that return tuples return struct tuples. The namespaces <see cref="N:FSharp.Collections.Immutable.Option"/> and
/// <see cref="N:FSharp.Collections.Immutable.Tuple"/> contain a module with the same name whose functions use options
/// and reference tuples instead.
/// </para>
/// </remarks>
[<RequireQualifiedAccess>]
module FSharp.Collections.Immutable.SortedSet

open System
open System.Collections.Generic
open System.Collections.Immutable
// Opened after System.Collections.Generic, so that SortedSet names the abbreviation of this library, not the mutable set
open FSharp.Collections.Immutable

// ---- Creation ----

/// <summary>
/// The empty sorted set, which compares with F# generic comparison.
/// </summary>
/// <remarks>
/// O(1). The value is <see cref="F:System.Collections.Immutable.ImmutableSortedSet`1.Empty"/> with the comparer
/// <see cref="M:Microsoft.FSharp.Core.LanguagePrimitives.FastGenericComparer``1"/>.
/// </remarks>
[<GeneralizableValue>]
[<CompiledName "Empty">]
val empty<'T when 'T : comparison> : SortedSet<'T>

/// <summary>
/// Creates the struct that creates sorted sets that order their elements with a comparer.
/// </summary>
/// <param name="comparer">The comparer of the elements.</param>
/// <returns>
/// The <see cref="T:FSharp.Collections.Immutable.CustomComparerSortedSet`1"/> whose members are the creation functions of
/// this module with the comparer: they create sorted sets and builders that order with it.
/// </returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="comparer"/> is <see langword="null"/>.</exception>
/// <remarks>
/// <para>
/// O(1). Creating the struct allocates nothing. The element type needs no F# comparison, because the comparer orders the
/// elements.
/// <see cref="M:FSharp.Collections.Immutable.CustomComparerSortedSet`1.OfSeq(System.Collections.Generic.IEnumerable{`0})"/>
/// also gives an existing sorted set the comparer (ADR 0001, D12).
/// </para>
/// <para>
/// A <see langword="null"/> comparer throws, where the BCL members <c>Create</c>, <c>CreateRange</c>,
/// <c>CreateBuilder</c> and <c>WithComparer</c> take it as
/// <see cref="P:System.Collections.Generic.Comparer`1.Default"/>. For that meaning use the default value of the struct,
/// which orders with <see cref="P:System.Collections.Generic.Comparer`1.Default"/>. The function
/// <see cref="M:FSharp.Collections.Immutable.SortedSet.OfSeq``1(System.Collections.Generic.IEnumerable{``0})"/> gives a
/// sorted set F# generic comparison instead, the default comparer of this module.
/// </para>
/// </remarks>
/// <example id="forComparer-1">
/// <code lang="fsharp">
/// let names = (SortedSet.forComparer StringComparer.OrdinalIgnoreCase).ofSeq [ "bob"; "Ann"; "ANN" ]
/// </code>
/// Evaluates to a sorted set of "Ann" and "bob", in this order, that compares without case.
/// </example>
[<CompiledName "ForComparer">]
val forComparer : comparer : IComparer<'T> -> CustomComparerSortedSet<'T>

/// <summary>
/// Creates a sorted set from the elements of an array.
/// </summary>
/// <param name="array">The input array.</param>
/// <returns>The sorted set of the distinct elements of the array, which compares with F# generic comparison.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="array"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n log n). Of the elements that compare equal, the sorted set keeps the first one, as
/// <see cref="M:Microsoft.FSharp.Collections.SetModule.OfArray``1(``0[])"/> does. Later changes to the array do not change
/// the sorted set.
/// </remarks>
[<CompiledName "OfArray">]
val ofArray : array : 'T array -> SortedSet<'T> when 'T : comparison

/// <summary>
/// Creates a sorted set from the elements of a list.
/// </summary>
/// <param name="elements">The input list.</param>
/// <returns>The sorted set of the distinct elements of the list, which compares with F# generic comparison.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="elements"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n log n). Of the elements that compare equal, the sorted set keeps the first one, as
/// <see cref="M:Microsoft.FSharp.Collections.SetModule.OfList``1(Microsoft.FSharp.Collections.FSharpList{``0})"/> does.
/// </remarks>
[<CompiledName "OfList">]
val ofList : elements : 'T list -> SortedSet<'T> when 'T : comparison

/// <summary>
/// Creates a sorted set from the elements of a read-only span.
/// </summary>
/// <param name="span">The input span.</param>
/// <returns>
/// The sorted set of the distinct elements of the span, which compares with F# generic comparison; an empty span gives the
/// empty sorted set.
/// </returns>
/// <remarks>
/// <para>
/// O(n log n). Of the elements that compare equal, the sorted set keeps the first one, as
/// <see cref="M:Microsoft.FSharp.Collections.SetModule.OfArray``1(``0[])"/> does. Later changes to the memory under the
/// span do not change the sorted set.
/// </para>
/// <para>
/// The function adds the elements one by one to a builder. It does not wrap
/// <see cref="M:System.Collections.Immutable.ImmutableSortedSet.Create``1(System.ReadOnlySpan{``0})"/>, which sorts the
/// elements with an unstable sort and can keep any of the elements that compare equal.
/// </para>
/// </remarks>
[<CompiledName "OfReadOnlySpan">]
val ofReadOnlySpan : span : ReadOnlySpan<'T> -> SortedSet<'T> when 'T : comparison

/// <summary>
/// Creates a sorted set from the elements of a sequence.
/// </summary>
/// <param name="elements">The input sequence.</param>
/// <returns>The sorted set of the distinct elements of the sequence, which compares with F# generic comparison.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="elements"/> is <see langword="null"/> or a default flat list.</exception>
/// <remarks>
/// O(n log n), or O(1) for a sorted set that has the comparer instance which this module uses for the element type: the
/// function returns such a sorted set without a copy, and gives a sorted set with another comparer F# generic comparison.
/// Of the elements that compare equal, the sorted set keeps the first one, as
/// <see cref="M:Microsoft.FSharp.Collections.SetModule.OfSeq``1(System.Collections.Generic.IEnumerable{``0})"/> does.
/// </remarks>
[<CompiledName "OfSeq">]
val ofSeq : elements : seq<'T> -> SortedSet<'T> when 'T : comparison

/// <summary>
/// Creates a sorted set from the elements of a span.
/// </summary>
/// <param name="span">The input span.</param>
/// <returns>
/// The sorted set of the distinct elements of the span, which compares with F# generic comparison; an empty span gives the
/// empty sorted set.
/// </returns>
/// <remarks>
/// O(n log n). The function converts the span to a read-only span and does what
/// <see cref="M:FSharp.Collections.Immutable.SortedSet.OfReadOnlySpan``1(System.ReadOnlySpan{``0})"/> does: of the
/// elements that compare equal, the sorted set keeps the first one.
/// </remarks>
[<CompiledName "OfSpan">]
val ofSpan : span : Span<'T> -> SortedSet<'T> when 'T : comparison

/// <summary>Creates a sorted set of one element.</summary>
/// <param name="value">The element.</param>
/// <returns>The sorted set of one element, which compares with F# generic comparison.</returns>
/// <remarks>O(1).</remarks>
[<CompiledName "Singleton">]
val singleton : value : 'T -> SortedSet<'T> when 'T : comparison

// ---- Builder ----

/// <summary>
/// Creates a sorted set with a builder that an action fills.
/// </summary>
/// <param name="action">
/// The action that adds the elements to the builder, which compares with F# generic comparison.
/// </param>
/// <returns>The sorted set of the elements that the action added.</returns>
/// <remarks>
/// O(1) apart from the cost of the action. The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableSortedSet.CreateBuilder``1(System.Collections.Generic.IComparer{``0})"/>
/// and <see cref="M:System.Collections.Immutable.ImmutableSortedSet`1.Builder.ToImmutable"/>.
/// </remarks>
[<CompiledName "Build">]
val build : action : (SortedSet<'T>.Builder -> unit) -> SortedSet<'T> when 'T : comparison

/// <summary>
/// Creates an empty builder that compares with F# generic comparison.
/// </summary>
/// <returns>The builder.</returns>
/// <remarks>
/// O(1). The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableSortedSet.CreateBuilder``1(System.Collections.Generic.IComparer{``0})"/>
/// with the comparer <see cref="M:Microsoft.FSharp.Core.LanguagePrimitives.FastGenericComparer``1"/>.
/// </remarks>
[<CompiledName "CreateBuilder">]
val builder : unit -> SortedSet<'T>.Builder when 'T : comparison

/// <summary>
/// Returns the contents of a builder as a sorted set.
/// </summary>
/// <param name="builder">The builder, which stays usable.</param>
/// <returns>The sorted set of the elements of the builder, with the comparer of the builder.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="builder"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(1) apart from freezing the nodes that changed since the builder last returned a sorted set: the sorted set shares
/// the nodes of the builder instead of copying the elements. The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableSortedSet`1.Builder.ToImmutable"/>.
/// </remarks>
[<CompiledName "OfBuilder">]
val ofBuilder : builder : SortedSet<'T>.Builder -> SortedSet<'T>

/// <summary>
/// Creates a builder that contains the elements of a sorted set.
/// </summary>
/// <param name="set">The input sorted set, which stays unchanged.</param>
/// <returns>The builder, with the comparer of the sorted set.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(1). The function wraps <see cref="M:System.Collections.Immutable.ImmutableSortedSet`1.ToBuilder"/>. The builder
/// shares the nodes of the sorted set until it changes them.
/// </remarks>
[<CompiledName "ToBuilder">]
val toBuilder : set : SortedSet<'T> -> SortedSet<'T>.Builder

/// <summary>
/// Creates a sorted set from a builder over the elements of a sorted set, after an action changes the builder.
/// </summary>
/// <param name="action">The action that changes the builder, which has the comparer of the sorted set.</param>
/// <param name="set">The input sorted set, which stays unchanged.</param>
/// <returns>The sorted set of the elements of the changed builder, with the comparer of the input.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(1) apart from the cost of the action. The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableSortedSet`1.ToBuilder"/> and
/// <see cref="M:System.Collections.Immutable.ImmutableSortedSet`1.Builder.ToImmutable"/>. It makes the in-place
/// operations of the builder, such as <c>IntersectWith</c> and <c>SymmetricExceptWith</c>, one expression.
/// </remarks>
[<CompiledName "Update">]
val update : action : (SortedSet<'T>.Builder -> unit) -> set : SortedSet<'T> -> SortedSet<'T>

// ---- Properties ----

/// <summary>
/// Returns the comparer that orders the elements of the sorted set.
/// </summary>
/// <param name="set">The input sorted set.</param>
/// <returns>The comparer.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(1). The function wraps <see cref="P:System.Collections.Immutable.ImmutableSortedSet`1.KeyComparer"/>.
/// </remarks>
[<CompiledName "Comparer">]
val comparer : set : SortedSet<'T> -> IComparer<'T>

/// <summary>Returns the number of elements of the sorted set.</summary>
/// <param name="set">The input sorted set.</param>
/// <returns>The number of elements.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>O(1).</remarks>
[<CompiledName "Count">]
val count : set : SortedSet<'T> -> int

/// <summary>Returns whether the sorted set has no elements.</summary>
/// <param name="set">The input sorted set.</param>
/// <returns><see langword="true"/> when the sorted set is empty.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>O(1).</remarks>
[<CompiledName "IsEmpty">]
val isEmpty : set : SortedSet<'T> -> bool

// ---- Access ----

/// <summary>
/// Returns the index of the element equal to a value, in the comparer of the sorted set.
/// </summary>
/// <param name="value">The value to find.</param>
/// <param name="set">The input sorted set.</param>
/// <returns>
/// The index of the equal element in ascending order. When no element equals the value, the bitwise complement of the
/// index of the first larger element, or the bitwise complement of the number of elements when no element is larger.
/// </returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(log n). The function wraps <see cref="M:System.Collections.Immutable.ImmutableSortedSet`1.IndexOf(`0)"/>. A missing
/// value gives a negative result, whose bitwise complement is the index at which the value would be inserted.
/// </remarks>
[<CompiledName "IndexOf">]
val indexOf : value : 'T -> set : SortedSet<'T> -> int

/// <summary>
/// Returns the element at an index, in ascending order.
/// </summary>
/// <param name="index">The index of the element.</param>
/// <param name="set">The input sorted set.</param>
/// <returns>The element at the index.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <exception cref="T:System.ArgumentOutOfRangeException">Thrown when the index is negative or not less than the number of elements.</exception>
/// <remarks>
/// O(log n). The function wraps the indexer
/// <see cref="P:System.Collections.Immutable.ImmutableSortedSet`1.Item(System.Int32)"/> and throws what it throws:
/// <see cref="T:Microsoft.FSharp.Collections.SetModule"/> has no function to mirror.
/// </remarks>
[<CompiledName "Item">]
val item : index : int -> set : SortedSet<'T> -> 'T

/// <summary>
/// Returns the largest element, in the order of the comparer of the sorted set.
/// </summary>
/// <param name="set">The input sorted set.</param>
/// <returns>The last element in ascending order.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <exception cref="T:System.ArgumentException">Thrown when the sorted set is empty.</exception>
/// <remarks>
/// O(log n). The function throws for an empty sorted set, as
/// <see cref="M:Microsoft.FSharp.Collections.SetModule.MaxElement``1(Microsoft.FSharp.Collections.FSharpSet{``0})"/>
/// does, and reads the last element with the indexer
/// <see cref="P:System.Collections.Immutable.ImmutableSortedSet`1.Item(System.Int32)"/>.
/// <see cref="P:System.Collections.Immutable.ImmutableSortedSet`1.Max"/> would return the default value of the element
/// type for an empty sorted set.
/// </remarks>
[<CompiledName "MaxElement">]
val maxElement : set : SortedSet<'T> -> 'T

/// <summary>
/// Returns the smallest element, in the order of the comparer of the sorted set.
/// </summary>
/// <param name="set">The input sorted set.</param>
/// <returns>The first element in ascending order.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <exception cref="T:System.ArgumentException">Thrown when the sorted set is empty.</exception>
/// <remarks>
/// O(log n). The function throws for an empty sorted set, as
/// <see cref="M:Microsoft.FSharp.Collections.SetModule.MinElement``1(Microsoft.FSharp.Collections.FSharpSet{``0})"/>
/// does, and reads the first element with the indexer
/// <see cref="P:System.Collections.Immutable.ImmutableSortedSet`1.Item(System.Int32)"/>.
/// <see cref="P:System.Collections.Immutable.ImmutableSortedSet`1.Min"/> would return the default value of the element
/// type for an empty sorted set.
/// </remarks>
[<CompiledName "MinElement">]
val minElement : set : SortedSet<'T> -> 'T

/// <summary>
/// Returns the element of the sorted set that equals a value, or <c>ValueNone</c> when there is none.
/// </summary>
/// <param name="value">The value to search for.</param>
/// <param name="set">The input sorted set.</param>
/// <returns>The element that the comparer of the sorted set considers equal to the value, or <c>ValueNone</c>.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(log n). The function wraps <see cref="M:System.Collections.Immutable.ImmutableSortedSet`1.TryGetValue(`0,`0@)"/>. The
/// element can differ from an equal value, for example in case under a case-insensitive comparer; the function returns
/// the instance that the sorted set holds.
/// </remarks>
[<CompiledName "TryGetValue">]
val tryGetValue : value : 'T -> set : SortedSet<'T> -> 'T voption

/// <summary>
/// Returns the element at an index, in ascending order, or <c>ValueNone</c> when the index is out of range.
/// </summary>
/// <param name="index">The index of the element.</param>
/// <param name="set">The input sorted set.</param>
/// <returns>The element at the index, or <c>ValueNone</c>.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(log n). The function checks the index against the number of elements and wraps the indexer
/// <see cref="P:System.Collections.Immutable.ImmutableSortedSet`1.Item(System.Int32)"/>.
/// </remarks>
[<CompiledName "TryItem">]
val tryItem : index : int -> set : SortedSet<'T> -> 'T voption

// ---- Search ----

/// <summary>
/// Returns whether the sorted set contains an element equal to a value, in the comparer of the sorted set.
/// </summary>
/// <param name="element">The value to find.</param>
/// <param name="set">The input sorted set.</param>
/// <returns><see langword="true"/> when an element equals the value.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(log n).
/// </remarks>
[<CompiledName "Contains">]
val contains : element : 'T -> set : SortedSet<'T> -> bool

/// <summary>
/// Returns whether any element satisfies the predicate.
/// </summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="set">The input sorted set.</param>
/// <returns><see langword="true"/> when an element satisfies the predicate.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n). The function tests the elements in ascending order and stops at the first one that satisfies the predicate.
/// </remarks>
[<CompiledName "Exists">]
val exists : predicate : ('T -> bool) -> set : SortedSet<'T> -> bool

/// <summary>
/// Returns whether all elements satisfy the predicate.
/// </summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="set">The input sorted set.</param>
/// <returns><see langword="true"/> when every element satisfies the predicate.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n). The function tests the elements in ascending order and stops at the first one that does not satisfy the
/// predicate.
/// </remarks>
[<CompiledName "ForAll">]
val forall : predicate : ('T -> bool) -> set : SortedSet<'T> -> bool

/// <summary>
/// Returns whether every element of the first sorted set is in the second one, and the second one has an element that
/// the first one does not have.
/// </summary>
/// <param name="set1">The potential subset.</param>
/// <param name="set2">The set to test against.</param>
/// <returns><see langword="true"/> when the first sorted set is a proper subset of the second one.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set1"/> or <paramref name="set2"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(m log (n + m)), where n and m are the numbers of elements of the first and the second sorted set. The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableSortedSet`1.IsProperSubsetOf(System.Collections.Generic.IEnumerable{`0})"/>
/// and compares in the comparer of the first sorted set.
/// </remarks>
[<CompiledName "IsProperSubset">]
val isProperSubset : set1 : SortedSet<'T> -> set2 : SortedSet<'T> -> bool

/// <summary>
/// Returns whether every element of the second sorted set is in the first one, and the first one has an element that
/// the second one does not have.
/// </summary>
/// <param name="set1">The potential superset.</param>
/// <param name="set2">The set to test against.</param>
/// <returns><see langword="true"/> when the first sorted set is a proper superset of the second one.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set1"/> or <paramref name="set2"/> is <see langword="null"/>.</exception>
/// <remarks>
/// <para>
/// O(m log n), where n and m are the numbers of elements of the first and the second sorted set, and O(m log m) more for a
/// helper set when the sorted sets have different comparers.
/// </para>
/// <para>
/// The function compares in the comparer of the first sorted set, with set semantics: elements of the second sorted set
/// that this comparer finds equal count once, as the documentation of
/// <see cref="M:System.Collections.Immutable.ImmutableSortedSet`1.IsProperSupersetOf(System.Collections.Generic.IEnumerable{`0})"/>
/// says, while that member counts each of them (ADR 0001, D14). It wraps the member when both sorted sets have the same
/// comparer, and otherwise answers with
/// <see cref="M:System.Collections.Immutable.ImmutableSortedSet`1.IsSupersetOf(System.Collections.Generic.IEnumerable{`0})"/>
/// and <see cref="M:System.Collections.Immutable.ImmutableSortedSet`1.IsSubsetOf(System.Collections.Generic.IEnumerable{`0})"/>.
/// </para>
/// </remarks>
[<CompiledName "IsProperSuperset">]
val isProperSuperset : set1 : SortedSet<'T> -> set2 : SortedSet<'T> -> bool

/// <summary>
/// Returns whether every element of the first sorted set is in the second one.
/// </summary>
/// <param name="set1">The potential subset.</param>
/// <param name="set2">The set to test against.</param>
/// <returns><see langword="true"/> when the first sorted set is a subset of the second one.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set1"/> or <paramref name="set2"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(m log (n + m)), where n and m are the numbers of elements of the first and the second sorted set. The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableSortedSet`1.IsSubsetOf(System.Collections.Generic.IEnumerable{`0})"/>
/// and compares in the comparer of the first sorted set.
/// </remarks>
[<CompiledName "IsSubset">]
val isSubset : set1 : SortedSet<'T> -> set2 : SortedSet<'T> -> bool

/// <summary>
/// Returns whether every element of the second sorted set is in the first one.
/// </summary>
/// <param name="set1">The potential superset.</param>
/// <param name="set2">The set to test against.</param>
/// <returns><see langword="true"/> when the first sorted set is a superset of the second one.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set1"/> or <paramref name="set2"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(m log n), where n and m are the numbers of elements of the first and the second sorted set. The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableSortedSet`1.IsSupersetOf(System.Collections.Generic.IEnumerable{`0})"/>
/// and compares in the comparer of the first sorted set.
/// </remarks>
[<CompiledName "IsSuperset">]
val isSuperset : set1 : SortedSet<'T> -> set2 : SortedSet<'T> -> bool

/// <summary>
/// Returns whether the sorted sets have an element in common.
/// </summary>
/// <param name="set1">The first sorted set.</param>
/// <param name="set2">The second sorted set.</param>
/// <returns><see langword="true"/> when an element of the second sorted set is in the first one.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set1"/> or <paramref name="set2"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(m log n), where n and m are the numbers of elements of the first and the second sorted set. The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableSortedSet`1.Overlaps(System.Collections.Generic.IEnumerable{`0})"/>
/// and compares in the comparer of the first sorted set.
/// </remarks>
[<CompiledName "Overlaps">]
val overlaps : set1 : SortedSet<'T> -> set2 : SortedSet<'T> -> bool

/// <summary>
/// Returns whether the sorted sets have the same elements.
/// </summary>
/// <param name="set1">The first sorted set.</param>
/// <param name="set2">The second sorted set.</param>
/// <returns><see langword="true"/> when every element of each sorted set is in the other one.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set1"/> or <paramref name="set2"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(m log (n + m)), where n and m are the numbers of elements of the first and the second sorted set. The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableSortedSet`1.SetEquals(System.Collections.Generic.IEnumerable{`0})"/>
/// and compares in the comparer of the first sorted set.
/// </remarks>
[<CompiledName "SetEquals">]
val setEquals : set1 : SortedSet<'T> -> set2 : SortedSet<'T> -> bool

// ---- Transformation ----

/// <summary>Returns a sorted set with a value added.</summary>
/// <param name="value">The value to add.</param>
/// <param name="set">The input sorted set.</param>
/// <returns>
/// A sorted set of the elements and the value, with the comparer of the input; the input itself when it already contains
/// an element equal to the value.
/// </returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>O(log n).</remarks>
[<CompiledName "Add">]
val add : value : 'T -> set : SortedSet<'T> -> SortedSet<'T>

/// <summary>
/// Returns a sorted set with the elements of a sequence added.
/// </summary>
/// <param name="elements">The elements to add.</param>
/// <param name="set">The input sorted set.</param>
/// <returns>
/// A sorted set of the elements and the elements of the sequence, with the comparer of the input; the input itself when the
/// sequence adds no element.
/// </returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="elements"/> is <see langword="null"/> or a default flat list, or <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>
/// <para>
/// O(m log (n + m)), where n and m are the numbers of elements of the sorted set and the sequence. The function adds the
/// elements one by one to the builder of the sorted set.
/// </para>
/// <para>
/// An element of the sequence that the comparer of the sorted set considers equal to an element of the sorted set does not
/// replace it, and of several equal elements of the sequence the first one is added (ADR 0001, D14).
/// </para>
/// <para>
/// The function does not wrap
/// <see cref="M:System.Collections.Immutable.ImmutableSortedSet`1.Union(System.Collections.Generic.IEnumerable{`0})"/>.
/// When the sequence is large against the sorted set, that member refills the tree from an unstable sort and can keep an
/// element of the sequence in place of the equal element of the sorted set. The price is time: for such a sequence the
/// BCL member is faster, and the more so the larger the sequence, because it sorts the elements and builds the tree at
/// once, while the function inserts them one by one. For a sequence that is not larger than the sorted set both do the
/// same work. To unite two sorted sets use
/// <see cref="M:FSharp.Collections.Immutable.SortedSet.Union``1(System.Collections.Immutable.ImmutableSortedSet{``0},System.Collections.Immutable.ImmutableSortedSet{``0})"/>,
/// which keeps the element of the larger sorted set when both have the same comparer.
/// </para>
/// </remarks>
[<CompiledName "AddRange">]
val addRange : elements : seq<'T> -> set : SortedSet<'T> -> SortedSet<'T>

/// <summary>
/// Returns the values that the chooser returns for the elements.
/// </summary>
/// <param name="chooser">The function that returns a value, or <c>ValueNone</c> to skip the element.</param>
/// <param name="set">The input sorted set.</param>
/// <returns>The sorted set of the distinct chosen values, which compares with F# generic comparison.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n log n). The function calls the chooser in ascending order of the elements and keeps the first of the values that
/// compare equal. The comparer of the input is not kept, even when the chosen values have the type of the elements: the
/// function cannot keep it for every type without a type test that would make its result depend on the type.
/// </remarks>
[<CompiledName "Choose">]
val choose : chooser : ('T -> 'U voption) -> set : SortedSet<'T> -> SortedSet<'U> when 'U : comparison

/// <summary>
/// Returns an empty sorted set with the comparer of the input.
/// </summary>
/// <param name="set">The input sorted set.</param>
/// <returns>The empty sorted set; the input itself when it is empty.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(1). The function wraps <see cref="M:System.Collections.Immutable.ImmutableSortedSet`1.Clear"/>.
/// </remarks>
[<CompiledName "Clear">]
val clear : set : SortedSet<'T> -> SortedSet<'T>

/// <summary>
/// Returns the elements of the first sorted set that are not in the second one.
/// </summary>
/// <param name="set1">The first sorted set.</param>
/// <param name="set2">The sorted set whose elements are removed from the first one.</param>
/// <returns>The difference, with the comparer of the first sorted set.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set1"/> or <paramref name="set2"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(m log n), where n and m are the numbers of elements of the first and the second sorted set. The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableSortedSet`1.Except(System.Collections.Generic.IEnumerable{`0})"/>
/// and compares in the comparer of the first sorted set.
/// </remarks>
[<CompiledName "Difference">]
val difference : set1 : SortedSet<'T> -> set2 : SortedSet<'T> -> SortedSet<'T>

/// <summary>
/// Returns the elements that satisfy the predicate.
/// </summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="set">The input sorted set.</param>
/// <returns>The sorted set of the elements that satisfy the predicate, with the comparer of the input.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n log n). The function tests the elements in ascending order.
/// </remarks>
[<CompiledName "Filter">]
val filter : predicate : ('T -> bool) -> set : SortedSet<'T> -> SortedSet<'T>

/// <summary>
/// Returns the elements that are in both sorted sets.
/// </summary>
/// <param name="set1">The first sorted set.</param>
/// <param name="set2">The second sorted set.</param>
/// <returns>The intersection, with the comparer of the first sorted set.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set1"/> or <paramref name="set2"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(m log n), where n and m are the numbers of elements of the first and the second sorted set. The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableSortedSet`1.Intersect(System.Collections.Generic.IEnumerable{`0})"/>,
/// which compares in the comparer of the first sorted set and keeps the elements of the second sorted set: where the
/// comparer considers two different elements equal, the intersection holds the one of the second sorted set.
/// </remarks>
[<CompiledName "Intersect">]
val intersect : set1 : SortedSet<'T> -> set2 : SortedSet<'T> -> SortedSet<'T>

/// <summary>
/// Returns the elements that are in every sorted set of a sequence.
/// </summary>
/// <param name="sets">The sorted sets.</param>
/// <returns>The intersection, with the comparer of the first sorted set.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="sets"/> is <see langword="null"/> or a default flat list, or one of its sorted sets is <see langword="null"/>.</exception>
/// <exception cref="T:System.ArgumentException">Thrown when the sequence is empty.</exception>
/// <remarks>
/// O(N log n), where N is the total number of elements of the other sorted sets and n the number of elements of the
/// first one. The function intersects the sorted sets from the first one to the last one.
/// </remarks>
[<CompiledName "IntersectMany">]
val intersectMany : sets : seq<SortedSet<'T>> -> SortedSet<'T>

/// <summary>
/// Returns the results of a function applied to each element.
/// </summary>
/// <param name="mapping">The function to transform the elements.</param>
/// <param name="set">The input sorted set.</param>
/// <returns>The sorted set of the distinct results, which compares with F# generic comparison.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n log n). The function maps the elements in ascending order and keeps the first of the results that compare equal,
/// as <see cref="M:Microsoft.FSharp.Collections.SetModule.Map``2(Microsoft.FSharp.Core.FSharpFunc{``0,``1},Microsoft.FSharp.Collections.FSharpSet{``0})"/>
/// does. The comparer of the input is not kept, even when the results have the type of the elements: the function cannot
/// keep it for every type without a type test that would make its result depend on the type.
/// </remarks>
[<CompiledName "Map">]
val map : mapping : ('T -> 'U) -> set : SortedSet<'T> -> SortedSet<'U> when 'U : comparison

/// <summary>Returns a sorted set without an element equal to a value.</summary>
/// <param name="value">The value to remove.</param>
/// <param name="set">The input sorted set.</param>
/// <returns>
/// A sorted set of the other elements, with the comparer of the input; the input itself when it has no element equal to
/// the value.
/// </returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>O(log n).</remarks>
[<CompiledName "Remove">]
val remove : value : 'T -> set : SortedSet<'T> -> SortedSet<'T>

/// <summary>
/// Returns a sorted set without the elements that are equal to an element of a sequence.
/// </summary>
/// <param name="elements">The elements to remove; an element without an equal in the sorted set is skipped.</param>
/// <param name="set">The input sorted set.</param>
/// <returns>
/// A sorted set of the other elements, with the comparer of the input; the input itself when it has no element equal to an
/// element of the sequence.
/// </returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="elements"/> is <see langword="null"/> or a default flat list, or <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(m log n), where n and m are the numbers of elements of the sorted set and the sequence. The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableSortedSet`1.Except(System.Collections.Generic.IEnumerable{`0})"/>
/// and compares in the comparer of the sorted set.
/// </remarks>
[<CompiledName "RemoveRange">]
val removeRange : elements : seq<'T> -> set : SortedSet<'T> -> SortedSet<'T>

/// <summary>
/// Returns the elements in descending order.
/// </summary>
/// <param name="set">The input sorted set.</param>
/// <returns>The sequence of the elements, from the largest one to the smallest one.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(1), and O(n) to enumerate the sequence. The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableSortedSet`1.Reverse"/>, whose sequence walks the sorted set from
/// the end without a copy.
/// </remarks>
[<CompiledName "Reverse">]
val rev : set : SortedSet<'T> -> seq<'T>

/// <summary>
/// Returns the elements that are in exactly one of the sorted sets.
/// </summary>
/// <param name="set1">The first sorted set.</param>
/// <param name="set2">The second sorted set.</param>
/// <returns>The symmetric difference, with the comparer of the first sorted set.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set1"/> or <paramref name="set2"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O((n + m) log (n + m)), where n and m are the numbers of elements of the first and the second sorted set. The function
/// wraps <see cref="M:System.Collections.Immutable.ImmutableSortedSet`1.SymmetricExcept(System.Collections.Generic.IEnumerable{`0})"/>
/// and compares in the comparer of the first sorted set.
/// </remarks>
[<CompiledName "SymmetricDifference">]
val symmetricDifference : set1 : SortedSet<'T> -> set2 : SortedSet<'T> -> SortedSet<'T>

/// <summary>
/// Returns the elements that are in either sorted set.
/// </summary>
/// <param name="set1">The first sorted set.</param>
/// <param name="set2">The second sorted set.</param>
/// <returns>The union, with the comparer of the first sorted set.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set1"/> or <paramref name="set2"/> is <see langword="null"/>.</exception>
/// <remarks>
/// <para>
/// O(m log (n + m)), where n and m are the numbers of elements of the first and the second sorted set. The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableSortedSet`1.Union(System.Collections.Generic.IEnumerable{`0})"/>
/// and compares in the comparer of the first sorted set.
/// </para>
/// <para>
/// Where the comparer considers two different elements equal, the union holds the element of the first sorted set, or the
/// element of the larger one when both have the same comparer, because the BCL member then adds the smaller sorted set to
/// the larger one (ADR 0001, D14). When the second sorted set has another comparer and is much larger than the first one,
/// the BCL member refills the tree from an unstable sort, and either element can stay. To add the elements of a sequence
/// to a sorted set and keep the elements of the sorted set in every case, use
/// <see cref="M:FSharp.Collections.Immutable.SortedSet.AddRange``1(System.Collections.Generic.IEnumerable{``0},System.Collections.Immutable.ImmutableSortedSet{``0})"/>.
/// </para>
/// </remarks>
[<CompiledName "Union">]
val union : set1 : SortedSet<'T> -> set2 : SortedSet<'T> -> SortedSet<'T>

/// <summary>
/// Returns the elements that are in any sorted set of a sequence.
/// </summary>
/// <param name="sets">The sorted sets.</param>
/// <returns>
/// The union, with the comparer of the first sorted set; the empty sorted set, which compares with F# generic comparison,
/// for an empty sequence.
/// </returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="sets"/> is <see langword="null"/> or a default flat list, or one of its sorted sets is <see langword="null"/>.</exception>
/// <remarks>
/// O(N log N), where N is the total number of elements of the sorted sets. The function unites the sorted sets from the
/// first one to the last one.
/// </remarks>
[<CompiledName "UnionMany">]
val unionMany : sets : seq<SortedSet<'T>> -> SortedSet<'T> when 'T : comparison

// ---- Folding ----

/// <summary>
/// Applies a function to a state and each element, in ascending order.
/// </summary>
/// <param name="folder">The function that computes the next state.</param>
/// <param name="state">The initial state.</param>
/// <param name="set">The input sorted set.</param>
/// <returns>The final state.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Fold">]
val fold : folder : ('State -> 'T -> 'State) -> state : 'State -> set : SortedSet<'T> -> 'State

/// <summary>
/// Applies a function to each element and a state, in descending order.
/// </summary>
/// <param name="folder">The function that computes the next state.</param>
/// <param name="set">The input sorted set.</param>
/// <param name="state">The initial state.</param>
/// <returns>The final state.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n). The function visits the elements from the largest one, as
/// <see cref="M:Microsoft.FSharp.Collections.SetModule.FoldBack``2(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpFunc{``1,``1}},Microsoft.FSharp.Collections.FSharpSet{``0},``1)"/>
/// does. It enumerates <see cref="M:System.Collections.Immutable.ImmutableSortedSet`1.Reverse"/> and copies no element.
/// </remarks>
[<CompiledName "FoldBack">]
val foldBack : folder : ('T -> 'State -> 'State) -> set : SortedSet<'T> -> state : 'State -> 'State

/// <summary>Applies an action to each element, in ascending order.</summary>
/// <param name="action">The action.</param>
/// <param name="set">The input sorted set.</param>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>O(n).</remarks>
[<CompiledName "Iterate">]
val iter : action : ('T -> unit) -> set : SortedSet<'T> -> unit

// ---- Partitioning ----

/// <summary>
/// Splits the elements by a predicate.
/// </summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="set">The input sorted set.</param>
/// <returns>
/// The sorted set of the elements that satisfy the predicate and the sorted set of the other ones, both with the comparer
/// of the input.
/// </returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n log n). The function tests the elements in ascending order.
/// </remarks>
[<CompiledName "Partition">]
val partition : predicate : ('T -> bool) -> set : SortedSet<'T> -> struct (SortedSet<'T> * SortedSet<'T>)

/// <summary>
/// Splits the elements by the choice that a function returns for them.
/// </summary>
/// <param name="partitioner">The function that returns <c>Choice1Of2</c> or <c>Choice2Of2</c> for an element.</param>
/// <param name="set">The input sorted set.</param>
/// <returns>
/// The sorted set of the values of the first choices and the sorted set of the values of the second choices, which
/// compare with F# generic comparison.
/// </returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n log n). The function calls the partitioner in descending order of the elements and keeps the first of the values
/// that compare equal, as <see cref="M:Microsoft.FSharp.Collections.SetModule.PartitionWith``3(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpChoice{``1,``2}},Microsoft.FSharp.Collections.FSharpSet{``0})"/>
/// does, so that the same one of two equal values stays.
/// </remarks>
[<CompiledName "PartitionWith">]
val partitionWith :
    partitioner : ('T -> Choice<'T1, 'T2>) -> set : SortedSet<'T> -> struct (SortedSet<'T1> * SortedSet<'T2>)
        when 'T1 : comparison and 'T2 : comparison

// ---- Conversion ----

/// <summary>Returns a new array with the elements, in ascending order.</summary>
/// <param name="set">The input sorted set.</param>
/// <returns>A copy of the elements.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>O(n).</remarks>
[<CompiledName "ToArray">]
val toArray : set : SortedSet<'T> -> 'T array

/// <summary>Returns a list of the elements, in ascending order.</summary>
/// <param name="set">The input sorted set.</param>
/// <returns>The list of the elements.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>O(n).</remarks>
[<CompiledName "ToList">]
val toList : set : SortedSet<'T> -> 'T list

/// <summary>Returns the sorted set as a sequence, in ascending order.</summary>
/// <param name="set">The input sorted set.</param>
/// <returns>The sequence of the elements.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>O(1).</remarks>
[<CompiledName "ToSeq">]
val toSeq : set : SortedSet<'T> -> seq<'T>
