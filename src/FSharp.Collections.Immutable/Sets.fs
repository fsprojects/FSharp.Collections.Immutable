namespace FSharp.Collections.Immutable

type SortedSet<'T> = System.Collections.Immutable.ImmutableSortedSet<'T>
type SortedSetBuilder<'T> = SortedSet<'T>.Builder

[<RequireQualifiedAccess;
  CompiledName((nameof System.Collections.Immutable.ImmutableSortedSet)
               + "Module")>]
module SortedSet =

    type internal SortedSetFactory = System.Collections.Immutable.ImmutableSortedSet

    let inline check (sortedSet : SortedSet<_>) = checkNotNull (nameof sortedSet) sortedSet

    ////////// Creating //////////

    let inline empty<'T> = SortedSetFactory.Create<'T>()
    let inline singleton<'T> (item : 'T) = SortedSetFactory.Create<'T>(item)
    let inline ofSeq source = SortedSetFactory.CreateRange (source)
    let inline ofSeqWithComparer comparer source = SortedSetFactory.Create (comparer, items = (source |> Array.ofSeq))
    let inline ofArray (source : _ array) = SortedSetFactory.CreateRange (source)

    let inline ofBuilder (sortedSetBuilder : SortedSetBuilder<_>) =
        checkNotNull (nameof sortedSetBuilder) sortedSetBuilder
        sortedSetBuilder.ToImmutable ()

    let inline ofComparer<'T> comparer = SortedSetFactory.Create<'T>(comparer = comparer)

    let inline toSeq (set : SortedSet<_>) = set :> seq<_>

    let inline toArray (set : SortedSet<_>) =
        check set
        Seq.toArray set

    ////////// Building //////////

    let inline builder () = SortedSetFactory.CreateBuilder ()
    let inline builderWith capacity : SortedSet<'T>.Builder = SortedSetFactory.CreateBuilder (capacity)
    let inline builderWithComparer comparer = SortedSetFactory.CreateBuilder (comparer)

    let inline toBuilder set : SortedSetBuilder<_> =
        check set
        set.ToBuilder ()

    let inline keyComparer set =
        check set
        set.KeyComparer


    let inline length set =
        check set
        set.Count

    let inline contains value set =
        check set
        set.Contains value

    let inline isEmpty set =
        check set
        set.IsEmpty

    let inline exists predicate map =
        check map
        map |> Seq.exists predicate

    let inline isSubset (set1 : SortedSet<_>) set2 =
        check set1
        set1.IsSubsetOf set2

    let inline isProperSubset (set1 : SortedSet<_>) set2 =
        check set1
        set1.IsProperSubsetOf set2

    let inline isSuperset (set1 : SortedSet<_>) set2 =
        check set1
        set1.IsSupersetOf set2

    let inline isProperSuperset (set1 : SortedSet<_>) set2 =
        check set1
        set1.IsProperSupersetOf set2

    let inline add value set : SortedSet<_> =
        check set
        set.Add (value)

    let inline union set values : SortedSet<_> =
        check set
        values |> set.Union

    let inline unionMany (sets : SortedSet<_> seq) = Seq.reduce union sets
    let inline intersect (set1 : SortedSet<_>) set2 = set1.Intersect set2
    let inline intersectMany (sets : SortedSet<_> seq) = Seq.reduce intersect sets

    let inline remove value set : SortedSet<_> =
        check set
        set.Remove value

    let inline difference values set : SortedSet<_> =
        check set
        values |> set.Except

    let inline clear set : SortedSet<_> =
        check set
        set.Clear ()

    let inline filter predicate set = set |> Seq.filter predicate |> empty.Union

    let inline where predicate set = set |> Seq.where predicate |> empty.Union

    let inline pick chooser set =
        check set
        set |> Seq.pick chooser

    let inline tryPick chooser set =
        check set
        set |> Seq.tryPick chooser

    let inline vTryPick chooser set =
        check set

        match set |> Seq.tryPick chooser with
        | Some value -> ValueSome value
        | None -> ValueNone

    let inline map mapping (set : SortedSet<_>) = set |> Seq.map mapping |> ofSeq

    let inline forall predicate set = set |> Seq.forall predicate

    let inline iter action (set : SortedSet<_>) =
        check set
        set |> Seq.iter action
