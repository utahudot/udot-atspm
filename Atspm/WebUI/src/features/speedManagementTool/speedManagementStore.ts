// speedManagementStore.ts
import {
  RouteSpeedOptions,
  TimePeriodFilter,
} from '@/api/speedManagement/aTSPMSpeedManagementApi.schemas'
import {
  DataSource,
  RouteRenderOption,
} from '@/features/speedManagementTool/enums'
import { toUTCDateStamp } from '@/utils/dateTime'
import { startOfMonth, subMonths } from 'date-fns'
import { Map as LeafletMap } from 'leaflet'
import { create } from 'zustand'

const defaultRouteSpeedRequest = (): RouteSpeedOptions => ({
  sourceId: [DataSource.PeMS],
  timePeriod: TimePeriodFilter.AllDay,
  excludeSourceId: false,
  category: null,
  startDate: toUTCDateStamp(startOfMonth(subMonths(new Date(), 2))),
  endDate: toUTCDateStamp(startOfMonth(subMonths(new Date(), 1))),
  startTime: '1970-01-01T00:00:00.000Z',
  endTime: '1970-01-01T23:59:59.000Z',
  violationThreshold: 5,
  region: null,
  excludeRegion: false,
  county: null,
  excludeCounty: false,
  city: null,
  excludeCity: false,
  accessCategory: null,
  excludeAccessCategory: false,
  functionalType: [
    'Blank',
    'Collector Distributer',
    'Interstate',
    'Major Collector',
    'Minor Arterial',
    'Other Principal Arterial',
    'Proposed Major Collector',
    'System To System',
  ],
  excludeFunctionalType: false,
  aggClassification: 'Weekday',
})

interface StoreState {
  routeRenderOption: RouteRenderOption
  setRouteRenderOption: (option: RouteRenderOption) => void

  routeSpeedRequest: RouteSpeedOptions
  setRouteSpeedRequest: (request: RouteSpeedOptions) => void

  submittedRouteSpeedRequest: RouteSpeedOptions
  setSubmittedRouteSpeedRequest: (request: RouteSpeedOptions) => void
  submitCurrentRequest: () => void
  resetToDefaults: () => void

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

  hotspotRoutes: any[]
  setHotspotRoutes: (routes: any[]) => void

  hoveredHotspot: any
  setHoveredHotspot: (hotspot: any) => void

  mapRef: LeafletMap | null
  setMapRef: (map: LeafletMap) => void
  zoomToHotspot: (coordinates: any, zoomLevel?: number) => void
}

const useSpeedManagementStore = create<StoreState>((set, get) => {
  const initial = defaultRouteSpeedRequest()
  return {
    routeRenderOption: RouteRenderOption.Average_Speed,
    setRouteRenderOption: (option) => set({ routeRenderOption: option }),

    routeSpeedRequest: initial,
    setRouteSpeedRequest: (request) => set({ routeSpeedRequest: request }),

    submittedRouteSpeedRequest: { ...initial },
    setSubmittedRouteSpeedRequest: (request) =>
      set({ submittedRouteSpeedRequest: request }),

    submitCurrentRequest: () =>
      set((s) => ({ submittedRouteSpeedRequest: { ...s.routeSpeedRequest } })),

    resetToDefaults: () => {
      const d = defaultRouteSpeedRequest()
      set({
        routeSpeedRequest: d,
        submittedRouteSpeedRequest: { ...d },
      })
    },

    mediumMin: 30,
    setMediumMin: (min) => set({ mediumMin: min }),
    mediumMax: 60,
    setMediumMax: (max) => set({ mediumMax: max }),
    sliderMax: 100,
    setSliderMax: (max) => set({ sliderMax: max }),
    sliderMin: 0,
    setSliderMin: (min) => set({ sliderMin: min }),

    multiselect: false,
    setMultiselect: (multiselect) => set({ multiselect }),

    hotspotRoutes: [],
    setHotspotRoutes: (routes) => set({ hotspotRoutes: routes }),

    hoveredHotspot: null,
    setHoveredHotspot: (hotspot) => set({ hoveredHotspot: hotspot }),

    mapRef: null,
    setMapRef: (map: LeafletMap) => set({ mapRef: map }),
    zoomToHotspot: (coordinates: any, zoomLevel?: number) => {
      const { mapRef } = get()
      if (mapRef) {
        mapRef.fitBounds(coordinates, {
          padding: [50, 50],
          maxZoom: zoomLevel ?? 10,
        })
      }
    },
  }
})

export default useSpeedManagementStore
