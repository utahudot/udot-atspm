import {
  Approach,
  deleteApproachFromKey,
  deleteDetectorFromKey,
  Detector,
  Location,
} from '@/api/config'
import { devtools } from 'zustand/middleware'
import { createWithEqualityFn } from 'zustand/traditional'

export interface ConfigLocation extends Omit<Location, 'id' | 'approaches'> {
  id: number
}

export interface ConfigApproach
  extends Omit<Approach, 'id' | 'detectors' | 'protectedPhaseNumber'> {
  id: number
  index?: number
  open?: boolean
  isNew?: boolean
  detectors: ConfigDetector[]
  protectedPhaseNumber: number | null
}

export interface ConfigDetector extends Omit<Detector, 'id' | 'approachId'> {
  id: number
  approachId?: number
  isNew?: boolean
}

interface LocationSlice {
  location: ConfigLocation | null
  errors: Record<string, { error: string; id: string }> | null
  warnings: Record<string, { warning: string; id: string }> | null

  setLocation: (location: ConfigLocation | null) => void
  handleLocationEdit: (name: string, value: string) => void

  setErrors: (
    errors: Record<string, { error: string; id: string }> | null
  ) => void
  setWarnings: (
    warnings: Record<string, { warning: string; id: string }> | null
  ) => void
  clearErrorsAndWarnings: () => void
}

interface ApproachSlice {
  approaches: ConfigApproach[]
  savedApproaches: ConfigApproach[] //used to check for unsaved changes
  hasUnsavedChanges: () => boolean
  channelMap: Map<number, number>

  updateApproaches: (newApproaches: ConfigApproach[]) => void
  addApproach: () => void
  updateApproach: (updatedApproach: ConfigApproach) => void
  updateSavedApproach: (updatedApproach: ConfigApproach) => void
  updateSavedApproaches: (updatedApproaches: ConfigApproach[]) => void
  resetApproaches: () => void
  copyApproach: (approach: ConfigApproach) => void
  deleteApproach: (approach: ConfigApproach) => void
  resetStore: () => void

  addDetector: (approachId: number) => void
  updateDetector: (detectorId: number, name: string, val: unknown) => void
  deleteDetector: (detectorId: number) => void
}

export type LocationStore = LocationSlice & ApproachSlice

