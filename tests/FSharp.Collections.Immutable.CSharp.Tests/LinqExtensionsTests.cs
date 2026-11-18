using System.Collections.Immutable;
using System.Linq.Expressions;
using FSharp.Collections.Immutable.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FSharp.Collections.Immutable.CSharp.Tests;

/// <summary>
/// The LINQ operators that take an <see cref="EquatableArray{T}"/> without boxing it: C# binds them instead of the
/// operators of <see cref="Enumerable"/>, and they must give the same results and throw the same exceptions.
/// </summary>
[TestClass, EquatableArrayTestCategory, InteropTestCategory]
public sealed class LinqExtensionsTests
{
    /// <summary>The arrays that the tests run on: none, one and several elements, with a duplicate.</summary>
    public static IEnumerable<int[]> Inputs => [[], [5], [3, 1, 4, 1, 5]];

    /// <summary>Each input with indices before, inside and beyond the array.</summary>
    public static IEnumerable<(int[] Items, int Index)> InputsAndIndices =>
        from items in Inputs from index in new[] { -1, 0, 2, 99 } select (items, index);

    /// <summary>The result of an operation as text, or the type of the exception that it throws.</summary>
    private static string Outcome<T>(Func<T> operation)
    {
        try
        {
            return operation() switch
            {
                Dictionary<int, int> dictionary => string.Join(",", dictionary.OrderBy(pair => pair.Key).Select(pair => $"{pair.Key}:{pair.Value}")),
                IEnumerable<int> sequence => $"[{string.Join(",", sequence)}]",
                var value => $"{value}",
            };
        }
        catch (Exception exception)
        {
            return $"throws {exception.GetType().Name}";
        }
    }

    /// <summary>Asserts that the extension method gives the result, or the exception, of the operator of Enumerable.</summary>
    private static void AssertSame<T>(Func<T> enumerable, Func<T> extension, string operation) =>
        Assert.AreEqual(
            Outcome(enumerable),
            Outcome(extension),
            $"{operation} must give the result, or throw the exception, of the operator of Enumerable");

    /// <summary>Asserts that the C# compiler binds the call to the extension class, which does not box the array.</summary>
    private static void AssertBound<T>(Expression<Func<T>> call, string operation)
    {
        var method = ((MethodCallExpression)call.Body).Method;
        Assert.AreEqual(
            typeof(EquatableArrayLinqExtensions),
            method.DeclaringType,
            $"{operation} must bind to the extension class, not to Enumerable, which boxes the array");
    }

    [TestMethod]
    public void Every_operator_binds_to_the_extension_class_and_not_to_Enumerable()
    {
        // Expression trees record the method that the compiler chose, so this fails when an overload falls back to the
        // operator of Enumerable. The collection expression cannot sit in the tree.
        EquatableArray<int> array = [3, 1, 4];

        AssertBound(() => array.Aggregate((left, right) => left + right), "Aggregate (func)");
        AssertBound(() => array.Aggregate(10, (sum, item) => sum + item), "Aggregate (seed, func)");
        AssertBound(() => array.Aggregate(10, (sum, item) => sum + item, sum => "=" + sum), "Aggregate (seed, func, result selector)");
        AssertBound(() => array.All(item => item > 0), "All");
        AssertBound(() => array.Any(), "Any ()");
        AssertBound(() => array.Any(item => item > 3), "Any (predicate)");
        AssertBound(() => array.ElementAt(1), "ElementAt");
        AssertBound(() => array.ElementAtOrDefault(1), "ElementAtOrDefault");
        AssertBound(() => array.First(), "First ()");
        AssertBound(() => array.First(item => item > 3), "First (predicate)");
        AssertBound(() => array.FirstOrDefault(), "FirstOrDefault ()");
        AssertBound(() => array.FirstOrDefault(item => item > 3), "FirstOrDefault (predicate)");
        AssertBound(() => array.Last(), "Last ()");
        AssertBound(() => array.Last(item => item > 3), "Last (predicate)");
        AssertBound(() => array.LastOrDefault(), "LastOrDefault ()");
        AssertBound(() => array.LastOrDefault(item => item > 3), "LastOrDefault (predicate)");
        AssertBound(() => array.Select(item => item * 2), "Select");
        AssertBound(() => array.SelectMany(item => new[] { item, -item }, (item, other) => item + other), "SelectMany");
        AssertBound(() => array.Single(), "Single ()");
        AssertBound(() => array.Single(item => item > 3), "Single (predicate)");
        AssertBound(() => array.SingleOrDefault(), "SingleOrDefault ()");
        AssertBound(() => array.SingleOrDefault(item => item > 3), "SingleOrDefault (predicate)");
        AssertBound(() => array.ToArray(), "ToArray");
        AssertBound(() => array.ToDictionary(item => item), "ToDictionary (key selector)");
        AssertBound(() => array.ToDictionary(item => item, item => item * 10), "ToDictionary (key selector, element selector)");
        AssertBound(() => array.ToDictionary(item => item, EqualityComparer<int>.Default), "ToDictionary (key selector, comparer)");
        AssertBound(() => array.ToDictionary(item => item, item => item * 10, EqualityComparer<int>.Default), "ToDictionary (key selector, element selector, comparer)");
        AssertBound(() => array.Where(item => item > 1), "Where");
    }

