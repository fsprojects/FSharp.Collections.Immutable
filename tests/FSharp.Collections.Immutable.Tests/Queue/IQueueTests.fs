namespace FSharp.Collections.Immutable.Tests

open System
open System.Collections
open System.Collections.Generic
open System.Collections.Immutable
open FSharp.Collections.Immutable
open Microsoft.VisualStudio.TestTools.UnitTesting

/// <summary>
/// An implementation of <see cref="T:System.Collections.Immutable.IImmutableQueue`1"/> that is not an immutable queue, to
/// tell whether a function keeps the implementation of its input.
/// </summary>
/// <remarks>
/// Every member that returns a queue returns a tagged queue.
/// </remarks>
type TaggedQueue<'T> (items : ImmutableQueue<'T>) =

    static let wrap (items : ImmutableQueue<'T>) = TaggedQueue items :> IImmutableQueue<'T>

    interface IImmutableQueue<'T> with
        member _.Clear () = wrap (items.Clear ())
        member _.Dequeue () = wrap (items.Dequeue ())
        member _.Enqueue value = wrap (items.Enqueue value)
        member _.IsEmpty = items.IsEmpty
        member _.Peek () = items.Peek ()

    interface IEnumerable<'T> with
        member _.GetEnumerator () = (items :> IEnumerable<'T>).GetEnumerator()

    interface IEnumerable with
        member _.GetEnumerator () = (items :> IEnumerable).GetEnumerator()

/// <summary>
/// An implementation of <see cref="T:System.Collections.Immutable.IImmutableQueue`1"/> that is not an immutable queue and
/// counts what its enumerators do, to tell how far a function reads a queue of another implementation and whether it
/// disposes the enumerator.
/// </summary>
/// <remarks>
/// The members that return a queue return an immutable queue.
/// </remarks>
type CountingQueue<'T> (items : ImmutableQueue<'T>) =

    let mutable enumerators = 0
    let mutable disposals = 0
    let mutable elementsRead = 0

    /// The number of enumerators that the queue gave out.
    member _.Enumerators = enumerators

    /// The number of calls of Dispose on the enumerators of the queue.
    member _.Disposals = disposals

    /// The number of elements that the enumerators of the queue read.
    member _.ElementsRead = elementsRead

    interface IImmutableQueue<'T> with
        member _.Clear () = items.Clear () :> _
        member _.Dequeue () = items.Dequeue () :> _
        member _.Enqueue value = items.Enqueue value :> _
        member _.IsEmpty = items.IsEmpty
        member _.Peek () = items.Peek ()

    interface IEnumerable<'T> with
        member _.GetEnumerator () =
            enumerators <- enumerators + 1
            let inner = (items :> IEnumerable<'T>).GetEnumerator()

            {
                new IEnumerator<'T> with
                    member _.Current = inner.Current
                interface IEnumerator with
                    member _.Current = box inner.Current

                    member _.MoveNext () =
                        let moved = inner.MoveNext ()

                        if moved then
                            elementsRead <- elementsRead + 1

                        moved

                    member _.Reset () = inner.Reset ()
                interface IDisposable with
                    member _.Dispose () =
                        disposals <- disposals + 1
                        inner.Dispose ()
            }

    interface IEnumerable with
        member this.GetEnumerator () = (this :> IEnumerable<'T>).GetEnumerator() :> IEnumerator

/// The behaviour of `IQueue` that the shared contracts do not cover: the implementation of the results, null inputs, the
/// patterns, the reading of queues of other implementations, the order of strings and the compiled names.
[<TestClass; IQueueTestCategory; PatternMatchingTestCategory>]
type IQueueTests () =

    static let nullQueue = Unchecked.defaultof<IQueue<int>>

    static let several () : IQueue<int> = Queue.ofArray Inputs.Several

    static let tagged () : IQueue<int> = TaggedQueue (ImmutableQueue.CreateRange Inputs.Several)

    // Each call draws from a new generator with the same seed, so that both implementations get the same choices
    static let random () = Random 42

    static let randomizer () =
        let random = Random 42
        fun () -> random.NextDouble ()

    static let first (struct (first : IQueue<int>, _ : IQueue<int>)) = first

    static let second (struct (_ : IQueue<int>, second : IQueue<int>)) = second

    /// The functions whose result has the element type of their input, by name, as functions of the input. The rows of
    /// the test are the names: a function has no readable name in a row.
    static let sameElementType : Map<string, IQueue<int> -> IQueue<int>> =
        Map.ofArray [|
            nameof IQueue.tail, IQueue.tail
            nameof IQueue.tryTail, (fun queue -> (IQueue.tryTail queue).Value)
            (nameof IQueue.uncons,
             fun queue ->
                 let struct (_, tail) = IQueue.uncons queue
                 tail)
            nameof IQueue.append, (fun queue -> IQueue.append queue (Queue.ofArray [| 1; 2 |]))
            ($"%s{nameof IQueue.append} to an empty queue",
             fun queue -> IQueue.append (IQueue.clear queue) (Queue.ofArray [| 1; 2 |]))
            nameof IQueue.enqueue, IQueue.enqueue 99
            nameof IQueue.enqueueMany, IQueue.enqueueMany [| 98; 99 |]
            nameof IQueue.filter, IQueue.filter (fun item -> item > 0)
            nameof IQueue.permute, IQueue.permute (fun index -> (index + 3) % 8)
            nameof IQueue.rev, IQueue.rev
            nameof IQueue.where, IQueue.where (fun item -> item > 0)
            nameof IQueue.distinct, IQueue.distinct
            nameof IQueue.distinctBy, IQueue.distinctBy abs
            nameof IQueue.except, IQueue.except [| 5 |]
            $"%s{nameof IQueue.partition} (accepted)", IQueue.partition (fun item -> item > 0) >> first
            $"%s{nameof IQueue.partition} (rejected)", IQueue.partition (fun item -> item > 0) >> second
            nameof IQueue.skip, IQueue.skip 3
            nameof IQueue.skipLast, IQueue.skipLast 3
            nameof IQueue.skipWhile, IQueue.skipWhile (fun item -> item > 0)
            $"%s{nameof IQueue.splitAt} (first part)", IQueue.splitAt 3 >> first
            $"%s{nameof IQueue.splitAt} (second part)", IQueue.splitAt 3 >> second
            nameof IQueue.take, IQueue.take 3
            nameof IQueue.takeLast, IQueue.takeLast 3
            nameof IQueue.takeWhile, IQueue.takeWhile (fun item -> item > 0)
            nameof IQueue.truncate, IQueue.truncate 3
            nameof IQueue.sort, IQueue.sort
            nameof IQueue.sortBy, IQueue.sortBy abs
            nameof IQueue.sortByDescending, IQueue.sortByDescending abs
            nameof IQueue.sortDescending, IQueue.sortDescending
            nameof IQueue.sortWith, IQueue.sortWith compare
            nameof IQueue.sortWithComparer, IQueue.sortWithComparer Comparer<int>.Default
            nameof IQueue.randomChoicesWith, (fun queue -> IQueue.randomChoicesWith (random ()) 3 queue)
            nameof IQueue.randomChoicesBy, (fun queue -> IQueue.randomChoicesBy (randomizer ()) 3 queue)
            nameof IQueue.randomSampleWith, (fun queue -> IQueue.randomSampleWith (random ()) 3 queue)
            nameof IQueue.randomSampleBy, (fun queue -> IQueue.randomSampleBy (randomizer ()) 3 queue)
            nameof IQueue.randomShuffleWith, (fun queue -> IQueue.randomShuffleWith (random ()) queue)
            nameof IQueue.randomShuffleBy, (fun queue -> IQueue.randomShuffleBy (randomizer ()) queue)
            nameof IQueue.clear, IQueue.clear
        |]

    /// The functions that take a queue, by name, with the parameter that names the null queue and the function on a null
    /// queue.
    static let nullQueueRejections =
        let pairs = Unchecked.defaultof<IQueue<struct (int * int)>>
        let triples = Unchecked.defaultof<IQueue<struct (int * int * int)>>
        let floats = Unchecked.defaultof<IQueue<float>>

        [|
            struct (nameof IQueue.isEmpty, "queue", (fun () -> IQueue.isEmpty nullQueue |> ignore))
            struct (nameof IQueue.length, "queue", (fun () -> IQueue.length nullQueue |> ignore))
            struct (nameof IQueue.exactlyOne, "queue", (fun () -> IQueue.exactlyOne nullQueue |> ignore))
            struct (nameof IQueue.head, "queue", (fun () -> IQueue.head nullQueue |> ignore))
            struct (nameof IQueue.last, "queue", (fun () -> IQueue.last nullQueue |> ignore))
            struct (nameof IQueue.tail, "queue", (fun () -> IQueue.tail nullQueue |> ignore))
            struct (nameof IQueue.tryExactlyOne, "queue", (fun () -> IQueue.tryExactlyOne nullQueue |> ignore))
            struct (nameof IQueue.tryHead, "queue", (fun () -> IQueue.tryHead nullQueue |> ignore))
            struct (nameof IQueue.tryLast, "queue", (fun () -> IQueue.tryLast nullQueue |> ignore))
            struct (nameof IQueue.tryTail, "queue", (fun () -> IQueue.tryTail nullQueue |> ignore))
            struct (nameof IQueue.tryUncons, "queue", (fun () -> IQueue.tryUncons nullQueue |> ignore))
            struct (nameof IQueue.uncons, "queue", (fun () -> IQueue.uncons nullQueue |> ignore))
            struct (nameof IQueue.(|Cons|_|), "queue", (fun () -> IQueue.(|Cons|_|) nullQueue |> ignore))
            struct (nameof IQueue.(|Nil|_|), "queue", (fun () -> IQueue.(|Nil|_|) nullQueue |> ignore))
            struct (nameof IQueue.contains, "source", (fun () -> IQueue.contains 1 nullQueue |> ignore))
            struct (nameof IQueue.exists, "queue", (fun () -> IQueue.exists ((=) 1) nullQueue |> ignore))
            struct (nameof IQueue.exists2, "queue1", (fun () -> IQueue.exists2 (=) nullQueue (several ()) |> ignore))
            struct (nameof IQueue.find, "queue", (fun () -> IQueue.find ((=) 1) nullQueue |> ignore))
            struct (nameof IQueue.findBack, "queue", (fun () -> IQueue.findBack ((=) 1) nullQueue |> ignore))
            struct (nameof IQueue.findIndex, "queue", (fun () -> IQueue.findIndex ((=) 1) nullQueue |> ignore))
            struct (nameof IQueue.findIndexBack, "queue", (fun () -> IQueue.findIndexBack ((=) 1) nullQueue |> ignore))
            struct (nameof IQueue.forall, "queue", (fun () -> IQueue.forall ((=) 1) nullQueue |> ignore))
            struct (nameof IQueue.forall2, "queue2", (fun () -> IQueue.forall2 (=) (several ()) nullQueue |> ignore))
            struct (nameof IQueue.pick, "queue", (fun () -> IQueue.pick ValueSome nullQueue |> ignore))
            struct (nameof IQueue.tryFind, "queue", (fun () -> IQueue.tryFind ((=) 1) nullQueue |> ignore))
            struct (nameof IQueue.tryFindBack, "queue", (fun () -> IQueue.tryFindBack ((=) 1) nullQueue |> ignore))
            struct (nameof IQueue.tryFindIndex, "queue", (fun () -> IQueue.tryFindIndex ((=) 1) nullQueue |> ignore))
            struct (nameof IQueue.tryFindIndexBack, "queue", (fun () -> IQueue.tryFindIndexBack ((=) 1) nullQueue |> ignore))
            struct (nameof IQueue.tryPick, "queue", (fun () -> IQueue.tryPick ValueSome nullQueue |> ignore))
            struct (nameof IQueue.allPairs, "queue2", (fun () -> IQueue.allPairs (several ()) nullQueue |> ignore))
            struct (nameof IQueue.append, "queue1", (fun () -> IQueue.append nullQueue (several ()) |> ignore))
            struct (nameof IQueue.choose, "queue", (fun () -> IQueue.choose ValueSome nullQueue |> ignore))
            struct (nameof IQueue.chunkBySize, "queue", (fun () -> IQueue.chunkBySize 2 nullQueue |> ignore))
            struct (nameof IQueue.collect, "queue", (fun () -> IQueue.collect (fun _ -> (several ())) nullQueue |> ignore))
            struct (nameof IQueue.concat, "queues", (fun () -> IQueue.concat [| (several ()); nullQueue |] |> ignore))
            struct (nameof IQueue.enqueue, "queue", (fun () -> IQueue.enqueue 1 nullQueue |> ignore))
            struct (nameof IQueue.enqueueMany, "queue", (fun () -> IQueue.enqueueMany [| 1 |] nullQueue |> ignore))
            struct (nameof IQueue.filter, "queue", (fun () -> IQueue.filter ((=) 1) nullQueue |> ignore))
            struct (nameof IQueue.indexed, "queue", (fun () -> IQueue.indexed nullQueue |> ignore))
            struct (nameof IQueue.map, "queue", (fun () -> IQueue.map id nullQueue |> ignore))
            struct (nameof IQueue.map2, "queue1", (fun () -> IQueue.map2 (+) nullQueue (several ()) |> ignore))
            struct (nameof IQueue.map3,
                    "queue2",
                    (fun () ->
                        IQueue.map3 (fun a b c -> a + b + c) (several ()) nullQueue (several ())
                        |> ignore
                    ))
            struct (nameof IQueue.mapi, "queue", (fun () -> IQueue.mapi (+) nullQueue |> ignore))
            struct (nameof IQueue.mapi2,
                    "queue2",
                    (fun () ->
                        IQueue.mapi2 (fun index a b -> index + a + b) (several ()) nullQueue
                        |> ignore
                    ))
            struct (nameof IQueue.pairwise, "queue", (fun () -> IQueue.pairwise nullQueue |> ignore))
            struct (nameof IQueue.permute, "queue", (fun () -> IQueue.permute id nullQueue |> ignore))
            struct (nameof IQueue.rev, "queue", (fun () -> IQueue.rev nullQueue |> ignore))
            struct (nameof IQueue.transpose, "queues", (fun () -> IQueue.transpose [| nullQueue |] |> ignore))
            struct (nameof IQueue.unzip, "queue", (fun () -> IQueue.unzip pairs |> ignore))
            struct (nameof IQueue.unzip3, "queue", (fun () -> IQueue.unzip3 triples |> ignore))
            struct (nameof IQueue.where, "queue", (fun () -> IQueue.where ((=) 1) nullQueue |> ignore))
            struct (nameof IQueue.windowed, "queue", (fun () -> IQueue.windowed 2 nullQueue |> ignore))
            struct (nameof IQueue.zip, "queue2", (fun () -> IQueue.zip (several ()) nullQueue |> ignore))
            struct (nameof IQueue.zip3, "queue3", (fun () -> IQueue.zip3 (several ()) (several ()) nullQueue |> ignore))
            struct (nameof IQueue.average, "queue", (fun () -> IQueue.average floats |> ignore))
            struct (nameof IQueue.averageBy, "queue", (fun () -> IQueue.averageBy float nullQueue |> ignore))
            struct (nameof IQueue.compareWith, "queue1", (fun () -> IQueue.compareWith compare nullQueue (several ()) |> ignore))
            struct (nameof IQueue.fold, "queue", (fun () -> IQueue.fold (+) 0 nullQueue |> ignore))
            struct (nameof IQueue.fold2,
                    "queue1",
                    (fun () ->
                        IQueue.fold2 (fun state a b -> state + a + b) 0 nullQueue (several ())
                        |> ignore
                    ))
            struct (nameof IQueue.foldBack, "queue", (fun () -> IQueue.foldBack (+) nullQueue 0 |> ignore))
            struct (nameof IQueue.foldBack2,
                    "queue2",
                    (fun () ->
                        IQueue.foldBack2 (fun a b state -> state + a + b) (several ()) nullQueue 0
                        |> ignore
                    ))
            struct (nameof IQueue.iter, "queue", (fun () -> IQueue.iter ignore nullQueue))
            struct (nameof IQueue.iter2, "queue1", (fun () -> IQueue.iter2 (fun _ _ -> ()) nullQueue (several ())))
            struct (nameof IQueue.iteri, "queue", (fun () -> IQueue.iteri (fun _ _ -> ()) nullQueue))
            struct (nameof IQueue.iteri2, "queue2", (fun () -> IQueue.iteri2 (fun _ _ _ -> ()) (several ()) nullQueue))
            struct (nameof IQueue.mapFold,
                    "queue",
                    (fun () ->
                        IQueue.mapFold (fun state item -> struct (item, state)) 0 nullQueue
                        |> ignore
                    ))
            struct (nameof IQueue.mapFoldBack,
                    "queue",
                    (fun () ->
                        IQueue.mapFoldBack (fun item state -> struct (item, state)) nullQueue 0
                        |> ignore
                    ))
            struct (nameof IQueue.max, "queue", (fun () -> IQueue.max nullQueue |> ignore))
            struct (nameof IQueue.maxBy, "queue", (fun () -> IQueue.maxBy id nullQueue |> ignore))
            struct (nameof IQueue.min, "queue", (fun () -> IQueue.min nullQueue |> ignore))
            struct (nameof IQueue.minBy, "queue", (fun () -> IQueue.minBy id nullQueue |> ignore))
            struct (nameof IQueue.reduce, "queue", (fun () -> IQueue.reduce (+) nullQueue |> ignore))
            struct (nameof IQueue.reduceBack, "queue", (fun () -> IQueue.reduceBack (+) nullQueue |> ignore))
            struct (nameof IQueue.scan, "queue", (fun () -> IQueue.scan (+) 0 nullQueue |> ignore))
            struct (nameof IQueue.scanBack, "queue", (fun () -> IQueue.scanBack (+) nullQueue 0 |> ignore))
            struct (nameof IQueue.sum, "queue", (fun () -> IQueue.sum nullQueue |> ignore))
            struct (nameof IQueue.sumBy, "queue", (fun () -> IQueue.sumBy id nullQueue |> ignore))
            struct (nameof IQueue.countBy, "queue", (fun () -> IQueue.countBy id nullQueue |> ignore))
            struct (nameof IQueue.distinct, "queue", (fun () -> IQueue.distinct nullQueue |> ignore))
            struct (nameof IQueue.distinctBy, "queue", (fun () -> IQueue.distinctBy id nullQueue |> ignore))
            struct (nameof IQueue.except, "queue", (fun () -> IQueue.except [| 1 |] nullQueue |> ignore))
            struct (nameof IQueue.groupBy, "queue", (fun () -> IQueue.groupBy id nullQueue |> ignore))
            struct (nameof IQueue.partition, "queue", (fun () -> IQueue.partition ((=) 1) nullQueue |> ignore))
            struct (nameof IQueue.partitionWith, "queue", (fun () -> IQueue.partitionWith Choice1Of2 nullQueue |> ignore))
            struct (nameof IQueue.skip, "queue", (fun () -> IQueue.skip 0 nullQueue |> ignore))
            struct (nameof IQueue.skipLast, "queue", (fun () -> IQueue.skipLast 0 nullQueue |> ignore))
            struct (nameof IQueue.skipWhile, "queue", (fun () -> IQueue.skipWhile ((=) 1) nullQueue |> ignore))
            struct (nameof IQueue.splitAt, "queue", (fun () -> IQueue.splitAt 0 nullQueue |> ignore))
            struct (nameof IQueue.splitInto, "queue", (fun () -> IQueue.splitInto 2 nullQueue |> ignore))
            struct (nameof IQueue.take, "queue", (fun () -> IQueue.take 0 nullQueue |> ignore))
            struct (nameof IQueue.takeLast, "queue", (fun () -> IQueue.takeLast 0 nullQueue |> ignore))
            struct (nameof IQueue.takeWhile, "queue", (fun () -> IQueue.takeWhile ((=) 1) nullQueue |> ignore))
            struct (nameof IQueue.truncate, "queue", (fun () -> IQueue.truncate 1 nullQueue |> ignore))
            struct (nameof IQueue.sort, "queue", (fun () -> IQueue.sort nullQueue |> ignore))
            struct (nameof IQueue.sortBy, "queue", (fun () -> IQueue.sortBy id nullQueue |> ignore))
            struct (nameof IQueue.sortByDescending, "queue", (fun () -> IQueue.sortByDescending id nullQueue |> ignore))
            struct (nameof IQueue.sortDescending, "queue", (fun () -> IQueue.sortDescending nullQueue |> ignore))
            struct (nameof IQueue.sortWith, "queue", (fun () -> IQueue.sortWith compare nullQueue |> ignore))
            struct (nameof IQueue.sortWithComparer,
                    "queue",
                    (fun () ->
                        IQueue.sortWithComparer Comparer<int>.Default nullQueue
                        |> ignore
                    ))
            struct (nameof IQueue.randomChoice, "source", (fun () -> IQueue.randomChoice nullQueue |> ignore))
            struct (nameof IQueue.randomChoiceBy, "source", (fun () -> IQueue.randomChoiceBy (fun () -> 0.5) nullQueue |> ignore))
            struct (nameof IQueue.randomChoiceWith, "source", (fun () -> IQueue.randomChoiceWith (Random 1) nullQueue |> ignore))
            struct (nameof IQueue.randomChoices, "source", (fun () -> IQueue.randomChoices 1 nullQueue |> ignore))
            struct (nameof IQueue.randomChoicesBy,
                    "source",
                    (fun () -> IQueue.randomChoicesBy (fun () -> 0.5) 1 nullQueue |> ignore))
            struct (nameof IQueue.randomChoicesWith,
                    "source",
                    (fun () -> IQueue.randomChoicesWith (Random 1) 1 nullQueue |> ignore))
            struct (nameof IQueue.randomSample, "source", (fun () -> IQueue.randomSample 1 nullQueue |> ignore))
            struct (nameof IQueue.randomSampleBy,
                    "source",
                    (fun () -> IQueue.randomSampleBy (fun () -> 0.5) 1 nullQueue |> ignore))
            struct (nameof IQueue.randomSampleWith, "source", (fun () -> IQueue.randomSampleWith (Random 1) 1 nullQueue |> ignore))
            struct (nameof IQueue.randomShuffle, "source", (fun () -> IQueue.randomShuffle nullQueue |> ignore))
            struct (nameof IQueue.randomShuffleBy,
                    "source",
                    (fun () -> IQueue.randomShuffleBy (fun () -> 0.5) nullQueue |> ignore))
            struct (nameof IQueue.randomShuffleWith, "source", (fun () -> IQueue.randomShuffleWith (Random 1) nullQueue |> ignore))
            struct (nameof IQueue.toArray, "queue", (fun () -> IQueue.toArray nullQueue |> ignore))
            struct (nameof IQueue.toList, "queue", (fun () -> IQueue.toList nullQueue |> ignore))
            struct (nameof IQueue.toSeq, "queue", (fun () -> IQueue.toSeq nullQueue |> ignore))
            struct (nameof IQueue.clear, "queue", (fun () -> IQueue.clear nullQueue |> ignore))
        |]
        |> Array.map (fun struct (name, paramName, action) -> name, struct (paramName, action))
        |> Map.ofArray

    /// The functions that take a sequence, by name and input, with the parameter that names the sequence and the function on
    /// the input. The inputs are a default flat list, which converts to a sequence without a check and fails with
    /// InvalidOperationException when it is enumerated, a default flat list of immutable queues as a sequence of the
    /// interface, and a default flat list of strings as a sequence of objects: seq is covariant, so a type test for the flat
    /// list of the element type of the function does not see the last two. The rows of the test are the names.
    static let defaultSequenceRejections =
        let defaultList = Inputs.DefaultFlatListAsSeq<int>()
        let defaultQueues = Inputs.DefaultFlatListAsSeq<IQueue<int>>()

        let widenedQueues =
            match box (Unchecked.defaultof<FlatList<Queue<int>>>) with
            | :? seq<IQueue<int>> as queues -> queues
            | _ -> invalidOp "A flat list of immutable queues must be a sequence of the interface"

        let widened = FlatListOracles.defaultStringsAsObjects

        [|
            struct (nameof IQueue.concat, "default flat list", "queues", (fun () -> IQueue.concat defaultQueues |> ignore))
            struct (nameof IQueue.concat,
                    "default flat list of immutable queues",
                    "queues",
                    (fun () -> IQueue.concat widenedQueues |> ignore))
            struct (nameof IQueue.transpose, "default flat list", "queues", (fun () -> IQueue.transpose defaultQueues |> ignore))
            struct (nameof IQueue.transpose,
                    "default flat list of immutable queues",
                    "queues",
                    (fun () -> IQueue.transpose widenedQueues |> ignore))
            struct (nameof IQueue.enqueueMany,
                    "default flat list",
                    "values",
                    (fun () -> IQueue.enqueueMany defaultList (several ()) |> ignore))
            struct (nameof IQueue.enqueueMany,
                    "default flat list of strings as objects",
                    "values",
                    (fun () ->
                        IQueue.enqueueMany widened (Queue.empty<obj> :> IQueue<obj>)
                        |> ignore
                    ))
            struct (nameof IQueue.except,
                    "default flat list",
                    "itemsToExclude",
                    (fun () -> IQueue.except defaultList (several ()) |> ignore))
            struct (nameof IQueue.except,
                    "default flat list and an empty queue",
                    "itemsToExclude",
                    (fun () ->
                        IQueue.except defaultList (Queue.empty<int> :> IQueue<int>)
                        |> ignore
                    ))
            struct (nameof IQueue.except,
                    "default flat list of strings as objects",
                    "itemsToExclude",
                    (fun () ->
                        IQueue.except widened (Queue.empty<obj> :> IQueue<obj>)
                        |> ignore
                    ))
        |]
        |> Array.map (fun struct (name, input, paramName, action) -> $"%s{name} of a %s{input}", struct (paramName, action))
        |> Map.ofArray

    /// The functions that keep every element of a queue for some arguments, by name, on such arguments. The names tell
    /// apart the two parts of splitAt.
    static let wholeQueues : Map<string, IQueue<int> -> IQueue<int>> =
        Map.ofArray [|
            nameof IQueue.take, IQueue.take 8
            nameof IQueue.takeLast, IQueue.takeLast 8
            nameof IQueue.takeWhile, IQueue.takeWhile (fun _ -> true)
            nameof IQueue.truncate, IQueue.truncate 100
            nameof IQueue.skip, IQueue.skip 0
            nameof IQueue.skipLast, IQueue.skipLast 0
            nameof IQueue.skipWhile, IQueue.skipWhile (fun _ -> false)
            ($"%s{nameof IQueue.splitAt} (first part)",
             fun queue ->
                 let struct (whole, _) = IQueue.splitAt 8 queue
                 whole)
            ($"%s{nameof IQueue.splitAt} (second part)",
             fun queue ->
                 let struct (_, rest) = IQueue.splitAt 0 queue
                 rest)
            nameof IQueue.append, (fun queue -> IQueue.append queue (IQueue.clear queue))
        |]

    /// The sorts that compare the elements or their keys, by name, with the Array function that sorts in the same order.
    static let stringSorts : Map<string, struct ((string array -> string array) * (IQueue<string> -> IQueue<string>))> =
        Map.ofArray [|
            nameof IQueue.sort, struct (Array.sort, IQueue.sort)
            nameof IQueue.sortBy, struct (Array.sortBy id, IQueue.sortBy id)
            nameof IQueue.sortByDescending, struct (Array.sortByDescending id, IQueue.sortByDescending id)
            nameof IQueue.sortDescending, struct (Array.sortDescending, IQueue.sortDescending)
        |]

    /// The names of the functions whose result has the element type of their input, as the rows of their test.
    static member SameElementType = sameElementType.Keys

    /// The names of the functions that take a queue, as the rows of their test.
    static member NullQueueRejections = nullQueueRejections.Keys

    /// The names of the functions and the default flat lists that they reject as sequences, as the rows of their test.
    static member DefaultSequenceRejections = defaultSequenceRejections.Keys

    /// The names of the cases that keep every element of a queue, as the rows of their test.
    static member WholeQueues = wholeQueues.Keys

    /// The names of the sorts that compare the elements or their keys, as the rows of the test of strings.
    static member StringSorts = stringSorts.Keys

    [<TestMethod; DynamicData(nameof IQueueTests.SameElementType)>]
    member _.``functions with the element type of their input keep its implementation`` (name : string) =
        let apply = sameElementType[name]
        let result = apply (tagged ())
        Assert.IsInstanceOfType<TaggedQueue<int>>(result, $"IQueue.%s{name} must keep the implementation of its input")
        |> ignore

        CollectionAssert.AreEqual (
            IQueue.toArray (apply (several ())),
            IQueue.toArray result,
            $"IQueue.%s{name} must return the same elements for every implementation"
        )

    [<TestMethod>]
    member _.``functions that combine queues or change the element type return an immutable queue`` () =
        let queue = tagged ()

        CollectionAssert.AreEqual (
            Array.map string Inputs.Several,
            Queue.toArray (IQueue.map string queue),
            "IQueue.map must map the elements of any implementation into an immutable queue"
        )

        let concatenated = IQueue.concat [| queue; several () |]

        CollectionAssert.AreEqual (
            Array.append Inputs.Several Inputs.Several,
            Queue.toArray concatenated,
            "IQueue.concat must concatenate queues of different implementations into an immutable queue"
        )

    [<TestMethod; DynamicData(nameof IQueueTests.NullQueueRejections)>]
    member _.``functions reject a null queue`` (name : string) =
        let struct (paramName, action) = nullQueueRejections[name]

        let ex =
            Assert.ThrowsExactly<ArgumentNullException>(Action action, $"IQueue.%s{name} must reject a null queue")

        Assert.AreEqual<string | null>(paramName, ex.ParamName, $"IQueue.%s{name} must name the parameter of the null queue")

    [<TestMethod>]
    member _.``enqueueMany rejects null values with the name of the parameter`` () =
        let ex =
            Assert.ThrowsExactly<ArgumentNullException>(
                Action (fun () ->
                    IQueue.enqueueMany (Unchecked.defaultof<seq<int>>) (several ())
                    |> ignore
                ),
                "IQueue.enqueueMany must reject null values"
            )

        Assert.AreEqual<string | null>("values", ex.ParamName, "IQueue.enqueueMany must name the values parameter")

    [<TestMethod; DynamicData(nameof IQueueTests.DefaultSequenceRejections)>]
    member _.``functions reject a default flat list given as a sequence with the names of their parameters`` (name : string) =
        let struct (paramName, action) = defaultSequenceRejections[name]
        FlatListAssert.RejectsNull ($"IQueue.%s{name}", paramName, action)

    [<TestMethod>]
    member _.``functions take a flat list of strings that is not default as a sequence of objects`` () =
        let widened = FlatListOracles.stringsAsObjects (FlatList.ofArray [| "a"; "b" |])
        let queue = Queue.ofSeq widened :> IQueue<obj>

        CollectionAssert.AreEqual (
            [| box "a"; box "b"; box "a"; box "b" |],
            IQueue.toArray (IQueue.enqueueMany widened queue),
            "IQueue.enqueueMany must take a flat list of strings as objects"
        )

        CollectionAssert.AreEqual (
            [| box "c" |],
            IQueue.toArray (IQueue.except widened (Queue.ofArray<obj> [| "a"; "b"; "c" |] :> IQueue<obj>)),
            "IQueue.except must take a flat list of strings as objects"
        )

    [<TestMethod>]
    member _.``enqueue and enqueueMany add the values after the last element`` () =
        CollectionAssert.AreEqual (
            Array.append Inputs.Several [| 99 |],
            IQueue.toArray (IQueue.enqueue 99 (tagged ())),
            "IQueue.enqueue must add the value at the end"
        )

        CollectionAssert.AreEqual (
            Array.append Inputs.Several [| 97; 98; 99 |],
            IQueue.toArray (IQueue.enqueueMany [| 97; 98; 99 |] (tagged ())),
            "IQueue.enqueueMany must add the values at the end, in their order"
        )

        let queue = tagged ()
        Assert.AreSame (queue, IQueue.enqueueMany [||] queue, "IQueue.enqueueMany without values must return the input")

    [<TestMethod>]
    member _.``uncons throws for an empty queue as List.head does`` () =
        ExceptionAssert.Same (
            QueueOps.paramName,
            (fun () -> List.head List.empty<int> |> ignore),
            (fun () -> IQueue.uncons (IQueue.clear (tagged ())) |> ignore),
            "IQueue.uncons of an empty queue must throw as List.head does"
        )

    [<TestMethod>]
    member _.``head, last and tail read the first and the last elements of any implementation`` () =
        let queue = tagged ()
        Assert.AreEqual<int>(Array.head Inputs.Several, IQueue.head queue, "IQueue.head must return the first element")
        Assert.AreEqual<int>(Array.last Inputs.Several, IQueue.last queue, "IQueue.last must return the last element")
        CollectionAssert.AreEqual (
            Array.tail Inputs.Several,
            IQueue.toArray (IQueue.tail queue),
            "IQueue.tail must drop the first element"
        )
        CollectionAssert.AreEqual (Inputs.Several, IQueue.toArray queue, "IQueue.tail must leave the input unchanged")

    [<TestMethod; DynamicData(nameof IQueueTests.WholeQueues)>]
    member _.``functions return the input itself when they keep every element`` (name : string) =
        let queue = tagged ()
        Assert.AreSame (queue, wholeQueues[name] queue, $"IQueue.%s{name} must return the input when it keeps every element")

    [<TestMethod>]
    member _.``Cons matches the first element and the other elements`` () =
        let rec sum (rest : IQueue<int>) =
            match rest with
            | IQueue.Cons (struct (head, tail)) -> head + sum tail
            | _ -> 0

        Assert.AreEqual<int>(Array.sum Inputs.Several, sum (tagged ()), "IQueue.Cons must match every element in turn")

        match tagged () with
        | IQueue.Cons (struct (_, tail)) ->
            Assert.IsInstanceOfType<TaggedQueue<int>>(tail, "IQueue.Cons must keep the implementation of the tail")
            |> ignore
        | _ -> Assert.Fail "IQueue.Cons must match a queue with elements"

    [<TestMethod>]
    member _.``Nil matches an empty queue only`` () =
        let describe (rest : IQueue<int>) =
            match rest with
            | IQueue.Nil -> "empty"
            | IQueue.Cons (struct (head, _)) -> $"starts with %d{head}"
            | _ -> "unreachable"

        Assert.AreEqual<string>("empty", describe (IQueue.clear (tagged ())), "IQueue.Nil must match an empty queue")
        Assert.AreEqual<string>("starts with 5", describe (several ()), "IQueue.Nil must not match a queue with elements")

    [<TestMethod>]
    member _.``contains compares in F# structural equality`` () =
        let arrays : IQueue<int array> = Queue.singleton [| 1; 2 |]

        Assert.IsTrue (
            IQueue.contains [| 1; 2 |] arrays,
            "IQueue.contains must compare arrays by their elements, as List.contains does"
        )

    [<TestMethod; DynamicData(nameof IQueueTests.StringSorts)>]
    member _.``sorts order strings in F# generic comparison, which is ordinal`` (name : string) =
        // A comparison under a culture puts "a" before "A" and both before "b"; F# generic comparison compares ordinally
        let elements = [| "b"; "A"; "a"; "B" |]
        let struct (oracle, sort) = stringSorts[name]

        CollectionAssert.AreEqual (
            oracle elements,
            IQueue.toArray (sort (Queue.ofArray elements)),
            $"IQueue.%s{name} must order strings in F# generic comparison, as the List function does"
        )

    [<TestMethod; DataRow 5; DataRow 8; DataRow 7; DataRow 99>]
    member _.``tryFind, tryFindIndex and tryPick read a queue of another implementation up to the first match`` (value : int) =
        let elements = Inputs.Several
        let queue = CountingQueue (ImmutableQueue.CreateRange elements)
        let isValue item = item = value
        let tenfoldIfValue item = if item = value then ValueSome (item * 10) else ValueNone

        Assert.AreEqual<int voption>(
            Array.tryFind isValue elements |> ValueOption.ofOption,
            IQueue.tryFind isValue queue,
            "IQueue.tryFind must find the first match in a queue of another implementation, as List.tryFind does"
        )

        Assert.AreEqual<int voption>(
            Array.tryFindIndex isValue elements |> ValueOption.ofOption,
            IQueue.tryFindIndex isValue queue,
            "IQueue.tryFindIndex must find the index of the first match in a queue of another implementation"
        )

        Assert.AreEqual<int voption>(
            Array.tryPick (tenfoldIfValue >> ValueOption.toOption) elements
            |> ValueOption.ofOption,
            IQueue.tryPick tenfoldIfValue queue,
            "IQueue.tryPick must return the first chosen value of a queue of another implementation"
        )

        // Each search reads the elements up to the first match, or every element when none matches
        let read =
            match Array.tryFindIndex isValue elements with
            | Some index -> index + 1
            | None -> elements.Length

        Assert.AreEqual<int>(3 * read, queue.ElementsRead, "The searches must stop reading at the first match")
        Assert.AreEqual<int>(3, queue.Enumerators, "Each search must take one enumerator of the queue")
        Assert.AreEqual<int>(3, queue.Disposals, "Each search must dispose the enumerator that it takes")

    [<TestMethod; DataRow 5; DataRow 8; DataRow 7>]
    member _.``find, findIndex and pick read a queue of another implementation up to the first match`` (value : int) =
        let elements = Inputs.Several
        let queue = CountingQueue (ImmutableQueue.CreateRange elements)
        let isValue item = item = value
        let tenfoldIfValue item = if item = value then ValueSome (item * 10) else ValueNone

        Assert.AreEqual<int>(
            Array.find isValue elements,
            IQueue.find isValue queue,
            "IQueue.find must find the first match in a queue of another implementation, as List.find does"
        )

        Assert.AreEqual<int>(
            Array.findIndex isValue elements,
            IQueue.findIndex isValue queue,
            "IQueue.findIndex must find the index of the first match in a queue of another implementation"
        )

        Assert.AreEqual<int>(
            Array.pick (tenfoldIfValue >> ValueOption.toOption) elements,
            IQueue.pick tenfoldIfValue queue,
            "IQueue.pick must return the first chosen value of a queue of another implementation"
        )

        Assert.AreEqual<int>(
            3 * (Array.findIndex isValue elements + 1),
            queue.ElementsRead,
            "The searches must stop reading at the first match"
        )

        Assert.AreEqual<int>(3, queue.Enumerators, "Each search must take one enumerator of the queue")
        Assert.AreEqual<int>(3, queue.Disposals, "Each search must dispose the enumerator that it takes")

    [<TestMethod; DataRow 0; DataRow 3; DataRow 8>]
    member _.``take reads a queue of another implementation up to the element after the ones that it takes`` (count : int) =
        let elements = Inputs.Several
        let queue = CountingQueue (ImmutableQueue.CreateRange elements)
        let taken = IQueue.take count queue
        // Read before toArray reads the result, which is the input itself when take keeps every element
        let struct (read, enumerators, disposals) =
            struct (queue.ElementsRead, queue.Enumerators, queue.Disposals)

        CollectionAssert.AreEqual (
            Array.take count elements,
            IQueue.toArray taken,
            "IQueue.take must return the first elements of a queue of another implementation"
        )

        Assert.AreEqual<int>(
            min (count + 1) elements.Length,
            read,
            "IQueue.take must read the elements that it takes and the next one, which tells whether there are more"
        )

        Assert.AreEqual<int>(1, enumerators, "IQueue.take must take one enumerator of the queue")
        Assert.AreEqual<int>(1, disposals, "IQueue.take must dispose the enumerator that it takes")

    [<TestMethod; DataRow 0; DataRow 3; DataRow 20>]
    member _.``truncate reads a queue of another implementation up to the element after the ones that it keeps`` (count : int) =
        let elements = Inputs.Several
        let queue = CountingQueue (ImmutableQueue.CreateRange elements)
        let truncated = IQueue.truncate count queue
        // Read before toArray reads the result, which is the input itself when truncate keeps every element
        let struct (read, enumerators, disposals) =
            struct (queue.ElementsRead, queue.Enumerators, queue.Disposals)

        CollectionAssert.AreEqual (
            Array.truncate count elements,
            IQueue.toArray truncated,
            "IQueue.truncate must return the first elements of a queue of another implementation"
        )

        Assert.AreEqual<int>(
            min (count + 1) elements.Length,
            read,
            "IQueue.truncate must read the elements that it keeps and the next one, which tells whether there are more"
        )

        Assert.AreEqual<int>(1, enumerators, "IQueue.truncate must take one enumerator of the queue")
        Assert.AreEqual<int>(1, disposals, "IQueue.truncate must dispose the enumerator that it takes")

    [<TestMethod>]
    [<DataRow(nameof IQueue.enqueue, "Enqueue")>]
    [<DataRow(nameof IQueue.enqueueMany, "EnqueueMany")>]
    [<DataRow(nameof IQueue.head, "Peek")>]
    [<DataRow(nameof IQueue.tail, "Dequeue")>]
    [<DataRow(nameof IQueue.tryHead, "TryPeek")>]
    [<DataRow(nameof IQueue.tryTail, "TryDequeue")>]
    [<DataRow(nameof IQueue.tryUncons, "TryPeekDequeue")>]
    [<DataRow(nameof IQueue.uncons, "PeekDequeue")>]
    member _.``the queue functions compile with the names of the queue operations of the BCL``
        (name : string, compiledName : string)
        =
        Assert.AreEqual<string | null>(
            name,
            QueueOps.sourceName "FSharp.Collections.Immutable.IQueue" compiledName,
            "IQueue must compile the function under the name of its queue operation in the BCL"
        )
