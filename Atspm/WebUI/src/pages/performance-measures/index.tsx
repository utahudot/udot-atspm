'use client'

import { Location } from '@/api/config'
import { ResponsivePageLayout } from '@/components/ResponsivePage'
import { StyledPaper } from '@/components/StyledPaper'
import SelectDateTime from '@/components/selectTimeSpan'
import { ChartOptions, ChartType } from '@/features/charts/common/types'
import ChartsContainer from '@/features/charts/components/chartsContainer'
import SelectChart from '@/features/charts/components/selectChart'
import { useLatestVersionOfAllLocations } from '@/features/locations/api'
import LocationsConfigContainer from '@/features/locations/components/locationConfigContainer'
import SelectLocation from '@/features/locations/components/selectLocation'
import useMissingDays from '@/hooks/useMissingDays'
import { TabContext, TabList, TabPanel } from '@mui/lab'
import { Box, Paper, Tab, useMediaQuery, useTheme } from '@mui/material'
import {
  differenceInMinutes,
  endOfMonth,
  endOfWeek,
  startOfMonth,
  startOfToday,
  startOfTomorrow,
  startOfWeek,
} from 'date-fns'
import { useSearchParams } from 'next/navigation'
import { useEffect, useMemo, useRef, useState } from 'react'

const CORE_KEYS = new Set(['location', 'chartType', 'start', 'end'])

