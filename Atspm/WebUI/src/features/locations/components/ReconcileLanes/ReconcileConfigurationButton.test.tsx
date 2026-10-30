import { getDeviceActiveDevicesByLocationFromLocationId } from '@/api/config/device/device'
import { useFlags } from '@/feature-flags/FeatureFlagContext'
import { useGetDeviceConfigurations } from '@/features/devices/api'
import { configRequest, reportsRequest } from '@/lib/axios'
import '@testing-library/jest-dom'
import { act, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from 'react-query'
import {
  ConfigApproach,
  ConfigLocation,
  useLocationStore,
} from '../editLocation/locationStore'
import ReconcileConfigurationButton from './ReconcileConfigurationButton'

jest.mock('@/feature-flags/FeatureFlagContext', () => ({ useFlags: jest.fn() }))
jest.mock('@/features/devices/api', () => ({
  useGetDeviceConfigurations: jest.fn(),
}))
jest.mock('@/api/config/device/device', () => ({
  getDeviceActiveDevicesByLocationFromLocationId: jest.fn(),
}))
jest.mock('@/lib/axios', () => ({
  configRequest: jest.fn(),
  reportsRequest: jest.fn(),
}))

beforeAll(() => {
  global.structuredClone = (value) => JSON.parse(JSON.stringify(value))
})

async function openReview(
  warnings: string[] = [],
  aliases = false,
  unnamed = false,
  leftCount = 20,
  rightCount = 0
) {
  jest.clearAllMocks()
  ;(useFlags as jest.Mock).mockReturnValue({ laneReconciliation: true })
  ;(useGetDeviceConfigurations as jest.Mock).mockReturnValue({
    data: { value: [] },
  })
  const approaches = [
    {
      id: 10,
      protectedPhaseNumber: 1,
      directionTypeId: 'NB',
      detectors: [
        {
          id: 100,
          detectorChannel: 301,
          dectectorIdentifier: 'Lane Alpha',
          movementType: 'L',
          laneType: 'V',
          laneNumber: 1,
        },
      ],
    },
  ] as unknown as ConfigApproach[]
  useLocationStore.setState({
    location: { id: 1, locationIdentifier: 'Site' } as ConfigLocation,
    approaches,
    savedApproaches: structuredClone(approaches),
  })
  ;(
    getDeviceActiveDevicesByLocationFromLocationId as jest.Mock
  ).mockResolvedValue([
    {
      id: 7,
      deviceStatus: 'Active',
      deviceIdentifier: '123',
      ipaddress: '127.0.0.1',
      deviceConfiguration: { port: 8080 },
      deviceProperties: { TmcDecoder: 'VisionCameraAPI' },
    },
    ...(aliases
      ? [
          {
            id: 8,
            deviceStatus: 'Active',
            deviceIdentifier: '1',
            ipaddress: '127.0.0.1',
            deviceConfiguration: { port: 8080 },
            deviceProperties: { TmcDecoder: 'VisionCameraAPI' },
          },
        ]
      : []),
  ])
  ;(configRequest as jest.Mock).mockResolvedValue(['Lane Alpha', 'Unused'])
  ;(reportsRequest as jest.Mock).mockResolvedValue({
    warnings,
    sourceDeviceIds: [7],
    zoneEvidence: [
      {
        deviceId: 7,
        zoneName: 'Lane Alpha',
        through: 80,
        left: leftCount,
        right: rightCount,
        bins: 96,
      },
      ...(unnamed
        ? [101, 102].map((zoneId) => ({
            deviceId: 7,
            zoneId,
            zoneName: null,
            through: 3,
            left: 0,
            right: 0,
            bins: 1,
          }))
        : []),
    ],
  })
  const client = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  })
  render(
    <QueryClientProvider client={client}>
      <ReconcileConfigurationButton
        onReconcileApproaches={() => Promise.resolve()}
        approachesBusy={false}
        approachReport={<div>Mock approach results</div>}
      />
    </QueryClientProvider>
  )
  fireEvent.click(
    screen.getByRole('button', {
      name: 'Reconcile Approaches',
    })
  )
  expect(screen.getByText('Mock approach results')).toBeVisible()
  expect(getDeviceActiveDevicesByLocationFromLocationId).not.toHaveBeenCalled()
  fireEvent.click(screen.getByRole('tab', { name: 'Lanes' }))
  await waitFor(() =>
    expect(getDeviceActiveDevicesByLocationFromLocationId).toHaveBeenCalled()
  )
  await waitFor(() =>
    expect(screen.getByRole('button', { name: 'Compare lanes' })).toBeEnabled()
  )
  expect(
    screen.queryByLabelText('Reference template location')
  ).not.toBeInTheDocument()
  expect(screen.getByLabelText('Decoder')).toHaveTextContent(
    'Vision Camera API'
  )
  expect(configRequest).not.toHaveBeenCalled()
  fireEvent.click(screen.getByRole('button', { name: 'Compare lanes' }))
  await screen.findByRole('table', { name: 'Observed zone counts' })
  return client
}

