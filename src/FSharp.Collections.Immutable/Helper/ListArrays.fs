/// <summary>
/// The functions of the FSharp.Core <see cref="T:Microsoft.FSharp.Collections.ListModule"/> module that walk two or three
/// lists together, over arrays, for the modules that mirror it and work on snapshots of their collections (ADR 0001, D5
/// and D6).
/// </summary>
/// <remarks>
/// <para>
/// The FSharp.Core <see cref="T:Microsoft.FSharp.Collections.ArrayModule"/> module has these functions too, but it handles
/// arrays of different lengths differently. Its functions compare the lengths before they call the callback once, and
/// report arrays. The functions of <see cref="T:Microsoft.FSharp.Collections.ListModule"/> walk the lists together, call
/// the callback for the common prefix, compare the lengths when the shorter list ends, and report lists. A callback with
/// a side effect shows the difference, and so does a predicate that decides before the end.
/// </para>
/// <para>
/// Each function here applies its callback to the elements of the common prefix in order and then checks the lengths with
/// the names that the caller passes (see <see cref="T:FSharp.Collections.Immutable.ListChecks"/>), so that the callback
/// sees what it sees in a list before the exception. A predicate of
/// <see cref="M:FSharp.Collections.Immutable.ListArrays.exists2``2(System.String,Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpFunc{``1,System.Boolean}},``0[],``1[])"/>
/// or
/// <see cref="M:FSharp.Collections.Immutable.ListArrays.forall2``2(System.String,System.String,Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpFunc{``1,System.Boolean}},``0[],``1[])"/>
/// that decides early ends the walk before the check, as in a list.
/// </para>
/// <para>
/// The summary of each function, the descriptions of its parameters and of its result are those of the function of
/// <see cref="T:Microsoft.FSharp.Collections.ListModule"/> (FSharp.Core 10.1), with arrays where the original says lists or
/// collections. The remarks say what differs and why the function exists.
/// </para>
/// </remarks>
module internal FSharp.Collections.Immutable.ListArrays

/// <summary>
/// Tests if any pair of corresponding elements of the arrays satisfies the given predicate.
/// </summary>
/// <param name="name2">The name to report for the second array; FSharp.Core uses <c>"list2"</c>.</param>
/// <param name="predicate">The function to test the input elements.</param>
/// <param name="array1">The first input array.</param>
/// <param name="array2">The second input array.</param>
/// <returns>True if any pair of elements satisfy the predicate.</returns>
/// <exception cref="T:System.ArgumentException">
/// No pair of the common prefix satisfies the predicate, and the arrays differ in length. The exception is the one of
/// <see cref="M:FSharp.Collections.Immutable.ListChecks.exists2(System.String,System.Int32,System.Int32)"/>.
/// </exception>
/// <remarks>
/// <para>
/// The predicate is applied to matching elements in the two arrays up to the lesser of the two lengths. If any application
/// returns true then the overall result is true and no further elements are tested. Otherwise, if one array is longer than
/// the other then the <see cref="T:System.ArgumentException"/> exception is raised. Otherwise, false is returned. This is
/// an O(n) operation in the worst case, where n is the length of the arrays.
/// </para>
/// <para>
/// It exists because
/// <see cref="M:Microsoft.FSharp.Collections.ArrayModule.Exists2``2(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpFunc{``1,System.Boolean}},``0[],``1[])"/>
/// compares the lengths before it calls the predicate and reports arrays, where
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.Exists2``2(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpFunc{``1,System.Boolean}},Microsoft.FSharp.Collections.FSharpList{``0},Microsoft.FSharp.Collections.FSharpList{``1})"/>
/// calls the predicate for the common prefix first and reports lists.
/// </para>
/// </remarks>
let exists2 (name2 : string) (predicate : 'T1 -> 'T2 -> bool) (array1 : 'T1 array) (array2 : 'T2 array) =
    let common = min array1.Length array2.Length
    let mutable found = false
    let mutable index = 0

    while not found && index < common do
        found <- predicate array1[index] array2[index]
        index <- index + 1

    if not found then
        ListChecks.exists2 name2 array1.Length array2.Length

    found

