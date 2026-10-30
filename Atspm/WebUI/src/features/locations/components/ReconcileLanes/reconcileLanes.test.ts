import type {
  ConfigApproach,
  ConfigDetector,
} from '../editLocation/locationStore'
import {
  applyLaneRecommendations,
  originalLaneChange,
  reconcileLanes,
  suggestLaneChannels,
  ZoneEvidence,
} from './reconcileLanes'

const detector = (id = 1, overrides = {}) =>
  ({
    id,
    detectorChannel: 300 + id,
    dectectorIdentifier: 'Arbitrary zone',
    laneNumber: 1,
    laneType: 'V',
    movementType: 'L',
    detectionTypes: [{ id: 4 }],
    ...overrides,
  }) as unknown as ConfigDetector
const approach = (detectors = [detector()]) =>
  [
    { id: 10, protectedPhaseNumber: 1, directionTypeId: 'NB', detectors },
  ] as unknown as ConfigApproach[]
const inventory = [{ deviceId: 7, names: ['Arbitrary zone'] }]
const evidence: ZoneEvidence[] = [
  {
    deviceId: 7,
    zoneName: 'Arbitrary zone',
    through: 80,
    left: 30,
    right: 0,
    bins: 96,
  },
]
beforeAll(() => {
  global.structuredClone = (value) => JSON.parse(JSON.stringify(value))
})

it('uses configured lanes and arbitrary names to recommend separate left and through entries', () => {
  const current = approach()
  const review = reconcileLanes(current, inventory, evidence)
  expect(review.recommendations[0].movements).toEqual(['L', 'T'])
  const next = applyLaneRecommendations(current, review.recommendations, {
    'movements-1': [302],
  })
  expect(next[0].detectors.map((d) => d.movementType)).toEqual(['L', 'T'])
  expect(next[0].detectors.map((d) => d.dectectorIdentifier)).toEqual([
    'Arbitrary zone',
    'Arbitrary zone',
  ])
  expect(next[0].detectors[1]).toMatchObject({
    isNew: true,
    laneNumber: 1,
    approachId: 10,
    detectionTypes: [{ id: 4 }],
  })
  expect(current[0].detectors).toHaveLength(1)
})

it('never matches null or whitespace zone names to unassigned lanes', () => {
  const review = reconcileLanes(
    approach([detector(1, { dectectorIdentifier: '' })]),
    [{ deviceId: 7, names: [''] }],
    [null, ' '].map((zoneName, i) => ({
      ...evidence[0],
      zoneName,
      zoneId: 101 + i,
    }))
  )
  expect(review.recommendations).toEqual([])
  expect(review.warnings).toContain(
    'Unnamed zone 101 on device 7: configure a zone name before matching lanes.'
  )
  expect(review.warnings).toContain(
    'Unnamed zone 102 on device 7: configure a zone name before matching lanes.'
  )
  expect(review.warnings.join(' ')).not.toContain('null')
})

it('excludes controller channels including 64 and does not call their zones unused', () => {
  const current = approach([
    detector(1, { detectorChannel: 1 }),
    detector(2, { detectorChannel: 64 }),
  ])
  expect(reconcileLanes(current, inventory, evidence)).toEqual({
    recommendations: [],
    warnings: [],
    unusedZones: [],
    manualActions: [],
  })
})

it('offers removal for zero counts but only removes explicitly selected entries', () => {
  const current = approach()
  const result = reconcileLanes(current, inventory, [
    { ...evidence[0], through: 0, left: 0 },
  ])
  expect(result.recommendations).toHaveLength(1)
  expect(result.recommendations[0].remove).toBe(true)
  expect(applyLaneRecommendations(current, [], {})[0].detectors).toHaveLength(1)
  expect(
    applyLaneRecommendations(current, result.recommendations, {})[0].detectors
  ).toHaveLength(0)
  expect(current[0].detectors).toHaveLength(1)
  expect(result.warnings.join(' ')).toContain('Zero counts')
})

