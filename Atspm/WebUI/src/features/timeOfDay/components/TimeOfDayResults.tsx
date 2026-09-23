import type { TimeOfDayResult } from '@/api/reports'
import { Alert, Box, Paper, Stack, Tab, Tabs } from '@mui/material'
import { useCallback, useMemo, useState } from 'react'
import {
  buildTimeOfDayAnalysisModel,
  buildTimeOfDayLocationNumberMap,
  hasPlanProfileData,
  hasSplitPressureData,
} from '../transformers'
import TimeOfDayChartWorkspace from './TimeOfDayChartWorkspace'
import TimeOfDaySchedules from './TimeOfDaySchedules'
import type { TimeOfDayDetailTab } from './chart/TimeOfDayChartHeader'
import TimeOfDayDetailsPanel from './results/TimeOfDayDetailsPanel'
import TimeOfDayLocationData from './results/TimeOfDayLocationData'
import TimeOfDaySummary from './results/TimeOfDaySummary'

interface TimeOfDayResultsProps {
  result: TimeOfDayResult
}

type TimeOfDayResultsTab = 'chart' | 'location-data' | 'schedules'

export default function TimeOfDayResults({ result }: TimeOfDayResultsProps) {
  const [activeTab, setActiveTab] = useState<TimeOfDayResultsTab>('chart')
  const carriedForwardPlanWarnings = (result.warnings ?? []).filter(
    (warning) => warning.code === 'PlanScheduleCarriedForward' && warning.message
  )
  const analysisModel = useMemo(
    () => buildTimeOfDayAnalysisModel(result),
    [result]
  )
  const locationNumberMap = useMemo(
    () => buildTimeOfDayLocationNumberMap(result),
    [result]
  )
  const hasChartData =
    hasPlanProfileData(result) ||
    hasSplitPressureData(result) ||
    analysisModel.layers.some(
      (layer) => layer.group === 'Schedules' && layer.available
    )
  const renderDetails = useCallback(
    ({
      detailTab,
      selectedSeries,
      selectedDetailKey,
      onSelectDetail,
      onSetSeriesVisibility,
    }: {
      detailTab: TimeOfDayDetailTab
      selectedSeries: Record<string, boolean>
      selectedDetailKey?: string
      onSelectDetail: (detailKey: string) => void
      onSetSeriesVisibility: (seriesNames: string[], visible: boolean) => void
    }) => (
      <TimeOfDayDetailsPanel
        result={result}
        detailTab={detailTab}
        selectedSeries={selectedSeries}
        locationNumberMap={locationNumberMap}
        selectedDetailKey={selectedDetailKey}
        onSelectDetail={onSelectDetail}
        onSetSeriesVisibility={onSetSeriesVisibility}
      />
    ),
    [result, locationNumberMap]
  )

  return (
    <Stack
      spacing={0}
      sx={{
        position: 'relative',
        width: {
          xs: 'calc(100% + 16px)',
          sm: 'calc(100% + 48px)',
        },
        ml: { xs: -1, sm: -3 },
      }}
    >
      {carriedForwardPlanWarnings.length > 0 && (
        <Alert severity="warning" sx={{ mt: 2, mx: 2 }}>
          Existing plan coverage needs review.
          <Box component="ul" sx={{ m: 0, pl: 2 }}>
            {carriedForwardPlanWarnings.map((warning, index) => (
              <li key={`${warning.locationIdentifier}-${index}`}>
                {warning.message}
              </li>
            ))}
          </Box>
        </Alert>
      )}
      <Tabs
        value={activeTab}
        onChange={(_, value: TimeOfDayResultsTab) => setActiveTab(value)}
        aria-label="Time-of-day results"
        sx={{ mt: 2 }}
      >
        <Tab value="chart" label="Time-of-Day Chart" />
        <Tab value="schedules" label="Schedules" />
        <Tab value="location-data" label="Location Data" />
      </Tabs>

      {activeTab === 'chart' && (
        <Paper
          sx={{ p: 0, ml: '2px', bgcolor: 'common.white' }}
          role="tabpanel"
          aria-label="Time-of-day chart"
        >
          {hasChartData ? (
            <TimeOfDayChartWorkspace
              model={analysisModel}
              renderDetails={renderDetails}
              summary={
                <TimeOfDaySummary
                  result={result}
                  peakItems={analysisModel.header.summaryItems}
                />
              }
            />
          ) : (
            <Alert severity="warning">No Data Available</Alert>
          )}
        </Paper>
      )}
      {activeTab === 'schedules' && (
        <Box role="tabpanel" aria-label="Schedules">
          <TimeOfDaySchedules result={result} />
        </Box>
      )}
      {activeTab === 'location-data' && (
        <Box role="tabpanel" aria-label="Location data">
          <TimeOfDayLocationData result={result} />
        </Box>
      )}
    </Stack>
  )
}
