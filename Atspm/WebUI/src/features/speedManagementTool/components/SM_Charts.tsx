import {
  CongestionTrackingOptions,
  DataQualityOptions,
  EffectivenessOfStrategiesOptions,
  SpeedComplianceOptions,
  SpeedOverDistanceOptions,
  SpeedOverTimeOptions,
  SpeedVariabilityOptions,
  SpeedViolationsDto,
} from '@/api/speedManagement/aTSPMSpeedManagementApi.schemas'
import CongestionTrackerChartsContainer from '@/features/charts/speedManagementTool/congestionTracker/components/CongestionTrackerChartsContainer'
import CongestionTrackingChartOptions from '@/features/charts/speedManagementTool/congestionTracker/components/CongestionTrackingChartOptions'
import DataQualityChartContainer from '@/features/charts/speedManagementTool/dataQuality/components/DataQualityChartContainer'
import DataQualityChartOptions from '@/features/charts/speedManagementTool/dataQuality/components/DataQualityChartOptions'
import EffectivenessOfStrategiesChartsContainer from '@/features/charts/speedManagementTool/effectivenessOfStrategies/EffectivenessOfStrategiesChartContainer'
import EffectivenessOfStrategiesChartOptions from '@/features/charts/speedManagementTool/effectivenessOfStrategies/EffectivenessOfStrategiesChartOptions'
import SpeedComplianceChartOptions from '@/features/charts/speedManagementTool/speedCompliance/SpeedComplianceChartOptions'
import SpeedComplianceChartsContainer from '@/features/charts/speedManagementTool/speedCompliance/SpeedComplianceChartsContainer'
import SpeedOverDistanceChartContainer from '@/features/charts/speedManagementTool/speedOverDistance/components/SpeedOverDistanceChartContainer'
import SpeedOverDistanceChartOptions from '@/features/charts/speedManagementTool/speedOverDistance/components/SpeedOverDistanceChartOptions'
import SpeedOverTimeChartContainer from '@/features/charts/speedManagementTool/speedOverTime/components/SpeedOverTimeChartContainer'
import SpeedOverTimeChartOptions from '@/features/charts/speedManagementTool/speedOverTime/components/SpeedOverTimeChartOptions'
import SpeedVariabilityChartContainer from '@/features/charts/speedManagementTool/speedVariability/components/SpeedVariabilityChartContainer'
import SpeedVariabilityChartOptions from '@/features/charts/speedManagementTool/speedVariability/components/SpeedVariabilityChartOptions'
import SpeedViolationsChartContainer from '@/features/charts/speedManagementTool/speedViolations/components/SpeedViolationsChartContainer'
import SpeedViolationsChartOptions from '@/features/charts/speedManagementTool/speedViolations/components/SpeedViolationsChartOptions'
import {
  SM_ChartType,
  useSMCharts,
} from '@/features/speedManagementTool/api/getSMCharts'
import { DataSource } from '@/features/speedManagementTool/enums'
import useSpeedManagementStore from '@/features/speedManagementTool/speedManagementStore'
import { SpeedManagementRoute } from '@/features/speedManagementTool/types/routes'
import HelpOutlineIcon from '@mui/icons-material/HelpOutline'
import PlayArrowIcon from '@mui/icons-material/PlayArrow'
import { LoadingButton } from '@mui/lab'
import {
  Alert,
  Box,
  Divider,
  IconButton,
  List,
  ListItemButton,
  Popover,
  Typography,
} from '@mui/material'
import { MouseEvent, useCallback, useEffect, useMemo, useState } from 'react'

type ChartOptions =
  | CongestionTrackingOptions
  | SpeedOverTimeOptions
  | SpeedOverDistanceOptions
  | SpeedComplianceOptions
  | DataQualityOptions
  | SpeedVariabilityOptions
  | SpeedViolationsDto
  | EffectivenessOfStrategiesOptions
  | null

