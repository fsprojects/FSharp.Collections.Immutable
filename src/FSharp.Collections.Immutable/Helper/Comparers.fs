/// The default comparers of the collections (ADR 0001, D5).
[<AutoOpen>]
module internal FSharp.Collections.Immutable.Comparers

open System.Collections.Generic

/// <summary>
/// The comparer of F# generic comparison for a type: the comparer of the sorts, of <c>binarySearch</c> and of the
/// sorted collections that are created without a comparer.
/// </summary>
/// <remarks>
/// <para>
/// The class resolves <see cref="M:Microsoft.FSharp.Core.LanguagePrimitives.FastGenericComparer``1"/> once per type, in
/// generic code, where FSharp.Core returns the comparer that it keeps for the type.
/// </para>
/// <para>
/// A call of <c>FastGenericComparer</c> at a concrete type that is not primitive creates a new comparer each time, and
/// the F# optimizer creates such calls when it inlines a small function of the library into its caller. Collections of
/// one type would then have different comparers, and the BCL could not take its shortcuts for two collections with the
/// same comparer, such as the one of <c>ImmutableSortedSet.Union</c>. A static field cannot be inlined.
/// </para>
/// </remarks>
[<Sealed>]
type DefaultComparer<'T when 'T : comparison> private () =

    static let comparer = LanguagePrimitives.FastGenericComparer<'T>

    /// The comparer of F# generic comparison for the type.
    static member Instance : IComparer<'T> = comparer