it('splits combined movements without losing an unobserved configured movement', () => {
  const current = approach([detector(1, { movementType: 'TR' })])
  const result = reconcileLanes(current, inventory, [
    { ...evidence[0], left: 0 },
  ])
  expect(
    result.recommendations.find((r) => r.id === 'movements-1')?.movements
  ).toEqual(['T', 'R'])
  const reduce = result.recommendations.find((r) => r.id === 'remove-1')!
  expect(
    applyLaneRecommendations(current, [reduce], {})[0].detectors[0].movementType
  ).toBe('T')
  expect(() =>
    applyLaneRecommendations(current, result.recommendations, {})
  ).toThrow('only one change')
  expect(result.warnings.join(' ')).toContain('Zero counts')
})

it('offers optional removal when a known zone has no returned bins', () => {
  const current = approach()
  const result = reconcileLanes(current, inventory, [])
  expect(result.recommendations[0].remove).toBe(true)
  expect(result.warnings.join(' ')).toContain('no unambiguous bin evidence')
  expect(
    applyLaneRecommendations(current, result.recommendations, {})[0].detectors
  ).toEqual([])
})

it('warns about overlapping assignments without adding more counts', () => {
  const current = approach([detector(1, { movementType: 'TL' }), detector(2)])
  const result = reconcileLanes(current, inventory, evidence)
  expect(result.recommendations).toHaveLength(0)
  expect(result.warnings.join(' ')).toContain('double-count')
})

it('warns about unused zones and missing configured zones', () => {
  const current = approach()
  const result = reconcileLanes(
    current,
    [{ deviceId: 7, names: ['Unassigned'] }],
    []
  )
  expect(result.warnings.join(' ')).toContain('not returned')
  expect(result.warnings.join(' ')).toContain('Unused zone Unassigned')
})

it('does not guess a replacement for a missing zone', () => {
  const current = approach([detector(1, { dectectorIdentifier: 'Mistyped' })])
  const result = reconcileLanes(current, inventory, evidence)
  expect(result.recommendations).toHaveLength(0)
  expect(result.warnings.join(' ')).toContain('not returned')
})

it('does not match identical names across cameras', () => {
  const current = approach()
  const result = reconcileLanes(
    current,
    [...inventory, { ...inventory[0], deviceId: 8 }],
    evidence
  )
  expect(result.recommendations).toHaveLength(0)
  expect(result.warnings.join(' ')).toContain('multiple devices')
})

it('suppresses edits when another device inventory cannot be read', () => {
  const current = approach()
  const result = reconcileLanes(
    current,
    [...inventory, { deviceId: 8, names: [], error: 'offline' }],
    evidence
  )
  expect(result.recommendations).toHaveLength(0)
  expect(result.warnings.join(' ')).toContain('inventory unavailable')
})

it('requires a configured physical lane number before recommending an added movement', () => {
  const result = reconcileLanes(
    approach([detector(1, { laneNumber: null })]),
    inventory,
    evidence
  )
  expect(result.recommendations).toHaveLength(0)
  expect(result.warnings.join(' ')).toContain('lane number needs verification')
})

it('does not recommend a new movement for a few misclassified vehicles', () => {
  const current = approach()
  const result = reconcileLanes(current, inventory, [
    { ...evidence[0], through: 1 },
  ])
  expect(result.recommendations).toHaveLength(0)
  expect(result.warnings.join(' ')).toContain('low-volume')
})

it.each([1, 64, 301, 302.5, NaN])(
  'rejects invalid or occupied new channel %s atomically',
  (channel) => {
    const current = approach()
    const result = reconcileLanes(current, inventory, evidence)
    expect(() =>
      applyLaneRecommendations(current, result.recommendations, {
        'movements-1': [channel],
      })
    ).toThrow()
    expect(current[0].detectors).toHaveLength(1)
  }
)

