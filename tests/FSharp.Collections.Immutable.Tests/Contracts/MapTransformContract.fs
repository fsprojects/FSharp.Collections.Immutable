namespace FSharp.Collections.Immutable.Tests

open Microsoft.VisualStudio.TestTools.UnitTesting

/// <summary>
/// The contract of the whole-map transformations that every map module provides: <c>HashMap</c>, <c>SortedMap</c>,
/// <c>IMap</c> and <c>FrozenMap</c> (ADR 0001, Appendix B).
/// </summary>
/// <remarks>
/// The comparers of the results are collection-specific and have their own tests.
/// </remarks>
[<AbstractClass>]
type MapTransformContract<'M> (ops : MapTransformOps<'M>) =
    inherit MapContractBase<'M> (ops.Name, ops.ParamName, ops.OfSeq, ops.ToArray)

    /// A chooser that returns a new value for the bindings of long values only.
    static let chooser (key : int) (value : string) =
        if value.Length > 4 then
            ValueSome (key * 10 + value.Length)
        else
            ValueNone

    /// Every input with each predicate of filter and partition: two that some bindings satisfy, one that no binding
    /// satisfies and one that every binding satisfies.
    static member InputsAndPredicates = seq {
        for input in MapInputs.All do
            for predicate in
                [|
                    BindingPredicate.HasLongValue
                    BindingPredicate.HasNegativeKey
                    BindingPredicate.Never
                    BindingPredicate.Always
                |] do
                struct (input, predicate)
    }

    [<TestMethod; DynamicData(nameof MapInputs.All, typeof<MapInputs>)>]
    member this.``choose keeps the chosen values under their keys`` (input : struct (int * string) array) =
        let struct (table, _) = this.Create input

        let expected =
            MapInputs.ToMap input
            |> Map.toSeq
            |> Seq.choose (fun (key, value) ->
                chooser key value
                |> ValueOption.map (fun chosen -> key, chosen)
                |> ValueOption.toOption
            )
            |> Map.ofSeq

        this.AreBindings (expected, ops.Choose chooser table, "choose must keep the chosen values under their keys")

    [<TestMethod; DynamicData(nameof MapTransformContract<'M>.InputsAndPredicates)>]
    member this.``filter keeps the bindings that satisfy the predicate as Map does``
        (input : struct (int * string) array, predicate : BindingPredicate)
        =
        let struct (table, _) = this.Create input

        this.AreBindings (
            Map.filter predicate.Matches (MapInputs.ToMap input),
            ops.ToArray (ops.Filter predicate.Matches table),
            "filter must keep the bindings that satisfy the predicate"
        )

    [<TestMethod; DynamicData(nameof MapInputs.All, typeof<MapInputs>)>]
    member this.``map transforms the values under their keys as Map does`` (input : struct (int * string) array) =
        let struct (table, _) = this.Create input
        let mapping (key : int) (value : string) = key * 10 + value.Length

        this.AreBindings (
            Map.map mapping (MapInputs.ToMap input),
            ops.Map mapping table,
            "map must transform the values under their keys"
        )

    [<TestMethod; DynamicData(nameof MapTransformContract<'M>.InputsAndPredicates)>]
    member this.``partition splits the bindings by the predicate as Map does``
        (input : struct (int * string) array, predicate : BindingPredicate)
        =
        let struct (table, _) = this.Create input
        let expectedAccepted, expectedRejected = Map.partition predicate.Matches (MapInputs.ToMap input)
        let struct (accepted, rejected) = ops.Partition predicate.Matches table
        this.AreBindings (expectedAccepted, ops.ToArray accepted, "partition must accept the bindings that satisfy the predicate")
        this.AreBindings (expectedRejected, ops.ToArray rejected, "partition must reject the other bindings")

    [<TestMethod>]
    member _.``partition calls the predicate once per binding`` () =
        let table = ops.OfSeq MapInputs.Several
        let visited = ResizeArray<int>()

        ops.Partition
            (fun key _ ->
                visited.Add key
                key > 0
            )
            table
        |> ignore

        CollectionAssert.AreEqual (
            MapInputs.KeysOf (ops.ToArray table),
            visited.ToArray (),
            $"%s{ops.Name}.partition must test each binding once, in the order of enumeration"
        )
