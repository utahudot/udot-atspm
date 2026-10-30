import '@testing-library/jest-dom'
import {
  act,
  fireEvent,
  render,
  renderHook,
  screen,
  within,
} from '@testing-library/react'
import { useLocationStore } from '../editLocation/locationStore'
import ApproachesReconcilationReport from './ApproachesReconcilationReport'
import useDiscrepancyStatuses from './useDiscrepancyStatuses'
import { configRequest } from '@/lib/axios'
import { hasUniqueDetectorChannels } from '../editApproach/utils/checkDetectors'

jest.mock('@/api/config', () => ({
  ...jest.requireActual('@/api/config'),
  useGetDetectionType: () => ({ data: { value: [] } }),
  useGetLocationType: () => ({ data: { value: [] } }),
}))

jest.mock('@/stores/notifications', () => ({
  useNotificationStore: () => ({ addNotification: jest.fn() }),
}))

jest.mock('@/lib/axios', () => ({ configRequest: jest.fn() }))
const categories = {
  foundPhaseNumbers: [3],
  foundOverlapNumbers: [3],
  foundDetectorChannels: [],
  notFoundApproaches: [],
  notFoundDetectorChannels: [],
}
beforeEach(() => {
  jest.clearAllMocks()
  useLocationStore.getState().resetStore()
  useLocationStore.setState({ location: { id: 1 } as any })
})

it('shows manual FYA warnings and stages regular phases and overlaps independently', async () => {
  render(
    <ApproachesReconcilationReport
      categories={{
        ...categories,
        warnings: [
          'FYA observed. Review the left-turn configuration manually.',
        ],
      }}
    />
  )
  expect(screen.getByRole('alert')).toHaveTextContent(
    'Review the left-turn configuration manually'
  )
  const choices = screen.getAllByRole('button', { name: '3' })
  expect(choices).toHaveLength(2)
  fireEvent.click(choices[1])
  const addButtons = screen.getAllByRole('button', { name: /Add Selected/ })
  fireEvent.click(addButtons[1])
  fireEvent.click(screen.getByRole('button', { name: 'Add approaches' }))
  await screen.findByRole('button', { name: '3' })
  expect(useLocationStore.getState().approaches).toEqual([
    expect.objectContaining({
      protectedPhaseNumber: 3,
      isProtectedPhaseOverlap: true,
      isNew: true,
    }),
  ])
  // Adding an overlap does not mark the regular phase of the same number configured.
  fireEvent.click(screen.getByRole('button', { name: '3' }))
  fireEvent.click(screen.getAllByRole('button', { name: /Add Selected/ })[0])
  fireEvent.click(screen.getByRole('button', { name: 'Add approaches' }))
  expect(useLocationStore.getState().approaches).toHaveLength(2)
  expect(useLocationStore.getState().approaches[0]).toMatchObject({
    protectedPhaseNumber: 3,
    isProtectedPhaseOverlap: false,
  })
})

it('does not show a warning when none was returned', () => {
  render(<ApproachesReconcilationReport categories={categories} />)
  expect(screen.queryByRole('alert')).not.toBeInTheDocument()
})

it('matches status by the individual protected/permissive overlap flags', () => {
  const approaches = [
    {
      id: 1,
      protectedPhaseNumber: 3,
      isProtectedPhaseOverlap: false,
      permissivePhaseNumber: 4,
      isPermissivePhaseOverlap: true,
      detectors: [],
    },
  ]
  const report = {
    ...categories,
    foundPhaseNumbers: [3, 4],
    foundOverlapNumbers: [3, 4],
  }
  const { result } = renderHook(() =>
    useDiscrepancyStatuses(report, approaches)
  )
  expect(result.current.itemStatuses).toMatchObject({
    'FOUND_PHASE:3': 'added',
    'FOUND_PHASE:4': 'pending',
    'FOUND_OVERLAP:3': 'pending',
    'FOUND_OVERLAP:4': 'added',
  })
})

