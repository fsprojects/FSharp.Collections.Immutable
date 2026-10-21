namespace FSharp.Collections.Immutable

open System
open System.Collections.Frozen
open System.Collections.Generic

// A struct with a primary constructor cannot check its argument, so the field and the constructor are explicit (ADR 0001,
// D12). The field is null only in the default value of the struct, which no constructor creates; the BCL treats a null
// comparer as the default comparer, and the Comparer property says so.
[<Struct; NoComparison; NoEquality>]
type CustomComparerFrozenSet<'T> =

    val private comparer : IEqualityComparer<'T> | null

    new (comparer : IEqualityComparer<'T>)
        =
        checkNotNull (nameof comparer) comparer
        { comparer = comparer }

    // ---- Creation ----

    // Create returns the shared empty frozen set for the default comparer and, unlike ToFrozenSet, collects nothing first
    [<CompiledName "Empty">]
    member this.empty : FrozenSet<'T> = FrozenSet.Create<'T>(this.Comparer, ReadOnlySpan<'T>.Empty)

    [<CompiledName "OfArray">]
    member this.ofArray (array : 'T array) : FrozenSet<'T> =
        checkNotNull (nameof array) array
        array.ToFrozenSet this.Comparer

    [<CompiledName "OfList">]
    member this.ofList (elements : 'T list) : FrozenSet<'T> =
        // Set.ofList dereferences a null list without a check; a null input throws ArgumentNullException here (D6)
        checkNotNull (nameof elements) elements
        elements.ToFrozenSet this.Comparer

    // The BCL keeps the first of the elements that the comparer finds equal, for every size of the span (the tests pin it)
    [<CompiledName "OfReadOnlySpan">]
    member this.ofReadOnlySpan (span : ReadOnlySpan<'T>) : FrozenSet<'T> = FrozenSet.Create<'T>(this.Comparer, span)

    [<CompiledName "OfSeq">]
    member this.ofSeq (elements : seq<'T>) : FrozenSet<'T> =
        // A default flat list given as a sequence would fail with InvalidOperationException in the BCL call (ADR 0001, D6)
        checkNotNullOrDefaultSeq (nameof elements) elements
        // ToFrozenSet returns a frozen set whose comparer equals the comparer, and copies any other sequence
        elements.ToFrozenSet this.Comparer

    // Create takes a read-only span; F# converts the span of the argument
    [<CompiledName "OfSpan">]
    member this.ofSpan (span : Span<'T>) : FrozenSet<'T> = FrozenSet.Create<'T>(this.Comparer, span)

    [<CompiledName "Singleton">]
    member this.singleton (value : 'T) : FrozenSet<'T> = [| value |].ToFrozenSet this.Comparer

    // ---- Properties ----

    member this.Comparer : IEqualityComparer<'T> =
        match this.comparer with
        | null -> EqualityComparer<'T>.Default :> IEqualityComparer<'T>
        | comparer -> comparer
