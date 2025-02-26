import {
  getLocationApproachesFromKey,
  useGetLocationLocationsForSearch,
  useGetRoute,
} from '@/api/config/aTSPMConfigurationApi'
import {
  SearchLocation as Location,
  Route,
} from '@/api/config/aTSPMConfigurationApi.schemas'
import { Filters } from '@/features/locations/components/selectLocation'
import LocationInput from '@/features/locations/components/selectLocation/LocationInput'
import SelectLocationMap from '@/features/locations/components/selectLocationMap'
import { TspLocation } from '@/pages/reports/transit-signal-priority'
import AddIcon from '@mui/icons-material/Add'
import {
  Box,
  Button,
  FormControl,
  InputLabel,
  MenuItem,
  Select,
  SelectChangeEvent,
} from '@mui/material'
import { useCallback, useMemo, useState } from 'react'

interface MultipleLocationsSelectProps {
  selectedLocations: TspLocation[]
  setLocations: (locations: Location[]) => void
  center?: [number, number]
  zoom?: number
  mapHeight?: number | string
  route?: number[][]
}

const MultipleLocationsSelect = ({
  selectedLocations,
  setLocations,
}: MultipleLocationsSelectProps) => {
  const { data: routesData } = useGetRoute({ expand: 'routeLocations' })
  const { data: locationsData } = useGetLocationLocationsForSearch()

  const routes = useMemo(() => routesData?.value || [], [routesData])
  const locations = useMemo(
    () => locationsData?.value || [],
    [locationsData]
  ) as Location[]

  const [selectedLocation, setSelectedLocation] = useState<TspLocation>()
  const [selectedRoute, setSelectedRoute] = useState<Route>()
  const [filters, setFilters] = useState<Filters>({})

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
      const newLocationsWithApproaches =
        await addApproachesToLocations(newLocations)
      setLocations([...selectedLocations, ...newLocationsWithApproaches])
    }
  }

  const onAddLocation = async () => {
    if (
      selectedLocation &&
      !selectedLocations.some((loc) => loc.id === selectedLocation.id)
    ) {
      const selectedLocationWithApproaches = await addApproachesToLocations([
        selectedLocation,
      ])
      setLocations([...selectedLocations, ...selectedLocationWithApproaches])
    }
  }

  const handleLocationInputChange = (
    _: React.SyntheticEvent,
    value: TspLocation | null
  ) => {
    if (value) {
      setSelectedLocation(value)
    }
  }

  return (
    <Box>
      <Box
        sx={{
          display: 'flex',
          flexDirection: 'row',
          alignItems: 'center',
          mb: 2,
        }}
      >
        <FormControl fullWidth>
          <InputLabel htmlFor="route-select">Route</InputLabel>
          <Select
            label="Route"
            variant="outlined"
            fullWidth
            value={selectedRoute?.id || ''}
            onChange={onRouteChange}
            inputProps={{ id: 'route-select' }}
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
          startIcon={<AddIcon />}
          onClick={onAddRoute}
          sx={{ ml: 2, width: 100 }}
        >
          Add
        </Button>
      </Box>
      <Box
        sx={{
          display: 'flex',
          flexDirection: 'row',
          alignItems: 'center',
          mb: 2,
        }}
      >
        <Box sx={{ flex: 1 }}>
          <LocationInput
            location={selectedLocation}
            locations={filteredLocations}
            handleChange={handleLocationInputChange}
            filters={filters}
          />
        </Box>
        <Button
          variant="contained"
          startIcon={<AddIcon />}
          onClick={onAddLocation}
          sx={{ ml: 2 }}
        >
          Add
        </Button>
      </Box>
      <Box>
        <SelectLocationMap
          location={selectedLocation || null}
          setLocation={setSelectedLocation}
          locations={locations}
          filteredLocations={filteredLocations}
          mapHeight={300}
          filters={filters}
          updateFilters={updateFilters}
        />
      </Box>
    </Box>
  )
}

export default MultipleLocationsSelect

const addApproachesToLocations = async (locations: Location[]) => {
  const updatedLocations = await Promise.all(
    locations.map(async (loc) => {
      try {
        const approaches = await getLocationApproachesFromKey(loc.id)
        return { ...loc, approaches: approaches.value, designatedPhases: [] }
      } catch (error) {
        console.error(
          `Failed to fetch approaches for location ${loc.id}:`,
          error
        )
        return { ...loc, approaches: [] }
      }
    })
  )
  return updatedLocations
}
