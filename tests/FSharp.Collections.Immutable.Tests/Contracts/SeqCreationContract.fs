namespace FSharp.Collections.Immutable.Tests

open FSharp.Collections.Immutable
open Microsoft.VisualStudio.TestTools.UnitTesting

/// <summary>
/// The contract of the creation functions of the concrete sequence-like modules: <c>FlatList</c>, <c>IndexedList</c>,
/// <c>Queue</c> and <c>Stack</c> (ADR 0001, Appendix A).
/// </summary>
[<AbstractClass>]
type SeqCreationContract<'C> (ops : SeqCreationOps<'C>) =
    inherit ContractBase (ops.Name, ops.Mirror, ops.ParamName)

    [<TestMethod; TestCategory(nameof FlatList.empty)>]
    member _.``empty has no elements`` () =
        Assert.IsEmpty (ops.ToList (ops.Empty ()), $"%s{ops.Name}.empty must have no elements")

    [<TestMethod; TestCategory(nameof FlatList.init)>]
    member _.``init creates the elements in index order`` () =
        for count in [ 0; 1; 5 ] do
            Assert.AreEqual<int list>(
                List.init count (fun index -> index * index),
                ops.ToList (ops.Init count (fun index -> index * index)),
                $"%s{ops.Name}.init %d{count}"
            )

    [<TestMethod; TestCategory(nameof FlatList.init)>]
    member this.``init throws for a negative count as FSharp.Core does`` () =
        this.ThrowsLikeFSharpCore (
            (fun () -> Array.init -1 id),
            (fun () -> List.init -1 id),
            (fun () -> ops.Init -1 id),
            "init -1 must throw"
        )

    [<TestMethod; TestCategory(nameof FlatList.ofArray)>]
    member _.``ofArray copies the elements in order`` () =
        for input in Inputs.all do
            let source = List.toArray input
            let collection = ops.OfArray source

            if source.Length > 0 then
                source[0] <- 1000

            Assert.AreEqual<int list>(input, ops.ToList collection, $"%s{ops.Name}.ofArray must copy %A{input}")

    [<TestMethod; TestCategory(nameof FlatList.ofArray)>]
    member this.``ofArray throws for a null array as FSharp.Core does`` () =
        let source = Unchecked.defaultof<int array>

        this.ThrowsLikeFSharpCore (
            (fun () -> Array.copy source),
            (fun () -> List.ofArray source),
            (fun () -> ops.OfArray source),
            "ofArray of a null array must throw"
        )

    [<TestMethod; TestCategory(nameof FlatList.ofList)>]
    member _.``ofList keeps the elements in order`` () =
        for input in Inputs.all do
            Assert.AreEqual<int list>(input, ops.ToList (ops.OfList input), $"%s{ops.Name}.ofList of %A{input}")

    [<TestMethod; TestCategory(nameof FlatList.ofReadOnlySpan)>]
    member _.``ofReadOnlySpan copies the elements in order`` () =
        for input in Inputs.all do
            Assert.AreEqual<int list>(
                input,
                ops.ToList (ops.OfReadOnlySpan (List.toArray input)),
                $"%s{ops.Name}.ofReadOnlySpan of %A{input}"
            )

    [<TestMethod; TestCategory(nameof FlatList.ofSeq)>]
    member _.``ofSeq keeps the elements in order`` () =
        for input in Inputs.all do
            Assert.AreEqual<int list>(input, ops.ToList (ops.OfSeq input), $"%s{ops.Name}.ofSeq of the list %A{input}")

            Assert.AreEqual<int list>(
                input,
                ops.ToList (ops.OfSeq (List.toArray input)),
                $"%s{ops.Name}.ofSeq of the array %A{input}"
            )

            Assert.AreEqual<int list>(
                input,
                ops.ToList (ops.OfSeq (Seq.map id input)),
                $"%s{ops.Name}.ofSeq of a lazy sequence of %A{input}"
            )

    [<TestMethod; TestCategory(nameof FlatList.ofSeq)>]
    member this.``ofSeq throws for a null sequence as FSharp.Core does`` () =
        let source = Unchecked.defaultof<seq<int>>

        this.ThrowsLikeFSharpCore (
            (fun () -> Array.ofSeq source),
            (fun () -> List.ofSeq source),
            (fun () -> ops.OfSeq source),
            "ofSeq of a null sequence must throw"
        )

    [<TestMethod; TestCategory(nameof FlatList.ofSpan)>]
    member _.``ofSpan copies the elements in order`` () =
        for input in Inputs.all do
            Assert.AreEqual<int list>(input, ops.ToList (ops.OfSpan (List.toArray input)), $"%s{ops.Name}.ofSpan of %A{input}")

    [<TestMethod; TestCategory(nameof FlatList.replicate)>]
    member _.``replicate repeats the value`` () =
        for count in [ 0; 1; 4 ] do
            Assert.AreEqual<int list>(
                List.replicate count 7,
                ops.ToList (ops.Replicate count 7),
                $"%s{ops.Name}.replicate %d{count}"
            )

    [<TestMethod; TestCategory(nameof FlatList.replicate)>]
    member this.``replicate throws for a negative count as FSharp.Core does`` () =
        this.ThrowsLikeFSharpCore (
            (fun () -> Array.replicate -1 7),
            (fun () -> List.replicate -1 7),
            (fun () -> ops.Replicate -1 7),
            "replicate -1 must throw"
        )

    [<TestMethod; TestCategory(nameof FlatList.singleton)>]
    member _.``singleton has one element`` () =
        Assert.AreEqual<int list>([ 42 ], ops.ToList (ops.Singleton 42), $"%s{ops.Name}.singleton must have one element")

    [<TestMethod; TestCategory(nameof FlatList.unfold)>]
    member _.``unfold generates the elements until the generator stops`` () =
        let generator (state : int) =
            if state > 0 then
                ValueSome (struct (state * 10, state - 1))
            else
                ValueNone
        let generatorOption (state : int) = if state > 0 then Some (state * 10, state - 1) else None

        for start in [ 0; 1; 5 ] do
            Assert.AreEqual<int list>(
                List.unfold generatorOption start,
                ops.ToList (ops.Unfold generator start),
                $"%s{ops.Name}.unfold from %d{start}"
            )