/// <summary>
/// Applies a function to corresponding elements of two collections, threading an accumulator argument through the
/// computation. The collections must have identical sizes. If the input function is <c>f</c> and the elements are
/// <c>i0...iN</c> and <c>j0...jN</c> then computes <c>f (... (f s i0 j0)...) iN jN</c>.
/// </summary>
/// <param name="name1">The name to report for the first array; FSharp.Core uses <c>"list1"</c>.</param>
/// <param name="name2">The name to report for the second array; FSharp.Core uses <c>"list2"</c>.</param>
/// <param name="folder">The function to update the state given the input elements.</param>
/// <param name="state">The initial state.</param>
/// <param name="array1">The first input array.</param>
/// <param name="array2">The second input array.</param>
/// <returns>The final state value.</returns>
/// <exception cref="T:System.ArgumentException">
/// The arrays differ in length. The folder has folded the pairs of the common prefix by then. The exception is the one of
/// <see cref="M:FSharp.Collections.Immutable.ListChecks.sameLengths(System.String,System.Int32,System.String,System.Int32)"/>.
/// </exception>
/// <remarks>
/// <para>
/// This is an O(n) operation, where n is the length of the arrays.
/// </para>
/// <para>
/// It exists because
/// <see cref="M:Microsoft.FSharp.Collections.ArrayModule.Fold2``3(Microsoft.FSharp.Core.FSharpFunc{``2,Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpFunc{``1,``2}}},``2,``0[],``1[])"/>
/// compares the lengths before it calls the folder and reports arrays, where
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.Fold2``3(Microsoft.FSharp.Core.FSharpFunc{``2,Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpFunc{``1,``2}}},``2,Microsoft.FSharp.Collections.FSharpList{``0},Microsoft.FSharp.Collections.FSharpList{``1})"/>
/// folds the common prefix first and reports lists.
/// </para>
/// </remarks>
let fold2
    (name1 : string)
    (name2 : string)
    (folder : 'State -> 'T1 -> 'T2 -> 'State)
    (state : 'State)
    (array1 : 'T1 array)
    (array2 : 'T2 array)
    =
    let mutable state = state

    for index = 0 to min array1.Length array2.Length - 1 do
        state <- folder state array1[index] array2[index]

    ListChecks.sameLengths name1 array1.Length name2 array2.Length
    state

/// <summary>
/// Tests if all corresponding elements of the arrays satisfy the given predicate pairwise.
/// </summary>
/// <param name="name1">The name to report for the first array; FSharp.Core uses <c>"list1"</c>.</param>
/// <param name="name2">The name to report for the second array; FSharp.Core uses <c>"list2"</c>.</param>
/// <param name="predicate">The function to test the input elements.</param>
/// <param name="array1">The first input array.</param>
/// <param name="array2">The second input array.</param>
/// <returns>True if all of the pairs of elements satisfy the predicate.</returns>
/// <exception cref="T:System.ArgumentException">
/// Every pair of the common prefix satisfies the predicate, and the arrays differ in length. The exception is the one of
/// <see cref="M:FSharp.Collections.Immutable.ListChecks.sameLengths(System.String,System.Int32,System.String,System.Int32)"/>.
/// </exception>
/// <remarks>
/// <para>
/// The predicate is applied to matching elements in the two arrays up to the lesser of the two lengths. If any application
/// returns false then the overall result is false and no further elements are tested. Otherwise, if one array is longer
/// than the other then the <see cref="T:System.ArgumentException"/> exception is raised. Otherwise, true is returned. This
/// is an O(n) operation in the worst case, where n is the length of the arrays.
/// </para>
/// <para>
/// It exists because
/// <see cref="M:Microsoft.FSharp.Collections.ArrayModule.ForAll2``2(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpFunc{``1,System.Boolean}},``0[],``1[])"/>
/// compares the lengths before it calls the predicate and reports arrays, where
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.ForAll2``2(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpFunc{``1,System.Boolean}},Microsoft.FSharp.Collections.FSharpList{``0},Microsoft.FSharp.Collections.FSharpList{``1})"/>
/// calls the predicate for the common prefix first and reports lists.
/// </para>
/// </remarks>
let forall2 (name1 : string) (name2 : string) (predicate : 'T1 -> 'T2 -> bool) (array1 : 'T1 array) (array2 : 'T2 array) =
    let common = min array1.Length array2.Length
    let mutable holds = true
    let mutable index = 0

    while holds && index < common do
        holds <- predicate array1[index] array2[index]
        index <- index + 1

    if holds then
        ListChecks.sameLengths name1 array1.Length name2 array2.Length

    holds

/// <summary>
/// Applies the given function to two collections simultaneously. The collections must have identical sizes.
/// </summary>
/// <param name="name1">The name to report for the first array; FSharp.Core uses <c>"list1"</c>.</param>
/// <param name="name2">The name to report for the second array; FSharp.Core uses <c>"list2"</c>.</param>
/// <param name="action">The function to apply to pairs of elements from the input arrays.</param>
/// <param name="array1">The first input array.</param>
/// <param name="array2">The second input array.</param>
/// <exception cref="T:System.ArgumentException">
/// The arrays differ in length. The action has been applied to the pairs of the common prefix by then. The exception is the
/// one of
/// <see cref="M:FSharp.Collections.Immutable.ListChecks.sameLengths(System.String,System.Int32,System.String,System.Int32)"/>.
/// </exception>
/// <remarks>
/// <para>
/// This is an O(n) operation, where n is the length of the arrays.
/// </para>
/// <para>
/// It exists because
/// <see cref="M:Microsoft.FSharp.Collections.ArrayModule.Iterate2``2(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpFunc{``1,Microsoft.FSharp.Core.Unit}},``0[],``1[])"/>
/// compares the lengths before it calls the action and reports arrays, where
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.Iterate2``2(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpFunc{``1,Microsoft.FSharp.Core.Unit}},Microsoft.FSharp.Collections.FSharpList{``0},Microsoft.FSharp.Collections.FSharpList{``1})"/>
/// applies the action to the common prefix first and reports lists.
/// </para>
/// </remarks>
let iter2 (name1 : string) (name2 : string) (action : 'T1 -> 'T2 -> unit) (array1 : 'T1 array) (array2 : 'T2 array) =
    for index = 0 to min array1.Length array2.Length - 1 do
        action array1[index] array2[index]

    ListChecks.sameLengths name1 array1.Length name2 array2.Length

/// <summary>
/// Applies the given function to two collections simultaneously. The collections must have identical sizes. The integer
/// passed to the function indicates the index of the element.
/// </summary>
/// <param name="name1">The name to report for the first array; FSharp.Core uses <c>"list1"</c>.</param>
/// <param name="name2">The name to report for the second array; FSharp.Core uses <c>"list2"</c>.</param>
/// <param name="action">The function to apply to a pair of elements from the input arrays along with their index.</param>
/// <param name="array1">The first input array.</param>
/// <param name="array2">The second input array.</param>
/// <exception cref="T:System.ArgumentException">
/// The arrays differ in length. The action has been applied to the pairs of the common prefix by then. The exception is the
/// one of
/// <see cref="M:FSharp.Collections.Immutable.ListChecks.sameLengths(System.String,System.Int32,System.String,System.Int32)"/>.
/// </exception>
/// <remarks>
/// <para>
/// This is an O(n) operation, where n is the length of the arrays.
/// </para>
/// <para>
/// It exists because
/// <see cref="M:Microsoft.FSharp.Collections.ArrayModule.IterateIndexed2``2(Microsoft.FSharp.Core.FSharpFunc{System.Int32,Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpFunc{``1,Microsoft.FSharp.Core.Unit}}},``0[],``1[])"/>
/// compares the lengths before it calls the action and reports arrays, where
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.IterateIndexed2``2(Microsoft.FSharp.Core.FSharpFunc{System.Int32,Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpFunc{``1,Microsoft.FSharp.Core.Unit}}},Microsoft.FSharp.Collections.FSharpList{``0},Microsoft.FSharp.Collections.FSharpList{``1})"/>
/// applies the action to the common prefix first and reports lists.
/// </para>
/// </remarks>
let iteri2 (name1 : string) (name2 : string) (action : int -> 'T1 -> 'T2 -> unit) (array1 : 'T1 array) (array2 : 'T2 array) =
    for index = 0 to min array1.Length array2.Length - 1 do
        action index array1[index] array2[index]

    ListChecks.sameLengths name1 array1.Length name2 array2.Length

