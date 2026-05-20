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
  aggregationOptions,
  dateMin,
  dateMax,
  dateMarks,
  dateRange,
  setDateRange,
  hourRange,
  setHourRange,
  timeUnit,
}: {
  aggregation: Aggregation
  setAggregation: (v: Aggregation) => void
  aggregationOptions: Aggregation[]
  dateMin: number
  dateMax: number
  dateMarks: number[]
  dateRange: [number, number]
  setDateRange: (v: [number, number]) => void
  hourRange: [number, number]
  setHourRange: (v: [number, number]) => void
  timeUnit?: string
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
  const showHourFilter = timeUnit === 'Hour'
  const useDateMarks = dateMarks.length > 0
  const dateSliderMarks = useDateMarks
    ? dateMarks.map((value) => ({ value }))
    : [{ value: dateMin }, { value: dateMax }]

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

      {aggregationOptions.length > 1 && (
        <div>
          <Typography variant="subtitle2" sx={{ mb: 0.5 }}>
            Aggregation method:
          </Typography>
          <FormControl size="small" fullWidth>
            <Select
              value={aggregation}
              onChange={(e) => setAggregation(e.target.value as Aggregation)}
            >
              {aggregationOptions.map((option) => (
                <MenuItem key={option} value={option}>
                  {option}
                </MenuItem>
              ))}
            </Select>
          </FormControl>
        </div>
      )}

      <div>
        <Typography variant="subtitle2">
          {useDateMarks ? 'Select Period Range' : 'Select Date Range'}
        </Typography>
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
            step={useDateMarks ? null : ONE_DAY}
            disableSwap
            size="small"
            marks={dateSliderMarks}
            sx={{ mt: 1, '& .MuiSlider-markLabel': { display: 'none' } }}
          />
        </Box>
      </div>

      {showHourFilter && (
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
      )}

      <Box sx={{ mt: 'auto' }}>
        <Button
          variant="outlined"
          size="small"
          onClick={() => {
            setAggregation(aggregationOptions[0])
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
