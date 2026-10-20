namespace FSharp.Collections.Immutable.Tests

open System
open System.Collections
open System.Collections.Generic
open System.Collections.Immutable
open FSharp.Collections.Immutable
open Microsoft.VisualStudio.TestTools.UnitTesting

/// <summary>
/// An implementation of <see cref="T:System.Collections.Immutable.IImmutableList`1"/> that is not an indexed list, to tell
/// whether a function keeps the implementation of its input.
/// </summary>
/// <remarks>
/// Every member that returns a list returns a tagged list. A tagged list that treats null values as no values, where the
/// members of <see cref="T:System.Collections.Immutable.ImmutableList`1"/> throw, shows whether a function rejects them
/// itself.
/// </remarks>
type TaggedList<'T> (items : ImmutableList<'T>, nullValuesAreNothing : bool) =

    static let wrap (items : ImmutableList<'T>) = TaggedList items :> IIndexedList<'T>

    new (items : ImmutableList<'T>) = TaggedList (items, false)

    interface IImmutableList<'T> with
        member _.Add value = wrap (items.Add value)
        member _.AddRange values = wrap (items.AddRange values)
        member _.Clear () = wrap (items.Clear ())
        member _.IndexOf (item, index, count, comparer) = items.IndexOf (item, index, count, comparer)
        member _.Insert (index, element) = wrap (items.Insert (index, element))
        member _.InsertRange (index, values) = wrap (items.InsertRange (index, values))
        member _.LastIndexOf (item, index, count, comparer) = items.LastIndexOf (item, index, count, comparer)
        member _.Remove (value, comparer) = wrap (items.Remove (value, comparer))
        member _.RemoveAll predicate = wrap (items.RemoveAll predicate)
        member _.RemoveAt index = wrap (items.RemoveAt index)

        member _.RemoveRange (values : IEnumerable<'T>, comparer : IEqualityComparer<'T> | null) =
            let values =
                if nullValuesAreNothing && obj.ReferenceEquals (values, null) then
                    Seq.empty
                else
                    values
            wrap (items.RemoveRange (values, comparer))

        member _.RemoveRange (index : int, count : int) = wrap (items.RemoveRange (index, count))
        member _.Replace (oldValue, newValue, comparer) = wrap (items.Replace (oldValue, newValue, comparer))
        member _.SetItem (index, value) = wrap (items.SetItem (index, value))

    interface IReadOnlyList<'T> with
        member _.Item
            with get index = items[index]

    interface IReadOnlyCollection<'T> with
        member _.Count = items.Count

    interface IEnumerable<'T> with
        member _.GetEnumerator () = (items :> IEnumerable<'T>).GetEnumerator()

    interface IEnumerable with
        member _.GetEnumerator () = (items :> IEnumerable).GetEnumerator()

/// The behaviour of `IIndexedList` that the shared contracts do not cover: the implementation of the results, null
/// inputs and the patterns.
[<TestClass; IIndexedListTestCategory; PatternMatchingTestCategory>]
type IIndexedListTests () =

    static let nullList = Unchecked.defaultof<IIndexedList<int>>

    static let several () : IIndexedList<int> = IndexedList.ofArray Inputs.Several

    static let tagged () : IIndexedList<int> = TaggedList (ImmutableList.CreateRange Inputs.Several)

    /// A tagged list that accepts null values, so that only the function can reject them.
    static let lenient () : IIndexedList<int> = TaggedList (ImmutableList.CreateRange Inputs.Several, true)

    // Each call draws from a new generator with the same seed, so that both implementations get the same choices
    static let random () = Random 42

    static let randomizer () =
        let random = Random 42
        fun () -> random.NextDouble ()

    static let first (struct (first : IIndexedList<int>, _ : IIndexedList<int>)) = first

    static let second (struct (_ : IIndexedList<int>, second : IIndexedList<int>)) = second

    /// The functions whose result has the element type of their input, by name, as functions of the input. The rows of
    /// the test are the names: a function has no readable name in a row.
    static let sameElementType : Map<string, IIndexedList<int> -> IIndexedList<int>> =
        Map.ofArray [|
            nameof IIndexedList.add, IIndexedList.add 99
            nameof IIndexedList.append, (fun list -> IIndexedList.append list (IndexedList.ofArray [| 1; 2 |]))
            nameof IIndexedList.cons, IIndexedList.cons 99
            nameof IIndexedList.filter, IIndexedList.filter (fun item -> item > 0)
            nameof IIndexedList.insertAt, IIndexedList.insertAt 3 99
            nameof IIndexedList.insertManyAt, IIndexedList.insertManyAt 3 [| 98; 99 |]
            nameof IIndexedList.permute, IIndexedList.permute (fun index -> (index + 3) % 8)
            nameof IIndexedList.removeAt, IIndexedList.removeAt 3
            nameof IIndexedList.remove, IIndexedList.remove 5
            nameof IIndexedList.removeMany, IIndexedList.removeMany [| 5; 8 |]
            nameof IIndexedList.removeManyAt, IIndexedList.removeManyAt 2 3
            nameof IIndexedList.removeManyWith, IIndexedList.removeManyWith EqualityComparer<int>.Default [| 5; 8 |]
            nameof IIndexedList.removeWith, IIndexedList.removeWith EqualityComparer<int>.Default 5
            nameof IIndexedList.replace, IIndexedList.replace 5 99
            nameof IIndexedList.replaceWith, IIndexedList.replaceWith EqualityComparer<int>.Default 5 99
            nameof IIndexedList.rev, IIndexedList.rev
            nameof IIndexedList.updateAt, IIndexedList.updateAt 3 99
            nameof IIndexedList.where, IIndexedList.where (fun item -> item > 0)
            nameof IIndexedList.distinct, IIndexedList.distinct
            nameof IIndexedList.distinctBy, IIndexedList.distinctBy abs
            nameof IIndexedList.except, IIndexedList.except [| 5 |]
            $"%s{nameof IIndexedList.partition} (accepted)", IIndexedList.partition (fun item -> item > 0) >> first
            $"%s{nameof IIndexedList.partition} (rejected)", IIndexedList.partition (fun item -> item > 0) >> second
            nameof IIndexedList.skip, IIndexedList.skip 3
            nameof IIndexedList.skipLast, IIndexedList.skipLast 3
            nameof IIndexedList.skipWhile, IIndexedList.skipWhile (fun item -> item > 0)
            $"%s{nameof IIndexedList.splitAt} (first part)", IIndexedList.splitAt 3 >> first
            $"%s{nameof IIndexedList.splitAt} (second part)", IIndexedList.splitAt 3 >> second
            nameof IIndexedList.take, IIndexedList.take 3
            nameof IIndexedList.takeLast, IIndexedList.takeLast 3
            nameof IIndexedList.takeWhile, IIndexedList.takeWhile (fun item -> item > 0)
            nameof IIndexedList.truncate, IIndexedList.truncate 3
            nameof IIndexedList.sort, IIndexedList.sort
            nameof IIndexedList.sortBy, IIndexedList.sortBy abs
            nameof IIndexedList.sortByDescending, IIndexedList.sortByDescending abs
            nameof IIndexedList.sortDescending, IIndexedList.sortDescending
            nameof IIndexedList.sortWith, IIndexedList.sortWith compare
            nameof IIndexedList.sortWithComparer, IIndexedList.sortWithComparer Comparer<int>.Default
            nameof IIndexedList.randomChoicesWith, (fun list -> IIndexedList.randomChoicesWith (random ()) 3 list)
            nameof IIndexedList.randomChoicesBy, (fun list -> IIndexedList.randomChoicesBy (randomizer ()) 3 list)
            nameof IIndexedList.randomSampleWith, (fun list -> IIndexedList.randomSampleWith (random ()) 3 list)
            nameof IIndexedList.randomSampleBy, (fun list -> IIndexedList.randomSampleBy (randomizer ()) 3 list)
            nameof IIndexedList.randomShuffleWith, (fun list -> IIndexedList.randomShuffleWith (random ()) list)
            nameof IIndexedList.randomShuffleBy, (fun list -> IIndexedList.randomShuffleBy (randomizer ()) list)
            nameof IIndexedList.tail, IIndexedList.tail
            nameof IIndexedList.tryTail,
            (fun list ->
                Assert.WantValueSome (
                    IIndexedList.tryTail list,
                    "IIndexedList.tryTail must return the tail of a list with elements"
                )
            )
            (nameof IIndexedList.uncons,
             fun list ->
                 let struct (_, tail) = IIndexedList.uncons list
                 tail)
            nameof IIndexedList.clear, IIndexedList.clear
        |]

    /// The functions that take a list, by name, with the parameter that names the null list and the function on a null
    /// list.
    static let nullListRejections =
        let pairs = Unchecked.defaultof<IIndexedList<struct (int * int)>>
        let triples = Unchecked.defaultof<IIndexedList<struct (int * int * int)>>
        let floats = Unchecked.defaultof<IIndexedList<float>>

        [|
            struct (nameof IIndexedList.isEmpty, "list", (fun () -> IIndexedList.isEmpty nullList |> ignore))
            struct (nameof IIndexedList.length, "list", (fun () -> IIndexedList.length nullList |> ignore))
            struct (nameof IIndexedList.exactlyOne, "list", (fun () -> IIndexedList.exactlyOne nullList |> ignore))
            struct (nameof IIndexedList.head, "list", (fun () -> IIndexedList.head nullList |> ignore))
            struct (nameof IIndexedList.item, "list", (fun () -> IIndexedList.item 0 nullList |> ignore))
            struct (nameof IIndexedList.last, "list", (fun () -> IIndexedList.last nullList |> ignore))
            struct (nameof IIndexedList.tail, "list", (fun () -> IIndexedList.tail nullList |> ignore))
            struct (nameof IIndexedList.tryExactlyOne, "list", (fun () -> IIndexedList.tryExactlyOne nullList |> ignore))
            struct (nameof IIndexedList.tryHead, "list", (fun () -> IIndexedList.tryHead nullList |> ignore))
            struct (nameof IIndexedList.tryItem, "list", (fun () -> IIndexedList.tryItem 0 nullList |> ignore))
            struct (nameof IIndexedList.tryLast, "list", (fun () -> IIndexedList.tryLast nullList |> ignore))
            struct (nameof IIndexedList.tryTail, "list", (fun () -> IIndexedList.tryTail nullList |> ignore))
            struct (nameof IIndexedList.tryUncons, "list", (fun () -> IIndexedList.tryUncons nullList |> ignore))
            struct (nameof IIndexedList.uncons, "list", (fun () -> IIndexedList.uncons nullList |> ignore))
            struct (nameof IIndexedList.(|Cons|_|), "list", (fun () -> IIndexedList.(|Cons|_|) nullList |> ignore))
            struct (nameof IIndexedList.(|Nil|_|), "list", (fun () -> IIndexedList.(|Nil|_|) nullList |> ignore))
            struct (nameof IIndexedList.contains, "source", (fun () -> IIndexedList.contains 1 nullList |> ignore))
            struct (nameof IIndexedList.exists, "list", (fun () -> IIndexedList.exists ((=) 1) nullList |> ignore))
            struct (nameof IIndexedList.exists2, "list1", (fun () -> IIndexedList.exists2 (=) nullList (several ()) |> ignore))
            struct (nameof IIndexedList.find, "list", (fun () -> IIndexedList.find ((=) 1) nullList |> ignore))
            struct (nameof IIndexedList.findBack, "list", (fun () -> IIndexedList.findBack ((=) 1) nullList |> ignore))
            struct (nameof IIndexedList.findIndex, "list", (fun () -> IIndexedList.findIndex ((=) 1) nullList |> ignore))
            struct (nameof IIndexedList.findIndexBack, "list", (fun () -> IIndexedList.findIndexBack ((=) 1) nullList |> ignore))
            struct (nameof IIndexedList.forall, "list", (fun () -> IIndexedList.forall ((=) 1) nullList |> ignore))
            struct (nameof IIndexedList.forall2, "list2", (fun () -> IIndexedList.forall2 (=) (several ()) nullList |> ignore))
            struct (nameof IIndexedList.indexOf, "list", (fun () -> IIndexedList.indexOf 1 nullList |> ignore))
            struct (nameof IIndexedList.indexOfRange, "list", (fun () -> IIndexedList.indexOfRange 0 0 1 nullList |> ignore))
            struct (nameof IIndexedList.indexOfRangeWith,
                    "list",
                    (fun () ->
                        IIndexedList.indexOfRangeWith EqualityComparer<int>.Default 0 0 1 nullList
                        |> ignore
                    ))
            struct (nameof IIndexedList.indexOfWith,
                    "list",
                    (fun () ->
                        IIndexedList.indexOfWith EqualityComparer<int>.Default 1 nullList
                        |> ignore
                    ))
            struct (nameof IIndexedList.lastIndexOf, "list", (fun () -> IIndexedList.lastIndexOf 1 nullList |> ignore))
            struct (nameof IIndexedList.lastIndexOfRange,
                    "list",
                    (fun () -> IIndexedList.lastIndexOfRange 0 0 1 nullList |> ignore))
            struct (nameof IIndexedList.lastIndexOfRangeWith,
                    "list",
                    (fun () ->
                        IIndexedList.lastIndexOfRangeWith EqualityComparer<int>.Default 0 0 1 nullList
                        |> ignore
                    ))
            struct (nameof IIndexedList.lastIndexOfWith,
                    "list",
                    (fun () ->
                        IIndexedList.lastIndexOfWith EqualityComparer<int>.Default 1 nullList
                        |> ignore
                    ))
            struct (nameof IIndexedList.pick, "list", (fun () -> IIndexedList.pick ValueSome nullList |> ignore))
            struct (nameof IIndexedList.tryFind, "list", (fun () -> IIndexedList.tryFind ((=) 1) nullList |> ignore))
            struct (nameof IIndexedList.tryFindBack, "list", (fun () -> IIndexedList.tryFindBack ((=) 1) nullList |> ignore))
            struct (nameof IIndexedList.tryFindIndex, "list", (fun () -> IIndexedList.tryFindIndex ((=) 1) nullList |> ignore))
            struct (nameof IIndexedList.tryFindIndexBack,
                    "list",
                    (fun () -> IIndexedList.tryFindIndexBack ((=) 1) nullList |> ignore))
            struct (nameof IIndexedList.tryPick, "list", (fun () -> IIndexedList.tryPick ValueSome nullList |> ignore))
            struct (nameof IIndexedList.add, "list", (fun () -> IIndexedList.add 1 nullList |> ignore))
            struct (nameof IIndexedList.allPairs, "list2", (fun () -> IIndexedList.allPairs (several ()) nullList |> ignore))
            struct (nameof IIndexedList.append, "list1", (fun () -> IIndexedList.append nullList (several ()) |> ignore))
            struct (nameof IIndexedList.choose, "list", (fun () -> IIndexedList.choose ValueSome nullList |> ignore))
            struct (nameof IIndexedList.chunkBySize, "list", (fun () -> IIndexedList.chunkBySize 2 nullList |> ignore))
            struct (nameof IIndexedList.collect,
                    "list",
                    (fun () ->
                        IIndexedList.collect (fun _ -> several ()) nullList
                        |> ignore
                    ))
            struct (nameof IIndexedList.concat, "lists", (fun () -> IIndexedList.concat [| several (); nullList |] |> ignore))
            struct (nameof IIndexedList.cons, "list", (fun () -> IIndexedList.cons 1 nullList |> ignore))
            struct (nameof IIndexedList.filter, "list", (fun () -> IIndexedList.filter ((=) 1) nullList |> ignore))
            struct (nameof IIndexedList.indexed, "list", (fun () -> IIndexedList.indexed nullList |> ignore))
            struct (nameof IIndexedList.insertAt, "source", (fun () -> IIndexedList.insertAt 0 1 nullList |> ignore))
            struct (nameof IIndexedList.insertManyAt, "source", (fun () -> IIndexedList.insertManyAt 0 [| 1 |] nullList |> ignore))
            struct (nameof IIndexedList.map, "list", (fun () -> IIndexedList.map id nullList |> ignore))
            struct (nameof IIndexedList.map2, "list1", (fun () -> IIndexedList.map2 (+) nullList (several ()) |> ignore))
            struct (nameof IIndexedList.map3,
                    "list2",
                    (fun () ->
                        IIndexedList.map3 (fun a b c -> a + b + c) (several ()) nullList (several ())
                        |> ignore
                    ))
            struct (nameof IIndexedList.mapi, "list", (fun () -> IIndexedList.mapi (+) nullList |> ignore))
            struct (nameof IIndexedList.mapi2,
                    "list2",
                    (fun () ->
                        IIndexedList.mapi2 (fun index a b -> index + a + b) (several ()) nullList
                        |> ignore
                    ))
            struct (nameof IIndexedList.pairwise, "list", (fun () -> IIndexedList.pairwise nullList |> ignore))
            struct (nameof IIndexedList.permute, "list", (fun () -> IIndexedList.permute id nullList |> ignore))
            struct (nameof IIndexedList.remove, "list", (fun () -> IIndexedList.remove 1 nullList |> ignore))
            struct (nameof IIndexedList.removeAt, "source", (fun () -> IIndexedList.removeAt 0 nullList |> ignore))
            struct (nameof IIndexedList.removeMany, "list", (fun () -> IIndexedList.removeMany [| 1 |] nullList |> ignore))
            struct (nameof IIndexedList.removeManyAt, "source", (fun () -> IIndexedList.removeManyAt 0 1 nullList |> ignore))
            struct (nameof IIndexedList.removeManyWith,
                    "list",
                    (fun () ->
                        IIndexedList.removeManyWith EqualityComparer<int>.Default [| 1 |] nullList
                        |> ignore
                    ))
            struct (nameof IIndexedList.removeWith,
                    "list",
                    (fun () ->
                        IIndexedList.removeWith EqualityComparer<int>.Default 1 nullList
                        |> ignore
                    ))
            struct (nameof IIndexedList.replace, "list", (fun () -> IIndexedList.replace 1 2 nullList |> ignore))
            struct (nameof IIndexedList.replaceWith,
                    "list",
                    (fun () ->
                        IIndexedList.replaceWith EqualityComparer<int>.Default 1 2 nullList
                        |> ignore
                    ))
            struct (nameof IIndexedList.rev, "list", (fun () -> IIndexedList.rev nullList |> ignore))
            struct (nameof IIndexedList.transpose, "lists", (fun () -> IIndexedList.transpose [| nullList |] |> ignore))
            struct (nameof IIndexedList.unzip, "list", (fun () -> IIndexedList.unzip pairs |> ignore))
            struct (nameof IIndexedList.unzip3, "list", (fun () -> IIndexedList.unzip3 triples |> ignore))
            struct (nameof IIndexedList.updateAt, "source", (fun () -> IIndexedList.updateAt 0 1 nullList |> ignore))
            struct (nameof IIndexedList.where, "list", (fun () -> IIndexedList.where ((=) 1) nullList |> ignore))
            struct (nameof IIndexedList.windowed, "list", (fun () -> IIndexedList.windowed 2 nullList |> ignore))
            struct (nameof IIndexedList.zip, "list2", (fun () -> IIndexedList.zip (several ()) nullList |> ignore))
            struct (nameof IIndexedList.zip3,
                    "list3",
                    (fun () ->
                        IIndexedList.zip3 (several ()) (several ()) nullList
                        |> ignore
                    ))
            struct (nameof IIndexedList.average, "list", (fun () -> IIndexedList.average floats |> ignore))
            struct (nameof IIndexedList.averageBy, "list", (fun () -> IIndexedList.averageBy float nullList |> ignore))
            struct (nameof IIndexedList.compareWith,
                    "list1",
                    (fun () ->
                        IIndexedList.compareWith compare nullList (several ())
                        |> ignore
                    ))
            struct (nameof IIndexedList.fold, "list", (fun () -> IIndexedList.fold (+) 0 nullList |> ignore))
            struct (nameof IIndexedList.fold2,
                    "list1",
                    (fun () ->
                        IIndexedList.fold2 (fun state a b -> state + a + b) 0 nullList (several ())
                        |> ignore
                    ))
            struct (nameof IIndexedList.foldBack, "list", (fun () -> IIndexedList.foldBack (+) nullList 0 |> ignore))
            struct (nameof IIndexedList.foldBack2,
                    "list2",
                    (fun () ->
                        IIndexedList.foldBack2 (fun a b state -> state + a + b) (several ()) nullList 0
                        |> ignore
                    ))
            struct (nameof IIndexedList.iter, "list", (fun () -> IIndexedList.iter ignore nullList))
            struct (nameof IIndexedList.iter2, "list1", (fun () -> IIndexedList.iter2 (fun _ _ -> ()) nullList (several ())))
            struct (nameof IIndexedList.iteri, "list", (fun () -> IIndexedList.iteri (fun _ _ -> ()) nullList))
            struct (nameof IIndexedList.iteri2, "list2", (fun () -> IIndexedList.iteri2 (fun _ _ _ -> ()) (several ()) nullList))
            struct (nameof IIndexedList.mapFold,
                    "list",
                    (fun () ->
                        IIndexedList.mapFold (fun state item -> struct (item, state)) 0 nullList
                        |> ignore
                    ))
            struct (nameof IIndexedList.mapFoldBack,
                    "list",
                    (fun () ->
                        IIndexedList.mapFoldBack (fun item state -> struct (item, state)) nullList 0
                        |> ignore
                    ))
            struct (nameof IIndexedList.max, "list", (fun () -> IIndexedList.max nullList |> ignore))
            struct (nameof IIndexedList.maxBy, "list", (fun () -> IIndexedList.maxBy id nullList |> ignore))
            struct (nameof IIndexedList.min, "list", (fun () -> IIndexedList.min nullList |> ignore))
            struct (nameof IIndexedList.minBy, "list", (fun () -> IIndexedList.minBy id nullList |> ignore))
            struct (nameof IIndexedList.reduce, "list", (fun () -> IIndexedList.reduce (+) nullList |> ignore))
            struct (nameof IIndexedList.reduceBack, "list", (fun () -> IIndexedList.reduceBack (+) nullList |> ignore))
            struct (nameof IIndexedList.scan, "list", (fun () -> IIndexedList.scan (+) 0 nullList |> ignore))
            struct (nameof IIndexedList.scanBack, "list", (fun () -> IIndexedList.scanBack (+) nullList 0 |> ignore))
            struct (nameof IIndexedList.sum, "list", (fun () -> IIndexedList.sum nullList |> ignore))
            struct (nameof IIndexedList.sumBy, "list", (fun () -> IIndexedList.sumBy id nullList |> ignore))
            struct (nameof IIndexedList.countBy, "list", (fun () -> IIndexedList.countBy id nullList |> ignore))
            struct (nameof IIndexedList.distinct, "list", (fun () -> IIndexedList.distinct nullList |> ignore))
            struct (nameof IIndexedList.distinctBy, "list", (fun () -> IIndexedList.distinctBy id nullList |> ignore))
            struct (nameof IIndexedList.except, "list", (fun () -> IIndexedList.except [| 1 |] nullList |> ignore))
            struct (nameof IIndexedList.groupBy, "list", (fun () -> IIndexedList.groupBy id nullList |> ignore))
            struct (nameof IIndexedList.partition, "list", (fun () -> IIndexedList.partition ((=) 1) nullList |> ignore))
            struct (nameof IIndexedList.partitionWith,
                    "list",
                    (fun () -> IIndexedList.partitionWith Choice1Of2 nullList |> ignore))
            struct (nameof IIndexedList.skip, "list", (fun () -> IIndexedList.skip 0 nullList |> ignore))
            struct (nameof IIndexedList.skipLast, "list", (fun () -> IIndexedList.skipLast 0 nullList |> ignore))
            struct (nameof IIndexedList.skipWhile, "list", (fun () -> IIndexedList.skipWhile ((=) 1) nullList |> ignore))
            struct (nameof IIndexedList.splitAt, "list", (fun () -> IIndexedList.splitAt 0 nullList |> ignore))
            struct (nameof IIndexedList.splitInto, "list", (fun () -> IIndexedList.splitInto 2 nullList |> ignore))
            struct (nameof IIndexedList.take, "list", (fun () -> IIndexedList.take 0 nullList |> ignore))
            struct (nameof IIndexedList.takeLast, "list", (fun () -> IIndexedList.takeLast 0 nullList |> ignore))
            struct (nameof IIndexedList.takeWhile, "list", (fun () -> IIndexedList.takeWhile ((=) 1) nullList |> ignore))
            struct (nameof IIndexedList.truncate, "list", (fun () -> IIndexedList.truncate 1 nullList |> ignore))
            struct (nameof IIndexedList.sort, "list", (fun () -> IIndexedList.sort nullList |> ignore))
            struct (nameof IIndexedList.sortBy, "list", (fun () -> IIndexedList.sortBy id nullList |> ignore))
            struct (nameof IIndexedList.sortByDescending, "list", (fun () -> IIndexedList.sortByDescending id nullList |> ignore))
            struct (nameof IIndexedList.sortDescending, "list", (fun () -> IIndexedList.sortDescending nullList |> ignore))
            struct (nameof IIndexedList.sortWith, "list", (fun () -> IIndexedList.sortWith compare nullList |> ignore))
            struct (nameof IIndexedList.sortWithComparer,
                    "list",
                    (fun () ->
                        IIndexedList.sortWithComparer Comparer<int>.Default nullList
                        |> ignore
                    ))
            struct (nameof IIndexedList.randomChoice, "source", (fun () -> IIndexedList.randomChoice nullList |> ignore))
            struct (nameof IIndexedList.randomChoiceBy,
                    "source",
                    (fun () ->
                        IIndexedList.randomChoiceBy (fun () -> 0.5) nullList
                        |> ignore
                    ))
            struct (nameof IIndexedList.randomChoiceWith,
                    "source",
                    (fun () -> IIndexedList.randomChoiceWith (Random 1) nullList |> ignore))
            struct (nameof IIndexedList.randomChoices, "source", (fun () -> IIndexedList.randomChoices 1 nullList |> ignore))
            struct (nameof IIndexedList.randomChoicesBy,
                    "source",
                    (fun () ->
                        IIndexedList.randomChoicesBy (fun () -> 0.5) 1 nullList
                        |> ignore
                    ))
            struct (nameof IIndexedList.randomChoicesWith,
                    "source",
                    (fun () ->
                        IIndexedList.randomChoicesWith (Random 1) 1 nullList
                        |> ignore
                    ))
            struct (nameof IIndexedList.randomSample, "source", (fun () -> IIndexedList.randomSample 1 nullList |> ignore))
            struct (nameof IIndexedList.randomSampleBy,
                    "source",
                    (fun () ->
                        IIndexedList.randomSampleBy (fun () -> 0.5) 1 nullList
                        |> ignore
                    ))
            struct (nameof IIndexedList.randomSampleWith,
                    "source",
                    (fun () ->
                        IIndexedList.randomSampleWith (Random 1) 1 nullList
                        |> ignore
                    ))
            struct (nameof IIndexedList.randomShuffle, "source", (fun () -> IIndexedList.randomShuffle nullList |> ignore))
            struct (nameof IIndexedList.randomShuffleBy,
                    "source",
                    (fun () ->
                        IIndexedList.randomShuffleBy (fun () -> 0.5) nullList
                        |> ignore
                    ))
            struct (nameof IIndexedList.randomShuffleWith,
                    "source",
                    (fun () -> IIndexedList.randomShuffleWith (Random 1) nullList |> ignore))
            struct (nameof IIndexedList.toArray, "list", (fun () -> IIndexedList.toArray nullList |> ignore))
            struct (nameof IIndexedList.toList, "list", (fun () -> IIndexedList.toList nullList |> ignore))
            struct (nameof IIndexedList.toSeq, "list", (fun () -> IIndexedList.toSeq nullList |> ignore))
            struct (nameof IIndexedList.clear, "list", (fun () -> IIndexedList.clear nullList |> ignore))
        |]
        |> Array.map (fun struct (name, paramName, action) -> name, struct (paramName, action))
        |> Map.ofArray

    /// The functions that take the values to remove, by name, on null values and on a list that would accept them.
    static let nullItemRejections : Map<string, unit -> unit> =
        let items = Unchecked.defaultof<seq<int>>

        Map.ofArray [|
            nameof IIndexedList.removeMany, (fun () -> IIndexedList.removeMany items (lenient ()) |> ignore)
            (nameof IIndexedList.removeManyWith,
             fun () ->
                 IIndexedList.removeManyWith EqualityComparer<int>.Default items (lenient ())
                 |> ignore)
        |]

    /// The functions that take a sequence, by name and by the input that they reject, with the parameter that names the
    /// input and the function on it. The shared contracts cover a null sequence and a default flat list given as a
    /// sequence of integers; the rows are a default flat list of strings given as a sequence of objects, which a type test
    /// for the flat list of objects does not see, the order of the checks, and a default flat list as an element of a
    /// sequence of lists: it implements the interface, so it can be one. A default flat list converts to a sequence
    /// without a check and fails when the sequence is enumerated, so the other inputs are valid.
    static let sequenceRejections : Map<string, struct (string * (unit -> unit))> =
        let widenedItems = FlatListOracles.defaultStringsAsObjects
        let objects = IndexedList.ofArray [| "a" :> obj |] :> IIndexedList<obj>
        let defaultItems = Inputs.DefaultFlatListAsSeq<int>()
        let defaultList = Unchecked.defaultof<FlatList<int>> :> IIndexedList<int>

        Map.ofArray [|
            "concat (default flat list as an element)",
            struct ("lists", (fun () -> IIndexedList.concat [| several (); defaultList |] |> ignore))
            "transpose (default flat list as an element)",
            struct ("lists",
                    (fun () ->
                        IIndexedList.transpose [| several (); defaultList |]
                        |> ignore
                    ))
            "insertManyAt (default flat list of strings as objects)",
            struct ("values", (fun () -> IIndexedList.insertManyAt 0 widenedItems objects |> ignore))
            // List.insertManyAt checks the source before the values
            "insertManyAt (null list and default flat list)",
            struct ("source", (fun () -> IIndexedList.insertManyAt 0 defaultItems nullList |> ignore))
            "except (default flat list of strings as objects)",
            struct ("itemsToExclude", (fun () -> IIndexedList.except widenedItems objects |> ignore))
            // List.except checks the items to exclude before the list
            "except (default flat list and null list)",
            struct ("itemsToExclude", (fun () -> IIndexedList.except defaultItems nullList |> ignore))
            "removeMany (default flat list of strings as objects)",
            struct ("items", (fun () -> IIndexedList.removeMany widenedItems objects |> ignore))
            "removeManyWith (default flat list of strings as objects)",
            struct ("items",
                    (fun () ->
                        IIndexedList.removeManyWith EqualityComparer<obj>.Default widenedItems objects
                        |> ignore
                    ))
        |]

    /// The strings of the searches in a range, in an implementation that is not an indexed list.
    static let taggedNames () : IIndexedList<string> = TaggedList (ImmutableList.CreateRange ListNames.Several)

    /// The names of the functions whose result has the element type of their input, as the rows of their test.
    static member SameElementType = sameElementType.Keys

    /// The names of the functions that take the values to remove, as the rows of the test of null values.
    static member NullItemRejections = nullItemRejections.Keys

    /// The names of the rows of the test of the sequences that the functions reject besides the ones of the contracts.
    static member SequenceRejections = sequenceRejections.Keys

    /// The names of the functions that take a list, as the rows of their test.
    static member NullListRejections = nullListRejections.Keys

    [<TestMethod; DynamicData(nameof IIndexedListTests.SameElementType)>]
    member _.``functions with the element type of their input keep its implementation`` (name : string) =
        let apply = sameElementType[name]
        let result = apply (tagged ())
        Assert.IsInstanceOfType<TaggedList<int>>(result, $"IIndexedList.%s{name} must keep the implementation of its input")
        |> ignore

        CollectionAssert.AreEqual (
            IIndexedList.toArray (apply (several ())),
            IIndexedList.toArray result,
            $"IIndexedList.%s{name} must return the same elements for every implementation"
        )

    [<TestMethod>]
    member _.``functions that combine lists or change the element type return an indexed list of the elements`` () =
        let list = tagged ()

        CollectionAssert.AreEqual (
            Array.map string Inputs.Several,
            IndexedList.toArray (IIndexedList.map string list),
            "IIndexedList.map must map the elements of any implementation into an indexed list"
        )

        let concatenated = IIndexedList.concat [| list; several () |]

        CollectionAssert.AreEqual (
            Array.append Inputs.Several Inputs.Several,
            IndexedList.toArray concatenated,
            "IIndexedList.concat must concatenate lists of different implementations into an indexed list"
        )

    [<TestMethod; DynamicData(nameof IIndexedListTests.NullListRejections)>]
    member _.``functions reject a null list`` (name : string) =
        let struct (paramName, action) = nullListRejections[name]
        FlatListAssert.RejectsNull ($"IIndexedList.%s{name}", paramName, action)

    [<TestMethod; DynamicData(nameof IIndexedListTests.SequenceRejections)>]
    member _.``functions that take a sequence reject a default flat list as an element or as a sequence of objects and check their inputs in order``
        (name : string)
        =
        let struct (paramName, actual) = sequenceRejections[name]
        FlatListAssert.RejectsNull ($"IIndexedList.%s{name}", paramName, actual)

    [<TestMethod>]
    member _.``concat and transpose take a flat list that is not default as a list`` () =
        let flat = FlatList.ofArray [| 1; 2 |] :> IIndexedList<int>

        CollectionAssert.AreEqual (
            [| 1; 2; 1; 2 |],
            IndexedList.toArray (IIndexedList.concat [| flat; flat |]),
            "IIndexedList.concat must take the elements of a flat list that is not default"
        )

        let columns = IIndexedList.transpose [| flat; flat |]

        CollectionAssert.AreEqual (
            [| 1; 1 |],
            IndexedList.toArray columns[0],
            "IIndexedList.transpose must take the elements of a flat list that is not default"
        )

    [<TestMethod>]
    member _.``a flat list of strings that is not default passes as a sequence of objects`` () =
        let widened = FlatListOracles.stringsAsObjects (FlatList.ofArray [| "a"; "b" |])
        let asObjects (strings : string array) = strings |> Array.map (fun item -> item :> obj)
        let objects = IndexedList.ofArray (asObjects [| "c" |]) :> IIndexedList<obj>
        let removable = IndexedList.ofArray (asObjects [| "a"; "c"; "b" |]) :> IIndexedList<obj>

        CollectionAssert.AreEqual (
            asObjects [| "a"; "b"; "c" |],
            IIndexedList.toArray (IIndexedList.insertManyAt 0 widened objects),
            "IIndexedList.insertManyAt must take the elements of a flat list of strings as objects"
        )

        CollectionAssert.AreEqual (
            asObjects [| "c" |],
            IIndexedList.toArray (IIndexedList.except widened objects),
            "IIndexedList.except must take the elements of a flat list of strings as objects"
        )

        CollectionAssert.AreEqual (
            asObjects [| "c" |],
            IIndexedList.toArray (IIndexedList.removeMany widened removable),
            "IIndexedList.removeMany must take the elements of a flat list of strings as objects"
        )

        CollectionAssert.AreEqual (
            asObjects [| "c" |],
            IIndexedList.toArray (IIndexedList.removeManyWith EqualityComparer<obj>.Default widened removable),
            "IIndexedList.removeManyWith must take the elements of a flat list of strings as objects"
        )

    [<TestMethod; DynamicData(nameof IIndexedListTests.NullItemRejections)>]
    member _.``removeMany and removeManyWith reject null items with the name of the parameter, also for a list that accepts them``
        (name : string)
        =
        FlatListAssert.RejectsNull ($"IIndexedList.%s{name}", "items", nullItemRejections[name])

    [<TestMethod; DataRow(0, 8); DataRow(1, 3); DataRow(4, 4); DataRow(8, 0)>]
    member _.``indexOfRange and indexOfRangeWith search the range of any implementation`` (startIndex : int, count : int) =
        let list = taggedNames ()

        Assert.AreEqual<int>(
            FlatListOracles.firstInRange (=) startIndex count "a" ListNames.Several,
            IIndexedList.indexOfRange startIndex count "a" list,
            "IIndexedList.indexOfRange must search the range of an implementation that is not an indexed list"
        )

        Assert.AreEqual<int>(
            FlatListOracles.firstInRange FlatListOracles.equalIgnoringCase startIndex count "A" ListNames.Several,
            IIndexedList.indexOfRangeWith StringComparer.OrdinalIgnoreCase startIndex count "A" list,
            "IIndexedList.indexOfRangeWith must search the range of an implementation that is not an indexed list"
        )

    [<TestMethod; DataRow(7, 8); DataRow(3, 2); DataRow(6, 3); DataRow(8, 0)>]
    member _.``lastIndexOfRange and lastIndexOfRangeWith search the range of any implementation``
        (startIndex : int, count : int)
        =
        let list = taggedNames ()

        Assert.AreEqual<int>(
            ListOracles.lastIndexOfRange (=) startIndex count "a" ListNames.Several,
            IIndexedList.lastIndexOfRange startIndex count "a" list,
            "IIndexedList.lastIndexOfRange must search the range of an implementation that is not an indexed list"
        )

        Assert.AreEqual<int>(
            ListOracles.lastIndexOfRange FlatListOracles.equalIgnoringCase startIndex count "A" ListNames.Several,
            IIndexedList.lastIndexOfRangeWith StringComparer.OrdinalIgnoreCase startIndex count "A" list,
            "IIndexedList.lastIndexOfRangeWith must search the range of an implementation that is not an indexed list"
        )

    [<TestMethod>]
    member _.``uncons throws for an empty list as List.head does`` () =
        ExceptionAssert.Same (
            id,
            (fun () -> List.head List.empty<int> |> ignore),
            (fun () -> IIndexedList.uncons (IndexedList.empty<int>) |> ignore),
            "IIndexedList.uncons of an empty list must throw as List.head does"
        )

    [<TestMethod>]
    member _.``cons adds the value before the first element`` () =
        CollectionAssert.AreEqual (
            Array.append [| 99 |] Inputs.Several,
            IIndexedList.toArray (IIndexedList.cons 99 (several ())),
            "IIndexedList.cons must add the value before the first element"
        )

    [<TestMethod>]
    member _.``Cons matches the first element and the other elements`` () =
        let rec sum (list : IIndexedList<int>) =
            match list with
            | IIndexedList.Cons (struct (head, tail)) -> head + sum tail
            | _ -> 0

        Assert.AreEqual<int>(Array.sum Inputs.Several, sum (tagged ()), "IIndexedList.Cons must match every element in turn")

        match tagged () with
        | IIndexedList.Cons (struct (_, tail)) ->
            Assert.IsInstanceOfType<TaggedList<int>>(tail, "IIndexedList.Cons must keep the implementation of the tail")
            |> ignore
        | _ -> Assert.Fail "IIndexedList.Cons must match a list with elements"

    [<TestMethod>]
    member _.``Nil matches an empty list only`` () =
        let describe (list : IIndexedList<int>) =
            match list with
            | IIndexedList.Nil -> "empty"
            | IIndexedList.Cons (struct (head, _)) -> $"starts with %d{head}"
            | _ -> "unreachable"

        Assert.AreEqual<string>("empty", describe (IIndexedList.clear (tagged ())), "IIndexedList.Nil must match an empty list")
        Assert.AreEqual<string>("starts with 5", describe (several ()), "IIndexedList.Nil must not match a list with elements")

    [<TestMethod>]
    member _.``contains compares in F# structural equality, indexOf in the default equality`` () =
        let arrays : IIndexedList<int array> = IndexedList.singleton [| 1; 2 |]

        Assert.IsTrue (
            IIndexedList.contains [| 1; 2 |] arrays,
            "IIndexedList.contains must compare arrays by their elements, as List.contains does"
        )

        Assert.AreEqual<int>(-1, IIndexedList.indexOf [| 1; 2 |] arrays, "IIndexedList.indexOf must compare arrays by reference")

    /// The contracts choose from short lists, which a new array holds; a long list goes through the rented array, and the
    /// selectivities give the results of all lengths, down to a few elements.
    [<TestMethod; DataRow 100; DataRow 50; DataRow 1>]
    member _.``choose of a long list keeps the chosen elements in order`` (percent : int) =
        let items = Array.init 100_000 id
        let list : IIndexedList<int> = IndexedList.ofArray items

        CollectionAssert.AreEqual (
            items
            |> Array.choose (fun x -> if x % 100 < percent then Some (x * 2) else None),
            IIndexedList.choose (fun x -> if x % 100 < percent then ValueSome (x * 2) else ValueNone) list
            |> IIndexedList.toArray,
            $"IIndexedList.choose must keep the chosen elements of a long list in order (%d{percent} %% chosen)"
        )

/// The sequence-like contract for `IIndexedList` over an implementation that is not an indexed list. The functions that
/// take a shortcut for indexed lists go the general way here, and must give the same results.
[<TestClass; IIndexedListTestCategory>]
type IIndexedListTaggedSeqLikeTests () =
    inherit
        SeqLikeContract<IIndexedList<int>> (
            {
                IIndexedListOps.seqLike with
                    Name = "IIndexedList of a tagged list"
                    OfSeq = fun items -> TaggedList (ImmutableList.CreateRange items) :> IIndexedList<int>
            }
        )