/// <summary>
/// Builds a new array whose elements are the results of applying the given function to the corresponding elements of the
/// two arrays pairwise.
/// </summary>
/// <param name="name1">The name to report for the first array; FSharp.Core uses <c>"list1"</c>.</param>
/// <param name="name2">The name to report for the second array; FSharp.Core uses <c>"list2"</c>.</param>
/// <param name="mapping">The function to transform pairs of elements from the input arrays.</param>
/// <param name="array1">The first input array.</param>
/// <param name="array2">The second input array.</param>
/// <returns>The array of transformed elements.</returns>
/// <exception cref="T:System.ArgumentException">
/// The arrays differ in length. The mapping has been applied to the pairs of the common prefix by then. The exception is
/// the one of
/// <see cref="M:FSharp.Collections.Immutable.ListChecks.sameLengths(System.String,System.Int32,System.String,System.Int32)"/>.
/// </exception>
/// <remarks>
/// <para>
/// This is an O(n) operation, where n is the length of the arrays.
/// </para>
/// <para>
/// It exists because
/// <see cref="M:Microsoft.FSharp.Collections.ArrayModule.Map2``3(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpFunc{``1,``2}},``0[],``1[])"/>
/// compares the lengths before it calls the mapping and reports arrays, where
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.Map2``3(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpFunc{``1,``2}},Microsoft.FSharp.Collections.FSharpList{``0},Microsoft.FSharp.Collections.FSharpList{``1})"/>
/// applies the mapping to the common prefix first and reports lists.
/// </para>
/// </remarks>
let map2 (name1 : string) (name2 : string) (mapping : 'T1 -> 'T2 -> 'U) (array1 : 'T1 array) (array2 : 'T2 array) : 'U array =
    let result = Array.zeroCreate (min array1.Length array2.Length)

    for index = 0 to result.Length - 1 do
        result[index] <- mapping array1[index] array2[index]

    ListChecks.sameLengths name1 array1.Length name2 array2.Length
    result

/// <summary>
/// Builds a new array whose elements are the results of applying the given function to the corresponding elements of the
/// three arrays simultaneously.
/// </summary>
/// <param name="name1">The name to report for the first array; FSharp.Core uses <c>"list1"</c>.</param>
/// <param name="name2">The name to report for the second array; FSharp.Core uses <c>"list2"</c>.</param>
/// <param name="name3">The name to report for the third array; FSharp.Core uses <c>"list3"</c>.</param>
/// <param name="mapping">The function to transform triples of elements from the input arrays.</param>
/// <param name="array1">The first input array.</param>
/// <param name="array2">The second input array.</param>
/// <param name="array3">The third input array.</param>
/// <returns>The array of transformed elements.</returns>
/// <exception cref="T:System.ArgumentException">
/// The arrays differ in length. The mapping has been applied to the triples of the common prefix by then. The exception is
/// the one of
/// <see cref="M:FSharp.Collections.Immutable.ListChecks.map3(System.String,System.Int32,System.String,System.Int32,System.String,System.Int32)"/>.
/// </exception>
/// <remarks>
/// <para>
/// This is an O(n) operation, where n is the length of the arrays.
/// </para>
/// <para>
/// It exists because
/// <see cref="M:Microsoft.FSharp.Collections.ArrayModule.Map3``4(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpFunc{``1,Microsoft.FSharp.Core.FSharpFunc{``2,``3}}},``0[],``1[],``2[])"/>
/// compares the lengths before it calls the mapping and reports arrays, where
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.Map3``4(Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpFunc{``1,Microsoft.FSharp.Core.FSharpFunc{``2,``3}}},Microsoft.FSharp.Collections.FSharpList{``0},Microsoft.FSharp.Collections.FSharpList{``1},Microsoft.FSharp.Collections.FSharpList{``2})"/>
/// applies the mapping to the common prefix first and reports lists.
/// </para>
/// </remarks>
let map3
    (name1 : string)
    (name2 : string)
    (name3 : string)
    (mapping : 'T1 -> 'T2 -> 'T3 -> 'U)
    (array1 : 'T1 array)
    (array2 : 'T2 array)
    (array3 : 'T3 array)
    : 'U array =
    let result = Array.zeroCreate (min array1.Length (min array2.Length array3.Length))

    for index = 0 to result.Length - 1 do
        result[index] <- mapping array1[index] array2[index] array3[index]

    ListChecks.map3 name1 array1.Length name2 array2.Length name3 array3.Length
    result

/// <summary>
/// Like
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.MapIndexed``2(Microsoft.FSharp.Core.FSharpFunc{System.Int32,Microsoft.FSharp.Core.FSharpFunc{``0,``1}},Microsoft.FSharp.Collections.FSharpList{``0})"/>,
/// but mapping corresponding elements from two arrays of equal length.
/// </summary>
/// <param name="name1">The name to report for the first array; FSharp.Core uses <c>"list1"</c>.</param>
/// <param name="name2">The name to report for the second array; FSharp.Core uses <c>"list2"</c>.</param>
/// <param name="mapping">The function to transform pairs of elements from the two arrays and their index.</param>
/// <param name="array1">The first input array.</param>
/// <param name="array2">The second input array.</param>
/// <returns>The array of transformed elements.</returns>
/// <exception cref="T:System.ArgumentException">
/// The arrays differ in length. The mapping has been applied to the pairs of the common prefix by then. The exception is
/// the one of
/// <see cref="M:FSharp.Collections.Immutable.ListChecks.sameLengths(System.String,System.Int32,System.String,System.Int32)"/>.
/// </exception>
/// <remarks>
/// <para>
/// This is an O(n) operation, where n is the length of the arrays.
/// </para>
/// <para>
/// It exists because
/// <see cref="M:Microsoft.FSharp.Collections.ArrayModule.MapIndexed2``3(Microsoft.FSharp.Core.FSharpFunc{System.Int32,Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpFunc{``1,``2}}},``0[],``1[])"/>
/// compares the lengths before it calls the mapping and reports arrays, where
/// <see cref="M:Microsoft.FSharp.Collections.ListModule.MapIndexed2``3(Microsoft.FSharp.Core.FSharpFunc{System.Int32,Microsoft.FSharp.Core.FSharpFunc{``0,Microsoft.FSharp.Core.FSharpFunc{``1,``2}}},Microsoft.FSharp.Collections.FSharpList{``0},Microsoft.FSharp.Collections.FSharpList{``1})"/>
/// applies the mapping to the common prefix first and reports lists.
/// </para>
/// </remarks>
let mapi2
    (name1 : string)
    (name2 : string)
    (mapping : int -> 'T1 -> 'T2 -> 'U)
    (array1 : 'T1 array)
    (array2 : 'T2 array)
    : 'U array =
    let result = Array.zeroCreate (min array1.Length array2.Length)

    for index = 0 to result.Length - 1 do
        result[index] <- mapping index array1[index] array2[index]

    ListChecks.sameLengths name1 array1.Length name2 array2.Length
    result
