/// <summary>
/// Functions for <see cref="T:System.Collections.Frozen.FrozenSet`1"/>, the frozen set.
/// </summary>
/// <remarks>
/// <para>
/// A frozen set is built once and read many times: it costs more to create than a hash set and finds its elements faster.
/// The module therefore offers creation, lookup, iteration, folds, conversions and the transformations of a whole set,
/// but no function that adds or removes a single element (ADR 0001, D1). Every transformation is an O(n) rebuild: it
/// collects its result and creates a new frozen set from it.
/// </para>
/// <para>
/// The module mirrors <see cref="T:Microsoft.FSharp.Collections.SetModule"/>: each function has the semantics, the
/// argument order and the exceptions of the function with the same name in that module (ADR 0001). Functions that
/// FSharp.Core lacks wrap a member of <see cref="T:System.Collections.Frozen.FrozenSet`1"/> and throw what that member
/// throws. Every function throws <see cref="T:System.ArgumentNullException"/> for a <see langword="null"/> frozen set.
/// </para>
/// <para>
/// A frozen set finds an element in O(1) on average. The order in which a frozen set enumerates its elements is
/// unspecified; the functions that visit the elements visit them in that order.
/// </para>
/// <para>
/// The creation functions create frozen sets that compare with
/// <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>.
/// <see cref="M:FSharp.Collections.Immutable.FrozenSet.ForComparer``1(System.Collections.Generic.IEqualityComparer{``0})"/>
/// creates a <see cref="T:FSharp.Collections.Immutable.CustomComparerFrozenSet`1"/>, whose members create frozen sets with
/// another comparer (ADR 0001, D12).
/// </para>
/// <para>
/// The transformations that keep the element type keep the comparer of their input; the set algebra keeps the comparer of
/// the first set and compares with it. The transformations that change the element type compare the new elements with
/// the default comparer of their type.
/// </para>
/// <para>
/// The module has the name of the class <see cref="T:System.Collections.Frozen.FrozenSet"/> and coexists with it: after
/// <c>open System.Collections.Frozen</c>, <c>FrozenSet.ofSeq</c> calls this module and <c>FrozenSet.ToFrozenSet</c> calls
/// the class, whichever namespace is opened last (ADR 0001, D3).
/// </para>
/// <para>
/// Functions that return an optional value return a <see cref="T:Microsoft.FSharp.Core.FSharpValueOption`1"/>, and functions
/// that return tuples return struct tuples. The namespaces <see cref="N:FSharp.Collections.Immutable.Option"/> and
/// <see cref="N:FSharp.Collections.Immutable.Tuple"/> contain a module with the same name whose functions use options
/// and reference tuples instead.
/// </para>
/// </remarks>
[<RequireQualifiedAccess>]
module FSharp.Collections.Immutable.FrozenSet

open System
open System.Collections.Frozen
open System.Collections.Generic

// ---- Creation ----

/// <summary>
/// The empty frozen set, which compares with <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>.
/// </summary>
/// <remarks>
/// O(1). The value is <see cref="P:System.Collections.Frozen.FrozenSet`1.Empty"/>.
/// </remarks>
[<GeneralizableValue>]
[<CompiledName "Empty">]
val empty<'T> : FrozenSet<'T>

/// <summary>
/// Creates the struct that creates frozen sets that compare their elements with an equality comparer.
/// </summary>
/// <param name="comparer">The equality comparer of the elements.</param>
/// <returns>
/// The <see cref="T:FSharp.Collections.Immutable.CustomComparerFrozenSet`1"/> whose members are the creation functions of
/// this module with the comparer: they create frozen sets that compare with it.
/// </returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="comparer"/> is <see langword="null"/>.</exception>
/// <remarks>
/// <para>
/// O(1). Creating the struct allocates nothing.
/// <see cref="M:FSharp.Collections.Immutable.CustomComparerFrozenSet`1.OfSeq(System.Collections.Generic.IEnumerable{`0})"/>
/// also gives an existing frozen set the comparer (ADR 0001, D12).
/// </para>
/// <para>
/// A <see langword="null"/> comparer throws, where the BCL methods <c>Create</c> and <c>ToFrozenSet</c> take it as the
/// default comparer. For that meaning use
/// <see cref="M:FSharp.Collections.Immutable.FrozenSet.OfSeq``1(System.Collections.Generic.IEnumerable{``0})"/>, which gives
/// a frozen set the default comparer, or the default value of the struct, which compares with
/// <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>.
/// </para>
/// </remarks>
/// <example id="forComparer-1">
/// <code lang="fsharp">
/// let names = (FrozenSet.forComparer StringComparer.OrdinalIgnoreCase).ofSeq [ "Ann"; "ANN"; "Bob" ]
/// </code>
/// Evaluates to a frozen set of "Ann" and "Bob" that compares without case.
/// </example>
[<CompiledName "ForComparer">]
val forComparer : comparer : IEqualityComparer<'T> -> CustomComparerFrozenSet<'T>

