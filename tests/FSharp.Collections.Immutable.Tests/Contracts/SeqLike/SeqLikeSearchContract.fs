namespace FSharp.Collections.Immutable.Tests

open FSharp.Collections.Immutable
open Microsoft.VisualStudio.TestTools.UnitTesting

/// <summary>The predicates of the search tests over elements, as the cases of their rows.</summary>
/// <remarks>
/// MSTest names a row after its values, and a function has no readable name, so a row holds a case and the test takes
/// the function of the case.
/// </remarks>
[<RequireQualifiedAccess>]
type ElementPredicate =
    /// The even elements.
    | IsEven
    /// The negative elements.
    | IsNegative
    /// The elements below a hundred, which are all the elements of every input.
    | IsBelowHundred
    /// No element.
    | Never

    /// Tells whether an element satisfies the predicate.
    member this.Matches (item : int) =
        match this with
        | ElementPredicate.IsEven -> item % 2 = 0
        | ElementPredicate.IsNegative -> item < 0
        | ElementPredicate.IsBelowHundred -> item < 100
        | ElementPredicate.Never -> false

/// <summary>The predicates of the search tests over the pairs of elements at the same index, as the cases of their rows.</summary>
/// <remarks>
/// MSTest names a row after its values, and a function has no readable name, so a row holds a case and the test takes
/// the function of the case.
/// </remarks>
[<RequireQualifiedAccess>]
type PairPredicate =
    /// The pairs whose sum is six.
    | SumIsSix
    /// The pairs whose first element is greater than the second.
    | FirstIsGreater
    /// The pairs whose first element is smaller than the second.
    | FirstIsSmaller
    /// The pairs of different elements.
    | AreDifferent
    /// No pair.
    | Never

    /// Tells whether a pair satisfies the predicate.
    member this.Matches (first : int) (second : int) =
        match this with
        | PairPredicate.SumIsSix -> first + second = 6
        | PairPredicate.FirstIsGreater -> first > second
        | PairPredicate.FirstIsSmaller -> first < second
        | PairPredicate.AreDifferent -> first <> second
        | PairPredicate.Never -> false

