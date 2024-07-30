import SelectTimeSpan from '@/components/selectTimeSpan'
import {
  Box,
  FormControl,
  InputLabel,
  MenuItem,
  Paper,
  Select,
  TextField,
} from '@mui/material'
import { ToolType } from '../../common/types'
import { TSHistoricHandler } from './handlers/handlers'

interface Props {
  handler: TSHistoricHandler
}

export const HistoricOptionsComponent = ({ handler }: Props) => {
  return (
    <Box display="flex" gap={2}>
      <Paper sx={{ p: 3, width: '350px' }}>
        <FormControl fullWidth sx={{ mb: 2 }}>
          <InputLabel htmlFor="route-input">Route</InputLabel>
          <Select
            label="Route"
            variant="outlined"
            fullWidth
            sx={{ mb: 2 }}
            onChange={(e) => handler.setRouteId(e.target.value)}
            value={
              handler.routes?.find(
                (route) => route.id === Number.parseInt(handler.routeId)
              )?.id || ''
            }
            inputProps={{ id: 'route-input' }}
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
        <TextField
          label="Speed Limit (mph)"
          variant="outlined"
          fullWidth
          sx={{ mb: 2 }}
          onChange={(e) => handler.setSpeedLimit(parseInt(e.target.value))}
        />
      </Paper>
      <Paper sx={{ p: 3 }}>
        <SelectTimeSpan
          startDateTime={handler.startDateTime}
          endDateTime={handler.endDateTime}
          changeStartDate={handler.changeStartDate}
          changeEndDate={handler.changeEndDate}
          chartType={ToolType.TimeSpaceHistoric}
          calendarLocation="right"
        />
      </Paper>
    </Box>
  )
}
