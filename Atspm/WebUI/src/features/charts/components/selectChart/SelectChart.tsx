import { Location } from '@/api/config'
import { useChartDefaults } from '@/features/charts/api/getChartDefaults'
import { useGetMeasureTypes } from '@/features/charts/api/getMeasureTypes'
import { ApproachDelayChartOptions } from '@/features/charts/approachDelay/components/ApproachDelayChartOptions'
import { ApproachSpeedChartOptions } from '@/features/charts/approachSpeed/components/ApproachSpeedChartOptions'
import { ApproachVolumeChartOptions } from '@/features/charts/approachVolume/components/ApproachVolumeChartOptions'
import { ArrivalsOnRedChartOptions } from '@/features/charts/arrivalsOnRed/components/ArrivalsOnRedChartOptions'
import { ChartOptions, ChartType } from '@/features/charts/common/types'
import { GreenTimeUtilizationChartOptions } from '@/features/charts/greenTimeUtilization/components/GreenTimeUtilizationChartOptions'
import { LeftTurnGapAnalysisChartOptions } from '@/features/charts/leftTurnGapAnalysis/components/LeftTurnGapAnalysisChartOptions'
import { PedestrianDelayChartOptions } from '@/features/charts/pedestrianDelay/components/PedestrianDelayChartOptions'
import { PreemptionDetailsChartOptions } from '@/features/charts/preemptionDetails/components/PreemptionDetailsChartOptions'
import { PrioritySummaryChartOptions } from '@/features/charts/prioritySummary/components/PrioritySummaryChartOptions'
import { PurdueCoordinationDiagramChartOptions } from '@/features/charts/purdueCoordinationDiagram/components/PurdueCoordinationDiagramChartOptions'
import { PurduePhaseTerminationChartOptions } from '@/features/charts/purduePhaseTermination/components/PurduePhaseTerminationChartOptions'
import { PurdueSplitFailureChartOptions } from '@/features/charts/purdueSplitFailure/components/PurdueSplitFailureChartOptions'
import { SplitMonitorChartOptions } from '@/features/charts/splitMonitor/components/SplitMonitorChartOptions'
import { TimingAndActuationChartOptions } from '@/features/charts/timingAndActuation/components/TimingAndActuationChartOptions'
import { TurningMovementCountsChartOptions } from '@/features/charts/turningMovementCounts/components/TurningMovementCountsChartOptions'
import { Default } from '@/features/charts/types'
import { getDisplayNameFromChartType } from '@/features/charts/utils'
import { WaitTimeChartOptions } from '@/features/charts/waitTime/components/WaitTimeOptions'
import { YellowAndRedActuationsChartOptions } from '@/features/charts/yellowAndRedActuations/components/YellowAndRedActuationsChartOptions'
import { TimeOfDayMeasureOptions } from '@/features/timeOfDay/components/TimeOfDayMeasureOptions'
import {
  Box,
  Divider,
  FormControl,
  InputLabel,
  MenuItem,
  Typography,
} from '@mui/material'
import Select, { SelectChangeEvent } from '@mui/material/Select'
import type { ComponentType, Dispatch, SetStateAction } from 'react'
import { useEffect, useMemo } from 'react'
import { RampMeteringChartOptions } from '../../rampMetering/components/RampMeteringChartOptions'

type ChartOptionsComponentProps = {
  chartDefaults: unknown
  handleChartOptionsUpdate: (update: Default) => void
  isMeasureDefaultView?: boolean
}

type ChartOptionsComponent = ComponentType<ChartOptionsComponentProps>

export const chartComponents: Record<string, ChartOptionsComponent> = {
  ApproachDelay: ApproachDelayChartOptions as unknown as ChartOptionsComponent,
  ApproachSpeed: ApproachSpeedChartOptions as unknown as ChartOptionsComponent,
  ApproachVolume:
    ApproachVolumeChartOptions as unknown as ChartOptionsComponent,
  ArrivalsOnRed: ArrivalsOnRedChartOptions as unknown as ChartOptionsComponent,
  GreenTimeUtilization:
    GreenTimeUtilizationChartOptions as unknown as ChartOptionsComponent,
  LeftTurnGapAnalysis:
    LeftTurnGapAnalysisChartOptions as unknown as ChartOptionsComponent,
  PedestrianDelay:
    PedestrianDelayChartOptions as unknown as ChartOptionsComponent,
  PreemptionDetails:
    PreemptionDetailsChartOptions as unknown as ChartOptionsComponent,
  PrioritySummary:
    PrioritySummaryChartOptions as unknown as ChartOptionsComponent,
  PurdueCoordinationDiagram:
    PurdueCoordinationDiagramChartOptions as unknown as ChartOptionsComponent,
  PurduePhaseTermination:
    PurduePhaseTerminationChartOptions as unknown as ChartOptionsComponent,
  PurdueSplitFailure:
    PurdueSplitFailureChartOptions as unknown as ChartOptionsComponent,
  SplitMonitor: SplitMonitorChartOptions as unknown as ChartOptionsComponent,
  TimingAndActuation:
    TimingAndActuationChartOptions as unknown as ChartOptionsComponent,
  TurningMovementCounts:
    TurningMovementCountsChartOptions as unknown as ChartOptionsComponent,
  WaitTime: WaitTimeChartOptions as unknown as ChartOptionsComponent,
  YellowAndRedActuations:
    YellowAndRedActuationsChartOptions as unknown as ChartOptionsComponent,
  RampMetering: RampMeteringChartOptions as unknown as ChartOptionsComponent,
  TimeOfDay: TimeOfDayMeasureOptions as unknown as ChartOptionsComponent,
} as const

