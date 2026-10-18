namespace FSharp.Collections.Immutable.Tests

open System
open System.Collections.Immutable
open System.Text.Json
open Microsoft.VisualStudio.TestTools.UnitTesting
open FSharp.Collections.Immutable

/// Decision 2 of ADR 0002: the default value is the empty array, and the default state of ImmutableArray is never
/// observable. Unchecked.defaultof, Array.zeroCreate and a missing JSON member produce the default value.
[<TestClass; EquatableArrayTestCategory; DefaultValueTestCategory>]
type EquatableArrayDefaultValueTests () =

    static let defaultValue = Unchecked.defaultof<EquatableArray<int>>

    [<TestMethod>]
    member _.``the default value equals the empty array and has its hash code`` () =
        Assert.AreEqual (EquatableArray.empty<int>, defaultValue, "the default value must equal the empty array")
        Assert.IsTrue ((defaultValue = EquatableArray.empty), "F# equality must hold too")
        Assert.AreEqual (hash EquatableArray.empty<int>, hash defaultValue, "equal values must have equal hash codes")
        Assert.AreEqual (0, compare defaultValue EquatableArray.empty, "the default value must compare equal to the empty array")

    [<TestMethod>]
    member _.``the default value has no elements`` () =
        Assert.AreEqual (0, defaultValue.Length, "the default value must have length 0")
        Assert.IsTrue (defaultValue.IsEmpty, "the default value must be empty")
        Assert.IsFalse (defaultValue.Items.IsDefault, "Items must never be a default ImmutableArray")
        Assert.IsEmpty (defaultValue, "the default value must enumerate no elements")

    [<TestMethod>]
    member _.``a for loop over the default value runs zero times`` () =
        let mutable count = 0

        for _ in defaultValue do
            count <- count + 1

        Assert.AreEqual (0, count, "a for loop must not throw and must not run")

    [<TestMethod>]
    member _.``an element of Array.zeroCreate behaves as the empty array`` () =
        let element = (Array.zeroCreate<EquatableArray<string>> 1)[0]
        Assert.IsTrue (element.IsEmpty, "a zero-initialized element must be empty")
        Assert.AreEqual (EquatableArray.empty<string>, element, "a zero-initialized element must equal the empty array")

    [<TestMethod>]
    member _.``the indexer of the default value throws IndexOutOfRangeException`` () =
        Assert.ThrowsExactly<IndexOutOfRangeException>(
            (fun () -> defaultValue[0] |> ignore),
            "the indexer must throw as for any empty array, not NullReferenceException"
        )
        |> ignore

    [<TestMethod>]
    member _.``slices of the default value are empty`` () =
        Assert.IsTrue (defaultValue[0..5].IsEmpty, "an F# slice of the default value must be empty")
        Assert.IsTrue ((defaultValue.Slice (0, 0)).IsEmpty, "an empty Slice of the default value must be empty")

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            (fun () -> defaultValue.Slice (0, 1) |> ignore),
            "Slice beyond the length must throw as for any empty array"
        )
        |> ignore

    [<TestMethod>]
    member _.``conversions of the default value give an empty ImmutableArray`` () =
        let converted : ImmutableArray<int> = EquatableArray<int>.op_Implicit defaultValue
        Assert.IsFalse (converted.IsDefault, "the implicit conversion must not give a default ImmutableArray")
        Assert.IsFalse (
            (EquatableArray.toImmutableArray defaultValue).IsDefault,
            "toImmutableArray must not give a default ImmutableArray"
        )
        Assert.AreEqual (0, defaultValue.AsSpan().Length, "the span of the default value must be empty")

    [<TestMethod>]
    member _.``the default value formats as an empty F# array`` () =
        Assert.AreEqual ("[||]", $"%A{defaultValue}", "%A must show the default value as [||]")
        Assert.AreEqual ("[||]", defaultValue.ToString (), "ToString must show the default value as [||]")

    [<TestMethod>]
    member _.``the default value serializes as an empty JSON array`` () =
        Assert.AreEqual ("[]", JsonSerializer.Serialize defaultValue, "the default value must be written as []")

    [<TestMethod>]
    member _.``the default value matches Arr0`` () =
        let matched =
            match defaultValue with
            | Arr0 -> true
            | _ -> false

        Assert.IsTrue (matched, "the default value must match the pattern of the empty array")
