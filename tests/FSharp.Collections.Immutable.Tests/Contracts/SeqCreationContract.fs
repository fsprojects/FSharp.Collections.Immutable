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

    [<TestMethod>]
    member _.``empty has no elements`` () =
        Assert.IsEmpty (ops.ToArray (ops.Empty ()), $"%s{ops.Name}.empty must have no elements")

    [<TestMethod; DataRow 0; DataRow 1; DataRow 5>]
    member _.``init creates the elements in index order`` (count : int) =
        CollectionAssert.AreEqual (
            Array.init count (fun index -> index * index),
            ops.ToArray (ops.Init count (fun index -> index * index)),
            $"%s{ops.Name}.init must create the elements in index order"
        )

    [<TestMethod>]
    member this.``init throws for a negative count as FSharp.Core does`` () =
        this.ThrowsLikeFSharpCore (
            (fun () -> Array.init -1 id),
            (fun () -> List.init -1 id),
            (fun () -> ops.Init -1 id),
            "init -1 must throw"
        )

    [<TestMethod; DynamicData(nameof Inputs.All, typeof<Inputs>)>]
    member _.``ofArray copies the elements in order`` (input : int array) =
        // The test changes the source after the creation, so it changes a copy of its row
        let source = Array.copy input
        let collection = ops.OfArray source

        if source.Length > 0 then
            source[0] <- 1000

        CollectionAssert.AreEqual (input, ops.ToArray collection, $"%s{ops.Name}.ofArray must copy the elements in order")

    [<TestMethod>]
    member this.``ofArray throws for a null array as FSharp.Core does`` () =
        let source = Unchecked.defaultof<int array>

        // List.ofArray reads a null array without a check and fails with NullReferenceException, while a null input throws
        // ArgumentNullException (ADR 0001, D6), as Array.toList does for the same conversion
        this.ThrowsLikeFSharpCore (
            (fun () -> Array.copy source),
            (fun () -> Array.toList source),
            (fun () -> ops.OfArray source),
            "ofArray of a null array must throw"
        )

    [<TestMethod; DynamicData(nameof Inputs.All, typeof<Inputs>)>]
    member _.``ofList keeps the elements in order`` (input : int array) =
        CollectionAssert.AreEqual (
            input,
            ops.ToArray (ops.OfList (List.ofArray input)),
            $"%s{ops.Name}.ofList must keep the elements in order"
        )

    [<TestMethod; DynamicData(nameof Inputs.All, typeof<Inputs>)>]
    member _.``ofReadOnlySpan copies the elements in order`` (input : int array) =
        CollectionAssert.AreEqual (
            input,
            ops.ToArray (ops.OfReadOnlySpan input),
            $"%s{ops.Name}.ofReadOnlySpan must copy the elements in order"
        )

    [<TestMethod; DynamicData(nameof Inputs.All, typeof<Inputs>)>]
    member _.``ofSeq keeps the elements in order`` (input : int array) =
        CollectionAssert.AreEqual (
            input,
            ops.ToArray (ops.OfSeq (List.ofArray input)),
            $"%s{ops.Name}.ofSeq of a list must keep the elements in order"
        )

        CollectionAssert.AreEqual (
            input,
            ops.ToArray (ops.OfSeq input),
            $"%s{ops.Name}.ofSeq of an array must keep the elements in order"
        )

        CollectionAssert.AreEqual (
            input,
            ops.ToArray (ops.OfSeq (Seq.map id input)),
            $"%s{ops.Name}.ofSeq of a lazy sequence must keep the elements in order"
        )

    [<TestMethod>]
    member this.``ofSeq throws for a null sequence as FSharp.Core does`` () =
        let source = Unchecked.defaultof<seq<int>>

        this.ThrowsLikeFSharpCore (
            (fun () -> Array.ofSeq source),
            (fun () -> List.ofSeq source),
            (fun () -> ops.OfSeq source),
            "ofSeq of a null sequence must throw"
        )

    [<TestMethod>]
    member _.``ofSeq rejects a default flat list given as a sequence as it rejects a null sequence`` () =
        let nullSource = Unchecked.defaultof<seq<int>>

        // The default flat list is a boxed struct, not null: it fails with InvalidOperationException when it is enumerated
        ExceptionAssert.Same (
            id,
            (fun () -> ops.OfSeq nullSource |> ignore),
            (fun () -> ops.OfSeq (Inputs.DefaultFlatListAsSeq<int>()) |> ignore),
            $"%s{ops.Name}.ofSeq of a default flat list must throw what it throws for a null sequence"
        )

    [<TestMethod; DynamicData(nameof Inputs.All, typeof<Inputs>)>]
    member _.``ofSpan copies the elements in order`` (input : int array) =
        CollectionAssert.AreEqual (input, ops.ToArray (ops.OfSpan input), $"%s{ops.Name}.ofSpan must copy the elements in order")

    [<TestMethod; DataRow 0; DataRow 1; DataRow 4>]
    member _.``replicate repeats the value`` (count : int) =
        CollectionAssert.AreEqual (
            Array.replicate count 7,
            ops.ToArray (ops.Replicate count 7),
            $"%s{ops.Name}.replicate must repeat the value"
        )

    [<TestMethod>]
    member this.``replicate throws for a negative count as FSharp.Core does`` () =
        this.ThrowsLikeFSharpCore (
            (fun () -> Array.replicate -1 7),
            (fun () -> List.replicate -1 7),
            (fun () -> ops.Replicate -1 7),
            "replicate -1 must throw"
        )

    [<TestMethod>]
    member _.``singleton has one element`` () =
        CollectionAssert.AreEqual ([| 42 |], ops.ToArray (ops.Singleton 42), $"%s{ops.Name}.singleton must have one element")

    [<TestMethod; DataRow 0; DataRow 1; DataRow 5>]
    member _.``unfold generates the elements until the generator stops`` (start : int) =
        let generator (state : int) =
            if state > 0 then
                ValueSome (struct (state * 10, state - 1))
            else
                ValueNone

        let generatorOption (state : int) = if state > 0 then Some (state * 10, state - 1) else None

        CollectionAssert.AreEqual (
            Array.unfold generatorOption start,
            ops.ToArray (ops.Unfold generator start),
            $"%s{ops.Name}.unfold must generate the elements until the generator stops"
        )
