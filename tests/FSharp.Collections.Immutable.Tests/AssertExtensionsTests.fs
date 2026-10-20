namespace FSharp.Collections.Immutable.Tests

open System
open Microsoft.VisualStudio.TestTools.UnitTesting

/// <summary>
/// The assertions on options, value options and results that extend
/// <see cref="T:Microsoft.VisualStudio.TestTools.UnitTesting.Assert"/>.
/// </summary>
[<TestClass; AssertExtensionsTestCategory>]
type AssertExtensionsTests () =

    /// The message of the failure that an assertion raises.
    static let failureOf (assertion : unit -> unit) : string =
        let failure = Assert.Throws<AssertFailedException>(Action assertion, "The assertion must fail")
        failure.Message

    static let assertContains (failure : string) (expected : string) (message : string) =
        StringAssert.Contains (failure, expected, StringComparison.Ordinal, message)

    // The empty option and value option of a known type: a type annotation cannot sit in a tuple of arguments.
    static let noInt : int option = None
    static let noValueInt : int voption = ValueNone

    // ---- option ----

    [<TestMethod>]
    member _.``WantSome returns the value of Some`` () =
        Assert.AreEqual<int>(5, Assert.WantSome (Some 5, "message"), "WantSome must return the value of Some")

    [<TestMethod>]
    member _.``WantSome fails with the message on None`` () =
        let failure = failureOf (fun () -> Assert.WantSome (noInt, "the value is missing") |> ignore)
        assertContains failure "the value is missing" "WantSome must fail with its message on None"

    [<TestMethod>]
    member _.``IsSome passes for Some and fails for None`` () =
        Assert.IsSome (Some 5, "message")
        let failure = failureOf (fun () -> Assert.IsSome (noInt, "the value is missing"))
        assertContains failure "the value is missing" "IsSome must fail with its message on None"

    [<TestMethod>]
    member _.``IsNone passes for None and fails for Some`` () =
        Assert.IsNone (noInt, "message")
        let failure = failureOf (fun () -> Assert.IsNone (Some 5, "the value is there"))
        assertContains failure "the value is there" "IsNone must fail with its message on Some"

    // ---- voption ----

    [<TestMethod>]
    member _.``WantValueSome returns the value of ValueSome`` () =
        Assert.AreEqual<int>(5, Assert.WantValueSome (ValueSome 5, "message"), "WantValueSome must return the value")

    [<TestMethod>]
    member _.``WantValueSome fails with the message on ValueNone`` () =
        let failure =
            failureOf (fun () ->
                Assert.WantValueSome (noValueInt, "the value is missing")
                |> ignore
            )

        assertContains failure "the value is missing" "WantValueSome must fail with its message on ValueNone"

    [<TestMethod>]
    member _.``IsValueSome passes for ValueSome and fails for ValueNone`` () =
        Assert.IsValueSome (ValueSome 5, "message")
        let failure = failureOf (fun () -> Assert.IsValueSome (noValueInt, "the value is missing"))
        assertContains failure "the value is missing" "IsValueSome must fail with its message on ValueNone"

    [<TestMethod>]
    member _.``IsValueNone passes for ValueNone and fails for ValueSome`` () =
        Assert.IsValueNone (noValueInt, "message")
        let failure = failureOf (fun () -> Assert.IsValueNone (ValueSome 5, "the value is there"))
        assertContains failure "the value is there" "IsValueNone must fail with its message on ValueSome"

    // ---- result ----

    [<TestMethod>]
    member _.``WantOk returns the value of Ok`` () =
        let result : Result<int, string> = Ok 5
        Assert.AreEqual<int>(5, Assert.WantOk (result, "message"), "WantOk must return the value of Ok")

    [<TestMethod>]
    member _.``WantOk fails with the message and the error on Error`` () =
        let result : Result<int, string> = Error "the error"
        let failure = failureOf (fun () -> Assert.WantOk (result, "the call must succeed") |> ignore)
        assertContains failure "the call must succeed" "WantOk must fail with its message on Error"
        assertContains failure "the error" "WantOk must fail with the error"

    [<TestMethod>]
    member _.``WantOk fails with the error alone when it has no message`` () =
        let result : Result<int, string> = Error "the error"
        let failure = failureOf (fun () -> Assert.WantOk (result) |> ignore)
        assertContains failure "the error" "WantOk must fail with the error"

    [<TestMethod>]
    member _.``IsOk passes for Ok and fails for Error`` () =
        let ok : Result<int, string> = Ok 5
        let error : Result<int, string> = Error "the error"
        Assert.IsOk (ok, "message")
        let failure = failureOf (fun () -> Assert.IsOk (error, "the call must succeed"))
        assertContains failure "the call must succeed" "IsOk must fail with its message on Error"

    [<TestMethod>]
    member _.``WantError returns the error of Error`` () =
        let result : Result<int, string> = Error "the error"
        Assert.AreEqual<string>("the error", Assert.WantError (result, "message"), "WantError must return the error")

    [<TestMethod>]
    member _.``WantError fails with the message and the value on Ok`` () =
        let result : Result<int, string> = Ok 5
        let failure = failureOf (fun () -> Assert.WantError (result, "the call must fail") |> ignore)
        assertContains failure "the call must fail" "WantError must fail with its message on Ok"
        assertContains failure "5" "WantError must fail with the value"

    [<TestMethod>]
    member _.``IsError passes for Error and fails for Ok`` () =
        let ok : Result<int, string> = Ok 5
        let error : Result<int, string> = Error "the error"
        Assert.IsError (error, "message")
        let failure = failureOf (fun () -> Assert.IsError (ok, "the call must fail"))
        assertContains failure "the call must fail" "IsError must fail with its message on Ok"

    [<TestMethod>]
    member _.``OkEquals passes for the expected value and fails for another value or for Error`` () =
        let ok : Result<int, string> = Ok 5
        Assert.OkEquals (5, ok, "message")
        let other : Result<int, string> = Ok 6
        let different = failureOf (fun () -> Assert.OkEquals (5, other, "the value must be 5"))
        assertContains different "the value must be 5" "OkEquals must fail with its message on another value"
        let error : Result<int, string> = Error "the error"
        let failure = failureOf (fun () -> Assert.OkEquals (5, error, "the call must succeed"))
        assertContains failure "the error" "OkEquals must fail with the error on Error"

    [<TestMethod>]
    member _.``ErrorEquals passes for the expected error and fails for another error or for Ok`` () =
        let error : Result<int, string> = Error "the error"
        Assert.ErrorEquals ("the error", error, "message")
        let other : Result<int, string> = Error "another error"
        let different =
            failureOf (fun () -> Assert.ErrorEquals ("the error", other, "the error must be the expected one"))
        assertContains different "the error must be the expected one" "ErrorEquals must fail with its message on another error"
        let ok : Result<int, string> = Ok 5
        let failure = failureOf (fun () -> Assert.ErrorEquals ("the error", ok, "the call must fail"))
        assertContains failure "5" "ErrorEquals must fail with the value on Ok"

    // ---- default values ----

    [<TestMethod>]
    member _.``IsDefaultOf passes for the default value and fails for another one`` () =
        Assert.IsDefaultOf (0, "message")
        Assert.IsDefaultOf (noInt, "message")
        let number = failureOf (fun () -> Assert.IsDefaultOf (1, "the number must be the default"))
        assertContains number "the number must be the default" "IsDefaultOf must fail with its message on a non-default number"
        let option = failureOf (fun () -> Assert.IsDefaultOf (Some 1, "the option must be the default"))
        assertContains option "the option must be the default" "IsDefaultOf must fail with its message on a non-default option"
