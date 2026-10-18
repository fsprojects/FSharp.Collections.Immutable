namespace FSharp.Collections.Immutable.Tests

open System.Collections.Generic
open System.Collections.Immutable
open Microsoft.VisualStudio.TestTools.UnitTesting
open FSharp.Collections.Immutable

/// Decision 5 of ADR 0002, pinned as regression tests: for elements that are F# arrays, ImmutableArrays or tuples that
/// hold them, equality compares the inner arrays by reference and comparison by contents, so hash-based and sorted
/// collections disagree on which values are one key. For other element types they agree.
[<TestClass; EquatableArrayTestCategory; EqualityTestCategory; ComparisonTestCategory>]
type EquatableArrayKeyTests () =

    // The BCL HashSet and SortedSet are written fully qualified: the HashSet and SortedSet of
    // FSharp.Collections.Immutable abbreviate the immutable sets, and that open comes last

    /// Asserts that x and y are two keys for hash-based collections and one key for sorted collections.
    static let assertKeySplit (name : string) (x : EquatableArray<'T>) (y : EquatableArray<'T>) =
        Assert.IsFalse ((x = y), $"%s{name}: equality must compare the inner arrays by reference")
        Assert.AreEqual (0, compare x y, $"%s{name}: comparison must compare the inner arrays by contents")
        let dictionary = Dictionary<EquatableArray<'T>, int>()
        dictionary[x] <- 1
        dictionary[y] <- 2
        Assert.HasCount (2, dictionary, $"%s{name}: a Dictionary must hold two keys")
        Assert.HasCount (2, System.Collections.Generic.HashSet<_> [ x; y ], $"%s{name}: a HashSet must hold two keys")
        Assert.HasCount (2, ImmutableHashSet.CreateRange [ x; y ], $"%s{name}: an ImmutableHashSet must hold two keys")
        Assert.HasCount (1, Set.ofList [ x; y ], $"%s{name}: a Set must hold one key")
        Assert.HasCount (1, Map.ofList [ x, 1; y, 2 ], $"%s{name}: a Map must hold one key")
        Assert.HasCount (1, System.Collections.Generic.SortedSet<_> [ x; y ], $"%s{name}: a SortedSet must hold one key")
        Assert.HasCount (1, ImmutableSortedSet.CreateRange [ x; y ], $"%s{name}: an ImmutableSortedSet must hold one key")

    /// Asserts that x and y are one key for every collection.
    static let assertOneKey (name : string) (x : EquatableArray<'T>) (y : EquatableArray<'T>) =
        Assert.IsTrue ((x = y), $"%s{name}: equality must compare the elements")
        Assert.AreEqual (0, compare x y, $"%s{name}: comparison must agree with equality")
        Assert.HasCount (1, System.Collections.Generic.HashSet<_> [ x; y ], $"%s{name}: a HashSet must hold one key")
        Assert.HasCount (1, Set.ofList [ x; y ], $"%s{name}: a Set must hold one key")
        Assert.HasCount (1, System.Collections.Generic.SortedSet<_> [ x; y ], $"%s{name}: a SortedSet must hold one key")

    [<TestMethod>]
    member _.``F# arrays as elements are two keys for hash-based collections and one key for sorted ones`` () =
        // The inner arrays have equal contents but are allocated separately
        assertKeySplit "int array" (ea [ [| 1; 2 |] ]) (ea [ [| 1; 2 |] ])

    [<TestMethod>]
    member _.``ImmutableArrays as elements are two keys for hash-based collections and one key for sorted ones`` () =
        assertKeySplit "ImmutableArray" (ea [ ImmutableArray.Create (1, 2) ]) (ea [ ImmutableArray.Create (1, 2) ])

    [<TestMethod>]
    member _.``tuples of F# arrays as elements are two keys for hash-based collections and one key for sorted ones`` () =
        assertKeySplit "struct tuple" (ea [ struct ([| 1; 2 |], 3) ]) (ea [ struct ([| 1; 2 |], 3) ])
        assertKeySplit "reference tuple" (ea [ ([| 1; 2 |], 3) ]) (ea [ ([| 1; 2 |], 3) ])

    [<TestMethod>]
    member _.``nested EquatableArrays, records and unions are one key for every collection`` () =
        assertOneKey "EquatableArray" (ea [ ea [ 1; 2 ] ]) (ea [ ea [ 1; 2 ] ])
        assertOneKey "record" (ea [ { Name = "x"; Tags = ea [ "a" ] } ]) (ea [ { Name = "x"; Tags = ea [ "a" ] } ])
        assertOneKey "union with an array field" (ea [ Branch (ea [ Leaf 1 ]) ]) (ea [ Branch (ea [ Leaf 1 ]) ])
        assertOneKey "NaN" (ea [ nan ]) (ea [ nan ])
