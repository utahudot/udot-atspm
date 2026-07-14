import { useEffect, useState } from 'react'

export type ItemStatus = 'pending' | 'ignored' | 'added' | 'deleted' | 'unsaved'

export interface LocationDiscrepancyReport {
  notFoundApproaches: { id: number; description?: string }[]
  notFoundDetectorChannels: string[]
  foundPhaseNumbers: number[]
  foundDetectorChannels: number[]
}

interface Approach {
  id: number
  protectedPhaseNumber: number | null
  isNew?: boolean
  detectors: {
    id?: number
    detectorChannel?: string | number
    isNew?: boolean
  }[]
}

const k = (
  kind: 'NOT_FOUND_APP' | 'NOT_FOUND_DET' | 'FOUND_PHASE' | 'FOUND_DET',
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

  // found phases: if they exist non-new -> added, else unsaved
  useEffect(() => {
    setItemStatuses((prev) => {
      const next = { ...prev }

      for (const phase of categories.foundPhaseNumbers ?? []) {
        const key = k('FOUND_PHASE', phase)

        const matching = approaches.filter(
          (a) => a.protectedPhaseNumber === phase
        )
        if (!matching.length) continue

        const existsNonNew = matching.some((a) => !a.isNew)
        const current = next[key] || 'pending'

        if (current !== 'pending') continue

        next[key] = existsNonNew ? 'added' : 'unsaved'
      }

      return next
    })
  }, [approaches, categories.foundPhaseNumbers])

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
