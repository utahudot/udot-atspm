import { SpeedManagementRoute } from '@/features/speedManagementTool/types/routes'
import { Box } from '@mui/material'
import L, { Map as LeafletMap } from 'leaflet'
import { memo, useEffect, useState } from 'react'
import { MapContainer, Polyline, TileLayer } from 'react-leaflet'

type StaticMapProps = {
  routes: SpeedManagementRoute[] // Array of routes
}

const StaticMap = ({ routes }: StaticMapProps) => {
  const [mapRef, setMapRef] = useState<LeafletMap | null>(null)

  // Extract all coordinates from routes
  const allCoordinates = routes.flatMap((route) => route.geometry.coordinates)

  // Calculate bounds from the coordinates
  const bounds = L.latLngBounds(
    allCoordinates.map((coord) => [coord[0], coord[1]])
  )

  useEffect(() => {
    if (mapRef && bounds.isValid()) {
      mapRef.fitBounds(bounds)
    }
  }, [mapRef, bounds])

  if (!routes[0]?.geometry) return null

  return (
    <Box sx={{ height: '100%', width: '100%' }}>
      <MapContainer
        style={{
          minHeight: '400px',
          height: '100%',
          width: '100%',
          zIndex: 0,
        }}
        ref={setMapRef}
        zoomControl={false}
        doubleClickZoom={false}
        closePopupOnClick={false}
        dragging={false}
        // zoomSnap={false}
        // zoomDelta={false}
        trackResize={false}
        touchZoom={false}
        scrollWheelZoom={false}
      >
        <TileLayer
          attribution='&copy; <a href="https://www.openaip.net/">openAIP Data</a> (<a href="https://creativecommons.org/licenses/by-sa/3.0/">CC-BY-NC-SA</a>)'
          url="https://tiles.stadiamaps.com/tiles/stamen-terrain/{z}/{x}/{y}{r}.png"
        />
        {routes.map((route, index) => (
          <Polyline
            key={index}
            positions={route.geometry.coordinates}
            weight={5}
            interactive={false}
          />
        ))}
      </MapContainer>
    </Box>
  )
}

export default memo(StaticMap)
