import type {
  ConfigApproach,
  ConfigDetector,
} from '../editLocation/locationStore'

export type ZoneEvidence = {
  deviceId: number
  zoneName: string | null
  zoneId?: number | null
  through: number
  left: number
  right: number
  bins: number
}
export type ZoneInventory = {
  deviceId: number
  names: string[]
  error?: string
}
export type Recommendation = {
  id: string
  detectorId: number
  description: string
  movements?: string[]
  remove?: boolean
  zoneChange?: { from: string; to: string }
  independentAddition?: boolean
  /** Related verification warning; render this optional change alongside it. */
  warning?: string
}
export type LaneReview = {
  recommendations: Recommendation[]
  warnings: string[]
  manualActions: { warning: string; zone: string; detectorIds: number[]; add?: boolean; remove?: boolean }[]
  unusedZones: { name: string; deviceIds: number[]; warning: string }[]
}
const movements: Record<string, string[]> = {
  T: ['T'],
  L: ['L'],
  R: ['R'],
  TL: ['T', 'L'],
  TR: ['T', 'R'],
  LTR: ['L', 'T', 'R'],
}
const movementName = (value: unknown) =>
  typeof value === 'number'
    ? ['NA', 'T', 'R', 'L', 'TR', 'TL', 'NW', 'LTR'][value]
    : String(value)
export const movementLabels: Record<string, string> = {
  T: 'Through',
  L: 'Left',
  R: 'Right',
}
const removalLabel = (d: ConfigDetector, removed: string[]) =>
  `Remove Channel ${d.detectorChannel} zone ${d.dectectorIdentifier} ${removed.map((m) => movementLabels[m] ?? m).join(' + ')}`
const active = (d: ConfigDetector) => !d.dateDisabled
const eligible = (d: ConfigDetector) =>
  active(d) && Number(d.detectorChannel) > 64
const flatten = (approaches: ConfigApproach[]) =>
  approaches.flatMap((a) => a.detectors.map((d) => ({ a, d })))

export const zoneEvidenceLabel = (row: ZoneEvidence) =>
  row.zoneName?.trim()
    ? row.zoneName
    : `Unnamed zone ${row.zoneId ?? '(ID unavailable)'}`