it('adds a missing through movement once for entries on the same physical lane', () => {
  const current = approach([detector(1, { movementType: 'R' }), detector(2)])
  const result = reconcileLanes(current, inventory, evidence)
  const additions = result.recommendations.filter((r) => !r.warning)
  expect(additions).toHaveLength(1)
  expect(additions[0].movements).toEqual(['R', 'T'])
  const next = applyLaneRecommendations(current, additions, {
    'movements-1': [303],
  })
  expect(next[0].detectors.map((d) => d.movementType).sort()).toEqual([
    'L',
    'R',
    'T',
  ])
  expect(current[0].detectors).toHaveLength(2)
})
it('keeps missing movement assignments manual when physical lanes differ', () => {
  const result = reconcileLanes(
    approach([
      detector(1, { movementType: 'R' }),
      detector(2, { laneNumber: 2 }),
    ]),
    inventory,
    evidence
  )
  expect(result.recommendations.filter((r) => !r.warning)).toHaveLength(0)
  expect(result.warnings.join(' ')).toContain('verify which lane')
})

it('suggests one distinct unused channel per added movement across proposals', () => {
  const current = approach([
    detector(1, { detectorChannel: 65 }),
    detector(2, { detectorChannel: 67, dateDisabled: '2026-01-01' }),
  ])
  expect(
    suggestLaneChannels(current, [
      { id: 'a', detectorId: 1, description: '', movements: ['L', 'T', 'R'] },
      { id: 'b', detectorId: 2, description: '', movements: ['T', 'R'] },
    ])
  ).toEqual({ a: ['66', '68'], b: ['69'] })
})

describe('mock lane configuration scenarios', () => {
  it.each([
    ['through-only matches', ['T'], 30, 0, 0, 0, 0],
    ['left-only matches', ['L'], 0, 30, 0, 0, 0],
    ['right-only matches', ['R'], 0, 0, 30, 0, 0],
    ['through needs left', ['T'], 30, 30, 0, 1, 0],
    ['through needs right', ['T'], 30, 0, 30, 1, 0],
    ['through needs both turns', ['T'], 30, 30, 30, 2, 0],
    ['left and right need through', ['L', 'R'], 30, 30, 30, 1, 0],
    ['all movements match', ['L', 'T', 'R'], 30, 30, 30, 0, 0],
    ['left has only through observations', ['L'], 30, 0, 0, 1, 1],
    ['all movements zero', ['L', 'T', 'R'], 0, 0, 0, 0, 3],
    ['right below threshold', ['T'], 30, 0, 2, 0, 0],
  ] as const)('%s', (_, configured, through, left, right, adds, removes) => {
    const current = approach(
      configured.map((movementType, i) => detector(i + 1, { movementType }))
    )
    const review = reconcileLanes(current, inventory, [
      { ...evidence[0], through, left, right },
    ])
    expect(review.recommendations.filter((r) => !r.warning)).toHaveLength(adds)
    expect(review.recommendations.filter((r) => r.warning)).toHaveLength(
      removes
    )
    expect(applyLaneRecommendations(current, [], {})).toEqual(current)
  })
})

it.each(['TR', 'TL', 4, 5])(
  'previews the original channel change for combined movement %s',
  (movementType) => {
    const current = approach([detector(1, { movementType })])
    const review = reconcileLanes(current, inventory, [
      { ...evidence[0], right: 30 },
    ])
    const split = review.recommendations.find((r) => !r.warning)!
    expect(originalLaneChange(current, split)).toMatch(
      /Channel 301 zone Arbitrary zone will change from Through\/(Right|Left) to Through\./
    )
    expect(current[0].detectors[0].movementType).toBe(movementType)
  }
)
it('does not claim the original movement changes when only adding a missing movement', () => {
  const current = approach()
  const review = reconcileLanes(current, inventory, evidence)
  expect(originalLaneChange(current, review.recommendations[0])).toBeUndefined()
})