    [TestMethod]
    [DynamicData(nameof(Inputs))]
    public void Select_Where_and_SelectMany_give_the_sequences_of_Enumerable(int[] items)
    {
        var array = EquatableArrayModule.OfArray(items);
        IEnumerable<int> boxed = array;

        AssertSame(() => boxed.Select(item => item * 2), () => array.Select(item => item * 2), "Select");
        AssertSame(() => boxed.Where(item => item % 2 == 1), () => array.Where(item => item % 2 == 1), "Where");
        AssertSame(
            () => boxed.SelectMany(item => new[] { item, -item }, (item, other) => item + other),
            () => array.SelectMany(item => new[] { item, -item }, (item, other) => item + other),
            "SelectMany");
    }

    [TestMethod]
    [DynamicData(nameof(Inputs))]
    public void All_and_Any_give_the_results_of_Enumerable(int[] items)
    {
        var array = EquatableArrayModule.OfArray(items);
        IEnumerable<int> boxed = array;

        AssertSame(() => boxed.All(item => item > 0), () => array.All(item => item > 0), "All");
        AssertSame(() => boxed.Any(), () => array.Any(), "Any ()");
        AssertSame(() => boxed.Any(item => item > 3), () => array.Any(item => item > 3), "Any (predicate)");
    }

    [TestMethod]
    [DynamicData(nameof(InputsAndIndices))]
    public void ElementAt_gives_the_result_or_the_exception_of_Enumerable(int[] items, int index)
    {
        var array = EquatableArrayModule.OfArray(items);
        IEnumerable<int> boxed = array;

        AssertSame(() => boxed.ElementAt(index), () => array.ElementAt(index), "ElementAt");
        AssertSame(() => boxed.ElementAtOrDefault(index), () => array.ElementAtOrDefault(index), "ElementAtOrDefault");
    }

    [TestMethod]
    [DynamicData(nameof(Inputs))]
    public void First_and_Last_give_the_results_or_the_exceptions_of_Enumerable(int[] items)
    {
        var array = EquatableArrayModule.OfArray(items);
        IEnumerable<int> boxed = array;

        AssertSame(() => boxed.First(), () => array.First(), "First ()");
        AssertSame(() => boxed.First(item => item > 3), () => array.First(item => item > 3), "First (a match)");
        AssertSame(() => boxed.First(item => item > 99), () => array.First(item => item > 99), "First (no match)");
        AssertSame(() => boxed.FirstOrDefault(), () => array.FirstOrDefault(), "FirstOrDefault ()");
        AssertSame(() => boxed.FirstOrDefault(item => item > 3), () => array.FirstOrDefault(item => item > 3), "FirstOrDefault (a match)");
        AssertSame(() => boxed.FirstOrDefault(item => item > 99), () => array.FirstOrDefault(item => item > 99), "FirstOrDefault (no match)");
        AssertSame(() => boxed.Last(), () => array.Last(), "Last ()");
        AssertSame(() => boxed.Last(item => item < 5), () => array.Last(item => item < 5), "Last (a match)");
        AssertSame(() => boxed.Last(item => item > 99), () => array.Last(item => item > 99), "Last (no match)");
        AssertSame(() => boxed.LastOrDefault(), () => array.LastOrDefault(), "LastOrDefault ()");
        AssertSame(() => boxed.LastOrDefault(item => item < 5), () => array.LastOrDefault(item => item < 5), "LastOrDefault (a match)");
        AssertSame(() => boxed.LastOrDefault(item => item > 99), () => array.LastOrDefault(item => item > 99), "LastOrDefault (no match)");
    }

