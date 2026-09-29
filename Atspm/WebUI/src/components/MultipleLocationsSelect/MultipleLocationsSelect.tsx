import {
  getLocationFromKey,
  SearchLocation as Location,
  Route,
  useGetLocationLatestVersionOfAllLocations,
  useGetRoute,
} from '@/api/config'
import { Filters } from '@/features/locations/components/selectLocation'
import LocationInput from '@/features/locations/components/selectLocation/LocationInput'
import SelectLocationMap from '@/features/locations/components/selectLocationMap'
import AddIcon from '@mui/icons-material/Add'
import {
  Box,
  Button,
  FormControl,
  InputLabel,
  MenuItem,
  Select,
  SelectChangeEvent,
  ToggleButton,
  ToggleButtonGroup,
} from '@mui/material'
import { useCallback, useEffect, useMemo, useState } from 'react'

interface MultipleLocationsSelectProps {
  selectedLocations: Location[]
  setLocations: (locations: Location[]) => void
  center?: [number, number]
  zoom?: number
  mapHeight?: number | string
  route?: number[][]
  removeRouteSelect?: boolean
  focusedLocation?: Location | null
  highlightedLocationId?: number
  showSelectionModeToggle?: boolean
  size?: 'small' | 'medium'
  fillAvailableHeight?: boolean
}

