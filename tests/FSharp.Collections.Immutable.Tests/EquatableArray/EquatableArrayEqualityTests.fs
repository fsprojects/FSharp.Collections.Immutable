namespace FSharp.Collections.Immutable.Tests

open System
open System.Collections.Generic
open System.Collections.Immutable
open Microsoft.VisualStudio.TestTools.UnitTesting
open FSharp.Collections.Immutable

[<TestClass; EquatableArrayTestCategory; EqualityTestCategory>]
type EquatableArrayEqualityTests () =

    [<TestMethod>]
    member _.``arrays with equal elements in the same order are equal under every caller`` () =
        // Built separately, so that no caller can pass by comparing the reference of one underlying array
        let left = EquatableArray.ofArray [| 1; 2; 3 |]
        let right = EquatableArray.ofList [ 1; 2; 3 ]
        Assert.IsTrue ((left = right), "F# equality must compare the elements")
        Assert.IsTrue (left.Equals right, "the typed Equals must compare the elements")
        Assert.IsTrue (left.Equals (box right), "Equals (obj) must compare the elements")
        Assert.IsTrue (
            EqualityComparer<EquatableArray<int>>.Default.Equals(left, right),
            "EqualityComparer.Default must compare the elements"
        )
        Assert.IsTrue (
            (HashIdentity.Structural<EquatableArray<int>>).Equals(left, right),
            "HashIdentity.Structural must compare the elements"
        )
        Assert.IsTrue ((struct (left, 1)).Equals(struct (right, 1)), "ValueTuple.Equals must compare the elements")
        Assert.AreEqual (left.GetHashCode (), right.GetHashCode (), "equal arrays must have equal hash codes")
        Assert.AreEqual (hash left, hash right, "F# hash must agree")

    [<TestMethod>]
    member _.``arrays that differ in order, length or an element are not equal`` () =
        let array = ea [ 1; 2; 3 ]
        Assert.AreNotEqual (ea [ 3; 2; 1 ], array, "the order of the elements must count")
        Assert.AreNotEqual (ea [ 1; 2 ], array, "the length must count")
        Assert.AreNotEqual (ea [ 1; 2; 4 ], array, "each element must count")
        Assert.IsFalse (array.Equals (box [| 1; 2; 3 |]), "an F# array with the same elements is not an EquatableArray")
        Assert.IsFalse (array.Equals (box (ImmutableArray.Create (1, 2, 3))), "an ImmutableArray is not an EquatableArray")

    [<TestMethod>]
    member _.``separately built equal arrays are one key of a Dictionary and a HashSet`` () =
        let dictionary = Dictionary<EquatableArray<int>, string>()
        dictionary[ea [ 1; 2 ]] <- "found"
        Assert.AreEqual ("found", dictionary[EquatableArray.ofArray [| 1; 2 |]], "a Dictionary must find an equal key")

        // Fully qualified: the HashSet of FSharp.Collections.Immutable abbreviates ImmutableHashSet
        let set =
            System.Collections.Generic.HashSet<EquatableArray<int>> [ ea [ 1; 2 ]; EquatableArray.ofArray [| 1; 2 |] ]

        Assert.ContainsSingle (set, "a HashSet must keep equal arrays as one element")
        |> ignore

    [<TestMethod>]
    member _.``equality compares elements with their own equality`` () =
        let first = String ([| 'a'; 'b' |])
        let second = String ([| 'a'; 'b' |])
        Assert.AreNotSame (first, second, "the test needs two string instances")
        Assert.AreEqual (ea [ first ], ea [ second ], "equal strings in different instances must be equal elements")

    [<TestMethod>]
    member _.``a record field and a recursive union compare the elements`` () =
        let document () = { Name = "x"; Tags = EquatableArray.ofList [ "a"; "b" ] }
        Assert.AreEqual (document (), document (), "generated record equality must compare the array elements")

        let tree () = Branch (ea [ Leaf 1; Branch (ea [ Leaf 2; Leaf 3 ]) ])
        Assert.AreEqual (tree (), tree (), "generated union equality must compare the array elements at every level")
        Assert.AreEqual (hash (tree ()), hash (tree ()), "equal trees must have equal hash codes")
        Assert.AreNotEqual (Branch (ea [ Leaf 1 ]), tree (), "trees with different children must not be equal")

    [<TestMethod>]
    member _.``the hash code reads every element once, from first to last`` () =
        let asked = List<int>()
        let array = EquatableArray.ofList [ for number in 0..19 -> HashProbe (number, asked) ]
        array.GetHashCode () |> ignore
        CollectionAssert.AreEqual (Array.init 20 id, asked.ToArray (), "the hash must read all elements, not the last 8 only")

    [<TestMethod>]
    member _.``arrays that differ only in the first of 40 elements have different hash codes`` () =
        let first = EquatableArray.ofList [ for number in 0..39 -> number ]
        let second = EquatableArray.ofList [ for number in 0..39 -> if number = 0 then -1 else number ]
        Assert.AreNotEqual (first.GetHashCode (), second.GetHashCode (), "the hash must use the first element too")

    [<TestMethod>]
    member _.``NaN elements are equal, unlike in F# lists`` () =
        // Pinned on purpose: the elements are compared with EqualityComparer.Default, where NaN equals NaN (ADR 0002 §2)
        Assert.IsTrue ((ea [ nan ] = ea [ nan ]), "EquatableArray compares NaN elements with Double.Equals")
        Assert.IsFalse (([ nan ] = [ nan ]), "an F# list compares NaN elements with F# equality, where NaN is not equal to NaN")

    [<TestMethod>]
    member _.``the element type parameter carries EqualityConditionalOn`` () =
        // A test cannot assert a compile error, so the attribute that gives conditional equality is read by reflection
        let typeParameter =
            typedefof<EquatableArray<_>>.GetGenericArguments()
            |> Array.exactlyOne
        Assert.IsTrue (
            typeParameter.IsDefined (typeof<EqualityConditionalOnAttribute>, false),
            "the type must support equality only when its element type does"
        )
