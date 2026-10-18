namespace FSharp.Collections.Immutable

open System.Collections.Immutable

// The abbreviation has its own file because the module of the folder is a root module, which cannot declare a type of
// the namespace; a file of abbreviations needs no signature file (ADR 0001, D2).

/// <summary>An immutable array, <see cref="T:System.Collections.Immutable.ImmutableArray`1"/>.</summary>
/// <remarks>The name comes from a similar construct in the sources of the F# compiler.</remarks>
type FlatList<'T> = ImmutableArray<'T>
