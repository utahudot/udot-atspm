import { SpeedManagementRoute } from '@/features/speedManagementTool/types/routes'
import { getEnv } from '@/utils/getEnv'
import { Box } from '@mui/material'
import L, { Map as LeafletMap } from 'leaflet'
import React, { memo, useEffect, useState } from 'react'
import { MapContainer, Polyline, TileLayer } from 'react-leaflet'
import SM_Legend, { getRouteColor } from '../../../SM_Map/SM_Legend'
import { HotSpotForReportMap, ImpactHotspotForReportMap } from '../../types'
import MarkerForReportMap from './MarkerForReportMap'

interface Props {
  routes: SpeedManagementRoute[]
  hotspots?: HotSpotForReportMap[]
  impacts?: ImpactHotspotForReportMap[]
}

function ReportMap(props: Props) {
  const { routes, hotspots, impacts } = props
  const [mapRef, setMapRef] = useState<LeafletMap | null>(null)
  const [initialLatLong, setInitialLatLong] = useState<[number, number] | null>(
    null
  )
  const [zoomLevel, setZoomLevel] = useState(6)

  useEffect(() => {
    const fetchEnv = async () => {
      const env = await getEnv()
      if (env) {
        setInitialLatLong([
          parseFloat(env.MAP_DEFAULT_LATITUDE),
          parseFloat(env.MAP_DEFAULT_LONGITUDE),
        ])
      }
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
  }, [mapRef, routes])

  useEffect(() => {
    if (mapRef) {
      // Set zoom level on zoom end
      mapRef.on('zoomend', () => {
        setZoomLevel(mapRef.getZoom())
      })

      // Handle resize
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

  useEffect(() => {
    if (mapRef && routes.length > 0) {
      const bounds = L.latLngBounds(
        routes.flatMap((route) =>
          route.geometry.coordinates.map((coord: [number, number]) =>
            L.latLng(coord[0], coord[1])
          )
        )
      )
      mapRef.fitBounds(bounds, { padding: [20, 20] }) // Add padding if needed
    }
  }, [mapRef, routes])

  const getColor = (route: SpeedManagementRoute) => {
    const field = 'averageSpeed'
    const val = route.properties[
      field as keyof SpeedManagementRoute['properties']
    ] as number
    if (val === null) return '#000'
    return getRouteColor(val)
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

  const renderer = L.canvas({ tolerance: 5 })

  if (!initialLatLong) {
    return <Box p={2}>Loading...</Box>
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
        className="ReportMapContainer"
        center={initialLatLong}
        zoom={zoomLevel}
        style={{
          height: '100%',
          width: '100%',
          zIndex: 0,
        }}
        ref={setMapRef}
        renderer={renderer}
        doubleClickZoom={false}
        preferCanvas={true}
      >
        <TileLayer
          id="ReportMapId"
          attribution='&copy; <a href="https://www.openaip.net/">openAIP Data</a> (<a href="https://creativecommons.org/licenses/by-sa/3.0/">CC-BY-NC-SA</a>)'
          url="https://server.arcgisonline.com/ArcGIS/rest/services/World_Topo_Map/MapServer/tile/{z}/{y}/{x}"
        />
        {routes.map((route, i) => (
          <Polyline
            key={`${route.properties.route_id}i${i}`}
            pathOptions={{
              color: getColor(route),
              weight: getPolylineWeight(zoomLevel),
            }}
            smoothFactor={0}
            positions={route.geometry.coordinates}
            interactive={true}
          />
        ))}
        {hotspots?.map((hotspot, index) => {
          if (!hotspot?.coordinates) return null
          const midpoint = getMidpoint(hotspot.coordinates)

          if (!midpoint) return null
          return (
            <React.Fragment key={index}>
              <MarkerForReportMap
                position={midpoint}
                rank={index + 1}
                segmentId={hotspot.segmentId}
              />
            </React.Fragment>
          )
        })}
        {impacts?.map((impact, index) => {
          if (impact.impactedSegments) {
            return impact.impactedSegments.map((segment, geometryIndex) => {
              return (
                <Polyline
                  key={`${segment.segmentId}-i${index}-g${geometryIndex}`}
                  pathOptions={{
                    weight: getPolylineWeight(zoomLevel) + 8,
                  }}
                  opacity={0.5}
                  positions={segment.coordinates}
                  interactive={false}
                  eventHandlers={{
                    add: (e) => e.target.bringToBack(),
                  }}
                />
              )
            })
          }
          return null
        })}
        <SM_Legend />
      </MapContainer>
    </Box>
  )
}

export default memo(ReportMap)
