namespace FSharp.Collections.Immutable

open System.Collections.Immutable

// The abbreviation has its own file because the module of the folder is a root module, which cannot declare a type of
// the namespace; a file of abbreviations needs no signature file (ADR 0001, D2).

/// <summary>
/// An immutable hash set, <see cref="T:System.Collections.Immutable.ImmutableHashSet`1"/>.
/// </summary>
/// <remarks>
/// A hash set compares its elements with an equality comparer: <see cref="P:System.Collections.Generic.EqualityComparer`1.Default"/>,
/// unless a <see cref="T:FSharp.Collections.Immutable.CustomComparerHashSet`1"/> created it with another one.
/// </remarks>
type HashSet<'T> = ImmutableHashSet<'T>
