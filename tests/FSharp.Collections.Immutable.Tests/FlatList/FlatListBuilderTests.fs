namespace FSharp.Collections.Immutable.Tests

open System
open System.Collections.Generic
open FSharp.Collections.Immutable
open Microsoft.VisualStudio.TestTools.UnitTesting

/// The builder functions of `FlatList` and the `FlatListBuilder` module.
[<TestClass; FlatListTestCategory; BuilderTestCategory>]
type FlatListBuilderTests () =

    /// A builder that holds the elements of the input with several elements.
    static let filled () =
        let builder = FlatList.builder<int>()
        builder.AddRange Inputs.several
        builder

    [<TestMethod>]
    member _.``build returns the elements that the action adds`` () =
        let array = FlatList.build (fun builder -> builder.AddRange Inputs.several)
        Assert.AreEqual<int list>(Inputs.several, FlatList.toList array, "FlatList.build must return the added elements")

    [<TestMethod>]
    member _.``builder starts empty`` () =
        let builder = FlatList.builder<int>()
        Assert.AreEqual<int>(0, builder.Count, "FlatList.builder must start without elements")

    [<TestMethod>]
    member _.``builderWith reserves the capacity`` () =
        let builder = FlatList.builderWith<int> 10
        Assert.AreEqual<int>(10, builder.Capacity, "FlatList.builderWith must reserve the capacity")
        Assert.AreEqual<int>(0, builder.Count, "FlatList.builderWith must start without elements")

    [<TestMethod>]
    member _.``builderWith throws for a negative capacity`` () =
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            Action (fun () -> FlatList.builderWith<int> -1 |> ignore),
            "FlatList.builderWith must reject a negative capacity as the BCL does"
        )
        |> ignore

    [<TestMethod>]
    member _.``moveFromBuilder takes the contents of a full builder`` () =
        let builder = FlatList.builderWith<int> 3
        builder.AddRange [ 1; 2; 3 ]
        let array = FlatList.moveFromBuilder builder
        Assert.AreEqual<int list>([ 1; 2; 3 ], FlatList.toList array, "FlatList.moveFromBuilder must return the elements")
        Assert.AreEqual<int>(0, builder.Count, "FlatList.moveFromBuilder must empty the builder")

    [<TestMethod>]
    member _.``moveFromBuilder throws for a builder that is not full`` () =
        let builder = FlatList.builderWith<int> 3
        builder.Add 1

        Assert.ThrowsExactly<InvalidOperationException>(
            Action (fun () -> FlatList.moveFromBuilder builder |> ignore),
            "FlatList.moveFromBuilder must reject a builder whose count differs from its capacity"
        )
        |> ignore

    [<TestMethod>]
    member _.``ofBuilder copies the contents and keeps the builder`` () =
        let builder = filled ()
        let array = FlatList.ofBuilder builder
        builder.Add 99
        Assert.AreEqual<int list>(Inputs.several, FlatList.toList array, "FlatList.ofBuilder must copy the elements")
        Assert.AreEqual<int>(Inputs.several.Length + 1, builder.Count, "FlatList.ofBuilder must keep the builder usable")

    [<TestMethod>]
    member _.``ofBuilder throws for a null builder`` () =
        let ex =
            Assert.ThrowsExactly<ArgumentNullException>(
                Action (fun () ->
                    FlatList.ofBuilder (Unchecked.defaultof<FlatList<int>.Builder>)
                    |> ignore
                ),
                "FlatList.ofBuilder must reject a null builder"
            )

        Assert.AreEqual<string | null>("builder", ex.ParamName, "FlatList.ofBuilder must name the builder parameter")

    [<TestMethod>]
    member _.``toBuilder copies the elements into a new builder`` () =
        let array = FlatList.ofList Inputs.several
        let builder = FlatList.toBuilder array
        builder.Add 99
        Assert.AreEqual<int list>(Inputs.several @ [ 99 ], List.ofSeq builder, "FlatList.toBuilder must hold the elements")
        Assert.AreEqual<int list>(Inputs.several, FlatList.toList array, "FlatList.toBuilder must not change the flat list")

    [<TestMethod>]
    member _.``update applies the action to a copy`` () =
        let array = FlatList.ofList Inputs.several
        let updated = FlatList.update (fun builder -> builder.RemoveAt 0) array
        Assert.AreEqual<int list>(List.tail Inputs.several, FlatList.toList updated, "FlatList.update must apply the action")
        Assert.AreEqual<int list>(Inputs.several, FlatList.toList array, "FlatList.update must not change the input")

    [<TestMethod>]
    member _.``add, addRange and insertAt return the same builder`` () =
        let builder = FlatList.builder<int>()
        let result =
            builder
            |> FlatListBuilder.add 1
            |> FlatListBuilder.addRange [ 2; 3 ]
            |> FlatListBuilder.insertAt 0 0
        Assert.AreSame (builder, result, "The functions that change a builder must return it")
        Assert.AreEqual<int list>([ 0; 1; 2; 3 ], List.ofSeq builder, "add, addRange and insertAt must change the builder")

    [<TestMethod>]
    member _.``capacity, setCapacity and length report the builder`` () =
        let builder =
            FlatList.builderWith<int> 4
            |> FlatListBuilder.add 1
            |> FlatListBuilder.setCapacity 10
        Assert.AreEqual<int>(10, FlatListBuilder.capacity builder, "FlatListBuilder.capacity must return the capacity")
        Assert.AreEqual<int>(1, FlatListBuilder.length builder, "FlatListBuilder.length must count the elements")

    [<TestMethod>]
    member _.``clear removes every element`` () =
        let builder = filled () |> FlatListBuilder.clear
        Assert.AreEqual<int>(0, FlatListBuilder.length builder, "FlatListBuilder.clear must remove every element")

    [<TestMethod>]
    member _.``contains, indexOf and lastIndexOf search the builder`` () =
        let builder = filled ()
        Assert.IsTrue (FlatListBuilder.contains 8 builder, "FlatListBuilder.contains must find 8")
        Assert.AreEqual<int>(1, FlatListBuilder.indexOf -3 builder, "FlatListBuilder.indexOf must return the first index")
        Assert.AreEqual<int>(5, FlatListBuilder.lastIndexOf -3 builder, "FlatListBuilder.lastIndexOf must return the last index")
        Assert.AreEqual<int>(-1, FlatListBuilder.indexOf 1000 builder, "FlatListBuilder.indexOf must return -1 when absent")

    [<TestMethod>]
    member _.``drainToFlatList returns the elements and empties the builder`` () =
        let builder = filled ()
        let array = FlatListBuilder.drainToFlatList builder
        Assert.AreEqual<int list>(
            Inputs.several,
            FlatList.toList array,
            "FlatListBuilder.drainToFlatList must return the elements"
        )
        Assert.AreEqual<int>(0, FlatListBuilder.length builder, "FlatListBuilder.drainToFlatList must empty the builder")

    [<TestMethod>]
    member _.``item and updateAt read and replace an element`` () =
        let builder = filled () |> FlatListBuilder.updateAt 2 99
        Assert.AreEqual<int>(99, FlatListBuilder.item 2 builder, "FlatListBuilder.updateAt must replace the element")

    [<TestMethod>]
    member _.``remove and removeAt remove elements`` () =
        let builder = filled ()
        Assert.IsTrue (FlatListBuilder.remove -3 builder, "FlatListBuilder.remove must report a removed element")
        Assert.IsFalse (FlatListBuilder.remove 1000 builder, "FlatListBuilder.remove must report a missing element")
        FlatListBuilder.removeAt 0 builder |> ignore
        Assert.AreEqual<int list>([ 8; 5; 0; -3; 12; 7 ], List.ofSeq builder, "remove and removeAt must remove the elements")

    [<TestMethod>]
    member _.``rev reverses the builder`` () =
        let builder = filled () |> FlatListBuilder.rev
        Assert.AreEqual<int list>(List.rev Inputs.several, List.ofSeq builder, "FlatListBuilder.rev must reverse the elements")

    [<TestMethod>]
    member _.``sort, sortWith and sortWithComparer order the builder`` () =
        Assert.AreEqual<int list>(
            List.sort Inputs.several,
            List.ofSeq (filled () |> FlatListBuilder.sort),
            "FlatListBuilder.sort must sort ascending"
        )

        Assert.AreEqual<int list>(
            List.sortDescending Inputs.several,
            List.ofSeq (
                filled ()
                |> FlatListBuilder.sortWith (fun left right -> compare right left)
            ),
            "FlatListBuilder.sortWith must use the comparison function"
        )

        Assert.AreEqual<int list>(
            List.sortDescending Inputs.several,
            List.ofSeq (
                filled ()
                |> FlatListBuilder.sortWithComparer (Comparer<int>.Create (fun left right -> compare right left))
            ),
            "FlatListBuilder.sortWithComparer must use the comparer"
        )

    [<TestMethod>]
    member _.``toArray copies the elements`` () =
        let builder = filled ()
        CollectionAssert.AreEqual (
            List.toArray Inputs.several,
            FlatListBuilder.toArray builder,
            "FlatListBuilder.toArray must copy the elements"
        )
