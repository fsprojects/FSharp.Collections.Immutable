#if INTERACTIVE
namespace global
#else
namespace FSharp.Collections.Immutable
#endif

[<RequireQualifiedAccess>]
module Seq =

    let inline ofFlatList flatList = FlatList.toSeq flatList
    let inline toFlatList seq = FlatList.ofSeq seq

    let inline ofStack stack = Stack.toSeq stack
    let inline toStack seq = Stack.ofSeq seq

    let inline ofImmutableList immutableList = ImmutableList.toSeq immutableList
    let inline toImmutableList seq = ImmutableList.ofSeq seq

    let inline ofQueue queue = Queue.toSeq queue
    let inline toQueue queue = Queue.ofSeq queue

    let inline ofHashMap hashMap = HashMap.toSeq hashMap
    let inline toHashMap hashMap = HashMap.ofSeq hashMap

    let inline ofSortedMap sortedHashMap = SortedMap.toSeq sortedHashMap
    let inline toSortedMap sortedHashMap = SortedMap.ofSeq sortedHashMap

    let inline ofHashSet hashSet = HashSet.toSeq hashSet
    let inline toHashSet hashSet = HashSet.ofSeq hashSet

    let inline ofSortedSet sortedSet = SortedSet.toSeq sortedSet
    let inline toSortedSet sortedSet = SortedSet.ofSeq sortedSet
