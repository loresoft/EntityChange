# Source Generator Refactor Plan

## Overview

Refactor EntityChange to use a Roslyn incremental source generator instead of runtime reflection for comparing object graphs. The existing reflection-based code remains untouched. New attributes, interfaces, and base types are added to the main `EntityChange` project so it carries the runtime contract. The generator project is development-only (`PrivateAssets="All"`).

---

## New & Modified Projects

### Modified: `src/EntityChange` — Add attributes, `IEntityComparer<T>`, `EntityComparer<T>` base class
### New: `src/EntityChange.Generators` — Source Generator + Diagnostic Analyzer (dev-only)
### New: `test/EntityChange.Generators.Tests` — Unit tests for the generator and analyzer

---

## Architecture

### User-Facing API (Target Usage)

```csharp
// User writes a partial class anchored by [GenerateComparer] and deriving from EntityComparer<T>
[GenerateComparer]
public partial class OrderComparer : EntityComparer<Order>
{
    // The source generator fills in the implementation
}

// Usage
IEntityComparer<Order> comparer = new OrderComparer();
IReadOnlyList<ChangeRecord> changes = comparer.Compare(original, current);

// Also works via the non-generic interface
IEntityComparer legacyComparer = comparer;
var changes2 = legacyComparer.Compare(original, current);
```

### Package Dependency Model

```
User Project
├── PackageReference: EntityChange          (runtime dependency — attributes, base class, interfaces)
└── PackageReference: EntityChange.Generators (dev-only — PrivateAssets="All", generates code at build)
```

The generator package ships with `<IncludeBuildOutput>false</IncludeBuildOutput>` and packs into `analyzers/dotnet/cs`. It is never a transitive runtime dependency.

### Type Hierarchy

```
IEntityComparer                          (existing interface, unchanged)
└── IEntityComparer<T>                   (new generic interface, in EntityChange)
    └── EntityComparer<T>                (new abstract base class, in EntityChange)
        └── OrderComparer [partial]      (user-declared + generated)
```

### Interface Definitions (in EntityChange project)

```csharp
// New generic interface — extends the existing non-generic one
public interface IEntityComparer<T> : IEntityComparer
{
    IReadOnlyList<ChangeRecord> Compare(T? original, T? current);

    // Explicit implementation of non-generic interface
    IReadOnlyList<ChangeRecord> IEntityComparer.Compare<TEntity>(TEntity? original, TEntity? current)
    {
        if (original is T o && current is T c)
            return Compare(o, c);
        if (original is T o2)
            return Compare(o2, default);
        if (current is T c2)
            return Compare(default, c2);
        return Compare(default, default);
    }
}
```

```csharp
// New abstract base class with shared runtime helpers
public abstract class EntityComparer<T> : IEntityComparer<T>
{
    public abstract IReadOnlyList<ChangeRecord> Compare(T? original, T? current);

    // Shared helpers used by generated code:
    protected PathStack PathStack { get; } = new();
    protected List<ChangeRecord> Changes { get; } = new();

    protected void CreateChange(ChangeOperation op, string propertyName, 
        string displayName, object? original, object? current,
        Func<object?, string?>? formatter = null) { ... }

    // Collection comparison helpers (reusable, not generated per-type):
    protected void CompareListByIndex<TElement>(...) { ... }
    protected void CompareListByEquality<TElement>(...) { ... }
    protected void CompareSet<TElement>(...) { ... }
    protected void CompareDictionary<TKey, TValue>(...) { ... }
    protected void CompareValue(object? original, object? current, 
        string propertyName, string displayName,
        Func<object?, object?, bool>? equality = null,
        Func<object?, string?>? formatter = null) { ... }
}
```

---

## Project Details

### Modified: `src/EntityChange/EntityChange.csproj`

New files added to the existing project (no existing files modified):

#### A. Attributes (in `EntityChange` namespace)

| Attribute | Target | Purpose |
|-----------|--------|---------|
| `[GenerateComparer]` | Class | Marks partial class for generation |
| `[CompareIgnore]` | Property | Exclude property from comparison |
| `[CompareDisplay(Name)]` | Property | Override display name (alternative to DataAnnotations) |
| `[CompareCollection(CollectionComparison)]` | Property | Set collection comparison mode |

