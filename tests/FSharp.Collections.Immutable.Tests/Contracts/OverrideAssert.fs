/// Compares a function of the `Option` or `Tuple` namespace with the default function after conversion (ADR 0001, D3).
[<RequireQualifiedAccess>]
module FSharp.Collections.Immutable.Tests.OverrideAssert

open Microsoft.VisualStudio.TestTools.UnitTesting

/// The `Option` namespace returns an `option` where the default module returns a `voption`.
let optionMatches (expected : 'T voption) (actual : 'T option) (message : string) =
    Assert.AreEqual<'T option>(ValueOption.toOption expected, actual, message)

/// The `Tuple` namespace returns a reference pair where the default module returns a struct pair.
let pairMatches (struct (expectedFirst : 'T1, expectedSecond : 'T2)) (actual : 'T1 * 'T2) (message : string) =
    Assert.AreEqual<'T1 * 'T2>((expectedFirst, expectedSecond), actual, message)

/// The `Tuple` namespace returns a reference triple where the default module returns a struct triple.
let tripleMatches
    (struct (expectedFirst : 'T1, expectedSecond : 'T2, expectedThird : 'T3))
    (actual : 'T1 * 'T2 * 'T3)
    (message : string)
    =
    Assert.AreEqual<'T1 * 'T2 * 'T3>((expectedFirst, expectedSecond, expectedThird), actual, message)
