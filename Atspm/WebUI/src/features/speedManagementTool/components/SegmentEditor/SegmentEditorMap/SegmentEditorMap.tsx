import RouteModeIndicator from '@/features/speedManagementTool/components/SegmentEditor/SegmentEditorMap/RouteModeIndicator'
import { useMapSetup } from '@/features/speedManagementTool/components/SegmentEditor/SegmentEditorMap/hooks/useMapSetup'
import DisplayNearBySegments from '@/features/speedManagementTool/components/SegmentEditor/SegmentEditorMap/mapFeatures/DisplayNearBySegments'
import DraftSegment from '@/features/speedManagementTool/components/SegmentEditor/SegmentEditorMap/mapFeatures/DraftSegment'
import {
  atspmIcon,
  atspmSelectedIcon,
  pemsIcon,
  pemsSelectedIcon,
} from '@/features/speedManagementTool/components/SegmentEditor/SegmentEditorMap/mapFeatures/EntityMarker'
import Legend from '@/features/speedManagementTool/components/SegmentEditor/SegmentEditorMap/mapFeatures/Legend'
import SegmentClickErrorMarker from '@/features/speedManagementTool/components/SegmentEditor/SegmentEditorMap/mapFeatures/SegmentClickErrorMarker'
import { UdotLrsSegments } from '@/features/speedManagementTool/components/SegmentEditor/SegmentEditorMap/mapFeatures/UdotLrsSegments'
import { ROUTE_COLORS } from '@/features/speedManagementTool/components/SegmentEditor/SegmentEditorMap/utils/colors'
import CircularProgress from '@mui/material/CircularProgress'

import EntityClusterGroup from '@/features/speedManagementTool/components/SegmentEditor/SegmentEditorMap/mapFeatures/EntityClusterGroup'
import HoveredEntity from '@/features/speedManagementTool/components/SegmentEditor/SegmentEditorMap/mapFeatures/HoveredEntity'
import {
  Entity,
  Segment,
  useSegmentEditorStore,
} from '@/features/speedManagementTool/components/SegmentEditor/segmentEditorStore'
import { DataSource } from '@/features/speedManagementTool/enums'
import { Box } from '@mui/material'
import 'leaflet/dist/leaflet.css'
import { useCallback, useMemo, useState } from 'react'
import { MapContainer, Pane, TileLayer } from 'react-leaflet'
import { RouteSelectionDialog } from './RouteSelectionDialog'
import { Feature, useMapClickHandler } from './hooks/useMapClickHandler'
import { useMilePointCalculator } from './hooks/useMilePointCalculator'
import { useUdotRoutes } from './hooks/useUdotRoutes'

