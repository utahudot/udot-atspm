import { create } from 'zustand'

interface LocationStoreState {
  badApproaches: string[] // List of bad approach descriptions
  badDetectors: string[] // List of bad detector channels
  setBadApproaches: (approaches: string[]) => void
  setBadDetectors: (detectors: string[]) => void
  reset: () => void
}

export const useLocationStore = create<LocationStoreState>((set) => ({
  badApproaches: [],
  badDetectors: [],
  setBadApproaches: (approaches) => set({ badApproaches: approaches }),
  setBadDetectors: (detectors) => set({ badDetectors: detectors }),
  reset: () =>
    set({
      badApproaches: [],
      badDetectors: [],
    }),
}))
