import { Location, Route } from '@/api/config/aTSPMConfigurationApi.schemas'
import { useLatestVersionOfAllLocations } from '@/features/locations/api'
import LocationInput from '@/features/locations/components/selectLocation/LocationInput'
import SelectLocationMap from '@/features/locations/components/selectLocationMap'
import { useGetRoutes } from '@/features/routes/api'
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
import { useState } from 'react'

interface MultipleLocationsSelectProps {
  selectedLocations: Location[]
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
  const [selectedLocation, setSelectedLocation] = useState<
    Location | undefined
  >(undefined)
  const [selectedRoute, setSelectedRoute] = useState<Route | undefined>(
    undefined
  )
  const { data: routesData } = useGetRoutes()
  const { data: locationsData } = useLatestVersionOfAllLocations()
  const routes = routesData?.value
  const locations = locationsData?.value || []

  const onRouteChange = (e: SelectChangeEvent<number>) => {
    const route = routes?.find((r) => r.id === e.target.value)
    setSelectedRoute(route)
  }

  const onAddRoute = () => {
    if (selectedRoute && selectedRoute.routeLocations) {
      const routeLocs = selectedRoute.routeLocations
        .map((rl) =>
          locations.find((l) => l.locationIdentifier === rl.locationIdentifier)
        )
        .filter((l): l is Location => Boolean(l))
      const newLocations = routeLocs.filter(
        (loc) => !selectedLocations.some((sel) => sel.id === loc.id)
      )
      if (newLocations.length > 0) {
        setLocations([...selectedLocations, ...newLocations])
      }
    }
  }

  const onAddLocation = () => {
    if (
      selectedLocation &&
      !selectedLocations.some((loc) => loc.id === selectedLocation.id)
    ) {
      setLocations([...selectedLocations, selectedLocation])
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
          sx={{ ml: 2 }}
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
          width: '100%',
        }}
      >
        <LocationInput
          location={selectedLocation}
          locations={locations}
          handleChange={handleLocationInputChange}
        />
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
          mapHeight={360}
        />
      </Box>
    </Box>
  )
}

export default MultipleLocationsSelect
