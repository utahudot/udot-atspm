import RouteDisplayToggle from '@/features/speedManagementTool/components/SM_Map/RouteDisplayToggle'
import { SM_Height } from '@/features/speedManagementTool/components/SM_Map/SM_MapWrapper'
import VectorRoutesSlicerLayer from '@/features/speedManagementTool/components/SM_Map/VectorSliceLayer'
import { RouteRenderOption } from '@/features/speedManagementTool/enums'
import useSpeedManagementStore from '@/features/speedManagementTool/speedManagementStore'
import { SpeedManagementRoute } from '@/features/speedManagementTool/types/routes'
import { useNotificationStore } from '@/stores/notifications'
import { getEnv } from '@/utils/getEnv'
import { lineString } from '@turf/helpers'
import lineOffset from '@turf/line-offset'
import PlaylistAddIcon from '@mui/icons-material/PlaylistAdd'
import {
  Box,
  Button,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableRow,
  Tooltip,
} from '@mui/material'
import L from 'leaflet'
import React, { memo, useEffect, useMemo, useState } from 'react'
import { MapContainer, Polyline, TileLayer } from 'react-leaflet'
import HotspotMarker from './HotspotMarker'
import SpeedLegend, { routeHasData } from './SM_Legend'

type HotspotCoordinate = [number, number]

type MapHotspot = {
  id?: string
  segmentId?: string
  segmentIds?: string[] | null
  hotspotSource?: 'impact' | 'monthly'
  geometry?: {
    coordinates?: HotspotCoordinate[]
    geometries?: Array<{
      coordinates?: HotspotCoordinate[]
    }>
  }
  properties?: {
    route_id?: string
    segmentId?: string
    segmentIds?: string[] | null
    name?: string
  }
}

type SM_MapProps = {
  fullScreenRef?: React.RefObject<HTMLDivElement> | null
  routes: SpeedManagementRoute[]
  setSelectedRouteId: (routeId: string) => void
  selectedRouteIds: string[]
  hotspots?: MapHotspot[]
}

