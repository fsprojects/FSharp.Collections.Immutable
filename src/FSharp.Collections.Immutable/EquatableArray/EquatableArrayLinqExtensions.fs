namespace FSharp.Collections.Immutable

open System
open System.Collections.Generic
open System.Collections.Immutable
open System.Linq
open System.Runtime.CompilerServices

// The operators of ImmutableArrayExtensions that end in OrDefault, and Aggregate, return T?: the default value of the
// element type, which is null for a reference type. F# cannot say so for an unconstrained 'T ('T | null needs
// 'T : not struct, so it could not take an int), and its own Unchecked.defaultof gives a plain 'T as well. The results of
// the public methods are therefore a plain 'T, and these calls are the one place where the nullness warning is off.
#nowarn "3261"

[<AbstractClass; Sealed>]
type internal NullableResults =

    static member Aggregate (items : ImmutableArray<'T>, func : Func<'T, 'T, 'T>) : 'T =
        ImmutableArrayExtensions.Aggregate (items, func)

    static member ElementAtOrDefault (items : ImmutableArray<'T>, index : int) : 'T =
        ImmutableArrayExtensions.ElementAtOrDefault (items, index)

    static member FirstOrDefault (items : ImmutableArray<'T>) : 'T = ImmutableArrayExtensions.FirstOrDefault items

    static member FirstOrDefault (items : ImmutableArray<'T>, predicate : Func<'T, bool>) : 'T =
        ImmutableArrayExtensions.FirstOrDefault (items, predicate)

    static member LastOrDefault (items : ImmutableArray<'T>) : 'T = ImmutableArrayExtensions.LastOrDefault items

    static member LastOrDefault (items : ImmutableArray<'T>, predicate : Func<'T, bool>) : 'T =
        ImmutableArrayExtensions.LastOrDefault (items, predicate)

    static member SingleOrDefault (items : ImmutableArray<'T>) : 'T = ImmutableArrayExtensions.SingleOrDefault items

    static member SingleOrDefault (items : ImmutableArray<'T>, predicate : Func<'T, bool>) : 'T =
        ImmutableArrayExtensions.SingleOrDefault (items, predicate)

#warnon "3261"

// Every method calls the method of the same name of System.Linq.ImmutableArrayExtensions on Items, which is never a
// default array. The methods of that class give the results and the exceptions of Enumerable, with one exception:
// Aggregate (see below).
[<Extension; AbstractClass; Sealed>]
type EquatableArrayLinqExtensions =

    [<Extension>]
    static member Aggregate (array : EquatableArray<'T>, func : Func<'T, 'T, 'T>) : 'T =
        // ImmutableArrayExtensions.Aggregate gives the default value for an empty array, where Enumerable.Aggregate
        // throws InvalidOperationException. A method that C# binds instead of Enumerable.Aggregate must not change what
        // existing code gets, so the empty array goes to Enumerable.Aggregate (boxing, on its way to the exception).
        if array.IsEmpty then
            Enumerable.Aggregate (array :> IEnumerable<'T>, func)
        else
            NullableResults.Aggregate (array.Items, func)

    [<Extension>]
    static member Aggregate
        (array : EquatableArray<'T>, seed : 'Accumulate, func : Func<'Accumulate, 'T, 'Accumulate>)
        : 'Accumulate
        =
        ImmutableArrayExtensions.Aggregate (array.Items, seed, func)

    [<Extension>]
    static member Aggregate
        (
            array : EquatableArray<'T>,
            seed : 'Accumulate,
            func : Func<'Accumulate, 'T, 'Accumulate>,
            resultSelector : Func<'Accumulate, 'Result>
        )
        : 'Result
        =
        ImmutableArrayExtensions.Aggregate (array.Items, seed, func, resultSelector)

    [<Extension>]
    static member All (array : EquatableArray<'T>, predicate : Func<'T, bool>) : bool =
        ImmutableArrayExtensions.All (array.Items, predicate)

    [<Extension>]
    static member Any (array : EquatableArray<'T>) : bool = ImmutableArrayExtensions.Any array.Items

    [<Extension>]
    static member Any (array : EquatableArray<'T>, predicate : Func<'T, bool>) : bool =
        ImmutableArrayExtensions.Any (array.Items, predicate)

    [<Extension>]
    static member ElementAt (array : EquatableArray<'T>, index : int) : 'T =
        ImmutableArrayExtensions.ElementAt (array.Items, index)

    [<Extension>]
    static member ElementAtOrDefault (array : EquatableArray<'T>, index : int) : 'T =
        NullableResults.ElementAtOrDefault (array.Items, index)

    [<Extension>]
    static member First (array : EquatableArray<'T>) : 'T = ImmutableArrayExtensions.First array.Items

    [<Extension>]
    static member First (array : EquatableArray<'T>, predicate : Func<'T, bool>) : 'T =
        ImmutableArrayExtensions.First (array.Items, predicate)

    [<Extension>]
    static member FirstOrDefault (array : EquatableArray<'T>) : 'T = NullableResults.FirstOrDefault array.Items

    [<Extension>]
    static member FirstOrDefault (array : EquatableArray<'T>, predicate : Func<'T, bool>) : 'T =
        NullableResults.FirstOrDefault (array.Items, predicate)

    [<Extension>]
    static member Last (array : EquatableArray<'T>) : 'T = ImmutableArrayExtensions.Last array.Items

    [<Extension>]
    static member Last (array : EquatableArray<'T>, predicate : Func<'T, bool>) : 'T =
        ImmutableArrayExtensions.Last (array.Items, predicate)

    [<Extension>]
    static member LastOrDefault (array : EquatableArray<'T>) : 'T = NullableResults.LastOrDefault array.Items

    [<Extension>]
    static member LastOrDefault (array : EquatableArray<'T>, predicate : Func<'T, bool>) : 'T =
        NullableResults.LastOrDefault (array.Items, predicate)

    [<Extension>]
    static member Select (array : EquatableArray<'T>, selector : Func<'T, 'Result>) : IEnumerable<'Result> =
        ImmutableArrayExtensions.Select (array.Items, selector)

    [<Extension>]
    static member SelectMany
        (
            array : EquatableArray<'T>,
            collectionSelector : Func<'T, IEnumerable<'Collection>>,
            resultSelector : Func<'T, 'Collection, 'Result>
        )
        : IEnumerable<'Result>
        =
        ImmutableArrayExtensions.SelectMany (array.Items, collectionSelector, resultSelector)

    [<Extension>]
    static member Single (array : EquatableArray<'T>) : 'T = ImmutableArrayExtensions.Single array.Items

    [<Extension>]
    static member Single (array : EquatableArray<'T>, predicate : Func<'T, bool>) : 'T =
        ImmutableArrayExtensions.Single (array.Items, predicate)

    [<Extension>]
    static member SingleOrDefault (array : EquatableArray<'T>) : 'T = NullableResults.SingleOrDefault array.Items

    [<Extension>]
    static member SingleOrDefault (array : EquatableArray<'T>, predicate : Func<'T, bool>) : 'T =
        NullableResults.SingleOrDefault (array.Items, predicate)

    [<Extension>]
    static member ToArray (array : EquatableArray<'T>) : 'T array =
        // Not ImmutableArrayExtensions.ToArray: it clones the underlying array, which keeps its run-time type. An array of
        // a base type over an array of a derived type (ImmutableArray<obj>.CastUp of a string array) would give a string
        // array, where Enumerable.ToArray gives an array of the base type, and a write of another object would throw. A
        // copy through a span has the element type, and an empty array gives Array.Empty.
        array.Items.AsSpan().ToArray()

    [<Extension>]
    static member ToDictionary
        (array : EquatableArray<'T>, keySelector : Func<'T, 'Key>)
        : Dictionary<'Key, 'T> when 'Key : not null
        =
        ImmutableArrayExtensions.ToDictionary (array.Items, keySelector)

    [<Extension>]
    static member ToDictionary
        (array : EquatableArray<'T>, keySelector : Func<'T, 'Key>, elementSelector : Func<'T, 'Element>)
        : Dictionary<'Key, 'Element> when 'Key : not null
        =
        ImmutableArrayExtensions.ToDictionary (array.Items, keySelector, elementSelector)

    [<Extension>]
    static member ToDictionary
        (array : EquatableArray<'T>, keySelector : Func<'T, 'Key>, comparer : IEqualityComparer<'Key> | null)
        : Dictionary<'Key, 'T> when 'Key : not null
        =
        ImmutableArrayExtensions.ToDictionary (array.Items, keySelector, comparer)

    [<Extension>]
    static member ToDictionary
        (
            array : EquatableArray<'T>,
            keySelector : Func<'T, 'Key>,
            elementSelector : Func<'T, 'Element>,
            comparer : IEqualityComparer<'Key> | null
        )
        : Dictionary<'Key, 'Element> when 'Key : not null
        =
        ImmutableArrayExtensions.ToDictionary (array.Items, keySelector, elementSelector, comparer)

    [<Extension>]
    static member Where (array : EquatableArray<'T>, predicate : Func<'T, bool>) : IEnumerable<'T> =
        ImmutableArrayExtensions.Where (array.Items, predicate)
