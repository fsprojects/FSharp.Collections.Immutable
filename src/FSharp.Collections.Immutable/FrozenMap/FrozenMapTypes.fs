namespace FSharp.Collections.Immutable

open System.Collections.Frozen

// The abbreviation has its own file because the module of the folder is a root module, which cannot declare a type of
// the namespace; a file of abbreviations needs no signature file (ADR 0001, D2).

/// <summary>
/// An immutable map that is built once and read many times, <see cref="T:System.Collections.Frozen.FrozenDictionary`2"/>.
/// </summary>
/// <remarks>
/// The creation of the map analyses its keys to make the lookups fast, so it costs more than the creation of a hash map. The
/// map enumerates its bindings in an unspecified order, which stays the same for the same map: the order of the arrays of
/// its keys and its values.
/// </remarks>
type FrozenMap<'Key, 'Value when 'Key : not null> = FrozenDictionary<'Key, 'Value>
