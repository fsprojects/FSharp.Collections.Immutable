namespace FSharp.Collections.Immutable.Tests

open System
open System.Collections.Generic

/// <summary>
/// The inputs that the set contracts run the functions on, and the comparers of the structs that create sets with a
/// comparer (ADR 0001, D12).
/// </summary>
/// <remarks>
/// Every property builds new arrays on each read: MSTest hands the arrays of a row source to the tests as they are, so an
/// array that the tests shared would carry the change that one test made into the others.
/// </remarks>
[<AbstractClass; Sealed>]
type SetInputs =

    static let sameRemainder : IEqualityComparer<int> = {
        new IEqualityComparer<int> with
            member _.Equals (left, right) = SetInputs.Remainder left = SetInputs.Remainder right
            member _.GetHashCode item = SetInputs.Remainder item
    }

    static let byRemainder : IComparer<int> = {
        new IComparer<int> with
            member _.Compare (left, right) = compare (SetInputs.Remainder left) (SetInputs.Remainder right)
    }

    /// A subset of `Inputs.Several` that overlaps `Inputs.Other`, for the subset and superset predicates.
    static member Subset = [| 5; 8 |]

    /// <summary>
    /// Every input, as the rows of the tests that run on each one: no elements, one element, elements with duplicates,
    /// an input that overlaps it and a subset of it.
    /// </summary>
    static member All = seq {
        Inputs.Empty
        Inputs.Singleton
        Inputs.Several
        Inputs.Other
        SetInputs.Subset
    }

    /// <summary>
    /// Every ordered pair of inputs, as the rows of the functions of two sets: equal, disjoint, overlapping, subsets and
    /// supersets.
    /// </summary>
    static member Pairs = seq {
        for first in SetInputs.All do
            for second in SetInputs.All do
                struct (first, second)
    }

    /// <summary>
    /// The pairs of inputs whose second input is a subset of the first one, as rows: the second input adds nothing to the
    /// first one, and empty and equal inputs are among them.
    /// </summary>
    static member SubsetPairs =
        SetInputs.Pairs
        |> Seq.filter (fun struct (first, second) -> Set.isSubset (Set.ofArray second) (Set.ofArray first))

    /// <summary>
    /// The pairs of inputs whose remainders in the division by 5 are all remainders of the first input too, as rows: the
    /// second input adds nothing to a set that compares by remainders, although it may hold other numbers.
    /// </summary>
    static member SubsetPairsByRemainder =
        SetInputs.Pairs
        |> Seq.filter (fun struct (first, second) -> Set.isSubset (SetInputs.Remainders second) (SetInputs.Remainders first))

    /// The pairs of inputs that have no number in common, as rows: the second input removes nothing from the first one.
    static member DisjointPairs =
        SetInputs.Pairs
        |> Seq.filter (fun struct (first, second) -> Set.isEmpty (Set.intersect (Set.ofArray first) (Set.ofArray second)))

    /// <summary>
    /// The pairs of inputs that have no remainder in the division by 5 in common, as rows: the second input removes nothing
    /// from a set that compares by remainders, although it may hold numbers of the first input.
    /// </summary>
    static member DisjointPairsByRemainder =
        SetInputs.Pairs
        |> Seq.filter (fun struct (first, second) ->
            Set.isEmpty (Set.intersect (SetInputs.Remainders first) (SetInputs.Remainders second))
        )

    /// The values to look up: in no input, in one input and in several inputs.
    static member Values = [| 42; 5; -3; 8; 1000 |]

    /// Every input with every value to look up, add or remove, as rows.
    static member InputsAndValues = seq {
        for input in SetInputs.All do
            for value in SetInputs.Values do
                struct (input, value)
    }

    /// <summary>
    /// The remainder of a number in the division by 5, from 0 to 4.
    /// </summary>
    /// <remarks>
    /// The comparers of the structs that create sets with a comparer consider two numbers equal when their remainders are
    /// equal, so that they merge elements of the inputs that the default comparer tells apart: 5 and 0, or -3, 12 and 7
    /// of <c>Inputs.Several</c>.
    /// </remarks>
    static member Remainder (item : int) = ((item % 5) + 5) % 5

    /// The remainders of the numbers of an input in the division by 5, as a set: the classes of the numbers for a set that
    /// compares by remainders.
    static member Remainders (input : int array) = Set.ofArray (Array.map SetInputs.Remainder input)

    /// The equality comparer of the structs that create hash and frozen sets: equal remainders make equal numbers.
    static member SameRemainder = sameRemainder

    /// The comparer of the structs that create sorted sets: it orders numbers by their remainders.
    static member ByRemainder = byRemainder

