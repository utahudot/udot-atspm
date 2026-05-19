import { Segment } from '@/api/speedManagement/aTSPMSpeedManagementApi.schemas'
import { useSegmentEditorStore } from '@/features/speedManagementTool/components/SegmentEditor/segmentEditorStore'
import { SegmentSelectMapProps } from '@/features/speedManagementTool/components/SegmentSelectMap'
import SegmentPolylines from '@/features/speedManagementTool/components/SegmentSelectMap/SegmentPolylines'
import { getEnv } from '@/utils/getEnv'
import {
  Alert,
  Box,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableRow,
} from '@mui/material'
import L, { Map as LeafletMap } from 'leaflet'
import 'leaflet/dist/leaflet.css'
import { memo, useEffect, useMemo, useState } from 'react'
import { MapContainer, TileLayer } from 'react-leaflet'

const SegmentSelectMap = ({
  segments,
  onSegmentSelect,
  selectedSegmentIds,
}: SegmentSelectMapProps) => {
  const {
    allSegments: storedSegments,
    associatedEntityIds,
    isLoadingSegments,
    mapCenter,
    setMapCenter,
  } = useSegmentEditorStore()

  const allSegments = segments ?? storedSegments
  const [mapRef, setMapRef] = useState<LeafletMap | null>(null)
  const [hoveredSegment, setHoveredSegment] = useState<Segment | null>(null)

  // Invalidate map size on mount and resize
  useEffect(() => {
    if (mapRef) {
      mapRef.invalidateSize()
      const mapContainer = mapRef.getContainer()
      const resizeObserver = new ResizeObserver(() => {
        mapRef.invalidateSize()
      })
      resizeObserver.observe(mapContainer)
      return () => {
        resizeObserver.disconnect()
      }
    }
  }, [mapRef])

  // Update zoom level when the map zooms
  useEffect(() => {
    if (mapRef) {
      const updateMapState = () => {
        const center = mapRef.getCenter()
        setMapCenter({
          lat: center.lat,
          lng: center.lng,
          zoom: mapRef.getZoom(),
        })
      }
      mapRef.on('zoomend', updateMapState)
      return () => {
        if (mapRef) {
          mapRef.off('zoomend', updateMapState)
        }
      }
    }
  }, [mapRef, setMapCenter])

  // Initialize initialLatLong and fit bounds
  useEffect(() => {
    const initializeMap = async () => {
      if (
        associatedEntityIds.length > 0 &&
        allSegments &&
        allSegments.length > 0 &&
        mapCenter !== null
      ) {
        const bounds = L.latLngBounds([])
        associatedEntityIds.forEach((id) => {
          const segment = allSegments.find((segment) => segment.id === id)
          if (segment?.geometry?.coordinates) {
            segment.geometry.coordinates.forEach((coord: [number, number]) => {
              bounds.extend(coord)
            })
          }
        })
        if (bounds.isValid() && mapRef) {
          mapRef.fitBounds(bounds, { padding: [100, 100] })
          const center = bounds.getCenter()
          setMapCenter({
            lat: center.lat,
            lng: center.lng,
            zoom: mapRef.getZoom(),
          })
        }
      } else if (!mapCenter) {
        try {
          const env = await getEnv()
          const newCenter: { lat: number; lng: number; zoom: number } = {
            lat: parseFloat(env?.MAP_DEFAULT_LATITUDE),
            lng: parseFloat(env?.MAP_DEFAULT_LONGITUDE),
            zoom: parseInt(env?.MAP_DEFAULT_ZOOM, 10) || 10,
          }
          setMapCenter(newCenter)
        } catch (error) {
          console.error('Failed to fetch env:', error)
        }
      }
    }
    initializeMap()
  }, [])

  const memoizedSegmentPolylines = useMemo(
    () => (
      <SegmentPolylines
        segments={allSegments || []}
        onSegmentSelect={onSegmentSelect}
        selectedSegmentIds={selectedSegmentIds}
        zoomLevel={mapCenter?.zoom}
        setHoveredSegment={setHoveredSegment}
      />
    ),
    [allSegments, onSegmentSelect, mapCenter?.zoom, selectedSegmentIds]
  )

  if (!mapCenter) {
    console.log('Initial coordinates not set yet.')
    return null
  }

  return (
    <Box sx={{ height: '100%', width: '100%', position: 'relative' }}>
      {isLoadingSegments && (
        <Alert
          sx={{
            position: 'absolute',
            top: 16,
            right: 16,
            zIndex: 1000,
            maxWidth: '300px',
          }}
          severity="info"
        >
          Segments are loading...
        </Alert>
      )}
      <MapContainer
        center={[mapCenter.lat, mapCenter.lng]}
        zoom={mapCenter.zoom}
        scrollWheelZoom={true}
        style={{
          height: '100%',
          width: '100%',
        }}
        renderer={L.canvas({ tolerance: 5 })}
        ref={setMapRef}
        doubleClickZoom={false}
      >
        <TileLayer
          attribution='&copy; <a href="https://www.openaip.net/">openAIP Data</a> (<a href="https://creativecommons.org/licenses/by-sa/3.0/">CC-BY-NC-SA</a>)'
          url="https://tiles.stadiamaps.com/tiles/alidade_smooth/{z}/{x}/{y}{r}.png"
        />
        {memoizedSegmentPolylines}
      </MapContainer>
      {hoveredSegment && (
        <Paper
          elevation={3}
          sx={{
            position: 'absolute',
            bottom: '20px',
            left: '20px',
            padding: '8px 12px',
            backgroundColor: 'rgba(255, 255, 255, 0.9)',
            zIndex: 1000,
            pointerEvents: 'none',
          }}
        >
          <TableContainer>
            <Table size="small">
              <TableBody>
                <TableRow>
                  <TableCell sx={{ fontWeight: 'bold' }}>
                    Segment Name:
                  </TableCell>
                  <TableCell align="right">
                    {hoveredSegment.properties.name}
                  </TableCell>
                </TableRow>
                <TableRow>
                  <TableCell sx={{ fontWeight: 'bold' }}>
                    Speed Limit:
                  </TableCell>
                  <TableCell align="right">
                    {hoveredSegment.properties.speedLimit} mph
                  </TableCell>
                </TableRow>
              </TableBody>
            </Table>
          </TableContainer>
        </Paper>
      )}
    </Box>
  )
}

SegmentSelectMap.displayName = 'SegmentSelectMap'

export default memo(SegmentSelectMap)
