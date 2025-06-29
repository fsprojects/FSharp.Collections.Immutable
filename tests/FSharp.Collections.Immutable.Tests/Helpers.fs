[<AutoOpen>]
module FSharp.Collections.Immutable.Tests.Helpers

let fstv tuple = let struct (a, _) = tuple in a
let sndv tuple = let struct (_, b) = tuple in b