it('keeps edited detector properties while assigning a new identity and approach', () => {
  useLocationStore.getState().addApproach(2)
  const approach = useLocationStore.getState().approaches[0]
  useLocationStore.getState().addDetector(approach.id, 119, {
    id: -1,
    approachId: -2,
    isNew: false,
    dectectorIdentifier: 'NB-18',
    laneNumber: 2,
    latencyCorrection: 0.5,
    distanceFromStopBar: 100,
  })
  const detector = useLocationStore.getState().approaches[0].detectors[0]
  expect(detector).toMatchObject({
    approachId: approach.id,
    detectorChannel: 119,
    isNew: true,
    dectectorIdentifier: 'NB-18',
    laneNumber: 2,
    latencyCorrection: 0.5,
    distanceFromStopBar: 100,
  })
  expect(detector.id).not.toBe(-1)
})

it('preserves approach draft properties without copying draft identity or detectors', () => {
  useLocationStore.getState().addApproach(3, true, {
    id: -1,
    locationId: -2,
    isNew: false,
    description: 'Northbound left',
    protectedPhaseNumber: 3,
    isProtectedPhaseOverlap: true,
    pedestrianPhaseNumber: 7,
    mph: 35,
    detectors: [{ id: -4 } as any],
  })
  const approach = useLocationStore.getState().approaches[0]
  expect(approach).toMatchObject({
    locationId: 1,
    isNew: true,
    detectors: [],
    description: 'Northbound left',
    protectedPhaseNumber: 3,
    isProtectedPhaseOverlap: true,
    pedestrianPhaseNumber: 7,
    mph: 35,
  })
  expect(approach.id).not.toBe(-1)
})

const mixedApproach = () => ({
  id: 11,
  description: 'Mixed approach',
  protectedPhaseNumber: 3,
  permissivePhaseNumber: 4,
  isPermissivePhaseOverlap: true,
  pedestrianPhaseNumber: 5,
  isPedestrianPhaseOverlap: false,
  detectors: [{ id: 99, detectorChannel: 118, dectectorIdentifier: 'NB-18' }],
})
const missing = [
  {
    approachId: 11,
    role: 'protected' as const,
    phaseNumber: 3,
    isOverlap: false,
  },
  {
    approachId: 11,
    role: 'permissive' as const,
    phaseNumber: 4,
    isOverlap: true,
  },
]
it('blocks bulk removal of all vehicle assignments even when a pedestrian phase remains', () => {
  const approach = mixedApproach()
  useLocationStore.setState({ approaches: [approach] as any })
  render(
    <ApproachesReconcilationReport
      categories={{ ...categories, missingPhaseAssignments: missing }}
    />
  )
  const card = screen.getByRole('region', {
    name: 'Current configuration: Mixed approach',
  })
  expect(
    screen.getAllByRole('region', {
      name: 'Current configuration: Mixed approach',
    })
  ).toHaveLength(1)
  expect(
    within(card).getByRole('table', { name: 'edit approach table' })
  ).toBeVisible()
  expect(card).toHaveTextContent('118')
  expect(card).toHaveTextContent('NB-18')
  expect(
    within(card).getByRole('button', { name: 'Remove protected phase 3' })
  ).toBeVisible()
  expect(
    within(card).getByRole('button', { name: 'Remove permissive overlap 4' })
  ).toBeVisible()
  fireEvent.click(screen.getByRole('button', { name: 'Select All' }))
  expect(screen.getByRole('button', { name: 'Remove selected' })).toBeDisabled()
  expect(useLocationStore.getState().approaches[0]).toEqual(approach)
})
it('only deletes an entire approach through the separate confirmed action', () => {
  useLocationStore.setState({ approaches: [mixedApproach()] as any })
  render(
    <ApproachesReconcilationReport
      categories={{ ...categories, missingPhaseAssignments: missing }}
    />
  )
  fireEvent.click(
    screen.getByRole('button', {
      name: 'Delete entire approach: Mixed approach',
    })
  )
  expect(useLocationStore.getState().approaches).toHaveLength(1)
  expect(screen.getByRole('dialog')).toHaveTextContent('all of its detectors')
  fireEvent.click(
    screen.getByRole('button', { name: 'Delete entire approach' })
  )
  expect(useLocationStore.getState().approaches).toHaveLength(0)
})
it('does not offer removal for a stale assignment whose number changed', () => {
  useLocationStore.setState({
    approaches: [{ ...mixedApproach(), protectedPhaseNumber: 7 }] as any,
  })
  render(
    <ApproachesReconcilationReport
      categories={{ ...categories, missingPhaseAssignments: [missing[0]] }}
    />
  )
  expect(
    screen.queryByRole('button', { name: /protected phase 3/ })
  ).not.toBeInTheDocument()
  expect(screen.getByRole('button', { name: 'Remove selected' })).toBeDisabled()
})
it('legacy whole-approach results never imply permission to clear individual fields', () => {
  useLocationStore.setState({ approaches: [mixedApproach()] as any })
  render(
    <ApproachesReconcilationReport
      categories={{
        ...categories,
        notFoundApproaches: [{ id: 11, description: 'Mixed approach' }],
      }}
    />
  )
  expect(screen.getByRole('button', { name: 'Remove selected' })).toBeDisabled()
  expect(
    screen.getByRole('button', {
      name: 'Delete entire approach: Mixed approach',
    })
  ).toBeVisible()
})

