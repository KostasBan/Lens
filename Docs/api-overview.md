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

For scene-owned providers, derive from `LensSectionBehaviour` so registration follows `OnEnable` and `OnDisable`. Lens resets static registry state on subsystem registration, which keeps Enter Play Mode with domain reload disabled from carrying providers across sessions.

When duplicate section titles are possible, implement `ILensIdentifiedSectionProvider` and return a stable `SectionId`. Lens uses the ID for fold state, entry drafts, popups, and custom drawer state while keeping `SectionTitle` as the visible label.

## Entries

Use `LensEntry` factory methods for common controls:

- `ReadOnly` and `new LensEntry(key, value)` for static rows.
- `Toggle`, `Text`, `Number`, and `Slider` for editable values.
- `SingleSelect<T>` and `MultiSelect<T>` for labeled options.
- `Progress` for read-only progress toward a known total.
- `Button` for predefined project-owned debug actions.
- `Custom` for project-owned controls rendered by `ILensEntryDrawer`.

Providers own all state and callbacks. Lens renders controls, invokes callbacks, and reports current values.

Keep `GetEntries()` cheap. Cache expensive data in the owning system and expose a snapshot through Lens instead of doing slow work during UI refresh. For frequently refreshed mutable entries, cache the `LensEntry` instances and delegates in the provider constructor instead of recreating closures every refresh.

Lens is main-thread-only. Providers should be called from Unity's frame/update flow and should not perform background-thread work.

## Runtime Policy

`LensRuntimePolicy` controls whether Lens is allowed at runtime. By default, Lens is enabled only in Editor, Development Build, or builds compiled with `LENS_ENABLED`.

`LensRuntimeConsole.Open()` and `Toggle()` do nothing when the policy disallows Lens. `Close()` always works.

Use `RefreshIntervalSeconds` to control how often the console rebuilds provider entries while open. The default is `0.25` seconds. Use `RefreshNow()` when a project knows provider data changed and wants the next draw to rebuild cached entries.

Use `LensRuntimeConsole.EnsureExists()` during bootstrap to create one console or reuse an already loaded console. Use `TryFindExisting(out var console)` when a project only wants to inspect whether a console is present.

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

Reports include schema version, Lens version, UTC timestamp, Unity/app/device/build metadata, sections, entries, action labels, info text, and redacted values. Report generation never executes action entries.

Use `LensReportCapture` to capture local screenshots under `Application.persistentDataPath/LensReports`.
Use `LensReportExporter` to write text and JSON report artifacts, and `LensNativeShare` to invoke native mobile sharing where supported.
Set `LensReportMetadata.ProjectBuildNumber` during project bootstrap when QA needs a project-specific build number in exported reports.
