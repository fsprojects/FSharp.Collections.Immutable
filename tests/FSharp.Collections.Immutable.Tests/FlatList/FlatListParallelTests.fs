namespace FSharp.Collections.Immutable.Tests

open System
open System.Collections.Concurrent
open FSharp.Collections.Immutable
open Microsoft.VisualStudio.TestTools.UnitTesting

/// <summary>The inputs of the tests of <c>FlatList.Parallel</c>, as the cases of their rows.</summary>
/// <remarks>
/// MSTest names a row after its values, and the larger input would write ten thousand numbers into the name of each of
/// its rows, so a row holds a case and the test takes the elements of the case.
/// </remarks>
[<RequireQualifiedAccess>]
type ParallelInput =
    /// The input without elements of the contracts.
    | Empty
    /// The input with one element of the contracts.
    | Singleton
    /// The input with several elements of the contracts.
    | Several
    /// An input long enough for the functions to split their work among several threads.
    | Large

    /// The elements of the input; the test class builds the elements of the larger input.
    member this.Elements (large : unit -> int array) =
        match this with
        | ParallelInput.Empty -> Inputs.Empty
        | ParallelInput.Singleton -> Inputs.Singleton
        | ParallelInput.Several -> Inputs.Several
        | ParallelInput.Large -> large ()

/// <summary>
/// The predicates of the tests of <c>FlatList.Parallel</c>, with the choosers that return a value for the same elements,
/// as the cases of their rows: matches at the start, matches late and rare in the larger input, and no match.
/// </summary>
/// <remarks>
/// MSTest names a row after its values, and a function has no readable name, so a row holds a case and the test takes
/// the function of the case.
/// </remarks>
[<RequireQualifiedAccess>]
type ParallelPredicate =
    /// The even elements.
    | IsEven
    /// The negative elements.
    | IsNegative
    /// The elements above 900, which come late and rarely in the larger input.
    | IsAboveNineHundred
    /// No element.
    | Never

    /// Tells whether an element satisfies the predicate.
    member this.Matches (item : int) =
        match this with
        | ParallelPredicate.IsEven -> item % 2 = 0
        | ParallelPredicate.IsNegative -> item < 0
        | ParallelPredicate.IsAboveNineHundred -> item > 900
        | ParallelPredicate.Never -> false

    /// A chooser that returns a value for the elements that satisfy the predicate.
    member this.Choose (item : int) =
        match this with
        | ParallelPredicate.IsEven -> if item % 2 = 0 then ValueSome (item / 2) else ValueNone
        | ParallelPredicate.IsNegative -> if item < 0 then ValueSome (-item) else ValueNone
        | ParallelPredicate.IsAboveNineHundred -> if item > 900 then ValueSome (item * 2) else ValueNone
        | ParallelPredicate.Never -> ValueNone

