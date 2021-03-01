namespace FSharp.Collections.Immutable

type IStack<'T> = System.Collections.Immutable.IImmutableStack<'T>

type Stack<'T> = System.Collections.Immutable.ImmutableStack<'T>

[<RequireQualifiedAccess; CompiledName((nameof System.Collections.Immutable.ImmutableStack) + "Module")>]
module Stack =
    type internal StackFactory = System.Collections.Immutable.ImmutableStack

    let inline internal check (stack : IStack<_>) = checkNotNull (nameof stack) stack

    let inline empty<'T> = StackFactory.Create<'T>()

    let inline ofSeq source = StackFactory.CreateRange source

    let inline ofArray (array : 'T []) : Stack<'T> = ofSeq array

    let inline toSeq (stack: IStack<_>) = stack :> seq<_>

    let push head stack : IStack<'T> =
        check stack
        stack.Push head

    let cons head stack = push head stack

    let peek stack =
        check stack
        stack.Peek()

    let head stack = peek stack

    let tail stack : IStack<_> =
        check stack
        stack.Pop()

    let pop stack =
        check stack
        stack.Peek(), tail stack

    let (|Cons|Nil|) stack =
        check stack
        if stack.IsEmpty then Nil
        else Cons(stack.Peek(), stack.Pop())

/////////////
module ImmutableStack = Stack