    [TestMethod]
    [DynamicData(nameof(Inputs))]
    public void Single_gives_the_result_or_the_exception_of_Enumerable(int[] items)
    {
        var array = EquatableArrayModule.OfArray(items);
        IEnumerable<int> boxed = array;

        AssertSame(() => boxed.Single(), () => array.Single(), "Single ()");
        AssertSame(() => boxed.Single(item => item == 4), () => array.Single(item => item == 4), "Single (one match)");
        AssertSame(() => boxed.Single(item => item == 1), () => array.Single(item => item == 1), "Single (two matches)");
        AssertSame(() => boxed.Single(item => item > 99), () => array.Single(item => item > 99), "Single (no match)");
        AssertSame(() => boxed.SingleOrDefault(), () => array.SingleOrDefault(), "SingleOrDefault ()");
        AssertSame(() => boxed.SingleOrDefault(item => item == 4), () => array.SingleOrDefault(item => item == 4), "SingleOrDefault (one match)");
        AssertSame(() => boxed.SingleOrDefault(item => item == 1), () => array.SingleOrDefault(item => item == 1), "SingleOrDefault (two matches)");
        AssertSame(() => boxed.SingleOrDefault(item => item > 99), () => array.SingleOrDefault(item => item > 99), "SingleOrDefault (no match)");
    }

    [TestMethod]
    [DynamicData(nameof(Inputs))]
    public void Aggregate_gives_the_result_or_the_exception_of_Enumerable(int[] items)
    {
        var array = EquatableArrayModule.OfArray(items);
        IEnumerable<int> boxed = array;

        AssertSame(() => boxed.Aggregate((left, right) => left + right), () => array.Aggregate((left, right) => left + right), "Aggregate (func)");
        AssertSame(() => boxed.Aggregate(10, (sum, item) => sum + item), () => array.Aggregate(10, (sum, item) => sum + item), "Aggregate (seed, func)");
        AssertSame(
            () => boxed.Aggregate(10, (sum, item) => sum + item, sum => $"={sum}"),
            () => array.Aggregate(10, (sum, item) => sum + item, sum => $"={sum}"),
            "Aggregate (seed, func, result selector)");
    }

    [TestMethod]
    public void Aggregate_of_an_empty_array_throws_where_ImmutableArray_gives_the_default_value()
    {
        // ImmutableArrayExtensions.Aggregate gives the default value for an empty array. The extension must not change
        // what existing code gets from Enumerable.Aggregate, which throws.
        Assert.AreEqual(0, ImmutableArray<int>.Empty.Aggregate((left, right) => left + right), "ImmutableArray gives the default value");
        Assert.ThrowsExactly<InvalidOperationException>(
            () => EquatableArrayModule.Empty<int>().Aggregate((left, right) => left + right),
            "an empty EquatableArray must throw, as Enumerable.Aggregate does");
    }

    [TestMethod]
    [DynamicData(nameof(Inputs))]
    public void ToArray_and_ToDictionary_give_the_results_or_the_exceptions_of_Enumerable(int[] items)
    {
        var array = EquatableArrayModule.OfArray(items);
        IEnumerable<int> boxed = array;

        AssertSame(() => boxed.ToArray(), () => array.ToArray(), "ToArray");
        AssertSame(() => boxed.ToDictionary(item => item), () => array.ToDictionary(item => item), "ToDictionary (key selector)");
        AssertSame(
            () => boxed.ToDictionary(item => item, item => item * 10),
            () => array.ToDictionary(item => item, item => item * 10),
            "ToDictionary (key selector, element selector)");
        AssertSame(
            () => boxed.ToDictionary(item => item, EqualityComparer<int>.Default),
            () => array.ToDictionary(item => item, EqualityComparer<int>.Default),
            "ToDictionary (key selector, comparer)");
        AssertSame(
            () => boxed.ToDictionary(item => item, item => item * 10, EqualityComparer<int>.Default),
            () => array.ToDictionary(item => item, item => item * 10, EqualityComparer<int>.Default),
            "ToDictionary (key selector, element selector, comparer)");
    }

