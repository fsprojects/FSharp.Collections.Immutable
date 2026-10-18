/// The expectations that the tests of the flat list and of its builder compute in plain F#, over arrays.
[<RequireQualifiedAccess>]
module FSharp.Collections.Immutable.Tests.FlatListOracles

open System
open System.Collections.Generic
open FSharp.Collections.Immutable

/// A flat list of strings as a sequence of objects. A flat list of strings is also a sequence of objects, by the
/// covariance of seq: the element type of the sequence is not the one of the flat list, and a type test for the flat
/// list of objects does not see it.
let stringsAsObjects (strings : FlatList<string>) : seq<obj> =
    match box strings with
    | :? seq<obj> as widened -> widened
    | _ -> invalidOp "A flat list of strings must be a sequence of objects"

/// A default flat list of strings as a sequence of objects.
let defaultStringsAsObjects : seq<obj> = stringsAsObjects Unchecked.defaultof<FlatList<string>>
/// A builder that holds a copy of the elements.
let builderOf (elements : 'T array) = FlatList.ofArray elements |> FlatList.toBuilder

/// The elements without the first one that the equality finds equal to the item: the oracle of the removal by value.
let removeFirst (equal : 'T -> 'T -> bool) (item : 'T) (elements : 'T array) =
    match Array.tryFindIndex (equal item) elements with
    | Some index -> Array.removeAt index elements
    | None -> elements

/// The elements with the first one that the equality finds equal to the old value replaced by the new value, or
/// ValueNone when no element is equal: the oracle of the replacement.
let replaceFirst (equal : 'T -> 'T -> bool) (oldValue : 'T) (newValue : 'T) (elements : 'T array) =
    match Array.tryFindIndex (equal oldValue) elements with
    | Some index -> ValueSome (Array.updateAt index newValue elements)
    | None -> ValueNone

/// Tells whether two strings are equal when the case is ignored.
let equalIgnoringCase (left : string) (right : string) = String.Equals (left, right, StringComparison.OrdinalIgnoreCase)

/// The strings of the removal by value; "b" and "B" are equal when the case is ignored.
let names () = [| "a"; "b"; "B"; "c" |]

/// The strings of the searches of a range: "b" is there twice, "B" once, and the three are equal when the case is
/// ignored.
let rangeNames () = [| "a"; "b"; "B"; "c"; "b" |]

/// The comparison of the descending order.
let descending (left : int) (right : int) = compare right left

/// The comparison of the order of the absolute values.
let byAbsolute (left : int) (right : int) = compare (abs left) (abs right)

/// The lines of the message of the exception that an action throws after the first line, which is empty when the
/// message has one line. FSharp.Core writes the names and the values of the arguments there, and it does not localize
/// them. The action must throw.
let messageDetail (action : unit -> unit) =
    let thrown =
        try
            action ()
            ValueNone
        with ex ->
            ValueSome ex

    match thrown with
    | ValueSome ex ->
        match ex.Message.IndexOf '\n' with
        | -1 -> ""
        | newline -> ex.Message.Substring (newline + 1)
    | ValueNone -> invalidOp "The action must throw"

/// The index of the first element of a range that the equality finds equal to the value, or -1: the oracle of the
/// searches from the front.
let firstInRange (equal : 'T -> 'T -> bool) startIndex count (value : 'T) (elements : 'T array) =
    seq { startIndex .. startIndex + count - 1 }
    |> Seq.tryFind (fun index -> equal elements[index] value)
    |> Option.defaultValue -1

/// The index of the first element that the equality finds equal to the value, searching backward from the start index
/// over the count of elements, or -1: the oracle of the searches from the back.
let lastInRange (equal : 'T -> 'T -> bool) startIndex count (value : 'T) (elements : 'T array) =
    seq { startIndex .. -1 .. startIndex - count + 1 }
    |> Seq.tryFind (fun index -> equal elements[index] value)
    |> Option.defaultValue -1

/// The result of a binary search of a range of elements that a comparer sorts and that are all different: the index of
/// the value, or the complement of the index of the first larger element of the range.
let searchRange (comparer : IComparer<'T>) index count (value : 'T) (sorted : 'T array) =
    let range = seq { index .. index + count - 1 }

    match
        range
        |> Seq.tryFind (fun at -> comparer.Compare (sorted[at], value) = 0)
    with
    | Some found -> found
    | None ->
        let smaller =
            range
            |> Seq.filter (fun at -> comparer.Compare (sorted[at], value) < 0)
            |> Seq.length

        ~~~(index + smaller)