/// <summary>
/// The inputs of the tests that pass a sequence to a relation or to an operation of the set algebra. The functions of the
/// set modules take sets, where the members of the BCL types take sequences, so a sequence is converted to a set first
/// (ADR 0001, D14).
/// </summary>
/// <remarks>
/// <para>
/// A test builds the first set from the strings of a relation with one of the comparers, builds the sequence from the
/// other strings, and compares the answer of the member of the BCL type for the sequence with the answer of the function of
/// the module for the set that the sequence is converted to.
/// </para>
/// <para>
/// The strings tell the comparers apart: when the first set compares by reference, each relation and each operation answers
/// differently for the sequence that the default comparer converts, which keeps one of "a1" and "a2", than for the sequence
/// that the comparer of the first set converts.
/// </para>
/// </remarks>
[<AbstractClass; Sealed>]
type SetRelationInputs =

    /// <summary>
    /// The strings by name: "a1" and "a2" have the text "a" and are different instances, "A" differs from them in case, and
    /// "b" has other text.
    /// </summary>
    /// <remarks>
    /// The instances are created once: a comparer by reference tells "a1" from "a2", so the first set and the sequence of a
    /// test must hold the same two instances.
    /// </remarks>
    static let pool : Map<string, string> =
        Map.ofArray [| "a1", String [| 'a' |]; "a2", String [| 'a' |]; "A", "A"; "b", "b" |]

    /// <summary>
    /// The comparers of the first set, by name, each with whether it finds equal every pair of strings that the default
    /// comparer finds equal.
    /// </summary>
    static let comparers : Map<string, struct (IEqualityComparer<string> * bool)> =
        Map.ofArray [|
            "default comparer", struct (EqualityComparer<string>.Default :> IEqualityComparer<string>, true)
            "comparer that ignores case", struct (StringComparer.OrdinalIgnoreCase :> IEqualityComparer<string>, true)
            // It tells "a1" from "a2", which the default comparer finds equal
            "reference comparer", struct (HashIdentity.Reference, false)
        |]

    /// <summary>
    /// The names of the strings of the first set and of the sequence of each relation, by the name of the relation.
    /// </summary>
    static let items : Map<string, struct (string array * string array)> =
        Map.ofArray [|
            "isProperSubset", struct ([| "a1"; "a2" |], [| "a1"; "a2"; "b" |])
            "isProperSuperset", struct ([| "a1"; "a2"; "A" |], [| "a1"; "a2"; "A" |])
            "isSubset", struct ([| "a1"; "a2"; "A" |], [| "a1"; "a2"; "A" |])
            "isSuperset", struct ([| "a1" |], [| "a1"; "a2" |])
            // The default comparer keeps "a2", the first string of the sequence
            "overlaps", struct ([| "a1" |], [| "a2"; "a1" |])
            "setEquals", struct ([| "a1"; "a2"; "A" |], [| "a1"; "a2"; "A" |])
        |]

    /// <summary>
    /// The names of the strings of the first set and of the sequence of each operation of the set algebra that takes a
    /// sequence in the BCL types and returns a set, by the name of the operation.
    /// </summary>
    static let operationItems : Map<string, struct (string array * string array)> =
        Map.ofArray [|
            "intersect", struct ([| "a1"; "a2" |], [| "a1"; "a2" |])
            "symmetricDifference", struct ([| "a1"; "a2" |], [| "a1"; "a2" |])
        |]

    /// The strings for their names in the pool.
    static member Strings (names : string array) = names |> Array.map (fun name -> pool[name])

    /// The names of the strings of the first set and of the sequence of each relation, by the name of the relation.
    static member Items = items

    /// The comparers of the first set, by name, each with whether it finds equal every pair of strings that the default
    /// comparer finds equal.
    static member Comparers = comparers

    /// The relations of a set with a sequence, each with every comparer of the first set, as rows.
    static member Rows = seq {
        for relation in items.Keys do
            for comparer in comparers.Keys do
                struct (relation, comparer)
    }

    /// The relations that need no conversion, each with every comparer of the first set, as rows.
    static member StreamingRows = seq {
        for relation in [ "isSuperset"; "overlaps" ] do
            for comparer in comparers.Keys do
                struct (relation, comparer)
    }

    /// The names of the strings of the first set and of the sequence of each operation of the set algebra, by the name of
    /// the operation.
    static member OperationItems = operationItems

    /// The operations of the set algebra with a sequence, each with every comparer of the first set, as rows.
    static member OperationRows = seq {
        for operation in operationItems.Keys do
            for comparer in comparers.Keys do
                struct (operation, comparer)
    }