const CHART_TOOLTIPS: Record<SM_ChartType, JSX.Element> = {
  [SM_ChartType.CONGESTION_TRACKING]: (
    <>
      Displays the average speed and 85th percentile speed of vehicles on the
      selected segment over the selected month.
    </>
  ),
  [SM_ChartType.SPEED_OVER_TIME]: (
    <>
      Displays the average speed and 85th percentile speed of vehicles on the
      selected segment over the selected time period. The user can zoom in on
      any part of the trend to see details.
    </>
  ),
  [SM_ChartType.DATA_QUALITY]: (
    <>
      Displays the data quality/confidence for the available data sources on the
      selected segment over the selected time period.
    </>
  ),
  [SM_ChartType.SPEED_VARIABILITY]: (
    <>
      Displays the minimum and maximum speeds recorded on the selected segment
      for each day in the selected time period. A data table is provided below
      the chart, displaying the variability in the daily speeds.
    </>
  ),
  [SM_ChartType.SPEED_VIOLATIONS]: (
    <>
      Displays the percent of the traffic volume on the selected segment
      recorded travelling faster than given thresholds for each day in the
      selected time period. The values are plotted against the total traffic
      flow for context.
      <br />
      <br />
      <strong>Thresholds:</strong>
      <br />
      <strong>Violations:</strong> 2 mph or more over the speed limit for
      non-freeway segments and 7 mph or more over the speed limit for freeway
      segments.
      <br />
      <strong>Extreme Violations:</strong> 10 mph or more over the speed limit.
      <br />
      <br />A data table is provided below the chart displaying the number of
      speeds recorded each day that were over the threshold speed.
    </>
  ),
  [SM_ChartType.EFFECTIVENESS_OF_STRATEGIES]: (
    <>
      Displays the speeds for the analysis periods before and after the
      implementation of the strategy or impact.
    </>
  ),
  [SM_ChartType.SPEED_OVER_DISTANCE]: (
    <>
      Displays the Speed Limit, Average Speed, and 85th Percentile Speed for all
      selected segments. Segments are plotted by milepoint (the X axis) so the
      user can observe changes in speeds along a corridor.
    </>
  ),
  [SM_ChartType.SPEED_COMPLIANCE]: (
    <>
      Displays the Speed Limit, Average Speed, and 85th Percentile Speed for all
      selected segments. Users have the option to replace the speed limit with a
      custom speed limit that is applied to all selected segments for the
      purposes of this chart. Segments are plotted by milepoint (the X axis) so
      the user can observe changes in speeds along a corridor. A data table is
      provided below the chart displaying the speeds and their difference from
      the speed limit (or custom speed limit if one was entered).
    </>
  ),
}

function getSingleRouteBaseCharts(): SM_ChartType[] {
  return [
    SM_ChartType.CONGESTION_TRACKING,
    SM_ChartType.SPEED_OVER_TIME,
    SM_ChartType.DATA_QUALITY,
    SM_ChartType.SPEED_VARIABILITY,
    SM_ChartType.SPEED_VIOLATIONS,
    SM_ChartType.EFFECTIVENESS_OF_STRATEGIES,
  ]
}

function getMultiRouteBaseCharts(): SM_ChartType[] {
  return [
    SM_ChartType.SPEED_OVER_DISTANCE,
    SM_ChartType.SPEED_COMPLIANCE,
    SM_ChartType.DATA_QUALITY,
    SM_ChartType.SPEED_VIOLATIONS,
  ]
}

interface ExclusionRule {
  chart: SM_ChartType
  sources: DataSource[]
}

const EXCLUSION_RULES: ExclusionRule[] = [
  {
    chart: SM_ChartType.SPEED_VIOLATIONS,
    sources: [DataSource.ClearGuide],
  },
]

function filterChartsBySourceId(
  chartTypes: SM_ChartType[],
  sourceId?: number | null
): SM_ChartType[] {
  if (!sourceId) return chartTypes
  return chartTypes.filter((chart) => {
    return !EXCLUSION_RULES.some(
      (rule) => rule.chart === chart && rule.sources.includes(sourceId)
    )
  })
}

