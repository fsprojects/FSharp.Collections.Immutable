namespace FSharp.Collections.Immutable

/// <summary>Functional helpers for <see cref="T:System.Collections.Immutable.ImmutableList`1" /> values.</summary>
[<RequireQualifiedAccess; CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module ImmutableList =

    /// <summary>Returns an empty immutable list.</summary>
    val inline empty<'T> : System.Collections.Immutable.ImmutableList<'T>

    /// <summary>Returns a sequence view of an immutable list.</summary>
    val inline toSeq : list : System.Collections.Immutable.ImmutableList<'T> -> seq<'T>

    /// <summary>Returns the number of items in the immutable list.</summary>
    val length : list : System.Collections.Immutable.IImmutableList<'T> -> int

    /// <summary>Returns the first item in the immutable list.</summary>
    val head : list : System.Collections.Immutable.IImmutableList<'T> -> 'T