const MultipleLocationsSelect = ({
  selectedLocations,
  setLocations,
  removeRouteSelect,
  focusedLocation,
  highlightedLocationId,
  mapHeight,
  showSelectionModeToggle = false,
  size = 'medium',
  fillAvailableHeight = false,
}: MultipleLocationsSelectProps) => {
  const { data: routesData } = useGetRoute({ expand: 'routeLocations' })
  const { data: locationsData } = useGetLocationLatestVersionOfAllLocations()

  const routes = useMemo(() => routesData?.value || [], [routesData])
  const locations = useMemo(
    () => locationsData?.value || [],
    [locationsData]
  ) as Location[]

  const [selectedLocation, setSelectedLocation] = useState<Location>()
  const [selectedRoute, setSelectedRoute] = useState<Route>()
  const [selectionMode, setSelectionMode] = useState<'route' | 'location'>(
    'route'
  )
  const [filters, setFilters] = useState<Filters>({})

  useEffect(() => {
    if (focusedLocation) {
      setSelectedLocation(focusedLocation)
    }
  }, [focusedLocation])

  const updateFilters = useCallback((newFilters: Partial<Filters>) => {
    setFilters((prevFilters) => ({ ...prevFilters, ...newFilters }))
  }, [])

  const filteredLocations = useMemo(() => {
    return locations.filter(
      (loc) =>
        (!filters.areaId || loc.areas?.includes(filters.areaId)) &&
        (!filters.regionId || loc.regionId === filters.regionId) &&
        (!filters.locationTypeId ||
          loc.locationTypeId === filters.locationTypeId) &&
        (!filters.measureTypeId ||
          loc.charts?.includes(filters.measureTypeId)) &&
        (!filters.jurisdictionId ||
          loc.jurisdictionId === filters.jurisdictionId)
    )
  }, [locations, filters])

  const onRouteChange = (e: SelectChangeEvent<number>) => {
    const route = routes?.find((r) => r.id === e.target.value)
    setSelectedRoute(route)
  }

  const onAddRoute = async () => {
    if (!selectedRoute?.routeLocations) return

    const routeLocs = selectedRoute.routeLocations
      .map((rl) =>
        locations.find((l) => l.locationIdentifier === rl.locationIdentifier)
      )
      .filter((l): l is Location => Boolean(l))
    const newLocations = routeLocs.filter(
      (loc) => !selectedLocations.some((sel) => sel.id === loc.id)
    )
    if (newLocations.length > 0) {
      const locationwithApproaches =
        await getLocationWithApproaches(newLocations)
      if (locationwithApproaches) {
        setLocations([...selectedLocations, ...locationwithApproaches])
      }
    }
  }

  const onAddLocation = async () => {
    if (
      selectedLocation &&
      !selectedLocations.some((loc) => loc.id === selectedLocation.id)
    ) {
      const locationwithApproaches = await getLocationWithApproaches([
        selectedLocation,
      ])
      if (locationwithApproaches) {
        setLocations([...selectedLocations, locationwithApproaches[0]])
      }
    }
  }

  const handleLocationInputChange = (
    _: React.SyntheticEvent,
    value: Location | null
  ) => {
    if (value) {
      setSelectedLocation(value)
    }
  }

  return (
    <Box
      sx={
        fillAvailableHeight
          ? {
              height: '100%',
              minHeight: 0,
              display: 'flex',
              flexDirection: 'column',
            }
          : undefined
      }
    >
      {!removeRouteSelect && showSelectionModeToggle && (
        <ToggleButtonGroup
          exclusive
          size="small"
          value={selectionMode}
          onChange={(_, mode: 'route' | 'location' | null) => {
            if (mode) setSelectionMode(mode)
          }}
          aria-label="Location selection method"
          sx={{
            mb: 1.5,
            width: 168,
            '& .MuiToggleButton-root': {
              flex: 1,
              py: 0.5,
              textTransform: 'none',
              fontWeight: 500,
            },
          }}
        >
          <ToggleButton value="route">Route</ToggleButton>
          <ToggleButton value="location">Location</ToggleButton>
        </ToggleButtonGroup>
      )}
      {!removeRouteSelect &&
        (!showSelectionModeToggle || selectionMode === 'route') && (
          <>
            <Box
              sx={{
                display: 'flex',
                flexDirection: 'row',
                alignItems: 'center',
                mb: 1.5,
              }}
            >
              <FormControl fullWidth size={size}>
                {!showSelectionModeToggle && (
                  <InputLabel htmlFor="route-select">Route</InputLabel>
                )}
                <Select
                  label={showSelectionModeToggle ? undefined : 'Route'}
                  variant="outlined"
                  fullWidth
                  displayEmpty={showSelectionModeToggle}
                  value={selectedRoute?.id || ''}
                  onChange={onRouteChange}
                  renderValue={() =>
                    !selectedRoute && showSelectionModeToggle ? (
                      <Box component="span" sx={{ color: 'text.secondary' }}>
                        Select a route
                      </Box>
                    ) : (
                      selectedRoute?.name
                    )
                  }
                  inputProps={{
                    id: 'route-select',
                    'aria-label': showSelectionModeToggle ? 'Route' : undefined,
                  }}
                >
                  {routes?.map((route) => (
                    <MenuItem key={route.id} value={route.id}>
                      {route.name}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
              <Button
                variant="contained"
                size={size}
                startIcon={<AddIcon />}
                onClick={onAddRoute}
                sx={{
                  ml: 1,
                  minWidth: 108,
                  whiteSpace: 'nowrap',
                  textTransform: 'none',
                  fontWeight: 600,
                }}
              >
                Add Route
              </Button>
            </Box>
          </>
        )}
      {(!showSelectionModeToggle ||
        removeRouteSelect ||
        selectionMode === 'location') && (
        <>
          <Box
            sx={{
              display: 'flex',
              flexDirection: 'row',
              alignItems: 'center',
              mb: 1.5,
            }}
          >
            <Box
              sx={{
                flex: 1,
                '& .MuiAutocomplete-root': { mb: 0 },
              }}
            >
              <LocationInput
                location={selectedLocation}
                locations={filteredLocations}
                handleChange={handleLocationInputChange}
                filters={filters}
                size={size}
                label={showSelectionModeToggle ? '' : 'Location'}
                placeholder={
                  showSelectionModeToggle ? 'Select a location' : undefined
                }
              />
            </Box>
            <Button
              variant="contained"
              size={size}
              startIcon={<AddIcon />}
              onClick={onAddLocation}
              sx={{
                ml: 1,
                minWidth: 124,
                whiteSpace: 'nowrap',
                textTransform: 'none',
                fontWeight: 600,
              }}
              disabled={!selectedLocation}
            >
              Add Location
            </Button>
          </Box>
        </>
      )}
      <Box
        sx={
          fillAvailableHeight
            ? {
                flex: 1,
                minHeight: mapHeight ?? 300,
              }
            : undefined
        }
      >
        <SelectLocationMap
          location={selectedLocation || null}
          setLocation={setSelectedLocation}
          locations={locations}
          filteredLocations={filteredLocations}
          mapHeight={fillAvailableHeight ? '100%' : (mapHeight ?? 300)}
          filters={filters}
          updateFilters={updateFilters}
          highlightedLocationId={highlightedLocationId}
        />
      </Box>
    </Box>
  )
}

export default MultipleLocationsSelect

export const getLocationWithApproaches = async (locations: Location[]) => {
  const locationsWithApproaches = await Promise.all(
    locations
      .filter((loc) => loc?.id)
      .map((loc) => getLocationFromKey(loc.id, { expand: 'approaches' }))
  )
  return locationsWithApproaches.map((res) => res.value[0])
}
