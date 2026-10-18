namespace FSharp.Collections.Immutable

open System
open System.Collections.Generic
open System.Collections.Immutable
// Opened after System.Collections.Generic, so that SortedSet names the abbreviation of this library, not the mutable set
open FSharp.Collections.Immutable

// A struct with a primary constructor cannot check its argument, so the field and the constructor are explicit (ADR 0001,
// D12). The field is null only in the default value of the struct, which no constructor creates. The BCL treats a null
// comparer as Comparer<'T>.Default, and the Comparer property does the same: F# generic comparison would need the
// comparison constraint, which the struct cannot require, because a custom comparer serves types without F# comparison.
[<Struct; NoComparison; NoEquality>]
type CustomComparerSortedSet<'T> =

    val private comparer : IComparer<'T> | null

    new (comparer : IComparer<'T>)
        =
        checkNotNull (nameof comparer) comparer
        { comparer = comparer }

    // ---- Creation ----

    [<CompiledName "Empty">]
    member this.empty : SortedSet<'T> = ImmutableSortedSet.Create<'T> this.Comparer

    // The elements go one by one into a builder, which keeps the first of the elements that compare equal, as Set.ofSeq
    // does; ImmutableSortedSet.CreateRange sorts them unstably and can keep any of them (SortedSets.ofSeq)

    [<CompiledName "OfArray">]
    member this.ofArray (array : 'T array) : SortedSet<'T> =
        checkNotNull (nameof array) array
        SortedSets.ofSeq this.Comparer array

    [<CompiledName "OfList">]
    member this.ofList (elements : 'T list) : SortedSet<'T> =
        // Set.ofList dereferences a null list without a check; a null input throws ArgumentNullException here (D6)
        checkNotNull (nameof elements) elements
        SortedSets.ofSeq this.Comparer elements

    // ImmutableSortedSet.Create with a span sorts the elements unstably and keeps an arbitrary one of the elements that the
    // comparer finds equal (measured: 32 of 100 first spellings kept, against 100 of 100 for a builder loop), so the
    // elements go into a builder one by one, as in ofArray. A byref-like parameter cannot reach SortedSets.ofSeq.
    [<CompiledName "OfReadOnlySpan">]
    member this.ofReadOnlySpan (span : ReadOnlySpan<'T>) : SortedSet<'T> =
        let builder = ImmutableSortedSet.CreateBuilder<'T> this.Comparer

        for item in span do
            builder.Add item |> ignore

        builder.ToImmutable ()

    [<CompiledName "OfSeq">]
    member this.ofSeq (elements : seq<'T>) : SortedSet<'T> =
        // A default flat list given as a sequence would fail with InvalidOperationException in the builder (ADR 0001, D6)
        checkNotNullOrDefaultSeq (nameof elements) elements
        let comparer = this.Comparer

        match elements with
        // A sorted set that orders with the same comparer instance is returned as it is, as ImmutableSortedSet.WithComparer
        // returns it (the test is reference equality, ADR 0001, D14). A sorted set with another comparer goes through the
        // builder like any sequence: WithComparer would refill the tree with an unstable sort.
        | :? SortedSet<'T> as set when obj.ReferenceEquals (set.KeyComparer, comparer) -> set
        | _ -> SortedSets.ofSeq comparer elements

    // The span of the argument is converted to a read-only span by the call
    [<CompiledName "OfSpan">]
    member this.ofSpan (span : Span<'T>) : SortedSet<'T> = this.ofReadOnlySpan span

    [<CompiledName "Singleton">]
    member this.singleton (value : 'T) : SortedSet<'T> = ImmutableSortedSet.Create<'T>(this.Comparer, value)

    // ---- Builder ----

    [<CompiledName "Build">]
    member this.build (action : SortedSet<'T>.Builder -> unit) : SortedSet<'T> =
        let builder = ImmutableSortedSet.CreateBuilder<'T> this.Comparer
        action builder
        builder.ToImmutable ()

    [<CompiledName "CreateBuilder">]
    member this.builder () : SortedSet<'T>.Builder = ImmutableSortedSet.CreateBuilder<'T> this.Comparer

    // ---- Properties ----

    member this.Comparer : IComparer<'T> =
        match this.comparer with
        | null -> Comparer<'T>.Default :> IComparer<'T>
        | comparer -> comparer
