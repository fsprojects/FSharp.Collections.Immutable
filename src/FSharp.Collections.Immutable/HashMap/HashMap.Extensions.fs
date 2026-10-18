[<AutoOpen>]
module FSharp.Collections.Immutable.HashMapExtensions

open System.Collections.Generic
open System.Collections.Immutable

// The members are the members of CustomComparersHashMap with the default comparers, so that every shape of the bindings
// has one implementation (ADR 0001, D13).

/// The comparer struct with the default comparers of the keys and the values (D5).
let private defaultComparers<'Key, 'Value when 'Key : not null> () =
    CustomComparersHashMap<'Key, 'Value> EqualityComparer<'Key>.Default

// The extended type repeats the constraint of the BCL type (FS0957)
type ImmutableDictionary<'Key, 'Value when 'Key : not null> with

    [<CompiledName "OfArray">]
    static member ofArray (elements : KeyValuePair<'Key, 'Value> array) = (defaultComparers ()).ofArray elements

    [<CompiledName "OfArray">]
    static member ofArray (elements : ('Key * 'Value) array) = (defaultComparers ()).ofArray elements

    [<CompiledName "OfArray">]
    static member ofArray (elements : struct ('Key * 'Value) array) = (defaultComparers ()).ofArray elements

    [<CompiledName "OfList">]
    static member ofList (elements : KeyValuePair<'Key, 'Value> list) = (defaultComparers ()).ofList elements

    [<CompiledName "OfList">]
    static member ofList (elements : ('Key * 'Value) list) = (defaultComparers ()).ofList elements

    [<CompiledName "OfList">]
    static member ofList (elements : struct ('Key * 'Value) list) = (defaultComparers ()).ofList elements

    [<CompiledName "OfSeq">]
    static member ofSeq (elements : seq<KeyValuePair<'Key, 'Value>>) = (defaultComparers ()).ofSeq elements

    [<CompiledName "OfSeq">]
    static member ofSeq (elements : seq<'Key * 'Value>) = (defaultComparers ()).ofSeq elements

    [<CompiledName "OfSeq">]
    static member ofSeq (elements : seq<struct ('Key * 'Value)>) = (defaultComparers ()).ofSeq elements
