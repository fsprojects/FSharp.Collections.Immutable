namespace FSharp.Collections.Immutable.Tests

open System
open System.Collections.Generic

/// <summary>
/// The functions of a sequence-like module for the contract tests, over <see cref="T:System.Int32"/> elements.
/// </summary>
/// <remarks>
/// <para>
/// A record of functions checks at compile time that a module provides every function of a facet (ADR 0001, D8). The
/// fields follow the order of the signature files.
/// </para>
/// <para>
/// Results of another element type are arrays: the instance of a collection converts its results with its own
/// <c>toArray</c>, so that one record type serves every collection.
/// </para>
/// </remarks>
type SeqOps<'C> = {
    /// The name of the module, for the messages of the assertions.
    Name : string
    /// The FSharp.Core module whose exceptions the module mirrors.
    Mirror : Mirror
    /// Maps the name of a parameter of the FSharp.Core oracle to the name of the parameter of the module.
    ParamName : string -> string
    /// Creates a collection that enumerates the given elements in their order.
    OfSeq : seq<int> -> 'C
    /// The elements in the order in which the collection enumerates them: the `toArray` of the module, which the
    /// conversion tests check as well.
    ToArray : 'C -> int array

    IsEmpty : 'C -> bool
    Length : 'C -> int

    ExactlyOne : 'C -> int
    Head : 'C -> int
    Last : 'C -> int
    Tail : 'C -> 'C
    TryExactlyOne : 'C -> int voption
    TryHead : 'C -> int voption
    TryLast : 'C -> int voption
    TryTail : 'C -> 'C voption

    Contains : int -> 'C -> bool
    Exists : (int -> bool) -> 'C -> bool
    Exists2 : (int -> int -> bool) -> 'C -> 'C -> bool
    Find : (int -> bool) -> 'C -> int
    FindBack : (int -> bool) -> 'C -> int
    FindIndex : (int -> bool) -> 'C -> int
    FindIndexBack : (int -> bool) -> 'C -> int
    Forall : (int -> bool) -> 'C -> bool
    Forall2 : (int -> int -> bool) -> 'C -> 'C -> bool
    Pick : (int -> int voption) -> 'C -> int
    TryFind : (int -> bool) -> 'C -> int voption
    TryFindBack : (int -> bool) -> 'C -> int voption
    TryFindIndex : (int -> bool) -> 'C -> int voption
    TryFindIndexBack : (int -> bool) -> 'C -> int voption
    TryPick : (int -> int voption) -> 'C -> int voption

    AllPairs : 'C -> 'C -> struct (int * int) array
    Append : 'C -> 'C -> 'C
    Choose : (int -> int voption) -> 'C -> 'C
    ChunkBySize : int -> 'C -> int[][]
    Collect : (int -> 'C) -> 'C -> 'C
    Concat : seq<'C> -> 'C
    Filter : (int -> bool) -> 'C -> 'C
    Indexed : 'C -> struct (int * int) array
    Map : (int -> int) -> 'C -> 'C
    Map2 : (int -> int -> int) -> 'C -> 'C -> 'C
    Map3 : (int -> int -> int -> int) -> 'C -> 'C -> 'C -> 'C
    Mapi : (int -> int -> int) -> 'C -> 'C
    Mapi2 : (int -> int -> int -> int) -> 'C -> 'C -> 'C
    Pairwise : 'C -> struct (int * int) array
    Permute : (int -> int) -> 'C -> 'C
    Rev : 'C -> 'C
    Transpose : seq<'C> -> int[][]
    /// Unzips a collection of the given pairs.
    Unzip : struct (int * int) array -> struct (int array * int array)
    /// Unzips a collection of the given triples.
    Unzip3 : struct (int * int * int) array -> struct (int array * int array * int array)
    Where : (int -> bool) -> 'C -> 'C
    Windowed : int -> 'C -> int[][]
    Zip : 'C -> 'C -> struct (int * int) array
    Zip3 : 'C -> 'C -> 'C -> struct (int * int * int) array

    /// Averages a collection of the given floats.
    Average : float array -> float
    AverageBy : (int -> float) -> 'C -> float
    CompareWith : (int -> int -> int) -> 'C -> 'C -> int
    Fold : (int array -> int -> int array) -> int array -> 'C -> int array
    Fold2 : (int array -> int -> int -> int array) -> int array -> 'C -> 'C -> int array
    FoldBack : (int -> int array -> int array) -> 'C -> int array -> int array
    FoldBack2 : (int -> int -> int array -> int array) -> 'C -> 'C -> int array -> int array
    Iter : (int -> unit) -> 'C -> unit
    Iter2 : (int -> int -> unit) -> 'C -> 'C -> unit
    Iteri : (int -> int -> unit) -> 'C -> unit
    Iteri2 : (int -> int -> int -> unit) -> 'C -> 'C -> unit
    MapFold : (int -> int -> struct (int * int)) -> int -> 'C -> struct (int array * int)
    MapFoldBack : (int -> int -> struct (int * int)) -> 'C -> int -> struct (int array * int)
    Max : 'C -> int
    MaxBy : (int -> int) -> 'C -> int
    Min : 'C -> int
    MinBy : (int -> int) -> 'C -> int
    Reduce : (int -> int -> int) -> 'C -> int
    ReduceBack : (int -> int -> int) -> 'C -> int
    Scan : (int array -> int -> int array) -> int array -> 'C -> int[][]
    ScanBack : (int -> int array -> int array) -> 'C -> int array -> int[][]
    Sum : 'C -> int
    SumBy : (int -> int) -> 'C -> int

    CountBy : (int -> int) -> 'C -> struct (int * int) array
    Distinct : 'C -> 'C
    DistinctBy : (int -> int) -> 'C -> 'C
    Except : seq<int> -> 'C -> 'C
    GroupBy : (int -> int) -> 'C -> struct (int * int array) array
    Partition : (int -> bool) -> 'C -> struct ('C * 'C)
    PartitionWith : (int -> Choice<int, string>) -> 'C -> struct (int array * string array)
    Skip : int -> 'C -> 'C
    SkipLast : int -> 'C -> 'C
    SkipWhile : (int -> bool) -> 'C -> 'C
    SplitAt : int -> 'C -> struct ('C * 'C)
    SplitInto : int -> 'C -> int[][]
    Take : int -> 'C -> 'C
    TakeLast : int -> 'C -> 'C
    TakeWhile : (int -> bool) -> 'C -> 'C
    Truncate : int -> 'C -> 'C

    Sort : 'C -> 'C
    SortBy : (int -> int) -> 'C -> 'C
    SortByDescending : (int -> int) -> 'C -> 'C
    SortDescending : 'C -> 'C
    SortWith : (int -> int -> int) -> 'C -> 'C
    SortWithComparer : IComparer<int> -> 'C -> 'C

    RandomChoice : 'C -> int
    RandomChoiceBy : (unit -> float) -> 'C -> int
    RandomChoiceWith : Random -> 'C -> int
    RandomChoices : int -> 'C -> 'C
    RandomChoicesBy : (unit -> float) -> int -> 'C -> 'C
    RandomChoicesWith : Random -> int -> 'C -> 'C
    RandomSample : int -> 'C -> 'C
    RandomSampleBy : (unit -> float) -> int -> 'C -> 'C
    RandomSampleWith : Random -> int -> 'C -> 'C
    RandomShuffle : 'C -> 'C
    RandomShuffleBy : (unit -> float) -> 'C -> 'C
    RandomShuffleWith : Random -> 'C -> 'C

    ToList : 'C -> int list
    ToSeq : 'C -> seq<int>
}

