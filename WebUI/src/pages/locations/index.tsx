import { TabContext, TabList, TabPanel } from '@mui/lab'
import { Box, Tab, useMediaQuery, useTheme } from '@mui/material'
import { startOfToday, startOfTomorrow } from 'date-fns'
import { useState } from 'react'

import { ResponsivePageLayout } from '@/components/ResponsivePage'
import { StyledPaper } from '@/components/StyledPaper'
import SelectDateTime from '@/components/selectTimeSpan'
import { ChartOptions, ChartType } from '@/features/charts/common/types'
import ChartsContainer from '@/features/charts/components/chartsContainer'
import SelectChart from '@/features/charts/components/selectChart'
import LocationsConfigContainer from '@/features/locations/components/locationConfigContainer'
import SelectLocation from '@/features/locations/components/selectLocation'
import { Location } from '@/features/locations/types'

const Locations = () => {
  const [currentTab, setCurrentTab] = useState('1')
  const [location, setLocation] = useState<Location | null>(null)
  const [chartType, setChartType] = useState<ChartType | null>(null)
  const [chartOptions, setChartOptions] = useState<Partial<ChartOptions>>()
  const [startDateTime, setStartDateTime] = useState(startOfToday())
  const [endDateTime, setEndDateTime] = useState(startOfTomorrow())
  const theme = useTheme()
  const isMobileView = useMediaQuery(theme.breakpoints.down('md'))

  const handleStartDateTimeChange = (date: Date) => {
    setStartDateTime(date)
  }

  const handleEndDateTimeChange = (date: Date) => {
    setEndDateTime(date)
  }

  const handleChange = (_: React.SyntheticEvent, newValue: string) => {
    setCurrentTab(newValue)
  }

  const handleLocationChange = (newLocation: Location) => {
    if (!location) {
      setChartType(ChartType.SplitMonitor)
    }
    setLocation(newLocation)
  }

  const locationIdentifier = location?.locationIdentifier

  return (
    <ResponsivePageLayout title={'Locations'} noBottomMargin>
      <TabContext value={currentTab}>
        <TabList
          onChange={handleChange}
          aria-label="Location options"
          textColor="primary"
          indicatorColor="primary"
        >
          <Tab label="Charts" value="1" />
          <Tab label="Configuration" value="2" />
        </TabList>
        <Box
          sx={{
            display: 'flex',
            flexWrap: 'wrap',
            marginY: 3,
            marginX: '0px',
            gap: 2,
          }}
        >
          {/* LocationMapComponent */}
          <StyledPaper
            sx={{
              flexGrow: 1,
              width: '20.188rem',
              padding: 3,
            }}
          >
            <SelectLocation
              location={location}
              setLocation={handleLocationChange}
            />
          </StyledPaper>
          {/* SideComponents */}
          <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 2 }}>
            <StyledPaper
              sx={{
                padding: 3,
                width: '336px',
                [theme.breakpoints.down('sm')]: {
                  width: '100%',
                },
              }}
            >
              <SelectDateTime
                chartType={chartType}
                startDateTime={startDateTime}
                endDateTime={endDateTime}
                changeStartDate={handleStartDateTimeChange}
                changeEndDate={handleEndDateTimeChange}
                noCalendar={isMobileView}
              />
            </StyledPaper>

            <StyledPaper
              sx={{
                padding: 3,
                width: '336px',
                [theme.breakpoints.down('sm')]: {
                  minWidth: '100%',
                },
              }}
            >
              <SelectChart
                chartType={chartType}
                setChartType={setChartType}
                setChartOptions={setChartOptions}
                location={location}
              />
            </StyledPaper>
          </Box>
        </Box>

        {/* ChartsComponent */}
        <TabPanel value="1" sx={{ padding: '0px' }}>
          <ChartsContainer
            location={location?.locationIdentifier ?? ''}
            chartType={chartType as ChartType}
            startDateTime={startDateTime}
            endDateTime={endDateTime}
            options={chartOptions}
          />
        </TabPanel>

        {/* ConfigurationComponent */}
        <TabPanel value="2" sx={{ padding: '0px' }}>
          {locationIdentifier && (
            <LocationsConfigContainer locationIdentifier={locationIdentifier} />
          )}
        </TabPanel>
      </TabContext>
    </ResponsivePageLayout>
  )
}

export default Locations
