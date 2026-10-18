namespace FSharp.Collections.Immutable

[<AutoOpen>]
module EquatableArrayPatterns =

    // ---- Access ----

    let (|Arr0|_|) (array : EquatableArray<'T>) : bool = array.IsEmpty

    [<return : Struct>]
    let (|Arr1|_|) (array : EquatableArray<'T>) : 'T voption = if array.Length = 1 then ValueSome array[0] else ValueNone

    [<return : Struct>]
    let (|Arr2|_|) (array : EquatableArray<'T>) : struct ('T * 'T) voption =
        if array.Length = 2 then
            ValueSome (struct (array[0], array[1]))
        else
            ValueNone

    [<return : Struct>]
    let (|Arr3|_|) (array : EquatableArray<'T>) : struct ('T * 'T * 'T) voption =
        if array.Length = 3 then
            ValueSome (struct (array[0], array[1], array[2]))
        else
            ValueNone

    let (|ArrN|_|) (length : int) (array : EquatableArray<'T>) : bool = array.Length = length
