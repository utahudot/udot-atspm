import Subtitle from '@/features/speedManagementTool/components/subtitle'
import useStore, {
  DataSource,
} from '@/features/speedManagementTool/speedManagementStore'
import {
  Box,
  InputAdornment,
  TextField,
  ToggleButton,
  ToggleButtonGroup,
} from '@mui/material'

export default function GeneralOptions() {
  const { routeSpeedRequest, setRouteSpeedRequest } = useStore()

  const handleDataSourceChange = (
    _: React.MouseEvent<HTMLElement>,
    newDataSource: DataSource
  ) => {
    if (newDataSource !== null) {
      setRouteSpeedRequest({
        ...routeSpeedRequest,
        dataSource: newDataSource,
      })
    }
  }

  const handleViolationsThresholdChange = (
    event: React.ChangeEvent<HTMLInputElement>
  ) => {
    setRouteSpeedRequest({
      ...routeSpeedRequest,
      violationThreshold: parseInt(event.target.value),
    })
  }

  return (
    <Box padding={'10px'}>
      <Box sx={{ marginBottom: '10px' }}>
        <Subtitle>Data Source and Violations</Subtitle>
      </Box>
      <Box display="flex" alignItems={'center'}>
        <ToggleButtonGroup
          value={routeSpeedRequest.dataSource}
          size="small"
          exclusive
          onChange={handleDataSourceChange}
        >
          <ToggleButton
            value={DataSource.ClearGuide}
            sx={{ textTransform: 'none' }}
          >
            ClearGuide
          </ToggleButton>
          <ToggleButton value={DataSource.ATSPM} sx={{ textTransform: 'none' }}>
            ATSPM
          </ToggleButton>
          <ToggleButton value={DataSource.PeMS} sx={{ textTransform: 'none' }}>
            PeMS
          </ToggleButton>
        </ToggleButtonGroup>

        <Box sx={{ padding: '10px', width: '140px' }}>
          <TextField
            label="Violations Threshold"
            variant="outlined"
            size="small"
            type="number"
            value={routeSpeedRequest.violationThreshold}
            onChange={handleViolationsThresholdChange}
            InputProps={{
              endAdornment: <InputAdornment position="end">%</InputAdornment>,
            }}
          />
        </Box>
      </Box>
    </Box>
  )
}
