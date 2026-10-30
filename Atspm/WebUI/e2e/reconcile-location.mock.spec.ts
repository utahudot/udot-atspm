import approachScenarios from './fixtures/approachMockScenarios.json'
import { expect, test, type Page } from '@playwright/test'
import {
  loadMockScenario,
  mockScenarios,
} from './fixtures/mockScenarios'

// Exercise the production location editor. Only its HTTP boundary is replaced;
// reconciliation, selection, staging and save serialization all run normally.
const locationId = 990001
test.use({ serviceWorkers: 'block' })
const enums = {
  DirectionTypes: ['NA', 'NB', 'SB', 'EB', 'WB'],
  MovementTypes: ['NA', 'T', 'R', 'L', 'TR', 'TL', 'NW', 'LTR'],
  LaneTypes: ['NA', 'V'],
  DetectionHardwareTypes: ['Unknown', 'Video'],
  DetectionTypes: ['Unknown', 'Basic', 'Advanced', 'Count', 'LaneByLaneCount'],
}
const metadata = `<Schema>${Object.entries(enums)
  .map(
    ([name, values]) =>
      `<EnumType Name="${name}">${values.map((name, value) => `<Member Name="${name}" Value="${value}"/>`).join('')}</EnumType>`
  )
  .join('')}</Schema>`

