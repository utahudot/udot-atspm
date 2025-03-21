import RouteChecker from '@/features/charts/timeSpaceDiagram/components/RouteChecker'
import { useGetRouteWithExpandedLocations } from '@/features/routes/api/getRouteWithExpandedLocations'
import {
  Alert,
  Autocomplete,
  Box,
  FormControl,
  InputAdornment,
  Paper,
  TextField,
} from '@mui/material'
import { useEffect } from 'react'
import { TSHistoricHandler } from './handlers/handlers'

interface Props {
  handler: TSHistoricHandler
}

export const TimeSpaceRouteSelect = ({ handler }: Props) => {
  const { data: routeData, refetch } = useGetRouteWithExpandedLocations({
    routeId: handler.routeId,
    includeLocationDetail: true,
  })

  useEffect(() => {
    if (handler.routeId) refetch()
  }, [handler.routeId, refetch])

  if (routeData && routeData?.routeLocations === undefined) return null

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

  const allDistancesValid = routeValuesToCheck.every((item, index) => {
    if (index === routeValuesToCheck.length - 1) return true
    return item.distance !== null
  })

  const someMphMissing = routeValuesToCheck.some((item) => item.mph === null)

  const renderContent = () => {
    if (routeValuesToCheck.length < 2) {
      return (
        <Alert severity="warning">
          Two or more locations are required to run a time-space diagram.
        </Alert>
      )
    }

    if (!allDistancesValid) {
      return (
        <Alert severity="warning">
          Please configure distances before running.
        </Alert>
      )
    }

    if (someMphMissing) {
      return (
        <>
          <Alert severity="warning" sx={{ my: 2 }}>
            Some locations are missing speed limits. Please add a default one.
          </Alert>
          <TextField
            label="Speed Limit"
            variant="outlined"
            fullWidth
            sx={{ mb: 2 }}
            onChange={(e) => handler.setSpeedLimit(parseInt(e.target.value))}
            InputProps={{
              endAdornment: <InputAdornment position="end">mph</InputAdornment>,
            }}
          />
        </>
      )
    }

    return <Alert severity="success">Ready to run</Alert>
  }

  return (
    <Paper
      sx={{
        p: 3,
        width: '450px',
        display: 'flex',
        flexDirection: 'column',
      }}
    >
      <FormControl fullWidth sx={{ mb: 2 }}>
        <Autocomplete
          options={handler.routes}
          getOptionLabel={(option) => option.name}
          value={
            handler.routes.find(
              (route) => route.id === Number.parseInt(handler.routeId)
            ) || null
          }
          onChange={(_, newValue) =>
            handler.setRouteId(newValue ? String(newValue.id) : '')
          }
          renderInput={(params) => (
            <TextField
              {...params}
              label="Route Select"
              variant="outlined"
              fullWidth
            />
          )}
        />
      </FormControl>
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
          <Box>{renderContent()}</Box>
        </>
      )}
    </Paper>
  )
}

export default TimeSpaceRouteSelect
