[<RequireQualifiedAccess>]
module FSharp.Collections.Immutable.Option.IQueue

open FSharp.Collections.Immutable

// The functions delegate to the default module and convert its results, or the results of the callbacks that they pass to
// it, so that both modules have the same checks and exceptions.

// ---- Access ----

[<CompiledName "TryExactlyOne">]
let tryExactlyOne (queue : IQueue<'T>) =
    FSharp.Collections.Immutable.IQueue.tryExactlyOne queue
    |> ValueOption.toOption

[<CompiledName "TryPeek">]
let tryHead (queue : IQueue<'T>) =
    FSharp.Collections.Immutable.IQueue.tryHead queue
    |> ValueOption.toOption

[<CompiledName "TryLast">]
let tryLast (queue : IQueue<'T>) =
    FSharp.Collections.Immutable.IQueue.tryLast queue
    |> ValueOption.toOption

[<CompiledName "TryDequeue">]
let tryTail (queue : IQueue<'T>) =
    FSharp.Collections.Immutable.IQueue.tryTail queue
    |> ValueOption.toOption

[<CompiledName "TryPeekDequeue">]
let tryUncons (queue : IQueue<'T>) =
    match FSharp.Collections.Immutable.IQueue.tryUncons queue with
    | ValueSome (struct (head, tail)) -> Some (head, tail)
    | ValueNone -> None

[<CompiledName "ConsPattern">]
let (|Cons|_|) (queue : IQueue<'T>) = tryUncons queue

// ---- Search ----

[<CompiledName "Pick">]
let pick (chooser : 'T -> 'U option) (queue : IQueue<'T>) =
    FSharp.Collections.Immutable.IQueue.pick (fun item -> chooser item |> ValueOption.ofOption) queue

[<CompiledName "TryFind">]
let tryFind predicate (queue : IQueue<'T>) =
    FSharp.Collections.Immutable.IQueue.tryFind predicate queue
    |> ValueOption.toOption

[<CompiledName "TryFindBack">]
let tryFindBack predicate (queue : IQueue<'T>) =
    FSharp.Collections.Immutable.IQueue.tryFindBack predicate queue
    |> ValueOption.toOption

[<CompiledName "TryFindIndex">]
let tryFindIndex predicate (queue : IQueue<'T>) =
    FSharp.Collections.Immutable.IQueue.tryFindIndex predicate queue
    |> ValueOption.toOption

[<CompiledName "TryFindIndexBack">]
let tryFindIndexBack predicate (queue : IQueue<'T>) =
    FSharp.Collections.Immutable.IQueue.tryFindIndexBack predicate queue
    |> ValueOption.toOption

[<CompiledName "TryPick">]
let tryPick (chooser : 'T -> 'U option) (queue : IQueue<'T>) =
    FSharp.Collections.Immutable.IQueue.tryPick (fun item -> chooser item |> ValueOption.ofOption) queue
    |> ValueOption.toOption

// ---- Transformation ----

[<CompiledName "Choose">]
let choose (chooser : 'T -> 'U option) (queue : IQueue<'T>) =
    FSharp.Collections.Immutable.IQueue.choose (fun item -> chooser item |> ValueOption.ofOption) queue
