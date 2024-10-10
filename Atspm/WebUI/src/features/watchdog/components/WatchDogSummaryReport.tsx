import { StyledPaper } from '@/components/StyledPaper'
import WatchdogChartsContainer from '@/features/charts/watchdogDashboard/components/WatchdogChartsContainer'
import PlayArrowIcon from '@mui/icons-material/PlayArrow'
import { LoadingButton } from '@mui/lab'
import { Box } from '@mui/material'
import {
  format,
  startOfToday,
  startOfTomorrow,
  subDays,
  subYears,
} from 'date-fns'
import { useState } from 'react'
import { useGetWatchdogDashboardData } from '../api/getWatchdogDashboardData'
import HorizontalDateInput from './HorizontalDateInputs'

const WatchdogSummaryReport = () => {
  const [startDateTime, setStartDateTime] = useState(
    subYears(startOfToday(), 1)
  )
  const [endDateTime, setEndDateTime] = useState(subDays(startOfTomorrow(), 1))
  const [fetchData, setFetchData] = useState(false)
  const formattedStartDate = format(startDateTime, "yyyy-MM-dd'T'HH:mm:ss'Z'")
  const formattedEndDate = format(endDateTime, "yyyy-MM-dd'T'HH:mm:ss'Z'")

  const { data, isLoading, error } = useGetWatchdogDashboardData({
    start: formattedStartDate,
    end: formattedEndDate,
    enabled: fetchData,
  })

  const handleGenerateSummary = () => {
    setFetchData(true)
  }

  const handleStartDateTimeChange = (date: Date) => {
    setStartDateTime(date)
  }

  const handleEndDateTimeChange = (date: Date) => {
    setEndDateTime(date)
  }

  if (!isLoading) console.log('KIMBRO', data)

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
        loading={isLoading}
        sx={{ mt: 2, padding: '10px', mb: 2 }}
        loadingPosition="start"
        startIcon={<PlayArrowIcon />}
        variant="contained"
        onClick={handleGenerateSummary}
      >
        Generate Summary
      </LoadingButton>

      {error && <Box>Error loading data</Box>}

      {!isLoading && data && (
        <WatchdogChartsContainer data={data} isLoading={isLoading} />
      )}
    </>
  )
}

export default WatchdogSummaryReport
