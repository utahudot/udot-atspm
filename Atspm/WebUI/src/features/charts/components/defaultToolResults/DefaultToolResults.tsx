import TimeSpaceEChart from '@/features/charts/timeSpaceDiagram/shared/components/TimeSpaceEChart'
import { useTimeSpaceSrmData } from '@/features/charts/timeSpaceDiagram/api/getTimeSpaceSrmData'
import { ToolType } from '@/features/charts/common/types'
import { SrmUploadAccordion } from '@/features/charts/timeSpaceDiagram/shared/components/SrmUploader/SrmUploadAccordion'
import LinkPivotAdjustmentTable from '@/features/tools/link-pivot/components/LinkPivotAdjustmentTable'
import { LinkPivotApproachLinkComponent } from '@/features/tools/link-pivot/components/LinkPivotApproachLinkComponent'
import { RawLinkPivotForTsdData } from '@/features/tools/link-pivot/types'
import { gzipAndBase64 } from '@/features/charts/timeSpaceDiagram/shared/fileEncoding'
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
import { transformTimeSpaceData } from '../../api'
import type { TransformedTimeSpaceResponse } from '../../types'
import { GpxUploadAccordion } from '../../timeSpaceDiagram/shared/components/GpxUploader/GpxUploadAccordion'
import type {
  GpxUploadOptions,
  RawTimeSpaceAverageData,
  RawTimeSpaceDiagramResponse,
  RawTimeSpaceHistoricData,
  TimeSpaceHistoricOptions,
  TimeSpaceOptions,
  TimeSpaceBaseData,
  TimeSpaceDiagramPhaseResult,
  TimeSpaceSrmPhaseOverlay,
} from '../../timeSpaceDiagram/shared/types'

export interface TimeSpaceChartProps {
  timeSpaceData: RawTimeSpaceDiagramResponse
  linkPivotTsdData: RawLinkPivotForTsdData[]
  timeSpaceOptions: TimeSpaceOptions
}

const STICKY_TOP = 12 // px

function createEmptyEntry(
  locations: string[],
  primary = false
): GpxUploadOptions {
  return {
    id: '',
    startLocation: locations[0] ?? '',
    endLocation: locations[locations.length - 1] ?? '',
    error: null,
    primary,
  }
}

function recomputeTimeSpaceData<T extends TimeSpaceBaseData>(
  baseData: T[],
  ignoredLocations: string[]
): T[] {
  const isIgnored = (id: string) => ignoredLocations.includes(id)

  const recomputeLane = (lane: T[]): T[] => {
    if (!lane.some((l) => isIgnored(l.locationIdentifier))) return lane

    const recomputed: T[] = []

    for (let i = 0; i < lane.length; i++) {
      const current = lane[i]

      if (isIgnored(current.locationIdentifier)) {
        recomputed.push({
          start: current.start,
          end: current.end,
          locationIdentifier: current.locationIdentifier,
          locationDescription: current.locationDescription,
          phaseType: current.phaseType,
          distanceToNextLocation: current.distanceToNextLocation,
          distanceToPreviousLocation: current.distanceToPreviousLocation,
          phaseNumber: current.phaseNumber,
          Description: current.description,
          speed: current.speed,
          approachId: current.approachId,
          approachDescription: current.approachDescription,
          calculatedDistanceToNext: 0,
          calculatedDistanceToPrevious: 0,
          isIgnoredLocation: true,
        } as T)
        continue
      }

      let distanceToPrevious = 0
      for (let j = i - 1; j >= 0; j--) {
        distanceToPrevious += lane[j].distanceToNextLocation
        if (!isIgnored(lane[j].locationIdentifier)) break
      }

      let distanceToNext = 0
      for (let j = i; j < lane.length - 1; j++) {
        distanceToNext += lane[j].distanceToNextLocation
        if (!isIgnored(lane[j + 1].locationIdentifier)) break
      }

      recomputed.push({
        ...current,
        calculatedDistanceToPrevious: distanceToPrevious,
        calculatedDistanceToNext: distanceToNext,
        isIgnoredLocation: false,
      })
    }

    return recomputed
  }

  const primaryLane = baseData.filter((p) => p.phaseType === 'Primary')
  const opposingLane = baseData.filter((p) => p.phaseType === 'Opposing')

  return [...recomputeLane(primaryLane), ...recomputeLane(opposingLane)]
}

