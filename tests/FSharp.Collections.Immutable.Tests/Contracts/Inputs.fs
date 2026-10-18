namespace FSharp.Collections.Immutable.Tests

/// The inputs that the contract tests run the functions on.
/// Expectations are computed with FSharp.Core over the order in which the collection enumerates its elements (ADR 0001, D8).
/// Every property builds new arrays on each read: MSTest hands the arrays of a row source to the tests as they are, so an
/// array that the tests shared would carry the change that one test made into the others.
[<AbstractClass; Sealed>]
type Inputs =

    /// No elements.
    static member Empty : int array = [||]

    /// One element.
    static member Singleton = [| 42 |]

    /// Unsorted, with duplicates, negative numbers, zero and an even length.
    static member Several = [| 5; -3; 8; 5; 0; -3; 12; 7 |]

    /// Every input, as the rows of the tests that run on each one.
    static member All = seq {
        Inputs.Empty
        Inputs.Singleton
        Inputs.Several
    }

    /// Every input with at least one element, as the rows of the tests that need one.
    static member NonEmpty = seq {
        Inputs.Singleton
        Inputs.Several
    }

    /// A second input for the functions of two collections, as long as `Several`.
    static member Other = [| 2; 4; 6; 8; 10; 12; 14; 16 |]

    /// A third input for the functions of three collections, as long as `Several`.
    static member Third = [| -1; -2; -3; -4; -5; -6; -7; -8 |]

    /// Elements whose projection collides, for the stability of sorting and the order of grouping:
    /// the pairs share a key in their first component and are told apart by the second.
    static member Pairs = [|
        struct (2, 'a')
        struct (1, 'b')
        struct (2, 'c')
        struct (1, 'd')
        struct (0, 'e')
        struct (2, 'f')
    |]
