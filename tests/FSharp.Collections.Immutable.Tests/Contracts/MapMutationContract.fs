namespace FSharp.Collections.Immutable.Tests

open System
open Microsoft.VisualStudio.TestTools.UnitTesting

/// <summary>
/// The functions that change one binding in the tests of <c>change</c>, as the cases of their rows.
/// </summary>
/// <remarks>
/// MSTest names a row after its values, and a function has no readable name, so a row holds a case and the test takes
/// the function of the case.
/// </remarks>
[<RequireQualifiedAccess>]
type BindingChange =
    /// Removes the binding.
    | KeepsNothing
    /// Sets the value "changed".
    | SetsAValue
    /// Appends "!" to the current value and keeps a missing value missing.
    | ExtendsTheValue
    /// Keeps the current value and adds the value "new" to a missing key.
    | AddsAMissingValue

    /// Every change, as the rows of the tests that run once per change.
    static member All = seq {
        BindingChange.KeepsNothing
        BindingChange.SetsAValue
        BindingChange.ExtendsTheValue
        BindingChange.AddsAMissingValue
    }

    /// The new value of a binding, or nothing to remove it, for its current value or nothing.
    member this.Apply (current : string voption) =
        match this with
        | BindingChange.KeepsNothing -> ValueNone
        | BindingChange.SetsAValue -> ValueSome "changed"
        | BindingChange.ExtendsTheValue -> current |> ValueOption.map (fun value -> value + "!")
        | BindingChange.AddsAMissingValue -> current |> ValueOption.orElse (ValueSome "new")

