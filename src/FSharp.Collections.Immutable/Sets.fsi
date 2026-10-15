namespace FSharp.Collections.Immutable

type ISet<'T> = System.Collections.Immutable.IImmutableSet<'T>

type HashSet<'T> = System.Collections.Immutable.ImmutableHashSet<'T>

/// <summary>Functional helpers for immutable hash sets.</summary>
[<RequireQualifiedAccess;
  CompiledName((nameof System.Collections.Immutable.ImmutableHashSet)
               + "Module")>]
module HashSet =

    /// <summary>Returns an empty hash set.</summary>
    val inline empty<'T> : HashSet<'T>

    /// <summary>Creates a hash set from a sequence.</summary>
    val inline ofSeq : source : seq<'T> -> HashSet<'T>

    /// <summary>Returns a sequence view of a hash set.</summary>
    val inline toSeq : set : HashSet<'T> -> seq<'T>

type SortedSet<'T> = System.Collections.Immutable.ImmutableSortedSet<'T>

/// <summary>Functional helpers for immutable sorted sets.</summary>
[<RequireQualifiedAccess;
  CompiledName((nameof System.Collections.Immutable.ImmutableSortedSet)
               + "Module")>]
module SortedSet =

    /// <summary>Returns an empty sorted set.</summary>
    val inline empty<'T> : SortedSet<'T>

    /// <summary>Creates a sorted set from a sequence.</summary>
    val inline ofSeq : source : seq<'T> -> SortedSet<'T>

    /// <summary>Returns a sequence view of a sorted set.</summary>
    val inline toSeq : set : SortedSet<'T> -> seq<'T>
