[<RequireQualifiedAccess>]
module FSharp.Collections.Immutable.Tuple.SortedMap

open FSharp.Collections.Immutable

// The conversions share their algorithm with the default module and only pass another tuple constructor; `maxKeyValue`,
// `minKeyValue` and `partition` delegate to the default module and repack its pair.

// ---- Access ----

[<CompiledName "MaxKeyValue">]
let maxKeyValue (table : SortedMap<'Key, 'Value>) =
    let struct (key, value) = FSharp.Collections.Immutable.SortedMap.maxKeyValue table
    key, value

[<CompiledName "MinKeyValue">]
let minKeyValue (table : SortedMap<'Key, 'Value>) =
    let struct (key, value) = FSharp.Collections.Immutable.SortedMap.minKeyValue table
    key, value

// ---- Partitioning ----

[<CompiledName "Partition">]
let partition predicate (table : SortedMap<'Key, 'Value>) =
    let struct (accepted, rejected) = FSharp.Collections.Immutable.SortedMap.partition predicate table
    accepted, rejected

// ---- Conversion ----

[<CompiledName "ToArray">]
let toArray (table : SortedMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    Dictionaries.toArray referencePair table

[<CompiledName "ToList">]
let toList (table : SortedMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    Dictionaries.toList referencePair table

[<CompiledName "ToSeq">]
let toSeq (table : SortedMap<'Key, 'Value>) =
    checkNotNull (nameof table) table
    Dictionaries.toSeq referencePair table
