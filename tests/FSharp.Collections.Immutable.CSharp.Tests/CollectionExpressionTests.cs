using FSharp.Collections.Immutable.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FSharp.Collections.Immutable.CSharp.Tests;

/// <summary>C# collection expressions, which call the factory that <c>CollectionBuilderAttribute</c> names.</summary>
[TestClass, EquatableArrayTestCategory, InteropTestCategory]
public sealed class CollectionExpressionTests
{
    [TestMethod]
    public void A_collection_expression_creates_the_array()
    {
        EquatableArray<int> array = [1, 2, 3];
        Assert.AreEqual(EquatableArrayModule.OfArray([1, 2, 3]), array, "a collection expression must give the elements in order");
    }

    [TestMethod]
    public void An_empty_collection_expression_equals_the_default_value()
    {
        EquatableArray<int> empty = [];
        Assert.IsTrue(empty.IsEmpty, "[] must give the empty array");
        Assert.AreEqual(default, empty, "[] must equal the default value");
    }

    [TestMethod]
    public void Spreads_keep_the_order_of_the_elements()
    {
        EquatableArray<int> first = [1, 2];
        int[] second = [4];
        EquatableArray<int> spread = [.. first, 3, .. second];
        Assert.AreEqual(EquatableArrayModule.OfArray([1, 2, 3, 4]), spread, "spreads must keep the order of the elements");
    }

    [TestMethod]
    public void Nested_collection_expressions_compare_by_elements_at_both_levels()
    {
        EquatableArray<EquatableArray<int>> nested = [[1], [2, 3]];
        EquatableArray<EquatableArray<int>> same = [[1], [2, 3]];
        Assert.HasCount(2, nested, "the outer array must hold two arrays");
        Assert.AreEqual(same, nested, "separately built nested arrays must be equal");
        // The expected values are plain arrays, not made by the collection builder under test: a builder that drops an
        // element gives the same wrong value on both sides of the equality above, which would pass
        CollectionAssert.AreEqual(new[] { 1 }, nested[0].ToArray(), "the first inner array must hold 1");
        CollectionAssert.AreEqual(new[] { 2, 3 }, nested[1].ToArray(), "the second inner array must hold 2 and 3");
    }
}
