import { transformTimeSpaceData } from '@/features/charts/api'
import { ToolType } from '@/features/charts/common/types'
import { useTimeSpaceSrmData } from '@/features/charts/timeSpaceDiagram/api/getTimeSpaceSrmData'
import { GpxUploadAccordion } from '@/features/charts/timeSpaceDiagram/shared/components/GpxUploader/GpxUploadAccordion'
import { SrmUploadAccordion } from '@/features/charts/timeSpaceDiagram/shared/components/SrmUploader/SrmUploadAccordion'
import TimeSpaceEChart from '@/features/charts/timeSpaceDiagram/shared/components/TimeSpaceEChart'
import { gzipAndBase64 } from '@/features/charts/timeSpaceDiagram/shared/fileEncoding'
import type { TransformedTimeSpaceResponse } from '@/features/charts/types'
import LinkPivotAdjustmentTable from '@/features/tools/link-pivot/components/LinkPivotAdjustmentTable'
import { LinkPivotApproachLinkComponent } from '@/features/tools/link-pivot/components/LinkPivotApproachLinkComponent'
import { getLinkPivotPcdTimeWindowFromTimeSpaceOptions } from '@/features/tools/link-pivot/linkPivotPcdTimeWindow'
import { RawLinkPivotForTsdData } from '@/features/tools/link-pivot/types'
import {
  Alert,
  Box,
  Paper,
  Tab,
  Tabs,
  Typography,
  useTheme,
} from '@mui/material'
import { useEffect, useState } from 'react'
import type {
  RawTimeSpaceDiagramResponse,
  RawTimeSpaceHistoricData,
  TimeSpaceDiagramPhaseResult,
  TimeSpaceHistoricOptions,
  TimeSpaceOptions,
} from '../../shared/types'
import {
  addDefaultTimeSpaceValues,
  createEmptyTimeSpaceEntry,
  getPrimaryTimeSpaceLocations,
  mergeSrmOverlaysIntoWrappedData,
  recomputeWrappedTimeSpaceData,
  supportsLinkPivotForTimeSpace,
} from '../utils/timeSpaceResultData'

export interface TimeSpaceResultsContainerProps {
  timeSpaceData: RawTimeSpaceDiagramResponse
  linkPivotTsdData: RawLinkPivotForTsdData[]
  timeSpaceOptions: TimeSpaceOptions
}