async function openScenario(page: Page, index: number, lanes = true, approachScenario?: typeof approachScenarios[number], decoder = 'VisionCameraAPI', unusedZone = false, configure?: (fixture: ReturnType<typeof loadMockScenario>) => void) {
  // Leaflet's pending zoom transition can outlive the General tab on unmount.
  await page.addInitScript('window.L_DISABLE_3D = true')

  const fixture = loadMockScenario(index)
  if (approachScenario) fixture.approaches = approachScenario.location.approaches.map(a => ({
    ...a, directionTypeId: 'NA',
    detectors: a.detectors.map(d => ({ ...d, detectionTypes: [], movementType: 'T', laneType: 'V', detectionHardware: 'Video' })),
  })) as any
  if (unusedZone) {
    fixture.inventory[0].names.push('SB-36')
    fixture.evidence.push({ deviceId: 1, zoneName: 'SB-36', through: 1, left: 0, right: 0, bins: 1 })
  }
  configure?.(fixture)
  const device = {
    id: 1,
    deviceStatus: 'Active',
    deviceType: 'FIRCamera',
    deviceIdentifier: 'mock-camera',
    ipaddress: '192.0.2.1',
    deviceConfiguration: { port: 80 },
    deviceProperties: { TmcDecoder: decoder },
  }
  const location = {
    id: locationId,
    locationIdentifier: 'MOCK',
    primaryName: 'Mock',
    secondaryName: 'Intersection',
    latitude: 40,
    longitude: -111,
    start: '2026-01-01T00:00:00',
    areas: [],
    devices: [device],
    approaches: fixture.approaches.map((a) => ({
      ...a,
      locationId,
      detectors: a.detectors.map((d) => ({
        ...d,
        dateAdded: '2026-01-01T00:00:00',
        detectionTypes: [{ id: 4, abbreviation: 'LaneByLaneCount' }],
      })),
    })),
  }
  const writes: any[] = []
  const deletions: { kind: string; id: number }[] = []
  const reports: any[] = []
  const unexpected: string[] = []
  page.on('pageerror', (error) => unexpected.push(error.message))
  let syncs = 0
  const origin = new URL(test.info().project.use.baseURL as string).origin
  await page.context().addCookies([
    { name: 'loggedIn', value: 'true', url: origin },
    { name: 'claims', value: 'Admin', url: origin },
  ])
  // Deny all API fallthrough, including writes and camera access. Test data
  // cannot reach a real service even if a new endpoint is introduced.
  await page.route('**/*', async (route) => {
    const request = route.request()
    const url = new URL(request.url())
    const path = url.pathname
    const json = (body: unknown) => route.fulfill({ json: body })
    if (path === '/api/get-env')
      return json({
        CONFIG_URL: `${origin}/mock/config`,
        REPORTS_URL: `${origin}/mock/reports`,
        IDENTITY_URL: `${origin}/mock/identity`,
        DATA_URL: `${origin}/mock/data`,
        MAP_DEFAULT_LATITUDE: '40',
        MAP_DEFAULT_LONGITUDE: '-111',
        MAP_DEFAULT_ZOOM: '12',
        MAP_TILE_LAYER: '',
        MAP_TILE_ATTRIBUTION: 'Mock',
      })
    if (path === '/api/google/tiles/session')
      return route.fulfill({
        status: 503,
        json: { error: 'Map tiles disabled in reconciliation tests' },
      })
    if (path === '/api/feature-flags')
      return json({ laneReconciliation: lanes, speedManagementTool: false })
    if (path.endsWith('/Profile'))
      return json({ firstName: 'Mock', lastName: 'Admin' })
    if (path.endsWith('/$metadata'))
      return route.fulfill({ contentType: 'application/xml', body: metadata })
    if (path.endsWith(`/Location/${locationId}`)) return json(location)
    if (
      path.endsWith('/GetLatestVersionOfAllLocations') ||
      path.endsWith('/GetLocationsForSearch')
    )
      return json({ value: [location] })
    if (path.endsWith('/SyncLocation')) {
      syncs++
      return json(approachScenario?.expected ?? {})
    }
    if (path.includes('/Device/GetActiveDevicesByLocation'))
      return json([device])
    if (path.endsWith('/Detector/retrieveDetectionData'))
      return json(fixture.inventory[0].names)
    if (path.endsWith('/TurningMovementCounts/getReportData')) {
      reports.push(request.postDataJSON())
      return json({
        zoneEvidence: fixture.evidence,
        sourceDeviceIds: [1],
        warnings: unusedZone ? ['Another camera returned incomplete data'] : [],
      })
    }
    const deletion = path.match(/\/(Approach|Detector)\/(\d+)$/)
    if (deletion && request.method() === 'DELETE') {
      deletions.push({ kind: deletion[1], id: Number(deletion[2]) })
      return route.fulfill({ status: 204 })
    }
    if (path.endsWith('/UpsertApproach')) {
      const body = request.postDataJSON()
      writes.push(body)
      return json({ ...body, id: body.id || 10000 + writes.length })
    }
    if (path.startsWith('/mock/') && request.method() === 'GET')
      return json({ value: [] })
    if (
      path.startsWith('/api/') ||
      path.startsWith('/mock/') ||
      request.method() !== 'GET' ||
      url.origin !== origin
    ) {
      unexpected.push(`${request.method()} ${path}`)
      return route.abort()
    }
    return route.continue()
  })
  await page.goto(`/admin/locations/${locationId}`)
  await expect(page.locator('.leaflet-container').first()).toBeVisible()
  await page.getByRole('tab', { name: 'Approaches', exact: true }).click()
  await page
    .getByRole('button', { name: 'Reconcile Approaches', exact: true })
    .click()
  await expect.poll(() => syncs).toBe(1)
  expect(reports).toHaveLength(0)
  if (lanes) {
    await page.getByRole('tab', { name: 'Lanes', exact: true }).click()
    await page
      .getByLabel('Start (location time)', { exact: true })
      .fill('2026-10-01T00:00')
    await page
      .getByLabel('End (exclusive)', { exact: true })
      .fill('2026-10-02T00:00')
    await page
      .getByRole('button', { name: 'Compare lanes', exact: true })
      .click()
    await expect(
      page.getByRole('table', { name: 'Observed zone counts' })
    ).toBeVisible()
    expect(reports).toHaveLength(1)
    expect(reports[0]).toMatchObject({
      locationIdentifier: 'MOCK',
      decoder,
      deviceIds: [1],
      reconcileLanes: true,
      start: '2026-10-01T00:00',
      end: '2026-10-02T00:00',
    })
  }
  return { fixture, writes, deletions, reports, unexpected }
}

