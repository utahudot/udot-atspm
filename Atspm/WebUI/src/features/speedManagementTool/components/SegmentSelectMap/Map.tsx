import { getEnv } from '@/lib/getEnv'
import { Box } from '@mui/material'
import L, { Map as LeafletMap } from 'leaflet'
import React, { memo, useCallback, useEffect, useMemo, useState } from 'react'
import { MapContainer, Polyline, TileLayer, useMap } from 'react-leaflet'

interface MapProps {
  segments: {
    type: string
    geometry: {
      type: string
      coordinates: [number, number][]
    }
    properties: {
      Id: string
      StartMilePoint:number,
      EndMilePoint:number
      // ... other properties
    }
  }[]
  selectedSegmentIds: string[]
  onSegmentSelect: (id: string) => void
}

const getPolylineWeight = (zoom: number) => {
  if (zoom >= 18) return 10
  if (zoom >= 15) return 5
  if (zoom >= 12) return 4
  if (zoom >= 10) return 3
  if (zoom >= 8) return 2
  return 2
}

const SegmentPolylines: React.FC<{
  segments: MapProps['segments']
  selectedSegmentIds: string[]
  onSegmentSelect: (id: string, startMile:number, endMile:number) => void
  zoomLevel: number // Add zoomLevel to props
}> = memo(function SegmentPolylines({ segments, selectedSegmentIds, onSegmentSelect, zoomLevel }) {
  const map = useMap()
  useEffect(() => {
    map.invalidateSize()
  }, [segments, selectedSegmentIds])

  return (
    <>
      {segments.length > 0 && segments.map((segment, index) => { // Check if segments are loaded
        return (
          <Polyline
            key={index}
            positions={segment.geometry.coordinates}
            pathOptions={{
              color: selectedSegmentIds == undefined || selectedSegmentIds.length === 0 || !selectedSegmentIds.includes(segment.properties.Id) // Check if selectedSegmentIds is empty
              ? 'gray'
              : 'blue',
              weight: getPolylineWeight(zoomLevel),
            }}
            eventHandlers={{
              click: () => onSegmentSelect(segment.properties.Id, segment.properties.StartMilePoint, segment.properties.EndMilePoint),
            }}
          />
        )
      })}
    </>
  )
})

const Map: React.FC<MapProps> = ({
  segments,
  selectedSegmentIds,
  onSegmentSelect,
}) => {
  const [mapRef, setMapRef] = useState<LeafletMap | null>(null)
  const [initialLatLong, setInitialLatLong] = useState<[number, number] | null>(
    null
  )
  const [zoomLevel, setZoomLevel] = useState(6)

  
  const handlePolylineClick = useCallback(
    (id: string, startMile: number, endMile: number) => {
      onSegmentSelect(id, startMile, endMile); // Pass the mile points
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
        zoomLevel={zoomLevel} // Pass zoomLevel here
      />
    ),
    [segments, selectedSegmentIds, handlePolylineClick, zoomLevel]
  )

  return (
    <Box sx={{ height: '100%', width: '100%' }}>
      {initialLatLong && (
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
      )}
    </Box>
  )
}

export default memo(Map)
