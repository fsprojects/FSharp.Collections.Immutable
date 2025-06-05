#if INTERACTIVE
namespace global
#else
namespace FSharp.Collections.Immutable
#endif

// The FlatList name comes from a similar construct seen in the official F# source code
type FlatList<'T> = System.Collections.Immutable.ImmutableArray<'T>

// based on the F# Array module source
[<RequireQualifiedAccess;
  CompiledName((nameof System.Collections.Immutable.ImmutableArray)
               + "Module")>]
module FlatList =

    type internal FlatListFactory = System.Collections.Immutable.ImmutableArray

    let inline internal checkNotDefault argName (list : FlatList<'T>) =
        if list.IsDefault then
            invalidArg argName "Uninstantiated ImmutableArray/FlatList"

    let inline internal check (list : FlatList<'T>) = checkNotDefault (nameof list) list

    let inline internal indexNotFound () = raise <| System.Collections.Generic.KeyNotFoundException ()

    let inline private lengthWhile predicate list =
        check list
        let mutable count = 0

        while count < list.Length && predicate list.[count] do
            count <- count + 1

        count

    ////////// Creating //////////

    /// <summary>Creates a new builder with the specified capacity</summary>
    /// <param name="capacity">The initial capacity of the builder</param>
    /// <returns>An empty builder with the specified capacity</returns>
    let inline builderWith capacity : FlatList<'T>.Builder = FlatListFactory.CreateBuilder (capacity)

    /// <summary>Builds a <see cref="FlatList{T}"/> from a builder, moving the elements and leaving the builder empty</summary>
    /// <param name="builder">The builder to build from</param>
    /// <returns>A <see cref="FlatList{T}"/> containing the elements from the builder</returns>
    /// <exception cref="System.ArgumentNullException">Thrown when builder is null</exception>
    let moveFromBuilder (builder : FlatList<_>.Builder) : FlatList<_> =
        checkNotNull (nameof builder) builder
        builder.MoveToImmutable ()

    /// <summary>Returns an empty <see cref="FlatList{T}"/></summary>
    /// <returns>An empty <see cref="FlatList{T}"/></returns>
    let inline empty<'T> : FlatList<'T> = FlatListFactory.Create<'T> ()

    /// <summary>Builds a <see cref="FlatList{T}"/> from the given array</summary>
    /// <param name="source">The array to build the <see cref="FlatList{T}"/> from</param>
    /// <returns>A <see cref="FlatList{T}"/> containing the elements of the array</returns>
    let inline ofArray (source : _ array) = FlatListFactory.CreateRange source

    /// <summary>Builds a <see cref="FlatList{T}"/> from the given sequence</summary>
    /// <param name="source">The sequence to build the <see cref="FlatList{T}"/> from</param>
    /// <returns>A <see cref="FlatList{T}"/> containing the elements of the sequence</returns>
    let inline ofSeq source = FlatListFactory.CreateRange source

    /// <summary>Returns a <see cref="FlatList{T}"/> with a single element</summary>
    /// <param name="item">The item to put into the <see cref="FlatList{T}"/></param>
    /// <returns>A <see cref="FlatList{T}"/> containing only the given item</returns>
    let inline singleton<'T> (item : 'T) : FlatList<'T> = FlatListFactory.Create<'T> (item)

    /// <summary>Creates a <see cref="FlatList{T}"/> by initializing each element with the given function</summary>
    /// <param name="count">The number of elements to create</param>
    /// <param name="initializer">The function to initialize each element</param>
    /// <returns>A new <see cref="FlatList{T}"/> with the initialized elements</returns>
    /// <exception cref="System.ArgumentException">Thrown when count is negative</exception>
    let init count initializer =
        if count < 0 then
            invalidArg (nameof count) ErrorStrings.InputMustBeNonNegative

        let builder = builderWith count

        for i = 0 to count - 1 do
            builder.Add <| initializer i

        moveFromBuilder builder

    /// <summary>Creates a <see cref="FlatList{T}"/> of a given length with all elements set to the given value</summary>
    /// <param name="count">The length of the <see cref="FlatList{T}"/> to create</param>
    /// <param name="item">The value to replicate</param>
    /// <returns>A <see cref="FlatList{T}"/> of the specified length with all elements equal to the given value</returns>
    let create count item = init count <| fun _ -> item // optimize

    /// <summary>Replicates a value into a <see cref="FlatList{T}"/> of a given length</summary>
    /// <param name="count">The length of the <see cref="FlatList{T}"/> to create</param>
    /// <param name="item">The value to replicate</param>
    /// <returns>A <see cref="FlatList{T}"/> of the specified length with all elements equal to the given value</returns>
    let replicate count item = create count item

    /// <summary>Views the <see cref="FlatList{T}"/> as a sequence</summary>
    /// <param name="flatList">The input <see cref="FlatList{T}"/></param>
    /// <returns>The sequence containing the elements of the <see cref="FlatList{T}"/></returns>
    let inline toSeq (flatList : FlatList<_>) = flatList :> seq<_>

    /// <summary>Builds an array from the given <see cref="FlatList{T}"/></summary>
    /// <param name="list">The <see cref="FlatList{T}"/> to build the array from</param>
    /// <returns>An array containing the elements of the <see cref="FlatList{T}"/></returns>
    let inline toArray (list : FlatList<_>) =
        check list
        Seq.toArray list

    ////////// Building //////////

    /// <summary>Builds a <see cref="FlatList{T}"/> from a builder, copying the elements</summary>
    /// <param name="builder">The builder to build from</param>
    /// <returns>A <see cref="FlatList{T}"/> containing the elements from the builder</returns>
    /// <exception cref="System.ArgumentNullException">Thrown when builder is null</exception>
    let ofBuilder (builder : FlatList<_>.Builder) : FlatList<_> =
        checkNotNull (nameof builder) builder
        builder.ToImmutable ()

    /// <summary>Creates a new builder</summary>
    /// <returns>An empty builder</returns>
    let inline builder () : FlatList<'T>.Builder = FlatListFactory.CreateBuilder ()

    /// <summary>Creates a builder containing the elements of the input <see cref="FlatList{T}"/></summary>
    /// <param name="list">The <see cref="FlatList{T}"/> to create the builder from</param>
    /// <returns>A builder containing the elements of the <see cref="FlatList{T}"/></returns>
    let toBuilder list : FlatList<_>.Builder =
        check list
        list.ToBuilder ()

    module Builder =
        let inline private check (builder : FlatList<'T>.Builder) = checkNotNull (nameof builder) builder

        /// <summary>Adds an item to the builder</summary>
        /// <param name="item">The item to add</param>
        /// <param name="builder">The builder to add to</param>
        let add item builder =
            check builder
            builder.Add (item)

    /// <summary>Checks if the <see cref="FlatList{T}"/> is empty</summary>
    /// <param name="list">The <see cref="FlatList{T}"/> to check</param>
    /// <returns>True if the <see cref="FlatList{T}"/> is empty, false otherwise</returns>
    let isEmpty (list : FlatList<_>) = list.IsEmpty

    /// <summary>Checks if the <see cref="FlatList{T}"/> is uninstantiated</summary>
    /// <param name="list">The <see cref="FlatList{T}"/> to check</param>
    /// <returns>True if the <see cref="FlatList{T}"/> is uninstantiated, false otherwise</returns>
    let isDefault (list : FlatList<_>) = list.IsDefault

    /// <summary>Checks if the <see cref="FlatList{T}"/> is uninstantiated or empty</summary>
    /// <param name="list">The <see cref="FlatList{T}"/> to check</param>
    /// <returns>True if the <see cref="FlatList{T}"/> is uninstantiated or empty, false otherwise</returns>
    let isDefaultOrEmpty (list : FlatList<_>) = list.IsDefaultOrEmpty

    ////////// IReadOnly* //////////

    /// <summary>Returns the number of elements in the <see cref="FlatList{T}"/></summary>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The number of elements in the <see cref="FlatList{T}"/></returns>
    let length list =
        check list
        list.Length

    /// <summary>Gets the element at the specified index in the <see cref="FlatList{T}"/></summary>
    /// <param name="index">The index to retrieve</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The element at the specified index</returns>
    /// <exception cref="System.IndexOutOfRangeException">Thrown when the index is out of range</exception>
    let item index list =
        check list
        list.[index]

    /// <summary>Appends two <see cref="FlatList{T}"/>s to create a new <see cref="FlatList{T}"/> containing all elements from both <see cref="FlatList{T}"/>s</summary>
    /// <param name="list1">The first <see cref="FlatList{T}"/></param>
    /// <param name="list2">The second <see cref="FlatList{T}"/></param>
    /// <returns>A new <see cref="FlatList{T}"/> containing all elements from both input <see cref="FlatList{T}"/>s</returns>
    let append list1 list2 : FlatList<'T> =
        checkNotDefault (nameof list1) list1
        checkNotDefault (nameof list2) list2
        list1.AddRange (list2 : FlatList<_>)

    /// <summary>Searches for the specified object and returns the zero-based index of the first occurrence within the range
    /// of elements in the <see cref="FlatList{T}"/> that starts at the specified index and
    /// contains the specified number of elements.</summary>
    /// <param name="comparer">The equality comparer to use</param>
    /// <param name="index">The starting index</param>
    /// <param name="count">The number of elements to search</param>
    /// <param name="item">The item to search for</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The zero-based index of the first occurrence of the item</returns>
    let indexRangeWith comparer index count item list =
        check list
        list.IndexOf (item, index, count, comparer)

    /// <summary>Searches for the specified object and returns the zero-based index of the first occurrence within the range
    /// of elements in the <see cref="FlatList{T}"/> that starts at the specified index and
    /// contains the specified number of elements.</summary>
    /// <param name="index">The starting index</param>
    /// <param name="count">The number of elements to search</param>
    /// <param name="item">The item to search for</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The zero-based index of the first occurrence of the item</returns>
    let indexRange index count item list = indexRangeWith HashIdentity.Structural index count item list

    /// <summary>Searches for the specified object and returns the zero-based index of the first occurrence within the range
    /// of elements in the <see cref="FlatList{T}"/> that starts at the specified index and
    /// contains the specified number of elements.</summary>
    /// <param name="comparer">The equality comparer to use</param>
    /// <param name="index">The starting index</param>
    /// <param name="item">The item to search for</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The zero-based index of the first occurrence of the item</returns>
    let indexFromWith comparer index item list = indexRangeWith comparer index (length list - index) item list

    /// <summary>Searches for the specified object and returns the zero-based index of the first occurrence within the range
    /// of elements in the <see cref="FlatList{T}"/> that starts at the specified index and
    /// contains the specified number of elements.</summary>
    /// <param name="index">The starting index</param>
    /// <param name="item">The item to search for</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The zero-based index of the first occurrence of the item</returns>
    let indexFrom index item list = indexFromWith HashIdentity.Structural index item list

    /// <summary>Searches for the specified object and returns the zero-based index of the first occurrence within the range
    /// of elements in the <see cref="FlatList{T}"/> that starts at the specified index and
    /// contains the specified number of elements.</summary>
    /// <param name="comparer">The equality comparer to use</param>
    /// <param name="item">The item to search for</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The zero-based index of the first occurrence of the item</returns>
    let indexWith comparer item list = indexFromWith comparer 0 item list

    /// <summary>Searches for the specified object and returns the zero-based index of the first occurrence within the range
    /// of elements in the <see cref="FlatList{T}"/> that starts at the specified index and
    /// contains the specified number of elements.</summary>
    /// <param name="item">The item to search for</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The zero-based index of the first occurrence of the item</returns>
    let index item list = indexWith HashIdentity.Structural item list

    /// <summary>Searches for the specified object and returns the zero-based index of the last occurrence within the
    /// range of elements in the <see cref="FlatList{T}"/> that contains the specified number
    /// of elements and ends at the specified index.</summary>
    /// <param name="comparer">The equality comparer to use</param>
    /// <param name="index">The ending index</param>
    /// <param name="count">The number of elements to search</param>
    /// <param name="item">The item to search for</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The zero-based index of the last occurrence of the item</returns>
    let lastIndexRangeWith comparer index count item list =
        check list
        list.LastIndexOf (item, index, count, comparer)

    /// <summary>Searches for the specified object and returns the zero-based index of the last occurrence within the
    /// range of elements in the <see cref="FlatList{T}"/> that contains the specified number
    /// of elements and ends at the specified index.</summary>
    /// <param name="index">The ending index</param>
    /// <param name="count">The number of elements to search</param>
    /// <param name="item">The item to search for</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The zero-based index of the last occurrence of the item</returns>
    let lastIndexRange index count item list = lastIndexRangeWith HashIdentity.Structural index count item list

    /// <summary>Searches for the specified object and returns the zero-based index of the last occurrence within the
    /// range of elements in the <see cref="FlatList{T}"/> that contains the specified number
    /// of elements and ends at the specified index.</summary>
    /// <param name="comparer">The equality comparer to use</param>
    /// <param name="index">The ending index</param>
    /// <param name="item">The item to search for</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The zero-based index of the last occurrence of the item</returns>
    let lastIndexFromWith comparer index item list = lastIndexRangeWith comparer index (index + 1) item list

    /// <summary>Searches for the specified object and returns the zero-based index of the last occurrence within the
    /// range of elements in the <see cref="FlatList{T}"/> that contains the specified number
    /// of elements and ends at the specified index.</summary>
    /// <param name="index">The ending index</param>
    /// <param name="item">The item to search for</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The zero-based index of the last occurrence of the item</returns>
    let lastIndexFrom index item list = lastIndexFromWith HashIdentity.Structural index item list

    /// <summary>Searches for the specified object and returns the zero-based index of the last occurrence within the
    /// range of elements in the <see cref="FlatList{T}"/> that contains the specified number
    /// of elements and ends at the specified index.</summary>
    /// <param name="comparer">The equality comparer to use</param>
    /// <param name="item">The item to search for</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The zero-based index of the last occurrence of the item</returns>
    let lastIndexWith comparer item list = lastIndexFromWith comparer (length list - 1) item list

    /// <summary>Searches for the specified object and returns the zero-based index of the last occurrence within the
    /// range of elements in the <see cref="FlatList{T}"/> that contains the specified number
    /// of elements and ends at the specified index.</summary>
    /// <param name="item">The item to search for</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The zero-based index of the last occurrence of the item</returns>
    let lastIndex item list = lastIndexWith HashIdentity.Structural item list

    /// <summary>Removes the specified objects from the <see cref="FlatList{T}"/> with the given comparer.</summary>
    /// <param name="comparer">The equality comparer to use</param>
    /// <param name="items">The items to remove</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>A new <see cref="FlatList{T}"/> with the specified items removed</returns>
    let removeAllWith (comparer : System.Collections.Generic.IEqualityComparer<'T>) (items : 'T seq) list : FlatList<_> =
        check list
        list.RemoveRange (items, comparer)

    /// <summary>Removes the specified objects from the <see cref="FlatList{T}"/>.</summary>
    /// <param name="items">The items to remove</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>A new <see cref="FlatList{T}"/> with the specified items removed</returns>
    let removeAll items list = removeAllWith HashIdentity.Structural items list

    /// <summary>Removes all the elements that do not match the conditions defined by the specified predicate.</summary>
    /// <param name="predicate">The predicate to test elements</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>A new <see cref="FlatList{T}"/> with elements that match the predicate</returns>
    let filter predicate list : FlatList<_> =
        check list
        System.Predicate (not << predicate) |> list.RemoveAll

    /// <summary>Removes all the elements that do not match the conditions defined by the specified predicate.</summary>
    /// <param name="predicate">The predicate to test elements</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>A new <see cref="FlatList{T}"/> with elements that match the predicate</returns>
    let where predicate list = filter predicate list

    /// <summary>Removes a range of elements from the <see cref="FlatList{T}"/>.</summary>
    /// <param name="index">The starting index</param>
    /// <param name="count">The number of elements to remove</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>A new <see cref="FlatList{T}"/> with the specified range of elements removed</returns>
    let removeRange index (count : int) list : FlatList<_> =
        check list
        list.RemoveRange (index, count)

    /// <summary>Copies a range of elements from the source <see cref="FlatList{T}"/> to the destination array</summary>
    /// <param name="source">The source <see cref="FlatList{T}"/></param>
    /// <param name="sourceIndex">The starting index in the source <see cref="FlatList{T}"/></param>
    /// <param name="destination">The destination array</param>
    /// <param name="destinationIndex">The starting index in the destination array</param>
    /// <param name="count">The number of elements to copy</param>
    /// <exception cref="System.ArgumentException">Thrown when the range is invalid</exception>
    let blit source sourceIndex (destination : 'T[]) destinationIndex count =
        checkNotDefault (nameof source) source

        try
            source.CopyTo (sourceIndex, destination, destinationIndex, count)
        with exn ->
            raise exn // throw same exception with the correct stack trace. Update exception code

    /// <summary>Sorts a range of elements in the <see cref="FlatList{T}"/> using the specified comparer</summary>
    /// <param name="comparer">The comparer to use</param>
    /// <param name="index">The starting index</param>
    /// <param name="count">The number of elements to sort</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>A new <see cref="FlatList{T}"/> with the specified range of elements sorted</returns>
    let sortRangeWithComparer comparer index count list =
        check list
        list.Sort (index, count, comparer)

    /// <summary>Sorts a range of elements in the <see cref="FlatList{T}"/> using the specified comparison function</summary>
    /// <param name="comparer">The comparison function to use</param>
    /// <param name="index">The starting index</param>
    /// <param name="count">The number of elements to sort</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>A new <see cref="FlatList{T}"/> with the specified range of elements sorted</returns>
    let sortRangeWith comparer index count list =
        sortRangeWithComparer (ComparisonIdentity.FromFunction comparer) index count list

    /// <summary>Sorts a range of elements in the <see cref="FlatList{T}"/> using the default comparer</summary>
    /// <param name="index">The starting index</param>
    /// <param name="count">The number of elements to sort</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>A new <see cref="FlatList{T}"/> with the specified range of elements sorted</returns>
    let sortRange index count list = sortRangeWithComparer ComparisonIdentity.Structural index count list

    /// <summary>Sorts the elements in the <see cref="FlatList{T}"/> using the specified comparer</summary>
    /// <param name="comparer">The comparer to use</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>A new <see cref="FlatList{T}"/> with the elements sorted</returns>
    let sortWithComparer (comparer : System.Collections.Generic.IComparer<_>) list =
        check list
        list.Sort (comparer)

    /// <summary>Sorts the elements in the <see cref="FlatList{T}"/> using the specified comparison function</summary>
    /// <param name="comparer">The comparison function to use</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>A new <see cref="FlatList{T}"/> with the elements sorted</returns>
    let sortWith comparer list = sortWithComparer (ComparisonIdentity.FromFunction comparer) list

    /// <summary>Sorts the elements in the <see cref="FlatList{T}"/> using the default comparer</summary>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>A new <see cref="FlatList{T}"/> with the elements sorted</returns>
    let sort list =
        check list
        list.Sort ()

    ////////// Loop-based //////////

    let inline private builderWithLengthOf list = builderWith <| length list

    let rec private concatAddLengths (arrs : FlatList<FlatList<_>>) i acc =
        if i >= length arrs then
            acc
        else
            concatAddLengths arrs (i + 1) (acc + arrs.[i].Length)

    /// <summary>Concatenates a <see cref="FlatList{T}"/> of <see cref="FlatList{T}"/>s into a single <see cref="FlatList{T}"/></summary>
    /// <param name="arrs">The <see cref="FlatList{T}"/> of <see cref="FlatList{T}"/>s to concatenate</param>
    /// <returns>A new <see cref="FlatList{T}"/> containing all elements from the input <see cref="FlatList{T}"/>s</returns>
    let concat (arrs : FlatList<FlatList<'T>>) =
        let result : FlatList<'T>.Builder = builderWith <| concatAddLengths arrs 0 0

        for i = 0 to length arrs - 1 do
            result.AddRange (arrs.[i] : FlatList<'T>)

        moveFromBuilder result

    /// <summary>Builds a new <see cref="FlatList{T}"/> from the elements of a <see cref="FlatList{T}"/> by applying a mapping function to each element</summary>
    /// <param name="mapping">A function to transform elements from the input <see cref="FlatList{T}"/></param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>A <see cref="FlatList{T}"/> containing the transformed elements</returns>
    let inline map mapping list =
        check list
        let builder = builderWithLengthOf list

        for i = 0 to length list - 1 do
            builder.Add (mapping list.[i])

        moveFromBuilder builder

    /// <summary>Counts the number of elements in the <see cref="FlatList{T}"/> that satisfy the given predicate</summary>
    /// <param name="projection">A function to project elements from the input <see cref="FlatList{T}"/></param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>A <see cref="FlatList{T}"/> of key-value pairs where the key is the projected value and the value is the count</returns>
    let countBy projection list =
        check list
        let dict = new System.Collections.Generic.Dictionary<'Key, int> (HashIdentity.Structural)

        for v in list do
            let key = projection v
            let mutable prev = Unchecked.defaultof<_>

            if dict.TryGetValue (key, &prev) then
                dict.[key] <- prev + 1
            else
                dict.[key] <- 1

        let res = builderWith dict.Count
        let mutable i = 0

        for group in dict do
            res.Add (group.Key, group.Value)
            i <- i + 1

        moveFromBuilder res

    /// <summary>Creates a <see cref="FlatList{T}"/> containing the elements of the original <see cref="FlatList{T}"/> paired with their indices</summary>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>A <see cref="FlatList{T}"/> containing pairs of indices and elements</returns>
    let indexed list =
        check list
        let builder = builderWithLengthOf list

        for i = 0 to length list - 1 do
            builder.Add (i, list.[i])

        moveFromBuilder builder

    /// <summary>Applies the given function to each element of the <see cref="FlatList{T}"/></summary>
    /// <param name="action">A function to apply to each element</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    let inline iter action list =
        check list

        for i = 0 to length list - 1 do
            action list.[i]

    /// <summary>Applies the given function to each element of the <see cref="FlatList{T}"/> and its index</summary>
    /// <param name="action">A function to apply to each element and its index</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    let iteri action list =
        check list
        let f = OptimizedClosures.FSharpFunc<_, _, _>.Adapt (action)
        let len = list.Length

        for i = 0 to len - 1 do
            f.Invoke (i, list.[i])

    /// <summary>Applies the given function to pair of elements at the same position in the two <see cref="FlatList{T}"/>s</summary>
    /// <param name="action">A function to apply to pairs of elements</param>
    /// <param name="list1">The first input <see cref="FlatList{T}"/></param>
    /// <param name="list2">The second input <see cref="FlatList{T}"/></param>
    /// <exception cref="System.ArgumentException">Thrown when the <see cref="FlatList{T}"/>s have different lengths</exception>
    let iter2 action list1 list2 =
        checkNotDefault (nameof list1) list1
        checkNotDefault (nameof list2) list2
        let f = OptimizedClosures.FSharpFunc<'T, 'U, unit>.Adapt (action)
        let len = length list1

        if len <> length list2 then
            invalidArg (nameof list2) ErrorStrings.ListsHaveDifferentLengths

        for i = 0 to len - 1 do
            f.Invoke (list1.[i], list2.[i])

    /// <summary>Applies the given function to the pair of elements at the same position in the two <see cref="FlatList{T}"/>s along with their index</summary>
    /// <param name="action">A function to apply to pairs of elements and their index</param>
    /// <param name="list1">The first input <see cref="FlatList{T}"/></param>
    /// <param name="list2">The second input <see cref="FlatList{T}"/></param>
    /// <exception cref="System.ArgumentException">Thrown when the <see cref="FlatList{T}"/>s have different lengths</exception>
    let iteri2 action list1 list2 =
        checkNotDefault (nameof list1) list1
        checkNotDefault (nameof list2) list2
        let f = OptimizedClosures.FSharpFunc<_, _, _, _>.Adapt (action)
        let len1 = list1.Length

        if len1 <> list2.Length then
            invalidArg (nameof list2) ErrorStrings.ListsHaveDifferentLengths

        for i = 0 to len1 - 1 do
            f.Invoke (i, list1.[i], list2.[i])

    /// <summary>Tests if any element of the <see cref="FlatList{T}"/> satisfies the given predicate</summary>
    /// <param name="predicate">A function to test each element</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>True if any element satisfies the predicate, false otherwise</returns>
    let exists predicate list =
        check list
        let len = list.Length
        let rec loop i = i < len && (predicate list.[i] || loop (i + 1))
        loop 0

    /// <summary>Tests if any corresponding pair of elements from the two <see cref="FlatList{T}"/>s satisfies the given predicate</summary>
    /// <param name="predicate">A function to test pairs of elements</param>
    /// <param name="list1">The first input <see cref="FlatList{T}"/></param>
    /// <param name="list2">The second input <see cref="FlatList{T}"/></param>
    /// <returns>True if any pair of elements satisfies the predicate, false otherwise</returns>
    /// <exception cref="System.ArgumentException">Thrown when the <see cref="FlatList{T}"/>s have different lengths</exception>
    let exists2 predicate list1 list2 =
        checkNotDefault (nameof list1) list1
        checkNotDefault (nameof list2) list2
        let f = OptimizedClosures.FSharpFunc<_, _, _>.Adapt (predicate)
        let len1 = list1.Length

        if len1 <> list2.Length then
            invalidArg (nameof list2) ErrorStrings.ListsHaveDifferentLengths

        let rec loop i =
            i < len1
            && (f.Invoke (list1.[i], list2.[i]) || loop (i + 1))

        loop 0

    /// <summary>Tests if all elements of the <see cref="FlatList{T}"/> satisfy the given predicate</summary>
    /// <param name="predicate">A function to test each element</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>True if all elements satisfy the predicate, false otherwise</returns>
    let forall predicate list =
        check list
        let len = list.Length
        let rec loop i = i >= len || (predicate list.[i] && loop (i + 1))
        loop 0

    /// <summary>Tests if all corresponding pairs of elements from the two <see cref="FlatList{T}"/>s satisfy the given predicate</summary>
    /// <param name="predicate">A function to test pairs of elements</param>
    /// <param name="list1">The first input <see cref="FlatList{T}"/></param>
    /// <param name="list2">The second input <see cref="FlatList{T}"/></param>
    /// <returns>True if all pairs of elements satisfy the predicate, false otherwise</returns>
    /// <exception cref="System.ArgumentException">Thrown when the <see cref="FlatList{T}"/>s have different lengths</exception>
    let forall2 predicate list1 list2 =
        checkNotDefault (nameof list1) list1
        checkNotDefault (nameof list2) list2
        let f = OptimizedClosures.FSharpFunc<_, _, _>.Adapt (predicate)
        let len1 = list1.Length

        if len1 <> list2.Length then
            invalidArg (nameof list2) ErrorStrings.ListsHaveDifferentLengths

        let rec loop i =
            i >= len1
            || (f.Invoke (list1.[i], list2.[i]) && loop (i + 1))

        loop 0

    /// <summary>Tests if the given element exists in the <see cref="FlatList{T}"/></summary>
    /// <param name="e">The element to find</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>True if the element exists in the <see cref="FlatList{T}"/>, false otherwise</returns>
    let inline contains e list =
        check list
        let mutable state = false
        let mutable i = 0

        while (not state && i < list.Length) do
            state <- e = list.[i]
            i <- i + 1

        state

    /// <summary>Splits the <see cref="FlatList{T}"/> into two <see cref="FlatList{T}"/>s, containing the elements for which the given predicate returns true and false respectively</summary>
    /// <param name="predicate">A function to test each element</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>A tuple of two <see cref="FlatList{T}"/>s, containing the elements for which the predicate returns true and false respectively</returns>
    let partition predicate list =
        check list
        let res1 = builderWith list.Length
        let res2 = builderWith list.Length

        for i = 0 to list.Length - 1 do
            let x = list.[i]
            if predicate x then res1.Add (x) else res2.Add (x)

        ofBuilder res1, ofBuilder res2

    /// <summary>Returns the first element for which the given predicate returns true</summary>
    /// <param name="predicate">A function to test elements</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The first element for which the predicate returns true</returns>
    /// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown if no element satisfies the predicate</exception>
    let find predicate list =
        check list

        let rec loop i =
            if i >= list.Length then indexNotFound ()
            else if predicate list.[i] then list.[i]
            else loop (i + 1)

        loop 0

    /// <summary>Returns the first element for which the given predicate returns true, or None if no such element exists</summary>
    /// <param name="predicate">A function to test elements</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>Some value if an element satisfies the predicate, None otherwise</returns>
    let tryFind predicate list =
        check list

        let rec loop i =
            if i >= list.Length then None
            else if predicate list.[i] then Some list.[i]
            else loop (i + 1)

        loop 0

    /// <summary>Returns the last element for which the given predicate returns true</summary>
    /// <param name="predicate">A function to test elements</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The last element for which the predicate returns true</returns>
    /// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown if no element satisfies the predicate</exception>
    let findBack predicate list =
        check list

        let rec loop i =
            if i < 0 then indexNotFound ()
            else if predicate list.[i] then list.[i]
            else loop (i - 1)

        loop <| length list - 1

    /// <summary>Returns the last element for which the given predicate returns true, or None if no such element exists</summary>
    /// <param name="predicate">A function to test elements</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>Some value if an element satisfies the predicate, None otherwise</returns>
    let tryFindBack predicate list =
        check list

        let rec loop i =
            if i < 0 then None
            else if predicate list.[i] then Some list.[i]
            else loop (i - 1)

        loop <| length list - 1

    /// <summary>Returns the last index for which the given predicate returns true</summary>
    /// <param name="predicate">A function to test elements</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The last index for which the predicate returns true</returns>
    /// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown if no element satisfies the predicate</exception>
    let findIndexBack predicate list =
        check list

        let rec loop i =
            if i < 0 then indexNotFound ()
            else if predicate list.[i] then i
            else loop (i - 1)

        loop <| length list - 1

    /// <summary>Returns the last index for which the given predicate returns true, or None if no such element exists</summary>
    /// <param name="predicate">A function to test elements</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>Some index if an element satisfies the predicate, None otherwise</returns>
    let tryFindIndexBack predicate list =
        check list

        let rec loop i =
            if i < 0 then None
            else if predicate list.[i] then Some i
            else loop (i - 1)

        loop <| length list - 1

    /// <summary>Returns the first value for which the given function returns Some value</summary>
    /// <param name="chooser">A function to generate options from the elements</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The first value for which the chooser returns Some value</returns>
    /// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown if the chooser returns None for all elements</exception>
    let pick chooser list =
        check list

        let rec loop i =
            if i >= list.Length then
                indexNotFound ()
            else
                match chooser list.[i] with
                | None -> loop (i + 1)
                | Some res -> res

        loop 0

    /// <summary>Returns the first value for which the given function returns Some value, or None if no such element exists</summary>
    /// <param name="chooser">A function to generate options from the elements</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The first value for which the chooser returns Some value, or None</returns>
    let tryPick chooser list =
        check list

        let rec loop i =
            if i >= list.Length then
                None
            else
                match chooser list.[i] with
                | None -> loop (i + 1)
                | res -> res

        loop 0

    /// <summary>Builds a new <see cref="FlatList{T}"/> containing only the elements for which the given function returns Some value</summary>
    /// <param name="chooser">A function to generate options from the elements</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>A <see cref="FlatList{T}"/> containing the values wrapped in Some by the chooser</returns>
    let choose chooser list =
        check list
        let res = builderWith list.Length

        for i = 0 to list.Length - 1 do
            match chooser list.[i] with
            | None -> ()
            | Some b -> res.Add (b)

        ofBuilder res

    /// <summary>Creates a <see cref="FlatList{T}"/> by applying a key-generating function to each element of the <see cref="FlatList{T}"/> and grouping the elements by the resulting keys</summary>
    /// <param name="projection">A function to transform elements into keys</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>A <see cref="FlatList{T}"/> of tuples where each tuple contains a key and a <see cref="FlatList{T}"/> of all elements that match the key</returns>
    let groupBy projection list =
        check list
        let dict = new System.Collections.Generic.Dictionary<'Key, ResizeArray<'T>> (HashIdentity.Structural)

        // Build the groupings
        for i = 0 to (list.Length - 1) do
            let v = list.[i]
            let key = projection v
            let ok, prev = dict.TryGetValue (key)

            if ok then
                prev.Add (v)
            else
                let prev = new ResizeArray<'T> (1)
                dict.[key] <- prev
                prev.Add (v)

        // Return the <see cref="FlatList{T}"/>-of-<see cref="FlatList{T}"/>s.
        let result = builderWith dict.Count
        let mutable i = 0

        for group in dict do
            result.Add (group.Key, ofSeq group.Value)
            i <- i + 1

        moveFromBuilder result

    /// <summary>Returns a new <see cref="FlatList{T}"/> that contains the elements of the original <see cref="FlatList{T}"/> but with duplicates eliminated by using the supplied projection function</summary>
    /// <param name="projection">A function to transform elements before comparing them</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>A <see cref="FlatList{T}"/> with distinct elements as determined by the projection function</returns>
    let distinctBy projection (list : FlatList<'T>) =
        let builder : FlatList<'T>.Builder = builderWith <| length list
        let set = System.Collections.Generic.HashSet<'Key> (HashIdentity.Structural)
        let mutable outputIndex = 0

        for i = 0 to length list - 1 do
            let item = list.[i]

            if set.Add <| projection item then
                outputIndex <- outputIndex + 1
                Builder.add item builder

        ofBuilder builder

    /// <summary>Creates a new <see cref="FlatList{T}"/> by applying a mapping function to each element of the input <see cref="FlatList{T}"/> and concatenating the results</summary>
    /// <param name="mapping">A function to transform elements of the input <see cref="FlatList{T}"/> into <see cref="FlatList{T}"/>s</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>A <see cref="FlatList{T}"/> containing the concatenation of all the <see cref="FlatList{T}"/>s generated by the mapping function</returns>
    let collect mapping list = concat <| map mapping list

    /// <summary>Gets an element in the <see cref="FlatList{T}"/> at the specified index</summary>
    /// <param name="index">The index of the element to retrieve</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>Some value containing the element, or None if the index is out of range</returns>
    let tryItem index list =
        if index >= length list || index < 0 then
            None
        else
            Some (list.[index])

    /// <summary>Returns the first element of the <see cref="FlatList{T}"/></summary>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The first element of the <see cref="FlatList{T}"/></returns>
    /// <exception cref="System.ArgumentException">Thrown when the <see cref="FlatList{T}"/> is empty</exception>
    let head list = item 0 list

    /// <summary>Returns the first element of the <see cref="FlatList{T}"/>, or None if the <see cref="FlatList{T}"/> is empty</summary>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>Some value containing the first element, or None if the <see cref="FlatList{T}"/> is empty</returns>
    let tryHead list = tryItem 0 list

    /// <summary>Returns the last element of the <see cref="FlatList{T}"/></summary>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The last element of the <see cref="FlatList{T}"/></returns>
    /// <exception cref="System.ArgumentException">Thrown when the <see cref="FlatList{T}"/> is empty</exception>
    let last (list : FlatList<_>) = list.[length list - 1]

    /// <summary>Returns the last element of the <see cref="FlatList{T}"/>, or None if the <see cref="FlatList{T}"/> is empty</summary>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>Some value containing the last element, or None if the <see cref="FlatList{T}"/> is empty</returns>
    let tryLast list = tryItem (length list - 1) list

    /// <summary>Returns the <see cref="FlatList{T}"/> without its first element</summary>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>A <see cref="FlatList{T}"/> containing all elements of the input <see cref="FlatList{T}"/> except the first one</returns>
    let tail list = removeRange 1 (length list - 1) list

    /// <summary>Returns the <see cref="FlatList{T}"/> without its first element, or None if the <see cref="FlatList{T}"/> is empty</summary>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>Some value containing the <see cref="FlatList{T}"/> without its first element, or None if the <see cref="FlatList{T}"/> is empty</returns>
    let tryTail list = if isEmpty list then None else Some <| tail list

    /// <summary>Returns the first N elements of the <see cref="FlatList{T}"/></summary>
    /// <param name="count">The number of elements to take</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>A <see cref="FlatList{T}"/> containing the first N elements</returns>
    let take count list = removeRange count (length list - count) list

    /// <summary>Returns a <see cref="FlatList{T}"/> containing the first elements of the input <see cref="FlatList{T}"/> for which the given predicate returns true</summary>
    /// <param name="predicate">A function to test each element</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>A <see cref="FlatList{T}"/> containing the first elements for which the predicate returns true</returns>
    let takeWhile predicate list = take (lengthWhile predicate list) list

    /// <summary>Returns the <see cref="FlatList{T}"/> without its first N elements</summary>
    /// <param name="index">The number of elements to skip</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>A <see cref="FlatList{T}"/> containing all except the first N elements</returns>
    let skip index list = removeRange 0 index list

    /// <summary>Returns a <see cref="FlatList{T}"/> that skips the elements of the input <see cref="FlatList{T}"/> while the given predicate returns true, then returns the rest</summary>
    /// <param name="predicate">A function to test each element</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>A <see cref="FlatList{T}"/> that skips the elements while the predicate returns true, then contains the rest</returns>
    let skipWhile predicate list = skip (lengthWhile predicate list) list

    /// <summary>Gets a sublist of the input <see cref="FlatList{T}"/></summary>
    /// <param name="start">The index of the first element to include</param>
    /// <param name="stop">The index of the element at which to end (exclusive)</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>A <see cref="FlatList{T}"/> containing the elements from start to stop-1</returns>
    let sub start stop list = skip start list |> take (stop - start - 1)

    /// <summary>Returns a <see cref="FlatList{T}"/> that contains no more than N elements of the input <see cref="FlatList{T}"/></summary>
    /// <param name="count">The maximum number of elements to include</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>A <see cref="FlatList{T}"/> containing at most N elements</returns>
    let truncate count list = if count < length list then take count list else list

    /// <summary>Splits the <see cref="FlatList{T}"/> into two <see cref="FlatList{T}"/>s at the specified index</summary>
    /// <param name="index">The index at which to split the <see cref="FlatList{T}"/></param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>A tuple of two <see cref="FlatList{T}"/>s, the first containing the elements up to the index, the second containing the rest</returns>
    let splitAt index list = take index list, skip index list

    /// <summary>Applies a function to the builder and returns the resulting <see cref="FlatList{T}"/></summary>
    /// <param name="f">The function to apply to the builder</param>
    /// <returns>The <see cref="FlatList{T}"/> created from the builder after applying the function</returns>
    let inline build f =
        let builder = builder ()
        f builder
        moveFromBuilder builder

    /// <summary>Updates the <see cref="FlatList{T}"/> by applying a function to a builder initialized with the <see cref="FlatList{T}"/>'s elements</summary>
    /// <param name="f">The function to apply to the builder</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The updated <see cref="FlatList{T}"/></returns>
    let inline update f list =
        let builder = toBuilder list
        f builder
        moveFromBuilder builder

    /// <summary>Returns the index of the first element in the <see cref="FlatList{T}"/> that satisfies the given predicate</summary>
    /// <param name="predicate">The function to test the input elements</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The index of the first element that satisfies the predicate</returns>
    /// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown if no element satisfies the predicate</exception>
    let findIndex predicate list =
        check list
        let len = list.Length
        let rec loop i =
            if i >= len then indexNotFound ()
            elif predicate list.[i] then i
            else loop (i + 1)
        loop 0

    /// <summary>Returns the index of the first element in the <see cref="FlatList{T}"/> that satisfies the given predicate, or None if no such element exists</summary>
    /// <param name="predicate">The function to test the input elements</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The index of the first element that satisfies the predicate, or None</returns>
    let tryFindIndex predicate list =
        check list
        let len = list.Length
        let rec loop i =
            if i >= len then None
            elif predicate list.[i] then Some i
            else loop (i + 1)
        loop 0

    /// <summary>Returns a new <see cref="FlatList{T}"/> containing elements corresponding to a sliding window of elements from the input <see cref="FlatList{T}"/></summary>
    /// <param name="windowSize">The size of the window</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The resulting <see cref="FlatList{T}"/> of sliding windows</returns>
    /// <exception cref="System.ArgumentException">Thrown when windowSize is not positive or when <see cref="FlatList{T}"/> is default</exception>
    let windowed windowSize list =
        check list
        if windowSize <= 0 then
            invalidArg (nameof windowSize) ErrorStrings.InputMustBeNonNegative

        let len = list.Length
        if windowSize > len then
            empty
        else
            let res = builderWith (len - windowSize + 1)
            for i = 0 to len - windowSize do
                let window = builderWith windowSize
                for j = 0 to windowSize - 1 do
                    window.Add list.[i + j]
                res.Add (ofBuilder window)
            moveFromBuilder res

    /// <summary>Returns a new <see cref="FlatList{T}"/> containing pairs of adjacent elements from the input <see cref="FlatList{T}"/></summary>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The resulting <see cref="FlatList{T}"/> of pairs</returns>
    let pairwise list =
        check list
        let len = list.Length
        if len < 2 then
            empty
        else
            let res = builderWith (len - 1)
            for i = 0 to len - 2 do
                res.Add (list.[i], list.[i + 1])
            moveFromBuilder res

    /// <summary>Splits the <see cref="FlatList{T}"/> into chunks of size at most 'chunkSize'</summary>
    /// <param name="chunkSize">The maximum size of each chunk</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The <see cref="FlatList{T}"/> split into chunks</returns>
    /// <exception cref="System.ArgumentException">Thrown when chunkSize is not positive or when <see cref="FlatList{T}"/> is default</exception>
    let chunkBySize chunkSize list =
        check list
        if chunkSize <= 0 then
            invalidArg (nameof chunkSize) ErrorStrings.InputMustBeNonNegative

        let len = list.Length
        if len = 0 then
            empty
        else
            let chunkCount = (len + chunkSize - 1) / chunkSize
            let res = builderWith chunkCount

            for i = 0 to chunkCount - 1 do
                let startIndex = i * chunkSize
                let size = min chunkSize (len - startIndex)
                let chunk = builderWith size

                for j = 0 to size - 1 do
                    chunk.Add list.[startIndex + j]

                res.Add (ofBuilder chunk)

            moveFromBuilder res

    /// <summary>Returns a new <see cref="FlatList{T}"/> that contains the elements of the original <see cref="FlatList{T}"/> but with duplicates removed</summary>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The <see cref="FlatList{T}"/> with distinct elements</returns>
    let distinct list =
        check list
        let len = list.Length
        let res = builderWith len
        let set = System.Collections.Generic.HashSet<'T> (HashIdentity.Structural)

        for i = 0 to len - 1 do
            let item = list.[i]
            if set.Add item then
                res.Add item

        ofBuilder res

    /// <summary>Returns a new <see cref="FlatList{T}"/> that contains all pairwise combinations of elements from the first and second <see cref="FlatList{T}"/>s</summary>
    /// <param name="xs">The first input <see cref="FlatList{T}"/></param>
    /// <param name="ys">The second input <see cref="FlatList{T}"/></param>
    /// <returns>The <see cref="FlatList{T}"/> of all pairwise combinations</returns>
    let allPairs xs ys =
        check xs
        check ys
        let lenXs = xs.Length
        let lenYs = ys.Length
        let res = builderWith (lenXs * lenYs)

        for i = 0 to lenXs - 1 do
            for j = 0 to lenYs - 1 do
                res.Add (xs.[i], ys.[j])

        moveFromBuilder res

    /// <summary>Returns a new <see cref="FlatList{T}"/> with the elements permuted according to the specified permutation</summary>
    /// <param name="indexMap">The function that maps input indices to output indices</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The permuted <see cref="FlatList{T}"/></returns>
    /// <exception cref="System.ArgumentException">Thrown when the permutation function returns an out-of-range index</exception>
    let permute indexMap list =
        check list
        let len = list.Length
        let res = builderWith len
        let permuted = Array.zeroCreate len

        for i = 0 to len - 1 do
            let j = indexMap i
            if j < 0 || j >= len then
                invalidArg (nameof indexMap) "Invalid permutation"
            permuted.[j] <- list.[i]

        for i = 0 to len - 1 do
            res.Add permuted.[i]

        moveFromBuilder res

    /// <summary>Combines the two <see cref="FlatList{T}"/>s into a <see cref="FlatList{T}"/> of pairs. The two <see cref="FlatList{T}"/>s must have equal lengths</summary>
    /// <param name="list1">The first input <see cref="FlatList{T}"/></param>
    /// <param name="list2">The second input <see cref="FlatList{T}"/></param>
    /// <returns>The <see cref="FlatList{T}"/> of pairs</returns>
    /// <exception cref="System.ArgumentException">Thrown when the <see cref="FlatList{T}"/>s have different lengths</exception>
    let zip list1 list2 =
        checkNotDefault (nameof list1) list1
        checkNotDefault (nameof list2) list2

        let len1 = list1.Length
        if len1 <> list2.Length then
            invalidArg (nameof list2) ErrorStrings.ListsHaveDifferentLengths

        let res = builderWith len1
        for i = 0 to len1 - 1 do
            res.Add (list1.[i], list2.[i])

        moveFromBuilder res

    /// <summary>Combines the three <see cref="FlatList{T}"/>s into a <see cref="FlatList{T}"/> of triples. The three <see cref="FlatList{T}"/>s must have equal lengths</summary>
    /// <param name="list1">The first input <see cref="FlatList{T}"/></param>
    /// <param name="list2">The second input <see cref="FlatList{T}"/></param>
    /// <param name="list3">The third input <see cref="FlatList{T}"/></param>
    /// <returns>The <see cref="FlatList{T}"/> of triples</returns>
    /// <exception cref="System.ArgumentException">Thrown when the <see cref="FlatList{T}"/>s have different lengths</exception>
    let zip3 list1 list2 list3 =
        checkNotDefault (nameof list1) list1
        checkNotDefault (nameof list2) list2
        checkNotDefault (nameof list3) list3

        let len1 = list1.Length
        if len1 <> list2.Length then
            invalidArg (nameof list2) ErrorStrings.ListsHaveDifferentLengths
        if len1 <> list3.Length then
            invalidArg (nameof list3) ErrorStrings.ListsHaveDifferentLengths

        let res = builderWith len1
        for i = 0 to len1 - 1 do
            res.Add (list1.[i], list2.[i], list3.[i])

        moveFromBuilder res

    /// <summary>Splits a <see cref="FlatList{T}"/> of pairs into two <see cref="FlatList{T}"/>s</summary>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The two <see cref="FlatList{T}"/>s unzipped from the input <see cref="FlatList{T}"/></returns>
    let unzip list =
        check list
        let len = list.Length
        let res1 = builderWith len
        let res2 = builderWith len

        for i = 0 to len - 1 do
            let x, y = list.[i]
            res1.Add x
            res2.Add y

        ofBuilder res1, ofBuilder res2

    /// <summary>Splits a <see cref="FlatList{T}"/> of triples into three <see cref="FlatList{T}"/>s</summary>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The three <see cref="FlatList{T}"/>s unzipped from the input <see cref="FlatList{T}"/></returns>
    let unzip3 list =
        check list
        let len = list.Length
        let res1 = builderWith len
        let res2 = builderWith len
        let res3 = builderWith len

        for i = 0 to len - 1 do
            let x, y, z = list.[i]
            res1.Add x
            res2.Add y
            res3.Add z

        ofBuilder res1, ofBuilder res2, ofBuilder res3

    /// <summary>Returns the average of the elements in the <see cref="FlatList{T}"/></summary>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The average of the elements</returns>
    /// <exception cref="System.ArgumentException">Thrown when the <see cref="FlatList{T}"/> is empty</exception>
    let average list =
        check list
        let len = list.Length
        if len = 0 then
            invalidArg (nameof list) "The input list was empty."

        let mutable sum = 0.0
        for i = 0 to len - 1 do
            sum <- sum + float list.[i]

        sum / float len

    /// <summary>Returns the average of the results of applying the function to each element of the <see cref="FlatList{T}"/></summary>
    /// <param name="projection">The function to transform the <see cref="FlatList{T}"/> elements before averaging</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The average of the projected elements</returns>
    /// <exception cref="System.ArgumentException">Thrown when the <see cref="FlatList{T}"/> is empty</exception>
    let averageBy projection list =
        check list
        let len = list.Length
        if len = 0 then
            invalidArg (nameof list) "The input list was empty."

        let mutable sum = 0.0
        for i = 0 to len - 1 do
            sum <- sum + (float (projection list.[i]))

        sum / float len

    /// <summary>Applies a function to each element of the <see cref="FlatList{T}"/>, threading an accumulator argument through the computation</summary>
    /// <param name="folder">The function to update the state given the input elements</param>
    /// <param name="state">The initial state</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The final state</returns>
    let fold<'T, 'State> folder (state : 'State) (list : FlatList<'T>) =
        check list
        let mutable state = state
        for i = 0 to list.Length - 1 do
            state <- folder state list.[i]
        state

    /// <summary>Applies a function to corresponding elements of two <see cref="FlatList{T}"/>s, threading an accumulator argument through the computation</summary>
    /// <param name="folder">The function to update the state given the input elements from both <see cref="FlatList{T}"/>s</param>
    /// <param name="state">The initial state</param>
    /// <param name="list1">The first input <see cref="FlatList{T}"/></param>
    /// <param name="list2">The second input <see cref="FlatList{T}"/></param>
    /// <returns>The final state</returns>
    /// <exception cref="System.ArgumentException">Thrown when the <see cref="FlatList{T}"/>s have different lengths</exception>
    let fold2<'T1, 'T2, 'State> folder (state : 'State) (list1 : FlatList<'T1>) (list2 : FlatList<'T2>) =
        checkNotDefault (nameof list1) list1
        checkNotDefault (nameof list2) list2
        let f = OptimizedClosures.FSharpFunc<_, _, _, _>.Adapt (folder)
        let len1 = list1.Length

        if len1 <> list2.Length then
            invalidArg (nameof list2) ErrorStrings.ListsHaveDifferentLengths

        let mutable state = state
        for i = 0 to len1 - 1 do
            state <- f.Invoke (state, list1.[i], list2.[i])
        state

    /// <summary>Applies a function to each element of the <see cref="FlatList{T}"/>, threading an accumulator argument through the computation, starting from the end</summary>
    /// <param name="folder">The function to update the state given the input elements, starting from the end</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <param name="state">The initial state</param>
    /// <returns>The final state</returns>
    let foldBack<'T, 'State> folder (list : FlatList<'T>) (state : 'State) =
        check list
        let mutable state = state
        for i = list.Length - 1 downto 0 do
            state <- folder list.[i] state
        state

    /// <summary>Applies a function to corresponding elements of two <see cref="FlatList{T}"/>s, threading an accumulator argument through the computation, starting from the end</summary>
    /// <param name="folder">The function to update the state given the input elements from both <see cref="FlatList{T}"/>s, starting from the end</param>
    /// <param name="list1">The first input <see cref="FlatList{T}"/></param>
    /// <param name="list2">The second input <see cref="FlatList{T}"/></param>
    /// <param name="state">The initial state</param>
    /// <returns>The final state</returns>
    /// <exception cref="System.ArgumentException">Thrown when the <see cref="FlatList{T}"/>s have different lengths</exception>
    let foldBack2<'T1, 'T2, 'State> folder (list1 : FlatList<'T1>) (list2 : FlatList<'T2>) (state : 'State) =
        checkNotDefault (nameof list1) list1
        checkNotDefault (nameof list2) list2
        let f = OptimizedClosures.FSharpFunc<_, _, _, _>.Adapt (folder)
        let len1 = list1.Length

        if len1 <> list2.Length then
            invalidArg (nameof list2) ErrorStrings.ListsHaveDifferentLengths

        let mutable state = state
        for i = len1 - 1 downto 0 do
            state <- f.Invoke (list1.[i], list2.[i], state)
        state

    /// <summary>Applies a function to each element of the <see cref="FlatList{T}"/>, threading an accumulator argument through the computation.
    /// This function takes the second argument, and applies the function to it and the first element of the <see cref="FlatList{T}"/>.
    /// Then, it passes this result into the function along with the second element, and so on.
    /// Finally, it returns the final result. If the <see cref="FlatList{T}"/> is empty, an exception is raised.</summary>
    /// <param name="operation">The function to reduce the <see cref="FlatList{T}"/> with</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The final accumulated value</returns>
    /// <exception cref="System.ArgumentException">Thrown when the <see cref="FlatList{T}"/> is empty</exception>
    let reduce operation list =
        check list
        if list.Length = 0 then
            invalidArg (nameof list) "The input list was empty."

        let mutable state = list.[0]
        for i = 1 to list.Length - 1 do
            state <- operation state list.[i]
        state

    /// <summary>Applies a function to each element of the <see cref="FlatList{T}"/>, threading an accumulator argument through the computation, starting from the end.
    /// This function takes the last element of the <see cref="FlatList{T}"/> and the second-to-last element, and applies the function to them.
    /// Then, it passes this result into the function along with the third-to-last element, and so on.
    /// Finally, it returns the final result. If the <see cref="FlatList{T}"/> is empty, an exception is raised.</summary>
    /// <param name="operation">The function to reduce the <see cref="FlatList{T}"/> with, starting from the end</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The final accumulated value</returns>
    /// <exception cref="System.ArgumentException">Thrown when the <see cref="FlatList{T}"/> is empty</exception>
    let reduceBack operation list =
        check list
        if list.Length = 0 then
            invalidArg (nameof list) "The input list was empty."

        let len = list.Length
        let mutable state = list.[len - 1]
        for i = len - 2 downto 0 do
            state <- operation list.[i] state
        state

    /// <summary>Like fold, but returns both the intermediate and final results</summary>
    /// <param name="folder">The function to update the state given the input elements</param>
    /// <param name="state">The initial state</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <returns>The <see cref="FlatList{T}"/> of all intermediate and final states</returns>
    let scan<'T, 'State> folder (state : 'State) (list : FlatList<'T>) =
        check list
        let len = list.Length
        let res = builderWith (len + 1)
        res.Add state

        let mutable state = state
        for i = 0 to len - 1 do
            state <- folder state list.[i]
            res.Add state

        moveFromBuilder res

    /// <summary>Like foldBack, but returns both the intermediate and final results</summary>
    /// <param name="folder">The function to update the state given the input elements, starting from the end</param>
    /// <param name="list">The input <see cref="FlatList{T}"/></param>
    /// <param name="state">The initial state</param>
    /// <returns>The <see cref="FlatList{T}"/> of all intermediate and final states, in reverse order of computation</returns>
    let scanBack<'T, 'State> folder (list : FlatList<'T>) (state : 'State) =
        check list
        let len = list.Length
        let res = builderWith (len + 1)
        let mutable states = Array.zeroCreate (len + 1)
        states.[len] <- state

        let mutable state = state
        for i = len - 1 downto 0 do
            state <- folder list.[i] state
            states.[i] <- state

        for i = 0 to len do
            res.Add states.[i]

        moveFromBuilder res

        /// <summary>Tries to reduce the <see cref="FlatList{T}"/> using the given function, returning ValueNone if empty.</summary>
    let tryReduce reduction (list: FlatList<'T>) : voption<'T> =
        check list
        if list.Length = 0 then ValueNone
        else
            let mutable state = list.[0]
            for i = 1 to list.Length - 1 do
                state <- reduction state list.[i]
            ValueSome state

    /// <summary>Tries to reduce the <see cref="FlatList{T}"/> from the end using the given function, returning ValueNone if empty.</summary>
    let tryReduceBack reduction (list: FlatList<'T>) : voption<'T> =
        check list
        let len = list.Length
        if len = 0 then ValueNone
        else
            let mutable state = list.[len - 1]
            for i = len - 2 downto 0 do
                state <- reduction list.[i] state
            ValueSome state

    /// <summary>Tries to compute the average of the <see cref="FlatList{T}"/>, returning ValueNone if empty.</summary>
    let tryAverage (list: FlatList<'T>) : voption<float> =
        check list
        let len = list.Length
        if len = 0 then ValueNone
        else
            let mutable sum = 0.0
            for i = 0 to len - 1 do
                sum <- sum + float list.[i]
            ValueSome (sum / float len)

    /// <summary>Tries to compute the average of the projected values of the <see cref="FlatList{T}"/>, returning ValueNone if empty.</summary>
    let tryAverageBy projection (list: FlatList<'T>) : voption<float> =
        check list
        let len = list.Length
        if len = 0 then ValueNone
        else
            let mutable sum = 0.0
            for i = 0 to len - 1 do
                sum <- sum + float (projection list.[i])
            ValueSome (sum / float len)

//////////

module ImmutableArray = FlatList
