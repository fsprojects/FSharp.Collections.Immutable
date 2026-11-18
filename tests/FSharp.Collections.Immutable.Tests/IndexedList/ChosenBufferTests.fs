namespace FSharp.Collections.Immutable.Tests

open System
open System.Buffers
open FSharp.Collections.Immutable
open Microsoft.VisualStudio.TestTools.UnitTesting

/// <summary>
/// A pool that records the arrays that it lends and what each of them holds when it comes back.
/// </summary>
/// <remarks>
/// The shared pool does not tell which array it gets back, and the next array that it lends need not be that one, so a test
/// of the return needs a pool of its own. The arrays are longer than asked and full of a marker, as an array of the shared
/// pool can be longer and can hold what its last user left.
/// </remarks>
type RecordingPool<'T> (marker : 'T) =
    inherit ArrayPool<'T> ()

    let lent = ResizeArray<'T array>()
    let returned = ResizeArray<struct ('T array * 'T array)>()

    /// The arrays that the pool lent, in the order of the calls.
    member _.Lent = lent

    /// The arrays that came back, each with a copy of what it held when it came back.
    member _.Returned = returned

    override _.Rent (minimumLength : int) =
        let array = Array.create (minimumLength + 3) marker
        lent.Add array
        array

    override _.Return (array : 'T array, _clearArray : bool) = returned.Add (struct (array, Array.copy array))

/// The buffer in which `choose` collects the elements: an array of its own below `Buffers.PooledFrom` elements, one rented
/// from the pool above, and a list for a result that does not fit the rented array.
[<TestClass; IndexedListTestCategory>]
type ChosenBufferTests () =

    /// The marker of the arrays of the recording pool of strings.
    static let stale = "stale"

    /// Asserts that a pool of strings lent one array and got it back with the written elements cleared and the rest as it was.
    static let assertReturnedClean (pool : RecordingPool<string>) (written : int) =
        let lent = Assert.ContainsSingle (pool.Lent, "The buffer must rent one array")
        let struct (array, contents) =
            Assert.ContainsSingle (pool.Returned, "The buffer must give one array back")
        Assert.AreSame<string array>(lent, array, "The buffer must give back the array that it rented")

        Assert.IsEmpty (
            contents
            |> Seq.take written
            |> Seq.filter (fun item -> not (obj.ReferenceEquals (item, null))),
            "The array must come back without the elements that the buffer wrote"
        )

        Assert.IsEmpty (
            contents
            |> Seq.skip written
            |> Seq.filter (fun item -> not (obj.ReferenceEquals (item, stale))),
            "The buffer must leave the part of the array that it did not write as it was"
        )

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
        use buffer = new Buffers.ChosenBuffer<int> (bound)

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
        let pool = RecordingPool<int> -1
        use buffer = new Buffers.ChosenBuffer<int> (count, pool)

        for value in 0 .. count - 1 do
            buffer.Add value

        Assert.ContainsSingle (pool.Returned, "The buffer must give the rented array back when its elements move into a list")
        |> ignore

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
        Assert.ContainsSingle (pool.Returned, "The buffer must not give an array back a second time")
        |> ignore

    [<TestMethod; DataRow 0; DataRow 1; DataRow(Buffers.PooledFrom - 1)>]
    member _.``a short buffer rents no array`` (bound : int) =
        let pool = RecordingPool<int> -1
        use buffer = new Buffers.ChosenBuffer<int> (bound, pool)

        for value in 0 .. bound - 1 do
            buffer.Add value

        buffer.ToImmutable () |> ignore
        Assert.IsEmpty (pool.Lent, "A buffer below the least pooled length must take an array of its own")

    [<TestMethod; DataRow(Buffers.PooledFrom, Buffers.PooledFrom); DataRow(5000, 17); DataRow(5000, 0); DataRow(5000, 5000)>]
    member _.``a buffer that builds its list gives its array back without the elements`` (bound : int, count : int) =
        let pool = RecordingPool<string> stale

        do
            use buffer = new Buffers.ChosenBuffer<string> (bound, pool)

            for _ in 1..count do
                buffer.Add "x"

            buffer.ToImmutable () |> ignore

        // The buffer is disposed after it built the list, which must not return the array again
        assertReturnedClean pool count

    [<TestMethod; DataRow(Buffers.PooledFrom, Buffers.PooledFrom); DataRow(5000, 17); DataRow(5000, 0)>]
    member _.``a buffer that is disposed before it builds a list gives its array back without the elements``
        (bound : int, count : int)
        =
        let pool = RecordingPool<string> stale

        // As in choose, when the chooser throws: the buffer is disposed and ToImmutable does not run
        Assert.ThrowsExactly<InvalidOperationException>(
            Action (fun () ->
                use buffer = new Buffers.ChosenBuffer<string> (bound, pool)

                for _ in 1..count do
                    buffer.Add "x"

                invalidOp "the chooser throws"
            ),
            "The exception of the chooser must pass through"
        )
        |> ignore

        assertReturnedClean pool count

    [<TestMethod>]
    member _.``an array of values comes back as it was written`` () =
        let pool = RecordingPool<int> -1

        do
            use buffer = new Buffers.ChosenBuffer<int> (5000, pool)

            for value in 0..16 do
                buffer.Add value

            buffer.ToImmutable () |> ignore

        let struct (_, contents) = Assert.ContainsSingle (pool.Returned, "The buffer must give one array back")

        CollectionAssert.AreEqual (
            Array.init 17 id,
            Array.take 17 contents,
            "An array of values holds no references, so the buffer does not clear it"
        )