/// <summary>
/// Creates a frozen set from the elements of an array.
/// </summary>
/// <param name="array">The input array.</param>
/// <returns>The frozen set of the distinct elements of the array.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="array"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n). Later changes to the array do not change the frozen set.
/// </remarks>
[<CompiledName "OfArray">]
val ofArray : array : 'T array -> FrozenSet<'T>

/// <summary>Creates a frozen set from the elements of a list.</summary>
/// <param name="elements">The input list.</param>
/// <returns>The frozen set of the distinct elements of the list.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="elements"/> is <see langword="null"/>.</exception>
/// <remarks>O(n).</remarks>
[<CompiledName "OfList">]
val ofList : elements : 'T list -> FrozenSet<'T>

/// <summary>
/// Creates a frozen set from the elements of a read-only span.
/// </summary>
/// <param name="span">The input span.</param>
/// <returns>
/// The frozen set of the distinct elements of the span; an empty span gives an empty frozen set.
/// </returns>
/// <remarks>
/// O(n). Later changes to the memory under the span do not change the frozen set. The function wraps
/// <see cref="M:System.Collections.Frozen.FrozenSet.Create``1(System.ReadOnlySpan{``0})"/>.
/// </remarks>
[<CompiledName "OfReadOnlySpan">]
val ofReadOnlySpan : span : ReadOnlySpan<'T> -> FrozenSet<'T>

/// <summary>
/// Creates a frozen set from the elements of a sequence.
/// </summary>
/// <param name="elements">The input sequence.</param>
/// <returns>The frozen set of the distinct elements of the sequence.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="elements"/> is <see langword="null"/> or a default flat list.</exception>
/// <remarks>
/// O(n), or O(1) for a frozen set that compares with the default comparer: the function returns such a frozen set
/// without a copy, and gives a frozen set with another comparer the default one. The function wraps
/// <see cref="M:System.Collections.Frozen.FrozenSet.ToFrozenSet``1(System.Collections.Generic.IEnumerable{``0},System.Collections.Generic.IEqualityComparer{``0})"/>.
/// </remarks>
[<CompiledName "OfSeq">]
val ofSeq : elements : seq<'T> -> FrozenSet<'T>

/// <summary>
/// Creates a frozen set from the elements of a span.
/// </summary>
/// <param name="span">The input span.</param>
/// <returns>
/// The frozen set of the distinct elements of the span; an empty span gives an empty frozen set.
/// </returns>
/// <remarks>
/// O(n). Later changes to the memory under the span do not change the frozen set. The function converts the span to a
/// read-only span and wraps
/// <see cref="M:System.Collections.Frozen.FrozenSet.Create``1(System.ReadOnlySpan{``0})"/>.
/// </remarks>
[<CompiledName "OfSpan">]
val ofSpan : span : Span<'T> -> FrozenSet<'T>

/// <summary>Creates a frozen set of one element.</summary>
/// <param name="value">The element.</param>
/// <returns>The frozen set of one element.</returns>
/// <remarks>O(1).</remarks>
[<CompiledName "Singleton">]
val singleton : value : 'T -> FrozenSet<'T>

// ---- Properties ----

/// <summary>
/// Returns the equality comparer of the elements of the frozen set.
/// </summary>
/// <param name="set">The input frozen set.</param>
/// <returns>The equality comparer.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(1). The function wraps <see cref="P:System.Collections.Frozen.FrozenSet`1.Comparer"/>.
/// </remarks>
[<CompiledName "Comparer">]
val comparer : set : FrozenSet<'T> -> IEqualityComparer<'T>

