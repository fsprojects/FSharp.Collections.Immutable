namespace FSharp.Collections.Immutable.Tests

open FSharp.Collections.Immutable
open Microsoft.VisualStudio.TestTools.UnitTesting

/// <summary>
/// The contract of the creation functions of the concrete set modules, <c>HashSet</c>, <c>SortedSet</c> and
/// <c>FrozenSet</c> (ADR 0001, Appendix C), and of the members of the struct that creates their sets with a comparer (D12).
/// </summary>
/// <remarks>
/// A set is a sequence of its elements, so the struct can give an existing set its comparer: the contract needs the set
/// type to be a sequence of numbers for that check.
/// </remarks>
[<AbstractClass>]
type SetCreationContract<'S when 'S :> seq<int>> (ops : SetCreationOps<'S>) =
    inherit SetContractBase<'S> (ops.Name, ops.ParamName, ops.OfSeq, ops.ToArray)

    let custom = ops.CustomComparer

    /// The struct in the messages of the assertions.
    let customName = $"%s{ops.Name}.forComparer"

    /// Asserts that a set compares with the comparer of the struct and holds one element of the input for each remainder,
    /// the classes of the comparer.
    let holdsOnePerRemainder (input : int array) (set : 'S) (message : string) =
        Assert.AreSame (custom.Comparer, ops.Comparer set, $"%s{message} must compare with the comparer of the struct")
        let elements = ops.ToArray set

        CollectionAssert.AreEqual (
            input
            |> Array.map SetInputs.Remainder
            |> Array.distinct
            |> Array.sort,
            elements |> Array.map SetInputs.Remainder |> Array.sort,
            $"%s{message} must hold one element for each remainder"
        )

        for element in elements do
            Assert.Contains (element, input, $"%s{message} must hold elements of the input")

    /// Creates a set from a copy of an input and changes the copy afterwards, so that a set that shared the memory of its
    /// source would show the change.
    let createFromChangedCopy (create : int array -> 'S) (input : int array) =
        let source = Array.copy input
        let set = create source

        if source.Length > 0 then
            source[0] <- 1000

        set

    // ---- Creation ----

    [<TestMethod>]
    member this.``empty has no elements`` () =
        Assert.IsEmpty (this.Sorted (ops.Empty ()), $"%s{ops.Name}.empty must have no elements")

    [<TestMethod; DynamicData(nameof SetInputs.All, typeof<SetInputs>)>]
    member this.``ofArray copies the distinct elements`` (input : int array) =
        CollectionAssert.AreEqual (
            Set.toArray (Set.ofArray input),
            this.Sorted (createFromChangedCopy ops.OfArray input),
            $"%s{ops.Name}.ofArray must copy the distinct elements"
        )

    [<TestMethod>]
    member this.``ofArray throws for a null array as FSharp.Core does`` () =
        let source = Unchecked.defaultof<int array>

        this.ThrowsLikeSet ((fun () -> Set.ofArray source), (fun () -> ops.OfArray source), "ofArray of a null array must throw")

    [<TestMethod; DynamicData(nameof SetInputs.All, typeof<SetInputs>)>]
    member this.``ofList keeps the distinct elements`` (input : int array) =
        CollectionAssert.AreEqual (
            Set.toArray (Set.ofArray input),
            this.Sorted (ops.OfList (List.ofArray input)),
            $"%s{ops.Name}.ofList must keep the distinct elements"
        )

    // Set.ofList dereferences a null list without a check, so the function throws ArgumentNullException instead (D6)
    [<TestMethod>]
    member this.``ofList throws ArgumentNullException for a null list`` () =
        this.ThrowsArgumentNull (
            "elements",
            (fun () -> ops.OfList (Unchecked.defaultof<int list>)),
            "ofList of a null list must throw"
        )

    [<TestMethod; DynamicData(nameof SetInputs.All, typeof<SetInputs>)>]
    member this.``ofReadOnlySpan copies the distinct elements`` (input : int array) =
        CollectionAssert.AreEqual (
            Set.toArray (Set.ofArray input),
            this.Sorted (createFromChangedCopy ops.OfReadOnlySpan input),
            $"%s{ops.Name}.ofReadOnlySpan must copy the distinct elements"
        )

    [<TestMethod; DynamicData(nameof SetInputs.All, typeof<SetInputs>)>]
    member this.``ofSeq keeps the distinct elements`` (input : int array) =
        let expected = Set.toArray (Set.ofArray input)

        CollectionAssert.AreEqual (
            expected,
            this.Sorted (ops.OfSeq (List.ofArray input)),
            $"%s{ops.Name}.ofSeq of a list must keep the distinct elements"
        )

        CollectionAssert.AreEqual (
            expected,
            this.Sorted (ops.OfSeq input),
            $"%s{ops.Name}.ofSeq of an array must keep the distinct elements"
        )

        CollectionAssert.AreEqual (
            expected,
            this.Sorted (ops.OfSeq (Seq.map id input)),
            $"%s{ops.Name}.ofSeq of a lazy sequence must keep the distinct elements"
        )

    // Set.ofSeq dereferences a null sequence without a check, so the function throws ArgumentNullException instead (D6)
    [<TestMethod>]
    member this.``ofSeq throws ArgumentNullException for a null sequence`` () =
        this.ThrowsArgumentNull (
            "elements",
            (fun () -> ops.OfSeq (Unchecked.defaultof<seq<int>>)),
            "ofSeq of a null sequence must throw"
        )

    [<TestMethod; DynamicData(nameof SetInputs.All, typeof<SetInputs>)>]
    member this.``ofSpan copies the distinct elements`` (input : int array) =
        CollectionAssert.AreEqual (
            Set.toArray (Set.ofArray input),
            this.Sorted (createFromChangedCopy ops.OfSpan input),
            $"%s{ops.Name}.ofSpan must copy the distinct elements"
        )

    [<TestMethod>]
    member this.``singleton has one element`` () =
        CollectionAssert.AreEqual ([| 42 |], this.Sorted (ops.Singleton 42), $"%s{ops.Name}.singleton must have one element")

    // ---- The struct of forComparer ----

    [<TestMethod>]
    member _.``the struct creates an empty set with its comparer`` () =
        let set = custom.Empty ()
        Assert.IsEmpty (set, $"%s{customName}.empty must have no elements")
        Assert.AreSame (custom.Comparer, ops.Comparer set, $"%s{customName}.empty must compare with the comparer of the struct")

    [<TestMethod; DynamicData(nameof SetInputs.All, typeof<SetInputs>)>]
    member _.``the struct copies one element of the array for each class of its comparer`` (input : int array) =
        holdsOnePerRemainder input (createFromChangedCopy custom.OfArray input) $"%s{customName}.ofArray"

    [<TestMethod>]
    member this.``the struct throws for a null array as FSharp.Core does`` () =
        let source = Unchecked.defaultof<int array>

        this.ThrowsLikeSet (
            (fun () -> Set.ofArray source),
            (fun () -> custom.OfArray source),
            "forComparer ofArray of a null array must throw"
        )

    [<TestMethod; DynamicData(nameof SetInputs.All, typeof<SetInputs>)>]
    member _.``the struct keeps one element of the list for each class of its comparer`` (input : int array) =
        holdsOnePerRemainder input (custom.OfList (List.ofArray input)) $"%s{customName}.ofList"

    [<TestMethod>]
    member this.``the struct throws ArgumentNullException for a null list`` () =
        this.ThrowsArgumentNull (
            "elements",
            (fun () -> custom.OfList (Unchecked.defaultof<int list>)),
            "forComparer ofList of a null list must throw"
        )

    [<TestMethod; DynamicData(nameof SetInputs.All, typeof<SetInputs>)>]
    member _.``the struct copies one element of the read-only span for each class of its comparer`` (input : int array) =
        holdsOnePerRemainder input (createFromChangedCopy custom.OfReadOnlySpan input) $"%s{customName}.ofReadOnlySpan"

    [<TestMethod; DynamicData(nameof SetInputs.All, typeof<SetInputs>)>]
    member _.``the struct keeps one element of the sequence for each class of its comparer`` (input : int array) =
        holdsOnePerRemainder input (custom.OfSeq (List.ofArray input)) $"%s{customName}.ofSeq of a list"
        holdsOnePerRemainder input (custom.OfSeq input) $"%s{customName}.ofSeq of an array"
        holdsOnePerRemainder input (custom.OfSeq (Seq.map id input)) $"%s{customName}.ofSeq of a lazy sequence"

    [<TestMethod; DynamicData(nameof SetInputs.All, typeof<SetInputs>)>]
    member this.``the struct gives an existing set its comparer`` (input : int array) =
        let existing = ops.OfSeq input

        holdsOnePerRemainder input (custom.OfSeq (existing :> seq<int>)) $"%s{customName}.ofSeq of a set"

        CollectionAssert.AreEqual (
            Set.toArray (Set.ofArray input),
            this.Sorted existing,
            $"%s{customName}.ofSeq must not change the set"
        )

    [<TestMethod>]
    member this.``the struct throws ArgumentNullException for a null sequence`` () =
        this.ThrowsArgumentNull (
            "elements",
            (fun () -> custom.OfSeq (Unchecked.defaultof<seq<int>>)),
            "forComparer ofSeq of a null sequence must throw"
        )

    [<TestMethod; DynamicData(nameof SetInputs.All, typeof<SetInputs>)>]
    member _.``the struct copies one element of the span for each class of its comparer`` (input : int array) =
        holdsOnePerRemainder input (createFromChangedCopy custom.OfSpan input) $"%s{customName}.ofSpan"

    [<TestMethod>]
    member _.``the struct creates a set of one element with its comparer`` () =
        holdsOnePerRemainder [| 42 |] (custom.Singleton 42) $"%s{customName}.singleton"