/// The contract of the search functions of a sequence-like module.
[<AbstractClass>]
type SeqLikeSearchContract<'C> (ops : SeqOps<'C>) =
    inherit SeqLikeAccessContract<'C> (ops)

    static let never (_ : int) = false

    /// A chooser that returns a value for the negative elements only.
    static let negatedIfNegative (item : int) = if item < 0 then ValueSome (-item) else ValueNone

    /// The same chooser as `negatedIfNegative`, for the FSharp.Core oracle.
    static let negatedIfNegativeOption (item : int) = if item < 0 then Some (-item) else None

    /// Every input with each value to look for: the element of the singleton, two elements of the input with several,
    /// both repeated, and a value that no input has.
    static member InputsAndValues = seq {
        for input in Inputs.All do
            for value in [| 42; 5; -3; 1000 |] do
                struct (input, value)
    }

    /// Every input with each predicate of exists and of the searches that may find nothing.
    static member InputsAndPredicates = seq {
        for input in Inputs.All do
            for predicate in [| ElementPredicate.IsEven; ElementPredicate.IsNegative; ElementPredicate.Never |] do
                struct (input, predicate)
    }

    /// Every input with each predicate of forall: one that some elements satisfy, one that every element satisfies and
    /// one that no element satisfies.
    static member InputsAndForallPredicates = seq {
        for input in Inputs.All do
            for predicate in [| ElementPredicate.IsEven; ElementPredicate.IsBelowHundred; ElementPredicate.Never |] do
                struct (input, predicate)
    }

    /// The predicates of find and its variants, which some elements of the input with several elements satisfy.
    static member FindPredicates = seq {
        ElementPredicate.IsEven
        ElementPredicate.IsNegative
    }

    /// The predicates of exists2.
    static member Exists2Predicates = seq {
        PairPredicate.SumIsSix
        PairPredicate.FirstIsGreater
        PairPredicate.Never
    }

    /// The predicates of forall2.
    static member Forall2Predicates = seq {
        PairPredicate.FirstIsSmaller
        PairPredicate.AreDifferent
        PairPredicate.Never
    }

    [<TestMethod; DynamicData(nameof SeqLikeSearchContract<'C>.InputsAndValues)>]
    member this.``contains finds an element equal to the value`` (input : int array, value : int) =
        let struct (collection, elements) = this.Create input

        Assert.AreEqual<bool>(
            Array.contains value elements,
            ops.Contains value collection,
            $"%s{ops.Name}.contains must find an element equal to the value"
        )

    [<TestMethod; DynamicData(nameof SeqLikeSearchContract<'C>.InputsAndPredicates)>]
    member this.``exists finds an element that satisfies the predicate`` (input : int array, predicate : ElementPredicate) =
        let struct (collection, elements) = this.Create input

        Assert.AreEqual<bool>(
            Array.exists predicate.Matches elements,
            ops.Exists predicate.Matches collection,
            $"%s{ops.Name}.exists must find an element that satisfies the predicate"
        )

    [<TestMethod; DynamicData(nameof SeqLikeSearchContract<'C>.Exists2Predicates)>]
    member this.``exists2 tests the pairs at the same index`` (predicate : PairPredicate) =
        let struct (collection, elements) = this.Create Inputs.Several
        let struct (other, otherElements) = this.Create Inputs.Other

        Assert.AreEqual<bool>(
            Array.exists2 predicate.Matches elements otherElements,
            ops.Exists2 predicate.Matches collection other,
            $"%s{ops.Name}.exists2 must test the pairs at the same index"
        )

    [<TestMethod>]
    member this.``exists2 throws for different lengths as FSharp.Core does`` () =
        this.ThrowsLikeFSharpCore (
            (fun () -> Array.exists2 (fun _ _ -> false) Inputs.Singleton Inputs.Several),
            (fun () -> List.exists2 (fun _ _ -> false) (List.ofArray Inputs.Singleton) (List.ofArray Inputs.Several)),
            (fun () -> ops.Exists2 (fun _ _ -> false) (ops.OfSeq Inputs.Singleton) (ops.OfSeq Inputs.Several)),
            "exists2 of collections of different lengths must throw"
        )

    [<TestMethod; DynamicData(nameof SeqLikeSearchContract<'C>.FindPredicates)>]
    member this.``find returns the first element that satisfies the predicate`` (predicate : ElementPredicate) =
        let struct (collection, elements) = this.Create Inputs.Several

        Assert.AreEqual<int>(
            Array.find predicate.Matches elements,
            ops.Find predicate.Matches collection,
            $"%s{ops.Name}.find must return the first element that satisfies the predicate"
        )

    [<TestMethod>]
    member this.``find throws when no element satisfies the predicate as FSharp.Core does`` () =
        this.ThrowsLikeFSharpCore (
            (fun () -> Array.find never Inputs.Several),
            (fun () -> List.find never (List.ofArray Inputs.Several)),
            (fun () -> ops.Find never (ops.OfSeq Inputs.Several)),
            "find without a match must throw"
        )

    [<TestMethod; DynamicData(nameof SeqLikeSearchContract<'C>.FindPredicates)>]
    member this.``findBack returns the last element that satisfies the predicate`` (predicate : ElementPredicate) =
        let struct (collection, elements) = this.Create Inputs.Several

        Assert.AreEqual<int>(
            Array.findBack predicate.Matches elements,
            ops.FindBack predicate.Matches collection,
            $"%s{ops.Name}.findBack must return the last element that satisfies the predicate"
        )

    [<TestMethod>]
    member this.``findBack throws when no element satisfies the predicate as FSharp.Core does`` () =
        this.ThrowsLikeFSharpCore (
            (fun () -> Array.findBack never Inputs.Several),
            (fun () -> List.findBack never (List.ofArray Inputs.Several)),
            (fun () -> ops.FindBack never (ops.OfSeq Inputs.Several)),
            "findBack without a match must throw"
        )

    [<TestMethod; DynamicData(nameof SeqLikeSearchContract<'C>.FindPredicates)>]
    member this.``findIndex returns the index of the first element that satisfies the predicate`` (predicate : ElementPredicate) =
        let struct (collection, elements) = this.Create Inputs.Several

        Assert.AreEqual<int>(
            Array.findIndex predicate.Matches elements,
            ops.FindIndex predicate.Matches collection,
            $"%s{ops.Name}.findIndex must return the index of the first element that satisfies the predicate"
        )

    [<TestMethod>]
    member this.``findIndex throws when no element satisfies the predicate as FSharp.Core does`` () =
        this.ThrowsLikeFSharpCore (
            (fun () -> Array.findIndex never Inputs.Several),
            (fun () -> List.findIndex never (List.ofArray Inputs.Several)),
            (fun () -> ops.FindIndex never (ops.OfSeq Inputs.Several)),
            "findIndex without a match must throw"
        )

    [<TestMethod; DynamicData(nameof SeqLikeSearchContract<'C>.FindPredicates)>]
    member this.``findIndexBack returns the index of the last element that satisfies the predicate``
        (predicate : ElementPredicate)
        =
        let struct (collection, elements) = this.Create Inputs.Several

        Assert.AreEqual<int>(
            Array.findIndexBack predicate.Matches elements,
            ops.FindIndexBack predicate.Matches collection,
            $"%s{ops.Name}.findIndexBack must return the index of the last element that satisfies the predicate"
        )

    [<TestMethod>]
    member this.``findIndexBack throws when no element satisfies the predicate as FSharp.Core does`` () =
        this.ThrowsLikeFSharpCore (
            (fun () -> Array.findIndexBack never Inputs.Several),
            (fun () -> List.findIndexBack never (List.ofArray Inputs.Several)),
            (fun () -> ops.FindIndexBack never (ops.OfSeq Inputs.Several)),
            "findIndexBack without a match must throw"
        )

    [<TestMethod; DynamicData(nameof SeqLikeSearchContract<'C>.InputsAndForallPredicates)>]
    member this.``forall checks that every element satisfies the predicate`` (input : int array, predicate : ElementPredicate) =
        let struct (collection, elements) = this.Create input

        Assert.AreEqual<bool>(
            Array.forall predicate.Matches elements,
            ops.Forall predicate.Matches collection,
            $"%s{ops.Name}.forall must check that every element satisfies the predicate"
        )

    [<TestMethod; DynamicData(nameof SeqLikeSearchContract<'C>.Forall2Predicates)>]
    member this.``forall2 tests every pair at the same index`` (predicate : PairPredicate) =
        let struct (collection, elements) = this.Create Inputs.Several
        let struct (other, otherElements) = this.Create Inputs.Other

        Assert.AreEqual<bool>(
            Array.forall2 predicate.Matches elements otherElements,
            ops.Forall2 predicate.Matches collection other,
            $"%s{ops.Name}.forall2 must test every pair at the same index"
        )

    [<TestMethod>]
    member this.``forall2 throws for different lengths as FSharp.Core does`` () =
        this.ThrowsLikeFSharpCore (
            (fun () -> Array.forall2 (fun _ _ -> true) Inputs.Singleton Inputs.Several),
            (fun () -> List.forall2 (fun _ _ -> true) (List.ofArray Inputs.Singleton) (List.ofArray Inputs.Several)),
            (fun () -> ops.Forall2 (fun _ _ -> true) (ops.OfSeq Inputs.Singleton) (ops.OfSeq Inputs.Several)),
            "forall2 of collections of different lengths must throw"
        )

    [<TestMethod>]
    member this.``pick returns the first chosen value`` () =
        let struct (collection, elements) = this.Create Inputs.Several

        Assert.AreEqual<int>(
            Array.pick negatedIfNegativeOption elements,
            ops.Pick negatedIfNegative collection,
            $"%s{ops.Name}.pick must return the first chosen value"
        )

    [<TestMethod>]
    member this.``pick throws when the chooser returns nothing as FSharp.Core does`` () =
        this.ThrowsLikeFSharpCore (
            (fun () -> Array.pick (fun _ -> None) Inputs.Several : int),
            (fun () -> List.pick (fun _ -> None) (List.ofArray Inputs.Several) : int),
            (fun () -> ops.Pick (fun _ -> ValueNone) (ops.OfSeq Inputs.Several)),
            "pick without a choice must throw"
        )

    [<TestMethod; DynamicData(nameof SeqLikeSearchContract<'C>.InputsAndPredicates)>]
    member this.``tryFind returns the first element that satisfies the predicate or nothing``
        (input : int array, predicate : ElementPredicate)
        =
        let struct (collection, elements) = this.Create input

        Assert.AreEqual<int voption>(
            Array.tryFind predicate.Matches elements
            |> ValueOption.ofOption,
            ops.TryFind predicate.Matches collection,
            $"%s{ops.Name}.tryFind must return the first element that satisfies the predicate or nothing"
        )

    [<TestMethod; DynamicData(nameof SeqLikeSearchContract<'C>.InputsAndPredicates)>]
    member this.``tryFindBack returns the last element that satisfies the predicate or nothing``
        (input : int array, predicate : ElementPredicate)
        =
        let struct (collection, elements) = this.Create input

        Assert.AreEqual<int voption>(
            Array.tryFindBack predicate.Matches elements
            |> ValueOption.ofOption,
            ops.TryFindBack predicate.Matches collection,
            $"%s{ops.Name}.tryFindBack must return the last element that satisfies the predicate or nothing"
        )

    [<TestMethod; DynamicData(nameof SeqLikeSearchContract<'C>.InputsAndPredicates)>]
    member this.``tryFindIndex returns the index of the first element that satisfies the predicate or nothing``
        (input : int array, predicate : ElementPredicate)
        =
        let struct (collection, elements) = this.Create input

        Assert.AreEqual<int voption>(
            Array.tryFindIndex predicate.Matches elements
            |> ValueOption.ofOption,
            ops.TryFindIndex predicate.Matches collection,
            $"%s{ops.Name}.tryFindIndex must return the index of the first element that satisfies the predicate or nothing"
        )

    [<TestMethod; DynamicData(nameof SeqLikeSearchContract<'C>.InputsAndPredicates)>]
    member this.``tryFindIndexBack returns the index of the last element that satisfies the predicate or nothing``
        (input : int array, predicate : ElementPredicate)
        =
        let struct (collection, elements) = this.Create input

        Assert.AreEqual<int voption>(
            Array.tryFindIndexBack predicate.Matches elements
            |> ValueOption.ofOption,
            ops.TryFindIndexBack predicate.Matches collection,
            $"%s{ops.Name}.tryFindIndexBack must return the index of the last element that satisfies the predicate or nothing"
        )

    [<TestMethod; DynamicData(nameof Inputs.All, typeof<Inputs>)>]
    member this.``tryPick returns the first chosen value or nothing`` (input : int array) =
        let struct (collection, elements) = this.Create input

        Assert.AreEqual<int voption>(
            Array.tryPick negatedIfNegativeOption elements
            |> ValueOption.ofOption,
            ops.TryPick negatedIfNegative collection,
            $"%s{ops.Name}.tryPick must return the first chosen value or nothing"
        )
