namespace FSharp.Collections.Immutable.Tests

open Microsoft.VisualStudio.TestTools.UnitTesting
open FSharp.Collections.Immutable

[<TestClass; TestCategory(nameof IndexedList)>]
type IndexedListTests () =

    [<TestMethod;
      TestCategory(nameof IndexedList.ofSeq);
      TestCategory(nameof IndexedList.length);
      TestCategory(nameof IndexedList.head);
      TestCategory(nameof Seq.ofIndexedList)>]
    member _.IndexedList_module_uses_the_immutable_list_bindings () =
        let indexedList = IndexedList.ofSeq [ 1; 2; 3 ]

        Assert.AreEqual (3, IndexedList.length indexedList, "IndexedList.length should return the source item count.")
        Assert.AreEqual (1, IndexedList.head indexedList, "IndexedList.head should return the first item.")

        CollectionAssert.AreEqual (
            [| 1; 2; 3 |],
            indexedList
            |> FSharp.Collections.Immutable.Seq.ofIndexedList
            |> Microsoft.FSharp.Collections.Seq.toArray,
            "Seq.ofIndexedList should preserve the indexed list contents."
        )