const abbreviationToChartType: Record<string, ChartType> = {
  AD: ChartType.ApproachDelay,
  AV: ChartType.ApproachVolume,
  AoR: ChartType.ArrivalsOnRed,
  Speed: ChartType.ApproachSpeed,
  GTU: ChartType.GreenTimeUtilization,
  LTGA: ChartType.LeftTurnGapAnalysis,
  PedD: ChartType.PedestrianDelay,
  PCD: ChartType.PurdueCoordinationDiagram,
  TSPS: ChartType.PrioritySummary,
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
  setChartOptions: Dispatch<SetStateAction<Partial<ChartOptions>>>
  chartOptions?: Partial<ChartOptions>
  location: Location | null
}

const SelectChart = ({
  chartType,
  setChartType,
  setChartOptions,
  chartOptions,
  location,
}: SelectChartProps) => {
  const { data: chartDefaultsData, isLoading } = useChartDefaults()
  const { data: measureTypesData } = useGetMeasureTypes()

  const chartDefaultsRaw =
    chartDefaultsData &&
    chartDefaultsData.value.find((chart) => chart.chartType === chartType)
      ?.measureOptions

  const chartDefaultsForUi: Default[] | undefined = useMemo(() => {
    if (!chartDefaultsRaw) return undefined
    if (!chartOptions || Object.keys(chartOptions).length === 0)
      return chartDefaultsRaw

    const asRecord = chartDefaultsRaw as unknown as Record<
      string,
      { value: unknown; [k: string]: unknown }
    >

    const merged: Record<string, { value: unknown; [k: string]: unknown }> = {
      ...asRecord,
    }

    Object.entries(chartOptions).forEach(([key, overrideValue]) => {
      if (overrideValue === undefined || overrideValue === null) return

      if (merged[key]) {
        merged[key] = { ...merged[key], value: overrideValue }
      } else {
        merged[key] = { value: overrideValue }
      }
    })

    return merged as unknown as Default[]
  }, [chartDefaultsRaw, chartOptions])

  const simplifyChartDefaults = (chartDefaults: Default[]) => {
    const simplifiedDefaults = chartDefaults
      ? Object.entries(chartDefaults).reduce<Record<string, unknown>>(
          (acc, [key, { value }]) => {
            acc[key] = value
            return acc
          },
          {}
        )
      : {}

    return simplifiedDefaults as ChartOptions
  }

  const availableCharts = useMemo(() => {
    if (!measureTypesData || !location) return {}

    const unsortedCharts = measureTypesData.value.reduce(
      (acc, measureType) => {
        if (
          (location as unknown as { charts?: number[] })?.charts?.includes(
            measureType.id
          ) &&
          measureType.showOnWebsite
        ) {
          const chartType = abbreviationToChartType[measureType.abbreviation]
          if (chartType) {
            acc[chartType] = chartComponents[chartType]
          }
        }
        return acc
      },
      {} as Partial<Record<ChartType, ChartOptionsComponent>>
    )

    const sortedKeys = Object.keys(unsortedCharts).sort((a, b) =>
      a.localeCompare(b)
    )

    return sortedKeys.reduce(
      (acc, key) => {
        const chartType = key as ChartType
        acc[chartType] = unsortedCharts[chartType]
        return acc
      },
      {} as Partial<Record<ChartType, ChartOptionsComponent>>
    )
  }, [measureTypesData, location])

  const isChartTypeAvailable = Boolean(
    location && chartType && chartType in availableCharts
  )

  useEffect(() => {
    if (!isLoading && chartDefaultsRaw) {
      const simplifiedDefaults = simplifyChartDefaults(chartDefaultsRaw)
      setChartOptions(simplifiedDefaults)
    }
  }, [chartType, chartDefaultsRaw, isLoading, setChartOptions])

  const handleChartTypeChange = (event: SelectChangeEvent<string>) => {
    setChartType(event.target.value as ChartType)
  }

  useEffect(() => {
    if (location && !isChartTypeAvailable) {
      setChartType(null)
    } else if (
      location &&
      !chartType &&
      availableCharts[ChartType.PurduePhaseTermination]
    ) {
      setChartType(ChartType.PurduePhaseTermination)
    }
  }, [location, chartType, availableCharts, setChartType, isChartTypeAvailable])

  const handleChartOptionsUpdate = (update: Default) => {
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

    if (!ChartComponent || !chartDefaultsForUi) return null
    if (isLoading) return <div>Loading...</div>

    return (
      <ChartComponent
        chartDefaults={chartDefaultsForUi}
        handleChartOptionsUpdate={handleChartOptionsUpdate}
      />
    )
  }

  return (
    <>
      <FormControl fullWidth sx={{ mb: 1 }}>
        <InputLabel
          htmlFor="measure-type-select"
          id="measure-type-select-label"
        >
          {location ? 'Measure' : 'Please select a location'}
        </InputLabel>
        <Select
          labelId="measure-type-select-label"
          id="measure-type-select"
          label="Measure"
          inputProps={{ id: 'measure-type-select' }}
          value={chartType && chartType in availableCharts ? chartType : ''}
          onChange={handleChartTypeChange}
          disabled={!location}
        >
          {Object.keys(availableCharts).map((type) => (
            <MenuItem key={type} value={type}>
              {getDisplayNameFromChartType(type as ChartType)}
            </MenuItem>
          ))}
        </Select>
      </FormControl>
      <Box
        marginTop={1}
        sx={{
          display: 'flex',
          flexDirection: 'column',
          flex: '1 1 0%',
          minHeight: 0,
        }}
      >
        <Divider sx={{ mb: 2 }}>
          <Typography variant="caption">Options</Typography>
        </Divider>
        {renderChartOptionsComponent()}
      </Box>
    </>
  )
}

export default SelectChart
