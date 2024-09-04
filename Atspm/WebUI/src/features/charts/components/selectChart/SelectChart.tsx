import { ChartOptions, ChartType } from '@/features/charts/common/types'

import { useChartDefaults } from '@/features/charts/performanceMeasures/api/getChartDefaults'
import { useGetMeasureTypes } from '@/features/charts/performanceMeasures/api/getMeasureTypes'
import { PreemptionDetailsChartOptions } from '@/features/charts/performanceMeasures/preemptionDetails/components/PreemptionDetailsChartOptions'

import { Default } from '@/features/charts/types'
import { getDisplayNameFromChartType } from '@/features/charts/utils'

import { ApproachDelayChartOptions } from '@/features/charts/performanceMeasures/approachDelay/components/ApproachDelayChartOptions'
import { ApproachSpeedChartOptions } from '@/features/charts/performanceMeasures/approachSpeed/components/ApproachSpeedChartOptions'
import { ApproachVolumeChartOptions } from '@/features/charts/performanceMeasures/approachVolume/components/ApproachVolumeChartOptions'
import { ArrivalsOnRedChartOptions } from '@/features/charts/performanceMeasures/arrivalsOnRed/components/ArrivalsOnRedChartOptions'
import { GreenTimeUtilizationChartOptions } from '@/features/charts/performanceMeasures/greenTimeUtilization/components/GreenTimeUtilizationChartOptions'
import { LeftTurnGapAnalysisChartOptions } from '@/features/charts/performanceMeasures/leftTurnGapAnalysis/components/LeftTurnGapAnalysisChartOptions'
import { PedestrianDelayChartOptions } from '@/features/charts/performanceMeasures/pedestrianDelay/components/PedestrianDelayChartOptions'
import { PurdueCoordinationDiagramChartOptions } from '@/features/charts/performanceMeasures/purdueCoordinationDiagram/components/PurdueCoordinationDiagramChartOptions'
import { PurduePhaseTerminationChartOptions } from '@/features/charts/performanceMeasures/purduePhaseTermination/components/PurduePhaseTerminationChartOptions'
import { PurdueSplitFailureChartOptions } from '@/features/charts/performanceMeasures/purdueSplitFailure/components/PurdueSplitFailureChartOptions'
import { SplitMonitorChartOptions } from '@/features/charts/performanceMeasures/splitMonitor/components/SplitMonitorChartOptions'
import { TimingAndActuationChartOptions } from '@/features/charts/performanceMeasures/timingAndActuation/components/TimingAndActuationChartOptions'
import { TurningMovementCountsChartOptions } from '@/features/charts/performanceMeasures/turningMovementCounts/components/TurningMovementCountsChartOptions'
import { WaitTimeChartOptions } from '@/features/charts/performanceMeasures/waitTime/components/WaitTimeOptions'
import { YellowAndRedActuationsChartOptions } from '@/features/charts/performanceMeasures/yellowAndRedActuations/components/YellowAndRedActuationsChartOptions'
import { RampMeteringChartOptions } from '@/features/charts/rampMetering/components/RampMeteringChartOptions'
import { Location } from '@/features/locations/types/Location'
import {
  Box,
  Divider,
  FormControl,
  InputLabel,
  MenuItem,
  Typography,
} from '@mui/material'
import Select, { SelectChangeEvent } from '@mui/material/Select'
import { useEffect, useMemo } from 'react'

export const chartComponents = {
  ApproachDelay: ApproachDelayChartOptions,
  ApproachSpeed: ApproachSpeedChartOptions,
  ApproachVolume: ApproachVolumeChartOptions,
  ArrivalsOnRed: ArrivalsOnRedChartOptions,
  GreenTimeUtilization: GreenTimeUtilizationChartOptions,
  LeftTurnGapAnalysis: LeftTurnGapAnalysisChartOptions,
  PedestrianDelay: PedestrianDelayChartOptions,
  PreemptionDetails: PreemptionDetailsChartOptions,
  PurdueCoordinationDiagram: PurdueCoordinationDiagramChartOptions,
  PurduePhaseTermination: PurduePhaseTerminationChartOptions,
  PurdueSplitFailure: PurdueSplitFailureChartOptions,
  SplitMonitor: SplitMonitorChartOptions,
  TimingAndActuation: TimingAndActuationChartOptions,
  TurningMovementCounts: TurningMovementCountsChartOptions,
  WaitTime: WaitTimeChartOptions,
  YellowAndRedActuations: YellowAndRedActuationsChartOptions,
  RampMetering: RampMeteringChartOptions,
} as const

