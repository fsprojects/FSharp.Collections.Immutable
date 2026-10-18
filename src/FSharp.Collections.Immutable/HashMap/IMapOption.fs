[<RequireQualifiedAccess>]
module FSharp.Collections.Immutable.Option.IMap

open FSharp.Collections.Immutable

// The functions delegate to the default module and convert the options at its boundary: the functions that it calls get
// value options, and its results become options.

// ---- Access ----

[<CompiledName "TryFind">]
let tryFind (key : 'Key) (table : IMap<'Key, 'Value>) =
    FSharp.Collections.Immutable.IMap.tryFind key table
    |> ValueOption.toOption

[<CompiledName "TryGetKey">]
let tryGetKey (key : 'Key) (table : IMap<'Key, 'Value>) =
    FSharp.Collections.Immutable.IMap.tryGetKey key table
    |> ValueOption.toOption

// ---- Search ----

[<CompiledName "Pick">]
let pick (chooser : 'Key -> 'Value -> 'U option) (table : IMap<'Key, 'Value>) =
    FSharp.Collections.Immutable.IMap.pick (fun key value -> chooser key value |> ValueOption.ofOption) table

[<CompiledName "TryFindKey">]
let tryFindKey predicate (table : IMap<'Key, 'Value>) =
    FSharp.Collections.Immutable.IMap.tryFindKey predicate table
    |> ValueOption.toOption

[<CompiledName "TryPick">]
let tryPick (chooser : 'Key -> 'Value -> 'U option) (table : IMap<'Key, 'Value>) =
    FSharp.Collections.Immutable.IMap.tryPick (fun key value -> chooser key value |> ValueOption.ofOption) table
    |> ValueOption.toOption

// ---- Transformation ----

[<CompiledName "Change">]
let change (key : 'Key) (f : 'Value option -> 'Value option) (table : IMap<'Key, 'Value>) =
    FSharp.Collections.Immutable.IMap.change key (fun current -> f (ValueOption.toOption current) |> ValueOption.ofOption) table

[<CompiledName "Choose">]
let choose (chooser : 'Key -> 'Value -> 'U option) (table : IMap<'Key, 'Value>) =
    FSharp.Collections.Immutable.IMap.choose (fun key value -> chooser key value |> ValueOption.ofOption) table
