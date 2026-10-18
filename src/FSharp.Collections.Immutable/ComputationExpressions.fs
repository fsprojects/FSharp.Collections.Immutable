[<AutoOpen>]
module FSharp.Collections.Immutable.ComputationExpressions

// ---- Creation ----

let flatList = FlatListCE ()

[<GeneralizableValue>]
let frozenMap<'Key when 'Key : not null> = FrozenMapCE<'Key>()

[<GeneralizableValue>]
let hashMap<'Key when 'Key : not null> = HashMapCE<'Key>()
