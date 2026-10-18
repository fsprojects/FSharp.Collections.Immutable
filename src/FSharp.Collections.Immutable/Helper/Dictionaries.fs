/// <summary>
/// Algorithms over the bindings of dictionaries that the map modules share (ADR 0001, D3, D5 and D13).
/// </summary>
/// <remarks>
/// <para>
/// The algorithms visit the bindings in the order in which the dictionary enumerates them, and the callers check their
/// arguments first. They take the interfaces of the dictionaries, so that a map module, its interface module and its
/// <c>Option</c> and <c>Tuple</c> modules share one implementation.
/// </para>
/// <para>
/// The algorithms that build or take tuples take the constructor or the deconstructor of the tuples, so that the default
/// modules and the <c>Tuple</c> modules share them and neither pays for a second pass.
/// </para>
/// <para>
/// A binding comes in three shapes (D13): a key/value pair, a reference tuple and a struct tuple. The algorithms that take
/// bindings take the deconstructor of their shape, <c>splitKeyValuePair</c>, <c>splitTuple</c> or <c>id</c>, so that
/// every map module and every comparer struct creates and changes its maps with one algorithm for the three shapes.
/// </para>
/// </remarks>
module internal FSharp.Collections.Immutable.Dictionaries

open System.Collections.Generic

/// Whether a binding satisfies the predicate; stops at the first one that does.
let exists (predicate : 'Key -> 'Value -> bool) (pairs : seq<KeyValuePair<'Key, 'Value>>) =
    use enumerator = pairs.GetEnumerator ()
    let mutable found = false

    while not found && enumerator.MoveNext () do
        let pair = enumerator.Current
        found <- predicate pair.Key pair.Value

    found

/// Whether every binding satisfies the predicate; stops at the first one that does not.
let forall (predicate : 'Key -> 'Value -> bool) (pairs : seq<KeyValuePair<'Key, 'Value>>) =
    use enumerator = pairs.GetEnumerator ()
    let mutable holds = true

    while holds && enumerator.MoveNext () do
        let pair = enumerator.Current
        holds <- predicate pair.Key pair.Value

    holds

/// The key of the first binding that satisfies the predicate.
let tryFindKey (predicate : 'Key -> 'Value -> bool) (pairs : seq<KeyValuePair<'Key, 'Value>>) =
    use enumerator = pairs.GetEnumerator ()
    let mutable result = ValueNone

    while result.IsNone && enumerator.MoveNext () do
        let pair = enumerator.Current

        if predicate pair.Key pair.Value then
            result <- ValueSome pair.Key

    result

