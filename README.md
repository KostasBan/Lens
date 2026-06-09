# Lens Runtime Debug Console

Lens is a small runtime debug/inspection panel for Unity projects. It gives developers and QA a quick way to inspect useful in-build state without attaching a debugger or wiring every system directly into one UI.

Lens is intentionally generic. Runtime systems register section providers, Lens renders those sections, and QA can copy a readable debug report for bug reproduction.

## Why Lens Exists

Many Unity bugs are hard to reproduce because the important runtime context is scattered across systems: build version, active scene, platform, environment, session, feature-like values, recent events, and performance. Lens puts that context into one provider-based overlay.

V0.1 is deliberately small:

- Runtime IMGUI overlay
- `F1` keyboard toggle
- Provider-based sections
- Built-in sample sections
- Copy-to-clipboard debug report
- Basic sample scene

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
6. Press `F1` to open or close Lens.
7. Click `Copy Debug Report` to copy the current section data.

The sample registers five providers:

- Build Info
- Session Info
- Sample Feature Flags
- Sample Recent Events
- Performance

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

## Debug Reports

Lens builds a readable plain text report from the currently registered providers:

```text
Lens Debug Report
Generated: 2026-06-09T12:30:00.0000000Z

[Build Info]
App Version: 0.1.0
Unity Version: 6000.3.16f1
Platform: WindowsEditor
Active Scene: LensDemo
Debug Build: True
```

This is useful for QA bug reports, staging checks, and quick developer handoffs.

## Production Notes

Lens is intended for Editor, Development Builds, or explicitly enabled internal builds. V0.1 keeps gating simple and visible in code rather than adding a security system too early.

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

These integrations are intentionally out of scope for V0.1.

## Roadmap

Potential future features:

- Search/filter entries
- Collapsible sections
- Favorite pinned values
- JSON export
- Screenshot capture
- Custom action buttons
- Local flag overrides
- Secure activation gesture
- Production-safe redaction rules

## License

MIT
