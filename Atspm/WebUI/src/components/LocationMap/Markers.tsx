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
import { Marker, Pane, Popup, Tooltip } from 'react-leaflet'
import MarkerClusterGroup from 'react-leaflet-cluster'
import LocationPopup, { type StreetViewAvailability } from './LocationPopup'
import styles from './Markers.module.css'

type MarkersProps = {
  locations: Location[] | undefined
  setLocation: (location: Location) => void
  highlightedLocationId?: number
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
  isHighlighted: boolean
  hasHighlightedMarker: boolean
}

type MarkerDisplayDetails = {
  regionName: string
  jurisdictionName: string
  areaNames: string[]
}

const POPUP_OFFSET: [number, number] = [0, -30]
const LOCATION_LABEL_PANE = 'location-labels'
const LOCATION_LABEL_PANE_STYLE = { zIndex: 550 }

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
    isHighlighted,
    hasHighlightedMarker,
  }: MarkerItemProps) => {
    const position = useMemo<[number, number]>(
      () => [marker.latitude, marker.longitude],
      [marker.latitude, marker.longitude]
    )
    const locationTypeLabelClass =
      marker.locationTypeId === 1
        ? styles.intersectionLocationLabel
        : marker.locationTypeId === 2
          ? styles.rampMeterLocationLabel
          : styles.defaultLocationLabel
    const locationIdentifier = marker.locationIdentifier?.trim()

    const eventHandlers = useMemo(
      () => ({
        click: () => onSelect(marker),
        popupopen: () => onPopupOpen(marker),
      }),
      [marker, onSelect, onPopupOpen]
    )

    return (
      <Marker
        position={position}
        icon={icon}
        eventHandlers={eventHandlers}
        opacity={hasHighlightedMarker && !isHighlighted ? 0.45 : 1}
        zIndexOffset={isHighlighted ? 1000 : 0}
      >
          <Tooltip
            permanent
            direction={'right'}
            offset={[-8, -22]}
            opacity={1}
            pane={LOCATION_LABEL_PANE}
            className={`${styles.locationIdentifierLabel} ${locationTypeLabelClass}`}
          >
            <span className={styles.locationIdentifierText}>
              {locationIdentifier}
            </span>
          </Tooltip>
        <Popup offset={POPUP_OFFSET} closeButton={false} autoPan>
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

const Markers = ({
  locations,
  setLocation,
  highlightedLocationId,
}: MarkersProps) => {
  const { data: regionsData } = useGetRegion()
  const { data: jurisdictionData } = useGetJurisdiction()
  const { data: areasData } = useGetArea()

  const [icons, setIcons] = useState<Record<string, L.DivIcon>>({})
  const [streetViewStatusById, setStreetViewStatusById] = useState<
    Record<string, StreetViewAvailability | undefined>
  >({})

  const streetViewStatusByIdRef = useRef<
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

  const markerDisplayDetailsById = useMemo(() => {
    const details: Record<string, MarkerDisplayDetails> = {}

    for (const marker of locations ?? []) {
      details[marker.id] = {
        regionName:
          marker.regionId != null
            ? regionNameById[String(marker.regionId)]
            : '',
        jurisdictionName:
          marker.jurisdictionId != null
            ? jurisdictionNameById[String(marker.jurisdictionId)]
            : '',
        areaNames: (marker.areas ?? [])
          .map((id) => areaNameById[String(id)])
          .filter(Boolean),
      }
    }

    return details
  }, [areaNameById, jurisdictionNameById, locations, regionNameById])

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

  const updateStreetViewStatus = useCallback(
    (id: string, status: StreetViewAvailability) => {
      streetViewStatusByIdRef.current[id] = status
      setStreetViewStatusById((prev) =>
        prev[id] === status ? prev : { ...prev, [id]: status }
      )
    },
    []
  )

  const checkStreetView = useCallback(
    async (id: string, lat: number, lng: number) => {
      const existing = streetViewStatusByIdRef.current[id]
      if (existing === 'available' || existing === 'unavailable') return
      if (streetViewInFlightRef.current[id]) return

      streetViewInFlightRef.current[id] = true
      updateStreetViewStatus(id, 'unknown')

      try {
        const r = await fetch(
          `/api/google/streetview/available?lat=${encodeURIComponent(
            lat
          )}&lng=${encodeURIComponent(lng)}`
        )
        const data = (await r.json()) as { available: boolean }
        updateStreetViewStatus(
          id,
          data.available ? 'available' : 'unavailable'
        )
      } catch {
        updateStreetViewStatus(id, 'unavailable')
      } finally {
        streetViewInFlightRef.current[id] = false
      }
    },
    [updateStreetViewStatus]
  )

  const handlePopupOpen = useCallback(
    (marker: Location) => {
      checkStreetView(marker.id, marker.latitude, marker.longitude)
    },
    [checkStreetView]
  )

  if (!locations) return null

  return (
    <>
      <Pane
        name={LOCATION_LABEL_PANE}
        style={LOCATION_LABEL_PANE_STYLE}
      />
      <MarkerClusterGroup chunkedLoading disableClusteringAtZoom={16}>
        {locations.map((marker) => {
          const icon = icons[marker.id]
          if (!icon) return null

          const streetViewStatus = streetViewStatusById[marker.id]

          const displayDetails = markerDisplayDetailsById[marker.id]

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
              regionName={displayDetails.regionName}
              jurisdictionName={displayDetails.jurisdictionName}
              areaNames={displayDetails.areaNames}
              isHighlighted={marker.id === highlightedLocationId}
              hasHighlightedMarker={highlightedLocationId != null}
            />
          )
        })}
      </MarkerClusterGroup>
    </>
  )
}

export default memo(Markers)
