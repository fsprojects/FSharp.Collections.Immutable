namespace FSharp.Collections.Immutable

open System.Collections.Immutable

// The abbreviation has its own file because the module of the folder is a root module, which cannot declare a type of
// the namespace; a file of abbreviations needs no signature file (ADR 0001, D2).

/// <summary>
/// An immutable sorted set, <see cref="T:System.Collections.Immutable.ImmutableSortedSet`1"/>.
/// </summary>
/// <remarks>
/// A sorted set orders its elements with a comparer and enumerates them in ascending order. The comparer is F# generic
/// comparison, <see cref="M:Microsoft.FSharp.Core.LanguagePrimitives.FastGenericComparer``1"/>, as for
/// <see cref="T:Microsoft.FSharp.Collections.FSharpSet`1"/>, unless a
/// <see cref="T:FSharp.Collections.Immutable.CustomComparerSortedSet`1"/> created the sorted set with another one.
/// </remarks>
type SortedSet<'T> = ImmutableSortedSet<'T>
