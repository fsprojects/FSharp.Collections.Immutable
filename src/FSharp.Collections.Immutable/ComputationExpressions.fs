[<AutoOpen>]
module FSharp.Collections.Immutable.ComputationExpressions

// ---- Creation ----

let flatList = FlatListCE ()

[<GeneralizableValue>]
let hashSet<'T> = HashSetCE<'T>()
