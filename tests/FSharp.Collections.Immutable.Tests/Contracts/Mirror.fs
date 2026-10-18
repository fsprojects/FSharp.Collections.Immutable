namespace FSharp.Collections.Immutable.Tests

/// The FSharp.Core module whose argument checks a collection module mirrors (ADR 0001, D6).
type Mirror =
    /// `FlatList` mirrors `Array`.
    | ArrayMirror
    /// `IndexedList`, `Queue`, `Stack` and their interface modules mirror `List`.
    | ListMirror
