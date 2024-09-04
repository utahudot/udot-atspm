import { DataSource } from '@/features/speedManagementTool/enums'
import useStore from '@/features/speedManagementTool/speedManagementStore'
import { Box, ToggleButton, ToggleButtonGroup } from '@mui/material'
import OptionsPopupWrapper from './OptionsPopupWrapper'

export default function GeneralOptionsPopup() {
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

  const getDataSourceLabel = () => {
    switch (routeSpeedRequest.sourceId) {
      case DataSource.ATSPM:
        return 'ATSPM'
      case DataSource.PeMS:
        return 'PeMS'
      case DataSource.ClearGuide:
        return 'ClearGuide'
      default:
        return 'Select Data Source'
    }
  }

  return (
    <OptionsPopupWrapper
      label="data-source"
      getLabel={getDataSourceLabel}
      width={'130px'}
    >
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
    </OptionsPopupWrapper>
  )
}
