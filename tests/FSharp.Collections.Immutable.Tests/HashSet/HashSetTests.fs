namespace FSharp.Collections.Immutable.Tests

open System
open System.Collections.Generic
open System.Globalization
open FSharp.Collections.Immutable
open Microsoft.VisualStudio.TestTools.UnitTesting

/// <summary>
/// The behaviour of <c>HashSet</c> that the shared contracts do not cover: comparers, the struct of <c>forComparer</c>,
/// shared storage, null inputs and the extras.
/// </summary>
/// <remarks>
/// The tables of functions are maps by name, and the names are the rows of their tests: MSTest names a row after its
/// values, and neither a function nor a hash set prints a readable value.
/// </remarks>
[<TestClass; HashSetTestCategory>]
type HashSetTests () =

    static let ignoreCase : IEqualityComparer<string> = StringComparer.OrdinalIgnoreCase

    static let defaultComparer : IEqualityComparer<string> = EqualityComparer<string>.Default

    /// Creates hash sets that compare without case.
    static let caseless = HashSet.forComparer ignoreCase

    /// Words that compare without case, so that a value can equal an element that differs from it.
    static let words () = caseless.ofArray [| "apple"; "Banana"; "cherry" |]

    /// The elements of a hash set, sorted in the order of F# generic comparison.
    static let sorted (set : HashSet<'T>) = HashSet.toArray set |> Array.sort

    static let isLong (word : string) = word.Length > 5

    static let byLength (word : string) = if isLong word then Choice1Of2 word else Choice2Of2 word

    static let nullSet = Unchecked.defaultof<HashSet<int>>

    static let nullComparer = Unchecked.defaultof<IEqualityComparer<string>>

    /// The hash set of a builder of the struct that added the elements.
    static let ofCaselessBuilder (elements : string array) =
        let builder = caseless.builder ()
        builder.UnionWith elements
        HashSet.ofBuilder builder

    /// The creation functions without a comparer, by name.
    static let defaultCreations : Map<string, unit -> HashSet<string>> =
        Map.ofArray [|
            nameof HashSet.empty, (fun () -> HashSet.empty<string>)
            nameof HashSet.ofArray, (fun () -> HashSet.ofArray [| "a" |])
            // The function under test takes a list
            nameof HashSet.ofList, (fun () -> HashSet.ofList [ "a" ])
            nameof HashSet.ofReadOnlySpan, (fun () -> HashSet.ofReadOnlySpan (ReadOnlySpan<string> [| "a" |]))
            nameof HashSet.ofSeq, (fun () -> HashSet.ofSeq (Seq.singleton "a"))
            nameof HashSet.ofSpan, (fun () -> HashSet.ofSpan (Span<string> [| "a" |]))
            nameof HashSet.singleton, (fun () -> HashSet.singleton "a")
            nameof HashSet.build, (fun () -> HashSet.build (fun builder -> builder.Add "a" |> ignore))
            nameof HashSet.builder, (fun () -> HashSet.ofBuilder (HashSet.builder ()))
        |]

    /// The members of the struct, by name, each with the hash set that it creates from "apple" and "APPLE".
    static let structCreations : Map<string, unit -> HashSet<string>> =
        Map.ofArray [|
            nameof caseless.empty, (fun () -> caseless.empty |> HashSet.add "apple" |> HashSet.add "APPLE")
            nameof caseless.ofArray, (fun () -> caseless.ofArray [| "apple"; "APPLE" |])
            // The member under test takes a list
            nameof caseless.ofList, (fun () -> caseless.ofList [ "apple"; "APPLE" ])
            nameof caseless.ofReadOnlySpan, (fun () -> caseless.ofReadOnlySpan (ReadOnlySpan<string> [| "apple"; "APPLE" |]))
            nameof caseless.ofSeq, (fun () -> caseless.ofSeq (Seq.ofArray [| "apple"; "APPLE" |]))
            nameof caseless.ofSpan, (fun () -> caseless.ofSpan (Span<string> [| "apple"; "APPLE" |]))
            nameof caseless.singleton, (fun () -> caseless.singleton "apple" |> HashSet.add "APPLE")
            nameof caseless.build, (fun () -> caseless.build (fun builder -> builder.UnionWith [| "apple"; "APPLE" |]))
            nameof caseless.builder, (fun () -> ofCaselessBuilder [| "apple"; "APPLE" |])
        |]

    /// The incremental changes, by name, applied to the words.
    static let changes : Map<string, HashSet<string> -> HashSet<string>> =
        Map.ofArray [|
            nameof HashSet.add, HashSet.add "date"
            nameof HashSet.addRange, HashSet.addRange [| "APPLE"; "date" |]
            nameof HashSet.clear, HashSet.clear
            nameof HashSet.remove, HashSet.remove "APPLE"
            nameof HashSet.removeRange, HashSet.removeRange [| "APPLE"; "date" |]
            nameof HashSet.update, HashSet.update (fun builder -> builder.Add "date" |> ignore)
        |]

    /// The functions that select elements, by name, applied to the words.
    static let selections : Map<string, HashSet<string> -> HashSet<string>> =
        Map.ofArray [|
            nameof HashSet.filter, HashSet.filter isLong
            ($"%s{nameof HashSet.partition} (accepted)",
             fun set ->
                 let struct (accepted, _) = HashSet.partition isLong set
                 accepted)
            ($"%s{nameof HashSet.partition} (rejected)",
             fun set ->
                 let struct (_, rejected) = HashSet.partition isLong set
                 rejected)
        |]

    /// The set algebra, by name, with the elements that it returns for the words and the hash set of "APPLE" and "date".
    static let algebra : Map<string, struct ((HashSet<string> -> HashSet<string> -> HashSet<string>) * string array)> =
        Map.ofArray [|
            nameof HashSet.difference, struct (HashSet.difference, [| "Banana"; "cherry" |])
            // Intersect keeps the elements of the second set
            nameof HashSet.intersect, struct (HashSet.intersect, [| "APPLE" |])
            nameof HashSet.symmetricDifference, struct (HashSet.symmetricDifference, [| "Banana"; "cherry"; "date" |])
            nameof HashSet.union, struct (HashSet.union, [| "Banana"; "apple"; "cherry"; "date" |])
        |]

    /// The transformations to another element type, by name, applied to the words.
    static let typeChanges : Map<string, HashSet<string> -> HashSet<string>> =
        Map.ofArray [|
            nameof HashSet.map, HashSet.map id
            nameof HashSet.choose, HashSet.choose ValueSome
            ($"%s{nameof HashSet.partitionWith} (first)",
             fun set ->
                 let struct (first, _) = HashSet.partitionWith byLength set
                 first)
            ($"%s{nameof HashSet.partitionWith} (second)",
             fun set ->
                 let struct (_, second) = HashSet.partitionWith byLength set
                 second)
        |]

    /// The functions that take a hash set, by name and parameter, with the name of the parameter and a call with null.
    static let nullRejections : Map<string, struct (string * (unit -> unit))> =
        let set = HashSet.ofArray Inputs.Several

        Map.ofArray [|
            nameof HashSet.add, struct ("set", (fun () -> HashSet.add 1 nullSet |> ignore))
            nameof HashSet.addRange, struct ("set", (fun () -> HashSet.addRange [| 1 |] nullSet |> ignore))
            nameof HashSet.choose, struct ("set", (fun () -> HashSet.choose ValueSome nullSet |> ignore))
            nameof HashSet.clear, struct ("set", (fun () -> HashSet.clear nullSet |> ignore))
            nameof HashSet.comparer, struct ("set", (fun () -> HashSet.comparer nullSet |> ignore))
            nameof HashSet.contains, struct ("set", (fun () -> HashSet.contains 1 nullSet |> ignore))
            nameof HashSet.count, struct ("set", (fun () -> HashSet.count nullSet |> ignore))
            $"%s{nameof HashSet.difference} (set1)", struct ("set1", (fun () -> HashSet.difference nullSet set |> ignore))
            $"%s{nameof HashSet.difference} (set2)", struct ("set2", (fun () -> HashSet.difference set nullSet |> ignore))
            nameof HashSet.exists, struct ("set", (fun () -> HashSet.exists ((=) 1) nullSet |> ignore))
            nameof HashSet.filter, struct ("set", (fun () -> HashSet.filter ((=) 1) nullSet |> ignore))
            nameof HashSet.fold, struct ("set", (fun () -> HashSet.fold (+) 0 nullSet |> ignore))
            nameof HashSet.foldBack, struct ("set", (fun () -> HashSet.foldBack (+) nullSet 0 |> ignore))
            nameof HashSet.forall, struct ("set", (fun () -> HashSet.forall ((=) 1) nullSet |> ignore))
            $"%s{nameof HashSet.intersect} (set1)", struct ("set1", (fun () -> HashSet.intersect nullSet set |> ignore))
            $"%s{nameof HashSet.intersect} (set2)", struct ("set2", (fun () -> HashSet.intersect set nullSet |> ignore))
            nameof HashSet.intersectMany, struct ("sets", (fun () -> HashSet.intersectMany [| set; nullSet |] |> ignore))
            nameof HashSet.isEmpty, struct ("set", (fun () -> HashSet.isEmpty nullSet |> ignore))
            $"%s{nameof HashSet.isProperSubset} (set1)", struct ("set1", (fun () -> HashSet.isProperSubset nullSet set |> ignore))
            $"%s{nameof HashSet.isProperSubset} (set2)", struct ("set2", (fun () -> HashSet.isProperSubset set nullSet |> ignore))
            $"%s{nameof HashSet.isProperSuperset} (set1)",
            struct ("set1", (fun () -> HashSet.isProperSuperset nullSet set |> ignore))
            $"%s{nameof HashSet.isProperSuperset} (set2)",
            struct ("set2", (fun () -> HashSet.isProperSuperset set nullSet |> ignore))
            $"%s{nameof HashSet.isSubset} (set1)", struct ("set1", (fun () -> HashSet.isSubset nullSet set |> ignore))
            $"%s{nameof HashSet.isSubset} (set2)", struct ("set2", (fun () -> HashSet.isSubset set nullSet |> ignore))
            $"%s{nameof HashSet.isSuperset} (set1)", struct ("set1", (fun () -> HashSet.isSuperset nullSet set |> ignore))
            $"%s{nameof HashSet.isSuperset} (set2)", struct ("set2", (fun () -> HashSet.isSuperset set nullSet |> ignore))
            nameof HashSet.iter, struct ("set", (fun () -> HashSet.iter ignore nullSet))
            nameof HashSet.map, struct ("set", (fun () -> HashSet.map id nullSet |> ignore))
            $"%s{nameof HashSet.overlaps} (set1)", struct ("set1", (fun () -> HashSet.overlaps nullSet set |> ignore))
            $"%s{nameof HashSet.overlaps} (set2)", struct ("set2", (fun () -> HashSet.overlaps set nullSet |> ignore))
            nameof HashSet.partition, struct ("set", (fun () -> HashSet.partition ((=) 1) nullSet |> ignore))
            nameof HashSet.partitionWith,
            struct ("set",
                    (fun () ->
                        HashSet.partitionWith (fun (item : int) -> Choice<int, int>.Choice1Of2 item) nullSet
                        |> ignore
                    ))
            nameof HashSet.remove, struct ("set", (fun () -> HashSet.remove 1 nullSet |> ignore))
            nameof HashSet.removeRange, struct ("set", (fun () -> HashSet.removeRange [| 1 |] nullSet |> ignore))
            $"%s{nameof HashSet.setEquals} (set1)", struct ("set1", (fun () -> HashSet.setEquals nullSet set |> ignore))
            $"%s{nameof HashSet.setEquals} (set2)", struct ("set2", (fun () -> HashSet.setEquals set nullSet |> ignore))
            $"%s{nameof HashSet.symmetricDifference} (set1)",
            struct ("set1", (fun () -> HashSet.symmetricDifference nullSet set |> ignore))
            $"%s{nameof HashSet.symmetricDifference} (set2)",
            struct ("set2", (fun () -> HashSet.symmetricDifference set nullSet |> ignore))
            nameof HashSet.toArray, struct ("set", (fun () -> HashSet.toArray nullSet |> ignore))
            nameof HashSet.toBuilder, struct ("set", (fun () -> HashSet.toBuilder nullSet |> ignore))
            nameof HashSet.toList, struct ("set", (fun () -> HashSet.toList nullSet |> ignore))
            nameof HashSet.toSeq, struct ("set", (fun () -> HashSet.toSeq nullSet |> ignore))
            nameof HashSet.tryGetValue, struct ("set", (fun () -> HashSet.tryGetValue 1 nullSet |> ignore))
            $"%s{nameof HashSet.union} (set1)", struct ("set1", (fun () -> HashSet.union nullSet set |> ignore))
            $"%s{nameof HashSet.union} (set2)", struct ("set2", (fun () -> HashSet.union set nullSet |> ignore))
            nameof HashSet.unionMany, struct ("sets", (fun () -> HashSet.unionMany [| nullSet; set |] |> ignore))
            nameof HashSet.update, struct ("set", (fun () -> HashSet.update ignore nullSet |> ignore))
        |]

    /// <summary>
    /// The functions that take a sequence, by name and by the input that they reject, with the parameter that names the
    /// input and the function on it.
    /// </summary>
    /// <remarks>
    /// A default flat list converts to a sequence without a check and fails when the sequence is enumerated, so the other
    /// inputs are valid and the function would enumerate the sequence (ADR 0001, D6).
    /// </remarks>
    static let sequenceRejections : Map<string, struct (string * (unit -> unit))> =
        let set = HashSet.ofArray Inputs.Several
        let ints = HashSet.forComparer (EqualityComparer<int>.Default :> IEqualityComparer<int>)
        let nullElements = Unchecked.defaultof<seq<int>>
        let defaultElements = Unchecked.defaultof<FlatList<int>> :> seq<int>
        let nullSets = Unchecked.defaultof<seq<HashSet<int>>>
        let defaultSets = Unchecked.defaultof<FlatList<HashSet<int>>> :> seq<HashSet<int>>

        // The flat list of strings is a sequence of objects, which a type test for the flat list of objects does not see
        let widenedElements = FlatListOracles.defaultStringsAsObjects
        let objects = HashSet.ofArray [| ("a" :> obj) |]

        Map.ofArray [|
            $"%s{nameof HashSet.addRange} (null elements)",
            struct ("elements", (fun () -> HashSet.addRange nullElements set |> ignore))
            $"%s{nameof HashSet.addRange} (default flat list)",
            struct ("elements", (fun () -> HashSet.addRange defaultElements set |> ignore))
            $"%s{nameof HashSet.addRange} (default flat list of strings as objects)",
            struct ("elements", (fun () -> HashSet.addRange widenedElements objects |> ignore))
            $"%s{nameof HashSet.intersectMany} (null sets)", struct ("sets", (fun () -> HashSet.intersectMany nullSets |> ignore))
            $"%s{nameof HashSet.intersectMany} (default flat list)",
            struct ("sets", (fun () -> HashSet.intersectMany defaultSets |> ignore))
            $"%s{nameof HashSet.ofSeq} (null elements)", struct ("elements", (fun () -> HashSet.ofSeq nullElements |> ignore))
            $"%s{nameof HashSet.ofSeq} (default flat list)",
            struct ("elements", (fun () -> HashSet.ofSeq defaultElements |> ignore))
            $"%s{nameof HashSet.ofSeq} (default flat list of strings as objects)",
            struct ("elements", (fun () -> HashSet.ofSeq widenedElements |> ignore))
            $"%s{nameof HashSet.removeRange} (null elements)",
            struct ("elements", (fun () -> HashSet.removeRange nullElements set |> ignore))
            $"%s{nameof HashSet.removeRange} (default flat list)",
            struct ("elements", (fun () -> HashSet.removeRange defaultElements set |> ignore))
            $"%s{nameof HashSet.removeRange} (default flat list of strings as objects)",
            struct ("elements", (fun () -> HashSet.removeRange widenedElements objects |> ignore))
            $"%s{nameof HashSet.unionMany} (null sets)", struct ("sets", (fun () -> HashSet.unionMany nullSets |> ignore))
            $"%s{nameof HashSet.unionMany} (default flat list)",
            struct ("sets", (fun () -> HashSet.unionMany defaultSets |> ignore))
            // The struct of forComparer
            $"forComparer %s{nameof ints.ofSeq} (null elements)",
            struct ("elements", (fun () -> ints.ofSeq nullElements |> ignore))
            $"forComparer %s{nameof ints.ofSeq} (default flat list)",
            struct ("elements", (fun () -> ints.ofSeq defaultElements |> ignore))
        |]

    /// <summary>
    /// The relations of a hash set with a sequence, by name: the member of the BCL type, which takes the sequence, and the
    /// function of the module, which takes a hash set.
    /// </summary>
    static let relationFunctions
        : Map<string, struct ((HashSet<string> -> seq<string> -> bool) * (HashSet<string> -> HashSet<string> -> bool))> =
        Map.ofArray [|
            nameof HashSet.isProperSubset, struct ((fun set elements -> set.IsProperSubsetOf elements), HashSet.isProperSubset)
            nameof HashSet.isProperSuperset,
            struct ((fun set elements -> set.IsProperSupersetOf elements), HashSet.isProperSuperset)
            nameof HashSet.isSubset, struct ((fun set elements -> set.IsSubsetOf elements), HashSet.isSubset)
            nameof HashSet.isSuperset, struct ((fun set elements -> set.IsSupersetOf elements), HashSet.isSuperset)
            nameof HashSet.overlaps, struct ((fun set elements -> set.Overlaps elements), HashSet.overlaps)
            nameof HashSet.setEquals, struct ((fun set elements -> set.SetEquals elements), HashSet.setEquals)
        |]

    /// <summary>
    /// The operations of the set algebra with a sequence, by name: the member of the BCL type, which takes the sequence, and
    /// the function of the module, which takes a hash set.
    /// </summary>
    static let operationFunctions
        : Map<
              string,
              struct ((HashSet<string> -> seq<string> -> HashSet<string>) *
              (HashSet<string> -> HashSet<string> -> HashSet<string>))
           > =
        Map.ofArray [|
            nameof HashSet.intersect, struct ((fun set elements -> set.Intersect elements), HashSet.intersect)
            nameof HashSet.symmetricDifference,
            struct ((fun set elements -> set.SymmetricExcept elements), HashSet.symmetricDifference)
        |]

    /// The relations that need no conversion, by name, as the changelog gives them: each tests the elements with contains.
    static let streamingRecipes : Map<string, HashSet<string> -> seq<string> -> bool> =
        Map.ofArray [|
            nameof HashSet.isSuperset, (fun set elements -> Seq.forall (fun item -> HashSet.contains item set) elements)
            nameof HashSet.overlaps, (fun set elements -> Seq.exists (fun item -> HashSet.contains item set) elements)
        |]

    /// The names of the creation functions without a comparer, as rows.
    static member DefaultCreations = defaultCreations.Keys

    /// The names of the members of the struct that create hash sets, as rows.
    static member StructCreations = structCreations.Keys

    /// The names of the incremental changes, as rows.
    static member Changes = changes.Keys

    /// The names of the functions that select elements, as rows.
    static member Selections = selections.Keys

    /// The names of the set algebra, as rows.
    static member Algebra = algebra.Keys

    /// The names of the transformations to another element type, as rows.
    static member TypeChanges = typeChanges.Keys

    /// The names of the functions that take a hash set, with the parameter where a function takes two, as rows.
    static member NullRejections = nullRejections.Keys

    /// The names of the functions that take a sequence, with the input that they reject, as rows.
    static member SequenceRejections = sequenceRejections.Keys

    // ---- Comparers ----

    [<TestMethod; DynamicData(nameof HashSetTests.DefaultCreations)>]
    member _.``creation without a comparer compares with the default comparer`` (name : string) =
        Assert.AreSame (
            defaultComparer,
            HashSet.comparer (defaultCreations[name]()),
            $"HashSet.%s{name} must compare with EqualityComparer.Default"
        )

    [<TestMethod>]
    member _.``forComparer creates the struct with the comparer`` () =
        Assert.AreSame (ignoreCase, (HashSet.forComparer ignoreCase).Comparer, "HashSet.forComparer must keep the comparer")

        Assert.AreSame (
            ignoreCase,
            (CustomComparerHashSet ignoreCase).Comparer,
            "The constructor of CustomComparerHashSet must keep the comparer"
        )

    [<TestMethod; DynamicData(nameof HashSetTests.StructCreations)>]
    member _.``the members of the struct create hash sets that compare with its comparer`` (name : string) =
        let set = structCreations[name]()
        Assert.AreSame (ignoreCase, HashSet.comparer set, $"CustomComparerHashSet.%s{name} must keep the comparer")

        Assert.AreEqual<string>(
            "apple",
            Assert.ContainsSingle (set, $"CustomComparerHashSet.%s{name} must compare without case"),
            $"CustomComparerHashSet.%s{name} must keep the first of the elements that the comparer considers equal"
        )

    [<TestMethod>]
    member _.``the struct gives an existing hash set its comparer`` () =
        let set = HashSet.ofArray [| "apple"; "APPLE"; "Banana" |]
        let ignoringCase = caseless.ofSeq set

        Assert.AreSame (
            ignoreCase,
            HashSet.comparer ignoringCase,
            "CustomComparerHashSet.ofSeq must give the hash set the comparer"
        )
        Assert.HasCount (2, ignoringCase, "CustomComparerHashSet.ofSeq must merge the elements that the comparer considers equal")
        Assert.HasCount (3, set, "CustomComparerHashSet.ofSeq must not change the hash set")

        Assert.AreSame (
            ignoringCase,
            caseless.ofSeq ignoringCase,
            "CustomComparerHashSet.ofSeq of a hash set with the comparer must return it"
        )

    // The BCL tests the instance of the comparer, not its behaviour, so two equal comparers are two comparers (D14)
    [<TestMethod>]
    member _.``the struct returns a hash set only for the same comparer instance`` () =
        let first : IEqualityComparer<string> = StringComparer.Create (CultureInfo.InvariantCulture, true)
        let second : IEqualityComparer<string> = StringComparer.Create (CultureInfo.InvariantCulture, true)
        Assert.AreNotSame (first, second, "The test needs two comparer instances")
        Assert.AreEqual<obj>(first, second, "The test needs two comparers that are equal")

        let set = (HashSet.forComparer first).ofArray [| "apple"; "Banana" |]

        Assert.AreSame (
            set,
            (HashSet.forComparer first).ofSeq set,
            "CustomComparerHashSet.ofSeq must return a hash set that has the same comparer instance"
        )

        let rebuilt = (HashSet.forComparer second).ofSeq set
        Assert.AreNotSame (set, rebuilt, "CustomComparerHashSet.ofSeq must rebuild a hash set whose comparer is another instance")
        Assert.AreSame (
            second,
            HashSet.comparer rebuilt,
            "CustomComparerHashSet.ofSeq must give the rebuilt hash set its comparer"
        )
        CollectionAssert.AreEqual ([| "Banana"; "apple" |], sorted rebuilt, "CustomComparerHashSet.ofSeq must keep the elements")

    [<TestMethod>]
    member _.``the default value of the struct compares with the default comparer`` () =
        let defaultValue = Unchecked.defaultof<CustomComparerHashSet<string>>
        let set = defaultValue.ofArray [| "apple"; "APPLE" |]

        Assert.AreSame (
            defaultComparer,
            defaultValue.Comparer,
            "The default CustomComparerHashSet must return the default comparer"
        )
        Assert.AreSame (defaultComparer, HashSet.comparer set, "The default CustomComparerHashSet must create hash sets with it")
        Assert.HasCount (2, set, "The default CustomComparerHashSet must tell the case apart")

    [<TestMethod; DynamicData(nameof HashSetTests.Changes)>]
    member _.``add, addRange, clear, remove, removeRange and update keep the comparer`` (name : string) =
        Assert.AreSame (ignoreCase, HashSet.comparer (changes[name](words ())), $"HashSet.%s{name} must keep the comparer")

    [<TestMethod>]
    member _.``addRange adds the elements that are not equal in the comparer to an element of the hash set`` () =
        // "APPLE" equals "apple" without case, so the hash set keeps "apple" (ADR 0001, D14)
        CollectionAssert.AreEqual (
            [| "Banana"; "apple"; "cherry"; "date" |],
            sorted (HashSet.addRange [| "APPLE"; "date" |] (words ())),
            "HashSet.addRange must compare with the comparer of the hash set"
        )

    [<TestMethod>]
    member _.``addRange to an empty hash set keeps its comparer for a hash set with another comparer`` () =
        // ImmutableHashSet.Union answers an empty hash set with the hash set of its argument, rebuilt with the comparer
        let result = HashSet.addRange (HashSet.ofArray [| "apple"; "APPLE" |]) caseless.empty

        Assert.AreSame (ignoreCase, HashSet.comparer result, "HashSet.addRange must keep the comparer of the hash set")
        Assert.HasCount (1, result, "HashSet.addRange must compare with the comparer of the hash set")

    // ImmutableHashSet.Union answers an empty input that receives a hash set with that hash set, or with a copy of it that
    // has the comparer of the input, so the empty input itself never comes back from the BCL member (ADR 0001, D14)
    [<TestMethod>]
    member _.``addRange returns an empty input for an empty hash set`` () =
        let set = caseless.empty

        Assert.AreSame (
            set,
            HashSet.addRange HashSet.empty<string> set,
            "HashSet.addRange of an empty hash set with another comparer must return the input"
        )

        Assert.AreSame (
            set,
            HashSet.addRange caseless.empty set,
            "HashSet.addRange of an empty hash set with the same comparer must return the input"
        )

    [<TestMethod>]
    member _.``remove removes the element that equals the value in the comparer`` () =
        CollectionAssert.AreEqual (
            [| "Banana"; "cherry" |],
            sorted (HashSet.remove "APPLE" (words ())),
            "HashSet.remove must remove the element that equals the value in the comparer"
        )

    [<TestMethod>]
    member _.``removeRange removes the elements that are equal in the comparer to an element of the sequence`` () =
        CollectionAssert.AreEqual (
            [| "Banana"; "cherry" |],
            sorted (HashSet.removeRange [| "APPLE"; "date" |] (words ())),
            "HashSet.removeRange must compare with the comparer of the hash set"
        )

    [<TestMethod; DynamicData(nameof HashSetTests.Selections)>]
    member _.``filter and partition keep the comparer`` (name : string) =
        Assert.AreSame (ignoreCase, HashSet.comparer (selections[name](words ())), $"HashSet.%s{name} must keep the comparer")

    [<TestMethod>]
    member _.``partition keeps comparing without case`` () =
        let struct (accepted, _) = HashSet.partition isLong (words ())
        Assert.IsTrue (HashSet.contains "BANANA" accepted, "HashSet.partition must keep comparing without case")

    [<TestMethod; DynamicData(nameof HashSetTests.Algebra)>]
    member _.``set algebra keeps the comparer of the first set and compares with it`` (name : string) =
        let struct (combine, expected) = algebra[name]
        let result = combine (words ()) (HashSet.ofArray [| "APPLE"; "date" |])

        Assert.AreSame (ignoreCase, HashSet.comparer result, $"HashSet.%s{name} must keep the comparer of the first set")
        CollectionAssert.AreEqual (expected, sorted result, $"HashSet.%s{name} must compare with the comparer of the first set")

    [<TestMethod>]
    member _.``isProperSuperset counts the elements that the comparer of the first set finds equal once`` () =
        // The second set tells "apple" and "APPLE" apart; for the first set they are one element, and it holds "Banana"
        // besides, as the mutable HashSet answers (ADR 0001, D14)
        Assert.IsTrue (
            HashSet.isProperSuperset (caseless.ofArray [| "apple"; "Banana" |]) (HashSet.ofArray [| "apple"; "APPLE" |]),
            "HashSet.isProperSuperset must count the elements that the comparer of the first set finds equal once"
        )

    [<TestMethod>]
    member _.``the predicates of two sets compare with the comparer of the first set`` () =
        let apple = caseless.ofArray [| "APPLE" |]
        let set = words ()

        Assert.IsTrue (HashSet.isSubset apple set, "HashSet.isSubset must compare without case")
        Assert.IsTrue (HashSet.isProperSubset apple set, "HashSet.isProperSubset must compare without case")
        Assert.IsTrue (HashSet.isSuperset set apple, "HashSet.isSuperset must compare without case")
        Assert.IsTrue (HashSet.isProperSuperset set apple, "HashSet.isProperSuperset must compare without case")
        Assert.IsTrue (HashSet.overlaps apple set, "HashSet.overlaps must compare without case")

        Assert.IsTrue (
            HashSet.setEquals set (HashSet.ofArray [| "APPLE"; "BANANA"; "CHERRY" |]),
            "HashSet.setEquals must compare without case"
        )

        Assert.IsFalse (
            HashSet.isSubset (HashSet.ofArray [| "APPLE" |]) set,
            "HashSet.isSubset must compare with the default comparer of the first set"
        )

    [<TestMethod>]
    member _.``intersectMany and unionMany keep the comparer of the first set`` () =
        let sets = [| words (); HashSet.ofArray [| "APPLE"; "date" |] |]
        let intersection = HashSet.intersectMany sets
        let union = HashSet.unionMany sets

        Assert.AreSame (
            ignoreCase,
            HashSet.comparer intersection,
            "HashSet.intersectMany must keep the comparer of the first set"
        )
        Assert.AreSame (ignoreCase, HashSet.comparer union, "HashSet.unionMany must keep the comparer of the first set")
        CollectionAssert.AreEqual ([| "APPLE" |], sorted intersection, "HashSet.intersectMany must compare without case")

        CollectionAssert.AreEqual (
            [| "Banana"; "apple"; "cherry"; "date" |],
            sorted union,
            "HashSet.unionMany must compare without case"
        )

    [<TestMethod>]
    member _.``unionMany of no sets is empty with the default comparer`` () =
        let union = HashSet.unionMany Seq.empty<HashSet<string>>
        Assert.IsEmpty (union, "HashSet.unionMany of no sets must be empty")
        Assert.AreSame (
            defaultComparer,
            HashSet.comparer union,
            "HashSet.unionMany of no sets must compare with the default comparer"
        )

    [<TestMethod; DynamicData(nameof HashSetTests.TypeChanges)>]
    member _.``map, choose and partitionWith compare with the default comparer of the result`` (name : string) =
        Assert.AreSame (
            defaultComparer,
            HashSet.comparer (typeChanges[name](words ())),
            $"HashSet.%s{name} must compare with the default comparer"
        )

    [<TestMethod>]
    member _.``map does not keep the comparer of its input`` () =
        // The default comparer tells the case apart, although the element type is the same
        Assert.HasCount (
            4,
            HashSet.add "APPLE" (HashSet.map id (words ())),
            "HashSet.map must not keep the comparer of its input"
        )

    // ---- Sharing ----

    [<TestMethod>]
    member _.``ofSeq of a hash set with the default comparer returns it`` () =
        let set = HashSet.ofArray Inputs.Several
        Assert.AreSame (set, HashSet.ofSeq set, "HashSet.ofSeq of a hash set with the default comparer must not copy it")

        let copy = HashSet.ofSeq (words ())
        Assert.AreSame (defaultComparer, HashSet.comparer copy, "HashSet.ofSeq must compare with the default comparer")
        Assert.HasCount (3, copy, "HashSet.ofSeq must keep the elements of a hash set with another comparer")

    [<TestMethod>]
    member _.``add returns the input when it contains an equal element`` () =
        let set = words ()
        Assert.AreSame (set, HashSet.add "APPLE" set, "HashSet.add of an element equal in the comparer must return the input")

    [<TestMethod>]
    member _.``remove returns the input when it has no equal element`` () =
        let set = words ()
        Assert.AreSame (set, HashSet.remove "date" set, "HashSet.remove of a missing element must return the input")

    [<TestMethod>]
    member _.``clear returns an empty input`` () =
        let set = caseless.empty
        Assert.AreSame (set, HashSet.clear set, "HashSet.clear of an empty hash set must return the input")

    // ---- Access and search ----

    [<TestMethod>]
    member _.``tryGetValue returns the element that the hash set holds`` () =
        let stored = String ('x', 3)
        let set = caseless.ofArray [| stored; "apple" |]

        match HashSet.tryGetValue "XXX" set with
        | ValueSome found -> Assert.AreSame (stored, found, "HashSet.tryGetValue must return the instance of the hash set")
        | ValueNone -> Assert.Fail "HashSet.tryGetValue must find the element that equals the value in the comparer"

        Assert.AreEqual<string voption>(
            ValueSome "apple",
            HashSet.tryGetValue "APPLE" set,
            "HashSet.tryGetValue must compare without case"
        )
        Assert.AreEqual<string voption>(
            ValueNone,
            HashSet.tryGetValue "date" set,
            "HashSet.tryGetValue must return nothing for a missing value"
        )

    [<TestMethod>]
    member _.``contains compares with the comparer of the hash set`` () =
        Assert.IsTrue (HashSet.contains "BANANA" (words ()), "HashSet.contains must compare without case")
        Assert.IsFalse (
            HashSet.contains "BANANA" (HashSet.ofArray [| "Banana" |]),
            "HashSet.contains must compare with the default comparer"
        )

    // ---- Relations with a sequence ----

    // The way to pass a sequence to a relation, as the changelog gives it: convert the sequence with the comparer of the
    // first set. The default comparer gives the same answer when the comparer of the first set finds equal what the default
    // comparer finds equal, and another answer when it tells such strings apart, as the reference comparer does
    [<TestMethod; DynamicData(nameof SetRelationInputs.Rows, typeof<SetRelationInputs>)>]
    member _.``a relation with a sequence takes the sequence that the comparer of the first set converts``
        (relation : string, comparer : string)
        =
        let instances = SetRelationInputs.Strings [| "a1"; "a2" |]
        Assert.AreNotSame (
            instances[0],
            instances[1],
            "The test needs two strings with the same text that are different instances"
        )

        let struct (setItems, sequenceItems) = SetRelationInputs.Items[relation]
        let struct (bclMember, moduleFunction) = relationFunctions[relation]
        let struct (firstComparer, defaultFindsEqualWhatItFindsEqual) = SetRelationInputs.Comparers[comparer]
        let set = (HashSet.forComparer firstComparer).ofArray(SetRelationInputs.Strings setItems)
        let elements = SetRelationInputs.Strings sequenceItems
        let expected = bclMember set elements

        Assert.AreEqual<bool>(
            expected,
            moduleFunction set ((HashSet.forComparer (HashSet.comparer set)).ofSeq elements),
            $"HashSet.%s{relation} must answer as the member of the BCL type for the sequence that the comparer of the first set converts"
        )

        let withDefaultComparer = moduleFunction set (HashSet.ofSeq elements)

        if defaultFindsEqualWhatItFindsEqual then
            Assert.AreEqual<bool>(
                expected,
                withDefaultComparer,
                $"HashSet.%s{relation} must answer as the member of the BCL type for the sequence that the default comparer converts, because the comparer of the first set finds equal what the default comparer finds equal"
            )
        else
            Assert.AreNotEqual<bool>(
                expected,
                withDefaultComparer,
                $"HashSet.%s{relation} must answer differently for the sequence that the default comparer converts, because the comparer of the first set tells apart strings that the default comparer finds equal"
            )

    // The same recipe for the operations of the set algebra that take a sequence in the BCL type
    [<TestMethod; DynamicData(nameof SetRelationInputs.OperationRows, typeof<SetRelationInputs>)>]
    member _.``an operation with a sequence takes the sequence that the comparer of the first set converts``
        (operation : string, comparer : string)
        =
        let struct (setItems, sequenceItems) = SetRelationInputs.OperationItems[operation]
        let struct (bclMember, moduleFunction) = operationFunctions[operation]
        let struct (firstComparer, defaultFindsEqualWhatItFindsEqual) = SetRelationInputs.Comparers[comparer]
        let set = (HashSet.forComparer firstComparer).ofArray(SetRelationInputs.Strings setItems)
        let elements = SetRelationInputs.Strings sequenceItems
        let expected = HashSet.toArray (bclMember set elements)

        CollectionAssert.AreEquivalent (
            expected,
            HashSet.toArray (moduleFunction set ((HashSet.forComparer (HashSet.comparer set)).ofSeq elements)),
            $"HashSet.%s{operation} must return the result of the member of the BCL type for the sequence that the comparer of the first set converts"
        )

        let withDefaultComparer = HashSet.toArray (moduleFunction set (HashSet.ofSeq elements))

        if defaultFindsEqualWhatItFindsEqual then
            CollectionAssert.AreEquivalent (
                expected,
                withDefaultComparer,
                $"HashSet.%s{operation} must return the result of the member of the BCL type for the sequence that the default comparer converts, because the comparer of the first set finds equal what the default comparer finds equal"
            )
        else
            CollectionAssert.AreNotEquivalent (
                expected,
                withDefaultComparer,
                $"HashSet.%s{operation} must return another result for the sequence that the default comparer converts, because the comparer of the first set tells apart strings that the default comparer finds equal"
            )

    [<TestMethod; DynamicData(nameof SetRelationInputs.StreamingRows, typeof<SetRelationInputs>)>]
    member _.``overlaps and isSuperset with a sequence test each element with contains`` (relation : string, comparer : string) =
        let struct (setItems, sequenceItems) = SetRelationInputs.Items[relation]
        let struct (bclMember, _) = relationFunctions[relation]
        let struct (firstComparer, _) = SetRelationInputs.Comparers[comparer]
        let set = (HashSet.forComparer firstComparer).ofArray(SetRelationInputs.Strings setItems)
        let elements = SetRelationInputs.Strings sequenceItems

        Assert.AreEqual<bool>(
            bclMember set elements,
            streamingRecipes[relation] set elements,
            $"%s{relation} with a sequence must test each element with HashSet.contains, in the comparer of the first set"
        )

    // ---- Null inputs ----

    [<TestMethod; DynamicData(nameof HashSetTests.NullRejections)>]
    member _.``functions reject a null hash set`` (name : string) =
        let struct (paramName, actual) = nullRejections[name]

        let ex =
            Assert.ThrowsExactly<ArgumentNullException>(Action actual, $"HashSet.%s{name} must reject a null hash set")

        Assert.AreEqual<string | null>(paramName, ex.ParamName, $"HashSet.%s{name} must name the parameter of the hash set")

    [<TestMethod; DynamicData(nameof HashSetTests.SequenceRejections)>]
    member _.``functions that take a sequence reject a null sequence and a default flat list given as the sequence``
        (name : string)
        =
        let struct (paramName, actual) = sequenceRejections[name]
        FlatListAssert.RejectsNull ($"HashSet.%s{name}", paramName, actual)

    [<TestMethod>]
    member _.``a flat list of strings that is not default passes as a sequence of objects`` () =
        let widened = FlatListOracles.stringsAsObjects (FlatList.ofArray [| "a"; "b" |])
        let objects = HashSet.ofArray [| ("b" :> obj); ("c" :> obj) |]

        CollectionAssert.AreEquivalent (
            [| ("a" :> obj); ("b" :> obj) |],
            HashSet.toArray (HashSet.ofSeq widened),
            "HashSet.ofSeq must take the elements of a flat list of strings as objects"
        )

        CollectionAssert.AreEquivalent (
            [| ("a" :> obj); ("b" :> obj) |],
            HashSet.toArray ((HashSet.forComparer (EqualityComparer<obj>.Default :> IEqualityComparer<obj>)).ofSeq widened),
            "CustomComparerHashSet.ofSeq must take the elements of a flat list of strings as objects"
        )

        CollectionAssert.AreEquivalent (
            [| ("a" :> obj); ("b" :> obj); ("c" :> obj) |],
            HashSet.toArray (HashSet.addRange widened objects),
            "HashSet.addRange must take the elements of a flat list of strings as objects"
        )

        CollectionAssert.AreEquivalent (
            [| ("c" :> obj) |],
            HashSet.toArray (HashSet.removeRange widened objects),
            "HashSet.removeRange must take the elements of a flat list of strings as objects"
        )

    [<TestMethod>]
    member _.``ofBuilder rejects a null builder`` () =
        let ex =
            Assert.ThrowsExactly<ArgumentNullException>(
                Action (fun () ->
                    HashSet.ofBuilder (Unchecked.defaultof<HashSet<int>.Builder>)
                    |> ignore
                ),
                "HashSet.ofBuilder must reject a null builder"
            )

        Assert.AreEqual<string | null>("builder", ex.ParamName, "HashSet.ofBuilder must name the parameter builder")

    // forComparer calls the constructor of the struct, which checks the comparer, so both name its parameter (D12)
    [<TestMethod>]
    member _.``forComparer rejects a null comparer`` () =
        let ex =
            Assert.ThrowsExactly<ArgumentNullException>(
                Action (fun () -> HashSet.forComparer nullComparer |> ignore),
                "HashSet.forComparer must reject a null comparer"
            )

        Assert.AreEqual<string | null>("comparer", ex.ParamName, "HashSet.forComparer must name the parameter comparer")

    [<TestMethod>]
    member _.``the constructor of the struct rejects a null comparer`` () =
        let ex =
            Assert.ThrowsExactly<ArgumentNullException>(
                Action (fun () -> CustomComparerHashSet nullComparer |> ignore),
                "The constructor of CustomComparerHashSet must reject a null comparer"
            )

        Assert.AreEqual<string | null>(
            "comparer",
            ex.ParamName,
            "The constructor of CustomComparerHashSet must name the parameter comparer"
        )
