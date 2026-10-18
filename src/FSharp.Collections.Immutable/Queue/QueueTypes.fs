namespace FSharp.Collections.Immutable

open System.Collections.Immutable

// The abbreviations have their own file because the modules of the folder are root modules, which cannot declare a type
// of the namespace; a file of abbreviations needs no signature file (ADR 0001, D2).

/// <summary>
/// An immutable first-in, first-out queue, <see cref="T:System.Collections.Immutable.ImmutableQueue`1"/>.
/// </summary>
/// <remarks>
/// The queue keeps its elements in two immutable stacks: the front one, from which they leave, and the back one, onto
/// which they join. Enqueueing and reading the first element take O(1). Dequeueing takes amortized O(1): when the front
/// runs out, the queue reverses the back once. The queue does not store its length.
/// </remarks>
type Queue<'T> = ImmutableQueue<'T>

/// <summary>
/// An immutable queue of any implementation, <see cref="T:System.Collections.Immutable.IImmutableQueue`1"/>.
/// </summary>
/// <remarks>
/// <see cref="T:System.Collections.Immutable.ImmutableQueue`1"/> implements the interface.
/// </remarks>
type IQueue<'T> = IImmutableQueue<'T>