const SegmentEditorMap = () => {
  const {
    polylineCoordinates,
    nearByEntities,
    activeTab,
    selectedEntityVersions,
    associatedEntityIds,
    addPolylineCoordinate,
    removePolylineCoordinate,
    updatePolylineCoordinate,
    setPolylineCoordinates,
    lockedRoute,
    setLockedRoute,
    setAssociatedEntityIds,
    allSegments,
    isLoadingEntities,
    initialPolylineCoordinates,
  } = useSegmentEditorStore()

  const [mapRef, setMapRef] = useState<import('leaflet').Map | null>(null)
  const [segmentRouteId, setSegmentRouteId] = useState<string | null>(null)
  const [pendingPoint, setPendingPoint] = useState<[number, number] | null>(
    null
  )
  const [routeOptions, setRouteOptions] = useState<any[]>([])
  const [isRouteDialogOpen, setIsRouteDialogOpen] = useState(false)
  const [routeErrorPos, setRouteErrorPos] = useState<[number, number] | null>(
    null
  )
  const [hoveredEntity, setHoveredEntity] = useState<Entity | null>(null)
  const [hoveredSegment, setHoveredSegment] = useState<Segment | null>(null)

  const canEditSegmentLines = activeTab === 0

  useMapSetup(mapRef, polylineCoordinates)

  const handleRouteError = useCallback((pos: [number, number] | null) => {
    setRouteErrorPos(pos)
  }, [])

  const initialPolyline = useMemo(() => {
    return (
      JSON.stringify(initialPolylineCoordinates) ===
      JSON.stringify(polylineCoordinates)
    )
  }, [initialPolylineCoordinates, polylineCoordinates])

  const { udotRoutes } = useUdotRoutes()
  const { calculateMilePoint } = useMilePointCalculator()
  const { handleRouteClick } = useMapClickHandler(
    mapRef,
    segmentRouteId,
    setSegmentRouteId,
    setPendingPoint,
    setRouteOptions,
    setIsRouteDialogOpen,
    calculateMilePoint,
    handleRouteError,
    canEditSegmentLines,
    initialPolyline
  )

  const filteredEntities = useMemo(() => {
    return nearByEntities.filter((entity) => {
      if (associatedEntityIds.includes(entity.id)) return true
      if (
        ![DataSource.ATSPM, DataSource.PeMS, DataSource.ClearGuide].includes(
          entity.sourceId
        )
      ) {
        return false
      }
      const selected = selectedEntityVersions[entity.sourceId]
      if (selected.length === 0) return false
      return selected.includes(entity.version)
    })
  }, [nearByEntities, selectedEntityVersions, associatedEntityIds])

  const handleRouteSelection = useCallback(
    async (feature: Feature | null) => {
      setIsRouteDialogOpen(false)

      if (!pendingPoint) return

      if (feature) {
        const pt = pendingPoint
        handleRouteClick(feature, pt)
        setLockedRoute(feature)
      } else {
        addPolylineCoordinate(pendingPoint)
      }

      clearPoint()
    },
    [
      pendingPoint,
      setIsRouteDialogOpen,
      handleRouteClick,
      addPolylineCoordinate,
      setLockedRoute,
    ]
  )

  const clearPoint = () => {
    setIsRouteDialogOpen(false)
    setPendingPoint(null)
    setRouteOptions([])
    setRouteErrorPos(null)
  }

  const clearRoute = () => {
    setSegmentRouteId(null)
    setPolylineCoordinates(initialPolylineCoordinates)
    setRouteErrorPos(null)
    setAssociatedEntityIds([])
    setRouteOptions([])
    setLockedRoute(null)
  }

  const getLegendSources = useCallback(() => {
    const sources = [
      {
        id: 'draft',
        label: 'Active Segment',
        color: '#3388ff',
        width: 5,
        shape: 'line',
        isSolid: true,
      },
    ]

    if (activeTab === 0) {
      sources.push(
        {
          id: 'lrs',
          label: 'LRS Routes',
          color: ROUTE_COLORS.Lrs.main,
          width: 3,
          shape: 'line',
          isSolid: false,
        },
        {
          id: 'existing',
          label: 'Existing Segments',
          color: ROUTE_COLORS.Nearby.main,
          width: 7,
          shape: 'line',
          isSolid: true,
        }
      )
    } else {
      sources.push(
        {
          id: 'clearguide',
          label: 'ClearGuide Source',
          color: ROUTE_COLORS.ClearGuide.main,
          width: 5,
          shape: 'line',
          isSolid: true,
        },
        {
          id: 'clearguide-selected',
          label: 'ClearGuide Sources (Selected)',
          color: ROUTE_COLORS.ClearGuide.hover,
          width: 5,
          shape: 'line',
          isSolid: true,
        },
        {
          id: 'pems',
          label: 'PeMS Source',
          renderer: () => (
            <Box
              component="img"
              src={pemsIcon.options.iconUrl}
              sx={{ width: 16, height: 16, mr: 1 }}
            />
          ),
        },
        {
          id: 'pems-selected',
          label: 'PeMS Source (Selected)',
          renderer: () => (
            <Box
              component="img"
              src={pemsSelectedIcon.options.iconUrl}
              sx={{ width: 16, height: 16, mr: 1 }}
            />
          ),
        },
        {
          id: 'atspm',
          label: 'ATSPM Source',
          renderer: () => (
            <Box
              component="img"
              src={atspmIcon.options.iconUrl}
              sx={{ width: 16, height: 16, mr: 1 }}
            />
          ),
        },
        {
          id: 'atspm-selected',
          label: 'ATSPM Source (Selected)',
          renderer: () => (
            <Box
              component="img"
              src={atspmSelectedIcon.options.iconUrl}
              sx={{ width: 16, height: 16, mr: 1 }}
            />
          ),
        }
      )
    }
    return sources
  }, [activeTab])

  return (
    <Box sx={{ height: '100%', width: '100%', position: 'relative' }}>
      {isLoadingEntities && activeTab === 1 && (
        <Box
          sx={{
            position: 'absolute',
            inset: 0,
            bgcolor: 'rgba(0,0,0,0.3)',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            zIndex: 9999,
          }}
        >
          <CircularProgress />
        </Box>
      )}
      <MapContainer
        style={{ minHeight: '400px', height: '100%' }}
        ref={setMapRef}
        zoomControl
        doubleClickZoom={false}
        dragging
        trackResize
        touchZoom={false}
        scrollWheelZoom
      >
        <TileLayer
          attribution='&copy; <a href="https://www.openaip.net/">openAIP Data</a> (<a href="https://creativecommons.org/licenses/by-sa/3.0/">CC-BY-NC-SA</a>)'
          url="https://tiles.stadiamaps.com/tiles/alidade_smooth/{z}/{x}/{y}{r}.png"
        />

        {allSegments && activeTab === 0 && (
          <Pane name="display-nearby-segments" style={{ zIndex: 2000 }}>
            <DisplayNearBySegments setHoveredSegment={setHoveredSegment} />
          </Pane>
        )}

        {activeTab === 0 && (
          <Pane name="udot-lrs-segments" style={{ zIndex: 1000 }}>
            <UdotLrsSegments
              segments={udotRoutes}
              segmentRouteId={segmentRouteId}
            />
          </Pane>
        )}

        <Pane name="draft-segment" style={{ zIndex: 6000 }}>
          <DraftSegment
            polylineCoordinates={polylineCoordinates}
            activeTab={activeTab}
            mapRef={mapRef}
            removePolylineCoordinate={removePolylineCoordinate}
            updatePolylineCoordinate={updatePolylineCoordinate}
          />
        </Pane>

        <Pane name="segment-click-error" style={{ zIndex: 4000 }}>
          <SegmentClickErrorMarker position={routeErrorPos} />
        </Pane>
        {filteredEntities.length > 0 && activeTab !== 0 && (
          <EntityClusterGroup
            entities={filteredEntities}
            associatedEntityIds={associatedEntityIds}
            setAssociatedEntityIds={setAssociatedEntityIds}
            setHoveredEntity={setHoveredEntity}
          />
        )}
      </MapContainer>

      {activeTab === 0 && (
        <RouteModeIndicator
          lockedRoute={lockedRoute}
          onClear={clearRoute}
          polylineCoordinates={polylineCoordinates}
          inEditMode={initialPolylineCoordinates?.length > 0}
        />
      )}

      <RouteSelectionDialog
        open={isRouteDialogOpen}
        routeOptions={routeOptions}
        onSelect={handleRouteSelection}
        onClose={clearPoint}
      />

      <HoveredEntity
        hoveredEntity={hoveredEntity ? hoveredEntity : hoveredSegment}
      />

      <Legend
        sx={{
          position: 'absolute',
          bottom: 30,
          right: 10,
          zIndex: 1000,
        }}
        title="Legend"
        items={getLegendSources() || []}
      />
    </Box>
  )
}

export default SegmentEditorMap