const SM_Map = ({
  routes,
  setSelectedRouteId,
  selectedRouteIds = [],
}: SM_MapProps) => {
  const { addNotification } = useNotificationStore()
  const [initialLatLong, setInitialLatLong] = useState<[number, number] | null>(
    null
  )
  const [zoomLevel, setZoomLevel] = useState(7)
  const [hoveredSegment, setHoveredSegment] =
    useState<SpeedManagementRoute | null>(null)
  const [includeNoDataSegments, setIncludeNoDataSegments] = useState(true)

  const {
    routeRenderOption,
    setRouteRenderOption,
    multiselect,
    setMultiselect,
    hotspotRoutes,
    setMapRef, // from Zustand store
    mapRef, // from Zustand store
  } = useSpeedManagementStore()

  const visibleRoutes = useMemo(
    () =>
      includeNoDataSegments
        ? routes
        : routes.filter((route) => routeHasData(route.properties)),
    [includeNoDataSegments, routes]
  )

  useEffect(() => {
    const fetchEnv = async () => {
      const env = await getEnv()
      setInitialLatLong([
        parseFloat(env.MAP_DEFAULT_LATITUDE),
        parseFloat(env.MAP_DEFAULT_LONGITUDE),
      ])
    }
    fetchEnv()
  }, [])

  useEffect(() => {
    // when routeRenderOption changes, set map extent to show all routes
    if (mapRef && visibleRoutes.length) {
      const bounds = L.latLngBounds(
        visibleRoutes.map((route) => route.geometry.coordinates).flat()
      )
      mapRef.fitBounds(bounds)
    }
  }, [routeRenderOption, mapRef, visibleRoutes])

  useEffect(() => {
    if (!includeNoDataSegments && !routeHasData(hoveredSegment?.properties)) {
      setHoveredSegment(null)
    }
  }, [includeNoDataSegments, hoveredSegment])

  useEffect(() => {
    if (mapRef) {
      mapRef.on('zoomend', () => {
        setZoomLevel(mapRef.getZoom())
      })
    }
  }, [mapRef])

  useEffect(() => {
    if (mapRef) {
      const mapContainer = mapRef.getContainer()
      const handleResize = () => {
        mapRef.invalidateSize()
      }
      const resizeObserver = new ResizeObserver(handleResize)
      resizeObserver.observe(mapContainer)
      return () => {
        resizeObserver.disconnect()
      }
    }
  }, [mapRef])

  const toggleMultiselect = () => {
    if (!multiselect && routeRenderOption === RouteRenderOption.Posted_Speed) {
      setRouteRenderOption(RouteRenderOption.Average_Speed)
      addNotification({
        title: 'Switched to Average Speed to allow multiselect',
        type: 'info',
      })
    }
    setMultiselect(!multiselect)
  }

  if (!initialLatLong) {
    return <Box p={2}>Loading...</Box>
  }

  const getPolylineWeight = (zoom: number) => {
    if (zoom >= 18) return 10
    if (zoom >= 15) return 5
    if (zoom >= 12) return 4
    if (zoom >= 10) return 3
    if (zoom >= 8) return 2
    return 2
  }

  const hasDir = (name: string | undefined, dir: 'nb' | 'sb' | 'eb' | 'wb') =>
    Boolean(name?.toLowerCase().includes(dir))

  const offsetMetersForZoom = (zoom: number) => {
    if (zoom >= 14) return 10
    if (zoom >= 13) return 30
    if (zoom >= 12) return 70
    if (zoom >= 10) return 100
    if (zoom >= 8) return 0
    return 0
  }

  const getSignedOffsetMeters = (name?: string) => {
    const meters = offsetMetersForZoom(zoomLevel)
    if (!meters) return 0
    if (hasDir(name, 'nb') || hasDir(name, 'eb')) return meters
    if (hasDir(name, 'sb') || hasDir(name, 'wb')) return -meters
    return 0
  }

  const isCoordinatePair = (value: unknown): value is number[] =>
    Array.isArray(value) &&
    value.length >= 2 &&
    typeof value[0] === 'number' &&
    typeof value[1] === 'number'

  const cleanLine = (coordinates?: HotspotCoordinate[]) =>
    Array.isArray(coordinates)
      ? coordinates.filter(isCoordinatePair).map(([lat, lng]) => [lat, lng])
      : []

  const toLngLat = ([lat, lng]: number[]) => [lng, lat]
  const toLatLng = ([lng, lat]: number[]) => [lat, lng] as HotspotCoordinate

  const getHotspotRoute = (hotspot: MapHotspot, geometryIndex?: number) => {
    const segmentIds = getHotspotSegmentIds(hotspot)
    const routeId =
      typeof geometryIndex === 'number' && segmentIds?.[geometryIndex]
        ? segmentIds[geometryIndex]
        : getSelectableHotspotRouteId(hotspot)

    return routes.find((route) => route.properties.route_id === routeId)
  }

  const getHotspotOffsetName = (
    hotspot: MapHotspot,
    geometryIndex?: number
  ) => getHotspotRoute(hotspot, geometryIndex)?.properties?.name ?? hotspot.properties?.name

  const getOffsetHotspotCoordinates = (
    hotspot: MapHotspot,
    coordinates?: HotspotCoordinate[],
    geometryIndex?: number
  ): HotspotCoordinate[] => {
    const cleanCoordinates = cleanLine(coordinates) as HotspotCoordinate[]
    if (cleanCoordinates.length < 2) return cleanCoordinates

    const meters = getSignedOffsetMeters(
      getHotspotOffsetName(hotspot, geometryIndex)
    )
    if (!meters) return cleanCoordinates

    try {
      const offsetLine = lineOffset(
        lineString(cleanCoordinates.map(toLngLat)),
        meters,
        { units: 'meters' }
      )

      return offsetLine.geometry.coordinates
        .filter(isCoordinatePair)
        .map(toLatLng)
    } catch {
      return cleanCoordinates
    }
  }

  const makeHotspotPathPassive = (layer: L.Polyline) => {
    const element = layer.getElement()
    if (element) {
      element.style.pointerEvents = 'none'
    }
    layer.bringToFront()
  }

  const getMidpoint = (coordinates: [number, number][]) => {
    if (!coordinates.length) return null
    const midpointIndex = Math.floor(coordinates.length / 2)
    return coordinates[midpointIndex]
  }

  const getHotspotMarkerId = (hotspot: MapHotspot) =>
    getSelectableHotspotRouteId(hotspot) ?? hotspot?.id ?? null

  const getHotspotSegmentIds = (hotspot: MapHotspot) =>
    hotspot?.properties?.segmentIds ?? hotspot?.segmentIds ?? null

  const isImpactHotspot = (hotspot: MapHotspot) =>
    hotspot.hotspotSource === 'impact'

  const getSelectableHotspotRouteId = (hotspot: MapHotspot) => {
    const segmentIds = getHotspotSegmentIds(hotspot)
    if (hotspot?.properties?.route_id) return hotspot.properties.route_id
    if (hotspot?.properties?.segmentId) return hotspot.properties.segmentId
    if (hotspot?.segmentId) return hotspot.segmentId
    if (segmentIds?.length) {
      return segmentIds[Math.floor(segmentIds.length / 2)]
    }
    return null
  }

  return (
    <Box
      sx={{
        height: '100%',
        width: '100%',
        position: 'relative',
        borderBottom: '1px solid',
        borderColor: 'divider',
      }}
    >
      <MapContainer
        center={initialLatLong}
        zoom={zoomLevel}
        scrollWheelZoom={true}
        style={{
          minHeight: SM_Height,
          height: '100%',
          width: '100%',
          zIndex: 0,
        }}
        ref={setMapRef}
        doubleClickZoom={false}
      >
        <TileLayer
          attribution='&copy; <a href="https://www.openaip.net/">openAIP Data</a> (<a href="https://creativecommons.org/licenses/by-sa/3.0/">CC-BY-NC-SA</a>)'
          url="https://tiles.stadiamaps.com/tiles/alidade_smooth/{z}/{x}/{y}{r}.png"
        />
        <Box
          sx={{
            position: 'absolute',
            right: '10px',
            top: '10px',
            zIndex: 1000,
          }}
        >
          <Tooltip title="Select Multiple Segments">
            <Button
              sx={{
                px: 1,
                minWidth: 0,
                textTransform: 'none',
              }}
              variant="contained"
              size="small"
              onClick={toggleMultiselect}
            >
              Multiselect &nbsp; <PlaylistAddIcon />
            </Button>
          </Tooltip>
        </Box>
        <RouteDisplayToggle
          includeNoDataSegments={includeNoDataSegments}
          setIncludeNoDataSegments={setIncludeNoDataSegments}
        />
        <VectorRoutesSlicerLayer
          routes={visibleRoutes}
          selectedRouteIds={selectedRouteIds}
          setSelectedRouteId={setSelectedRouteId}
          setHoveredSegment={setHoveredSegment}
        />
        {hotspotRoutes?.map((hotspot: MapHotspot, index) => {
          const hotspotMarkerId = getHotspotMarkerId(hotspot)

          if (hotspot.geometry?.geometries) {
            return hotspot.geometry.geometries.map(
              (geometry, geometryIndex) => {
                const positions = getOffsetHotspotCoordinates(
                  hotspot,
                  geometry.coordinates,
                  geometryIndex
                )
                if (positions.length < 2) return null

                return (
                  <Polyline
                    key={`${hotspotMarkerId || 'hotspot'}-i${index}-g${geometryIndex}`}
                    pathOptions={{
                      weight: getPolylineWeight(zoomLevel) + 8,
                      opacity: 0.45,
                    }}
                    positions={positions}
                    interactive={false}
                    eventHandlers={{
                      add: (e) => makeHotspotPathPassive(e.target),
                    }}
                  />
                )
              }
            )
          }

          if (hotspot.geometry?.coordinates) {
            const positions = getOffsetHotspotCoordinates(
              hotspot,
              hotspot.geometry.coordinates
            )
            if (positions.length < 2) return null

            return (
              <Polyline
                key={`${hotspotMarkerId || 'hotspot'}-i${index}`}
                pathOptions={{
                  weight: getPolylineWeight(zoomLevel) + 8,
                  opacity: 0.5,
                }}
                positions={positions}
                interactive={false}
                eventHandlers={{
                  add: (e) => makeHotspotPathPassive(e.target),
                }}
              />
            )
          }

          return null
        })}
        {hotspotRoutes?.map((hotspot: MapHotspot, index) => {
          if (!hotspot?.geometry) return null
          const hotspotMarkerId = getHotspotMarkerId(hotspot)
          if (!hotspotMarkerId) return null
          const selectableRouteId = getSelectableHotspotRouteId(hotspot)
          const markerSelectableRouteId = isImpactHotspot(hotspot)
            ? null
            : selectableRouteId

          // If hotspot has multiple geometries, get the midpoint of the middle one.
          let midpoint
          if (hotspot.geometry.geometries) {
            const midpointGeometryIndex = Math.floor(
              hotspot.geometry.geometries.length / 2
            )
            midpoint = getMidpoint(
              getOffsetHotspotCoordinates(
                hotspot,
                hotspot.geometry.geometries[midpointGeometryIndex]
                  .coordinates || [],
                midpointGeometryIndex
              )
            )
          } else {
            midpoint = getMidpoint(
              getOffsetHotspotCoordinates(
                hotspot,
                hotspot.geometry.coordinates || []
              )
            )
          }
          if (!midpoint) return null
          return (
            <React.Fragment key={`${hotspotMarkerId}-${index}`}>
              <HotspotMarker
                position={midpoint}
                rank={index + 1}
                segmentId={hotspotMarkerId}
                onClick={
                  markerSelectableRouteId
                    ? () => setSelectedRouteId(markerSelectableRouteId)
                    : undefined
                }
              />
            </React.Fragment>
          )
        })}
        <SpeedLegend />
      </MapContainer>
      {hoveredSegment && (
        <Paper
          elevation={3}
          sx={{
            position: 'absolute',
            bottom: '10px',
            left: '10px',
            padding: 0,
            backgroundColor: 'rgba(255, 255, 255, 0.9)',
            zIndex: 1000,
            width: '250px',
          }}
        >
          <TableContainer>
            <Table size="small">
              <TableBody>
                {hoveredSegment.properties.name && (
                  <TableRow>
                    <TableCell sx={{ fontWeight: 'bold', fontSize: '12px' }}>
                      Segment ID:
                    </TableCell>
                    <TableCell
                      align="right"
                      sx={{
                        fontSize: '12px',
                        whiteSpace: 'nowrap',
                      }}
                    >
                      {hoveredSegment.properties.name}
                    </TableCell>
                  </TableRow>
                )}
                <TableRow>
                  <TableCell sx={{ fontWeight: 'bold', fontSize: '12px' }}>
                    Speed Limit:
                  </TableCell>
                  <TableCell align="right" sx={{ fontSize: '12px' }}>
                    {hoveredSegment.properties.speedLimit ?? 'Unknown'} mph
                  </TableCell>
                </TableRow>
                {hoveredSegment.properties.averageSpeed && (
                  <TableRow>
                    <TableCell sx={{ fontWeight: 'bold', fontSize: '12px' }}>
                      Avg Speed:
                    </TableCell>
                    <TableCell align="right" sx={{ fontSize: '12px' }}>
                      {Math.round(hoveredSegment.properties.averageSpeed)} mph
                    </TableCell>
                  </TableRow>
                )}
                {hoveredSegment.properties.averageEightyFifthSpeed && (
                  <TableRow>
                    <TableCell
                      sx={{
                        fontWeight: 'bold',
                        fontSize: '12px',
                        borderBottom: 'none',
                      }}
                    >
                      85th Percentile:
                    </TableCell>
                    <TableCell
                      align="right"
                      sx={{ fontSize: '12px', borderBottom: 'none' }}
                    >
                      {Math.round(
                        hoveredSegment.properties.averageEightyFifthSpeed
                      )}{' '}
                      mph
                    </TableCell>
                  </TableRow>
                )}
              </TableBody>
            </Table>
          </TableContainer>
        </Paper>
      )}
    </Box>
  )
}

export default memo(SM_Map)
