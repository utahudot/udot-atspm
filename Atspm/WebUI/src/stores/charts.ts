// #region license
// Copyright 2024 Utah Departement of Transportation
// for WebUI - charts.ts
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

interface ChartsStore {
  activeChart: number | string | null
  setActiveChart: (activeChart: number | string | null) => void
  yAxisMaxStore: string | null
  setYAxisMaxStore: (yAxisMaxStore: string | null) => void
  syncZoom: boolean
  setSyncZoom: (syncZoom: boolean) => void
}

export const useChartsStore = create<ChartsStore>((set) => ({
  activeChart: null,
  setActiveChart: (activeChart: number | string | null) => set({ activeChart }),

  yAxisMaxStore: null,
  setYAxisMaxStore: (yAxisMaxStore: string | null) => set({ yAxisMaxStore }),

  syncZoom: false,
  setSyncZoom: (syncZoom: boolean) => set({ syncZoom }),
}))
