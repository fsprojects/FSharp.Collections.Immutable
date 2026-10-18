namespace FSharp.Collections.Immutable.Tests

open System
open System.Globalization
open Microsoft.VisualStudio.TestTools.UnitTesting

/// The FSharp.Core module whose argument checks a collection module mirrors (ADR 0001, D6).
type Mirror =
    /// `FlatList` mirrors `Array`.
    | ArrayMirror
    /// `IndexedList`, `Queue`, `Stack` and their interface modules mirror `List`.
    | ListMirror

/// Compares the exception of a function with the exception of the FSharp.Core function that it mirrors (ADR 0001, D8).
[<RequireQualifiedAccess>]
module ExceptionAssert =

    /// Runs an action under the invariant culture.
    /// FSharp.Core ships localized messages and this library does not, so the messages are compared in English.
    let private invariant (action : unit -> 'T) =
        let culture = CultureInfo.CurrentCulture
        let uiCulture = CultureInfo.CurrentUICulture
        CultureInfo.CurrentCulture <- CultureInfo.InvariantCulture
        CultureInfo.CurrentUICulture <- CultureInfo.InvariantCulture

        try
            action ()
        finally
            CultureInfo.CurrentCulture <- culture
            CultureInfo.CurrentUICulture <- uiCulture

    /// <summary>The first line of a message, without the parameter suffix of <see cref="T:System.ArgumentException"/>.</summary>
    /// <remarks>
    /// FSharp.Core writes parameter names into the lines after the first one ("list1 is 2 elements shorter than list2"),
    /// and this library writes the names of its own parameters there, so only the first line is compared.
    /// </remarks>
    let headline (ex : exn) =
        let message =
            match ex with
            | :? ArgumentException as argumentException ->
                match argumentException.ParamName with
                | null -> ex.Message
                | paramName ->
                    let suffix = $" (Parameter '%s{paramName}')"

                    if ex.Message.EndsWith (suffix, StringComparison.Ordinal) then
                        ex.Message.Substring (0, ex.Message.Length - suffix.Length)
                    else
                        ex.Message
            | _ -> ex.Message

        match message.IndexOf ('\n') with
        | -1 -> message
        | newline -> message.AsSpan(0, newline).TrimEnd('\r').ToString()

    /// <summary>
    /// Asserts that <paramref name="actual"/> throws what <paramref name="oracle"/> throws: an exception of the same type
    /// with the same headline and, for an argument exception, the parameter name that
    /// <paramref name="paramName"/> maps the parameter name of the oracle to.
    /// </summary>
    let same (paramName : string -> string) (oracle : unit -> unit) (actual : unit -> unit) (message : string) =
        let expected =
            invariant (fun () -> Assert.Throws<exn>(Action oracle, "The FSharp.Core oracle must throw"))
        let thrown = invariant (fun () -> Assert.Throws<exn>(Action actual, message))
        Assert.AreEqual<Type>(expected.GetType (), thrown.GetType (), message)
        Assert.AreEqual<string>(headline expected, headline thrown, message)

        match expected, thrown with
        | (:? ArgumentException as expected), (:? ArgumentException as thrown) ->
            let expectedName =
                match expected.ParamName with
                | null -> null
                | name -> paramName name

            Assert.AreEqual<string | null>(expectedName, thrown.ParamName, message)
        | _ -> ()

    /// <summary>
    /// Asserts that <paramref name="actual"/> throws what the FSharp.Core module that the collection mirrors throws:
    /// <paramref name="arrayOracle"/> for <see cref="F:FSharp.Collections.Immutable.Tests.Mirror.ArrayMirror"/>,
    /// <paramref name="listOracle"/> for <see cref="F:FSharp.Collections.Immutable.Tests.Mirror.ListMirror"/>.
    /// </summary>
    let mirrored
        (mirror : Mirror)
        (paramName : string -> string)
        (arrayOracle : unit -> unit)
        (listOracle : unit -> unit)
        (actual : unit -> unit)
        (message : string)
        =
        let oracle =
            match mirror with
            | ArrayMirror -> arrayOracle
            | ListMirror -> listOracle

        same paramName oracle actual message
