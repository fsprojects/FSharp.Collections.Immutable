[<RequireQualifiedAccess>]
module FSharp.Collections.Immutable.SortedSet

open System
open System.Collections.Generic
open System.Collections.Immutable
open System.Linq
// Opened after System.Collections.Generic, so that SortedSet names the abbreviation of this library, not the mutable set
open FSharp.Collections.Immutable

// The functions call the members of ImmutableSortedSet: Contains, TryGetValue, IndexOf, the indexer, Reverse, Add,
// Remove, Clear, the set algebra and the builders (ADR 0001, D5). The sorted sets created without a comparer compare with
// F# generic comparison, the comparer of the F# set (D5, Comparers); DefaultComparer resolves it once per type, so that
// the sorted sets of one type share it. The creation functions are the members of CustomComparerSortedSet, which add the
// elements to a builder: a builder keeps the first of the elements that compare equal as Set.ofSeq does, while
// CreateRange sorts unstably and can keep any of them. FSharp.Core sets cannot be null, so the null checks name the
// parameters of this module (D6).

/// The struct with the default comparer: the creation functions are its members, so that each has one implementation (D12).
let private withDefaultComparer<'T when 'T : comparison> () = CustomComparerSortedSet<'T>(DefaultComparer<'T>.Instance)

// ---- Creation ----

[<GeneralizableValue>]
[<CompiledName "Empty">]
let empty<'T when 'T : comparison> : SortedSet<'T> = (withDefaultComparer<'T>()).empty

