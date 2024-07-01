import { ExpandLocationHandler } from '@/features/data/aggregate/handlers/expandLocationHandler'
import SelectLocation from '@/features/locations/components/selectLocation'
import { Location } from '@/features/locations/types'
import {
  Box,
  Button,
  FormControl,
  InputLabel,
  MenuItem,
  Select,
  Skeleton,
} from '@mui/material'
import { useState } from 'react'
import { LocationHandler } from '../handlers/locationHandler'
import { RouteHandler } from '../handlers/routeHandler'
import SelectedLocationsDisplay from './SelectedLocationsDisplay'

type Handler = RouteHandler & LocationHandler & Partial<ExpandLocationHandler>

interface RouteSelectProps {
  handler: Handler
  hasLocationMap?: boolean
  hasLocationNames?: boolean
}

export const RouteSelect = ({
  handler,
  hasLocationMap,
  hasLocationNames,
}: RouteSelectProps) => {
  const [selectedLocation, setSelectedLocation] = useState<Location | null>(
    null
  )
  if (!handler.routes.length || handler.routes === null) {
    return
  }

  return (
    <Box>
      <FormControl fullWidth sx={{ mb: 2 }}>
        <InputLabel htmlFor="route-select">Route Id</InputLabel>

        <Select
          label="Route"
          variant="outlined"
          fullWidth
          sx={{ mb: 2 }}
          value={
            handler.routes.find(
              (r) => r.id.toString() === handler.routeId.toString()
            )?.id || ''
          }
          onChange={(e) => handler.changeRouteId(e.target.value as string)}
          inputProps={{ id: 'route-select' }}
        >
          {handler.routes.map(
            (route: { name: string; id: number }, index: number) => {
              return (
                <MenuItem key={index} value={route.id}>
                  {route.name}
                </MenuItem>
              )
            }
          )}
        </Select>
      </FormControl>
      <Box sx={{ display: 'flex', flexDirection: 'column' }}>
        <Box
          sx={{
            justifyContent: 'center',
            alignItems: 'center',
          }}
        >
          <Box>
            {hasLocationMap && (
              <Box>
                <Button
                  variant="contained"
                  color="success"
                  onClick={() =>
                    selectedLocation && handler.changeLocation(selectedLocation)
                  }
                  sx={{ marginBottom: 1 }}
                >
                  Add Location
                </Button>
                <SelectLocation
                  location={selectedLocation}
                  setLocation={setSelectedLocation}
                />
                <SelectedLocationsDisplay
                  handler={handler as ExpandLocationHandler}
                />
              </Box>
            )}
          </Box>
          {hasLocationNames && (
            <Skeleton
              sx={{ marginTop: '-10px', paddingBottom: '-150px' }}
              width={396}
              height={400}
            />
          )}
        </Box>
      </Box>
    </Box>
  )
}
