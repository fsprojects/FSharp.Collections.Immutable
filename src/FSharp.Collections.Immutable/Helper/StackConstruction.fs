/// <summary>
/// The construction of the stacks that the stack modules and the <c>stack { }</c> expression return.
/// </summary>
/// <remarks>
/// <see cref="M:System.Collections.Immutable.ImmutableStack.CreateRange``1(System.Collections.Generic.IEnumerable{``0})"/>
/// pushes the elements in order and leaves the last element on top. The stack modules keep the first element on top
/// instead, so that a stack enumerates its elements in the order in which it was created (ADR 0001, D5). The module serves
/// the stack modules, their <c>Tuple</c> counterparts and the computation expression, so that they build their stacks with
/// one implementation.
/// </remarks>
module internal FSharp.Collections.Immutable.StackConstruction

open System.Collections.Immutable

/// <summary>
/// Pushes the elements of an array onto a stack, from the last element to the first one.
/// </summary>
/// <param name="items">The elements; neither checked for <see langword="null"/> nor kept.</param>
/// <param name="stack">
/// The stack to push the elements onto, an <see cref="T:System.Collections.Immutable.ImmutableStack`1"/> or an
/// <see cref="T:System.Collections.Immutable.IImmutableStack`1"/>.
/// </param>
/// <returns>A stack of the elements of the array, the first one on top, followed by the elements of the stack.</returns>
/// <remarks>
/// O(m), where m is the length of the array. The result shares the nodes of the stack and keeps its implementation.
/// </remarks>
let inline pushArray (items : 'T array) (stack : ^Stack) : ^Stack when ^Stack : (member Push : 'T -> ^Stack) =
    // The constraint serves the concrete stack and the interface with one loop: each Push returns the type it is called
    // on, so the result keeps the implementation of the stack
    let mutable result = stack

    // Pushing from the last element leaves the first one on top, so that the result enumerates the array in its order
    for index = items.Length - 1 downto 0 do
        result <- (^Stack : (member Push : 'T -> ^Stack) (result, items[index]))

    result

/// <summary>
/// A stack whose top is the first element of an array, which enumerates the elements in the order of the array.
/// </summary>
/// <param name="array">The elements; neither checked for <see langword="null"/> nor kept.</param>
/// <returns>The stack of the elements.</returns>
/// <remarks>
/// O(n). The function pushes the elements onto the empty stack from the last one to the first one.
/// </remarks>
let ofArrayUnchecked (array : 'T array) : ImmutableStack<'T> = pushArray array ImmutableStack<'T>.Empty
