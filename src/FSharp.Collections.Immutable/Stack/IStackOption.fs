[<RequireQualifiedAccess>]
module FSharp.Collections.Immutable.Option.IStack

open FSharp.Collections.Immutable

// The functions delegate to the default module and convert its results, or the results of the callbacks that they pass to
// it, so that both modules have the same checks and exceptions.

// ---- Access ----

[<CompiledName "TryExactlyOne">]
let tryExactlyOne (stack : IStack<'T>) =
    FSharp.Collections.Immutable.IStack.tryExactlyOne stack
    |> ValueOption.toOption

[<CompiledName "TryPeek">]
let tryHead (stack : IStack<'T>) =
    FSharp.Collections.Immutable.IStack.tryHead stack
    |> ValueOption.toOption

[<CompiledName "TryLast">]
let tryLast (stack : IStack<'T>) =
    FSharp.Collections.Immutable.IStack.tryLast stack
    |> ValueOption.toOption

[<CompiledName "TryPop">]
let tryTail (stack : IStack<'T>) =
    FSharp.Collections.Immutable.IStack.tryTail stack
    |> ValueOption.toOption

[<CompiledName "TryPeekPop">]
let tryUncons (stack : IStack<'T>) =
    match FSharp.Collections.Immutable.IStack.tryUncons stack with
    | ValueSome (struct (head, tail)) -> Some (head, tail)
    | ValueNone -> None

[<CompiledName "ConsPattern">]
let (|Cons|_|) (stack : IStack<'T>) = tryUncons stack

// ---- Search ----

[<CompiledName "Pick">]
let pick (chooser : 'T -> 'U option) (stack : IStack<'T>) =
    FSharp.Collections.Immutable.IStack.pick (fun item -> chooser item |> ValueOption.ofOption) stack

[<CompiledName "TryFind">]
let tryFind predicate (stack : IStack<'T>) =
    FSharp.Collections.Immutable.IStack.tryFind predicate stack
    |> ValueOption.toOption

[<CompiledName "TryFindBack">]
let tryFindBack predicate (stack : IStack<'T>) =
    FSharp.Collections.Immutable.IStack.tryFindBack predicate stack
    |> ValueOption.toOption

[<CompiledName "TryFindIndex">]
let tryFindIndex predicate (stack : IStack<'T>) =
    FSharp.Collections.Immutable.IStack.tryFindIndex predicate stack
    |> ValueOption.toOption

[<CompiledName "TryFindIndexBack">]
let tryFindIndexBack predicate (stack : IStack<'T>) =
    FSharp.Collections.Immutable.IStack.tryFindIndexBack predicate stack
    |> ValueOption.toOption

[<CompiledName "TryPick">]
let tryPick (chooser : 'T -> 'U option) (stack : IStack<'T>) =
    FSharp.Collections.Immutable.IStack.tryPick (fun item -> chooser item |> ValueOption.ofOption) stack
    |> ValueOption.toOption

// ---- Transformation ----

[<CompiledName "Choose">]
let choose (chooser : 'T -> 'U option) (stack : IStack<'T>) =
    FSharp.Collections.Immutable.IStack.choose (fun item -> chooser item |> ValueOption.ofOption) stack
