/// <summary>
/// Algorithms over the bindings of dictionaries that the map modules share (ADR 0001, D3, D5 and D13).
/// </summary>
/// <remarks>
/// <para>
/// The functions of the FSharp.Core <see cref="T:Microsoft.FSharp.Collections.MapModule"/> module take an F# map, and the
/// maps of this library are dictionaries of the BCL, which enumerate their bindings as key/value pairs. Each algorithm here
/// does what its function of <see cref="T:Microsoft.FSharp.Collections.MapModule"/> does over such a sequence of pairs. The
/// algorithms visit the bindings in the order in which the dictionary enumerates them, and the callers check their
/// arguments first. They take the interfaces of the dictionaries, so that a map module, its interface module and its
/// <see cref="N:FSharp.Collections.Immutable.Option"/> and <see cref="N:FSharp.Collections.Immutable.Tuple"/> modules share
/// one implementation.
/// </para>
/// <para>
/// The algorithms that build or take tuples take the constructor or the deconstructor of the tuples, so that the default
/// modules and the modules of the <see cref="N:FSharp.Collections.Immutable.Tuple"/> namespace share them and neither pays
/// for a second pass.
/// </para>
/// <para>
/// A binding comes in three shapes (D13): a key/value pair, a reference tuple and a struct tuple. The algorithms that take
/// bindings take the deconstructor of their shape,
/// <see cref="M:FSharp.Collections.Immutable.Dictionaries.splitKeyValuePair``2(System.Collections.Generic.KeyValuePair{``0,``1})"/>,
/// <see cref="M:FSharp.Collections.Immutable.Dictionaries.splitTuple``2(``0,``1)"/>
/// or <see cref="M:Microsoft.FSharp.Core.Operators.Identity``1(``0)"/>, so that every map
/// module and every comparer struct creates and changes its maps with one algorithm for the three shapes.
/// </para>
/// <para>
/// The summary of each function is the summary of the function of
/// <see cref="T:Microsoft.FSharp.Collections.MapModule"/> word for word (FSharp.Core 10.1), and so are the descriptions of
/// the parameters and of the result where the signatures agree. The remarks say what differs and why the function exists.
/// </para>
/// </remarks>
module internal FSharp.Collections.Immutable.Dictionaries

open System.Collections.Generic

/// <summary>
/// Returns true if the given predicate returns true for one of the bindings in the map.
/// </summary>
/// <param name="predicate">The function to test the input elements.</param>
/// <param name="pairs">The bindings of the dictionary.</param>
/// <returns>True if the predicate returns true for one of the key/value pairs.</returns>
/// <remarks>
/// <para>
/// This is an O(n) operation in the worst case, where n is the number of bindings in the map. The loop stops at the first
/// binding that satisfies the predicate.
/// </para>
/// <para>
/// It exists because
/// <see cref="M:Microsoft.FSharp.Collections.MapModule.Exists``2(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpFunc{``1,System.Boolean}},Microsoft.FSharp.Collections.FSharpMap{``0,``1})"/>
/// takes an F# map, and a dictionary of the BCL is not one. The loop calls the curried
/// predicate with the key and the value of each pair.
/// </para>
/// </remarks>
/// <seealso cref="M:FSharp.Collections.Immutable.HashMap.Exists``2(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpFunc{``1,System.Boolean}},System.Collections.Immutable.ImmutableDictionary{``0,``1})"/>
/// <seealso cref="M:FSharp.Collections.Immutable.IMap.Exists``2(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpFunc{``1,System.Boolean}},System.Collections.Immutable.IImmutableDictionary{``0,``1})"/>
let exists (predicate : 'Key -> 'Value -> bool) (pairs : seq<KeyValuePair<'Key, 'Value>>) =
    use enumerator = pairs.GetEnumerator ()
    let mutable found = false

    while not found && enumerator.MoveNext () do
        let pair = enumerator.Current
        found <- predicate pair.Key pair.Value

    found

