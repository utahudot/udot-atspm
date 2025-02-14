import { SearchLocation as Location } from '@/api/config/aTSPMConfigurationApi.schemas'
import Markers from '@/components/LocationMap/Markers'
import MapFilters from '@/components/MapFilters'
import { getEnv } from '@/utils/getEnv'
import ClearIcon from '@mui/icons-material/Clear'
import {
  Box,
  Button,
  ButtonGroup,
  ClickAwayListener,
  Popper,
  Skeleton,
  useTheme,
} from '@mui/material'
import L, { Map as LeafletMap } from 'leaflet'
import 'leaflet/dist/leaflet.css'
import { memo, useCallback, useEffect, useRef, useState } from 'react'
import { MapContainer, Polyline, TileLayer } from 'react-leaflet'

interface Filters {
  areaId?: number | null
  regionId?: number | null
  locationTypeId?: number | null
  jurisdictionId?: number | null
  measureTypeId?: number | null
}

interface LocationMapProps {
  location: Location | null
  setLocation: (location: Location) => void
  locations: Location[]
  filteredLocations: Location[]
  route?: number[][]
  center?: [number, number]
  mapHeight?: number | string
  filters: Filters
  updateFilters: (filters: Partial<Filters>) => void
}

const LocationMap = ({
  location,
  setLocation,
  locations,
  filteredLocations,
  route,
  center,
  mapHeight,
  filters,
  updateFilters,
}: LocationMapProps) => {
  const theme = useTheme()
  const [mapRef, setMapRef] = useState<LeafletMap | null>(null)
  const [isFiltersOpen, setIsFiltersOpen] = useState(false)
  const filtersButtonRef = useRef(null)

  const [mapInfo, setMapInfo] = useState<{
    tile_layer: string
    attribution: string
    initialLat: number
    initialLong: number
    mapZoom: number
  } | null>(null)

  const locationsEnabledLength = locations.filter((l) => l.chartEnabled).length

  useEffect(() => {
    const fetchEnv = async () => {
      const env = await getEnv()
      setMapInfo({
        tile_layer: env.MAP_TILE_LAYER,
        attribution: env.MAP_TILE_ATTRIBUTION,
        initialLat: parseFloat(env.MAP_DEFAULT_LATITUDE),
        initialLong: parseFloat(env.MAP_DEFAULT_LONGITUDE),
        mapZoom: parseInt(env?.MAP_DEFAULT_ZOOM),
      })
    }
    fetchEnv()
  }, [])

  // Pan to the selected location
  useEffect(() => {
    if (location && mapRef) {
      const markerLocation = locations.find((loc) => loc.id === location.id)
      if (markerLocation) {
        const { latitude, longitude } = markerLocation
        mapRef.setView([latitude, longitude], 16)
      }
    }
  }, [location, mapRef, locations])

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

  useEffect(() => {
    if (
      mapRef &&
      filteredLocations.length > 0 &&
      filteredLocations.length < locationsEnabledLength
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

  // Handle filter changes using MapFilters
  const handleFiltersClick = useCallback(() => {
    setIsFiltersOpen((prev) => !prev)
  }, [])

  const handleFiltersClearClick = useCallback(() => {
    updateFilters({
      areaId: null,
      regionId: null,
      locationTypeId: null,
      jurisdictionId: null,
      measureTypeId: null,
    })

    if (mapInfo?.initialLat && mapInfo?.initialLong) {
      mapRef?.setView([mapInfo.initialLat, mapInfo.initialLong], 6)
    }
  }, [updateFilters])

  const handleClosePopper = useCallback(() => {
    setIsFiltersOpen(false)
  }, [])

  if (!mapInfo) {
    return <Skeleton variant="rectangular" height={mapHeight ?? 400} />
  }

  return (
    <MapContainer
      center={center || [mapInfo.initialLat, mapInfo.initialLong]}
      zoom={mapInfo.mapZoom || 6}
      scrollWheelZoom={true}
      style={{
        height: mapHeight || 'calc(100% - 80px)',
        minHeight: mapHeight || '400px',
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
              disabled={!Object.values(filters).some((value) => value != null)}
              sx={{
                '&:disabled': { backgroundColor: theme.palette.grey[300] },
              }}
            >
              <ClearIcon fontSize="small" sx={{ p: 0 }} />
            </Button>
          </ButtonGroup>
          <Popper
            open={isFiltersOpen}
            anchorEl={filtersButtonRef.current}
            placement="bottom-end"
            style={{ zIndex: 1000 }}
          >
            <MapFilters
              filters={filters}
              onFilterChange={updateFilters}
              locationsTotal={locationsEnabledLength}
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
