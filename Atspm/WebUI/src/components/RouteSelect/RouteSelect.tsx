import RouteChecker from '@/features/charts/timeSpaceDiagram/components/RouteChecker'
import { ExpandLocationHandler } from '@/features/data/aggregate/handlers/expandLocationHandler'
import SelectLocation from '@/features/locations/components/selectLocation'
import { Location } from '@/features/locations/types'
import { useGetRouteWithExpandedLocations } from '@/features/routes/api/getRouteWithExpandedLocations'
import {
  Box,
  Button,
  FormControl,
  InputLabel,
  MenuItem,
  Select,
  Skeleton,
} from '@mui/material'
import { useEffect, useState } from 'react'
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

  const { data: routeData, refetch } = useGetRouteWithExpandedLocations({
    routeId: handler.routeId,
    includeLocationDetail: true,
  })

  useEffect(() => {
    if (handler.routeId) refetch()
  }, [handler.routeId, refetch])

  const routeValuesToCheck =
    routeData?.routeLocations
      .map((location) => {
        const approach = location.approaches.find((approach) => {
          return approach.protectedPhaseNumber === location.primaryPhase
        })

        return {
          locationIdentifier: location.locationIdentifier,
          approachDescription: approach ? approach.description : null,
          mph: approach ? approach.mph : null,
          distance: location?.nextLocationDistance?.distance ?? null,
          order: location.order,
        }
      })
      // sort so 1 comes before 2, etc.
      .sort((a, b) => a.order - b.order) || []

  if (!handler.routes.length || handler.routes === null) {
    return
  }

  return (
    <Box>
      <FormControl fullWidth sx={{ mb: 2 }}>
        <InputLabel htmlFor="route-select">Route</InputLabel>

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
      {handler.routeId && (
        <>
          <Box
            sx={{
              maxHeight: '350px',
              overflowY: 'auto',
              outline: '1px solid #ccc',
              marginBottom: 2,
              flexGrow: 1,
            }}
          >
            {routeValuesToCheck && <RouteChecker data={routeValuesToCheck} />}
          </Box>
        </>
      )}
    </Box>
  )
}