/// <summary>
/// The contract of the incremental changes of the map modules that have them: <c>HashMap</c>, <c>SortedMap</c> and
/// <c>IMap</c> (ADR 0001, Appendix B).
/// </summary>
/// <remarks>
/// <para>
/// <c>change</c>, <c>remove</c>, <c>setItem</c> and <c>setItems</c> have the semantics of the FSharp.Core <c>Map</c> module
/// where <c>Map</c> has them, and <c>add</c>, <c>addRange</c> and <c>removeRange</c> wrap the immutable dictionaries of the
/// BCL, which share their semantics: <c>add</c> and <c>addRange</c> reject another value of a key and accept an equal
/// binding, <c>setItem</c> and <c>setItems</c> replace the value (ADR 0001, D14). <c>Map.add</c> replaces the value, so the
/// tests compare <c>add</c> with it only for keys without a binding.
/// </para>
/// <para>
/// <c>addRange</c> and <c>setItems</c> take key/value pairs, and the nested modules <c>Tuple</c> and <c>ValueTuple</c> take
/// reference and struct tuples (ADR 0001, D13). Their tests run once per shape and input.
/// </para>
/// </remarks>
[<AbstractClass>]
type MapMutationContract<'M> (ops : MapMutationOps<'M>) =
    inherit MapContractBase<'M> (ops.Name, ops.ParamName, ops.OfSeq, ops.ToArray)

    /// The keys of a row of an input: each key of the input and each key that no input has.
    static let keysAndAbsentKeys (input : struct (int * string) array) =
        Array.append (MapInputs.KeysOf input) MapInputs.AbsentKeys

    /// Every input with each of its keys and each key that no input has: the rows of add, remove and setItem. The rows of
    /// every property are built on each read.
    static member InputsAndKeys = seq {
        for input in MapInputs.All do
            for key in keysAndAbsentKeys input do
                struct (input, key)
    }

    /// Every input with each key that no input has: the rows of add of a key without a binding.
    static member InputsAndNewKeys = seq {
        for input in MapInputs.All do
            for key in MapInputs.AbsentKeys do
                struct (input, key)
    }

    /// Every input that has bindings with each of its bindings: the rows of add of a key with a binding.
    static member NonEmptyInputsAndBindings = seq {
        for input in MapInputs.NonEmpty do
            for struct (key, value) in input do
                struct (input, key, value)
    }

    /// Every input with each of its keys, each key that no input has and each change: the rows of change.
    static member InputsKeysAndChanges = seq {
        for input in MapInputs.All do
            for key in keysAndAbsentKeys input do
                for change in BindingChange.All do
                    struct (input, key, change)
    }

    /// Every shape with every input: the rows of addRange and setItems.
    static member ShapesAndBindings = seq {
        for shape in BindingShape.All do
            for input in MapInputs.All do
                struct (shape, input)
    }

    /// Every shape with every input that has bindings.
    static member ShapesAndNonEmptyBindings = seq {
        for shape in BindingShape.All do
            for input in MapInputs.NonEmpty do
                struct (shape, input)
    }

    /// Adds bindings given as struct tuples with addRange of a shape.
    member private _.AddRange (shape : BindingShape, bindings : struct (int * string) array, table : 'M) =
        match shape with
        | BindingShape.KeyValuePairs -> ops.KeyValuePairs.AddRange (Array.map MapInputs.ToKeyValuePair bindings) table
        | BindingShape.Tuples -> ops.Tuples.AddRange (Array.map MapInputs.ToTuple bindings) table
        | BindingShape.StructTuples -> ops.StructTuples.AddRange bindings table

    /// Sets bindings given as struct tuples with setItems of a shape.
    member private _.SetItems (shape : BindingShape, bindings : struct (int * string) array, table : 'M) =
        match shape with
        | BindingShape.KeyValuePairs -> ops.KeyValuePairs.SetItems (Array.map MapInputs.ToKeyValuePair bindings) table
        | BindingShape.Tuples -> ops.Tuples.SetItems (Array.map MapInputs.ToTuple bindings) table
        | BindingShape.StructTuples -> ops.StructTuples.SetItems bindings table

    [<TestMethod; DynamicData(nameof MapMutationContract<'M>.InputsAndNewKeys)>]
    member this.``add adds a key without a binding as Map does`` (input : struct (int * string) array, key : int) =
        let struct (table, _) = this.Create input

        this.AreBindings (
            Map.add key "added" (MapInputs.ToMap input),
            ops.ToArray (ops.Add key "added" table),
            "add must add a key without a binding"
        )

    [<TestMethod; DynamicData(nameof MapMutationContract<'M>.NonEmptyInputsAndBindings)>]
    member _.``add returns the input itself for a binding that the map has as the BCL Add does``
        (input : struct (int * string) array, key : int, value : string)
        =
        let table = ops.OfSeq input

        Assert.AreSame<obj>(
            box table,
            box (ops.Add key value table),
            $"%s{ops.Name}.add must return the input itself for a binding that the map has"
        )

    [<TestMethod; DynamicData(nameof MapMutationContract<'M>.NonEmptyInputsAndBindings)>]
    member _.``add throws for a key that is bound to another value as the BCL Add does``
        (input : struct (int * string) array, key : int, value : string)
        =
        let table = ops.OfSeq input

        Assert.ThrowsExactly<ArgumentException>(
            Action (fun () -> ops.Add key (value + "!") table |> ignore),
            $"%s{ops.Name}.add must reject another value of a key of the map"
        )
        |> ignore

    [<TestMethod; DynamicData(nameof MapMutationContract<'M>.ShapesAndBindings)>]
    member this.``addRange adds the bindings of new keys`` (shape : BindingShape, input : struct (int * string) array) =
        let pairs = [| struct (1, "one"); struct (-5, "minus five"); struct (100, "hundred") |]

        let expected =
            (MapInputs.ToMap input, pairs)
            ||> Array.fold (fun map struct (key, value) -> Map.add key value map)

        this.AreBindings (
            expected,
            ops.ToArray (this.AddRange (shape, pairs, ops.OfSeq input)),
            "addRange must add the bindings of new keys"
        )

    [<TestMethod; DynamicData(nameof MapMutationContract<'M>.ShapesAndNonEmptyBindings)>]
    member this.``addRange accepts a binding that the map already has``
        (shape : BindingShape, input : struct (int * string) array)
        =
        this.AreBindings (
            MapInputs.ToMap input,
            ops.ToArray (this.AddRange (shape, input, ops.OfSeq input)),
            "addRange must accept the bindings that the map already has"
        )

    [<TestMethod; DynamicData(nameof BindingShape.All, typeof<BindingShape>)>]
    member this.``addRange throws for another value of a key as the BCL AddRange does`` (shape : BindingShape) =
        let table = ops.OfSeq MapInputs.Several

        Assert.ThrowsExactly<ArgumentException>(
            Action (fun () ->
                this.AddRange (shape, [| struct (5, "other") |], table)
                |> ignore
            ),
            $"%s{ops.Name}.addRange must reject another value of a key of the map"
        )
        |> ignore

        Assert.ThrowsExactly<ArgumentException>(
            Action (fun () ->
                this.AddRange (shape, [| struct (1, "one"); struct (1, "uno") |], table)
                |> ignore
            ),
            $"%s{ops.Name}.addRange must reject two values of a new key"
        )
        |> ignore

    [<TestMethod; DynamicData(nameof MapMutationContract<'M>.InputsKeysAndChanges)>]
    member this.``change adds, replaces and removes a binding as Map does``
        (input : struct (int * string) array, key : int, change : BindingChange)
        =
        let struct (table, _) = this.Create input

        let expected =
            Map.change
                key
                (fun current ->
                    change.Apply (ValueOption.ofOption current)
                    |> ValueOption.toOption
                )
                (MapInputs.ToMap input)

        this.AreBindings (
            expected,
            ops.ToArray (ops.Change key change.Apply table),
            "change must change the binding as Map.change does"
        )

    [<TestMethod; DataRow 5; DataRow 1>]
    member _.``change calls the function once with the current value`` (key : int) =
        let table = ops.OfSeq MapInputs.Several
        let expected =
            Map.tryFind key (MapInputs.ToMap MapInputs.Several)
            |> ValueOption.ofOption
        let received = ResizeArray<string voption>()

        ops.Change
            key
            (fun current ->
                received.Add current
                current
            )
            table
        |> ignore

        CollectionAssert.AreEqual ([| expected |], received.ToArray (), $"%s{ops.Name}.change must pass the current value once")

    [<TestMethod; DynamicData(nameof MapInputs.All, typeof<MapInputs>)>]
    member this.``clear removes every binding`` (input : struct (int * string) array) =
        let struct (table, _) = this.Create input
        Assert.IsEmpty (ops.ToArray (ops.Clear table), $"%s{ops.Name}.clear must remove every binding")

    [<TestMethod; DynamicData(nameof MapMutationContract<'M>.InputsAndKeys)>]
    member this.``remove removes the binding of a key as Map does`` (input : struct (int * string) array, key : int) =
        let struct (table, _) = this.Create input

        this.AreBindings (
            Map.remove key (MapInputs.ToMap input),
            ops.ToArray (ops.Remove key table),
            "remove must remove the binding of the key"
        )

    [<TestMethod; DynamicData(nameof MapInputs.All, typeof<MapInputs>)>]
    member this.``removeRange removes the bindings of keys and skips the other keys`` (input : struct (int * string) array) =
        let struct (table, _) = this.Create input
        let keys = [| 5; 1; -7; 100; 5 |]

        let expected =
            (MapInputs.ToMap input, keys)
            ||> Array.fold (fun map key -> Map.remove key map)

        this.AreBindings (expected, ops.ToArray (ops.RemoveRange keys table), "removeRange must remove the bindings of the keys")

    [<TestMethod; DynamicData(nameof MapMutationContract<'M>.InputsAndKeys)>]
    member this.``setItem adds a new key and replaces the value of an existing key``
        (input : struct (int * string) array, key : int)
        =
        let struct (table, _) = this.Create input

        this.AreBindings (
            Map.add key "set" (MapInputs.ToMap input),
            ops.ToArray (ops.SetItem key "set" table),
            "setItem must add a new key and replace the value of an existing key"
        )

    [<TestMethod; DynamicData(nameof MapMutationContract<'M>.ShapesAndBindings)>]
    member this.``setItems sets the pairs in order, so that a later pair of a key wins``
        (shape : BindingShape, input : struct (int * string) array)
        =
        let pairs = [| struct (5, "five again"); struct (1, "one"); struct (1, "uno"); struct (-3, "minus three") |]

        let expected =
            (MapInputs.ToMap input, pairs)
            ||> Array.fold (fun map struct (key, value) -> Map.add key value map)

        this.AreBindings (
            expected,
            ops.ToArray (this.SetItems (shape, pairs, ops.OfSeq input)),
            "setItems must set the bindings in order, so that a later binding of a key wins"
        )