it('removes only the selected overlap, preserving unselected missing fields and other approaches', () => {
  const first = mixedApproach()
  const second = {
    ...mixedApproach(),
    id: 12,
    description: 'Other approach',
    detectors: [],
  }
  useLocationStore.setState({ approaches: [first, second] as any })
  render(
    <ApproachesReconcilationReport
      categories={{ ...categories, missingPhaseAssignments: missing }}
    />
  )
  fireEvent.click(screen.getByRole('button', { name: 'Select All' }))
  fireEvent.click(
    screen.getByRole('button', {
      name: 'Remove permissive overlap 4',
    })
  )
  fireEvent.click(screen.getByRole('button', { name: 'Confirm removal' }))
  expect(useLocationStore.getState().approaches[0]).toMatchObject({
    protectedPhaseNumber: 3,
    permissivePhaseNumber: null,
    pedestrianPhaseNumber: 5,
    detectors: first.detectors,
  })
  expect(useLocationStore.getState().approaches[1]).toEqual(second)
})

it('offers only whole approach deletion for the last vehicle assignment', () => {
  const approach = { ...mixedApproach(), permissivePhaseNumber: null }
  useLocationStore.setState({ approaches: [approach] as any })
  render(
    <ApproachesReconcilationReport
      categories={{ ...categories, missingPhaseAssignments: [missing[0]] }}
    />
  )
  expect(
    screen.queryByRole('button', { name: 'Remove protected phase 3' })
  ).not.toBeInTheDocument()
  expect(
    screen.queryByRole('checkbox', { name: /Select Mixed approach/ })
  ).not.toBeInTheDocument()
  expect(
    screen.getByRole('button', {
      name: 'Delete entire approach: Mixed approach',
    })
  ).toBeVisible()
})
it('sets protected phase to zero while retaining a permissive overlap and detectors', () => {
  const approach = mixedApproach()
  useLocationStore.setState({ approaches: [approach] as any })
  render(
    <ApproachesReconcilationReport
      categories={{ ...categories, missingPhaseAssignments: missing }}
    />
  )
  fireEvent.click(
    screen.getByRole('button', { name: 'Remove protected phase 3' })
  )
  fireEvent.click(screen.getByRole('button', { name: 'Confirm removal' }))
  expect(useLocationStore.getState().approaches[0]).toMatchObject({
    protectedPhaseNumber: 0,
    permissivePhaseNumber: 4,
    detectors: approach.detectors,
  })
  expect(
    screen.queryByRole('button', { name: 'Remove permissive overlap 4' })
  ).not.toBeInTheDocument()
})

it('removes overlap 3 without removing regular phase 3 on the same approach', () => {
  const approach = { ...mixedApproach(), permissivePhaseNumber: 3 }
  useLocationStore.setState({ approaches: [approach] as any })
  render(
    <ApproachesReconcilationReport
      categories={{
        ...categories,
        missingPhaseAssignments: [{ ...missing[1], phaseNumber: 3 }],
      }}
    />
  )
  fireEvent.click(
    screen.getByRole('button', { name: 'Remove permissive overlap 3' })
  )
  fireEvent.click(screen.getByRole('button', { name: 'Confirm removal' }))
  expect(useLocationStore.getState().approaches[0]).toMatchObject({
    protectedPhaseNumber: 3,
    permissivePhaseNumber: null,
    detectors: approach.detectors,
  })
})