/** The current location already includes its template. Match exact configured zone names without a naming convention. */
export function reconcileLanes(
  approaches: ConfigApproach[],
  inventory: ZoneInventory[],
  evidence: ZoneEvidence[],
  minimum = 25,
  allowZoneRepair = true
): LaneReview {
  const recommendations: Recommendation[] = []
  const warnings: string[] = []
  const unusedZones: LaneReview['unusedZones'] = []
  const manualActions: LaneReview['manualActions'] = []
  const configured = flatten(approaches).filter(({ d }) => eligible(d))
  // Build indexes once rather than rescanning every detector and bin row per lane.
  const groups = new Map<string, typeof configured>()
  for (const entry of configured) {
    const name = entry.d.dectectorIdentifier ?? ''
    const group = groups.get(name) ?? []
    group.push(entry)
    groups.set(name, group)
  }
  const assignments = new Map(
    [...groups].map(([name, group]) => {
      const assigned = group.flatMap(
        ({ d }) => movements[movementName(d.movementType)] ?? []
      )
      return [
        name,
        {
          assigned: new Set(assigned),
          overlap: new Set(assigned).size !== assigned.length,
        },
      ]
    })
  )
  const evidenceByDevice = new Map<number, Map<string, ZoneEvidence[]>>()
  for (const row of evidence) {
    if (!row.zoneName?.trim()) {
      warnings.push(
        `${zoneEvidenceLabel(row)} on device ${row.deviceId}: configure a zone name before matching lanes.`
      )
      continue
    }
    if (row.bins === 0) continue // No observations: use the missing-evidence review.
    const zones =
      evidenceByDevice.get(row.deviceId) ?? new Map<string, ZoneEvidence[]>()
    const rows = zones.get(row.zoneName) ?? []
    rows.push(row)
    zones.set(row.zoneName, rows)
    evidenceByDevice.set(row.deviceId, zones)
  }
  const owners = new Map<string, number[]>()
  for (const camera of inventory) {
    if (camera.error)
      warnings.push(
        `Device ${camera.deviceId}: zone inventory unavailable (${camera.error}). Missing zones cannot be verified.`
      )
    else
      for (const name of new Set(camera.names))
        owners.set(name, [...(owners.get(name) ?? []), camera.deviceId])
  }
  const completeInventory =
    inventory.length > 0 && inventory.every((i) => !i.error)
  for (const { d } of configured) {
    const label = `Channel ${d.detectorChannel}, zone ${d.dectectorIdentifier || '(unassigned)'}`
    const name = d.dectectorIdentifier ?? ''
    const devices = owners.get(name) ?? []
    if (devices.length !== 1) {
      if (devices.length > 1)
        warnings.push(
          `${label}: zone name occurs on multiple devices. The importer matches names; verify uniqueness before applying changes.`
        )
      else if (completeInventory) {
        warnings.push(
          `${label}: not returned by the devices. Verify the assignment or camera configuration; do not delete the lane automatically.`
        )
        manualActions.push({ warning: warnings[warnings.length - 1], zone: name, detectorIds: [d.id], remove: true })
      }
      continue
    }
    if (!completeInventory) continue // Another unreachable device could own the same zone name.
    const rows = evidenceByDevice.get(devices[0])?.get(name) ?? []
    if (rows.length !== 1) {
      warnings.push(
        `${label}: no unambiguous bin evidence in the selected period.`
      )
      if (
        name.trim() &&
        ['V', '1'].includes(String(d.laneType)) &&
        rows.length === 0
      )
        recommendations.push({
          id: `remove-${d.id}`,
          warning: warnings[warnings.length - 1],
          detectorId: d.id,
          remove: true,
          description: removalLabel(
            d,
            movements[movementName(d.movementType)] ?? [
              movementName(d.movementType),
            ]
          ),
        })
      continue
    }
    const counts: Record<string, number> = {
      T: rows[0].through,
      L: rows[0].left,
      R: rows[0].right,
    }
    const group = groups.get(name)!
    const { assigned, overlap } = assignments.get(name)!
    if (overlap) {
      if (group[0].d.id === d.id) {
        warnings.push(
          `Zone ${name}: overlapping movement assignments can double-count vehicles. Review channels ${group.map((r) => r.d.detectorChannel).join(', ')}.`
        )
        manualActions.push({ warning: warnings[warnings.length - 1], zone: name, detectorIds: group.map(({ d }) => d.id), remove: true })
      }
      continue
    }
    const declared = movements[movementName(d.movementType)]
    if (!declared || !['V', '1'].includes(String(d.laneType))) {
      warnings.push(
        `${label}: verify movement/lane type manually (T ${counts.T}, L ${counts.L}, R ${counts.R}).`
      )
      manualActions.push({ warning: warnings[warnings.length - 1], zone: name, detectorIds: [d.id] })
      continue
    }
    const absent = declared.filter((m) => counts[m] === 0)
    if (absent.length) {
      warnings.push(
        `${label}: no ${absent.join('/')} observed. Zero counts do not establish a turn-only lane; verify permitted movements before choosing removal.`
      )
      const retained = declared.filter((m) => !absent.includes(m))
      const retainedType = Object.keys(movements).find(
        (key) =>
          movements[key].length === retained.length &&
          retained.every((movement) => movements[key].includes(movement))
      )
      // Removal narrows this entry; splitting is a separate, explicit choice.
      // ATSPM has no combined LR type, so that case needs a manual review.
      if (!retained.length || retainedType)
        recommendations.push({
          id: `remove-${d.id}`,
          warning: warnings[warnings.length - 1],
          detectorId: d.id,
          ...(retainedType ? { movements: [retainedType] } : { remove: true }),
          description: removalLabel(d, absent),
        })
      else
        warnings.push(
          `${label}: the remaining Left/Right movements cannot be represented by one entry. Review and split the lane before removing Through.`
        )
    }
    const unassigned = Object.keys(counts).filter(
      (m) => counts[m] > 0 && !assigned.has(m)
    )
    // Do not infer which of several physical lanes sharing a zone should receive another movement.
    const samePhysicalLane = group.every(
      ({ a, d: other }) =>
        a.id === group[0].a.id &&
        Number(other.laneNumber) > 0 &&
        Number(other.laneNumber) === Number(d.laneNumber) &&
        ['V', '1'].includes(String(other.laneType))
    )
    if (group.length > 1 && unassigned.length && !samePhysicalLane) {
      warnings.push(
        `Zone ${name}: unassigned ${unassigned.join('/')} observed across multiple configured lanes; verify which lane serves it.`
      )
      if (group[0].d.id === d.id) manualActions.push({ warning: warnings[warnings.length - 1], zone: name, detectorIds: [], add: true })
      continue
    }
    // Shared movement entries need only one proposal for the missing movement.
    const enough =
      group[0].d.id === d.id
        ? unassigned.filter((m) => counts[m] >= minimum)
        : []
    if (unassigned.some((m) => counts[m] < minimum)) {
      warnings.push(
        `${label}: low-volume unassigned movements; review T ${counts.T}, L ${counts.L}, R ${counts.R} before adding a lane.`
      )
      if (group[0].d.id === d.id) manualActions.push({ warning: warnings[warnings.length - 1], zone: name, detectorIds: [], add: true })
    }
    if (Number(d.laneNumber) > 0 && (declared.length > 1 || enough.length)) {
      const next = [...new Set([...declared, ...enough])]
      const proposals =
        declared.length === 1
          ? enough.map((movement) => [declared[0], movement])
          : [next]
      proposals.forEach((proposal, index) =>
        recommendations.push({
          id: `movements-${d.id}${index ? `-${proposal[1]}` : ''}`,
          detectorId: d.id,
          independentAddition: declared.length === 1,
          movements: proposal,
          description: `${label}: use separate ${proposal.join(' + ')} entries sharing this zone and physical lane (T ${counts.T}, L ${counts.L}, R ${counts.R}). Verify camera classification and allowed turns before applying.`,
        })
      )
    } else if (enough.length) {
      warnings.push(
        `${label}: unassigned ${enough.join('/')} observed; configured lane number needs verification before recommending a split.`
      )
      manualActions.push({ warning: warnings[warnings.length - 1], zone: name, detectorIds: [d.id] })
    }
  }
  // Prefer repairing an existing single-movement entry on the same physical
  // lane over creating a duplicate. An absent zone is evidence to review, not
  // permission to remap a detector automatically.
  const repairs = new Map<Recommendation, (typeof configured)[number]>()
  const ambiguous = new Set<Recommendation>()
  for (const proposal of recommendations) {
    if (!proposal.independentAddition || proposal.movements?.length !== 2)
      continue
    const source = configured.find(({ d }) => d.id === proposal.detectorId)!
    const candidates = configured.filter(
      ({ a, d }) =>
        a.id === source.a.id &&
        Number(d.laneNumber) > 0 &&
        Number(d.laneNumber) === Number(source.d.laneNumber) &&
        ['V', '1'].includes(String(d.laneType)) &&
        movementName(d.movementType) === proposal.movements![1] &&
        !!d.dectectorIdentifier?.trim() &&
        !owners.has(d.dectectorIdentifier)
    )
    if (candidates.length && !allowZoneRepair) {
      ambiguous.add(proposal)
      warnings.push(
        `Zone ${source.d.dectectorIdentifier}: another configured decoder may own the existing turn detector. Verify device ownership before changing its zone; no additional detector is recommended.`
      )
    } else if (candidates.length === 1) repairs.set(proposal, candidates[0])
    else if (candidates.length > 1) {
      ambiguous.add(proposal)
      warnings.push(
        `Zone ${source.d.dectectorIdentifier}: several existing ${movementLabels[proposal.movements[1]]} detectors could share this zone (channels ${candidates.map(({ d }) => d.detectorChannel).join(', ')}). Verify which detector to reassign; no additional detector is recommended.`
      )
    }
  }
  for (const [proposal, candidate] of repairs) {
    if (
      [...repairs.values()].filter(({ d }) => d.id === candidate.d.id).length >
      1
    ) {
      ambiguous.add(proposal)
      warnings.push(
        `Channel ${candidate.d.detectorChannel}: multiple camera zones could match this detector. Verify the zone assignment manually.`
      )
      continue
    }
    const source = configured.find(({ d }) => d.id === proposal.detectorId)!.d
    const from = candidate.d.dectectorIdentifier!
    const to = source.dectectorIdentifier!
    Object.assign(proposal, {
      id: `zone-${candidate.d.id}`,
      detectorId: candidate.d.id,
      independentAddition: false,
      movements: undefined,
      zoneChange: { from, to },
      description: `Change Channel ${candidate.d.detectorChannel} zone ${from} to ${to}`,
    })
  }
  for (let i = recommendations.length - 1; i >= 0; i--)
    if (ambiguous.has(recommendations[i])) recommendations.splice(i, 1)
  const allAssigned = new Set(
    flatten(approaches)
      .filter(({ d }) => active(d))
      .map(({ d }) => d.dectectorIdentifier)
  )
  for (const [name, devices] of owners) {
    // Zones used exclusively on channels 1–64 are deliberately outside this review.
    if (!allAssigned.has(name)) {
      const warning = `Unused zone ${name} (device ${devices.join(', ')}): no configured detector uses it. Assign only after verifying its purpose.`
      warnings.push(warning)
      if (name.trim()) unusedZones.push({ name, deviceIds: devices, warning })
    }
  }
  for (const row of evidence)
    if (
      row.zoneName?.trim() &&
      !owners.get(row.zoneName)?.includes(row.deviceId)
    )
      warnings.push(
        `Bin zone ${row.zoneName} on device ${row.deviceId} is absent from the current inventory; configuration may have changed.`
      )
  return { recommendations, warnings: [...new Set(warnings)], unusedZones, manualActions }
}

