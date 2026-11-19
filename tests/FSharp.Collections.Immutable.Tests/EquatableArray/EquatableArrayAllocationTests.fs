namespace FSharp.Collections.Immutable.Tests

// Release only: a Debug build does not optimize F# equality and the loops that these tests measure, so their counts
// would not show the costs of ADR 0002 §3. CI runs both configurations.
#if !DEBUG

open System
open Microsoft.VisualStudio.TestTools.UnitTesting
open FSharp.Collections.Immutable

[<TestClass; EquatableArrayTestCategory; AllocationTestCategory>]
type EquatableArrayAllocationTests () =

    /// Counts the bytes that action allocates on this thread in 100 calls. The caller creates the closure before the
    /// first count. The test project turns off tiered compilation in Release, so the code is fully optimized from the
    /// first call, and the first call outside the count does the static initialization.
    static let allocatedBytes (action : unit -> unit) =
        action ()
        let before = GC.GetAllocatedBytesForCurrentThread ()

        for _ in 1..100 do
            action ()

        GC.GetAllocatedBytesForCurrentThread () - before

    static let assertNoAllocation (operation : string) (action : unit -> unit) =
        Assert.AreEqual (0L, allocatedBytes action, $"%s{operation} must not allocate")

    /// Two equal arrays of 32 elements, built separately.
    static let pair (element : int -> 'T) =
        EquatableArray.ofArray (Array.init 32 element), EquatableArray.ofArray (Array.init 32 element)

    [<TestMethod>]
    member _.``the counter sees an allocation`` () =
        // The control of the other tests: a count of 0 must mean no allocation, not a counter that sees nothing.
        // GC.KeepAlive makes the array escape, so the JIT cannot put it on the stack.
        Assert.IsTrue (
            allocatedBytes (fun () -> GC.KeepAlive (Array.zeroCreate<byte> 32)) > 0L,
            "the counter must see a heap allocation"
        )

    [<TestMethod>]
    member _.``F# equality allocates nothing for the elements of ADR 0002 §3`` () =
        let leftInts, rightInts = pair id
        let leftStrings, rightStrings = pair string
        let leftUnions, rightUnions = pair Leaf
        let leftTuples, rightTuples = pair (fun index -> struct (index, string index))
        let leftPoints, rightPoints = pair (fun index -> { X = index; Y = -index })
        assertNoAllocation "= on int elements" (fun () -> ignore (leftInts = rightInts))
        assertNoAllocation "= on string elements" (fun () -> ignore (leftStrings = rightStrings))
        assertNoAllocation "= on union elements" (fun () -> ignore (leftUnions = rightUnions))
        assertNoAllocation "= on struct tuple elements" (fun () -> ignore (leftTuples = rightTuples))
        assertNoAllocation "= on struct record elements" (fun () -> ignore (leftPoints = rightPoints))

    [<TestMethod>]
    member _.``F# equality of a record with an array field allocates nothing`` () =
        let first = { Name = "x"; Tags = EquatableArray.ofArray (Array.init 32 string) }
        let second = { Name = "x"; Tags = EquatableArray.ofArray (Array.init 32 string) }
        assertNoAllocation "= on a record field" (fun () -> ignore (first = second))

    [<TestMethod>]
    member _.``hash and CompareTo allocate nothing`` () =
        let leftInts, rightInts = pair id
        let leftStrings, rightStrings = pair string
        assertNoAllocation "hash on int elements" (fun () -> ignore (hash leftInts))
        assertNoAllocation "CompareTo on int elements" (fun () -> ignore (leftInts.CompareTo rightInts))
        assertNoAllocation "CompareTo on string elements" (fun () -> ignore (leftStrings.CompareTo rightStrings))

    [<TestMethod>]
    member _.``a for loop and a pattern match allocate nothing`` () =
        let array = EquatableArray.ofArray (Array.init 32 id)
        let three = EquatableArray.ofArray [| 1; 2; 3 |]

        assertNoAllocation
            "a for loop"
            (fun () ->
                let mutable sum = 0

                for item in array do
                    sum <- sum + item

                ignore sum
            )

        assertNoAllocation
            "a pattern match"
            (fun () ->
                match three with
                | Arr3 (first, second, third) -> ignore (first + second + third)
                | _ -> ()
            )

#endif
