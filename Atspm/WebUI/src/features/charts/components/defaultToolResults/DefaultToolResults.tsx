import TimeSpaceEChart from '@/features/charts/timeSpaceDiagram/shared/components/TimeSpaceEChart'
import LinkPivotAdjustmentTable from '@/features/tools/link-pivot/components/LinkPivotAdjustmentTable'
import { LinkPivotApproachLinkComponent } from '@/features/tools/link-pivot/components/LinkPivotApproachLinkComponent'
import { RawLinkPivotForTsdData } from '@/features/tools/link-pivot/types'
import ChevronLeftIcon from '@mui/icons-material/ChevronLeft'
import ChevronRightIcon from '@mui/icons-material/ChevronRight'
import {
  Box,
  IconButton,
  Paper,
  Tab,
  Tabs,
  Tooltip,
  Typography,
  useTheme,
} from '@mui/material'
import { useEffect, useState } from 'react'
import { transformTimeSpaceData } from '../../api'
import { GpxUploadAccordion } from '../../timeSpaceDiagram/shared/components/GpxUploader/GpxUploadAccordion'
import { IgnoreLocationsAccordion } from '../../timeSpaceDiagram/shared/components/IgnoredLocations/IgnoredLocations'
import type {
  GpxUploadOptions,
  RawTimeSpaceDiagramResponse,
  TimeSpaceBaseData,
} from '../../timeSpaceDiagram/shared/types'

export interface TimeSpaceChartProps {
  timeSpaceData: RawTimeSpaceDiagramResponse
  linkPivotTsdData: RawLinkPivotForTsdData[]
}

const STICKY_TOP = 12 // px

function createEmptyEntry(
  locations: string[],
  primary = false
): GpxUploadOptions {
  return {
    id: '',
    startLocation: locations[0],
    endLocation: locations[locations.length - 1],
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
          phaseNumberSort: current.phaseNumberSort,
          speed: current.speed,
          approachId: current.approachId,
          approachDescription: current.approachDescription,
          calculatedDistanceToNext: 0,
          calculatedDistanceToPrevious: 0,
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
      })
    }

    return recomputed
  }

  const primaryLane = baseData.filter((p) => p.phaseType === 'Primary')
  const opposingLane = baseData.filter((p) => p.phaseType === 'Opposing')

  return [...recomputeLane(primaryLane), ...recomputeLane(opposingLane)]
}

function addDefaultValues(
  timeSpaceData: RawTimeSpaceDiagramResponse
): RawTimeSpaceDiagramResponse {
  const data = timeSpaceData.data
  data.forEach((lane) => {
    lane.calculatedDistanceToNext = lane.distanceToNextLocation
    lane.calculatedDistanceToPrevious = lane.distanceToPreviousLocation
  })
  return { type: timeSpaceData.type, data }
}

export default function TimeSpaceChart({
  timeSpaceData,
  linkPivotTsdData,
}: TimeSpaceChartProps) {
  const theme = useTheme()
  const [activeTab, setActiveTab] = useState(0)

  const [baseTimeSpaceData] = useState<RawTimeSpaceDiagramResponse>(
    addDefaultValues(timeSpaceData)
  )

  const [transformedData, setTransformedData] = useState(() =>
    transformTimeSpaceData(timeSpaceData)
  )

  const [sidebarOpen, setSidebarOpen] = useState(false)

  const SIDEBAR_WIDTH = 320
  const SIDEBAR_MIN_WIDTH = 260

  // one place to tune animation timing/easing for BOTH panes
  const TRANSITION_MS = 200
  const EASING = 'cubic-bezier(0.2, 0, 0, 1)'

  const locations = timeSpaceData.data
    .filter((p) => p.phaseType === 'Primary')
    .map((p) => p.locationIdentifier)

  const [gpxEntries, setGpxEntries] = useState<GpxUploadOptions[]>([
    createEmptyEntry(locations),
  ])
  const [ignoredLocations, setIgnoredLocation] = useState<string[]>([])

  useEffect(() => {
    const recalculatedData =
      ignoredLocations.length > 0
        ? recomputeTimeSpaceData(baseTimeSpaceData.data, ignoredLocations)
        : baseTimeSpaceData.data

    const updatedResponse: RawTimeSpaceDiagramResponse = {
      type: baseTimeSpaceData.type,
      data: recalculatedData,
    }

    setTransformedData(transformTimeSpaceData(updatedResponse))
  }, [ignoredLocations, baseTimeSpaceData])

  const chartHeight = transformedData.data.chart.displayProps.height

  return (
    <Box
      sx={{
        width: '100%',
        position: 'relative',
        position: 'absolute',
        left: 0,
      }}
    >
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
                  <GpxUploadAccordion
                    locations={locations}
                    entries={gpxEntries}
                    setEntries={setGpxEntries}
                  />
                  <IgnoreLocationsAccordion
                    locations={locations}
                    ignoredLocations={ignoredLocations}
                    setIgnoredLocations={setIgnoredLocation}
                  />
                </Box>
              </Box>

              {/* TOGGLE BUTTON — sticks because it's inside the sticky shell */}
              <Box
                sx={{
                  position: 'absolute',
                  left: sidebarOpen ? SIDEBAR_WIDTH : 15,
                  top: 300,
                  transform: 'translateX(-50%)',
                  zIndex: 4,
                  transition: `left ${TRANSITION_MS}ms ${EASING}`,
                  willChange: 'left',
                }}
              >
                <IconButton
                  size="small"
                  onClick={() => {
                    setSidebarOpen((v) => !v)
                    requestAnimationFrame(() => {
                      window.dispatchEvent(new Event('resize'))
                    })
                  }}
                  sx={{
                    bgcolor: 'background.paper',
                    border: '1px solid',
                    borderColor: 'divider',
                    boxShadow: 1,
                    '&:hover': { bgcolor: 'background.paper' },
                  }}
                >
                  {sidebarOpen ? (
                    <Tooltip title="Hide options" placement="right">
                      <ChevronLeftIcon fontSize="small" />
                    </Tooltip>
                  ) : (
                    <Tooltip title="Show options" placement="right">
                      <ChevronRightIcon fontSize="small" />
                    </Tooltip>
                  )}
                </IconButton>
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
                  cycleLength={baseTimeSpaceData.data[0].cycleLength}
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
