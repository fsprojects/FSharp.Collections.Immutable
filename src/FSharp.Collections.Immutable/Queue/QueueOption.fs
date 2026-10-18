[<RequireQualifiedAccess>]
module FSharp.Collections.Immutable.Option.Queue

open FSharp.Collections.Immutable

// The functions delegate to the default module and convert its results, or the results of the callbacks that they pass to
// it, so that both modules have the same checks and exceptions.

// ---- Creation ----

[<CompiledName "Unfold">]
let unfold (generator : 'State -> ('T * 'State) option) (state : 'State) =
    let generateStruct state =
        match generator state with
        | Some (item, next) -> ValueSome (struct (item, next))
        | None -> ValueNone

    FSharp.Collections.Immutable.Queue.unfold generateStruct state

// ---- Access ----

[<CompiledName "TryExactlyOne">]
let tryExactlyOne (queue : Queue<'T>) =
    FSharp.Collections.Immutable.Queue.tryExactlyOne queue
    |> ValueOption.toOption

[<CompiledName "TryPeek">]
let tryHead (queue : Queue<'T>) =
    FSharp.Collections.Immutable.Queue.tryHead queue
    |> ValueOption.toOption

[<CompiledName "TryLast">]
let tryLast (queue : Queue<'T>) =
    FSharp.Collections.Immutable.Queue.tryLast queue
    |> ValueOption.toOption

[<CompiledName "TryDequeue">]
let tryTail (queue : Queue<'T>) =
    FSharp.Collections.Immutable.Queue.tryTail queue
    |> ValueOption.toOption

[<CompiledName "TryPeekDequeue">]
let tryUncons (queue : Queue<'T>) =
    match FSharp.Collections.Immutable.Queue.tryUncons queue with
    | ValueSome (struct (head, tail)) -> Some (head, tail)
    | ValueNone -> None

[<CompiledName "ConsPattern">]
let (|Cons|_|) (queue : Queue<'T>) = tryUncons queue

// ---- Search ----

[<CompiledName "Pick">]
let pick (chooser : 'T -> 'U option) (queue : Queue<'T>) =
    FSharp.Collections.Immutable.Queue.pick (fun item -> chooser item |> ValueOption.ofOption) queue

[<CompiledName "TryFind">]
let tryFind predicate (queue : Queue<'T>) =
    FSharp.Collections.Immutable.Queue.tryFind predicate queue
    |> ValueOption.toOption

[<CompiledName "TryFindBack">]
let tryFindBack predicate (queue : Queue<'T>) =
    FSharp.Collections.Immutable.Queue.tryFindBack predicate queue
    |> ValueOption.toOption

[<CompiledName "TryFindIndex">]
let tryFindIndex predicate (queue : Queue<'T>) =
    FSharp.Collections.Immutable.Queue.tryFindIndex predicate queue
    |> ValueOption.toOption

[<CompiledName "TryFindIndexBack">]
let tryFindIndexBack predicate (queue : Queue<'T>) =
    FSharp.Collections.Immutable.Queue.tryFindIndexBack predicate queue
    |> ValueOption.toOption

[<CompiledName "TryPick">]
let tryPick (chooser : 'T -> 'U option) (queue : Queue<'T>) =
    FSharp.Collections.Immutable.Queue.tryPick (fun item -> chooser item |> ValueOption.ofOption) queue
    |> ValueOption.toOption

// ---- Transformation ----

[<CompiledName "Choose">]
let choose (chooser : 'T -> 'U option) (queue : Queue<'T>) =
    FSharp.Collections.Immutable.Queue.choose (fun item -> chooser item |> ValueOption.ofOption) queue
