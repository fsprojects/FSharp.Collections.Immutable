[<RequireQualifiedAccess>]
module FSharp.Collections.Immutable.Option.Stack

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

    Stack.unfold generateStruct state

// ---- Access ----

[<CompiledName "TryExactlyOne">]
let tryExactlyOne (stack : Stack<'T>) = Stack.tryExactlyOne stack |> ValueOption.toOption

[<CompiledName "TryPeek">]
let tryHead (stack : Stack<'T>) = Stack.tryHead stack |> ValueOption.toOption

[<CompiledName "TryLast">]
let tryLast (stack : Stack<'T>) = Stack.tryLast stack |> ValueOption.toOption

[<CompiledName "TryPop">]
let tryTail (stack : Stack<'T>) = Stack.tryTail stack |> ValueOption.toOption

[<CompiledName "TryPeekPop">]
let tryUncons (stack : Stack<'T>) =
    match Stack.tryUncons stack with
    | ValueSome (struct (head, tail)) -> Some (head, tail)
    | ValueNone -> None

[<CompiledName "ConsPattern">]
let (|Cons|_|) (stack : Stack<'T>) = tryUncons stack

// ---- Search ----

[<CompiledName "Pick">]
let pick (chooser : 'T -> 'U option) (stack : Stack<'T>) = Stack.pick (fun item -> chooser item |> ValueOption.ofOption) stack

[<CompiledName "TryFind">]
let tryFind predicate (stack : Stack<'T>) = Stack.tryFind predicate stack |> ValueOption.toOption

[<CompiledName "TryFindBack">]
let tryFindBack predicate (stack : Stack<'T>) = Stack.tryFindBack predicate stack |> ValueOption.toOption

[<CompiledName "TryFindIndex">]
let tryFindIndex predicate (stack : Stack<'T>) = Stack.tryFindIndex predicate stack |> ValueOption.toOption

[<CompiledName "TryFindIndexBack">]
let tryFindIndexBack predicate (stack : Stack<'T>) =
    Stack.tryFindIndexBack predicate stack
    |> ValueOption.toOption

[<CompiledName "TryPick">]
let tryPick (chooser : 'T -> 'U option) (stack : Stack<'T>) =
    Stack.tryPick (fun item -> chooser item |> ValueOption.ofOption) stack
    |> ValueOption.toOption

// ---- Transformation ----

[<CompiledName "Choose">]
let choose (chooser : 'T -> 'U option) (stack : Stack<'T>) = Stack.choose (fun item -> chooser item |> ValueOption.ofOption) stack
