namespace FSharp.Collections.Immutable.Tests

open System
open Microsoft.VisualStudio.TestTools.UnitTesting

/// <summary>
/// Measures what an action allocates, for the tests that bound the cost of a function.
/// </summary>
/// <remarks>
/// The bytes are those that the current thread allocates, as
/// <see cref="M:System.GC.GetAllocatedBytesForCurrentThread"/> counts them, so the tests that run in parallel do not
/// disturb each other. A limit should leave room for the closures and the enumerators of the action and stay far below
/// what the slow implementation allocates, so that it tells the two apart in a Debug build as well.
/// </remarks>
[<AbstractClass; Sealed>]
type AllocationAssert =

    /// <summary>
    /// The bytes that an action allocates on the current thread when it runs a second time, after the first run has
    /// compiled the code.
    /// </summary>
    /// <param name="action">The action to measure.</param>
    /// <returns>The number of allocated bytes.</returns>
    static member AllocatedBy (action : unit -> unit) : int64 =
        action ()
        let before = GC.GetAllocatedBytesForCurrentThread ()
        action ()
        GC.GetAllocatedBytesForCurrentThread () - before

    /// <summary>
    /// Asserts that an action allocates fewer bytes than a limit.
    /// </summary>
    /// <param name="limit">The number of bytes that the action must stay below.</param>
    /// <param name="action">The action to measure.</param>
    /// <param name="message">What the action had to do, as the message of the failure.</param>
    static member LessThan (limit : int64, action : unit -> unit, message : string) =
        Assert.IsLessThan (limit, AllocationAssert.AllocatedBy action, message)
