[<RequireQualifiedAccess>]
module FSharp.Collections.Immutable.Option.SortedMap

open FSharp.Collections.Immutable

// FSharp.Core has no function over an immutable dictionary, so the functions delegate to the default module and convert
// the options at its boundary: the functions that it calls get value options, and its results become options.

// ---- Access ----

[<CompiledName "TryFind">]
let tryFind (key : 'Key) (table : SortedMap<'Key, 'Value>) = SortedMap.tryFind key table |> ValueOption.toOption

[<CompiledName "TryGetKey">]
let tryGetKey (key : 'Key) (table : SortedMap<'Key, 'Value>) = SortedMap.tryGetKey key table |> ValueOption.toOption

// ---- Search ----

[<CompiledName "Pick">]
let pick (chooser : 'Key -> 'Value -> 'U option) (table : SortedMap<'Key, 'Value>) =
    SortedMap.pick (fun key value -> chooser key value |> ValueOption.ofOption) table

[<CompiledName "TryFindKey">]
let tryFindKey predicate (table : SortedMap<'Key, 'Value>) = SortedMap.tryFindKey predicate table |> ValueOption.toOption

[<CompiledName "TryPick">]
let tryPick (chooser : 'Key -> 'Value -> 'U option) (table : SortedMap<'Key, 'Value>) =
    SortedMap.tryPick (fun key value -> chooser key value |> ValueOption.ofOption) table
    |> ValueOption.toOption

// ---- Transformation ----

[<CompiledName "Change">]
let change (key : 'Key) (f : 'Value option -> 'Value option) (table : SortedMap<'Key, 'Value>) =
    SortedMap.change key (fun current -> f (ValueOption.toOption current) |> ValueOption.ofOption) table

[<CompiledName "Choose">]
let choose (chooser : 'Key -> 'Value -> 'U option) (table : SortedMap<'Key, 'Value>) =
    SortedMap.choose (fun key value -> chooser key value |> ValueOption.ofOption) table
