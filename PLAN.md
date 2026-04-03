# Source Generator Refactor Plan

## Overview

Refactor EntityChange to use a Roslyn incremental source generator instead of runtime reflection for comparing object graphs. The existing reflection-based code remains untouched — all new code lives in new projects.

---

## New Projects

### 1. `src/EntityChange.Generators` — Source Generator + Attributes + Diagnostic Analyzer
### 2. `test/EntityChange.Generators.Tests` — Unit tests for the generator and analyzer

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

### Type Hierarchy

```
IEntityComparer                          (existing interface, unchanged)
└── IEntityComparer<T>                   (new generic interface)
    └── EntityComparer<T>                (new abstract base class)
        └── OrderComparer [partial]      (user-declared + generated)
```

### Interface Definitions (shipped in EntityChange.Generators as source)

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

### Project 1: `src/EntityChange.Generators/EntityChange.Generators.csproj`

**Target:** `netstandard2.0` (required for analyzers/generators)

**NuGet References:**
- `Microsoft.CodeAnalysis.CSharp` (>= 4.3.0 for incremental generators)

**Ships as an analyzer/generator NuGet package** — uses `<IncludeBuildOutput>false</IncludeBuildOutput>` and packs into `analyzers/dotnet/cs`.

**Contents:**

#### A. Attributes (embedded as source via `[ExcludeFromCodeCoverage]` approach or `additionalfiles`)

These are injected into the consuming project via the generator so there is no runtime dependency:

| Attribute | Target | Purpose |
|-----------|--------|---------|
| `[GenerateComparer]` | Class | Marks partial class for generation |
| `[CompareIgnore]` | Property | Exclude property from comparison |
| `[CompareDisplay(Name)]` | Property | Override display name (alternative to DataAnnotations) |
| `[CompareFormat(Format)]` | Property | Format string for value display (e.g. `"C"`, `"d"`) |
| `[CompareCollection(CollectionComparison)]` | Property | Set collection comparison mode |

The generator also reads existing `System.ComponentModel.DataAnnotations` attributes:
- `[Display(Name = "...")]` → display name
- `[DisplayName("...")]` → display name  
- `[NotMapped]` → treated as ignored

#### B. Incremental Source Generator: `EntityChangeGenerator`

**Pipeline design for optimal caching:**