/// <summary>
/// Returns true if the given predicate returns true for all of the bindings in the map.
/// </summary>
/// <param name="predicate">The function to test the input elements.</param>
/// <param name="pairs">The bindings of the dictionary.</param>
/// <returns>True if the predicate evaluates to true for all of the bindings in the map.</returns>
/// <remarks>
/// <para>
/// This is an O(n) operation, where n is the number of bindings in the map. The loop stops at the first binding that does
/// not satisfy the predicate.
/// </para>
/// <para>
/// It exists because
/// <see cref="M:Microsoft.FSharp.Collections.MapModule.ForAll``2(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpFunc{``1,System.Boolean}},Microsoft.FSharp.Collections.FSharpMap{``0,``1})"/>
/// takes an F# map, and a dictionary of the BCL is not one.
/// </para>
/// </remarks>
/// <seealso cref="M:FSharp.Collections.Immutable.HashMap.ForAll``2(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpFunc{``1,System.Boolean}},System.Collections.Immutable.ImmutableDictionary{``0,``1})"/>
/// <seealso cref="M:FSharp.Collections.Immutable.IMap.ForAll``2(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpFunc{``1,System.Boolean}},System.Collections.Immutable.IImmutableDictionary{``0,``1})"/>
let forall (predicate : 'Key -> 'Value -> bool) (pairs : seq<KeyValuePair<'Key, 'Value>>) =
    use enumerator = pairs.GetEnumerator ()
    let mutable holds = true

    while holds && enumerator.MoveNext () do
        let pair = enumerator.Current
        holds <- predicate pair.Key pair.Value

    holds

/// <summary>
/// Returns the key of the first mapping in the collection that satisfies the given predicate. Returns <c>ValueNone</c> if
/// no such element exists.
/// </summary>
/// <param name="predicate">The function to test the input elements.</param>
/// <param name="pairs">The bindings of the dictionary.</param>
/// <returns>
/// The first key for which the predicate returns true or <c>ValueNone</c> if the predicate evaluates to false for each
/// key/value pair.
/// </returns>
/// <remarks>
/// <para>
/// This is an O(n) operation in the worst case, where n is the number of bindings in the map.
/// </para>
/// <para>
/// It exists because
/// <see cref="M:Microsoft.FSharp.Collections.MapModule.TryFindKey``2(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpFunc{``1,System.Boolean}},Microsoft.FSharp.Collections.FSharpMap{``0,``1})"/>
/// takes an F# map and returns an option, where the functions of this library return a
/// value option (ADR 0001, D3). The modules of the <see cref="N:FSharp.Collections.Immutable.Option"/> namespace convert the
/// result.
/// </para>
/// </remarks>
/// <seealso cref="M:FSharp.Collections.Immutable.HashMap.TryFindKey``2(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpFunc{``1,System.Boolean}},System.Collections.Immutable.ImmutableDictionary{``0,``1})"/>
/// <seealso cref="M:FSharp.Collections.Immutable.IMap.TryFindKey``2(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpFunc{``1,System.Boolean}},System.Collections.Immutable.IImmutableDictionary{``0,``1})"/>
let tryFindKey (predicate : 'Key -> 'Value -> bool) (pairs : seq<KeyValuePair<'Key, 'Value>>) =
    use enumerator = pairs.GetEnumerator ()
    let mutable result = ValueNone

    while result.IsNone && enumerator.MoveNext () do
        let pair = enumerator.Current

        if predicate pair.Key pair.Value then
            result <- ValueSome pair.Key

    result

