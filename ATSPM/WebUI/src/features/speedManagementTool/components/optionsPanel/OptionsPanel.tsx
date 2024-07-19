import { useRoutes } from '@/features/speedManagementTool/api/getRoutes'
import AnalysisPeriodOptions from '@/features/speedManagementTool/components/optionsPanel/AnalysisPeriodOptions'
import DateRangeOptions from '@/features/speedManagementTool/components/optionsPanel/DateRangeOptions'
import { DaysOfWeekOptions } from '@/features/speedManagementTool/components/optionsPanel/DayOfTheWeekOptions'
import GeneralOptions from '@/features/speedManagementTool/components/optionsPanel/GeneralOptions'
import useStore, {
  DataSource,
} from '@/features/speedManagementTool/speedManagementStore'
import { LoadingButton } from '@mui/lab'
import { Box, Divider } from '@mui/material'
import { useEffect, useState } from 'react'

function OptionsPanel() {
  const {
    routeSpeedRequest,
    submittedRouteSpeedRequest,
    setSubmittedRouteSpeedRequest,
    setMediumMax,
    setMediumMin,
    setSliderMax,
  } = useStore()
  const { refetch: fetchRoutes, isLoading } = useRoutes({
    options: submittedRouteSpeedRequest,
  })
  const [isRequestChanged, setIsRequestChanged] = useState(false)

  useEffect(() => {
    const change =
      JSON.stringify(routeSpeedRequest) !==
      JSON.stringify(submittedRouteSpeedRequest)
    setIsRequestChanged(change)
  }, [routeSpeedRequest, submittedRouteSpeedRequest])

  const handleOptionClick = () => {
    fetchRoutes()
    setSubmittedRouteSpeedRequest(routeSpeedRequest)
    if (routeSpeedRequest.dataSource === DataSource.ATSPM) {
      setMediumMin(80)
      setMediumMax(300)
      setSliderMax(500)
    } else if (routeSpeedRequest.dataSource === DataSource.PeMS) {
      setMediumMin(150)
      setMediumMax(400)
      setSliderMax(600)
    }
  }

  return (
    <Box sx={{ height: '100%', overflowY: 'auto' }}>
      <Box padding={'0px'}>
        <GeneralOptions />
        <Divider />
        <DateRangeOptions />
        <Divider />
        <DaysOfWeekOptions />
        <Divider />
        <AnalysisPeriodOptions />
        <Divider />
        <Box display={'flex'} justifyContent={'center'} padding={'10px'}>
          <LoadingButton
            variant="contained"
            loading={isLoading}
            onClick={handleOptionClick}
            disabled={!isRequestChanged}
          >
            Update Routes
          </LoadingButton>
        </Box>
      </Box>
    </Box>
  )
}

export default OptionsPanel
