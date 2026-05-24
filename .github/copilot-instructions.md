# Copilot Instructions

## Project Details

* .NET SDK pinned in #file:'global.json'
* Common parameters specified in #file:'Directory.Build.props'
* Central NuGet package version management – versions go in #file:'Directory.Packages.props', not in `.fsproj` files
* Build: `dotnet build FSharp.Collections.Immutable.slnx`
* Test: `dotnet test FSharp.Collections.Immutable.slnx`

## Solution Structure

**IMPORTANT: Every collection binding implementation file must have a corresponding `.fsi` signature file.**

```text
/
├── build/                                              – FAKE build scripts and release automation
├── docsSrc/                                            – FSharp.Formatting documentation source
├── src/FSharp.Collections.Immutable/                    – main library
│   ├── Helper.fs
│   ├── ComputationExpressions.fs
│   ├── FlatList/                                       – ImmutableArray API surface and builders
│   │   ├── FlatList.fs / FlatList.fsi
│   │   ├── FlatListBuilder.fs
│   │   └── FlatListCE.fs
│   ├── IndexedList/                                    – ImmutableList API surface and builders
│   │   ├── IIndexedList.fs / IIndexedList.fsi
│   │   ├── IndexedList.fs / IndexedList.fsi
│   │   ├── IndexedListBuilder.fs
│   │   └── IndexedListCE.fs
│   ├── Queue/                                          – ImmutableQueue API surface and CE
│   │   ├── IQueue.fs / IQueue.fsi
│   │   ├── Queue.fs / Queue.fsi
│   │   └── QueueCE.fs
│   ├── Stack/                                          – ImmutableStack API surface and CE
│   │   ├── IStack.fs / IStack.fsi
│   │   ├── Stack.fs / Stack.fsi
│   │   └── StackCE.fs
│   ├── HashMap/                                        – ImmutableDictionary API surface and CE
│   │   ├── IHashMap.fs / IHashMap.fsi
│   │   ├── HashMap.fs / HashMap.fsi
│   │   └── HashMapCE.fs
│   ├── SortedMap/                                      – ImmutableSortedDictionary API surface and CE
│   │   ├── ISortedMap.fs / ISortedMap.fsi
│   │   ├── SortedMap.fs / SortedMap.fsi
│   │   └── SortedMapCE.fs
│   ├── HashSet/                                        – ImmutableHashSet API surface and CE
│   │   ├── IHashSet.fs / IHashSet.fsi
│   │   ├── HashSet.fs / HashSet.fsi
│   │   └── HashSetCE.fs
│   └── SortedSet/                                      – ImmutableSortedSet API surface and CE
│       ├── ISortedSet.fs / ISortedSet.fsi
│       ├── SortedSet.fs / SortedSet.fsi
│       └── SortedSetCE.fs
└── tests/FSharp.Collections.Immutable.Tests/            – MSTest test project
    ├── HelperTests.fs
    ├── TestCategories.fs                               – centralized custom test category attributes
    ├── FlatList/                                       – focused test files per operation group
    ├── IndexedList/                                    – focused test files per operation group
    ├── HashMap/                                        – focused test files per operation group
    ├── SortedMap/                                      – focused test files per operation group
    ├── HashSet/                                        – focused test files per operation group
    ├── SortedSet/                                      – focused test files per operation group
    ├── Queue/                                          – focused test files per operation group
    └── Stack/                                          – focused test files per operation group
```

## Libraries in Use

