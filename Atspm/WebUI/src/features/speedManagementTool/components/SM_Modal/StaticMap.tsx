import { SpeedManagementRoute } from '@/features/speedManagementTool/types/routes'
import { Box } from '@mui/material'
import L, { Map as LeafletMap } from 'leaflet'
import { memo, useEffect, useState } from 'react'
import { CircleMarker, MapContainer, Polyline, TileLayer } from 'react-leaflet'

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
        trackResize={false}
        touchZoom={false}
        scrollWheelZoom={false}
      >
        <TileLayer
          attribution='&copy; <a href="https://www.openaip.net/">openAIP Data</a> (<a href="https://creativecommons.org/licenses/by-sa/3.0/">CC-BY-NC-SA</a>)'
          url="https://tiles.stadiamaps.com/tiles/alidade_bright/{z}/{x}/{y}{r}.png"
        />
        {routes.map((route, index) => (
          <>
            {/* Outer Border Polyline (acts like a polygon border) */}
            <Polyline
              key={`border-${index}`}
              positions={route.geometry.coordinates}
              weight={9} // Border thickness
              color="#1859b5" // Border color
              interactive={false}
              lineCap="round"
              lineJoin="round"
            />

            {/* Actual Polyline */}
            <Polyline
              key={`route-${index}`}
              positions={route.geometry.coordinates}
              weight={5}
              interactive={false}
              lineCap="square"
              lineJoin="round"
            />
            {/* Start Point Marker */}
            <CircleMarker
              center={route.geometry.coordinates[0]}
              radius={5}
              fillOpacity={1}
              key={`start-marker-${index}`}
              color="#1859b5"
            />

            {/* End Point Marker */}
            <CircleMarker
              fillOpacity={1}
              radius={5}
              key={`end-marker-${index}`}
              color="#1859b5"
              center={
                route.geometry.coordinates[
                  route.geometry.coordinates.length - 1
                ]
              }
            />
          </>
        ))}
      </MapContainer>
    </Box>
  )
}

export default memo(StaticMap)
