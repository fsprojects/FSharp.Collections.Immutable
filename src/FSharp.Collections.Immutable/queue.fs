namespace FSharp.Collections.Immutable

type IQueue<'T> = System.Collections.Immutable.IImmutableQueue<'T>

type Queue<'T> = System.Collections.Immutable.ImmutableQueue<'T>

[<RequireQualifiedAccess; CompiledName((nameof System.Collections.Immutable.ImmutableQueue) + "Module")>]
module Queue =

    type internal QueueFactory = System.Collections.Immutable.ImmutableQueue

    let inline private check (queue: IQueue<_>) = checkNotNull (nameof queue) queue

    let inline empty<'T> : Queue<'T> = QueueFactory.Create<'T>()

    let inline singleton<'T> (item : 'T) : Queue<'T> = QueueFactory.Create<'T> (item)

    let inline ofSeq(source : 'T seq) : Queue<'T> = QueueFactory.CreateRange source

    let inline toSeq (queue: Queue<_>) = queue :> seq<_>

    let isEmpty queue = check queue; queue.IsEmpty

    let clear queue : IQueue<_> = check queue; queue.Clear()

    let enqueue item queue : IQueue<_> = check queue; queue.Enqueue item

    let head queue = check queue; queue.Peek()

    let tail queue : IQueue<_> = check queue; queue.Dequeue()

    //////////

    let (|Cons|Nil|) queue = // consider renaming
        if isEmpty queue then Nil else Cons(head queue, tail queue)

    ////////// Predicate based //////////

    let filter predicate queue =
        let rec loop queue result =
            match queue with
            |Cons(head, tail) ->
                loop tail <| if predicate head then enqueue head result else result
            |Nil -> result

        loop queue <| clear queue

    ////////// Seq-based //////////

    let find predicate queue = check queue; Seq.find predicate queue
    let tryFind predicate queue = check queue; Seq.tryFind predicate queue

    let findIndex predicate queue = check queue; Seq.findIndex predicate queue
    let tryFindIndex predicate queue = check queue; Seq.tryFindIndex predicate queue

    let pick chooser queue = check queue; Seq.pick chooser queue
    let tryPick chooser queue = check queue; Seq.tryPick chooser queue

    let iter action queue = check queue; Seq.iter action queue
    let iteri action queue = check queue; Seq.iteri action queue
    let iter2 action (queue1: IQueue<_>) (queue2: IQueue<_>) =
        checkNotNull (nameof queue1) queue1
        checkNotNull (nameof queue2) queue2
        Seq.iter2 action queue1 queue2

    let fold folder state queue = check queue; Seq.fold folder state queue

    let forall predicate queue = check queue; Seq.forall predicate

    let exists predicate queue = check queue; Seq.exists predicate queue

    let reduce reduction queue = check queue; Seq.reduce reduction queue

    let inline sum queue = check queue; Seq.sum queue
    let inline sumBy projection queue = check queue; Seq.sumBy projection queue

    let inline average queue = check queue; Seq.average queue
    let inline averageBy projection queue = check queue; Seq.averageBy projection

module ImmutableQueue = Queue
