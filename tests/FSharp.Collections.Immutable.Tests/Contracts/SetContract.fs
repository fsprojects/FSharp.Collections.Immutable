namespace FSharp.Collections.Immutable.Tests

open System
open FSharp.Collections.Immutable
open Microsoft.VisualStudio.TestTools.UnitTesting

/// <summary>
/// The helpers that the contract bases of the set modules share.
/// </summary>
/// <remarks>
/// <para>
/// Every set module mirrors one FSharp.Core module, <c>Set</c> (ADR 0001, D6). The base therefore compares the exceptions
/// with the <c>Set</c> oracle through <c>ExceptionAssert.Same</c> and needs no case of <c>Mirror</c>, which keeps the
/// contracts of the sequence-like modules unchanged.
/// </para>
/// <para>
/// The order in which a hash set enumerates its elements is unspecified, so the contracts compare the elements of a set
/// as a sorted array, the order of an F# set.
/// </para>
/// </remarks>
/// <param name="name">The name of the module under test, for the messages of the assertions.</param>
/// <param name="paramName">Maps the name of a parameter of the FSharp.Core oracle to the name of the parameter of the module.</param>
/// <param name="ofSeq">Creates a set of the given elements.</param>
/// <param name="toArray">The elements in the order in which the set enumerates them.</param>
[<AbstractClass>]
type SetContractBase<'S> (name : string, paramName : string -> string, ofSeq : seq<int> -> 'S, toArray : 'S -> int array) =

    /// The elements of a set, sorted.
    member _.Sorted (set : 'S) = Array.sort (toArray set)

    /// The set of the elements of an input and the F# set of the same elements, the oracle.
    member _.Create (input : int array) = struct (ofSeq input, Set.ofArray input)

    /// Asserts that a function of the module throws what the same function of the FSharp.Core `Set` module throws.
    member _.ThrowsLikeSet (oracle : unit -> 'O, actual : unit -> 'R, message : string) =
        ExceptionAssert.Same (paramName, oracle >> ignore, actual >> ignore, $"%s{name}: %s{message}")

    /// <summary>
    /// Asserts that a function of the module throws <see cref="T:System.ArgumentNullException"/> for a null input that the
    /// FSharp.Core <c>Set</c> function dereferences without a check.
    /// </summary>
    /// <param name="setParamName">The name of the parameter in the signature of the FSharp.Core function.</param>
    /// <param name="actual">The call of the function of the module.</param>
    /// <param name="message">The message of the assertion.</param>
    member _.ThrowsArgumentNull (setParamName : string, actual : unit -> 'R, message : string) =
        let ex =
            Assert.ThrowsExactly<ArgumentNullException>(Action (actual >> ignore), $"%s{name}: %s{message}")

        Assert.AreEqual<string | null>(paramName setParamName, ex.ParamName, $"%s{name}: %s{message} names the parameter")

/// <summary>
/// The contract of the reading functions that every set module provides: <c>HashSet</c>, <c>SortedSet</c>, <c>ISet</c> and
/// <c>FrozenSet</c> (ADR 0001, Appendix C).
/// </summary>
[<AbstractClass>]
type SetContract<'S> (ops : SetOps<'S>) =
    inherit SetContractBase<'S> (ops.Name, ops.ParamName, ops.OfSeq, ops.ToArray)

    /// A folder whose state records the order in which it saw the elements.
    static let record (visited : int array) (item : int) = Array.append visited [| item |]

    /// A folder from the end whose state records the order in which it saw the elements.
    static let recordBack (item : int) (visited : int array) = Array.append visited [| item |]

    /// <summary>
    /// The sets of a pair of inputs for the relations under the comparer of the first set.
    /// </summary>
    /// <remarks>
    /// The first set compares by remainders and the second one with the default comparer, so that elements that the second
    /// set tells apart can be one element for the first set (ADR 0001, D14).
    /// </remarks>
    let createByRemainder (first : int array) (second : int array) = struct (ops.OfSeqByRemainder first, ops.OfSeq second)

    /// <summary>
    /// Every input with each predicate of exists and forall, as rows: one that some elements satisfy, one that few or no
    /// elements satisfy, one that no element satisfies and one that every element satisfies.
    /// </summary>
    static member InputsAndPredicates = seq {
        for input in SetInputs.All do
            for predicate in
                [|
                    ElementPredicate.IsEven
                    ElementPredicate.IsNegative
                    ElementPredicate.Never
                    ElementPredicate.IsBelowHundred
                |] do
                struct (input, predicate)
    }

    // ---- Properties ----

    [<TestMethod; DynamicData(nameof SetInputs.All, typeof<SetInputs>)>]
    member this.``count counts the distinct elements`` (input : int array) =
        let struct (set, expected) = this.Create input
        Assert.AreEqual<int>(Set.count expected, ops.Count set, $"%s{ops.Name}.count must count the distinct elements")

    [<TestMethod; DynamicData(nameof SetInputs.All, typeof<SetInputs>)>]
    member this.``isEmpty is true only for a set without elements`` (input : int array) =
        let struct (set, expected) = this.Create input

        Assert.AreEqual<bool>(
            Set.isEmpty expected,
            ops.IsEmpty set,
            $"%s{ops.Name}.isEmpty must be true only for a set without elements"
        )

    // ---- Access ----

    [<TestMethod; DynamicData(nameof SetInputs.InputsAndValues, typeof<SetInputs>)>]
    member this.``tryGetValue returns the element equal to the value or nothing`` (input : int array, value : int) =
        let struct (set, expected) = this.Create input

        Assert.AreEqual<int voption>(
            (if Set.contains value expected then
                 ValueSome value
             else
                 ValueNone),
            ops.TryGetValue value set,
            $"%s{ops.Name}.tryGetValue must return the element equal to the value or nothing"
        )

    // ---- Search ----

    [<TestMethod; DynamicData(nameof SetInputs.InputsAndValues, typeof<SetInputs>)>]
    member this.``contains finds an element equal to the value`` (input : int array, value : int) =
        let struct (set, expected) = this.Create input

        Assert.AreEqual<bool>(
            Set.contains value expected,
            ops.Contains value set,
            $"%s{ops.Name}.contains must find an element equal to the value"
        )

    [<TestMethod; DynamicData(nameof SetContract<'S>.InputsAndPredicates)>]
    member this.``exists finds an element that satisfies the predicate`` (input : int array, predicate : ElementPredicate) =
        let struct (set, expected) = this.Create input

        Assert.AreEqual<bool>(
            Set.exists predicate.Matches expected,
            ops.Exists predicate.Matches set,
            $"%s{ops.Name}.exists must find an element that satisfies the predicate"
        )

    [<TestMethod>]
    member _.``exists stops at the first element that satisfies the predicate`` () =
        let set = ops.OfSeq Inputs.Several
        let mutable calls = 0

        let found =
            ops.Exists
                (fun _ ->
                    calls <- calls + 1
                    true
                )
                set

        Assert.IsTrue (found, $"%s{ops.Name}.exists must find an element")
        Assert.AreEqual<int>(1, calls, $"%s{ops.Name}.exists must stop at the first element that satisfies the predicate")

    [<TestMethod; DynamicData(nameof SetContract<'S>.InputsAndPredicates)>]
    member this.``forall tests every element`` (input : int array, predicate : ElementPredicate) =
        let struct (set, expected) = this.Create input

        Assert.AreEqual<bool>(
            Set.forall predicate.Matches expected,
            ops.Forall predicate.Matches set,
            $"%s{ops.Name}.forall must test every element"
        )

    [<TestMethod>]
    member _.``forall stops at the first element that does not satisfy the predicate`` () =
        let set = ops.OfSeq Inputs.Several
        let mutable calls = 0

        let satisfied =
            ops.Forall
                (fun _ ->
                    calls <- calls + 1
                    false
                )
                set

        Assert.IsFalse (satisfied, $"%s{ops.Name}.forall must find an element that does not satisfy the predicate")
        Assert.AreEqual<int>(1, calls, $"%s{ops.Name}.forall must stop at the first element that does not satisfy the predicate")

    [<TestMethod; DynamicData(nameof SetInputs.Pairs, typeof<SetInputs>)>]
    member this.``isProperSubset tests whether the first set is a proper subset of the second one``
        (first : int array, second : int array)
        =
        let struct (set1, expected1) = this.Create first
        let struct (set2, expected2) = this.Create second

        Assert.AreEqual<bool>(
            Set.isProperSubset expected1 expected2,
            ops.IsProperSubset set1 set2,
            $"%s{ops.Name}.isProperSubset must test whether the first set is a proper subset of the second one"
        )

    [<TestMethod; DynamicData(nameof SetInputs.Pairs, typeof<SetInputs>)>]
    member this.``isProperSuperset tests whether the first set is a proper superset of the second one``
        (first : int array, second : int array)
        =
        let struct (set1, expected1) = this.Create first
        let struct (set2, expected2) = this.Create second

        Assert.AreEqual<bool>(
            Set.isProperSuperset expected1 expected2,
            ops.IsProperSuperset set1 set2,
            $"%s{ops.Name}.isProperSuperset must test whether the first set is a proper superset of the second one"
        )

    [<TestMethod; DynamicData(nameof SetInputs.Pairs, typeof<SetInputs>)>]
    member this.``isSubset tests whether the first set is a subset of the second one`` (first : int array, second : int array) =
        let struct (set1, expected1) = this.Create first
        let struct (set2, expected2) = this.Create second

        Assert.AreEqual<bool>(
            Set.isSubset expected1 expected2,
            ops.IsSubset set1 set2,
            $"%s{ops.Name}.isSubset must test whether the first set is a subset of the second one"
        )

    [<TestMethod; DynamicData(nameof SetInputs.Pairs, typeof<SetInputs>)>]
    member this.``isSuperset tests whether the first set is a superset of the second one``
        (first : int array, second : int array)
        =
        let struct (set1, expected1) = this.Create first
        let struct (set2, expected2) = this.Create second

        Assert.AreEqual<bool>(
            Set.isSuperset expected1 expected2,
            ops.IsSuperset set1 set2,
            $"%s{ops.Name}.isSuperset must test whether the first set is a superset of the second one"
        )

    [<TestMethod; DynamicData(nameof SetInputs.Pairs, typeof<SetInputs>)>]
    member this.``overlaps tests whether the sets have an element in common`` (first : int array, second : int array) =
        let struct (set1, expected1) = this.Create first
        let struct (set2, expected2) = this.Create second

        Assert.AreEqual<bool>(
            not (Set.isEmpty (Set.intersect expected1 expected2)),
            ops.Overlaps set1 set2,
            $"%s{ops.Name}.overlaps must test whether the sets have an element in common"
        )

    [<TestMethod; DynamicData(nameof SetInputs.Pairs, typeof<SetInputs>)>]
    member this.``setEquals tests whether the sets have the same elements`` (first : int array, second : int array) =
        let struct (set1, expected1) = this.Create first
        let struct (set2, expected2) = this.Create second

        Assert.AreEqual<bool>(
            (expected1 = expected2),
            ops.SetEquals set1 set2,
            $"%s{ops.Name}.setEquals must test whether the sets have the same elements"
        )

    [<TestMethod; DynamicData(nameof SetInputs.Pairs, typeof<SetInputs>)>]
    member _.``isProperSubset compares in the comparer of the first set`` (first : int array, second : int array) =
        let struct (set1, set2) = createByRemainder first second

        Assert.AreEqual<bool>(
            Set.isProperSubset (SetInputs.Remainders first) (SetInputs.Remainders second),
            ops.IsProperSubset set1 set2,
            $"%s{ops.Name}.isProperSubset must count the elements that the comparer of the first set finds equal once"
        )

    [<TestMethod; DynamicData(nameof SetInputs.Pairs, typeof<SetInputs>)>]
    member _.``isProperSuperset compares in the comparer of the first set`` (first : int array, second : int array) =
        let struct (set1, set2) = createByRemainder first second

        Assert.AreEqual<bool>(
            Set.isProperSuperset (SetInputs.Remainders first) (SetInputs.Remainders second),
            ops.IsProperSuperset set1 set2,
            $"%s{ops.Name}.isProperSuperset must count the elements that the comparer of the first set finds equal once"
        )

    [<TestMethod; DynamicData(nameof SetInputs.Pairs, typeof<SetInputs>)>]
    member _.``isSubset compares in the comparer of the first set`` (first : int array, second : int array) =
        let struct (set1, set2) = createByRemainder first second

        Assert.AreEqual<bool>(
            Set.isSubset (SetInputs.Remainders first) (SetInputs.Remainders second),
            ops.IsSubset set1 set2,
            $"%s{ops.Name}.isSubset must count the elements that the comparer of the first set finds equal once"
        )

    [<TestMethod; DynamicData(nameof SetInputs.Pairs, typeof<SetInputs>)>]
    member _.``isSuperset compares in the comparer of the first set`` (first : int array, second : int array) =
        let struct (set1, set2) = createByRemainder first second

        Assert.AreEqual<bool>(
            Set.isSuperset (SetInputs.Remainders first) (SetInputs.Remainders second),
            ops.IsSuperset set1 set2,
            $"%s{ops.Name}.isSuperset must count the elements that the comparer of the first set finds equal once"
        )

    [<TestMethod; DynamicData(nameof SetInputs.Pairs, typeof<SetInputs>)>]
    member _.``overlaps compares in the comparer of the first set`` (first : int array, second : int array) =
        let struct (set1, set2) = createByRemainder first second

        Assert.AreEqual<bool>(
            not (Set.isEmpty (Set.intersect (SetInputs.Remainders first) (SetInputs.Remainders second))),
            ops.Overlaps set1 set2,
            $"%s{ops.Name}.overlaps must compare in the comparer of the first set"
        )

    [<TestMethod; DynamicData(nameof SetInputs.Pairs, typeof<SetInputs>)>]
    member _.``setEquals compares in the comparer of the first set`` (first : int array, second : int array) =
        let struct (set1, set2) = createByRemainder first second

        Assert.AreEqual<bool>(
            (SetInputs.Remainders first = SetInputs.Remainders second),
            ops.SetEquals set1 set2,
            $"%s{ops.Name}.setEquals must count the elements that the comparer of the first set finds equal once"
        )

    // ---- Folding ----

    [<TestMethod; DynamicData(nameof SetInputs.All, typeof<SetInputs>)>]
    member this.``fold visits every element once in the order of enumeration`` (input : int array) =
        let struct (set, expected) = this.Create input
        let visited = ops.Fold record [||] set

        CollectionAssert.AreEqual (
            Array.fold record [||] (ops.ToArray set),
            visited,
            $"%s{ops.Name}.fold must visit the elements in the order of enumeration"
        )

        CollectionAssert.AreEqual (Set.toArray expected, Array.sort visited, $"%s{ops.Name}.fold must visit every element once")

    [<TestMethod; DynamicData(nameof SetInputs.All, typeof<SetInputs>)>]
    member this.``foldBack visits every element once in the reverse order of enumeration`` (input : int array) =
        let struct (set, expected) = this.Create input
        let visited = ops.FoldBack recordBack set [||]

        CollectionAssert.AreEqual (
            Array.foldBack recordBack (ops.ToArray set) [||],
            visited,
            $"%s{ops.Name}.foldBack must visit the elements in the reverse order of enumeration"
        )

        CollectionAssert.AreEqual (
            Set.toArray expected,
            Array.sort visited,
            $"%s{ops.Name}.foldBack must visit every element once"
        )

    [<TestMethod; DynamicData(nameof SetInputs.All, typeof<SetInputs>)>]
    member this.``iter visits every element once in the order of enumeration`` (input : int array) =
        let struct (set, _) = this.Create input
        let visited = ResizeArray<int>()
        ops.Iter visited.Add set

        CollectionAssert.AreEqual (
            ops.ToArray set,
            visited.ToArray (),
            $"%s{ops.Name}.iter must visit every element once in the order of enumeration"
        )

    // ---- Conversion ----

    [<TestMethod; DynamicData(nameof SetInputs.All, typeof<SetInputs>)>]
    member this.``toArray returns every element of the input once`` (input : int array) =
        // The other tests observe a set with toArray, so toArray is checked against the sequence that the set was created
        // from, whose distinct elements a set holds
        CollectionAssert.AreEqual (
            Set.toArray (Set.ofArray input),
            this.Sorted (ops.OfSeq input),
            $"%s{ops.Name}.toArray must return every distinct element of the sequence that the set was created from once"
        )

    [<TestMethod>]
    member this.``toArray returns a new array`` () =
        let set = ops.OfSeq Inputs.Several
        let array = ops.ToArray set
        array[0] <- 1000

        CollectionAssert.AreEqual (
            Set.toArray (Set.ofArray Inputs.Several),
            this.Sorted set,
            $"%s{ops.Name}.toArray must return a copy that does not change the set"
        )

    [<TestMethod; DynamicData(nameof SetInputs.All, typeof<SetInputs>)>]
    member this.``toList returns the elements in the order of enumeration`` (input : int array) =
        let struct (set, _) = this.Create input

        CollectionAssert.AreEqual (
            ops.ToArray set,
            List.toArray (ops.ToList set),
            $"%s{ops.Name}.toList must return the elements in the order in which the set enumerates them"
        )

    [<TestMethod; DynamicData(nameof SetInputs.All, typeof<SetInputs>)>]
    member this.``toSeq returns the elements in the order of enumeration`` (input : int array) =
        let struct (set, _) = this.Create input

        CollectionAssert.AreEqual (
            ops.ToArray set,
            Seq.toArray (ops.ToSeq set),
            $"%s{ops.Name}.toSeq must return the elements in the order in which the set enumerates them"
        )
