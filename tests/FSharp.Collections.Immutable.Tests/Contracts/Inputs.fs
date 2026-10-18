/// The inputs that the contract tests run the functions on.
/// Expectations are computed with FSharp.Core over the order in which the collection enumerates its elements (ADR 0001, D8).
[<RequireQualifiedAccess>]
module FSharp.Collections.Immutable.Tests.Inputs

/// No elements.
let empty : int list = []

/// One element.
let singleton = [ 42 ]

/// Unsorted, with duplicates, negative numbers, zero and an even length.
let several = [ 5; -3; 8; 5; 0; -3; 12; 7 ]

/// Every input.
let all = [ empty; singleton; several ]

/// Every input with at least one element.
let nonEmpty = [ singleton; several ]

/// A second input for the functions of two collections, as long as `several`.
let other = [ 2; 4; 6; 8; 10; 12; 14; 16 ]

/// A third input for the functions of three collections, as long as `several`.
let third = [ -1; -2; -3; -4; -5; -6; -7; -8 ]

/// Elements whose projection collides, for the stability of sorting and the order of grouping:
/// the pairs share a key in their first component and are told apart by the second.
let pairs = [
    struct (2, 'a')
    struct (1, 'b')
    struct (2, 'c')
    struct (1, 'd')
    struct (0, 'e')
    struct (2, 'f')
]
