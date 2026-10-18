namespace FSharp.Collections.Immutable.Tests

open System.Collections.Generic

/// <summary>
/// The reading functions of a map module for the contract tests, over <see cref="T:System.Int32"/> keys and
/// <see cref="T:System.String"/> values.
/// </summary>
/// <remarks>
/// <para>
/// A record of functions checks at compile time that a module provides every function of a facet (ADR 0001, D8). The
/// fields follow the order of the signature files.
/// </para>
/// <para>
/// Results of another type of the values are arrays: the instance of a module converts them with its own <c>toArray</c>,
/// so that one record type serves every map module.
/// </para>
/// </remarks>
type MapOps<'M> = {
    /// The name of the module, for the messages of the assertions.
    Name : string
    /// Maps the name of a parameter of the FSharp.Core oracle to the name of the parameter of the module.
    ParamName : string -> string
    /// Creates a map of the given bindings; the bindings of the inputs have distinct keys.
    OfSeq : seq<struct (int * string)> -> 'M
    /// The bindings in the order in which the map enumerates them: the `toArray` of the module, which the conversion tests
    /// check as well.
    ToArray : 'M -> struct (int * string) array

    Count : 'M -> int
    IsEmpty : 'M -> bool
    Keys : 'M -> seq<int>
    Values : 'M -> seq<string>

    Find : int -> 'M -> string
    TryFind : int -> 'M -> string voption

    ContainsBinding : int -> string -> 'M -> bool
    ContainsKey : int -> 'M -> bool
    Exists : (int -> string -> bool) -> 'M -> bool
    FindKey : (int -> string -> bool) -> 'M -> int
    Forall : (int -> string -> bool) -> 'M -> bool
    Pick : (int -> string -> int voption) -> 'M -> int
    TryFindKey : (int -> string -> bool) -> 'M -> int voption
    TryPick : (int -> string -> int voption) -> 'M -> int voption

    Fold :
        (struct (int * string) array -> int -> string -> struct (int * string) array)
            -> struct (int * string) array
            -> 'M
            -> struct (int * string) array
    FoldBack :
        (int -> string -> struct (int * string) array -> struct (int * string) array)
            -> 'M
            -> struct (int * string) array
            -> struct (int * string) array
    Iter : (int -> string -> unit) -> 'M -> unit

    ToList : 'M -> struct (int * string) list
    ToSeq : 'M -> seq<struct (int * string)>
}

/// <summary>
/// The whole-map transformations of a map module for the contract tests.
/// </summary>
/// <remarks>
/// The functions that change the type of the values return the bindings of their result as an array.
/// </remarks>
type MapTransformOps<'M> = {
    /// The name of the module, for the messages of the assertions.
    Name : string
    /// Maps the name of a parameter of the FSharp.Core oracle to the name of the parameter of the module.
    ParamName : string -> string
    /// Creates a map of the given bindings; the bindings of the inputs have distinct keys.
    OfSeq : seq<struct (int * string)> -> 'M
    /// The bindings in the order in which the map enumerates them.
    ToArray : 'M -> struct (int * string) array

    /// Chooses new values and returns the bindings of the result.
    Choose : (int -> string -> int voption) -> 'M -> struct (int * int) array
    Filter : (int -> string -> bool) -> 'M -> 'M
    /// Maps the values and returns the bindings of the result.
    Map : (int -> string -> int) -> 'M -> struct (int * int) array
    Partition : (int -> string -> bool) -> 'M -> struct ('M * 'M)
}

/// <summary>
/// The shapes of the bindings of a map (ADR 0001, D13): the rows of the tests of the functions that take bindings, which
/// convert their inputs, struct tuples, to the shape of the row.
/// </summary>
[<RequireQualifiedAccess>]
type BindingShape =
    /// `KeyValuePair<'Key, 'Value>`, which every .NET dictionary enumerates.
    | KeyValuePairs
    /// `'Key * 'Value`, the tuples of most F# code.
    | Tuples
    /// `struct ('Key * 'Value)`, the tuples of ADR 0001, D3.
    | StructTuples

    /// Every shape: the rows of the tests that run once per shape. The property builds its rows on each read.
    static member All : BindingShape seq = seq {
        BindingShape.KeyValuePairs
        BindingShape.Tuples
        BindingShape.StructTuples
    }

