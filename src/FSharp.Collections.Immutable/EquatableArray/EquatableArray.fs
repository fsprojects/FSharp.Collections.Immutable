namespace FSharp.Collections.Immutable

open System
open System.Collections
open System.Collections.Generic
open System.Collections.Immutable
open System.Runtime.CompilerServices
open System.Runtime.InteropServices
open System.Text.Json
open System.Text.Json.Serialization

/// The comparer of the elements of an EquatableArray, chosen once per element type: the order of F# generic
/// comparison, without boxing where a BCL comparer has that order.
[<AbstractClass; Sealed>]
type internal ElementComparer<'T> private () =

    static let comparer : IComparer<'T> =
        let elementType = typeof<'T>

        if Type.(=) (elementType, typeof<string>) then
            // Comparer<string>.Default compares with the current culture; F# compares strings ordinally
            unbox<IComparer<'T>>(box StringComparer.Ordinal)
        elif
            elementType.IsPrimitive
            || Type.(=) (elementType, typeof<decimal>)
        then
            // On these types Comparer<'T>.Default agrees with F# compare (NaN, signed zero and the extremes included)
            // and does not box
            Comparer<'T>.Default
        else
            // Unchecked.compare needs no 'comparison' constraint, which a conditional constraint cannot give
            {
                new IComparer<'T> with
                    member _.Compare (x, y) = Unchecked.compare x y
            }

    static member Instance = comparer

/// The nesting of EquatableArray displays on the current thread. %A starts a new formatter for each nested display, so
/// its own depth limit and cycle check do not reach through them; this counter does.
[<AbstractClass; Sealed>]
type internal DisplayDepth private () =

    [<ThreadStatic; DefaultValue>]
    static val mutable private depth : int

    /// The nesting at which a display shows "...", as %A does at its own default depth limit.
    static member Limit = 100

    static member Current = DisplayDepth.depth
    static member Enter () = DisplayDepth.depth <- DisplayDepth.depth + 1
    static member Exit () = DisplayDepth.depth <- DisplayDepth.depth - 1

