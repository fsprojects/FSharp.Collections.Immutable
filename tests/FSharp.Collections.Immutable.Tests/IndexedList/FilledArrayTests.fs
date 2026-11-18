namespace FSharp.Collections.Immutable.Tests

open System
open FSharp.Collections.Immutable
open Microsoft.VisualStudio.TestTools.UnitTesting

/// The temporary array in which a function writes the elements of a list of a known length: one rented from the pool for
/// `Buffers.PooledFrom` to `Buffers.PooledUpTo` elements, and a new one of the exact length below and above.
[<TestClass; IndexedListTestCategory>]
type FilledArrayTests () =

    /// The marker of the arrays of the recording pool of strings.
    static let stale = "stale"

    /// Writes the strings of the indexes into the first elements of an array.
    static let fill (count : int) (items : string array) =
        for index in 0 .. count - 1 do
            items[index] <- string index

    /// Asserts that a pool of strings lent one array and got it back with the elements of the list cleared and the rest as
    /// it was.
    static let assertReturnedClean (pool : RecordingPool<string>) (length : int) =
        let lent = Assert.ContainsSingle (pool.Lent, "The function must rent one array")
        let struct (array, contents) =
            Assert.ContainsSingle (pool.Returned, "The function must give one array back")
        Assert.AreSame<string array>(lent, array, "The function must give back the array that it rented")

        Assert.IsEmpty (
            contents
            |> Seq.take length
            |> Seq.filter (fun item -> not (obj.ReferenceEquals (item, null))),
            "The array must come back without the elements of the list"
        )

        Assert.IsEmpty (
            contents
            |> Seq.skip length
            |> Seq.filter (fun item -> not (obj.ReferenceEquals (item, stale))),
            "The function must leave the part of the array beyond the list as it was"
        )

    [<TestMethod; DataRow 0; DataRow 1; DataRow(Buffers.PooledFrom - 1)>]
    member _.``a short list takes a new array`` (length : int) =
        let pool = RecordingPool<string> stale
        let struct (list, arrayLength) =
            Buffers.ofFilledFrom
                pool
                length
                (fun items ->
                    fill length items
                    items.Length
                )

        Assert.IsEmpty (pool.Lent, "A list below the least pooled length must take an array of its own")
        Assert.AreEqual<int>(length, arrayLength, "The array of a short list must have the length of the list")
        CollectionAssert.AreEqual (Array.init length string, IndexedList.toArray list, "The list must hold the written elements")

    [<TestMethod; DataRow(Buffers.PooledFrom); DataRow 5000>]
    member _.``a long list takes a rented array and gives it back without the elements`` (length : int) =
        let pool = RecordingPool<string> stale
        let struct (list, ()) = Buffers.ofFilledFrom pool length (fill length)

        CollectionAssert.AreEqual (Array.init length string, IndexedList.toArray list, "The list must hold the written elements")
        assertReturnedClean pool length

    [<TestMethod; DataRow(Buffers.PooledFrom); DataRow 5000>]
    member _.``a function that throws gives the rented array back without the elements`` (length : int) =
        let pool = RecordingPool<string> stale

        Assert.ThrowsExactly<InvalidOperationException>(
            Action (fun () ->
                Buffers.ofFilledFrom
                    pool
                    length
                    (fun items ->
                        fill (length / 2) items
                        invalidOp "the function throws"
                    )
                |> ignore
            ),
            "The exception of the function must pass through"
        )
        |> ignore

        assertReturnedClean pool length

    [<TestMethod>]
    member _.``a list longer than the most pooled elements takes a new array of its length`` () =
        let pool = RecordingPool<int> -1
        let struct (atLimit, _) = Buffers.ofFilledFrom pool Buffers.PooledUpTo (fun items -> items.Length)

        Assert.HasCount (Buffers.PooledUpTo, atLimit, "The list of the most pooled elements must have its length")
        Assert.ContainsSingle (pool.Lent, "A list of the most pooled elements must take a rented array")
        |> ignore
        Assert.ContainsSingle (pool.Returned, "A list of the most pooled elements must give its array back")
        |> ignore

        let length = Buffers.PooledUpTo + 1
        let struct (above, arrayLength) = Buffers.ofFilledFrom pool length (fun items -> items.Length)

        Assert.HasCount (length, above, "The longer list must have its length")
        Assert.AreEqual<int>(length, arrayLength, "The array of a longer list must have the exact length of the list")
        Assert.ContainsSingle (pool.Lent, "A list longer than the most pooled elements must not rent an array")
        |> ignore
