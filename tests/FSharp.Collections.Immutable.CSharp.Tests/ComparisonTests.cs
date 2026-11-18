using System.Globalization;
using FSharp.Collections.Immutable.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FSharp.Collections.Immutable.CSharp.Tests;

/// <summary>Comparison as C# code sees it: sorted collections and the default comparer.</summary>
[TestClass, EquatableArrayTestCategory, ComparisonTestCategory]
public sealed class ComparisonTests
{
    [TestMethod]
    public void A_SortedSet_orders_by_length_then_by_elements()
    {
        EquatableArray<int>[] unsorted = [[3], [1, 2], [1]];
        var sorted = new SortedSet<EquatableArray<int>>(unsorted);
        EquatableArray<int>[] expected = [[1], [3], [1, 2]];
        CollectionAssert.AreEqual(expected, sorted.ToArray(), "a SortedSet must order by length first, then by elements");
    }

    [TestMethod]
    [DataRow("tr-TR")]
    [DataRow("en-US")]
    [DataRow("de-DE")]
    public void Strings_compare_ordinally_in_every_culture(string culture)
    {
        var previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo(culture);
            EquatableArray<string> lower = ["a"];
            EquatableArray<string> upper = ["B"];
            EquatableArray<string> dotted = ["i"];
            EquatableArray<string> dotless = ["ı"];
            var comparer = Comparer<EquatableArray<string>>.Default;
            Assert.IsGreaterThan(0, comparer.Compare(lower, upper), "'a' must come after 'B' ordinally, not by culture");
            Assert.IsLessThan(0, comparer.Compare(dotted, dotless), "'i' must come before 'ı' ordinally, not by culture");
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }
}
