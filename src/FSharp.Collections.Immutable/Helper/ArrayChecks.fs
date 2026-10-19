/// The checks of the FSharp.Core Array module that the functions of several arrays share.
module internal FSharp.Collections.Immutable.ArrayChecks

/// The arrays of a function of two arrays must have the same length.
let inline sameLengths (name1 : string) (array1 : 'T1 array) (name2 : string) (array2 : 'T2 array) =
    if array1.Length <> array2.Length then
        Raise.invalidArgDifferentArrayLength name1 array1.Length name2 array2.Length

/// The arrays of a function of three arrays must have the same length.
let inline sameLengths3
    (name1 : string)
    (array1 : 'T1 array)
    (name2 : string)
    (array2 : 'T2 array)
    (name3 : string)
    (array3 : 'T3 array)
    =
    if
        array1.Length <> array2.Length
        || array1.Length <> array3.Length
    then
        Raise.invalidArg3ArraysDifferent name1 name2 name3 array1.Length array2.Length array3.Length
