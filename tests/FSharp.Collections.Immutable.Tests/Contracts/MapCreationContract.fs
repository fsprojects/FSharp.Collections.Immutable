namespace FSharp.Collections.Immutable.Tests

open System.Collections.Generic
open Microsoft.VisualStudio.TestTools.UnitTesting

/// <summary>
/// The contract of the creation functions of the concrete map modules and of their comparer structs: <c>HashMap</c>,
/// <c>SortedMap</c> and <c>FrozenMap</c> (ADR 0001, Appendix B, D12 and D13).
/// </summary>
/// <remarks>
/// <para>
/// <c>ofArray</c>, <c>ofList</c> and <c>ofSeq</c> take key/value pairs, reference tuples and struct tuples, and have the
/// semantics of the FSharp.Core <c>Map</c> module in every shape: a later binding of a key wins. Their tests run once per
/// shape and input. <c>ofSeqBy</c> wraps the BCL, which handles a repeated key differently in the immutable and the frozen
/// dictionaries, so the contract runs it on distinct keys and the collections test the repeated keys.
/// </para>
/// <para>
/// Every created map must look its keys up with the key comparer of the functions under test, which
/// <see cref="P:FSharp.Collections.Immutable.Tests.MapCreationOps`1.EquivalentKey"/> describes.
/// </para>
/// </remarks>
[<AbstractClass>]
type MapCreationContract<'M> (ops : MapCreationOps<'M>) =
    inherit MapContractBase<'M> (ops.Name, ops.ParamName, ops.OfSeq, ops.ToArray)

    /// The bindings of a map of the module as key/value pairs, the shape that every map enumerates.
    static let asKeyValuePairs (table : 'M) =
        match box table with
        | :? seq<KeyValuePair<int, string>> as pairs -> pairs
        | _ -> failwith $"%s{typeof<'M>.Name} must enumerate key/value pairs"

    /// Every shape with every input, the bindings that repeat keys included: the rows of ofArray, ofList and ofSeq. The
    /// rows of every property are built on each read.
    static member ShapesAndBindings = seq {
        for shape in BindingShape.All do
            for input in
                seq {
                    yield! MapInputs.All
                    MapInputs.RepeatedKeys
                } do
                struct (shape, input)
    }

    /// The values of ofSeqBy, whose lengths are their keys.
    static member Values : TestDataRow<string[]> seq = seq {
        TestDataRow<string[]> [||]
        TestDataRow<string[]> [| "one" |]
        TestDataRow<string[]> [| "a"; "three"; "be"; "seven!" |]
    }

    /// Creates a map of the bindings with ofArray of a shape and changes the array afterwards.
    static member private CreateThenChange (bind : struct (int * string) -> 'Binding, ofArray : 'Binding array -> 'M) =
        let elements = MapInputs.Several |> Array.map bind
        let table = ofArray elements
        elements[0] <- bind (struct (1000, "thousand"))
        table

    /// Asserts that a created map has the bindings of an FSharp.Core map and finds every key with its key comparer.
    member private this.AreCreated (expected : Map<int, string>, table : 'M, rule : string) =
        this.AreBindings (expected, ops.ToArray table, rule)

        for key in Map.keys expected do
            Assert.IsTrue (
                ops.ContainsKey (ops.EquivalentKey key) table,
                $"%s{ops.Name}: the map must find %d{ops.EquivalentKey key} with its key comparer"
            )

    /// Creates a map with ofArray of a shape, from bindings given as struct tuples.
    member private _.OfArray (shape : BindingShape, bindings : struct (int * string) array) =
        match shape with
        | BindingShape.KeyValuePairs -> ops.KeyValuePairs.OfArray (Array.map MapInputs.ToKeyValuePair bindings)
        | BindingShape.Tuples -> ops.Tuples.OfArray (Array.map MapInputs.ToTuple bindings)
        | BindingShape.StructTuples -> ops.StructTuples.OfArray bindings

    /// Creates a map with ofList of a shape, from bindings given as struct tuples; the list is the input of ofList.
    member private _.OfList (shape : BindingShape, bindings : struct (int * string) array) =
        match shape with
        | BindingShape.KeyValuePairs ->
            ops.KeyValuePairs.OfList (
                bindings
                |> Array.map MapInputs.ToKeyValuePair
                |> List.ofArray
            )
        | BindingShape.Tuples -> ops.Tuples.OfList (bindings |> Array.map MapInputs.ToTuple |> List.ofArray)
        | BindingShape.StructTuples -> ops.StructTuples.OfList (List.ofArray bindings)

    /// Creates a map with ofSeq of a shape, from a lazy sequence of bindings given as struct tuples.
    member private _.OfSeq (shape : BindingShape, bindings : struct (int * string) array) =
        match shape with
        | BindingShape.KeyValuePairs -> ops.KeyValuePairs.OfSeq (Seq.map MapInputs.ToKeyValuePair bindings)
        | BindingShape.Tuples -> ops.Tuples.OfSeq (Seq.map MapInputs.ToTuple bindings)
        | BindingShape.StructTuples -> ops.StructTuples.OfSeq (Seq.map id bindings)

    [<TestMethod>]
    member _.``empty has no bindings`` () =
        Assert.IsEmpty (ops.ToArray (ops.Empty ()), $"%s{ops.Name}.empty must have no bindings")

    [<TestMethod; DynamicData(nameof MapCreationContract<'M>.ShapesAndBindings)>]
    member this.``ofArray keeps the last binding of each key as Map does``
        (shape : BindingShape, input : struct (int * string) array)
        =
        this.AreCreated (
            Map.ofArray (Array.map MapInputs.ToTuple input),
            this.OfArray (shape, input),
            "ofArray must keep the last binding of each key, as Map.ofArray does"
        )

    [<TestMethod; DynamicData(nameof BindingShape.All, typeof<BindingShape>)>]
    member _.``ofArray copies the bindings`` (shape : BindingShape) =
        let table =
            match shape with
            | BindingShape.KeyValuePairs ->
                MapCreationContract<'M>.CreateThenChange(MapInputs.ToKeyValuePair, ops.KeyValuePairs.OfArray)
            | BindingShape.Tuples -> MapCreationContract<'M>.CreateThenChange(MapInputs.ToTuple, ops.Tuples.OfArray)
            | BindingShape.StructTuples -> MapCreationContract<'M>.CreateThenChange(id, ops.StructTuples.OfArray)

        Assert.IsFalse (ops.ContainsKey 1000 table, $"%s{ops.Name}.ofArray must copy the bindings")

    [<TestMethod; DynamicData(nameof MapCreationContract<'M>.ShapesAndBindings)>]
    member this.``ofList keeps the last binding of each key as Map does``
        (shape : BindingShape, input : struct (int * string) array)
        =
        this.AreCreated (
            Map.ofList (input |> Array.map MapInputs.ToTuple |> List.ofArray),
            this.OfList (shape, input),
            "ofList must keep the last binding of each key, as Map.ofList does"
        )

    [<TestMethod; DynamicData(nameof MapCreationContract<'M>.ShapesAndBindings)>]
    member this.``ofSeq keeps the last binding of each key as Map does``
        (shape : BindingShape, input : struct (int * string) array)
        =
        this.AreCreated (
            Map.ofSeq (Seq.map MapInputs.ToTuple input),
            this.OfSeq (shape, input),
            "ofSeq must keep the last binding of each key, as Map.ofSeq does"
        )

    [<TestMethod; DynamicData(nameof MapInputs.All, typeof<MapInputs>)>]
    member this.``ofSeq takes the key value pairs of a dictionary`` (input : struct (int * string) array) =
        this.AreCreated (
            MapInputs.ToMap input,
            ops.KeyValuePairs.OfSeq (dict (Array.map MapInputs.ToTuple input)),
            "ofSeq must take the key/value pairs of a dictionary"
        )

    [<TestMethod; DynamicData(nameof MapInputs.All, typeof<MapInputs>)>]
    member this.``ofSeq takes the key value pairs of an F# map`` (input : struct (int * string) array) =
        let expected = MapInputs.ToMap input
        this.AreCreated (expected, ops.KeyValuePairs.OfSeq expected, "ofSeq must take the key/value pairs of an F# map")

    [<TestMethod; DynamicData(nameof MapInputs.All, typeof<MapInputs>)>]
    member this.``ofSeq takes the key value pairs of a map of the module`` (input : struct (int * string) array) =
        this.AreCreated (
            MapInputs.ToMap input,
            ops.KeyValuePairs.OfSeq (asKeyValuePairs (ops.OfSeq input)),
            "ofSeq must take the key/value pairs of a map of the module"
        )

    [<TestMethod; DynamicData(nameof MapCreationContract<'M>.Values)>]
    member this.``ofSeqBy binds the values to the keys of the projection`` (values : string array) =
        this.AreCreated (
            Map.ofArray (Array.map (fun (value : string) -> value.Length, value) values),
            ops.OfSeqBy String.length values,
            "ofSeqBy must bind every value to the key that the projection returns for it"
        )

    [<TestMethod>]
    member this.``singleton has one binding`` () =
        this.AreCreated (Map.add 42 "answer" Map.empty, ops.Singleton 42 "answer", "singleton must have one binding")
