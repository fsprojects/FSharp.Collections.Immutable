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

    static let array = FlatList.ofList Inputs.several

    /// A flat list long enough for the functions to split their work among several threads.
    static let large = FlatList.init 10_000 (fun index -> index % 1_000 - 500)

    static let defaultArray = Unchecked.defaultof<FlatList<int>>

    static let nullArray = Unchecked.defaultof<int array>

    static let isAbove (limit : int) (item : int) = item > limit

    static let negatedIfNegative (item : int) = if item < 0 then Some (-item) else None

    [<TestMethod>]
    member _.``tryFind returns an option`` () =
        for source in [ array; large ] do
            let found : int option = FlatList.Parallel.tryFind (isAbove 6) source

            OverrideAssert.optionMatches
                (FSharp.Collections.Immutable.FlatList.Parallel.tryFind (isAbove 6) source)
                found
                $"Option.FlatList.Parallel.tryFind over %d{source.Length} elements must match the default"

    [<TestMethod>]
    member _.``tryFindIndex returns an option`` () =
        for source in [ array; large ] do
            let found : int option = FlatList.Parallel.tryFindIndex (isAbove 6) source

            OverrideAssert.optionMatches
                (FSharp.Collections.Immutable.FlatList.Parallel.tryFindIndex (isAbove 6) source)
                found
                $"Option.FlatList.Parallel.tryFindIndex over %d{source.Length} elements must match the default"

    [<TestMethod>]
    member _.``tryPick takes a chooser of options and returns an option`` () =
        for source in [ array; large ] do
            let found : int option = FlatList.Parallel.tryPick negatedIfNegative source

            Assert.AreEqual<int option>(
                Array.tryPick negatedIfNegative (FlatList.toArray source),
                found,
                $"Option.FlatList.Parallel.tryPick over %d{source.Length} elements must pick as Array.tryPick does"
            )

    [<TestMethod>]
    member _.``choose takes a chooser of options`` () =
        for source in [ array; large ] do
            let chosen : FlatList<int> = FlatList.Parallel.choose negatedIfNegative source

            Assert.AreEqual<int list>(
                List.choose negatedIfNegative (FlatList.toList source),
                FlatList.toList chosen,
                $"Option.FlatList.Parallel.choose over %d{source.Length} elements must choose as List.choose does"
            )

    [<TestMethod>]
    member _.``the functions reject a default flat list as Array.Parallel rejects a null array`` () =
        let cases : struct (string * (unit -> unit) * (unit -> unit)) list = [
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
        ]

        for struct (name, oracle, actual) in cases do
            ExceptionAssert.same id oracle actual $"Option.FlatList.Parallel.%s{name} must reject a default flat list"

    [<TestMethod>]
    member _.``the functions without an option keep resolving to the default module`` () =
        let mapped : FlatList<int> = FlatList.Parallel.map (fun item -> item * 2) array
        let zipped : FlatList<int * int> = FlatList.Parallel.zip array array

        Assert.AreEqual<int list>(
            List.map (fun item -> item * 2) Inputs.several,
            FlatList.toList mapped,
            "FlatList.Parallel.map must resolve to the default module"
        )

        Assert.AreEqual<(int * int) list>(
            List.zip Inputs.several Inputs.several,
            FlatList.toList zipped,
            "FlatList.Parallel.zip must resolve to the Tuple module"
        )