export default function TimeSpaceResultsContainer({
  timeSpaceData,
  linkPivotTsdData,
  timeSpaceOptions,
}: TimeSpaceResultsContainerProps) {
  const theme = useTheme()
  const [activeTab, setActiveTab] = useState(0)
  const [transformErrors, setTransformErrors] = useState<string[]>([])
  const [baseTimeSpaceData, setBaseTimeSpaceData] =
    useState<RawTimeSpaceDiagramResponse>(() =>
      addDefaultTimeSpaceValues(timeSpaceData)
    )
  const [transformedData, setTransformedData] =
    useState<TransformedTimeSpaceResponse>(() => ({
      type: timeSpaceData.type,
      data: { chart: {} },
    }))

  const [srmError, setSrmError] = useState<string | null>(null)
  const [hasAppliedSrm, setHasAppliedSrm] = useState(false)
  const { mutateAsync: fetchSrmData, isLoading: isApplyingSrm } =
    useTimeSpaceSrmData()

  const locations = getPrimaryTimeSpaceLocations(baseTimeSpaceData)

  const [gpxEntries, setGpxEntries] = useState([
    createEmptyTimeSpaceEntry(locations),
  ])
  const [ignoredLocations, setIgnoredLocation] = useState<string[]>([])
  const supportsLinkPivot = supportsLinkPivotForTimeSpace(
    baseTimeSpaceData.type
  )

  const toggleIgnoredLocation = (location: string) => {
    setIgnoredLocation((prev) =>
      prev.includes(location)
        ? prev.filter((current) => current !== location)
        : [...prev, location]
    )
  }

  const handleApplySrm = async (file: File) => {
    if (baseTimeSpaceData.type !== ToolType.TimeSpaceHistoric) return

    const historicOptions = timeSpaceOptions as TimeSpaceHistoricOptions

    try {
      setSrmError(null)
      const srmCsvContentBase64 = await gzipAndBase64(file)
      const overlays = await fetchSrmData({
        routeId: historicOptions.routeId,
        start: historicOptions.start,
        end: historicOptions.end,
        srmCsvContentBase64,
      })

      setBaseTimeSpaceData((prev) => ({
        type: prev.type,
        data: mergeSrmOverlaysIntoWrappedData(
          prev.data as TimeSpaceDiagramPhaseResult<RawTimeSpaceHistoricData>[],
          overlays
        ) as RawTimeSpaceDiagramResponse['data'],
      }))
      setHasAppliedSrm(true)
    } catch (error) {
      setSrmError(
        error instanceof Error ? error.message : 'Unable to apply SRM'
      )
    }
  }

  const handleClearSrm = () => {
    setSrmError(null)
    setHasAppliedSrm(false)
    setBaseTimeSpaceData((prev) => ({
      type: prev.type,
      data: mergeSrmOverlaysIntoWrappedData(
        prev.data as TimeSpaceDiagramPhaseResult<RawTimeSpaceHistoricData>[],
        []
      ) as RawTimeSpaceDiagramResponse['data'],
    }))
  }

  useEffect(() => {
    const nextBaseData = addDefaultTimeSpaceValues(timeSpaceData)
    const nextLocations = getPrimaryTimeSpaceLocations(nextBaseData)

    setActiveTab(0)
    setBaseTimeSpaceData(nextBaseData)
    setIgnoredLocation([])
    setGpxEntries([createEmptyTimeSpaceEntry(nextLocations)])
    setSrmError(null)
    setHasAppliedSrm(false)
  }, [timeSpaceData])

  useEffect(() => {
    const recalculatedData =
      ignoredLocations.length > 0
        ? recomputeWrappedTimeSpaceData(
            baseTimeSpaceData.data,
            ignoredLocations
          )
        : baseTimeSpaceData.data

    const updatedResponse: RawTimeSpaceDiagramResponse = {
      type: baseTimeSpaceData.type,
      data: recalculatedData,
    }

    try {
      const result = transformTimeSpaceData(updatedResponse)
      setTransformedData(result)
      if ('errors' in result && result.errors) {
        setTransformErrors(result.errors)
      } else {
        setTransformErrors([])
      }
    } catch (error) {
      console.error('Error transforming time space data:', error)
      setTransformErrors([
        error instanceof Error ? error.message : 'Unknown transformation error',
      ])
      setTransformedData({
        type: baseTimeSpaceData.type,
        data: { chart: {} },
      })
    }
  }, [ignoredLocations, baseTimeSpaceData])

  const chartHeight = transformedData.data.chart.displayProps?.height ?? 500
  const pcdTimeWindow =
    getLinkPivotPcdTimeWindowFromTimeSpaceOptions(timeSpaceOptions)
  const sidebarUploadContent = (
    <>
      {baseTimeSpaceData.type === ToolType.TimeSpaceHistoric && (
        <SrmUploadAccordion
          loading={isApplyingSrm}
          error={srmError}
          hasAppliedSrm={hasAppliedSrm}
          onApply={handleApplySrm}
          onClear={handleClearSrm}
        />
      )}

      <GpxUploadAccordion
        locations={locations}
        entries={gpxEntries}
        setEntries={setGpxEntries}
      />
    </>
  )

  return (
    <Box
      sx={{
        width: '100%',
        position: 'absolute',
        left: 0,
      }}
    >
      {transformErrors.length > 0 && (
        <Box sx={{ mt: 2, mx: 2 }}>
          <Alert severity="warning" sx={{ mb: 2 }}>
            <Typography variant="subtitle2" fontWeight="bold" sx={{ mb: 1 }}>
              Some phases failed to process:
            </Typography>
            <Box component="ul" sx={{ m: 0, pl: 2 }}>
              {transformErrors.map((error, index) => (
                <li key={index}>
                  <Typography variant="body2">{error}</Typography>
                </li>
              ))}
            </Box>
          </Alert>
        </Box>
      )}

      {supportsLinkPivot && (
        <Tabs
          value={activeTab}
          onChange={(_, value) => setActiveTab(value)}
          sx={{ mt: 2 }}
        >
          <Tab label="Time Space Chart" />
          <Tab label="Link Pivot" />
        </Tabs>
      )}

      <Box
        sx={{
          display: !supportsLinkPivot || activeTab === 0 ? 'block' : 'none',
          mt: supportsLinkPivot ? 0 : 2,
        }}
      >
        <Paper sx={{ p: 0, ml: '2px', bgcolor: 'white' }}>
          <Box
            sx={{
              width: '100%',
              position: 'relative',
            }}
          >
            <TimeSpaceEChart
              id="time-space-chart"
              option={transformedData.data.chart}
              theme={theme.palette.mode}
              isVisible={activeTab === 0}
              style={{
                width: '100%',
                height: `${chartHeight}px`,
                position: 'relative',
              }}
              gpxEntries={gpxEntries}
              ignoredLocations={ignoredLocations}
              onToggleIgnoredLocation={toggleIgnoredLocation}
              sidebarUploadContent={sidebarUploadContent}
            />
          </Box>
        </Paper>
      </Box>

      <Box
        sx={{
          display: supportsLinkPivot && activeTab === 1 ? 'block' : 'none',
          m: 2,
        }}
      >
        {linkPivotTsdData.map((pivot) => (
          <Box key={pivot.direction} sx={{ mb: 6 }}>
            <Typography variant="h4" fontWeight="bold" sx={{ my: 3 }}>
              {pivot.direction} Direction
            </Typography>

            <Typography variant="h5" fontWeight="bold" sx={{ mb: 2 }}>
              Adjustments
            </Typography>
            <Paper sx={{ mb: 3 }}>
              <LinkPivotAdjustmentTable
                data={pivot.data.adjustments}
                cycleLength={
                  baseTimeSpaceData.data.find(
                    (entry) => entry.isSuccess && entry.result
                  )?.result?.cycleLength || 0
                }
              />
            </Paper>

            <Typography variant="h5" fontWeight="bold" sx={{ mb: 2 }}>
              Approach Link Comparison
            </Typography>
            <Paper>
              <LinkPivotApproachLinkComponent
                data={pivot.data.approachLinks}
                corridorSummary={pivot.data}
                pcdTimeWindow={pcdTimeWindow}
              />
            </Paper>
          </Box>
        ))}
      </Box>
    </Box>
  )
}
