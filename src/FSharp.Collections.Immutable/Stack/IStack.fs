[<RequireQualifiedAccess>]
module FSharp.Collections.Immutable.IStack

// System.Collections.Generic stays closed: its Stack<'T> would shadow the Stack<'T> abbreviation of this library
open System
open System.Collections.Immutable
open System.Linq
open FSharp.Collections.Immutable.StackConstruction

// The interface has the members Clear, Push, Peek and Pop of the stacks. A result with the element type of the input keeps
// the implementation of the input: it comes from Pop, or from Clear followed by Push from the last element of a snapshot to
// the first one. A result of another element type is an ImmutableStack in the same order. The checks of ListChecks throw
// what the List functions throw, with the names of the parameters of this module (ADR 0001, D5 and D6); Lengths.stackUpTo
// counts a stack only as far as a check needs it.

/// A stack of the implementation of the input with the given elements, the first one on top.
let private rebuild (stack : IStack<'T>) (items : 'T array) = pushArray items (stack.Clear ())

/// The stack below the first elements; the caller checks that the stack has them.
let private dropTop (count : int) (stack : IStack<'T>) =
    let mutable node = stack

    for _ in 1..count do
        node <- node.Pop ()

    node

/// The first elements in an array and the stack below them; the caller checks that the stack has them.
let private splitTop (count : int) (stack : IStack<'T>) =
    let top = Array.zeroCreate count
    let mutable node = stack

    for index = 0 to count - 1 do
        top[index] <- node.Peek ()
        node <- node.Pop ()

    struct (top, node)

/// The index and the first element that satisfies the predicate, or ValueNone.
let private firstMatch (predicate : 'T -> bool) (stack : IStack<'T>) =
    use enumerator = stack.GetEnumerator ()
    let mutable result = ValueNone
    let mutable index = 0

    while result.IsNone && enumerator.MoveNext () do
        let item = enumerator.Current

        if predicate item then
            result <- ValueSome (struct (index, item))

        index <- index + 1

    result

/// The index and the last element that satisfies the predicate, or ValueNone; List searches a copy from the end too.
let private lastMatch (predicate : 'T -> bool) (stack : IStack<'T>) =
    let array = Snapshot.ofSeq (nameof stack) stack
    let mutable index = array.Length - 1

    while index >= 0 && not (predicate array[index]) do
        index <- index - 1

    if index < 0 then
        ValueNone
    else
        ValueSome (struct (index, array[index]))

/// The first value that the chooser returns for the elements, in their order.
let private firstChoice (chooser : 'T -> 'U voption) (stack : IStack<'T>) =
    use enumerator = stack.GetEnumerator ()
    let mutable result = ValueNone

    while result.IsNone && enumerator.MoveNext () do
        result <- chooser enumerator.Current

    result

// ---- Properties ----

[<CompiledName "IsEmpty">]
let isEmpty (stack : IStack<'T>) =
    checkNotNull (nameof stack) stack
    stack.IsEmpty

[<CompiledName "Length">]
let length (stack : IStack<'T>) =
    checkNotNull (nameof stack) stack
    // The interface does not tell the length
    Lengths.stackUpTo Int32.MaxValue stack

// ---- Access ----

[<CompiledName "ExactlyOne">]
let exactlyOne (stack : IStack<'T>) =
    checkNotNull (nameof stack) stack
    // List.exactlyOne reports its input under the name "source"; two elements tell a longer stack apart
    ListChecks.exactlyOne "source" (Lengths.stackUpTo 2 stack)
    stack.Peek ()

[<CompiledName "Peek">]
let head (stack : IStack<'T>) =
    checkNotNull (nameof stack) stack
    ListChecks.notEmpty (nameof stack) (Lengths.stackUpTo 1 stack)
    stack.Peek ()

[<CompiledName "Last">]
let last (stack : IStack<'T>) =
    checkNotNull (nameof stack) stack
    ListChecks.notEmpty (nameof stack) (Lengths.stackUpTo 1 stack)
    Enumerable.Last stack

[<CompiledName "Pop">]
let tail (stack : IStack<'T>) =
    checkNotNull (nameof stack) stack
    ListChecks.notEmpty (nameof stack) (Lengths.stackUpTo 1 stack)
    stack.Pop ()

[<CompiledName "TryExactlyOne">]
let tryExactlyOne (stack : IStack<'T>) =
    checkNotNull (nameof stack) stack

    if Lengths.stackUpTo 2 stack = 1 then
        ValueSome (stack.Peek ())
    else
        ValueNone

[<CompiledName "TryPeek">]
let tryHead (stack : IStack<'T>) =
    checkNotNull (nameof stack) stack
    if stack.IsEmpty then
        ValueNone
    else
        ValueSome (stack.Peek ())

[<CompiledName "TryLast">]
let tryLast (stack : IStack<'T>) =
    checkNotNull (nameof stack) stack

    if stack.IsEmpty then
        ValueNone
    else
        ValueSome (Enumerable.Last stack)

[<CompiledName "TryPop">]
let tryTail (stack : IStack<'T>) =
    checkNotNull (nameof stack) stack
    if stack.IsEmpty then
        ValueNone
    else
        ValueSome (stack.Pop ())

[<CompiledName "TryPeekPop">]
let tryUncons (stack : IStack<'T>) =
    checkNotNull (nameof stack) stack

    if stack.IsEmpty then
        ValueNone
    else
        ValueSome (struct (stack.Peek (), stack.Pop ()))

[<CompiledName "PeekPop">]
let uncons (stack : IStack<'T>) =
    checkNotNull (nameof stack) stack
    // An empty stack throws what List.head throws
    ListChecks.notEmpty (nameof stack) (Lengths.stackUpTo 1 stack)
    struct (stack.Peek (), stack.Pop ())

[<return : Struct>]
[<CompiledName "ConsPattern">]
let (|Cons|_|) (stack : IStack<'T>) = tryUncons stack

[<CompiledName "NilPattern">]
let (|Nil|_|) (stack : IStack<'T>) =
    checkNotNull (nameof stack) stack
    stack.IsEmpty

// ---- Search ----

[<CompiledName "Contains">]
let contains (value : 'T) (source : IStack<'T>) =
    checkNotNull (nameof source) source
    // F# equality with the value on the left, as List.contains compares
    (firstMatch (fun item -> value = item) source).IsSome

[<CompiledName "Exists">]
let exists (predicate : 'T -> bool) (stack : IStack<'T>) =
    checkNotNull (nameof stack) stack
    (firstMatch predicate stack).IsSome

[<CompiledName "Exists2">]
let exists2 predicate (stack1 : IStack<'T1>) (stack2 : IStack<'T2>) =
    ListArrays.exists2 (nameof stack2) predicate (Snapshot.ofSeq (nameof stack1) stack1) (Snapshot.ofSeq (nameof stack2) stack2)

[<CompiledName "Find">]
let find (predicate : 'T -> bool) (stack : IStack<'T>) =
    checkNotNull (nameof stack) stack

    match firstMatch predicate stack with
    | ValueSome (struct (_, item)) -> item
    | ValueNone -> Raise.indexNotFound ()

[<CompiledName "FindBack">]
let findBack (predicate : 'T -> bool) (stack : IStack<'T>) =
    match lastMatch predicate stack with
    | ValueSome (struct (_, item)) -> item
    | ValueNone -> Raise.indexNotFound ()

[<CompiledName "FindIndex">]
let findIndex (predicate : 'T -> bool) (stack : IStack<'T>) =
    checkNotNull (nameof stack) stack

    match firstMatch predicate stack with
    | ValueSome (struct (index, _)) -> index
    | ValueNone -> Raise.indexNotFound ()

[<CompiledName "FindIndexBack">]
let findIndexBack (predicate : 'T -> bool) (stack : IStack<'T>) =
    match lastMatch predicate stack with
    | ValueSome (struct (index, _)) -> index
    | ValueNone -> Raise.indexNotFound ()

[<CompiledName "ForAll">]
let forall (predicate : 'T -> bool) (stack : IStack<'T>) =
    checkNotNull (nameof stack) stack
    (firstMatch (fun item -> not (predicate item)) stack).IsNone

[<CompiledName "ForAll2">]
let forall2 predicate (stack1 : IStack<'T1>) (stack2 : IStack<'T2>) =
    ListArrays.forall2
        (nameof stack1)
        (nameof stack2)
        predicate
        (Snapshot.ofSeq (nameof stack1) stack1)
        (Snapshot.ofSeq (nameof stack2) stack2)

[<CompiledName "Pick">]
let pick (chooser : 'T -> 'U voption) (stack : IStack<'T>) =
    checkNotNull (nameof stack) stack

    match firstChoice chooser stack with
    | ValueSome value -> value
    | ValueNone -> Raise.indexNotFound ()

[<CompiledName "TryFind">]
let tryFind (predicate : 'T -> bool) (stack : IStack<'T>) =
    checkNotNull (nameof stack) stack

    match firstMatch predicate stack with
    | ValueSome (struct (_, item)) -> ValueSome item
    | ValueNone -> ValueNone

[<CompiledName "TryFindBack">]
let tryFindBack (predicate : 'T -> bool) (stack : IStack<'T>) =
    match lastMatch predicate stack with
    | ValueSome (struct (_, item)) -> ValueSome item
    | ValueNone -> ValueNone

[<CompiledName "TryFindIndex">]
let tryFindIndex (predicate : 'T -> bool) (stack : IStack<'T>) =
    checkNotNull (nameof stack) stack

    match firstMatch predicate stack with
    | ValueSome (struct (index, _)) -> ValueSome index
    | ValueNone -> ValueNone

[<CompiledName "TryFindIndexBack">]
let tryFindIndexBack (predicate : 'T -> bool) (stack : IStack<'T>) =
    match lastMatch predicate stack with
    | ValueSome (struct (index, _)) -> ValueSome index
    | ValueNone -> ValueNone

[<CompiledName "TryPick">]
let tryPick (chooser : 'T -> 'U voption) (stack : IStack<'T>) =
    checkNotNull (nameof stack) stack
    firstChoice chooser stack

// ---- Transformation ----

[<CompiledName "AllPairs">]
let allPairs (stack1 : IStack<'T1>) (stack2 : IStack<'T2>) =
    Arrays.allPairs structPair (Snapshot.ofSeq (nameof stack1) stack1) (Snapshot.ofSeq (nameof stack2) stack2)
    |> ofArrayUnchecked

[<CompiledName "Append">]
let append (stack1 : IStack<'T>) (stack2 : IStack<'T>) =
    checkNotNull (nameof stack1) stack1
    checkNotNull (nameof stack2) stack2

    if stack1.IsEmpty then
        stack2
    else
        // The elements of the first stack go on top of the second one, whose implementation and nodes the result keeps
        pushArray (Snapshot.ofSeq (nameof stack1) stack1) stack2

[<CompiledName "Choose">]
let choose (chooser : 'T -> 'U voption) (stack : IStack<'T>) =
    checkNotNull (nameof stack) stack
    let chosen = ResizeArray<'U>()

    for item in stack do
        match chooser item with
        | ValueSome value -> chosen.Add value
        | ValueNone -> ()

    ofArrayUnchecked (chosen.ToArray ())

[<CompiledName "ChunkBySize">]
let chunkBySize chunkSize (stack : IStack<'T>) =
    checkNotNull (nameof stack) stack
    ListChecks.positive (nameof chunkSize) chunkSize

    Array.chunkBySize chunkSize (Snapshot.ofSeq (nameof stack) stack)
    |> Array.map ofArrayUnchecked
    |> ofArrayUnchecked

[<CompiledName "Collect">]
let collect (mapping : 'T -> IStack<'U>) (stack : IStack<'T>) =
    checkNotNull (nameof stack) stack
    let collected = ResizeArray<'U>()

    for item in stack do
        collected.AddRange (mapping item)

    ofArrayUnchecked (collected.ToArray ())

[<CompiledName "Concat">]
let concat (stacks : seq<IStack<'T>>) =
    checkNotNullOrDefaultSeq (nameof stacks) stacks
    let concatenated = ResizeArray<'T>()

    for stack in stacks do
        checkNotNull (nameof stacks) stack
        concatenated.AddRange stack

    ofArrayUnchecked (concatenated.ToArray ())

[<CompiledName "Push">]
let cons (value : 'T) (stack : IStack<'T>) =
    checkNotNull (nameof stack) stack
    stack.Push value

[<CompiledName "PushMany">]
let consMany (values : seq<'T>) (stack : IStack<'T>) =
    checkNotNullOrDefaultSeq (nameof values) values
    checkNotNull (nameof stack) stack
    pushArray (Snapshot.ofSeq (nameof values) values) stack

[<CompiledName "Filter">]
let filter (predicate : 'T -> bool) (stack : IStack<'T>) =
    let array = Snapshot.ofSeq (nameof stack) stack
    rebuild stack (Array.filter predicate array)

[<CompiledName "Indexed">]
let indexed (stack : IStack<'T>) =
    Arrays.indexed structPair (Snapshot.ofSeq (nameof stack) stack)
    |> ofArrayUnchecked

[<CompiledName "Map">]
let map (mapping : 'T -> 'U) (stack : IStack<'T>) =
    Array.map mapping (Snapshot.ofSeq (nameof stack) stack)
    |> ofArrayUnchecked

[<CompiledName "Map2">]
let map2 (mapping : 'T1 -> 'T2 -> 'U) (stack1 : IStack<'T1>) (stack2 : IStack<'T2>) =
    ListArrays.map2
        (nameof stack1)
        (nameof stack2)
        mapping
        (Snapshot.ofSeq (nameof stack1) stack1)
        (Snapshot.ofSeq (nameof stack2) stack2)
    |> ofArrayUnchecked

[<CompiledName "Map3">]
let map3 (mapping : 'T1 -> 'T2 -> 'T3 -> 'U) (stack1 : IStack<'T1>) (stack2 : IStack<'T2>) (stack3 : IStack<'T3>) =
    ListArrays.map3
        (nameof stack1)
        (nameof stack2)
        (nameof stack3)
        mapping
        (Snapshot.ofSeq (nameof stack1) stack1)
        (Snapshot.ofSeq (nameof stack2) stack2)
        (Snapshot.ofSeq (nameof stack3) stack3)
    |> ofArrayUnchecked

[<CompiledName "MapIndexed">]
let mapi (mapping : int -> 'T -> 'U) (stack : IStack<'T>) =
    Array.mapi mapping (Snapshot.ofSeq (nameof stack) stack)
    |> ofArrayUnchecked

[<CompiledName "MapIndexed2">]
let mapi2 (mapping : int -> 'T1 -> 'T2 -> 'U) (stack1 : IStack<'T1>) (stack2 : IStack<'T2>) =
    ListArrays.mapi2
        (nameof stack1)
        (nameof stack2)
        mapping
        (Snapshot.ofSeq (nameof stack1) stack1)
        (Snapshot.ofSeq (nameof stack2) stack2)
    |> ofArrayUnchecked

[<CompiledName "Pairwise">]
let pairwise (stack : IStack<'T>) =
    Arrays.pairwise structPair (Snapshot.ofSeq (nameof stack) stack)
    |> ofArrayUnchecked

[<CompiledName "Permute">]
let permute indexMap (stack : IStack<'T>) =
    // List.permute applies the algorithm of Array.permute to a copy of the list, with the same checks
    let array = Snapshot.ofSeq (nameof stack) stack
    rebuild stack (Array.permute indexMap array)

[<CompiledName "Reverse">]
let rev (stack : IStack<'T>) =
    checkNotNull (nameof stack) stack

    if Lengths.stackUpTo 2 stack <= 1 then
        stack
    else
        // Pushing the elements in their order leaves the last one on top
        let mutable reversed = stack.Clear ()

        for item in stack do
            reversed <- reversed.Push item

        reversed

[<CompiledName "Transpose">]
let transpose (stacks : seq<IStack<'T>>) =
    checkNotNullOrDefaultSeq (nameof stacks) stacks

    let arrays =
        stacks
        |> Seq.map (fun stack -> Snapshot.ofSeq (nameof stacks) stack)
        |> Seq.toArray

    // List.transpose names the lists "list.[0]", "list.[1]", …, which are the stacks "stack.[0]", "stack.[1]", … here;
    // once their lengths agree, Array.transpose agrees with it
    ListChecks.transpose "stack" (arrays |> Array.map _.Length)

    Array.transpose arrays
    |> Array.map ofArrayUnchecked
    |> ofArrayUnchecked

[<CompiledName "Unzip">]
let unzip (stack : IStack<struct ('T1 * 'T2)>) =
    let struct (first, second) = Arrays.unzip id (Snapshot.ofSeq (nameof stack) stack)
    struct (ofArrayUnchecked first, ofArrayUnchecked second)

[<CompiledName "Unzip3">]
let unzip3 (stack : IStack<struct ('T1 * 'T2 * 'T3)>) =
    let struct (first, second, third) = Arrays.unzip3 id (Snapshot.ofSeq (nameof stack) stack)
    struct (ofArrayUnchecked first, ofArrayUnchecked second, ofArrayUnchecked third)

[<CompiledName "Where">]
let where (predicate : 'T -> bool) (stack : IStack<'T>) =
    let array = Snapshot.ofSeq (nameof stack) stack
    rebuild stack (Array.filter predicate array)

[<CompiledName "Windowed">]
let windowed windowSize (stack : IStack<'T>) =
    checkNotNull (nameof stack) stack
    ListChecks.positive (nameof windowSize) windowSize

    Array.windowed windowSize (Snapshot.ofSeq (nameof stack) stack)
    |> Array.map ofArrayUnchecked
    |> ofArrayUnchecked

[<CompiledName "Zip">]
let zip (stack1 : IStack<'T1>) (stack2 : IStack<'T2>) =
    let array1 = Snapshot.ofSeq (nameof stack1) stack1
    let array2 = Snapshot.ofSeq (nameof stack2) stack2
    ListChecks.sameLengths (nameof stack1) array1.Length (nameof stack2) array2.Length
    Arrays.zip structPair array1 array2 |> ofArrayUnchecked

[<CompiledName "Zip3">]
let zip3 (stack1 : IStack<'T1>) (stack2 : IStack<'T2>) (stack3 : IStack<'T3>) =
    let array1 = Snapshot.ofSeq (nameof stack1) stack1
    let array2 = Snapshot.ofSeq (nameof stack2) stack2
    let array3 = Snapshot.ofSeq (nameof stack3) stack3
    ListChecks.zip3 (nameof stack1) array1.Length (nameof stack2) array2.Length (nameof stack3) array3.Length

    Arrays.zip3 structTriple array1 array2 array3
    |> ofArrayUnchecked

// ---- Folding ----

// The inline functions can use public members only, so they check the stack themselves; the empty stack throws what
// List.average and List.averageBy throw, with the name "source"
[<CompiledName "Average">]
let inline average (stack : IStack< ^T >) : ^T =
    if obj.ReferenceEquals (stack, null) then
        nullArg (nameof stack)

    if stack.IsEmpty then
        invalidArg "source" ErrorStrings.InputSequenceEmpty

    let mutable sum = LanguagePrimitives.GenericZero< ^T>
    let mutable count = 0

    for item in stack do
        sum <- Checked.(+) sum item
        count <- count + 1

    LanguagePrimitives.DivideByInt< ^T> sum count

[<CompiledName "AverageBy">]
let inline averageBy ([<InlineIfLambda>] projection : 'T -> ^U) (stack : IStack<'T>) : ^U =
    if obj.ReferenceEquals (stack, null) then
        nullArg (nameof stack)

    if stack.IsEmpty then
        invalidArg "source" ErrorStrings.InputSequenceEmpty

    let mutable sum = LanguagePrimitives.GenericZero< ^U>
    let mutable count = 0

    for item in stack do
        sum <- Checked.(+) sum (projection item)
        count <- count + 1

    LanguagePrimitives.DivideByInt< ^U> sum count

[<CompiledName "CompareWith">]
let compareWith (comparer : 'T -> 'T -> int) (stack1 : IStack<'T>) (stack2 : IStack<'T>) =
    // Array.compareWith compares as List.compareWith does: the first difference, then the lengths
    Array.compareWith comparer (Snapshot.ofSeq (nameof stack1) stack1) (Snapshot.ofSeq (nameof stack2) stack2)

[<CompiledName "Fold">]
let fold (folder : 'State -> 'T -> 'State) (state : 'State) (stack : IStack<'T>) =
    checkNotNull (nameof stack) stack
    let mutable state = state

    for item in stack do
        state <- folder state item

    state

[<CompiledName "Fold2">]
let fold2 (folder : 'State -> 'T1 -> 'T2 -> 'State) (state : 'State) (stack1 : IStack<'T1>) (stack2 : IStack<'T2>) =
    ListArrays.fold2
        (nameof stack1)
        (nameof stack2)
        folder
        state
        (Snapshot.ofSeq (nameof stack1) stack1)
        (Snapshot.ofSeq (nameof stack2) stack2)

[<CompiledName "FoldBack">]
let foldBack (folder : 'T -> 'State -> 'State) (stack : IStack<'T>) (state : 'State) =
    Array.foldBack folder (Snapshot.ofSeq (nameof stack) stack) state

[<CompiledName "FoldBack2">]
let foldBack2 (folder : 'T1 -> 'T2 -> 'State -> 'State) (stack1 : IStack<'T1>) (stack2 : IStack<'T2>) (state : 'State) =
    let array1 = Snapshot.ofSeq (nameof stack1) stack1
    let array2 = Snapshot.ofSeq (nameof stack2) stack2
    // List.foldBack2 checks the lengths before it calls the folder
    ListChecks.foldBack2 (nameof stack1) array1.Length (nameof stack2) array2.Length
    Array.foldBack2 folder array1 array2 state

[<CompiledName "Iterate">]
let iter (action : 'T -> unit) (stack : IStack<'T>) =
    checkNotNull (nameof stack) stack

    for item in stack do
        action item

[<CompiledName "Iterate2">]
let iter2 (action : 'T1 -> 'T2 -> unit) (stack1 : IStack<'T1>) (stack2 : IStack<'T2>) =
    ListArrays.iter2
        (nameof stack1)
        (nameof stack2)
        action
        (Snapshot.ofSeq (nameof stack1) stack1)
        (Snapshot.ofSeq (nameof stack2) stack2)

[<CompiledName "IterateIndexed">]
let iteri (action : int -> 'T -> unit) (stack : IStack<'T>) =
    checkNotNull (nameof stack) stack
    let mutable index = 0

    for item in stack do
        action index item
        index <- index + 1

[<CompiledName "IterateIndexed2">]
let iteri2 (action : int -> 'T1 -> 'T2 -> unit) (stack1 : IStack<'T1>) (stack2 : IStack<'T2>) =
    ListArrays.iteri2
        (nameof stack1)
        (nameof stack2)
        action
        (Snapshot.ofSeq (nameof stack1) stack1)
        (Snapshot.ofSeq (nameof stack2) stack2)

[<CompiledName "MapFold">]
let mapFold (mapping : 'State -> 'T -> struct ('Result * 'State)) (state : 'State) (stack : IStack<'T>) =
    let source = Snapshot.ofSeq (nameof stack) stack
    let results = Array.zeroCreate source.Length
    let mutable state = state

    for index = 0 to source.Length - 1 do
        let struct (result, next) = mapping state source[index]
        results[index] <- result
        state <- next

    struct (ofArrayUnchecked results, state)

[<CompiledName "MapFoldBack">]
let mapFoldBack (mapping : 'T -> 'State -> struct ('Result * 'State)) (stack : IStack<'T>) (state : 'State) =
    let source = Snapshot.ofSeq (nameof stack) stack
    let results = Array.zeroCreate source.Length
    let mutable state = state

    for index = source.Length - 1 downto 0 do
        let struct (result, next) = mapping source[index] state
        results[index] <- result
        state <- next

    struct (ofArrayUnchecked results, state)

// Seq.max and the others compare as the List functions do once the stack has elements, so the List checks come first

[<CompiledName "Max">]
let max (stack : IStack<'T>) =
    checkNotNull (nameof stack) stack
    ListChecks.notEmptySequence (nameof stack) (Lengths.stackUpTo 1 stack)
    Seq.max stack

[<CompiledName "MaxBy">]
let maxBy (projection : 'T -> 'U) (stack : IStack<'T>) =
    checkNotNull (nameof stack) stack
    ListChecks.notEmptySequence (nameof stack) (Lengths.stackUpTo 1 stack)
    Seq.maxBy projection stack

[<CompiledName "Min">]
let min (stack : IStack<'T>) =
    checkNotNull (nameof stack) stack
    ListChecks.notEmptySequence (nameof stack) (Lengths.stackUpTo 1 stack)
    Seq.min stack

[<CompiledName "MinBy">]
let minBy (projection : 'T -> 'U) (stack : IStack<'T>) =
    checkNotNull (nameof stack) stack
    ListChecks.notEmptySequence (nameof stack) (Lengths.stackUpTo 1 stack)
    Seq.minBy projection stack

[<CompiledName "Reduce">]
let reduce (reduction : 'T -> 'T -> 'T) (stack : IStack<'T>) =
    checkNotNull (nameof stack) stack
    ListChecks.notEmpty (nameof stack) (Lengths.stackUpTo 1 stack)
    Seq.reduce reduction stack

[<CompiledName "ReduceBack">]
let reduceBack (reduction : 'T -> 'T -> 'T) (stack : IStack<'T>) =
    let array = Snapshot.ofSeq (nameof stack) stack
    ListChecks.notEmpty (nameof stack) array.Length
    Array.reduceBack reduction array

[<CompiledName "Scan">]
let scan (folder : 'State -> 'T -> 'State) (state : 'State) (stack : IStack<'T>) =
    Array.scan folder state (Snapshot.ofSeq (nameof stack) stack)
    |> ofArrayUnchecked

[<CompiledName "ScanBack">]
let scanBack (folder : 'T -> 'State -> 'State) (stack : IStack<'T>) (state : 'State) =
    Array.scanBack folder (Snapshot.ofSeq (nameof stack) stack) state
    |> ofArrayUnchecked

[<CompiledName "Sum">]
let inline sum (stack : IStack< ^T >) : ^T =
    if obj.ReferenceEquals (stack, null) then
        nullArg (nameof stack)

    let mutable sum = LanguagePrimitives.GenericZero< ^T>

    for item in stack do
        sum <- Checked.(+) sum item

    sum

[<CompiledName "SumBy">]
let inline sumBy ([<InlineIfLambda>] projection : 'T -> ^U) (stack : IStack<'T>) : ^U =
    if obj.ReferenceEquals (stack, null) then
        nullArg (nameof stack)

    let mutable sum = LanguagePrimitives.GenericZero< ^U>

    for item in stack do
        sum <- Checked.(+) sum (projection item)

    sum

// ---- Partitioning and grouping ----

[<CompiledName "CountBy">]
let countBy (projection : 'T -> 'Key) (stack : IStack<'T>) =
    Arrays.countBy structPair projection (Snapshot.ofSeq (nameof stack) stack)
    |> ofArrayUnchecked

[<CompiledName "Distinct">]
let distinct (stack : IStack<'T>) =
    let array = Snapshot.ofSeq (nameof stack) stack
    rebuild stack (Array.distinct array)

[<CompiledName "DistinctBy">]
let distinctBy (projection : 'T -> 'Key) (stack : IStack<'T>) =
    let array = Snapshot.ofSeq (nameof stack) stack
    rebuild stack (Array.distinctBy projection array)

[<CompiledName "Except">]
let except (itemsToExclude : seq<'T>) (stack : IStack<'T>) =
    // List.except checks the items to exclude first and does not read them for an empty list
    checkNotNullOrDefaultSeq (nameof itemsToExclude) itemsToExclude
    checkNotNull (nameof stack) stack

    if stack.IsEmpty then
        stack
    else
        rebuild stack (Array.except itemsToExclude (Snapshot.ofSeq (nameof stack) stack))

[<CompiledName "GroupBy">]
let groupBy (projection : 'T -> 'Key) (stack : IStack<'T>) =
    Arrays.groupBy (fun key items -> struct (key, ofArrayUnchecked items)) projection (Snapshot.ofSeq (nameof stack) stack)
    |> ofArrayUnchecked

[<CompiledName "Partition">]
let partition (predicate : 'T -> bool) (stack : IStack<'T>) =
    let accepted, rejected = Array.partition predicate (Snapshot.ofSeq (nameof stack) stack)
    struct (rebuild stack accepted, rebuild stack rejected)

[<CompiledName "PartitionWith">]
let partitionWith (partitioner : 'T -> Choice<'T1, 'T2>) (stack : IStack<'T>) =
    let first, second = Array.partitionWith partitioner (Snapshot.ofSeq (nameof stack) stack)
    struct (ofArrayUnchecked first, ofArrayUnchecked second)

[<CompiledName "Skip">]
let skip count (stack : IStack<'T>) =
    checkNotNull (nameof stack) stack
    // A stack shorter than the count stops the counting at its length, which the exception reports
    ListChecks.skip (nameof count) count (Lengths.stackUpTo count stack)
    dropTop count stack

[<CompiledName "SkipLast">]
let skipLast count (stack : IStack<'T>) =
    checkNotNull (nameof stack) stack

    if count <= 0 then
        stack
    else
        let length = Lengths.stackUpTo Int32.MaxValue stack
        // The checks of List.skip
        ListChecks.skip (nameof count) count length
        let struct (top, _) = splitTop (length - count) stack
        rebuild stack top

[<CompiledName "SkipWhile">]
let skipWhile (predicate : 'T -> bool) (stack : IStack<'T>) =
    checkNotNull (nameof stack) stack
    let mutable node = stack

    while not node.IsEmpty && predicate (node.Peek ()) do
        node <- node.Pop ()

    node

[<CompiledName "SplitAt">]
let splitAt index (stack : IStack<'T>) =
    checkNotNull (nameof stack) stack
    // A stack shorter than the index stops the counting at its length, which the exception reports
    ListChecks.splitAt (nameof index) index (Lengths.stackUpTo index stack)
    let struct (top, rest) = splitTop index stack
    struct ((if rest.IsEmpty then stack else rebuild stack top), rest)

[<CompiledName "SplitInto">]
let splitInto count (stack : IStack<'T>) =
    checkNotNull (nameof stack) stack
    ListChecks.positive (nameof count) count

    Array.splitInto count (Snapshot.ofSeq (nameof stack) stack)
    |> Array.map ofArrayUnchecked
    |> ofArrayUnchecked

[<CompiledName "Take">]
let take count (stack : IStack<'T>) =
    checkNotNull (nameof stack) stack
    // A stack shorter than the count stops the counting at its length, which the exception reports
    ListChecks.take (nameof count) count (Lengths.stackUpTo count stack)
    let struct (top, rest) = splitTop count stack
    if rest.IsEmpty then stack else rebuild stack top

[<CompiledName "TakeLast">]
let takeLast count (stack : IStack<'T>) =
    checkNotNull (nameof stack) stack
    let length = Lengths.stackUpTo Int32.MaxValue stack
    // The checks of List.take
    ListChecks.take (nameof count) count length
    dropTop (length - count) stack

[<CompiledName "TakeWhile">]
let takeWhile (predicate : 'T -> bool) (stack : IStack<'T>) =
    checkNotNull (nameof stack) stack
    let taken = ResizeArray<'T>()
    let mutable node = stack
    let mutable go = true

    while go && not node.IsEmpty do
        let item = node.Peek ()

        if predicate item then
            taken.Add item
            node <- node.Pop ()
        else
            go <- false

    if node.IsEmpty then
        stack
    else
        rebuild stack (taken.ToArray ())

[<CompiledName "Truncate">]
let truncate count (stack : IStack<'T>) =
    checkNotNull (nameof stack) stack
    let struct (top, rest) = splitTop (Lengths.stackUpTo count stack) stack
    if rest.IsEmpty then stack else rebuild stack top

// ---- Sorting ----

// The sorts are stable, as the sorts of List are (ADR 0001, D5). The functions that compare the elements, or that call the
// projection in the comparison as List.sortByDescending does, sort with the Seq functions of FSharp.Core, which share the
// stable sort of List: the same comparisons run in Array.Sort, which wraps the exception of a comparison in an
// InvalidOperationException (D6). Seq.sortDescending and Seq.sortByDescending are inline and box the elements in every
// comparison once they are compiled into a generic function, so the two descending sorts call Seq.sortWith through
// DescendingSorts, with the cached comparer. sortBy and sortWithComparer sort with LINQ, which computes the keys first, as
// List.sortBy does, and wraps the exception of a comparison as Array.Sort does. The sorted elements go to an array, which
// the stack of the implementation of the input is pushed from. A stack of one element is returned without a call of the
// projection or the comparer, as List returns it.

[<CompiledName "Sort">]
let sort (stack : IStack<'T>) =
    checkNotNull (nameof stack) stack

    if Lengths.stackUpTo 2 stack <= 1 then
        stack
    else
        rebuild stack (Enumerable.ToArray (Seq.sort stack))

[<CompiledName "SortBy">]
let sortBy (projection : 'T -> 'Key) (stack : IStack<'T>) =
    checkNotNull (nameof stack) stack

    if Lengths.stackUpTo 2 stack <= 1 then
        stack
    else
        rebuild stack (stack.OrderBy(Func<'T, 'Key> projection, DefaultComparer<'Key>.Instance).ToArray())

[<CompiledName "SortByDescending">]
let sortByDescending (projection : 'T -> 'Key) (stack : IStack<'T>) =
    checkNotNull (nameof stack) stack

    if Lengths.stackUpTo 2 stack <= 1 then
        stack
    else
        rebuild stack (Enumerable.ToArray (DescendingSorts.sortByDescending projection stack))

[<CompiledName "SortDescending">]
let sortDescending (stack : IStack<'T>) =
    checkNotNull (nameof stack) stack

    if Lengths.stackUpTo 2 stack <= 1 then
        stack
    else
        rebuild stack (Enumerable.ToArray (DescendingSorts.sortDescending stack))

[<CompiledName "SortWith">]
let sortWith (comparer : 'T -> 'T -> int) (stack : IStack<'T>) =
    checkNotNull (nameof stack) stack

    if Lengths.stackUpTo 2 stack <= 1 then
        stack
    else
        rebuild stack (Enumerable.ToArray (Seq.sortWith comparer stack))

[<CompiledName "SortWithComparer">]
let sortWithComparer (comparer : Collections.Generic.IComparer<'T>) (stack : IStack<'T>) =
    checkNotNull (nameof stack) stack

    if Lengths.stackUpTo 2 stack <= 1 then
        stack
    else
        rebuild stack (stack.Order(comparer).ToArray())

// ---- Random ----

// The functions draw the random numbers as the List functions do, from a snapshot after the List checks: the Array
// functions throw other messages for an empty array

[<CompiledName "RandomChoice">]
let randomChoice (source : IStack<'T>) =
    let array = Snapshot.ofSeq (nameof source) source
    ListChecks.notEmptySequence (nameof source) array.Length
    array[Random.Shared.Next (0, array.Length)]

[<CompiledName "RandomChoiceBy">]
let randomChoiceBy (randomizer : unit -> float) (source : IStack<'T>) =
    let array = Snapshot.ofSeq (nameof source) source
    ListChecks.notEmptySequence (nameof source) array.Length
    array[ListChecks.randomIndex (nameof randomizer) randomizer array.Length]

[<CompiledName "RandomChoiceWith">]
let randomChoiceWith (random : Random) (source : IStack<'T>) =
    checkNotNull (nameof random) random
    let array = Snapshot.ofSeq (nameof source) source
    ListChecks.notEmptySequence (nameof source) array.Length
    array[random.Next (0, array.Length)]

[<CompiledName "RandomChoices">]
let randomChoices count (source : IStack<'T>) =
    let array = Snapshot.ofSeq (nameof source) source
    ListChecks.randomChoices (nameof count) count (nameof source) array.Length
    rebuild source (Array.randomChoices count array)

[<CompiledName "RandomChoicesBy">]
let randomChoicesBy (randomizer : unit -> float) count (source : IStack<'T>) =
    let array = Snapshot.ofSeq (nameof source) source
    ListChecks.randomChoices (nameof count) count (nameof source) array.Length
    rebuild source (Array.randomChoicesBy randomizer count array)

[<CompiledName "RandomChoicesWith">]
let randomChoicesWith (random : Random) count (source : IStack<'T>) =
    checkNotNull (nameof random) random
    let array = Snapshot.ofSeq (nameof source) source
    ListChecks.randomChoices (nameof count) count (nameof source) array.Length
    rebuild source (Array.randomChoicesWith random count array)

[<CompiledName "RandomSample">]
let randomSample count (source : IStack<'T>) =
    let array = Snapshot.ofSeq (nameof source) source
    ListChecks.randomSample (nameof count) count (nameof source) array.Length
    rebuild source (Array.randomSample count array)

[<CompiledName "RandomSampleBy">]
let randomSampleBy (randomizer : unit -> float) count (source : IStack<'T>) =
    let array = Snapshot.ofSeq (nameof source) source
    ListChecks.randomSample (nameof count) count (nameof source) array.Length
    rebuild source (Array.randomSampleBy randomizer count array)

[<CompiledName "RandomSampleWith">]
let randomSampleWith (random : Random) count (source : IStack<'T>) =
    checkNotNull (nameof random) random
    let array = Snapshot.ofSeq (nameof source) source
    ListChecks.randomSample (nameof count) count (nameof source) array.Length
    rebuild source (Array.randomSampleWith random count array)

[<CompiledName "RandomShuffle">]
let randomShuffle (source : IStack<'T>) =
    let array = Snapshot.ofSeq (nameof source) source
    Array.randomShuffleInPlace array
    rebuild source array

[<CompiledName "RandomShuffleBy">]
let randomShuffleBy (randomizer : unit -> float) (source : IStack<'T>) =
    let array = Snapshot.ofSeq (nameof source) source
    Array.randomShuffleInPlaceBy randomizer array
    rebuild source array

[<CompiledName "RandomShuffleWith">]
let randomShuffleWith (random : Random) (source : IStack<'T>) =
    checkNotNull (nameof random) random
    let array = Snapshot.ofSeq (nameof source) source
    Array.randomShuffleInPlaceWith random array
    rebuild source array

// ---- Conversion ----

[<CompiledName "ToArray">]
let toArray (stack : IStack<'T>) = Snapshot.ofSeq (nameof stack) stack

[<CompiledName "ToList">]
let toList (stack : IStack<'T>) =
    checkNotNull (nameof stack) stack
    List.ofSeq stack

[<CompiledName "ToSeq">]
let toSeq (stack : IStack<'T>) =
    checkNotNull (nameof stack) stack
    stack :> seq<'T>

// ---- Type-specific ----

[<CompiledName "Clear">]
let clear (stack : IStack<'T>) =
    checkNotNull (nameof stack) stack
    stack.Clear ()
