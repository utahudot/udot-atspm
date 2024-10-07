import HotspotMarker from '@/features/speedManagementTool/components/SM_Map/HotspotMarker'
import RouteDisplayToggle from '@/features/speedManagementTool/components/SM_Map/RouteDisplayToggle'
import { SM_Height } from '@/features/speedManagementTool/components/SM_Map/SM_MapWrapper'
import { RouteRenderOption } from '@/features/speedManagementTool/enums'
import useSpeedManagementStore from '@/features/speedManagementTool/speedManagementStore'
import { SpeedManagementRoute } from '@/features/speedManagementTool/types/routes'
import { ViolationColors } from '@/features/speedManagementTool/utils/colors'
import { getEnv } from '@/lib/getEnv'
import PlaylistAddIcon from '@mui/icons-material/PlaylistAdd'
import { Box, Button, Tooltip } from '@mui/material'
import L, { Map as LeafletMap } from 'leaflet'
import React, { memo, useEffect, useState } from 'react'
import { MapContainer, Polyline, TileLayer } from 'react-leaflet'
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
  const [mapRef, setMapRef] = useState<LeafletMap | null>(null)
  const [initialLatLong, setInitialLatLong] = useState<[number, number] | null>(
    null
  )
  const [zoomLevel, setZoomLevel] = useState(6)

  const {
    routeRenderOption,
    mediumMin,
    mediumMax,
    multiselect,
    setMultiselect,
    hotspotRoutes,
    hoveredHotspot,
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
    if (mapRef) {
      mapRef.on('zoomend', () => {
        setZoomLevel(mapRef.getZoom())
      })
    }
  }, [mapRef])

  const getColor = (route: SpeedManagementRoute) => {
    let field
    switch (routeRenderOption) {
      case RouteRenderOption.Violations:
        field = 'violations'
        break
      case RouteRenderOption.Posted_Speed:
        field = 'speedLimit'
        break
      case RouteRenderOption.Average_Speed:
        field = 'averageSpeed'
        break
      case RouteRenderOption.Percentile_85th:
        field = 'averageEightyFifthSpeed'
        break
      default:
        field = 'averageSpeed'
        break
    }

    const val = route.properties[
      field as keyof SpeedManagementRoute['properties']
    ] as number

    if (routeRenderOption === RouteRenderOption.Violations) {
      if (val <= mediumMin) return ViolationColors.Low
      if (val < mediumMax) return ViolationColors.Medium
      return ViolationColors.High
    }

    if (val === null) return '#000'

    if (val < 20) return 'rgba(0, 115, 255, 1)'
    if (val < 30) return 'rgba(0, 255, 170, 1)'
    if (val < 35) return 'rgba(55, 255, 0, 1)'
    if (val < 40) return 'rgba(175, 250, 0, 1)'
    if (val < 45) return 'rgba(247, 214, 0, 1)'
    if (val < 55) return 'rgba(245, 114, 0, 1)'
    if (val < 65) return 'rgba(245, 57, 0, 1)'
    if (val < 75) return 'rgba(245, 0, 0, 1)'
    if (val >= 75) return 'rgba(115, 0, 0, 1)'
    return '#000'
  }

  const renderer = L.canvas({ tolerance: 5 }) // Increase clickability of polylines

  const toggleMultiselect = () => {
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
          url="https://tiles.stadiamaps.com/tiles/alidade_bright/{z}/{x}/{y}{r}.png"
        />
        <Box
          sx={{
            position: 'absolute',
            right: '10px',
            top: '10px',
            zIndex: 1000,
          }}
        >
          <Tooltip title="Select Multiple Routes">
            <Button
              sx={{
                px: 1,
                minWidth: 0,
              }}
              variant="contained"
              onClick={toggleMultiselect}
            >
              <PlaylistAddIcon />
            </Button>
          </Tooltip>
        </Box>
        <RouteDisplayToggle />
        {routes.map((route, index) => (
          <Polyline
            key={index}
            pathOptions={{
              color: selectedRouteIds.includes(route.properties.route_id)
                ? 'blue'
                : getColor(route),
              weight: getPolylineWeight(zoomLevel),
            }}
            smoothFactor={0}
            positions={route.geometry.coordinates}
            eventHandlers={{
              click: () => setSelectedRouteId(route.properties.route_id),
              mouseover: (e) => {
                e.target.setStyle({
                  weight: getPolylineWeight(zoomLevel) + 3,
                  color: 'blue',
                })
              },
              mouseout: (e) => {
                e.target.setStyle({
                  weight: getPolylineWeight(zoomLevel),
                  color: selectedRouteIds.includes(route.properties.route_id)
                    ? 'blue'
                    : getColor(route),
                })
              },
            }}
          />
        ))}
        {hotspotRoutes?.map((hotspot, index) => {
          const midpoint = getMidpoint(hotspot.geometry.coordinates)

          if (!midpoint) return null

          return (
            <React.Fragment key={hotspot.properties.route_id}>
              <HotspotMarker
                position={midpoint}
                rank={index + 1}
                segmentId={hotspot.properties.route_id}
                onClick={setSelectedRouteId}
                hoveredHotspot={hoveredHotspot}
              />
            </React.Fragment>
          )
        })}
        <SpeedLegend />
      </MapContainer>
    </Box>
  )
}

export default memo(SM_Map)
