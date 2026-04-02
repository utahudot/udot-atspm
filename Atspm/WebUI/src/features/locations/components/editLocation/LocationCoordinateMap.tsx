import { generatePin } from '@/features/locations/utils'
import { getEnv } from '@/utils/getEnv'
import { Box, Skeleton } from '@mui/material'
import type L from 'leaflet'
import { Map as LeafletMap } from 'leaflet'
import 'leaflet/dist/leaflet.css'
import { useEffect, useState } from 'react'
import { MapContainer, Marker, TileLayer, useMapEvents } from 'react-leaflet'

type LatLngTuple = [number, number]

type LocationCoordinatesMapProps = {
  center: LatLngTuple
  onSelect: (lat: number, lng: number) => void
  zoom: number
  locationTypeId?: number | null
}

const ClickHandler = ({
  onSelect,
  setMarkerPosition,
}: {
  onSelect: (lat: number, lng: number) => void
  setMarkerPosition: (pos: LatLngTuple) => void
}) => {
  useMapEvents({
    click(e) {
      const pos: LatLngTuple = [e.latlng.lat, e.latlng.lng]
      setMarkerPosition(pos)
      onSelect(e.latlng.lat, e.latlng.lng)
    },
  })
  return null
}

export default function LocationCoordinateMap({
  center,
  onSelect,
  zoom,
  locationTypeId,
}: LocationCoordinatesMapProps) {
  const [mapRef, setMapRef] = useState<LeafletMap | null>(null)
  const [markerPosition, setMarkerPosition] = useState<LatLngTuple>(center)
  const [markerIcon, setMarkerIcon] = useState<L.DivIcon | null>(null)
  const [mapInfo, setMapInfo] = useState<{
    tileLayer: string | null | undefined
    attribution: string
  } | null>(null)

  useEffect(() => {
    setMarkerPosition(center)
  }, [center])

  useEffect(() => {
    let cancelled = false

    async function loadMapInfo() {
      try {
        const env = await getEnv()

        if (!cancelled) {
          setMapInfo({
            tileLayer: env?.MAP_TILE_LAYER,
            attribution: env?.MAP_TILE_ATTRIBUTION ?? '',
          })
        }
      } catch {
        if (!cancelled) {
          setMapInfo({
            tileLayer: null,
            attribution: '',
          })
        }
      }
    }

    loadMapInfo()

    return () => {
      cancelled = true
    }
  }, [])

  useEffect(() => {
    let cancelled = false

    async function loadIcon() {
      const id = locationTypeId ?? 0
      const icon = await generatePin(id)
      if (!cancelled) {
        setMarkerIcon(icon)
      }
    }

    loadIcon()

    return () => {
      cancelled = true
    }
  }, [locationTypeId])

  useEffect(() => {
    if (!mapRef) return

    mapRef.setView(center, zoom)

    const animationFrame = window.requestAnimationFrame(() => {
      mapRef.invalidateSize()
    })

    return () => {
      window.cancelAnimationFrame(animationFrame)
    }
  }, [mapRef, center, zoom])

  useEffect(() => {
    if (!mapRef || typeof ResizeObserver === 'undefined') return

    const mapContainer = mapRef.getContainer()
    const resizeObserver = new ResizeObserver(() => {
      mapRef.invalidateSize()
    })

    resizeObserver.observe(mapContainer)

    return () => {
      resizeObserver.disconnect()
    }
  }, [mapRef])

  const handleDragEnd = (e: L.LeafletEvent) => {
    const target = e.target as L.Marker
    const pos = target.getLatLng()
    const newPos: LatLngTuple = [pos.lat, pos.lng]
    setMarkerPosition(newPos)
    onSelect(pos.lat, pos.lng)
  }

  if (!mapInfo) {
    return (
      <Skeleton
        variant="rectangular"
        sx={{ height: '100%', minHeight: 300, width: '100%' }}
      />
    )
  }

  if (!mapInfo.tileLayer) {
    return (
      <Box
        sx={{
          alignItems: 'center',
          display: 'flex',
          height: '100%',
          justifyContent: 'center',
          minHeight: 300,
          width: '100%',
        }}
      >
        Map tiles are not configured.
      </Box>
    )
  }

  return (
    <MapContainer
      center={center}
      zoom={zoom}
      style={{ height: '100%', width: '100%' }}
      scrollWheelZoom
      ref={setMapRef}
    >
      <TileLayer attribution={mapInfo.attribution} url={mapInfo.tileLayer} />

      {markerIcon && (
        <Marker
          position={markerPosition}
          icon={markerIcon}
          draggable
          eventHandlers={{ dragend: handleDragEnd }}
        />
      )}

      <ClickHandler onSelect={onSelect} setMarkerPosition={setMarkerPosition} />
    </MapContainer>
  )
}
