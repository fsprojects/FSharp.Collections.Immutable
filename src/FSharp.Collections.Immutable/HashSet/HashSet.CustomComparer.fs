namespace FSharp.Collections.Immutable

open System
open System.Collections.Generic
open System.Collections.Immutable
// Opened after System.Collections.Generic, so that HashSet names the abbreviation of this library, not the mutable set
open FSharp.Collections.Immutable

// A struct with a primary constructor cannot check its argument, so the field and the constructor are explicit (ADR 0001,
// D12). The field is null only in the default value of the struct, which no constructor creates; the BCL treats a null
// comparer as the default comparer, and the Comparer property says so.
[<Struct; NoComparison; NoEquality>]
type CustomComparerHashSet<'T> =

    val private comparer : IEqualityComparer<'T> | null

    new (comparer : IEqualityComparer<'T>)
        =
        checkNotNull (nameof comparer) comparer
        { comparer = comparer }

    // ---- Creation ----

    [<CompiledName "Empty">]
    member this.empty : HashSet<'T> = ImmutableHashSet.Create<'T> this.Comparer

    [<CompiledName "OfArray">]
    member this.ofArray (array : 'T array) : HashSet<'T> =
        checkNotNull (nameof array) array
        ImmutableHashSet.CreateRange<'T>(this.Comparer, array)

    [<CompiledName "OfList">]
    member this.ofList (elements : 'T list) : HashSet<'T> =
        // Set.ofList dereferences a null list without a check; a null input throws ArgumentNullException here (D6)
        checkNotNull (nameof elements) elements
        let builder = ImmutableHashSet.CreateBuilder<'T> this.Comparer

        for item in elements do
            builder.Add item |> ignore

        builder.ToImmutable ()

    // The elements of the span are added in order, so the first of the elements that the comparer finds equal stays
    [<CompiledName "OfReadOnlySpan">]
    member this.ofReadOnlySpan (span : ReadOnlySpan<'T>) : HashSet<'T> = ImmutableHashSet.Create<'T>(this.Comparer, span)

    [<CompiledName "OfSeq">]
    member this.ofSeq (elements : seq<'T>) : HashSet<'T> =
        // A default flat list given as a sequence would fail with InvalidOperationException in the BCL call (ADR 0001, D6)
        checkNotNullOrDefaultSeq (nameof elements) elements
        // A hash set gets the comparer through WithComparer, which returns the hash set itself for the same comparer
        // instance (the test is reference equality, ADR 0001, D14)
        ImmutableHashSet.CreateRange<'T>(this.Comparer, elements)

    // Create takes a read-only span; F# converts the span of the argument
    [<CompiledName "OfSpan">]
    member this.ofSpan (span : Span<'T>) : HashSet<'T> = ImmutableHashSet.Create<'T>(this.Comparer, span)

    [<CompiledName "Singleton">]
    member this.singleton (value : 'T) : HashSet<'T> = ImmutableHashSet.Create<'T>(this.Comparer, value)

    // ---- Builder ----

    [<CompiledName "Build">]
    member this.build (action : HashSet<'T>.Builder -> unit) : HashSet<'T> =
        let builder = ImmutableHashSet.CreateBuilder<'T> this.Comparer
        action builder
        builder.ToImmutable ()

    [<CompiledName "CreateBuilder">]
    member this.builder () : HashSet<'T>.Builder = ImmutableHashSet.CreateBuilder<'T> this.Comparer

    // ---- Properties ----

    member this.Comparer : IEqualityComparer<'T> =
        match this.comparer with
        | null -> EqualityComparer<'T>.Default :> IEqualityComparer<'T>
        | comparer -> comparer