**No custom format attribute.** Instead, the generator reads the standard `[DisplayFormat(DataFormatString = "...")]` from `System.ComponentModel.DataAnnotations` (already referenced by the project for netstandard2.0).

The generator also reads these existing `System.ComponentModel.DataAnnotations` attributes on the target type `T`:
- `[Display(Name = "...")]` → display name
- `[DisplayName("...")]` → display name
- `[DisplayFormat(DataFormatString = "...")]` → value format string (e.g. `"{0:C}"`, `"{0:d}"`)
- `[NotMapped]` → treated as ignored

#### B. New Types (in `EntityChange` namespace)

| Type | Kind | Purpose |
|------|------|---------|
| `IEntityComparer<T>` | Interface | Generic comparer interface, extends `IEntityComparer` |
| `EntityComparer<T>` | Abstract class | Base class with comparison helpers for generated code |

These are compiled into the `EntityChange` assembly and available at runtime. The generator references them by fully-qualified name in emitted code.

---

### New: `src/EntityChange.Generators/EntityChange.Generators.csproj`

**Target:** `netstandard2.0` (required for analyzers/generators)

**NuGet References:**
- `Microsoft.CodeAnalysis.CSharp` (>= 4.3.0 for incremental generators)

**Ships as a dev-only analyzer/generator NuGet package:**
- `<IncludeBuildOutput>false</IncludeBuildOutput>`
- Packs into `analyzers/dotnet/cs`
- Consumers reference with `PrivateAssets="All"` — no transitive runtime dependency

**Contents:**

#### A. Incremental Source Generator: `EntityChangeGenerator`

**Pipeline design for optimal caching:**

```
Step 1: SyntaxProvider.ForAttributeWithMetadataName("EntityChange.GenerateComparerAttribute")
        → Filter to partial classes deriving from EntityComparer<T>
        → Extract: ComparerModel (equatable record)
            - ClassName, Namespace, Accessibility
            - TypeParameter T (fully qualified)
            
Step 2: For each ComparerModel, walk T's property tree → PropertyModel[]
        → Each PropertyModel (equatable record):
            - Name, TypeFullName, DisplayName
            - IsIgnored, FormatString
            - IsCollection, IsSet, IsDictionary, IsComplexObject, IsValueType
            - CollectionComparison mode
            - CollectionElementType (if applicable)
            - DictionaryKeyType, DictionaryValueType (if applicable)
            
Step 3: Combine ComparerModel + PropertyModel[] → GenerationModel (equatable)

Step 4: RegisterSourceOutput → Generate source for each GenerationModel
```

All models are **equatable value types/records** so the incremental pipeline caches correctly and avoids regeneration when unrelated code changes.

**Generated code structure** (for `OrderComparer : EntityComparer<Order>`):

```csharp
// <auto-generated/>
#nullable enable

namespace UserNamespace;

partial class OrderComparer
{
    public override IReadOnlyList<ChangeRecord> Compare(Order? original, Order? current)
    {
        Changes.Clear();
        PathStack.Clear();
        
        CompareOrder(original, current);
        
        return Changes.ToList();
    }
    
    private void CompareOrder(Order? original, Order? current)
    {
        if (original is null && current is null) return;
        
        if (original is null) { CreateChange(ChangeOperation.Replace, ...); return; }
        if (current is null)  { CreateChange(ChangeOperation.Replace, ...); return; }
        
        // --- Scalar properties (inline, no reflection) ---
        // Id
        PathStack.PushProperty("Id");
        CompareValue(original.Id, current.Id, "Id", "Id");
        PathStack.Pop();
        
        // OrderNumber  
        PathStack.PushProperty("OrderNumber");
        CompareValue(original.OrderNumber, current.OrderNumber, "OrderNumber", "Order Number");
        PathStack.Pop();
        
        // Total — has [DisplayFormat(DataFormatString = "{0:C}")]
        PathStack.PushProperty("Total");
        CompareValue(original.Total, current.Total, "Total", "Total",
            formatter: v => string.Format("{0:C}", v));
        PathStack.Pop();
        
        // --- Nested complex object ---
        // BillingAddress
        PathStack.PushProperty("BillingAddress");
        CompareMailingAddress(original.BillingAddress, current.BillingAddress);
        PathStack.Pop();
        
        // --- Collection property ---
        // Items
        PathStack.PushProperty("Items");
        CompareListByIndex(original.Items, current.Items,
            (o, c) => CompareOrderLine(o, c));
        PathStack.Pop();
    }
    
    private void CompareMailingAddress(MailingAddress? original, MailingAddress? current) { ... }
    private void CompareOrderLine(OrderLine? original, OrderLine? current) { ... }
}
```

