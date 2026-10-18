namespace FSharp.Collections.Immutable

open System
open System.Collections.Generic
// Opened after System.Collections.Generic, so that HashSet names the abbreviation of this library, not the mutable set
open FSharp.Collections.Immutable

/// <summary>
/// Creates hash sets that compare their elements with an equality comparer (ADR 0001, D12).
/// </summary>
/// <remarks>
/// <para>
/// <see cref="M:FSharp.Collections.Immutable.HashSet.ForComparer``1(System.Collections.Generic.IEqualityComparer{``0})"/>
/// creates the struct. Its members are the creation functions of <see cref="T:FSharp.Collections.Immutable.HashSet"/>
/// under the same names, with the comparer applied, so that a call reads as one phrase.
/// </para>
/// <para>
/// The struct holds only the comparer, so creating it allocates nothing. Its default value, which no constructor
/// creates, compares with <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>.
/// </para>
/// </remarks>
/// <example id="customComparerHashSet-1">
/// <code lang="fsharp">
/// let names = (HashSet.forComparer StringComparer.OrdinalIgnoreCase).ofList [ "Ann"; "ANN"; "Bob" ]
/// </code>
/// Evaluates to a hash set of "Ann" and "Bob" that compares without case.
/// </example>
[<Struct; NoComparison; NoEquality>]
type CustomComparerHashSet<'T> =

    // A signature reveals the fields of a struct, private ones included (FS0314); null only in the default value
    val private comparer : IEqualityComparer<'T> | null

    /// <summary>Creates the struct for an equality comparer.</summary>
    /// <param name="comparer">The equality comparer of the elements of the hash sets that the struct creates.</param>
    /// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="comparer"/> is <see langword="null"/>.</exception>
    new : comparer : IEqualityComparer<'T> -> CustomComparerHashSet<'T>

    // ---- Creation ----

    /// <summary>
    /// The empty hash set that compares with the comparer.
    /// </summary>
    /// <remarks>
    /// O(1). The member wraps
    /// <see cref="M:System.Collections.Immutable.ImmutableHashSet.Create``1(System.Collections.Generic.IEqualityComparer{``0})"/>.
    /// </remarks>
    [<CompiledName "Empty">]
    member empty : HashSet<'T>

    /// <summary>
    /// Creates a hash set from the elements of an array, compared with the comparer.
    /// </summary>
    /// <param name="array">The input array.</param>
    /// <returns>
    /// The hash set of the elements that are distinct in the comparer; of the elements that the comparer considers equal,
    /// it holds the first one.
    /// </returns>
    /// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="array"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// O(n log n). Later changes to the array do not change the hash set. The member wraps
    /// <see cref="M:System.Collections.Immutable.ImmutableHashSet.CreateRange``1(System.Collections.Generic.IEqualityComparer{``0},System.Collections.Generic.IEnumerable{``0})"/>.
    /// </remarks>
    [<CompiledName "OfArray">]
    member ofArray : array : 'T array -> HashSet<'T>

    /// <summary>
    /// Creates a hash set from the elements of a list, compared with the comparer.
    /// </summary>
    /// <param name="elements">The input list.</param>
    /// <returns>
    /// The hash set of the elements that are distinct in the comparer; of the elements that the comparer considers equal,
    /// it holds the first one.
    /// </returns>
    /// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="elements"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// O(n log n).
    /// </remarks>
    [<CompiledName "OfList">]
    member ofList : elements : 'T list -> HashSet<'T>

    /// <summary>
    /// Creates a hash set from the elements of a read-only span, compared with the comparer.
    /// </summary>
    /// <param name="span">The input span.</param>
    /// <returns>
    /// The hash set of the elements that are distinct in the comparer; of the elements that the comparer considers equal,
    /// it holds the first one. An empty span gives the empty hash set.
    /// </returns>
    /// <remarks>
    /// O(n log n). Later changes to the memory under the span do not change the hash set. The member wraps
    /// <see cref="M:System.Collections.Immutable.ImmutableHashSet.Create``1(System.Collections.Generic.IEqualityComparer{``0},System.ReadOnlySpan{``0})"/>.
    /// </remarks>
    [<CompiledName "OfReadOnlySpan">]
    member ofReadOnlySpan : span : ReadOnlySpan<'T> -> HashSet<'T>

    /// <summary>
    /// Creates a hash set from the elements of a sequence, compared with the comparer.
    /// </summary>
    /// <param name="elements">The input sequence, which can be a hash set with another comparer.</param>
    /// <returns>
    /// The hash set of the elements that are distinct in the comparer; of the elements that the comparer considers equal,
    /// it holds the first one.
    /// </returns>
    /// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="elements"/> is <see langword="null"/> or a default flat list.</exception>
    /// <remarks>
    /// <para>
    /// O(n log n). The member wraps
    /// <see cref="M:System.Collections.Immutable.ImmutableHashSet.CreateRange``1(System.Collections.Generic.IEqualityComparer{``0},System.Collections.Generic.IEnumerable{``0})"/>.
    /// </para>
    /// <para>
    /// A hash set gets the comparer, as
    /// <see cref="M:System.Collections.Immutable.ImmutableHashSet`1.WithComparer(System.Collections.Generic.IEqualityComparer{`0})"/>
    /// gives it: the member returns the hash set itself in O(1) when its comparer is the same instance as the comparer of
    /// the struct, and otherwise a new hash set of its elements in the comparer. A comparer that is equal to the comparer
    /// of the struct but is another instance does not count (ADR 0001, D14).
    /// </para>
    /// </remarks>
    [<CompiledName "OfSeq">]
    member ofSeq : elements : seq<'T> -> HashSet<'T>

    /// <summary>
    /// Creates a hash set from the elements of a span, compared with the comparer.
    /// </summary>
    /// <param name="span">The input span.</param>
    /// <returns>
    /// The hash set of the elements that are distinct in the comparer; of the elements that the comparer considers equal,
    /// it holds the first one. An empty span gives the empty hash set.
    /// </returns>
    /// <remarks>
    /// O(n log n). Later changes to the memory under the span do not change the hash set. The member converts the span to a
    /// read-only span and wraps
    /// <see cref="M:System.Collections.Immutable.ImmutableHashSet.Create``1(System.Collections.Generic.IEqualityComparer{``0},System.ReadOnlySpan{``0})"/>.
    /// </remarks>
    [<CompiledName "OfSpan">]
    member ofSpan : span : Span<'T> -> HashSet<'T>

    /// <summary>
    /// Creates a hash set of one element that compares with the comparer.
    /// </summary>
    /// <param name="value">The element.</param>
    /// <returns>The hash set of one element.</returns>
    /// <remarks>
    /// O(1). The member wraps
    /// <see cref="M:System.Collections.Immutable.ImmutableHashSet.Create``1(System.Collections.Generic.IEqualityComparer{``0},``0)"/>.
    /// </remarks>
    [<CompiledName "Singleton">]
    member singleton : value : 'T -> HashSet<'T>

    // ---- Builder ----

    /// <summary>
    /// Creates a hash set with a builder that an action fills, compared with the comparer.
    /// </summary>
    /// <param name="action">The action that adds the elements to the builder, which compares with the comparer.</param>
    /// <returns>The hash set of the elements that the action added.</returns>
    /// <remarks>
    /// O(1) apart from the cost of the action. The member wraps
    /// <see cref="M:System.Collections.Immutable.ImmutableHashSet.CreateBuilder``1(System.Collections.Generic.IEqualityComparer{``0})"/>
    /// and <see cref="M:System.Collections.Immutable.ImmutableHashSet`1.Builder.ToImmutable"/>.
    /// </remarks>
    [<CompiledName "Build">]
    member build : action : (HashSet<'T>.Builder -> unit) -> HashSet<'T>

    /// <summary>
    /// Creates an empty builder that compares with the comparer.
    /// </summary>
    /// <returns>The builder.</returns>
    /// <remarks>
    /// O(1). The member wraps
    /// <see cref="M:System.Collections.Immutable.ImmutableHashSet.CreateBuilder``1(System.Collections.Generic.IEqualityComparer{``0})"/>.
    /// </remarks>
    [<CompiledName "CreateBuilder">]
    member builder : unit -> HashSet<'T>.Builder

    // ---- Properties ----

    /// <summary>
    /// The equality comparer of the elements of the hash sets that the struct creates.
    /// </summary>
    /// <remarks>
    /// O(1). The default value of the struct returns <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>.
    /// </remarks>
    member Comparer : IEqualityComparer<'T>
