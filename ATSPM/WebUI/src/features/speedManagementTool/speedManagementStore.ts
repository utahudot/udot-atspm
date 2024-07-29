import { create } from 'zustand'

// Route options
export enum RouteRenderOption {
  Violations = 'Violations',
  Posted_Speed = 'Posted Speed',
  Average_Speed = 'Average Speed',
  Percentile_85th = '85th Percentile',
  Percentile_95th = '95th Percentile',
  Percentile_99th = '99th Percentile',
}

export enum AnalysisPeriod {
  AllDay,
  PeekPeriod,
  CustomHour,
}

export enum DataSource {
  ATSPM = 1,
  PeMS = 2,
  ClearGuide = 3,
}

export interface RouteSpeedRequest {
  sourceId: DataSource
  startDate: string
  endDate: string
  daysOfWeek: number[]
  analysisPeriod: AnalysisPeriod
  violationThreshold: number
  customStartTime?: Date
  customEndTime?: Date
}

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
}

// Create the Zustand store
const useSpeedManagementStore = create<StoreState>((set) => ({
  routeRenderOption: RouteRenderOption.Average_Speed,

  // Function to set the selected route render option
  setRouteRenderOption: (option: RouteRenderOption) =>
    set({ routeRenderOption: option }),

  // Default route speed request
  routeSpeedRequest: {
    sourceId: DataSource.ClearGuide,
    startDate: '2024-02-08',
    endDate: '2024-02-09',
    daysOfWeek: [1, 2, 3, 4, 5],
    analysisPeriod: AnalysisPeriod.AllDay,
    violationThreshold: 5,
  },

  submittedRouteSpeedRequest: {
    sourceId: DataSource.ClearGuide,
    startDate: '2024-02-08',
    endDate: '2024-02-09',
    daysOfWeek: [1, 2, 3, 4, 5],
    analysisPeriod: AnalysisPeriod.AllDay,
    violationThreshold: 5,
  },

  // Function to set the route speed request
  setRouteSpeedRequest: (request: RouteSpeedRequest) => {
    set({ routeSpeedRequest: request })
  },

  // Function to set the submitted route speed request
  setSubmittedRouteSpeedRequest: (request: RouteSpeedRequest) => {
    set({ submittedRouteSpeedRequest: request })
  },

  // Default slider min/max values
  // Default medium min/max values
  mediumMin: 30,
  setMediumMax: (max: number) => set({ mediumMax: max }),
  mediumMax: 60,
  setMediumMin: (min: number) => set({ mediumMin: min }),
  sliderMax: 100,
  setSliderMax: (max: number) => set({ sliderMax: max }),
  sliderMin: 0,
  setSliderMin: (min: number) => set({ sliderMin: min }),
}))

export default useSpeedManagementStore
