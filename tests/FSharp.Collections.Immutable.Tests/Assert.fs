/// <summary>
/// Assertions on F# <see cref="T:Microsoft.FSharp.Core.FSharpOption`1"/>,
/// <see cref="T:Microsoft.FSharp.Core.FSharpValueOption`1"/> and <see cref="T:Microsoft.FSharp.Core.FSharpResult`2"/>
/// values as extensions of <see cref="T:Microsoft.VisualStudio.TestTools.UnitTesting.Assert"/>.
/// <para>
/// The assertions whose names start with <c>Want</c> return the unwrapped value or fail the test; their <c>Is</c>
/// twins discard the value.
/// </para>
/// </summary>
[<AutoOpen>]
module FSharp.Collections.Immutable.Tests.AssertExtensions

open System.Runtime.InteropServices
open Microsoft.VisualStudio.TestTools.UnitTesting

// MSTest takes the message of a failure as a text that is not null, while an optional message that the caller
// omitted is null.
let private messageOrEmpty (message : string | null) : string =
    match message with
    | null -> ""
    | message -> message

type Assert with

    /// <summary>
    /// Returns the value of <see cref="T:Microsoft.FSharp.Core.FSharpOption`1.Some"/>; fails the test with
    /// <paramref name="message"/> on <see cref="T:Microsoft.FSharp.Core.FSharpOption`1.None"/>.
    /// </summary>
    static member WantSome (value, [<Optional>] message : string | null) =
        match value with
        | Some some -> some
        | None ->
            Assert.Fail (messageOrEmpty message)
            Unchecked.defaultof<_>

    /// <summary>
    /// Fails the test with <paramref name="message"/> unless <paramref name="value"/> is
    /// <see cref="T:Microsoft.FSharp.Core.FSharpOption`1.Some"/>.
    /// </summary>
    static member IsSome (value, [<Optional>] message : string | null) = Assert.WantSome (value, message) |> ignore

    /// <summary>
    /// Fails the test with <paramref name="message"/> unless <paramref name="value"/> is
    /// <see cref="T:Microsoft.FSharp.Core.FSharpOption`1.None"/>.
    /// </summary>
    static member IsNone (value, [<Optional>] message : string | null) =
        match value with
        | Some _ -> Assert.Fail (messageOrEmpty message)
        | None -> ()

    /// <summary>
    /// Returns the value of <see cref="T:Microsoft.FSharp.Core.FSharpValueOption`1.ValueSome"/>; fails the test
    /// with <paramref name="message"/> on <see cref="T:Microsoft.FSharp.Core.FSharpValueOption`1.ValueNone"/>.
    /// </summary>
    static member WantValueSome (value, [<Optional>] message : string | null) =
        match value with
        | ValueSome some -> some
        | ValueNone ->
            Assert.Fail (messageOrEmpty message)
            Unchecked.defaultof<_>

    /// <summary>
    /// Fails the test with <paramref name="message"/> unless <paramref name="value"/> is
    /// <see cref="T:Microsoft.FSharp.Core.FSharpValueOption`1.ValueSome"/>.
    /// </summary>
    static member IsValueSome (value, [<Optional>] message : string | null) = Assert.WantValueSome (value, message) |> ignore

    /// <summary>
    /// Fails the test with <paramref name="message"/> unless <paramref name="value"/> is
    /// <see cref="T:Microsoft.FSharp.Core.FSharpValueOption`1.ValueNone"/>.
    /// </summary>
    static member IsValueNone (value, [<Optional>] message : string | null) =
        match value with
        | ValueSome _ -> Assert.Fail (messageOrEmpty message)
        | ValueNone -> ()

    /// <summary>
    /// Returns the value of <see cref="T:Microsoft.FSharp.Core.FSharpResult`2.Ok"/>; fails the test with
    /// <paramref name="message"/> and the error on <see cref="T:Microsoft.FSharp.Core.FSharpResult`2.Error"/>.
    /// </summary>
    static member WantOk (value, [<Optional>] message : string | null) =
        match value with
        | Ok ok -> ok
        | Error error ->
            match message with
            | null -> Assert.Fail (string error)
            | message -> Assert.Fail ($"'{message}': {error}")
            Unchecked.defaultof<_>

    /// <summary>
    /// Fails the test with <paramref name="message"/> unless <paramref name="value"/> is
    /// <see cref="T:Microsoft.FSharp.Core.FSharpResult`2.Ok"/>.
    /// </summary>
    static member IsOk (value, [<Optional>] message : string | null) = Assert.WantOk (value, message) |> ignore

    /// <summary>
    /// Returns the error of <see cref="T:Microsoft.FSharp.Core.FSharpResult`2.Error"/>; fails the test with
    /// <paramref name="message"/> and the value on <see cref="T:Microsoft.FSharp.Core.FSharpResult`2.Ok"/>.
    /// </summary>
    static member WantError (value, [<Optional>] message : string | null) =
        match value with
        | Error error -> error
        | Ok value ->
            match message with
            | null -> Assert.Fail (string value)
            | message -> Assert.Fail ($"'{message}': {value}")
            Unchecked.defaultof<_>

    /// <summary>
    /// Fails the test with <paramref name="message"/> unless <paramref name="value"/> is
    /// <see cref="T:Microsoft.FSharp.Core.FSharpResult`2.Error"/>.
    /// </summary>
    static member IsError (value, [<Optional>] message : string | null) = Assert.WantError (value, message) |> ignore

    /// <summary>
    /// Fails the test with <paramref name="message"/> unless <paramref name="value"/> is the default value of its
    /// type.
    /// </summary>
    static member inline IsDefaultOf< ^T> (value : ^T, [<Optional>] message : string) =
        Assert.AreEqual (box value, box Unchecked.defaultof< ^T>, message)

    /// <summary>
    /// Fails the test with <paramref name="message"/> unless <paramref name="actual"/> is
    /// <see cref="T:Microsoft.FSharp.Core.FSharpResult`2.Ok"/> holding <paramref name="expected"/>.
    /// </summary>
    static member inline OkEquals< ^R, 'E> (expected : ^R, actual : Result< ^R, 'E >, [<Optional>] message : string | null) =
        Assert.AreEqual (box expected, box (Assert.WantOk (actual, message)), message)

    /// <summary>
    /// Fails the test with <paramref name="message"/> unless <paramref name="actual"/> is
    /// <see cref="T:Microsoft.FSharp.Core.FSharpResult`2.Error"/> holding <paramref name="expected"/>.
    /// </summary>
    static member inline ErrorEquals<'R, ^E> (expected : ^E, actual : Result<'R, ^E>, [<Optional>] message : string | null) =
        Assert.AreEqual (box expected, box (Assert.WantError (actual, message)), message)