/** Stages changes only. Existing IDs/settings remain intact; new entries require explicit unique channels. */
export function applyLaneRecommendations(
  approaches: ConfigApproach[],
  selected: Recommendation[],
  channels: Record<string, number[]>
): ConfigApproach[] {
  const result = structuredClone(approaches)
  if (
    selected.some((r, index) =>
      selected.slice(index + 1).some((other) => laneChangesConflict(r, other))
    )
  )
    throw new Error(
      'Choose only one change per lane entry, except distinct movement additions.'
    )
  const used = new Set(
    flatten(result).map(({ d }) => Number(d.detectorChannel))
  )
  let nextId = Math.max(0, ...flatten(result).map(({ d }) => d.id)) + 1
  for (const change of selected) {
    const match = flatten(result).find(({ d }) => d.id === change.detectorId)
    if (!match || !eligible(match.d))
      throw new Error('Configuration changed. Run reconciliation again.')
    const { a, d } = match
    if (change.zoneChange) {
      if (
        d.dectectorIdentifier !== change.zoneChange.from ||
        !change.zoneChange.to.trim()
      )
        throw new Error('Configuration changed. Run reconciliation again.')
      d.dectectorIdentifier = change.zoneChange.to
      continue
    }
    if (change.remove) {
      a.detectors = a.detectors.filter((entry) => entry.id !== d.id)
      continue
    }
    if (change.movements) {
      const [first, ...rest] = change.movements
      const additions = channels[change.id] ?? []
      if (additions.length !== rest.length)
        throw new Error(
          'Provide one unused channel above 64 for each new movement.'
        )
      const original = { ...d }
      d.movementType = first as unknown as ConfigDetector['movementType']
      rest.forEach((movement, i) => {
        const channel = additions[i]
        if (!Number.isInteger(channel) || channel <= 64 || used.has(channel))
          throw new Error(`Channel ${channel} must be unused and above 64.`)
        used.add(channel)
        a.detectors.push({
          ...original,
          id: nextId++,
          isNew: true,
          approachId: a.id,
          detectorChannel: channel,
          movementType: movement as unknown as ConfigDetector['movementType'],
          dateAdded: new Date().toISOString(),
          detectorComments: [],
        })
      })
    }
  }
  return result
}