const abbreviationToChartType = {
  AD: ChartType.ApproachDelay,
  AV: ChartType.ApproachVolume,
  AoR: ChartType.ArrivalsOnRed,
  Speed: ChartType.ApproachSpeed,
  GTU: ChartType.GreenTimeUtilization,
  LTGA: ChartType.LeftTurnGapAnalysis,
  PedD: ChartType.PedestrianDelay,
  PCD: ChartType.PurdueCoordinationDiagram,
  PD: ChartType.PreemptionDetails,
  PPT: ChartType.PurduePhaseTermination,
  SF: ChartType.PurdueSplitFailure,
  SM: ChartType.SplitMonitor,
  TAA: ChartType.TimingAndActuation,
  TMC: ChartType.TurningMovementCounts,
  WT: ChartType.WaitTime,
  YRA: ChartType.YellowAndRedActuations,
  RM: ChartType.RampMetering,
}

interface SelectChartProps {
  chartType: ChartType | null
  setChartType: (chart: ChartType | null) => void
  setChartOptions: (options: Partial<ChartOptions>) => void
  location: Location | null
}

const SelectChart = ({
  chartType,
  setChartType,
  setChartOptions,
  location,
}: SelectChartProps) => {
  const { data: chartDefaultsData, isLoading } = useChartDefaults()
  const { data: measureTypesData } = useGetMeasureTypes()

  const chartDefaults =
    chartDefaultsData &&
    chartDefaultsData.value.find((chart) => chart.chartType === chartType)
      ?.measureOptions

  const simplifyChartDefaults = (chartDefaults: Default[]) => {
    return chartDefaults
      ? Object.entries(chartDefaults).reduce((acc, [key, { value }]) => {
          acc[key] = value
          return acc
        }, {} as ChartOptions)
      : {}
  }

  const availableCharts = useMemo(() => {
    if (!measureTypesData || !location) return {}

    const unsortedCharts = measureTypesData.value.reduce(
      (acc, measureType) => {
        if (
          location.charts.includes(measureType.id) &&
          measureType.showOnWebsite
        ) {
          const chartType = abbreviationToChartType[measureType.abbreviation]
          if (chartType) {
            acc[chartType] = chartComponents[chartType]
          }
        }
        return acc
      },
      {} as Record<ChartType, React.ComponentType<any>>
    )

    const sortedKeys = Object.keys(unsortedCharts).sort((a, b) =>
      a.localeCompare(b)
    )
    const sortedCharts = sortedKeys.reduce(
      (acc, key) => {
        const chartType = key as ChartType
        acc[chartType] = unsortedCharts[chartType]
        return acc
      },
      {} as Record<ChartType, React.ComponentType<any>>
    )

    return sortedCharts
  }, [measureTypesData, location])

  const isChartTypeAvailable = Boolean(
    location && chartType && chartType in availableCharts
  )

  useEffect(() => {
    if (!isLoading && chartDefaults) {
      const simplifiedDefaults = simplifyChartDefaults(chartDefaults)
      setChartOptions(simplifiedDefaults)
    }
  }, [chartType, chartDefaults, isLoading, setChartOptions])

  const handleChartTypeChange = (event: SelectChangeEvent<string>) => {
    setChartType(event.target.value as ChartType)
  }

  useEffect(() => {
    if (location && !isChartTypeAvailable) {
      setChartType(null)
    } else if (
      location &&
      !chartType &&
      availableCharts[ChartType.SplitMonitor]
    ) {
      setChartType(ChartType.SplitMonitor)
    }
  }, [location, chartType, availableCharts, setChartType, isChartTypeAvailable])

  const handleChartOptionsUpdate = (update: {
    option: string
    value: string | number
  }) => {
    setChartOptions((prevOptions) => {
      return {
        ...prevOptions,
        [update.option]: update.value,
      }
    })
  }

  const renderChartOptionsComponent = () => {
    const ChartComponent =
      chartComponents[chartType as keyof typeof chartComponents]

    if (!ChartComponent || !chartDefaults) return null

    if (isLoading) return <div>Loading...</div>

    return (
      <ChartComponent
        chartDefaults={chartDefaults}
        handleChartOptionsUpdate={handleChartOptionsUpdate}
      />
    )
  }

  return (
    <>
      <FormControl fullWidth sx={{ mb: 1 }}>
        <InputLabel htmlFor="measure-type-label">
          {location ? 'Measure' : 'Please select a location'}
        </InputLabel>
        <Select
          inputProps={{ id: 'measure-type-label' }}
          label="Measure"
          value={chartType && chartType in availableCharts ? chartType : ''}
          onChange={handleChartTypeChange}
          placeholder="Please select a location"
          displayEmpty
          disabled={!location}
        >
          {Object.keys(availableCharts).map((type) => (
            <MenuItem key={type} value={type}>
              {getDisplayNameFromChartType(type as ChartType)}
            </MenuItem>
          ))}
        </Select>
      </FormControl>
      <Box marginTop={1} minHeight={'32px'}>
        <Divider sx={{ mb: 2 }}>
          <Typography variant="caption">Options</Typography>
        </Divider>
        {renderChartOptionsComponent()}
      </Box>
    </>
  )
}

export default SelectChart
