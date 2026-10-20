namespace FSharp.Collections.Immutable.Tests

open System
open System.Collections.Generic
open System.Globalization
open FSharp.Collections.Immutable
open Microsoft.VisualStudio.TestTools.UnitTesting

/// The behaviour of `IndexedList` that the shared contracts do not cover: null inputs, shared trees, the extras and the
/// details of the semantics of the List module.
[<TestClass; IndexedListTestCategory; PatternMatchingTestCategory>]
type IndexedListTests () =

    static let nullList = Unchecked.defaultof<IndexedList<int>>

    static let several () = IndexedList.ofArray Inputs.Several

    /// The exception of an action and its complete message, under the invariant culture, in which FSharp.Core writes the
    /// English messages that this library copies.
    static let thrown (action : unit -> unit) (message : string) =
        let culture = CultureInfo.CurrentUICulture
        CultureInfo.CurrentUICulture <- CultureInfo.InvariantCulture

        try
            Assert.Throws<exn>(Action action, message)
        finally
            CultureInfo.CurrentUICulture <- culture

    /// The functions that take an indexed list, by name, with the parameter that names the null indexed list and the
    /// function on a null indexed list. The rows of the test are the names: a function has no readable name in a row.
    static let nullListRejections =
        let pairs = Unchecked.defaultof<IndexedList<struct (int * int)>>
        let triples = Unchecked.defaultof<IndexedList<struct (int * int * int)>>
        let floats = Unchecked.defaultof<IndexedList<float>>

        [|
            struct (nameof IndexedList.toBuilder, "list", (fun () -> IndexedList.toBuilder nullList |> ignore))
            struct (nameof IndexedList.update, "list", (fun () -> IndexedList.update ignore nullList |> ignore))
            struct (nameof IndexedList.isEmpty, "list", (fun () -> IndexedList.isEmpty nullList |> ignore))
            struct (nameof IndexedList.length, "list", (fun () -> IndexedList.length nullList |> ignore))
            struct (nameof IndexedList.exactlyOne, "list", (fun () -> IndexedList.exactlyOne nullList |> ignore))
            struct (nameof IndexedList.head, "list", (fun () -> IndexedList.head nullList |> ignore))
            struct (nameof IndexedList.item, "list", (fun () -> IndexedList.item 0 nullList |> ignore))
            struct (nameof IndexedList.last, "list", (fun () -> IndexedList.last nullList |> ignore))
            struct (nameof IndexedList.tail, "list", (fun () -> IndexedList.tail nullList |> ignore))
            struct (nameof IndexedList.tryExactlyOne, "list", (fun () -> IndexedList.tryExactlyOne nullList |> ignore))
            struct (nameof IndexedList.tryHead, "list", (fun () -> IndexedList.tryHead nullList |> ignore))
            struct (nameof IndexedList.tryItem, "list", (fun () -> IndexedList.tryItem 0 nullList |> ignore))
            struct (nameof IndexedList.tryLast, "list", (fun () -> IndexedList.tryLast nullList |> ignore))
            struct (nameof IndexedList.tryTail, "list", (fun () -> IndexedList.tryTail nullList |> ignore))
            struct (nameof IndexedList.tryUncons, "list", (fun () -> IndexedList.tryUncons nullList |> ignore))
            struct (nameof IndexedList.uncons, "list", (fun () -> IndexedList.uncons nullList |> ignore))
            struct (nameof IndexedList.(|Cons|_|), "list", (fun () -> IndexedList.(|Cons|_|) nullList |> ignore))
            struct (nameof IndexedList.(|Nil|_|), "list", (fun () -> IndexedList.(|Nil|_|) nullList |> ignore))
            struct (nameof IndexedList.binarySearch, "list", (fun () -> IndexedList.binarySearch 1 nullList |> ignore))
            struct (nameof IndexedList.binarySearchRange,
                    "list",
                    (fun () -> IndexedList.binarySearchRange 0 0 1 nullList |> ignore))
            struct (nameof IndexedList.binarySearchRangeWithComparer,
                    "list",
                    (fun () ->
                        IndexedList.binarySearchRangeWithComparer Comparer<int>.Default 0 0 1 nullList
                        |> ignore
                    ))
            struct (nameof IndexedList.binarySearchWithComparer,
                    "list",
                    (fun () ->
                        IndexedList.binarySearchWithComparer Comparer<int>.Default 1 nullList
                        |> ignore
                    ))
            struct (nameof IndexedList.contains, "source", (fun () -> IndexedList.contains 1 nullList |> ignore))
            struct (nameof IndexedList.exists, "list", (fun () -> IndexedList.exists ((=) 1) nullList |> ignore))
            struct (nameof IndexedList.exists2, "list2", (fun () -> IndexedList.exists2 (=) (several ()) nullList |> ignore))
            struct (nameof IndexedList.find, "list", (fun () -> IndexedList.find ((=) 1) nullList |> ignore))
            struct (nameof IndexedList.findBack, "list", (fun () -> IndexedList.findBack ((=) 1) nullList |> ignore))
            struct (nameof IndexedList.findIndex, "list", (fun () -> IndexedList.findIndex ((=) 1) nullList |> ignore))
            struct (nameof IndexedList.findIndexBack, "list", (fun () -> IndexedList.findIndexBack ((=) 1) nullList |> ignore))
            struct (nameof IndexedList.forall, "list", (fun () -> IndexedList.forall ((=) 1) nullList |> ignore))
            struct (nameof IndexedList.forall2, "list1", (fun () -> IndexedList.forall2 (=) nullList (several ()) |> ignore))
            struct (nameof IndexedList.indexOf, "list", (fun () -> IndexedList.indexOf 1 nullList |> ignore))
            struct (nameof IndexedList.indexOfRange, "list", (fun () -> IndexedList.indexOfRange 0 0 1 nullList |> ignore))
            struct (nameof IndexedList.indexOfRangeWith,
                    "list",
                    (fun () ->
                        IndexedList.indexOfRangeWith EqualityComparer<int>.Default 0 0 1 nullList
                        |> ignore
                    ))
            struct (nameof IndexedList.indexOfWith,
                    "list",
                    (fun () ->
                        IndexedList.indexOfWith EqualityComparer<int>.Default 1 nullList
                        |> ignore
                    ))
            struct (nameof IndexedList.lastIndexOf, "list", (fun () -> IndexedList.lastIndexOf 1 nullList |> ignore))
            struct (nameof IndexedList.lastIndexOfRange, "list", (fun () -> IndexedList.lastIndexOfRange 0 0 1 nullList |> ignore))
            struct (nameof IndexedList.lastIndexOfRangeWith,
                    "list",
                    (fun () ->
                        IndexedList.lastIndexOfRangeWith EqualityComparer<int>.Default 0 0 1 nullList
                        |> ignore
                    ))
            struct (nameof IndexedList.lastIndexOfWith,
                    "list",
                    (fun () ->
                        IndexedList.lastIndexOfWith EqualityComparer<int>.Default 1 nullList
                        |> ignore
                    ))
            struct (nameof IndexedList.pick, "list", (fun () -> IndexedList.pick ValueSome nullList |> ignore))
            struct (nameof IndexedList.tryFind, "list", (fun () -> IndexedList.tryFind ((=) 1) nullList |> ignore))
            struct (nameof IndexedList.tryFindBack, "list", (fun () -> IndexedList.tryFindBack ((=) 1) nullList |> ignore))
            struct (nameof IndexedList.tryFindIndex, "list", (fun () -> IndexedList.tryFindIndex ((=) 1) nullList |> ignore))
            struct (nameof IndexedList.tryFindIndexBack,
                    "list",
                    (fun () -> IndexedList.tryFindIndexBack ((=) 1) nullList |> ignore))
            struct (nameof IndexedList.tryFindIndexBackRange,
                    "list",
                    (fun () ->
                        IndexedList.tryFindIndexBackRange 0 0 ((=) 1) nullList
                        |> ignore
                    ))
            struct (nameof IndexedList.tryFindIndexRange,
                    "list",
                    (fun () -> IndexedList.tryFindIndexRange 0 0 ((=) 1) nullList |> ignore))
            struct (nameof IndexedList.tryPick, "list", (fun () -> IndexedList.tryPick ValueSome nullList |> ignore))
            struct (nameof IndexedList.add, "list", (fun () -> IndexedList.add 1 nullList |> ignore))
            struct (nameof IndexedList.allPairs, "list1", (fun () -> IndexedList.allPairs nullList (several ()) |> ignore))
            struct (nameof IndexedList.append, "list2", (fun () -> IndexedList.append (several ()) nullList |> ignore))
            struct (nameof IndexedList.choose, "list", (fun () -> IndexedList.choose ValueSome nullList |> ignore))
            struct (nameof IndexedList.chunkBySize, "list", (fun () -> IndexedList.chunkBySize 2 nullList |> ignore))
            struct (nameof IndexedList.collect, "list", (fun () -> IndexedList.collect IndexedList.singleton nullList |> ignore))
            struct (nameof IndexedList.concat, "lists", (fun () -> IndexedList.concat [| several (); nullList |] |> ignore))
            struct (nameof IndexedList.cons, "list", (fun () -> IndexedList.cons 1 nullList |> ignore))
            struct (nameof IndexedList.filter, "list", (fun () -> IndexedList.filter ((=) 1) nullList |> ignore))
            struct (nameof IndexedList.indexed, "list", (fun () -> IndexedList.indexed nullList |> ignore))
            struct (nameof IndexedList.insertAt, "source", (fun () -> IndexedList.insertAt 0 1 nullList |> ignore))
            struct (nameof IndexedList.insertManyAt, "source", (fun () -> IndexedList.insertManyAt 0 [| 1 |] nullList |> ignore))
            struct (nameof IndexedList.map, "list", (fun () -> IndexedList.map id nullList |> ignore))
            struct (nameof IndexedList.map2, "list2", (fun () -> IndexedList.map2 (+) (several ()) nullList |> ignore))
            struct (nameof IndexedList.map3,
                    "list3",
                    (fun () ->
                        IndexedList.map3 (fun a b c -> a + b + c) (several ()) (several ()) nullList
                        |> ignore
                    ))
            struct (nameof IndexedList.mapi, "list", (fun () -> IndexedList.mapi (+) nullList |> ignore))
            struct (nameof IndexedList.mapi2,
                    "list1",
                    (fun () ->
                        IndexedList.mapi2 (fun index a b -> index + a + b) nullList (several ())
                        |> ignore
                    ))
            struct (nameof IndexedList.pairwise, "list", (fun () -> IndexedList.pairwise nullList |> ignore))
            struct (nameof IndexedList.permute, "list", (fun () -> IndexedList.permute id nullList |> ignore))
            struct (nameof IndexedList.remove, "list", (fun () -> IndexedList.remove 1 nullList |> ignore))
            struct (nameof IndexedList.removeAt, "source", (fun () -> IndexedList.removeAt 0 nullList |> ignore))
            struct (nameof IndexedList.removeMany, "list", (fun () -> IndexedList.removeMany [| 1 |] nullList |> ignore))
            struct (nameof IndexedList.removeManyAt, "source", (fun () -> IndexedList.removeManyAt 0 1 nullList |> ignore))
            struct (nameof IndexedList.removeManyWith,
                    "list",
                    (fun () ->
                        IndexedList.removeManyWith EqualityComparer<int>.Default [| 1 |] nullList
                        |> ignore
                    ))
            struct (nameof IndexedList.removeWith,
                    "list",
                    (fun () ->
                        IndexedList.removeWith EqualityComparer<int>.Default 1 nullList
                        |> ignore
                    ))
            struct (nameof IndexedList.replace, "list", (fun () -> IndexedList.replace 1 2 nullList |> ignore))
            struct (nameof IndexedList.replaceWith,
                    "list",
                    (fun () ->
                        IndexedList.replaceWith EqualityComparer<int>.Default 1 2 nullList
                        |> ignore
                    ))
            struct (nameof IndexedList.rev, "list", (fun () -> IndexedList.rev nullList |> ignore))
            struct (nameof IndexedList.revRange, "list", (fun () -> IndexedList.revRange 0 0 nullList |> ignore))
            struct (nameof IndexedList.transpose, "lists", (fun () -> IndexedList.transpose [| nullList |] |> ignore))
            struct (nameof IndexedList.unzip, "list", (fun () -> IndexedList.unzip pairs |> ignore))
            struct (nameof IndexedList.unzip3, "list", (fun () -> IndexedList.unzip3 triples |> ignore))
            struct (nameof IndexedList.updateAt, "source", (fun () -> IndexedList.updateAt 0 1 nullList |> ignore))
            struct (nameof IndexedList.where, "list", (fun () -> IndexedList.where ((=) 1) nullList |> ignore))
            struct (nameof IndexedList.windowed, "list", (fun () -> IndexedList.windowed 2 nullList |> ignore))
            struct (nameof IndexedList.zip, "list1", (fun () -> IndexedList.zip nullList (several ()) |> ignore))
            struct (nameof IndexedList.zip3,
                    "list2",
                    (fun () ->
                        IndexedList.zip3 (several ()) nullList (several ())
                        |> ignore
                    ))
            struct (nameof IndexedList.average, "list", (fun () -> IndexedList.average floats |> ignore))
            struct (nameof IndexedList.averageBy, "list", (fun () -> IndexedList.averageBy float nullList |> ignore))
            struct (nameof IndexedList.compareWith,
                    "list2",
                    (fun () ->
                        IndexedList.compareWith compare (several ()) nullList
                        |> ignore
                    ))
            struct (nameof IndexedList.fold, "list", (fun () -> IndexedList.fold (+) 0 nullList |> ignore))
            struct (nameof IndexedList.fold2,
                    "list2",
                    (fun () ->
                        IndexedList.fold2 (fun state a b -> state + a + b) 0 (several ()) nullList
                        |> ignore
                    ))
            struct (nameof IndexedList.foldBack, "list", (fun () -> IndexedList.foldBack (+) nullList 0 |> ignore))
            struct (nameof IndexedList.foldBack2,
                    "list1",
                    (fun () ->
                        IndexedList.foldBack2 (fun a b state -> state + a + b) nullList (several ()) 0
                        |> ignore
                    ))
            struct (nameof IndexedList.iter, "list", (fun () -> IndexedList.iter ignore nullList))
            struct (nameof IndexedList.iter2, "list2", (fun () -> IndexedList.iter2 (fun _ _ -> ()) (several ()) nullList))
            struct (nameof IndexedList.iteri, "list", (fun () -> IndexedList.iteri (fun _ _ -> ()) nullList))
            struct (nameof IndexedList.iteri2, "list1", (fun () -> IndexedList.iteri2 (fun _ _ _ -> ()) nullList (several ())))
            struct (nameof IndexedList.mapFold,
                    "list",
                    (fun () ->
                        IndexedList.mapFold (fun state item -> struct (item, state)) 0 nullList
                        |> ignore
                    ))
            struct (nameof IndexedList.mapFoldBack,
                    "list",
                    (fun () ->
                        IndexedList.mapFoldBack (fun item state -> struct (item, state)) nullList 0
                        |> ignore
                    ))
            struct (nameof IndexedList.max, "list", (fun () -> IndexedList.max nullList |> ignore))
            struct (nameof IndexedList.maxBy, "list", (fun () -> IndexedList.maxBy id nullList |> ignore))
            struct (nameof IndexedList.min, "list", (fun () -> IndexedList.min nullList |> ignore))
            struct (nameof IndexedList.minBy, "list", (fun () -> IndexedList.minBy id nullList |> ignore))
            struct (nameof IndexedList.reduce, "list", (fun () -> IndexedList.reduce (+) nullList |> ignore))
            struct (nameof IndexedList.reduceBack, "list", (fun () -> IndexedList.reduceBack (+) nullList |> ignore))
            struct (nameof IndexedList.scan, "list", (fun () -> IndexedList.scan (+) 0 nullList |> ignore))
            struct (nameof IndexedList.scanBack, "list", (fun () -> IndexedList.scanBack (+) nullList 0 |> ignore))
            struct (nameof IndexedList.sum, "list", (fun () -> IndexedList.sum nullList |> ignore))
            struct (nameof IndexedList.sumBy, "list", (fun () -> IndexedList.sumBy id nullList |> ignore))
            struct (nameof IndexedList.countBy, "list", (fun () -> IndexedList.countBy id nullList |> ignore))
            struct (nameof IndexedList.distinct, "list", (fun () -> IndexedList.distinct nullList |> ignore))
            struct (nameof IndexedList.distinctBy, "list", (fun () -> IndexedList.distinctBy id nullList |> ignore))
            struct (nameof IndexedList.except, "list", (fun () -> IndexedList.except [| 1 |] nullList |> ignore))
            struct (nameof IndexedList.groupBy, "list", (fun () -> IndexedList.groupBy id nullList |> ignore))
            struct (nameof IndexedList.partition, "list", (fun () -> IndexedList.partition ((=) 1) nullList |> ignore))
            struct (nameof IndexedList.partitionWith, "list", (fun () -> IndexedList.partitionWith Choice1Of2 nullList |> ignore))
            struct (nameof IndexedList.skip, "list", (fun () -> IndexedList.skip 0 nullList |> ignore))
            struct (nameof IndexedList.skipLast, "list", (fun () -> IndexedList.skipLast 0 nullList |> ignore))
            struct (nameof IndexedList.skipWhile, "list", (fun () -> IndexedList.skipWhile ((=) 1) nullList |> ignore))
            struct (nameof IndexedList.splitAt, "list", (fun () -> IndexedList.splitAt 0 nullList |> ignore))
            struct (nameof IndexedList.splitInto, "list", (fun () -> IndexedList.splitInto 2 nullList |> ignore))
            struct (nameof IndexedList.sub, "list", (fun () -> IndexedList.sub nullList 0 0 |> ignore))
            struct (nameof IndexedList.take, "list", (fun () -> IndexedList.take 0 nullList |> ignore))
            struct (nameof IndexedList.takeLast, "list", (fun () -> IndexedList.takeLast 0 nullList |> ignore))
            struct (nameof IndexedList.takeWhile, "list", (fun () -> IndexedList.takeWhile ((=) 1) nullList |> ignore))
            struct (nameof IndexedList.truncate, "list", (fun () -> IndexedList.truncate 1 nullList |> ignore))
            struct (nameof IndexedList.sort, "list", (fun () -> IndexedList.sort nullList |> ignore))
            struct (nameof IndexedList.sortBy, "list", (fun () -> IndexedList.sortBy id nullList |> ignore))
            struct (nameof IndexedList.sortByDescending, "list", (fun () -> IndexedList.sortByDescending id nullList |> ignore))
            struct (nameof IndexedList.sortDescending, "list", (fun () -> IndexedList.sortDescending nullList |> ignore))
            struct (nameof IndexedList.sortRange, "list", (fun () -> IndexedList.sortRange 0 0 nullList |> ignore))
            struct (nameof IndexedList.sortRangeWith, "list", (fun () -> IndexedList.sortRangeWith compare 0 0 nullList |> ignore))
            struct (nameof IndexedList.sortRangeWithComparer,
                    "list",
                    (fun () ->
                        IndexedList.sortRangeWithComparer Comparer<int>.Default 0 0 nullList
                        |> ignore
                    ))
            struct (nameof IndexedList.sortWith, "list", (fun () -> IndexedList.sortWith compare nullList |> ignore))
            struct (nameof IndexedList.sortWithComparer,
                    "list",
                    (fun () ->
                        IndexedList.sortWithComparer Comparer<int>.Default nullList
                        |> ignore
                    ))
            struct (nameof IndexedList.randomChoice, "source", (fun () -> IndexedList.randomChoice nullList |> ignore))
            struct (nameof IndexedList.randomChoiceBy,
                    "source",
                    (fun () ->
                        IndexedList.randomChoiceBy (fun () -> 0.5) nullList
                        |> ignore
                    ))
            struct (nameof IndexedList.randomChoiceWith,
                    "source",
                    (fun () -> IndexedList.randomChoiceWith (Random 1) nullList |> ignore))
            struct (nameof IndexedList.randomChoices, "source", (fun () -> IndexedList.randomChoices 1 nullList |> ignore))
            struct (nameof IndexedList.randomChoicesBy,
                    "source",
                    (fun () ->
                        IndexedList.randomChoicesBy (fun () -> 0.5) 1 nullList
                        |> ignore
                    ))
            struct (nameof IndexedList.randomChoicesWith,
                    "source",
                    (fun () ->
                        IndexedList.randomChoicesWith (Random 1) 1 nullList
                        |> ignore
                    ))
            struct (nameof IndexedList.randomSample, "source", (fun () -> IndexedList.randomSample 1 nullList |> ignore))
            struct (nameof IndexedList.randomSampleBy,
                    "source",
                    (fun () ->
                        IndexedList.randomSampleBy (fun () -> 0.5) 1 nullList
                        |> ignore
                    ))
            struct (nameof IndexedList.randomSampleWith,
                    "source",
                    (fun () -> IndexedList.randomSampleWith (Random 1) 1 nullList |> ignore))
            struct (nameof IndexedList.randomShuffle, "source", (fun () -> IndexedList.randomShuffle nullList |> ignore))
            struct (nameof IndexedList.randomShuffleBy,
                    "source",
                    (fun () ->
                        IndexedList.randomShuffleBy (fun () -> 0.5) nullList
                        |> ignore
                    ))
            struct (nameof IndexedList.randomShuffleWith,
                    "source",
                    (fun () -> IndexedList.randomShuffleWith (Random 1) nullList |> ignore))
            struct (nameof IndexedList.blit, "source", (fun () -> IndexedList.blit nullList 0 [| 0 |] 0 0))
            struct (nameof IndexedList.toArray, "list", (fun () -> IndexedList.toArray nullList |> ignore))
            struct (nameof IndexedList.toList, "list", (fun () -> IndexedList.toList nullList |> ignore))
            struct (nameof IndexedList.toSeq, "list", (fun () -> IndexedList.toSeq nullList |> ignore))
        |]
        |> Array.map (fun struct (name, paramName, action) -> name, struct (paramName, action))
        |> Map.ofArray

    /// The functions that take another input that can be null, by name, with the name of that parameter and the function
    /// on a null input.
    static let otherNullRejections =
        [|
            struct (nameof IndexedList.ofArray,
                    "array",
                    (fun () ->
                        IndexedList.ofArray (Unchecked.defaultof<int array>)
                        |> ignore
                    ))
            struct (nameof IndexedList.ofSeq, "source", (fun () -> IndexedList.ofSeq (Unchecked.defaultof<seq<int>>) |> ignore))
            struct (nameof IndexedList.ofBuilder,
                    "builder",
                    (fun () ->
                        IndexedList.ofBuilder (Unchecked.defaultof<IndexedList<int>.Builder>)
                        |> ignore
                    ))
            struct (nameof IndexedList.concat,
                    "lists",
                    (fun () ->
                        IndexedList.concat (Unchecked.defaultof<seq<IndexedList<int>>>)
                        |> ignore
                    ))
            struct (nameof IndexedList.transpose,
                    "lists",
                    (fun () ->
                        IndexedList.transpose (Unchecked.defaultof<seq<IndexedList<int>>>)
                        |> ignore
                    ))
            struct (nameof IndexedList.insertManyAt,
                    "values",
                    (fun () ->
                        IndexedList.insertManyAt 0 (Unchecked.defaultof<seq<int>>) (several ())
                        |> ignore
                    ))
            struct (nameof IndexedList.blit,
                    "target",
                    (fun () -> IndexedList.blit (several ()) 0 (Unchecked.defaultof<int array>) 0 0))
            struct (nameof IndexedList.removeMany,
                    "items",
                    (fun () ->
                        IndexedList.removeMany (Unchecked.defaultof<seq<int>>) (several ())
                        |> ignore
                    ))
            struct (nameof IndexedList.removeManyWith,
                    "items",
                    (fun () ->
                        IndexedList.removeManyWith EqualityComparer<int>.Default (Unchecked.defaultof<seq<int>>) (several ())
                        |> ignore
                    ))
        |]
        |> Array.map (fun struct (name, paramName, action) -> name, struct (paramName, action))
        |> Map.ofArray

    /// The functions that take a sequence, by name and by the input that they reject, with the parameter that names the
    /// input and the function on it. The shared contracts cover a null sequence and a default flat list given as a
    /// sequence of integers; the rows are a default flat list of strings given as a sequence of objects, which a type test
    /// for the flat list of objects does not see, and the order of the checks. A default flat list converts to a sequence
    /// without a check and fails when the sequence is enumerated, so the other inputs are valid.
    static let sequenceRejections : Map<string, struct (string * (unit -> unit))> =
        // The flat list of strings is a sequence of objects, which a type test for the flat list of objects does not see
        let widenedItems = FlatListOracles.defaultStringsAsObjects
        let objects = IndexedList.ofArray [| "a" :> obj |]
        let defaultItems = Inputs.DefaultFlatListAsSeq<int>()
        let nullIndexedList = Unchecked.defaultof<IndexedList<int>>

        Map.ofArray [|
            "ofSeq (default flat list of strings as objects)",
            struct ("source", (fun () -> IndexedList.ofSeq widenedItems |> ignore))
            "insertManyAt (default flat list of strings as objects)",
            struct ("values", (fun () -> IndexedList.insertManyAt 0 widenedItems objects |> ignore))
            // List.insertManyAt checks the source before the values
            "insertManyAt (null indexed list and default flat list)",
            struct ("source",
                    (fun () ->
                        IndexedList.insertManyAt 0 defaultItems nullIndexedList
                        |> ignore
                    ))
            "except (default flat list of strings as objects)",
            struct ("itemsToExclude", (fun () -> IndexedList.except widenedItems objects |> ignore))
            // List.except checks the items to exclude before the list
            "except (default flat list and null indexed list)",
            struct ("itemsToExclude", (fun () -> IndexedList.except defaultItems nullIndexedList |> ignore))
            "removeMany (default flat list of strings as objects)",
            struct ("items", (fun () -> IndexedList.removeMany widenedItems objects |> ignore))
            "removeManyWith (default flat list of strings as objects)",
            struct ("items",
                    (fun () ->
                        IndexedList.removeManyWith EqualityComparer<obj>.Default widenedItems objects
                        |> ignore
                    ))
        |]

    /// The functions that take a range, by name, on the whole range of the input with several elements.
    static let wholeRanges : Map<string, IndexedList<int> -> IndexedList<int>> =
        Map.ofArray [|
            nameof IndexedList.take, IndexedList.take 8
            nameof IndexedList.takeLast, IndexedList.takeLast 8
            nameof IndexedList.takeWhile, IndexedList.takeWhile (fun _ -> true)
            nameof IndexedList.truncate, IndexedList.truncate 100
            nameof IndexedList.skip, IndexedList.skip -1
            nameof IndexedList.skipLast, IndexedList.skipLast 0
            nameof IndexedList.skipWhile, IndexedList.skipWhile (fun _ -> false)
            nameof IndexedList.removeManyAt, IndexedList.removeManyAt 3 0
            nameof IndexedList.sub, (fun list -> IndexedList.sub list 0 list.Count)
        |]

    /// The sorting functions, by name, with a projection or a comparison function that must not run on one element.
    static let singleElementSorts : Map<string, IndexedList<int> -> IndexedList<int>> =
        Map.ofArray [|
            nameof IndexedList.sort, IndexedList.sort
            nameof IndexedList.sortBy, IndexedList.sortBy (fun _ -> failwith<int> "The projection must not run")
            (nameof IndexedList.sortByDescending,
             IndexedList.sortByDescending (fun _ -> failwith<int> "The projection must not run"))
            nameof IndexedList.sortDescending, IndexedList.sortDescending
            nameof IndexedList.sortWith, IndexedList.sortWith (fun _ _ -> failwith "The comparer must not run")
        |]

    /// Strings whose ordinal order differs from their order in the current culture.
    static let strings () = [| "b"; "A"; "a"; "B" |]


    /// The functions of two lists, by name, with the List function that they mirror, as functions of the action that
    /// records the pairs; both run on a shorter and a longer list.
    static let commonPrefixWalks =
        let short = [| 1; 2 |]
        let long = [| 10; 20; 30 |]

        Map.ofArray [|
            (nameof IndexedList.iter2,
             struct ((fun (action : int -> int -> unit) -> List.iter2 action (List.ofArray short) (List.ofArray long)),
                     (fun (action : int -> int -> unit) ->
                         IndexedList.iter2 action (IndexedList.ofArray short) (IndexedList.ofArray long)
                     )))
            (nameof IndexedList.map2,
             struct ((fun (action : int -> int -> unit) ->
                         List.map2 action (List.ofArray long) (List.ofArray short)
                         |> ignore
                     ),
                     (fun (action : int -> int -> unit) ->
                         IndexedList.map2 action (IndexedList.ofArray long) (IndexedList.ofArray short)
                         |> ignore
                     )))
            (nameof IndexedList.fold2,
             struct ((fun (action : int -> int -> unit) ->
                         List.fold2 (fun () a b -> action a b) () (List.ofArray short) (List.ofArray long)
                     ),
                     (fun (action : int -> int -> unit) ->
                         IndexedList.fold2 (fun () a b -> action a b) () (IndexedList.ofArray short) (IndexedList.ofArray long)
                     )))
            (nameof IndexedList.foldBack2,
             struct ((fun (action : int -> int -> unit) ->
                         List.foldBack2 (fun a b () -> action a b) (List.ofArray short) (List.ofArray long) ()
                     ),
                     (fun (action : int -> int -> unit) ->
                         IndexedList.foldBack2
                             (fun a b () -> action a b)
                             (IndexedList.ofArray short)
                             (IndexedList.ofArray long)
                             ()
                     )))
        |]

    /// The functions whose exceptions List writes with more than one line, by name, with the List function on the same
    /// input. The names tell apart two inputs of the same function.
    static let completeMessages =
        let one = [| 1 |]
        let three = [| 1; 2; 3 |]
        let four = [| 1; 2; 3; 4 |]

        Map.ofArray [|
            (nameof IndexedList.zip3,
             struct ((fun () ->
                         List.zip3 (List.ofArray three) (List.ofArray one) (List.ofArray four)
                         |> ignore
                     ),
                     (fun () ->
                         IndexedList.zip3 (IndexedList.ofArray three) (IndexedList.ofArray one) (IndexedList.ofArray four)
                         |> ignore
                     )))
            (nameof IndexedList.map3,
             struct ((fun () ->
                         List.map3 (fun a b c -> a + b + c) (List.ofArray three) (List.ofArray one) (List.ofArray four)
                         |> ignore
                     ),
                     (fun () ->
                         IndexedList.map3
                             (fun a b c -> a + b + c)
                             (IndexedList.ofArray three)
                             (IndexedList.ofArray one)
                             (IndexedList.ofArray four)
                         |> ignore
                     )))
            (nameof IndexedList.zip,
             struct ((fun () -> List.zip (List.ofArray four) (List.ofArray one) |> ignore),
                     (fun () ->
                         IndexedList.zip (IndexedList.ofArray four) (IndexedList.ofArray one)
                         |> ignore
                     )))
            (nameof IndexedList.foldBack2,
             struct ((fun () ->
                         List.foldBack2 (fun _ _ state -> state) (List.ofArray one) (List.ofArray four) 0
                         |> ignore
                     ),
                     (fun () ->
                         IndexedList.foldBack2 (fun _ _ state -> state) (IndexedList.ofArray one) (IndexedList.ofArray four) 0
                         |> ignore
                     )))
            ($"%s{nameof IndexedList.foldBack2} of an empty first list",
             struct ((fun () ->
                         List.foldBack2 (fun _ _ state -> state) List.empty<int> (List.ofArray four) 0
                         |> ignore
                     ),
                     (fun () ->
                         IndexedList.foldBack2 (fun _ _ state -> state) IndexedList.empty<int> (IndexedList.ofArray four) 0
                         |> ignore
                     )))
            (nameof IndexedList.take,
             struct ((fun () -> List.take 5 (List.ofArray three) |> ignore),
                     (fun () -> IndexedList.take 5 (IndexedList.ofArray three) |> ignore)))
            (nameof IndexedList.skip,
             struct ((fun () -> List.skip 5 (List.ofArray three) |> ignore),
                     (fun () -> IndexedList.skip 5 (IndexedList.ofArray three) |> ignore)))
            (nameof IndexedList.splitAt,
             struct ((fun () -> List.splitAt 5 (List.ofArray three) |> ignore),
                     (fun () -> IndexedList.splitAt 5 (IndexedList.ofArray three) |> ignore)))
            (nameof IndexedList.init, struct ((fun () -> List.init -2 id |> ignore), (fun () -> IndexedList.init -2 id |> ignore)))
            (nameof IndexedList.windowed,
             struct ((fun () -> List.windowed 0 (List.ofArray three) |> ignore),
                     (fun () -> IndexedList.windowed 0 (IndexedList.ofArray three) |> ignore)))
            (nameof IndexedList.transpose,
             struct ((fun () ->
                         List.transpose [| List.ofArray four; List.ofArray three; List.ofArray four |]
                         |> ignore
                     ),
                     (fun () ->
                         IndexedList.transpose [|
                             IndexedList.ofArray four
                             IndexedList.ofArray three
                             IndexedList.ofArray four
                         |]
                         |> ignore
                     )))
            (nameof IndexedList.randomChoiceBy,
             struct ((fun () ->
                         List.randomChoiceBy (fun () -> 1.5) (List.ofArray three)
                         |> ignore
                     ),
                     (fun () ->
                         IndexedList.randomChoiceBy (fun () -> 1.5) (IndexedList.ofArray three)
                         |> ignore
                     )))
        |]

    /// The names of the functions that take an indexed list, as the rows of their test.
    static member NullListRejections = nullListRejections.Keys

    /// The names of the functions that take another input that can be null, as the rows of their test.
    static member OtherNullRejections = otherNullRejections.Keys

    /// The names of the rows of the test of the sequences that the functions reject besides the ones of the contracts.
    static member SequenceRejections = sequenceRejections.Keys

    /// The names of the functions that take a range, as the rows of the test of the whole range.
    static member WholeRanges = wholeRanges.Keys

    /// The names of the sorting functions, as the rows of the test of one element.
    static member SingleElementSorts = singleElementSorts.Keys

    /// Every index of the strings that binarySearch searches.
    static member StringIndices = seq { 0 .. (strings ()).Length - 1 }


    /// The names of the functions of two lists, as the rows of the test of the common prefix.
    static member CommonPrefixWalks = commonPrefixWalks.Keys

    /// The names of the functions whose exceptions have several lines, as the rows of the test of the messages.
    static member CompleteMessages = completeMessages.Keys

    [<TestMethod; DynamicData(nameof IndexedListTests.NullListRejections)>]
    member _.``functions reject a null indexed list`` (name : string) =
        let struct (paramName, action) = nullListRejections[name]
        FlatListAssert.RejectsNull ($"IndexedList.%s{name}", paramName, action)

    [<TestMethod; DynamicData(nameof IndexedListTests.OtherNullRejections)>]
    member _.``functions reject other null inputs with the names of their parameters`` (name : string) =
        let struct (paramName, action) = otherNullRejections[name]
        FlatListAssert.RejectsNull ($"IndexedList.%s{name}", paramName, action)

    [<TestMethod; DynamicData(nameof IndexedListTests.SequenceRejections)>]
    member _.``functions that take a sequence reject a default flat list of strings as objects and check their inputs in order``
        (name : string)
        =
        let struct (paramName, actual) = sequenceRejections[name]
        FlatListAssert.RejectsNull ($"IndexedList.%s{name}", paramName, actual)

    [<TestMethod>]
    member _.``a flat list of strings that is not default passes as a sequence of objects`` () =
        let widened = FlatListOracles.stringsAsObjects (FlatList.ofArray [| "a"; "b" |])
        let asObjects (strings : string array) = strings |> Array.map (fun item -> item :> obj)
        let objects = IndexedList.ofArray (asObjects [| "c" |])

        CollectionAssert.AreEqual (
            asObjects [| "a"; "b" |],
            IndexedList.toArray (IndexedList.ofSeq widened),
            "IndexedList.ofSeq must take the elements of a flat list of strings as objects"
        )

        CollectionAssert.AreEqual (
            asObjects [| "a"; "b"; "c" |],
            IndexedList.toArray (IndexedList.insertManyAt 0 widened objects),
            "IndexedList.insertManyAt must take the elements of a flat list of strings as objects"
        )

        CollectionAssert.AreEqual (
            asObjects [| "c" |],
            IndexedList.toArray (IndexedList.except widened objects),
            "IndexedList.except must take the elements of a flat list of strings as objects"
        )

        CollectionAssert.AreEqual (
            asObjects [| "c" |],
            IndexedList.toArray (IndexedList.removeMany widened (IndexedList.ofArray (asObjects [| "a"; "c"; "b" |]))),
            "IndexedList.removeMany must take the elements of a flat list of strings as objects"
        )

        CollectionAssert.AreEqual (
            asObjects [| "c" |],
            IndexedList.toArray (
                IndexedList.removeManyWith
                    EqualityComparer<obj>.Default
                    widened
                    (IndexedList.ofArray (asObjects [| "a"; "c"; "b" |]))
            ),
            "IndexedList.removeManyWith must take the elements of a flat list of strings as objects"
        )

    [<TestMethod>]
    member _.``ofSeq of an indexed list returns it without a copy`` () =
        let list = several ()
        Assert.AreSame (list, IndexedList.ofSeq list, "IndexedList.ofSeq of an indexed list must return it")

    [<TestMethod; DynamicData(nameof IndexedListTests.WholeRanges)>]
    member _.``range functions return the input itself for the whole range`` (name : string) =
        let list = several ()
        Assert.AreSame (list, wholeRanges[name] list, $"IndexedList.%s{name} of the whole range must return the input")

    [<TestMethod; DynamicData(nameof IndexedListTests.SingleElementSorts)>]
    member _.``sorts return a list of one element itself, as List does`` (name : string) =
        let single = IndexedList.singleton 1
        Assert.AreSame (single, singleElementSorts[name] single, $"IndexedList.%s{name} of one element must return the input")

    [<TestMethod>]
    member _.``append returns the second indexed list when the first one is empty`` () =
        let list = several ()
        Assert.AreSame (
            list,
            IndexedList.append IndexedList.empty list,
            "IndexedList.append of an empty first list must return the second one"
        )

    [<TestMethod>]
    member _.``cons adds the value before the first element`` () =
        CollectionAssert.AreEqual (
            Array.append [| 99 |] Inputs.Several,
            IndexedList.toArray (IndexedList.cons 99 (several ())),
            "IndexedList.cons must add the value before the first element"
        )

    [<TestMethod>]
    member _.``uncons returns the first element and the other elements`` () =
        let struct (head, tail) = IndexedList.uncons (several ())
        Assert.AreEqual<int>(Array.head Inputs.Several, head, "IndexedList.uncons must return the first element")
        CollectionAssert.AreEqual (
            Array.tail Inputs.Several,
            IndexedList.toArray tail,
            "IndexedList.uncons must return the other elements"
        )

    [<TestMethod>]
    member _.``uncons throws for an empty indexed list as List.head does`` () =
        ExceptionAssert.Same (
            id,
            (fun () -> List.head List.empty<int> |> ignore),
            (fun () -> IndexedList.uncons IndexedList.empty<int> |> ignore),
            "IndexedList.uncons of an empty list must throw as List.head does"
        )

    [<TestMethod>]
    member _.``tryUncons returns the first element and the other elements or nothing`` () =
        let struct (head, tail) =
            Assert.WantValueSome (
                IndexedList.tryUncons (several ()),
                "IndexedList.tryUncons must return the elements of a list with elements"
            )

        Assert.AreEqual<int>(Array.head Inputs.Several, head, "IndexedList.tryUncons must return the first element")

        CollectionAssert.AreEqual (
            Array.tail Inputs.Several,
            IndexedList.toArray tail,
            "IndexedList.tryUncons must return the other elements"
        )

        Assert.IsTrue (
            (IndexedList.tryUncons IndexedList.empty<int>).IsNone,
            "IndexedList.tryUncons of an empty list must return nothing"
        )

    [<TestMethod>]
    member _.``Cons matches the first element and the other elements`` () =
        let rec sum (list : IndexedList<int>) =
            match list with
            | IndexedList.Cons (struct (head, tail)) -> head + sum tail
            | _ -> 0

        Assert.AreEqual<int>(Array.sum Inputs.Several, sum (several ()), "IndexedList.Cons must match every element in turn")

        match IndexedList.empty<int> with
        | IndexedList.Cons _ -> Assert.Fail "IndexedList.Cons must not match an empty list"
        | _ -> ()

    [<TestMethod>]
    member _.``Nil matches an empty indexed list only`` () =
        let describe (list : IndexedList<int>) =
            match list with
            | IndexedList.Nil -> "empty"
            | IndexedList.Cons (struct (head, _)) -> $"starts with %d{head}"
            | _ -> "unreachable"

        Assert.AreEqual<string>("empty", describe IndexedList.empty, "IndexedList.Nil must match an empty list")
        Assert.AreEqual<string>("starts with 5", describe (several ()), "IndexedList.Nil must not match a list with elements")

    [<TestMethod>]
    member _.``binarySearch finds a value or the complement of its insertion point`` () =
        let list = IndexedList.ofArray [| 1; 3; 5; 7 |]
        Assert.AreEqual<int>(2, IndexedList.binarySearch 5 list, "IndexedList.binarySearch must find 5 at index 2")
        Assert.AreEqual<int>(
            ~~~2,
            IndexedList.binarySearch 4 list,
            "IndexedList.binarySearch must return the complement of the insertion point of 4"
        )
        Assert.AreEqual<int>(
            ~~~4,
            IndexedList.binarySearch 9 list,
            "IndexedList.binarySearch must return the complement of the length for 9"
        )

    [<TestMethod; DynamicData(nameof IndexedListTests.StringIndices)>]
    member _.``binarySearch uses the order of sort, which is ordinal for strings`` (index : int) =
        let list = IndexedList.ofArray (strings ()) |> IndexedList.sort
        CollectionAssert.AreEqual (
            [| "A"; "B"; "a"; "b" |],
            IndexedList.toArray list,
            "IndexedList.sort must order strings ordinally"
        )

        Assert.AreEqual<int>(
            index,
            IndexedList.binarySearch list[index] list,
            "IndexedList.binarySearch must find every element of a sorted indexed list at its index"
        )

    [<TestMethod>]
    member _.``blit copies a range into an array`` () =
        let target = Array.zeroCreate<int> 5
        IndexedList.blit (several ()) 2 target 1 3
        CollectionAssert.AreEqual (
            [| 0; 8; 5; 0; 0 |],
            target,
            "IndexedList.blit must copy three elements from index 2 to index 1"
        )

    [<TestMethod; DynamicData(nameof ListMemberCases.BlitRejections, typeof<ListMemberCases>)>]
    member _.``blit throws for a range out of range with the names of its parameters``
        (sourceIndex : int, targetIndex : int, count : int, paramName : string)
        =
        let ex =
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(
                Action (fun () -> IndexedList.blit (several ()) sourceIndex (Array.zeroCreate 5) targetIndex count),
                "IndexedList.blit must reject a range out of range"
            )

        Assert.AreEqual<string | null>(paramName, ex.ParamName, $"IndexedList.blit must name %s{paramName}")

    [<TestMethod>]
    member _.``contains compares in F# structural equality, indexOf in the default equality`` () =
        let arrays = IndexedList.singleton [| 1; 2 |]
        Assert.IsTrue (
            IndexedList.contains [| 1; 2 |] arrays,
            "IndexedList.contains must compare arrays by their elements, as List.contains does"
        )
        Assert.AreEqual<int>(-1, IndexedList.indexOf [| 1; 2 |] arrays, "IndexedList.indexOf must compare arrays by reference")

        let nan = IndexedList.singleton Double.NaN
        Assert.AreEqual<bool>(
            List.contains Double.NaN [ Double.NaN ],
            IndexedList.contains Double.NaN nan,
            "IndexedList.contains must compare NaN as List.contains does"
        )

    [<TestMethod>]
    member _.``map and mapi call the mapping in the order of the elements`` () =
        let list = IndexedList.init 100 id
        let visited = ResizeArray<int>()
        IndexedList.map visited.Add list |> ignore
        CollectionAssert.AreEqual (Array.init 100 id, visited.ToArray (), "IndexedList.map must call the mapping in order")

        visited.Clear ()
        IndexedList.mapi (fun _ item -> visited.Add item) list
        |> ignore
        CollectionAssert.AreEqual (Array.init 100 id, visited.ToArray (), "IndexedList.mapi must call the mapping in order")

    [<TestMethod; DynamicData(nameof IndexedListTests.CommonPrefixWalks)>]
    member _.``the functions of two lists walk the common prefix before they throw, as List does`` (name : string) =
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
            $"IndexedList.%s{name} must call its function for the pairs that List calls it for"
        )

    [<TestMethod>]
    member _.``exists2 and forall2 decide before they compare the lengths, as List does`` () =
        let short = [| 1; 2 |]
        let long = [| 1; 20; 30 |]
        Assert.AreEqual<bool>(
            List.exists2 (=) (List.ofArray short) (List.ofArray long),
            IndexedList.exists2 (=) (IndexedList.ofArray short) (IndexedList.ofArray long),
            "IndexedList.exists2 must return true for a match before the end of the shorter list"
        )
        Assert.AreEqual<bool>(
            List.forall2 (=) (List.ofArray short) (List.ofArray long),
            IndexedList.forall2 (=) (IndexedList.ofArray short) (IndexedList.ofArray long),
            "IndexedList.forall2 must return false for a mismatch before the end of the shorter list"
        )

    [<TestMethod>]
    member _.``except does not read the items to exclude for an empty indexed list, as List does`` () =
        let items = seq { failwith "The items to exclude must not be read" }
        Assert.IsEmpty (IndexedList.except items IndexedList.empty<int>, "IndexedList.except of an empty list must be empty")

    [<TestMethod>]
    member _.``transpose names the list that ends first as List does`` () =
        ExceptionAssert.Same (
            id,
            (fun () -> List.transpose [| [ 1 ]; [ 1; 2 ] |] |> ignore),
            (fun () ->
                IndexedList.transpose [| IndexedList.ofArray [| 1 |]; IndexedList.ofArray [| 1; 2 |] |]
                |> ignore
            ),
            "IndexedList.transpose of a shorter first list must throw as List.transpose does"
        )

    [<TestMethod>]
    member _.``splitAt of an empty indexed list throws as List.splitAt does`` () =
        ExceptionAssert.Same (
            id,
            (fun () -> List.splitAt 1 List.empty<int> |> ignore),
            (fun () -> IndexedList.splitAt 1 IndexedList.empty<int> |> ignore),
            "IndexedList.splitAt 1 of an empty list must throw as List.splitAt does"
        )

    [<TestMethod; DynamicData(nameof IndexedListTests.CompleteMessages)>]
    member _.``the exceptions have the complete messages of List`` (name : string) =
        let struct (oracle, actual) = completeMessages[name]
        let expected = thrown oracle $"List.%s{name} must throw"
        let ex = thrown actual $"IndexedList.%s{name} must throw"
        Assert.AreEqual<Type>(
            expected.GetType (),
            ex.GetType (),
            $"IndexedList.%s{name} must throw the type of exception of List"
        )
        Assert.AreEqual<string>(expected.Message, ex.Message, $"IndexedList.%s{name} must throw the message of List")

    /// An F# list is an `IReadOnlyList` whose indexer walks the list from its head, and the BCL builds a tree by indexing,
    /// so `ImmutableList.CreateRange` of a list takes time that grows with the square of the length: about 6 s for 100 000
    /// elements and four times as much for 200 000, where the copy that `ofList` makes takes a few milliseconds. The timeout is
    /// far above the time of the copy and a fraction of the time of the quadratic build.
    [<TestMethod; Timeout 5000>]
    member _.``ofList and ofSeq copy a long list before they build the tree`` () =
        let expected = Array.init 200_000 id
        let list = List.ofArray expected

        CollectionAssert.AreEqual (
            expected,
            IndexedList.toArray (IndexedList.ofList list),
            "IndexedList.ofList must keep the elements of a long list in order"
        )

        CollectionAssert.AreEqual (
            expected,
            IndexedList.toArray (IndexedList.ofSeq list),
            "IndexedList.ofSeq must keep the elements of a long list in order"
        )

    /// The functions that walk the list use the enumerator of the interface for reference types (see Walk), a path that
    /// the contracts, over integers, do not reach.
    [<TestMethod; DataRow 0; DataRow 1; DataRow 5; DataRow 2000>]
    member _.``the functions that walk a list give the results of List for strings`` (length : int) =
        let items = Array.init length (fun index -> $"item %d{index}")
        let list = IndexedList.ofArray items
        let expected = List.ofArray items
        let visited = ResizeArray<string>()
        IndexedList.iter visited.Add list
        CollectionAssert.AreEqual (items, visited.ToArray (), "IndexedList.iter must visit the strings in order")

        let visitedAt = ResizeArray<string>()
        IndexedList.iteri (fun index item -> visitedAt.Add $"%d{index}:%s{item}") list

        CollectionAssert.AreEqual (
            expected
            |> List.mapi (fun index item -> $"%d{index}:%s{item}")
            |> List.toArray,
            visitedAt.ToArray (),
            "IndexedList.iteri must pass the index and the string in order"
        )

        let fold (total : int) (item : string) = total * 31 + item.Length

        Assert.AreEqual<int>(
            List.fold fold 7 expected,
            IndexedList.fold fold 7 list,
            "IndexedList.fold must thread the state through the strings in order"
        )

        CollectionAssert.AreEqual (
            expected
            |> List.mapi (fun index item -> $"%d{index}:%s{item}")
            |> List.toArray,
            IndexedList.mapi (fun index item -> $"%d{index}:%s{item}") list
            |> IndexedList.toArray,
            "IndexedList.mapi must map the strings in order"
        )

        CollectionAssert.AreEqual (
            expected |> List.scan fold 7 |> List.toArray,
            IndexedList.scan fold 7 list |> IndexedList.toArray,
            "IndexedList.scan must give the states for the strings"
        )

        let pick (item : string) =
            if item.EndsWith ('7') then
                ValueSome item.Length
            else
                ValueNone

        Assert.AreEqual<int voption>(
            expected
            |> List.tryPick (fun item -> pick item |> ValueOption.toOption)
            |> ValueOption.ofOption,
            IndexedList.tryPick pick list,
            "IndexedList.tryPick must return the first value for the strings"
        )

        CollectionAssert.AreEqual (
            expected
            |> List.choose (fun item -> pick item |> ValueOption.toOption)
            |> List.toArray,
            IndexedList.choose pick list |> IndexedList.toArray,
            "IndexedList.choose must keep the chosen values for the strings"
        )

        CollectionAssert.AreEqual (
            expected
            |> List.collect (fun item -> [ item; item ])
            |> List.toArray,
            IndexedList.collect (fun item -> IndexedList.ofArray [| item; item |]) list
            |> IndexedList.toArray,
            "IndexedList.collect must concatenate the results for the strings"
        )

    /// The contracts choose from short lists, which a new array holds; a long list goes through the rented array, and the
    /// selectivities give the results of all lengths, down to a few elements.
    [<TestMethod; DataRow 100; DataRow 50; DataRow 1>]
    member _.``choose of a long list keeps the chosen elements in order`` (percent : int) =
        let items = Array.init 100_000 id

        CollectionAssert.AreEqual (
            items
            |> Array.choose (fun x -> if x % 100 < percent then Some (x * 2) else None),
            IndexedList.ofArray items
            |> IndexedList.choose (fun x -> if x % 100 < percent then ValueSome (x * 2) else ValueNone)
            |> IndexedList.toArray,
            $"IndexedList.choose must keep the chosen elements of a long list in order (%d{percent} %% chosen)"
        )
