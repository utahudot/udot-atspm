import {
  AnalysisPeriod,
  DataSource,
  RouteRenderOption,
} from '@/features/speedManagementTool/enums'
import { RouteSpeedRequest } from '@/features/speedManagementTool/types/routeSpeedRequest'
import { create } from 'zustand'

interface StoreState {
  routeRenderOption: RouteRenderOption
  setRouteRenderOption: (option: RouteRenderOption) => void

  routeSpeedRequest: RouteSpeedRequest
  setRouteSpeedRequest: (request: RouteSpeedRequest) => void

  submittedRouteSpeedRequest: RouteSpeedRequest
  setSubmittedRouteSpeedRequest: (request: RouteSpeedRequest) => void

  mediumMin: number
  setMediumMin: (min: number) => void
  mediumMax: number
  setMediumMax: (max: number) => void
  sliderMax: number
  setSliderMax: (max: number) => void
  sliderMin: number
  setSliderMin: (min: number) => void

  multiselect: boolean
  setMultiselect: (multiselect: boolean) => void
}

// Create the Zustand store
const useSpeedManagementStore = create<StoreState>((set) => ({
  routeRenderOption: RouteRenderOption.Average_Speed,

  setRouteRenderOption: (option: RouteRenderOption) =>
    set({ routeRenderOption: option }),

  routeSpeedRequest: {
    sourceId: DataSource.PeMS,
    startDate: '2023-01-01',
    endDate: '2024-08-30',
    daysOfWeek: [1, 2, 3, 4, 5],
    analysisPeriod: AnalysisPeriod.AllDay,
    violationThreshold: 5,
    region: null,
    county: null,
    city: null,
    accessCategory: null,
    functionalType: null,
  },

  submittedRouteSpeedRequest: {
    sourceId: DataSource.PeMS,
    startDate: '2023-01-01',
    endDate: '2024-08-30',
    daysOfWeek: [1, 2, 3, 4, 5],
    analysisPeriod: AnalysisPeriod.AllDay,
    violationThreshold: 5,
    region: null,
    county: null,
    city: null,
    accessCategory: null,
    functionalType: null,
  },

  setRouteSpeedRequest: (request: RouteSpeedRequest) => {
    set({ routeSpeedRequest: request })
  },

  setSubmittedRouteSpeedRequest: (request: RouteSpeedRequest) => {
    set({ submittedRouteSpeedRequest: request })
  },

  mediumMin: 30,
  setMediumMax: (max: number) => set({ mediumMax: max }),
  mediumMax: 60,
  setMediumMin: (min: number) => set({ mediumMin: min }),
  sliderMax: 100,
  setSliderMax: (max: number) => set({ sliderMax: max }),
  sliderMin: 0,
  setSliderMin: (min: number) => set({ sliderMin: min }),

  multiselect: false,
  setMultiselect: (multiselect: boolean) => set({ multiselect }),
}))

export default useSpeedManagementStore