/// <summary>
/// Searches the map looking for the first element where the given function returns a <c>ValueSome</c> value.
/// </summary>
/// <param name="chooser">The function to generate value options from the key/value pairs.</param>
/// <param name="pairs">The bindings of the dictionary.</param>
/// <returns>The first result, or <c>ValueNone</c> if the function returns <c>ValueNone</c> for each key/value pair.</returns>
/// <remarks>
/// <para>
/// This is an O(n) operation in the worst case, where n is the number of bindings in the map.
/// </para>
/// <para>
/// It exists because
/// <see cref="M:Microsoft.FSharp.Collections.MapModule.TryPick``3(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpFunc{``1,Microsoft.FSharp.Core.FSharpOption{``2}}},Microsoft.FSharp.Collections.FSharpMap{``0,``1})"/>
/// takes an F# map and a function that returns an option, where the functions of this
/// library take and return value options (ADR 0001, D3).
/// </para>
/// </remarks>
/// <seealso cref="M:FSharp.Collections.Immutable.HashMap.TryPick``3(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpFunc{``1,Microsoft.FSharp.Core.FSharpValueOption{``2}}},System.Collections.Immutable.ImmutableDictionary{``0,``1})"/>
/// <seealso cref="M:FSharp.Collections.Immutable.IMap.TryPick``3(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpFunc{``1,Microsoft.FSharp.Core.FSharpValueOption{``2}}},System.Collections.Immutable.IImmutableDictionary{``0,``1})"/>
let tryPick (chooser : 'Key -> 'Value -> 'U voption) (pairs : seq<KeyValuePair<'Key, 'Value>>) =
    use enumerator = pairs.GetEnumerator ()
    let mutable result = ValueNone

    while result.IsNone && enumerator.MoveNext () do
        let pair = enumerator.Current
        result <- chooser pair.Key pair.Value

    result

/// <summary>
/// Folds over the bindings in the map
/// </summary>
/// <param name="folder">The function to update the state given the input key/value pairs.</param>
/// <param name="state">The initial state.</param>
/// <param name="pairs">The bindings of the dictionary.</param>
/// <returns>The final state value.</returns>
/// <remarks>
/// <para>
/// This is an O(n) operation, where n is the number of bindings in the map. The folder sees the bindings in the order in
/// which the dictionary enumerates them.
/// </para>
/// <para>
/// It exists because
/// <see cref="M:Microsoft.FSharp.Collections.MapModule.Fold``3(Microsoft.FSharp.Core.FSharpFunc{``2,Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpFunc{``1,``2}}},``2,Microsoft.FSharp.Collections.FSharpMap{``0,``1})"/>
/// takes an F# map, and a dictionary of the BCL is not one.
/// </para>
/// </remarks>
/// <seealso cref="M:FSharp.Collections.Immutable.HashMap.Fold``3(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpFunc{``1,Microsoft.FSharp.Core.FSharpFunc{``2,``0}}},``0,System.Collections.Immutable.ImmutableDictionary{``1,``2})"/>
/// <seealso cref="M:FSharp.Collections.Immutable.IMap.Fold``3(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpFunc{``1,Microsoft.FSharp.Core.FSharpFunc{``2,``0}}},``0,System.Collections.Immutable.IImmutableDictionary{``1,``2})"/>
let fold (folder : 'State -> 'Key -> 'Value -> 'State) (state : 'State) (pairs : seq<KeyValuePair<'Key, 'Value>>) =
    let mutable state = state

    for pair in pairs do
        state <- folder state pair.Key pair.Value

    state

/// <summary>
/// Folds over the bindings in the map.
/// </summary>
/// <param name="folder">The function to update the state given the input key/value pairs.</param>
/// <param name="pairs">The bindings of the dictionary.</param>
/// <param name="state">The initial state.</param>
/// <returns>The final state value.</returns>
/// <remarks>
/// <para>
/// This is an O(n) operation, where n is the number of bindings in the map. The folder sees the bindings in the reverse
/// order of the enumeration of the dictionary. For a dictionary that enumerates its keys in ascending order, such as a
/// sorted one, that is from the greatest key, as
/// <see cref="M:Microsoft.FSharp.Collections.MapModule.FoldBack``3(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpFunc{``1,Microsoft.FSharp.Core.FSharpFunc{``2,``2}}},Microsoft.FSharp.Collections.FSharpMap{``0,``1},``2)"/>
/// folds. A hash map has no order of its keys, so it folds
/// from the last binding that it enumerates.
/// </para>
/// <para>
/// It exists because
/// <see cref="M:Microsoft.FSharp.Collections.MapModule.FoldBack``3(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpFunc{``1,Microsoft.FSharp.Core.FSharpFunc{``2,``2}}},Microsoft.FSharp.Collections.FSharpMap{``0,``1},``2)"/>
/// takes an F# map, and a dictionary of the BCL is not one. An enumerator moves only
/// forwards, so the function copies the bindings into an array first.
/// </para>
/// </remarks>
/// <seealso cref="M:FSharp.Collections.Immutable.HashMap.FoldBack``3(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpFunc{``1,Microsoft.FSharp.Core.FSharpFunc{``2,``2}}},System.Collections.Immutable.ImmutableDictionary{``0,``1},``2)"/>
/// <seealso cref="M:FSharp.Collections.Immutable.IMap.FoldBack``3(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpFunc{``1,Microsoft.FSharp.Core.FSharpFunc{``2,``2}}},System.Collections.Immutable.IImmutableDictionary{``0,``1},``2)"/>
let foldBack (folder : 'Key -> 'Value -> 'State -> 'State) (pairs : seq<KeyValuePair<'Key, 'Value>>) (state : 'State) =
    // An enumerator only moves forwards, so the bindings are copied first
    let bindings = Seq.toArray pairs
    let mutable state = state

    for index = bindings.Length - 1 downto 0 do
        let pair = bindings[index]
        state <- folder pair.Key pair.Value state

    state

/// <summary>
/// Applies the given function to each binding in the dictionary
/// </summary>
/// <param name="action">The function to apply to each key/value pair.</param>
/// <param name="pairs">The bindings of the dictionary.</param>
/// <remarks>
/// <para>
/// This is an O(n) operation, where n is the number of bindings in the map. The action sees the bindings in the order in
/// which the dictionary enumerates them.
/// </para>
/// <para>
/// It exists because
/// <see cref="M:Microsoft.FSharp.Collections.MapModule.Iterate``2(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpFunc{``1,Microsoft.FSharp.Core.Unit}},Microsoft.FSharp.Collections.FSharpMap{``0,``1})"/>
/// takes an F# map, and a dictionary of the BCL is not one.
/// </para>
/// </remarks>
/// <seealso cref="M:FSharp.Collections.Immutable.HashMap.Iterate``2(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpFunc{``1,Microsoft.FSharp.Core.Unit}},System.Collections.Immutable.ImmutableDictionary{``0,``1})"/>
/// <seealso cref="M:FSharp.Collections.Immutable.IMap.Iterate``2(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpFunc{``1,Microsoft.FSharp.Core.Unit}},System.Collections.Immutable.IImmutableDictionary{``0,``1})"/>
let iter (action : 'Key -> 'Value -> unit) (pairs : seq<KeyValuePair<'Key, 'Value>>) =
    for pair in pairs do
        action pair.Key pair.Value

/// <summary>
/// Sets the value that the chooser returns for a binding under its key in the target, for each binding for which the
/// chooser returns a value.
/// </summary>
/// <param name="chooser">The function to generate value options from the key/value pairs.</param>
/// <param name="target">The dictionary that receives the chosen values, empty when the function starts.</param>
/// <param name="pairs">The bindings of the dictionary.</param>
/// <remarks>
/// <para>
/// This is an O(n) operation, where n is the number of bindings in the map.
/// </para>
/// <para>
/// FSharp.Core has no such function for maps, so there is no summary to repeat. The function fills a target that the caller
/// creates, instead of returning a map, so that the caller picks the comparers of the result. The values change their type,
/// so they get the default comparer of their type (ADR 0001, D5, Comparers). The keys keep the comparer of the input in the
/// function of a hash map; the function of the interface cannot read that comparer and uses the default one.
/// </para>
/// </remarks>
/// <seealso cref="M:FSharp.Collections.Immutable.HashMap.Choose``3(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpFunc{``1,Microsoft.FSharp.Core.FSharpValueOption{``2}}},System.Collections.Immutable.ImmutableDictionary{``0,``1})"/>
/// <seealso cref="M:FSharp.Collections.Immutable.IMap.Choose``3(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpFunc{``1,Microsoft.FSharp.Core.FSharpValueOption{``2}}},System.Collections.Immutable.IImmutableDictionary{``0,``1})"/>
let choose (chooser : 'Key -> 'Value -> 'U voption) (target : IDictionary<'Key, 'U>) (pairs : seq<KeyValuePair<'Key, 'Value>>) =
    for pair in pairs do
        match chooser pair.Key pair.Value with
        | ValueSome value -> target[pair.Key] <- value
        | ValueNone -> ()

/// <summary>
/// Sets the result of the application of the given function to each binding under the key of the binding in the target. The
/// key passed to the function indicates the key of element being transformed.
/// </summary>
/// <param name="mapping">The function to transform the key/value pairs.</param>
/// <param name="target">The dictionary that receives the transformed values, empty when the function starts.</param>
/// <param name="pairs">The bindings of the dictionary.</param>
/// <remarks>
/// <para>
/// This is an O(n) operation, where n is the number of bindings in the map, creating the bindings of a new map with the same
/// keys.
/// </para>
/// <para>
/// It exists because
/// <see cref="M:Microsoft.FSharp.Collections.MapModule.Map``3(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpFunc{``1,``2}},Microsoft.FSharp.Collections.FSharpMap{``0,``1})"/>
/// takes an F# map and returns one. The function fills a target that the caller creates, so that
/// the caller picks the comparers of the result. The values change their type, so they get the default comparer of their
/// type (ADR 0001, D5, Comparers). The keys keep the comparer of the input in the function of a hash map; the function of
/// the interface cannot read that comparer and uses the default one.
/// </para>
/// </remarks>
/// <seealso cref="M:FSharp.Collections.Immutable.HashMap.Map``3(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpFunc{``1,``2}},System.Collections.Immutable.ImmutableDictionary{``0,``1})"/>
/// <seealso cref="M:FSharp.Collections.Immutable.IMap.Map``3(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpFunc{``1,``2}},System.Collections.Immutable.IImmutableDictionary{``0,``1})"/>
let map (mapping : 'Key -> 'Value -> 'U) (target : IDictionary<'Key, 'U>) (pairs : seq<KeyValuePair<'Key, 'Value>>) =
    for pair in pairs do
        target[pair.Key] <- mapping pair.Key pair.Value

/// <summary>
/// Removes the bindings for which the given predicate returns false from the target, which starts with every binding of the
/// map.
/// </summary>
/// <param name="predicate">The function to test the key/value pairs.</param>
/// <param name="target">The dictionary to filter: a builder over the map that <paramref name="pairs"/> enumerates.</param>
/// <param name="pairs">The bindings of the dictionary.</param>
/// <remarks>
/// <para>
/// This is an O(n) operation, where n is the number of bindings in the map, plus the cost of the removals.
/// </para>
/// <para>
/// It exists because
/// <see cref="M:Microsoft.FSharp.Collections.MapModule.Filter``2(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpFunc{``1,System.Boolean}},Microsoft.FSharp.Collections.FSharpMap{``0,``1})"/>
/// takes an F# map and builds a new one. The target is a builder over the same map, so that
/// the result shares what stays and the builder returns the map itself when nothing goes.
/// </para>
/// </remarks>
/// <seealso cref="M:FSharp.Collections.Immutable.HashMap.Filter``2(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpFunc{``1,System.Boolean}},System.Collections.Immutable.ImmutableDictionary{``0,``1})"/>
let filter (predicate : 'Key -> 'Value -> bool) (target : IDictionary<'Key, 'Value>) (pairs : seq<KeyValuePair<'Key, 'Value>>) =
    for pair in pairs do
        if not (predicate pair.Key pair.Value) then
            target.Remove pair.Key |> ignore

/// <summary>
/// Moves the bindings for which the given predicate returns false from the first target, which starts with every binding of
/// the map, to the second one, which starts empty.
/// </summary>
/// <param name="predicate">The function to test the input elements.</param>
/// <param name="accepted">
/// The dictionary of the bindings for which the predicate returns true: a builder over the map that
/// <paramref name="pairs"/> enumerates.
/// </param>
/// <param name="rejected">The dictionary of the remaining bindings: an empty builder with the comparers of the map.</param>
/// <param name="pairs">The bindings of the dictionary.</param>
/// <remarks>
/// <para>
/// This is an O(n) operation, where n is the number of bindings in the map, plus the cost of the removals and of the
/// insertions.
/// </para>
/// <para>
/// It exists because
/// <see cref="M:Microsoft.FSharp.Collections.MapModule.Partition``2(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpFunc{``1,System.Boolean}},Microsoft.FSharp.Collections.FSharpMap{``0,``1})"/>
/// takes an F# map and builds two new ones. The first target is a builder over the same
/// map, so that the first result shares what stays and the builder returns the map itself when every binding stays.
/// </para>
/// </remarks>
/// <seealso cref="M:FSharp.Collections.Immutable.HashMap.Partition``2(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpFunc{``1,System.Boolean}},System.Collections.Immutable.ImmutableDictionary{``0,``1})"/>
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

/// <summary>
/// The key and the value of a key/value pair, the shape of the bindings that every .NET dictionary enumerates.
/// </summary>
/// <param name="pair">The key/value pair.</param>
/// <returns>The key and the value, as a struct tuple.</returns>
/// <remarks>
/// FSharp.Core has no such function. It is one of the three deconstructors (D13) that the algorithms of this module take,
/// next to
/// <see cref="M:FSharp.Collections.Immutable.Dictionaries.splitTuple``2(``0,``1)"/>
/// and <see cref="M:Microsoft.FSharp.Core.Operators.Identity``1(``0)"/>.
/// </remarks>
let inline splitKeyValuePair (pair : KeyValuePair<'Key, 'Value>) = struct (pair.Key, pair.Value)

/// <summary>
/// The key and the value of a reference tuple, the shape of the bindings of most F# code.
/// </summary>
/// <param name="key">The first item of the tuple.</param>
/// <param name="value">The second item of the tuple.</param>
/// <returns>The key and the value, as a struct tuple.</returns>
/// <remarks>
/// FSharp.Core has no such function. It is one of the three deconstructors (D13) that the algorithms of this module take,
/// next to
/// <see cref="M:FSharp.Collections.Immutable.Dictionaries.splitKeyValuePair``2(System.Collections.Generic.KeyValuePair{``0,``1})"/>
/// and <see cref="M:Microsoft.FSharp.Core.Operators.Identity``1(``0)"/>.
/// </remarks>
let inline splitTuple (key : 'Key, value : 'Value) = struct (key, value)

/// <summary>
/// Sets each element, split into a key and a value, in the target; a later binding of a key replaces an earlier one, as in
/// <see cref="M:Microsoft.FSharp.Collections.MapModule.OfSeq``2(System.Collections.Generic.IEnumerable{System.Tuple{``0,``1}})"/>.
/// </summary>
/// <param name="split">The deconstructor of the shape of the elements.</param>
/// <param name="target">The dictionary that receives the bindings.</param>
/// <param name="elements">The bindings, in any of the three shapes.</param>
/// <remarks>
/// <para>
/// This is an O(n) operation, where n is the number of elements.
/// </para>
/// <para>
/// A later binding replaces an earlier one through the indexer of the target. When the value comparer of the target finds the
/// new value equal to the stored one, the indexer of the builder of <see cref="T:System.Collections.Immutable.ImmutableDictionary`2"/>
/// leaves the stored binding as it is, key included, and so does this function (ADR 0001, D14).
/// </para>
/// <para>
/// It exists because the three shapes of a binding (D13) need one algorithm, not three: the creation members of the comparer
/// structs and of the extensions pass the deconstructor of their shape.
/// </para>
/// </remarks>
let inline setAll
    ([<InlineIfLambda>] split : 'Element -> struct ('Key * 'Value))
    (target : IDictionary<'Key, 'Value>)
    (elements : seq<'Element>)
    =
    for element in elements do
        let struct (key, value) = split element
        target[key] <- value

/// <summary>
/// The elements, split into keys and values, as key/value pairs, computed on demand.
/// </summary>
/// <param name="split">The deconstructor of the shape of the elements.</param>
/// <param name="elements">The bindings, in any of the three shapes.</param>
/// <returns>The key/value pairs, in the order of the elements; a pair is created when the sequence is read.</returns>
/// <remarks>
/// FSharp.Core has no such function. It exists because the dictionaries of the BCL take key/value pairs, where the callers
/// receive bindings in the shape of their choice (D13):
/// <see cref="M:System.Collections.Immutable.ImmutableDictionary`2.AddRange(System.Collections.Generic.IEnumerable{System.Collections.Generic.KeyValuePair{`0,`1}})"/>
/// and
/// <see cref="M:System.Collections.Immutable.ImmutableDictionary`2.SetItems(System.Collections.Generic.IEnumerable{System.Collections.Generic.KeyValuePair{`0,`1}})"/>
/// take the result.
/// </remarks>
let inline toKeyValuePairs
    ([<InlineIfLambda>] split : 'Element -> struct ('Key * 'Value))
    (elements : seq<'Element>)
    : seq<KeyValuePair<'Key, 'Value>> =
    elements
    |> Seq.map (fun element ->
        let struct (key, value) = split element
        KeyValuePair (key, value)
    )

/// <summary>
/// Returns an array of all key-value pairs in the mapping.
/// </summary>
/// <param name="tuple">The function that builds an element of the result from a key and a value.</param>
/// <param name="pairs">The bindings of the dictionary.</param>
/// <returns>The array of key/value pairs, in the order in which the dictionary enumerates them.</returns>
/// <remarks>
/// <para>
/// This is an O(n) operation, where n is the number of bindings in the map. The array has the size of the map from the
/// start, because the map knows its count.
/// </para>
/// <para>
/// It exists because
/// <see cref="M:Microsoft.FSharp.Collections.MapModule.ToArray``2(Microsoft.FSharp.Collections.FSharpMap{``0,``1})"/>
/// takes an F# map and returns reference tuples. The default function of a map module
/// returns struct tuples, and the function of the <see cref="N:FSharp.Collections.Immutable.Tuple"/> namespace returns
/// reference tuples, so each passes the constructor of its tuples and neither pays for a second pass.
/// </para>
/// </remarks>
/// <seealso cref="M:FSharp.Collections.Immutable.HashMap.ToArray``2(System.Collections.Immutable.ImmutableDictionary{``0,``1})"/>
/// <seealso cref="M:FSharp.Collections.Immutable.Tuple.HashMap.ToArray``2(System.Collections.Immutable.ImmutableDictionary{``0,``1})"/>
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

/// <summary>
/// Returns a list of all key-value pairs in the mapping.
/// </summary>
/// <param name="tuple">The function that builds an element of the result from a key and a value.</param>
/// <param name="pairs">The bindings of the dictionary.</param>
/// <returns>The list of key/value pairs, in the order in which the dictionary enumerates them.</returns>
/// <remarks>
/// <para>
/// This is an O(n) operation, where n is the number of bindings in the map.
/// </para>
/// <para>
/// It exists because
/// <see cref="M:Microsoft.FSharp.Collections.MapModule.ToList``2(Microsoft.FSharp.Collections.FSharpMap{``0,``1})"/>
/// takes an F# map and returns reference tuples, for the reason given for
/// <see cref="M:FSharp.Collections.Immutable.Dictionaries.toArray``3(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpFunc{``1,``2}},System.Collections.Generic.IReadOnlyCollection{System.Collections.Generic.KeyValuePair{``0,``1}})"/>.
/// </para>
/// </remarks>
/// <seealso cref="M:FSharp.Collections.Immutable.HashMap.ToList``2(System.Collections.Immutable.ImmutableDictionary{``0,``1})"/>
/// <seealso cref="M:FSharp.Collections.Immutable.Tuple.HashMap.ToList``2(System.Collections.Immutable.ImmutableDictionary{``0,``1})"/>
let inline toList ([<InlineIfLambda>] tuple : 'Key -> 'Value -> 'R) (pairs : seq<KeyValuePair<'Key, 'Value>>) : 'R list = [
    for pair in pairs -> tuple pair.Key pair.Value
]

/// <summary>
/// Views the collection as an enumerable sequence of pairs.
/// </summary>
/// <param name="tuple">The function that builds an element of the result from a key and a value.</param>
/// <param name="pairs">The bindings of the dictionary.</param>
/// <returns>The sequence of key/value pairs, in the order in which the dictionary enumerates them; computed on demand.</returns>
/// <remarks>
/// <para>
/// This is an O(n) operation, where n is the number of bindings in the map, when the sequence is read.
/// </para>
/// <para>
/// It exists because
/// <see cref="M:Microsoft.FSharp.Collections.MapModule.ToSeq``2(Microsoft.FSharp.Collections.FSharpMap{``0,``1})"/>
/// takes an F# map and returns reference tuples, for the reason given for
/// <see cref="M:FSharp.Collections.Immutable.Dictionaries.toArray``3(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpFunc{``1,``2}},System.Collections.Generic.IReadOnlyCollection{System.Collections.Generic.KeyValuePair{``0,``1}})"/>.
/// </para>
/// </remarks>
/// <seealso cref="M:FSharp.Collections.Immutable.HashMap.ToSeq``2(System.Collections.Immutable.ImmutableDictionary{``0,``1})"/>
/// <seealso cref="M:FSharp.Collections.Immutable.Tuple.HashMap.ToSeq``2(System.Collections.Immutable.ImmutableDictionary{``0,``1})"/>
let inline toSeq ([<InlineIfLambda>] tuple : 'Key -> 'Value -> 'R) (pairs : seq<KeyValuePair<'Key, 'Value>>) : seq<'R> =
    pairs |> Seq.map (fun pair -> tuple pair.Key pair.Value)