[<CompiledName "ForComparer">]
let forComparer (comparer : IComparer<'T>) = CustomComparerSortedSet<'T> comparer

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

// ---- Builder ----

[<CompiledName "Build">]
let build (action : SortedSet<'T>.Builder -> unit) = (withDefaultComparer ()).build action

[<CompiledName "CreateBuilder">]
let builder () : SortedSet<'T>.Builder = (withDefaultComparer<'T>()).builder()

[<CompiledName "OfBuilder">]
let ofBuilder (builder : SortedSet<'T>.Builder) =
    checkNotNull (nameof builder) builder
    builder.ToImmutable ()

[<CompiledName "ToBuilder">]
let toBuilder (set : SortedSet<'T>) =
    checkNotNull (nameof set) set
    set.ToBuilder ()

[<CompiledName "Update">]
let update (action : SortedSet<'T>.Builder -> unit) (set : SortedSet<'T>) =
    checkNotNull (nameof set) set
    let builder = set.ToBuilder ()
    action builder
    builder.ToImmutable ()

// ---- Properties ----

[<CompiledName "Comparer">]
let comparer (set : SortedSet<'T>) =
    checkNotNull (nameof set) set
    set.KeyComparer

[<CompiledName "Count">]
let count (set : SortedSet<'T>) =
    checkNotNull (nameof set) set
    set.Count

[<CompiledName "IsEmpty">]
let isEmpty (set : SortedSet<'T>) =
    checkNotNull (nameof set) set
    set.IsEmpty

// ---- Access ----

[<CompiledName "IndexOf">]
let indexOf (value : 'T) (set : SortedSet<'T>) =
    checkNotNull (nameof set) set
    set.IndexOf value

[<CompiledName "Item">]
let item (index : int) (set : SortedSet<'T>) =
    checkNotNull (nameof set) set
    set[index]

[<CompiledName "MaxElement">]
let maxElement (set : SortedSet<'T>) =
    checkNotNull (nameof set) set

    if set.IsEmpty then
        invalidArg (nameof set) ErrorStrings.SetContainsNoElements

    // The indexer returns the element type. Max returns the default value for an empty sorted set, so it is nullable,
    // and Unchecked.nonNull would constrain the element type to reference types.
    set[set.Count - 1]

[<CompiledName "MinElement">]
let minElement (set : SortedSet<'T>) =
    checkNotNull (nameof set) set

    if set.IsEmpty then
        invalidArg (nameof set) ErrorStrings.SetContainsNoElements

    // The indexer returns the element type. Min returns the default value for an empty sorted set, so it is nullable,
    // and Unchecked.nonNull would constrain the element type to reference types.
    set[0]

[<CompiledName "TryGetValue">]
let tryGetValue (value : 'T) (set : SortedSet<'T>) =
    checkNotNull (nameof set) set

    match set.TryGetValue value with
    | true, actual -> ValueSome actual
    | false, _ -> ValueNone

[<CompiledName "TryItem">]
let tryItem (index : int) (set : SortedSet<'T>) =
    checkNotNull (nameof set) set

    if index >= 0 && index < set.Count then
        ValueSome set[index]
    else
        ValueNone

// ---- Search ----

[<CompiledName "Contains">]
let contains (element : 'T) (set : SortedSet<'T>) =
    checkNotNull (nameof set) set
    set.Contains element

// exists and forall loop over the struct enumerator of the sorted set, which allocates nothing. LINQ Any and All allocated
// 152 bytes per call, a delegate and the boxed enumerator, and Seq.exists and Seq.forall 64 bytes, the boxed enumerator
// (measured in Release; a measured faster path replaces a higher priority, D5). Dispose returns the pooled stack of the
// enumerator.

[<CompiledName "Exists">]
let exists (predicate : 'T -> bool) (set : SortedSet<'T>) =
    checkNotNull (nameof set) set
    let mutable enumerator = set.GetEnumerator ()

    try
        let mutable found = false

        while not found && enumerator.MoveNext () do
            found <- predicate enumerator.Current

        found
    finally
        enumerator.Dispose ()

[<CompiledName "ForAll">]
let forall (predicate : 'T -> bool) (set : SortedSet<'T>) =
    checkNotNull (nameof set) set
    let mutable enumerator = set.GetEnumerator ()

    try
        let mutable satisfied = true

        while satisfied && enumerator.MoveNext () do
            satisfied <- predicate enumerator.Current

        satisfied
    finally
        enumerator.Dispose ()

[<CompiledName "IsProperSubset">]
let isProperSubset (set1 : SortedSet<'T>) (set2 : SortedSet<'T>) =
    checkNotNull (nameof set1) set1
    checkNotNull (nameof set2) set2
    set1.IsProperSubsetOf set2

[<CompiledName "IsProperSuperset">]
let isProperSuperset (set1 : SortedSet<'T>) (set2 : SortedSet<'T>) =
    checkNotNull (nameof set1) set1
    checkNotNull (nameof set2) set2

    // ImmutableSortedSet.IsProperSupersetOf counts every element of the other sequence, so elements that the comparer of
    // set1 finds equal count twice, against its documentation (ADR 0001, D14). The count is right when set2 orders with
    // the same comparer; otherwise a proper superset is a superset that is not a subset, and only IsSubsetOf allocates,
    // one helper set of set2.
    if obj.ReferenceEquals (set1.KeyComparer, set2.KeyComparer) then
        set1.IsProperSupersetOf set2
    else
        set1.IsSupersetOf set2 && not (set1.IsSubsetOf set2)

[<CompiledName "IsSubset">]
let isSubset (set1 : SortedSet<'T>) (set2 : SortedSet<'T>) =
    checkNotNull (nameof set1) set1
    checkNotNull (nameof set2) set2
    set1.IsSubsetOf set2

[<CompiledName "IsSuperset">]
let isSuperset (set1 : SortedSet<'T>) (set2 : SortedSet<'T>) =
    checkNotNull (nameof set1) set1
    checkNotNull (nameof set2) set2
    set1.IsSupersetOf set2

[<CompiledName "Overlaps">]
let overlaps (set1 : SortedSet<'T>) (set2 : SortedSet<'T>) =
    checkNotNull (nameof set1) set1
    checkNotNull (nameof set2) set2
    set1.Overlaps set2

[<CompiledName "SetEquals">]
let setEquals (set1 : SortedSet<'T>) (set2 : SortedSet<'T>) =
    checkNotNull (nameof set1) set1
    checkNotNull (nameof set2) set2
    set1.SetEquals set2

// ---- Transformation ----

[<CompiledName "Add">]
let add (value : 'T) (set : SortedSet<'T>) =
    checkNotNull (nameof set) set
    set.Add value

// Union is not used: when the sequence is large against the set, it refills the tree from an unstable sort and can keep an
// element of the sequence in place of the equal element of the set (measured: 4 of 25 elements of the set kept for 250
// added elements). The builder adds the elements one by one, so the element of the set stays and the first of several
// equal new elements is added in every case (ADR 0001, D14).
[<CompiledName "AddRange">]
let addRange (elements : seq<'T>) (set : SortedSet<'T>) =
    checkNotNullOrDefaultSeq (nameof elements) elements
    checkNotNull (nameof set) set
    let builder = set.ToBuilder ()
    let mutable added = false

    for item in elements do
        if builder.Add item then
            added <- true

    // The builder returns the input from ToImmutable when nothing changed, but the BCL does not document it; the flag makes
    // the promise of the function independent of that
    if added then builder.ToImmutable () else set

[<CompiledName "Choose">]
let choose (chooser : 'T -> 'U voption) (set : SortedSet<'T>) =
    checkNotNull (nameof set) set
    SortedSets.choose chooser set

[<CompiledName "Clear">]
let clear (set : SortedSet<'T>) =
    checkNotNull (nameof set) set
    set.Clear ()

[<CompiledName "Difference">]
let difference (set1 : SortedSet<'T>) (set2 : SortedSet<'T>) =
    checkNotNull (nameof set1) set1
    checkNotNull (nameof set2) set2
    set1.Except set2

[<CompiledName "Filter">]
let filter (predicate : 'T -> bool) (set : SortedSet<'T>) =
    checkNotNull (nameof set) set
    let builder = ImmutableSortedSet.CreateBuilder<'T> set.KeyComparer

    for item in set do
        if predicate item then
            builder.Add item |> ignore

    builder.ToImmutable ()

[<CompiledName "Intersect">]
let intersect (set1 : SortedSet<'T>) (set2 : SortedSet<'T>) =
    checkNotNull (nameof set1) set1
    checkNotNull (nameof set2) set2
    set1.Intersect set2

[<CompiledName "IntersectMany">]
let intersectMany (sets : seq<SortedSet<'T>>) =
    // Set.intersectMany is Seq.reduce over Set.intersect, so its checks are those of Seq.reduce
    checkNotNullOrDefaultSeq (nameof sets) sets

    // The type of a set is annotated, because the sequence that fixes it comes last
    let checkSet (set : SortedSet<'T>) = checkNotNull (nameof sets) set

    match Sets.reduce checkSet (fun set1 set2 -> set1.Intersect set2) sets with
    | ValueSome result -> result
    | ValueNone -> invalidArg (nameof sets) ErrorStrings.InputSequenceEmpty

[<CompiledName "Map">]
let map (mapping : 'T -> 'U) (set : SortedSet<'T>) =
    checkNotNull (nameof set) set
    SortedSets.map mapping set

[<CompiledName "Remove">]
let remove (value : 'T) (set : SortedSet<'T>) =
    checkNotNull (nameof set) set
    set.Remove value

[<CompiledName "RemoveRange">]
let removeRange (elements : seq<'T>) (set : SortedSet<'T>) =
    checkNotNullOrDefaultSeq (nameof elements) elements
    checkNotNull (nameof set) set
    // ImmutableSortedSet returns the input itself when nothing is removed (ADR 0001, D14), so unlike in addRange no check
    // of the result is needed
    set.Except elements

[<CompiledName "Reverse">]
let rev (set : SortedSet<'T>) =
    checkNotNull (nameof set) set
    set.Reverse ()

[<CompiledName "SymmetricDifference">]
let symmetricDifference (set1 : SortedSet<'T>) (set2 : SortedSet<'T>) =
    checkNotNull (nameof set1) set1
    checkNotNull (nameof set2) set2
    set1.SymmetricExcept set2

[<CompiledName "Union">]
let union (set1 : SortedSet<'T>) (set2 : SortedSet<'T>) =
    checkNotNull (nameof set1) set1
    checkNotNull (nameof set2) set2
    set1.Union set2

[<CompiledName "UnionMany">]
let unionMany (sets : seq<SortedSet<'T>>) =
    checkNotNullOrDefaultSeq (nameof sets) sets

    let checkSet (set : SortedSet<'T>) = checkNotNull (nameof sets) set

    // Set.unionMany folds from the empty set; starting from the first set instead keeps its comparer
    match Sets.reduce checkSet (fun set1 set2 -> set1.Union set2) sets with
    | ValueSome result -> result
    | ValueNone -> empty

// ---- Folding ----

[<CompiledName "Fold">]
let fold (folder : 'State -> 'T -> 'State) (state : 'State) (set : SortedSet<'T>) =
    checkNotNull (nameof set) set
    Seq.fold folder state set

[<CompiledName "FoldBack">]
let foldBack (folder : 'T -> 'State -> 'State) (set : SortedSet<'T>) (state : 'State) =
    checkNotNull (nameof set) set
    // Reverse walks the tree from the largest element, so the elements are not copied into an array first
    Seq.fold (fun state item -> folder item state) state (set.Reverse ())

[<CompiledName "Iterate">]
let iter (action : 'T -> unit) (set : SortedSet<'T>) =
    checkNotNull (nameof set) set
    Seq.iter action set

// ---- Partitioning ----

[<CompiledName "Partition">]
let partition (predicate : 'T -> bool) (set : SortedSet<'T>) =
    checkNotNull (nameof set) set
    let accepted = ImmutableSortedSet.CreateBuilder<'T> set.KeyComparer
    let rejected = ImmutableSortedSet.CreateBuilder<'T> set.KeyComparer

    for item in set do
        if predicate item then
            accepted.Add item |> ignore
        else
            rejected.Add item |> ignore

    struct (accepted.ToImmutable (), rejected.ToImmutable ())

[<CompiledName "PartitionWith">]
let partitionWith (partitioner : 'T -> Choice<'T1, 'T2>) (set : SortedSet<'T>) =
    checkNotNull (nameof set) set
    SortedSets.partitionWith partitioner set

// ---- Conversion ----

[<CompiledName "ToArray">]
let toArray (set : SortedSet<'T>) =
    checkNotNull (nameof set) set
    Enumerable.ToArray set

[<CompiledName "ToList">]
let toList (set : SortedSet<'T>) =
    checkNotNull (nameof set) set
    List.ofSeq set

[<CompiledName "ToSeq">]
let toSeq (set : SortedSet<'T>) =
    checkNotNull (nameof set) set
    set :> seq<'T>
