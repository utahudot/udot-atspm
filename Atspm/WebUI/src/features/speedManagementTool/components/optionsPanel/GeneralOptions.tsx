import { StyledComponentHeader } from '@/components/HeaderStyling/StyledComponentHeader'
import { DataSource } from '@/features/speedManagementTool/enums'
import useStore from '@/features/speedManagementTool/speedManagementStore'
import { Box, ToggleButton, ToggleButtonGroup } from '@mui/material'

export default function GeneralOptions() {
  const { routeSpeedRequest, setRouteSpeedRequest } = useStore()

  const handleDataSourceChange = (
    _: React.MouseEvent<HTMLElement>,
    newDataSource: DataSource
  ) => {
    if (newDataSource !== null) {
      setRouteSpeedRequest({
        ...routeSpeedRequest,
        sourceId: newDataSource,
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
    <>
      <StyledComponentHeader header={'Data Source'} />
      <Box padding={'10px'}>
        <Box display="flex" alignItems={'center'}>
          <ToggleButtonGroup
            value={routeSpeedRequest.sourceId}
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
            <ToggleButton
              value={DataSource.ATSPM}
              sx={{ textTransform: 'none' }}
            >
              ATSPM
            </ToggleButton>
            <ToggleButton
              value={DataSource.PeMS}
              sx={{ textTransform: 'none' }}
            >
              PeMS
            </ToggleButton>
          </ToggleButtonGroup>
        </Box>
      </Box>
    </>
  )
}
