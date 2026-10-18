namespace FSharp.Collections.Immutable.Tests

open Microsoft.VisualStudio.TestTools.UnitTesting

/// Compares a function of the `Option` or `Tuple` namespace with the default function after conversion (ADR 0001, D3).
[<AbstractClass; Sealed>]
type OverrideAssert =

    /// The `Option` namespace returns an `option` where the default module returns a `voption`.
    /// The equality of an F# option is structural, so an option of an array compares the elements of the array.
    static member OptionMatches (expected : 'T voption, actual : 'T option, [<Struct>] ?message : string) =
        Assert.AreEqual<'T option>(ValueOption.toOption expected, actual, message |> ValueOption.defaultValue "")

    /// The `Tuple` namespace returns a reference pair where the default module returns a struct pair.
    /// The equality of a tuple compares arrays by reference, so the pairs are compared with F# structural equality.
    static member PairMatches
        (struct (expectedFirst : 'T1, expectedSecond : 'T2), actual : 'T1 * 'T2, [<Struct>] ?message : string)
        =
        Assert.AreEqual<'T1 * 'T2>(
            (expectedFirst, expectedSecond),
            actual,
            HashIdentity.Structural,
            message |> ValueOption.defaultValue ""
        )

    /// The `Tuple` namespace returns a reference triple where the default module returns a struct triple.
    /// The equality of a tuple compares arrays by reference, so the triples are compared with F# structural equality.
    static member TripleMatches
        (
            struct (expectedFirst : 'T1, expectedSecond : 'T2, expectedThird : 'T3),
            actual : 'T1 * 'T2 * 'T3,
            [<Struct>] ?message : string
        )
        =
        Assert.AreEqual<'T1 * 'T2 * 'T3>(
            (expectedFirst, expectedSecond, expectedThird),
            actual,
            HashIdentity.Structural,
            message |> ValueOption.defaultValue ""
        )
