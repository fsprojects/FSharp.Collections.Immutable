[<RequireQualifiedAccess>]
module FSharp.Collections.Immutable.Option.IndexedList

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

    IndexedList.unfold generateStruct state

// ---- Access ----

[<CompiledName "TryExactlyOne">]
let tryExactlyOne (list : IndexedList<'T>) = IndexedList.tryExactlyOne list |> ValueOption.toOption

[<CompiledName "TryHead">]
let tryHead (list : IndexedList<'T>) = IndexedList.tryHead list |> ValueOption.toOption

[<CompiledName "TryItem">]
let tryItem index (list : IndexedList<'T>) = IndexedList.tryItem index list |> ValueOption.toOption

[<CompiledName "TryLast">]
let tryLast (list : IndexedList<'T>) = IndexedList.tryLast list |> ValueOption.toOption

[<CompiledName "TryTail">]
let tryTail (list : IndexedList<'T>) = IndexedList.tryTail list |> ValueOption.toOption

[<CompiledName "TryUncons">]
let tryUncons (list : IndexedList<'T>) =
    match IndexedList.tryUncons list with
    | ValueSome (struct (head, tail)) -> Some (head, tail)
    | ValueNone -> None

[<CompiledName "ConsPattern">]
let (|Cons|_|) (list : IndexedList<'T>) = tryUncons list

// ---- Search ----

[<CompiledName "Pick">]
let pick (chooser : 'T -> 'U option) (list : IndexedList<'T>) =
    IndexedList.pick (fun item -> chooser item |> ValueOption.ofOption) list

[<CompiledName "TryFind">]
let tryFind predicate (list : IndexedList<'T>) = IndexedList.tryFind predicate list |> ValueOption.toOption

[<CompiledName "TryFindBack">]
let tryFindBack predicate (list : IndexedList<'T>) =
    IndexedList.tryFindBack predicate list
    |> ValueOption.toOption

[<CompiledName "TryFindIndex">]
let tryFindIndex predicate (list : IndexedList<'T>) =
    IndexedList.tryFindIndex predicate list
    |> ValueOption.toOption

[<CompiledName "TryFindIndexBack">]
let tryFindIndexBack predicate (list : IndexedList<'T>) =
    IndexedList.tryFindIndexBack predicate list
    |> ValueOption.toOption

[<CompiledName "TryFindIndexBackRange">]
let tryFindIndexBackRange startIndex count predicate (list : IndexedList<'T>) =
    IndexedList.tryFindIndexBackRange startIndex count predicate list
    |> ValueOption.toOption

[<CompiledName "TryFindIndexRange">]
let tryFindIndexRange startIndex count predicate (list : IndexedList<'T>) =
    IndexedList.tryFindIndexRange startIndex count predicate list
    |> ValueOption.toOption

[<CompiledName "TryPick">]
let tryPick (chooser : 'T -> 'U option) (list : IndexedList<'T>) =
    IndexedList.tryPick (fun item -> chooser item |> ValueOption.ofOption) list
    |> ValueOption.toOption

// ---- Transformation ----

[<CompiledName "Choose">]
let choose (chooser : 'T -> 'U option) (list : IndexedList<'T>) =
    IndexedList.choose (fun item -> chooser item |> ValueOption.ofOption) list
