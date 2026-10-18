[<RequireQualifiedAccess>]
module FSharp.Collections.Immutable.HashSet

open System
open System.Collections.Generic
open System.Collections.Immutable
open System.Linq
// Opened after System.Collections.Generic, so that HashSet names the abbreviation of this library, not the mutable set
open FSharp.Collections.Immutable

// The functions call the members of ImmutableHashSet: Contains, TryGetValue, Add, Remove, Clear, the set algebra and the
// builders (ADR 0001, D5). The transformations that keep the element type collect their result in a builder with the
// comparer of the input; the ones that change it build a hash set with the default comparer of the new type (D5,
// Comparers). FSharp.Core sets cannot be null, so the null checks name the parameters of this module (D6).

/// The struct with the default comparer: the creation functions are its members, so that each has one implementation (D12).
let private withDefaultComparer<'T> () = CustomComparerHashSet<'T>(EqualityComparer<'T>.Default)

// ---- Creation ----

[<GeneralizableValue>]
[<CompiledName "Empty">]
let empty<'T> : HashSet<'T> = ImmutableHashSet<'T>.Empty

[<CompiledName "ForComparer">]
let forComparer (comparer : IEqualityComparer<'T>) = CustomComparerHashSet<'T> comparer

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
let build (action : HashSet<'T>.Builder -> unit) = (withDefaultComparer ()).build action

[<CompiledName "CreateBuilder">]
let builder () : HashSet<'T>.Builder = (withDefaultComparer<'T>()).builder()

[<CompiledName "OfBuilder">]
let ofBuilder (builder : HashSet<'T>.Builder) =
    checkNotNull (nameof builder) builder
    builder.ToImmutable ()

[<CompiledName "ToBuilder">]
let toBuilder (set : HashSet<'T>) =
    checkNotNull (nameof set) set
    set.ToBuilder ()

[<CompiledName "Update">]
let update (action : HashSet<'T>.Builder -> unit) (set : HashSet<'T>) =
    checkNotNull (nameof set) set
    let builder = set.ToBuilder ()
    action builder
    builder.ToImmutable ()

// ---- Properties ----

[<CompiledName "Comparer">]
let comparer (set : HashSet<'T>) =
    checkNotNull (nameof set) set
    set.KeyComparer

[<CompiledName "Count">]
let count (set : HashSet<'T>) =
    checkNotNull (nameof set) set
    set.Count

[<CompiledName "IsEmpty">]
let isEmpty (set : HashSet<'T>) =
    checkNotNull (nameof set) set
    set.IsEmpty

// ---- Access ----

[<CompiledName "TryGetValue">]
let tryGetValue (value : 'T) (set : HashSet<'T>) =
    checkNotNull (nameof set) set

    match set.TryGetValue value with
    | true, actual -> ValueSome actual
    | false, _ -> ValueNone

// ---- Search ----

[<CompiledName "Contains">]
let contains (element : 'T) (set : HashSet<'T>) =
    checkNotNull (nameof set) set
    set.Contains element

// exists and forall loop over the struct enumerator of the hash set, which allocates nothing. LINQ Any and All allocated
// 232 bytes per call, a delegate and the boxed enumerator, and Seq.exists and Seq.forall 144 bytes, the boxed enumerator
// (measured in Release; a measured faster path replaces a higher priority, D5). Dispose returns the pooled stack of the
// enumerator.

[<CompiledName "Exists">]
let exists (predicate : 'T -> bool) (set : HashSet<'T>) =
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
let forall (predicate : 'T -> bool) (set : HashSet<'T>) =
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
let isProperSubset (set1 : HashSet<'T>) (set2 : HashSet<'T>) =
    checkNotNull (nameof set1) set1
    checkNotNull (nameof set2) set2
    set1.IsProperSubsetOf set2

[<CompiledName "IsProperSuperset">]
let isProperSuperset (set1 : HashSet<'T>) (set2 : HashSet<'T>) =
    checkNotNull (nameof set1) set1
    checkNotNull (nameof set2) set2

    // ImmutableHashSet.IsProperSupersetOf counts every element of the other sequence, so elements that the comparer of
    // set1 finds equal count twice, against its documentation (ADR 0001, D14). The count is right when set2 compares with
    // the same comparer; otherwise a proper superset is a superset that is not a subset, and only IsSubsetOf allocates,
    // one helper set of set2.
    if obj.ReferenceEquals (set1.KeyComparer, set2.KeyComparer) then
        set1.IsProperSupersetOf set2
    else
        set1.IsSupersetOf set2 && not (set1.IsSubsetOf set2)

[<CompiledName "IsSubset">]
let isSubset (set1 : HashSet<'T>) (set2 : HashSet<'T>) =
    checkNotNull (nameof set1) set1
    checkNotNull (nameof set2) set2
    set1.IsSubsetOf set2

[<CompiledName "IsSuperset">]
let isSuperset (set1 : HashSet<'T>) (set2 : HashSet<'T>) =
    checkNotNull (nameof set1) set1
    checkNotNull (nameof set2) set2
    set1.IsSupersetOf set2

[<CompiledName "Overlaps">]
let overlaps (set1 : HashSet<'T>) (set2 : HashSet<'T>) =
    checkNotNull (nameof set1) set1
    checkNotNull (nameof set2) set2
    set1.Overlaps set2

[<CompiledName "SetEquals">]
let setEquals (set1 : HashSet<'T>) (set2 : HashSet<'T>) =
    checkNotNull (nameof set1) set1
    checkNotNull (nameof set2) set2
    set1.SetEquals set2

// ---- Transformation ----

[<CompiledName "Add">]
let add (value : 'T) (set : HashSet<'T>) =
    checkNotNull (nameof set) set
    set.Add value

[<CompiledName "AddRange">]
let addRange (elements : seq<'T>) (set : HashSet<'T>) =
    checkNotNullOrDefaultSeq (nameof elements) elements
    checkNotNull (nameof set) set
    let result = set.Union elements

    // Union returns the input itself when it adds nothing, except for an empty input and a hash set as the argument: it
    // then returns a hash set built from the argument, another instance even when the argument is empty (ADR 0001,
    // D14). Union only adds, so an unchanged count means that nothing was added.
    if result.Count = set.Count then set else result

[<CompiledName "Choose">]
let choose (chooser : 'T -> 'U voption) (set : HashSet<'T>) =
    checkNotNull (nameof set) set
    HashSets.choose chooser set

[<CompiledName "Clear">]
let clear (set : HashSet<'T>) =
    checkNotNull (nameof set) set
    set.Clear ()

[<CompiledName "Difference">]
let difference (set1 : HashSet<'T>) (set2 : HashSet<'T>) =
    checkNotNull (nameof set1) set1
    checkNotNull (nameof set2) set2
    set1.Except set2

[<CompiledName "Filter">]
let filter (predicate : 'T -> bool) (set : HashSet<'T>) =
    checkNotNull (nameof set) set
    let builder = ImmutableHashSet.CreateBuilder<'T> set.KeyComparer

    for item in set do
        if predicate item then
            builder.Add item |> ignore

    builder.ToImmutable ()

[<CompiledName "Intersect">]
let intersect (set1 : HashSet<'T>) (set2 : HashSet<'T>) =
    checkNotNull (nameof set1) set1
    checkNotNull (nameof set2) set2
    set1.Intersect set2

[<CompiledName "IntersectMany">]
let intersectMany (sets : seq<HashSet<'T>>) =
    // Set.intersectMany is Seq.reduce over Set.intersect, so its checks are those of Seq.reduce
    checkNotNullOrDefaultSeq (nameof sets) sets

    // The type of a set is annotated, because the sequence that fixes it comes last
    let checkSet (set : HashSet<'T>) = checkNotNull (nameof sets) set

    match Sets.reduce checkSet (fun set1 set2 -> set1.Intersect set2) sets with
    | ValueSome result -> result
    | ValueNone -> invalidArg (nameof sets) ErrorStrings.InputSequenceEmpty

[<CompiledName "Map">]
let map (mapping : 'T -> 'U) (set : HashSet<'T>) =
    checkNotNull (nameof set) set
    HashSets.map mapping set

[<CompiledName "Remove">]
let remove (value : 'T) (set : HashSet<'T>) =
    checkNotNull (nameof set) set
    set.Remove value

[<CompiledName "RemoveRange">]
let removeRange (elements : seq<'T>) (set : HashSet<'T>) =
    checkNotNullOrDefaultSeq (nameof elements) elements
    checkNotNull (nameof set) set
    set.Except elements

[<CompiledName "SymmetricDifference">]
let symmetricDifference (set1 : HashSet<'T>) (set2 : HashSet<'T>) =
    checkNotNull (nameof set1) set1
    checkNotNull (nameof set2) set2
    set1.SymmetricExcept set2

[<CompiledName "Union">]
let union (set1 : HashSet<'T>) (set2 : HashSet<'T>) =
    checkNotNull (nameof set1) set1
    checkNotNull (nameof set2) set2
    set1.Union set2

[<CompiledName "UnionMany">]
let unionMany (sets : seq<HashSet<'T>>) =
    checkNotNullOrDefaultSeq (nameof sets) sets

    let checkSet (set : HashSet<'T>) = checkNotNull (nameof sets) set

    // Set.unionMany folds from the empty set; starting from the first set instead keeps its comparer
    match Sets.reduce checkSet (fun set1 set2 -> set1.Union set2) sets with
    | ValueSome result -> result
    | ValueNone -> empty

// ---- Folding ----

[<CompiledName "Fold">]
let fold (folder : 'State -> 'T -> 'State) (state : 'State) (set : HashSet<'T>) =
    checkNotNull (nameof set) set
    Seq.fold folder state set

[<CompiledName "FoldBack">]
let foldBack (folder : 'T -> 'State -> 'State) (set : HashSet<'T>) (state : 'State) =
    checkNotNull (nameof set) set
    Seq.foldBack folder set state

[<CompiledName "Iterate">]
let iter (action : 'T -> unit) (set : HashSet<'T>) =
    checkNotNull (nameof set) set
    Seq.iter action set

// ---- Partitioning ----

[<CompiledName "Partition">]
let partition (predicate : 'T -> bool) (set : HashSet<'T>) =
    checkNotNull (nameof set) set
    let accepted = ImmutableHashSet.CreateBuilder<'T> set.KeyComparer
    let rejected = ImmutableHashSet.CreateBuilder<'T> set.KeyComparer

    for item in set do
        if predicate item then
            accepted.Add item |> ignore
        else
            rejected.Add item |> ignore

    struct (accepted.ToImmutable (), rejected.ToImmutable ())

[<CompiledName "PartitionWith">]
let partitionWith (partitioner : 'T -> Choice<'T1, 'T2>) (set : HashSet<'T>) =
    checkNotNull (nameof set) set
    HashSets.partitionWith partitioner set

// ---- Conversion ----

[<CompiledName "ToArray">]
let toArray (set : HashSet<'T>) =
    checkNotNull (nameof set) set
    Enumerable.ToArray set

[<CompiledName "ToList">]
let toList (set : HashSet<'T>) =
    checkNotNull (nameof set) set
    List.ofSeq set

[<CompiledName "ToSeq">]
let toSeq (set : HashSet<'T>) =
    checkNotNull (nameof set) set
    set :> seq<'T>
