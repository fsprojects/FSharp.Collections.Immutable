/// <summary>
/// Functions for <see cref="T:FSharp.Collections.Immutable.HashSet`1"/>, the immutable hash set.
/// </summary>
/// <remarks>
/// <para>
/// The module mirrors <see cref="T:Microsoft.FSharp.Collections.SetModule"/>: each function has the semantics, the
/// argument order and the exceptions of the function with the same name in that module (ADR 0001). Functions that
/// FSharp.Core lacks wrap a member of <see cref="T:System.Collections.Immutable.ImmutableHashSet`1"/> and throw what that
/// member throws. Every function throws <see cref="T:System.ArgumentNullException"/> for a <see langword="null"/> hash set.
/// </para>
/// <para>
/// The order in which a hash set enumerates its elements is unspecified. The functions that visit the elements visit them
/// in that order.
/// </para>
/// <para>
/// The creation functions create hash sets that compare with
/// <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>.
/// <see cref="M:FSharp.Collections.Immutable.HashSet.ForComparer``1(System.Collections.Generic.IEqualityComparer{``0})"/>
/// creates a <see cref="T:FSharp.Collections.Immutable.CustomComparerHashSet`1"/>, whose members create hash sets with
/// another comparer (ADR 0001, D12).
/// </para>
/// <para>
/// The transformations that keep the element type keep the comparer of their input; the set algebra keeps the comparer of
/// the first set. The transformations that change the element type compare the new elements with the default comparer of
/// their type.
/// </para>
/// <para>
/// Functions that return an optional value return a <see cref="T:Microsoft.FSharp.Core.FSharpValueOption`1"/>, and functions
/// that return tuples return struct tuples. The namespaces <see cref="N:FSharp.Collections.Immutable.Option"/> and
/// <see cref="N:FSharp.Collections.Immutable.Tuple"/> contain a module with the same name whose functions use options
/// and reference tuples instead.
/// </para>
/// </remarks>
[<RequireQualifiedAccess>]
module FSharp.Collections.Immutable.HashSet

open System
open System.Collections.Generic
open System.Collections.Immutable
// Opened after System.Collections.Generic, so that HashSet names the abbreviation of this library, not the mutable set
open FSharp.Collections.Immutable

// ---- Creation ----

/// <summary>
/// The empty hash set, which compares with <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>.
/// </summary>
/// <remarks>
/// O(1). The value is <see cref="F:System.Collections.Immutable.ImmutableHashSet`1.Empty"/>.
/// </remarks>
[<GeneralizableValue>]
[<CompiledName "Empty">]
val empty<'T> : HashSet<'T>

/// <summary>
/// Creates the struct that creates hash sets that compare their elements with an equality comparer.
/// </summary>
/// <param name="comparer">The equality comparer of the elements.</param>
/// <returns>
/// The <see cref="T:FSharp.Collections.Immutable.CustomComparerHashSet`1"/> whose members are the creation functions of
/// this module with the comparer: they create hash sets and builders that compare with it.
/// </returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="comparer"/> is <see langword="null"/>.</exception>
/// <remarks>
/// <para>
/// O(1). Creating the struct allocates nothing.
/// <see cref="M:FSharp.Collections.Immutable.CustomComparerHashSet`1.OfSeq(System.Collections.Generic.IEnumerable{`0})"/>
/// also gives an existing hash set the comparer (ADR 0001, D12).
/// </para>
/// <para>
/// A <see langword="null"/> comparer throws, where the BCL members <c>Create</c>, <c>CreateRange</c>,
/// <c>CreateBuilder</c> and <c>WithComparer</c> take it as the default comparer. For that meaning use
/// <see cref="M:FSharp.Collections.Immutable.HashSet.OfSeq``1(System.Collections.Generic.IEnumerable{``0})"/>, which gives
/// a hash set the default comparer, or the default value of the struct, which compares with
/// <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>.
/// </para>
/// </remarks>
/// <example id="forComparer-1">
/// <code lang="fsharp">
/// let names = (HashSet.forComparer StringComparer.OrdinalIgnoreCase).ofSeq [ "Ann"; "ANN"; "Bob" ]
/// </code>
/// Evaluates to a hash set of "Ann" and "Bob" that compares without case.
/// </example>
[<CompiledName "ForComparer">]
val forComparer : comparer : IEqualityComparer<'T> -> CustomComparerHashSet<'T>