const PerformanceMeasures = () => {
  const theme = useTheme()
  const isMobileView = useMediaQuery(theme.breakpoints.down('md'))
  const searchParams = useSearchParams()
  const { data: locationsData } = useLatestVersionOfAllLocations()

  const [currentTab, setCurrentTab] = useState('1')
  const [location, setLocation] = useState<Location | null>(null)
  const [calendarStartDate, setCalendarStartDate] = useState<Date>(
    startOfWeek(startOfMonth(startOfToday()))
  )
  const [calendarEndDate, setCalendarEndDate] = useState<Date>(
    endOfWeek(endOfMonth(startOfToday()))
  )
  const [chartType, setChartType] = useState<ChartType | null>(null)
  const [chartOptions, setChartOptions] = useState<Partial<ChartOptions>>()
  const [startDateTime, setStartDateTime] = useState(startOfToday())
  const [endDateTime, setEndDateTime] = useState(startOfTomorrow())

  const appliedUrlRef = useRef(false)
  useEffect(() => {
    if (!searchParams) return
    if (appliedUrlRef.current) return

    const locationParam = searchParams.get('location')
    if (locationParam && !locationsData?.value?.length) return

    const chartTypeParam = searchParams.get('chartType')
    if (chartTypeParam) setChartType(chartTypeParam as ChartType)

    if (locationParam) {
      const matchedLocation = locationsData?.value?.find(
        (loc) => loc.locationIdentifier === locationParam
      )
      if (matchedLocation) setLocation(matchedLocation)
    }

    const startParam = searchParams.get('start')
    if (startParam) {
      const parsedStart = new Date(startParam)
      if (!isNaN(parsedStart.getTime())) setStartDateTime(parsedStart)
    }

    const endParam = searchParams.get('end')
    if (endParam) {
      const parsedEnd = new Date(endParam)
      if (!isNaN(parsedEnd.getTime())) setEndDateTime(parsedEnd)
    }

    const optionsFromUrl: Record<string, string | string[]> = {}
    const keys = Array.from(new Set(Array.from(searchParams.keys())))
    keys.forEach((key) => {
      if (CORE_KEYS.has(key)) return
      const values = searchParams.getAll(key)
      if (values.length === 0) return
      optionsFromUrl[key] = values.length === 1 ? values[0] : values
    })

    if (Object.keys(optionsFromUrl).length > 0) {
      setChartOptions(optionsFromUrl as unknown as Partial<ChartOptions>)
    }

    appliedUrlRef.current = true
  }, [searchParams, locationsData])

  useEffect(() => {
    const hasChartTypeParam = Boolean(searchParams?.get('chartType'))
    if (hasChartTypeParam) return

    if (location && !chartType) {
      setChartType(ChartType.PurduePhaseTermination)
    }
  }, [location, chartType, searchParams])

  const missingDays = useMissingDays(
    location?.locationIdentifier ?? '',
    chartType === ChartType.ApproachSpeed ? 'SpeedEvent' : 'IndianaEvent',
    'raw',
    calendarStartDate,
    calendarEndDate
  )

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
    setLocation(newLocation)
  }

  const handleCalendarRangeChange = (start: Date, end: Date) => {
    setCalendarStartDate(start)
    setCalendarEndDate(end)
  }

  const locationIdentifier = location?.locationIdentifier

  const timespanWarning = useMemo(() => {
    if (chartType !== ChartType.TimingAndActuation) return null

    const diffMinutes = differenceInMinutes(endDateTime, startDateTime)
    return diffMinutes > 120
      ? 'A time span of 2 hours or less is recommended for this chart.'
      : null
  }, [chartType, startDateTime, endDateTime])

  const binSizeWarning = useMemo(() => {
    if (chartOptions && 'binSize' in chartOptions && chartOptions.binSize) {
      const diffMinutes = differenceInMinutes(endDateTime, startDateTime)
      return diffMinutes < Number(chartOptions.binSize)
        ? 'The selected bin size is larger than the selected time span.'
        : null
    }
    return null
  }, [chartOptions, startDateTime, endDateTime])

  const handleDateChange = (date: Date | null) => {
    if (!date) return
    const newStart = startOfWeek(startOfMonth(date))
    const newEnd = endOfWeek(endOfMonth(date))
    handleCalendarRangeChange(newStart, newEnd)
  }

  return (
    <ResponsivePageLayout title={'Performance Measures'} noBottomMargin>
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

        <Box sx={{ display: 'flex', flexWrap: 'wrap', marginY: 3, gap: 2 }}>
          <StyledPaper sx={{ flexGrow: 1, width: '20.188rem', padding: 3 }}>
            <SelectLocation
              location={location}
              setLocation={handleLocationChange}
              chartsDisabled
            />
          </StyledPaper>

          <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 2 }}>
            <Paper
              sx={{
                padding: 3,
                width: '336px',
                [theme.breakpoints.down('sm')]: { width: '100%' },
              }}
            >
              <SelectDateTime
                startDateTime={startDateTime}
                endDateTime={endDateTime}
                changeStartDate={handleStartDateTimeChange}
                changeEndDate={handleEndDateTimeChange}
                noCalendar={isMobileView}
                warning={binSizeWarning ?? timespanWarning}
                markDays={location ? missingDays : undefined}
                onMonthChange={handleDateChange}
                onChange={handleDateChange}
              />
            </Paper>

            <StyledPaper
              sx={{
                display: 'flex',
                flexDirection: 'column',
                flex: '1 1 336px',
                minHeight: 0,
                padding: 3,
                width: '336px',
                [theme.breakpoints.down('sm')]: { minWidth: '100%' },
              }}
            >
              <SelectChart
                chartType={chartType}
                setChartType={setChartType}
                setChartOptions={setChartOptions}
                chartOptions={chartOptions}
                location={location}
              />
            </StyledPaper>
          </Box>
        </Box>

        <TabPanel value="1" sx={{ padding: '0px' }}>
          <ChartsContainer
            location={location?.locationIdentifier ?? ''}
            chartType={chartType as ChartType}
            startDateTime={startDateTime}
            endDateTime={endDateTime}
            options={chartOptions}
          />
        </TabPanel>

        <TabPanel value="2" sx={{ padding: '0px' }}>
          {locationIdentifier && currentTab === '2' && (
            <LocationsConfigContainer locationIdentifier={locationIdentifier} />
          )}
        </TabPanel>
      </TabContext>
    </ResponsivePageLayout>
  )
}

export default PerformanceMeasures
