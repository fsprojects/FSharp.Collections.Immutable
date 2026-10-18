[<RequireQualifiedAccess>]
module FSharp.Collections.Immutable.Option.HashSet

open FSharp.Collections.Immutable

// The functions delegate to the default module and convert between options and value options.

// ---- Access ----

[<CompiledName "TryGetValue">]
let tryGetValue (value : 'T) (set : HashSet<'T>) =
    FSharp.Collections.Immutable.HashSet.tryGetValue value set
    |> ValueOption.toOption

// ---- Transformation ----

[<CompiledName "Choose">]
let choose (chooser : 'T -> 'U option) (set : HashSet<'T>) =
    FSharp.Collections.Immutable.HashSet.choose (fun item -> chooser item |> ValueOption.ofOption) set
