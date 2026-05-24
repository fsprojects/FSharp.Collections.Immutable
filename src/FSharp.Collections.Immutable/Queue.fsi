namespace FSharp.Collections.Immutable

type IQueue<'T> = System.Collections.Immutable.IImmutableQueue<'T>

type Queue<'T> = System.Collections.Immutable.ImmutableQueue<'T>

/// <summary>Functional helpers for immutable queues.</summary>
[<RequireQualifiedAccess;
  CompiledName((nameof System.Collections.Immutable.ImmutableQueue)
               + "Module")>]
module Queue =

    /// <summary>Returns an empty queue.</summary>
    val inline empty<'T> : Queue<'T>

    /// <summary>Creates a queue from a sequence.</summary>
    val inline ofSeq : source : seq<'T> -> Queue<'T>

    /// <summary>Returns a sequence view of a queue.</summary>
    val inline toSeq : queue : Queue<'T> -> seq<'T>
