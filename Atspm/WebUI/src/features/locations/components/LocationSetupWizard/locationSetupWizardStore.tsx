import { create } from 'zustand'

type Status = 'NOT_STARTED' | 'READY_TO_RUN' | 'DONE'

interface LocationWizardStore {
  useWizard: boolean
  setUseWizard: (useWizard: boolean) => void
  activeStep: number
  setActiveStep: (step: number) => void
  deviceVerificationStatus: Status
  setDeviceVerificationStatus: (status: Status) => void
  approachVerificationStatus: Status
  setApproachVerificationStatus: (status: Status) => void
  badApproaches: string[]
  badDetectors: string[]
  setBadApproaches: (approaches: number[]) => void
  setBadDetectors: (detectors: string[]) => void
  resetStore: () => void
}

export const useLocationWizardStore = create<LocationWizardStore>(
  (set, get) => ({
    useWizard: false,
    activeStep: 0,
    deviceVerificationStatus: 'NOT_STARTED',
    approachVerificationStatus: 'NOT_STARTED',
    badApproaches: [],
    badDetectors: [],

    setUseWizard: (useWizard) => set({ useWizard }),

    setActiveStep: (step) => {
      set({ activeStep: step })

      const { deviceVerificationStatus, approachVerificationStatus } = get()

      if (step === 1 && deviceVerificationStatus === 'NOT_STARTED') {
        set({ deviceVerificationStatus: 'READY_TO_RUN' })
      }

      if (step === 2 && approachVerificationStatus === 'NOT_STARTED') {
        set({ approachVerificationStatus: 'READY_TO_RUN' })
      }
    },

    setDeviceVerificationStatus: (status) =>
      set({ deviceVerificationStatus: status }),
    setApproachVerificationStatus: (status) =>
      set({ approachVerificationStatus: status }),

    setBadApproaches: (approaches) => set({ badApproaches: approaches }),
    setBadDetectors: (detectors) => set({ badDetectors: detectors }),

    resetStore: () =>
      set({
        activeStep: 0,
        deviceVerificationStatus: 'NOT_STARTED',
        approachVerificationStatus: 'NOT_STARTED',
        badApproaches: [],
        badDetectors: [],
      }),
  })
)