it.each([['L'], ['R'], ['L', 'R']])(
  'independently applies missing turns %s',
  (...turns) => {
    const current = approach([detector(1, { movementType: 'T' })])
    const review = reconcileLanes(current, inventory, [
      { ...evidence[0], right: 30 },
    ])
    expect(review.recommendations).toHaveLength(2)
    const selected = review.recommendations.filter((r) =>
      turns.includes(r.movements![1])
    )
    const suggested = suggestLaneChannels(current, review.recommendations)
    const next = applyLaneRecommendations(
      current,
      selected,
      Object.fromEntries(
        Object.entries(suggested).map(([id, values]) => [
          id,
          values.map(Number),
        ])
      )
    )
    expect(next[0].detectors.map((d) => d.movementType)).toEqual([
      'T',
      ...turns,
    ])
    expect(
      next[0].detectors.every(
        (d) => d.laneNumber === 1 && d.dectectorIdentifier === 'Arbitrary zone'
      )
    ).toBe(true)
    expect(current[0].detectors).toHaveLength(1)
  }
)

it.each([24, 25])('defaults to 25 observed counts: %s', (right) => {
  const review = reconcileLanes(
    approach([detector(1, { movementType: 'T' })]),
    inventory,
    [{ ...evidence[0], left: 0, right }]
  )
  expect(review.recommendations.filter((r) => !r.warning)).toHaveLength(
    right >= 25 ? 1 : 0
  )
})

it('reassigns an existing Right detector without adding or changing its other settings', () => {
  const current = approach([
    detector(1, { movementType: 'T' }),
    detector(2, {
      movementType: 'R',
      dectectorIdentifier: 'Old right zone',
      latencyCorrection: 0.5,
    }),
  ])
  const review = reconcileLanes(current, inventory, [
    { ...evidence[0], left: 0, right: 30 },
  ])
  expect(review.recommendations).toHaveLength(1)
  expect(review.recommendations[0].zoneChange).toEqual({
    from: 'Old right zone',
    to: 'Arbitrary zone',
  })
  const next = applyLaneRecommendations(current, review.recommendations, {})
  expect(next[0].detectors).toEqual([
    current[0].detectors[0],
    { ...current[0].detectors[1], dectectorIdentifier: 'Arbitrary zone' },
  ])
  expect(
    reconcileLanes(next, inventory, [{ ...evidence[0], left: 0, right: 30 }])
      .recommendations
  ).toHaveLength(0)
  current[0].detectors[1].dectectorIdentifier = 'Changed meanwhile'
  expect(() =>
    applyLaneRecommendations(current, review.recommendations, {})
  ).toThrow('Configuration changed')
})
it('does not guess between multiple existing Right detectors', () => {
  const current = approach([
    detector(1, { movementType: 'T' }),
    detector(2, { movementType: 'R', dectectorIdentifier: 'Old A' }),
    detector(3, { movementType: 'R', dectectorIdentifier: 'Old B' }),
  ])
  const review = reconcileLanes(current, inventory, [
    { ...evidence[0], left: 0, right: 30 },
  ])
  expect(review.recommendations).toHaveLength(0)
  expect(review.warnings.join(' ')).toContain(
    'several existing Right detectors'
  )
})
it('does not reassign Right from another physical lane or a valid camera zone', () => {
  for (const overrides of [
    { laneNumber: 2, dectectorIdentifier: 'Old right' },
    { dectectorIdentifier: 'Other camera zone' },
  ]) {
    const current = approach([
      detector(1, { movementType: 'T' }),
      detector(2, { movementType: 'R', ...overrides }),
    ])
    const review = reconcileLanes(
      current,
      [{ deviceId: 7, names: ['Arbitrary zone', 'Other camera zone'] }],
      [{ ...evidence[0], left: 0, right: 30 }]
    )
    expect(review.recommendations.some((r) => r.zoneChange)).toBe(false)
  }
})

