namespace FSharp.Collections.Immutable.Tests

open FsUnit
open NUnit.Framework

[<AutoOpen>]
module TestHelpers =
    let appliedTo value func = fun () -> func value
    let appliedToFunc value func = fun () -> func (value ())
    let produces value func = fun () -> () |> func |> should equal value
    let producesEquivalentOf value func = fun () -> () |> func |> should equivalent value
    let producesSameAs value func = fun () -> () |> func |> Seq.fold2 (fun acc x y -> acc && (x = y)) true value |> should equal true
    let throws func = fun () -> Assert.Catch (func >> ignore) |> ignore
    let doesNotThrow func = fun () -> func () |> ignore |> should equal ()
    let noopPredicate _ = true
    let ignore2 _ _ = ()
    let ignore3 _ _ _ = ()
    let testCase name category (code:unit->unit) = ((TestCaseData code).SetName (category + " - " + name)).SetCategory category
    let withSecondArg a f = fun b -> f b a
    let withSecondArgFrom a f = fun b -> f b (a ())