/// <summary>
/// The creation functions of a sequence-like module for the contract tests.
/// </summary>
/// <remarks>
/// A function value cannot take a span, so the span functions take an array that the instance turns into a span.
/// </remarks>
type SeqCreationOps<'C> = {
    /// The name of the module, for the messages of the assertions.
    Name : string
    /// The FSharp.Core module whose exceptions the module mirrors.
    Mirror : Mirror
    /// Maps the name of a parameter of the FSharp.Core oracle to the name of the parameter of the module.
    ParamName : string -> string
    /// The elements in the order in which the collection enumerates them.
    ToArray : 'C -> int array

    Empty : unit -> 'C
    Init : int -> (int -> int) -> 'C
    OfArray : int array -> 'C
    OfList : int list -> 'C
    OfReadOnlySpan : int array -> 'C
    OfSeq : seq<int> -> 'C
    OfSpan : int array -> 'C
    Replicate : int -> int -> 'C
    Singleton : int -> 'C
    Unfold : (int -> struct (int * int) voption) -> int -> 'C
}

/// <summary>
/// The index-based functions of a sequence-like module for the contract tests.
/// </summary>
type IndexedOps<'C> = {
    /// The name of the module, for the messages of the assertions.
    Name : string
    /// The FSharp.Core module whose exceptions the module mirrors.
    Mirror : Mirror
    /// Maps the name of a parameter of the FSharp.Core oracle to the name of the parameter of the module.
    ParamName : string -> string
    /// Creates a collection that enumerates the given elements in their order.
    OfSeq : seq<int> -> 'C
    /// The elements in the order in which the collection enumerates them.
    ToArray : 'C -> int array

    Item : int -> 'C -> int
    TryItem : int -> 'C -> int voption

    IndexOf : int -> 'C -> int
    IndexOfWith : IEqualityComparer<int> -> int -> 'C -> int
    LastIndexOf : int -> 'C -> int
    LastIndexOfWith : IEqualityComparer<int> -> int -> 'C -> int

    Add : int -> 'C -> 'C
    InsertAt : int -> int -> 'C -> 'C
    InsertManyAt : int -> seq<int> -> 'C -> 'C
    RemoveAt : int -> 'C -> 'C
    RemoveManyAt : int -> int -> 'C -> 'C
    UpdateAt : int -> int -> 'C -> 'C
}
