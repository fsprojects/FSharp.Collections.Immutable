namespace FSharp.Collections.Immutable.Tests

open System.Collections.Generic
open Microsoft.VisualStudio.TestTools.UnitTesting

/// <summary>
/// The predicates over the bindings of the map contracts, as the cases of their rows.
/// </summary>
/// <remarks>
/// MSTest names a row after its values, and a function has no readable name, so a row holds a case and the test takes
/// the function of the case.
/// </remarks>
[<RequireQualifiedAccess>]
type BindingPredicate =
    /// The bindings whose value is longer than four characters.
    | HasLongValue
    /// The bindings of negative keys.
    | HasNegativeKey
    /// The bindings of keys below a hundred, which are all the bindings of every input.
    | HasKeyBelowHundred
    /// Every binding.
    | Always
    /// No binding.
    | Never

    /// Tells whether a binding satisfies the predicate.
    member this.Matches (key : int) (value : string) =
        match this with
        | BindingPredicate.HasLongValue -> value.Length > 4
        | BindingPredicate.HasNegativeKey -> key < 0
        | BindingPredicate.HasKeyBelowHundred -> key < 100
        | BindingPredicate.Always -> true
        | BindingPredicate.Never -> false

/// <summary>
/// The helpers that the contract bases of the map modules share.
/// </summary>
/// <remarks>
/// <para>
/// Every map module mirrors the FSharp.Core <c>Map</c> module (ADR 0001, D6), so the map contracts take no
/// <see cref="T:FSharp.Collections.Immutable.Tests.Mirror"/>: they compare the exceptions with the <c>Map</c> oracle through
/// <c>ExceptionAssert.Same</c>, and <c>Mirror</c> stays about the sequence-like modules.
/// </para>
/// <para>
/// A hash map enumerates its bindings in an unspecified order. The contracts compare whole maps as bindings sorted by key,
/// and compute the expectations of the functions that visit the bindings one by one over the order in which the map under
/// test enumerates them, so that one test body serves hash, sorted and frozen maps.
/// </para>
/// </remarks>
/// <param name="name">The name of the module under test, for the messages of the assertions.</param>
/// <param name="paramName">Maps the name of a parameter of the FSharp.Core oracle to the name of the parameter of the module.</param>
/// <param name="ofSeq">Creates a map of the given bindings.</param>
/// <param name="toArray">The bindings in the order in which the map enumerates them.</param>
[<AbstractClass>]
type MapContractBase<'M>
    (
        name : string,
        paramName : string -> string,
        ofSeq : seq<struct (int * string)> -> 'M,
        toArray : 'M -> struct (int * string) array
    )
    =

    /// The map of the bindings of an input and its bindings in the order in which the map enumerates them.
    member _.Create (input : struct (int * string) array) =
        let table = ofSeq input
        struct (table, toArray table)

    /// Asserts that bindings, in any order, are the bindings of an FSharp.Core map.
    member _.AreBindings (expected : Map<int, 'T>, actual : struct (int * 'T) array, message : string) =
        CollectionAssert.AreEqual (MapInputs.OfMap expected, MapInputs.SortByKey actual, $"%s{name}: %s{message}")

    /// Asserts that a function of the module throws what the same function of the FSharp.Core Map module throws.
    member _.ThrowsLikeMap (oracle : unit -> 'A, actual : unit -> 'R, message : string) =
        ExceptionAssert.Same (paramName, oracle >> ignore, actual >> ignore, $"%s{name}: %s{message}")