/// The `FlatList.Parallel` module, compared with the sequential functions of `FlatList` (ADR 0001, D11).
/// Every function runs on the inputs of the contracts and on a larger input, which is long enough for the functions to split
/// their work among several threads. Results whose order the module leaves open are compared after sorting, and exceptions
/// are compared with those of `Array.Parallel`, which the module mirrors.
[<TestClass; FlatListTestCategory; ParallelTestCategory>]
type FlatListParallelTests () =

    static let defaultArray = Unchecked.defaultof<FlatList<int>>

    static let nullArray = Unchecked.defaultof<int array>

    /// Numbers from -1000 to 999 in a scrambled order, each about five times. 7919 and 10 007 are primes, so the products
    /// modulo 10 007 do not repeat before the last remainder folds them onto 2000 values.
    static let large () = Array.init 10_000 (fun index -> index * 7_919 % 10_007 % 2_000 - 1_000)

    /// The inputs of the contracts and the larger input.
    static let allInputs = [| ParallelInput.Empty; ParallelInput.Singleton; ParallelInput.Several; ParallelInput.Large |]

    /// The predicates of the tests.
    static let predicates = [|
        ParallelPredicate.IsEven
        ParallelPredicate.IsNegative
        ParallelPredicate.IsAboveNineHundred
        ParallelPredicate.Never
    |]

    /// The flat list of the elements of an input.
    static let flatListOf (input : ParallelInput) = FlatList.ofArray (input.Elements large)

    /// Negation is injective, so only equal elements share a key, and the result of a sort does not depend on its stability.
    static let negate (item : int) = -item

    /// The keys of the groups and the elements of every key, sorted, for the comparison of groupings whose order is open.
    static let normalized (groups : FlatList<struct ('Key * FlatList<int>)>) =
        let sorted =
            groups
            |> Seq.map (fun struct (key, items) -> struct (key, items |> Seq.sort |> Seq.toArray))
            |> Seq.sortBy (fun struct (key, _) -> key)
            |> Seq.toArray

        struct (sorted |> Array.map (fun struct (key, _) -> key), sorted |> Array.map (fun struct (_, items) -> items))

    static let firstChoice (item : int) : Choice<int, string> = Choice1Of2 item

    /// The functions that mirror an Array.Parallel function, by name, with that function on a null array and the function
    /// on a default flat list. The rows of the test are the names: a function has no readable name in a row.
    static let arrayRejections =
        [|
            struct (nameof FlatList.Parallel.average,
                    (fun () ->
                        Array.Parallel.average (Unchecked.defaultof<float array>)
                        |> ignore
                    ),
                    (fun () ->
                        FlatList.Parallel.average (Unchecked.defaultof<FlatList<float>>)
                        |> ignore
                    ))
            struct (nameof FlatList.Parallel.averageBy,
                    (fun () -> Array.Parallel.averageBy float nullArray |> ignore),
                    (fun () -> FlatList.Parallel.averageBy float defaultArray |> ignore))
            struct (nameof FlatList.Parallel.choose,
                    (fun () -> Array.Parallel.choose Some nullArray |> ignore),
                    (fun () -> FlatList.Parallel.choose ValueSome defaultArray |> ignore))
            struct (nameof FlatList.Parallel.collect,
                    (fun () -> Array.Parallel.collect Array.singleton nullArray |> ignore),
                    (fun () ->
                        FlatList.Parallel.collect FlatList.singleton defaultArray
                        |> ignore
                    ))
            struct (nameof FlatList.Parallel.exists,
                    (fun () -> Array.Parallel.exists ((=) 1) nullArray |> ignore),
                    (fun () -> FlatList.Parallel.exists ((=) 1) defaultArray |> ignore))
            struct (nameof FlatList.Parallel.filter,
                    (fun () -> Array.Parallel.filter ((=) 1) nullArray |> ignore),
                    (fun () -> FlatList.Parallel.filter ((=) 1) defaultArray |> ignore))
            struct (nameof FlatList.Parallel.forall,
                    (fun () -> Array.Parallel.forall ((=) 1) nullArray |> ignore),
                    (fun () -> FlatList.Parallel.forall ((=) 1) defaultArray |> ignore))
            struct (nameof FlatList.Parallel.groupBy,
                    (fun () -> Array.Parallel.groupBy id nullArray |> ignore),
                    (fun () -> FlatList.Parallel.groupBy id defaultArray |> ignore))
            struct (nameof FlatList.Parallel.iter,
                    (fun () -> Array.Parallel.iter ignore nullArray),
                    (fun () -> FlatList.Parallel.iter ignore defaultArray))
            struct (nameof FlatList.Parallel.iteri,
                    (fun () -> Array.Parallel.iteri (fun _ _ -> ()) nullArray),
                    (fun () -> FlatList.Parallel.iteri (fun _ _ -> ()) defaultArray))
            struct (nameof FlatList.Parallel.map,
                    (fun () -> Array.Parallel.map id nullArray |> ignore),
                    (fun () -> FlatList.Parallel.map id defaultArray |> ignore))
            struct (nameof FlatList.Parallel.mapi,
                    (fun () -> Array.Parallel.mapi (fun _ item -> item) nullArray |> ignore),
                    (fun () ->
                        FlatList.Parallel.mapi (fun _ item -> item) defaultArray
                        |> ignore
                    ))
            struct (nameof FlatList.Parallel.max,
                    (fun () -> Array.Parallel.max nullArray |> ignore),
                    (fun () -> FlatList.Parallel.max defaultArray |> ignore))
            struct (nameof FlatList.Parallel.maxBy,
                    (fun () -> Array.Parallel.maxBy abs nullArray |> ignore),
                    (fun () -> FlatList.Parallel.maxBy abs defaultArray |> ignore))
            struct (nameof FlatList.Parallel.min,
                    (fun () -> Array.Parallel.min nullArray |> ignore),
                    (fun () -> FlatList.Parallel.min defaultArray |> ignore))
            struct (nameof FlatList.Parallel.minBy,
                    (fun () -> Array.Parallel.minBy abs nullArray |> ignore),
                    (fun () -> FlatList.Parallel.minBy abs defaultArray |> ignore))
            struct (nameof FlatList.Parallel.partition,
                    (fun () -> Array.Parallel.partition ((=) 1) nullArray |> ignore),
                    (fun () -> FlatList.Parallel.partition ((=) 1) defaultArray |> ignore))
            struct (nameof FlatList.Parallel.partitionWith,
                    (fun () -> Array.Parallel.partitionWith firstChoice nullArray |> ignore),
                    (fun () ->
                        FlatList.Parallel.partitionWith firstChoice defaultArray
                        |> ignore
                    ))
            struct (nameof FlatList.Parallel.reduce,
                    (fun () -> Array.Parallel.reduce (+) nullArray |> ignore),
                    (fun () -> FlatList.Parallel.reduce (+) defaultArray |> ignore))
            struct (nameof FlatList.Parallel.reduceBy,
                    (fun () -> Array.Parallel.reduceBy abs (+) nullArray |> ignore),
                    (fun () -> FlatList.Parallel.reduceBy abs (+) defaultArray |> ignore))
            struct (nameof FlatList.Parallel.sort,
                    (fun () -> Array.Parallel.sort nullArray |> ignore),
                    (fun () -> FlatList.Parallel.sort defaultArray |> ignore))
            struct (nameof FlatList.Parallel.sortBy,
                    (fun () -> Array.Parallel.sortBy negate nullArray |> ignore),
                    (fun () -> FlatList.Parallel.sortBy negate defaultArray |> ignore))
            struct (nameof FlatList.Parallel.sortByDescending,
                    (fun () -> Array.Parallel.sortByDescending negate nullArray |> ignore),
                    (fun () ->
                        FlatList.Parallel.sortByDescending negate defaultArray
                        |> ignore
                    ))
            struct (nameof FlatList.Parallel.sortDescending,
                    (fun () -> Array.Parallel.sortDescending nullArray |> ignore),
                    (fun () -> FlatList.Parallel.sortDescending defaultArray |> ignore))
            struct (nameof FlatList.Parallel.sortWith,
                    (fun () -> Array.Parallel.sortWith compare nullArray |> ignore),
                    (fun () -> FlatList.Parallel.sortWith compare defaultArray |> ignore))
            struct (nameof FlatList.Parallel.tryFind,
                    (fun () -> Array.Parallel.tryFind ((=) 1) nullArray |> ignore),
                    (fun () -> FlatList.Parallel.tryFind ((=) 1) defaultArray |> ignore))
            struct (nameof FlatList.Parallel.tryFindIndex,
                    (fun () -> Array.Parallel.tryFindIndex ((=) 1) nullArray |> ignore),
                    (fun () ->
                        FlatList.Parallel.tryFindIndex ((=) 1) defaultArray
                        |> ignore
                    ))
            struct (nameof FlatList.Parallel.tryPick,
                    (fun () -> Array.Parallel.tryPick Some nullArray |> ignore),
                    (fun () -> FlatList.Parallel.tryPick ValueSome defaultArray |> ignore))
            // zip takes two flat lists; each of them is a case of its own, named after the parameter that is default
            struct ($"%s{nameof FlatList.Parallel.zip} (array1)",
                    (fun () -> Array.Parallel.zip nullArray [| 1 |] |> ignore),
                    (fun () ->
                        FlatList.Parallel.zip defaultArray (FlatList.singleton 1)
                        |> ignore
                    ))
            struct ($"%s{nameof FlatList.Parallel.zip} (array2)",
                    (fun () -> Array.Parallel.zip [| 1 |] nullArray |> ignore),
                    (fun () ->
                        FlatList.Parallel.zip (FlatList.singleton 1) defaultArray
                        |> ignore
                    ))
        |]
        |> Array.map (fun struct (name, oracle, actual) -> name, struct (oracle, actual))
        |> Map.ofArray

    /// The functions that reject a default flat list where Array.Parallel dereferences a null array, by name.
    static let dereferences : Map<string, unit -> unit> =
        Map.ofArray [|
            nameof FlatList.Parallel.sum, (fun () -> FlatList.Parallel.sum defaultArray |> ignore)
            nameof FlatList.Parallel.sumBy, (fun () -> FlatList.Parallel.sumBy abs defaultArray |> ignore)
        |]

    /// The inputs of the contracts and the larger input.
    static member AllInputs = Seq.ofArray allInputs

    /// The inputs with at least one element.
    static member NonEmptyInputs =
        allInputs
        |> Seq.filter (fun input -> input <> ParallelInput.Empty)

    /// Every input with each predicate.
    static member InputsAndPredicates = seq {
        for input in allInputs do
            for predicate in predicates do
                struct (input, predicate)
    }

    /// Every input with each predicate, as it is and negated.
    static member InputsAndForallPredicates = seq {
        for input in allInputs do
            for predicate in predicates do
                for negated in [| false; true |] do
                    struct (input, predicate, negated)
    }

    /// The counts of init: none, one, the length of the input with several elements and the length of the larger input.
    static member InitCounts = seq {
        0
        1
        Inputs.Several.Length
        (large ()).Length
    }

    /// Two inputs of different lengths, in both orders.
    static member DifferentLengths = seq {
        struct (Inputs.Singleton, Inputs.Several)
        struct (Inputs.Several, Inputs.Singleton)
    }

    /// The number of processors, the length of the arrays and the chunks of the parallel loop of zip.
    static member ZipChunks = seq {
        struct (4, 0, Array.empty<struct (int * int)>)
        struct (4, 255, [| struct (0, 255) |])
        struct (3, 1_000, [| struct (0, 334); struct (334, 668); struct (668, 1_000) |])
        struct (2, 1_500_000_000, [| struct (0, 750_000_000); struct (750_000_000, 1_500_000_000) |])
    }

    /// The names of the functions that mirror an Array.Parallel function, as the rows of their test.
    static member ArrayRejections = arrayRejections.Keys

    /// The names of the functions that reject a default flat list where Array.Parallel dereferences a null array.
    static member Dereferences = dereferences.Keys

    // ---- Creation ----

    [<TestMethod; DynamicData(nameof FlatListParallelTests.InitCounts)>]
    member _.``init creates the elements as the sequential init does`` (count : int) =
        CollectionAssert.AreEqual (
            FlatList.toArray (FlatList.init count (fun index -> index * 3)),
            FlatList.toArray (FlatList.Parallel.init count (fun index -> index * 3)),
            "FlatList.Parallel.init must create the elements as the sequential init does"
        )

    [<TestMethod>]
    member _.``init throws for a negative count as Array.Parallel does`` () =
        ExceptionAssert.same
            id
            (fun () -> Array.Parallel.init -1 id |> ignore)
            (fun () -> FlatList.Parallel.init -1 id |> ignore)
            "FlatList.Parallel.init -1 must throw as Array.Parallel.init does"

    // ---- Search ----

    [<TestMethod; DynamicData(nameof FlatListParallelTests.InputsAndPredicates)>]
    member _.``exists answers as the sequential exists does`` (input : ParallelInput, predicate : ParallelPredicate) =
        let array = flatListOf input

        Assert.AreEqual<bool>(
            FlatList.exists predicate.Matches array,
            FlatList.Parallel.exists predicate.Matches array,
            "FlatList.Parallel.exists must answer as the sequential exists does"
        )

    [<TestMethod; DynamicData(nameof FlatListParallelTests.InputsAndForallPredicates)>]
    member _.``forall answers as the sequential forall does``
        (input : ParallelInput, predicate : ParallelPredicate, negated : bool)
        =
        let array = flatListOf input
        let test item =
            if negated then
                not (predicate.Matches item)
            else
                predicate.Matches item

        Assert.AreEqual<bool>(
            FlatList.forall test array,
            FlatList.Parallel.forall test array,
            "FlatList.Parallel.forall must answer as the sequential forall does"
        )

    [<TestMethod; DynamicData(nameof FlatListParallelTests.InputsAndPredicates)>]
    member _.``tryFind returns the first element that satisfies the predicate``
        (input : ParallelInput, predicate : ParallelPredicate)
        =
        let array = flatListOf input

        Assert.AreEqual<int voption>(
            FlatList.tryFind predicate.Matches array,
            FlatList.Parallel.tryFind predicate.Matches array,
            "FlatList.Parallel.tryFind must return the first element that satisfies the predicate"
        )

    [<TestMethod; DynamicData(nameof FlatListParallelTests.InputsAndPredicates)>]
    member _.``tryFindIndex returns the index of the first element that satisfies the predicate``
        (input : ParallelInput, predicate : ParallelPredicate)
        =
        let array = flatListOf input

        Assert.AreEqual<int voption>(
            FlatList.tryFindIndex predicate.Matches array,
            FlatList.Parallel.tryFindIndex predicate.Matches array,
            "FlatList.Parallel.tryFindIndex must return the index of the first element that satisfies the predicate"
        )

    [<TestMethod; DynamicData(nameof FlatListParallelTests.InputsAndPredicates)>]
    member _.``tryPick returns the value for the first element that the chooser returns a value for``
        (input : ParallelInput, predicate : ParallelPredicate)
        =
        let array = flatListOf input

        Assert.AreEqual<int voption>(
            FlatList.tryPick predicate.Choose array,
            FlatList.Parallel.tryPick predicate.Choose array,
            "FlatList.Parallel.tryPick must return the value for the first element that the chooser returns a value for"
        )

    // ---- Transformation ----

    [<TestMethod; DynamicData(nameof FlatListParallelTests.InputsAndPredicates)>]
    member _.``choose keeps the chosen values in the order of their elements``
        (input : ParallelInput, predicate : ParallelPredicate)
        =
        let array = flatListOf input

        CollectionAssert.AreEqual (
            FlatList.toArray (FlatList.choose predicate.Choose array),
            FlatList.toArray (FlatList.Parallel.choose predicate.Choose array),
            "FlatList.Parallel.choose must keep the chosen values in the order of their elements"
        )

    [<TestMethod; DynamicData(nameof FlatListParallelTests.AllInputs)>]
    member _.``collect concatenates the flat lists in the order of the elements`` (input : ParallelInput) =
        let array = flatListOf input
        // The flat lists of the mapping have zero, one or two elements
        let mapping (item : int) = FlatList.replicate (abs item % 3) item

        CollectionAssert.AreEqual (
            FlatList.toArray (FlatList.collect mapping array),
            FlatList.toArray (FlatList.Parallel.collect mapping array),
            "FlatList.Parallel.collect must concatenate the flat lists in the order of the elements"
        )

    [<TestMethod>]
    member _.``collect throws ArgumentNullException for a default flat list of the mapping`` () =
        // Array.Parallel.collect dereferences a null result; a default flat list is rejected as in the sequential collect
        let ex =
            Assert.ThrowsExactly<ArgumentNullException>(
                Action (fun () ->
                    FlatList.Parallel.collect (fun _ -> defaultArray) (FlatList.singleton 1)
                    |> ignore
                ),
                "FlatList.Parallel.collect must reject a default flat list of the mapping"
            )

        Assert.AreEqual<string | null>(
            "mapping",
            ex.ParamName,
            "FlatList.Parallel.collect must name the mapping as the source of the default flat list"
        )

    [<TestMethod; DynamicData(nameof FlatListParallelTests.InputsAndPredicates)>]
    member _.``filter keeps the elements that satisfy the predicate in their order``
        (input : ParallelInput, predicate : ParallelPredicate)
        =
        let array = flatListOf input

        CollectionAssert.AreEqual (
            FlatList.toArray (FlatList.filter predicate.Matches array),
            FlatList.toArray (FlatList.Parallel.filter predicate.Matches array),
            "FlatList.Parallel.filter must keep the elements that satisfy the predicate in their order"
        )

    [<TestMethod; DynamicData(nameof FlatListParallelTests.AllInputs)>]
    member _.``map transforms every element in its order`` (input : ParallelInput) =
        let array = flatListOf input

        CollectionAssert.AreEqual (
            FlatList.toArray (FlatList.map string array),
            FlatList.toArray (FlatList.Parallel.map string array),
            "FlatList.Parallel.map must transform every element in its order"
        )

    [<TestMethod; DynamicData(nameof FlatListParallelTests.AllInputs)>]
    member _.``mapi transforms every element with its index`` (input : ParallelInput) =
        let array = flatListOf input
        let mapping (index : int) (item : int) = struct (index, item * 3)

        CollectionAssert.AreEqual (
            FlatList.toArray (FlatList.mapi mapping array),
            FlatList.toArray (FlatList.Parallel.mapi mapping array),
            "FlatList.Parallel.mapi must transform every element with its index"
        )

    [<TestMethod; DynamicData(nameof FlatListParallelTests.AllInputs)>]
    member _.``zip pairs the elements at the same index`` (input : ParallelInput) =
        let array = flatListOf input
        let other = FlatList.rev array

        CollectionAssert.AreEqual (
            FlatList.toArray (FlatList.zip array other),
            FlatList.toArray (FlatList.Parallel.zip array other),
            "FlatList.Parallel.zip must pair the elements at the same index"
        )

    [<TestMethod; DynamicData(nameof FlatListParallelTests.DifferentLengths)>]
    member _.``zip throws for different lengths as Array.Parallel does`` (first : int array, second : int array) =
        ExceptionAssert.same
            id
            (fun () -> Array.Parallel.zip first second |> ignore)
            (fun () ->
                FlatList.Parallel.zip (FlatList.ofArray first) (FlatList.ofArray second)
                |> ignore
            )
            "FlatList.Parallel.zip of flat lists of different lengths must throw as Array.Parallel.zip does"

    [<TestMethod; DynamicData(nameof FlatListParallelTests.ZipChunks)>]
    member _.``zip divides every array length into chunks without overflow``
        (processors : int, length : int, expected : struct (int * int) array)
        =
        // The chunks of the parallel loop of zip are checked directly: arrays of these lengths do not fit into the memory of
        // a test run. The length plus the chunk length overflowed for 1.5 billion elements, so the loop did nothing.
        CollectionAssert.AreEqual (
            expected,
            ParallelArrays.chunks processors length,
            "The chunks must divide the length among the processors without overflow"
        )

    [<TestMethod>]
    member _.``zip divides the longest array into adjacent chunks that end at its length`` () =
        // On 64 processors, the first index plus the chunk length of the last chunk of the longest array is more than
        // Int32.MaxValue, so the chunk must be cut to the length before the addition
        let bounds = ParallelArrays.chunks 64 Array.MaxLength
        Assert.HasCount (64, bounds, "The longest array on 64 processors must have one chunk per processor")

        Assert.AreEqual<struct (int * int)>(
            struct (2_113_929_216, Array.MaxLength),
            Array.last bounds,
            "The last chunk of the longest array must end at its length"
        )

        for index = 1 to bounds.Length - 1 do
            let struct (_, previousEnd) = bounds[index - 1]
            let struct (first, _) = bounds[index]
            Assert.AreEqual<int>(
                previousEnd,
                first,
                $"Chunk %d{index} of the longest array must start where the chunk before it ends"
            )

    // ---- Folding ----

    [<TestMethod; DynamicData(nameof FlatListParallelTests.NonEmptyInputs)>]
    member _.``average returns the average of the elements`` (input : ParallelInput) =
        // Whole numbers add up exactly in any order, so the parallel sum is the sequential one
        let floats = FlatList.map float (flatListOf input)

        Assert.AreEqual<float>(
            FlatList.average floats,
            FlatList.Parallel.average floats,
            "FlatList.Parallel.average must return the average of the elements"
        )

    [<TestMethod>]
    member _.``average throws for an empty flat list as Array.Parallel does`` () =
        ExceptionAssert.same
            id
            (fun () -> Array.Parallel.average Array.empty<float> |> ignore)
            (fun () -> FlatList.Parallel.average FlatList.empty<float> |> ignore)
            "FlatList.Parallel.average of an empty flat list must throw as Array.Parallel.average does"

    [<TestMethod; DynamicData(nameof FlatListParallelTests.NonEmptyInputs)>]
    member _.``averageBy returns the average of the projections`` (input : ParallelInput) =
        let array = flatListOf input

        Assert.AreEqual<float>(
            FlatList.averageBy float array,
            FlatList.Parallel.averageBy float array,
            "FlatList.Parallel.averageBy must return the average of the projections"
        )

    [<TestMethod>]
    member _.``averageBy throws for an empty flat list as Array.Parallel does`` () =
        ExceptionAssert.same
            id
            (fun () -> Array.Parallel.averageBy float Array.empty<int> |> ignore)
            (fun () ->
                FlatList.Parallel.averageBy float FlatList.empty<int>
                |> ignore
            )
            "FlatList.Parallel.averageBy of an empty flat list must throw as Array.Parallel.averageBy does"

    [<TestMethod; DynamicData(nameof FlatListParallelTests.AllInputs)>]
    member _.``iter visits every element once`` (input : ParallelInput) =
        let array = flatListOf input
        let visited = ConcurrentQueue<int>()
        FlatList.Parallel.iter visited.Enqueue array

        CollectionAssert.AreEqual (
            array |> Seq.sort |> Seq.toArray,
            visited |> Seq.sort |> Seq.toArray,
            "FlatList.Parallel.iter must visit every element once"
        )

    [<TestMethod; DynamicData(nameof FlatListParallelTests.AllInputs)>]
    member _.``iteri visits every element once with its index`` (input : ParallelInput) =
        let array = flatListOf input
        let visited = ConcurrentQueue<struct (int * int)>()
        FlatList.Parallel.iteri (fun index item -> visited.Enqueue (struct (index, item))) array

        CollectionAssert.AreEqual (
            FlatList.toArray (FlatList.indexed array),
            visited
            |> Seq.sortBy (fun struct (index, _) -> index)
            |> Seq.toArray,
            "FlatList.Parallel.iteri must visit every element once with its index"
        )

    [<TestMethod; DynamicData(nameof FlatListParallelTests.NonEmptyInputs)>]
    member _.``max returns the greatest element`` (input : ParallelInput) =
        let array = flatListOf input
        Assert.AreEqual<int>(
            FlatList.max array,
            FlatList.Parallel.max array,
            "FlatList.Parallel.max must return the greatest element"
        )

    [<TestMethod>]
    member _.``max throws for an empty flat list as Array.Parallel does`` () =
        ExceptionAssert.same
            id
            (fun () -> Array.Parallel.max Array.empty<int> |> ignore)
            (fun () -> FlatList.Parallel.max FlatList.empty<int> |> ignore)
            "FlatList.Parallel.max of an empty flat list must throw as Array.Parallel.max does"

    [<TestMethod; DynamicData(nameof FlatListParallelTests.NonEmptyInputs)>]
    member _.``maxBy returns the element with the greatest projection`` (input : ParallelInput) =
        // In every input, only equal elements share the greatest absolute value, so any of them is the expected one
        let array = flatListOf input

        Assert.AreEqual<int>(
            FlatList.maxBy abs array,
            FlatList.Parallel.maxBy abs array,
            "FlatList.Parallel.maxBy must return the element with the greatest projection"
        )

    [<TestMethod>]
    member _.``maxBy chooses among equal projections as Array.Parallel does`` () =
        let key (struct (key : int, _ : char)) = key

        Assert.AreEqual<struct (int * char)>(
            Array.Parallel.maxBy key Inputs.Pairs,
            FlatList.Parallel.maxBy key (FlatList.ofArray Inputs.Pairs),
            "FlatList.Parallel.maxBy must choose among equal keys as Array.Parallel.maxBy does"
        )

    [<TestMethod>]
    member _.``maxBy throws for an empty flat list as Array.Parallel does`` () =
        ExceptionAssert.same
            id
            (fun () -> Array.Parallel.maxBy abs Array.empty<int> |> ignore)
            (fun () -> FlatList.Parallel.maxBy abs FlatList.empty<int> |> ignore)
            "FlatList.Parallel.maxBy of an empty flat list must throw as Array.Parallel.maxBy does"

    [<TestMethod; DynamicData(nameof FlatListParallelTests.NonEmptyInputs)>]
    member _.``min returns the smallest element`` (input : ParallelInput) =
        let array = flatListOf input
        Assert.AreEqual<int>(
            FlatList.min array,
            FlatList.Parallel.min array,
            "FlatList.Parallel.min must return the smallest element"
        )

    [<TestMethod>]
    member _.``min throws for an empty flat list as Array.Parallel does`` () =
        ExceptionAssert.same
            id
            (fun () -> Array.Parallel.min Array.empty<int> |> ignore)
            (fun () -> FlatList.Parallel.min FlatList.empty<int> |> ignore)
            "FlatList.Parallel.min of an empty flat list must throw as Array.Parallel.min does"

    [<TestMethod; DynamicData(nameof FlatListParallelTests.NonEmptyInputs)>]
    member _.``minBy returns the element with the smallest projection`` (input : ParallelInput) =
        // In every input, only equal elements share the smallest absolute value, so any of them is the expected one
        let array = flatListOf input

        Assert.AreEqual<int>(
            FlatList.minBy abs array,
            FlatList.Parallel.minBy abs array,
            "FlatList.Parallel.minBy must return the element with the smallest projection"
        )

    [<TestMethod>]
    member _.``minBy chooses among equal projections as Array.Parallel does`` () =
        let key (struct (key : int, _ : char)) = key

        Assert.AreEqual<struct (int * char)>(
            Array.Parallel.minBy key Inputs.Pairs,
            FlatList.Parallel.minBy key (FlatList.ofArray Inputs.Pairs),
            "FlatList.Parallel.minBy must choose among equal keys as Array.Parallel.minBy does"
        )

    [<TestMethod>]
    member _.``minBy throws for an empty flat list as Array.Parallel does`` () =
        ExceptionAssert.same
            id
            (fun () -> Array.Parallel.minBy abs Array.empty<int> |> ignore)
            (fun () -> FlatList.Parallel.minBy abs FlatList.empty<int> |> ignore)
            "FlatList.Parallel.minBy of an empty flat list must throw as Array.Parallel.minBy does"

    [<TestMethod; DynamicData(nameof FlatListParallelTests.NonEmptyInputs)>]
    member _.``reduce combines the elements as the sequential reduce does for an associative and commutative function``
        (input : ParallelInput)
        =
        let array = flatListOf input

        Assert.AreEqual<int>(
            FlatList.reduce (+) array,
            FlatList.Parallel.reduce (+) array,
            "FlatList.Parallel.reduce must combine the elements as the sequential reduce does"
        )

    [<TestMethod>]
    member _.``reduce throws for an empty flat list as Array.Parallel does`` () =
        ExceptionAssert.same
            id
            (fun () -> Array.Parallel.reduce (+) Array.empty<int> |> ignore)
            (fun () -> FlatList.Parallel.reduce (+) FlatList.empty<int> |> ignore)
            "FlatList.Parallel.reduce of an empty flat list must throw as Array.Parallel.reduce does"

    [<TestMethod; DynamicData(nameof FlatListParallelTests.NonEmptyInputs)>]
    member _.``reduceBy combines the projections as Array.Parallel does`` (input : ParallelInput) =
        // The sequential module has no reduceBy; it is reduce after map there
        let array = flatListOf input
        let combined = FlatList.Parallel.reduceBy abs (+) array

        Assert.AreEqual<int>(
            Array.Parallel.reduceBy abs (+) (FlatList.toArray array),
            combined,
            "FlatList.Parallel.reduceBy must combine as Array.Parallel.reduceBy does"
        )

        Assert.AreEqual<int>(
            FlatList.map abs array |> FlatList.reduce (+),
            combined,
            "FlatList.Parallel.reduceBy must combine as the sequential reduce after map does"
        )

    [<TestMethod>]
    member _.``reduceBy throws for an empty flat list as Array.Parallel does`` () =
        ExceptionAssert.same
            id
            (fun () -> Array.Parallel.reduceBy abs (+) Array.empty<int> |> ignore)
            (fun () ->
                FlatList.Parallel.reduceBy abs (+) FlatList.empty<int>
                |> ignore
            )
            "FlatList.Parallel.reduceBy of an empty flat list must throw as Array.Parallel.reduceBy does"

    [<TestMethod; DynamicData(nameof FlatListParallelTests.AllInputs)>]
    member _.``sum adds the elements`` (input : ParallelInput) =
        let array = flatListOf input
        Assert.AreEqual<int>(FlatList.sum array, FlatList.Parallel.sum array, "FlatList.Parallel.sum must add the elements")

    [<TestMethod; DynamicData(nameof FlatListParallelTests.AllInputs)>]
    member _.``sumBy adds the projections`` (input : ParallelInput) =
        let array = flatListOf input
        Assert.AreEqual<int>(
            FlatList.sumBy abs array,
            FlatList.Parallel.sumBy abs array,
            "FlatList.Parallel.sumBy must add the projections"
        )

    // ---- Partitioning and grouping ----

    [<TestMethod; DynamicData(nameof FlatListParallelTests.AllInputs)>]
    member _.``groupBy groups the elements as the sequential groupBy does`` (input : ParallelInput) =
        let array = flatListOf input
        let struct (expectedKeys, expectedGroups) = normalized (FlatList.groupBy (fun item -> item % 3) array)
        let struct (keys, groups) = normalized (FlatList.Parallel.groupBy (fun item -> item % 3) array)
        CollectionAssert.AreEqual (expectedKeys, keys, "FlatList.Parallel.groupBy must find the keys of the sequential groupBy")

        CollectionAssert.AreEqual (
            expectedGroups,
            groups,
            "FlatList.Parallel.groupBy must group the elements of each key as the sequential groupBy does"
        )

    [<TestMethod; DynamicData(nameof FlatListParallelTests.AllInputs)>]
    member _.``groupBy groups the elements of a null key`` (input : ParallelInput) =
        let array = flatListOf input
        let projection (item : int) : string | null = if item < 0 then null else string (item % 3)
        let struct (expectedKeys, expectedGroups) = normalized (FlatList.groupBy projection array)
        let struct (keys, groups) = normalized (FlatList.Parallel.groupBy projection array)

        CollectionAssert.AreEqual (
            expectedKeys,
            keys,
            "FlatList.Parallel.groupBy must find the keys of the sequential groupBy, the null key included"
        )

        CollectionAssert.AreEqual (
            expectedGroups,
            groups,
            "FlatList.Parallel.groupBy must group the elements of each key as the sequential groupBy does, the null key included"
        )

    [<TestMethod>]
    member _.``groupBy puts the NaN keys of type float and float32 in one group as Array.Parallel does`` () =
        // Array.Parallel.groupBy compares these keys with ER semantics, while the sequential Array.groupBy makes a group of
        // each NaN. The keys are compared as text, because NaN is not equal to itself.
        let check (source : 'Key array) =
            CollectionAssert.AreEqual (
                Array.Parallel.groupBy id source
                |> Seq.map (fun (key, items) -> string key, items.Length)
                |> Seq.sort
                |> Seq.toArray,
                FlatList.Parallel.groupBy id (FlatList.ofArray source)
                |> Seq.map (fun struct (key, items) -> string key, items.Length)
                |> Seq.sort
                |> Seq.toArray,
                $"FlatList.Parallel.groupBy of %A{source} must group as Array.Parallel.groupBy does"
            )

        check [| nan; 1.0; nan; 2.0; nan |]
        check [| nanf; 1.0f; nanf; 2.0f; nanf |]

    [<TestMethod; DynamicData(nameof FlatListParallelTests.InputsAndPredicates)>]
    member _.``partition splits the elements in their order`` (input : ParallelInput, predicate : ParallelPredicate) =
        let array = flatListOf input
        let struct (expectedAccepted, expectedRejected) = FlatList.partition predicate.Matches array
        let struct (accepted, rejected) = FlatList.Parallel.partition predicate.Matches array

        CollectionAssert.AreEqual (
            FlatList.toArray expectedAccepted,
            FlatList.toArray accepted,
            "FlatList.Parallel.partition must return the elements that satisfy the predicate in their order"
        )

        CollectionAssert.AreEqual (
            FlatList.toArray expectedRejected,
            FlatList.toArray rejected,
            "FlatList.Parallel.partition must return the elements that do not satisfy the predicate in their order"
        )

    [<TestMethod; DynamicData(nameof FlatListParallelTests.AllInputs)>]
    member _.``partitionWith splits the elements by the choice in their order`` (input : ParallelInput) =
        let array = flatListOf input

        let partitioner (item : int) =
            if item > 0 then
                Choice1Of2 item
            else
                Choice2Of2 (string item)

        let struct (expectedFirst, expectedSecond) = FlatList.partitionWith partitioner array
        let struct (first, second) = FlatList.Parallel.partitionWith partitioner array

        CollectionAssert.AreEqual (
            FlatList.toArray expectedFirst,
            FlatList.toArray first,
            "FlatList.Parallel.partitionWith must return the first choices in their order"
        )

        CollectionAssert.AreEqual (
            FlatList.toArray expectedSecond,
            FlatList.toArray second,
            "FlatList.Parallel.partitionWith must return the second choices in their order"
        )

    // ---- Sorting ----

    [<TestMethod; DynamicData(nameof FlatListParallelTests.AllInputs)>]
    member _.``sort orders the elements ascending`` (input : ParallelInput) =
        let array = flatListOf input

        CollectionAssert.AreEqual (
            FlatList.toArray (FlatList.sort array),
            FlatList.toArray (FlatList.Parallel.sort array),
            "FlatList.Parallel.sort must order the elements ascending"
        )

    [<TestMethod; DynamicData(nameof FlatListParallelTests.AllInputs)>]
    member _.``sortBy orders the elements by the projection`` (input : ParallelInput) =
        let array = flatListOf input

        CollectionAssert.AreEqual (
            FlatList.toArray (FlatList.sortBy negate array),
            FlatList.toArray (FlatList.Parallel.sortBy negate array),
            "FlatList.Parallel.sortBy must order the elements by the projection"
        )

    [<TestMethod; DynamicData(nameof FlatListParallelTests.AllInputs)>]
    member _.``sortByDescending orders the elements by the projection descending`` (input : ParallelInput) =
        let array = flatListOf input

        CollectionAssert.AreEqual (
            FlatList.toArray (FlatList.sortByDescending negate array),
            FlatList.toArray (FlatList.Parallel.sortByDescending negate array),
            "FlatList.Parallel.sortByDescending must order the elements by the projection descending"
        )

    [<TestMethod; DynamicData(nameof FlatListParallelTests.AllInputs)>]
    member _.``sortDescending orders the elements descending`` (input : ParallelInput) =
        let array = flatListOf input

        CollectionAssert.AreEqual (
            FlatList.toArray (FlatList.sortDescending array),
            FlatList.toArray (FlatList.Parallel.sortDescending array),
            "FlatList.Parallel.sortDescending must order the elements descending"
        )

    [<TestMethod; DynamicData(nameof FlatListParallelTests.AllInputs)>]
    member _.``sortWith orders the elements with the comparison function`` (input : ParallelInput) =
        let array = flatListOf input
        let descending (left : int) (right : int) = compare right left

        CollectionAssert.AreEqual (
            FlatList.toArray (FlatList.sortWith descending array),
            FlatList.toArray (FlatList.Parallel.sortWith descending array),
            "FlatList.Parallel.sortWith must order the elements with the comparison function"
        )

    [<TestMethod>]
    member _.``the sorting functions leave the shared array of the input unchanged`` () =
        let array = FlatList.ofArray (large ())

        FlatList.Parallel.sort array |> ignore
        FlatList.Parallel.sortBy negate array |> ignore
        FlatList.Parallel.sortByDescending negate array |> ignore
        FlatList.Parallel.sortDescending array |> ignore
        FlatList.Parallel.sortWith compare array |> ignore

        CollectionAssert.AreEqual (large (), FlatList.toArray array, "The sorting functions must sort a copy of the input")

    // ---- Default flat lists ----

    [<TestMethod; DynamicData(nameof FlatListParallelTests.ArrayRejections)>]
    member _.``functions reject a default flat list as Array.Parallel rejects a null array`` (name : string) =
        let struct (oracle, actual) = arrayRejections[name]
        ExceptionAssert.same id oracle actual $"FlatList.Parallel.%s{name} must reject a default flat list"

    [<TestMethod; DynamicData(nameof FlatListParallelTests.Dereferences)>]
    member _.``sum and sumBy reject a default flat list where Array.Parallel dereferences a null array`` (name : string) =
        // Array.Parallel.sum and sumBy throw NullReferenceException for a null array; their documentation promises
        // ArgumentNullException, which a default flat list throws
        let actual = dereferences[name]

        let ex =
            Assert.ThrowsExactly<ArgumentNullException>(
                Action actual,
                $"FlatList.Parallel.%s{name} must reject a default flat list"
            )

        Assert.AreEqual<string | null>(
            "array",
            ex.ParamName,
            $"FlatList.Parallel.%s{name} must name the parameter of the default flat list"
        )
