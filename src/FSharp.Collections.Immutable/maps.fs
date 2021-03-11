namespace FSharp.Collections.Immutable

open System.Collections.Generic

type IMap<'Key, 'Value> = System.Collections.Immutable.IImmutableDictionary<'Key, 'Value>

type HashMap<'Key, 'Value> =
    System.Collections.Immutable.ImmutableDictionary<'Key, 'Value>

type HashMapBuilder<'Key, 'Value> = HashMap<'Key, 'Value>.Builder

[<RequireQualifiedAccess; CompiledName((nameof System.Collections.Immutable.ImmutableDictionary) + "Module")>]
module HashMap =

    type internal HashMapFactory = System.Collections.Immutable.ImmutableDictionary

    let inline check (map: HashMap<_, _>) = checkNotNull (nameof map) map

    ////////// Creating //////////

    let inline empty<'Key, 'Value> = HashMapFactory.Create<'Key, 'Value>()
    let inline singleton item = empty.Add(item)

    let inline ofSeq source = HashMapFactory.CreateRange(source)
    let inline ofSeqWith getKey source =
        source
        |> Seq.map (fun i -> KeyValuePair(getKey i, i))
        |> HashMapFactory.CreateRange
    let inline ofSeqGroupBy getKey source =
        source
        |> Seq.groupBy getKey
        |> Seq.map (fun (key,value) -> KeyValuePair(key, value))
        |> HashMapFactory.CreateRange
    let inline ofArray (source : _ array) = HashMapFactory.CreateRange(source)

    let inline toSeq (map: HashMap<_,_>) = map :> seq<_>
    let inline toArray (map : HashMap<_,_>) = check map; Seq.toArray map

    ////////// Building //////////

    let inline builder() = HashMapFactory.CreateBuilder()
    let inline builderWithKeyComparer comparer = HashMapFactory.CreateBuilder(comparer)
    let inline builderWithComparers keyComparer valueComparer = HashMapFactory.CreateBuilder(keyComparer, valueComparer)

    let inline ofBuilder (mapBuilder: HashMapBuilder<_,_>) =
        checkNotNull (nameof mapBuilder) mapBuilder
        mapBuilder.ToImmutable()

    let inline toBuilder map : HashMapBuilder<_,_> = check map; map.ToBuilder()

    let inline ofKeyComparer<'Key, 'Value> comparer = HashMapFactory.Create<'Key, 'Value>(comparer)
    let inline ofComparers<'Key, 'Value> keyComparer valueComparer = HashMapFactory.Create<'Key, 'Value>(keyComparer, valueComparer)


    let inline isEmpty map = check map; map.IsEmpty

    let inline length map = check map; map.Count

    let inline keyComparer map = check map; map.KeyComparer
    let inline valueComparer map = check map; map.ValueComparer

    let inline containsKey key map = check map; map.ContainsKey key;

    let inline find key map = check map; map.[key]
    let inline tryFind key map =
        check map
        match map.TryGetValue(key) with
        | true, value -> Some value
        | false, _ -> None
    let inline vTryFind key map =
        check map
        match map.TryGetValue(key) with
        | true, value -> ValueSome value
        | false, _ -> ValueNone

    let inline pick chooser map = check map; map |> Seq.pick (fun kvp -> chooser kvp.Key kvp.Value)
    let inline tryPick chooser map = check map; map |> Seq.tryPick (fun kvp -> chooser kvp.Key kvp.Value)
    let inline vTryPick chooser map =
        check map
        match map |> Seq.tryPick (fun kvp -> chooser kvp.Key kvp.Value) with
        | Some value -> ValueSome value
        | None -> ValueNone

    let inline iter action (map: HashMap<_,_>) = check map; map |> Seq.iter (fun kvp -> action kvp.Key kvp.Value)

    let inline exists predicate map = check map; map |> Seq.exists (fun kvp -> predicate kvp.Key kvp.Value)

    let inline add key value map : HashMap<_,_> = check map; map.Add(key, value)
    let inline append map pairs : HashMap<_,_> =
        check map
        checkNotNull (nameof pairs) pairs
        map.AddRange pairs

    let inline remove key map : HashMap<_,_> = check map; map.Remove key
    let inline except keys map : HashMap<_,_> = check map; map.RemoveRange keys

    let inline clear map: HashMap<_,_> = check map; map.Clear()

    let inline filter predicate map =
        map |> Seq.filter (fun (kvp:KeyValuePair<_,_>) -> predicate kvp.Key kvp.Value)

    let inline forall predicate map =
        map |> Seq.forall (fun (kvp:KeyValuePair<_,_>) -> predicate kvp.Key kvp.Value)

    let inline map mapping map' =
        map' |> Seq.map (fun (kvp:KeyValuePair<_,_>) -> mapping kvp.Key kvp.Value)  |> ofSeq

    let inline where predicate map =
        map |> Seq.where (fun (kvp:KeyValuePair<_,_>) -> predicate kvp.Key kvp.Value) |> empty.AddRange


type SortedMap<'Key, 'Value> =
    System.Collections.Immutable.ImmutableSortedDictionary<'Key, 'Value>

type SortedMapBuilder<'Key, 'Value> = SortedMap<'Key, 'Value>.Builder

