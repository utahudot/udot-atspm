import Markers from '@/components/LocationMap/Markers'
import MapFilters from '@/components/MapFilters'
import { Location } from '@/features/locations/types'
import { getEnv } from '@/utils/getEnv'
import ClearIcon from '@mui/icons-material/Clear'
import {
  Box,
  Button,
  ButtonGroup,
  ClickAwayListener,
  Popper,
  useTheme,
} from '@mui/material'
import L, { Map as LeafletMap } from 'leaflet'
import 'leaflet/dist/leaflet.css'
import { memo, useEffect, useRef, useState } from 'react'
import { MapContainer, Polyline, TileLayer } from 'react-leaflet'

type MapProps = {
  location: Location | null
  setLocation: (location: Location) => void
  locations: Location[]
  route?: number[][]
  center?: [number, number]
  zoom?: number
  mapHeight?: number | string
}

const LocationMap = ({
  location,
  setLocation,
  locations,
  route,
  center,
  zoom,
  mapHeight,
}: MapProps) => {
  const theme = useTheme()
  const [mapRef, setMapRef] = useState<LeafletMap | null>(null)
  const [isPopperOpen, setIsPopperOpen] = useState(false)
  const filtersButtonRef = useRef(null)
  const [selectedAreaId, setSelectedAreaId] = useState<number | null>(null)
  const [selectedRegionId, setSelectedRegionId] = useState<number | null>(null)
  const [selectedLocationTypeId, setSelectedLocationTypeId] = useState<
    number | null
  >(null)
  const [selectedJurisdictionId, setSelectedJurisdictionId] = useState<
    number | null
  >(null)
  const [selectedMeasureTypeId, setSelectedMeasureTypeId] = useState<
    number | null
  >(null)
  const [filteredLocations, setFilteredLocations] = useState(locations)
  const [mapInfo, setMapInfo] = useState<{
    tile_layer: string
    attribution: string
    initialLat: number
    initialLong: number
  } | null>(null)

  useEffect(() => {
    const fetchEnv = async () => {
      const env = await getEnv()
      setMapInfo({
        tile_layer: env.MAP_TILE_LAYER,
        attribution: env.MAP_TILE_ATTRIBUTION,
        initialLat: parseFloat(env.MAP_DEFAULT_LATITUDE),
        initialLong: parseFloat(env.MAP_DEFAULT_LONGITUDE),
      })
    }
    fetchEnv()
  }, [])

  // Pan to the selected location
  useEffect(() => {
    if (location && mapRef) {
      const markerToPanTo = filteredLocations?.find(
        (marker) => marker.locationIdentifier === location.locationIdentifier
      )

      if (markerToPanTo) {
        const { latitude, longitude } = markerToPanTo
        mapRef?.setView([latitude, longitude], 16)
      }
    }
  }, [location, filteredLocations, mapRef])

  // Resize the map when the container resizes
  useEffect(() => {
    if (!mapRef) return

    const mapContainer = mapRef.getContainer()

    const handleResize = () => {
      mapRef.invalidateSize()
    }

    const resizeObserver = new ResizeObserver(handleResize)
    resizeObserver.observe(mapContainer)

    return () => {
      resizeObserver.disconnect()
    }
  }, [mapRef])

  // Filter locations based on selected filters
  useEffect(() => {
    const filtered = locations.filter(
      (location) =>
        (!selectedAreaId || location.areas?.includes(selectedAreaId)) &&
        (!selectedRegionId || location.regionId === selectedRegionId) &&
        (!selectedLocationTypeId ||
          location.locationTypeId === selectedLocationTypeId) &&
        (!selectedMeasureTypeId ||
          location.charts?.includes(selectedMeasureTypeId)) &&
        (!selectedJurisdictionId ||
          location.jurisdictionId === selectedJurisdictionId)
    )
    setFilteredLocations(filtered)
  }, [
    locations,
    selectedAreaId,
    selectedRegionId,
    selectedLocationTypeId,
    selectedJurisdictionId,
    selectedMeasureTypeId,
  ])

  useEffect(() => {
    if (
      mapRef &&
      filteredLocations.length > 0 &&
      filteredLocations.length < locations.length
    ) {
      const bounds = L.latLngBounds(
        filteredLocations
          .filter(
            (loc) =>
              loc.latitude >= -90 &&
              loc.latitude <= 90 &&
              loc.longitude >= -180 &&
              loc.longitude <= 180
          )
          .map((loc) => [loc.latitude, loc.longitude])
      )

      // Check if bounds are valid before fitting the map to these bounds
      if (bounds.isValid()) {
        mapRef.fitBounds(bounds)
      }
    }
  }, [mapRef, filteredLocations, locations])

  const handleFiltersClick = () => {
    setIsPopperOpen(!isPopperOpen)
  }

  const handleFiltersClearClick = () => {
    setSelectedAreaId(null)
    setSelectedRegionId(null)
    setSelectedLocationTypeId(null)
    setSelectedJurisdictionId(null)
    setSelectedMeasureTypeId(null)
    if (mapInfo?.initialLat && mapInfo?.initialLong) {
      mapRef?.setView([mapInfo.initialLat, mapInfo.initialLong], 6)
    }
  }

  const handleClosePopper = () => {
    setIsPopperOpen(false)
  }

  if (!mapInfo) {
    return <div>Loading...</div>
  }

  return (
    <MapContainer
      center={center || [mapInfo.initialLat, mapInfo.initialLong]}
      zoom={zoom || 6}
      scrollWheelZoom={true}
      style={{
        height: mapHeight || 'calc(100% - 80px)',
        minHeight: '400px',
        width: '100%',
      }}
      ref={setMapRef}
    >
      <ClickAwayListener onClickAway={handleClosePopper}>
        <Box>
          <ButtonGroup
            variant="contained"
            size="small"
            disableElevation
            sx={{
              position: 'absolute',
              right: '10px',
              top: '10px',
              zIndex: 1000,
            }}
          >
            <Button variant="contained" onClick={handleFiltersClick}>
              Filters
            </Button>
            <Button
              ref={filtersButtonRef}
              variant="contained"
              size="small"
              aria-label="Clear filters"
              onClick={handleFiltersClearClick}
              disabled={
                !selectedAreaId &&
                !selectedRegionId &&
                !selectedLocationTypeId &&
                !selectedJurisdictionId &&
                !selectedMeasureTypeId
              }
              sx={{
                '&:disabled': { backgroundColor: theme.palette.grey[300] },
              }}
            >
              <ClearIcon fontSize="small" sx={{ p: 0 }} />
            </Button>
          </ButtonGroup>
          <Popper
            open={isPopperOpen}
            anchorEl={filtersButtonRef.current}
            placement="bottom-end"
            style={{ zIndex: 1000 }}
          >
            <MapFilters
              setSelectedAreaId={setSelectedAreaId}
              setSelectedRegionId={setSelectedRegionId}
              setSelectedLocationTypeId={setSelectedLocationTypeId}
              setSelectedJurisdictionId={setSelectedJurisdictionId}
              setSelectedMeasureTypeId={setSelectedMeasureTypeId}
              selectedAreaId={selectedAreaId}
              selectedRegionId={selectedRegionId}
              selectedLocationTypeId={selectedLocationTypeId}
              selectedJurisdictionId={selectedJurisdictionId}
              selectedMeasureTypeId={selectedMeasureTypeId}
              locationsTotal={locations.length}
              locationsFiltered={filteredLocations.length}
            />
          </Popper>
        </Box>
      </ClickAwayListener>
      <TileLayer attribution={mapInfo.attribution} url={mapInfo.tile_layer} />
      <Markers locations={filteredLocations} setLocation={setLocation} />
      {route && route.length > 0 && (
        <Polyline
          positions={route.map((coord) => [coord[0], coord[1]])}
          weight={5}
        />
      )}
    </MapContainer>
  )
}

export default memo(LocationMap)
