namespace FSharp.Collections.Immutable

/// <summary>
/// Struct partial active patterns that match an <see cref="T:FSharp.Collections.Immutable.EquatableArray`1"/> by its
/// length. Code over the children of a syntax node can thus read like a list pattern without a conversion to an F# list.
/// <para>
/// A match does not allocate. Each length has its own partial pattern, because a multi-case active pattern returns a
/// <see cref="T:Microsoft.FSharp.Core.FSharpChoice`3"/> on the heap for each match. A length without a pattern of its
/// own falls through to the next case.
/// </para>
/// </summary>
/// <example>
/// <code lang="fsharp">
/// match arguments with
/// | Arr0 -> "no argument"
/// | Arr1 single -> "one argument"
/// | Arr2 (first, second) -> "two arguments"
/// | Arr3 (first, second, third) -> "three arguments"
/// | ArrN 4 -> "four arguments"
/// | _ -> "more arguments"
/// </code>
/// </example>
[<AutoOpen>]
module EquatableArrayPatterns =

    // ---- Access ----

    /// <summary>Matches an array without elements. The default value matches.</summary>
    /// <param name="array">The array to match.</param>
    val (|Arr0|_|) : array : EquatableArray<'T> -> bool

    /// <summary>Matches an array with exactly one element and gives that element.</summary>
    /// <param name="array">The array to match.</param>
    [<return : Struct>]
    val (|Arr1|_|) : array : EquatableArray<'T> -> 'T voption

    /// <summary>
    /// Matches an array with exactly two elements and gives them as a struct tuple. The pattern binds the tuple with or
    /// without the <see langword="struct"/> keyword.
    /// </summary>
    /// <param name="array">The array to match.</param>
    [<return : Struct>]
    val (|Arr2|_|) : array : EquatableArray<'T> -> struct ('T * 'T) voption

    /// <summary>Matches an array with exactly three elements and gives them as a struct tuple.</summary>
    /// <param name="array">The array to match.</param>
    [<return : Struct>]
    val (|Arr3|_|) : array : EquatableArray<'T> -> struct ('T * 'T * 'T) voption

    /// <summary>
    /// Matches an array with exactly <paramref name="length"/> elements, for the lengths that have no pattern of their
    /// own. Use the indexer to read the elements.
    /// </summary>
    /// <param name="length">The number of elements to match, written as the argument of the pattern.</param>
    /// <param name="array">The array to match.</param>
    val (|ArrN|_|) : length : int -> array : EquatableArray<'T> -> bool
