namespace FSharp.Collections.Immutable.Option

open FSharp.Collections.Immutable

// The functions call the FSharp.Core Array functions, which take and return options, over the underlying array, or
// convert the result of the default module where it implements the function itself.
[<RequireQualifiedAccess>]
[<CompiledName "ImmutableArrayModule">]
module FlatList =

    // ---- Creation ----

    [<CompiledName "Unfold">]
    let unfold (generator : 'State -> ('T * 'State) option) (state : 'State) = Array.unfold generator state |> wrapArray

    // ---- Access ----

    [<CompiledName "TryExactlyOne">]
    let tryExactlyOne (array : FlatList<'T>) = Array.tryExactlyOne (unwrapArray (nameof array) array)

    [<CompiledName "TryHead">]
    let tryHead (array : FlatList<'T>) = Array.tryHead (unwrapArray (nameof array) array)

    [<CompiledName "TryItem">]
    let tryItem index (array : FlatList<'T>) = Array.tryItem index (unwrapArray (nameof array) array)

    [<CompiledName "TryLast">]
    let tryLast (array : FlatList<'T>) = Array.tryLast (unwrapArray (nameof array) array)

    [<CompiledName "TryTail">]
    let tryTail (array : FlatList<'T>) =
        FSharp.Collections.Immutable.FlatList.tryTail array
        |> ValueOption.toOption

    // ---- Search ----

    [<CompiledName "Pick">]
    let pick (chooser : 'T -> 'U option) (array : FlatList<'T>) = Array.pick chooser (unwrapArray (nameof array) array)

    [<CompiledName "TryFind">]
    let tryFind predicate (array : FlatList<'T>) = Array.tryFind predicate (unwrapArray (nameof array) array)

    [<CompiledName "TryFindBack">]
    let tryFindBack predicate (array : FlatList<'T>) = Array.tryFindBack predicate (unwrapArray (nameof array) array)

    [<CompiledName "TryFindIndex">]
    let tryFindIndex predicate (array : FlatList<'T>) = Array.tryFindIndex predicate (unwrapArray (nameof array) array)

    [<CompiledName "TryFindIndexBack">]
    let tryFindIndexBack predicate (array : FlatList<'T>) = Array.tryFindIndexBack predicate (unwrapArray (nameof array) array)

    [<CompiledName "TryPick">]
    let tryPick (chooser : 'T -> 'U option) (array : FlatList<'T>) = Array.tryPick chooser (unwrapArray (nameof array) array)

    // ---- Transformation ----

    [<CompiledName "Choose">]
    let choose (chooser : 'T -> 'U option) (array : FlatList<'T>) =
        Array.choose chooser (unwrapArray (nameof array) array)
        |> wrapArray
