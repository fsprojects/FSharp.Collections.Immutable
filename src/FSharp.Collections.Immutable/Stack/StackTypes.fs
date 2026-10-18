namespace FSharp.Collections.Immutable

open System.Collections.Immutable

// The abbreviations have their own file because the modules of the folder are root modules, which cannot declare a type
// of the namespace; a file of abbreviations needs no signature file (ADR 0001, D2).

/// <summary>
/// An immutable stack, <see cref="T:System.Collections.Immutable.ImmutableStack`1"/>.
/// </summary>
/// <remarks>
/// The stack is a linked list of nodes: pushing, reading and popping the top element take O(1), and a stack shares the
/// nodes of the stack that it was pushed onto.
/// </remarks>
type Stack<'T> = ImmutableStack<'T>

/// <summary>
/// An immutable stack of any implementation, <see cref="T:System.Collections.Immutable.IImmutableStack`1"/>.
/// </summary>
/// <remarks>
/// <see cref="T:System.Collections.Immutable.ImmutableStack`1"/> implements the interface.
/// </remarks>
type IStack<'T> = IImmutableStack<'T>
