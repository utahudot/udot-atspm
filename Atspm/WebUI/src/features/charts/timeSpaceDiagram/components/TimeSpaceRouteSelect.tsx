import RouteChecker from '@/features/charts/timeSpaceDiagram/components/RouteChecker'
import { useGetRouteWithExpandedLocations } from '@/features/routes/api/getRouteWithExpandedLocations'
import {
  Alert,
  Autocomplete,
  Box,
  FormControl,
  Paper,
  TextField,
  Typography,
} from '@mui/material'
import { useEffect } from 'react'
import { TSHistoricHandler } from './handlers/handlers'

interface Props {
  handler: TSHistoricHandler
}

interface RouteRow {
  locationIdentifier: string
  primaryPhaseDescription: string | null
  primaryMph: number | null
  opposingPhaseDescription: string | null
  opposingMph: number | null
  distance: number | null
  order: number
}

export const TimeSpaceRouteSelect = ({ handler }: Props) => {
  const { data: routeData, refetch } = useGetRouteWithExpandedLocations({
    routeId: handler.routeId,
    includeLocationDetail: true,
  })

  useEffect(() => {
    if (handler.routeId) refetch()
  }, [handler.routeId, refetch])

  if (routeData && routeData.routeLocations === undefined) return null

  const routeValuesToCheck: RouteRow[] =
    routeData?.routeLocations
      .map((loc) => {
        const primary = loc.approaches.find(
          (a) => a.protectedPhaseNumber === loc.primaryPhase
        )
        const opposing = loc.approaches.find(
          (a) => a.protectedPhaseNumber === loc.opposingPhase
        )

        return {
          locationIdentifier: loc.locationIdentifier,
          primaryPhaseDescription: primary?.description ?? null,
          primaryMph: primary?.mph ?? null,
          opposingPhaseDescription: opposing?.description ?? null,
          opposingMph: opposing?.mph ?? null,
          distance: loc.nextLocationDistance?.distance ?? null,
          order: loc.order,
        }
      })
      .sort((a, b) => a.order - b.order) || []

  const allDistancesValid = routeValuesToCheck.every(
    (row, idx) => idx === routeValuesToCheck.length - 1 || row.distance !== null
  )

  const someMphMissing = routeValuesToCheck.some(
    (r) => r.primaryMph === null || r.opposingMph === null
  )

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
            Some approaches are missing speed limits. Please add a default one.
          </Alert>
          <Box sx={{ display: 'flex', alignItems: 'center', my: 1 }}>
            <TextField
              label="Speed Limit"
              sx={{ maxWidth: 150 }}
              onChange={(e) => handler.setSpeedLimit(+e.target.value)}
              size="small"
            />
            <Typography sx={{ marginX: '0.5rem' }}>mph</Typography>
          </Box>
        </>
      )
    }

    return <Alert severity="success">Ready to run</Alert>
  }

  return (
    <Paper
      sx={{
        p: 3,
        display: 'flex',
        minWidth: 600,
        flexDirection: 'column',
      }}
    >
      <FormControl sx={{ mb: 2 }}>
        <Autocomplete
          options={handler.routes}
          getOptionLabel={(o) => o.name}
          value={
            handler.routes.find(
              (r) => r.id === Number.parseInt(handler.routeId)
            ) || null
          }
          onChange={(_, v) => handler.setRouteId(v ? String(v.id) : '')}
          renderInput={(params) => (
            <TextField {...params} label="Route Select" />
          )}
        />
      </FormControl>

      {handler.routeId && (
        <>
          <Box
            sx={{
              maxHeight: 350,
              overflowY: 'auto',
              outline: '1px solid #ccc',
              mb: 2,
              flexGrow: 1,
            }}
          >
            <RouteChecker data={routeValuesToCheck} />
          </Box>
          {renderContent()}
        </>
      )}
    </Paper>
  )
}

export default TimeSpaceRouteSelect
