# Future Lens Integrations

Lens is intentionally generic. Future systems should integrate by registering their own `ILensSectionProvider` or `ILensIdentifiedSectionProvider`; Lens should not depend on those packages directly.

## Beacon

Beacon could expose active environment, config source, config version, evaluated flags, rollout values, and remote-config health. A Beacon provider should use a stable `SectionId` such as `beacon.remote-config`.

## Pulse

Pulse could expose session context, recent analytics events, event counts, upload status, and funnel/debug state. Reports would make QA tickets easier to reproduce without coupling Lens to analytics implementation details.

## Signal

Signal could attach Lens text or JSON reports to smoke-test output, internal QA runs, or release-confidence tooling. Signal should request reports through Lens report APIs rather than executing Lens UI actions.

## Integration Rule

Each future package owns its data, caching, permissions, and side effects. Lens renders provider entries and builds reports from the values providers expose.
