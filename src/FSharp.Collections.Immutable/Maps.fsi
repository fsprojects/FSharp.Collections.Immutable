namespace FSharp.Collections.Immutable

open System.Collections.Generic

type IMap<'Key, 'Value> = System.Collections.Immutable.IImmutableDictionary<'Key, 'Value>

type HashMap<'Key, 'Value when 'Key : not null> = System.Collections.Immutable.ImmutableDictionary<'Key, 'Value>

/// <summary>Functional helpers for immutable hash maps.</summary>
[<RequireQualifiedAccess;
  CompiledName((nameof System.Collections.Immutable.ImmutableDictionary)
               + "Module")>]
module HashMap =

    /// <summary>Returns an empty hash map.</summary>
    val inline empty<'Key, 'Value when 'Key : not null> : HashMap<'Key, 'Value>

    /// <summary>Creates a hash map from a sequence of key/value pairs.</summary>
    val inline ofSeq : source : seq<KeyValuePair<'Key, 'Value>> -> HashMap<'Key, 'Value> when 'Key : not null

    /// <summary>Returns a sequence view of a hash map.</summary>
    val inline toSeq : map : HashMap<'Key, 'Value> -> seq<KeyValuePair<'Key, 'Value>> when 'Key : not null

type SortedMap<'Key, 'Value when 'Key : not null> = System.Collections.Immutable.ImmutableSortedDictionary<'Key, 'Value>

/// <summary>Functional helpers for immutable sorted maps.</summary>
[<RequireQualifiedAccess;
  CompiledName((nameof System.Collections.Immutable.ImmutableSortedDictionary)
               + "Module")>]
module SortedMap =

    /// <summary>Returns an empty sorted map.</summary>
    val inline empty<'Key, 'Value when 'Key : not null> : SortedMap<'Key, 'Value>

    /// <summary>Creates a sorted map from a sequence of key/value pairs.</summary>
    val inline ofSeq : source : seq<KeyValuePair<'Key, 'Value>> -> SortedMap<'Key, 'Value> when 'Key : not null

    /// <summary>Returns a sequence view of a sorted map.</summary>
    val inline toSeq : map : SortedMap<'Key, 'Value> -> seq<KeyValuePair<'Key, 'Value>> when 'Key : not null
