# Reconciliation browser tests

These tests open the regular `/admin/locations/990001` page and use the combined **Reconcile Approaches → Lanes** dialog. They do not use the `/dev/reconcile-scenarios` page or inject editor state.

Start the WebUI, then run from `Atspm/WebUI` in PowerShell:

```powershell
$env:TMC_E2E_URL = 'http://localhost:3080'
npm run test:e2e -- e2e/reconcile-location.mock.spec.ts
```

For interactive debugging, add `--headed` or `--ui`. Playwright Chromium must be installed (`npx playwright install chromium`). No camera, database credentials, login, or enabled server-side feature flag is needed. The browser mocks the feature flag, configuration, profile, zone inventory, approach comparison, bin-statistics report, and approach save responses. All API traffic is intercepted; unmatched writes are blocked and fail assertions. Map services are disabled in this test browser. The existing hardware smoke tests are separate and are not selected by this command.

The reviewed lane fixtures are in `fixtures/mockScenarios.ts`. Assertions independently specify the expected saved channels and movements. Tests verify split previews, separate turn additions, optional removal warnings, unchanged valid configurations, ambiguous/low-volume warnings, separate Through lanes on different zones, the disabled feature flag, individual turn selection with an edited channel, and closing without applying. Lane changes must not write to the API when applied; only explicitly saving the approach sends the intercepted payload.

All 43 approach fixtures in `fixtures/approachMockScenarios.json` also run on the regular location page. Each test checks expected warnings, stages offered phase/overlap and detector additions, removes missing assignments or explicitly deletes the whole approach when no vehicle assignment would remain, and saves every retained/new approach. Assertions compare saved phase numbers, overlap flags, and detector channels/ownership with the expected configuration. Warning-only and already-matching scenarios verify unchanged save payloads. Confirmed detector/whole-approach deletions are immediate API writes and are asserted separately; assignment edits and additions must remain local until Save. Unexpected API calls are blocked.

On failure, Playwright retains a trace in `test-results`. Open it with `npx playwright show-trace <trace.zip>`.

## Interactive scenario tool

The standalone interactive tool is preserved on branch `feature/scenario-mock-tool` and is not included in this branch. Regular-location tests above exercise the real location page using API interception.

The backend test ApproachScenarioFixtureTests verifies the approach fixtures against the real reconciliation rules. To intentionally regenerate them, set REGENERATE_APPROACH_SCENARIOS=1, run that test, review the diff, then unset the variable and rerun. Ordinary tests do not rewrite fixtures.
