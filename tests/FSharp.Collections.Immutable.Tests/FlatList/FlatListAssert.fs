namespace FSharp.Collections.Immutable.Tests

open System
open Microsoft.VisualStudio.TestTools.UnitTesting

/// The assertions of the tests of the flat list and of its builder.
[<AbstractClass; Sealed>]
type FlatListAssert =

    /// Asserts that an action rejects its input with an ArgumentNullException that names the parameter.
    static member RejectsNull (label : string, paramName : string, actual : unit -> unit) =
        let ex =
            Assert.ThrowsExactly<ArgumentNullException>(Action actual, $"%s{label} must throw ArgumentNullException")

        Assert.AreEqual<string | null>(paramName, ex.ParamName, $"%s{label} must name the parameter of the rejected input")
