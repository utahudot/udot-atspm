import { useApiV1MapLayerCount } from '@/api/config/aTSPMConfigurationApi'
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
  const [isLayersPopperOpen, setIsLayersPopperOpen] = useState(false)
  const layersButtonRef = useRef(null)

  const { data: mapLayerData, isLoading } = useApiV1MapLayerCount()
  const [activeLayers, setActiveLayers] = useState<number[]>([])

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
    mapLayerData?.value.forEach((layer) => {
      if (activeLayers.includes(layer.id)) {
        if (layer.serviceType === 'mapserver') {
          const baseUrl = layer.mapLayerUrl.split('/query')[0].replace('/0', '')
          new DynamicMapLayer({
            url: baseUrl,
            opacity: 1,
            layers: [0],
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
  }, [mapRef, activeLayers])

  // Add this handler
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
            {mapLayerData?.value.map((layer) => (
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
