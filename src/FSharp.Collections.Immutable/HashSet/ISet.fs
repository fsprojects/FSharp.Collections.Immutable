[<RequireQualifiedAccess>]
module FSharp.Collections.Immutable.ISet

open System
open System.Collections.Immutable
open System.Linq

// The functions call the members of IImmutableSet. A transformation that keeps the element type and builds a new set
// starts from Clear, which keeps the implementation and the comparer of its input, and fills it with Union (ADR 0001,
// D5). The interface cannot tell its comparer, so the transformations to another element type return a HashSet with the
// default comparer of that type.

// ---- Properties ----

[<CompiledName "Count">]
let count (set : IImmutableSet<'T>) =
    checkNotNull (nameof set) set
    set.Count

[<CompiledName "IsEmpty">]
let isEmpty (set : IImmutableSet<'T>) =
    checkNotNull (nameof set) set
    set.Count = 0

// ---- Access ----

[<CompiledName "TryGetValue">]
let tryGetValue (value : 'T) (set : IImmutableSet<'T>) =
    checkNotNull (nameof set) set

    match set.TryGetValue value with
    | true, actual -> ValueSome actual
    | false, _ -> ValueNone

// ---- Search ----

[<CompiledName "Contains">]
let contains (element : 'T) (set : IImmutableSet<'T>) =
    checkNotNull (nameof set) set
    set.Contains element

// exists and forall call Seq, which allocates only the enumerator of the set: 144 bytes per call for a hash set and 64 for
// a sorted set, against 232 and 152 for LINQ Any and All, which also allocate a delegate (measured in Release; a measured
// faster path replaces a higher priority, D5).

[<CompiledName "Exists">]
let exists (predicate : 'T -> bool) (set : IImmutableSet<'T>) =
    checkNotNull (nameof set) set
    Seq.exists predicate set

[<CompiledName "ForAll">]
let forall (predicate : 'T -> bool) (set : IImmutableSet<'T>) =
    checkNotNull (nameof set) set
    Seq.forall predicate set

[<CompiledName "IsProperSubset">]
let isProperSubset (set1 : IImmutableSet<'T>) (set2 : IImmutableSet<'T>) =
    checkNotNull (nameof set1) set1
    checkNotNull (nameof set2) set2
    set1.IsProperSubsetOf set2

[<CompiledName "IsProperSuperset">]
let isProperSuperset (set1 : IImmutableSet<'T>) (set2 : IImmutableSet<'T>) =
    checkNotNull (nameof set1) set1
    checkNotNull (nameof set2) set2

    // ImmutableHashSet and ImmutableSortedSet count every element of the other sequence in IsProperSupersetOf, so
    // elements that the comparer of set1 finds equal count twice, against its documentation (ADR 0001, D14). A proper
    // superset is a superset that is not a subset; the interface does not tell the comparers, and only IsSubsetOf
    // allocates, one helper set of set2.
    set1.IsSupersetOf set2 && not (set1.IsSubsetOf set2)

[<CompiledName "IsSubset">]
let isSubset (set1 : IImmutableSet<'T>) (set2 : IImmutableSet<'T>) =
    checkNotNull (nameof set1) set1
    checkNotNull (nameof set2) set2
    set1.IsSubsetOf set2

[<CompiledName "IsSuperset">]
let isSuperset (set1 : IImmutableSet<'T>) (set2 : IImmutableSet<'T>) =
    checkNotNull (nameof set1) set1
    checkNotNull (nameof set2) set2
    set1.IsSupersetOf set2

[<CompiledName "Overlaps">]
let overlaps (set1 : IImmutableSet<'T>) (set2 : IImmutableSet<'T>) =
    checkNotNull (nameof set1) set1
    checkNotNull (nameof set2) set2
    set1.Overlaps set2

[<CompiledName "SetEquals">]
let setEquals (set1 : IImmutableSet<'T>) (set2 : IImmutableSet<'T>) =
    checkNotNull (nameof set1) set1
    checkNotNull (nameof set2) set2
    set1.SetEquals set2

// ---- Transformation ----

[<CompiledName "Add">]
let add (value : 'T) (set : IImmutableSet<'T>) =
    checkNotNull (nameof set) set
    set.Add value

[<CompiledName "AddRange">]
let addRange (elements : seq<'T>) (set : IImmutableSet<'T>) =
    checkNotNullOrDefaultSeq (nameof elements) elements
    checkNotNull (nameof set) set
    let result = set.Union elements

    // ImmutableHashSet returns another instance when an empty set receives a hash set as the argument, and the interface
    // promises no instance at all. Union only adds, so an unchanged count means that nothing was added, and the input
    // itself is the answer for every implementation (ADR 0001, D14).
    if result.Count = set.Count then set else result

[<CompiledName "Choose">]
let choose (chooser : 'T -> 'U voption) (set : IImmutableSet<'T>) : HashSet<'U> =
    checkNotNull (nameof set) set
    HashSets.choose chooser set

[<CompiledName "Clear">]
let clear (set : IImmutableSet<'T>) =
    checkNotNull (nameof set) set
    set.Clear ()

[<CompiledName "Difference">]
let difference (set1 : IImmutableSet<'T>) (set2 : IImmutableSet<'T>) =
    checkNotNull (nameof set1) set1
    checkNotNull (nameof set2) set2
    set1.Except set2

[<CompiledName "Filter">]
let filter (predicate : 'T -> bool) (set : IImmutableSet<'T>) =
    checkNotNull (nameof set) set
    // LINQ Where with its delegate stays: Seq.filter allocates as many bytes and runs slower, a list of the accepted
    // elements allocates more for large sets (32 536 against 28 336 bytes for a hash set of 1 000), and removing the
    // rejected elements allocates ten times as much (measured in Release, D5)
    set.Clear().Union(Enumerable.Where (set, Func<'T, bool> predicate))

[<CompiledName "Intersect">]
let intersect (set1 : IImmutableSet<'T>) (set2 : IImmutableSet<'T>) =
    checkNotNull (nameof set1) set1
    checkNotNull (nameof set2) set2
    set1.Intersect set2

[<CompiledName "IntersectMany">]
let intersectMany (sets : seq<IImmutableSet<'T>>) =
    // Set.intersectMany is Seq.reduce over Set.intersect, so its checks are those of Seq.reduce
    checkNotNullOrDefaultSeq (nameof sets) sets

    // The type of a set is annotated, because the sequence that fixes it comes last
    let checkSet (set : IImmutableSet<'T>) = checkNotNull (nameof sets) set

    match Sets.reduce checkSet (fun set1 set2 -> set1.Intersect set2) sets with
    | ValueSome result -> result
    | ValueNone -> invalidArg (nameof sets) ErrorStrings.InputSequenceEmpty

[<CompiledName "Map">]
let map (mapping : 'T -> 'U) (set : IImmutableSet<'T>) : HashSet<'U> =
    checkNotNull (nameof set) set
    HashSets.map mapping set

[<CompiledName "Remove">]
let remove (value : 'T) (set : IImmutableSet<'T>) =
    checkNotNull (nameof set) set
    set.Remove value

[<CompiledName "RemoveRange">]
let removeRange (elements : seq<'T>) (set : IImmutableSet<'T>) =
    checkNotNullOrDefaultSeq (nameof elements) elements
    checkNotNull (nameof set) set
    // ImmutableHashSet and ImmutableSortedSet return the input itself when nothing is removed (ADR 0001, D14), so unlike
    // in addRange no check of the result is needed
    set.Except elements

[<CompiledName "SymmetricDifference">]
let symmetricDifference (set1 : IImmutableSet<'T>) (set2 : IImmutableSet<'T>) =
    checkNotNull (nameof set1) set1
    checkNotNull (nameof set2) set2
    set1.SymmetricExcept set2

[<CompiledName "Union">]
let union (set1 : IImmutableSet<'T>) (set2 : IImmutableSet<'T>) =
    checkNotNull (nameof set1) set1
    checkNotNull (nameof set2) set2
    set1.Union set2

[<CompiledName "UnionMany">]
let unionMany (sets : seq<IImmutableSet<'T>>) =
    checkNotNullOrDefaultSeq (nameof sets) sets

    let checkSet (set : IImmutableSet<'T>) = checkNotNull (nameof sets) set

    // Starting from the first set keeps its implementation and comparer; without a set, the result is a HashSet
    match Sets.reduce checkSet (fun set1 set2 -> set1.Union set2) sets with
    | ValueSome result -> result
    | ValueNone -> ImmutableHashSet<'T>.Empty :> IImmutableSet<'T>

// ---- Folding ----

[<CompiledName "Fold">]
let fold (folder : 'State -> 'T -> 'State) (state : 'State) (set : IImmutableSet<'T>) =
    checkNotNull (nameof set) set
    Seq.fold folder state set

[<CompiledName "FoldBack">]
let foldBack (folder : 'T -> 'State -> 'State) (set : IImmutableSet<'T>) (state : 'State) =
    checkNotNull (nameof set) set
    Seq.foldBack folder set state

[<CompiledName "Iterate">]
let iter (action : 'T -> unit) (set : IImmutableSet<'T>) =
    checkNotNull (nameof set) set
    Seq.iter action set

// ---- Partitioning ----

[<CompiledName "Partition">]
let partition (predicate : 'T -> bool) (set : IImmutableSet<'T>) =
    checkNotNull (nameof set) set
    let accepted = ResizeArray<'T>()
    let rejected = ResizeArray<'T>()

    for item in set do
        if predicate item then
            accepted.Add item
        else
            rejected.Add item

    let empty = set.Clear ()
    struct (empty.Union accepted, empty.Union rejected)

[<CompiledName "PartitionWith">]
let partitionWith (partitioner : 'T -> Choice<'T1, 'T2>) (set : IImmutableSet<'T>) : struct (HashSet<'T1> * HashSet<'T2>) =
    checkNotNull (nameof set) set
    HashSets.partitionWith partitioner set

// ---- Conversion ----

[<CompiledName "ToArray">]
let toArray (set : IImmutableSet<'T>) =
    checkNotNull (nameof set) set
    Enumerable.ToArray set

[<CompiledName "ToList">]
let toList (set : IImmutableSet<'T>) =
    checkNotNull (nameof set) set
    List.ofSeq set

[<CompiledName "ToSeq">]
let toSeq (set : IImmutableSet<'T>) =
    checkNotNull (nameof set) set
    set :> seq<'T>
