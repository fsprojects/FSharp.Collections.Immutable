namespace FSharp.Collections.Immutable.Tests

open System
open System.Collections
open System.Collections.Generic
open System.Collections.Immutable
open FSharp.Collections.Immutable
open Microsoft.VisualStudio.TestTools.UnitTesting

/// <summary>
/// An implementation of <see cref="T:System.Collections.Immutable.IImmutableStack`1"/> that is not an immutable stack, to
/// tell whether a function keeps the implementation of its input.
/// </summary>
/// <remarks>
/// Every member that returns a stack returns a tagged stack.
/// </remarks>
type TaggedStack<'T> (items : ImmutableStack<'T>) =

    static let wrap (items : ImmutableStack<'T>) = TaggedStack items :> IImmutableStack<'T>

    interface IImmutableStack<'T> with
        member _.Clear () = wrap (items.Clear ())
        member _.IsEmpty = items.IsEmpty
        member _.Peek () = items.Peek ()
        member _.Pop () = wrap (items.Pop ())
        member _.Push value = wrap (items.Push value)

    interface IEnumerable<'T> with
        member _.GetEnumerator () = (items :> IEnumerable<'T>).GetEnumerator()

    interface IEnumerable with
        member _.GetEnumerator () = (items :> IEnumerable).GetEnumerator()

/// The behaviour of `IStack` that the shared contracts do not cover: the implementation of the results, the order of the
/// elements, null inputs and the patterns.
[<TestClass; IStackTestCategory; PatternMatchingTestCategory>]
type IStackTests () =

    static let nullStack = Unchecked.defaultof<IImmutableStack<int>>

    static let several () : IImmutableStack<int> = Stack.ofArray Inputs.Several

    static let tagged () : IImmutableStack<int> = TaggedStack (Stack.ofArray Inputs.Several)

    // Each call draws from a new generator with the same seed, so that both implementations get the same choices
    static let random () = Random 42

    static let randomizer () =
        let random = Random 42
        fun () -> random.NextDouble ()

    static let first (struct (first : IImmutableStack<int>, _ : IImmutableStack<int>)) = first

    static let second (struct (_ : IImmutableStack<int>, second : IImmutableStack<int>)) = second

    /// The functions whose result has the element type of their input, by name, as functions of the input. The rows of
    /// the test are the names: a function has no readable name in a row.
    static let sameElementType : Map<string, IImmutableStack<int> -> IImmutableStack<int>> =
        Map.ofArray [|
            nameof IStack.tail, IStack.tail
            nameof IStack.tryTail, (fun stack -> (IStack.tryTail stack).Value)
            (nameof IStack.uncons,
             fun stack ->
                 let struct (_, tail) = IStack.uncons stack
                 tail)
            // The second stack receives the elements of the first one, so the result has its implementation (ADR 0001, D14)
            nameof IStack.append, (fun stack -> IStack.append (Stack.ofArray [| 1; 2 |]) stack)
            nameof IStack.cons, IStack.cons 99
            nameof IStack.consMany, IStack.consMany [| 98; 99 |]
            nameof IStack.filter, IStack.filter (fun item -> item > 0)
            nameof IStack.permute, IStack.permute (fun index -> (index + 3) % 8)
            nameof IStack.rev, IStack.rev
            nameof IStack.where, IStack.where (fun item -> item > 0)
            nameof IStack.distinct, IStack.distinct
            nameof IStack.distinctBy, IStack.distinctBy abs
            nameof IStack.except, IStack.except [| 5 |]
            $"%s{nameof IStack.partition} (accepted)", IStack.partition (fun item -> item > 0) >> first
            $"%s{nameof IStack.partition} (rejected)", IStack.partition (fun item -> item > 0) >> second
            nameof IStack.skip, IStack.skip 3
            nameof IStack.skipLast, IStack.skipLast 3
            nameof IStack.skipWhile, IStack.skipWhile (fun item -> item > 0)
            $"%s{nameof IStack.splitAt} (first part)", IStack.splitAt 3 >> first
            $"%s{nameof IStack.splitAt} (second part)", IStack.splitAt 3 >> second
            nameof IStack.take, IStack.take 3
            nameof IStack.takeLast, IStack.takeLast 3
            nameof IStack.takeWhile, IStack.takeWhile (fun item -> item > 0)
            nameof IStack.truncate, IStack.truncate 3
            nameof IStack.sort, IStack.sort
            nameof IStack.sortBy, IStack.sortBy abs
            nameof IStack.sortByDescending, IStack.sortByDescending abs
            nameof IStack.sortDescending, IStack.sortDescending
            nameof IStack.sortWith, IStack.sortWith compare
            nameof IStack.sortWithComparer, IStack.sortWithComparer Comparer<int>.Default
            nameof IStack.randomChoicesWith, (fun stack -> IStack.randomChoicesWith (random ()) 3 stack)
            nameof IStack.randomChoicesBy, (fun stack -> IStack.randomChoicesBy (randomizer ()) 3 stack)
            nameof IStack.randomSampleWith, (fun stack -> IStack.randomSampleWith (random ()) 3 stack)
            nameof IStack.randomSampleBy, (fun stack -> IStack.randomSampleBy (randomizer ()) 3 stack)
            nameof IStack.randomShuffleWith, (fun stack -> IStack.randomShuffleWith (random ()) stack)
            nameof IStack.randomShuffleBy, (fun stack -> IStack.randomShuffleBy (randomizer ()) stack)
            nameof IStack.clear, IStack.clear
        |]

    /// The functions that take a stack, by name, with the parameter that names the null stack and the function on a null
    /// stack.
    static let nullStackRejections =
        let stack = several ()
        let pairs = Unchecked.defaultof<IImmutableStack<struct (int * int)>>
        let triples = Unchecked.defaultof<IImmutableStack<struct (int * int * int)>>
        let floats = Unchecked.defaultof<IImmutableStack<float>>

        [|
            struct (nameof IStack.isEmpty, "stack", (fun () -> IStack.isEmpty nullStack |> ignore))
            struct (nameof IStack.length, "stack", (fun () -> IStack.length nullStack |> ignore))
            struct (nameof IStack.exactlyOne, "stack", (fun () -> IStack.exactlyOne nullStack |> ignore))
            struct (nameof IStack.head, "stack", (fun () -> IStack.head nullStack |> ignore))
            struct (nameof IStack.last, "stack", (fun () -> IStack.last nullStack |> ignore))
            struct (nameof IStack.tail, "stack", (fun () -> IStack.tail nullStack |> ignore))
            struct (nameof IStack.tryExactlyOne, "stack", (fun () -> IStack.tryExactlyOne nullStack |> ignore))
            struct (nameof IStack.tryHead, "stack", (fun () -> IStack.tryHead nullStack |> ignore))
            struct (nameof IStack.tryLast, "stack", (fun () -> IStack.tryLast nullStack |> ignore))
            struct (nameof IStack.tryTail, "stack", (fun () -> IStack.tryTail nullStack |> ignore))
            struct (nameof IStack.tryUncons, "stack", (fun () -> IStack.tryUncons nullStack |> ignore))
            struct (nameof IStack.uncons, "stack", (fun () -> IStack.uncons nullStack |> ignore))
            struct (nameof IStack.(|Cons|_|), "stack", (fun () -> IStack.(|Cons|_|) nullStack |> ignore))
            struct (nameof IStack.(|Nil|_|), "stack", (fun () -> IStack.(|Nil|_|) nullStack |> ignore))
            struct (nameof IStack.contains, "source", (fun () -> IStack.contains 1 nullStack |> ignore))
            struct (nameof IStack.exists, "stack", (fun () -> IStack.exists ((=) 1) nullStack |> ignore))
            struct (nameof IStack.exists2, "stack1", (fun () -> IStack.exists2 (=) nullStack stack |> ignore))
            struct (nameof IStack.find, "stack", (fun () -> IStack.find ((=) 1) nullStack |> ignore))
            struct (nameof IStack.findBack, "stack", (fun () -> IStack.findBack ((=) 1) nullStack |> ignore))
            struct (nameof IStack.findIndex, "stack", (fun () -> IStack.findIndex ((=) 1) nullStack |> ignore))
            struct (nameof IStack.findIndexBack, "stack", (fun () -> IStack.findIndexBack ((=) 1) nullStack |> ignore))
            struct (nameof IStack.forall, "stack", (fun () -> IStack.forall ((=) 1) nullStack |> ignore))
            struct (nameof IStack.forall2, "stack2", (fun () -> IStack.forall2 (=) stack nullStack |> ignore))
            struct (nameof IStack.pick, "stack", (fun () -> IStack.pick ValueSome nullStack |> ignore))
            struct (nameof IStack.tryFind, "stack", (fun () -> IStack.tryFind ((=) 1) nullStack |> ignore))
            struct (nameof IStack.tryFindBack, "stack", (fun () -> IStack.tryFindBack ((=) 1) nullStack |> ignore))
            struct (nameof IStack.tryFindIndex, "stack", (fun () -> IStack.tryFindIndex ((=) 1) nullStack |> ignore))
            struct (nameof IStack.tryFindIndexBack, "stack", (fun () -> IStack.tryFindIndexBack ((=) 1) nullStack |> ignore))
            struct (nameof IStack.tryPick, "stack", (fun () -> IStack.tryPick ValueSome nullStack |> ignore))
            struct (nameof IStack.allPairs, "stack2", (fun () -> IStack.allPairs stack nullStack |> ignore))
            struct (nameof IStack.append, "stack1", (fun () -> IStack.append nullStack stack |> ignore))
            struct (nameof IStack.choose, "stack", (fun () -> IStack.choose ValueSome nullStack |> ignore))
            struct (nameof IStack.chunkBySize, "stack", (fun () -> IStack.chunkBySize 2 nullStack |> ignore))
            struct (nameof IStack.collect, "stack", (fun () -> IStack.collect (fun _ -> stack) nullStack |> ignore))
            struct (nameof IStack.concat, "stacks", (fun () -> IStack.concat [| stack; nullStack |] |> ignore))
            struct (nameof IStack.cons, "stack", (fun () -> IStack.cons 1 nullStack |> ignore))
            struct (nameof IStack.consMany, "stack", (fun () -> IStack.consMany [| 1 |] nullStack |> ignore))
            struct (nameof IStack.filter, "stack", (fun () -> IStack.filter ((=) 1) nullStack |> ignore))
            struct (nameof IStack.indexed, "stack", (fun () -> IStack.indexed nullStack |> ignore))
            struct (nameof IStack.map, "stack", (fun () -> IStack.map id nullStack |> ignore))
            struct (nameof IStack.map2, "stack1", (fun () -> IStack.map2 (+) nullStack stack |> ignore))
            struct (nameof IStack.map3,
                    "stack2",
                    (fun () ->
                        IStack.map3 (fun a b c -> a + b + c) stack nullStack stack
                        |> ignore
                    ))
            struct (nameof IStack.mapi, "stack", (fun () -> IStack.mapi (+) nullStack |> ignore))
            struct (nameof IStack.mapi2,
                    "stack2",
                    (fun () ->
                        IStack.mapi2 (fun index a b -> index + a + b) stack nullStack
                        |> ignore
                    ))
            struct (nameof IStack.pairwise, "stack", (fun () -> IStack.pairwise nullStack |> ignore))
            struct (nameof IStack.permute, "stack", (fun () -> IStack.permute id nullStack |> ignore))
            struct (nameof IStack.rev, "stack", (fun () -> IStack.rev nullStack |> ignore))
            struct (nameof IStack.transpose, "stacks", (fun () -> IStack.transpose [| nullStack |] |> ignore))
            struct (nameof IStack.unzip, "stack", (fun () -> IStack.unzip pairs |> ignore))
            struct (nameof IStack.unzip3, "stack", (fun () -> IStack.unzip3 triples |> ignore))
            struct (nameof IStack.where, "stack", (fun () -> IStack.where ((=) 1) nullStack |> ignore))
            struct (nameof IStack.windowed, "stack", (fun () -> IStack.windowed 2 nullStack |> ignore))
            struct (nameof IStack.zip, "stack2", (fun () -> IStack.zip stack nullStack |> ignore))
            struct (nameof IStack.zip3, "stack3", (fun () -> IStack.zip3 stack stack nullStack |> ignore))
            struct (nameof IStack.average, "stack", (fun () -> IStack.average floats |> ignore))
            struct (nameof IStack.averageBy, "stack", (fun () -> IStack.averageBy float nullStack |> ignore))
            struct (nameof IStack.compareWith, "stack1", (fun () -> IStack.compareWith compare nullStack stack |> ignore))
            struct (nameof IStack.fold, "stack", (fun () -> IStack.fold (+) 0 nullStack |> ignore))
            struct (nameof IStack.fold2,
                    "stack1",
                    (fun () ->
                        IStack.fold2 (fun state a b -> state + a + b) 0 nullStack stack
                        |> ignore
                    ))
            struct (nameof IStack.foldBack, "stack", (fun () -> IStack.foldBack (+) nullStack 0 |> ignore))
            struct (nameof IStack.foldBack2,
                    "stack2",
                    (fun () ->
                        IStack.foldBack2 (fun a b state -> state + a + b) stack nullStack 0
                        |> ignore
                    ))
            struct (nameof IStack.iter, "stack", (fun () -> IStack.iter ignore nullStack))
            struct (nameof IStack.iter2, "stack1", (fun () -> IStack.iter2 (fun _ _ -> ()) nullStack stack))
            struct (nameof IStack.iteri, "stack", (fun () -> IStack.iteri (fun _ _ -> ()) nullStack))
            struct (nameof IStack.iteri2, "stack2", (fun () -> IStack.iteri2 (fun _ _ _ -> ()) stack nullStack))
            struct (nameof IStack.mapFold,
                    "stack",
                    (fun () ->
                        IStack.mapFold (fun state item -> struct (item, state)) 0 nullStack
                        |> ignore
                    ))
            struct (nameof IStack.mapFoldBack,
                    "stack",
                    (fun () ->
                        IStack.mapFoldBack (fun item state -> struct (item, state)) nullStack 0
                        |> ignore
                    ))
            struct (nameof IStack.max, "stack", (fun () -> IStack.max nullStack |> ignore))
            struct (nameof IStack.maxBy, "stack", (fun () -> IStack.maxBy id nullStack |> ignore))
            struct (nameof IStack.min, "stack", (fun () -> IStack.min nullStack |> ignore))
            struct (nameof IStack.minBy, "stack", (fun () -> IStack.minBy id nullStack |> ignore))
            struct (nameof IStack.reduce, "stack", (fun () -> IStack.reduce (+) nullStack |> ignore))
            struct (nameof IStack.reduceBack, "stack", (fun () -> IStack.reduceBack (+) nullStack |> ignore))
            struct (nameof IStack.scan, "stack", (fun () -> IStack.scan (+) 0 nullStack |> ignore))
            struct (nameof IStack.scanBack, "stack", (fun () -> IStack.scanBack (+) nullStack 0 |> ignore))
            struct (nameof IStack.sum, "stack", (fun () -> IStack.sum nullStack |> ignore))
            struct (nameof IStack.sumBy, "stack", (fun () -> IStack.sumBy id nullStack |> ignore))
            struct (nameof IStack.countBy, "stack", (fun () -> IStack.countBy id nullStack |> ignore))
            struct (nameof IStack.distinct, "stack", (fun () -> IStack.distinct nullStack |> ignore))
            struct (nameof IStack.distinctBy, "stack", (fun () -> IStack.distinctBy id nullStack |> ignore))
            struct (nameof IStack.except, "stack", (fun () -> IStack.except [| 1 |] nullStack |> ignore))
            struct (nameof IStack.groupBy, "stack", (fun () -> IStack.groupBy id nullStack |> ignore))
            struct (nameof IStack.partition, "stack", (fun () -> IStack.partition ((=) 1) nullStack |> ignore))
            struct (nameof IStack.partitionWith, "stack", (fun () -> IStack.partitionWith Choice1Of2 nullStack |> ignore))
            struct (nameof IStack.skip, "stack", (fun () -> IStack.skip 0 nullStack |> ignore))
            struct (nameof IStack.skipLast, "stack", (fun () -> IStack.skipLast 0 nullStack |> ignore))
            struct (nameof IStack.skipWhile, "stack", (fun () -> IStack.skipWhile ((=) 1) nullStack |> ignore))
            struct (nameof IStack.splitAt, "stack", (fun () -> IStack.splitAt 0 nullStack |> ignore))
            struct (nameof IStack.splitInto, "stack", (fun () -> IStack.splitInto 2 nullStack |> ignore))
            struct (nameof IStack.take, "stack", (fun () -> IStack.take 0 nullStack |> ignore))
            struct (nameof IStack.takeLast, "stack", (fun () -> IStack.takeLast 0 nullStack |> ignore))
            struct (nameof IStack.takeWhile, "stack", (fun () -> IStack.takeWhile ((=) 1) nullStack |> ignore))
            struct (nameof IStack.truncate, "stack", (fun () -> IStack.truncate 1 nullStack |> ignore))
            struct (nameof IStack.sort, "stack", (fun () -> IStack.sort nullStack |> ignore))
            struct (nameof IStack.sortBy, "stack", (fun () -> IStack.sortBy id nullStack |> ignore))
            struct (nameof IStack.sortByDescending, "stack", (fun () -> IStack.sortByDescending id nullStack |> ignore))
            struct (nameof IStack.sortDescending, "stack", (fun () -> IStack.sortDescending nullStack |> ignore))
            struct (nameof IStack.sortWith, "stack", (fun () -> IStack.sortWith compare nullStack |> ignore))
            struct (nameof IStack.sortWithComparer,
                    "stack",
                    (fun () ->
                        IStack.sortWithComparer Comparer<int>.Default nullStack
                        |> ignore
                    ))
            struct (nameof IStack.randomChoice, "source", (fun () -> IStack.randomChoice nullStack |> ignore))
            struct (nameof IStack.randomChoiceBy, "source", (fun () -> IStack.randomChoiceBy (fun () -> 0.5) nullStack |> ignore))
            struct (nameof IStack.randomChoiceWith, "source", (fun () -> IStack.randomChoiceWith (Random 1) nullStack |> ignore))
            struct (nameof IStack.randomChoices, "source", (fun () -> IStack.randomChoices 1 nullStack |> ignore))
            struct (nameof IStack.randomChoicesBy,
                    "source",
                    (fun () -> IStack.randomChoicesBy (fun () -> 0.5) 1 nullStack |> ignore))
            struct (nameof IStack.randomChoicesWith,
                    "source",
                    (fun () -> IStack.randomChoicesWith (Random 1) 1 nullStack |> ignore))
            struct (nameof IStack.randomSample, "source", (fun () -> IStack.randomSample 1 nullStack |> ignore))
            struct (nameof IStack.randomSampleBy,
                    "source",
                    (fun () -> IStack.randomSampleBy (fun () -> 0.5) 1 nullStack |> ignore))
            struct (nameof IStack.randomSampleWith, "source", (fun () -> IStack.randomSampleWith (Random 1) 1 nullStack |> ignore))
            struct (nameof IStack.randomShuffle, "source", (fun () -> IStack.randomShuffle nullStack |> ignore))
            struct (nameof IStack.randomShuffleBy,
                    "source",
                    (fun () -> IStack.randomShuffleBy (fun () -> 0.5) nullStack |> ignore))
            struct (nameof IStack.randomShuffleWith, "source", (fun () -> IStack.randomShuffleWith (Random 1) nullStack |> ignore))
            struct (nameof IStack.toArray, "stack", (fun () -> IStack.toArray nullStack |> ignore))
            struct (nameof IStack.toList, "stack", (fun () -> IStack.toList nullStack |> ignore))
            struct (nameof IStack.toSeq, "stack", (fun () -> IStack.toSeq nullStack |> ignore))
            struct (nameof IStack.clear, "stack", (fun () -> IStack.clear nullStack |> ignore))
        |]
        |> Array.map (fun struct (name, paramName, action) -> name, struct (paramName, action))
        |> Map.ofArray

    /// The functions that take a sequence, by name and by the input that they reject, with the parameter that names the
    /// input and the function on it. The shared contracts cover a null sequence and a default flat list given as a
    /// sequence of integers for concat, transpose and except; the rows are consMany, which no contract covers, a default
    /// flat list of strings given as a sequence of objects, which a type test for the flat list of objects does not see,
    /// and the order of the checks. A default flat list converts to a sequence without a check and fails when the sequence
    /// is enumerated, so the other inputs are valid.
    static let sequenceRejections : Map<string, struct (string * (unit -> unit))> =
        let widenedItems = FlatListOracles.defaultStringsAsObjects
        let objects = Stack.ofArray [| "a" :> obj |] :> IImmutableStack<obj>
        let defaultItems = Inputs.DefaultFlatListAsSeq<int>()

        Map.ofArray [|
            "consMany (default flat list)", struct ("values", (fun () -> IStack.consMany defaultItems (several ()) |> ignore))
            "consMany (default flat list of strings as objects)",
            struct ("values", (fun () -> IStack.consMany widenedItems objects |> ignore))
            "except (default flat list of strings as objects)",
            struct ("itemsToExclude", (fun () -> IStack.except widenedItems objects |> ignore))
            // List.except checks the items to exclude before the list
            "except (default flat list and null stack)",
            struct ("itemsToExclude", (fun () -> IStack.except defaultItems nullStack |> ignore))
        |]

    /// The names of the functions whose result has the element type of their input, as the rows of their test.
    static member SameElementType = sameElementType.Keys

    /// The names of the functions that take a stack, as the rows of their test.
    static member NullStackRejections = nullStackRejections.Keys

    /// The functions that take a sequence and the inputs that they reject, as the rows of their test.
    static member SequenceRejections = sequenceRejections.Keys

    [<TestMethod; DynamicData(nameof IStackTests.SameElementType)>]
    member _.``functions with the element type of their input keep its implementation`` (name : string) =
        let apply = sameElementType[name]
        let result = apply (tagged ())
        Assert.IsInstanceOfType<TaggedStack<int>>(result, $"IStack.%s{name} must keep the implementation of its input")
        |> ignore

        CollectionAssert.AreEqual (
            IStack.toArray (apply (several ())),
            IStack.toArray result,
            $"IStack.%s{name} must return the same elements for every implementation"
        )

    [<TestMethod; DataRow(0, 0); DataRow(0, 3); DataRow(2, 0); DataRow(2, 3)>]
    member _.``append pushes the first stack onto the second one, whose implementation and nodes it keeps``
        (length1 : int, length2 : int)
        =
        // The first stack has another implementation, so that the result shows whose implementation it keeps, also when
        // the second stack is empty
        let first : IImmutableStack<int> =
            TaggedStack (Stack.ofArray (Array.init length1 (fun index -> 10 + index)))
        let second : IImmutableStack<int> = Stack.ofArray (Array.init length2 id)
        let appended = IStack.append first second

        Assert.IsInstanceOfType<ImmutableStack<int>>(appended, "IStack.append must keep the implementation of the second stack")
        |> ignore

        CollectionAssert.AreEqual (
            Array.append (IStack.toArray first) (IStack.toArray second),
            IStack.toArray appended,
            "IStack.append must keep the elements of the first stack on top of the elements of the second one"
        )

        // List.append shares its second list in the same way; an empty first stack gives the second stack itself
        Assert.AreSame (
            second,
            IStack.skip length1 appended,
            "IStack.append must push the first stack onto the second one, whose nodes the result shares"
        )

    [<TestMethod>]
    member _.``functions that combine stacks or change the element type return a stack of the elements`` () =
        let stack = tagged ()

        CollectionAssert.AreEqual (
            Array.map string Inputs.Several,
            Stack.toArray (IStack.map string stack),
            "IStack.map must map the elements of any implementation into a stack, in their order"
        )

        let concatenated = IStack.concat [| stack; several () |]

        CollectionAssert.AreEqual (
            Array.append Inputs.Several Inputs.Several,
            Stack.toArray concatenated,
            "IStack.concat must concatenate stacks of different implementations into a stack, in their order"
        )

    [<TestMethod>]
    member _.``the functions read any implementation from the top`` () =
        let stack = tagged ()

        Assert.AreEqual<int>(Array.head Inputs.Several, IStack.head stack, "IStack.head must return the top")
        Assert.AreEqual<int>(Array.last Inputs.Several, IStack.last stack, "IStack.last must return the bottom")
        Assert.AreEqual<int>(Array.length Inputs.Several, IStack.length stack, "IStack.length must count the elements")
        CollectionAssert.AreEqual (Inputs.Several, IStack.toArray stack, "IStack.toArray must copy the elements from the top")

        CollectionAssert.AreEqual (
            Array.rev Inputs.Several,
            IStack.toArray (IStack.rev stack),
            "IStack.rev must put the last element on top"
        )

    [<TestMethod>]
    member _.``the sorts order strings in F# generic comparison, which is ordinal`` () =
        // A culture-sensitive comparer, such as Comparer<string>.Default, puts "a" before "B"
        let input = [| "b"; "A"; "a"; "B" |]
        let stack : IImmutableStack<string> = Stack.ofArray input

        CollectionAssert.AreEqual (
            Array.sort input,
            IStack.toArray (IStack.sort stack),
            "IStack.sort must order as Array.sort does"
        )

        CollectionAssert.AreEqual (
            Array.sortDescending input,
            IStack.toArray (IStack.sortDescending stack),
            "IStack.sortDescending must order as Array.sortDescending does"
        )

        CollectionAssert.AreEqual (
            Array.sortBy id input,
            IStack.toArray (IStack.sortBy id stack),
            "IStack.sortBy must order the keys as Array.sortBy does"
        )

        CollectionAssert.AreEqual (
            Array.sortByDescending id input,
            IStack.toArray (IStack.sortByDescending id stack),
            "IStack.sortByDescending must order the keys as Array.sortByDescending does"
        )

    [<TestMethod; DynamicData(nameof IStackTests.NullStackRejections)>]
    member _.``functions reject a null stack`` (name : string) =
        let struct (paramName, action) = nullStackRejections[name]

        let ex =
            Assert.ThrowsExactly<ArgumentNullException>(Action action, $"IStack.%s{name} must reject a null stack")

        Assert.AreEqual<string | null>(paramName, ex.ParamName, $"IStack.%s{name} must name the parameter of the null stack")

    [<TestMethod>]
    member _.``consMany rejects null values with the name of the parameter`` () =
        let ex =
            Assert.ThrowsExactly<ArgumentNullException>(
                Action (fun () ->
                    IStack.consMany (Unchecked.defaultof<seq<int>>) (several ())
                    |> ignore
                ),
                "IStack.consMany must reject null values"
            )

        Assert.AreEqual<string | null>("values", ex.ParamName, "IStack.consMany must name the values parameter")

    [<TestMethod; DynamicData(nameof IStackTests.SequenceRejections)>]
    member _.``functions that take a sequence reject a default flat list and check their inputs in order`` (name : string) =
        let struct (paramName, actual) = sequenceRejections[name]
        FlatListAssert.RejectsNull ($"IStack.%s{name}", paramName, actual)

    [<TestMethod>]
    member _.``a flat list that is not default passes as a sequence`` () =
        let flat = FlatList.ofArray [| 1; 2; 3 |] :> seq<int>

        let stacks =
            FlatList.ofArray [| Stack.ofArray [| 1; 2 |] :> IStack<int>; Stack.ofArray [| 3; 4 |] :> IStack<int> |]
            :> seq<IStack<int>>

        let single = IStack.cons 99 (IStack.clear (tagged ()))

        CollectionAssert.AreEqual (
            [| 1; 2; 3; 99 |],
            IStack.toArray (IStack.consMany flat single),
            "IStack.consMany must take the elements of a flat list"
        )

        CollectionAssert.AreEqual (
            [| 99 |],
            IStack.toArray (IStack.except flat (IStack.cons 2 single)),
            "IStack.except must take the elements of a flat list"
        )

        CollectionAssert.AreEqual (
            [| 1; 2; 3; 4 |],
            Stack.toArray (IStack.concat stacks),
            "IStack.concat must take the stacks of a flat list"
        )

        CollectionAssert.AreEqual (
            [| 1; 3 |],
            Stack.toArray (Stack.head (IStack.transpose stacks)),
            "IStack.transpose must take the stacks of a flat list"
        )

    [<TestMethod>]
    member _.``a flat list of strings that is not default passes as a sequence of objects`` () =
        let widened = FlatListOracles.stringsAsObjects (FlatList.ofArray [| "a"; "b" |])
        let asObjects (strings : string array) = strings |> Array.map (fun item -> item :> obj)
        let objects = Stack.ofArray (asObjects [| "c" |]) :> IStack<obj>

        CollectionAssert.AreEqual (
            asObjects [| "a"; "b"; "c" |],
            IStack.toArray (IStack.consMany widened objects),
            "IStack.consMany must take the elements of a flat list of strings as objects"
        )

        CollectionAssert.AreEqual (
            asObjects [| "c" |],
            IStack.toArray (IStack.except widened (Stack.ofArray (asObjects [| "a"; "c"; "b" |]))),
            "IStack.except must take the elements of a flat list of strings as objects"
        )

    [<TestMethod>]
    member _.``uncons throws for an empty stack as List.head does`` () =
        ExceptionAssert.Same (
            StackOps.paramName,
            (fun () -> List.head List.empty<int> |> ignore),
            (fun () -> IStack.uncons (IStack.clear (tagged ())) |> ignore),
            "IStack.uncons of an empty stack must throw as List.head does"
        )

    [<TestMethod>]
    member _.``cons and consMany push the values on top`` () =
        CollectionAssert.AreEqual (
            Array.append [| 99 |] Inputs.Several,
            IStack.toArray (IStack.cons 99 (tagged ())),
            "IStack.cons must add the value before the first element"
        )

        CollectionAssert.AreEqual (
            Array.append [| 98; 99 |] Inputs.Several,
            IStack.toArray (IStack.consMany [| 98; 99 |] (tagged ())),
            "IStack.consMany must add the values before the elements, the first value on top"
        )

    [<TestMethod>]
    member _.``Cons matches the top and the stack below it`` () =
        let rec elements (stack : IImmutableStack<int>) =
            match stack with
            | IStack.Cons (struct (head, tail)) -> Array.append [| head |] (elements tail)
            | _ -> [||]

        CollectionAssert.AreEqual (Inputs.Several, elements (tagged ()), "IStack.Cons must match the elements from the top")

        match tagged () with
        | IStack.Cons (struct (_, tail)) ->
            Assert.IsInstanceOfType<TaggedStack<int>>(tail, "IStack.Cons must keep the implementation of the tail")
            |> ignore
        | _ -> Assert.Fail "IStack.Cons must match a stack with elements"

    [<TestMethod>]
    member _.``Nil matches an empty stack only`` () =
        let describe (stack : IImmutableStack<int>) =
            match stack with
            | IStack.Nil -> "empty"
            | IStack.Cons (struct (head, _)) -> $"%d{head} on top"
            | _ -> "unreachable"

        Assert.AreEqual<string>("empty", describe (IStack.clear (tagged ())), "IStack.Nil must match an empty stack")
        Assert.AreEqual<string>("5 on top", describe (several ()), "IStack.Nil must not match a stack with elements")

    [<TestMethod>]
    member _.``contains compares in F# structural equality`` () =
        let arrays : IImmutableStack<int array> = Stack.singleton [| 1; 2 |]
        Assert.IsTrue (
            IStack.contains [| 1; 2 |] arrays,
            "IStack.contains must compare arrays by their elements, as List.contains does"
        )

    [<TestMethod>]
    [<DataRow(nameof IStack.cons, "Push")>]
    [<DataRow(nameof IStack.consMany, "PushMany")>]
    [<DataRow(nameof IStack.head, "Peek")>]
    [<DataRow(nameof IStack.tail, "Pop")>]
    [<DataRow(nameof IStack.tryHead, "TryPeek")>]
    [<DataRow(nameof IStack.tryTail, "TryPop")>]
    [<DataRow(nameof IStack.tryUncons, "TryPeekPop")>]
    [<DataRow(nameof IStack.uncons, "PeekPop")>]
    member _.``the stack functions compile with the names of the stack operations of the BCL``
        (name : string, compiledName : string)
        =
        Assert.AreEqual<string | null>(
            name,
            StackOps.sourceName "FSharp.Collections.Immutable.IStack" compiledName,
            "IStack must compile the function under the name of its stack operation in the BCL"
        )
