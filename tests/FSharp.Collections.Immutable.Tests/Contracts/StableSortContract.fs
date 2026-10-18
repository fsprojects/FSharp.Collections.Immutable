namespace FSharp.Collections.Immutable.Tests

open System
open System.Collections.Generic
open FSharp.Collections.Immutable
open Microsoft.VisualStudio.TestTools.UnitTesting

/// <summary>
/// An element whose equality and comparison look at its key only.
/// </summary>
/// <remarks>
/// Elements with the same key and different tags compare equal, so a sort that compares whole elements, such as
/// <c>sort</c>, shows on them whether it keeps the order of equal elements. An element with the key
/// <see cref="P:FSharp.Collections.Immutable.Tests.KeyedItem.UncomparableKey"/> throws when it is compared, for the tests
/// of a sort whose comparison throws.
/// </remarks>
[<CustomEquality; CustomComparison>]
type KeyedItem = {
    /// The key that the equality and the comparison look at.
    Key : int
    /// The tag that tells the elements with the same key apart.
    Tag : char
} with

    /// The key of the elements whose comparison throws.
    static member UncomparableKey = Int32.MinValue

    /// The keyed items of the pairs of <see cref="P:FSharp.Collections.Immutable.Tests.Inputs.ManyPairs"/>, in their order:
    /// the first component of a pair is the key, and the second one is the tag.
    static member OfManyPairs () : KeyedItem array =
        Inputs.ManyPairs
        |> Array.map (fun struct (key, tag) -> { Key = key; Tag = tag })

    /// The keys and the tags of keyed items, which tell apart the items that compare equal.
    static member KeysAndTags (items : seq<KeyedItem>) : struct (int * char) array =
        items
        |> Seq.map (fun item -> struct (item.Key, item.Tag))
        |> Seq.toArray

    override this.Equals other =
        match other with
        | :? KeyedItem as other -> this.Key = other.Key
        | _ -> false

    override this.GetHashCode () = this.Key.GetHashCode ()

    // F# generic comparison calls the non-generic interface
    interface IComparable with
        member this.CompareTo other =
            match other with
            | :? KeyedItem as other when
                this.Key = KeyedItem.UncomparableKey
                || other.Key = KeyedItem.UncomparableKey
                ->
                raise (NotSupportedException "A keyed item with this key cannot be compared")
            | :? KeyedItem as other -> compare this.Key other.Key
            | _ -> invalidArg (nameof other) "A keyed item compares with keyed items only"

/// <summary>
/// The directions of the comparisons of the sorting tests, as the cases of their rows.
/// </summary>
/// <remarks>
/// MSTest names a row after its values, and a function has no readable name, so a row holds a case and the test takes
/// the comparison of the case.
/// </remarks>
[<RequireQualifiedAccess>]
type SortDirection =
    /// The order of the comparison.
    | Ascending
    /// The reverse order of the comparison.
    | Descending

    /// The comparison in this direction.
    member this.Apply (comparison : 'T -> 'T -> int) : 'T -> 'T -> int =
        match this with
        | SortDirection.Ascending -> comparison
        | SortDirection.Descending -> fun left right -> comparison right left

/// <summary>
/// The sorting functions of a module that sorts stably, for the contract tests.
/// </summary>
/// <remarks>
/// The elements are the pairs of <see cref="P:FSharp.Collections.Immutable.Tests.Inputs.ManyPairs"/>, more than an unstable
/// sort keeps in order: the functions sort them by the key in the first component, and the tag in the second component
/// shows the order of the elements with equal keys.
/// <see cref="P:FSharp.Collections.Immutable.Tests.StableSortOps`1.Sort"/> and
/// <see cref="P:FSharp.Collections.Immutable.Tests.StableSortOps`1.SortDescending"/> compare whole elements and pairs
/// never compare equal, so they take <see cref="T:FSharp.Collections.Immutable.Tests.KeyedItem"/> elements instead, and the
/// instance creates and reads the collection itself, as
/// <see cref="P:FSharp.Collections.Immutable.Tests.SeqOps`1.Unzip"/> does for its pairs. The same holds for the two
/// functions that sort integers:
/// the tests of the allocation of the descending sorts need elements that a comparison boxes.
/// </remarks>
type StableSortOps<'C> = {
    /// The name of the module, for the messages of the assertions.
    Name : string
    /// Creates a collection that enumerates the given elements in their order.
    OfSeq : seq<struct (int * char)> -> 'C
    /// The elements in the order in which the collection enumerates them.
    ToArray : 'C -> struct (int * char) array

    /// Sorts a collection of the given keyed items.
    Sort : KeyedItem array -> KeyedItem array
    SortBy : (struct (int * char) -> int) -> 'C -> 'C
    SortByDescending : (struct (int * char) -> int) -> 'C -> 'C
    /// Sorts a collection of the given keyed items in descending order.
    SortDescending : KeyedItem array -> KeyedItem array
    SortWith : (struct (int * char) -> struct (int * char) -> int) -> 'C -> 'C
    SortWithComparer : IComparer<struct (int * char)> -> 'C -> 'C

    /// Sorts a collection of the given integers with <c>sortDescending</c>.
    SortIntegersDescending : int array -> int array
    /// Sorts a collection of the given integers with <c>sortByDescending</c>, by the integers themselves.
    SortIntegersByDescending : int array -> int array
}

