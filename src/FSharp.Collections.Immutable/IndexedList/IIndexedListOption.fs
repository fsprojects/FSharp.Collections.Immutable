[<RequireQualifiedAccess>]
module FSharp.Collections.Immutable.Option.IIndexedList

open FSharp.Collections.Immutable

// The functions delegate to the default module and convert its results, or the results of the callbacks that they pass to
// it, so that both modules have the same checks and exceptions.

// ---- Access ----

[<CompiledName "TryExactlyOne">]
let tryExactlyOne (list : IIndexedList<'T>) =
    FSharp.Collections.Immutable.IIndexedList.tryExactlyOne list
    |> ValueOption.toOption

[<CompiledName "TryHead">]
let tryHead (list : IIndexedList<'T>) =
    FSharp.Collections.Immutable.IIndexedList.tryHead list
    |> ValueOption.toOption

[<CompiledName "TryItem">]
let tryItem index (list : IIndexedList<'T>) =
    FSharp.Collections.Immutable.IIndexedList.tryItem index list
    |> ValueOption.toOption

[<CompiledName "TryLast">]
let tryLast (list : IIndexedList<'T>) =
    FSharp.Collections.Immutable.IIndexedList.tryLast list
    |> ValueOption.toOption

[<CompiledName "TryTail">]
let tryTail (list : IIndexedList<'T>) =
    FSharp.Collections.Immutable.IIndexedList.tryTail list
    |> ValueOption.toOption

[<CompiledName "TryUncons">]
let tryUncons (list : IIndexedList<'T>) =
    match FSharp.Collections.Immutable.IIndexedList.tryUncons list with
    | ValueSome (struct (head, tail)) -> Some (head, tail)
    | ValueNone -> None

[<CompiledName "ConsPattern">]
let (|Cons|_|) (list : IIndexedList<'T>) = tryUncons list

// ---- Search ----

[<CompiledName "Pick">]
let pick (chooser : 'T -> 'U option) (list : IIndexedList<'T>) =
    FSharp.Collections.Immutable.IIndexedList.pick (fun item -> chooser item |> ValueOption.ofOption) list

[<CompiledName "TryFind">]
let tryFind predicate (list : IIndexedList<'T>) =
    FSharp.Collections.Immutable.IIndexedList.tryFind predicate list
    |> ValueOption.toOption

[<CompiledName "TryFindBack">]
let tryFindBack predicate (list : IIndexedList<'T>) =
    FSharp.Collections.Immutable.IIndexedList.tryFindBack predicate list
    |> ValueOption.toOption

[<CompiledName "TryFindIndex">]
let tryFindIndex predicate (list : IIndexedList<'T>) =
    FSharp.Collections.Immutable.IIndexedList.tryFindIndex predicate list
    |> ValueOption.toOption

[<CompiledName "TryFindIndexBack">]
let tryFindIndexBack predicate (list : IIndexedList<'T>) =
    FSharp.Collections.Immutable.IIndexedList.tryFindIndexBack predicate list
    |> ValueOption.toOption

[<CompiledName "TryPick">]
let tryPick (chooser : 'T -> 'U option) (list : IIndexedList<'T>) =
    FSharp.Collections.Immutable.IIndexedList.tryPick (fun item -> chooser item |> ValueOption.ofOption) list
    |> ValueOption.toOption

// ---- Transformation ----

[<CompiledName "Choose">]
let choose (chooser : 'T -> 'U option) (list : IIndexedList<'T>) =
    FSharp.Collections.Immutable.IIndexedList.choose (fun item -> chooser item |> ValueOption.ofOption) list
