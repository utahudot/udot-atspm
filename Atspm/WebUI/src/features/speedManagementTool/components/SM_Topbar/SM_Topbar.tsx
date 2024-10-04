import ViolationsThresholdPopup from '@/features/speedManagementTool/components/RoutesToggle/ViolationsThresholdPopup'
import AnalysisPeriodOptionsPopup from '@/features/speedManagementTool/components/SM_Topbar/AnalysisPeriodOptionsPopup'
import DateRangeOptionsPopup from '@/features/speedManagementTool/components/SM_Topbar/DateRangeOptionsPopup'
import FiltersButton from '@/features/speedManagementTool/components/SM_Topbar/Filters'
import { LoadingButton } from '@mui/lab'
import { Box } from '@mui/material'
import DaysOfWeekOptionsPopup from './DaysOfWeekOptionsPopup'
import GeneralOptionsPopup from './GeneralOptionsPopup'

interface TopBarProps {
  handleOptionClick: () => void
  isLoading: boolean
  isRequestChanged: boolean
}

export default function SM_TopBar({
  handleOptionClick,
  isLoading,
  isRequestChanged,
}: TopBarProps) {
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
