[<RequireQualifiedAccess>]
module FSharp.Collections.Immutable.FrozenSet

open System
open System.Collections.Frozen
open System.Collections.Generic

// The functions call the members of FrozenSet: Contains, TryGetValue, the set predicates, Comparer and Items, whose array
// the reading functions pass to the FSharp.Core Array functions without a copy (ADR 0001, D5). A frozen set has no
// builder, so a transformation collects its result in a System.Collections.Generic.HashSet with the comparer of the
// result: ToFrozenSet takes a hash set with the requested comparer over without copying it into another one. In this file
// HashSet is that mutable hash set, because System.Collections.Generic shadows the HashSet abbreviation of this namespace.
// FSharp.Core sets cannot be null, so the null checks name the parameters of this module (D6).

/// The struct with the default comparer: the creation functions are its members, so that each has one implementation (D12).
let private withDefaultComparer<'T> () = CustomComparerFrozenSet<'T>(EqualityComparer<'T>.Default)

// ---- Creation ----

[<GeneralizableValue>]
[<CompiledName "Empty">]
let empty<'T> : FrozenSet<'T> = FrozenSet<'T>.Empty

[<CompiledName "ForComparer">]
let forComparer (comparer : IEqualityComparer<'T>) = CustomComparerFrozenSet<'T> comparer

[<CompiledName "OfArray">]
let ofArray (array : 'T array) = (withDefaultComparer ()).ofArray array

[<CompiledName "OfList">]
let ofList (elements : 'T list) = (withDefaultComparer ()).ofList elements

[<CompiledName "OfReadOnlySpan">]
let ofReadOnlySpan (span : ReadOnlySpan<'T>) = (withDefaultComparer ()).ofReadOnlySpan span

[<CompiledName "OfSeq">]
let ofSeq (elements : seq<'T>) = (withDefaultComparer ()).ofSeq elements

[<CompiledName "OfSpan">]
let ofSpan (span : Span<'T>) = (withDefaultComparer ()).ofSpan span

[<CompiledName "Singleton">]
let singleton (value : 'T) = (withDefaultComparer ()).singleton value

// ---- Properties ----

[<CompiledName "Comparer">]
let comparer (set : FrozenSet<'T>) =
    checkNotNull (nameof set) set
    set.Comparer

[<CompiledName "Count">]
let count (set : FrozenSet<'T>) =
    checkNotNull (nameof set) set
    set.Count

[<CompiledName "IsEmpty">]
let isEmpty (set : FrozenSet<'T>) =
    checkNotNull (nameof set) set
    set.Count = 0

// ---- Access ----

[<CompiledName "TryGetValue">]
let tryGetValue (value : 'T) (set : FrozenSet<'T>) =
    checkNotNull (nameof set) set

    match set.TryGetValue value with
    | true, actual -> ValueSome actual
    | false, _ -> ValueNone

// ---- Search ----

[<CompiledName "Contains">]
let contains (element : 'T) (set : FrozenSet<'T>) =
    checkNotNull (nameof set) set
    set.Contains element

[<CompiledName "Exists">]
let exists (predicate : 'T -> bool) (set : FrozenSet<'T>) =
    checkNotNull (nameof set) set
    Array.exists predicate (unwrapArray (nameof set) set.Items)

[<CompiledName "ForAll">]
let forall (predicate : 'T -> bool) (set : FrozenSet<'T>) =
    checkNotNull (nameof set) set
    Array.forall predicate (unwrapArray (nameof set) set.Items)

[<CompiledName "IsProperSubset">]
let isProperSubset (set1 : FrozenSet<'T>) (set2 : FrozenSet<'T>) =
    checkNotNull (nameof set1) set1
    checkNotNull (nameof set2) set2
    set1.IsProperSubsetOf set2

[<CompiledName "IsProperSuperset">]
let isProperSuperset (set1 : FrozenSet<'T>) (set2 : FrozenSet<'T>) =
    checkNotNull (nameof set1) set1
    checkNotNull (nameof set2) set2
    set1.IsProperSupersetOf set2

[<CompiledName "IsSubset">]
let isSubset (set1 : FrozenSet<'T>) (set2 : FrozenSet<'T>) =
    checkNotNull (nameof set1) set1
    checkNotNull (nameof set2) set2
    set1.IsSubsetOf set2

[<CompiledName "IsSuperset">]
let isSuperset (set1 : FrozenSet<'T>) (set2 : FrozenSet<'T>) =
    checkNotNull (nameof set1) set1
    checkNotNull (nameof set2) set2
    set1.IsSupersetOf set2

[<CompiledName "Overlaps">]
let overlaps (set1 : FrozenSet<'T>) (set2 : FrozenSet<'T>) =
    checkNotNull (nameof set1) set1
    checkNotNull (nameof set2) set2
    set1.Overlaps set2

[<CompiledName "SetEquals">]
let setEquals (set1 : FrozenSet<'T>) (set2 : FrozenSet<'T>) =
    checkNotNull (nameof set1) set1
    checkNotNull (nameof set2) set2
    set1.SetEquals set2

// ---- Transformation ----

[<CompiledName "Choose">]
let choose (chooser : 'T -> 'U voption) (set : FrozenSet<'T>) =
    checkNotNull (nameof set) set
    let chosen = HashSet<'U>()

    for item in set.Items do
        match chooser item with
        | ValueSome value -> chosen.Add value |> ignore
        | ValueNone -> ()

    chosen.ToFrozenSet ()

[<CompiledName "Difference">]
let difference (set1 : FrozenSet<'T>) (set2 : FrozenSet<'T>) =
    checkNotNull (nameof set1) set1
    checkNotNull (nameof set2) set2
    let difference = HashSet<'T>(set1, set1.Comparer)
    difference.ExceptWith set2
    difference.ToFrozenSet set1.Comparer

[<CompiledName "Filter">]
let filter (predicate : 'T -> bool) (set : FrozenSet<'T>) =
    checkNotNull (nameof set) set
    let accepted = HashSet<'T> set.Comparer

    for item in set.Items do
        if predicate item then
            accepted.Add item |> ignore

    accepted.ToFrozenSet set.Comparer

[<CompiledName "Intersect">]
let intersect (set1 : FrozenSet<'T>) (set2 : FrozenSet<'T>) =
    checkNotNull (nameof set1) set1
    checkNotNull (nameof set2) set2
    // The elements of the second set are looked up in the first one, in the comparer of the first set. The intersection
    // keeps the elements of the second set, as ImmutableHashSet.Intersect and ImmutableSortedSet.Intersect do; FrozenSet
    // has no such member, so it follows them (ADR 0001, D14)
    let intersection = HashSet<'T> set1.Comparer

    for item in set2.Items do
        if set1.Contains item then
            intersection.Add item |> ignore

    intersection.ToFrozenSet set1.Comparer

[<CompiledName "IntersectMany">]
let intersectMany (sets : seq<FrozenSet<'T>>) =
    // Set.intersectMany is Seq.reduce over Set.intersect, so its checks are those of Seq.reduce. A default flat list given as
    // a sequence would fail with InvalidOperationException when the reduction enumerates it (ADR 0001, D6)
    checkNotNullOrDefaultSeq (nameof sets) sets

    // The type of a set is annotated, because the sequence that fixes it comes last
    let checkSet (set : FrozenSet<'T>) = checkNotNull (nameof sets) set

    // Each step builds a frozen set, but an intersection never grows: it is at most as large as the set that the step
    // looks up, so the steps together take time in proportion to the elements of the sequence
    match Sets.reduce checkSet intersect sets with
    | ValueSome result -> result
    | ValueNone -> invalidArg (nameof sets) ErrorStrings.InputSequenceEmpty

[<CompiledName "Map">]
let map (mapping : 'T -> 'U) (set : FrozenSet<'T>) =
    checkNotNull (nameof set) set
    let mapped = HashSet<'U>()

    for item in set.Items do
        mapped.Add (mapping item) |> ignore

    mapped.ToFrozenSet ()

[<CompiledName "SymmetricDifference">]
let symmetricDifference (set1 : FrozenSet<'T>) (set2 : FrozenSet<'T>) =
    checkNotNull (nameof set1) set1
    checkNotNull (nameof set2) set2
    let difference = HashSet<'T>(set1, set1.Comparer)
    difference.SymmetricExceptWith set2
    difference.ToFrozenSet set1.Comparer

[<CompiledName "Union">]
let union (set1 : FrozenSet<'T>) (set2 : FrozenSet<'T>) =
    checkNotNull (nameof set1) set1
    checkNotNull (nameof set2) set2
    let union = HashSet<'T>(set1, set1.Comparer)
    union.UnionWith set2
    union.ToFrozenSet set1.Comparer

[<CompiledName "UnionMany">]
let unionMany (sets : seq<FrozenSet<'T>>) =
    // Set.unionMany is Seq.fold over Set.union, so its checks are those of Seq.fold. A default flat list given as a
    // sequence would fail with InvalidOperationException when the fold enumerates it (ADR 0001, D6)
    checkNotNullOrDefaultSeq (nameof sets) sets

    // The fold collects the elements of every set in one hash set with the comparer of the first set and freezes them
    // once. Uniting the frozen sets pairwise, as intersectMany intersects them, would rebuild a growing frozen set for
    // every set of the sequence: quadratic time for many sets.
    let union =
        (null, sets)
        ||> Seq.fold (fun (union : HashSet<'T> | null) set ->
            checkNotNull (nameof sets) set

            match union with
            | null -> HashSet<'T>(set, set.Comparer)
            | union ->
                union.UnionWith set
                union
        )

    match union with
    | null -> empty
    | union -> union.ToFrozenSet union.Comparer

// ---- Folding ----

[<CompiledName "Fold">]
let fold (folder : 'State -> 'T -> 'State) (state : 'State) (set : FrozenSet<'T>) =
    checkNotNull (nameof set) set
    Array.fold folder state (unwrapArray (nameof set) set.Items)

[<CompiledName "FoldBack">]
let foldBack (folder : 'T -> 'State -> 'State) (set : FrozenSet<'T>) (state : 'State) =
    checkNotNull (nameof set) set
    Array.foldBack folder (unwrapArray (nameof set) set.Items) state

[<CompiledName "Iterate">]
let iter (action : 'T -> unit) (set : FrozenSet<'T>) =
    checkNotNull (nameof set) set
    Array.iter action (unwrapArray (nameof set) set.Items)

// ---- Partitioning ----

[<CompiledName "Partition">]
let partition (predicate : 'T -> bool) (set : FrozenSet<'T>) =
    checkNotNull (nameof set) set
    let accepted = HashSet<'T> set.Comparer
    let rejected = HashSet<'T> set.Comparer

    for item in set.Items do
        if predicate item then
            accepted.Add item |> ignore
        else
            rejected.Add item |> ignore

    struct (accepted.ToFrozenSet set.Comparer, rejected.ToFrozenSet set.Comparer)

[<CompiledName "PartitionWith">]
let partitionWith (partitioner : 'T -> Choice<'T1, 'T2>) (set : FrozenSet<'T>) =
    checkNotNull (nameof set) set
    let first = HashSet<'T1>()
    let second = HashSet<'T2>()

    for item in set.Items do
        match partitioner item with
        | Choice1Of2 value -> first.Add value |> ignore
        | Choice2Of2 value -> second.Add value |> ignore

    struct (first.ToFrozenSet (), second.ToFrozenSet ())

// ---- Conversion ----

[<CompiledName "ToArray">]
let toArray (set : FrozenSet<'T>) =
    checkNotNull (nameof set) set
    Array.copy (unwrapArray (nameof set) set.Items)

[<CompiledName "ToFlatList">]
let toFlatList (set : FrozenSet<'T>) : FlatList<'T> =
    checkNotNull (nameof set) set
    set.Items

[<CompiledName "ToList">]
let toList (set : FrozenSet<'T>) =
    checkNotNull (nameof set) set
    Array.toList (unwrapArray (nameof set) set.Items)

[<CompiledName "ToSeq">]
let toSeq (set : FrozenSet<'T>) =
    checkNotNull (nameof set) set
    set :> seq<'T>

#if NET10_0_OR_GREATER
// ---- Type-specific ----

[<CompiledName "AlternateLookup">]
let alternateLookup<'Alternate, 'T> (set : FrozenSet<'T>) =
    checkNotNull (nameof set) set
    set.GetAlternateLookup<'Alternate>()

[<CompiledName "TryAlternateLookup">]
let tryAlternateLookup<'Alternate, 'T> (set : FrozenSet<'T>) =
    checkNotNull (nameof set) set

    match set.TryGetAlternateLookup<'Alternate>() with
    | true, lookup -> ValueSome lookup
    | false, _ -> ValueNone
#endif
