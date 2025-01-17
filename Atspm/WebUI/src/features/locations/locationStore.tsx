import { Device } from '@/api/config/aTSPMConfigurationApi.schemas'
import { create } from 'zustand'

interface StepperState {
  activeStep: number
  setActiveStep: (step: number) => void
  downloadDataResult: any | null
  setDownloadDataResult: (data: any) => void
  syncResult: any | null
  setSyncResult: (data: any) => void
}

interface ValidationState {
  badApproaches: string[]
  badDetectors: string[]
  setBadApproaches: (approaches: string[]) => void
  setBadDetectors: (detectors: string[]) => void
}

interface ModelState {
  devices: Device[]
  setDevices: (devices: Device[]) => void
}

interface LocationStoreState extends StepperState, ValidationState, ModelState {
  resetStore: () => void
}

export const useLocationStore = create<LocationStoreState>((set) => ({
  // Stepper State
  activeStep: 0,
  setActiveStep: (step) => set({ activeStep: step }),
  downloadDataResult: null,
  setDownloadDataResult: (data) => set({ downloadDataResult: data }),
  syncResult: null,
  setSyncResult: (data) => set({ syncResult: data }),

  // Validation State
  badApproaches: [],
  badDetectors: [],
  setBadApproaches: (approaches) => set({ badApproaches: approaches }),
  setBadDetectors: (detectors) => set({ badDetectors: detectors }),

  // Model State
  devices: [],
  setDevices: (devices) => set({ devices }),

  resetStore: () =>
    set({
      activeStep: 0,
      downloadDataResult: null,
      syncResult: null,
      badApproaches: [],
      badDetectors: [],
    }),
}))
