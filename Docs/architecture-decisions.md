# Lens Architecture Decisions

## Provider-Based Sections

Lens uses provider-owned sections so runtime systems can expose debug state without Lens depending on those systems. A provider owns the data, callbacks, validation, permissions, and side effects; Lens owns rendering, search, lightweight UI state, and report generation.

## IMGUI For V1

Lens uses IMGUI because it is available in Unity projects without extra runtime dependencies. This keeps the package easy to install through UPM and useful in existing projects that do not share a UI stack.

## Internal-Build Policy

Lens defaults to Editor, Development Build, or `LENS_ENABLED` usage. This makes internal build enablement explicit while still letting projects override the policy during bootstrap.

## Provider-Owned State And Callbacks

Interactive entries use provider-owned getters and setters. Lens invokes callbacks, but it does not store authoritative gameplay, config, account, save, inventory, or analytics state.

## Reports Never Execute Actions

Text and JSON reports read current values and list available actions, but they never execute action entries. Report generation should be safe to trigger during QA triage.
