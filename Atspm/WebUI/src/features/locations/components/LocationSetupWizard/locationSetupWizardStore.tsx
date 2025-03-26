import { create } from 'zustand'

interface LocationWizardStore {
  // Wizard steps
  activeStep: number
  setActiveStep: (step: number) => void

  // Steps that trigger data download and approaches sync automatically
  dataDownloadStep: number
  syncApproachesStep: number

  // Have we done these operations yet?
  dataDownloaded: boolean
  approachesSynced: boolean

  // Should components perform these operations now?
  shouldDownloadData: boolean
  shouldSyncApproaches: boolean

  // Mark them done or reset flags
  setDataDownloaded: (val: boolean) => void
  setApproachesSynced: (val: boolean) => void
  setShouldDownloadData: (val: boolean) => void
  setShouldSyncApproaches: (val: boolean) => void

  // Bad detectors/approaches
  badApproaches: string[]
  badDetectors: string[]
  setBadApproaches: (approaches: string[]) => void
  setBadDetectors: (detectors: string[]) => void

  // Reset everything
  resetStore: () => void
}

export const useLocationWizardStore = create<LocationWizardStore>(
  (set, get) => ({
    activeStep: 0,

    // By default: step=1 => fetch devices, step=2 => sync approaches
    dataDownloadStep: 1,
    syncApproachesStep: 2,

    dataDownloaded: false,
    approachesSynced: false,

    shouldDownloadData: false,
    shouldSyncApproaches: false,

    // "Bad" items
    badApproaches: [],
    badDetectors: [],

    // Step setter automatically checks if we need to do anything on that step
    setActiveStep: (step) => {
      set({ activeStep: step })
      const {
        dataDownloadStep,
        syncApproachesStep,
        dataDownloaded,
        approachesSynced,
      } = get()

      // If we're on the "download" step and haven't downloaded data yet
      if (step === dataDownloadStep && !dataDownloaded) {
        set({ shouldDownloadData: true })
      }
      // If we're on the "sync" step and haven't synced approaches yet
      if (step === syncApproachesStep && !approachesSynced) {
        set({ shouldSyncApproaches: true })
      }
    },

    setDataDownloaded: (val) => set({ dataDownloaded: val }),
    setApproachesSynced: (val) => set({ approachesSynced: val }),
    setShouldDownloadData: (val) => set({ shouldDownloadData: val }),
    setShouldSyncApproaches: (val) => set({ shouldSyncApproaches: val }),

    setBadApproaches: (approaches) => set({ badApproaches: approaches }),
    setBadDetectors: (detectors) => set({ badDetectors: detectors }),

    resetStore: () =>
      set({
        activeStep: 0,
        dataDownloaded: false,
        approachesSynced: false,
        shouldDownloadData: false,
        shouldSyncApproaches: false,
        badApproaches: [],
        badDetectors: [],
      }),
  })
)