/// <summary>
/// Creates a hash set from the elements of an array.
/// </summary>
/// <param name="array">The input array.</param>
/// <returns>The hash set of the distinct elements of the array.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="array"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n log n). Later changes to the array do not change the hash set.
/// </remarks>
[<CompiledName "OfArray">]
val ofArray : array : 'T array -> HashSet<'T>

/// <summary>Creates a hash set from the elements of a list.</summary>
/// <param name="elements">The input list.</param>
/// <returns>The hash set of the distinct elements of the list.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="elements"/> is <see langword="null"/>.</exception>
/// <remarks>O(n log n).</remarks>
[<CompiledName "OfList">]
val ofList : elements : 'T list -> HashSet<'T>

/// <summary>
/// Creates a hash set from the elements of a read-only span.
/// </summary>
/// <param name="span">The input span.</param>
/// <returns>
/// The hash set of the distinct elements of the span; an empty span gives the empty hash set.
/// </returns>
/// <remarks>
/// O(n log n). Later changes to the memory under the span do not change the hash set. The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableHashSet.Create``1(System.ReadOnlySpan{``0})"/>.
/// </remarks>
[<CompiledName "OfReadOnlySpan">]
val ofReadOnlySpan : span : ReadOnlySpan<'T> -> HashSet<'T>

/// <summary>
/// Creates a hash set from the elements of a sequence.
/// </summary>
/// <param name="elements">The input sequence.</param>
/// <returns>The hash set of the distinct elements of the sequence.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="elements"/> is <see langword="null"/> or a default flat list.</exception>
/// <remarks>
/// O(n log n), or O(1) for a hash set that compares with the default comparer: the function returns such a hash set
/// without a copy, and gives a hash set with another comparer the default one. The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableHashSet.CreateRange``1(System.Collections.Generic.IEqualityComparer{``0},System.Collections.Generic.IEnumerable{``0})"/>.
/// </remarks>
[<CompiledName "OfSeq">]
val ofSeq : elements : seq<'T> -> HashSet<'T>

/// <summary>
/// Creates a hash set from the elements of a span.
/// </summary>
/// <param name="span">The input span.</param>
/// <returns>
/// The hash set of the distinct elements of the span; an empty span gives the empty hash set.
/// </returns>
/// <remarks>
/// O(n log n). Later changes to the memory under the span do not change the hash set. The function converts the span to a
/// read-only span and wraps
/// <see cref="M:System.Collections.Immutable.ImmutableHashSet.Create``1(System.ReadOnlySpan{``0})"/>.
/// </remarks>
[<CompiledName "OfSpan">]
val ofSpan : span : Span<'T> -> HashSet<'T>

/// <summary>Creates a hash set of one element.</summary>
/// <param name="value">The element.</param>
/// <returns>The hash set of one element.</returns>
/// <remarks>O(1).</remarks>
[<CompiledName "Singleton">]
val singleton : value : 'T -> HashSet<'T>

// ---- Builder ----

