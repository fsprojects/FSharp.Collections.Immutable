[<RequireQualifiedAccess>]
module FSharp.Collections.Immutable.Option.FrozenSet

open System.Collections.Frozen

// The functions delegate to the default module and convert between options and value options.

// ---- Access ----

[<CompiledName "TryGetValue">]
let tryGetValue (value : 'T) (set : FrozenSet<'T>) =
    FSharp.Collections.Immutable.FrozenSet.tryGetValue value set
    |> ValueOption.toOption

// ---- Transformation ----

[<CompiledName "Choose">]
let choose (chooser : 'T -> 'U option) (set : FrozenSet<'T>) =
    FSharp.Collections.Immutable.FrozenSet.choose (fun item -> chooser item |> ValueOption.ofOption) set

#if NET10_0_OR_GREATER
// ---- Type-specific ----

[<CompiledName "TryAlternateLookup">]
let tryAlternateLookup<'Alternate, 'T> (set : FrozenSet<'T>) =
    FSharp.Collections.Immutable.FrozenSet.tryAlternateLookup<'Alternate, 'T> set
    |> ValueOption.toOption
#endif
