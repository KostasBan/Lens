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

## Integration Pattern

Prefer adding an `ILensSectionProvider` owned by the system that owns the data. Register it during the system/bootstrap lifetime and unregister it when that owner is destroyed.

```csharp
LensSectionRegistry.Register(new MySystemLensSection(mySystem));
```

Use callback entries for important mutable values:

```csharp
yield return LensEntry.Toggle("Feature Enabled", () => system.Enabled, value => system.Enabled = value);
yield return LensEntry.Number("Spawn Rate", () => system.SpawnRate, value => system.SpawnRate = value);
yield return LensEntry.Button("Unlock Debug Content", system.UnlockDebugContent);
```

Lens should render and invoke callbacks; the consuming system should own state, validation, permissions, and side effects.

## Repo Rules

- Keep runtime code under `Runtime/`.
- Keep tests under `Tests/Runtime/`.
- Do not add external runtime dependencies without a clear package-level decision.
- Preserve `new LensEntry(string key, string value)` compatibility.
- Prefer small focused collaborators over growing `LensRuntimeConsole` into a large class.
- Validate with Unity `6000.3.16f1` when changing runtime code.

## Validation Checklist

- Package imports in a clean Unity project.
- Runtime tests pass.
- Sample scene compiles.
- README examples still match the public API.
- Reports never execute action entries.
