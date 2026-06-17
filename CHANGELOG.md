# Changelog

## 1.0.1 - Mobile Portrait Polish

- Slimmed compact portrait controls and footer buttons.
- Moved compact info buttons beside entry labels to avoid empty value-row gaps.
- Improved compact progress entry rendering by separating the label from the progress bar.
- Added README screenshots for the runtime overlay and text/JSON report output.

## 1.0.0 - Stabilization And QA Retrieval

- Added domain-reload-off runtime reset for static Lens state.
- Added `LensSectionBehaviour` for scene-owned provider registration.
- Added report schema versioning, device/app/build metadata, report file export, and native share support.
- Added descriptive wrong-kind guards for `LensEntry` accessors.
- Added provider lifecycle, report export, and API hardening tests.
- Documented provider allocation guidance and main-thread-only usage.

### Behavior Changes

- Wrong-kind `LensEntry` access now throws descriptive `InvalidOperationException` errors.
- `LensSectionRegistry.Unregister(null)` now throws `ArgumentNullException`, matching `Register(null)`.
- Static registry, drawer registry, runtime policy, and report metadata state now resets on subsystem registration.

## 0.10.0 - Runtime Performance Pass

- Added refresh interval control and explicit refresh support on `LensRuntimeConsole`.
- Added internal provider entry caching to reduce per-frame provider polling.
- Avoided fetching collapsed section entries unless search or rendering requires them.
- Reduced repeated value reads and draft lookups in entry drawing paths.
- Documented provider performance expectations.

## 0.9.0 - Repository Trust And Samples

- Added README badges, quick start guidance, and an architecture diagram.
- Added API overview and internal-build safety documentation.
- Added GitHub issue templates for bugs, features, and integration questions.
- Added a dependency-free Provider Cookbook sample.
- Documented roadmap priorities and repository trust signals.

## 0.8.0 - Public Portfolio Polish

- Added public-facing design goals to the README.
- Added an optional manual Unity test workflow for GitHub Actions.
- Documented GameCI license setup for package test validation.
- Prepared the package for a tagged GitHub release.

## 0.7.0 - QA Evidence Bundle

- Added JSON debug report export.
- Added local screenshot capture for QA evidence.
- Added report footer actions for copying text, copying JSON, and capturing screenshots.
- Included optional screenshot path metadata in text and JSON reports.
- Documented report artifact location and QA usage.

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
