// #region license
// Copyright 2026 Utah Departement of Transportation
// for WebUI - locationSetupWizardStore.tsx
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//http://www.apache.org/licenses/LICENSE-2.
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// #endregion

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
  badApproaches: number[]
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
