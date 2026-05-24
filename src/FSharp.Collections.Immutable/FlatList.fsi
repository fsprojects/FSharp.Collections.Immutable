namespace FSharp.Collections.Immutable

type FlatList<'T> = System.Collections.Immutable.ImmutableArray<'T>

/// <summary>Functional helpers for <see cref="T:System.Collections.Immutable.ImmutableArray`1" /> values.</summary>
[<RequireQualifiedAccess;
  CompiledName((nameof System.Collections.Immutable.ImmutableArray)
               + "Module")>]
module FlatList =

    /// <summary>Returns an empty flat list.</summary>
    val inline empty<'T> : FlatList<'T>

    /// <summary>Creates a flat list from a sequence.</summary>
    val inline ofSeq : source : seq<'T> -> FlatList<'T>

    /// <summary>Returns a sequence view of a flat list.</summary>
    val inline toSeq : flatList : FlatList<'T> -> seq<'T>
