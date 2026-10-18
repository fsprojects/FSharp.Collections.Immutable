namespace FSharp.Collections.Immutable

open System.Collections.Immutable

// The abbreviations have their own file because the modules of the folder are root modules, which cannot declare a type
// of the namespace; a file of abbreviations needs no signature file (ADR 0001, D2).

/// <summary>
/// An immutable list, <see cref="T:System.Collections.Immutable.ImmutableList`1"/>.
/// </summary>
/// <remarks>
/// The list is a balanced binary tree: reading, inserting, replacing or removing the element at an index takes O(log n).
/// </remarks>
type IndexedList<'T> = ImmutableList<'T>

/// <summary>
/// An immutable list of any implementation, <see cref="T:System.Collections.Immutable.IImmutableList`1"/>.
/// </summary>
/// <remarks>
/// <see cref="T:System.Collections.Immutable.ImmutableList`1"/> and
/// <see cref="T:System.Collections.Immutable.ImmutableArray`1"/> implement the interface.
/// </remarks>
type IIndexedList<'T> = IImmutableList<'T>
