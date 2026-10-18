namespace FSharp.Collections.Immutable.Tests

open System.Collections.Immutable
open FSharp.Collections.Immutable
open Microsoft.VisualStudio.TestTools.UnitTesting

/// The sequence-like contract for `Stack`.
[<TestClass; StackTestCategory>]
type StackSeqLikeTests () =
    inherit SeqLikeContract<Stack<int>> (StackOps.seqLike)

/// The creation contract for `Stack`.
[<TestClass; StackTestCategory>]
type StackCreationTests () =
    inherit SeqCreationContract<Stack<int>> (StackOps.creation)

/// The stable sorting contract for `Stack`.
[<TestClass; StackTestCategory>]
type StackStableSortTests () =
    inherit StableSortContract<Stack<struct (int * char)>> (StackOps.stableSort)

/// The sequence-like contract for `IStack`.
[<TestClass; IStackTestCategory>]
type IStackSeqLikeTests () =
    inherit SeqLikeContract<IImmutableStack<int>> (IStackOps.seqLike)

/// The stable sorting contract for `IStack`.
[<TestClass; IStackTestCategory>]
type IStackStableSortTests () =
    inherit StableSortContract<IImmutableStack<struct (int * char)>> (IStackOps.stableSort)
