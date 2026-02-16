import DownloadIcon from '@mui/icons-material/Download'
import { Box, Button, Paper, Stack, Tab, Tabs } from '@mui/material'

import type { FilterOption } from './TurningMovementCountsFilters'

interface TurningMovementCountsTableToolbarProps {
  activeLaneType: string
  laneTypeTabOptions: FilterOption[]
  onLaneTypeChange: (value: string) => void
  onDownloadCsv: () => void
}

export default function TurningMovementCountsTableToolbar({
  activeLaneType,
  laneTypeTabOptions,
  onLaneTypeChange,
  onDownloadCsv,
}: TurningMovementCountsTableToolbarProps) {
  return (
    <Paper
      variant="outlined"
      sx={{
        mb: 2,
        borderRadius: 2,
        bgcolor: 'grey.50',
        borderColor: 'grey.200',
      }}
    >
      <Stack direction="row" justifyContent="space-between" sx={{ mb: 1 }}>
        <Tabs
          value={activeLaneType}
          onChange={(_, value) => onLaneTypeChange(value)}
          variant="scrollable"
          scrollButtons="auto"
          allowScrollButtonsMobile
        >
          {laneTypeTabOptions.map((option) => (
            <Tab
              key={option.value}
              value={option.value}
              label={option.label}
              sx={{ textTransform: 'none' }}
            />
          ))}
        </Tabs>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, pr: 2 }}>
          <Button
            variant="outlined"
            size="small"
            startIcon={<DownloadIcon />}
            onClick={onDownloadCsv}
          >
            Download CSV
          </Button>
        </Box>
      </Stack>
    </Paper>
  )
}
