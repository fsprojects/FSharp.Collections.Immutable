/// <summary>
/// Functions for <see cref="T:System.Collections.Immutable.IImmutableSet`1"/>, the interface of the immutable sets.
/// </summary>
/// <remarks>
/// <para>
/// The module mirrors <see cref="T:Microsoft.FSharp.Collections.SetModule"/> as <see cref="T:FSharp.Collections.Immutable.HashSet"/>
/// does: each function has the semantics, the argument order and the exceptions of the function with the same name in
/// that module (ADR 0001). Functions that FSharp.Core lacks wrap a member of the interface. Every function throws
/// <see cref="T:System.ArgumentNullException"/> for a <see langword="null"/> set.
/// </para>
/// <para>
/// The module has no type abbreviation, because an abbreviation named after it would shadow
/// <see cref="T:System.Collections.Generic.ISet`1"/>.
/// </para>
/// <para>
/// The transformations that keep the element type return the implementation of their input with its comparer: they call
/// the members of the interface, and the ones that build a new set start from
/// <see cref="M:System.Collections.Immutable.IImmutableSet`1.Clear"/>. The set algebra keeps the implementation and the
/// comparer of the first set. The interface cannot tell its comparer, so the transformations that change the element
/// type return a <see cref="T:FSharp.Collections.Immutable.HashSet`1"/> that compares with
/// <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>.
/// </para>
/// <para>
/// The interface can be implemented by a hash set, a sorted set or another type, so the complexity of most functions
/// depends on the implementation. Their remarks give it for <see cref="T:System.Collections.Immutable.ImmutableHashSet`1"/>,
/// and also for <see cref="T:System.Collections.Immutable.ImmutableSortedSet`1"/> where it is the same. The functions that
/// visit the elements visit each of them once, in the order of enumeration of the implementation.
/// </para>
/// </remarks>
[<RequireQualifiedAccess>]
module FSharp.Collections.Immutable.ISet

open System.Collections.Immutable

// ---- Properties ----

/// <summary>
/// Returns the number of elements of the set.
/// </summary>
/// <param name="set">The input set.</param>
/// <returns>The number of elements.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>
/// The complexity depends on the implementation: O(1) for
/// <see cref="T:System.Collections.Immutable.ImmutableHashSet`1"/> and <see cref="T:System.Collections.Immutable.ImmutableSortedSet`1"/>.
/// </remarks>
[<CompiledName "Count">]
val count : set : IImmutableSet<'T> -> int

/// <summary>
/// Returns whether the set has no elements.
/// </summary>
/// <param name="set">The input set.</param>
/// <returns><see langword="true"/> when the set is empty.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>
/// The complexity depends on the implementation: O(1) for
/// <see cref="T:System.Collections.Immutable.ImmutableHashSet`1"/> and <see cref="T:System.Collections.Immutable.ImmutableSortedSet`1"/>.
/// </remarks>
[<CompiledName "IsEmpty">]
val isEmpty : set : IImmutableSet<'T> -> bool

// ---- Access ----

/// <summary>
/// Returns the element of the set that equals a value, or <c>ValueNone</c> when there is none.
/// </summary>
/// <param name="value">The value to search for.</param>
/// <param name="set">The input set.</param>
/// <returns>The element that the comparer of the set considers equal to the value, or <c>ValueNone</c>.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>
/// The complexity depends on the implementation: O(log n) for
/// <see cref="T:System.Collections.Immutable.ImmutableHashSet`1"/> and <see cref="T:System.Collections.Immutable.ImmutableSortedSet`1"/>.
/// The function wraps <see cref="M:System.Collections.Immutable.IImmutableSet`1.TryGetValue(`0,`0@)"/>. The element can
/// differ from an equal value, for example in case under a case-insensitive comparer; the function returns the instance
/// that the set holds.
/// </remarks>
[<CompiledName "TryGetValue">]
val tryGetValue : value : 'T -> set : IImmutableSet<'T> -> 'T voption