```
Step 1: SyntaxProvider.ForAttributeWithMetadataName("GenerateComparerAttribute")
        → Filter to partial classes deriving from EntityComparer<T>
        → Extract: ComparerModel (equatable record)
            - ClassName, Namespace, Accessibility
            - TypeParameter T (fully qualified)
            
Step 2: For each ComparerModel, walk T's property tree → PropertyModel[]
        → Each PropertyModel (equatable record):
            - Name, TypeFullName, DisplayName
            - IsIgnored, FormatString
            - IsCollection, IsDictionary, IsComplexObject, IsValueType
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
        
        // Total — has [CompareFormat("C")]
        PathStack.PushProperty("Total");
        CompareValue(original.Total, current.Total, "Total", "Total",
            formatter: v => ((decimal)v).ToString("C"));
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
6. **Display names**: Resolved at generation time from `[Display]`, `[DisplayName]`, `[CompareDisplay]`, or PascalCase→Title conversion
7. **Format strings**: Resolved at generation time from `[CompareFormat]` — generates inline `ToString(format)` calls
8. **Ignored properties**: Simply omitted from generated code
9. **Nullable properties**: Null-checked before accessing
10. **Enum properties**: Treated as value types, compared with `Equals`
11. **Abstract/interface properties**: Use runtime type via `GetType()` for the specific compare, with fallback to `CompareValue`
12. **Auto-detect sets**: Properties typed as `ISet<T>`, `HashSet<T>`, or `IReadOnlySet<T>` automatically use `CompareSet` unless overridden via `[CompareCollection]`

#### C. Source-Injected Types

The generator injects these types into the consuming assembly (via `RegisterPostInitializationOutput`):

1. `GenerateComparerAttribute` — marker attribute
2. `CompareIgnoreAttribute` — ignore property
3. `CompareDisplayAttribute` — display name override  
4. `CompareFormatAttribute` — format string
5. `CompareCollectionAttribute` — collection comparison mode
6. `IEntityComparer<T>` — generic interface
7. `EntityComparer<T>` — abstract base class with helpers

#### D. Diagnostic Analyzer (same project)

The analyzer lives in the same `EntityChange.Generators` assembly alongside the generator. Both are loaded from the `analyzers/dotnet/cs` NuGet folder. This keeps packaging simple and lets the analyzer share attribute symbol constants with the generator.

**Diagnostic Rules:**

| ID | Severity | Description |
|----|----------|-------------|
| `EC0001` | Warning | Class with `[GenerateComparer]` must be `partial` |
| `EC0002` | Warning | Class with `[GenerateComparer]` must derive from `EntityComparer<T>` |
| `EC0003` | Warning | `EntityComparer<T>` type parameter `T` must be a reference type with accessible properties |
| `EC0004` | Warning | `[CompareFormat]` applied to a non-formattable type |
| `EC0005` | Warning | `[CompareCollection]` applied to a non-collection property |
| `EC0006` | Info | Property type is not supported for deep comparison (will be compared by reference) |

**Implementation:**
- `EntityChangeAnalyzer : DiagnosticAnalyzer` 
- Registers `SymbolAction` on `NamedType` to check classes with the attribute
- Registers `SymbolAction` on `Property` for attribute validation

---

### Project 2: `test/EntityChange.Generators.Tests/EntityChange.Generators.Tests.csproj`

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
3. Add both projects to `EntityChange.slnx`
4. Create `src/EntityChange.Generators/Properties/launchSettings.json` for debugging

### Phase 2: Attributes & Base Types (Post-Initialization Source)
6. Create attribute source strings: `GenerateComparerAttribute`, `CompareIgnoreAttribute`, `CompareDisplayAttribute`, `CompareFormatAttribute`, `CompareCollectionAttribute`
7. Create `IEntityComparer<T>` interface source (bridges to existing `IEntityComparer`)
8. Create `EntityComparer<T>` abstract base class source with:
   - `PathStack`, `Changes` fields
   - `CreateChange()` helper
   - `CompareValue()` for scalar comparison
   - `CompareListByIndex()` for index-based collection comparison
   - `CompareListByEquality()` for equality-based collection comparison
   - `CompareSet<TElement>()` for set comparison (added/removed via set difference)
   - `CompareDictionary<TKey, TValue>()` for dictionary comparison
9. Register all via `RegisterPostInitializationOutput`

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
26. Read `[Display(Name=...)]` from `System.ComponentModel.DataAnnotations`
27. Read `[DisplayName(...)]` from `System.ComponentModel`
28. Read `[NotMapped]` → treat as ignored
29. Read `[CompareDisplay]`, `[CompareIgnore]`, `[CompareFormat]`, `[CompareCollection]` (new attributes)
30. Priority: `[CompareDisplay]` > `[Display]` > `[DisplayName]` > PascalCase→Title

### Phase 7: Diagnostic Analyzer (same project as generator)
31. Create `EntityChangeAnalyzer` class in `EntityChange.Generators`
32. Implement EC0001–EC0006 diagnostics
33. Register appropriate symbol/syntax actions

### Phase 8: Tests
34. Generator snapshot tests with Verify
35. Round-trip behavior tests (compile + run generated comparer, compare output with legacy)
36. Analyzer diagnostic tests
37. Edge case tests

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

- `src/EntityChange/` — all existing code (reflection-based comparer, fluent config, formatters, etc.) is untouched
- `test/EntityChange.Tests/` — existing tests remain as-is
- `IEntityComparer` interface — the new `IEntityComparer<T>` extends it, maintaining backward compatibility
- `ChangeRecord`, `ChangeOperation`, `CollectionComparison`, `PathStack` — reused by generated code via the base class