**Key generation rules:**
1. **Value types & strings**: Direct `CompareValue()` call with property access — no reflection
2. **Nested complex objects**: Generate a typed `Compare{TypeName}` method, recurse into its properties
3. **Collections (IList, ICollection, arrays)**: Delegate to base `CompareListByIndex` or `CompareListByEquality` with a typed comparison callback
4. **Sets (ISet\<T\>, HashSet\<T\>, IReadOnlySet\<T\>)**: Delegate to base `CompareSet<TElement>` — uses set semantics (added = `current.Except(original)`, removed = `original.Except(current)`), no index paths
5. **Dictionaries**: Delegate to base `CompareDictionary<TKey, TValue>` 
6. **Display names**: Resolved at generation time from `[CompareDisplay]` > `[Display]` > `[DisplayName]` > PascalCase→Title conversion
7. **Format strings**: Resolved at generation time from `[DisplayFormat(DataFormatString = "...")]` — generates inline `string.Format(format, v)` calls
8. **Ignored properties**: `[CompareIgnore]` or `[NotMapped]` — simply omitted from generated code
9. **Nullable properties**: Null-checked before accessing
10. **Enum properties**: Treated as value types, compared with `Equals`
11. **Abstract/interface properties**: Use runtime type via `GetType()` for the specific compare, with fallback to `CompareValue`
12. **Auto-detect sets**: Properties typed as `ISet<T>`, `HashSet<T>`, or `IReadOnlySet<T>` automatically use `CompareSet` unless overridden via `[CompareCollection]`

#### B. Diagnostic Analyzer (same project)

The analyzer lives in the same `EntityChange.Generators` assembly alongside the generator. Both are loaded from the `analyzers/dotnet/cs` NuGet folder.

**Diagnostic Rules:**

| ID | Severity | Description |
|----|----------|-------------|
| `EC0001` | Warning | Class with `[GenerateComparer]` must be `partial` |
| `EC0002` | Warning | Class with `[GenerateComparer]` must derive from `EntityComparer<T>` |
| `EC0003` | Warning | `EntityComparer<T>` type parameter `T` must be a reference type with accessible properties |
| `EC0004` | Warning | `[DisplayFormat]` applied to a non-formattable type |
| `EC0005` | Warning | `[CompareCollection]` applied to a non-collection property |
| `EC0006` | Info | Property type is not supported for deep comparison (will be compared by reference) |

**Implementation:**
- `EntityChangeAnalyzer : DiagnosticAnalyzer` 
- Registers `SymbolAction` on `NamedType` to check classes with the attribute
- Registers `SymbolAction` on `Property` for attribute validation

---

### New: `test/EntityChange.Generators.Tests/EntityChange.Generators.Tests.csproj`

**Target:** `net9.0`

**References:**
- `Microsoft.CodeAnalysis.CSharp.Analyzer.Testing` 
- `Microsoft.CodeAnalysis.CSharp.SourceGenerators.Testing`
- `xunit`, `FluentAssertions`, `Verify.SourceGenerators`

**Test Categories:**

1. **Generator snapshot tests** — Verify generated source output using `Verify.SourceGenerators`
2. **Generator behavior tests** — Compile generated code and run comparisons, verify `ChangeRecord` output matches legacy `EntityComparer` behavior
3. **Analyzer tests** — Verify each diagnostic fires correctly using `AnalyzerTest<>` infrastructure
4. **Edge cases** — nullable properties, enums, abstract types, recursive types, empty classes, generic properties

---

## Implementation Steps

### Phase 1: Project Scaffolding
1. Create `src/EntityChange.Generators/EntityChange.Generators.csproj` with correct SDK, TFM, and analyzer packaging properties
2. Create `test/EntityChange.Generators.Tests/EntityChange.Generators.Tests.csproj`
3. Add both new projects to `EntityChange.slnx`