it('shows the Config API provider limitation instead of a generic zone failure', async () => {
  await openReview()
  ;(configRequest as jest.Mock).mockRejectedValueOnce({ response: { data: { error: {
    message: 'The built-in provider supports Econolite Vision FIR/AI cameras; other products require a compatible zone provider.',
  } } } })
  fireEvent.click(screen.getByRole('button', { name: 'Compare lanes' }))
  expect(await screen.findByText(/Get Zones failed: The built-in provider supports Econolite Vision/)).toBeVisible()
})

it('reads independent zones and report evidence, separates warnings, and stages selected changes without saving', async () => {
  const client = await openReview()
  expect(configRequest).toHaveBeenCalledWith(
    expect.objectContaining({
      url: '/Detector/retrieveDetectionData',
      method: 'POST',
    })
  )
  expect(reportsRequest).toHaveBeenCalledWith(
    expect.objectContaining({
      data: expect.objectContaining({
        reconcileLanes: true,
        decoder: 'VisionCameraAPI',
        deviceIds: [7],
      }),
    })
  )
  expect(screen.getByText(/Unused zone Unused/)).toBeInTheDocument()
  fireEvent.click(
    screen.getByRole('button', {
      name: /Add Channel .* zone Lane Alpha Through/,
    })
  )
  expect(screen.getByLabelText('Channel for Lane Alpha Through')).toHaveValue(
    65
  )
  fireEvent.change(screen.getByLabelText('Channel for Lane Alpha Through'), {
    target: { value: '302' },
  })
  expect(
    screen.getByRole('button', {
      name: 'Add Channel 302 zone Lane Alpha Through',
    })
  ).toHaveAttribute('aria-pressed', 'true')
  fireEvent.click(
    screen.getByRole('button', { name: 'Apply selected to editor' })
  )
  expect(await screen.findByText(/Changes staged/)).toBeVisible()
  expect(useLocationStore.getState().approaches[0].detectors).toHaveLength(2)
  expect(useLocationStore.getState().savedApproaches[0].detectors).toHaveLength(
    1
  )
  expect(useLocationStore.getState().hasUnsavedChanges()).toBe(true)
  expect(
    (configRequest as jest.Mock).mock.calls.every(
      ([c]) => !['PATCH', 'PUT', 'DELETE'].includes(c.method)
    )
  ).toBe(true)
  client.clear()
})

it('blocks applying stale results after review inputs change', async () => {
  const client = await openReview()
  fireEvent.click(
    screen.getByRole('button', {
      name: /Add Channel .* zone Lane Alpha Through/,
    })
  )
  fireEvent.change(screen.getByLabelText('Minimum observed count'), {
    target: { value: '10' },
  })
  expect(
    screen.getByRole('button', { name: 'Apply selected to editor' })
  ).toBeDisabled()
  expect(useLocationStore.getState().approaches[0].detectors).toHaveLength(1)
  client.clear()
})

it('keeps partial source responses diagnostic-only', async () => {
  const client = await openReview(['Device did not return one day'])
  expect(
    screen.queryByRole('button', { name: /Add Channel/ })
  ).not.toBeInTheDocument()
  expect(screen.getByText(/Resolve them and rerun/)).toBeVisible()
  client.clear()
})

it('lets the user choose removal instead of addition and stages it without saving', async () => {
  const client = await openReview([], false, false, 0)
  const remove = screen.getByRole('button', {
    name: 'Remove Channel 301 zone Lane Alpha Left',
  })
  const add = screen.getByRole('button', {
    name: /Add Channel .* zone Lane Alpha Through/,
  })
  expect(remove.closest('[role="alert"]')).toHaveTextContent('no L observed')
  expect(add.closest('[role="alert"]')).toBeNull()
  expect(remove).toHaveAttribute('aria-pressed', 'false')
  expect(add).toHaveAttribute('aria-pressed', 'false')
  fireEvent.click(add)
  fireEvent.click(remove)
  expect(add).toHaveAttribute('aria-pressed', 'false')
  expect(remove).toHaveAttribute('aria-pressed', 'true')
  fireEvent.click(
    screen.getByRole('button', { name: 'Apply selected to editor' })
  )
  expect(useLocationStore.getState().approaches[0].detectors).toHaveLength(0)
  expect(useLocationStore.getState().savedApproaches[0].detectors).toHaveLength(
    1
  )
  expect(useLocationStore.getState().hasUnsavedChanges()).toBe(true)
  expect(
    (configRequest as jest.Mock).mock.calls.every(
      ([c]) => c.url === '/Detector/retrieveDetectionData'
    )
  ).toBe(true)
  client.clear()
})

