# Lens Runtime Debug Console

Lens is a small runtime debug/inspection panel for Unity projects. It gives developers and QA a quick way to inspect useful in-build state without attaching a debugger or wiring every system directly into one UI.

Lens is intentionally generic. Runtime systems register section providers, Lens renders those sections, and QA can copy a readable debug report for bug reproduction.

## How To Use Lens

Use Lens whenever an important runtime value should be visible during development, QA, staging, or controlled internal builds.

Common examples:

- expose the active environment and config source,
- show evaluated feature flags,
- show safe session/debug identifiers,
- list recent gameplay/product events,
- expose performance counters,
- add safe action buttons for project-owned debug tools.

Install the package, add a provider, register it during bootstrap, then open Lens with `F1` or the floating `Lens` button. On mobile portrait screens, Lens scales up automatically and switches to a compact stacked layout.

## Why Lens Exists

Many Unity bugs are hard to reproduce because the important runtime context is scattered across systems: build version, active scene, platform, environment, session, feature-like values, recent events, and performance. Lens puts that context into one provider-based overlay.

V0.6 is deliberately small, extensible, and responsive:

- Runtime IMGUI overlay
- `F1` keyboard toggle
- Optional draggable floating button for mobile-friendly activation
- Foldable sections
- Search by section, key, value, or action label
- Provider-based sections
- Read-only, toggle, text, number, and button entries
- Slider, single-select, multi-select, and progress entries
- Custom entry drawers for project-owned controls
- Optional entry info text
- Automatic DPI/screen-aware IMGUI scaling
- Compact stacked layout for mobile portrait screens
- Internal-build enablement policy
- Sensitive entry redaction
- Optional confirmation for action buttons
- Copy-to-clipboard debug report
- Basic sample scene
- Runtime-safe package tests

## Screenshots

Screenshots will be added here once the visual pass is ready.

```text
Docs/images/lens-overlay.png
Docs/images/lens-interactive-entries.png
```

## Install

Add the package through Unity Package Manager from the Git URL:

```text
https://github.com/KostasBan/Lens.git
```

Or add this entry to your Unity project's `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.kostasban.lens": "https://github.com/KostasBan/Lens.git"
  }
}
```

## Run The Sample

1. Open your Unity project with Unity `6000.3` or newer.
2. Install Lens through Package Manager.
3. Import the `Basic Lens Demo` sample.
4. Open `LensDemo`.
5. Press Play.
6. Press `F1` or click the floating `Lens` button to open or close Lens.
7. Click `Copy Debug Report` to copy the current section data.

Lens handles the toggle through IMGUI events, so it does not require the legacy Input Manager or the new Input System package.

The sample registers five providers:

- Build Info
- Session Info
- Sample Feature Flags
- Sample Recent Events
- Performance

The sample feature flag section includes editable toggles, text, numbers, a slider, single-select, multi-select, progress, action buttons, a redacted sample token, info text, and a confirmed unlock action.

## Add A Custom Section

Create a provider that implements `ILensSectionProvider`:

```csharp
using System.Collections.Generic;
using KostasBan.Lens;

public sealed class MyGameLensSection : ILensSectionProvider
{
    public string SectionTitle => "My Game";

    public IEnumerable<LensEntry> GetEntries()
    {
        yield return new LensEntry("Coins", "120");
        yield return new LensEntry("Current Level", "3");
        yield return new LensEntry("Difficulty", "Normal");
    }
}
```

Register it at runtime:

```csharp
LensSectionRegistry.Register(new MyGameLensSection());
```

Unregister it when the owner is destroyed:

```csharp
LensSectionRegistry.Unregister(provider);
```

## Interactive Entries

Providers own all mutations through callbacks. Lens renders controls and invokes callbacks, but it does not store authoritative game state.

```csharp
public sealed class DebugSettingsLensSection : ILensSectionProvider
{
    private bool godMode;
    private string environment = "Development";
    private float coinMultiplier = 1f;

    public string SectionTitle => "Debug Settings";

    public IEnumerable<LensEntry> GetEntries()
    {
        yield return LensEntry.Toggle("God Mode", () => godMode, value => godMode = value);
        yield return LensEntry.Text("Environment", () => environment, value => environment = value);
        yield return LensEntry.Number("Coin Multiplier", () => coinMultiplier, value => coinMultiplier = value);
        yield return LensEntry.Slider("Rollout Percent", () => rollout, value => rollout = value, 0f, 100f, 5f);
        yield return LensEntry.Button("Unlock Content", UnlockContent, true, "Unlock all content?");
    }

    private void UnlockContent()
    {
        // Call project-owned debug code here.
    }
}
```

Button entries are useful for predefined debug actions, such as unlocking content, resetting tutorial state, opening an internal diagnostics panel, or showing a third-party in-game debug console. Lens does not depend on those tools directly; the consuming project wires that behavior in the callback.

Use confirmation for actions that mutate account, progression, inventory, tutorial, economy, or save state.

Text, number, slider, and multi-select entries draft locally and call setters only when applied. Toggle and single-select entries commit immediately when changed.

## Rich Entries

Use explicit option labels so UI, search, and reports stay stable:

```csharp
var environments = new[]
{
    new LensOption<string>("dev", "Development"),
    new LensOption<string>("stage", "Staging")
};

