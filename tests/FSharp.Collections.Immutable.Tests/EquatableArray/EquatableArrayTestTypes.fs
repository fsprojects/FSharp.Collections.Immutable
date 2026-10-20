namespace FSharp.Collections.Immutable.Tests

open System.Collections.Generic
open System.Collections.Immutable
open System.Runtime.InteropServices
open FSharp.Collections.Immutable

/// A persisted document with an array member.
type Document = { Name : string; Tags : EquatableArray<string> }

/// A mutable class with an array property, as System.Text.Json fills it.
type ClassDocument () =
    member val Name = "" with get, set
    member val Tags = EquatableArray.empty<string> with get, set

/// A node of a recursive syntax tree whose children are an array.
type SyntaxNode =
    | Leaf of value : int
    | Branch of children : EquatableArray<SyntaxNode>

/// A record that can hold itself through a mutable field, to show that the display of a cycle stops.
type CyclicNode = { mutable Children : EquatableArray<CyclicNode> }

/// An F# struct record as an element type.
[<Struct>]
type Point = { X : int; Y : int }

/// A union with a struct tuple array as an element type.
type Shape =
    | Circle of radius : float
    | Polygon of points : EquatableArray<struct (float * float)>

/// Arrays nested in arrays, in a value option and in union cases.
type Nested = {
    Rows : EquatableArray<EquatableArray<int>>
    Maybe : EquatableArray<int> voption
    Shapes : EquatableArray<Shape>
}

/// An element whose hash code records each call, to show which elements a hash reads.
type HashProbe (number : int, asked : List<int>) =
    override _.GetHashCode () =
        asked.Add number
        number

    override this.Equals (other : objnull) = obj.ReferenceEquals (this, other)

[<AutoOpen>]
module internal EquatableArrayTestHelpers =

    /// Builds an array from a list, the short form for test data.
    let ea (items : 'T list) = EquatableArray.ofList items

    /// The array that an ImmutableArray wraps, to show whether an operation shares or copies it.
    let underlying (items : ImmutableArray<'T>) : 'T array | null = ImmutableCollectionsMarshal.AsArray items