it('uses the decoder-selected owner for serial/index aliases and retains recommendations', async () => {
  const client = await openReview([], true)
  expect(reportsRequest).toHaveBeenCalledWith(
    expect.objectContaining({
      data: expect.objectContaining({ deviceIds: [7, 8] }),
    })
  )
  expect(configRequest).toHaveBeenCalledTimes(1)
  expect(configRequest).toHaveBeenCalledWith(
    expect.objectContaining({
      data: expect.objectContaining({ configurationDeviceId: 7 }),
    })
  )
  const recommendation = screen.getByRole('button', {
    name: /Add Channel .* zone Lane Alpha Through/,
  })
  fireEvent.click(recommendation)
  expect(recommendation).toHaveAttribute('aria-pressed', 'true')
  client.clear()
})

it('shows separate unnamed zone IDs without suppressing valid named recommendations', async () => {
  const client = await openReview([], false, true)
  for (const id of [101, 102]) {
    expect(
      screen.getByRole('cell', { name: `Unnamed zone ${id}` })
    ).toBeVisible()
    expect(
      screen.getByText(new RegExp(`Unnamed zone ${id} on device 7`))
    ).toBeVisible()
  }
  const recommendation = screen.getByRole('button', {
    name: /Add Channel .* zone Lane Alpha Through/,
  })
  fireEvent.click(recommendation)
  expect(recommendation).toHaveAttribute('aria-pressed', 'true')
  client.clear()
})

it('invalidates the memoized review when location configuration changes', async () => {
  const client = await openReview()
  fireEvent.click(
    screen.getByRole('button', {
      name: /Add Channel .* zone Lane Alpha Through/,
    })
  )
  act(() =>
    useLocationStore.setState({
      approaches: useLocationStore.getState().approaches.map((a) => ({
        ...a,
        detectors: a.detectors.map((d) => ({ ...d, laneNumber: 2 })),
      })),
    })
  )
  expect(
    screen.getByRole('button', { name: 'Apply selected to editor' })
  ).toBeDisabled()
  client.clear()
})

it('provides an editable channel field for every added movement', async () => {
  const client = await openReview([], false, false, 20, 30)
  const through = screen.getByLabelText('Channel for Lane Alpha Through')
  const right = screen.getByLabelText('Channel for Lane Alpha Right')
  expect(through).toHaveValue(65)
  expect(right).toHaveValue(66)
  fireEvent.change(through, { target: { value: '118' } })
  expect(right).toHaveValue(66)
  expect(
    screen.getByRole('button', {
      name: 'Add Channel 118 zone Lane Alpha Through',
    })
  ).toBeVisible()
  client.clear()
})

it('runs the real lane panel inside combined reconciliation and stages mock additions', async () => {
  const client = await openReview([], false, false, 20, 30)
  fireEvent.click(
    screen.getByRole('button', {
      name: /Add Channel 65 zone Lane Alpha Through/,
    })
  )
  fireEvent.click(
    screen.getByRole('button', { name: 'Add Channel 66 zone Lane Alpha Right' })
  )
  expect(
    screen.getByRole('button', {
      name: 'Add Channel 65 zone Lane Alpha Through',
    })
  ).toHaveAttribute('aria-pressed', 'true')
  fireEvent.click(
    screen.getByRole('button', { name: 'Apply selected to editor' })
  )
  expect(
    useLocationStore
      .getState()
      .approaches[0].detectors.map((d) => d.movementType)
  ).toEqual(['L', 'T', 'R'])
  expect(useLocationStore.getState().savedApproaches[0].detectors).toHaveLength(
    1
  )
  fireEvent.click(screen.getByRole('tab', { name: 'Approaches' }))
  expect(screen.getByText('Mock approach results')).toBeVisible()
  client.clear()
})

it.each([undefined, false, 'true'])(
  'keeps approaches available and lanes unmounted when flag is %s',
  (flag) => {
    jest.clearAllMocks()
    ;(useFlags as jest.Mock).mockReturnValue({ laneReconciliation: flag })
    const reconcile = jest.fn().mockResolvedValue(undefined)
    render(
      <ReconcileConfigurationButton
        onReconcileApproaches={reconcile}
        approachesBusy={false}
        approachReport={null}
      />
    )
    fireEvent.click(
      screen.getByRole('button', { name: 'Reconcile Approaches' })
    )
    expect(reconcile).toHaveBeenCalledTimes(1)
    expect(screen.queryByRole('tab', { name: 'Lanes' })).not.toBeInTheDocument()
    expect(screen.queryByRole('dialog')).not.toBeInTheDocument()
    expect(
      getDeviceActiveDevicesByLocationFromLocationId
    ).not.toHaveBeenCalled()
    expect(reportsRequest).not.toHaveBeenCalled()
  }
)