### Phase 2: Attributes & Base Types (in EntityChange project)
4. Add `GenerateComparerAttribute.cs` to `src/EntityChange/`
5. Add `CompareIgnoreAttribute.cs` to `src/EntityChange/`
6. Add `CompareDisplayAttribute.cs` to `src/EntityChange/`
7. Add `CompareCollectionAttribute.cs` to `src/EntityChange/`
8. Add `IEntityComparer{T}.cs` — generic interface bridging to existing `IEntityComparer`
9. Add `EntityComparer{T}.cs` — abstract base class with:
   - `PathStack`, `Changes` fields
   - `CreateChange()` helper
   - `CompareValue()` for scalar comparison
   - `CompareListByIndex()` for index-based collection comparison
   - `CompareListByEquality()` for equality-based collection comparison
   - `CompareSet<TElement>()` for set comparison (added/removed via set difference)
   - `CompareDictionary<TKey, TValue>()` for dictionary comparison

### Phase 3: Generator Models (Equatable Records)
10. Define `ComparerInfo` record: class name, namespace, accessibility, target type symbol
11. Define `PropertyInfo` record: name, type info, display name, format, ignore flag, collection mode, nested type info
12. Define `GenerationModel` record: comparer info + property list
13. Ensure all records implement `IEquatable<T>` with value semantics for caching

### Phase 4: Incremental Pipeline
14. Use `ForAttributeWithMetadataName` to find `[GenerateComparer]` classes
15. Transform: extract `ComparerInfo` from syntax/semantic model
16. Transform: walk target type `T` properties recursively, build `PropertyInfo[]`
17. Combine into `GenerationModel`
18. Register source output

### Phase 5: Code Generation
19. Build `SourceBuilder` utility (indented `StringBuilder` wrapper)
20. Generate `Compare(T?, T?)` override method
21. Generate per-type `Compare{TypeName}()` private methods for complex objects
22. Generate inline scalar comparisons with display names and format strings
23. Generate collection delegation calls (list, array)
24. Generate set delegation calls (auto-detected for ISet<T>/HashSet<T>/IReadOnlySet<T>)
25. Generate dictionary delegation calls
26. Handle nullability, enums, abstract types

### Phase 6: Data Annotation Support in Generator
27. Read `[Display(Name=...)]` from `System.ComponentModel.DataAnnotations`
28. Read `[DisplayName(...)]` from `System.ComponentModel`
29. Read `[DisplayFormat(DataFormatString=...)]` from `System.ComponentModel.DataAnnotations`
30. Read `[NotMapped]` → treat as ignored
31. Read `[CompareDisplay]`, `[CompareIgnore]`, `[CompareCollection]` (EntityChange attributes)
32. Priority for display name: `[CompareDisplay]` > `[Display]` > `[DisplayName]` > PascalCase→Title
33. Priority for format: `[DisplayFormat]` (only source)

### Phase 7: Diagnostic Analyzer (same project as generator)
34. Create `EntityChangeAnalyzer` class in `EntityChange.Generators`
35. Implement EC0001–EC0006 diagnostics
36. Register appropriate symbol/syntax actions

### Phase 8: Tests
37. Generator snapshot tests with Verify
38. Round-trip behavior tests (compile + run generated comparer, compare output with legacy)
39. Analyzer diagnostic tests
40. Edge case tests

---

## Caching Strategy (Critical for IDE Performance)

The incremental generator pipeline is designed for maximum cache hits:

1. **`ForAttributeWithMetadataName`** — only triggers when a class with `[GenerateComparer]` changes
2. **Equatable models** — all intermediate pipeline values are equatable records/structs. If a user edits an unrelated file, the pipeline short-circuits and reuses cached output
3. **No `Compilation`-level transforms** — we avoid `RegisterSourceOutput(compilationProvider, ...)` which would invalidate on every keystroke
4. **Property walking is deterministic** — properties are sorted by declaration order, so the model is stable across compilations
5. **Lightweight analyzer** — diagnostics use symbol actions only (no syntax tree walking), so they don't interfere with the generator's incremental cache

---

## What Stays Unchanged

- All existing files in `src/EntityChange/` — reflection-based comparer, fluent config, formatters, etc. are untouched
- `test/EntityChange.Tests/` — existing tests remain as-is
- `IEntityComparer` interface — the new `IEntityComparer<T>` extends it, maintaining backward compatibility
- `ChangeRecord`, `ChangeOperation`, `CollectionComparison`, `PathStack` — reused by generated code via the base class
