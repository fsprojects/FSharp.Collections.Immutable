#if INTERACTIVE
namespace global
#else
namespace FSharp.Collections.Immutable
#endif

[<AutoOpen>]
module internal ImmutableCollectionUtil =

    let inline checkNotNull name (arg : _ | null) =
        match arg with
        | null -> nullArg name
        | _ -> ()

module internal ErrorStrings =
    [<Literal>]
    let InputMustBeNonNegative = "The input must be non-negative."

    [<Literal>]
    let ListsHaveDifferentLengths = "The lists have different lengths."

[<AutoOpen>]
module internal ValueOption =

    module internal Seq =

        let vtryHead (source : 'T seq) =
            use enumerator = source.GetEnumerator ()
            if not (enumerator.MoveNext ()) then
                ValueNone
            else if obj.ReferenceEquals (enumerator.Current, null) then
                ValueNone
            else
                ValueSome enumerator.Current

        let vtryLast (source : 'T seq) =
            use enumerator = source.GetEnumerator ()
            if not (enumerator.MoveNext ()) then
                ValueNone
            else
                let mutable last = enumerator.Current
                while enumerator.MoveNext () do
                    last <- enumerator.Current
                if obj.ReferenceEquals (enumerator.Current, null) then
                    ValueNone
                else
                    ValueSome last

        let vchoose mapping (source : 'T seq) =
            source
            |> Seq.map mapping
            |> Seq.where ValueOption.isSome
            |> Seq.map ValueOption.get

        let vtryFind predicate (source : 'T seq) = source |> Seq.where predicate |> vtryHead
