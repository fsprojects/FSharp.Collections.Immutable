using System.Runtime.InteropServices;
using FSharp.Collections.Immutable.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FSharp.Collections.Immutable.CSharp.Tests;

/// <summary>C# indices and ranges, which use <c>Length</c>, the indexer and <c>Slice</c>.</summary>
[TestClass, EquatableArrayTestCategory, InteropTestCategory]
public sealed class IndexAndRangeTests
{
    private static readonly EquatableArray<int> Numbers = [1, 2, 3, 4];

    [TestMethod]
    public void An_index_from_the_end_reads_the_element() => Assert.AreEqual(4, Numbers[^1], "^1 must read the last element");

    [TestMethod]
    public void A_range_gives_the_selected_elements()
    {
        EquatableArray<int> inner = [2, 3];
        Assert.AreEqual(inner, Numbers[1..^1], "1..^1 must give the inner elements");
        Assert.IsTrue(Numbers[2..2].IsEmpty, "an empty range must give the empty array");
    }

    [TestMethod]
    public void A_range_of_all_elements_shares_the_array()
    {
        var underlying = ImmutableCollectionsMarshal.AsArray(Numbers.Items);
        Assert.AreSame(underlying, ImmutableCollectionsMarshal.AsArray(Numbers[..].Items), "a range of all elements must not copy");
        Assert.AreNotSame(underlying, ImmutableCollectionsMarshal.AsArray(Numbers[1..].Items), "a smaller range must copy");
    }

    [TestMethod]
    public void A_reversed_range_throws() =>
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => _ = Numbers[2..1], "a range that ends before its start must throw");
}
