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
    static let underlying (array : FlatList<int>) = ImmutableCollectionsMarshal.AsArray array

    [<TestMethod>]
    member _.``create repeats the value`` () =
        Assert.AreEqual<int list>([ 7; 7; 7 ], FlatList.toList (FlatList.create 3 7), "FlatList.create must repeat the value")
        Assert.IsEmpty (FlatList.create 0 7, "FlatList.create 0 must be empty")

    [<TestMethod>]
    member _.``create throws for a negative count as Array does`` () =
        ExceptionAssert.same
            id
            (fun () -> Array.create -1 7 |> ignore)
            (fun () -> FlatList.create -1 7 |> ignore)
            "FlatList.create -1 must throw as Array.create does"

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

    [<TestMethod>]
    member _.``functions reject a default flat list as Array rejects a null array`` () =
        let cases : struct (string * (unit -> unit) * (unit -> unit)) list = [
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
        ]

        for struct (name, oracle, actual) in cases do
            ExceptionAssert.same id oracle actual $"FlatList.%s{name} must reject a default flat list"

    [<TestMethod>]
    member _.``functions without an Array counterpart reject a default flat list`` () =
        let cases : struct (string * string * (unit -> unit)) list = [
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
            struct (nameof FlatList.skipLast, "array", (fun () -> FlatList.skipLast 0 defaultArray |> ignore))
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
        ]

        for struct (name, paramName, actual) in cases do
            let ex =
                Assert.ThrowsExactly<ArgumentNullException>(Action actual, $"FlatList.%s{name} must reject a default flat list")
            Assert.AreEqual<string | null>(
                paramName,
                ex.ParamName,
                $"FlatList.%s{name} must name the parameter of the default flat list"
            )

    [<TestMethod>]
    member _.``countBy counts null keys like any other key`` () =
        let array = FlatList.ofList Inputs.several
        let keyOf (item : int) = if item > 0 then Some (item % 2) else None

        Assert.AreEqual<struct (int option * int) list>(
            List.countBy keyOf Inputs.several
            |> List.map (fun (key, count) -> struct (key, count)),
            FlatList.countBy keyOf array |> FlatList.toList,
            "FlatList.countBy must count the key None, which is null, as List.countBy does"
        )

    [<TestMethod>]
    member _.``asSpan views the elements`` () =
        let array = FlatList.ofList Inputs.several
        let span = FlatList.asSpan array
        CollectionAssert.AreEqual (List.toArray Inputs.several, span.ToArray (), "FlatList.asSpan must view the elements")

    [<TestMethod>]
    member _.``asMemory views the underlying array without a copy`` () =
        let array = FlatList.ofList Inputs.several
        let memory = FlatList.asMemory array
        let isArray, segment = MemoryMarshal.TryGetArray memory
        Assert.IsTrue (isArray, "FlatList.asMemory must view an array")
        Assert.AreSame (underlying array, segment.Array, "FlatList.asMemory must view the underlying array")
        Assert.HasCount (Inputs.several.Length, segment, "FlatList.asMemory must view every element")

    [<TestMethod>]
    member _.``binarySearch finds a value or the complement of its insertion point`` () =
        let array = FlatList.ofList [ 1; 3; 5; 7 ]
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

    [<TestMethod>]
    member _.``binarySearch uses the order of sort, which is ordinal for strings`` () =
        let array = FlatList.ofList [ "b"; "A"; "a"; "B" ] |> FlatList.sort
        Assert.AreEqual<string list>([ "A"; "B"; "a"; "b" ], FlatList.toList array, "FlatList.sort must order strings ordinally")

        for index in 0 .. array.Length - 1 do
            Assert.AreEqual<int>(
                index,
                FlatList.binarySearch array[index] array,
                $"FlatList.binarySearch must find %s{array[index]}"
            )

    [<TestMethod>]
    member _.``blit copies a range into an array`` () =
        let target = Array.zeroCreate<int> 5
        FlatList.blit (FlatList.ofList Inputs.several) 2 target 1 3
        CollectionAssert.AreEqual ([| 0; 8; 5; 0; 0 |], target, "FlatList.blit must copy three elements from index 2 to index 1")

    [<TestMethod>]
    member _.``blit throws for a range out of range as Array does`` () =
        let source = List.toArray Inputs.several

        ExceptionAssert.same
            id
            (fun () -> Array.blit source 6 (Array.zeroCreate 5) 0 3)
            (fun () -> FlatList.blit (FlatList.ofArray source) 6 (Array.zeroCreate 5) 0 3)
            "FlatList.blit beyond the source must throw as Array.blit does"

    [<TestMethod>]
    member _.``ofSeq of a flat list shares its storage`` () =
        let array = FlatList.ofList Inputs.several
        Assert.AreSame (underlying array, underlying (FlatList.ofSeq array), "FlatList.ofSeq of a flat list must not copy it")

    [<TestMethod>]
    member _.``range functions return the input itself for the whole range`` () =
        let array = FlatList.ofList Inputs.several

        let results = [
            nameof FlatList.take, FlatList.take 8 array
            nameof FlatList.takeLast, FlatList.takeLast 8 array
            nameof FlatList.takeWhile, FlatList.takeWhile (fun _ -> true) array
            nameof FlatList.truncate, FlatList.truncate 100 array
            nameof FlatList.skip, FlatList.skip -1 array
            nameof FlatList.skipLast, FlatList.skipLast 0 array
            nameof FlatList.skipWhile, FlatList.skipWhile (fun _ -> false) array
        ]

        for name, result in results do
            Assert.AreSame (underlying array, underlying result, $"FlatList.%s{name} of the whole range must return the input")

    [<TestMethod>]
    member _.``range functions copy a partial range`` () =
        let array = FlatList.ofList Inputs.several
        let struct (before, after) = FlatList.splitAt 3 array

        for name, result in
            [
                nameof FlatList.take, FlatList.take 3 array
                nameof FlatList.skip, FlatList.skip 3 array
                nameof FlatList.splitAt, before
                nameof FlatList.splitAt, after
            ] do
            Assert.AreNotSame (underlying array, underlying result, $"FlatList.%s{name} of a partial range must copy it")

    [<TestMethod>]
    member _.``append returns the other flat list when one is empty`` () =
        let array = FlatList.ofList Inputs.several
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
        let array = FlatList.ofList [ 1; 2; 3 ]
        Assert.AreSame (
            underlying array,
            underlying (FlatList.sortWithComparer Comparer<int>.Default array),
            "FlatList.sortWithComparer must not copy a sorted flat list"
        )