it('keeps lane review selections and edited channels across tab switches', async () => {
  const client = await openReview()
  const input = screen.getByLabelText('Channel for Lane Alpha Through')
  fireEvent.change(input, { target: { value: '118' } })
  fireEvent.click(
    screen.getByRole('button', {
      name: 'Add Channel 118 zone Lane Alpha Through',
    })
  )
  const calls = (reportsRequest as jest.Mock).mock.calls.length
  fireEvent.click(screen.getByRole('tab', { name: 'Approaches' }))
  expect(screen.getByText('Mock approach results')).toBeVisible()
  fireEvent.click(screen.getByRole('tab', { name: 'Lanes' }))
  expect(screen.getByLabelText('Channel for Lane Alpha Through')).toHaveValue(
    118
  )
  expect(
    screen.getByRole('button', {
      name: 'Add Channel 118 zone Lane Alpha Through',
    })
  ).toHaveAttribute('aria-pressed', 'true')
  expect(reportsRequest).toHaveBeenCalledTimes(calls)
  client.clear()
})

it('blocks Escape, backdrop and Close while lane comparison is pending', async () => {
  const client = await openReview()
  let finish!: (value: unknown) => void
  ;(reportsRequest as jest.Mock).mockImplementationOnce(
    () =>
      new Promise((resolve) => {
        finish = resolve
      })
  )
  fireEvent.click(screen.getByRole('button', { name: 'Compare lanes' }))
  const dialog = screen.getByRole('dialog')
  fireEvent.keyDown(dialog, { key: 'Escape', code: 'Escape', keyCode: 27 })
  fireEvent.click(document.querySelector('.MuiDialog-container')!)
  expect(dialog).toBeVisible()
  expect(
    screen.getByRole('button', { name: 'Close', exact: true })
  ).toBeDisabled()
  fireEvent.click(screen.getByRole('tab', { name: 'Approaches' }))
  expect(
    screen.getByRole('button', { name: 'Close', exact: true })
  ).toBeDisabled()
  await act(async () =>
    finish({ zoneEvidence: [], sourceDeviceIds: [7], warnings: [] })
  )
  await waitFor(() =>
    expect(
      screen.getByRole('button', { name: 'Close', exact: true })
    ).toBeEnabled()
  )
  fireEvent.click(screen.getByRole('button', { name: 'Close', exact: true }))
  await waitFor(() =>
    expect(screen.queryByRole('dialog')).not.toBeInTheDocument()
  )
  client.clear()
})

it.each(['VisionBinStatisticsStored', undefined, ''])(
  'does not repair a detector outside the selected source (TmcDecoder: %s)',
  async (otherDecoder) => {
    const client = await openReview()
    act(() => {
      const original = useLocationStore.getState().approaches[0]
      useLocationStore.setState({
        approaches: [
          {
            ...original,
            detectors: [
              { ...original.detectors[0], movementType: 'T' } as any,
              {
                ...original.detectors[0],
                id: 101,
                detectorChannel: 302,
                movementType: 'R',
                dectectorIdentifier: 'Other source zone',
              } as any,
            ],
          },
        ],
      })
      client.setQueryData(
        ['lane-review-devices', 1],
        [
          {
            id: 7,
            deviceStatus: 'Active',
            deviceIdentifier: '123',
            deviceProperties: { TmcDecoder: 'VisionCameraAPI' },
          },
          {
            id: 8,
            deviceStatus: 'Active',
            deviceIdentifier: '456',
            deviceProperties:
              otherDecoder === undefined ? {} : { TmcDecoder: otherDecoder },
          },
        ]
      )
    })
    if (otherDecoder) {
      fireEvent.mouseDown(screen.getByRole('combobox', { name: 'Decoder' }))
      fireEvent.click(screen.getByRole('option', { name: /Vision Camera API/ }))
    }
    ;(reportsRequest as jest.Mock).mockResolvedValue({
      warnings: [],
      sourceDeviceIds: [7],
      zoneEvidence: [
        {
          deviceId: 7,
          zoneName: 'Lane Alpha',
          through: 80,
          left: 0,
          right: 30,
          bins: 96,
        },
      ],
    })
    fireEvent.click(screen.getByRole('button', { name: 'Compare lanes' }))
    await screen.findByText(/another configured decoder may own/)
    expect(
      screen.queryByRole('button', { name: /Change Channel 302/ })
    ).not.toBeInTheDocument()
    client.clear()
  }
)
