using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FSharp.Collections.Immutable.CSharp.Tests;

/// <summary>C# indices and ranges, which use <c>Length</c>, the indexer and <c>Slice</c>.</summary>
[TestClass, EquatableArrayTestCategory, InteropTestCategory]
public sealed class IndexAndRangeTests
{
    private static readonly EquatableArray<int> Numbers = [1, 2, 3, 4];

    [TestMethod]
    public void IndexFromTheEndReadsTheElement() => Assert.AreEqual(4, Numbers[^1], "^1 must read the last element");

    [TestMethod]
    public void RangeGivesTheSelectedElements()
    {
        EquatableArray<int> inner = [2, 3];
        Assert.AreEqual(inner, Numbers[1..^1], "1..^1 must give the inner elements");
        Assert.IsTrue(Numbers[2..2].IsEmpty, "an empty range must give the empty array");
    }

    [TestMethod]
    public void RangeOfAllElementsSharesTheArray()
    {
        var underlying = ImmutableCollectionsMarshal.AsArray(Numbers.Items);
        Assert.AreSame(underlying, ImmutableCollectionsMarshal.AsArray(Numbers[..].Items), "a range of all elements must not copy");
        Assert.AreNotSame(underlying, ImmutableCollectionsMarshal.AsArray(Numbers[1..].Items), "a smaller range must copy");
    }

    [TestMethod]
    public void ReversedRangeThrows() =>
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => _ = Numbers[2..1], "a range that ends before its start must throw");
}
