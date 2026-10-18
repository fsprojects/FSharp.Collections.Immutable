[<RequireQualifiedAccess>]
module FSharp.Collections.Immutable.Option.SortedSet

open FSharp.Collections.Immutable

// The functions delegate to the default module and convert between options and value options.

// ---- Access ----

[<CompiledName "TryGetValue">]
let tryGetValue (value : 'T) (set : SortedSet<'T>) =
    FSharp.Collections.Immutable.SortedSet.tryGetValue value set
    |> ValueOption.toOption

[<CompiledName "TryItem">]
let tryItem (index : int) (set : SortedSet<'T>) =
    FSharp.Collections.Immutable.SortedSet.tryItem index set
    |> ValueOption.toOption

// ---- Transformation ----

[<CompiledName "Choose">]
let choose (chooser : 'T -> 'U option) (set : SortedSet<'T>) =
    FSharp.Collections.Immutable.SortedSet.choose (fun item -> chooser item |> ValueOption.ofOption) set
