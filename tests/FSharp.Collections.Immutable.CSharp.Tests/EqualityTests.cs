using FSharp.Collections.Immutable.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FSharp.Collections.Immutable.CSharp.Tests;

/// <summary>Equality as C# code sees it: the operators, the default collections and record structs.</summary>
[TestClass, EquatableArrayTestCategory, EqualityTestCategory]
public sealed class EqualityTests
{
    [TestMethod]
    public void The_operators_compare_the_elements()
    {
        EquatableArray<int> left = [1, 2, 3];
        var right = EquatableArrayModule.OfArray([1, 2, 3]);
        EquatableArray<int> different = [3, 2, 1];

        // Each operator result goes into a variable, so that the test reads the operator, not an assertion of it
        var equal = left == right;
        var notEqual = left != right;
        var equalToDifferent = left == different;
        Assert.IsTrue(equal, "== must be true for equal elements in the same order");
        Assert.IsFalse(notEqual, "!= must be false for equal elements in the same order");
        Assert.IsFalse(equalToDifferent, "== must be false for elements in another order");
        Assert.AreEqual(left.GetHashCode(), right.GetHashCode(), "equal arrays must have equal hash codes");
    }

    [TestMethod]
    public void The_default_value_equals_the_empty_array()
    {
        EquatableArray<int> empty = [];
        var equal = default(EquatableArray<int>) == empty;
        Assert.IsTrue(equal, "the default value must equal []");
    }

    [TestMethod]
    public void Default_collections_find_equal_arrays()
    {
        EquatableArray<int> key = [1, 2];
        var equal = EquatableArrayModule.OfArray([1, 2]);
        var set = new HashSet<EquatableArray<int>> { key };
        Assert.Contains(equal, set, "a HashSet must find an equal array");
        var dictionary = new Dictionary<EquatableArray<int>, string> { [key] = "found" };
        Assert.AreEqual("found", dictionary[equal], "a Dictionary must find an equal key");
    }

    [TestMethod]
    public void Record_structs_compare_the_elements() =>
        Assert.AreEqual(
            new Holder("n", [1, 2]),
            new Holder("n", EquatableArrayModule.OfArray([1, 2])),
            "a record struct must compare its array member by elements");

    /// <summary>A record struct that holds an array, as C# models do.</summary>
    public readonly record struct Holder(string Name, EquatableArray<int> Items);
}
