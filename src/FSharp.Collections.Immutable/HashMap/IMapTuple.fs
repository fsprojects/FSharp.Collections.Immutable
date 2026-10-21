[<RequireQualifiedAccess>]
module FSharp.Collections.Immutable.Tuple.IMap

open FSharp.Collections.Immutable

// The conversions share their algorithm with the default module and only pass another tuple constructor; `partition`
// delegates to the default module and repacks its pair.

// ---- Partitioning ----

[<CompiledName "Partition">]
let partition predicate (table : IMap<'Key, 'Value>) =
    let struct (accepted, rejected) = IMap.partition predicate table
    accepted, rejected

// ---- Conversion ----

[<CompiledName "ToArray">]
let toArray (table : IMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    Dictionaries.toArray referencePair table

[<CompiledName "ToList">]
let toList (table : IMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    Dictionaries.toList referencePair table

[<CompiledName "ToSeq">]
let toSeq (table : IMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    Dictionaries.toSeq referencePair table
