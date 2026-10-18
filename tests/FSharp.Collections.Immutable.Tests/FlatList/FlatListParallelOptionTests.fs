namespace FSharp.Collections.Immutable.Tests

open FSharp.Collections.Immutable
open FSharp.Collections.Immutable.Tuple
open FSharp.Collections.Immutable.Option
open Microsoft.VisualStudio.TestTools.UnitTesting

// The namespaces are opened in the order that gives the shapes of FSharp.Core: Tuple first, Option last.
// The type annotations of the results are the proof that the functions of the Option namespace shadow the default ones,
// also in the nested Parallel module.

/// The `FlatList.Parallel` module of the `Option` namespace.
[<TestClass; FlatListTestCategory; OptionNamespaceTestCategory; ParallelTestCategory>]
type FlatListParallelOptionTests () =

    static let array = FlatList.ofArray Inputs.Several

    /// The elements of a flat list long enough for the functions to split their work among several threads.
    static let large () = Array.init 10_000 (fun index -> index % 1_000 - 500)

    static let defaultArray = Unchecked.defaultof<FlatList<int>>

    static let nullArray = Unchecked.defaultof<int array>

    static let isAbove (limit : int) (item : int) = item > limit

    static let negatedIfNegative (item : int) = if item < 0 then Some (-item) else None

    /// The flat list of the elements of an input.
    static let flatListOf (input : ParallelInput) = FlatList.ofArray (input.Elements large)

    /// The functions that mirror an Array.Parallel function, by name, with that function on a null array and the function
    /// on a default flat list. The rows of the test are the names: a function has no readable name in a row.
    static let arrayRejections =
        [|
            struct (nameof FlatList.Parallel.choose,
                    (fun () -> Array.Parallel.choose negatedIfNegative nullArray |> ignore),
                    (fun () ->
                        FlatList.Parallel.choose negatedIfNegative defaultArray
                        |> ignore
                    ))
            struct (nameof FlatList.Parallel.tryFind,
                    (fun () -> Array.Parallel.tryFind (isAbove 6) nullArray |> ignore),
                    (fun () -> FlatList.Parallel.tryFind (isAbove 6) defaultArray |> ignore))
            struct (nameof FlatList.Parallel.tryFindIndex,
                    (fun () -> Array.Parallel.tryFindIndex (isAbove 6) nullArray |> ignore),
                    (fun () ->
                        FlatList.Parallel.tryFindIndex (isAbove 6) defaultArray
                        |> ignore
                    ))
            struct (nameof FlatList.Parallel.tryPick,
                    (fun () -> Array.Parallel.tryPick negatedIfNegative nullArray |> ignore),
                    (fun () ->
                        FlatList.Parallel.tryPick negatedIfNegative defaultArray
                        |> ignore
                    ))
        |]
        |> Array.map (fun struct (name, oracle, actual) -> name, struct (oracle, actual))
        |> Map.ofArray

    /// The input with several elements of the contracts and the larger input.
    static member Sources = seq {
        ParallelInput.Several
        ParallelInput.Large
    }

    /// The names of the functions that mirror an Array.Parallel function, as the rows of their test.
    static member ArrayRejections = arrayRejections.Keys

    [<TestMethod; DynamicData(nameof FlatListParallelOptionTests.Sources)>]
    member _.``tryFind returns an option`` (input : ParallelInput) =
        let source = flatListOf input
        let found : int option = FlatList.Parallel.tryFind (isAbove 6) source

        OverrideAssert.optionMatches
            (FSharp.Collections.Immutable.FlatList.Parallel.tryFind (isAbove 6) source)
            found
            "Option.FlatList.Parallel.tryFind must match the default"

    [<TestMethod; DynamicData(nameof FlatListParallelOptionTests.Sources)>]
    member _.``tryFindIndex returns an option`` (input : ParallelInput) =
        let source = flatListOf input
        let found : int option = FlatList.Parallel.tryFindIndex (isAbove 6) source

        OverrideAssert.optionMatches
            (FSharp.Collections.Immutable.FlatList.Parallel.tryFindIndex (isAbove 6) source)
            found
            "Option.FlatList.Parallel.tryFindIndex must match the default"

    [<TestMethod; DynamicData(nameof FlatListParallelOptionTests.Sources)>]
    member _.``tryPick takes a chooser of options and returns an option`` (input : ParallelInput) =
        let source = flatListOf input
        let found : int option = FlatList.Parallel.tryPick negatedIfNegative source

        Assert.AreEqual<int option>(
            Array.tryPick negatedIfNegative (FlatList.toArray source),
            found,
            "Option.FlatList.Parallel.tryPick must pick as Array.tryPick does"
        )

    [<TestMethod; DynamicData(nameof FlatListParallelOptionTests.Sources)>]
    member _.``choose takes a chooser of options`` (input : ParallelInput) =
        let source = flatListOf input
        let chosen : FlatList<int> = FlatList.Parallel.choose negatedIfNegative source

        CollectionAssert.AreEqual (
            Array.choose negatedIfNegative (FlatList.toArray source),
            FlatList.toArray chosen,
            "Option.FlatList.Parallel.choose must choose as Array.choose does"
        )

    [<TestMethod; DynamicData(nameof FlatListParallelOptionTests.ArrayRejections)>]
    member _.``the functions reject a default flat list as Array.Parallel rejects a null array`` (name : string) =
        let struct (oracle, actual) = arrayRejections[name]
        ExceptionAssert.same id oracle actual $"Option.FlatList.Parallel.%s{name} must reject a default flat list"

    [<TestMethod>]
    member _.``the functions without an option keep resolving to the default module`` () =
        let mapped : FlatList<int> = FlatList.Parallel.map (fun item -> item * 2) array
        let zipped : FlatList<int * int> = FlatList.Parallel.zip array array

        CollectionAssert.AreEqual (
            Array.map (fun item -> item * 2) Inputs.Several,
            FlatList.toArray mapped,
            "FlatList.Parallel.map must resolve to the default module"
        )

        CollectionAssert.AreEqual (
            Array.zip Inputs.Several Inputs.Several,
            FlatList.toArray zipped,
            "FlatList.Parallel.zip must resolve to the Tuple module"
        )