/// <summary>
/// The contract of the reading functions that every map module provides: <c>HashMap</c>, <c>SortedMap</c>, <c>IMap</c>
/// and <c>FrozenMap</c> (ADR 0001, Appendix B).
/// </summary>
[<AbstractClass>]
type MapContract<'M> (ops : MapOps<'M>) =
    inherit MapContractBase<'M> (ops.Name, ops.ParamName, ops.OfSeq, ops.ToArray)

    /// A chooser that returns a value for the bindings of negative keys only.
    static let negatedIfNegative (key : int) (value : string) =
        if key < 0 then
            ValueSome (value.Length - key)
        else
            ValueNone

    /// A folder whose state records the order in which it saw the bindings.
    static let record (state : struct (int * string) array) (key : int) (value : string) =
        Array.append state [| struct (key, value) |]

    /// A folder from the end whose state records the order in which it saw the bindings: prepending from the last binding
    /// to the first one rebuilds the order of enumeration.
    static let recordBack (key : int) (value : string) (state : struct (int * string) array) =
        Array.append [| struct (key, value) |] state

    /// The bindings of a map of the module as struct tuples, in the order of its enumeration of key/value pairs: the
    /// observation that toArray is compared with.
    static let enumerated (table : 'M) =
        match box table with
        | :? seq<KeyValuePair<int, string>> as pairs ->
            pairs
            |> Seq.map (fun pair -> struct (pair.Key, pair.Value))
            |> Seq.toArray
        | _ -> failwith $"%s{typeof<'M>.Name} must enumerate key/value pairs"

    /// Every input that has bindings with each of its keys: the rows of the lookups that find a binding.
    static member NonEmptyInputsAndKeys = seq {
        for input in MapInputs.NonEmpty do
            for key in MapInputs.KeysOf input do
                struct (input, key)
    }

    /// Every input with each key that no input has: the rows of the lookups that fail.
    static member InputsAndAbsentKeys = seq {
        for input in MapInputs.All do
            for key in MapInputs.AbsentKeys do
                struct (input, key)
    }

    /// Every input with each of its keys and each key that no input has: the rows of the lookups that may find nothing.
    static member InputsAndKeys = seq {
        for input in MapInputs.All do
            for key in Array.append (MapInputs.KeysOf input) MapInputs.AbsentKeys do
                struct (input, key)
    }

    /// Every input with the probes of containsBinding: each of its bindings, each of its keys with another value, and each
    /// key that no input has with a value that an input has.
    static member InputsAndBindingProbes = seq {
        for input in MapInputs.All do
            for struct (key, value) in input do
                struct (input, key, value)
                struct (input, key, value + "!")

            for key in MapInputs.AbsentKeys do
                struct (input, key, "five")
    }

    /// Every input with each predicate of exists and tryFindKey: one that some bindings satisfy, one that other bindings
    /// satisfy and one that no binding satisfies.
    static member InputsAndPredicates = seq {
        for input in MapInputs.All do
            for predicate in [| BindingPredicate.HasLongValue; BindingPredicate.HasNegativeKey; BindingPredicate.Never |] do
                struct (input, predicate)
    }

    /// Every input with each predicate of forall: one that some bindings satisfy, one that every binding satisfies and one
    /// that no binding satisfies.
    static member InputsAndForallPredicates = seq {
        for input in MapInputs.All do
            for predicate in [| BindingPredicate.HasLongValue; BindingPredicate.HasKeyBelowHundred; BindingPredicate.Never |] do
                struct (input, predicate)
    }

    /// The predicates of findKey, which some bindings of the input with several bindings satisfy.
    static member FindKeyPredicates = seq {
        BindingPredicate.HasLongValue
        BindingPredicate.HasNegativeKey
    }

    // ---- Properties ----

    [<TestMethod; DynamicData(nameof MapInputs.All, typeof<MapInputs>)>]
    member this.``count counts the bindings`` (input : struct (int * string) array) =
        let struct (table, _) = this.Create input
        Assert.AreEqual<int>(Map.count (MapInputs.ToMap input), ops.Count table, $"%s{ops.Name}.count must count the bindings")

    [<TestMethod; DynamicData(nameof MapInputs.All, typeof<MapInputs>)>]
    member this.``isEmpty is true only for a map without bindings`` (input : struct (int * string) array) =
        let struct (table, _) = this.Create input

        Assert.AreEqual<bool>(
            Map.isEmpty (MapInputs.ToMap input),
            ops.IsEmpty table,
            $"%s{ops.Name}.isEmpty must be true only for a map without bindings"
        )

    [<TestMethod; DynamicData(nameof MapInputs.All, typeof<MapInputs>)>]
    member this.``keys returns the keys in the order of enumeration`` (input : struct (int * string) array) =
        let struct (table, bindings) = this.Create input
        let keys = Seq.toArray (ops.Keys table)
        CollectionAssert.AreEqual (MapInputs.KeysOf bindings, keys, $"%s{ops.Name}.keys must follow the enumeration")

        CollectionAssert.AreEqual (
            Seq.toArray (Map.keys (MapInputs.ToMap input)),
            Array.sort keys,
            $"%s{ops.Name}.keys must return the keys of the map"
        )

    [<TestMethod; DynamicData(nameof MapInputs.All, typeof<MapInputs>)>]
    member this.``values returns the values with duplicates in the order of enumeration`` (input : struct (int * string) array) =
        let struct (table, bindings) = this.Create input
        let values = Seq.toArray (ops.Values table)

        CollectionAssert.AreEqual (
            bindings |> Array.map (fun struct (_, value) -> value),
            values,
            $"%s{ops.Name}.values must follow the enumeration"
        )

        CollectionAssert.AreEqual (
            Array.sort (Seq.toArray (Map.values (MapInputs.ToMap input))),
            Array.sort values,
            $"%s{ops.Name}.values must return the values of the map with duplicates"
        )

    // ---- Access ----

    [<TestMethod; DynamicData(nameof MapContract<'M>.NonEmptyInputsAndKeys)>]
    member this.``find returns the value of each key`` (input : struct (int * string) array, key : int) =
        let struct (table, _) = this.Create input

        Assert.AreEqual<string>(
            Map.find key (MapInputs.ToMap input),
            ops.Find key table,
            $"%s{ops.Name}.find must return the value of the key"
        )

    [<TestMethod; DynamicData(nameof MapContract<'M>.InputsAndAbsentKeys)>]
    member this.``find throws for a key without a binding as Map does`` (input : struct (int * string) array, key : int) =
        let struct (table, _) = this.Create input

        this.ThrowsLikeMap (
            (fun () -> Map.find key (MapInputs.ToMap input)),
            (fun () -> ops.Find key table),
            "find of a key without a binding must throw"
        )

    [<TestMethod; DynamicData(nameof MapContract<'M>.InputsAndKeys)>]
    member this.``tryFind returns the value of a key or nothing`` (input : struct (int * string) array, key : int) =
        let struct (table, _) = this.Create input

        Assert.AreEqual<string voption>(
            Map.tryFind key (MapInputs.ToMap input)
            |> ValueOption.ofOption,
            ops.TryFind key table,
            $"%s{ops.Name}.tryFind must return the value of the key or nothing"
        )

    // ---- Search ----

    [<TestMethod; DynamicData(nameof MapContract<'M>.InputsAndBindingProbes)>]
    member this.``containsBinding is true only for a key that is bound to the value``
        (input : struct (int * string) array, key : int, value : string)
        =
        let struct (table, _) = this.Create input

        Assert.AreEqual<bool>(
            Map.tryFind key (MapInputs.ToMap input)
            |> Option.exists (fun stored -> stored = value),
            ops.ContainsBinding key value table,
            $"%s{ops.Name}.containsBinding must be true only for a key that is bound to the value"
        )

    [<TestMethod; DynamicData(nameof MapContract<'M>.InputsAndKeys)>]
    member this.``containsKey tells the keys with a binding`` (input : struct (int * string) array, key : int) =
        let struct (table, _) = this.Create input

        Assert.AreEqual<bool>(
            Map.containsKey key (MapInputs.ToMap input),
            ops.ContainsKey key table,
            $"%s{ops.Name}.containsKey must tell the keys with a binding"
        )

    [<TestMethod; DynamicData(nameof MapContract<'M>.InputsAndPredicates)>]
    member this.``exists finds a binding that satisfies the predicate``
        (input : struct (int * string) array, predicate : BindingPredicate)
        =
        let struct (table, _) = this.Create input

        Assert.AreEqual<bool>(
            Map.exists predicate.Matches (MapInputs.ToMap input),
            ops.Exists predicate.Matches table,
            $"%s{ops.Name}.exists must find a binding that satisfies the predicate"
        )

    [<TestMethod; DynamicData(nameof MapContract<'M>.FindKeyPredicates)>]
    member this.``findKey returns the key of the first binding that satisfies the predicate`` (predicate : BindingPredicate) =
        let struct (table, bindings) = this.Create MapInputs.Several

        let expected =
            bindings
            |> Array.pick (fun struct (key, value) -> if predicate.Matches key value then Some key else None)

        let actual = ops.FindKey predicate.Matches table
        Assert.AreEqual<int>(expected, actual, $"%s{ops.Name}.findKey must return the first key in the enumeration")

        Assert.IsTrue (
            predicate.Matches actual (Map.find actual (MapInputs.ToMap MapInputs.Several)),
            $"%s{ops.Name}.findKey must return a matching key"
        )

    [<TestMethod; DynamicData(nameof MapInputs.All, typeof<MapInputs>)>]
    member this.``findKey throws when no binding satisfies the predicate as Map does`` (input : struct (int * string) array) =
        let struct (table, _) = this.Create input
        let never = BindingPredicate.Never

        this.ThrowsLikeMap (
            (fun () -> Map.findKey never.Matches (MapInputs.ToMap input)),
            (fun () -> ops.FindKey never.Matches table),
            "findKey without a match must throw"
        )

    [<TestMethod; DynamicData(nameof MapContract<'M>.InputsAndForallPredicates)>]
    member this.``forall checks that every binding satisfies the predicate``
        (input : struct (int * string) array, predicate : BindingPredicate)
        =
        let struct (table, _) = this.Create input

        Assert.AreEqual<bool>(
            Map.forall predicate.Matches (MapInputs.ToMap input),
            ops.Forall predicate.Matches table,
            $"%s{ops.Name}.forall must check that every binding satisfies the predicate"
        )

    [<TestMethod>]
    member this.``pick returns the first chosen value`` () =
        let struct (table, bindings) = this.Create MapInputs.Several

        Assert.AreEqual<int>(
            bindings
            |> Array.pick (fun struct (key, value) -> negatedIfNegative key value |> ValueOption.toOption),
            ops.Pick negatedIfNegative table,
            $"%s{ops.Name}.pick must return the first value in the enumeration"
        )

    [<TestMethod; DynamicData(nameof MapInputs.All, typeof<MapInputs>)>]
    member this.``pick throws when the chooser returns nothing as Map does`` (input : struct (int * string) array) =
        let struct (table, _) = this.Create input

        this.ThrowsLikeMap (
            (fun () -> Map.pick (fun _ _ -> None) (MapInputs.ToMap input) : int),
            (fun () -> ops.Pick (fun _ _ -> ValueNone) table),
            "pick without a choice must throw"
        )

    [<TestMethod; DynamicData(nameof MapContract<'M>.InputsAndPredicates)>]
    member this.``tryFindKey returns the key of the first binding that satisfies the predicate or nothing``
        (input : struct (int * string) array, predicate : BindingPredicate)
        =
        let struct (table, bindings) = this.Create input

        let expected =
            bindings
            |> Array.tryPick (fun struct (key, value) -> if predicate.Matches key value then Some key else None)

        Assert.AreEqual<int voption>(
            ValueOption.ofOption expected,
            ops.TryFindKey predicate.Matches table,
            $"%s{ops.Name}.tryFindKey must return the first key in the enumeration or nothing"
        )

        Assert.AreEqual<bool>(
            (Map.tryFindKey predicate.Matches (MapInputs.ToMap input)).IsSome,
            (ops.TryFindKey predicate.Matches table).IsSome,
            $"%s{ops.Name}.tryFindKey must find a key when Map.tryFindKey does"
        )

    [<TestMethod; DynamicData(nameof MapInputs.All, typeof<MapInputs>)>]
    member this.``tryPick returns the first chosen value or nothing`` (input : struct (int * string) array) =
        let struct (table, bindings) = this.Create input

        Assert.AreEqual<int voption>(
            bindings
            |> Array.tryPick (fun struct (key, value) -> negatedIfNegative key value |> ValueOption.toOption)
            |> ValueOption.ofOption,
            ops.TryPick negatedIfNegative table,
            $"%s{ops.Name}.tryPick must return the first value in the enumeration or nothing"
        )

    // ---- Folding ----

    [<TestMethod; DynamicData(nameof MapInputs.All, typeof<MapInputs>)>]
    member this.``fold visits every binding once in the order of enumeration`` (input : struct (int * string) array) =
        let struct (table, bindings) = this.Create input
        let visited = ops.Fold record [||] table
        CollectionAssert.AreEqual (bindings, visited, $"%s{ops.Name}.fold must follow the enumeration")
        this.AreBindings (MapInputs.ToMap input, visited, "fold must visit every binding")

    [<TestMethod; DynamicData(nameof MapInputs.All, typeof<MapInputs>)>]
    member this.``foldBack visits every binding once in the reverse order of enumeration`` (input : struct (int * string) array) =
        let struct (table, bindings) = this.Create input
        let visited = ops.FoldBack recordBack table [||]
        CollectionAssert.AreEqual (bindings, visited, $"%s{ops.Name}.foldBack must follow the reverse enumeration")
        this.AreBindings (MapInputs.ToMap input, visited, "foldBack must visit every binding")

    [<TestMethod; DynamicData(nameof MapInputs.All, typeof<MapInputs>)>]
    member this.``iter visits every binding once in the order of enumeration`` (input : struct (int * string) array) =
        let struct (table, bindings) = this.Create input
        let visited = ResizeArray<struct (int * string)>()
        ops.Iter (fun key value -> visited.Add (struct (key, value))) table
        CollectionAssert.AreEqual (bindings, visited.ToArray (), $"%s{ops.Name}.iter must follow the enumeration")
        this.AreBindings (MapInputs.ToMap input, visited.ToArray (), "iter must visit every binding")

    // ---- Conversion ----

    [<TestMethod; DynamicData(nameof MapInputs.All, typeof<MapInputs>)>]
    member this.``toArray returns the bindings in the order of enumeration`` (input : struct (int * string) array) =
        let table = ops.OfSeq input
        let array = ops.ToArray table
        CollectionAssert.AreEqual (enumerated table, array, $"%s{ops.Name}.toArray must follow the enumeration of the map")
        this.AreBindings (MapInputs.ToMap input, array, "toArray must return every binding")

    [<TestMethod>]
    member _.``toArray returns a new array`` () =
        let table = ops.OfSeq MapInputs.Several
        let array = ops.ToArray table
        array[0] <- struct (1000, "thousand")
        Assert.AreEqual<int>(MapInputs.Several.Length, ops.Count table, $"%s{ops.Name}.toArray must return a copy")
        Assert.IsFalse (ops.ContainsKey 1000 table, $"%s{ops.Name}.toArray must return a copy that does not change the map")

    [<TestMethod; DynamicData(nameof MapInputs.All, typeof<MapInputs>)>]
    member this.``toList returns the bindings in the order of enumeration`` (input : struct (int * string) array) =
        let struct (table, bindings) = this.Create input
        CollectionAssert.AreEqual (bindings, List.toArray (ops.ToList table), $"%s{ops.Name}.toList must follow the enumeration")

    [<TestMethod; DynamicData(nameof MapInputs.All, typeof<MapInputs>)>]
    member this.``toSeq returns the bindings in the order of enumeration`` (input : struct (int * string) array) =
        let struct (table, bindings) = this.Create input
        CollectionAssert.AreEqual (bindings, Seq.toArray (ops.ToSeq table), $"%s{ops.Name}.toSeq must follow the enumeration")
