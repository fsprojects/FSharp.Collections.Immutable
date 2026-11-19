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

/// The trees that the display tests build. Their shapes are the ones that a display must bound: the work of a tree that
/// holds the same child twice doubles with each level, if the display does not limit the nested displays that it starts.
[<AbstractClass; Sealed>]
type DisplayTrees =

    /// A tree with the given number of levels in which every node holds the same child twice: a node and the array of
    /// two references to it, again and again. It has one node per level, and 2^levels leaves when it is shown in full.
    static member Shared (levels : int) : SyntaxNode =
        let mutable tree = Leaf 0

        for _ in 1..levels do
            tree <- Branch (EquatableArray.unsafeOfArray [| tree; tree |])

        tree

    /// A balanced binary tree with 2^levels leaves and 2^levels - 1 arrays, all of them different nodes.
    static member Balanced (levels : int) : SyntaxNode =
        // A ref cell, because a closure cannot capture a mutable local
        let leaves = ref 0

        let rec build (level : int) =
            if level = 0 then
                leaves.Value <- leaves.Value + 1
                Leaf leaves.Value
            else
                let left = build (level - 1)
                let right = build (level - 1)
                Branch (EquatableArray.unsafeOfArray [| left; right |])

        build levels

    /// A node that holds itself as many times as given: its display meets the cycle again in each child.
    static member Cyclic (copies : int) : CyclicNode =
        let node = { Children = EquatableArray.empty }
        node.Children <- EquatableArray.unsafeOfArray (Array.create copies node)
        node

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

    /// Wraps the array of a test literal, the short form for test data. The array is not copied: nothing else holds the
    /// literal. Tests of the functions that create arrays call those functions.
    let ea (items : 'T array) = EquatableArray.unsafeOfArray items

    /// The array that an ImmutableArray wraps, to show whether an operation shares or copies it.
    let underlying (items : ImmutableArray<'T>) : 'T array | null = ImmutableCollectionsMarshal.AsArray items