export const useLocationStore = createWithEqualityFn<LocationStore>()(
  devtools((set, get) => ({
    location: null,
    errors: null,
    warnings: null,
    savedApproaches: [],

    setLocation: (location) => {
      const approachList = location?.approaches ?? []
      const newMap = new Map<number, number>()
      approachList.forEach((approach) =>
        approach.detectors.forEach((detector) =>
          newMap.set(detector.id, detector.detectorChannel || 0)
        )
      )

      set(() => ({
        location: location ? location : null,
        approaches: approachList,
        savedApproaches: JSON.parse(JSON.stringify(approachList)),
        channelMap: newMap,
      }))
    },

    hasUnsavedChanges: () => {
      const { approaches, savedApproaches } = get()
      if (approaches.length !== savedApproaches.length) return true

      const stable = (obj: any) => JSON.stringify(normalize(obj))

      for (const a of approaches) {
        const saved = savedApproaches.find((s) => s.id === a.id)
        if (!saved) return true

        if (stable(stripUIFlags(a)) !== stable(stripUIFlags(saved))) return true
      }
      return false
    },

    handleLocationEdit: (name, value) => {
      set((state) => ({
        location: state.location
          ? { ...state.location, [name]: value }
          : state.location,
      }))
    },

    setErrors: (errors) => set({ errors }),
    setWarnings: (warnings) => set({ warnings }),
    clearErrorsAndWarnings: () => set({ errors: null, warnings: null }),

    approaches: [],
    channelMap: new Map(),

    updateApproaches: (newApproaches) => {
      set(() => ({ approaches: newApproaches }))
    },

    updateApproach: (updatedApproach) => {
      const { approaches } = get()
      const idx = approaches.findIndex((a) => a.id === updatedApproach.id)

      if (idx === -1) {
        set({ approaches: [...approaches, updatedApproach] })
        return
      }

      const copy = [...approaches]
      copy[idx] = updatedApproach
      set({ approaches: copy })
    },

    updateSavedApproach: (updatedApproach) => {
      const { savedApproaches } = get()
      const idx = savedApproaches.findIndex((a) => a.id === updatedApproach.id)

      if (idx === -1) {
        set({ savedApproaches: [...savedApproaches, updatedApproach] })
        return
      }

      const copy = [...savedApproaches]
      copy[idx] = updatedApproach
      set({ savedApproaches: copy })
    },

    updateSavedApproaches: (updatedApproaches) => {
      set({ savedApproaches: updatedApproaches })
    },

    resetApproaches: () => {
      const { savedApproaches } = get()
      set({ approaches: JSON.parse(JSON.stringify(savedApproaches)) })
    },

    addApproach: () => {
      const { location, approaches } = get()
      const id = Math.round(Math.random() * 10000)
      const index = approaches.length

      const newApproach: ConfigApproach = {
        id,
        index,
        description: 'New Approach',
        isNew: true,
        detectors: [],
        isProtectedPhaseOverlap: false,
        isPermissivePhaseOverlap: false,
        isPedestrianPhaseOverlap: false,
        permissivePhaseNumber: null,
        pedestrianPhaseNumber: null,
        pedestrianDetectors: '',
        locationId: location?.id,
        directionType: {
          id: '0',
          abbreviation: 'NA',
          description: 'Unknown',
          displayOrder: 0,
        },
        protectedPhaseNumber: null,
      }

      set({ approaches: [newApproach, ...approaches] })
    },

    copyApproach: (approach) => {
      const { approaches } = get()
      const newApproach: ConfigApproach = {
        ...approach,
        id: Math.round(Math.random() * 10000),
        index: approaches.length,
        isNew: true,
        description: `${approach.description} (copy)`,
        detectors: approach.detectors.map(({ id, ...rest }) => ({
          ...rest,
          id: Math.round(Math.random() * 10000),
          isNew: true,
        })),
      }
      set({ approaches: [...approaches, newApproach] })
    },

    deleteApproach: (approach) => {
      const { approaches } = get()
      const filtered = approaches.filter((a) => a.id !== approach.id)

      if (!approach.isNew) {
        try {
          deleteApproachFromKey(approach.id)
        } catch (err) {
          console.error(err)
        }
      }
      set({ approaches: filtered })
    },

    resetStore: () => {
      set({
        location: null,
        approaches: [],
        savedApproaches: [],
        channelMap: new Map(),
        errors: null,
        warnings: null,
      })
    },

    addDetector: (approachId) => {
      const { approaches, updateApproach } = get()
      const approach = approaches.find((a) => a.id === approachId)
      if (!approach) return

      const newDetector: ConfigDetector = {
        isNew: true,
        id: Math.floor(Math.random() * 1e8),
        approachId,
        dateDisabled: null,
        decisionPoint: null,
        dectectorIdentifier: '',
        distanceFromStopBar: null,
        laneNumber: null,
        latencyCorrection: 0,
        movementDelay: null,
        detectionTypes: [],
        dateAdded: new Date().toISOString(),
        detectorComments: [],
        detectionHardware: 'NA',
        movementType: 'NA',
        laneType: 'NA',
      }

      updateApproach({
        ...approach,
        detectors: [newDetector, ...approach.detectors],
      })
    },

    updateDetector: (detectorId, name, val) => {
      const { approaches, channelMap } = get()

      const updatedApproaches = approaches.map((approach) => {
        let found = false
        const newDetectors = approach.detectors.map((det) => {
          if (det.id === detectorId) {
            found = true
            return { ...det, [name]: val }
          }
          return det
        })
        if (!found) {
          return approach
        }
        return { ...approach, detectors: newDetectors }
      })

      if (name === 'detectorChannel') {
        const newChannel =
          typeof val === 'number' ? val : parseInt(val as string) || 0
        channelMap.set(detectorId, newChannel)
        set({ channelMap: new Map(channelMap) })
      }

      set({ approaches: updatedApproaches })
    },

    deleteDetector: (detectorId) => {
      const { approaches, channelMap } = get()
      let shouldCallApi = false

      const updatedApproaches = approaches.map((approach) => {
        const index = approach.detectors.findIndex((d) => d.id === detectorId)
        if (index === -1) {
          return approach
        }
        const filtered = approach.detectors.filter((d) => {
          if (d.id === detectorId && !d.isNew) {
            shouldCallApi = true
          }
          return d.id !== detectorId
        })
        return { ...approach, detectors: filtered }
      })

      if (shouldCallApi) {
        try {
          deleteDetectorFromKey(detectorId)
          channelMap.delete(detectorId)
        } catch (err) {
          console.error(err)
        }
      }

      set({ approaches: updatedApproaches })
    },
  }))
)

const normalize = (v: any): any => {
  if (Array.isArray(v)) return v.map(normalize)
  if (v !== null && typeof v === 'object')
    return Object.fromEntries(
      Object.entries(v).map(([k, val]) => [k, normalize(val)])
    )
  return typeof v === 'number' ? String(v) : v
}

const stripUIFlags = (approach: ConfigApproach) => {
  const { open, index, isNew, ...clean } = approach
  return {
    ...clean,
    detectors: approach.detectors.map(({ isNew: _dNew, ...d }) => d),
  }
}
