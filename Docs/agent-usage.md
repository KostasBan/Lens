# Using Lens From Agent-Generated Unity Work

This guide is for coding agents adding or modifying Unity gameplay, config, analytics, QA, or production tooling code in a project that includes Lens.

## Default Behavior

When an important runtime value would help QA or developers understand behavior, add or update a Lens section provider instead of relying only on logs.

Good candidates:

- active environment or config source,
- evaluated feature flags,
- player/session/debug identifiers safe for sharing,
- current scene or flow state,
- recent analytics/product events,
- content unlock status,
- performance or loading diagnostics,
- safe debug actions.

Use Lens only from Editor, Development Build, staging, QA, dogfood, or explicitly enabled internal builds. Check `LensRuntimePolicy.IsAllowed` before creating package-owned bootstrap objects in consuming projects.

## Add A Section

Create a class that implements `ILensSectionProvider` near the system that owns the data.

```csharp
using System.Collections.Generic;
using KostasBan.Lens;

public sealed class EconomyLensSection : ILensSectionProvider
{
    private readonly EconomyService economy;

    public EconomyLensSection(EconomyService economy)
    {
        this.economy = economy;
    }

    public string SectionTitle => "Economy";

    public IEnumerable<LensEntry> GetEntries()
    {
        yield return new LensEntry("Coins", economy.Coins.ToString());
        yield return LensEntry.Toggle("Double Rewards", () => economy.DoubleRewards, value => economy.DoubleRewards = value);
        yield return LensEntry.Text("Session Token", () => economy.SessionToken, value => economy.SessionToken = value, true);
        yield return LensEntry.Button("Grant Test Coins", () => economy.GrantCoins(100), true, "Grant 100 test coins?");
    }
}
```

Register the provider from bootstrap or system initialization:

```csharp
LensSectionRegistry.Register(new EconomyLensSection(economy));
```

Unregister the same provider when its owner is disposed or destroyed.

## Safe Mutable Entries

Use mutable entries only when changing the value is useful for testing and the owning system can safely apply the change.

- `LensEntry.Toggle` for feature flags and booleans.
- `LensEntry.Text` for labels, environment names, IDs safe to share, or debug strings.
- `LensEntry.Number` for tuning values.
- `LensEntry.Button` for explicit debug actions.

Do not expose destructive actions with vague names. Prefer labels like `Reset Local Tutorial State` over `Reset`.
Mark useful sensitive values as redacted with the `isSensitive` overload. Use confirmation for actions that mutate progression, inventory, account state, save data, tutorial state, or content unlocks.

## Third-Party Debug Panels

Lens can expose a button that opens another project-owned tool, such as an in-game command console or diagnostics panel. Do not add a dependency from Lens to that tool. The consuming project should call its own API from the callback.

```csharp
yield return LensEntry.Button("Open Debug Console", debugConsole.Show);
```

## Report Behavior

Lens reports read current values from callbacks. Action buttons are listed as available actions and are not executed while generating reports. Sensitive entries are emitted as `[redacted]`, and search does not match their raw values.
