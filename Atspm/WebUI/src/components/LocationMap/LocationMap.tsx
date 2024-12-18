import { useGetMapLayer } from '@/api/config/aTSPMConfigurationApi'
import { ServiceType } from '@/api/config/aTSPMConfigurationApi.schemas'
import Markers from '@/components/LocationMap/Markers'
import MapFilters from '@/components/MapFilters'
import { Location } from '@/features/locations/types'
import { getEnv } from '@/lib/getEnv'
import ClearIcon from '@mui/icons-material/Clear'
import LayersIcon from '@mui/icons-material/Layers'
import {
  Box,
  Button,
  ButtonGroup,
  Checkbox,
  ClickAwayListener,
  FormControlLabel,
  Popper,
  useTheme,
} from '@mui/material'
import { DynamicMapLayer, FeatureLayer } from 'esri-leaflet'
import 'esri-leaflet-renderers'
import L, { Map as LeafletMap } from 'leaflet'
import 'leaflet/dist/leaflet.css'
import { memo, useEffect, useRef, useState } from 'react'
import { MapContainer, Polyline, TileLayer } from 'react-leaflet'

interface MapProps {
  location: Location | null
  setLocation: (location: Location) => void
  locations: Location[]
  route?: number[][]
  center?: [number, number]
  zoom?: number
  mapHeight?: number | string
}

interface FilterProps {
  areaId: number | null
  regionId: number | null
  locationTypeId: number | null
  jurisdictionId: number | null
  measureTypeId: number | null
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
  const [filters, setFilters] = useState<FilterProps>({
    areaId: null,
    regionId: null,
    locationTypeId: null,
    jurisdictionId: null,
    measureTypeId: null,
  })
  const [filteredLocations, setFilteredLocations] = useState(locations)
  const [mapInfo, setMapInfo] = useState<{
    tile_layer: string
    attribution: string
    initialLat: number
    initialLong: number
  } | null>(null)
  const [isLayersPopperOpen, setIsLayersPopperOpen] = useState(false)
  const layersButtonRef = useRef(null)

  const { data: mapLayerData } = useGetMapLayer()
  const [activeLayers, setActiveLayers] = useState<number[]>([])

  console.log('mapLayerData', mapLayerData)

  useEffect(() => {
    if (mapLayerData?.value) {
      setActiveLayers(
        mapLayerData.value
          .filter((layer) => layer.showByDefault)
          .map((layer) => layer.id)
      )
    }
  }, [mapLayerData])

  useEffect(() => {
    if (!mapRef) return

    // Clear existing layers
    mapRef.eachLayer((layer) => {
      if (layer instanceof FeatureLayer || layer instanceof DynamicMapLayer) {
        mapRef.removeLayer(layer)
      }
    })

    // Add active layers
    mapLayerData?.value?.forEach((layer) => {
      if (activeLayers.includes(layer.id)) {
        if (layer.serviceType === ServiceType.MapServer) {
          new DynamicMapLayer({
            url: layer.mapLayerUrl,
            opacity: 1,
          }).addTo(mapRef)
        } else {
          // Feature server - just use the URL as is
          new FeatureLayer({
            url: layer.mapLayerUrl,
            useCors: false, // Sometimes needed for ArcGIS services
          }).addTo(mapRef)
        }
      }
    })
  }, [mapRef, activeLayers, mapLayerData])

  const handleLayerToggle = (layerId: number) => {
    setActiveLayers((prev) =>
      prev.includes(layerId)
        ? prev.filter((id) => id !== layerId)
        : [...prev, layerId]
    )
  }

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
    const filtered = locations.filter((location) => {
      return (
        (!filters.areaId || location.areas?.includes(filters.areaId)) &&
        (!filters.regionId || location.regionId === filters.regionId) &&
        (!filters.locationTypeId ||
          location.locationTypeId === filters.locationTypeId) &&
        (!filters.measureTypeId ||
          location.charts?.includes(filters.measureTypeId)) &&
        (!filters.jurisdictionId ||
          location.jurisdictionId === filters.jurisdictionId)
      )
    })
    setFilteredLocations(filtered)
  }, [filters, locations])

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

      if (bounds.isValid()) {
        mapRef.fitBounds(bounds)
      }
    }
  }, [mapRef, filteredLocations, locations])

  const handleFiltersChange = (newFilters: Partial<typeof filters>) => {
    setFilters((prevFilters) => ({ ...prevFilters, ...newFilters }))
  }

  const handleFiltersClearClick = () => {
    setFilters({
      areaId: null,
      regionId: null,
      locationTypeId: null,
      jurisdictionId: null,
      measureTypeId: null,
    })
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
            <Button
              variant="contained"
              onClick={() => setIsPopperOpen(!isPopperOpen)}
            >
              Filters
            </Button>
            <Button
              ref={filtersButtonRef}
              variant="contained"
              size="small"
              aria-label="Clear filters"
              onClick={handleFiltersClearClick}
              disabled={Object.values(filters).every((value) => value === null)}
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
              filters={filters}
              onFiltersChange={handleFiltersChange}
              locationsTotal={locations.length}
              locationsFiltered={filteredLocations.length}
            />
          </Popper>
        </Box>
      </ClickAwayListener>
      <ButtonGroup
        variant="contained"
        size="small"
        disableElevation
        sx={{
          position: 'absolute',
          left: '10px',
          bottom: '20px',
          zIndex: 1000,
        }}
      >
        <Button
          ref={layersButtonRef}
          variant="contained"
          onClick={() => setIsLayersPopperOpen(!isLayersPopperOpen)}
        >
          <LayersIcon fontSize="small" />
        </Button>
      </ButtonGroup>

      <Popper
        open={isLayersPopperOpen}
        anchorEl={layersButtonRef.current}
        placement="top-start"
        style={{ zIndex: 1000 }}
      >
        <ClickAwayListener onClickAway={() => setIsLayersPopperOpen(false)}>
          <Box
            sx={{
              p: 2,
              bgcolor: 'background.paper',
              borderRadius: 1,
              boxShadow: 3,
              display: 'flex',
              flexDirection: 'column',
              gap: 1,
            }}
          >
            {mapLayerData?.value?.map((layer) => (
              <FormControlLabel
                key={layer.id}
                control={
                  <Checkbox
                    checked={activeLayers.includes(layer.id)}
                    onChange={() => handleLayerToggle(layer.id)}
                    size="small"
                  />
                }
                label={layer.name}
              />
            ))}
          </Box>
        </ClickAwayListener>
      </Popper>
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