* [`System.Collections.Immutable`](https://www.nuget.org/packages/System.Collections.Immutable/) – immutable collections foundation
* [`FSharp.Core`](https://www.nuget.org/packages/FSharp.Core/) – F# language core library
* [`MSTest`](https://github.com/microsoft/testfx) – test framework
* [`Unquote`](https://github.com/SwensenSoftware/unquote) – expressive assertions for complex checks
* [`FAKE`](https://fake.build/) – build and release scripting
* [`Argu`](https://github.com/fsprojects/Argu) – command-line parsing for build tooling

## F# Coding Guidelines

### Language Preferences

* Always use the latest F# 10 features over old syntax.
* Prefer `voption` over `option`.
* Prefer `task` CE over `async` CE.
* Prefer underscore lambda syntax like `Seq.map _.Name` over `Seq.map (fun x -> x.Name)`, but only when the expression is a simple member access. Complex expressions like `Seq.where (fun x -> x.Name = name)` or `Seq.map (fun x -> x.Field1, x.Field2)` cannot be simplified.
* Simplify `Seq.map (fun x -> someFunction x)` to `Seq.map someFunction`.
* When pipe operators are used on a materializable collection multiple times in a row, prefer `Seq` module for the chain and materialize at the end.
* Prefer interpolated strings over `printf` functions for string formatting.
* Use `withNull` for null checks instead of boxing delegates/functions (avoid `isNull (box value)`).

### Nullable Reference Types

* Declare variables non-nullable; check for `null` at entry points only.
* Trust the SDK null annotations – do not add null checks when the type system says a value cannot be null.
* Prefer `match` on `null` over `if isNull`:

  ```fsharp
  // Preferred
  match someObject with
  | null -> ()
  | someObject -> someObject.SomeProperty
  ```

### Class Constructors

This is how to define a non-default F# class constructor:

```fsharp
type DerivedClass =
    inherit BaseClass

    new (``arguments here``) as ``created object``
        =
        // create any objects used in the base class constructor
        let fieldValue = ""
        {
            inherit
                BaseClass (``arguments here``)
        }
        then
            ``created object``.otherField <- fieldValue

    [<DefaultValue>]
    val mutable otherField : FieldType
```

### Class Instantiation

Always prefer F# class initializers over property assignment! **You absolutely must use F# class initializers instead of property assignment**!

Class declaration:

```fsharp
type MyClass (someConstructorParam : string) =
    member ReadOnlyProperty = someConstructorParam

    member val MutableProperty1 = "" with get, set
    member val MutableProperty2 = "" with get, set
```

Wrong:

```fsharp
let myClass = MyClass("some value")
myClass.MutableProperty1 <- "new value"
myClass.MutableProperty2 <- "new value"
```

Right:

```fsharp
let myClass =
    MyClass(
        // constructor parameters go first without names
        "some value",
        // then mutable properties go next with names
        MutableProperty1 = "new value",
        MutableProperty2 =
            // operations must be placed into parentheses
            (5 |> string)
    )
```

### C#-Consumable Extension Members

```fsharp
// AutoOpen makes the module automatically available without an explicit open statement
// Extension makes the members visible to C#
[<AutoOpen; Extension>]
module MyTypeExtensions =

    type MyType with

        // Extension is visible to C#
        // CompiledName makes the method name friendly to C#
        [<Extension; CompiledName "ExtensionMethod">]
        member this.ExtensionMethod (param1 : string) : ReturnType =
            ()
```

## Naming Conventions

* Use PascalCase for modules, types, and public members.
* Use camelCase for `let` bindings, functions, private fields, and local variables.
* Prefix interface names with `I`.
* Do not prefix type parameters with `T` (e.g., use `'Result` instead of `'TResult`).

## Testing

* Tests use MSTest 4.x.
* Each test must have category applied:
  * Add a collection-level category on each test class (for example: `[<TestClass; QueueTestCategory>]`, `[<TestClass; IndexedListTestCategory>]`).
  * Add operation-level categories on each test method using `nameof` (for example: `[<TestMethod; TestCategory(nameof Queue.enqueue)>]`).
  * For specialized areas, add dedicated category attributes together with operation categories (for example: `[<TestMethod; BuilderTestCategory; TestCategory(nameof IndexedList.ofBuilder)>]`).
* Keep category names centralized in `TestCategories.fs` via custom `TestCategoryBaseAttribute` types; prefer attributes over repeating string literals.
* `CollectionAssert` cannot work with F# lists – use F# array syntax (`[| ... |]`) instead.
* Use Unquote only for complex object/hierarchy assertions; for simple scalar checks prefer standard `Assert.*` APIs.
* Every `Assert.*` call **must include a failure message** so output is self-explanatory.
* Async tests must return `Task`, not `Async` or `Task<unit>` – always declare `) : Task = task {`.

## General

* Make only high-confidence suggestions when reviewing code changes.
* Write code with good maintainability practices, including comments on why certain design decisions were made.
* Handle edge cases and write clear exception handling.
* Never duplicate code unless explicitly allowed.
* All comments, documentation, README files, and markdown files must be written in **English only**.
