using System.Collections.Immutable;
using System.Globalization;
using System.Runtime.InteropServices;
using FSharp.Collections.Immutable.Tests;
using Microsoft.FSharp.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FSharp.Collections.Immutable.CSharp.Tests;

/// <summary>Conversions, enumeration and the module functions, as C# code uses them.</summary>
[TestClass, EquatableArrayTestCategory, InteropTestCategory]
public sealed class ConversionTests
{
    [TestMethod]
    public void The_implicit_conversions_keep_the_array()
    {
        var items = ImmutableArray.Create(1, 2, 3);
        EquatableArray<int> array = items;
        ImmutableArray<int> back = array;
        Assert.AreSame(ImmutableCollectionsMarshal.AsArray(items), ImmutableCollectionsMarshal.AsArray(array.Items), "the conversion to EquatableArray must not copy");
        Assert.AreSame(ImmutableCollectionsMarshal.AsArray(items), ImmutableCollectionsMarshal.AsArray(back), "the conversion to ImmutableArray must not copy");
    }

    [TestMethod]
    public void The_implicit_conversion_of_the_default_value_gives_an_empty_ImmutableArray()
    {
        ImmutableArray<int> converted = default(EquatableArray<int>);
        Assert.IsFalse(converted.IsDefault, "the default value must become an empty, not a default, ImmutableArray");
    }

    [TestMethod]
    public void Foreach_and_the_indexer_read_the_elements()
    {
        EquatableArray<int> array = [1, 2, 3];
        var items = new List<int>();
        foreach (var item in array)
        {
            items.Add(item);
        }

        // The items themselves, in order: a sum cannot see a loop that skips one element and repeats another
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, items, "foreach must read every element, in order");
        Assert.AreEqual(2, array[1], "the indexer must read the element");
    }

    [TestMethod]
    public void Module_functions_have_CSharp_names()
    {
        var array = EquatableArrayModule.OfSeq(new List<int> { 1, 2 });
        Assert.AreEqual(2, EquatableArrayModule.Length(array), "Length must count the elements");
        var mapped = EquatableArrayModule.Map(FuncConvert.FromFunc<int, string>(item => item.ToString(CultureInfo.InvariantCulture)), array);
        EquatableArray<string> expected = ["1", "2"];
        Assert.AreEqual(expected, mapped, "Map must apply the function to each element");
        Assert.IsTrue(EquatableArrayModule.Empty<int>().IsEmpty, "Empty must give the empty array");
    }
}
