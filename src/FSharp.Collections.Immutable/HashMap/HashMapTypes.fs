namespace FSharp.Collections.Immutable

open System.Collections.Immutable

// The abbreviations have their own file because the modules of the folder are root modules, which cannot declare a type
// of the namespace; a file of abbreviations needs no signature file (ADR 0001, D2).

/// <summary>
/// An immutable hash map, <see cref="T:System.Collections.Immutable.ImmutableDictionary`2"/>.
/// </summary>
/// <remarks>
/// The map finds a key by its hash code in an equality comparer. It enumerates its bindings in an order that depends on the
/// hash codes: the order stays the same for the same map, but it is not the order of the keys.
/// </remarks>
type HashMap<'Key, 'Value when 'Key : not null> = ImmutableDictionary<'Key, 'Value>

/// <summary>
/// An immutable map of any implementation, <see cref="T:System.Collections.Immutable.IImmutableDictionary`2"/>.
/// </summary>
/// <remarks>
/// <see cref="T:FSharp.Collections.Immutable.HashMap`2"/> and <see cref="T:System.Collections.Immutable.ImmutableSortedDictionary`2"/>
/// implement the interface.
/// </remarks>
type IMap<'Key, 'Value> = IImmutableDictionary<'Key, 'Value>
