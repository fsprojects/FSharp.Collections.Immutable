using System.Collections.Immutable;
using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.FSharp.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FSharp.Collections.Immutable.CSharp.Tests;

/// <summary>Conversions, enumeration and the module functions, as C# code uses them.</summary>
[TestClass, EquatableArrayTestCategory, InteropTestCategory]
public sealed class ConversionTests
{
    [TestMethod]
    public void ImplicitConversionsKeepTheArray()
    {
        var items = ImmutableArray.Create(1, 2, 3);
        EquatableArray<int> array = items;
        ImmutableArray<int> back = array;
        Assert.AreSame(ImmutableCollectionsMarshal.AsArray(items), ImmutableCollectionsMarshal.AsArray(array.Items), "the conversion to EquatableArray must not copy");
        Assert.AreSame(ImmutableCollectionsMarshal.AsArray(items), ImmutableCollectionsMarshal.AsArray(back), "the conversion to ImmutableArray must not copy");
    }

    [TestMethod]
    public void ImplicitConversionOfTheDefaultValueGivesAnEmptyImmutableArray()
    {
        ImmutableArray<int> converted = default(EquatableArray<int>);
        Assert.IsFalse(converted.IsDefault, "the default value must become an empty, not a default, ImmutableArray");
    }

    [TestMethod]
    public void ForeachAndLinqReadTheElements()
    {
        EquatableArray<int> array = [1, 2, 3];
        var sum = 0;
        foreach (var item in array)
        {
            sum += item;
        }

        Assert.AreEqual(6, sum, "foreach must read every element");
        Assert.AreEqual(12, array.Select(item => item * 2).Sum(), "LINQ must read every element");
        Assert.AreEqual(2, array[1], "the indexer must read the element");
    }

    [TestMethod]
    public void ModuleFunctionsHaveCSharpNames()
    {
        var array = EquatableArrayModule.OfSeq(new List<int> { 1, 2 });
        Assert.AreEqual(2, EquatableArrayModule.Length(array), "Length must count the elements");
        var mapped = EquatableArrayModule.Map(FuncConvert.FromFunc<int, string>(item => item.ToString(CultureInfo.InvariantCulture)), array);
        EquatableArray<string> expected = ["1", "2"];
        Assert.AreEqual(expected, mapped, "Map must apply the function to each element");
        Assert.IsTrue(EquatableArrayModule.Empty<int>().IsEmpty, "Empty must give the empty array");
    }
}
