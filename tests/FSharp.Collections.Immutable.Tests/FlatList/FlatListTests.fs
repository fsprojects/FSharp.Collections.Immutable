namespace FSharp.Collections.Immutable.Tests

open System
open System.Collections.Generic
open System.Runtime.InteropServices
open FSharp.Collections.Immutable
open Microsoft.VisualStudio.TestTools.UnitTesting

/// The behaviour of `FlatList` that the shared contracts do not cover: default values, shared storage and the extras.
[<TestClass; FlatListTestCategory>]
type FlatListTests () =

    static let defaultArray = Unchecked.defaultof<FlatList<int>>

    static let nullArray = Unchecked.defaultof<int array>

    /// The array under a flat list, to tell whether two flat lists share their storage.
    static let underlying (array : FlatList<'T>) = ImmutableCollectionsMarshal.AsArray array

    /// Asserts that an action rejects its input with an ArgumentNullException that names the parameter.
    static let assertRejects (name : string) (paramName : string) (actual : unit -> unit) =
        let ex =
            Assert.ThrowsExactly<ArgumentNullException>(Action actual, $"FlatList.%s{name} must throw ArgumentNullException")

        Assert.AreEqual<string | null>(
            paramName,
            ex.ParamName,
            $"FlatList.%s{name} must name the parameter of the rejected input"
        )

    /// Maps the parameter names of the Array oracles to the ones of the flat list functions that name a sequence of
    /// values: Array.insertManyAt names its values source, as Seq.toArray does, where the flat list function names them values.
    static let valuesParamName (name : string) =
        match name with
        | "source" -> "values"
        | other -> other

    /// Maps the parameter names of Array.sub to the ones of FlatList.sub, which names the flat list list.
    static let subParamName (name : string) =
        match name with
        | "array" -> "list"
        | other -> other

    /// The elements without the first one that the equality finds equal to the item: the oracle of the removal by value.
    static let removeFirst (equal : 'T -> 'T -> bool) (item : 'T) (elements : 'T array) =
        match Array.tryFindIndex (equal item) elements with
        | Some index -> Array.removeAt index elements
        | None -> elements

    /// Tells whether two strings are equal when the case is ignored.
    static let equalIgnoringCase (left : string) (right : string) =
        String.Equals (left, right, StringComparison.OrdinalIgnoreCase)

    /// The strings of the removal by value; "b" and "B" are equal when the case is ignored.
    static let names () = [| "a"; "b"; "B"; "c" |]

    /// The functions that mirror an Array function, by name, with that function on a null array and the function on a
    /// default flat list. The rows of the test are the names: a function has no readable name in a row.
    static let arrayRejections =
        [|
            struct (nameof FlatList.allPairs,
                    (fun () -> Array.allPairs nullArray [| 1 |] |> ignore),
                    (fun () ->
                        FlatList.allPairs defaultArray (FlatList.singleton 1)
                        |> ignore
                    ))
            struct (nameof FlatList.append,
                    (fun () -> Array.append [| 1 |] nullArray |> ignore),
                    (fun () ->
                        FlatList.append (FlatList.singleton 1) defaultArray
                        |> ignore
                    ))
            struct (nameof FlatList.average,
                    (fun () -> Array.average (Unchecked.defaultof<float array>) |> ignore),
                    (fun () ->
                        FlatList.average (Unchecked.defaultof<FlatList<float>>)
                        |> ignore
                    ))
            struct (nameof FlatList.choose,
                    (fun () -> Array.choose Some nullArray |> ignore),
                    (fun () -> FlatList.choose ValueSome defaultArray |> ignore))
            struct (nameof FlatList.chunkBySize,
                    (fun () -> Array.chunkBySize 2 nullArray |> ignore),
                    (fun () -> FlatList.chunkBySize 2 defaultArray |> ignore))
            struct (nameof FlatList.contains,
                    (fun () -> Array.contains 1 nullArray |> ignore),
                    (fun () -> FlatList.contains 1 defaultArray |> ignore))
            struct (nameof FlatList.countBy,
                    (fun () -> Array.countBy id nullArray |> ignore),
                    (fun () -> FlatList.countBy id defaultArray |> ignore))
            struct (nameof FlatList.distinct,
                    (fun () -> Array.distinct nullArray |> ignore),
                    (fun () -> FlatList.distinct defaultArray |> ignore))
            struct (nameof FlatList.exactlyOne,
                    (fun () -> Array.exactlyOne nullArray |> ignore),
                    (fun () -> FlatList.exactlyOne defaultArray |> ignore))
            struct (nameof FlatList.except,
                    (fun () -> Array.except [ 1 ] nullArray |> ignore),
                    (fun () -> FlatList.except [ 1 ] defaultArray |> ignore))
            struct (nameof FlatList.exists,
                    (fun () -> Array.exists ((=) 1) nullArray |> ignore),
                    (fun () -> FlatList.exists ((=) 1) defaultArray |> ignore))
            struct (nameof FlatList.filter,
                    (fun () -> Array.filter ((=) 1) nullArray |> ignore),
                    (fun () -> FlatList.filter ((=) 1) defaultArray |> ignore))
            struct (nameof FlatList.fold,
                    (fun () -> Array.fold (+) 0 nullArray |> ignore),
                    (fun () -> FlatList.fold (+) 0 defaultArray |> ignore))
            struct (nameof FlatList.groupBy,
                    (fun () -> Array.groupBy id nullArray |> ignore),
                    (fun () -> FlatList.groupBy id defaultArray |> ignore))
            struct (nameof FlatList.head,
                    (fun () -> Array.head nullArray |> ignore),
                    (fun () -> FlatList.head defaultArray |> ignore))
            struct (nameof FlatList.indexed,
                    (fun () -> Array.indexed nullArray |> ignore),
                    (fun () -> FlatList.indexed defaultArray |> ignore))
            struct (nameof FlatList.insertAt,
                    (fun () -> Array.insertAt 0 1 nullArray |> ignore),
                    (fun () -> FlatList.insertAt 0 1 defaultArray |> ignore))
            struct (nameof FlatList.isEmpty,
                    (fun () -> Array.isEmpty nullArray |> ignore),
                    (fun () -> FlatList.isEmpty defaultArray |> ignore))
            struct (nameof FlatList.iter, (fun () -> Array.iter ignore nullArray), (fun () -> FlatList.iter ignore defaultArray))
            struct (nameof FlatList.map,
                    (fun () -> Array.map id nullArray |> ignore),
                    (fun () -> FlatList.map id defaultArray |> ignore))
            struct (nameof FlatList.mapFold,
                    (fun () -> Array.mapFold (fun s x -> x, s) 0 nullArray |> ignore),
                    (fun () ->
                        FlatList.mapFold (fun s x -> struct (x, s)) 0 defaultArray
                        |> ignore
                    ))
            struct (nameof FlatList.max,
                    (fun () -> Array.max nullArray |> ignore),
                    (fun () -> FlatList.max defaultArray |> ignore))
            struct (nameof FlatList.pairwise,
                    (fun () -> Array.pairwise nullArray |> ignore),
                    (fun () -> FlatList.pairwise defaultArray |> ignore))
            struct (nameof FlatList.partition,
                    (fun () -> Array.partition ((=) 1) nullArray |> ignore),
                    (fun () -> FlatList.partition ((=) 1) defaultArray |> ignore))
            struct (nameof FlatList.pick,
                    (fun () -> Array.pick Some nullArray |> ignore),
                    (fun () -> FlatList.pick ValueSome defaultArray |> ignore))
            struct (nameof FlatList.randomChoice,
                    (fun () -> Array.randomChoice nullArray |> ignore),
                    (fun () -> FlatList.randomChoice defaultArray |> ignore))
            struct (nameof FlatList.rev,
                    (fun () -> Array.rev nullArray |> ignore),
                    (fun () -> FlatList.rev defaultArray |> ignore))
            struct (nameof FlatList.skip,
                    (fun () -> Array.skip 0 nullArray |> ignore),
                    (fun () -> FlatList.skip 0 defaultArray |> ignore))
            struct (nameof FlatList.sort,
                    (fun () -> Array.sort nullArray |> ignore),
                    (fun () -> FlatList.sort defaultArray |> ignore))
            struct (nameof FlatList.splitAt,
                    (fun () -> Array.splitAt 0 nullArray |> ignore),
                    (fun () -> FlatList.splitAt 0 defaultArray |> ignore))
            struct (nameof FlatList.sum,
                    (fun () -> Array.sum nullArray |> ignore),
                    (fun () -> FlatList.sum defaultArray |> ignore))
            struct (nameof FlatList.tail,
                    (fun () -> Array.tail nullArray |> ignore),
                    (fun () -> FlatList.tail defaultArray |> ignore))
            struct (nameof FlatList.take,
                    (fun () -> Array.take 0 nullArray |> ignore),
                    (fun () -> FlatList.take 0 defaultArray |> ignore))
            struct (nameof FlatList.toList,
                    (fun () -> Array.toList nullArray |> ignore),
                    (fun () -> FlatList.toList defaultArray |> ignore))
            struct (nameof FlatList.tryFind,
                    (fun () -> Array.tryFind ((=) 1) nullArray |> ignore),
                    (fun () -> FlatList.tryFind ((=) 1) defaultArray |> ignore))
            struct (nameof FlatList.tryHead,
                    (fun () -> Array.tryHead nullArray |> ignore),
                    (fun () -> FlatList.tryHead defaultArray |> ignore))
            struct (nameof FlatList.unzip,
                    (fun () ->
                        Array.unzip (Unchecked.defaultof<(int * int) array>)
                        |> ignore
                    ),
                    (fun () ->
                        FlatList.unzip (Unchecked.defaultof<FlatList<struct (int * int)>>)
                        |> ignore
                    ))
            struct (nameof FlatList.windowed,
                    (fun () -> Array.windowed 2 nullArray |> ignore),
                    (fun () -> FlatList.windowed 2 defaultArray |> ignore))
            struct (nameof FlatList.zip,
                    (fun () -> Array.zip [| 1 |] nullArray |> ignore),
                    (fun () -> FlatList.zip (FlatList.singleton 1) defaultArray |> ignore))
        |]
        |> Array.map (fun struct (name, oracle, actual) -> name, struct (oracle, actual))
        |> Map.ofArray

    /// The functions without an Array counterpart, by name, with the parameter that names the default flat list that they
    /// reject and the function on a default flat list.
    static let otherRejections =
        [|
            struct (nameof FlatList.add, "array", (fun () -> FlatList.add 1 defaultArray |> ignore))
            struct (nameof FlatList.asMemory, "array", (fun () -> FlatList.asMemory defaultArray |> ignore))
            struct (nameof FlatList.binarySearch, "array", (fun () -> FlatList.binarySearch 1 defaultArray |> ignore))
            struct (nameof FlatList.blit, "source", (fun () -> FlatList.blit defaultArray 0 [| 0 |] 0 0))
            // Array.concat dereferences a null element array without a check; a default element is a default flat list (D6)
            struct (nameof FlatList.concat, "arrays", (fun () -> FlatList.concat [ defaultArray ] |> ignore))
            struct (nameof FlatList.transpose, "arrays", (fun () -> FlatList.transpose [ defaultArray ] |> ignore))
            // Array.collect dereferences a null result of the mapping; a default result is rejected as in concat
            struct (nameof FlatList.collect,
                    "mapping",
                    (fun () ->
                        FlatList.collect (fun _ -> defaultArray) (FlatList.singleton 1)
                        |> ignore
                    ))
            // A default flat list passed as a sequence is still a default flat list
            struct (nameof FlatList.ofSeq, "source", (fun () -> FlatList.ofSeq (defaultArray :> seq<int>) |> ignore))
            struct (nameof FlatList.indexOf, "array", (fun () -> FlatList.indexOf 1 defaultArray |> ignore))
            struct (nameof FlatList.indexOfWith,
                    "array",
                    (fun () ->
                        FlatList.indexOfWith EqualityComparer<int>.Default 1 defaultArray
                        |> ignore
                    ))
            struct (nameof FlatList.lastIndexOf, "array", (fun () -> FlatList.lastIndexOf 1 defaultArray |> ignore))
            struct (nameof FlatList.lastIndexOfWith,
                    "array",
                    (fun () ->
                        FlatList.lastIndexOfWith EqualityComparer<int>.Default 1 defaultArray
                        |> ignore
                    ))
            struct (nameof FlatList.length, "array", (fun () -> FlatList.length defaultArray |> ignore))
            struct (nameof FlatList.remove, "list", (fun () -> FlatList.remove 1 defaultArray |> ignore))
            struct (nameof FlatList.removeMany, "list", (fun () -> FlatList.removeMany [| 1 |] defaultArray |> ignore))
            struct (nameof FlatList.removeManyWith,
                    "list",
                    (fun () ->
                        FlatList.removeManyWith EqualityComparer<int>.Default [| 1 |] defaultArray
                        |> ignore
                    ))
            struct (nameof FlatList.removeWith,
                    "list",
                    (fun () ->
                        FlatList.removeWith EqualityComparer<int>.Default 1 defaultArray
                        |> ignore
                    ))
            struct (nameof FlatList.skipLast, "array", (fun () -> FlatList.skipLast 0 defaultArray |> ignore))
            struct (nameof FlatList.sortRange, "list", (fun () -> FlatList.sortRange 0 0 defaultArray |> ignore))
            struct (nameof FlatList.sortRangeWith, "list", (fun () -> FlatList.sortRangeWith compare 0 0 defaultArray |> ignore))
            struct (nameof FlatList.sortRangeWithComparer,
                    "list",
                    (fun () ->
                        FlatList.sortRangeWithComparer Comparer<int>.Default 0 0 defaultArray
                        |> ignore
                    ))
            struct (nameof FlatList.sortWithComparer,
                    "array",
                    (fun () ->
                        FlatList.sortWithComparer Comparer<int>.Default defaultArray
                        |> ignore
                    ))
            struct (nameof FlatList.takeLast, "array", (fun () -> FlatList.takeLast 0 defaultArray |> ignore))
            struct (nameof FlatList.toBuilder, "array", (fun () -> FlatList.toBuilder defaultArray |> ignore))
            struct (nameof FlatList.toSeq, "array", (fun () -> FlatList.toSeq defaultArray |> ignore))
            struct (nameof FlatList.tryTail, "array", (fun () -> FlatList.tryTail defaultArray |> ignore))
            struct (nameof FlatList.update, "array", (fun () -> FlatList.update ignore defaultArray |> ignore))
        |]
        |> Array.map (fun struct (name, paramName, actual) -> name, struct (paramName, actual))
        |> Map.ofArray

    /// The functions that take a sequence, by name and by the input that they reject, with the parameter that names the
    /// input and the function on it. A default flat list converts to a sequence without a check and fails when the
    /// sequence is enumerated, so the other inputs are valid and the function would enumerate it.
    static let sequenceRejections =
        let defaultOuter = Unchecked.defaultof<FlatList<FlatList<int>>>
        let nullItems = Unchecked.defaultof<seq<int>>

        [|
            struct ($"%s{nameof FlatList.concat} (default outer)",
                    "arrays",
                    (fun () -> FlatList.concat (defaultOuter :> seq<_>) |> ignore))
            struct ($"%s{nameof FlatList.transpose} (default outer)",
                    "arrays",
                    (fun () -> FlatList.transpose (defaultOuter :> seq<_>) |> ignore))
            struct ($"%s{nameof FlatList.insertManyAt} (default values)",
                    "values",
                    (fun () ->
                        FlatList.insertManyAt 1 (defaultArray :> seq<_>) (FlatList.ofArray Inputs.Several)
                        |> ignore
                    ))
            // Array.insertManyAt checks the source before the values
            struct ($"%s{nameof FlatList.insertManyAt} (default source and values)",
                    "source",
                    (fun () ->
                        FlatList.insertManyAt 1 (defaultArray :> seq<_>) defaultArray
                        |> ignore
                    ))
            struct ($"%s{nameof FlatList.except} (default items)",
                    "itemsToExclude",
                    (fun () ->
                        FlatList.except (defaultArray :> seq<_>) (FlatList.ofArray Inputs.Several)
                        |> ignore
                    ))
            // Array.except returns an empty input without enumerating the items, but it checks the items for null first
            struct ($"%s{nameof FlatList.except} (default items, empty array)",
                    "itemsToExclude",
                    (fun () ->
                        FlatList.except (defaultArray :> seq<_>) FlatList.empty<int>
                        |> ignore
                    ))
            // Array.except checks the items to exclude before the array
            struct ($"%s{nameof FlatList.except} (default items and array)",
                    "itemsToExclude",
                    (fun () ->
                        FlatList.except (defaultArray :> seq<_>) defaultArray
                        |> ignore
                    ))
            struct ($"%s{nameof FlatList.removeMany} (default items)",
                    "items",
                    (fun () ->
                        FlatList.removeMany (defaultArray :> seq<_>) (FlatList.ofArray Inputs.Several)
                        |> ignore
                    ))
            // ImmutableArray.RemoveRange checks the flat list before the items
            struct ($"%s{nameof FlatList.removeMany} (default list and items)",
                    "list",
                    (fun () ->
                        FlatList.removeMany (defaultArray :> seq<_>) defaultArray
                        |> ignore
                    ))
            struct ($"%s{nameof FlatList.removeMany} (null items)",
                    "items",
                    (fun () ->
                        FlatList.removeMany nullItems (FlatList.ofArray Inputs.Several)
                        |> ignore
                    ))
            struct ($"%s{nameof FlatList.removeManyWith} (default items)",
                    "items",
                    (fun () ->
                        FlatList.removeManyWith
                            EqualityComparer<int>.Default
                            (defaultArray :> seq<_>)
                            (FlatList.ofArray Inputs.Several)
                        |> ignore
                    ))
            struct ($"%s{nameof FlatList.removeManyWith} (default list and items)",
                    "list",
                    (fun () ->
                        FlatList.removeManyWith EqualityComparer<int>.Default (defaultArray :> seq<_>) defaultArray
                        |> ignore
                    ))
            struct ($"%s{nameof FlatList.removeManyWith} (null items)",
                    "items",
                    (fun () ->
                        FlatList.removeManyWith EqualityComparer<int>.Default nullItems (FlatList.ofArray Inputs.Several)
                        |> ignore
                    ))
        |]
        |> Array.map (fun struct (name, paramName, actual) -> name, struct (paramName, actual))
        |> Map.ofArray

    /// The functions that take a sequence and mirror an Array function, by name, with that function on a null sequence and
    /// the function on a null sequence. The other inputs are valid.
    static let nullSequenceMirrors =
        let nullArrays = Unchecked.defaultof<seq<int array>>
        let nullOuter = Unchecked.defaultof<seq<FlatList<int>>>
        let nullItems = Unchecked.defaultof<seq<int>>

        [|
            struct (nameof FlatList.concat,
                    (fun () -> Array.concat nullArrays |> ignore),
                    (fun () -> FlatList.concat nullOuter |> ignore))
            struct (nameof FlatList.transpose,
                    (fun () -> Array.transpose nullArrays |> ignore),
                    (fun () -> FlatList.transpose nullOuter |> ignore))
            struct (nameof FlatList.except,
                    (fun () -> Array.except nullItems Inputs.Several |> ignore),
                    (fun () ->
                        FlatList.except nullItems (FlatList.ofArray Inputs.Several)
                        |> ignore
                    ))
            struct (nameof FlatList.insertManyAt,
                    (fun () -> Array.insertManyAt 1 nullItems Inputs.Several |> ignore),
                    (fun () ->
                        FlatList.insertManyAt 1 nullItems (FlatList.ofArray Inputs.Several)
                        |> ignore
                    ))
        |]
        |> Array.map (fun struct (name, oracle, actual) -> name, struct (oracle, actual))
        |> Map.ofArray

    /// The functions that sort a range, by name, with the function and the sort of the range in the oracle. The inputs
    /// have no two different elements with the same absolute value, and the comparer of `sortRangeWithComparer` orders by
    /// the absolute value, so the result does not depend on the stability of the sort.
    static let rangeSorts : Map<string, struct ((int -> int -> FlatList<int> -> FlatList<int>) * (int array -> int array))> =
        let descending (left : int) (right : int) = compare right left
        let byAbsolute (left : int) (right : int) = compare (abs left) (abs right)

        Map.ofArray [|
            nameof FlatList.sortRange, struct (FlatList.sortRange, Array.sortWith compare)
            nameof FlatList.sortRangeWith, struct (FlatList.sortRangeWith descending, Array.sortWith descending)
            nameof FlatList.sortRangeWithComparer,
            struct (FlatList.sortRangeWithComparer (Comparer<int>.Create(Comparison<int> byAbsolute)), Array.sortWith byAbsolute)
        |]

    /// The functions that take a range, by name, on the whole range of the input with several elements.
    static let wholeRanges : Map<string, FlatList<int> -> FlatList<int>> =
        Map.ofArray [|
            nameof FlatList.take, FlatList.take 8
            nameof FlatList.takeLast, FlatList.takeLast 8
            nameof FlatList.takeWhile, FlatList.takeWhile (fun _ -> true)
            nameof FlatList.truncate, FlatList.truncate 100
            nameof FlatList.skip, FlatList.skip -1
            nameof FlatList.skipLast, FlatList.skipLast 0
            nameof FlatList.skipWhile, FlatList.skipWhile (fun _ -> false)
            nameof FlatList.sub, (fun array -> FlatList.sub array 0 array.Length)
        |]

    /// The functions that take a range, by name, on a partial range of the input with several elements.
    static let partialRanges : Map<string, FlatList<int> -> FlatList<int>> =
        Map.ofArray [|
            nameof FlatList.take, FlatList.take 3
            nameof FlatList.skip, FlatList.skip 3
            nameof FlatList.sub, (fun array -> FlatList.sub array 3 3)
            ($"%s{nameof FlatList.splitAt} (first part)",
             fun array ->
                 let struct (before, _) = FlatList.splitAt 3 array
                 before)
            ($"%s{nameof FlatList.splitAt} (second part)",
             fun array ->
                 let struct (_, after) = FlatList.splitAt 3 array
                 after)
        |]

    /// Strings whose ordinal order differs from their order in the current culture.
    static let strings () = [| "b"; "A"; "a"; "B" |]

    /// The names of the functions that mirror an Array function, as the rows of their test.
    static member ArrayRejections = arrayRejections.Keys

    /// The names of the functions without an Array counterpart, as the rows of their test.
    static member OtherRejections = otherRejections.Keys

    /// The names of the functions that take a sequence, as the rows of the test of the sequences that they reject.
    static member SequenceRejections = sequenceRejections.Keys

    /// The names of the functions that take a sequence and mirror an Array function, as the rows of the test of a null sequence.
    static member NullSequenceMirrors = nullSequenceMirrors.Keys

    /// Every range of every input, as the index of its first element and its length.
    static member SubRanges = seq {
        for input in Inputs.All do
            for startIndex in 0 .. input.Length do
                for count in 0 .. input.Length - startIndex do
                    struct (input, startIndex, count)
    }

    /// Every input with ranges that Array.sub rejects: a negative start or count, a count beyond the length, and a start
    /// beyond the length or at its end with an element to take.
    static member SubRangesOutOfRange = seq {
        for input in Inputs.All do
            let ranges =
                [|
                    struct (-1, 0)
                    struct (0, -1)
                    struct (-1, -1)
                    struct (0, input.Length + 1)
                    struct (input.Length + 1, 0)
                    struct (input.Length, 1)
                    struct (1, input.Length)
                |]
                |> Array.distinct

            for struct (startIndex, count) in ranges do
                struct (input, startIndex, count)
    }

    /// The ranges that the functions sort: none, one element, all of them, a prefix, a suffix and the inside of the input
    /// with several elements, and the ranges of the smaller inputs.
    static member SortRanges = seq {
        let ranges = [|
            struct (Inputs.Empty, 0, 0)
            struct (Inputs.Singleton, 0, 0)
            struct (Inputs.Singleton, 0, 1)
            struct (Inputs.Singleton, 1, 0)
            struct (Inputs.Several, 0, 0)
            struct (Inputs.Several, 0, 1)
            struct (Inputs.Several, 0, 8)
            struct (Inputs.Several, 8, 0)
            struct (Inputs.Several, 4, 0)
            struct (Inputs.Several, 0, 3)
            struct (Inputs.Several, 2, 4)
            struct (Inputs.Several, 5, 3)
            struct (Inputs.Several, 7, 1)
        |]

        for name in rangeSorts.Keys do
            for struct (input, index, count) in ranges do
                struct (name, input, index, count)
    }

    /// The ranges of the input with several elements that the sorts reject, with the parameter that the BCL names.
    static member SortRangesOutOfRange = seq {
        let ranges = [|
            struct (-1, 1, "index")
            struct (0, -1, "count")
            struct (0, 9, "count")
            struct (5, 4, "count")
            struct (9, 0, "count")
        |]

        for name in rangeSorts.Keys do
            for struct (index, count, paramName) in ranges do
                struct (name, index, count, paramName)
    }

    /// The ranges of the input with several elements that need no sorting: they have at most one element.
    static member SortRangesWithoutSorting = seq {
        for name in rangeSorts.Keys do
            for struct (index, count) in [| struct (0, 0); struct (2, 1); struct (8, 0) |] do
                struct (name, index, count)
    }

    /// Every input with each value to remove: the first of two equal elements, the only element, the last element, and
    /// a value that no input has.
    static member RemovalValues = seq {
        for input in Inputs.All do
            for value in [| 42; 5; -3; 8; 7; 1000 |] do
                struct (input, value)
    }

    /// Every input with values that none of them has.
    static member AbsentValues = seq {
        for input in Inputs.All do
            for value in [| 1000; 99 |] do
                struct (input, value)
    }

    /// Every input with the values to remove: none, one, a value as often as the input with several elements has it and
    /// more often, and several values of which some are missing.
    static member RemovalItems = seq {
        for input in Inputs.All do
            for items in [| [||]; [| 5 |]; [| 5; 5 |]; [| 5; 5; 5 |]; [| -3; 5; 1000 |]; [| 7; 5; -3; 8; 5; 0 |] |] do
                struct (input, items)
    }

    /// Every input with sequences of values that none of them has.
    static member AbsentItems = seq {
        for input in Inputs.All do
            for items in [| [||]; [| 1000 |]; [| 1000; 99 |] |] do
                struct (input, items)
    }

    /// The values to remove from the strings with a case-insensitive comparer: equal in the same case, equal in another
    /// case, and missing.
    static member NameValues = seq {
        "a"
        "b"
        "B"
        "C"
        "d"
    }

    /// The sequences of values to remove from the strings with a case-insensitive comparer.
    static member NameItems : seq<TestDataRow<string array>> = seq {
        TestDataRow Array.empty<string>
        TestDataRow [| "B" |]
        TestDataRow [| "b"; "B" |]
        TestDataRow [| "B"; "b"; "b" |]
        TestDataRow [| "C"; "d"; "A" |]
    }

    /// The names of the functions that take a range, as the rows of the test of the whole range.
    static member WholeRanges = wholeRanges.Keys

    /// The names of the functions that take a range, as the rows of the test of a partial range.
    static member PartialRanges = partialRanges.Keys

    /// Every index of the strings that binarySearch searches.
    static member StringIndices = seq { 0 .. (strings ()).Length - 1 }

    [<TestMethod>]
    member _.``create repeats the value`` () =
        CollectionAssert.AreEqual ([| 7; 7; 7 |], FlatList.toArray (FlatList.create 3 7), "FlatList.create must repeat the value")
        Assert.IsEmpty (FlatList.create 0 7, "FlatList.create 0 must be empty")

    [<TestMethod>]
    member _.``create throws for a negative count as Array does`` () =
        ExceptionAssert.Same (
            id,
            (fun () -> Array.create -1 7 |> ignore),
            (fun () -> FlatList.create -1 7 |> ignore),
            "FlatList.create -1 must throw as Array.create does"
        )

    [<TestMethod>]
    member _.``isDefault and isDefaultOrEmpty tell a default value from an empty one`` () =
        Assert.IsTrue (FlatList.isDefault defaultArray, "isDefault must be true for a default value")
        Assert.IsFalse (FlatList.isDefault FlatList.empty<int>, "isDefault must be false for an empty flat list")
        Assert.IsTrue (FlatList.isDefaultOrEmpty defaultArray, "isDefaultOrEmpty must be true for a default value")
        Assert.IsTrue (FlatList.isDefaultOrEmpty FlatList.empty<int>, "isDefaultOrEmpty must be true for an empty flat list")
        Assert.IsFalse (
            FlatList.isDefaultOrEmpty (FlatList.singleton 1),
            "isDefaultOrEmpty must be false for a non-empty flat list"
        )

    [<TestMethod; DynamicData(nameof FlatListTests.ArrayRejections)>]
    member _.``functions reject a default flat list as Array rejects a null array`` (name : string) =
        let struct (oracle, actual) = arrayRejections[name]
        ExceptionAssert.Same (id, oracle, actual, $"FlatList.%s{name} must reject a default flat list")

    [<TestMethod; DynamicData(nameof FlatListTests.OtherRejections)>]
    member _.``functions without an Array counterpart reject a default flat list`` (name : string) =
        let struct (paramName, actual) = otherRejections[name]
        assertRejects name paramName actual

    [<TestMethod; DynamicData(nameof FlatListTests.SequenceRejections)>]
    member _.``functions that take a sequence reject a default flat list given as the sequence and a null sequence``
        (name : string)
        =
        let struct (paramName, actual) = sequenceRejections[name]
        assertRejects name paramName actual

    [<TestMethod; DynamicData(nameof FlatListTests.NullSequenceMirrors)>]
    member _.``functions that take a sequence reject a null sequence as Array does`` (name : string) =
        let struct (oracle, actual) = nullSequenceMirrors[name]

        ExceptionAssert.Same (
            valuesParamName,
            oracle,
            actual,
            $"FlatList.%s{name} must reject a null sequence as Array.%s{name} does"
        )

    [<TestMethod; DataRow(-1, "null"); DataRow(9, "null"); DataRow(-1, "default"); DataRow(9, "default")>]
    member _.``insertManyAt rejects an index out of range before the values as Array.insertManyAt does``
        (index : int, kind : string)
        =
        // The input has 8 elements, so the indices -1 and 9 are out of range. Array.insertManyAt checks the index
        // before it enumerates the values, and the default flat list is as good as a null sequence for the oracle
        let values =
            match kind with
            | "null" -> Unchecked.defaultof<seq<int>>
            | _ -> defaultArray :> seq<_>

        ExceptionAssert.Same (
            id,
            (fun () -> Array.insertManyAt index values Inputs.Several |> ignore),
            (fun () ->
                FlatList.insertManyAt index values (FlatList.ofArray Inputs.Several)
                |> ignore
            ),
            "FlatList.insertManyAt must check the index before the values as Array.insertManyAt does"
        )

    [<TestMethod>]
    member _.``countBy counts null keys like any other key`` () =
        let array = FlatList.ofArray Inputs.Several
        let keyOf (item : int) = if item > 0 then Some (item % 2) else None

        CollectionAssert.AreEqual (
            Array.countBy keyOf Inputs.Several
            |> Array.map (fun (key, count) -> struct (key, count)),
            FlatList.countBy keyOf array |> FlatList.toArray,
            "FlatList.countBy must count the key None, which is null, as Array.countBy does"
        )

    [<TestMethod>]
    member _.``asSpan views the elements`` () =
        let array = FlatList.ofArray Inputs.Several
        let span = FlatList.asSpan array
        CollectionAssert.AreEqual (Inputs.Several, span.ToArray (), "FlatList.asSpan must view the elements")

    [<TestMethod>]
    member _.``asMemory views the underlying array without a copy`` () =
        let array = FlatList.ofArray Inputs.Several
        let memory = FlatList.asMemory array
        let isArray, segment = MemoryMarshal.TryGetArray memory
        Assert.IsTrue (isArray, "FlatList.asMemory must view an array")
        Assert.AreSame (underlying array, segment.Array, "FlatList.asMemory must view the underlying array")
        Assert.HasCount (Inputs.Several.Length, segment, "FlatList.asMemory must view every element")

    [<TestMethod>]
    member _.``binarySearch finds a value or the complement of its insertion point`` () =
        let array = FlatList.ofArray [| 1; 3; 5; 7 |]
        Assert.AreEqual<int>(2, FlatList.binarySearch 5 array, "FlatList.binarySearch must find 5 at index 2")
        Assert.AreEqual<int>(
            ~~~2,
            FlatList.binarySearch 4 array,
            "FlatList.binarySearch must return the complement of the insertion point of 4"
        )
        Assert.AreEqual<int>(
            ~~~4,
            FlatList.binarySearch 9 array,
            "FlatList.binarySearch must return the complement of the length for 9"
        )

    [<TestMethod; DynamicData(nameof FlatListTests.StringIndices)>]
    member _.``binarySearch uses the order of sort, which is ordinal for strings`` (index : int) =
        let array = FlatList.ofArray (strings ()) |> FlatList.sort
        CollectionAssert.AreEqual ([| "A"; "B"; "a"; "b" |], FlatList.toArray array, "FlatList.sort must order strings ordinally")

        Assert.AreEqual<int>(
            index,
            FlatList.binarySearch array[index] array,
            "FlatList.binarySearch must find every element of a sorted flat list at its index"
        )

    [<TestMethod>]
    member _.``blit copies a range into an array`` () =
        let target = Array.zeroCreate<int> 5
        FlatList.blit (FlatList.ofArray Inputs.Several) 2 target 1 3
        CollectionAssert.AreEqual ([| 0; 8; 5; 0; 0 |], target, "FlatList.blit must copy three elements from index 2 to index 1")

    [<TestMethod>]
    member _.``blit throws for a range out of range as Array does`` () =
        let source = Inputs.Several

        ExceptionAssert.Same (
            id,
            (fun () -> Array.blit source 6 (Array.zeroCreate 5) 0 3),
            (fun () -> FlatList.blit (FlatList.ofArray source) 6 (Array.zeroCreate 5) 0 3),
            "FlatList.blit beyond the source must throw as Array.blit does"
        )

    [<TestMethod>]
    member _.``ofSeq of a flat list shares its storage`` () =
        let array = FlatList.ofArray Inputs.Several
        Assert.AreSame (underlying array, underlying (FlatList.ofSeq array), "FlatList.ofSeq of a flat list must not copy it")

    [<TestMethod; DynamicData(nameof FlatListTests.WholeRanges)>]
    member _.``range functions return the input itself for the whole range`` (name : string) =
        let array = FlatList.ofArray Inputs.Several

        Assert.AreSame (
            underlying array,
            underlying (wholeRanges[name] array),
            $"FlatList.%s{name} of the whole range must return the input"
        )

    [<TestMethod; DynamicData(nameof FlatListTests.PartialRanges)>]
    member _.``range functions copy a partial range`` (name : string) =
        let array = FlatList.ofArray Inputs.Several

        Assert.AreNotSame (
            underlying array,
            underlying (partialRanges[name] array),
            $"FlatList.%s{name} of a partial range must copy it"
        )

    [<TestMethod>]
    member _.``append returns the other flat list when one is empty`` () =
        let array = FlatList.ofArray Inputs.Several
        Assert.AreSame (
            underlying array,
            underlying (FlatList.append array FlatList.empty),
            "FlatList.append of an empty second flat list must return the first one"
        )
        Assert.AreSame (
            underlying array,
            underlying (FlatList.append FlatList.empty array),
            "FlatList.append of an empty first flat list must return the second one"
        )

    [<TestMethod>]
    member _.``sortWithComparer returns a sorted input itself`` () =
        let array = FlatList.ofArray [| 1; 2; 3 |]
        Assert.AreSame (
            underlying array,
            underlying (FlatList.sortWithComparer Comparer<int>.Default array),
            "FlatList.sortWithComparer must not copy a sorted flat list"
        )

    // ---- sub ----

    [<TestMethod; DynamicData(nameof FlatListTests.SubRanges)>]
    member _.``sub returns the range of the elements as Array.sub does`` (input : int array, startIndex : int, count : int) =
        CollectionAssert.AreEqual (
            Array.sub input startIndex count,
            FlatList.toArray (FlatList.sub (FlatList.ofArray input) startIndex count),
            "FlatList.sub must return the range of the elements as Array.sub does"
        )

    [<TestMethod; DynamicData(nameof FlatListTests.SubRangesOutOfRange)>]
    member _.``sub throws for a range out of range as Array.sub does`` (input : int array, startIndex : int, count : int) =
        ExceptionAssert.Same (
            subParamName,
            (fun () -> Array.sub input startIndex count |> ignore),
            (fun () ->
                FlatList.sub (FlatList.ofArray input) startIndex count
                |> ignore
            ),
            "FlatList.sub must throw as Array.sub does for a range out of range"
        )

    [<TestMethod; DataRow(0, 0); DataRow(-1, 0); DataRow(0, -1); DataRow(9, 9)>]
    member _.``sub rejects a default flat list before the range as Array.sub rejects a null array``
        (startIndex : int, count : int)
        =
        ExceptionAssert.Same (
            subParamName,
            (fun () -> Array.sub nullArray startIndex count |> ignore),
            (fun () -> FlatList.sub defaultArray startIndex count |> ignore),
            "FlatList.sub must reject a default flat list as Array.sub rejects a null array"
        )

    [<TestMethod; DataRow 0; DataRow 3; DataRow 8>]
    member _.``sub returns the empty flat list for an empty range`` (startIndex : int) =
        let array = FlatList.ofArray Inputs.Several

        Assert.AreSame (
            underlying FlatList.empty<int>,
            underlying (FlatList.sub array startIndex 0),
            "FlatList.sub of an empty range must return the empty flat list"
        )

    // ---- sortRange, sortRangeWith and sortRangeWithComparer ----

    [<TestMethod; DynamicData(nameof FlatListTests.SortRanges)>]
    member _.``functions that sort a range sort the range and keep the other elements``
        (name : string, input : int array, index : int, count : int)
        =
        let struct (sortRange, sortPart) = rangeSorts[name]

        let expected =
            Array.concat [|
                Array.sub input 0 index
                sortPart (Array.sub input index count)
                Array.sub input (index + count) (input.Length - index - count)
            |]

        CollectionAssert.AreEqual (
            expected,
            FlatList.toArray (sortRange index count (FlatList.ofArray input)),
            $"FlatList.%s{name} must sort the range and keep the other elements"
        )

    [<TestMethod; DynamicData(nameof FlatListTests.SortRangesOutOfRange)>]
    member _.``functions that sort a range throw for a range out of range as the BCL does``
        (name : string, index : int, count : int, paramName : string)
        =
        let struct (sortRange, _) = rangeSorts[name]

        let ex =
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(
                Action (fun () ->
                    sortRange index count (FlatList.ofArray Inputs.Several)
                    |> ignore
                ),
                $"FlatList.%s{name} must throw ArgumentOutOfRangeException for a range out of range"
            )

        Assert.AreEqual<string | null>(paramName, ex.ParamName, $"FlatList.%s{name} must name the parameter that the BCL names")

    [<TestMethod; DynamicData(nameof FlatListTests.SortRangesWithoutSorting)>]
    member _.``functions that sort a range return the input itself for a range of at most one element``
        (name : string, index : int, count : int)
        =
        let struct (sortRange, _) = rangeSorts[name]
        let array = FlatList.ofArray Inputs.Several

        Assert.AreSame (
            underlying array,
            underlying (sortRange index count array),
            $"FlatList.%s{name} must not copy a range that needs no sorting"
        )

    [<TestMethod>]
    member _.``sortRange returns the input itself when the range is already sorted`` () =
        let array = FlatList.ofArray [| 1; 2; 3; 9; 5 |]

        Assert.AreSame (
            underlying array,
            underlying (FlatList.sortRange 0 3 array),
            "FlatList.sortRange must not copy a range that is already sorted"
        )

    [<TestMethod>]
    member _.``sortRange orders strings ordinally, as F# generic comparison does`` () =
        let array = FlatList.ofArray (strings ())

        CollectionAssert.AreEqual (
            [| "A"; "B"; "a"; "b" |],
            FlatList.toArray (FlatList.sortRange 0 4 array),
            "FlatList.sortRange must sort strings ordinally"
        )

    // ---- remove, removeWith, removeMany and removeManyWith ----

    [<TestMethod; DynamicData(nameof FlatListTests.RemovalValues)>]
    member _.``remove removes the first equal element`` (input : int array, value : int) =
        CollectionAssert.AreEqual (
            removeFirst (=) value input,
            FlatList.toArray (FlatList.remove value (FlatList.ofArray input)),
            "FlatList.remove must remove the first equal element"
        )

    [<TestMethod; DynamicData(nameof FlatListTests.AbsentValues)>]
    member _.``remove returns the input itself when no element equals the value`` (input : int array, value : int) =
        let array = FlatList.ofArray input

        Assert.AreSame (
            underlying array,
            underlying (FlatList.remove value array),
            "FlatList.remove must not copy a flat list that has no equal element"
        )

    [<TestMethod>]
    member _.``remove compares in the default equality, which tells b from B`` () =
        CollectionAssert.AreEqual (
            [| "a"; "b"; "c" |],
            FlatList.toArray (FlatList.remove "B" (FlatList.ofArray (names ()))),
            "FlatList.remove must not find B equal to b"
        )

    [<TestMethod; DynamicData(nameof FlatListTests.NameValues)>]
    member _.``removeWith removes the first element that the comparer finds equal`` (value : string) =
        CollectionAssert.AreEqual (
            removeFirst equalIgnoringCase value (names ()),
            FlatList.toArray (FlatList.removeWith StringComparer.OrdinalIgnoreCase value (FlatList.ofArray (names ()))),
            "FlatList.removeWith must remove the first element that the comparer finds equal"
        )

    [<TestMethod; DataRow "d"; DataRow "D">]
    member _.``removeWith returns the input itself when no element is equal`` (value : string) =
        let array = FlatList.ofArray (names ())

        Assert.AreSame (
            underlying array,
            underlying (FlatList.removeWith StringComparer.OrdinalIgnoreCase value array),
            "FlatList.removeWith must not copy a flat list that has no equal element"
        )

    [<TestMethod; DynamicData(nameof FlatListTests.RemovalItems)>]
    member _.``removeMany removes one element for each value`` (input : int array, items : int array) =
        let expected = Array.fold (fun elements item -> removeFirst (=) item elements) input items

        CollectionAssert.AreEqual (
            expected,
            FlatList.toArray (FlatList.removeMany items (FlatList.ofArray input)),
            "FlatList.removeMany must remove one element for each value"
        )

    [<TestMethod>]
    member _.``removeMany takes the values as any sequence`` () =
        let array = FlatList.ofArray Inputs.Several
        // The values 5, -3 and 5 remove the first 5, the first -3 and the second 5
        let expected = [| 8; 0; -3; 12; 7 |]

        CollectionAssert.AreEqual (
            expected,
            FlatList.toArray (FlatList.removeMany [| 5; -3; 5 |] array),
            "FlatList.removeMany must take the values as an array"
        )

        CollectionAssert.AreEqual (
            expected,
            FlatList.toArray (FlatList.removeMany [ 5; -3; 5 ] array),
            "FlatList.removeMany must take the values as a list"
        )

        CollectionAssert.AreEqual (
            expected,
            FlatList.toArray (FlatList.removeMany (Seq.map id [| 5; -3; 5 |]) array),
            "FlatList.removeMany must take the values as a lazy sequence"
        )

    [<TestMethod; DynamicData(nameof FlatListTests.AbsentItems)>]
    member _.``removeMany returns the input itself when no value is in it`` (input : int array, items : int array) =
        let array = FlatList.ofArray input

        Assert.AreSame (
            underlying array,
            underlying (FlatList.removeMany items array),
            "FlatList.removeMany must not copy a flat list that has none of the values"
        )

    [<TestMethod>]
    member _.``removeMany compares in the default equality, which tells b from B`` () =
        CollectionAssert.AreEqual (
            [| "a"; "b"; "c" |],
            FlatList.toArray (FlatList.removeMany [| "B" |] (FlatList.ofArray (names ()))),
            "FlatList.removeMany must not find B equal to b"
        )

    [<TestMethod; DynamicData(nameof FlatListTests.NameItems)>]
    member _.``removeManyWith removes one element for each value that the comparer finds equal`` (items : string array) =
        let expected =
            Array.fold (fun elements item -> removeFirst equalIgnoringCase item elements) (names ()) items

        CollectionAssert.AreEqual (
            expected,
            FlatList.toArray (FlatList.removeManyWith StringComparer.OrdinalIgnoreCase items (FlatList.ofArray (names ()))),
            "FlatList.removeManyWith must remove one element for each value that the comparer finds equal"
        )