/// <summary>
/// The functions of a map module that change a map by bindings of one shape (ADR 0001, D13): the module takes key/value
/// pairs, its nested modules <c>Tuple</c> and <c>ValueTuple</c> take reference and struct tuples.
/// </summary>
type MapShapeMutationOps<'Binding, 'M> = { AddRange : seq<'Binding> -> 'M -> 'M; SetItems : seq<'Binding> -> 'M -> 'M }

/// <summary>
/// The incremental changes of a map module for the contract tests.
/// </summary>
/// <remarks>
/// A frozen map has no incremental changes, so its module does not derive this facet (ADR 0001, D1).
/// </remarks>
type MapMutationOps<'M> = {
    /// The name of the module, for the messages of the assertions.
    Name : string
    /// Maps the name of a parameter of the FSharp.Core oracle to the name of the parameter of the module.
    ParamName : string -> string
    /// Creates a map of the given bindings; the bindings of the inputs have distinct keys.
    OfSeq : seq<struct (int * string)> -> 'M
    /// The bindings in the order in which the map enumerates them.
    ToArray : 'M -> struct (int * string) array

    Add : int -> string -> 'M -> 'M
    Change : int -> (string voption -> string voption) -> 'M -> 'M
    Clear : 'M -> 'M
    Remove : int -> 'M -> 'M
    RemoveRange : seq<int> -> 'M -> 'M
    SetItem : int -> string -> 'M -> 'M

    /// `addRange` and `setItems` of the module, over key/value pairs.
    KeyValuePairs : MapShapeMutationOps<KeyValuePair<int, string>, 'M>
    /// `addRange` and `setItems` of the nested module `Tuple`, over reference tuples.
    Tuples : MapShapeMutationOps<int * string, 'M>
    /// `addRange` and `setItems` of the nested module `ValueTuple`, over struct tuples.
    StructTuples : MapShapeMutationOps<struct (int * string), 'M>
}

/// <summary>
/// The functions of a map module that create a map from bindings of one shape (ADR 0001, D13): the overloads of the
/// extension members, or of the members of a comparer struct (D12), for one shape.
/// </summary>
type MapShapeCreationOps<'Binding, 'M> = {
    OfArray : 'Binding array -> 'M
    OfList : 'Binding list -> 'M
    OfSeq : seq<'Binding> -> 'M
}

/// <summary>
/// The creation functions of a concrete map module, or of its comparer struct, for the contract tests.
/// </summary>
/// <remarks>
/// <para>
/// A collection derives the contract once for the functions with the default comparers and once for the members of its
/// comparer struct with a key comparer that compares the absolute values of the keys (ADR 0001, D8 and D12).
/// <see cref="P:FSharp.Collections.Immutable.Tests.MapCreationOps`1.EquivalentKey"/> tells the contract how the created maps
/// look the keys up.
/// </para>
/// <para>
/// A frozen map has no value comparer and no builder; the record holds only what every concrete map module creates.
/// </para>
/// </remarks>
type MapCreationOps<'M> = {
    /// The name of the module or of the struct, for the messages of the assertions.
    Name : string
    /// Maps the name of a parameter of the FSharp.Core oracle to the name of the parameter of the module.
    ParamName : string -> string
    /// Creates a map of the given bindings with the default comparers, as an existing map of the module.
    OfSeq : seq<struct (int * string)> -> 'M
    /// The bindings in the order in which the map enumerates them.
    ToArray : 'M -> struct (int * string) array
    /// Whether the map has a binding of a key, in its key comparer.
    ContainsKey : int -> 'M -> bool
    /// A key that the key comparer of the created maps considers equal to the given key: the key itself for the default
    /// comparer, its negation for the comparer of absolute values.
    EquivalentKey : int -> int

    Empty : unit -> 'M
    /// `ofArray`, `ofList` and `ofSeq` over key/value pairs.
    KeyValuePairs : MapShapeCreationOps<KeyValuePair<int, string>, 'M>
    /// `ofArray`, `ofList` and `ofSeq` over reference tuples.
    Tuples : MapShapeCreationOps<int * string, 'M>
    /// `ofArray`, `ofList` and `ofSeq` over struct tuples.
    StructTuples : MapShapeCreationOps<struct (int * string), 'M>
    /// Creates a map of the given values under the keys that the projection returns for them.
    OfSeqBy : (string -> int) -> seq<string> -> 'M
    Singleton : int -> string -> 'M
}