// Helper function to unwrap, recompute, and re-wrap data
function recomputeWrappedTimeSpaceData(
  wrappedData: RawTimeSpaceDiagramResponse['data'],
  ignoredLocations: string[]
): RawTimeSpaceDiagramResponse['data'] {
  type WrappedResult =
    | TimeSpaceDiagramPhaseResult<RawTimeSpaceHistoricData>
    | TimeSpaceDiagramPhaseResult<RawTimeSpaceAverageData>
  type SuccessfulWrappedResult = WrappedResult & {
    isSuccess: true
    result: RawTimeSpaceHistoricData | RawTimeSpaceAverageData
  }

  // Extract successful results
  const unwrappedData = wrappedData
    .filter(
      (item): item is SuccessfulWrappedResult => item.isSuccess && !!item.result
    )
    .map((item) => item.result)

  // Recompute with ignored locations
  const recomputed = recomputeTimeSpaceData(unwrappedData, ignoredLocations)

  // Re-wrap the recomputed data while preserving original error entries.
  let recomputedIndex = 0
  return wrappedData.map((item) => {
    if (!item.isSuccess || !item.result) {
      return item
    }

    const nextResult = recomputed[recomputedIndex++] ?? item.result
    return {
      error: null,
      result: nextResult,
      isSuccess: true,
    }
  }) as RawTimeSpaceDiagramResponse['data']
}

function mergeSrmOverlaysIntoWrappedData(
  wrappedData: TimeSpaceDiagramPhaseResult<RawTimeSpaceHistoricData>[],
  overlays: TimeSpaceSrmPhaseOverlay[]
): TimeSpaceDiagramPhaseResult<RawTimeSpaceHistoricData>[] {
  const overlayMap = new Map(
    overlays.map((overlay) => [
      `${overlay.locationIdentifier}|${overlay.phaseType}|${overlay.order}`,
      overlay.srmEntityTracks ?? [],
    ])
  )

  return wrappedData.map((item) => {
    if (!item.isSuccess || !item.result) {
      return item
    }

    const key = `${item.result.locationIdentifier}|${item.result.phaseType}|${item.result.order}`

    return {
      ...item,
      result: {
        ...item.result,
        srmEntityTracks: overlayMap.get(key) ?? [],
      },
    }
  })
}

function addDefaultValues(
  timeSpaceData: RawTimeSpaceDiagramResponse
): RawTimeSpaceDiagramResponse {
  const wrappedData = timeSpaceData.data

  // Process each wrapped result
  const processedData = wrappedData.map((wrappedItem) => {
    // If this is an error result, return it unchanged
    if (!wrappedItem.isSuccess || !wrappedItem.result) {
      return wrappedItem
    }

    // For successful results, add calculated distance values
    const lane = wrappedItem.result
    return {
      ...wrappedItem,
      result: {
        ...lane,
        calculatedDistanceToNext: lane.distanceToNextLocation,
        calculatedDistanceToPrevious: lane.distanceToPreviousLocation,
        isIgnoredLocation: false,
      },
    }
  })

  return {
    type: timeSpaceData.type,
    data: processedData as RawTimeSpaceDiagramResponse['data'],
  }
}