const expected = [
  [
    [118, 'T'],
    [65, 'R'],
  ],
  [
    [118, 'T'],
    [65, 'L'],
  ],
  [
    [118, 'L'],
    [65, 'T'],
  ],
  [
    [118, 'T'],
    [65, 'L'],
    [66, 'R'],
  ],
  [
    [118, 'L'],
    [119, 'R'],
    [65, 'T'],
  ],
  [[118, 'T']],
  [
    [118, 'T'],
    [119, 'L'],
    [120, 'R'],
  ],
  [
    [118, 'TL'],
    [119, 'TR'],
  ],
  [[118, 'T']],
  [
    [118, 'T'],
    [119, 'T'],
  ],
  [[118, 'T'], [119, 'R']],
]
const additions = [1, 1, 1, 2, 1, 0, 0, 0, 0, 0, 1]
for (const [index, scenario] of mockScenarios.entries()) {
  test(scenario.name, async ({ page }) => {
    const state = await openScenario(page, index)
    const dialog = page.getByRole('dialog')
    await expect(
      dialog.getByText(`Recommended lane changes (${additions[index]})`, {
        exact: true,
      })
    ).toBeVisible()
    if ([5, 7, 8, 10].includes(index))
      await expect(
        dialog.getByText(/Warnings — verify manually \([1-9]/)
      ).toBeVisible()
    else
      await expect(
        dialog.getByText('Warnings — verify manually (0)', { exact: true })
      ).toBeVisible()
    if (index === 5)
      await expect(
        dialog.getByText(/Zero counts do not establish a turn-only lane/)
      ).toBeVisible()
    if (index === 7)
      await expect(
        dialog.getByText(/overlapping movement assignments can double-count/)
      ).toBeVisible()
    if (index === 8)
      await expect(
        dialog.getByText(/low-volume unassigned movements/)
      ).toBeVisible()
    if (index < 2)
      await expect(
        dialog.getByText(
          new RegExp(
            'Channel 118 zone NB-18 will change from Through/.* to Through'
          )
        )
      ).toBeVisible()
    if (index === 9) {
      const rows = dialog.getByRole('table').getByRole('row')
      await expect(rows).toHaveCount(3)
      await expect(rows.nth(1)).toContainText('NB-18')
      await expect(rows.nth(1)).toContainText('120')
      await expect(rows.nth(2)).toContainText('NB-19')
      await expect(rows.nth(2)).toContainText('85')
    }
    const choices = dialog.getByRole('button', {
      name: /^(Add|Remove|Change) Channel/,
    })
    await expect(choices).toHaveCount(additions[index] + (index === 5 ? 1 : 0))
    await expect(
      dialog.getByRole('button', { name: 'Apply selected to editor' })
    ).toBeDisabled()
    for (const choice of await choices.all()) {
      await expect(choice).toHaveAttribute('aria-pressed', 'false')
      await choice.click()
    }
    expect(state.writes).toHaveLength(0)
    if (await choices.count()) {
      await dialog
        .getByRole('button', { name: 'Apply selected to editor' })
        .click()
      await expect(
        dialog.getByText(/Changes staged in the location editor/)
      ).toBeVisible()
    }
    expect(state.writes).toHaveLength(0)
    await dialog.getByRole('button', { name: 'Close', exact: true }).click()
    await page
      .getByRole('button', { name: 'save approach', exact: true })
      .click()
    await expect.poll(() => state.writes.length).toBe(1)
    const detectors = state.writes[0].detectors
    const actual = detectors
      .map((d: any) => [d.detectorChannel, enums.MovementTypes[d.movementType]])
      .sort((a: any, b: any) => a[0] - b[0])
    expect(actual).toEqual(
      [...expected[index]].sort((a, b) => Number(a[0]) - Number(b[0]))
    )
    for (const d of detectors) {
      expect(d.dectectorIdentifier).toBe(
        index === 9 && d.detectorChannel === 119 ? 'NB-19' : 'NB-18'
      )
      expect(d.laneNumber).toBe(
        [7, 9].includes(index) && d.detectorChannel === 119 ? 2 : 1
      )
    }
    expect(state.unexpected).toEqual([])
  })
}

test('disabled flag retains approaches without requesting lane evidence', async ({
  page,
}) => {
  const state = await openScenario(page, 0, false)
  await expect(page.getByRole('dialog')).toHaveCount(0)
  await expect(
    page.getByRole('tab', { name: 'Lanes', exact: true })
  ).toHaveCount(0)
  expect(state.reports).toEqual([])
  expect(state.writes).toEqual([])
  expect(state.unexpected).toEqual([])
})

for (const movement of ['Left', 'Right']) {
  test(`both turns missing: select only ${movement} and edit its channel`, async ({
    page,
  }) => {
    const state = await openScenario(page, 3)
    const dialog = page.getByRole('dialog')
    await dialog
      .getByRole('spinbutton', { name: `Channel for NB-18 ${movement}` })
      .fill('130')
    await dialog
      .getByRole('button', {
        name: `Add Channel 130 zone NB-18 ${movement}`,
        exact: true,
      })
      .click()
    await dialog
      .getByRole('button', { name: 'Apply selected to editor' })
      .click()
    await expect(
      dialog.getByText(/Changes staged in the location editor/)
    ).toBeVisible()
    expect(state.writes).toHaveLength(0)
    await dialog.getByRole('button', { name: 'Close', exact: true }).click()
    await page
      .getByRole('button', { name: 'save approach', exact: true })
      .click()
    await expect.poll(() => state.writes.length).toBe(1)
    expect(
      state.writes[0].detectors.map((d: any) => ({
        channel: d.detectorChannel,
        movement: d.movementType,
        zone: d.dectectorIdentifier,
        lane: d.laneNumber,
      }))
    ).toEqual([
      { channel: 118, movement: 1, zone: 'NB-18', lane: 1 },
      {
        channel: 130,
        movement: movement === 'Left' ? 3 : 2,
        zone: 'NB-18',
        lane: 1,
      },
    ])
    expect(state.unexpected).toEqual([])
  })
}

test('closing a selected split without applying preserves the original lane', async ({
  page,
}) => {
  const state = await openScenario(page, 0)
  const dialog = page.getByRole('dialog')
  await dialog
    .getByRole('button', {
      name: 'Add Channel 65 zone NB-18 Right',
      exact: true,
    })
    .click()
  await dialog.getByRole('button', { name: 'Close', exact: true }).click()
  expect(state.writes).toHaveLength(0)
  await page.getByRole('button', { name: 'save approach', exact: true }).click()
  await expect.poll(() => state.writes.length).toBe(1)
  expect(state.writes[0].detectors).toHaveLength(1)
  expect(state.writes[0].detectors[0]).toMatchObject({
    detectorChannel: 118,
    movementType: 4,
    dectectorIdentifier: 'NB-18',
    laneNumber: 1,
  })
  expect(state.unexpected).toEqual([])
})


// Use every shared approach fixture on the production location page. Expected
// responses are also checked against the real backend by ApproachScenarioFixtureTests.
// These assertions test UI decisions and save payloads, not a mocked save implementation.
for (const scenario of approachScenarios) {
  test(`approach scenario: ${scenario.name}`, async ({ page }) => {
    const state = await openScenario(page, 0, false, scenario)
    const expectedApproaches: any[] = structuredClone(state.fixture.approaches)
    const expectedDeletes: { kind: string; id: number }[] = []
    const result = scenario.expected
    for (const warning of result.reconciliationWarnings)
      await expect(page.getByText(warning, { exact: true })).toBeVisible()
    expect(state.writes).toEqual([])
    expect(state.deletions).toEqual([])

    // Scope identical phase/overlap numbers to their separate Found cards.
    const foundCard = (title: string) => page.getByText(title, { exact: true })
      .locator('..').locator('..')
    for (const [title, numbers, overlap] of [
      ['Phases', result.loggedButUnusedProtectedOrPermissivePhases, false],
      ['Overlaps', result.loggedButUnusedOverlapPhases, true],
    ] as const) {
      const card = foundCard(title)
      await expect(card.getByRole('button', { name: /^Add Selected/ })).toBeDisabled()
      for (const number of numbers)
        await card.getByRole('button', { name: String(number), exact: true }).click()
      if (!numbers.length) continue
      await card.getByRole('button', { name: /^Add Selected/ }).click()
      const draftDialog = page.getByRole('dialog', { name: 'Configure approaches before adding' })
      for (const [index, number] of numbers.entries()) {
        const cell = draftDialog.locator('[data-row="0"][data-col="1"]').nth(index)
        await cell.click()
        await cell.press('Enter')
        await cell.getByRole('textbox').fill(`Added ${title} ${number}`)
        await cell.getByRole('textbox').press('Enter')
      }
      await page.getByRole('button', { name: 'Add approaches', exact: true }).click()
      expectedApproaches.push(...numbers.map(number => ({
        description: `Added ${title} ${number}`, protectedPhaseNumber: number,
        isProtectedPhaseOverlap: overlap, permissivePhaseNumber: null,
        pedestrianPhaseNumber: null, detectors: [],
      })))
    }
    expect(state.writes).toEqual([]) // additions are staged, never saved implicitly

    for (const channel of result.loggedButUnusedDetectorChannels) {
      await page.getByRole('button', { name: String(channel), exact: true }).click()
      await page.getByRole('button', { name: /Add selected to/ }).click()
      const target = expectedApproaches[0]
      await page.getByRole('menuitem', { name: target.description, exact: true }).click()
      await page.getByRole('button', { name: 'Add detectors', exact: true }).click()
      target.detectors.push({ detectorChannel: channel })
    }
    expect(state.writes).toEqual([])

    // Delete only explicitly selected missing detectors. The production store
    // immediately persists these deletes, unlike assignment edits.
    for (const channel of result.removedDetectors) {
      const owner = expectedApproaches.find(a => a.detectors.some((d: any) => String(d.detectorChannel) === channel))
      const detector = owner.detectors.find((d: any) => String(d.detectorChannel) === channel)
      await page.getByRole('region', { name: 'Not Found detector channels' })
        .getByRole('button', { name: channel, exact: true }).click()
      await page.getByRole('button', { name: 'Remove selected', exact: true }).click()
      await page.getByRole('button', { name: 'Confirm removal', exact: true }).click()
      expectedDeletes.push({ kind: 'Detector', id: detector.id })
      owner.detectors = owner.detectors.filter((d: any) => d.id !== detector.id)
    }

    for (const before of scenario.location.approaches) {
      const missing = result.missingPhaseAssignments.filter(a => a.approachId === before.id)
      if (!missing.length) continue
      const target = expectedApproaches.find(a => a.id === before.id)
      const card = page.getByRole('region', { name: `Current configuration: ${before.description}` })
      const retainedVehicle = ['protected', 'permissive'].some(role =>
        Number((before as any)[`${role}PhaseNumber`]) > 0 && !missing.some(a => a.role === role))
      if (!retainedVehicle) {
        // Clearing the last vehicle assignment is invalid. Explicit whole-approach
        // deletion is a different action, including its owned detectors.
        if (missing.filter(a => a.role !== 'pedestrian').length === 1)
          await expect(card.getByRole('button', { name: /^Remove (protected|permissive)/ })).toHaveCount(0)
        await card.getByRole('button', { name: /^Delete entire approach:/ }).click()
        await page.getByRole('button', { name: 'Delete entire approach', exact: true }).click()
        expectedDeletes.push({ kind: 'Approach', id: before.id })
        expectedApproaches.splice(expectedApproaches.indexOf(target), 1)
      } else {
        for (const assignment of missing) {
          await card.getByRole('button', { name: `Remove ${assignment.role} ${assignment.isOverlap ? 'overlap' : 'phase'} ${assignment.phaseNumber}`, exact: true }).click()
          await page.getByRole('button', { name: 'Confirm removal', exact: true }).click()
          target[`${assignment.role}PhaseNumber`] = assignment.role === 'protected' ? 0 : null
          target[`is${assignment.role[0].toUpperCase()}${assignment.role.slice(1)}PhaseOverlap`] = false
        }
      }
    }
    await expect.poll(() => state.deletions).toEqual(expectedDeletes)
    expect(state.writes).toEqual([])
    // Saving all retained/new approaches also checks that warnings and inactive
    // events did not silently change unaffected assignments or detector ownership.
    const saves = page.getByRole('button', { name: 'save approach', exact: true })
    await expect(saves).toHaveCount(expectedApproaches.length)
    for (let index = 0; index < expectedApproaches.length; index++) {
      await page.getByRole('heading', { name: expectedApproaches[index].description, exact: true, level: 3 })
        .locator('../../..').getByRole('button', { name: 'save approach', exact: true }).click()
      await expect.poll(() => state.writes.length).toBe(index + 1)
      await expect(page.getByText('Approach saved successfully', { exact: true }).first()).toBeVisible()
    }
    const configuration = (a: any) => ({
      description: a.description,
      protected: a.protectedPhaseNumber == null ? null : Number(a.protectedPhaseNumber),
      permissive: a.permissivePhaseNumber == null ? null : Number(a.permissivePhaseNumber),
      pedestrian: a.pedestrianPhaseNumber == null ? null : Number(a.pedestrianPhaseNumber),
      protectedOverlap: Boolean(a.isProtectedPhaseOverlap),
      permissiveOverlap: Boolean(a.isPermissivePhaseOverlap),
      pedestrianOverlap: Boolean(a.isPedestrianPhaseOverlap),
      channels: a.detectors.map((d: any) => d.detectorChannel).sort((a: number, b: number) => a - b),
    })
    const sorted = (rows: any[]) => rows.map(configuration).sort((a, b) => JSON.stringify(a).localeCompare(JSON.stringify(b)))
    expect(sorted(state.writes)).toEqual(sorted(expectedApproaches))
    expect(state.reports).toEqual([])
    expect(state.unexpected).toEqual([])
  })
}

test('lane selection and edited channel survive switching reconciliation tabs', async ({ page }) => {
  const state = await openScenario(page, 0)
  const channel = page.getByLabel('Channel for NB-18 Right')
  await channel.fill('121')
  const addition = page.getByRole('button', { name: 'Add Channel 121 zone NB-18 Right', exact: true })
  await addition.click()
  await expect(addition).toHaveAttribute('aria-pressed', 'true')
  await page.getByRole('tab', { name: 'Approaches', exact: true }).click()
  await page.getByRole('tab', { name: 'Lanes', exact: true }).click()
  await expect(channel).toHaveValue('121')
  await expect(addition).toHaveAttribute('aria-pressed', 'true')
  expect(state.reports).toHaveLength(1)
  expect(state.writes).toHaveLength(0)
})


test('a new decoder can reconcile on the regular location page without a frontend adapter', async ({ page }) => {
  const state = await openScenario(page, 0, true, undefined, 'FutureRadar')
  await expect(page.getByLabel('Decoder')).toContainText('Future Radar')
  await page.getByRole('button', { name: 'Add Channel 65 zone NB-18 Right', exact: true }).click()
  await page.getByRole('button', { name: 'Apply selected to editor' }).click()
  await expect(page.getByText(/Changes staged/)).toBeVisible()
  expect(state.reports[0].decoder).toBe('FutureRadar')
  expect(state.writes).toHaveLength(0)
})


test('unused zone permits manual detector addition despite low counts and source warnings', async ({ page }) => {
  const state = await openScenario(page, 7, true, undefined, 'VisionCameraAPI', true)
  await expect(page.getByText(/overlapping movement assignments/)).toBeVisible()
  const warning = page.getByRole('alert').filter({ hasText: 'Unused zone SB-36' })
  await warning.getByRole('button', { name: 'Add Detector', exact: true }).click()
  const editor = page.getByRole('dialog', { name: 'Add detector for zone SB-36' })
  await expect(editor.getByRole('button', { name: 'Add detector to editor' })).toBeDisabled()
  await editor.getByLabel('Approach', { exact: true }).click()
  await page.getByRole('option').first().click()
  const row = editor.getByRole('table', { name: 'detector table' }).getByRole('row').last()
  await expect(row.locator('td').nth(0)).toHaveText('SB-36')
  await expect(row.locator('td').nth(1)).toHaveText('65')
  await row.locator('td').nth(1).dblclick()
  await row.getByRole('textbox').first().fill('118')
  await row.getByRole('textbox').first().press('Enter')
  await expect(editor.getByRole('button', { name: 'Add detector to editor' })).toBeDisabled()
  await row.locator('td').nth(1).dblclick()
  await row.getByRole('textbox').first().fill('65')
  await row.getByRole('textbox').first().press('Enter')
  await row.locator('td').nth(6).click()
  await page.getByRole('option', { name: 'Thru', exact: true }).click()
  await editor.getByRole('button', { name: 'Add detector to editor' }).click()
  expect(state.writes).toHaveLength(0)
  await page.getByRole('button', { name: 'Close', exact: true }).click()
  await page.getByRole('button', { name: 'save approach', exact: true }).click()
  await expect.poll(() => state.writes.length).toBe(1)
  expect(state.writes[0].detectors.find((d: any) => d.detectorChannel === 65)).toMatchObject({ dectectorIdentifier: 'SB-36', movementType: 1 })
  expect(state.unexpected).toEqual([])
})

test('cancel unused-zone detector leaves configuration unchanged', async ({ page }) => {
  const state = await openScenario(page, 0, true, undefined, 'VisionCameraAPI', true)
  await page.getByRole('alert').filter({ hasText: 'Unused zone SB-36' }).getByRole('button', { name: 'Add Detector', exact: true }).click()
  await page.getByRole('dialog', { name: 'Add detector for zone SB-36' }).getByRole('button', { name: 'Cancel' }).click()
  await expect(page.getByRole('alert').filter({ hasText: 'Unused zone SB-36' })).toBeVisible()
  expect(state.writes).toHaveLength(0)
  expect(state.unexpected).toEqual([])
})


for (const kind of ['overlap', 'missing zone', 'missing lane number', 'unknown movement', 'unknown lane type'] as const) {
  test(`manual detector edit: ${kind}`, async ({ page }) => {
    const state = await openScenario(page, kind === 'overlap' ? 7 : 0, true, undefined, 'VisionCameraAPI', false, (fixture) => {
      const detector = fixture.approaches[0].detectors[0]
      if (kind === 'missing zone') detector.dectectorIdentifier = 'Old zone'
      if (kind === 'missing lane number') { detector.laneNumber = null as any; detector.movementType = 'T' as any }
      if (kind === 'unknown movement') detector.movementType = 'NA' as any
      if (kind === 'unknown lane type') detector.laneType = 'NA' as any
    })
    await page.getByRole('button', { name: 'Edit Channel 118', exact: true }).click()
    const editor = page.getByRole('dialog', { name: 'Edit detector Channel 118' })
    const row = editor.getByRole('table', { name: 'detector table' }).getByRole('row').last()
    await row.locator('td').nth(0).dblclick()
    await row.getByRole('textbox').first().fill('Corrected zone')
    await row.getByRole('textbox').first().press('Enter')
    await editor.getByRole('button', { name: 'Apply detector changes' }).click()
    expect(state.writes).toHaveLength(0)
    await page.getByRole('button', { name: 'Close', exact: true }).click()
    await page.getByRole('button', { name: 'save approach', exact: true }).click()
    await expect.poll(() => state.writes.length).toBe(1)
    expect(state.writes[0].detectors.find((d: any) => d.detectorChannel === 118).dectectorIdentifier).toBe('Corrected zone')
    expect(state.deletions).toEqual([])
    expect(state.unexpected).toEqual([])
  })
}

for (const kind of ['overlap', 'missing zone'] as const) {
  test(`manual detector removal: ${kind}`, async ({ page }) => {
    const state = await openScenario(page, kind === 'overlap' ? 7 : 0, true, undefined, 'VisionCameraAPI', false, (fixture) => {
      if (kind === 'missing zone') fixture.approaches[0].detectors[0].dectectorIdentifier = 'Old zone'
    })
    await page.getByRole('button', { name: 'Edit Channel 118', exact: true }).click()
    await page.getByRole('button', { name: 'Remove Channel 118 from draft' }).click()
    expect(state.deletions).toEqual([])
    expect(state.writes).toEqual([])
    await page.getByRole('button', { name: 'Close', exact: true }).click()
    await page.getByRole('button', { name: 'save approach', exact: true }).click()
    await expect.poll(() => state.writes.length).toBe(1)
    expect(state.writes[0].detectors.some((d: any) => d.detectorChannel === 118)).toBe(false)
    expect(state.unexpected).toEqual([])
  })
}

for (const kind of ['low volume', 'multiple lanes'] as const) {
  test(`manual movement addition: ${kind}`, async ({ page }) => {
    const state = await openScenario(page, 0, true, undefined, 'VisionCameraAPI', false, (fixture) => {
      fixture.approaches[0].detectors[0].movementType = 'T' as any
      if (kind === 'low volume') fixture.evidence[0].right = 1
      else fixture.approaches[0].detectors.push({ ...fixture.approaches[0].detectors[0], id: 999, detectorChannel: 119, laneNumber: 2, movementType: 'L' as any })
    })
    await page.getByRole('button', { name: 'Add Detector', exact: true }).click()
    const editor = page.getByRole('dialog', { name: 'Add detector for zone NB-18' })
    await editor.getByLabel('Approach', { exact: true }).click()
    await page.getByRole('option').first().click()
    await editor.getByRole('button', { name: 'Add detector to editor' }).click()
    await page.getByRole('button', { name: 'Close', exact: true }).click()
    await page.getByRole('button', { name: 'save approach', exact: true }).click()
    await expect.poll(() => state.writes.length).toBe(1)
    expect(state.writes[0].detectors.find((d: any) => d.detectorChannel === 65).dectectorIdentifier).toBe('NB-18')
    expect(state.unexpected).toEqual([])
  })
}

test('cancel manual detector edit preserves existing configuration', async ({ page }) => {
  const state = await openScenario(page, 7)
  await page.getByRole('button', { name: 'Edit Channel 118', exact: true }).click()
  await page.getByRole('dialog', { name: 'Edit detector Channel 118' }).getByRole('button', { name: 'Cancel' }).click()
  await expect(page.getByText(/overlapping movement assignments/)).toBeVisible()
  expect(state.writes).toEqual([])
  expect(state.deletions).toEqual([])
})
