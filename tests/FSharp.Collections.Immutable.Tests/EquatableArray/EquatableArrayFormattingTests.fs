namespace FSharp.Collections.Immutable.Tests

open System
open Microsoft.VisualStudio.TestTools.UnitTesting
open FSharp.Collections.Immutable

[<TestClass; EquatableArrayTestCategory; FormattingTestCategory>]
type EquatableArrayFormattingTests () =

    /// Asserts that %A and ToString show the array as %A shows the F# array with the same elements.
    static let assertShownAsArray (items : 'T array) (message : string) =
        let expected = $"%A{items}"
        let array = EquatableArray.ofArray items
        Assert.AreEqual (expected, $"%A{array}", $"%%A: %s{message}")
        Assert.AreEqual (expected, array.ToString (), $"ToString: %s{message}")

    [<TestMethod>]
    member _.``%A and ToString show the elements in F# array notation`` () =
        assertShownAsArray [| 1; 2; 3 |] "numbers must be shown as [|1; 2; 3|]"
        assertShownAsArray Array.empty<int> "the empty array must be shown as [||]"
        assertShownAsArray [| "a"; "b" |] "strings must be shown with quotes"
        assertShownAsArray [| Leaf 1; Leaf 2 |] "union cases must be shown as %A shows them"
        Assert.AreEqual ("[|1; 2|]", (ea [ 1; 2 ]).ToString(), "ToString must give the text that the first consumer gives")

    [<TestMethod>]
    member _.``a nested array shows its children`` () =
        let nested = ea [ ea [ 1; 2 ]; EquatableArray.empty ]
        Assert.AreEqual ("[|[|1; 2|]; [||]|]", $"%A{nested}", "a nested array must show its elements, not '...'")

    [<TestMethod>]
    member _.``a cycle through a mutable record stops with ... instead of overflowing the stack`` () =
        let node = { Children = EquatableArray.empty }
        node.Children <- EquatableArray.singleton node
        // Object.ToString is annotated as nullable; this override never returns null
        Assert.Contains ("...", nonNull (node.Children.ToString ()), "ToString must stop the cycle with '...'")
        Assert.Contains ("...", $"%A{node}", "%A must stop the cycle with '...'")

    [<TestMethod>]
    member _.``a tree deeper than the depth limit ends in ... and a shallower tree is shown completely`` () =
        let deepen (depth : int) =
            let mutable tree = Leaf 0

            for _ in 1..depth do
                tree <- Branch (EquatableArray.singleton tree)

            tree

        let deep = $"%A{deepen 10_000}"
        Assert.Contains ("...", deep, "a tree deeper than the limit must end in '...'")
        Assert.DoesNotContain ("Leaf 0", deep, "the leaf below the limit must not be shown")
        Assert.Contains ("Leaf 0", $"%A{deepen 50}", "a tree inside the limit must be shown down to its leaf")
        Assert.AreEqual ("[|[|1|]|]", (ea [ ea [ 1 ] ]).ToString(), "a display after a deep one must start at depth 0 again")

    [<TestMethod>]
    member _.``a record field shows the elements`` () =
        let document = { Name = "x"; Tags = ea [ "a"; "b" ] }
        Assert.Contains ("""Tags = [|"a"; "b"|]""", $"%A{document}", "the field must show the elements of the array")

    [<TestMethod>]
    member _.``an array longer than 100 elements is shown as %A shows long F# arrays`` () =
        let items = Array.init 150 id
        assertShownAsArray items "the display must stop after 100 elements, as for an F# array"
        StringAssert.EndsWith (
            (EquatableArray.ofArray items).ToString(),
            "...|]",
            StringComparison.Ordinal,
            "the display must end with '...'"
        )
