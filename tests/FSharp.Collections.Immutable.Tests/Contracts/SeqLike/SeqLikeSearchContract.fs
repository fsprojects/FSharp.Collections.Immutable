namespace FSharp.Collections.Immutable.Tests

open FSharp.Collections.Immutable
open Microsoft.VisualStudio.TestTools.UnitTesting

/// The contract of the search functions of a sequence-like module.
[<AbstractClass>]
type SeqLikeSearchContract<'C> (ops : SeqOps<'C>) =
    inherit SeqLikeAccessContract<'C> (ops)

    static let isEven (item : int) = item % 2 = 0
    static let isNegative (item : int) = item < 0
    static let never (_ : int) = false

    /// A chooser that returns a value for the negative elements only.
    static let negatedIfNegative (item : int) = if item < 0 then ValueSome (-item) else ValueNone

    /// The same chooser as `negatedIfNegative`, for the FSharp.Core oracle.
    static let negatedIfNegativeOption (item : int) = if item < 0 then Some (-item) else None

    [<TestMethod; TestCategory(nameof FlatList.contains)>]
    member this.``contains finds an element equal to the value`` () =
        this.ForEach (
            Inputs.all,
            fun collection elements ->
                for value in [ 42; 5; -3; 1000 ] do
                    Assert.AreEqual<bool>(
                        List.contains value elements,
                        ops.Contains value collection,
                        $"%s{ops.Name}.contains %d{value} in %A{elements}"
                    )
        )

    [<TestMethod; TestCategory(nameof FlatList.exists)>]
    member this.``exists finds an element that satisfies the predicate`` () =
        this.ForEach (
            Inputs.all,
            fun collection elements ->
                for predicate in [ isEven; isNegative; never ] do
                    Assert.AreEqual<bool>(
                        List.exists predicate elements,
                        ops.Exists predicate collection,
                        $"%s{ops.Name}.exists over %A{elements}"
                    )
        )

    [<TestMethod; TestCategory(nameof FlatList.exists2)>]
    member this.``exists2 tests the pairs at the same index`` () =
        let collection = ops.OfSeq Inputs.several
        let other = ops.OfSeq Inputs.other
        let elements = ops.ToList collection
        let otherElements = ops.ToList other

        for predicate in [ (fun a b -> a + b = 6); (fun a b -> a > b); (fun _ _ -> false) ] do
            Assert.AreEqual<bool>(
                List.exists2 predicate elements otherElements,
                ops.Exists2 predicate collection other,
                $"%s{ops.Name}.exists2 over %A{elements} and %A{otherElements}"
            )

    [<TestMethod; TestCategory(nameof FlatList.exists2)>]
    member this.``exists2 throws for different lengths as FSharp.Core does`` () =
        this.ThrowsLikeFSharpCore (
            (fun () -> Array.exists2 (fun _ _ -> false) (List.toArray Inputs.singleton) (List.toArray Inputs.several)),
            (fun () -> List.exists2 (fun _ _ -> false) Inputs.singleton Inputs.several),
            (fun () -> ops.Exists2 (fun _ _ -> false) (ops.OfSeq Inputs.singleton) (ops.OfSeq Inputs.several)),
            "exists2 of collections of different lengths must throw"
        )

    [<TestMethod; TestCategory(nameof FlatList.find)>]
    member this.``find returns the first element that satisfies the predicate`` () =
        this.ForEach (
            [ Inputs.several ],
            fun collection elements ->
                for predicate in [ isEven; isNegative ] do
                    Assert.AreEqual<int>(
                        List.find predicate elements,
                        ops.Find predicate collection,
                        $"%s{ops.Name}.find in %A{elements}"
                    )
        )

    [<TestMethod; TestCategory(nameof FlatList.find)>]
    member this.``find throws when no element satisfies the predicate as FSharp.Core does`` () =
        this.ThrowsLikeFSharpCore (
            (fun () -> Array.find never (List.toArray Inputs.several)),
            (fun () -> List.find never Inputs.several),
            (fun () -> ops.Find never (ops.OfSeq Inputs.several)),
            "find without a match must throw"
        )

    [<TestMethod; TestCategory(nameof FlatList.findBack)>]
    member this.``findBack returns the last element that satisfies the predicate`` () =
        this.ForEach (
            [ Inputs.several ],
            fun collection elements ->
                for predicate in [ isEven; isNegative ] do
                    Assert.AreEqual<int>(
                        List.findBack predicate elements,
                        ops.FindBack predicate collection,
                        $"%s{ops.Name}.findBack in %A{elements}"
                    )
        )

    [<TestMethod; TestCategory(nameof FlatList.findBack)>]
    member this.``findBack throws when no element satisfies the predicate as FSharp.Core does`` () =
        this.ThrowsLikeFSharpCore (
            (fun () -> Array.findBack never (List.toArray Inputs.several)),
            (fun () -> List.findBack never Inputs.several),
            (fun () -> ops.FindBack never (ops.OfSeq Inputs.several)),
            "findBack without a match must throw"
        )

    [<TestMethod; TestCategory(nameof FlatList.findIndex)>]
    member this.``findIndex returns the index of the first element that satisfies the predicate`` () =
        this.ForEach (
            [ Inputs.several ],
            fun collection elements ->
                for predicate in [ isEven; isNegative ] do
                    Assert.AreEqual<int>(
                        List.findIndex predicate elements,
                        ops.FindIndex predicate collection,
                        $"%s{ops.Name}.findIndex in %A{elements}"
                    )
        )

    [<TestMethod; TestCategory(nameof FlatList.findIndex)>]
    member this.``findIndex throws when no element satisfies the predicate as FSharp.Core does`` () =
        this.ThrowsLikeFSharpCore (
            (fun () -> Array.findIndex never (List.toArray Inputs.several)),
            (fun () -> List.findIndex never Inputs.several),
            (fun () -> ops.FindIndex never (ops.OfSeq Inputs.several)),
            "findIndex without a match must throw"
        )

    [<TestMethod; TestCategory(nameof FlatList.findIndexBack)>]
    member this.``findIndexBack returns the index of the last element that satisfies the predicate`` () =
        this.ForEach (
            [ Inputs.several ],
            fun collection elements ->
                for predicate in [ isEven; isNegative ] do
                    Assert.AreEqual<int>(
                        List.findIndexBack predicate elements,
                        ops.FindIndexBack predicate collection,
                        $"%s{ops.Name}.findIndexBack in %A{elements}"
                    )
        )

    [<TestMethod; TestCategory(nameof FlatList.findIndexBack)>]
    member this.``findIndexBack throws when no element satisfies the predicate as FSharp.Core does`` () =
        this.ThrowsLikeFSharpCore (
            (fun () -> Array.findIndexBack never (List.toArray Inputs.several)),
            (fun () -> List.findIndexBack never Inputs.several),
            (fun () -> ops.FindIndexBack never (ops.OfSeq Inputs.several)),
            "findIndexBack without a match must throw"
        )

    [<TestMethod; TestCategory(nameof FlatList.forall)>]
    member this.``forall checks that every element satisfies the predicate`` () =
        this.ForEach (
            Inputs.all,
            fun collection elements ->
                for predicate in [ isEven; (fun item -> item < 100); never ] do
                    Assert.AreEqual<bool>(
                        List.forall predicate elements,
                        ops.Forall predicate collection,
                        $"%s{ops.Name}.forall over %A{elements}"
                    )
        )

    [<TestMethod; TestCategory(nameof FlatList.forall2)>]
    member this.``forall2 tests every pair at the same index`` () =
        let collection = ops.OfSeq Inputs.several
        let other = ops.OfSeq Inputs.other
        let elements = ops.ToList collection
        let otherElements = ops.ToList other

        for predicate in [ (fun a b -> a < b); (fun a b -> a <> b); (fun _ _ -> false) ] do
            Assert.AreEqual<bool>(
                List.forall2 predicate elements otherElements,
                ops.Forall2 predicate collection other,
                $"%s{ops.Name}.forall2 over %A{elements} and %A{otherElements}"
            )

    [<TestMethod; TestCategory(nameof FlatList.forall2)>]
    member this.``forall2 throws for different lengths as FSharp.Core does`` () =
        this.ThrowsLikeFSharpCore (
            (fun () -> Array.forall2 (fun _ _ -> true) (List.toArray Inputs.singleton) (List.toArray Inputs.several)),
            (fun () -> List.forall2 (fun _ _ -> true) Inputs.singleton Inputs.several),
            (fun () -> ops.Forall2 (fun _ _ -> true) (ops.OfSeq Inputs.singleton) (ops.OfSeq Inputs.several)),
            "forall2 of collections of different lengths must throw"
        )

    [<TestMethod; TestCategory(nameof FlatList.pick)>]
    member this.``pick returns the first chosen value`` () =
        this.ForEach (
            [ Inputs.several ],
            fun collection elements ->
                Assert.AreEqual<int>(
                    List.pick negatedIfNegativeOption elements,
                    ops.Pick negatedIfNegative collection,
                    $"%s{ops.Name}.pick in %A{elements}"
                )
        )

    [<TestMethod; TestCategory(nameof FlatList.pick)>]
    member this.``pick throws when the chooser returns nothing as FSharp.Core does`` () =
        this.ThrowsLikeFSharpCore (
            (fun () -> Array.pick (fun _ -> None) (List.toArray Inputs.several) : int),
            (fun () -> List.pick (fun _ -> None) Inputs.several : int),
            (fun () -> ops.Pick (fun _ -> ValueNone) (ops.OfSeq Inputs.several)),
            "pick without a choice must throw"
        )

    [<TestMethod; TestCategory(nameof FlatList.tryFind)>]
    member this.``tryFind returns the first element that satisfies the predicate or nothing`` () =
        this.ForEach (
            Inputs.all,
            fun collection elements ->
                for predicate in [ isEven; isNegative; never ] do
                    Assert.AreEqual<int voption>(
                        List.tryFind predicate elements |> ValueOption.ofOption,
                        ops.TryFind predicate collection,
                        $"%s{ops.Name}.tryFind in %A{elements}"
                    )
        )

    [<TestMethod; TestCategory(nameof FlatList.tryFindBack)>]
    member this.``tryFindBack returns the last element that satisfies the predicate or nothing`` () =
        this.ForEach (
            Inputs.all,
            fun collection elements ->
                for predicate in [ isEven; isNegative; never ] do
                    Assert.AreEqual<int voption>(
                        List.tryFindBack predicate elements |> ValueOption.ofOption,
                        ops.TryFindBack predicate collection,
                        $"%s{ops.Name}.tryFindBack in %A{elements}"
                    )
        )

    [<TestMethod; TestCategory(nameof FlatList.tryFindIndex)>]
    member this.``tryFindIndex returns the index of the first element that satisfies the predicate or nothing`` () =
        this.ForEach (
            Inputs.all,
            fun collection elements ->
                for predicate in [ isEven; isNegative; never ] do
                    Assert.AreEqual<int voption>(
                        List.tryFindIndex predicate elements |> ValueOption.ofOption,
                        ops.TryFindIndex predicate collection,
                        $"%s{ops.Name}.tryFindIndex in %A{elements}"
                    )
        )

    [<TestMethod; TestCategory(nameof FlatList.tryFindIndexBack)>]
    member this.``tryFindIndexBack returns the index of the last element that satisfies the predicate or nothing`` () =
        this.ForEach (
            Inputs.all,
            fun collection elements ->
                for predicate in [ isEven; isNegative; never ] do
                    Assert.AreEqual<int voption>(
                        List.tryFindIndexBack predicate elements
                        |> ValueOption.ofOption,
                        ops.TryFindIndexBack predicate collection,
                        $"%s{ops.Name}.tryFindIndexBack in %A{elements}"
                    )
        )

    [<TestMethod; TestCategory(nameof FlatList.tryPick)>]
    member this.``tryPick returns the first chosen value or nothing`` () =
        this.ForEach (
            Inputs.all,
            fun collection elements ->
                Assert.AreEqual<int voption>(
                    List.tryPick negatedIfNegativeOption elements
                    |> ValueOption.ofOption,
                    ops.TryPick negatedIfNegative collection,
                    $"%s{ops.Name}.tryPick in %A{elements}"
                )
        )
