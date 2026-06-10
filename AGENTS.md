# Agent Guide For Lens

Lens is a Unity runtime debug console package. When working in this repo or integrating Lens into another Unity project, keep the package small, dependency-free, and provider-based.

## When To Use Lens

Use Lens when a runtime value helps developers or QA answer practical questions in Play Mode, development builds, staging builds, or controlled internal builds.

Good Lens values:

- build/version/environment state,
- current scene and session context,
- feature flags and config-like values,
- recent product/debug events,
- performance counters,
- content unlock/debug state,
- safe predefined debug actions.

Avoid exposing:

- auth tokens, secrets, API keys, or credentials,
- private player data,
- destructive actions without clear labels and project-side gating,
- production-only controls without an explicit build gate.
- unredacted internal identifiers that would be unsafe in a copied report.

## Integration Pattern

Prefer adding an `ILensSectionProvider` owned by the system that owns the data. Register it during the system/bootstrap lifetime and unregister it when that owner is destroyed.

```csharp
LensSectionRegistry.Register(new MySystemLensSection(mySystem));
```

Use callback entries for important mutable values:

```csharp
yield return LensEntry.Toggle("Feature Enabled", () => system.Enabled, value => system.Enabled = value);
yield return LensEntry.Number("Spawn Rate", () => system.SpawnRate, value => system.SpawnRate = value);
yield return LensEntry.Text("Session Token", () => system.SessionToken, value => system.SessionToken = value, true);
yield return LensEntry.Button("Unlock Debug Content", system.UnlockDebugContent, true, "Unlock debug content?");
```

Lens should render and invoke callbacks; the consuming system should own state, validation, permissions, and side effects.
Use redaction for values that help QA but should not appear raw in the overlay or reports. Use confirmation for progression, inventory, account, save, tutorial, or content-unlock actions.
Use `LensOption<T>` labels for option entries, and use `LensEntry.Custom` plus `LensEntryDrawerRegistry` for project-owned controls that do not belong in Lens core.
Use `LensEntryDrawContext.IsCompact` and logical screen metrics when writing custom drawers so they remain usable on mobile portrait screens.

## Repo Rules

- Keep runtime code under `Runtime/`.
- Keep tests under `Tests/Runtime/`.
- Do not add external runtime dependencies without a clear package-level decision.
- Preserve `new LensEntry(string key, string value)` compatibility.
- Preserve fail-fast behavior for provider, action, report, and drawer exceptions.
- Keep IMGUI controls responsive by using `LensLayoutMetrics` instead of fixed pixel widths.
- Prefer small focused collaborators over growing `LensRuntimeConsole` into a large class.
- Validate with Unity `6000.3.16f1` when changing runtime code.

## Validation Checklist

- Package imports in a clean Unity project.
- Runtime tests pass.
- Sample scene compiles.
- README examples still match the public API.
- Reports never execute action entries.
- Text and JSON reports preserve redaction rules.
- Reports and search do not expose raw sensitive values.
- Screenshot capture remains local-only.
- Custom drawer registrations are explicit and easy to audit.
