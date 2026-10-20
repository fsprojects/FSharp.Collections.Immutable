// The TestCategoryBaseAttribute descendants that categorize the C# tests, sorted by type name. They give the same
// categories as the attributes of the same names in the F# test project.
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FSharp.Collections.Immutable.CSharp.Tests;

/// <summary>Categorizes a test of the order of values.</summary>
public sealed class ComparisonTestCategoryAttribute : TestCategoryBaseAttribute
{
    /// <inheritdoc />
    public override IList<string> TestCategories { get; } = ["Comparison"];
}

/// <summary>Categorizes a test of equality and hash codes.</summary>
public sealed class EqualityTestCategoryAttribute : TestCategoryBaseAttribute
{
    /// <inheritdoc />
    public override IList<string> TestCategories { get; } = ["Equality"];
}

/// <summary>Categorizes a test of <see cref="EquatableArray{T}"/>.</summary>
public sealed class EquatableArrayTestCategoryAttribute : TestCategoryBaseAttribute
{
    /// <inheritdoc />
    public override IList<string> TestCategories { get; } = ["EquatableArray"];
}

/// <summary>Categorizes a test of the members that .NET interfaces, conversions, spans and C# use.</summary>
public sealed class InteropTestCategoryAttribute : TestCategoryBaseAttribute
{
    /// <inheritdoc />
    public override IList<string> TestCategories { get; } = ["Interop"];
}

/// <summary>Categorizes a test of the conversion to and from JSON.</summary>
public sealed class JsonTestCategoryAttribute : TestCategoryBaseAttribute
{
    /// <inheritdoc />
    public override IList<string> TestCategories { get; } = ["JSON"];
}
