# Changelog

## 0.6.0 - Responsive Mobile Layout

- Added automatic DPI/screen-based IMGUI scaling.
- Added compact stacked layout for narrow portrait screens.
- Added runtime scale settings on `LensRuntimeConsole`.
- Exposed layout info to custom entry drawers.
- Added tests for scale calculation, compact detection, and floating button clamping.

## 0.5.0 - Extensible Rich Entries

- Added custom entry drawer registry and custom entry payload support.
- Added info text, sliders, single-select, multi-select, and progress entries.
- Changed Lens runtime/report/action behavior to fail fast instead of swallowing exceptions.
- Clarified value-change callbacks and commit behavior for editable entries.
- Updated sample controls and removed the intentional failing sample action.

## 0.4.0 - Production Safety

- Added `LensRuntimePolicy` for explicit internal-build enablement.
- Added sensitive entry metadata and redacted UI/report display values.
- Added optional confirmation for action button entries.
- Added Lens package version to copied debug reports.
- Updated sample and docs with safe internal-build usage guidance.

## 0.3.0 - Quality Release

Planned/implemented focus:

- Add runtime-safe package tests.
- Add agent usage guidance.
- Add package contribution and validation docs.
- Add lightweight GitHub package validation.
- Improve README usage guidance for public GitHub consumption.

## 0.2.0 - Interactive Runtime Console

- Added mobile-friendly floating Lens button.
- Added foldable sections and search.
- Added typed entries for toggles, editable text, editable numbers, and action buttons.
- Added provider-owned callbacks for mutations and actions.
- Split console behavior into smaller focused runtime collaborators.

## 0.1.0 - Minimal Runtime Debug Console Foundation

- Added UPM package structure.
- Added provider registry and read-only entries.
- Added IMGUI runtime overlay.
- Added built-in sample providers.
- Added copyable debug report.
- Added basic sample scene.