/// <summary>
/// The contract of the modules that sort stably, as <see cref="T:Microsoft.FSharp.Collections.ListModule"/> does:
/// <see cref="T:FSharp.Collections.Immutable.IndexedList"/>,
/// <see cref="T:FSharp.Collections.Immutable.IIndexedList"/>, <c>Queue</c>, <c>IQueue</c>, <c>Stack</c> and <c>IStack</c>
/// (ADR 0001, D5).
/// </summary>
/// <remarks>
/// The expectations come from the stable sorts of <see cref="T:Microsoft.FSharp.Collections.ListModule"/> over the
/// elements in the order in which the collection enumerates them: the sorts of
/// <see cref="T:Microsoft.FSharp.Collections.ArrayModule"/> are not stable.
/// <see cref="T:FSharp.Collections.Immutable.FlatList"/> sorts unstably, as
/// <see cref="T:Microsoft.FSharp.Collections.ArrayModule"/> does, and has no such contract.
/// </remarks>
[<AbstractClass>]
type StableSortContract<'C> (ops : StableSortOps<'C>) =

    static let key (struct (key : int, _ : char)) = key

    static let compareKeys (left : struct (int * char)) (right : struct (int * char)) = compare (key left) (key right)


    /// The keyed items of the pairs of the input, one of which throws when it is compared.
    static let uncomparableItems () =
        let items = KeyedItem.OfManyPairs ()
        items[items.Length / 2] <- { Key = KeyedItem.UncomparableKey; Tag = '!' }
        items

    /// A comparison function that throws.
    static let throwingComparer (_ : struct (int * char)) (_ : struct (int * char)) : int =
        raise (NotSupportedException "The comparison fails")

    /// A projection that throws.
    static let throwingProjection (_ : struct (int * char)) : int = raise (NotSupportedException "The projection fails")

    /// 10 000 integers with many duplicates, in a fixed pseudo-random order.
    static let manyIntegers () =
        let random = Random 42
        Array.init 10_000 (fun _ -> random.Next 1000)

    /// What a sort of 10 000 integers may allocate: the arrays of the sort and the tree of the result need about 1.2 MB,
    /// and the boxes of the integers, which a comparison allocates when it goes through the generic operator, 6.8 MB more.
    static let allocationLimit = 3_000_000L


    /// The collection of the input pairs and the elements that it enumerates.
    let pairs () =
        let collection = ops.OfSeq Inputs.ManyPairs
        struct (collection, ops.ToArray collection)

    /// Both directions of the comparison, as the rows of the tests that sort both ways.
    static member Directions = seq {
        SortDirection.Ascending
        SortDirection.Descending
    }

    [<TestMethod>]
    member _.``sort keeps the order of the elements that compare equal`` () =
        let items = KeyedItem.OfManyPairs ()

        CollectionAssert.AreEqual (
            List.sort (List.ofArray items)
            |> List.toArray
            |> KeyedItem.KeysAndTags,
            ops.Sort items |> KeyedItem.KeysAndTags,
            $"%s{ops.Name}.sort must keep the order of equal elements, as List.sort does"
        )

    [<TestMethod>]
    member _.``sortBy keeps the order of the elements with equal keys`` () =
        let struct (collection, elements) = pairs ()

        CollectionAssert.AreEqual (
            List.sortBy key (List.ofArray elements) |> List.toArray,
            ops.ToArray (ops.SortBy key collection),
            $"%s{ops.Name}.sortBy must keep the order of equal keys, as List.sortBy does"
        )

    [<TestMethod>]
    member _.``sortByDescending keeps the order of the elements with equal keys`` () =
        let struct (collection, elements) = pairs ()

        CollectionAssert.AreEqual (
            List.sortByDescending key (List.ofArray elements)
            |> List.toArray,
            ops.ToArray (ops.SortByDescending key collection),
            $"%s{ops.Name}.sortByDescending must keep the order of equal keys, as List.sortByDescending does"
        )

    [<TestMethod>]
    member _.``sortDescending keeps the order of the elements that compare equal`` () =
        let items = KeyedItem.OfManyPairs ()

        CollectionAssert.AreEqual (
            List.sortDescending (List.ofArray items)
            |> List.toArray
            |> KeyedItem.KeysAndTags,
            ops.SortDescending items |> KeyedItem.KeysAndTags,
            $"%s{ops.Name}.sortDescending must keep the order of equal elements, as List.sortDescending does"
        )

    [<TestMethod; DynamicData(nameof StableSortContract<'C>.Directions)>]
    member _.``sortWith keeps the order of the elements that compare equal`` (direction : SortDirection) =
        let struct (collection, elements) = pairs ()
        let comparer = direction.Apply compareKeys

        CollectionAssert.AreEqual (
            List.sortWith comparer (List.ofArray elements)
            |> List.toArray,
            ops.ToArray (ops.SortWith comparer collection),
            $"%s{ops.Name}.sortWith must keep the order of equal elements, as List.sortWith does"
        )

    [<TestMethod>]
    member _.``sortWithComparer keeps the order of the elements that compare equal`` () =
        let struct (collection, elements) = pairs ()

        CollectionAssert.AreEqual (
            List.sortWith compareKeys (List.ofArray elements)
            |> List.toArray,
            ops.ToArray (
                ops.SortWithComparer (Comparer<struct (int * char)>.Create (fun left right -> compareKeys left right)) collection
            ),
            $"%s{ops.Name}.sortWithComparer must keep the order of equal elements, as List.sortWith does"
        )

    // List sorts in Array.Sort, which wraps the exception of a comparison in an InvalidOperationException; a projection
    // that List calls before it sorts throws as it is

    [<TestMethod>]
    member _.``sort throws what List.sort throws when a comparison throws`` () =
        let items = uncomparableItems ()

        ExceptionAssert.Same (
            id,
            (fun () -> List.sort (List.ofArray items) |> ignore),
            (fun () -> ops.Sort items |> ignore),
            $"%s{ops.Name}.sort must throw what List.sort throws when a comparison throws"
        )

    [<TestMethod>]
    member _.``sortBy throws what List.sortBy throws when the projection throws`` () =
        let struct (collection, elements) = pairs ()

        ExceptionAssert.Same (
            id,
            (fun () ->
                List.sortBy throwingProjection (List.ofArray elements)
                |> ignore
            ),
            (fun () -> ops.SortBy throwingProjection collection |> ignore),
            $"%s{ops.Name}.sortBy must throw what List.sortBy throws when the projection throws"
        )

    [<TestMethod>]
    member _.``sortByDescending throws what List.sortByDescending throws when the projection throws`` () =
        let struct (collection, elements) = pairs ()

        ExceptionAssert.Same (
            id,
            (fun () ->
                List.sortByDescending throwingProjection (List.ofArray elements)
                |> ignore
            ),
            (fun () -> ops.SortByDescending throwingProjection collection |> ignore),
            $"%s{ops.Name}.sortByDescending must throw what List.sortByDescending throws when the projection throws"
        )

    [<TestMethod>]
    member _.``sortDescending throws what List.sortDescending throws when a comparison throws`` () =
        let items = uncomparableItems ()

        ExceptionAssert.Same (
            id,
            (fun () -> List.sortDescending (List.ofArray items) |> ignore),
            (fun () -> ops.SortDescending items |> ignore),
            $"%s{ops.Name}.sortDescending must throw what List.sortDescending throws when a comparison throws"
        )

    [<TestMethod>]
    member _.``sortWith throws what List.sortWith throws when the comparer throws`` () =
        let struct (collection, elements) = pairs ()

        ExceptionAssert.Same (
            id,
            (fun () ->
                List.sortWith throwingComparer (List.ofArray elements)
                |> ignore
            ),
            (fun () -> ops.SortWith throwingComparer collection |> ignore),
            $"%s{ops.Name}.sortWith must throw what List.sortWith throws when the comparer throws"
        )

    // The projection of sortByDescending runs in the comparisons, twice for each of them, as it does in
    // List.sortByDescending. A sort that computed the keys first would call it once for each element.

    [<TestMethod>]
    member _.``sortByDescending calls the projection in the comparisons as List.sortByDescending does`` () =
        let struct (collection, elements) = pairs ()

        let record (calls : ResizeArray<struct (int * char)>) (pair : struct (int * char)) =
            calls.Add pair
            key pair

        let expected = ResizeArray ()
        List.sortByDescending (record expected) (List.ofArray elements)
        |> ignore
        let actual = ResizeArray ()
        ops.SortByDescending (record actual) collection |> ignore

        CollectionAssert.AreEqual (
            expected.ToArray (),
            actual.ToArray (),
            $"%s{ops.Name}.sortByDescending must call the projection in the comparisons, as List.sortByDescending does"
        )

    // Seq.sortDescending and Seq.sortByDescending are inline: in a generic function they compare with the generic
    // operator, which boxes the integers in every comparison and allocates 7.5 MB for 10 000 of them

    [<TestMethod>]
    member _.``sortDescending allocates no box for an integer in a comparison`` () =
        let items = manyIntegers ()
        AllocationAssert.LessThan (
            allocationLimit,
            (fun () -> ops.SortIntegersDescending items |> ignore),
            $"%s{ops.Name}.sortDescending of 10 000 integers must not box them in the comparisons"
        )

    [<TestMethod>]
    member _.``sortByDescending allocates no box for a key in a comparison`` () =
        let items = manyIntegers ()
        AllocationAssert.LessThan (
            allocationLimit,
            (fun () -> ops.SortIntegersByDescending items |> ignore),
            $"%s{ops.Name}.sortByDescending of 10 000 integers must not box the keys in the comparisons"
        )
