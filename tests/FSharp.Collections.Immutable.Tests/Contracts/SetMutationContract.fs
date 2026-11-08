namespace FSharp.Collections.Immutable.Tests

open FSharp.Collections.Immutable
open Microsoft.VisualStudio.TestTools.UnitTesting

/// <summary>
/// The contract of the incremental changes of the set modules that have them: <c>HashSet</c>, <c>SortedSet</c> and
/// <c>ISet</c> (ADR 0001, Appendix C).
/// </summary>
[<AbstractClass>]
type SetMutationContract<'S> (ops : SetMutationOps<'S>) =
    inherit SetContractBase<'S> (ops.Name, ops.ParamName, ops.OfSeq, ops.ToArray)

    /// An input in the shapes that a sequence argument takes, each with its name for the message of the assertion: an
    /// array, a list and a lazy sequence.
    static let shapes (input : int array) = [|
        struct ("an array", input :> seq<int>)
        struct ("a list", List.ofArray input :> seq<int>)
        struct ("a lazy sequence", Seq.map id input)
    |]

    [<TestMethod; DynamicData(nameof SetInputs.InputsAndValues, typeof<SetInputs>)>]
    member this.``add adds a value that the set does not contain`` (input : int array, value : int) =
        let struct (set, expected) = this.Create input

        CollectionAssert.AreEqual (
            Set.toArray (Set.add value expected),
            this.Sorted (ops.Add value set),
            $"%s{ops.Name}.add must add a value that the set does not contain"
        )

    [<TestMethod>]
    member this.``add leaves the input unchanged`` () =
        let set = ops.OfSeq Inputs.Several
        ops.Add 1000 set |> ignore

        CollectionAssert.AreEqual (
            Set.toArray (Set.ofArray Inputs.Several),
            this.Sorted set,
            $"%s{ops.Name}.add must not change its input"
        )

    [<TestMethod; DynamicData(nameof SetInputs.Pairs, typeof<SetInputs>)>]
    member this.``addRange adds the elements that the set does not contain`` (first : int array, second : int array) =
        let struct (set, expected) = this.Create first
        let expected = Set.toArray (Set.union expected (Set.ofArray second))

        for struct (shape, elements) in shapes second do
            CollectionAssert.AreEqual (
                expected,
                this.Sorted (ops.AddRange elements set),
                $"%s{ops.Name}.addRange of %s{shape} must add the elements that the set does not contain"
            )

    // The elements of the set stay, and of the elements that are new to it the first one of each class is added (D14)
    [<TestMethod; DynamicData(nameof SetInputs.Pairs, typeof<SetInputs>)>]
    member this.``addRange keeps the elements of the set that the comparer finds equal`` (first : int array, second : int array) =
        let set = ops.OfSeqByRemainder first
        let held = ops.ToArray set
        let classes = SetInputs.Remainders held

        let added =
            second
            |> Array.filter (fun item -> not (Set.contains (SetInputs.Remainder item) classes))
            |> Array.distinctBy SetInputs.Remainder

        for struct (shape, elements) in shapes second do
            CollectionAssert.AreEqual (
                Array.sort (Array.append held added),
                this.Sorted (ops.AddRange elements set),
                $"%s{ops.Name}.addRange of %s{shape} must keep the elements of the set and add the first of the other elements of each remainder"
            )

    [<TestMethod>]
    member this.``addRange leaves the input unchanged`` () =
        let set = ops.OfSeq Inputs.Several
        ops.AddRange Inputs.Other set |> ignore

        CollectionAssert.AreEqual (
            Set.toArray (Set.ofArray Inputs.Several),
            this.Sorted set,
            $"%s{ops.Name}.addRange must not change its input"
        )

    [<TestMethod; DynamicData(nameof SetInputs.SubsetPairs, typeof<SetInputs>)>]
    member _.``addRange returns the input when the sequence adds no element`` (first : int array, second : int array) =
        let set = ops.OfSeq first

        Assert.AreSame (
            box set,
            box (ops.AddRange second set),
            $"%s{ops.Name}.addRange must return the input when the sequence adds no element"
        )

    [<TestMethod; DynamicData(nameof SetInputs.SubsetPairsByRemainder, typeof<SetInputs>)>]
    member _.``addRange returns the input when the comparer finds every element of the sequence in the set``
        (first : int array, second : int array)
        =
        let set = ops.OfSeqByRemainder first

        Assert.AreSame (
            box set,
            box (ops.AddRange second set),
            $"%s{ops.Name}.addRange must return the input when the comparer finds every element of the sequence in the set"
        )

    [<TestMethod>]
    member this.``addRange throws ArgumentNullException for a null sequence`` () =
        this.ThrowsArgumentNull (
            "elements",
            (fun () -> ops.AddRange (Unchecked.defaultof<seq<int>>) (ops.OfSeq Inputs.Several)),
            "addRange of a null sequence must throw"
        )

    [<TestMethod; DynamicData(nameof SetInputs.All, typeof<SetInputs>)>]
    member this.``clear removes every element`` (input : int array) =
        let struct (set, _) = this.Create input
        Assert.IsEmpty (this.Sorted (ops.Clear set), $"%s{ops.Name}.clear must remove every element")

    [<TestMethod; DynamicData(nameof SetInputs.InputsAndValues, typeof<SetInputs>)>]
    member this.``remove removes the element equal to the value`` (input : int array, value : int) =
        let struct (set, expected) = this.Create input

        CollectionAssert.AreEqual (
            Set.toArray (Set.remove value expected),
            this.Sorted (ops.Remove value set),
            $"%s{ops.Name}.remove must remove the element equal to the value"
        )

    [<TestMethod>]
    member this.``remove leaves the input unchanged`` () =
        let set = ops.OfSeq Inputs.Several
        ops.Remove 5 set |> ignore

        CollectionAssert.AreEqual (
            Set.toArray (Set.ofArray Inputs.Several),
            this.Sorted set,
            $"%s{ops.Name}.remove must not change its input"
        )

    [<TestMethod; DynamicData(nameof SetInputs.Pairs, typeof<SetInputs>)>]
    member this.``removeRange removes the elements that the sequence contains`` (first : int array, second : int array) =
        let struct (set, expected) = this.Create first
        let expected = Set.toArray (Set.difference expected (Set.ofArray second))

        for struct (shape, elements) in shapes second do
            CollectionAssert.AreEqual (
                expected,
                this.Sorted (ops.RemoveRange elements set),
                $"%s{ops.Name}.removeRange of %s{shape} must remove the elements that the sequence contains"
            )

    // The comparer of the set decides which elements are equal to those of the sequence (D14)
    [<TestMethod; DynamicData(nameof SetInputs.Pairs, typeof<SetInputs>)>]
    member this.``removeRange removes the elements that the comparer finds equal`` (first : int array, second : int array) =
        let set = ops.OfSeqByRemainder first
        let classes = SetInputs.Remainders second

        let remaining =
            ops.ToArray set
            |> Array.filter (fun item -> not (Set.contains (SetInputs.Remainder item) classes))
            |> Array.sort

        for struct (shape, elements) in shapes second do
            CollectionAssert.AreEqual (
                remaining,
                this.Sorted (ops.RemoveRange elements set),
                $"%s{ops.Name}.removeRange of %s{shape} must remove the elements that the comparer finds equal to those of the sequence"
            )

    [<TestMethod>]
    member this.``removeRange leaves the input unchanged`` () =
        let set = ops.OfSeq Inputs.Several
        ops.RemoveRange Inputs.Other set |> ignore

        CollectionAssert.AreEqual (
            Set.toArray (Set.ofArray Inputs.Several),
            this.Sorted set,
            $"%s{ops.Name}.removeRange must not change its input"
        )

    [<TestMethod; DynamicData(nameof SetInputs.DisjointPairs, typeof<SetInputs>)>]
    member _.``removeRange returns the input when the sequence has no element of the set``
        (first : int array, second : int array)
        =
        let set = ops.OfSeq first

        Assert.AreSame (
            box set,
            box (ops.RemoveRange second set),
            $"%s{ops.Name}.removeRange must return the input when the sequence has no element of the set"
        )

    [<TestMethod; DynamicData(nameof SetInputs.DisjointPairsByRemainder, typeof<SetInputs>)>]
    member _.``removeRange returns the input when the comparer finds no element of the sequence in the set``
        (first : int array, second : int array)
        =
        let set = ops.OfSeqByRemainder first

        Assert.AreSame (
            box set,
            box (ops.RemoveRange second set),
            $"%s{ops.Name}.removeRange must return the input when the comparer finds no element of the sequence in the set"
        )

    [<TestMethod>]
    member this.``removeRange throws ArgumentNullException for a null sequence`` () =
        this.ThrowsArgumentNull (
            "elements",
            (fun () -> ops.RemoveRange (Unchecked.defaultof<seq<int>>) (ops.OfSeq Inputs.Several)),
            "removeRange of a null sequence must throw"
        )
