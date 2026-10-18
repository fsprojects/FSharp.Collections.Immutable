namespace FSharp.Collections.Immutable.Tests

open System
open System.Collections.Generic
open System.Collections.Immutable
open System.Globalization
open Microsoft.VisualStudio.TestTools.UnitTesting
open FSharp.Collections.Immutable

[<TestClass; EquatableArrayTestCategory; ComparisonTestCategory>]
type EquatableArrayComparisonTests () =

    /// Runs action with culture as the current culture of this thread, then restores the previous culture.
    static let withCulture (culture : string) (action : unit -> unit) =
        let previous = CultureInfo.CurrentCulture

        try
            CultureInfo.CurrentCulture <- CultureInfo culture
            action ()
        finally
            CultureInfo.CurrentCulture <- previous

    [<TestMethod>]
    member _.``a shorter array comes first, as for F# arrays`` () =
        Assert.IsTrue (compare (ea [ 9 ]) (ea [ 1; 2 ]) < 0, "the length must decide before the elements")
        Assert.IsTrue (compare [| 9 |] [| 1; 2 |] < 0, "F# arrays have the same order")
        Assert.IsTrue (compare [ 9 ] [ 1; 2 ] > 0, "F# lists compare the elements first, so their order differs")

    [<TestMethod>]
    member _.``arrays of the same length compare by their first different element`` () =
        Assert.IsTrue (compare (ea [ 1; 2; 3 ]) (ea [ 1; 3; 0 ]) < 0, "the first different element must decide")
        Assert.AreEqual (0, compare (ea [ 1; 2 ]) (ea [ 1; 2 ]), "equal arrays must compare as 0")
        Assert.AreEqual (0, (ea [ 1; 2 ]).CompareTo(ea [ 1; 2 ]), "CompareTo must agree with compare")
        Assert.IsTrue (
            Comparer<EquatableArray<int>>.Default.Compare(ea [ 1 ], ea [ 2 ]) < 0,
            "Comparer.Default must use the same order"
        )

    [<TestMethod>]
    member _.``arrays of different lengths sort and build a Set`` () =
        // ImmutableArray throws here; EquatableArray must not
        let arrays = [ ea [ 3; 1 ]; ea [ 2 ]; ea [ 1; 2; 3 ]; ea [ 1 ] ]
        Assert.AreEqual<EquatableArray<int> list>(
            [ ea [ 1 ]; ea [ 2 ]; ea [ 3; 1 ]; ea [ 1; 2; 3 ] ],
            List.sort arrays,
            "sorting must order by length, then by elements"
        )
        Assert.HasCount (4, Set.ofList arrays, "a Set must hold arrays of different lengths")

    [<TestMethod>]
    member _.``strings compare ordinally in every culture`` () =
        // Comparer<string>.Default would put "a" before "B" and order "i" and "ı" by culture (ADR 0002 §3)
        for culture in [ "tr-TR"; "en-US"; "" ] do
            withCulture
                culture
                (fun () ->
                    Assert.IsTrue (
                        compare (ea [ "a" ]) (ea [ "B" ]) > 0,
                        $"'a' must come after 'B' ordinally under '%s{culture}'"
                    )
                    Assert.IsTrue (
                        compare (ea [ "i" ]) (ea [ "ı" ]) < 0,
                        $"'i' must come before 'ı' ordinally under '%s{culture}'"
                    )
                    Assert.IsTrue (
                        compare (ea [ "I" ]) (ea [ "i" ]) < 0,
                        $"'I' must come before 'i' ordinally under '%s{culture}'"
                    )
                )

    [<TestMethod>]
    member _.``struct tuples of strings compare in F# order`` () =
        Assert.IsTrue (
            compare (ea [ struct ("a", 1) ]) (ea [ struct ("B", 1) ]) > 0,
            "the strings in the tuples must compare ordinally"
        )
        Assert.IsTrue (
            compare (ea [ struct ("a", 1) ]) (ea [ struct ("a", 2) ]) < 0,
            "the second tuple element must decide a tie"
        )

    [<TestMethod>]
    member _.``F# arrays as elements compare by contents`` () =
        Assert.IsTrue (compare (ea [ [| 1 |] ]) (ea [ [| 2 |] ]) < 0, "inner arrays must compare by their elements")
        Assert.IsTrue (compare (ea [ [| 1; 2 |] ]) (ea [ [| 9 |] ]) > 0, "inner arrays must compare by length first")

    [<TestMethod>]
    member _.``NaN comes before every number, unlike in F# lists`` () =
        // Pinned on purpose: the comparison operators follow CompareTo, which orders NaN first (ADR 0002 §2)
        Assert.IsTrue (ea [ nan ] < ea [ 1.0 ], "EquatableArray orders NaN before every number")
        Assert.IsFalse ([ nan ] < [ 1.0 ], "the F# list operators treat NaN as unordered")
        Assert.AreEqual (0, compare (ea [ nan ]) (ea [ nan ]), "NaN must compare equal to NaN")

    [<TestMethod>]
    member _.``the non-generic CompareTo accepts null and rejects other types`` () =
        let comparable = ea [ 1 ] :> IComparable
        Assert.AreEqual (1, comparable.CompareTo null, "every array must come after null")
        Assert.AreEqual (0, comparable.CompareTo (box (ea [ 1 ])), "a boxed equal array must compare as 0")

        Assert.ThrowsExactly<ArgumentException>(
            (fun () -> comparable.CompareTo (box [| 1 |]) |> ignore),
            "an object of another type must be rejected"
        )
        |> ignore

    [<TestMethod>]
    member _.``the element type parameter carries ComparisonConditionalOn`` () =
        // A test cannot assert a compile error, so the attribute that gives conditional comparison is read by reflection
        let typeParameter =
            typedefof<EquatableArray<_>>.GetGenericArguments()
            |> Array.exactlyOne
        Assert.IsTrue (
            typeParameter.IsDefined (typeof<ComparisonConditionalOnAttribute>, false),
            "the type must support comparison only when its element type does"
        )

    [<TestMethod>]
    member _.``ImmutableArray elements of different lengths throw ArgumentException`` () =
        // The element comparison is the F# comparison of the element type; for ImmutableArray it throws on different
        // lengths, and EquatableArray passes that on (ADR 0002 §2)
        Assert.ThrowsExactly<ArgumentException>(
            (fun () ->
                compare (ea [ ImmutableArray.Create 1 ]) (ea [ ImmutableArray.Create (1, 2) ])
                |> ignore
            ),
            "the comparison of the ImmutableArray elements must throw as compare on them does"
        )
        |> ignore