export default function TimeSpaceChart({
  timeSpaceData,
  linkPivotTsdData,
  timeSpaceOptions,
}: TimeSpaceChartProps) {
  const theme = useTheme()
  const [activeTab, setActiveTab] = useState(0)
  const [transformErrors, setTransformErrors] = useState<string[]>([])
  const [baseTimeSpaceData, setBaseTimeSpaceData] =
    useState<RawTimeSpaceDiagramResponse>(() => addDefaultValues(timeSpaceData))
  const [transformedData, setTransformedData] =
    useState<TransformedTimeSpaceResponse>(() => ({
    type: timeSpaceData.type,
    data: { chart: {} },
  }))

  const [sidebarOpen, setSidebarOpen] = useState(false)
  const [srmError, setSrmError] = useState<string | null>(null)
  const [hasAppliedSrm, setHasAppliedSrm] = useState(false)
  const { mutateAsync: fetchSrmData, isLoading: isApplyingSrm } =
    useTimeSpaceSrmData()

  const SIDEBAR_WIDTH = 320
  const SIDEBAR_MIN_WIDTH = 260

  // one place to tune animation timing/easing for BOTH panes
  const TRANSITION_MS = 200
  const EASING = 'cubic-bezier(0.2, 0, 0, 1)'

  const locations = baseTimeSpaceData.data
    .filter(
      (p) => p.isSuccess && !!p.result && p.result.phaseType === 'Primary'
    )
    .map((p) => p.result.locationIdentifier)

  const [gpxEntries, setGpxEntries] = useState<GpxUploadOptions[]>([
    createEmptyEntry(locations),
  ])
  const [ignoredLocations, setIgnoredLocation] = useState<string[]>([])

  const toggleOptionsSidebar = () => {
    setSidebarOpen((v) => !v)
    requestAnimationFrame(() => {
      window.dispatchEvent(new Event('resize'))
    })
  }

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
      setSrmError(error instanceof Error ? error.message : 'Unable to apply SRM')
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
    const nextBaseData = addDefaultValues(timeSpaceData)
    const nextLocations = nextBaseData.data
      .filter(
        (p) => p.isSuccess && !!p.result && p.result.phaseType === 'Primary'
      )
      .map((p) => p.result.locationIdentifier)

    setBaseTimeSpaceData(nextBaseData)
    setIgnoredLocation([])
    setGpxEntries([createEmptyEntry(nextLocations)])
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
      // Check if transformation returned errors
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

  return (
    <Box
      sx={{
        width: '100%',
        position: 'absolute',
        left: 0,
      }}
    >
      {/* Display errors if any */}
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

      {/* 🔹 Tabs Outside Paper */}
      <Tabs
        value={activeTab}
        onChange={(_, v) => setActiveTab(v)}
        sx={{ mt: 2 }}
      >
        <Tab label="Time Space Chart" />
        <Tab label="Link Pivot" />
      </Tabs>
      {activeTab === 0 && (
        <Paper sx={{ p: 0, mt: 2, ml: '2px', bgcolor: 'white' }}>
          <Box
            sx={{
              display: 'flex',
              width: '100%',
              position: 'relative',
            }}
          >
            {/* LEFT — STICKY SHELL (sidebar + button stick together) */}
            <Box
              sx={{
                position: 'sticky',
                top: `${STICKY_TOP}px`,
                alignSelf: 'flex-start',
                maxHeight: `100vh`,
                height: '100%',
                zIndex: 3,
                overflow: 'visible',
                // the shell itself animates width so the chart doesn't jump
                width: sidebarOpen ? SIDEBAR_WIDTH : 0,
                minWidth: sidebarOpen ? SIDEBAR_MIN_WIDTH : 0,
                willChange: 'width, min-width',
                transition: `width ${TRANSITION_MS}ms ${EASING}, min-width ${TRANSITION_MS}ms ${EASING}`,
              }}
            >
              {/* SIDEBAR PANEL */}
              <Box
                sx={{
                  height: '100%',
                  overflow: 'hidden',
                }}
              >
                {/* scroll the content, not the page */}
                <Box
                  sx={{
                    height: '100%',
                    overflowY: 'auto',
                    p: 2,
                    opacity: sidebarOpen ? 1 : 0,
                    transform: sidebarOpen
                      ? 'translateX(0)'
                      : 'translateX(-8px)',
                    transition: `opacity ${TRANSITION_MS}ms ${EASING}, transform ${TRANSITION_MS}ms ${EASING}`,
                    willChange: 'opacity, transform',
                    pointerEvents: sidebarOpen ? 'auto' : 'none',
                  }}
                >
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
                </Box>
              </Box>

            </Box>

            {/* RIGHT — CHART */}
            <Box
              sx={{
                flex: 1,
                minWidth: 0,
                p: 2,
                willChange: 'transform',
                transition: `transform ${TRANSITION_MS}ms ${EASING}`,
                transform: sidebarOpen ? 'translateX(0)' : 'translateX(-4px)',
                borderLeft: sidebarOpen ? '1px solid' : 'none',
                borderColor: 'divider',
              }}
            >
              <TimeSpaceEChart
                id="time-space-chart"
                option={transformedData.data.chart}
                theme={theme.palette.mode}
                style={{
                  width: '100%',
                  height: `${chartHeight}px`,
                  position: 'relative',
                }}
                gpxEntries={gpxEntries}
                ignoredLocations={ignoredLocations}
                onToggleIgnoredLocation={toggleIgnoredLocation}
                leftSidebarOpen={sidebarOpen}
                onToggleLeftSidebar={toggleOptionsSidebar}
              />
            </Box>
          </Box>
        </Paper>
      )}
      {activeTab === 1 && (
        <Box>
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
                    baseTimeSpaceData.data.find((d) => d.isSuccess && d.result)
                      ?.result?.cycleLength || 0
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
                  lpHandler={null}
                />
              </Paper>
            </Box>
          ))}
        </Box>
      )}
    </Box>
  )
}
