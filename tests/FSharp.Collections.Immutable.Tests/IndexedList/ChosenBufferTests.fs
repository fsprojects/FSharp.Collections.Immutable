namespace FSharp.Collections.Immutable.Tests

open System.Buffers
open FSharp.Collections.Immutable
open Microsoft.VisualStudio.TestTools.UnitTesting

/// The buffer in which `choose` collects the elements: an array of its own below `Buffers.PooledFrom` elements, one rented
/// from the pool above, and a list for a result that does not fit the rented array.
[<TestClass; IndexedListTestCategory>]
type ChosenBufferTests () =

    [<TestMethod>]
    [<DataRow(0, 0)>]
    [<DataRow(1, 1)>]
    [<DataRow(5, 3)>]
    [<DataRow(Buffers.PooledFrom - 1, Buffers.PooledFrom - 1)>]
    [<DataRow(Buffers.PooledFrom, 0)>]
    [<DataRow(Buffers.PooledFrom, Buffers.PooledFrom)>]
    [<DataRow(5000, 17)>]
    [<DataRow(5000, 5000)>]
    member _.``the buffer builds the list of the elements that were added`` (bound : int, count : int) =
        let buffer = Buffers.ChosenBuffer<int> bound

        for value in 0 .. count - 1 do
            buffer.Add value

        CollectionAssert.AreEqual (
            Array.init count id,
            buffer.ToImmutable () |> IndexedList.toArray,
            "The buffer must give the elements that were added, in order"
        )

    [<TestMethod>]
    member _.``a result longer than the rented array moves into a list`` () =
        let count = Buffers.PooledUpTo + 10
        let buffer = Buffers.ChosenBuffer<int> count

        for value in 0 .. count - 1 do
            buffer.Add value

        let list = buffer.ToImmutable ()
        Assert.HasCount (count, list, "The buffer must keep all elements of a result longer than the rented array")
        Assert.AreEqual<int>(
            Buffers.PooledUpTo - 1,
            list[Buffers.PooledUpTo - 1],
            "The last element of the rented array must stay"
        )
        Assert.AreEqual<int>(
            Buffers.PooledUpTo,
            list[Buffers.PooledUpTo],
            "The first element after the rented array must follow it"
        )
        Assert.AreEqual<int>(count - 1, list[count - 1], "The last element must stay")

    [<TestMethod; DataRow 1024; DataRow 5000>]
    member _.``the array that a buffer gives back holds no references`` (count : int) =
        let buffer = Buffers.ChosenBuffer<string> count

        for _ in 1..count do
            buffer.Add "x"

        buffer.ToImmutable () |> ignore
        let rented = ArrayPool<string>.Shared.Rent count

        try
            let leftovers =
                rented
                |> Array.filter (fun item -> not (obj.ReferenceEquals (item, null)))
            Assert.IsEmpty (leftovers, "The pool must not keep the elements that a buffer held")
        finally
            ArrayPool<string>.Shared.Return rented
