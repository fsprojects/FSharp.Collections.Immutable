namespace FSharp.Collections.Immutable.Tests

open System
open System.Collections.Generic
open System.Globalization
open FSharp.Collections.Immutable
open Microsoft.VisualStudio.TestTools.UnitTesting

/// The behaviour of `Queue` that the shared contracts do not cover: the first-in, first-out order, the extras, null inputs,
/// the results that are their inputs, the details of the semantics of the List module, the order of strings and the
/// compiled names.
[<TestClass; QueueTestCategory; PatternMatchingTestCategory>]
type QueueTests () =

    static let nullQueue = Unchecked.defaultof<Queue<int>>

    static let several () = Queue.ofArray Inputs.Several

    /// The elements of `Inputs.Several` in both stacks of a queue. ImmutableQueue dequeues from a front stack and enqueues
    /// onto a back stack, so a queue that was dequeued and then enqueued keeps elements in both.
    static let mixed () =
        Queue.ofArray [| 0; 5; -3; 8 |]
        |> Queue.tail
        |> Queue.enqueueMany [| 5; 0; -3; 12; 7 |]

    /// The exception of an action and its complete message, under the invariant culture, in which FSharp.Core writes the
    /// English messages that this library copies.
    static let thrown (action : unit -> unit) (message : string) =
        let culture = CultureInfo.CurrentUICulture
        CultureInfo.CurrentUICulture <- CultureInfo.InvariantCulture

        try
            Assert.Throws<exn>(Action action, message)
        finally
            CultureInfo.CurrentUICulture <- culture

    /// The functions that take a queue, by name, with the parameter that names the null queue and the function on a null
    /// queue. The rows of the test are the names: a function has no readable name in a row.
    static let nullQueueRejections =
        let pairs = Unchecked.defaultof<Queue<struct (int * int)>>
        let triples = Unchecked.defaultof<Queue<struct (int * int * int)>>
        let floats = Unchecked.defaultof<Queue<float>>

        [|
            struct (nameof Queue.isEmpty, "queue", (fun () -> Queue.isEmpty nullQueue |> ignore))
            struct (nameof Queue.length, "queue", (fun () -> Queue.length nullQueue |> ignore))
            struct (nameof Queue.exactlyOne, "queue", (fun () -> Queue.exactlyOne nullQueue |> ignore))
            struct (nameof Queue.head, "queue", (fun () -> Queue.head nullQueue |> ignore))
            struct (nameof Queue.last, "queue", (fun () -> Queue.last nullQueue |> ignore))
            struct (nameof Queue.tail, "queue", (fun () -> Queue.tail nullQueue |> ignore))
            struct (nameof Queue.tryExactlyOne, "queue", (fun () -> Queue.tryExactlyOne nullQueue |> ignore))
            struct (nameof Queue.tryHead, "queue", (fun () -> Queue.tryHead nullQueue |> ignore))
            struct (nameof Queue.tryLast, "queue", (fun () -> Queue.tryLast nullQueue |> ignore))
            struct (nameof Queue.tryTail, "queue", (fun () -> Queue.tryTail nullQueue |> ignore))
            struct (nameof Queue.tryUncons, "queue", (fun () -> Queue.tryUncons nullQueue |> ignore))
            struct (nameof Queue.uncons, "queue", (fun () -> Queue.uncons nullQueue |> ignore))
            struct (nameof Queue.(|Cons|_|), "queue", (fun () -> Queue.(|Cons|_|) nullQueue |> ignore))
            struct (nameof Queue.(|Nil|_|), "queue", (fun () -> Queue.(|Nil|_|) nullQueue |> ignore))
            struct (nameof Queue.contains, "source", (fun () -> Queue.contains 1 nullQueue |> ignore))
            struct (nameof Queue.exists, "queue", (fun () -> Queue.exists ((=) 1) nullQueue |> ignore))
            struct (nameof Queue.exists2, "queue2", (fun () -> Queue.exists2 (=) (several ()) nullQueue |> ignore))
            struct (nameof Queue.find, "queue", (fun () -> Queue.find ((=) 1) nullQueue |> ignore))
            struct (nameof Queue.findBack, "queue", (fun () -> Queue.findBack ((=) 1) nullQueue |> ignore))
            struct (nameof Queue.findIndex, "queue", (fun () -> Queue.findIndex ((=) 1) nullQueue |> ignore))
            struct (nameof Queue.findIndexBack, "queue", (fun () -> Queue.findIndexBack ((=) 1) nullQueue |> ignore))
            struct (nameof Queue.forall, "queue", (fun () -> Queue.forall ((=) 1) nullQueue |> ignore))
            struct (nameof Queue.forall2, "queue1", (fun () -> Queue.forall2 (=) nullQueue (several ()) |> ignore))
            struct (nameof Queue.pick, "queue", (fun () -> Queue.pick ValueSome nullQueue |> ignore))
            struct (nameof Queue.tryFind, "queue", (fun () -> Queue.tryFind ((=) 1) nullQueue |> ignore))
            struct (nameof Queue.tryFindBack, "queue", (fun () -> Queue.tryFindBack ((=) 1) nullQueue |> ignore))
            struct (nameof Queue.tryFindIndex, "queue", (fun () -> Queue.tryFindIndex ((=) 1) nullQueue |> ignore))
            struct (nameof Queue.tryFindIndexBack, "queue", (fun () -> Queue.tryFindIndexBack ((=) 1) nullQueue |> ignore))
            struct (nameof Queue.tryPick, "queue", (fun () -> Queue.tryPick ValueSome nullQueue |> ignore))
            struct (nameof Queue.allPairs, "queue1", (fun () -> Queue.allPairs nullQueue (several ()) |> ignore))
            struct (nameof Queue.append, "queue2", (fun () -> Queue.append (several ()) nullQueue |> ignore))
            struct (nameof Queue.choose, "queue", (fun () -> Queue.choose ValueSome nullQueue |> ignore))
            struct (nameof Queue.chunkBySize, "queue", (fun () -> Queue.chunkBySize 2 nullQueue |> ignore))
            struct (nameof Queue.collect, "queue", (fun () -> Queue.collect Queue.singleton nullQueue |> ignore))
            struct (nameof Queue.concat, "queues", (fun () -> Queue.concat [| (several ()); nullQueue |] |> ignore))
            struct (nameof Queue.enqueue, "queue", (fun () -> Queue.enqueue 1 nullQueue |> ignore))
            struct (nameof Queue.enqueueMany, "queue", (fun () -> Queue.enqueueMany [| 1 |] nullQueue |> ignore))
            struct (nameof Queue.filter, "queue", (fun () -> Queue.filter ((=) 1) nullQueue |> ignore))
            struct (nameof Queue.indexed, "queue", (fun () -> Queue.indexed nullQueue |> ignore))
            struct (nameof Queue.map, "queue", (fun () -> Queue.map id nullQueue |> ignore))
            struct (nameof Queue.map2, "queue2", (fun () -> Queue.map2 (+) (several ()) nullQueue |> ignore))
            struct (nameof Queue.map3,
                    "queue3",
                    (fun () ->
                        Queue.map3 (fun a b c -> a + b + c) (several ()) (several ()) nullQueue
                        |> ignore
                    ))
            struct (nameof Queue.mapi, "queue", (fun () -> Queue.mapi (+) nullQueue |> ignore))
            struct (nameof Queue.mapi2,
                    "queue1",
                    (fun () ->
                        Queue.mapi2 (fun index a b -> index + a + b) nullQueue (several ())
                        |> ignore
                    ))
            struct (nameof Queue.pairwise, "queue", (fun () -> Queue.pairwise nullQueue |> ignore))
            struct (nameof Queue.permute, "queue", (fun () -> Queue.permute id nullQueue |> ignore))
            struct (nameof Queue.rev, "queue", (fun () -> Queue.rev nullQueue |> ignore))
            struct (nameof Queue.transpose, "queues", (fun () -> Queue.transpose [| nullQueue |] |> ignore))
            struct (nameof Queue.unzip, "queue", (fun () -> Queue.unzip pairs |> ignore))
            struct (nameof Queue.unzip3, "queue", (fun () -> Queue.unzip3 triples |> ignore))
            struct (nameof Queue.where, "queue", (fun () -> Queue.where ((=) 1) nullQueue |> ignore))
            struct (nameof Queue.windowed, "queue", (fun () -> Queue.windowed 2 nullQueue |> ignore))
            struct (nameof Queue.zip, "queue1", (fun () -> Queue.zip nullQueue (several ()) |> ignore))
            struct (nameof Queue.zip3, "queue2", (fun () -> Queue.zip3 (several ()) nullQueue (several ()) |> ignore))
            struct (nameof Queue.average, "queue", (fun () -> Queue.average floats |> ignore))
            struct (nameof Queue.averageBy, "queue", (fun () -> Queue.averageBy float nullQueue |> ignore))
            struct (nameof Queue.compareWith, "queue2", (fun () -> Queue.compareWith compare (several ()) nullQueue |> ignore))
            struct (nameof Queue.fold, "queue", (fun () -> Queue.fold (+) 0 nullQueue |> ignore))
            struct (nameof Queue.fold2,
                    "queue2",
                    (fun () ->
                        Queue.fold2 (fun state a b -> state + a + b) 0 (several ()) nullQueue
                        |> ignore
                    ))
            struct (nameof Queue.foldBack, "queue", (fun () -> Queue.foldBack (+) nullQueue 0 |> ignore))
            struct (nameof Queue.foldBack2,
                    "queue1",
                    (fun () ->
                        Queue.foldBack2 (fun a b state -> state + a + b) nullQueue (several ()) 0
                        |> ignore
                    ))
            struct (nameof Queue.iter, "queue", (fun () -> Queue.iter ignore nullQueue))
            struct (nameof Queue.iter2, "queue2", (fun () -> Queue.iter2 (fun _ _ -> ()) (several ()) nullQueue))
            struct (nameof Queue.iteri, "queue", (fun () -> Queue.iteri (fun _ _ -> ()) nullQueue))
            struct (nameof Queue.iteri2, "queue1", (fun () -> Queue.iteri2 (fun _ _ _ -> ()) nullQueue (several ())))
            struct (nameof Queue.mapFold,
                    "queue",
                    (fun () ->
                        Queue.mapFold (fun state item -> struct (item, state)) 0 nullQueue
                        |> ignore
                    ))
            struct (nameof Queue.mapFoldBack,
                    "queue",
                    (fun () ->
                        Queue.mapFoldBack (fun item state -> struct (item, state)) nullQueue 0
                        |> ignore
                    ))
            struct (nameof Queue.max, "queue", (fun () -> Queue.max nullQueue |> ignore))
            struct (nameof Queue.maxBy, "queue", (fun () -> Queue.maxBy id nullQueue |> ignore))
            struct (nameof Queue.min, "queue", (fun () -> Queue.min nullQueue |> ignore))
            struct (nameof Queue.minBy, "queue", (fun () -> Queue.minBy id nullQueue |> ignore))
            struct (nameof Queue.reduce, "queue", (fun () -> Queue.reduce (+) nullQueue |> ignore))
            struct (nameof Queue.reduceBack, "queue", (fun () -> Queue.reduceBack (+) nullQueue |> ignore))
            struct (nameof Queue.scan, "queue", (fun () -> Queue.scan (+) 0 nullQueue |> ignore))
            struct (nameof Queue.scanBack, "queue", (fun () -> Queue.scanBack (+) nullQueue 0 |> ignore))
            struct (nameof Queue.sum, "queue", (fun () -> Queue.sum nullQueue |> ignore))
            struct (nameof Queue.sumBy, "queue", (fun () -> Queue.sumBy id nullQueue |> ignore))
            struct (nameof Queue.countBy, "queue", (fun () -> Queue.countBy id nullQueue |> ignore))
            struct (nameof Queue.distinct, "queue", (fun () -> Queue.distinct nullQueue |> ignore))
            struct (nameof Queue.distinctBy, "queue", (fun () -> Queue.distinctBy id nullQueue |> ignore))
            struct (nameof Queue.except, "queue", (fun () -> Queue.except [| 1 |] nullQueue |> ignore))
            struct (nameof Queue.groupBy, "queue", (fun () -> Queue.groupBy id nullQueue |> ignore))
            struct (nameof Queue.partition, "queue", (fun () -> Queue.partition ((=) 1) nullQueue |> ignore))
            struct (nameof Queue.partitionWith, "queue", (fun () -> Queue.partitionWith Choice1Of2 nullQueue |> ignore))
            struct (nameof Queue.skip, "queue", (fun () -> Queue.skip 0 nullQueue |> ignore))
            struct (nameof Queue.skipLast, "queue", (fun () -> Queue.skipLast 0 nullQueue |> ignore))
            struct (nameof Queue.skipWhile, "queue", (fun () -> Queue.skipWhile ((=) 1) nullQueue |> ignore))
            struct (nameof Queue.splitAt, "queue", (fun () -> Queue.splitAt 0 nullQueue |> ignore))
            struct (nameof Queue.splitInto, "queue", (fun () -> Queue.splitInto 2 nullQueue |> ignore))
            struct (nameof Queue.take, "queue", (fun () -> Queue.take 0 nullQueue |> ignore))
            struct (nameof Queue.takeLast, "queue", (fun () -> Queue.takeLast 0 nullQueue |> ignore))
            struct (nameof Queue.takeWhile, "queue", (fun () -> Queue.takeWhile ((=) 1) nullQueue |> ignore))
            struct (nameof Queue.truncate, "queue", (fun () -> Queue.truncate 1 nullQueue |> ignore))
            struct (nameof Queue.sort, "queue", (fun () -> Queue.sort nullQueue |> ignore))
            struct (nameof Queue.sortBy, "queue", (fun () -> Queue.sortBy id nullQueue |> ignore))
            struct (nameof Queue.sortByDescending, "queue", (fun () -> Queue.sortByDescending id nullQueue |> ignore))
            struct (nameof Queue.sortDescending, "queue", (fun () -> Queue.sortDescending nullQueue |> ignore))
            struct (nameof Queue.sortWith, "queue", (fun () -> Queue.sortWith compare nullQueue |> ignore))
            struct (nameof Queue.sortWithComparer,
                    "queue",
                    (fun () ->
                        Queue.sortWithComparer Comparer<int>.Default nullQueue
                        |> ignore
                    ))
            struct (nameof Queue.randomChoice, "source", (fun () -> Queue.randomChoice nullQueue |> ignore))
            struct (nameof Queue.randomChoiceBy, "source", (fun () -> Queue.randomChoiceBy (fun () -> 0.5) nullQueue |> ignore))
            struct (nameof Queue.randomChoiceWith, "source", (fun () -> Queue.randomChoiceWith (Random 1) nullQueue |> ignore))
            struct (nameof Queue.randomChoices, "source", (fun () -> Queue.randomChoices 1 nullQueue |> ignore))
            struct (nameof Queue.randomChoicesBy,
                    "source",
                    (fun () -> Queue.randomChoicesBy (fun () -> 0.5) 1 nullQueue |> ignore))
            struct (nameof Queue.randomChoicesWith, "source", (fun () -> Queue.randomChoicesWith (Random 1) 1 nullQueue |> ignore))
            struct (nameof Queue.randomSample, "source", (fun () -> Queue.randomSample 1 nullQueue |> ignore))
            struct (nameof Queue.randomSampleBy, "source", (fun () -> Queue.randomSampleBy (fun () -> 0.5) 1 nullQueue |> ignore))
            struct (nameof Queue.randomSampleWith, "source", (fun () -> Queue.randomSampleWith (Random 1) 1 nullQueue |> ignore))
            struct (nameof Queue.randomShuffle, "source", (fun () -> Queue.randomShuffle nullQueue |> ignore))
            struct (nameof Queue.randomShuffleBy, "source", (fun () -> Queue.randomShuffleBy (fun () -> 0.5) nullQueue |> ignore))
            struct (nameof Queue.randomShuffleWith, "source", (fun () -> Queue.randomShuffleWith (Random 1) nullQueue |> ignore))
            struct (nameof Queue.toArray, "queue", (fun () -> Queue.toArray nullQueue |> ignore))
            struct (nameof Queue.toList, "queue", (fun () -> Queue.toList nullQueue |> ignore))
            struct (nameof Queue.toSeq, "queue", (fun () -> Queue.toSeq nullQueue |> ignore))
        |]
        |> Array.map (fun struct (name, paramName, action) -> name, struct (paramName, action))
        |> Map.ofArray

    /// The functions that take another input that can be null, by name, with the name of that parameter and the function
    /// on a null input.
    static let otherNullRejections =
        [|
            struct (nameof Queue.ofArray, "array", (fun () -> Queue.ofArray (Unchecked.defaultof<int array>) |> ignore))
            struct (nameof Queue.ofSeq, "source", (fun () -> Queue.ofSeq (Unchecked.defaultof<seq<int>>) |> ignore))
            struct (nameof Queue.concat,
                    "queues",
                    (fun () ->
                        Queue.concat (Unchecked.defaultof<seq<Queue<int>>>)
                        |> ignore
                    ))
            struct (nameof Queue.transpose,
                    "queues",
                    (fun () ->
                        Queue.transpose (Unchecked.defaultof<seq<Queue<int>>>)
                        |> ignore
                    ))
            struct (nameof Queue.enqueueMany,
                    "values",
                    (fun () ->
                        Queue.enqueueMany (Unchecked.defaultof<seq<int>>) (several ())
                        |> ignore
                    ))
        |]
        |> Array.map (fun struct (name, paramName, action) -> name, struct (paramName, action))
        |> Map.ofArray

    /// The functions that take a sequence, by name and input, with the parameter that names the sequence and the function on
    /// the input. The inputs are a default flat list, which converts to a sequence without a check and fails with
    /// InvalidOperationException when it is enumerated, and a default flat list of strings as a sequence of objects, which a
    /// type test for the flat list of objects does not see because seq is covariant. The rows of the test are the names.
    static let defaultSequenceRejections =
        let defaultList = Inputs.DefaultFlatListAsSeq<int>()
        let defaultQueues = Inputs.DefaultFlatListAsSeq<Queue<int>>()
        let widened = FlatListOracles.defaultStringsAsObjects

        [|
            struct (nameof Queue.ofSeq, "default flat list", "source", (fun () -> Queue.ofSeq defaultList |> ignore))
            struct (nameof Queue.ofSeq,
                    "default flat list of strings as objects",
                    "source",
                    (fun () -> Queue.ofSeq widened |> ignore))
            struct (nameof Queue.concat, "default flat list", "queues", (fun () -> Queue.concat defaultQueues |> ignore))
            struct (nameof Queue.transpose, "default flat list", "queues", (fun () -> Queue.transpose defaultQueues |> ignore))
            struct (nameof Queue.enqueueMany,
                    "default flat list",
                    "values",
                    (fun () -> Queue.enqueueMany defaultList (several ()) |> ignore))
            struct (nameof Queue.enqueueMany,
                    "default flat list of strings as objects",
                    "values",
                    (fun () -> Queue.enqueueMany widened Queue.empty |> ignore))
            struct (nameof Queue.except,
                    "default flat list",
                    "itemsToExclude",
                    (fun () -> Queue.except defaultList (several ()) |> ignore))
            struct (nameof Queue.except,
                    "default flat list and an empty queue",
                    "itemsToExclude",
                    (fun () -> Queue.except defaultList Queue.empty |> ignore))
            struct (nameof Queue.except,
                    "default flat list of strings as objects",
                    "itemsToExclude",
                    (fun () -> Queue.except widened Queue.empty |> ignore))
        |]
        |> Array.map (fun struct (name, input, paramName, action) -> $"%s{name} of a %s{input}", struct (paramName, action))
        |> Map.ofArray

    /// The functions that keep every element of a queue for some arguments, by name, on such arguments. The names tell
    /// apart the cases of the same function.
    static let wholeQueues : Map<string, Queue<int> -> Queue<int>> =
        Map.ofArray [|
            nameof Queue.take, Queue.take 8
            nameof Queue.takeLast, Queue.takeLast 8
            nameof Queue.takeWhile, Queue.takeWhile (fun _ -> true)
            nameof Queue.truncate, Queue.truncate 100
            $"%s{nameof Queue.skip} of a negative count", Queue.skip -1
            $"%s{nameof Queue.skip} of zero", Queue.skip 0
            nameof Queue.skipLast, Queue.skipLast 0
            nameof Queue.skipWhile, Queue.skipWhile (fun _ -> false)
            ($"%s{nameof Queue.splitAt} (first part)",
             fun queue ->
                 let struct (whole, _) = Queue.splitAt 8 queue
                 whole)
            ($"%s{nameof Queue.splitAt} (second part)",
             fun queue ->
                 let struct (_, rest) = Queue.splitAt 0 queue
                 rest)
            $"%s{nameof Queue.append} of an empty second queue", (fun queue -> Queue.append queue Queue.empty)
            $"%s{nameof Queue.append} of an empty first queue", (fun queue -> Queue.append Queue.empty queue)
        |]

    /// The sorting functions, by name, with a projection or a comparison function that must not run on one element.
    static let singleElementSorts : Map<string, Queue<int> -> Queue<int>> =
        Map.ofArray [|
            nameof Queue.sort, Queue.sort
            nameof Queue.sortBy, Queue.sortBy (fun _ -> failwith<int> "The projection must not run")
            nameof Queue.sortByDescending, Queue.sortByDescending (fun _ -> failwith<int> "The projection must not run")
            nameof Queue.sortDescending, Queue.sortDescending
            nameof Queue.sortWith, Queue.sortWith (fun _ _ -> failwith "The comparer must not run")
            (nameof Queue.sortWithComparer,
             Queue.sortWithComparer (Comparer<int>.Create (fun _ _ -> failwith "The comparer must not run")))
        |]

    /// The sorts that compare the elements or their keys, by name, with the Array function that sorts in the same order.
    static let stringSorts : Map<string, struct ((string array -> string array) * (Queue<string> -> Queue<string>))> =
        Map.ofArray [|
            nameof Queue.sort, struct (Array.sort, Queue.sort)
            nameof Queue.sortBy, struct (Array.sortBy id, Queue.sortBy id)
            nameof Queue.sortByDescending, struct (Array.sortByDescending id, Queue.sortByDescending id)
            nameof Queue.sortDescending, struct (Array.sortDescending, Queue.sortDescending)
        |]

    /// The functions of two queues, by name, with the List function that they mirror, as functions of the action that
    /// records the pairs; both run on a shorter and a longer queue.
    static let commonPrefixWalks =
        let short = [| 1; 2 |]
        let long = [| 10; 20; 30 |]

        Map.ofArray [|
            (nameof Queue.iter2,
             struct ((fun (action : int -> int -> unit) -> List.iter2 action (List.ofArray short) (List.ofArray long)),
                     (fun (action : int -> int -> unit) -> Queue.iter2 action (Queue.ofArray short) (Queue.ofArray long))))
            (nameof Queue.map2,
             struct ((fun (action : int -> int -> unit) ->
                         List.map2 action (List.ofArray long) (List.ofArray short)
                         |> ignore
                     ),
                     (fun (action : int -> int -> unit) ->
                         Queue.map2 action (Queue.ofArray long) (Queue.ofArray short)
                         |> ignore
                     )))
            (nameof Queue.fold2,
             struct ((fun (action : int -> int -> unit) ->
                         List.fold2 (fun () a b -> action a b) () (List.ofArray short) (List.ofArray long)
                     ),
                     (fun (action : int -> int -> unit) ->
                         Queue.fold2 (fun () a b -> action a b) () (Queue.ofArray short) (Queue.ofArray long)
                     )))
            (nameof Queue.foldBack2,
             struct ((fun (action : int -> int -> unit) ->
                         List.foldBack2 (fun a b () -> action a b) (List.ofArray short) (List.ofArray long) ()
                     ),
                     (fun (action : int -> int -> unit) ->
                         Queue.foldBack2 (fun a b () -> action a b) (Queue.ofArray short) (Queue.ofArray long) ()
                     )))
        |]

    /// The functions whose exceptions List writes with more than one line, by name, with the List function on the same
    /// input. The names tell apart two inputs of the same function.
    static let completeMessages =
        let one = [| 1 |]
        let three = [| 1; 2; 3 |]
        let four = [| 1; 2; 3; 4 |]

        Map.ofArray [|
            (nameof Queue.zip3,
             struct ((fun () ->
                         List.zip3 (List.ofArray three) (List.ofArray one) (List.ofArray four)
                         |> ignore
                     ),
                     (fun () ->
                         Queue.zip3 (Queue.ofArray three) (Queue.ofArray one) (Queue.ofArray four)
                         |> ignore
                     )))
            (nameof Queue.map3,
             struct ((fun () ->
                         List.map3 (fun a b c -> a + b + c) (List.ofArray three) (List.ofArray one) (List.ofArray four)
                         |> ignore
                     ),
                     (fun () ->
                         Queue.map3 (fun a b c -> a + b + c) (Queue.ofArray three) (Queue.ofArray one) (Queue.ofArray four)
                         |> ignore
                     )))
            (nameof Queue.zip,
             struct ((fun () -> List.zip (List.ofArray four) (List.ofArray one) |> ignore),
                     (fun () -> Queue.zip (Queue.ofArray four) (Queue.ofArray one) |> ignore)))
            (nameof Queue.foldBack2,
             struct ((fun () ->
                         List.foldBack2 (fun _ _ state -> state) (List.ofArray one) (List.ofArray four) 0
                         |> ignore
                     ),
                     (fun () ->
                         Queue.foldBack2 (fun _ _ state -> state) (Queue.ofArray one) (Queue.ofArray four) 0
                         |> ignore
                     )))
            ($"%s{nameof Queue.foldBack2} of an empty first queue",
             struct ((fun () ->
                         List.foldBack2 (fun _ _ state -> state) List.empty<int> (List.ofArray four) 0
                         |> ignore
                     ),
                     (fun () ->
                         Queue.foldBack2 (fun _ _ state -> state) Queue.empty<int> (Queue.ofArray four) 0
                         |> ignore
                     )))
            (nameof Queue.head,
             struct ((fun () -> List.head List.empty<int> |> ignore), (fun () -> Queue.head Queue.empty<int> |> ignore)))
            (nameof Queue.take,
             struct ((fun () -> List.take 5 (List.ofArray three) |> ignore),
                     (fun () -> Queue.take 5 (Queue.ofArray three) |> ignore)))
            (nameof Queue.skip,
             struct ((fun () -> List.skip 5 (List.ofArray three) |> ignore),
                     (fun () -> Queue.skip 5 (Queue.ofArray three) |> ignore)))
            (nameof Queue.skipLast,
             struct ((fun () -> List.skip 5 (List.ofArray three) |> ignore),
                     (fun () -> Queue.skipLast 5 (Queue.ofArray three) |> ignore)))
            (nameof Queue.splitAt,
             struct ((fun () -> List.splitAt 5 (List.ofArray three) |> ignore),
                     (fun () -> Queue.splitAt 5 (Queue.ofArray three) |> ignore)))
            (nameof Queue.init, struct ((fun () -> List.init -2 id |> ignore), (fun () -> Queue.init -2 id |> ignore)))
            (nameof Queue.windowed,
             struct ((fun () -> List.windowed 0 (List.ofArray three) |> ignore),
                     (fun () -> Queue.windowed 0 (Queue.ofArray three) |> ignore)))
            (nameof Queue.transpose,
             struct ((fun () ->
                         List.transpose [| List.ofArray four; List.ofArray three; List.ofArray four |]
                         |> ignore
                     ),
                     (fun () ->
                         Queue.transpose [| Queue.ofArray four; Queue.ofArray three; Queue.ofArray four |]
                         |> ignore
                     )))
            (nameof Queue.randomChoiceBy,
             struct ((fun () ->
                         List.randomChoiceBy (fun () -> 1.5) (List.ofArray three)
                         |> ignore
                     ),
                     (fun () ->
                         Queue.randomChoiceBy (fun () -> 1.5) (Queue.ofArray three)
                         |> ignore
                     )))
        |]

    /// The names of the functions that take a queue, as the rows of their test.
    static member NullQueueRejections = nullQueueRejections.Keys

    /// The names of the functions that take another input that can be null, as the rows of their test.
    static member OtherNullRejections = otherNullRejections.Keys

    /// The names of the functions and the default flat lists that they reject as sequences, as the rows of their test.
    static member DefaultSequenceRejections = defaultSequenceRejections.Keys

    /// The names of the cases that keep every element of a queue, as the rows of their test.
    static member WholeQueues = wholeQueues.Keys

    /// The names of the sorting functions, as the rows of the test of one element.
    static member SingleElementSorts = singleElementSorts.Keys

    /// The names of the sorts that compare the elements or their keys, as the rows of the test of strings.
    static member StringSorts = stringSorts.Keys

    /// The names of the functions of two queues, as the rows of the test of the common prefix.
    static member CommonPrefixWalks = commonPrefixWalks.Keys

    /// The names of the functions whose exceptions have several lines, as the rows of the test of the messages.
    static member CompleteMessages = completeMessages.Keys

    [<TestMethod; DynamicData(nameof QueueTests.NullQueueRejections)>]
    member _.``functions reject a null queue`` (name : string) =
        let struct (paramName, action) = nullQueueRejections[name]

        let ex =
            Assert.ThrowsExactly<ArgumentNullException>(Action action, $"Queue.%s{name} must reject a null queue")

        Assert.AreEqual<string | null>(paramName, ex.ParamName, $"Queue.%s{name} must name the parameter of the null queue")

    [<TestMethod; DynamicData(nameof QueueTests.OtherNullRejections)>]
    member _.``functions reject other null inputs with the names of their parameters`` (name : string) =
        let struct (paramName, action) = otherNullRejections[name]

        let ex =
            Assert.ThrowsExactly<ArgumentNullException>(Action action, $"Queue.%s{name} must reject a null input")

        Assert.AreEqual<string | null>(paramName, ex.ParamName, $"Queue.%s{name} must name the null parameter")

    [<TestMethod; DynamicData(nameof QueueTests.DefaultSequenceRejections)>]
    member _.``functions reject a default flat list given as a sequence with the names of their parameters`` (name : string) =
        let struct (paramName, action) = defaultSequenceRejections[name]
        FlatListAssert.RejectsNull ($"Queue.%s{name}", paramName, action)

    [<TestMethod>]
    member _.``functions take a flat list of strings that is not default as a sequence of objects`` () =
        let widened = FlatListOracles.stringsAsObjects (FlatList.ofArray [| "a"; "b" |])
        let queue = Queue.ofSeq widened

        CollectionAssert.AreEqual (
            [| box "a"; box "b" |],
            Queue.toArray queue,
            "Queue.ofSeq must take a flat list of strings as objects"
        )

        CollectionAssert.AreEqual (
            [| box "a"; box "b"; box "a"; box "b" |],
            Queue.toArray (Queue.enqueueMany widened queue),
            "Queue.enqueueMany must take a flat list of strings as objects"
        )

        CollectionAssert.AreEqual (
            [| box "c" |],
            Queue.toArray (Queue.except widened (Queue.ofArray<obj> [| "a"; "b"; "c" |])),
            "Queue.except must take a flat list of strings as objects"
        )

    [<TestMethod>]
    member _.``the elements leave the queue in the order in which they joined it`` () =
        let joined =
            Queue.empty<int>
            |> Queue.enqueue 1
            |> Queue.enqueue 2
            |> Queue.enqueue 3

        let rec drain (rest : Queue<int>) =
            match rest with
            | Queue.Cons (struct (head, tail)) -> Array.append [| head |] (drain tail)
            | _ -> [||]

        Assert.AreEqual<int>(1, Queue.head joined, "Queue.head must return the element that joined first")
        Assert.AreEqual<int>(3, Queue.last joined, "Queue.last must return the element that joined last")
        CollectionAssert.AreEqual (
            [| 1; 2; 3 |],
            drain joined,
            "Queue.tail must remove the elements in the order in which they joined"
        )
        CollectionAssert.AreEqual (
            [| 1; 2; 3 |],
            List.toArray (Queue.toList joined),
            "Queue.toList must list the elements in the order in which they leave"
        )

    [<TestMethod; DynamicData(nameof Inputs.All, typeof<Inputs>)>]
    member _.``ofSeq is the inverse of toList`` (input : int array) =
        CollectionAssert.AreEqual (input, List.toArray (Queue.toList (Queue.ofSeq input)), "Queue.toList must undo Queue.ofSeq")

    [<TestMethod>]
    member _.``ofSeq of the list of a queue in both stacks rebuilds the queue`` () =
        let original = mixed ()

        CollectionAssert.AreEqual (
            Queue.toArray original,
            Queue.toArray (Queue.ofSeq (Queue.toList original)),
            "Queue.ofSeq must undo Queue.toList"
        )

    [<TestMethod>]
    member _.``ofSeq of a queue returns it without a copy`` () =
        let original = mixed ()
        Assert.AreSame (original, Queue.ofSeq original, "Queue.ofSeq of a queue must return it")

    [<TestMethod>]
    member _.``enqueue adds the value after the last element and leaves the input unchanged`` () =
        let original = several ()
        let enqueued = Queue.enqueue 99 original

        CollectionAssert.AreEqual (
            Array.append Inputs.Several [| 99 |],
            Queue.toArray enqueued,
            "Queue.enqueue must add the value at the end"
        )

        CollectionAssert.AreEqual (Inputs.Several, Queue.toArray original, "Queue.enqueue must leave the input unchanged")

    [<TestMethod>]
    member _.``enqueueMany adds the values in their order after the last element`` () =
        let original = several ()

        CollectionAssert.AreEqual (
            Array.append Inputs.Several [| 97; 98; 99 |],
            Queue.toArray (Queue.enqueueMany [| 97; 98; 99 |] original),
            "Queue.enqueueMany must add the values at the end, in their order"
        )

        CollectionAssert.AreEqual (
            [| 1; 2 |],
            Queue.toArray (Queue.enqueueMany (Seq.map id [| 1; 2 |]) Queue.empty),
            "Queue.enqueueMany must fill an empty queue from a lazy sequence"
        )

        Assert.AreSame (original, Queue.enqueueMany [||] original, "Queue.enqueueMany without values must return the input")
        CollectionAssert.AreEqual (Inputs.Several, Queue.toArray original, "Queue.enqueueMany must leave the input unchanged")

    [<TestMethod>]
    member _.``head and tail read the queue without changing it`` () =
        let original = mixed ()

        Assert.AreEqual<int>(Array.head Inputs.Several, Queue.head original, "Queue.head must return the first element")
        CollectionAssert.AreEqual (
            Array.tail Inputs.Several,
            Queue.toArray (Queue.tail original),
            "Queue.tail must drop the first element"
        )
        CollectionAssert.AreEqual (Inputs.Several, Queue.toArray original, "Queue.tail must leave the input unchanged")

    [<TestMethod>]
    member _.``uncons returns the first element and the other elements`` () =
        let struct (head, tail) = Queue.uncons (mixed ())
        Assert.AreEqual<int>(Array.head Inputs.Several, head, "Queue.uncons must return the first element")
        CollectionAssert.AreEqual (Array.tail Inputs.Several, Queue.toArray tail, "Queue.uncons must return the other elements")

    [<TestMethod>]
    member _.``uncons throws for an empty queue as List.head does`` () =
        ExceptionAssert.Same (
            QueueOps.paramName,
            (fun () -> List.head List.empty<int> |> ignore),
            (fun () -> Queue.uncons Queue.empty<int> |> ignore),
            "Queue.uncons of an empty queue must throw as List.head does"
        )

    [<TestMethod>]
    member _.``tryUncons returns the first element and the other elements or nothing`` () =
        match Queue.tryUncons (mixed ()) with
        | ValueSome (struct (head, tail)) ->
            Assert.AreEqual<int>(Array.head Inputs.Several, head, "Queue.tryUncons must return the first element")
            CollectionAssert.AreEqual (
                Array.tail Inputs.Several,
                Queue.toArray tail,
                "Queue.tryUncons must return the other elements"
            )
        | ValueNone -> Assert.Fail "Queue.tryUncons must return the elements of a queue with elements"

        Assert.IsTrue ((Queue.tryUncons Queue.empty<int>).IsNone, "Queue.tryUncons of an empty queue must return nothing")

    [<TestMethod>]
    member _.``exactlyOne tells one element from two in different stacks`` () =
        // The first enqueued element goes to the front stack of an empty queue, the second one to the back stack
        let two = Queue.empty<int> |> Queue.enqueue 1 |> Queue.enqueue 2
        let one = Queue.tail two

        Assert.AreEqual<int voption>(
            ValueNone,
            Queue.tryExactlyOne two,
            "Queue.tryExactlyOne of two elements must return nothing"
        )
        Assert.AreEqual<int voption>(ValueSome 2, Queue.tryExactlyOne one, "Queue.tryExactlyOne of one element must return it")
        Assert.AreEqual<int>(2, Queue.exactlyOne one, "Queue.exactlyOne of one element must return it")

        ExceptionAssert.Same (
            QueueOps.paramName,
            (fun () -> List.exactlyOne [ 1; 2 ] |> ignore),
            (fun () -> Queue.exactlyOne two |> ignore),
            "Queue.exactlyOne of two elements must throw as List.exactlyOne does"
        )

    [<TestMethod>]
    member _.``Cons matches the first element and the other elements`` () =
        let rec sum (rest : Queue<int>) =
            match rest with
            | Queue.Cons (struct (head, tail)) -> head + sum tail
            | _ -> 0

        Assert.AreEqual<int>(Array.sum Inputs.Several, sum (mixed ()), "Queue.Cons must match every element in turn")

        match Queue.empty<int> with
        | Queue.Cons _ -> Assert.Fail "Queue.Cons must not match an empty queue"
        | _ -> ()

    [<TestMethod>]
    member _.``Nil matches an empty queue only`` () =
        let describe (rest : Queue<int>) =
            match rest with
            | Queue.Nil -> "empty"
            | Queue.Cons (struct (head, _)) -> $"starts with %d{head}"
            | _ -> "unreachable"

        Assert.AreEqual<string>("empty", describe Queue.empty, "Queue.Nil must match an empty queue")
        Assert.AreEqual<string>("starts with 5", describe (several ()), "Queue.Nil must not match a queue with elements")

    [<TestMethod>]
    member _.``the functions read the elements of both stacks of a queue in order`` () =
        let both = mixed ()
        let elements = Inputs.Several
        let expectedBefore, expectedAfter = Array.splitAt 3 elements
        let struct (before, after) = Queue.splitAt 3 both

        CollectionAssert.AreEqual (
            elements,
            List.toArray (Queue.toList both),
            "Queue.toList must list the front stack and then the back stack"
        )

        CollectionAssert.AreEqual (elements, Queue.toArray both, "Queue.toArray must copy both stacks in order")
        Assert.AreEqual<int>(elements.Length, Queue.length both, "Queue.length must count both stacks")
        Assert.AreEqual<int>(Array.last elements, Queue.last both, "Queue.last must return the last element of the back stack")
        CollectionAssert.AreEqual (Array.rev elements, Queue.toArray (Queue.rev both), "Queue.rev must reverse both stacks")
        CollectionAssert.AreEqual (
            Array.skip 3 elements,
            Queue.toArray (Queue.skip 3 both),
            "Queue.skip must dequeue into the back stack"
        )
        CollectionAssert.AreEqual (
            Array.take 3 elements,
            Queue.toArray (Queue.take 3 both),
            "Queue.take must take from the front stack"
        )
        CollectionAssert.AreEqual (
            Array.skip 5 elements,
            Queue.toArray (Queue.takeLast 3 both),
            "Queue.takeLast must take from the back stack"
        )
        CollectionAssert.AreEqual (
            Array.take 5 elements,
            Queue.toArray (Queue.skipLast 3 both),
            "Queue.skipLast must skip from the back stack"
        )
        CollectionAssert.AreEqual (expectedBefore, Queue.toArray before, "Queue.splitAt must return the first part")
        CollectionAssert.AreEqual (expectedAfter, Queue.toArray after, "Queue.splitAt must return the second part")
        CollectionAssert.AreEqual (Array.sort elements, Queue.toArray (Queue.sort both), "Queue.sort must sort both stacks")

        CollectionAssert.AreEqual (
            Array.foldBack (fun item state -> Array.append state [| item |]) elements [||],
            Queue.foldBack (fun item state -> Array.append state [| item |]) both [||],
            "Queue.foldBack must visit both stacks from the last element"
        )

    [<TestMethod; DynamicData(nameof QueueTests.WholeQueues)>]
    member _.``functions return the input itself when they keep every element`` (name : string) =
        let original = mixed ()
        Assert.AreSame (original, wholeQueues[name] original, $"Queue.%s{name} must return the input when it keeps every element")

    [<TestMethod; DynamicData(nameof QueueTests.SingleElementSorts)>]
    member _.``sorts return a queue of one element itself, as List does`` (name : string) =
        let single = Queue.singleton 1
        Assert.AreSame (single, singleElementSorts[name] single, $"Queue.%s{name} of one element must return the input")

    [<TestMethod; DynamicData(nameof QueueTests.StringSorts)>]
    member _.``sorts order strings in F# generic comparison, which is ordinal`` (name : string) =
        // A comparison under a culture puts "a" before "A" and both before "b"; F# generic comparison compares ordinally
        let elements = [| "b"; "A"; "a"; "B" |]
        let struct (oracle, sort) = stringSorts[name]

        CollectionAssert.AreEqual (
            oracle elements,
            Queue.toArray (sort (Queue.ofArray elements)),
            $"Queue.%s{name} must order strings in F# generic comparison, as the List function does"
        )

    [<TestMethod>]
    member _.``contains compares in F# structural equality`` () =
        Assert.IsTrue (
            Queue.contains [| 1; 2 |] (Queue.singleton [| 1; 2 |]),
            "Queue.contains must compare arrays by their elements, as List.contains does"
        )

        Assert.AreEqual<bool>(
            List.contains Double.NaN [ Double.NaN ],
            Queue.contains Double.NaN (Queue.singleton Double.NaN),
            "Queue.contains must compare NaN as List.contains does"
        )

    [<TestMethod>]
    member _.``map and mapi call the mapping in the order of the elements`` () =
        let original = Queue.init 100 id
        let visited = ResizeArray<int>()
        Queue.map visited.Add original |> ignore
        CollectionAssert.AreEqual (Array.init 100 id, visited.ToArray (), "Queue.map must call the mapping in order")

        visited.Clear ()
        Queue.mapi (fun _ item -> visited.Add item) original
        |> ignore
        CollectionAssert.AreEqual (Array.init 100 id, visited.ToArray (), "Queue.mapi must call the mapping in order")

    [<TestMethod; DynamicData(nameof QueueTests.CommonPrefixWalks)>]
    member _.``the functions of two queues walk the common prefix before they throw, as List does`` (name : string) =
        let struct (oracle, actual) = commonPrefixWalks[name]

        let walk (run : (int -> int -> unit) -> unit) =
            let visited = ResizeArray<struct (int * int)>()

            try
                run (fun a b -> visited.Add (struct (a, b)))
            with :? ArgumentException ->
                ()

            visited.ToArray ()

        CollectionAssert.AreEqual (
            walk oracle,
            walk actual,
            $"Queue.%s{name} must call its function for the pairs that List calls it for"
        )

    [<TestMethod>]
    member _.``exists2 and forall2 decide before they compare the lengths, as List does`` () =
        let short = [| 1; 2 |]
        let long = [| 1; 20; 30 |]

        Assert.AreEqual<bool>(
            List.exists2 (=) (List.ofArray short) (List.ofArray long),
            Queue.exists2 (=) (Queue.ofArray short) (Queue.ofArray long),
            "Queue.exists2 must return true for a match before the end of the shorter queue"
        )

        Assert.AreEqual<bool>(
            List.forall2 (=) (List.ofArray short) (List.ofArray long),
            Queue.forall2 (=) (Queue.ofArray short) (Queue.ofArray long),
            "Queue.forall2 must return false for a mismatch before the end of the shorter queue"
        )

    [<TestMethod>]
    member _.``except does not read the items to exclude for an empty queue, as List does`` () =
        let items = seq { failwith "The items to exclude must not be read" }
        Assert.IsEmpty (Queue.except items Queue.empty<int>, "Queue.except of an empty queue must be empty")

    [<TestMethod>]
    member _.``transpose names the queue that ends first as List names the list`` () =
        ExceptionAssert.Same (
            QueueOps.paramName,
            (fun () -> List.transpose [| [ 1 ]; [ 1; 2 ] |] |> ignore),
            (fun () ->
                Queue.transpose [| Queue.ofArray [| 1 |]; Queue.ofArray [| 1; 2 |] |]
                |> ignore
            ),
            "Queue.transpose of a shorter first queue must throw as List.transpose does"
        )

    [<TestMethod>]
    member _.``splitAt of an empty queue throws as List.splitAt does`` () =
        ExceptionAssert.Same (
            QueueOps.paramName,
            (fun () -> List.splitAt 1 List.empty<int> |> ignore),
            (fun () -> Queue.splitAt 1 Queue.empty<int> |> ignore),
            "Queue.splitAt 1 of an empty queue must throw as List.splitAt does"
        )

    [<TestMethod; DynamicData(nameof QueueTests.CompleteMessages)>]
    member _.``the exceptions have the complete messages of List with the names of the queues`` (name : string) =
        let struct (oracle, actual) = completeMessages[name]
        let expected = thrown oracle $"List.%s{name} must throw"
        let ex = thrown actual $"Queue.%s{name} must throw"
        Assert.AreEqual<Type>(expected.GetType (), ex.GetType (), $"Queue.%s{name} must throw the type of exception of List")

        Assert.AreEqual<string>(
            QueueOps.message expected.Message,
            ex.Message,
            $"Queue.%s{name} must throw the message of List with the names of the queues"
        )

    [<TestMethod>]
    [<DataRow(nameof Queue.enqueue, "Enqueue")>]
    [<DataRow(nameof Queue.enqueueMany, "EnqueueMany")>]
    [<DataRow(nameof Queue.head, "Peek")>]
    [<DataRow(nameof Queue.tail, "Dequeue")>]
    [<DataRow(nameof Queue.tryHead, "TryPeek")>]
    [<DataRow(nameof Queue.tryTail, "TryDequeue")>]
    [<DataRow(nameof Queue.tryUncons, "TryPeekDequeue")>]
    [<DataRow(nameof Queue.uncons, "PeekDequeue")>]
    member _.``the queue functions compile with the names of the queue operations of the BCL``
        (name : string, compiledName : string)
        =
        Assert.AreEqual<string | null>(
            name,
            QueueOps.sourceName "FSharp.Collections.Immutable.Queue" compiledName,
            "Queue must compile the function under the name of its queue operation in the BCL"
        )
