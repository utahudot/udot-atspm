import type {
  ConfigApproach,
  ConfigDetector,
} from '@/features/locations/components/editLocation/locationStore'
import type { ZoneEvidence, ZoneInventory } from '@/features/locations/components/ReconcileLanes/reconcileLanes'

type MockScenario = {
  name: string
  movements: string[]
  counts: number[]
  configuredZones?: string[]
  separateLanes?: boolean
  zones?: { name: string; counts: number[] }[]
  explanation: string
}
export const mockScenarios: MockScenario[] = [
  {
    name: 'Split Through/Right',
    movements: ['TR'],
    counts: [120, 0, 30],
    explanation:
      'Keep the existing channel for Through and add Right on a new channel, using the same zone and physical lane.',
  },
  {
    name: 'Split Through/Left',
    movements: ['TL'],
    counts: [120, 25, 0],
    explanation:
      'Keep the existing channel for Through and add Left on a new channel, using the same zone and physical lane.',
  },
  {
    name: 'Left only, missing Through',
    movements: ['L'],
    counts: [120, 25, 0],
    explanation:
      'Keep Left and offer a new Through entry sharing its zone and physical lane.',
  },
  {
    name: 'Through only, both turns missing',
    movements: ['T'],
    counts: [120, 25, 30],
    explanation:
      'Offer separate Left and Right entries with distinct suggested channels.',
  },
  {
    name: 'Separate Left/Right, missing Through',
    movements: ['L', 'R'],
    counts: [120, 25, 30],
    explanation:
      'Offer Through once; keep the existing Left and Right entries separate.',
  },
  {
    name: 'Zero-count turn removal',
    movements: ['T', 'R'],
    counts: [120, 0, 0],
    explanation:
      'Offer optional removal of Right. Zero counts do not prove that the turn is unavailable.',
  },
  {
    name: 'Already matching',
    movements: ['T', 'L', 'R'],
    counts: [120, 25, 30],
    explanation: 'No additions or removals should be needed.',
  },
  {
    name: 'Overlapping TL/TR on different physical lanes',
    movements: ['TL', 'TR'],
    counts: [120, 25, 30],
    separateLanes: true,
    explanation:
      'Both entries claim Through. Warn and preserve both physical lanes instead of duplicating zone totals.',
  },
  {
    name: 'Low-volume turn',
    movements: ['T'],
    counts: [120, 0, 2],
    explanation:
      'Below the minimum count of 25: warn rather than recommend adding Right.',
  },
  {
    name: 'Multiple Through lanes, different zones',
    movements: ['T', 'T'],
    counts: [120, 0, 0],
    separateLanes: true,
    zones: [
      { name: 'NB-18', counts: [120, 0, 0] },
      { name: 'NB-19', counts: [85, 0, 0] },
    ],
    explanation:
      'Channels 118 and 119 are separate Through lanes on camera zones NB-18 and NB-19. Keep both lanes and their counts separate; no overlap warnings or changes are expected.',
  },
  {
    name: 'Separate Through/Right zones, shared camera zone',
    movements: ['T', 'R'],
    counts: [120, 0, 30],
    configuredZones: ['NB-18', 'NB-19'],
    explanation:
      'Channel 118 (Through) uses NB-18 and Channel 119 (Right) uses NB-19 on the same physical lane. The camera only has NB-18, reporting 120 Through and 30 Right. Select Change Channel 119 zone NB-19 to NB-18 to reuse the existing Right detector. Its channel and settings remain intact, and no additional Right detector is created.',
  },
]

export function loadMockScenario(index: number) {
  const scenario = mockScenarios[index]
  const zones = scenario.zones ?? [{ name: 'NB-18', counts: scenario.counts }]
  const approaches = [
    {
      id: 1,
      description: 'Mock Northbound',
      directionTypeId: 'NB',
      protectedPhaseNumber: 2,
      detectors: scenario.movements.map(
        (movementType, i) =>
          ({
            id: i + 1,
            approachId: 1,
            detectorChannel: 118 + i,
            dectectorIdentifier: scenario.configuredZones?.[i] ?? zones[scenario.zones ? i : 0].name,
            laneNumber: scenario.separateLanes ? i + 1 : 1,
            movementType,
            laneType: 'V',
            detectionTypes: [{ id: 4 }],
          }) as unknown as ConfigDetector
      ),
    },
  ] as unknown as ConfigApproach[]
  const inventory: ZoneInventory[] = [
    { deviceId: 1, names: zones.map((zone) => zone.name) },
  ]
  const evidence: ZoneEvidence[] = zones.map((zone, i) => ({
    deviceId: 1,
    zoneId: 100 + i,
    zoneName: zone.name,
    through: zone.counts[0],
    left: zone.counts[1],
    right: zone.counts[2],
    bins: 96,
  }))
  return { approaches, inventory, evidence }
}
