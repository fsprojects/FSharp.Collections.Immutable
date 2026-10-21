[<RequireQualifiedAccess>]
module FSharp.Collections.Immutable.Option.IIndexedList

open FSharp.Collections.Immutable

// The functions delegate to the default module and convert its results, or the results of the callbacks that they pass to
// it, so that both modules have the same checks and exceptions.

// ---- Access ----

[<CompiledName "TryExactlyOne">]
let tryExactlyOne (list : IIndexedList<'T>) = IIndexedList.tryExactlyOne list |> ValueOption.toOption

[<CompiledName "TryHead">]
let tryHead (list : IIndexedList<'T>) = IIndexedList.tryHead list |> ValueOption.toOption

[<CompiledName "TryItem">]
let tryItem index (list : IIndexedList<'T>) = IIndexedList.tryItem index list |> ValueOption.toOption

[<CompiledName "TryLast">]
let tryLast (list : IIndexedList<'T>) = IIndexedList.tryLast list |> ValueOption.toOption

[<CompiledName "TryTail">]
let tryTail (list : IIndexedList<'T>) = IIndexedList.tryTail list |> ValueOption.toOption

[<CompiledName "TryUncons">]
let tryUncons (list : IIndexedList<'T>) =
    match IIndexedList.tryUncons list with
    | ValueSome (struct (head, tail)) -> Some (head, tail)
    | ValueNone -> None

[<CompiledName "ConsPattern">]
let (|Cons|_|) (list : IIndexedList<'T>) = tryUncons list

// ---- Search ----

[<CompiledName "Pick">]
let pick (chooser : 'T -> 'U option) (list : IIndexedList<'T>) =
    IIndexedList.pick (fun item -> chooser item |> ValueOption.ofOption) list

[<CompiledName "TryFind">]
let tryFind predicate (list : IIndexedList<'T>) = IIndexedList.tryFind predicate list |> ValueOption.toOption

[<CompiledName "TryFindBack">]
let tryFindBack predicate (list : IIndexedList<'T>) =
    IIndexedList.tryFindBack predicate list
    |> ValueOption.toOption

[<CompiledName "TryFindIndex">]
let tryFindIndex predicate (list : IIndexedList<'T>) =
    IIndexedList.tryFindIndex predicate list
    |> ValueOption.toOption

[<CompiledName "TryFindIndexBack">]
let tryFindIndexBack predicate (list : IIndexedList<'T>) =
    IIndexedList.tryFindIndexBack predicate list
    |> ValueOption.toOption

[<CompiledName "TryPick">]
let tryPick (chooser : 'T -> 'U option) (list : IIndexedList<'T>) =
    IIndexedList.tryPick (fun item -> chooser item |> ValueOption.ofOption) list
    |> ValueOption.toOption

// ---- Transformation ----

[<CompiledName "Choose">]
let choose (chooser : 'T -> 'U option) (list : IIndexedList<'T>) =
    IIndexedList.choose (fun item -> chooser item |> ValueOption.ofOption) list
