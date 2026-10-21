[<RequireQualifiedAccess>]
module FSharp.Collections.Immutable.Tuple.HashMap

open FSharp.Collections.Immutable

// The conversions share their algorithm with the default module and only pass another tuple constructor; `partition`
// delegates to the default module and repacks its pair.

// ---- Partitioning ----

[<CompiledName "Partition">]
let partition predicate (table : HashMap<'Key, 'Value>) =
    let struct (accepted, rejected) = HashMap.partition predicate table
    accepted, rejected

// ---- Conversion ----

[<CompiledName "ToArray">]
let toArray (table : HashMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    Dictionaries.toArray referencePair table

[<CompiledName "ToList">]
let toList (table : HashMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    Dictionaries.toList referencePair table

[<CompiledName "ToSeq">]
let toSeq (table : HashMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    Dictionaries.toSeq referencePair table
