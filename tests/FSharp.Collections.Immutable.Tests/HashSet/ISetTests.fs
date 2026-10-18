namespace FSharp.Collections.Immutable.Tests

open System
open System.Collections.Generic
open System.Collections.Immutable
open FSharp.Collections.Immutable
open Microsoft.VisualStudio.TestTools.UnitTesting

/// <summary>
/// The behaviour of <c>ISet</c> that the shared contracts do not cover: the implementation and the comparer of the input,
/// the hash sets of the transformations to another element type and null inputs.
/// </summary>
/// <remarks>
/// The tables of functions are maps by name, and the names are the rows of their tests: MSTest names a row after its
/// values, and a function prints no readable value.
/// </remarks>
[<TestClass; ISetTestCategory>]
type ISetTests () =

    static let ignoreCase : IEqualityComparer<string> = StringComparer.OrdinalIgnoreCase

    static let defaultComparer : IEqualityComparer<string> = EqualityComparer<string>.Default

    /// A sorted set behind the interface, whose implementation differs from the hash sets of the module.
    static let sortedSet (elements : int array) = ImmutableSortedSet.CreateRange<int> elements :> IImmutableSet<int>

    /// Words that compare without case, in a hash set behind the interface.
    static let words () =
        (HashSet.forComparer ignoreCase).ofArray [| "apple"; "Banana"; "cherry" |] :> IImmutableSet<string>

    /// The comparer of a hash set behind the interface.
    static let comparerOf (set : IImmutableSet<string>) (message : string) =
        let hashSet = Assert.IsInstanceOfType<ImmutableHashSet<string>>(set, message)
        hashSet.KeyComparer

    static let isLong (word : string) = word.Length > 5

    static let byLength (word : string) = if isLong word then Choice1Of2 word else Choice2Of2 word

    static let nullSet = Unchecked.defaultof<IImmutableSet<int>>

    /// The transformations that keep the element type, by name, applied to a set and another set.
    static let sameTypeTransformations : Map<string, IImmutableSet<int> -> IImmutableSet<int> -> IImmutableSet<int>> =
        Map.ofArray [|
            nameof ISet.add, (fun set _ -> ISet.add 100 set)
            nameof ISet.addRange, (fun set other -> ISet.addRange other set)
            nameof ISet.clear, (fun set _ -> ISet.clear set)
            nameof ISet.difference, ISet.difference
            nameof ISet.filter, (fun set _ -> ISet.filter (fun item -> item > 0) set)
            nameof ISet.intersect, ISet.intersect
            nameof ISet.intersectMany, (fun set other -> ISet.intersectMany [| set; other |])
            ($"%s{nameof ISet.partition} (accepted)",
             fun set _ ->
                 let struct (accepted, _) = ISet.partition (fun item -> item > 0) set
                 accepted)
            ($"%s{nameof ISet.partition} (rejected)",
             fun set _ ->
                 let struct (_, rejected) = ISet.partition (fun item -> item > 0) set
                 rejected)
            nameof ISet.remove, (fun set _ -> ISet.remove 5 set)
            nameof ISet.removeRange, (fun set other -> ISet.removeRange other set)
            nameof ISet.symmetricDifference, ISet.symmetricDifference
            nameof ISet.union, ISet.union
            nameof ISet.unionMany, (fun set other -> ISet.unionMany [| set; other |])
        |]

    /// The transformations that keep the comparer, by name, applied to the words.
    static let comparerKeepers : Map<string, IImmutableSet<string> -> IImmutableSet<string>> =
        Map.ofArray [|
            nameof ISet.add, ISet.add "date"
            nameof ISet.addRange, ISet.addRange [| "APPLE"; "date" |]
            nameof ISet.clear, ISet.clear
            nameof ISet.filter, ISet.filter isLong
            ($"%s{nameof ISet.partition} (accepted)",
             fun set ->
                 let struct (accepted, _) = ISet.partition isLong set
                 accepted)
            ($"%s{nameof ISet.partition} (rejected)",
             fun set ->
                 let struct (_, rejected) = ISet.partition isLong set
                 rejected)
            nameof ISet.remove, ISet.remove "APPLE"
            nameof ISet.removeRange, ISet.removeRange [| "APPLE"; "date" |]
            nameof ISet.union, (fun set -> ISet.union set (HashSet.ofArray [| "APPLE"; "date" |]))
        |]

    /// The transformations to another element type, by name, applied to the words.
    static let typeChanges : Map<string, IImmutableSet<string> -> HashSet<string>> =
        Map.ofArray [|
            nameof ISet.map, ISet.map id
            nameof ISet.choose, ISet.choose ValueSome
            ($"%s{nameof ISet.partitionWith} (first)",
             fun set ->
                 let struct (first, _) = ISet.partitionWith byLength set
                 first)
            ($"%s{nameof ISet.partitionWith} (second)",
             fun set ->
                 let struct (_, second) = ISet.partitionWith byLength set
                 second)
        |]

    /// The functions that take a set, by name and parameter, with the name of the parameter and a call with null.
    static let nullRejections : Map<string, struct (string * (unit -> unit))> =
        let set = HashSet.ofArray Inputs.Several :> IImmutableSet<int>

        Map.ofArray [|
            nameof ISet.add, struct ("set", (fun () -> ISet.add 1 nullSet |> ignore))
            nameof ISet.addRange, struct ("set", (fun () -> ISet.addRange [| 1 |] nullSet |> ignore))
            nameof ISet.choose, struct ("set", (fun () -> ISet.choose ValueSome nullSet |> ignore))
            nameof ISet.clear, struct ("set", (fun () -> ISet.clear nullSet |> ignore))
            nameof ISet.contains, struct ("set", (fun () -> ISet.contains 1 nullSet |> ignore))
            nameof ISet.count, struct ("set", (fun () -> ISet.count nullSet |> ignore))
            $"%s{nameof ISet.difference} (set1)", struct ("set1", (fun () -> ISet.difference nullSet set |> ignore))
            $"%s{nameof ISet.difference} (set2)", struct ("set2", (fun () -> ISet.difference set nullSet |> ignore))
            nameof ISet.exists, struct ("set", (fun () -> ISet.exists ((=) 1) nullSet |> ignore))
            nameof ISet.filter, struct ("set", (fun () -> ISet.filter ((=) 1) nullSet |> ignore))
            nameof ISet.fold, struct ("set", (fun () -> ISet.fold (+) 0 nullSet |> ignore))
            nameof ISet.foldBack, struct ("set", (fun () -> ISet.foldBack (+) nullSet 0 |> ignore))
            nameof ISet.forall, struct ("set", (fun () -> ISet.forall ((=) 1) nullSet |> ignore))
            $"%s{nameof ISet.intersect} (set1)", struct ("set1", (fun () -> ISet.intersect nullSet set |> ignore))
            $"%s{nameof ISet.intersect} (set2)", struct ("set2", (fun () -> ISet.intersect set nullSet |> ignore))
            nameof ISet.intersectMany, struct ("sets", (fun () -> ISet.intersectMany [| set; nullSet |] |> ignore))
            nameof ISet.isEmpty, struct ("set", (fun () -> ISet.isEmpty nullSet |> ignore))
            $"%s{nameof ISet.isProperSubset} (set1)", struct ("set1", (fun () -> ISet.isProperSubset nullSet set |> ignore))
            $"%s{nameof ISet.isProperSubset} (set2)", struct ("set2", (fun () -> ISet.isProperSubset set nullSet |> ignore))
            $"%s{nameof ISet.isProperSuperset} (set1)", struct ("set1", (fun () -> ISet.isProperSuperset nullSet set |> ignore))
            $"%s{nameof ISet.isProperSuperset} (set2)", struct ("set2", (fun () -> ISet.isProperSuperset set nullSet |> ignore))
            $"%s{nameof ISet.isSubset} (set1)", struct ("set1", (fun () -> ISet.isSubset nullSet set |> ignore))
            $"%s{nameof ISet.isSubset} (set2)", struct ("set2", (fun () -> ISet.isSubset set nullSet |> ignore))
            $"%s{nameof ISet.isSuperset} (set1)", struct ("set1", (fun () -> ISet.isSuperset nullSet set |> ignore))
            $"%s{nameof ISet.isSuperset} (set2)", struct ("set2", (fun () -> ISet.isSuperset set nullSet |> ignore))
            nameof ISet.iter, struct ("set", (fun () -> ISet.iter ignore nullSet))
            nameof ISet.map, struct ("set", (fun () -> ISet.map id nullSet |> ignore))
            $"%s{nameof ISet.overlaps} (set1)", struct ("set1", (fun () -> ISet.overlaps nullSet set |> ignore))
            $"%s{nameof ISet.overlaps} (set2)", struct ("set2", (fun () -> ISet.overlaps set nullSet |> ignore))
            nameof ISet.partition, struct ("set", (fun () -> ISet.partition ((=) 1) nullSet |> ignore))
            nameof ISet.partitionWith,
            struct ("set",
                    (fun () ->
                        ISet.partitionWith (fun (item : int) -> Choice<int, int>.Choice1Of2 item) nullSet
                        |> ignore
                    ))
            nameof ISet.remove, struct ("set", (fun () -> ISet.remove 1 nullSet |> ignore))
            nameof ISet.removeRange, struct ("set", (fun () -> ISet.removeRange [| 1 |] nullSet |> ignore))
            $"%s{nameof ISet.setEquals} (set1)", struct ("set1", (fun () -> ISet.setEquals nullSet set |> ignore))
            $"%s{nameof ISet.setEquals} (set2)", struct ("set2", (fun () -> ISet.setEquals set nullSet |> ignore))
            $"%s{nameof ISet.symmetricDifference} (set1)",
            struct ("set1", (fun () -> ISet.symmetricDifference nullSet set |> ignore))
            $"%s{nameof ISet.symmetricDifference} (set2)",
            struct ("set2", (fun () -> ISet.symmetricDifference set nullSet |> ignore))
            nameof ISet.toArray, struct ("set", (fun () -> ISet.toArray nullSet |> ignore))
            nameof ISet.toList, struct ("set", (fun () -> ISet.toList nullSet |> ignore))
            nameof ISet.toSeq, struct ("set", (fun () -> ISet.toSeq nullSet |> ignore))
            nameof ISet.tryGetValue, struct ("set", (fun () -> ISet.tryGetValue 1 nullSet |> ignore))
            $"%s{nameof ISet.union} (set1)", struct ("set1", (fun () -> ISet.union nullSet set |> ignore))
            $"%s{nameof ISet.union} (set2)", struct ("set2", (fun () -> ISet.union set nullSet |> ignore))
            nameof ISet.unionMany, struct ("sets", (fun () -> ISet.unionMany [| nullSet; set |] |> ignore))
        |]

    /// A flat list of hash sets as a sequence of interface sets. A flat list of hash sets is also a sequence of interface
    /// sets, by the covariance of seq: the element type of the sequence is not the one of the flat list, and a type test for
    /// the flat list of interface sets does not see it.
    static let asInterfaceSets (sets : FlatList<HashSet<int>>) : seq<IImmutableSet<int>> =
        match box sets with
        | :? seq<IImmutableSet<int>> as widened -> widened
        | _ -> invalidOp "A flat list of hash sets must be a sequence of interface sets"

    /// <summary>
    /// The functions that take a sequence, by name and by the input that they reject, with the parameter that names the
    /// input and the function on it.
    /// </summary>
    /// <remarks>
    /// A default flat list converts to a sequence without a check and fails when the sequence is enumerated, so the other
    /// inputs are valid and the function would enumerate the sequence (ADR 0001, D6).
    /// </remarks>
    static let sequenceRejections : Map<string, struct (string * (unit -> unit))> =
        let set = HashSet.ofArray Inputs.Several :> IImmutableSet<int>
        let nullElements = Unchecked.defaultof<seq<int>>
        let defaultElements = Unchecked.defaultof<FlatList<int>> :> seq<int>
        let nullSets = Unchecked.defaultof<seq<IImmutableSet<int>>>
        let defaultSets = Unchecked.defaultof<FlatList<IImmutableSet<int>>> :> seq<IImmutableSet<int>>

        // The flat lists of strings and of hash sets are sequences of objects and of interface sets
        let widenedElements = FlatListOracles.defaultStringsAsObjects
        let objects = HashSet.ofArray [| ("a" :> obj) |] :> IImmutableSet<obj>
        let widenedSets = asInterfaceSets Unchecked.defaultof<FlatList<HashSet<int>>>

        Map.ofArray [|
            $"%s{nameof ISet.addRange} (null elements)", struct ("elements", (fun () -> ISet.addRange nullElements set |> ignore))
            $"%s{nameof ISet.addRange} (default flat list)",
            struct ("elements", (fun () -> ISet.addRange defaultElements set |> ignore))
            $"%s{nameof ISet.addRange} (default flat list of strings as objects)",
            struct ("elements", (fun () -> ISet.addRange widenedElements objects |> ignore))
            $"%s{nameof ISet.intersectMany} (null sets)", struct ("sets", (fun () -> ISet.intersectMany nullSets |> ignore))
            $"%s{nameof ISet.intersectMany} (default flat list)",
            struct ("sets", (fun () -> ISet.intersectMany defaultSets |> ignore))
            $"%s{nameof ISet.intersectMany} (default flat list of hash sets as interface sets)",
            struct ("sets", (fun () -> ISet.intersectMany widenedSets |> ignore))
            $"%s{nameof ISet.removeRange} (null elements)",
            struct ("elements", (fun () -> ISet.removeRange nullElements set |> ignore))
            $"%s{nameof ISet.removeRange} (default flat list)",
            struct ("elements", (fun () -> ISet.removeRange defaultElements set |> ignore))
            $"%s{nameof ISet.removeRange} (default flat list of strings as objects)",
            struct ("elements", (fun () -> ISet.removeRange widenedElements objects |> ignore))
            $"%s{nameof ISet.unionMany} (null sets)", struct ("sets", (fun () -> ISet.unionMany nullSets |> ignore))
            $"%s{nameof ISet.unionMany} (default flat list)", struct ("sets", (fun () -> ISet.unionMany defaultSets |> ignore))
            $"%s{nameof ISet.unionMany} (default flat list of hash sets as interface sets)",
            struct ("sets", (fun () -> ISet.unionMany widenedSets |> ignore))
        |]

    /// <summary>
    /// The relations of a set with a sequence, by name: the member of the interface, which takes the sequence, and the
    /// function of the module, which takes a set.
    /// </summary>
    static let relationFunctions
        : Map<
              string,
              struct ((IImmutableSet<string> -> seq<string> -> bool) * (IImmutableSet<string> -> IImmutableSet<string> -> bool))
           > =
        Map.ofArray [|
            nameof ISet.isProperSubset, struct ((fun set elements -> set.IsProperSubsetOf elements), ISet.isProperSubset)
            nameof ISet.isProperSuperset, struct ((fun set elements -> set.IsProperSupersetOf elements), ISet.isProperSuperset)
            nameof ISet.isSubset, struct ((fun set elements -> set.IsSubsetOf elements), ISet.isSubset)
            nameof ISet.isSuperset, struct ((fun set elements -> set.IsSupersetOf elements), ISet.isSuperset)
            nameof ISet.overlaps, struct ((fun set elements -> set.Overlaps elements), ISet.overlaps)
            nameof ISet.setEquals, struct ((fun set elements -> set.SetEquals elements), ISet.setEquals)
        |]

    /// <summary>
    /// The operations of the set algebra with a sequence, by name: the member of the interface, which takes the sequence,
    /// and the function of the module, which takes a set.
    /// </summary>
    static let operationFunctions
        : Map<
              string,
              struct ((IImmutableSet<string> -> seq<string> -> IImmutableSet<string>) *
              (IImmutableSet<string> -> IImmutableSet<string> -> IImmutableSet<string>))
           > =
        Map.ofArray [|
            nameof ISet.intersect, struct ((fun set elements -> set.Intersect elements), ISet.intersect)
            nameof ISet.symmetricDifference, struct ((fun set elements -> set.SymmetricExcept elements), ISet.symmetricDifference)
        |]

    /// The relations that need no conversion, by name, as the changelog gives them: each tests the elements with contains.
    static let streamingRecipes : Map<string, IImmutableSet<string> -> seq<string> -> bool> =
        Map.ofArray [|
            nameof ISet.isSuperset, (fun set elements -> Seq.forall (fun item -> ISet.contains item set) elements)
            nameof ISet.overlaps, (fun set elements -> Seq.exists (fun item -> ISet.contains item set) elements)
        |]

    /// The names of the transformations that keep the element type, as rows.
    static member SameTypeTransformations = sameTypeTransformations.Keys

    /// The names of the transformations that keep the comparer, as rows.
    static member ComparerKeepers = comparerKeepers.Keys

    /// The names of the transformations to another element type, as rows.
    static member TypeChanges = typeChanges.Keys

    /// The names of the functions that take a set, with the parameter where a function takes two, as rows.
    static member NullRejections = nullRejections.Keys

    /// The names of the functions that take a sequence, with the input that they reject, as rows.
    static member SequenceRejections = sequenceRejections.Keys

    [<TestMethod; DynamicData(nameof ISetTests.SameTypeTransformations)>]
    member _.``the transformations that keep the element type keep the implementation`` (name : string) =
        let result = sameTypeTransformations[name] (sortedSet Inputs.Several) (sortedSet Inputs.Other)

        let sorted =
            Assert.IsInstanceOfType<ImmutableSortedSet<int>>(result, $"ISet.%s{name} must keep the implementation of its input")

        CollectionAssert.AreEqual (
            Array.sort (Seq.toArray sorted),
            Seq.toArray sorted,
            $"ISet.%s{name} must keep the order of the sorted set"
        )

    [<TestMethod; DynamicData(nameof ISetTests.ComparerKeepers)>]
    member _.``the transformations that keep the element type keep the comparer`` (name : string) =
        Assert.AreSame (
            ignoreCase,
            comparerOf (comparerKeepers[name](words ())) $"ISet.%s{name} must keep the hash set of its input",
            $"ISet.%s{name} must keep the comparer"
        )

    [<TestMethod>]
    member _.``isProperSuperset of a sorted set counts the elements that its comparer finds equal once`` () =
        // The sorted set orders by remainders, so it holds one element of each of the five remainders of its input; the
        // second set holds six elements with three remainders (ADR 0001, D14)
        let sorted =
            ImmutableSortedSet.CreateRange<int>(SetInputs.ByRemainder, Inputs.Other) :> IImmutableSet<int>

        Assert.IsTrue (
            ISet.isProperSuperset sorted (HashSet.ofArray Inputs.Several),
            "ISet.isProperSuperset must count the elements that the comparer of a sorted set finds equal once"
        )

    [<TestMethod>]
    member _.``addRange compares with the comparer of the set`` () =
        // "APPLE" equals "apple" without case, so the set keeps "apple" (ADR 0001, D14)
        CollectionAssert.AreEqual (
            [| "Banana"; "apple"; "cherry"; "date" |],
            Array.sort (ISet.toArray (ISet.addRange [| "APPLE"; "date" |] (words ()))),
            "ISet.addRange must compare with the comparer of the set"
        )

    [<TestMethod>]
    member _.``removeRange compares with the comparer of the set`` () =
        CollectionAssert.AreEqual (
            [| "Banana"; "cherry" |],
            Array.sort (ISet.toArray (ISet.removeRange [| "APPLE"; "date" |] (words ()))),
            "ISet.removeRange must compare with the comparer of the set"
        )

    // The hash set answers an empty input that receives a hash set with the argument or a copy of it, and the interface
    // promises no instance, so the function decides by the count (ADR 0001, D14)
    [<TestMethod>]
    member _.``addRange returns an empty input for an empty hash set`` () =
        let set = (HashSet.forComparer ignoreCase).empty :> IImmutableSet<string>

        Assert.AreSame (set, ISet.addRange HashSet.empty<string> set, "ISet.addRange of an empty hash set must return the input")

    [<TestMethod>]
    member _.``union compares with the comparer of the first set`` () =
        Assert.HasCount (
            4,
            ISet.union (words ()) (HashSet.ofArray [| "APPLE"; "date" |]),
            "ISet.union must compare with the comparer of the first set"
        )

    [<TestMethod; DynamicData(nameof ISetTests.TypeChanges)>]
    member _.``the transformations to another element type return hash sets with the default comparer`` (name : string) =
        Assert.AreSame (
            defaultComparer,
            HashSet.comparer (typeChanges[name](words ())),
            $"ISet.%s{name} must compare with the default comparer"
        )

    [<TestMethod>]
    member _.``map of a sorted set returns a hash set of the results`` () =
        let fromSorted : HashSet<int> = ISet.map id (sortedSet Inputs.Several)

        CollectionAssert.AreEqual (
            Set.toArray (Set.ofArray Inputs.Several),
            HashSet.toArray fromSorted |> Array.sort,
            "ISet.map of a sorted set must return a hash set of the results"
        )

    [<TestMethod>]
    member _.``unionMany of no sets is an empty hash set`` () =
        let union = ISet.unionMany Seq.empty<IImmutableSet<int>>
        let hashSet =
            Assert.IsInstanceOfType<ImmutableHashSet<int>>(union, "ISet.unionMany of no sets must return a hash set")
        Assert.IsEmpty (hashSet, "ISet.unionMany of no sets must be empty")

    [<TestMethod>]
    member _.``tryGetValue returns the element that the set holds`` () =
        Assert.AreEqual<string voption>(
            ValueSome "apple",
            ISet.tryGetValue "APPLE" (words ()),
            "ISet.tryGetValue must compare without case"
        )
        Assert.AreEqual<string voption>(
            ValueNone,
            ISet.tryGetValue "date" (words ()),
            "ISet.tryGetValue must return nothing for a missing value"
        )
        Assert.AreEqual<int voption>(
            ValueSome 5,
            ISet.tryGetValue 5 (sortedSet Inputs.Several),
            "ISet.tryGetValue must search a sorted set"
        )

    [<TestMethod; DynamicData(nameof ISetTests.NullRejections)>]
    member _.``functions reject a null set`` (name : string) =
        let struct (paramName, actual) = nullRejections[name]

        let ex =
            Assert.ThrowsExactly<ArgumentNullException>(Action actual, $"ISet.%s{name} must reject a null set")

        Assert.AreEqual<string | null>(paramName, ex.ParamName, $"ISet.%s{name} must name the parameter of the set")

    // The way to pass a sequence to a relation, as the changelog gives it: the interface cannot tell the comparer of the
    // first set, so the sequence is added to the empty set that clear returns, which keeps the implementation and the
    // comparer. The conversion with the default comparer gives the same answer when the comparer of the first set finds equal
    // what the default comparer finds equal, and another answer when it tells such strings apart
    [<TestMethod; DynamicData(nameof SetRelationInputs.Rows, typeof<SetRelationInputs>)>]
    member _.``a relation with a sequence takes the sequence that is added to the empty set of the first set``
        (relation : string, comparer : string)
        =
        let instances = SetRelationInputs.Strings [| "a1"; "a2" |]
        Assert.AreNotSame (
            instances[0],
            instances[1],
            "The test needs two strings with the same text that are different instances"
        )

        let struct (setItems, sequenceItems) = SetRelationInputs.Items[relation]
        let struct (interfaceMember, moduleFunction) = relationFunctions[relation]
        let struct (firstComparer, defaultFindsEqualWhatItFindsEqual) = SetRelationInputs.Comparers[comparer]

        let set =
            (HashSet.forComparer firstComparer).ofArray(SetRelationInputs.Strings setItems) :> IImmutableSet<string>

        let elements = SetRelationInputs.Strings sequenceItems
        let expected = interfaceMember set elements

        Assert.AreEqual<bool>(
            expected,
            moduleFunction set (ISet.addRange elements (ISet.clear set)),
            $"ISet.%s{relation} must answer as the member of the interface for the sequence that is added to the empty set of the first set"
        )

        let withDefaultComparer = moduleFunction set (HashSet.ofSeq elements)

        if defaultFindsEqualWhatItFindsEqual then
            Assert.AreEqual<bool>(
                expected,
                withDefaultComparer,
                $"ISet.%s{relation} must answer as the member of the interface for the sequence that the default comparer converts, because the comparer of the first set finds equal what the default comparer finds equal"
            )
        else
            Assert.AreNotEqual<bool>(
                expected,
                withDefaultComparer,
                $"ISet.%s{relation} must answer differently for the sequence that the default comparer converts, because the comparer of the first set tells apart strings that the default comparer finds equal"
            )

    // The same recipe for the operations of the set algebra that take a sequence in the interface
    [<TestMethod; DynamicData(nameof SetRelationInputs.OperationRows, typeof<SetRelationInputs>)>]
    member _.``an operation with a sequence takes the sequence that is added to the empty set of the first set``
        (operation : string, comparer : string)
        =
        let struct (setItems, sequenceItems) = SetRelationInputs.OperationItems[operation]
        let struct (interfaceMember, moduleFunction) = operationFunctions[operation]
        let struct (firstComparer, defaultFindsEqualWhatItFindsEqual) = SetRelationInputs.Comparers[comparer]

        let set =
            (HashSet.forComparer firstComparer).ofArray(SetRelationInputs.Strings setItems) :> IImmutableSet<string>

        let elements = SetRelationInputs.Strings sequenceItems
        let expected = ISet.toArray (interfaceMember set elements)

        CollectionAssert.AreEquivalent (
            expected,
            ISet.toArray (moduleFunction set (ISet.addRange elements (ISet.clear set))),
            $"ISet.%s{operation} must return the result of the member of the interface for the sequence that is added to the empty set of the first set"
        )

        let withDefaultComparer = ISet.toArray (moduleFunction set (HashSet.ofSeq elements))

        if defaultFindsEqualWhatItFindsEqual then
            CollectionAssert.AreEquivalent (
                expected,
                withDefaultComparer,
                $"ISet.%s{operation} must return the result of the member of the interface for the sequence that the default comparer converts, because the comparer of the first set finds equal what the default comparer finds equal"
            )
        else
            CollectionAssert.AreNotEquivalent (
                expected,
                withDefaultComparer,
                $"ISet.%s{operation} must return another result for the sequence that the default comparer converts, because the comparer of the first set tells apart strings that the default comparer finds equal"
            )

    [<TestMethod; DynamicData(nameof SetRelationInputs.StreamingRows, typeof<SetRelationInputs>)>]
    member _.``overlaps and isSuperset with a sequence test each element with contains`` (relation : string, comparer : string) =
        let struct (setItems, sequenceItems) = SetRelationInputs.Items[relation]
        let struct (interfaceMember, _) = relationFunctions[relation]
        let struct (firstComparer, _) = SetRelationInputs.Comparers[comparer]

        let set =
            (HashSet.forComparer firstComparer).ofArray(SetRelationInputs.Strings setItems) :> IImmutableSet<string>

        let elements = SetRelationInputs.Strings sequenceItems

        Assert.AreEqual<bool>(
            interfaceMember set elements,
            streamingRecipes[relation] set elements,
            $"%s{relation} with a sequence must test each element with ISet.contains, in the comparer of the first set"
        )

    [<TestMethod; DynamicData(nameof ISetTests.SequenceRejections)>]
    member _.``functions that take a sequence reject a null sequence and a default flat list given as the sequence``
        (name : string)
        =
        let struct (paramName, actual) = sequenceRejections[name]
        FlatListAssert.RejectsNull ($"ISet.%s{name}", paramName, actual)

    [<TestMethod>]
    member _.``a flat list that is not default passes as a sequence of objects and as a sequence of interface sets`` () =
        let widened = FlatListOracles.stringsAsObjects (FlatList.ofArray [| "a"; "b" |])
        let objects = HashSet.ofArray [| ("b" :> obj); ("c" :> obj) |] :> IImmutableSet<obj>

        let sets =
            asInterfaceSets (FlatList.ofArray [| HashSet.ofArray [| 1; 2 |]; HashSet.ofArray [| 2; 3 |] |])

        CollectionAssert.AreEquivalent (
            [| ("a" :> obj); ("b" :> obj); ("c" :> obj) |],
            ISet.toArray (ISet.addRange widened objects),
            "ISet.addRange must take the elements of a flat list of strings as objects"
        )

        CollectionAssert.AreEquivalent (
            [| ("c" :> obj) |],
            ISet.toArray (ISet.removeRange widened objects),
            "ISet.removeRange must take the elements of a flat list of strings as objects"
        )

        CollectionAssert.AreEquivalent (
            [| 1; 2; 3 |],
            ISet.toArray (ISet.unionMany sets),
            "ISet.unionMany must take the sets of a flat list of hash sets as interface sets"
        )

        CollectionAssert.AreEquivalent (
            [| 2 |],
            ISet.toArray (ISet.intersectMany sets),
            "ISet.intersectMany must take the sets of a flat list of hash sets as interface sets"
        )
