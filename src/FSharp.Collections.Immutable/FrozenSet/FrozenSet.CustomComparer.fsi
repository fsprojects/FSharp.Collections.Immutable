namespace FSharp.Collections.Immutable

open System
open System.Collections.Frozen
open System.Collections.Generic

/// <summary>
/// Creates frozen sets that compare their elements with an equality comparer (ADR 0001, D12).
/// </summary>
/// <remarks>
/// <para>
/// <see cref="M:FSharp.Collections.Immutable.FrozenSet.ForComparer``1(System.Collections.Generic.IEqualityComparer{``0})"/>
/// creates the struct. Its members are the creation functions of <see cref="T:FSharp.Collections.Immutable.FrozenSet"/>
/// under the same names, with the comparer applied, so that a call reads as one phrase. A frozen set has no builder, so
/// the struct has no <c>builder</c> member.
/// </para>
/// <para>
/// The struct holds only the comparer, so creating it allocates nothing. Its default value, which no constructor
/// creates, compares with <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>.
/// </para>
/// </remarks>
/// <example id="customComparerFrozenSet-1">
/// <code lang="fsharp">
/// let names = (FrozenSet.forComparer StringComparer.OrdinalIgnoreCase).ofList [ "Ann"; "ANN"; "Bob" ]
/// </code>
/// Evaluates to a frozen set of "Ann" and "Bob" that compares without case.
/// </example>
[<Struct; NoComparison; NoEquality>]
type CustomComparerFrozenSet<'T> =

    // A signature reveals the fields of a struct, private ones included (FS0314); null only in the default value
    val private comparer : IEqualityComparer<'T> | null

    /// <summary>Creates the struct for an equality comparer.</summary>
    /// <param name="comparer">The equality comparer of the elements of the frozen sets that the struct creates.</param>
    /// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="comparer"/> is <see langword="null"/>.</exception>
    new : comparer : IEqualityComparer<'T> -> CustomComparerFrozenSet<'T>

    // ---- Creation ----

    /// <summary>
    /// The empty frozen set that compares with the comparer.
    /// </summary>
    /// <remarks>
    /// O(1). The member wraps
    /// <see cref="M:System.Collections.Frozen.FrozenSet.Create``1(System.Collections.Generic.IEqualityComparer{``0},System.ReadOnlySpan{``0})"/>,
    /// which returns <see cref="P:System.Collections.Frozen.FrozenSet`1.Empty"/> for
    /// <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>.
    /// </remarks>
    [<CompiledName "Empty">]
    member empty : FrozenSet<'T>

    /// <summary>
    /// Creates a frozen set from the elements of an array, compared with the comparer.
    /// </summary>
    /// <param name="array">The input array.</param>
    /// <returns>
    /// The frozen set of the elements that are distinct in the comparer; of the elements that the comparer considers equal,
    /// it holds the first one.
    /// </returns>
    /// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="array"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// O(n). Later changes to the array do not change the frozen set. The member wraps
    /// <see cref="M:System.Collections.Frozen.FrozenSet.ToFrozenSet``1(System.Collections.Generic.IEnumerable{``0},System.Collections.Generic.IEqualityComparer{``0})"/>.
    /// </remarks>
    [<CompiledName "OfArray">]
    member ofArray : array : 'T array -> FrozenSet<'T>

    /// <summary>
    /// Creates a frozen set from the elements of a list, compared with the comparer.
    /// </summary>
    /// <param name="elements">The input list.</param>
    /// <returns>
    /// The frozen set of the elements that are distinct in the comparer; of the elements that the comparer considers equal,
    /// it holds the first one.
    /// </returns>
    /// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="elements"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// O(n).
    /// </remarks>
    [<CompiledName "OfList">]
    member ofList : elements : 'T list -> FrozenSet<'T>

    /// <summary>
    /// Creates a frozen set from the elements of a read-only span, compared with the comparer.
    /// </summary>
    /// <param name="span">The input span.</param>
    /// <returns>
    /// The frozen set of the elements that are distinct in the comparer; of the elements that the comparer considers equal,
    /// it holds the first one. An empty span gives an empty frozen set.
    /// </returns>
    /// <remarks>
    /// O(n). Later changes to the memory under the span do not change the frozen set. The member wraps
    /// <see cref="M:System.Collections.Frozen.FrozenSet.Create``1(System.Collections.Generic.IEqualityComparer{``0},System.ReadOnlySpan{``0})"/>.
    /// </remarks>
    [<CompiledName "OfReadOnlySpan">]
    member ofReadOnlySpan : span : ReadOnlySpan<'T> -> FrozenSet<'T>

    /// <summary>
    /// Creates a frozen set from the elements of a sequence, compared with the comparer.
    /// </summary>
    /// <param name="elements">The input sequence, which can be a frozen set with another comparer.</param>
    /// <returns>
    /// The frozen set of the elements that are distinct in the comparer; of the elements that the comparer considers equal,
    /// it holds the first one.
    /// </returns>
    /// <exception cref="T:System.ArgumentNullException">Thrown when <paramref name="elements"/> is <see langword="null"/> or a default flat list.</exception>
    /// <remarks>
    /// <para>
    /// O(n). The member wraps
    /// <see cref="M:System.Collections.Frozen.FrozenSet.ToFrozenSet``1(System.Collections.Generic.IEnumerable{``0},System.Collections.Generic.IEqualityComparer{``0})"/>.
    /// </para>
    /// <para>
    /// A frozen set gets the comparer: the member returns the frozen set itself in O(1) when its comparer equals the
    /// comparer, and otherwise a new frozen set of its elements in the comparer.
    /// </para>
    /// </remarks>
    [<CompiledName "OfSeq">]
    member ofSeq : elements : seq<'T> -> FrozenSet<'T>

    /// <summary>
    /// Creates a frozen set from the elements of a span, compared with the comparer.
    /// </summary>
    /// <param name="span">The input span.</param>
    /// <returns>
    /// The frozen set of the elements that are distinct in the comparer; of the elements that the comparer considers equal,
    /// it holds the first one. An empty span gives an empty frozen set.
    /// </returns>
    /// <remarks>
    /// O(n). Later changes to the memory under the span do not change the frozen set. The member converts the span to a
    /// read-only span and wraps
    /// <see cref="M:System.Collections.Frozen.FrozenSet.Create``1(System.Collections.Generic.IEqualityComparer{``0},System.ReadOnlySpan{``0})"/>.
    /// </remarks>
    [<CompiledName "OfSpan">]
    member ofSpan : span : Span<'T> -> FrozenSet<'T>

    /// <summary>
    /// Creates a frozen set of one element that compares with the comparer.
    /// </summary>
    /// <param name="value">The element.</param>
    /// <returns>The frozen set of one element.</returns>
    /// <remarks>
    /// O(1).
    /// </remarks>
    [<CompiledName "Singleton">]
    member singleton : value : 'T -> FrozenSet<'T>

    // ---- Properties ----

    /// <summary>
    /// The equality comparer of the elements of the frozen sets that the struct creates.
    /// </summary>
    /// <remarks>
    /// O(1). The default value of the struct returns <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>.
    /// </remarks>
    member Comparer : IEqualityComparer<'T>
