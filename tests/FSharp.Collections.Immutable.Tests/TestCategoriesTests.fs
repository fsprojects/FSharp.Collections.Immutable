namespace FSharp.Collections.Immutable.Tests

open System
open System.Reflection
open Microsoft.VisualStudio.TestTools.UnitTesting

/// Enforces the category rules of `.github/instructions/test-categories.instructions.md` and of ADR 0001, D8.
[<TestClass; ConventionTestCategory>]
type TestCategoriesTests () =

    static let suffix = "TestCategoryAttribute"

    static let assembly = typeof<TestCategoriesTests>.Assembly

    static let categoryAttributeTypes =
        assembly.GetTypes ()
        |> Array.filter (fun candidate ->
            candidate.IsSubclassOf typeof<TestCategoryBaseAttribute>
            && not candidate.IsAbstract
        )

    static let testClasses =
        assembly.GetTypes ()
        |> Array.filter (fun candidate -> candidate.IsDefined (typeof<TestClassAttribute>, false))

    /// A category attribute of this project, as opposed to the `TestCategory` attribute of MSTest.
    static let isProjectCategory (attribute : TestCategoryBaseAttribute) = attribute.GetType().Assembly = assembly

    [<TestMethod>]
    member _.``Every category attribute gives one category named after its type`` () =
        Assert.IsNotEmpty (categoryAttributeTypes, "TestCategories.fs must define category attributes")

        for attributeType in categoryAttributeTypes do
            let name = attributeType.Name
            StringAssert.EndsWith (
                name,
                suffix,
                StringComparison.Ordinal,
                "A category attribute must end with the TestCategoryAttribute suffix"
            )

            let attribute = nonNull (Activator.CreateInstance attributeType) :?> TestCategoryBaseAttribute
            let category =
                Assert.ContainsSingle (attribute.TestCategories, $"%s{name} must give exactly one category")
            let expected = name.Substring (0, name.Length - suffix.Length)

            Assert.AreEqual<string>(
                expected.ToUpperInvariant (),
                category.Replace(" ", "").ToUpperInvariant(),
                $"The category of %s{name} must be its type name without the suffix, written as separate words"
            )

            Assert.IsTrue (Char.IsUpper category[0], $"The category of %s{name} must start with a capital letter")

    [<TestMethod>]
    member _.``Category attributes are sorted by type name`` () =
        let names = categoryAttributeTypes |> Array.map _.Name
        let sorted =
            names
            |> Array.sortWith (fun left right -> String.CompareOrdinal (left, right))
        CollectionAssert.AreEqual (sorted, names, "TestCategories.fs must keep its attributes sorted by type name")

    [<TestMethod>]
    member _.``Every test class has a category attribute of this project`` () =
        Assert.IsNotEmpty (testClasses, "The assembly must contain test classes")

        for testClass in testClasses do
            let categories =
                testClass.GetCustomAttributes<TestCategoryBaseAttribute>(true)
                |> Seq.filter isProjectCategory

            Assert.IsNotEmpty (categories, $"%s{testClass.Name} must carry the category attribute of what it examines")

    [<TestMethod>]
    member _.``No test class has a category written as a string`` () =
        for testClass in testClasses do
            Assert.IsFalse (
                testClass.IsDefined (typeof<TestCategoryAttribute>, true),
                $"%s{testClass.Name} must take its categories from the attributes of TestCategories.fs, not from TestCategory"
            )

    [<TestMethod>]
    member _.``Test methods carry no categories`` () =
        // The categories belong to the test classes, so every test of a class shares them
        for testClass in testClasses do
            let testMethods =
                testClass.GetMethods (BindingFlags.Public ||| BindingFlags.Instance)
                |> Array.filter (fun testMethod -> testMethod.IsDefined (typeof<TestMethodAttribute>, true))

            for testMethod in testMethods do
                let categories = testMethod.GetCustomAttributes<TestCategoryBaseAttribute>(true)
                Assert.IsEmpty (categories, $"%s{testClass.Name}.%s{testMethod.Name} must not carry a category")
