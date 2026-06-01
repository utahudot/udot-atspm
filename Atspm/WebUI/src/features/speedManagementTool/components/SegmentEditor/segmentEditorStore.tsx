import { AllSegmentsProperties } from '@/api/speedManagement/aTSPMSpeedManagementApi.schemas'
import { Feature } from '@/features/speedManagementTool/components/SegmentEditor/SegmentEditorMap/hooks/useMapClickHandler'
import { create } from 'zustand'
import { devtools } from 'zustand/middleware'
import { DataSource } from '../../enums'

export interface Segment {
  id: string
  type?: string | null
  geometry: {
    type?: string
    coordinates: number[][] | null
  }
  properties?: AllSegmentsProperties | undefined
}

export interface SegmentProperties {
  id: string | null
  udotRouteNumber: string | null
  startMilePoint: number | null
  endMilePoint: number | null
  functionalType: string | null
  name: string | null
  direction: string | null
  polarity: 'PM' | 'NM' | null
  speedLimit: number | null
  region: string | null
  city: string | null
  county: string | null
  alternateIdentifier: string | null
  accessCategory: string | null
  offset: number | null
  shape: string | null
  shapeWKT: string | null
  entities: string[] | null
}

export interface Entity {
  id: string
  entityId: string
  entityType: string
  sourceId: DataSource
  coordinates: number[][]
  version: string
  isWithin50Ft?: boolean
}

interface SegmentEditorState {
  allSegments: Segment[] | null
  isLoadingSegments: boolean
  segmentProperties: SegmentProperties
  polylineCoordinates: [number, number][]
  initialPolylineCoordinates: [number, number][]
  nearByEntities: Entity[]
  associatedEntityIds: string[]
  selectedEntityVersions: {
    [DataSource.ATSPM]: string[]
    [DataSource.PeMS]: string[]
    [DataSource.ClearGuide]: string[]
  }
  isLoadingEntities: boolean
  activeTab: number
  udotRoutes: any[]
  isLoadingUdotRoutes: boolean
  mapCenter: { lat: number; lng: number; zoom: number } | null
  legendVisibility: {
    draft: boolean
    lrs: boolean
    existing: boolean
    clearguide: boolean
    'clearguide-selected': boolean
    pems: boolean
    'pems-selected': boolean
    atspm: boolean
    'atspm-selected': boolean
  }
  setLegendVisibility: (
    key: keyof SegmentEditorState['legendVisibility'],
    visible: boolean
  ) => void
  toggleLegendVisibility: (
    key: keyof SegmentEditorState['legendVisibility']
  ) => void
  setAllSegments: (segments: Segment[] | null) => void
  setIsLoadingSegments: (isLoading: boolean) => void
  updateSegmentProperties: (properties: Partial<SegmentProperties>) => void
  setSegmentProperties: (properties: SegmentProperties) => void
  setInitialPolylineCoordinates: (coordinates: [number, number][]) => void
  setPolylineCoordinates: (coordinates: [number, number][]) => void
  addPolylineCoordinate: (
    latlng: [number, number],
    position?: 'start' | 'end'
  ) => void
  removePolylineCoordinate: (index: number) => void
  updatePolylineCoordinate: (
    index: number,
    newPosition: [number, number]
  ) => void
  lockedRoute: Feature | null
  setLockedRoute: (route: Feature | null) => void
  setNearByEntities: (entities: Entity[]) => void
  setAssociatedEntityIds: (ids: string[]) => void
  setSelectedEntityVersions: (source: DataSource, versions: string[]) => void
  toggleEntitySelection: (entityId: string) => void
  setIsLoadingEntities: (isLoading: boolean) => void
  setActiveTab: (tab: number) => void
  setUdotRoutes: (routes: any[]) => void
  setIsLoadingUdotRoutes: (isLoading: boolean) => void
  setMapCenter: (mapCenter: { lat: number; lng: number; zoom: number }) => void
  upsertSegment: (segment: Segment) => void
  removeSegment: (segmentId: string) => void
  reset: () => void
}