// ---- Search ----

/// <summary>
/// Returns whether the set contains an element equal to a value, in the comparer of the set.
/// </summary>
/// <param name="element">The value to find.</param>
/// <param name="set">The input set.</param>
/// <returns><see langword="true"/> when an element equals the value.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>
/// The complexity depends on the implementation: O(log n) for
/// <see cref="T:System.Collections.Immutable.ImmutableHashSet`1"/> and <see cref="T:System.Collections.Immutable.ImmutableSortedSet`1"/>.
/// </remarks>
[<CompiledName "Contains">]
val contains : element : 'T -> set : IImmutableSet<'T> -> bool

/// <summary>
/// Returns whether any element satisfies the predicate.
/// </summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="set">The input set.</param>
/// <returns><see langword="true"/> when an element satisfies the predicate.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n). The function stops at the first element that satisfies the predicate.
/// </remarks>
[<CompiledName "Exists">]
val exists : predicate : ('T -> bool) -> set : IImmutableSet<'T> -> bool

/// <summary>
/// Returns whether all elements satisfy the predicate.
/// </summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="set">The input set.</param>
/// <returns><see langword="true"/> when every element satisfies the predicate.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n). The function stops at the first element that does not satisfy the predicate.
/// </remarks>
[<CompiledName "ForAll">]
val forall : predicate : ('T -> bool) -> set : IImmutableSet<'T> -> bool

/// <summary>
/// Returns whether every element of the first set is in the second one, and the second one has an element that the first
/// one does not have.
/// </summary>
/// <param name="set1">The potential subset.</param>
/// <param name="set2">The set to test against.</param>
/// <returns><see langword="true"/> when the first set is a proper subset of the second one.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set1"/> or <paramref name="set2"/> is <see langword="null"/>.</exception>
/// <remarks>
/// The complexity depends on the implementation of the first set: O(m log n) for an
/// <see cref="T:System.Collections.Immutable.ImmutableHashSet`1"/>, where n and m are the numbers of elements of the first
/// and the second set. The function wraps
/// <see cref="M:System.Collections.Immutable.IImmutableSet`1.IsProperSubsetOf(System.Collections.Generic.IEnumerable{`0})"/>
/// and compares in the comparer of the first set.
/// </remarks>
[<CompiledName "IsProperSubset">]
val isProperSubset : set1 : IImmutableSet<'T> -> set2 : IImmutableSet<'T> -> bool

/// <summary>
/// Returns whether every element of the second set is in the first one, and the first one has an element that the second
/// one does not have.
/// </summary>
/// <param name="set1">The potential superset.</param>
/// <param name="set2">The set to test against.</param>
/// <returns><see langword="true"/> when the first set is a proper superset of the second one.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set1"/> or <paramref name="set2"/> is <see langword="null"/>.</exception>
/// <remarks>
/// <para>
/// The complexity depends on the implementation of the first set: O(m log n) for an
/// <see cref="T:System.Collections.Immutable.ImmutableHashSet`1"/>, where n and m are the numbers of elements of the first
/// and the second set, with a helper set of the second set.
/// </para>
/// <para>
/// The function compares in the comparer of the first set, with set semantics: elements of the second set that this
/// comparer finds equal count once, as the documentation of
/// <see cref="M:System.Collections.Immutable.IImmutableSet`1.IsProperSupersetOf(System.Collections.Generic.IEnumerable{`0})"/>
/// says, while <see cref="T:System.Collections.Immutable.ImmutableHashSet`1"/> and
/// <see cref="T:System.Collections.Immutable.ImmutableSortedSet`1"/> count each of them (ADR 0001, D14). It answers with
/// <see cref="M:System.Collections.Immutable.IImmutableSet`1.IsSupersetOf(System.Collections.Generic.IEnumerable{`0})"/>
/// and <see cref="M:System.Collections.Immutable.IImmutableSet`1.IsSubsetOf(System.Collections.Generic.IEnumerable{`0})"/>.
/// </para>
/// </remarks>
[<CompiledName "IsProperSuperset">]
val isProperSuperset : set1 : IImmutableSet<'T> -> set2 : IImmutableSet<'T> -> bool

/// <summary>
/// Returns whether every element of the first set is in the second one.
/// </summary>
/// <param name="set1">The potential subset.</param>
/// <param name="set2">The set to test against.</param>
/// <returns><see langword="true"/> when the first set is a subset of the second one.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set1"/> or <paramref name="set2"/> is <see langword="null"/>.</exception>
/// <remarks>
/// The complexity depends on the implementation of the first set: O(m log n) for an
/// <see cref="T:System.Collections.Immutable.ImmutableHashSet`1"/>, where n and m are the numbers of elements of the first
/// and the second set. The function wraps
/// <see cref="M:System.Collections.Immutable.IImmutableSet`1.IsSubsetOf(System.Collections.Generic.IEnumerable{`0})"/>
/// and compares in the comparer of the first set.
/// </remarks>
[<CompiledName "IsSubset">]
val isSubset : set1 : IImmutableSet<'T> -> set2 : IImmutableSet<'T> -> bool

/// <summary>
/// Returns whether every element of the second set is in the first one.
/// </summary>
/// <param name="set1">The potential superset.</param>
/// <param name="set2">The set to test against.</param>
/// <returns><see langword="true"/> when the first set is a superset of the second one.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set1"/> or <paramref name="set2"/> is <see langword="null"/>.</exception>
/// <remarks>
/// The complexity depends on the implementation of the first set: O(m log n) for an
/// <see cref="T:System.Collections.Immutable.ImmutableHashSet`1"/>, where n and m are the numbers of elements of the first
/// and the second set. The function wraps
/// <see cref="M:System.Collections.Immutable.IImmutableSet`1.IsSupersetOf(System.Collections.Generic.IEnumerable{`0})"/>
/// and compares in the comparer of the first set.
/// </remarks>
[<CompiledName "IsSuperset">]
val isSuperset : set1 : IImmutableSet<'T> -> set2 : IImmutableSet<'T> -> bool

/// <summary>
/// Returns whether the sets have an element in common.
/// </summary>
/// <param name="set1">The first set.</param>
/// <param name="set2">The second set.</param>
/// <returns><see langword="true"/> when an element of the second set is in the first one.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set1"/> or <paramref name="set2"/> is <see langword="null"/>.</exception>
/// <remarks>
/// The complexity depends on the implementation of the first set: O(m log n) for an
/// <see cref="T:System.Collections.Immutable.ImmutableHashSet`1"/>, where n and m are the numbers of elements of the first
/// and the second set. The function wraps
/// <see cref="M:System.Collections.Immutable.IImmutableSet`1.Overlaps(System.Collections.Generic.IEnumerable{`0})"/>
/// and compares in the comparer of the first set.
/// </remarks>
[<CompiledName "Overlaps">]
val overlaps : set1 : IImmutableSet<'T> -> set2 : IImmutableSet<'T> -> bool

/// <summary>
/// Returns whether the sets have the same elements.
/// </summary>
/// <param name="set1">The first set.</param>
/// <param name="set2">The second set.</param>
/// <returns><see langword="true"/> when every element of each set is in the other one.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set1"/> or <paramref name="set2"/> is <see langword="null"/>.</exception>
/// <remarks>
/// The complexity depends on the implementation of the first set: O((n + m) log (n + m)) for an
/// <see cref="T:System.Collections.Immutable.ImmutableHashSet`1"/>, where n and m are the numbers of elements of the first
/// and the second set. The function wraps
/// <see cref="M:System.Collections.Immutable.IImmutableSet`1.SetEquals(System.Collections.Generic.IEnumerable{`0})"/>
/// and compares in the comparer of the first set.
/// </remarks>
[<CompiledName "SetEquals">]
val setEquals : set1 : IImmutableSet<'T> -> set2 : IImmutableSet<'T> -> bool

// ---- Transformation ----

/// <summary>
/// Returns a set with a value added.
/// </summary>
/// <param name="value">The value to add.</param>
/// <param name="set">The input set.</param>
/// <returns>A set of the elements and the value, with the implementation and the comparer of the input.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>
/// The complexity depends on the implementation: O(log n) for
/// <see cref="T:System.Collections.Immutable.ImmutableHashSet`1"/> and <see cref="T:System.Collections.Immutable.ImmutableSortedSet`1"/>.
/// The function wraps <see cref="M:System.Collections.Immutable.IImmutableSet`1.Add(`0)"/>.
/// </remarks>
[<CompiledName "Add">]
val add : value : 'T -> set : IImmutableSet<'T> -> IImmutableSet<'T>

/// <summary>
/// Returns a set with the elements of a sequence added.
/// </summary>
/// <param name="elements">The elements to add.</param>
/// <param name="set">The input set.</param>
/// <returns>
/// A set of the elements and the elements of the sequence, with the implementation and the comparer of the input; the
/// input itself when the sequence adds no element.
/// </returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="elements"/> is <see langword="null"/> or a default flat list, or <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>
/// <para>
/// The complexity depends on the implementation of the set: O(m log (n + m)) for an
/// <see cref="T:System.Collections.Immutable.ImmutableHashSet`1"/>, where n and m are the numbers of elements of the set
/// and the sequence. The function wraps
/// <see cref="M:System.Collections.Immutable.IImmutableSet`1.Union(System.Collections.Generic.IEnumerable{`0})"/>.
/// </para>
/// <para>
/// Which of two elements that the comparer of the set considers equal stays depends on the implementation (ADR 0001,
/// D14). An <see cref="T:System.Collections.Immutable.ImmutableHashSet`1"/> keeps the element of the set, and the first
/// of several equal elements of the sequence. An <see cref="T:System.Collections.Immutable.ImmutableSortedSet`1"/> keeps
/// the element of the set when the sequence is about as large as the set or smaller, and may replace it when the
/// sequence is much larger.
/// </para>
/// </remarks>
[<CompiledName "AddRange">]
val addRange : elements : seq<'T> -> set : IImmutableSet<'T> -> IImmutableSet<'T>

/// <summary>
/// Returns the values that the chooser returns for the elements.
/// </summary>
/// <param name="chooser">The function that returns a value, or <c>ValueNone</c> to skip the element.</param>
/// <param name="set">The input set.</param>
/// <returns>
/// The hash set of the distinct chosen values, which compares with
/// <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>.
/// </returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n log n).
/// </remarks>
[<CompiledName "Choose">]
val choose : chooser : ('T -> 'U voption) -> set : IImmutableSet<'T> -> HashSet<'U>

/// <summary>
/// Returns an empty set with the implementation and the comparer of the input.
/// </summary>
/// <param name="set">The input set.</param>
/// <returns>The empty set.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>
/// The complexity depends on the implementation: O(1) for
/// <see cref="T:System.Collections.Immutable.ImmutableHashSet`1"/> and <see cref="T:System.Collections.Immutable.ImmutableSortedSet`1"/>.
/// The function wraps <see cref="M:System.Collections.Immutable.IImmutableSet`1.Clear"/>.
/// </remarks>
[<CompiledName "Clear">]
val clear : set : IImmutableSet<'T> -> IImmutableSet<'T>

/// <summary>
/// Returns the elements of the first set that are not in the second one.
/// </summary>
/// <param name="set1">The first set.</param>
/// <param name="set2">The set whose elements are removed from the first one.</param>
/// <returns>The difference, with the implementation and the comparer of the first set.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set1"/> or <paramref name="set2"/> is <see langword="null"/>.</exception>
/// <remarks>
/// The complexity depends on the implementation of the first set: O(m log n) for an
/// <see cref="T:System.Collections.Immutable.ImmutableHashSet`1"/>, where n and m are the numbers of elements of the first
/// and the second set. The function wraps
/// <see cref="M:System.Collections.Immutable.IImmutableSet`1.Except(System.Collections.Generic.IEnumerable{`0})"/>.
/// </remarks>
[<CompiledName "Difference">]
val difference : set1 : IImmutableSet<'T> -> set2 : IImmutableSet<'T> -> IImmutableSet<'T>

/// <summary>
/// Returns the elements that satisfy the predicate.
/// </summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="set">The input set.</param>
/// <returns>The set of the elements that satisfy the predicate, with the implementation and the comparer of the input.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>
/// The complexity depends on the implementation: O(n log n) for
/// <see cref="T:System.Collections.Immutable.ImmutableHashSet`1"/> and <see cref="T:System.Collections.Immutable.ImmutableSortedSet`1"/>.
/// </remarks>
[<CompiledName "Filter">]
val filter : predicate : ('T -> bool) -> set : IImmutableSet<'T> -> IImmutableSet<'T>

/// <summary>
/// Returns the elements that are in both sets.
/// </summary>
/// <param name="set1">The first set.</param>
/// <param name="set2">The second set.</param>
/// <returns>The intersection, with the implementation and the comparer of the first set.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set1"/> or <paramref name="set2"/> is <see langword="null"/>.</exception>
/// <remarks>
/// The complexity depends on the implementation of the first set: O(m log n) for an
/// <see cref="T:System.Collections.Immutable.ImmutableHashSet`1"/>, where n and m are the numbers of elements of the first
/// and the second set. The function wraps
/// <see cref="M:System.Collections.Immutable.IImmutableSet`1.Intersect(System.Collections.Generic.IEnumerable{`0})"/>.
/// </remarks>
[<CompiledName "Intersect">]
val intersect : set1 : IImmutableSet<'T> -> set2 : IImmutableSet<'T> -> IImmutableSet<'T>

/// <summary>
/// Returns the elements that are in every set of a sequence.
/// </summary>
/// <param name="sets">The sets.</param>
/// <returns>The intersection, with the implementation and the comparer of the first set.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="sets"/> is <see langword="null"/> or a default flat list, or one of its sets is <see langword="null"/>.</exception>
/// <exception cref="T:System.ArgumentException">Thrown when the sequence is empty.</exception>
/// <remarks>
/// The complexity depends on the implementation of the first set: O(N log n) for an
/// <see cref="T:System.Collections.Immutable.ImmutableHashSet`1"/>, where N is the total number of elements of the other
/// sets and n the number of elements of the first one. The function intersects the sets from the first one to the last one.
/// </remarks>
[<CompiledName "IntersectMany">]
val intersectMany : sets : seq<IImmutableSet<'T>> -> IImmutableSet<'T>

/// <summary>Returns the results of a function applied to each element.</summary>
/// <param name="mapping">The function to transform the elements.</param>
/// <param name="set">The input set.</param>
/// <returns>
/// The hash set of the distinct results, which compares with
/// <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>.
/// </returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>O(n log n).</remarks>
[<CompiledName "Map">]
val map : mapping : ('T -> 'U) -> set : IImmutableSet<'T> -> HashSet<'U>

/// <summary>
/// Returns a set without an element equal to a value.
/// </summary>
/// <param name="value">The value to remove.</param>
/// <param name="set">The input set.</param>
/// <returns>A set of the other elements, with the implementation and the comparer of the input.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>
/// The complexity depends on the implementation: O(log n) for
/// <see cref="T:System.Collections.Immutable.ImmutableHashSet`1"/> and <see cref="T:System.Collections.Immutable.ImmutableSortedSet`1"/>.
/// The function wraps <see cref="M:System.Collections.Immutable.IImmutableSet`1.Remove(`0)"/>.
/// </remarks>
[<CompiledName "Remove">]
val remove : value : 'T -> set : IImmutableSet<'T> -> IImmutableSet<'T>

/// <summary>
/// Returns a set without the elements that are equal to an element of a sequence.
/// </summary>
/// <param name="elements">The elements to remove; an element without an equal in the set is skipped.</param>
/// <param name="set">The input set.</param>
/// <returns>
/// A set of the other elements, with the implementation and the comparer of the input; the input itself when it has no
/// element equal to an element of the sequence.
/// </returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="elements"/> is <see langword="null"/> or a default flat list, or <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>
/// <para>
/// The complexity depends on the implementation of the set: O(m log n) for an
/// <see cref="T:System.Collections.Immutable.ImmutableHashSet`1"/>, where n and m are the numbers of elements of the set
/// and the sequence. The function wraps
/// <see cref="M:System.Collections.Immutable.IImmutableSet`1.Except(System.Collections.Generic.IEnumerable{`0})"/>
/// and compares in the comparer of the set.
/// </para>
/// <para>
/// An <see cref="T:System.Collections.Immutable.ImmutableHashSet`1"/> and an
/// <see cref="T:System.Collections.Immutable.ImmutableSortedSet`1"/> return the input itself from the wrapped member when
/// nothing is removed; the function does not check the result of another implementation.
/// </para>
/// </remarks>
[<CompiledName "RemoveRange">]
val removeRange : elements : seq<'T> -> set : IImmutableSet<'T> -> IImmutableSet<'T>

/// <summary>
/// Returns the elements that are in exactly one of the sets.
/// </summary>
/// <param name="set1">The first set.</param>
/// <param name="set2">The second set.</param>
/// <returns>The symmetric difference, with the implementation and the comparer of the first set.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set1"/> or <paramref name="set2"/> is <see langword="null"/>.</exception>
/// <remarks>
/// The complexity depends on the implementation of the first set: O((n + m) log (n + m)) for an
/// <see cref="T:System.Collections.Immutable.ImmutableHashSet`1"/>, where n and m are the numbers of elements of the first
/// and the second set. The function wraps
/// <see cref="M:System.Collections.Immutable.IImmutableSet`1.SymmetricExcept(System.Collections.Generic.IEnumerable{`0})"/>.
/// </remarks>
[<CompiledName "SymmetricDifference">]
val symmetricDifference : set1 : IImmutableSet<'T> -> set2 : IImmutableSet<'T> -> IImmutableSet<'T>

/// <summary>
/// Returns the elements that are in either set.
/// </summary>
/// <param name="set1">The first set.</param>
/// <param name="set2">The second set.</param>
/// <returns>The union, with the implementation and the comparer of the first set.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set1"/> or <paramref name="set2"/> is <see langword="null"/>.</exception>
/// <remarks>
/// The complexity depends on the implementation of the first set: O(m log (n + m)) for an
/// <see cref="T:System.Collections.Immutable.ImmutableHashSet`1"/>, where n and m are the numbers of elements of the first
/// and the second set. The function wraps
/// <see cref="M:System.Collections.Immutable.IImmutableSet`1.Union(System.Collections.Generic.IEnumerable{`0})"/>.
/// </remarks>
[<CompiledName "Union">]
val union : set1 : IImmutableSet<'T> -> set2 : IImmutableSet<'T> -> IImmutableSet<'T>

/// <summary>
/// Returns the elements that are in any set of a sequence.
/// </summary>
/// <param name="sets">The sets.</param>
/// <returns>
/// The union, with the implementation and the comparer of the first set; for an empty sequence, the empty
/// <see cref="T:FSharp.Collections.Immutable.HashSet`1"/>, which compares with
/// <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>.
/// </returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="sets"/> is <see langword="null"/> or a default flat list, or one of its sets is <see langword="null"/>.</exception>
/// <remarks>
/// The complexity depends on the implementation of the first set: O(N log N) for an
/// <see cref="T:System.Collections.Immutable.ImmutableHashSet`1"/>, where N is the total number of elements of the sets.
/// The function unites the sets from the first one to the last one.
/// </remarks>
[<CompiledName "UnionMany">]
val unionMany : sets : seq<IImmutableSet<'T>> -> IImmutableSet<'T>

// ---- Folding ----

/// <summary>
/// Applies a function to a state and each element, in the order of enumeration.
/// </summary>
/// <param name="folder">The function that computes the next state.</param>
/// <param name="state">The initial state.</param>
/// <param name="set">The input set.</param>
/// <returns>The final state.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Fold">]
val fold : folder : ('State -> 'T -> 'State) -> state : 'State -> set : IImmutableSet<'T> -> 'State

/// <summary>
/// Applies a function to each element and a state, in the reverse order of enumeration.
/// </summary>
/// <param name="folder">The function that computes the next state.</param>
/// <param name="set">The input set.</param>
/// <param name="state">The initial state.</param>
/// <returns>The final state.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n). The function copies the elements into an array first.
/// </remarks>
[<CompiledName "FoldBack">]
val foldBack : folder : ('T -> 'State -> 'State) -> set : IImmutableSet<'T> -> state : 'State -> 'State

/// <summary>
/// Applies an action to each element, in the order of enumeration.
/// </summary>
/// <param name="action">The action.</param>
/// <param name="set">The input set.</param>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Iterate">]
val iter : action : ('T -> unit) -> set : IImmutableSet<'T> -> unit

// ---- Partitioning ----

/// <summary>
/// Splits the elements by a predicate.
/// </summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="set">The input set.</param>
/// <returns>
/// The set of the elements that satisfy the predicate and the set of the other ones, both with the implementation and
/// the comparer of the input.
/// </returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>
/// The complexity depends on the implementation: O(n log n) for
/// <see cref="T:System.Collections.Immutable.ImmutableHashSet`1"/> and <see cref="T:System.Collections.Immutable.ImmutableSortedSet`1"/>.
/// </remarks>
[<CompiledName "Partition">]
val partition : predicate : ('T -> bool) -> set : IImmutableSet<'T> -> struct (IImmutableSet<'T> * IImmutableSet<'T>)

/// <summary>
/// Splits the elements by the choice that a function returns for them.
/// </summary>
/// <param name="partitioner">The function that returns <c>Choice1Of2</c> or <c>Choice2Of2</c> for an element.</param>
/// <param name="set">The input set.</param>
/// <returns>
/// The hash set of the values of the first choices and the hash set of the values of the second choices, which compare
/// with <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>.
/// </returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n log n).
/// </remarks>
[<CompiledName "PartitionWith">]
val partitionWith : partitioner : ('T -> Choice<'T1, 'T2>) -> set : IImmutableSet<'T> -> struct (HashSet<'T1> * HashSet<'T2>)

// ---- Conversion ----

/// <summary>
/// Returns a new array with the elements, in the order of enumeration.
/// </summary>
/// <param name="set">The input set.</param>
/// <returns>A copy of the elements.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "ToArray">]
val toArray : set : IImmutableSet<'T> -> 'T array

/// <summary>Returns a list of the elements, in the order of enumeration.</summary>
/// <param name="set">The input set.</param>
/// <returns>The list of the elements.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>O(n).</remarks>
[<CompiledName "ToList">]
val toList : set : IImmutableSet<'T> -> 'T list

/// <summary>Returns the set as a sequence.</summary>
/// <param name="set">The input set.</param>
/// <returns>The sequence of the elements.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>O(1).</remarks>
[<CompiledName "ToSeq">]
val toSeq : set : IImmutableSet<'T> -> seq<'T>
