namespace FSharp.Collections.Immutable.Tests

open FSharp.Collections.Immutable
open Microsoft.VisualStudio.TestTools.UnitTesting

/// The sequence-like contract for `Queue`.
[<TestClass; QueueTestCategory>]
type QueueSeqLikeTests () =
    inherit SeqLikeContract<Queue<int>> (QueueOps.seqLike)

/// The creation contract for `Queue`.
[<TestClass; QueueTestCategory>]
type QueueCreationTests () =
    inherit SeqCreationContract<Queue<int>> (QueueOps.creation)

/// The stable sorting contract for `Queue`.
[<TestClass; QueueTestCategory>]
type QueueStableSortTests () =
    inherit StableSortContract<Queue<struct (int * char)>> (QueueOps.stableSort)

/// The sequence-like contract for `IQueue`.
[<TestClass; IQueueTestCategory>]
type IQueueSeqLikeTests () =
    inherit SeqLikeContract<IQueue<int>> (IQueueOps.seqLike)

/// The stable sorting contract for `IQueue`.
[<TestClass; IQueueTestCategory>]
type IQueueStableSortTests () =
    inherit StableSortContract<IQueue<struct (int * char)>> (IQueueOps.stableSort)
