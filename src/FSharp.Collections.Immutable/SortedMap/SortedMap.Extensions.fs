[<AutoOpen>]
module FSharp.Collections.Immutable.SortedMapExtensions

open System.Collections.Generic
open System.Collections.Immutable

// The members are the members of CustomComparersSortedMap with the default comparers, so that every shape of the bindings
// has one implementation (ADR 0001, D13). Each member declares the type parameters of the map that it creates, because
// only its own type parameters can require F# comparison of the keys (FS0340).

/// The comparer struct with the default comparers: F# generic comparison of the keys, one cached comparer per type, and the
/// default equality comparer of the values (D5).
let private defaultComparers<'K, 'V when 'K : not null and 'K : comparison> () =
    CustomComparersSortedMap<'K, 'V> DefaultComparer<'K>.Instance

// The extended type repeats the constraint of the BCL type (FS0957)
type ImmutableSortedDictionary<'Key, 'Value when 'Key : not null> with

    [<CompiledName "OfArray">]
    static member ofArray<'K, 'V when 'K : not null and 'K : comparison> (elements : KeyValuePair<'K, 'V> array) =
        (defaultComparers<'K, 'V>()).ofArray elements

    [<CompiledName "OfArray">]
    static member ofArray<'K, 'V when 'K : not null and 'K : comparison> (elements : ('K * 'V) array) =
        (defaultComparers<'K, 'V>()).ofArray elements

    [<CompiledName "OfArray">]
    static member ofArray<'K, 'V when 'K : not null and 'K : comparison> (elements : struct ('K * 'V) array) =
        (defaultComparers<'K, 'V>()).ofArray elements

    [<CompiledName "OfList">]
    static member ofList<'K, 'V when 'K : not null and 'K : comparison> (elements : KeyValuePair<'K, 'V> list) =
        (defaultComparers<'K, 'V>()).ofList elements

    [<CompiledName "OfList">]
    static member ofList<'K, 'V when 'K : not null and 'K : comparison> (elements : ('K * 'V) list) =
        (defaultComparers<'K, 'V>()).ofList elements

    [<CompiledName "OfList">]
    static member ofList<'K, 'V when 'K : not null and 'K : comparison> (elements : struct ('K * 'V) list) =
        (defaultComparers<'K, 'V>()).ofList elements

    [<CompiledName "OfSeq">]
    static member ofSeq<'K, 'V when 'K : not null and 'K : comparison> (elements : seq<KeyValuePair<'K, 'V>>) =
        (defaultComparers<'K, 'V>()).ofSeq elements

    [<CompiledName "OfSeq">]
    static member ofSeq<'K, 'V when 'K : not null and 'K : comparison> (elements : seq<'K * 'V>) =
        (defaultComparers<'K, 'V>()).ofSeq elements

    [<CompiledName "OfSeq">]
    static member ofSeq<'K, 'V when 'K : not null and 'K : comparison> (elements : seq<struct ('K * 'V)>) =
        (defaultComparers<'K, 'V>()).ofSeq elements
