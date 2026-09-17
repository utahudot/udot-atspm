import {
  Area,
  Jurisdiction,
  SearchLocation as Location,
  Region,
  useGetArea,
  useGetJurisdiction,
  useGetRegion,
} from '@/api/config'
import { generatePin } from '@/features/locations/utils'
import { memo, useCallback, useEffect, useMemo, useRef, useState } from 'react'
import { Marker, Popup } from 'react-leaflet'
import MarkerClusterGroup from 'react-leaflet-cluster'
import LocationPopup, { type StreetViewAvailability } from './LocationPopup'

type MarkersProps = {
  locations: Location[] | undefined
  setLocation: (location: Location) => void
}

type MarkerItemProps = {
  marker: Location
  icon: L.DivIcon
  streetViewStatus: StreetViewAvailability | undefined
  onSelect: (loc: Location) => void
  onPopupOpen: (m: Location) => void
  streetViewUrl: (lat: number, lng: number) => string
  googleMapsUrl: (lat: number, lng: number) => string
  regionName: string
  jurisdictionName: string
  areaNames: string[]
}

const MarkerItem = memo(
  ({
    marker,
    icon,
    streetViewStatus,
    onSelect,
    onPopupOpen,
    streetViewUrl,
    googleMapsUrl,
    regionName,
    jurisdictionName,
    areaNames,
  }: MarkerItemProps) => {
    const eventHandlers = useMemo(
      () => ({
        click: () => onSelect(marker),
        popupopen: () => onPopupOpen(marker),
      }),
      [marker, onSelect, onPopupOpen]
    )

    return (
      <Marker
        key={marker.id}
        position={[marker.latitude, marker.longitude]}
        icon={icon}
        eventHandlers={eventHandlers}
      >
        <Popup offset={[0, -30]} closeButton={false} autoPan>
          <LocationPopup
            marker={marker}
            regionName={regionName}
            jurisdictionName={jurisdictionName}
            areaNames={areaNames}
            streetViewStatus={streetViewStatus}
            streetViewUrl={streetViewUrl}
            googleMapsUrl={googleMapsUrl}
          />
        </Popup>
      </Marker>
    )
  }
)
MarkerItem.displayName = 'MarkerItem'

const Markers = ({ locations, setLocation }: MarkersProps) => {
  const { data: regionsData } = useGetRegion()
  const { data: jurisdictionData } = useGetJurisdiction()
  const { data: areasData } = useGetArea()

  const [icons, setIcons] = useState<Record<string, L.DivIcon>>({})
  const [streetViewStatusById, setStreetViewStatusById] = useState<
    Record<string, StreetViewAvailability | undefined>
  >({})

  const streetViewInFlightRef = useRef<Record<string, boolean>>({})

  const regionNameById = useMemo(() => {
    const map: Record<string, string> = {}
    for (const r of (regionsData?.value ?? []) as Region[]) {
      if (r?.id != null) map[String(r.id)] = String(r.description ?? '')
    }
    return map
  }, [regionsData])

  const jurisdictionNameById = useMemo(() => {
    const map: Record<string, string> = {}
    for (const j of (jurisdictionData?.value ?? []) as Jurisdiction[]) {
      if (j?.id != null) map[String(j.id)] = String(j.name ?? '')
    }
    return map
  }, [jurisdictionData])

  const areaNameById = useMemo(() => {
    const map: Record<string, string> = {}
    for (const a of (areasData?.value ?? []) as Area[]) {
      if (a?.id != null) map[String(a.id)] = String(a.name ?? '')
    }
    return map
  }, [areasData])

  useEffect(() => {
    if (!locations) return

    const fetchIcons = async () => {
      const iconResults = await Promise.all(
        locations.map(async (location) => ({
          id: location.id,
          icon: await generatePin(location.locationTypeId),
        }))
      )

      const iconMap = iconResults.reduce<Record<string, L.DivIcon>>(
        (acc, { id, icon }) => {
          acc[id] = icon
          return acc
        },
        {}
      )

      setIcons(iconMap)
    }

    fetchIcons()
  }, [locations])

  const streetViewUrl = useCallback(
    (lat: number, lng: number) =>
      `https://www.google.com/maps/@?api=1&map_action=pano&viewpoint=${lat},${lng}`,
    []
  )

  const googleMapsUrl = useCallback(
    (lat: number, lng: number) =>
      `https://www.google.com/maps/search/?api=1&query=${encodeURIComponent(
        `${lat},${lng}`
      )}`,
    []
  )

  const handleSelectLocation = useCallback(
    (location: Location) => setLocation(location),
    [setLocation]
  )

  const checkStreetView = useCallback(
    async (id: string, lat: number, lng: number) => {
      const existing = streetViewStatusById[id]
      if (existing === 'available' || existing === 'unavailable') return
      if (streetViewInFlightRef.current[id]) return

      streetViewInFlightRef.current[id] = true

      // Optional: if you hate the extra “unknown” re-render, remove this line.
      setStreetViewStatusById((prev) => ({ ...prev, [id]: 'unknown' }))

      try {
        const r = await fetch(
          `/api/google/streetview/available?lat=${encodeURIComponent(
            lat
          )}&lng=${encodeURIComponent(lng)}`
        )
        const data = (await r.json()) as { available: boolean }
        setStreetViewStatusById((prev) => ({
          ...prev,
          [id]: data.available ? 'available' : 'unavailable',
        }))
      } catch {
        setStreetViewStatusById((prev) => ({ ...prev, [id]: 'unavailable' }))
      } finally {
        streetViewInFlightRef.current[id] = false
      }
    },
    [streetViewStatusById]
  )

  const handlePopupOpen = useCallback(
    (marker: Location) => {
      checkStreetView(marker.id, marker.latitude, marker.longitude)
    },
    [checkStreetView]
  )

  if (!locations) return null

  return (
    <MarkerClusterGroup chunkedLoading disableClusteringAtZoom={16}>
      {locations.map((marker) => {
        const icon = icons[marker.id]
        if (!icon) return null

        const streetViewStatus = streetViewStatusById[marker.id]

        const regionName =
          marker.regionId != null ? regionNameById[String(marker.regionId)] : ''
        const jurisdictionName =
          marker.jurisdictionId != null
            ? jurisdictionNameById[String(marker.jurisdictionId)]
            : ''
        const areaNames = (marker.areas ?? [])
          .map((id) => areaNameById[String(id)])
          .filter(Boolean)

        return (
          <MarkerItem
            key={marker.id}
            marker={marker}
            icon={icon}
            streetViewStatus={streetViewStatus}
            onSelect={handleSelectLocation}
            onPopupOpen={handlePopupOpen}
            streetViewUrl={streetViewUrl}
            googleMapsUrl={googleMapsUrl}
            regionName={regionName}
            jurisdictionName={jurisdictionName}
            areaNames={areaNames}
          />
        )
      })}
    </MarkerClusterGroup>
  )
}

export default memo(Markers)
