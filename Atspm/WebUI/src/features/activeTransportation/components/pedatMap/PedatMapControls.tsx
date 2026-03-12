import { Aggregation } from '@/features/activeTransportation/components/pedatMap/PedatMap'
import {
  Box,
  Button,
  FormControl,
  MenuItem,
  Paper,
  Select,
  Slider,
  Typography,
} from '@mui/material'

const ControlsPanel = ({
  aggregation,
  setAggregation,
  dateMin,
  dateMax,
  dateRange,
  setDateRange,
  hourRange,
  setHourRange,
}: {
  aggregation: Aggregation
  setAggregation: (v: Aggregation) => void
  dateMin: number
  dateMax: number
  dateRange: [number, number]
  setDateRange: (v: [number, number]) => void
  hourRange: [number, number]
  setHourRange: (v: [number, number]) => void
}) => {
  const iso = (t: number) => {
    const d = new Date(t)
    const y = d.getFullYear()
    const m = String(d.getMonth() + 1).padStart(2, '0')
    const day = String(d.getDate()).padStart(2, '0')
    return `${y}-${m}-${day}`
  }
  const fmtHour = (h: number) => `${h % 12 || 12} ${h < 12 ? 'AM' : 'PM'}`

  const ONE_DAY = 24 * 60 * 60 * 1000

  return (
    <Paper
      elevation={3}
      sx={{
        height: '100%',
        p: 2,
        display: 'flex',
        flexDirection: 'column',
        gap: 2,
        minWidth: 320,
      }}
    >
      <Typography variant="h6">Filters</Typography>

      <div>
        <Typography variant="subtitle2" sx={{ mb: 0.5 }}>
          Aggregation method:
        </Typography>
        <FormControl size="small" fullWidth>
          <Select
            value={aggregation}
            onChange={(e) => setAggregation(e.target.value as Aggregation)}
          >
            <MenuItem value="Average Hour">Average Hour</MenuItem>
            <MenuItem value="Average Daily">Average Daily</MenuItem>
            <MenuItem value="Total">Total</MenuItem>
          </Select>
        </FormControl>
      </div>

      <div>
        <Typography variant="subtitle2">Select Date Range</Typography>
        <Box sx={{ px: 2 }}>
          <Box
            sx={{
              display: 'flex',
              justifyContent: 'space-between',
              fontSize: 12,
              color: 'text.secondary',
              mt: 0.5,
              '& span': { whiteSpace: 'nowrap' },
            }}
          >
            <span>{iso(dateRange[0])}</span>
            <span>{iso(dateRange[1])}</span>
          </Box>
          <Slider
            value={dateRange}
            onChange={(_, v) => {
              const nv = v as number[]
              setDateRange([nv[0], nv[1]]) // clone for new ref
            }}
            min={dateMin}
            max={dateMax}
            step={ONE_DAY}
            disableSwap
            size="small"
            marks={[{ value: dateMin }, { value: dateMax }]}
            sx={{ mt: 1, '& .MuiSlider-markLabel': { display: 'none' } }}
          />
        </Box>
      </div>

      <div>
        <Typography variant="subtitle2">Select Hour Range</Typography>
        <Box sx={{ px: 2 }}>
          <Box
            sx={{
              display: 'flex',
              justifyContent: 'space-between',
              fontSize: 12,
              color: 'text.secondary',
              mt: 0.5,
              '& span': { whiteSpace: 'nowrap' },
            }}
          >
            <span>{fmtHour(hourRange[0])}</span>
            <span>{fmtHour(hourRange[1])}</span>
          </Box>
          <Slider
            value={hourRange}
            onChange={(_, v) => {
              const nv = v as number[]
              setHourRange([nv[0], nv[1]]) // clone for new ref
            }}
            min={0}
            max={23}
            step={1}
            disableSwap
            size="small"
            marks={[{ value: 0 }, { value: 23 }]}
            sx={{ mt: 1, '& .MuiSlider-markLabel': { display: 'none' } }}
          />
        </Box>
      </div>

      <Box sx={{ mt: 'auto' }}>
        <Button
          variant="outlined"
          size="small"
          onClick={() => {
            setAggregation('Average Hour')
            setDateRange([dateMin, dateMax])
            setHourRange([0, 23])
          }}
        >
          RESET
        </Button>
      </Box>
    </Paper>
  )
}

export default ControlsPanel
