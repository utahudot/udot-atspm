import { ResponsivePageLayout } from '@/components/ResponsivePage'
import { useTimeSpaceCall } from '@/features/charts/api/getTools'
import { ToolType } from '@/features/charts/common/types'
import TimeSpaceChart from '@/features/charts/components/defaultToolResults/DefaultToolResults'
import { useLinkPivotForTsd } from '@/features/charts/timeSpaceDiagram/api/getLinkPivotForTsd'
import { AverageOptionsComponent } from '@/features/charts/timeSpaceDiagram/average/TimeSpaceAverageOptions'
import { useAverageOptionsHandler } from '@/features/charts/timeSpaceDiagram/average/TimeSpaceAverageOptions/timeSpaceAverageOptions.handler'
import HistoricOptionsComponent from '@/features/charts/timeSpaceDiagram/historic/TimeSpaceHistoricOptions/TimeSpaceHistoricOptions'
import { useHistoricOptionsHandler } from '@/features/charts/timeSpaceDiagram/historic/TimeSpaceHistoricOptions/historicTimeSpaceOptions.handler'
import {
  TimeSpaceAverageOptions,
  TimeSpaceOptions,
} from '@/features/charts/timeSpaceDiagram/shared/types'
import { useGetRoute } from '@/features/routes/api/getRoutes'
import PlayArrowIcon from '@mui/icons-material/PlayArrow'
import { LoadingButton, TabContext, TabList, TabPanel } from '@mui/lab'
import { Alert, Box, Tab, Typography } from '@mui/material'
import { AxiosError } from 'axios'
import { usePathname, useRouter, useSearchParams } from 'next/navigation'
import { useEffect, useMemo, useRef, useState } from 'react'

const TimeSpaceDiagram = () => {
  const router = useRouter()
  const pathname = usePathname()
  const searchParams = useSearchParams()
  const appliedUrlRef = useRef(false)

  const [currentTab, setCurrentTab] = useState<
    ToolType.TimeSpaceHistoric | ToolType.TimeSpaceAverage
  >(ToolType.TimeSpaceHistoric)
  const [hasAttemptedGenerate, setHasAttemptedGenerate] = useState(false)

  const { data: routesData, isLoading: isLoadingRoutes } = useGetRoute()

  const routes = useMemo(() => {
    const list = routesData?.value ?? []
    return [...list].sort((a, b) => a.name.localeCompare(b.name))
  }, [routesData])

  const historicHandler = useHistoricOptionsHandler({ routes })
  const averageHandler = useAverageOptionsHandler({ routes })

  const activeHandler =
    currentTab === ToolType.TimeSpaceHistoric ? historicHandler : averageHandler

  const [submittedOptions, setSubmittedOptions] = useState<TimeSpaceOptions>(
    () => historicHandler.toOptions()
  )
  const [shouldFetch, setShouldFetch] = useState(false)

  const {
    refetch,
    data: chartData,
    isError,
    isLoading,
    error,
  } = useTimeSpaceCall({
    toolType: currentTab,
    toolOptions: submittedOptions,
  })

  const { refetch: fetchLp, data: lpTsdData } = useLinkPivotForTsd({
    toolType: ToolType.LpTsd,
    toolOptions: submittedOptions,
  })

  useEffect(() => {
    if (!searchParams) return
    if (appliedUrlRef.current) return
    if (isLoadingRoutes) return
    if (!routesData) return

    const toolTypeParam = searchParams.get('toolType') as ToolType | null
    const nextTab =
      toolTypeParam === ToolType.TimeSpaceAverage
        ? ToolType.TimeSpaceAverage
        : ToolType.TimeSpaceHistoric

    setCurrentTab(nextTab)

    historicHandler.applyFromSearchParams(searchParams)
    averageHandler.applyFromSearchParams(searchParams)

    appliedUrlRef.current = true
  }, [
    searchParams,
    isLoadingRoutes,
    routesData,
    historicHandler,
    averageHandler,
  ])

  useEffect(() => {
    if (!shouldFetch) return
    refetch()
    fetchLp()
    setShouldFetch(false)
  }, [shouldFetch, refetch, fetchLp])

  const handleChange = (
    _: React.SyntheticEvent,
    newValue: ToolType.TimeSpaceHistoric | ToolType.TimeSpaceAverage
  ) => {
    setCurrentTab(newValue)
    setHasAttemptedGenerate(false)
  }

  const isTimeSpaceAverageInvalidOptions = useMemo(() => {
    if (currentTab !== ToolType.TimeSpaceAverage) return false
    const opts = averageHandler.toOptions() as TimeSpaceAverageOptions
    return (opts.startTime ?? '') === '' || (opts.endTime ?? '') === ''
  }, [currentTab, averageHandler])

  const pushActiveParamsToUrl = () => {
    const params = activeHandler.toSearchParams()
    params.set('toolType', String(currentTab))

    const qs = params.toString()
    router.replace(qs ? `${pathname}?${qs}` : pathname, { scroll: false })
  }

  const handleGenerateCharts = async () => {
    setHasAttemptedGenerate(true)

    const options = activeHandler.toOptions() as TimeSpaceOptions
    if (!options.routeId) return
    if (isTimeSpaceAverageInvalidOptions) return

    pushActiveParamsToUrl()

    setSubmittedOptions(options)
    setShouldFetch(true)
  }

  if (isLoadingRoutes) {
    return <Typography>Loading...</Typography>
  }

  if (!routesData) {
    return <Typography>Error retrieving routes</Typography>
  }

  return (
    <ResponsivePageLayout title={'Time-Space Diagrams'} noBottomMargin>
      <TabContext value={currentTab}>
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

        <Box>
          <TabPanel value={ToolType.TimeSpaceHistoric} sx={{ padding: '0px' }}>
            <HistoricOptionsComponent handler={historicHandler} />
          </TabPanel>

          <TabPanel value={ToolType.TimeSpaceAverage} sx={{ padding: '0px' }}>
            <AverageOptionsComponent handler={averageHandler} />
          </TabPanel>

          <Box>
            <Box display="flex" alignItems="center" height="50px" marginTop={2}>
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
                  {String((error as AxiosError).response?.data ?? 'Error')}
                </Alert>
              )}

              {hasAttemptedGenerate && activeHandler.routeId === '' && (
                <Alert severity="error" sx={{ marginLeft: 1 }}>
                  Please Select a route
                </Alert>
              )}

              {isTimeSpaceAverageInvalidOptions && (
                <Alert severity="error" sx={{ marginLeft: 1 }}>
                  Select start and end time ranges
                </Alert>
              )}
            </Box>

            {chartData && (
              <TimeSpaceChart
                timeSpaceData={chartData}
                linkPivotTsdData={lpTsdData ?? []}
                timeSpaceOptions={submittedOptions}
              />
            )}
          </Box>
        </Box>
      </TabContext>
    </ResponsivePageLayout>
  )
}

export default TimeSpaceDiagram
