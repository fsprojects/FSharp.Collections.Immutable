namespace FSharp.Collections.Immutable.Tests

open System
open Microsoft.VisualStudio.TestTools.UnitTesting
open FSharp.Collections.Immutable

[<TestClass; EquatableArrayTestCategory; FormattingTestCategory>]
type EquatableArrayFormattingTests () =

    /// The limit of the tests for the length of a display that must stay bounded. The displays of the trees below take
    /// 20 to 40 KB; before the display limits, a shared tree of 20 levels took 21 million characters.
    static let boundedLength = 200_000

    /// The number of times that part occurs in text, compared ordinally.
    static let occurrences (text : string) (part : string) =
        (text.Split ([| part |], StringSplitOptions.None)).Length
        - 1

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
        Assert.AreEqual ("[|1; 2|]", (ea [| 1; 2 |]).ToString(), "ToString must give the text that the first consumer gives")

    [<TestMethod>]
    member _.``a nested array shows its children`` () =
        let nested = ea [| ea [| 1; 2 |]; EquatableArray.empty |]
        Assert.AreEqual ("[|[|1; 2|]; [||]|]", $"%A{nested}", "a nested array must show its elements, not '...'")

    [<TestMethod; Timeout 60000; DataRow 1; DataRow 2>]
    member _.``a cycle through a mutable record stops with ... instead of overflowing the stack`` (copies : int) =
        // A node that holds itself once recurses until the stack ends; one that holds itself twice also doubles the work
        // of each level
        let node = DisplayTrees.Cyclic copies
        let text = nonNull (node.Children.ToString ())
        Assert.Contains ("...", text, "ToString must stop the cycle with '...'")
        Assert.IsLessThan (boundedLength, text.Length, "the display of a cycle must have a bounded length")
        Assert.Contains ("...", $"%A{node}", "%A must stop the cycle with '...'")

    [<TestMethod; Timeout 60000; DataRow 20; DataRow 100; DataRow 1000>]
    member _.``a tree that holds the same child twice shows a bounded text`` (levels : int) =
        // The text of such a tree doubles with each level, and so would the work, if each nested display started a new
        // %A formatter without a limit that they share
        let text = $"%A{DisplayTrees.Shared levels}"
        Assert.Contains ("...", text, "the display must end in '...' where it is cut")
        Assert.IsLessThan (boundedLength, text.Length, "the display must have a bounded length")

    [<TestMethod; DataRow 3; DataRow 6>]
    member _.``a tree that holds the same child twice is shown completely inside the limits`` (levels : int) =
        let text = $"%A{DisplayTrees.Shared levels}"
        Assert.AreEqual (1 <<< levels, occurrences text "Leaf 0", "every leaf of the tree must be shown")
        Assert.DoesNotContain ("...", text, "a tree inside the limits must not be cut")

    [<TestMethod; DataRow 4; DataRow 8; DataRow 9>]
    member _.``a balanced tree of up to 511 arrays is shown completely`` (levels : int) =
        let text = $"%A{DisplayTrees.Balanced levels}"
        Assert.AreEqual (1 <<< levels, occurrences text "Leaf ", "every leaf of the tree must be shown")
        Assert.DoesNotContain ("...", text, "a tree inside the limits must not be cut")

    [<TestMethod; Timeout 60000; DataRow 10; DataRow 14>]
    member _.``a balanced tree of more than 1000 arrays is cut`` (levels : int) =
        let text = $"%A{DisplayTrees.Balanced levels}"
        Assert.Contains ("...", text, "the display must end in '...' where it is cut")
        Assert.IsLessThan (1 <<< levels, occurrences text "Leaf ", "the cut display must show fewer leaves than the tree has")
        Assert.IsLessThan (boundedLength, text.Length, "the display must have a bounded length")

    [<TestMethod>]
    member _.``a display after a cut display has a new budget`` () =
        // The limits belong to one outermost display: the next one starts again
        $"%A{DisplayTrees.Shared 100}" |> ignore
        Assert.AreEqual ("[|[|1|]|]", (ea [| ea [| 1 |] |]).ToString(), "a display after a cut one must start at depth 0 again")
        Assert.AreEqual (64, occurrences $"%A{DisplayTrees.Shared 6}" "Leaf 0", "the next big display must have its full budget")

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

    [<TestMethod>]
    member _.``a record field shows the elements`` () =
        let document = { Name = "x"; Tags = ea [| "a"; "b" |] }
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
