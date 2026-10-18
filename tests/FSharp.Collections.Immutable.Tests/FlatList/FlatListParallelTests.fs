namespace FSharp.Collections.Immutable.Tests

open System
open System.Collections.Concurrent
open FSharp.Collections.Immutable
open Microsoft.VisualStudio.TestTools.UnitTesting

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
    static let large = List.init 10_000 (fun index -> index * 7_919 % 10_007 % 2_000 - 1_000)

    /// The inputs of the contracts and the larger input.
    static let allInputs = [ yield! Inputs.all; large ]

    /// The inputs with at least one element.
    static let nonEmptyInputs = [ yield! Inputs.nonEmpty; large ]

    /// Runs a check on the flat list of every given input, with a description of the input for the messages.
    static let forEach (inputs : int list list) (check : FlatList<int> -> string -> unit) =
        for input in inputs do
            let description =
                if input.Length > Inputs.several.Length then
                    $"%d{input.Length} elements"
                else
                    $"%A{input}"

            check (FlatList.ofList input) description

    /// The predicates of the tests: matches at the start, matches late and rare in the larger input, and no match.
    static let predicates : (int -> bool) list = [
        (fun item -> item % 2 = 0)
        (fun item -> item < 0)
        (fun item -> item > 900)
        (fun _ -> false)
    ]

    /// The choosers of the tests, which return a value for the same elements as the predicates.
    static let choosers : (int -> int voption) list = [
        (fun item -> if item % 2 = 0 then ValueSome (item / 2) else ValueNone)
        (fun item -> if item < 0 then ValueSome (-item) else ValueNone)
        (fun item -> if item > 900 then ValueSome (item * 2) else ValueNone)
        (fun _ -> ValueNone)
    ]

    /// Negation is injective, so only equal elements share a key, and the result of a sort does not depend on its stability.
    static let negate (item : int) = -item

    /// The groups with their keys and the elements of every key sorted, for the comparison of groupings whose order is open.
    static let normalized (groups : FlatList<struct ('Key * FlatList<int>)>) =
        groups
        |> Seq.map (fun struct (key, items) -> struct (key, items |> Seq.sort |> Seq.toList))
        |> Seq.sortBy (fun struct (key, _) -> key)
        |> Seq.toList

    // ---- Creation ----

    [<TestMethod; TestCategory(nameof FlatList.Parallel.init)>]
    member _.``init creates the elements as the sequential init does`` () =
        for count in [ 0; 1; Inputs.several.Length; large.Length ] do
            Assert.AreEqual<int list>(
                FlatList.toList (FlatList.init count (fun index -> index * 3)),
                FlatList.toList (FlatList.Parallel.init count (fun index -> index * 3)),
                $"FlatList.Parallel.init %d{count}"
            )

    [<TestMethod; TestCategory(nameof FlatList.Parallel.init)>]
    member _.``init throws for a negative count as Array.Parallel does`` () =
        ExceptionAssert.same
            id
            (fun () -> Array.Parallel.init -1 id |> ignore)
            (fun () -> FlatList.Parallel.init -1 id |> ignore)
            "FlatList.Parallel.init -1 must throw as Array.Parallel.init does"

    // ---- Search ----

    [<TestMethod; TestCategory(nameof FlatList.Parallel.exists)>]
    member _.``exists answers as the sequential exists does`` () =
        forEach
            allInputs
            (fun array description ->
                for predicate in predicates do
                    Assert.AreEqual<bool>(
                        FlatList.exists predicate array,
                        FlatList.Parallel.exists predicate array,
                        $"FlatList.Parallel.exists over %s{description}"
                    )
            )

    [<TestMethod; TestCategory(nameof FlatList.Parallel.forall)>]
    member _.``forall answers as the sequential forall does`` () =
        forEach
            allInputs
            (fun array description ->
                for predicate in predicates do
                    let negated item = not (predicate item)

                    for test in [ predicate; negated ] do
                        Assert.AreEqual<bool>(
                            FlatList.forall test array,
                            FlatList.Parallel.forall test array,
                            $"FlatList.Parallel.forall over %s{description}"
                        )
            )

    [<TestMethod; TestCategory(nameof FlatList.Parallel.tryFind)>]
    member _.``tryFind returns the first element that satisfies the predicate`` () =
        forEach
            allInputs
            (fun array description ->
                for predicate in predicates do
                    Assert.AreEqual<int voption>(
                        FlatList.tryFind predicate array,
                        FlatList.Parallel.tryFind predicate array,
                        $"FlatList.Parallel.tryFind in %s{description}"
                    )
            )

    [<TestMethod; TestCategory(nameof FlatList.Parallel.tryFindIndex)>]
    member _.``tryFindIndex returns the index of the first element that satisfies the predicate`` () =
        forEach
            allInputs
            (fun array description ->
                for predicate in predicates do
                    Assert.AreEqual<int voption>(
                        FlatList.tryFindIndex predicate array,
                        FlatList.Parallel.tryFindIndex predicate array,
                        $"FlatList.Parallel.tryFindIndex in %s{description}"
                    )
            )

    [<TestMethod; TestCategory(nameof FlatList.Parallel.tryPick)>]
    member _.``tryPick returns the value for the first element that the chooser returns a value for`` () =
        forEach
            allInputs
            (fun array description ->
                for chooser in choosers do
                    Assert.AreEqual<int voption>(
                        FlatList.tryPick chooser array,
                        FlatList.Parallel.tryPick chooser array,
                        $"FlatList.Parallel.tryPick in %s{description}"
                    )
            )

    // ---- Transformation ----

    [<TestMethod; TestCategory(nameof FlatList.Parallel.choose)>]
    member _.``choose keeps the chosen values in the order of their elements`` () =
        forEach
            allInputs
            (fun array description ->
                for chooser in choosers do
                    Assert.AreEqual<int list>(
                        FlatList.toList (FlatList.choose chooser array),
                        FlatList.toList (FlatList.Parallel.choose chooser array),
                        $"FlatList.Parallel.choose of %s{description}"
                    )
            )

    [<TestMethod; TestCategory(nameof FlatList.Parallel.collect)>]
    member _.``collect concatenates the flat lists in the order of the elements`` () =
        // The flat lists of the mapping have zero, one or two elements
        let mapping (item : int) = FlatList.replicate (abs item % 3) item

        forEach
            allInputs
            (fun array description ->
                Assert.AreEqual<int list>(
                    FlatList.toList (FlatList.collect mapping array),
                    FlatList.toList (FlatList.Parallel.collect mapping array),
                    $"FlatList.Parallel.collect of %s{description}"
                )
            )

    [<TestMethod; TestCategory(nameof FlatList.Parallel.collect)>]
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

    [<TestMethod; TestCategory(nameof FlatList.Parallel.filter)>]
    member _.``filter keeps the elements that satisfy the predicate in their order`` () =
        forEach
            allInputs
            (fun array description ->
                for predicate in predicates do
                    Assert.AreEqual<int list>(
                        FlatList.toList (FlatList.filter predicate array),
                        FlatList.toList (FlatList.Parallel.filter predicate array),
                        $"FlatList.Parallel.filter of %s{description}"
                    )
            )

    [<TestMethod; TestCategory(nameof FlatList.Parallel.map)>]
    member _.``map transforms every element in its order`` () =
        forEach
            allInputs
            (fun array description ->
                Assert.AreEqual<string list>(
                    FlatList.toList (FlatList.map string array),
                    FlatList.toList (FlatList.Parallel.map string array),
                    $"FlatList.Parallel.map of %s{description}"
                )
            )

    [<TestMethod; TestCategory(nameof FlatList.Parallel.mapi)>]
    member _.``mapi transforms every element with its index`` () =
        let mapping (index : int) (item : int) = struct (index, item * 3)

        forEach
            allInputs
            (fun array description ->
                Assert.AreEqual<struct (int * int) list>(
                    FlatList.toList (FlatList.mapi mapping array),
                    FlatList.toList (FlatList.Parallel.mapi mapping array),
                    $"FlatList.Parallel.mapi of %s{description}"
                )
            )

    [<TestMethod; TestCategory(nameof FlatList.Parallel.zip)>]
    member _.``zip pairs the elements at the same index`` () =
        forEach
            allInputs
            (fun array description ->
                let other = FlatList.rev array

                Assert.AreEqual<struct (int * int) list>(
                    FlatList.toList (FlatList.zip array other),
                    FlatList.toList (FlatList.Parallel.zip array other),
                    $"FlatList.Parallel.zip of %s{description}"
                )
            )

    [<TestMethod; TestCategory(nameof FlatList.Parallel.zip)>]
    member _.``zip throws for different lengths as Array.Parallel does`` () =
        let short = List.toArray Inputs.singleton
        let long = List.toArray Inputs.several

        for struct (first, second) in [ struct (short, long); struct (long, short) ] do
            ExceptionAssert.same
                id
                (fun () -> Array.Parallel.zip first second |> ignore)
                (fun () ->
                    FlatList.Parallel.zip (FlatList.ofArray first) (FlatList.ofArray second)
                    |> ignore
                )
                $"FlatList.Parallel.zip of %d{first.Length} and %d{second.Length} elements must throw as Array.Parallel.zip does"

    // ---- Folding ----

    [<TestMethod; TestCategory(nameof FlatList.Parallel.average)>]
    member _.``average returns the average of the elements`` () =
        // Whole numbers add up exactly in any order, so the parallel sum is the sequential one
        forEach
            nonEmptyInputs
            (fun array description ->
                let floats = FlatList.map float array

                Assert.AreEqual<float>(
                    FlatList.average floats,
                    FlatList.Parallel.average floats,
                    $"FlatList.Parallel.average of %s{description}"
                )
            )

    [<TestMethod; TestCategory(nameof FlatList.Parallel.average)>]
    member _.``average throws for an empty flat list as Array.Parallel does`` () =
        ExceptionAssert.same
            id
            (fun () -> Array.Parallel.average Array.empty<float> |> ignore)
            (fun () -> FlatList.Parallel.average FlatList.empty<float> |> ignore)
            "FlatList.Parallel.average of an empty flat list must throw as Array.Parallel.average does"

    [<TestMethod; TestCategory(nameof FlatList.Parallel.averageBy)>]
    member _.``averageBy returns the average of the projections`` () =
        forEach
            nonEmptyInputs
            (fun array description ->
                Assert.AreEqual<float>(
                    FlatList.averageBy float array,
                    FlatList.Parallel.averageBy float array,
                    $"FlatList.Parallel.averageBy of %s{description}"
                )
            )

    [<TestMethod; TestCategory(nameof FlatList.Parallel.averageBy)>]
    member _.``averageBy throws for an empty flat list as Array.Parallel does`` () =
        ExceptionAssert.same
            id
            (fun () -> Array.Parallel.averageBy float Array.empty<int> |> ignore)
            (fun () ->
                FlatList.Parallel.averageBy float FlatList.empty<int>
                |> ignore
            )
            "FlatList.Parallel.averageBy of an empty flat list must throw as Array.Parallel.averageBy does"

    [<TestMethod; TestCategory(nameof FlatList.Parallel.iter)>]
    member _.``iter visits every element once`` () =
        forEach
            allInputs
            (fun array description ->
                let visited = ConcurrentQueue<int>()
                FlatList.Parallel.iter visited.Enqueue array

                Assert.AreEqual<int list>(
                    array |> Seq.sort |> Seq.toList,
                    visited |> Seq.sort |> Seq.toList,
                    $"FlatList.Parallel.iter over %s{description}"
                )
            )

    [<TestMethod; TestCategory(nameof FlatList.Parallel.iteri)>]
    member _.``iteri visits every element once with its index`` () =
        forEach
            allInputs
            (fun array description ->
                let visited = ConcurrentQueue<struct (int * int)>()
                FlatList.Parallel.iteri (fun index item -> visited.Enqueue (struct (index, item))) array

                Assert.AreEqual<struct (int * int) list>(
                    FlatList.toList (FlatList.indexed array),
                    visited
                    |> Seq.sortBy (fun struct (index, _) -> index)
                    |> Seq.toList,
                    $"FlatList.Parallel.iteri over %s{description}"
                )
            )

    [<TestMethod; TestCategory(nameof FlatList.Parallel.max)>]
    member _.``max returns the greatest element`` () =
        forEach
            nonEmptyInputs
            (fun array description ->
                Assert.AreEqual<int>(
                    FlatList.max array,
                    FlatList.Parallel.max array,
                    $"FlatList.Parallel.max of %s{description}"
                )
            )

    [<TestMethod; TestCategory(nameof FlatList.Parallel.max)>]
    member _.``max throws for an empty flat list as Array.Parallel does`` () =
        ExceptionAssert.same
            id
            (fun () -> Array.Parallel.max Array.empty<int> |> ignore)
            (fun () -> FlatList.Parallel.max FlatList.empty<int> |> ignore)
            "FlatList.Parallel.max of an empty flat list must throw as Array.Parallel.max does"

    [<TestMethod; TestCategory(nameof FlatList.Parallel.maxBy)>]
    member _.``maxBy returns the element with the greatest projection`` () =
        // In every input, only equal elements share the greatest absolute value, so any of them is the expected one
        forEach
            nonEmptyInputs
            (fun array description ->
                Assert.AreEqual<int>(
                    FlatList.maxBy abs array,
                    FlatList.Parallel.maxBy abs array,
                    $"FlatList.Parallel.maxBy of %s{description}"
                )
            )

    [<TestMethod; TestCategory(nameof FlatList.Parallel.maxBy)>]
    member _.``maxBy chooses among equal projections as Array.Parallel does`` () =
        let key (struct (key : int, _ : char)) = key

        Assert.AreEqual<struct (int * char)>(
            Array.Parallel.maxBy key (List.toArray Inputs.pairs),
            FlatList.Parallel.maxBy key (FlatList.ofList Inputs.pairs),
            "FlatList.Parallel.maxBy must choose among equal keys as Array.Parallel.maxBy does"
        )

    [<TestMethod; TestCategory(nameof FlatList.Parallel.maxBy)>]
    member _.``maxBy throws for an empty flat list as Array.Parallel does`` () =
        ExceptionAssert.same
            id
            (fun () -> Array.Parallel.maxBy abs Array.empty<int> |> ignore)
            (fun () -> FlatList.Parallel.maxBy abs FlatList.empty<int> |> ignore)
            "FlatList.Parallel.maxBy of an empty flat list must throw as Array.Parallel.maxBy does"

    [<TestMethod; TestCategory(nameof FlatList.Parallel.min)>]
    member _.``min returns the smallest element`` () =
        forEach
            nonEmptyInputs
            (fun array description ->
                Assert.AreEqual<int>(
                    FlatList.min array,
                    FlatList.Parallel.min array,
                    $"FlatList.Parallel.min of %s{description}"
                )
            )

    [<TestMethod; TestCategory(nameof FlatList.Parallel.min)>]
    member _.``min throws for an empty flat list as Array.Parallel does`` () =
        ExceptionAssert.same
            id
            (fun () -> Array.Parallel.min Array.empty<int> |> ignore)
            (fun () -> FlatList.Parallel.min FlatList.empty<int> |> ignore)
            "FlatList.Parallel.min of an empty flat list must throw as Array.Parallel.min does"

    [<TestMethod; TestCategory(nameof FlatList.Parallel.minBy)>]
    member _.``minBy returns the element with the smallest projection`` () =
        // In every input, only equal elements share the smallest absolute value, so any of them is the expected one
        forEach
            nonEmptyInputs
            (fun array description ->
                Assert.AreEqual<int>(
                    FlatList.minBy abs array,
                    FlatList.Parallel.minBy abs array,
                    $"FlatList.Parallel.minBy of %s{description}"
                )
            )

    [<TestMethod; TestCategory(nameof FlatList.Parallel.minBy)>]
    member _.``minBy chooses among equal projections as Array.Parallel does`` () =
        let key (struct (key : int, _ : char)) = key

        Assert.AreEqual<struct (int * char)>(
            Array.Parallel.minBy key (List.toArray Inputs.pairs),
            FlatList.Parallel.minBy key (FlatList.ofList Inputs.pairs),
            "FlatList.Parallel.minBy must choose among equal keys as Array.Parallel.minBy does"
        )

    [<TestMethod; TestCategory(nameof FlatList.Parallel.minBy)>]
    member _.``minBy throws for an empty flat list as Array.Parallel does`` () =
        ExceptionAssert.same
            id
            (fun () -> Array.Parallel.minBy abs Array.empty<int> |> ignore)
            (fun () -> FlatList.Parallel.minBy abs FlatList.empty<int> |> ignore)
            "FlatList.Parallel.minBy of an empty flat list must throw as Array.Parallel.minBy does"

    [<TestMethod; TestCategory(nameof FlatList.Parallel.reduce)>]
    member _.``reduce combines the elements as the sequential reduce does for an associative and commutative function`` () =
        forEach
            nonEmptyInputs
            (fun array description ->
                Assert.AreEqual<int>(
                    FlatList.reduce (+) array,
                    FlatList.Parallel.reduce (+) array,
                    $"FlatList.Parallel.reduce of %s{description}"
                )
            )

    [<TestMethod; TestCategory(nameof FlatList.Parallel.reduce)>]
    member _.``reduce throws for an empty flat list as Array.Parallel does`` () =
        ExceptionAssert.same
            id
            (fun () -> Array.Parallel.reduce (+) Array.empty<int> |> ignore)
            (fun () -> FlatList.Parallel.reduce (+) FlatList.empty<int> |> ignore)
            "FlatList.Parallel.reduce of an empty flat list must throw as Array.Parallel.reduce does"

    [<TestMethod; TestCategory(nameof FlatList.Parallel.reduceBy)>]
    member _.``reduceBy combines the projections as Array.Parallel does`` () =
        // The sequential module has no reduceBy; it is reduce after map there
        forEach
            nonEmptyInputs
            (fun array description ->
                let combined = FlatList.Parallel.reduceBy abs (+) array

                Assert.AreEqual<int>(
                    Array.Parallel.reduceBy abs (+) (FlatList.toArray array),
                    combined,
                    $"FlatList.Parallel.reduceBy of %s{description} must combine as Array.Parallel.reduceBy does"
                )

                Assert.AreEqual<int>(
                    FlatList.map abs array |> FlatList.reduce (+),
                    combined,
                    $"FlatList.Parallel.reduceBy of %s{description} must combine as the sequential reduce after map does"
                )
            )

    [<TestMethod; TestCategory(nameof FlatList.Parallel.reduceBy)>]
    member _.``reduceBy throws for an empty flat list as Array.Parallel does`` () =
        ExceptionAssert.same
            id
            (fun () -> Array.Parallel.reduceBy abs (+) Array.empty<int> |> ignore)
            (fun () ->
                FlatList.Parallel.reduceBy abs (+) FlatList.empty<int>
                |> ignore
            )
            "FlatList.Parallel.reduceBy of an empty flat list must throw as Array.Parallel.reduceBy does"

    [<TestMethod; TestCategory(nameof FlatList.Parallel.sum)>]
    member _.``sum adds the elements`` () =
        forEach
            allInputs
            (fun array description ->
                Assert.AreEqual<int>(
                    FlatList.sum array,
                    FlatList.Parallel.sum array,
                    $"FlatList.Parallel.sum of %s{description}"
                )
            )

    [<TestMethod; TestCategory(nameof FlatList.Parallel.sumBy)>]
    member _.``sumBy adds the projections`` () =
        forEach
            allInputs
            (fun array description ->
                Assert.AreEqual<int>(
                    FlatList.sumBy abs array,
                    FlatList.Parallel.sumBy abs array,
                    $"FlatList.Parallel.sumBy of %s{description}"
                )
            )

    // ---- Partitioning and grouping ----

    [<TestMethod; TestCategory(nameof FlatList.Parallel.groupBy)>]
    member _.``groupBy groups the elements as the sequential groupBy does`` () =
        forEach
            allInputs
            (fun array description ->
                Assert.AreEqual<struct (int * int list) list>(
                    normalized (FlatList.groupBy (fun item -> item % 3) array),
                    normalized (FlatList.Parallel.groupBy (fun item -> item % 3) array),
                    $"FlatList.Parallel.groupBy of %s{description}"
                )
            )

    [<TestMethod; TestCategory(nameof FlatList.Parallel.groupBy)>]
    member _.``groupBy groups the elements of a null key`` () =
        let projection (item : int) : string | null = if item < 0 then null else string (item % 3)

        forEach
            allInputs
            (fun array description ->
                Assert.AreEqual<struct ((string | null) * int list) list>(
                    normalized (FlatList.groupBy projection array),
                    normalized (FlatList.Parallel.groupBy projection array),
                    $"FlatList.Parallel.groupBy with null keys of %s{description}"
                )
            )

    [<TestMethod; TestCategory(nameof FlatList.Parallel.partition)>]
    member _.``partition splits the elements in their order`` () =
        forEach
            allInputs
            (fun array description ->
                for predicate in predicates do
                    let struct (expectedAccepted, expectedRejected) = FlatList.partition predicate array
                    let struct (accepted, rejected) = FlatList.Parallel.partition predicate array

                    Assert.AreEqual<int list>(
                        FlatList.toList expectedAccepted,
                        FlatList.toList accepted,
                        $"FlatList.Parallel.partition accepted of %s{description}"
                    )

                    Assert.AreEqual<int list>(
                        FlatList.toList expectedRejected,
                        FlatList.toList rejected,
                        $"FlatList.Parallel.partition rejected of %s{description}"
                    )
            )

    [<TestMethod; TestCategory(nameof FlatList.Parallel.partitionWith)>]
    member _.``partitionWith splits the elements by the choice in their order`` () =
        let partitioner (item : int) =
            if item > 0 then
                Choice1Of2 item
            else
                Choice2Of2 (string item)

        forEach
            allInputs
            (fun array description ->
                let struct (expectedFirst, expectedSecond) = FlatList.partitionWith partitioner array
                let struct (first, second) = FlatList.Parallel.partitionWith partitioner array

                Assert.AreEqual<int list>(
                    FlatList.toList expectedFirst,
                    FlatList.toList first,
                    $"FlatList.Parallel.partitionWith first choices of %s{description}"
                )

                Assert.AreEqual<string list>(
                    FlatList.toList expectedSecond,
                    FlatList.toList second,
                    $"FlatList.Parallel.partitionWith second choices of %s{description}"
                )
            )

    // ---- Sorting ----

    [<TestMethod; TestCategory(nameof FlatList.Parallel.sort)>]
    member _.``sort orders the elements ascending`` () =
        forEach
            allInputs
            (fun array description ->
                Assert.AreEqual<int list>(
                    FlatList.toList (FlatList.sort array),
                    FlatList.toList (FlatList.Parallel.sort array),
                    $"FlatList.Parallel.sort of %s{description}"
                )
            )

    [<TestMethod; TestCategory(nameof FlatList.Parallel.sortBy)>]
    member _.``sortBy orders the elements by the projection`` () =
        forEach
            allInputs
            (fun array description ->
                Assert.AreEqual<int list>(
                    FlatList.toList (FlatList.sortBy negate array),
                    FlatList.toList (FlatList.Parallel.sortBy negate array),
                    $"FlatList.Parallel.sortBy of %s{description}"
                )
            )

    [<TestMethod; TestCategory(nameof FlatList.Parallel.sortByDescending)>]
    member _.``sortByDescending orders the elements by the projection descending`` () =
        forEach
            allInputs
            (fun array description ->
                Assert.AreEqual<int list>(
                    FlatList.toList (FlatList.sortByDescending negate array),
                    FlatList.toList (FlatList.Parallel.sortByDescending negate array),
                    $"FlatList.Parallel.sortByDescending of %s{description}"
                )
            )

    [<TestMethod; TestCategory(nameof FlatList.Parallel.sortDescending)>]
    member _.``sortDescending orders the elements descending`` () =
        forEach
            allInputs
            (fun array description ->
                Assert.AreEqual<int list>(
                    FlatList.toList (FlatList.sortDescending array),
                    FlatList.toList (FlatList.Parallel.sortDescending array),
                    $"FlatList.Parallel.sortDescending of %s{description}"
                )
            )

    [<TestMethod; TestCategory(nameof FlatList.Parallel.sortWith)>]
    member _.``sortWith orders the elements with the comparison function`` () =
        let descending (left : int) (right : int) = compare right left

        forEach
            allInputs
            (fun array description ->
                Assert.AreEqual<int list>(
                    FlatList.toList (FlatList.sortWith descending array),
                    FlatList.toList (FlatList.Parallel.sortWith descending array),
                    $"FlatList.Parallel.sortWith of %s{description}"
                )
            )

    [<TestMethod; TestCategory(nameof FlatList.Parallel.sort)>]
    member _.``the sorting functions leave the shared array of the input unchanged`` () =
        let array = FlatList.ofList large

        FlatList.Parallel.sort array |> ignore
        FlatList.Parallel.sortBy negate array |> ignore
        FlatList.Parallel.sortByDescending negate array |> ignore
        FlatList.Parallel.sortDescending array |> ignore
        FlatList.Parallel.sortWith compare array |> ignore

        Assert.AreEqual<int list>(large, FlatList.toList array, "The sorting functions must sort a copy of the input")

    // ---- Default flat lists ----

    [<TestMethod; TestCategory(nameof FlatList.isDefault)>]
    member _.``functions reject a default flat list as Array.Parallel rejects a null array`` () =
        let firstChoice (item : int) : Choice<int, string> = Choice1Of2 item

        let cases : struct (string * (unit -> unit) * (unit -> unit)) list = [
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
            struct (nameof FlatList.Parallel.zip,
                    (fun () -> Array.Parallel.zip nullArray [| 1 |] |> ignore),
                    (fun () ->
                        FlatList.Parallel.zip defaultArray (FlatList.singleton 1)
                        |> ignore
                    ))
            struct (nameof FlatList.Parallel.zip,
                    (fun () -> Array.Parallel.zip [| 1 |] nullArray |> ignore),
                    (fun () ->
                        FlatList.Parallel.zip (FlatList.singleton 1) defaultArray
                        |> ignore
                    ))
        ]

        for struct (name, oracle, actual) in cases do
            ExceptionAssert.same id oracle actual $"FlatList.Parallel.%s{name} must reject a default flat list"

    [<TestMethod; TestCategory(nameof FlatList.isDefault)>]
    member _.``sum and sumBy reject a default flat list where Array.Parallel dereferences a null array`` () =
        // Array.Parallel.sum and sumBy throw NullReferenceException for a null array; their documentation promises
        // ArgumentNullException, which a default flat list throws
        let cases : struct (string * (unit -> unit)) list = [
            struct (nameof FlatList.Parallel.sum, (fun () -> FlatList.Parallel.sum defaultArray |> ignore))
            struct (nameof FlatList.Parallel.sumBy, (fun () -> FlatList.Parallel.sumBy abs defaultArray |> ignore))
        ]

        for struct (name, actual) in cases do
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
