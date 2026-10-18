namespace FSharp.Collections.Immutable.Tests

open System
open System.Collections.Generic

/// <summary>
/// The inputs that the contract tests of the map modules run the functions on.
/// </summary>
/// <remarks>
/// <para>
/// The bindings map <see cref="T:System.Int32"/> keys to <see cref="T:System.String"/> values. The FSharp.Core <c>Map</c>
/// module computes the expectations. A hash map enumerates its bindings in an unspecified order, so the contracts compare
/// the bindings sorted by key, and compute the expectations of the functions that visit the bindings one by one over the
/// order in which the map under test enumerates them (ADR 0001, D8).
/// </para>
/// <para>
/// Every property builds new arrays on each read: MSTest hands the arrays of a row source to the tests as they are, so an
/// array that the tests shared would carry the change that one test made into the others.
/// </para>
/// </remarks>
[<AbstractClass; Sealed>]
type MapInputs =

    /// No bindings.
    static member Empty : struct (int * string) array = [||]

    /// One binding.
    static member Singleton = [| struct (42, "answer") |]

    /// Unsorted keys, negative keys and zero, and a value that two keys share. The absolute values of the keys are distinct, so
    /// that a key comparer of absolute values keeps every binding.
    static member Several = [|
        struct (5, "five")
        struct (-3, "minus three")
        struct (8, "eight")
        struct (0, "zero")
        struct (12, "twelve")
        struct (-7, "five")
        struct (21, "twenty-one")
        struct (2, "two")
    |]

    /// Every input, as the rows of the tests that run on each one.
    static member All = seq {
        MapInputs.Empty
        MapInputs.Singleton
        MapInputs.Several
    }

    /// Every input with at least one binding, as the rows of the tests that need one.
    static member NonEmpty = seq {
        MapInputs.Singleton
        MapInputs.Several
    }

    /// Bindings that repeat keys with other values; in the FSharp.Core <c>Map</c> module the later binding of a key wins.
    static member RepeatedKeys = [|
        struct (1, "one")
        struct (2, "two")
        struct (1, "uno")
        struct (3, "three")
        struct (2, "dos")
    |]

    /// Keys that no input has.
    static member AbsentKeys = [| 1; -5; 100 |]

    /// The FSharp.Core map of bindings, the oracle of the contracts.
    static member ToMap (bindings : seq<struct (int * 'T)>) : Map<int, 'T> =
        bindings
        |> Seq.map (fun struct (key, value) -> key, value)
        |> Map.ofSeq

    /// The bindings of an FSharp.Core map, sorted by key.
    static member OfMap (map : Map<int, 'T>) : struct (int * 'T) array =
        map
        |> Map.toArray
        |> Array.map (fun (key, value) -> struct (key, value))

    /// Bindings sorted by key, to compare the bindings of a map under test with the oracle.
    static member SortByKey (bindings : struct (int * 'T) array) = bindings |> Array.sortBy (fun struct (key, _) -> key)

    /// The keys of bindings, in their order.
    static member KeysOf (bindings : struct (int * 'T) array) = bindings |> Array.map (fun struct (key, _) -> key)

    /// A binding as a key/value pair, the shape that every .NET dictionary enumerates (ADR 0001, D13).
    static member ToKeyValuePair (struct (key : 'Key, value : 'T)) = KeyValuePair (key, value)

    /// A binding as a reference tuple, the shape of most F# code (ADR 0001, D13).
    static member ToTuple (struct (key : 'Key, value : 'T)) = key, value

    /// <summary>
    /// A new string equal to the given one.
    /// </summary>
    /// <remarks>
    /// The default comparer finds the copy equal to the original, and a reference comparison tells them apart: the tests
    /// of the values that a value comparer finds equal see which one a map keeps (ADR 0001, D14).
    /// </remarks>
    static member Copy (text : string) = String (text.ToCharArray ())