const SM_Charts = ({ routes }: { routes: SpeedManagementRoute[] }) => {
  const [selectedChart, setSelectedChart] = useState<SM_ChartType | null>(null)
  const [chartOptions, setChartOptions] = useState<ChartOptions>(null)
  const [hasFetched, setHasFetched] = useState(false)
  const [anchorEl, setAnchorEl] = useState<HTMLElement | null>(null)

  const open = Boolean(anchorEl)
  const id = open ? 'chart-guide-popover' : undefined

  const { multiselect, routeSpeedRequest } = useSpeedManagementStore()
  const sourceId = routeSpeedRequest.sourceId

  const segmentIds = useMemo(
    () => routes.map((route) => route.properties.route_id),
    [routes]
  )

  const handleChartOptionsChange = useCallback(
    (options: ChartOptions) => {
      if (
        multiselect ||
        selectedChart === SM_ChartType.DATA_QUALITY ||
        selectedChart === SM_ChartType.SPEED_VIOLATIONS ||
        selectedChart === SM_ChartType.EFFECTIVENESS_OF_STRATEGIES
      ) {
        setChartOptions({ ...options, segmentIds })
      } else {
        setChartOptions({ ...options, segmentId: segmentIds[0] })
      }
    },
    [multiselect, selectedChart, segmentIds]
  )

  const baseCharts = useMemo(() => {
    return multiselect ? getMultiRouteBaseCharts() : getSingleRouteBaseCharts()
  }, [multiselect])

  const chartTypes: SM_ChartType[] = useMemo(() => {
    return filterChartsBySourceId(baseCharts, sourceId?.[0])
  }, [baseCharts, sourceId])

  useEffect(() => {
    if (selectedChart === null || !chartTypes.includes(selectedChart)) {
      setSelectedChart(chartTypes[0] || null)
      handleChartOptionsChange(null)
    }
  }, [routes, selectedChart, chartTypes, handleChartOptionsChange])

  const { data, isLoading, refetch } = useSMCharts(selectedChart, chartOptions)

  const handleChartChange = (chartType: SM_ChartType) => {
    if (selectedChart === chartType) return
    setHasFetched(false)
    setSelectedChart(chartType)
    handleChartOptionsChange(null)
  }

  const handleOptionsChange = useCallback(
    (options: ChartOptions) => {
      setHasFetched(false)
      handleChartOptionsChange(options)
    },
    [handleChartOptionsChange]
  )

  const handleRunChart = () => {
    refetch().then(() => setHasFetched(true))
  }

  const handleOpenPopover = (event: MouseEvent<HTMLElement>) => {
    setAnchorEl(event.currentTarget)
  }

  const handleClosePopover = () => {
    setAnchorEl(null)
  }

  const renderOptionsComponent = useCallback(() => {
    if (!sourceId) return null
    switch (selectedChart) {
      case SM_ChartType.CONGESTION_TRACKING:
        return (
          <CongestionTrackingChartOptions
            onOptionsChange={handleOptionsChange}
            sourceId={sourceId}
          />
        )
      case SM_ChartType.SPEED_OVER_TIME:
        return (
          <SpeedOverTimeChartOptions
            onOptionsChange={handleOptionsChange}
            sourceId={sourceId}
          />
        )
      case SM_ChartType.SPEED_OVER_DISTANCE:
        return (
          <SpeedOverDistanceChartOptions
            onOptionsChange={handleOptionsChange}
          />
        )
      case SM_ChartType.SPEED_COMPLIANCE:
        return (
          <SpeedComplianceChartOptions onOptionsChange={handleOptionsChange} />
        )
      case SM_ChartType.DATA_QUALITY:
        return <DataQualityChartOptions onOptionsChange={handleOptionsChange} />
      case SM_ChartType.SPEED_VARIABILITY:
        return (
          <SpeedVariabilityChartOptions
            onOptionsChange={handleOptionsChange}
            sourceId={sourceId}
          />
        )
      case SM_ChartType.SPEED_VIOLATIONS:
        return (
          <SpeedViolationsChartOptions
            onOptionsChange={handleOptionsChange}
            sourceId={sourceId}
          />
        )
      case SM_ChartType.EFFECTIVENESS_OF_STRATEGIES:
        return (
          <EffectivenessOfStrategiesChartOptions
            onOptionsChange={handleOptionsChange}
          />
        )
      default:
        return null
    }
  }, [selectedChart, sourceId, handleOptionsChange])

  const renderChartContainer = () => {
    if (isLoading) return <Typography sx={{ ml: 2 }}>Loading...</Typography>
    if (hasFetched && (!data || data?.charts?.length === 0)) {
      return (
        <Alert severity="error" sx={{ p: 2, m: 2 }}>
          No data found for the selected segment and date range.
        </Alert>
      )
    }
    if (!data) return null
    switch (selectedChart) {
      case SM_ChartType.CONGESTION_TRACKING:
        return <CongestionTrackerChartsContainer chartData={data} />
      case SM_ChartType.SPEED_OVER_TIME:
        return <SpeedOverTimeChartContainer chartData={data} />
      case SM_ChartType.SPEED_OVER_DISTANCE:
        return <SpeedOverDistanceChartContainer chartData={data} />
      case SM_ChartType.SPEED_COMPLIANCE:
        return <SpeedComplianceChartsContainer chartData={data} />
      case SM_ChartType.DATA_QUALITY:
        return <DataQualityChartContainer chartData={data} />
      case SM_ChartType.SPEED_VARIABILITY:
        return <SpeedVariabilityChartContainer chartData={data} />
      case SM_ChartType.SPEED_VIOLATIONS:
        return <SpeedViolationsChartContainer chartData={data} />
      case SM_ChartType.EFFECTIVENESS_OF_STRATEGIES:
        return <EffectivenessOfStrategiesChartsContainer chartData={data} />
      default:
        return null
    }
  }

  const tooltipContent = selectedChart ? CHART_TOOLTIPS[selectedChart] : null

  return (
    <>
      <Box display="flex" sx={{ py: 2, pl: 3, height: '100%' }}>
        <Box
          sx={{
            flexShrink: 0,
            minWidth: '300px',
            overflowY: 'auto',
            maxHeight: '100%',
          }}
        >
          <List>
            {chartTypes.map((chartType) => (
              <ListItemButton
                key={chartType}
                selected={selectedChart === chartType}
                onClick={() => handleChartChange(chartType)}
              >
                {chartType}
              </ListItemButton>
            ))}
          </List>
        </Box>
        <Divider orientation="vertical" flexItem sx={{ mx: 3 }} />
        <Box
          flex={1}
          sx={{
            mt: 2,
            minHeight: '270px',
            display: 'flex',
            flexDirection: 'column',
          }}
        >
          <Box sx={{ display: 'flex', alignItems: 'center', mb: 3, ml: 1 }}>
            <Typography variant="h5" fontWeight="bold">
              {selectedChart}
            </Typography>
            <IconButton onClick={handleOpenPopover}>
              <HelpOutlineIcon />
            </IconButton>
          </Box>
          <Popover
            id={id}
            open={open}
            anchorEl={anchorEl}
            onClose={handleClosePopover}
            anchorOrigin={{ vertical: 'bottom', horizontal: 'left' }}
            transformOrigin={{ vertical: 'top', horizontal: 'left' }}
          >
            <Box sx={{ p: 2, maxWidth: 350 }}>{tooltipContent}</Box>
          </Popover>

          <Box sx={{ flexGrow: 1 }}>{renderOptionsComponent()}</Box>

          <Box sx={{ mt: 2 }}>
            <LoadingButton
              variant="contained"
              onClick={handleRunChart}
              startIcon={<PlayArrowIcon />}
              loading={isLoading}
            >
              Run Chart
            </LoadingButton>
          </Box>
        </Box>
        <Divider sx={{ my: 2 }} />
      </Box>
      <Divider />
      <Box sx={{ width: '100%', mt: 2 }}>{renderChartContainer()}</Box>
    </>
  )
}

export default SM_Charts
