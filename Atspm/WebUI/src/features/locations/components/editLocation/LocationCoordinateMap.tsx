import { generatePin } from '@/features/locations/utils'
import { useEnv } from '@/hooks/useEnv'
import type L from 'leaflet'
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
  const { data: env } = useEnv()
  const [markerPosition, setMarkerPosition] = useState<LatLngTuple>(center)
  const [markerIcon, setMarkerIcon] = useState<L.DivIcon | null>(null)

  useEffect(() => {
    setMarkerPosition(center)
  }, [center])

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

  const handleDragEnd = (e: L.LeafletEvent) => {
    const target = e.target as L.Marker
    const pos = target.getLatLng()
    const newPos: LatLngTuple = [pos.lat, pos.lng]
    setMarkerPosition(newPos)
    onSelect(pos.lat, pos.lng)
  }

  if (!env?.MAP_TILE_LAYER || !env?.MAP_TILE_ATTRIBUTION) return null

  return (
    <MapContainer
      center={center}
      zoom={zoom}
      style={{ height: '100%', width: '100%' }}
      scrollWheelZoom
    >
      <TileLayer
        attribution={env.MAP_TILE_ATTRIBUTION}
        url={`${env.MAP_TILE_LAYER}`}
      />

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
