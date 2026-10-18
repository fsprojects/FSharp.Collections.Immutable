namespace FSharp.Collections.Immutable.Tests

open System
open System.Collections.Generic
open System.Collections.Immutable
open Microsoft.VisualStudio.TestTools.UnitTesting
open FSharp.Collections.Immutable

[<TestClass; EquatableArrayTestCategory; ModuleFunctionsTestCategory>]
type EquatableArrayModuleTests () =

    [<TestMethod>]
    member _.``empty has no elements and equals the default value`` () =
        let empty = EquatableArray.empty<int>
        Assert.IsTrue (empty.IsEmpty, "empty must have no elements")
        Assert.IsFalse (empty.Items.IsDefault, "the elements of empty must not be a default ImmutableArray")
        Assert.AreEqual (Unchecked.defaultof<EquatableArray<int>>, empty, "empty must equal the default value")

    [<TestMethod>]
    member _.``ofArray copies the elements`` () =
        let source = [| 1; 2; 3 |]
        let array = EquatableArray.ofArray source
        source[0] <- 9
        Assert.AreEqual (ea [ 1; 2; 3 ], array, "a later change of the source must not change the result")
        Assert.AreNotSame<int array | null>(source, underlying array.Items, "ofArray must not share the source")

    [<TestMethod>]
    member _.``ofArray rejects null`` () =
        let ex =
            Assert.ThrowsExactly<ArgumentNullException>(
                (fun () ->
                    EquatableArray.ofArray Unchecked.defaultof<int array>
                    |> ignore
                ),
                "ofArray must reject null as Array.copy does"
            )

        Assert.AreEqual<string | null>("array", ex.ParamName, "the exception must name the parameter")

    [<TestMethod>]
    member _.``ofImmutableArray shares the array and turns a default array into the empty array`` () =
        let items = ImmutableArray.Create (1, 2, 3)
        Assert.AreSame<int array | null>(
            underlying items,
            underlying (EquatableArray.ofImmutableArray items).Items,
            "ofImmutableArray must not copy"
        )

        let fromDefault = EquatableArray.ofImmutableArray Unchecked.defaultof<ImmutableArray<int>>
        Assert.IsTrue (fromDefault.IsEmpty, "a default ImmutableArray must give the empty array")
        Assert.IsFalse (fromDefault.Items.IsDefault, "the elements must never be a default ImmutableArray")

    [<TestMethod>]
    member _.``ofList keeps the order of the elements`` () =
        Assert.AreEqual (EquatableArray.ofArray [| 3; 1; 2 |], EquatableArray.ofList [ 3; 1; 2 ], "ofList must keep the order")
        Assert.IsTrue ((EquatableArray.ofList List.empty<int>).IsEmpty, "an empty list must give the empty array")

    [<TestMethod>]
    member _.``ofSeq copies a list or an array`` () =
        let source = [| 1; 2; 3 |]
        let array = EquatableArray.ofSeq source
        source[0] <- 9
        Assert.AreEqual (ea [ 1; 2; 3 ], array, "a later change of the source array must not change the result")
        Assert.AreEqual (ea [ 1; 2 ], EquatableArray.ofSeq (List<int> [ 1; 2 ]), "ofSeq must copy a List")
        Assert.AreEqual (ea [ 0; 1; 2 ], EquatableArray.ofSeq (Seq.init 3 id), "ofSeq must read a lazy sequence")

    [<TestMethod>]
    member _.``ofSeq shares an ImmutableArray and rejects a default one`` () =
        let items = ImmutableArray.Create (1, 2)
        Assert.AreSame<int array | null>(
            underlying items,
            underlying (EquatableArray.ofSeq items).Items,
            "ofSeq must not copy an ImmutableArray, which cannot change"
        )

        Assert.ThrowsExactly<InvalidOperationException>(
            (fun () ->
                EquatableArray.ofSeq Unchecked.defaultof<ImmutableArray<int>>
                |> ignore
            ),
            "ofSeq must reject a default ImmutableArray, which cannot be enumerated"
        )
        |> ignore

    [<TestMethod>]
    member _.``ofSeq rejects null`` () =
        let ex =
            Assert.ThrowsExactly<ArgumentNullException>(
                (fun () -> EquatableArray.ofSeq Unchecked.defaultof<int seq> |> ignore),
                "ofSeq must reject null as Array.ofSeq does"
            )

        Assert.AreEqual<string | null>("source", ex.ParamName, "the exception must name the parameter")

    [<TestMethod>]
    member _.``singleton holds one element`` () =
        let array = EquatableArray.singleton "a"
        Assert.AreEqual (1, array.Length, "singleton must hold one element")
        Assert.AreEqual ("a", array[0], "singleton must hold the given element")

    [<TestMethod>]
    member _.``unsafeOfArray shares the array`` () =
        let source = [| 1; 2; 3 |]
        let array = EquatableArray.unsafeOfArray source
        Assert.AreSame<int array | null>(source, underlying array.Items, "unsafeOfArray must not copy")
        source[0] <- 9
        Assert.AreEqual (9, array[0], "a later write to the source shows in the result, as the documentation warns")

    [<TestMethod>]
    member _.``unsafeOfArray works as a first-class function`` () =
        let arrays =
            [ [| 1 |]; [| 2; 3 |] ]
            |> List.map EquatableArray.unsafeOfArray
        Assert.AreEqual<EquatableArray<int> list>([ ea [ 1 ]; ea [ 2; 3 ] ], arrays, "unsafeOfArray must work in List.map")

    [<TestMethod>]
    member _.``unsafeOfArray rejects null`` () =
        let ex =
            Assert.ThrowsExactly<ArgumentNullException>(
                (fun () ->
                    EquatableArray.unsafeOfArray Unchecked.defaultof<int array>
                    |> ignore
                ),
                "unsafeOfArray must reject null instead of giving the empty array"
            )

        Assert.AreEqual<string | null>("array", ex.ParamName, "the exception must name the parameter")

    [<TestMethod>]
    member _.``isEmpty is true only without elements`` () =
        Assert.IsTrue (EquatableArray.isEmpty EquatableArray.empty<int>, "the empty array must be empty")
        Assert.IsTrue (EquatableArray.isEmpty Unchecked.defaultof<EquatableArray<int>>, "the default value must be empty")
        Assert.IsFalse (EquatableArray.isEmpty (ea [ 1 ]), "an array with an element must not be empty")

    [<TestMethod>]
    member _.``length counts the elements`` () =
        Assert.AreEqual (0, EquatableArray.length Unchecked.defaultof<EquatableArray<int>>, "the default value has no elements")
        Assert.AreEqual (3, EquatableArray.length (ea [ 1; 2; 3 ]), "length must count every element")

    [<TestMethod>]
    member _.``map applies the function to each element in order`` () =
        let calls = List<int>()

        let mapped =
            ea [ 1; 2; 3 ]
            |> EquatableArray.map (fun item ->
                calls.Add item
                string (item * 10)
            )

        Assert.AreEqual (ea [ "10"; "20"; "30" ], mapped, "map must apply the function to each element")
        CollectionAssert.AreEqual ([| 1; 2; 3 |], calls.ToArray (), "map must call the function once per element, in order")
        Assert.IsTrue (
            (EquatableArray.map string Unchecked.defaultof<EquatableArray<int>>).IsEmpty,
            "map of the default value must be empty"
        )

    [<TestMethod>]
    member _.``toImmutableArray shares the array and never gives a default array`` () =
        let items = ImmutableArray.Create (1, 2)
        Assert.AreSame<int array | null>(
            underlying items,
            underlying (EquatableArray.toImmutableArray (EquatableArray items)),
            "toImmutableArray must not copy"
        )

        let fromDefault = EquatableArray.toImmutableArray Unchecked.defaultof<EquatableArray<int>>
        Assert.IsFalse (fromDefault.IsDefault, "the default value must give an empty, not a default, ImmutableArray")
        Assert.IsTrue (fromDefault.IsEmpty, "the default value must give an empty ImmutableArray")
