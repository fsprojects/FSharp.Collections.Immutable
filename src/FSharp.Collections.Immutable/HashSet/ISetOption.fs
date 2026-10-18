[<RequireQualifiedAccess>]
module FSharp.Collections.Immutable.Option.ISet

open System.Collections.Immutable
open FSharp.Collections.Immutable

// The functions delegate to the default module and convert between options and value options.

// ---- Access ----

[<CompiledName "TryGetValue">]
let tryGetValue (value : 'T) (set : IImmutableSet<'T>) =
    FSharp.Collections.Immutable.ISet.tryGetValue value set
    |> ValueOption.toOption

// ---- Transformation ----

[<CompiledName "Choose">]
let choose (chooser : 'T -> 'U option) (set : IImmutableSet<'T>) =
    FSharp.Collections.Immutable.ISet.choose (fun item -> chooser item |> ValueOption.ofOption) set
