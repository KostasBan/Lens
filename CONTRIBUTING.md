# Contributing To Lens

Lens is a small Unity runtime debug console package. Contributions should keep it focused, dependency-free, and easy to install through Unity Package Manager.

## Local Validation

Use Unity `6000.3.16f1` or newer.

Recommended local flow:

1. Create or open a clean Unity project outside this package root.
2. Add Lens through Package Manager using a local path or Git URL.
3. Add `com.kostasban.lens` to the project's `testables` list.
4. Run EditMode tests from Unity Test Runner.
5. Import the `Basic Lens Demo` sample and confirm it compiles.

The package should also import in projects using either legacy Input Manager, Input System, or both.

## Package Rules

- Runtime source belongs in `Runtime/`.
- Runtime tests belong in `Tests/Runtime/`.
- Samples belong in `Samples~/`.
- Keep public APIs small and backward-compatible when practical.
- Do not add runtime dependencies without documenting why.
- Do not make Lens depend on game-specific systems such as remote config, analytics, or debug console packages.

## Versioning

Use package versions to communicate intent:

- Patch: bug fixes and docs.
- Minor: additive public API or sample improvements.
- Major: breaking public API changes.

Tag releases with the package version, for example `v0.6.0`.

## Safety

Interactive entries can mutate runtime state. Keep names clear, avoid exposing secrets, and gate risky project-owned actions in the consuming project.

- Use `LensRuntimePolicy` for internal-build enablement.
- Mark sensitive values with redaction instead of relying on docs alone.
- Require confirmation for destructive or hard-to-undo action buttons.
- Preserve fail-fast debugging behavior unless a future plan explicitly changes it.
- Keep custom entry support generic; project-specific controls should live in consuming projects.
- Avoid new hard-coded UI widths in runtime drawing code; route layout sizing through the responsive metrics layer.
