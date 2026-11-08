namespace FSharp.Collections.Immutable.Tests

open System
open System.Collections.Generic
open System.Collections.Immutable
open System.Text.Json
open System.Text.Json.Serialization
open Microsoft.VisualStudio.TestTools.UnitTesting
open FSharp.Collections.Immutable

/// The serializer options of the tests and a reader that suits F# nullness.
module internal JsonText =

    /// Plain System.Text.Json, without any registration.
    let plain = JsonSerializerOptions ()

    /// FSharp.SystemTextJson with its defaults.
    let fsharp = JsonFSharpOptions.Default().ToJsonSerializerOptions()

    /// FSharp.SystemTextJson with skippable option fields.
    let skippable =
        JsonFSharpOptions
            .Default()
            .WithSkippableOptionFields(SkippableOptionFields.Always, deserializeNullAsNone = true)
            .ToJsonSerializerOptions()

    /// Every option set, with a name for the failure messages.
    let all = [
        "plain System.Text.Json", plain
        "FSharp.SystemTextJson", fsharp
        "FSharp.SystemTextJson with skippable options", skippable
    ]

    /// Reads a JSON text through the non-generic overload: the generic one returns a value that can be null, which
    /// F# nullness rejects for a struct result (FS3265).
    let read<'T> (options : JsonSerializerOptions) (json : string) : 'T =
        match JsonSerializer.Deserialize (json, typeof<'T>, options) with
        | :? 'T as value -> value
        | _ -> failwith $"The JSON %s{json} did not give a value of type %s{typeof<'T>.Name}."

    /// Writes a value as a JSON text.
    let write (options : JsonSerializerOptions) (value : 'T) : string = JsonSerializer.Serialize (value, options)

[<TestClass; EquatableArrayTestCategory; JsonTestCategory>]
type EquatableArrayJsonTests () =

    [<TestMethod>]
    member _.``an array is written as a JSON array and read back`` () =
        for name, options in JsonText.all do
            let array = ea [ "a"; "b" ]
            Assert.AreEqual (
                """["a","b"]""",
                JsonText.write options array,
                $"%s{name}: the array must be written as a JSON array"
            )
            Assert.AreEqual (
                array,
                JsonText.read<EquatableArray<string>> options """["a","b"]""",
                $"%s{name}: a JSON array must be read back"
            )

    [<TestMethod>]
    member _.``the JSON is the same as for ImmutableArray`` () =
        Assert.AreEqual (
            JsonText.write JsonText.plain (ImmutableArray.Create (1, 2)),
            JsonText.write JsonText.plain (ea [ 1; 2 ]),
            "a document member can change from ImmutableArray to EquatableArray without a change of the stored JSON"
        )

    [<TestMethod>]
    member _.``a record round-trips`` () =
        for name, options in JsonText.all do
            let document = { Name = "x"; Tags = ea [ "a"; "b" ] }
            let json = JsonText.write options document
            Assert.AreEqual ("""{"Name":"x","Tags":["a","b"]}""", json, $"%s{name}: the record must be written with a JSON array")
            Assert.AreEqual (document, JsonText.read<Document> options json, $"%s{name}: the record must be read back")

    [<TestMethod>]
    member _.``a default member is written as an empty JSON array`` () =
        for name, options in JsonText.all do
            let json = JsonText.write options { Name = "x"; Tags = Unchecked.defaultof<_> }
            Assert.AreEqual ("""{"Name":"x","Tags":[]}""", json, $"%s{name}: the default value must be written as []")

    [<TestMethod>]
    member _.``a missing or null member is the empty array under plain System.Text.Json`` () =
        let missing = JsonText.read<Document> JsonText.plain """{"Name":"x"}"""
        Assert.IsTrue (missing.Tags.IsEmpty, "a missing member must give the default value, which is empty")
        let explicitNull = JsonText.read<Document> JsonText.plain """{"Name":"x","Tags":null}"""
        Assert.IsTrue (explicitNull.Tags.IsEmpty, "a null member must give the empty array")

    [<TestMethod>]
    member _.``a missing or null member is an error of the FSharp.SystemTextJson record converter`` () =
        // FSharp.SystemTextJson requires every field that is not an option; the converter of the array is not called
        for name, options in [ "defaults", JsonText.fsharp; "skippable options", JsonText.skippable ] do
            Assert.ThrowsExactly<JsonException>(
                (fun () -> JsonText.read<Document> options """{"Name":"x"}""" |> ignore),
                $"%s{name}: a missing member must be an error"
            )
            |> ignore

            Assert.ThrowsExactly<JsonException>(
                (fun () ->
                    JsonText.read<Document> options """{"Name":"x","Tags":null}"""
                    |> ignore
                ),
                $"%s{name}: a null member must be an error"
            )
            |> ignore

    [<TestMethod>]
    member _.``a JSON null is the empty array`` () =
        for name, options in JsonText.all do
            Assert.IsTrue (
                (JsonText.read<EquatableArray<string>> options "null").IsEmpty,
                $"%s{name}: null must give the empty array"
            )

    [<TestMethod>]
    member _.``a JSON value that is not an array is an error`` () =
        Assert.ThrowsExactly<JsonException>(
            (fun () ->
                JsonText.read<EquatableArray<string>> JsonText.plain """{"a":1}"""
                |> ignore
            ),
            "an object must not be read as an array"
        )
        |> ignore

    [<TestMethod>]
    member _.``a mutable class and a dictionary value round-trip`` () =
        for name, options in JsonText.all do
            let json = JsonText.write options (ClassDocument (Name = "x", Tags = ea [ "a" ]))
            Assert.AreEqual (
                ea [ "a" ],
                (JsonText.read<ClassDocument> options json).Tags,
                $"%s{name}: a class property must round-trip"
            )

            let dictionary = Dictionary<string, EquatableArray<int>>()
            dictionary["k"] <- ea [ 1; 2 ]
            let back =
                JsonText.read<Dictionary<string, EquatableArray<int>>> options (JsonText.write options dictionary)
            Assert.AreEqual (ea [ 1; 2 ], back["k"], $"%s{name}: a dictionary value must round-trip")

    [<TestMethod>]
    member _.``nested arrays, value options, unions and struct tuples round-trip with FSharp.SystemTextJson`` () =
        let value = {
            Rows = ea [ ea [ 1; 2 ]; EquatableArray.empty ]
            Maybe = ValueSome (ea [ 3 ])
            Shapes = ea [ Circle 1.0; Polygon (ea [ struct (0.0, 1.0) ]) ]
        }

        for name, options in [ "defaults", JsonText.fsharp; "skippable options", JsonText.skippable ] do
            let json = JsonText.write options value
            Assert.AreEqual (value, JsonText.read<Nested> options json, $"%s{name}: the value must round-trip")

    [<TestMethod>]
    member _.``the converters and the naming policy of the options reach the elements`` () =
        let options = JsonSerializerOptions (PropertyNamingPolicy = JsonNamingPolicy.CamelCase)
        options.Converters.Add (JsonStringEnumConverter ())
        let days = ea [ DayOfWeek.Monday; DayOfWeek.Friday ]
        Assert.AreEqual ("""["Monday","Friday"]""", JsonText.write options days, "an element converter must write the elements")
        Assert.AreEqual (
            days,
            JsonText.read<EquatableArray<DayOfWeek>> options """["Monday","Friday"]""",
            "an element converter must read the elements"
        )

        let documents = ea [ { Name = "x"; Tags = EquatableArray.empty } ]
        Assert.AreEqual (
            """[{"name":"x","tags":[]}]""",
            JsonText.write options documents,
            "the naming policy must apply to the elements"
        )

    [<TestMethod>]
    member _.``an explicitly registered factory still works`` () =
        let options = JsonSerializerOptions ()
        options.Converters.Add (EquatableArrayJsonConverterFactory ())
        Assert.AreEqual ("[1]", JsonText.write options (ea [ 1 ]), "a registered factory must write the array")
        Assert.AreEqual (ea [ 1 ], JsonText.read<EquatableArray<int>> options "[1]", "a registered factory must read the array")