const initialSegmentProperties: SegmentProperties = {
  id: null,
  udotRouteNumber: null,
  startMilePoint: null,
  endMilePoint: null,
  functionalType: null,
  name: null,
  direction: null,
  polarity: 'PM',
  speedLimit: null,
  region: null,
  city: null,
  county: null,
  alternateIdentifier: null,
  accessCategory: null,
  offset: 0,
  shape: null,
  shapeWKT: null,
  entities: [],
}

const coordinatesToLineString = (coords: number[][]): string => {
  const coordString = coords.map(([lng, lat]) => `${lng} ${lat}`).join(', ')
  return `LINESTRING (${coordString})`
}

const getFirstVersion = (entities: Entity[], source: DataSource): string[] => {
  const versions = Array.from(
    new Set(
      entities
        .filter((entity) => entity.sourceId === source)
        .map((entity) => entity.version)
        .filter((version): version is string => !!version)
    )
  ).sort()
  return versions.length ? [versions[0]] : []
}

export const useSegmentEditorStore = create<SegmentEditorState>()(
  devtools(
    (set, get) => ({
      allSegments: null,
      isLoadingSegments: false,
      segmentProperties: initialSegmentProperties,
      polylineCoordinates: [],
      initialPolylineCoordinates: [],
      nearByEntities: [],
      selectedEntityVersions: {
        [DataSource.ATSPM]: [],
        [DataSource.PeMS]: [],
        [DataSource.ClearGuide]: [],
      },
      associatedEntityIds: [],
      isLoadingEntities: false,
      activeTab: 0,
      udotRoutes: [],
      isLoadingUdotRoutes: false,
      mapCenter: null,

      lockedRoute: null,
      setLockedRoute: (route) => set({ lockedRoute: route }),

      setAllSegments: (segments) => set({ allSegments: segments }),

      setIsLoadingSegments: (isLoading) =>
        set({ isLoadingSegments: isLoading }),

      updateSegmentProperties: (properties) =>
        set((state) => ({
          segmentProperties: {
            ...state.segmentProperties,
            ...properties,
            shape: properties.polylineCoordinates
              ? coordinatesToLineString(properties.polylineCoordinates)
              : state.segmentProperties.shape,
          },
        })),

      setSegmentProperties: (properties) =>
        set({
          segmentProperties: {
            ...initialSegmentProperties,
            ...properties,
            shape: properties.polylineCoordinates
              ? coordinatesToLineString(properties.polylineCoordinates)
              : properties.shape,
          },
        }),

      setInitialPolylineCoordinates: (coordinates) =>
        set((state) => ({
          initialPolylineCoordinates: coordinates,
          segmentProperties: {
            ...state.segmentProperties,
            shape: coordinatesToLineString(coordinates),
          },
        })),

      setPolylineCoordinates: (coordinates) =>
        set((state) => ({
          polylineCoordinates: coordinates,
          segmentProperties: {
            ...state.segmentProperties,
            shape: coordinatesToLineString(coordinates),
          },
        })),

      addPolylineCoordinate: (latlng, position = 'end') =>
        set((state) => {
          let newCoordinates: number[][]
          if (state.polylineCoordinates.length === 0) {
            newCoordinates = [latlng]
          } else {
            newCoordinates =
              position === 'start'
                ? [latlng, ...state.polylineCoordinates]
                : [...state.polylineCoordinates, latlng]
          }
          return {
            polylineCoordinates: newCoordinates,
            segmentProperties: {
              ...state.segmentProperties,
              shape: coordinatesToLineString(newCoordinates),
            },
          }
        }),

      removePolylineCoordinate: (index) =>
        set((state) => {
          const newCoordinates = state.polylineCoordinates.filter(
            (_, i) => i !== index
          )
          return {
            polylineCoordinates: newCoordinates,
            segmentProperties: {
              ...state.segmentProperties,
              shape: coordinatesToLineString(newCoordinates),
            },
          }
        }),

      updatePolylineCoordinate: (index, newPosition) =>
        set((state) => {
          const newCoordinates = [...state.polylineCoordinates]
          newCoordinates[index] = newPosition
          return {
            polylineCoordinates: newCoordinates,
            segmentProperties: {
              ...state.segmentProperties,
              shape: coordinatesToLineString(newCoordinates),
            },
          }
        }),

      setNearByEntities: (entities) =>
        set({
          nearByEntities: entities.filter(
            (entity, index, self) =>
              index === self.findIndex((e) => e.id === entity.id)
          ),
          selectedEntityVersions: {
            [DataSource.ATSPM]: getFirstVersion(entities, DataSource.ATSPM),
            [DataSource.PeMS]: getFirstVersion(entities, DataSource.PeMS),
            [DataSource.ClearGuide]: getFirstVersion(
              entities,
              DataSource.ClearGuide
            ),
          },
        }),

      setAssociatedEntityIds: (ids) => set({ associatedEntityIds: ids }),

      setSelectedEntityVersions: (source, versions) =>
        set((state) => ({
          selectedEntityVersions: {
            ...state.selectedEntityVersions,
            [source]: versions,
          },
        })),

      toggleEntitySelection: (entityId) =>
        set((state) => {
          const isSelected = state.associatedEntityIds.includes(entityId)
          const newSelectedEntityIds = isSelected
            ? state.associatedEntityIds.filter((id) => id !== entityId)
            : [...state.associatedEntityIds, entityId]
          return {
            associatedEntityIds: newSelectedEntityIds,
            segmentProperties: {
              ...state.segmentProperties,
              entities: newSelectedEntityIds,
            },
          }
        }),

      setIsLoadingEntities: (isLoading) =>
        set({ isLoadingEntities: isLoading }),

      setActiveTab: (tab) => set({ activeTab: tab }),

      setUdotRoutes: (routes) => set({ udotRoutes: routes }),

      setIsLoadingUdotRoutes: (isLoading) =>
        set({ isLoadingUdotRoutes: isLoading }),

      setMapCenter: (center) => set({ mapCenter: center }),
      upsertSegment: (segment) =>
        set((state) => {
          const list = state.allSegments ? [...state.allSegments] : []
          const idx = list.findIndex((s) => s.id === segment.id)
          idx === -1 ? list.push(segment) : (list[idx] = segment)
          return { allSegments: list }
        }),

      removeSegment: (segmentId) =>
        set((state) => ({
          allSegments:
            state.allSegments?.filter((s) => s.id !== segmentId) || null,
        })),

      legendVisibility: {
        draft: true,
        lrs: true,
        existing: true,
        clearguide: true,
        'clearguide-selected': true,
        pems: true,
        'pems-selected': true,
        atspm: true,
        'atspm-selected': true,
      },

      setLegendVisibility: (key, visible) =>
        set((state) => ({
          legendVisibility: { ...state.legendVisibility, [key]: visible },
        })),

      toggleLegendVisibility: (key) =>
        set((state) => ({
          legendVisibility: {
            ...state.legendVisibility,
            [key]: !state.legendVisibility[key],
          },
        })),

      reset: () =>
        set((state) => {
          return {
            allSegments: state.allSegments,
            segmentProperties: initialSegmentProperties,
            polylineCoordinates: [],
            initialPolylineCoordinates: [],
            nearByEntities: [],
            associatedEntityIds: [],
            selectedEntityVersions: {
              [DataSource.ATSPM]: [],
              [DataSource.PeMS]: [],
              [DataSource.ClearGuide]: [],
            },
            isLoadingEntities: false,
            isLoadingSegments: false,
            polarity: 'PM',
            activeTab: 0,
            udotRoutes: state.udotRoutes,
            isLoadingUdotRoutes: state.isLoadingUdotRoutes,
            mapCenter: state.mapCenter,
            lockedRoute: null,
          }
        }),
    }),
    { name: 'SegmentEditorStore' }
  )
)
