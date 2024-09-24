import { getEnv } from '@/lib/getEnv'
import { Box } from '@mui/material'
import L, { Map as LeafletMap } from 'leaflet'
import React, { memo, useCallback, useEffect, useMemo, useState } from 'react'
import { MapContainer, Polyline, TileLayer } from 'react-leaflet'

// ... (interfaces remain the same)

const getPolylineWeight = (zoom: number) => {
  if (zoom >= 18) return 10
  if (zoom >= 15) return 5
  if (zoom >= 12) return 4
  if (zoom >= 10) return 3
  if (zoom >= 8) return 2
  return 2
}

const SegmentPolylines = memo(function SegmentPolylines({ 
  segments, 
  selectedSegmentIds, 
  onSegmentSelect, 
  zoomLevel 
}) {
  return (
    <>
      {segments.map((segment, index) => (
        <Polyline
          key={segment.properties.Id}
          positions={segment.geometry.coordinates}
          pathOptions={{
            color: selectedSegmentIds.includes(segment.properties.Id) ? 'blue' : 'gray',
            weight: getPolylineWeight(zoomLevel),
          }}
          eventHandlers={{
            click: () => onSegmentSelect(segment.properties.Id, segment.properties.StartMilePoint, segment.properties.EndMilePoint),
          }}
        />
      ))}
    </>
  )
})

const Map: React.FC<MapProps> = memo(({
  segments,
  selectedSegmentIds,
  onSegmentSelect,
}) => {
  const [mapRef, setMapRef] = useState<LeafletMap | null>(null)
  const [initialLatLong, setInitialLatLong] = useState<[number, number] | null>(null)
  const [zoomLevel, setZoomLevel] = useState(6)

  const handlePolylineClick = useCallback(
    (id: string, startMile: number, endMile: number) => {
      onSegmentSelect(id, startMile, endMile);
    },
    [onSegmentSelect]
  );

  useEffect(() => {
    if (mapRef) {
      mapRef.on('zoomend', () => {
        setZoomLevel(mapRef.getZoom())
      })
    }
  }, [mapRef])

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

  const memoizedSegmentPolylines = useMemo(
    () => (
      <SegmentPolylines
        segments={segments}
        selectedSegmentIds={selectedSegmentIds}
        onSegmentSelect={handlePolylineClick}
        zoomLevel={zoomLevel}
      />
    ),
    [segments, selectedSegmentIds, handlePolylineClick, zoomLevel]
  )

  if (!initialLatLong) {
    return null;
  }

  return (
    <Box sx={{ height: '100%', width: '100%' }}>
      <MapContainer
        center={initialLatLong}
        zoom={10}
        scrollWheelZoom={true}
        style={{
          height: '100%',
          width: '100%',
          zIndex: 0,
        }}
        renderer={L.canvas({ tolerance: 5 })}
        ref={setMapRef}
        doubleClickZoom={false}
      >
        <TileLayer
          attribution='&copy; <a href="https://www.openaip.net/">openAIP Data</a> (<a href="https://creativecommons.org/licenses/by-sa/3.0/">CC-BY-NC-SA</a>)'
          url="https://tiles.stadiamaps.com/tiles/alidade_bright/{z}/{x}/{y}{r}.png"
        />
        {memoizedSegmentPolylines}
      </MapContainer>
    </Box>
  )
})

Map.displayName = 'Map';

export default Map