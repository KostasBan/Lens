# Security Policy

Lens is a runtime debugging aid for Unity Editor, development builds, staging, QA, dogfood, and explicitly enabled internal builds.

Lens is not intended to be exposed as an unmanaged production-user feature.

## Supported Usage

- Gate Lens with `LensRuntimePolicy`, build symbols, project-side bootstrap rules, or internal build configuration.
- Mark useful but sensitive values as redacted.
- Require confirmation for risky debug actions.
- Keep project-owned permissions, validation, and side effects inside the consuming project.

## Do Not Expose

- Auth tokens, API keys, secrets, or credentials.
- Private player data or payment data.
- Raw internal identifiers that should not appear in copied reports.
- Destructive or irreversible actions without clear labels and project-side gating.

## Redaction

Lens redaction is a safety aid for overlays, search, and copied reports. It is not a security boundary. The safest value is one that is never exposed to Lens in the first place.

## Reporting Issues

For public package issues, use the GitHub issue templates. Do not include secrets, private project data, player data, or production credentials in issues, logs, screenshots, or pasted reports.
