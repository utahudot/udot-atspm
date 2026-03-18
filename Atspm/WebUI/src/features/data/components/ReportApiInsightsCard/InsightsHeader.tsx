import {
  FormControl,
  MenuItem,
  Select,
  Stack,
  ToggleButton,
  ToggleButtonGroup,
  Typography,
} from '@mui/material'
import { visuallyHidden } from '@mui/utils'

export type ViewKey = 'type' | 'time' | 'user' | 'agency'
export type SortBy = 'Name' | 'Amount'
export type GroupBy = 'day' | 'week' | 'month'
export type Metric = 'ReportsGenerated' | 'DataDownloaded'

export default function InsightsHeader({
  view,
  onViewChange,
  metric,
  onMetricChange,
  sortBy,
  onSortByChange,
  groupBy,
  onGroupByChange,
}: {
  titleLeft?: string
  view: ViewKey
  onViewChange: (v: ViewKey) => void
  metric: Metric
  onMetricChange: (v: Metric) => void
  sortBy: SortBy
  onSortByChange: (v: SortBy) => void
  groupBy: GroupBy
  onGroupByChange: (v: GroupBy) => void
}) {
  const isTime = view === 'time'

  return (
    <Stack spacing={2} sx={{ mb: 1 }}>
      <Stack
        direction={{ xs: 'column', lg: 'row' }}
        spacing={2}
        alignItems={{ xs: 'stretch', lg: 'center' }}
      >
        <Stack
          direction={{ xs: 'column', sm: 'row' }}
          spacing={2}
          alignItems={{ xs: 'flex-start', sm: 'center' }}
          sx={{ minWidth: 0 }}
        >
          <Typography variant="h4" sx={{ whiteSpace: 'nowrap' }}>
            Insights by
          </Typography>

          <ToggleButtonGroup
            exclusive
            size="small"
            value={view}
            onChange={(_, v) => {
              if (v) onViewChange(v)
            }}
            sx={{
              width: { xs: '100%', sm: 'auto' },
              flexWrap: 'wrap',
              '& .MuiToggleButton-root': {
                textTransform: 'none',
                px: 1.25,
                flex: { xs: '1 1 50%', sm: '0 0 auto' },
              },
            }}
          >
            <ToggleButton value="type">Type</ToggleButton>
            <ToggleButton value="time">Time</ToggleButton>
            <ToggleButton value="user">User</ToggleButton>
            <ToggleButton value="agency">Agency</ToggleButton>
          </ToggleButtonGroup>
        </Stack>

        <Stack
          direction={{ xs: 'column', sm: 'row' }}
          spacing={1}
          alignItems={{ xs: 'stretch', sm: 'center' }}
          sx={{ width: { xs: '100%', lg: 'auto' }, ml: { lg: 'auto' } }}
        >
          <Typography
            variant="body2"
            color="text.secondary"
            sx={{ whiteSpace: 'nowrap' }}
          >
            Show
          </Typography>

          <FormControl
            size="small"
            sx={{ width: { xs: '100%', sm: 'auto' }, minWidth: { sm: 200 } }}
          >
            <Typography component="label" sx={visuallyHidden} id="metric-label">
              Metric
            </Typography>
            <Select
              aria-labelledby="metric-label"
              value={metric}
              onChange={(e) => onMetricChange(e.target.value as Metric)}
            >
              <MenuItem value="ReportsGenerated">Reports generated</MenuItem>
              <MenuItem value="DataDownloaded">Data downloaded</MenuItem>
            </Select>
          </FormControl>

          <Typography
            variant="body2"
            color="text.secondary"
            sx={{ whiteSpace: 'nowrap' }}
          >
            {isTime ? 'grouped by' : 'sorted by'}
          </Typography>

          {isTime ? (
            <FormControl
              size="small"
              sx={{ width: { xs: '100%', sm: 'auto' }, minWidth: { sm: 150 } }}
            >
              <Typography
                component="label"
                sx={visuallyHidden}
                id="group-label"
              >
                Group by
              </Typography>
              <Select
                aria-labelledby="group-label"
                value={groupBy}
                onChange={(e) => onGroupByChange(e.target.value as GroupBy)}
              >
                <MenuItem value="day">Day</MenuItem>
                <MenuItem value="week">Week</MenuItem>
                <MenuItem value="month">Month</MenuItem>
              </Select>
            </FormControl>
          ) : (
            <FormControl
              size="small"
              sx={{ width: { xs: '100%', sm: 'auto' }, minWidth: { sm: 160 } }}
            >
              <Typography component="label" sx={visuallyHidden} id="sort-label">
                Sort
              </Typography>
              <Select
                aria-labelledby="sort-label"
                value={sortBy}
                onChange={(e) => onSortByChange(e.target.value as SortBy)}
              >
                <MenuItem value="Name">Name</MenuItem>
                <MenuItem value="Amount">Amount</MenuItem>
              </Select>
            </FormControl>
          )}
        </Stack>
      </Stack>
    </Stack>
  )
}
