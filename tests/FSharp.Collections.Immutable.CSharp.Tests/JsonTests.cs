using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FSharp.Collections.Immutable.CSharp.Tests;

/// <summary>A JSON round trip, as C# code does it.</summary>
[TestClass, EquatableArrayTestCategory, JsonTestCategory]
public sealed class JsonTests
{
    [TestMethod]
    public void RecordStructRoundTrips()
    {
        var holder = new Holder("n", [1, 2]);
        var json = JsonSerializer.Serialize(holder);
        Assert.AreEqual("""{"Name":"n","Items":[1,2]}""", json, "the array member must be written as a JSON array");
        Assert.AreEqual(holder, JsonSerializer.Deserialize<Holder>(json), "the record struct must be read back");
    }

    /// <summary>A record struct that holds an array, as C# models do.</summary>
    public readonly record struct Holder(string Name, EquatableArray<int> Items);
}