yield return LensEntry.SingleSelect("Environment", () => environment, value => environment = value, environments);
yield return LensEntry.MultiSelect("Rewards", () => rewards, values => rewards = values.ToList(), rewardOptions);
yield return LensEntry.Progress("Download", () => downloadedMb, () => totalMb, "Catalog");
```

Add info text when a value needs context:

```csharp
yield return LensEntry.Toggle("shop_v2", () => shopV2, value => shopV2 = value, infoText: "Controls the new shop flow.");
```

## Custom Entries

Register a project-owned IMGUI drawer for custom entry types:

```csharp
LensEntryDrawerRegistry.Register("my.graph", new MyGraphDrawer());

yield return LensEntry.Custom(
    "Spawn Graph",
    "my.graph",
    payload: graphData,
    displayValue: payload => "Graph available",
    searchText: payload => "spawn graph diagnostics",
    reportValue: payload => "Graph available");
```

Custom drawer exceptions are not swallowed. Lens is a debug tool, so failures should surface with useful Unity stack traces.

Custom drawers can inspect `LensEntryDrawContext.UiScale`, `IsCompact`, `LogicalScreenWidth`, and `LogicalScreenHeight` to adapt their own IMGUI controls.

## Responsive UI

Lens defaults to automatic scaling:

```csharp
var console = gameObject.AddComponent<LensRuntimeConsole>();
console.UiScaleMode = LensUiScaleMode.Auto;
```

For project-specific tuning, use fixed scale or auto-scale clamps:

```csharp
console.UiScaleMode = LensUiScaleMode.Fixed;
console.FixedUiScale = 1.5f;
console.SetAutoScaleLimits(1f, 3f);
```

The default auto mode uses DPI when available, falls back to portrait screen size when DPI is unavailable, and clamps scale to avoid unreadably small or oversized UI.

## Internal Build Policy

Lens is enabled by default only for Editor, Development Build, or builds compiled with `LENS_ENABLED`:

```csharp
if (LensRuntimePolicy.IsAllowed)
{
    LensSectionRegistry.Register(new MyGameLensSection());
}
```

Projects can override this at runtime for their own internal bootstrap:

```csharp
LensRuntimePolicy.SetAllowed(true);
LensRuntimePolicy.ResetToDefault();
```

`LensRuntimeConsole.Open()` and `Toggle()` do nothing when Lens is not allowed. `Close()` always works.

## Redacted Entries

Mark sensitive values explicitly. Lens keeps the raw value available to the provider-owned callback path, but the overlay, search, and copied reports use the redacted display value.

```csharp
yield return LensEntry.Text("User Token", () => token, value => token = value, true);
yield return new LensEntry("Install Id", installId, true);
```

Redaction is a safety aid, not a security boundary. Do not expose secrets, auth tokens, payment data, private player data, or anything that should never exist in an internal debug overlay.

## Debug Reports

Lens builds a readable plain text report from the currently registered providers:

```text
Lens Debug Report
Generated: 2026-06-09T12:30:00.0000000Z
Lens Version: 0.6.0

[Build Info]
App Version: 0.1.0
Unity Version: 6000.3.16f1
Platform: WindowsEditor
Active Scene: LensDemo
Debug Build: True
```

This is useful for QA bug reports, staging checks, and quick developer handoffs.

Interactive values are reported using their current callback values. Action buttons are listed as available actions and are never executed while building a report.
Sensitive values are reported as `[redacted]`.

## Production Notes

Lens is intended for Editor, Development Builds, or explicitly enabled internal builds.

For later versions, a project can wrap Lens initialization with:

```csharp
#if UNITY_EDITOR || DEVELOPMENT_BUILD || LENS_ENABLED
// Register Lens providers or create the Lens console.
#endif
```

Do not expose secrets, auth tokens, or private player data through custom providers.

## Future Integrations

Lens is designed so future systems can register their own sections without becoming package dependencies:

- Beacon can expose active environment, config source, config version, and evaluated feature flags.
- Pulse can expose recent analytics events, event counts, session context, and funnel/debug state.
- Signal can attach Lens debug reports to smoke-test or QA output.

These integrations are intentionally out of scope for V0.6.

## Roadmap

Potential future features:

- Screenshots and visual README assets
- JSON export
- Screenshot capture
- Local flag overrides
- Secure activation gesture
- Production-safe redaction rules
- Optional adapters for project DI patterns such as Zenject
- Optional richer UI path if IMGUI becomes limiting

## For Agents And Contributors

- See `AGENTS.md` for repo-level coding-agent guidance.
- See `Docs/agent-usage.md` for how agents should expose important runtime values in consuming Unity projects.
- See `CONTRIBUTING.md` for validation and versioning guidance.

## License

MIT
