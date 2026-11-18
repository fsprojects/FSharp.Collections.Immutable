namespace FSharp.Collections.Immutable

open System
open System.Collections.Generic
open System.Runtime.CompilerServices

/// <summary>
/// The LINQ operators for C# that take an <see cref="T:FSharp.Collections.Immutable.EquatableArray`1"/> without boxing
/// it.
/// <para>
/// An <see cref="T:FSharp.Collections.Immutable.EquatableArray`1"/> is a struct. The operators of
/// <see cref="T:System.Linq.Enumerable"/> take an <see cref="T:System.Collections.Generic.IEnumerable`1"/>, so each call
/// boxes the array: one allocation of 24 bytes. The methods of this class take the array itself, so C# binds a call
/// such as <c>array.First ()</c> to them, and the call allocates nothing.
/// </para>
/// <para>
/// The operators are those that <see cref="T:System.Linq.ImmutableArrayExtensions"/> gives an
/// <see cref="T:System.Collections.Immutable.ImmutableArray`1"/>, and each method calls the method of that class. A
/// method gives the result, and throws the exception, of the operator of <see cref="T:System.Linq.Enumerable"/> that it
/// replaces. The one difference of <see cref="T:System.Linq.ImmutableArrayExtensions"/>, the default value that
/// <see cref="M:System.Linq.ImmutableArrayExtensions.Aggregate``1(System.Collections.Immutable.ImmutableArray{``0},System.Func{``0,``0,``0})"/>
/// gives for an empty array, is not passed on: <c>Aggregate</c> throws there, as before.
/// </para>
/// <para>
/// Every other operator, such as <c>Count</c> or <c>Contains</c>, still boxes. For the length use
/// <see cref="P:FSharp.Collections.Immutable.EquatableArray`1.Length"/>, and for the equality of two arrays use
/// <see cref="M:FSharp.Collections.Immutable.EquatableArray`1.Equals(FSharp.Collections.Immutable.EquatableArray{`0})"/>.
/// </para>
/// </summary>
/// <example>
/// <code lang="csharp">
/// using System.Linq;
/// using FSharp.Collections.Immutable;
///
/// EquatableArray&lt;int&gt; numbers = [3, 1, 4, 1, 5];
/// var first = numbers.First ();                      // no boxing
/// var big = numbers.Where (number => number > 2);    // no boxing
/// </code>
/// </example>
[<Extension; AbstractClass; Sealed>]
type EquatableArrayLinqExtensions =

    /// <summary>Applies an accumulator function over the array.</summary>
    /// <param name="array">The array.</param>
    /// <param name="func">The accumulator function. It gets the result so far and the next element.</param>
    /// <returns>The final result. For an array with one element, that element.</returns>
    /// <exception cref="T:System.InvalidOperationException">The array is empty.</exception>
    /// <exception cref="T:System.ArgumentNullException"><paramref name="func"/> is <see langword="null"/>.</exception>
    [<Extension>]
    static member Aggregate : array : EquatableArray<'T> * func : Func<'T, 'T, 'T> -> 'T

    /// <summary>Applies an accumulator function over the array, from a seed value.</summary>
    /// <param name="array">The array.</param>
    /// <param name="seed">The first value of the accumulator.</param>
    /// <param name="func">The accumulator function. It gets the result so far and the next element.</param>
    /// <returns>The final value of the accumulator. For an empty array, the seed.</returns>
    /// <exception cref="T:System.ArgumentNullException"><paramref name="func"/> is <see langword="null"/>.</exception>
    [<Extension>]
    static member Aggregate :
        array : EquatableArray<'T> * seed : 'Accumulate * func : Func<'Accumulate, 'T, 'Accumulate> -> 'Accumulate

    /// <summary>
    /// Applies an accumulator function over the array, from a seed value, and converts the final value with a result
    /// selector.
    /// </summary>
    /// <param name="array">The array.</param>
    /// <param name="seed">The first value of the accumulator.</param>
    /// <param name="func">The accumulator function. It gets the result so far and the next element.</param>
    /// <param name="resultSelector">The function that converts the final value of the accumulator.</param>
    /// <returns>The converted final value of the accumulator.</returns>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="func"/> or <paramref name="resultSelector"/> is <see langword="null"/>.
    /// </exception>
    [<Extension>]
    static member Aggregate :
        array : EquatableArray<'T> *
        seed : 'Accumulate *
        func : Func<'Accumulate, 'T, 'Accumulate> *
        resultSelector : Func<'Accumulate, 'Result> ->
            'Result

    /// <summary>Returns <see langword="true"/> when every element satisfies a condition.</summary>
    /// <param name="array">The array.</param>
    /// <param name="predicate">The condition.</param>
    /// <returns><see langword="true"/> when every element satisfies the condition, or when the array is empty.</returns>
    /// <exception cref="T:System.ArgumentNullException"><paramref name="predicate"/> is <see langword="null"/>.</exception>
    [<Extension>]
    static member All : array : EquatableArray<'T> * predicate : Func<'T, bool> -> bool

    /// <summary>Returns <see langword="true"/> when the array has an element.</summary>
    /// <param name="array">The array.</param>
    /// <returns><see langword="true"/> when the array is not empty.</returns>
    [<Extension>]
    static member Any : array : EquatableArray<'T> -> bool

    /// <summary>Returns <see langword="true"/> when an element satisfies a condition.</summary>
    /// <param name="array">The array.</param>
    /// <param name="predicate">The condition.</param>
    /// <returns><see langword="true"/> when at least one element satisfies the condition.</returns>
    /// <exception cref="T:System.ArgumentNullException"><paramref name="predicate"/> is <see langword="null"/>.</exception>
    [<Extension>]
    static member Any : array : EquatableArray<'T> * predicate : Func<'T, bool> -> bool

    /// <summary>Returns the element at an index.</summary>
    /// <param name="array">The array.</param>
    /// <param name="index">The zero-based index.</param>
    /// <returns>The element at the index.</returns>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    /// <paramref name="index"/> is less than zero, or it is not less than the length.
    /// </exception>
    [<Extension>]
    static member ElementAt : array : EquatableArray<'T> * index : int -> 'T

    /// <summary>Returns the element at an index, or the default value when the index is out of range.</summary>
    /// <param name="array">The array.</param>
    /// <param name="index">The zero-based index.</param>
    /// <returns>The element at the index, or the default value of the element type.</returns>
    [<Extension>]
    static member ElementAtOrDefault : array : EquatableArray<'T> * index : int -> 'T

    /// <summary>Returns the first element.</summary>
    /// <param name="array">The array.</param>
    /// <returns>The first element.</returns>
    /// <exception cref="T:System.InvalidOperationException">The array is empty.</exception>
    [<Extension>]
    static member First : array : EquatableArray<'T> -> 'T

    /// <summary>Returns the first element that satisfies a condition.</summary>
    /// <param name="array">The array.</param>
    /// <param name="predicate">The condition.</param>
    /// <returns>The first element that satisfies the condition.</returns>
    /// <exception cref="T:System.InvalidOperationException">No element satisfies the condition.</exception>
    /// <exception cref="T:System.ArgumentNullException"><paramref name="predicate"/> is <see langword="null"/>.</exception>
    [<Extension>]
    static member First : array : EquatableArray<'T> * predicate : Func<'T, bool> -> 'T

    /// <summary>Returns the first element, or the default value when the array is empty.</summary>
    /// <param name="array">The array.</param>
    /// <returns>The first element, or the default value of the element type.</returns>
    [<Extension>]
    static member FirstOrDefault : array : EquatableArray<'T> -> 'T

    /// <summary>
    /// Returns the first element that satisfies a condition, or the default value when no element does.
    /// </summary>
    /// <param name="array">The array.</param>
    /// <param name="predicate">The condition.</param>
    /// <returns>The first element that satisfies the condition, or the default value of the element type.</returns>
    /// <exception cref="T:System.ArgumentNullException"><paramref name="predicate"/> is <see langword="null"/>.</exception>
    [<Extension>]
    static member FirstOrDefault : array : EquatableArray<'T> * predicate : Func<'T, bool> -> 'T

    /// <summary>Returns the last element.</summary>
    /// <param name="array">The array.</param>
    /// <returns>The last element.</returns>
    /// <exception cref="T:System.InvalidOperationException">The array is empty.</exception>
    [<Extension>]
    static member Last : array : EquatableArray<'T> -> 'T

    /// <summary>Returns the last element that satisfies a condition.</summary>
    /// <param name="array">The array.</param>
    /// <param name="predicate">The condition.</param>
    /// <returns>The last element that satisfies the condition.</returns>
    /// <exception cref="T:System.InvalidOperationException">No element satisfies the condition.</exception>
    /// <exception cref="T:System.ArgumentNullException"><paramref name="predicate"/> is <see langword="null"/>.</exception>
    [<Extension>]
    static member Last : array : EquatableArray<'T> * predicate : Func<'T, bool> -> 'T

    /// <summary>Returns the last element, or the default value when the array is empty.</summary>
    /// <param name="array">The array.</param>
    /// <returns>The last element, or the default value of the element type.</returns>
    [<Extension>]
    static member LastOrDefault : array : EquatableArray<'T> -> 'T

    /// <summary>
    /// Returns the last element that satisfies a condition, or the default value when no element does.
    /// </summary>
    /// <param name="array">The array.</param>
    /// <param name="predicate">The condition.</param>
    /// <returns>The last element that satisfies the condition, or the default value of the element type.</returns>
    /// <exception cref="T:System.ArgumentNullException"><paramref name="predicate"/> is <see langword="null"/>.</exception>
    [<Extension>]
    static member LastOrDefault : array : EquatableArray<'T> * predicate : Func<'T, bool> -> 'T

    /// <summary>Projects each element into a new form.</summary>
    /// <param name="array">The array.</param>
    /// <param name="selector">The function that converts an element.</param>
    /// <returns>The converted elements, in order. The sequence is lazy: the function runs while it is enumerated.</returns>
    /// <exception cref="T:System.ArgumentNullException"><paramref name="selector"/> is <see langword="null"/>.</exception>
    [<Extension>]
    static member Select : array : EquatableArray<'T> * selector : Func<'T, 'Result> -> IEnumerable<'Result>

    /// <summary>
    /// Projects each element into a sequence, and flattens the sequences into one with a result selector.
    /// </summary>
    /// <param name="array">The array.</param>
    /// <param name="collectionSelector">The function that converts an element into a sequence.</param>
    /// <param name="resultSelector">The function that converts an element and an element of its sequence.</param>
    /// <returns>The results for every element of every sequence, in order.</returns>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="collectionSelector"/> or <paramref name="resultSelector"/> is <see langword="null"/>.
    /// </exception>
    [<Extension>]
    static member SelectMany :
        array : EquatableArray<'T> *
        collectionSelector : Func<'T, IEnumerable<'Collection>> *
        resultSelector : Func<'T, 'Collection, 'Result> ->
            IEnumerable<'Result>

    /// <summary>Returns the only element.</summary>
    /// <param name="array">The array.</param>
    /// <returns>The only element.</returns>
    /// <exception cref="T:System.InvalidOperationException">The array is empty, or it has more than one element.</exception>
    [<Extension>]
    static member Single : array : EquatableArray<'T> -> 'T

    /// <summary>Returns the only element that satisfies a condition.</summary>
    /// <param name="array">The array.</param>
    /// <param name="predicate">The condition.</param>
    /// <returns>The only element that satisfies the condition.</returns>
    /// <exception cref="T:System.InvalidOperationException">
    /// No element satisfies the condition, or more than one does.
    /// </exception>
    /// <exception cref="T:System.ArgumentNullException"><paramref name="predicate"/> is <see langword="null"/>.</exception>
    [<Extension>]
    static member Single : array : EquatableArray<'T> * predicate : Func<'T, bool> -> 'T

    /// <summary>Returns the only element, or the default value when the array is empty.</summary>
    /// <param name="array">The array.</param>
    /// <returns>The only element, or the default value of the element type.</returns>
    /// <exception cref="T:System.InvalidOperationException">The array has more than one element.</exception>
    [<Extension>]
    static member SingleOrDefault : array : EquatableArray<'T> -> 'T

    /// <summary>
    /// Returns the only element that satisfies a condition, or the default value when no element does.
    /// </summary>
    /// <param name="array">The array.</param>
    /// <param name="predicate">The condition.</param>
    /// <returns>The only element that satisfies the condition, or the default value of the element type.</returns>
    /// <exception cref="T:System.InvalidOperationException">More than one element satisfies the condition.</exception>
    /// <exception cref="T:System.ArgumentNullException"><paramref name="predicate"/> is <see langword="null"/>.</exception>
    [<Extension>]
    static member SingleOrDefault : array : EquatableArray<'T> * predicate : Func<'T, bool> -> 'T

    /// <summary>Copies the elements into a new array.</summary>
    /// <param name="array">The array.</param>
    /// <returns>A new array with the elements, in order.</returns>
    [<Extension>]
    static member ToArray : array : EquatableArray<'T> -> 'T array

    /// <summary>Creates a dictionary from the elements with a key selector.</summary>
    /// <param name="array">The array.</param>
    /// <param name="keySelector">The function that gives the key of an element.</param>
    /// <returns>A dictionary that holds the elements under their keys.</returns>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="keySelector"/> is <see langword="null"/>, or it gives <see langword="null"/> for an element.
    /// </exception>
    /// <exception cref="T:System.ArgumentException">Two elements have the same key.</exception>
    [<Extension>]
    static member ToDictionary :
        array : EquatableArray<'T> * keySelector : Func<'T, 'Key> -> Dictionary<'Key, 'T> when 'Key : not null

    /// <summary>Creates a dictionary from the elements with a key selector and an element selector.</summary>
    /// <param name="array">The array.</param>
    /// <param name="keySelector">The function that gives the key of an element.</param>
    /// <param name="elementSelector">The function that gives the value of an element.</param>
    /// <returns>A dictionary that holds the values under their keys.</returns>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="keySelector"/> or <paramref name="elementSelector"/> is <see langword="null"/>, or
    /// <paramref name="keySelector"/> gives <see langword="null"/> for an element.
    /// </exception>
    /// <exception cref="T:System.ArgumentException">Two elements have the same key.</exception>
    [<Extension>]
    static member ToDictionary :
        array : EquatableArray<'T> * keySelector : Func<'T, 'Key> * elementSelector : Func<'T, 'Element> ->
            Dictionary<'Key, 'Element>
            when 'Key : not null

    /// <summary>Creates a dictionary from the elements with a key selector and a key comparer.</summary>
    /// <param name="array">The array.</param>
    /// <param name="keySelector">The function that gives the key of an element.</param>
    /// <param name="comparer">The comparer of the keys, or <see langword="null"/> for the default comparer.</param>
    /// <returns>A dictionary that holds the elements under their keys.</returns>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="keySelector"/> is <see langword="null"/>, or it gives <see langword="null"/> for an element.
    /// </exception>
    /// <exception cref="T:System.ArgumentException">Two elements have equal keys.</exception>
    [<Extension>]
    static member ToDictionary :
        array : EquatableArray<'T> * keySelector : Func<'T, 'Key> * comparer : IEqualityComparer<'Key> | null ->
            Dictionary<'Key, 'T>
            when 'Key : not null

    /// <summary>
    /// Creates a dictionary from the elements with a key selector, an element selector and a key comparer.
    /// </summary>
    /// <param name="array">The array.</param>
    /// <param name="keySelector">The function that gives the key of an element.</param>
    /// <param name="elementSelector">The function that gives the value of an element.</param>
    /// <param name="comparer">The comparer of the keys, or <see langword="null"/> for the default comparer.</param>
    /// <returns>A dictionary that holds the values under their keys.</returns>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="keySelector"/> or <paramref name="elementSelector"/> is <see langword="null"/>, or
    /// <paramref name="keySelector"/> gives <see langword="null"/> for an element.
    /// </exception>
    /// <exception cref="T:System.ArgumentException">Two elements have equal keys.</exception>
    [<Extension>]
    static member ToDictionary :
        array : EquatableArray<'T> *
        keySelector : Func<'T, 'Key> *
        elementSelector : Func<'T, 'Element> *
        comparer : IEqualityComparer<'Key> | null ->
            Dictionary<'Key, 'Element>
            when 'Key : not null

    /// <summary>Keeps the elements that satisfy a condition.</summary>
    /// <param name="array">The array.</param>
    /// <param name="predicate">The condition.</param>
    /// <returns>The elements that satisfy the condition, in order. The sequence is lazy.</returns>
    /// <exception cref="T:System.ArgumentNullException"><paramref name="predicate"/> is <see langword="null"/>.</exception>
    [<Extension>]
    static member Where : array : EquatableArray<'T> * predicate : Func<'T, bool> -> IEnumerable<'T>
