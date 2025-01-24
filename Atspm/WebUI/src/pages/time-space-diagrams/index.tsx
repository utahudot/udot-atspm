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
import { AxiosError, AxiosResponse } from 'axios'
import { set, subDays, subMonths } from 'date-fns'
import { RefObject, useRef, useState } from 'react'

const TimeSpaceDiagram = () => {
  const [currentTab, setCurrentTab] = useState(ToolType.TimeSpaceHistoric)
  const chartRefs = useRef<RefObject<HTMLDivElement>[]>([])
  const [hasAttemptedGenerate, setHasAttemptedGenerate] = useState(false)
  const [routeId, setRouteId] = useState('')

  // Dynamic default options generation based on selected tab
  const getDefaultChartOptions = (tab: ToolType): TimeSpaceOptions => {
    const yesterday = subDays(new Date(), 1)

    if (tab === ToolType.TimeSpaceHistoric) {
      return {
        extendStartStopSearch: 2,
        showAllLanesInfo: true,
        start: set(yesterday, { hours: 16, minutes: 0, seconds: 0 }),
        end: set(yesterday, { hours: 16, minutes: 20, seconds: 0 }),
        routeId: routeId || '',
        chartType: '',
        speedLimit: null,
        locationIdentifier: '',
      }
    }

    const formatTimestampToYYYYMMDD = (timestamp: string | Date) => {
      const date =
        typeof timestamp === 'string' ? new Date(timestamp) : timestamp

      const day = String(date.getDate()).padStart(2, '0')
      const month = String(date.getMonth() + 1).padStart(2, '0')
      const year = date.getFullYear()

      return `${year}-${month}-${day}`
    }

    const formattedStartTime = toUTCDateWithTimeStamp(
      set(new Date(), { hours: 16, minutes: 0, seconds: 0 })
    )
    const formattedEndTime = toUTCDateWithTimeStamp(
      set(new Date(), { hours: 16, minutes: 20, seconds: 0 })
    )
    return {
      startDate: formatTimestampToYYYYMMDD(subMonths(yesterday, 1)),
      endDate: formatTimestampToYYYYMMDD(yesterday),
      startTime: formattedStartTime,
      endTime: formattedEndTime,
      routeId: routeId || '',
      speedLimit: null,
      daysOfWeek: [1, 2, 3, 4, 5],
      sequence: [],
      coordinatedPhases: [],
    }
  }
  const [toolOptions, setToolOptions] = useState<TimeSpaceOptions>(
    getDefaultChartOptions(ToolType.TimeSpaceHistoric)
  )

  const handleRouteIdChange = (routeId: string) => {
    setRouteId(routeId)
    setToolOptions((prev) => ({ ...prev, routeId }))
  }

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

  const { data: routesData, isLoading: isLoadingRoutes } = useGetRoute()

  if (isLoadingRoutes) {
    return <Typography>Loading...</Typography>
  }

  if (routesData === undefined) {
    return <Typography>Error retrieving routes</Typography>
  }

  const routes = routesData?.value?.sort((a, b) => a.name.localeCompare(b.name))

  const handleGenerateCharts = () => {
    setHasAttemptedGenerate(true)
    if (toolOptions.routeId) {
      refetch()
    }
  }

  const handleChange = (_: React.SyntheticEvent, newValue: ToolType) => {
    setCurrentTab(newValue)
    setToolOptions(getDefaultChartOptions(newValue))
    setHasAttemptedGenerate(false)
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
              routes={routes}
              routeId={routeId}
              setRouteId={handleRouteIdChange}
            />
          </TabPanel>
          <TabPanel value={ToolType.TimeSpaceAverage} sx={{ padding: '0px' }}>
            <TimeSpaceAverageContainer
              handleToolOptions={handleToolOptions}
              routes={routes}
              routeId={routeId}
              setRouteId={handleRouteIdChange}
            />
          </TabPanel>
          <Box>
            <Box
              display={'flex'}
              alignItems={'center'}
              height={'50px'}
              marginTop={2}
            >
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
                  {(
                    ((error as AxiosError).response as unknown as AxiosResponse)
                      ?.data as string
                  ).includes('Controller')
                    ? ((
                        (error as AxiosError)
                          .response as unknown as AxiosResponse
                      )?.data as string)
                    : ((
                        (error as AxiosError)
                          .response as unknown as AxiosResponse
                      )?.data as string) + '. Verify in Configuration'}
                </Alert>
              )}
              {hasAttemptedGenerate && toolOptions.routeId === '' && (
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
