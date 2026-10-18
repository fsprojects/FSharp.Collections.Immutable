namespace FSharp.Collections.Immutable.Tests

open System
open System.Collections.Generic
open System.Collections.Immutable
open System.Globalization
open System.Text.RegularExpressions
open FSharp.Collections.Immutable
open Microsoft.VisualStudio.TestTools.UnitTesting

/// The behaviour of `Stack` that the shared contracts do not cover: the order of the elements, which puts the first element
/// on top, the shared nodes, null inputs, the extras and the details of the semantics of the List module.
[<TestClass; StackTestCategory; PatternMatchingTestCategory>]
type StackTests () =

    static let nullStack = Unchecked.defaultof<Stack<int>>

    static let several () = Stack.ofArray Inputs.Several

    /// The exception of an action and its complete message, under the invariant culture, in which FSharp.Core writes the
    /// English messages that this library copies.
    static let thrown (action : unit -> unit) (message : string) =
        let culture = CultureInfo.CurrentUICulture
        CultureInfo.CurrentUICulture <- CultureInfo.InvariantCulture

        try
            Assert.Throws<exn>(Action action, message)
        finally
            CultureInfo.CurrentUICulture <- culture

    /// The names of the lists in a message of List, such as "list1", "list.[1]" and "(Parameter 'list')", as the names of
    /// the stacks; the words of the text, such as "The input list was empty.", stay.
    static let stackNames (message : string) = Regex.Replace (message, @"\blist(?=\d|\.\[|')", "stack")

    /// The creation functions, by name, as functions that create a stack of 1, 2 and 3. The rows of the test are the
    /// names: a function has no readable name in a row.
    static let creations : Map<string, unit -> Stack<int>> =
        Map.ofArray [|
            nameof Stack.ofArray, (fun () -> Stack.ofArray [| 1; 2; 3 |])
            nameof Stack.ofList, (fun () -> Stack.ofList [ 1; 2; 3 ])
            nameof Stack.ofReadOnlySpan, (fun () -> Stack.ofReadOnlySpan (ReadOnlySpan [| 1; 2; 3 |]))
            nameof Stack.ofSeq, (fun () -> Stack.ofSeq (Seq.map id [| 1; 2; 3 |]))
            nameof Stack.ofSpan, (fun () -> Stack.ofSpan (Span [| 1; 2; 3 |]))
            nameof Stack.init, (fun () -> Stack.init 3 (fun index -> index + 1))
            (nameof Stack.unfold,
             fun () ->
                 Stack.unfold
                     (fun state ->
                         if state <= 3 then
                             ValueSome (struct (state, state + 1))
                         else
                             ValueNone
                     )
                     1)
        |]

    /// The functions that remove the first elements, by name, with the number of elements that they remove from the
    /// input with several elements.
    static let topRemovals : Map<string, struct ((Stack<int> -> Stack<int>) * int)> =
        Map.ofArray [|
            nameof Stack.tail, struct (Stack.tail, 1)
            nameof Stack.tryTail, struct ((fun stack -> (Stack.tryTail stack).Value), 1)
            nameof Stack.skip, struct (Stack.skip 3, 3)
            nameof Stack.skipWhile, struct (Stack.skipWhile (fun item -> item <> 8), 2)
            nameof Stack.takeLast, struct (Stack.takeLast 5, 3)
            (nameof Stack.splitAt,
             struct ((fun stack ->
                         let struct (_, after) = Stack.splitAt 3 stack
                         after
                     ),
                     3))
        |]

    /// The functions that keep every element of the input with several elements, by name.
    static let wholeStacks : Map<string, Stack<int> -> Stack<int>> =
        Map.ofArray [|
            nameof Stack.ofSeq, (fun stack -> Stack.ofSeq stack)
            nameof Stack.take, Stack.take 8
            nameof Stack.takeLast, Stack.takeLast 8
            nameof Stack.takeWhile, Stack.takeWhile (fun _ -> true)
            nameof Stack.truncate, Stack.truncate 100
            nameof Stack.skip, Stack.skip -1
            nameof Stack.skipLast, Stack.skipLast 0
            nameof Stack.skipWhile, Stack.skipWhile (fun _ -> false)
            (nameof Stack.splitAt,
             fun stack ->
                 let struct (before, _) = Stack.splitAt 8 stack
                 before)
        |]

    /// The sorting functions, by name, with a projection or a comparison function that must not run on one element.
    static let singleElementSorts : Map<string, Stack<int> -> Stack<int>> =
        Map.ofArray [|
            nameof Stack.sort, Stack.sort
            nameof Stack.sortBy, Stack.sortBy (fun _ -> failwith<int> "The projection must not run")
            (nameof Stack.sortByDescending, Stack.sortByDescending (fun _ -> failwith<int> "The projection must not run"))
            nameof Stack.sortDescending, Stack.sortDescending
            nameof Stack.sortWith, Stack.sortWith (fun _ _ -> failwith "The comparer must not run")
            nameof Stack.sortWithComparer, Stack.sortWithComparer Comparer<int>.Default
        |]

    /// The functions that take a stack, by name, with the parameter that names the null stack and the function on a null
    /// stack. The rows of the test are the names: a function has no readable name in a row.
    static let nullStackRejections =
        let stack = several ()
        let pairs = Unchecked.defaultof<Stack<struct (int * int)>>
        let triples = Unchecked.defaultof<Stack<struct (int * int * int)>>
        let floats = Unchecked.defaultof<Stack<float>>

        [|
            struct (nameof Stack.isEmpty, "stack", (fun () -> Stack.isEmpty nullStack |> ignore))
            struct (nameof Stack.length, "stack", (fun () -> Stack.length nullStack |> ignore))
            struct (nameof Stack.exactlyOne, "stack", (fun () -> Stack.exactlyOne nullStack |> ignore))
            struct (nameof Stack.head, "stack", (fun () -> Stack.head nullStack |> ignore))
            struct (nameof Stack.last, "stack", (fun () -> Stack.last nullStack |> ignore))
            struct (nameof Stack.tail, "stack", (fun () -> Stack.tail nullStack |> ignore))
            struct (nameof Stack.tryExactlyOne, "stack", (fun () -> Stack.tryExactlyOne nullStack |> ignore))
            struct (nameof Stack.tryHead, "stack", (fun () -> Stack.tryHead nullStack |> ignore))
            struct (nameof Stack.tryLast, "stack", (fun () -> Stack.tryLast nullStack |> ignore))
            struct (nameof Stack.tryTail, "stack", (fun () -> Stack.tryTail nullStack |> ignore))
            struct (nameof Stack.tryUncons, "stack", (fun () -> Stack.tryUncons nullStack |> ignore))
            struct (nameof Stack.uncons, "stack", (fun () -> Stack.uncons nullStack |> ignore))
            struct (nameof Stack.(|Cons|_|), "stack", (fun () -> Stack.(|Cons|_|) nullStack |> ignore))
            struct (nameof Stack.(|Nil|_|), "stack", (fun () -> Stack.(|Nil|_|) nullStack |> ignore))
            struct (nameof Stack.contains, "source", (fun () -> Stack.contains 1 nullStack |> ignore))
            struct (nameof Stack.exists, "stack", (fun () -> Stack.exists ((=) 1) nullStack |> ignore))
            struct (nameof Stack.exists2, "stack2", (fun () -> Stack.exists2 (=) stack nullStack |> ignore))
            struct (nameof Stack.find, "stack", (fun () -> Stack.find ((=) 1) nullStack |> ignore))
            struct (nameof Stack.findBack, "stack", (fun () -> Stack.findBack ((=) 1) nullStack |> ignore))
            struct (nameof Stack.findIndex, "stack", (fun () -> Stack.findIndex ((=) 1) nullStack |> ignore))
            struct (nameof Stack.findIndexBack, "stack", (fun () -> Stack.findIndexBack ((=) 1) nullStack |> ignore))
            struct (nameof Stack.forall, "stack", (fun () -> Stack.forall ((=) 1) nullStack |> ignore))
            struct (nameof Stack.forall2, "stack1", (fun () -> Stack.forall2 (=) nullStack stack |> ignore))
            struct (nameof Stack.pick, "stack", (fun () -> Stack.pick ValueSome nullStack |> ignore))
            struct (nameof Stack.tryFind, "stack", (fun () -> Stack.tryFind ((=) 1) nullStack |> ignore))
            struct (nameof Stack.tryFindBack, "stack", (fun () -> Stack.tryFindBack ((=) 1) nullStack |> ignore))
            struct (nameof Stack.tryFindIndex, "stack", (fun () -> Stack.tryFindIndex ((=) 1) nullStack |> ignore))
            struct (nameof Stack.tryFindIndexBack, "stack", (fun () -> Stack.tryFindIndexBack ((=) 1) nullStack |> ignore))
            struct (nameof Stack.tryPick, "stack", (fun () -> Stack.tryPick ValueSome nullStack |> ignore))
            struct (nameof Stack.allPairs, "stack1", (fun () -> Stack.allPairs nullStack stack |> ignore))
            struct (nameof Stack.append, "stack2", (fun () -> Stack.append stack nullStack |> ignore))
            struct (nameof Stack.choose, "stack", (fun () -> Stack.choose ValueSome nullStack |> ignore))
            struct (nameof Stack.chunkBySize, "stack", (fun () -> Stack.chunkBySize 2 nullStack |> ignore))
            struct (nameof Stack.collect, "stack", (fun () -> Stack.collect Stack.singleton nullStack |> ignore))
            struct (nameof Stack.concat, "stacks", (fun () -> Stack.concat [| stack; nullStack |] |> ignore))
            struct (nameof Stack.cons, "stack", (fun () -> Stack.cons 1 nullStack |> ignore))
            struct (nameof Stack.consMany, "stack", (fun () -> Stack.consMany [| 1 |] nullStack |> ignore))
            struct (nameof Stack.filter, "stack", (fun () -> Stack.filter ((=) 1) nullStack |> ignore))
            struct (nameof Stack.indexed, "stack", (fun () -> Stack.indexed nullStack |> ignore))
            struct (nameof Stack.map, "stack", (fun () -> Stack.map id nullStack |> ignore))
            struct (nameof Stack.map2, "stack2", (fun () -> Stack.map2 (+) stack nullStack |> ignore))
            struct (nameof Stack.map3,
                    "stack3",
                    (fun () ->
                        Stack.map3 (fun a b c -> a + b + c) stack stack nullStack
                        |> ignore
                    ))
            struct (nameof Stack.mapi, "stack", (fun () -> Stack.mapi (+) nullStack |> ignore))
            struct (nameof Stack.mapi2,
                    "stack1",
                    (fun () ->
                        Stack.mapi2 (fun index a b -> index + a + b) nullStack stack
                        |> ignore
                    ))
            struct (nameof Stack.pairwise, "stack", (fun () -> Stack.pairwise nullStack |> ignore))
            struct (nameof Stack.permute, "stack", (fun () -> Stack.permute id nullStack |> ignore))
            struct (nameof Stack.rev, "stack", (fun () -> Stack.rev nullStack |> ignore))
            struct (nameof Stack.transpose, "stacks", (fun () -> Stack.transpose [| nullStack |] |> ignore))
            struct (nameof Stack.unzip, "stack", (fun () -> Stack.unzip pairs |> ignore))
            struct (nameof Stack.unzip3, "stack", (fun () -> Stack.unzip3 triples |> ignore))
            struct (nameof Stack.where, "stack", (fun () -> Stack.where ((=) 1) nullStack |> ignore))
            struct (nameof Stack.windowed, "stack", (fun () -> Stack.windowed 2 nullStack |> ignore))
            struct (nameof Stack.zip, "stack1", (fun () -> Stack.zip nullStack stack |> ignore))
            struct (nameof Stack.zip3, "stack2", (fun () -> Stack.zip3 stack nullStack stack |> ignore))
            struct (nameof Stack.average, "stack", (fun () -> Stack.average floats |> ignore))
            struct (nameof Stack.averageBy, "stack", (fun () -> Stack.averageBy float nullStack |> ignore))
            struct (nameof Stack.compareWith, "stack2", (fun () -> Stack.compareWith compare stack nullStack |> ignore))
            struct (nameof Stack.fold, "stack", (fun () -> Stack.fold (+) 0 nullStack |> ignore))
            struct (nameof Stack.fold2,
                    "stack2",
                    (fun () ->
                        Stack.fold2 (fun state a b -> state + a + b) 0 stack nullStack
                        |> ignore
                    ))
            struct (nameof Stack.foldBack, "stack", (fun () -> Stack.foldBack (+) nullStack 0 |> ignore))
            struct (nameof Stack.foldBack2,
                    "stack1",
                    (fun () ->
                        Stack.foldBack2 (fun a b state -> state + a + b) nullStack stack 0
                        |> ignore
                    ))
            struct (nameof Stack.iter, "stack", (fun () -> Stack.iter ignore nullStack))
            struct (nameof Stack.iter2, "stack2", (fun () -> Stack.iter2 (fun _ _ -> ()) stack nullStack))
            struct (nameof Stack.iteri, "stack", (fun () -> Stack.iteri (fun _ _ -> ()) nullStack))
            struct (nameof Stack.iteri2, "stack1", (fun () -> Stack.iteri2 (fun _ _ _ -> ()) nullStack stack))
            struct (nameof Stack.mapFold,
                    "stack",
                    (fun () ->
                        Stack.mapFold (fun state item -> struct (item, state)) 0 nullStack
                        |> ignore
                    ))
            struct (nameof Stack.mapFoldBack,
                    "stack",
                    (fun () ->
                        Stack.mapFoldBack (fun item state -> struct (item, state)) nullStack 0
                        |> ignore
                    ))
            struct (nameof Stack.max, "stack", (fun () -> Stack.max nullStack |> ignore))
            struct (nameof Stack.maxBy, "stack", (fun () -> Stack.maxBy id nullStack |> ignore))
            struct (nameof Stack.min, "stack", (fun () -> Stack.min nullStack |> ignore))
            struct (nameof Stack.minBy, "stack", (fun () -> Stack.minBy id nullStack |> ignore))
            struct (nameof Stack.reduce, "stack", (fun () -> Stack.reduce (+) nullStack |> ignore))
            struct (nameof Stack.reduceBack, "stack", (fun () -> Stack.reduceBack (+) nullStack |> ignore))
            struct (nameof Stack.scan, "stack", (fun () -> Stack.scan (+) 0 nullStack |> ignore))
            struct (nameof Stack.scanBack, "stack", (fun () -> Stack.scanBack (+) nullStack 0 |> ignore))
            struct (nameof Stack.sum, "stack", (fun () -> Stack.sum nullStack |> ignore))
            struct (nameof Stack.sumBy, "stack", (fun () -> Stack.sumBy id nullStack |> ignore))
            struct (nameof Stack.countBy, "stack", (fun () -> Stack.countBy id nullStack |> ignore))
            struct (nameof Stack.distinct, "stack", (fun () -> Stack.distinct nullStack |> ignore))
            struct (nameof Stack.distinctBy, "stack", (fun () -> Stack.distinctBy id nullStack |> ignore))
            struct (nameof Stack.except, "stack", (fun () -> Stack.except [| 1 |] nullStack |> ignore))
            struct (nameof Stack.groupBy, "stack", (fun () -> Stack.groupBy id nullStack |> ignore))
            struct (nameof Stack.partition, "stack", (fun () -> Stack.partition ((=) 1) nullStack |> ignore))
            struct (nameof Stack.partitionWith, "stack", (fun () -> Stack.partitionWith Choice1Of2 nullStack |> ignore))
            struct (nameof Stack.skip, "stack", (fun () -> Stack.skip 0 nullStack |> ignore))
            struct (nameof Stack.skipLast, "stack", (fun () -> Stack.skipLast 0 nullStack |> ignore))
            struct (nameof Stack.skipWhile, "stack", (fun () -> Stack.skipWhile ((=) 1) nullStack |> ignore))
            struct (nameof Stack.splitAt, "stack", (fun () -> Stack.splitAt 0 nullStack |> ignore))
            struct (nameof Stack.splitInto, "stack", (fun () -> Stack.splitInto 2 nullStack |> ignore))
            struct (nameof Stack.take, "stack", (fun () -> Stack.take 0 nullStack |> ignore))
            struct (nameof Stack.takeLast, "stack", (fun () -> Stack.takeLast 0 nullStack |> ignore))
            struct (nameof Stack.takeWhile, "stack", (fun () -> Stack.takeWhile ((=) 1) nullStack |> ignore))
            struct (nameof Stack.truncate, "stack", (fun () -> Stack.truncate 1 nullStack |> ignore))
            struct (nameof Stack.sort, "stack", (fun () -> Stack.sort nullStack |> ignore))
            struct (nameof Stack.sortBy, "stack", (fun () -> Stack.sortBy id nullStack |> ignore))
            struct (nameof Stack.sortByDescending, "stack", (fun () -> Stack.sortByDescending id nullStack |> ignore))
            struct (nameof Stack.sortDescending, "stack", (fun () -> Stack.sortDescending nullStack |> ignore))
            struct (nameof Stack.sortWith, "stack", (fun () -> Stack.sortWith compare nullStack |> ignore))
            struct (nameof Stack.sortWithComparer,
                    "stack",
                    (fun () ->
                        Stack.sortWithComparer Comparer<int>.Default nullStack
                        |> ignore
                    ))
            struct (nameof Stack.randomChoice, "source", (fun () -> Stack.randomChoice nullStack |> ignore))
            struct (nameof Stack.randomChoiceBy, "source", (fun () -> Stack.randomChoiceBy (fun () -> 0.5) nullStack |> ignore))
            struct (nameof Stack.randomChoiceWith, "source", (fun () -> Stack.randomChoiceWith (Random 1) nullStack |> ignore))
            struct (nameof Stack.randomChoices, "source", (fun () -> Stack.randomChoices 1 nullStack |> ignore))
            struct (nameof Stack.randomChoicesBy,
                    "source",
                    (fun () -> Stack.randomChoicesBy (fun () -> 0.5) 1 nullStack |> ignore))
            struct (nameof Stack.randomChoicesWith, "source", (fun () -> Stack.randomChoicesWith (Random 1) 1 nullStack |> ignore))
            struct (nameof Stack.randomSample, "source", (fun () -> Stack.randomSample 1 nullStack |> ignore))
            struct (nameof Stack.randomSampleBy, "source", (fun () -> Stack.randomSampleBy (fun () -> 0.5) 1 nullStack |> ignore))
            struct (nameof Stack.randomSampleWith, "source", (fun () -> Stack.randomSampleWith (Random 1) 1 nullStack |> ignore))
            struct (nameof Stack.randomShuffle, "source", (fun () -> Stack.randomShuffle nullStack |> ignore))
            struct (nameof Stack.randomShuffleBy, "source", (fun () -> Stack.randomShuffleBy (fun () -> 0.5) nullStack |> ignore))
            struct (nameof Stack.randomShuffleWith, "source", (fun () -> Stack.randomShuffleWith (Random 1) nullStack |> ignore))
            struct (nameof Stack.toArray, "stack", (fun () -> Stack.toArray nullStack |> ignore))
            struct (nameof Stack.toList, "stack", (fun () -> Stack.toList nullStack |> ignore))
            struct (nameof Stack.toSeq, "stack", (fun () -> Stack.toSeq nullStack |> ignore))
        |]
        |> Array.map (fun struct (name, paramName, action) -> name, struct (paramName, action))
        |> Map.ofArray

    /// The functions that take another input that can be null, by name, with the name of that parameter and the function
    /// on a null input. The shared contracts cover a null sequence for ofSeq, concat, transpose and except.
    static let otherNullRejections =
        let stack = several ()

        [|
            struct (nameof Stack.ofArray, "array", (fun () -> Stack.ofArray (Unchecked.defaultof<int array>) |> ignore))
            struct (nameof Stack.consMany,
                    "values",
                    (fun () ->
                        Stack.consMany (Unchecked.defaultof<seq<int>>) stack
                        |> ignore
                    ))
        |]
        |> Array.map (fun struct (name, paramName, action) -> name, struct (paramName, action))
        |> Map.ofArray

    /// The functions that take a sequence, by name and by the input that they reject, with the parameter that names the
    /// input and the function on it. The shared contracts cover a null sequence and a default flat list given as a
    /// sequence of integers for ofSeq, concat, transpose and except; the rows are consMany, which no contract covers, a
    /// default flat list of strings given as a sequence of objects, which a type test for the flat list of objects does
    /// not see, and the order of the checks. A default flat list converts to a sequence without a check and fails when the
    /// sequence is enumerated, so the other inputs are valid.
    static let sequenceRejections : Map<string, struct (string * (unit -> unit))> =
        let widenedItems = FlatListOracles.defaultStringsAsObjects
        let objects = Stack.ofArray [| "a" :> obj |]
        let defaultItems = Inputs.DefaultFlatListAsSeq<int>()

        Map.ofArray [|
            "ofSeq (default flat list of strings as objects)", struct ("source", (fun () -> Stack.ofSeq widenedItems |> ignore))
            "consMany (default flat list)", struct ("values", (fun () -> Stack.consMany defaultItems (several ()) |> ignore))
            "consMany (default flat list of strings as objects)",
            struct ("values", (fun () -> Stack.consMany widenedItems objects |> ignore))
            "except (default flat list of strings as objects)",
            struct ("itemsToExclude", (fun () -> Stack.except widenedItems objects |> ignore))
            // List.except checks the items to exclude before the list
            "except (default flat list and null stack)",
            struct ("itemsToExclude", (fun () -> Stack.except defaultItems nullStack |> ignore))
        |]

    /// The functions of two stacks, by name, with the List function that they mirror, as functions of the action that
    /// records the pairs; both run on a shorter and a longer input.
    static let commonPrefixWalks =
        let short = [| 1; 2 |]
        let long = [| 10; 20; 30 |]

        Map.ofArray [|
            (nameof Stack.iter2,
             struct ((fun (action : int -> int -> unit) -> List.iter2 action (List.ofArray short) (List.ofArray long)),
                     (fun (action : int -> int -> unit) -> Stack.iter2 action (Stack.ofArray short) (Stack.ofArray long))))
            (nameof Stack.map2,
             struct ((fun (action : int -> int -> unit) ->
                         List.map2 action (List.ofArray long) (List.ofArray short)
                         |> ignore
                     ),
                     (fun (action : int -> int -> unit) ->
                         Stack.map2 action (Stack.ofArray long) (Stack.ofArray short)
                         |> ignore
                     )))
            (nameof Stack.fold2,
             struct ((fun (action : int -> int -> unit) ->
                         List.fold2 (fun () a b -> action a b) () (List.ofArray short) (List.ofArray long)
                     ),
                     (fun (action : int -> int -> unit) ->
                         Stack.fold2 (fun () a b -> action a b) () (Stack.ofArray short) (Stack.ofArray long)
                     )))
            (nameof Stack.foldBack2,
             struct ((fun (action : int -> int -> unit) ->
                         List.foldBack2 (fun a b () -> action a b) (List.ofArray short) (List.ofArray long) ()
                     ),
                     (fun (action : int -> int -> unit) ->
                         Stack.foldBack2 (fun a b () -> action a b) (Stack.ofArray short) (Stack.ofArray long) ()
                     )))
        |]

    /// The functions whose exceptions List writes with more than one line or with the names of its lists, by name, with
    /// the List function on the same input. The names tell apart two inputs of the same function.
    static let completeMessages =
        let one = [| 1 |]
        let three = [| 1; 2; 3 |]
        let four = [| 1; 2; 3; 4 |]

        Map.ofArray [|
            (nameof Stack.zip3,
             struct ((fun () ->
                         List.zip3 (List.ofArray three) (List.ofArray one) (List.ofArray four)
                         |> ignore
                     ),
                     (fun () ->
                         Stack.zip3 (Stack.ofArray three) (Stack.ofArray one) (Stack.ofArray four)
                         |> ignore
                     )))
            (nameof Stack.map3,
             struct ((fun () ->
                         List.map3 (fun a b c -> a + b + c) (List.ofArray three) (List.ofArray one) (List.ofArray four)
                         |> ignore
                     ),
                     (fun () ->
                         Stack.map3 (fun a b c -> a + b + c) (Stack.ofArray three) (Stack.ofArray one) (Stack.ofArray four)
                         |> ignore
                     )))
            (nameof Stack.zip,
             struct ((fun () -> List.zip (List.ofArray four) (List.ofArray one) |> ignore),
                     (fun () -> Stack.zip (Stack.ofArray four) (Stack.ofArray one) |> ignore)))
            (nameof Stack.foldBack2,
             struct ((fun () ->
                         List.foldBack2 (fun _ _ state -> state) (List.ofArray one) (List.ofArray four) 0
                         |> ignore
                     ),
                     (fun () ->
                         Stack.foldBack2 (fun _ _ state -> state) (Stack.ofArray one) (Stack.ofArray four) 0
                         |> ignore
                     )))
            ($"%s{nameof Stack.foldBack2} of an empty first stack",
             struct ((fun () ->
                         List.foldBack2 (fun _ _ state -> state) List.empty<int> (List.ofArray four) 0
                         |> ignore
                     ),
                     (fun () ->
                         Stack.foldBack2 (fun _ _ state -> state) Stack.empty<int> (Stack.ofArray four) 0
                         |> ignore
                     )))
            (nameof Stack.exactlyOne,
             struct ((fun () -> List.exactlyOne (List.ofArray three) |> ignore),
                     (fun () -> Stack.exactlyOne (Stack.ofArray three) |> ignore)))
            (nameof Stack.head,
             struct ((fun () -> List.head List.empty<int> |> ignore), (fun () -> Stack.head Stack.empty<int> |> ignore)))
            (nameof Stack.take,
             struct ((fun () -> List.take 5 (List.ofArray three) |> ignore),
                     (fun () -> Stack.take 5 (Stack.ofArray three) |> ignore)))
            (nameof Stack.takeLast,
             struct ((fun () -> List.take 5 (List.ofArray three) |> ignore),
                     (fun () -> Stack.takeLast 5 (Stack.ofArray three) |> ignore)))
            (nameof Stack.skip,
             struct ((fun () -> List.skip 5 (List.ofArray three) |> ignore),
                     (fun () -> Stack.skip 5 (Stack.ofArray three) |> ignore)))
            (nameof Stack.skipLast,
             struct ((fun () -> List.skip 5 (List.ofArray three) |> ignore),
                     (fun () -> Stack.skipLast 5 (Stack.ofArray three) |> ignore)))
            (nameof Stack.splitAt,
             struct ((fun () -> List.splitAt 5 (List.ofArray three) |> ignore),
                     (fun () -> Stack.splitAt 5 (Stack.ofArray three) |> ignore)))
            (nameof Stack.init, struct ((fun () -> List.init -2 id |> ignore), (fun () -> Stack.init -2 id |> ignore)))
            (nameof Stack.windowed,
             struct ((fun () -> List.windowed 0 (List.ofArray three) |> ignore),
                     (fun () -> Stack.windowed 0 (Stack.ofArray three) |> ignore)))
            (nameof Stack.transpose,
             struct ((fun () ->
                         List.transpose [| List.ofArray four; List.ofArray three; List.ofArray four |]
                         |> ignore
                     ),
                     (fun () ->
                         Stack.transpose [| Stack.ofArray four; Stack.ofArray three; Stack.ofArray four |]
                         |> ignore
                     )))
            (nameof Stack.randomChoiceBy,
             struct ((fun () ->
                         List.randomChoiceBy (fun () -> 1.5) (List.ofArray three)
                         |> ignore
                     ),
                     (fun () ->
                         Stack.randomChoiceBy (fun () -> 1.5) (Stack.ofArray three)
                         |> ignore
                     )))
        |]

    /// The names of the creation functions, as the rows of their test.
    static member Creations = creations.Keys

    /// The names of the functions that remove the first elements, as the rows of their test.
    static member TopRemovals = topRemovals.Keys

    /// The names of the functions that keep every element, as the rows of their test.
    static member WholeStacks = wholeStacks.Keys

    /// The names of the sorting functions, as the rows of the test of one element.
    static member SingleElementSorts = singleElementSorts.Keys

    /// The names of the functions that take a stack, as the rows of their test.
    static member NullStackRejections = nullStackRejections.Keys

    /// The names of the functions that take another input that can be null, as the rows of their test.
    static member OtherNullRejections = otherNullRejections.Keys

    /// The functions that take a sequence and the inputs that they reject, as the rows of their test.
    static member SequenceRejections = sequenceRejections.Keys

    /// The names of the functions of two stacks, as the rows of the test of the common prefix.
    static member CommonPrefixWalks = commonPrefixWalks.Keys

    /// The names of the functions whose exceptions have complete messages, as the rows of the test of the messages.
    static member CompleteMessages = completeMessages.Keys

    // ---- Order ----

    [<TestMethod>]
    member _.``ofSeq puts the first element on top, unlike ImmutableStack.CreateRange`` () =
        let stack = Stack.ofSeq [| 1; 2; 3 |]

        Assert.AreEqual<int>(1, Stack.head stack, "Stack.ofSeq must put the first element on top")
        Assert.AreEqual<int>(1, stack.Peek (), "The top of the stack must be the first element of the sequence")
        Assert.AreEqual<int>(3, Stack.last stack, "Stack.ofSeq must put the last element at the bottom")

        Assert.AreEqual<int>(
            3,
            ImmutableStack.CreateRange([| 1; 2; 3 |]).Peek(),
            "ImmutableStack.CreateRange puts the last element on top, which Stack.ofSeq must not copy"
        )

    [<TestMethod; DynamicData(nameof Inputs.All, typeof<Inputs>)>]
    member _.``ofSeq and toList are inverse functions`` (input : int array) =
        let stack = Stack.ofSeq input

        CollectionAssert.AreEqual (input, List.toArray (Stack.toList stack), "Stack.toList must undo Stack.ofSeq")

        CollectionAssert.AreEqual (
            Stack.toArray stack,
            Stack.toArray (Stack.ofSeq (Stack.toList stack)),
            "Stack.ofSeq must undo Stack.toList"
        )

        CollectionAssert.AreEqual (input, Seq.toArray (Stack.toSeq stack), "Stack.toSeq must enumerate the elements from the top")
        CollectionAssert.AreEqual (input, Stack.toArray stack, "Stack.toArray must copy the elements from the top")

    [<TestMethod; DynamicData(nameof StackTests.Creations)>]
    member _.``every creation function puts the first element on top`` (name : string) =
        let stack = creations[name]()

        Assert.AreEqual<int>(1, Stack.head stack, $"Stack.%s{name} must put the first element on top")
        CollectionAssert.AreEqual ([| 1; 2; 3 |], Stack.toArray stack, $"Stack.%s{name} must keep the order of the elements")

    [<TestMethod>]
    member _.``head returns the top, the element that Peek returns`` () =
        let stack = several ()
        Assert.AreEqual<int>(stack.Peek (), Stack.head stack, "Stack.head must return the top of the stack")
        Assert.AreEqual<int>(Array.head Inputs.Several, Stack.head stack, "Stack.head must return the first element of ofArray")

    [<TestMethod>]
    member _.``cons pushes the value on top`` () =
        let stack = several ()
        let pushed = Stack.cons 99 stack

        CollectionAssert.AreEqual (
            Array.append [| 99 |] Inputs.Several,
            Stack.toArray pushed,
            "Stack.cons must add the value before the first element"
        )

        Assert.AreEqual<int>(99, pushed.Peek (), "Stack.cons must push the value on top")
        Assert.AreSame (stack, pushed.Pop (), "Stack.cons must push onto the input, whose nodes the result shares")

    [<TestMethod>]
    member _.``consMany pushes the values so that the first one ends on top`` () =
        let stack = Stack.ofArray [| 4; 5 |]
        let pushed = Stack.consMany [| 1; 2; 3 |] stack

        CollectionAssert.AreEqual (
            [| 1; 2; 3; 4; 5 |],
            Stack.toArray pushed,
            "Stack.consMany must add the values before the elements"
        )
        Assert.AreEqual<int>(1, Stack.head pushed, "Stack.consMany must leave the first value on top")
        Assert.AreSame (stack, Stack.skip 3 pushed, "Stack.consMany must push onto the input, whose nodes the result shares")

        CollectionAssert.AreEqual (
            Stack.toArray (Array.foldBack Stack.cons [| 1; 2; 3 |] stack),
            Stack.toArray pushed,
            "Stack.consMany must give what cons gives from the last value to the first one"
        )

        Assert.AreSame (stack, Stack.consMany [||] stack, "Stack.consMany of no values must return the input")

    [<TestMethod>]
    member _.``rev reverses the elements, so the last element ends on top`` () =
        let stack = Stack.ofArray [| 1; 2; 3 |]
        let reversed = Stack.rev stack

        CollectionAssert.AreEqual ([| 3; 2; 1 |], Stack.toArray reversed, "Stack.rev must reverse the elements")
        Assert.AreEqual<int>(Stack.last stack, Stack.head reversed, "Stack.rev must put the last element on top")
        CollectionAssert.AreEqual ([| 1; 2; 3 |], Stack.toArray (Stack.rev reversed), "Stack.rev must undo itself")

        let single = Stack.singleton 1
        Assert.AreSame (single, Stack.rev single, "Stack.rev of one element must return the input, as List.rev does")

    [<TestMethod>]
    member _.``append puts the elements of the first stack on top of the second one`` () =
        let first = Stack.ofArray [| 1; 2 |]
        let second = Stack.ofArray [| 3; 4 |]
        let appended = Stack.append first second

        CollectionAssert.AreEqual (
            [| 1; 2; 3; 4 |],
            Stack.toArray appended,
            "Stack.append must keep the first stack before the second one"
        )
        Assert.AreEqual<int>(1, Stack.head appended, "Stack.append must leave the top of the first stack on top")
        Assert.AreSame (second, Stack.skip 2 appended, "Stack.append must push onto the second stack, whose nodes it shares")

        Assert.AreSame (
            second,
            Stack.append Stack.empty second,
            "Stack.append of an empty first stack must return the second one"
        )

        Assert.AreSame (first, Stack.append first Stack.empty, "Stack.append of an empty second stack must return the first one")

    [<TestMethod>]
    member _.``uncons returns the top and the stack below it`` () =
        let stack = several ()
        let struct (head, tail) = Stack.uncons stack

        Assert.AreEqual<int>(Array.head Inputs.Several, head, "Stack.uncons must return the first element")
        CollectionAssert.AreEqual (Array.tail Inputs.Several, Stack.toArray tail, "Stack.uncons must return the other elements")
        Assert.AreSame (stack.Pop (), tail, "Stack.uncons must return the stack below the top, without a copy")

    [<TestMethod>]
    member _.``uncons throws for an empty stack as List.head does`` () =
        ExceptionAssert.Same (
            StackOps.paramName,
            (fun () -> List.head List.empty<int> |> ignore),
            (fun () -> Stack.uncons Stack.empty<int> |> ignore),
            "Stack.uncons of an empty stack must throw as List.head does"
        )

    [<TestMethod>]
    member _.``tryUncons returns the top and the stack below it or nothing`` () =
        let stack = several ()

        match Stack.tryUncons stack with
        | ValueSome (struct (head, tail)) ->
            Assert.AreEqual<int>(Array.head Inputs.Several, head, "Stack.tryUncons must return the first element")
            Assert.AreSame (stack.Pop (), tail, "Stack.tryUncons must return the stack below the top")
        | ValueNone -> Assert.Fail "Stack.tryUncons must return the elements of a stack with elements"

        Assert.IsTrue ((Stack.tryUncons Stack.empty<int>).IsNone, "Stack.tryUncons of an empty stack must return nothing")

    [<TestMethod>]
    member _.``Cons matches the top and the stack below it`` () =
        let rec elements (stack : Stack<int>) =
            match stack with
            | Stack.Cons (struct (head, tail)) -> Array.append [| head |] (elements tail)
            | _ -> [||]

        CollectionAssert.AreEqual (Inputs.Several, elements (several ()), "Stack.Cons must match the elements from the top")

        match Stack.empty<int> with
        | Stack.Cons _ -> Assert.Fail "Stack.Cons must not match an empty stack"
        | _ -> ()

    [<TestMethod>]
    member _.``Nil matches an empty stack only`` () =
        let describe (stack : Stack<int>) =
            match stack with
            | Stack.Nil -> "empty"
            | Stack.Cons (struct (head, _)) -> $"%d{head} on top"
            | _ -> "unreachable"

        Assert.AreEqual<string>("empty", describe Stack.empty, "Stack.Nil must match an empty stack")
        Assert.AreEqual<string>("5 on top", describe (several ()), "Stack.Nil must not match a stack with elements")

    [<TestMethod>]
    member _.``the indices count from the top`` () =
        let stack = Stack.ofArray [| 10; 20; 30 |]
        Assert.AreEqual<int>(0, Stack.findIndex ((=) 10) stack, "Stack.findIndex must give the top the index 0")
        Assert.AreEqual<int>(2, Stack.findIndexBack ((=) 30) stack, "Stack.findIndexBack must give the bottom the last index")

        CollectionAssert.AreEqual (
            [| struct (0, 10); struct (1, 20); struct (2, 30) |],
            Stack.toArray (Stack.indexed stack),
            "Stack.indexed must give the top the index 0"
        )

    [<TestMethod>]
    member _.``the functions call their callbacks from the top`` () =
        let stack = Stack.init 100 id
        let visited = ResizeArray<int>()

        Stack.map visited.Add stack |> ignore
        CollectionAssert.AreEqual (Array.init 100 id, visited.ToArray (), "Stack.map must call the mapping from the top")

        visited.Clear ()
        Stack.iter visited.Add stack
        CollectionAssert.AreEqual (Array.init 100 id, visited.ToArray (), "Stack.iter must call the action from the top")

        visited.Clear ()
        Stack.foldBack (fun item () -> visited.Add item) stack ()

        CollectionAssert.AreEqual (
            Array.init 100 (fun index -> 99 - index),
            visited.ToArray (),
            "Stack.foldBack must start at the bottom"
        )

    // ---- Shared nodes ----

    [<TestMethod; DynamicData(nameof StackTests.TopRemovals)>]
    member _.``the functions that remove the first elements return the stack below them`` (name : string) =
        let struct (remove, count) = topRemovals[name]
        let stack = several ()
        let mutable below = stack

        for _ in 1..count do
            below <- below.Pop ()

        Assert.AreSame (below, remove stack, $"Stack.%s{name} must return the stack below the removed elements, without a copy")

    [<TestMethod; DynamicData(nameof StackTests.WholeStacks)>]
    member _.``the functions return the input itself when they keep every element`` (name : string) =
        let stack = several ()
        Assert.AreSame (stack, wholeStacks[name] stack, $"Stack.%s{name} of every element must return the input")

    [<TestMethod; DynamicData(nameof StackTests.SingleElementSorts)>]
    member _.``sorts return a stack of one element itself, as List does`` (name : string) =
        let single = Stack.singleton 1
        Assert.AreSame (single, singleElementSorts[name] single, $"Stack.%s{name} of one element must return the input")

    [<TestMethod>]
    member _.``the sorts order strings in F# generic comparison, which is ordinal`` () =
        // A culture-sensitive comparer, such as Comparer<string>.Default, puts "a" before "B"
        let input = [| "b"; "A"; "a"; "B" |]
        let stack = Stack.ofArray input

        CollectionAssert.AreEqual (
            [| "A"; "B"; "a"; "b" |],
            Array.sort input,
            "F# generic comparison must order strings ordinally"
        )
        CollectionAssert.AreEqual (Array.sort input, Stack.toArray (Stack.sort stack), "Stack.sort must order as Array.sort does")

        CollectionAssert.AreEqual (
            Array.sortDescending input,
            Stack.toArray (Stack.sortDescending stack),
            "Stack.sortDescending must order as Array.sortDescending does"
        )

        CollectionAssert.AreEqual (
            Array.sortBy id input,
            Stack.toArray (Stack.sortBy id stack),
            "Stack.sortBy must order the keys as Array.sortBy does"
        )

        CollectionAssert.AreEqual (
            Array.sortByDescending id input,
            Stack.toArray (Stack.sortByDescending id stack),
            "Stack.sortByDescending must order the keys as Array.sortByDescending does"
        )

    // ---- Null inputs ----

    [<TestMethod; DynamicData(nameof StackTests.NullStackRejections)>]
    member _.``functions reject a null stack`` (name : string) =
        let struct (paramName, action) = nullStackRejections[name]

        let ex =
            Assert.ThrowsExactly<ArgumentNullException>(Action action, $"Stack.%s{name} must reject a null stack")

        Assert.AreEqual<string | null>(paramName, ex.ParamName, $"Stack.%s{name} must name the parameter of the null stack")

    [<TestMethod; DynamicData(nameof StackTests.OtherNullRejections)>]
    member _.``functions reject other null inputs with the names of their parameters`` (name : string) =
        let struct (paramName, action) = otherNullRejections[name]

        let ex =
            Assert.ThrowsExactly<ArgumentNullException>(Action action, $"Stack.%s{name} must reject a null input")

        Assert.AreEqual<string | null>(paramName, ex.ParamName, $"Stack.%s{name} must name the null parameter")

    [<TestMethod; DynamicData(nameof StackTests.SequenceRejections)>]
    member _.``functions that take a sequence reject a default flat list and check their inputs in order`` (name : string) =
        let struct (paramName, actual) = sequenceRejections[name]
        FlatListAssert.RejectsNull ($"Stack.%s{name}", paramName, actual)

    [<TestMethod>]
    member _.``a flat list that is not default passes as a sequence`` () =
        let flat = FlatList.ofArray [| 1; 2; 3 |] :> seq<int>
        let stacks =
            FlatList.ofArray [| Stack.ofArray [| 1; 2 |]; Stack.ofArray [| 3; 4 |] |] :> seq<Stack<int>>

        CollectionAssert.AreEqual (
            [| 1; 2; 3 |],
            Stack.toArray (Stack.ofSeq flat),
            "Stack.ofSeq must take the elements of a flat list"
        )

        CollectionAssert.AreEqual (
            [| 1; 2; 3; 9 |],
            Stack.toArray (Stack.consMany flat (Stack.ofArray [| 9 |])),
            "Stack.consMany must take the elements of a flat list"
        )

        CollectionAssert.AreEqual (
            [| 9 |],
            Stack.toArray (Stack.except flat (Stack.ofArray [| 2; 9; 3 |])),
            "Stack.except must take the elements of a flat list"
        )

        CollectionAssert.AreEqual (
            [| 1; 2; 3; 4 |],
            Stack.toArray (Stack.concat stacks),
            "Stack.concat must take the stacks of a flat list"
        )

        CollectionAssert.AreEqual (
            [| 1; 3 |],
            Stack.toArray (Stack.head (Stack.transpose stacks)),
            "Stack.transpose must take the stacks of a flat list"
        )

    [<TestMethod>]
    member _.``a flat list of strings that is not default passes as a sequence of objects`` () =
        let widened = FlatListOracles.stringsAsObjects (FlatList.ofArray [| "a"; "b" |])
        let asObjects (strings : string array) = strings |> Array.map (fun item -> item :> obj)

        CollectionAssert.AreEqual (
            asObjects [| "a"; "b" |],
            Stack.toArray (Stack.ofSeq widened),
            "Stack.ofSeq must take the elements of a flat list of strings as objects"
        )

        CollectionAssert.AreEqual (
            asObjects [| "a"; "b"; "c" |],
            Stack.toArray (Stack.consMany widened (Stack.ofArray (asObjects [| "c" |]))),
            "Stack.consMany must take the elements of a flat list of strings as objects"
        )

        CollectionAssert.AreEqual (
            asObjects [| "c" |],
            Stack.toArray (Stack.except widened (Stack.ofArray (asObjects [| "a"; "c"; "b" |]))),
            "Stack.except must take the elements of a flat list of strings as objects"
        )

    // ---- Semantics of List ----

    [<TestMethod>]
    member _.``contains compares in F# structural equality`` () =
        let arrays = Stack.singleton [| 1; 2 |]
        Assert.IsTrue (
            Stack.contains [| 1; 2 |] arrays,
            "Stack.contains must compare arrays by their elements, as List.contains does"
        )

        let nan = Stack.singleton Double.NaN
        Assert.AreEqual<bool>(
            List.contains Double.NaN [ Double.NaN ],
            Stack.contains Double.NaN nan,
            "Stack.contains must compare NaN as List.contains does"
        )

    [<TestMethod; DynamicData(nameof StackTests.CommonPrefixWalks)>]
    member _.``the functions of two stacks walk the common prefix before they throw, as List does`` (name : string) =
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
            $"Stack.%s{name} must call its function for the pairs that List calls it for"
        )

    [<TestMethod>]
    member _.``exists2 and forall2 decide before they compare the lengths, as List does`` () =
        let short = [| 1; 2 |]
        let long = [| 1; 20; 30 |]

        Assert.AreEqual<bool>(
            List.exists2 (=) (List.ofArray short) (List.ofArray long),
            Stack.exists2 (=) (Stack.ofArray short) (Stack.ofArray long),
            "Stack.exists2 must return true for a match before the end of the shorter stack"
        )

        Assert.AreEqual<bool>(
            List.forall2 (=) (List.ofArray short) (List.ofArray long),
            Stack.forall2 (=) (Stack.ofArray short) (Stack.ofArray long),
            "Stack.forall2 must return false for a mismatch before the end of the shorter stack"
        )

    [<TestMethod>]
    member _.``except does not read the items to exclude for an empty stack, as List does`` () =
        let items = seq { failwith "The items to exclude must not be read" }
        Assert.IsEmpty (Stack.except items Stack.empty<int>, "Stack.except of an empty stack must be empty")

    [<TestMethod>]
    member _.``transpose names the stack that ends first as List does`` () =
        ExceptionAssert.Same (
            StackOps.paramName,
            (fun () -> List.transpose [| [ 1 ]; [ 1; 2 ] |] |> ignore),
            (fun () ->
                Stack.transpose [| Stack.ofArray [| 1 |]; Stack.ofArray [| 1; 2 |] |]
                |> ignore
            ),
            "Stack.transpose of a shorter first stack must throw as List.transpose does"
        )

    [<TestMethod>]
    member _.``splitAt of an empty stack throws as List.splitAt does`` () =
        ExceptionAssert.Same (
            StackOps.paramName,
            (fun () -> List.splitAt 1 List.empty<int> |> ignore),
            (fun () -> Stack.splitAt 1 Stack.empty<int> |> ignore),
            "Stack.splitAt 1 of an empty stack must throw as List.splitAt does"
        )

    [<TestMethod; DynamicData(nameof StackTests.CompleteMessages)>]
    member _.``the exceptions have the complete messages of List with the names of the stacks`` (name : string) =
        let struct (oracle, actual) = completeMessages[name]
        let expected = thrown oracle $"List.%s{name} must throw"
        let ex = thrown actual $"Stack.%s{name} must throw"
        Assert.AreEqual<Type>(expected.GetType (), ex.GetType (), $"Stack.%s{name} must throw the type of exception of List")

        Assert.AreEqual<string>(
            stackNames expected.Message,
            ex.Message,
            $"Stack.%s{name} must throw the message of List with the names of the stacks"
        )

    [<TestMethod>]
    [<DataRow(nameof Stack.cons, "Push")>]
    [<DataRow(nameof Stack.consMany, "PushMany")>]
    [<DataRow(nameof Stack.head, "Peek")>]
    [<DataRow(nameof Stack.tail, "Pop")>]
    [<DataRow(nameof Stack.tryHead, "TryPeek")>]
    [<DataRow(nameof Stack.tryTail, "TryPop")>]
    [<DataRow(nameof Stack.tryUncons, "TryPeekPop")>]
    [<DataRow(nameof Stack.uncons, "PeekPop")>]
    member _.``the stack functions compile with the names of the stack operations of the BCL``
        (name : string, compiledName : string)
        =
        Assert.AreEqual<string | null>(
            name,
            StackOps.sourceName "FSharp.Collections.Immutable.Stack" compiledName,
            "Stack must compile the function under the name of its stack operation in the BCL"
        )