it.each([
  ['right', { through: 80, left: 30, right: 0 }, 'TL'],
  ['left', { through: 80, left: 0, right: 30 }, 'TR'],
])(
  'removes only %s from LTR without creating another detector',
  (_, counts, remaining) => {
    const current = approach([detector(1, { movementType: 'LTR' })])
    const review = reconcileLanes(current, inventory, [
      { ...evidence[0], ...counts },
    ])
    const removal = review.recommendations.find((r) => r.id === 'remove-1')!
    expect(suggestLaneChannels(current, [removal])).toEqual({ 'remove-1': [] })
    const next = applyLaneRecommendations(current, [removal], {})
    expect(next[0].detectors).toEqual([
      { ...current[0].detectors[0], movementType: remaining },
    ])
  }
)

it('treats a zero-bin row like missing evidence', () => {
  expect(
    reconcileLanes(approach(), inventory, [{ ...evidence[0], bins: 0 }])
  ).toEqual(reconcileLanes(approach(), inventory, []))
})

it('does not offer an invalid LR entry when only Through is absent from LTR', () => {
  const current = approach([detector(1, { movementType: 'LTR' })])
  const review = reconcileLanes(current, inventory, [
    { ...evidence[0], through: 0, right: 30 },
  ])
  expect(review.recommendations.some((r) => r.id === 'remove-1')).toBe(false)
  expect(review.warnings.join(' ')).toContain(
    'cannot be represented by one entry'
  )
})

it('does not retarget an absent turn zone when another decoder may own it', () => {
  const current = approach([
    detector(1, { movementType: 'T' }),
    detector(2, {
      movementType: 'R',
      dectectorIdentifier: 'Other decoder zone',
    }),
  ])
  const review = reconcileLanes(
    current,
    inventory,
    [{ ...evidence[0], left: 0, right: 30 }],
    25,
    false
  )
  expect(review.recommendations).toHaveLength(0)
  expect(review.warnings.join(' ')).toContain(
    'another configured decoder may own'
  )
})

it('exposes unused inventory zones for manual configuration without sufficient counts', () => {
  const review = reconcileLanes(approach(), [{ deviceId: 7, names: ['Arbitrary zone', 'Unused', '  '] }], [], 25)
  expect(review.unusedZones).toEqual([{ name: 'Unused', deviceIds: [7], warning: expect.stringContaining('Unused zone Unused') }])
  expect(review.warnings).toContain(review.unusedZones[0].warning)
})

it('does not offer an already assigned controller zone as unused', () => {
  const review = reconcileLanes(approach([detector(1, { detectorChannel: 1 })]), inventory, [], 25)
  expect(review.unusedZones).toEqual([])
})


it('offers explicit edit/removal for overlaps and missing configured zones', () => {
  const overlapping = reconcileLanes(approach([detector(1), detector(2)]), inventory, evidence)
  expect(overlapping.manualActions[0]).toMatchObject({ detectorIds: [1, 2], remove: true })
  const missing = reconcileLanes(approach(), [{ deviceId: 7, names: ['Other'] }], [])
  expect(missing.manualActions[0]).toMatchObject({ detectorIds: [1], remove: true })
})

it('offers manual addition below threshold and for ambiguous physical lanes', () => {
  const low = reconcileLanes(approach(), inventory, [{ ...evidence[0], through: 1 }])
  expect(low.manualActions).toContainEqual(expect.objectContaining({ add: true, zone: 'Arbitrary zone', detectorIds: [] }))
  const multiple = reconcileLanes(approach([detector(1), detector(2, { movementType: 'R', laneNumber: 2 })]), inventory, evidence)
  expect(multiple.manualActions).toContainEqual(expect.objectContaining({ add: true, detectorIds: [] }))
})

it.each([{ laneNumber: null }, { movementType: 'NA' }, { laneType: 'NA' }])('offers editing for incomplete lane configuration %j', (fields) => {
  const review = reconcileLanes(approach([detector(1, fields)]), inventory, evidence)
  expect(review.manualActions).toContainEqual(expect.objectContaining({ detectorIds: [1] }))
})

it('does not introduce a manual action for removing Through from LTR', () => {
  const review = reconcileLanes(approach([detector(1, { movementType: 'LTR' })]), inventory, [{ ...evidence[0], through: 0, right: 30 }])
  expect(review.manualActions).toEqual([])
})
