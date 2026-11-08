namespace FSharp.Collections.Immutable.Tests

open System
open System.Collections.Generic
open System.Collections.Immutable
open System.Linq
open System.Reflection
open System.Runtime.CompilerServices
open System.Runtime.InteropServices
open Microsoft.VisualStudio.TestTools.UnitTesting
open FSharp.Collections.Immutable

[<TestClass; EquatableArrayTestCategory; InteropTestCategory>]
type EquatableArrayInteropTests () =

    // Methods that take one type and get the other: F# applies op_Implicit silently at a method argument
    static member private TakeImmutable (items : ImmutableArray<int>) : ImmutableArray<int> = items
    static member private TakeEquatable (array : EquatableArray<int>) : EquatableArray<int> = array

    [<TestMethod>]
    member _.``the constructor and Items keep the underlying array`` () =
        let items = ImmutableArray.Create (1, 2, 3)
        // EquatableArray here is the constructor of the struct, although a non-generic class of the same name exists
        let array = EquatableArray items
        Assert.AreSame<int array | null>(underlying items, underlying array.Items, "the constructor and Items must not copy")

    [<TestMethod>]
    member _.``the implicit conversions keep the underlying array`` () =
        let items = ImmutableArray.Create (1, 2, 3)
        let array = EquatableArrayInteropTests.TakeEquatable items
        Assert.AreSame<int array | null>(
            underlying items,
            underlying array.Items,
            "the conversion to EquatableArray must not copy"
        )
        let back = EquatableArrayInteropTests.TakeImmutable array
        Assert.AreSame<int array | null>(underlying items, underlying back, "the conversion to ImmutableArray must not copy")

    [<TestMethod>]
    member _.``the implicit conversions never expose a default ImmutableArray`` () =
        let fromDefault = EquatableArrayInteropTests.TakeEquatable Unchecked.defaultof<ImmutableArray<int>>
        Assert.IsTrue (fromDefault.IsEmpty, "a default ImmutableArray must become the empty array")
        let toImmutable = EquatableArrayInteropTests.TakeImmutable Unchecked.defaultof<EquatableArray<int>>
        Assert.IsFalse (toImmutable.IsDefault, "the default value must become an empty, not a default, ImmutableArray")

    [<TestMethod>]
    member _.``CollectionBuilderAttribute names the factory class, which has exactly one Create`` () =
        let attribute =
            typedefof<EquatableArray<_>>.GetCustomAttributes(typeof<CollectionBuilderAttribute>, false)
            |> Array.exactlyOne
            :?> CollectionBuilderAttribute

        Assert.AreEqual<Type>(
            typeof<EquatableArray>,
            attribute.BuilderType,
            "C# collection expressions must use the factory class"
        )
        Assert.AreEqual ("Create", attribute.MethodName, "C# collection expressions must call Create")

        let creates =
            typeof<EquatableArray>.GetMethods(BindingFlags.Public ||| BindingFlags.Static)
            |> Array.filter (fun method -> method.Name = "Create")

        // F# collection expressions under RFC FS-1342 accept exactly one such method
        Assert.HasCount (1, creates, "the factory class must have exactly one Create")

    [<TestMethod>]
    member _.``Create copies the span`` () =
        let source = [| 1; 2 |]
        let array = EquatableArray.Create (ReadOnlySpan<int> source)
        Assert.AreEqual (ea [ 1; 2 ], array, "Create must keep the elements")
        Assert.AreNotSame<int array | null>(source, underlying array.Items, "Create must copy the span")

    [<TestMethod>]
    member _.``F# slices follow the rules of F# array slices`` () =
        let source = [| 1; 2; 3; 4 |]
        let array = EquatableArray.ofArray source

        let check (expected : int array) (actual : EquatableArray<int>) (message : string) =
            Assert.AreEqual (EquatableArray.ofArray expected, actual, message)

        check source[1..2] array[1..2] "a range inside the array"
        check source[..1] array[..1] "a range without a start"
        check source[2..] array[2..] "a range without an end"
        check source[-1 .. 1] array[-1 .. 1] "a negative start must count as the first element"
        check source[2..10] array[2..10] "a finish beyond the end must count as the last element"
        check source[5..9] array[5..9] "a range after the end must be empty"
        check source[2..1] array[2..1] "a reversed range must be empty"

    [<TestMethod>]
    member _.``F# slices with extreme reversed bounds are empty`` () =
        // The length of such a range overflows when it is computed before the bounds are compared
        let array = ea [ 1; 2; 3; 4 ]
        Assert.IsTrue (array[2 .. Int32.MinValue].IsEmpty, "a finish of Int32.MinValue must give the empty array")
        Assert.IsTrue (array[Int32.MaxValue .. Int32.MinValue].IsEmpty, "the most reversed range must give the empty array")
        Assert.IsTrue (array[Int32.MaxValue ..].IsEmpty, "a start of Int32.MaxValue must give the empty array")

    [<TestMethod>]
    member _.``an F# slice of all elements shares the array and other slices copy`` () =
        let array = ea [ 1; 2; 3; 4 ]
        Assert.AreSame<int array | null>(
            underlying array.Items,
            underlying array[*].Items,
            "a slice of all elements must not copy"
        )
        Assert.AreSame<int array | null>(
            underlying array.Items,
            underlying array[0..3].Items,
            "a range of all elements must not copy"
        )
        Assert.AreNotSame<int array | null>(underlying array.Items, underlying array[1..2].Items, "a smaller range must copy")

    [<TestMethod>]
    member _.``Slice follows ImmutableArray.Slice`` () =
        let array = ea [ 1; 2; 3; 4 ]
        Assert.AreEqual (ea [ 2; 3 ], array.Slice (1, 2), "Slice must take length elements from start")
        Assert.AreSame<int array | null>(
            underlying array.Items,
            underlying (array.Slice (0, 4)).Items,
            "a Slice of all elements must not copy"
        )
        Assert.IsTrue ((array.Slice (4, 0)).IsEmpty, "an empty range at the end must be valid")

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            (fun () -> array.Slice (3, 2) |> ignore),
            "a range beyond the end must throw, as C# ranges expect"
        )
        |> ignore

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            (fun () -> array.Slice (-1, 1) |> ignore),
            "a negative start must throw"
        )
        |> ignore

    [<TestMethod>]
    member _.``the C# equality operators agree with Equals`` () =
        let array = ea [ 1; 2 ]
        let equal = EquatableArray.ofArray [| 1; 2 |]
        let different = ea [ 2; 1 ]
        Assert.IsTrue (EquatableArray<int>.op_Equality(array, equal), "== must be true for equal arrays")
        Assert.IsFalse (EquatableArray<int>.op_Inequality(array, equal), "!= must be false for equal arrays")
        Assert.IsFalse (EquatableArray<int>.op_Equality(array, different), "== must be false for different arrays")
        Assert.IsTrue (EquatableArray<int>.op_Inequality(array, different), "!= must be true for different arrays")

    [<TestMethod>]
    member _.``IReadOnlyList gives the count, the elements and their order`` () =
        let list = ea [ 10; 20; 30 ] :> IReadOnlyList<int>
        Assert.HasCount (3, list, "the count must be the length")
        Assert.AreEqual (20, list[1], "the indexer must give the element")
        CollectionAssert.AreEqual ([| 10; 20; 30 |], Seq.toArray list, "the enumeration must keep the order")

    [<TestMethod>]
    member _.``IList is read-only`` () =
        let list = ea [ 10; 20; 30 ] :> IList<int>
        Assert.IsTrue (list.IsReadOnly, "the array must report that it is read-only")
        Assert.AreEqual (1, list.IndexOf 20, "IndexOf must find the element")
        Assert.IsTrue (list.Contains 30, "Contains must find the element")
        let target = Array.zeroCreate<int> 4
        list.CopyTo (target, 1)
        CollectionAssert.AreEqual ([| 0; 10; 20; 30 |], target, "CopyTo must copy the elements to the index")

        let writers : (string * (unit -> unit)) list = [
            "Add", (fun () -> list.Add 40)
            "Clear", (fun () -> list.Clear ())
            "Remove", (fun () -> list.Remove 10 |> ignore)
            "Insert", (fun () -> list.Insert (0, 5))
            "RemoveAt", (fun () -> list.RemoveAt 0)
            "the indexer setter", (fun () -> list[0] <- 5)
        ]

        for name, write in writers do
            Assert.ThrowsExactly<NotSupportedException>(
                (fun () -> write ()),
                $"%s{name} must throw, because the array is read-only"
            )
            |> ignore

    [<TestMethod>]
    member _.``AsSpan and AsMemory show the elements without a copy`` () =
        let array = ea [ 1; 2; 3 ]
        CollectionAssert.AreEqual ([| 1; 2; 3 |], array.AsSpan().ToArray(), "the span must show the elements")
        let mutable segment = ArraySegment<int>()
        Assert.IsTrue (MemoryMarshal.TryGetArray (array.AsMemory (), &segment), "the memory must be backed by an array")
        Assert.AreSame<int array | null>(underlying array.Items, segment.Array, "the memory must not copy the elements")

    [<TestMethod>]
    member _.``FSharp.Core and LINQ read the elements`` () =
        let array = ea [ 1; 2; 3 ]
        Assert.AreEqual (3, Seq.length array, "Seq.length must count the elements")
        CollectionAssert.AreEqual ([| 1; 2; 3 |], Seq.toArray array, "Seq.toArray must copy the elements in order")
        Assert.AreEqual (3, Enumerable.Count array, "Enumerable.Count must count the elements")
        CollectionAssert.AreEqual (
            [| 2; 4; 6 |],
            array.Select(fun item -> item * 2).ToArray(),
            "LINQ must enumerate the elements"
        )
