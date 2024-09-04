import { StyledPaper } from '@/components/StyledPaper'
import WatchDogIssueTypeContainer from '@/features/charts/watchdogDashboard/components/WatchDogIssueTypeContainer'
import PlayArrowIcon from '@mui/icons-material/PlayArrow'
import { LoadingButton } from '@mui/lab'
import { useMediaQuery, useTheme } from '@mui/material'
import { startOfToday, startOfTomorrow, subDays, subYears } from 'date-fns'
import { useEffect, useState } from 'react'
import HorizontalDateInput from './HorizontalDateInputs'
import { useGetDetectionTypeGroup } from '../api/getDetectionTypeGroup'

const WatchdogSummaryReport = () => {
  const [startDateTime, setStartDateTime] = useState(
    subYears(startOfToday(), 1)
  )
  const [endDateTime, setEndDateTime] = useState(subDays(startOfTomorrow(), 1))
  const theme = useTheme()
  const isMobileView = useMediaQuery(theme.breakpoints.down('md'))

  const handleStartDateTimeChange = (date: Date) => {
    setStartDateTime(date)
  }

  const handleEndDateTimeChange = (date: Date) => {
    setEndDateTime(date)
  }


  return (
    <>
      <StyledPaper
        sx={{
          flexGrow: 1,
          maxWidth: '30rem',
          padding: 2,
        }}
      >
        <HorizontalDateInput
          startDateTime={startDateTime}
          endDateTime={endDateTime}
          changeStartDate={handleStartDateTimeChange}
          changeEndDate={handleEndDateTimeChange}
        />
      </StyledPaper>

      <LoadingButton
        //   loading={isLoading}
        sx={{ mt: 2, padding: '10px' }}
        loadingPosition="start"
        startIcon={<PlayArrowIcon />}
        variant="contained"
        onClick={() => console.log('summary report')}
      >
        Generate Summary
      </LoadingButton>

        <WatchDogIssueTypeContainer />
    </>
  )
}

export default WatchdogSummaryReport