[<Struct; CustomEquality; CustomComparison>]
[<CollectionBuilder(typeof<EquatableArray>, "Create")>]
[<JsonConverter(typeof<EquatableArrayJsonConverterFactory>)>]
[<StructuredFormatDisplay "{StructuredDisplayText}">]
type EquatableArray<[<EqualityConditionalOn; ComparisonConditionalOn>] 'T> =

    // The only field, so that the wrapper is as large as one reference
    val private array : ImmutableArray<'T>

    new (items : ImmutableArray<'T>) = { array = items }

    // A default field comes from Unchecked.defaultof, Array.zeroCreate or a missing JSON member. Every member reads
    // the elements through Items, so the default state of ImmutableArray is never observable.
    member this.Items : ImmutableArray<'T> =
        if this.array.IsDefault then
            ImmutableArray<'T>.Empty
        else
            this.array

    member this.Length : int = if this.array.IsDefault then 0 else this.array.Length
    member this.IsEmpty : bool = this.array.IsDefaultOrEmpty

    member this.Item
        with get (index : int) : 'T = this.Items[index]

    member this.GetEnumerator () : ImmutableArray<'T>.Enumerator = this.Items.GetEnumerator ()
    member this.AsSpan () : ReadOnlySpan<'T> = this.Items.AsSpan ()
    member this.AsMemory () : ReadOnlyMemory<'T> = this.Items.AsMemory ()

    member this.GetSlice (start : int option, finish : int option) : EquatableArray<'T> =
        let items = this.Items
        // The bounds of F# array slices: a missing or negative start means the first element, a missing or too large
        // finish means the last element, and an empty range gives an empty array
        let low =
            match start with
            | Some index when index >= 0 -> index
            | _ -> 0

        let high =
            match finish with
            | Some index when index < items.Length -> index
            | _ -> items.Length - 1

        // Compare before subtracting: high - low + 1 overflows for extreme bounds such as Int32.MinValue
        if high < low then
            EquatableArray<'T> ImmutableArray<'T>.Empty
        else
            // Both bounds are inside the array here. ImmutableArray.Slice returns the same array for the whole range
            // and copies any other range.
            EquatableArray<'T>(items.Slice (low, high - low + 1))

    // The member that C# ranges call. It checks its arguments as ImmutableArray.Slice does.
    member this.Slice (start : int, length : int) : EquatableArray<'T> = EquatableArray<'T>(this.Items.Slice (start, length))

    member this.Equals (other : EquatableArray<'T>) : bool =
        let left = this.Items
        let right = other.Items
        // The same underlying array is equal to itself without a scan, because EqualityComparer<'T>.Default is
        // reflexive. SequenceEqual compares the lengths first and does not box the elements.
        left.Equals right
        || MemoryExtensions.SequenceEqual (left.AsSpan (), right.AsSpan (), EqualityComparer<'T>.Default)

    override this.Equals (other : objnull) : bool =
        match other with
        | :? EquatableArray<'T> as other -> this.Equals other
        | _ -> false

    override this.GetHashCode () : int =
        // All elements, not the last 8 as the structural hash of ImmutableArray
        let mutable hash = HashCode ()

        for item in this.Items.AsSpan () do
            hash.Add item

        hash.ToHashCode ()

    member this.CompareTo (other : EquatableArray<'T>) : int =
        let left = this.Items
        let right = other.Items

        // Length first, as F# compares arrays: arrays of different lengths are ordered without reading an element
        if left.Length <> right.Length then
            left.Length.CompareTo right.Length
        else
            let comparer = ElementComparer<'T>.Instance
            let mutable result = 0
            let mutable index = 0

            while result = 0 && index < left.Length do
                result <- comparer.Compare (left[index], right[index])
                index <- index + 1

            result

    // A text, not a list: a value with StructuredFormatDisplay nested in the display of another one prints as "...",
    // which would hide the children of a syntax tree. %A reads this property through reflection, also when it is
    // private.
    member private this.StructuredDisplayText : string =
        // Each nested display starts a new %A formatter, which resets its depth limit and cycle check. Without a shared
        // guard, a cycle through a mutable record or a very deep tree recurses until the stack overflows. The guard
        // shows "..." at its limit, as %A does; the stack check also protects a thread with a small stack.
        if
            DisplayDepth.Current >= DisplayDepth.Limit
            || not (RuntimeHelpers.TryEnsureSufficientExecutionStack ())
        then
            "..."
        else
            DisplayDepth.Enter ()

            try
                // Items is never a default array, so its underlying array is never null
                let items =
                    this.Items
                    |> ImmutableCollectionsMarshal.AsArray
                    |> Unchecked.nonNull

                $"%A{items}"
            finally
                DisplayDepth.Exit ()

    override this.ToString () : string = this.StructuredDisplayText

    static member op_Equality (left : EquatableArray<'T>, right : EquatableArray<'T>) : bool = left.Equals right
    static member op_Inequality (left : EquatableArray<'T>, right : EquatableArray<'T>) : bool = not (left.Equals right)

    static member op_Implicit (source : EquatableArray<'T>) : ImmutableArray<'T> = source.Items
    static member op_Implicit (source : ImmutableArray<'T>) : EquatableArray<'T> = EquatableArray<'T> source

    interface IEquatable<EquatableArray<'T>> with
        /// <inheritdoc />
        member this.Equals other = this.Equals other

    interface IComparable<EquatableArray<'T>> with
        /// <inheritdoc />
        member this.CompareTo other = this.CompareTo other

    interface IComparable with
        /// <inheritdoc />
        member this.CompareTo other =
            match other with
            | :? EquatableArray<'T> as other -> this.CompareTo other
            | null -> 1
            | _ -> invalidArg (nameof other) "The object is not an EquatableArray of the same element type."

    interface IEnumerable with
        /// <inheritdoc />
        member this.GetEnumerator () : IEnumerator = (this.Items :> IEnumerable).GetEnumerator()

    interface IEnumerable<'T> with
        /// <inheritdoc />
        member this.GetEnumerator () : IEnumerator<'T> = (this.Items :> IEnumerable<'T>).GetEnumerator()

    interface IReadOnlyCollection<'T> with
        /// <inheritdoc />
        member this.Count = this.Length

    interface IReadOnlyList<'T> with
        /// <inheritdoc />
        member this.Item
            with get index = this.Items[index]

    // Read-only, as on ImmutableArray: FSharp.Core and LINQ take their fast paths (length, copy) for ICollection<'T>
    // and IList<'T>, not for IReadOnlyList<'T>. The members that write throw.
    interface ICollection<'T> with
        /// <inheritdoc />
        member this.Count = this.Length

        /// <inheritdoc />
        member _.IsReadOnly = true

        /// <inheritdoc />
        member this.Contains item = this.Items.Contains item

        /// <inheritdoc />
        member this.CopyTo (array, arrayIndex) = this.Items.CopyTo (array, arrayIndex)

        /// <inheritdoc />
        member _.Add _ = raise (NotSupportedException ())

        /// <inheritdoc />
        member _.Clear () = raise (NotSupportedException ())

        /// <inheritdoc />
        member _.Remove _ = raise (NotSupportedException ())

    interface IList<'T> with
        /// <inheritdoc />
        member this.Item
            with get index = this.Items[index]
            and set _ _ = raise (NotSupportedException ())

        /// <inheritdoc />
        member this.IndexOf item = this.Items.IndexOf item

        /// <inheritdoc />
        member _.Insert (_, _) = raise (NotSupportedException ())

        /// <inheritdoc />
        member _.RemoveAt _ = raise (NotSupportedException ())

and [<AbstractClass; Sealed>] EquatableArray =
    static member Create<'T> (items : ReadOnlySpan<'T>) : EquatableArray<'T> = EquatableArray<'T>(ImmutableArray.Create<'T> items)

// Absent from the signature: the factory creates it through reflection for each element type
and [<Sealed>] EquatableArrayJsonConverter<'T> () =
    inherit JsonConverter<EquatableArray<'T>> ()

    // A JSON null gives the empty array, the counterpart of the default value
    override _.HandleNull = true

    override _.Read (reader : byref<Utf8JsonReader>, _ : Type, options : JsonSerializerOptions) : EquatableArray<'T> =
        if reader.TokenType = JsonTokenType.Null then
            EquatableArray<'T> ImmutableArray<'T>.Empty
        else
            // The serializer's own handling of ImmutableArray reads the elements, so element converters and naming
            // policies apply. The non-generic overload, because the generic one on a struct result gives FS3265.
            match JsonSerializer.Deserialize (&reader, typeof<ImmutableArray<'T>>, options) with
            | :? ImmutableArray<'T> as items -> EquatableArray<'T> items
            | _ -> EquatableArray<'T> ImmutableArray<'T>.Empty

    override _.Write (writer : Utf8JsonWriter, value : EquatableArray<'T>, options : JsonSerializerOptions) : unit =
        JsonSerializer.Serialize (writer, value.Items, options)

and [<Sealed>] EquatableArrayJsonConverterFactory () =
    inherit JsonConverterFactory ()

    override _.CanConvert (typeToConvert : Type) : bool =
        typeToConvert.IsGenericType
        && Type.(=) (typeToConvert.GetGenericTypeDefinition (), typedefof<EquatableArray<_>>)

    override _.CreateConverter (typeToConvert : Type, _ : JsonSerializerOptions) : JsonConverter =
        let converterType =
            typedefof<EquatableArrayJsonConverter<_>>.MakeGenericType(typeToConvert.GetGenericArguments ())

        nonNull (Activator.CreateInstance converterType) :?> JsonConverter

[<RequireQualifiedAccess>]
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module EquatableArray =

    // ---- Creation ----

    [<GeneralizableValue>]
    [<CompiledName "Empty">]
    let empty<'T> : EquatableArray<'T> = EquatableArray<'T> ImmutableArray<'T>.Empty

    [<CompiledName "OfArray">]
    let ofArray (array : 'T array) : EquatableArray<'T> =
        checkNotNull (nameof array) array
        // The copy is referenced by nothing else, so it is wrapped without a second copy
        EquatableArray<'T>(ImmutableCollectionsMarshal.AsImmutableArray (Array.copy array))

    [<CompiledName "OfImmutableArray">]
    let ofImmutableArray (items : ImmutableArray<'T>) : EquatableArray<'T> = EquatableArray<'T> items

    [<CompiledName "OfList">]
    let ofList (list : 'T list) : EquatableArray<'T> =
        EquatableArray<'T>(ImmutableCollectionsMarshal.AsImmutableArray (List.toArray list))

    [<CompiledName "OfSeq">]
    let ofSeq (source : seq<'T>) : EquatableArray<'T> =
        checkNotNull (nameof source) source
        EquatableArray<'T>(ImmutableArray.CreateRange<'T> source)

    [<CompiledName "Singleton">]
    let singleton (value : 'T) : EquatableArray<'T> = EquatableArray<'T>(ImmutableArray.Create<'T> value)

    [<CompiledName "UnsafeOfArray">]
    let unsafeOfArray (array : 'T array) : EquatableArray<'T> =
        // Without the check, null would become a default array and thus silently the empty array
        checkNotNull (nameof array) array
        EquatableArray<'T>(ImmutableCollectionsMarshal.AsImmutableArray array)

    // ---- Properties ----

    [<CompiledName "IsEmpty">]
    let isEmpty (array : EquatableArray<'T>) : bool = array.IsEmpty

    [<CompiledName "Length">]
    let length (array : EquatableArray<'T>) : int = array.Length

    // ---- Transformation ----

    [<CompiledName "Map">]
    let map (mapping : 'T -> 'U) (array : EquatableArray<'T>) : EquatableArray<'U> =
        // Items is never a default array, so its underlying array is never null
        let source =
            array.Items
            |> ImmutableCollectionsMarshal.AsArray
            |> Unchecked.nonNull
        // Array.map returns a new array that nothing else references, so it is wrapped without a copy
        EquatableArray<'U>(
            source
            |> Array.map mapping
            |> ImmutableCollectionsMarshal.AsImmutableArray
        )

    // ---- Conversion ----

    [<CompiledName "ToImmutableArray">]
    let toImmutableArray (array : EquatableArray<'T>) : ImmutableArray<'T> = array.Items
