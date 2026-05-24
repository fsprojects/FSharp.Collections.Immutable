namespace FSharp.Collections.Immutable

[<RequireQualifiedAccess>]
module Seq =

    /// <summary>Returns a sequence view of a flat list.</summary>
    val inline ofFlatList : flatList : FlatList<'T> -> seq<'T>

    /// <summary>Creates a flat list from a sequence.</summary>
    val inline toFlatList : seq : seq<'T> -> FlatList<'T>

    /// <summary>Returns a sequence view of a stack.</summary>
    val inline ofStack : stack : IStack<'T> -> seq<'T>

    /// <summary>Creates a stack from a sequence.</summary>
    val inline toStack : seq : seq<'T> -> Stack<'T>

    /// <summary>Returns a sequence view of an immutable list.</summary>
    val inline ofImmutableList : immutableList : System.Collections.Immutable.ImmutableList<'T> -> seq<'T>

    /// <summary>Creates an immutable list from a sequence.</summary>
    val inline toImmutableList : seq : seq<'T> -> System.Collections.Immutable.ImmutableList<'T>

    /// <summary>Returns a sequence view of an indexed list.</summary>
    val inline ofIndexedList : indexedList : IndexedList<'T> -> seq<'T>

    /// <summary>Creates an indexed list from a sequence.</summary>
    val inline toIndexedList : seq : seq<'T> -> IndexedList<'T>

    /// <summary>Returns a sequence view of a queue.</summary>
    val inline ofQueue : queue : Queue<'T> -> seq<'T>

    /// <summary>Creates a queue from a sequence.</summary>
    val inline toQueue : source : seq<'T> -> Queue<'T>

    /// <summary>Returns a sequence view of a hash map.</summary>
    val inline ofHashMap :
        hashMap : HashMap<'Key, 'Value> -> seq<System.Collections.Generic.KeyValuePair<'Key, 'Value>> when 'Key : not null

    /// <summary>Creates a hash map from a sequence.</summary>
    val inline toHashMap :
        hashMap : seq<System.Collections.Generic.KeyValuePair<'Key, 'Value>> -> HashMap<'Key, 'Value> when 'Key : not null

    /// <summary>Returns a sequence view of a sorted map.</summary>
    val inline ofSortedMap :
        sortedHashMap : SortedMap<'Key, 'Value> -> seq<System.Collections.Generic.KeyValuePair<'Key, 'Value>> when 'Key : not null

    /// <summary>Creates a sorted map from a sequence.</summary>
    val inline toSortedMap :
        sortedHashMap : seq<System.Collections.Generic.KeyValuePair<'Key, 'Value>> -> SortedMap<'Key, 'Value> when 'Key : not null

    /// <summary>Returns a sequence view of a hash set.</summary>
    val inline ofHashSet : hashSet : HashSet<'T> -> seq<'T>

    /// <summary>Creates a hash set from a sequence.</summary>
    val inline toHashSet : hashSet : seq<'T> -> HashSet<'T>

    /// <summary>Returns a sequence view of a sorted set.</summary>
    val inline ofSortedSet : sortedSet : SortedSet<'T> -> seq<'T>

    /// <summary>Creates a sorted set from a sequence.</summary>
    val inline toSortedSet : sortedSet : seq<'T> -> SortedSet<'T>