/// <summary>
/// Creates a hash set with a builder that an action fills.
/// </summary>
/// <param name="action">
/// The action that adds the elements to the builder, which compares with
/// <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>.
/// </param>
/// <returns>The hash set of the elements that the action added.</returns>
/// <remarks>
/// O(1) apart from the cost of the action. The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableHashSet.CreateBuilder``1(System.Collections.Generic.IEqualityComparer{``0})"/>
/// and <see cref="M:System.Collections.Immutable.ImmutableHashSet`1.Builder.ToImmutable"/>.
/// </remarks>
[<CompiledName "Build">]
val build : action : (HashSet<'T>.Builder -> unit) -> HashSet<'T>

/// <summary>
/// Creates an empty builder that compares with <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>.
/// </summary>
/// <returns>The builder.</returns>
/// <remarks>
/// O(1). The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableHashSet.CreateBuilder``1(System.Collections.Generic.IEqualityComparer{``0})"/>.
/// </remarks>
[<CompiledName "CreateBuilder">]
val builder : unit -> HashSet<'T>.Builder

/// <summary>
/// Returns the contents of a builder as a hash set.
/// </summary>
/// <param name="builder">The builder, which stays usable.</param>
/// <returns>The hash set of the elements of the builder, with the comparer of the builder.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="builder"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(1) apart from freezing the nodes that changed since the builder last returned a hash set: the hash set shares the
/// nodes of the builder instead of copying the elements. The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableHashSet`1.Builder.ToImmutable"/>.
/// </remarks>
[<CompiledName "OfBuilder">]
val ofBuilder : builder : HashSet<'T>.Builder -> HashSet<'T>

/// <summary>
/// Creates a builder that contains the elements of a hash set.
/// </summary>
/// <param name="set">The input hash set, which stays unchanged.</param>
/// <returns>The builder, with the comparer of the hash set.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(1). The function wraps <see cref="M:System.Collections.Immutable.ImmutableHashSet`1.ToBuilder"/>. The builder shares
/// the nodes of the hash set until it changes them.
/// </remarks>
[<CompiledName "ToBuilder">]
val toBuilder : set : HashSet<'T> -> HashSet<'T>.Builder

/// <summary>
/// Creates a hash set from a builder over the elements of a hash set, after an action changes the builder.
/// </summary>
/// <param name="action">The action that changes the builder, which has the comparer of the hash set.</param>
/// <param name="set">The input hash set, which stays unchanged.</param>
/// <returns>The hash set of the elements of the changed builder, with the comparer of the input.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(1) apart from the cost of the action. The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableHashSet`1.ToBuilder"/> and
/// <see cref="M:System.Collections.Immutable.ImmutableHashSet`1.Builder.ToImmutable"/>.
/// </remarks>
[<CompiledName "Update">]
val update : action : (HashSet<'T>.Builder -> unit) -> set : HashSet<'T> -> HashSet<'T>

// ---- Properties ----

/// <summary>
/// Returns the equality comparer of the elements of the hash set.
/// </summary>
/// <param name="set">The input hash set.</param>
/// <returns>The equality comparer.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(1). The function wraps <see cref="P:System.Collections.Immutable.ImmutableHashSet`1.KeyComparer"/>.
/// </remarks>
[<CompiledName "Comparer">]
val comparer : set : HashSet<'T> -> IEqualityComparer<'T>

/// <summary>Returns the number of elements of the hash set.</summary>
/// <param name="set">The input hash set.</param>
/// <returns>The number of elements.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>O(1).</remarks>
[<CompiledName "Count">]
val count : set : HashSet<'T> -> int

/// <summary>Returns whether the hash set has no elements.</summary>
/// <param name="set">The input hash set.</param>
/// <returns><see langword="true"/> when the hash set is empty.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>O(1).</remarks>
[<CompiledName "IsEmpty">]
val isEmpty : set : HashSet<'T> -> bool

// ---- Access ----

/// <summary>
/// Returns the element of the hash set that equals a value, or <c>ValueNone</c> when there is none.
/// </summary>
/// <param name="value">The value to search for.</param>
/// <param name="set">The input hash set.</param>
/// <returns>The element that the comparer of the hash set considers equal to the value, or <c>ValueNone</c>.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(log n). The function wraps <see cref="M:System.Collections.Immutable.ImmutableHashSet`1.TryGetValue(`0,`0@)"/>. The
/// element can differ from an equal value, for example in case under a case-insensitive comparer; the function returns
/// the instance that the hash set holds.
/// </remarks>
[<CompiledName "TryGetValue">]
val tryGetValue : value : 'T -> set : HashSet<'T> -> 'T voption

// ---- Search ----

/// <summary>
/// Returns whether the hash set contains an element equal to a value, in the comparer of the hash set.
/// </summary>
/// <param name="element">The value to find.</param>
/// <param name="set">The input hash set.</param>
/// <returns><see langword="true"/> when an element equals the value.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(log n).
/// </remarks>
[<CompiledName "Contains">]
val contains : element : 'T -> set : HashSet<'T> -> bool

/// <summary>
/// Returns whether any element satisfies the predicate.
/// </summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="set">The input hash set.</param>
/// <returns><see langword="true"/> when an element satisfies the predicate.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n). The function stops at the first element that satisfies the predicate.
/// </remarks>
[<CompiledName "Exists">]
val exists : predicate : ('T -> bool) -> set : HashSet<'T> -> bool

/// <summary>
/// Returns whether all elements satisfy the predicate.
/// </summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="set">The input hash set.</param>
/// <returns><see langword="true"/> when every element satisfies the predicate.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n). The function stops at the first element that does not satisfy the predicate.
/// </remarks>
[<CompiledName "ForAll">]
val forall : predicate : ('T -> bool) -> set : HashSet<'T> -> bool

/// <summary>
/// Returns whether every element of the first hash set is in the second one, and the second one has an element that the
/// first one does not have.
/// </summary>
/// <param name="set1">The potential subset.</param>
/// <param name="set2">The set to test against.</param>
/// <returns><see langword="true"/> when the first hash set is a proper subset of the second one.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set1"/> or <paramref name="set2"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(m log n), where n and m are the numbers of elements of the first and the second hash set. The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableHashSet`1.IsProperSubsetOf(System.Collections.Generic.IEnumerable{`0})"/>
/// and compares in the comparer of the first hash set.
/// </remarks>
[<CompiledName "IsProperSubset">]
val isProperSubset : set1 : HashSet<'T> -> set2 : HashSet<'T> -> bool

/// <summary>
/// Returns whether every element of the second hash set is in the first one, and the first one has an element that the
/// second one does not have.
/// </summary>
/// <param name="set1">The potential superset.</param>
/// <param name="set2">The set to test against.</param>
/// <returns><see langword="true"/> when the first hash set is a proper superset of the second one.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set1"/> or <paramref name="set2"/> is <see langword="null"/>.</exception>
/// <remarks>
/// <para>
/// O(m log n), where n and m are the numbers of elements of the first and the second hash set, and O(m) more for a helper
/// set when the hash sets compare with different comparers.
/// </para>
/// <para>
/// The function compares in the comparer of the first hash set, with set semantics: elements of the second hash set that
/// this comparer finds equal count once, as the documentation of
/// <see cref="M:System.Collections.Immutable.ImmutableHashSet`1.IsProperSupersetOf(System.Collections.Generic.IEnumerable{`0})"/>
/// says, while that member counts each of them (ADR 0001, D14). It wraps the member when both hash sets have the same
/// comparer, and otherwise answers with
/// <see cref="M:System.Collections.Immutable.ImmutableHashSet`1.IsSupersetOf(System.Collections.Generic.IEnumerable{`0})"/>
/// and <see cref="M:System.Collections.Immutable.ImmutableHashSet`1.IsSubsetOf(System.Collections.Generic.IEnumerable{`0})"/>.
/// </para>
/// </remarks>
[<CompiledName "IsProperSuperset">]
val isProperSuperset : set1 : HashSet<'T> -> set2 : HashSet<'T> -> bool

/// <summary>
/// Returns whether every element of the first hash set is in the second one.
/// </summary>
/// <param name="set1">The potential subset.</param>
/// <param name="set2">The set to test against.</param>
/// <returns><see langword="true"/> when the first hash set is a subset of the second one.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set1"/> or <paramref name="set2"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(m log n), where n and m are the numbers of elements of the first and the second hash set. The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableHashSet`1.IsSubsetOf(System.Collections.Generic.IEnumerable{`0})"/>
/// and compares in the comparer of the first hash set.
/// </remarks>
[<CompiledName "IsSubset">]
val isSubset : set1 : HashSet<'T> -> set2 : HashSet<'T> -> bool

/// <summary>
/// Returns whether every element of the second hash set is in the first one.
/// </summary>
/// <param name="set1">The potential superset.</param>
/// <param name="set2">The set to test against.</param>
/// <returns><see langword="true"/> when the first hash set is a superset of the second one.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set1"/> or <paramref name="set2"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(m log n), where n and m are the numbers of elements of the first and the second hash set. The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableHashSet`1.IsSupersetOf(System.Collections.Generic.IEnumerable{`0})"/>
/// and compares in the comparer of the first hash set.
/// </remarks>
[<CompiledName "IsSuperset">]
val isSuperset : set1 : HashSet<'T> -> set2 : HashSet<'T> -> bool

/// <summary>
/// Returns whether the hash sets have an element in common.
/// </summary>
/// <param name="set1">The first hash set.</param>
/// <param name="set2">The second hash set.</param>
/// <returns><see langword="true"/> when an element of the second hash set is in the first one.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set1"/> or <paramref name="set2"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(m log n), where n and m are the numbers of elements of the first and the second hash set. The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableHashSet`1.Overlaps(System.Collections.Generic.IEnumerable{`0})"/>
/// and compares in the comparer of the first hash set.
/// </remarks>
[<CompiledName "Overlaps">]
val overlaps : set1 : HashSet<'T> -> set2 : HashSet<'T> -> bool

/// <summary>
/// Returns whether the hash sets have the same elements.
/// </summary>
/// <param name="set1">The first hash set.</param>
/// <param name="set2">The second hash set.</param>
/// <returns><see langword="true"/> when every element of each hash set is in the other one.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set1"/> or <paramref name="set2"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O((n + m) log (n + m)), where n and m are the numbers of elements of the first and the second hash set. The function
/// wraps <see cref="M:System.Collections.Immutable.ImmutableHashSet`1.SetEquals(System.Collections.Generic.IEnumerable{`0})"/>
/// and compares in the comparer of the first hash set.
/// </remarks>
[<CompiledName "SetEquals">]
val setEquals : set1 : HashSet<'T> -> set2 : HashSet<'T> -> bool

// ---- Transformation ----

/// <summary>Returns a hash set with a value added.</summary>
/// <param name="value">The value to add.</param>
/// <param name="set">The input hash set.</param>
/// <returns>
/// A hash set of the elements and the value, with the comparer of the input; the input itself when it already contains
/// an element equal to the value.
/// </returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>O(log n).</remarks>
[<CompiledName "Add">]
val add : value : 'T -> set : HashSet<'T> -> HashSet<'T>

/// <summary>
/// Returns a hash set with the elements of a sequence added.
/// </summary>
/// <param name="elements">The elements to add.</param>
/// <param name="set">The input hash set.</param>
/// <returns>
/// A hash set of the elements and the elements of the sequence, with the comparer of the input; the input itself when the
/// sequence adds no element.
/// </returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="elements"/> is <see langword="null"/> or a default flat list, or <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>
/// <para>
/// O(m log (n + m)), where n and m are the numbers of elements of the hash set and the sequence. The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableHashSet`1.Union(System.Collections.Generic.IEnumerable{`0})"/>.
/// </para>
/// <para>
/// An element of the sequence that the comparer of the hash set considers equal to an element of the hash set does not
/// replace it, and of several equal elements of the sequence the first one is added (ADR 0001, D14).
/// </para>
/// </remarks>
[<CompiledName "AddRange">]
val addRange : elements : seq<'T> -> set : HashSet<'T> -> HashSet<'T>

/// <summary>
/// Returns the values that the chooser returns for the elements.
/// </summary>
/// <param name="chooser">The function that returns a value, or <c>ValueNone</c> to skip the element.</param>
/// <param name="set">The input hash set.</param>
/// <returns>
/// The hash set of the distinct chosen values, which compares with
/// <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>.
/// </returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n log n). The comparer of the input is not kept, even when the chosen values have the type of the elements: the
/// function cannot keep it for every type without a type test that would make its result depend on the type.
/// </remarks>
[<CompiledName "Choose">]
val choose : chooser : ('T -> 'U voption) -> set : HashSet<'T> -> HashSet<'U>

/// <summary>
/// Returns an empty hash set with the comparer of the input.
/// </summary>
/// <param name="set">The input hash set.</param>
/// <returns>The empty hash set; the input itself when it is empty.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(1). The function wraps <see cref="M:System.Collections.Immutable.ImmutableHashSet`1.Clear"/>.
/// </remarks>
[<CompiledName "Clear">]
val clear : set : HashSet<'T> -> HashSet<'T>

/// <summary>
/// Returns the elements of the first hash set that are not in the second one.
/// </summary>
/// <param name="set1">The first hash set.</param>
/// <param name="set2">The hash set whose elements are removed from the first one.</param>
/// <returns>The difference, with the comparer of the first hash set.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set1"/> or <paramref name="set2"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(m log n), where n and m are the numbers of elements of the first and the second hash set. The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableHashSet`1.Except(System.Collections.Generic.IEnumerable{`0})"/>
/// and compares in the comparer of the first hash set.
/// </remarks>
[<CompiledName "Difference">]
val difference : set1 : HashSet<'T> -> set2 : HashSet<'T> -> HashSet<'T>

/// <summary>Returns the elements that satisfy the predicate.</summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="set">The input hash set.</param>
/// <returns>The hash set of the elements that satisfy the predicate, with the comparer of the input.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>O(n log n).</remarks>
[<CompiledName "Filter">]
val filter : predicate : ('T -> bool) -> set : HashSet<'T> -> HashSet<'T>

/// <summary>
/// Returns the elements that are in both hash sets.
/// </summary>
/// <param name="set1">The first hash set.</param>
/// <param name="set2">The second hash set.</param>
/// <returns>The intersection, with the comparer of the first hash set.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set1"/> or <paramref name="set2"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(m log n), where n and m are the numbers of elements of the first and the second hash set. The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableHashSet`1.Intersect(System.Collections.Generic.IEnumerable{`0})"/>,
/// which compares in the comparer of the first hash set and keeps the elements of the second hash set: where the
/// comparer considers two different elements equal, the intersection holds the one of the second hash set.
/// </remarks>
[<CompiledName "Intersect">]
val intersect : set1 : HashSet<'T> -> set2 : HashSet<'T> -> HashSet<'T>

/// <summary>
/// Returns the elements that are in every hash set of a sequence.
/// </summary>
/// <param name="sets">The hash sets.</param>
/// <returns>The intersection, with the comparer of the first hash set.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="sets"/> is <see langword="null"/> or a default flat list, or one of its hash sets is <see langword="null"/>.</exception>
/// <exception cref="T:System.ArgumentException">Thrown when the sequence is empty.</exception>
/// <remarks>
/// O(N log n), where N is the total number of elements of the other hash sets and n the number of elements of the first
/// one. The function intersects the hash sets from the first one to the last one.
/// </remarks>
[<CompiledName "IntersectMany">]
val intersectMany : sets : seq<HashSet<'T>> -> HashSet<'T>

/// <summary>
/// Returns the results of a function applied to each element.
/// </summary>
/// <param name="mapping">The function to transform the elements.</param>
/// <param name="set">The input hash set.</param>
/// <returns>
/// The hash set of the distinct results, which compares with
/// <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>.
/// </returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n log n). The comparer of the input is not kept, even when the results have the type of the elements: the function
/// cannot keep it for every type without a type test that would make its result depend on the type.
/// </remarks>
[<CompiledName "Map">]
val map : mapping : ('T -> 'U) -> set : HashSet<'T> -> HashSet<'U>

/// <summary>Returns a hash set without an element equal to a value.</summary>
/// <param name="value">The value to remove.</param>
/// <param name="set">The input hash set.</param>
/// <returns>
/// A hash set of the other elements, with the comparer of the input; the input itself when it has no element equal to
/// the value.
/// </returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>O(log n).</remarks>
[<CompiledName "Remove">]
val remove : value : 'T -> set : HashSet<'T> -> HashSet<'T>

/// <summary>
/// Returns a hash set without the elements that are equal to an element of a sequence.
/// </summary>
/// <param name="elements">The elements to remove; an element without an equal in the hash set is skipped.</param>
/// <param name="set">The input hash set.</param>
/// <returns>
/// A hash set of the other elements, with the comparer of the input; the input itself when it has no element equal to an
/// element of the sequence.
/// </returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="elements"/> is <see langword="null"/> or a default flat list, or <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(m log n), where n and m are the numbers of elements of the hash set and the sequence. The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableHashSet`1.Except(System.Collections.Generic.IEnumerable{`0})"/>
/// and compares in the comparer of the hash set.
/// </remarks>
[<CompiledName "RemoveRange">]
val removeRange : elements : seq<'T> -> set : HashSet<'T> -> HashSet<'T>

/// <summary>
/// Returns the elements that are in exactly one of the hash sets.
/// </summary>
/// <param name="set1">The first hash set.</param>
/// <param name="set2">The second hash set.</param>
/// <returns>The symmetric difference, with the comparer of the first hash set.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set1"/> or <paramref name="set2"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O((n + m) log (n + m)), where n and m are the numbers of elements of the first and the second hash set. The function
/// wraps <see cref="M:System.Collections.Immutable.ImmutableHashSet`1.SymmetricExcept(System.Collections.Generic.IEnumerable{`0})"/>
/// and compares in the comparer of the first hash set.
/// </remarks>
[<CompiledName "SymmetricDifference">]
val symmetricDifference : set1 : HashSet<'T> -> set2 : HashSet<'T> -> HashSet<'T>

/// <summary>
/// Returns the elements that are in either hash set.
/// </summary>
/// <param name="set1">The first hash set.</param>
/// <param name="set2">The second hash set.</param>
/// <returns>The union, with the comparer of the first hash set.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set1"/> or <paramref name="set2"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(m log (n + m)), where n and m are the numbers of elements of the first and the second hash set. The function wraps
/// <see cref="M:System.Collections.Immutable.ImmutableHashSet`1.Union(System.Collections.Generic.IEnumerable{`0})"/>
/// and compares in the comparer of the first hash set.
/// </remarks>
[<CompiledName "Union">]
val union : set1 : HashSet<'T> -> set2 : HashSet<'T> -> HashSet<'T>

/// <summary>
/// Returns the elements that are in any hash set of a sequence.
/// </summary>
/// <param name="sets">The hash sets.</param>
/// <returns>
/// The union, with the comparer of the first hash set; the empty hash set with the default comparer for an empty
/// sequence.
/// </returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="sets"/> is <see langword="null"/> or a default flat list, or one of its hash sets is <see langword="null"/>.</exception>
/// <remarks>
/// O(N log N), where N is the total number of elements of the hash sets. The function unites the hash sets from the first
/// one to the last one.
/// </remarks>
[<CompiledName "UnionMany">]
val unionMany : sets : seq<HashSet<'T>> -> HashSet<'T>

// ---- Folding ----

/// <summary>
/// Applies a function to a state and each element, in the order of enumeration.
/// </summary>
/// <param name="folder">The function that computes the next state.</param>
/// <param name="state">The initial state.</param>
/// <param name="set">The input hash set.</param>
/// <returns>The final state.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Fold">]
val fold : folder : ('State -> 'T -> 'State) -> state : 'State -> set : HashSet<'T> -> 'State

/// <summary>
/// Applies a function to each element and a state, in the reverse order of enumeration.
/// </summary>
/// <param name="folder">The function that computes the next state.</param>
/// <param name="set">The input hash set.</param>
/// <param name="state">The initial state.</param>
/// <returns>The final state.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n). The function copies the elements into an array first.
/// </remarks>
[<CompiledName "FoldBack">]
val foldBack : folder : ('T -> 'State -> 'State) -> set : HashSet<'T> -> state : 'State -> 'State

/// <summary>
/// Applies an action to each element, in the order of enumeration.
/// </summary>
/// <param name="action">The action.</param>
/// <param name="set">The input hash set.</param>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Iterate">]
val iter : action : ('T -> unit) -> set : HashSet<'T> -> unit

// ---- Partitioning ----

/// <summary>Splits the elements by a predicate.</summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="set">The input hash set.</param>
/// <returns>
/// The hash set of the elements that satisfy the predicate and the hash set of the other ones, both with the comparer of
/// the input.
/// </returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>O(n log n).</remarks>
[<CompiledName "Partition">]
val partition : predicate : ('T -> bool) -> set : HashSet<'T> -> struct (HashSet<'T> * HashSet<'T>)

/// <summary>
/// Splits the elements by the choice that a function returns for them.
/// </summary>
/// <param name="partitioner">The function that returns <c>Choice1Of2</c> or <c>Choice2Of2</c> for an element.</param>
/// <param name="set">The input hash set.</param>
/// <returns>
/// The hash set of the values of the first choices and the hash set of the values of the second choices, which compare
/// with <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>.
/// </returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n log n).
/// </remarks>
[<CompiledName "PartitionWith">]
val partitionWith : partitioner : ('T -> Choice<'T1, 'T2>) -> set : HashSet<'T> -> struct (HashSet<'T1> * HashSet<'T2>)

// ---- Conversion ----

/// <summary>
/// Returns a new array with the elements, in the order of enumeration.
/// </summary>
/// <param name="set">The input hash set.</param>
/// <returns>A copy of the elements.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "ToArray">]
val toArray : set : HashSet<'T> -> 'T array

/// <summary>Returns a list of the elements, in the order of enumeration.</summary>
/// <param name="set">The input hash set.</param>
/// <returns>The list of the elements.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>O(n).</remarks>
[<CompiledName "ToList">]
val toList : set : HashSet<'T> -> 'T list

/// <summary>Returns the hash set as a sequence.</summary>
/// <param name="set">The input hash set.</param>
/// <returns>The sequence of the elements.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>O(1).</remarks>
[<CompiledName "ToSeq">]
val toSeq : set : HashSet<'T> -> seq<'T>
