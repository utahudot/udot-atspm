import { useEffect, useState } from 'react'

export type ItemStatus = 'pending' | 'ignored' | 'added' | 'deleted' | 'unsaved'

export interface MissingPhaseAssignment {
  approachId: number
  role: 'protected' | 'permissive' | 'pedestrian'
  phaseNumber: number
  isOverlap: boolean
}
export const assignmentKey = (a: MissingPhaseAssignment) =>
  `NOT_FOUND_ASSIGNMENT:${a.approachId}:${a.role}:${a.phaseNumber}:${a.isOverlap}`
export const assignmentFields = {
  protected: ['protectedPhaseNumber', 'isProtectedPhaseOverlap'],
  permissive: ['permissivePhaseNumber', 'isPermissivePhaseOverlap'],
  pedestrian: ['pedestrianPhaseNumber', 'isPedestrianPhaseOverlap'],
} as const
export interface LocationDiscrepancyReport {
  missingPhaseAssignments?: MissingPhaseAssignment[]
  notFoundApproaches: { id: number; description?: string }[]
  notFoundDetectorChannels: string[]
  foundPhaseNumbers: number[]
  foundOverlapNumbers?: number[]
  warnings?: string[]
  foundDetectorChannels: number[]
}

interface Approach {
  id: number
  protectedPhaseNumber: number | null
  permissivePhaseNumber?: number | null
  isProtectedPhaseOverlap?: boolean
  isPermissivePhaseOverlap?: boolean
  pedestrianPhaseNumber?: number | null
  isPedestrianPhaseOverlap?: boolean
  isNew?: boolean
  detectors: {
    id?: number
    detectorChannel?: string | number
    isNew?: boolean
  }[]
}

const k = (
  kind:
    | 'NOT_FOUND_APP'
    | 'NOT_FOUND_DET'
    | 'FOUND_PHASE'
    | 'FOUND_OVERLAP'
    | 'FOUND_DET',
  raw: string | number
) => `${kind}:${raw}`

const useDiscrepancyStatuses = (
  categories: LocationDiscrepancyReport,
  approaches: Approach[]
) => {
  const [itemStatuses, setItemStatuses] = useState<Record<string, ItemStatus>>(
    {}
  )

  const updateStatus = (id: string, status: ItemStatus) => {
    setItemStatuses((prev) => ({ ...prev, [id]: status }))
  }

  // init statuses (only set missing keys)
  useEffect(() => {
    setItemStatuses((prev) => {
      const next = { ...prev }

      for (const a of categories.notFoundApproaches ?? []) {
        const key = k('NOT_FOUND_APP', a.id)
        if (!next[key]) next[key] = 'pending'
      }

      for (const det of categories.notFoundDetectorChannels ?? []) {
        const key = k('NOT_FOUND_DET', det)
        if (!next[key]) next[key] = 'pending'
      }

      for (const phase of categories.foundOverlapNumbers ?? []) {
        const key = k('FOUND_OVERLAP', phase)
        if (!next[key]) next[key] = 'pending'
      }

      for (const phase of categories.foundPhaseNumbers ?? []) {
        const key = k('FOUND_PHASE', phase)
        if (!next[key]) next[key] = 'pending'
      }

      for (const det of categories.foundDetectorChannels ?? []) {
        const key = k('FOUND_DET', det)
        if (!next[key]) next[key] = 'pending'
      }

      return next
    })
  }, [categories])

  // if a not-found approach no longer exists in store, mark deleted
  useEffect(() => {
    setItemStatuses((prev) => {
      const next = { ...prev }
      for (const a of categories.notFoundApproaches ?? []) {
        const key = k('NOT_FOUND_APP', a.id)
        const exists = approaches.some((x) => x.id === a.id)
        if (!exists && next[key] !== 'deleted') next[key] = 'deleted'
      }
      return next
    })
  }, [approaches, categories.notFoundApproaches])

  // if a not-found detector channel no longer exists in store, mark deleted
  useEffect(() => {
    const storeChannels = approaches.flatMap((a) =>
      a.detectors.map((d) => d.detectorChannel?.toString())
    )
    setItemStatuses((prev) => {
      const next = { ...prev }
      for (const det of categories.notFoundDetectorChannels ?? []) {
        const key = k('NOT_FOUND_DET', det)
        const exists = storeChannels.includes(det)
        if (!exists && next[key] !== 'deleted') next[key] = 'deleted'
      }
      return next
    })
  }, [approaches, categories.notFoundDetectorChannels])

  useEffect(() => {
    setItemStatuses((prev) => {
      const next = { ...prev }
      for (const assignment of categories.missingPhaseAssignments ?? []) {
        const fields = assignmentFields[assignment.role]
        if (!fields) continue
        const approach = approaches.find((a) => a.id === assignment.approachId)
        const matches =
          approach &&
          Number(approach[fields[0]]) === assignment.phaseNumber &&
          Boolean(approach[fields[1]]) === assignment.isOverlap
        const key = assignmentKey(assignment)
        if (!matches) next[key] = 'deleted'
        else if (!next[key] || next[key] === 'deleted') next[key] = 'pending'
      }
      return next
    })
  }, [approaches, categories.missingPhaseAssignments])

  // found phases: if they exist non-new -> added, else unsaved
  useEffect(() => {
    setItemStatuses((prev) => {
      const next = { ...prev }

      for (const overlap of [false, true]) {
        for (const phase of (overlap
          ? categories.foundOverlapNumbers
          : categories.foundPhaseNumbers) ?? []) {
          const key = k(overlap ? 'FOUND_OVERLAP' : 'FOUND_PHASE', phase)
          const matching = approaches.filter(
            (a) =>
              (a.protectedPhaseNumber === phase &&
                !!a.isProtectedPhaseOverlap === overlap) ||
              (a.permissivePhaseNumber === phase &&
                !!a.isPermissivePhaseOverlap === overlap)
          )
          if (!matching.length || (next[key] && next[key] !== 'pending'))
            continue
          next[key] = matching.some((a) => !a.isNew) ? 'added' : 'unsaved'
        }
      }

      return next
    })
  }, [approaches, categories.foundPhaseNumbers, categories.foundOverlapNumbers])

  // found detector channels: if exist non-new -> added, else unsaved
  useEffect(() => {
    setItemStatuses((prev) => {
      const next = { ...prev }

      for (const det of categories.foundDetectorChannels ?? []) {
        const key = k('FOUND_DET', det)

        const matching = approaches
          .flatMap((a) => a.detectors)
          .filter((d) => d.detectorChannel?.toString() === det.toString())

        if (!matching.length) continue

        const existsNonNew = matching.some((d) => !d.isNew)
        const current = next[key] || 'pending'

        if (current !== 'pending') continue

        next[key] = existsNonNew ? 'added' : 'unsaved'
      }

      return next
    })
  }, [approaches, categories.foundDetectorChannels])

  return { itemStatuses, updateStatus, k }
}

export default useDiscrepancyStatuses
