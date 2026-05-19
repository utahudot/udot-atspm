import RouteDisplayToggle from '@/features/speedManagementTool/components/SM_Map/RouteDisplayToggle'
import { SM_Height } from '@/features/speedManagementTool/components/SM_Map/SM_MapWrapper'
import VectorRoutesSlicerLayer from '@/features/speedManagementTool/components/SM_Map/VectorSliceLayer'
import { RouteRenderOption } from '@/features/speedManagementTool/enums'
import useSpeedManagementStore from '@/features/speedManagementTool/speedManagementStore'
import { SpeedManagementRoute } from '@/features/speedManagementTool/types/routes'
import { useNotificationStore } from '@/stores/notifications'
import { getEnv } from '@/utils/getEnv'
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
import React, { memo, useEffect, useState } from 'react'
import { MapContainer, Polyline, TileLayer } from 'react-leaflet'
import HotspotMarker from './HotspotMarker'
import SpeedLegend from './SM_Legend'

type SM_MapProps = {
  fullScreenRef?: React.RefObject<HTMLDivElement> | null
  routes: SpeedManagementRoute[]
  setSelectedRouteId: (routeId: string) => void
  selectedRouteIds: string[]
  hotspots?: any
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

  const {
    routeRenderOption,
    setRouteRenderOption,
    multiselect,
    setMultiselect,
    hotspotRoutes,
    setMapRef, // from Zustand store
    mapRef, // from Zustand store
  } = useSpeedManagementStore()

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
    if (mapRef && routes.length) {
      const bounds = L.latLngBounds(
        routes.map((route) => route.geometry.coordinates).flat()
      )
      mapRef.fitBounds(bounds)
    }
  }, [routeRenderOption, mapRef, routes])

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

  const renderer = L.canvas({ tolerance: 5 }) // Increase clickability of polylines

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

  const getMidpoint = (coordinates: [number, number][]) => {
    if (!coordinates.length) return null
    const midpointIndex = Math.floor(coordinates.length / 2)
    return coordinates[midpointIndex]
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
        renderer={renderer}
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
        <RouteDisplayToggle />
        {hotspotRoutes?.map((hotspot, index) => {
          if (hotspot.geometry.geometries) {
            return hotspot.geometry.geometries.map(
              (geometry, geometryIndex) => {
                return (
                  <Polyline
                    key={`${hotspot.properties.route_id}-i${index}-g${geometryIndex}`}
                    pathOptions={{
                      weight: getPolylineWeight(zoomLevel) + 8,
                    }}
                    opacity={0.5}
                    positions={geometry.coordinates}
                    interactive={false}
                    eventHandlers={{
                      add: (e) => e.target.bringToBack(),
                    }}
                  />
                )
              }
            )
          }
          return null
        })}
        <VectorRoutesSlicerLayer
          routes={routes}
          selectedRouteIds={selectedRouteIds}
          setSelectedRouteId={setSelectedRouteId}
          setHoveredSegment={setHoveredSegment}
        />
        {hotspotRoutes?.map((hotspot, index) => {
          if (!hotspot?.geometry) return null
          // If hotspot has multiple geometries, get the midpoint of the middle one.
          let midpoint
          if (hotspot.geometry.geometries) {
            midpoint = getMidpoint(
              hotspot.geometry.geometries[
                Math.floor(hotspot.geometry.geometries.length / 2)
              ].coordinates
            )
          } else {
            midpoint = getMidpoint(hotspot.geometry.coordinates)
          }
          if (!midpoint) return null
          return (
            <React.Fragment key={index}>
              <HotspotMarker
                position={midpoint}
                rank={index + 1}
                segmentId={hotspot.properties.route_id}
                onClick={setSelectedRouteId}
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
