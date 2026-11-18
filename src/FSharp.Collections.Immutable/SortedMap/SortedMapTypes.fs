namespace FSharp.Collections.Immutable

open System.Collections.Immutable

// The abbreviation has its own file because the modules of the folder are root modules, which cannot declare a type of
// the namespace; a file of abbreviations needs no signature file (ADR 0001, D2).

/// <summary>
/// An immutable sorted map, <see cref="T:System.Collections.Immutable.ImmutableSortedDictionary`2"/>.
/// </summary>
/// <remarks>
/// The map keeps its bindings in a balanced binary tree that a key comparer orders, and enumerates them in the ascending
/// order of the keys.
/// </remarks>
type SortedMap<'Key, 'Value when 'Key : not null> = ImmutableSortedDictionary<'Key, 'Value>
