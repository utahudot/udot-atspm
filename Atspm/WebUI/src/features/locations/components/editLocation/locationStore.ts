import {
  deleteApproachFromKey,
  deleteDetectorFromKey,
} from '@/api/config/aTSPMConfigurationApi'
import {
  Approach,
  Detector,
  Location,
} from '@/api/config/aTSPMConfigurationApi.schemas'
import { create } from 'zustand'
import { devtools } from 'zustand/middleware'

export interface ConfigLocation extends Omit<Location, 'id' | 'approaches'> {
  approaches: ConfigApproach[]
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

export interface LocationStore {
  location: ConfigLocation | null
  errors: Record<string, { error: string; id: string }> | null
  warnings: Record<string, { warning: string; id: string }> | null

  // location
  setLocation: (location: ConfigLocation | null) => void
  handleLocationEdit: (name: string, value: string) => void

  // approach
  updateApproaches: (newApproaches: ConfigApproach[]) => void
  addApproach: () => void
  updateApproach: (updatedApproach: ConfigApproach) => void
  copyApproach: (approach: ConfigApproach) => void
  deleteApproach: (approach: ConfigApproach) => void

  // detector
  addDetector: (approach: ConfigApproach) => void
  updateDetector: (
    detectorId: number,
    name: string,
    val: string | number | null
  ) => void
  deleteDetector: (detectorId: number) => void
}

export const useLocationStore = create<LocationStore>()(
  devtools((set, get) => ({
    location: null,
    errors: null,
    warnings: null,

    setLocation: (location: ConfigLocation | null) => {
      set({ location })
    },

    handleLocationEdit: (name: string, value: string) => {
      set((state) => ({
        location: state.location
          ? { ...state.location, [name]: value }
          : state.location,
      }))
    },

    updateApproach: (updatedApproach: ConfigApproach) =>
      set((state) => {
        if (!state.location) return state
        return {
          location: {
            ...state.location,
            approaches:
              state?.location?.approaches?.map((a) =>
                a.id === updatedApproach.id ? updatedApproach : a
              ) || [],
          },
        }
      }),

    updateApproaches: (newApproaches: ConfigApproach[]) =>
      set((state) => {
        if (!state.location) return state
        return {
          location: {
            ...state.location,
            approaches: newApproaches,
          },
        }
      }),

    copyApproach: (approach: ConfigApproach) => {
      const { location, updateApproach } = get()
      if (!location) return

      const newApproach: ConfigApproach = {
        ...approach,
        index: location.approaches?.length || 0,
        isNew: true,
        description: `${approach.description} (copy)`,
        detectors: (approach.detectors || []).map(
          ({ id, ...restDetector }) => ({
            id: Math.round(Math.random() * 10000),
            ...restDetector,
          })
        ),
      }

      updateApproach(newApproach)
    },

    addApproach: () => {
      const { location } = get()

      const id = Math.round(Math.random() * 10000)
      const index = location?.approaches?.length || 0

      const approach: Partial<ConfigApproach> = {
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
        locationId: location.id,
        directionType: {
          id: '0',
          abbreviation: 'NA',
          description: 'Unknown',
          displayOrder: 0,
        },
      }

      set({
        location: {
          ...location,
          approaches: [...(location.approaches || []), approach],
        },
      })
    },

    deleteApproach: (approach: ConfigApproach) => {
      const { location, updateApproaches } = get()
      if (!location) return

      const newApproaches = location.approaches?.filter(
        (a) => a.id !== approach.id
      )

      if (approach.isNew) {
        // If it wasn't saved yet, just remove it from local store
        updateApproaches(newApproaches)
      } else {
        // Otherwise, delete from API
        try {
          deleteApproachFromKey(approach.id)
          updateApproaches(newApproaches)
        } catch (e) {
          console.error(e)
        }
      }
    },

    addDetector: (approach: ConfigApproach) => {
      const { updateApproach } = get()
      const newDetector: ConfigDetector = {
        isNew: true,
        id: Math.floor(Math.random() * 1e8),
        approachId: approach.id,
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
      }

      updateApproach({
        ...approach,
        detectors: [newDetector, ...approach.detectors],
      })
    },

    updateDetector: (detectorId: number, name: string, val: unknown) => {
      const { location } = get()
      if (!location?.approaches) return

      val = val === '' ? null : val

      const newApproaches = location.approaches.map((approach) => ({
        ...approach,
        detectors: approach.detectors?.map((detector) =>
          detector.id === detectorId ? { ...detector, [name]: val } : detector
        ),
      }))

      set({
        location: {
          ...location,
          approaches: newApproaches,
        },
      })
    },

    deleteDetector: (detectorId: number) => {
      const { location } = get()
      if (!location?.approaches) return

      let shouldCallDeleteDetectorFromKey = false

      const newApproaches = location.approaches.map((approach) => ({
        ...approach,
        detectors: approach.detectors?.filter((d) => {
          if (d.id === detectorId && !d.isNew) {
            shouldCallDeleteDetectorFromKey = true
          }
          return d.id !== detectorId
        }),
      }))

      if (shouldCallDeleteDetectorFromKey) {
        deleteDetectorFromKey(detectorId)
      }

      set({
        location: {
          ...location,
          approaches: newApproaches,
        },
      })
    },
  }))
)
