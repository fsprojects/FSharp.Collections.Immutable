[<RequireQualifiedAccess>]
module FSharp.Collections.Immutable.Option.FrozenMap

open FSharp.Collections.Immutable

// FSharp.Core has no function over a frozen dictionary, so the functions delegate to the default module and convert the
// options at its boundary: the functions that it calls get value options, and its results become options.

// ---- Access ----

[<CompiledName "TryFind">]
let tryFind (key : 'Key) (table : FrozenMap<'Key, 'Value>) =
    FSharp.Collections.Immutable.FrozenMap.tryFind key table
    |> ValueOption.toOption

// ---- Search ----

[<CompiledName "Pick">]
let pick (chooser : 'Key -> 'Value -> 'U option) (table : FrozenMap<'Key, 'Value>) =
    FSharp.Collections.Immutable.FrozenMap.pick (fun key value -> chooser key value |> ValueOption.ofOption) table

[<CompiledName "TryFindKey">]
let tryFindKey predicate (table : FrozenMap<'Key, 'Value>) =
    FSharp.Collections.Immutable.FrozenMap.tryFindKey predicate table
    |> ValueOption.toOption

[<CompiledName "TryPick">]
let tryPick (chooser : 'Key -> 'Value -> 'U option) (table : FrozenMap<'Key, 'Value>) =
    FSharp.Collections.Immutable.FrozenMap.tryPick (fun key value -> chooser key value |> ValueOption.ofOption) table
    |> ValueOption.toOption

// ---- Transformation ----

[<CompiledName "Choose">]
let choose (chooser : 'Key -> 'Value -> 'U option) (table : FrozenMap<'Key, 'Value>) =
    FSharp.Collections.Immutable.FrozenMap.choose (fun key value -> chooser key value |> ValueOption.ofOption) table

#if NET10_0_OR_GREATER
// ---- Type-specific ----

[<CompiledName "TryAlternateLookup">]
let tryAlternateLookup<'Alternate, 'Key, 'Value when 'Key : not null and 'Alternate : not null>
    (table : FrozenMap<'Key, 'Value>)
    =
    FSharp.Collections.Immutable.FrozenMap.tryAlternateLookup<'Alternate, 'Key, 'Value> table
    |> ValueOption.toOption
#endif