[<RequireQualifiedAccess; CompiledName((nameof System.Collections.Immutable.ImmutableSortedDictionary) + "Module")>]
module SortedMap =

    type internal SortedMapFactory = System.Collections.Immutable.ImmutableSortedDictionary

    let inline check (sortedMap: SortedMap<_, _>) = checkNotNull (nameof sortedMap) sortedMap

    ////////// Creating //////////

    let inline empty<'Key, 'Value> = SortedMapFactory.Create<'Key, 'Value>()
    let inline singleton item = SortedMapFactory.Create(item)

    let inline ofSeq source = SortedMapFactory.CreateRange(source)
    let inline ofSeqWith getKey source =
        source
        |> Seq.map (fun i -> KeyValuePair(getKey i, i))
        |> SortedMapFactory.CreateRange
    let inline ofSeqGroupBy getKey source =
        source
        |> Seq.groupBy getKey
        |> Seq.map (fun (key,value) -> KeyValuePair(key, value))
    let inline ofArray (source : _ array) = SortedMapFactory.CreateRange(source)

    let inline toSeq (map: SortedMap<_,_>) = map :> seq<_>
    let inline toArray (map : SortedMap<_,_>) = check map; Seq.toArray map

    ////////// Building //////////

    let inline builder() = SortedMapFactory.CreateBuilder()
    let inline builderWithKeyComparer comparer = SortedMapFactory.CreateBuilder(comparer)
    let inline builderWithComparers keyComparer valueComparer = SortedMapFactory.CreateBuilder(keyComparer, valueComparer)

    let inline ofBuilder (sortedMapBuilder: SortedMapBuilder<_,_>) =
        checkNotNull (nameof sortedMapBuilder) sortedMapBuilder
        sortedMapBuilder.ToImmutable()

    let inline toBuilder map : SortedMapBuilder<_,_> = check map; map.ToBuilder()

    let inline ofKeyComparer<'Key, 'Value> comparer = SortedMapFactory.Create<'Key, 'Value>(comparer)
    let inline ofComparers<'Key, 'Value> keyComparer valueComparer = SortedMapFactory.Create<'Key, 'Value>(keyComparer, valueComparer)


    let inline isEmpty map = check map; map.IsEmpty

    let inline length map = check map; map.Count

    let inline keyComparer map = check map; map.KeyComparer
    let inline valueComparer map = check map; map.ValueComparer

    let inline containsKey key map = check map; map.ContainsKey key

    let inline find key map = check map; map.[key]
    let inline tryFind key map =
        check map
        match map.TryGetValue(key) with
        | true,value -> Some value
        | false,_ -> None
    let inline vTryFind key map =
        check map
        match map.TryGetValue(key) with
        | true, value -> ValueSome value
        | false, _ -> ValueNone

    let inline pick chooser map = check map; map |> Seq.pick (fun kvp -> chooser kvp.Key kvp.Value)
    let inline tryPick chooser map = check map; map |> Seq.tryPick (fun kvp -> chooser kvp.Key kvp.Value)
    let inline vTryPick chooser map =
        check map
        match map |> Seq.tryPick (fun kvp -> chooser kvp.Key kvp.Value) with
        | Some value -> ValueSome value
        | None -> ValueNone

    let inline iter action (map: SortedMap<_,_>) = check map; map |> Seq.iter (fun kvp -> action kvp.Key kvp.Value)

    let inline exists predicate map = check map; map |> Seq.exists (fun kvp -> predicate kvp.Key kvp.Value)

    let inline add key value map : SortedMap<_,_> = check map; map.Add(key, value)
    let inline append map pairs : SortedMap<_,_> =
        check map
        checkNotNull (nameof pairs) pairs
        map.AddRange pairs

    let inline remove key map : SortedMap<_,_> = check map; map.Remove key
    let inline except keys map : SortedMap<_,_> = check map; map.RemoveRange keys

    let inline clear map: SortedMap<_,_> = check map; map.Clear()

    let inline findKey predicate map =
        check map
        match (map |> Seq.tryFind (fun kvp -> predicate kvp.Key kvp.Value)) with
        | Some value -> value.Key
        | None -> raise (new KeyNotFoundException())
    let inline tryFindKey predicate map = check map; map |> Seq.tryFind (fun kvp -> predicate kvp.Key kvp.Value)
    let inline vTryFindKey predicate map =
        check map
        match (map |> Seq.tryFind (fun kvp -> predicate kvp.Key kvp.Value)) with
        | Some value -> ValueSome value.Key
        | None -> ValueNone

    let inline filter predicate map =
        map |> Seq.filter (fun (kvp:KeyValuePair<_,_>) -> predicate kvp.Key kvp.Value)

    let inline forall predicate map =
        map |> Seq.forall (fun (kvp:KeyValuePair<_,_>) -> predicate kvp.Key kvp.Value)

    let inline map mapping (map': SortedMap<_,_>) =
        map' |> Seq.map (fun (kvp:KeyValuePair<_,_>) -> mapping kvp.Key kvp.Value)  |> ofSeq

    let inline where predicate map =
        map |> Seq.where (fun (kvp:KeyValuePair<_,_>) -> predicate kvp.Key kvp.Value) |> empty.AddRange