/// <summary>Returns the number of elements of the frozen set.</summary>
/// <param name="set">The input frozen set.</param>
/// <returns>The number of elements.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>O(1).</remarks>
[<CompiledName "Count">]
val count : set : FrozenSet<'T> -> int

/// <summary>Returns whether the frozen set has no elements.</summary>
/// <param name="set">The input frozen set.</param>
/// <returns><see langword="true"/> when the frozen set is empty.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>O(1).</remarks>
[<CompiledName "IsEmpty">]
val isEmpty : set : FrozenSet<'T> -> bool

// ---- Access ----

/// <summary>
/// Returns the element of the frozen set that equals a value, or <c>ValueNone</c> when there is none.
/// </summary>
/// <param name="value">The value to search for.</param>
/// <param name="set">The input frozen set.</param>
/// <returns>The element that the comparer of the frozen set considers equal to the value, or <c>ValueNone</c>.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(1) on average. The function wraps <see cref="M:System.Collections.Frozen.FrozenSet`1.TryGetValue(`0,`0@)"/>. The
/// element can differ from an equal value, for example in case under a case-insensitive comparer; the function returns
/// the instance that the frozen set holds.
/// </remarks>
[<CompiledName "TryGetValue">]
val tryGetValue : value : 'T -> set : FrozenSet<'T> -> 'T voption

// ---- Search ----

/// <summary>
/// Returns whether the frozen set contains an element equal to a value, in the comparer of the frozen set.
/// </summary>
/// <param name="element">The value to find.</param>
/// <param name="set">The input frozen set.</param>
/// <returns><see langword="true"/> when an element equals the value.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(1) on average. The function wraps <see cref="M:System.Collections.Frozen.FrozenSet`1.Contains(`0)"/>.
/// </remarks>
[<CompiledName "Contains">]
val contains : element : 'T -> set : FrozenSet<'T> -> bool