it('mixed removal preserves unsaved assignments and resets only unpersisted edits', async () => {
  ;(configRequest as jest.Mock).mockResolvedValue(undefined)
  const first = mixedApproach()
  const other = { ...mixedApproach(), id: 12, description: 'Other', detectors: [] }
  useLocationStore.getState().setLocation({ id: 1, approaches: [first, other] } as any)
  useLocationStore.getState().updateApproach({ ...other, description: 'Unsaved other' } as any)
  render(<ApproachesReconcilationReport categories={{
    ...categories, foundPhaseNumbers: [], foundOverlapNumbers: [],
    missingPhaseAssignments: [{ approachId: first.id, role: 'protected', phaseNumber: 3, isOverlap: false }],
    notFoundDetectorChannels: ['118'],
  }} />)
  fireEvent.click(screen.getByRole('button', { name: 'Select All' }))
  fireEvent.click(screen.getByRole('button', { name: 'Remove selected' }))
  fireEvent.click(screen.getByRole('button', { name: 'Confirm removal' }))
  await act(async () => {})
  expect(configRequest).toHaveBeenCalledWith(expect.objectContaining({ url: '/Detector/99', method: 'DELETE' }))
  expect(useLocationStore.getState().approaches[0]).toMatchObject({ protectedPhaseNumber: 0, detectors: [] })
  expect(useLocationStore.getState().hasUnsavedChanges()).toBe(true)
  act(() => useLocationStore.getState().resetApproaches())
  expect(useLocationStore.getState().approaches[0]).toMatchObject({ protectedPhaseNumber: 3, detectors: [] })
  expect(useLocationStore.getState().approaches[1].description).toBe('Other')
  expect(useLocationStore.getState().hasUnsavedChanges()).toBe(false)
})

it('reconciliation draft channels participate in subsequent duplicate checks', () => {
  const approach = mixedApproach()
  useLocationStore.getState().setLocation({ id: 1, approaches: [approach] } as any)
  render(<ApproachesReconcilationReport categories={{
    ...categories, foundPhaseNumbers: [], foundOverlapNumbers: [], foundDetectorChannels: [119],
  }} />)
  fireEvent.click(screen.getByRole('button', { name: '119', exact: true }))
  fireEvent.click(screen.getByRole('button', { name: /Add selected to/ }))
  fireEvent.click(screen.getByRole('menuitem', { name: approach.description, exact: true }))
  fireEvent.click(screen.getByRole('button', { name: 'Add detectors', exact: true }))
  const added = useLocationStore.getState().approaches[0].detectors.find(d => d.detectorChannel === 119)!
  expect(added.approachId).toBe(approach.id)
  expect(hasUniqueDetectorChannels(useLocationStore.getState().channelMap).isValid).toBe(true)
  act(() => useLocationStore.getState().updateDetector(99, 'detectorChannel', 119))
  const duplicate = hasUniqueDetectorChannels(useLocationStore.getState().channelMap)
  expect(duplicate.isValid).toBe(false)
  expect(Object.keys(duplicate.errors).sort()).toEqual(['99', String(added.id)].sort())
  act(() => useLocationStore.getState().updateDetector(99, 'detectorChannel', 118))
  act(() => useLocationStore.getState().updateDetector(added.id, 'detectorChannel', 118))
  expect(hasUniqueDetectorChannels(useLocationStore.getState().channelMap).isValid).toBe(false)
  act(() => useLocationStore.getState().resetApproaches())
  expect([...useLocationStore.getState().channelMap.entries()]).toEqual([[99, 118]])
})


it('discarding an added detector removes its validation entry without saving other drafts', () => {
  const approach = mixedApproach()
  useLocationStore.getState().setLocation({ id: 1, approaches: [approach] } as any)
  useLocationStore.getState().updateApproach({ ...approach, protectedPhaseNumber: 0 } as any)
  useLocationStore.getState().addDetector(approach.id, 119, { dectectorIdentifier: 'NB-19', laneNumber: 2 })
  const added = useLocationStore.getState().approaches[0].detectors[0]
  useLocationStore.getState().deleteDetector(added.id)
  expect(configRequest).not.toHaveBeenCalled()
  expect(useLocationStore.getState().channelMap.has(added.id)).toBe(false)
  expect(useLocationStore.getState().hasUnsavedChanges()).toBe(true)
  useLocationStore.getState().resetApproaches()
  expect(useLocationStore.getState().approaches[0]).toEqual(approach)
})