/// The first value that the chooser returns for a binding.
let tryPick (chooser : 'Key -> 'Value -> 'U voption) (pairs : seq<KeyValuePair<'Key, 'Value>>) =
    use enumerator = pairs.GetEnumerator ()
    let mutable result = ValueNone

    while result.IsNone && enumerator.MoveNext () do
        let pair = enumerator.Current
        result <- chooser pair.Key pair.Value

    result

/// Applies the folder to the state and each binding, in the order of enumeration.
let fold (folder : 'State -> 'Key -> 'Value -> 'State) (state : 'State) (pairs : seq<KeyValuePair<'Key, 'Value>>) =
    let mutable state = state

    for pair in pairs do
        state <- folder state pair.Key pair.Value

    state

/// Applies the folder to each binding and the state, in the reverse order of enumeration.
let foldBack (folder : 'Key -> 'Value -> 'State -> 'State) (pairs : seq<KeyValuePair<'Key, 'Value>>) (state : 'State) =
    // An enumerator only moves forwards, so the bindings are copied first
    let bindings = Seq.toArray pairs
    let mutable state = state

    for index = bindings.Length - 1 downto 0 do
        let pair = bindings[index]
        state <- folder pair.Key pair.Value state

    state

/// Applies the action to each binding, in the order of enumeration.
let iter (action : 'Key -> 'Value -> unit) (pairs : seq<KeyValuePair<'Key, 'Value>>) =
    for pair in pairs do
        action pair.Key pair.Value

/// Sets the value that the chooser returns for a binding under its key in the target.
let choose (chooser : 'Key -> 'Value -> 'U voption) (target : IDictionary<'Key, 'U>) (pairs : seq<KeyValuePair<'Key, 'Value>>) =
    for pair in pairs do
        match chooser pair.Key pair.Value with
        | ValueSome value -> target[pair.Key] <- value
        | ValueNone -> ()

/// Sets the result of the mapping of each binding under its key in the target.
let map (mapping : 'Key -> 'Value -> 'U) (target : IDictionary<'Key, 'U>) (pairs : seq<KeyValuePair<'Key, 'Value>>) =
    for pair in pairs do
        target[pair.Key] <- mapping pair.Key pair.Value

/// Removes the bindings that do not satisfy the predicate from a target that starts with every binding.
/// The target is a builder over the same map, so that it shares what stays and returns the map itself when nothing goes.
let filter (predicate : 'Key -> 'Value -> bool) (target : IDictionary<'Key, 'Value>) (pairs : seq<KeyValuePair<'Key, 'Value>>) =
    for pair in pairs do
        if not (predicate pair.Key pair.Value) then
            target.Remove pair.Key |> ignore

/// Moves the bindings that do not satisfy the predicate from a target that starts with every binding to an empty one.
let partition
    (predicate : 'Key -> 'Value -> bool)
    (accepted : IDictionary<'Key, 'Value>)
    (rejected : IDictionary<'Key, 'Value>)
    (pairs : seq<KeyValuePair<'Key, 'Value>>)
    =
    for pair in pairs do
        if not (predicate pair.Key pair.Value) then
            accepted.Remove pair.Key |> ignore
            rejected[pair.Key] <- pair.Value

/// The key and the value of a key/value pair, the shape of the bindings that every .NET dictionary enumerates.
let inline splitKeyValuePair (pair : KeyValuePair<'Key, 'Value>) = struct (pair.Key, pair.Value)

/// The key and the value of a reference tuple, the shape of the bindings of most F# code.
let inline splitTuple (key : 'Key, value : 'Value) = struct (key, value)

/// Sets each element, split into a key and a value, in the target; a later binding of a key replaces an earlier one,
/// as in `Map.ofSeq`.
let inline setAll
    ([<InlineIfLambda>] split : 'Element -> struct ('Key * 'Value))
    (target : IDictionary<'Key, 'Value>)
    (elements : seq<'Element>)
    =
    for element in elements do
        let struct (key, value) = split element
        target[key] <- value

/// The elements, split into keys and values, as key/value pairs, computed on demand.
let inline toKeyValuePairs
    ([<InlineIfLambda>] split : 'Element -> struct ('Key * 'Value))
    (elements : seq<'Element>)
    : seq<KeyValuePair<'Key, 'Value>> =
    elements
    |> Seq.map (fun element ->
        let struct (key, value) = split element
        KeyValuePair (key, value)
    )

/// The bindings as an array of tuples, in the order of enumeration.
let inline toArray
    ([<InlineIfLambda>] tuple : 'Key -> 'Value -> 'R)
    (pairs : IReadOnlyCollection<KeyValuePair<'Key, 'Value>>)
    : 'R array =
    let result = Array.zeroCreate pairs.Count
    let mutable index = 0

    for pair in pairs do
        result[index] <- tuple pair.Key pair.Value
        index <- index + 1

    result

/// The bindings as a list of tuples, in the order of enumeration.
let inline toList ([<InlineIfLambda>] tuple : 'Key -> 'Value -> 'R) (pairs : seq<KeyValuePair<'Key, 'Value>>) : 'R list = [
    for pair in pairs -> tuple pair.Key pair.Value
]

/// The bindings as a sequence of tuples, in the order of enumeration, computed on demand.
let inline toSeq ([<InlineIfLambda>] tuple : 'Key -> 'Value -> 'R) (pairs : seq<KeyValuePair<'Key, 'Value>>) : seq<'R> =
    pairs |> Seq.map (fun pair -> tuple pair.Key pair.Value)
