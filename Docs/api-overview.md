# Lens API Overview

This page summarizes the public runtime API that consuming Unity projects are expected to use.

## Section Providers

Implement `ILensSectionProvider` near the system that owns the data:

```csharp
public sealed class EconomyLensSection : ILensSectionProvider
{
    public string SectionTitle => "Economy";

    public IEnumerable<LensEntry> GetEntries()
    {
        yield return new LensEntry("Coins", "120");
    }
}
```

Register and unregister the provider with `LensSectionRegistry` during the owner lifetime.

## Entries

Use `LensEntry` factory methods for common controls:

- `ReadOnly` and `new LensEntry(key, value)` for static rows.
- `Toggle`, `Text`, `Number`, and `Slider` for editable values.
- `SingleSelect<T>` and `MultiSelect<T>` for labeled options.
- `Progress` for read-only progress toward a known total.
- `Button` for predefined project-owned debug actions.
- `Custom` for project-owned controls rendered by `ILensEntryDrawer`.

Providers own all state and callbacks. Lens renders controls, invokes callbacks, and reports current values.

## Runtime Policy

`LensRuntimePolicy` controls whether Lens is allowed at runtime. By default, Lens is enabled only in Editor, Development Build, or builds compiled with `LENS_ENABLED`.

`LensRuntimeConsole.Open()` and `Toggle()` do nothing when the policy disallows Lens. `Close()` always works.

## Redaction And Actions

Mark values as sensitive when they are useful for debugging but should not be visible in the overlay or reports. Redacted values appear as `[redacted]`.

Use confirmation for action buttons that mutate progression, inventory, tutorial, save data, account state, or content unlocks.

## Custom Drawers

Register project-owned drawers through `LensEntryDrawerRegistry`:

```csharp
LensEntryDrawerRegistry.Register("my.graph", new MyGraphDrawer());
```

Custom drawers receive `LensEntryDrawContext`, including section title, stable entry id, layout metrics, lightweight state storage, and status helpers.

## Reports

Use `LensReportBuilder` to generate plain text or JSON reports from registered providers:

```csharp
var text = LensReportBuilder.BuildReport(LensSectionRegistry.Providers, LensReportFormat.Text);
var json = LensReportBuilder.BuildReport(LensSectionRegistry.Providers, LensReportFormat.Json);
```

Reports include Lens version, UTC timestamp, sections, entries, action labels, info text, and redacted values. Report generation never executes action entries.

Use `LensReportCapture` to capture local screenshots under `Application.persistentDataPath/LensReports`.