/** Reserve distinct defaults across proposals, including channels on disabled entries. */
export function suggestLaneChannels(
  approaches: ConfigApproach[],
  recommendations: Recommendation[]
): Record<string, string[]> {
  const used = new Set(
    flatten(approaches).map(({ d }) => Number(d.detectorChannel))
  )
  let next = 65
  return Object.fromEntries(
    recommendations.map((r) => [
      r.id,
      (r.movements?.slice(1) ?? []).map(() => {
        while (used.has(next)) next++
        used.add(next)
        return String(next++)
      }),
    ])
  )
}

/** Preview the existing entry's movement change without mutating configuration. */
export function originalLaneChange(
  approaches: ConfigApproach[],
  recommendation: Recommendation
): string | undefined {
  const detector = flatten(approaches).find(
    ({ d }) => d.id === recommendation.detectorId
  )?.d
  const after = recommendation.movements?.[0]
  if (!detector || recommendation.remove || !after) return undefined
  const before = movementName(detector.movementType)
  if (before === after) return undefined
  const beforeLabel = (movements[before] ?? [before])
    .map((m) => movementLabels[m] ?? m)
    .join('/')
  const afterLabel = (movements[after] ?? [after])
    .map((m) => movementLabels[m] ?? m)
    .join('/')
  return `When applied, Channel ${detector.detectorChannel} zone ${detector.dectectorIdentifier} will change from ${beforeLabel} to ${afterLabel}.`
}

/** Separate additions can coexist; removals and rewrites remain exclusive. */
export function laneChangesConflict(
  a: Recommendation,
  b: Recommendation
): boolean {
  if (a.detectorId !== b.detectorId) return false
  return !(
    a.independentAddition &&
    b.independentAddition &&
    a.movements?.[0] === b.movements?.[0] &&
    a.movements?.[1] !== b.movements?.[1]
  )
}