    [TestMethod]
    public void ToArray_copies_the_elements()
    {
        EquatableArray<int> array = [1, 2, 3];
        var copy = array.ToArray();
        copy[0] = 9;
        Assert.AreEqual(1, array[0], "a change of the copy must not change the array");
    }

    [TestMethod]
    public void A_null_delegate_throws_ArgumentNullException()
    {
        EquatableArray<int> array = [1, 2, 3];

        Assert.ThrowsExactly<ArgumentNullException>(() => array.Select((Func<int, int>)null!), "Select must reject a null selector");
        Assert.ThrowsExactly<ArgumentNullException>(() => array.Where((Func<int, bool>)null!), "Where must reject a null predicate");
        Assert.ThrowsExactly<ArgumentNullException>(() => array.All((Func<int, bool>)null!), "All must reject a null predicate");
        Assert.ThrowsExactly<ArgumentNullException>(() => array.Any((Func<int, bool>)null!), "Any must reject a null predicate");
        Assert.ThrowsExactly<ArgumentNullException>(() => array.First((Func<int, bool>)null!), "First must reject a null predicate");
        Assert.ThrowsExactly<ArgumentNullException>(() => array.Aggregate((Func<int, int, int>)null!), "Aggregate must reject a null function");
        Assert.ThrowsExactly<ArgumentNullException>(() => array.ToDictionary((Func<int, int>)null!), "ToDictionary must reject a null key selector");
    }

#if !DEBUG
    // Release only: a Debug build does not optimize the code that these tests measure. The project turns tiered
    // compilation off in Release, so the code is fully optimized from its first call.

    /// <summary>Counts the bytes that an action allocates on this thread in 100 calls, after a first call.</summary>
    private static long AllocatedBytes(Action action)
    {
        action();
        var before = GC.GetAllocatedBytesForCurrentThread();

        for (var call = 0; call < 100; call++)
        {
            action();
        }

        return GC.GetAllocatedBytesForCurrentThread() - before;
    }

    [TestMethod]
    public void The_operators_allocate_nothing_where_Enumerable_boxes_the_array()
    {
        EquatableArray<int> array = [3, 1, 4, 1, 5];

        // The control: calling Enumerable on the array boxes it, and the counter sees that
        Assert.IsGreaterThan(0L, AllocatedBytes(() => _ = Enumerable.First<int>(array)), "Enumerable.First must box the array");

        Assert.AreEqual(0L, AllocatedBytes(() => _ = array.Any()), "Any () must not allocate");
        Assert.AreEqual(0L, AllocatedBytes(() => _ = array.Any(item => item > 3)), "Any (predicate) must not allocate");
        Assert.AreEqual(0L, AllocatedBytes(() => _ = array.All(item => item > 0)), "All must not allocate");
        Assert.AreEqual(0L, AllocatedBytes(() => _ = array.ElementAt(2)), "ElementAt must not allocate");
        Assert.AreEqual(0L, AllocatedBytes(() => _ = array.First()), "First () must not allocate");
        Assert.AreEqual(0L, AllocatedBytes(() => _ = array.First(item => item > 3)), "First (predicate) must not allocate");
        Assert.AreEqual(0L, AllocatedBytes(() => _ = array.FirstOrDefault()), "FirstOrDefault () must not allocate");
        Assert.AreEqual(0L, AllocatedBytes(() => _ = array.Last()), "Last () must not allocate");
        Assert.AreEqual(0L, AllocatedBytes(() => _ = array.LastOrDefault(item => item < 4)), "LastOrDefault (predicate) must not allocate");
        Assert.AreEqual(0L, AllocatedBytes(() => _ = array.Single(item => item == 4)), "Single (predicate) must not allocate");
        Assert.AreEqual(0L, AllocatedBytes(() => _ = array.Aggregate(10, (sum, item) => sum + item)), "Aggregate (seed, func) must not allocate");
    }
#endif
}
