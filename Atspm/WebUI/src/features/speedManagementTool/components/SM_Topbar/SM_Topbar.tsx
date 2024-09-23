import { useRoutes } from '@/features/speedManagementTool/api/getRoutes'
import ViolationsThresholdPopup from '@/features/speedManagementTool/components/RoutesToggle/ViolationsThresholdPopup'
import AnalysisPeriodOptionsPopup from '@/features/speedManagementTool/components/SM_Topbar/AnalysisPeriodOptionsPopup'
import DateRangeOptionsPopup from '@/features/speedManagementTool/components/SM_Topbar/DateRangeOptionsPopup'
import FiltersButton from '@/features/speedManagementTool/components/SM_Topbar/Filters'
import { DataSource } from '@/features/speedManagementTool/enums'
import useStore from '@/features/speedManagementTool/speedManagementStore'
import { LoadingButton } from '@mui/lab'
import { Box } from '@mui/material'
import { useEffect, useState } from 'react'
import DaysOfWeekOptionsPopup from './DaysOfWeekOptionsPopup'
import GeneralOptionsPopup from './GeneralOptionsPopup'

export default function TopBar() {
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

  const handleOptionClick = () => {
    fetchRoutes()
    setSubmittedRouteSpeedRequest(routeSpeedRequest)
    if (routeSpeedRequest.sourceId === DataSource.ATSPM) {
      setMediumMin(80)
      setMediumMax(300)
      setSliderMax(500)
    } else if (routeSpeedRequest.sourceId === DataSource.PeMS) {
      setMediumMin(150)
      setMediumMax(400)
      setSliderMax(600)
    }
  }

  useEffect(() => {
    const change =
      JSON.stringify(routeSpeedRequest) !==
      JSON.stringify(submittedRouteSpeedRequest)

    setIsRequestChanged(change)
  }, [routeSpeedRequest, submittedRouteSpeedRequest])

  const [isRequestChanged, setIsRequestChanged] = useState(false)
  // const [selectedRoute, setSelectedRoute] =
  //   useState<SpeedManagementRoute | null>(null)

  // const handleRouteChange = (
  //   _: React.SyntheticEvent,
  //   value: SpeedManagementRoute | null
  // ) => {
  //   setSelectedRoute(value)
  // }

  return (
    <Box
      sx={{
        display: 'flex',
        padding: 2,
        gap: 2,
        alignItems: 'center',
        backgroundColor: 'background.paper',
        border: '1px solid',
        borderColor: 'divider',
      }}
    >
      {/* <Box sx={{ flexGrow: 1 }}>
        <SearchRoutesInput
          route={selectedRoute}
          routes={routes}
          handleChange={handleRouteChange}
        />
      </Box> */}
      <Box sx={{ display: 'flex', gap: 2 }}>
        <GeneralOptionsPopup />
        <ViolationsThresholdPopup />
        <DateRangeOptionsPopup />
        <DaysOfWeekOptionsPopup />
        <AnalysisPeriodOptionsPopup />
        <FiltersButton />
        <LoadingButton
          variant="contained"
          loading={isLoading}
          onClick={handleOptionClick}
          disabled={!isRequestChanged}
          sx={{ textTransform: 'none' }}
        >
          Update Routes
        </LoadingButton>
      </Box>
    </Box>
  )
}