/// <summary>
/// Returns whether any element satisfies the predicate.
/// </summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="set">The input frozen set.</param>
/// <returns><see langword="true"/> when an element satisfies the predicate.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n). The function stops at the first element that satisfies the predicate.
/// </remarks>
[<CompiledName "Exists">]
val exists : predicate : ('T -> bool) -> set : FrozenSet<'T> -> bool

/// <summary>
/// Returns whether all elements satisfy the predicate.
/// </summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="set">The input frozen set.</param>
/// <returns><see langword="true"/> when every element satisfies the predicate.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n). The function stops at the first element that does not satisfy the predicate.
/// </remarks>
[<CompiledName "ForAll">]
val forall : predicate : ('T -> bool) -> set : FrozenSet<'T> -> bool

/// <summary>
/// Returns whether every element of the first frozen set is in the second one, and the second one has an element that
/// the first one does not have.
/// </summary>
/// <param name="set1">The potential subset.</param>
/// <param name="set2">The set to test against.</param>
/// <returns><see langword="true"/> when the first frozen set is a proper subset of the second one.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set1"/> or <paramref name="set2"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n + m), where n and m are the numbers of elements of the first and the second frozen set. The function wraps
/// <see cref="M:System.Collections.Frozen.FrozenSet`1.IsProperSubsetOf(System.Collections.Generic.IEnumerable{`0})"/>
/// and compares in the comparer of the first frozen set.
/// </remarks>
[<CompiledName "IsProperSubset">]
val isProperSubset : set1 : FrozenSet<'T> -> set2 : FrozenSet<'T> -> bool

/// <summary>
/// Returns whether every element of the second frozen set is in the first one, and the first one has an element that
/// the second one does not have.
/// </summary>
/// <param name="set1">The potential superset.</param>
/// <param name="set2">The set to test against.</param>
/// <returns><see langword="true"/> when the first frozen set is a proper superset of the second one.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set1"/> or <paramref name="set2"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(m), where m is the number of elements of the second frozen set. The function wraps
/// <see cref="M:System.Collections.Frozen.FrozenSet`1.IsProperSupersetOf(System.Collections.Generic.IEnumerable{`0})"/>
/// and compares in the comparer of the first frozen set.
/// </remarks>
[<CompiledName "IsProperSuperset">]
val isProperSuperset : set1 : FrozenSet<'T> -> set2 : FrozenSet<'T> -> bool

/// <summary>
/// Returns whether every element of the first frozen set is in the second one.
/// </summary>
/// <param name="set1">The potential subset.</param>
/// <param name="set2">The set to test against.</param>
/// <returns><see langword="true"/> when the first frozen set is a subset of the second one.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set1"/> or <paramref name="set2"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n + m), where n and m are the numbers of elements of the first and the second frozen set. The function wraps
/// <see cref="M:System.Collections.Frozen.FrozenSet`1.IsSubsetOf(System.Collections.Generic.IEnumerable{`0})"/>
/// and compares in the comparer of the first frozen set.
/// </remarks>
[<CompiledName "IsSubset">]
val isSubset : set1 : FrozenSet<'T> -> set2 : FrozenSet<'T> -> bool

/// <summary>
/// Returns whether every element of the second frozen set is in the first one.
/// </summary>
/// <param name="set1">The potential superset.</param>
/// <param name="set2">The set to test against.</param>
/// <returns><see langword="true"/> when the first frozen set is a superset of the second one.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set1"/> or <paramref name="set2"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(m), where m is the number of elements of the second frozen set. The function wraps
/// <see cref="M:System.Collections.Frozen.FrozenSet`1.IsSupersetOf(System.Collections.Generic.IEnumerable{`0})"/>
/// and compares in the comparer of the first frozen set.
/// </remarks>
[<CompiledName "IsSuperset">]
val isSuperset : set1 : FrozenSet<'T> -> set2 : FrozenSet<'T> -> bool

/// <summary>
/// Returns whether the frozen sets have an element in common.
/// </summary>
/// <param name="set1">The first frozen set.</param>
/// <param name="set2">The second frozen set.</param>
/// <returns><see langword="true"/> when an element of the second frozen set is in the first one.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set1"/> or <paramref name="set2"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(m), where m is the number of elements of the second frozen set. The function wraps
/// <see cref="M:System.Collections.Frozen.FrozenSet`1.Overlaps(System.Collections.Generic.IEnumerable{`0})"/>
/// and compares in the comparer of the first frozen set.
/// </remarks>
[<CompiledName "Overlaps">]
val overlaps : set1 : FrozenSet<'T> -> set2 : FrozenSet<'T> -> bool

/// <summary>
/// Returns whether the frozen sets have the same elements.
/// </summary>
/// <param name="set1">The first frozen set.</param>
/// <param name="set2">The second frozen set.</param>
/// <returns><see langword="true"/> when every element of each frozen set is in the other one.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set1"/> or <paramref name="set2"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(m), where m is the number of elements of the second frozen set. The function wraps
/// <see cref="M:System.Collections.Frozen.FrozenSet`1.SetEquals(System.Collections.Generic.IEnumerable{`0})"/>
/// and compares in the comparer of the first frozen set.
/// </remarks>
[<CompiledName "SetEquals">]
val setEquals : set1 : FrozenSet<'T> -> set2 : FrozenSet<'T> -> bool

// ---- Transformation ----

/// <summary>
/// Returns the values that the chooser returns for the elements.
/// </summary>
/// <param name="chooser">The function that returns a value, or <c>ValueNone</c> to skip the element.</param>
/// <param name="set">The input frozen set.</param>
/// <returns>
/// The frozen set of the distinct chosen values, which compares with
/// <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>.
/// </returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n): the function builds a new frozen set. The comparer of the input is not kept, even when the chosen values have
/// the type of the elements: the function cannot keep it for every type without a type test that would make its result
/// depend on the type.
/// </remarks>
[<CompiledName "Choose">]
val choose : chooser : ('T -> 'U voption) -> set : FrozenSet<'T> -> FrozenSet<'U>

/// <summary>
/// Returns the elements of the first frozen set that are not in the second one.
/// </summary>
/// <param name="set1">The first frozen set.</param>
/// <param name="set2">The frozen set whose elements are removed from the first one.</param>
/// <returns>The difference, with the comparer of the first frozen set.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set1"/> or <paramref name="set2"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n + m), where n and m are the numbers of elements of the first and the second frozen set: the function builds a
/// new frozen set. It compares in the comparer of the first frozen set.
/// </remarks>
[<CompiledName "Difference">]
val difference : set1 : FrozenSet<'T> -> set2 : FrozenSet<'T> -> FrozenSet<'T>

/// <summary>Returns the elements that satisfy the predicate.</summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="set">The input frozen set.</param>
/// <returns>The frozen set of the elements that satisfy the predicate, with the comparer of the input.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>O(n): the function builds a new frozen set.</remarks>
[<CompiledName "Filter">]
val filter : predicate : ('T -> bool) -> set : FrozenSet<'T> -> FrozenSet<'T>

/// <summary>
/// Returns the elements that are in both frozen sets.
/// </summary>
/// <param name="set1">The first frozen set.</param>
/// <param name="set2">The second frozen set.</param>
/// <returns>
/// The intersection, with the comparer of the first frozen set; where the comparer considers two different elements
/// equal, the intersection holds the one of the second frozen set.
/// </returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set1"/> or <paramref name="set2"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(m), where m is the number of elements of the second frozen set: the function looks them up in the first frozen
/// set, in its comparer, and builds a new frozen set of the elements of the second frozen set that it finds. It keeps
/// the elements of the second frozen set, as
/// <see cref="M:System.Collections.Immutable.ImmutableHashSet`1.Intersect(System.Collections.Generic.IEnumerable{`0})"/>
/// and <see cref="M:System.Collections.Immutable.ImmutableSortedSet`1.Intersect(System.Collections.Generic.IEnumerable{`0})"/>
/// do (ADR 0001, D14).
/// </remarks>
[<CompiledName "Intersect">]
val intersect : set1 : FrozenSet<'T> -> set2 : FrozenSet<'T> -> FrozenSet<'T>

/// <summary>
/// Returns the elements that are in every frozen set of a sequence.
/// </summary>
/// <param name="sets">The frozen sets.</param>
/// <returns>
/// The intersection, with the comparer of the first frozen set; where the comparer considers different elements equal,
/// the intersection holds the one of the last frozen set, as
/// <see cref="M:FSharp.Collections.Immutable.FrozenSet.Intersect``1(System.Collections.Frozen.FrozenSet{``0},System.Collections.Frozen.FrozenSet{``0})"/>
/// holds the one of its second frozen set. For a sequence of one frozen set, that frozen set itself.
/// </returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="sets"/> is <see langword="null"/> or a default flat list, or one of its frozen sets is <see langword="null"/>.</exception>
/// <exception cref="T:System.ArgumentException">Thrown when the sequence is empty.</exception>
/// <remarks>
/// O(N), where N is the total number of elements of the frozen sets after the first one. The function intersects the
/// frozen sets from the first one to the last one and builds a new frozen set at each step; an intersection never grows,
/// so the steps together take O(N).
/// </remarks>
[<CompiledName "IntersectMany">]
val intersectMany : sets : seq<FrozenSet<'T>> -> FrozenSet<'T>

/// <summary>
/// Returns the results of a function applied to each element.
/// </summary>
/// <param name="mapping">The function to transform the elements.</param>
/// <param name="set">The input frozen set.</param>
/// <returns>
/// The frozen set of the distinct results, which compares with
/// <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>.
/// </returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n): the function builds a new frozen set. The comparer of the input is not kept, even when the results have the
/// type of the elements: the function cannot keep it for every type without a type test that would make its result
/// depend on the type.
/// </remarks>
[<CompiledName "Map">]
val map : mapping : ('T -> 'U) -> set : FrozenSet<'T> -> FrozenSet<'U>

/// <summary>
/// Returns the elements that are in exactly one of the frozen sets.
/// </summary>
/// <param name="set1">The first frozen set.</param>
/// <param name="set2">The second frozen set.</param>
/// <returns>The symmetric difference, with the comparer of the first frozen set.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set1"/> or <paramref name="set2"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n + m), where n and m are the numbers of elements of the first and the second frozen set: the function builds a
/// new frozen set. It compares in the comparer of the first frozen set.
/// </remarks>
[<CompiledName "SymmetricDifference">]
val symmetricDifference : set1 : FrozenSet<'T> -> set2 : FrozenSet<'T> -> FrozenSet<'T>

/// <summary>
/// Returns the elements that are in either frozen set.
/// </summary>
/// <param name="set1">The first frozen set.</param>
/// <param name="set2">The second frozen set.</param>
/// <returns>
/// The union, with the comparer of the first frozen set; where the comparer considers two different elements equal,
/// the union holds the one of the first frozen set.
/// </returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set1"/> or <paramref name="set2"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n + m), where n and m are the numbers of elements of the first and the second frozen set: the function builds a
/// new frozen set. It compares in the comparer of the first frozen set.
/// </remarks>
[<CompiledName "Union">]
val union : set1 : FrozenSet<'T> -> set2 : FrozenSet<'T> -> FrozenSet<'T>

/// <summary>
/// Returns the elements that are in any frozen set of a sequence.
/// </summary>
/// <param name="sets">The frozen sets.</param>
/// <returns>
/// The union, with the comparer of the first frozen set; the empty frozen set with the default comparer for an empty
/// sequence.
/// </returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="sets"/> is <see langword="null"/> or a default flat list, or one of its frozen sets is <see langword="null"/>.</exception>
/// <remarks>
/// O(N), where N is the total number of elements of the frozen sets. The function collects the elements of the frozen
/// sets from the first one to the last one and builds one new frozen set from them.
/// </remarks>
[<CompiledName "UnionMany">]
val unionMany : sets : seq<FrozenSet<'T>> -> FrozenSet<'T>

// ---- Folding ----

/// <summary>
/// Applies a function to a state and each element, in the order of enumeration.
/// </summary>
/// <param name="folder">The function that computes the next state.</param>
/// <param name="state">The initial state.</param>
/// <param name="set">The input frozen set.</param>
/// <returns>The final state.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Fold">]
val fold : folder : ('State -> 'T -> 'State) -> state : 'State -> set : FrozenSet<'T> -> 'State

/// <summary>
/// Applies a function to each element and a state, in the reverse order of enumeration.
/// </summary>
/// <param name="folder">The function that computes the next state.</param>
/// <param name="set">The input frozen set.</param>
/// <param name="state">The initial state.</param>
/// <returns>The final state.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "FoldBack">]
val foldBack : folder : ('T -> 'State -> 'State) -> set : FrozenSet<'T> -> state : 'State -> 'State

/// <summary>
/// Applies an action to each element, in the order of enumeration.
/// </summary>
/// <param name="action">The action.</param>
/// <param name="set">The input frozen set.</param>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "Iterate">]
val iter : action : ('T -> unit) -> set : FrozenSet<'T> -> unit

// ---- Partitioning ----

/// <summary>Splits the elements by a predicate.</summary>
/// <param name="predicate">The function to test the elements.</param>
/// <param name="set">The input frozen set.</param>
/// <returns>
/// The frozen set of the elements that satisfy the predicate and the frozen set of the other ones, both with the
/// comparer of the input.
/// </returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>O(n): the function builds two new frozen sets.</remarks>
[<CompiledName "Partition">]
val partition : predicate : ('T -> bool) -> set : FrozenSet<'T> -> struct (FrozenSet<'T> * FrozenSet<'T>)

/// <summary>
/// Splits the elements by the choice that a function returns for them.
/// </summary>
/// <param name="partitioner">The function that returns <c>Choice1Of2</c> or <c>Choice2Of2</c> for an element.</param>
/// <param name="set">The input frozen set.</param>
/// <returns>
/// The frozen set of the values of the first choices and the frozen set of the values of the second choices, which
/// compare with <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>.
/// </returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n): the function builds two new frozen sets.
/// </remarks>
[<CompiledName "PartitionWith">]
val partitionWith : partitioner : ('T -> Choice<'T1, 'T2>) -> set : FrozenSet<'T> -> struct (FrozenSet<'T1> * FrozenSet<'T2>)

// ---- Conversion ----

/// <summary>
/// Returns a new array with the elements, in the order of enumeration.
/// </summary>
/// <param name="set">The input frozen set.</param>
/// <returns>A copy of the elements.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(n).
/// </remarks>
[<CompiledName "ToArray">]
val toArray : set : FrozenSet<'T> -> 'T array

/// <summary>
/// Returns the elements of the frozen set as a flat list, in the order of enumeration.
/// </summary>
/// <param name="set">The input frozen set.</param>
/// <returns>The flat list of the elements, which shares the storage of the frozen set.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>
/// O(1). The function returns <see cref="P:System.Collections.Frozen.FrozenSet`1.Items"/> without a copy.
/// </remarks>
[<CompiledName "ToFlatList">]
val toFlatList : set : FrozenSet<'T> -> FlatList<'T>

/// <summary>Returns a list of the elements, in the order of enumeration.</summary>
/// <param name="set">The input frozen set.</param>
/// <returns>The list of the elements.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>O(n).</remarks>
[<CompiledName "ToList">]
val toList : set : FrozenSet<'T> -> 'T list

/// <summary>Returns the frozen set as a sequence.</summary>
/// <param name="set">The input frozen set.</param>
/// <returns>The sequence of the elements.</returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>O(1).</remarks>
[<CompiledName "ToSeq">]
val toSeq : set : FrozenSet<'T> -> seq<'T>

#if NET10_0_OR_GREATER
// ---- Type-specific ----

/// <summary>
/// Returns a lookup that finds the elements of the frozen set by values of an alternate type.
/// </summary>
/// <param name="set">The input frozen set.</param>
/// <returns>
/// The lookup, whose <see cref="M:System.Collections.Frozen.FrozenSet`1.AlternateLookup`1.Contains(`1)"/> and
/// <see cref="M:System.Collections.Frozen.FrozenSet`1.AlternateLookup`1.TryGetValue(`1,`0@)"/> take a value of the
/// alternate type.
/// </returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <exception cref="T:System.InvalidOperationException">
/// Thrown when the comparer of the frozen set does not implement
/// <see cref="T:System.Collections.Generic.IAlternateEqualityComparer`2"/> for the alternate type and the element type.
/// </exception>
/// <remarks>
/// <para>
/// O(1). The function wraps <see cref="M:System.Collections.Frozen.FrozenSet`1.GetAlternateLookup``1"/> and exists on
/// <c>net10.0</c> only. The alternate type comes first, so that F# can infer the element type:
/// <c>FrozenSet.alternateLookup&lt;int, _&gt; set</c>.
/// </para>
/// <para>
/// An F# generic function cannot be instantiated with a byref-like type, so the function cannot look up by a
/// <see cref="T:System.ReadOnlySpan`1"/>, the alternate type of strings under
/// <see cref="P:System.StringComparer.Ordinal"/>. Call <see cref="M:System.Collections.Frozen.FrozenSet`1.GetAlternateLookup``1"/>
/// directly for a span: <c>set.GetAlternateLookup&lt;ReadOnlySpan&lt;char&gt;&gt;()</c>. A frozen set of strings supports
/// no other alternate type.
/// </para>
/// </remarks>
[<CompiledName "AlternateLookup">]
val alternateLookup<'Alternate, 'T> : set : FrozenSet<'T> -> FrozenSet<'T>.AlternateLookup<'Alternate>

/// <summary>
/// Returns a lookup that finds the elements of the frozen set by values of an alternate type, or <c>ValueNone</c> when
/// the comparer of the frozen set cannot compare that type.
/// </summary>
/// <param name="set">The input frozen set.</param>
/// <returns>
/// The lookup, or <c>ValueNone</c> when the comparer of the frozen set does not implement
/// <see cref="T:System.Collections.Generic.IAlternateEqualityComparer`2"/> for the alternate type and the element type.
/// </returns>
/// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="set"/> is <see langword="null"/>.</exception>
/// <remarks>
/// <para>
/// O(1). The function wraps
/// <see cref="M:System.Collections.Frozen.FrozenSet`1.TryGetAlternateLookup``1(System.Collections.Frozen.FrozenSet{`0}.AlternateLookup{``0}@)"/>
/// and exists on <c>net10.0</c> only. The alternate type comes first, so that F# can infer the element type:
/// <c>FrozenSet.tryAlternateLookup&lt;int, _&gt; set</c>.
/// </para>
/// <para>
/// An F# generic function cannot be instantiated with a byref-like type, so the function cannot look up by a
/// <see cref="T:System.ReadOnlySpan`1"/>, the alternate type of strings under
/// <see cref="P:System.StringComparer.Ordinal"/>. Call
/// <see cref="M:System.Collections.Frozen.FrozenSet`1.TryGetAlternateLookup``1(System.Collections.Frozen.FrozenSet{`0}.AlternateLookup{``0}@)"/>
/// directly for a span. A frozen set of strings supports no other alternate type, so the function returns
/// <c>ValueNone</c> for it.
/// </para>
/// </remarks>
[<CompiledName "TryAlternateLookup">]
val tryAlternateLookup<'Alternate, 'T> : set : FrozenSet<'T> -> FrozenSet<'T>.AlternateLookup<'Alternate> voption
#endif
