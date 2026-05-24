namespace FSharp.Collections.Immutable.Tests

open System.Collections.Generic
open Microsoft.VisualStudio.TestTools.UnitTesting

[<AbstractClass>]
type CollectionTestCategoryAttribute (categoryName : string) =
    inherit TestCategoryBaseAttribute ()

    override _.TestCategories = ResizeArray ([ categoryName ]) :> IList<string>

type IndexedListTestCategoryAttribute () =
    inherit CollectionTestCategoryAttribute ("IndexedList")
