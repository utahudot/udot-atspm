import { ResponsivePageLayout } from '@/components/ResponsivePage'
import { useTools } from '@/features/charts/api/getTools'
import { ToolType } from '@/features/charts/common/types'
import DefaultChartResults from '@/features/charts/components/defaultToolResults/DefaultToolResults'
import { TimeSpaceAverageContainer } from '@/features/charts/timeSpaceDiagram/components/containers/TimeSpaceAverageContainer'
import { TimeSpaceHistoricContainer } from '@/features/charts/timeSpaceDiagram/components/containers/TimeSpaceHistoricContainer'
import {
  TimeSpaceAverageOptions,
  TimeSpaceOptions,
} from '@/features/charts/timeSpaceDiagram/types'
import { TransformedToolResponse } from '@/features/charts/types'
import { useGetRoute } from '@/features/routes/api/getRoutes'
import { toUTCDateWithTimeStamp } from '@/utils/dateTime'
import PlayArrowIcon from '@mui/icons-material/PlayArrow'
import { LoadingButton, TabContext, TabList, TabPanel } from '@mui/lab'
import { Alert, Box, Tab, Typography } from '@mui/material'
import { AxiosError } from 'axios'
import { startOfDay } from 'date-fns'
import { RefObject, useRef, useState } from 'react'

const TimeSpaceDiagram = () => {
  const [currentTab, setCurrentTab] = useState(ToolType.TimeSpaceHistoric)
  const chartRefs = useRef<RefObject<HTMLDivElement>[]>([])

  // Dynamic default options generation based on selected tab
  const getDefaultChartOptions = (tab: ToolType): TimeSpaceOptions => {
    if (tab === ToolType.TimeSpaceHistoric) {
      return {
        extendStartStopSearch: 2,
        showAllLanesInfo: true,
        start: startOfDay(new Date()),
        end: startOfDay(new Date()),
        routeId: '',
        chartType: '',
        speedLimit: null,
        locationIdentifier: '',
      }
    } else {
      const date = new Date()
      const year = date.getUTCFullYear()
      const month = String(date.getUTCMonth() + 1).padStart(2, '0')
      const day = String(date.getUTCDate()).padStart(2, '0')
      const startTime = toUTCDateWithTimeStamp(startOfDay(date))
      const endTime = toUTCDateWithTimeStamp(startOfDay(date))
      const formattedDate = `${year}-${month}-${day}`
      return {
        startDate: formattedDate,
        endDate: formattedDate,
        startTime,
        endTime,
        routeId: '',
        speedLimit: null,
        daysOfWeek: [1, 2, 3, 4, 5],
        sequence: [],
        coordinatedPhases: [],
      }
    }
  }
  const [toolOptions, setToolOptions] = useState<TimeSpaceOptions>(
    getDefaultChartOptions(ToolType.TimeSpaceHistoric)
  )

  const {
    refetch,
    data: chartData,
    isError,
    isLoading,
    error,
  } = useTools({
    toolType: currentTab,
    toolOptions,
  })

  const { data: routes, isLoading: isLoadingRoutes } = useGetRoute()

  if (isLoadingRoutes) {
    return <Typography>Waiting to load data</Typography>
  }

  if (routes === undefined) {
    return <Typography>Error retrieving routes</Typography>
  }
  const handleGenerateCharts = () => {
    refetch()
  }

  const handleChange = (_: React.SyntheticEvent, newValue: ToolType) => {
    setCurrentTab(newValue)
    setToolOptions(getDefaultChartOptions(newValue))
  }

  const handleToolOptions = (value: Partial<TimeSpaceOptions>) => {
    const [key, val] = Object.entries(value)[0]
    setToolOptions((prev) => ({ ...prev, [key]: val }))
  }

  const handleChartType = () => {
    return (
      <Box>
        <TabList
          onChange={handleChange}
          aria-label="Location options"
          textColor="primary"
          indicatorColor="primary"
        >
          <Tab label="Historic" value={ToolType.TimeSpaceHistoric} />
          <Tab label="50th Percentile" value={ToolType.TimeSpaceAverage} />
        </TabList>
      </Box>
    )
  }

  const isTimeSpaceAverageValidOptions = () => {
    return (
      (currentTab === ToolType.TimeSpaceAverage &&
        (toolOptions as TimeSpaceAverageOptions).startTime === '') ||
      (toolOptions as TimeSpaceAverageOptions).endTime === ''
    )
  }

  return (
    <ResponsivePageLayout title={'Time-Space Diagrams'} noBottomMargin>
      <TabContext value={currentTab}>
        {handleChartType()}
        <Box>
          <TabPanel value={ToolType.TimeSpaceHistoric} sx={{ padding: '0px' }}>
            <TimeSpaceHistoricContainer
              handleToolOptions={handleToolOptions}
              routes={routes?.value}
            />
          </TabPanel>
          <TabPanel value={ToolType.TimeSpaceAverage} sx={{ padding: '0px' }}>
            <TimeSpaceAverageContainer
              handleToolOptions={handleToolOptions}
              routes={routes?.value}
            />
          </TabPanel>
          <Box>
            <Box display={'flex'} alignItems={'center'} height={'50px'}>
              <LoadingButton
                loading={isLoading}
                loadingPosition="start"
                startIcon={<PlayArrowIcon />}
                variant="contained"
                sx={{ padding: '10px' }}
                onClick={handleGenerateCharts}
              >
                Generate Charts
              </LoadingButton>
              {isError && (
                <Alert severity="error" sx={{ marginLeft: 1 }}>
                  {error instanceof AxiosError
                    ? error.response?.data
                    : (error as Error).message}
                </Alert>
              )}
              {toolOptions.routeId === '' && (
                <Alert severity="error" sx={{ marginLeft: 1 }}>
                  Please Select a route
                </Alert>
              )}
              {isTimeSpaceAverageValidOptions() && (
                <Alert severity="error" sx={{ marginLeft: 1 }}>
                  Select start and end time ranges
                </Alert>
              )}
            </Box>
            {chartData && (
              <DefaultChartResults
                refs={chartRefs.current}
                chartData={chartData as TransformedToolResponse}
              />
            )}
          </Box>
        </Box>
      </TabContext>
    </ResponsivePageLayout>
  )
}

export default TimeSpaceDiagram
