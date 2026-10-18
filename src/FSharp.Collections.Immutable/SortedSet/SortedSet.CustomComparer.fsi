namespace FSharp.Collections.Immutable

open System
open System.Collections.Generic
// Opened after System.Collections.Generic, so that SortedSet names the abbreviation of this library, not the mutable set
open FSharp.Collections.Immutable

/// <summary>
/// Creates sorted sets that order their elements with a comparer (ADR 0001, D12).
/// </summary>
/// <remarks>
/// <para>
/// <see cref="M:FSharp.Collections.Immutable.SortedSet.ForComparer``1(System.Collections.Generic.IComparer{``0})"/>
/// creates the struct. Its members are the creation functions of <see cref="T:FSharp.Collections.Immutable.SortedSet"/>
/// under the same names, with the comparer applied, so that a call reads as one phrase.
/// </para>
/// <para>
/// The struct holds only the comparer, so creating it allocates nothing. The element type needs no F# comparison, because
/// the comparer orders the elements. For the same reason, the default value of the struct, which no constructor creates,
/// cannot fall back to F# generic comparison: it orders with <see cref="P:System.Collections.Generic.Comparer`1.Default"/>,
/// as the BCL does for a <see langword="null"/> comparer.
/// </para>
/// </remarks>
/// <example id="customComparerSortedSet-1">
/// <code lang="fsharp">
/// let names = (SortedSet.forComparer StringComparer.OrdinalIgnoreCase).ofList [ "bob"; "Ann"; "ANN" ]
/// </code>
/// Evaluates to a sorted set of "Ann" and "bob", in this order, that compares without case.
/// </example>
[<Struct; NoComparison; NoEquality>]
type CustomComparerSortedSet<'T> =

    // A signature reveals the fields of a struct, private ones included (FS0314); null only in the default value
    val private comparer : IComparer<'T> | null

    /// <summary>Creates the struct for a comparer.</summary>
    /// <param name="comparer">The comparer that orders the elements of the sorted sets that the struct creates.</param>
    /// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="comparer"/> is <see langword="null"/>.</exception>
    new : comparer : IComparer<'T> -> CustomComparerSortedSet<'T>

    // ---- Creation ----

    /// <summary>
    /// The empty sorted set that orders with the comparer.
    /// </summary>
    /// <remarks>
    /// O(1). The member wraps
    /// <see cref="M:System.Collections.Immutable.ImmutableSortedSet.Create``1(System.Collections.Generic.IComparer{``0})"/>.
    /// </remarks>
    [<CompiledName "Empty">]
    member empty : SortedSet<'T>

    /// <summary>
    /// Creates a sorted set from the elements of an array, ordered by the comparer.
    /// </summary>
    /// <param name="array">The input array.</param>
    /// <returns>
    /// The sorted set of the elements that are distinct in the comparer; of the elements that the comparer considers equal,
    /// it holds the first one.
    /// </returns>
    /// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="array"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// O(n log n). Later changes to the array do not change the sorted set. The member adds the elements one by one to a
    /// builder that
    /// <see cref="M:System.Collections.Immutable.ImmutableSortedSet.CreateBuilder``1(System.Collections.Generic.IComparer{``0})"/>
    /// creates, as <see cref="M:FSharp.Collections.Immutable.CustomComparerSortedSet`1.OfSeq(System.Collections.Generic.IEnumerable{`0})"/>
    /// does.
    /// </remarks>
    [<CompiledName "OfArray">]
    member ofArray : array : 'T array -> SortedSet<'T>

    /// <summary>
    /// Creates a sorted set from the elements of a list, ordered by the comparer.
    /// </summary>
    /// <param name="elements">The input list.</param>
    /// <returns>
    /// The sorted set of the elements that are distinct in the comparer; of the elements that the comparer considers equal,
    /// it holds the first one.
    /// </returns>
    /// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="elements"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// O(n log n). The member adds the elements one by one to a builder, as
    /// <see cref="M:FSharp.Collections.Immutable.CustomComparerSortedSet`1.OfSeq(System.Collections.Generic.IEnumerable{`0})"/>
    /// does.
    /// </remarks>
    [<CompiledName "OfList">]
    member ofList : elements : 'T list -> SortedSet<'T>

    /// <summary>
    /// Creates a sorted set from the elements of a read-only span, ordered by the comparer.
    /// </summary>
    /// <param name="span">The input span.</param>
    /// <returns>
    /// The sorted set of the elements that are distinct in the comparer; of the elements that the comparer considers equal,
    /// it holds the first one. An empty span gives the empty sorted set.
    /// </returns>
    /// <remarks>
    /// <para>
    /// O(n log n). Later changes to the memory under the span do not change the sorted set. The member adds the elements
    /// one by one to a builder that
    /// <see cref="M:System.Collections.Immutable.ImmutableSortedSet.CreateBuilder``1(System.Collections.Generic.IComparer{``0})"/>
    /// creates.
    /// </para>
    /// <para>
    /// It does not wrap
    /// <see cref="M:System.Collections.Immutable.ImmutableSortedSet.Create``1(System.Collections.Generic.IComparer{``0},System.ReadOnlySpan{``0})"/>,
    /// which sorts the elements with an unstable sort and can keep any of the elements that compare equal.
    /// </para>
    /// </remarks>
    [<CompiledName "OfReadOnlySpan">]
    member ofReadOnlySpan : span : ReadOnlySpan<'T> -> SortedSet<'T>

    /// <summary>
    /// Creates a sorted set from the elements of a sequence, ordered by the comparer.
    /// </summary>
    /// <param name="elements">The input sequence, which can be a sorted set with another comparer.</param>
    /// <returns>
    /// The sorted set of the elements that are distinct in the comparer; of the elements that the comparer considers equal,
    /// it holds the first one.
    /// </returns>
    /// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="elements"/> is <see langword="null"/> or a default flat list.</exception>
    /// <remarks>
    /// <para>
    /// O(n log n). The member adds the elements one by one to a builder that
    /// <see cref="M:System.Collections.Immutable.ImmutableSortedSet.CreateBuilder``1(System.Collections.Generic.IComparer{``0})"/>
    /// creates. It does not call
    /// <see cref="M:System.Collections.Immutable.ImmutableSortedSet.CreateRange``1(System.Collections.Generic.IComparer{``0},System.Collections.Generic.IEnumerable{``0})"/>,
    /// which sorts the elements with an unstable sort and can keep any of the elements that compare equal.
    /// </para>
    /// <para>
    /// A sorted set gets the comparer, as
    /// <see cref="M:System.Collections.Immutable.ImmutableSortedSet`1.WithComparer(System.Collections.Generic.IComparer{`0})"/>
    /// gives it: the member returns the sorted set itself in O(1) when it orders with the same comparer instance, an
    /// empty one included, and otherwise a new sorted set of its elements in the comparer. The BCL tests the instance, not
    /// the behaviour of the comparer, so an equal but distinct instance rebuilds the sorted set (ADR 0001, D14).
    /// </para>
    /// </remarks>
    [<CompiledName "OfSeq">]
    member ofSeq : elements : seq<'T> -> SortedSet<'T>

    /// <summary>
    /// Creates a sorted set from the elements of a span, ordered by the comparer.
    /// </summary>
    /// <param name="span">The input span.</param>
    /// <returns>
    /// The sorted set of the elements that are distinct in the comparer; of the elements that the comparer considers equal,
    /// it holds the first one. An empty span gives the empty sorted set.
    /// </returns>
    /// <remarks>
    /// O(n log n). Later changes to the memory under the span do not change the sorted set. The member converts the span
    /// to a read-only span and does what
    /// <see cref="M:FSharp.Collections.Immutable.CustomComparerSortedSet`1.OfReadOnlySpan(System.ReadOnlySpan{`0})"/>
    /// does.
    /// </remarks>
    [<CompiledName "OfSpan">]
    member ofSpan : span : Span<'T> -> SortedSet<'T>

    /// <summary>
    /// Creates a sorted set of one element that orders with the comparer.
    /// </summary>
    /// <param name="value">The element.</param>
    /// <returns>The sorted set of one element.</returns>
    /// <remarks>
    /// O(1). The member wraps
    /// <see cref="M:System.Collections.Immutable.ImmutableSortedSet.Create``1(System.Collections.Generic.IComparer{``0},``0)"/>.
    /// </remarks>
    [<CompiledName "Singleton">]
    member singleton : value : 'T -> SortedSet<'T>

    // ---- Builder ----

    /// <summary>
    /// Creates a sorted set with a builder that an action fills.
    /// </summary>
    /// <param name="action">The action that adds the elements to the builder, which orders with the comparer.</param>
    /// <returns>The sorted set of the elements that the action added.</returns>
    /// <remarks>
    /// O(1) apart from the cost of the action. The member wraps
    /// <see cref="M:System.Collections.Immutable.ImmutableSortedSet.CreateBuilder``1(System.Collections.Generic.IComparer{``0})"/>
    /// and <see cref="M:System.Collections.Immutable.ImmutableSortedSet`1.Builder.ToImmutable"/>.
    /// </remarks>
    [<CompiledName "Build">]
    member build : action : (SortedSet<'T>.Builder -> unit) -> SortedSet<'T>

    /// <summary>
    /// Creates an empty builder that orders with the comparer.
    /// </summary>
    /// <returns>The builder.</returns>
    /// <remarks>
    /// O(1). The member wraps
    /// <see cref="M:System.Collections.Immutable.ImmutableSortedSet.CreateBuilder``1(System.Collections.Generic.IComparer{``0})"/>.
    /// </remarks>
    [<CompiledName "CreateBuilder">]
    member builder : unit -> SortedSet<'T>.Builder

    // ---- Properties ----

    /// <summary>
    /// The comparer that orders the elements of the sorted sets that the struct creates.
    /// </summary>
    /// <remarks>
    /// O(1). The default value of the struct returns <see cref="P:System.Collections.Generic.Comparer`1.Default"/>, as the
    /// BCL does for a <see langword="null"/> comparer, and not F# generic comparison: the struct serves element types without
    /// F# comparison too.
    /// </remarks>
    member Comparer : IComparer<'T>
