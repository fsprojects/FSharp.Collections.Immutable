[<AutoOpen>]
module FSharp.Collections.Immutable.ComputationExpressions

// ---- Creation ----

let flatList = FlatListCE ()

[<GeneralizableValue>]
let hashMap<'Key when 'Key : not null> = HashMapCE<'Key>()

[<GeneralizableValue>]
let sortedMap<'Key when 'Key : not null and 'Key : comparison> = SortedMapCE<'Key>()
