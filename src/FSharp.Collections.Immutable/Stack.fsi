namespace FSharp.Collections.Immutable

type IStack<'T> = System.Collections.Immutable.IImmutableStack<'T>

type Stack<'T> = System.Collections.Immutable.ImmutableStack<'T>

/// <summary>Functional helpers for immutable stacks.</summary>
[<RequireQualifiedAccess;
  CompiledName((nameof System.Collections.Immutable.ImmutableStack)
               + "Module")>]
module Stack =

    /// <summary>Returns an empty stack.</summary>
    val inline empty<'T> : Stack<'T>

    /// <summary>Creates a stack from a sequence.</summary>
    val inline ofSeq : source : seq<'T> -> Stack<'T>

    /// <summary>Returns a sequence view of a stack.</summary>
    val inline toSeq : stack : IStack<'T> -> seq<'T>
