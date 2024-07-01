import { useCharts } from '@/features/charts/api'
import ApproachVolumeChartResults from '@/features/charts/approachVolume/components/ApproachVolumeChartResults'
import { ChartOptions, ChartType } from '@/features/charts/common/types'
import ChartsToolbox from '@/features/charts/components/chartsToolbox'
import DefaultChartResults from '@/features/charts/components/defaultChartResults'
import PhaseTable from '@/features/charts/splitMonitor/components/PhaseTable'
import TimingAndActuationChartsResults from '@/features/charts/timingAndActuation/components/timingAndActuationChartsResults/TimingAndActuationChartsResults'
import TimingAndActuationChartsToolbox from '@/features/charts/timingAndActuation/components/timingAndActuationChartsToolbox/TimingAndActuationChartsToolbox'
import TurningMovementCountsTable from '@/features/charts/turningMovementCounts/components/TurningMovementCountsTable'
import { TransformedApproachVolumeResponse } from '@/features/charts/types'
import LocationsConfigContainer from '@/features/locations/components/locationConfigContainer'
import PlayArrowIcon from '@mui/icons-material/PlayArrow'
import { LoadingButton } from '@mui/lab'
import { Alert, Box } from '@mui/material'
import { AxiosError } from 'axios'
import { RefObject, createRef, useEffect, useRef, useState } from 'react'

interface ChartsContainerProps {
  location: string
  chartType: ChartType
  startDateTime: Date
  endDateTime: Date
  options: Partial<ChartOptions> | undefined
}

export default function ChartsContainer({
  location,
  chartType,
  startDateTime,
  endDateTime,
  options,
}: ChartsContainerProps) {
  const [showConfig, setShowConfig] = useState(false)
  const [scrollPositionForCharts, setScrollPositionForCharts] = useState(0)
  const [scrollPositionForConfig, setScrollPositionForConfig] = useState(0)
  const [areRefsReady, setAreRefsReady] = useState(false)
  const [alert, setAlert] = useState('')
  const chartRefs = useRef<RefObject<HTMLDivElement>[]>([])

  let useChartsController = false

  const chartOptions = {
    locationIdentifier: location,
    start: startDateTime,
    end: endDateTime,
    ...options,
  }

  const {
    refetch,
    data: chartData,
    isError,
    isLoading,
    error,
  } = useCharts({
    chartType,
    chartOptions,
  })

  useEffect(() => {
    if (alert && location !== '' && chartType !== null) {
      setAlert('')
    }
  }, [location, alert, chartType])

  useEffect(() => {
    showConfig
      ? setScrollPositionForCharts(window.scrollY)
      : setScrollPositionForConfig(window.scrollY)
  }, [showConfig])

  useEffect(() => {
    showConfig
      ? window.scrollTo({ top: scrollPositionForConfig })
      : window.scrollTo({ top: scrollPositionForCharts })
  }, [showConfig, scrollPositionForCharts, scrollPositionForConfig])

  useEffect(() => {
    setAreRefsReady(false)
    if (!chartData) {
      return
    }
    const dataLength = chartData.data.charts.length
    chartRefs.current = new Array(dataLength).fill(null).map(() => createRef())

    setAreRefsReady(chartRefs.current.length === dataLength)
  }, [chartData])

  const handleGenerateCharts = () => {
    if (location === '' && chartType === null) {
      setAlert('Please select a location and measure')
      return
    } else if (location === '') {
      setAlert('Please select a location')
      return
    } else if (chartType === null) {
      setAlert('Please select a measure')
      return
    }
    setAlert('')
    refetch()
  }

  if (chartData && chartData.data.charts.length > 0) {
    if ('displayProps' in chartData.data.charts[0].chart) {
      useChartsController =
        chartData.data.charts[0].chart.displayProps !== undefined
    }
  }

  const displayStyle = (shouldShow: boolean) => ({
    display: shouldShow ? 'block' : 'none',
  })

  const displayCharts = () => {
    if (!chartData) {
      return null
    }

    if (showConfig) {
      return <LocationsConfigContainer locationIdentifier={location} />
    }

    switch (chartType) {
      case ChartType.ApproachVolume:
        return (
          <ApproachVolumeChartResults
            chartData={chartData as TransformedApproachVolumeResponse}
            refs={chartRefs.current}
          />
        )
      case ChartType.TimingAndActuation:
        return (
          <TimingAndActuationChartsResults
            chartData={chartData}
            refs={chartRefs.current}
          />
        )
      case ChartType.TurningMovementCounts:
        return (
          <>
            <TurningMovementCountsTable chartData={chartData} />
            <DefaultChartResults
              refs={chartRefs.current}
              chartData={chartData}
            />
          </>
        )
      case ChartType.SplitMonitor:
        return (
          <>
            <DefaultChartResults
              refs={chartRefs.current}
              chartData={chartData}
            />
            <PhaseTable phases={chartData.data.charts} />
          </>
        )
      default:
        return (
          <DefaultChartResults refs={chartRefs.current} chartData={chartData} />
        )
    }
  }

  return (
    <>
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
        {alert && (
          <Alert severity="error" sx={{ marginLeft: 1 }}>
            {alert}
          </Alert>
        )}
      </Box>
      {useChartsController &&
        areRefsReady &&
        (chartType === ChartType.TimingAndActuation ? (
          <TimingAndActuationChartsToolbox
            chartRefs={chartRefs.current}
            chartData={chartData}
            toggleConfig={() => setShowConfig(!showConfig)}
            toggleConfigLabel={showConfig ? 'Charts' : 'Config'}
          />
        ) : (
          <ChartsToolbox
            chartRefs={chartRefs.current}
            chartData={chartData}
            toggleConfig={() => setShowConfig(!showConfig)}
            toggleConfigLabel={showConfig ? 'Charts' : 'Config'}
          />
        ))}
      <Box display={displayStyle(!showConfig)}>
        {chartData && displayCharts()}
      </Box>
      {location && (
        <Box display={displayStyle(showConfig)}>
          <LocationsConfigContainer locationIdentifier={location} />
        </Box>
      )}
    </>
  )
}
