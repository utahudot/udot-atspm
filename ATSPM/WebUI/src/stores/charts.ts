import { create } from 'zustand'

interface ChartsStore {
  activeChart: number | string | null
  setActiveChart: (activeChart: number | string | null) => void
}
export const useChartsStore = create<ChartsStore>((set) => ({
  activeChart: null,
  setActiveChart: (activeChart: number | string | null) => set({ activeChart }),
}))
