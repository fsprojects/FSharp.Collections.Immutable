[<RequireQualifiedAccess>]
module FSharp.Collections.Immutable.Option.IStack

open FSharp.Collections.Immutable

// The functions delegate to the default module and convert its results, or the results of the callbacks that they pass to
// it, so that both modules have the same checks and exceptions.

// ---- Access ----

[<CompiledName "TryExactlyOne">]
let tryExactlyOne (stack : IStack<'T>) = IStack.tryExactlyOne stack |> ValueOption.toOption

[<CompiledName "TryPeek">]
let tryHead (stack : IStack<'T>) = IStack.tryHead stack |> ValueOption.toOption

[<CompiledName "TryLast">]
let tryLast (stack : IStack<'T>) = IStack.tryLast stack |> ValueOption.toOption

[<CompiledName "TryPop">]
let tryTail (stack : IStack<'T>) = IStack.tryTail stack |> ValueOption.toOption

[<CompiledName "TryPeekPop">]
let tryUncons (stack : IStack<'T>) =
    match IStack.tryUncons stack with
    | ValueSome (struct (head, tail)) -> Some (head, tail)
    | ValueNone -> None

[<CompiledName "ConsPattern">]
let (|Cons|_|) (stack : IStack<'T>) = tryUncons stack

// ---- Search ----

[<CompiledName "Pick">]
let pick (chooser : 'T -> 'U option) (stack : IStack<'T>) = IStack.pick (fun item -> chooser item |> ValueOption.ofOption) stack

[<CompiledName "TryFind">]
let tryFind predicate (stack : IStack<'T>) = IStack.tryFind predicate stack |> ValueOption.toOption

[<CompiledName "TryFindBack">]
let tryFindBack predicate (stack : IStack<'T>) = IStack.tryFindBack predicate stack |> ValueOption.toOption

[<CompiledName "TryFindIndex">]
let tryFindIndex predicate (stack : IStack<'T>) = IStack.tryFindIndex predicate stack |> ValueOption.toOption

[<CompiledName "TryFindIndexBack">]
let tryFindIndexBack predicate (stack : IStack<'T>) =
    IStack.tryFindIndexBack predicate stack
    |> ValueOption.toOption

[<CompiledName "TryPick">]
let tryPick (chooser : 'T -> 'U option) (stack : IStack<'T>) =
    IStack.tryPick (fun item -> chooser item |> ValueOption.ofOption) stack
    |> ValueOption.toOption

// ---- Transformation ----

[<CompiledName "Choose">]
let choose (chooser : 'T -> 'U option) (stack : IStack<'T>) =
    IStack.choose (fun item -> chooser item |> ValueOption.ofOption) stack
