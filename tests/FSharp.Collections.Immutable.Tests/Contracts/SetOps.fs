namespace FSharp.Collections.Immutable.Tests

/// <summary>
/// The reading functions of a set module for the contract tests, over <see cref="T:System.Int32"/> elements.
/// </summary>
/// <remarks>
/// <para>
/// A record of functions checks at compile time that a module provides every function of a facet (ADR 0001, D8). The
/// fields follow the order of the signature files. Every set module has this facet: <c>HashSet</c>, <c>SortedSet</c>,
/// <c>ISet</c> and <c>FrozenSet</c>.
/// </para>
/// <para>
/// The oracle of every set module is the FSharp.Core <c>Set</c> module. The order in which a hash set enumerates its
/// elements is unspecified, so the contracts compare the elements of a set as a sorted array, the order of an F# set.
/// </para>
/// </remarks>
type SetOps<'S> = {
    /// The name of the module, for the messages of the assertions.
    Name : string
    /// Maps the name of a parameter of the FSharp.Core oracle to the name of the parameter of the module.
    ParamName : string -> string
    /// Creates a set of the given elements.
    OfSeq : seq<int> -> 'S
    /// Creates a set of the given elements that compares them by their remainders in the division by 5: the struct of
    /// `forComparer` with `SetInputs.SameRemainder`, or `SetInputs.ByRemainder` for a sorted set.
    OfSeqByRemainder : seq<int> -> 'S
    /// The elements in the order in which the set enumerates them: the `toArray` of the module, which the conversion
    /// tests check as well.
    ToArray : 'S -> int array

    Count : 'S -> int
    IsEmpty : 'S -> bool

    TryGetValue : int -> 'S -> int voption

    Contains : int -> 'S -> bool
    Exists : (int -> bool) -> 'S -> bool
    Forall : (int -> bool) -> 'S -> bool
    IsProperSubset : 'S -> 'S -> bool
    IsProperSuperset : 'S -> 'S -> bool
    IsSubset : 'S -> 'S -> bool
    IsSuperset : 'S -> 'S -> bool
    Overlaps : 'S -> 'S -> bool
    SetEquals : 'S -> 'S -> bool

    Fold : (int array -> int -> int array) -> int array -> 'S -> int array
    FoldBack : (int -> int array -> int array) -> 'S -> int array -> int array
    Iter : (int -> unit) -> 'S -> unit

    ToList : 'S -> int list
    ToSeq : 'S -> seq<int>
}

/// <summary>
/// The transformations of a set module for the contract tests: every set module has them.
/// </summary>
/// <remarks>
/// Results of another set type or element type are arrays: the instance of a module converts its results with its own
/// <c>toArray</c>, so that one record type serves every module. <c>ISet</c>, for example, maps to a hash set.
/// </remarks>
type SetTransformOps<'S> = {
    /// The name of the module, for the messages of the assertions.
    Name : string
    /// Maps the name of a parameter of the FSharp.Core oracle to the name of the parameter of the module.
    ParamName : string -> string
    /// Creates a set of the given elements.
    OfSeq : seq<int> -> 'S
    /// The elements in the order in which the set enumerates them.
    ToArray : 'S -> int array

    Choose : (int -> int voption) -> 'S -> int array
    Difference : 'S -> 'S -> 'S
    Filter : (int -> bool) -> 'S -> 'S
    Intersect : 'S -> 'S -> 'S
    IntersectMany : seq<'S> -> 'S
    Map : (int -> int) -> 'S -> int array
    SymmetricDifference : 'S -> 'S -> 'S
    Union : 'S -> 'S -> 'S
    UnionMany : seq<'S> -> 'S

    Partition : (int -> bool) -> 'S -> struct ('S * 'S)
    PartitionWith : (int -> Choice<int, string>) -> 'S -> struct (int array * string array)
}

/// <summary>
/// The incremental changes of a set module for the contract tests: every set module except <c>FrozenSet</c>, which is
/// built once (ADR 0001, D1).
/// </summary>
type SetMutationOps<'S> = {
    /// The name of the module, for the messages of the assertions.
    Name : string
    /// Maps the name of a parameter of the FSharp.Core oracle to the name of the parameter of the module.
    ParamName : string -> string
    /// Creates a set of the given elements.
    OfSeq : seq<int> -> 'S
    /// Creates a set of the given elements that compares them by their remainders in the division by 5, as the field of
    /// `SetOps` does: the contract states which of two elements that the comparer finds equal stays (ADR 0001, D14).
    OfSeqByRemainder : seq<int> -> 'S
    /// The elements in the order in which the set enumerates them.
    ToArray : 'S -> int array

    Add : int -> 'S -> 'S
    AddRange : seq<int> -> 'S -> 'S
    Clear : 'S -> 'S
    Remove : int -> 'S -> 'S
    RemoveRange : seq<int> -> 'S -> 'S
}

/// <summary>
/// The members of the struct that creates the sets of a module with a comparer (ADR 0001, D12) for the creation contract:
/// <c>CustomComparerHashSet</c>, <c>CustomComparerSortedSet</c> and <c>CustomComparerFrozenSet</c>.
/// </summary>
/// <remarks>
/// The struct is created with a comparer of <c>SetInputs</c>: <c>SetInputs.SameRemainder</c> for hash and frozen sets and
/// <c>SetInputs.ByRemainder</c> for sorted sets. Both consider two numbers equal when their remainders in the division by
/// 5 are equal, so the contract states its expectations in remainders, whatever the type of the comparer.
/// </remarks>
type SetCustomComparerOps<'S> = {
    /// The comparer that the struct returns from its <c>Comparer</c> property.
    Comparer : obj

    Empty : unit -> 'S
    OfArray : int array -> 'S
    OfList : int list -> 'S
    /// Creates a set from a read-only span over the array: a span cannot be an argument of a function type.
    OfReadOnlySpan : int array -> 'S
    OfSeq : seq<int> -> 'S
    /// Creates a set from a span over the array: a span cannot be an argument of a function type.
    OfSpan : int array -> 'S
    Singleton : int -> 'S
}

/// <summary>
/// The creation functions of a concrete set module for the contract tests: <c>HashSet</c>, <c>SortedSet</c> and
/// <c>FrozenSet</c>.
/// </summary>
/// <remarks>
/// The facet also runs the members of the struct that creates sets with a comparer, the struct that <c>forComparer</c>
/// of the module returns. The builders are not part of it, because <c>FrozenSet</c> has none.
/// </remarks>
type SetCreationOps<'S> = {
    /// The name of the module, for the messages of the assertions.
    Name : string
    /// Maps the name of a parameter of the FSharp.Core oracle to the name of the parameter of the module.
    ParamName : string -> string
    /// The elements in the order in which the set enumerates them.
    ToArray : 'S -> int array
    /// The comparer of a set, as the <c>comparer</c> function of the module returns it.
    Comparer : 'S -> obj

    Empty : unit -> 'S
    OfArray : int array -> 'S
    OfList : int list -> 'S
    /// Creates a set from a read-only span over the array: a span cannot be an argument of a function type.
    OfReadOnlySpan : int array -> 'S
    OfSeq : seq<int> -> 'S
    /// Creates a set from a span over the array: a span cannot be an argument of a function type.
    OfSpan : int array -> 'S
    Singleton : int -> 'S

    /// The members of the struct that <c>forComparer</c> creates with the comparer of <c>SetInputs</c>.
    CustomComparer : SetCustomComparerOps<'S>
}
